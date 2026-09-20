# Phase 11A 第三版外形与导入验证

日期：2026-09-18；Blender 5.2.2 LTS；Unity 6000.3.24f1。

## 范围与结果

三只标杆角色继续处于制作阶段。本轮实现连续颈胸网格、参考配色和配饰、可见瞳孔及眼睑闭合、轮廓羽片、UV/PBR 工作贴图、LOD 和 Unity 检查用 Prefab。所有新增角色源文件均在 `Assets/_Game/Art/Characters/Production/VisualV3/`；没有改变业务代码、比赛结果生成或正式场景。

执行 `validate_phase11a_visual_v3.py`（使用 `--python-exit-code 1` 保证断言失败返回非零）通过：

- 46 骨共享层级；所有导出顶点的有效骨骼权重归一。
- 9 个表情存在，保存时全部归零；眨眼对眼白、虹膜、瞳孔、高光顶点位移为零，眼睑有实际位移。
- 瞳孔位于眼白表面前方。
- 共 99 个动作姿势抽样无非有限顶点或异常爆形；最低点为 0.002 m。检查是起/中/末帧抽样，不能替代逐帧自穿插与亚帧接触检查。
- Root 在抽样姿势保持静止；CenterOfMass 负责离地高度修正。
- FBX 导出后重新导入，三档主体/配件网格、面数、蒙皮、9 个形变和 11 组动画保持完整。Blender 回读会将同一动画 Stack 的骨骼和形变轨道拆成两份 Action，这不是重复的游戏动作。

执行 Unity `Phase11BenchmarkImporter.ImportAndValidate` 通过，日志包含 `PHASE11_V3_UNITY_IMPORT_PASS`，退出码为 0：

| 角色 | 有效 Generic Avatar | BlendShape | 动作 | LOD0 / LOD1 / LOD2 | Root Motion |
|---|---|---:|---:|---|---|
| 闪电鸡 | 是 | 9 | 11 | 29,732 / 17,242 / 8,470 | 关闭 |
| 胖墩 | 是 | 9 | 11 | 34,388 / 19,942 / 9,797 | 关闭 |
| 摸鱼鸡 | 是 | 9 | 11 | 34,556 / 20,040 / 9,848 | 关闭 |

Unity 编辑器程序集编译并成功执行导入工具；无 C# 编译错误、丢失骨骼引用或导入断言错误。第一次沙盒启动无法连接许可证客户端，终止本轮进程后在正常用户环境重试成功。成功运行日志中的许可证 Access Token 刷新提示未阻止本地已授权许可证验证和导入。

原始报告：`VisualV3/validation_report.json` 和 `VisualV3/unity_validation_report.json`。本地 Unity 日志：`Artifacts/phase11-v3-import.log`（不提交）。

## 明确限制与下一入口

- 模型为程序构建的制作中版本，不宣称手工细雕或重拓扑完成；面数满足预算不是美术验收。
- 11 个动作仍为 Blocking。仍需脚底约束、关节修权重、自穿插处理、控制 Rig、角色独立 Timing/Pose 与羽毛二级运动。
- Normal 来自程序表面凹凸，Mask G=1，尚缺真正的高模法线/AO 烘焙；UV 为自动打包。
- LOD1/2 不含 BlendShape，过渡时的表情保持尚未完成。
- Unity 已导入并生成独立检查用 Prefab/Controller/材质，但没有正式游戏驱动接线、五鸡同屏回放或投影实拍。
- 未执行本轮无改动的经济/赛果回归、Windows 构建或红外触控实测；本记录不宣称这些通过。

下一入口仍为 Phase 11A：以 `VisualV3/Source` 的三个标杆为基础，继续对照原画细化外形与动作，然后进行正式表现驱动接入和现场投影验收。
