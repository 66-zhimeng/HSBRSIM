# BobsBuddy 战斗模拟器 — 逆向证据与算法要点

> 本目录由 rea-operator 生成（2026-10-18）。所有结论都附**可复现证据**（文件 + 行号 / REA 证据 JSON）。
> 目标程序集（只读）：`%USERPROFILE%\AppData\Local\HearthstoneDeckTracker\app-1.53.0\BobsBuddy.dll`
> sha256（REA 实测）= `26dd65ca3db6eb54…`（完整值见 `rea_summary.json` / `rea_Simulator_members.json`），
> `BobsBuddy, Version=1.34.11.0`，1817 类型 / 5991 方法（`../net/BobsBuddy/refs.json`）。
> 插件宿主：`…\app-1.53.0\HearthstoneDeckTracker.exe`（`Hearthstone_Deck_Tracker.BobsBuddy.*`）。

## 0. 本目录文件

| 文件 | 内容 |
|------|------|
| `src_full/BobsBuddy.decompiled.cs` | ilspycmd 9.1 全量反编译（29,200 行；**主要证据来源**） |
| `rea_artifact.json` | REA `inspect-managed-artifact` 原始证据（身份/metadata/引用） |
| `rea_Simulator_members.json` | REA `inspect-managed-members --type BobsBuddy.Simulation.Simulator` 原始证据（99 方法 / 787 member_refs / 2617 call_edges / 660 field_accesses / 57 ldstr 字符串） |
| `rea_simulator_methods.json` | 上者的压缩摘要（方法名+token+IL sha256），供脚本/审阅消费 |
| `rea_script_test/` | `tools/rea_analyze_dotnet.ps1` 实跑输出（含 summary） |
| `types_raw.txt` | ilspycmd 类型清单（1745 行） |
| `../hdt_bobsbuddy/BobsBuddyInvoker.cs` | HDT 宿主插件（输入构造、DamageCap、迭代数）——**伤害上限的证据** |
| `../hdt_bobsbuddy/BobsBuddyUtils.cs` | 同上（工具类） |

复现：
```powershell
# REA（便携 Node 24）
pwsh tools\rea_analyze_dotnet.ps1 `
  -DllPath "$env:LOCALAPPDATA\HearthstoneDeckTracker\app-1.53.0\BobsBuddy.dll" `
  -Types BobsBuddy.Simulation.Simulator,BobsBuddy.Simulation.Minion
# 反编译（REA 的托管 provider 不做 C# 反编译，用 ilspycmd 补方法体）
& "$env:USERPROFILE\.dotnet\tools\ilspycmd.EXE" --disable-updatecheck `
  -r "$env:LOCALAPPDATA\HearthstoneDeckTracker\app-1.53.0" `
  -o reversed\rea\bobsbuddy\src_full `
  "$env:LOCALAPPDATA\HearthstoneDeckTracker\app-1.53.0\BobsBuddy.dll"
