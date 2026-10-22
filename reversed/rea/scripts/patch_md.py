p=r"reversed\rea\official_implementation_audit.md"
s=open(p,encoding="utf-8").read()
s=s.replace("| B. **BobsBuddy .NET 桥接**（第三方完整战斗实现） | **可行** | **1-3 天** | 本机既有 `dotnet SDK 9.0.101`，且有 `BobsBuddy.dll`（1.28MB, 1817 类型/5991 方法）、`BobsBuddy.Common.dll`、`HearthDb.dll`（HDT app-1.53.0）。",
            "| B. **BobsBuddy .NET 桥接**（第三方完整战斗实现） | **可行** | **1-3 天** | 本机既有 `dotnet SDK 9.0.101`，且 HDT app-1.53.0 内有 `BobsBuddy.dll`（1,282,560 B / sha256 `26dd65ca3db6eb54…` / 1817 类型 5991 方法）、`BobsBuddy.Common.dll`（4,096 B）、`HearthDb.dll`（36,744,704 B，BobsBuddy 的卡数据库依赖）。")
s=s.replace("- `reversed/rea/official_implementation_audit.json`（56KB；",
            "- `reversed/rea/official_implementation_audit.json`（57KB；")
open(p,"w",encoding="utf-8").write(s)
print("patched md")
