# BobsBuddy 离线 oracle 可行性（2026-10-18）

> 结论：**可行**。`BobsBuddy.dll` 是独立 netstandard 库（引用仅
> `HearthDb` / `BobsBuddy.Common` / `netstandard`），**不需要 HDT
> GameV2/实体数据库**；pythonnet 可直接加载并调用
> `SimulationRunner.SimulateMultiThreaded`。已落地最小可用 oracle 与
> smoke 对照；覆盖范围有限，不能承诺穷举。

## 1. API 路径（源码 + DLL 反射确认）

### 1.1 依赖/入口
`BobsBuddy.dll`（1.6MB，HDT app-1.59.3）程序集引用：
`netstandard`, `HearthDb`, `BobsBuddy.Common`；无 HDT 主程序依赖。
调用链（反射签名）：

```text
HearthDb.Cards.LoadBaseData()
Simulator()
  .MinionFactory.Create(cardId: string, controlledByPlayer: bool) -> Minion
Input()
  .Player/.Opponent = Player(Input)
Player.Side: List<Minion>
Player.Health/Tier/... + AddHeroPower(...)
Input.SetTurn(int); Input.SetHealths/SetTiers 可选
SimulationRunner()
  .SimulateMultiThreaded(Input, iterations:int, threads:int,
                         timeAllottedMs:int) -> Task<Output>
Output.winRate/tieRate/lossRate/avDamage/simulationCount
```

`BobsBuddyInvoker`（HDT 侧）只是 UI/日志适配器，离线不需要；其
`SetupInputPlayer` 展示了 HDT 如何从实体构造 `Input`（本 oracle 用
trace 的状态直接构造等价输入）。源码参考：
<https://github.com/HearthSim/Hearthstone-Deck-Tracker/tree/master/Hearthstone%20Deck%20Tracker/BobsBuddy>
（`BobsBuddyInvoker.cs::SetupInputPlayer`、`SnapshotBoardState`）。

### 1.2 已落地文件
```text
tools/bb_oracle/
  bb_oracle.py    # CLI: JSON/--trace -> win/tie/loss/avDamage
  schema.json     # 输入 schema
  test_input.json # 示例（Southsea Busker vs Scarlet Survivor）
  README.md       # 依赖/用法/限制/性能
```
用法：
```powershell
python -m pip install pythonnet
python tools/bb_oracle/bb_oracle.py --input tools/bb_oracle/test_input.json
python tools/bb_oracle/bb_oracle.py \
  --trace runs/official_traces_hdt2/7210c2dcf328ffe3.json \
  --iterations 3000 --threads 4
```
输出示例（`7210c2dcf328ffe3`，3000 次）：
```json
{"ok": true, "win": 0.0, "tie": 0.503, "loss": 0.497,
 "av_damage": -0.99, "simulation_count": 3000,
 "ignored_fields": [], "elapsed_s": 2.79, "sec_per_1000": 0.93}
```

## 2. 状态字段差距表（BB 接受 vs 真实 trace）

| 状态 | BB 能否接受 | 本 oracle 映射 | 真实 trace 可得性 | 覆盖声明 |
|------|-------------|----------------|-------------------|----------|
| 卡 id / 基础 atk/health | ✅ `MinionFactory.Create` + `baseAttack/baseHealth` | ✅ | ✅ trace 的 `card_id/atk/health` | 覆盖（边界快照值） |
| golden | ✅ `Minion.golden` | ✅ | ✅ | 覆盖 |
| taunt/DS/windfury/mega/cleave/poison/venom/reborn/stealth/cannotAttack | ✅ Minion 属性 | ✅（`keywords`） | ⚠️ trace 的 `keywords` 可能缺失（HDT trace 多为 `tags`/Enchantments） | 覆盖字段；缺失来源记 unsupported |
| ScriptDataNum1..4 | ✅ Minion 属性 | ✅（`script_data`/tags） | ⚠️ trace `tags` 有时有，Power.log 抽取常缺 | 有则覆盖，无则 unsupported |
| tier / turn / 玩家 Health | ✅ `Player.Tier` / `Input.SetTurn` | ✅ | ✅ | 覆盖 |
| hero powers | ✅ `Player.AddHeroPower(cardId, data1..3, attachedMinion, id)` | ⚠️ 只支持 id+data1 默认，语义未完整映射 | ⚠️ trace 有 `hero_power_id/data`，但 data 槽语义需核对 | 部分 |
| Enchantments（JawsOfDeath/Fortification 等） | ⚠️ BB `Minion.Enchantments` 为只读集合，卡脚本内部消费；无公开 add API | ❌ 不喂，`ignored_fields` 记录 `trace_tags:Enchantments` | ✅ HDT trace `tags["Enchantments"]` | **不支持**；6253 smoke 因此不齐 |
| trinkets / quests / objectives / secrets | ✅ `Player.Trinkets/Quests/Objectives/Secrets` | ❌ 未映射（构造工厂 APIs 存在但 trace 无字段） | ❌ 当前 trace 无 | **不支持** |
| anomaly / available races | ✅ `Input.Anomaly/availableRaces` | ❌ | ❌/未映射 | **不支持** |
| hand / setaside / 手牌 buff | ✅ `Player.Hand`、`Minion.FriendlyHand` 等 | ❌ | ⚠️ trace 只有 hand 卡名，无 buff/script data | **不支持** |
| 动态 counters（EternalKnight/GoldenMinionsPlayed/ElementalPlayCounter 等） | ✅ `Player.*Counter` 属性 | ❌ 未映射 | ⚠️ trace tags 偶见，未系统抽取 | **不支持**（除非调用方显式 JSON 扩展） |
| aura/临时关键词/附魔来源 | ⚠️ 最终值可 bake 到 atk/health/keywords；来源不可表达 | 部分 | 否 | **不承诺来源等价** |

