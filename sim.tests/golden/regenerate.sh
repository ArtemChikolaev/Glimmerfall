#!/usr/bin/env bash
# Перезапись golden-реплеев. Только осознанно (скилл sim-determinism): правило, данные или порядок RNG поменялись,
# версия баланса бампнута (SimVersion.Balance), дифф строк `digest` просмотрен глазами.
# bash, а не zsh: zsh не делит $SHORT на слова, и оверрайды молча склеиваются в один аргумент.
set -euo pipefail
cd "$(dirname "$0")/../.."
dotnet build sim.cli/Glimmerfall.Sim.Cli.csproj -c Release -v quiet
CLI=sim.cli/bin/Release/net10.0/glimmerfall-sim
G=sim.tests/golden
# Короткий акт сжимает рост спавна — те же оверрайды, что TestRuns.ShortActOverrides.
SHORT=(--set enemy.gloom_warden.hp=300 --set spawn.endMilli=4000)

"$CLI" record --policy kiter --seed golden-kiter --acts 1 --act-seconds 120 "${SHORT[@]}" --every 600 --out "$G/kiter-short-act.gfr"
"$CLI" record --policy afk --seed golden-afk --acts 1 --act-seconds 120 "${SHORT[@]}" --every 600 --out "$G/afk-short-act.gfr"
"$CLI" record --policy kiter --seed golden-acts --acts 2 --act-seconds 120 "${SHORT[@]}" --every 600 --out "$G/kiter-two-acts.gfr"
"$CLI" record --policy kiter --scenario horde --seed golden-horde --max-ticks 600 --every 120 --out "$G/horde-bench.gfr"
"$CLI" verify "$G"/*.gfr
