"""Проходы Blender → индексированные кадры → лист и метаданные (G2.3).

Порядок на кадр (ADR 0003 п. 5, скилл sprite-pipeline):
  1. свет по нормали в осях камеры: ключевой сверху слева, 3 полосы тона (тень / свет / блик);
  2. ключ (материал, тон) → даунскейл блоками K×K: прозрачность по покрытию, цвет — мода блока;
  3. внутренние линии по скачку глубины и контур — уже после даунскейла, в целевых пикселях;
  4. (материал, тон) → индекс мастер-палитры: квантование структурное, адаптивной палитры нет.
"""

from __future__ import annotations

import hashlib
import json
from dataclasses import dataclass, field
from pathlib import Path

import numpy as np
from PIL import Image

from .palette import REPO, Palette

SPRITES_IMAGE_DIR = REPO / "game" / "art" / "sprites"
SPRITES_META_DIR = REPO / "content" / "art" / "sprites"
PIPELINE_VERSION = 1


def _unit(v):
    v = np.asarray(v, dtype=np.float64)
    return v / np.linalg.norm(v)


@dataclass(frozen=True)
class Style:
    key_light: tuple = tuple(_unit((-0.55, 0.65, 0.52)))   # x вправо, y вверх, z к зрителю: сверху слева
    shadow_below: float = 0.30      # освещённость ниже — тон «тень»
    highlight_above: float = 0.82   # выше — «блик»
    glow_core_above: float = 0.55   # светящиеся материалы: N·V выше — ядро рампы
    coverage: float = 0.5           # доля покрытых субпикселей, с которой пиксель непрозрачен
    line_depth_m: float = 0.10      # скачок глубины к соседу, с которого дальний пиксель уходит в тёмный тон
    despeckle_px: int = 2           # одиночные вспышки до стольких пикселей убираются по соседним кадрам (0 — выкл.)
    outline: bool = True

    def as_dict(self) -> dict:
        return {k: (list(v) if isinstance(v, tuple) else v) for k, v in self.__dict__.items()}


TONE_DEEP, TONE_SHADOW, TONE_LIT, TONE_HIGHLIGHT = 0, 1, 2, 3


def read_rgba(path: Path) -> np.ndarray:
    with Image.open(path) as image:
        return np.asarray(image.convert("RGBA"))


def shade_tones(normal: np.ndarray, material: np.ndarray, materials: dict, style: Style):
    """Супер-сэмплы → (покрытие, номер материала, тон, глубина 0..1)."""
    covered = material[..., 3] > 127
    mat = material[..., 0].astype(np.int32)
    depth = material[..., 2].astype(np.float64) / 255.0
    n = normal[..., :3].astype(np.float64) / 255.0 * 2.0 - 1.0
    n[..., 2] = -n[..., 2]                                  # в Blender z камеры смотрит от зрителя
    n /= np.maximum(np.linalg.norm(n, axis=-1, keepdims=True), 1e-6)
    light = np.clip(n @ np.array(style.key_light), 0.0, 1.0)
    tone = np.where(light < style.shadow_below, TONE_SHADOW, np.where(light > style.highlight_above, TONE_HIGHLIGHT, TONE_LIT))
    glow_ids = [int(k) for k, v in materials.items() if v["glow"]]
    if glow_ids:
        glowing = np.isin(mat, glow_ids)
        tone = np.where(glowing, np.where(n[..., 2] > style.glow_core_above, TONE_HIGHLIGHT, TONE_LIT), tone)
    mat = np.where(covered, mat, 0)
    return covered, mat, tone.astype(np.int32), depth


def downscale(covered, mat, tone, depth, k: int, coverage: float):
    """Блоки k×k: непрозрачность по доле покрытия, (материал, тон) — мода блока, глубина — ближайшая."""
    h, w = covered.shape[0] // k, covered.shape[1] // k

    def blocks(a):
        return a.reshape(h, k, w, k).transpose(0, 2, 1, 3).reshape(h, w, k * k)

    cov = blocks(covered)
    opaque = cov.sum(axis=-1) >= coverage * k * k
    key = np.where(covered, mat * 8 + tone, -1)
    kb = blocks(key)
    values = np.unique(key[key >= 0])
    best = np.full((h, w), -1, dtype=np.int32)
    best_count = np.zeros((h, w), dtype=np.int32)
    for value in values:                        # значений немного: материалов ≤ 31, тонов 4
        count = (kb == value).sum(axis=-1)
        better = count > best_count
        best = np.where(better, value, best)
        best_count = np.where(better, count, best_count)
    near = np.where(cov, blocks(depth), 2.0).min(axis=-1)
    best = np.where(opaque, best, -1)
    return opaque, best // 8, best % 8, np.where(opaque, near, 2.0)


