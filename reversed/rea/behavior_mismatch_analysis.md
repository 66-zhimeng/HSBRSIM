# behavior_mismatch 根因定位 — BobsBuddy 反编译证据 + 引擎/差分 harness 对照

> 只读分析（**未修改 `hsrl2/`、未修改 `tools/`**）。2026-10-18，rea-operator。
> 输入：`runs/official_traces/{6 traces}.json`、`runs/diff_report.{md,json}`（16:44 版）、
> `tools/diff_replay_combat.py`、`hsrl2/`（战斗核心 + 英雄技能脚本）、
> `reversed/rea/bobsbuddy/src_full/BobsBuddy.decompiled.cs`（下称 `BB:`）、
> `reversed/rea/hdt_bobsbuddy/BobsBuddyInvoker.cs`（下称 `INV:`）。
> 实验脚本（可复跑，只读注入）：`reversed/rea/scripts/behavior_mismatch_repro.py`、
> `reversed/rea/scripts/ozumat_fix_check.py`。

---

## 0. TL;DR（重要：报告已过期 + 发现 1 个真引擎 bug）

1. **Lead 引用的「6 条 behavior_mismatch」来自 16:35 的旧 `runs/diff_report.md`；
   当前 16:44 版报告里 `behavior_mismatch` 已经是 0**（6 条已被 diff-harness 重新归因为
   `unmodeled_context: 2` + `unmodeled_dynamic_state: 8`，`context_gap=10`）。
   调查结论与这次重归因**大体一致**，但有 1 条例外（见第 2 条）。
2. **D4（River Skipper vs Risen Rider 3/2，Ozumat 技能）不是上下文缺口，而是真实的引擎脚本 bug**：
   `OzumatScript` 用 `while not hero.board_full()` 循环召唤 Tentacle → 玩家 1 只随从时
   会召唤 **6 只** 2/2 Taunt Tentacle；官方/BobsBuddy 是**每场战斗只召唤 1 只**。
   - 官方日志结果 `loss`；BobsBuddy 10000 次模拟 `loss`；
   - 引擎现状 `win`；**用 monkey-patch 改成"只召唤 1 只"后引擎立刻变 `loss`（8/8）**（实验已做）；
   - 当前 harness 把它吞成 `hero_power_state` 上下文缺口（因为 `hero_power_data=2≠0`），
     属于**误判**，见 §5 的启发式修正建议。
3. **其余 5 条的根因都是"日志里存在、但引擎/harness 未建模的外部上下文"**，不是战斗核心 bug：
   | 轨迹 | 根因 | 证据 |
   |------|------|------|
   | D1/D2 | 玩家有 **Pack Tactics 秘密**（Akazamzarak 技能 `Prestidigitation`） | trace `detected=[{kind:S,value:PackTactics}]`；`BB:9148-9171` |
   | D3 | Risen Rider 被额外赋予 **"Deathrattle: Summon a 3/2 Crab"**（Surf n' Surf 的 Spellcraft `BG27_004t`） | trace `tags.Deathrattles=[Crab]`；`BB:297-305, 5544-5567` |
   | D5 | **Teron Gorefiend 技能的目标状态**（`hero_power_data=376` = 被选随从的 game_id；引擎 `_teron_uuid` 无输入） | `BB:32451-32519`；trace `hero_power_state:...:376` |
   | D6 | 动态属性（Eternal Knight 的 `ScriptDataNum` 击杀计数）+ Teron 状态 | trace `tags.ScriptDataNum1/2`、`dynamic_stat_script` |
4. **Lead 点名的 4 类战斗核心机制（圣盾吸收/风怒第二击/复生时机/同时伤害批处理）经对照后
   引擎与 BobsBuddy 一致，不是这 6 条的原因**（§3 有方法+行号+伪代码+实测）。
   D1 裸战团（无秘密）的「tiè」是**官方规则下的正确结果**：双方圣盾互破后，风怒第二击打死
   Annoy-o-Tron、反伤 1 点打死 Crackling Cyclone → 双亡 = 平局。官方实战是 `win` 只因秘密。

---

## 1. 方法与复现

```powershell
cd C:\HSagent
$env:PYTHONIOENCODING="utf-8"
# 6 条轨迹原样复现 + 补上下文实验（复用 harness 的 trace loader/卡名解析）
C:\Python313\python.exe reversed\rea\scripts\behavior_mismatch_repro.py
# D4 的 Ozumat 修复方向验证（monkey-patch，不改文件）
C:\Python313\python.exe reversed\rea\scripts\ozumat_fix_check.py
```
实验设计：用 `tools/diff_replay_combat.py` 的 `CardResolver`/`HeroIndex`/`load_trace`
与引擎 `Game/Hero/wire_hero_power_on_bind/run_combat` 原样复现（16 seeds），
再用**进程内监听器**补上"日志里有、引擎没建模"的上下文（秘密/外部亡语/技能目标），
观察官方结果是否变为可达。全程不写任何引擎文件。

