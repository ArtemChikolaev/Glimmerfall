#!/usr/bin/env bash
# Запуск закреплённого Godot (.godot-version) .NET: $GODOT, иначе стандартное место установки на macOS.
# Примеры: tools/godot.sh --headless --path game --import
#          tools/godot.sh --headless --path game --fixed-fps 60 -- --smoke-frames 240
set -euo pipefail
version="$(tr -d '[:space:]' < "$(dirname "$0")/../.godot-version")"
minor="${version%-stable}"
if [[ -n "${GODOT:-}" ]]; then
  exec "$GODOT" "$@"
fi
mac="/Applications/Godot_mono_${minor}.app/Contents/MacOS/Godot"
if [[ -x "$mac" ]]; then
  exec "$mac" "$@"
fi
echo "Godot ${version} .NET не найден: поставь его (docs/spikes/G1.0-environment.md) или задай GODOT=/путь/к/Godot" >&2
exit 2
