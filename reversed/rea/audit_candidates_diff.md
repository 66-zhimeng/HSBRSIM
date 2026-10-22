# 不确定卡牌审计候选（差分证据，第二轮）

> 数据源：679 场 HDT BobsBuddy 差分（`runs/diff_report.json`）、F-15 gap-aware 300 状态 BB 批量（`runs/diff_bulk_bb_after_f15_gapaware.jsonl`）、26 场真实 Power.log（A/B/C 三局，`runs/official_traces_powerlog*`）。
> 口径：证据优先；**TVD 是采样指标、C'Thun split 是已知 gap、BB 是第三方参照**；无真实 trace 的卡明确标注“仅单测”。

## 0. 总览

- 候选卡 **370**；有真实 Power.log trace **86**；有 679 差分证据 **213**；有 BB 高 TVD(≥0.15) **127**。
- 类别分布：`疑似卡牌效果` 165；`脚本缺失` 156；`动态状态` 97；`状态口径缺口` 48；`已知 TOP/审计名单` 17；`C 类证据不足` 6。

## 1. 候选总表（按证据强度）

| # | 卡 | 名称 | 类别 | 679 分歧/缺脚本/动态 | BB n / mean / max TVD | 真实 trace | 备注 |
|---|---|---|---|---|---|---|---|
| 1 | `BG26_174` | Soul Rewinder | C 类证据不足、动态状态、已知 TOP/审计名单、疑似卡牌效果 | 16 / 0 / 45 | 54 / 0.0693 / 0.6839 | 13 | C 类 |
| 2 | `BG31_801` | Forest Rover | 动态状态、疑似卡牌效果 | 14 / 0 / 47 | 25 / 0.0467 / 0.3594 | 0 |  |
| 3 | `BG31_171` | Moonsteel Juggernaut | 动态状态、已知 TOP/审计名单 | 12 / 0 / 32 | — | 0 | 已知 TOP |
| 4 | `BG33_156` | Silent Enforcer | 疑似卡牌效果、脚本缺失 | 11 / 26 / 0 | 7 / 0.1538 / 0.5796 | 0 |  |
| 5 | `BG31_826` | Doubloon Grifter | 动态状态、状态口径缺口、疑似卡牌效果、脚本缺失 | 10 / 21 / 11 | 10 / 0.0284 / 0.2034 | 1 |  |
| 6 | `BG34_Giant_031` | Timewarped Leapfrogger | 疑似卡牌效果、脚本缺失 | 11 / 21 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 7 | `BG33_240` | Charmwing | 疑似卡牌效果、脚本缺失 | 10 / 24 / 0 | 4 / 0.0 / 0.0 | 0 |  |
| 8 | `BG23_009` | Lava Lurker | 动态状态、疑似卡牌效果 | 10 / 0 / 1 | 10 / 0.067 / 0.1713 | 0 |  |
| 9 | `BG31_035` | Groundbreaker | C 类证据不足、动态状态、已知 TOP/审计名单、疑似卡牌效果 | 10 / 0 / 25 | 5 / 0.0498 / 0.249 | 0 | C 类 |
| 10 | `BG28_583` | Geomagus Roogug | 已知 TOP/审计名单、疑似卡牌效果、脚本缺失 | 9 / 26 / 0 | 6 / 0.1308 / 0.556 | 0 | 已知 TOP |
| 11 | `BGS_004` | Wrath Weaver | C 类证据不足、动态状态、已知 TOP/审计名单、疑似卡牌效果 | 9 / 0 / 40 | 28 / 0.0242 / 0.2644 | 6 | C 类 |
| 12 | `BG34_781` | Felhorn | 疑似卡牌效果、脚本缺失 | 9 / 23 / 0 | 5 / 0.0039 / 0.0156 | 0 |  |
| 13 | `TB_BaconShop_HERO_33_Buddy` | Mishmash | 疑似卡牌效果、脚本缺失 | 9 / 20 / 0 | 4 / 0.0 / 0.0 | 0 |  |
| 14 | `BG32_822` | Fire-forged Evoker | C 类证据不足、动态状态、已知 TOP/审计名单 | 9 / 0 / 21 | 3 / 0.0 / 0.0 | 0 | C 类 |
| 15 | `BG29_815` | Nightbane, Ignited | 疑似卡牌效果、脚本缺失 | 8 / 19 / 0 | 2 / 0.2475 / 0.257 | 0 |  |
| 16 | `BG25_008` | Eternal Knight | 动态状态、疑似卡牌效果 | 17 / 0 / 14（skip 28） | 55 / 0.0534 / 0.6839 | 3 |  |
| 17 | `BG34_Giant_208` | Timewarped Pagle | 疑似卡牌效果、脚本缺失 | 8 / 23 / 0 | 1 / 0.0995 / 0.0995 | 0 |  |
| 18 | `BG20_203` | Prophet of the Boar | 疑似卡牌效果、脚本缺失 | 8 / 23 / 0 | 3 / 0.0687 / 0.1066 | 0 |  |
| 19 | `BG34_765` | Dead Sea Ravager | 动态状态、疑似卡牌效果、脚本缺失 | 7 / 21 / 10 | 3 / 0.0579 / 0.1737 | 0 |  |
| 20 | `BG29_807` | Trigore the Lasher | 动态状态、疑似卡牌效果、脚本缺失 | 7 / 15 / 1 | 8 / 0.0567 / 0.2034 | 0 |  |
| 21 | `BG32_204` | Silky Shimmermoth | 动态状态、疑似卡牌效果、脚本缺失 | 7 / 13 / 7 | 5 / 0.0016 / 0.004 | 0 |  |
| 22 | `BG29_862` | Barrens Conjurer | 疑似卡牌效果、脚本缺失 | 8 / 18 / 0 | 2 / 0.0 / 0.0 | 0 |  |
| 23 | `BG34_Giant_314` | Timewarped Poet | 疑似卡牌效果、脚本缺失 | 8 / 19 / 0 | 3 / 0.0 / 0.0 | 0 |  |
| 24 | `BG33_155` | Devout Hellcaller | C 类证据不足、动态状态、已知 TOP/审计名单、疑似卡牌效果 | 8 / 0 / 21 | 13 / 0.1381 / 0.5796 | 1 | C 类 |
| 25 | `BG34_320` | The Last One Standing | 动态状态、已知 TOP/审计名单、状态口径缺口 | 4 / 0 / 5 | — | 2 | 已知 TOP |
| 26 | `BG34_523` | Hunting Tiger Shark | 疑似卡牌效果、脚本缺失 | 7 / 22 / 0 | 3 / 0.165 / 0.257 | 0 |  |
| 27 | `BG33_241` | Sleepy Supporter | 动态状态、疑似卡牌效果、脚本缺失 | 6 / 22 / 2 | 10 / 0.1602 / 0.8373 | 0 |  |
| 28 | `BG_DAL_775` | Tunnel Blaster | 疑似卡牌效果、脚本缺失 | 7 / 13 / 0 | 1 / 0.1094 / 0.1094 | 0 |  |
| 29 | `BG32_433` | Dreaming Thornweaver | 动态状态、疑似卡牌效果、脚本缺失 | 6 / 18 / 2 | 4 / 0.0422 / 0.125 | 0 |  |
| 30 | `BG26_159` | Moon-Bacon Jazzer | 疑似卡牌效果、脚本缺失 | 7 / 24 / 0 | 13 / 0.0392 / 0.2034 | 0 |  |
| 31 | `BG34_731` | Twilight Broodmother | 疑似卡牌效果、脚本缺失 | 7 / 20 / 0 | 9 / 0.0287 / 0.258 | 0 |  |
| 32 | `BG29_806` | Iridescent Skyblazer | 动态状态、疑似卡牌效果、脚本缺失 | 6 / 21 / 3 | 6 / 0.0026 / 0.0156 | 0 |  |
| 33 | `BG26_523` | Tichondrius | — | 0 / 0 / 0 | 4 / 0.0 / 0.0 | 4 |  |
| 34 | `BG30_122` | Mrglin' Burglar | 疑似卡牌效果、脚本缺失 | 7 / 16 / 0 | — | 0 |  |
| 35 | `BG33_319` | Rimescale Priestess | 疑似卡牌效果、脚本缺失 | 7 / 26 / 0 | 2 / 0.0 / 0.0 | 0 |  |
| 36 | `BG33_894` | Coldlight Diver | 疑似卡牌效果、脚本缺失 | 7 / 16 / 0 | 7 / 0.0 / 0.0 | 0 |  |
| 37 | `BG34_Giant_602` | Timewarped Lei | 疑似卡牌效果、脚本缺失 | 7 / 13 / 0 | 3 / 0.0 / 0.0 | 0 |  |
| 38 | `BG36_101` | Unwilling Slacker | 状态口径缺口 | 0 / 0 / 0 | 1 / 0.0 / 0.0 | 6 |  |
| 39 | `BG36_115` | Nightmare Corroder | 状态口径缺口 | 0 / 0 / 0 | 2 / 0.0 / 0.0 | 6 |  |
| 40 | `BG36_311` | Abyssal Envoy | 状态口径缺口 | 0 / 0 / 0 | 1 / 0.0 / 0.0 | 6 |  |
| 41 | `TB_BaconUps_079` | Wrath Weaver | — | 0 / 0 / 0 | 4 / 0.0 / 0.0 | 4 |  |
| 42 | `BG35_701` | Brazen Buccaneer | C 类证据不足、动态状态、已知 TOP/审计名单、疑似卡牌效果 | 7 / 0 / 36 | 8 / 0.0432 / 0.1737 | 0 | C 类 |
| 43 | `BG32_324` | Drustfallen Butcher | 动态状态 | 7 / 0 / 12 | 5 / 0.0277 / 0.131 | 0 |  |
| 44 | `BG32_234` | Dastardly Drust | 动态状态 | 7 / 0 / 15 | 6 / 0.0166 / 0.0995 | 0 |  |
| 45 | `BG25_032` | Peggy Sturdybone | 疑似卡牌效果、脚本缺失 | 6 / 14 / 0 | 2 / 0.2475 / 0.257 | 0 |  |
| 46 | `BG26_867` | Three Lil' Quilboar | 疑似卡牌效果、脚本缺失 | 6 / 20 / 0 | 5 / 0.0587 / 0.125 | 0 |  |
| 47 | `BG21_014` | Prized Promo-Drake | 动态状态、疑似卡牌效果、脚本缺失 | 5 / 12 / 4 | 2 / 0.0219 / 0.0437 | 0 |  |
| 48 | `BG21_005` | Famished Felbat | 脚本缺失 | 6 / 27 / 0 | 7 / 0.0 / 0.0 | 0 |  |
| 49 | `BG33_809` | Divine Sparkbot | 疑似卡牌效果、脚本缺失 | 6 / 13 / 0 | — | 0 |  |
| 50 | `BG34_Giant_597` | Timewarped Immortal | 疑似卡牌效果、脚本缺失 | 6 / 14 / 0 | — | 0 |  |
| 51 | `BG35_890` | Ingenious Inventor | 动态状态、疑似卡牌效果、脚本缺失 | 5 / 8 / 4 | — | 0 |  |
| 52 | `BG28_633` | Felboar | 动态状态、疑似卡牌效果 | 0 / 0 / 2 | 8 / 0.0189 / 0.1509 | 3 |  |
| 53 | `BG32_873` | Ashen Corruptor | 动态状态 | 2 / 0 / 7 | 4 / 0.0 / 0.0 | 2 |  |
| 54 | `BG35_334` | Skeletal Strafer | 动态状态、疑似卡牌效果、脚本缺失 | 4 / 13 / 6 | 1 / 0.5796 / 0.5796 | 0 |  |
| 55 | `BG27_004` | Surf n' Surf | — | 17 / 0 / 0 | — | 0 |  |
| 56 | `BG35_437` | Vinespeaker | 动态状态 | 6 / 0 / 8 | 2 / 0.0 / 0.0 | 0 |  |
| 57 | `BG28_308` | Archlich Kel'Thuzad | 疑似卡牌效果、脚本缺失 | 5 / 11 / 0 | 2 / 0.2898 / 0.5796 | 0 |  |
| 58 | `BG35_921` | Abyssal Bruiser | 动态状态、疑似卡牌效果 | 4 / 0 / 13（skip 17） | 2 / 0.2483 / 0.3454 | 0 |  |
| 59 | `BG35_151` | Woodland Defiler | 疑似卡牌效果、脚本缺失 | 5 / 24 / 0 | 13 / 0.0404 / 0.3454 | 0 |  |
| 60 | `BG29_300` | Very Hungry Winterfinner | 状态口径缺口 | 0 / 0 / 0 | 5 / 0.0 / 0.0 | 10 |  |
| 61 | `BG32_434` | Skulking Bristlemane | 疑似卡牌效果、脚本缺失 | 5 / 14 / 0 | 8 / 0.0 / 0.0 | 0 |  |
| 62 | `BG32_111` | Nightmare Par-tea Guest | 状态口径缺口、疑似卡牌效果 | 19 / 0 / 0 | 18 / 0.0111 / 0.1509 | 3 |  |
| 63 | `BG26_174_G` | Soul Rewinder | — | 0 / 0 / 0 | 3 / 0.0 / 0.0 | 3 |  |
| 64 | `BG36_103` | N'raqi Sapper | 状态口径缺口 | 0 / 0 / 0 | 2 / 0.0 / 0.0 | 3 |  |
| 65 | `BG20_HERO_242p` | Natural Balance | — | 6 / 0 / 0 | — | 0 |  |
| 66 | `BG28_HERO_400p2` | Lucky Roll | — | 6 / 0 / 0 | — | 0 |  |
| 67 | `TB_BaconShop_HP_037a` | Wax Warband | — | 6 / 0 / 0 | — | 0 |  |
| 68 | `BG29_841` | Greymane's Champion | 疑似卡牌效果、脚本缺失 | 4 / 14 / 0 | 3 / 0.2008 / 0.5796 | 0 |  |
| 69 | `BG20_301` | Sun-Bacon Relaxer | 疑似卡牌效果、脚本缺失 | 4 / 14 / 0 | 13 / 0.1058 / 0.6875 | 0 |  |
| 70 | `BG26_800` | Manasaber | 疑似卡牌效果、脚本缺失 | 4 / 10 / 0 | 5 / 0.0625 / 0.3125 | 0 |  |
| 71 | `BG23_018` | Darkgaze Elder | 动态状态、疑似卡牌效果、脚本缺失 | 3 / 10 / 8 | 4 / 0.0312 / 0.125 | 0 |  |
| 72 | `BG26_175` | Elemental of Surprise | 脚本缺失 | 4 / 9 / 0 | 1 / 0.0046 / 0.0046 | 0 |  |
| 73 | `BG34_Giant_042` | Timewarped Chameleon | 脚本缺失 | 4 / 12 / 0 | — | 0 |  |
| 74 | `BG34_Giant_067` | Timewarped Sellemental | 脚本缺失 | 4 / 9 / 0 | — | 0 |  |
| 75 | `BG35_153` | Consummate Conqueror | 动态状态、脚本缺失 | 3 / 16 / 3 | 2 / 0.0 / 0.0 | 0 |  |
| 76 | `BG35_601` | Wyvern Outrider | 动态状态、脚本缺失 | 3 / 10 / 1 | — | 0 |  |
| 77 | `BG_BOT_911` | Annoy-o-Module | 脚本缺失 | 4 / 10 / 0 | — | 0 |  |
| 78 | `BG29_816` | Roaring Recruiter | 动态状态、疑似卡牌效果 | 4 / 0 / 5 | 10 / 0.2372 / 0.6426 | 0 |  |
| 79 | `BG31_824` | Dual-Wield Corsair | 动态状态、疑似卡牌效果 | 4 / 0 / 14 | 8 / 0.1825 / 0.4825 | 0 |  |
| 80 | `BG34_856` | Waveling | 动态状态 | 2 / 0 / 6 | 5 / 0.0022 / 0.0056 | 1 |  |
| 81 | `BG34_692` | Forsaken Weaver | 动态状态 | 4 / 0 / 9 | 3 / 0.0447 / 0.131 | 0 |  |
| 82 | `BG31_803` | Buzzing Vermin | 动态状态 | 4 / 0 / 16 | 17 / 0.0068 / 0.1094 | 0 |  |
| 83 | `BG26_137` | Bream Counter | 动态状态 | 4 / 0 / 5 | — | 0 |  |
| 84 | `BG31_809` | Turquoise Skitterer | 动态状态 | 4 / 0 / 9 | 5 / 0.0 / 0.0 | 0 |  |
| 85 | `TB_BaconShop_HERO_78_Buddy` | Snow Elemental | 疑似卡牌效果、脚本缺失 | 3 / 4 / 0 | 2 / 0.249 / 0.249 | 0 |  |
| 86 | `BG24_707` | Bristlemane Scrapsmith | 疑似卡牌效果、脚本缺失 | 3 / 12 / 0 | 7 / 0.1994 / 0.6875 | 0 |  |
| 87 | `BG30_117` | Volcanic Visitor | 疑似卡牌效果、脚本缺失 | 3 / 6 / 0 | 1 / 0.0995 / 0.0995 | 0 |  |
| 88 | `BG25_HERO_103_Buddy` | Shadowy Construct | 疑似卡牌效果、脚本缺失 | 3 / 7 / 0 | 3 / 0.0907 / 0.2722 | 0 |  |
| 89 | `BG34_Giant_038` | Timewarped Elise | 动态状态、脚本缺失 | 2 / 5 / 3 | 4 / 0.0 / 0.0 | 1 |  |
| 90 | `TB_BaconShop_HERO_53_Buddy` | Valithria Dreamwalker | 疑似卡牌效果、脚本缺失 | 3 / 5 / 0 | 5 / 0.0544 / 0.2722 | 0 |  |
| 91 | `TB_BaconShop_HERO_41_Buddy` | Sr. Tomb Diver | 疑似卡牌效果、脚本缺失 | 3 / 9 / 0 | 8 / 0.034 / 0.2722 | 0 |  |
| 92 | `BG29_808` | Spiked Savior | 疑似卡牌效果、脚本缺失 | 3 / 9 / 0 | 2 / 0.002 / 0.004 | 0 |  |
| 93 | `BG35_433` | Redtusk Thornraiser | 疑似卡牌效果、脚本缺失 | 3 / 11 / 0 | 4 / 0.001 / 0.004 | 0 |  |
| 94 | `BG23_013` | Tidemistress Athissa | 动态状态、脚本缺失 | 2 / 6 / 2 | — | 0 |  |
| 95 | `BG31_176` | Dr. Boom's Monster | 脚本缺失 | 3 / 10 / 0 | — | 0 |  |
| 96 | `BG34_Giant_309` | Timewarped Nine Frogs | 动态状态、疑似卡牌效果、脚本缺失 | 2 / 2 / 1 | — | 0 |  |
| 97 | `BG34_Giant_321` | Timewarped Mrrrglr | 脚本缺失 | 3 / 8 / 0 | — | 0 |  |
| 98 | `BG34_Giant_322` | Timewarped Pioneer | 动态状态、疑似卡牌效果、脚本缺失 | 2 / 3 / 1 | — | 0 |  |
| 99 | `BG34_Giant_333` | Timewarped Scout | 动态状态、疑似卡牌效果、脚本缺失 | 2 / 2 / 1 | — | 0 |  |
| 100 | `BG34_Giant_680` | Timewarped Collector | 疑似卡牌效果、脚本缺失 | 3 / 5 / 0 | — | 0 |  |
| 101 | `BG35_342` | Falling Sky Golem | 动态状态 | 2 / 0 / 3（skip 31） | — | 0 |  |
| 102 | `BG35_702` | Roving Sailor | 动态状态、脚本缺失 | 2 / 7 / 4 | 1 / 0.0 / 0.0 | 0 |  |
| 103 | `TB_BaconShop_HERO_11_Buddy` | Lucifron | 疑似卡牌效果、脚本缺失 | 3 / 5 / 0 | — | 0 |  |
| 104 | `TB_BaconShop_HERO_56_Buddy` | Vaelastrasz | 疑似卡牌效果、脚本缺失 | 3 / 5 / 0 | — | 0 |  |
| 105 | `TB_BaconShop_HERO_90_Buddy` | Burth | 动态状态、疑似卡牌效果、脚本缺失 | 2 / 4 / 3 | — | 0 |  |
| 106 | `BG26_524` | Malchezaar, Prince of Dance | 动态状态 | 1 / 0 / 2 | 10 / 0.0021 / 0.0163 | 3 |  |
| 107 | `BG33_923` | Sundered Matriarch | 动态状态、疑似卡牌效果 | 3 / 0 / 8 | 6 / 0.1658 / 0.3454 | 0 |  |
| 108 | `BG34_500` | Flaming Enforcer | 疑似卡牌效果 | 0 / 0 / 0 | 27 / 0.0339 / 0.1649 | 2 |  |
| 109 | `BG30_123` | Fearless Foodie | 状态口径缺口 | 0 / 0 / 0 | 3 / 0.0215 / 0.0437 | 2 |  |
| 110 | `BG34_500_G` | Flaming Enforcer | — | 0 / 0 / 0 | 2 / 0.0 / 0.0 | 2 |  |
| 111 | `BG36_300` | N'raqi Frostcaller | 已知 TOP/审计名单、状态口径缺口 | 0 / 0 / 0 | — | 2 | 已知 TOP |
| 112 | `BG36_520` | Bilgewater Breakout | 状态口径缺口 | 0 / 0 / 0 | — | 2 |  |
| 113 | `BG36_731` | Imp-lusionist | — | 0 / 0 / 0 | 2 / 0.0 / 0.0 | 2 |  |
| 114 | `BG35_801` | Gluttonous Trogg | 动态状态、疑似卡牌效果 | 3 / 0 / 21 | 47 / 0.1096 / 1.0 | 0 |  |
| 115 | `BG34_690` | Plaguerunner | 动态状态、疑似卡牌效果 | 2 / 0 / 2 | 1 / 0.832 / 0.832 | 0 |  |
| 116 | `BG34_HERO_004p` | Warped Conflux | — | 4 / 0 / 0 | — | 0 |  |
| 117 | `BGS_018` | Goldrinn, the Great Wolf | 动态状态 | 3 / 0 / 9 | 2 / 0.0 / 0.0 | 0 |  |
| 118 | `TB_BaconShop_HP_008` | Smart Savings | — | 4 / 0 / 0 | — | 0 |  |
| 119 | `BG33_323` | Dustbone Devastator | 动态状态、疑似卡牌效果 | 2 / 0 / 7 | 1 / 0.5796 / 0.5796 | 0 |  |
| 120 | `TB_BaconShop_HERO_28_Buddy` | Clockwork Assistant | 疑似卡牌效果、脚本缺失 | 2 / 2 / 0 | 2 / 0.2483 / 0.3454 | 0 |  |
| 121 | `TB_BaconShop_HERO_29_Buddy` | Tentacle of C'Thun | 疑似卡牌效果、脚本缺失 | 2 / 2 / 0 | 2 / 0.1361 / 0.2722 | 0 |  |
| 122 | `BG21_HERO_010_Buddy` | Warden Thelwater | 疑似卡牌效果、脚本缺失 | 2 / 6 / 0 | 4 / 0.068 / 0.2722 | 0 |  |
| 123 | `BG26_135` | Southsea Busker | 状态口径缺口、疑似卡牌效果 | 1 / 0 / 0 | 60 / 0.0426 / 1.0 | 7 |  |
| 124 | `BG34_231` | Old Soul | 脚本缺失 | 2 / 5 / 0 | 2 / 0.0078 / 0.0156 | 0 |  |
| 125 | `BG31_859` | Technical Element | 脚本缺失 | 2 / 7 / 0 | — | 0 |  |
| 126 | `BG31_HERO_802pt4` | Immortal | 脚本缺失 | 2 / 8 / 0 | — | 0 |  |
| 127 | `BG31_HERO_802pt7` | Mothership | 脚本缺失 | 2 / 9 / 0 | — | 0 |  |
| 128 | `BG33_840` | Stomping Stegodon | 动态状态、脚本缺失 | 1 / 2 / 1 | — | 0 |  |
| 129 | `BG34_321` | Rabid Panther | 疑似卡牌效果、脚本缺失 | 2 / 10 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 130 | `BG34_Giant_034` | Timewarped Geist | 疑似卡牌效果、脚本缺失 | 2 / 2 / 0 | — | 0 |  |
| 131 | `BG34_Giant_041` | Timewarped Greenskeeper | 脚本缺失 | 2 / 6 / 0 | — | 0 |  |
| 132 | `BG34_Giant_071` | Timewarped Bassgill | 脚本缺失 | 2 / 4 / 0 | — | 0 |  |
| 133 | `BG34_Giant_074t` | Timewarped Nellie's Ship | 疑似卡牌效果、脚本缺失 | 2 / 8 / 0 | — | 0 |  |
| 134 | `BG34_Giant_078` | Timewarped Thorncaller | 脚本缺失 | 2 / 6 / 0 | — | 0 |  |
| 135 | `BG34_Giant_324` | Timewarped Summoner | 疑似卡牌效果、脚本缺失 | 2 / 4 / 0 | — | 0 |  |
| 136 | `BG34_Giant_332` | Timewarped Embalmer | 动态状态、脚本缺失 | 1 / 4 / 1 | — | 0 |  |
| 137 | `BG34_Giant_591` | Timewarped Acolyte | 脚本缺失 | 2 / 6 / 0 | — | 0 |  |
| 138 | `BG34_Giant_678` | Timewarped Lava Lurker | 脚本缺失 | 2 / 5 / 0 | — | 0 |  |
| 139 | `BG35_122` | Determined Defender | 疑似卡牌效果、脚本缺失 | 2 / 2 / 0 | — | 0 |  |
| 140 | `BG35_434` | Hired Ritualist | 脚本缺失 | 2 / 5 / 0 | — | 0 |  |
| 141 | `BG35_340` | Alert Alarmist | 动态状态、疑似卡牌效果 | 2 / 0 / 6 | 8 / 0.1798 / 0.8373 | 0 |  |
| 142 | `BG28_707` | Living Azerite | 动态状态 | 0 / 0 / 2 | 1 / 0.0 / 0.0 | 1 |  |
| 143 | `BG32_821` | Felfire Conjurer | 动态状态、状态口径缺口 | 0 / 0 / 4 | 2 / 0.0 / 0.0 | 1 |  |
| 144 | `BG31_816` | Fire Baller | 动态状态 | 2 / 0 / 4 | 14 / 0.0039 / 0.0499 | 0 |  |
| 145 | `BG34_865` | En-Djinn Blazer | 动态状态 | 2 / 0 / 7 | 3 / 0.0015 / 0.0046 | 0 |  |
| 146 | `BG27_016` | Champion of Sargeras | 动态状态 | 2 / 0 / 3 | — | 0 |  |
| 147 | `BG31_920` | Darkcrest Strategist | 动态状态 | 2 / 0 / 5 | — | 0 |  |
| 148 | `BG33_HERO_001p_ALT` | Heroic Inspiration | — | 3 / 0 / 0 | — | 0 |  |
| 149 | `BG34_175` | Junk Jouster | 动态状态 | 2 / 0 / 4 | — | 0 |  |
| 150 | `BG35_140` | Mama Mrrglton | 动态状态 | 2 / 0 / 3 | — | 0 |  |
| 151 | `BG35_141` | Papa Mrrglton | 动态状态 | 2 / 0 / 11 | — | 0 |  |
| 152 | `TB_BaconShop_HP_052` | Arcane Alteration | — | 3 / 0 / 0 | — | 0 |  |
| 153 | `TB_BaconShop_HP_105` | Avatar of N'Zoth | — | 3 / 0 / 0 | — | 0 |  |
| 154 | `BG33_886` | Tusked Camper | 状态口径缺口、疑似卡牌效果 | 0 / 0 / 0 | 2 / 0.1769 / 0.3125 | 4 |  |
| 155 | `BG32_835` | Tranquil Meditative | 动态状态、已知 TOP/审计名单、疑似卡牌效果 | 1 / 0 / 1 | 6 / 0.4142 / 0.832 | 0 | 已知 TOP |
| 156 | `BG22_HERO_201_Buddy` | Submersible Chef | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 157 | `BG24_005` | Sinrunner Blanchy | 脚本缺失 | 1 / 5 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 158 | `BG25_040` | Blazing Skyfin | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 159 | `BG25_155` | Gem Smuggler | 脚本缺失 | 1 / 3 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 160 | `BG26_360` | Scourfin | 脚本缺失 | 1 / 8 / 0 | — | 0 |  |
| 161 | `BG26_HERO_104_Buddy` | Akali, Rock Rhino | 动态状态、脚本缺失 | 0 / 1 / 1 | — | 0 |  |
| 162 | `BG26_ICC_065` | Bone Baron | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 163 | `BG27_HERO_801_Buddy` | Veranus, Stormlord's Mount | 脚本缺失 | 1 / 6 / 0 | 2 / 0.0 / 0.0 | 0 |  |
| 164 | `BG28_HERO_801_Buddy` | The Nine Frogs | 动态状态、脚本缺失 | 0 / 1 / 1 | — | 0 |  |
| 165 | `BG29_840` | Mooneater's Champion | 脚本缺失 | 1 / 7 / 0 | — | 0 |  |
| 166 | `BG31_178` | Marquee Ticker | 脚本缺失 | 1 / 6 / 0 | — | 0 |  |
| 167 | `BG31_360` | Arcane Behemoth | 脚本缺失 | 1 / 3 / 0 | — | 0 |  |
| 168 | `BG33_111` | Carapace Raiser | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 169 | `BG34_Giant_006` | Timewarped Molten Rock | 动态状态、脚本缺失 | 0 / 1 / 1 | — | 0 |  |
| 170 | `BG34_Giant_015` | Timewarped Hooktail | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 171 | `BG34_Giant_082` | Timewarped Recycler | 脚本缺失 | 1 / 3 / 0 | — | 0 |  |
| 172 | `BG34_Giant_204` | Timewarped Pillager | 脚本缺失 | 1 / 3 / 0 | — | 0 |  |
| 173 | `BG34_Giant_211` | Timewarped Pashmar | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 174 | `BG34_Giant_327` | Timewarped Peggy | 脚本缺失 | 1 / 4 / 0 | 2 / 0.0 / 0.0 | 0 |  |
| 175 | `BG34_Giant_330` | Timewarped Radio Star | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 176 | `BG34_Giant_598` | Timewarped Mothership | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 177 | `BG34_Giant_608` | Timewarped Lil' Quilboar | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 178 | `BG34_Giant_679` | Timewarped Chimera | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 179 | `BG34_Giant_777` | Timewarped Behemoth | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 180 | `BG35_431` | Earthsong Shaman | 脚本缺失 | 1 / 4 / 0 | — | 0 |  |
| 181 | `BG35_700` | Ship Jumper | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 182 | `BG35_895` | Tidecaller Prophet | 动态状态、脚本缺失 | 0 / 1 / 1 | — | 0 |  |
| 183 | `BGS_030` | King Bagurgle | 脚本缺失 | 1 / 4 / 0 | 3 / 0.0 / 0.0 | 0 |  |
| 184 | `TB_BaconShop_HERO_02_Buddy` | Apostle of Galakrond | 脚本缺失 | 1 / 3 / 0 | — | 0 |  |
| 185 | `TB_BaconShop_HERO_16_Buddy` | Snack Vendor | 脚本缺失 | 1 / 3 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 186 | `TB_BaconShop_HERO_57_Buddy` | Chromie | 动态状态、脚本缺失 | 0 / 8 / 1 | — | 0 |  |
| 187 | `TB_BaconShop_HERO_59_Buddy` | Sklibb, Demon Hunter | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 188 | `TB_BaconShop_HERO_702_Buddy` | Mawsworn Soulkeeper | 脚本缺失 | 1 / 2 / 0 | — | 0 |  |
| 189 | `BG35_150` | Laboratory Assistant | 状态口径缺口 | 0 / 0 / 0 | 8 / 0.0168 / 0.131 | 3 |  |
| 190 | `BG31_326` | Gem Rat | 状态口径缺口 | 0 / 0 / 0 | — | 3 |  |
| 191 | `BGFYM_002t` | Aberrant Tentacle | 状态口径缺口 | 0 / 0 / 0 | — | 3 |  |
| 192 | `BG30_129` | Catacomb Crasher | 动态状态、疑似卡牌效果 | 1 / 0 / 15 | 6 / 0.1826 / 0.832 | 0 |  |
| 193 | `BG32_846` | Unleashed Mana Surge | 动态状态、疑似卡牌效果 | 1 / 0 / 5 | 3 / 0.166 / 0.249 | 0 |  |
| 194 | `BG32_236` | Aureate Laureate | 状态口径缺口、疑似卡牌效果 | 0 / 0 / 0 | 38 / 0.0377 / 0.533 | 2 |  |
| 195 | `BGS_115t` | Water Droplet | 疑似卡牌效果 | 2 / 0 / 0 | 13 / 0.0909 / 0.4688 | 1 |  |
| 196 | `BG34_858` | Air Revenant | 动态状态、疑似卡牌效果 | 1 / 0 / 8 | 1 / 0.1509 / 0.1509 | 0 |  |
| 197 | `BG34_920` | Tide Raiser | 疑似卡牌效果 | 2 / 0 / 0 | 6 / 0.089 / 0.2644 | 0 |  |
| 198 | `BG33_920` | Wrathscale Rogue | 疑似卡牌效果 | 2 / 0 / 0 | 3 / 0.083 / 0.249 | 0 |  |
| 199 | `BG_LOE_077` | Brann Bronzebeard | 疑似卡牌效果 | 33 / 0 / 0 | 18 / 0.0204 / 0.1509 | 1 |  |
| 200 | `BG20_104` | Bonker | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 201 | `BG25_022_G` | Scarlet Skull | — | 0 / 0 / 0 | 1 / 0.0 / 0.0 | 1 |  |
| 202 | `BG31_323` | Turbo Hogrider | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 203 | `BG31_326_G` | Gem Rat | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 204 | `BG32_860` | Shoalfin Mystic | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 205 | `BG34_140` | Expert Aviator | 动态状态、状态口径缺口 | 1 / 0 / 3 | 4 / 0.0 / 0.0 | 1 |  |
| 206 | `BG35_881_G` | Leyline Surfacer | — | 0 / 0 / 0 | 1 / 0.0 / 0.0 | 1 |  |
| 207 | `BG36_106_G` | Cutthroat K'Thir | 已知 TOP/审计名单、状态口径缺口 | 0 / 0 / 0 | — | 1 | 已知 TOP |
| 208 | `BG36_109` | The Shadow of Doubt | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 209 | `BG36_113` | Drifting Sacrifice | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 210 | `BG36_115_G` | Nightmare Corroder | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 211 | `BG36_181` | Air Baller | — | 0 / 0 / 0 | 1 / 0.0 / 0.0 | 1 |  |
| 212 | `BG36_318` | Faceless Converter | 已知 TOP/审计名单、状态口径缺口 | 0 / 0 / 0 | — | 1 | 已知 TOP |
| 213 | `BG36_320_G` | Mysterious K'Thir | 已知 TOP/审计名单、状态口径缺口 | 0 / 0 / 0 | — | 1 | 已知 TOP |
| 214 | `BG36_523` | Enterprising Escapee | 已知 TOP/审计名单、状态口径缺口 | 0 / 0 / 0 | — | 1 | 已知 TOP |
| 215 | `BG36_620_G` | Boom-in-a-Box | — | 0 / 0 / 0 | 1 / 0.0 / 0.0 | 1 |  |
| 216 | `BG36_621` | Deft Deserter | — | 0 / 0 / 0 | 1 / 0.0 / 0.0 | 1 |  |
| 217 | `BG36_640` | Gatekeeper Amalgam | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 218 | `BG36_733` | Eredar Escapist | — | 0 / 0 / 0 | 1 / 0.0 / 0.0 | 1 |  |
| 219 | `BGS_104` | Nomi, Kitchen Nightmare | 动态状态、疑似卡牌效果 | 1 / 0 / 1 | 4 / 0.0622 / 0.249 | 0 |  |
| 220 | `BG25_041` | Felemental | 动态状态、疑似卡牌效果 | 1 / 0 / 12 | 8 / 0.0408 / 0.1649 | 0 |  |
| 221 | `BG34_312` | Hardy Orca | 动态状态 | 1 / 0 / 5 | 3 / 0.0241 / 0.0723 | 0 |  |
| 222 | `BG26_810` | Gunpowder Courier | 动态状态、疑似卡牌效果 | 1 / 0 / 14 | 12 / 0.0145 / 0.1737 | 0 |  |
| 223 | `BG32_880` | Friendly Geist | 动态状态 | 1 / 0 / 4 | 11 / 0.0126 / 0.131 | 0 |  |
| 224 | `BG22_HERO_000p_Alt` | Lock and Load | — | 2 / 0 / 0 | — | 0 |  |
| 225 | `BG22_HERO_305p` | Broodmother | — | 2 / 0 / 0 | — | 0 |  |
| 226 | `BG23_004` | Deep-Sea Angler | 动态状态 | 1 / 0 / 1 | — | 0 |  |
| 227 | `BG26_162` | Dancing Barnstormer | 动态状态 | 1 / 0 / 4 | 3 / 0.0 / 0.0 | 0 |  |
| 228 | `BG28_741` | Charging Czarina | 动态状态 | 1 / 0 / 2 | — | 0 |  |
| 229 | `BG31_HERO_801p` | Lift Off | — | 2 / 0 / 0 | — | 0 |  |
| 230 | `BG33_890t` | Magicfin Apprentice | 动态状态 | 1 / 0 / 1 | — | 0 |  |
| 231 | `BG34_635t` | Black Chromadrake | 动态状态 | 1 / 0 / 1 | — | 0 |  |
| 232 | `BG34_HERO_000p` | Alternate Timeline | — | 2 / 0 / 0 | — | 0 |  |
| 233 | `BGS_041` | Kalecgos, Arcane Aspect | 动态状态 | 1 / 0 / 4 | — | 0 |  |
| 234 | `TB_BaconShop_HP_033` | Menagerist | — | 2 / 0 / 0 | — | 0 |  |
| 235 | `TB_BaconShop_HP_053` | I'll Take That! | — | 2 / 0 / 0 | — | 0 |  |
| 236 | `BG32_HERO_002_Buddy` | Zippers | 脚本缺失 | 0 / 1 / 0 | 1 / 0.0 / 0.0 | 1 |  |
| 237 | `BG31_815` | Dune Dweller | 动态状态、疑似卡牌效果 | 0 / 0 / 2 | 29 / 0.0563 / 0.6839 | 10 |  |
| 238 | `BG_TTN_401` | Ancestral Automaton | 动态状态、疑似卡牌效果 | 0 / 0 / 0（skip 12） | 28 / 0.0436 / 0.8373 | 0 |  |
| 239 | `BG20_HERO_100_Buddy` | Icesnarl the Mighty | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 240 | `BG20_HERO_102_Buddy` | Dranosh Saurfang | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 241 | `BG20_HERO_103_Buddy` | Death's Head Sage | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 242 | `BG20_HERO_280_Buddy` | Living Nightmare | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 243 | `BG22_HERO_002_Buddy` | Frostwolf Lieutenant | 脚本缺失 | 0 / 2 / 0 | 3 / 0.0 / 0.0 | 0 |  |
| 244 | `BG25_003` | Relentless Sentry | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 245 | `BG27_513` | Tide Oracle Morgl | 脚本缺失 | 0 / 2 / 0 | — | 0 |  |
| 246 | `BG30_121` | Hot-Air Surveyor | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 247 | `BG30_HERO_304_Buddy` | Fantastic Bellhop | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 248 | `BG31_HERO_003_Buddy` | Outland Sunbeam | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 249 | `BG31_HERO_811t3` | Roach | 脚本缺失 | 0 / 1 / 0 | 2 / 0.0 / 0.0 | 0 |  |
| 250 | `BG32_891` | Shadowdancer | 脚本缺失 | 0 / 7 / 0 | 6 / 0.0 / 0.0 | 0 |  |
| 251 | `BG32_HERO_001_Buddy` | Malorne | 脚本缺失 | 0 / 4 / 0 | — | 0 |  |
| 252 | `BG33_154` | Lord of the Ruins | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 253 | `BG33_888` | Hog Watcher | 脚本缺失 | 0 / 2 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 254 | `BG34_Giant_001` | Timewarped Busker | 脚本缺失 | 0 / 3 / 0 | — | 0 |  |
| 255 | `BG34_Giant_072` | Timewarped Skipper | 脚本缺失 | 0 / 3 / 0 | — | 0 |  |
| 256 | `BG34_Giant_110` | Timewarped Sea Glass | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 257 | `BG34_Giant_210` | Timewarped Commander | 脚本缺失 | 0 / 1 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 258 | `BG34_Giant_212` | Timewarped Archer | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 259 | `BG34_Giant_304` | Timewarped Sapper | 脚本缺失 | 0 / 4 / 0 | 3 / 0.0 / 0.0 | 0 |  |
| 260 | `BG34_Giant_328` | Timewarped Tide Razor | 脚本缺失 | 0 / 1 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 261 | `BG34_Giant_370` | Timewarped Hawkstrider | 脚本缺失 | 0 / 1 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 262 | `BG34_Giant_580` | Timewarped Ragnaros | 脚本缺失 | 0 / 1 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 263 | `BG34_Giant_589` | Timewarped Sailor | 脚本缺失 | 0 / 1 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 264 | `BG34_Giant_590` | Timewarped Festergut | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 265 | `BG34_Giant_595` | Timewarped Tamuzo | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 266 | `BG34_Giant_609` | Timewarped Ghoul-acabra | 脚本缺失 | 0 / 3 / 0 | — | 0 |  |
| 267 | `BG34_Giant_618` | Timewarped Caretaker | 脚本缺失 | 0 / 2 / 0 | — | 0 |  |
| 268 | `BG34_PreMadeChamp_067` | Timewarped Plunderer | 脚本缺失 | 0 / 3 / 0 | 1 / 0.0 / 0.0 | 0 |  |
| 269 | `TB_BaconShop_HERO_08_Buddy` | Eclipsion Illidari | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 270 | `TB_BaconShop_HERO_10_Buddy` | Bilgewater Mogul | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 271 | `TB_BaconShop_HERO_43_Buddy` | Brann's Epic Egg | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 272 | `TB_BaconShop_HERO_45_Buddy` | Loyal Henchman | 脚本缺失 | 0 / 1 / 0 | 3 / 0.0 / 0.0 | 0 |  |
| 273 | `TB_BaconShop_HERO_75_Buddy` | Lantern Tender | 脚本缺失 | 0 / 1 / 0 | — | 0 |  |
| 274 | `BG34_922` | Maelstrom Emergent | 疑似卡牌效果 | 14 / 0 / 0 | 8 / 0.1798 / 0.832 | 0 |  |
| 275 | `BG34_925` | Seafloor Recruiter | 疑似卡牌效果 | 1 / 0 / 0 | 11 / 0.1527 / 0.832 | 0 |  |
| 276 | `BG25_354` | Titus Rivendare | 疑似卡牌效果 | 33 / 0 / 0 | 10 / 0.1473 / 0.832 | 0 |  |
| 277 | `BG25_806` | Sly Raptor | 动态状态、疑似卡牌效果 | 0 / 0 / 1 | 10 / 0.1129 / 0.5702 | 0 |  |
| 278 | `BG35_883` | Balinda Stonehearth | 疑似卡牌效果 | 22 / 0 / 0 | 13 / 0.0881 / 0.3454 | 0 |  |
| 279 | `BG32_330` | Flighty Scout | 状态口径缺口、疑似卡牌效果 | 1 / 0 / 0 | 7 / 0.0245 / 0.1715 | 1 |  |
| 280 | `BG31_818` | Snow Baller | 动态状态、疑似卡牌效果 | 0 / 0 / 1 | 7 / 0.0464 / 0.1713 | 0 |  |
| 281 | `BG25_039` | Pufferquil | — | 1 / 0 / 0 | 5 / 0.0438 / 0.1094 | 0 |  |
| 282 | `BG35_123` | Cataclysmic Harbinger | 疑似卡牌效果 | 1 / 0 / 0 | 8 / 0.0369 / 0.1509 | 0 |  |
| 283 | `BG26_ICC_901` | Drakkari Enchanter | 疑似卡牌效果 | 1 / 0 / 0 | 7 / 0.0319 / 0.1509 | 0 |  |
| 284 | `BG33_823` | Sky Admiral Rogers | 动态状态 | 0 / 0 / 2 | 1 / 0.0156 / 0.0156 | 0 |  |
| 285 | `BG33_140` | River Skipper | 疑似卡牌效果 | 1 / 0 / 0 | 11 / 0.0156 / 0.1715 | 0 |  |
| 286 | `BG32_170` | Metallic Hunter | — | 1 / 0 / 0 | 26 / 0.0022 / 0.0207 | 0 |  |
| 287 | `BGS_071` | Deflect-o-Bot | 动态状态 | 0 / 0 / 5 | 9 / 0.0017 / 0.0156 | 0 |  |
| 288 | `BG20_HERO_283p_t3` | Eastern Plaguelands | — | 1 / 0 / 0 | — | 0 |  |
| 289 | `BG24_HERO_100p` | Whodunit? | — | 1 / 0 / 0 | — | 0 |  |
| 290 | `BG26_505` | Zesty Shaker | — | 1 / 0 / 0 | — | 0 |  |
| 291 | `BG26_814` | Lovesick Balladist | 动态状态 | 0 / 0 / 3 | 3 / 0.0 / 0.0 | 0 |  |
| 292 | `BG31_810` | Ultraviolet Ascendant | 动态状态 | 0 / 0 / 1 | — | 0 |  |
| 293 | `BG32_HERO_002p` | Growing Collection | — | 1 / 0 / 0 | — | 0 |  |
| 294 | `BG34_636t` | Green Chromadrake | 动态状态 | 0 / 0 / 1 | — | 0 |  |
| 295 | `BG34_637t` | Bronze Chromadrake | 动态状态 | 0 / 0 / 2 | — | 0 |  |
| 296 | `BG34_950` | Stone Age Slab | 动态状态 | 0 / 0 / 5 | — | 0 |  |
| 297 | `BG35_152` | Void Pup Trainer | 动态状态 | 0 / 0 / 1 | — | 0 |  |
| 298 | `BG35_602` | Lurking Leviathan | 动态状态 | 0 / 0 / 4 | 1 / 0.0 / 0.0 | 0 |  |
| 299 | `TB_BaconShop_HP_001` | Sharpen Blades | — | 1 / 0 / 0 | — | 0 |  |
| 300 | `TB_BaconShop_HP_035` | All Patched Up | — | 1 / 0 / 0 | — | 0 |  |
| 301 | `TB_BaconShop_HP_048` | Battle Brand | — | 1 / 0 / 0 | — | 0 |  |
| 302 | `BG21_HERO_030t` | Sneed's New Shredder | 疑似卡牌效果 | 0 / 0 / 0 | 1 / 0.6839 / 0.6839 | 1 |  |
| 303 | `BG34_694` | Wintergrasp Ghoul | 已知 TOP/审计名单、疑似卡牌效果 | 0 / 0 / 0 | 4 / 0.499 / 0.832 | 0 | 已知 TOP |
| 304 | `BG35_432` | Bristleback Bully | 已知 TOP/审计名单、疑似卡牌效果 | 0 / 0 / 0 | 3 / 0.4925 / 0.6875 | 0 | 已知 TOP |
| 305 | `BG34_926` | Ruthless Queensguard | 状态口径缺口、疑似卡牌效果 | 0 / 0 / 0 | 6 / 0.4016 / 0.832 | 0 |  |
| 306 | `BGS_034` | Bronze Warden | 疑似卡牌效果 | 0 / 0 / 0 | 5 / 0.137 / 0.6839 | 4 |  |
| 307 | `BG34_635_Gt` | Black Chromadrake | 疑似卡牌效果 | 0 / 0 / 0 | 4 / 0.3656 / 0.6875 | 0 |  |
| 308 | `BG21_015` | Tarecgosa | 疑似卡牌效果 | 0 / 0 / 0 | 26 / 0.1422 / 0.8373 | 3 |  |
| 309 | `BG32_235` | Surfing Sylvar | 状态口径缺口、疑似卡牌效果 | 0 / 0 / 0 | 5 / 0.2668 / 0.533 | 1 |  |
| 310 | `BG28_309` | Mummifier | 疑似卡牌效果 | 0 / 0 / 0 | 5 / 0.3166 / 0.832 | 0 |  |
| 311 | `BG34_636_Gt` | Green Chromadrake | 疑似卡牌效果 | 0 / 0 / 0 | 2 / 0.278 / 0.556 | 0 |  |
| 312 | `BG36_345` | Suspicious Prisonguard | 状态口径缺口 | 0 / 0 / 0 | 3 / 0.0 / 0.0 | 4 |  |
| 313 | `BG24_500` | Amber Guardian | 疑似卡牌效果 | 0 / 0 / 0 | 6 / 0.1802 / 0.4825 | 1 |  |
| 314 | `BG26_501` | Reef Riffer | 疑似卡牌效果 | 0 / 0 / 0 | 2 / 0.2412 / 0.4825 | 0 |  |
| 315 | `BG25_022` | Scarlet Skull | 疑似卡牌效果 | 0 / 0 / 0 | 13 / 0.1148 / 0.6839 | 2 |  |
| 316 | `BG34_604` | Heroic Underdog | 疑似卡牌效果 | 0 / 0 / 0 | 7 / 0.04 / 0.2362 | 3 |  |
| 317 | `BG33_821` | Shipwrecked Rascal | 疑似卡牌效果 | 0 / 0 / 0 | 4 / 0.2256 / 0.4825 | 0 |  |
| 318 | `BG26_802` | Banana Slamma | 疑似卡牌效果 | 0 / 0 / 0 | 4 / 0.2132 / 0.8373 | 0 |  |
| 319 | `BG26_524_G` | Malchezaar, Prince of Dance | 疑似卡牌效果 | 0 / 0 / 0 | 3 / 0.0787 / 0.2362 | 2 |  |
| 320 | `BG36_354` | Decoy Conjurer | 疑似卡牌效果 | 0 / 0 / 0 | 3 / 0.0787 / 0.2362 | 2 |  |
| 321 | `BG31_330` | Ominous Seer | 状态口径缺口、疑似卡牌效果 | 0 / 0 / 0 | 39 / 0.0723 / 1.0 | 2 |  |
| 322 | `BG36_342` | Clever Castaway | 状态口径缺口 | 0 / 0 / 0 | 3 / 0.0 / 0.0 | 3 |  |
| 323 | `BG29_611` | Cord Puller | 疑似卡牌效果 | 0 / 0 / 0 | 48 / 0.0548 / 1.0 | 2 |  |
| 324 | `BG27_084` | Sprightly Scarab | 疑似卡牌效果 | 0 / 0 / 0 | 4 / 0.1797 / 0.3594 | 0 |  |
| 325 | `BG31_999` | Stitched Salvager | 疑似卡牌效果 | 0 / 0 / 0 | 7 / 0.1737 / 0.832 | 0 |  |
| 326 | `BG26_525` | Imposing Percussionist | 疑似卡牌效果 | 0 / 0 / 0 | 5 / 0.0993 / 0.3454 | 1 |  |
| 327 | `BG34_682` | Razorfen Flapper | 疑似卡牌效果 | 0 / 0 / 0 | 5 / 0.153 / 0.556 | 0 |  |
| 328 | `BG35_814` | Scarlet Survivor | 疑似卡牌效果 | 0 / 0 / 0 | 45 / 0.0828 / 0.6426 | 1 |  |
| 329 | `BG26_502` | Deep Blue Crooner | 疑似卡牌效果 | 0 / 0 / 0 | 8 / 0.1406 / 0.5702 | 0 |  |
| 330 | `BGS_115` | Sellemental | 疑似卡牌效果 | 0 / 0 / 0 | 13 / 0.1406 / 1.0 | 0 |  |
| 331 | `BG32_237` | Intrepid Botanist | 疑似卡牌效果 | 0 / 0 / 0 | 11 / 0.137 / 0.3594 | 0 |  |
| 332 | `BG34_632` | Incubation Researcher | 疑似卡牌效果 | 0 / 0 / 0 | 7 / 0.1322 / 0.8373 | 0 |  |
| 333 | `BG31_327` | Thorned Trailblazer | 状态口径缺口 | 0 / 0 / 0 | — | 2 |  |
| 334 | `BG26_146` | Lullabot | 疑似卡牌效果 | 0 / 0 / 0 | 15 / 0.0394 / 0.2479 | 1 |  |
| 335 | `BG26_817` | Blade Collector | 状态口径缺口、疑似卡牌效果 | 0 / 0 / 0 | 16 / 0.0391 / 0.1737 | 1 |  |
| 336 | `BG29_813` | Persistent Poet | 疑似卡牌效果 | 15 / 0 / 0 | 6 / 0.0972 / 0.257 | 0 |  |
| 337 | `BGS_116` | Refreshing Anomaly | 疑似卡牌效果 | 0 / 0 / 0 | 3 / 0.083 / 0.249 | 0 |  |
| 338 | `BG28_300` | Harmless Bonehead | 疑似卡牌效果 | 0 / 0 / 0 | 28 / 0.0123 / 0.1715 | 1 |  |
| 339 | `BG34_630` | Twilight Hatchling | 疑似卡牌效果 | 15 / 0 / 0 | 78 / 0.0745 / 1.0 | 0 |  |
| 340 | `BG26_199` | Upbeat Duo | 疑似卡牌效果 | 0 / 0 / 0 | 7 / 0.0739 / 0.3454 | 0 |  |
| 341 | `BG22_202` | Tad | 疑似卡牌效果 | 0 / 0 / 0 | 3 / 0.0735 / 0.1715 | 0 |  |
| 342 | `BGS_049` | Freedealing Gambler | 疑似卡牌效果 | 0 / 0 / 0 | 10 / 0.0719 / 0.3594 | 0 |  |
| 343 | `BG35_155` | Twisted Wrathguard | 疑似卡牌效果 | 0 / 0 / 0 | 6 / 0.0677 / 0.2722 | 0 |  |
| 344 | `BG24_009` | Picky Eater | 疑似卡牌效果 | 0 / 0 / 0 | 23 / 0.0666 / 0.4825 | 0 |  |
| 345 | `BG24_715` | Patient Scout | 疑似卡牌效果 | 0 / 0 / 0 | 31 / 0.0653 / 0.556 | 0 |  |
| 346 | `BG20_101` | Roadboar | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 347 | `BG31_320` | Crater Miner | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 348 | `BG33_886_G` | Tusked Camper | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 349 | `BG34_140_G` | Expert Aviator | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 350 | `BG36_100` | Wandering Willbreaker | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 351 | `BG36_110` | Joyous | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 352 | `BG36_116` | Underrot Spawn | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 353 | `BG36_342_G` | Clever Castaway | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 354 | `BG36_763` | Treasure Parrot | 状态口径缺口 | 0 / 0 / 0 | — | 1 |  |
| 355 | `BG23_002` | Shell Collector | 疑似卡牌效果 | 0 / 0 / 0 | 15 / 0.0623 / 0.3454 | 0 |  |
| 356 | `BG34_921` | Ring Bearer | 疑似卡牌效果 | 18 / 0 / 0 | 7 / 0.061 / 0.249 | 0 |  |
| 357 | `BG26_801` | Rylak Metalhead | 疑似卡牌效果 | 24 / 0 / 0 | 26 / 0.0584 / 0.257 | 0 |  |
| 358 | `BG33_822` | Bigwig Bandit | 疑似卡牌效果 | 0 / 0 / 0 | 9 / 0.0536 / 0.4825 | 0 |  |
| 359 | `BG30_125` | Cadaver Caretaker | 疑似卡牌效果 | 17 / 0 / 0 | 27 / 0.0528 / 0.832 | 0 |  |
| 360 | `BGS_126` | Wildfire Elemental | 疑似卡牌效果 | 0 / 0 / 0 | 9 / 0.0521 / 0.4688 | 0 |  |
| 361 | `BGS_078` | Monstrous Macaw | 疑似卡牌效果 | 23 / 0 / 0 | 13 / 0.0506 / 0.257 | 0 |  |
| 362 | `BG33_820` | Lost City Looter | 疑似卡牌效果 | 0 / 0 / 0 | 19 / 0.0441 / 0.8373 | 0 |  |
| 363 | `BG20_100` | Razorfen Geomancer | 疑似卡牌效果 | 0 / 0 / 0 | 34 / 0.0413 / 0.533 | 0 |  |
| 364 | `BG27_005` | Timecap'n Hooktail | 疑似卡牌效果 | 0 / 0 / 0 | 17 / 0.0408 / 0.556 | 0 |  |
| 365 | `BG26_529` | Upbeat Frontdrake | 疑似卡牌效果 | 0 / 0 / 0 | 25 / 0.0401 / 0.556 | 0 |  |
| 366 | `BG21_018` | Defiant Shipwright | 疑似卡牌效果 | 0 / 0 / 0 | 19 / 0.0333 / 0.2479 | 0 |  |
| 367 | `BG25_001` | Risen Rider | 疑似卡牌效果 | 0 / 0 / 0 | 30 / 0.0328 / 0.4688 | 0 |  |
| 368 | `BG33_825` | Proud Privateer | 疑似卡牌效果 | 0 / 0 / 0 | 6 / 0.0316 / 0.1737 | 0 |  |
| 369 | `BG26_147` | Accord-o-Tron | 疑似卡牌效果 | 0 / 0 / 0 | 12 / 0.031 / 0.2722 | 0 |  |
| 370 | `BG32_430` | Glowgullet Warlord | 疑似卡牌效果 | 0 / 0 / 0 | 16 / 0.0296 / 0.1649 | 0 |  |

