# 决定性取证：官方酒馆战棋「完整实现」能否从本机游戏本体直接获得？

> 只读分析，未改引擎/data、未运行游戏。2026-10-18，rea-operator。
> 机读清单：`reversed/rea/official_implementation_audit.json`；
> 可复现脚本：`reversed/rea/scripts/audit_official_impl.py`（15/15 证据自动定位）。
> 目标版本：`37.0.0.255130`；`Assembly-CSharp.dll` sha256 `786376bddad54470…`（23,114,752 B）。

## 0. 结论（对用户决策的直接回答）

**不能。官方完整实现（战斗裁决/回合流程）不在本机游戏本体里，无法通过迁移客户端代码获得。**

| 问题 | 答案 |
|------|------|
| 客户端里有没有酒馆战棋战斗裁决实现（攻击轮转/目标随机/死亡结算/伤害/随机数）？ | **没有**。126 个托管 DLL 全量扫描：`CombatSimulator / SimulateCombat / ResolveCombat / CalculateCombat / PredictCombat / CombatResolver / CombatAdjudicator / BattleSimulator / CombatEngine / CombatLogic` 命中 **0**；`Simulate*` 只有 5 个且全是网络/UI 工具（见 §4） |
| 客户端到底是什么？ | **服务器 `PowerHistory` 的状态应用器 + 动画播放器 + UI**：`server → PegasusGame.PowerHistory(protobuf) → Network.GetPowerHistory() → Gameplay.OnPowerHistory → GameState/PowerProcessor → Entity.SetTag → SpellController/UI 播放`（§1 逐步证据） |
| 那这些"战斗"符号（`BaconCombat`/`BattlegroundsCombat`/`DamageCap`/`HistoryBlock.ATTACK/DEATHS`）是什么？ | **表现层**：战斗阶段标志/播放步骤枚举/侧栏上限显示/战斗动画加速（§2） |
| 客户端里能拿走什么官方资产？ | 卡牌/DBF 数据、GAME_TAG 等枚举、规则文本、**Power 协议 schema**、UI 侧伤害公式、`HistoryBlock` 类型（§6）——这些我们**已经**在用 |
| 想做"与官方一致"怎么办？ | **官方实现拿不到；最高一致性路线是「官方 Power.log 事件级轨迹 + 官方数据/文本 + 差分校准」**（Power.log 是服务器下发的权威 history，客户端只接收/打印），可选 BobsBuddy 作为第二实现参照（§7/§8） |

**置信度：高。** 唯一残余不确定性：`libacsdk_x64.dll`（7.3MB 混淆原生）是反作弊 SDK，无 BG/Combat 字符串（仅 `AntiCheat`×43），理论上不能 100% 排除隐藏代码，但它不是游戏逻辑载体；服务端不在本机，无法分析。

---

## 1. 决定性证据链：战斗完全由服务器驱动的 PowerHistory 决定

> 关键点：客户端**没有**"根据双方棋盘推演战斗"的入口；它只把服务器下发的 `PowerHistoryData` 列表转成 `PowerTask` 并按顺序播放/应用。
> 证据均可由 `reversed/rea/scripts/audit_official_impl.py` 自动定位（`official_implementation_audit.json → evidence[]`）。

**① 客户端订阅 PowerHistory 网络包**
`reversed/rea/audit/Gameplay.cs:243-245`（`reversed/rea/audit/` 为本次 ilspycmd 反编译产物）：
```csharp
Network network = Network.Get();
...
network.RegisterNetHandler(PowerHistory.PacketID.ID, OnPowerHistory);
```
`Gameplay.OnPowerHistory()`（`Gameplay.cs:706-718`）：
```csharp
List<Network.PowerHistory> powerHistory = Network.Get().GetPowerHistory();
...
GameState.Get().OnPowerHistory(powerHistory);   // 或入队 m_queuedPowerHistory
```
（队列消费：`Gameplay.ProcessQueuedPowerHistory()`，`Gameplay.cs:1693-1698`。）

