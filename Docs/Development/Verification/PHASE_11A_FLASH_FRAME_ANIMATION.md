# Phase 11A：闪电鸡原画连帧动画验收记录

日期：2026-09-22

## 用户确认的制作方式

闪电鸡动作表已经包含迈左腿、迈右腿、腾空、冲刺、跌倒、起身、转身、干扰和庆祝等原画。正式动画直接切换这些完整角色帧，不进行身体拆件拼合，也不使用生成式补画替换角色部位。

## 帧映射

- Idle：待机原画。
- Warmup：预备与待机原画循环。
- Run：接触、换腿、腾空、换腿四帧循环。
- Sprint：腾空与冲刺原画循环。
- Stop：冲刺、减速、待机顺序播放。
- Fall：减速失衡、跌倒顺序播放。
- Recover：跌倒、起身、待机顺序播放。
- Turn、Interfere、Celebrate：直接使用各自原画。
- Lose：减速与低姿态原画循环。

## 资产规则

- 源文件为 `CH_flash_motion_keyposes_v1.png`。
- 按原图 4×3 网格输出 12 张固定 362×362 Sprite。
- 保留原画像素位置、角色比例和画布基线；只删除透明区中的离散彩色噪点。
- Animator 只切换 `SpriteRenderer.m_Sprite`，不包含赛道方向位移或 Root Motion。

## 自动化结果

- 原画连帧专项 EditMode：3/3 通过。
- 完整 EditMode 回归：82/82 通过。
- 覆盖：12 帧数量与固定画布、Run 帧序、单 SpriteRenderer Prefab、无 Root Motion、角色定义引用完整。

## 产物

- `PF_Chicken_flash_2D_Frames.prefab`
- `AC_flash_2D_Frames.controller`
- `AOC_flash_2D_Frames.overrideController`
- `CFG_Chicken2D_flash_Frames.asset`
- 11 个 `AN_flash_*_2D_Frames.anim` 动画片段
- `CH_flash_frame_run_loop.gif` Unity 实际运行预览
