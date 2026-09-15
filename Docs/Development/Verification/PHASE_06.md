# Phase 06 验收记录

Unity 版本：6000.3.24f1

Git 提交/工作区标识：未提交工作区，Phase 06 实施完成

测试日期与设备：2026-09-13，Windows 11 本地开发机，NVIDIA GeForce RTX 3060 Laptop GPU

执行的 EditMode 测试：53/53 通过，0 失败，0 跳过。新增覆盖押中返还 `2 × stake`、未押中不返还、多人同押、无人押中、同一比赛重复提交不重复派彩、冠军留场、四名败者全部替换、阵容唯一性、配置库少于 9 只时拒绝、单人破产、全员破产、最大局数，以及连续自动运行 100 局无负余额、重复派彩或错误阵容。

执行的 PlayMode 测试：12/12 通过，0 失败，0 跳过。新增真实 `SCN_Main` 全按钮流程测试：两名玩家加入并下注，比赛完成后进入结算，生成两名玩家明细；点击“下一局”后局号递增，局级下注、结算和 RacePlan 清空，金币与加入状态保留，冠军保持原赛道实例和稳定 ID，其他四只全部替换。

场景与 UI：新增“本局结算”和 GameOver 面板。结算页显示冠军、每名玩家本局输赢/返还/余额，以及适合公共展项的非血腥咖喱鸡排饭文案；提供“下一局”和“返回主菜单”按钮。GameOver 按结束原因显示最终金币排名，并提供“再来一局”和“返回主菜单”按钮。所有流程操作仍仅使用 Unity UI `Button`。

业务实现：`SettlementService` 以局号与 RacePlan ID 组成幂等键，所有余额变化经 `PlayerManager` 统一记账；`RosterService` 使用种子确定性选择替补，13 只鸡的配置库可在不复用本局败者的前提下补齐四只新鸡；`SessionEndEvaluator` 支持 `Bankrupt` 与 `MaxRounds` 两种结束原因。重开会把已加入玩家恢复到 50 金币并重置为第 1 局。

配置验证：全部 ScriptableObject 配置通过项目验证器。小鸡目录含 13 个唯一稳定 ID，满足至少 9 只的开局前约束；默认规则仍为初始金币 50、下注 1/5/10、最大 20 局。

Windows 构建路径与结果：`Builds/Windows/Development/GroundChickenKing.exe`，Windows x64 Development Build 成功，完整构建 159,991,096 bytes。构建后的程序通过按钮驱动完成一局并在 1920×960 截取结算画面。

输入与确定性检查：运行时代码静态扫描未发现 `KeyCode`、`Input.Get*`、`GetKey`、`Keyboard.current`、`Mouse.current`、`OnMouse*`、`UnityEngine.Random` 或 `Random.*` 输入/随机旁路。比赛结果仍在全部下注锁定后由 `RaceDirector` 预生成，且每个 RacePlan 恰好一名冠军。

红外触控设备/驱动：当前环境未连接目标 Windows HID 红外多点触控设备；实体多点并发、边缘触控、投影映射和长时间展项压力测试保留至 Phase 08。

已知限制：新增的八只替补鸡目前复用可替换的占位 Prefab 结构，稳定 ID、显示配置与换鸡链路完整，美术造型和命名本地化可在后续内容阶段替换。当前 GameOver 逻辑与按钮已由规则/状态测试和场景配置验证，尚未在本机用实体触控设备完整游玩 20 局。

证据：`Logs/Phase06-EditModeResults.xml`、`Logs/Phase06-PlayModeResults.xml`、`Logs/Phase06-Settlement-1920x960.png`，以及项目外 `GroundChickenKing-phase06-setup.log`、`GroundChickenKing-phase06-editmode.log`、`GroundChickenKing-phase06-playmode.log`、`GroundChickenKing-phase06-config.log`、`GroundChickenKing-phase06-build.log`、`GroundChickenKing-phase06-runtime.log`。
