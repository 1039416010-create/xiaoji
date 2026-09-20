# Phase 11A Blender 标杆角色资产

本目录保存正式角色生产源文件。当前版本是 Phase 11A 的首轮工程灰模，用于验证共享骨架、体型差异、配件结构、蒙皮和 Unity Generic Rig 导入；它不是最终细雕模型。

最新工作稿位于 **`VisualV3/`**，完成外形、眼睛/眼睑、羽片和配饰调整，提供烘焙材质图集与实际 Unity 导入后的检查用预制体。详情和未完成项见 `VisualV3/README.md`；下文保留前两版的复现方式。

## 目录

- `Blender/build_phase11a_benchmark.py`：可重复生成灰模、骨架、Blocking 动作、预览图和 FBX。
- `Blender/validate_phase11a_assets.py`：检查共享骨架、蒙皮、动作集合、静止姿态和 Root Motion 约束。
- `Blender/advance_phase11a_wip_v2.py`：从灰模生成带 LOD、表情形变和完整动作槽的第二版制作资产。
- `Blender/validate_phase11a_wip_v2.py`：验证第二版 LOD、UV、表情形变、动作和 Root Motion 约束。
- `Source/GCK_ChickenRig_Template_v1.blend`：共享骨架模板。
- `Source/CH_<id>_blockout_v1.blend`：闪电鸡、胖墩、摸鱼鸡的可编辑 Blender 源文件。
- `Source/CH_<id>_production_wip_v2.blend`：第二版可编辑制作文件，包含 LOD0/1/2 和表情形变。
- `FBX/CH_<id>_blockout_v1.fbx`：Unity Generic Rig 验证用 FBX。
- `FBX/CH_<id>_production_wip_v2.fbx`：第二版 Unity 接入验证用 FBX。
- `Preview/CH_<id>_blockout_v1.png`：灰模和材质识别预览。
- `Preview/CH_<id>_wip_v2_<pose>.png`：冲刺、摔倒、庆祝极限姿势预览。

## 当前完成度

- 三只角色使用相同骨名与父子层级。
- 单个主体 Skinned Mesh，眼睛、配件与羽毛颜色保留为多材质槽。
- Root 骨骼不写位移关键帧；所有比赛位移仍由 Unity 程序控制。
- 第二版包含 LOD0/1/2（16,320 / 12,240 / 7,344 三角面）以及统一的 `UV0`。
- 第二版包含 Blink L/R、Smile、Surprise、Sad、Dizzy、Grit、Squash 八个独立表情形变。
- 已补齐 11 个工程动作槽；动作仍处于 Blocking 阶段，尚不是最终性格动画。
- 下一步是在第二版基础上手工细雕、重拓扑、PBR 材质和逐姿势修权重，再将 11 个动作打磨为角色专属动画。

## 重新生成

在仓库根目录执行：

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python 'Assets\_Game\Art\Characters\Production\Blender\build_phase11a_benchmark.py'
```

生成过程只写入本目录的 `Source`、`FBX` 和 `Preview`。

生成后执行结构验收：

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python 'Assets\_Game\Art\Characters\Production\Blender\validate_phase11a_assets.py'
```

生成并验证第二版：

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python 'Assets\_Game\Art\Characters\Production\Blender\advance_phase11a_wip_v2.py'
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python 'Assets\_Game\Art\Characters\Production\Blender\validate_phase11a_wip_v2.py'
```