**覆盖声明**：oracle 覆盖"真实战斗边界快照的静态可数值化状态"（卡 id、
最终属性、金色、可映射关键词、现有 ScriptDataNum、tier/turn/health）；
**不覆盖** enchantment 身份、饰品/任务/秘密、手牌内容、anomaly、动态
counter 语义。采样差分时，遇到任何 unsupported 字段必须把样本标为
`unsupported`，不能算 oracle 覆盖。真实状态空间无止境，本工具只做
**采样**，不承诺穷举。

## 3. Smoke 对照（HDT 日志既有 bb_rates）

命令：
```powershell
python tools/bb_oracle/bb_oracle.py \
  --trace runs/official_traces_hdt2/<trace>.json \
  --iterations 3000 --threads 4
```

| trace | oracle | HDT log bb_rates | 判定 |
|-------|--------|------------------|------|
| `7210c2dcf328ffe3`（Southsea 3/1 vs BG33_886 2/3） | tie .503 / loss .497 | tie .497 / loss .503 | ✅ Δ~0.6pp |
| `eb66c6d07040bc4f`（3v3 简单战团） | win .504 / tie .496 | win .5 / tie .5 | ✅ Δ~0.4pp |
| `62533201dc326471`（6v5 含 Enchantments） | win .162 / tie .457 / loss .381 | win .753 / tie .186 / loss .061 | ❌ 不齐：`ignored_fields` 含 `trace_tags:Enchantments`；BB 无法接收该来源 |

结论：对**静态边界状态可完整表达**的简单战团，oracle 与 HDT 既有 BB
概率在 1pp 内；一旦 trace 依赖 Enchantment/饰品/动态 counter，oracle
不可直接当 ground truth（需标 unsupported 或扩展输入）。

性能（本机 pythonnet，单个战团）：
- 单线程：~0.85–1.0 s / 1000 次；
- 4 线程：~0.27 s / 1000 次（7210，10000 次）。

## 4. 与当前 Power.log trace e057 的关系

`e05763998417728e` 的官方事件流把战后战团还原 `eid=904` 误记为战斗内
summon（见 `reversed/rea/powerlog_e057_analysis.md`）；BB oracle 对该
双方各 1 只的棋盘输出 tie/win 约 50/50（BB 本身也有先手随机），而引擎
tie100%（引擎规则路径）；该 trace 的 `official_result=win` 是 harness
抽取错误，不应拿来校准 oracle。用 HDT 自带 bb_rates 的 trace 做 smoke
才是正确对照。

## 5. 下一步（交给 diff-harness）

1. 批量采样：以 `runs/official_traces_*` 的战斗边界状态为 seed，
   构造 `tools/bb_oracle` 输入；对 `ignored_fields` 非空或卡创建失败
   的样本标 `unsupported`，报告覆盖比例。
2. 状态扰动采样（用户要求不只看基础卡）：在真实边界值上采样
   累计 buff（atk/health）、关键词、金色、ScriptDataNum、tier/turn；
   记录每个样本的来源 trace + 扰动字段，避免无界随机。
3. 先只对 **简单/可完整表达**样本做批量（7210/eb66 类），把
   Enchantments/trinkets/quests/anomaly 样本单列；oracle 与引擎的差异
   用于定位引擎 bug，而不是把 BB 当绝对真值。
4. 若要扩大覆盖：补 traceizer 抽取 Enchantments 列表/trinkets/quests
   与 `Player.*Counter`，再扩展 `bb_oracle` schema；这是 harness 侧工作。
