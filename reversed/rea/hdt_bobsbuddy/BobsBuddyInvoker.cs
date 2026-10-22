using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BobsBuddy;
using BobsBuddy.Enchantments;
using BobsBuddy.Minions.Duos;
using BobsBuddy.Simulation;
using BobsBuddy.Trinkets;
using BobsBuddy.Utils;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.RemoteData;

namespace Hearthstone_Deck_Tracker.BobsBuddy;

internal class BobsBuddyInvoker
{
	private class SecretDbfIdComparer : IEqualityComparer<int?>
	{
		public bool Equals(int? x, int? y)
		{
			if (!x.HasValue || !y.HasValue)
			{
				return false;
			}
			return x == y;
		}

		public int GetHashCode(int? obj)
		{
			if (!obj.HasValue)
			{
				return 0;
			}
			return obj.GetHashCode();
		}
	}

	private const int Iterations = 10000;

	private const int StateChangeDelay = 500;

	private const int MaxTime = 1500;

	private const int MaxTimeForComplexBoards = 3000;

	private const int MaxTimeForLeapfrogger = 5000;

	private const int MinimumSimulationsToReportSentry = 2500;

	private const int LichKingDelay = 2000;

	private readonly GameV2 _game;

	private readonly Random _rnd = new Random();

	private Input? _input;

	private int _turn;

	private Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity? _attackingHero;

	private Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity? _defendingHero;

	public Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity? LastAttackingHero;

	public int LastAttackingHeroAttack;

	private List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> _opponentHand = new List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>();

	private readonly Dictionary<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity, Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> _opponentHandMap = new Dictionary<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity, Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>();

	private List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> _opponentSecrets = new List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>();

	private static Guid _currentGameId;

	private static readonly Dictionary<string, BobsBuddyInvoker> _instances = new Dictionary<string, BobsBuddyInvoker>();

	private global::BobsBuddy.Simulation.Player DuosInputPlayer = new global::BobsBuddy.Simulation.Player(null);

	private global::BobsBuddy.Simulation.Player DuosInputOpponent = new global::BobsBuddy.Simulation.Player(null);

	private global::BobsBuddy.Simulation.Player? DuosInputPlayerTeammate;

	private global::BobsBuddy.Simulation.Player? DuosInputOpponentTeammate;

	private readonly string _instanceKey;

	private (UnsupportedInteractionException ex, string message)? _pendingUnsupportedInteraction;

	private bool _unsupportedInteractionReported;

	private BobsBuddyState _state;

	private int _reRunCount;

	internal static int ThreadCount => Environment.ProcessorCount / 2;

	private static BobsBuddyPanel BobsBuddyDisplay => Core.Overlay.BobsBuddyDisplay;

	private static bool ReportErrors
	{
		get
		{
			if (Version.TryParse(Remote.Config.Data?.BobsBuddy?.SentryMinRequiredVersion ?? string.Empty, out var result))
			{
				return Helper.GetCurrentVersion() >= result;
			}
			return false;
		}
	}

	public Output? Output { get; private set; }

	private bool DoNotReport { get; set; } = true;

	public BobsBuddyErrorState ErrorState { get; private set; }

