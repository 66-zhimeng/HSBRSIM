# reversed/ — 官方一致性逆向数据与产物

> 归属：`rea-analyst`（M0.5 官方一致性）。**只读**游戏目录与 `data/`；
> 所有提取物、差分报告、试跑数据都写在这里。对外结论汇总见
> [`docs/OFFICIAL_EXACTNESS.md`](../docs/OFFICIAL_EXACTNESS.md)。

## 目录

| 路径 | 内容 | 生成命令 |
|------|------|----------|
| `hsdata_255130/` | 官方 build 255130 数据：`CardDefs.xml`（构筑）、`CardDefs.Bacon.xml`（**酒馆战棋**）、`RaceTagMap.xml`、`README.md`、每文件 `.sha256`、`source.json`（URL/大小/哈希/时间） | `python tools/rea_fetch_hsdata.py --tag 255130` |
| `data_diff_255130_vs_249896.{md,json}` | 官方 255130 vs 本地 249896 差分：+179/-4 卡、384 处数值变化、16 个新 tag | `python tools/rea_data_diff.py` |
| `data_255130/` | 用 255130 试跑 `generate_bg_data.py` 的产物（12 个 JSON，**不覆盖 `data/`**） | `python tools/rea_generate_probe.py` |
| `data_255130_input/CardDefs.merged.xml` | `CardDefs.xml + CardDefs.Bacon.xml` 合并文件（现有生成器只吃单文件） | 同上 |
| `data_255130_generator_report.{md,json}` | 生成器消费性 + `CardDB.load` 验证 + 与当前 `data/` 对比 | 同上 |
| `net/` | 官方/社区程序集元数据提取（Mono.Cecil）：`Assembly-CSharp`、`blizzard.bgssdk`、`blizzard.bgsclient`、`HearthDb`、`BobsBuddy`、`HearthWatcher`（types/methods/enums/strings/refs）+ `SUMMARY.md` + `key_enums.json` | `python tools/rea_net_extract.py` |
| `net/decompiled/` | 关键类型反编译源码（`EntityDef`/`GameDbf`/`GameState`/`PowerTaskList`/`TB_BaconShop` 等），仅本地规则核对 | `ilspycmd -t <Type> Assembly-CSharp.dll` |
| `net_lists/` | `ilspycmd -l` 全量类型清单 | `python tools/rea_net_extract.py` |
| `net_extractor/` | C# 提取器源码（Mono.Cecil 直接引用本机 BepInEx 的 DLL） | `dotnet build -c Release` |
| `tag_diff.{md,json}` | 官方 `GAME_TAG`(1214) vs `hsrl2/tags.py`(126)：11 处同名数值错、1181 缺失、93 多余、36 撞号 | `python tools/rea_tag_compare.py` |
| `engine_impact_255130.json` | 数据升级命中 146 个引擎文件 / 71 个测试文件 | `python reversed/scratch/impact.py` |
| `powerlog_corpus.{md,json}` | 本机 HDT 录像统计（1,365 个，全为 2 人构筑局，0 个 BG） | `python reversed/scratch/replays.py` |
| `scratch/` | 一次性分析脚本（枚举检查/种族/影响面/录像统计），可复现上述 json | 各自运行 |

## 约束

- 游戏目录 `C:\Program Files (x86)\Hearthstone` **只读**（未写入/未改 BepInEx/未运行）；
- 反编译产物仅供本项目内部规则核对，不二次分发；
- 官方 CardDefs 数据来自 HearthSim/hsdata 对应 build tag（由官方客户端 DBF 转储），
  哈希已锁定，见 `hsdata_255130/source.json`；
- 原始 Power.log/录像含玩家信息，**不落盘**到本目录（只保存聚合统计）。
