# BobsBuddy 1.34.11 vs hsrl2 战斗引擎 — 逐条对比

> 参照实现：BobsBuddy（社区酒馆战棋离线战斗模拟器，HDT 内置），反编译证据
> `reversed/rea/bobsbuddy/src_full/BobsBuddy.decompiled.cs`（行号下称 `BB:`），
> 宿主输入构造 `reversed/rea/hdt_bobsbuddy/BobsBuddyInvoker.cs`（下称 `INV:`）。
> 本项目引擎：`hsrl2/combat.py`（`CB:`）、`hsrl2/game.py`（`GM:`）、`hsrl2/constants.py`（`C:`）、
> `hsrl2/minion.py`（`MN:`）。
>
> 结论口径：BobsBuddy 是**社区参照实现，非官方**；"分歧"= 两者行为可观测不同，
> 官方以 Power.log 实测为准（本机当前无 Power.log，见 `docs/REA_FINDINGS.md` §6）。

## 0. 总览（按严重度排序）

| # | 主题 | BobsBuddy | hsrl2 | 判定 | 严重度 |
|---|------|-----------|-------|------|--------|
| 1 | 复生属性 | `CloneAsVanilla()`：丢 buff/金色加成，1 血；仅关键字类保留 taunt/windfury/曾有的圣盾（BB:934-983） | 同一实体原位复活，**保留 buff/金色/关键词**，仅清除 REBORN（GM:2543-2565） | **分歧** | P0 |
| 2 | 猛毒/剧毒生效条件 | 扣血阶段直接 `baseHealth=-10000`，**不要求攻击者存活**，组内先结算（BB:7533-7548） | 双向伤害+死亡波次**之后**才判猛毒，且要求 `not attacker.dead`；剧毒也晚一波（CB:307-337） | **分歧** | P0 |
| 3 | 顺劈 | 与主目标/反击同一伤害组，**攻击者死亡照常结算**（BB:6552-6581） | `if attacker.cleave and not attacker.dead`：**反击打死攻击者则顺劈丢失**（CB:295-304） | **分歧** | P0 |
| 4 | 伤害上限来源 | 服务端 tag `BACON_COMBAT_DAMAGE_CAP`(2089)（`BACON_COMBAT_DAMAGE_CAP_ENABLED`=3403 门控），`INV:652-655` | 本地 turn 表：≤3 回合 5 / ≤7 回合 10 / 之后 15，存活≤4 取消（C:42-57） | **分歧（数据源）** | P0 |
| 5 | 死亡波次内顺序 | 整批死者**先注册全部亡语并移除**，再统一跑亡语，最后统一复生；**双方先后随机**；战斗内召唤的随从排最后死（BB:7623-7757, 7128-7143） | 逐随从全流程：离场→亡语→**立刻复生**→Avenge；固定英雄顺序；无 `CreatedDuringCombat` 排序（GM:2438-2590） | **分歧** | P0 |
| 6 | 开局战斗效果顺序 | Anomaly → 任务奖励 → 饰品 → 英雄技能 → Objectives → 手牌 → **随从两侧交替**；先手侧=棋盘大者，平随机（BB:8265-8524） | 饰品 → 任务奖励 → 随从 → 手牌 → 英雄技能；固定 `a` 侧全跑完再 `b` 侧（CB:129-156） | **分歧** | P0 |
| 7 | Mega-Windfury | `megaWindfury` → 同一轮 4 次攻击（BB:6408-6414, 6608） | 只判 `windfury` 两次；`MEGA_WINDFURY` tag 存在（tags.py:139）但**无实现** | **我们缺实现** | P1 |
| 8 | 伤害同时性 | 同组全部先扣血，再统一触发（BB:7470-7591） | 逐条 `take_damage` + 逐条事件（CB:281-292, 300-304） | **分歧（可观测事件序）** | P1 |
| 9 | 攻击前死亡检查 | 伤害前 resolve 触发并结死，目标已死则**本次不造成伤害**（BB:6550-6581） | Rally 后不结死，但 `take_damage` 对已死目标返回 0、且反击 `def_power` 置 0（CB:272-284） | 基本一致（死亡结算时点不同） | P2 |
| 10 | Avenge 同波死者 | 同批死亡的所有 `IAvenge` 实体**不参与**本次计数（BB:7635, 7714-7720） | 只排除"正在处理的那一个"（GM:2577-2578） | **分歧** | P1 |
| 11 | 潜行规则 | 每轮 `CheckForStealth`：0 攻潜行失潜行；全员潜行→全部解除（BB:7108-7126） | 仅"潜行不可被选为目标"（CB:253） | **我们缺实现** | P2 |
| 12 | 平局/僵局伤害 | 仍按 tier 差计算有符号伤害（可能非 0）（BB:7002-7031） | 平局返回 0（GM:3499-3504） | **分歧（语义）** | P2 |
| 13 | 攻击轮转 | 每方左→右 + 全轮 `hasAttacked` 重置（BB:7072-7085） | 游标 `cursor` 取模 + 跳过 0 攻（CB:177-238） | 一致（实现不同） | — |
| 14 | 目标选择 | 存活非潜行；多嘲讽优先，**均匀随机**（BB:1051-1063） | 同（`rng.choice` 均匀）（CB:251-258） | **一致** | — |
| 15 | 先攻判定 | 随从多者先；平则随机（BB:8464-8491） | 同（CB:160-166） | **一致** | — |
| 16 | 风怒第 2 击目标 | 重新随机选目标（BB:6415-6418） | 重新 `_choose_target`（CB:232-236） | **一致** | — |
| 17 | 召唤上限 | `side.Count >= 7`（BB:8857） | `BOARD_SIZE`(7)（引擎侧一致） | **一致** | — |
| 18 | 随机分支分布 | 均匀 `Random.Next`；10000 次 Monte Carlo（BB:1411-1452, INV:46/1171） | `game.rng` 单次采样（确定性；分布均匀） | 一致（用途不同） | — |