```

## 1. 顶层结构（证据：`src_full/BobsBuddy.decompiled.cs`）

| 类型 | 行 | 作用 |
|------|----|------|
| `BobsBuddy.Minion` | 366 | 随从实体（atk/health 由 base+passive 计算；目标选择；复生） |
| `BobsBuddy.Simulation.Simulator` | 6280 | **战斗引擎主体**（3000 行） |
| `GameState` / `PlayerState` | 9292 | 每场模拟的状态（Side/Hand/Trinkets/Quests/HeroPowers） |
| `Input` | 9745 | 输入：`Player/Opponent`、`DamageCap`、`turn`、`isDuos`、`Anomaly`、`availableRaces` |
| `Output` | 10284 | 输出：`damageResults`（有符号英雄伤害）、win/tie/loss、lethal 率 |
| `SimulationRunner` | 10733 | 多线程 Monte Carlo 包装（`Parallel.For` + 每线程独立 `Simulator`） |
| `SafeRandom` | 1385 | 全局随机源（**均匀分布**，`ForceNextBaseSeed` 可定种） |
| `CloningGallery` | 10766 | 每场战斗前输入棋盘的深拷贝模板（**每场重新克隆 → 战斗内状态天然每场清零**） |

## 2. 逐场模拟的外层循环（Monte Carlo）

- `SimulationRunner.SimulateMultiThreaded`（10735）：`threadCount = Environment.ProcessorCount/2`，
  总迭代 **10000**（`HearthstoneDeckTracker` 侧常量 `Iterations = 10000`，`BobsBuddyInvoker.cs:46`，
  调用点 `:1171`），单线程超时 `maxDuration`=1500ms（跳蛙组合 5000ms / 6+ 随从 3000ms，`:1162-1170`）。
- `Simulator.SimulateForInput`（6869）：循环调用 `SimulateFight(input)` 收集 `damageResults`，
  超出 `maxDuration` 提前退出（`ExitConditions.Time`）。
- **随机分支采样 = 均匀分布**：`SafeRandom.Next(min,max)` / `TryGetRandom` 均为 `Random.Next` 均匀抽取
  （`1385-1452`）；默认种子 `Environment.TickCount + Interlocked.Increment`（每线程不同），
  `ForceNextBaseSeed(seed)` 可复现（单测路径）。**没有加权/重要性采样**：概率由 10000 次 Monte Carlo 频率估计。
- 每个 `Simulator` 实例每场 `SetupFight` 都从 `CloningGallery`（输入棋盘的克隆）重新构建，
  **战斗内 buff/死亡/复生不会跨模拟泄漏**。

## 3. 攻击轮转（`OneRound` 7043 / `Fight` 6891）

```
Fight(): for i in 0..500:  若 eitherSideWon(任一方 board 空) 或 noValidAttackers → break; OneRound()
OneRound():
  CheckForStealth()                                    # 0 攻潜行失去潜行；全员潜行则全部失去
  若 activeSide 无有效攻击者 → 换边；双方都无 → return（僵局）
  minion = activeSide 中第一个 !hasAttacked 且 IsValidAttacker 的（左→右）
  跳过无效者时置 hasAttacked=true；若全部已攻击 → 全体清 hasAttacked 后取最左有效者
  AttackWithMinion(minion); minion.hasAttacked = true
  activeSide = 另一侧
```

- `IsValidAttacker`（`Minion` 755）：`IsAlive() && attack() > 0 && !cannotAttack`。
- **每方轮流一次攻击**；每方按棋盘左→右推进，一轮走完重置（不是"每场一次"）。
- 一方棋盘空 → 战斗结束；双方都无法攻击 → 僵局结束（最多 500 次 `OneRound`）。

### 3.1 单体攻击 + 风怒（`AttackWithMinion` 6402）

```
num2 = megaWindfury ? 4 : windfury ? 2 : 1        # 每回合内连续攻击次数
for num in 0..num2:
    if num > 0: target = null                      # 风怒第 2 击**重新选目标**
    PerformAttack(attacker, target)
    ResolveTriggersInCurrentScope(); ResolveReborn()
    ResolveAllDeathsAndTriggers(resolveReborn: true)
    OnAfterAttackStep  → 先己方全部实体，再敌方全部实体
```

- 风怒 = 同一攻击轮内连续 2 次；**Mega-Windfury = 4 次**（`MinionHasMegaWindfury` 6608，含 `DAL_742` 光环）。
- 第 2 次攻击重新随机选目标（`BobsBuddyInvoker` 不传 `preserveTarget`）。

### 3.2 单次攻击的精确结算顺序（`PerformAttack` 6457）

```
1  目标选择 ChooseAttackTarget()
2  攻击者失去潜行 → 触发 IOnFriendlyMinionLostStealth（立即 resolve）
3  Rally 次数 num3 = 1 + (BG34_Giant_376 光环) + (BG33_Reward_021 计数)
4  OnAttackSecretCheck(target)                          # 攻击时奥秘
5  IOnFriendlyMinionIsAttacking 触发注册 → resolve → resolve 法术后触发
6  IOnAttack 触发注册（攻击者自身）
7  IOnRally 触发注册（num3 次）；AdditionalRallies
8  target.OnIsAttacked(attacker) + IOnFriendlyMinionIsAttacked
9  resolve 上述触发 → ResolveAllDeathsAndTriggers(true)  # ★ 伤害前的死亡结算
10 if target.IsAlive():
     伤害组 = [attacker.attack() → target]
             + cleave ? [left_neighbor, right_neighbor]   # ★ 顺劈在**同一伤害组**
             + (非 ImmuneWhileAttacking ? [target.attack() → attacker] : 消耗免疫)
     ProcessDamage(伤害组)                              # ★ 整组同时结算
