# 动态缺口卡逐卡证据审计（TOP context_gap 卡）

> 日期：2026-10-18 ｜ 数据：官方 37.0.0.255130 ｜ 引擎：`hsrl2`（F-10 迁移后）
> 依据：`runs/diff_after_dynmig.json`（679 场）、官方 CardDefs、引擎脚本源码。
> 目标：对主导 `unmodeled_dynamic_state=57` 的 TOP 卡逐张判定
> **A（动态 atk/health 钩子 + 印面 + 计数 → 可迁移）/
> B（其它动态状态 → 记录语义 + harness 通道需求）/
> C（证据不足 → DEFERRED + 缺失证据清单）**，禁止近似。

## 0. 方法与审计口径

1. **脚本清点**：全仓 `def atk(`/`def health(` 动态钩子只有
   `batch_counters.py` 的 8 族（F-10 已迁移）与 Volumizer（F-07，
   hero 计数，非官方 NUM 通道）。本清单 17 张卡**均无动态
   atk/health 钩子**；其属性来自普通 `Buff`/token 模板/引擎计数。
2. **官方值**：`tools/diff_replay_combat.py` 的
   `simulation.script_data_injected` 逐实体记录官方 `ScriptDataNum*`
   原值；与 trace 打印 atk/health/golden 对照。
3. **缺口口径**：harness 仅在"官方值 ≠ 引擎 CardDef 静态
   `script_data_num_n` 且脚本未声明消费该序号"时记
   `dynamic_script_data:<card>:<n>`。
4. **A 类定义**：脚本自身以 `atk`/`health` 动态钩子按
   "印面值 × 计数" 重算属性（F-10 同型）。本清单 **A=0**。

## 1. 分类总览

| 分类 | 数量 | 卡 |
|------|------|----|
| **A（可迁移动态属性钩子）** | **0** | —（清单内无动态 atk/health 钩子；见 §2 证伪） |
| **B（其它动态状态：进度/计时/标记/派生 token 属性）** | **11** | Forest Rover、Buzzing Vermin、Patient Scout、Gluttonous Trogg、Scarlet Survivor、Sky Admiral Rogers、Upbeat Frontdrake、Upbeat Duo、Zesty Shaker、Felemental、Catacomb Crasher |
| **C（证据不足）** | **6** | Wrath Weaver、Groundbreaker、Soul Rewinder、Brazen Buccaneer、Fire-forged Evoker、Devout Hellcaller |

## 2. 为什么 A=0（证伪证据）

- 17 张卡的脚本均无 `atk`/`health` 钩子；属性变化全部经
  `actions.Buff`（永久附魔）或 token 模板 `BASE_ATK/HEALTH`：
  Wrath Weaver/Groundbreaker/Devout Hellcaller/Brazen Buccaneer/
  Catacomb Crasher = `Buff`；Forest Rover/Buzzing Vermin = token 模板；
  Fire-forged Evoker = SoC `Buff`；其余 = 计时/进度/标记。
- 这些卡在 trace 的打印 atk/health 已包含 Buff 结果，引擎复现的是
  Buff 序列；官方 `ScriptDataNum*` 记录的是**状态量**（进度/计时/
  标记/派生 token 属性），不是"本卡 atk/health 的独立动态钩子"。
  因此不存在 F-10 式的"钩子重算"迁移对象；把 NUM_3 硬套到 Buff
  计数会违反二态铁律（无证据）。

## 3. B 类逐卡证据（状态量；可评估通道，但非本轮 A 迁移）

### 3.1 Forest Rover `BG31_801/_G`（gap 47）
- 卡面：BC "Your Beetles have +{2}/+{3} this game"（NUM_3/4=2/1）；
  DR "Summon a {0}/{1} Beetle"（NUM_1/2=2/2）。
- 官方值样本（trace `06e9e47d98dfcfef` T3）：`NUM_1=4, NUM_2=3,
  NUM_3=2, NUM_4=1`；打印 Forest Rover 1/1。
- 解释：`4/3 = Beetle 基础 2/2 + 兽族光环 2/1`（已由 BC `ApplyRaceAura`
  建模）；`NUM_3/4` 与 CardDef 相同（故无 gap），gap 来自
  `NUM_1/2`（官方=当前 Beetle 最终属性）。
- 分类 **B（派生 token 属性）**；引擎已能由 base+race aura 复算，
  属 **测量口径**问题，不需要 NUM_3。

### 3.2 Buzzing Vermin `BG31_803/_G`
- 卡面：DR Summon a {0}/{1} Beetle（NUM_1/2=2/2）。
- 样本 `06e9e47d98dfcfef`：NUM_1=4,NUM_2=3（同 Forest Rover 的 Beetle
  最终属性）；打印 1/1。分类 **B（派生 token 属性）**。

