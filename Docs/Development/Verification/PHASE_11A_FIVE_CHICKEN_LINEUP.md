# Phase 11A：五鸡同屏与 2:1 构图验收

## 产物

- 独立验证场景：`Assets/_Game/Scenes/SCN_Phase11A_2DLineup.unity`
- 五赛道样板 Prefab：`Assets/_Game/Prefabs/Race/PF_Race_Phase11A_2DLineup.prefab`
- 1920×960 渲染证据：`Assets/_Game/Art/Characters/Production/2D/Phase11A/Preview/PHASE11A_2D_Lineup_1920x960.png`
- 统一材质：闪电鸡、胖墩、摸鱼鸡各自独立的 URP 2D Unlit Material。

样板使用 `闪电鸡 → 胖墩 → 摸鱼鸡 → 闪电鸡 → 胖墩` 组成五个压力位，以同时覆盖
瘦高、宽体和松垮体型。重复实例只存在于验证场景，不接入 RosterService，也不改变正式
比赛五只鸡 ID 唯一的规则。

## 验收结果

- 输出尺寸为 1920×960，严格 2:1。
- 五条赛道和五个完整角色均在画面安全区内，无身体或配饰裁切。
- 每条赛道可见轮廓底部与基线的误差不超过 0.001 世界单位。
- 五个角色 SpriteRenderer 使用唯一排序值，赛道、基线和终点线层级固定。
- 三只角色 Prefab 均保持单 SpriteRenderer、原比例、Root Motion 关闭。
- URP 批处理渲染正确显示角色纹理和透明边缘，不再出现白色剪影。

## 自动化

- `Phase11ALineupTests`：3/3 通过。
- 全项目 EditMode：91/91 通过。
- 覆盖：五赛道数量、完整角色单 SpriteRenderer、材质、排序、基线、无缩放、无 Root
  Motion、2:1 适配组件、1920×960 图片尺寸及非空视觉内容。

## 结论

Phase 11A 的三个标杆角色与五鸡同屏门禁均已通过，可以进入其余十只角色的完整原画连帧
批量制作。实体投影、红外触控和 Windows Release 保留到 Phase 11 全角色完成后的最终验收。
