"""Мастер-палитра (content/art/palette.json): загрузка по схеме, правила стиль-библии, квантование и экспорт.

Индексы: 0 — прозрачный, цвета палитры — с 1 в порядке `colors`. Так кадры хранятся индексированными PNG,
и «все пиксели из палитры» верно по построению.
"""

from __future__ import annotations

import json
from dataclasses import dataclass
from functools import cached_property
from pathlib import Path

import numpy as np

from . import color

REPO = Path(__file__).resolve().parents[3]
PALETTE_PATH = REPO / "content" / "art" / "palette.json"
GPL_PATH = REPO / "content" / "art" / "palette.gpl"
SCHEMA_DIR = REPO / "content" / "schema"

# Правила стиль-библии (docs/art/style-bible.md), проверяются тестом и командой `palette`.
MIN_RAMP_STEP = 0.04        # соседние тона рампы отличаются по светлоте хотя бы на столько
MIN_PAIR_DISTANCE = 0.025   # два цвета ближе этого — пустая трата слота палитры
VFX_CORE_LIGHTNESS = 0.90   # у каждой рампы эффектов есть светящееся ядро
TELEGRAPH_MIN_DISTANCE = 0.12
TELEGRAPH_MIN_HUE = 35.0    # градусов от любого насыщенного (C ≥ 0.08) цвета эффектов игрока


def validate_schema(document: dict, schema_name: str) -> None:
    import jsonschema

    schema = json.loads((SCHEMA_DIR / schema_name).read_text(encoding="utf-8"))
    jsonschema.validate(document, schema)


@dataclass(frozen=True)
class Ramp:
    name: str
    use: str
    tones: tuple[str, ...]
    note: str


