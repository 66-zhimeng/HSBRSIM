using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.AssetManager;
using Cysharp.Threading.Tasks;
using PegasusGame;
using Unity.Profiling;
using UnityEngine;

public class Entity : EntityBase
{
	public enum LoadState
	{
		INVALID,
		PENDING,
		LOADING,
		DONE
	}

	public class LoadCardData
	{
		public bool updateActor;

		public bool restartStateSpells;

		public bool fromChangeEntity;
	}

	private struct CachedDebugName
	{
		public bool Dirty;

		public string Name;
	}

	public enum RerollButtonEnableResult
	{
		FREE = 0,
		REROLL = 1,
		FREE_UNLOCK = 2,
		UNLOCK = 3,
		ENABLED = 3,
		OUT_OF_CURRENCY = 4,
		HERO_REROLL_LIMITATION_REACHED = 5,
		INSUFFICIENT_MULLIGAN_TIME_LEFT = 6,
		MULLIGAN_NOT_ACTIVE = 7,
		LOCKED = 8
	}

	private class EnchantmentComparer : IEqualityComparer<Entity>
	{
		public bool Equals(Entity a, Entity b)
		{
			if (a == b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			if (a.GetCardId() != b.GetCardId())
			{
				return false;
			}
			string cardTextInHand = a.GetCardTextInHand();
			string cardTextInHand2 = b.GetCardTextInHand();
			return cardTextInHand == cardTextInHand2;
		}

		public int GetHashCode(Entity entity)
		{
			if (entity == null)
			{
				return 0;
			}
			if (entity.GetCardId() == null)
			{
				return 0;
			}
			return entity.GetCardId().GetHashCode();
		}
	}

	private EntityDef m_staticEntityDef = new EntityDef();

	private EntityDef m_dynamicEntityDef;

	protected Card m_card;

	private LoadState m_loadState;

	private int m_cardAssetLoadCount;

	private bool m_useBattlecryPower;

	private bool m_duplicateForHistory;

	private CardTextHistoryData m_cardTextHistoryData;

	private List<Entity> m_attachments = new List<Entity>();

	private List<int> m_subCardIDs = new List<int>();

	private List<int> m_lettuceAbilityEntityIDs = new List<int>();

	private int m_realTimeCost;

	private int m_realTimeAttack;

	private int m_realTimeHealth;

	private int m_realTimeDamage;

	private int m_realTimeArmor;

	private int m_realTimeZone;

	private int m_realTimeZonePosition;

	private int m_realTimeLinkedEntityId;

	private int m_realTimeParentEntityId;

	private bool m_realTimePoweredUp;

	private bool m_realTimeDivineShield;

	private bool m_realTimeIsImmune;

	private bool m_realTimeIsImmuneWhileAttacking;

	private bool m_realTimeIsPoisonous;

	private bool m_realTimeIsVenomous;

	private bool m_realTimeIsDormant;

	private int m_realTimeSpellpower;

	private bool m_realTimeSpellpowerDouble;

	private bool m_realTimeHealingDoesDamageHint;

	private bool m_realTimeLifestealDoesDamageHint;

	private bool m_realTimeCardCostsHealth;

	private bool m_realTimeCardCostsArmor;

	private bool m_realTimeCardCostsCorpses;

	private bool m_realTimeCardCostsAltTavernCoin;

	private bool m_realTimeAttackableByRush;

	private bool m_realTimeBaconCombatPhaseHero;

	private bool m_realTimeBaconDamageCapEnabled;

	private bool m_realTimeBaconCombatPhase;

	private int m_realTimeDeckActionCost;

	private bool m_realTimeIsTitan;

	private bool m_realTimeTitanAbilityUsed1;

	private bool m_realTimeTitanAbilityUsed2;

	private bool m_realTimeTitanAbilityUsed3;

	private bool m_realTimeTitanAbilityUsed4;

	private bool m_realTimeIsOldGod;

	private TAG_CARDTYPE m_realTimeCardType;

	private TAG_PREMIUM m_realTimePremium;

	private int m_realTimePlayerLeaderboardPlace;

	private int m_realTimePlayerTechLevel;

	private int m_realTimePlayerFightsFirst;

	private int m_realTimeBaconDuoPassable;

	private int m_queuedRealTimeControllerTagChangeCount;

	private int m_queuedChangeEntityCount;

	private List<Network.HistChangeEntity> m_transformPowersProcessed = new List<Network.HistChangeEntity>();

	private string m_displayedCreatorName;

	private string m_enchantmentCreatorCardIDForPortrait;

	private CachedDebugName m_cachedDebugName;

	private static ProfilerMarker s_cardInitMarker = new ProfilerMarker("Entity.CardInit");

	public override string ToString()
	{
		return GetDebugName();
	}

	public virtual void OnRealTimeFullEntity(Network.HistFullEntity fullEntity)
	{
		SetTags(fullEntity.Entity.Tags);
		SetTagLists(fullEntity.Entity.TagLists);
		InitRealTimeValues(fullEntity.Entity.Tags);
		InitCard();
		LoadEntityDef(fullEntity.Entity.CardID);
	}

	public async UniTaskVoid OnFullEntity(Network.HistFullEntity fullEntity)
	{
		m_loadState = LoadState.PENDING;
		if (GameMgr.Get()?.IsBattlegrounds() ?? false)
		{
			LoadCard(fullEntity.Entity.CardID);
			await UniTask.NextFrame();
		}
		else
		{
			LoadCard(fullEntity.Entity.CardID, null, async: true);
			await UniTask.WaitUntil(() => !IsLoadingAssets());
		}
		if (GameMgr.Get() == null)
		{
			string text = $"Entity.OnFullEntity() - ChoiceCardMgr is null.";
			TelemetryManager.Client().SendLiveIssue("Entity_OnFullEntity", text);
			Log.Power.Print(text);
		}
		int tag = GetTag(GAME_TAG.ATTACHED);
		if (tag != 0)
		{
			Entity entity = GameState.Get()?.GetEntity(tag);
			if (entity != null)
			{
				entity.AddAttachment(this);
			}
			else
			{
				Log.Gameplay.PrintError("Unable to find attached id id={0}", tag);
			}
		}
		int tag2 = GetTag(GAME_TAG.PARENT_CARD);
		if (tag2 != 0)
		{
			Entity entity2 = GameState.Get()?.GetEntity(tag2);
			if (entity2 != null)
			{
				entity2.AddSubCard(this);
			}
			else
			{
				Log.Gameplay.PrintError("Unable to find parent entity id={0}", tag2);
			}
		}
		int tag3 = GetTag(GAME_TAG.LETTUCE_ABILITY_OWNER);
		if (tag3 != 0 && IsLettuceAbility())
		{
			(GameState.Get()?.GetEntity(tag3))?.AddLettuceAbilityEntityID(GetEntityId());
		}
		if (GetZone() == TAG_ZONE.PLAY)
		{
			Player controller = GetController();
			if (controller != null)
			{
				if (IsHero())
				{
					controller.SetHero(this);
					if (HasTag(GAME_TAG.BACON_IS_KEL_THUZAD))
					{
						PlayerLeaderboardManager.Get().SetOddManOutOpponentHero(this);
					}
				}
				else if (IsHeroPower() && !HasTag(GAME_TAG.ADDITIONAL_HERO_POWER_INDEX))
				{
					controller.SetHeroPower(this);
				}
			}
		}
		if (fullEntity.Entity.DefTags.Count > 0)
		{
			EntityDef orCreateDynamicDefinition = GetOrCreateDynamicDefinition();
			for (int num = 0; num < fullEntity.Entity.DefTags.Count; num++)
			{
				orCreateDynamicDefinition.SetTag(fullEntity.Entity.DefTags[num].Name, fullEntity.Entity.DefTags[num].Value);
			}
		}
		if (HasTag(GAME_TAG.DISPLAYED_CREATOR))
		{
			SetDisplayedCreatorName(GetTag(GAME_TAG.DISPLAYED_CREATOR));
		}
		if (HasTag(GAME_TAG.CREATOR_DBID))
		{
			ResolveEnchantmentPortraitCardID(GetTag(GAME_TAG.CREATOR_DBID));
		}
		if (HasTag(GAME_TAG.PLAYER_LEADERBOARD_PLACE) && GetRealTimeZone() != TAG_ZONE.GRAVEYARD && !HasTag(GAME_TAG.WAS_DISCOVER_OPTION))
		{
			PlayerLeaderboardManager.Get().CreatePlayerTile(this);
			int tag4 = GetTag(GAME_TAG.PLAYER_ID);
			if (GameState.Get() != null && GameState.Get().GetPlayerInfoMap().ContainsKey(tag4))
			{
				GameState.Get().GetPlayerInfoMap()[tag4].SetPlayerHero(this);
			}
			if (HasTag(GAME_TAG.REPLACEMENT_ENTITY))
			{
				PlayerLeaderboardManager.Get().ApplyEntityReplacement(tag4, this);
			}
		}
		if (GetZone() == TAG_ZONE.COSMETIC && IsPet())
		{
			GetController()?.SetPet(this);
		}
	}

	public virtual void OnRealTimeShowEntity(Network.HistShowEntity showEntity)
	{
		HandleRealTimeEntityChange(showEntity.Entity);
	}

	public void OnShowEntity(Network.HistShowEntity showEntity)
	{
		HandleEntityChange(showEntity.Entity, new LoadCardData
		{
			updateActor = false,
			restartStateSpells = false,
			fromChangeEntity = false
		}, fromShowEntity: true);
	}

	public void OnHideEntity(Network.HistHideEntity hideEntity)
	{
		SetTagAndHandleChange(GAME_TAG.ZONE, hideEntity.Zone);
		EntityDef entityDef = GetEntityDef();
		SetTag(GAME_TAG.ATK, entityDef.GetATK());
		SetTag(GAME_TAG.HEALTH, entityDef.GetHealth());
		SetTag(GAME_TAG.COST, entityDef.GetCost());
		SetTag(GAME_TAG.DAMAGE, 0);
		SetCardId(null);
	}

	public virtual void OnRealTimeChangeEntity(List<Network.PowerHistory> powerList, int index, Network.HistChangeEntity changeEntity)
	{
		m_queuedChangeEntityCount++;
		HandleRealTimeEntityChange(changeEntity.Entity);
		CheckRealTimeTransform(powerList, index, changeEntity);
	}

	public void OnChangeEntity(Network.HistChangeEntity changeEntity)
	{
		if (m_transformPowersProcessed.Contains(changeEntity))
		{
			m_transformPowersProcessed.Remove(changeEntity);
			return;
		}
		m_subCardIDs.Clear();
		m_queuedChangeEntityCount--;
		LoadCardData data = new LoadCardData
		{
			updateActor = ShouldUpdateActorOnChangeEntity(changeEntity),
			restartStateSpells = ShouldRestartStateSpellsOnChangeEntity(changeEntity),
			fromChangeEntity = true
		};
		HandleEntityChange(changeEntity.Entity, data, fromShowEntity: false);
	}

	private bool IsTagChanged(Network.HistChangeEntity changeEntity, GAME_TAG tag)
	{
		Network.Entity.Tag tag2 = changeEntity.Entity.Tags.Find((Network.Entity.Tag currTag) => currTag.Name == (int)tag);
		if (tag2 != null)
		{
			return GetTag(tag) != tag2.Value;
		}
		return false;
	}

	private bool SignatureFrameWillChange(string cardId, string changeId)
	{
		if (string.IsNullOrEmpty(cardId) || string.IsNullOrEmpty(changeId))
		{
			return false;
		}
		if (GetTag<TAG_PREMIUM>(GAME_TAG.PREMIUM) == TAG_PREMIUM.SIGNATURE)
		{
			return ActorNames.GetSignatureFrameId(cardId) != ActorNames.GetSignatureFrameId(changeId);
		}
		return false;
	}

	private bool ShouldUpdateActorOnChangeEntity(Network.HistChangeEntity changeEntity)
	{
		if (!IsTagChanged(changeEntity, GAME_TAG.CARDTYPE) && GetTag(GAME_TAG.CARDTYPE) == (int)m_realTimeCardType && !IsTagChanged(changeEntity, GAME_TAG.PREMIUM) && GetTag(GAME_TAG.PREMIUM) == (int)m_realTimePremium && !IsTagChanged(changeEntity, GAME_TAG.LETTUCE_MERCENARY))
		{
			return SignatureFrameWillChange(base.m_cardId, changeEntity.Entity.CardID);
		}
		return true;
	}

	private bool ShouldRestartStateSpellsOnChangeEntity(Network.HistChangeEntity changeEntity)
	{
		if (GameState.Get() != null && GameState.Get().GetGameEntity().HasTag(GAME_TAG.BACON_COIN_ON_ENEMY_MINIONS) && !IsControlledByFriendlySidePlayer() && IsMinion() && GetZone() == TAG_ZONE.PLAY)
		{
			return true;
		}
		return IsTagChanged(changeEntity, GAME_TAG.ELITE);
	}

