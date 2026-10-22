# 伤害上限（Damage Cap）证据报告

> 只读分析；产出机读版 `reversed/rea/damage_cap_evidence.json`（由
> `reversed/rea/scripts/damage_cap_evidence.py` 可复现生成）。
> 2026-10-18，rea-operator。数据源：HDT BobsBuddy 会话日志（27 个）、
> `runs/official_traces/`（679 条 BG 战斗）、官方客户端 37.0.0.255130
> 的反编译产物与 `Strings\zhCN`、引擎 `hsrl2/constants.py`、BobsBuddy 宿主反编译。

## 0. TL;DR（三句话）

1. **本地 HDT 日志无法观测伤害上限**：27 个日志、70,984 行、679 场战斗里，
   `DamageCap / DAMAGE_CAP / BACON_COMBAT_DAMAGE_CAP` 命中 **0 次**；
   没有任何英雄血量/护甲/伤害/淘汰事件（`health` 的 105 次命中全是
   `pBeetleAtk/pBeetleHealth` 甲虫计数）。唯一可观测值是 **官方 win/tie/loss**，
   而 cap 只影响"战败方掉多少血"，**不影响胜负** → 无法从本语料反推。
   **观察表为空（0 行），这是数据可得性结论，不是"没有上限"。**
2. **官方客户端证据确认了机制**：cap 是服务器下发的 GameEntity tag
   `BACON_COMBAT_DAMAGE_CAP`(2089)，由 `BACON_COMBAT_DAMAGE_CAP_ENABLED`(3403) 门控；
   客户端在致命判定里用它截断伤害（`TB_BaconShop.cs:1694-1699`），并监听它的
   **TAG_CHANGE**（`GameState.cs:1908-1910`）→ **cap 在对局中会变化**（服务器按回合/人数更新），
   侧栏 `DamageCapPanel` 显示当前值。官方文本另确认：**只剩 4 位玩家后解除**、
   **单次上限（最大）15 点**。
3. **引擎 `5/10/15` 逐档表：机制性偏差（用本地表代替服务器 tag），档位数值 UNVERIFIED**。
   "只剩 4 人解除" ✅ 与官方一致；"上限 15"与官方 tip 一致；
   但"turn≤3→5 / ≤7→10 / ≥8→15"没有任何本地样本可验证（官方文本只说"具体数值看左边栏"）。
   修复方向：bridge 从 Power.log 抓 tag 2089/3403，用引擎已有 `_damage_cap_override`
   注入；解析器已准备好（`scripts/damage_cap_extract.py`，自测 PASS）。

## 1. 观察表（按存活人数 / 回合）

> **数据可得性：0 行。** 原因是本地没有任何"逐场 cap 值"或"英雄掉血量"可观测项
> （见 §3 负证据表）。本节保留表结构，供拿到 Power.log 后由
> `damage_cap_extract.py --markdown` 直接填充。

| turn | 存活人数 | 观察到的 cap | 样本数 | 来源 |
|------|----------|--------------|--------|------|
| — | — | **NO DATA** | 0 | HDT 日志无 tag/血量/伤害；无 Power.log |

**能提供的替代覆盖**：679 场真实 BG 战斗的**回合分布**（用于说明引擎 5/10/15 的三档
在实际语料里分别覆盖多少战斗；不代表 cap 值）：

| turn | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 |
|------|---|---|---|---|---|---|---|---|---|----|----|----|----|----|----|----|----|----|
| 场次 | 53 | 55 | 53 | 56 | 54 | 52 | 53 | 56 | 54 | 52 | 42 | 38 | 31 | 15 | 9 | 2 | 2 | 2 |

（合计 **679**，turn 1-18，无缺失。按引擎三档聚合：**turn≤3 = 161 场（23.7%）**、
turn 4-7 = 215 场（31.7%）、turn≥8 = 303 场（44.6%）——即约 76% 的战斗落在
turn 4-7 或更晚，档位差异对总伤害统计影响很大。
来源：`reversed/rea/damage_cap_evidence.json -> corpus.turn_histogram`。）

## 2. 官方客户端证据（build 37.0.0.255130）

### 2.1 GAME_TAG（`reversed/net/key_enums.json -> game_tag.values`）