---

## 1. P0：复生属性（BB:934-983 / GM:2543-2565）

**BobsBuddy**
```csharp
// Minion.OnReborn()
bool keepEnch = list.Contains(RebornBehavior.KeepEnchantments);
Minion minion = keepEnch ? Clone() : CloneAsVanilla();   // 默认 = 原版克隆
minion.reborn = false;
minion.baseHealth = 1;                                   // 默认 1 血
if (!keepEnch && list.Contains(KeepBonusKeywords)) { taunt/windfury 恢复; 曾有过圣盾则 div=1 }
if (list.Contains(KeepMaxHealth)) minion.baseHealth = maxHealth + passiveHealth;
if (list.Contains(KeepMaxAttack)) minion.baseAttack = maxAttack + passiveAttack;
```
即：**默认复生 = 原版克隆（丢弃所有 enchantment/buff/金色加成），1 血，仅按卡牌声明的
`IRebornBehavior` 决定是否保留 buff/最大攻血/关键字**（`RebornBehavior` 枚举见
`reversed/rea/bobsbuddy/...`：`KeepEnchantments=1, KeepBonusKeywords=2, KeepMaxHealth=3, KeepMaxAttack=4`）。

**hsrl2** 把同一个实体原位放回：`m.clear(REBORN); m.clear(DEAD); m.set(HEALTH,1)`，buff、金色、
关键词全部保留（仅 `Persisting Horror` 的 `_reborn_full_uuids` 特例满状态）。

**影响**：任何带 buff 的复生随从（跳蛙/机械/龙等）在两边引擎中属性完全不同 → 胜率与伤害都会偏。

**最小测试建议**（`hsrl2/tests/`，不改引擎也可先写成失败用例）：
1. 3/3 Reborn 随从 + 酒馆 buff 到 5/5，战死后复生 → 断言复生体 atk/health：
   BobsBuddy 语义 = **3/1**（原版攻、1 血）；当前引擎 = 5/1。
2. 带圣盾的 Reborn：BobsBuddy 仅在 `KeepBonusKeywords` 时恢复圣盾 → 断言默认不复生圣盾。
3. 金色 Reborn 复生：默认 clone-vanilla → 非金色原版？验证 `CloneAsVanilla` 对 golden 的处理
   （`Minion.CloneAsVanilla` 未在本文件展开，需再反编译确认——**待补证据**，见 §7）。

## 2. P0：猛毒/剧毒时机与条件（BB:7533-7548 / CB:307-337）

**BobsBuddy**：在 `ProcessDamage` 扣血阶段，只要该伤害条目 `amount != 0` 且未被圣盾/预防：
```csharp
if (source is Minion m && m.venomous) { target.baseHealth = -10000; m.venomous = false; … }
else if (source is Minion m && m.poisonous) { target.baseHealth = -10000; }
```
- **不检查来源是否存活**（同组反击随后打死来源也不影响）；
- 与普通伤害在**同一批**里生效 → 所有死亡（含毒杀）在**同一个死亡波次**统一处理；
- 圣盾完全吸收时 `continue`，不触发毒。

