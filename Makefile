# Частые команды Glimmerfall (по образцу aegis-draft). `make` без цели печатает список.
# Godot запускается через tools/godot.sh: закреплённая версия из .godot-version или путь из $GODOT.
# Переменные можно переопределить: make bot POLICY=afk RUNS=120 ACTS=1 TIER=2 SEED=base

GODOT   := tools/godot.sh
CLI_DLL := sim.cli/bin/Release/net10.0/glimmerfall-sim.dll
CLI     := dotnet $(CLI_DLL)
SIM     := $(CURDIR)/sim.cli/bin/Release/net10.0/glimmerfall-sim
VENV    := .venv/.stamp
SPRITES := PYTHONPATH=tools/sprites .venv/bin/python -m glimmer_sprites
MODELS  ?= lamplighter warden sparkwright lamplighter_ashen warden_bloodbear sparkwright_boneseer \
	skeleton skeleton_archer ghoul brute horned ravager gloom_warden \
	pillar tomb cross brazier statue sarcophagus shrine altar bones stake deadtree

POLICY  ?= kiter
RUNS    ?= 36
ACTS    ?= 3
TIER    ?= 0
SEED    ?= make
SECONDS ?= 60
ARGS    ?=
# Android-сборки подписываются стандартным debug-ключом Android Studio (пароль android — общеизвестный, не секрет).
KEYSTORE ?= $(HOME)/.android/debug.keystore

.DEFAULT_GOAL := help
.PHONY: help doctor build build-game cli import debug debug-run debug-horde editor test smoke check \
	bench bench-mac bot afk career digest golden \
	venv palette sprites sprites-check \
	export-deck export-mac export-android export-ios \
	server server-test validator-image clean

help: ## этот список
	@awk 'BEGIN {FS = ":.*## "} /^## / {printf "\n%s\n", substr($$0, 4)} /^[a-z][a-z-]*:.*## / {printf "  make %-16s %s\n", $$1, $$2}' $(MAKEFILE_LIST)
	@echo ""
	@echo "Переменные: POLICY=$(POLICY) RUNS=$(RUNS) ACTS=$(ACTS) TIER=$(TIER) SEED=$(SEED) SECONDS=$(SECONDS) ARGS=\"$(ARGS)\""

## Разработка
doctor: ## версии инструментов и сверка пинов (.NET, Godot, Go)
	@echo ".NET SDK:   $$(dotnet --version)  (global.json: 10.0.4xx)"
	@echo "Godot:      $$($(GODOT) --version 2>/dev/null || echo 'не найден')  (.godot-version: $$(cat .godot-version))"
	@echo "Go:         $$(go version 2>/dev/null || echo 'не найден')"
	@echo "Docker:     $$(docker --version 2>/dev/null || echo 'не найден')"
	@v="$$(tr -d '[:space:]' < .godot-version)"; grep -q "Sdk=\"Godot.NET.Sdk/$${v%-stable}\"" game/Glimmerfall.csproj \
		&& echo "Пины Godot совпадают с game/Glimmerfall.csproj" || (echo "Пин Godot расходится с game/Glimmerfall.csproj" && exit 1)

build: ## собрать все C#-проекты (Debug)
	dotnet build Glimmerfall.slnx

build-game: ## собрать C# игры для запуска из Godot (Debug)
	dotnet build game/Glimmerfall.csproj

cli: ## собрать glimmerfall-sim (Release) — бенч, бот, реплеи, валидатор
	dotnet build sim.cli/Glimmerfall.Sim.Cli.csproj -c Release

# Импорт ресурсов Godot — по штампу, как зависимости в aegis-draft: свежий клон, make clean или новые/изменённые листы.
GAME_ART := $(shell find game -path game/.godot -prune -o \( -name '*.png' -o -name '*.svg' \) -print)

import: game/.godot/.import-stamp ## headless-импорт ресурсов Godot, когда ассеты новее кэша

game/.godot/.import-stamp: $(GAME_ART)
	$(GODOT) --headless --path game --import
	@touch $@

