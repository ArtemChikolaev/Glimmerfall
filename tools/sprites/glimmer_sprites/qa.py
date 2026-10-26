"""QA спрайт-листов по всем кадрам и направлениям (скилл sprite-pipeline): метрики, гейт и контактный лист.

Признаки (идеи — из QA aegis-draft, код свой):
  пусто    — кадры, где силуэт почти исчез;
  куски    — на сколько частей (8-связность, от 3 px) распадается силуэт;
  рывок    — вторая разность центроида соседних кадров, px: кольцо стоит, модель прыгает;
  снос     — линейный тренд центроида за цикл, px: недонейтрализованный root motion;
  провал   — падение площади силуэта между соседними кадрами, доля;
  строб    — кадр t похож на t+2 больше, чем на t+1: две позы чередуются вместо движения;
  мерцание — одиночные вспышки пикселя: значение в кадре t отличается от t−1 и t+1, которые равны между собой,
             и вспышка — пятно не больше 2 px (край, который просто сдвинулся в пике движения, — длинная полоса);
  тон      — средняя светлота силуэта и доля пикселей светлее мира (план светлоты): тёмная модель тонет в фоне;
  рост     — высота силуэта стойки против цели ADR 0003.
Метрика не доказывает качество сама: после гейта — контактный лист глазами и проверка в игре.
"""

from __future__ import annotations

import json
from dataclasses import dataclass, field
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

from . import build, sheet as sheets
from .palette import REPO, Palette

FLICKER_SPECK_PX = 2


@dataclass(frozen=True)
class Gate:
    # Циклы (стойка, бег) на экране почти всё время — к ним строже; разовые клипы короткие, в них главное — крупное
    # движение, а резкий рывок удара задуман (у смерти рывок не меряется вовсе). Ловим только поломки экспорта.
    flicker_mean: float = 1.0      # % площади силуэта, среднее по кадрам цикла
    flicker_action_mean: float = 1.5
    flicker_max: float = 3.0       # % в худшем кадре
    jerk_loop: float = 0.12        # доля целевой высоты
    jerk_action: float = 0.60
    drift_loop_px: float = 1.5
    dropout_loop: float = 0.30
    dropout_action: float = 0.45
    strobe_min: float = 0.8
    pieces_max: int = 3
    height_tolerance_px: float = 3.0
    tone_min: float = 0.30
    lit_share_min: float = 0.30


GATE = Gate()


def flicker_rates(frames: list[np.ndarray], loop: bool) -> list[float]:
    """% площади кадра t, занятый одиночными вспышками (A-B-A пятнами ≤ 2 px), по каждой тройке кадров."""
    rates = []
    for a, b, c in build.triples(len(frames), loop):
        speckles = sum(len(p) for p in build.blips(frames[a], frames[b], frames[c], FLICKER_SPECK_PX))
        rates.append(100.0 * speckles / max(int((frames[b] > 0).sum()), 1))
    return rates


def sequence_metrics(frames: list[np.ndarray], loop: bool) -> dict:
    n = len(frames)
    area = np.array([int((f > 0).sum()) for f in frames], dtype=np.float64)
    centroids = []
    for f in frames:
        ys, xs = np.nonzero(f)
        centroids.append((xs.mean(), ys.mean()) if len(xs) else (np.nan, np.nan))
    c = np.array(centroids)
    jerk = 0.0
    for a, b, d in build.triples(n, loop):
        jerk = max(jerk, float(np.linalg.norm(c[d] - 2 * c[b] + c[a])))
    pairs = [(t, (t + 1) % n) for t in range(n)] if loop else [(t, t + 1) for t in range(n - 1)]
    dropout = max([(area[a] - area[b]) / area[a] for a, b in pairs if area[a] > 0] or [0.0])
    drift = 0.0
    if loop and n >= 3:
        # снос: шов цикла (последний → первый кадр) прыгает сильнее обычного шага — root motion не снят
        steps = [float(np.linalg.norm(c[t + 1] - c[t])) for t in range(n - 1)]
        drift = max(0.0, float(np.linalg.norm(c[0] - c[n - 1])) - float(np.median(steps)))
    strobe = None
    if loop and n >= 6:
        masks = [f > 0 for f in frames]

        def diff(k):
            return np.mean([(masks[t] ^ masks[(t + k) % n]).sum() / max(int((masks[t] | masks[(t + k) % n]).sum()), 1)
                            for t in range(n)])
        d1 = diff(1)
        strobe = float(diff(2) / d1) if d1 > 0 else None
    pieces = max(sum(1 for c in build.components(f > 0) if len(c) >= 3) for f in frames)
    rates = flicker_rates(frames, loop)
    return {
        "frames": n,
        "area_min": int(area.min()),
        "jerk_px": round(jerk, 2),
        "drift_px": round(drift, 2),
        "dropout": round(float(dropout), 3),
        "strobe": None if strobe is None else round(strobe, 2),
        "pieces": int(pieces),
        "flicker_mean": round(float(np.mean(rates)) if rates else 0.0, 3),
        "flicker_max": round(float(np.max(rates)) if rates else 0.0, 3),
    }


def tone_metrics(frame: np.ndarray, palette: Palette) -> dict:
    outline = palette.index(palette.roles["outline"])
    inside = (frame > 0) & (frame != outline)
    lightness = palette.lab[frame[inside] - 1, 0]
    world_max = palette.lightness["world"][1]
    ys, _ = np.nonzero(inside)
    return {
        "height_px": int(ys.max() - ys.min() + 1) if len(ys) else 0,
        "tone": round(float(lightness.mean()), 3) if lightness.size else 0.0,
        "lit_share": round(float((lightness > world_max).mean()), 3) if lightness.size else 0.0,
    }


