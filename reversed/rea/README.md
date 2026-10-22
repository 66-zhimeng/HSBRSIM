# reversed/rea — REA 逆向工作区索引

> rea-operator 产出（2026-10-18）。**只读目标**：游戏目录 `C:\Program Files (x86)\Hearthstone`、
> HDT 目录 `%USERPROFILE%\AppData\Local\HearthstoneDeckTracker`。本目录可写。

## 快速使用

```powershell
# 1) 便携 Node 24（不污染系统 Node v22.15）
reversed\rea\node\node.exe --version          # -> v24.13.0

# 2) REA CLI（本地安装，6.7.0；不做 rea setup，避免写 agent 配置）
$env:PATH = "$PWD\reversed\rea\node;$env:PATH"
reversed\rea\rea-install\node_modules\.bin\rea.cmd --help
reversed\rea\rea-install\node_modules\.bin\rea.cmd doctor

# 3) 通用分析脚本（给定 dll -> REA -> JSON）
pwsh tools\rea_analyze_dotnet.ps1 -DllPath <dll> -OutDir <dir> -Types <Type1>,<Type2>
```

## 目录

| 路径 | 说明 |
|------|------|
| `node/` | 便携 Node v24.13.0 + npm 11.6.2（REA 要求 ^22.19\|\|^24.11\|\|>=26；系统 Node 是 v22.15 不满足） |
| `rea-install/` | `npm install rea-agents@latest` 的本地安装（6.7.0），避免每次 npx 解析；**无 MCP 注册副作用** |
| `npm-cache/` | 独立 npm 缓存（不写用户全局缓存） |
| `bobsbuddy/` | **BobsBuddy 战斗模拟器逆向产物**（`README.md` 算法要点、`src_full/` 全量反编译、REA 证据 JSON） |
| `bobsbuddy_vs_hsrl2.md` | **BobsBuddy vs hsrl2 战斗引擎逐条对比**（按严重度；P0/P1/P2） |
| `client_constants.json` | 客户端可见的战斗/规则常量与枚举（每项附证据；含"客户端无战斗模拟器"与"无费用/概率表"的负证据） |
| `client/` | Assembly-CSharp 定点产物：`Assets_Global.cs`（BaconCombatStep / BoardVisualState 枚举） |
| `client_official/` | `tools/rea_analyze_dotnet.ps1` 对 Assembly-CSharp 的实跑结果（artifact OK / members 不可用：resource-limit） |
| `hdt_bobsbuddy/` | HDT 宿主插件反编译：`BobsBuddyInvoker.cs`（输入构造 + DamageCap 来源 + 迭代数）、`BobsBuddyUtils.cs` |
| `hsmod/` | HsMod.dll 产物：REA artifact、类型清单、`src/` 全量反编译、关键类片段 |
| `scripts/` | 本次用到的小工具（tag 查询、HsMod 能力提取、summary 抽取） |
| `rea_help_raw.txt` / `rea_capabilities.txt` | REA `--help` / `capabilities` 原始输出（可用性证据） |

## 本次结论（TL;DR）

1. **REA 可用**：Node 24 便携版 + `rea-agents 6.7.0`。托管静态 provider `rea-dotnet-static v1`
   对 BobsBuddy.dll（1.28MB）与 HsMod.dll（746KB）**完整可用**（方法/字段/调用边/字段访问/ldstr 字符串）。
2. **REA 的限制**：
   - 不做 C# 反编译（provider 自述 "never … decompiles C#"）→ 方法体用 ilspycmd 补；
   - 对 Assembly-CSharp.dll（23MB）`inspect-managed-members` 因 **metadata 表示超 64MiB 预算** 不可用
     （`resource-limit`）；`inspect-managed-artifact` 仍完整。类型级结论沿用 `reversed/net/` 的 ilspycmd 清单。
   - 原生引擎（Hopper/Ghidra/IDA）全缺 —— 对本项目 .NET 目标无影响。
3. **BobsBuddy 关键算法**已逐段取证（见 `bobsbuddy/README.md`），与 hsrl2 存在 6 个 P0 级分歧
   （复生属性、猛毒/剧毒时机、顺劈同组、伤害上限来源、死亡波次批处理、SoC 顺序）。
4. **伤害上限不是公式**：BobsBuddy 从服务端 tag `BACON_COMBAT_DAMAGE_CAP`(2089) 读，由 3403 门控
   （`hdt_bobsbuddy/BobsBuddyInvoker.cs:652-655`）。这是本项目 bridge 需要抓取的数据。
5. **客户端无战斗模拟器**、**客户端无费用/刷新概率表**（DBF 只有外观表），与既有 `docs/OFFICIAL_EXACTNESS.md` 结论一致。
6. **HsMod 无战棋自动化/模拟**：只有加速战斗表现、隐藏 Bob 台词、对手手牌数显示等 QoL。
