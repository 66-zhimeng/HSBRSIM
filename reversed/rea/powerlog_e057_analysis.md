# Power.log 规则级候选 `e05763998417728e` 裁定（2026-10-18）

> trace：`runs/official_traces_powerlog/e05763998417728e.json`
> 原始日志：`runs/powerlog/Power.log`（战斗窗口 21:18:26.254–27.222）
> 核心结论：**不是 Reborn、不是饰品/英雄技能、不是引擎 bug；是
> diff-harness 把「战后战团还原」误记为战斗内 summon，导致
> official_result 被算成 win。引擎 64 seeds tie 100% 是正确的。**

## 1. 事实复核（来自原始 Power.log）

### 1.1 参战实体
- P：`id=351 南海卖艺者（Southsea Busker）BG26_135`，player=3；
  战前由 `TB_BaconShop_HP_086`（Al'Akir 技能「随风而行」）
  依次授予 `WINDFURY=1`、`TAUNT=1`、`DIVINE_SHIELD=1`
  （`Power.log` 12062–12064；`LAST_AFFECTED_BY=183`）。
- O：`id=983 血色幸存飞龙（Scarlet Survivor）BG35_814`，player=11，
  仅 `TAUNT=1`。
- 双方基础卡均无 Reborn；战斗内实体 351/983 的 tag 流中**没有
  `REBORN` 或 `REBIRTH`**（21:17–21:19 窗口全文检索 0 命中）。

### 1.2 战斗事件（21:18:26.674）
- 351 攻击 983 两次（windfury）：每次 `PREDAMAGE=3` →
  `DAMAGE` 累计 3→6（12102–12162）。
- 983 反击 351：第一次被 `DIVINE_SHIELD` 抵消
  （`DIVINE_SHIELD_DAMAGE=1`、`DIVINE_SHIELD=0`，12122–12125），
  第二次 `DAMAGE=3` 致死（12163–12175）。
- `DEATHS` block（12190）：351 死亡（12261 `ZONE=GRAVEYARD`），
  983 死亡（12289 `ZONE=GRAVEYARD`）。
- 英雄伤害 0/0（trace `official_damage={"player":0,"opponent":0}`），
  与「双方随从全灭 → 平局」一致。

### 1.3 `eid=904` 的真正来源（关键）

| 行 | 时间 | 证据 |
|----|------|------|
| 11091 | 21:18:26.238 | `FULL_ENTITY - Creating ID=904 CardID=BG26_135`（**战斗开始前**创建） |
| 11686–11690 | 21:18:26.254 | 904 初始为 `zone=SETASIDE`、`LAST_AFFECTED_BY=352`（drag-buy 实体）、**`COPIED_FROM_ENTITY_ID=351`** |
| 12266 | 21:18:26.674 | 351 死亡后：904 获得 `tag=BACON_DIED_LAST_COMBAT value=1` |
| 12303–12319 | 21:18:26.674–708 | 战后清理 block（`TB_BaconShop_8P_PlayerE EffectIndex=-1`），移除战斗临时实体 |
| **12392** | **21:18:26.708** | **904 `TAG_CHANGE tag=ZONE value=PLAY`**（战后战团还原；随后 12393 `ZONE_POSITION=1`、12394 `1196=1`、12395 `EXHAUSTED=1`） |
| 12519 | 21:18:26.708 | `GameEntity BOARD_VISUAL_STATE=1`（战后 UI 还原） |
| 12855 | 21:18:27.222 | `BACON_IN_COMBAT_PHASE=0`（战斗窗口结束） |

**判定**：904 是 Hearthstone 在战斗开始时为玩家战团创建的
**SETASIDE 副本**（`COPIED_FROM_ENTITY_ID=351`），战斗结束后被
**还原到 PLAY**；`BACON_DIED_LAST_COMBAT=1` 只是「上战团已死」标记。
它不是战斗内召唤、不是 Reborn 复活、也不是 Drust/英雄技能产物。
（904 的 `ZONE=PLAY` 发生在 `DEATHS` block 结束后 34ms，且处于
`BOARD_VISUAL_STATE=1` 的战后还原阶段。）

## 2. 为什么引擎 64 seeds 全 tie（正确）

用 trace 原始棋盘（P `BG26_135` 3/1，keywords taunt/DS/windfury；
O `BG35_814` 3/6 taunt）跑 `simulate_trace(seeds=64)`：
**engine rates = win 0 / tie 1.0 / loss 0**。
规则推演：
1. Southsea 3 攻 + windfury → 两次攻击 ×3 = 6 伤害，恰好击杀 3/6
   Scarlet；
