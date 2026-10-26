"""Конвейер спрайтов: `PYTHONPATH=tools/sprites python -m glimmer_sprites <команда>` (обычно через make).

  palette               проверить палитру, записать content/art/palette.gpl и docs/art/palette.png
  build MODEL [--scale] рендер в Blender → кадры → лист game/art/sprites и метаданные content/art/sprites
  qa [ID…] [--gate]     метрики по всем кадрам; --gate — код выхода 1 при нарушении; --contact DIR — контактные листы
  fresh                 индекс листов совпадает с файлами и исходниками
  provenance [--release] у каждого ассета game/ есть запись; --release — ещё и ни одной ИИ-заглушки
  readability PREFIX…   серый снимок и контраст героя и врагов по маске (снимки — игра с --screenshot)
"""

from __future__ import annotations

import argparse
import datetime
import json
import os
import shutil
import subprocess
import sys
from pathlib import Path

from . import build, palette as palettes, provenance, qa, readability, sheet as sheets
from .palette import REPO

MAC_BLENDER = "/Applications/Blender.app/Contents/MacOS/Blender"


def blender_binary() -> str:
    found = os.environ.get("BLENDER") or shutil.which("blender") or (MAC_BLENDER if Path(MAC_BLENDER).exists() else None)
    if not found:
        sys.exit("Blender не найден: укажи путь в переменной BLENDER")
    return found


def sheet_id(model: str, scale: float) -> str:
    return model if scale == 1.0 else f"{model}_{round(640 * scale)}"


def cmd_palette(_args) -> int:
    p = palettes.load()
    problems = p.problems()
    for line in problems:
        print("палитра:", line)
    palettes.GPL_PATH.write_text(p.gpl(), encoding="utf-8")
    preview = REPO / "docs" / "art" / "palette.png"
    preview.parent.mkdir(parents=True, exist_ok=True)
    p.preview().save(preview, optimize=True)
    print(f"{len(p.ids)} цветов, {len(p.ramps)} рамп → {qa.relative(palettes.GPL_PATH)}, {qa.relative(preview)}")
    return 1 if problems else 0


def cmd_build(args) -> int:
    p = palettes.load()
    if p.problems():
        sys.exit("палитра нарушает правила — сначала `make palette`")
    sid = args.id or sheet_id(args.model, args.scale)
    raw = Path(args.raw) if args.raw else REPO / "build" / "sprites" / "raw" / sid
    if not args.no_render:
        if raw.exists():
            shutil.rmtree(raw)
        command = [blender_binary(), "-b", "--factory-startup", "-P", str(REPO / "tools/sprites/blender/render.py"), "--",
                   "--model", args.model, "--scale", str(args.scale), "--supersample", str(args.supersample), "--out", str(raw)]
        done = subprocess.run(command, capture_output=True, text=True)
        tail = [line for line in done.stdout.splitlines() if line.startswith("render.py:") or "Error" in line]
        print("\n".join(tail))
        if done.returncode != 0:
            print(done.stdout[-3000:], done.stderr[-3000:])
            return done.returncode
    style = build.Style()
    frames = build.render_frames(raw, p, style)
    if frames.cut:
        print(f"{sid}: силуэт упирается в край холста рендера: {frames.cut[:6]} — увеличь canvas модели")
        return 1
    indexed, meta = sheets.pack(frames, sid, p, style)
    image_path, meta_path = sheets.write_sheet(indexed, meta, p)
    sheets.update_index([sid])
    provenance.upsert({
        "path": image_path.relative_to(REPO).as_posix(),
        "kind": "sprite-sheet",
        "source": f"процедурный манекен «{args.model}» (tools/sprites/blender/models.py), Blender {frames.manifest['blender']}, конвейер tools/sprites",
        "author": "ИИ-агент (Claude Code) кодом из примитивов; конвейер проекта",
        "license": "собственность проекта",
        "date": datetime.date.today().isoformat(),
        "ai": True,
        "placeholder": True,
        "note": f"заглушка прототипа, не арт: {frames.manifest['note']}",
    })
    print(f"{sid}: ячейка {meta['cell'][0]}×{meta['cell'][1]}, лист {indexed.shape[1]}×{indexed.shape[0]}, "
          f"{image_path.stat().st_size // 1024} КБ → {qa.relative(image_path)}, {qa.relative(meta_path)}")
    return 0


