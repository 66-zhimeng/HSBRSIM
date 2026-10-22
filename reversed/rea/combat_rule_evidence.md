# 争议机制取证：Reborn / 剧毒(毒/烈毒) / 顺劈 / 死亡波次

> 只读分析（未改 `hsrl2/`、`data/`）。2026-10-18，rea-operator。
> 证据层级（高→低）：**官方客户端数据/代码** > **官方卡牌文本（CardDefs）** > **BobsBuddy 反编译（第三方，非官方）** > **HDT 日志（仅战前快照+胜负）**。
> 统计脚本：`reversed/rea/scripts/combat_rule_stats.py`（间接检验，可复跑）。

## 0. 结论总表

| # | 争议点 | 日志**直接**证据 | 官方数据/文本证据 | 第三方实现(BobsBuddy) | 引擎当前 | 结论/置信度 |
|---|--------|------------------|--------------------|------------------------|----------|--------------|
| 1 | Reborn 复生后是否保留 buff/金色/关键词 | **NO EVIDENCE**（无逐事件/复生后属性） | `GLOBAL_KEYWORD_BACON_REBORN_TEXT`："召唤一个具有1点生命值的**新的复制**"；`GLUE_TIP_BACON_21`："**亡语**会在**复生**之前触发" | `OnReborn` 默认 `CloneAsVanilla()`（原版克隆，丢附魔；金色只翻倍基础值） | 同一实体放回：1 血、保留 buff/金色/关键词 | buff 保留：**NO EVIDENCE/低**；"亡语先于复生"：**高（官方 tip）** |
| 2 | 毒/烈毒是否"造成即毁灭"、是否要求攻击者存活 | **NO EVIDENCE** | `GLOBAL_KEYWORD_POISONOUS_TEXT`："消灭任何受到其伤害的随从"；`GLOBAL_KEYWORD_VENOMOUS_TEXT`："每场战斗可以消灭第一个受到其伤害的随从" | 扣血阶段直接 `baseHealth=-10000`，**不要求来源存活**（`BB:7533-7548`） | 猛毒要求 `not attacker.dead` 且在第一波死亡结算**之后**才判定（`CB:315-328`） | 规则语义（造成伤害即毁灭）**高（官方文本）**；引擎"攻击者存活"门槛**很可能错**；同波时序 **NO EVIDENCE** |
| 3 | 顺劈：攻击者被反击打死时相邻伤害是否结算 | **NO EVIDENCE** | 无中文 CLEAVE 关键词文本；官方 CardDefs 卡面："Also damages the minions next to whomever it attacks."（例 `AT_067` Magnataur Alpha，`hsdata/CardDefs.xml:6323`；BG 例 `BG26_817` Blade Collector） | 顺劈与主目标/反击**同一伤害组**，攻击者死亡不影响（`BB:6552-6581`） | `if attacker.cleave and not attacker.dead`：攻击者死则**跳过顺劈**（`CB:295-304`） | **中高**：同时伤害 + 卡面文本 → 引擎门槛可疑；无事件级日志证据 |
| 4 | 死亡波次 / 复生时机（多亡语同时死亡） | **NO EVIDENCE**（无 death/damage/zone 事件） | `GLUE_TIP_BACON_21`：亡语先于复生（**高**）；无"同时死亡顺序"官方文本 | 整批收集死亡→统一跑亡语→统一复生；**双方死亡先后随机**（`BB:7128-7145/7623-7757`） | 逐随从"离场→亡语→**立即复生**→Avenge"；波次内按固定顺序（`GM:2438-2590`） | "亡语先于复生"**一致（官方高）**；多死亡**批处理/顺序 NO EVIDENCE**，引擎与 BB 的差异（立即复生 vs 批后复生）未验证 |

## 1. 数据可得性（为什么大多是 NO EVIDENCE）

