# Power.log 实测：伤害上限（build 37.0.0.255130）

> 来源：用户 2026-10-18 21:14–21:24 的一局 8 人酒馆战棋（`Hearthstone_2026_10_18_21_14_50/Power.log`，
> 原始路径 `C:\Program Files (x86)\Hearthstone\Logs\...`，副本 `runs/powerlog/Power.log`，
> 10,244,485 B，77,372 行；**日志在 10,000 KB 处被客户端截断**）。
> 游戏类型：`GameState.DebugPrintGame() - GameType=GT_BATTLEGROUNDS`；8 英雄、`TB_BaconShop_8P_*`。

## 1. 观测（GameEntity 服务端 tag）

| 时间 | GameEntity TURN | 回合(round=ceil(TURN/2)) | `BACON_COMBAT_DAMAGE_CAP` | `..._ENABLED` |
|------|-----------------|--------------------------|---------------------------|---------------|
| 21:16:02.5176（CREATE_GAME） | — | 开局前占位 | **2** | 1 |
| 21:16:56.9156（首回合开始） | 1 | 1 | **5** | 1 |
| 21:19:23.4320（第 4 回合开始） | 7 | 4 | **10** | 1 |
| 之后至日志截断 | 12 | 6 | 无变化（10） | 1 |

- 开局 `2` 是 CREATE_GAME 时的默认占位值，随后在首回合开始即被服务端改为 **5**；
- 第 4 回合开始（GameEntity TURN=7）服务端下发 **10**；
- 日志在 **第 6 回合**（TURN=12）被 10MB 上限截断，**15 档与"剩 4 人解除上限"未观测到**。

## 2. 与引擎 `hsrl2/constants.py::damage_cap` 对照

引擎规则：`alive≤4 → None`；`turn≤3 → 5`；`turn≤7 → 10`；否则 `15`
（引擎 `turn` = 招募回合数 = `ceil(GameEntity TURN / 2)`）。

| 档位 | 引擎 | 本局实测 | 结论 |
|------|------|----------|------|
| 第 1–3 回合 | 5 | **5** | ✅ 一致（高置信，1 局） |
| 第 4 回合起 | 10 | **10** | ✅ 一致（高置信，1 局） |
| 第 8 回合起 | 15 | 未观测 | ⏳ 需更长日志 |
| alive≤4 解除 | None | 未观测 | ⏳ 需更长日志 |
| 机制 | 本地 turn 表（近似） | 服务端 tag 2089 动态下发（`_ENABLED` 门控） | 机制上仍是近似；已提供 `Game.set_damage_cap` 注入 API 供真实值回放 |

## 3. 结论与后续

1. **引擎 5/10 两档首次获得真实官方日志确认**；`REA`/`wiki` 推断的"服务端 tag 下发"机制成立
   （日志中 tag 以 `GameEntity tag=BACON_COMBAT_DAMAGE_CAP` 出现）。
2. 15 档/四强解除仍需更完整日志；本局因 **Power.log 10MB 截断**只覆盖到第 6 回合。
3. 引擎默认行为**无需改动**（5/10 已一致）；差分 harness 可在有 tag 时通过
   `set_damage_cap` 注入官方值（已实现）。
4. 采集更长日志的可行方式（见 `docs/REAL_GAME_BRIDGE.md`）：
   - 每局结束后立即复制 Power.log（单局即一个文件；本局已接近上限）；
   - 关闭 `[Power] Verbose`（可显著减小体积，但会丢失 FULL_ENTITY 的 tag 明细，需重测解析）；
   - 用 HDT（BobsBuddy 会话日志）记录战团与结果，不受 10MB 限制。

## 4. 解析器复核（diff-harness 修复版 `damage_cap_extract.py`）

- `runs/damage_cap_powerlog.json`：8 条观测（含 GameState/PowerTaskList 重复），去重后仍是
  **2（开局占位）→ 5（turn 1）→ 10（turn 4）**，与本文件 §1 独立解析一致。
- 注意：本日志的 `alive_players` 恒为 **2**（Power.log 只有本地 + Bob 两个 Player 实体），
  **不能**用于验证"剩 4 人解除上限"；该结论仍需 8 人存活信息（HDT/完整日志）。

## 5. 第二局（2026-10-18 22:14–22:32，HDT 运行）——三档全部确认

- 数据：`runs/powerlog_2214/Power.log`（31,291,968 B、232,161 行、无截断、完整一局 10 场战斗）。
- 官方 `BACON_COMBAT_DAMAGE_CAP` 时间线：

| 时间 | GameEntity TURN | round | cap | 引擎 `damage_cap` |
|------|-----------------|-------|-----|-------------------|
| 22:15:04（CREATE_GAME） | — | — | 2（占位） | — |
| 22:15:30 | 1 | 1 | **5** | 5（turn≤3） ✅ |
| 22:17:56 | 7 | 4 | **10** | 10（turn 4–7） ✅ |
| 22:24:33 | 15 | 8 | **15** | 15（turn≥8） ✅ |
| 之后至 22:32 结束 | 16–20 | 5–10 | 无变化（15） | — |

- **结论：引擎 5/10/15 三档全部被真实官方日志确认**（前提 alive>4；本局用户第 5 名，剩 4 人，未观测到解除上限）。
- 解析产物：`runs/damage_cap_powerlog_session2.json`（观测 2×4、5×2、10×2、15×2）。
- 同局 HDT 交叉验证与事件级差分见 `reversed/rea/powerlog_session2_analysis.md`。
