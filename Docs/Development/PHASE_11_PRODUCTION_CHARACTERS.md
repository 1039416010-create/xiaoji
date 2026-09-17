# Phase 11：精品角色建模、骨骼与个性动画生产

## 结论

Phase 10 的程序化基础 Mesh 只能作为技术占位与玩法验证，不能作为最终展项角色资产。最终版本应改为 Blender 或 Maya 制作的手工拓扑蒙皮模型，使用一套兼容的非人形 Generic 骨架、13 套独立角色外形和按性格制作的动画。AI 生成 3D 只能用于概念草图或初始雕刻参考，不直接进入发布包。

## 不可破坏的运行规则

- `ChickenController.NormalizedProgress` 继续是赛道位移唯一真相；骨骼动画只表现动作，禁止 Root Motion 推进名次。
- `RaceDirector` 仍在下注全部锁定后生成剧本；模型、Animator、碰撞和帧率不得影响胜负。
- 所有 FBX 在 Unity 中使用 `Generic` Rig，指定 `Root`，所有位移动画 Bake Into Pose。
- 模型不配置动态刚体碰撞；脚底接触、摔倒和干扰均为纯表现。
- 五只同时上场时，在目标 Windows 设备的 1920×960 Development Build 中保持稳定帧时间。

## 推荐生产路线

采用“共享兼容骨架 + 三种体型母版 + 13 套独立雕刻与拓扑 + 每只鸡专属动画覆盖”的路线。

这不是简单换皮。共享骨架只负责工程兼容和复用技术动作，每只鸡必须拥有独立轮廓、头身比例、羽毛分区、面部结构、配饰、材质和动作节奏。先完整制作 3 只标杆角色（闪电鸡、胖墩、摸鱼鸡），通过桌面投影实拍验收后，再批量制作其余 10 只。

不建议直接购买 13 个来源不同的模型：骨架、比例、材质和美术风格会难以统一。若采购商用基础资产，只能作为合法授权的拓扑起点，最终仍需统一重雕、重新 UV、重新蒙皮和制作专属动画。

## 角色设计矩阵

| ID | 角色方向 | 建模重点 | 动作语言 |
|---|---|---|---|
| chicken-flash | 闪电鸡，爆发型 | 流线瘦高、后掠羽毛、红头带 | 前倾高步频、短促预备、猛烈冲刺 |
| chicken-chubby | 胖墩，沉稳型 | 大腹软羽、短腿、领结 | 重心低、左右摇摆、落地有肉感回弹 |
| chicken-tiny | 豆丁，灵巧型 | 大头小身、细腿、跑鞋 | 极高步频、小幅跳步、快速回头 |
| chicken-bro | 鸡哥，好斗型 | 宽胸、粗翅、蓝围巾 | 挺胸压步、挑衅、横向伸腿干扰 |
| chicken-slacker | 摸鱼鸡，随性型 | 松垮羽毛、半闭眼、墨镜 | 拖步、打哈欠、停下躺平后突然追赶 |
| chicken-thunder | 雷鸣鸡，强势型 | 锯齿鸡冠、雷纹羽片 | 重踏蓄力、抖羽、爆发时有电击节奏 |
| chicken-dumpling | 小笼包，憨厚型 | 白色团子体型、包子帽 | 滚圆弹跳、刹车过冲、摔倒后翻身 |
| chicken-captain | 鸡队长，纪律型 | 挺拔胸型、队长帽、整齐尾羽 | 军步热身、鸣哨指挥、标准冲线敬礼 |
| chicken-sleepy | 瞌睡鸡，迟钝型 | 下垂眼睑、睡帽、蓬松羽毛 | 低头打盹、踉跄跑、惊醒式加速 |
| chicken-rocket | 火箭鸡，冒险型 | 流线身体、背负式火箭装置 | 起步后坐、喷射冲刺、过热喘气 |
| chicken-ninja | 忍者鸡，敏捷型 | 紧身羽片、面罩、低轮廓 | 贴地疾跑、翻滚恢复、无声干扰 |
| chicken-scholar | 学霸鸡，理性型 | 学士帽、规整羽毛、聪明眼神 | 计算步点、观察对手、胜利后推演板书手势 |
| chicken-lucky | 幸运鸡，表演型 | 金色羽毛、小皇冠、夸张尾羽 | 自信弹步、祈运动作、舞台式庆祝 |

## 模型与材质标准

- 每只角色 LOD0 建议 25k–45k 三角面；LOD1 12k–22k；LOD2 5k–10k。
- 同一角色最多 2 个主体 SkinnedMeshRenderer，眼睛和小型硬表面配件可独立；避免几十个散碎 Renderer。
- 采用手工重拓扑，关节环线覆盖翅根、颈部、髋、膝、踝、脚趾和尾根。
- 每只角色一套 2048² PBR 图集：Base Color、Normal、Mask（Metallic/AO/Smoothness）；眼睛可共享 1024² 图集。
- 羽毛采用“大形体雕刻 + 法线细节 + 少量轮廓羽片”，不使用大量透明卡片，避免投影锯齿和过度 Overdraw。
- 角色实际高度统一在约 1 Unity unit 的可控范围；应用变换后导出，禁止负缩放。
- 交付源文件必须包含 `.blend` 或 `.ma/.mb`、高模、低模、UV、贴图源文件和 FBX，不只交付不可编辑 FBX。

## 骨骼标准

使用约 40–55 根变形骨骼的共享层级：

