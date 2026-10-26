# aegis-draft inventory: what was built, what was learned, and what can carry over to a new native, original-IP pixel-art action roguelite

Scope and method. This is a strictly read-only inventory of `/Users/Shared/Coding/aegis-draft` (last commit 2026-10-05, inventory taken 2026-10-25) and of the owner's agent memory notes in `/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/`. Line counts come from `wc -l`. Content counts come from a read-only `tsx` script that imported the Arcade content registries and printed their sizes. The script lives in the session scratchpad, not in the repo. No sims, tests, dev servers or npm scripts were run, so every win-rate below is the last value **recorded in the docs**, not a fresh measurement. Project docs are in Russian; quotes are translated.

Path shorthands used in links:
- `REPO` = `/Users/Shared/Coding/aegis-draft`
- `MEM` = `/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory`

---

## 1. What exists: modes, systems, numbers, architecture, tooling and process

### Takeaway
In about 3 months (2026-07-11 → 2026-10-05, 816 commits) a solo developer working with AI agents built a Dota 2 fan web app with 4 live game modes and 2 online modes: Duel is built but not deployed, and the Arena is only partly built. The app has a Go ETL pipeline, a Go server, a 56.5k-line TypeScript client and a very large, well-documented agent process (CLAUDE.md, 17 skills, 3 ADRs, PRD, a 1.27 MB BACKLOG and 45 memory notes plus an index).

The "Arcade" survivors-like mode was built in one month (2026-09-05 → 2026-10-05, config versions a0.1.0 → a0.85.0, 192 commits touching its sim). It has:
- 126 heroes;
- 34 enemy kinds;
- 5 "schools" with 68 blessings;
- a 40-step difficulty ladder;
- 299 cosmetics;
- a deterministic 60 Hz sim with input-log replays;
- a headless bot balance harness;
- a Blender pipeline that renders 3D models into pixel sprite sheets.

### Cited findings

#### Scale, timeline and the stack as built
- **History.** 816 commits. The first commit is 2026-07-11 ("Initial commit") and the last is 2026-10-05 (M26 docs). 192 commits touch `web/src/game/arcade`; the first is 2026-09-05. 373 commits landed from 2026-09-05 onward, peaking at 103 on 2026-09-06 and 79 on 2026-09-13. About 33 of those are data-refresh bot commits. Source: `git log`, `git rev-list` (read-only) in [REPO](/Users/Shared/Coding/aegis-draft).
- **Stack** ([README](/Users/Shared/Coding/aegis-draft/README.md), [ADR 0001](/Users/Shared/Coding/aegis-draft/docs/adr/0001-tech-stack.md)):
  - Go ETL `pipeline/` (OpenDota → game JSON, stages `fetch→normalize→aggregate→rate→emit→validate`);
  - TS + React + Vite + Zustand client `web/`;
  - Go + chi + Postgres server `server/`;
  - JSON Schema contract `schema/`;
  - "static-first", no Kubernetes.
- **Lines of code** (measured with `wc -l`):

  | Area | Lines | Notes |
  |---|---|---|
  | `web/src` TS/TSX | 56,536 | `features/` 23,042; `game/` 21,870; `i18n/` 7,273; `state/` 4,862; `ui/` 2,572 |
  | `web/test` | 21,035 | 169 test files |
  | `web/e2e` | 2,171 | 10 Playwright specs, including `arcade.spec.ts`, `offline.spec.ts`, `contrast.spec.ts` |
  | Go server | 2,320 + 2,011 tests | |
  | Go pipeline | 4,425 + 2,453 tests | |

- **Modes** ([README](/Users/Shared/Coding/aegis-draft/README.md)):
  - Live: Classic (quick run and roguelite season), Esports Manager, Real Tournament, Arcade.
  - Duel (online 1v1): playable only against a locally run server.
  - Arena (18-player online): marked ⬜. The README says it "is not assembled yet" and waits for the WebSocket server, although its shared-draft engine slice (MP2) exists ([MEM/arena-mp2-shared-draft.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arena-mp2-shared-draft.md)).
  - Live site: GitHub Pages.

#### Arcade code size ([REPO/web/src/game/arcade](/Users/Shared/Coding/aegis-draft/web/src/game/arcade), [REPO/web/src/features/arcade](/Users/Shared/Coding/aegis-draft/web/src/features/arcade))
- **Sim core.** `game/arcade` totals 11,068 lines:
  - `sim.ts` 5,726 lines: one `ArcadeSim` class with about 265 method definitions (rough regex count of 2-space-indented signatures);
  - `types.ts` 727, `config.ts` 513, `mapgen.ts` 180, `replay.ts` 157;
  - `content/*.ts`: `heroes` 1,031, `parts` 832, `cosmetics` 662, `schools` 190, `gear` 195, `talents` 187, `enemies` 107, plus smaller files.
- **Client.** `features/arcade` totals 7,932 lines: `ArcadeScreen.tsx` 1,787, `renderer.ts` 1,694, `sprites.ts` 952, `HeroWardrobe.tsx` 704, `effects.ts` 494, `particles.ts` 344, `terrain.ts` 235, `input.ts` 203, `soundscape.ts` 166, `heroSfx.ts` 143, `music.ts` 75, `gamepad.ts` 65.
- **Other.** `state/arcadeStore.ts` 872 lines; `i18n/arcade.ts` 4,860 lines (RU/EN Arcade strings as a lazy chunk); `game/dmath.ts` 116 lines; `game/rng.ts` 74 lines.
- **Arcade tests.** 107 `web/test/arcade*` files (10,451 lines) plus `gameDeterminism.test.ts` and `sound.test.ts`. Source: `wc`, `ls` on [REPO/web/test](/Users/Shared/Coding/aegis-draft/web/test).

#### Arcade content counts
From the content registries ([content/](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content)) via the read-only count script:

| Content | Count and detail |
|---|---|
| Heroes | 126. 55 distinct ability "kinds" are used across Q/W/E/R. Most common: nova 44, line_burst 43, dash 38, signature 30, lightning_bolt 27, damage_ward 23, armor_buff 20. |
| Hero extras | 121 heroes have a signature passive. 25 abilities spawn summons; 4 are transformation forms. |
| Enemy kinds | 34: 13 elite-flagged, 3 rival enemy heroes (Pudge/Axe/Lina), 9 with neutral casts, plus Roshan, Tormentor and Ancient. |
| Elite affixes | 5: haste, vampiric, volatile, frost, splitter. |
| Items | 13 shop items; 30 neutral items in 5 tiers, with 10 enchants, 7 of them with special properties. |
| Persistent gear | 34 bases, 6 slots (weapon/helm/armor/boots/amulet/ring) and 6 boss uniques: Aegis of the Immortal, Tormentor's Shard, Heart of the Ancient, Divine Rapier, Manta, Giant's Ring. |
| Schools | 5: radiance, skadi, maelstrom, beast, venom. |
| Upgrades ("blessings") | 68. By school: radiance 14, skadi 15, maelstrom 15, beast 12, venom 12. By type: power 14, attack 9, strike 7, passive 20, cast 9, dash 8, summon 1. 23 legendaries; 10 hybrids, one per school pair (`hyb_*`). |
| Rank ladder | 40 steps: 8 tiers × 5 stars. |
| Cosmetics | 299 entries: skin 238, form 21, summon 15, trail 10, frame 4, aura 4, death 4, tint 3. By rarity: standard 6, refined 18, exotic 247, arcana 28. Plus 5 gem styles. |
| Slot-based wardrobe | Part layers for 9 heroes (20 "families"). |
| Pets | 5 kinds (hawk/wolf/bear/illusion/summon) and 14 summon bodies. |
| Meta and progression | 4 starting traits; 5 Legacy branches × 4 ranks; 5 expeditions; 3 rolled map compositions (wilds/trade/siege); 4 Roshan rewards (aegis/cheese/refresher_shard/aghanims_shard); talent levels 10/15/20/25 with 8 generic talents. |
| Config | `ARCADE_CONFIG_VERSION = "a0.85.0"`; the `ARCADE` config object has about 68 top-level sections. |