## 2. 重点卡证据明细

### 1. `BG26_174` Soul Rewinder

- 卡面：After your hero takes damage, rewind it and give this +{2} Health.
- 类别：C 类证据不足, 动态状态, 已知 TOP/审计名单, 疑似卡牌效果；679 分歧 16、缺脚本 0、动态 45
- 679 样本：`10a2e4c1efcb29fa` distribution_distance official=loss L1=0.290625；`1779cc01ecb9c134` official_result_unreachable official=win L1=None；`3f283d47969121dc` distribution_distance official=tie L1=0.23562499999999997
- BB 高 TVD 状态：state12 turn8 TVD=0.6839 engine={'win': 0.078125, 'tie': 0.34375, 'loss': 0.578125} oracle={'win': 0.7620000243186951, 'tie': 0.15199999511241913, 'loss': 0.0860000029206276}；state12 turn8 TVD=0.6839 engine={'win': 0.078125, 'tie': 0.34375, 'loss': 0.578125} oracle={'win': 0.7620000243186951, 'tie': 0.15199999511241913, 'loss': 0.0860000029206276}；state148 turn8 TVD=0.5796 engine={'win': 0.203125, 'tie': 0.15625, 'loss': 0.640625} oracle={'win': 0.7820000052452087, 'tie': 0.15700000524520874, 'loss': 0.061000000685453415}
- 真实 trace：`9fc29dc3c568763e` turn6 official=win status=MATCH；`b35b5f02a9479705` turn10 official=win status=MATCH；`cc3df395f76f94b8` turn8 official=win status=MATCH