| 观测项 | 是否可得 | 证据 |
|--------|----------|------|
| 战前战团快照（名字/攻血/关键词/`ScriptDataNum*`/外部亡语注释/黄金） | ✅ | 679 个 `Simulation Input` 块（27 个 HDT 日志） |
| 官方战斗结果（Win/Loss/Tie） | ✅ 524/679 | `ValidateSimulationResultAsync >> result=...` |
| BobsBuddy 10k 次模拟概率 | ✅ 666/679 | `WinRate=... TieRate=... LossRate=...` |
| 逐事件伤害/死亡/属性变化/复生后属性 | ❌ | 日志无 DAMAGE/DEATH/ZONE_CHANGE/属性行；`events=[]`（`runs/official_traces/index.json` field_completeness） |
| 英雄血量/护甲/伤害 | ❌ | 关键词 `damage/armor/health` 命中 0（`health` 仅 `pBeetleHealth`） |
| 游戏侧 Power.log（事件级） | ❌ | 3 个常见路径均不存在（游戏近期未运行） |

**因此**：4 项机制都**不能**用日志"直接观察"结算顺序/复生后属性；
下面分别给"官方文本/代码（有则给）"+"BobsBuddy 实现（第三方）"+"引擎当前"，
并用**间接统计**（官方结果 vs 引擎/BB 预测）做方向性参考（明确标注非因果）。

## 2. 逐项取证

### 2.1 Reborn：复生后是否保留 buff / 金色 / 关键词

**日志直接证据：NO EVIDENCE**
- 日志只在**战前快照**里出现 `Reborn` 关键词；没有任何战斗内属性/复生后属性行。例：
  - `hdt_log.txt:1117`：`[Timewarped Pillager 9/4, Taunt, Reborn]`
  - `hdt_log.txt:1225`：`[Turquoise Skitterer 6/6, Reborn, ScriptDataNum1=12, ScriptDataNum2=12, ScriptDataNum3=5, ScriptDataNum4=5]`
  - 语料里含 Reborn 的轨迹：179/500（有官方结果），例 `00aae6bb87ac02a5`、`00cfafe3224325eb`。
- 可观测的只有"该场官方 win/loss/tie"，无法区分复生后是 1/1 原版还是 5/1 保留 buff。

**官方数据/文本证据**
- `GLOBAL_KEYWORD_BACON_REBORN_TEXT`（`Strings\zhCN\GLOBAL.txt:789`）：
  「在其首次死亡时**召唤一个具有1点生命值的新的复制**。」
  （"summon a **new copy** with 1 Health when it first dies"）→ 明确是"**新的复制**"，
  但**没有说明是否继承附魔/buff**；"新的复制"更像新建实体（支持 BobsBuddy 的 new-minion 语义），
  但 HS 的"复制(copy)"通常包含当前属性/附魔，因此**文本层面无法定论**。
- `GLUE_TIP_BACON_21`（`Strings\zhCN\GLUE.txt:3645`）：**「亡语会在复生之前触发。」**
  → 官方确认 **deathrattle → reborn** 顺序（与引擎 `GM:2523-2527` 一致）。
- `GLUE_TIP_BACON_11`（`GLUE.txt:3634`）：战斗阶段增益在战斗结束时消失
  → 只说明战斗内 buff 不跨战斗保留，不回答"复生体是否保留战斗内 buff"。

**BobsBuddy 反编译（第三方）**
- `Minion.OnReborn`（`reversed/rea/bobsbuddy/src_full/BobsBuddy.decompiled.cs:934-983`）：
  默认 `CloneAsVanilla()` → 1 血；仅当卡牌声明 `IRebornBehavior.KeepEnchantments`
  才 `Clone()` 保留附魔；`KeepBonusKeywords` 恢复 taunt/windfury/曾有的圣盾。
- `CloneAsVanilla`（`BB:1254-1268`）：`CreateFromCardId(CardID)` **新建一张基础卡实体**；
  金色仅把基础攻/血 ×2；**不复制 `_buffs`/enchantments**（但会重挂 `AttachedModularEntity`）。

**引擎当前**
- `Game._process_death_inner`（`hsrl2/game.py:2543-2565`）：**同一个 Minion 实体**放回原位，
  `HEALTH=1`、清 `REBORN`/`DEAD`，**保留**所有 buff/金色/关键词；
  `Persisting Horror` 特例满状态（`_reborn_full_uuids`）。

