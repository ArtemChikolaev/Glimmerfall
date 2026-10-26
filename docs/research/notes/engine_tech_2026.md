# Engine & technology stack for a "maximally native" 2D pixel-art action roguelite (state as of 2026-10-25)

*Scope: PC/Steam first (Win/macOS/Linux/Steam Deck), consoles later (Switch 2, PS5, Xbox), gamepad-first, 1,000–5,000 entities + projectiles at 60 fps, pixel art pre-rendered from 3D, solo dev + AI coding agents, team knows TS/React, Go, Python, Blender scripting. Method: WebSearch + WebFetch of primary pages (official docs/release notes/blogs, Wikipedia infoboxes, vendor pages). The session's WebSearch budget ran out near the end; the last verifications were done by fetching known primary URLs directly. PCGamingWiki and SteamDB returned HTTP 403, so engine verification relies on Wikipedia infoboxes, developer/vendor statements and modding evidence. Dates are given wherever the source states them. "Inferences" are my judgement, not sourced facts.*

## Q1. Current state (Oct 2026) of the candidate engines/stacks

### Takeaway
Godot 4.7.2 (18 Aug 2026) is the current stable Godot. C# works for desktop but still can't export to web, and on consoles it is only "beta" through W4. Unity 6.3 LTS is the safe Unity line: ECS becomes a core package from 6.4, and console builds need Pro at $2,310/seat/yr. GameMaker is in the middle of a runtime transition (GMRT; JS/TS/C# promised for 2026). Defold 1.14 (12 Oct 2026) gives free console access, now including Xbox. Bevy 0.19 still has no editor and no console story. MonoGame 3.8.5 rebuilt its native backend. LÖVE is still 11.5. Phaser 4 shipped, but JS stacks still have no practical console path.

### Cited Findings