实测输出（节选）：

| 场景 | 引擎结果 | 官方 | BobsBuddy |
|------|----------|------|-----------|
| D1 原样（Prestidigitation + PackTactics 未建模） | tie 16/16 | win | win 1.0 |
| D1 + PackTactics 3/3 复制 | **win 16/16** | win | win 1.0 |
| D2 原样（对面 Wingmen 已建模） | loss 16/16（关技能 tie） | win | win 1.0 |
| D2 + PackTactics 3/3 复制 | **win 16/16** | win | win 1.0 |
| D3 原样（Risen Rider 带外部 Crab 亡语未建模） | tie 16/16 | loss | loss 1.0 |
| D3 + Crab(3/2) 亡语 | **loss 16/16** | loss | loss 1.0 |
| D4 原样（Ozumat） | **win 16/16**；SoC 后玩家棋盘 = `[Skipper, Tentacle×6]` | loss | loss 1.0 |
| D4 改成"开局 1 只 2/2 Taunt Tentacle" | **loss 16/16** | loss | loss 1.0 |
| D5 原样（Teron 目标未接线） | tie 16/16 | win | win 1.0 |
| D5 + Teron 目标接线（`_teron_uuid`） | **win 16/16** | win | win 1.0 |
| D6 原样 | win 6/16, tie 10/16 | tie | tie .739 / loss .261 |

---

## 2. 六条轨迹逐条根因（证据 + 判定）

### D1 `4c3c73caa189365e` — Pack Tactics 秘密（未建模，非引擎 bug）
- trace：玩家 Annoy-o-Tron 1/2 Taunt+DS，`hero_power_id=TB_BaconShop_HP_020`
  （CardDefs 权威文本：**Prestidigitation — "Choose a Secret. Put it into the battlefield."**），
  `detected=[{kind:"S", value:"PackTactics"}]`。
- BobsBuddy：`Simulator.CheckPackTactics`（`BB:9148-9171`）在**友方随从被攻击时**（`PerformAttack`
  的 `OnAttackSecretCheck(target)`，`BB:6508`）判断：友方棋盘有空位 → 以
  `target.CloneAsShallowExactCopy()` 创建复制、`SetStats=(3,3)`、在目标右侧召唤（`BB:9154-9158`）。
- 引擎：**无 BG Secret 子系统**（`hsrl2/scripts/batches/batch_hero_remaining.py:110` 明确
  `TB_BaconShop_HP_020 ... Dependency: Secret 子系统`；`REGISTRY` 无该卡脚本）。
  → 引擎只会打裸战团，结果是正确规则下的 **tie**（双方圣盾互破 + 风怒双亡，见 §3.1/§3.4）。
- 判定：**unmodeled_context（秘密）**，harness 当前归因正确。

### D2 `aa4c75414541d87b` — 同上（Pack Tactics）+ 对面 Wingmen（已建模）
- 同 D1；对面技能 `TB_BaconShop_HP_069` = **Wingmen**
  （"SoC: Your left- and right-most minions gain +2/+1 and attack immediately"），
  CardDefs 权威文本与 `BB:32084-32132` 一致，引擎 `IllidanWingmenScript`
  （`hsrl2/scripts/batches/batch_hero_combat3.py:212`）**已建模**。
- 因此"开技能 → loss、关技能 → tie"的差异来自 Wingmen（引擎行为合理），
  而官方 `win` 仍由 Pack Tactics 解释（补上后 **win 16/16**）。
- 判定：**unmodeled_context（秘密）**。

### D3 `b2683903c4464b6f` — 外部亡语 Crab（未建模，非引擎 bug）
- trace：opponent Risen Rider 2/1 Taunt+Reborn，`tags={"Deathrattles": "[GenericDeathrattles.Crab]"}`。
  BobsBuddy 的 `AdditionalDeathrattles` 来自 HDT 读取的 `Deathrattles` tag；
  `GenericDeathrattles.Crab`（`BB:297-305`）= 死亡时召唤 `BG27_004t2`（Crab 3/2）；
  来源是 **Surf n' Surf 的 Spellcraft `BG27_004t`（Crab Mount）**：
  "Give a minion 'Deathrattle: Summon a 3/2 Crab' until next turn"（`BB:5544-5567`）。
- 引擎：`BG27_004` **不在 REGISTRY**（缺脚本，属已知 `missing_script` 类），且 trace 的
  `deathrattle_annotation` 是**外部授予**的亡语；引擎只按卡自身脚本复现亡语。
  → 引擎裸战团 tie；补 Crab 亡语后 **loss 16/16**（= 官方）。
- 判定：**unmodeled_dynamic_state（deathrattle_annotation）**，归因正确；
  该 gap 比同 trace 的两个 `hero_power_state` 更关键（后者是招募期技能，见 §5）。