### 2. `BG31_801` Forest Rover

- 卡面：[x]<b>Battlecry:</b> Your Beetles have +{2}/+{3} this game. <b>Deathrattle:</b> Summon a {0}/{1} Beetle.
- 类别：动态状态, 疑似卡牌效果；679 分歧 14、缺脚本 0、动态 47
- 679 样本：`10a2e4c1efcb29fa` distribution_distance official=loss L1=0.290625；`14730b2d0fc661f0` deterministic_mismatch official=loss L1=None；`2ffd4d12136ed1b9` deterministic_mismatch official=win L1=None
- BB 高 TVD 状态：state27 turn4 TVD=0.3594 engine={'win': 0.359375, 'tie': 0.640625, 'loss': 0.0} oracle={'win': 0.0, 'tie': 0.9210000038146973, 'loss': 0.07900000363588333}；state279 turn4 TVD=0.3594 engine={'win': 0.359375, 'tie': 0.640625, 'loss': 0.0} oracle={'win': 0.0, 'tie': 0.9039999842643738, 'loss': 0.09600000083446503}；state35 turn6 TVD=0.2034 engine={'win': 0.359375, 'tie': 0.234375, 'loss': 0.40625} oracle={'win': 0.15600000321865082, 'tie': 0.3059999942779541, 'loss': 0.5379999876022339}
- **无真实 Power.log trace → 仅单测/合成证据**