**② PowerHistory 来自网络协议（protobuf），不是本地推演**
`Network.GetPowerHistory()`（`reversed/rea/audit/Network.cs:10578-10668`）：
```csharp
PegasusGame.PowerHistory powerHistory = m_connectApi.GetPowerHistory();   // 来自连接 API（服务器包）
...
foreach (PowerHistoryData powerHistoryData in powerHistory.List) {
    if (powerHistoryData.HasFullEntity)        ... GetFullEntity(...)
    else if (powerHistoryData.HasShowEntity)   ... GetShowEntity(...)
    else if (powerHistoryData.HasTagChange)    ... GetTagChange(...)
    else if (powerHistoryData.HasPowerStart)   ... GetBlockStart(...)      // 战斗 ATTACK/DEATHS 块
    ...
}
```
协议消息定义在 **`blizzard.bgsclient.dll`** 的 `PegasusGame.PowerHistory*`（19 类型/333 方法，全是 `Serialize/Deserialize`）；`blizzard.bgssdk.dll` 无任何 BG/Combat 符号（§5）。

**③ 服务器 history → PowerTaskList/PowerTask（纯包装）**
`PowerProcessor.OnPowerHistory(List<Network.PowerHistory>)`（`reversed/rea/audit/PowerProcessor.cs:329`）→
`BuildTaskList(powerList, ref i, taskList)`（`:696`）；
`PowerTaskList.CreateTask(Network.PowerHistory netPower)`（`reversed/net/decompiled/PowerTaskList.cs:496-504`）：
```csharp
PowerTask powerTask = new PowerTask();
powerTask.SetPower(netPower);      // 不计算任何规则，只是把服务器条目包起来
```

**④ 每条 history 被"应用"到实体，值来自服务器**
`PowerTask.DoTask()`（`reversed/rea/audit/PowerTask.cs:162` 起）分派到
`GameState.OnTagChange/OnFullEntity/OnShowEntity/OnChangeEntity/...`；
`GameState.OnTagChange(Network.HistTagChange netChange)`（`reversed/net/decompiled/GameState.cs:4631-4653`）：
```csharp
tagDelta.tag = netChange.Tag;
tagDelta.oldValue = entity.GetTag(netChange.Tag);
tagDelta.newValue = netChange.Value;      // ← 服务器下发的值，客户端不做裁决
entity.SetTag(tagDelta.tag, tagDelta.newValue);
```
`GameUtils.ApplyPower/ApplyShowEntity`（`reversed/rea/audit/GameUtils.cs:1722-1744`）：
```csharp
foreach (Network.Entity.Tag tag in showEntity.Entity.Tags)
    entity.SetTag(tag.Name, tag.Value);   // ← 同：服务器给的 tag
```

**⑤ "战斗事件"在客户端 = 服务器 HistoryBlock 条目**
`TB_BaconShop.GetRemainingCombatEvents`（`reversed/net/decompiled/TB_BaconShop.cs:2914-2936`）：
```csharp
powerProcessor.ForEachTaskList((index, taskList) => {
    if (taskList.IsStartOfBlock()) {
        Network.HistBlockStart blockStart = taskList.GetBlockStart();
        if (blockStart.BlockType == HistoryBlock.Type.ATTACK ||
            blockStart.BlockType == HistoryBlock.Type.DEATHS)   // ← 战斗事件来自服务器 block
            remainingEvents++;
        if (blockStart.BlockType == HistoryBlock.Type.ATTACK)
            remainingAttacks++;
    }
});
```
同文件 `HandleCombatSpeedUp()`（`:2938-2948`）读 `BG_COMBAT_SPEED_START_TIME / ACCELERATION /
MAX_SPEED / ATTACKS_REMAINING_*` 等**服务器 tag** 控制动画速度——战斗节奏也由服务器下发。

