# DEFERRED 原语需求清单（差分驱动脚本线）

> 维护：diff-harness。来源：批 3/批 4 实施过程中遇到的"脚本层无法安全实现"的
> 引擎/数据缺口。目的：Lead 排期给 engine-analyst；**未擅自改引擎核心**。
> 每项含：卡、官方文本、阻塞点、最小原语设计、影响面、验收建议。

---

## P1. `minion_gained(hero, minion)` 统一"获得"事件（**RESOLVED**：engine-analyst 已落地，2026-10-18）

**卡**：`BG32_234` Dastardly Drust（T6 5/4 海盗）
**文本**："Whenever you get a Pirate, give your minions +{0}/+{1}. Give
Golden ones +{2}/+{3} instead."

**阻塞点**：引擎没有统一的"获得（get）"事件。三条入手路径互不广播：
1. 购买：`Game.buy_from_tavern` 直接把酒馆实体移入 `hero.hand`；
2. Discover / "Get a copy"：`Game.grant_card_copy_to_hand`（占池/生成）；
3. 生成/奖励：`create_minion` + `pending_hand_add`（满手队列）。
`entity_created` 只覆盖 `create_minion`（且不含购买路径），无法作为
"get"观察点；`card_played` 是"打出"不是"获得"。

**最小原语设计**（engine-analyst 决策）：
* 在 `hero.add_to_hand(entity)` / `game.pending_hand_add` /
  `buy_from_tavern`（成交后）的统一路径 fire 一次性
  `minion_gained(hero=, minion=)`（只对 **Minion**，不含法术/饰品）；
* 语义要求：一次"get"只广播一次（购买 ≈ 1 次；Discover 选中 1 次；
  "Get 2 copies" = 2 次），满手排队路径应在**队列交付时**广播；
* 影响面：所有"whenever you get/acquire a X" 家族（含未来卡）、
  池记账/满手协议；需逐路径审计避免重复广播（购买路径目前移手牌可
  复用同一入口）；不应触发 `stats_gained`/`card_played` 语义。

**验收建议**：单测覆盖 buy/discover/get-copy/满手排队四路径各广播 1 次；
Dastardly Drust 用真实路径触发（购买海盗 → 全体 +2/+2；金色海盗用
NUM_3/NUM_4 = 6/6）。

---

## P2. `Game.combat_spell_extra_casts(hero) -> int` 战斗内法术额外结算（**RESOLVED**：engine-analyst 已落地，2026-10-18）

**卡**：`BG34_922` Maelstrom Emergent（T5 2/7 Naga）
**文本**："Your spells cast an extra time in combat."（金色："2 extra
times"；CardDef NUM_1=1）。

**阻塞点**：战斗内"效果施法"（`Game.cast_spell_effect`，如 Ring Bearer
施放 Shiny Ring）没有全局倍率/查询钩子；脚本层无法拦截"任意己方法术
结算"（事件 `spell_resolved` 是**后置**，无法回放效果；`spell_resolving`
同理且无重复结算协议）。实现该卡需要引擎在施法结算路径读取 aura。

**最小原语设计**：
* `Game.combat_spell_extra_casts(hero) -> int`：遍历 `hero.board`
  （及饰品/任务奖励，如适用）上脚本 `spell_extra_casts_in_combat`
  静态属性/钩子，返回额外次数（默认 0；仅 `game.in_combat` 生效）；
* `Game.cast_spell_effect(...)` 在效果结算处 `for _ in range(1 + extra)`
  重复结算；避免重复占池/重复计 TAVERN_SPELLS_CAST；
* 影响面：战斗内所有 `cast_spell_effect` 调用点（Ring Bearer /
  Shiny Ring 等）；与 `spell_resolved` 后置监听语义需明确（每次重复
  是否各广播一次——建议广播，官方"cast extra time"按施放次数计）。

**验收建议**：Maelstrom Emergent 基础 +1 / 金色 +2；战斗内 Ring Bearer
的 Shiny Ring 实际结算 2/3 次；非战斗施法不受影响；`behavior_mismatch`
不回归（679 场 A/B）。

---

## P2b. 8 族动态适配器"反推失败"逐卡证据（交 engine-analyst 排查）

harness 适配器规则：`printed - base` 必须能被步长整除，且攻/血两维
反推 count 一致；否则**不注入 NUM_3、保留缺口**（避免双计），并在
`records[].simulation.script_data_injected[].adapter_skip` 记录证据。
679 场实测（`runs/diff_report.json`，2026-10-18）：

| 卡 | 出现 | 适配成功 | 反推失败（保留缺口） | 失败样本（trace: printed/base/step） |
|----|------|----------|----------------------|--------------------------------------|
| `BG25_008` Eternal Knight | 61 | 33 | 28 | `0a36546b…`: 95/44 vs 8/4, step 8/4；`11c40129…`: 39/18 vs 4/2, step 4/2 |
| `BG35_342` Falling Sky Golem | 39 | 8 | 31 | `0311764c…`: 289/189 vs 4/2, step 4/2；`1fb2317c…`: 180/84 vs 4/2 |
| `BG35_921` Abyssal Bruiser | 18 | 1 | 17 | `0834b480…`: 116/107 vs 2/1, step 2/1；`0eb66d81…`: 5/5 vs 2/1 |
| `BG_TTN_401` Ancestral Automaton | 30 | 18 | 12 | `04b4fd3f…`: 88/69 vs 6/8, step 3/2；`289643b1…`: 28/17 vs 6/8 |
| `BG26_537` / `BG36_524` / `BG25_013` | 0（本批 679 场语料未出现） | — | — | 登记已就绪，等语料覆盖 |