### D4 `f5d1c647b2f449d1` — **真实引擎 bug：Ozumat 一次战斗召唤 6 只 Tentacle**
- trace：玩家（Ozumat）River Skipper 1/1，`hero_power_id=BG23_HERO_201p`，
  `hero_power_data=2`；对手 Risen Rider 3/2 Taunt+Reborn。
- 官方文本（CardDefs 权威 255130）：**Tentacular — "When you have space in combat,
  summon a @/@ Tentacle with Taunt. (Gains +1/+1 after you sell a minion!)"**（单数 "a Tentacle"）。
- BobsBuddy：`OzumatHeroPower`（`BB:32185-32219`）——`OnStartOfCombat`/`OnAfterAttackStep`
  调 `CheckForTentacular()`：**`if (FriendlySide.Count < 7 && !_tokenSummoned)`** 才召唤
  `BG23_HERO_201pt`，`SetStats=(Data.Data, Data.Data)`，并置 `_tokenSummoned=true`
  → **每场战斗最多 1 只**。
- 引擎：`OzumatScript._try_fill`（`hsrl2/scripts/batches/batch_hero_combat3.py:696-711`）：
  ```python
  while not hero.board_full():        # ← bug: 循环填满棋盘
      src = pick(g, hero)             # 取任意存活随从
      tentacle = g.create_minion(TENTACLE_ID, controller=hero)
      if not g.summon(hero, tentacle): return
  ```
  实测 SoC 后玩家棋盘 = `[BG33_140, BG23_HERO_201pt ×6]`（7 只）→ 轻松 `win`。
- 实验验证（不改文件）：把 `on_bind` 替换成"只在有空位时召唤 1 只"后：
  棋盘 = Skipper + 1 Tentacle → **loss 8/8**（= 官方 = BobsBuddy）。
- 判定：**引擎脚本 bug（behavior_mismatch 类）**，不是上下文缺口。当前 harness 因
  `hero_power_state:player:BG23_HERO_201p:2`（`2≠0`）误把它归为上下文而掩盖了 bug。
- 修复建议（交 diff-harness 在 `tmp_diff_fix/hsrl2/` 做，不要我改）：
  ```python
  # OzumatScript.on_bind 内
  def _try_fill(g):
      if hero.board_full() or getattr(hero, "_ozumat_tentacle_summoned", False):
          return
      ...
      if g.summon(hero, tentacle):
          hero._ozumat_tentacle_summoned = True
  ```
  并在每场战斗开始的清理点重置该标记（或每场新建 hero 状态）。
  风险：该脚本的 docstring/测试（`test_batch_hero_combat3.py` 的"空位被 2/2 Taunt Tentacle 填满"）
  是按"填满"写的 → 需要同步改测试期望（**这正是既有测试固化了错误语义**）。
- 另注：引擎在"空棋盘"时不召唤（`pick` 返回 None 直接 return），BobsBuddy 会召唤；
  边界次要，可在同一修复中按 BobsBuddy 语义处理（`FriendlySide.Count < 7` 即可召唤）。

### D5 `9e80beef2ada485a` — Teron Gorefiend 目标状态（未建模，非引擎 bug）
- trace：玩家 Risen Rider 2/1 Taunt+Reborn，`hero_power_id=BG25_HERO_103p`，
  `hero_power_used=true`、`hero_power_data=376`（= 被选随从的 **game_id**）。
- BobsBuddy：`TeronGorefiendHeroPower`（`BB:32451-32519`）：
  `OnStartOfCombat` → 按 `Data.Data == game_id` 找到目标 → `CloneAsShallowExactCopy()` 存 `_clone`
  → `Destroy(target)` → 注册触发器；`OnAfterAttackStep`/`OnAfterStartOfCombat` 调
  `TrySummonClone()`（按"离右端距离"复位召唤）。官方结果 `win` 来自：Teron 摧毁/Reborn 交互
  后玩家最终多出一只随从。
- 引擎：`TeronReanimationScript.on_bind`（`hsrl2/scripts/batches/batch_hero_combat3.py:489-530`）
  用 `hero._teron_uuid` 定位目标；harness 的 trace **没有逐随从 game_id**，无法接线
  → `_teron_uuid` 为空 → SoC 什么都不做 → tie。
- 实验验证：手工把 `hero._teron_uuid` 设为唯一随从的 uuid → **win 16/16**（= 官方 = BobsBuddy）。
- 判定：**unmodeled_dynamic_state（hero power target）**，归因正确；
  **真正的修法在 traceizer**：抽取每条轨迹时记录每只随从的 `game_id`/`entity_id`，
  harness 再按 `hero_power_data` 找到目标并传给引擎（新 gap 名建议
  `hero_power_target_unresolved`，仅当没有 entity id 时出现）。

### D6 `11285dbbe007f132` — 动态属性 + Teron（medium）
- trace：Eternal Knight 20/10 `ScriptDataNum1=4/2`、Soul Rewinder 8/1、Risen Rider；
  对面 Wildfire Elemental 6/3、Risen Rider 3/1、Water Droplet 3/3；`dynamic_stat_script`。