	public BobsBuddyState State
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
			Core.Overlay.ChinaModuleVM.BobsBuddyState = value;
			DebugLog($"New State: {value}", "State", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		}
	}

	public static BobsBuddyInvoker GetInstance(Guid gameId, int turn, bool createInstanceIfNoneFound = true)
	{
		if (_currentGameId != gameId)
		{
			Log.Debug("New GameId. Clearing instances...", "GetInstance", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			_instances.Clear();
		}
		_currentGameId = gameId;
		string key = $"{gameId}_{turn}";
		if (!_instances.TryGetValue(key, out BobsBuddyInvoker value) && createInstanceIfNoneFound)
		{
			value = new BobsBuddyInvoker(key);
			_instances[key] = value;
		}
		return value;
	}

	public void DebugLog(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
	{
		Log.Info(msg, memberName, sourceFilePath);
	}

	private BobsBuddyInvoker(string key)
	{
		_game = Core.Game;
		_instanceKey = key;
	}

	public bool ShouldRun()
	{
		if (!Config.Instance.RunBobsBuddy)
		{
			return false;
		}
		if (Remote.Config.Data?.BobsBuddy?.Disabled == true)
		{
			return false;
		}
		if (ErrorState == BobsBuddyErrorState.None && Version.TryParse(Remote.Config.Data?.BobsBuddy?.MinRequiredVersion, out var result) && result > Helper.GetCurrentVersion())
		{
			DebugLog($"Update to {result} required. Not running simulations.", "ShouldRun", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			ErrorState = BobsBuddyErrorState.UpdateRequired;
			BobsBuddyDisplay.SetErrorState(BobsBuddyErrorState.UpdateRequired);
		}
		if (ErrorState == BobsBuddyErrorState.UpdateRequired)
		{
			return false;
		}
		return true;
	}

	public async void StartCombat()
	{
		_opponentHand = new List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>();
		_opponentSecrets = new List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>();
		try
		{
			if (!ShouldRun())
			{
				return;
			}
			DebugLog(_instanceKey, "StartCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (_game.IsBattlegroundsDuosMatch)
			{
				SnapshotBoardState(_game.GetTurnNumber());
				BobsBuddyDisplay.SetState(BobsBuddyState.WaitingForTeammates);
				BobsBuddyDisplay.ResetText();
				if (_input != null && (DuosInputPlayerTeammate == null || DuosInputOpponentTeammate == null))
				{
					DebugLog("Waiting Teammates. Exiting.", "StartCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					return;
				}
			}
			else
			{
				if (State >= BobsBuddyState.Combat)
				{
					DebugLog($"{_instanceKey} already in {State} state. Exiting", "StartCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					return;
				}
				SnapshotBoardState(_game.GetTurnNumber());
			}
			State = BobsBuddyState.Combat;
			DebugLog(_instanceKey + " Waiting for state changes...", "StartCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			await Task.Delay(500);
			if (State != BobsBuddyState.Combat)
			{
				DebugLog($"{_instanceKey} no longer in combat: State={State}. Exiting", "StartCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				return;
			}
			DebugLog(_instanceKey + " continuing...", "StartCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (!HasErrorState("StartCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs"))
			{
				DebugLog("Setting UI state to combat...", "StartCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				BobsBuddyDisplay.SetState(BobsBuddyState.Combat);
				BobsBuddyDisplay.ResetText();
				if (_input != null && (_input.Player.HeroPowers.Any((HeroPowerData hp) => hp.CardId == "TB_BaconShop_HP_024" && hp.IsActivated) || _input.Opponent.HeroPowers.Any((HeroPowerData hp) => hp.CardId == "TB_BaconShop_HP_024" && hp.IsActivated)))
				{
					await Task.Delay(2000);
				}
				await RunAndDisplaySimulationAsync();
			}
		}
		catch (Exception ex)
		{
			DebugLog(ex.ToString(), "StartCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			Log.Error(ex, "StartCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (ReportErrors)
			{
				Sentry.CaptureBobsBuddyException(ex, _input, _turn, _game.IsBattlegroundsDuosMatch);
			}
		}
	}

	public async void MaybeRunDuosPartialCombat()
	{
		if (_input != null && DuosInputPlayerTeammate != null && DuosInputOpponentTeammate != null)
		{
			DebugLog("No need to run partial combat, all teammates found. Exiting.", "MaybeRunDuosPartialCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			return;
		}
		try
		{
			if (!ShouldRun())
			{
				return;
			}
			DebugLog(_instanceKey, "MaybeRunDuosPartialCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (!HasErrorState("MaybeRunDuosPartialCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs"))
			{
				State = BobsBuddyState.CombatPartial;
				DebugLog("Setting UI state to combat...", "MaybeRunDuosPartialCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				BobsBuddyDisplay.SetState(BobsBuddyState.CombatPartial);
				BobsBuddyDisplay.ResetText();
				if (_input != null && DuosInputPlayerTeammate == null)
				{
					_input.PlayerTeammate = null;
				}
				if (_input != null && DuosInputOpponentTeammate == null)
				{
					_input.OpponentTeammate = null;
				}
				await RunAndDisplaySimulationAsync();
			}
		}
		catch (Exception ex)
		{
			DebugLog(ex.ToString(), "MaybeRunDuosPartialCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			Log.Error(ex, "MaybeRunDuosPartialCombat", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (ReportErrors)
			{
				Sentry.CaptureBobsBuddyException(ex, _input, _turn, isDuos: true);
			}
		}
	}

	private async Task RunAndDisplaySimulationAsync()
	{
		DebugLog("Running simulation...", "RunAndDisplaySimulationAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		BobsBuddyDisplay.HidePercentagesShowSpinners();
		Output output = await RunSimulation();
		if (output == null || _input == null)
		{
			DebugLog("Simulation returned no result. Exiting.", "RunAndDisplaySimulationAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		}
		else if (output.simulationCount <= 500 && output.myExitCondition == Simulator.ExitConditions.Time)
		{
			DebugLog("Could not perform enough simulations. Displaying error state and exiting.", "RunAndDisplaySimulationAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			ErrorState = BobsBuddyErrorState.NotEnoughData;
			BobsBuddyDisplay.SetErrorState(BobsBuddyErrorState.NotEnoughData);
		}
		else if (State == BobsBuddyState.CombatPartial)
		{
			DebugLog("Displaying partial simulation results", "RunAndDisplaySimulationAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			BobsBuddyDisplay.ShowPartialDuosSimulation(output.winRate, output.tieRate, output.lossRate, output.theirDeathRate, output.myDeathRate, output.damageResults.ToList(), DuosInputPlayerTeammate == null, _input.Player.Health <= _input.DamageCap, _input.Opponent.Health <= _input.DamageCap);
		}
		else
		{
			DebugLog("Displaying simulation results", "RunAndDisplaySimulationAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			BobsBuddyDisplay.ShowCompletedSimulation(output.winRate, output.tieRate, output.lossRate, output.theirDeathRate, output.myDeathRate, output.damageResults.ToList());
		}
	}

	public async Task StartShoppingAsync(bool isGameOver = false)
	{
		try
		{
			ReportPendingUnsupportedInteraction();
			if (!ShouldRun())
			{
				return;
			}
			DebugLog(_instanceKey, "StartShoppingAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (State == BobsBuddyState.Shopping || State == BobsBuddyState.ShoppingAfterPartial)
			{
				DebugLog(_instanceKey + " already in shopping state. Exiting", "StartShoppingAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				return;
			}
			bool flag = State == BobsBuddyState.CombatPartial;
			if (isGameOver)
			{
				if (State != BobsBuddyState.Initial)
				{
					DebugLog("Setting UI state to GameOver", "StartShoppingAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					State = (flag ? BobsBuddyState.GameOverAfterPartial : BobsBuddyState.GameOver);
				}
			}
			else
			{
				DebugLog("Setting UI state to Shopping", "StartShoppingAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				State = (flag ? BobsBuddyState.ShoppingAfterPartial : BobsBuddyState.Shopping);
			}
			if (!HasErrorState("StartShoppingAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs"))
			{
				BobsBuddyDisplay.SetLastOutcome(GetLastCombatDamageDealt());
				BobsBuddyDisplay.SetState(State);
				ValidateSimulationResultAsync().Forget();
			}
		}
		catch (Exception ex)
		{
			DebugLog(ex.ToString(), "StartShoppingAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			Log.Error(ex, "StartShoppingAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (ReportErrors)
			{
				Sentry.CaptureBobsBuddyException(ex, _input, _turn, _game.IsBattlegroundsDuosMatch);
			}
		}
	}

	private void ReportPendingUnsupportedInteraction()
	{
		(UnsupportedInteractionException, string)? pendingUnsupportedInteraction = _pendingUnsupportedInteraction;
		if (!pendingUnsupportedInteraction.HasValue)
		{
			return;
		}
		(UnsupportedInteractionException, string) valueOrDefault = pendingUnsupportedInteraction.GetValueOrDefault();
		_pendingUnsupportedInteraction = null;
		if (!_unsupportedInteractionReported)
		{
			_unsupportedInteractionReported = true;
			bool isBattlegroundsDuosMatch = _game.IsBattlegroundsDuosMatch;
			if (ReportErrors)
			{
				Sentry.CaptureBobsBuddyException(valueOrDefault.Item1, _input, _turn, isBattlegroundsDuosMatch);
			}
			Influx.OnBobsBuddyUnsupportedInteraction(valueOrDefault.Item1.Entity?.CardID, valueOrDefault.Item2, _turn, isBattlegroundsDuosMatch, valueOrDefault.Item1.Entity?.ControlledByPlayer);
		}
	}

	private bool HasErrorState([CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
	{
		if (ErrorState == BobsBuddyErrorState.None)
		{
			return false;
		}
		BobsBuddyDisplay.SetErrorState(ErrorState);
		DebugLog($"ErrorState={ErrorState}", "HasErrorState", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		return true;
	}

	internal void UpdateAttackingEntities(Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity attacker, Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity defender)
	{
		if (attacker.IsHero && defender.IsHero)
		{
			DebugLog("Updating entities with attacker=" + attacker.Card.Name + ", defender=" + defender.Card.Name, "UpdateAttackingEntities", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			_defendingHero = defender;
			_attackingHero = attacker;
		}
	}

	private void SetupInputPlayer(Simulator simulator, Hearthstone_Deck_Tracker.Hearthstone.Player gamePlayer, global::BobsBuddy.Simulation.Player inputPlayer, Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity? playerEntity, bool friendly)
	{
		Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity hero = gamePlayer.Hero;
		if (playerEntity == null)
		{
			throw new ArgumentException(friendly ? "Player" : "Opponent Entity could not be found. Exiting.");
		}
		foreach (Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity item2 in gamePlayer.Board)
		{
			if (string.IsNullOrWhiteSpace(item2.CardId))
			{
				continue;
			}
			if (!item2.Card.IsKnownCard)
			{
				ErrorState = BobsBuddyErrorState.UnknownCards;
				throw new ArgumentException("Board has unknown cards. Exiting.");
			}
			if (item2.Card.Data == null || !item2.IsMinion)
			{
				continue;
			}
			switch (SupportedCards.VerifyCardIsSupported(item2.Card.Data))
			{
			case SupportedCards.Result.UnknownCard:
				ErrorState = BobsBuddyErrorState.UnknownCards;
				throw new ArgumentException("Board has unknown cards. Exiting.");
			case SupportedCards.Result.TextChanged:
				if (simulator.MinionFactory.HasImplementationFor(item2.CardId))
				{
					ErrorState = BobsBuddyErrorState.UnknownCards;
					throw new ArgumentException("Board has cards with changed text. Exiting.");
				}
				break;
			}
		}
		if (hero == null)
		{
			throw new ArgumentException("Hero(es) could not be found. Exiting.");
		}
		if (!friendly && inputPlayer.Health <= 0)
		{
			inputPlayer.Health = 1000;
		}
		inputPlayer.Health = hero.Health + hero.GetTag(GameTag.ARMOR);
		inputPlayer.DamageTaken = hero.GetTag(GameTag.DAMAGE);
		inputPlayer.Tier = hero.GetTag(GameTag.PLAYER_TECH_LEVEL);
		List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> list = gamePlayer.Board.Where((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.IsHeroPower).Take(2).ToList();
		HashSet<int> claimedHeroPowerTargets = new HashSet<int>();
		foreach (Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity item3 in list)
		{
			int data = item3?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0;
			int data2 = item3?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2) ?? 0;
			int data3 = item3?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_3) ?? 0;
			Minion attachedMinion = null;
			if (item3?.CardId == "BG25_HERO_103p")
			{
				IEnumerable<int> minionsInPlay = from x in gamePlayer.Board
					where x.IsMinion && x.IsControlledBy(gamePlayer.Id)
					select x.Id;
				int num = (from x in gamePlayer.PlayerEntities
					where x.CardId == "BG25_HERO_103pe" && (!friendly || (x.IsInPlay && friendly))
					select x.GetTag(GameTag.ATTACHED)).FirstOrDefault((int x) => minionsInPlay.Any((int y) => y == x) && !claimedHeroPowerTargets.Contains(x));
				if (num > 0)
				{
					data = num;
					claimedHeroPowerTargets.Add(num);
				}
			}
			if (item3?.CardId == "BGDUO_HERO_101p")
			{
				IEnumerable<int> minionsInPlay2 = from x in gamePlayer.Board
					where x.IsMinion && x.IsControlledBy(gamePlayer.Id)
					select x.Id;
				int num2 = (from x in gamePlayer.PlayerEntities
					where x.CardId == "BGDUO_HERO_101pe2" && (!friendly || (x.IsInPlay && friendly))
					select x.GetTag(GameTag.ATTACHED)).FirstOrDefault((int x) => minionsInPlay2.Any((int y) => y == x) && !claimedHeroPowerTargets.Contains(x));
				if (num2 > 0)
				{
					data = num2;
					claimedHeroPowerTargets.Add(num2);
				}
			}
			if (item3?.CardId == "BG22_HERO_000p_Alt")
			{
				int attachedEntityId = item3.GetTag(GameTag.TAG_SCRIPT_DATA_ENT_1);
				Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity = gamePlayer.SetAside.FirstOrDefault((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity e) => e.Id == attachedEntityId);
				if (entity != null)
				{
					attachedMinion = BobsBuddyUtils.GetMinionFromEntity(simulator, friendly, entity, GetAttachedEntities(attachedEntityId));
				}
			}
			inputPlayer.AddHeroPower(item3?.CardId ?? "", friendly, BobsBuddyUtils.WasHeroPowerActivated(item3), data, data2, data3, attachedMinion, item3?.Id ?? 0);
		}
		foreach (Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity quest in gamePlayer.Quests)
		{
			Card cardFromDbfId = Database.GetCardFromDbfId(quest.GetTag(GameTag.QUEST_REWARD_DATABASE_ID), collectible: false);
			inputPlayer.Quests.Add(new QuestData
			{
				QuestProgress = quest.GetTag(GameTag.QUEST_PROGRESS),
				QuestProgressTotal = quest.GetTag(GameTag.QUEST_PROGRESS_TOTAL),
				QuestCardId = (quest.CardId ?? ""),
				RewardCardId = (cardFromDbfId?.Id ?? "")
			});
		}
		foreach (Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity questReward in gamePlayer.QuestRewards)
		{
			inputPlayer.Quests.Add(new QuestData
			{
				RewardCardId = (questReward.Info.LatestCardId ?? ""),
				RewardScriptDataNum1 = questReward.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1),
				RewardScriptDataNum2 = questReward.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2)
			});
		}
		foreach (Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity trinket in gamePlayer.Trinkets)
		{
			inputPlayer.Trinkets.Add(BobsBuddyUtils.GetTrinketFromEntity(simulator.TrinketFactory, friendly, trinket));
		}
		foreach (Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity objective in gamePlayer.Objectives)
		{
			inputPlayer.Objectives.Add(BobsBuddyUtils.GetObjectiveFromEntity(simulator.ObjectiveFactory, friendly, objective));
		}
		foreach (Minion item4 in from e in BobsBuddyUtils.GetOrderedMinions(gamePlayer.Board)
			where e.IsControlledBy(gamePlayer.Id)
			select BobsBuddyUtils.GetMinionFromEntity(simulator, friendly, e, GetAttachedEntities(e.Id), _game.Entities))
		{
			inputPlayer.Side.Add(item4);
		}
		if (friendly)
		{
			inputPlayer.SetSecrets(gamePlayer.Secrets.Select<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity, int?>((Func<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity, int?>)((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.Card.DbfId)).ToList());
			foreach (Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity orderedHandEntity in BobsBuddyUtils.GetOrderedHandEntities(gamePlayer.Hand))
			{
				if (orderedHandEntity.IsMinion)
				{
					MinionCardEntity item = new MinionCardEntity(BobsBuddyUtils.GetMinionFromEntity(simulator, player: true, orderedHandEntity, GetAttachedEntities(orderedHandEntity.Id)), null, simulator)
					{
						CanSummon = !orderedHandEntity.HasTag(GameTag.LITERALLY_UNPLAYABLE)
					};
					inputPlayer.Hand.Add(item);
				}
				else if (orderedHandEntity.CardId == "BG20_GEM")
				{
					inputPlayer.Hand.Add(new BloodGem(null, simulator));
				}
				else if (orderedHandEntity.IsSpell)
				{
					inputPlayer.Hand.Add(new SpellCardEntity(null, simulator));
				}
				else
				{
					inputPlayer.Hand.Add(new CardEntity(orderedHandEntity.CardId ?? "", null, simulator));
				}
			}
		}
		else
		{
			if (BobsBuddyUtils.IsOpponentHeroKelThuzad())
			{
				inputPlayer.SetHeroIsKelThuzad();
			}
			inputPlayer.SetSecrets((_opponentSecrets = gamePlayer.Secrets.ToList()).Select((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => string.IsNullOrEmpty(x.CardId) ? ((int?)null) : new int?(x.Card.DbfId)).Distinct(new SecretDbfIdComparer()).ToList());
			_opponentHand = BobsBuddyUtils.GetOrderedHandEntities(gamePlayer.Hand).ToList();
			inputPlayer.Hand.Clear();
			inputPlayer.Hand.AddRange(GetOpponentHandEntities(simulator));
		}
		List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> source = GetAttachedEntities(playerEntity.Id).ToList();
		Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity2 = source.FirstOrDefault((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.CardId == "BG25_008pe");
		if (entity2 != null)
		{
			inputPlayer.EternalKnightCounter = entity2.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
		}
		Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity3 = source.FirstOrDefault((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.CardId == "BG25_011pe");
		if (entity3 != null)
		{
			inputPlayer.UndeadAttackBonus = entity3.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			inputPlayer.UndeadHealthBonus = entity3.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
		}
		Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity4 = source.FirstOrDefault((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.CardId == "BG34_Giant_362pe");
		if (entity4 != null)
		{
			inputPlayer.BeastAttackBonus = entity4.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			inputPlayer.BeastHealthBonus = entity4.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
			Log.Info($"pBeastAttack={inputPlayer.BeastAttackBonus}, pBeastHealth={inputPlayer.BeastHealthBonus}, friendly={friendly}", "SetupInputPlayer", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		}
		Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity5 = source.FirstOrDefault((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.CardId == "BG_TTN_401pe");
		if (entity5 != null)
		{
			inputPlayer.AncestralAutomatonCounter = entity5.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
		}
		Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity6 = source.FirstOrDefault((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.CardId == "BG31_808pe");
		if (entity6 != null)
		{
			inputPlayer.BeetlesAtkBuff = entity6.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			inputPlayer.BeetlesHealthBuff = entity6.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
			Log.Info($"pBeetleAtk={inputPlayer.BeetlesAtkBuff}, pBeetleHealth={inputPlayer.BeetlesHealthBuff}, friendly={friendly}", "SetupInputPlayer", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		}
		Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity7 = source.FirstOrDefault((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.CardId == "BG34_402pe");
		if (entity7 != null)
		{
			inputPlayer.WhelpAttackBonus = entity7.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			inputPlayer.WhelpHealthBonus = entity7.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
			Log.Info($"pWhelpAttack={inputPlayer.WhelpAttackBonus}, pWhelpHealth={inputPlayer.WhelpHealthBonus}, friendly={friendly}", "SetupInputPlayer", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		}
		inputPlayer.ElementalPlayCounter = playerEntity.GetTag(GameTag.BACON_ELEMENTAL_PLAY_COUNTER);
		Log.Info($"pEternal={inputPlayer.EternalKnightCounter}, pUndead={inputPlayer.UndeadAttackBonus}, pElemental={inputPlayer.ElementalPlayCounter}, friendly={friendly}", "SetupInputPlayer", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		inputPlayer.PiratesSummonCounter = playerEntity.GetTag((GameTag)2358);
		inputPlayer.ResourcesSpentThisGame = playerEntity.GetTag(GameTag.NUM_RESOURCES_SPENT_THIS_GAME);
		inputPlayer.BeastsSummonCounter = playerEntity.GetTag((GameTag)3962);
		inputPlayer.FriendlyMinionsDeadLastCombatCounter = playerEntity.GetTag((GameTag)2717);
		inputPlayer.BattlecryCounter = playerEntity.GetTag((GameTag)3236);
		Log.Info($"pPirates={inputPlayer.PiratesSummonCounter}, pBeasts={inputPlayer.BeastsSummonCounter}, pDeadLastCombat={inputPlayer.FriendlyMinionsDeadLastCombatCounter}, pBattlecry={inputPlayer.BattlecryCounter}, friendly={friendly}", "SetupInputPlayer", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		inputPlayer.BloodGemAtkBuff = playerEntity.GetTag(GameTag.BACON_BLOODGEMBUFFATKVALUE);
		inputPlayer.BloodGemHealthBuff = playerEntity.GetTag(GameTag.BACON_BLOODGEMBUFFHEALTHVALUE);
		Log.Info($"pBloodGem=+{inputPlayer.BloodGemAtkBuff}/+{inputPlayer.BloodGemHealthBuff}, friendly={friendly}", "SetupInputPlayer", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		inputPlayer.TavernSpellAtkBuff = playerEntity.GetTag(GameTag.TAVERN_SPELL_ATTACK_INCREASE);
		inputPlayer.TavernSpellHealthBuff = playerEntity.GetTag(GameTag.TAVERN_SPELL_HEALTH_INCREASE);
		inputPlayer.TavernSpellCounter = playerEntity.GetTag((GameTag)3088);
		inputPlayer.DeathrattleCounter = playerEntity.GetTag((GameTag)4639);
		Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity8 = source.FirstOrDefault((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.CardId == "BG33_112pe");
		if (entity8 != null)
		{
			inputPlayer.HauntedAtkBuff = entity8.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			inputPlayer.HauntedHealthBuff = entity8.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
			Log.Info($"pHauntedAtk={inputPlayer.HauntedAtkBuff}, pHauntedHealth={inputPlayer.HauntedHealthBuff}, friendly={friendly}", "SetupInputPlayer", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		}
	}

	private void SnapshotBoardState(int turn)
	{
		DebugLog("Snapshotting board state...", "SnapshotBoardState", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		LastAttackingHero = null;
		Simulator simulator = new Simulator();
		Input input = new Input();
		if (_game.GameEntity == null)
		{
			DebugLog("GameEntity could not be found. Exiting.", "SnapshotBoardState", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			return;
		}
		input.availableRaces = BattlegroundsUtils.GetAvailableRaces(_currentGameId).ToList();
		if (_game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP_ENABLED) > 0)
		{
			input.DamageCap = _game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP);
		}
		try
		{
			if (_input == null)
			{
				SetupInputPlayer(simulator, _game.Player, input.Player, _game.PlayerEntity, friendly: true);
				SetupInputPlayer(simulator, _game.Opponent, input.Opponent, _game.OpponentEntity, friendly: false);
				DuosInputPlayer = input.Player;
				DuosInputOpponent = input.Opponent;
				DuosInputPlayerTeammate = null;
				DuosInputOpponentTeammate = null;
			}
			else
			{
				if (_game.DuosWasPlayerHeroModified && DuosInputPlayerTeammate == null && input.PlayerTeammate != null)
				{
					SetupInputPlayer(simulator, _game.Player, input.PlayerTeammate, _game.PlayerEntity, friendly: true);
					DuosInputPlayerTeammate = input.PlayerTeammate;
				}
				if (_game.DuosWasOpponentHeroModified && DuosInputOpponentTeammate == null && input.OpponentTeammate != null)
				{
					SetupInputPlayer(simulator, _game.Opponent, input.OpponentTeammate, _game.OpponentEntity, friendly: false);
					DuosInputOpponentTeammate = input.OpponentTeammate;
				}
			}
			if (_game.IsBattlegroundsDuosMatch)
			{
				input.isDuos = true;
				input.Player = DuosInputPlayer;
				input.Opponent = DuosInputOpponent;
				input.PlayerTeammate = DuosInputPlayerTeammate ?? input.PlayerTeammate;
				input.OpponentTeammate = DuosInputOpponentTeammate ?? input.OpponentTeammate;
			}
		}
		catch (Exception ex)
		{
			DebugLog(ex.ToString(), "SnapshotBoardState", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			return;
		}
		int? battlegroundsAnomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(_game.GameEntity);
		string text = ((!battlegroundsAnomalyDbfId.HasValue) ? null : Database.GetCardFromDbfId(battlegroundsAnomalyDbfId.Value, collectible: false)?.Id);
		if (text != null)
		{
			input.Anomaly = simulator.AnomalyFactory.Create(text);
		}
		input.SetTurn(turn);
		_input = input;
		_turn = turn;
		DebugLog("Successfully snapshotted board state", "SnapshotBoardState", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
	}

	private Task TryRerun()
	{
		if (_reRunCount++ <= 10)
		{
			DebugLog($"Input changed, re-running simulation! (#{_reRunCount})", "TryRerun", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (ShouldRun())
			{
				bool flag = ErrorState == BobsBuddyErrorState.None && Config.Instance.ShowBobsBuddyDuringCombat;
				ErrorState = BobsBuddyErrorState.None;
				BobsBuddyDisplay.SetErrorState(BobsBuddyErrorState.None, null, BobsBuddyDisplay.ResultsPanelExpanded || flag);
				Output = null;
				return RunAndDisplaySimulationAsync();
			}
		}
		else
		{
			DebugLog("Input changed, but the simulation already re-ran ten times", "TryRerun", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		}
		return Task.CompletedTask;
	}

	internal async void UpdateOpponentHand(Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity, Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity copy)
	{
		if (_input != null && State == BobsBuddyState.Combat)
		{
			_opponentHandMap[entity] = copy;
			List<CardEntity> list = GetOpponentHandEntities(new Simulator()).ToList();
			if (list.Count((CardEntity x) => x is MinionCardEntity) > _input.Opponent.Hand.Count((CardEntity x) => x is MinionCardEntity))
			{
				_input.Opponent.Hand.Clear();
				_input.Opponent.Hand.AddRange(list);
				await TryRerun();
			}
		}
	}

	internal async void UpdateOpponentSecret(Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity)
	{
		if (_input != null && State == BobsBuddyState.Combat && !_game.IsBattlegroundsDuosMatch)
		{
			_input.Opponent.SetSecrets(_opponentSecrets.Select((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => string.IsNullOrEmpty(x.CardId) ? ((int?)null) : new int?(x.Card.DbfId)).Distinct(new SecretDbfIdComparer()).ToList());
			await TryRerun();
		}
	}

	internal async void UpdateLockAndLoadHeroPower(Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity attachedEntity, bool isOpponent)
	{
		if (_input == null || State != BobsBuddyState.Combat)
		{
			return;
		}
		bool flag = _game.IsBattlegroundsDuosMatch;
		bool flag2 = false;
		if (isOpponent && !flag)
		{
			HeroPowerData heroPowerData = _input.Opponent.HeroPowers.FirstOrDefault((HeroPowerData hp) => hp.CardId == "BG22_HERO_000p_Alt");
			if (heroPowerData == null)
			{
				flag = true;
			}
			else if (heroPowerData.AttachedMinion == null && heroPowerData.Data3 == 0)
			{
				heroPowerData.AttachedMinion = BobsBuddyUtils.GetMinionFromEntity(new Simulator(), player: false, attachedEntity, GetAttachedEntities(attachedEntity.Id));
				flag2 = true;
			}
		}
		if (flag)
		{
			UpdateDuosLockAndLoadHeroPower(attachedEntity.Card.DbfId);
		}
		if (flag2)
		{
			await TryRerun();
		}
	}

	internal async void UpdateDuosLockAndLoadHeroPower(int cardDbfid)
	{
		if (_input == null || State != BobsBuddyState.Combat)
		{
			return;
		}
		HeroPowerData heroPowerData = _input.Player.HeroPowers.FirstOrDefault((HeroPowerData hp) => hp.CardId == "BG22_HERO_000p_Alt");
		if (heroPowerData == null && _input.PlayerTeammate != null)
		{
			heroPowerData = _input.PlayerTeammate.HeroPowers.FirstOrDefault((HeroPowerData hp) => hp.CardId == "BG22_HERO_000p_Alt");
		}
		if (heroPowerData == null)
		{
			heroPowerData = _input.Opponent.HeroPowers.FirstOrDefault((HeroPowerData hp) => hp.CardId == "BG22_HERO_000p_Alt");
		}
		if (heroPowerData == null && _input.OpponentTeammate != null)
		{
			heroPowerData = _input.OpponentTeammate.HeroPowers.FirstOrDefault((HeroPowerData hp) => hp.CardId == "BG22_HERO_000p_Alt");
		}
		if (heroPowerData != null && heroPowerData.AttachedMinion == null && heroPowerData.Data3 <= 0)
		{
			heroPowerData.Data3 = cardDbfid;
			await TryRerun();
		}
	}

	internal async void UpdateBackToBackSpellBonus(Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity backToBackEnchantmentEntity, bool isOpponent)
	{
		if (_input == null || State != BobsBuddyState.Combat)
		{
			return;
		}
		if (isOpponent)
		{
			if (_input.Opponent.BackToBackAtk != 0 || _input.Opponent.BackToBackHealth != 0)
			{
				return;
			}
			_input.Opponent.BackToBackAtk = backToBackEnchantmentEntity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			_input.Opponent.BackToBackHealth = backToBackEnchantmentEntity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
		}
		else
		{
			if (_input.Player.BackToBackAtk != 0 || _input.Player.BackToBackHealth != 0)
			{
				return;
			}
			_input.Player.BackToBackAtk = backToBackEnchantmentEntity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			_input.Player.BackToBackHealth = backToBackEnchantmentEntity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
		}
		await TryRerun();
	}

	internal async void UpdateSandyTransformDuos(Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity attachedEntity, int sandyEntityId)
	{
		if (_input == null || State != BobsBuddyState.Combat)
		{
			return;
		}
		bool player = true;
		Minion minion = _input.Player.Side.FirstOrDefault((Minion m) => m.game_id == sandyEntityId);
		if (minion == null && _input.PlayerTeammate != null)
		{
			minion = _input.PlayerTeammate.Side.FirstOrDefault((Minion m) => m.game_id == sandyEntityId);
		}
		if (minion == null)
		{
			player = false;
			minion = _input.Opponent.Side.FirstOrDefault((Minion m) => m.game_id == sandyEntityId);
		}
		if (minion == null && _input.OpponentTeammate != null)
		{
			minion = _input.OpponentTeammate.Side.FirstOrDefault((Minion m) => m.game_id == sandyEntityId);
		}
		if (minion != null && ((Sandy)minion).AttachedMinion == null)
		{
			((Sandy)minion).AttachedMinion = BobsBuddyUtils.GetMinionFromEntity(new Simulator(), player, attachedEntity, GetAttachedEntities(attachedEntity.Id));
			await TryRerun();
		}
	}

	internal async void UpdateFlobbidinousFloopTransformDuos(Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity attachedEntity)
	{
		if (_input == null || State != BobsBuddyState.Combat)
		{
			return;
		}
		bool player = true;
		HeroPowerData heroPowerData = _input.Player.HeroPowers.FirstOrDefault((HeroPowerData hp) => hp.CardId == "BGDUO_HERO_101p");
		if (heroPowerData == null && _input.PlayerTeammate != null)
		{
			heroPowerData = _input.PlayerTeammate.HeroPowers.FirstOrDefault((HeroPowerData hp) => hp.CardId == "BGDUO_HERO_101p");
		}
		if (heroPowerData == null)
		{
			player = false;
			heroPowerData = _input.Opponent.HeroPowers.FirstOrDefault((HeroPowerData hp) => hp.CardId == "BGDUO_HERO_101p");
		}
		if (heroPowerData == null && _input.OpponentTeammate != null)
		{
			heroPowerData = _input.OpponentTeammate.HeroPowers.FirstOrDefault((HeroPowerData hp) => hp.CardId == "BGDUO_HERO_101p");
		}
		if (heroPowerData != null && heroPowerData.AttachedMinion == null)
		{
			heroPowerData.AttachedMinion = BobsBuddyUtils.GetMinionFromEntity(new Simulator(), player, attachedEntity, GetAttachedEntities(attachedEntity.Id));
			await TryRerun();
		}
	}

	internal async void UpdateSummoningSphereDuos(Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity attachedEntity, int trinketEntityId)
	{
		if (_input == null || State != BobsBuddyState.Combat)
		{
			return;
		}
		bool player = true;
		Trinket trinket = _input.Player.Trinkets.FirstOrDefault((Trinket t) => t.game_id == trinketEntityId && t.CardID == "BGDUO_MagicItem_003");
		if (trinket == null && _input.PlayerTeammate != null)
		{
			trinket = _input.PlayerTeammate.Trinkets.FirstOrDefault((Trinket t) => t.game_id == trinketEntityId && t.CardID == "BGDUO_MagicItem_003");
		}
		if (trinket == null)
		{
			player = false;
			trinket = _input.Opponent.Trinkets.FirstOrDefault((Trinket t) => t.game_id == trinketEntityId && t.CardID == "BGDUO_MagicItem_003");
		}
		if (trinket == null && _input.OpponentTeammate != null)
		{
			trinket = _input.OpponentTeammate.Trinkets.FirstOrDefault((Trinket t) => t.game_id == trinketEntityId && t.CardID == "BGDUO_MagicItem_003");
		}
		if (trinket != null && ((SummoningSphere)trinket).AttachedMinion == null)
		{
			((SummoningSphere)trinket).AttachedMinion = BobsBuddyUtils.GetMinionFromEntity(new Simulator(), player, attachedEntity, GetAttachedEntities(attachedEntity.Id));
			await TryRerun();
		}
	}

	internal async void UpdateMinionEnchantment(Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity enchantmentEntity, int attachedToEntityId, bool isPlayerMinion)
	{
		if (_input == null || State != BobsBuddyState.Combat)
		{
			return;
		}
		Minion minion = (isPlayerMinion ? _input.Player : _input.Opponent).Side.FirstOrDefault((Minion m) => m.game_id == attachedToEntityId);
		if (minion == null)
		{
			return;
		}
		if (enchantmentEntity.Card.TypeEnum == CardType.ENCHANTMENT && enchantmentEntity.CardId != null)
		{
			Enchantment enchantment = new Simulator().EnchantmentFactory.Create(enchantmentEntity.CardId, minion.ControlledByPlayer);
			if (enchantment != null)
			{
				enchantment.ScriptDataNum1 = enchantmentEntity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
				enchantment.ScriptDataNum2 = enchantmentEntity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
				minion.AttachEnchantment(enchantment);
			}
		}
		await TryRerun();
	}

	internal async void UpdateTimewarpedMagnanimoose(List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> summonedEntities, int magnanimooseEntityId, bool isPlayerMinion)
	{
		if (_input == null || State != BobsBuddyState.Combat)
		{
			return;
		}
		Minion minion = (isPlayerMinion ? _input.Player : _input.Opponent).Side.FirstOrDefault((Minion m) => m.game_id == magnanimooseEntityId);
		if (minion != null)
		{
			Simulator simulator = new Simulator();
			List<Minion> summonedMinions = summonedEntities.Select((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity e) => BobsBuddyUtils.GetMinionFromEntity(simulator, isPlayerMinion, e, GetAttachedEntities(e.Id))).ToList();
			if (simulator.EnchantmentFactory.Create("BACON_FAKE_Magnanimoose_Enchantment", minion.ControlledByPlayer) is TimewarpedMagnanimooseEnchantment timewarpedMagnanimooseEnchantment)
			{
				timewarpedMagnanimooseEnchantment.SummonedMinions = summonedMinions;
				minion.AttachEnchantment(timewarpedMagnanimooseEnchantment);
			}
			await TryRerun();
		}
	}

	internal async void UpdateNelliesShipEnchantment(int[] cardDbfids, int attachedToEntityId, bool isPlayerMinion)
	{
		if (_input == null || State != BobsBuddyState.Combat)
		{
			return;
		}
		Minion minion = (isPlayerMinion ? _input.Player : _input.Opponent).Side.FirstOrDefault((Minion m) => m.game_id == attachedToEntityId);
		if (minion != null)
		{
			Enchantment enchantment = new Simulator().EnchantmentFactory.Create("BACON_FAKE_NelliesShip_Enchantment", minion.ControlledByPlayer);
			if (enchantment != null)
			{
				enchantment.ScriptDataNum1 = cardDbfids.ElementAtOrDefault(0);
				enchantment.ScriptDataNum2 = cardDbfids.ElementAtOrDefault(1);
				minion.AttachEnchantment(enchantment);
			}
			await TryRerun();
		}
	}

	private IEnumerable<CardEntity> GetOpponentHandEntities(Simulator simulator)
	{
		foreach (Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity item in _opponentHand)
		{
			Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity value;
			Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity = (_opponentHandMap.TryGetValue(item, out value) ? value : item);
			if (entity.IsMinion)
			{
				IEnumerable<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> attachedEntities = GetAttachedEntities(entity.Id);
				yield return new MinionCardEntity(BobsBuddyUtils.GetMinionFromEntity(simulator, player: false, entity, attachedEntities), null, simulator)
				{
					CanSummon = !entity.HasTag(GameTag.LITERALLY_UNPLAYABLE)
				};
			}
			else if (entity.CardId == "BG20_GEM")
			{
				yield return new BloodGem(null, simulator);
			}
			else if (entity.IsSpell)
			{
				yield return new SpellCardEntity(null, simulator);
			}
			else if (!string.IsNullOrEmpty(entity.CardId))
			{
				yield return new CardEntity(entity.CardId ?? "", null, simulator);
			}
			else
			{
				yield return new UnknownCardEntity(null, simulator);
			}
		}
	}

	private IEnumerable<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> GetAttachedEntities(int entityId)
	{
		return from x in _game.Entities.Values
			where x.IsAttachedTo(entityId) && (x.IsInPlay || x.IsInSetAside || x.IsInGraveyard)
			select x.Clone();
	}

	private async Task<Output?> RunSimulation()
	{
		DebugLog("Running simulations...", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		if (_input == null)
		{
			DebugLog("No input. Exiting.", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			return null;
		}
		try
		{
			DebugLog("----- Simulation Input -----", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (_input.Player.HeroPowers.Any())
			{
				DebugLog($"Player: heroPower={_input.Player.HeroPowers[0].CardId}, used={_input.Player.HeroPowers[0].IsActivated}, data={_input.Player.HeroPowers[0].Data}", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				if (_input.Player.HeroPowers.Count > 1)
				{
					DebugLog($"Player: extraHeroPower={_input.Player.HeroPowers[1].CardId}, used={_input.Player.HeroPowers[1].IsActivated}, data={_input.Player.HeroPowers[1].Data}", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				}
			}
			DebugLog("Hand: " + string.Join(", ", _input.Player.Hand.Select((CardEntity x) => x.ToString())), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			foreach (Minion item in _input.Player.Side)
			{
				DebugLog(item.ToString(), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			}
			foreach (QuestData quest in _input.Player.Quests)
			{
				DebugLog($"[{quest.QuestCardId} ({quest.QuestProgress}/{quest.QuestProgressTotal}): {quest.RewardCardId}, {quest.RewardScriptDataNum1}, {quest.RewardScriptDataNum2}]", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			}
			DebugLog("---", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (_input.Opponent.HeroPowers.Any())
			{
				DebugLog($"Opponent: heroPower={_input.Opponent.HeroPowers[0].CardId}, used={_input.Opponent.HeroPowers[0].IsActivated}, data={_input.Opponent.HeroPowers[0].Data}", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				if (_input.Opponent.HeroPowers.Count > 1)
				{
					DebugLog($"Opponent: extraHeroPower={_input.Opponent.HeroPowers[1].CardId}, used={_input.Opponent.HeroPowers[1].IsActivated}, data={_input.Opponent.HeroPowers[1].Data}", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				}
			}
			DebugLog("Hand: " + string.Join(", ", _input.Opponent.Hand.Select((CardEntity x) => x.ToString())), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			foreach (Minion item2 in _input.Opponent.Side)
			{
				DebugLog(item2.ToString(), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			}
			foreach (QuestData quest2 in _input.Opponent.Quests)
			{
				DebugLog($"[{quest2.QuestCardId} ({quest2.QuestProgress}/{quest2.QuestProgressTotal}): {quest2.RewardCardId}, {quest2.RewardScriptDataNum1}, {quest2.RewardScriptDataNum2}]", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			}
			if (_input.isDuos)
			{
				DebugLog("---", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				if (_input.PlayerTeammate != null)
				{
					DebugLog("---", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					if (_input.PlayerTeammate.HeroPowers.Any())
					{
						DebugLog("PlayerTeammate: heroPower=" + _input.PlayerTeammate.HeroPowers[0].CardId + ", used=" + _input.PlayerTeammate.HeroPowers[0].IsActivated + ", data=" + _input.PlayerTeammate.HeroPowers[0].Data, "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
						if (_input.PlayerTeammate.HeroPowers.Count > 1)
						{
							DebugLog("PlayerTeammate: extraHeroPower=" + _input.PlayerTeammate.HeroPowers[1].CardId + ", used=" + _input.PlayerTeammate.HeroPowers[1].IsActivated + ", data=" + _input.PlayerTeammate.HeroPowers[1].Data, "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
						}
					}
					DebugLog("Hand: " + string.Join(", ", _input.PlayerTeammate.Hand.Select((CardEntity x) => x.ToString())), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					foreach (Minion item3 in _input.PlayerTeammate.Side)
					{
						DebugLog(item3.ToString(), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					}
					foreach (QuestData quest3 in _input.PlayerTeammate.Quests)
					{
						DebugLog($"[{quest3.QuestCardId} ({quest3.QuestProgress}/{quest3.QuestProgressTotal}): {quest3.RewardCardId}, {quest3.RewardScriptDataNum1}, {quest3.RewardScriptDataNum2}]", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					}
				}
				else
				{
					DebugLog("PlayerTeammate: null", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				}
				if (_input.OpponentTeammate != null)
				{
					DebugLog("---", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					if (_input.OpponentTeammate.HeroPowers.Any())
					{
						DebugLog("OpponentTeammate: heroPower=" + _input.OpponentTeammate.HeroPowers[0].CardId + ", used=" + _input.OpponentTeammate.HeroPowers[0].IsActivated + ", data=" + _input.OpponentTeammate.HeroPowers[0].Data, "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
						if (_input.OpponentTeammate.HeroPowers.Count > 1)
						{
							DebugLog("OpponentTeammate: extraHeroPower=" + _input.OpponentTeammate.HeroPowers[1].CardId + ", used=" + _input.OpponentTeammate.HeroPowers[1].IsActivated + ", data=" + _input.OpponentTeammate.HeroPowers[1].Data, "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
						}
					}
					DebugLog("Hand: " + string.Join(", ", _input.OpponentTeammate.Hand.Select((CardEntity x) => x.ToString())), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					foreach (Minion item4 in _input.OpponentTeammate.Side)
					{
						DebugLog(item4.ToString(), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					}
					foreach (QuestData quest4 in _input.OpponentTeammate.Quests)
					{
						DebugLog($"[{quest4.QuestCardId} ({quest4.QuestProgress}/{quest4.QuestProgressTotal}): {quest4.RewardCardId}, {quest4.RewardScriptDataNum1}, {quest4.RewardScriptDataNum2}]", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					}
				}
				else
				{
					DebugLog("OpponentTeammate: null", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				}
			}
			DebugLog("---", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (_input.Player.Secrets.Any())
			{
				DebugLog("Detected the following player S.", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				foreach (Simulator.Secrets secret in _input.Player.Secrets)
				{
					DebugLog(secret.ToString(), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				}
			}
			if (_input.Opponent.Secrets.Any())
			{
				DebugLog("Detected the following opponent S.", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				foreach (Simulator.Secrets secret2 in _input.Opponent.Secrets)
				{
					DebugLog(secret2.ToString(), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				}
			}
			if (_input.isDuos)
			{
				if (_input.OpponentTeammate != null && _input.OpponentTeammate.Secrets.Any())
				{
					DebugLog("Detected the following opponent teammate S.", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					foreach (Simulator.Secrets secret3 in _input.OpponentTeammate.Secrets)
					{
						DebugLog(secret3.ToString(), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					}
				}
				if (_input.PlayerTeammate != null && _input.PlayerTeammate.Secrets.Any())
				{
					DebugLog("Detected the following player teammate S.", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					foreach (Simulator.Secrets secret4 in _input.PlayerTeammate.Secrets)
					{
						DebugLog(secret4.ToString(), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
					}
				}
			}
			DebugLog("----- End of Input -----", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			DebugLog($"Running simulations with MaxIterations={10000} and ThreadCount={ThreadCount}...", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			DateTime start = DateTime.Now;
			int maxDuration = 1500;
			if (IsLeapfroggerCombo(_input.Player.Side) || IsLeapfroggerCombo(_input.PlayerTeammate?.Side) || IsLeapfroggerCombo(_input.Opponent.Side) || IsLeapfroggerCombo(_input.OpponentTeammate?.Side))
			{
				maxDuration = 5000;
			}
			else if (_input.Player.Side.Count >= 6 || _input.Opponent.Side.Count >= 6)
			{
				maxDuration = 3000;
			}
			Output = await new SimulationRunner().SimulateMultiThreaded(_input, 10000, ThreadCount, maxDuration);
			DoNotReport = false;
			_pendingUnsupportedInteraction = null;
			DebugLog("----- Simulation Output -----", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			DebugLog($"Duration={(DateTime.Now - start).TotalMilliseconds}ms, " + $"ExitCondition={Output.myExitCondition}, " + $"Iterations={Output.simulationCount}", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			DebugLog($"WinRate={Output.winRate * 100f}% " + $"(Lethal={Output.theirDeathRate * 100f}%), " + $"TieRate={Output.tieRate * 100f}%, " + $"LossRate={Output.lossRate * 100f}% " + $"(Lethal={Output.myDeathRate * 100f}%)", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			DebugLog("----- End of Output -----", "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			return Output;
		}
		catch (AggregateException ex)
		{
			if (!(ex.InnerExceptions.FirstOrDefault((Exception x) => x is UnsupportedInteractionException) is UnsupportedInteractionException ex2))
			{
				throw;
			}
			DebugLog("Unsupported interaction: " + ex2.Entity?.ToString() + ": " + ex2.Message, "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			Log.Error(ex2, "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			string text = Database.GetCardFromId(ex2.Entity?.CardID)?.LocalizedName;
			string text2 = ((text != null) ? (text + ": ") : "") + ex2.Message;
			BobsBuddyDisplay.SetErrorState(BobsBuddyErrorState.UnsupportedInteraction, text2);
			_pendingUnsupportedInteraction = (ex2, text2);
			Output = null;
			return null;
		}
		catch (Exception ex3)
		{
			DebugLog(ex3.ToString(), "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			Log.Error(ex3, "RunSimulation", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			if (ReportErrors)
			{
				Sentry.CaptureBobsBuddyException(ex3, _input, _turn, _game.IsBattlegroundsDuosMatch);
			}
			Output = null;
			return null;
		}
		static bool IsLeapfroggerCombo(IEnumerable<Minion>? side)
		{
			if (side == null)
			{
				return false;
			}
			List<Minion> source = side.ToList();
			if (source.Count() >= 3)
			{
				return source.Any((Minion x) => x.CardID == "BG21_000");
			}
			return false;
		}
	}

	public void HandleNewAttackingEntity(Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity newAttacker)
	{
		if (newAttacker.IsHero)
		{
			LastAttackingHero = newAttacker;
			LastAttackingHeroAttack = newAttacker.Attack;
		}
	}

	private int GetLastCombatDamageDealt()
	{
		if (LastAttackingHero != null)
		{
			return LastAttackingHeroAttack;
		}
		return 0;
	}

	private CombatResult GetLastCombatResult()
	{
		if (LastAttackingHero == null)
		{
			return CombatResult.Tie;
		}
		if (LastAttackingHero.IsControlledBy(_game.Player.Id))
		{
			return CombatResult.Win;
		}
		return CombatResult.Loss;
	}

	private LethalResult GetLastLethalResult()
	{
		if (_defendingHero == null || _attackingHero == null)
		{
			return LethalResult.NoOneDied;
		}
		int num = _defendingHero.Health + _defendingHero.GetTag(GameTag.ARMOR);
		if (_attackingHero.Attack >= num)
		{
			if (_attackingHero.IsControlledBy(_game.Player.Id))
			{
				return LethalResult.OpponentDied;
			}
			return LethalResult.FriendlyDied;
		}
		return LethalResult.NoOneDied;
	}

	private async Task ValidateSimulationResultAsync()
	{
		DebugLog("Validating results...", "ValidateSimulationResultAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		if (Output == null)
		{
			DebugLog("Output is null. Exiting", "ValidateSimulationResultAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			return;
		}
		if (DoNotReport)
		{
			DebugLog("Output was invalidated. Exiting", "ValidateSimulationResultAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			return;
		}
		if (Output.simulationCount < 2500)
		{
			DebugLog("Did not complete enough simulations to report terminal cases. Exiting.", "ValidateSimulationResultAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			return;
		}
		double metricSampling = (Remote.Config.Data?.BobsBuddy?.MetricSampling).GetValueOrDefault();
		DebugLog($"metricSampling={metricSampling}, reportErrors={ReportErrors}", "ValidateSimulationResultAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		if (!ReportErrors && metricSampling == 0.0)
		{
			DebugLog("Nothing to report. Exiting.", "ValidateSimulationResultAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			return;
		}
		await Task.Delay(50);
		CombatResult lastCombatResult = GetLastCombatResult();
		LethalResult lastLethalResult = GetLastLethalResult();
		DebugLog($"result={lastCombatResult}, lethalResult={lastLethalResult}", "ValidateSimulationResultAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		if (lastLethalResult == LethalResult.FriendlyDied && (_game.CurrentGameStats?.WasConceded ?? false))
		{
			DebugLog("Game was conceded. Not reporting.", "ValidateSimulationResultAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
			return;
		}
		bool flag = false;
		if (IsIncorrectCombatResult(lastCombatResult))
		{
			flag = true;
			if (ReportErrors && metricSampling > 0.0 && _rnd.NextDouble() < metricSampling)
			{
				AlertWithLastInputOutput(lastCombatResult.ToString());
			}
		}
		if (IsIncorrectLethalResult(lastLethalResult) && !OpposingKelThuzadDied(lastLethalResult))
		{
			if (_turn <= 5)
			{
				DebugLog($"There should not be missed lethals on turn ${_turn}, this is probably a bug. This won't be reported.", "ValidateSimulationResultAsync", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
				return;
			}
			flag = true;
			if (ReportErrors && metricSampling > 0.0 && _rnd.NextDouble() < metricSampling)
			{
				AlertWithLastInputOutput(lastLethalResult.ToString());
			}
		}
		Influx.OnBobsBuddySimulationCompleted(lastCombatResult, Output, _turn, _game.CurrentRegion.ToString(), _input?.Anomaly, flag, _game.IsBattlegroundsDuosMatch, IsOpposingAkazamzarak(), _input?.Opponent?.HeroIsKelThuzad == true);
		if (flag)
		{
			Core.Game.Metrics.IncrementBobsBuddyTerminalCase();
		}
	}

	private bool IsIncorrectCombatResult(CombatResult result)
	{
		if (result == CombatResult.Tie)
		{
			Output? output = Output;
			if (output != null && output.tieRate == 0f)
			{
				goto IL_005d;
			}
		}
		if (result == CombatResult.Win)
		{
			Output? output2 = Output;
			if (output2 != null && output2.winRate == 0f)
			{
				goto IL_005d;
			}
		}
		if (result == CombatResult.Loss)
		{
			Output? output3 = Output;
			if (output3 == null)
			{
				return false;
			}
			return output3.lossRate == 0f;
		}
		return false;
		IL_005d:
		return true;
	}

	private bool IsIncorrectLethalResult(LethalResult result)
	{
		if (result == LethalResult.FriendlyDied)
		{
			Output? output = Output;
			if (output != null && output.myDeathRate == 0f)
			{
				return true;
			}
		}
		if (result == LethalResult.OpponentDied)
		{
			Output? output2 = Output;
			if (output2 == null)
			{
				return false;
			}
			return output2.theirDeathRate == 0f;
		}
		return false;
	}

	private bool OpposingKelThuzadDied(LethalResult result)
	{
		if (result == LethalResult.OpponentDied && _input != null && (_input?.OpponentTeammate?.HeroPowers.Any((HeroPowerData hp) => hp.CardId == "kel'thuzad") == true || _input?.Opponent?.HeroIsKelThuzad == true))
		{
			return true;
		}
		return false;
	}

	private bool IsOpposingAkazamzarak()
	{
		if (_input?.Opponent?.HeroPowers.Any((HeroPowerData hp) => hp.CardId == "TB_BaconShop_HP_020") != true)
		{
			return _input?.OpponentTeammate?.HeroPowers.Any((HeroPowerData hp) => hp.CardId == "TB_BaconShop_HP_020") == true;
		}
		return true;
	}

	private void AlertWithLastInputOutput(string result)
	{
		DebugLog($"Queueing alert... (valid input: {_input != null})", "AlertWithLastInputOutput", "D:\\a\\Hearthstone-Deck-Tracker\\Hearthstone-Deck-Tracker\\Hearthstone Deck Tracker\\BobsBuddy\\BobsBuddyInvoker.cs");
		if (_input != null && Output != null)
		{
			Sentry.QueueBobsBuddyTerminalCase(_input, Output, result, _turn, _game.CurrentRegion, _game.IsBattlegroundsDuosMatch, IsOpposingAkazamzarak());
		}
	}
}