**⑥ 结算顺序不体现为客户端逻辑，只体现为服务器 block 的到达顺序**
客户端把 `PowerTaskList` 按 block 顺序交给 `SpellController` 播放
（`PowerProcessor.DoTaskListWithSpellController`，`reversed/rea/audit/PowerProcessor.cs:1655+`）；
`PowerProcessor.CanDeferTaskList/CanBatchTaskList/IsDeferBlockerTaskList`
（`:653-696`）只是**表现调度**（合并/延迟播放），不是规则重算。

**结论**：官方战斗裁决 = 服务器根据双方棋盘 + 随机种子生成一串 `PowerHistory`
（`FULL_ENTITY/SHOW_ENTITY/TAG_CHANGE/BLOCK_START(ATTACK|DEATHS|TRIGGER)/BLOCK_END`…）；
客户端只做 `Entity.SetTag` + 播放。**整条链路上没有"攻击轮转/目标随机/死亡顺序"的任何客户端实现。**

---

## 2. `BaconCombat` / `BattlegroundsCombat` / `DamageCap` 的语义（回答"是表现还是裁决"）

| 符号 | 所在类型/方法 | 判定 | 证据 |
|------|----------------|------|------|
| `BaconCombat`（ascii ×9） | `Assets.Global/BaconCombatStep` 枚举（`INVALID, COPY_OPPONENT_BOARD, END_OF_MINION_COMBAT, END_OF_COMBAT, DETERMINE_WINNER, PREPARE_COPY_BOARD, START_COMBAT_TRACKER`）+ `GlobalDbfRecord.get_Baconcombatstep()` | **表现/数据列**：DBF 里配置的"战斗播放步骤"，用于 UI/皮肤状态机 | `reversed/rea/client/Assets_Global.cs:478-487`；`types.tsv/methods.tsv` |
| `BattlegroundsCombat`（×1） | `GameEntity.IsInBattlegroundsCombatPhase()`（virtual false）+ `TB_BaconShop` override `return IsBattlePhase()` | **表现**：当前是否在战斗阶段的 UI 标志（由服务器 tag `BACON_IN_COMBAT_PHASE` 驱动） | `TB_BaconShop.cs:2538-2541`；`GameEntity.cs:1524-1532`、`:659-664` |
| `DamageCap`（×21） | `DamageCapPanel`；`GameState.DamageCapChangedCallback/Listener`；`PlayerLeaderboardManager.UpdateDamageCapFX`；`Entity.Get/SetRealtimeBaconDamageCapEnabled`；`TB_BaconShop.PlayVOIfLethal` | **UI/tag 显示**：值来自 GameEntity tag 2089（`BACON_COMBAT_DAMAGE_CAP`），仅用于"是否致命/击杀语音/侧栏数字/FX"，客户端**无档位表** | `GameState.cs:1908-1910`；`TB_BaconShop.cs:1695`；`PlayerLeaderboardRecentCombatsPanel.cs:154-162` |
| `BaconBoardTester` | `MonoBehaviour`：PlayMaker FSM + 棋盘皮肤下拉框（PC/Phone 皮肤/状态切换） | **开发期视觉 authoring 工具**，非战斗 | `reversed/net/BaconBoardTester.cs` 全文 |

另有 `BaconBoardCombatSkinBehaviour`（战斗棋盘的**皮肤**行为）、
`PetGameplayManager.UpdateBaconCombatOpponent`（宠物随战斗切换朝向，`PetGameplayManager.cs:164-176`）、
`TB_BaconShop.OnCombatPhase`（UI coroutine）、`HandleCombatSpeedUp`（动画加速）——
全部为表现层。

---

## 3. 全量程序集与符号扫描（126 个托管 DLL）

