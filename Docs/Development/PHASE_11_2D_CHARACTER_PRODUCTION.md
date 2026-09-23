# Phase 11：2D 精品角色与动作生产

## 目标

使用已确认的 13 套角色细化图制作正式二维角色。比赛采用完整角色 Sprite 连帧：每个
动作直接切换对应的整只角色原画，不拆头、躯干或四肢重新拼合，在保持角色造型的同时
完成 11 种动作及顺畅切换。

## 不可破坏规则

- `ChickenController.NormalizedProgress` 是唯一比赛位置来源。
- Sprite、骨骼、Animator、帧率和特效不得决定名次或经济结算。
- Run/Sprint 动画中不得包含向前 Root 位移。
- 所有 13 只鸡使用相同状态名，但姿势、节奏、重心和二级动作必须体现角色性格。
- 角色资产不得遮挡或绕过任何玩家触控按钮。

## 制作路线

1. 闪电鸡：用动作姿势表建立完整角色 Sprite 连帧及核心可玩动作。
2. 胖墩与摸鱼鸡：验证宽体型、松垮体型的整图帧尺度、基线和状态过渡。
3. 其余十只：按通过的画布、基线、Animator 和 Prefab 模板批量生产。
4. 十三只补齐 11 个动作、表情、配饰二级运动和胜负表现。
5. 五鸡同屏、1920×960、投影和 Windows Release 验收。

## 角色资产标准

- 比赛动作帧为透明背景、完整身体和统一脚底基线。
- 每帧必须保留原动作表中的头身比例、四肢位置、表情、配饰和完整轮廓；禁止运行时拆件拼装。
- 站立动作保持统一角色尺度；摔倒允许改变包围盒但不得裁切。
- 图集边缘必须无黑边、彩色散点和半透明背景污染。
- 正面细化图用于下注头像、冠军展示和图鉴，不强行变形成侧视跑步帧。

## 动画状态与过渡

状态仍为 `Idle`、`Warmup`、`Run`、`Sprint`、`Stop`、`Fall`、`Recover`、
`Turn`、`Interfere`、`Celebrate`、`Lose`。

- Run ↔ Sprint：0.10 秒交叉淡化并保留脚步周期相位。
- 任意状态 → Fall：短过渡，快速进入明确失衡姿势。
- Fall → Recover：从落地末帧衔接起身首帧。
- Turn/Interfere：短过渡并由比赛事件归一化时间控制。
- Celebrate/Lose：较柔和过渡；结算期间允许循环但不能改变比赛位置。
- 相同状态重复设置时不得重启动画。

## 当前检查点

- 闪电鸡动作关键姿势表与侧视透明母版已生成并保存到 `Production/2D/Flash/`。
- 原拆件重绘方案已被用户否决：其拼合比例、位置和整体轮廓偏离已确认概念图，不得继续复制到其他角色。
- 闪电鸡已改为“概念锁定版”：运行时仅引用已确认的完整侧视母版，不重画、不补件、不重新拼合。
- 已建立单 Sprite 正式 Prefab、Animator Override、角色定义及 11 个整图动作片段；静止造型与母版为同一 Sprite 资源。
- 完整母版摆动版只保留为静态回退方案，不作为最终 Run/Sprint 表现。
- 用户随后确认动作表中的迈左腿、迈右腿、腾空、跌倒、起身等均为可直接使用的动作原画；正式表现已改为 Sprite 连帧，不再以整图摆动模拟奔跑。
- 闪电鸡 12 张动作原画已按固定 362×362 画布无损切帧，只清除透明区离散噪点；所有帧保持动作表中的原始位置、比例和共同基线。
- Run 使用 `run_contact → run_passing → run_airborne → run_passing` 循环；Sprint、Stop、Fall、Recover、Turn、Interfere、Celebrate 和 Lose 均由对应原画帧直接播放。
- 胖墩与摸鱼鸡已直接使用各自已确认动作姿势表：只移除灰色底板并放入 384×512 透明固定画布，不重画、不拆件、不缩放角色。
- 两只角色均已生成 11 个 Animator 状态、单 SpriteRenderer Prefab、Animator Override 和 `Chicken2DVisualDefinition`；摸鱼鸡额外直接使用动作表中的专属 Lose 原画。
- 两只角色的 Run 均在动作表的两张完整奔跑原画间循环切换；胖墩保持宽圆体型和领结，摸鱼鸡保持松垮灰羽、护目镜与围巾。
- Run/Sprint 只包含零值 X 曲线，比赛前进仍完全由 `ChickenController.NormalizedProgress` 驱动。
- 相同状态不重启、Run/Sprint 保留周期相位及状态专属过渡已接入运行控制器。
- 概念锁定版专项 EditMode 3/3 通过：只含一个 SpriteRenderer、直接引用已确认母版、不缩放变形、无赛道方向位移。
- 原画连帧专项 EditMode 3/3、完整 EditMode 回归 82/82 通过；正式入口为 `CFG_Chicken2D_flash_Frames.asset`。
- 胖墩与摸鱼鸡专项 EditMode 6/6、完整 EditMode 回归 88/88 通过；正式入口分别为 `CFG_Chicken2D_chubby_Frames.asset` 与 `CFG_Chicken2D_slacker_Frames.asset`。
- 已建立 `SCN_Phase11A_2DLineup.unity` 和可复用五赛道样板 Prefab，以闪电鸡、胖墩、摸鱼鸡交错复用形成五个体型压力位；该重复仅用于同屏验证，不改变正式阵容唯一性规则。
- 三只正式 2D Prefab 已统一使用各自独立的 URP 2D Unlit 材质，修复批处理渲染只显示白色剪影的问题，同时保持 Animator 换帧能力。
- 1920×960 实际渲染图已检查：五只角色完整可见、透明边缘可读、五条脚底基线一致、Sprite 排序唯一、终点与画面安全边距无裁切。
- 五鸡同屏专项 EditMode 3/3、完整 EditMode 回归 91/91 通过。
- Phase 11A 退出条件已满足；下一入口为按已通过模板制作其余十只角色的完整角色 Sprite 连帧。
- 鸡哥、鸡队长、小笼包、幸运鸡、忍者鸡、火箭鸡、学霸鸡、瞌睡鸡、雷鸣鸡、豆丁已全部从各自确认的动作姿势表提取为 384×512 透明固定画布整鸡帧；不拆件、不镜像、不缩放、不重画。
- 十只角色各有 13 张正式帧和 11 个 Animator 状态，Run 只在原画的迈步/奔跑两格之间交替；Sprint、Stop、Fall、Recover、Turn、Interfere、Celebrate、Lose 均直接使用对应动作原画。
- 十只角色各自使用独立 URP 2D Unlit 材质、单 SpriteRenderer Prefab、Animator Override 与 `Chicken2DVisualDefinition`；十三个正式 2D 角色 ID 已验证唯一。
- 剩余阵容专项 EditMode 31/31、完整 EditMode 回归 122/122 通过；下一入口为十三只全阵容同屏、状态切换与最终 Windows Release 验收。

## 11A 退出条件

- 闪电鸡、胖墩、摸鱼鸡各有干净的侧视分层母版。
- 三只标杆角色可在 Unity 播放 Idle、Warmup、Run、Sprint、Fall、Recover。
- Run/Sprint 切换无明显脚步跳变；Fall/Recover 不重复重启。
- 五鸡同屏时透明边缘、排序、脚底基线和 2:1 画面可读性通过。

状态：已满足。实机投影和 Windows Release 属于 Phase 11 全角色完成后的最终发布门禁。

未满足以上条件前，不宣告十三只正式角色动画完成。