debug: build-game import ## игра: титульный экран, настройки, забег (WASD/стик, пробел — рывок, Q — смена, Esc — пауза)
	$(GODOT) --path game -- $(ARGS)

debug-run: build-game import ## сразу забег мимо титула (ARGS="--autopilot" — забег ведёт бот)
	$(GODOT) --path game -- --autostart $(ARGS)

debug-horde: build-game import ## стресс-сцена орды: 5 тыс. врагов и спираль снарядов, F1 — автопилот/ручное
	$(GODOT) --path game -- --scenario horde $(ARGS)

editor: build-game import ## открыть проект в редакторе Godot
	$(GODOT) --editor --path game

## Проверки
test: ## тесты сима (golden-реплеи, тест-запрет по IL, инварианты)
	dotnet test --project sim.tests/Glimmerfall.Sim.Tests.csproj

smoke: build-game ## headless-импорт, смоук сцены орды и пути меню → забег → пауза, как в CI
	$(GODOT) --headless --path game --import
	$(GODOT) --headless --path game --fixed-fps 60 -- --smoke-frames 240
	$(GODOT) --headless --path game --fixed-fps 60 -- --smoke-flow 2400

check: build test afk smoke server-test sprites-check ## всё, что гоняет CI (кроме матрицы ОС и Docker), плюс скиллы и diff
	python3 -m venv /tmp/glimmerfall-skill-check
	/tmp/glimmerfall-skill-check/bin/pip install -q -r .claude/skills/agent-skill-maintenance/tools/requirements.txt
	/tmp/glimmerfall-skill-check/bin/python .claude/skills/agent-skill-maintenance/tools/validate_skills.py
	/tmp/glimmerfall-skill-check/bin/python -m unittest discover -s .claude/skills/agent-skill-maintenance/tools -p 'test_*.py'
	git diff --check

## Харнесс и замеры
bench: cli ## бенч тиков сима: 5 тыс. врагов и 2 тыс. снарядов, дайджест в конце
	$(CLI) bench

bench-mac: export-mac ## замер кадра на Release-экспорте macOS (SECONDS=60; из редактора сим в Debug — не мерить)
	build/macos/Glimmerfall.app/Contents/MacOS/Glimmerfall -- --bench-seconds $(SECONDS) $(ARGS)

bot: cli ## бот-прогоны: POLICY=kiter|afk|afk-after:120 RUNS ACTS TIER SEED — статистика по актам
	$(CLI) bot --policy $(POLICY) --runs $(RUNS) --acts $(ACTS) --tier $(TIER) --seed $(SEED)

afk: cli ## гейт угрозы (CI): AFK гибнет в акте 1, медиана ≤ 90 с; «AFK со 2-й минуты» — ≤ 20 с после остановки
	$(CLI) bot --policy afk --runs $(RUNS) --acts 1 --seed $(SEED)-afk --gate-wipe-p50 90
	$(CLI) bot --policy afk-after:120 --runs $(RUNS) --acts 1 --seed $(SEED)-afk120 --gate-wipe-p50 140

career: cli ## карьера: забеги подряд с переносом меты (RUNS ACTS SEED)
	$(CLI) career --runs $(RUNS) --acts $(ACTS) --seed $(SEED)

digest: cli ## набор дайджестов для сверки ОС (как джоб CI determinism)
	$(CLI) digest

golden: ## перезаписать golden-реплеи — только осознанно, после бампа SimVersion.Balance
	bash sim.tests/golden/regenerate.sh

## Арт-конвейер (tools/sprites, скилл sprite-pipeline)
venv: $(VENV) ## Python-окружение конвейера в .venv (numpy, Pillow, jsonschema — пины в tools/sprites/requirements.txt)

$(VENV): tools/sprites/requirements.txt
	python3 -m venv .venv
	.venv/bin/python -m pip install -q -r tools/sprites/requirements.txt
	touch $(VENV)

palette: venv ## проверить мастер-палитру, записать palette.gpl для Aseprite и превью docs/art/palette.png
	$(SPRITES) palette

