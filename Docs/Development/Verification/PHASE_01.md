# Phase 01 验收记录

Unity 版本：6000.3.24f1

Git 提交/工作区标识：未提交工作区，Phase 01 自动化实施完成

测试日期与设备：2026-09-11，Windows 本地开发机

执行的 EditMode 测试：8/8 通过，0 失败，0 跳过；覆盖配置默认值、非法配置、2:1 视口和主场景触控基线；结果位于 `Logs/EditModeResults.xml`

执行的 PlayMode 测试：1/1 通过，0 失败，0 跳过；结果位于 `Logs/PlayModeResults.xml`

通过的手工场景：自动检查确认 SCN_Boot、SCN_Main 已进入 Build Settings；SCN_Main 使用 `InputSystemUIInputModule`、2:1 ContentRoot 和可见退出按钮。已检查 Development Build 的 1920×960 与 1920×1080 离屏渲染图：1920×960 铺满，1920×1080 上下 letterbox 正确，内容未拉伸或遮挡。

Windows 构建路径与结果：`Builds/Windows/Development/GroundChickenKing.exe`，Unity BuildPipeline 成功；完整构建 159,673,476 bytes

红外触控设备/驱动：当前环境未连接目标红外 HID 设备，待 Phase 08 实机验证

已知限制：尚未进行 180×90 cm 投影映射、实体按钮尺寸和并发红外触控测试；这些项目不能由无界面自动化代替，按计划留到 Phase 08。Phase 01 仅建立技术基线和可见退出按钮，不包含玩家加入、下注与比赛流程。

证据（截图、日志、种子）：`Logs/EditModeResults.xml`、`Logs/PlayModeResults.xml`、`Logs/Layout-1920x960.png`、`Logs/Layout-1920x1080.png`、项目外构建日志 `GroundChickenKing-build.log`