| tag | id |
|-----|----|
| `BACON_COMBAT_DAMAGE_CAP` | **2089** |
| `BACON_COMBAT_DAMAGE_CAP_ENABLED` | **3403** |

### 2.2 客户端代码：在致命判定中应用 tag（官方 = 服务器值）

`reversed/net/decompiled/TB_BaconShop.cs:1689-1707`（`PlayVOIfLethal`）：
```csharp
int num = player.GetTag(GAME_TAG.PLAYER_TECH_LEVEL) + GetPlayerCombinedMinionsTechLevel(player);
int tag = GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP);
if (tag != 0 && num > tag && GameState.Get().GetGameEntity().GetRealtimeBaconDamageCapEnabled())
{
    num = tag;          // cap = 服务器下发的 tag，而不是本地档位表
}
if (num >= defense) { /* 播放击杀语音 */ }
```
- 同函数还给出**官方伤害公式**：`PLAYER_TECH_LEVEL + Σ 场上随从 TECH_LEVEL`
  （与引擎 `hsrl2/game.py:3499-3504` 一致）。
- `Entity::GetRealtimeBaconDamageCapEnabled()` / `SetRealtimeBaconDamageCapEnabled(int)`
  在 `reversed/net/Assembly-CSharp/methods.tsv:79971-79972`。

### 2.3 客户端代码：cap 会变（TAG_CHANGE 监听）

`reversed/net/decompiled/GameState.cs:1908-1910`：
```csharp
case GAME_TAG.BACON_COMBAT_DAMAGE_CAP:
    OnDamageCapChanged(change.oldValue, change.newValue);
    break;
```
配套（同文件）：`DamageCapChangedCallback(oldValue, newValue, userData)`（:72）、
`DamageCapChangedListener`（:252）、`RegisterDamageCapChangedListener`（:2645+）、
`FireDamageCapChangedEvent`（:2982+）。
→ **cap 在对局内会发生 TAG_CHANGE**，这与"按回合/存活人数变化的档位"一致，
也说明客户端不可能内置一张固定档位表（值来自服务器）。

**HUD/UI 消费方（官方代码，决定性证据）**

`reversed/rea/client/PlayerLeaderboardRecentCombatsPanel.cs:154-162`（侧栏"最近战斗"面板）：
```csharp
private void UpdateDamageCap()
{
    if (m_damageCap != null)
    {
        m_damageCapValue = GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP);
        m_damageCap.gameObject.SetActive(GameState.Get().GetGameEntity().GetRealtimeBaconDamageCapEnabled());
        m_damageCap.SetText(m_damageCapValue.ToString());   // 侧栏显示的就是服务器 tag 的数值
    }
}
```
`reversed/rea/client/PlayerLeaderboardManager.cs:714-766`（cap 变化时播 FX）：
```csharp
bool enabled = GameState.Get().GetGameEntity().HasTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP_ENABLED);
...
newValue = GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP);
...
if (!forceUpdate && oldValue == newValue && m_wasLeaderboardDamageCapFXActive == enabled) { ... }
// enabled 时：oldValue != newValue → 播放 ACTION FX（说明 cap 值确实会变）
```
`reversed/rea/client/Entity.cs:2636-2643`：`SetRealtimeBaconDamageCapEnabled(int)` /
`GetRealtimeBaconDamageCapEnabled()`（ENABLED tag 3403 的本地标志）。
`reversed/rea/client/DamageCapPanel.cs`：`SetText/ClearText`（`GLUE_BACON_GAMEPLAYLABEL_DAMAGECAP`
"伤害上限：" 的数值控件）。

→ **官方客户端显示/逻辑都只认 tag 2089 + 3403；cap 值随 TAG_CHANGE 变化。**
这也解释了为什么客户端程序集里找不到 5/10/15 档位表——档位完全在服务器侧。
（原始证据见 `reversed/rea/damage_cap_evidence.json -> official_client_evidence.client_ui_code`。）

### 2.4 官方规则文本（`C:\Program Files (x86)\Hearthstone\Strings\zhCN`）

