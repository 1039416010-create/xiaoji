# Phase 11A：胖墩与摸鱼鸡完整角色连帧验收

## 结论

胖墩与摸鱼鸡已从各自确认过的动作姿势表直接制作成完整角色 Sprite 连帧。处理过程只移除
灰色展示背景，并把原格内容放入统一透明画布；没有拆头、拆腿、重画、镜像或缩放角色。

## 美术来源

- 胖墩：`Docs/Art/Concepts/Phase11/CH_chubby_pose_sheet_v1.png`
- 摸鱼鸡：`Docs/Art/Concepts/Phase11/CH_slacker_pose_sheet_v1.png`
- 生产帧：`Assets/_Game/Art/Characters/Production/2D/Chubby/FrameAnimation/Frames/`
- 生产帧：`Assets/_Game/Art/Characters/Production/2D/Slacker/FrameAnimation/Frames/`

所有生产帧均为 384×512 RGBA。原动作表在格内的位置被保留，仅按画布底部对齐；透明化脚本
位于 `Tools/Art/extract_phase11_pose_frames.py`，可以从已确认原画重复生成相同输入布局。

## 动作接入

两只角色都提供 `Idle`、`Warmup`、`Run`、`Sprint`、`Stop`、`Fall`、`Recover`、
`Turn`、`Interfere`、`Celebrate`、`Lose` 共 11 个状态。

- Run：`run_contact → run_passing → run_contact → run_passing`
- Sprint：`run_airborne → sprint`
- Fall：`slowdown → fall`
- Recover：`fall → recover → idle`
- 胖墩 Lose：使用其失速与恢复原画组合。
- 摸鱼鸡 Lose：直接使用动作表中的低头沮丧原画。

每个 Prefab 只有一个 SpriteRenderer，Animator 禁用 Root Motion，动画片段不含 Transform
位置曲线。比赛世界位置仍由 `ChickenController.NormalizedProgress` 唯一驱动。

## 自动化结果

- 胖墩与摸鱼鸡专项 EditMode：6/6 通过。
- 全项目 EditMode：88/88 通过。
- 检查内容：帧数量与尺寸、透明 Sprite 导入、Run 原画顺序、无位移曲线、单
  SpriteRenderer、Prefab 无缩放、Root Motion 关闭、角色定义有效。

## 五鸡同屏收尾

五鸡样板场景、Prefab 和 1920×960 渲染图已完成，专项 EditMode 3/3、全项目 EditMode
91/91 通过。三只正式角色使用独立 URP 2D Unlit 材质，五个压力位的 Sprite 排序唯一，
可见轮廓底部与赛道基线误差不超过 0.001 世界单位。

样板重复使用闪电鸡、胖墩、摸鱼鸡来覆盖瘦高、宽体和松垮三种体型极值，不进入正式阵容，
不改变“每局五只 ID 唯一”的业务规则。

证据：

- `Assets/_Game/Scenes/SCN_Phase11A_2DLineup.unity`
- `Assets/_Game/Prefabs/Race/PF_Race_Phase11A_2DLineup.prefab`
- `Assets/_Game/Art/Characters/Production/2D/Phase11A/Preview/PHASE11A_2D_Lineup_1920x960.png`

Phase 11A 退出条件已满足。红外触控、实体投影和 Windows Release 仍属于全角色制作完成后的
最终设备/发布验收，不在本次样板场景范围内。
