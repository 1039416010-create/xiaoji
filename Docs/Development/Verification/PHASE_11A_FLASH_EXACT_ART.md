# Phase 11A：闪电鸡概念锁定版验收记录

日期：2026-09-22

## 修正原因

用户否决原分层拼合版。该版本使用生成式拆件和补画脚掌，造成头身比例、腿长、尾羽连接、关节位置和整体重心偏离已确认概念图。

## 新的不可变美术规则

- 正式角色只引用 `CH_flash_side_master_v1.png`，不得重新生成身体零件。
- Prefab 中仅有一个 SpriteRenderer；显示 Sprite 与已确认母版为同一 Unity 资产引用。
- Run 与 Sprint 不含缩放曲线，不改变角色比例。
- 动画不得含赛道方向位移，比赛推进继续只由 `ChickenController.NormalizedProgress` 控制。
- 在没有原始 PSD/分层源文件的情况下，不制作需要补画遮挡区域的独立肢体大幅运动。

## 产物

- `PF_Chicken_flash_2D_Exact.prefab`
- `AC_flash_2D_Exact.controller`
- `AOC_flash_2D_Exact.overrideController`
- `CFG_Chicken2D_flash_Exact.asset`
- 11 个 `AN_flash_*_2D_Exact.anim` 动作片段
- 20 帧 Unity 实际渲染的 Run 循环及 GIF 预览

## 自动化结果

- 概念锁定版专项 EditMode：3/3 通过，0 失败。
- 验证内容：单一原画 Sprite 引用、Prefab 零偏移/单位缩放、Run 无缩放变形、Run 的 X 曲线恒为零、正式角色定义引用准确。

## 限制

概念锁定版能保证造型与原画一致，但扁平 PNG 不包含被身体遮挡的腿根、翅根和背面像素。若需要腿、翼、尾羽独立大幅运动且仍要求逐像素保持设计，必须提供原画的 PSD/PSB/Spine/Live2D 等分层源文件；使用生成式补画无法满足“无任何出入”。
