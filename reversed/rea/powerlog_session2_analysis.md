# 真实对局 #2（HDT + Power.log 同局）验证报告

> 2026-10-18 22:14–22:32 一局 8 人酒馆战棋（用户 HDT 1.59.3 运行中）。
> Power.log：`Hearthstone_2026_10_18_22_14_17/Power.log`，31,291,968 B / 232,161 行，
> 无截断（与上一局 10MB 截断不同——HDT 运行下本局完整落入日志）。
> HDT：`%APPDATA%\HearthstoneDeckTracker\Logs\hdt_log.txt`（BobsBuddy 行 473 条、
> `ValidateSimulationResult` 30 条）；`BgsLastGames.xml` 记录本局：
> `Start=22:15:03 End=22:32:11 Hero=TB_BaconShop_HERO_27 Placement=5 RatingAfter=376`。
> 副本：`runs/powerlog_2214/`、`runs/hdt_logs_2214/`、`runs/bg_last_games_20261018.xml`。

## 1. 抽取与交叉验证

| 来源 | 战斗数 | 官方结果分布 | 备注 |
|------|--------|--------------|------|
| Power.log（事件级） | **10/10** | loss 5 / tie 2 / win 3 | events 完整率 100%；战团 90%（1 场空场） |
| HDT（BobsBuddy） | **10/10** | loss 5 / tie 2 / win 3 | bb_rates 100%；hero/事件字段缺失（新版 HDT 格式差异） |

按回合对齐（Power.log `GameEntity TURN` 为回合×2：turn 2,4,…,20 ↔ HDT round 1..10）：

* **10/10 场官方结果完全一致**（win/tie/loss 逐场相同）——两个独立来源同局互证；
* 战团签名不完全一致的原因：本局 HDT 轨迹 `card_id` 缺失（HDT 1.59.3 格式变化，需 bridge 适配）、
  以及个别场次战团数量差 1（Power.log 快照 vs BB 记录口径），不影响结果对齐。

## 2. 引擎差分

| 指标 | Power.log（事件级） | HDT（BobsBuddy 分布） |
|------|---------------------|------------------------|
| MATCH | **5** | **5** |
| ENGINE_DIVERGENCE | 5 | 0 |
| behavior_mismatch | **0** | **0** |
| RNG_ORDER（首分歧=攻击先手/顺序） | **5** | — |
| context_gap | 0 | 4（3 unmodeled_context + 1 dynamic_state） |
| DATA_GAP / skipped | 0 / 0 | 0 / 1 |

**结论**：本局 10 场真实战斗中，引擎没有规则/数值分歧；全部 5 条事件级分歧都是攻击先手/顺序
（服务端 RNG 流，属预期）。HDT 侧 100% 可比较样本 MATCH。两局累计 16 场真实战斗 behavior=0。

## 3. 伤害上限（三档全部确认）

| 观测点 | 官方 `BACON_COMBAT_DAMAGE_CAP` | 回合（round） | 引擎 `damage_cap` |
|--------|-------------------------------|---------------|-------------------|
| CREATE_GAME | 2（占位） | — | — |
| 22:15:30 | **5** | 1 | 5（turn≤3） ✅ |
| 22:17:56 | **10** | 4 | 10（turn 4–7） ✅ |
| 22:24:33 | **15** | 8 | 15（turn≥8） ✅ |
| 之后至结束 | 无变化（15） | 5–10 | — |

* 引擎 `hsrl2/constants.py::damage_cap` 的 **三档 5/10/15 全部获得真实日志确认**（前提 alive>4）；
* 本局用户第 5 名被淘汰（剩 4 人），"剩 4 人解除上限"仍未直接观测到——需要一局**进入前四**的日志；
* 解析器 `reversed/rea/scripts/damage_cap_extract.py` 输出 `runs/damage_cap_powerlog_session2.json`
  （10 条观测：2×4、5×2、10×2、15×2）。
* 注：Power.log 只有本地+Bob 两个 Player 实体，`turn_alive_rows.alive_players` 恒为 2，
  不能用于 alive 档位判定；真实存活数需从 8 个 PLAYER_ID 英雄或 HDT 记录推导。

## 4. 其它发现

1. HDT 1.59.3 的 BobsBuddy 日志可用（`RunBobsBuddy=true` 生效），但**轨迹中 hero_card_id /
   minion card_id 缺失**，与 HDT 1.53 旧格式不同 → 建议 `hsbg/bridge/trace.py` 适配新格式
   （否则 HDT 轨迹只能用于分布对照，不能做战团签名/事件级交叉）。
2. 本局 Power.log 31MB **没有 10MB 截断**（HDT 运行下完整），与上一局（截断）不同；
   推测 HDT 的 log 配置/轮转改变了文件大小行为。后续采集建议保持 HDT 运行。
3. 事件级差分产物：`runs/diff_powerlog2.json/.md`；HDT 差分：`runs/diff_hdt2.json/.md`；
   轨迹：`runs/official_traces_powerlog2/`、`runs/official_traces_hdt2/`。
