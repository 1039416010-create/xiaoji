# Phase 11 角色美术生成记录

## 生成方式

本轮使用 Codex 内置图像生成工具。所有最终图片已复制到本目录，没有只保留在默认生成目录。

## 通用动作表 Prompt

```text
Use case: stylized-concept. Create a premium animated-feature 3D character action and expression reference sheet for Blender modeling and rigging, landscape 3:2, high detail. Reference 1 is the approved character identity: preserve exact silhouette, colors, face, feather pattern, existing accessories and proportions in every panel. Reference 2 only supplies render quality and warm grey studio background. Do not copy the second character. Clean spacious 4 columns by 3 rows, eleven complete full-body poses in cells 1-11, cell 12 contains four expressive head closeups (joy, shock, determination, sadness). Pose order: Idle, Warmup, Run, Sprint, StopOrSlow, TripFall, Recover, TurnAround with planted pivot foot, Interfere with a playful sideways wing or leg extension, Celebrate, Lose. No labels, text or watermarks. Family-friendly physical comedy. All bodies and toes fully visible, separated, no overlap, consistent anatomy and accessories, no new props. Broad sculpted feather masses and riggable joints, readable eyes eyelids beak expressions and weight shifts. This is a designed pose sheet, not a repeated turnaround. Character-specific movement direction: <character suffix>
```

参考图 1 为对应角色的 `turnaround`；参考图 2 为 `CH_flash_pose_sheet_v1.png`，仅用于版式、背景和渲染完成度。

## 角色专属后缀

- `slacker`：Lazy relaxed slouch, half-closed eyelids, sunglasses: loose idle, yawning stretch, dragging-foot run, sudden catch-up sprint, stopping to recline, comical stumble, reluctant getting up, understated smug victory.
- `tiny`：Tiny big-headed agile chicken: curious idle, springy warmup, rapid short running steps, airborne fast sprint, startled braking, small comic tumble, quick bouncing recovery, exuberant tiny victory hop.
- `bro`：Broad-chested strong blue-scarfed chicken: proud chest-forward idle, wing flex warmup, powerful stomping run, determined sprint, assertive braking, comic shocked trip, stubborn push-up recovery, triumphant chest-out victory.
- `thunder`：Dominant thunder chicken: stern idle, feather-shaking charged warmup, heavy stomp run, sharp explosive sprint, forceful braking, surprised comic trip, defiant recovery, powerful raised-wing celebration.
- `dumpling`：Soft white dumpling-shaped chicken with green ankle band, NO hat: gentle idle, squash-stretch warmup, bouncy rolling gait, eager sprint, braking overshoot, soft comic tumble, rocking back upright, delighted bounce celebration.
- `captain`：Disciplined captain chicken: attention stance, marching warmup, measured upright run, focused sprint, precise halt, undignified comic trip, quick uniform-adjusting recovery, crisp victory salute.
- `sleepy`：Drowsy sleepy chicken: nodding-off idle, yawning stretch, stumbling half-asleep run, wide-eyed startled sprint, sleepy slowdown, harmless nap-like fall, groggy recovery, sleepy happy victory.
- `rocket`：Adventurous rocket-equipped chicken: eager idle, checking existing pack warmup, forward intent run, dramatic launch sprint with tiny stylized exhaust only, overheating braking, surprised stumble, resilient pack-balancing recovery, enthusiastic victory.
- `ninja`：Agile dark ninja chicken: low watchful idle, stealth stretch, low streamlined running, sharp sprint, balanced crouched stop, comic surprised tumble, rolling crouch recovery, reserved confident victory. Preserve uncovered beak and dark face feathers as reference, no additional cloth face mask.
- `scholar`：Intellectual scholar chicken: thoughtful idle, calculating step warmup, precise measured running, concentrated sprint, analytical braking, baffled comic trip, composed recovery adjusting existing hat, inspired wing-gesture victory.
- `lucky`：Flamboyant golden crowned lucky chicken: confident showy idle, hopeful warmup, buoyant elegant run, exuberant sprint, theatrical braking, startled comic fall, dignified recovery, broad stage-bow victory.

## 闪电鸡与胖墩补充页 Prompt

```text
Use case: stylized-concept. Blender character art supplemental sheet for exactly the character in reference 1; reference 2 is its existing action sheet. Preserve its exact design and accessories. Landscape 3:2, premium stylized animated-feature 3D render, warm grey neutral studio. Top half: three large, separated, fully visible full-body poses, left planted-foot 180-degree turn, middle playful sideways wing/leg interference gesture, right disappointed defeated slouch. Bottom left and middle: six clear separated expressive head studies, neutral, joy, shock, gritted determination, sad, dizzy with asymmetrical eyelids. Bottom right: a compact still life of material swatch spheres matching the actual feathers, beak/feet, accessory cloth, and glossy eye of this reference, plus one clear wing-root/feather overlap detail. No text, labels, logos or watermarks. Keep anatomy intact, feathered wings not human hands, no extra limbs, no cropping or overlapping panels, no injury. Character personality: <character suffix>
```

- `flash`：Athletic slender golden chicken with red headband and blue lightning wing markings; cocky, explosive, quick.
- `chubby`：Round heavy cream-gold chicken with teal bow tie, short orange legs, red comb; gentle, steady, belly bounce.