- 引擎分布 win 6/tie 10 vs BobsBuddy tie .739/loss .261、官方 tie。
  Eternal Knight 的"本局死过的 Eternal Knight 数量"来自动态 ScriptDataNum，引擎按 CardDef 静态值
  重建 → 属性/脚本行为偏差；Teron 目标同样未接线。
- 判定：**unmodeled_dynamic_state**（medium）；需要 Power.log 级动态计数才能真实复现，不是战斗核心 bug。

---

## 3. Lead 点名的 4 类交互：BobsBuddy 精确路径 vs 引擎（结论：一致，不是根因）

> 记号：`BB:` = `reversed/rea/bobsbuddy/src_full/BobsBuddy.decompiled.cs`；
> `CB:` = `hsrl2/combat.py`；`GM:` = `hsrl2/game.py`；`MN:` = `hsrl2/minion.py`。

### 3.1 圣盾吸收 / 破盾后的第二次攻击

**BobsBuddy**
1. 圣盾在**伤害组结算**里吸收整条伤害并 `continue`（不扣血、不触发剧毒/击杀）：
   `Simulator.ProcessDamage(IEnumerable<Damage>)`，`BB:7470-7498`：
   ```
   foreach dmg in group:
       if amount == 0: continue
       if target.hasDiv:
           target.div--; 立即 resolve 破盾触发; continue      # 整条吸收
       ...
       target.baseHealth -= amount
   ```
2. 风怒/超级风怒的多次攻击是 `AttackWithMinion` 的循环（`BB:6402-6444`）：
   ```
   num2 = mega ? 4 : windfury ? 2 : 1
   for num in 0..num2:
       if num > 0 and target != null and !preserveTarget: target = null   # 重新选目标
       if !PerformAttack(attacker, target): break
       ResolveTriggersInCurrentScope(); ResolveReborn(); ResolveAllDeathsAndTriggers(true)
       双方 OnAfterAttackStep
   ```
   即：**第二次攻击是独立的一次攻击实例**（重新选目标、独立伤害组、独立圣盾判定），
   除非攻击者在结算中死亡（`PerformAttack` 开头 `if (!attacker.IsAlive()) return false`，`BB:6459-6462`）。

**引擎**：每次 `_execute_attack` 独立走 `take_damage`；圣盾在 `Minion.take_damage`
（`MN:159-194`）里首次吸收并 `clear(DIVINE_SHIELD)`；风怒第二次攻击在
`_attack_loop` 的 `if attacker.windfury and not attacker.dead and attacker.can_attack():`
（`CB:232-236`）里重新 `_choose_target`。
**实测（裸战团，无秘密）**：Annoy 1/2 Taunt+DS vs Cyclone 2/1 DS+WF → **tie 16/16**。
推演：第一击 Annoy 圣盾吸收 2、反伤 1 破 Cyclone 圣盾；风怒第二击 Annoy 无盾吃 2 死、
反伤 1 打死 Cyclone → **双亡=平**。这与官方规则一致；官方 `win` 是因为 Pack Tactics 秘密。
**结论：一致，不是根因。**

### 3.2 风怒第二击的目标重选

- BobsBuddy：`AttackWithMinion` 第 2 次循环前 `target = null`（`BB:6415-6418`）→
  `PerformAttack` 内 `attacker.ChooseAttackTarget()`（`BB:6463-6465`）；
  目标选择 = 存活且非潜行 → 嘲讽优先 → **均匀随机**（`Minion.ChooseAttackTarget`，`BB:1051-1063`）。
- 引擎：`CB:232-238` 每次 `_choose_target(defender_side)`；`_choose_target`（`CB:251-258`）
  同样是嘲讽优先 + `game.rng.choice` 均匀。
- **结论：一致**。D1 只有一个目标，重选无差异，不可能是 tie 的原因。

### 3.3 复生触发时机 / "攻击者存活判定"

- BobsBuddy（复生是**延迟批处理**，且在整波亡语之后）：
  - 死亡处理 `ResolveDeathsForSide`（`BB:7635-7757`）对每个死者只把复生**登记**到
    `state.Reborn`（`BB:7721-7728`），不立即召唤；
  - `ResolveAllDeathsAndTriggers`（`BB:7128-7145`）循环：
    ```
    do { changed = ResolveDeaths(); changed |= ResolveTriggersInCurrentScope();
         changed |= ResolveReborn(); } while (changed)
    ```
  - `ResolveReborn`（`BB:7921-7953`）：跳过"还有未跑完亡语"的 source（`SourceHasPendingDeathrattle`）；
    召唤体由 `Minion.OnReborn`（`BB:934-983`）生成（默认 `CloneAsVanilla()` + 1 血）。
  - **"攻击者是否存活"与复生无关**：复生只取决于死者自身 `reborn`；发起攻击者的存活只影响
    它自己能否继续风怒连击（见 3.1）。