### 3.3 Patient Scout `BG24_715/_G`（gap 38）
- 卡面：卖出时 Discover Tier @（NUM_1=1 基值，"Improves each turn"）。
- 样本 `06bbbdf2c3b413f5` T5：NUM_1=4；`0b7dc2a3f20cd48d` T5：NUM_1=2/4；
  打印 1/1/2/2。官方 = 当前 Discover tier（2..6，与 turn 相关）。
- 分类 **B（计时进度状态）**；引擎按 `num0 + turn -1` 计算，未读
  官方值。若迁移应读 **NUM_1**（不是 NUM_3），并仅在招募路径生效。

### 3.4 Gluttonous Trogg `BG35_801/_G`（gap 37）
- 卡面："Once you buy 4 cards, gain +{1}/+{2} ({0} left!)"。
- 样本 `04d5275e21f1172c` T3：NUM_1=2,NUM_2=4,NUM_3=4；打印 2/3；
  `0502f1b15163d311`：NUM_1=1；打印 3/5。官方 NUM_1 = 已购买进度
  （1..4），NUM_2/3 = CardDef 静态增益（无 gap）。
- 分类 **B（购买进度）**；引擎监听购买事件计数，语义一致；可把
  官方 NUM_1 作基线。

### 3.5 Scarlet Survivor `BG35_814/_G`（gap 37）
- 卡面："Once this reaches {0} Attack, gain Divine Shield (Done!)"
  （NUM_1=6）。
- 样本 `09168073d1c26621`：NUM_1=6（=CardDef，无 gap），NUM_2=1
  （CardDef None → gap）；打印 24/20/28/30。NUM_2=1 = **Done 标记**
  （已获圣盾）。
- 分类 **B（一次性标记）**；引擎已有 `_scarlet_done` 等价状态。

### 3.6 Sky Admiral Rogers `BG33_823/_G`（gap 33）
- 卡面："After you spend {2} Gold, get a random Bounty ({0} Gold left!)"
  （NUM_0=9 显示、NUM_2=9 阈值）。
- 样本 `05acccc0d1c89008`：NUM_1=6,NUM_3=9；`0834b4802acd88bb`：NUM_1=3；
  打印 78/76、615/920。NUM_3=9=CardDef（无 gap）；NUM_1=已花金币进度
  （0..9）≠CardDef NUM_1=9 → gap。
- 分类 **B（花费进度）**；引擎 `_rogers_progress` 已有等价状态。

### 3.7 Upbeat Frontdrake `BG26_529/_G`
- 卡面："At the end of every 3 turns, get a random Dragon (X turns left!)"
  （NUM_1=3 间隔）。
- 样本 `0b7dc2a3f20cd48d`：NUM_1=1；`0c49e393d82e99b0`：NUM_1=3；
  打印 1/1、5/5/6/6。NUM_1 = 剩余回合（1..3）≠CardDef 3。
- 分类 **B（计时状态）**；引擎 `_upbeat_frontdrake_left` 等价。

### 3.8 Upbeat Duo `BG26_199/_G`
- 卡面："At the end of every 2 turns, get a plain copy ... (X turns left!)"
  （NUM_1=2 间隔）。
- 样本 `0e3aaacd39f173e6`：NUM_1=1；`0970bf8f41826f5d`：NUM_1=2；
  打印 11/11、27/19。NUM_1 = 剩余回合（1..2）≠CardDef 2。
- 分类 **B（计时状态）**；引擎 `_upbeat_duo_left` 等价。

### 3.9 Zesty Shaker `BG26_505/_G`
- 卡面："Once per turn, when a Spellcraft spell is played on this, get
  a new copy of it."（CardDef nums 全 None）。
- 样本 `0a15c937184cd400`：NUM_1=1；打印 90/84。NUM_1 = 本回合已用
  标记（0/1）。
- 分类 **B（每回合一次标记）**；引擎事件守卫等价。

### 3.10 Felemental `BG25_041/_G`
- 卡面：BC "Give minions in the Tavern +{0}/+{1} this game"
  （NUM_1/2=2/1；金色 twice）。
- 样本 `00e73481dae255f5`：NUM_1=4,NUM_2=3（打印 250/217，golden）；
  `02b2ef86c6f36613`：NUM_1=2,NUM_2=1（=CardDef，无 gap）。
- 解释：官方值 = 当前酒馆持久 Buff 累计（多张/金色叠加）；引擎
  `hero.tavern_buffs` 已建模，属 **测量口径**（B：派生累计值）。

### 3.11 Catacomb Crasher `BG30_129/_G`
- 卡面："Whenever you would summon a minion that doesn't fit, give
  your minions +{0}/+{1} permanently"（NUM_1/2=2/2；金色 4/4）。