| 检查 | 结果 |
|------|------|
| `Hearthstone_Data/Managed/*.dll` | **126 个**（游戏逻辑只有 `Assembly-CSharp.dll` 22MB / `Assembly-CSharp-firstpass.dll` 30KB；其余为 Unity/Blizzard T5/System/第三方） |
| 全 DLL 字节扫描：`CombatSimulator/SimulateCombat/ResolveCombat/CalculateCombat/PredictCombat/CombatResolver/CombatAdjudicator/BattleSimulator/CombatEngine/CombatLogic` | **0 个 DLL 命中** |
| 含 `BaconCombat/BattlegroundsCombat/DamageCap/BaconBoardTester` 的 DLL | **只有 Assembly-CSharp.dll** |
| `Assembly-CSharp` 类型/方法 | **14,390 类型 / 113,222 方法** |
| 其中 BG 相关（`Bacon|Battleground`） | **381 类型 / 3,705 方法**（其中 53/381 继承 MonoBehaviour/ScriptableObject，其余多为数据/枚举/回调；**没有任何战斗裁决入口**） |
| `Combat` 子串 | 15 类型 / 131 方法（全部 UI/皮肤/教程/阶段） |
| `Simulate*` | **5 个**：`Network.SimulateUncleanDisconnectFromGameServer`、`Network.SimulateReceivedPacketFromServer`、`SelectorWheel.SimulateWheel`、`QuestManager.SimulateQuestProgress`、`QuestManager.SimulateQuestNotificationPopup` —— 全部与战斗无关 |
| `Predict*` | 11 个，全部 `ZoneMgr.PredictZone*` / `InputManager.PredictSpentMana`：**UI 区域/费用预测**（用于动画平滑），不是战斗 |
| `Calculate*` | 47 个，UI 尺寸/经验/卡组统计（BigCard bounds、XP bar 等） |
| `blizzard.bgsclient.dll`（1.9MB） | 1,706 类型/15,874 方法；BG 相关 108 类型/1,007 方法 = **PegasusUtil 协议消息**（评分/皮肤/组队/举报等）；`PowerHistory` 19 类型/333 方法 = **protobuf Serialize/Deserialize**；`Combat` 2 类型 38 方法 = **Mercenaries(Lettuce) 地图数据**；`Simulat*` 0 |
| `blizzard.bgssdk.dll`（6.3MB） | 4,372 类型/49,392 方法；**Battleground/Bacon/Combat/PowerHistory/Simulat 全部 0**（纯 Battle.net Game Service SDK） |
| 原生 `Hearthstone_Data/Plugins/x86_64/*.dll` | `libacsdk_x64.dll`（7.3MB，反作弊：`AntiCheat`×43，BG/Combat/PowerHistory=0）、`blz_commerce_sdk_plugin`、`commerce_http_client`、`tracert_plugin`、`W8TouchDLL` —— 无游戏逻辑 |
| `Data/Win/*.unity3d` | **5,155 个 / 13.06 GB**，类别：prefab 1429、material 957、texture 688、sound 588、initial 465、cardasset 423、carddef 283、cardtexture 262、video、hero、music…；**没有代码类目**。Unity Mono 工程：C# 只存在于 `Managed/*.dll`，asset bundle 不承载 CIL（抽样无 `Assembly-CSharp`/`MonoScript`/`CombatSimulator` 标记） |
| 是否存在本地/离线模式 | 否：`LocalGame` 唯一命中是资产字段名 `requireLocalGameData`；BG 所有对局（含教程）都要连 GameServer（`Gameplay` 全程 `Network.Get()`；教程结束时 `Network.Get().DisconnectFromGameServer(...)`） |
| 断线重连/观战是否本地重算战斗 | 否：`ReconnectMgr` 重新 `StartGame(...LoadGameState)` / `GameMgr.ReconnectGame(...)` 从服务器恢复（`ReconnectMgr.cs:338/453/693/704`）；观战/回放同样消费服务器 history |