11 IOnAfterAttack / AttachedModularEntity / target.OnWasAttacked / IOnFriendlyMinionAfterAttack 注册
   （不在本函数内 resolve，留给 AttackWithMinion 的 ResolveTriggersInCurrentScope）
```

要点：
- **伤害是"组"语义**：主目标、顺劈左右邻居、反击全部进入同一个 `ProcessDamage(IEnumerable<Damage>)`，
  先统一扣血（`baseHealth -= amount`），再统一结算伤害触发。攻击者即便会死于反击，
  顺劈伤害照常生效（因为组在扣血前构造，且全部条目都会走完）。
- 若第 9 步的触发把目标弄死，第 10 步**完全不造成伤害**（直接跳过）。

## 4. 伤害结算（`ProcessDamage` 7470）

对伤害组内**每个条目按顺序**执行（此时全场都在扣血阶段，事件延后）：

```
跳过 amount == 0
if target.hasDiv: div--; 触发 IOnFriendlyMinionLostDiv(立即); continue   # 圣盾完全吸收
if TryPreventDamage(...): 记录触发; continue
flag = target.IsAlive()                    # 扣血前是否活着
overkill = max(0, amount - target.health())
target.baseHealth -= amount
记录 OnTakeDamage / IOnFriendlyMinionTakeDamage / IOnFriendlyMinionDealsDamage 触发
if source is Minion && source.venomous:
    target.baseHealth = -10000; source.venomous = false; 记录 lost-venomous 触发
elif source is Minion && source.poisonous:
    target.baseHealth = -10000
OnDamageSecretCheck
记录 first/second time taken damage 触发；TimesTakenDamage++
if flag && target.IsDead(): OnKilled + (overkill>0 && 源方是 activeSide ? OnOverkill) + IOnFriendlyMinionKilledEnemy
ActualDamageDealt = amount
（英雄技能伤害额外触发 OnAfterHeroPowerDamage）
组处理完毕 → 统一 resolve 全部记录触发（scope "DamageTriggers"）
```

**关键语义**（与常见实现差异最大处）：
1. 剧毒/猛毒在**扣血阶段**直接 `baseHealth = -10000`，**不要求攻击者存活**，也不看实际扣血量
   （只看该条目 `amount != 0` 且未被圣盾/预防）。
2. 猛毒是"每场战斗一次"：用完置 `venomous=false`（并触发 lost-venomous）；剧毒永久。
3. 伤害组内所有条目**先全部扣血**，之后才统一解决"受到伤害/击杀"触发 → 与"逐条扣血+逐条触发"有可观测差异。

## 5. 死亡、亡语、复生、Avenge（7623 / 7759 / 7921 / 7971）

### 5.1 波次循环（`ResolveAllDeathsAndTriggers` 7128）

```
do:
   changed  = ResolveDeaths()                 # 双方死亡收集/移除/亡语注册/Avenge
   changed |= ResolveTriggersInCurrentScope() # 统一 resolve 亡语等
   changed |= ResolveReborn()                 # 复生（延迟到最后）