def inner_lines(opaque, tone, depth, threshold):
    """Дальний пиксель на скачке глубины к непрозрачному соседу получает тёмный тон рампы."""
    tone = tone.copy()
    far = np.zeros_like(opaque)
    for dy, dx in ((-1, 0), (1, 0), (0, -1), (0, 1)):
        neighbour_depth = np.full_like(depth, 2.0)
        neighbour_opaque = np.zeros_like(opaque)
        src_y = slice(max(dy, 0), depth.shape[0] + min(dy, 0))
        dst_y = slice(max(-dy, 0), depth.shape[0] + min(-dy, 0))
        src_x = slice(max(dx, 0), depth.shape[1] + min(dx, 0))
        dst_x = slice(max(-dx, 0), depth.shape[1] + min(-dx, 0))
        neighbour_depth[dst_y, dst_x] = depth[src_y, src_x]
        neighbour_opaque[dst_y, dst_x] = opaque[src_y, src_x]
        far |= opaque & neighbour_opaque & (depth - neighbour_depth > threshold)
    tone[far] = TONE_DEEP
    return tone


def components(mask: np.ndarray) -> list[list[tuple[int, int]]]:
    """8-связные компоненты маски — списки пикселей (y, x)."""
    h, w = mask.shape
    seen = np.zeros_like(mask, dtype=bool)
    out = []
    for y0, x0 in zip(*np.nonzero(mask)):
        if seen[y0, x0]:
            continue
        seen[y0, x0] = True
        pixels = [(int(y0), int(x0))]
        i = 0
        while i < len(pixels):
            y, x = pixels[i]
            i += 1
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    ny, nx = y + dy, x + dx
                    if 0 <= ny < h and 0 <= nx < w and mask[ny, nx] and not seen[ny, nx]:
                        seen[ny, nx] = True
                        pixels.append((ny, nx))
        out.append(pixels)
    return out


def triples(n: int, loop: bool):
    """Тройки соседних кадров (t−1, t, t+1): у цикла — по кругу, у разового клипа — только внутренние."""
    if loop and n >= 3:
        return [((t - 1) % n, t, (t + 1) % n) for t in range(n)]
    return [(t - 1, t, t + 1) for t in range(1, n - 1)]


MOTION_RADIUS = 2


def _shifted(a: np.ndarray, dy: int, dx: int, fill: int = -1) -> np.ndarray:
    """out[y, x] = a[y − dy, x − dx] (за краем — fill)."""
    out = np.full(a.shape, fill, dtype=np.int64)
    h, w = a.shape
    out[max(dy, 0):h + min(dy, 0), max(dx, 0):w + min(dx, 0)] = a[max(-dy, 0):h + min(-dy, 0), max(-dx, 0):w + min(-dx, 0)]
    return out


def passing_through(prev: np.ndarray, frame: np.ndarray, nxt: np.ndarray) -> np.ndarray:
    """Значение пикселя «проходит» через него: в t−1 оно было на d с одной стороны, в t+1 — на d с другой
    (|d| ≤ MOTION_RADIUS). Так выглядит кончик клинка или край, который движется, а не мигает."""
    out = np.zeros(frame.shape, dtype=bool)
    value = frame.astype(np.int64)
    for dy in range(-MOTION_RADIUS, MOTION_RADIUS + 1):
        for dx in range(-MOTION_RADIUS, MOTION_RADIUS + 1):
            if dy or dx:
                out |= (_shifted(prev, dy, dx) == value) & (_shifted(nxt, -dy, -dx) == value)
    return out


