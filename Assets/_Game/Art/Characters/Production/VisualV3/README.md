# Phase 11A 第三版：外形、表情与 Unity 导入工作稿

本版继续制作闪电鸡、胖墩和摸鱼鸡，依据 `Docs/Art/Concepts/Phase11/` 已确认的三视图与美术圣经调整外形。所有资产仍为 **程序构建的制作中版本**；面数达标不代表手工拓扑、电影级美术或最终动画验收通过。

## 本轮改进

- 将躯干、颈部与头部连接为连续网格，过渡区使用两骨混合权重。
- 重做眼白、虹膜、瞳孔、高光与眼睑，修复瞳孔埋入眼白；眨眼只移动眼睑，不压扁眼球。
- 增加有厚度的翼尖、冠羽、尾羽和脚趾；闪电鸡保留金色冠羽、红头带和蓝色翼纹，胖墩改回红冠与青绿领结，摸鱼鸡保留半闭眼、额上护目镜和橙围巾。
- 主体、配件分别为一个蒙皮网格；每档 LOD 两个 Renderer、共用一套材质。原始独立部件保存在隐藏的 `SOURCE_COMPONENTS` 集合，使用未归一化的建模坐标，便于选取修改。
- 闪电鸡高度 1.08 m，胖墩和摸鱼鸡 1.00 m；输出对象缩放为 1。
- 逐帧计算旧 Blocking 的离地高度，通过 CenterOfMass 修正地面穿透，Root 不写动画；仍需修正滑步、自身穿插和动作节奏。

## 文件

- `Source/CH_<id>_visual_wip_v3.blend`：可编辑骨架、网格、9 个表情形变、11 个 Blocking、LOD、打包贴图及预览场景。
- `FBX/CH_<id>_visual_wip_v3.fbx`：经重新导入检查的 Generic 骨架资产。
- `Textures/T_<id>_BaseColor.png`：2048²、sRGB。
- `Textures/T_<id>_Normal.png`：2048²、切线空间、线性；来自程序表面凹凸烘焙，不是高模雕刻烘焙。
- `Textures/T_<id>_Mask.png`：2048²、线性；R = Metallic，G = 1（尚未烘焙 AO），B = 0，A = Smoothness。
- `Unity/`：URP 材质、11 状态检查用 Animator Controller、带 LODGroup 的 WIP Prefab。动画状态尚未接入正式游戏表现驱动。
- `Preview/`：三分之四、侧面、背面、闭眼、摔倒与冲刺预览。
- `validation_report.json`：Blender/FBX 回读检查。
- `unity_validation_report.json`：Unity 6000.3.24f1 实际导入检查。

## 当前参数

| 角色 | LOD0 | LOD1 | LOD2 | 骨骼 | 表情 | 动作 |
|---|---:|---:|---:|---:|---:|---:|
| 闪电鸡 | 29,732 | 17,242 | 8,470 | 46 | 9 | 11 Blocking |
| 胖墩 | 34,388 | 19,942 | 9,797 | 46 | 9 | 11 Blocking |
| 摸鱼鸡 | 34,556 | 20,040 | 9,848 | 46 | 9 | 11 Blocking |

表情：`Blink_L`、`Blink_R`、`Smile`、`Shock`、`Determined`、`Sad`、`Dizzy`、`Cheek_Puff`、`Beak_Corner_Up`。LOD1/2 当前不带表情形变，只保留骨骼动画，需要后续评估远距离表情切换。

## 复现

在项目根目录运行：

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python-exit-code 1 --python Assets/_Game/Art/Characters/Production/Blender/build_phase11a_visual_v3.py
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python-exit-code 1 --python Assets/_Game/Art/Characters/Production/Blender/validate_phase11a_visual_v3.py
```

生成脚本可传 `-- --character flash` 仅生成一只，或 `-- --renders-only` 从已有第三版源文件重渲染预览。重新生成会覆盖第三版本身，不覆盖 v1/v2。

Unity 菜单：`Ground Chicken King > Phase 11 > Import and Validate WIP Benchmarks`。也可批处理调用 `GroundChickenKing.Editor.Phase11BenchmarkImporter.ImportAndValidate`。导入器为这些工作稿配置 Generic/Root、Bake Into Pose、URP 材质与 LOD，检查 Avatar、动作、表情和骨骼引用；不会修改游戏场景。

## 阶段边界

尚未达到已确认原画的精细羽毛、脸部塑形和材质品质；尚未完成手工关节拓扑、控制 Rig、逐角色性格动画与脚底约束。UV 为自动打包，Normal/AO 还需正式高低模烘焙。当前检查没有覆盖逐帧自穿插、亚帧地面穿透、投影实拍、性能、红外硬件和发布构建。Phase 11A 仍未完成，不进入 Phase 11B。