### 3. `BG31_171` Moonsteel Juggernaut

- 卡面：[x]At the end of your turn, get two {1}/{1} <b>Magnetic</b> Satellites and improve this.
- 类别：动态状态, 已知 TOP/审计名单；679 分歧 12、缺脚本 0、动态 32
- 679 样本：`00cfafe3224325eb` distribution_distance official=tie L1=0.5792499999999999；`0311764c97bfa869` distribution_distance official=None L1=0.19299999999999998；`552dc72592af31c7` distribution_distance official=win L1=0.20599999999999996
- **无真实 Power.log trace → 仅单测/合成证据**

### 4. `BG33_156` Silent Enforcer

- 卡面：[x]<b>Taunt</b> <b>Deathrattle:</b> Deal {0} damage to all minions <i>(except friendly Demons)</i>.
- 类别：疑似卡牌效果, 脚本缺失；679 分歧 11、缺脚本 26、动态 0
- 679 样本：`117eaf818ca6f8ba` deterministic_mismatch official=None L1=None；`33f759e96742ba38` deterministic_mismatch official=loss L1=None；`88d0dcb51ebe6836` distribution_distance official=win L1=0.522125
- BB 高 TVD 状态：state148 turn8 TVD=0.5796 engine={'win': 0.203125, 'tie': 0.15625, 'loss': 0.640625} oracle={'win': 0.7820000052452087, 'tie': 0.15700000524520874, 'loss': 0.061000000685453415}；state142 turn7 TVD=0.3454 engine={'win': 0.046875, 'tie': 0.09375, 'loss': 0.859375} oracle={'win': 0.11299999803304672, 'tie': 0.37299999594688416, 'loss': 0.5139999985694885}；state234 turn7 TVD=0.1513 engine={'win': 0.03125, 'tie': 0.09375, 'loss': 0.875} oracle={'win': 0.02800000086426735, 'tie': 0.24500000476837158, 'loss': 0.7269999980926514}
- **无真实 Power.log trace → 仅单测/合成证据**