def blips(prev: np.ndarray, frame: np.ndarray, nxt: np.ndarray, max_px: int):
    """Одиночные вспышки кадра: пятна ≤ max_px, где кадр отличается от соседей, а соседи равны между собой
    («A-B-A»), и пятно не объясняется движением сквозь пиксель. Остаются кипение полос света, крошки на краях
    и «хлопок» пика, попавшего ровно на кадр."""
    mask = (prev == nxt) & (frame != prev)
    if not mask.any():
        return []
    moving = passing_through(prev, frame, nxt)
    return [c for c in components(mask) if len(c) <= max_px and not all(moving[y, x] for y, x in c)]


def despeckle(frames: list[np.ndarray], loop: bool, max_px: int) -> list[np.ndarray]:
    """Временная чистка «кипения» 3D-рендера: вспышка на один кадр получает значение соседних кадров.
    Поиск — по исходным кадрам, замена — одновременно, поэтому порядок кадров на результат не влияет."""
    if max_px <= 0:
        return frames
    out = [f.copy() for f in frames]
    for a, b, c in triples(len(frames), loop):
        for pixels in blips(frames[a], frames[b], frames[c], max_px):
            ys, xs = zip(*pixels)
            out[b][ys, xs] = frames[a][ys, xs]
    return out


def outline_mask(opaque: np.ndarray) -> np.ndarray:
    """Прозрачные пиксели, касающиеся силуэта по 4-соседству."""
    padded = np.pad(opaque, 1)
    touch = padded[:-2, 1:-1] | padded[2:, 1:-1] | padded[1:-1, :-2] | padded[1:-1, 2:]
    return touch & ~opaque


def to_indices(opaque, mat, tone, materials: dict, palette: Palette) -> np.ndarray:
    lut = np.zeros((64, 8), dtype=np.uint8)
    for key, spec in materials.items():
        tones = palette.ramp_indices(spec["ramp"])
        for t in range(8):
            if spec["glow"]:
                # светящееся: «свет» — предпоследний тон, «блик» — ядро; тени у света нет
                lut[int(key), t] = tones[-1] if t == TONE_HIGHLIGHT else tones[-2]
            else:
                lut[int(key), t] = tones[min(t, len(tones) - 1)] if len(tones) >= 4 else tones[max(0, min(t - 1, len(tones) - 1))]
    out = lut[np.clip(mat, 0, 63), np.clip(tone, 0, 7)]
    return np.where(opaque, out, 0).astype(np.uint8)


def process_frame(normal_path: Path, material_path: Path, manifest: dict, palette: Palette, style: Style):
    k = manifest["supersample"]
    normal = read_rgba(normal_path)
    material = read_rgba(material_path)
    covered, mat, tone, depth = shade_tones(normal, material, manifest["materials"], style)
    edge = covered[0, :].any() or covered[-1, :].any() or covered[:, 0].any() or covered[:, -1].any()
    opaque, mat_px, tone_px, depth_px = downscale(covered, mat, tone, depth, k, style.coverage)
    tone_px = inner_lines(opaque, tone_px, depth_px, style.line_depth_m / manifest["depth_range_m"])
    indexed = to_indices(opaque, mat_px, tone_px, manifest["materials"], palette)
    return indexed, bool(edge)


@dataclass
class Frames:
    """Кадры листа: клип → направление → список индексированных кадров (холст + поле 1 px под контур)."""
    manifest: dict
    clips: dict = field(default_factory=dict)
    cut: list = field(default_factory=list)


def render_frames(raw_dir: Path, palette: Palette, style: Style) -> Frames:
    manifest = json.loads((raw_dir / "frames.json").read_text(encoding="utf-8"))
    result = Frames(manifest)
    outline_index = palette.index(palette.roles["outline"])
    for clip, spec in manifest["clips"].items():
        result.clips[clip] = {}
        for direction in spec["dirs"]:
            frames = []
            for f in range(spec["frames"]):
                base = raw_dir / clip / direction / f"{f:02d}"
                indexed, cut = process_frame(Path(f"{base}_n.png"), Path(f"{base}_m.png"), manifest, palette, style)
                if cut:
                    result.cut.append(f"{clip}/{direction}/{f}")
                frames.append(np.pad(indexed, 1))
            frames = despeckle(frames, spec["loop"], style.despeckle_px)
            if style.outline:
                for indexed in frames:
                    indexed[outline_mask(indexed > 0)] = outline_index
            result.clips[clip][direction] = frames
    return result


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()
