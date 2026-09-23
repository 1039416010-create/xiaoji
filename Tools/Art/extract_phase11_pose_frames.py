"""Extract approved Phase 11 pose-sheet art into transparent full-character frames.

The script never redraws, warps, mirrors, or rescales a character. It only removes
the neutral sheet background and places each authored pose on a fixed transparent
canvas so Unity can switch complete-character sprites frame by frame.
"""

from __future__ import annotations

from collections import deque
from dataclasses import dataclass
from pathlib import Path
import shutil

import numpy as np
from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
CONCEPTS = ROOT / "Docs" / "Art" / "Concepts" / "Phase11"
PRODUCTION = ROOT / "Assets" / "_Game" / "Art" / "Characters" / "Production" / "2D"
CANVAS_SIZE = (384, 512)


@dataclass(frozen=True)
class CharacterSheet:
    key: str
    source_name: str
    cells: tuple[tuple[int, int, int, int], ...]
    poses: dict[str, int]


CHUBBY = CharacterSheet(
    key="chubby",
    source_name="CH_chubby_pose_sheet_v1.png",
    cells=tuple(
        (column * 384, row * 512, (column + 1) * 384, (row + 1) * 512)
        for row in range(2)
        for column in range(4)
    ),
    poses={
        "idle": 0,
        "warmup": 1,
        "run_contact": 2,
        "run_passing": 3,
        "run_airborne": 3,
        "sprint": 3,
        "slowdown": 4,
        "fall": 5,
        "recover": 6,
        "turn": 0,
        "interfere": 4,
        "celebrate": 7,
    },
)


SLACKER = CharacterSheet(
    key="slacker",
    source_name="CH_slacker_pose_sheet_v1.png",
    cells=(
        (0, 0, 382, 335),
        (384, 0, 767, 335),
        (769, 0, 1151, 335),
        (1153, 0, 1536, 335),
        (0, 337, 382, 650),
        (384, 337, 767, 650),
        (769, 337, 1151, 650),
        (1153, 337, 1536, 650),
        (0, 652, 382, 1024),
        (384, 652, 767, 1024),
        (769, 652, 1151, 1024),
    ),
    poses={
        "idle": 0,
        "warmup": 1,
        "run_contact": 2,
        "run_passing": 3,
        "run_airborne": 3,
        "sprint": 3,
        "slowdown": 4,
        "fall": 5,
        "recover": 6,
        "turn": 7,
        "interfere": 8,
        "celebrate": 9,
        "lose": 10,
    },
)


STANDARD_ACTION_POSES = {
    "idle": 0,
    "warmup": 1,
    "run_contact": 2,
    "run_passing": 3,
    "run_airborne": 3,
    "sprint": 3,
    "slowdown": 4,
    "fall": 5,
    "recover": 6,
    "turn": 7,
    "interfere": 8,
    "celebrate": 9,
    "lose": 10,
}

# The approved Phase 11 action sheets share the same four-column layout. The
# two-pixel separators are deliberately excluded so no grid line enters a
# character frame. Row heights differ because the final row reserves more room
# for the expression reference cell.
STANDARD_ACTION_CELLS = (
    (3, 3, 379, 337),
    (387, 3, 763, 337),
    (771, 3, 1147, 337),
    (1155, 3, 1533, 337),
    (3, 345, 379, 647),
    (387, 345, 763, 647),
    (771, 345, 1147, 647),
    (1155, 345, 1533, 647),
    (3, 662, 379, 1021),
    (387, 662, 763, 1021),
    (771, 662, 1147, 1021),
)


def standard_action_sheet(key: str) -> CharacterSheet:
    return CharacterSheet(
        key=key,
        source_name=f"CH_{key}_pose_sheet_v1.png",
        cells=STANDARD_ACTION_CELLS,
        poses=STANDARD_ACTION_POSES,
    )


REMAINING_ROSTER = tuple(
    standard_action_sheet(key)
    for key in (
        "bro",
        "captain",
        "dumpling",
        "lucky",
        "ninja",
        "rocket",
        "scholar",
        "sleepy",
        "thunder",
        "tiny",
    )
)