- 引擎：`Game._process_death_inner`（`GM:2476-2590`）在同一函数内做 离场→亡语→复生
  （`GM:2543-2565`）→Avenge；`check_deaths`（`GM:2438-2458`）按波次循环。
- 差异：BobsBuddy **整波亡语后统一复生**，引擎**每个死者亡语后立即复生**（已在
  `reversed/rea/bobsbuddy_vs_hsrl2.md` §5 记为 P0-5，中等置信）。**但 D1/D3/D5 都是单/双随从，
  该差异对结果无影响**；实测 M2（Risen 2/1 Taunt+Reborn vs 3/3）引擎 tie，与规则推演一致。
- **结论：不是这 6 条的原因**；复生"保留 buff 与否"才是与 BobsBuddy 的分歧点（见 §6 置信度）。

### 3.4 双向同时伤害的死亡 / 复生批处理

- BobsBuddy：一次攻击的**主目标 + 顺劈邻居 + 反击**在**同一个伤害组**里（`BB:6552-6581`），
  `ProcessDamage` 先对组内所有条目扣血、再统一 `ResolveTriggers`（`BB:7470-7591`），
  之后 `ResolveAllDeathsAndTriggers` 统一收集死亡波（`ResolveDeaths`，`BB:7623-7633`，
  双方死亡先后随机）。
- 引擎：主目标/反击在同一次 `_execute_attack` 内依次 `take_damage`（`CB:281-292`），然后
  `check_deaths()`（`CB:307`）一次收双方死亡；顺劈在死亡判定**之后**单独结算（`CB:295-304`）。
- 对 D1 这类"1v1 同归于尽"场景，两者可观测结果相同（双亡→平）。
  **实测 M3（2/2 vs 2/2）→ tie 16/16；M5（Annoy 1/2 Taunt+DS vs Risen 2/1 Taunt+Reborn）→ tie。**
- **结论：不是这 6 条的原因**（顺劈“攻击者死亡后不结算”的另一问题与本案无关，见 §6）。

---

## 4. 为什么"引擎 tie / 官方 win(loss)"看起来像战斗顺序 bug

因为 trace 记录的是**日志里可见的战团快照**：秘密（`detected`）、外部亡语
（`Deathrattles` 注释）、技能目标（`hero_power_data`）都是**卡面/外部效果**，不在随从属性里。
引擎只拿"名字+攻血+关键词"复现，当然打不出官方结果。这属于**输入不完整**，
不是 `combat.py` 的结算顺序错误——§3 的四类机制在裸战团上的实测结果都符合规则推演。

---

## 5. 给 diff-harness 的修复建议（含最小失败测试断言）

### 5.1 关键修正 1：`hero_power_state` 启发式太粗（会把真 bug 藏起来）

当前 `simulate_trace`（`tools/diff_replay_combat.py:410-413`）：
```python
if band.hero_power_data not in (None, 0):
    result.context_gaps.append(f"hero_power_state:{side}:{power_id}:{data}")
```
`hero_power_data` 有两种含义：
* **静态参数**（= CardDef 的 `script_data_num_*`）：Ozumat 的 `2` 就是 Tentacle 基础攻血
  （引擎 `BG23_HERO_201p` 的 `num(0)=2`，已实测）；For the Horde! 的 `3`= 剩余购买数。
  **这类不应算 blocking gap**。
* **动态状态**：Teron 的 `376` / `1913` = 被选随从的 game_id；Choose Your Champion 的 `57`
  = 招募期金币进度（**与战斗无关**）。只有"战斗相关且目标不可解析"才应 blocking。

建议实现（伪代码）：
```python
COMBAT_POWER_TARGETED = {"BG25_HERO_103p"}          # 战斗内按 game_id 选目标
RECRUIT_ONLY_POWERS = {                              # 招募期技能：data 与战斗无关
    "BG20_HERO_102p", "BG20_HERO_280p5", "BG23_HERO_304p",
    "BG27_HERO_801p2", "TB_BaconShop_HP_020",  # 秘密子系统（另由 detected_effect 处理）
}
def classify_power(power_id, data, db):
    if data in (None, 0) or power_id in RECRUIT_ONLY_POWERS:
        return None                                  # 非战斗上下文
    cd = db.get(power_id)
    static = {cd.num(i) for i in range(4)} - {None}
    if data in static:
        return None                                  # 静态参数，如 Ozumat 2
    if power_id in COMBAT_POWER_TARGETED:
        return f"hero_power_target_unresolved:{power_id}:{data}"   # Teron：需要 entity id
    return f"hero_power_state_unclassified:{power_id}:{data}"      # 保守：仍 blocking
```
再配套：抽取 trace 时给每只随从记录 `game_id`/`entity_id`，Teron 类技能就能真实接线，
`hero_power_target_unresolved` 也会消失。

