# Phase 10 验收记录

Unity 版本：6000.3.24f1（Unity 6.3 LTS）

测试日期：2026-09-16

## 自动化

- Phase 10 场景装配完成，`SCN_Main` 已包含共享 3D 小鸡舞台、下注热身画面、动态领奖台、四份鸡排饭和全屏按钮。
- EditMode：基础版本 64/64 通过；移动修复后 65/65 通过，报告 `Logs/Phase10-MovementFix-EditMode-Final.xml`。
- PlayMode：16/16 通过，报告 `Logs/Phase10-PlayModeResults-Final.xml`。
- Windows x64 Development Build 成功：`Builds/Windows/Development/GroundChickenKing.exe`，移动修复版完整构建 189,364,864 bytes。
- 最终构建日志：`Logs/phase10-movement-fix-build-final-2.log`，返回码 0。

## 视觉验收

- `Builds/Visual/Phase10/Warmup-3D-Final-1920x960.png`：五鸡在五条赛道动态热身。
- `Builds/Visual/Phase10/Betting-LiveRoster-Final-1920x960.png`：下注期间保持上场阵容和赛道可见。
- `Builds/Visual/Phase10/Race-3D-Final-1920x960.png`：3D 小鸡按 RaceDirector 剧本比赛。
- `Builds/Visual/Phase10/Settlement-Podium-Final-1920x960.png`：冠军领奖台、动态彩纸和四份败者鸡排饭。
- `Builds/Visual/Phase10/Settings-Fullscreen-Final-1920x960.png`：全屏/窗口切换按钮可见。

## 实机项目

- 红外 HID 多点触控仍按 ADR-0002 在目标设备复验。
- 全屏投影需在最终投影机 EDID 与 Windows 缩放配置下复验。

## 2026-09-16 赛道移动修复

- 原因：`Chicken3DStage` 的控制器数组只在编辑器装配期间赋值且未序列化，Windows 运行时数组为空；3D 模型因此只能显示热身姿态，无法读取 `NormalizedProgress`。
- 修复：`ChickenRacePresenter.Awake` 在运行时把场景中的五个 `ChickenController` 重新绑定到 3D 舞台；`Configure` 和舞台替换时也会重新同步。
- 回归测试：新增 `ChickenRacePresenter_Awake_RebindsFiveControllersToImmersiveStage`，要求绑定数量严格等于 5。
- 最终测试：EditMode 65/65、PlayMode 16/16；无图形测试环境会安全跳过 RenderTexture 创建，Windows 图形环境正常启用舞台。
- 视觉验收：`Builds/Visual/Phase10/Race-Movement-Fixed-1920x960.png` 应显示五只鸡位于不同的横向比赛进度，而非全部停在起点。
- Phase 10 程序化模型仍是技术占位；正式精品建模与动画生产标准见 `Docs/Development/PHASE_11_PRODUCTION_CHARACTERS.md`。