**Bundle 内容说明**：Unity Mono 构建下，脚本编译产物在 `Managed/`；`Data/Win` 的 `.unity3d` 只序列化
MonoBehaviour 字段与 `MonoScript` 引用（脚本类名→程序集），以及 PlayMaker FSM 等**表现状态机**资产。
因此"asset bundle 里藏着战斗逻辑"不成立。

---

## 4. 对"本地可执行战斗/调试执行器"候选的逐一排除

| 候选 | 排除理由 |
|------|----------|
| `BaconBoardTester` | MonoBehaviour 棋盘皮肤预览工具（下拉框选皮肤/状态、PlayMaker FSM `SHOP/COMBAT` 视觉切换），无随从/伤害 |
| `BaconBoard.Cheat*` / `BaconBoardSkinBehaviour.Cheat*` / `DebugTriggerFSMState` | 皮肤/特效调试（`CheatTriggerHeroHeavyHitEffects`、`CheatTriggerAllBoardEffects` 等），非战斗 |
| `TB_BaconShop_Tutorial.OnCombatPhase` | 教学弹窗/Bob 语音 coroutine；结束时断开 GameServer（`TB_BaconShop_Tutorial.cs:215-219, 645`） |
| `Gameplay.OnScriptDebugInformation` / `AIDebugInformation` / `DebugMessage` | 客户端只**接收并显示**服务器调试信息（`Gameplay.cs:791-813`），不是本地裁决；生产环境通常不下发 |
| `ZoneMgr.PredictZone*` | 根据服务器 `PowerHistory` 预测区域变化以平滑动画，输入是服务器 history |
| `PowerProcessor.DoTaskListWithSpellController` 等 | 播放服务器 task；`CanDefer/CanBatch` 是表现调度 |
| `GameMgr.CreateGameEntity(List<Network.PowerHistory>,…)` | 从服务器 history 创建 GameEntity |
| `Utils/Dev/Debug` 类扫描（311 条 cheat/debug/tutorial 相关方法） | 全部为视觉/皮肤/教程/UI；无战斗执行器 |
| `LocalGame` | 仅是 `requireLocalGameData` 资产字段名 |

> 备注：`PowerProcessor.ContainsPoisonousEffect/ContainsCriticalHitEffect` 之类方法名看起来"像规则"，
> 但实现是**扫描服务器 history 决定播哪个特效**（暴击/毒动画），不计算结果。

---

## 5. 客户端能拿到 / 拿不到的官方资产

**能拿到（且多数已在用）**
1. **卡牌与 tag 数据**：`CardDefs.xml` / DBF（`data/` 生成源；`GameDbf` 表清单）。
2. **枚举**：`GAME_TAG` 1214 项、`TAG_CARDTYPE/ZONE/RACE/...`（`reversed/net/key_enums.json`）。
3. **官方规则文本**（`Strings/zhCN/*.txt`）：伤害公式（`GLUE_TIP_BACON_3`）、伤害上限解除（`GLUE_TIP_BACON_SOLO_1` / COMBAT_TIPS）、**亡语先于复生**（`GLUE_TIP_BACON_21`）、剧毒/烈毒定义、战斗增益临时（`GLUE_TIP_BACON_11`）、先攻规则（`GLUE_TIP_BACON_20`）。
4. **Power 协议 schema**：`PegasusGame.PowerHistory*`（bgsclient）→ 可写 Power.log 解析器/回放器。
5. **战斗事件类型**：`HistoryBlock.Type.ATTACK / DEATHS / TRIGGER / PLAY …`（客户端按此播放）。
6. **UI 侧伤害公式**：`TB_BaconShop.PlayVOIfLethal`（`PLAYER_TECH_LEVEL + Σ 场上随从 TECH_LEVEL`，与引擎 `game.py:3499-3504` 一致）。
7. **伤害上限机制**：tag 2089/3403 + `TAG_CHANGE` 监听 + 侧栏显示（见 `damage_cap_evidence.md`）。