**效果（实测预期）**：
- D3：`hero_power_state:...280p5:3`（招募期）被移除；仍被 `deathrattle_annotation:BG25_001` 阻断 → 归因不变。
- D4：两个 `hero_power_state` 都被移除（Ozumat 静态 2 / For the Horde! 招募期）
  → 该轨迹变回 **ENGINE_DIVERGENCE（deterministic_mismatch）**，暴露 Ozumat bug。
- D5：玩家 Teron gap 变为 `hero_power_target_unresolved`（保留阻断）；对手 `801p2:57` 移除。
- D6：Teron 同理。

### 5.2 关键修正 2：报告 md/json 一致性 + `docs/DIFF_HARNESS.md` §5.4 已过期
`runs/diff_report.md`（16:35 旧版）仍写 `behavior_mismatch 6`，而 16:44 的 json 已是 0。
建议回归命令固定先删旧产物再生成，或让 `--md` 与 `--json` 永远同一次写出：
```powershell
Remove-Item runs\diff_report.md, runs\diff_report.json -ErrorAction SilentlyContinue
python tools\diff_replay_combat.py
# 断言：summary.divergence_attribution 里没有 behavior_mismatch（修好 Ozumat 前应为 missing_script 247 + 1 条 ozumat）
```
另：**`docs/DIFF_HARNESS.md` §5.4 的「复现 3（对照组）」已过期/不成立**——文档写
"官方 100% 我方负；引擎剥离英雄技能后一致 → `{'loss': 16}`"，但本轮实测同一战团
（Risen Rider 2/1 Taunt+Reborn vs BG30_129 3/3、无英雄技能）引擎是 **`{'tie': 16}`**
（`reversed/rea/scripts/repro3_check.py`，seed 0 轨迹：3/3 先手→Risen 复生→Risen 打死 3/3
同归于尽 = 平）。因此该条**不能**作为"引擎在裸战团上会判 loss"的控制组证据；
`§5.4` 的复现 1/2 的 `tie` 则是**正确**结果（缺秘密/外部亡语），不应改引擎去迎合。

### 5.3 最小失败测试断言（可直接落 `tests/` 或 `tmp_diff_fix` 回归）

> 说明：以下断言基于 16:44 的 harness 行为 + 本报告 §2 的根因。**D4 那条在修正 5.1 之前会失败**，
> 修正后应通过（此时 D4 暴露为引擎分歧，再由 §5.4 的引擎修复消解）。

**(A) harness 级（纯函数 / 轨迹级，无需重跑 679 场）**
```python
# tests/test_diff_behavior_mismatch.py
import json
from pathlib import Path

TRACES = Path(r"runs/official_traces")

def _record(trace_id):
    data = json.loads(Path(r"runs/diff_report.json").read_text(encoding="utf-8-sig"))
    return next(r for r in data["records"] if r["trace"]["trace_id"] == trace_id)

def test_no_behavior_mismatch_for_known_context():
    """D1/D2（秘密）、D3（外部亡语）、D5（Teron 目标）必须被归为上下文，而不是引擎 bug。"""
    expect = {
        "4c3c73caa189365e": "detected_effect:player:S:PackTactics",   # D1
        "aa4c75414541d87b": "detected_effect:player:S:PackTactics",   # D2
        "b2683903c4464b6f": "deathrattle_annotation:BG25_001",        # D3
        "9e80beef2ada485a": "hero_power_target_unresolved:BG25_HERO_103p:376",  # D5（修正 5.1 后）
    }
    for tid, gap in expect.items():
        rec = _record(tid)
        assert rec["attribution"]["attribution"] != "behavior_mismatch", tid
        assert any(g == gap or g.startswith(gap.split(":376")[0]) for g in rec["simulation"]["context_gaps"]), (tid, rec["simulation"]["context_gaps"])

def test_ozumat_is_not_swallowed_as_context():
    """D4 是真实引擎 bug：修正 5.1 后必须暴露为引擎分歧（官方 loss / 引擎 win），不能算上下文。"""
    rec = _record("f5d1c647b2f449d1")
    assert rec["attribution"]["attribution"] != "unmodeled_dynamic_state"
    assert rec["comparison"]["status"] == "ENGINE_DIVERGENCE"
    assert rec["comparison"]["subclass"] == "deterministic_mismatch"
    assert "BG23_HERO_201p" in str(rec["attribution"].get("suspect_cards"))

def test_recruit_only_power_state_not_blocking():
    """招募期技能的 hero_power_data 不应产生 blocking gap（For the Horde! / Glaive Ricochet / Choose Your Champion）。"""
    for tid in ("f5d1c647b2f449d1", "b2683903c4464b6f", "9e80beef2ada485a"):
        gaps = _record(tid)["simulation"]["context_gaps"]
        assert not any(g.startswith(("hero_power_state:player:BG20_HERO_102p",
                                     "hero_power_state:player:BG20_HERO_280p5",
                                     "hero_power_state:opponent:BG27_HERO_801p2")) for g in gaps), (tid, gaps)
```

