# Phase 05 验收记录

Unity 版本：6000.3.24f1

Git 提交/工作区标识：未提交工作区，Phase 05 实施完成

测试日期与设备：2026-09-13，Windows 11 本地开发机，NVIDIA GeForce RTX 3060 Laptop GPU

执行的 EditMode 测试：44/44 通过，0 失败，0 跳过；Phase 01–04 规则与确定性比赛核心全部回归通过

执行的 PlayMode 测试：11/11 通过，0 失败，0 跳过；覆盖六类计划行为进入/退出、无 Animator 降级、取消/重置/强制安全完成、大小帧步长终点一致、单实例连续播放 100 局无残留、相邻赛道成对干扰，以及真实场景五鸡同时开始比赛

通过的手工场景：Windows Development Build 通过可见 UI Button 自动完成两名玩家加入、下注、全员锁定、3/2/1/GO 并进入比赛。五只外形和颜色不同的小鸡在五条独立赛道从左向右运动；1920×960 完整可见，1920×1080 保持 2:1 内容并正确上下留黑。干扰事件只驱动相邻赛道目标的成对表现，不使用碰撞且不修改逻辑进度

Windows 构建路径与结果：`Builds/Windows/Development/GroundChickenKing.exe`，Unity BuildPipeline 成功；完整构建 159,916,112 bytes。Development Build 性能探针在倒计时结束后的五鸡比赛中采样 300 帧：主线程平均 0.096 ms、最大 0.174 ms、GC 总分配 0 bytes、单帧最大 0 bytes

红外触控设备/驱动：当前环境未连接目标红外 HID 设备；比赛前流程仍全部使用 Unity UI `Button`，实体多点并发、边缘触控和投影映射留待 Phase 08

已知限制：当前五只鸡使用可替换的程序化 UI 造型和安全空动画片段，11 个 Animator 状态、Prefab 与配置接口已经建立；后续可直接替换美术动画资源。Phase 05 不实现派彩、咖喱鸡排饭、冠军留场或败者换新，这些属于 Phase 06。性能数据来自开发机离屏样本，不替代目标展项电脑的 60 分钟压力测试

证据（截图、日志、种子）：`Logs/EditModeResults.xml`、`Logs/PlayModeResults.xml`、`Logs/Phase05-Racing-1920x960.png`、`Logs/Phase05-Racing-1920x1080.png`、项目外 `GroundChickenKing-phase05-setup.log`、`GroundChickenKing-editmode-phase05.log`、`GroundChickenKing-playmode-phase05.log`、`GroundChickenKing-phase05-config.log`、`GroundChickenKing-phase05-build.log`、`GroundChickenKing-phase05-performance.log`