### 5. `BG31_826` Doubloon Grifter

- 卡面：Whenever you get 4 Pirates, increase your maximum Gold by 1. <i>(@ left!)</i>
- 类别：动态状态, 状态口径缺口, 疑似卡牌效果, 脚本缺失；679 分歧 10、缺脚本 21、动态 11
- 679 样本：`10a2e4c1efcb29fa` distribution_distance official=loss L1=0.290625；`224536d3760c859f` distribution_distance official=win L1=0.31575；`473eee566f1f1350` deterministic_mismatch official=win L1=None
- BB 高 TVD 状态：state35 turn6 TVD=0.2034 engine={'win': 0.359375, 'tie': 0.234375, 'loss': 0.40625} oracle={'win': 0.15600000321865082, 'tie': 0.3059999942779541, 'loss': 0.5379999876022339}；state199 turn9 TVD=0.0335 engine={'win': 0.9375, 'tie': 0.03125, 'loss': 0.03125} oracle={'win': 0.9710000157356262, 'tie': 0.028999999165534973, 'loss': 0.0}；state73 turn9 TVD=0.0156 engine={'win': 0.015625, 'tie': 0.0, 'loss': 0.984375} oracle={'win': 0.0, 'tie': 0.0, 'loss': 1.0}
- 真实 trace：`cc299c64ccb53245` turn14 official=loss status=MATCH

### 6. `BG34_Giant_031` Timewarped Leapfrogger

- 卡面：[x]<b>Taunt</b>, <b>Reborn</b> <b>Deathrattle:</b> Give a friendly Beast +1/+1 and this <b>Deathrattle</b>.
- 类别：疑似卡牌效果, 脚本缺失；679 分歧 11、缺脚本 21、动态 0
- 679 样本：`0737059f4cd15cc1` distribution_distance official=loss L1=0.16674999999999998；`14730b2d0fc661f0` deterministic_mismatch official=loss L1=None；`19eb90cad2ae4ea5` distribution_distance official=None L1=0.875
- BB 高 TVD 状态：state44 turn12 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 7. `BG33_240` Charmwing

- 卡面：[x]<b>Rally:</b> Give {0} friendly Dragons this minion's maximum Health <i>(except Charmwing)</i>.
- 类别：疑似卡牌效果, 脚本缺失；679 分歧 10、缺脚本 24、动态 0
- 679 样本：`04b4fd3fb79492da` deterministic_mismatch official=loss L1=None；`19eb90cad2ae4ea5` distribution_distance official=None L1=0.875；`38868db1b6096423` distribution_distance official=None L1=0.9
- BB 高 TVD 状态：state41 turn10 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}；state84 turn13 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}；state151 turn11 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 8. `BG23_009` Lava Lurker

- 卡面：[x]The first <b>Spellcraft</b> spell _played from hand on this each turn is permanent. <i>({0} left!)</i>
- 类别：动态状态, 疑似卡牌效果；679 分歧 10、缺脚本 0、动态 1
- 679 样本：`0a15c937184cd400` distribution_distance official=None L1=0.743；`2ae1486d2351af05` distribution_distance official=None L1=0.17612499999999998；`350d52399313c120` distribution_distance official=win L1=0.862875
- BB 高 TVD 状态：state89 turn4 TVD=0.1713 engine={'win': 0.09375, 'tie': 0.515625, 'loss': 0.390625} oracle={'win': 0.26499998569488525, 'tie': 0.45399999618530273, 'loss': 0.2809999883174896}；state89 turn4 TVD=0.1713 engine={'win': 0.09375, 'tie': 0.515625, 'loss': 0.390625} oracle={'win': 0.26499998569488525, 'tie': 0.45399999618530273, 'loss': 0.2809999883174896}；state251 turn4 TVD=0.1535 engine={'win': 0.109375, 'tie': 0.453125, 'loss': 0.4375} oracle={'win': 0.25099998712539673, 'tie': 0.4650000035762787, 'loss': 0.2840000092983246}
- **无真实 Power.log trace → 仅单测/合成证据**