**失败原因**：printed 面板还含其它来源附魔/光环/宝石/复制 buff，无法把
"动态计数贡献"从面板中分离；例如 Eternal Knight 95/44 减去基础 8/4 后
攻维不能被 8 整除。**不建议**强行注入（会与其它 buff 双计，A/B 实测会
让分布变差）。请 engine-analyst/证据线逐卡确认：这些 trace 中面板的
额外来源是否来自可建模状态（光环/宝石）；若可分离，harness 适配器可
扩展为"多贡献拆分"，否则等真实 Power.log 的逐事件属性序列。

---

## P2c. 阶段 2 登记（**RESOLVED**：root cause 后已登记，2026-10-18）（BG35_801/BG35_814/BG26_529）

harness stage-2 预演确认：按正确消费槽位登记（BG35_801 `{1}`、
BG35_814 `{2}`、BG26_529 `{1}`）后，以下 5 条轨迹由 `CONTEXT_GAP` 转为
`ENGINE_DIVERGENCE`（attribution behavior_mismatch；既有分歧被 context
遮蔽，非新脚本引入）：
`0502f1b15163d311` / `3e4c0c97c6c32601` / `4fb28230d318b126` /
`aa771aaa615fb618` / `be480c4c15e54656`。

完整证据（官方结果 / BobsBuddy 概率 / 引擎 64 seeds 分布 / entity 级
atk·health·golden·keywords·ScriptDataNum / 英雄技能 id+data /
context_gaps）：
* `runs/behavior_mismatch_evidence.json`
* `runs/behavior_mismatch_evidence.md`

**已正式登记**（`BG35_801 {1}` / `BG35_814 {2}` / `BG26_529 {1}`）：5 条轨迹转为 `CONTEXT_GAP / unmodeled_context`（hand/未知技能残留），基线更新为 MATCH 400 / div 153 / ctx 85 / 干净子集 33/33。root cause 见 `reversed/rea/behavior_mismatch_5_rootcause.md`。

---

## P3. 官方 Notes/事件序列证据（非原语，`BG31_171` Moonsteel Juggernaut）

**卡**：`BG31_171`（T6 8/8，EoT get two {1}/{1} Magnetic Satellites and
**improve this**；NUM_1 空、NUM_2=6，金色 NUM_2=12）

**证据现状**（官方 trace，BobsBuddy 日志）：
* `ScriptDataNum1/2` 与手中 Satellite 数值相关但**不一致**：
  00cfafe3224325eb: Moonsteel 132/138 vs 手中 Satellite 342/342、
  366/366、132/132；0311764c: 102/114 vs 102/102；17493f10: 8/12 vs
  4/4、8/8；
* 与 CardDef num(1)=6（金色 12）也不一致；"improve this" 的改善对象
  （卫星数值/卫星数量/自身？）和步长**无权威定义**（wiki Notes 缺失）。

**结论**：DEFERRED，不猜。若后续确认"每次 EoT 后未来卫星 +6/+6"，
现有 `create_minion` + `Buff`/脚本数据通道即可实现（**无新原语**）；
建议采真实 BG `Power.log` 的 EoT 前后实体数值序列作为判据。

---

## P4. 饰品动态状态在官方语料中的可见性（harness/数据缺口）

**卡**：`BG36_MagicItem_216` Eternal Portrait（饰品，脚本读 NUM_3 =
友方 Undead 死亡数；engine-analyst 已迁移）

**现状**：BobsBuddy 会话日志的 `ScriptDataNum` 只打印**棋盘随从**；
饰品实体不在 trace 中，harness 无法注入其 NUM_3 → 相关贡献只能以
`dynamic_script_data`/上下文缺口呈现（不误判为引擎 bug）。

**建议**：后续采集真实 `Power.log` 的饰品实体 tag（traceizer 扩展
`trinkets[]`），或 HDT 日志侧补充饰品 tag 输出；属数据/桥接层，非引擎原语。

---

## P5. 动态步长数据缺口（数据生成器）

**卡**：`BG_TTN_401` Ancestral Automaton（+3/+2，金色 +6/+4）、
`BG25_013` Rot Hide Gnoll（+1/+2 Attack）等 CardDef `script_data_num_*`
为空，文本数值未被数据链表达。harness 已在
`tools/diff_replay_combat.py::DYNAMIC_STAT_STEPS` 用**卡面文本回退表**
（仅登记明确卡，非通用猜测）；建议数据生成器/引擎把每计数步长写入
CardDef `script_data_num_1/2`（或等价字段），harness 回退表即可删除，
避免两处维护。

---

## 附：已解决（不需原语的实现记录）

| 卡 | 实现原语 | 状态 |
|----|----------|------|
| `BG24_009` Picky Eater | `ConsumeRandomTavernMinion`（已有） | batch 4 已实现 |
| `BG33_820` Lost City Looter | `_bounty_ids` + `_get_pool_spell`（已有） | batch 4 已实现 |
| `BG34_632` Incubation Researcher | Avenge 钩子 + Chromadrake token 集（已有） | batch 3 已实现 |
| `BG26_502` Deep Blue Crooner | ScriptDataNum 通道（已有）+ harness `CONSUMED_SCRIPT_DATA` | batch 3 已实现 |
| 8 族动态属性 | `NUM_3` 通道（engine-analyst 已迁移）+ harness `DYNAMIC_STAT_SCRIPT_IDS`/`DYNAMIC_STAT_STEPS` | 本批 harness 已登记 |