| key | 文件:行 | 原文（zhCN） | 含义 |
|-----|---------|--------------|------|
| `GLUE_BACON_GAMEPLAYLABEL_DAMAGECAP` | GLUE.txt:384 | 伤害上限： | 侧栏显示当前 cap 的标签 |
| `GLUE_BACON_INFO_POPUP_COMBAT_TIPS` | GLUE.txt:387 | …**直到决出四强之前，单次伤害都有上限，具体数值请看左边栏。** | cap **值动态**（看侧栏），四强前有上限 |
| `GLUE_BACON_INFO_POPUP_COMBAT_TIPS_DUOS` | GLUE.txt:388 | …**单次伤害上限为15点，该限制会在只剩4位玩家后解除** | 双打文本：上限 15、4 人解除 |
| `GLUE_TIP_BACON_SOLO_1` | GLUE.txt:3674 | **英雄受到的单次伤害上限为15点。这一限制会在场上只剩4位玩家后解除。** | 单打 tip：上限 15、4 人解除 |
| `GLUE_TIP_BACON_3` | GLUE.txt:3654 | 你对敌方英雄造成的伤害等于你当前等级加上所有随从的等级。 | 官方伤害公式 |
| `GLUE_TIP_BACON_11` | GLUE.txt:3634 | 只有招募阶段的增益效果会永久保留。战斗阶段的增益效果会在战斗结束时消失。 | 战斗内增益临时（与引擎快照回滚一致） |

**解读**：单打 popup 明确写"具体数值请看左边栏"而不是写死 15，说明**单打 cap 值随对局变化**；
`GLUE_TIP_BACON_SOLO_1` 的"15"应理解为**上限/最大值（旧文案）**。

## 3. 负证据表（为什么本机拿不到观察值）

| 检查项 | 结果 | 证据 |
|--------|------|------|
| HDT 日志：`DamageCap` / `damage cap` / `DAMAGE_CAP` / `BACON_COMBAT_DAMAGE_CAP` | **0 次命中** | 27 文件 / 70,984 行扫描 |
| HDT 日志：`Simulation Input` 块 | 679（只有战团/技能快照） | `log_scan.simulation_input_blocks` |
| HDT 日志：`ValidateSimulationResultAsync >> result=` | 536 行（仅 Win/Loss/Tie + lethalResult） | `reversed/rea/hdt_bobsbuddy/BobsBuddyInvoker.cs:1296-1298` |
| HDT 日志：`health` | 105 行，**全部**是 `pBeetleAtk/pBeetleHealth`（甲虫计数） | 例 `hdt_log.txt:458` |
| HDT 日志：`armor` / `damage` / 存活人数 / 淘汰 | **0** | 关键词扫描 |
| HDT `BgsLastGames.xml` | 60 局，属性只有 `Player/StartTime/EndTime/Hero/Rating/RatingAfter/Placemenent/FriendlyGame/Duos`；**0** 个 damage/health token | 全文件扫描 |
| HDT `DeckStats.xml` / `DefaultDeckStats.xml` | 0 damage/health token | 全文件扫描 |
| 游戏侧 `Power.log`（3 个常见路径） | **不存在**（游戏近期未运行） | `powerlog_scan.found = []` |
| `runs/official_traces/*.json` | 字段只有 turn/战团/官方结果/BB 概率；`health=null`、`events=[]` | `index.json -> field_completeness.events = 0.0` |

**为什么不能"从胜负反推"**：`SimulateFight` 里 cap 只参与
`clamp(num, -cap, +cap)`（`reversed/rea/bobsbuddy/src_full/BobsBuddy.decompiled.cs:7020-7030`），
只改变"伤害数值"；官方结果的 win/tie/loss 与伤害数值独立。
要反推 cap 必须有"战败方英雄实际掉血"或"cap tag"之一，本地两样都没有。

## 4. 引擎规则 vs 官方：逐项差异