while changed
（若当前 scope 的父 scope 有未解决亡语，则本层跳过复生 —— "亡语先于复生"）
```

### 5.2 死亡收集顺序（`ResolveDeaths` 7623 / `ResolveDeathsForSide` 7635）

- **双方死亡结算先后随机**：`SafeRandom.NextBool()` 决定 player 侧先还是 opponent 侧先（7626-7632）。
- 同侧内排序：`orderby CreatedDuringCombat ? 1 : 0`（**战斗内召唤出来的随从排到最后死亡**），
  同级保持棋盘顺序（稳定排序）。
- 对每个死者（尚未移除出 board）：
  1. 记录 `LastKnownAttack/Health`；`baseHealth=-10000`；`div--`
  2. 若 `IDeathrattleAttacksImmediately` → **先**触发亡语
  3. 注册 `IOnFriendlyMinionDied`（其他存活实体）
  4. `TriggerDeathrattles(minion)` —— 注意此函数主要**注册**触发（含 Titus/亡语翻倍次数，
     见 7759-7910：`num3 = 1 + 光环/任务奖励/EchoesOfArgus`），不立即执行
  5. **Avenge**：先英雄技能 `IAvenge`，再同侧其他 `IAvenge` 实体（`HandleAvenge`）
  6. 若 `reborn` → `state.Reborn.Add(RebornTrigger(...))`（**延迟**）
  7. 死亡奥秘、`IOnAfterFriendlyMinionDied`、手牌触发
- 整批处理完后才把死者 `side.Remove()` 并加入 `DeadMinions`（**先全部注册触发，再统一移除**）。

### 5.3 亡语执行

- `ResolveReborn` 在 `ResolveTriggersInCurrentScope` **之后**执行（7128-7143），即：
  **整波死亡 → 全部亡语 → 复生**。`SourceHasPendingDeathrattle` 保证"该亡语未跑完则先不复生"。

### 5.4 复生（`Minion.OnReborn` 934-983）

```
默认: CloneAsVanilla()            # ★ 原版克隆：丢弃所有 enchantment/buff
      baseHealth = 1
      reborn = false；保留 ScriptDataNum1..4
      if IRebornBehavior 声明 KeepEnchantments → Clone()（保留 buff）
      if KeepEnchantments||KeepBonusKeywords → 恢复 taunt/windfury，且曾有过圣盾(_divWasEverTrue)则 div=1
      if KeepMaxHealth → baseHealth = maxHealth + 被动生命加成
      if KeepMaxAttack → baseAttack = maxAttack + 被动攻击加成
      OnMinionRebornResetCounters
      TrySummonMinion(minion, reborn)
```

### 5.5 Avenge（`HandleAvenge` 7971）

```
if AvengeRequirement > 0:
    AvengeCounter++
    if AvengeCounter % AvengeRequirement == 0 → 注册 OnAvenge 触发
```
- 每个**死亡随从**都会给每个 `IAvenge` 实体计数；计数在 `Minion.Clone`/`TryMakeGolden` 时归零
  （1313），而每场模拟都重新克隆输入棋盘 → **每场战斗自然清零**。
- 死亡波中**同批死亡的其他 Avenge 实体不参与本次计数**（7635 的 `if (!list2.Contains(entity) ...)`
  把整批死者排除在 Avenge 源之外）。

## 6. 召唤上限与插入位置（`TrySummonMinion` 8854）

- **硬上限 7**：`if (side.Count >= 7)` → 触发 `IOnFriendlyMinionFailedToSummonNoSpace` 并返回空
  （8857-8867）。注意此时 `side` 里可能仍有本波"已死未移除"的随从。
- 插入位置由调用方给出（`Minion.TrySummonMinion`：`IsDead() ? LastKnownPosition : BoardPosition()+1`），
  再经 `UpdateSummonCounter` 修正（把 scope 内的召唤计数考虑进去，8996）。
- 召唤后依次 resolve：`OnMinionSummoned` → `IOnFriendlyMinionSummoned` → `IOnSummoned` →
  `OnMinionSummonedByFriendly` → `IOnAfterFriendlyMinionSummoned` → `IOnFriendlyMinionSummonedMultipliesAttack`。
- 战斗内召唤的随从置 `CreatedDuringCombat = true`（→ 死亡排序靠后，见 5.2）。

## 7. 开局效果（Start of Combat）顺序（`ResolveStartOfCombatEffects` 8265）

```
1  OnCombatStartSetup()：先 player 侧全部实体，再 opponent 侧全部实体
2  决定 SoC 先手 flag = playerSide.Count > opponentSide.Count
                        || (opponentSide.Count <= playerSide.Count && SafeRandom.NextBool())
3  Anomaly 的 SoC
4  QuestRewards：先 flag 侧，再另一侧（每张奖励后 ResolveTriggers + 死亡；PromoPortrait 首触发翻倍、
   ValdrakkenWindChimes 全部翻倍）