- 样本 `0a15c937184cd400`：NUM_1=2,NUM_2=2（=CardDef，无 gap）；
  打印 27/30。部分 gap 来自**金色/基础卡解析错位或累计 Buff**
  （官方值等于 CardDef 的 golden/base 对应值）。
- 分类 **B（测量口径/派生 Buff 值）**，非动态计数。

## 4. C 类逐卡证据（证据不足 → DEFERRED）

| 卡 | 官方值样本 | 与文本/引擎的冲突 | 缺失证据 |
|----|------------|-------------------|----------|
| Wrath Weaver `BGS_004` | `NUM_1=2,NUM_2=1`（恒定，41 次） | 文本 +2/+2；打印 1/4、3/5、7/10；值不随触发次数变化 | 官方 Power.log 该 tag 含义（是否 enchant 字段）；wiki Notes |
| Groundbreaker `BG31_035` | (1=2,2=29,3=2)、(1=4,2=10,3=4) | 文本 "every 3 spells improve" + 打印 86/36、543/491；三槽关系不明（NUM_2 可能 spell 计数） | 官方 tag 槽位语义 + 事件级日志 |
| Soul Rewinder `BG26_174` | 有时 NUM_1=27，有时 NUM_2=5/9/21 | 文本固定 +{2} Health（NUM_3=2）；NUM_1/2 含义未知（可能 rewind 快照/触发计数） | Power.log + wiki Notes；rewind 快照字段 |
| Brazen Buccaneer `BG35_701` | (1=2,2=3)、(1=2,2=6)、(1=2,2=3,3=20) | 文本 EoT 按本回合打出卡数重复；打印 100/88、541/571；槽位无法对齐 | 官方 tag 语义（重复次数 vs Buff 值） |
| Fire-forged Evoker `BG32_822` | (62,31,30)、(168,84,82)、(150,75,73) | 非金：NUM_1=2×NUM_2、NUM_2=NUM_3+1；金：168=2×84，但金 num0=4 → 与"num0×(1+count)"不一致 | 官方 Improve 计数公式 + 金色 tag 是否用基础 num；Power.log |
| Devout Hellcaller `BG33_155` | (1=1,2=2)、(1=2,2=4) | 文本 +2/+2 permanent；打印 74/89、320/570（累计 Buff）；tags 与触发次数关系不明 | 官方 trigger 计数 tag 语义；Power.log |

## 5. B 类需要的 harness/引擎通道（汇总）

> 原则：**不硬套 NUM_3**；按卡面语义读对应槽位；以下均为
> "召回/计时/标记/派生值"，不是 atk/health 动态钩子。

1. **进度/计时（引擎可在招募期读官方值作基线）**：
   - Patient Scout：`NUM_1`（当前 Discover tier）
   - Gluttonous Trogg：`NUM_1`（购买进度）
   - Sky Admiral Rogers：`NUM_1`（已花金币进度）
   - Upbeat Frontdrake / Duo：`NUM_1`（剩余回合）
   - Zesty Shaker：`NUM_1`（本回合已用标记）
   - Scarlet Survivor：`NUM_2`（Done 标记）
   若迁移：脚本在对应钩子用 `script_data_or_card(n)` 作基线（默认
   无注入行为不变），并在 diff-harness 的 `CONSUMED_SCRIPT_DATA`
   登记对应序号；仅招募期生效，不改变战斗回放。
2. **派生/累计值（无需引擎消费，属测量口径）**：
   - Forest Rover / Buzzing Vermin：`NUM_1/2` = Beetle 最终属性
     （base+Beast aura，引擎可复算）。
   - Felemental：`NUM_1/2` = 当前酒馆持久 Buff 累计（引擎
     `hero.tavern_buffs` 可复算）。
   - Catacomb Crasher：官方值等于 CardDef（golden/base 解析错位）。
   建议 diff-harness 对这批 "派生值" 比较**引擎计算结果**而非
   CardDef 静态值，或明确豁免（需 Lead 批）。
3. **C 类**：暂不登记消费；需官方事件级日志/Notes 后再判。

## 6. 本轮结论 / 下一步

- **A=0**：本清单无 F-10 式动态 atk/health 钩子迁移对象；不存在
  "改脚本即降 context_gap" 的 A 类项。**本轮不改引擎/脚本**，
  diff 维持 F-10 后状态（MATCH 399、context_gap 62、
  unmodeled_dynamic_state 57）。
- **B=11**：语义清楚但属其它动态状态；建议下一批按 §5 逐项接
  `script_data_or_card(n)`（非 NUM_3）+ harness `CONSUMED_SCRIPT_DATA`
  登记；仅招募期效果，风险低。
