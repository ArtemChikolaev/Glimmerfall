"""Читаемость кадра без цвета (ADR 0003 п. 1, BACKLOG G2.6): серый скриншот + маска силуэтов того же кадра.

Игра снимает кадр и тот же кадр маской (`--screenshot`): герой — чистый красный, рядовые — зелёный, элиты — синий.
Контраст класса — медиана разницы OKLab-светлоты на границе силуэта: пиксель силуэта против соседа снаружи.
Медиана всего силуэта не годится: тёмный контур и светлая середина в среднем могут совпасть с полом, хотя силуэт
виден отлично. Порог 0,10 — заметная разница светлоты; цветом её не подменить, потому что считаем по серому.
"""

from __future__ import annotations

import json
from pathlib import Path

import numpy as np
from PIL import Image

from . import color

CLASSES = {"hero": (255, 0, 0), "enemy": (0, 255, 0), "elite": (0, 0, 255)}
RING_PX = 2
MIN_CONTRAST = 0.10
LIGHT_ABOVE_FLOOR = 0.15   # «светлый ключ»: пиксель силуэта заметно светлее пола
MIN_KEY_GAP = 0.15         # герой отличим от орды без цвета: его светлый ключ больше на столько (доля силуэта)


def lightness(rgb: np.ndarray) -> np.ndarray:
    return color.srgb_to_oklab(rgb[..., :3] / 255.0)[..., 0]


def dilate(mask: np.ndarray, radius: int) -> np.ndarray:
    out = mask.copy()
    h, w = mask.shape
    padded = np.pad(mask, radius)
    for dy in range(2 * radius + 1):
        for dx in range(2 * radius + 1):
            out |= padded[dy:dy + h, dx:dx + w]
    return out


def edge_contrast(gray: np.ndarray, mask: np.ndarray) -> float:
    """Медиана |L внутри − L снаружи| по парам пикселей через границу силуэта (4-соседство)."""
    diffs = []
    for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
        inside = mask[max(-dy, 0):mask.shape[0] - max(dy, 0), max(-dx, 0):mask.shape[1] - max(dx, 0)]
        outside = ~mask[max(dy, 0):mask.shape[0] - max(-dy, 0), max(dx, 0):mask.shape[1] - max(-dx, 0)]
        g_in = gray[max(-dy, 0):gray.shape[0] - max(dy, 0), max(-dx, 0):gray.shape[1] - max(dx, 0)]
        g_out = gray[max(dy, 0):gray.shape[0] - max(-dy, 0), max(dx, 0):gray.shape[1] - max(-dx, 0)]
        pair = inside & outside
        diffs.append(np.abs(g_in[pair] - g_out[pair]))
    values = np.concatenate(diffs)
    return float(np.median(values)) if values.size else 0.0


def analyse(prefix: Path) -> dict:
    shot = np.asarray(Image.open(f"{prefix}.png").convert("RGB")).astype(np.int32)
    mask = np.asarray(Image.open(f"{prefix}.mask.png").convert("RGB")).astype(np.int32)
    info = json.loads(Path(f"{prefix}.json").read_text(encoding="utf-8"))
    gray = lightness(shot)
    masks = {name: np.all(mask == rgb, axis=-1) for name, rgb in CLASSES.items()}
    anything = np.zeros(gray.shape, dtype=bool)
    for m in masks.values():
        anything |= m
    floor = float(np.median(gray[~dilate(anything, RING_PX)])) if (~anything).any() else 0.0
    result = {"grid": info["grid"], "visible_enemies": info["visible_enemies"], "floor_L": round(floor, 3), "classes": {}}
    for name, m in masks.items():
        if not m.any():
            continue
        ring = dilate(m, RING_PX) & ~m
        inside = float(np.median(gray[m]))
        around = float(np.median(gray[ring])) if ring.any() else floor
        edge = edge_contrast(gray, m)
        result["classes"][name] = {
            "pixels": int(m.sum()),
            "L": round(inside, 3),
            "ring_L": round(around, 3),
            "contrast": round(edge, 3),
            "light_share": round(float((gray[m] > floor + LIGHT_ABOVE_FLOOR).mean()), 3),
            "readable": edge >= MIN_CONTRAST,
        }
    classes = result["classes"]
    if "hero" in classes and "enemy" in classes:
        gap = classes["hero"]["light_share"] - classes["enemy"]["light_share"]
        result["hero_key_gap"] = round(gap, 3)
        result["hero_separable"] = gap >= MIN_KEY_GAP
    return result


def gray_image(prefix: Path) -> Image.Image:
    shot = np.asarray(Image.open(f"{prefix}.png").convert("RGB"))
    value = color.linear_to_srgb(np.clip(lightness(shot), 0, 1) ** 3)
    return Image.fromarray((np.clip(value, 0, 1) * 255).round().astype(np.uint8), "L")