5  Trinkets：`OrderStartOfCombatTrinkets`（BG35_MagicItem_732 优先），同样先 flag 侧再另一侧
6  HeroPowers：`GetOrderedHeroPowers(flag, …)`（含 Teron 特判/翻倍饰品）
7  Objectives
8  Hand（仅 `ISummonFromHand && IOnStartOfCombat` 的手牌）
9  OnStartOfCombatSecretCheck（flag 侧再另一侧）
10 随从 SoC：**两侧交替**，每次取一侧最左未处理者（先 flag 侧），最多 500 步
```

### 7.1 先攻判定（8464-8491）

```
if state.ActiveSide == null:
    playerSide.Count > opponentSide.Count → player 先
    opponent > player → opponent 先
    相等 → SafeRandom.NextBool()
```
（先攻在 **SoC 效果结算之后**基于存活棋盘大小决定；SoC 可能改变随从数量。）

## 8. 伤害上限（DamageCap）

- `Simulator.DamageCap` = `Input.DamageCap`（6680），只在**结算战败伤害**时钳制（7020-7030）。
- **`Input.DamageCap` 不是本地公式**：HDT 宿主从**服务端下发的 GameEntity tag** 读取
  （`../hdt_bobsbuddy/BobsBuddyInvoker.cs:652-655`）：
  ```csharp
  if (_game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP_ENABLED) > 0)
      input.DamageCap = _game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP);
  ```
- tag id（客户端 GAME_TAG 枚举，证据 `../net/key_enums.json`）：
  `BACON_COMBAT_DAMAGE_CAP = 2089`，`BACON_COMBAT_DAMAGE_CAP_ENABLED = 3403`。
- 伤害值公式（`SimulateFight` 6982-7041）：
  ```
  num = Σ(胜方存活随从 tier) - Σ(败方存活随从 tier)
  if num > 0: num += Player.Tier     elif num < 0: num -= Opponent.Tier
  if DamageCap > 0: num = clamp(num, -DamageCap, +DamageCap)
  num += (对手掉血量 num3) / num -= (自己掉血量 num2)     # 英雄技能等战前对英雄的伤害
  ```

## 9. 战斗内状态清理（每场战斗重置）

- 每场 `SimulateFight → SetupFight`（6802）都重建 `GameState`，并从 `CloningGallery` 重新克隆输入；
  `ResetPlayerStateAndUnbuffCloningGallery`（6749）对克隆模板做：
  `RemovePassiveEffectsSide`（去掉被动光环加成的 baseAttack/baseHealth，7227-7255）、
  `RemovePassiveTavernSpellBuffsSide`（去掉酒馆法术加成，7257）、
  `maxAttack = baseAttack; maxHealth = baseHealth`（6760-6764）。
- 因此**战斗内获得的光环/buff 不会泄漏到下一场模拟**；输入棋盘的持久数值来自 HDT 读取的实体状态。
- `AvengeCounter`、`venomous`、`TimesTakenDamage`、`hasAttacked`、`stealth` 等都在克隆/新状态里重置
  （注意：`hasAttacked` 每场战斗开始为 false，`TrySummonMinion` 会按"攻击者已攻击"传播，8917-8920）。

## 10. 输出语义（`Output` 10284）

- `damageResults: List<int>`：每场一条**有符号**英雄伤害（>0 表示我方赢并打对方；<0 表示我方输）。
- `winRate = count(>0)/N`, `tieRate = count(0)/N`, `lossRate = count(<0)/N`；
  `myDeathRate = count(-dmg >= myHealth)/N`, `theirDeathRate = count(dmg >= theirHealth)/N`（lethal 率）。
- `avDamage` 平均（有符号），`medianDamage` 取排序中位。
- `IsDifferntFrom(..., percentMarginOfError=0.05, averageDamageMarginOfError=0.1)` 用于判定两次模拟是否显著不同。

## 11. 覆盖边界（BobsBuddy 自己承认的近似）

- `UnsupportedInteractionException`：遇到未实现交互时**整场模拟放弃**（HDT 显示错误态），
  不是静默近似（`BobsBuddyInvoker.cs:1180-1190`）。
- 只模拟"战斗"本身：招募期、商店、英雄技能选择等由 HDT 读取的真实状态作为输入。
- 结论：BobsBuddy 是**高保真参照实现**，但不是官方实现；分歧处应以 Power.log 实测为准。