**拿不到（都在服务端）**
1. **战斗裁决器**：攻击轮转、目标随机、风怒/顺劈、死亡/亡语/复生/Avenge 顺序、召唤上限、伤害上限档位值。
2. **随机数**：攻击目标、随机效果、酒馆刷新/发现；服务端 seed 不可见。
3. **服务端卡牌脚本实现**：客户端只有卡面文本；服务端 Power 系统才是唯一执行者。
4. **服务器状态**：匹配、淘汰、隐藏信息、对手手牌（客户端只收到过滤后的 history）。
5. **服务端二进制**：本机不存在；无法离线分析。

---

## 6. 迁移可行性评估（若用户仍想"迁移实现"）

| 方案 | 可行性 | 工作量 | 说明 |
|------|--------|--------|------|
| A. 迁移客户端 C# 战斗代码（Assembly-CSharp → Python） | **不可行/无意义** | — | 客户端没有裁决代码；381 BG 类型/3,705 方法 + 14,390/113,222 总量，依赖 Unity/PlayMaker/UI/网络；移植后仍需自写规则 |
| B. **BobsBuddy .NET 桥接**（第三方完整战斗实现） | **可行** | **1-3 天** | 本机既有 `dotnet SDK 9.0.101`，且 HDT app-1.53.0 内有 `BobsBuddy.dll`（1,282,560 B / sha256 `26dd65ca3db6eb54…` / 1817 类型 5991 方法）、`BobsBuddy.Common.dll`（4,096 B）、`HearthDb.dll`（36,744,704 B，BobsBuddy 的卡数据库依赖）。写 `net9.0`（或 netstandard 兼容）console：构造 `Input`（HearthDb 卡+关键词/tag）→ `Simulator.SimulateForInput(input, 10000)` → JSON；Python subprocess 调用。**注意：第三方实现、非官方，许可证/来源需确认，覆盖仅限其已实现卡** |
| C. BobsBuddy C#→Python 全量移植 | 可行但昂贵 | 数周~数月 | ~29,200 行反编译代码 / 5,991 方法；回归风险高；不推荐（B 方案更快） |
| D. 服务端二进制分析 | 未知/通常不可得 | 极高 | 需合法获得服务端 build；REA 原生 provider 当前未安装（Ghidra/IDA/Hopper missing）；Linux 原生符号剥离，逆向成本极高；有 ToS/法律风险；**不建议** |

**若"官方实现"是硬约束**：答案是 **不可能**（客户端没有、服务端不可得）。
**若目标是"与官方结果一致"**：不需要迁移客户端代码，走 §7 路线即可。

---

## 7. 最高一致性路线（建议）

**路线 = 官方 Power.log 事件级轨迹 + 官方数据/文本 + 差分验证 + BobsBuddy 第二实现参照**。

理由：
1. **`Power.log` 就是服务器下发的 `PowerHistory`**（客户端 `DebugPrintPower` 打印收到的条目）；
   事件级轨迹 = 官方裁决的**可观测真值**（攻击/伤害/死亡/亡语/复生/召唤的精确顺序）。
2. 现有 679 场 BG 战斗（HDT BobsBuddy 会话日志）只有"战前快照+胜负"；升级为 Power.log 后，
   可获得**事件级**真值，用于：
   - 校验 attack 顺序/目标随机分布/风怒/顺劈/毒/复生/亡语/Avenge/死亡波次；
   - 反推伤害上限 tag（2089/3403）逐回合值；
   - 事件级差分（diff-harness 已有 `compare_events` 骨架，缺真实数据）。
3. 官方数据/文本提供规则常量与边界条件；BobsBuddy 提供第二实现做"分歧定位参考"（非权威）。

**立即可执行动作**：
1. **采集 Power.log**（唯一 blocker）：用户下次运行游戏时开启日志；解析器基线已就绪
   （`reversed/rea/scripts/damage_cap_extract.py`，可扩展为通用事件解析）。
