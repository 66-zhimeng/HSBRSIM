# C round6 修复 + BB 批量 TOP 候选分析（2026-10-18）

## 1. C round6：F-14（Reborn 复生体恢复卡面基础关键词）

### 1.1 trace 识别更正
- Lead 消息中的 `be819be0e16f9746` 当前复现：P 7 只 vs O 5 只
  （BG33_924/BG26_174/…/BGS_034…），引擎 64 seeds **win 100%**，
  `runs/diff_powerlog3.json` 已是 **MATCH**（无需修复）。
- 实际 **tie 98.4% / win 0%** 的 trace 是
  **`a07dc079cf3bd8d9`**（C round6，turn 6）：
  P `BGS_034 2/1 DS+reborn` + `BG31_815 3/3` ×2；
  O `BG21_015 4/7`、`BG21_015 4/4`；官方 win。
  这与 `runs/diff_powerlog3.md` D1 的 98.4% tie 记录一致。

### 1.2 官方证据（Power.log_2240）
- BGS_034（eid 1248）第一次死亡时 DS 已破（`DIVINE_SHIELD=0`）。
- 官方 Reborn 新实体 **eid=1487** 的 `FULL_ENTITY - Creating ID=1487
  CardID=BGS_034` 明确带：
  `DIVINE_SHIELD=1`、`REBORN=1`（随后 `REBORN=0`）、
  `CREATOR=1248`、`HAS_BEEN_REBORN=1`、`ATK=2 / HEALTH=1`。
- 即 Reborn 是**卡面基础关键词的新副本**（DS 恢复），不是“只保留
  死亡瞬间 tag 的同一实体”。

### 1.3 根因 / 修复
- 旧引擎 `_process_death_inner` 的 Reborn 分支只保留死亡瞬间 tag；
  BGS_034 的 DS 已消耗 → 复生体无 DS，1 血立即被 4 攻随从再次击杀，
  双方清场 → tie。
- 修复（`hsrl2/game.py`）：Reborn 原位回归时按 `CardDef.keywords`
  恢复缺失的卡面基础关键词（跳过 `reborn` 本身）；DS/taunt 等恢复。
- 失败测试：`hsrl2/tests/test_reborn_keyword_restore.py`（先红后绿，
  2 例）。

### 1.4 结果
- `a07dc` 引擎 64 seeds：win **90.6%** / tie 9.4%（修复前 tie 98.4%）。
- `runs/diff_powerlog3_after_f14.json`：MATCH **6/10**（修前 5/10）、
  **behavior_mismatch 0**（修前 1）、engine_divergence 3。
- 全量回归 **5,158 passed / 3 skipped / 14 subtests**；审计 PASS；
  smoke 8 局 0 crash。

## 2. BB 批量 TOP 候选（`runs/diff_bulk_bb_20261018_232140.jsonl`）

### 2.1 TOP 卡聚类：state 23 / 268（Deity 隐藏状态）
- 两状态共用同一批 TOP 卡：`BG36_300 / BG36_318 / BG36_523 /
  BG36_320_G / BG36_106_G`（+ `BG32_821`），玩家侧含 `BG34_320`。
- state 23（turn18，powerlog，官方 loss）：
  engine 64：win .922 / tie .0625 / loss .0156；
  BB oracle 1000：win 0 / tie 0 / loss 1；TVD **.984**。
- 引擎敏感性（64 seeds，逐卡移除）：
  - 移除玩家 `BG34_320` → **loss .984**；
  - 移除玩家 `BG32_111`（ALL 种族）→ loss 1.0；
  - 移除玩家 `BG36_103` → loss 1.0；
  - 移除任一对手 TOP 卡 → 仍 win .84–1.0（不改变方向）。
- 机制判断：这些卡是 **Deity / Discard / Rally** 家族；state 的
  `hidden_context_reasons` 含 `anomaly/hero_power_used/quest/trinkets
  not in schema`，**没有 Deity 字段**。引擎 `Game.deity()` 会按
  GameRNG lazy 创建 Deity（F-07），官方事件流中确实出现
  `BGFYM_000`/`BGFYM_011` Deity 伤害/死亡（state23 trace 事件）；
  但状态语料/oracle 输入没有 Deity 的 card_id/atk/health/progress/
  awakened。BB `Minion.IsDeity` + `Player.FriendlyDeitySigil` 需要显式
  输入。
- 裁定：**主要是 harness 状态缺口（c），不是脚本 bug**。修复方向：
  state_corpus/trace 增加每英雄 Deity 字段：
  `deity_card_id`、`deity_atk`、`deity_health`、`deity_progress`、
  `deity_awakened`、`deity_sigil_card_id`；引擎侧可用
  `Game.select_deity` + `give_deity_stats` 注入；BB oracle schema 增加
  `is_deity` minion / `friendly_deity_sigil`。在补齐前，这批状态应标
  `unsupported: deity`，不计入 TVD。

