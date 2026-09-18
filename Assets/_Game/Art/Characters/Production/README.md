# Phase 11A Blender 标杆角色资产

本目录保存正式角色生产源文件。当前版本是 Phase 11A 的首轮工程灰模，用于验证共享骨架、体型差异、配件结构、蒙皮和 Unity Generic Rig 导入；它不是最终细雕模型。

## 目录

- `Blender/build_phase11a_benchmark.py`：可重复生成灰模、骨架、Blocking 动作、预览图和 FBX。
- `Blender/validate_phase11a_assets.py`：检查共享骨架、蒙皮、动作集合、静止姿态和 Root Motion 约束。
- `Source/GCK_ChickenRig_Template_v1.blend`：共享骨架模板。
- `Source/CH_<id>_blockout_v1.blend`：闪电鸡、胖墩、摸鱼鸡的可编辑 Blender 源文件。
- `FBX/CH_<id>_blockout_v1.fbx`：Unity Generic Rig 验证用 FBX。
- `Preview/CH_<id>_blockout_v1.png`：灰模和材质识别预览。

## 当前完成度

- 三只角色使用相同骨名与父子层级。
- 单个主体 Skinned Mesh，眼睛、配件与羽毛颜色保留为多材质槽。
- Root 骨骼不写位移关键帧；所有比赛位移仍由 Unity 程序控制。
- 包含 `Idle_Blocking`、`Warmup_Blocking`、`Run_Blocking`、`Sprint_Blocking`、`Stop_Blocking`、`Fall_Blocking`、`Recover_Blocking`、`Turn_Blocking` 八个工程动作。
- 下一步是在灰模基础上手工细雕、重拓扑、UV、PBR 材质、修权重，并为每只角色制作 11 个性格动画。

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
