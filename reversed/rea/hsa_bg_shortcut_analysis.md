# 炉石传说盒子（HSA）酒馆战棋快捷操作逆向记录

> 目标：判断 HSA 的 BG 快捷键能否提供"非 UI / 更安全"的操作通道。
> 结论先行：**当前证据显示 HSA 的 BG 快捷键仍是 UI 输入自动化（overlay 拦截按键 + 向游戏窗口注入输入），未发现游戏公开 API / 内存写操作通道。**

## 1. 环境

- 安装目录：`D:\炉石传说盒子`
- 进程：`HSAng.exe`（Qt/CEF 主程序）、`HSAExternal.exe`（注入 broker）、`agent.exe`、`heyboxacc.exe`
- 战棋对局中注入到 `Hearthstone.exe` 的模块（实测 `Get-Process -Module`）：
  - `D:\炉石传说盒子\heart_overlay_x64.dll`
  - `D:\炉石传说盒子\NeteaseHSRecord.x64.dll`

## 2. 配置中的 BG 快捷键

`%APPDATA%\HSAng\hsa.config` 实测：

| 配置项 | 值 | 含义 |
|---|---|---|
| `BuyBGShortcut` | `W` | 购买 |
| `FreezeBGShortcut` | `S` | 冻结 |
| `RefreshBGShortcut` | `D` | 刷新 |
| `SaleBGShortcut` | `E` | 出售 |
| `TransferBGShortcut` | `C` | 移动/交换 |
| `UpgradeBGShortcut` | `F` | 升级 |
| `EnableBGShortcut` | `true` | 启用快捷操作 |

## 3. 静态分析（导入表）

### `NeteaseHSRecord.x64.dll`
- KERNEL32：`ReadFile`/`WriteFile`/`CreateNamedPipeW`/`VirtualAlloc`/`VirtualProtect`/`SuspendThread`/`ResumeThread`/`GetThreadContext` 等；
- USER32：仅 `GetProcessWindowStation`、`GetUserObjectInformationW`；
- **没有** `PostMessage` / `SendInput` / `mouse_event` / `keybd_event` / `WriteProcessMemory`（其他进程）等输入/注入 API。
- 判断：记录/读取游戏状态（Power/日志/内存扫描）为主，不是操作执行器。

### `heart_overlay_x64.dll`
- USER32 导入包含：`PostMessageW`、`SendMessageW`、`GetAsyncKeyState`、`SetWindowsHookExW`、`SetCursorPos`、`GetRawInputData`、`EnumWindows` 等；
- 反汇编中 `PostMessageW` 调用点主要传 `0x8203/0x8201`（overlay/CEF 窗口消息）与 `WM_CLOSE(0x10)`，属于 overlay 自身窗口/CEF 消息循环，未看到针对 Hearthstone 窗口的买/卖/刷新消息序列；
- 该 DLL 还包含 `HotkeyInfo`、`InGameHotkeyDown`、`overlay.hotkey.toggleInputIntercept` 等符号，说明它负责**全局热键拦截 + overlay 输入**。
- 判断：HSA 的快捷操作是 **overlay 捕获热键 → HSA 侧合成鼠标/键盘输入到游戏窗口**，并非调用游戏公开 API 或写内存动作。

### `HSAng.exe` / `HSAExternal.exe`
- HSAExternal 是 `HSAExternalBroker` + 命名管道 + 注入（`dllName/processId/clientName`），把 `heart_overlay`/`NeteaseHSRecord` 注入游戏；
- HSAng 主体未见直接 `SendInput`/`mouse_event` 静态导入（Qt/CEF 动态加载），BG 逻辑主要在 native/加密资源中，未找到可复用的动作 API。

## 4. 与"安全内存操作"的关系

- 用户期望：只读内存拿状态 + 直接操作游戏，不靠 UI。
- 实测 HSA **没有提供这样的通道**；它只是把 UI 点击做得更快/更稳（overlay + 热键 + 输入注入）。
- 因此：
  1. 逆向 HSA 不能得到协议/内存动作接口；
  2. 使用 HSA 热键仍属于 UI 自动化，封号风险与我们自己的 UI bot 同类；
  3. 若目标是"无 UI"，仍然只有进程内公开 API bridge（BepInEx 类）或协议注入，二者都需要改客户端/注入。

## 5. 可选后续

1. **复用 HSA 热键作为"执行器"**（不自己拖拽）：bot 只负责决策 + 把鼠标移到目标卡，按 W/E/D/F/S；需 1 局验证命中率（仍有 UI/封号风险）。
2. **动态逆向 overlay 的输入注入**：x64dbg/API Monitor 断 `PostMessageW`/`SetCursorPos`/`GetRawInputData`，确认 HSA 向 Hearthstone 发送的确切消息/坐标；若可复现，可替代我们现有的 UI 拖拽（仍属输入自动化）。
3. **不再使用第三方盒子**，继续用我们自己的只读内存 + HSA 风格消息/输入；或回到 BepInEx 公开 API bridge（无 UI，但需注入）。

## 6. 证据等级

- 配置快捷键、注入模块、导入表：**实测/静态证据**；
- "HSA 用 UI 输入而非 API/内存写动作"：**基于导入表与反汇编调用点的推断**，未做动态断点验证；
- 未在真实对局中测试 HSA 热键（避免进一步账号风险）。