### 9. `BG31_035` Groundbreaker

- 卡面：[x]After you play a Naga, gain +{1}/+{1}. <i>(Improved by every 3 spells you've cast this game!)</i>@[x]After you play a Naga, gain +{1}/+{1}. <i>(Cast {2}/3 sp
- 类别：C 类证据不足, 动态状态, 已知 TOP/审计名单, 疑似卡牌效果；679 分歧 10、缺脚本 0、动态 25
- 679 样本：`4bfedbc1b864dd90` distribution_distance official=None L1=0.956；`5e2c5730b0095944` deterministic_mismatch official=None L1=None；`61213b73fc54a1c1` distribution_distance official=None L1=0.48324999999999996
- BB 高 TVD 状态：state147 turn12 TVD=0.249 engine={'win': 0.75, 'tie': 0.15625, 'loss': 0.09375} oracle={'win': 0.9990000128746033, 'tie': 0.0010000000474974513, 'loss': 0.0}；state34 turn11 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}；state74 turn13 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 10. `BG28_583` Geomagus Roogug

- 卡面：[x]<b>Divine Shield</b>. Whenever a <b>Blood Gem</b> is played on this, this plays a <b>Blood Gem</b> on a __different friendly minion.
- 类别：已知 TOP/审计名单, 疑似卡牌效果, 脚本缺失；679 分歧 9、缺脚本 26、动态 0
- 679 样本：`1d1f2e7dd0142b98` deterministic_mismatch official=win L1=None；`7fab0ab66926a413` distribution_distance official=None L1=0.65875；`a966c70501b3e4b6` deterministic_mismatch official=None L1=None
- BB 高 TVD 状态：state49 turn6 TVD=0.556 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 0.4440000057220459, 'tie': 0.421999990940094, 'loss': 0.1340000033378601}；state91 turn15 TVD=0.125 engine={'win': 0.09375, 'tie': 0.03125, 'loss': 0.875} oracle={'win': 0.0, 'tie': 0.0, 'loss': 1.0}；state42 turn8 TVD=0.0995 engine={'win': 0.890625, 'tie': 0.0625, 'loss': 0.046875} oracle={'win': 0.7960000038146973, 'tie': 0.16200000047683716, 'loss': 0.041999999433755875}
- **无真实 Power.log trace → 仅单测/合成证据**

### 11. `BGS_004` Wrath Weaver

- 卡面：After you play a Demon, deal 1 damage to your hero and gain +{0}/+{1}.
- 类别：C 类证据不足, 动态状态, 已知 TOP/审计名单, 疑似卡牌效果；679 分歧 9、缺脚本 0、动态 40
- 679 样本：`33f759e96742ba38` deterministic_mismatch official=loss L1=None；`3f283d47969121dc` distribution_distance official=tie L1=0.23562499999999997；`4806bdded859c936` distribution_distance official=win L1=0.240875
- BB 高 TVD 状态：state127 turn4 TVD=0.2644 engine={'win': 0.609375, 'tie': 0.359375, 'loss': 0.03125} oracle={'win': 0.3449999988079071, 'tie': 0.4909999966621399, 'loss': 0.164000004529953}；state247 turn4 TVD=0.1603 engine={'win': 0.90625, 'tie': 0.09375, 'loss': 0.0} oracle={'win': 0.7459999918937683, 'tie': 0.1979999989271164, 'loss': 0.0560000017285347}；state185 turn12 TVD=0.131 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 0.8690000176429749, 'tie': 0.06599999964237213, 'loss': 0.06499999761581421}
- 真实 trace：`d2c649a75f48a0b4` turn12 official=None status=MATCH；`3da1958066407ef1` turn14 official=loss status=MATCH；`3da1958066407ef1` turn14 official=loss status=MATCH

### 12. `BG34_781` Felhorn

- 卡面：<b>Battlecry:</b> Give your other Demons and Beasts +{0}/+{1} and deal 1 damage to them, twice.
- 类别：疑似卡牌效果, 脚本缺失；679 分歧 9、缺脚本 23、动态 0
- 679 样本：`2a6b51ef5638b7bc` distribution_distance official=None L1=0.947；`473eee566f1f1350` deterministic_mismatch official=win L1=None；`86c7e93438096704` distribution_distance official=None L1=0.949
- BB 高 TVD 状态：state73 turn9 TVD=0.0156 engine={'win': 0.015625, 'tie': 0.0, 'loss': 0.984375} oracle={'win': 0.0, 'tie': 0.0, 'loss': 1.0}；state141 turn11 TVD=0.004 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 0.9959999918937683, 'tie': 0.0020000000949949026, 'loss': 0.0020000000949949026}；state51 turn12 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 13. `TB_BaconShop_HERO_33_Buddy` Mishmash

- 卡面：Whenever your Amalgam gains stats, this gains them too.
- 类别：疑似卡牌效果, 脚本缺失；679 分歧 9、缺脚本 20、动态 0
- 679 样本：`04b4fd3fb79492da` deterministic_mismatch official=loss L1=None；`2a6b51ef5638b7bc` distribution_distance official=None L1=0.947；`2b21f9e1f97672c9` distribution_distance official=None L1=0.337
- BB 高 TVD 状态：state51 turn12 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}；state51 turn12 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}；state264 turn12 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 14. `BG32_822` Fire-forged Evoker

- 卡面：[x]<b>Start of Combat:</b> Give your Dragons +{0}/+{1}. Improves permanently after ____you cast a Tavern spell.
- 类别：C 类证据不足, 动态状态, 已知 TOP/审计名单；679 分歧 9、缺脚本 0、动态 21
- 679 样本：`19eb90cad2ae4ea5` distribution_distance official=None L1=0.875；`1e097a1db45427b6` distribution_distance official=None L1=0.8809999880999988；`336f2ed562a5eb03` distribution_distance official=None L1=0.20499999999999996
- BB 高 TVD 状态：state84 turn13 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}；state84 turn13 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}；state151 turn11 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 15. `BG29_815` Nightbane, Ignited

- 卡面：<b>Taunt</b> <b>Deathrattle:</b> Give {0} different friendly |4(minion, minions) this minion's Attack.
- 类别：疑似卡牌效果, 脚本缺失；679 分歧 8、缺脚本 19、动态 0
- 679 样本：`09168073d1c26621` distribution_distance official=None L1=0.63；`0a36546bf7a9fbd7` distribution_distance official=win L1=0.801125；`16ee52c5288df010` official_result_unreachable official=tie L1=None
- BB 高 TVD 状态：state155 turn8 TVD=0.257 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 0.7429999709129333, 'tie': 0.15399999916553497, 'loss': 0.10300000011920929}；state29 turn8 TVD=0.238 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 0.7620000243186951, 'tie': 0.13099999725818634, 'loss': 0.10700000077486038}
- **无真实 Power.log trace → 仅单测/合成证据**

### 16. `BG25_008` Eternal Knight

- 卡面：[x]Has +{0}/+{1} for each friendly Eternal Knight that died this __game <i>(wherever this is)</i>.
- 类别：动态状态, 疑似卡牌效果；679 分歧 17、缺脚本 0、动态 14、adapter_skip 28
- 679 样本：`0a36546bf7a9fbd7` distribution_distance official=win L1=0.801125；`2b21f9e1f97672c9` distribution_distance official=None L1=0.337；`9bd6f79a79e28edd` deterministic_mismatch official=win L1=None
- BB 高 TVD 状态：state12 turn8 TVD=0.6839 engine={'win': 0.078125, 'tie': 0.34375, 'loss': 0.578125} oracle={'win': 0.7620000243186951, 'tie': 0.15199999511241913, 'loss': 0.0860000029206276}；state12 turn8 TVD=0.6839 engine={'win': 0.078125, 'tie': 0.34375, 'loss': 0.578125} oracle={'win': 0.7620000243186951, 'tie': 0.15199999511241913, 'loss': 0.0860000029206276}；state148 turn8 TVD=0.5796 engine={'win': 0.203125, 'tie': 0.15625, 'loss': 0.640625} oracle={'win': 0.7820000052452087, 'tie': 0.15700000524520874, 'loss': 0.061000000685453415}
- 真实事件级首分歧：`bb025184419af723` [damage_target] idx=0 official={'seq': 0, 'kind': 'damage', 'actor_slot': None, 'target_slot': None, 'actor_side': None, 'target_side': 'opponent', 'amount': 3, 'card_id': 'BG25_022_G', 'effect_card_id': None} engine={'kind': 'damage', 'target_side': 'opponent', 'target_slot': 0, 'amount': 3}

### 17. `BG34_Giant_208` Timewarped Pagle

- 卡面：[x]Once per combat, when this attacks and kills a minion, get a Triple Reward.
- 类别：疑似卡牌效果, 脚本缺失；679 分歧 8、缺脚本 23、动态 0
- 679 样本：`1d1f2e7dd0142b98` deterministic_mismatch official=win L1=None；`58896431a5f51a36` distribution_distance official=None L1=0.817875010399999；`d4e77edaab8b0472` distribution_distance official=win L1=0.16137500000000002
- BB 高 TVD 状态：state42 turn8 TVD=0.0995 engine={'win': 0.890625, 'tie': 0.0625, 'loss': 0.046875} oracle={'win': 0.7960000038146973, 'tie': 0.16200000047683716, 'loss': 0.041999999433755875}
- **无真实 Power.log trace → 仅单测/合成证据**

### 18. `BG20_203` Prophet of the Boar

- 卡面：<b>Taunt</b> After you play a Quilboar, get a <b>Blood Gem</b>.
- 类别：疑似卡牌效果, 脚本缺失；679 分歧 8、缺脚本 23、动态 0
- 679 样本：`1d1f2e7dd0142b98` deterministic_mismatch official=win L1=None；`350d52399313c120` distribution_distance official=win L1=0.862875；`46e83a29a29eef7a` distribution_distance official=loss L1=0.501
- BB 高 TVD 状态：state123 turn5 TVD=0.1066 engine={'win': 0.1875, 'tie': 0.328125, 'loss': 0.484375} oracle={'win': 0.1420000046491623, 'tie': 0.2669999897480011, 'loss': 0.5910000205039978}；state42 turn8 TVD=0.0995 engine={'win': 0.890625, 'tie': 0.0625, 'loss': 0.046875} oracle={'win': 0.7960000038146973, 'tie': 0.16200000047683716, 'loss': 0.041999999433755875}；state157 turn4 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 19. `BG34_765` Dead Sea Ravager

- 卡面：<b>Rally:</b> Give {0} other friendly minions this minion's Attack.
- 类别：动态状态, 疑似卡牌效果, 脚本缺失；679 分歧 7、缺脚本 21、动态 10
- 679 样本：`34fd102a22596a99` distribution_distance official=None L1=0.263875；`4bfedbc1b864dd90` distribution_distance official=None L1=0.956；`8ae720817fa470df` deterministic_mismatch official=None L1=None
- BB 高 TVD 状态：state85 turn9 TVD=0.1737 engine={'win': 0.046875, 'tie': 0.171875, 'loss': 0.78125} oracle={'win': 0.007000000216066837, 'tie': 0.03799999877810478, 'loss': 0.9549999833106995}；state187 turn9 TVD=0.0 engine={'win': 0.0, 'tie': 0.0, 'loss': 1.0} oracle={'win': 0.0, 'tie': 0.0, 'loss': 1.0}；state205 turn11 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 20. `BG29_807` Trigore the Lasher

- 卡面：[x]Whenever another friendly Beast takes damage, gain +{1} Health permanently.
- 类别：动态状态, 疑似卡牌效果, 脚本缺失；679 分歧 7、缺脚本 15、动态 1
- 679 样本：`10a2e4c1efcb29fa` distribution_distance official=loss L1=0.290625；`2ffd4d12136ed1b9` deterministic_mismatch official=win L1=None；`473eee566f1f1350` deterministic_mismatch official=win L1=None
- BB 高 TVD 状态：state35 turn6 TVD=0.2034 engine={'win': 0.359375, 'tie': 0.234375, 'loss': 0.40625} oracle={'win': 0.15600000321865082, 'tie': 0.3059999942779541, 'loss': 0.5379999876022339}；state58 turn8 TVD=0.1094 engine={'win': 0.890625, 'tie': 0.046875, 'loss': 0.0625} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}；state58 turn8 TVD=0.1094 engine={'win': 0.890625, 'tie': 0.046875, 'loss': 0.0625} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 21. `BG32_204` Silky Shimmermoth

- 卡面：[x]Whenever this takes damage, your Beetles have +{2}/+{3} this game. <b>Deathrattle:</b> Summon a {0}/{1} Beetle.
- 类别：动态状态, 疑似卡牌效果, 脚本缺失；679 分歧 7、缺脚本 13、动态 7
- 679 样本：`2a6b51ef5638b7bc` distribution_distance official=None L1=0.947；`3fc36992db4aac6b` distribution_distance official=loss L1=0.7165；`86c7e93438096704` distribution_distance official=None L1=0.949
- BB 高 TVD 状态：state141 turn11 TVD=0.004 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 0.9959999918937683, 'tie': 0.0020000000949949026, 'loss': 0.0020000000949949026}；state141 turn11 TVD=0.004 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 0.9959999918937683, 'tie': 0.0020000000949949026, 'loss': 0.0020000000949949026}；state51 turn12 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 22. `BG29_862` Barrens Conjurer

