# Phase 07 验收记录

Unity 版本：6000.3.24f1

Git 提交/工作区标识：未提交工作区，Phase 07 实施完成

测试日期与设备：2026-09-13，Windows 11 本地开发机，NVIDIA GeForce RTX 3060 Laptop GPU

执行的 EditMode 测试：55/55 通过，0 失败，0 跳过。新增展项设置边界与草稿隔离测试；Phase 01–06 的状态机、经济、确定性比赛、结算和百轮循环全部回归通过。

执行的 PlayMode 测试：15/15 通过，0 失败，0 跳过。新增设置页与诊断页全 Button 开关、返回主菜单确认/取消、四玩家同帧独立点击和统一触控反馈检查；完整比赛回归额外验证冠军留场换鸡后，下一局下注按钮已同步新阵容 ID 与展示名。

完整 UI：主菜单的“设置”已启用并保留开始、设置、退出三个可见 Button。玩家加入、热身、下注、倒计时、比赛、结算、GameOver 均有安全返回路径。玩家 3/4 面板继续整体旋转 180°，四区使用颜色与 ◆/●/▲/■ 图形双编码。全部 Button 统一 Normal、Pressed、Disabled 颜色，禁用导航 Hover 依赖，并加入每按钮独立的 0.2 秒触控冷却与按压缩放反馈。

设置与本机保存：设置页仅使用 `＋/－/保存/取消` Button，可调整主音量、视觉强度和最大局数。值经过 `ExhibitSettings` 边界校验并保存至本机 PlayerPrefs JSON；不保存游客或个人数据。视觉强度使用不拦截 Raycast 的覆盖层，取消设置会恢复已保存值。

确认与双侧阅读：退出、返回主菜单和 GameOver 重开均经过确认弹窗。弹窗同时显示正向与旋转 180° 的操作说明，确认/取消按钮使用 ✓/✕ 图形表达，桌面两侧均可理解；取消不会改变会话。

音频：新增 `MIX_Exhibit` AudioMixer，含 Master、Music、SFX 组。开发占位音频以低强度程序音色提供循环背景、按钮、倒计时、GO、冲线与结算反馈；音量设置即时生效。占位音色接口可在不修改业务层的情况下替换为正式 `BGM_`/`SFX_` 资源。

诊断与运维：主菜单通过可见“设备诊断” Button 进入诊断页，显示版本、Unity 版本、实际分辨率、活动触点数、Windows HID/Input System 状态、最近比赛种子、最近错误和日志路径。`BoundedLogWriter` 将展项日志限制为 1 MiB 并保留一个轮转备份。120 秒无操作时仅在非倒计时、非比赛状态安全返回首页；比赛不会被空闲策略中断。应用恢复焦点时刷新全部 UI 到当前业务状态。

文字入口：运行时动态 UI 文案集中到 `UiTextCatalog`，作为后续语言包替换入口；领域对象不持有界面文案。

配置与静态检查：全部 ScriptableObject 配置通过验证。运行时代码扫描未发现 `KeyCode`、`Input.Get*`、`GetKey`、`Keyboard.current`、`Mouse.current`、`OnMouse*` 或 `UnityEngine.Random`。所有游客操作仍通过 Unity UI `Button.onClick`，诊断触点读取使用 Input System Enhanced Touch，不能触发业务命令。

Windows 构建路径与结果：`Builds/Windows/Development/GroundChickenKing.exe`，Windows x64 Development Build 成功，完整构建 160,043,680 bytes。1920×960 构建已分别打开设置页、诊断页和双向确认弹窗并完成截图验证。

红外触控设备/驱动：当前环境未连接目标 Windows HID 红外设备。四个玩家按钮的同帧独立命令测试通过，但真实 4/10 点并发触控、25 mm 物理按钮尺寸、桌面边缘映射和失焦恢复仍须在 Phase 08 目标设备实测。

已知限制：当前 BGM/SFX 为低音量程序化占位音色，尚未进行现场扬声器响度和环境噪声调校；正式美术、正式音频、Release Build、60 分钟压力测试和实体红外设备验收属于 Phase 08。Unity 自带空 Animator 的提示为既有占位角色表现，不影响状态、比赛或按钮交互。

证据：`Logs/Phase07-EditModeResults.xml`、`Logs/Phase07-PlayModeResults.xml`、`Logs/Phase07-Settings-1920x960.png`、`Logs/Phase07-Diagnostics-1920x960.png`、`Logs/Phase07-Confirmation-1920x960.png`，以及项目外 `GroundChickenKing-phase07-setup.log`、`GroundChickenKing-phase07-editmode.log`、`GroundChickenKing-phase07-playmode.log`、`GroundChickenKing-phase07-config.log`、`GroundChickenKing-phase07-build.log` 和三个 Phase 07 runtime 日志。