**(B) 引擎级（`tmp_diff_fix/hsrl2/tests/`，修 Ozumat 前必失败）**
```python
def test_ozumat_summons_exactly_one_tentacle():
    """官方 CardDefs: 'summon a @/@ Tentacle'; BobsBuddy OzumatHeroPower 用 _tokenSummoned 保证每场 1 只。
    现状 bug：OzumatScript._try_fill 的 while 循环会填满棋盘（1 只随从 → 6 只 Tentacle）。"""
    ...  # 构造：Hero(BG20_HERO_100 或含 BG23_HERO_201p 的 hero) + wire_hero_power_on_bind
    # 玩家：River Skipper 1/1；对手：Risen Rider 3/2 Taunt+Reborn
    # 在第一次 before_attack 事件时快照玩家棋盘
    assert len(player_board_at_first_attack) == 2      # Skipper + 1 Tentacle（当前实际 7）
    assert (r.winner is b) is True                     # 官方 loss / BobsBuddy 100% loss（当前 win）

def test_ozumat_one_tentacle_is_loss_reference():
    """等价正确性基准：手工摆 Skipper + 1×2/2 Taunt Tentacle（不接技能）vs Risen Rider 3/2 → loss。"""
    ...
```
（参考实现：`reversed/rea/scripts/behavior_mismatch_repro.py` 的 `run_trace`/`micro`；
`reversed/rea/scripts/ozumat_fix_check.py` 展示了 monkey-patch 后 8/8 loss。）

**(C) 核心机制回归（防止为了"修 bug"改坏 combat 核心）**
```python
def test_divine_shield_windfury_exchange_is_tie():
    """Annoy 1/2 Taunt+DS vs Cyclone 2/1 DS+WF（无秘密）→ 双亡=平（官方规则推演）。"""
    assert micro([("BG_GVG_085", 1, 2, [TAUNT, DIVINE_SHIELD])],
                 [("BGS_119", 2, 1, [DIVINE_SHIELD, WINDFURY])]) == {"tie": 16}

def test_reborn_minion_double_ko_is_tie():
    """Risen 2/1 Taunt+Reborn vs 3/3 → 双方最终都死 = 平。"""
    assert micro([("BG25_001", 2, 1, [TAUNT, REBORN])],
                 [("BG35_814", 3, 3, [])]) == {"tie": 16}

def test_windfury_second_attack_retargets():
    """风怒第二击必须重新选目标（BobsBuddy BB:6415；引擎 CB:232）。
    用 before_attack 事件流断言第二击 defender_slot 与第一击不同。"""
    # 玩家：Cyclone 2/1 DS+WF（slot0）+ 2×1/1；对手：2×1/1 → 玩家先攻（3>2）
    # events[0] = attack(actor player slot0, target opp slot0)
    # events[1] = attack(actor player slot0, target opp slot1)   ← 断言
```

### 5.4 引擎修复点（给 diff-harness，在 `tmp_diff_fix/hsrl2/` 做）
- 文件：`hsrl2/scripts/batches/batch_hero_combat3.py`，`OzumatScript._try_fill`（646-727 行区间）。
- 改法：把 `while not hero.board_full()` 改为"**每场战斗最多一次**"（`_token_summoned` 标记），
  召唤**一只** `BG23_HERO_201pt`，`SetStats` 语义保留（`num(0)+已售加成`）。
- 同步更新 `test_batch_hero_combat3.py` 中"空位被 Tentacle 填满"的既有断言（它固化了错误语义）。
- 风险：低（单技能脚本，战斗核心不动）；影响面：Ozumat 对局胜率会显著下降（更真实）。

---

## 6. 置信度：BobsBuddy 行为 vs 官方规则依据（回应 Lead 的提醒）

> BobsBuddy 是**社区第三方实现**，不等于官方。下表给前次报告 P0 的置信度，
> 并说明本次 6 条的影响。