#### Documentation drift (caught during this inventory)
- **README** still says "four schools" ([README](/Users/Shared/Coding/aegis-draft/README.md)). The code has 5 (Venom was added by T13.47).
- **PRD §5.15** gives 2026-09-08 numbers ([PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md)): "42 upgrades, 18 legendary, four hybrids" and "155 cosmetic entries". The code now has 68/23/10 and 299.
- **The brief** planned "30 Hz sim + interpolation" ([brief](/Users/Shared/Coding/aegis-draft/docs/arcade-survivors-brief.md)). The core shipped at 60 Hz without interpolation ([BACKLOG T13.1](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).

#### Arcade game structure ([PRD §5.15, lines 834–888](/Users/Shared/Coding/aegis-draft/docs/PRD.md))
- **Act and hero.** An act lasts 20 minutes, like the reference game. A hero has an auto-attack plus Q/W/E/R and levels 1–25, with the ult at 6/12/18 and talents at 10/15/20/25. Blink dash: Shift/L2, 150 px, 0.3 s invulnerability, a 6 s charge.
- **Schedule.** Creep waves every 30 s, with every fifth a siege wave. Roshan spawns at 7:00 and 14:00 and drops a reward the player chooses. Tormentor at 10:30. The Ancient at 20:00 under mega-creeps, with escalation after 22:00. Normal spawns pause while Roshan lives, except in the River act.
- **Acts and modes.** Acts: Radiant forest by day, Dire forest at night, the River with Roshan's pit. A shorter "warm-up to 9:00" mode also exists.
- **Economy.** Gold from kills and from bounty runes every 3 minutes. Secret Shop windows at 3:00 and 6:00, plus 11:00 and 16:30 in full acts. Neutral-item tokens at minutes 2/5/9/13/17.
- **Map events.** The act also has a corrupted camp with a champion, an outpost, a lotus pond, cursed chests and curses, a forge, a rift, a caravan escort, hunt contracts, six champions, rival enemy heroes, ancient camps, killing streaks and multi-kills with an announcer, elite affixes and a one-time buyback.
- **Cosmetics** change no numbers and are not part of the seed.

#### Arcade architecture
- **Deterministic sim.** `seed + input log` replays a run bit-exactly. The tick is fixed. The sim uses no DOM, no `Math.sin`, no `Date.now`. Since M24 it uses the project's own `dmath`; since M25 that rule covers all of `src/game`.
- **Rendering.** The renderer `ArcadeRenderer` uses Canvas 2D with no dependencies; PixiJS was never needed. React draws only the HUD and modals. DOM rendering is forbidden.
- **Store.** `state/arcadeStore.ts` sits beside the other mode stores. There is no mid-run save; the run pauses on `visibilitychange`.
- **Content and config.** Content is TS in `game/arcade/content/` with no schema. Config is `ARCADE` in `config.ts`, with its own `ARCADE_CONFIG_VERSION`.
- **Input.** Keyboard, touch (virtual stick) and gamepad all produce one `ArcadeInput` through `input.ts`, so all three share one replay log.
- **Profile.** Local `localStorage`. Source: [PRD §5.15 engineering decisions](/Users/Shared/Coding/aegis-draft/docs/PRD.md).
- **Input contract.** `ArcadeInput {mx, my, cast, choose, act}`. `act` multiplexes many modal actions through integer codes: `SHOP_ACT` 1–6 and 10+/20+, `BANISH_ACT` 30+, `AUTOCAST_ACT` 40+, `PICKUP_ACT` 61, `BUILD_ACT` 62, `BAG_EQUIP_ACT` 70+, `BAG_DROP_ACT` 90+ ([types.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/types.ts)).
- **Replay format.** `A1~seed~hero~rank~act~ver~base64url(log)`. The log is packed at about 4 bytes per entry and carries snapshots of worn gear, the Legacy allocation and the starting trait. Replays are rejected on a config-version mismatch.
- **Daily.** Seed `daily-YYYY-MM-DD` (UTC). The hero comes from a hash of the date; rank 0, full act. No gear, Legacy or traits apply ([replay.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/replay.ts)).
- **Persistence.** Per-origin `localStorage` keys `aegis-draft.arcade.{history,cosmetics,gear,autocast,favorites,rank,progress}`. History is capped at 50 entries. A permanent profile is kept separate from that history ([arcadeStore.ts](/Users/Shared/Coding/aegis-draft/web/src/state/arcadeStore.ts)).

#### Server, online and offline
- **Server** (`server/internal/{transport,service,store,model,auth,telegram}`):
  - goose migrations for users/identities and saves; sqlc queries;
  - a Telegram initData login → HS256 JWT;
  - cloud saves with a 409 conflict guarded by a compare-and-set on a monotonic revision;
  - in-memory rooms with a WebSocket relay.

  Daily server validation (T8.5) and the leaderboard (T8.6) are ⬜ (not started). Sources: [BACKLOG M8](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [server tree](/Users/Shared/Coding/aegis-draft/server/internal).
- **Relay multiplayer pattern** (Duel, then Arena) ([MEM/duel-mode-relay.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/duel-mode-relay.md), [MEM/arena-mp2-shared-draft.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arena-mp2-shared-draft.md)):
  - The server holds a generic relay log `{seq, from, payload}`. It stamps `from` from the auth token and does not interpret the payload.
  - Clients apply the server-ordered log with a deterministic "applier". Invalid entries are ignored identically by everyone. A client does not apply its own actions optimistically.
  - Reconnect means fetching the relay log and replaying it from scratch.
  - The Arena reused the server unchanged.
- **Offline-first PWA** ([ADR 0003](/Users/Shared/Coding/aegis-draft/docs/adr/0003-offline-first-pwa.md)):
  - a service worker precaches the app shell;
  - the dataset swaps atomically by `dataHash`;
  - art is mirrored in git instead of cached from the Steam CDN (opaque responses, tainted canvas, captive-portal risk);
  - fonts are bundled from `@fontsource-variable`.
- **Assets in git.** About 649 MB of art: sprites 545 MB (`dota_px` 817 WebP / 132 MB; `dota_px2` 815 WebP / 412 MB; LPC 980 KB) and SFX 98 MB (`du` on [REPO/web/public/art](/Users/Shared/Coding/aegis-draft/web/public/art)). The GitHub Pages limit (~1 GB) drove later weight cuts ([sprites doc §7a](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md)).

#### Tooling
- **Balance bot `sim_arcade.ts`** (501 lines; [sim_arcade.ts](/Users/Shared/Coding/aegis-draft/web/scripts/sim_arcade.ts)).
  - Policy: kite from the enemy center of mass, collect XP, buy in the shop, Blink away from Roshan's telegraphed hit.
  - Flags: `--runs --seed --hero --act --rank --school --trait --composition --contract [oath] --build --gift --places --noblink --nobuyback --rivalfight --career N --gear --gear-tier --set k=v --enemy kind.f=v --talents --only --trace --verbose`.
  - `--career N` plays consecutive runs like a real player: gear carries over, Rapier and Aegis are lost on use or death, seals are spent.
- **Other balance and determinism scripts:** `sim_kit.ts` (probes one ability's contribution over 3 seed bases); `cross_engine_digests.mts` (the same bot runs in Chromium/V8, WebKit/JSC and Node must produce identical digests); `bench.mts` (µs per tick plus a digest; lived in the scratchpad). Sources: [cross_engine_digests.mts](/Users/Shared/Coding/aegis-draft/web/scripts/cross_engine_digests.mts), [MEM/arcade-blink-m16.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-blink-m16.md).
- **Sprite pipeline:** `blender/render_dota_sprites.py` (1,081 lines), `blender/dota_pipeline.sh` (129), TSV manifests (462 lines each for px and px2; 355 each for the part-layer manifests), `dota_part_layers.mts` (335), `dota_item_index.mts` (209, which parses `items_game.txt`), `dota_vpk_list.mts` (39, a VPK directory parser), `sheet_single_dir.mts`, `pixel_terrain.mts`, `gen_sheet_index.mjs`.
- **QA scripts:** `qa_sheet_health.mts`, `qa_sprite_sheets.mts`, `qa_frame_gaps.mts`, `qa_frame_drift.mts`, `qa_part_attach.mts`, `qa_set_slots.mjs`, `qa_clip_pick.mjs`.
- **Audio scripts:** `dota_sounds.sh`, `dota_voice.sh`, `dota_sfx_pack.py`, `dota_ability_icons.sh`. Source: `wc` on [REPO/web/scripts](/Users/Shared/Coding/aegis-draft/web/scripts).
- **CI** ([ci.yml](/Users/Shared/Coding/aegis-draft/.github/workflows/ci.yml)):
  - jobs: shared skills (metadata/links/adapters); Go pipeline; Go server (with a Postgres service); Web (mock dataset → schema validation → Vitest → build → Playwright smoke → offline spec on the production build); Web on the real dataset; antipattern scan; Deploy to Pages (needs pipeline, web, antipatterns);
  - a daily 06:00 UTC data-refresh cron commits data ([data-refresh.yml](/Users/Shared/Coding/aegis-draft/.github/workflows/data-refresh.yml));
  - the Fly.io server deploy is skipped while `FLY_API_TOKEN` is missing ([README](/Users/Shared/Coding/aegis-draft/README.md)).

#### Process
- **`CLAUDE.md` (= `AGENTS.md` symlink)** ([CLAUDE.md](/Users/Shared/Coding/aegis-draft/CLAUDE.md)):
  - "Rule 0 — Reuse-first", with the formula "NOT creating X, Y — A and B exist; creating only Q";
  - the data contract;
  - frontend layering (`design/` · `i18n/` · `ui/` · `features/` · `app/`);
  - game-state layering: mode shell ≠ `RunConfig` ≠ `RunEngine`;
  - a "Before done" checklist; "Push ≠ deploy";
  - port etiquette: 5173 for the owner, 5273 for agents;
  - skill discipline: a new skill comes "from a real bump".
- **17 skills, 649 lines in total, each short (14–81 lines)** ([.claude/skills](/Users/Shared/Coding/aegis-draft/.claude/skills), [INDEX](/Users/Shared/Coding/aegis-draft/docs/ai/INDEX.md)):
  - a routing table ([INDEX](/Users/Shared/Coding/aegis-draft/docs/ai/INDEX.md));
  - rules in [PRINCIPLES](/Users/Shared/Coding/aegis-draft/docs/ai/PRINCIPLES.md): `description` ≤200 chars, SKILL.md ≤500 lines, one canonical source with symlink adapters for Codex/Cursor;
  - a Python validator run in CI;
  - a trigger-case doc.
- **Hooks.** Non-blocking PostToolUse reminders for the data contract, frontend tokens and "Push ≠ deploy" ([settings.json](/Users/Shared/Coding/aegis-draft/.claude/settings.json), [hooks](/Users/Shared/Coding/aegis-draft/.claude/hooks)).
- **Antipattern scan.** Blocking categories are a data-contract violation, the forbidden role split, a literal secret, and mock data in the real dataset ([antipatterns_grep.sh](/Users/Shared/Coding/aegis-draft/.claude/skills/self-review-checklist/tools/antipatterns_grep.sh)).
- **Product docs.**

  | Doc | Size and status |
  |---|---|
  | [PRD](/Users/Shared/Coding/aegis-draft/docs/PRD.md) | 1,031 lines / 239 KB, versioned v0.2–v0.16 |
  | [BACKLOG](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md) | 4,262 lines / **1.27 MB** |
  | [ADRs](/Users/Shared/Coding/aegis-draft/docs/adr) | 3 |
  | [audits](/Users/Shared/Coding/aegis-draft/docs/audits) | dated parity/design audits with an evidence folder |
  | [MEMORY index](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/MEMORY.md) | 45 memory notes plus the MEMORY.md index (46 files, about 223 KB), each note with Why / How to apply |

- **Multiple agents.** Different models audited each other's work:
  - the 2026-09-15 customization audit reviewed "the assembly Claude started" ([audit 2026-09-15](/Users/Shared/Coding/aegis-draft/docs/audits/2026-09-15-customization.md));
  - memory notes refer to "Astra" and "Codex" audits ([MEM/arcade-stage1-camp-loop.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-stage1-camp-loop.md), [MEM/arcade-parts-families.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-parts-families.md)).

### Inferences
- **Breadth was cheap.** One month of agent-driven work produced very broad content. The mechanism was data-driven kits (a "kind + params" ability model, 55 kinds reused by 126 heroes) plus a single monolithic sim class. The cost moved into per-hero balance calibration and a 5.7k-line god class whose refactor was only partly done (see §3).
- **The process layer is the most mature asset** and is mostly stack-agnostic: CLAUDE.md rules, short skills with a validator, memory notes in Why/How form, dated audits, and "verify, don't trust ✅".
- **Doc drift was measurable** (README and PRD versus the code). A new repo would benefit from generated counts, or tests that assert documented numbers.

### Gaps
- Current bot win rates were not re-measured (read-only constraint). The latest numbers are the M26 notes dated 2026-10-05.
- `HeroId` is still typed as "Unique" (5) versus "Template" (121) in [heroes.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/heroes.ts), while the PRD says archetype templates were removed and every hero has its own kit. The type names look historical; I did not verify each kit.
- Ability-kind counts include a `signature` pseudo-kind, so "55 kinds" slightly over-counts distinct mechanics.

---

## 2. The Arcade mode: key design decisions, rationale, and what worked or failed

### Takeaway
The core bets paid off technically:
- determinism from day one, which delivered replay, daily runs and bit-exact bot regression;
- a headless bot harness as the balance oracle;
- strict isolation from the rest of the app;
- a 3D-model → pixel-sprite pipeline.

The big failures were measurement and verification failures, not architecture failures:
- a doc promise of determinism that the code broke (35 `Math.sin` calls);
- a bot that ignored W/E abilities;
- a difficulty ladder calibrated under false gear assumptions;
- infinite loops on modal states;
- sprite defects found by the owner's screenshots instead of by metrics.

The owner's "is it fun for 5 minutes" playtest gate was never formally closed.

### Cited findings

#### Origin and scope decisions
- **Genesis.** The owner asked for "another 2D roguelike like Death Must Die, with Dota heroes". The brief's verdict was "feasible as a bounded vertical slice, not a clone". It recommended slice 0 (one hero, one map, 7 minutes to Roshan) and a go/no-go on the playtest question "is it fun for 5 minutes". Full DMD parity was explicitly not planned ([brief §0, §3.4](/Users/Shared/Coding/aegis-draft/docs/arcade-survivors-brief.md), [BACKLOG M13](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- **Owner decisions of 2026-09-05** (T13.0): go; Canvas 2D with no dependencies; "token" medallion art plus procedural VFX; working name "Arcade"; Juggernaut first; pro-player signatures deferred ([BACKLOG T13.0](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- **Translation principle.** "Take the reference's mechanics, but every entity must read to a Dota player without explanation." DMD gods became item "schools" capped at 3 per run, and DMD bosses at 7:00/14:00 became Roshan with the Aegis as a one-time resurrection ([brief §2](/Users/Shared/Coding/aegis-draft/docs/arcade-survivors-brief.md)).
- **Reference principles borrowed** (2026-09-08 audit):
  - Dead Cells: cursed biomes/chests tie extra risk to better loot;
  - Balatro: jokers change rules, Boss Blinds impose constraints;
  - DMD: blessings, items and shrines produce different builds;
  - Vampire Survivors: run gold buys upgrades for the next character.

  Explicitly not imported: Dead Cells' one-hit death in a dense survivors crowd ([audit 2026-09-08](/Users/Shared/Coding/aegis-draft/docs/audits/2026-09-08-arcade-content-and-progression.md)).

#### Determinism
- **What worked.** The input log is keyed by `step()` number, not by tick, because the tick stops during card selection; the first replay test failed on exactly this. The DoD was "two runs of 10,000 ticks produce identical snapshots" ([BACKLOG T13.1](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- **What failed.** JS `Math.sin/cos/atan2/hypot/pow` are implementation-approximated. V8 and JavaScriptCore diverged in 9 of 10 bot runs within 2–5 minutes. "The PRD always promised 'no Math.sin', yet the sim had 35 calls — a promise in a doc is not compliance" ([MEM/arcade-m24-determinism-steal.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-m24-determinism-steal.md)).
- **The fix.** `game/dmath.ts` (Taylor series, using only `+ − × ÷`, `Math.round`, `Math.sqrt`; precision about 1e-12), enforced by `gameDeterminism.test.ts` and `cross_engine_digests.mts`. In M25 the Box–Muller `rng.normal` (built on `Math.log/cos`) also diverged across Chromium, WebKit and Node, so the rule was extended to all game logic ([dmath.ts](/Users/Shared/Coding/aegis-draft/web/src/game/dmath.ts), [MEM/arcade-m25-uniques-slots-ancients.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-m25-uniques-slots-ancients.md)).

#### Balance harness: early success, later traps
- **Slice 0 calibration.** a0.1.0 gave 0% Roshan kills. Five fixes brought a0.2.0 into the target band (to Roshan 70–75%, kill 42–55%, win 35–52%):
  1. melee range (74) was below the boss contact zone; raised to 88;
  2. Roshan could be kited forever; he got a lunge;
  3. a punish window after each hit;
  4. regen 1.6 → 4/s;
  5. XP pickup radius 64 → 112, which needed a quadratic XP curve.

  Source: [BACKLOG T13.6](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md).
- **Noise.** 20 runs ≈ ±11%, 30 ≈ ±9%, 60 ≈ ±7%. Six-run screening was useless (16.7% step). A full act needs ≥36 runs (0/12 later turned out to be 6/36). Confirm on 2–3 seed bases ([MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md), [MEM/arcade-blink-m16.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-blink-m16.md)).
- **Bot blind spot.** The bot levels only Q and R: its pick priority was ult 60, Q 40, W 30, E 20, and any school card 50–53. W/E edits changed no number at all. The design rule that followed: "the Q slot must carry each hero's main clear". The 21 heroes whose ult does no damage and the 50 whose Q does not clear were exactly the bottom of the sweep ([MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md)).
- **Ability probe.** `sim_kit.ts` measured a noise threshold of ±20%. An earlier claim of "negative ability contribution" based on single runs had to be publicly retracted (same source).
- **Wrong ladder model.** The ladder was calibrated with fixed-gear runs (`--gear`), which produced a "saw" pattern. The new `--career` mode showed the bot already in full tier-3 gear with 4 arcanas on entering Guardian (run 7): the first full-act win passes 14:00, where tier-3 drops.
  - Old linear multipliers (+6% HP, +5% damage per step) gave Guardian–Archon 93–96% wins, made Guardian easier than Herald, and let the bot clear the whole ladder in 51 runs.
  - The new concave curve (Herald 2–5★ steep, then +4.5%/+4% per step) gives a smooth 83 → 40% from Guardian to Immortal, with the ladder taking about 58 runs.

  Sources: [BACKLOG T26.7](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [MEM/arcade-m26-rivals-ladder.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-m26-rivals-ladder.md).
- **Meta pace** (T26.8): Legacy reaches 20/20 at about the 24th run, and full tier-3 gear with arcanas comes by run 7. The shard economy is effectively off because cosmetics are free. With full shard prices the first arcana would come around run 10, and 54 of 296 pool items would be owned by the end of the ladder ([BACKLOG T26.8](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- **Band drift.** The bot target band was Herald 30–50% short / 15–25% full ([MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md)). By M20–M23 the bot sat at about 65–73% on Herald short, for example M23 averaging 69.4 → 72.8% over 18 heroes. The owner's decision on the Herald band (T18.1) is still open ([BACKLOG calibration M20–M23, T19.5](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- **Neutral casts.** Frequency, not strength, decides difficulty. Two telegraphs at once with a 1.2 s gap dropped melee Juggernaut from 73 to 38%; one at a time with a 2.5 s gap gave 72%. Weakening the effects at the same frequency did nothing ([BACKLOG calibration M20](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- **Healing interacts with bot heuristics.** Honestly halving a heal raised Omniknight's bot win rate by 12–18 pp: with strong healing the bot stayed above its retreat threshold and died at Roshan ([BACKLOG calibration M21](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- **Sweep cost.** 126 heroes × 60 short runs ≈ 70 min on 9 processes; a full act for 40 heroes ≈ 50 min; 24 careers ≈ 25 min ([BACKLOG calibration M25/M26](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).

#### Modal deadlocks
`ArcadeSim.step` returns without advancing `tick` while a modal is open (pending, shop, neutral, loot). A bot sending an action the modal does not understand loops forever. One seed spun for 22 minutes at tick 21706, which "looks like a slow run". A test loop `while(!cond && tick<N) step()` once hung CI for 6 hours, because Vitest cannot abort a synchronous loop.

Fixes: handle all modal states first in the bot; add a "tick not advancing" watchdog; give windows one source of truth, `sim.activeModal()` ([MEM/sim-arcade-modal-deadlock.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/sim-arcade-modal-deadlock.md), [MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md), [MEM/arcade-blink-m16.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-blink-m16.md)).

#### Object-pool hazards
- Enemy objects are reused. A stale `roshan` reference pointed at a recycled kobold, so "Roshan alive" muted spawns for the rest of the run. This corrupted all measurements a0.2–a0.5 after 7:00 ([BACKLOG T13.9](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- Spawning inside `killEnemy` hands out the dying object. Death effects must be queued (`drainDeathEffects`) ([arcade-simulation skill §3](/Users/Shared/Coding/aegis-draft/.claude/skills/arcade-simulation/SKILL.md)).

#### Silent content bugs
- **Wrong slot lookups.** Abilities were looked up by slot letter instead of by kind, so 11 abilities did nothing (for example Pangolier's Rolling Thunder and Chen's Hand of God). A test now forbids `H.<slot>.kind === "…"`.
- **Ambiguous units.** The `ward` value was sometimes a fraction of max HP and sometimes flat HP, so Witch Doctor, Warlock and Undying "simply didn't die" ([MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md)).
- **Dead ability ranks.** 35 abilities had ranks 2–4 that changed nothing (fixed in T16.3) ([BACKLOG T15.5](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).

#### Owner directives that became design laws
- **Summons are entities.** "Any summon/mount/illusion runs after its owner and attacks." A "damage zone with a picture on top" is banned; only Dota-static totems stay rooted. `SUMMON_DPS` was calibrated against HEAD, because live summons add +30–60% kills ([MEM/arcade-summons-are-entities.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-summons-are-entities.md)).
- **Pick up loot with a button, not on touch.** There is also a build screen with a bag ([MEM/arcade-loot-build-effects.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-loot-build-effects.md)).
- **Every hero must be unique.** Archetype kits were removed ([MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md)).
- **Legacy gives conveniences, not power** (T13.88): pickup radius, speed, shop prices, rerolls and starting gold ([PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md)).
- **Cosmetics are free in all builds** (decision of 2026-09-07) ([PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md)).

#### Map "places" design rules ([MEM/arcade-stage1-camp-loop.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-stage1-camp-loop.md), [audit 2026-09-08](/Users/Shared/Coding/aegis-draft/docs/audits/2026-09-08-arcade-content-and-progression.md))
- **Places sleep until contact.** They wake in an inner ring and are released beyond an outer ring (hysteresis). Without this, a kiting bot ran into the camp at minute 1 and wins fell from 27 to 15%.
- **Own RNG stream per place** (`Rng("place:seed:act")`), so new content does not shift every seed.
- **Act clock ≠ world tick.** The rift pauses the act clock via `pausedTicks`.
- **Limits.** At most 2 optional "invitations" on screen at once. No closing of all exits; barrier waves were rejected for that reason. A champion is never fully immune to a school; it gets capped control with resistance instead.

#### Art path decisions and failures
- **Path:**
  1. procedural rigs, at about 120 fps with 45 enemies;
  2. LPC/Kenney placeholders;
  3. the owner's "path A": real Dota models via Source 2 Viewer CLI → glTF → Blender → sprite sheets;
  4. pixel style the docs call "Dead Cells-approach": Workbench flat shading, black outline, no anti-aliasing, pngquant 48 colors without dithering.

  Sources: [BACKLOG T13.13](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [sprites doc §7](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md).
- **Two pixel sets chosen by device pixel ratio.** Hero frames are 80 px in `dota_px` and 160 px in `dota_px2`. The owner saw "soap" (blur) on Retina because of factor 2, that is, 4 physical pixels per art pixel ([MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md)).
- **Sprite defects found by the owner, not by tooling.** Seven owner screenshots showed Drow without arms, Muerta stuttering, PA and Terrorblade black, Razor grey and QoP white. These came after three "fixed" reports that each checked a single idle row at a single angle. Every defect proved systemic and measurable, which led to `qa_sheet_health.mts` (strobe, jerk, dropout, pieces, darkness, empty frames across all frames and directions) ([MEM/arcade-sprite-qa.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-sprite-qa.md)).
- **Timings:**

  | Job | Time |
  |---|---|
  | One sheet (frame 160, 8 directions) | ≈25–30 s |
  | 47 sheets | ≈30 min |
  | 70 rows | ≈35 min |
  | 293 sheets in 3 parallel chunks | ≈1.5 h |
  | Cosmetic slice 2 (12 families, ~200 sheets) | ≈2 h of background Blender |
  | `qa_sheet_health` on 293 sheets | ≈10 min |
  | `qa_sprite_sheets` on 276 sheets | ≈1 min |

  Sources: [MEM/arcade-parts-layers.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-parts-layers.md), [MEM/arcade-parts-families.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-parts-families.md), [MEM/arcade-sprite-qa.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-sprite-qa.md), [MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md).
- **Weight.** Rendering death in one direction only removed 129 MB from 1,504 sheets, losslessly via `dwebp -pam` → `cwebp -lossless -exact`. Canvas re-encoding corrupts premultiplied alpha. Re-encoding WebP harder gains only about 1%. Rule: "compute weight BEFORE rendering"; for example, all Drow sets on two bodies would add 180 MB ([MEM/arcade-parts-families.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-parts-families.md), [sprites doc §7a](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md)).

#### Performance
- **Frame time.** Live headless run: up to 136 enemies at p50 8.3 ms, p99 10.4 ms per frame (120 Hz headless), heap 45–60 MB, load 0.3 s. Every frame "hitch" over 50 ms coincided with the test's own `page.screenshot()` ([BACKLOG T15.1](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- **Sim speed** ([MEM/code-revision-2026-09-19.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/code-revision-2026-09-19.md), [arcade-simulation skill §8](/Users/Shared/Coding/aegis-draft/.claude/skills/arcade-simulation/SKILL.md)):
  - ~40% faster from one uniform object shape for all enemy kinds, which fixed V8 inline caches;
  - hot paths are `moveEnemies` (53%) and `resolveInto`;
  - a neighbor cache gave no gain (62.0 → 61.7 µs/tick) and was reverted;
  - every perf change is gated by bit-exact bot digests.

### Inferences
- The two pillars to rebuild first in any engine are a deterministic, headless-steppable sim and a bot plus career harness on top of it. Most of the Arcade's verified knowledge (balance numbers, regressions, perf gates) came through them.
- Many "failures" share one root cause: trusting an unmeasured proxy. Examples: a doc promise, single-angle sprite checks, fixed-gear ladder assumptions, single-seed comparisons, a green CI badge.
- **Content breadth outran validation.** 126 heroes and dozens of map events were built before the owner's "fun for 5 minutes" gate was closed. Bot win rates also stopped matching the original Herald band (65–73% versus 30–50%) with no product decision yet.

### Gaps
- No human playtest metrics exist (session length, retention, perceived difficulty). The only "fun" signals are owner feedback quoted in BACKLOG and memory.
- Mobile/Android/Telegram Mini App live checks of Arcade are still open (T13.10 ⬜) ([BACKLOG T13.10](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- The current per-hero balance spread after M26 was not re-measured here.

---

## 3. Lessons that should become rules or skills in the new repo (with evidence)

### Takeaway
About 30 hard-won lessons generalize beyond Dota and TS. They cluster into:
- determinism and replay;
- sim correctness invariants;
- bot-based balance statistics;
- headless QA and CI hygiene;
- the art pipeline with metric QA and weight budgets;
- owner and agent collaboration process.

Most already exist as skill text or memory notes and can be rewritten engine-neutrally.

### Cited findings (each item is a rule plus its evidence)

#### Determinism and replay
1. **A determinism promise needs an enforcing test and a cross-platform digest check.** The sim had 35 `Math.sin` calls despite the PRD, and 9/10 runs diverged between V8 and JSC → `dmath` + `gameDeterminism.test.ts` + `cross_engine_digests.mts` ([MEM/arcade-m24-determinism-steal.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-m24-determinism-steal.md), [arcade-simulation skill §2](/Users/Shared/Coding/aegis-draft/.claude/skills/arcade-simulation/SKILL.md)).
2. **Key the input log by step, not by tick, and route every player action through the input log.** Pickup, build, bag, shop and contract all go through `step()`; otherwise replays diverge ([BACKLOG T13.1](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [MEM/arcade-loot-build-effects.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-loot-build-effects.md)).
3. **Give each subsystem its own RNG stream.** Moving bot names to another stream changed every bot's strength and broke golden tests. Places use their own `Rng("place:seed:act")`. Any RNG-order change requires a deliberate golden regeneration, checked by eye ([MEM/golden-tests-skip-locally.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/golden-tests-skip-locally.md), [MEM/arcade-stage1-camp-loop.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-stage1-camp-loop.md)).
4. **Replays and dailies snapshot everything that affects outcome** (gear, Legacy, trait, config version) and never read the viewer's current profile. The daily uses a fixed zero meta ([replay.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/replay.ts), [audit 2026-09-08](/Users/Shared/Coding/aegis-draft/docs/audits/2026-09-08-arcade-content-and-progression.md)).

#### Sim correctness invariants
5. **Modal states must never deadlock headless runs.** One `activeModal()` source of truth; bots handle modals before world actions; every step loop has an iteration cap and a "tick not advancing" watchdog ([MEM/sim-arcade-modal-deadlock.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/sim-arcade-modal-deadlock.md), [MEM/arcade-blink-m16.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-blink-m16.md)).
6. **Pool safety.** Clear entity references on death. Defer spawns to the end of the tick. Queue death-triggered effects instead of recursing ([BACKLOG T13.9](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [arcade-simulation skill §3](/Users/Shared/Coding/aegis-draft/.claude/skills/arcade-simulation/SKILL.md)).
7. **Act clock ≠ world tick.** Schedules read act time; status timers read world time; any world pause increments a pause counter ([MEM/arcade-stage1-camp-loop.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-stage1-camp-loop.md)).
8. **Look up abilities by kind, not by slot,** and give every value field explicit units, with a "every kind reads its value" check ([MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md)).
9. **Run-time hero data is a copy** (`cloneHeroDef`). Talents mutate the copy, never shared content ([arcade-simulation skill §9](/Users/Shared/Coding/aegis-draft/.claude/skills/arcade-simulation/SKILL.md)).
10. **UI numbers must come from the same formulas as the sim.** `upgradeFigures` mirrors the sim, and `arcadeFigures.test.ts` catches divergence. Signature texts read hero numbers (`SIG_CAPS`) ([MEM/arcade-loot-build-effects.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-loot-build-effects.md), [PRD §5.15 T24](/Users/Shared/Coding/aegis-draft/docs/PRD.md)).
11. **One offer pipeline.** Every blessing source goes through `rollUpgradeOffer`, which enforces type slots. Every new enemy field goes through `uniform()`, or it is silently undefined ([arcade-simulation skill §10](/Users/Shared/Coding/aegis-draft/.claude/skills/arcade-simulation/SKILL.md)).

#### Encounter design
12. **Optional places sleep until contact, with wake/leash hysteresis.** Telegraph frequency matters more than telegraph strength. Never close all exits. Bosses get capped control with resistance, not immunity ([MEM/arcade-stage1-camp-loop.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-stage1-camp-loop.md), [BACKLOG calibration M20](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [audit 2026-09-08](/Users/Shared/Coding/aegis-draft/docs/audits/2026-09-08-arcade-content-and-progression.md)).

#### Balance methodology
13. **Win-rate statistics.**
    - Use ≥20–30 runs to read a band and ≥36 for a full act.
    - Confirm on 2–3 seed bases and across at least two hero classes.
    - Identical numbers in every cell of a parameter grid mean the override was not applied (an argv or zsh bug), not "no effect".

    Sources: [MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md), [MEM/arcade-stage1-camp-loop.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-stage1-camp-loop.md), [MEM/dota-vpk-source-of-truth.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/dota-vpk-source-of-truth.md). For the roguelite mode, n=120 has a standard error of about 4 pp at 35%, which is as large as the effects being measured ([MEM/balance-calibration-offline-sweep.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/balance-calibration-offline-sweep.md)).
14. **Measure baselines from a frozen snapshot** (`git archive HEAD` plus a symlinked `node_modules`), never from the live working tree in the middle of a series ([MEM/arcade-blink-m16.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-blink-m16.md)).
15. **Know your bot's blind spots and report artifacts as artifacts.**
    - The bot skips W/E and ranks school cards above ability ranks.
    - Healing changes its retreat behavior.
    - A new mechanic must be usable by the bot, or its cards are dead picks for it.
    - Decompose effects with an ablation snapshot plus `--set lever=1e6`.

    Sources: [MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md), [MEM/arcade-blink-m16.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-blink-m16.md), [arcade-simulation skill §7](/Users/Shared/Coding/aegis-draft/.claude/skills/arcade-simulation/SKILL.md).
16. **Calibrate the difficulty ladder and meta pace with career simulation** (consecutive runs with carried-over gear), not fixed-loadout runs. Calibrate the top of the ladder together with the middle ([BACKLOG T26.7](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [MEM/arcade-m26-rivals-ladder.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-m26-rivals-ladder.md)).
17. **Dump distributions and sweep offline,** with a final confirming sim run. Report metrics per act, not just the total ([MEM/balance-calibration-offline-sweep.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/balance-calibration-offline-sweep.md)).
18. **Version the balance config;** a version change invalidates incompatible saves and replays on purpose ([MEM/balance-config-versioning.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/balance-config-versioning.md), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md)).

#### Performance
19. **Perf work only with a profile plus bit-exact digests before and after.** Do not optimize from audit lists ([MEM/arcade-blink-m16.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-blink-m16.md), [arcade-simulation skill §8](/Users/Shared/Coding/aegis-draft/.claude/skills/arcade-simulation/SKILL.md)).

#### QA, CI and verification
20. **Headless QA with a dev hook** (`window.__arcadeSim()` to fast-forward the clock, teleport, open windows). Hidden browser panes give black screenshots. Screenshots themselves cause 100–300 ms hitches, so frame measurements must be cross-checked against screenshot moments ([MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md), [MEM/code-revision-2026-09-19.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/code-revision-2026-09-19.md), [MEM/qa-playthrough-headless-playwright.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/qa-playthrough-headless-playwright.md)).
21. **CI hygiene.**
    - The CI runner is 3–4× slower than an M-series Mac.
    - Long synchronous sims starve test-runner RPC ("Timeout calling onTaskUpdate"), so yield between heroes.
    - Publish log tails as annotations.
    - "An expired monitor is not green CI."
    - Bot commits made with the default `GITHUB_TOKEN` do not trigger CI.

    Sources: [MEM/vitest-ci-worker-rpc.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/vitest-ci-worker-rpc.md), [self-review-checklist skill](/Users/Shared/Coding/aegis-draft/.claude/skills/self-review-checklist/SKILL.md), [MEM/code-revision-2026-09-19.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/code-revision-2026-09-19.md).

#### Art pipeline
22. **Sprite QA by metrics over all frames and directions,** then a contact sheet by eye.
    - Frame the sprite on the most compact of the stance and walk poses.
    - Take clip names from the main model only.
    - Detect the loop period by autocorrelation.
    - Neutralize root motion on the load-bearing torso bone.
    - Compute the foot anchor by projecting the model origin.

    Sources: [MEM/arcade-sprite-qa.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-sprite-qa.md), [MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md), [sprites doc "What to do if"](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md).
23. **Budget asset weight before rendering.** Drop unused rows (death in one direction). Use lossless byte-level row moves, never a canvas round-trip. Keep a sheet index so missing sheets are not discovered through 404s, plus a test that the index is fresh ([MEM/arcade-parts-families.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-parts-families.md), [sprites doc §5, §7a](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md)).
24. **Pipeline mechanics.** Never edit a running bash pipeline script. Clear stale export folders. Keep tools in the repo, because the OS cleans the scratchpad within days ([MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md), [MEM/arcade-m25-uniques-slots-ancients.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-m25-uniques-slots-ancients.md), [MEM/arcade-blink-m16.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-blink-m16.md)).

#### Saves and progression
25. **Permanent unlocks must not be derived from a truncated history.** Rewards are granted once per stable completion ID. Formats carry migrations, for example `migrateCosmeticsV2`. Read errors never overwrite saves with empty data. Arcade unlocks were lost because they were computed from the last 50 runs; fixed by T13.37 ([save-compatibility skill](/Users/Shared/Coding/aegis-draft/.claude/skills/save-compatibility/SKILL.md), [audit 2026-09-08](/Users/Shared/Coding/aegis-draft/docs/audits/2026-09-08-arcade-content-and-progression.md)).

#### Game feel
26. **Game-feel escalation ladder.**
    - Feedback is proportional to the event; shake only at climaxes.
    - Numbers that matter for decisions never count up.
    - Sound escalates by pitch and layering, not volume.
    - The game must be fully playable in silence.
    - Crit damage numbers are bigger and red, with their own sound.

    Sources: [game-feel-juice skill](/Users/Shared/Coding/aegis-draft/.claude/skills/game-feel-juice/SKILL.md), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md).

#### Collaboration process
27. **Owner collaboration.**
    - "What else to add?" without "implement" means: propose a list and let the owner pick.
    - Owner screenshots usually reveal systemic defects; fix the class, not the instance.
    - Statuses in the backlog drift; grep the code before trusting ✅ or ⬜.

    Sources: [MEM/arcade-blink-m16.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-blink-m16.md), [MEM/arcade-sprite-qa.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-sprite-qa.md), [MEM/backlog-status-revision.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/backlog-status-revision.md).
28. **Parallel agent work.**
    - Use APFS-clone worktrees with explicit file zones and separate ports; integrate by cherry-pick.
    - A session limit stops all agents at once; resume them by agent ID.
    - About 10% of audit agents' "dead code" findings are false; confirm with a full test run.

    Sources: [MEM/code-revision-2026-09-13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/code-revision-2026-09-13.md), [MEM/code-audit-2026-09-02.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/code-audit-2026-09-02.md).
29. **Token-efficient agent work.** Edit with tools, not `sed`. Never read huge docs whole: map headings first, then read offsets. Prefer text checks over screenshots ([MEM/token-efficient-workflow.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/token-efficient-workflow.md)).
30. **Solo workflow.** Commit straight to main, protected by checks before the commit. Push ≠ deploy: wait for CI and verify live ([MEM/commit-straight-to-main.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/commit-straight-to-main.md), [CLAUDE.md](/Users/Shared/Coding/aegis-draft/CLAUDE.md)).
31. **Shell pitfalls.**
    - zsh does not word-split `$arg`, so flags are silently ignored.
    - `xargs -L 1` joins lines that end in a space.
    - Two concurrent `npx` runs hang each other.
    - Use bash scripts for parameter grids.

    Sources: [MEM/arcade-blink-m16.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-blink-m16.md), [MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md), [MEM/code-revision-2026-09-13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/code-revision-2026-09-13.md).

#### Doc and repo hygiene
32. **Keep docs small and rotate them.** The BACKLOG grew to 1.27 MB and 4,262 lines, so it can only be read in slices. The memory note "Не читать BACKLOG целиком" ("don't read the BACKLOG whole") already exists ([BACKLOG](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [MEM/token-efficient-workflow.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/token-efficient-workflow.md)).
33. **Secrets hygiene.** A plaintext `api_keys.txt` sits in the repo root. It is gitignored, and I did not open it. CLAUDE.md requires secrets from env ([.gitignore line 21](/Users/Shared/Coding/aegis-draft/.gitignore), [CLAUDE.md](/Users/Shared/Coding/aegis-draft/CLAUDE.md)).

### Inferences
Candidate skills for the new repo, re-cut engine-neutrally from the above:
- `sim-determinism` (items 1–4, 19);
- `sim-invariants` (5–11);
- `encounter-design` (12);
- `balance-harness` (13–18);
- `headless-qa-and-ci` (20–21);
- `sprite-pipeline-qa` (22–24);
- `save-and-progression` (25);
- `game-feel` (26).

The existing `discovery-before-code`, `plan-first-communication`, `self-review-checklist`, `reference-parity-audit` and `agent-skill-maintenance` transfer almost verbatim, with project names replaced.

**Native-engine caveat (general knowledge, not from the repo).** Native float determinism has different failure modes: compiler FMA contraction, x87 versus SSE, platform libm differences, SIMD reassociation, and engine physics that is not deterministic. Rule 1 should therefore become "deterministic math library or fixed-point, plus a cross-platform/cross-build digest CI job", not "avoid `Math.sin`".

### Gaps
- No lessons exist yet about native-specific concerns (asset import pipelines in an engine editor, console certification, Steam builds), because the project never shipped natively.
- The `api_keys.txt` observation is from a directory listing only. Its contents and whether it is still needed were not inspected (deliberately).

---

## 4. Transfer map for a new native, original-IP repository

### Takeaway
- **Transfers as-is:** almost all process assets and most tooling *ideas* (bot harness, career sim, digest gates, sprite metric QA).
- **Transfers as design:** the run structure, the system shapes and the difficulty-ladder method.
- **Transfers as code only if the stack stayed TS/web:** the sim, renderer, store, PWA and tests. In a native engine they must be rewritten.
- **Must not transfer:** anything Valve/Dota (names, models, sounds, icons, data domain) or the share-alike LPC art.
- **Unfinished:** the backend deploy, the owner playtest gate and several content and polish tails.

### Cited findings

#### (a) Transfers as-is: process, docs and tooling ideas
- **CLAUDE.md/AGENTS.md structure:**
  - Rule 0 reuse-first ("NOT creating X…");
  - an explicit layer-boundary list;
  - a "Before done" checklist with checks proportional to the change;
  - "Push ≠ deploy";
  - port etiquette for agents versus the owner;
  - "skill from a real bump".

  Source: [CLAUDE.md](/Users/Shared/Coding/aegis-draft/CLAUDE.md).
- **Skills system** ([PRINCIPLES](/Users/Shared/Coding/aegis-draft/docs/ai/PRINCIPLES.md), [validate_skills.py](/Users/Shared/Coding/aegis-draft/.claude/skills/agent-skill-maintenance/tools/validate_skills.py), [skill-trigger-cases.md](/Users/Shared/Coding/aegis-draft/docs/ai/skill-trigger-cases.md)):
  - one canonical `.claude/skills/<name>/SKILL.md` with symlink adapters for Codex (`.agents/skills`) and Cursor;
  - a `description` that states both trigger and boundary in ≤200 chars;
  - an INDEX routing table and composition rules;
  - a Python validator in CI;
  - trigger cases.
- **Generic skills reusable nearly verbatim:** `discovery-before-code`, `plan-first-communication`, `self-review-checklist` (minus data/OpenDota items), `reference-parity-audit` (with its `validate_audit.mjs` and audit template), `agent-skill-maintenance`, `game-feel-juice` (escalation ladder and audio rules), `save-compatibility`, `game-state-architecture` (mode shell ≠ run config ≠ engine; reset keeps the mode; confirm on progress loss). Source: [.claude/skills](/Users/Shared/Coding/aegis-draft/.claude/skills).
- **Doc artifacts:**
  - ADR format;
  - PRD with change-log versions;
  - BACKLOG items with goal/files/skills/DoD;
  - dated audits with an evidence/provenance folder;
  - the memory-note format (name, description, type; Why; How to apply; links).

  Sources: [docs/adr](/Users/Shared/Coding/aegis-draft/docs/adr), [BACKLOG T13.1 format](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [audits/evidence](/Users/Shared/Coding/aegis-draft/docs/audits), [MEM](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/MEMORY.md).
- **Tooling ideas, engine-agnostic:**
  - the headless bot with A/B flags (`--set`, `--enemy`, ablation flags), `--career N`, `--places`, `--trace`/`--only`;
  - a `sim_kit`-style single-ability probe with a measured noise threshold;
  - a bench that prints µs/tick plus a digest;
  - a cross-platform digest comparison;
  - an antipattern scanner with blocking categories gating deploy;
  - non-blocking reminder hooks.

  Sources: [sim_arcade.ts](/Users/Shared/Coding/aegis-draft/web/scripts/sim_arcade.ts), [cross_engine_digests.mts](/Users/Shared/Coding/aegis-draft/web/scripts/cross_engine_digests.mts), [antipatterns_grep.sh](/Users/Shared/Coding/aegis-draft/.claude/skills/self-review-checklist/tools/antipatterns_grep.sh).
- **Art QA metrics and render-pipeline techniques (independent of Valve data):**
  - Workbench flat shading with an outline;
  - a 48-color palette without dithering;
  - a per-DPR pixel factor;
  - root-motion neutralization on the load-bearing bone;
  - loop-period autocorrelation;
  - "most compact pose" framing;
  - fit-from shared framing so layers align;
  - single-direction death clip;
  - quantize the layer, not the raw diff;
  - strobe/jerk/dropout/pieces/darkness metrics.

  Sources: [sprites doc §3, §7, §7a, §8](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md), [MEM/arcade-sprite-qa.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-sprite-qa.md), [MEM/arcade-parts-layers.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-parts-layers.md).

#### (b) Transfers as design
- **Run skeleton.** A 20-minute act with an XP level-up card economy. Scheduled bosses at about 7/14 minutes stop normal spawns, except where a boss is tied to an arena. An optional mid-boss. A final objective at 20:00 with escalation after it. A warm-up short act for onboarding ([PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md), [brief §1–2](/Users/Shared/Coding/aegis-draft/docs/arcade-survivors-brief.md)).
- **Build system:**
  - ≤3 schools per run;
  - 7 blessing types, with one slot each for active types and no cap on power/passive;
  - rarities standard/refined/exotic/arcana;
  - legendaries rarely offered from level 8, guaranteed at 12/18/24;
  - a hybrid for every school pair (10 for 5 schools);
  - dependencies, reroll, banish;
  - "cast" blessings so the hero kit feeds school statuses (at most once per second per target).

  Sources: [schools.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/schools.ts), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md).
- **Status palette:** burn (strongest damage-over-time with duration refresh); chill → freeze; stun; rupture (damage on movement); corrosive armor amp (capped); poison (up to 5 stacks with a shared refreshing timer). Bosses get capped control with resistance, not immunity ([audit 2026-09-08](/Users/Shared/Coding/aegis-draft/docs/audits/2026-09-08-arcade-content-and-progression.md), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md)).
- **Difficulty ladder method.**
  - 40 steps (8 named tiers × 5 stars). Each tier adds one named rule: double golems, bigger waves, troll packs, frequent sieges, early boss, status resistance, less XP. Low tiers have a gentle start.
  - HP/damage multipliers are concave: steep early, then flat.
  - Unlock the next step by winning a full act.
  - Calibrate by career simulation.

  Sources: [ranks.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/ranks.ts), [BACKLOG T13.7, T26.7](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md).
- **Map structure:**
  - seeded compositions of "places" with an act property;
  - invariants: a pond, an outpost and ≥2 champions;
  - the rift and forge are base beats;
  - "come and it's yours" timers (events do not expire while the hero is within 220 px);
  - at most 2 optional invitations on screen;
  - a minimap with off-screen loot pointers.

  Sources: [compositions.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/compositions.ts), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md).
- **Persistence and meta design:**
  - persistent gear: 6 slots, 34 bases, 1–4 affixes by rarity, salvage values;
  - high-risk uniques: the Rapier is lost on death; the resurrection item burns when used;
  - Legacy as capped conveniences;
  - hero mastery marks unlocking starting traits that carry both a plus and a minus;
  - expeditions rewarding titles;
  - a bestiary and mechanics codex unlocked on first encounter;
  - a daily with zero meta;
  - shareable replay codes;
  - cosmetics never in the sim or the seed.

  Sources: [gear.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/gear.ts), [legacy.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/legacy.ts), [traits.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/traits.ts), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md).
- **Architecture as design:**
  - a pure deterministic sim stepped by an input log; the renderer only reads;
  - one input contract for keyboard, gamepad and touch, so all three share replays;
  - HUD glyphs switch on first input, like Steam Input;
  - the game pauses on focus loss and has no mid-run save;
  - a config object with its own version stamped into replays and saves;
  - the relay-log plus deterministic-applier multiplayer pattern for asynchronous or turn-like modes.

  Sources: [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md), [MEM/duel-mode-relay.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/duel-mode-relay.md).

#### (c) Transfers as code only if the stack stayed TS/web; native needs a rewrite
These modules are TS/Canvas/React/Zustand/browser-bound and would need a rewrite in a native engine:

| Module | Size |
|---|---|
| `game/arcade/sim.ts` | 5,726 lines |
| `content/*.ts` | ~3.9k lines |
| `replay.ts`, `dmath.ts`, `rng.ts` | |
| `features/arcade/*` (Canvas 2D renderer, sprite loader and compositor with ByteLru cache, deterministic particles, cosmetic effects, input, soundscape on HTMLAudio/WebAudio, React HUD and wardrobe) | 7,932 lines |
| `state/arcadeStore.ts` | 872 lines |
| PWA service worker and `i18n/arcade.ts` | 4,860 lines (i18n) |
| 107 Arcade test files | 10,451 lines |
| Playwright e2e/QA scripts | |
| `sim_arcade.ts` | 501 lines |

Source: `wc` on [REPO/web](/Users/Shared/Coding/aegis-draft/web).

Note: some web-specific optimizations do not carry over to a native engine, for example "uniform object shape for V8 inline caches" and "HeapNumber boxing" ([MEM/code-revision-2026-09-19.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/code-revision-2026-09-19.md), [arcade-simulation skill §8](/Users/Shared/Coding/aegis-draft/.claude/skills/arcade-simulation/SKILL.md)).

Partially reusable in any stack:
- `render_dota_sprites.py` (Blender Python, 1,081 lines), for its generic camera, lighting, Workbench, outline, quantize and root-motion code. Its S2V/glTF-specific import, Valve material hacks (`--fix-tex`, `--glow-mask` and similar) and manifest columns are Dota-specific ([render_dota_sprites.py](/Users/Shared/Coding/aegis-draft/web/scripts/blender/render_dota_sprites.py), [sprites doc](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md)).
- The algorithms in `qa_sheet_health.mts`, `qa_frame_gaps.mts`, `qa_frame_drift.mts` and `qa_part_attach.mts`, but not their Playwright/WebP plumbing ([web/scripts](/Users/Shared/Coding/aegis-draft/web/scripts)).
- The Go server's account, save (CAS 409) and relay-room logic as a backend pattern ([server/internal](/Users/Shared/Coding/aegis-draft/server/internal)).

#### (d) Must NOT transfer
- **Valve/Dota 2 IP.**
  - Hero names and kits named after Dota abilities (Blade Fury, Omnislash, Spell Steal, Meat Hook…).
  - Item names: Aegis of the Immortal, Divine Rapier, Radiance, Skadi, Maelstrom, Aghanim's Shard/Scepter, BKB, Cheese, Refresher…
  - Bosses and places: Roshan, Tormentor, the Ancient, Radiant/Dire, Secret Shop, bounty/wisdom runes.
  - Ranked medal names Herald → Immortal; announcer lines (Killing Spree… Rampage).
  - All rendered sprite sheets (545 MB), terrain textures, ability and item icons, portraits.
  - All sounds, VO and music (98 MB).
  - `dota_item_index.json` (14,004 lines parsed from `items_game.txt`), the slot overrides and exclusions, every Dota manifest, and the VPK/S2V tooling.

  The README itself states that Dota 2 materials are Valve's property and the project is a non-commercial fan project. Sources: [README license](/Users/Shared/Coding/aegis-draft/README.md), [sprites doc legal frame](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md), [web/scripts/blender](/Users/Shared/Coding/aegis-draft/web/scripts/blender).
- **The OpenDota/Liquipedia data domain:** pipeline, schema, scoring (OVR/synergy/chemistry), draft modes, Manager, Real Tournament, the Telegram bot's Dota branding ([ADR 0001](/Users/Shared/Coding/aegis-draft/docs/adr/0001-tech-stack.md), [README](/Users/Shared/Coding/aegis-draft/README.md)).
- **LPC sprites/tiles,** licensed CC-BY-SA 3.0 / GPL 3.0 with attribution in `web/public/art/sprites/lpc/ATTRIBUTION.md`. Share-alike terms make them unsuitable for a closed commercial game unless deliberately complied with ([README license](/Users/Shared/Coding/aegis-draft/README.md), [BACKLOG T13.13](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- **Names with Dota roots:** "Aegis" itself, including "Aegis Draft" and "Наследие Aegis" ("Aegis Legacy"), is a Dota item name ([brief §2](/Users/Shared/Coding/aegis-draft/docs/arcade-survivors-brief.md)).

#### (e) Known gaps and unfinished items (as of the last commit, 2026-10-05)
- **Backend never deployed.**
  - Fly.io deploy is blocked: `FLY_API_TOKEN` and `VITE_API_BASE` are missing; the README says it stalled on Fly.io card verification.
  - Duel and Arena are unavailable live.
  - Daily server re-simulation (T8.5) and the leaderboard (T8.6) are ⬜. A Go re-sim of the real-time sim is "not planned".

  Sources: [README](/Users/Shared/Coding/aegis-draft/README.md), [BACKLOG M8](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md).
- **Owner playtest gate.** "Fun for 5 minutes" is still listed as the criterion for further slices and balance (T13.3 ⬜). The Herald difficulty band decision (T18.1) is open ([PRD §5.15 "Гейт" (gate)](/Users/Shared/Coding/aegis-draft/docs/PRD.md), [BACKLOG T13.3, T19.5](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- **Partial or open items** ([BACKLOG](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)):

  | Item | Status |
  |---|---|
  | T13.80 slot-based cosmetics | 🟨, slice 3 |
  | T13.36 arcanas for remaining heroes | ⬜ in progress |
  | T13.25 per-hero balance | 🟨 |
  | T13.6 harness calibration | 🟨 |
  | T13.27 wardrobe | 🟨 |
  | T13.69 profile export/import | ⬜ |
  | T13.10 real Telegram Mini App check | ⬜ |
  | T15.4 and T19.5 candidate lists | client perf on the main thread (`scanGlow`/`gemSheet`), gamepad mapping, a11y, a Telegram CloudStorage timeout that can overwrite the career |
  | T26.7 alternative not taken | loot tier cap by rank |

- **Refactor tails:** split `castAbility`/`killEnemy`/`spawnTick`; shared test helpers; a common `ArcadeOverlay`; move `DEV_FREE_SHOP` into sim options ([MEM/arcade-refactor-lessons.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-refactor-lessons.md)).
- **Bot policy limits.** The bot does not learn W/E, does not go to places by default and does not fight rivals by default, so many balance claims about W/E and optional content rest on tests rather than measurement ([MEM/arcade-mode-m13.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-mode-m13.md), [sim_arcade.ts header](/Users/Shared/Coding/aegis-draft/web/scripts/sim_arcade.ts)).
- **Second-pass audit items not done:** alternative starting kits, a frame for titles, rock cover from archer volleys ([MEM/arcade-stage1-camp-loop.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-stage1-camp-loop.md)).

### Inferences
- **Choose an engine that allows headless stepping.** The engine for the new repo should allow a pure simulation library that can run headless and fast: in CI, in a bot harness, and ideally on a server for leaderboard validation. This is the one capability the web prototype proved indispensable and could not extend to its Go server.
- **Port the method, not the scripts.** Building the "Death-Must-Die-like" content again in original IP is mostly a content and naming exercise over the design in (b). The expensive part to rebuild is the art pipeline: either own 3D models through the same render-to-pixel technique, or hand-drawn pixel animation. The Dota pipeline's QA lessons are the reusable part.
- **Asset budgets change on native.** GitHub Pages' 1 GB limit and offline precache drove many weight decisions. On native/Steam the binding constraints become VRAM, texture atlases and download size, but "budget weight before rendering" still applies.

### Gaps
- I did not audit which parts of `render_dota_sprites.py` are cleanly separable from Valve-specific code. A code read is needed before reuse.
- Fonts (Manrope, Space Grotesk via `@fontsource-variable`) are likely open licenses, but I did not verify their license files.
- No engine choice exists in this repo. The transfer map assumes a generic native engine.

---

## 5. Content-design structures worth re-skinning into an original IP

### Takeaway
The structures that carry the design are the **shape**, not the Dota skin:
- a 5-school/pantheon blessing system with pair hybrids and slot-limited active types;
- scheduled boss beats with a choose-one boss reward (one of them a single resurrection);
- a named-rule difficulty ladder;
- map "places" that change the player's task;
- risk/reward runes and curses;
- an announcer-style streak layer;
- persistent gear with high-risk uniques;
- capped convenience meta, mastery marks and traits;
- slot-based cosmetics kept out of the seed.

Each can be renamed and re-themed without changing its mechanics or tuning methodology.

### Cited findings (Dota structure → generic function → re-skin candidate)

The re-skin candidates are illustrative suggestions, not repo facts.

#### Build and boss systems
- **Item "schools" (Radiance burn, Skadi cold, Maelstrom chain lightning, Beast summons, Venom poison stacks) → a god or pantheon.**
  - Structure: ≤3 per run; 68 cards across 7 types; 23 legendaries; 10 pair hybrids; Dash/Cast/Summon types that hook into hero mobility, abilities and pets.
  - Source: [schools.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/schools.ts), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md).
  - The brief maps these one-to-one to DMD gods (Summer = burn, Winter = cold, Leigong = lightning, Mort = summons, Moirai = luck, Justice = lifesteal, Time = CDR/pickup) ([brief §1–2](/Users/Shared/Coding/aegis-draft/docs/arcade-survivors-brief.md)).
  - Re-skin: an original pantheon of 5–6 patron deities, each owning one status.
- **Roshan at 7:00/14:00 with a choose-one reward → a recurring boss with a resurrection relic.**
  - Rewards: Aegis = one resurrection; Cheese = auto-heal at ≤25% HP; Refresher = reset cooldowns; Shard = spare talents.
  - The second boss is stronger (×1.4 HP, ×1.25 damage) and gains Wave of Force plus Bash. Roshan's control is capped at 1.2 s with immunity afterwards; the River-act boss is leashed to its pit and regenerates if the player leaves.
  - Sources: [roshan.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/roshan.ts), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md), [BACKLOG T13.9](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md).
  - Re-skin: an act guardian dropping a "second-life" relic. The resurrection relic burns when used and does not stack, as in the M23 Aegis rule.
- **Tormentor (stationary, reflects 25% of damage) → a reflective mid-boss and optional challenge. The Ancient at 20:00 under mega-creeps → a final siege objective** ("survive and destroy the throne") ([BACKLOG T13.9](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [brief §2](/Users/Shared/Coding/aegis-draft/docs/arcade-survivors-brief.md)).

#### Difficulty
- **Rank ladder Herald→Immortal × 5★ → named difficulty tiers.**
  - Each tier adds one named rule (doubleGolems, bigWaves, trollPacks, siegeOften, earlyRoshan, resistStatus, lessXp; gentleStart on low tiers).
  - Concave multipliers reach ×3.6 HP / ×3.3 damage at the top.
  - Win rates come from career calibration.
  - Sources: [ranks.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/ranks.ts), [BACKLOG T26.7](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md).
  - Re-skin: "Circles", "Oaths" or "Heresies" with a named rule per tier. The brief notes DMD's own ladder runs to 40 ([brief §1](/Users/Shared/Coding/aegis-draft/docs/arcade-survivors-brief.md)).

#### Hero progression
- **Talents and Aghanim's upgrades → a per-hero talent fork plus "relic" alternates.**
  - At levels 10/15/20 the player picks a kit talent (one number of one ability) or a generic talent (+20 damage / +150 HP / +6 armor). At level 25 both ult talents are given.
  - Shard grants spare Q/W/E talents; Scepter grants both ult talents (shop from 6:00, absorbed).
  - Sources: [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md), [talents.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/talents.ts).
  - Calibration note: kit talents at +25% lost to school cards, so they were raised to +60% ([BACKLOG calibration M22](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).
- **Signature passives (souls stacking, fury swipes, great cleave, time lock…) → one identity mechanic per hero** (121 of 126 heroes have one) ([heroes.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/heroes.ts), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md)).
- **Blink dash plus Dash blessings → a universal dodge** whose blessings trigger elemental effects at departure or arrival, with three legendary dash upgrades ([PRD §5.15 T16](/Users/Shared/Coding/aegis-draft/docs/PRD.md)).

#### Runes, events and encounters
- **Runes → shrines and pickups.**
  - Bounty runes give gold every 3 min.
  - "Greed" runes trigger 60 s of double spawns and double XP for a permanent +8% enemy power per stack (the DMD altar analogue).
  - Power runes appear at the river; wisdom runes give 60% of a level every 7 min.
  - Sources: [BACKLOG T13.7](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md).
- **Map places → biome events that change the task** ([PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md), [audit 2026-09-08](/Users/Shared/Coding/aegis-draft/docs/audits/2026-09-08-arcade-content-and-progression.md)):
  - corrupted camp (destroy three totems; the champion is shielded while they stand);
  - outpost (hold 25 s; it pauses but does not reset; reward: vision and markers);
  - lotus pond (one use: heal or cleanse);
  - cursed chest with curses (Wither, Shopkeeper's Debt, Blood Hunt);
  - forge (temper, reforge, sacrifice);
  - rift (pick 1 of 2 rules, survive 50 s with the act clock paused, reward an arcana card);
  - caravan escort (no HP bar, to avoid punishing poor escort AI);
  - hunt contract (two targets with declared rewards; "Hunter's Oath" twist);
  - three seeded compositions with an "act property".
- **Champions as "change the player's task" mini-bosses.** Each has 1–2 telegraphed patterns, a response window and a retreat option ([audit 2026-09-08](/Users/Shared/Coding/aegis-draft/docs/audits/2026-09-08-arcade-content-and-progression.md), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md)):

  | Champion | Pattern | Counterplay |
  |---|---|---|
  | Centaur | telegraphed charge | lure it into a rock, which stuns it |
  | Necromancer | raises skeletons while bone idols stand | destroy the idols |
  | Thunder golem | three charged zones | one safe side always remains |
  | River warden | phase shields | waves have an island gap |
  | Dire stalker | ambush mark with sound | punish the window after it jumps |
  | Rival heroes | hook, taunt, nuke | they leave after 90 s so the fight never overlaps the boss |

- **Elite affixes, killing streaks and multi-kills → an announcer/combo layer.**
  - From 3:00, every 45 s, an elite arrives with 1–2 affixes (Haste, Vampiric, Volatile, Frost, Splitter).
  - Streaks of kills without taking damage give +10% gold per step.
  - Multi-kills within 2 s give tiered rewards (an extra blink charge; −1 s on all cooldowns).
  - Source: [PRD §5.15 T17, T21](/Users/Shared/Coding/aegis-draft/docs/PRD.md).
  - Re-skin: an original announcer voice and original tier names.
- **Neutral camps with telegraphed casts → regular enemies with readable skills.** At most one cast active at a time, only on screen, and stun interrupts. Spell Steal lets a hero copy them ([PRD §5.15 T20, T24](/Users/Shared/Coding/aegis-draft/docs/PRD.md), [BACKLOG calibration M20](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md)).

#### Economy and meta
- **Economy:**
  - shop windows at fixed minutes (3:00/6:00/11:00/16:30);
  - paid rarity upgrades;
  - selling at half price;
  - neutral-item tokens by minute in 5 tiers (pick 1 of 2; one slot);
  - persistent gear (6 slots, 1–4 affixes by rarity, salvage);
  - six boss uniques with Dota-like properties (illusions, damage aura, reflect, out-of-combat regen).

  Sources: [items.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/items.ts), [neutrals.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/neutrals.ts), [gear.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/gear.ts), [PRD §5.15](/Users/Shared/Coding/aegis-draft/docs/PRD.md).
- **Meta:**
  - Legacy, 5 convenience branches × 4: pickup radius, speed, shop prices, rerolls, starting gold; deliberately no damage/HP;
  - mastery: 13 mark IDs → titles novice/veteran/master/legend;
  - traits: Berserk/Bulwark for 1 mark, Swift/Scavenger for 2, each with a plus and a minus;
  - expeditions: 5 chains × 3 steps with different heroes, rewarding titles;
  - bestiary and codex.

  Sources: [legacy.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/legacy.ts), [arcadeStore.ts MARK_IDS](/Users/Shared/Coding/aegis-draft/web/src/state/arcadeStore.ts), [traits.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/traits.ts), [expeditions.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/expeditions.ts).
- **Cosmetics kept out of the seed.**
  - Structure: base/arcana/style "families" with slot-based parts; summon and form skins; ground effects (frame, aura, trail, death, tint); gem recolors on accent pixels.
  - Shard prices exist (`SHARD_PRICE_FULL` 20/50/120/320 per rarity) but are set to 0. They could become a premium or earned layer in a commercial game.
  - Sources: [cosmetics.ts](/Users/Shared/Coding/aegis-draft/web/src/game/arcade/content/cosmetics.ts), [BACKLOG T26.8](/Users/Shared/Coding/aegis-draft/docs/BACKLOG.md), [MEM/arcade-loot-build-effects.md](/Users/artemchikolaev/.claude/projects/-Users-Shared-Coding-aegis-draft/memory/arcade-loot-build-effects.md).

### Inferences
- **Highest-value structures to keep** are those already measured with the bot or with career simulation: the ladder curve, blessing slots, Legacy pace, neutral-cast frequency, summon DPS, rival pacing. Their tuning methodology transfers even though the numbers will need re-measuring under new content.
- **Re-tune for a smaller, deeper roster.** DMD shipped about 6 heroes per the brief, while aegis-draft has 126 wide-but-shallow kits whose per-hero balancing consumed much of the calibration effort. A commercial original-IP roguelite is better served by fewer heroes with deeper kits and a strong pantheon layer (judgment, not a repo fact).
- **Rename everything Dota-flavored before reuse,** including mechanic labels that are Dota idioms: "Aegis", "buyback", "bounty rune", "Secret Shop", "Aghanim's", "Divine Rapier", rank medal names, "Radiant/Dire".
- **DMD likeness is a separate question.** The seven blessing types and "max 3 gods" are DMD's own structure ([brief §1](/Users/Shared/Coding/aegis-draft/docs/arcade-survivors-brief.md)). Mechanics are generally not protected the way art and names are, but the reputational distance from DMD is a design choice.

### Gaps
- No quantitative evidence exists on which structures players enjoy most. All evidence is bot metrics plus owner feedback.
- Dead Cells-specific structures (melee-first combat, biome branching, cell-currency meta) were not part of this prototype beyond the cursed-chest and cursed-biome principle and the pixel-render approach, so this repo offers little evidence for them.