sprites: venv ## Blender → листы обеих сеток (MODELS=…) в game/art/sprites; QA и контактные листы в build/sprites/contact
	@for m in $(MODELS); do $(SPRITES) build $$m && $(SPRITES) build $$m --scale 1.5 || exit 1; done
	$(SPRITES) qa --contact build/sprites/contact

sprites-check: venv ## как джоб CI: тесты конвейера, QA-гейт листов, свежесть индекса, реестр происхождения
	PYTHONPATH=tools/sprites .venv/bin/python -m unittest discover -s tools/sprites/tests
	$(SPRITES) qa --gate
	$(SPRITES) fresh
	$(SPRITES) provenance

## Сборки для устройств
export-deck: build-game ## Linux x86_64 для Steam Deck → build/glimmerfall-steamdeck-spike.zip
	rm -rf build/steamdeck/Glimmerfall.x86_64 build/steamdeck/Glimmerfall.pck build/steamdeck/data_Glimmerfall_linuxbsd_x86_64
	mkdir -p build/steamdeck
	$(GODOT) --headless --path game --export-release SteamDeck ../build/steamdeck/Glimmerfall.x86_64
	printf '%s\n' '#!/usr/bin/env bash' 'cd "$$(dirname "$$0")"' './Glimmerfall.x86_64 --fullscreen -- --bench-seconds 60 --warmup-seconds 5 "$$@"' > build/steamdeck/run-bench.sh
	chmod +x build/steamdeck/run-bench.sh
	cd build && rm -f glimmerfall-steamdeck-spike.zip && zip -qr -y glimmerfall-steamdeck-spike.zip steamdeck
	@echo "Готово: build/glimmerfall-steamdeck-spike.zip — инструкция в docs/spikes/G1.3-horde.md"

export-mac: build-game ## macOS (universal) → build/macos/Glimmerfall.app
	rm -rf build/macos/Glimmerfall.app
	mkdir -p build/macos
	$(GODOT) --headless --path game --export-release macOS ../build/macos/Glimmerfall.app

export-android: build-game ## APK с автозамером (тег bench) → build/android/glimmerfall-spike.apk
	mkdir -p build/android
	GODOT_ANDROID_KEYSTORE_RELEASE_PATH="$(KEYSTORE)" GODOT_ANDROID_KEYSTORE_RELEASE_USER=androiddebugkey GODOT_ANDROID_KEYSTORE_RELEASE_PASSWORD=android \
		$(GODOT) --headless --path game --export-release "Android Bench" ../build/android/glimmerfall-spike.apk
	@echo "Готово: build/android/glimmerfall-spike.apk — установить: adb install -r build/android/glimmerfall-spike.apk"

export-ios: build-game ## Xcode-проект с NativeAOT → build/ios (нужен application/app_store_team_id в пресете iOS Bench)
	@if grep -q '^application/app_store_team_id=""' game/export_presets.cfg; then \
		echo "Впиши свой Team ID в game/export_presets.cfg → [preset.3.options] application/app_store_team_id (Xcode → Settings → Accounts)"; exit 1; fi
	mkdir -p build/ios
	$(GODOT) --headless --path game --export-release "iOS Bench" ../build/ios/Glimmerfall.ipa
	@echo "Готово: открой build/ios/Glimmerfall.xcodeproj в Xcode и запусти на iPhone"

## Сервер
server: cli ## Go-сервер на :8080 с локальным валидатором (тот же Sim.dll)
	cd server && VALIDATOR_CMD="$(SIM) verify --json -" go run ./cmd/api

server-test: cli ## gofmt, vet и тесты сервера с настоящим валидатором
	cd server && test -z "$$(gofmt -l .)" && go vet ./... && GLIMMERFALL_SIM="$(SIM)" go test ./...

validator-image: ## Docker-образ валидатора (скачает базовые образы .NET ~1 ГБ при первой сборке)
	docker build -f validator/Dockerfile -t glimmerfall-validator .
	docker run --rm -i glimmerfall-validator < sim.tests/golden/kiter-short-act.gfr

## Уборка
clean: ## удалить сборки: bin/obj, build/, кэш импорта Godot
	rm -rf sim/bin sim/obj sim.tests/bin sim.tests/obj sim.cli/bin sim.cli/obj build game/.godot
