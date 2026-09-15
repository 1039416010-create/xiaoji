# Phase 09：场景、美术、UI 与角色表现升级

## 目标

在不修改比赛公平性、经济和全触控契约的前提下，把原型级纯色界面升级为可展示的“乡村嘉年华”视觉版本，并建立可重复执行的 Unity 美术装配流程。

## Codex 执行任务

1. 使用 GPT 图像生成引擎制作 2:1 桌面背景、五鸡角色图集、按钮、面板和功能图标。
2. 所有生成图禁止烘焙中文或交互结果；文字、金额和状态继续由 Unity UI 渲染。
3. 生成图保存到 `Assets/_Game/Art/*/Generated`，保留透明通道和素材清单。
4. 新增幂等的 `Phase09ArtSceneBuilder`，配置导入参数、九宫格边界、场景层级和 Prefab 引用。
5. 五只鸡共用一张横向图集，以固定 UV 栏位呈现，保持各赛道实例与比赛逻辑不变。
6. Animator 只驱动 `Visual/Artwork` 子节点；`ChickenController` 继续独占赛道位置和事件状态。
7. 为 Idle、Warmup、Run、Sprint、Stop、Fall、Recover、Turn、Interfere、Celebrate、Lose 写入表现曲线。
8. 所有游客按钮继续使用 `Button` 与 `TouchButtonFeedback`，不得新增隐藏热区或键盘路径。
9. 为背景、按钮、面板、角色图集、动画曲线和场景引用增加自动化测试。
10. 对主菜单、加入、下注、比赛、结算、设置、诊断、确认和 GameOver 生成 1920×960 截图验收。

## 退出条件

- 生图资产已经进入仓库，不引用 Codex 外部缓存路径。
- 场景和五个小鸡 Prefab 引用完整，运行时无 Missing Script/Texture。
- 按钮 Normal、Pressed、Disabled 状态清楚，不依赖 Hover。
- 所有动画不使用 Root Motion，不改变比赛结果。
- EditMode、PlayMode 与 Windows Development Build 通过。
- 设计规范、Prompt、截图和已知限制已经记录。

