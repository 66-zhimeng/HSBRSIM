# 真实战斗边界状态语料报告（state_corpus）

> 自动生成，勿手改。生成命令：
> `python -m hsbg.bridge.state_corpus export`

- 生成时间（UTC）: 2026-10-18T15:59:15+00:00
- 数据基线 patch: `37.0.0.255130`
- states: `C:\HSagent\runs\state_corpus\states.jsonl`（sha256 `b097a7db07c6279e…`）
- schema: `C:\HSagent\runs\state_corpus\schema.json`

## 1. 规模

| 指标 | 值 |
|------|----|
| `states` | 746 |
| `minions` | 7128 |
| `minions_with_hidden_fields` | 0 |
| `states_hidden_state` | 45 |
| `hidden_state_ratio` | 0.0603 |
| `states_hidden_context` | 746 |
| `hidden_context_ratio` | 1.0 |
| `unique_cards` | 524 |
| `pool_minions_covered` | 176 |
| `pool_minions_total` | 273 |

- 输入加载 748 条，按 `(source, trace_id)` 去重 **2** 条（powerlog 分批目录间有重复）。

## 2. 来源构成

| source | states | boundaries | hidden_state | hidden_context |
|--------|-------:|------------|-------------:|---------------:|
| `bg_last_games_xml` | 45 | game_end_final_board | 45 | 45 |
| `hdt_bobsbuddy` | 677 | combat_start | 0 | 677 |
| `powerlog` | 24 | combat_start | 0 | 24 |

## 3. 可见 / 隐藏比例

- 板面级 `hidden_state`（对手缺失/随从缺身份或数值）：**45/746 = 0.0603**
- 上下文级 `hidden_context`（英雄血量/技能/饰品/异变等缺失）：**746/746 = 1.0**
- 说明：Power.log/HDT 的双方战团 minion 状态是**真实观测**；HDT 缺英雄血量/等级、XML 缺对手，均显式写 hidden，未用卡面替代。

### 上下文缺失原因（出现状态数）

| reason | states |
|--------|-------:|
| `anomaly_not_in_official_combat_trace_schema` | 701 |
| `quest_not_in_official_combat_trace_schema` | 701 |
| `trinkets_not_in_official_combat_trace_schema` | 701 |
| `hero_armor_not_in_source` | 678 |
| `hero_health_not_in_source` | 677 |
| `tavern_tier_not_in_source` | 677 |
| `hero_card_id_not_in_source` | 304 |
| `hero_power_used_not_in_source` | 61 |
| `anomaly_not_in_final_board` | 45 |
| `hero_health_armor_tier_not_in_final_board` | 45 |
| `hero_power_not_in_final_board` | 45 |
| `opponent_hidden: BgsLastGames only stores local player's final board` | 45 |
| `quest_not_in_final_board` | 45 |
| `trinkets_not_in_final_board` | 45 |
| `hero_power_id_not_in_source` | 37 |
| `deity_not_in_source` | 5 |
| `deity_not_in_final_board` | 1 |

### 侧级缺失字段（出现次数）

| 字段 | 次数 |
|------|-----:|
| `player.anomaly` | 746 |
| `player.quest` | 746 |
| `player.trinkets` | 746 |
| `player.hero_armor` | 722 |
| `player.hero_health` | 722 |
| `player.tavern_tier` | 722 |
| `opponent.anomaly` | 701 |
| `opponent.quest` | 701 |
| `opponent.trinkets` | 701 |
| `opponent.hero_armor` | 678 |
| `opponent.hero_health` | 677 |
| `opponent.tavern_tier` | 677 |
| `opponent.hero_card_id` | 244 |
| `player.hero_card_id` | 236 |
| `player.hero_power_used` | 72 |
| `opponent.hero_power_used` | 61 |
| `player.hero_power_id` | 48 |
| `opponent.hero_power_id` | 37 |
| `opponent.deity` | 3 |
| `player.deity` | 3 |

## 4. 覆盖卡集合 / 池覆盖

- 唯一 card_id: **524**
- 命中当前池内随从: **176/273**
- 本语料**未覆盖**的当前池内随从: 97 个（前 30：`BG23_000, BG23_357, BG25_013, BG25_034, BG27_000, BG28_582, BG29_888, BG31_148, BG31_149, BG31_177, BG31_810, BG31_924, BG31_925, BG32_231, BG32_336, BG32_340, BG32_820, BG32_837, BG33_830, BG33_883, BG33_885, BG34_170, BG34_170t2, BG34_171, BG34_405, BG36_097, BG36_098, BG36_099, BG36_102, BG36_104`）
- 当前 `data/` 中找不到的 card_id: 2 个：`BG29_160, BG29_160_G`

## 5. 关键词分布

| keyword | minion 次数 |
|---------|------------:|
| `cleave` | 75 |
| `divine_shield` | 823 |
| `poisonous` | 2 |
| `reborn` | 575 |
| `stealth` | 14 |
| `taunt` | 1248 |
| `venomous` | 87 |
| `windfury` | 93 |

### Top 关键词组合

| 组合 | 次数 |
|------|-----:|
| `(none)` | 4859 |
| `taunt` | 823 |
| `divine_shield` | 463 |
| `reborn` | 271 |
| `divine_shield|taunt` | 163 |
| `reborn|taunt` | 162 |
| `divine_shield|reborn|taunt` | 83 |
| `venomous` | 74 |
| `cleave` | 67 |
| `divine_shield|windfury` | 50 |
| `divine_shield|reborn` | 38 |
| `windfury` | 16 |

## 6. 数值分位（真实状态，非卡面）

