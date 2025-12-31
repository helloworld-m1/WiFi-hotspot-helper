# WiFi 热点助手 / WiFi Hotspot Helper

此解决方案用于 Windows 系统上创建与管理 WiFi 热点，并支持根据“绑定的上行网络适配器状态”自动启停热点。

This project helps create and manage a Wi‑Fi hotspot on Windows 10/11 and can automatically start/stop the hotspot based on the state of a bound uplink network adapter.

频段需要在 2.4GHz 或 5GHz 之间切换时，请使用 Windows 自带的“移动热点”功能（本程序不支持频段切换）。

When switching between 2.4GHz or 5GHz frequency bands, please use the Windows built-in "Mobile Hotspot" function (this program does not support frequency band switching).

## 功能 / Features
- 配置热点名称与密码 / Configure hotspot SSID and password
- 扫描并绑定网络适配器（保存 GUID） / Scan and bind a network adapter (persist GUID)
- 自动管理热点（定时检查）/ Auto-manage hotspot (periodic check)
- 开机自启（当前用户）/ Autostart (current user)
- 热点状态检测 / Hotspot status detection
- 热点自动恢复：当“应开启”但被系统/其他方式关闭时尝试重新开启 / Auto-recovery: if hotspot should be ON but was turned OFF externally, try to turn it back ON

## 自动管理判定逻辑 / Auto-manage decision logic
自动管理开启时（定时器触发），程序会以“绑定适配器是否真的联网/可路由”为依据决定是否开启热点。

When Auto Manage is enabled (timer tick), the app decides whether to start the hotspot based on whether the bound adapter is truly connected/routable.

判定条件（必须同时满足）/ Conditions (all required):
- 适配器状态为 `Up` / Adapter `OperationalStatus == Up`
- 存在非 APIPA 的 IPv4（非 `169.254.*`）/ Has a non‑APIPA IPv4 (not `169.254.*`)
- 存在 IPv4 默认网关（更符合“可路由/已联网”）/ Has an IPv4 default gateway (more likely routable/online)

当条件满足时：尝试启动热点；否则：尝试停止热点。

If conditions are met: try to start hotspot; otherwise: try to stop hotspot.

## 执行模式 / Execution modes
本项目支持两种执行模式（由界面单选控制并写入配置）:

This project supports two execution modes (selected via UI radio buttons and persisted in config):

1) `netsh`（Hosted Network）
- 使用 `netsh wlan set/start/stop hostednetwork`
- 兼容性：部分 Windows 10/11 + 新驱动可能不再支持 Hosted Network

2) `mobile`（Windows “移动热点” WinRT）
- 通过 PowerShell 调用 WinRT `NetworkOperatorTetheringManager`

## 无密码热点 / Open hotspot (no password)
- `mobile` 模式：当密码为空时，程序不会设置 `Passphrase`（是否能真正创建无密码热点取决于系统政策/版本）。
- `netsh` 模式：Hosted Network 需要 WPA2‑PSK，**不支持空密码**，必须 8–63 位。

- `mobile` mode: if password is empty, the app will not set `Passphrase` (whether an open hotspot is allowed depends on OS policy/version).
- `netsh` mode: Hosted Network requires WPA2‑PSK, **empty password is not supported**, must be 8–63 chars.

## 热点状态检测与自动恢复 / Hotspot status & auto-recovery
- 状态检测 / Status detection
  - `netsh`：`netsh wlan show hostednetwork` 解析 Started/Stopped
  - `mobile`：读取 `TetheringOperationalState`（0=Off, 1=On, 2=InTransition, 3=Unknown）

- 自动恢复 / Auto-recovery
  - 自动管理开启且“应开启(desired=true)”时，会定期检查热点实际状态（当前约每 10 秒一次）
  - 如果检测到热点为关闭状态，会尝试重新开启（例如系统因无设备连接而自动关闭）

## 运行环境 / Requirements
- Windows 10 / Windows 11
- .NET Framework 4.7.2

## 配置文件 / Configuration file
配置文件保存路径 / Location:
- `%AppData%\WiFi hotspot helper\config.ini`

主要字段（示例）/ Main keys (example):
- `HotspotName=...`
- `HotspotPassword=...`（可为空 / can be empty）
- `AutoManage=1|0`
- `BoundAdapterId=...`（GUID）
- `ExecMode=netsh|mobile`

## 使用说明 / Usage
1. 点击“刷新”扫描网络适配器列表 / Click `Refresh` to scan adapters
2. 选择需要绑定的适配器并点击“绑定” / Select an adapter and click `Bind`
3. 勾选“自动管理” / Enable `Auto Manage`
   - 程序会周期性判断绑定适配器是否满足“Up + IPv4 + 默认网关” / The app periodically checks: `Up + IPv4 + default gateway`
   - 满足则尝试启动热点，否则停止 / Start if satisfied, otherwise stop
4. 勾选“开机运行”写入当前用户启动项 / Enable `Run at startup` to add current-user autorun entry