**hsrl2**：先 `defender.take_damage` / `attacker.take_damage` → 触发 damage 事件 → **顺劈** →
`check_deaths()`（第一批死亡：普通伤害死者，亡语/复生/Avenge 已跑完）→ 之后才判猛毒（要求
`attacker.venomous_active and not attacker.dead and not defender.dead`）/剧毒 → 第二次 `check_deaths()`。

**影响**：
- 攻击者被反击打死时猛毒**不生效**（官方/BobsBuddy 是生效的，"同归于尽但对方被毒死"）；
- 毒杀死亡被推迟到第二个死亡波次 → 亡语顺序、Avenge 计数波次、复生时机全部错位。

**最小测试**：
1. 1/1 猛毒 vs 5/5：双方互殴后应双方死亡（BobsBuddy）；当前引擎猛毒方死亡则不毒。
2. 场上 Avenge(3) + 普通死者 + 被毒死者：断言同波计数为 2 次而非分两波。
3. 有圣盾的猛毒攻击者：圣盾吸收 → 不消耗猛毒。

## 3. P0：顺劈与反击的同一伤害组（BB:6552-6581 / CB:295-304）

**BobsBuddy**：
```csharp
List<Damage> group = { new Damage(attacker.attack(), target) };
if (attacker.cleave) { group.Add(leftNeighbor); group.Add(rightNeighbor); }
if (!immune) group.Add(new Damage(target.attack(), attacker));   // 反击
ProcessDamage(group);   // 整组同时结算
```
伤害组在扣血**之前**构造，所有条目都会走完。因此**攻击者会被反击打死时，顺劈伤害仍然生效**。

**hsrl2**：
```python
actual_to_attacker = attacker.take_damage(def_power, …)          # 先扣反击
if attacker.cleave and not attacker.dead:                        # ← 已死则整段跳过
    for adj in adjacent: adj.take_damage(atk_power, …)
```

**影响**：低血顺劈随从（如 2 血 Cleave）撞大怪时，BobsBuddy 会带走两个邻居，hsrl2 不会。

**最小测试**：1/2 Cleave 攻击 5/5，两侧各 1/1 → BobsBuddy：两侧 1/1 死、攻击者死；
当前引擎：两侧 1/1 存活。另测顺劈相邻实体在**击杀主目标后**的邻居锁定（BobsBuddy 用扣血前
`GetLeftNeighbor/GetRightNeighbor`）。

## 4. P0：伤害上限的数据来源（INV:652-655 / C:42-57）

**BobsBuddy**（宿主 HDT 构造输入时）：
```csharp
if (_game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP_ENABLED) > 0)
    input.DamageCap = _game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP);
```
- cap 是**服务端下发的实体 tag**（2089），由 3403 门控；客户端/模拟器**不自行推导档位**。
- 模拟器只在最终伤害上 `clamp(±DamageCap)`（BB:7020-7030）。

**hsrl2**：`C.damage_cap(turn, alive)`：turn≤3→5，≤7→10，else 15；存活≤4 返回 None。

**影响**：若官方在某赛季/异变下改变档位（或 tag 与实际 turn 表不一致），hsrl2 会系统性偏差。
当前 turn 表与社区认知一致，但**权威值在 tag 里**。

**建议**（不需改引擎即可做）：
- `hsbg/bridge` 的 Power.log 解析把 GameEntity 的 `tag=2089`/`tag=3403` 抓出来，
  存到 bridge 数据资产；引擎侧保留 `_damage_cap_override`（GM:3139-3140 已存在）作为注入点。
- 最小测试：`Game(turn=9, alive=3).run_combat` → 断言 cap=None；构造 override=10 → 断言 10 生效。

## 5. P0：死亡波次内的顺序与批处理（BB:7128-7757 / GM:2438-2590）

| 细节 | BobsBuddy | hsrl2 |
|------|-----------|-------|
| 一波死亡的收集 | 一次收集该侧全部死者（`orderby CreatedDuringCombat ? 1 : 0`，稳定） | 一次收集双方全部死者（固定 `self.heroes` 顺序） |
| 先处理哪一侧 | **随机**（`SafeRandom.NextBool()`，7626） | 固定 |
| 离场 vs 亡语 | 先给所有死者注册亡语触发并**移除**，再统一跑亡语 | 每个死者：离场→亡语→复生→Avenge，**逐个完整处理** |
| 复生时机 | 整波亡语跑完后统一 `ResolveReborn` | 每个死者自己的亡语跑完**立刻**复生 |
| 战斗内召唤的随从 | 排序靠后死亡（`CreatedDuringCombat`） | 无此排序 |
| Avenge 源排除 | 排除**整批**死者 | 仅排除当前死者 |

