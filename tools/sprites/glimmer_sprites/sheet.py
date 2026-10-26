"""Упаковка листа, метаданные (content/art/sprites/<id>.json) и индекс листов со свежестью.

Лист — индексированный PNG в мастер-палитре: ряд на пару (клип, направление), столбец на кадр, ячейки одного
размера. Зеркальные направления не рендерятся и не хранятся — игра отражает ряд (ADR 0003: 5 + зеркало).
Индекс хранит хэши листа, метаданных и исходников конвейера: правка модели, стиля или палитры без перерендера
ловится тестом свежести, а не ошибкой загрузки в игре.
"""

from __future__ import annotations

import json
from pathlib import Path

import numpy as np
from PIL import Image

from . import build
from .palette import PALETTE_PATH, REPO, Palette, validate_schema

INDEX_PATH = build.SPRITES_META_DIR / "index.json"

# Всё, от чего зависят пиксели листа. QA и тесты сюда не входят: их правка лист не меняет.
SOURCES = [
    "tools/sprites/blender/models.py",
    "tools/sprites/blender/render.py",
    "tools/sprites/glimmer_sprites/build.py",
    "tools/sprites/glimmer_sprites/sheet.py",
    "tools/sprites/glimmer_sprites/palette.py",
    "tools/sprites/glimmer_sprites/color.py",
    PALETTE_PATH.relative_to(REPO).as_posix(),
]

GRIDS = {1.0: [640, 360], 1.5: [960, 540]}


def sources_digest() -> dict:
    return {path: build.sha256(REPO / path) for path in SOURCES}


def pack(frames: build.Frames, sheet_id: str, palette: Palette, style: build.Style):
    manifest = frames.manifest
    every = [f for clip in frames.clips.values() for seq in clip.values() for f in seq]
    mask = np.zeros_like(every[0], dtype=bool)
    for f in every:
        mask |= f > 0
    ys, xs = np.nonzero(mask)
    y0, y1, x0, x1 = int(ys.min()), int(ys.max()) + 1, int(xs.min()), int(xs.max()) + 1
    cell_w, cell_h = x1 - x0, y1 - y0
    columns = max(len(seq) for clip in frames.clips.values() for seq in clip.values())
    rows = sum(len(clip) for clip in frames.clips.values())
    sheet = np.zeros((rows * cell_h, columns * cell_w), dtype=np.uint8)
    clips = {}
    row = 0
    for name, by_dir in frames.clips.items():
        spec = manifest["clips"][name]
        clips[name] = {"row": row, "frames": spec["frames"], "fps": spec["fps"], "loop": spec["loop"], "dirs": spec["dirs"]}
        if spec["phases"]:
            clips[name]["phases"] = spec["phases"]
        for direction in spec["dirs"]:
            for f, frame in enumerate(by_dir[direction]):
                sheet[row * cell_h:(row + 1) * cell_h, f * cell_w:(f + 1) * cell_w] = frame[y0:y1, x0:x1]
            row += 1
    anchor_x, anchor_y = manifest["anchor"]
    meta = {
        "$schema": "../../schema/sprite-sheet.schema.json",
        "id": sheet_id,
        "image": f"{sheet_id}.png",
        "grid": GRIDS.get(manifest["scale"], [round(640 * manifest["scale"]), round(360 * manifest["scale"])]),
        "cell": [cell_w, cell_h],
        "columns": columns,
        "anchor": [anchor_x + 1 - x0, anchor_y + 1 - y0],     # +1 — поле под контур
        "palette": f"{palette.name}@{palette.version}",
        "directions": {"rendered": manifest["directions"], "mirror": manifest["mirror"]},
        "clips": clips,
        "source": {
            "kind": "procedural",
            "class": manifest.get("class", "character"),
            "model": manifest["model"],
            "note": manifest["note"],
            "scale": manifest["scale"],
            "target_px": manifest["target_px"],
            "supersample": manifest["supersample"],
            "pixels_per_meter": round(manifest["pixels_per_meter"], 4),
            "renderer": f"Blender {manifest['blender']} Cycles",
            "pipeline": build.PIPELINE_VERSION,
            "style": style.as_dict(),
        },
    }
    return sheet, meta


def save_png(indexed: np.ndarray, palette: Palette, path: Path) -> None:
    image = Image.fromarray(indexed, mode="P")
    image.putpalette(palette.pil_palette())
    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path, optimize=True, transparency=0)


def load_png(path: Path) -> np.ndarray:
    with Image.open(path) as image:
        if image.mode != "P":
            raise ValueError(f"{path}: ожидается индексированный PNG, а не {image.mode}")
        return np.asarray(image)


