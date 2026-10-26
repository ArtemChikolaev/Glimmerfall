#!/usr/bin/env bash
# CI (Linux x64): скачивает Godot .NET версии из .godot-version, сверяет SHA-512 с закреплённым и кладёт в $1.
# Хеш — из SHA512-SUMS.txt релиза godotengine/godot; меняется вместе с .godot-version.
set -euo pipefail
dest="${1:?usage: install-godot.sh DEST_DIR}"
version="$(tr -d '[:space:]' < "$(dirname "$0")/../../.godot-version")"
expected_version="4.7.2-stable"
expected_sha512="1855960b27ee3ef5e66e5e228cced69d55637b24334a7411162687dcd077d8f9f645348cdb8eae984bec8135d49ed855a1e3a16476786b8bce60774fd8402d13"
if [[ "$version" != "$expected_version" ]]; then
  echo ".godot-version ($version) != pinned hash version ($expected_version): обнови хеш в $0" >&2
  exit 1
fi
name="Godot_v${version}_mono_linux_x86_64"
binary="$dest/$name/Godot_v${version}_mono_linux.x86_64"
mkdir -p "$dest"
if [[ ! -x "$binary" ]]; then
  curl -sSfL -o "$dest/$name.zip" "https://github.com/godotengine/godot/releases/download/${version}/${name}.zip"
  echo "${expected_sha512}  $dest/$name.zip" | sha512sum -c - >&2
  unzip -q -o "$dest/$name.zip" -d "$dest"
  rm "$dest/$name.zip"
fi
echo "$binary"
