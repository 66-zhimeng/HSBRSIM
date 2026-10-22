# Power.log 语料统计（本机 HDT 录像）

> 自动生成，勿手改。复现：`python reversed/scratch/replays.py`

- 录像目录（只读）: `%USERPROFILE%\AppData\Roaming\HearthstoneDeckTracker\Replays`
- 录像总数: **1365**；按玩家数分布: `{2: 1365}`（2 = 构筑/双人模式，8 = 酒馆战棋）
- **严格判定的酒馆战棋对局: 0 个**

## 1. 格式（对差分 harness 有效）

`.hdtreplay` 是 zip，内含 `output_log.txt`，即完整 Power.log 的子集：

```text
D 17:47:38.9236160 GameState.DebugPrintPower() - CREATE_GAME
D 17:47:38.9236160 GameState.DebugPrintPower() -     GameEntity EntityID=1
D 17:47:38.9236160 GameState.DebugPrintPower() -         tag=CARDTYPE value=GAME
```

## 2. 语料统计

| 指标 | 全部录像合计 |
|------|--------------|
| 文件数 | 1365 |
| 平均行数 / 大小 | 9,759 行 / 1,302,593 B |
| GameState.* 行 | 13,319,287 |
| PowerTaskList.* 行（HDT 录像里为 0；实时日志里会重复，解析时应跳过） | 0 |
| CREATE_GAME | 1,365 |
| FULL_ENTITY | 221,254 |
| TAG_CHANGE | 5,608,553 |
| BLOCK_START | 491,411 |
| BlockType=ATTACK | 31,323 |

## 3. 判定口径与结论

- 严格判定：`CREATE_GAME` 之后出现 **≥8 个 Player 实体** 才是酒馆战棋；
- 本机 1365 个录像的玩家数分布为 `{2: 1365}`，**没有 8 人局 → 没有可用的 BG 轨迹**；
- 诊断信号：含 `TB_BaconShop` 的文件 0 个、含 `tag=BACON_*` 的文件 103 个、含 `CardID=BG<数字>` 的文件 2 个——这些是**构筑局里引用的 BG 实体/Bob 皮肤**，不能当 BG 语料；
- 历史上 `REAL_GAME_BRIDGE.md` 记录的 `TB_BaconShop_8P_PlayerE` 战斗结束标志在本机录像中从未出现（0 次），**必须在首份真实 BG 日志上重新验证**；
- 结论：差分 harness 的 BG 语料需要**新采集**（`log.config` 的 `[Power]` 已开启，正常打一局即可产生 `Logs\Power.log`）；见 `docs/OFFICIAL_EXACTNESS.md` §2。
