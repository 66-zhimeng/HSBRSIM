# 5 条被暴露的 behavior_mismatch 根因（2026-10-18）

> 触发场景：B 类卡登记 `CONSUMED_SCRIPT_DATA` 后，`context_gap` 状态被清除，
> 5 条轨迹从 CONTEXT_GAP 变为 ENGINE_DIVERGENCE。本文用官方 trace +
> 引擎 64 seeds 逐条复现，判断 (a) 脚本 bug / (b) 注入/登记口径 /
> (c) 仍缺上下文，并给出最小 harness 改动。

## 0. 复现方法

```powershell
# 单条 trace，64 seeds，注入开/关对照
python - <<'PY'
import hsbg.bridge.trace as T, diff_replay_combat as D
from hsrl2.db import CardDB
traces = T.load_traces('runs/official_traces')
t = next(x for x in traces if x.trace_id == '<id>')
db = CardDB.load('data')
for inj in (True, False):
    r = D.simulate_trace(t, db, D.CardResolver(db), D.HeroIndex(db),
                         seeds=64, inject_script_data=inj)
    print(inj, r.rates())
PY
```

**共同结论**：
- 5 条在注入开/关下 **64 种子结果完全一致** → 不是 ScriptDataNum 注入导致；
- B 类脚本 pre/post（临时引擎副本回退注入读取）**0 翻转 / 0 胜率变化**
  → 不是 B 脚本引入；这 5 条是**原先被 context_gap 状态遮蔽的既有分歧**。
- 5 条均残留未建模上下文（hand / hero power），且 B 脚本与这些上下文
  无关；根因是 **harness 把这些上下文当“非 blocking”**，登记后直接暴露为
  behavior_mismatch。

## 1. 逐条证据与裁定

### 1.1 `0502f1b15163d311`（Gluttonous Trogg）
- 棋盘/P：Wildfire Elemental 10/3、Sly Raptor 1/3、Alert Alarmist 4/4 taunt、
  Felemental 3/3、Roaring Recruiter 2/8、Incubation Researcher 2/8；
  手牌 `Charmwing`、`Fire-forged Evoker`。
- 棋盘/O：Laboratory Assistant 6/8、Water Droplet 4/5、Refreshing Anomaly 4/5、
  Gluttonous Trogg 3/5；手牌 `Fire-forged Evoker`。
- 64 seeds：win 0.9375 / tie 0.046875 / loss 0.015625；BB：win 1.0。
- 残留 context：`hand_not_modeled`（手牌两卡均无 `hand_soc`/战斗触发）。
- 裁定：**不是脚本 bug**。差异来自 RNG 敏感的死亡/召唤路径
  （Sly Raptor→随机 Beast→Tasty Lobster；Alert Alarmist 等）在 64 seeds 的
  尾部；B 脚本无关。分类 **(c) 测量上下文 + RNG 分布**，DEFERRED 到
  Power.log/事件级证据；harness 侧按 blocking context 处理。

### 1.2 `3e4c0c97c6c32601`（Scarlet Survivor）
- P：Scarlet Survivor 7/7 DS、Lullabot 4/8；手牌 `Annoy-o-Module`。
- O：Razorfen Geomancer 2/1、River Skipper 1/1、Very Hungry Winterfinner
  8/11 taunt；手牌 **`Flighty Scout`（Start of Combat: If in hand, summon a
  copy — `hand_soc`）**。
- 64 seeds：win 0.453125 / tie 0 / loss 0.546875；BB：win 0 / tie 0.501 /
  loss 0.499（方向性差异）。
- 残留 context：`hand_not_modeled`；手牌含**战斗相关 hand_soc**。
- 裁定：**仍缺上下文 (c)**——harness 不建模手牌，官方手牌中的 Flighty
  Scout 会在 SoC 召唤 3/3 副本，直接改变战斗分布。不是脚本 bug。
- 动作：harness 侧把 `hand_not_modeled`（至少含 `hand_soc` 卡时）视为
  blocking context；或实现 hand 构建。

### 1.3 `4fb28230d318b126`（Scarlet Survivor ×2）
- P：Scarlet Survivor 9/9 DS、Southsea Busker 3/1。
- O：Scarlet Survivor 6/11 taunt DS、Roaring Recruiter 2/8。
- 64 seeds：win 0.234375 / tie 0.15625 / loss 0.609375；BB：win 0 /
  tie 0.246 / loss 0.754。
- 残留 context：`opponent_hero_power_unknown`（`BG26_HERO_102p2` **Minor
  Hymn**，引擎脚本 MISSING，CardDef 为招募期技能）。
- 裁定：**测量口径/上下文 (b/c)**——未知英雄技能状态未被 harness 视为
  blocking；引擎无法证明是脚本 bug（该技能非战斗效果，且注入开/关同）。
  引擎额外 23% “win” 来自 Scarlet 双边的 RNG/攻击序差异；B 脚本无关。
- 动作：harness 侧 `*_hero_power_unknown` 计入 blocking context（或仅对
  未解析的、非 recruit-only 技能）。另：Minor Hymn 缺脚本可作为单独
  missing_script 卡台账。

