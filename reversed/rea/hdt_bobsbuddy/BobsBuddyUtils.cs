using System;
using System.Collections.Generic;
using System.Linq;
using BobsBuddy;
using BobsBuddy.Enchantments;
using BobsBuddy.Factory;
using BobsBuddy.HeroPowers;
using BobsBuddy.Minions.Beast;
using BobsBuddy.Minions.Mech;
using BobsBuddy.Minions.Pirate;
using BobsBuddy.Simulation;
using BobsBuddy.Spells;
using BobsBuddy.Trinkets;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.BobsBuddy;

internal static class BobsBuddyUtils
{
	private const string ReplicatingMenace_Normal = "BG_BOT_312e";

	private const string ReplicatingMenace_Golden = "TB_BaconUps_032e";

	private const string WhirringProtector_Normal = "BG33_807e";

	private const string WhirringProtector_Golden = "BG33_807_Ge";

	private const string LivingSpores = "UNG_999t2e";

	public const string RebornRiteEnchmantment = "TB_BaconShop_HP_024e2";

	public const string SneedsEnchantment = "BG21_HERO_030pe";

	internal const string RebornRite = "TB_BaconShop_HP_024";

	private static string? _versionString;

	internal static string VersionString => _versionString ?? (_versionString = "v" + typeof(SimulationRunner).Assembly.GetName().Version.ToVersionString());