def cmd_qa(args) -> int:
    p = palettes.load()
    metas = qa.load_metas(args.ids or None)
    reports = [qa.analyse(meta, p) for meta in metas]
    if args.json:
        print(json.dumps({r.sheet_id: {"summary": r.summary, "sequences": r.sequences, "failures": r.failures} for r in reports},
                         ensure_ascii=False, indent=1))
    else:
        for r in reports:
            worst = max(r.sequences.items(), key=lambda kv: kv[1]["flicker_mean"])
            jerk = max(r.sequences.items(), key=lambda kv: kv[1]["jerk_px"] if not kv[0].startswith("death") else 0)
            print(f"{r.sheet_id}: рост {r.summary['height_px']} px, тон {r.summary['tone']}, светлее мира {r.summary['lit_share']:.0%}; "
                  f"мерцание худшее {worst[1]['flicker_mean']}% ({worst[0]}), рывок худший {jerk[1]['jerk_px']} px ({jerk[0]})")
            for line in r.failures:
                print("  ✗", line)
    if args.contact:
        for path in qa.write_contacts(metas, p, Path(args.contact)):
            print("контактный лист:", qa.relative(path))
    failed = sum(len(r.failures) for r in reports)
    if args.gate and failed:
        print(f"QA: {failed} нарушений")
        return 1
    return 0


def cmd_fresh(_args) -> int:
    problems = sheets.freshness_problems()
    for line in problems:
        print("свежесть:", line)
    print("индекс листов свежий" if not problems else f"{len(problems)} проблем")
    return 1 if problems else 0


def cmd_readability(args) -> int:
    failed = 0
    for prefix in args.prefixes:
        result = readability.analyse(Path(prefix))
        readability.gray_image(Path(prefix)).save(f"{prefix}.gray.png")
        Path(f"{prefix}.readability.json").write_text(json.dumps(result, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")
        parts = [f"{name}: контраст границы {c['contrast']}{'' if c['readable'] else ' — НЕ читается'}, светлый ключ {c['light_share']:.0%}"
                 for name, c in result["classes"].items()]
        separable = ""
        if "hero_key_gap" in result:
            separable = f"; герой отличим от орды: {'да' if result['hero_separable'] else 'НЕТ'} (+{result['hero_key_gap']:.0%} светлого)"
        print(f"{prefix}: сетка {result['grid'][0]}×{result['grid'][1]}, врагов в кадре {result['visible_enemies']}, пол L {result['floor_L']}; "
              + "; ".join(parts) + separable)
        failed += sum(1 for c in result["classes"].values() if not c["readable"]) + (0 if result.get("hero_separable", True) else 1)
    return 1 if args.gate and failed else 0


def cmd_provenance(args) -> int:
    problems = provenance.problems()
    if args.release:
        problems += [f"в релиз нельзя: {line}" for line in provenance.release_problems()]
    for line in problems:
        print("происхождение:", line)
    print(f"реестр: {len(provenance.load()['assets'])} ассетов, проблем {len(problems)}")
    return 1 if problems else 0


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(prog="glimmer_sprites")
    sub = parser.add_subparsers(dest="command", required=True)
    sub.add_parser("palette").set_defaults(fn=cmd_palette)
    b = sub.add_parser("build")
    b.add_argument("model")
    b.add_argument("--scale", type=float, default=1.0)
    b.add_argument("--supersample", type=int, default=4)
    b.add_argument("--id")
    b.add_argument("--raw")
    b.add_argument("--no-render", action="store_true", help="взять готовые проходы из --raw или build/sprites/raw")
    b.set_defaults(fn=cmd_build)
    q = sub.add_parser("qa")
    q.add_argument("ids", nargs="*")
    q.add_argument("--gate", action="store_true")
    q.add_argument("--json", action="store_true")
    q.add_argument("--contact")
    q.set_defaults(fn=cmd_qa)
    sub.add_parser("fresh").set_defaults(fn=cmd_fresh)
    pr = sub.add_parser("provenance")
    pr.add_argument("--release", action="store_true", help="релизный гейт: ИИ-ассеты и заглушки запрещены")
    pr.set_defaults(fn=cmd_provenance)
    rd = sub.add_parser("readability")
    rd.add_argument("prefixes", nargs="+", help="снимки игры без расширения (--screenshot)")
    rd.add_argument("--gate", action="store_true")
    rd.set_defaults(fn=cmd_readability)
    args = parser.parse_args(argv)
    return args.fn(args)


if __name__ == "__main__":
    sys.exit(main())