def write_sheet(sheet: np.ndarray, meta: dict, palette: Palette) -> tuple[Path, Path]:
    validate_schema(meta, "sprite-sheet.schema.json")
    image_path = build.SPRITES_IMAGE_DIR / meta["image"]
    meta_path = build.SPRITES_META_DIR / f"{meta['id']}.json"
    save_png(sheet, palette, image_path)
    meta_path.parent.mkdir(parents=True, exist_ok=True)
    meta_path.write_text(json.dumps(meta, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")
    return image_path, meta_path


def load_index() -> dict:
    if not INDEX_PATH.exists():
        return {"pipeline": build.PIPELINE_VERSION, "sources": {}, "sheets": {}}
    return json.loads(INDEX_PATH.read_text(encoding="utf-8"))


def update_index(sheet_ids: list[str]) -> dict:
    """Записывает хэши листов и исходников. Листы не из этого прогона тоже перехэшируются: исходники общие."""
    index = load_index()
    index["pipeline"] = build.PIPELINE_VERSION
    index["sources"] = sources_digest()
    for sheet_id in sheet_ids:
        index["sheets"][sheet_id] = {}
    for sheet_id in sorted(index["sheets"]):
        meta_path = build.SPRITES_META_DIR / f"{sheet_id}.json"
        meta = json.loads(meta_path.read_text(encoding="utf-8"))
        image_path = build.SPRITES_IMAGE_DIR / meta["image"]
        index["sheets"][sheet_id] = {
            "meta_sha256": build.sha256(meta_path),
            "image_sha256": build.sha256(image_path),
        }
    index["sheets"] = dict(sorted(index["sheets"].items()))
    INDEX_PATH.write_text(json.dumps(index, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")
    return index


def freshness_problems() -> list[str]:
    """Пустой список — все листы на месте и собраны из текущих исходников."""
    index = load_index()
    out = []
    if index.get("pipeline") != build.PIPELINE_VERSION:
        out.append(f"версия конвейера в индексе {index.get('pipeline')} ≠ {build.PIPELINE_VERSION}")
    current = sources_digest()
    for path, digest in current.items():
        if index["sources"].get(path) != digest:
            out.append(f"исходник {path} изменён после сборки листов — нужен make sprites")
    for path in set(index["sources"]) - set(current):
        out.append(f"в индексе лишний исходник {path}")
    metas = {p.stem for p in build.SPRITES_META_DIR.glob("*.json") if p.name != "index.json"}
    images = {p.stem for p in build.SPRITES_IMAGE_DIR.glob("*.png")}
    for sheet_id, entry in index["sheets"].items():
        meta_path = build.SPRITES_META_DIR / f"{sheet_id}.json"
        if not meta_path.exists():
            out.append(f"{sheet_id}: нет метаданных {meta_path.relative_to(REPO)}")
            continue
        meta = json.loads(meta_path.read_text(encoding="utf-8"))
        image_path = build.SPRITES_IMAGE_DIR / meta["image"]
        if not image_path.exists():
            out.append(f"{sheet_id}: нет листа {image_path.relative_to(REPO)}")
            continue
        if build.sha256(meta_path) != entry["meta_sha256"]:
            out.append(f"{sheet_id}: метаданные правились мимо конвейера")
        if build.sha256(image_path) != entry["image_sha256"]:
            out.append(f"{sheet_id}: лист правился мимо конвейера")
    out.extend(f"метаданные {m}.json не в индексе" for m in sorted(metas - set(index["sheets"])))
    listed_images = set()
    for sheet_id in index["sheets"]:
        meta_path = build.SPRITES_META_DIR / f"{sheet_id}.json"
        if meta_path.exists():
            listed_images.add(Path(json.loads(meta_path.read_text(encoding="utf-8"))["image"]).stem)
    out.extend(f"лист {i}.png не в индексе" for i in sorted(images - listed_images))
    return out


def read_frames(meta: dict) -> dict:
    """Лист → {клип: {направление: [кадры]}} — для QA и тестов."""
    sheet = load_png(build.SPRITES_IMAGE_DIR / meta["image"])
    cell_w, cell_h = meta["cell"]
    out = {}
    for name, clip in meta["clips"].items():
        out[name] = {}
        for k, direction in enumerate(clip["dirs"]):
            r = clip["row"] + k
            out[name][direction] = [sheet[r * cell_h:(r + 1) * cell_h, f * cell_w:(f + 1) * cell_w] for f in range(clip["frames"])]
    return out