**判定与置信度**
- "亡语先于复生"：**一致，官方文本，置信度高**。
- "复生体是否保留 buff/金色/关键词"：**NO EVIDENCE（官方文本不足以定论；无日志事件）**。
  - BobsBuddy = 原版克隆（丢 buff）；引擎 = 保留 buff。**两者都可能错**，不能凭 BobsBuddy 改引擎。
  - 需要 Power.log：寻找 `tag=REBORN`/`ZONE_CHANGE`/复生体 `tag=ATK/HEALTH` + `BUFF` 相关 tag
    的变化，或复生体后续 `DAMAGE` 数值（若复生体攻击力仍是 buff 后数值→保留）。
- 间接统计（§3）：`reborn_buffed` 子集官方更偏向 BobsBuddy（31:5），但**混杂缺脚本**，
  **不能作为改引擎依据**（clean 子集仅 1 条）。

### 2.2 剧毒 / 烈毒：是否"造成即毁灭"、是否要求攻击者存活

**日志直接证据：NO EVIDENCE**
- 只有战前快照里的关键词：`hdt_log.txt:683`：`[Bile Spitter 23/41, G, Venomous]`；
  `hdt_log_1777990417.txt:483`：`[Pufferquil 13/18, Venomous]`。
- 语料里含 Venomous/Poisonous 的轨迹 45/500（例 `00cfafe3224325eb`）；
  无任何"某随从被毒杀"的逐事件记录。

**官方数据/文本证据（强）**
- `GLOBAL_KEYWORD_POISONOUS_TEXT`（`GLOBAL.txt:1021`）：**「消灭任何受到其伤害的随从。」**
- `GLOBAL_KEYWORD_VENOMOUS_TEXT`（`GLOBAL.txt:1231`）：**「在每场战斗中可以消灭第一个受到其伤害的随从。」**
- 两条都是"**受到其伤害即消灭**"，**没有**"来源需存活"的条件；
  伤害是同时结算的（攻击/反击同一时点），因此"互殴同归于尽时毒仍生效"是文本的自然推论。

**BobsBuddy 反编译（第三方）**
- `Simulator.ProcessDamage`（`BB:7533-7548`）：扣血阶段
  `if (source is Minion m && m.venomous) { target.baseHealth = -10000; m.venomous = false; }`
  `else if (m.poisonous) target.baseHealth = -10000;`
  → **不检查来源是否存活**；猛毒每场一次（用完置 false）；圣盾/预防伤害先行 `continue`。

**引擎当前**
- `hsrl2/combat.py:315-328`：
  ```python
  if (actual_to_defender > 0 and attacker.venomous_active
          and not attacker.dead and not defender.dead): ...  # ← 要求攻击者存活
  if (actual_to_attacker > 0 and defender.venomous_active
          and not defender.dead and not attacker.dead): ...
  ```
  且位于 `game.check_deaths()`（`CB:307`，第一波死亡结算）**之后**——
  毒杀会落到第二个死亡波次（`CB:337`）。
- 剧毒（`CB:330-335`）没有"攻击者存活"要求，但同样在死亡波之后。

**判定与置信度**
- "造成伤害即毁灭、不要求来源存活"：**官方文本支持，置信度高**；
  → 引擎的 `not attacker.dead` 门槛**很可能与官方不符**（互殴时猛毒方先死则毒不触发）。
- "毒杀是否与普通伤害同波结算"：**NO EVIDENCE**（只影响亡语/Avenge 波次；
  官方未给文本，日志无事件）。BobsBuddy 是同波（扣血阶段即死亡标记），引擎分两波。
- 间接统计（§3）：venom 子集 45 条，引擎 75.6% vs BB 86.7%；冲突 7 条中官方选 BB 5、
  引擎 0、都不符 2；clean 子集 0 → **方向性，不因果**。

### 2.3 顺劈（Cleave）：攻击者被反击死亡时相邻伤害是否仍结算

