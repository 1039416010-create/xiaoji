# Phase 11B：剩余十只角色整图帧动画验收

## 范围

本阶段完成鸡哥、鸡队长、小笼包、幸运鸡、忍者鸡、火箭鸡、学霸鸡、瞌睡鸡、雷鸣鸡、豆丁的正式 2D 动画资产。输入只使用 `Docs/Art/Concepts/Phase11/` 下已经确认的动作姿势表，没有调用图像生成重新绘制角色。

## 制作结果

- 每只角色从动作表提取 13 张 384×512 RGBA 完整角色帧。
- 提取仅执行背景清理与固定透明画布放置；不拆分身体、不镜像、不拉伸、不改变头身比例。
- 动画状态统一为 `Idle`、`Warmup`、`Run`、`Sprint`、`Stop`、`Fall`、`Recover`、`Turn`、`Interfere`、`Celebrate`、`Lose`。
- `Run` 在两张已确认奔跑原画之间循环；所有动画片段只切换 `SpriteRenderer.m_Sprite`，没有 Transform 位移或缩放曲线。
- 每只角色使用独立 URP 2D Unlit 材质、单 SpriteRenderer Prefab、Animator Controller、Animator Override 与角色定义。
- `Animator.applyRootMotion=false`；比赛位移继续由 `ChickenController.NormalizedProgress` 唯一控制。

## 自动化验收

- 构建入口：`Ground Chicken King/Phase 11/Build Remaining Roster Frame Animation`
- 专项 EditMode：31/31 通过。
- 完整 EditMode 回归：122/122 通过。
- 覆盖内容：帧数量与画布、透明导入、奔跑帧顺序、无 Transform 曲线、11 状态、单渲染器、独立材质、无 Root Motion、角色定义有效、十三个角色 ID 唯一。

## 待后续验证

- 十三只角色在 1920×960 五赛道中的随机阵容尺度与遮挡压力测试。
- Run/Sprint/Fall/Recover 等状态在实际比赛事件中的切换观感。
- Windows x64 Development/Release 完整多局运行与红外触控实机验收。

状态：Phase 11B 资产制作与专项自动化验收通过，进入全阵容集成验收。
