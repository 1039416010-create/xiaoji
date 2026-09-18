# Phase 11 角色美术审核

## 当前完成状态

- 13 只鸡的三视图已完成并通过用户审核，不再重复生成或重新审核。
- 13 只鸡均已完成 `turnaround` 和 `pose_sheet`。
- 闪电鸡、胖墩另有 `expression_material` 补充页，用于覆盖掉头、干扰、失败、表情和材质参考。
- 2026-09-17 用户明确要求“建模之前不用审核，把这些角色的美术全部做完”。取消逐张等待审核，连续补齐全部角色美术；生成完成不等于用户逐张确认。
- `CHARACTER_ART_BIBLE.md` 已记录色板、轮廓、材质、配件、动作签名、绑定重点和 Blender 交接要求。
- `GENERATION_PROMPTS.md` 已记录本轮最终 Prompt 集和生成方式。
- 建模前角色美术准备完成；下一入口为 Phase 11A 的闪电鸡、胖墩、摸鱼鸡三只标杆角色建模。

## 审核门禁

本目录中的三视图通过用户审核以前，不得据此制作或替换 Blender 模型，也不得接入 Unity 正式角色 Prefab。

当前第一轮完整角色阵容：

- `CH_flash_turnaround_v1.png`：闪电鸡，瘦高爆发型。
- `CH_chubby_turnaround_v1.png`：胖墩，圆胖沉稳型。
- `CH_slacker_turnaround_v1.png`：摸鱼鸡，松垮慵懒型。
- `CH_tiny_turnaround_v1.png`：豆丁，大头细腿灵巧型。
- `CH_bro_turnaround_v1.png`：鸡哥，宽胸强壮好斗型。
- `CH_thunder_turnaround_v1.png`：雷鸣鸡，锯齿冠羽强势型。
- `CH_dumpling_turnaround_v1.png`：小笼包，白色软弹团子型。
- `CH_captain_turnaround_v1.png`：鸡队长，挺拔纪律型。
- `CH_sleepy_turnaround_v1.png`：瞌睡鸡，蓬松迟钝型。
- `CH_rocket_turnaround_v1.png`：火箭鸡，流线冒险型。
- `CH_ninja_turnaround_v1.png`：忍者鸡，低姿敏捷型。
- `CH_scholar_turnaround_v1.png`：学霸鸡，端正理性型。
- `CH_lucky_turnaround_v1.png`：幸运鸡，华丽表演型。

## 审核重点

1. 三个角度是否像同一只角色，配饰和色块是否一致。
2. 五鸡同屏时轮廓能否快速区分。
3. 腿、翅、颈、眼睑和尾羽是否有足够空间完成既定动作。
4. 角色是否符合家庭互动展项，而不是写实斗鸡或幼儿贴纸风格。
5. 通过后锁定角色比例、色板和配饰，再进入 Blender 建模。

## 生成方式

图片由内置 GPT 图像生成能力制作。三视图是美术方向与建模参考，不是可直接投产的精确工程蓝图；建模前仍需校正正交比例、对齐地面线，并补充表情和动作 Pose Sheet。

统一提示词要求：动画电影级 stylized 3D 角色设定；正面、严格左侧面、严格背面使用同一比例、配饰与色块；浅灰暖色摄影棚背景；完整展示鸡冠至脚底；结构必须可用于 Blender 骨骼绑定；禁止文字、水印、复杂场景、机械人体和真实暴力元素。每只角色再按上表的轮廓、性格、配色和动作语言添加独立设计约束。