| 指标 | min | p10 | p25 | p50 | p75 | p90 | p99 | max | n |
|------|----:|----:|----:|----:|----:|----:|----:|----:|--:|
| atk | 0 | 2 | 4 | 11 | 48 | 194 | 1697 | 20437 | 7128 |
| health | 1 | 2 | 4 | 11 | 43 | 179 | 2073 | 512341 | 7128 |

## 7. ScriptData / 附魔派生

| 字段 | 有值 minion 数 |
|------|--------------:|
| `script_data_num_1` | 2652 |
| `script_data_num_2` | 1688 |
| `script_data_num_3` | 466 |
| `script_data_num_4` | 131 |
| `script_data_num_5` | 0 |
| `script_data_num_6` | 0 |
| `enchantment_derived` | 589 |

## 8. Deity（旧神）覆盖

- 含至少一侧 Deity 观测的状态: **7/746 = 0.0094**；双方都有: **7**
- 侧级观测/隐藏/无 Deity 信号: player 7/3/736，opponent 7/3/691
- 按来源: `powerlog` 7
- 字段来源计数: `powerlog_BACON_OLD_GOD_tag` 7，`powerlog_BG_OldGod_objective` 7
- Deity 卡 id: `BGFYM_000` 14

| 字段 | 有值侧数 | 侧覆盖率 |
|------|--------:|---------:|
| `deity_card_id` | 14 | 0.0094 |
| `deity_atk` | 14 | 0.0094 |
| `deity_health` | 14 | 0.0094 |
| `deity_progress` | 14 | 0.0094 |
| `deity_awakened` | 14 | 0.0094 |
| `deity_sigil_card_id` | 14 | 0.0094 |

| Deity tag（Power.log） | 语义 |
|----------------------|------|
| `deity_card_id` | BG_OldGod.BACON_EVOLUTION_CARD_ID/TAG_SCRIPT_DATA_NUM_6 (dbf→BGFYM_000/011) 或 BGFYM token card id |
| `deity_atk` | BG_OldGod.TAG_SCRIPT_DATA_NUM_2；本地 BGFYM token owner 优先 BACON_OLD_GOD_ATTACK |
| `deity_health` | BG_OldGod.TAG_SCRIPT_DATA_NUM_3；本地 BGFYM token owner 优先 BACON_OLD_GOD_HEALTH |
| `deity_progress` | 战斗开始按卡面 each combat 重置为 0（objective QUEST_PROGRESS 为战斗内进度） |
| `deity_awakened` | 战斗开始为 False（awaken 在战斗内发生） |
| `deity_sigil_card_id` | BG_OldGod.BACON_DEITY_SIGIL=1 → "BG_OldGod" |

缺失原因（侧数）：`deity_not_in_final_board` 1，`deity_not_in_source` 5

> 只从真实 Power.log 抽；HDT/XML 源无 Deity 实体 → 六字段 null + `deity_hidden=true`（原因 `deity_not_in_source`），不猜。战斗开始 `deity_progress=0`、`deity_awakened=false`（卡面 each combat 重置）。

## 9. 无法导出的字段及原因

| 字段 | 原因 |
|------|------|
| 对手终局战团（XML 源） | `BgsLastGames.xml` 只保存本地玩家 `FinalBoard` |
| 英雄血量/护甲/酒馆等级（HDT 源） | BobsBuddy Simulation Input 不包含这些字段 |
| 技能使用状态（HDT/Power.log 部分） | trace schema 未保留 EXHAUSTED/激活信息 |
| 饰品 / 异变 / 任务实体 | `official_combat_trace.v1` 不含这些实体 |
| 完整附魔链 | 原始 trace/XML 只保留部分字段（HDT 的 Enchantments/Deathrattles、XML 的 CREATOR/COPIED_FROM_ENTITY_ID），没有完整 enchantment 实体图 |
| XML 无法映射的数值 tag | 旧 tag/客户端内部 tag 不在官方 255130 枚举中，保留为 `tag_<id>` 与 `unmapped_tag_ids` |

### XML 未映射 tag（top）

| tag_id | 出现 minion 次数 |
|--------|-----------------:|
| `479` | 301 |
| `1037` | 301 |
| `1196` | 301 |
| `3085` | 301 |
| `1254` | 298 |
| `3669` | 291 |
| `1068` | 286 |
| `1475` | 66 |
| `3225` | 39 |
| `1724` | 11 |
| `3592` | 10 |
| `1500` | 7 |
| `2711` | 7 |
| `4741` | 7 |
| `4901` | 7 |
| `1804` | 6 |
| `2536` | 6 |
| `3521` | 5 |
| `3779` | 5 |
| `851` | 4 |

## 10. 抽样校验

- 抽样条数: 30；通过: 30；失败: 0

## 11. 口径与限制

- atk/health/golden/keywords/script_data 全部来自真实日志或 trace 快照；tier/races/name 若由 `data/bg_cards.json` 补全，字段旁标 `*_source=carddb`。
- 状态空间随对局历史变化：语料保留的是**该时间点真实状态**，不是基础卡面；使用时不要把 card 静态字段当作状态。
- HDT 结果的 `official_result` 来自官方结算（BobsBuddy Validate 日志）；`bb_rates` 仅为第二实现参照。
- XML 源的 45 局为 HDT 历史终局（2024 年对局，旧 build），card_id 有 2 个不在当前 37.0.0 数据中，已列入 §4；不得用其替代当前版本状态分布。

## 12. 复现

```powershell
python -m hsbg.bridge.state_corpus export
python -m hsbg.bridge.state_corpus validate
python -m pytest tests\test_state_corpus.py -q
```