**日志直接证据：NO EVIDENCE**
- 战前快照示例：`hdt_log.txt:1283`：`[Blade Collector 251/220, G, Cleave]`；
  `hdt_log_1777991602.txt:366`、`hdt_log_1777992002.txt:395` 同一张卡。
- 语料含 Cleave 轨迹 32/500（例 `00cfafe3224325eb`、`00e73481dae255f5`）；
  无逐事件伤害/死亡记录。

**官方数据/文本证据**
- 中文本地化中**没有** CLEAVE 关键词定义（`GLOBAL.txt` 的 `GLOBAL_KEYWORD_*` 列表无 CLEAVE；
  搜索"顺劈"/"CLEAVE"只命中 VO 语音）。→ 关键词定义文本不可得。
- 官方**卡牌文本**（`hsdata/CardDefs.xml`，build 255130）：
  `AT_067` Magnataur Alpha（`:6323`）「Also damages the minions next to whomever he attacks.」；
  BG 卡 `BG26_817` Blade Collector / `BG_GVG_113` Foe Reaper 4000 同款文本。
  → 顺劈伤害是**该次攻击**的一部分；HS 攻击伤害同时结算，**文本没有"攻击者存活"条件**。

**BobsBuddy 反编译（第三方）**
- `PerformAttack`（`BB:6552-6581`）：主目标 + `target.GetLeftNeighbor()` +
  `target.GetRightNeighbor()` + 反击被放进**同一个 `List<Damage>`**，然后
  `ProcessDamage(list2)`（`BB:7470-7591`，组内先全部扣血）→
  **攻击者会死于反击也照常结算顺劈**。

**引擎当前**
- `hsrl2/combat.py:295-304`：先执行反击 `attacker.take_damage(...)`，然后
  `if attacker.cleave and not attacker.dead:` 才结算相邻伤害 → **攻击者死亡则顺劈丢失**。

**判定与置信度**
- 同时伤害语义 + 卡面文本 → **中高**置信度：顺劈不应因攻击者死亡而取消；
  引擎门槛可疑。**但**：没有官方关键词定义文本/事件日志直接证明"攻击者死亡"这一极端情形
  → 不作为"官方已确认"，建议按"中高置信修复候选"排期，并配 Power.log 验证。
- 间接统计（§3）：cleave 子集 32 条，引擎 68.8% vs BB 78.1%；冲突 6 条中官方选 BB 3、
  引擎 0、都不符 3；clean 0 → 方向性，不因果。

### 2.4 死亡波次 / 复生时机（多亡语同时死亡）

**日志直接证据：NO EVIDENCE**
- 无 death/damage/zone-change 事件；只有战前快照与战后胜负。
- 但日志能证明**外部亡语注释确实存在**（即官方上下文里"同一随从多亡语"很常见）：
  `hdt_log_1780064984.txt:1591`：
  `[Risen Rider 2/1, Taunt, Reborn, Deathrattles=[GenericDeathrattles.Crab]]`
  （另一条 `hdt_log.txt:1369`：`[Proud Privateer 51/33, Taunt, Deathrattles=[Crab, Crab]]`）。
  共 203 行含 `Deathrattles=[...]`，但都是**战前**的"该随从有哪些亡语"，
  **不含结算顺序**。

**官方数据/文本证据**
- `GLUE_TIP_BACON_21`（`GLUE.txt:3645`）：**「亡语会在复生之前触发。」** → 仅此一条官方顺序文本。
- 官方未提供"多个随从同时死亡时谁先结算/是否同波"的文本（无证据）。

**BobsBuddy 反编译（第三方）**
- `ResolveAllDeathsAndTriggers`（`BB:7128-7145`）：
  `do { ResolveDeaths(); ResolveTriggersInCurrentScope(); ResolveReborn(); } while (changed)`；
- `ResolveDeathsForSide`（`BB:7635-7757`）：先收集本侧死者（`CreatedDuringCombat` 排后）、
  注册亡语、跑 Avenge、**登记**复生，最后统一移除；`ResolveReborn`（`BB:7921-7953`）
  在整波亡语之后执行；
