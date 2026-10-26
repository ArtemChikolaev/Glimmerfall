# Glimmerfall

Изометрический пиксельный экшн-рогалик: отряд из трёх фонарщиков несёт свет через мир, который поглощает Тьма. Драфт героев с «сыгранностью», переключение посреди орды, огонь и молнии покровителей, дейлики с призраками друзей.

> Рабочее название. Статус — **препродакшн**, этап 1: технические спайки — детерминированный сим, бот-харнесс, спайк орды в Godot, каркас сервера ([отчёты](docs/spikes/README.md), [BACKLOG](docs/BACKLOG.md)). Контент и механики пока — заглушки спайков.

## Документы
- [PRD](docs/PRD.md) — что за игра: столпы, отряд, бой, мета, платформы, монетизация.
- ADR: [0001 — концепция и рамки](docs/adr/0001-concept-and-scope.md) · [0002 — стек](docs/adr/0002-tech-stack.md) · [0003 — арт-направление](docs/adr/0003-art-direction.md).
- [BACKLOG](docs/BACKLOG.md) — этапы 0–3 с критериями выхода.
- [Исследование](docs/research/README.md) — анализ референсов, рынка, движков и арт-пайплайна (октябрь 2026), с которого стартовал проект.
- [CLAUDE.md](CLAUDE.md) — контракт для AI-агентов (он же `AGENTS.md`); скиллы — [docs/ai/INDEX.md](docs/ai/INDEX.md).

## Стек (ADR 0002)
| Слой | Технология | Роль |
|---|---|---|
| `sim/` | чистый C# (`netstandard2.1`), фиксированная точка Q48.16 | вся игровая логика: детерминированно, headless, без движка |
| `sim.tests/`, `sim.cli/` | xUnit v3; CLI `glimmerfall-sim` | тесты и golden-реплеи; бенч, бот-прогоны, карьера, запись и проверка реплеев |
| `game/` | Godot 4.7.2 (.NET 10) | презентация: рендер, VFX, звук, UI, ввод, платформы |
| `content/` | JSON + JSON Schema | герои, покровители, благословения, враги, акты (после вертикального среза) |
| `tools/` | Python + Blender, shell | 3D → пиксельные спрайт-листы, генераторы таблиц, запуск Godot |
| `server/`, `validator/` | Go; Docker с тем же Sim.dll | дейлики, приём и ре-симуляция реплеев, лидерборды |

Окружение и пины версий — [G1.0](docs/spikes/G1.0-environment.md): .NET SDK 10.0.4xx (`global.json`), Godot 4.7.2 .NET (`.godot-version`), Go 1.26.

## Проверки
Из корня репозитория:

```bash
dotnet build Glimmerfall.slnx
dotnet test --project sim.tests/Glimmerfall.Sim.Tests.csproj
tools/godot.sh --headless --path game --import
tools/godot.sh --headless --path game --fixed-fps 60 -- --smoke-frames 240
(cd server && go vet ./... && go test ./...)
```

Скиллы и документы — валидатором:

```bash
python3 -m venv /tmp/glimmerfall-skill-check
/tmp/glimmerfall-skill-check/bin/pip install -r .claude/skills/agent-skill-maintenance/tools/requirements.txt
/tmp/glimmerfall-skill-check/bin/python .claude/skills/agent-skill-maintenance/tools/validate_skills.py
```

## Харнесс и спайк орды
```bash
dotnet build sim.cli/Glimmerfall.Sim.Cli.csproj -c Release
sim.cli/bin/Release/net10.0/glimmerfall-sim bench
sim.cli/bin/Release/net10.0/glimmerfall-sim bot --policy kiter --runs 36 --acts 3
tools/godot.sh --path game
```
В окне спайка F1 переключает автопилот и ручное управление (WASD или стик, пробел — рывок, Q — свап, 1–3 — выбор).

## Права
© 2026 Artem Chikolaev. Все права защищены. Коммерческий проект с оригинальной IP; сторонние ассеты допускаются только по лицензии, разрешающей коммерческое использование без share-alike.