@dataclass
class SheetReport:
    sheet_id: str
    meta: dict
    sequences: dict = field(default_factory=dict)
    summary: dict = field(default_factory=dict)
    failures: list = field(default_factory=list)


def analyse(meta: dict, palette: Palette, gate: Gate = GATE) -> SheetReport:
    frames = sheets.read_frames(meta)
    report = SheetReport(meta["id"], meta)
    reference_clip = next(name for name, clip in meta["clips"].items() if clip["loop"])
    reference = frames[reference_clip][meta["clips"][reference_clip]["dirs"][0]][0]
    report.summary = tone_metrics(reference, palette)
    target = meta["source"].get("target_px", report.summary["height_px"])
    reference_area = int((reference > 0).sum())
    fail = report.failures

    s = report.summary
    if abs(s["height_px"] - target) > gate.height_tolerance_px:
        fail.append(f"рост {s['height_px']} px вне цели {target}±{gate.height_tolerance_px}")
    # реквизит — часть мира и обязан быть в мировых тонах; светлее мира должны быть фигуры (план светлоты)
    if meta["source"].get("class", "character") == "character":
        if s["tone"] < gate.tone_min:
            fail.append(f"тон {s['tone']} < {gate.tone_min}: модель тонет в тёмном мире")
        if s["lit_share"] < gate.lit_share_min:
            fail.append(f"светлее мира только {s['lit_share']:.0%} силуэта < {gate.lit_share_min:.0%}")

    for name, clip in meta["clips"].items():
        death = name == "death"
        for direction, seq in frames[name].items():
            m = sequence_metrics(seq, clip["loop"])
            m["empty"] = int(sum(1 for f in seq if (f > 0).sum() < 0.25 * reference_area))
            report.sequences[f"{name}/{direction}"] = m
            where = f"{name}/{direction}"
            if m["empty"]:
                fail.append(f"{where}: пустых кадров {m['empty']}")
            if m["pieces"] > gate.pieces_max + (1 if death else 0):
                fail.append(f"{where}: силуэт распадается на {m['pieces']} куска")
            flicker_limit = gate.flicker_mean if clip["loop"] else gate.flicker_action_mean
            if m["flicker_mean"] > flicker_limit or m["flicker_max"] > gate.flicker_max:
                fail.append(f"{where}: мерцание {m['flicker_mean']}% (макс {m['flicker_max']}%)")
            if clip["loop"]:
                if m["jerk_px"] > gate.jerk_loop * target:
                    fail.append(f"{where}: рывок {m['jerk_px']} px > {gate.jerk_loop * target:.1f}")
                if m["drift_px"] > gate.drift_loop_px:
                    fail.append(f"{where}: снос {m['drift_px']} px за цикл")
                if m["dropout"] > gate.dropout_loop:
                    fail.append(f"{where}: провал площади {m['dropout']:.0%}")
                if m["strobe"] is not None and m["strobe"] < gate.strobe_min:
                    fail.append(f"{where}: строб {m['strobe']} — позы чередуются")
            elif not death:
                if m["jerk_px"] > gate.jerk_action * target:
                    fail.append(f"{where}: рывок {m['jerk_px']} px > {gate.jerk_action * target:.1f}")
                if m["dropout"] > gate.dropout_action:
                    fail.append(f"{where}: провал площади {m['dropout']:.0%}")
    return report


def load_metas(ids: list[str] | None = None) -> list[dict]:
    index = sheets.load_index()
    chosen = ids or list(index["sheets"])
    return [json.loads((build.SPRITES_META_DIR / f"{i}.json").read_text(encoding="utf-8")) for i in chosen]


def contact_sheet(meta: dict, palette: Palette, zoom: int = 3) -> Image.Image:
    """Все кадры всех направлений крупно на тёмном полу мира — для проверки глазами."""
    frames = sheets.read_frames(meta)
    cell_w, cell_h = meta["cell"]
    label = 70
    rows = [(f"{name}/{d}", seq) for name, by_dir in frames.items() for d, seq in by_dir.items()]
    columns = max(len(seq) for _, seq in rows)
    width = label + columns * (cell_w * zoom + 4)
    height = len(rows) * (cell_h * zoom + 4) + 4
    floor = tuple(int(c) for c in palette.rgb8[palette.index("stone2") - 1])
    image = Image.new("RGB", (width, height), floor)
    draw = ImageDraw.Draw(image)
    for r, (title, seq) in enumerate(rows):
        y = 4 + r * (cell_h * zoom + 4)
        draw.text((4, y + cell_h * zoom // 2 - 5), title, fill=(220, 214, 196))
        for f, frame in enumerate(seq):
            rgba = palette.to_rgba(frame)
            tile = Image.fromarray(rgba, "RGBA").resize((cell_w * zoom, cell_h * zoom), Image.NEAREST)
            x = label + f * (cell_w * zoom + 4)
            image.paste(tile, (x, y), tile)
            ax, ay = meta["anchor"]
            draw.point((x + ax * zoom, y + ay * zoom), fill=(249, 93, 162))
    return image


def write_contacts(metas: list[dict], palette: Palette, out_dir: Path) -> list[Path]:
    out_dir.mkdir(parents=True, exist_ok=True)
    paths = []
    for meta in metas:
        path = out_dir / f"{meta['id']}.png"
        contact_sheet(meta, palette).save(path)
        paths.append(path)
    return paths


def relative(path: Path) -> str:
    try:
        return path.relative_to(REPO).as_posix()
    except ValueError:
        return str(path)
