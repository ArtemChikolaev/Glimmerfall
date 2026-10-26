# Dead Cells and side-scrolling action-roguelite references: production, systems, feel, live-ops, business (as of 2026-10-25)

_Method note: the shared WebSearch budget ran out midway, so later facts come from direct fetches of primary pages (Wikipedia, Steam, Game Developer, deepnight.net, wiki.gg, Steamcharts). Gamalytic and VG Insights (VG Insights now redirects to Sensor Tower) could not be read (JS pages / HTTP 403), so this file has **no third-party sales estimates**. The few rough sales ranges below are my own review-count inferences and are labelled as such. Everything is paraphrased; dates are given where the source has them._

## 1. Dead Cells facts: studios, team, structure, engine, timeline, end of development, sales, platforms, price, scores

### Takeaway
Dead Cells was made by a ~8–11-person worker co-op (Motion Twin, Bordeaux) on its own Haxe/Heaps/HashLink stack. It ran ~15 months in Early Access (May 2017 → Aug 2018), then got ~25 post-launch versions and 5 DLCs (1 free, 4 paid). From 2019 almost all of that was produced by the spin-off Evil Empire, which grew to 45–70 people. Sales reached **10M+ copies by June 2023**, the last official figure I found. New content was stopped by the IP owner (Motion Twin) in Feb 2024, with a final update on 19 Aug 2024. Officially, the game was presented as finished. Ex-lead designer Bénard reads it as a business and branding move ahead of Windblown.