**影响**：
- 多个随从同时死亡时，第 1 个死者的亡语看到的是"其余死者**已离场**"（BobsBuddy）还是"仍在场"
  （hsrl2 在轮到它自己之前不处理其他死者，故仍在场——影响满场召唤/相邻判定）；
- 复生体出现时机早于同波其它亡语（hsrl2）与晚于全部亡语（BobsBuddy）——影响"Avenge 计数后复生体是否被
  亡语 buff"等组合；
- 死亡侧顺序在 BobsBuddy 里是随机的（作者对"同时死亡"顺序不确定的建模）——**这条不建议照抄**，
  引擎需要确定性（AGENTS.md §3）。建议保留确定性但**记录为已知不确定点**。

**最小测试**：
1. 两个同时死亡且都有亡语的随从（左侧 A 亡语召唤，右侧 B 亡语 buff 友方）：断言召唤/复生的相对顺序。
2. Reborn + 相邻亡语给友方 +1/+1：断言复生体是否吃到该 buff（BobsBuddy：吃到，因为复生在亡语后；
   hsrl2：若复生体在亡语前已回场则也吃到——需构造先后不同的用例区分）。
3. `CreatedDuringCombat`：战斗内召唤的随从与原始随从同波死亡 → 断言死亡（及亡语）顺序。

## 6. P0：开局战斗效果（SoC）顺序（BB:8265-8524 / CB:129-156）

**BobsBuddy**（完整顺序）：
```
OnCombatStartSetup（player 全部 → opponent 全部）
→ Anomaly
→ QuestRewards（flag 侧 → 另一侧；PromoPortrait/Valdrakken 附加触发）
→ Trinkets（OrderStartOfCombatTrinkets：BG35_MagicItem_732 优先；flag 侧 → 另一侧）
→ HeroPowers（GetOrderedHeroPowers）
→ Objectives
→ Hand（ISummonFromHand 且 IOnStartOfCombat）
→ Secret checks
→ 随从：两侧交替，先 flag 侧最左
flag = playerSide.Count > opponentSide.Count || (<= 且 SafeRandom.NextBool())
```

**hsrl2**（`_start_of_combat_phase`）：`for hero in (a, b)`：饰品 → 任务奖励 → 棋盘随从（左→右）
→ 手牌 hand_soc → 英雄技能事件。

**分歧点**：
1. 类别顺序：BobsBuddy 任务奖励在饰品**之前**；hsrl2 饰品在前（CB 注释 C12 明确写"饰品→任务奖励"）。
2. 两侧交织：BobsBuddy 随从 SoC 是**两侧交替**；hsrl2 是先 `a` 侧全部再 `b` 侧。
3. 先手侧：BobsBuddy 按棋盘大小决定（平则随机）；hsrl2 固定 `a` 先。
4. BobsBuddy 多了 Anomaly/Objectives/手牌在随从之前。

**影响**：SoC 顺序决定 buff 顺序、死亡顺序、先攻判定前的棋盘规模（先攻在 SoC 后判定，两边一致）。

**最小测试**：a 侧 2 随从（含 SoC 召唤）、b 侧 3 随从（含 SoC 死亡）→ 断言 SoC 结束后先攻方；
构造两侧各一个 SoC 随从来断言交替顺序。

## 7. P1：其余分歧 / 缺实现

### 7.1 Mega-Windfury（BB:6408-6414）
`num2 = megaWindfury ? 4 : windfury ? 2 : 1`。hsrl2 无 mega 分支 → 添加：
`if attacker.has(GameTag.MEGA_WINDFURY): 连续 4 次`（每次重新选目标）。测试：Mega-Windfury 1/1
攻击 3/3 三次后第 4 次是否继续（BobsBuddy 4 次）。

### 7.2 伤害组同时性（BB:7470-7591）
BobsBuddy 先把组内所有 `baseHealth -= amount` 执行完，再把收集到的 OnTakeDamage/OnKilled 触发统一 resolve；
hsrl2 是"扣血→触发→扣血→触发"。可观测差异：两个目标同时被顺劈时，"受到伤害"监听器的触发顺序/中间状态。
测试：两个"每当受到伤害"监听器分别记录顺序 → 断言两者都在第二次扣血前/后触发。

