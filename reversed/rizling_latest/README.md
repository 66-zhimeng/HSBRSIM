# reversed/rizling_latest/ — rizling 最新发行包静态分析产物

> 来源：`https://rizling.top/assistant/updates/latest.zip`（用户提供）
> 下载时间：2026-10-18T08:36Z（本地时间 16:36）
> 包版本：**AgenticAssistant 0.6.1**，built `20261018-160659`，
> 自报 `game_version = 36.6.3.253932`
>
> **未运行**包内 exe、未登录、未联网认证、未修改游戏目录；只做下载 + 解包 +
> 只读静态分析。完整报告见 [`docs/VERSION_SYNC.md`](../../docs/VERSION_SYNC.md)。

## 包本体校验

| 项 | 值 |
|----|----|
| zip 大小 | 53,852,237 B |
| zip sha256 | `B3C3FFEFCBD4A7B33E19B876A580844135C2AF120CE5E486290F4505EFA3BC1F` |
| zip 条目数 | 2083（含目录） |
| 整包是否保留 | **否**（54MB，避免误提交；需要时按上面的 URL 重新下载并校验 sha256） |
| 关键文件 vs manifest 哈希 | `AgenticAssistant.exe` / `AgenticUpdater.exe` / 说明 / cmd / catalog / game_tags 均**一致** |

## 文件索引

| 文件 | 内容 |
|------|------|
| `PACKAGE_INFO.json` | zip 大小/sha256/URL + 关键文件逐个 sha256 + 保留策略 |
| `zip_listing.json` | zip 内 2083 个条目的路径/压缩前后大小 |
| `release-manifest.json` | 包内 manifest 原文件（版本/built/game_version/模块/2031 个文件哈希） |
| `使用说明.txt` | 0.6.1 用户说明（注意：新版不再写"当前规则版本"行） |
| `战斗预测说明.txt` | 明确"所有战斗模拟在服务器执行"，客户端只显示概率/伤害统计 |
| `启动离线预览.cmd` | 离线 UI 预览入口（说明明确它不读游戏/不连后台） |
| `data/catalog.json` | **0.6.1 公共卡牌目录**（3983 卡；新带 atk/hp/armor/cost/text/pool 标记） |
| `data/game_tags.json` | rizling 的 GameTag/CardType/Race/Zone 枚举（与 0.5.3 **完全相同**） |
| `bgassistant/data/catalog.json` 等 | 解包后的原始相对路径副本（便于对照 manifest / 代码里的路径） |
| `STATIC_ANALYSIS.json` | 新旧 exe 的 PyInstaller PYZ 只读分析：模块增删、字节级一致模块、禁止模块缺失、版本常量扫描、hotkeys/artwork 证据 |
| `diff_061_vs_053.json` | 0.5.3 ↔ 0.6.1 包差异（版本/模块/目录/枚举/字段） |
| `diff_vs_project.json` | 0.6.1 catalog ↔ 本项目 `data/`（37.0.0.255130）对齐统计 |

## 一句话结论

0.6.1 是 **UI/悬浮窗/卡图/本地展示** 大版本：新增 9 个客户端模块（desk/widgets/
overlay/hotkeys/artwork/hud/tracker/…），catalog 从"公共元数据"升级为**带数值/
文本/卡池标记**的目录；但 **Power.log 解析器、GameTag 映射、内存读取、protocol
DTO 等核心模块与 0.5.3 字节级完全一致**，自报规则版本仍是 **36.6.3.253932**，
没有 37.0.0/255130 的解析或规则更新，也仍然**没有本地模拟器/模型权重/自动点击**。