| 项 | 官方（客户端代码/文本） | 引擎（`hsrl2/constants.py:42-57`） | 判定 | 置信度 |
|----|--------------------------|-----------------------------------|------|--------|
| cap 来源 | GameEntity tag **2089**，3403 门控（服务器下发，客户端只读） | 本地 `turn` 表（5/10/15） | **机制不一致**（引擎是近似） | **高**（客户端代码 + BobsBuddy 同机制） |
| cap 是否随对局变化 | **是**（`GameState` 监听 `BACON_COMBAT_DAMAGE_CAP` TAG_CHANGE；侧栏显示"具体数值"） | 随 turn 变化（3 档） | 方向一致，具体值未验证 | 高（会变）/ 低（档位值） |
| 解除阈值 | **只剩 4 人后解除**（GLUE_TIP_BACON_SOLO_1 / DUOS） | `DAMAGE_CAP_REMOVED_AT_PLAYERS = 4`（alive≤4 → None） | **一致** | 高 |
| 上限值 | **15 点**（SOLO tip；DUOS 文本） | turn≥8 → 15 | **上限一致** | 高（文本；旧文案风险小） |
| 逐档：turn≤3→5 | 无文本/无样本 | 5 | **UNVERIFIED**（无本地证据） | 低 |
| 逐档：turn 4-7→10 | 无文本/无样本 | 10 | **UNVERIFIED** | 低 |
| 伤害公式 | `PLAYER_TECH_LEVEL + Σ TECH_LEVEL`（`TB_BaconShop.cs:1694`） | `winner.tavern_tier + Σ 存活随从 tech_level`（`game.py:3499-3504`） | **一致** | 高 |
| 生效位置 | 客户端只用于"是否致命"判断；真实伤害由服务器结算 | `run_combat` 战后 clamp（`game.py:3138-3143`） | 结构一致 | 中高 |

**结论**：
- 引擎"只剩 4 人解除"和"上限 15"与官方一致 → **不需要动**；
- 引擎用 **turn 表代替服务器 tag** 是机制性近似：若服务器档位变化节奏与 turn 表不同
  （例如随存活人数、赛季、异变改动），引擎会系统性偏差；
- 5/10/15 的具体档位**目前没有任何本地证据支持或反驳**（UNVERIFIED）。

## 5. 可执行建议（按优先级）

1. **（零风险，最高优先）接入 tag 数据源**：下次游戏运行后采集 Power.log，
   跑 `reversed/rea/scripts/damage_cap_extract.py` 产出逐回合/存活人数观察表；
   引擎用**已有** `_damage_cap_override`（`hsrl2/game.py:3139-3140`）注入实测值，
   `constants.damage_cap` 保留为 fallback。这样不需要改规则代码即可对齐官方。
2. **（数据）要求采集 ≥20 场 BG 对局**：至少覆盖 turn 1→12、存活 8→4 的档位切换，
   用于验证/替换 5/10/15 表；解析器已按"tag=2089/3403 + TURN + PLAYSTATE"实现并自测通过。
3. **（文档）标注 fallback**：在 `docs/ENGINE_NOTES.md`/常量注释里注明
   "turn 表是近似，权威值为服务器 tag 2089，见 `reversed/rea/damage_cap_evidence.md`"。
4. **不要**根据 `GLUE_TIP_BACON_SOLO_1` 把引擎改成"恒定 15"：
   popup 明确说"具体数值看左边栏"，且 `GameState` 有 TAG_CHANGE 监听；
   恒定 15 与"cap 会变"矛盾。

## 6. 复现

```powershell
cd C:\HSagent
$env:PYTHONIOENCODING="utf-8"

# 生成 JSON 证据（负证据 + 客户端/官方证据 + 语料回合直方图）
C:\Python313\python.exe reversed\rea\scripts\damage_cap_evidence.py

# Power.log 解析器自测（无需真实日志；当前 PASS）
C:\Python313\python.exe reversed\rea\scripts\damage_cap_extract.py --self-test

# 拿到 Power.log 后（游戏侧，非 HDT）：
C:\Python313\python.exe reversed\rea\scripts\damage_cap_extract.py `
  --powerlog "C:\Program Files (x86)\Hearthstone\Logs\Power.log" --markdown
```

产物：
- `reversed/rea/damage_cap_evidence.json`（机读：负证据计数、官方文件/行号、引擎对比、
  语料直方图、采集步骤）
- `reversed/rea/scripts/damage_cap_evidence.py`（生成器）
- `reversed/rea/scripts/damage_cap_extract.py`（Power.log → 观察表；`--self-test` PASS）

## 7. 局限

- 观察表为空是**数据不可得**，不是"官方没有上限"；官方上限/解除阈值有客户端文本+代码证据。
- `GLUE_TIP_BACON_SOLO_1` 的"15"是静态旧文案；popup 的"看左边栏"是更权威的动态说明。
- 若后续能取得 **HDT 截图/UI OCR**（侧栏"伤害上限：N"），也可作为次优证据源；
  但最可靠仍是 Power.log tag。