2. **可选 POC**：BobsBuddy .NET 桥接（1-3 天）→ 本地获得一个"完整战斗实现"参照，
   与 hsrl2 做逐场差分（比 HDT 离线日志更细粒度）。
3. 继续基于官方 CardDefs/DBF 的数据升级（build 255130）与规则文本校准（毒/顺劈/复生等）。

---

## 8. 复现与证据索引

```powershell
cd C:\HSagent
$env:PYTHONIOENCODING="utf-8"
# 全量审计（枚举 126 个 Managed DLL、字节扫描、解析 TSV、定位 ilspycmd 证据行）
C:\Python313\python.exe reversed\rea\scripts\audit_official_impl.py
# 只读已有反编译（更快）
C:\Python313\python.exe reversed\rea\scripts\audit_official_impl.py --skip-decompile
```

产物：
- `reversed/rea/official_implementation_audit.json`（57KB；程序集清单/符号扫描/BG 语义/证据行/迁移评估）
- `reversed/rea/audit/*.cs`（本次定点反编译：Network/Gameplay/PowerTask/PowerProcessor/
  GameUtils/GameEntity/ReconnectMgr/TB_BaconShop_Tutorial，其余复用 `reversed/net/decompiled/`）

关键证据索引（`evidence[]` 的 id → 文件:行）：
| id | 位置 | 说明 |
|----|------|------|
| `network_power_history_from_protocol` | `reversed/rea/audit/Network.cs:10578-10668` | `GetPowerHistory()` ← `m_connectApi`（protobuf） |
| `gameplay_registers_power_history_packet` | `Gameplay.cs:245, 706-718` | 注册 `PowerHistory.PacketID` → `GameState.OnPowerHistory` |
| `power_processor_builds_tasks_from_history` | `PowerProcessor.cs:329, 696` | history → PowerTaskList |
| `power_task_dispatches_server_history` | `PowerTask.cs:162+` | `DoTask()` → `GameState.On*` |
| `power_tasklist_create_task_wraps_history` | `PowerTaskList.cs:496-504` | 仅包装服务器条目 |
| `gamestate_on_tag_change_uses_server_value` | `GameState.cs:4631-4653` | `newValue = netChange.Value` |
| `gameutils_apply_power_from_network` | `GameUtils.cs:1722-1744` | 服务器 tag → `Entity.SetTag` |
| `tb_shop_combat_events_are_server_blocks` | `TB_BaconShop.cs:2914-2936` | 战斗事件 = server `ATTACK/DEATHS` blocks |
| `tb_shop_combat_speed_uses_server_tags` | `TB_BaconShop.cs:2938-2948` | 动画节奏读服务器 tag |
| `tb_shop_combat_phase_is_ui_state` | `TB_BaconShop.cs:2538-2541` | 战斗阶段 = `IsBattlePhase()`（UI） |
| `gameentity_bacon_tags_drive_ui` | `GameEntity.cs:659-664` | `BACON_*` tag → UI 标志 |
| `damagecap_applied_for_vo_only` | `TB_BaconShop.cs:1689-1707` | 上限只用于致命语音判断 |
| `gamestate_damagecap_tag_change` | `GameState.cs:1908-1910` | cap tag 变化 → UI 事件 |
| `reconnect_reloads_game_from_server` | `ReconnectMgr.cs:43/338/453/693/704` | 重连=从服务器载入，非本地重算 |

## 9. 局限

- 服务端程序不在本机，无法给出服务端裁决代码级证据（只能证明"客户端没有"）。
- `libacsdk_x64.dll` 为混淆反作弊模块（7.3MB），无 BG/Combat 字符串，未做深度反汇编（不承载游戏逻辑的置信度高，但非 100%）。
- 本机未安装 Ghidra/IDA/Hopper；若将来用户提供合法服务端二进制，需先安装原生分析 provider。
- 版本基线：`37.0.0.255130`；不同 build 的结论（"客户端无裁决"）在 Hearthstone 架构上长期成立，但若要引用请记录版本。