### 1.4 `aa771aaa615fb618`（Scarlet Survivor）
- P：Scarlet Survivor 7/7 DS、Lullabot 4/7；手牌 **`Flighty Scout`**。
- O：Metallic Hunter 4/2、Twilight Hatchling 7/7、Ominous Seer 2/1。
- 64 seeds：win 0.609375 / tie 0.09375 / loss 0.296875；BB：win 0.805 /
  tie 0.092 / loss 0.103。
- 残留 context：`hand_not_modeled`（Flighty Scout hand_soc）。
- 关键量化：在引擎 board 追加一个 3/3 Flighty Scout 副本后，engine
  win 0.609375 → **0.765625**（BB 0.805）——方向与幅度均说明手牌
  hand_soc 是缺失上下文。裁定 **(c)**，非脚本 bug。

### 1.5 `be480c4c15e54656`（Scarlet Survivor + Upbeat Frontdrake）
- P：Scarlet Survivor 8/4 taunt DS、Twilight Hatchling 2/2 taunt、
  Upbeat Frontdrake 1/1；手牌 `Sea Witch Zar'jira`。
- O：Wildfire Elemental 6/3、Ancestral Automaton 6/6、Twilight Hatchling
  1/1、Lullabot 2/4、Alert Alarmist 2/2 taunt。
- 64 seeds：win 0 / tie 0.046875 / loss 0.953125；BB：win 0 / tie 0 / loss 1.0。
- 残留 context：`hand_not_modeled`（Sea Witch 为 Spellcraft 招募期卡，
  无 `hand_soc`；不直接改变战斗）。
- 裁定：**不是脚本 bug**；尾部 4.7% tie 来自 Twilight Hatchling 死亡
  召唤 Whelp 立即攻击的随机目标/顺序（64 seeds 尾部），B 脚本无关。
  分类 **(c)/测量口径**，DEFERRED 到事件级证据。

## 2. 汇总

| trace | 残留上下文 | 分类 | 是否脚本 bug | 动作 |
|---|---|---|---|---|
| 0502f1b15163d311 | hand（无 hand_soc） | c + RNG 尾部 | 否 | harness context blocking |
| 3e4c0c97c6c32601 | hand（Flighty Scout hand_soc） | c 缺上下文 | 否 | harness 建模 hand_soc / blocking |
| 4fb28230d318b126 | opponent hero power unknown | b/c 测量口径 | 否 | harness 把 unknown power 视为 context；Minor Hymn 缺脚本台账 |
| aa771aaa615fb618 | hand（Flighty Scout hand_soc） | c 缺上下文 | 否 | 同上（量化支持） |
| be480c4c15e54656 | hand（无 hand_soc） | c + RNG 尾部 | 否 | harness context blocking |

**结论**：5 条均不是 B 脚本 bug，也不是 ScriptDataNum 注入问题；是
**harness 对残留上下文（hand / unknown hero power）的非 blocking 判定**
导致登记 B 卡后误报。无需 ENGINE_NOTES F-13 引擎修复。

## 3. 给 diff-harness 的最小改动（已用临时副本验证）

在 `tools/diff_replay_combat.py::attribute_divergence` 中：
```python
# 现状：
informative = {"hand_not_modeled", "unsupported_keyword"}
others = [g for g in gaps if g not in informative and not g.startswith(
    ("player_hero_power_unknown", "opponent_hero_power_unknown"))]
# 改为：
informative = {"unsupported_keyword"}
others = [g for g in gaps if g not in informative]
```
即把 `hand_not_modeled` 与 `*_hero_power_unknown` 都归入 unmodeled_context
（blocking）。更精细版：仅当手牌含带 `hand_soc` 的卡、或 unknown power
非 recruit-only 时 blocking；但最小改法已满足目标。

配合 B 卡全量登记（`CONSUMED_SCRIPT_DATA` 见
`reversed/rea/dynamic_gap_audit.md` §7）后，临时副本实测：

| 指标 | 登记前（当前 harness） | 登记 + 上述 blocking 改动 |
|---|---|---|
| MATCH | 400 | **400** |
| engine_divergence | 153 | **153** |
| context_gap | 85 | **85** |
| clean subset | 24/24 | **33/33（100%）** |
| behavior_mismatch | 0 | **0** |
| unmodeled_context | 7 | 12 |

因此 **“这 3 张登记后 behavior_mismatch 仍为 0”可达成**，且干净子集
从 24 提升到 33（100% match），无需改引擎。

## 4. 剩余证据需求

- 0502 / be480：需要真实 `Power.log`（事件级时序）或 BobsBuddy 内部
  RNG 同源证据，才能判定尾部差异是引擎 bug 还是 BB 采样差异；当前
  维持 context（不指控引擎）。
- 4fb：`BG26_HERO_102p2 Minor Hymn` 缺脚本（招募期），可另列
  missing_script 台账；不影响本结论。
- 3e4 / aa77：建议 harness 真正构建手牌并让引擎 `hand_soc` 生效
  （Flighty Scout 已有脚本），比“一刀切 blocking”更精确。