2. Scarlet 反击两次 ×3：第一次被 DS 吸收，第二次击杀 1 血 Southsea；
3. 同一攻击轮双方随从全灭 → **平局**（双方英雄 0 伤害）。
因此引擎 tie 100% 是正确的；trace 的 `official_result=win` 是
**harness 事件归因/胜负数错误**。

## 3. 根因：diff-harness 把战后还原记成战斗内 summon

`hsbg/bridge/trace.py` 的 `_on_zone_change()` 规则：
> `new_zone == "PLAY" and old_zone not in (None, "PLAY")` → 记
> `kind="summon"`

904 的 `SETASIDE → PLAY` 满足该规则，被记成 `summon(player, eid=904)`；
随后 `_official_result()` 用「快照初始数 + 战斗内 summon - death」
计算终局存活：p_surv = 1 - 1 + 1 = 1、o_surv = 1 - 1 = 0 → **win**。
若去掉该伪 summon：p_surv=0、o_surv=0 → **tie**，与引擎 64 seeds
tie 100% 完全一致（手工 `compare_combat(official_result="tie")`
返回 `MATCH / outcome_match`）。

**这是抓取/归因遗漏，不是引擎 bug。**

## 4. 给 diff-harness 的最小修改（不改 hsbg）

在 `hsbg/bridge/trace.py::_on_zone_change` 中，过滤「战后战团还原」：

```python
elif new_zone == "PLAY" and old_zone not in (None, "PLAY"):
    # 战后战团还原：战斗开始时创建、COPIED_FROM 旧战团实体、
    # 死亡后带 BACON_DIED_LAST_COMBAT 的 SETASIDE 副本 → 不是召唤
    if old_zone == "SETASIDE" and (
            ent.tag("BACON_DIED_LAST_COMBAT") is not None
            or ent.tag("COPIED_FROM_ENTITY_ID") is not None):
        return
    self._combat["events"].append(...)
```

更稳的补充（二选一）：
1. 在 `DEATHS` block 结束 / `BOARD_VISUAL_STATE=1` 后**停止记录
   summon/death 事件**（战斗事件窗口在战后还原前关闭）；
2. 记录 `_start_combat` 时已存在的 SETASIDE 副本 eid 集合，凡该集合
   内实体进入 PLAY 均视为还原（而非召唤）。
建议同时给 traceizer 增加守护测试：`e05763998417728e` 提取后
`official_result == "tie"` 且 events 中无 eid904 summon。

## 5. `GAME_SEED` 能否对齐官方攻击先手/RNG

- 日志 `GameEntity GAME_SEED=1757749361` 是**服务端本局种子**；
- 引擎 `GameRNG` 是 Python `random.Random` 的薄封装
  （`hsrl2/rng.py`），与 Hearthstone 服务端 RNG 的算法/调用顺序
  不同；把 `GAME_SEED` 直接当 `Game(seed=...)` **不能**复现官方
  攻击先手、随机目标或随机召唤结果。
- 可做的：
  1. **同一官方局的标识/分层**：用 GAME_SEED 分组、做分布对齐/配对
     统计（harness 已可把 seed 作为元数据保存）；
  2. 官方事件流已给出真实**首个攻击者**（本 trace 是 player），
     如需精确对齐先手，应由 diff-harness 从 `events[0]` 提取并让引擎
     以「官方先手」模式跑（当前引擎 `_determine_first` 仅按存活数 +
     rng，无外部先手注入 API；需另开引擎 hook，属新改动，建议先评估
     收益）。
- 结论：**GAME_SEED 只能做分布/分组对齐，不能做事件级 RNG 等价**；
  不要为逐事件复现去改引擎 RNG。

## 6. 总结

| 问题 | 裁定 |
|------|------|
| 904 来源 | 战后战团还原副本（SETASIDE→PLAY，COPIED_FROM 351，BACON_DIED_LAST_COMBAT=1） |
| Reborn/饰品/技能 | 否；351/904 无 REBORN；无相关触发 |
| official_result=win | **harness 误算**（把 904 还原当战斗内 summon → 存活数 1:0） |
| 引擎 64 seeds tie 100% | **正确**；双方随从全灭且英雄 0 伤害 = 平局 |
| 分类 | **harness 抓取/归因遗漏**，非引擎 bug |
| 动作 | diff-harness 过滤战后还原事件 + 守护测试；重抽取后该 trace 应为 MATCH |
| GAME_SEED | 不能复现官方 RNG/先手；仅可用于分组/分布对齐 |