- **双方死亡先后随机**（`ResolveDeaths` 中 `SafeRandom.NextBool()`，`BB:7626-7632`）——
  这是 BobsBuddy 对"同时死亡顺序未知"的建模选择，**不是官方证据**。

**引擎当前**
- `Game.check_deaths`（`GM:2438-2458`）：按波次收集双方死者；`_process_death_inner`
  （`GM:2476-2590`）对**每个死者**按 死亡事件→离场→亡语→**立即复生**→Avenge 的顺序处理；
  波次内顺序固定（`self.heroes` + 棋盘顺序）。
- 与官方 tip 21 的关系：引擎确实是"亡语→复生"（单个死者层面），**一致**；
  但与 BobsBuddy 的差异在于"整波所有亡语跑完才复生"（引擎是逐死者立即复生）。

**判定与置信度**
- "亡语先于复生"：**一致（官方文本，高）**。
- "多亡语同时死亡的批处理/顺序"：**NO EVIDENCE**（无官方文本、无事件日志）。
  → 不建议现在改引擎；BobsBuddy 的"双方死亡随机"**不要照抄**（会破坏确定性）。
- 间接统计（§3）：deathrattle 卡子集 410 条，引擎 70.5% vs BB 88.8%；
  冲突 106 条中官方选 BB 86、引擎 11、都不符 9；但该子集混杂"引擎缺亡语脚本"
  （247 条 missing_script 的主体），**不能归因于结算顺序**。

## 3. 间接统计（官方结果 vs 引擎/BB 预测；**非因果**）

> 方法：`reversed/rea/scripts/combat_rule_stats.py` 读 `runs/diff_report.json`。
> `clean` = 无 `no_script/detected_effect/dynamic_script_data/deathrattle_annotation/
> hero_power_state/unknown_keyword/dynamic_stat_script` 缺口。
> **注意**：BobsBuddy 实现的卡/机制比引擎多，官方更常与 BB 一致不能直接归因到本表机制。

| 子集 | 有官方结果 | 引擎命中官方 | BB 命中官方 | 引擎 vs BB 冲突 | 官方选 BB | 官方选引擎 | 都不符 | clean 子集 |
|------|-----------|--------------|-------------|----------------|-----------|------------|--------|-----------|
| 全部 | 500 | 72.6% | 90.0% | 120 | 99 | 12 | 9 | 14（引擎 85.7%/BB 100%） |
| Reborn（任意） | 179 | 67.6% | 89.4% | 52 | 44 | 5 | 3 | 2（引擎 0/2，BB 2/2） |
| Reborn（复生单位数值≠CardDef 基础值=有 buff/金色） | 140 | 68.6% | 87.1% | 39 | 31 | 5 | 3 | **1** |
| Venomous/Poisonous | 45 | 75.6% | 86.7% | 7 | 5 | 0 | 2 | **0** |
| Cleave | 32 | 68.8% | 78.1% | 6 | 3 | 0 | 3 | **0** |
| 卡面含亡语（文本代理，488 张） | 410 | 70.5% | 88.8% | 106 | 86 | 11 | 9 | 4（两方均 100%） |

**读数**：
- 方向上天平偏向 BobsBuddy（尤其 Reborn 子集），但 **clean 子集样本 0-2 条**，
  且 BB 覆盖更多卡 → **不能作为"引擎某条机制错了"的因果证据**；
- 对 2.2/2.3 的"引擎门槛可疑"结论，主要依据是**官方文本/卡面文本**，不是这张表；
- 对 2.1（复生 buff）与 2.4（批次顺序），这张表**不足以定论**。

## 4. 建议修复优先级（供 Lead 排序；**不要**直接据此改引擎）