	internal static Minion GetMinionFromEntity(Simulator sim, bool player, Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity, IEnumerable<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> attachedEntities, IReadOnlyDictionary<int, Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>? allEntities = null)
	{
		string text = entity.Info.LatestCardId ?? "Unknown";
		Minion minion = sim.MinionFactory.CreateFromCardId(text, player);
		minion.PrimaryRace = (Race)entity.GetTag(GameTag.CARDRACE);
		minion.baseAttack = entity.GetTag(GameTag.ATK);
		minion.baseHealth = entity.GetTag(GameTag.HEALTH) - entity.GetTag(GameTag.DAMAGE);
		minion.maxAttack = entity.GetTag(GameTag.ATK);
		minion.maxHealth = entity.GetTag(GameTag.HEALTH);
		minion.taunt = entity.HasTag(GameTag.TAUNT);
		minion.div = (entity.HasTag(GameTag.DIVINE_SHIELD) ? 1 : 0);
		minion.cleave = MinionFactory.cardIDsWithCleave.Contains(minion.CardID);
		minion.poisonous = entity.HasTag(GameTag.POISONOUS);
		minion.venomous = entity.HasTag(GameTag.VENOMOUS);
		minion.windfury = entity.HasTag(GameTag.WINDFURY);
		minion.megaWindfury = entity.HasTag(GameTag.MEGA_WINDFURY) || MinionFactory.cardIdsWithMegaWindfury.Contains(text);
		minion.stealth = entity.HasTag(GameTag.STEALTH);
		minion.golden = entity.HasTag(GameTag.PREMIUM);
		minion.tier = entity.GetTag(GameTag.TECH_LEVEL);
		minion.reborn = entity.HasTag(GameTag.REBORN);
		minion.ScriptDataNum1 = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
		minion.ScriptDataNum2 = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
		minion.ScriptDataNum3 = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_3);
		minion.ScriptDataNum4 = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_4);
		int dbfId = entity.Card.DbfId;
		int tag = entity.GetTag(GameTag.MODULAR_ENTITY_PART_1);
		int tag2 = entity.GetTag(GameTag.MODULAR_ENTITY_PART_2);
		if (tag > 0 && tag2 > 0 && (tag == dbfId || tag2 == dbfId))
		{
			Card cardFromDbfId = Database.GetCardFromDbfId((tag == dbfId) ? tag2 : tag, collectible: false);
			if (cardFromDbfId != null)
			{
				Minion minion2 = (minion.AttachedModularEntity = sim.MinionFactory.CreateFromCardId(cardFromDbfId.Id, player));
				minion2.AttachedTo = minion;
			}
		}
		if (minion.golden && MinionFactory.cardIdsWithoutPremiumImplementations.Contains(text))
		{
			minion.vanillaAttack *= 2;
			minion.vanillaHealth *= 2;
		}
		foreach (Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity attachedEntity in attachedEntities)
		{
			switch (attachedEntity.CardId)
			{
			case "BG_BOT_312e":
				minion.AdditionalDeathrattles.Add(ReplicatingMenace.Deathrattle(golden: false));
				continue;
			case "TB_BaconUps_032e":
				minion.AdditionalDeathrattles.Add(ReplicatingMenace.Deathrattle(golden: true));
				continue;
			case "BG33_807e":
				minion.AdditionalRallies.Add(WhirringProtector.Rally(golden: false));
				continue;
			case "BG33_807_Ge":
				minion.AdditionalRallies.Add(WhirringProtector.Rally(golden: true));
				continue;
			case "UNG_999t2e":
				minion.AdditionalDeathrattles.Add(GenericDeathrattles.Plants);
				continue;
			case "BG21_HERO_030pe":
				minion.AdditionalDeathrattles.Add(GenericDeathrattles.SneedHeroPower);
				continue;
			case "BG22_HERO_001p_t1e":
				minion.AdditionalDeathrattles.Add(GenericDeathrattles.EarthInvocationDeathrattle);
				continue;
			case "BG27_004e":
				minion.AdditionalDeathrattles.Add(GenericDeathrattles.Crab);
				continue;
			case "BG27_004_Ge":
				minion.AdditionalDeathrattles.Add(GenericDeathrattles.CrabGolden);
				continue;
			case "BG22_HERO_001_Buddy_e1":
				minion.AdditionalDeathrattles.Add(BrukanInvocationDeathrattles.Earth);
				continue;
			case "BG22_HERO_001_Buddy_e2":
				minion.AdditionalDeathrattles.Add(BrukanInvocationDeathrattles.Fire);
				continue;
			case "BG22_HERO_001_Buddy_e3":
				minion.AdditionalDeathrattles.Add(BrukanInvocationDeathrattles.Water);
				continue;
			case "BG22_HERO_001_Buddy_e4":
				minion.AdditionalDeathrattles.Add(BrukanInvocationDeathrattles.Lightning);
				continue;
			case "TB_BaconShop_HP_069e":
				minion.HasWingmen = true;
				continue;
			case "BG30_119e":
				minion.AdditionalDeathrattles.Add(Scallywag.Deathrattle(golden: false));
				continue;
			case "BG30_119_Ge":
				minion.AdditionalDeathrattles.Add(Scallywag.Deathrattle(golden: true));
				continue;
			case "BG21_000e":
				minion.AdditionalDeathrattles.Add(Leapfrogger.Deathrattle(golden: false));
				continue;
			case "BG21_000_Ge":
				minion.AdditionalDeathrattles.Add(Leapfrogger.Deathrattle(golden: true));
				continue;
			case "BG30_MagicItem_917e":
				minion.AdditionalDeathrattles.Add(RustyTrident.Deathrattle());
				continue;
			case "BG30_MagicItem_411e":
				minion.AdditionalDeathrattles.Add(HoggyBank.Deathrattle());
				continue;
			case "BG30_MagicItem_952e":
				minion.AdditionalDeathrattles.Add(JarredFrostling.Deathrattle());
				continue;
			case "BG20_GEMe2":
			{
				int tag3 = attachedEntity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
				int tag4 = attachedEntity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
				minion.SetBloodGemStats(tag3, tag4);
				continue;
			}
			}
			if (attachedEntity.Card.TypeEnum == CardType.ENCHANTMENT && attachedEntity.CardId != null)
			{
				Enchantment enchantment = sim.EnchantmentFactory.Create(attachedEntity.CardId, minion.ControlledByPlayer);
				if (enchantment != null)
				{
					enchantment.ScriptDataNum1 = attachedEntity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
					enchantment.ScriptDataNum2 = attachedEntity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
					minion.AttachEnchantment(enchantment);
				}
			}
		}
		if (minion.IsMech() && attachedEntities.Any((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity e) => e.HasTag(GameTag.MAGNETIC)))
		{
			CheckForMagneticDeathrattles(minion, attachedEntities, allEntities);
		}
		minion.game_id = entity.Id;
		return minion;
	}

	private static void CheckForMagneticDeathrattles(Minion minion, IEnumerable<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> attachedEntities, IReadOnlyDictionary<int, Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity>? allEntities)
	{
		if (allEntities == null)
		{
			return;
		}
		foreach (int magneticId in (from e in attachedEntities
			select e.GetTag(GameTag.CREATOR) into id
			where id > 0
			select id).Distinct())
		{
			if (!allEntities.TryGetValue(magneticId, out Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity value))
			{
				continue;
			}
			int num = allEntities.Values.Count((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.IsAttachedTo(magneticId) && x.CardId == "BG32_172e");
			if (num != 0)
			{
				int num2 = Math.Max(value.GetTag((GameTag)4741), num);
				for (int num3 = 0; num3 < num2; num3++)
				{
					minion.AdditionalDeathrattles.Add(AutoAssembler.Deathrattle(golden: false));
				}
			}
		}
	}

	private static void SetScriptDataProperties(dynamic item, Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity)
	{
		int tag = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
		int tag2 = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
		if (tag > 0)
		{
			item.ScriptDataNum1 = tag;
		}
		if (tag2 > 0)
		{
			item.ScriptDataNum2 = tag2;
		}
	}

	internal static Objective GetObjectiveFromEntity(ObjectiveFactory factory, bool player, Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity)
	{
		Objective objective = factory.Create(entity.CardId ?? "", player);
		SetScriptDataProperties(objective, entity);
		return objective;
	}

	internal static Trinket GetTrinketFromEntity(TrinketFactory factory, bool player, Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity entity)
	{
		Trinket trinket = factory.Create(entity.CardId ?? "", player);
		SetScriptDataProperties(trinket, entity);
		if (entity.CardId == "BG30_MagicItem_434")
		{
			trinket.ScriptDataNum1 = entity.GetTag((GameTag)4696);
		}
		trinket.game_id = entity.Id;
		return trinket;
	}

	internal static bool IsOpponentHeroKelThuzad()
	{
		int? heroEntityId = Core.Game.OpponentEntity?.GetTag(GameTag.HERO_ENTITY);
		return (Core.Game.Opponent.PlayerEntities?.FirstOrDefault((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.Id == heroEntityId))?.CardId == "TB_BaconShop_HERO_KelThuzad";
	}

	internal static bool WasHeroPowerActivated(Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity? heroPower)
	{
		if (heroPower != null)
		{
			if (!heroPower.HasTag(GameTag.EXHAUSTED))
			{
				return heroPower.HasTag(GameTag.BACON_HERO_POWER_ACTIVATED);
			}
			return true;
		}
		return false;
	}

	internal static IOrderedEnumerable<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> GetOrderedMinions(IEnumerable<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> board)
	{
		return from x in board
			where x.IsMinion
			select x.Clone() into x
			orderby x.GetTag(GameTag.ZONE_POSITION)
			select x;
	}

	internal static IOrderedEnumerable<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> GetOrderedHandEntities(IEnumerable<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> hand)
	{
		return hand.OrderBy((Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity x) => x.GetTag(GameTag.ZONE_POSITION));
	}
}