### Cited Findings
**Studios, team, structure**
- Motion Twin was founded in 2001 in Bordeaux and has been a worker cooperative since 2004, with equal salary and equal decision-making power. Before Dead Cells it made browser games (Hordes/Die2Nite, My Brute, etc.; 10M registered users by 2009) and had little success on mobile. — [Wikipedia: Motion Twin](https://en.wikipedia.org/wiki/Motion_Twin)
- Bénard's GDC 2019 slides describe the co-op as same work hours, same say and same salary. Dead Cells was a a last-chance project with about 2.5 years of development. — [GDC 2019 slides, Bénard](https://media.gdcvault.com/gdc2019/presentations/Benard-Sebastian-DeepCells.pdf)
- A Japanese write-up of the same talk splits that time into about 1 year on the game and 1.5 years on feedback plus console versions. The studio was near bankruptcy after weak mobile results. — [Game Watch / Impress](https://game.watch.impress.co.jp/docs/news/1176126.html)
- The core team was kept at 8–10 people so the co-op stayed manageable. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- Kotaku counted 11 developers in 2018, and Filby described an 8–10-person ceiling of the co-op model. — [VICE, 30 Oct 2019](https://www.vice.com/en/article/3kxed3/the-ambitious-future-of-dead-cells-is-ditching-co-ops-for-capitalism)
- Motion Twin had 6 employees when Filby joined in 2014. — [Inverse, 25 Sep 2023](https://www.inverse.com/gaming/dead-cells-retrospective)
- Art team: Thomas Vasseur was the only artist for the first year (style, characters, monsters, animation, VFX, most backgrounds). His twin brother Gwenaël Massé joined later. — [Vasseur article, Habr translation, 10 Jul 2018](https://habr.com/post/416589)
- Origins: the project started as a free-to-play, multiplayer, mobile/web tower-defense sequel to Die2Nite/Hordes. The alpha wasn't fun, so the team kept a single-player prototype (on the advice of Shiro Games' head, Nicolas Cannasse) and cut multiplayer, mobile and F2P. — [MCV/Develop, 16 Nov 2018](https://mcvuk.com/development-news/when-we-made-dead-cells/); [PCGamesInsider, 2 May 2018](https://www.pcgamesinsider.biz/indie-interview/66988/how-motion-twin-stopped-dead-cells-being-just-another-roguelike-pixel-art-metroidvania-game/); [Inverse](https://www.inverse.com/gaming/dead-cells-retrospective)
- The game placed 2nd at the Big Indie Pitch in 2014. Combat design took about a year, to the end of 2015. About 50 weapons were built for the early version. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- Evil Empire was founded by Steve Filby, Ben Laulan, Joan Blachère and Thomas Pfeiffer, and began work on Dead Cells in Jan 2019. Motion Twin kept the IP and creative approval, and had fully stepped away from the game by summer 2019. — [Inverse](https://www.inverse.com/gaming/dead-cells-retrospective)
  - VICE (Oct 2019) instead says it was formed in August 2019. Evil Empire is deliberately not a co-op, so that it can grow past 8–10 people. It pays revenue shares at milestones and keeps a cash reserve. — [VICE](https://www.vice.com/en/article/3kxed3/the-ambitious-future-of-dead-cells-is-ditching-co-ops-for-capitalism)
  - The founding date conflicts between the two sources.
- Evil Empire's size over time:
  - It began as three people doing publishing for Dead Cells, tripled from 15 to 45 people in one year, and had about 60 people in May 2026. It has handled all Dead Cells live ops since 2019. — [Edge, May 2026](https://knowledge.edge-online.com/p/from-three-guys-publishing-dead-cells-to-60-people-making-the-next-castlevania-the-rise-of-evil-empi)
  - The Aug 2024 press release says 70 employees. — [Game Developer press release, 19 Aug 2024](https://www.gamedeveloper.com/press-release/the-last-deadly-traps-are-set-final-content-update-for-dead-cells-now-available)
  - Bénard says it hired about 50 people over roughly 5 years. — [deepnight.net, 12 Feb 2024](https://deepnight.net/blog/regarding-dead-cells-termination/)
- Per Bénard, by early 2024 Motion Twin had only about 3 people from the original Dead Cells team, plus a recent hire and an administrator. Alumni founded Shiro Games (Cannasse, 2012), Deepnight Games (Bénard, 2020) and PUNKCAKE (Soulé, 2021). — [deepnight.net](https://deepnight.net/blog/regarding-dead-cells-termination/); [Wikipedia: Motion Twin](https://en.wikipedia.org/wiki/Motion_Twin)

**Engine and tech: Haxe + Heaps + HashLink, and why**
- Bénard said Haxe was chosen because it was built to target many platforms and the team already knew it well. Motion Twin had created the language years earlier, and the game also uses the Heaps framework. — [MCV](https://mcvuk.com/development-news/when-we-made-dead-cells/)
- Haxe descends from MTASC, a compiler Motion Twin invented. — [Wikipedia: Motion Twin](https://en.wikipedia.org/wiki/Motion_Twin)
- Heaps is a GPU-oriented 2D/3D engine with four toolkits: h2d, h3d, hxd and hxsl (shaders). It targets HTML5, DirectX/OpenGL, mobile, Switch, PS4 and Xbox. The pitch: free with no revenue share, one codebase, compiles in seconds. It showcases Dead Cells, Northgard, Evoland and Darksburg. — [heaps.io](https://heaps.io/)
- The full Motion Twin/Shiro stack:
  - HashLink VM: JIT for daily development, HL/C (compile to C) for release builds.
  - The C output is what is used for console ports on PlayStation, Xbox and Switch. Heaps has HL/C renderers for NVN (Switch), GNM (PS4) and Xbox One.
  - Tools around it: the HIDE editor, CastleDB (a JSON-like database for designers), HScript, DomKit (UI) and HxBit (serialization and network sync).
  - — [Heaps FullStack wiki](https://github.com/HeapsIO/heaps/wiki/FullStack); [hashlink.haxe.org](https://hashlink.haxe.org/)
- Room tiles were authored in CastleDB. — [Bénard, level-design article](https://deepnight.net/tutorial/the-level-design-of-dead-cells-a-hybrid-approach/)
- Game data (outfits, biome difficulty, mobs) lives in JSON files. — [Dead Cells wiki: Development](https://deadcells.wiki.gg/wiki/Development)
- Gameplay logic runs in fixed 30 FPS loops while rendering runs at 60 FPS. — [Bénard, GameBase tutorial, 6 Mar 2020](https://deepnight.net/tutorial/using-my-gamebase-to-create-a-heaps-game/)

**Timeline, platforms, porting**
- Early Access launched 10 May 2017 on Windows, at an estimated 30–40% complete. GOG followed in Nov 2017 and macOS/Linux on 26 Jun 2018. Version 1.0 shipped 7 Aug 2018 on PC, PS4, Xbox One and Switch; Steam lists 6 Aug 2018. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells); [Steam](https://store.steampowered.com/app/588650/Dead_Cells/)
- Later platforms: iOS 28 Aug 2019 and Android 3 Jun 2020, both published by Playdigious; PS5 29 Jun 2023; a Netflix (subscription) mobile edition 31 Oct 2023. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- Early Access length:
  - Motion Twin promised about 12 months and took about 15. In hindsight Filby would budget 18 or more. — [PCGamesInsider, 13 Sep 2019](https://www.pcgamesinsider.biz/industry-icon/69669/what-motion-twin-learnt-from-making-dead-cells/)
  - MCV says the one-year plan was extended by 6 months for the console port. — [MCV](https://mcvuk.com/development-news/when-we-made-dead-cells/)
- Console ports:
  - Basic Xbox and Switch ports took about 1–2 months each. The main Switch problem was UI legibility. — [MCV](https://mcvuk.com/development-news/when-we-made-dead-cells/)
  - Filby said they badly underestimated a simultaneous console launch; certification forced planning about 6 months backwards. — [PCGamesInsider 2019](https://www.pcgamesinsider.biz/industry-icon/69669/what-motion-twin-learnt-from-making-dead-cells/)
- Version history: EA versions 0.0–0.9, then 1.0, then post-launch versions 1.1 (22 Dec 2018) through 3.4 "Clean Cut" (27 Apr 2023). Version 3.5 "The End is Near" came out 19 Aug 2024. — [Dead Cells wiki: Updates](https://deadcells.wiki.gg/wiki/Updates)
- June 2023 plans: the 10M announcement said Evil Empire would keep supporting the game through 2025. — [Game Developer, 5 Jun 2023](https://www.gamedeveloper.com/business/dead-cells-has-topped-10-million-sales-worldwide); [TechRadar](https://www.techradar.com/news/indie-banger-dead-cells-roars-past-10m-sales-support-and-updates-confirmed-until-2025)

**End of active development: when and why**
- On 9 Feb 2024 Motion Twin announced it would stop creating new Dead Cells content. — [Wikipedia: Motion Twin](https://en.wikipedia.org/wiki/Motion_Twin)
- Bénard (ex-lead designer, about 19 years at Motion Twin) wrote that Motion Twin cancelled updates Evil Empire had planned for 2024–25. — [deepnight.net, 12 Feb 2024](https://deepnight.net/blog/regarding-dead-cells-termination/)
  - He guesses the reasons were clearing the market for Windblown and unease that the press credited Evil Empire as the game's real authors. He calls this his own interpretation.
  - He criticizes Motion Twin's historic pattern of releasing, updating a little, then dropping.
- The final update 35, "The End is Near", shipped 19 Aug 2024. It added 3 mobs (Sore Loser, Curser, Doom Bringer), 2 curse-themed weapons (Misericorde, Anathema), 3 mutations and 40 heads. Motion Twin framed it as the moment the game was finished. — [Game Developer press release](https://www.gamedeveloper.com/press-release/the-last-deadly-traps-are-set-final-content-update-for-dead-cells-now-available)
- After that, only bug fixes and QoL hotfixes continue. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- The wiki notes that the balance of one aspect (Damned) reflects the rushed development of version 3.5. — [Dead Cells wiki: Aspects](https://deadcells.wiki.gg/wiki/Aspects)

**Sales milestones (official unless noted)**
- 730k during Early Access, as of about May 2018. — [Game Developer, 31 Aug 2018](https://www.gamedeveloper.com/business/tuning-i-dead-cells-i-to-appeal-to-players-both-fast-and-slow)
- 850k+ just before 1.0, with 94% positive of about 15k Steam reviews. — [Game Developer, 1 Aug 2018](https://www.gamedeveloper.com/design/how-player-criticism-helped-make-i-dead-cells-i-the-game-it-is-today)
- 1M+, about 60% on PC, per the GDC 2019 slide. — [GDC slides](https://media.gdcvault.com/gdc2019/presentations/Benard-Sebastian-DeepCells.pdf)
- Shortly after launch the game sold 4× faster on Switch than on PS4. — [MCV](https://mcvuk.com/development-news/when-we-made-dead-cells/)
- 2M by May 2019, then 5M by March 2021. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
  - PCGamesInsider reports "over 2M" as of Sept 2019 and notes that PC still led overall, but Switch was selling faster at the time. — [PCGamesInsider 2019](https://www.pcgamesinsider.biz/industry-icon/69669/what-motion-twin-learnt-from-making-dead-cells/)
- 10M+ in six years, announced 5 Jun 2023. — [Game Developer](https://www.gamedeveloper.com/business/dead-cells-has-topped-10-million-sales-worldwide)
- Bénard's reading of public data: Motion Twin drove the first ~1M units, Evil Empire the next ~9M. — [deepnight.net](https://deepnight.net/blog/regarding-dead-cells-termination/)
- Windblown's press kit still cites "10M+ players", so I found no newer official figure. — [Windblown press kit](https://windblown.game/Press)

**Price, scores, awards, transmedia**
- Steam base price is €24.99. — [Steam](https://store.steampowered.com/app/588650/Dead_Cells/)
- GOG's "Medley of Pain" bundle lists at €39.99. — [GOG](https://www.gog.com/en/game/dead_cells_medley_of_pain_bundle)
- Metacritic: PC 89, Switch 89, PS4 87, Xbox One 91, iOS 84. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- Steam (Oct 2026): 96% positive of 44,766 English reviews ("Overwhelmingly Positive"); 94% of 863 in the last 30 days. — [Steam](https://store.steampowered.com/app/588650/Dead_Cells/)
- Awards include TGA 2018 Best Action Game, Golden Joystick 2018 Best Indie, the NAVGTR control-design award, and Pégases 2020 Best Mobile Game and Best Game-as-a-Service. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- Animated series "Dead Cells: Immortalis" (Bobbypills) premiered 19 Jun 2024 on ADN, 10 episodes. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)

### Inferences
- **Two business phases.**
  - Phase 1: a small co-op of about 8–11 people with 15+ years on its own toolchain built a polished core and launched through Early Access.
  - Phase 2: a separately run and capitalized live-ops studio scaled content and ports and turned roughly 1M units into 10M+.
  - For a solo developer, phase 1 is the realistic template. The 10M tail came from years of live ops by a 45–70-person team.
- **Engine choice.** "Native" here meant owning the language, engine and compile-to-C toolchain, not a commercial engine. The payoff was cheap ports (1–2 months each) and long-term control. The cost was decades of in-house tooling expertise most teams don't have.
- **Why it ended.** Development ended for ownership and branding reasons, not falling engagement. Steam monthly averages were still about 2.1–3.5k in 2024 (see §5).
- **Revenue (rough, my own).** No official revenue was found. A blended net of US$5–10 per unit (discounts, regional pricing, mobile, platform cuts) would put 10M units at very roughly US$50–100M net over 2017–2023. This is an order-of-magnitude inference, not reported data.

### Gaps
- No source names an external console-porting partner. MCV implies the Xbox and Switch ports were in-house, and Playdigious is documented only for mobile.
- No official revenue, no platform split after 2019, and no official unit total after June 2023.
- Evil Empire's founding month conflicts: January 2019 (Inverse) vs August 2019 (VICE).
- A "3.5M copies" milestone appeared only as a PCGamesInsider headline; the page redirected and its date is unverified.
- One search-engine summary claimed 1M sales already in early 2018. That conflicts with 730k (May 2018) and 850k (Aug 2018), so treat it as wrong.

## 2. Production: the 3D→2D pixel pipeline, level design, biome graph, scaling content with a small team

### Takeaway
Two production systems carried Dead Cells:
- **3D→pixel animation.** Low-poly 3ds Max models are rendered tiny with no anti-aliasing by an in-house tool, with a per-frame normal map and a toon shader. Animation is pose-to-pose with VFX doing the motion work, and rigs and parts are reused heavily. A retake drops from days to under an hour.
- **Hybrid level generation.** A fixed hand-made world map holds hand-made, biome-specific room chunks, assembled per run by a per-biome "concept graph", with rule-based monster placement.

Backgrounds were grayscale paintings recolored with gradient maps. Most content scaling came from reuse and data-driven tools, then from Early Access feedback, then from Evil Empire's larger team.

### Cited Findings
**3D→2D pipeline (Thomas Vasseur, "Art Design Deep Dive", Game Developer, 25 Jan 2018)**
- Workflow:
  - Start from a simple 2D pixel-art model sheet.
  - Build a very low-poly model and skeleton in 3ds Max and export to FBX.
  - An in-house tool, "Cruncher", renders the mesh at a very small size with no anti-aliasing to get the pixel look.
  - Each frame is exported as a PNG plus a normal map, and a basic toon shader adds volume.
  - In-game characters are about 50 px tall, so detailed models weren't worth it.
  - — [Game Developer, Vasseur](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-); [Habr translation](https://habr.com/post/416589)
- Animation style:
  - Keyframed with the fewest keys that read well. Attacks are essentially pose-to-pose: interpolated frames are added before or after the key poses, not between them, and VFX supply the sense of motion and impact.
  - Animation runs at 30 FPS. The workflow was first proven on ScarKrow, a 72-hour Ludum Dare 32 prototype from 2015.
  - — [Game Developer, Vasseur](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)
- Why 3D:
  - Vasseur had little hand-animation experience; he had only started pixel art in 2013.
  - Flash hand animation for ScarKrow was far too slow, and retakes of hand-drawn work were unaffordable.
  - Inspirations were fighting games: KOF, BlazBlue, Guilty Gear.
  - For Dead Cells they added an automatic lighting/shadow system and a pixelation tool.
  - — [Habr translation](https://habr.com/post/416589); [Game Developer, Vasseur](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)
- Time savings:
  - Reusing model parts across new monsters was the single most useful trick and saved hundreds of hours. Weapons and armor attach to existing models, and one animation can drive several models.
  - Dozens of timing tweaks for a new weapon take only minutes; two retakes took less time than uploading the GIFs for the article.
  - — [Game Developer, Vasseur](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)
- Filby (2017): animations are built in 3D with pixel art rendered from the models and key frames picked on export. A tweak reaches the game in 30–60 minutes, versus 3–4 days to re-animate by hand. — [Game Developer, 7 Jul 2017](https://www.gamedeveloper.com/design/designing-each-of-the-50-weapons-in-i-dead-cells-i-to-feel-distinctive)
- Drawbacks: flickering pixels remained unsolved, since cleaning them by hand would kill the speed advantage. Detail is lower than hand-drawn art, which the team accepted to prioritize animation. — [Game Developer, Vasseur](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)

**Backgrounds and environments (Gwenaël Massé, Game Developer, 3 Apr 2018)**
- Painting and tooling:
  - Parallax layers are painted in grayscale and colored with gradient maps; swapping the map re-themes a biome quickly.
  - A homemade tool tunes per-level lighting, shaders, smoke, water, mist and vegetation density.
  - Normal maps for backgrounds were hand-drawn in Photoshop because plug-ins looked flat or embossed, and they cost the artists many weekends of work.
- Readability hierarchy:
  - Paths and collisions get the strongest contrast against the background.
  - Interactive elements get mid contrast.
  - Enemies, projectiles and spells get the highest saturation, contrast and brightness.
- Procedural support: backgrounds are cut into sections with decor rules (for example, torches near alcoves and doors) so they work with generated levels. Two artists handled backgrounds.
- — [Game Developer, Massé](https://www.gamedeveloper.com/production/art-design-deep-dive-giving-back-colors-to-cryptic-worlds-in-i-dead-cells-i-)

**Level design: the hybrid approach (Sébastien Bénard; Gamasutra Mar 2017, republished on deepnight.net 15 Feb 2020)**
- Why hybrid: a handmade-levels prototype was dropped because the team was too small to finish it. Fully procedural levels felt chaotic and inconsistent. The middle way borrowed from Spelunky, FTL, and Left 4 Dead's AI Director (pacing with peaks and rests). — [deepnight.net](https://deepnight.net/tutorial/the-level-design-of-dead-cells-a-hybrid-approach/)
- The six steps — [deepnight.net](https://deepnight.net/tutorial/the-level-design-of-dead-cells-a-hybrid-approach/):
  1. A fixed, hand-designed world frame: island map, biome interconnections and key locations, the same on every seed.
  2. Hand-made room tiles with a purpose (combat, treasure, merchant, …), varied by number of entrances/exits. Tiles are biome-specific; for example, the sewers are tight and limit jumping and dodging. They were authored in CastleDB.
  3. A per-biome concept graph: entrance and exit, then special rooms, then combat and exploration nodes. It sets level length, number of special rooms, how labyrinthine the biome is, and the distance from the entrance to the nearest exit.
  4. Room picking by brute-force constraint fitting.
  5. Monster budget by combat-tile length, with per-type rules. Example: 250 combat tiles at 1 mob per 5 tiles gives 50 mobs; a dangerous mob counts as 10 tiles; mobs don't share a platform and some need open space.
  6. Loot by a recipe the article doesn't disclose.
- In a 2022 comment, Bénard said the room algorithm was later reworked toward a Wave-Function-Collapse-like approach because the Switch port was tricky. — [deepnight.net](https://deepnight.net/tutorial/the-level-design-of-dead-cells-a-hybrid-approach/)
- Filby put the mix at about 50:50 random and curated. Each chunk was hand-designed so its jumps are known to be possible, and fully random monster placement had produced unfair combinations. — [PCGamesInsider, 2018](https://www.pcgamesinsider.biz/indie-interview/66988/how-motion-twin-stopped-dead-cells-being-just-another-roguelike-pixel-art-metroidvania-game/)
- Bénard called level design the biggest challenge of a "roguevania". Runes, permanent abilities that open blocked paths, are how a procedural world still signals what you can and can't reach yet. — [MCV](https://mcvuk.com/development-news/when-we-made-dead-cells/)

**Biome graph and interconnection**
- There are **36 biomes**. The base game has 15 main and 4 boss biomes; DLC adds 9 main and 8 boss biomes. They run in tiers from Prisoners' Quarters to Throne Room, Lighthouse, Master's Keep and the 5-BSC Astrolab and Observatory, with branches at every tier. — [Dead Cells wiki: Biomes](https://deadcells.wiki.gg/wiki/Biomes)
- Access to branches is gated by runes (Vine, Teleportation, Ram, Spider, Homunculus), by Boss Stem Cell level (for example Cavern and Guardian's Haven need 2+ BSC and the Giant beaten; Astrolab needs 5 BSC) and by keys. — [Dead Cells wiki: Biomes](https://deadcells.wiki.gg/wiki/Biomes)
- Biome transitions are hand-designed (a mandatory elevator, a long tunnel) so the sequence of biomes stays believable. — [Game Developer, Massé](https://www.gamedeveloper.com/production/art-design-deep-dive-giving-back-colors-to-cryptic-worlds-in-i-dead-cells-i-)

**Scaling content with a small team**
- 40–50% of all Early Access changes came from player feedback. The final character-build system was about 90% inspired by player ideas, and the original Forge (upgrading specific weapons) was reworked after objections. — [Game Developer, 1 Aug 2018](https://www.gamedeveloper.com/design/how-player-criticism-helped-make-i-dead-cells-i-the-game-it-is-today)
- Before Early Access the team benchmarked the top 10 Steam roguelikes and top 10 metroidvanias. They launched a polished core with about half the final game missing, and coined "roguevania" to stand out from the crowd of pixel-art roguelikes. — [PCGamesInsider, 2018](https://www.pcgamesinsider.biz/indie-interview/66988/how-motion-twin-stopped-dead-cells-being-just-another-roguelike-pixel-art-metroidvania-game/)
- Post-launch content (DLCs, crossovers) was produced by Evil Empire. For Return to Castlevania, Konami's team visited Bordeaux to review the build. — [Inverse](https://www.inverse.com/gaming/dead-cells-retrospective); [Edge](https://knowledge.edge-online.com/p/from-three-guys-publishing-dead-cells-to-60-people-making-the-next-castlevania-the-rise-of-evil-empi)

### Inferences
- **The owner's pipeline already matches the Dead Cells recipe.** The differences that matter:
  - render a single side view;
  - target roughly 40–60 px characters;
  - output a normal map with every frame for toon/dynamic lighting;
  - animate pose-to-pose with VFX smears instead of smooth in-betweens;
  - build a modular parts and rig library to reuse across enemies.
  The biggest savings came from reuse and fast retiming, not from rendering itself.
- **Background normal maps.** They were a manual pain point at Motion Twin. Generating background normals procedurally (from 3D blockouts or height maps) is where a modern solo pipeline could beat 2017 practice.
- **Level design as a content multiplier.** Per-biome tile pools plus a graph template multiply content cheaply, but every tile still has to be traversal-validated by hand. That is a cost a top-down arena game largely avoids (see §8). Bénard's later open-source level editor, LDtk, descends from this way of working; it is linked from his devlog pages but I did not verify it in detail.

### Gaps
- No published counts of Beheaded or enemy animations or frames, and no per-enemy production time.
- Mirroring and flipping are not mentioned in Vasseur's article, though it is standard for side-scrollers.
- Cruncher's exact features (palette reduction, outlines) are not described.
- No source gives room/tile counts per biome, and none confirms that the normal maps drive dynamic in-engine lights (a lighting system is mentioned).

## 3. Systems: meta-progression, stats, counts, BSC ladder, runes, curse, modes, cosmetics, crossovers, paid DLCs

### Takeaway
Dead Cells layers three things:
- **Permanent unlocks**: cells plus blueprints, spent at the Collector between biomes.
- **Per-run builds**: two weapons, two skills, an amulet and 3 mutations, scaled by three color stats with +15% compounding DPS per point.
- **A 6-step difficulty ladder** (0–5 Boss Stem Cells) that removes healing, adds enemies and biomes, and ends with Malaise.

Around that sit a fixed world graph gated by runes, risk mechanics (Curse, cursed biomes), side modes (Daily, Custom, Boss Rush, Aspects, Speedrun) and a large cosmetics and crossover catalog. Content at the end of support: about 125 weapons, 56 mutations, 96 enemy types, 36 biomes and roughly 13 boss fights.

### Cited Findings
**Permanent progression and stats**
- Meta-progression — [Dead Cells wiki: The Collector](https://deadcells.wiki.gg/wiki/The_Collector):
  - Cells taken from enemies are given to the Collector in the Passages between biomes.
  - Blueprints (from drops, lore rooms and so on) are traded for permanent unlocks: general upgrades and runes, mutations, weapons and skills.
  - Outfit blueprints go to the Tailor.
  - At 5 BSC the Collector's Intern replaces him.
- Stats: Brutality, Tactics and Survival each start at 1 per run. — [Dead Cells wiki: Stats](https://deadcells.wiki.gg/wiki/Stats)
  - Points come from Scrolls of Power (+1), dual-color scrolls, three-color scrolls, amulets, and four scroll fragments (from BSC 3).
  - Items of a color gain +15% DPS per point, compounding (Base × 1.15^(stat−1)).
  - Each stat also multiplies max HP with diminishing returns; Survival gives the most HP, Tactics the least.
  - Colorless items scale off your highest stat.
- Mutations: 56 in total (12 Brutality, 12 Tactics, 12 Survival, 20 colorless). — [Dead Cells wiki: Mutations](https://deadcells.wiki.gg/wiki/Mutations)
  - You gain one mutation slot per cleared area, up to 3.
  - Resetting costs gold, starting at 1,000 and doubling up to 8,000.
  - Version 3.5 added 3 colorless curse-themed mutations.

**Content counts**
- Weapons: the wiki tables list 125 entries (69 melee, 41 ranged, 15 shields). Of these, 92 are base game; DLCs add Rise of the Giant 8, The Bad Seed 4, Fatal Falls 3, The Queen and the Sea 8, Return to Castlevania 10. — [Dead Cells wiki: Weapons](https://deadcells.wiki.gg/wiki/Weapons)
- Official store copy:
  - The base game has about 150 weapons and skills, 17 levels and 5 bosses.
  - The Bad Seed, Fatal Falls and The Queen and the Sea together add 8 levels, 5 bosses (one only at top difficulty), 21 enemies, 34 weapons/skills and 52 outfits.
  - Return to Castlevania adds 14 weapons, 2 levels, 3 boss fights, 20 outfits and 51 classic plus 12 remixed tracks.
  - — [GOG Medley of Pain bundle](https://www.gog.com/en/game/dead_cells_medley_of_pain_bundle)
- Skills: the wiki page was truncated, so the count is partial: 13 deployables, at least 10 grenades, plus powers. — [Dead Cells wiki: Skills](https://deadcells.wiki.gg/wiki/Skills)
- Enemies: 96 types, 63 base and 33 DLC (Rise of the Giant 7, The Bad Seed 5, Fatal Falls 8, The Queen and the Sea 2, Return to Castlevania 11). — [Dead Cells wiki: Enemies](https://deadcells.wiki.gg/wiki/Enemies)
  - Some appear only at higher BSC: Knife Thrower 1+, Oven Knight 2+, Rampager 3+, Failed Experiment 4+.
  - Cursers, Doom Bringers and Sore Losers appear only in cursed biomes (2+ BSC).
- Biomes: 36 (see §2). — [Dead Cells wiki: Biomes](https://deadcells.wiki.gg/wiki/Biomes)

**Affixes, quality, legendaries**
- Gear power (level plus quality) sets affix slots, from 0 up to 6. Quality tiers are +, ++, S and L (legendary). — [Dead Cells wiki: Weapons](https://deadcells.wiki.gg/wiki/Weapons)
- Legendaries:
  - Drop rates are 1% from common enemies and 6% from elites; they also come from altars and from bosses beaten without taking a hit.
  - Altar chance is 2% per biome, plus 15% for each biome without one.
  - Each legendary has a fixed legendary affix, is colorless, scales off your highest stat, and can't be upgraded.
  - Quality is raised at the Legendary Forge, whose caps rise with BSC.
  - — [Dead Cells wiki: Weapons](https://deadcells.wiki.gg/wiki/Weapons)

**Boss Stem Cells (difficulty 0–5)** — [Dead Cells wiki: Boss Stem Cells](https://deadcells.wiki.gg/wiki/Boss_Stem_Cells)
- At every level from 1 BSC up, the bosses' early phases change.
- **1 BSC:** Health Fountains in only every other Passage (the rest give a flask recharge). The Forge cap rises so ++ items reach 100%. New doors open to Ancient Sewers and Ramparts.
- **2 BSC:** no fountains, one flask charge per Passage. S-quality cap 25%. Enemy cell drops doubled. Cavern and Guardian's Haven doors open.
- **3 BSC:** only 3 flask refills per run. S cap 50%. Doubled cells. Scroll fragments appear. Item levels +1. Cursed-chest colorless items upgraded to S. Insufferable Crypt opens from Ramparts.
- **4 BSC:** most mobs get larger detection range and can teleport to the player. No flask refills. S cap 100%. Tripled cells. More fragments. Item levels +3. The 5th cell is obtained from the Giant.
- **5 BSC:** adds **Malaise**, which moved here from 4 BSC in version 2.3. The Astrolab and Observatory open, and the Collector's Intern replaces the Collector.

**Runes as metroidvania gating**
- Vine, Teleportation, Ram and Spider runes open specific biome routes. The Homunculus rune opens the Cavern Key door and is also found in the Throne Room. Explorer's and Challenger's runes are also rewards. — [Dead Cells wiki: Biomes](https://deadcells.wiki.gg/wiki/Biomes)
- Runes are the solution to "backtracking vs procedural" in a roguevania. — [MCV](https://mcvuk.com/development-news/when-we-made-dead-cells/)

**Curse**
- Curse is a counter: almost any damage kills you while it's active, and each kill removes one point. — [Dead Cells wiki: Curse](https://deadcells.wiki.gg/wiki/Curse)
  - Sources: cursed chests (+10), golden doors and special doors (+5 to +50), corrupted artifacts (+20), Cursed Flask, Cursed Sword, and the 3.5 mobs and weapons.
  - From 2 BSC, "cursed biomes" appear: up to 8 per run at 5 BSC, a 25% chance per exit, about 9 cursed enemies each, and gear one level higher.

**Modes and side systems (version / date)** — [Dead Cells wiki: Updates](https://deadcells.wiki.gg/wiki/Updates); [Dead Cells wiki: Aspects](https://deadcells.wiki.gg/wiki/Aspects)
- Daily Run: version 0.3, 17 Aug 2017.
- Custom Mode: 1.1, 22 Dec 2018.
- Malaise update: 2.1, Dec 2020.
- Training room and **Aspects**: 2.5, 16 Sep 2021. There are 13 aspects; you pick one per run, and doing so disables BSC progress and flawless rewards.
- The Bank: 2.8, Mar 2022.
- Accessibility update: 2.9, Jun 2022.
- **Boss Rush**: 3.1, 5 Oct 2022. A DIY Boss Rush followed in 3.4.
- **Speedrun Mode**: 3.4 "Clean Cut", 27 Apr 2023.
- Twitch integration / Streamer Mode shipped with 1.0. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells); [Inverse](https://www.inverse.com/gaming/dead-cells-retrospective)

**Outfits and cosmetics**
- How outfits unlock: enemy or boss drops (often gated by BSC), cells at the Collector, lore and secret rooms, achievements and challenges, Boss Rush, and DLC. A few are free (Classic, Galaxy, Baguette, Retro, Winter, Reverse Burglar). Collecting 16 outfits unlocks Sewing Scissors and 51 unlocks Giant Comb. — [Dead Cells wiki: Outfits](https://deadcells.wiki.gg/wiki/Outfits)
- Paid DLC added 52 + 20 outfits. — [GOG](https://www.gog.com/en/game/dead_cells_medley_of_pain_bundle)
- Update 35 added 40 heads. — [Game Developer press release](https://www.gamedeveloper.com/press-release/the-last-deadly-traps-are-set-final-content-update-for-dead-cells-now-available)

**Crossovers ("Everyone Is Here" and others)**
- Everyone Is Here, version 2.6 (22 Nov 2021): Hyper Light Drifter, Curse of the Dead Gods, Guacamelee, Skul, Blasphemous, Hollow Knight. — [Dead Cells wiki: Updates](https://deadcells.wiki.gg/wiki/Updates)
- Everyone Is Here Vol. 2, version 3.2 (7 Nov 2022): Terraria, Shovel Knight, Hotline Miami, Katana Zero, Slay the Spire, Risk of Rain. — [Dead Cells wiki: Updates](https://deadcells.wiki.gg/wiki/Updates)
- Version 3.0 also added Soul Knight content. — [Dead Cells wiki: Updates](https://deadcells.wiki.gg/wiki/Updates)
- Outfits also reference Dragon Ball, TMNT, FFVII, Street Fighter, Half-Life, Fallout, Aladdin and GOG Galaxy. — [Dead Cells wiki: Outfits](https://deadcells.wiki.gg/wiki/Outfits)
- Outbound crossovers: Dead Cells content was added to Roboquest (Aug 2024) and BlazBlue Entropy Effect (Sep 2025). — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- How the Castlevania deal grew:
  - Evil Empire first proposed an Alucard skin or a rapier, then mocked up Castlevania levels in Dead Cells style and pitched a full DLC.
  - Konami gave it carte blanche, then asked Evil Empire to make the next Castlevania, which became Belmont's Curse (Oct 2026).
  - — [Edge, May 2026](https://knowledge.edge-online.com/p/from-three-guys-publishing-dead-cells-to-60-people-making-the-next-castlevania-the-rise-of-evil-empi)
- **Marketing effect in Steam concurrents** (monthly peaks) — [Steamcharts](https://steamcharts.com/app/588650):
  - Everyone Is Here: Nov 2021 peak 8,530, vs 4,579 in Oct 2021.
  - Everyone Is Here Vol. 2: Nov 2022 peak 8,619, vs 3,609 in Oct 2022.
  - Return to Castlevania: Mar 2023 peak **14,880**, the all-time high.

**Paid DLCs (price / date / content)** — [Steam](https://store.steampowered.com/app/588650/Dead_Cells/); [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)

| DLC | Price | Release | Content |
|---|---|---|---|
| Rise of the Giant | free | Mar 2019 | — |
| The Bad Seed | €4.99 | 11 Feb 2020 | 2 biomes + boss |
| Fatal Falls | €4.99 | 26 Jan 2021 | levels, weapons, boss |
| The Queen and the Sea | €4.99 | 6–7 Jan 2022 | 3 levels + alternate ending |
| Return to Castlevania | €9.99 | 6 Mar 2023 | Konami licence, Richter |

- Each paid DLC month is a visible concurrency spike, which serves as a reception proxy: Feb 2021 peak 13,651; Jan 2022 average 7,021 (the highest monthly average on record) and peak 13,556; Mar 2023 peak 14,880. — [Steamcharts](https://steamcharts.com/app/588650)

### Inferences
- **What to copy for an original game:** the **stat-color system plus mutation slots** (simple to read, deep through synergies), the **BSC ladder** (a long tail from one map), **runes as a world-graph key**, and cheap side modes (Daily, Custom, Boss Rush, Speedrun) that reuse existing content.
- **Crossovers.** Indie-to-indie crossovers were cheap cosmetic and weapon reskins that roughly doubled monthly peak concurrents. The licensed DLC beat every paid expansion. For an original IP, reciprocal indie crossovers are realistic, while big licences come after a hit.
- **Price ladder.** €4.99 DLCs on a €24.99 base, plus free updates, is a proven pattern. The €9.99 licensed DLC performed best.

### Gaps
- No total outfit count; the wiki page was too long to tally fully. Memory suggests 200+, but that is unverified.
- Skill count is incomplete, and per-DLC review scores were not retrieved.
- The details of the Malaise mechanic and the 3.5 changes to the Curse were not covered by the pages I fetched.

## 4. Combat feel: what makes it feel great (developer commentary)

### Takeaway
Motion Twin's own explanation centers on three things:
- **Controls that are never at fault**, tuned first, before any challenge was designed: just-in-time jumps, ledge forgiveness that fixes landings by hopping, teleporting or climbing, double jump from the start, free turning during attacks, and auto-aim.
- **Fighting-game "juice"**: 1-frame freezes on crits, short slow-downs, particles and blood sprays, distinct impact sounds, and an extra frame or two of anticipation for weight.
- **Fast, readable fights**: a "two-second combat" rule, deaths that are always understandable, and difficulty that is hard but fair.

The technical base underneath is a 30 FPS fixed gameplay tick with 60 FPS rendering. Bénard's GameBase advice is to read input at render rate.

### Cited Findings
- Bénard's GDC 2019 talk ("Dead Cells: what the f*n?!") — [GDC 2019 slides](https://media.gdcvault.com/gdc2019/presentations/Benard-Sebastian-DeepCells.pdf):
  - Pillars: combat, progression, replayability.
  - Because of permadeath, the game must never be blamed for poor controls.
  - Control tricks: "just-in-time" jumps; fixing your arrival by teleporting, hopping or climbing; trying to read the player's intention; double jump from the start; traversal without helpers.
  - Combat tricks: free turn-around and auto-aim.
  - Most reviews talked about controls before gameplay.
  - Closing advice: polish the details that aren't part of the challenge, and make it the way you remember old games, not how they really were.
- Talk write-up — [Game Watch / Impress](https://game.watch.impress.co.jp/docs/news/1176126.html):
  - Ledge leniency: jumps and landings that are a few pixels short still succeed. Without it the character needed repeated jumps or died just short of the edge.
  - Control feel was finished first, and the challenge was built on top. Challenge belongs to enemies and terrain, not to input execution.
- Juice and impact (Bénard and Filby, 2017) — [Game Developer, 7 Jul 2017](https://www.gamedeveloper.com/design/designing-each-of-the-50-weapons-in-i-dead-cells-i-to-feel-distinctive):
  - Fighting-game references: SF4, BlazBlue, Mark of the Wolves. The team uses many particles, stop frames and slowdowns.
  - A critical hit freezes the game for 1 frame, then slows it for a few tenths of a second, with a blood spray and its own impact sound.
  - Weight: the animator adds a frame or two so an effect lasts about 0.2 s longer. Designers ask for a pause at the top of the backswing and test it in game.
  - Weapons get conditional crits (a backstab dagger; a sword that always crits below 50% HP) and are designed to combine with your secondary weapon.
- Fairness and pace — [Game Developer, 31 Aug 2018](https://www.gamedeveloper.com/business/tuning-i-dead-cells-i-to-appeal-to-players-both-fast-and-slow); [Game Informer, 30 Dec 2018](https://gameinformer.com/interview/2018/12/30/dead-cells-designer-discusses-scrapped-ideas-roguelikes-and-the-potential-for); [MCV](https://mcvuk.com/development-news/when-we-made-dead-cells/):
  - A strict "two-second combat" rule, because there are dozens of enemies per level.
  - If a move isn't meant to be a challenge, the player should succeed.
  - Players must understand why they failed.
  - The goal was a game that is hard but fair, with Dark Souls as the fairness reference.
- Ranged balance came from watching players. Bows with infinite ammo let people camp, so ammo returns only after a kill, with further boss-specific tweaks. — [Game Developer, 27 Oct 2017](https://www.gamedeveloper.com/design/how-watching-people-play-i-dead-cells-i-incorrectly-influenced-its-designers)
- The dodge roll replaced an earlier dash after the artists pushed for it in a co-op vote. — [Inverse](https://www.inverse.com/gaming/dead-cells-retrospective)
- Gameplay ticks at 30 FPS fixed while rendering runs at 60 FPS. Input should be polled in the real-FPS update so presses between ticks aren't lost. — [deepnight GameBase tutorial](https://deepnight.net/tutorial/using-my-gamebase-to-create-a-heaps-game/)
- Bénard's "Game feel demo" (v2, May 2024) is a playable toggle demo showing how small details change feel, with source code. Reddit commenters list screenshake, hit-pause and coyote time among the toggles, but the page itself doesn't list values. — [deepnight.net game-feel](https://deepnight.net/games/game-feel/)
- Motion Twin's later thinking, in Windblown:
  - The dash has no cooldown because that improved pacing. "Alterattacks" reward switching weapons, unlike Dead Cells. Weapon depth draws on Monster Hunter. — [Wikipedia: Windblown](https://en.wikipedia.org/wiki/Windblown)
  - The Oct 2026 update added a dynamic warning system that previews enemy attacks, adjusted attack timings, and an auto-combo accessibility option. — [Rogueliker](https://rogueliker.com/windblown-last-update-before-launch/); [Capsule Computers](https://www.capsulecomputers.com.au/2026/10/one-last-leap-windblown-unleashes-final-early-access-update-before-1-0-launch/)
- Bénard's post-Dead Cells roguelite Tenjutsu has "Master the Feel" as a core pillar and aims to push Dead Cells' combat legacy forward. — [deepnight Tenjutsu devlog 2](https://deepnight.net/blog/tenjutsu-devlog-2-from-beat-em-up-to-roguelite/)

### Inferences
- **For a side-scroller, "feel" is mostly invisible assists.** Coyote/just-in-time jumps, ledge correction, auto-aim, turning mid-attack and input polling at render rate do most of the work. Those have to come before content, and they cost tuning time rather than art.
- **The juice is cheap to implement and data-drivable.** A 1-frame freeze, short slow-mo, particles and sound are cheap, and the extra anticipation frames come almost free from the 3D pipeline's retiming.
- **Telegraphs.** Windblown's 2026 telegraph system suggests that readable enemy tells were a problem in their newer, 3D, co-op, top-down game. Read: top-down plus co-op raises readability costs.

### Gaps
- No primary source gives Dead Cells' input-buffer window, coyote-time frames, roll invulnerability frames, animation-cancel rules or screen-shake values. The GDC 2019 video, not the slides, may contain them.
- No primary statement on Dead Cells enemy telegraph design rules was found.

## 5. Retention evidence: update cadence, player counts, community, mods, speedrunning

### Takeaway
About **36 versions over 7 years**: 11 in Early Access, then about 25 after 1.0, roughly every 2–3 months from Dec 2018 to Apr 2023, then one final update in Aug 2024. On top of that, 5 DLCs, several crossovers and many modes. This kept a baseline of roughly **1.1–3.5k Steam monthly-average concurrents from 2019 through 2026**, with monthly peaks of 8.5–14.9k on DLC and crossover months. Monthly averages were still about 1.8–2.6k in Jan–Sep 2026, two years after development stopped. Modding is limited but present, and Speedrun Mode is official since 2023.

### Cited Findings
- Version count:
  - The wiki lists versions 0.0–0.9 plus 1.0 for Early Access, and 1.1–3.5 after launch, from 22 Dec 2018 to 19 Aug 2024. — [Dead Cells wiki: Updates](https://deadcells.wiki.gg/wiki/Updates)
  - Game Developer (June 2023) counts 34 free updates and 4 premium DLCs. — [Game Developer](https://www.gamedeveloper.com/business/dead-cells-has-topped-10-million-sales-worldwide)
  - Bénard credits Evil Empire with 22 major updates and DLCs over about 5 years. — [deepnight.net](https://deepnight.net/blog/regarding-dead-cells-termination/)
- Steam concurrents by month (Steamcharts) — [Steamcharts](https://steamcharts.com/app/588650):
  - Early Access launch, May 2017: average 2,561, peak 6,712. Low point Oct 2017: average 447.
  - 1.0, Aug 2018: average 4,438, peak 10,344.
  - The Bad Seed, Feb 2020: peak 9,717.
  - Fatal Falls, Feb 2021: average 6,182, peak 13,651.
  - The Queen and the Sea, Jan 2022: average 7,021, peak 13,556.
  - Return to Castlevania, Mar 2023: average 6,235, peak 14,880 (all-time).
  - Final update, Aug 2024: average 2,618, peak 5,914.
  - 2025 monthly averages: 1,925–2,958.
  - Jan–Sep 2026 monthly averages: 1,802–2,568; Sep 2026 peak 5,857.
- Inverse (Sept 2023) reports peak daily users of 98k at the 2018 launch and 125k after Rise of the Giant; average daily Steam users went from 25k to 50k. It also says Dead Cells was the 6th most-played game on Steam Deck. These are daily-user figures, not concurrents. — [Inverse](https://www.inverse.com/gaming/dead-cells-retrospective)
- Third-party "all-time peak" trackers disagree (Steambase 7,858 on 3 Mar 2024; Raijin 6,062; Levvvel 14,190). I used the Steamcharts monthly series. — [Steambase](https://steambase.io/apps/dead-cells); [Levvvel](https://levvvel.com/statistics/dead-cells/)
- Community loop:
  - Patch notes highlighted community suggestions, with the advice to act as a showrunner during Early Access. — [GDC slides](https://media.gdcvault.com/gdc2019/presentations/Benard-Sebastian-DeepCells.pdf)
  - 40–50% of Early Access changes came from feedback. — [Game Developer](https://www.gamedeveloper.com/design/how-player-criticism-helped-make-i-dead-cells-i-the-game-it-is-today)
- Mods:
  - Language mods came in 0.1, basic, partial mod support in 0.9 (Jun 2018), and improvements in 2.4. — [Dead Cells wiki: Updates](https://deadcells.wiki.gg/wiki/Updates); [ModDB](https://www.moddb.com/news/mod-support-is-coming-to-indie-roguelike-dead-cells)
  - Steam Workshop items are kept compatible through the 35.x patches. — [Steam Workshop changelog](https://steamcommunity.com/sharedfiles/filedetails/changelog/1528468610)
- Speedrunning: Speedrun Mode became official in 3.4 (Apr 2023). Timed doors with rewards appear in normal runs and are listed as blueprint sources. — [Dead Cells wiki: Updates](https://deadcells.wiki.gg/wiki/Updates); [Dead Cells wiki: Weapons](https://deadcells.wiki.gg/wiki/Weapons)
- Evil Empire now applies the "Dead Cells model" to other hits, for example running Brotato's live ops (Brotato has 10M+ sales). The pitch is that a hit still has thousands of active players to serve with new content. — [Edge](https://knowledge.edge-online.com/p/from-three-guys-publishing-dead-cells-to-60-people-making-the-next-castlevania-the-rise-of-evil-empi)

### Inferences
- The engagement floor rose only modestly, from about 1.1–1.5k monthly-average concurrents at the 2019 lows to about 1.8–2.0k at the 2025–26 lows. Updates mostly created spikes rather than lasting floor growth.
- The floor was sustained by the evergreen loop (BSC ladder, builds, Daily Run) plus deep discounts. The free-update cadence mainly generated news beats and sales spikes; this is my reading of the curves together with the 50% discounts seen on Steam.
- For a solo developer: a 2–3-month content cadence was sustained by a 15–70-person studio. A solo developer should instead plan fewer, larger beats and build modes that reuse content.

### Gaps
- No speedrun.com statistics were retrieved, and no Steam Workshop item counts.
- No console or mobile player counts.
- No official retention or daily-user data after 2023.

## 6. Windblown (Motion Twin, Early Access 2024): what changed vs Dead Cells, and its status in Oct 2026

### Takeaway
Windblown is a **fully 3D, top-down/isometric**, 1–3-player online co-op action roguelite. It entered Early Access on 24 Oct 2024. On 22 Oct 2026 it received its 10th and final Early Access content update, "0.9999999"; 1.0 is still undated. Motion Twin consciously changed from Dead Cells: co-op, a dash with no cooldown, Alterattacks on weapon swap, and Monster Hunter-style weapon depth. It kept Dead Cells-style unlocks.

Reviews are strong (92% positive), but Steam concurrency fell from a 6.9k all-time peak to roughly 100–185 monthly average in 2026. Commercially it has not repeated Dead Cells' trajectory, based on player data; no sales figures are public.

### Cited Findings
- Facts: developer Motion Twin, co-published with Kepler Ghost, director Yannick Elahee. Announced at TGA on 7 Dec 2023. Closed alpha with about 300 users in Feb 2024. Early Access on Steam (Windows) on 24 Oct 2024. — [Wikipedia: Windblown](https://en.wikipedia.org/wiki/Windblown)
- Perspective and co-op:
  - The press kit describes a "fully 3D world" of floating islands and online co-op for up to 3 players. — [Windblown press kit](https://windblown.game/Press)
  - PCGamesN calls it a 3D top-down action roguelike. — [PCGamesN guide](https://www.pcgamesn.com/windblown/guide)
  - Wikipedia's Motion Twin page describes it as a fast-paced isometric action game. — [Wikipedia: Motion Twin](https://en.wikipedia.org/wiki/Motion_Twin)
  - Steam lists it as 1–3-player co-op, Windows only. — [Steam](https://store.steampowered.com/app/1911610/Windblown/)
- The game received support from FAJV, a fund financed by the CNC. — [press kit](https://windblown.game/Press)
- What changed vs Dead Cells, and why — [Wikipedia: Windblown](https://en.wikipedia.org/wiki/Windblown):
  - The team first wanted to leave the roguelike genre, but built on what Dead Cells taught them.
  - Co-op was a key goal.
  - Islands are placed in sight and reached by dashing; removing the dash cooldown improved pacing.
  - They tried Isaac- and Hades-style meta-progression, then returned to Dead Cells-like unlocks.
  - Alterattacks fix Dead Cells' "no benefit to switching weapons" problem.
  - Monster Hunter inspired co-op combat and weapon depth.
- Early Access updates named by PCGamesN: Revenge update, Sudden Death rework, the Sanctuary biome, Endless Mode, and From the Leapers (memory deck, timed chests, skins, upscaling for low-end GPUs). The Early Access launch build had 5 biomes and 4 main weapons. — [PCGamesN guide](https://www.pcgamesn.com/windblown/guide)
- The Feb 2025 roadmap teased New Game+, extra modes and a 1.0 release. — [PCGamesN roadmap](https://www.pcgamesn.com/windblown/roadmap)
- The "0.9999999" update (22 Oct 2026) — [Rogueliker](https://rogueliker.com/windblown-last-update-before-launch/); [Capsule Computers](https://www.capsulecomputers.com.au/2026/10/one-last-leap-windblown-unleashes-final-early-access-update-before-1-0-launch/):
  - Motion Twin calls it the 10th and final Early Access content patch.
  - Content: reworked shops (six shop types), 17 new affixes, a rarity rework, Mythic Gift variants, new islands, better controller support, auto-combo accessibility, an enemy-attack warning system, timing tweaks and fast-return portals.
  - 1.0 is not dated; the team says it isn't ready to let go yet.
  - It launched alongside La Motion Week, with 30% off until 5 Nov.
- Store page (Oct 2026) — [Steam](https://store.steampowered.com/app/1911610/Windblown/):
  - The developers expect to leave Early Access sometime in 2026, and the price may rise at 1.0.
  - Base price €24.99.
  - Reviews: 92% positive of 4,947 English reviews (Very Positive); recent: 78% of 69.
  - Content: 25+ weapons, 20 trinkets, 105 passive upgrades, 20 hexes, 6 biomes and 170+ unlockables. The Early Access FAQ lists 7 biomes and 27 main weapons instead.
- Steam concurrents — [Steamcharts](https://steamcharts.com/app/1911610):
  - All-time peak 6,940 (month not shown; presumably the Oct 2024 launch).
  - Nov 2024: average 1,522. Jan 2025: 560. Mid-2025: about 290–440.
  - 2026 monthly averages: 95–185. Sep 2026: average 114, peak 260.

### Inferences
- **Moving to 3D top-down plus co-op did not hold an audience the way Dead Cells did.** Monthly-average concurrents fell about 70–80% within 6–8 months (1,522 in Nov 2024 to about 290–440 by mid-2025), and monthly peaks fell about 85% from the all-time high, despite good reviews.
- **Rough sales (my own).** A rule-of-thumb 30–60 sales per Steam review applied to about 5k English reviews (more across all languages) suggests very roughly 0.15–0.4M copies. This is not a sourced estimate.
- **For an original pixel side-scroller:** Dead Cells' own makers chose not to repeat the side-scrolling 2D format. Windblown's results suggest the format change carried risk, which slightly strengthens the case that a well-executed side-view pixel roguevania is less contested than it looks. This is speculative.

### Gaps
- No public sales figures, team size or engine for Windblown. Heaps is plausible but unconfirmed: heaps.io doesn't list Windblown.
- No 1.0 date.
- No direct developer quote on why they went 3D rather than 2D pixel art; the GamesRadar interviews were not machine-readable.
- October 2026 player data after the update was not yet in Steamcharts.

## 7. Adjacent side-scrolling references (team, engine, art approach, hook, results)

### Takeaway
The closest side-scrolling comparables split into three art strategies:
- **3D-rendered or 3D characters**: Dead Cells; Rogue Legacy 2 (3D characters on hand-drawn backdrops with stepped animation); The Rogue Prince of Persia (hand-drawn, 2.5D look).
- **Hand-drawn frame-by-frame pixel art**: Astral Ascent, Blasphemous (and Skul, tagged pixel graphics).
- **Hand-drawn non-pixel 2D**: Have a Nice Death.

Official results range from about 1M (Blasphemous, 2021) to 2M (Skul, Dec 2023). The rest have no public totals. Dead Cells' alumni studios are now the best-documented continuation of its approach: Evil Empire (Rogue Prince of Persia, Castlevania: Belmont's Curse) and Deepnight (Tenjutsu).

### Cited Findings
**Skul: The Hero Slayer** (SouthPAW Games, Korea; publisher Neowiz)
- Early Access 19 Feb 2020; 1.0 21 Jan 2021 (Windows, macOS, Linux); consoles 21 Oct 2021; mobile 4 Jun 2024. — [Wikipedia: Skul](https://en.wikipedia.org/wiki/Skul:_The_Hero_Slayer)
- Hook: swap your head for found skulls, two equipped at a time, each changing your abilities. — [Wikipedia: Skul](https://en.wikipedia.org/wiki/Skul:_The_Hero_Slayer)
- Official sales: 1M+ by Jan 2022 and 2M+ by Dec 2023. — [Wikipedia: Skul](https://en.wikipedia.org/wiki/Skul:_The_Hero_Slayer)
- Steam: €16.79; 92% positive of 9,704 English reviews; pixel art. — [Steam](https://store.steampowered.com/app/1147560/)
- Skul content appears in Dead Cells' Everyone Is Here update. — [Dead Cells wiki: Updates](https://deadcells.wiki.gg/wiki/Updates)

**Rogue Legacy 2** (Cellar Door Games, Canada; self-published; Unity)
- Early Access 18 Aug 2020, starting with 1.5 dungeons, 4 classes and 1 boss, with big updates planned about every 2 months. — [Wikipedia: Rogue Legacy 2](https://en.wikipedia.org/wiki/Rogue_Legacy_2)
- 1.0 on 28 Apr 2022 (PC and Xbox); Switch Nov 2022; PS Jun 2023. About 4 years of development. — [Wikipedia: Rogue Legacy 2](https://en.wikipedia.org/wiki/Rogue_Legacy_2)
- Metacritic: PC 88, Xbox Series 90, PS5 90, Switch 87. — [Wikipedia: Rogue Legacy 2](https://en.wikipedia.org/wiki/Rogue_Legacy_2)
- Steam describes a 2.5D look: 3D characters on hand-drawn backdrops, with stepped animation to keep the first game's feel. It lists 15 classes. — [Steam](https://store.steampowered.com/app/1253920/)
- Steam: 91% positive of 7,734 English reviews; €24.50. — [Steam](https://store.steampowered.com/app/1253920/)

**Astral Ascent** (Hibernian Workshop; publishers Hibernian Workshop and MP2 Games)
- Early Access 12 Apr 2022; 1.0 14 Nov 2023. — [Steam](https://store.steampowered.com/app/1280930/)
- Art: hand-drawn pixel graphics with frame-by-frame animation. — [Steam](https://store.steampowered.com/app/1280930/)
- Hook: 4 heroes, many spells, and 12 Zodiac bosses. — [Steam](https://store.steampowered.com/app/1280930/)
- Steam: €24.50; 95% positive of 1,862 English reviews ("Overwhelmingly Positive"). — [Steam](https://store.steampowered.com/app/1280930/)

**Blasphemous** (The Game Kitchen, Spain; publisher Team17; Unity)
- A pixel-art metroidvania (not a roguelite), included as a production reference. — [Wikipedia: Blasphemous](https://en.wikipedia.org/wiki/Blasphemous_(video_game))
- Kickstarter campaign in 2017; release 10 Sep 2019 on PC and consoles, with console ports by Warp Digital; mobile Nov 2024. — [Wikipedia: Blasphemous](https://en.wikipedia.org/wiki/Blasphemous_(video_game))
- 1M+ players by 2 Mar 2021 (official). — [Wikipedia: Blasphemous](https://en.wikipedia.org/wiki/Blasphemous_(video_game))
- Metacritic 76–82. Three free content updates followed: The Stir of Dawn (Aug 2020), Strife and Ruin (Feb 2021, with a Bloodstained crossover), and Wounds of Eventide (Dec 2021). Blasphemous 2 released 24 Aug 2023. — [Wikipedia: Blasphemous](https://en.wikipedia.org/wiki/Blasphemous_(video_game))
- Steam: €24.99; 90% positive of 19,925 English reviews. The store page advertises pixel-perfect execution animations. — [Steam](https://store.steampowered.com/app/774361/)

**The Rogue Prince of Persia** (Evil Empire; publisher Ubisoft)
- Announced Apr 2024 at the Triple-i showcase. Early Access 27 May 2024, delayed after Hades II's surprise Early Access launch. Act 2 arrived Nov 2024. — [Wikipedia](https://en.wikipedia.org/wiki/The_Rogue_Prince_of_Persia); [Steam](https://store.steampowered.com/app/2717880/)
- 1.0 on 20 Aug 2025 (PC, PS5, Xbox Series); Switch and Switch 2 on 16 Dec 2025. — [Wikipedia](https://en.wikipedia.org/wiki/The_Rogue_Prince_of_Persia)
- Gameplay: 2D side-scroller with procedural levels and parkour (wall-run, swing). A Sands-of-Time-style rewind was cut because it broke combat rhythm. — [Wikipedia](https://en.wikipedia.org/wiki/The_Rogue_Prince_of_Persia)
- Art: "ligne claire" style. Steam tags it hand-drawn and 2.5D. — [Wikipedia](https://en.wikipedia.org/wiki/The_Rogue_Prince_of_Persia); [Steam](https://store.steampowered.com/app/2717880/)
- Metacritic: 80 (PC) / 81 (PS5). — [Wikipedia](https://en.wikipedia.org/wiki/The_Rogue_Prince_of_Persia)
- Steam: €29.99; 100+ weapons and medallions; 90% positive of 1,427 English reviews. — [Steam](https://store.steampowered.com/app/2717880/)

**Castlevania: Belmont's Curse** (Evil Empire with Konami; Oct 2026) — a 2D action metroidvania, not a roguelite
- Announced Feb 2026 at Sony's State of Play; released 15 Oct 2026 on PS5, Xbox Series, Switch and PC. — [Edge](https://knowledge.edge-online.com/p/from-three-guys-publishing-dead-cells-to-60-people-making-the-next-castlevania-the-rise-of-evil-empi); [Game Informer](https://gameinformer.com/2026/07/17/castlvania-belmonts-curse-has-no-ai-developer-evil-empire-says); [Wikipedia: Motion Twin](https://en.wikipedia.org/wiki/Motion_Twin)
- Who made it, and the "no AI" stance:
  - Dead Cells' Wikipedia page lists Evil Empire and Motion Twin as co-developers. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
  - PCGamesN describes it as Evil Empire with Konami, with Motion Twin acting as a godfather figure. — [PCGamesN, Gamescom 2026](https://www.pcgamesn.com/castlevania-belmonts-curse/interview-gamescom-2026)
  - Evil Empire says no generative AI was used. — [Game Informer, 17 Jul 2026](https://gameinformer.com/2026/07/17/castlvania-belmonts-curse-has-no-ai-developer-evil-empire-says)
- Reviews are mostly positive. A secondary roundup reports IGN 8/10 and Famitsu 32/40. — [roundup](https://everythingedinburgh.com/?p=30653)
- Critics say it is safe and not genre-pushing, with some fiddly controls (Creative Bloq), "uneven" (Siliconera), and weak music (Gfinity). — [Creative Bloq](https://www.creativebloq.com/reviews/castlevania-belmonts-curse-review-a-new-adventure-with-a-new-generation-of-belmont-in-the-shadow-of-modern-metroidvanias); [Siliconera](https://www.siliconera.com/review-castlevania-belmonts-curse-is-an-uneven-return/); [Gfinity](https://www.gfinityesports.com/article/castlevania-belmonts-curse-our-review)

**Have a Nice Death** (Magic Design Studios, France; publisher Gearbox; Unity)
- A side-scrolling roguelite where you play Death as a CEO. Early Access Mar 2022; Switch and PC 22 Mar 2023; PS and Xbox Dec 2023. — [Wikipedia](https://en.wikipedia.org/wiki/Have_a_Nice_Death)
- Bénard's Deepnight Games worked on it as a gameplay consultant. — [deepnight.net](https://deepnight.net/games/have-a-nice-death/)

**Tenjutsu** (Deepnight Games, Sébastien Bénard)
- A roguelite brawler ("rogue-fu") in an open Japanese city.
- It grew out of a 48-hour jam game and has been in development for about 3 years. Bénard works solo with an animator (Dnilox), and Devolver handles most of the marketing. — [devlog 1, 30 Jun 2025](https://deepnight.net/blog/tenjutsu-dev-log-1-an-action-scene-which-became-a-game/)
- Converting it from a beat-'em-up to a roguelite took about 2–3 months. — [devlog 2](https://deepnight.net/blog/tenjutsu-devlog-2-from-beat-em-up-to-roguelite/)
- No release date is given in either devlog.

### Inferences
- **Most informative for a solo or small original pixel side-scroller:**
  - Astral Ascent, for the scope ceiling of hand-drawn pixel art: 4 heroes, 12 bosses.
  - Skul, for a cheap but deep hook (head-swapping changes the moveset) and the 2M ceiling of a mid-budget Korean indie.
  - Rogue Legacy 2, for 3D characters as a way to support many class/equipment combinations.
  - Tenjutsu, for the solo designer-programmer plus one animator model.
- **Rough Steam-only volumes (my own inference).** Applying a rough 30–60 sales per English Steam review: Rogue Legacy 2 about 0.25–0.5M, Astral Ascent about 60–120k. Low confidence; this excludes consoles, Game Pass and other stores.
- **Market saturation.** Evil Empire's pivot to big licences, and the "safe" reviews of Belmont's Curse, suggest the 2D action-platformer space is crowded with polished work. Differentiation has to come from the hook or the art, not from execution alone.

### Gaps
- No Gamalytic or VG Insights estimates; both sites were not machine-readable from here.
- Team sizes for SouthPAW, Cellar Door, Hibernian and The Game Kitchen; engines for Skul and Astral Ascent; Blasphemous animation workload and Kickstarter amount.
- Art pipelines for The Rogue Prince of Persia and Belmont's Curse.
- Sales for Rogue Legacy 2, Astral Ascent, The Rogue Prince of Persia and Have a Nice Death.

## 8. Production cost: side-scroller (Dead Cells-like) vs top-down (Death Must Die-like)

### Takeaway
Developer statements located here document the **side-scroller's** specific costs well:
- **Controls**: platforming-grade input forgiveness is non-negotiable under permadeath, and reviewers judge controls first.
- **Level construction**: hand-made, traversal-validated room chunks per biome, plus a fixed world graph with metroidvania gating, were too expensive even in "fully handmade" form for an 8–10-person team.
- **Animation iteration**: hand-drawn retakes cost days, versus under an hour with a 3D pipeline.

I found **no developer quote directly comparing** the two perspectives' animation counts. The structural difference is well known: top-down or isometric sprites need 4–8 facings per animation (often 5 unique plus mirrored for 8-way), while side-view needs one facing plus a mirror. Motion Twin's own top-down successor, Windblown, avoided that multiplier by going full 3D.

With the owner's 3D→sprite pipeline, the direction multiplier becomes mostly render time and storage. The side-scroller's control-tuning and level-authoring costs stay human costs.

### Cited Findings
- **Side-scroller controls are judged first.** With permadeath, controls must never be blamed. Most reviews discussed controls before gameplay. The team built just-in-time jumps, arrival correction, auto-aim and free turning before designing challenge. — [GDC 2019 slides](https://media.gdcvault.com/gdc2019/presentations/Benard-Sebastian-DeepCells.pdf); [Game Watch / Impress](https://game.watch.impress.co.jp/docs/news/1176126.html)
- **Level authoring cost:**
  - A small team could not finish handmade levels, so it went hybrid. Tiles are biome-specific and hand-made, and every biome needs its own graph. — [deepnight.net](https://deepnight.net/tutorial/the-level-design-of-dead-cells-a-hybrid-approach/)
  - Chunks are hand-designed so jumps are known to be doable, at about a 50:50 curated/random split. — [PCGamesInsider 2018](https://www.pcgamesinsider.biz/indie-interview/66988/how-motion-twin-stopped-dead-cells-being-just-another-roguelike-pixel-art-metroidvania-game/)
  - Level design was the biggest roguevania challenge, solved with runes. — [MCV](https://mcvuk.com/development-news/when-we-made-dead-cells/)
  - Monster placement in side view needs platform and space rules. — [deepnight.net](https://deepnight.net/tutorial/the-level-design-of-dead-cells-a-hybrid-approach/)
- **Animation cost:**
  - Hand-drawn retakes were too costly and Flash animation took ages. With 3D: dozens of timing tweaks in minutes, and part reuse saved hundreds of hours. — [Game Developer, Vasseur](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)
  - A tweak takes 30–60 minutes with 3D, versus 3–4 days to re-animate by hand. — [Game Developer, 2017](https://www.gamedeveloper.com/design/designing-each-of-the-50-weapons-in-i-dead-cells-i-to-feel-distinctive)
- **Environment art cost.** Parallax backgrounds per biome: grayscale plus gradient maps is the cheap path; hand-drawn background normal maps cost many weekends. Two artists handled all backgrounds. — [Game Developer, Massé](https://www.gamedeveloper.com/production/art-design-deep-dive-giving-back-colors-to-cryptic-worlds-in-i-dead-cells-i-)
- **Porting cost on an owned native stack.** Basic Xbox and Switch ports took about 1–2 months each with the Haxe/HL-C stack. — [MCV](https://mcvuk.com/development-news/when-we-made-dead-cells/); [Heaps FullStack](https://github.com/HeapsIO/heaps/wiki/FullStack)
- **Top-down examples:**
  - Windblown chose a "fully 3D world", top-down, for co-op and dash-driven traversal. — [Windblown press kit](https://windblown.game/Press); [PCGamesN guide](https://www.pcgamesn.com/windblown/guide); [Wikipedia: Windblown](https://en.wikipedia.org/wiki/Windblown)
  - In Oct 2026 it still needed an enemy-attack warning system for readability. — [Rogueliker](https://rogueliker.com/windblown-last-update-before-launch/)
  - Death Must Die (Realm Archive) is isometric 2D pixel art with 6 heroes, 8 gods and hundreds of blessings. Its Steam page (Oct 2026) still shows Early Access since 14 Nov 2023; cross-check with the Death Must Die researcher. Steam: 91% positive of 14,051 English reviews. — [Steam](https://store.steampowered.com/app/2334730/)
  - Hades (top-down): about 20 staff, a custom C++ engine on The Forge, about 2 years of Early Access, 700k Early Access sales and 1M+ within 3 days of 1.0 (Sep 2020). — [Wikipedia: Hades](https://en.wikipedia.org/wiki/Hades_(video_game))
- **Hybrid art approaches in side-scrollers.**
  - Rogue Legacy 2 uses 3D characters on hand-drawn backdrops with stepped animation. — [Steam](https://store.steampowered.com/app/1253920/)
  - Astral Ascent uses hand-drawn frame-by-frame pixel art with a small roster: 4 heroes, 12 bosses. — [Steam](https://store.steampowered.com/app/1280930/)

### Inferences
Cost-driver comparison, from cited facts plus general practice, labelled as inference:

| Cost driver | Side-scroller roguevania (Dead Cells-like) | Top-down / isometric action roguelite (Death Must Die-like) |
|---|---|---|
| Directions per animation | 1 facing + horizontal flip (standard practice, not stated by Motion Twin) | 4–8 facings (often 5 unique + mirror). With a 3D→sprite pipeline the extra cost is mostly render, VRAM and atlas size, plus per-direction cleanup of pixel flicker, a known drawback in Vasseur's pipeline |
| Player moveset | Traversal set: jump, fall, land, double jump, roll, ledge, climb, wall moves, plus per-weapon attack poses with readable silhouettes at about 50 px. Dead Cells has ~125 weapons, most needing bespoke hero attack animation | Fewer traversal states (move, dash). Attacks must read in every facing, and swarms of many enemies push VFX and performance budgets |
| Controls / feel | Platforming assists (coyote/just-in-time jumps, ledge correction, auto-aim, turn mid-attack) are mandatory and tuned before content (Bénard) | 8-way or analog aim and dash feel, and readability under crowds. Windblown needed extra telegraph UI, but there is less platforming-assist engineering |
| Level design | Hand-made, traversal-validated chunks per biome; per-biome graph; metroidvania gating; platform-aware enemy placement; 1–4 parallax layers per biome | Arenas or open fields with spawn waves; less traversal validation. Content scales through enemies, builds and bosses more than geometry |
| Iteration risk | Every new movement ability can invalidate chunks; collision and camera bugs are side-scroller specific | Balance of builds and the hordes' power curve dominates |

- **Net for the owner:** the side-scroller costs more **design and QA hours** (controls plus levels), and top-down costs more **art volume** (directions) but is cheaper in level authoring. The owner's existing Blender pre-render pipeline largely neutralizes the top-down art multiplier. It also gives the side-scroller Dead Cells' cheapest-iteration advantage, since retiming in 3D takes minutes.

### Gaps
- No direct developer quote comparing animation counts or level-design hours between side-scrolling and top-down roguelites; the search budget was exhausted before I could look further.
- Worth checking:
  - Supergiant talks on Hades' 3D-to-2D character pipeline;
  - the Children of Morta, Enter the Gungeon or Moonlighter developers on 8-directional sprites;
  - Realm Archive (Death Must Die) on its sprite pipeline;
  - the GDC 2019 video (not just the slides) for exact frame windows.
- No published total of Dead Cells hero animations or frames to quantify the side-view moveset cost.
