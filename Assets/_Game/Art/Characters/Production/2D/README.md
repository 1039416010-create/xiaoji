# Phase 11 2D Character Production

The final character presentation is now a 2D hybrid pipeline. Approved refined
concept art remains the identity authority. Existing 3D checkpoints are kept as
archives and are not final game presentation assets.

## Runtime approach

- Side-view layered sprites and 2D bones for idle, warmup, run, sprint and slow.
- Character-specific replacement drawings for fall, recover, turn, interfere,
  celebrate and lose where skeletal deformation would damage the silhouette.
- `ChickenController.NormalizedProgress` remains the only race-position truth.
- Animator states never contain forward root displacement.
- Run and Sprint preserve normalized cycle phase when cross-fading, preventing
  a visible foot-cycle pop. One-shot actions always start at their authored
  beginning.

## Art acceptance

- Stable identity, palette, accessories and body proportions match the approved
  turnaround and character art bible.
- Every standing pose shares a consistent foot baseline and visual scale.
- Near/far wings, legs, eyelids, beak halves, tail groups and cloth accessories
  remain independently editable.
- Final runtime sprites use transparent backgrounds with clean alpha edges.
- Generated motion sheets are pose references until they are cleaned, layered
  and imported as production sprites.

## First benchmark

`Flash/MotionReference/CH_flash_motion_keyposes_v1.png` contains twelve Flash
motion keys generated from the approved turnaround. It is an art-direction
reference, not a directly shipped sprite atlas. The next art gate is a layered
Flash side-view master followed by a five-state playable animation benchmark.
