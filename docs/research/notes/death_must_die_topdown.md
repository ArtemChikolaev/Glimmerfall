# Death Must Die and adjacent top-down action roguelites / "survivors-ARPGs": dossier (as of 2026-10-25)

Research date: 2026-10-25. All Steam numbers (prices, review counts, "current players") were pulled on 2026-10-25 from Steam's official store/news APIs and SteamSpy unless stated otherwise. SteamSpy owner ranges, Raijin and similar figures are **third-party estimates**, not sales data.

Method notes:
- The shared WebSearch budget for this turn ran out midway. After that, every fact came from fetching known primary pages: Steam APIs, the Steam news feed, Wikipedia and the PCGamingWiki API.
- These sites blocked fetches: Fandom (HTTP 402), Gamalytic, Raijin, Prima Games and GameSkinny (403), Reddit (blocked), and PCGamingWiki's HTML and Cargo query (the per-page API worked).
- Some DMD wiki-level details therefore remain gaps and are flagged below.

## 1. Death Must Die: core facts (developer, engine, release status, price, platforms, co-op, reviews, sales, update history, roadmap)

### Takeaway
- **Who and what:** DMD is a self-published Unity game by Realm Archive, a tiny studio: 4 people in 2024, 5 in-house by Aug 2026 (3 artists and 2 programmers).
- **Status:** It is still in Steam Early Access almost 3 years after its 14 Nov 2023 launch. It ships on Windows and macOS only, single-player, English only, at an unchanged $6.99 / €6.89.
- **Reviews and sales:** About 21k Steam reviews at roughly 91% positive. Estimated sales are around 0.5–1M copies, or about $3M+ gross (estimates).
- **Development pace:** One major "Act" ships per year (Act II Apr 2024, Act III Apr 2025). A ground-up redesign announced Nov 2025 pushed Act 4 to "most likely early 2027".
- **Sentiment:** Recent reviews have slipped to 81%, and the top complaint is slow updates and silence.

