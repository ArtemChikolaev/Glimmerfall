"""Реестр происхождения ассетов (content/art/provenance.json): полнота и релизный гейт.

Ассет — любой файл с расширением из ASSET_EXTENSIONS в game/ (кроме кэша импорта Godot). У каждого есть запись;
записи с ai или placeholder блокируют релизную сборку (`release_problems`), но не разработку.
"""

from __future__ import annotations

import json
from pathlib import Path

from .palette import REPO, validate_schema

PROVENANCE_PATH = REPO / "content" / "art" / "provenance.json"
ASSET_EXTENSIONS = {".png", ".svg", ".jpg", ".jpeg", ".webp", ".exr", ".wav", ".ogg", ".mp3", ".ttf", ".otf", ".glb", ".gltf", ".blend"}


def load() -> dict:
    document = json.loads(PROVENANCE_PATH.read_text(encoding="utf-8"))
    validate_schema(document, "provenance.schema.json")
    return document


def save(document: dict) -> None:
    document["assets"] = sorted(document["assets"], key=lambda a: a["path"])
    validate_schema(document, "provenance.schema.json")
    PROVENANCE_PATH.write_text(json.dumps(document, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")


def upsert(entry: dict) -> None:
    """Добавляет или обновляет запись; дата создания у существующей записи сохраняется."""
    document = load()
    old = next((a for a in document["assets"] if a["path"] == entry["path"]), None)
    if old is not None:
        entry = {**entry, "date": old["date"]}
    document["assets"] = [a for a in document["assets"] if a["path"] != entry["path"]] + [entry]
    save(document)


def asset_files() -> list[str]:
    game = REPO / "game"
    files = []
    for path in game.rglob("*"):
        if path.is_file() and path.suffix.lower() in ASSET_EXTENSIONS and ".godot" not in path.relative_to(game).parts:
            files.append(path.relative_to(REPO).as_posix())
    return sorted(files)


def problems() -> list[str]:
    document = load()
    listed = [a["path"] for a in document["assets"]]
    out = [f"{p}: две записи" for p in sorted({p for p in listed if listed.count(p) > 1})]
    out += [f"{p}: нет записи в реестре происхождения" for p in asset_files() if p not in listed]
    out += [f"{p}: в реестре, но файла нет" for p in listed if not (REPO / p).exists()]
    return out


def release_problems() -> list[str]:
    """Что не пускает в релиз: ИИ-ассеты и заглушки (ADR 0003 п. 8)."""
    out = []
    for asset in load()["assets"]:
        flags = [name for name in ("ai", "placeholder") if asset[name]]
        if flags:
            out.append(f"{asset['path']}: {', '.join(flags)}")
    return out
