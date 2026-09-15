# Phase 09 验收记录

Unity 版本：6000.3.24f1（Unity 6.3 LTS）

测试日期：2026-09-15

## 自动化

- Editor 编译与 Phase 09 场景装配：通过，返回码 0。
- EditMode 全量测试：60/60 通过，0 失败，0 跳过。
- PlayMode 全量测试：16/16 通过，0 失败，0 跳过。
- Windows Development Build：通过，返回码 0；构建日志报告 181,410,152 bytes。
- 禁用输入 API 静态扫描：通过；Runtime 未发现 `Input.Get*`、`GetKey`、`Keyboard.current`、`Mouse.current`、`OnMouse*` 或 `UnityEngine.Random`。

## 视觉截图

- 基线：`Builds/Visual/before-art-mainmenu.png`（忽略目录，仅本机证据）。
- 最终截图目录：`Builds/Visual/Phase09/`（忽略目录，仅本机证据）。
- 已核对：主菜单、玩家加入、下注、比赛、设置和诊断界面，均为 1920×960。
- 主菜单：木桌节庆背景、中央木框、皇冠、角色立绘与统一木牌按钮正常。
- 玩家加入：上下两侧玩家区方向正确，上侧 UI 按桌面使用方式旋转 180°。
- 比赛：五条赛道、五种角色、终点旗帜、标题和随机种子信息均无裁切。
- 设置与诊断：面板、增减按钮、返回/保存及校准入口均保持可见 Button 交互。

## 已知限制

- 单张角色图通过变换曲线表现动画，不是逐帧手绘动画。
- 实体红外 HID 验收仍遵循 ADR-0002 的暂缓决定。
- 结算与 GameOver 沿用同一运行时主题装配；本轮自动截图集中覆盖高频主流程与维护界面。