def fit_background(rgb: np.ndarray) -> np.ndarray:
    """Fit the sheet's smooth neutral matte from clean edge samples."""
    height, width, _ = rgb.shape
    yy, xx = np.mgrid[0:height, 0:width]
    x = xx / max(width - 1, 1)
    y = yy / max(height - 1, 1)
    features = np.stack((np.ones_like(x), x, y, x * y, x * x, y * y), axis=-1)
    edge = max(5, min(width, height) // 40)
    sample_mask = (xx < edge) | (xx >= width - edge) | (yy < edge) | (yy >= height - edge)
    sample_features = features[sample_mask]
    sample_rgb = rgb[sample_mask]

    keep = np.ones(sample_rgb.shape[0], dtype=bool)
    coefficients = None
    for _ in range(3):
        coefficients = np.linalg.lstsq(sample_features[keep], sample_rgb[keep], rcond=None)[0]
        residual = np.linalg.norm(sample_features @ coefficients - sample_rgb, axis=1)
        median = np.median(residual)
        deviation = np.median(np.abs(residual - median)) + 1.0
        keep = residual <= median + 3.0 * deviation
    return np.clip(features @ coefficients, 0.0, 255.0)


def largest_component(mask: np.ndarray) -> np.ndarray:
    """Keep the authored character and discard isolated sheet/grid noise."""
    height, width = mask.shape
    visited = np.zeros_like(mask, dtype=bool)
    largest: list[tuple[int, int]] = []
    for start_y, start_x in zip(*np.nonzero(mask & ~visited)):
        if visited[start_y, start_x]:
            continue
        queue = deque([(start_y, start_x)])
        visited[start_y, start_x] = True
        component: list[tuple[int, int]] = []
        while queue:
            y, x = queue.popleft()
            component.append((y, x))
            for next_y, next_x in ((y - 1, x), (y + 1, x), (y, x - 1), (y, x + 1)):
                if 0 <= next_y < height and 0 <= next_x < width:
                    if mask[next_y, next_x] and not visited[next_y, next_x]:
                        visited[next_y, next_x] = True
                        queue.append((next_y, next_x))
        if len(component) > len(largest):
            largest = component
    output = np.zeros_like(mask, dtype=bool)
    if largest:
        ys, xs = zip(*largest)
        output[np.asarray(ys), np.asarray(xs)] = True
    return output


def extract_character(cell: Image.Image, conservative_matte: bool) -> Image.Image:
    rgb = np.asarray(cell.convert("RGB"), dtype=np.float32)
    background = fit_background(rgb)
    distance = np.linalg.norm(rgb - background, axis=2)
    chroma = rgb.max(axis=2) - rgb.min(axis=2)
    strength = np.maximum(distance, chroma * 0.72)
    if conservative_matte:
        alpha = np.clip((strength - 8.0) * (255.0 / 28.0), 0.0, 255.0)
        core_threshold = 82.0
    else:
        alpha = np.clip((strength - 6.0) * (255.0 / 22.0), 0.0, 255.0)
        core_threshold = 42.0

    # Generated concept sheets contain subtle compression/grain in the neutral
    # studio background. A conservative opaque-core threshold prevents that
    # texture from becoming connected matte islands around dark silhouettes.
    core = largest_component(alpha >= core_threshold)
    # Keep soft feather edges adjacent to the main opaque character.
    expanded = core.copy()
    for _ in range(3):
        expanded[1:, :] |= expanded[:-1, :]
        expanded[:-1, :] |= expanded[1:, :]
        expanded[:, 1:] |= expanded[:, :-1]
        expanded[:, :-1] |= expanded[:, 1:]
    alpha *= expanded
    alpha[alpha < (24.0 if conservative_matte else 10.0)] = 0.0

    # The concept sheets include a neutral floor contact shadow. It is not part
    # of the character and would otherwise form a pale stripe across a race lane.
    yy = np.arange(rgb.shape[0])[:, None]
    if conservative_matte:
        brightness = rgb.mean(axis=2)
        floor_matte = (
            (yy >= int(rgb.shape[0] * 0.84))
            & (chroma < 65.0)
            & (brightness < 195.0)
        )
    else:
        floor_matte = (yy >= int(rgb.shape[0] * 0.84)) & (chroma < 25.0)
    alpha[floor_matte] = 0.0

    # Remove the neutral matte colour from soft feather-edge pixels without
    # changing opaque authored pixels.
    opacity = alpha[..., None] / 255.0
    safe_opacity = np.maximum(opacity, 0.18)
    foreground = (rgb - background * (1.0 - opacity)) / safe_opacity
    rgb = np.where(opacity < 0.995, foreground, rgb)
    rgb = np.clip(rgb, 0.0, 255.0)

    rgba = np.dstack((rgb.astype(np.uint8), alpha.astype(np.uint8)))
    return Image.fromarray(rgba, "RGBA")


def place_on_canvas(frame: Image.Image) -> Image.Image:
    canvas = Image.new("RGBA", CANVAS_SIZE, (0, 0, 0, 0))
    alpha = np.asarray(frame.getchannel("A"))
    nonzero = np.argwhere(alpha > 10)
    if nonzero.size == 0:
        raise RuntimeError("Background extraction removed the entire authored pose.")
    if frame.width > canvas.width or frame.height > canvas.height:
        raise RuntimeError(f"Authored pose does not fit fixed canvas: {frame.size}")
    # Preserve the pose sheet's exact within-cell position. Cells are only
    # centered horizontally and bottom-aligned to a shared transparent canvas.
    x = (canvas.width - frame.width) // 2
    y = canvas.height - frame.height
    canvas.alpha_composite(frame, (x, y))
    return canvas


def extract_sheet(spec: CharacterSheet) -> None:
    source = CONCEPTS / spec.source_name
    target_root = PRODUCTION / spec.key.capitalize()
    reference_dir = target_root / "MotionReference"
    frames_dir = target_root / "FrameAnimation" / "Frames"
    reference_dir.mkdir(parents=True, exist_ok=True)
    frames_dir.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source, reference_dir / source.name)

    sheet = Image.open(source).convert("RGB")
    extracted: dict[int, Image.Image] = {}
    for index in sorted(set(spec.poses.values())):
        conservative_matte = spec in REMAINING_ROSTER
        extracted[index] = place_on_canvas(
            extract_character(sheet.crop(spec.cells[index]), conservative_matte)
        )
    for pose_name, index in spec.poses.items():
        output = frames_dir / f"CH_{spec.key}_{pose_name}_v1.png"
        extracted[index].save(output, optimize=True)
        print(output.relative_to(ROOT))


def main() -> None:
    extract_sheet(CHUBBY)
    extract_sheet(SLACKER)
    for spec in REMAINING_ROSTER:
        extract_sheet(spec)


if __name__ == "__main__":
    main()