	public virtual void OnRealTimeTagChanged(Network.HistTagChange change)
	{
		switch ((GAME_TAG)change.Tag)
		{
		case GAME_TAG.COST:
			SetRealTimeCost(change.Value);
			break;
		case GAME_TAG.ATK:
			SetRealTimeAttack(change.Value);
			break;
		case GAME_TAG.HEALTH:
			SetRealTimeHealth(change.Value);
			break;
		case GAME_TAG.DAMAGE:
			SetRealTimeDamage(change.Value);
			break;
		case GAME_TAG.ARMOR:
			SetRealTimeArmor(change.Value);
			break;
		case GAME_TAG.ZONE:
			SetRealTimeZone(change.Value);
			break;
		case GAME_TAG.ZONE_POSITION:
			SetRealTimeZonePosition(change.Value);
			ZoneMgr.Get().OnRealTimeZonePosChange(this);
			break;
		case GAME_TAG.POWERED_UP:
			SetRealTimePoweredUp(change.Value);
			break;
		case GAME_TAG.LINKED_ENTITY:
			SetRealTimeLinkedEntityId(change.Value);
			break;
		case GAME_TAG.PARENT_CARD:
			SetRealTimeParentEntityId(change.Value);
			break;
		case GAME_TAG.DIVINE_SHIELD:
			SetRealTimeDivineShield(change.Value);
			break;
		case GAME_TAG.IMMUNE:
			SetRealTimeIsImmune(change.Value);
			break;
		case GAME_TAG.IMMUNE_WHILE_ATTACKING:
			SetRealTimeIsImmuneWhileAttacking(change.Value);
			break;
		case GAME_TAG.POISONOUS:
		case GAME_TAG.NON_KEYWORD_POISONOUS:
			SetRealTimeIsPoisonous(change.Value);
			break;
		case GAME_TAG.VENOMOUS:
			SetRealTimeIsVenomous(change.Value);
			break;
		case GAME_TAG.SPELLPOWER:
			SetRealTimeHasSpellpower(change.Value);
			break;
		case GAME_TAG.SPELLPOWER_DOUBLE:
			SetRealTimeSpellpowerDouble(change.Value);
			break;
		case GAME_TAG.HEALING_DOES_DAMAGE_HINT:
			SetRealTimeHealingDoesDamageHint(change.Value);
			break;
		case GAME_TAG.LIFESTEAL_DOES_DAMAGE_HINT:
			SetRealTimeLifestealDoesDamageHint(change.Value);
			break;
		case GAME_TAG.DORMANT:
			SetRealTimeIsDormant(change.Value);
			break;
		case GAME_TAG.CARDTYPE:
			SetRealTimeCardType((TAG_CARDTYPE)change.Value);
			break;
		case GAME_TAG.CARD_ALTERNATE_COST:
		{
			TAG_CARD_ALTERNATE_COST value = (TAG_CARD_ALTERNATE_COST)change.Value;
			SetRealTimeCardCostsHealth(value == TAG_CARD_ALTERNATE_COST.HEALTH || value == TAG_CARD_ALTERNATE_COST.OPPONENTS_HEALTH);
			SetRealTimeCardCostsArmor(value == TAG_CARD_ALTERNATE_COST.ARMOR);
			SetRealTimeCardCostsCorpses(value == TAG_CARD_ALTERNATE_COST.CORPSES);
			SetRealTimeCardCostsAltTavernCoin(value == TAG_CARD_ALTERNATE_COST.ALT_TAVERN_COIN);
			break;
		}
		case GAME_TAG.ATTACKABLE_BY_RUSH:
			SetRealTimeAttackableByRush(change.Value);
			break;
		case GAME_TAG.PUZZLE_COMPLETED:
			OnRealTimePuzzleCompleted(change.Value);
			break;
		case GAME_TAG.PREMIUM:
			SetRealTimePremium((TAG_PREMIUM)change.Value);
			break;
		case GAME_TAG.PLAYER_LEADERBOARD_PLACE:
			SetRealTimePlayerLeaderboardPlace(change.Value);
			if (!(GameState.Get().GetGameEntity() is TB_BaconShop_Tutorial) || GetControllerSide() != Player.Side.OPPOSING)
			{
				UpdateSharedPlayer();
			}
			break;
		case GAME_TAG.BACON_DUO_PLAYER_FIGHTS_FIRST_NEXT_COMBAT:
			SetRealTimePlayerFightsFirst(change.Value);
			break;
		case GAME_TAG.PLAYER_TECH_LEVEL:
		{
			int tag = GetTag(GAME_TAG.PLAYER_ID);
			if (tag != 0 && change.Value != 0)
			{
				SetRealTimePlayerTechLevel(change.Value);
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(tag, PlayerLeaderboardManager.PlayerTileEvent.TECH_LEVEL, !GetRealTimeBaconCombatPhaseHero());
			}
			break;
		}
		case GAME_TAG.BACON_DUO_PASSABLE:
			SetRealTimeBaconDuoPassable(change.Value);
			break;
		case GAME_TAG.PLAYER_TRIPLES:
			if (change.Value != 0 && !GetRealTimeBaconCombatPhaseHero())
			{
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.TRIPLE);
			}
			break;
		case GAME_TAG.BACON_PLAYER_NUM_HERO_BUDDIES_GAINED:
			switch (change.Value)
			{
			case 1:
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.HERO_BUDDY);
				break;
			case 2:
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.DOUBLE_HERO_BUDDY);
				break;
			default:
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.DOUBLE_HERO_BUDDY);
				Debug.LogWarning($"Unexpected Number of Hero Buddies gained: {change.Value}");
				break;
			case 0:
				break;
			}
			break;
		case GAME_TAG.BACON_MUKLA_BANANA_SPAWN_COUNT:
			if (!GetRealTimeBaconCombatPhaseHero())
			{
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.BANANA);
			}
			break;
		case GAME_TAG.BACON_QUEST_COMPLETED:
			if (!GetRealTimeBaconCombatPhaseHero())
			{
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.QUEST_COMPLETE);
			}
			break;
		case GAME_TAG.BACON_COMBAT_PHASE_HERO:
			SetRealTimeBaconCombatPhaseHero(change.Value);
			break;
		case GAME_TAG.CONTROLLER:
			m_queuedRealTimeControllerTagChangeCount++;
			break;
		case GAME_TAG.TITAN:
			SetRealTimeIsTitan(change.Value);
			break;
		case GAME_TAG.OLD_GOD:
			SetRealTimeIsOldGod(change.Value);
			break;
		case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_1:
			SetRealTimeTitanAbilityUsed1(change.Value);
			break;
		case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_2:
			SetRealTimeTitanAbilityUsed2(change.Value);
			break;
		case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_3:
			SetRealTimeTitanAbilityUsed3(change.Value);
			break;
		case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_4:
			SetRealTimeTitanAbilityUsed4(change.Value);
			break;
		case GAME_TAG.DECK_ACTION_COST:
			SetRealTimeDeckActionCost(change.Value);
			break;
		}
	}

	private void UpdateSharedPlayer()
	{
		PlayerLeaderboardManager.Get().CreatePlayerTile(this);
		int tag = GetTag(GAME_TAG.PLAYER_ID);
		if (tag == 0)
		{
			tag = GetTag(GAME_TAG.CONTROLLER);
		}
		if (GameState.Get().GetPlayerInfoMap().ContainsKey(tag) && GameState.Get().GetPlayerInfoMap()[tag].GetPlayerHero() == null)
		{
			GameState.Get().GetPlayerInfoMap()[tag].SetPlayerHero(this);
		}
	}

	public void OnRealTimePuzzleCompleted(int newValue)
	{
		if (IsPuzzle() && !(m_card == null) && !(m_card.GetActor() == null))
		{
			PuzzleController component = m_card.GetActor().GetComponent<PuzzleController>();
			if (component == null)
			{
				Log.Gameplay.PrintError("Puzzle card {0} does not have a PuzzleController component.", this);
			}
			else
			{
				component.OnRealTimePuzzleCompleted(newValue);
			}
		}
	}

	public virtual void HandlePreTransformTagChanges(TagDeltaList changeList)
	{
		if (m_card != null)
		{
			m_card.DeactivateCustomKeywordEffect();
		}
	}

	public virtual void OnTagsChanged(TagDeltaList changeList, bool fromShowEntity)
	{
		bool flag = false;
		for (int i = 0; i < changeList.Count; i++)
		{
			TagDelta change = changeList[i];
			if (IsNameChange(change))
			{
				flag = true;
			}
			HandleTagChange(change);
		}
		if (!(m_card == null))
		{
			if (flag)
			{
				UpdateCardName();
			}
			m_card.OnTagsChanged(changeList, fromShowEntity);
		}
	}

	public virtual void OnTagChanged(TagDelta change)
	{
		HandleTagChange(change);
		if (!(m_card == null))
		{
			if (IsNameChange(change))
			{
				UpdateCardName();
			}
			m_card.OnTagChanged(change, fromShowEntity: false);
		}
	}

	public void OnTagListChanged(int tag, List<int> values)
	{
		if (!(m_card == null))
		{
			m_tagLists.SetValues(tag, values);
		}
	}

	public virtual void OnCachedTagForDormantChanged(TagDelta change)
	{
		SetCachedTagForDormant(change.tag, change.newValue);
	}

	protected override void OnUpdateCardId()
	{
		UpdateCardName();
	}

	public virtual void OnMetaData(Network.HistMetaData metaData)
	{
		if (!(m_card == null))
		{
			m_card.OnMetaData(metaData);
		}
	}

	private void HandleRealTimeEntityChange(Network.Entity netEntity)
	{
		InitRealTimeValues(netEntity.Tags);
	}

	private bool HasRealTimeTransformTag(Network.Entity netEntity)
	{
		foreach (Network.Entity.Tag tag in netEntity.Tags)
		{
			if (tag.Name == 859 && tag.Value == 1)
			{
				return true;
			}
		}
		return false;
	}

	private void CheckRealTimeTransform(List<Network.PowerHistory> powerList, int index, Network.HistChangeEntity changeEntity)
	{
		if (HasRealTimeTransformTag(changeEntity.Entity) && CanRealTimeTransform(powerList, index))
		{
			OnChangeEntity(changeEntity);
			m_transformPowersProcessed.Add(changeEntity);
		}
	}

	private bool CanRealTimeTransform(List<Network.PowerHistory> powerList, int index)
	{
		for (int i = 0; i < index; i++)
		{
			Network.PowerHistory power = powerList[i];
			if (!CheckPowerHistoryForRealTimeTransform(power))
			{
				return false;
			}
		}
		foreach (PowerTaskList item in GameState.Get().GetPowerProcessor().GetPowerQueue())
		{
			if (!CheckPowerTaskListForRealTimeTransform(item))
			{
				return false;
			}
		}
		PowerTaskList currentTaskList = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
		if (!CheckPowerTaskListForRealTimeTransform(currentTaskList))
		{
			return false;
		}
		return true;
	}

	private bool CheckPowerHistoryForRealTimeTransform(Network.PowerHistory power)
	{
		switch (power.Type)
		{
		case Network.PowerType.FULL_ENTITY:
			if (((Network.HistFullEntity)power).Entity.ID == GetEntityId())
			{
				return false;
			}
			break;
		case Network.PowerType.SHOW_ENTITY:
			if (((Network.HistShowEntity)power).Entity.ID == GetEntityId())
			{
				return false;
			}
			break;
		case Network.PowerType.HIDE_ENTITY:
			if (((Network.HistHideEntity)power).Entity == GetEntityId())
			{
				return false;
			}
			break;
		case Network.PowerType.TAG_CHANGE:
		{
			Network.HistTagChange histTagChange = (Network.HistTagChange)power;
			if (histTagChange.Entity == GetEntityId() && histTagChange.Tag != 263 && histTagChange.Tag != 385 && histTagChange.Tag != 466)
			{
				return false;
			}
			break;
		}
		case Network.PowerType.CHANGE_ENTITY:
		{
			Network.HistChangeEntity histChangeEntity = (Network.HistChangeEntity)power;
			if (histChangeEntity.Entity.ID == GetEntityId() && !m_transformPowersProcessed.Contains(histChangeEntity))
			{
				return false;
			}
			break;
		}
		case Network.PowerType.META_DATA:
		{
			Network.HistMetaData metaDataEntity = (Network.HistMetaData)power;
			if (!CheckPowerHistoryMetaDataForRealTimeTransform(metaDataEntity))
			{
				return false;
			}
			break;
		}
		}
		return true;
	}

	private bool CheckPowerHistoryMetaDataForRealTimeTransform(Network.HistMetaData metaDataEntity)
	{
		switch (metaDataEntity.MetaType)
		{
		case HistoryMeta.Type.TARGET:
		case HistoryMeta.Type.DAMAGE:
		case HistoryMeta.Type.HEALING:
		case HistoryMeta.Type.JOUST:
		case HistoryMeta.Type.HISTORY_TARGET:
			foreach (int item in metaDataEntity.Info)
			{
				if (item == GetEntityId())
				{
					return false;
				}
			}
			break;
		case HistoryMeta.Type.SHOW_BIG_CARD:
		case HistoryMeta.Type.EFFECT_TIMING:
		case HistoryMeta.Type.OVERRIDE_HISTORY:
		case HistoryMeta.Type.HISTORY_TARGET_DONT_DUPLICATE_UNTIL_END:
		case HistoryMeta.Type.BEGIN_ARTIFICIAL_HISTORY_TILE:
		case HistoryMeta.Type.BEGIN_ARTIFICIAL_HISTORY_TRIGGER_TILE:
		case HistoryMeta.Type.BURNED_CARD:
			if (metaDataEntity.Info.Count > 0 && metaDataEntity.Info[0] == GetEntityId())
			{
				return false;
			}
			break;
		}
		return true;
	}

	private bool CheckPowerTaskListForRealTimeTransform(PowerTaskList powerTaskList)
	{
		if (powerTaskList == null)
		{
			return true;
		}
		foreach (PowerTask task in powerTaskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (!task.IsCompleted() && !CheckPowerHistoryForRealTimeTransform(power))
			{
				return false;
			}
		}
		return true;
	}

	private void HandleEntityChange(Network.Entity netEntity, LoadCardData data, bool fromShowEntity)
	{
		TagDeltaList changeList = m_tags.CreateDeltas(netEntity.Tags);
		SetTags(netEntity.Tags);
		SetTagLists(netEntity.TagLists);
		HandlePreTransformTagChanges(changeList);
		if (m_card != null)
		{
			m_card.DestroyCardDefAssetsOnEntityChanged();
		}
		LoadCard(netEntity.CardID, data);
		if (GetZone() == TAG_ZONE.HAND && GetCard() != null && GetCard().GetZone() != null)
		{
			if (data.updateActor)
			{
				GetCard().GetZone().UpdateLayout();
			}
			GetCard().UpdateActorState(forceHighlightRefresh: true);
		}
		if (netEntity.DefTags.Count > 0)
		{
			EntityDef orCreateDynamicDefinition = GetOrCreateDynamicDefinition();
			for (int i = 0; i < netEntity.DefTags.Count; i++)
			{
				orCreateDynamicDefinition.SetTag(netEntity.DefTags[i].Name, netEntity.DefTags[i].Value);
			}
		}
		RemoveCachedEntityRaces();
		OnTagsChanged(changeList, fromShowEntity);
		CorpseCounter.UpdateTextAll();
	}

	private void HandleTagChange(TagDelta change)
	{
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.ZONE:
			UpdateUseBattlecryFlag(fromGameState: false);
			if (GameState.Get().IsTurnStartManagerActive() && change.oldValue == 2 && change.newValue == 3)
			{
				PowerTaskList currentTaskList = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
				if (currentTaskList != null && currentTaskList.GetSourceEntity() == GameState.Get().GetFriendlySidePlayer())
				{
					TurnStartManager.Get().NotifyOfCardDrawn(this);
				}
			}
			if (change.newValue == 1)
			{
				if (IsHero())
				{
					GetController().SetHero(this);
					if (HasTag(GAME_TAG.BACON_IS_KEL_THUZAD))
					{
						PlayerLeaderboardManager.Get().SetOddManOutOpponentHero(this);
					}
				}
				else if (IsHeroPower())
				{
					GetController().SetHeroPower(this);
				}
			}
			else if (GetTag(GAME_TAG.DIVINE_SHIELD) > 1)
			{
				Actor actor5 = m_card.GetActor();
				if (actor5 != null)
				{
					actor5.ResetDivineShieldEmpowered();
				}
			}
			if (change.newValue == 4 && IsLettuceAbility())
			{
				GetLettuceAbilityOwner()?.RemoveLettuceAbilityEntityID(GetEntityId());
			}
			CheckZoneChangeForEnchantment(change);
			if (change.newValue == 5)
			{
				GameState.Get().GetGameEntity().QueueEntityForRemoval(this);
			}
			if (change.newValue == 9 && IsPet())
			{
				GetController().SetPet(this);
			}
			break;
		case GAME_TAG.ATTACHED:
			GameState.Get().GetEntity(change.oldValue)?.RemoveAttachment(this);
			GameState.Get().GetEntity(change.newValue)?.AddAttachment(this);
			break;
		case GAME_TAG.PARENT_CARD:
		{
			GameState.Get().GetEntity(change.oldValue)?.RemoveSubCard(this);
			Entity entity2 = GameState.Get().GetEntity(change.newValue);
			if (entity2 != null)
			{
				entity2.AddSubCard(this);
			}
			else if (change.newValue != 0)
			{
				Log.Gameplay.PrintError("Unable to find parent entity id={0}", change.newValue);
			}
			break;
		}
		case GAME_TAG.CONTROLLER:
		{
			Entity parentEntity = GetParentEntity();
			if (parentEntity != null)
			{
				if (GameState.Get().GetFriendlyPlayerId() != change.newValue)
				{
					if (parentEntity.GetZone() != TAG_ZONE.PLAY)
					{
						parentEntity.RemoveSubCard(this);
					}
				}
				else
				{
					parentEntity.AddSubCard(this);
				}
			}
			if (IsHeroPower())
			{
				GetController().SetHeroPower(this);
			}
			m_queuedRealTimeControllerTagChangeCount--;
			break;
		}
		case GAME_TAG.DISPLAYED_CREATOR:
			SetDisplayedCreatorName(change.newValue);
			break;
		case GAME_TAG.CREATOR_DBID:
			ResolveEnchantmentPortraitCardID(change.newValue);
			break;
		case GAME_TAG.HERO_POWER:
		case GAME_TAG.HERO_POWER_ENTITY:
		{
			PlayerLeaderboardManager playerLeaderboardManager = PlayerLeaderboardManager.Get();
			if (playerLeaderboardManager != null && playerLeaderboardManager.IsEnabled() && !GetRealTimeBaconCombatPhaseHero())
			{
				playerLeaderboardManager.UpdatePlayerTileHeroPower(this, change.newValue);
			}
			break;
		}
		case GAME_TAG.ADDITIONAL_HERO_POWER_ENTITY_1:
		{
			PlayerLeaderboardManager playerLeaderboardManager = PlayerLeaderboardManager.Get();
			if (playerLeaderboardManager != null && playerLeaderboardManager.IsEnabled() && !GetRealTimeBaconCombatPhaseHero())
			{
				playerLeaderboardManager.SetPlayerTileAdditionalHeroPowerDirty(this);
			}
			break;
		}
		case GAME_TAG.LETTUCE_ABILITY_OWNER:
		{
			GameState.Get().GetEntity(change.oldValue)?.RemoveLettuceAbilityEntityID(GetEntityId());
			Entity entity4 = GameState.Get().GetEntity(change.newValue);
			if (entity4 != null && IsLettuceAbility())
			{
				entity4.AddLettuceAbilityEntityID(GetEntityId());
			}
			break;
		}
		case GAME_TAG.LETTUCE_SELECTED_ABILITY_QUEUE_ORDER:
			if (GameState.Get().GetGameEntity() is LettuceMissionEntity lettuceMissionEntity)
			{
				lettuceMissionEntity.UpdateAllMercenaryAbilityOrderBubbleText();
			}
			break;
		case GAME_TAG.BACON_HERO_BUDDY_PROGRESS:
		{
			Actor actor2 = TB_BaconShop.GetHeroBuddyCard(IsControlledByFriendlySidePlayer() ? Player.Side.FRIENDLY : Player.Side.OPPOSING)?.GetActor();
			if (!(actor2 != null))
			{
				break;
			}
			HeroBuddyWidgetProgressBar component = actor2.GetComponent<HeroBuddyWidgetProgressBar>();
			if (!(component != null))
			{
				break;
			}
			Entity entity = GetController()?.GetHero();
			int num = 0;
			if (entity != null)
			{
				num = entity.GetTag(GAME_TAG.BACON_PLAYER_NUM_HERO_BUDDIES_GAINED);
			}
			if (component != null)
			{
				int num2 = 100 * num + change.newValue;
				if (num2 > 200)
				{
					num2 = 200;
				}
				component.UpdateProgressBar((float)num2 / 200f);
			}
			break;
		}
		case GAME_TAG.BACON_PLAYER_NUM_HERO_BUDDIES_GAINED:
		{
			Card heroBuddyCard = TB_BaconShop.GetHeroBuddyCard(IsControlledByFriendlySidePlayer() ? Player.Side.FRIENDLY : Player.Side.OPPOSING);
			Actor actor7 = heroBuddyCard?.GetActor();
			if (!(actor7 != null) || !actor7.IsShown())
			{
				break;
			}
			switch (change.newValue)
			{
			case 1:
			{
				SpellUtils.ActivateBirthIfNecessary(actor7.GetLoadedSpell(SpellType.HERO_BUDDY_SINGLE));
				HeroBuddyWidgetCoinBased component2 = actor7.GetComponent<HeroBuddyWidgetCoinBased>();
				if (component2 != null)
				{
					component2.EnterStage2();
					heroBuddyCard.GetEntity()?.SetTagAndHandleChange(GAME_TAG.EXHAUSTED, 0);
				}
				break;
			}
			case 2:
				SpellUtils.ActivateBirthIfNecessary(actor7.GetLoadedSpell(SpellType.HERO_BUDDY_DOUBLE));
				break;
			default:
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.DOUBLE_HERO_BUDDY);
				Debug.LogWarning($"Unexpected Number of Hero Buddies gained: {change.newValue}");
				break;
			case 0:
				break;
			}
			break;
		}
		case GAME_TAG.BACON_HERO_QUEST_REWARD_DATABASE_ID:
		case GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_DATABASE_ID:
		case GAME_TAG.BACON_HERO_QUEST_REWARD_COMPLETED:
		case GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_COMPLETED:
			PlayerLeaderboardManager.Get()?.NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.QUEST_UPDATE);
			break;
		case GAME_TAG.BACON_FIRST_TRINKET_DATABASE_ID:
		case GAME_TAG.BACON_SECOND_TRINKET_DATABASE_ID:
		case GAME_TAG.BACON_HEROPOWER_TRINKET_DATABASE_ID:
			PlayerLeaderboardManager.Get()?.NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.TRINKET_UPDATE);
			break;
		case GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_SDN1:
		case GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_SDN1:
		case GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_SDN1:
		case GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_SDN2:
		case GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_SDN2:
		case GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_SDN2:
		case GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_ALT_TEXT:
		case GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_ALT_TEXT:
		case GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_ALT_TEXT:
		case GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_SDN3:
		case GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_SDN3:
		case GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_SDN3:
			PlayerLeaderboardManager.Get()?.NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.TRINKET_TAG_UPDATE);
			break;
		case GAME_TAG.BACON_HERO_QUEST_REWARD_SDN1:
		case GAME_TAG.BACON_HERO_QUEST_REWARD_SDN2:
		case GAME_TAG.BACON_HERO_QUEST_REWARD_ALT_TEXT:
		case GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_SDN1:
		case GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_SDN2:
		case GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_ALT_TEXT:
			PlayerLeaderboardManager.Get()?.NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.QUEST_REWARD_TAG_UPDATE);
			break;
		case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_1:
		case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_2:
		case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_3:
		case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_4:
		{
			Card card6 = GetCard();
			if (card6 != null)
			{
				Actor actor9 = card6.GetActor();
				if (actor9 != null)
				{
					actor9.UpdateActivatableMinionPips((GAME_TAG)change.tag, change.newValue);
				}
			}
			break;
		}
		case GAME_TAG.ULDIR_QUARANTINE_PIP_BLOOD:
		case GAME_TAG.ULDIR_QUARANTINE_PIP_FROST:
		case GAME_TAG.ULDIR_QUARANTINE_PIP_UNHOLY:
		{
			Card card5 = GetCard();
			if (card5 == null)
			{
				break;
			}
			Actor actor8 = card5.GetActor();
			if (!(actor8 == null))
			{
				Entity entity3 = actor8.GetEntity();
				if (entity3 != null && entity3.HasTag(GAME_TAG.ULDIR_QUARANTINE_TRACKER))
				{
					actor8.UpdateUldirQuarantinePips((GAME_TAG)change.tag, change.newValue);
				}
			}
			break;
		}
		case GAME_TAG.SUPPRESS_IMMOLATE_VISUAL_FOR_OPPONENT:
		{
			Card card4 = GetCard();
			if (card4 != null)
			{
				Actor actor6 = card4.GetActor();
				if (actor6 != null)
				{
					actor6.UpdateImmolating((GAME_TAG)change.tag, change.oldValue, change.newValue);
				}
			}
			break;
		}
		case GAME_TAG.IMMOLATESTAGE:
		{
			Card card3 = GetCard();
			if (card3 != null)
			{
				Actor actor4 = card3.GetActor();
				if (actor4 != null)
				{
					actor4.UpdateImmolating((GAME_TAG)change.tag, change.oldValue, change.newValue);
				}
			}
			break;
		}
		case GAME_TAG.DIVINE_SHIELD:
		{
			Card card2 = GetCard();
			if (card2 != null)
			{
				Actor actor3 = card2.GetActor();
				if (actor3 != null)
				{
					actor3.UpdateDivineShield((GAME_TAG)change.tag, change.oldValue, change.newValue);
				}
			}
			break;
		}
		case GAME_TAG.DIVINE_SHIELD_DAMAGE:
		{
			Card card = GetCard();
			if (card != null)
			{
				Actor actor = card.GetActor();
				if (actor != null)
				{
					actor.UpdateDivineShield((GAME_TAG)change.tag, change.oldValue, change.newValue);
				}
			}
			break;
		}
		}
	}

	private void SetDisplayedCreatorName(int entityID)
	{
		Entity entity = GameState.Get().GetEntity(entityID);
		if (entity == null)
		{
			m_displayedCreatorName = null;
		}
		else if (string.IsNullOrEmpty(entity.m_cardId))
		{
			m_displayedCreatorName = GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY");
		}
		else
		{
			m_displayedCreatorName = entity.GetName();
		}
		foreach (int subCardID in GetSubCardIDs())
		{
			GameState.Get().GetEntity(subCardID).SetDisplayedCreatorName(entityID);
		}
	}

	private bool HasEnchantmentPortrait(string enchantmentPortraitCardID)
	{
		if (string.IsNullOrEmpty(enchantmentPortraitCardID))
		{
			return false;
		}
		using DefLoader.DisposableCardDef disposableCardDef = DefLoader.Get().GetCardDef(enchantmentPortraitCardID);
		if (disposableCardDef == null)
		{
			return false;
		}
		if (DefLoader.Get().GetEntityDef(enchantmentPortraitCardID) == null)
		{
			return false;
		}
		TAG_PREMIUM premium = TAG_PREMIUM.NORMAL;
		if (disposableCardDef.CardDef.TryGetEnchantmentPortrait(out var enchantmentPortraitMat))
		{
			bool num = CheckMaterialIsValid(enchantmentPortraitMat, disposableCardDef.CardDef);
			enchantmentPortraitMat.Dispose();
			if (num)
			{
				return true;
			}
		}
		if (disposableCardDef.CardDef.TryGetHistoryTileFullPortrait(premium, out enchantmentPortraitMat))
		{
			bool num2 = CheckMaterialIsValid(enchantmentPortraitMat, disposableCardDef.CardDef);
			enchantmentPortraitMat.Dispose();
			if (num2)
			{
				return true;
			}
		}
		if (!string.IsNullOrEmpty(disposableCardDef.CardDef.GetPortraitTexturePath(premium, fallbackToLowerPremium: false, out var _)))
		{
			return true;
		}
		return false;
	}

	private bool CheckMaterialIsValid(AssetHandle<Material> materialHandle, CardDef cardDef)
	{
		if (materialHandle == null)
		{
			return false;
		}
		if (materialHandle.Asset.mainTexture != null)
		{
			return true;
		}
		if (!string.IsNullOrEmpty(cardDef.GetPortraitTexturePath(TAG_PREMIUM.NORMAL, fallbackToLowerPremium: true, out var _)))
		{
			return true;
		}
		return false;
	}

	public string GetEnchantmentCreatorCardIDForPortrait()
	{
		return m_enchantmentCreatorCardIDForPortrait;
	}

	public void SetEnchantmentPortraitCardID(int creatorDBID)
	{
		m_enchantmentCreatorCardIDForPortrait = null;
		EntityDef entityDef = DefLoader.Get().GetEntityDef(creatorDBID);
		if (entityDef != null)
		{
			if (!HasEnchantmentPortrait(entityDef.GetCardId()) && entityDef.HasTag(GAME_TAG.FALLBACK_ENCHANTMENT_PORTRAIT_DBID))
			{
				SetEnchantmentPortraitCardID(entityDef.GetTag(GAME_TAG.FALLBACK_ENCHANTMENT_PORTRAIT_DBID));
			}
			else
			{
				m_enchantmentCreatorCardIDForPortrait = entityDef.GetCardId();
			}
		}
	}

	private void ResolveEnchantmentPortraitCardID(int creatorDBID)
	{
		m_enchantmentCreatorCardIDForPortrait = null;
		if (!IsEnchantment() || creatorDBID == 0)
		{
			return;
		}
		EntityDef entityDef = DefLoader.Get()?.GetEntityDef(creatorDBID);
		if (entityDef == null)
		{
			return;
		}
		m_enchantmentCreatorCardIDForPortrait = entityDef.GetCardId();
		Entity creator = GetCreator();
		while (!HasEnchantmentPortrait(m_enchantmentCreatorCardIDForPortrait))
		{
			if (creator == null || (!creator.IsEnchantment() && creator.GetCardType() != TAG_CARDTYPE.INVALID))
			{
				m_enchantmentCreatorCardIDForPortrait = null;
				return;
			}
			entityDef = creator.GetCreatorDef();
			creator = creator.GetCreator();
			if (entityDef == null)
			{
				m_enchantmentCreatorCardIDForPortrait = null;
				return;
			}
			m_enchantmentCreatorCardIDForPortrait = entityDef.GetCardId();
		}
		if (GameState.Get() != null)
		{
			Entity entity = GameState.Get().GetEntity(GetAttached());
			if (entity != null && entity.m_card != null)
			{
				entity.m_card.UpdateTooltip();
			}
		}
	}

	private void CheckZoneChangeForEnchantment(TagDelta change)
	{
		if (change.tag == 49 && IsEnchantment() && change.oldValue != change.newValue && (change.newValue == 5 || change.newValue == 4))
		{
			GameState.Get().GetEntity(GetAttached())?.RemoveAttachment(this);
			if (m_card != null)
			{
				m_card.Destroy();
				m_card = null;
			}
		}
	}

	private bool IsNameChange(TagDelta change)
	{
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.ZONE:
		case GAME_TAG.CONTROLLER:
		case GAME_TAG.ENTITY_ID:
		case GAME_TAG.CARDTYPE:
		case GAME_TAG.ZONE_POSITION:
		case GAME_TAG.OVERRIDECARDNAME:
		case GAME_TAG.CARD_NAME_DATA_1:
			return true;
		default:
			return false;
		}
	}

	public EntityDef GetEntityDef()
	{
		if (m_dynamicEntityDef == null)
		{
			return m_staticEntityDef;
		}
		return m_dynamicEntityDef;
	}

	public EntityDef GetOrCreateDynamicDefinition()
	{
		if (m_dynamicEntityDef == null)
		{
			m_dynamicEntityDef = m_staticEntityDef.Clone();
			m_staticEntityDef = null;
		}
		return m_dynamicEntityDef;
	}

	public Card InitCard()
	{
		using (s_cardInitMarker.Auto())
		{
			GameObject gameObject = AssetLoader.Get().InstantiatePrefab("BaseCard.prefab:465d44bb92c351f48ba03163aa012389");
			m_card = gameObject.GetComponent<Card>();
			m_card.SetEntity(this);
			UpdateCardName();
			return m_card;
		}
	}

	public DefLoader.DisposableCardDef ShareDisposableCardDef()
	{
		if (m_duplicateForHistory)
		{
			return GetCardDefForHistory();
		}
		if (m_card != null)
		{
			return m_card.ShareDisposableCardDef();
		}
		if (!string.IsNullOrEmpty(base.m_cardId))
		{
			return DefLoader.Get().GetCardDef(base.m_cardId);
		}
		return null;
	}

	private DefLoader.DisposableCardDef GetCardDefForHistory()
	{
		if (m_card != null)
		{
			if (IsHidden() && !m_card.HasHiddenCardDef)
			{
				return DefLoader.Get().GetCardDef("HiddenCard");
			}
			if (base.m_cardId == m_card.GetEntity().GetCardId())
			{
				return m_card.ShareDisposableCardDef();
			}
		}
		if (!string.IsNullOrEmpty(base.m_cardId))
		{
			return DefLoader.Get().GetCardDef(base.m_cardId);
		}
		return DefLoader.Get().GetCardDef("HiddenCard");
	}

	public Card GetCard()
	{
		return m_card;
	}

	public void SetCard(Card card)
	{
		m_card = card;
	}

	public void Destroy()
	{
		if (m_card != null)
		{
			m_card.Destroy();
			m_card = null;
		}
	}

	public LoadState GetLoadState()
	{
		return m_loadState;
	}

	public bool IsLoadingAssets()
	{
		return m_loadState == LoadState.LOADING;
	}

	public bool IsBusy()
	{
		if (IsLoadingAssets())
		{
			return true;
		}
		if (m_card != null && !m_card.IsActorReady())
		{
			return true;
		}
		return false;
	}

	public bool IsHidden()
	{
		return string.IsNullOrEmpty(base.m_cardId);
	}

	public bool HasQueuedChangeEntity()
	{
		return m_queuedChangeEntityCount > 0;
	}

	public bool HasQueuedControllerTagChange()
	{
		return m_queuedRealTimeControllerTagChangeCount > 0;
	}

	public void SetTagAndHandleChange<TagEnum>(GAME_TAG tag, TagEnum tagValue)
	{
		SetTagAndHandleChange((int)tag, Convert.ToInt32(tagValue));
	}

	public TagDelta SetTagAndHandleChange(int tag, int tagValue)
	{
		int tag2 = m_tags.GetTag(tag);
		SetTag(tag, tagValue);
		TagDelta tagDelta = new TagDelta();
		tagDelta.tag = tag;
		tagDelta.oldValue = tag2;
		tagDelta.newValue = tagValue;
		OnTagChanged(tagDelta);
		return tagDelta;
	}

	public override int GetReferencedTag(int tag)
	{
		return GetEntityDef().GetReferencedTag(tag);
	}

	public int GetDefCost()
	{
		return GetEntityDef().GetCost();
	}

	public int GetDefATK()
	{
		return GetEntityDef().GetATK();
	}

	public int GetDefHealth()
	{
		return GetEntityDef().GetHealth();
	}

	public bool HasRace(TAG_RACE race)
	{
		if ((HasTag(GAME_TAG.CARDRACE) ? GetTag<TAG_RACE>(GAME_TAG.CARDRACE) : GetEntityDef().GetTag<TAG_RACE>(GAME_TAG.CARDRACE)) == TAG_RACE.ALL && race != TAG_RACE.INVALID)
		{
			return true;
		}
		return GetRaces().Contains(race);
	}

	public override TAG_CLASS GetClass()
	{
		if (IsSecret())
		{
			return base.GetClass();
		}
		return GetEntityDef().GetClass();
	}

	public override void GetClasses(IList<TAG_CLASS> classes)
	{
		base.GetClasses(classes);
		if (classes.Count == 0)
		{
			GetEntityDef().GetClasses(classes);
		}
		if (classes.Count == 0)
		{
			classes.Add(TAG_CLASS.INVALID);
		}
	}

	public TAG_ENCHANTMENT_VISUAL GetEnchantmentBirthVisual()
	{
		return GetEntityDef().GetEnchantmentBirthVisual();
	}

	public TAG_ENCHANTMENT_VISUAL GetEnchantmentIdleVisual()
	{
		return GetEntityDef().GetEnchantmentIdleVisual();
	}

	public string GetCustomEnchantmentBannerText()
	{
		return GameDbf.GetIndex().GetClientString(GetTag(GAME_TAG.ENCHANTMENT_BANNER_TEXT));
	}

	public string GetCustomSideQuestBannerText()
	{
		return GameStrings.Format("GLUE_SIDEQUEST_PROGRESS_BANNER", GetTag(GAME_TAG.QUEST_PROGRESS), GetTag(GAME_TAG.QUEST_PROGRESS_TOTAL));
	}

	public string GetCustomObjectiveBannerText()
	{
		string cardId = GetCardId();
		int num = GetTag(GAME_TAG.QUEST_PROGRESS_TOTAL);
		if (cardId == "ETC_387t")
		{
			switch (GetTag(GAME_TAG.HIDDEN_CHOICE))
			{
			case 1:
				num = 2;
				break;
			case 2:
				num = 4;
				break;
			}
		}
		else if (cardId == "ETC_335")
		{
			if (GetTag(GAME_TAG.QUEST_PROGRESS) > 0)
			{
				return GameStrings.Format("GLUE_OBJECTIVES_BANNER_SPELL_PLAYED");
			}
			return GameStrings.Format("GLUE_OBJECTIVES_BANNER_NO_SPELL_PLAYED");
		}
		if (HasTag(GAME_TAG.QUEST_HIDE_PROGRESS))
		{
			return "";
		}
		string text = "";
		if (num > 0)
		{
			int num2 = num - GetTag(GAME_TAG.QUEST_PROGRESS);
			return (GetEntityDef() != null && GetEntityDef().HasTag(GAME_TAG.BACON_SHOW_REFRESH_LEFT_BANNER)) ? GameStrings.Format("GLUE_OBJECTIVES_BANNER_REFRESH", num2) : GameStrings.Format("GLUE_OBJECTIVES_BANNER", num2);
		}
		return GameStrings.Format("GLUE_OBJECTIVES_BANNER_UNKNOWN");
	}

	public EnchantmentBanner.Icon GetCustomObjectiveBannerIcon()
	{
		if (GetCardId() == "ETC_335")
		{
			if (GetTag(GAME_TAG.QUEST_PROGRESS) > 0)
			{
				return EnchantmentBanner.Icon.Checkmark;
			}
			return EnchantmentBanner.Icon.Xmark;
		}
		return EnchantmentBanner.Icon.Nothing;
	}

	public void ApplyCustomBannerTextOffset(UberText enchantmentBannerText)
	{
		if (!(enchantmentBannerText == null) && GetCardId() == "ETC_335")
		{
			enchantmentBannerText.transform.localScale *= 0.8f;
			if (UniversalInputManager.Get().IsTouchMode())
			{
				enchantmentBannerText.transform.localPosition += new Vector3(0.14f, 0.084f, 0f);
			}
			else if (IsControlledByFriendlySidePlayer())
			{
				enchantmentBannerText.transform.position += new Vector3(0.14f, 0.084f, 0.05f);
			}
			else
			{
				enchantmentBannerText.transform.position += new Vector3(0.14f, 0.084f, 0.08f);
			}
		}
	}

	public TAG_RARITY GetRarity()
	{
		return GetEntityDef().GetRarity();
	}

	public new TAG_CARD_SET GetCardSet()
	{
		return GetEntityDef().GetCardSet();
	}

	public bool CanBeDamagedRealTime()
	{
		if (GetRealTimeDivineShield() || GetRealTimeIsImmune())
		{
			return false;
		}
		if (GetRealTimeIsImmuneWhileAttacking() && (bool)TargetReticleManager.Get() && TargetReticleManager.Get().ArrowSourceEntityID == GetEntityId())
		{
			return false;
		}
		return true;
	}

	public int GetCurrentHealth()
	{
		return GetTag(GAME_TAG.HEALTH) - GetTag(GAME_TAG.DAMAGE) - GetTag(GAME_TAG.PREDAMAGE);
	}

	public int GetCurrentDefense()
	{
		return GetCurrentHealth() + GetArmor();
	}

	public int GetCurrentVitality()
	{
		if (IsCharacter() || IsWeapon() || IsLocation())
		{
			return GetCurrentDefense();
		}
		Error.AddDevFatal("Entity.GetCurrentVitality() should not be called on {0}. This entity is neither a character nor a weapon nor a location.", this);
		return 0;
	}

	public virtual Player GetController()
	{
		return GameState.Get()?.GetPlayer(GetControllerId());
	}

	public Player.Side GetControllerSide()
	{
		return GetController()?.GetSide() ?? Player.Side.NEUTRAL;
	}

	public bool IsControlledByLocalUser()
	{
		Player controller = GetController();
		if (controller != null)
		{
			return controller.IsLocalUser();
		}
		Log.Gameplay.PrintError("Failed to find controller for entity: {0}", ToString());
		return false;
	}

	public bool IsControlledByFriendlySidePlayer()
	{
		Player controller = GetController();
		if (controller != null)
		{
			return controller.IsFriendlySide();
		}
		Log.Gameplay.PrintError("Failed to find controller for entity: {0}", ToString());
		return false;
	}

	public bool IsControlledByOpposingSidePlayer()
	{
		Player controller = GetController();
		if (controller != null)
		{
			return controller.IsOpposingSide();
		}
		Log.Gameplay.PrintError("Failed to find controller for entity: {0}", ToString());
		return false;
	}

	public bool IsControlledByRevealedPlayer()
	{
		Player controller = GetController();
		if (controller != null)
		{
			return controller.IsRevealed();
		}
		Log.Gameplay.PrintError("Failed to find controller for entity: {0}", ToString());
		return false;
	}

	public bool IsControlledByConcealedPlayer()
	{
		return !IsControlledByRevealedPlayer();
	}

	public Entity GetCreator()
	{
		return GameState.Get()?.GetEntity(GetCreatorId());
	}

	public EntityDef GetCreatorDef()
	{
		return DefLoader.Get().GetEntityDef(GetCreatorDBID());
	}

	public string GetDisplayedCreatorName()
	{
		return m_displayedCreatorName;
	}

	public virtual Entity GetHero()
	{
		return GetController()?.GetHero();
	}

	public virtual Card GetHeroCard()
	{
		return GetController()?.GetHeroCard();
	}

	public virtual Entity GetHeroPower()
	{
		if (IsHeroPower())
		{
			return this;
		}
		return GetController()?.GetHeroPower();
	}

	public virtual Card GetHeroPowerCard()
	{
		if (IsHeroPower())
		{
			return GetCard();
		}
		return GetController()?.GetHeroPowerCard();
	}

	public virtual Card GetWeaponCard()
	{
		if (IsWeapon())
		{
			return GetCard();
		}
		return GetController()?.GetWeaponCard();
	}

	public virtual Card GetHeroBuddyCard()
	{
		if (IsBattlegroundHeroBuddy())
		{
			return GetCard();
		}
		return GetController()?.GetHeroBuddyCard();
	}

	public virtual Card GetQuestRewardFromHeroPowerCard()
	{
		if (IsBattlegroundQuestReward())
		{
			return GetCard();
		}
		return GetController()?.GetQuestRewardFromHeroPowerCard();
	}

	public virtual Card GetQuestRewardCard()
	{
		if (IsBattlegroundQuestReward())
		{
			return GetCard();
		}
		return GetController()?.GetQuestRewardCard();
	}

	public virtual List<Card> GetQuestRewardCards()
	{
		if (IsBattlegroundQuestReward())
		{
			return new List<Card> { GetCard() };
		}
		Player controller = GetController();
		if (controller == null)
		{
			return new List<Card>();
		}
		return controller.GetQuestRewardCards();
	}

	public virtual int GetHeroBuddyCardId()
	{
		if (HasTag(GAME_TAG.BACON_SKIN) && HasTag(GAME_TAG.BACON_SKIN_PARENT_ID))
		{
			int tag = GetTag(GAME_TAG.BACON_SKIN_PARENT_ID);
			using DefLoader.DisposableFullDef disposableFullDef = DefLoader.Get().GetFullDef(tag);
			if (disposableFullDef?.EntityDef == null || disposableFullDef?.CardDef == null)
			{
				Log.Gameplay.PrintError("GetHeroBuddyId(): Unable to load def for card ID {0}.", tag);
				return 0;
			}
			return disposableFullDef.EntityDef.GetTag(GAME_TAG.BACON_COMPANION_ID);
		}
		return GetTag(GAME_TAG.BACON_COMPANION_ID);
	}

	public virtual Entity GetPet()
	{
		if (IsPet())
		{
			return this;
		}
		return GetController()?.GetPet();
	}

	public virtual Card GetPetCard()
	{
		if (IsPet())
		{
			return GetCard();
		}
		return GetController()?.GetPetCard();
	}

	public virtual PetController GetPetController()
	{
		Card petCard = GetPetCard();
		if (petCard == null)
		{
			return null;
		}
		Actor actor = petCard.GetActor();
		if (!(actor == null))
		{
			return actor.m_petController;
		}
		return null;
	}

	public virtual bool HasValidDisplayName()
	{
		return GetEntityDef().HasValidDisplayName();
	}

	public virtual string GetName()
	{
		int tag = GetTag(GAME_TAG.OVERRIDECARDNAME);
		EntityDef entityDef;
		if (tag > 0)
		{
			entityDef = DefLoader.Get().GetEntityDef(tag);
			if (entityDef != null)
			{
				return entityDef.GetName();
			}
		}
		entityDef = GetEntityDef();
		if (entityDef != null && entityDef.GetCardTextBuilder() != null)
		{
			return entityDef.GetCardTextBuilder().BuildCardName(this);
		}
		if (!string.IsNullOrEmpty(base.m_cardId))
		{
			Debug.LogWarning($"Entity.GetName: No textbuilder found for {base.m_cardId}, returning default name");
		}
		return CardTextBuilder.GetDefaultCardName(GetEntityDef());
	}

	public virtual string GetDebugName()
	{
		if (m_cachedDebugName.Name == null)
		{
			m_cachedDebugName.Dirty = true;
		}
		if (m_cachedDebugName.Dirty)
		{
			string name = GetEntityDef().GetName();
			if (name != null)
			{
				m_cachedDebugName.Name = $"[entityName={name} id={GetEntityId()} zone={GetZone()} zonePos={GetZonePosition()} cardId={base.m_cardId} player={GetControllerId()}]";
			}
			else if (base.m_cardId != null)
			{
				m_cachedDebugName.Name = $"[id={GetEntityId()} cardId={base.m_cardId} type={GetCardType()} zone={GetZone()} zonePos={GetZonePosition()} player={GetControllerId()}]";
			}
			else
			{
				m_cachedDebugName.Name = $"UNKNOWN ENTITY [id={GetEntityId()} type={GetCardType()} zone={GetZone()} zonePos={GetZonePosition()}]";
			}
			m_cachedDebugName.Dirty = false;
		}
		return m_cachedDebugName.Name;
	}

	public void UpdateCardName()
	{
		m_cachedDebugName.Dirty = true;
		if (m_card == null)
		{
			return;
		}
		string name = GetEntityDef().GetName();
		if (name != null)
		{
			if (string.IsNullOrEmpty(base.m_cardId))
			{
				m_card.gameObject.name = $"{name} [id={GetEntityId()} zone={GetZone()} zonePos={GetZonePosition()}]";
			}
			else
			{
				m_card.gameObject.name = $"{name} [id={GetEntityId()} cardId={GetCardId()} zone={GetZone()} zonePos={GetZonePosition()} player={GetControllerId()}]";
			}
		}
		else
		{
			m_card.gameObject.name = $"Hidden Entity [id={GetEntityId()} zone={GetZone()} zonePos={GetZonePosition()}]";
		}
		if (m_card.GetActor() != null)
		{
			m_card.GetActor().UpdateNameText();
		}
	}

	public string GetCardTextInHand()
	{
		using DefLoader.DisposableCardDef disposableCardDef = ShareDisposableCardDef();
		if (disposableCardDef?.CardDef == null)
		{
			Log.All.PrintError("Entity.GetCardTextInHand(): entity {0} does not have a CardDef", GetEntityId());
			return string.Empty;
		}
		return GetCardTextBuilder().BuildCardTextInHand(this);
	}

	public string GetCardTextInHand(bool useAssembleTextVariant)
	{
		if (!useAssembleTextVariant)
		{
			return GetCardTextInHand();
		}
		using DefLoader.DisposableCardDef disposableCardDef = ShareDisposableCardDef();
		if (disposableCardDef?.CardDef == null)
		{
			Log.All.PrintError("Entity.GetCardTextInHand(): entity {0} does not have a CardDef", GetEntityId());
			return string.Empty;
		}
		return GetCardTextBuilder().BuildCardTextInHand(this, useAssembleTextVariant: true);
	}

	public string GetCardTextInHistory()
	{
		using DefLoader.DisposableCardDef disposableCardDef = ShareDisposableCardDef();
		if (disposableCardDef?.CardDef == null)
		{
			Log.All.PrintError("Entity.GetCardTextInHand(): entity {0} does not have a CardDef", GetEntityId());
			return string.Empty;
		}
		return GetCardTextBuilder().BuildCardTextInHistory(this);
	}

	public string GetTargetingArrowText()
	{
		using DefLoader.DisposableCardDef disposableCardDef = ShareDisposableCardDef();
		if (disposableCardDef?.CardDef == null)
		{
			Log.All.PrintError("Entity.GetTargetingArrowText(): entity {0} does not have a CardDef", GetEntityId());
			return string.Empty;
		}
		return GetCardTextBuilder().GetTargetingArrowText(this);
	}

	public void AddAttachment(Entity entity)
	{
		int count = m_attachments.Count;
		if (m_attachments.Contains(entity))
		{
			Log.Gameplay.Print($"Entity.AddAttachment() - {entity} is already an attachment of {this}");
			return;
		}
		m_attachments.Add(entity);
		if (!(m_card == null))
		{
			m_card.OnEnchantmentAdded(count, entity);
		}
	}

	public void RemoveAttachment(Entity entity)
	{
		int count = m_attachments.Count;
		if (!m_attachments.Remove(entity))
		{
			Log.Gameplay.Print("Entity.RemoveAttachment() - {0} is not an attachment of {1}", entity, this);
		}
		else if (!(m_card == null))
		{
			m_card.OnEnchantmentRemoved(count, entity);
		}
	}

	public bool AddSubCard(Entity entity)
	{
		if (m_subCardIDs.Contains(entity.GetEntityId()))
		{
			return false;
		}
		m_subCardIDs.Add(entity.GetEntityId());
		m_subCardIDs.Sort();
		return true;
	}

	private void RemoveSubCard(Entity entity)
	{
		m_subCardIDs.Remove(entity.GetEntityId());
	}

	private void RemoveLettuceAbilityEntityID(int entityID)
	{
		m_lettuceAbilityEntityIDs.Remove(entityID);
	}

	private void AddLettuceAbilityEntityID(int entityID)
	{
		if (!m_lettuceAbilityEntityIDs.Contains(entityID))
		{
			m_lettuceAbilityEntityIDs.Add(entityID);
		}
	}

	public List<int> GetLettuceAbilityEntityIDs()
	{
		return m_lettuceAbilityEntityIDs;
	}

	public int GetSelectedLettuceAbilityID()
	{
		int tag = GetTag(GAME_TAG.LETTUCE_ABILITY_TILE_VISUAL_SELF_ONLY);
		if (tag > 0)
		{
			return tag;
		}
		return GetTag(GAME_TAG.LETTUCE_ABILITY_TILE_VISUAL_ALL_VISIBLE);
	}

	public List<Entity> GetAttachments()
	{
		return m_attachments;
	}

	public bool DoEnchantmentsHaveTag(GAME_TAG tag)
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasTag(tag))
			{
				return true;
			}
		}
		return false;
	}

	public bool DoEnchantmentsHaveTriggerVisuals()
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasTriggerVisual())
			{
				return true;
			}
		}
		return false;
	}

	public bool DoEnchantmentsHaveOverKill()
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasTag(GAME_TAG.OVERKILL))
			{
				return true;
			}
		}
		return false;
	}

	public bool DoEnchantmentsHaveSpellburst()
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasSpellburst())
			{
				return true;
			}
		}
		return false;
	}

	public bool DoEnchantmentsHaveCounter()
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasCounter())
			{
				return true;
			}
		}
		return false;
	}

	public bool DoEnchantmentsHaveHonorableKill()
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasTag(GAME_TAG.HONORABLE_KILL))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEnchanted()
	{
		return m_attachments.Count > 0;
	}

	public bool IsEnchantment()
	{
		return GetRealTimeCardType() == TAG_CARDTYPE.ENCHANTMENT;
	}

	public bool IsDarkWandererSecret()
	{
		if (IsSecret())
		{
			return GetClass() == TAG_CLASS.WARRIOR;
		}
		return false;
	}

	public bool IsDeathrattleDisabled()
	{
		return HasTag(GAME_TAG.CANT_TRIGGER_DEATHRATTLE);
	}

	public bool IsToBeDestroyed()
	{
		return HasTag(GAME_TAG.TO_BE_DESTROYED);
	}

	public List<Entity> GetEnchantments()
	{
		return GetAttachments();
	}

	public List<Entity> GetDisplayedEnchantments(bool unique = false)
	{
		List<Entity> list = new List<Entity>(GetAttachments());
		list.RemoveAll((Entity enchant) => enchant.HasTag(GAME_TAG.ENCHANTMENT_INVISIBLE));
		if (!unique)
		{
			return list;
		}
		return list.Distinct(new EnchantmentComparer()).ToList();
	}

	public bool HasSubCards()
	{
		if (m_subCardIDs != null)
		{
			return m_subCardIDs.Count > 0;
		}
		return false;
	}

	public List<int> GetSubCardIDs()
	{
		return m_subCardIDs;
	}

	public Entity GetSubCard(int suboption)
	{
		if (m_subCardIDs != null && suboption < m_subCardIDs.Count)
		{
			return GameState.Get()?.GetEntity(m_subCardIDs[suboption]);
		}
		return null;
	}

	public int GetSubCardIndex(Entity entity)
	{
		if (entity == null)
		{
			return -1;
		}
		int entityId = entity.GetEntityId();
		for (int i = 0; i < m_subCardIDs.Count; i++)
		{
			if (m_subCardIDs[i] == entityId)
			{
				return i;
			}
		}
		return -1;
	}

	public Entity GetParentEntity()
	{
		int tag = GetTag(GAME_TAG.PARENT_CARD);
		return GameState.Get()?.GetEntity(tag);
	}

	public CardTextBuilder GetCardTextBuilder()
	{
		if (GetEntityDef() != null && GetEntityDef().GetCardTextBuilder() != null)
		{
			return GetEntityDef().GetCardTextBuilder();
		}
		if (!string.IsNullOrEmpty(base.m_cardId))
		{
			Debug.LogWarning($"Entity.GetCardTextBuilder: No textbuilder found for {base.m_cardId}, returning fallback text builder");
		}
		return CardTextBuilder.GetFallbackCardTextBuilder();
	}

	public Entity CloneForZoneMgr()
	{
		Entity entity = new Entity();
		entity.m_staticEntityDef = GetEntityDef();
		entity.m_dynamicEntityDef = null;
		entity.m_card = m_card;
		entity.m_cardId = base.m_cardId;
		entity.ReplaceTags(m_tags);
		entity.m_loadState = m_loadState;
		entity.m_cachedDebugName.Dirty = true;
		return entity;
	}

	public Entity CloneForHistory(HistoryInfo historyInfo)
	{
		Entity entity = new Entity();
		entity.m_duplicateForHistory = true;
		entity.m_staticEntityDef = GetEntityDef();
		entity.m_dynamicEntityDef = null;
		entity.m_card = m_card;
		entity.m_cardId = base.m_cardId;
		entity.ReplaceTags(m_tags);
		entity.m_cardTextHistoryData = GetCardTextBuilder().CreateCardTextHistoryData();
		entity.m_cardTextHistoryData.SetHistoryData(this, historyInfo);
		entity.m_subCardIDs = m_subCardIDs;
		if (!IsHero())
		{
			entity.SetTag(GAME_TAG.ZONE, TAG_ZONE.HAND);
		}
		entity.m_loadState = m_loadState;
		entity.m_displayedCreatorName = m_displayedCreatorName;
		entity.m_enchantmentCreatorCardIDForPortrait = m_enchantmentCreatorCardIDForPortrait;
		return entity;
	}

	public bool IsHistoryDupe()
	{
		return m_duplicateForHistory;
	}

	public int GetJadeGolem()
	{
		Player controller = GetController();
		if (controller != null)
		{
			return Mathf.Min(controller.GetTag(GAME_TAG.JADE_GOLEM) + 1, 30);
		}
		Log.Gameplay.PrintError("Failed to find controller for entity: {0}", ToString());
		return 1;
	}

	private bool IsEnchantmentAffectedBySpellPower()
	{
		if (IsEnchantment())
		{
			return IsAffectedBySpellPower();
		}
		return false;
	}

	private Player GetControllerForDamageOrHealingBonus()
	{
		if (HasTag(GAME_TAG.SOURCE_OVERRIDE_FOR_MODIFIER_TEXT))
		{
			return GameState.Get().GetEntity(GetTag(GAME_TAG.SOURCE_OVERRIDE_FOR_MODIFIER_TEXT)).GetController();
		}
		if (IsLettuceAbility())
		{
			Entity lettuceAbilityOwner = GetLettuceAbilityOwner();
			if (lettuceAbilityOwner != null)
			{
				return lettuceAbilityOwner.GetController();
			}
		}
		Entity parentEntity = GetParentEntity();
		if (parentEntity != null && parentEntity.IsLettuceAbility())
		{
			Entity parentEntity2 = GetParentEntity();
			if (parentEntity2 != null)
			{
				Entity lettuceAbilityOwner2 = parentEntity2.GetLettuceAbilityOwner();
				if (lettuceAbilityOwner2 != null)
				{
					return lettuceAbilityOwner2.GetController();
				}
			}
		}
		return GetController();
	}

	public int GetDamageBonus()
	{
		Player controllerForDamageOrHealingBonus = GetControllerForDamageOrHealingBonus();
		if (controllerForDamageOrHealingBonus == null)
		{
			return 0;
		}
		Entity ent = GetParentEntity() ?? this;
		if (IsSpell() || IsMinion() || IsLettuceAbilitySpellCasting() || IsEnchantmentAffectedBySpellPower())
		{
			int num = controllerForDamageOrHealingBonus.TotalSpellpower(ent, GetSpellSchool());
			if (HasTag(GAME_TAG.RECEIVES_DOUBLE_SPELLDAMAGE_BONUS))
			{
				num *= 2;
			}
			return num;
		}
		if (IsHeroPower())
		{
			int num2 = controllerForDamageOrHealingBonus.GetTag(GAME_TAG.CURRENT_HEROPOWER_DAMAGE_BONUS);
			if (GetCardTextBuilder() is SpellDamageOnlyCardTextBuilder)
			{
				int num3 = controllerForDamageOrHealingBonus.TotalSpellpower(this, GetSpellSchool());
				if (HasTag(GAME_TAG.RECEIVES_DOUBLE_SPELLDAMAGE_BONUS))
				{
					num3 *= 2;
				}
				num2 += num3;
			}
			return num2;
		}
		return 0;
	}

	public int GetDamageBonusDouble()
	{
		Player controllerForDamageOrHealingBonus = GetControllerForDamageOrHealingBonus();
		if (controllerForDamageOrHealingBonus == null)
		{
			return 0;
		}
		if (IsSpell() || IsLettuceAbilitySpellCasting() || IsEnchantmentAffectedBySpellPower())
		{
			return controllerForDamageOrHealingBonus.GetTag(GAME_TAG.SPELLPOWER_DOUBLE);
		}
		if (IsHeroPower())
		{
			return controllerForDamageOrHealingBonus.GetTag(GAME_TAG.HERO_POWER_DOUBLE);
		}
		return 0;
	}

	public int GetHealingBonus()
	{
		Player controllerForDamageOrHealingBonus = GetControllerForDamageOrHealingBonus();
		if (controllerForDamageOrHealingBonus == null)
		{
			return 0;
		}
		if (IsMinion() || IsSpell() || IsHeroPower() || IsLettuceAbilitySpellCasting() || IsEnchantmentAffectedBySpellPower())
		{
			return controllerForDamageOrHealingBonus.GetTag(GAME_TAG.CURRENT_HEALING_POWER);
		}
		return 0;
	}

	public int GetHealingDouble()
	{
		Player controllerForDamageOrHealingBonus = GetControllerForDamageOrHealingBonus();
		if (controllerForDamageOrHealingBonus == null)
		{
			return 0;
		}
		int tag = controllerForDamageOrHealingBonus.GetTag(GAME_TAG.ALL_HEALING_DOUBLE);
		if (IsSpell() || IsLettuceAbilitySpellCasting() || IsEnchantmentAffectedBySpellPower())
		{
			return controllerForDamageOrHealingBonus.GetTag(GAME_TAG.SPELL_HEALING_DOUBLE) + tag;
		}
		if (IsHeroPower())
		{
			return controllerForDamageOrHealingBonus.GetTag(GAME_TAG.HERO_POWER_DOUBLE) + tag;
		}
		return tag;
	}

	public int GetAttackBonus()
	{
		int tag = GetTag(GAME_TAG.HERO_ATTACK_GIVEN_ADDITIONAL);
		Player controllerForDamageOrHealingBonus = GetControllerForDamageOrHealingBonus();
		if (controllerForDamageOrHealingBonus == null)
		{
			return tag;
		}
		int tag2 = controllerForDamageOrHealingBonus.GetTag(GAME_TAG.HERO_ATTACK_GIVEN_ADDITIONAL);
		return tag + tag2;
	}

	public int GetArmorBonus()
	{
		int tag = GetTag(GAME_TAG.HERO_ARMOR_GIVEN_ADDITIONAL);
		Player controllerForDamageOrHealingBonus = GetControllerForDamageOrHealingBonus();
		if (controllerForDamageOrHealingBonus == null)
		{
			return tag;
		}
		int tag2 = controllerForDamageOrHealingBonus.GetTag(GAME_TAG.HERO_ARMOR_GIVEN_ADDITIONAL);
		return tag + tag2;
	}

	public void ClearBattlecryFlag()
	{
		m_useBattlecryPower = false;
	}

	public void UpdateUseBattlecryFlag(bool fromGameState)
	{
		if (IsMinion() && !IsCutsceneEntity() && GameState.Get() != null)
		{
			bool flag = fromGameState || GameState.Get().EntityHasTargets(this);
			if (TAG_ZONE.HAND == GetZone() && flag)
			{
				m_useBattlecryPower = true;
			}
		}
	}

	public virtual void InitRealTimeValues(List<Network.Entity.Tag> tags)
	{
		foreach (Network.Entity.Tag tag in tags)
		{
			switch ((GAME_TAG)tag.Name)
			{
			case GAME_TAG.COST:
				SetRealTimeCost(tag.Value);
				break;
			case GAME_TAG.ATK:
				SetRealTimeAttack(tag.Value);
				break;
			case GAME_TAG.HEALTH:
				SetRealTimeHealth(tag.Value);
				break;
			case GAME_TAG.DAMAGE:
				SetRealTimeDamage(tag.Value);
				break;
			case GAME_TAG.ARMOR:
				SetRealTimeArmor(tag.Value);
				break;
			case GAME_TAG.ZONE:
				SetRealTimeZone(tag.Value);
				break;
			case GAME_TAG.ZONE_POSITION:
				SetRealTimeZonePosition(tag.Value);
				break;
			case GAME_TAG.POWERED_UP:
				SetRealTimePoweredUp(tag.Value);
				break;
			case GAME_TAG.LINKED_ENTITY:
				SetRealTimeLinkedEntityId(tag.Value);
				break;
			case GAME_TAG.PARENT_CARD:
				SetRealTimeParentEntityId(tag.Value);
				break;
			case GAME_TAG.DIVINE_SHIELD:
				SetRealTimeDivineShield(tag.Value);
				break;
			case GAME_TAG.IMMUNE:
				SetRealTimeIsImmune(tag.Value);
				break;
			case GAME_TAG.IMMUNE_WHILE_ATTACKING:
				SetRealTimeIsImmuneWhileAttacking(tag.Value);
				break;
			case GAME_TAG.POISONOUS:
			case GAME_TAG.NON_KEYWORD_POISONOUS:
				SetRealTimeIsPoisonous(tag.Value);
				break;
			case GAME_TAG.VENOMOUS:
				SetRealTimeIsVenomous(tag.Value);
				break;
			case GAME_TAG.CARDTYPE:
				SetRealTimeCardType((TAG_CARDTYPE)tag.Value);
				break;
			case GAME_TAG.CARD_ALTERNATE_COST:
			{
				TAG_CARD_ALTERNATE_COST value = (TAG_CARD_ALTERNATE_COST)tag.Value;
				SetRealTimeCardCostsHealth(value == TAG_CARD_ALTERNATE_COST.HEALTH || value == TAG_CARD_ALTERNATE_COST.OPPONENTS_HEALTH);
				SetRealTimeCardCostsArmor(value == TAG_CARD_ALTERNATE_COST.ARMOR);
				SetRealTimeCardCostsCorpses(value == TAG_CARD_ALTERNATE_COST.CORPSES);
				SetRealTimeCardCostsAltTavernCoin(value == TAG_CARD_ALTERNATE_COST.ALT_TAVERN_COIN);
				break;
			}
			case GAME_TAG.ATTACKABLE_BY_RUSH:
				SetRealTimeAttackableByRush(tag.Value);
				break;
			case GAME_TAG.PREMIUM:
				SetRealTimePremium((TAG_PREMIUM)tag.Value);
				break;
			case GAME_TAG.PLAYER_LEADERBOARD_PLACE:
				SetRealTimePlayerLeaderboardPlace(tag.Value);
				break;
			case GAME_TAG.BACON_DUO_PASSABLE:
				SetRealTimeBaconDuoPassable(tag.Value);
				break;
			case GAME_TAG.PLAYER_TECH_LEVEL:
				SetRealTimePlayerTechLevel(tag.Value);
				break;
			case GAME_TAG.BACON_DUO_PLAYER_FIGHTS_FIRST_NEXT_COMBAT:
				SetRealTimePlayerFightsFirst(tag.Value);
				break;
			case GAME_TAG.TITAN:
				SetRealTimeIsTitan(tag.Value);
				break;
			case GAME_TAG.OLD_GOD:
				SetRealTimeIsOldGod(tag.Value);
				break;
			case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_1:
				SetRealTimeTitanAbilityUsed1(tag.Value);
				break;
			case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_2:
				SetRealTimeTitanAbilityUsed2(tag.Value);
				break;
			case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_3:
				SetRealTimeTitanAbilityUsed3(tag.Value);
				break;
			case GAME_TAG.ACTIVATABLE_MINION_ABILITY_USED_4:
				SetRealTimeTitanAbilityUsed4(tag.Value);
				break;
			case GAME_TAG.DECK_ACTION_COST:
				SetRealTimeDeckActionCost(tag.Value);
				break;
			case GAME_TAG.BACON_COMBAT_PHASE_HERO:
				SetRealTimeBaconCombatPhaseHero(tag.Value);
				break;
			}
		}
	}

	public void SetRealTimeCost(int newCost)
	{
		m_realTimeCost = newCost;
	}

	public int GetRealTimeCost()
	{
		if (m_realTimeCost == -1)
		{
			return GetCost();
		}
		return m_realTimeCost;
	}

	public void SetRealTimeAttack(int newAttack)
	{
		m_realTimeAttack = newAttack;
	}

	public int GetRealTimeAttack()
	{
		return m_realTimeAttack;
	}

	public void SetRealTimeBaconCombatPhaseHero(int value)
	{
		m_realTimeBaconCombatPhaseHero = value > 0;
	}

	public bool GetRealTimeBaconCombatPhaseHero()
	{
		return m_realTimeBaconCombatPhaseHero;
	}

	public void SetRealtimeBaconDamageCapEnabled(int value)
	{
		m_realTimeBaconDamageCapEnabled = value > 0;
	}

	public bool GetRealtimeBaconDamageCapEnabled()
	{
		return m_realTimeBaconDamageCapEnabled;
	}

	public void SetRealtimeBaconCombatPhase(int value)
	{
		m_realTimeBaconCombatPhase = value > 0;
	}

	public void SetRealTimeDeckActionCost(int cost)
	{
		m_realTimeDeckActionCost = cost;
	}

	public int GetRealTimeDeckActionCost()
	{
		return m_realTimeDeckActionCost;
	}

	public void SetRealTimeHealth(int newHealth)
	{
		m_realTimeHealth = newHealth;
	}

	public void SetRealTimeDamage(int newDamage)
	{
		m_realTimeDamage = newDamage;
	}

	public void SetRealTimeArmor(int newArmor)
	{
		m_realTimeArmor = newArmor;
	}

	public int GetRealTimeRemainingHP()
	{
		return m_realTimeHealth + m_realTimeArmor - m_realTimeDamage;
	}

	public bool WouldAnAttackZeroHealth(int realTimeAttack)
	{
		uint num = (uint)(m_realTimeHealth + m_realTimeArmor - m_realTimeDamage);
		if (m_realTimeHealth > 4294967295u - m_realTimeArmor)
		{
			int num2 = m_realTimeHealth - m_realTimeDamage;
			num = ((num2 <= 4294967295u - m_realTimeArmor) ? ((uint)(num2 + m_realTimeArmor)) : 4294967295u);
		}
		return realTimeAttack >= num;
	}

	public int GetRealTimeArmor()
	{
		return m_realTimeArmor;
	}

	public void SetRealTimeZone(int zone)
	{
		m_realTimeZone = zone;
	}

	public TAG_ZONE GetRealTimeZone()
	{
		return (TAG_ZONE)m_realTimeZone;
	}

	public void SetRealTimeZonePosition(int zonePosition)
	{
		m_realTimeZonePosition = zonePosition;
	}

	public int GetRealTimeZonePosition()
	{
		return m_realTimeZonePosition;
	}

	public void SetRealTimeLinkedEntityId(int linkedEntityId)
	{
		m_realTimeLinkedEntityId = linkedEntityId;
	}

	public int GetRealTimeLinkedEntityId()
	{
		return m_realTimeLinkedEntityId;
	}

	public void SetRealTimeParentEntityId(int parentEntityId)
	{
		m_realTimeParentEntityId = parentEntityId;
	}

	public int GetRealTimeParentEntityId()
	{
		return m_realTimeParentEntityId;
	}

	public void SetRealTimePoweredUp(int poweredUp)
	{
		m_realTimePoweredUp = poweredUp > 0;
	}

	public bool GetRealTimePoweredUp()
	{
		return m_realTimePoweredUp;
	}

	public void SetRealTimeDivineShield(int divineShield)
	{
		m_realTimeDivineShield = divineShield > 0;
	}

	public bool GetRealTimeDivineShield()
	{
		return m_realTimeDivineShield;
	}

	public void SetRealTimeIsImmune(int immune)
	{
		m_realTimeIsImmune = immune > 0;
	}

	public bool GetRealTimeIsImmune()
	{
		return m_realTimeIsImmune;
	}

	public void SetRealTimeIsImmuneWhileAttacking(int immune)
	{
		m_realTimeIsImmuneWhileAttacking = immune > 0;
	}

	public bool GetRealTimeIsImmuneWhileAttacking()
	{
		return m_realTimeIsImmuneWhileAttacking;
	}

	public void SetRealTimeIsPoisonous(int poisonous)
	{
		m_realTimeIsPoisonous = poisonous > 0;
	}

	public bool GetRealTimeIsPoisonous()
	{
		return m_realTimeIsPoisonous;
	}

	public void SetRealTimeIsVenomous(int venomous)
	{
		m_realTimeIsVenomous = venomous > 0;
	}

	public bool GetRealTimeIsVenomous()
	{
		return m_realTimeIsVenomous;
	}

	public void SetRealTimeIsDormant(int dormant)
	{
		m_realTimeIsDormant = dormant > 0;
	}

	public bool GetRealTimeIsDormant()
	{
		return m_realTimeIsDormant;
	}

	public void SetRealTimeHasSpellpower(int spellpower)
	{
		m_realTimeSpellpower = spellpower;
	}

	public int GetRealTimeSpellpower()
	{
		return m_realTimeSpellpower;
	}

	public void SetRealTimeSpellpowerDouble(int powerDouble)
	{
		m_realTimeSpellpowerDouble = powerDouble > 0;
	}

	public bool GetRealTimeSpellpowerDouble()
	{
		return m_realTimeSpellpowerDouble;
	}

	public void SetRealTimeHealingDoesDamageHint(int healingDoesDamageHint)
	{
		m_realTimeHealingDoesDamageHint = healingDoesDamageHint > 0;
	}

	public bool GetRealTimeHealingDoeDamageHint()
	{
		return m_realTimeHealingDoesDamageHint;
	}

	public void SetRealTimeLifestealDoesDamageHint(int lifestealDoesDamageHint)
	{
		m_realTimeLifestealDoesDamageHint = lifestealDoesDamageHint > 0;
	}

	public bool GetRealTimeLifestealDoesDamageHint()
	{
		return m_realTimeLifestealDoesDamageHint;
	}

	public void SetRealTimeCardCostsHealth(bool value)
	{
		m_realTimeCardCostsHealth = value;
	}

	public bool GetRealTimeCardCostsHealth()
	{
		return m_realTimeCardCostsHealth;
	}

	public void SetRealTimeCardCostsArmor(bool value)
	{
		m_realTimeCardCostsArmor = value;
	}

	public bool GetRealTimeCardCostsArmor()
	{
		return m_realTimeCardCostsArmor;
	}

	public void SetRealTimeCardCostsCorpses(bool value)
	{
		m_realTimeCardCostsCorpses = value;
	}

	public bool GetRealTimeCardCostsCorpses()
	{
		return m_realTimeCardCostsCorpses;
	}

	public void SetRealTimeCardCostsAltTavernCoin(bool value)
	{
		m_realTimeCardCostsAltTavernCoin = value;
	}

	public bool GetRealTimeCardCostsAltTavernCoin()
	{
		return m_realTimeCardCostsAltTavernCoin;
	}

	public void SetRealTimeAttackableByRush(int value)
	{
		m_realTimeAttackableByRush = value > 0;
	}

	public bool GetRealTimeAttackableByRush()
	{
		return m_realTimeAttackableByRush;
	}

	public void SetRealTimeCardType(TAG_CARDTYPE cardType)
	{
		m_realTimeCardType = cardType;
	}

	public TAG_CARDTYPE GetRealTimeCardType()
	{
		return m_realTimeCardType;
	}

	public void SetRealTimePremium(TAG_PREMIUM premium)
	{
		m_realTimePremium = premium;
	}

	public void SetRealTimePlayerLeaderboardPlace(int playerLeaderboardPlace)
	{
		m_realTimePlayerLeaderboardPlace = playerLeaderboardPlace;
	}

	public int GetRealTimePlayerLeaderboardPlace()
	{
		return m_realTimePlayerLeaderboardPlace;
	}

	public void SetRealTimePlayerFightsFirst(int value)
	{
		m_realTimePlayerFightsFirst = value;
	}

	public void SetRealTimePlayerTechLevel(int playerTechLevel)
	{
		m_realTimePlayerTechLevel = playerTechLevel;
	}

	public int GetRealTimePlayerFightsFirst()
	{
		return m_realTimePlayerFightsFirst;
	}

	public int GetRealTimePlayerTechLevel()
	{
		return m_realTimePlayerTechLevel;
	}

	public void SetRealTimeBaconDuoPassable(int baconDuoPassable)
	{
		m_realTimeBaconDuoPassable = baconDuoPassable;
	}

	public void SetRealTimeIsTitan(int isTitan)
	{
		m_realTimeIsTitan = isTitan > 0;
	}

	public void SetRealTimeIsOldGod(int isOldGod)
	{
		m_realTimeIsOldGod = isOldGod > 0;
	}

	public void SetRealTimeTitanAbilityUsed1(int used)
	{
		m_realTimeTitanAbilityUsed1 = used > 0;
	}

	public void SetRealTimeTitanAbilityUsed2(int used)
	{
		m_realTimeTitanAbilityUsed2 = used > 0;
	}

	public void SetRealTimeTitanAbilityUsed3(int used)
	{
		m_realTimeTitanAbilityUsed3 = used > 0;
	}

	public void SetRealTimeTitanAbilityUsed4(int used)
	{
		m_realTimeTitanAbilityUsed4 = used > 0;
	}

	public bool GetRealTimeTitanAbilityUsable()
	{
		if (m_realTimeIsTitan || m_realTimeIsOldGod)
		{
			if (m_realTimeTitanAbilityUsed1 && m_realTimeTitanAbilityUsed2 && m_realTimeTitanAbilityUsed3)
			{
				return !m_realTimeTitanAbilityUsed4;
			}
			return true;
		}
		return false;
	}

	public CardTextHistoryData GetCardTextHistoryData()
	{
		return m_cardTextHistoryData;
	}

	private void LoadEntityDef(string cardId)
	{
		if (base.m_cardId != cardId)
		{
			base.m_cardId = cardId;
		}
		if (!string.IsNullOrEmpty(cardId))
		{
			m_dynamicEntityDef = null;
			m_staticEntityDef = DefLoader.Get().GetEntityDef(cardId);
			if (m_staticEntityDef == null)
			{
				Error.AddDevFatal("Failed to load a card xml for {0}", cardId);
			}
			else
			{
				UpdateCardName();
			}
		}
	}

	public void LoadCard(string cardId, LoadCardData data = null, bool async = false)
	{
		LoadEntityDef(cardId);
		m_loadState = LoadState.LOADING;
		if (string.IsNullOrEmpty(cardId))
		{
			if (async)
			{
				DefLoader.Get().LoadCardDefAsync("HiddenCard", OnCardDefLoaded);
			}
			else
			{
				DefLoader.Get().LoadCardDef("HiddenCard", OnCardDefLoaded);
			}
			return;
		}
		CardPortraitQuality.GetDefault().PremiumType = m_realTimePremium;
		if (async)
		{
			DefLoader.Get().LoadCardDefAsync(cardId, OnCardDefLoaded, data);
		}
		else
		{
			DefLoader.Get().LoadCardDef(cardId, OnCardDefLoaded, data);
		}
	}

	private void OnCardDefLoaded(string cardId, DefLoader.DisposableCardDef cardDef, object callbackData)
	{
		using (cardDef)
		{
			if (cardDef == null)
			{
				Debug.LogErrorFormat("Entity.OnCardDefLoaded() - {0} does not have an asset!", cardId);
				m_loadState = LoadState.DONE;
				return;
			}
			LoadCardData loadCardData = new LoadCardData
			{
				updateActor = false,
				restartStateSpells = false,
				fromChangeEntity = false
			};
			if (callbackData is LoadCardData)
			{
				loadCardData = (LoadCardData)callbackData;
			}
			if (m_card != null)
			{
				m_card.SetCardDef(cardDef, loadCardData.updateActor);
				if (loadCardData.updateActor)
				{
					if (m_card.GetActor() != null && loadCardData.fromChangeEntity)
					{
						m_card.GetActor().SetCardDef(cardDef);
					}
					m_card.UpdateActor();
					m_card.ActivateStateSpells();
				}
				else if (loadCardData.restartStateSpells)
				{
					m_card.ActivateStateSpells(forceActivate: true);
				}
				if (loadCardData.fromChangeEntity && m_card.GetActor() != null && m_card.GetActor().UseTechLevelManaGem(checkInPlay: true))
				{
					m_card.GetActor().ShowTavernTierSpell();
				}
				m_card.RefreshCardsInTooltip();
				if (loadCardData.fromChangeEntity && IsMinion() && m_card.GetZone() is ZonePlay)
				{
					m_card.ActivateCharacterPlayEffects();
				}
			}
			UpdateUseBattlecryFlag(fromGameState: false);
			m_loadState = LoadState.DONE;
			if (m_card != null)
			{
				m_card.RefreshActor();
			}
		}
	}

	public SpellType GetTriggerSpellType()
	{
		GameState gameState = GameState.Get();
		GameMgr gameMgr = GameMgr.Get();
		if (gameMgr != null && gameState != null)
		{
			if (HasTag(GAME_TAG.BACON_TRIGGER_XY_STAY))
			{
				return SpellType.TRIGGER_XY_STAY;
			}
			if (HasTag(GAME_TAG.BACON_TRIGGER_XY))
			{
				return SpellType.TRIGGER_XY;
			}
			if (HasTag(GAME_TAG.BACON_TRIGGER_UPBEAT))
			{
				return SpellType.TRIGGER_UPBEAT;
			}
			if (gameMgr.IsBattlegrounds() && gameState.IsUsingFastActorTriggers() && !IsHeroPower())
			{
				return SpellType.FAST_TRIGGER;
			}
		}
		return SpellType.TRIGGER;
	}

	public override SpellType GetPrioritizedBaubleSpellType()
	{
		if (HasLockVisual())
		{
			return SpellType.LOCK;
		}
		if (IsPoisonous())
		{
			return SpellType.POISONOUS;
		}
		if (IsVenomous())
		{
			return SpellType.VENOMOUS;
		}
		if (HasAvenge())
		{
			return SpellType.AVENGE;
		}
		bool flag = HasSpellburst() || DoEnchantmentsHaveSpellburst() || DoEnchantmentsHaveCounter();
		bool flag2 = HasTriggerVisual() || DoEnchantmentsHaveTriggerVisuals() || HasOverheal() || HasEndOfTurnTrigger() || HasTag(GAME_TAG.FORGETFUL) || HasTag(GAME_TAG.STARSHIP_LAUNCH_TRIGGER);
		bool flag3 = HasLifesteal();
		if (flag && flag2)
		{
			return SpellType.TRIGGER_AND_SPELLBURST;
		}
		if (flag3 && flag)
		{
			return SpellType.LIFESTEAL_AND_SPELLBURST;
		}
		if (flag2)
		{
			return GetTriggerSpellType();
		}
		if (flag3)
		{
			return SpellType.LIFESTEAL;
		}
		if (HasInspire())
		{
			return SpellType.INSPIRE;
		}
		if (HasOverKill() || DoEnchantmentsHaveOverKill())
		{
			return SpellType.OVERKILL;
		}
		if (flag)
		{
			return SpellType.SPELLBURST;
		}
		if (HasFrenzy())
		{
			return SpellType.FRENZY;
		}
		if (HasHonorableKill() || DoEnchantmentsHaveHonorableKill())
		{
			return SpellType.HONORABLEKILL;
		}
		return SpellType.NONE;
	}

	public TAG_CARD_SET GetWatermarkCardSetOverride()
	{
		if (HasTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET))
		{
			return (TAG_CARD_SET)GetTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET);
		}
		EntityDef entityDef = GetEntityDef();
		if (entityDef != null && entityDef.HasTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET))
		{
			return (TAG_CARD_SET)entityDef.GetTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET);
		}
		return TAG_CARD_SET.INVALID;
	}

	public bool IsTauntIgnored()
	{
		Player firstOpponentPlayer = GameState.Get().GetFirstOpponentPlayer(GetController());
		if (firstOpponentPlayer != null && firstOpponentPlayer.HasTag(GAME_TAG.IGNORE_TAUNT))
		{
			return true;
		}
		return false;
	}

	public Entity GetLettuceAbilityOwner()
	{
		return GameState.Get().GetEntity(GetTag(GAME_TAG.LETTUCE_ABILITY_OWNER));
	}

	public bool IsMyLettuceRoleStrongAgainst(Entity otherEntity)
	{
		if (otherEntity == null)
		{
			return false;
		}
		TAG_ROLE tag = GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
		TAG_ROLE tag2 = otherEntity.GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
		if (tag == TAG_ROLE.CASTER && tag2 == TAG_ROLE.TANK)
		{
			return true;
		}
		if (tag == TAG_ROLE.TANK && tag2 == TAG_ROLE.FIGHTER)
		{
			return true;
		}
		if (tag == TAG_ROLE.FIGHTER && tag2 == TAG_ROLE.CASTER)
		{
			return true;
		}
		return false;
	}

	public bool HasSelectedLettuceAbility()
	{
		if (!HasTag(GAME_TAG.LETTUCE_ABILITY_TILE_VISUAL_ALL_VISIBLE))
		{
			return HasTag(GAME_TAG.LETTUCE_ABILITY_TILE_VISUAL_SELF_ONLY);
		}
		return true;
	}

	public bool IsMercenary()
	{
		return HasTag(GAME_TAG.LETTUCE_MERCENARY);
	}

	public bool ShouldShowEquipmentTextOnMerc()
	{
		int tag = GetTag(GAME_TAG.LETTUCE_EQUIPMENT_ID);
		if (tag == 0)
		{
			return false;
		}
		return GameDbf.GetIndex().GetEquipmentTierFromCardID(tag)?.ShowTextOnMerc ?? false;
	}

	public bool ShouldShowHeroRerollButton()
	{
		Entity gameEntity = GameState.Get().GetGameEntity();
		if (gameEntity == null)
		{
			return false;
		}
		bool num = !HasTag(GAME_TAG.BACON_LOCKED_MULLIGAN_HERO);
		bool flag = HasTag(GAME_TAG.BACON_LOCKED_MULLIGAN_HERO) && gameEntity.HasTag(GAME_TAG.BACON_UNLOCK_MULLIGAN_HERO_ENABLED);
		return num || flag;
	}

	public RerollButtonEnableResult ShouldEnableRerollButton(bool? hasFreeReroll = null, bool? hasPaidReroll = null)
	{
		MulliganManager mulliganManager = MulliganManager.Get();
		if (mulliganManager != null)
		{
			_ = !mulliganManager.IsMulliganTimerLeftExceedBaconRerollButtonCutoff();
		}
		else
			_ = 0;
		if (mulliganManager == null)
		{
			return RerollButtonEnableResult.MULLIGAN_NOT_ACTIVE;
		}
		if (mulliganManager.IsMulliganTimerLeftExceedBaconRerollButtonCutoff())
		{
			return RerollButtonEnableResult.INSUFFICIENT_MULLIGAN_TIME_LEFT;
		}
		if (BaconNumRerollLeft() <= 0)
		{
			return RerollButtonEnableResult.HERO_REROLL_LIMITATION_REACHED;
		}
		if ((hasFreeReroll.HasValue && hasFreeReroll.Value) || BaconFreeRerollLeft() > 0)
		{
			if (!HasTag(GAME_TAG.BACON_LOCKED_MULLIGAN_HERO))
			{
				return RerollButtonEnableResult.FREE;
			}
			return RerollButtonEnableResult.FREE_UNLOCK;
		}
		if ((hasPaidReroll.HasValue && hasPaidReroll.Value) || NetCache.Get().GetBattlegroundsTokenBalance() > 0)
		{
			if (!HasTag(GAME_TAG.BACON_LOCKED_MULLIGAN_HERO))
			{
				return RerollButtonEnableResult.REROLL;
			}
			return RerollButtonEnableResult.UNLOCK;
		}
		return RerollButtonEnableResult.OUT_OF_CURRENCY;
	}

	public int BaconNumRerollLeft()
	{
		Entity gameEntity = GameState.Get().GetGameEntity();
		if (gameEntity == null)
		{
			return 0;
		}
		return gameEntity.GetTag(GAME_TAG.BACON_NUM_MAX_REROLL_PER_HERO) - GetTag(GAME_TAG.BACON_NUM_MULLIGAN_REFRESH_USED);
	}

	public virtual int BaconFreeRerollLeft()
	{
		Player controller = GetController();
		if (controller == null)
		{
			return 0;
		}
		return controller.GetTag(GAME_TAG.BACON_PREMIUM_FREE_REROLLS) - controller.GetTag(GAME_TAG.BACON_NUM_FREE_REROLLS_USED);
	}

	public Entity GetEquipmentEntity()
	{
		int tag = GetTag(GAME_TAG.LETTUCE_EQUIPMENT_ID);
		if (tag == 0)
		{
			return null;
		}
		foreach (int lettuceAbilityEntityID in GetLettuceAbilityEntityIDs())
		{
			Entity entity = GameState.Get().GetEntity(lettuceAbilityEntityID);
			if (GameUtils.TranslateCardIdToDbId(entity.GetCardId()) == tag)
			{
				return entity;
			}
		}
		return null;
	}

	public bool CanBeMagnitizedBy(Entity entity)
	{
		if (HasRace(TAG_RACE.MECHANICAL))
		{
			return true;
		}
		if (entity.HasTag(GAME_TAG.MAGNETIC_TO_RACE))
		{
			return HasRace((TAG_RACE)entity.GetTag(GAME_TAG.MAGNETIC_TO_RACE));
		}
		return false;
	}

	public int GetUsableActivatableMinionAbilityCount()
	{
		if (!IsActivatableMinion())
		{
			return 0;
		}
		int num = 0;
		foreach (int subCardID in m_subCardIDs)
		{
			if (!GameState.Get().GetEntity(subCardID).HasTag(GAME_TAG.LITERALLY_UNPLAYABLE))
			{
				num++;
			}
		}
		return num;
	}

	public bool HasActivatableMinionAbilities()
	{
		return GetUsableActivatableMinionAbilityCount() > 0;
	}

	public bool? InteractableObjectCanBeActivatedNow()
	{
		Player controller = GetController();
		if (controller == null)
		{
			return false;
		}
		if (!HasTag(GAME_TAG.INTERACTABLE_OBJECT))
		{
			return false;
		}
		int tag = GetTag(GAME_TAG.INTERACTABLE_OBJECT_COST);
		if (controller.GetNumAvailableResources() < tag)
		{
			return false;
		}
		Network.Options optionsPacket = GameState.Get().GetOptionsPacket();
		if (optionsPacket == null)
		{
			return null;
		}
		List<Network.Options.Option> list = optionsPacket.List;
		if (list == null)
		{
			return null;
		}
		int entId = GetEntityId();
		return list.Any((Network.Options.Option option) => option != null && option.Main != null && option.Main.ID == entId && option.Main.PowerKeyword == GAME_TAG.INTERACTABLE_OBJECT && option.Main.PlayErrorInfo.IsValid());
	}

	public bool IsHiddenSecret()
	{
		if (IsSecret() && IsHidden())
		{
			return IsControlledByConcealedPlayer();
		}
		return false;
	}

	public bool IsHiddenForge()
	{
		if ((IsForgeable() || HasTag(GAME_TAG.FORGE_REVEALED)) && IsHidden())
		{
			return IsControlledByConcealedPlayer();
		}
		return false;
	}

	public bool IsHiddenShatter()
	{
		if ((IsShatterable() || HasTag(GAME_TAG.SHATTERED)) && IsHidden())
		{
			return IsControlledByConcealedPlayer();
		}
		return false;
	}

	public bool IsHiddenPrepare()
	{
		if ((IsPrepareable() || HasTag(GAME_TAG.PREPARE_REVEALED)) && IsHidden())
		{
			return IsControlledByConcealedPlayer();
		}
		return false;
	}

	public bool IsDisabledHeroPower()
	{
		if (!IsHeroPower())
		{
			return false;
		}
		if (HasTag(GAME_TAG.HERO_POWER_DISABLED))
		{
			return true;
		}
		if (GetController() != null && !HasTag(GAME_TAG.ADDITIONAL_HERO_POWER_INDEX))
		{
			return GetController().HasTag(GAME_TAG.HERO_POWER_DISABLED);
		}
		return false;
	}

	public bool HasHandTarget()
	{
		if (GetTag(GAME_TAG.CAN_TARGET_CARDS_IN_HAND) <= 0)
		{
			return false;
		}
		Network.EntityChoices entityChoices = GameState.Get()?.GetFriendlyEntityChoices();
		if (entityChoices != null)
		{
			foreach (int entity3 in entityChoices.Entities)
			{
				Entity entity = GameState.Get().GetEntity(entity3);
				if (entity != null && entity.GetCard() != null && entity.GetCard().GetZone() != null && entity.GetCard().GetZone().m_ServerTag == TAG_ZONE.HAND)
				{
					return true;
				}
			}
		}
		Network.Options.Option option = GameState.Get()?.GetSelectedNetworkOption();
		if (option == null)
		{
			return false;
		}
		foreach (Network.Options.Option.TargetOption target in option.Main.Targets)
		{
			if (target.PlayErrorInfo.IsValid())
			{
				Entity entity2 = GameState.Get().GetEntity(target.ID);
				if (entity2 != null && entity2.GetCard() != null && entity2.GetCard().GetZone() != null && entity2.GetCard().GetZone().m_ServerTag == TAG_ZONE.HAND)
				{
					return true;
				}
			}
		}
		return false;
	}
}