class Palette:
    def __init__(self, document: dict):
        self.document = document
        self.name: str = document["name"]
        self.version: str = document["version"]
        self.ids: tuple[str, ...] = tuple(document["colors"])
        self.hexes: tuple[str, ...] = tuple(document["colors"].values())
        self.ramps: dict[str, Ramp] = {
            name: Ramp(name, r["use"], tuple(r["tones"]), r["note"]) for name, r in document["ramps"].items()
        }
        self.roles: dict[str, str] = dict(document["roles"])
        self.lightness: dict[str, tuple[float, float]] = {k: (v[0], v[1]) for k, v in document["lightness"].items()}
        self._index = {cid: i + 1 for i, cid in enumerate(self.ids)}

    @cached_property
    def srgb(self) -> np.ndarray:
        return np.array([color.hex_to_srgb(h) for h in self.hexes])

    @cached_property
    def rgb8(self) -> np.ndarray:
        return np.array([[int(h[i:i + 2], 16) for i in (1, 3, 5)] for h in self.hexes], dtype=np.uint8)

    @cached_property
    def lab(self) -> np.ndarray:
        return color.srgb_to_oklab(self.srgb)

    def index(self, color_id: str) -> int:
        """Индекс цвета в индексированном кадре (с 1)."""
        return self._index[color_id]

    def ramp_indices(self, ramp: str) -> list[int]:
        return [self._index[t] for t in self.ramps[ramp].tones]

    def lightness_of(self, color_id: str) -> float:
        return float(self.lab[self._index[color_id] - 1, 0])

    def quantize(self, rgba: np.ndarray, alpha_threshold: int = 128) -> np.ndarray:
        """RGBA uint8 (H, W, 4) → индексы (H, W): ближайший цвет палитры в OKLab, без дизеринга."""
        h, w = rgba.shape[:2]
        flat = rgba.reshape(-1, 4)
        lab = color.srgb_to_oklab(flat[:, :3] / 255.0)
        out = np.zeros(h * w, dtype=np.uint8)
        opaque = flat[:, 3] >= alpha_threshold
        if opaque.any():
            d = ((lab[opaque, None, :] - self.lab[None, :, :]) ** 2).sum(axis=2)
            out[opaque] = np.argmin(d, axis=1).astype(np.uint8) + 1
        return out.reshape(h, w)

    def to_rgba(self, indexed: np.ndarray) -> np.ndarray:
        table = np.zeros((len(self.ids) + 1, 4), dtype=np.uint8)
        table[1:, :3] = self.rgb8
        table[1:, 3] = 255
        return table[indexed]

    def pil_palette(self) -> list[int]:
        """Палитра для PNG режима «P»: индекс 0 — прозрачный (#000000 с tRNS), далее цвета палитры."""
        flat = [0, 0, 0]
        for rgb in self.rgb8:
            flat.extend(int(c) for c in rgb)
        return flat + [0] * (768 - len(flat))

    def gpl(self) -> str:
        """Палитра для Aseprite/GIMP; порядок совпадает с индексами кадров."""
        lines = ["GIMP Palette", f"Name: {self.name} {self.version}", "Columns: 8", "#"]
        for cid, rgb in zip(self.ids, self.rgb8):
            lines.append(f"{rgb[0]:3d} {rgb[1]:3d} {rgb[2]:3d}\t{cid}")
        return "\n".join(lines) + "\n"

    def problems(self) -> list[str]:
        """Нарушения правил стиль-библии; пустой список — палитра годна."""
        out: list[str] = []
        known = set(self.ids)
        if len(set(self.hexes)) != len(self.hexes):
            out.append("одинаковые hex у разных id")
        used: set[str] = set(self.roles.values())
        for ramp in self.ramps.values():
            missing = [t for t in ramp.tones if t not in known]
            if missing:
                out.append(f"рампа {ramp.name}: нет цветов {missing}")
                continue
            used.update(ramp.tones)
            lightness = [self.lightness_of(t) for t in ramp.tones]
            for a, b, ta, tb in zip(lightness, lightness[1:], ramp.tones, ramp.tones[1:]):
                if b - a < MIN_RAMP_STEP:
                    out.append(f"рампа {ramp.name}: {ta}→{tb} светлеет на {b - a:.3f} < {MIN_RAMP_STEP}")
            lo, hi = self.lightness[ramp.use]
            for t, value in zip(ramp.tones, lightness):
                if not lo <= value <= hi:
                    out.append(f"рампа {ramp.name}: {t} L={value:.3f} вне плана светлоты {ramp.use} [{lo}, {hi}]")
            if ramp.use == "vfx" and lightness[-1] < VFX_CORE_LIGHTNESS:
                out.append(f"рампа {ramp.name}: ядро L={lightness[-1]:.3f} < {VFX_CORE_LIGHTNESS}")
        for role, cid in self.roles.items():
            if cid not in known:
                out.append(f"роль {role}: нет цвета {cid}")
        out.extend(f"цвет {cid} не входит ни в одну рампу или роль" for cid in self.ids if cid not in used)
        if self.roles.get("outline") in known:
            darkest = self.ids[int(np.argmin(self.lab[:, 0]))]
            if darkest != self.roles["outline"]:
                out.append(f"контур {self.roles['outline']} не самый тёмный цвет ({darkest})")
        out.extend(self._telegraph_problems())
        lab = self.lab
        for i in range(len(self.ids)):
            d = np.linalg.norm(lab[i + 1:] - lab[i], axis=1)
            for j in np.nonzero(d < MIN_PAIR_DISTANCE)[0]:
                out.append(f"{self.ids[i]} и {self.ids[i + 1 + j]} почти одинаковы (ΔE {d[j]:.3f})")
        return out

    def _telegraph_problems(self) -> list[str]:
        out: list[str] = []
        telegraph = {t for r in self.ramps.values() if r.use == "telegraph" for t in r.tones}
        if not telegraph:
            return ["нет рампы телеграфов"]
        for r in self.ramps.values():
            if r.use != "telegraph" and telegraph & set(r.tones):
                out.append(f"цвет телеграфа в рампе {r.name}: он зарезервирован")
        vfx = {t for r in self.ramps.values() if r.use == "vfx" for t in r.tones}
        lch = color.oklab_to_oklch(self.lab)
        for t in sorted(telegraph):
            ti = self._index[t] - 1
            for v in sorted(vfx):
                vi = self._index[v] - 1
                distance = float(np.linalg.norm(self.lab[ti] - self.lab[vi]))
                if distance < TELEGRAPH_MIN_DISTANCE:
                    out.append(f"телеграф {t} близок к эффекту {v} (ΔE {distance:.3f})")
                if lch[vi, 1] >= 0.08 and color.hue_distance(lch[ti, 2], lch[vi, 2]) < TELEGRAPH_MIN_HUE:
                    out.append(f"телеграф {t} по тону близок к эффекту {v}")
        return out

    def preview(self, swatch: int = 24):
        """Превью для стиль-библии: строка на рампу, подпись и тона тёмный → светлый."""
        from PIL import Image, ImageDraw

        label = 84
        columns = max(len(r.tones) for r in self.ramps.values())
        image = Image.new("RGB", (label + columns * swatch, len(self.ramps) * swatch), (14, 12, 20))
        draw = ImageDraw.Draw(image)
        for row, ramp in enumerate(self.ramps.values()):
            y = row * swatch
            draw.text((4, y + swatch // 2 - 5), ramp.name, fill=(205, 200, 185))
            for col, tone in enumerate(ramp.tones):
                rgb = tuple(int(c) for c in self.rgb8[self._index[tone] - 1])
                draw.rectangle([label + col * swatch, y, label + (col + 1) * swatch - 1, y + swatch - 1], fill=rgb)
        return image


def load(path: Path = PALETTE_PATH) -> Palette:
    document = json.loads(Path(path).read_text(encoding="utf-8"))
    validate_schema(document, "palette.schema.json")
    return Palette(document)