| 结论 | 依据 | 置信度 | 说明 |
|------|------|--------|------|
| 本报告 D4（Ozumat 每场 1 只 Tentacle） | CardDefs 权威文本 "summon **a** @/@ Tentacle" + BobsBuddy `_tokenSummoned` + 官方日志 loss + 引擎改 1 只后 loss | **高** | 三方一致（文本/第三方实现/官方结果）；不是"BobsBuddy 一家之言" |
| 本报告 D1/D2（PackTactics 3/3 复制） | trace `detected` 字段 + BobsBuddy `CheckPackTactics` + 官方 win 100% | **高**（机制）；中（3/3 数值只有 BobsBuddy 实现证据） | 官方"Pack Tactics"秘密文本未在本地 CardDefs 找到关键词定义；但 3/3 与社区认知一致 |
| 本报告 D3（Crab 亡语 3/2） | trace `Deathrattles` 注释 + BobsBuddy `GenericDeathrattles.Crab` + Surf n' Surf 文本（CardDefs 权威） | **高** | 卡面文本与 token 数值（BG27_004t2=3/2）本地可验证 |
| 本报告 D5（Teron 目标/克隆） | CardDefs 权威文本（Destroy/Start of Combat/resummon exact copy）+ BobsBuddy 实现 + 官方 win + 引擎接线后 win | **高** | 三方一致 |
| P0 #1 复生默认"原版克隆"（丢 buff） | **仅 BobsBuddy** `Minion.OnReborn` 默认 `CloneAsVanilla()`（`BB:934-983`）；本地 CardDefs 无 Reborn 关键词定义文本；引擎注释引用 hearthstone.wiki.gg/wiki/Reborn#Notes | **中低** | "复生保留/丢失附魔"官方文本未取证；**不要**据此直接改引擎。建议先采 Power.log（复生随从带 buff 的战例）再定 |
| P0 #2 猛毒/剧毒"不要求攻击者存活" | CardDefs 权威文本：Poisonous/Venomous = "Destroy any minion damaged by this minion."（本地实测到原文）；BobsBuddy 在伤害阶段直接 `baseHealth=-10000`（`BB:7533-7548`） | **高**（规则语义）；**中**（内部时序） | "造成伤害即毁灭"与来源是否存活无关是高置信；"毒杀与普通伤害同一死亡波"是引擎可实现细节，官方需 Power.log 验证 |
| P0 #3 顺劈与反击同一伤害组 | CardDefs 权威文本：Cleave = "Also damages the minions next to whomever this attacks."（本地实测到原文）；伤害同时结算 → 攻击者死亡不取消顺劈；BobsBuddy 同组（`BB:6552-6581`） | **高** | 规则语义高置信；引擎 `CB:295-304` 的 `not attacker.dead` 门槛值得修 |
| P0 #4 伤害上限 = 服务端 tag 2089 | HDT 代码 `INV:652-655` + 客户端 GAME_TAG 枚举（`BACON_COMBAT_DAMAGE_CAP=2089`） | **高** | 这是宿主实现+客户端数据的直接证据，不是 BobsBuddy 推测 |
| P0 #5 死亡波次"整波亡语后统一复生" | 仅 BobsBuddy（`BB:7128-7145`）；官方顺序需 Power.log | **中低** | 本报告 §3.3 指出该差异对 6 条均无影响；不建议据此大改 |
| P0 #6 SoC 顺序（任务奖励先于饰品/两侧交替） | 仅 BobsBuddy（`BB:8265-8524`）；官方顺序需 Power.log | **中低** | 同上；本报告未用其解释任何分歧 |

**行动含义**：P0 #1/#5/#6 先不要动引擎（证据只有 BobsBuddy）；P0 #2/#3/#4 可以按"官方文本+规则语义"
排期修；本报告 D4 是**唯一**由官方日志+BobsBuddy+实验三方确认的引擎脚本 bug。

---

## 7. 结论清单

| 项 | 结论 |
|----|------|
| 「6 条 behavior_mismatch」 | 报告已过期（现为 0）；其中 4 条真上下文缺失、1 条动态属性、**1 条真引擎 bug（D4 Ozumat）** |
| 战斗核心 4 类机制（Lead 点名） | BobsBuddy 路径已取证（§3，方法+行号+伪代码）；**引擎实测一致，不是根因** |
| D4 是否"英雄技能接线 artifact" | **不是接线问题**，是引擎 `OzumatScript` 召唤数量 bug（while 填满棋盘）；关技能"一致"只是巧合 |
| 需改的地方 | ①引擎 `OzumatScript`（1 只 Tentacle + 改测试期望）；②harness `hero_power_state` 分类启发式；③traceizer 记录逐随从 game_id（Teron 类目标）；④报告再生成 |
| 不需要改的地方 | `combat.py` 的圣盾/风怒/复生/同时伤害结算（本 6 条不指向它们）；`hsrl2/` 本体（由 Lead 排期） |

## 8. 复现与证据文件

| 证据 | 路径 |
|------|------|
| 6 条轨迹原始 JSON | `runs/official_traces/{4c3c…,aa4c…,b268…,f5d1…,9e80…,11285d….}.json` |
| 旧报告（behavior_mismatch 6） | `runs/diff_report.md` 16:35 版（已被 16:44 版覆盖；本报告引用旧值） |
| 当前报告（attribution） | `runs/diff_report.json` → `records[*].attribution` / `summary.divergence_attribution` |
| 根因实验脚本 | `reversed/rea/scripts/behavior_mismatch_repro.py`、`ozumat_fix_check.py` |
| BobsBuddy 反编译证据 | `reversed/rea/bobsbuddy/src_full/BobsBuddy.decompiled.cs`（行号见正文） |
| 引擎代码 | `hsrl2/combat.py`、`hsrl2/game.py`、`hsrl2/scripts/batches/batch_hero_combat3.py` |
| CardDefs 权威文本 | `hsdata/CardDefs.xml`（build 255130） |
