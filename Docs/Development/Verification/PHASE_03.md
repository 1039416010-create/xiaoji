# Phase 03 验收记录

Unity 版本：6000.3.24f1

Git 提交/工作区标识：未提交工作区，Phase 03 实施完成

测试日期与设备：2026-09-11，Windows 本地开发机

执行的 EditMode 测试：35/35 通过，0 失败；覆盖合法下注、非法金额 0/2/25、非法鸡 ID、余额边界、缺少选择、重复确认、锁定后修改、1–4 人全员锁定、破产旁观者、清理未锁定下注，以及 Phase 01/02 回归

执行的 PlayMode 测试：6/6 通过，0 失败；覆盖玩家下注互不串位、双击确认只扣一次、全员通过真实 `Button.onClick` 锁定后进入 `RaceCountdown`，并断言 Phase 03 尚不存在 `RaceDirector`

通过的手工场景：`SCN_Main` 已由 Phase 03 场景构建器生成四个独立下注面板；上侧玩家 3/4 面板旋转 180°，下侧玩家 1/2 正向；每个面板包含 5 个选鸡按钮、1/5/10 金币按钮和“确认下注”按钮；全员锁定后显示倒计时准备页，不生成或显示冠军

Windows 构建路径与结果：目标为 `Builds/Windows/Development/GroundChickenKing.exe`。本轮刷新构建时 Unity Licensing Client 持续拒绝本机 IPC 通道并循环重连，构建方法未开始执行；现有可执行文件仍是 Phase 02 产物，不作为 Phase 03 构建通过证据。恢复 Unity 许可证服务后必须补跑 Development Build 与离屏截图

红外触控设备/驱动：当前环境未连接目标红外 HID 设备；下注交互仅通过 Unity UI `Button`，实体多点并发、边缘触控与误触测试留待 Phase 08

已知限制：本阶段按规范停在 `RaceCountdown`，没有比赛结果、比赛脚本、结算或换鸡逻辑。Windows 构建及 1920×960/1920×1080 离屏画面因许可证服务异常待补验；在补验前不得把 Phase 03 标为完整发布候选

证据（截图、日志、种子）：`Logs/EditModeResults.xml`、`Logs/PlayModeResults.xml`、项目外 `GroundChickenKing-phase03.log`（场景构建成功）、`GroundChickenKing-playmode.log`（PlayMode 通过）、`GroundChickenKing-phase03-build-retry.log` 与 `GroundChickenKing-phase03-build-gui.log`（许可证 IPC 故障）；本阶段尚未生成比赛种子