| 优先级 | 项 | 依据强度 | 建议 |
|--------|----|----------|------|
| **P1** | 顺劈：攻击者被反击死亡时仍应结算相邻伤害（`CB:295-304`） | 官方卡面文本 + 同时伤害语义（中高）；BobsBuddy 同行为 | 写失败测试（1/2 Cleave 撞 5/5 + 两侧各 1/1 → 两侧应死），Lead 批准后改 |
| **P1** | 毒/烈毒：去掉"攻击者存活"门槛（`CB:315-328`）；并考虑把毒杀并入同一死亡波 | 官方关键词文本（高：造成伤害即毁灭）；同波时序无证据 | 先修"门槛"（低风险）；"同波"改动影响亡语/Avenge，需单独测试 |
| **P2** | Reborn 复生体 buff/金色/关键词 | **NO EVIDENCE**（官方"新的复制"未言明附魔；无日志事件） | **暂不改**；采集 Power.log（见下）后再定；当前 bob 为原版克隆、引擎为保留，二者都未被官方文本确认 |
| **P2** | 死亡波次批处理/复生时机 | 官方只确认"亡语先于复生"（引擎已一致）；批次/顺序无证据 | **暂不改**；不要照抄 BobsBuddy 的"双方死亡随机"（破坏确定性） |
| **P3** | 用日志间接统计持续监控上述子集 | 弱（混杂） | 每轮补脚本/数据后重跑 `combat_rule_stats.py`，看 clean 子集是否扩大 |

**取证下一步（可执行）**：下次游戏运行后采集 **Power.log**，用事件级数据回答：
- Reborn：复生体 `ZONE_CHANGE` 后的 `ATK/HEALTH` 与复生前 buff 对比（是否保留）；
  以及 `BUFF`/`ENCHANTMENT` 相关 tag 是否随复生重置。
- 毒：`DAMAGE` 事件与 `DEATHS` 批次顺序；互殴同归于尽时毒是否仍生效。
- 顺劈：同一次 `ATTACK` 下 `DAMAGE` 目标集合是否包含死亡攻击者的邻居。
- 死亡波次：一次死亡批次中的 `DEATHRATTLE`/`REBORN` 事件顺序。
解析器基线已在 `reversed/rea/scripts/damage_cap_extract.py`（可扩展为通用 Power.log 事件解析）。

## 5. 附：本次用到的官方证据索引

| 证据 | 路径:行 |
|------|---------|
| BG Reborn 关键词文本 | `C:\Program Files (x86)\Hearthstone\Strings\zhCN\GLOBAL.txt:788-789` |
| 亡语先于复生 tip | `Strings\zhCN\GLUE.txt:3645`（`GLUE_TIP_BACON_21`） |
| 战斗增益临时 tip | `Strings\zhCN\GLUE.txt:3634`（`GLUE_TIP_BACON_11`） |
| 剧毒关键词文本 | `Strings\zhCN\GLOBAL.txt:1020-1021` |
| 烈毒关键词文本 | `Strings\zhCN\GLOBAL.txt:1230-1231` |
| 顺劈卡面文本（示例） | `hsdata\CardDefs.xml:6323`（AT_067）、`:1510663`（BGS_061 区域）、`BG26_817`/`BG_GVG_113` |
| BobsBuddy Reborn | `reversed/rea/bobsbuddy/src_full/BobsBuddy.decompiled.cs:934-983, 1254-1268, 7921-7953` |
| BobsBuddy 毒 | 同上 `:7533-7548` |
| BobsBuddy 顺劈/伤害组 | 同上 `:6552-6581, 7470-7591` |
| BobsBuddy 死亡批处理 | 同上 `:7128-7145, 7623-7757` |
| 引擎 Reborn | `hsrl2/game.py:2543-2565` |
| 引擎 毒 | `hsrl2/combat.py:315-328` |
| 引擎 顺劈 | `hsrl2/combat.py:295-304` |
| 引擎 死亡波次 | `hsrl2/game.py:2438-2458, 2476-2590` |
| 日志示例（战前快照） | `hdt_log.txt:1117/1225/683/1283`、`hdt_log_1780064984.txt:1591` |

## 6. 复现

```powershell
cd C:\HSagent
$env:PYTHONIOENCODING="utf-8"
C:\Python313\python.exe reversed\rea\scripts\combat_rule_stats.py   # 打印 §3 统计 JSON
```

> 统计依赖 `runs/diff_report.json`（diff-harness 产物）；若该文件被重新生成，
> 数值会变，脚本可重复运行。