- 卡面：<b>Deathrattle:</b> Get a random <b>Battlecry</b> minion.
- 类别：疑似卡牌效果, 脚本缺失；679 分歧 8、缺脚本 18、动态 0
- 679 样本：`8820bc016fae345c` distribution_distance official=loss L1=0.23912500000000003；`9bd6f79a79e28edd` deterministic_mismatch official=win L1=None；`ad38e5f9097c2731` deterministic_mismatch official=win L1=None
- BB 高 TVD 状态：state41 turn10 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}；state169 turn7 TVD=0.0 engine={'win': 0.0, 'tie': 0.0, 'loss': 1.0} oracle={'win': 0.0, 'tie': 0.0, 'loss': 1.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 23. `BG34_Giant_314` Timewarped Poet

- 卡面：[x]<b>Divine Shield</b> All your Dragons keep <b><b>Bonus Keyword</b>s</b> and stats gained in combat.
- 类别：疑似卡牌效果, 脚本缺失；679 分歧 8、缺脚本 19、动态 0
- 679 样本：`04b4fd3fb79492da` deterministic_mismatch official=loss L1=None；`16ee52c5288df010` official_result_unreachable official=tie L1=None；`1e097a1db45427b6` distribution_distance official=None L1=0.8809999880999988
- BB 高 TVD 状态：state84 turn13 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}；state151 turn11 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}；state151 turn11 TVD=0.0 engine={'win': 1.0, 'tie': 0.0, 'loss': 0.0} oracle={'win': 1.0, 'tie': 0.0, 'loss': 0.0}
- **无真实 Power.log trace → 仅单测/合成证据**

### 24. `BG33_155` Devout Hellcaller

- 卡面：After another friendly Demon deals damage, gain +{0}/+{1} permanently.
- 类别：C 类证据不足, 动态状态, 已知 TOP/审计名单, 疑似卡牌效果；679 分歧 8、缺脚本 0、动态 21
- 679 样本：`117eaf818ca6f8ba` deterministic_mismatch official=None L1=None；`8c03a24ee5d61622` distribution_distance official=loss L1=0.671；`97bf220091c5bdf9` distribution_distance official=loss L1=0.61225
- BB 高 TVD 状态：state148 turn8 TVD=0.5796 engine={'win': 0.203125, 'tie': 0.15625, 'loss': 0.640625} oracle={'win': 0.7820000052452087, 'tie': 0.15700000524520874, 'loss': 0.061000000685453415}；state148 turn8 TVD=0.5796 engine={'win': 0.203125, 'tie': 0.15625, 'loss': 0.640625} oracle={'win': 0.7820000052452087, 'tie': 0.15700000524520874, 'loss': 0.061000000685453415}；state142 turn7 TVD=0.3454 engine={'win': 0.046875, 'tie': 0.09375, 'loss': 0.859375} oracle={'win': 0.11299999803304672, 'tie': 0.37299999594688416, 'loss': 0.5139999985694885}
- 真实 trace：`d2c649a75f48a0b4` turn12 official=None status=MATCH

### 25. `BG34_320` The Last One Standing

- 卡面：<b>Rally:</b> Give a friendly minion of each type +{0}/+{1} permanently.
- 类别：动态状态, 已知 TOP/审计名单, 状态口径缺口；679 分歧 4、缺脚本 0、动态 5
- 679 样本：`75608cb597faa64b` distribution_distance official=win L1=0.762；`d92c24ef6f176298` distribution_distance official=win L1=0.4314999618999962；`e5c6f90d0ae41612` official_result_unreachable official=win L1=None
- 真实事件级首分歧：`20f1a5042f7d0037` [event_kind] idx=0 official={'seq': 0, 'kind': 'damage', 'actor_slot': None, 'target_slot': None, 'actor_side': None, 'target_side': 'player', 'amount': 12, 'card_id': 'TB_BaconShop_HERO_15_SKIN_G', 'effect_card_id': None} engine={'kind': 'attack', 'actor_side': 'opponent', 'actor_slot': 0, 'target_side': 'player', 'target_slot': 5}；`ff75d9a57e3babce` [event_kind] idx=0 official={'seq': 0, 'kind': 'damage', 'actor_slot': None, 'target_slot': None, 'actor_side': None, 'target_side': 'player', 'amount': 5, 'card_id': 'TB_BaconShop_HERO_40', 'effect_card_id': None} engine={'kind': 'attack', 'actor_side': 'opponent', 'actor_slot': 0, 'target_side': 'player', 'target_slot': 6}

## 3. 按类清单

### 脚本缺失（156）

`BG33_156` Silent Enforcer、`BG31_826` Doubloon Grifter、`BG34_Giant_031` Timewarped Leapfrogger、`BG33_240` Charmwing、`BG28_583` Geomagus Roogug、`BG34_781` Felhorn、`TB_BaconShop_HERO_33_Buddy` Mishmash、`BG29_815` Nightbane, Ignited、`BG34_Giant_208` Timewarped Pagle、`BG20_203` Prophet of the Boar、`BG34_765` Dead Sea Ravager、`BG29_807` Trigore the Lasher、`BG32_204` Silky Shimmermoth、`BG29_862` Barrens Conjurer、`BG34_Giant_314` Timewarped Poet、`BG34_523` Hunting Tiger Shark、`BG33_241` Sleepy Supporter、`BG_DAL_775` Tunnel Blaster、`BG32_433` Dreaming Thornweaver、`BG26_159` Moon-Bacon Jazzer、`BG34_731` Twilight Broodmother、`BG29_806` Iridescent Skyblazer、`BG30_122` Mrglin' Burglar、`BG33_319` Rimescale Priestess、`BG33_894` Coldlight Diver、`BG34_Giant_602` Timewarped Lei、`BG25_032` Peggy Sturdybone、`BG26_867` Three Lil' Quilboar、`BG21_014` Prized Promo-Drake、`BG21_005` Famished Felbat、`BG33_809` Divine Sparkbot、`BG34_Giant_597` Timewarped Immortal、`BG35_890` Ingenious Inventor、`BG35_334` Skeletal Strafer、`BG28_308` Archlich Kel'Thuzad、`BG35_151` Woodland Defiler、`BG32_434` Skulking Bristlemane、`BG29_841` Greymane's Champion、`BG20_301` Sun-Bacon Relaxer、`BG26_800` Manasaber、`BG23_018` Darkgaze Elder、`BG26_175` Elemental of Surprise、`BG34_Giant_042` Timewarped Chameleon、`BG34_Giant_067` Timewarped Sellemental、`BG35_153` Consummate Conqueror、`BG35_601` Wyvern Outrider、`BG_BOT_911` Annoy-o-Module、`TB_BaconShop_HERO_78_Buddy` Snow Elemental、`BG24_707` Bristlemane Scrapsmith、`BG30_117` Volcanic Visitor、`BG25_HERO_103_Buddy` Shadowy Construct、`BG34_Giant_038` Timewarped Elise、`TB_BaconShop_HERO_53_Buddy` Valithria Dreamwalker、`TB_BaconShop_HERO_41_Buddy` Sr. Tomb Diver、`BG29_808` Spiked Savior、`BG35_433` Redtusk Thornraiser、`BG23_013` Tidemistress Athissa、`BG31_176` Dr. Boom's Monster、`BG34_Giant_309` Timewarped Nine Frogs、`BG34_Giant_321` Timewarped Mrrrglr、`BG34_Giant_322` Timewarped Pioneer、`BG34_Giant_333` Timewarped Scout、`BG34_Giant_680` Timewarped Collector、`BG35_702` Roving Sailor、`TB_BaconShop_HERO_11_Buddy` Lucifron、`TB_BaconShop_HERO_56_Buddy` Vaelastrasz、`TB_BaconShop_HERO_90_Buddy` Burth、`TB_BaconShop_HERO_28_Buddy` Clockwork Assistant、`TB_BaconShop_HERO_29_Buddy` Tentacle of C'Thun、`BG21_HERO_010_Buddy` Warden Thelwater、`BG34_231` Old Soul、`BG31_859` Technical Element、`BG31_HERO_802pt4` Immortal、`BG31_HERO_802pt7` Mothership、`BG33_840` Stomping Stegodon、`BG34_321` Rabid Panther、`BG34_Giant_034` Timewarped Geist、`BG34_Giant_041` Timewarped Greenskeeper、`BG34_Giant_071` Timewarped Bassgill、`BG34_Giant_074t` Timewarped Nellie's Ship、`BG34_Giant_078` Timewarped Thorncaller、`BG34_Giant_324` Timewarped Summoner、`BG34_Giant_332` Timewarped Embalmer、`BG34_Giant_591` Timewarped Acolyte、`BG34_Giant_678` Timewarped Lava Lurker、`BG35_122` Determined Defender、`BG35_434` Hired Ritualist、`BG22_HERO_201_Buddy` Submersible Chef、`BG24_005` Sinrunner Blanchy、`BG25_040` Blazing Skyfin、`BG25_155` Gem Smuggler、`BG26_360` Scourfin、`BG26_HERO_104_Buddy` Akali, Rock Rhino、`BG26_ICC_065` Bone Baron、`BG27_HERO_801_Buddy` Veranus, Stormlord's Mount、`BG28_HERO_801_Buddy` The Nine Frogs、`BG29_840` Mooneater's Champion、`BG31_178` Marquee Ticker、`BG31_360` Arcane Behemoth、`BG33_111` Carapace Raiser、`BG34_Giant_006` Timewarped Molten Rock、`BG34_Giant_015` Timewarped Hooktail、`BG34_Giant_082` Timewarped Recycler、`BG34_Giant_204` Timewarped Pillager、`BG34_Giant_211` Timewarped Pashmar、`BG34_Giant_327` Timewarped Peggy、`BG34_Giant_330` Timewarped Radio Star、`BG34_Giant_598` Timewarped Mothership、`BG34_Giant_608` Timewarped Lil' Quilboar、`BG34_Giant_679` Timewarped Chimera、`BG34_Giant_777` Timewarped Behemoth、`BG35_431` Earthsong Shaman、`BG35_700` Ship Jumper、`BG35_895` Tidecaller Prophet、`BGS_030` King Bagurgle、`TB_BaconShop_HERO_02_Buddy` Apostle of Galakrond、`TB_BaconShop_HERO_16_Buddy` Snack Vendor、`TB_BaconShop_HERO_57_Buddy` Chromie、`TB_BaconShop_HERO_59_Buddy` Sklibb, Demon Hunter、`TB_BaconShop_HERO_702_Buddy` Mawsworn Soulkeeper、`BG32_HERO_002_Buddy` Zippers、`BG20_HERO_100_Buddy` Icesnarl the Mighty、`BG20_HERO_102_Buddy` Dranosh Saurfang、`BG20_HERO_103_Buddy` Death's Head Sage、`BG20_HERO_280_Buddy` Living Nightmare、`BG22_HERO_002_Buddy` Frostwolf Lieutenant、`BG25_003` Relentless Sentry、`BG27_513` Tide Oracle Morgl、`BG30_121` Hot-Air Surveyor、`BG30_HERO_304_Buddy` Fantastic Bellhop、`BG31_HERO_003_Buddy` Outland Sunbeam、`BG31_HERO_811t3` Roach、`BG32_891` Shadowdancer、`BG32_HERO_001_Buddy` Malorne、`BG33_154` Lord of the Ruins、`BG33_888` Hog Watcher、`BG34_Giant_001` Timewarped Busker、`BG34_Giant_072` Timewarped Skipper、`BG34_Giant_110` Timewarped Sea Glass、`BG34_Giant_210` Timewarped Commander、`BG34_Giant_212` Timewarped Archer、`BG34_Giant_304` Timewarped Sapper、`BG34_Giant_328` Timewarped Tide Razor、`BG34_Giant_370` Timewarped Hawkstrider、`BG34_Giant_580` Timewarped Ragnaros、`BG34_Giant_589` Timewarped Sailor、`BG34_Giant_590` Timewarped Festergut、`BG34_Giant_595` Timewarped Tamuzo、`BG34_Giant_609` Timewarped Ghoul-acabra、`BG34_Giant_618` Timewarped Caretaker、`BG34_PreMadeChamp_067` Timewarped Plunderer、`TB_BaconShop_HERO_08_Buddy` Eclipsion Illidari、`TB_BaconShop_HERO_10_Buddy` Bilgewater Mogul、`TB_BaconShop_HERO_43_Buddy` Brann's Epic Egg、`TB_BaconShop_HERO_45_Buddy` Loyal Henchman、`TB_BaconShop_HERO_75_Buddy` Lantern Tender

### 动态状态（97）

