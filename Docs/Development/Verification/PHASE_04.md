# Phase 04 验收记录

Unity 版本：6000.3.24f1

Git 提交/工作区标识：未提交工作区，Phase 04 实施完成

测试日期与设备：2026-09-13，Windows 11 本地开发机

执行的 EditMode 测试：44/44 通过，0 失败，0 跳过；新增覆盖锁定前拒绝生成、只生成一次、10,000 个种子逐一验证、同种子复现、下注数据隔离、生成重试与保底模板、3/2/1/GO 倒计时、播放故障硬超时、不同帧步长不改变冠军，并包含 Phase 01–03 全部回归

执行的 PlayMode 测试：6/6 通过，0 失败，0 跳过；真实场景通过 `Button.onClick` 完成两名玩家加入、下注和锁定，断言生成一份包含五只鸡且只有一个冠军的计划，结果在倒计时前为空，4 秒后进入 `Racing`

通过的手工场景：Windows Development Build 离屏运行成功。1920×960 下倒计时标题、显式比赛种子、数字 3 和返回主菜单按钮完整可见；1920×1080 下中央 2:1 内容保持比例，上下 letterbox 正确。生成器不接收下注快照，运行时未发现 `UnityEngine.Random`、键盘输入或 Root Motion 赛果依赖

Windows 构建路径与结果：`Builds/Windows/Development/GroundChickenKing.exe`，Unity BuildPipeline 成功；最终完整构建 159,844,919 bytes，运行时程序集于 2026-09-13 更新并通过离屏启动退出验证

红外触控设备/驱动：当前环境未连接目标红外 HID 设备；场景流程继续只使用 Unity UI `Button`，实体多点并发和边缘校准留待 Phase 08

已知限制：Phase 04 只播放逻辑倒计时与计划时钟，尚未实例化五只鸡、映射赛道世界坐标或播放角色动画；这些属于 Phase 05。进入 `Settlement` 后仅展示冠军提示，不执行派彩、换鸡或下一局，这些属于 Phase 06

证据（截图、日志、种子）：`Logs/EditModeResults.xml`、`Logs/PlayModeResults.xml`、`Logs/Phase04-Countdown-1920x960.png`、`Logs/Phase04-Countdown-1920x1080.png`、项目外 `GroundChickenKing-phase04-setup.log`、`GroundChickenKing-editmode-phase04.log`、`GroundChickenKing-playmode-phase04.log`、`GroundChickenKing-phase04-build.log`、`GroundChickenKing-phase04-config.log`；每次构建运行在倒计时页显示并记录实际比赛种子、配置哈希、冠军和计划 ID