**Godot 4.x**
- Godot 4.6.0 stable shipped around 26 Jan 2026, followed by 4.6.1 (16 Feb 2026), 4.6.2 (1 Apr 2026) and a 4.6.3 RC (about May 2026) — [GameFromScratch: Godot 4.6 Released](https://gamefromscratch.com/godot-4-6-released/); [APKMirror 4.6.0 listing](https://www.apkmirror.com/?p=12268842); [Official 4.6.2 announcement](https://godotengine.org/article/maintenance-release-godot-4-6-2/); [Godot forum: 4.6.3 RC 1](https://forum.godotengine.org/t/release-candidate-godot-4-6-3-rc-1/138406)
- Godot 4.7 ("Lights, Camera, Action!") appears on the official blog dated 18 June 2026, then 4.7.1 (14 July) and a 4.7.2 RC (3 Aug) — [Godot blog index](https://godotengine.org/blog/index.html). A secondary site gives 19 June instead, probably a time-zone difference — [cinevva](https://app.cinevva.com/hi/news/2026-06-19-godot-4-7-released). Wikipedia's infobox lists **4.7.2, 18 August 2026** as the latest stable — [Wikipedia: Godot](https://en.wikipedia.org/wiki/Godot_(game_engine)). 4.7.2 carried 57 fixes from 39 contributors — [OpenSourceForU](https://www.opensourceforu.com/2026/08/godot-4-7-2-released/)
- 4.7 headline features: HDR output (Windows/macOS/iOS/visionOS/Linux-Wayland), AreaLight3D, `Control` offset transforms, a new Asset Store replacing the Asset Library, a stable Android build environment (GABE), and `DrawableTexture2D`. The 2D changes are minor: a one-way collision direction on `CollisionShape2D`, `FILL_CONIC` gradients, and a 2D "Scene Paint" editor mode. The new nearest-neighbor scaling option applies only to 3D rendering — [Godot 4.7 release page](https://godotengine.org/releases/4.7/)
- Godot 4.4 (4.4.1 builds dated 26 Mar 2025):
  - .NET 8 is the minimum for C#.
  - The UID workflow is now complete: `.uid` sidecar files make projects "more resistant to changes in your filesystem organization."
  - Jolt physics is built in but labeled experimental.
  - **2D batching was extended to the Forward+/Mobile renderers.**
  - Typed dictionaries and an embedded game window were added.
  - Source: [Godot 4.4 release page](https://godotengine.org/releases/4.4/)
- Godot 4.3 (15 Aug 2024) added physics interpolation that decouples tick rate from frame rate, including in 2D. It also added `TileMapLayer`, `Parallax2D`, and fixes for pixel-art snapping gaps — [Godot 4.3 release page](https://godotengine.org/releases/4.3/)
- Pixel-art guidance in the official docs (4.7 docs):
  - Use the `viewport` stretch mode.
  - Use the `integer` stretch scale mode, available "Since Godot 4.2".
  - A 640×360 base resolution scales cleanly to 720p, 1080p, 1440p and 4K.
  - Prefer Exclusive Fullscreen so integer scaling doesn't drop a factor.
  - Source: [Godot docs: Multiple resolutions](https://docs.godotengine.org/en/stable/tutorials/rendering/multiple_resolutions.html)
- C# status, per the 4.7 docs:
  - Godot 4.5+ needs .NET 8+, and Android export needs .NET 9+.
  - Desktop export (Linux/Windows/macOS) works. Android/iOS are experimental (since 4.2).
  - "Projects written in C# using Godot 4 currently cannot be exported to the web platform."
  - Source: [Godot docs: C# basics](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html)
- A C# web prototype was demoed at GodotCon Boston (May 2025). It statically linked Mono, produced a large payload, and came with no committed timeline — [cinevva guide (secondary)](https://app.cinevva.com/guides/godot-vs-unity-web-games); [raulsntos/godot4-web-dotnet-prototype](https://github.com/raulsntos/godot4-web-dotnet-prototype). A community libgodot-based NuGet package for C# web export exists (unofficial) — [Godot forum: 2dog](https://forum.godotengine.org/t/2dog-using-libgodot-for-c-web-export-unit-testing-and-more/142750)
- Godot 4.0 moved C# from Mono to .NET (6) CoreCLR. Wikipedia says consoles are "not officially supported" because proprietary SDKs conflict with the open-source license — [Wikipedia: Godot](https://en.wikipedia.org/wiki/Godot_(game_engine))
- The official console page (4.5 docs) gives three reasons consoles are not official: "risks of legal liability", disproportionate cost, and open-source licensing. Steam Deck is the only officially supported "console", through the Linux export.
  - Listed porters and the consoles they cover:
    - Lone Wolf Technology: Switch, PS4
    - **Pineapple Works**: Switch 1 & 2, Xbox One/Series, PS5. It is the only one that explicitly lists **GDScript and C#**.
    - RAWRLAB: Switch
    - mazette!: Switch, Xbox
    - Olde Sküül: Switch, XB1, PS4, PS5
    - Tuanisapps: Switch
    - Seaven Studio: Switch, Xbox, PS4/5
    - Sickhead Games: Switch, PS4/5, Xbox
  - W4 Games is listed as middleware.
  - Requirements: a legal entity, NDAs, a months-long acceptance process, devkits at confidential prices, and age ratings.
  - Source: [Godot docs 4.5: Console support](https://docs.godotengine.org/en/4.5/tutorials/platform/consoles.html)
- **W4 Consoles** (page fetched Oct 2026):
  - **Platforms:** Switch 1, Xbox Series and PS5 are available. **Switch 2 is "early beta"** and needs its own license. PS4/Xbox One are "Not at this time."
  - **Pricing:**
    - Starter: $800/yr per platform or $2,000/yr for all platforms. Eligibility: company under $300k revenue/funds and at most 30 people.
    - Pro: $4,000/yr per platform or $10,000/yr for all.
    - Enterprise: quote.
    - Premium support: $7,000/yr.
    - No revenue share.
    - **Porting houses are allowed only on Enterprise.**
  - **C# is "in beta" on Switch and Xbox Series.** Other platforms: contact W4.
  - Godot versions covered: 4.4–4.6 on Switch 1/2, PS5 and Xbox.
  - Source: [W4 Consoles](https://www.w4games.com/w4consoles). The original 2023 pricing is in [W4 Games, Dec 2023](https://w4games.com/2023/12/11/w4-games-announces-pricing-model-for-console-ports/)
- RAWRLAB released a free MIT-licensed Switch port in January 2024. It has basic functionality only, no C#/GDExtension, and no support — [GamingOnLinux](https://gamingonlinux.com/2024/01/godot-engine-gets-a-free-nintendo-switch-port-for-game-devs)
- GodotSteam 4.22 / 4.11 targets Godot 4.7.2 (22 Aug 2026). The module and GDExtension code were merged — [GodotSteam blog](https://godotsteam.com/blog/2026/08/22/godot-472-and-godotsteam-422--411/)
- godot-rust: the `godot` crate reached 0.4.5 on 12 Dec 2025. v0.5 supports Godot 4.6 out of the box (and all versions ≥4.2) plus Rust edition 2024. crates.io releases lag the master branch — [lib.rs: godot](https://lib.rs/crates/godot); [docs.rs godot 0.5.5](https://docs.rs/godot/0.5.5)
- Mega Crit (Slay the Spire 2) became a Corporate Platinum sponsor of Godot in April 2026, roughly €36k/yr — [GamingOnLinux](https://gamingonlinux.com/2026/04/godot-gets-a-funding-boost-from-slay-the-spire-2-devs-mega-crit)

**Unity 6.x**
- Pricing:
  - Personal is limited to under $200k in revenue plus funding.
  - **Pro costs $2,310/seat/yr or $210/mo**, after a 5% increase effective 12 Jan 2026.
  - Enterprise applies above $25M revenue.
  - Havok is no longer bundled from 6.3.
  - Sources: [Unity pricing updates](https://unity.com/products/pricing-updates); [CG Channel, Nov 2025](https://www.cgchannel.com/?p=171673)
- The Runtime Fee was cancelled in Sept 2024 — [Unity: runtime fee page](https://unity.com/runtime-fee); [Notebookcheck](https://www.notebookcheck.net/Unity-cancels-Runtime-Fee-before-controversial-pricing-structure-even-goes-into-effect-in-Unity-6.887828.0.html)
- **Console builds need an active Unity Pro subscription or a platform holder's "Preferred Platform license key"**, plus approval from each platform holder — [Unity Personal page](https://unity.com/products/unity-personal). Sony and Nintendo offered these keys and Microsoft did not, as of 2021 — [Game Developer, 2021](https://www.gamedeveloper.com/programming/going-forward-unity-devs-will-need-unity-pro-to-publish-on-consoles)
- Unity 6.3 LTS:
  - The 2D Renderer can draw Mesh/Skinned Mesh Renderers alongside sprites (2D URP).
  - A new low-level 2D physics API (`UnityEngine.LowLevelPhysics2D`) is built on **Box2D v3**.
  - A Sprite Atlas Analyser was added.
  - The LTS gets about 2 years of support.
  - Sources: [Unity docs: New in 6.3 LTS](https://docs.unity.com/en-us/engine/6000.6/manual/whats-new/unity63); [GameFromScratch](https://gamefromscratch.com/unity-6-3-released/)
- ECS ("ECS for All") progress:
  - From 6.4, Entities, Collections, Mathematics and Entities Graphics ship as **core packages**, and `EntityId` replaces InstanceID.
  - 6.6 completed the 8-byte EntityId migration and is deprecating Managed Components.
  - CoreCLR for the full Editor is targeted at 6.8.
  - A 6.7 alpha shipped in July 2026.
  - Sources: [ECS status Dec 2025](https://discussions.unity.com/t/ecs-development-status-december-2025/1699284/1); [Unity 6.4 ECS docs](https://docs.unity3d.com/6000.4/Documentation/Manual/ECSFeature.html); [CoreCLR/ECS update Mar 2026](https://discussions.unity.com/t/coreclr-scripting-and-ecs-status-update-march-2026/1711852); [ECS status for 6.6](https://discussions.unity.com/t/ecs-development-status-for-unity-6-6/1724480)
- The official **Unity MCP server** entered open beta on 11 May 2026.
  - What it does: agents can inspect scenes, read the console, edit scripts and trigger Editor actions.
  - Requirements: Unity 6+, the AI Assistant package, a project connected to Unity Cloud, and an active trial or subscription.
  - Unity says features and availability are "in flux".
  - Sources: [Unity blog](https://unity.com/blog/unity-ai-mcp-how-to-get-started); [Unity MCP docs](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-overview.html)

**GameMaker**
- The current release is LTS 2026.0 (21 May 2026), which added Switch 2. PS5 and Xbox Series need an "Enterprise" licence — [Wikipedia: GameMaker](https://en.wikipedia.org/wiki/GameMaker)
- Pricing:
  - Non-commercial use is free, with no forced splash screen.
  - Professional is a one-time $99.99.
  - **Console export requires the Enterprise subscription** — [GameMaker FAQ](https://gamemaker.io/help/articles/november-2023-pricing-terms-change-faq)
  - Enterprise was reported at $79.99/mo (or $799.99/yr) when announced in Nov 2023. I did not find a 2026 confirmation — [Toolradar](https://toolradar.com/tools/gamemaker/pricing); [GameWorldObserver](https://gameworldobserver.com/?p=21945)
- **GMRT (new runtime)** launched on desktop on 30 April 2026, together with **GM-CLI** (open source, Apache, on npm as `@gamemaker/gm-cli`).
  - "Project files are plain text that track well in Git".
  - Opera's press release names **Claude Code** integration in the CLI.
  - Language roadmap: JavaScript (Q2 2026), TypeScript (Q3), and a C# preview (Q4 2026).
  - Desktop/mobile/web GMRT source is available to all users. Console source is for Enterprise users only.
  - Mobile and console GMRT are due "through the rest of the year". v0.20 lists a Switch target.
  - The legacy GMS2 runtime is "feature complete" and supported in LTS26 until at least Q1 2028.
  - Sources: [GameMaker Spring 2026 update](https://gamemaker.io/blog/update-spring-2026); [Opera press release, 30 Apr 2026](https://press.opera.com/?p=3924)

**Defold**
- Latest is **1.14.0 (12 Oct 2026)**.
  - Scripting is Lua, with native extensions in C/C++.
  - Consoles: Switch (needs Nintendo approval), PS4 and PS5 (need Sony approval).
  - The Defold License is source-available, derived from Apache 2.0, and royalty-free.
  - The Defold Foundation has run the engine since 2020.
  - Source: [Wikipedia: Defold](https://en.wikipedia.org/wiki/Defold)
- 1.14.0 added **Xbox support, free for approved Xbox developers** — [Digital Citizen](https://www.digitalcitizen.life/defold-1-14-0-adds-free-xbox-support-for-approved-developers/); [Windows Central](https://www.windowscentral.com/gaming/xbox/xbox-just-gained-support-for-defold-game-engine)
- Console builds are free for anyone the platform holder approves, and console-specific code is not public — [Defold blog 2023](https://defold.com/2023/10/01/How-Defold-works-with-consoles/). Access to console source code used to cost $200/mo; that fee was later dropped — [wnhub](https://wnhub.io/news/engines/item-42605)

**Bevy (Rust)**
- **0.19 released 19 June 2026** (261 contributors, 1,185 PRs), and 0.19.1 followed on 13 Aug 2026. 0.18.1 came out on 2 Mar 2026 — [Bevy 0.19 news](https://bevy.org/news/bevy-0-19/); [docs.rs bevy_ui 0.19.1](https://docs.rs/crate/bevy_ui/0.19.1); [PrismNews](https://www.prismnews.com/hobbies/rust-programming/bevy-releases-v0181-to-stabilize-018-fix-regressions)
- Per the 0.19 post:
  - There is **no official editor yet** (an entity inspector has been prototyped).
  - Scenes use the BSN `bsn!` macro, and "we aren't yet shipping a first-party `.bsn` asset loader".
  - 2D rendering "has started to lag behind" 3D.
  - No consoles are mentioned.
  - There were big breaking changes (the render graph was replaced by ECS schedules), covered by a migration guide.
  - Source: [Bevy 0.19 news](https://bevy.org/news/bevy-0-19/)

**Heaps / HashLink (Haxe)**
- Shiro Games' stack (6 Apr 2020, Nicolas Cannasse) is Haxe, Heaps, HashLink, HL/C, HIDE and CastleDB. HL/C ("HashLink compiled to C") is what they use "for our console ports on PlayStation, Xbox and Nintendo Switch" — [haxe.org: Shiro Games Technology Stack](https://haxe.org/blog/shirogames-stack/)
- Northgard runs on Switch/PS4/Xbox One — [HashLink site](https://hashlink.haxe.org/). The Heaps README lists consoles for registered developers, and console access is by contacting Nicolas — [HeapsIO/heaps](https://github.com/HeapsIO/heaps)

**MonoGame / FNA (C#, XNA lineage)**
- **MonoGame 3.8.5 (15 Jul 2026)** moved to a single native C/C++ backend.
  - New targets: DesktopVK (Vulkan/SDL2/FAudio) and WindowsDX12. WindowsDX12 was "created as a bounty for Xbox One and Xbox Series support".
  - The team is "transitioning our console implementations to this new native backend".
  - Content building is now code-centric.
  - Planned next: 3.8.6 (hardening), then 3.9 (fonts, unified shader compiler, native mobile).
  - Source: [MonoGame blog](https://monogame.net/blog/2026-07-15-3.8.5-release-2026/)
- Point release 3.8.5.1 shipped on 14 Aug 2026. Wikipedia lists PS4/5, Xbox One/Series and Switch/Switch 2 among its platforms — [Wikipedia: MonoGame](https://en.wikipedia.org/wiki/MonoGame). 3.9 is planned as the LTS of 3.x — [MonoGame roadmap](https://docs.monogame.net/roadmap/)
- Sickhead Games moved its focus to MonoGame, which it helped steward. Its ports include Darkest Dungeon, Stardew Valley, Celeste and Slay the Spire — [Wikipedia: Sickhead Games](https://en.wikipedia.org/wiki/Sickhead_Games)
- FNA reimplements XNA 4.0 Refresh. Its platform focus is "exclusively on fully open platforms" (desktop) — [FNA (SourceForge mirror)](https://www.sourceforge.net/mirror/fna). Ethan Lee stopped future macOS ports in 2021 — [GamingOnLinux](https://www.gamingonlinux.com/2021/01/fna-dev-and-porter-ethan-lee-stops-future-macos-ports-linux-to-be-their-focus/comment_id=196203)

**LÖVE (Lua)**
- The stable version is still **11.5 (3 Dec 2023)**, with 12.0 "In development". It is zlib-licensed and runs on Windows/macOS/Linux/BSD/iOS/Android. Wikipedia mentions no consoles. Balatro is listed as a commercial hit "without a mainstream engine" — [Wikipedia: LÖVE](https://en.wikipedia.org/wiki/L%C3%96VE_(game_framework))
- Balatro shipped on Switch, PS4/PS5, Xbox One/Series, and Switch 2 (25 Feb 2026) — [Wikipedia: Balatro](https://en.wikipedia.org/wiki/Balatro). One search snippet placed the Switch 2 edition in Feb 2025, which is impossible because the console launched in June 2025, so it is disregarded.

**Web stack (Phaser 4 / PixiJS + Tauri / Electron)**
- **Phaser 4 launched in April 2026** (GameFromScratch coverage dated 14 Apr 2026).
  - It has a new RenderNode architecture and recovers automatically from WebGL context loss.
  - A vendor/reviewer claim: a "Sprite GPU Layer" can render over a million sprites.
  - v4.2.1 shipped on 9 Jul 2026.
  - Sources: [Phaser: Phaser 4 renderer](https://phaser.io/news/2026/04/phaser-4-renderer-faster-cleaner-and-built-for-modern-games); [Phaser: GFS review](https://phaser.io/news/2026/04/gamefromscratch-reviews-phaser-4-the-biggest-release-ever); [Phaser v4.0.0 discussion](https://github.com/phaserjs/phaser/discussions/7274)
- Tauri has **no built-in Steamworks**; you need a Rust plugin or FFI — [GDevelop forum](https://forum.gdevelop.io/t/implementing-steam-achievements-with-tauri/73202)
- Electron can use steamworks.js (achievements, cloud, workshop, plus an overlay helper), but the overlay is reported as unreliable — [GDevelop forum](https://forum.gdevelop.io/t/steam-overlay-and-leaderboards/60725). Construct removed its overlay workaround in 2024 because of bugs, then later re-added it on Windows — [Construct Steamworks addon](https://www.construct.net/fr/make-games/addons/1105/steamworks/versions). A 2022 report says Electron apps on SteamOS needed `--no-sandbox` — [Steam community](https://steamcommunity.com/app/1675200/discussions/1/4826187053916237100)
- **Vampire Survivors moved from Phaser to Unity on 17 Aug 2023 (v1.6).** The move was framed as bringing better cross-platform support and performance. Phaser's creator said "you just can't get JS on consoles easily" — [GamingOnLinux](https://gamingonlinux.com/2023/07/vampire-survivors-switching-to-new-game-engine-on-august-17); [Phaser devlog July 2023](https://phaser.io/devlogs/279); [Wikipedia: Vampire Survivors](https://en.wikipedia.org/wiki/Vampire_Survivors)
- **CrossCode** (JS/HTML5):
  - The Nintendo Web Framework was discontinued for Switch.
  - Deck13 built an interpreter that "locks [the code] up in a cage", which made performance a major challenge.
  - Live-compiling in Chromium raised security problems on consoles.
  - The Switch version slipped to 2020.
  - Sources: [RPG Site](https://www.rpgsite.net/news/9249-deck13-and-radical-fish-games-provide-an-update-on-the-console-versions-of-crosscode); [Nintendo Everything](https://nintendoeverything.com/crosscode-delayed-to-2020-on-switch-porting-studio-shares-status-update/); [Steam discussion (Deck13 dev)](https://steamcommunity.com/app/368340/discussions/0/1741106440027709150)

### Inferences
- Godot ships about two minor releases a year (4.6 in Jan 2026, 4.7 in Jun 2026). Expect 4.8 around early 2027. W4 lags by a few months: its page still lists 4.4–4.6, so console builds should pin an engine minor that W4 or the porter supports.
- For a native-first game, C# in Godot is production-proven on desktop (Slay the Spire 2 ships on it; see Q2). The weak points are consoles (W4 C# "beta"; Pineapple Works lists C#) and the missing web export, which only matters for browser demos.
- Unity has the most mature console pipeline and the largest body of genre references. The costs are recurring per-seat fees, an editor-centric workflow, and an ECS layer that is still being restructured (6.4–6.8). The safe baseline is 6.3 LTS with plain C#. Use Entities only if it is really needed.
- GameMaker is mid-migration (GMRT, new languages). Starting a 2–3 year console-bound action game on GMRT in late 2026 means betting on a runtime that leaves beta during the project. The legacy runtime is stable but frozen.
- Defold is now the cheapest console route (free on Switch/PS4/PS5/Xbox for approved developers). Its limits are Lua and few precedents in this genre.
- Bevy (no editor, no consoles, quarterly breaking changes) and LÖVE/web stacks (no official console path) do not meet the "maximally native, consoles later" bar for a commercial project.

### Gaps
- I could not find whether Godot 4.7.3 or a 4.8 beta existed as of 25 Oct 2026; the search budget ran out.
- W4's commercial Switch 2 release date and C# status on PS5 are not public ("contact W4").
- I did not confirm whether Unity 6.7 becomes the next LTS, or its date.
- Not checked: the current PixiJS 8.x version, the Tauri 2.x version, and steamworks.js maintenance status.
- Not re-verified this session: details of Unity's 2D toolset (Pixel Perfect Camera, 2D lights/normal maps in URP) and Godot's 2D normal-map lighting (CanvasTexture). Both are believed to exist.
- No 2025–2026 source on HashLink console maintenance.

## Q2. Verified engine list for notable games

### Takeaway
Unity dominates the console-heavy action titles in the list (Silksong, Cult of the Lamb, Enter the Gungeon, Blasphemous, Rogue Legacy 2, Vampire Survivors, Ball x Pit, Megabonk, Children of Morta, and probably Death Must Die). Godot has the rising horde/roguelite hits (Brotato, Halls of Torment, Dome Keeper, Cassette Beasts, Slay the Spire 2 in C#). Custom or niche stacks (Heaps, LÖVE, Supergiant's C++ engine, Noita's Falling Everything, XNA) worked for strong engineers but needed bespoke console work. Windblown, the closest reference for "Dead Cells-like action roguelite", uses **Unity for the view layer and Photon Quantum for a deterministic simulation**.

### Cited Findings
| Game | Engine / stack | Verification and notes |
|---|---|---|
| Dead Cells | Haxe + Heaps + HashLink (Motion Twin / Evil Empire) | Haxe-community sources: [haxe.org: Nicolas about Haxe #2 – Heaps](https://haxe.org/blog/nicolas-about-haxe-episode-2/) and [ACCU 2019 slides](https://accu.org/conf-docs/PDFs_2019/george_corney_-_haxe_an_understated_powerhouse.pdf) (Dead Cells made with Heaps; Switch/PS4/XB1). Wikipedia names no engine; it lists consoles (2018; PS5 in 2023) and Evil Empire's role — [Wikipedia](https://en.wikipedia.org/wiki/Dead_Cells) |
| Windblown | **Unity (view layer) + Photon Quantum (deterministic sim, rollback, replays)** | Photon case study, 26 Jun 2025: "Unity is used purely for the view layer", Burst used heavily — [Photon blog](https://blog.photonengine.com/?p=6487). **Conflict:** a secondary May 2026 article calls it a proprietary successor of the Dead Cells engine — [foro3d](https://foro3d.com/en/2026/mayo/windblown-la-evolucion-tecnica-del-motor-de-dead-cells-al-caos-isometr.html). I favour Photon's direct statement. Early Access since 24 Oct 2024; 1.0 expected in 2026 — [Wikipedia](https://en.wikipedia.org/wiki/Windblown) |
| Death Must Die | Unity (weak evidence) | Steam users report "unity engine" crashes — [Steam discussion](https://steamcommunity.com/app/2334730/discussions/0/4034725629424917234/?l=german). Developer and publisher Realm Archive; Early Access 14 Nov 2023; Windows/macOS — [Steam store](https://store.steampowered.com/app/2334730/). SteamDB/PCGamingWiki blocked (403) |
| Halls of Torment | Godot | [Wikipedia](https://en.wikipedia.org/wiki/Halls_of_Torment). Heavy code moved to C++ and threads after a "performance wall" — [Fullcleared interview](https://fullcleared.com/features/inside-halls-of-torment-an-interview-with-chasing-carrots/). PS5/Xbox Series on 28 Oct 2025 — [Gematsu](https://www.gematsu.com/2025/09/halls-of-torment-for-ps5-and-xbox-series-launches-october-28-alongside-dlc-the-boglands-and-free-update). Over 1M sales — [GamingOnLinux](https://gamingonlinux.com/2024/10/halls-of-torment-hits-1-million-sales-more-free-updates-planned) |
| Vampire Survivors | Phaser (JS) → Unity from v1.6 (17 Aug 2023) | [Wikipedia](https://en.wikipedia.org/wiki/Vampire_Survivors); [GamingOnLinux](https://gamingonlinux.com/2023/07/vampire-survivors-switching-to-new-game-engine-on-august-17) |
| Brotato | Godot 3 (Blobfish) | Consoles ported by Seaven Studio — [Seaven Studio](https://www.seaven-studio.com); [Godot forums](https://godotforums.org/d/38787-brotato-releases-to-xbox-one-xbox-series-ps4-and-ps5); [Wikipedia: Godot](https://en.wikipedia.org/wiki/Godot_(game_engine)) |
| Balatro | LÖVE (Lua) | [Wikipedia: LÖVE](https://en.wikipedia.org/wiki/L%C3%96VE_(game_framework)); [Wikipedia: Balatro](https://en.wikipedia.org/wiki/Balatro) |
| Slay the Spire 2 | **Godot + C# (.NET)**, migrated from about 2 years of Unity work | [Wikipedia](https://en.wikipedia.org/wiki/Slay_the_Spire_II); [Game Developer](https://gamedeveloper.com/business/slay-the-spire-devs-followed-through-on-abandoning-unity). C# confirmed through mod templates that target net9.0 and reference `sts2.dll` and `GodotSharp.dll` — [NuGet template](https://www.nuget.org/packages/s2smodtemplate.templates); [Modding tutorial](https://github.com/fresh-milkshake/Modding-Tutorial). Lead programmer Jake Card said Godot was better than Unity in almost every respect for this project (paraphrased from a Japanese article); his one regret was the smaller community — [Game*Spark (JP)](https://www.gamespark.jp/article/2026/03/10/163700.html) |
| Hades | Custom engine: C#/XNA, rewritten in C++ "based on the framework The Forge" | [Wikipedia](https://en.wikipedia.org/wiki/Hades_(video_game)) |
| Hades II | In-house engine (Supergiant C++ lineage) | Wikipedia names no engine but lists Lua scripting; 1.0 on 25 Sep 2025; PS5/Xbox Series on 14 Apr 2026 — [Wikipedia](https://en.wikipedia.org/wiki/Hades_II). An engineer hired in 2020 to move the custom C# engine to C++ later worked on Hades II's in-house engine — [DigiPen](https://www.digipen.edu/showcase/news/hades-2-graphic-programmer) |
| Noita | Custom "Falling Everything" engine | [Wikipedia](https://en.wikipedia.org/wiki/Noita_(video_game)) |
| Enter the Gungeon | Unity | [Wikipedia](https://en.wikipedia.org/wiki/Enter_the_Gungeon) |
| Children of Morta | Unity (with custom editor tools) | [Unity case study](https://activation.unity3d.com/case-study/children-of-morta) |
| Blasphemous | Unity | [Wikipedia](https://en.wikipedia.org/wiki/Blasphemous_(video_game)) |
| Hollow Knight: Silksong | Unity (4 Sep 2025, including Switch 2) | [Wikipedia](https://en.wikipedia.org/wiki/Hollow_Knight:_Silksong) |
| Celeste | Microsoft XNA (per the infobox) | [Wikipedia](https://en.wikipedia.org/wiki/Celeste_(video_game)). Sickhead lists Celeste among its ports — [Wikipedia: Sickhead](https://en.wikipedia.org/wiki/Sickhead_Games) |
| Hyper Light Drifter | GameMaker Studio | [Wikipedia](https://en.wikipedia.org/wiki/Hyper_Light_Drifter). Wii U/Vita versions were cancelled over GameMaker problems; PS/Xbox ports took about 6 months; the Switch port was done by Abylight together with YoYo Games |
| Pizza Tower | GameMaker | [Wikipedia: GameMaker](https://en.wikipedia.org/wiki/GameMaker) |
| Megabonk | Unity (IL2CPP; 3D), solo developer vedinad, 18 Sep 2025 | [Wikipedia](https://en.wikipedia.org/wiki/Megabonk); [BepInEx IL2CPP pack](https://thunderstore.io/c/megabonk/p/BepInEx/BepInExPack_IL2CPP) |
| Ball x Pit | Unity | PC/PS5/Xbox/Switch on 15 Oct 2025; Switch 2 on 28 Oct 2025; mobile on 12 Mar 2026 — [Wikipedia](https://en.wikipedia.org/wiki/Ball_x_Pit) |
| Cult of the Lamb | Unity | [Wikipedia](https://en.wikipedia.org/wiki/Cult_of_the_Lamb) |
| Rogue Legacy 2 | Unity | [Wikipedia](https://en.wikipedia.org/wiki/Rogue_Legacy_2) |
| Dome Keeper | Godot | [Wikipedia: Godot](https://en.wikipedia.org/wiki/Godot_(game_engine)) |
| Cassette Beasts | Godot | [Wikipedia: Godot](https://en.wikipedia.org/wiki/Godot_(game_engine)) |
| Skul: The Hero Slayer | **Not verified** | The Wikipedia infobox lists no engine — [Wikipedia](https://en.wikipedia.org/wiki/Skul:_The_Hero_Slayer) |

- Brotato's console porter, Seaven Studio, said the work split roughly 50/50 between making the game console-compliant and making it run smoothly, which "sometimes meant rewriting parts of the engine" (Godot 3) — [Seaven Studio news](https://www.seaven-studio.com/news/)

### Inferences
- Both closest references point to the same two architectures:
  - Windblown: Unity plus a deterministic C# simulation (Quantum).
  - Dead Cells: a custom Haxe engine with HL/C console ports.
- Halls of Torment shows the alternative: Godot plus native code for the hot paths, shipped on PS5 and Xbox.
- Every JS-based hit in the list (Vampire Survivors, CrossCode) had to re-platform or build an interpreter for consoles. That is direct evidence against a web/Tauri stack for this project.
- Godot's console track record now covers several porters and shipped horde/roguelite titles: Brotato on Switch/PS/Xbox via Seaven, and Halls of Torment on PS5/Xbox Series since Oct 2025. I found no evidence yet of a Godot **C#** game shipped on consoles: Slay the Spire 2's console status was not verified.

### Gaps
- Skul's engine is unverified (widely reported as Unity). The original Hollow Knight's engine is unverified this session (widely reported as Unity).
- Death Must Die is only weakly confirmed as Unity.
- How Celeste, Hyper Light Drifter and Balatro were technically ported to consoles: a search snippet says LocalThunk did Balatro's Switch, PC, macOS and mobile ports himself (Reddit AMA), but I could not fetch the original. How LÖVE runs on consoles is unknown.
- Whether Slay the Spire 2 has shipped or announced console versions, and through which porter (this would prove Godot C# on consoles).

## Q3. Horde performance patterns per engine (Steam Deck / Switch-class)

### Takeaway
In every engine, 1,000–5,000 enemies plus projectiles at 60 fps works only with a data-oriented sim. That means contiguous arrays, a spatial hash or grid instead of engine physics bodies, pooling, and instanced or batched sprite rendering. Godot needs to bypass per-enemy nodes (MultiMesh/RenderingServer plus C#/C++ logic), as Halls of Torment learned. Unity offers Burst/Entities but sprites under ECS need community instancing. Bevy is ECS-native, but its 2D rendering lags its 3D.

### Cited Findings
- Godot nodes are expensive at horde scale:
  - Thousands of `CharacterBody2D` nodes are "very heavy", and the per-node velocity update dominated frame time. Godot has no DOTS equivalent — [Godot forum](https://forum.godotengine.org/t/godot-does-not-have-a-technology-similar-to-unitys-dots/56810)
  - Every collision shape creates pair objects with its neighbours, so turn collision detection off wherever possible and watch the 2D collision-pair counter — [Godot forum: Collision pairs](https://forum.godotengine.org/t/collision-pairs-optimizing-performance-of-bullet-hell-enemy-hell-games/35027)
- Godot patterns that work:
  - MultiMesh plus low-level servers — [Godot forum MultiMesh guide](https://forum.godotengine.org/t/rendering-a-million-objects-in-godot-a-complete-guide-to-the-multi-mesh-system/138856)
  - The PerfBullets addon draws bullets with `MultiMeshInstance2D` and runs the logic in C++ — [Asset Library: PerfBullets](https://godotengine.org/asset-library/asset/edit/10491)
  - One developer saw no problems until 3,000+ bullets by using distance checks instead of physics — [itch.io post](https://itch.io/post/3174429)
- Godot's "Using servers" docs:
  - When to use: for "tens of thousands of instances" processed every frame.
  - RIDs are allocated and freed manually.
  - Avoid querying servers every frame, because it stalls the asynchronous servers; write-only calls are preferred.
  - The Bullet Shower demo is the reference example.
  - Source: [Godot docs: Using servers](https://docs.godotengine.org/en/stable/tutorials/performance/using_servers.html)
- Godot 2D batching:
  - 4.4 brought batching to the Forward+/Mobile renderers ([Godot 4.4](https://godotengine.org/releases/4.4/)).
  - Batching works best for repeated sprites that share a texture, and draw order is painter's order ([Godot 3.5 batching docs](https://docs.godotengine.org/en/3.5/tutorials/performance/batching.html)).
  - A 4.4-dev regression report on 2D Forward+ performance existed, and I did not confirm its resolution — [GitHub #99420](https://github.com/godotengine/godot/issues/99420)
- Godot physics interpolation:
  - It renders between fixed ticks (60 ticks/s by default). It is optional and off by default.
  - It adds 1–2 ticks of visual latency.
  - Choose the tick rate at design time.
  - Sources: [Godot docs: Physics interpolation](https://docs.godotengine.org/en/stable/tutorials/physics/interpolation/physics_interpolation_introduction.html); 2D support arrived in [4.3](https://godotengine.org/releases/4.3/)
- **Halls of Torment** hit "a performance wall" as sprite counts grew. They fixed it by moving the most calculation-heavy code to C++ and onto separate threads, which was possible because Godot is open source — [Fullcleared](https://fullcleared.com/features/inside-halls-of-torment-an-interview-with-chasing-carrots/)
- Unity:
  - From 6.4, ECS (Entities, Collections, Mathematics, Entities Graphics) ships as core packages — [Unity 6.4 docs](https://docs.unity3d.com/6000.4/Documentation/Manual/ECSFeature.html)
  - Sprites under ECS are usually drawn by community instanced renderers. NSprites draws all entities sharing a material in one draw call, with a shader that must support instancing — [NSprites](https://github.com/Antoshidza/NSprites)
  - The older `Graphics.DrawMeshInstanced` caps at 1,023 instances per call — [paullj instanced sprite renderer](https://github.com/paullj/unity-ecs-instanced-sprite-renderer/blob/master/README.md)
  - The 2D Entities conversion supports only some SpriteRenderer properties — [Unity 2D Entities docs](https://docs.unity3d.com/Packages/com.unity.2d.entities@0.31/manual/Conversion.html)
- Windblown:
  - Content generation is batched and multithreaded, with heavy use of Unity's Burst.
  - Culling runs in both Unity and Quantum's logic layer.
  - Some VFX are limited for remote players.
  - Source: [Photon blog](https://blog.photonengine.com/?p=6487)
- Vampire Survivors' move to Unity was publicly framed as bringing "big performance improvements" — [GamingOnLinux](https://gamingonlinux.com/2023/07/vampire-survivors-switching-to-new-game-engine-on-august-17)
- Bevy says its 2D rendering "has started to lag behind" 3D and plans to unify the internals — [Bevy 0.19](https://bevy.org/news/bevy-0-19/)
- Phaser 4 claims its Sprite GPU Layer can render over 1M sprites (vendor/reviewer claim) — [Phaser news](https://phaser.io/news/2026/04/gamefromscratch-reviews-phaser-4-the-biggest-release-ever)
- Hardware floor:
  - **Switch 2** (Digital Foundry figures):
    - CPU: 8× Cortex-A78C, 6 available to games, about 1.0–1.1 GHz (max 1.7).
    - GPU: Ampere, 1,536 CUDA cores.
    - Memory: 12 GB LPDDR5X, 9 GB for games, 102/68 GB/s docked/handheld.
    - Sources: [Nintendo Life](https://www.nintendolife.com/news/2025/05/nintendo-switch-2-final-tech-specs-have-been-confirmed); [Notebookcheck/DF](https://www.notebookcheck.net/Digital-Foundry-confirms-Nintendo-Switch-2-specs-with-CPU-GPU-and-surprising-memory-allocation-details.1015981.0.html)
  - Steam Deck Verified requires a default config that hits **30 fps at 800p**. Valve's compatibility page now also tests "Steam Machine" at 30 fps / 1080p — [Steamworks: Deck compatibility](https://partner.steamgames.com/doc/steamdeck/compat)

### Inferences
- **Recommended horde pattern (engine-agnostic):**
  - Keep the sim in a compiled language (C#/C++/Rust), never in GDScript/Lua/GML for the hot path.
  - Store entities as struct-of-arrays (SoA) with fixed capacity and free-lists (pooling).
  - Use a uniform-grid spatial hash rebuilt every tick for separation, contact damage and projectile hits.
  - Use one shared flow field to the player (BFS on a coarse grid), recomputed every N ticks, instead of per-enemy pathfinding.
  - Use simple circle colliders and no engine physics.
  - Update far or off-screen enemies at lower frequency.
  - Cap projectiles by budget and merge damage numbers.
  - Render with a few instanced draws (one MultiMesh or instanced batch per atlas/material), driven by per-instance animation frame and tint data.
- The CPU budget is set by the Switch-class floor. Switch 2's game cores run at about 1 GHz on A78C; a 5k-enemy sim should target about 2–4 ms per 60 Hz tick on Deck/Switch 2 so that rollback (re-simulating several ticks) stays feasible later. Switch 1 (not researched) would probably need lower caps or a 30 Hz sim with interpolation.
- In Godot, the practical stack is a C# sim operating on arrays, with Godot as a thin renderer through `MultiMeshInstance2D` or the RenderingServer, plus physics/visual interpolation. Use C++/Rust GDExtension only if profiling demands it, as Halls of Torment did.
- In Unity, plain C# plus Burst jobs over NativeArrays, rendered with `Graphics.RenderMeshInstanced` or a sprite-instancing package, avoids committing to Entities while ECS is still in flux.

### Gaps
- I found no published benchmarks of 5k-entity hordes per engine on Steam Deck, Switch 1 or Switch 2.
- Not re-verified this session: Godot `MultiMeshInstance2D` per-instance custom data for animation frames (believed supported), and Unity's current recommended API for instanced sprites.
- Switch 1 specs and feasibility were not researched (the search budget was used up).

## Q4. Determinism: seeded dailies, replays, server-side validation, lockstep/rollback co-op

### Takeaway
Basic IEEE-754 operations (+, −, ×, ÷, √) are reproducible, but transcendental functions (libm), fused multiply-add (FMA) contraction, compiler flags and SIMD widths are not. Go itself is allowed to fuse `x*y+z`. WASM leaves NaN bit patterns open, and its relaxed SIMD is non-deterministic. The robust choice for a sim that must replay identically on PC, consoles and the validation server is **integer fixed-point state plus an in-house PRNG, in a pure sim core**. Engine physics (Godot, Unity PhysX/Box2D) is not cross-platform deterministic. Off-the-shelf deterministic frameworks exist mainly for Unity (Photon Quantum) and Rust (GGRS/Rapier).

### Cited Findings
- Float pitfalls:
  - Gaffer on Games: floats can be made deterministic only with the same compiler, the same architecture and platform-specific tricks. It is "incredibly naive" to expect arbitrary C/C++ float code to match across compilers or architectures — [Gaffer on Games](https://new.gafferongames.com/post/floating_point_determinism/)
  - Basic operations are deterministic in practice, but transcendentals differ — [arXiv: Lock-step simulation is child's play](https://arxiv.org/pdf/1705.09704)
  - sin/exp/ln/pow come from the platform libm and differ in the last ulp between macOS, glibc, MSVC and wasm; the fix avoids `mul_add` — [alice-det-math docs](https://docs.rs/alice-det-math)
  - Fusing into FMA rounds once instead of twice — [bugnet](https://bugnet.io/blog/how-to-debug-floating-point-determinism-across-platforms)
  - Factorio needed exact-match trig implementations — [Factorio forum](https://forums.factorio.com/viewtopic.php?p=28794)
- **Go:** "An implementation may combine multiple floating-point operations into a single fused operation, possibly across statements", and an explicit conversion such as `float64(x*y) + z` prevents fusion — [Go spec](https://go.dev/ref/spec). This matters if the Go server ever re-implements float sim math, for example on arm64 servers.
- **WebAssembly:**
  - NaN bit patterns (and the sign bit) may vary — [webassembly.org: Nondeterminism](https://www.webassembly.org/docs/nondeterminism/)
  - Relaxed SIMD trades cross-architecture determinism for speed. Wasmtime can canonicalize NaNs or make relaxed SIMD deterministic at a cost — [Wasmtime docs](https://docs.wasmtime.dev/examples-deterministic-wasm-execution.html)
  - A proposal to canonicalize NaNs exists; its merge status is unverified — [hackmd proposal](https://hackmd.io/@sunfish/BJXasjFEo)
- **Rapier (Rust physics)** is only locally deterministic by default.
  - Cross-platform determinism, including WASM, requires the `enhanced-determinism` feature and trig functions from nalgebra traits.
  - The feature conflicts with `simd8`.
  - The JS npm builds are not cross-platform deterministic; the `-deterministic` builds are.
  - Source: [Rapier determinism guide](https://rapier.rs/docs/user_guides/rust/determinism/)
- **Unity Burst `FloatMode.Deterministic`:**
  - "Only supported on 64-bit architectures".
  - Uses deterministic math function implementations and flushes subnormals to zero.
  - NaN bit patterns can still differ.
  - Applies **only to Burst-compiled code**, so managed code is outside the guarantee.
  - Source: [Unity Burst docs (6.7)](https://docs.unity.com/en-us/engine/6000.7/manual/programming-environment/script-compilation/burst/compilation/float-precision-determinism)
  - Earlier forum threads describe Burst determinism as having been "on hold", with teams switching to fixed-point instead — [Unity discussions](https://discussions.unity.com/t/state-of-determinism-in-unity/867770); [Intel vs AMD thread](https://discussions.unity.com/t/floating-point-determinism-in-unity-intel-vs-amd/813566)
- **Photon Quantum (Unity):**
  - Described as "100% deterministic".
  - Clients predict inputs and roll back and re-simulate on server-confirmed inputs.
  - Input-only networking, a "garbage collection free ECS", and built-in physics, pathfinding and bots.
  - Replays were called "the most effective debugging tool encountered in over 20 years" at Motion Twin.
  - Unity is the view layer only; Quantum simulations "run anywhere".
  - Learning curve is "a few weeks if new to ECS".
  - Source: [Photon blog: Windblown](https://blog.photonengine.com/?p=6487)
  - Pricing: free up to 100 CCU; Quantum Plus is $95 for 200 CCU/month for one year. Latest 3.0.12 (8 Jul 2026) — [Unity Asset Store: Quantum Plus](https://assetstore.unity.com/packages/tools/network/photon-quantum-plus-deterministic-game-engine-301832). The sample needs Unity 2022.3.38f1+ — [Photon docs](https://doc.photonengine.com/quantum/v3/game-samples/quantum-simple-fps/download)
- **Godot:**
  - Built-in physics uses floats and is unsuitable for deterministic simulation. GDQuest points to plugins like SG Physics 2D and warns you must also write your own fixed-point code — [GDQuest glossary](https://www.gdquest.com/library/glossary/deterministic_simulation)
  - SG Physics 2D (by dsnopek) is fixed-point physics written for rollback — [Godot forums](https://godotforums.org/d/28338-getting-started-with-sg-physics-2d-and-deterministic-physics-in-godot-good-for-rollback-netcode)
  - `RandomNumberGenerator` "currently uses PCG32", and the algorithm is "an implementation detail and should not be depended upon" — [Godot docs: RandomNumberGenerator](https://docs.godotengine.org/en/stable/classes/class_randomnumbergenerator.html)
- Godot rollback addons:
  - "Godot Rollback Netcode (Godot 4)" by dsnopek: 1.0.0-alpha, targets Godot 4.2, page dated 2023-12-25 — [Asset Library](https://godotengine.org/asset-library/asset/2450)
  - "Delta Rollback": 0.11.0, targets Godot 4.2 — [Asset Library](https://godotengine.org/asset-library/asset/edit/24522)
  - **netfox** (MIT, "Godot 4.x is supported"): client-side prediction with server-side reconciliation, i.e. a server-authoritative state-sync rollback model, not lockstep. C# support is "experimental" through Netfox Sharp — [GitHub: netfox](https://github.com/foxssake/netfox)
  - netfox.noray adds NAT punch-through and relay — [Asset Library: netfox.extras](https://www.godotengine.org/asset-library/asset/edit/19533)
- GGPO became open source under MIT on 9 Oct 2019 — [Wikipedia: GGPO](https://en.wikipedia.org/wiki/GGPO). GGRS is its Rust reimplementation (MIT/Apache) — [GitHub: ggrs](https://github.com/gschup/ggrs). **bevy_ggrs 0.19 targets Bevy 0.17**, lagging behind Bevy 0.19 — [docs.rs bevy_ggrs](https://docs.rs/crate/bevy_ggrs/0.19.0)
- Networking on Steam:
  - **Steam Datagram Relay** relays P2P and dedicated-server traffic, hides IPs, and authenticates, encrypts and rate-limits. For P2P you just use `ISteamNetworkingSockets::CreateListenSocketP2P`/`ConnectP2P` — [Steamworks: SDR](https://partner.steamgames.com/doc/features/multiplayer/steamdatagramrelay)
  - The open-source GameNetworkingSockets lacks Steam-only pieces (auth, signaling, SDR) — [GameNetworkingSockets](https://github.com/michicc/GameNetworkingSockets)
  - **Remote Play Together** streams a local-co-op game to friends, and only the host needs to own it. It is enabled automatically for games tagged Local Co-op/Multiplayer — [Steamworks: Remote Play](https://partner.steamgames.com/doc/features/remoteplay)

### Inferences
- **Use integer fixed-point for all sim state** (for example Q16.16 in `int32` for positions/velocities, `int64` for accumulators).
  - Use lookup-table or polynomial trig and integer sqrt, and floats only in presentation.
  - Integer math is bit-identical across x64, ARM64 (Switch 2, Apple Silicon), IL2CPP/NativeAOT, WASM and Go, and no FMA or libm issue can touch it. That is far cheaper than repeating aegis-draft's V8/JSC dmath experience on more compilers.
  - This is the same approach the Unity and Godot ecosystems point to (fixed-point Quantum-style math, SG Physics).
- Write the PRNG yourself in sim code (PCG32 or xoshiro), with **separate streams per subsystem** (spawns, loot, AI, crits), so content tweaks don't cascade through every roll. Never use engine RNGs: Godot calls its algorithm an implementation detail.
- **Replay format:** sim version, content-data hash, seed, input stream (per-tick packed gamepad state), and a per-N-tick state hash. CI should replay golden runs on Windows x64, Linux x64 and macOS arm64 and compare hashes at every tick.
- **Co-op:**
  - Phase 1 is local co-op on one sim, which gets online play for free through Remote Play Together on Steam.
  - Phase 2 is online co-op on the same deterministic sim: delay-based lockstep (fine for PvE at 2–4 ticks of input delay) or GGPO-style rollback. Rollback is only viable if snapshotting and re-simulating 5k entities over 5–8 ticks fits the frame budget. That argues for flat SoA state that can be `memcpy`-snapshotted.
- If online co-op is a launch pillar, **Unity + Photon Quantum** is the only turnkey, proven deterministic rollback stack for this genre (Windblown). Elsewhere you write the lockstep/rollback layer yourself: GGRS in Rust, or a small GGPO-style layer in C#.

### Gaps
- Not verified (search budget exhausted):
  - whether .NET RyuJIT or NativeAOT ever auto-contracts FMA;
  - whether .NET `Math.Sin` and friends differ between Windows, Linux, macOS and ARM64;
  - whether IL2CPP's C++ output can be FMA-contracted by console compilers.
  Integer fixed-point sidesteps all three, but they matter for any float code in the sim.
- Photon Quantum's FP format and the pricing of server-side validation plugins (the docs page returned 404).
- Whether SDR is free for all Steam titles. It is widely understood to be, but that wasn't stated in the fetched docs.
- Godot 4 rollback addon compatibility beyond 4.2 (Godot Rollback Netcode, Delta Rollback).

## Q5. Agent-friendliness for AI coding agents

### Takeaway
Godot is the most agent-friendly full engine:
- text scenes and resources;
- a full headless CLI (import, export, run scripts, check-only);
- CLI test runners (GUT/gdUnit4) with a GitHub Action;
- several MCP servers.

Its main hazard is LLMs mixing in Godot 3 API. Unity is improving (official MCP beta, May 2026), but YAML + GUID scenes, `.meta` files and an editor-centric workflow remain harder for agents. GameMaker's new GMRT (plain-text projects, GM-CLI, Claude Code integration) is promising but new. Code-first stacks (MonoGame, Bevy) are naturally agent-friendly but leave tooling to you. For any engine, the biggest single lever is a **pure sim library with fast engine-free tests**.

### Cited Findings
- Godot VCS docs (4.7): the engine "aims to be VCS-friendly and generate mostly readable and mergeable files". Ignore `.godot/` (cache) and `*.translation` — [Godot docs: VCS](https://docs.godotengine.org/en/stable/tutorials/best_practices/version_control_systems.html). 4.4 made UIDs and `.uid` sidecar files a fully supported workflow — [Godot 4.4](https://godotengine.org/releases/4.4/)
- Godot CLI (4.7 docs):
  - `--headless` (required on CI without a GPU)
  - `--export-release` / `--export-debug` / `--export-pack <preset> <path>` (needs an editor build and installed templates)
  - `--import` (imports and quits)
  - `-s/--script` (SceneTree/MainLoop scripts)
  - `--check-only` (parse script and quit)
  - A relative export path resolves against the directory of `project.godot`.
  - Source: [Godot docs: Command line](https://docs.godotengine.org/en/stable/tutorials/editor/command_line_tutorial.html)
- Godot test runners:
  - GUT runs headless with `godot --headless -s addons/gut/gut_cmdln.gd`. It needs `-gexit` or CI hangs, and can write JUnit XML — [tessl guide](https://tessl.io/registry/testland/godot-gut-tests). A CI freeze was reported with GUT 9.0.1 on Godot 4.0.2 — [GUT #491](https://github.com/bitwes/Gut/issues/491)
  - gdUnit4 has a CLI runner with JUnit/HTML reports and a maintained GitHub Action (`godot-version`, `paths` inputs) — [gdUnit4 Asset Store](https://store.godotengine.org/asset/mikeschulze/gdunit4-unit-testing-framework/); [gdUnit4-action](https://github.com/MikeSchulze/gdUnit4-action)
  - C# tests may fail headless on Linux, and the workaround runs under xvfb, which is slow without a GPU — [gdUnit4Net #350](https://github.com/godot-gdunit-labs/gdUnit4Net/discussions/350)
- Godot MCP servers (community; maintenance quality unverified):
  - "Godot AI": about 43 tools, Godot 4.5+ (4.7+ recommended), set up with `claude mcp add` — [PyPI listing, as surfaced by search; page fetch failed](https://pypi.org/project/godot-ai)
  - VberAI godot-mcp: in-editor HTTP server, no Node.js needed — [mcpservers.org](https://mcpservers.org/ru/servers/vberai/godot-mcp)
  - Godot MCP Pro: paid, 172 tools — [Glama](https://glama.ai/mcp/servers/gyfy6t0b2i)
  - A Slay the Spire 2 modding MCP (decompile, build, live scene inspection) shows the tooling around Godot C# — [Glama: STS2 Modding MCP](https://glama.ai/mcp/servers/p6zqo1xzsy)
- LLM confusion between Godot 3 and 4:
  - Common wrong patterns: `yield` instead of `await`, `KinematicBody2D` instead of `CharacterBody2D`, `export var` instead of `@export`, and string-based `connect()`.
  - The cited cause is years of Godot 3 tutorials in the training data.
  - These sources are vendor blogs with no independent study — [Surmado](https://www.surmado.com/blog/stop-learning-godot-from-wrong-ai); [Summer Engine](https://www.summerengine.com/blog/best-ai-for-gdscript)
- Unity:
  - UnityYAMLMerge ("Smart Merge") merges `.unity`, `.prefab` and `.meta` semantically. By default it does not merge other YAML such as `.asset`, `.mat` and `.controller`, and unresolved conflicts go to a fallback tool — [Unity docs 6.6: Smart Merge](https://docs.unity3d.com/Manual/SmartMerge.html)
  - The official MCP is in open beta and requires Unity Cloud plus an AI subscription or trial — [Unity blog](https://unity.com/blog/unity-ai-mcp-how-to-get-started)
  - Community MCPs exist: IvanMurzak/Unity-MCP (Apache-2.0) and Coplay's unity-mcp — [Enterprise DNA directory](https://enterprisedna.co/directories/mcp/ivanmurzak-unity-mcp); [xikelabs/unity-mcp](https://github.com/xikelabs/unity-mcp)
- GameMaker GMRT: "project files are plain text that track well in Git", GM-CLI covers build/run/compile and project operations (create project, add sprite, add event), and Claude Code integration is named — [Opera press](https://press.opera.com/?p=3924); [GameMaker blog](https://gamemaker.io/blog/update-spring-2026)
- Bevy is code-first (the `bsn!` macro, no `.bsn` loader yet) and ships breaking changes every release with migration guides — [Bevy 0.19](https://bevy.org/news/bevy-0-19/)
- MonoGame 3.8.5 replaces the content tool with a code-centric Content Builder (a C# project) — [MonoGame blog](https://monogame.net/blog/2026-07-15-3.8.5-release-2026/)
- GameMaker GM-CLI is open source under Apache on npm — [GameMaker blog](https://gamemaker.io/blog/update-spring-2026)

### Inferences
- **Recommended conventions for an agent-driven repo:**
  1. Put the sim in a separate plain .NET (or Rust) library with no engine references. Agents iterate with `dotnet test` in seconds, without the editor, GPU or import step. This mirrors aegis-draft's headless bot-sim workflow.
  2. Treat content as data: JSON/YAML definitions under a JSON Schema (as aegis-draft already does with `schema/`), validated in CI. Agents edit data, not scenes.
  3. Keep scenes thin and mostly generated or instanced from code. Avoid deep inheritance of editor-authored scenes that agents can't diff.
  4. Pin the engine version in AGENTS.md/CLAUDE.md ("Godot 4.7.x, C#, Godot 4 API only") and add a grep lint for Godot 3-isms (`yield(`, `KinematicBody2D`, `export var`, string `connect(`).
  5. Use headless CI: `godot --headless --import`, then the tests, then `--export-release` for Windows, Linux and macOS. Add golden-replay determinism checks on a three-OS matrix.
  6. Commit `.uid` files, which inferring from 4.4's UID workflow are part of project identity. Ignore `.godot/`.
  7. Use an MCP server only for inspection and screenshots, not as the source of truth.
- Unity can be made workable (Force Text, Smart Merge, batchmode CI, official MCP), but merge conflicts in YAML scenes and prefabs, plus GUID/`.meta` hygiene, will cost a solo agent-heavy workflow more than Godot's text formats.
- Choosing C# over GDScript reduces the Godot 3-vs-4 confusion surface (C# APIs also changed between 3 and 4, but less is in the training data) and matches LLMs' strong C# fluency.

### Gaps
- Not re-verified this session: Unity batchmode/`-runTests` CLI specifics and Unity's current headless Linux build story.
- Defold's on-disk formats (believed to be text) were not checked.
- I found no independent benchmark of LLM code accuracy per engine.
- I couldn't fetch the godot-ai PyPI page to verify its details.

## Q6. Backend: Steamworks vs own Go server; server-side re-simulation

### Takeaway
Use Steamworks for what it does natively and well:
- leaderboards with replay attachments and **server-trusted writes**;
- cloud saves;
- Steam Input glyphs and Deck compliance;
- SDR P2P networking;
- Remote Play Together.

Keep the Go server for what Steam can't do: issuing daily seeds, receiving run submissions, **re-simulating replays to validate them**, cross-platform identity, and leaderboards for consoles. Validation should run the *exact same sim binary*. For a C# sim, that means a .NET validator worker next to the Go service. For a Rust/C sim, compile to WASM and run it in-process in Go via wazero.

### Cited Findings
- Steam leaderboards:
  - Up to **10,000 leaderboards** per title and unlimited entries. Each score is an `int32` plus up to **64 `int32` details**.
  - **`AttachLeaderboardUGC`** attaches replays or ghosts, which "will be available even if the underlying Cloud file is changed or deleted".
  - With **Writes = Trusted**, "scores cannot be set by clients"; they can only be set through the **Web API `SetLeaderboardScore`**.
  - Upload methods are KeepBest and ForceUpdate.
  - Source: [Steamworks: Leaderboards](https://partner.steamgames.com/doc/features/leaderboards)
- Steam Deck Verified criteria:
  - The default controller config must reach all content.
  - On-screen glyphs must match the input device; the Steam Input API is recommended.
  - Text entry uses the Steamworks on-screen keyboard API or built-in controller entry.
  - 1280×800 is preferred, and fonts must be at least 9 px (12 px recommended).
  - No launcher friction.
  - Performance: 30 fps at 800p (Steam Machine: 30 fps at 1080p).
  - Source: [Steamworks: Deck compatibility](https://partner.steamgames.com/doc/steamdeck/compat)
- SDR and Steam networking APIs — [Steamworks: SDR](https://partner.steamgames.com/doc/features/multiplayer/steamdatagramrelay). Remote Play Together can be invoked from custom UI through `ISteamRemotePlay::BSendRemotePlayTogetherInvite()` — [Steamworks: Remote Play](https://partner.steamgames.com/doc/features/remoteplay). GodotSteam exposes a Remote Play class — [GodotSteam docs](https://godotsteam.com/classes/remote_play)
- **wazero:**
  - A WebAssembly Core 1.0/2.0 runtime written in Go, with "no dependencies" and no CGO, so cross-compilation keeps working.
  - Its ahead-of-time compiler is the default and "often by order of magnitude (10x)" faster than its interpreter (project's own claim).
  - The compiler is tested on Linux amd64/arm64 and macOS arm64.
  - Source: [GitHub: wazero](https://github.com/wazero/wazero); [README](https://cdn.jsdelivr.net/gh/wazero/wazero@main/README.md)
- WASM's NaN non-determinism and relaxed SIMD caveats apply to a WASM validator — [webassembly.org](https://www.webassembly.org/docs/nondeterminism/)
- Godot's `--headless` mode is meant for servers and `--script` runs, so a headless engine build could act as a validator — [Godot CLI docs](https://docs.godotengine.org/en/stable/tutorials/editor/command_line_tutorial.html)
- Quantum simulations are not Unity-dependent ("run anywhere") — [Photon blog](https://blog.photonengine.com/?p=6487)

### Inferences
- **Validation flow:**
  1. The Go API issues the daily seed and content version.
  2. The client plays and uploads its replay (seed, inputs, state hashes) to Go.
  3. Go queues the replay for a validator worker.
  4. The worker re-runs the sim headless at full speed. A 20-minute run at 60 Hz is 72k ticks; with a fixed-point SoA sim this should take seconds of CPU, but that needs measuring.
  5. On a matching final state and hashes, Go writes the score to Postgres and, for Steam, to the **trusted** Steam leaderboard via `SetLeaderboardScore`.
  6. The client attaches the replay as UGC for ghosts and replays. Whether a client can attach UGC to a server-written entry is unverified.
- **Validator implementation options:**
  - **(a) .NET worker container** running the same `Sim.dll`. Simplest for a C# sim; Go calls it over HTTP/gRPC or a job queue.
  - **(b) Rust/C/Zig sim compiled to WASM, run in-process via wazero.** Zero extra runtime, deterministic if NaN payloads never matter and relaxed SIMD is disabled. Best if the sim is Rust.
  - **(c) Headless engine build.** Heaviest option, and it couples validation to the renderer build. Avoid it.
- Porting the sim to Go would duplicate logic and invite drift. Don't do it, even though integer fixed-point would make a Go port exact.
- The existing chi + Postgres + websocket relay can keep handling accounts, saves, dailies and leaderboards. For action netcode, add UDP (Steam SDR on Steam; a self-hosted UDP relay for cross-play later), because websockets are TCP and suffer head-of-line blocking.
- Steam Cloud should hold save files on PC. Use Steam Input (via GodotSteam or Steamworks.NET) for glyphs to pass Deck Verified. Consoles will need their own platform services (trophies/achievements, saves, online), so keep a platform-abstraction interface in the game.

### Gaps
- Whether a client can attach UGC to a leaderboard entry written by the Web API on a Trusted board.
- Steam Web API rate limits.
- The maturity of C# → WASM (WASI) for running a C# sim inside wazero.
- Console-platform policies on external leaderboards and accounts (not researched).

## Q7. Recommendation: ranked stacks, decision matrix, risks, proposed architecture

### Takeaway
**#1: Godot 4.7 (.NET build) as the presentation shell, with an engine-agnostic deterministic C# simulation library.** It gives the best agent-friendliness, licensing and 2D/pixel tooling, and has genre proof (Brotato, Halls of Torment, Slay the Spire 2 in C#). Consoles go through W4 or a porter.
**#2: Unity 6.3 LTS with the same C# sim core**, or **Photon Quantum 3 if online co-op with rollback is a launch pillar** (the Windblown precedent). It has the most proven console path, but higher cost and friction for agents.
**#3: MonoGame 3.8.5, code-first, with the same C# sim core.** It gives maximal control and is fully text/code-based, but you build your own tooling.

The common denominator, a **pure C# fixed-point sim**, keeps the engine decision reversible and allows .NET-based server validation.

### Cited Findings
- Godot ships 4.7.2 stable (18 Aug 2026) under MIT, with C# desktop export, pixel-art integer scaling, 2D physics interpolation, 2D batching and a full headless CLI — [Wikipedia](https://en.wikipedia.org/wiki/Godot_(game_engine)); [C# docs](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html); [multiple resolutions](https://docs.godotengine.org/en/stable/tutorials/rendering/multiple_resolutions.html); [4.3](https://godotengine.org/releases/4.3/); [4.4](https://godotengine.org/releases/4.4/); [CLI](https://docs.godotengine.org/en/stable/tutorials/editor/command_line_tutorial.html)
- Godot consoles: W4 Starter is $800/yr per platform or $2,000/yr for all (<$300k revenue), C# is beta on Switch/Xbox, and Switch 2 is early beta — [W4](https://www.w4games.com/w4consoles). Porters such as Pineapple Works list C# — [Godot console docs](https://docs.godotengine.org/en/4.5/tutorials/platform/consoles.html)
- Slay the Spire 2 shipped on Godot with C#, after migrating about 2 years of Unity work — [Wikipedia](https://en.wikipedia.org/wiki/Slay_the_Spire_II); [NuGet mod template](https://www.nuget.org/packages/s2smodtemplate.templates)
- Unity: consoles need Pro ($2,310/seat/yr) or a Preferred Platform key — [Unity Personal](https://unity.com/products/unity-personal); [pricing](https://unity.com/products/pricing-updates). Windblown uses Unity plus Quantum — [Photon](https://blog.photonengine.com/?p=6487)
- MonoGame 3.8.5 rebuilt its native backend, with Xbox sharing the DX12 code path — [MonoGame blog](https://monogame.net/blog/2026-07-15-3.8.5-release-2026/). Sickhead stewards MonoGame and ports to consoles — [Wikipedia: Sickhead](https://en.wikipedia.org/wiki/Sickhead_Games)
- Defold has free console access, including Xbox since 1.14 (Oct 2026) — [Wikipedia: Defold](https://en.wikipedia.org/wiki/Defold); [Digital Citizen](https://www.digitalcitizen.life/defold-1-14-0-adds-free-xbox-support-for-approved-developers/)
- JS stacks needed engine changes or an interpreter to reach consoles (Vampire Survivors, CrossCode) — [GamingOnLinux](https://gamingonlinux.com/2023/07/vampire-survivors-switching-to-new-game-engine-on-august-17); [RPG Site](https://www.rpgsite.net/news/9249-deck13-and-radical-fish-games-provide-an-update-on-the-console-versions-of-crosscode)

### Inferences

**Decision matrix.** My judgement scores, 1 (poor) to 5 (best), for this specific project. Each score is grounded in the cited findings above.

| Criterion (weight) | Godot 4.7 + C# sim | Unity 6.3 LTS + C# sim (opt. Quantum) | MonoGame 3.8.5 + C# sim | GameMaker (GMS2 LTS / GMRT) | Defold 1.14 | Bevy 0.19 | Heaps/HashLink | LÖVE 11.5 | Web (Phaser 4/Pixi + Electron/Tauri) |
|---|---|---|---|---|---|---|---|---|---|
| Native PC/Deck/Linux, CLI builds (×2) | 5 | 5 | 4 | 4 | 4 | 4 | 4 | 4 | 3 |
| Console path: maturity × cost (×3) | 3 (W4/porters; C# beta) | 5 (Pro seat) | 3 (private/porters) | 4 (Enterprise) | 4 (free, approval) | 1 | 2 (HL/C, private) | 1 | 1 |
| 2D pixel-art tooling (×2) | 4 | 4 | 2 (DIY) | 5 | 3 | 2 | 3 | 3 | 4 |
| Horde ceiling 1–5k @60 fps (×3) | 4 (C# sim + MultiMesh; C++ if needed) | 5 (Burst/Jobs/ECS) | 4 (you control it) | 2 | 3 (needs C++ ext) | 5 | 4 | 2 | 3 |
| Sim-core separation/determinism (×3) | 5 | 5 (Quantum turnkey) | 5 | 2 | 3 | 5 | 4 | 3 | 4 |
| Server re-sim reuse with Go (×2) | 4 (.NET worker) | 4 (.NET worker; Quantum server) | 4 | 1 | 2 | 5 (WASM/wazero) | 3 | 2 | 4 |
| Agent-friendliness: text/CLI/tests (×3) | 5 | 3 | 5 | 3 (GMRT new) | 3 | 4 | 3 | 4 | 4 |
| LLM familiarity / API stability (×2) | 3 (3-vs-4 confusion; ~6-monthly minors) | 4 | 4 | 2 | 3 | 2 (0.x churn) | 1 | 4 | 5 |
| Licensing/business risk (×2) | 5 (MIT) | 2 (fees, trust history) | 5 (MIT) | 3 | 4 | 5 | 5 | 5 | 5 |
| Genre precedents (×1) | 4 | 5 | 3 | 3 | 2 | 1 | 4 | 3 | 2 |

Weighted totals (weights sum to 23, so the maximum is 115): Godot 97, Unity 97, MonoGame 92, Bevy 82, Web 80, Heaps 75, Defold 73, LÖVE 69, GameMaker 66.

**The console path is a gate, not just a weight.** Options scoring 1 there (Bevy, LÖVE, Web) are disqualified by the "consoles later" requirement whatever their total. After applying the gate, the ranking is Godot ≈ Unity > MonoGame > Heaps ≈ Defold > GameMaker. Godot and Unity tie on paper. **Agent-friendliness, licensing and team-fit tip the balance to Godot**, unless online rollback co-op at launch is mandatory, in which case Quantum's turnkey determinism tips it to Unity.

**Ranking with reasoning, risks and mitigations**

1. **Godot 4.7 (.NET) + `Sim` (pure C#, fixed-point).**
   - *Why:*
     - MIT license with no fees.
     - Text scenes and resources plus a headless CLI suit agents.
     - Integer scaling and a 2D renderer with batching.
     - Proven on hordes (Halls of Torment), roguelites (Brotato) and C# at scale (Slay the Spire 2).
     - The team's TS/Go background maps well to C#.
   - *Risk:* C# on consoles is beta at W4.
     - Mitigation: keep the C# simple (no reflection-heavy libraries, no runtime codegen, NativeAOT/AOT-friendly).
     - Mitigation: buy W4 Starter for a console spike as soon as platform dev accounts are approved, or contract Pineapple Works.
     - Fallback: the sim library moves unchanged to Unity or MonoGame.
   - *Risk:* node overhead for hordes. Mitigation: render through MultiMesh/RenderingServer from sim arrays, with no per-enemy nodes and no physics bodies. Move to C++/Rust GDExtension only if profiling demands it.
   - *Risk:* engine churn (two minors a year) and LLM Godot-3 code. Mitigation: pin a minor, upgrade deliberately behind CI, and use AGENTS.md rules plus a lint.
   - *Risk:* no C# web demo. Mitigation: ship Steam demos only, or keep the existing TS prototype as a web teaser.
2. **Unity 6.3 LTS + the same `Sim` library**, or **Photon Quantum 3** if co-op is core.
   - *Why:* the best console pipeline and documentation; Burst for hot loops; Windblown proves Quantum for exactly this genre (deterministic, rollback, replays as a debugging tool).
   - *Risks:* $2,310/seat/yr Pro once you pass the revenue threshold or go to consoles; trust and pricing history; YAML + GUID merges; MCP needs a Unity Cloud subscription; ECS restructuring through 6.8; Quantum is a CCU-priced, Unity-centric vendor dependency.
   - *Mitigation:* stay on LTS; plain C# + Burst jobs instead of DOTS; Force Text + Smart Merge; prefabs generated from data.
3. **MonoGame 3.8.5 + the same `Sim` library.**
   - *Why:* entirely code and text, so the friendliest for agents; you control rendering and hordes completely; the XNA/MonoGame lineage shipped many console ports through Sickhead.
   - *Risks:* you build your own editor-less tooling (animation state, UI, particles, level tools); console access and process are less documented; 3.8.5's new native backend is fresh.
   - *Mitigation:* use the Blender pipeline and JSON content as the "editor", and ImGui-style debug tools.

Alternatives considered:
- **Godot + Rust sim (godot-rust 0.5) with wazero validation in Go.** Strongest determinism and Go reuse, but Rust GDExtension on consoles is undocumented (RAWRLAB has no GDExtension; W4 doesn't mention it) and the team doesn't know Rust.
- **Defold.** The cheapest consoles, but Lua and a native-extension sim split things awkwardly.
- **GameMaker GMRT.** Revisit once C#/TS and consoles leave beta.
- **Bevy.** No editor or consoles.
- **Heaps.** Dead Cells pedigree, but Haxe is niche for LLMs and console access is private.
- **LÖVE/web.** No console path.

**Proposed high-level architecture (engine-agnostic core):**
```
repo/
  sim/            Pure C# (.NET 8/9) class library, no engine refs
                  - fixed-point math (int32 Q16.16 / int64), LUT trig, int sqrt
                  - own PRNG (PCG32/xoshiro), one stream per subsystem
                  - fixed 60 Hz tick; Input = packed per-tick gamepad state per player
                  - SoA entity storage, pools, uniform-grid spatial hash, shared flow field
                  - content loaded from validated data (no engine types)
                  - per-tick state hash (e.g., xxHash64) for replays/desync detection
  sim.tests/      xUnit: unit + golden replays + cross-OS determinism (CI matrix win/linux/mac-arm64)
  sim.cli/        Headless runner: bot playthroughs for balance (port of aegis-draft's bot sims),
                  replay verifier, daily-seed tooling, perf benchmarks (ticks/sec @ 5k entities)
  game/           Godot 4.7 .NET project = presentation only
                  - reads sim snapshot each frame; interpolates between ticks
                  - MultiMeshInstance2D / RenderingServer batches per atlas; VFX/audio/UI/camera
                  - input → Steam Input (GodotSteam or Steamworks.NET) → sim input frames
                  - platform layer (Steam now; console services later) behind an interface
  content/        JSON/YAML defs + JSON Schema (single source of truth, as in aegis-draft),
                  sprite atlases + metadata from Blender pipeline (8/16 dirs, normal maps optional)
  tools/          Blender (Python) render scripts, atlas packer, schema validator, replay viewer
  server/         Go (chi + Postgres): dailies (seed+content version), run submissions,
                  leaderboards (own + Steam trusted writes via Web API), accounts/saves (optional)
  validator/      .NET worker container running sim.cli verify (same Sim.dll as the game)
CI: dotnet test → godot --headless --import → engine tests (gdUnit4/GUT) → exports (win/linux/mac)
    → determinism replays on 3 OS → artifact upload to Steam branch (SteamPipe) on tag
```

**Phased de-risking spikes (inference):**
1. A horde spike in Godot: 5k enemies plus 2k projectiles from a C# sim into MultiMesh, measured on Steam Deck. Kill criterion: under 8 ms per frame total.
2. A determinism spike: golden replays bit-identical on Windows x64, Linux x64 and macOS arm64, plus the .NET validator in Docker.
3. A Steam integration spike: Steam Input glyphs, Cloud, a trusted leaderboard write from Go, and replay UGC.
4. A console spike once platform accounts are approved: a W4 Starter license (or a porter quote) on Switch 2 or PS5 with the C# build.
5. A co-op decision: local co-op plus Remote Play Together for v1; lockstep or rollback over SDR only after the sim's snapshot and re-simulation cost is measured.

### Gaps
- No hands-on benchmarks: horde ceilings per engine on Deck and Switch 2 are inferred, not measured.
- C# on Godot consoles (W4 beta, Pineapple Works) has no public shipped-title evidence in my sources. This is the single biggest uncertainty for #1.
- Pricing for porting houses (Pineapple Works, Sickhead, Seaven) and for console devkits is confidential or not found.
- Not researched: Switch 1 viability, macOS notarization and signing for Godot exports, and SteamPipe CI specifics.
- The matrix weights are subjective and should be revisited if online co-op becomes a launch requirement, which would favour Unity + Quantum.