`BG26_174` Soul Rewinder、`BG31_801` Forest Rover、`BG31_171` Moonsteel Juggernaut、`BG31_826` Doubloon Grifter、`BG23_009` Lava Lurker、`BG31_035` Groundbreaker、`BGS_004` Wrath Weaver、`BG32_822` Fire-forged Evoker、`BG25_008` Eternal Knight、`BG34_765` Dead Sea Ravager、`BG29_807` Trigore the Lasher、`BG32_204` Silky Shimmermoth、`BG33_155` Devout Hellcaller、`BG34_320` The Last One Standing、`BG33_241` Sleepy Supporter、`BG32_433` Dreaming Thornweaver、`BG29_806` Iridescent Skyblazer、`BG35_701` Brazen Buccaneer、`BG32_324` Drustfallen Butcher、`BG32_234` Dastardly Drust、`BG21_014` Prized Promo-Drake、`BG35_890` Ingenious Inventor、`BG28_633` Felboar、`BG32_873` Ashen Corruptor、`BG35_334` Skeletal Strafer、`BG35_437` Vinespeaker、`BG35_921` Abyssal Bruiser、`BG23_018` Darkgaze Elder、`BG35_153` Consummate Conqueror、`BG35_601` Wyvern Outrider、`BG29_816` Roaring Recruiter、`BG31_824` Dual-Wield Corsair、`BG34_856` Waveling、`BG34_692` Forsaken Weaver、`BG31_803` Buzzing Vermin、`BG26_137` Bream Counter、`BG31_809` Turquoise Skitterer、`BG34_Giant_038` Timewarped Elise、`BG23_013` Tidemistress Athissa、`BG34_Giant_309` Timewarped Nine Frogs、`BG34_Giant_322` Timewarped Pioneer、`BG34_Giant_333` Timewarped Scout、`BG35_342` Falling Sky Golem、`BG35_702` Roving Sailor、`TB_BaconShop_HERO_90_Buddy` Burth、`BG26_524` Malchezaar, Prince of Dance、`BG33_923` Sundered Matriarch、`BG35_801` Gluttonous Trogg、`BG34_690` Plaguerunner、`BGS_018` Goldrinn, the Great Wolf、`BG33_323` Dustbone Devastator、`BG33_840` Stomping Stegodon、`BG34_Giant_332` Timewarped Embalmer、`BG35_340` Alert Alarmist、`BG28_707` Living Azerite、`BG32_821` Felfire Conjurer、`BG31_816` Fire Baller、`BG34_865` En-Djinn Blazer、`BG27_016` Champion of Sargeras、`BG31_920` Darkcrest Strategist、`BG34_175` Junk Jouster、`BG35_140` Mama Mrrglton、`BG35_141` Papa Mrrglton、`BG32_835` Tranquil Meditative、`BG26_HERO_104_Buddy` Akali, Rock Rhino、`BG28_HERO_801_Buddy` The Nine Frogs、`BG34_Giant_006` Timewarped Molten Rock、`BG35_895` Tidecaller Prophet、`TB_BaconShop_HERO_57_Buddy` Chromie、`BG30_129` Catacomb Crasher、`BG32_846` Unleashed Mana Surge、`BG34_858` Air Revenant、`BG34_140` Expert Aviator、`BGS_104` Nomi, Kitchen Nightmare、`BG25_041` Felemental、`BG34_312` Hardy Orca、`BG26_810` Gunpowder Courier、`BG32_880` Friendly Geist、`BG23_004` Deep-Sea Angler、`BG26_162` Dancing Barnstormer、`BG28_741` Charging Czarina、`BG33_890t` Magicfin Apprentice、`BG34_635t` Black Chromadrake、`BGS_041` Kalecgos, Arcane Aspect、`BG31_815` Dune Dweller、`BG_TTN_401` Ancestral Automaton、`BG25_806` Sly Raptor、`BG31_818` Snow Baller、`BG33_823` Sky Admiral Rogers、`BGS_071` Deflect-o-Bot、`BG26_814` Lovesick Balladist、`BG31_810` Ultraviolet Ascendant、`BG34_636t` Green Chromadrake、`BG34_637t` Bronze Chromadrake、`BG34_950` Stone Age Slab、`BG35_152` Void Pup Trainer、`BG35_602` Lurking Leviathan

### 状态口径缺口（48）

`BG31_826` Doubloon Grifter、`BG34_320` The Last One Standing、`BG36_101` Unwilling Slacker、`BG36_115` Nightmare Corroder、`BG36_311` Abyssal Envoy、`BG29_300` Very Hungry Winterfinner、`BG32_111` Nightmare Par-tea Guest、`BG36_103` N'raqi Sapper、`BG30_123` Fearless Foodie、`BG36_300` N'raqi Frostcaller、`BG36_520` Bilgewater Breakout、`BG26_135` Southsea Busker、`BG32_821` Felfire Conjurer、`BG33_886` Tusked Camper、`BG35_150` Laboratory Assistant、`BG31_326` Gem Rat、`BGFYM_002t` Aberrant Tentacle、`BG32_236` Aureate Laureate、`BG20_104` Bonker、`BG31_323` Turbo Hogrider、`BG31_326_G` Gem Rat、`BG32_860` Shoalfin Mystic、`BG34_140` Expert Aviator、`BG36_106_G` Cutthroat K'Thir、`BG36_109` The Shadow of Doubt、`BG36_113` Drifting Sacrifice、`BG36_115_G` Nightmare Corroder、`BG36_318` Faceless Converter、`BG36_320_G` Mysterious K'Thir、`BG36_523` Enterprising Escapee、`BG36_640` Gatekeeper Amalgam、`BG32_330` Flighty Scout、`BG34_926` Ruthless Queensguard、`BG32_235` Surfing Sylvar、`BG36_345` Suspicious Prisonguard、`BG31_330` Ominous Seer、`BG36_342` Clever Castaway、`BG31_327` Thorned Trailblazer、`BG26_817` Blade Collector、`BG20_101` Roadboar、`BG31_320` Crater Miner、`BG33_886_G` Tusked Camper、`BG34_140_G` Expert Aviator、`BG36_100` Wandering Willbreaker、`BG36_110` Joyous、`BG36_116` Underrot Spawn、`BG36_342_G` Clever Castaway、`BG36_763` Treasure Parrot

### 疑似卡牌效果（165）

`BG26_174` Soul Rewinder、`BG31_801` Forest Rover、`BG33_156` Silent Enforcer、`BG31_826` Doubloon Grifter、`BG34_Giant_031` Timewarped Leapfrogger、`BG33_240` Charmwing、`BG23_009` Lava Lurker、`BG31_035` Groundbreaker、`BG28_583` Geomagus Roogug、`BGS_004` Wrath Weaver、`BG34_781` Felhorn、`TB_BaconShop_HERO_33_Buddy` Mishmash、`BG29_815` Nightbane, Ignited、`BG25_008` Eternal Knight、`BG34_Giant_208` Timewarped Pagle、`BG20_203` Prophet of the Boar、`BG34_765` Dead Sea Ravager、`BG29_807` Trigore the Lasher、`BG32_204` Silky Shimmermoth、`BG29_862` Barrens Conjurer、`BG34_Giant_314` Timewarped Poet、`BG33_155` Devout Hellcaller、`BG34_523` Hunting Tiger Shark、`BG33_241` Sleepy Supporter、`BG_DAL_775` Tunnel Blaster、`BG32_433` Dreaming Thornweaver、`BG26_159` Moon-Bacon Jazzer、`BG34_731` Twilight Broodmother、`BG29_806` Iridescent Skyblazer、`BG30_122` Mrglin' Burglar、`BG33_319` Rimescale Priestess、`BG33_894` Coldlight Diver、`BG34_Giant_602` Timewarped Lei、`BG35_701` Brazen Buccaneer、`BG25_032` Peggy Sturdybone、`BG26_867` Three Lil' Quilboar、`BG21_014` Prized Promo-Drake、`BG33_809` Divine Sparkbot、`BG34_Giant_597` Timewarped Immortal、`BG35_890` Ingenious Inventor、`BG28_633` Felboar、`BG35_334` Skeletal Strafer、`BG28_308` Archlich Kel'Thuzad、`BG35_921` Abyssal Bruiser、`BG35_151` Woodland Defiler、`BG32_434` Skulking Bristlemane、`BG32_111` Nightmare Par-tea Guest、`BG29_841` Greymane's Champion、`BG20_301` Sun-Bacon Relaxer、`BG26_800` Manasaber、`BG23_018` Darkgaze Elder、`BG29_816` Roaring Recruiter、`BG31_824` Dual-Wield Corsair、`TB_BaconShop_HERO_78_Buddy` Snow Elemental、`BG24_707` Bristlemane Scrapsmith、`BG30_117` Volcanic Visitor、`BG25_HERO_103_Buddy` Shadowy Construct、`TB_BaconShop_HERO_53_Buddy` Valithria Dreamwalker、`TB_BaconShop_HERO_41_Buddy` Sr. Tomb Diver、`BG29_808` Spiked Savior、`BG35_433` Redtusk Thornraiser、`BG34_Giant_309` Timewarped Nine Frogs、`BG34_Giant_322` Timewarped Pioneer、`BG34_Giant_333` Timewarped Scout、`BG34_Giant_680` Timewarped Collector、`TB_BaconShop_HERO_11_Buddy` Lucifron、`TB_BaconShop_HERO_56_Buddy` Vaelastrasz、`TB_BaconShop_HERO_90_Buddy` Burth、`BG33_923` Sundered Matriarch、`BG34_500` Flaming Enforcer、`BG35_801` Gluttonous Trogg、`BG34_690` Plaguerunner、`BG33_323` Dustbone Devastator、`TB_BaconShop_HERO_28_Buddy` Clockwork Assistant、`TB_BaconShop_HERO_29_Buddy` Tentacle of C'Thun、`BG21_HERO_010_Buddy` Warden Thelwater、`BG26_135` Southsea Busker、`BG34_321` Rabid Panther、`BG34_Giant_034` Timewarped Geist、`BG34_Giant_074t` Timewarped Nellie's Ship、`BG34_Giant_324` Timewarped Summoner、`BG35_122` Determined Defender、`BG35_340` Alert Alarmist、`BG33_886` Tusked Camper、`BG32_835` Tranquil Meditative、`BG30_129` Catacomb Crasher、`BG32_846` Unleashed Mana Surge、`BG32_236` Aureate Laureate、`BGS_115t` Water Droplet、`BG34_858` Air Revenant、`BG34_920` Tide Raiser、`BG33_920` Wrathscale Rogue、`BG_LOE_077` Brann Bronzebeard、`BGS_104` Nomi, Kitchen Nightmare、`BG25_041` Felemental、`BG26_810` Gunpowder Courier、`BG31_815` Dune Dweller、`BG_TTN_401` Ancestral Automaton、`BG34_922` Maelstrom Emergent、`BG34_925` Seafloor Recruiter、`BG25_354` Titus Rivendare、`BG25_806` Sly Raptor、`BG35_883` Balinda Stonehearth、`BG32_330` Flighty Scout、`BG31_818` Snow Baller、`BG35_123` Cataclysmic Harbinger、`BG26_ICC_901` Drakkari Enchanter、`BG33_140` River Skipper、`BG21_HERO_030t` Sneed's New Shredder、`BG34_694` Wintergrasp Ghoul、`BG35_432` Bristleback Bully、`BG34_926` Ruthless Queensguard、`BGS_034` Bronze Warden、`BG34_635_Gt` Black Chromadrake、`BG21_015` Tarecgosa、`BG32_235` Surfing Sylvar、`BG28_309` Mummifier、`BG34_636_Gt` Green Chromadrake、`BG24_500` Amber Guardian、`BG26_501` Reef Riffer、`BG25_022` Scarlet Skull、`BG34_604` Heroic Underdog、`BG33_821` Shipwrecked Rascal、`BG26_802` Banana Slamma、`BG26_524_G` Malchezaar, Prince of Dance、`BG36_354` Decoy Conjurer、`BG31_330` Ominous Seer、`BG29_611` Cord Puller、`BG27_084` Sprightly Scarab、`BG31_999` Stitched Salvager、`BG26_525` Imposing Percussionist、`BG34_682` Razorfen Flapper、`BG35_814` Scarlet Survivor、`BG26_502` Deep Blue Crooner、`BGS_115` Sellemental、`BG32_237` Intrepid Botanist、`BG34_632` Incubation Researcher、`BG26_146` Lullabot、`BG26_817` Blade Collector、`BG29_813` Persistent Poet、`BGS_116` Refreshing Anomaly、`BG28_300` Harmless Bonehead、`BG34_630` Twilight Hatchling、`BG26_199` Upbeat Duo、`BG22_202` Tad、`BGS_049` Freedealing Gambler、`BG35_155` Twisted Wrathguard、`BG24_009` Picky Eater、`BG24_715` Patient Scout、`BG23_002` Shell Collector、`BG34_921` Ring Bearer、`BG26_801` Rylak Metalhead、`BG33_822` Bigwig Bandit、`BG30_125` Cadaver Caretaker、`BGS_126` Wildfire Elemental、`BGS_078` Monstrous Macaw、`BG33_820` Lost City Looter、`BG20_100` Razorfen Geomancer、`BG27_005` Timecap'n Hooktail、`BG26_529` Upbeat Frontdrake、`BG21_018` Defiant Shipwright、`BG25_001` Risen Rider、`BG33_825` Proud Privateer、`BG26_147` Accord-o-Tron、`BG32_430` Glowgullet Warlord

### C 类证据不足（6）

`BG26_174` Soul Rewinder、`BG31_035` Groundbreaker、`BGS_004` Wrath Weaver、`BG32_822` Fire-forged Evoker、`BG33_155` Devout Hellcaller、`BG35_701` Brazen Buccaneer

