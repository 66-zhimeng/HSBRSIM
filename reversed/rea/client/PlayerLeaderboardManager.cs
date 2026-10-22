using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core;
using PegasusGame;
using UnityEngine;

public class PlayerLeaderboardManager : CardTileListDisplay
{
	public enum PlayerTileEvent
	{
		TRIPLE,
		WIN_STREAK,
		TECH_LEVEL,
		RECENT_COMBAT,
		KNOCK_OUT,
		RACES,
		BANANA,
		HERO_BUDDY,
		DOUBLE_HERO_BUDDY,
		QUEST_COMPLETE,
		QUEST_UPDATE,
		TRINKET_UPDATE,
		TRINKET_TAG_UPDATE,
		QUEST_REWARD_TAG_UPDATE
	}

	private readonly PlatformDependentValue<float> SPACE_BETWEEN_TILES = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 0.15f,
		Phone = 0.1f
	};

	private readonly PlatformDependentValue<float> SPACE_BETWEEN_TILES_DUOS = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 0.2f,
		Phone = 0.1f
	};

	private readonly PlatformDependentValue<float> LEADERBOARD_TILE_X_CORRECTION_FOR_PERSPECTIVE_MAX = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = -0.08f,
		Phone = -0.08f
	};

	private readonly PlatformDependentVector3 LEADERBOARD_TILE_SCALE = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(1f, 1f, 1f),
		Phone = new Vector3(1f, 1f, 1f)
	};

	private readonly PlatformDependentVector3 LEADERBOARD_TILE_CARD_SCALE = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(1.75f, 1.75f, 1.75f),
		Phone = new Vector3(1.3125f, 1.3125f, 1.3125f)
	};

	private readonly PlatformDependentVector3 LEADERBOARD_TILE_SCALE_DUOS = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(1f, 1f, 1f),
		Phone = new Vector3(0.75f, 0.75f, 0.75f)
	};

	private readonly PlatformDependentVector3 LEADERBOARD_TILE_CARD_SCALE_DUOS = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(1f, 1f, 1f),
		Phone = new Vector3(1f, 1f, 1f)
	};

	public readonly PlatformDependentValue<float> HIGHEST_MAIN_ACTOR_Z_WORLD = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = -1.5f,
		Phone = -2.5f,
		Tablet = -0.5f
	};

	public readonly PlatformDependentValue<float> LOWEST_MAIN_ACTOR_Z_WORLD = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = -3.4f,
		Phone = -2.5f,
		Tablet = -2.4f
	};

	private readonly string SOLO_TEAM_LEADERBOARD_CONTAINER_PREFAB = "PlayerLeaderboardTeamSolo.prefab:a09e9bf137c83104e8564db2abf1d38b";

	private readonly string DUOS_TEAM_LEADERBOARD_CONTAINER_PREFAB = "PlayerLeaderboardTeamDuo.prefab:69107c598787af14ea63ac8a7f316a4b";

	private static PlayerLeaderboardManager s_instance;

	private bool m_disabled;

	private List<PlayerLeaderboardTeam> m_teams = new List<PlayerLeaderboardTeam>();

	private PlayerLeaderboardCard m_currentlyMousedOverTile;

	private bool m_isMousedOver;

	private bool m_isNewMouseOver = true;

	private List<int> m_addedTileForPlayerId = new List<int>();

	private List<int> m_addedContainerForTeamId = new List<int>();

	private Map<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>> m_combatHistory = new Map<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>>();

	private Map<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>> m_incomingHistory = new Map<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>>();

	private Map<int, PlayerRealTimeBattlefieldRaces> m_pendingRaceCountUpdates = new Map<int, PlayerRealTimeBattlefieldRaces>();

	private Entity m_oddManOutOpponentHero;

	private const int NULL_PLAYER = 0;

	private bool m_allowFakePlayerTiles;

	private bool m_isLeaderboardDamageCapFXActive = true;

	private bool m_wasLeaderboardDamageCapFXActive;

	private Dictionary<int, int> m_currentWinStreak = new Dictionary<int, int>();

	private Dictionary<int, int> m_currentLoseStreak = new Dictionary<int, int>();

	private static HistoryTileInitInfo CreateHistoryTileInitInfo(Entity entity)
	{
		HistoryTileInitInfo historyTileInitInfo = new HistoryTileInitInfo();
		historyTileInitInfo.m_entity = entity;
		historyTileInitInfo.m_cardDef = entity.ShareDisposableCardDef();
		using (historyTileInitInfo.m_cardDef)
		{
			if (historyTileInitInfo.m_cardDef?.CardDef != null)
			{
				TAG_PREMIUM premiumType = entity.GetPremiumType();
				AssetHandle.Take(ref historyTileInitInfo.m_portraitTextureHandle, historyTileInitInfo.m_cardDef.CardDef.GetPortraitTexture(premiumType, fallbackToLowerPremium: true));
				historyTileInitInfo.m_portraitTexture = historyTileInitInfo.m_portraitTextureHandle;
				AssetHandle.Take(ref historyTileInitInfo.m_portraitGoldenMaterialHandle, historyTileInitInfo.m_cardDef.CardDef.GetPortraitMaterial(TAG_PREMIUM.GOLDEN));
				if ((bool)historyTileInitInfo.m_portraitGoldenMaterialHandle && historyTileInitInfo.m_portraitGoldenMaterialHandle.Asset.mainTexture == null)
				{
					historyTileInitInfo.m_portraitGoldenMaterialHandle.Asset.mainTexture = historyTileInitInfo.m_portraitTexture;
				}
				historyTileInitInfo.m_portraitGoldenMaterial = historyTileInitInfo.m_portraitGoldenMaterialHandle;
				if (!string.IsNullOrEmpty(historyTileInitInfo.m_cardDef.CardDef.m_LeaderboardTileFullPortraitPath))
				{
					AssetHandle.Take(ref historyTileInitInfo.m_fullTileMaterialHandle, historyTileInitInfo.m_cardDef.CardDef.GetLeaderboardTileFullPortrait());
					if ((bool)historyTileInitInfo.m_fullTileMaterialHandle && historyTileInitInfo.m_fullTileMaterialHandle.Asset.mainTexture == null)
					{
						historyTileInitInfo.m_fullTileMaterialHandle.Asset.mainTexture = historyTileInitInfo.m_portraitTexture;
					}
				}
				else
				{
					historyTileInitInfo.m_cardDef.CardDef.TryGetHistoryTileFullPortrait(premiumType, out var fullHistoryTileMat);
					if ((bool)fullHistoryTileMat && fullHistoryTileMat.Asset.mainTexture == null)
					{
						fullHistoryTileMat.Asset.mainTexture = historyTileInitInfo.m_portraitTexture;
					}
					AssetHandle.Take(ref historyTileInitInfo.m_fullTileMaterialHandle, fullHistoryTileMat);
				}
				historyTileInitInfo.m_cardDef.CardDef.TryGetHistoryTileHalfPortrait(premiumType, out var halfHistoryTileMat);
				if ((bool)halfHistoryTileMat && halfHistoryTileMat.Asset.mainTexture == null)
				{
					halfHistoryTileMat.Asset.mainTexture = historyTileInitInfo.m_portraitTexture;
				}
				AssetHandle.Take(ref historyTileInitInfo.m_halfTileMaterialHandle, halfHistoryTileMat);
			}
			return historyTileInitInfo;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		s_instance = this;
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + 0.15f, base.transform.position.z);
		SetEnabled(enabled: false);
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			Debug.LogWarning("PlayerLeaderboardManager.Awake() - GameState was not Initialized. Initializing now...");
			gameState = GameState.Initialize();
		}
		gameState.RegisterTurnChangedListener(OnTurnChanged);
		gameState.RegisterCreateGameListener(OnCreateGame);
		gameState.RegisterDamageCapChangedListener(OnDamageCapChanged);
		gameState.RegisterDiabloFightPlayerIDChangedListener(OnDiabloFightPlayerIDChanged);
	}

	protected override void OnDestroy()
	{
		s_instance = null;
		if (GameState.Get() != null)
		{
			GameState.Get().UnregisterTurnChangedListener(OnTurnChanged);
			GameState.Get().UnregisterCreateGameListener(OnCreateGame);
			GameState.Get().UnregisterDamageCapChangedListener(OnDamageCapChanged);
			GameState.Get().UnregisterDamageCapChangedListener(OnDamageCapChanged);
		}
		base.OnDestroy();
	}

	public static PlayerLeaderboardManager Get()
	{
		return s_instance;
	}

	public void SetEnabled(bool enabled)
	{
		m_disabled = !enabled;
		GetComponent<Collider>().enabled = enabled;
	}

	public bool IsEnabled()
	{
		return !m_disabled;
	}

	public void SetAllowFakePlayers(bool enabled)
	{
		m_allowFakePlayerTiles = enabled;
	}

	public void CreatePlayerTile(Entity playerHero)
	{
		if (m_disabled)
		{
			return;
		}
		int playerHeroId = playerHero.GetTag(GAME_TAG.PLAYER_ID);
		if (playerHeroId == 0)
		{
			playerHeroId = playerHero.GetTag(GAME_TAG.CONTROLLER);
		}
		if (!GameState.Get().GetPlayerInfoMap().ContainsKey(playerHeroId))
		{
			if (!m_allowFakePlayerTiles)
			{
				Log.Gameplay.PrintError($"PlayerLeaderboardManager.CreatePlayerTile() - Attempt to add player id {playerHeroId} to leaderboard, but that is not a valid id.");
				return;
			}
			SharedPlayerInfo sharedPlayerInfo = new SharedPlayerInfo();
			sharedPlayerInfo.SetPlayerId(playerHeroId);
			GameState.Get().AddPlayerInfo(sharedPlayerInfo);
		}
		if (m_addedTileForPlayerId.Any((int t) => t == playerHeroId))
		{
			return;
		}
		m_addedTileForPlayerId.Add(playerHeroId);
		int playerTeamId = 0;
		string text = "";
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			playerTeamId = playerHero.GetTag(GAME_TAG.BACON_DUO_TEAM_ID);
			if (playerTeamId == 0)
			{
				playerTeamId = playerHero.GetController().GetTag(GAME_TAG.BACON_DUO_TEAM_ID);
			}
			text = DUOS_TEAM_LEADERBOARD_CONTAINER_PREFAB;
		}
		else
		{
			playerTeamId = playerHero.GetTag(GAME_TAG.PLAYER_ID);
			text = SOLO_TEAM_LEADERBOARD_CONTAINER_PREFAB;
		}
		if (!m_addedContainerForTeamId.Any((int t) => t == playerTeamId))
		{
			m_addedContainerForTeamId.Add(playerTeamId);
			AssetLoader.Get().InstantiatePrefab(text, TeamContainerLoadedCallback, playerHero, AssetLoadingOptions.IgnorePrefabPosition);
		}
		else
		{
			StartCoroutine(AddPlayerToTeamWhenTeamIsReady(playerTeamId, playerHero));
		}
	}

	private IEnumerator AddPlayerToTeamWhenTeamIsReady(int teamId, Entity playerHero)
	{
		if (playerHero != null)
		{
			while (playerHero.GetLoadState() == Entity.LoadState.LOADING)
			{
				yield return null;
			}
			PlayerLeaderboardTeam teamForTeamId = GetTeamForTeamId(teamId);
			while (teamForTeamId == null)
			{
				yield return null;
				teamForTeamId = GetTeamForTeamId(teamId);
			}
			HistoryTileInitInfo member = CreateHistoryTileInitInfo(playerHero);
			teamForTeamId.AddMember(member);
		}
	}

	public void UpdatePlayerTileHeroPower(Entity hero, int newHeroPowerId)
	{
		int playerId = hero.GetTag(GAME_TAG.PLAYER_ID);
		PlayerLeaderboardCard tileForPlayerId = GetTileForPlayerId(playerId);
		if (tileForPlayerId != null)
		{
			tileForPlayerId.SetHeroPower(hero);
		}
	}

	public void SetPlayerTileAdditionalHeroPowerDirty(Entity hero)
	{
		int playerId = hero.GetTag(GAME_TAG.PLAYER_ID);
		PlayerLeaderboardCard tileForPlayerId = GetTileForPlayerId(playerId);
		if (tileForPlayerId != null)
		{
			tileForPlayerId.SetAdditionalHeroPowersDirty();
		}
	}

	public List<PlayerLeaderboardCard> GetAllTiles()
	{
		List<PlayerLeaderboardCard> list = new List<PlayerLeaderboardCard>();
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			foreach (PlayerLeaderboardCard member in team.Members)
			{
				list.Add(member);
			}
		}
		return list;
	}

	public void NotifyBattlegroundHeroBuddyEnabledDirty()
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.SetBattlegroundHeroBuddyEnabledDirty();
		}
	}

	public void NotifyBattlegroundsQuestRewardEnabledDirty()
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.SetBGQuestRewardDirty();
		}
	}

	public void NotifyBattlegroundsTrinketEnabledDirty()
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.SetBGTrinketDirty();
		}
	}

	public void NotifyPlayerTileEvent(int playerId, PlayerTileEvent tileEvent, bool showEmote = true)
	{
		if (!m_addedTileForPlayerId.Contains(playerId))
		{
			return;
		}
		EmoteType emoteType = EmoteType.INVALID;
		PlayerLeaderboardCard tileForPlayerId = GetTileForPlayerId(playerId);
		switch (tileEvent)
		{
		default:
			return;
		case PlayerTileEvent.TECH_LEVEL:
		{
			int value = 1;
			if (GameState.Get().GetPlayerInfoMap().ContainsKey(playerId) && GameState.Get().GetPlayerInfoMap()[playerId].GetPlayerHero() != null)
			{
				value = GameState.Get().GetPlayerInfoMap()[playerId].GetPlayerHero().GetRealTimePlayerTechLevel();
			}
			value = Mathf.Clamp(value, 1, 7);
			if (tileForPlayerId != null)
			{
				tileForPlayerId.SetTechLevelDirty();
			}
			switch (value)
			{
			default:
				return;
			case 2:
				emoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_02;
				break;
			case 3:
				emoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_03;
				break;
			case 4:
				emoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_04;
				break;
			case 5:
				emoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_05;
				break;
			case 6:
				emoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_06;
				break;
			case 7:
				emoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_07;
				break;
			}
			break;
		}
		case PlayerTileEvent.TRIPLE:
			if (tileForPlayerId != null)
			{
				tileForPlayerId.SetTriplesDirty();
			}
			emoteType = EmoteType.BATTLEGROUNDS_VISUAL_TRIPLE;
			break;
		case PlayerTileEvent.WIN_STREAK:
			emoteType = EmoteType.BATTLEGROUNDS_VISUAL_HOT_STREAK;
			break;
		case PlayerTileEvent.BANANA:
			emoteType = EmoteType.BATTLEGROUNDS_VISUAL_BANANA;
			break;
		case PlayerTileEvent.QUEST_COMPLETE:
			emoteType = EmoteType.BATTLEGROUNDS_VISUAL_QUEST_COMPLETE;
			break;
		case PlayerTileEvent.HERO_BUDDY:
			emoteType = EmoteType.BATTLEGROUNDS_VISUAL_HERO_BUDDY;
			break;
		case PlayerTileEvent.DOUBLE_HERO_BUDDY:
			emoteType = EmoteType.BATTLEGROUNDS_VISUAL_DOUBLE_HERO_BUDDY;
			break;
		case PlayerTileEvent.KNOCK_OUT:
			return;
		case PlayerTileEvent.RECENT_COMBAT:
			if (tileForPlayerId != null)
			{
				tileForPlayerId.SetRecentCombatsDirty();
			}
			return;
		case PlayerTileEvent.RACES:
			if (tileForPlayerId != null)
			{
				tileForPlayerId.SetRacesDirty();
			}
			return;
		case PlayerTileEvent.QUEST_UPDATE:
			if (tileForPlayerId != null)
			{
				tileForPlayerId.SetBGQuestRewardDirty();
			}
			return;
		case PlayerTileEvent.TRINKET_UPDATE:
			if (tileForPlayerId != null)
			{
				tileForPlayerId.SetBGTrinketDirty();
			}
			return;
		case PlayerTileEvent.TRINKET_TAG_UPDATE:
			if (tileForPlayerId != null)
			{
				tileForPlayerId.UpdateTrinketTags();
			}
			return;
		case PlayerTileEvent.QUEST_REWARD_TAG_UPDATE:
			if (tileForPlayerId != null)
			{
				tileForPlayerId.UpdateQuestRewardTags();
			}
			return;
		}
		if (showEmote)
		{
			GameState.Get().GetGameEntity().PlayAlternateEnemyEmote(playerId, emoteType);
		}
	}

	public void NotifyOfInput(Vector3 hitPoint)
	{
		m_isMousedOver = true;
		if (m_teams.Count == 0)
		{
			CheckForMouseOff();
			return;
		}
		float num = 1000f;
		float num2 = -1000f;
		float num3 = 1000f;
		PlayerLeaderboardCard playerLeaderboardCard = null;
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			if (!allTile.HasBeenShown())
			{
				continue;
			}
			Collider tileCollider = allTile.GetTileCollider();
			if (!(tileCollider == null))
			{
				float num4 = tileCollider.bounds.center.z - tileCollider.bounds.extents.z;
				float num5 = tileCollider.bounds.center.z + tileCollider.bounds.extents.z;
				if (num4 < num)
				{
					num = num4;
				}
				if (num5 > num2)
				{
					num2 = num5;
				}
				if (num4 < hitPoint.z && num5 > hitPoint.z)
				{
					playerLeaderboardCard = allTile;
					break;
				}
				float num6 = Mathf.Abs(hitPoint.z - num4);
				if (num6 < num3)
				{
					num3 = num6;
					playerLeaderboardCard = allTile;
				}
				float num7 = Mathf.Abs(hitPoint.z - num5);
				if (num7 < num3)
				{
					num3 = num7;
					playerLeaderboardCard = allTile;
				}
			}
		}
		if (hitPoint.z < num || hitPoint.z > num2)
		{
			CheckForMouseOff();
			return;
		}
		if (playerLeaderboardCard == null)
		{
			CheckForMouseOff();
			return;
		}
		Collider component = base.gameObject.GetComponent<BoxCollider>();
		Collider tileCollider2 = playerLeaderboardCard.GetTileCollider();
		float num8 = 0f;
		if (playerLeaderboardCard.GetNextOpponentState())
		{
			num8 = playerLeaderboardCard.GetPoppedOutBoneX() * playerLeaderboardCard.m_tileActor.transform.localScale.x;
		}
		if (hitPoint.x < component.bounds.center.x - tileCollider2.bounds.extents.x || hitPoint.x > component.bounds.center.x + tileCollider2.bounds.extents.x + num8)
		{
			CheckForMouseOff();
		}
		else if (!(playerLeaderboardCard == m_currentlyMousedOverTile))
		{
			if (m_currentlyMousedOverTile != null)
			{
				m_currentlyMousedOverTile.NotifyMousedOut();
			}
			else
			{
				FadeVignetteIn();
			}
			m_currentlyMousedOverTile = playerLeaderboardCard;
			playerLeaderboardCard.NotifyMousedOver();
			m_isNewMouseOver = false;
		}
	}

	public void NotifyOfMouseOff()
	{
		CheckForMouseOff();
	}

	public void SetNextOpponent(int opponentPlayerId)
	{
		if (opponentPlayerId == 0)
		{
			return;
		}
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			team.SetNextOpponentState(team.IsPlayerOnTeam(opponentPlayerId), opponentPlayerId);
		}
	}

	public void SetCurrentOpponent(int opponentPlayerId)
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			bool flag = team.IsPlayerOnTeam(opponentPlayerId);
			team.SetSwordsIconActive(flag, opponentPlayerId);
			if (!flag)
			{
				continue;
			}
			foreach (PlayerLeaderboardCard member in team.Members)
			{
				if (GameState.Get().GetPlayerInfoMap().TryGetValue(member.Entity.GetTag(GAME_TAG.PLAYER_ID), out var value) && value != null)
				{
					BnetRecentPlayerMgr.Get().AddRecentPlayer(value.GetBnetPlayer(), BnetRecentPlayerMgr.RecentReason.CURRENT_OPPONENT);
				}
			}
		}
	}

	private void HideAllSwordsDisplay()
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			team.SetSwordsIconActive(active: false, -1);
		}
		foreach (PlayerLeaderboardTeam team2 in m_teams)
		{
			team2.DeactivateDizzyEffects();
		}
	}

	public void UpdateSwordsDisplay()
	{
		if (!GameState.Get().GetGameEntity().IsInBattlegroundsCombatPhase())
		{
			HideAllSwordsDisplay();
			return;
		}
		int num = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_CURRENT_COMBAT_PLAYER_ID);
		int num2 = GameState.Get().GetOpposingSidePlayer().GetTag(GAME_TAG.BACON_CURRENT_COMBAT_PLAYER_ID);
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			bool flag = team.IsPlayerOnTeam(num2);
			bool flag2 = team.IsPlayerOnTeam(num);
			if (flag)
			{
				team.SetSwordsIconActive(flag, num2);
			}
			else if (flag2)
			{
				team.SetSwordsIconActive(flag2, num);
			}
			else
			{
				team.SetSwordsIconActive(active: false, -1);
			}
		}
		int num3 = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.NEXT_OPPONENT_PLAYER_ID);
		int playerId = GameState.Get().GetFriendlySidePlayer().GetPlayerId();
		bool flag3 = GameState.Get().GetFriendlySidePlayer().HasTag(GAME_TAG.BACON_DUO_PLAYER_FIGHTS_FIRST_NEXT_COMBAT);
		bool flag4 = num2 != num3;
		bool flag5 = (num != playerId && flag3) || (num == playerId && !flag3);
		foreach (PlayerLeaderboardTeam team2 in m_teams)
		{
			bool flag6 = team2.IsPlayerOnTeam(num2);
			bool flag7 = team2.IsPlayerOnTeam(num);
			if (flag4 && flag6)
			{
				team2.ActivateDizzyEffectOnPlayerTeammate(num2);
			}
			else if (flag5 && flag7)
			{
				team2.ActivateDizzyEffectOnPlayerTeammate(num);
			}
			else
			{
				team2.DeactivateDizzyEffects();
			}
		}
	}

	public void ApplyEntityReplacement(int playerID, Entity replacementEntity)
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			if (allTile.Entity.GetTag(GAME_TAG.PLAYER_ID) == playerID)
			{
				HistoryTileInitInfo initInfo = CreateHistoryTileInitInfo(replacementEntity);
				allTile.ReplaceHero(replacementEntity, initInfo);
			}
			allTile.RefreshRecentCombats();
		}
		if (m_oddManOutOpponentHero != null && m_oddManOutOpponentHero.GetTag(GAME_TAG.PLAYER_ID) == playerID)
		{
			m_oddManOutOpponentHero = replacementEntity;
		}
	}

	private void OnTurnChanged(int oldTurn, int newTurn, object userdata)
	{
		int num = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.NEXT_OPPONENT_PLAYER_ID);
		if (GameState.Get().GetCurrentPlayer().IsFriendlySide())
		{
			SetNextOpponent(num);
			SetCurrentOpponent(-1);
			ApplyIncomingCombatHistory();
			return;
		}
		SetCurrentOpponent(num);
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.PauseHealthUpdates();
		}
	}

	private void OnDamageCapChanged(int oldValue, int newValue, object userdata)
	{
		StartCoroutine(UpdateDamageCapFX(oldValue, newValue));
	}

	private void OnDiabloFightPlayerIDChanged(int oldValue, int newValue, object userdata)
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.UpdateDiabloPlayerFightFX(oldValue, newValue);
		}
	}

	public void EnableDamageCapFX(bool enable)
	{
		m_isLeaderboardDamageCapFXActive = enable;
		StartCoroutine(UpdateDamageCapFX(-1, -1, forceUpdate: true));
	}

	public void UpdateDamageCap()
	{
		StartCoroutine(UpdateDamageCapFX());
	}

	public IEnumerator UpdateDamageCapFX(int oldValue = -1, int newValue = -1, bool forceUpdate = false)
	{
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		if (GameState.Get() == null || gameEntity == null)
		{
			Debug.Log("[PlayerLeaderboardManager::UpdateDamageCapFX] - Game State/Game Entity is null");
			yield return null;
		}
		bool enabled = GameState.Get().GetGameEntity().HasTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP_ENABLED);
		Spell damageCapSpell = Board.Get().m_leaderboardDamageCapFX;
		if (!m_isLeaderboardDamageCapFXActive)
		{
			if (damageCapSpell != null)
			{
				m_wasLeaderboardDamageCapFXActive = false;
				while (gameEntity.IsInBattlegroundsCombatPhase() || gameEntity.IsStateChangePopupVisible())
				{
					yield return new WaitForSeconds(0.1f);
				}
				SpellUtils.ActivateDeathIfNecessary(damageCapSpell);
			}
			yield return null;
		}
		if (newValue == -1)
		{
			newValue = GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP);
		}
		if (!forceUpdate && oldValue == newValue && m_wasLeaderboardDamageCapFXActive == enabled)
		{
			yield return null;
		}
		if (!(damageCapSpell != null))
		{
			yield break;
		}
		while (gameEntity.IsInBattlegroundsCombatPhase() || gameEntity.IsStateChangePopupVisible())
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (enabled)
		{
			damageCapSpell.gameObject.SetActive(value: true);
			m_wasLeaderboardDamageCapFXActive = true;
			if (!SpellUtils.ActivateBirthIfNecessary(damageCapSpell) && oldValue != newValue && oldValue != -1)
			{
				SpellUtils.ActivateStateIfNecessary(damageCapSpell, SpellStateType.ACTION);
			}
		}
		else
		{
			m_wasLeaderboardDamageCapFXActive = false;
			SpellUtils.ActivateDeathIfNecessary(damageCapSpell);
		}
	}

	private void OnCreateGame(GameState.CreateGamePhase phase, object userData)
	{
		UpdateLayout(animate: false);
		ApplyIncomingCombatHistory(suppressNotifications: true);
		StartCoroutine(UpdateDamageCapFX());
	}

	public bool IsMousedOver()
	{
		return m_isMousedOver;
	}

	public bool IsNewlyMousedOver()
	{
		if (m_isMousedOver)
		{
			return m_isNewMouseOver;
		}
		return false;
	}

	private void CheckForMouseOff()
	{
		if (!m_isMousedOver)
		{
			return;
		}
		m_isMousedOver = false;
		m_isNewMouseOver = true;
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.NotifyMousedOut();
		}
		if (m_currentlyMousedOverTile != null)
		{
			FadeVignetteOut();
		}
		m_currentlyMousedOverTile = null;
	}

	private void FadeVignetteIn()
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			if (!(allTile.m_parent == null))
			{
				LayerUtils.SetLayer(allTile.m_parent.gameObject, GameLayer.Tooltip);
			}
		}
		LayerUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
		AnimateBlurVignetteIn();
	}

	private void FadeVignetteOut()
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			if (!(allTile.m_parent == null))
			{
				LayerUtils.SetLayer(allTile.m_parent.gameObject, GameLayer.Default);
			}
		}
		LayerUtils.SetLayer(base.gameObject, GameLayer.CardRaycast);
		AnimateBlurVignetteOut();
	}

	protected override void OnFullScreenEffectOutFinished()
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			if (!(allTile.m_tileActor == null))
			{
				LayerUtils.SetLayer(allTile.m_tileActor.gameObject, GameLayer.Default);
			}
		}
	}

	private void TeamContainerLoadedCallback(AssetReference assetReference, GameObject go, object callbackData)
	{
		Entity entity = (Entity)callbackData;
		PlayerLeaderboardTeam component = go.GetComponent<PlayerLeaderboardTeam>();
		go.transform.localScale = (GameMgr.Get().IsBattlegroundDuoGame() ? LEADERBOARD_TILE_SCALE_DUOS : LEADERBOARD_TILE_SCALE);
		PlayerLeaderboardCard[] componentsInChildren = go.GetComponentsInChildren<PlayerLeaderboardCard>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.transform.localScale = (GameMgr.Get().IsBattlegroundDuoGame() ? LEADERBOARD_TILE_CARD_SCALE_DUOS : LEADERBOARD_TILE_CARD_SCALE);
		}
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			component.TeamId = entity.GetTag(GAME_TAG.BACON_DUO_TEAM_ID);
			if (component.TeamId == 0)
			{
				component.TeamId = entity.GetController().GetTag(GAME_TAG.BACON_DUO_TEAM_ID);
			}
		}
		else
		{
			component.TeamId = entity.GetTag(GAME_TAG.PLAYER_ID);
		}
		if (GameState.Get().IsMulliganManagerActive())
		{
			component.SetRevealed(revealed: false, isNextOpponent: false);
		}
		m_teams.Add(component);
		StartCoroutine(AddPlayerToTeamWhenTeamIsReady(component.TeamId, entity));
	}

	public PlayerLeaderboardTeam GetTeamForTeamId(int teamId)
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			if (team.TeamId == teamId)
			{
				return team;
			}
		}
		return null;
	}

	public PlayerLeaderboardCard GetTileForPlayerId(int playerId)
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			foreach (PlayerLeaderboardCard member in team.Members)
			{
				if (member != null && member.Entity != null && member.Entity.GetTag(GAME_TAG.PLAYER_ID) == playerId)
				{
					return member;
				}
			}
		}
		return null;
	}

	public Vector3 GetTopTilePosition()
	{
		float num = 0.05f;
		if (m_teams != null && m_teams.Count > 0 && m_teams[0] is PlayerLeaderboardDuosTeam)
		{
			num = 0.05f;
		}
		return new Vector3(base.transform.position.x + num, base.transform.position.y - 0.15f, base.transform.position.z);
	}

	public void UpdatePlayerCombatOrder(bool animate = true)
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			team.UpdatePlayerOrder(animate);
		}
	}

	public void UpdateLayout(bool animate = true)
	{
		SortPlayers();
		UpdateHealthTotals();
		HideAllSwordsDisplay();
		AnimateTeamsToPositions(animate);
		UpdatePlayerCombatOrder(animate);
	}

	public void AnimateTeamsToPositions(bool animate)
	{
		float num = (GameMgr.Get().IsBattlegroundDuoGame() ? SPACE_BETWEEN_TILES_DUOS : SPACE_BETWEEN_TILES);
		float num2 = 0f;
		float num3 = 0f;
		Vector3 topTilePosition = GetTopTilePosition();
		for (int i = 0; i < m_teams.Count; i++)
		{
			PlayerLeaderboardTeam playerLeaderboardTeam = m_teams[i];
			Collider tileCollider = playerLeaderboardTeam.GetTileCollider();
			Vector3 position = new Vector3(topTilePosition.x + num3, topTilePosition.y, topTilePosition.z - num2);
			if (animate)
			{
				iTween.MoveTo(playerLeaderboardTeam.gameObject, position, 1f);
			}
			else
			{
				playerLeaderboardTeam.gameObject.transform.position = position;
			}
			int num4 = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.NEXT_OPPONENT_PLAYER_ID);
			bool isNextOpponent = playerLeaderboardTeam.IsPlayerOnTeam(num4);
			if (!playerLeaderboardTeam.HasBeenShown() && GameState.Get().IsMulliganManagerActive())
			{
				playerLeaderboardTeam.SetRevealed(revealed: true, isNextOpponent);
			}
			playerLeaderboardTeam.MarkAsShown();
			playerLeaderboardTeam.UpdateOddPlayerOutFx(isNextOpponent, num4);
			if (tileCollider != null)
			{
				num2 += tileCollider.bounds.size.z + num;
			}
			num3 += (float)LEADERBOARD_TILE_X_CORRECTION_FOR_PERSPECTIVE_MAX / (float)m_teams.Count;
		}
	}

	public void UpdateRoundHistory(GameRoundHistory gameRoundHistory)
	{
		m_incomingHistory.Clear();
		for (int i = 0; i < gameRoundHistory.Rounds.Count; i++)
		{
			GameRoundHistoryEntry gameRound = gameRoundHistory.Rounds[i];
			AddCombatRound(gameRound);
		}
	}

	public void UpdatePlayerRaces(PlayerRealTimeBattlefieldRaces realTimeBattlefieldRaces)
	{
		PlayerLeaderboardCard tileForPlayerId = GetTileForPlayerId(realTimeBattlefieldRaces.PlayerId);
		if (tileForPlayerId != null)
		{
			tileForPlayerId.UpdateRacesCount(realTimeBattlefieldRaces.Races);
		}
		else if (!m_pendingRaceCountUpdates.ContainsKey(realTimeBattlefieldRaces.PlayerId))
		{
			m_pendingRaceCountUpdates.Add(realTimeBattlefieldRaces.PlayerId, realTimeBattlefieldRaces);
		}
		else
		{
			m_pendingRaceCountUpdates[realTimeBattlefieldRaces.PlayerId] = realTimeBattlefieldRaces;
		}
	}

	private void ApplyIncomingCombatHistory(bool suppressNotifications = false)
	{
		m_combatHistory.Clear();
		m_combatHistory = new Map<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>>(m_incomingHistory);
		int num = 0;
		foreach (KeyValuePair<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>> item in m_combatHistory)
		{
			if (item.Value != null && item.Value.Count != 0)
			{
				num = Math.Max(num, item.Value.Count);
			}
		}
		foreach (KeyValuePair<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>> item2 in m_combatHistory)
		{
			if (item2.Value == null || item2.Value.Count == 0)
			{
				continue;
			}
			PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo recentCombatInfo = default(PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo);
			PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo recentCombatInfo2 = item2.Value.LastOrDefault();
			if (item2.Value.Count > 1)
			{
				recentCombatInfo = item2.Value[item2.Value.Count - 2];
			}
			if (item2.Value.Count == num)
			{
				int num2 = 0;
				if (m_currentWinStreak.ContainsKey(item2.Key))
				{
					num2 = m_currentWinStreak[item2.Key];
				}
				m_currentWinStreak[item2.Key] = recentCombatInfo2.winStreak;
				m_currentLoseStreak[item2.Key] = recentCombatInfo2.loseStreak;
				if (recentCombatInfo2.winStreak > 1 && recentCombatInfo2.winStreak > recentCombatInfo.winStreak && !suppressNotifications && num2 != recentCombatInfo2.winStreak)
				{
					NotifyPlayerTileEvent(item2.Key, PlayerTileEvent.WIN_STREAK);
				}
			}
			NotifyPlayerTileEvent(item2.Key, PlayerTileEvent.RECENT_COMBAT);
		}
	}

	private void AddCombatRound(GameRoundHistoryEntry gameRound)
	{
		Dictionary<int, GameRoundHistoryPlayerEntry> dictionary = gameRound.Combats.ToDictionary((GameRoundHistoryPlayerEntry combat) => combat.PlayerId, (GameRoundHistoryPlayerEntry combat) => combat);
		foreach (KeyValuePair<int, GameRoundHistoryPlayerEntry> item in dictionary)
		{
			int key = item.Key;
			if (key != 0 && (!item.Value.PlayerIsDead || item.Value.PlayerDiedThisRound))
			{
				AddPlayerToCombatHistoryIfNeeded(key);
				GameRoundHistoryPlayerEntry value = item.Value;
				GameRoundHistoryPlayerEntry opponentEntry = dictionary[value.PlayerOpponentId];
				m_incomingHistory[key].Add(ConvertGameRoundHistoryToRecentCombatInfo(value, opponentEntry));
			}
		}
	}

	private PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo ConvertGameRoundHistoryToRecentCombatInfo(GameRoundHistoryPlayerEntry playerEntry, GameRoundHistoryPlayerEntry opponentEntry)
	{
		PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo result = new PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo
		{
			ownerId = playerEntry.PlayerId,
			opponentId = opponentEntry.PlayerId,
			damage = ((playerEntry.PlayerDamageTaken != 0) ? playerEntry.PlayerDamageTaken : opponentEntry.PlayerDamageTaken),
			isDefeated = (playerEntry.PlayerIsDead || opponentEntry.PlayerIsDead),
			friendlyIsDizzy = playerEntry.PlayerIsDizzy,
			enemyIsDizzy = playerEntry.PlayerOpponentIsDizzy,
			ownerTeammateId = playerEntry.PlayerTeammateId,
			opponentTeammateId = playerEntry.PlayerOpponentTeammateId,
			ownerIsFirst = playerEntry.PlayerIsFirst,
			opponentIsFirst = playerEntry.PlayerOpponentIsFirst,
			ownerIsOddManOut = playerEntry.PlayerIsOddManOut,
			opponentIsOddManOut = playerEntry.OpponentIsOddManOut
		};
		if (playerEntry.PlayerDamageTaken != 0)
		{
			result.damageTarget = playerEntry.PlayerId;
		}
		else if (opponentEntry.PlayerDamageTaken != 0)
		{
			result.damageTarget = opponentEntry.PlayerId;
		}
		else
		{
			result.damageTarget = PlayerLeaderboardRecentCombatsPanel.NO_DAMAGE_TARGET;
		}
		if (m_incomingHistory[playerEntry.PlayerId].Count > 0)
		{
			PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo recentCombatInfo = m_incomingHistory[playerEntry.PlayerId].Last();
			result.loseStreak = recentCombatInfo.loseStreak;
			result.winStreak = recentCombatInfo.winStreak;
		}
		if (result.damageTarget == PlayerLeaderboardRecentCombatsPanel.NO_DAMAGE_TARGET)
		{
			return result;
		}
		if (result.damageTarget == playerEntry.PlayerId && (result.damage > 0 || result.isDefeated))
		{
			result.winStreak = 0;
			result.loseStreak++;
		}
		else
		{
			result.winStreak++;
			result.loseStreak = 0;
		}
		return result;
	}

	private void AddPlayerToCombatHistoryIfNeeded(int playerId)
	{
		if (playerId != 0 && !m_incomingHistory.ContainsKey(playerId))
		{
			m_incomingHistory.Add(playerId, new List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>());
		}
	}

	public List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo> GetRecentCombatHistoryForPlayer(int playerId)
	{
		if (m_combatHistory.ContainsKey(playerId))
		{
			return m_combatHistory[playerId];
		}
		return null;
	}

	public int GetLatestWinStreakForPlayer(int playerId)
	{
		if (!m_currentWinStreak.TryGetValue(playerId, out var value))
		{
			return 0;
		}
		return value;
	}

	public int GetLatestLoseStreakForPlayer(int playerId)
	{
		if (!m_currentLoseStreak.TryGetValue(playerId, out var value))
		{
			return 0;
		}
		return value;
	}

	private void SortPlayers()
	{
		m_teams = m_teams.OrderBy((PlayerLeaderboardTeam t) => t.GetBestPlacement()).ToList();
		for (int num = 0; num < m_teams.Count; num++)
		{
			m_teams[num].SetPlaceIcon(num + 1);
		}
	}

	private void UpdateHealthTotals()
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			team.UpdateTileHealth();
		}
	}

	public void SetOddManOutOpponentHero(Entity entity)
	{
		m_oddManOutOpponentHero = entity;
	}

	public Entity GetOddManOutOpponentHero()
	{
		return m_oddManOutOpponentHero;
	}

	public int GetBattlegroundsLeaderboardMaxArmor()
	{
		if (GameState.Get() == null)
		{
			return 0;
		}
		if (GameState.Get().GetGameEntity() == null)
		{
			return 0;
		}
		return GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_MAX_LEADERBOARD_ARMOR);
	}
}