### Cited Findings
**Identity, studio, engine**
- **Developer and publisher:** both are Realm Archive (self-published) — [Steam store](https://store.steampowered.com/app/2334730/Death_Must_Die/); [Steam appdetails API](https://store.steampowered.com/api/appdetails?appids=2334730&cc=us)
- **Team size:**
  - Feb 2024: a team of "just four people" — [GIGA, 2024-02-25](https://www.giga.de/news/94-prozent-positiv-steam-geheimtipp-kombiniert-2-der-groessten-hits-der-letzten-jahre-xyz/)
  - The NeonLightsMedia review (Nov) also calls it "a four-person team" — [NeonLightsMedia](https://www.neonlightsmedia.com/blog/death-must-die-review-a-hellish-roguelike-adventure)
  - Aug 2026: the team says it "recently expanded" to **five in-house members: three artists and two programmers**. It had used freelancers before, but onboarding them into code, design and visuals cost too much time, and it has "little intention" to grow further — [Steam "Act 4 News", 2026-08-17 (news API full text)](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements); human-readable hub: [Steam news](https://store.steampowered.com/news/app/2334730)
- **Developer voice:** a developer called "Ren" posts as Realm Archive on the Steam forums. The Nov 2025 roadmap says Ren will discuss the design informally on the forums and Discord — [Steam forum thread](https://steamcommunity.com/app/2334730/discussions/0/596271369016080451); [Roadmap News 2025-11-21](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)
- **Engine and technical details:**
  - PCGamingWiki lists the engine as **Unity**.
  - Saves go to `%USERPROFILE%\AppData\LocalLow\Realm Archive\Death Must Die\Saves`, which is Unity's standard persistentDataPath pattern.
  - The executable is 64-bit only.
  - There are separate Music/SFX/Voice/UI volume sliders.
  - Ultrawide needs a community fix.
  - Source: [PCGamingWiki (API wikitext)](https://www.pcgamingwiki.com/w/api.php?action=parse&page=Death_Must_Die&prop=wikitext&format=json)
- **Enemy AI:** elite and boss AI uses **behavior trees defined in TOML files** (composite, condition and action nodes). Example: the Act II Mummy boss switches to a stronger ability below 40% HP — [Act II Weekly Dev Log #1, 2024-03-10](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000)
- **Studio location:** not stated in any source I could reach (see Gaps).

**Release status, price, platforms, co-op**
- **Release timeline:**
  - Demo streams in June 2023. Release date announced 2023-08-16. The demo was taken down 2023-11-06 because saves would not carry over.
  - **Early Access launched 2023-11-14.**
  - Version numbering: 0.6.x at launch, 0.7.x with Act II, 0.8.x with Act III.
  - Source: [Steam news list (API)](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=100&maxlength=300&feeds=steam_community_announcements)
- **No 1.0 yet:**
  - The game is still in Early Access as of 2026-10-25. Steam genre tags still include "Early Access", and no full release date is listed.
  - Neither the Nov 2025 nor the Aug 2026 roadmap post mentions 1.0.
  - Sources: [Steam store](https://store.steampowered.com/app/2334730/Death_Must_Die/); [Act 4 News / Roadmap News](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)
- **Early Access FAQ (Steam page):**
  - Price is lower in EA and "will rise gradually with each major patch".
  - Base content is about 5–10 hours.
  - **5 Acts planned**, with no fixed timeline.
  - The content list on the page is stale: "6 playable characters with talent trees, 8 gods, 2 areas and 6 bosses".
  - Source: [Steam store](https://store.steampowered.com/app/2334730/Death_Must_Die/)
- **Price:**
  - **$6.99** / **€6.89** today. SteamSpy shows the current price equal to the undiscounted price (699 cents) — [Steam appdetails](https://store.steampowered.com/api/appdetails?appids=2334730&cc=us); [SteamSpy API](https://steamspy.com/api.php?request=appdetails&appid=2334730)
  - At launch it was $6.29 with a 10% launch discount, i.e. $6.99 base — [PCGamesN, 2023-11-18](https://www.pcgamesn.com/death-must-die/steam-game)
  - About €7 in Feb 2024 — [GIGA](https://www.giga.de/news/94-prozent-positiv-steam-geheimtipp-kombiniert-2-der-groessten-hits-der-letzten-jahre-xyz/)
- **Bundles:** cross-promo bundles with *20 Minutes Till Dawn* ("Death at Dawn"), *Brotato* ("Brotato Must Die"), *Sworn* and others. The page shows 7 bundles in total — [Steam store](https://store.steampowered.com/app/2334730/Death_Must_Die/). Bundles with 20MTD and CD 2: Trap Master date back to 2023-11-22 — [Steam news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=100&maxlength=300&feeds=steam_community_announcements)
- **Platforms:**
  - Windows and macOS only (no Linux build, no consoles, no mobile). English is the only language. There are 59 achievements and full controller support — [Steam store](https://store.steampowered.com/app/2334730/Death_Must_Die/); [appdetails](https://store.steampowered.com/api/appdetails?appids=2334730&cc=us)
  - Consoles: PC Invasion reports DMD appeared in the Triple-i Initiative showcase montage on 2024-04-10 as an upcoming Xbox title — [PC Invasion](https://www.pcinvasion.com/is-death-must-die-on-consoles-and-game-pass/)
  - However, DMD's own Triple-i Steam post covers talent trees and the goddess Ninh, not consoles. No console release or date has followed through Oct 2026 — [Act II dev log batch incl. Triple-i post](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000)
- **Co-op and multiplayer:** single-player only. On 2025-04-06 the developer (Ren) said there are "generally" no plans for multiplayer because the game "isn't set up for it" in code or design. It might be considered after 1.0 — [Steam forum](https://steamcommunity.com/app/2334730/discussions/0/596271369016080451)

**Reviews and players**
- **Steam store (English reviews):** Very Positive, **91% of 14,051**. **Recent (last 30 days): 81% of 61** — [Steam store, fetched 2026-10-25](https://store.steampowered.com/app/2334730/Death_Must_Die/)
- **All languages:**
  - Steam's recommendation count is **20,997** — [appdetails](https://store.steampowered.com/api/appdetails?appids=2334730&cc=us)
  - SteamSpy counts 18,498 positive and 1,829 negative, which is 91.0% — [SteamSpy](https://steamspy.com/api.php?request=appdetails&appid=2334730)
- **Earlier review scores:**
  - About 1,000 reviews at about 89% four days after launch, with "thousands" of concurrent players — [PCGamesN, 2023-11-18](https://www.pcgamesn.com/death-must-die/steam-game)
  - 94% positive in Feb 2024 — [GIGA](https://www.giga.de/news/94-prozent-positiv-steam-geheimtipp-kombiniert-2-der-groessten-hits-der-letzten-jahre-xyz/)
  - 91% of 19,000+ in Apr 2025 — [PCGamesN, 2025-04-27](https://www.pcgamesn.com/death-must-die/act-3)
- **Concurrent players today:**
  - **230** players at one moment on 2026-10-25 — [Steam GetNumberOfCurrentPlayers](https://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1/?appid=2334730)
  - SteamSpy's previous-day peak CCU is 499 — [SteamSpy](https://steamspy.com/api.php?request=appdetails&appid=2334730)
- **Top user tags:** Action Roguelike, Bullet Hell, Rogue-lite, Pixel Graphics, RPG, Hack and Slash, Isometric, Loot, Dark Fantasy — [SteamSpy](https://steamspy.com/api.php?request=appdetails&appid=2334730)

**Sales and revenue (ESTIMATES)**
- **SteamSpy:** 500,000–1,000,000 owners (estimate) — [SteamSpy](https://steamspy.com/api.php?request=appdetails&appid=2334730)
- **Raijin:** about **562.9K copies** and about **$3.3M gross revenue**, "medium confidence", dated 2026-07-28 (estimate). Caveat: this came from a search-result summary. The page itself returned HTTP 403 to my fetch, so I could not check its method — [Raijin](https://raijin.gg/app/2334730/Death_Must_Die)
- **Gamalytic accuracy:** Gamalytic says 77% of its estimates fall within 30% error and 98% within 50% (useful as an error bar for all such tools). Its page for DMD could not be fetched — [steampageanalyzer note on Gamalytic accuracy](https://www.steampageanalyzer.com/blog/gamalytic-alternatives)

**Update history (all 49 Steam announcements)** — [Steam news list (API)](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=100&maxlength=300&feeds=steam_community_announcements)
- **Launch period:**
  - 2023-11-14: EA launch.
  - About 12 patches in the first 3 weeks (0.6.13→0.6.77). They covered Fates blessings, enemy damage −10%, a Darkness modifier rework, rebalancing **Difficulty 30 to be "consistently beatable"**, extra stash tabs, and Merchant/Buyback tabs.
  - 2023-11-30: Roadmap (image only).
  - 2023-12-20: "Snowswatch" holiday event.
- **Act II, Endless Desert (2024-04-20):**
  - Six weekly dev logs ran from 2024-03-10 to 04-15.
  - Act II added the hero Lorien and the goddess Ninh, talent trees with 30+ talents per hero, Tier 2 items and dozens of uniques, Darkness 100, and the removal of item binding — [Act II posts](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000)
- **Second half of 2024:**
  - 2024-07-19: "LIVE roadmap" (images).
  - 2024-08: two QoL patches (item and talent loadouts, Armory, loot filter).
  - 2024-08-25: public balance beta branch.
  - **2024-10-10 "Procedural Horde" patch 0.7.70** (procedural waves, enemy subtypes). Source: [2024 posts](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=6&maxlength=0&feeds=steam_community_announcements&enddate=1728700000)
  - 2024-12-16: Snowswatch returns.
- **Act III, The Darkmoor (2025-04-25):**
  - Added the new god Aegir, a Druid shapeshifter hero, a summon system, Tier 3 items and uniques, and Fated Infusions.
  - A next-day hotfix cut enemy HP by 30% and damage by 20%, doubled item drops (+100%) and raised XP by about 15%.
  - Patches 0.8.2–0.8.5 followed through 2025-05-22.
  - Source: [Act III posts](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements&enddate=1745800000)
- **Since then (silence gaps):**
  - About 6 months of silence until the **2025-11-21 "Roadmap News"**, then a 2025-12-17 Snowswatch post with hotfix 0.8.7.
  - About 8 months of silence until the **2026-08-17 "Act 4 News"**. No new Act or content patch has shipped since May 2025, only the seasonal Snowswatch event and hotfix 0.8.7 in Dec 2025.

**Roadmap (Nov 2025) and Act 4 status (Aug 2026)** — [full text via news API](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)
- **Nov 2025 plan, part 1 (scope and structure):**
  - "Sweeping changes" to expand scope, and minor updates skipped in favour of one large Act 4 update targeted for mid-to-late 2026.
  - The world moves from an "infinite plane with scattered objects" to explorable levels: open areas, winding paths, secret rooms and special encounters.
  - A map editor, procedural dungeon generation and new pathfinding are planned.
  - Art for Acts 1–3 is to be redone.
- **Nov 2025 plan, part 2 (skills, blessings, modes):**
  - A new active-skill system allowing 0 to 5 active buttons, with cast, channel and charge animations.
  - "Rituals" as an alternative blessing type.
  - Every god should offer options for every playstyle.
  - Players choose their path, rewards, gods, shops and encounters, with "bad RNG protection".
  - Custom modes are planned for Act 4: an infinite mode, an "endless terrain" mode (the current level style) and toggleable modifiers.
- **Aug 2026, delay and causes:** Act 4 "will most likely spill into early 2027". Stated causes are overscoping, experimental design needing iteration, "life delays" and repeatedly missed internal estimates. The team will not give a new date.
- **Aug 2026, blessing rebuild:** the god system was rebuilt from scratch.
  - Blessing **slots were removed**.
  - Cross-god and within-god mechanics were added (e.g., Mort harvesting souls, Leigong's static bursts).
  - Players choose their god after combat.
- **Aug 2026, skills, characters, levels:**
  - Every active skill gets its own upgrade tree.
  - Two new heroes (Necromancer, Shaman) are in development and may slip past Act 4.
  - The procedural level system (height, holes, tile variety, wall matching) "took many months".

### Inferences
- **The price never rose.** The FAQ promised gradual increases, but the base price is $6.99 at launch (Nov 2023), in Feb 2024 and in Oct 2026. Revenue therefore relies on volume, bundles and sales.
- **Rough net revenue (inference, very approximate):** take an estimated 0.56–1M units at $6.99 with frequent discounts and bundles, subtract Steam's 30% and VAT/refunds/regional pricing. That lands plausibly in the **$1.5–3M net range over about 3 years** for a 4–5 person team. Order-of-magnitude only.
- **A Boxleiter-style check is consistent with the third-party estimates.** About 21k Steam reviews × 30–50 sales per review ≈ 0.6–1.05M units. This is an inference, not data.
- **Cadence and communication are DMD's weak point.** One Act per year, then about 17 months between content drops (May 2025 → 2027), and long silences. This lines up with the decline from 91% all-time to 81% recent, and with the top complaint on VaporLens (Section 3).
- **The team is rebuilding the base game mid-Early-Access.** Changes include the world structure, the blessing system, active skills and 12× animation per hero. That signals the original "survivors on an infinite plane + Hades-style slotted boons" design hit a depth or retention ceiling after 3 acts.
- **Opening for a newcomer:** a competitor offering DMD's fantasy with a faster, more reliable cadence (and co-op or localization) has room.
- **Stale project brief.** The brief's section 1 (2026-09-05) is outdated on several points. Gods now number 10, not 9 (Aegir, added Apr 2025). Heroes are now about 8, not 6 (Lorien and Druid). Waves have been procedural since Oct 2024, not fixed minutes. The Darkness ladder runs to 100, not "30–40". The announced Act 4 redesign removes blessing slots. Note: Act 4 is **not yet shipped**, so the live game still has the 2025 systems.

### Gaps
- Studio location/country and founders' backgrounds: not found in reachable sources (Steam, the GIGA article and NeonLights don't state it).
- All-time peak CCU (SteamDB): not fetched (SteamDB is behind Cloudflare). Only today's snapshot (230) and SteamSpy's previous-day peak (499) are available.
- Gamalytic and VG Insights (now Sensor Tower, which redirects to app.sensortower.com) estimates: not accessible. Raijin's figure is second-hand (search snippet).
- Whether Xbox/console ports are still planned: no 2025–26 statement found.
- Whether a 1.0 date or plan exists: not stated anywhere in 2025–26 posts.

## 2. Death Must Die: systems in depth and content counts

### Takeaway
DMD's loop is a ~20-minute timed act. You auto-attack hordes, and bosses spawn at fixed minutes (7/14/20 in Act I). Level-ups offer Hades-style blessings from up to 3 gods (Moirai can be a 4th). Gods, as of 2026, number 10:
- Time, Krom, Ninh, Leigong, Moirai, Summer, Winter, Mort, Justice and Aegir.

The progression around that loop:
- **Loot:** Diablo-like gear (tiered T1–T3 by act, affixes, uniques) feeds a persistent stash/Armory.
- **Per-hero talent tree:** the Constellarium, 30+ talents, funded by Cosmic Essence from bosses.
- **Difficulty:** a Darkness ladder up to 100.

Content (live game) is roughly 8 heroes, 10 gods, 3 acts with about 3 bosses each, "hundreds" of blessings and tiered randomized and unique items. A big rework (blessings, active skills, structured levels) is pending for Act 4 (2027).

### Cited Findings
**Run structure**
- **Act I bosses:** a timer at the top of the screen; bosses spawn at set minutes. Act I has the Baron's Hands (left and right, up to two at once) at **7:00**, the Necromancer at **14:00**, and Dracula, Baron of the Outer Cycle at **20:00** — [Gamepressure, 2023-12-22](https://www.gamepressure.com/newsroom/death-must-die-bosses/z2665a)
- **Brief's unverified claims:** the project brief (citing the Fandom wiki) says normal spawns pause while a boss lives and some waves have "barriers". I could not re-verify this because Fandom returned 402 — [project brief, docs/arcade-survivors-brief.md §1, 2026-09-05](https://dmd.fandom.com/wiki/Waves)
- **Procedural waves:** since patch 0.7.70 (2024-10-10), "every run" has procedurally generated waves plus new enemy subtypes with special powers for minions and elites. Bosses now try to stay inside the arena — [0.7.70 notes](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=6&maxlength=0&feeds=steam_community_announcements&enddate=1728700000)
- **Map content:**
  - The store lists "ancient shrines, mythical creatures" and the risk-reward of increasing the odds of finding lost relics "at the cost of empowering Death's legions" — [Steam appdetails description](https://store.steampowered.com/api/appdetails?appids=2334730&cc=us)
  - Curses from graves and coffins became temporary in Oct 2024 — [0.7.70](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=6&maxlength=0&feeds=steam_community_announcements&enddate=1728700000)
  - Act III added charm mechanics from the Siren and the Lady (nerfed the next day) and an optional charm encounter — [Act III hotfix](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements&enddate=1745800000); [news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=100&maxlength=300&feeds=steam_community_announcements)
- **Acts:**
  - Act I at launch (Nov 2023).
  - Act II, **Endless Desert** (2024-04-20).
  - Act III, **The Darkmoor** (2025-04-25). Bosses and enemies named in 0.8.2 tuning include the Frog King, Siren, Lady of the Lake, Scarecrow and Wraiths.
  - Act 4 is in development; 5 acts are planned.
  - Sources: [news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=100&maxlength=300&feeds=steam_community_announcements); [Steam store FAQ](https://store.steampowered.com/app/2334730/Death_Must_Die/)
- **Pact of Greed:** unlocked after beating Act 2. It lets you run Act 1 at Act 2 difficulty for Tier 2 rewards — [0.7.70](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=6&maxlength=0&feeds=steam_community_announcements&enddate=1728700000)

**Heroes**
- **Launch-era roster:** Avoron the Knight (the starter), Merris the Sorceress (the only ranged attacker at the time), Kront the Barbarian (AoE attacks), Skadi the Warrior (large melee) and Nixi the Assassin (melee, mobility). This comes from Prima Games and others as summarized in search results; their pages were not directly fetchable — [Prima Games](https://primagames.com/tips/all-unlockable-characters-in-death-must-die-listed); [GameSkinny](https://www.gameskinny.com/tips/death-must-die-best-characters-ranked/)
- **Later heroes:**
  - **Lorien the Ranger** (bow, mobility, volleys and seeking bolts) arrived with Act II — [Dev Log #4, 2024-03-31](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000)
  - **The Druid** (shapeshifts into a tanky bear or agile panther, with more forms unlockable and upgradable) arrived with Act III — [PCGamesN, 2025-04-27](https://www.pcgamesn.com/death-must-die/act-3)
  - **Necromancer** and **Shaman** are in development (Aug 2026) — [Act 4 News](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)
- **Conflicting roster counts:**
  - The Steam FAQ says 6 characters — [Steam store](https://store.steampowered.com/app/2334730/Death_Must_Die/)
  - Prima (undated, about 2024) says 7, with Avoron as the starter — [Prima (via search)](https://primagames.com/tips/all-unlockable-characters-in-death-must-die-listed)
  - PCGamesN at launch reported "three playable characters, seven enemy types, one final boss", probably the initially available content — [PCGamesN, 2023-11-18](https://www.pcgamesn.com/death-must-die/steam-game)
- **Attack redesign (Act II):**
  - The attack movement penalty was removed, after being the #1 complaint.
  - Auto-attack fires only when an enemy is in range.
  - Each hero got multiple attack animations with tunable speed, area and timing per sub-attack (e.g., Skadi's swing followed by a jump-cleave).
  - Source: [Dev Log #2, 2024-03-17](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000)

**Gods and blessings**
- **Per-god themes (TheGamer ranking, Jun 2024):**

  | God | Theme / status effect | Notes |
  |---|---|---|
  | Time | Support | Alteration Ritual, called the strongest blessing; Force widens XP pickup range |
  | Krom | Conquest; **Rupture** | Rupture damages moving enemies, more when knocked back |
  | Ninh | Earth; **stun** | Strong Cast blessings |
  | Leigong | Lightning | Most Strike blessings; move speed and cooldown reduction |
  | Moirai | Chance | Three sisters: Clo, Lachi, Aisa |
  | Summer | **Burning** (DoT) | |
  | Winter | **Chilled** → **Frozen** at 7 stacks | |
  | Mort | **Cursed** (drains a share of HP) | Summon focus |
  | Lady Justice | Healing / defense | |

  Source: [TheGamer, 2024-06-06](https://www.thegamer.com/death-must-die-gods-ranked/)
- **Aegir of the Seas** was added in Act III (2025-04-25) — [PCGamesN](https://www.pcgamesn.com/death-must-die/act-3); [Act III post](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements&enddate=1745800000)
- **Death himself** is the unplayable antagonist god ("9 gods, one unplayable" in an older guide) — [Prima (via search)](https://primagames.com/tips/all-gods-in-death-must-die-listed)
- **Launch-era god count:** "six gods, each with over ten skills" — [PCGamesN, 2023-11-18](https://www.pcgamesn.com/death-must-die/steam-game). The Steam FAQ says 8 gods (stale) — [Steam](https://store.steampowered.com/app/2334730/Death_Must_Die/)
- **Rules:**
  - Blessings come from **at most 3 gods per run**. The Moirai can still appear after 3 gods are chosen, which effectively allows a 4th.
  - Blessings have rarities, including Legendary, and can be upgraded.
  - Source: [TheGamer](https://www.thegamer.com/death-must-die-gods-ranked/)
  - **Reroll / Banish / Alter** exist (controller-navigation fix, Nov 2023) — [patch 0.6.47](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=100&maxlength=300&feeds=steam_community_announcements)
  - "Hundreds of god blessings" — [Steam store](https://store.steampowered.com/app/2334730/Death_Must_Die/)
- **Blessing categories (Mar 2024 redesign):**
  - Summons and dashes had counted as "spells", so spell bonuses boosted them and attack+dash builds were not viable.
  - They became separate categories, and spells were split into **Strikes** (triggered by attacks), **Casts** (cooldown-based) and **Powers** (other).
  - Together with Attack and Passive, these match the 7 types listed in the brief.
  - Source: [Dev Log #3, 2024-03-24](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000)
- **Announced for Act 4 (not live):**
  - Blessing **slots removed**, with blessings built around inherent synergies.
  - Cross-god "status rituals".
  - God choice after combat, with bad-RNG protection to reduce reliance on rerolls.
  - Per-skill upgrade trees and 0–5 active buttons.
  - Sources: [Act 4 News / Roadmap News](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)

**Loot, stash, merchants**
- **Gear types:** weapons, armor, rings, pendants "and more" in a "Diablo-style loot and gear system" — [PCGamesN, 2023](https://www.pcgamesn.com/death-must-die/steam-game); [PCGamesN Act 2](https://www.pcgamesn.com/death-must-die/act-2-steam)
- **Item tiers by act:** Tier 2 items and "dozens of new Unique Items" in Act II; Tier 3 items and uniques "with powerful abilities" in Act III — [Act II LIVE](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000); [Act III](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements&enddate=1745800000)
- **Affix changes (Oct 2024):**
  - Items drop with fewer but more build-tailored affixes, and more affix types can appear on more item types.
  - Base affixes show in white, rolled or enhanced ones in green.
  - **Drops favour the currently played hero.**
  - New T2 uniques were added and some T1 uniques reworked.
  - Source: [0.7.70](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=6&maxlength=0&feeds=steam_community_announcements&enddate=1728700000)
- **Persistence and storage:**
  - Item binding was removed (Act II).
  - Stash tabs were doubled to **8**.
  - A Library stash holds Lore items (Nov 2023).
  - Merchant and Buyback got separate shop tabs (Nov 2023).
  - Item and talent **loadouts**, **multi-equip** and a **loot rarity filter** arrived in Aug 2024, plus an Armory browsable by type, preset and affix, and autocompare.
  - Sources: [Dev Log #3 / Act II](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000); [news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=100&maxlength=300&feeds=steam_community_announcements); [2024 QoL posts](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=6&maxlength=0&feeds=steam_community_announcements&enddate=1728700000)
- **"Master" item offers** were extended to all item types in the Act III hotfix — [Act III hotfix](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements&enddate=1745800000)

**Meta-progression and difficulty**
- **Talent trees ("Constellarium"):**
  - 30+ talents per hero, replacing the earlier simple "signs".
  - Heroes earn up to 36 **Cosmic Essence** from bosses (Apr 2024).
  - Four example Knight builds were shown (Classic, Holy, Great Archon of Aulum, Champion).
  - Source: [Triple-i post, 2024-04-10](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000)
  - Boss essence rewards were later raised: Act 1 from 4 to 8, Act 2 from 8 to 12 — [0.7.70](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=6&maxlength=0&feeds=steam_community_announcements&enddate=1728700000)
- **Darkness (difficulty ladder):**
  - Darkness 30 gives maximum item drops.
  - **Darkness 100** was added with Act II as the ultimate challenge.
  - At D30 players pick which modifiers to take.
  - The Darkness record is tracked per act.
  - Attack Slowdown became an optional modifier.
  - Source: [Dev Log #5, 2024-04-07 / Act II LIVE](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000)
- **Other progression systems:**
  - Fated Infusions, books and achievements (Act III) — [Act III](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements&enddate=1745800000)
  - A Mailbox for dev news (Aug 2024) — [news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=100&maxlength=300&feeds=steam_community_announcements)
  - 59 Steam achievements — [appdetails](https://store.steampowered.com/api/appdetails?appids=2334730&cc=us)
  - Seasonal "Snowswatch" event every December (2023, 2024, 2025) — [news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=100&maxlength=300&feeds=steam_community_announcements)
- **Endgame now:** Darkness up to 100 plus Pact of Greed. **Infinite/endless mode and custom modifiers are only planned** (Act 4) — [Roadmap News 2025-11-21](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)

### Inferences
- **Current DMD content (live game, inferred from the above):**
  - About **8 heroes**: 5–6 launch heroes plus Lorien and the Druid.
  - **10 gods.**
  - **3 acts** with about 3 boss slots each, so roughly 9–10 bosses. Act I has 4 boss entities, counting both Baron's Hands.
  - **3 item tiers**, "dozens" of uniques per tier and "hundreds" of blessings.
  - Exact enemy, blessing and item counts are unpublished.
- **What drives DMD's depth:** Hades-style gods/status effects (each god owns a status: Rupture, Burning, Chilled/Frozen, Stun, Cursed…) layered on Diablo gear with persistent stash, loadouts and an affix-sorting Armory. That is the "ARPG" half that pure survivors-likes (Vampire Survivors, Brotato) lack.
- **The 2024 "spells vs attacks" fix is a cautionary design lesson.** Category taxonomy (what counts as Strike/Cast/Power/Summon/Dash) decides which builds are viable. Getting it wrong early forced a costly rework (animations, rebalancing, refactoring) that delayed Act II.
- **"Drops favor the current hero" and loadouts show what DMD learned:** a shared stash across many heroes produces loot noise. Hero-weighted drops and preset loadouts are retention QoL a newcomer should ship from day one.

### Gaps
- Fandom pages (Blessing, Item, Waves, Village, Darkness) returned HTTP 402, so these are unverified:
  - exact equipment slot list;
  - item rarity names and the blessing rarity ladder (the brief says Novice→Master; TheGamer confirms "Legendary");
  - salvage/crafting mechanics;
  - hub (village) NPCs;
  - exact blessing counts per god;
  - enemy counts per act.
- The name and class of the 6th launch-era hero (if any) could not be confirmed. The FAQ (6), Prima (7) and the launch-era PCGamesN figure (3 available) conflict.
- Whether boss fights pause regular spawning and how barrier waves work in 2026: unverified (only the brief's earlier Fandom citation).

## 3. Death Must Die: presentation (art, animation, VFX, UI, audio) and what players and press praise or criticise

### Takeaway
- **Look:** DMD reads as isometric 2D pixel art on a flat plane.
- **How it's made:** the team states it hand-draws animation frames for every direction. Its 2026 overhaul adds 32-direction channel animations and 16-direction dash attacks, roughly a **12× per-hero animation budget**, plus VFX and environment overhauls.
- **Praise:** fun, build variety, talent trees, pixel art, sound and voice acting, and price.
- **Criticism:** dominated by **slow updates and poor communication**, then no localization, limited maps and content, repetitiveness and an unfinished story.

### Cited Findings
**Art and animation**
- **Tags:** user tags include Pixel Graphics (310 votes), 2D (237) and Isometric (196) — [SteamSpy](https://steamspy.com/api.php?request=appdetails&appid=2334730)
- **Press on the visuals:**
  - "Gorgeous pixel aesthetic"; a "large, flat, isometric map" with skeletons and shield knights — [PCGamesN, 2023-11-18](https://www.pcgamesn.com/death-must-die/steam-game)
  - The pixel art gets the most praise, with a flat isometric map and a "modern twist" — [NeonLightsMedia (8.5/10)](https://www.neonlightsmedia.com/blog/death-must-die-review-a-hellish-roguelike-adventure)
- **Production method (Aug 2026):**
  - The team says it "drew all the required frames". Earlier attempts to avoid drawing every direction "didn't play well" and wasted time, and they wish they had drawn all directions earlier.
  - New per-hero animations:
    - target channel, plus walking forward and backward while channeling: **32 directions**;
    - dash attack: **16 directions**;
    - target and no-target casts and channels, plus a post-dash slide.
  - This is "roughly 12×" the animation budget per character. Preview art of Nixi's channel was shared.
  - The post doesn't name "pixel art" or mention 3D.
  - Source: [Act 4 News](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)
- **Earlier animation work (Mar 2024):** multiple attack animations per hero, each with sub-attack timing and area — [Dev Log #2](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000)
- **VFX:** new spells and effects designed to "feel as powerful as possible"; older weak VFX being overhauled (2026) — [Act 4 News](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)
- **Environment:**
  - It started as an "infinite plane with scattered objects".
  - A new procedural level system handles height, holes, tile variety and wall matching. It needed months of work on collision, pathing, wall sorting and optimization.
  - Tiles, walls and props are still being drawn, slower than desired.
  - Sources: [Roadmap News / Act 4 News](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)

**UI and audio**
- **UI:** full controller support — [appdetails](https://store.steampowered.com/api/appdetails?appids=2334730&cc=us). Ultrawide needs a community fix — [PCGamingWiki](https://www.pcgamingwiki.com/w/api.php?action=parse&page=Death_Must_Die&prop=wikitext&format=json)
- **QoL UI added in 2024:** Armory filters, stash indicators, autocompare and a loot filter — [2024 QoL posts](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=6&maxlength=0&feeds=steam_community_announcements&enddate=1728700000)
- **Audio:**
  - Separate Voice volume, implying voiced content — [PCGamingWiki](https://www.pcgamingwiki.com/w/api.php?action=parse&page=Death_Must_Die&prop=wikitext&format=json)
  - A narrator/bard frame ("The Saga of Death's Demise", told by The Bard) — [Steam description](https://store.steampowered.com/api/appdetails?appids=2334730&cc=us)
  - Act II brought a "thematic soundtrack" — [Act II LIVE](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000)
  - No soundtrack DLC exists — [Steam store](https://store.steampowered.com/app/2334730/Death_Must_Die/)

**Player sentiment (aggregated Steam reviews, VaporLens)** — [VaporLens](https://vaporlens.app/app/2334730/death_must_die.md)
- **Top positives (weight):** highly enjoyable (0.24), diverse skill trees (0.19), build variety (0.17), beautiful pixel art (0.12), top-tier roguelite (0.10), worth the price (0.09), polished feel (0.08), great sound and voice acting (0.08), good pacing (0.08), meaningful uniques (0.07).
- **Top negatives (weight):** **lack of updates and communication (0.31)**, missing localizations (0.18), limited maps and content (0.13), gets boring quickly (0.09), not worth the money (0.07), story incomplete (0.06).
- **Category sentiment:**
  - Performance is positive (Steam Deck, Proton, low-end PCs).
  - Players see a fair price as $5–10.
  - Content sentiment is "mixed".
  - Friction: no tutorial, unclear progression.
- **Recent trend:** recent reviews are 81% (61 reviews in 30 days) against 91% all-time — [Steam store](https://store.steampowered.com/app/2334730/Death_Must_Die/)

**Press**
- Genre fusion: "Hades meets Vampire Survivors" with Diablo loot — [GIGA](https://www.giga.de/news/94-prozent-positiv-steam-geheimtipp-kombiniert-2-der-groessten-hits-der-letzten-jahre-xyz/); [PCGamesN](https://www.pcgamesn.com/death-must-die/steam-game)
- Review score: NeonLightsMedia 8.5/10 — [NeonLightsMedia](https://www.neonlightsmedia.com/blog/death-must-die-review-a-hellish-roguelike-adventure)
- Early complaints, as the developer reported them:
  - The attack movement penalty was the biggest complaint.
  - Attacks felt "underwhelming" and melee heroes too similar.
  - Attack builds were weaker than spell builds.
  - Source: [Dev Log #2](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=8&maxlength=0&feeds=steam_community_announcements&enddate=1713700000)
- Act III launched over-tuned and was patched the next day: enemy HP −30%, damage −20%, charm nerfs, drops +100% — [Act III hotfix](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements&enddate=1745800000)

### Inferences
- **DMD's appeal is "feel plus build-craft at an impulse price", not visual spectacle.** Its art is praised but modest: a flat plane and limited directional animation pre-2026. The team now believes it must pay for 12× animation, VFX and structured environments to keep competing. For a new game aiming to beat DMD on style and animation (the Dead Cells-like ambition), this is the most expensive axis, and DMD's 3-artist team is struggling with it.
- **Directional hand-drawn sprites scale badly.** Costs multiply per direction × per action × per hero. DMD went from about 8 to 16/32 directions on new actions. Halls of Torment (pre-rendered 3D), Hades II (real-time 3D characters) and Megabonk (3D) show alternative pipelines (Section 4).
- **Cheap wins against DMD's main complaints:**
  - localization (English-only after 3 years);
  - predictable content cadence and public roadmaps;
  - an endless mode;
  - map variety (structured levels instead of an infinite plane).

### Gaps
- Whether DMD's pixel art is fully hand-pixelled or uses 3D/paint-over or downscaling: not stated. "Drew all frames" suggests hand-drawn.
- Sprite resolution and scale, and the number of base movement directions (pre-2026): not found.
- Composer and sound designer credits: not found.
- Reddit sentiment (r/DeathMustDie) could not be fetched (blocked), so sentiment rests on Steam/VaporLens and press.

## 4. Adjacent references: developer, team, engine, dates, price, sales, hook, meta, art, content, retention

### Takeaway
The closest commercial comparables split into three groups:
1. **Ultra-cheap survivors-likes made by solo devs or tiny teams:** Vampire Survivors, Brotato, Halls of Torment, Megabonk, 20 Minutes Till Dawn. They run $4.99–9.99, sell 1M–10M+, and are built in Phaser→Unity, Godot or Unity.
2. **Mid-priced "survivors+" hybrids on a strong IP or studio:** DRG: Survivor and Soulstone Survivors ($12.99–14.99, Unity, 1M+).
3. **Premium hand-crafted action roguelites ($22–30) from experienced studios with bigger teams and/or custom engines:** Hades/Hades II, Windblown, Ravenswatch, Absolum, Children of Morta.

DMD sits in group 1 on price but group 3 in ambition (ARPG loot, gods). Its team size is group 1.

### Cited Findings
**Snapshot table (2026-10-25)**

Sources for the table:
- [Steam appdetails API](https://store.steampowered.com/api/appdetails?appids=1794680&cc=us) (one call per appid; prices are US list prices);
- [SteamSpy API](https://steamspy.com/api.php?request=appdetails&appid=1794680) (percent positive and owners, both estimates; SteamSpy counts lag, e.g., Hades II at 64k counted vs 115k Steam recs);
- [Steam current players API](https://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1/?appid=1794680) (a single moment, all titles within about 2 minutes);
- engines from the per-game sources below.

| Game | Dev (publisher) | Engine | EA → 1.0 | US list price | Steam recs | % pos (SteamSpy) | SteamSpy owners (est.) | Players now |
|---|---|---|---|---|---|---|---|---|
| Death Must Die | Realm Archive (self) | Unity | 2023-11-14 → none yet | $6.99 | 20,997 | 91.0 | 0.5–1M | 230 |
| Halls of Torment | Chasing Carrots (self) | Godot (from Unity) | 2023-05-24 → 2024-09-24 | $6.66 (sale $4.99) | 30,896 | 95.8 | 0.5–1M | 1,165 |
| Vampire Survivors | poncle | Phaser → Unity (v1.6, 2023-08) | 2021-12-17 → 2022-10-20 | $4.99 | 252,572 | 98.5 | 5–10M | 5,009 |
| Brotato | Blobfish (+Evil Empire from 2025) | Godot | 2022-09-27 → 2023-06-23 | $4.99 | 112,475 | 96.7 | 2–5M | 6,576 |
| Hades | Supergiant | custom C++ on The Forge | 2018-12 (EGS) / 2019-12-10 (Steam) → 2020-09-17 | $24.99 | 287,492 | 98.2 | 5–10M | n/m |
| Hades II | Supergiant | (custom, Lua-scripted) | 2024-05 → 2025-09-25 | $29.99 | 115,495 | 94.9 | 2–5M | 4,586 |
| Children of Morta | Dead Mage (11 bit studios) | Unity 2017→2019 | release 2019-09-03 (EA history not checked) | $21.99 | 18,511 | 88.8 | 1–2M | n/m |
| Soulstone Survivors | Game Smithing Limited | Unity | 2022-11-07 → 2025-06-17 | $14.99 (sale $10.49) | 25,866 | 91.3 | 0.5–1M | 683 |
| Megabonk | vedinad (solo) | Unity | no EA; 2025-09-18 | $9.99 | 101,373 | n/a | n/a | 4,496 |
| Ravenswatch | Passtech Games (Nacon) | "OEngine" (in-house) | 2023-04-06 → 2024-09-26 | $24.99 | 17,199 | 80.1 | 0.2–0.5M | 2,420 |
| DRG: Survivor | Funday Games (Ghost Ship Publishing) | Unity | 2024-02-14 → 2025-09-17 | $12.99 | 46,584 | 86.7 | 1–2M | 1,866 |
| Windblown | Motion Twin (+Kepler Ghost) | Unity | 2024-10-24 → 1.0 TBA (2026 target) | $24.99 (sale $17.49) | 9,148 | 90.0 | 0.2–0.5M | 420 |
| Absolum | Guard Crush + Supamonks (Dotemu; Gamirror) | XNA (per PCGW) | no EA; 2025-10-09 | $24.99 | 9,574 | n/a | n/a | 327 |
| 20 Minutes Till Dawn | flanne | (not checked) | → 2023-06-08 | $4.99 | 27,936 | 91.5 | 1–2M | n/m |

**Halls of Torment (HoT)**
- **Studio and team:**
  - Chasing Carrots, based in Stuttgart, Germany. Team size is not stated.
  - Credits list 4 key people: creative director, producer, programmer and artist.
  - **Godot**, after switching from Unity (cited past challenges; Godot is free and open source).
  - Source: [Wikipedia](https://en.wikipedia.org/wiki/Halls_of_Torment)
  - Godot 4.3 at 1.0 — [HoT 1.0 post](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2218750&count=1&maxlength=0&feeds=steam_community_announcements&enddate=1727222400)
- **Timeline:**
  - Concept Oct 2022 → announced Jan 2023 → free demo Mar 2023 → EA 2023-05-24.
  - 1.0 on 2024-09-24.
  - Android/iOS 2024-10-10 (port by Erabit Studios).
  - PS5/Xbox 2025-10-28.
  - Source: [Wikipedia](https://en.wikipedia.org/wiki/Halls_of_Torment)
  - EA lasted about 17 months against a planned 6 — [HoT 1.0 post](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2218750&count=1&maxlength=0&feeds=steam_community_announcements&enddate=1727222400)
- **Sales:**
  - 70k+ units in the first week of EA; 1M+ on Steam (Oct 2024) — [Wikipedia](https://en.wikipedia.org/wiki/Halls_of_Torment)
  - The 1M milestone post came shortly after 1.0. A Supporter DLC was framed as a "test" of DLC integration before bigger paid content — [HoT Million post, 2024-10-17](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2218750&count=3&maxlength=0&feeds=steam_community_announcements&enddate=1729209600)
  - Paid DLC: **The Boglands** (2025-10-28, $3.99) and **The Lost Archives** (2026-10-22, $3.99) — [Steam appdetails 3919420](https://store.steampowered.com/api/appdetails?appids=3919420&cc=us); [4100210](https://store.steampowered.com/api/appdetails?appids=4100210&cc=us)
- **Hook and art:**
  - A "pre-rendered retro look reminiscent of RPGs from the late 90s", 30-minute runs and quest-based meta-progression — [Steam appdetails](https://store.steampowered.com/api/appdetails?appids=2218750&cc=us)
  - Pre-rendered isometric hellscapes in the style of Diablo, grainy sprites and an orb health display. The pitch: Diablo "three course meal" vs HoT "candy bar" — [Wikipedia](https://en.wikipedia.org/wiki/Halls_of_Torment)
- **Content now (store list):**
  - 6 stages, 11 characters, 25 blessings;
  - 60 unique items plus 240 higher-rarity variants;
  - 74 abilities and upgrades, 30 artifacts;
  - 35+ bosses, 70+ monsters;
  - **500 quests**, 1000+ traits.
  - Source: [Steam appdetails](https://store.steampowered.com/api/appdetails?appids=2218750&cc=us)
- **1.0 added:**
  - A final "Vault" stage with entry-cost modifiers.
  - The Shrine of Torment / **Agony** mode with 30 artifacts (harder for better loot).
  - The Scriptor NPC for permanent upgrades via Torment Shards.
  - Rank V traits; 500 quests in total.
  - Source: [HoT 1.0 post](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2218750&count=1&maxlength=0&feeds=steam_community_announcements&enddate=1727222400)
- **Reception:** Metacritic 88, OpenCritic 93% recommend, PC Gamer 90. Nominated for Mobile Game of the Year at the D.I.C.E. Awards — [Wikipedia](https://en.wikipedia.org/wiki/Halls_of_Torment)
- **Cadence:**
  - 105 Steam announcements. Regular "Experimental" beta updates continue into Oct 2026.
  - A public Codecks board tracks its work (open.codecks.io/hot).
  - Sources: [Steam news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2218750&count=200&maxlength=1&feeds=steam_community_announcements); [HoT 1.0 post](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2218750&count=1&maxlength=0&feeds=steam_community_announcements&enddate=1727222400)

**Vampire Survivors (VS)**
- **Origin, budget, engine:**
  - Made by poncle; Luca Galante began it in 2020 while unemployed, then hired freelancers in Mar 2022.
  - He spent about **£1,100 on assets, art and music** during the year-long EA.
  - Built in **Phaser**; Unity from v1.6 onward.
  - Source: [Wikipedia](https://en.wikipedia.org/wiki/Vampire_Survivors)
- **Why the Unity switch:**
  - Steam v1.6 (2023-08-17): "New engine & Local co-op".
  - Reasons given: porting and stability, better performance, features that were "very difficult or nearly impossible" on the old engine, the overhead of maintaining two engine versions, and co-op and cross-save costs.
  - They discarded a first working port to stay closer to the original gameplay feel.
  - The old engine survives only on a frozen password-protected branch.
  - Source: [VS Steam posts Jul–Aug 2023](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=1794680&count=5&maxlength=0&feeds=steam_community_announcements&enddate=1692316800)
- **Timeline and platforms:**
  - itch 2021-03-31; Steam EA 2021-12-17 — [Steam appdetails](https://store.steampowered.com/api/appdetails?appids=1794680&cc=us)
  - 1.0 2022-10-20; Xbox Nov 2022; mobile free-to-play with ads Dec 2022; Switch Aug 2023; PS Aug 2024; Meta Quest Nov 2025.
  - Local co-op Aug 2023; online co-op fully live Oct 2025.
  - Source: [Wikipedia](https://en.wikipedia.org/wiki/Vampire_Survivors)
- **Sales and awards:**
  - No official unit total.
  - 70k+ concurrent players in Feb 2022.
  - The *Sunday Times* estimated Galante's wealth at £40M (Aug 2024).
  - BAFTA Best Game 2023 and Evolving Game 2025.
  - Source: [Wikipedia](https://en.wikipedia.org/wiki/Vampire_Survivors)
- **DLC strategy (cheap DLC and licensed crossovers):**

  | DLC | Release | Price | Adds |
  |---|---|---|---|
  | Legacy of the Moonspell | Dec 2022 | | 13 weapons, 8 characters |
  | Tides of the Foscari | Apr 2023 | | 13 weapons, 8 characters |
  | Emergency Meeting (Among Us) | Dec 2023 | | |
  | Operation Guns (Contra) | May 2024 | | |
  | Ode to Castlevania | Oct 2024 | $3.99 | 80+16 characters, 100+ weapons |
  | Emerald Diorama (SaGa) | Apr 2025 | free | |
  | Ante Chamber (Balatro) | Oct 2025 | free | |
  | Legacy of the Bloodmoon | Aug 2026 | $0.99 | 12 characters, 18 weapons |

  Sources: [Wikipedia](https://en.wikipedia.org/wiki/Vampire_Survivors); prices: [appdetails 3210350](https://store.steampowered.com/api/appdetails?appids=3210350&cc=us), [4781330](https://store.steampowered.com/api/appdetails?appids=4781330&cc=us)
- **Spin-off:** *Vampire Crawlers* (deck-builder by Nosebleed Interactive) released 2026-04-21 at $9.99 with 19,007 recs — [Wikipedia](https://en.wikipedia.org/wiki/Vampire_Survivors); [appdetails](https://store.steampowered.com/api/appdetails?appids=3265700&cc=us)

**Brotato**
- **Developer and engine:** solo dev Thomas Gervraud (Blobfish), **Godot**. Evil Empire took over ongoing development in Sep 2025, with Blobfish keeping direction — [Wikipedia](https://en.wikipedia.org/wiki/Brotato)
- **Timeline:**
  - EA 2022-09-27 → 1.0 2023-06-23.
  - Mobile (Erabit) Mar 2023, Switch Aug 2023, PS/Xbox Jan 2024, Switch 2 Nov 2025.
  - Source: [Wikipedia](https://en.wikipedia.org/wiki/Brotato)
- **Sales:**
  - "Over 10 million" by 2025 per Wikipedia (likely all platforms incl. mobile; unverified breakdown).
  - Wikipedia also says 1M "as of September 2022", which looks like a date error given the EA launched 2022-09-27. Flag this.
  - Source: [Wikipedia](https://en.wikipedia.org/wiki/Brotato)
- **Hook:**
  - A potato wielding up to 6 auto-firing weapons.
  - Short waves of 20–90 seconds with a shop between waves; runs under 30 minutes.
  - "Dozens of characters" and "hundreds of items and weapons".
  - Local co-op for 4.
  - Source: [Steam appdetails](https://store.steampowered.com/api/appdetails?appids=1942280&cc=us)
- **1.0 update:** added 5 elites, 6 characters, 7 weapons, 6 items and 12 challenges, plus an endless-mode toggle — [Brotato 1.0 post, 2023-06-23](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=1942280&count=2&maxlength=0&feeds=steam_community_announcements&enddate=1687564800)
- **DLC:** Abyssal Terrors (2024-10-25, $3.99, shipped with local co-op); Primal Dread (2026-11-02, co-developed with Evil Empire) — [appdetails 2868390](https://store.steampowered.com/api/appdetails?appids=2868390&cc=us); [4498220](https://store.steampowered.com/api/appdetails?appids=4498220&cc=us)
- **Players:** the most concurrent players of any title checked: 6,576 at one moment on 2026-10-25 — [current players](https://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1/?appid=1942280)

**Hades and Hades II (Supergiant)**
- **Hades team and engine:** about 20 employees when it entered EA. The custom engine was rewritten in C++ on **The Forge**, replacing the C#/XNA engine used since Bastion — [Wikipedia Hades](https://en.wikipedia.org/wiki/Hades_(video_game))
- **Hades EA and sales:** EA on EGS (Dec 2018), Steam 2019-12-10, 1.0 2020-09-17. About 700k copies in EA, then 1M+ three days after 1.0 — [Wikipedia Hades](https://en.wikipedia.org/wiki/Hades_(video_game))
- **Hades systems:**
  - Boons are choices of 3 god-themed persistent effects per offering.
  - The narrative meta-progression was about 10 hours of dialogue, with event chains advancing per run.
  - Major awards include BAFTA Best Game and a Hugo.
  - Source: [Wikipedia Hades](https://en.wikipedia.org/wiki/Hades_(video_game))
- **Hades II timeline:**
  - Technical test Apr 2024, surprise EA May 2024.
  - **3 major EA updates:** Olympic (Oct 2024, also the macOS launch), Warsong (Feb 2025, final boss), Unseen (Jun 2025).
  - 1.0 2025-09-25 (Switch/Switch 2 timed exclusive), PS5/Xbox Apr 2026 (Game Pass).
  - Source: [Wikipedia Hades II](https://en.wikipedia.org/wiki/Hades_II)
- **Hades II content and reception:** 8 regions on two paths and 8 regional bosses. Metacritic 94 (PC), TGA 2025 Best Action — [Wikipedia Hades II](https://en.wikipedia.org/wiki/Hades_II)
- **Hades II presentation:** "hand-painted environments, even smoother real-time 3D characters"; dozens of fully voiced characters — [Steam appdetails](https://store.steampowered.com/api/appdetails?appids=1145350&cc=us)
- **Hades II EA patch cadence:** EA patches 6, 8, 10 and 11 between the major updates — [Hades II news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=1145350&count=200&maxlength=1&feeds=steam_community_announcements)

**Children of Morta**
- **Developer, publisher, engine:** Dead Mage, published by 11 bit studios; **Unity** (2017.4 → 2019.2) — [PCGamingWiki](https://www.pcgamingwiki.com/w/api.php?action=parse&page=Children_of_Morta&prop=wikitext&format=json&section=0)
- **Releases:** PC 2019-09-03, consoles Oct/Nov 2019, mobile Oct 2024 — [Wikipedia](https://en.wikipedia.org/wiki/Children_of_Morta)
- **Hook and content:** a story-driven family of up to 7 playable Bergsons; each levels up and grants improvements to the others; procedurally generated dungeons — [Wikipedia](https://en.wikipedia.org/wiki/Children_of_Morta)
- **Co-op:** local and online co-op — [Steam appdetails](https://store.steampowered.com/api/appdetails?appids=330020&cc=us)
- **Reception:** Metacritic PC 82 — [Wikipedia](https://en.wikipedia.org/wiki/Children_of_Morta)

**Soulstone Survivors**
- **Developer, engine, timeline:** Game Smithing Limited, **Unity**; EA 2022-11-07 → 1.0 2025-06-17 (~31 months) — [PCGamingWiki](https://www.pcgamingwiki.com/w/api.php?action=parse&page=Soulstone_Survivors&prop=wikitext&format=json&section=0)
- **1.0 launch and pricing:** 1.0 shipped together with PS5/Xbox. Price rose from $9.99 to $14.99 in May 2025, before 1.0 — [Soulstone posts Apr 2025](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2066020&count=2&maxlength=0&feeds=steam_community_announcements&enddate=1745452800)
- **Store claims:** "350+ skills", "over 100 unique weapons", "over a million players" — [Steam appdetails](https://store.steampowered.com/api/appdetails?appids=2066020&cc=us)
- **Cadence:**
  - 139 announcements, with named major updates every 1–3 months (e.g., Unholy Cathedral, Embrace of Chaos, Cursed Fleet, Titan Hunt, Void King).
  - Post-1.0 updates continued through Jun 2026.
  - Source: [news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2066020&count=200&maxlength=1&feeds=steam_community_announcements)

**Megabonk**
- **Developer, engine, development time:** solo dev "vedinad", **Unity**. Development began Aug 2024; release 2025-09-18 (about 13 months), delayed from Sep 4 to avoid clashing with Silksong — [Wikipedia](https://en.wikipedia.org/wiki/Megabonk)
- **Sales:** **1M+ copies in two weeks**, peak **117,336** concurrent players (SteamDB, per Wikipedia) — [Wikipedia](https://en.wikipedia.org/wiki/Megabonk)
- **Awards:** withdrew from TGA 2025 Best Debut Indie, saying the dev had made games before under other names — [Wikipedia](https://en.wikipedia.org/wiki/Megabonk)
- **Hook:** a 3D third-person "Vampire Survivors-like" on procedurally generated 3D maps with verticality — [Wikipedia](https://en.wikipedia.org/wiki/Megabonk)
- **Content:** 20 characters, 70+ items, 240 quests — [Steam appdetails](https://store.steampowered.com/api/appdetails?appids=3405340&cc=us). Critics flagged only 2 maps — [Wikipedia](https://en.wikipedia.org/wiki/Megabonk)
- **Price and languages:** $9.99, 17 languages — [appdetails](https://store.steampowered.com/api/appdetails?appids=3405340&cc=us)
- **Post-launch cadence:** only 12 Steam announcements in total; the last major patch was in Dec 2025 (Spooky/Christmas) — [news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=3405340&count=200&maxlength=1&feeds=steam_community_announcements); [Dec 2025 posts](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=3405340&count=4&maxlength=0&feeds=steam_community_announcements)
- **Players:** 4,496 concurrent at one moment (2026-10-25) — [current players](https://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1/?appid=3405340)

**Ravenswatch**
- **Developer, engine, timeline:** Passtech Games (makers of Curse of the Dead Gods), published by Nacon. Engine listed as "**OEngine**" (in-house). EA 2023-04-06 → 1.0 2024-09-26 — [PCGamingWiki](https://www.pcgamingwiki.com/w/api.php?action=parse&page=Ravenswatch&prop=wikitext&format=json&section=0)
- **Hook and content at 1.0:**
  - Up to 4-player online co-op with 9 fairy-tale heroes.
  - 3 open maps and a "3 days and 3 nights" timer.
  - 50+ enemies, 200+ talents, 50 magical items, 4 difficulty levels.
  - Source: [Steam appdetails](https://store.steampowered.com/api/appdetails?appids=2071280&cc=us)
- **Post-1.0 model:**
  - Free updates plus hero DLC: Romeo & Juliet (free, May 2025), **Merlin** ($7.99, 2026-01-19), **Kintarō** ($7.99, 2026-10-12).
  - Cosmetic packs ($3.99) and a 2026 roadmap.
  - Sources: [news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2071280&count=200&maxlength=1&feeds=steam_community_announcements); [appdetails 3607950](https://store.steampowered.com/api/appdetails?appids=3607950&cc=us); [4809870](https://store.steampowered.com/api/appdetails?appids=4809870&cc=us); [Hourglass update post](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2071280&count=1&maxlength=0&feeds=steam_community_announcements&enddate=1759104000)
- **Reception:** 80.1% positive, the lowest among those checked — [SteamSpy](https://steamspy.com/api.php?request=appdetails&appid=2071280)

**Deep Rock Galactic: Survivor (DRG:S)**
- **Developer, engine:** Funday Games, published by Ghost Ship Publishing; **Unity** — [Wikipedia](https://en.wikipedia.org/wiki/Deep_Rock_Galactic:_Survivor)
- **Timeline:** EA 2024-02-14 → 1.0 2025-09-17 (+Xbox); mobile Nov 2025; PS5/Switch Oct 2026 — [Wikipedia](https://en.wikipedia.org/wiki/Deep_Rock_Galactic:_Survivor)
- **Sales:** **500k+ in the first week of EA; 1M within a month** — [Wikipedia](https://en.wikipedia.org/wiki/Deep_Rock_Galactic:_Survivor)
- **Content:** 4 classes × 10 weapons, 3 special roles per class, and 5 biomes × 5 hazard levels — [Wikipedia](https://en.wikipedia.org/wiki/Deep_Rock_Galactic:_Survivor)
- **1.0 additions:**
  - Gear: 6 types × 4 rarities.
  - 300 achievements.
  - Escort missions.
  - Twitch-chat integration.
  - Save slots.
  - Source: [DRG:S 1.0 posts](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2321470&count=2&maxlength=0&feeds=steam_community_announcements&enddate=1758153600)
- **Post-launch:**
  - Quarterly "Roadmap Update" posts in 2024 and a price increase in Dec 2024.
  - **Heavy Duty Expansion** ($9.99, 2026-04-30); Knockout Expansion announced.
  - Sources: [news list](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2321470&count=200&maxlength=1&feeds=steam_community_announcements); [appdetails 4395500](https://store.steampowered.com/api/appdetails?appids=4395500&cc=us)

**Windblown (Motion Twin, the makers of Dead Cells)**
- **Engine and publishing:** **Unity**; co-published with Kepler Ghost — [PCGamingWiki](https://www.pcgamingwiki.com/w/api.php?action=parse&page=Windblown&prop=wikitext&format=json&section=0)
- **Timeline:** announced at TGA 2023; alpha with about 300 users in Feb 2024; EA 2024-10-24; 1.0 "anticipated 2026" — [Wikipedia](https://en.wikipedia.org/wiki/Windblown_(video_game))
- **Latest status:** the **"0.9999999" update (2026-10-22) is the 10th and final EA update**; no 1.0 date yet — [latest posts](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=1911610&count=2&maxlength=0&feeds=steam_community_announcements)
- **Hook and content (store):**
  - 1–3 player co-op; dual-wield "Alterattacks"; dash with no cooldown.
  - 25+ weapons, 20 trinkets, 105 passives, 20 hexes, 3 magifish, 6 biomes in EA, 170+ unlockables.
  - Sources: [Steam appdetails](https://store.steampowered.com/api/appdetails?appids=1911610&cc=us); [Wikipedia](https://en.wikipedia.org/wiki/Windblown_(video_game))
- **Anniversary update (Oct 2025):** added a biome, a boss, 10 enemies + 10 variants and 4 weapons. The 2026 roadmap leads to 1.0 — [anniversary post](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=1911610&count=2&maxlength=0&feeds=steam_community_announcements&enddate=1761264000)
- **Players:** 420 concurrent at one moment on 2026-10-25 despite a sale — [current players](https://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1/?appid=1911610)

**Absolum (Dotemu / Guard Crush / Supamonks)**
- **Team:** a roguelite beat-'em-up. Guard Crush is the Streets of Rage 4 team; Supamonks did the art and animation; composer Gareth Coker with guest composers — [Wikipedia](https://en.wikipedia.org/wiki/Absolum_(video_game))
- **Engine:** "XNA" per PCGamingWiki — [PCGamingWiki](https://www.pcgamingwiki.com/w/api.php?action=parse&page=Absolum&prop=wikitext&format=json&section=0)
- **Releases:** 2025-10-09 on PC, PS and Switch; Xbox Mar 2026; Switch 2 Sep 2026; mobile Nov 2026 — [Wikipedia](https://en.wikipedia.org/wiki/Absolum_(video_game))
- **Sales:** 200k in the first week; 500k by end of Jan 2026 — [Wikipedia](https://en.wikipedia.org/wiki/Absolum_(video_game)). The dev post phrases it as "500,000 Rebels" (copies vs players unstated) — [Absolum post 2026-01-27](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=1904480&count=1&maxlength=0&feeds=steam_community_announcements&enddate=1769558400)
- **Structure:**
  - **4 heroes**; non-random levels with chosen paths.
  - Randomized "Rituals" (passives) lost on death, plus boss-unlocked "Inspirations" (active skills).
  - 2-player co-op; cross-platform multiplayer since May 2026.
  - Source: [Wikipedia](https://en.wikipedia.org/wiki/Absolum_(video_game))
- **Reception:** Metacritic 87–90, OpenCritic 97% recommend — [Wikipedia](https://en.wikipedia.org/wiki/Absolum_(video_game))

### Inferences
- **Engine choice among small teams is overwhelmingly Unity or Godot.**
  - Unity: DMD, DRG:S, Soulstone, Megabonk, Windblown, Children of Morta, VS after 2023.
  - Godot: HoT (after leaving Unity), Brotato.
  - Custom engines appear only at established studios: Supergiant (C++/The Forge), Passtech (OEngine), Guard Crush (XNA lineage).
  - VS proves a web stack (Phaser) can launch a mega-hit. Its own team still migrated for consoles, performance, co-op and maintenance. That is directly relevant to a "maximally native stack" decision.
- **Price is a positioning tool, not a quality signal, in this subgenre.**
  - $4.99–9.99 games (VS, Brotato, HoT, Megabonk, DMD) dominate review volume.
  - Premium $25–30 titles need studio pedigree or a strong IP (Hades, Windblown, Absolum, Ravenswatch).
- **Survivors-likes sell on IP and memes:** DRG:S (an existing IP: 1M in a month) and Megabonk (meme virality: 1M in 2 weeks) show fast launches. HoT shows a slow-burn path: 70k in week 1 → 1M at about 17 months.
- **Retention leaders combine steady post-launch cadence with cheap DLC:**
  - Brotato: ~monthly patches, a co-dev studio and $3.99 DLC.
  - VS: regular DLC/crossovers.
  - Soulstone: frequent named updates.
  - Ravenswatch: hero DLC.
  - By contrast, Megabonk's thin post-launch cadence and DMD's year-plus gaps align with smaller live audiences relative to their sales.
- **DMD vs Halls of Torment is the closest head-to-head:** same dark-fantasy ARPG-survivor niche, similar price, similar owner estimates. HoT shipped 1.0 in 17 months, then mobile, consoles and paid DLC. DMD has no 1.0, no consoles and no localization after 35 months. HoT also has about 5× DMD's concurrent players today. HoT's pre-rendered-3D art pipeline appears to let 4 key people produce 11 characters and 35+ bosses.

### Gaps
- Team sizes for Funday Games, Game Smithing Limited, Passtech, Motion Twin (Windblown) and Dead Mage: not found in reachable sources.
- Hades/Hades II boon specifics (number of Olympians, Duo/Legendary boons, Heat/Fear difficulty systems): not stated on the fetched Wikipedia pages, so not cited here.
- Hades II unit sales: no official figure found (SteamSpy 2–5M owners is an estimate).
- Ravenswatch and Windblown unit sales: no official figures found.
- Megabonk total sales after the first month: not found.
- Art-pipeline specifics:
  - Windblown, Ravenswatch and DRG:S appear 3D, but this is not confirmed in fetched text.
  - Absolum's hand-drawn technique is credited to Supamonks without process details.
- Children of Morta's team size and Kickstarter history: not covered by the fetched sources.
- 20 Minutes Till Dawn's engine and team: not checked.

## 5. Cross-cutting: typical content volumes, dev timelines and team sizes for successful indies in this subgenre

### Takeaway
A successful 1.0 in this space typically ships with:
- **roughly 8–20 characters** (4 for premium/hand-animated titles);
- **dozens to hundreds of skills/weapons/boons**;
- **50–100 enemy types with 10–35 bosses**;
- **3–8 biomes/stages**;
- plus a long tail of quests/achievements (200–500) to drive retention.

Typical paths:
- Tiny teams (1–5 people) take **7–13 months to EA or launch**.
- They then spend **10–20 months in EA**. DMD's 35+ months is an outlier.
- Studio titles with 15–25+ people take 2–3+ years.

### Cited Findings
**Content volumes**
- **Characters at or after 1.0:**

  | Game | Characters | Source |
  |---|---|---|
  | HoT | 11 | [Steam](https://store.steampowered.com/api/appdetails?appids=2218750&cc=us) |
  | Megabonk | 20 | [Steam](https://store.steampowered.com/api/appdetails?appids=3405340&cc=us) |
  | Ravenswatch (at 1.0) | 9 | [Steam](https://store.steampowered.com/api/appdetails?appids=2071280&cc=us) |
  | DRG:S | 4 classes | [Wikipedia](https://en.wikipedia.org/wiki/Deep_Rock_Galactic:_Survivor) |
  | Absolum | 4 heroes | [Wikipedia](https://en.wikipedia.org/wiki/Absolum_(video_game)) |
  | Children of Morta | 7 family members | [Wikipedia](https://en.wikipedia.org/wiki/Children_of_Morta) |
  | Brotato | "dozens" | [Steam](https://store.steampowered.com/api/appdetails?appids=1942280&cc=us) |
  | DMD (live) | about 8 | Section 2 |

- **Skills, weapons, items:**

  | Game | Volume | Source |
  |---|---|---|
  | HoT | 74 abilities/upgrades; 60 uniques + 240 rarity variants; 1000+ traits; 25 blessings | [Steam](https://store.steampowered.com/api/appdetails?appids=2218750&cc=us) |
  | Soulstone | 350+ skills; 100+ weapons | [Steam](https://store.steampowered.com/api/appdetails?appids=2066020&cc=us) |
  | DRG:S | 40 weapons (4 × 10) | [Wikipedia](https://en.wikipedia.org/wiki/Deep_Rock_Galactic:_Survivor) |
  | Ravenswatch | 200+ talents; 50 items | [Steam](https://store.steampowered.com/api/appdetails?appids=2071280&cc=us) |
  | Windblown | 25+ weapons; 105 passives; 20 trinkets; 20 hexes | [Steam](https://store.steampowered.com/api/appdetails?appids=1911610&cc=us) |
  | Megabonk | 70+ items | [Steam](https://store.steampowered.com/api/appdetails?appids=3405340&cc=us) |
  | DMD | "hundreds" of blessings | [Steam](https://store.steampowered.com/app/2334730/Death_Must_Die/) |

- **Enemies and bosses:**

  | Game | Enemies / bosses | Source |
  |---|---|---|
  | HoT | 70+ monsters; 35+ bosses | [Steam](https://store.steampowered.com/api/appdetails?appids=2218750&cc=us) |
  | Ravenswatch | 50+ enemies | [Steam](https://store.steampowered.com/api/appdetails?appids=2071280&cc=us) |
  | Hades II | 8 regional bosses | [Wikipedia](https://en.wikipedia.org/wiki/Hades_II) |
  | DMD (FAQ, 2 acts) | 6 bosses | [Steam](https://store.steampowered.com/app/2334730/Death_Must_Die/) |
  | Brotato 1.0 update alone | 5 elites | [Brotato 1.0](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=1942280&count=2&maxlength=0&feeds=steam_community_announcements&enddate=1687564800) |

- **Biomes and maps:**

  | Game | Biomes / maps | Source |
  |---|---|---|
  | Megabonk | 2 (criticized as too few) | [Wikipedia](https://en.wikipedia.org/wiki/Megabonk) |
  | Ravenswatch | 3 open maps | [Steam](https://store.steampowered.com/api/appdetails?appids=2071280&cc=us) |
  | DMD | 3 acts | Section 2 |
  | DRG:S | 5 biomes | [Wikipedia](https://en.wikipedia.org/wiki/Deep_Rock_Galactic:_Survivor) |
  | HoT | 6 stages | [Steam](https://store.steampowered.com/api/appdetails?appids=2218750&cc=us) |
  | Windblown | 6 biomes (EA) + 1 | [Steam](https://store.steampowered.com/api/appdetails?appids=1911610&cc=us) |
  | Hades II | 8 regions | [Wikipedia](https://en.wikipedia.org/wiki/Hades_II) |

- **Retention long tails:**
  - HoT: 500 quests — [Steam](https://store.steampowered.com/api/appdetails?appids=2218750&cc=us)
  - Megabonk: 240 quests — [Steam](https://store.steampowered.com/api/appdetails?appids=3405340&cc=us)
  - DRG:S: 300 achievements — [DRG:S 1.0](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2321470&count=2&maxlength=0&feeds=steam_community_announcements&enddate=1758153600)
  - DMD: 59 achievements — [appdetails](https://store.steampowered.com/api/appdetails?appids=2334730&cc=us)
  - HoT Agony mode with 30 artifacts — [HoT 1.0](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2218750&count=1&maxlength=0&feeds=steam_community_announcements&enddate=1727222400)
  - Absolum player-made "Mystic Ordeals" — [Absolum 1.1 post](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=1904480&count=1&maxlength=0&feeds=steam_community_announcements&enddate=1769558400)
  - DRG:S Twitch integration — [DRG:S 1.0](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2321470&count=2&maxlength=0&feeds=steam_community_announcements&enddate=1758153600)

**Timelines**
- HoT: concept Oct 2022 → EA May 2023 (about 7 months) → 1.0 Sep 2024 (17 months EA vs a planned 6) — [Wikipedia](https://en.wikipedia.org/wiki/Halls_of_Torment); [HoT 1.0](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2218750&count=1&maxlength=0&feeds=steam_community_announcements&enddate=1727222400)
- Megabonk: about 13 months, solo, no EA — [Wikipedia](https://en.wikipedia.org/wiki/Megabonk)
- VS: started 2020 → itch Mar 2021 → Steam EA Dec 2021 → 1.0 Oct 2022 — [Wikipedia](https://en.wikipedia.org/wiki/Vampire_Survivors); [Steam](https://store.steampowered.com/api/appdetails?appids=1794680&cc=us)
- Brotato: about 10 months EA — [Brotato 1.0](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=1942280&count=2&maxlength=0&feeds=steam_community_announcements&enddate=1687564800)
- Hades: about 21 months EA — [Wikipedia](https://en.wikipedia.org/wiki/Hades_(video_game))
- Hades II: about 16.5 months EA, with 3 majors — [Wikipedia](https://en.wikipedia.org/wiki/Hades_II)
- DRG:S: 19 months EA — [Wikipedia](https://en.wikipedia.org/wiki/Deep_Rock_Galactic:_Survivor)
- Ravenswatch: about 17.5 months EA — [PCGW](https://www.pcgamingwiki.com/w/api.php?action=parse&page=Ravenswatch&prop=wikitext&format=json&section=0)
- Soulstone: about 31 months EA — [PCGW](https://www.pcgamingwiki.com/w/api.php?action=parse&page=Soulstone_Survivors&prop=wikitext&format=json&section=0)
- Windblown: 24+ months EA and counting, 10 EA updates — [Steam](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=1911610&count=2&maxlength=0&feeds=steam_community_announcements)
- DMD: 35+ months and counting, with Act 4 in 2027 and 5 acts planned — [Act 4 News](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)

**Team sizes**
- Solo:
  - VS initially (later freelancers) — [Wikipedia](https://en.wikipedia.org/wiki/Vampire_Survivors)
  - Brotato (later Evil Empire co-dev) — [Wikipedia](https://en.wikipedia.org/wiki/Brotato)
  - Megabonk — [Wikipedia](https://en.wikipedia.org/wiki/Megabonk)
- 4–5 people:
  - DMD (3 artists + 2 programmers) — [Act 4 News](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)
  - HoT (4 key credits) — [Wikipedia](https://en.wikipedia.org/wiki/Halls_of_Torment)
- About 20 people: Supergiant (Hades) — [Wikipedia](https://en.wikipedia.org/wiki/Hades_(video_game))
- Three companies: Absolum — [Wikipedia](https://en.wikipedia.org/wiki/Absolum_(video_game))

**Ports and outsourcing patterns**
- Erabit Studios ported both Brotato and HoT to mobile — [Wikipedia Brotato](https://en.wikipedia.org/wiki/Brotato); [Wikipedia HoT](https://en.wikipedia.org/wiki/Halls_of_Torment)
- Evil Empire co-develops Brotato content — [Wikipedia](https://en.wikipedia.org/wiki/Brotato)
- DMD's freelancer experience (onboarding overhead) — [Act 4 News](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=3&maxlength=0&feeds=steam_community_announcements)

### Inferences
- **"Beat DMD" bar for a solo+AI developer (inference):**
  - At least 6–8 heroes and at least 8 god/school-equivalents with status-effect identities.
  - 150–300+ upgrade options, 3+ biomes and about 10 bosses.
  - A loot layer with persistent stash and loadouts, plus a difficulty ladder.
  - Day-one localization (5–10+ languages; HoT and Megabonk ship 17), an endless mode and a visible roadmap.
  - The art/animation budget is the binding constraint, not code.
- **Fastest proven solo/tiny paths keep art cheap or automatable:**
  - VS: about £1,100 of assets.
  - Brotato: a potato with floating weapons, no directional animation.
  - HoT: pre-rendered 3D.
  - Megabonk: low-poly 3D.
  - DMD's and Dead Cells-style ambitions (hand-drawn, many directions) multiply cost. DMD itself reports a 12× animation budget increase and slipping dates. A 3D-model → rendered-sprite pipeline (HoT; Dead Cells is commonly reported to use a similar 3D-to-pixel approach, but that was not verified in this session) is the likely best lever for a solo developer wanting rich animation.
- **EA strategy implications:**
  - Successful EAs here run about 10–20 months with predictable major updates: Hades II had 3 majors, DRG:S quarterly roadmap updates, Windblown 10 updates in 24 months.
  - Long silences (DMD) correlate with sentiment decay.
  - A price increase at or before 1.0 is common and accepted (Soulstone $9.99→$14.99; DRG:S Dec 2024).
- **Co-op is a recurring differentiator** (VS, Brotato, Ravenswatch, Windblown, Absolum, CoM). DMD explicitly can't add it because of its architecture. A new codebase should decide on co-op/determinism before writing the core.

### Gaps
- No systematic dataset of content counts at 1.0 (as opposed to "now") exists for most titles. Store pages show current totals, and 1.0 posts often list only deltas (e.g., Brotato).
- Typical budgets (USD) for these games are not public apart from VS's anecdotal £1,100 asset spend.
- Claims about Dead Cells' 3D-to-2D pipeline were not verified in this session; another researcher may cover it.
- SteamDB all-time peaks and Gamalytic/VG Insights revenue estimates for the comparison set were not accessible (blocked or search budget exhausted).