- **C=6**：维持 DEFERRED；缺证据见 §4（优先采集真实 BG Power.log）。
- 缺失脚本卡（Moonsteel Juggernaut `BG31_171`、Dastardly Drust
  `BG32_234` 等）不在本清单，属另一类 `missing_script`，需先补脚本。

## 7. 实现状态（2026-10-18，B 类迁移 + F-12 原语）

### 7.1 B 类落地（7 张，默认无注入逐位不变）

| 卡 | 消费槽位 | 实现 | 测试 |
|----|----------|------|------|
| Patient Scout `BG24_715/_G` | NUM_1 | `on_sell` 用注入 tier 覆盖 turn 公式 | `test_dynamic_state_migration.py` |
| Gluttonous Trogg `BG35_801/_G` | NUM_1 | 购买进度基线（≥阈值 = 已 Done 不重放 Buff） | 同上 |
| Sky Admiral Rogers `BG33_823/_G` | NUM_1 | "Gold left" → `progress = threshold - injected` | 同上 |
| Upbeat Frontdrake `BG26_529/_G` | NUM_1 | 剩余回合基线 | 同上 |
| Upbeat Duo `BG26_199/_G` | NUM_1 | 剩余回合基线 | 同上 |
| Zesty Shaker `BG26_505/_G` | NUM_1 | 已用标记 → 本回合 `_shaker_turn` | 同上 |
| Scarlet Survivor `BG35_814/_G` | NUM_2 | "(Done!)" → 置 `_scarlet_done` + 补 DS | 同上 |

- 引擎侧基础设施：`Entity.is_script_data_injected(num)`（区分
  CardDef 静态初始化与官方轨迹注入；`Game.set_entity_script_data` /
  `set_script_data` 会标记，静态初始化不标记）。
- **默认行为**：无注入时所有分支走旧逻辑；新增 13 例测试 +
  既有批次测试全绿（总计 5,090 passed）。
- **待 diff-harness 登记 `CONSUMED_SCRIPT_DATA`**（卡 id → 槽位）：
  ```
  BG24_715/_G → {1}      BG35_801/_G → {1}
  BG33_823/_G → {1}      BG26_529/_G → {1}
  BG26_199/_G → {1}      BG26_505/_G → {1}
  BG35_814/_G → {2}
  ```
- 预期收益（临时 harness 副本估算，未改正式 harness）：
  * 全部登记：`context_gap 80→75`、`unmodeled 73→68`、
    干净子集 `19→33/38`（86.8%），但暴露 **5 条既有
    behavior_mismatch**（`BG35_801`/`BG35_814`/`BG26_529` 相关：
    `0502f1b15163d311`、`3e4c0c97c6c32601`、`4fb28230d318b126`、
    `aa771aaa615fb618`、`be480c4c15e54656`）——这些不是 B 脚本引入
    （受控 pre/post B 脚本 0 翻转），是原先被 context_gap 状态遮蔽的
    行为分歧；登记前需先处理/裁定。
  * **安全子集**（推荐先登记：Patient Scout / Rogers / Upbeat Duo /
    Zesty Shaker）：干净子集 `19→24`，`behavior_mismatch=0`，
    MATCH/context 不变；无副作用。
  * 5 条暴露分歧的根因与最小 harness 改动见
    `reversed/rea/behavior_mismatch_5_rootcause.md`：把
    `hand_not_modeled`/`*_hero_power_unknown` 归入 unmodeled_context 后，
    全部 B 卡登记可保持 behavior_mismatch=0、干净子集 33/33。
- 派生值 4 张（Forest Rover `BG31_801`、Buzzing Vermin `BG31_803`、
  Felemental `BG25_041`、Catacomb Crasher `BG30_129`）**不接脚本
  消费**：官方 NUM 值 = 引擎可复算的派生值（Beetle 最终属性 /
  酒馆 Buff 累计 / CardDef 静态值）；建议 harness 比较引擎计算值或
  豁免，而非要求脚本读通道。

### 7.2 F-12 原语

| 原语 | 状态 | 说明 |
|------|------|------|
| `Game.minion_gained(hero, minion)` | ✅ 实现 | `Hero.add_to_hand` 单点广播（购买/Discover/生成/排队入手统一，满手排队不广播）；测试 5 例；已用于 `BG32_234 Dastardly Drust`（batch_37x_gapfill） |
| `Game.combat_spell_extra_casts(hero)` | ✅ 实现 | 默认 0；`cast_spell_effect` 战斗内按值重复结算（额外每 cast `cast_no` 递增，目标失效即停）；测试 6 例；已用于 `BG34_922 Maelstrom Emergent`（普通 1/金色 2） |