```text
Root
└── CenterOfMass
    ├── Body_01 -> Body_02 -> Neck_01 -> Neck_02 -> Head
    │   ├── Beak_Upper / Beak_Lower
    │   ├── Eye_L / Eye_R / Lid bones
    │   └── Comb chain / accessory socket
    ├── Wing_L: Shoulder -> Elbow -> Wrist -> Feather fan
    ├── Wing_R: Shoulder -> Elbow -> Wrist -> Feather fan
    ├── Leg_L: Hip -> Knee -> Ankle -> Toe_A / Toe_B
    ├── Leg_R: Hip -> Knee -> Ankle -> Toe_A / Toe_B
    └── Tail_01 -> Tail_02 -> Tail fan
```

- 控制 Rig 与导出 Deform Rig 分离，FBX 只导出变形骨和必要 Socket。
- 眼睑、笑、惊讶、委屈、眩晕、咬牙等表情采用 6–10 个 BlendShape，眼球/瞳孔仍可用骨骼控制。
- 自动权重只作为起点；翅根、腿根、颈部和摔倒挤压必须逐姿势手工修权重。
- 三种体型母版使用相同骨名和拓扑语义，允许不同骨长；动画通过同层级重定向后再由动画师逐角色修型。

## 动画清单

每只鸡至少完成以下 11 个经过性格化处理的 Clip：

1. `Idle`：4–6 秒循环。
2. `Warmup`：6–8 秒循环，体现角色习惯。
3. `Run`：0.6–1.1 秒循环，步频和腾空时间各异。
4. `Sprint`：0.45–0.8 秒循环。
5. `StopOrSlow`：1.5–3 秒可保持片段。
6. `TripFall`：明确失衡、触地和表情变化。
7. `Recover`：从地面重新站起并接回 Run。
8. `TurnAround`：180° 转身且双脚不滑步。
9. `Interfere`：伸腿或翅膀干扰，相邻赛道只做表演反应。
10. `Celebrate`：3–5 秒循环或可衔接循环。
11. `Lose`：2–4 秒，沮丧但保持喜剧感。

先做 8 个共享技术 Blocking Clip 验证状态切换，再为 13 只鸡逐一做 Timing、Pose、重心、表情和二级羽毛动作覆盖。最终不是简单调播放速度；关键姿势和运动轨迹必须由动画师逐角色调整。

## Unity 接入结构

1. 新增 `ChickenVisualDefinition` ScriptableObject：稳定 ID、角色 Prefab、AnimatorOverrideController、材质、舞台缩放、脚底高度、表情/VFX 配置。
2. 新增 `ChickenVisualDriver`：只接收 `ChickenVisualState`、事件归一化时间和角色配置，驱动 Animator 与表情；不得写比赛进度。
3. `Chicken3DStage` 改为五槽对象池，仅在换鸡时实例化/替换 Prefab，比赛每帧只更新 Transform 和 Animator 参数。
4. `ChickenController` 继续计算 `NormalizedProgress`；舞台把它映射到固定起终点，Animator 的 `applyRootMotion` 永远为 `false`。
5. 每个角色使用同一基础 Animator Controller，通过 Animator Override Controller 替换 11 个性格 Clip。
6. 程序化几何保留为缺失资产的 Development Build 降级占位，但 Release Build 若任一角色缺少正式 Prefab 必须阻止构建。

## 分阶段交付与验收

### 11A：美术圣经与三只标杆角色

- 完成正侧背三视图、色板、材质样板、表情表和动作 Pose Sheet。
- 制作闪电鸡、胖墩、摸鱼鸡完整模型、骨骼和 11 个动作。
- Unity 五鸡同屏验证轮廓、灯光、脚底接触、跌倒/爬起和 2:1 投影可读性。

退出条件：三只角色在实际桌面投影上无需文字即可区分性格；动画无穿模、滑步、爆权重和 Root Motion 位移。

### 11B：其余十只角色

- 按通过的骨架、材质和文件模板完成剩余角色。
- 每完成一只即经过模型转台、动作蒙太奇和 Unity 比赛回放三项评审。

退出条件：13 个 Prefab、143 个性格化动作槽均完整，任意五只混排不存在明显品质落差。

### 11C：整合与性能

- 完成对象池、Animator Override、LOD、GPU Instancing/SRP Batcher 兼容、贴图压缩和资产校验器。
- 进行 100 局自动播放、五鸡同时复杂事件、结算切换和 Windows x64 长稳测试。

退出条件：无 Missing Clip/Material/Bone；五鸡同屏稳定；比赛结果与相同种子保持不变；实机 10 点触控流程完整。

## 工期与采购判断

- 推荐团队：1 名角色原画/美术指导、1 名角色模型师、1 名绑定动画师；材质可由模型师兼任。
- 三只标杆角色：约 3–5 周；其余十只批量生产：约 6–10 周；Unity 整合和返修：约 2–3 周。具体取决于每只角色动作是否全部独立制作。
- 最省风险的采购方式是按“3 只标杆验收通过后再扩到 13 只”的里程碑合同付款；必须约定源文件、商业使用权、修改权和 Unity 工程内使用权。
- 仅购买现成模型成本最低，但无法满足“每只鸡独特建模和动作”的质量目标；AI 3D 一键模型也不能替代重拓扑、绑定、权重和动画打磨。

## 官方技术依据

- Unity 非人形模型应使用 Generic Rig，并明确 Root node：<https://docs.unity3d.com/cn/current/Manual/GenericAnimations.html>
- Blender 蒙皮与 Armature Modifier：<https://docs.blender.org/manual/en/5.0/animation/armatures/skinning/introduction.html>
- Blender FBX 可烘焙修改器与动画，导出时需管理轴向、骨骼和 Action：<https://docs.blender.org/manual/en/5.0/addons/import_export/scene_fbx.html>