### 2.2 外部死亡奇点：BG27_004 + `Deathrattles: [GenericDeathrattles.Crab]`
- state 111（after-F14 corpus；turn10）：P 含 `BG27_004`，
  `enchantment_derived = {"Deathrattles": "[GenericDeathrattles.Crab]"}`；
  engine win 1.0 / BB loss .74；TVD **.819**。
- 同类状态 83（TVD .628）、33（.497）等也带
  `Deathrattles: Crab/CrabGolden` 或 AutoAssemblerEnchantment。
- 根因：外部亡语/附魔来源不在引擎或 oracle 的可表达状态中；BB 侧
  需要 `AdditionalDeathrattles`（Minion 字段）/ enchantment 列表，
  引擎侧没有 generic external deathrattle 注入原语。
- 裁定：**harness/状态缺口（c）**；短期将 `enchantment_derived`
  样本标 unsupported，长期补外部亡语注入通道（引擎+BB schema 同步）。

### 2.3 简单状态 60/132/271（TVD=1.0）
- turn1–3 单侧/低样本，engine 与 BB 的结果方向相反；
  这类状态主要暴露 **BB 与引擎在低样本/随机攻击序上的差异**，
  非本轮 3 张卡/脚本机制；暂 DEFERRED（需更多 1000+ sim 与官方
  对照），不优先改引擎。

## 3. TVD 复跑（F-14 后，同一 300 状态集）

命令：
```powershell
python tools/diff_bulk_fuzz.py --states-file runs/state_corpus/states.jsonl \
  --states 300 --engine-seeds 64 --oracle bobsbuddy --oracle-sims 1000 \
  --bb-threads 4 --out-jsonl runs/diff_bulk_bb_after_f14.jsonl \
  --out-md runs/diff_bulk_bb_after_f14.md
```

| 指标 | F-14 前 | F-14 后 |
|---|---|---|
| supported / 300 | 300 | 300 |
| agree（TVD≤0.15） | 258/300 (86.0%) | **262/300 (87.3%)** |
| TVD mean | 0.0661 | **0.0667** |
| p90 TVD | 0.234 | **0.243** |
| reborn 子集 mean TVD | 0.0764 | 0.0773 |
| TOP 卡 BG36_300/318/523/320_G/106_G | 0.766 | 0.766（state23/268 的 Deity 缺口未补） |

结论：F-14 修复了 C round6 这一真实官方错误（behavior_mismatch→0），
但批量 TVD 由 **Deity 状态缺口**（TOP 卡聚类）与外部附魔/亡语主导；
reborn 关键词的全局 mean 略升（其他 reborn 状态/BB 差异抵消），不能
用“TVD 下降”证明 F-14，应以 C round6 官方事件为准。TVD 只统计
supported 样本；Deity/enchantment 样本应标 unsupported 后重算。

## 4. 下一步（优先级）
1. diff-harness：state_corpus 增 Deity 六字段 + oracle schema
   `is_deity`/`friendly_deity_sigil`；对 state23/268 重跑 TVD。
2. 外部亡语/附魔：`enchantment_derived` 进入 `unsupported`（短期）；
   长期给引擎 generic external deathrattle 通道（另行立案）。
3. BG34_320：单独构造最小状态（BG34_320 + 一个 ALL 种族随从）
   对比 BB 1000 sims 与引擎 64 seeds，确认 Rally “each type”对同一
   ALL 随从的多重选取是否符合 BB/官方；若确认再走失败测试流程。
4. `BG34_320`（用户终局卡）优先作为下一张单独审计卡。

## 5. 2026-10-19 收尾分类（最终，F-16..F-19 后）

- **F-15**：Deity 六字段/sigil/board token 注入 API；默认未调用逐位不变，
  9 例测试。state23 TVD **0.9844→0.8438**、state268 **0.0**。
- **F-16 C'Thun split**：真实 Power.log（172605/174028/108099）证明
  `floor(atk/N)/floor(health/N)`、余数丢弃；已实现并测试。**C'Thun
  不再是 known gap**，state23/268 应重新纳入 diff 复跑；F-19 后本地
  bulk state23 **0.8438→0.7344**、state268 **0.0**。
- **F-17/F-18/F-19**：Bristleback Bully 缺脚本、Moonsteel 缺脚本、
  Fire-forged 公式；均有测试与 ENGINE_NOTES。
- **BB 仅第三方参照**：不用 BB 行为反推官方 C'Thun split；F-16 只用
  真实 Power.log。
- **BG34_320**（Rally + ALL 多次选取）仍 partial evidence，维持观察、
  不猜。
- 收尾：`pytest hsrl2/tests` **4,718 passed / 2 skipped**；全量
  `hsrl2/tests tests` 5,240 passed + 1 个非本 scope 搜索测试失败
  （policy/search）；审计 PASS；smoke 8 局 0 crash。