### 7.3 攻击前死亡结算（BB:6550-6581）
BobsBuddy 在 OnAttack/OnIsAttacked/Rally 触发 resolve 后**先结算死亡**；若目标已死则本次不造成伤害
（但仍继续 AFTER_ATTACK 流程）。hsrl2 在 Rally 后不结死：目标若已被 Rally 打死，
`defender.take_damage` 因 `dead` 直接返回 0（MN:166）且反击力被置 0（CB:272），**净效果相同**。
差异仅在"死亡/亡语 resolve 的时点"（BobsBuddy 在伤害前跑完亡语；hsrl2 在攻击结束后 `check_deaths` 统一跑）。
测试：Rally 杀死目标 → 断言目标亡语在本随从攻击动作**之前**还是之后触发（BobsBuddy 之后一次攻击前，
hsrl2 在 `_execute_attack` 结束的 `check_deaths`）。

### 7.4 Avenge 同波死者排除（BB:7714-7720）
BobsBuddy：`!list2.Contains(entity)` 排除整批死者；且先算英雄技能 `IAvenge` 再算其他实体。
hsrl2：`if other is m: continue` 只排除当前死者。测试：两个 Avenge 随从同波死亡 → 断言互不计数。

### 7.5 潜行（BB:7108-7126）
BobsBuddy 每轮 `CheckForStealth`：0 攻潜行随从失去潜行；若**全部**随从都有潜行则全部解除。
hsrl2 只做"潜行不可被选为目标"。测试：0 攻潜行随从是否在下一轮变为可被攻击。

### 7.6 平局伤害（BB:7002-7031 / GM:3499-3504）
BobsBuddy 对"双方都有随从但僵局"仍计算 `Σtier 差 + 英雄 tier` 的有符号值（用于输出统计）；
hsrl2 平局返回 0。对局规则上僵局=0 伤害，hsrl2 更贴近规则；**不建议改**，仅在解释 BobsBuddy
胜率统计时注意。

### 7.7 一致项（无需改动，作为交叉验证）
- 目标选择：嘲讽优先 + 均匀随机（BB:1051 / CB:251）。
- 先攻：随从多者先、平随机（BB:8464 / CB:160）。
- 风怒第 2 击重新选目标（BB:6415 / CB:232）。
- 攻击轮转语义（左→右、每方交替、风怒同轮连击、一方空即结束）。
- 召唤上限 7（BB:8857）。
- 随机分布均匀（BB:1411-1452）——但 BobsBuddy 是 Monte Carlo 10000 次，hsrl2 是单次确定性采样，
  用途不同（RL 需要确定性，正确）。

## 8. 待补证据 / 未验证

1. `Minion.CloneAsVanilla()` 的确切实现（是否丢弃 golden、maxAttack 等）尚未展开——当前结论基于
   `OnReborn` 的调用与 `RebornBehavior` 语义。补法：ilspycmd `-t BobsBuddy.Minion`（已在
   `rea_script_test/rea_members_BobsBuddy.Minion.json` 有 153 个方法清单，方法体需反编译文件对应行段）。
2. `ChooseAttackTarget` 的卡牌覆写（BB:18702/18758/30297）——特殊卡（如嘲讽相关）的选目标覆写；
   对普通随从无影响。
3. 剧毒/猛毒与"免疫"/"防止伤害"的交互（`ITryPreventDamage`）——hsrl2 有 IMMUNE_WHILE_ATTACKING，
   但无通用 prevent；如需覆盖特殊卡再补。
4. 官方行为仍需 Power.log 实测（本机当前无 `Hearthstone\Logs` 目录，见 `docs/REA_FINDINGS.md` §6）。

## 9. 复现路径

| 证据 | 生成方式 |
|------|----------|
| `bobsbuddy/src_full/BobsBuddy.decompiled.cs` | `ilspycmd -r <HDT app dir> -o src_full BobsBuddy.dll` |
| `bobsbuddy/rea_Simulator_members.json` | `tools/rea_analyze_dotnet.ps1 -DllPath … -Types BobsBuddy.Simulation.Simulator` |
| `hdt_bobsbuddy/BobsBuddyInvoker.cs` | `ilspycmd -t Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyInvoker HearthstoneDeckTracker.exe` |
