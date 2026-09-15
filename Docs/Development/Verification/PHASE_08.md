# Phase 08 软件发布准备与待现场验收记录

Unity 版本：6000.3.24f1（由 `ProjectVersion.txt` 固定）

包锁：`Packages/manifest.json` 与 `Packages/packages-lock.json` 已存在；Input System 1.20.0、URP 17.3.0、UGUI 2.0.0、Test Framework 1.6.0 的直接依赖已固定。

测试日期与设备：2026-09-15，Windows 11 本地开发机，NVIDIA GeForce RTX 3060 Laptop GPU；未连接目标投影机或 Windows HID 红外框。

执行的 EditMode 测试：56/56 通过，0 失败，0 跳过。包括 10,000 个确定性种子验证与新增四玩家 500 局完整逻辑 soak。500 局测试耗时 51 ms，强制回收后的托管内存增量 688,128 bytes；无卡局、负余额、重复派彩、错误冠军或无效阵容。

执行的 PlayMode 测试：16/16 通过，0 失败，0 跳过。新增诊断页进入五点校准、依次点击四角与中心、全部完成并安全返回的全 Button 测试；Phase 01–07 场景、下注、比赛、结算、换鸡、设置与确认流程全部回归通过。

现场工具：诊断页新增“五点校准”。校准目标位于 1920×960 安全区内，尺寸显著大于 25 mm 设计基准；Development Build 提供可见的“开启触点显示”按钮，可显示最多 10 个 Enhanced Touch 触点，Release 中该调试按钮禁用，但诊断与五点校准入口保留。

日志与恢复：展项日志维持 1 MiB 上限和一个轮转备份；新增目录不可写、磁盘写入失败和权限异常的无递归优雅降级，日志失败不会中断游戏。异常信息仍显示在诊断页。

Development 候选版：`Builds/Windows/Development/GroundChickenKing.exe`，Windows x64 Development Build 成功，完整构建 160,062,424 bytes，保留触点可视化、种子与详细日志。

Release 候选版：`Builds/Windows/Release/GroundChickenKing.exe`，Windows x64 非 Development Build 成功，完整构建 96,908,740 bytes。`boot.config` 不含 Development 标记；本机冷启动日志确认在第 2 帧完成 `Boot -> MainMenu`，未发现运行时异常。

归档：Release 与恢复用 rc1 归档均已生成。Release ZIP 为 36,624,628 bytes，SHA-256 为 `D9AF0217441AEA59A811B43B8EE08FB91D3E31799F1D6BDAD65F8CCA2943746D`。由于尚无早期正式现场版，当前回滚包是 rc1 的同内容恢复副本，不代表跨版本降级。

配置与静态检查：全部 ScriptableObject 配置有效；运行时代码未发现 `KeyCode`、`Input.Get*`、`GetKey`、`Keyboard.current`、`Mouse.current`、`OnMouse*` 或 `UnityEngine.Random`。Release 保持本地离线，无账号、购买或真实货币能力。

现场文档：已新增 `Docs/Operations/ON_SITE_MANUAL.md`、`HARDWARE_ACCEPTANCE.md`、`RELEASE_CHECKLIST.md` 和 `SOFTWARE_RELEASE_MANIFEST.md`，覆盖开关机、投影映射、五点/并发触控、日志、故障恢复、发布与回滚。

本机硬件探测：当前 Windows 会话只有 `DISPLAY1`，分辨率 2560×1440；Present HIDClass 设备数为 0，未发现投影机或红外触控框。因此不能在此主机上执行或伪造目标硬件验收。

尚未验证且不可由开发机代替：180×90 cm 投影映射；目标主机/投影机/红外框型号与驱动；Windows 显示映射；实体 4/10 点并发、边缘、快速重复和手掌误触；投影与 HID 断连重连；60 分钟目标设备运行；1–4 人各 20 次实体流程；从冷启动实玩到 Bankrupt 与 MaxRounds；现场签字。

豁免决定：2026-09-15，项目方明确指示实体硬件步骤可以跳过。该决定记录于 `ADR-0002-HARDWARE-ACCEPTANCE-WAIVER.md`。上述项目状态保持“未执行”，不改写为“通过”。

阶段结论：软件侧 RC、现场工具、文档、Development/Release 包均已齐备；Phase 08 按项目方硬件验收豁免决定收口。`0.1.0-rc1` 可继续用于演示和部署准备，但不附带实体投影或红外多点触控已验证的声明。

证据：`Logs/Phase08-EditModeResults.xml`、`Logs/Phase08-PlayModeResults.xml`、`Logs/Phase08-TouchCalibration-1920x960.png`、Release `RELEASE_INFO.txt`、`SHA256SUMS.txt`，以及项目外 Phase 08 setup/config/test/build/runtime 日志。
