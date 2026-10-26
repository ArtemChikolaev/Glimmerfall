# Retention, in-game progression and cosmetics in action roguelites: research notes for a design playbook

*Compiled 2026-10-25 for a planned original-IP pixel-art action roguelite inspired by Death Must Die (DMD) and Dead Cells. Steam store pages, Steam global achievement percentages and wikis were fetched on 2026-10-25 unless another date is given. **[search summary]** marks a fact taken from a web-search result summary whose page I did not open myself; treat those as lower confidence. The shared web-search budget ran out partway through, so I filled the remaining gaps only by fetching known URLs. Everything still unknown is listed under "Gaps".*

---

## 1. Run and session design: run length, mid-run save/quit, fast restart, how "one more run" is engineered

### Takeaway
Runs in genre hits are deliberately short. Survivors-likes run about 15–30 min (Vampire Survivors soft-caps a stage at 15/20/30 min; Brotato is "under 30 minutes"; Halls of Torment is about 30 min; DMD is about 15 min). Hades-style action roguelites run about 30–45 min. Supergiant's stated reason: a run that lasts hours hides the part of the loop that makes the genre "cool". The "one more run" pull comes from making death productive (story beats, currency, unlocks, quest ticks) and from heavily juiced reward reveals. The slot-machine side of that juice has drawn rating trouble and critical scrutiny.

### Cited Findings
- **Designer on run length (Greg Kasavin, Supergiant):** "If you're playing for five hours before restarting, then you're not experiencing the cool part." (Inverse interview, quoted 2021-08-11) — [Can I Play That](https://caniplaythat.com/2021/08/11/hades-god-mode-explained-by-supergiant-games)
- **Hades run length:** reviews put one escape attempt at roughly 30 min on average for a successful run, and about 40 min per run in the Switch launch review. Supergiant has published no official target. — [GameBanshee review](https://www.gamebanshee.com/3hyyf), [DJCube review](https://djcube.co.uk/review-hades/) [search summary]
- **Session overrun:** a Hades fan essay describes planning a 30-min session and playing 2 hours because of "just one more run". — [The Supergiant Run | Hades (Substack)](https://oyito.substack.com/p/the-supergiant-run-hades) [search summary]
- **Vampire Survivors:** a soft time limit of 15, 20 or 30 min depending on the stage. At the limit a Reaper spawns, and another arrives every minute after that. — [Wikipedia: Vampire Survivors](https://en.wikipedia.org/wiki/Vampire_Survivors)
- **Brotato:** the store lists "Fast runs (under 30 minutes)" and waves of 20–90 seconds (fetched 2026-10-25). — [Steam: Brotato](https://store.steampowered.com/app/1942280/Brotato/)
- **Halls of Torment:** "Quick and casual 30 minute runs." — [Steam: Halls of Torment](https://store.steampowered.com/app/2218750/Halls_of_Torment/)
- **Death Must Die:** in aggregated review analysis, typical runs take about 15 min and players reach the endgame after about 7 hours. The store says the base content lasts "roughly 5-10 hours depending on skill and luck". — [Vaporlens DMD summary](https://vaporlens.app/app/2334730/death_must_die.md); [Steam: Death Must Die](https://store.steampowered.com/app/2334730/Death_Must_Die/)
- **Dead Cells Daily Challenge is a micro-session:** a 4 min 30 s time limit on a single daily map, with a score posted on death or on finishing. — [Dead Cells Wiki: Daily Challenge](https://deadcells.wiki.gg/wiki/Daily_Challenge)
- **Risk of Rain 2 runs often pass 30 min:** 77.3% of players hold "The Lone Survivor" (survive 30 consecutive minutes). — [Steam global achievements: RoR2](https://steamcommunity.com/stats/632360/achievements)
- **Making death part of the structure (Hades):** Kasavin said the team wanted no "game over" feeling, chose "many channels of permanent progression and an ongoing narrative that evolves" between runs, and organised the work around "take the sting out of failure". Comic characters such as Hypnos and Skelly were partly picked to soften deaths. — [Tom's Guide interview](https://www.tomsguide.com/uk/features/hades-exclusive-interview-supergiant) [search summary]
- **Hades death loop:** death returns Zagreus to the House of Hades. Obols and some upgrades are lost, while another currency buys permanent upgrades. Dialogue events also unlock hub improvements. — [Wikipedia: Hades](https://en.wikipedia.org/wiki/Hades_(video_game))
- **Why Supergiant chose a roguelike:** Pyre had shown that most players finish a game only once, so branching narrative needed repeat playthroughs built into the structure. — [Wikipedia: Hades](https://en.wikipedia.org/wiki/Hades_(video_game))
- **Risk tension between runs (Dead Cells):** Cells from enemies can only be spent at the end of a dungeon section, and dying first loses them. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- **Use-it-or-lose-it hub ritual (Rogue Legacy 2):** a guide notes that Charon takes all leftover gold before the next run, so players spend everything at the manor. — [Magnetic Mag RL2 tips](https://magneticmag.com/2022/06/five-tips-for-rogue-legacy-2/) [search summary]
- **Reward-reveal juice and gambling roots:** Luca Galante worked in the gambling industry before Vampire Survivors, and his slot-machine graphics experience informed the chest-opening animations. — [Wikipedia: Vampire Survivors](https://en.wikipedia.org/wiki/Vampire_Survivors)
- A Stanford HCI-blog critical play (Seamus Joseph Allen, 2024-05-22) describes how a VS chest works:
  - Play pauses, possible rewards scroll past like a slot machine, a gold counter climbs, and the music builds tension.
  - It quotes Galante, via The Verge, that in slot design "[the player] is actually spending money every time they press it", which explains the care over sound and animation.
  - It argues that "almost every failed attempt feels like a near miss", which drives retries.
  - Source: [Mechanics of Magic: Critical Play — Vampire Survivors](https://mechanicsofmagic.com/?p=31948)
- **Risk of gambling optics:**
  - PEGI re-rated Balatro to 18+ for "prominent gambling imagery" and it was temporarily pulled in some territories. An appeal reclassified it as PEGI 12 in February 2025.
  - LocalThunk wrote into his will that the game must not be used for gambling.
  - Source: [Wikipedia: Balatro](https://en.wikipedia.org/wiki/Balatro)
- **Low-stakes framing:** LocalThunk wanted Balatro to feel "evergreen, comforting, and enjoyable in a very low-stakes way", like solitaire or a crossword (blog summarised 2025-02-28). — [80.lv](https://80.lv/articles/localthunk-shares-the-most-important-inspiration-of-balatro)
- **Saving (accessibility guidance):** the Game Accessibility Guidelines list "Provide an autosave feature" (Intermediate) and "Provide a manual save feature" (Intermediate). — [Game Accessibility Guidelines](https://gameaccessibilityguidelines.com/full-list/)

### Inferences
- **Session target for a DMD/Dead Cells hybrid:**
  - Full runs of about 20–35 min, split into 3–4 acts or biomes of about 6–12 min, each ending in a hub-like checkpoint (shop or forge).
  - DMD's ~15-min runs and Vampire Survivors' 15–30-min caps show that shorter, completable sessions suit couch and Steam Deck play.
- **Day-one session features:**
  - Suspend-save on quit at any room or biome transition.
  - One-button restart from the death screen.
  - A death screen that always shows (a) persistent progress earned, (b) progress bars toward the next unlock or quest, and (c) one teaser of a locked item or character.
- **Make every death productive:** every run, even a failed one, should advance at least one persistent track: currency, codex entries, quest or bounty counters, or a story beat. Hades, Dead Cells (Cells and blueprints) and Halls of Torment (500 quests) all do this.
- **Be careful copying the VS slot-machine chest:** the same juice (pause, scrolling rewards, counting gold) draws critical scrutiny, and Balatro shows that gambling-adjacent presentation can trigger rating problems.
  - Keep reveals exciting but avoid casino iconography: coins, reels, card-table symbols.
  - Never pair reveals with real-money purchases.

### Gaps
- No primary Supergiant, Motion Twin, Mega Crit or LocalThunk statement giving a target run length. I found no sourced run lengths for Slay the Spire, Balatro or Dead Cells.
- I could not source each game's mid-run save behaviour (Hades, Dead Cells, Balatro, StS, RoR2, DMD); this needs hands-on verification.
- No public A/B or telemetry evidence on which "one more run" hooks (death screen, unlock teasers, near-miss feedback) measurably raise retention.

---

## 2. Meta-progression taxonomy and trade-offs; avoiding grind walls; fair dailies and leaderboards

### Takeaway
The hits mix five kinds of meta-progression:
- **Power:** permanent stats.
- **Variety:** content added to the pools.
- **Knowledge/access:** runes, shortcuts, new routes.
- **Narrative.**
- **Cosmetic.**

Pure-skill purists praise variety-only unlocks (Slay the Spire, Balatro). Mass-market hits add bounded, reversible power (Vampire Survivors refundable PowerUps, Hades' Mirror) plus optional assist power (God Mode). Grind complaints concentrate where power is slow, capped and costly to respec (DMD's talent tree). Competitive modes stay fair by stripping meta power: fixed seeds and rules, assist and custom runs excluded.

### Cited Findings
**Power (permanent stats)**
- **Vampire Survivors PowerUps:** 28 shop entries.
  - Examples: Might 5 ranks from 200 gold; Amount 1 rank at 5,000; Revival 10,000; Seal I–III/All 10 ranks each from 10,000.
  - Pricing: each rank costs the initial price × the rank number. After the first purchase every rank adds a fee of floor(20 × 1.1^TotalBought). Since v0.7.2 fees are additive, so purchase order doesn't matter.
  - A "Refund PowerUps" button returns all ranks and gold, and maxed PowerUps can be toggled off (for example to drop Curse).
  - Source: [VS Wiki: PowerUps](https://vampire.survivors.wiki/w/PowerUps)
- **Vampire Survivors refund policy:** a 2021 developer post said players can reset purchases "without losing anything", but characters are not refundable. — [Steam forum](https://steamcommunity.com/app/1794680/discussions/0/3201493834358639726) [search summary]
- **Vampire Survivors other power sources:** gold also unlocks characters, and golden eggs give permanent character boosts. — [Wikipedia: Vampire Survivors](https://en.wikipedia.org/wiki/Vampire_Survivors)
- **Hades:** the Mirror of Night makes Zagreus stronger between runs, so growing player knowledge and character power together carry players further. — [Tom's Guide](https://www.tomsguide.com/uk/features/hades-exclusive-interview-supergiant) [search summary]
- **Dead Cells Cells:** Cells are spent on permanent upgrades, and only at the end of a section. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- **Death Must Die:** gear is randomly generated, carries over between runs, and has rarity tiers and affixes; each hero has a talent tree. — [Steam: DMD](https://store.steampowered.com/app/2334730/Death_Must_Die/); [Vaporlens](https://vaporlens.app/app/2334730/death_must_die.md)
- **DMD talent pace:** player reports from April–May 2024 give 4 talent points per complete run and a maximum of 36 per hero. The talent tree replaced the earlier per-hero perks in the Act 2 update (April 2024). — [Steam discussions](https://steamcommunity.com/app/2334730/discussions/0/4358997268258492140), [Steam discussions](https://steamcommunity.com/app/2334730/discussions/0/4357871582654485912) [search summary]
- **DMD meta-progression sentiment:**
  - Early critics said there was little meta-progression beyond gear, and the developer disagreed.
  - Later players described a farmable "skill tree … that makes you incredibly overpowered".
  - Aggregated reviews flag the talent-tree reset cost as an annoyance and say the skill-tree cap reduces long-term interest.
  - Sources: [Steam discussions](https://steamcommunity.com/app/2334730/discussions/0/4036976577927254244) [search summary]; [Vaporlens](https://vaporlens.app/app/2334730/death_must_die.md)
- **Rogue Legacy 2:** you "[spend] your parents' inheritance and grow the family manor". Taking it slow lets you "build your character up until you're ready for greater challenges". — [Steam: Rogue Legacy 2](https://store.steampowered.com/app/1253920/Rogue_Legacy_2/)
- **Rogue Legacy 2 cost scaling:** a review notes each upgrade raises the cost of the next, which "gets costly pretty fast". — [The Workprint review](https://www.theworkprint.com/rogue-legacy-2-review-bigger-bolder-better/123) [search summary]
- **Halls of Torment:** quest-based meta with 500 quests. Unique items can be "sent up to the overworld" to customise future runs, and 25 blessings make you stronger each run. — [Steam: Halls of Torment](https://store.steampowered.com/app/2218750/Halls_of_Torment/)

**Variety (lateral unlocks into pools)**
- **Balatro:**
  - 105 of 150 Jokers are available from the first run; 45 unlock through conditions, some spanning several runs.
  - Decks and per-deck stakes unlock by completing runs.
  - Sources: [Balatro Wiki: Jokers](https://balatrowiki.org/w/Jokers); [Balatro Wiki: Stakes](https://balatrowiki.org/w/Stakes)
- **Slay the Spire:**
  - Each character has five unlock levels that add cards and relics, and these also reach other characters' runs. — [hamatti notes, 2024-12-31](https://notes.hamatti.org/Gaming/Video-games/Meta-progression-with-gradual-tutorial-in-roguelike-games)
  - Players cite it as a model where "no power is ever given via meta progression" and unlocks only teach the game. — [Tiny Rogues Steam forum](https://steamcommunity.com/app/2088570/discussions/2/3361397532265670884) [search summary]
- **Dead Cells blueprints:** blueprints drop in dungeons and must be carried out of the level to unlock new items. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- **Risk of Rain 2:**
  - In-game challenges unlock survivors, items, abilities and modifiers (Artifacts). — [Wikipedia: RoR2](https://en.wikipedia.org/wiki/Risk_of_Rain_2)
  - Each survivor has 2–4 challenges that unlock a skill variant or alternate skin, and the game has more than 75 items to unlock. — [GamesRadar](https://www.gamesradar.com/risk-of-rain-2/cheats/) [search summary]

**Knowledge/access**
- **Dead Cells runes:** runes are permanent traversal upgrades taken from Elite enemies. Each requires the previous one and opens new routes, which is Metroidvania-style access progression. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)

**Critiques and design cautions**
- **Calibration:** a design explainer says meta-progression must be tuned so it doesn't trivialise the skill-based core. — [Bugnet blog](https://bugnet.io/blog/how-to-design-a-roguelite-meta-progression) [search summary]
- **Variety over power:** a developer blog argues for unlocking variety rather than power: "a veteran should have more options than a new player, but not be 10x stronger". — [StraySpark blog](https://www.strayspark.studio/blog/building-roguelike-ue5-procedural-progression) [search summary]
- **Player critique:** a widely shared post argues that meta-progression "remov[es] the sense of improvement", while still liking sideways unlocks that ease players in or add variety. — [PieFed thread](https://piefed.keyboardvagabond.com/c/games/p/232999/meta-progression-in-roguelites-was-fun-for-a-while-but-it-s-starting-to-f) [search summary]
- **Book:** *The Rise of the Roguelite* (Cartlidge, 2025) has design chapters on what makes a good roguelite and on synergies between upgrades. — [Eludamos review](https://eludamos.org/index.php/eludamos/article/download/8183/8977/38877) [search summary]

**Anti-grind tools**
- **Vampire Survivors:** refundable, toggleable PowerUps (see above).
- **Dead Cells Custom Mode** (v1.1, December 2018):
  - Choose starting weapon, shield and bow.
  - Remove unwanted items from shops and chests.
  - Unlock items not yet found, and relock them later.
  - Source: [PCGamesN](https://www.pcgamesn.com/dead-cells/custom-game-mode-patch)
- **Dead Cells Assist Mode** (June 2022) — see §9.
- **Rogue Legacy 2 "House Rules":** toggle contact damage, adjust global damage and health, and grant flight for hard platforming. — [Steam: RL2](https://store.steampowered.com/app/1253920/Rogue_Legacy_2/)
- **Brotato:** adjustable enemy health, damage and speed. — [Steam: Brotato](https://store.steampowered.com/app/1942280/Brotato/)
- **Death Must Die, November 21 [2023] patch:** goal was to make Difficulty 30 "consistently beatable with a wider range of builds"; enemy health bonuses were cut 73% and attack-damage bonuses 38%. — [Prima Games](https://primagames.com/tips/death-must-die-november-21-update-full-patch-notes-listed) [search summary]

**Fairness of dailies and leaderboards when meta power exists**
- **Dead Cells Daily Challenge:**
  - Unlocked by the Challenger Rune.
  - Gear and stat scrolls are all found inside the run.
  - Fixed 4:30 time limit and score-based ranking.
  - Assist Mode runs are not posted to leaderboards.
  - Rewards are blueprints for the 1st, 5th and 10th cumulative completions (one per day).
  - Source: [Dead Cells Wiki: Daily Challenge](https://deadcells.wiki.gg/wiki/Daily_Challenge)
- **Slay the Spire Daily Climb:** added 2018-03-01 with one shared seed per day plus run modifiers; leaderboards came later. — [Into the Spine](https://intothespine.com/2018/03/05/slay-the-spire-daily-climb/)
- **Slay the Spire Custom Mode:** disables achievements, leaderboards and Neow's starting bonuses, and does not count toward the daily leaderboard. — [StS Wiki: Custom Mode](https://slaythespire.wiki.gg/wiki/Custom_Mode) [search summary]
- **Slay the Spire 2 daily:**
  - Fixed character, set Ascension level and three modifiers each day.
  - Players report that only the first attempt is submitted, to stop rerolling.
  - Sources: [Bossdown guide](https://bossdown.com/guides/slay-the-spire-2-daily-climb-guide/), [Steam forum](https://steamcommunity.com/app/2868840/discussions/0/591810592556252896/) [search summary]
- **Risk of Rain 2 Eclipse:**
  - Artifacts (modifiers) are disabled.
  - In multiplayer the Eclipse level is set to the lowest level in the lobby.
  - Source: [RoR2 Wiki: Eclipse](https://riskofrain2.wiki.gg/wiki/Eclipse)
- **Balatro challenge decks:** playable only on White Stake. — [Balatro Wiki: Stakes](https://balatrowiki.org/w/Stakes)

### Inferences
- **Recommended mix for the new game:**
  - Make *variety* (gods, skills, items, heroes entering pools) the main unlock currency.
  - Add a small, capped *power* layer, roughly the first 10–15 hours, as an onboarding ramp and not the endgame.
  - Make power fully refundable and toggleable (VS model); DMD's respec cost is a recurring complaint.
- **Prefer access progression for a roguevania hybrid:** Dead Cells-style runes and shortcuts give permanent "I can go further" progress without inflating stats.
- **Separate assist power from ladder power:** an optional God-Mode-style assist (see §9) lets struggling players progress without raising everyone's baseline.
- **Fair competitive modes from day one:**
  - Dailies and weeklies use a fixed hero and loadout or in-run-only power.
  - Seed and rules are identical for all players, and only the first attempt counts.
  - Assist and custom runs are excluded from boards; meta power is ignored or normalised.
  - Rewards are cosmetic or lateral unlocks (Dead Cells' daily blueprints).

### Gaps
- No primary interview text on Hades' Mirror of Night or keepsake design (the Hades fandom wiki returned HTTP 402; the Tom's Guide page did not render).
- No quantitative evidence on how power-meta versus variety-meta affects retention or reviews.

---

## 3. Unlock pacing and onboarding over the first 10–20 hours

### Takeaway
Successful games start with a large but not complete pool: Balatro opens with 70% of its Jokers. They then use achievement-like conditions and run completions as a guided curriculum that unlocks new mechanics, ladders and modes in sequence. Advocates frame unlocks as "you are now ready for this" signals rather than rewards. DMD is criticised for weak onboarding (no tutorial, unclear progression and difficulty sources), which is a direct lesson for a DMD-inspired game.

### Cited Findings
- **Gradual-tutorial principles** (hamatti notes, 2024-12-31, building on Mark Brown's "inverted pyramid of decision making"):
  - Meta-progression should hold back advanced mechanics until players are ready.
  - Unlocks should come quickly, perhaps based on runs rather than achievements.
  - Unlocks should be advanced mechanics or complex synergies.
  - Unlocks should be automatic, not bought with meta-currency.
  - Offer an "enable everything" option for veterans.
  - Ascension "keeps the game loop the same but lengthens the enjoyment of the game drastically over time".
  - Source: [hamatti notes](https://notes.hamatti.org/Gaming/Video-games/Meta-progression-with-gradual-tutorial-in-roguelike-games)
- **Balatro's meta layer:** LocalThunk names achievements, stake levels, unlocks and challenges as the meta-game layer solitaire lacks, and links these systems to the game's long playtime (2025-02-28). — [80.lv](https://80.lv/articles/localthunk-shares-the-most-important-inspiration-of-balatro)
- **Balatro stake wins unlock decks:** Red unlocks Zodiac, Green Painted, Black Anaglyph, Blue Plasma and Orange Erratic; 105 of 150 Jokers are available from run one. — [Balatro Wiki: Stakes](https://balatrowiki.org/w/Stakes); [Balatro Wiki: Jokers](https://balatrowiki.org/w/Jokers)
- **Balatro demo iteration:** the demo was pulled on 2024-01-01 and returned 2024-01-25 with nearly 40 new Jokers, new bosses and booster packs; beta feedback shaped features such as boss blinds. — [Wikipedia: Balatro](https://en.wikipedia.org/wiki/Balatro)
- **Vampire Survivors achievements as unlocks:**
  - 243 Steam achievements (203 base, 40 DLC; the page's section header is inconsistent and says 140 base). The easiest, "Hollow Heart — Survive 1 minute with any character", is held by 97.8%.
  - Item-named achievements sit at 87–93%, e.g. "Garlic — Find 5 Floor Chickens" 93.4% and "Clover — Find a Little Clover" 87.1%.
  - Source: [Steam global achievements: VS](https://steamcommunity.com/stats/1794680/achievements)
- **Vampire Survivors DLC unlocks:** the Emergency Meeting DLC adds 20 in-game-only unlocks with no Steam achievements. — [Steam: Emergency Meeting](https://store.steampowered.com/app/2690330/Vampire_Survivors_Emergency_Meeting/)
- **Slay the Spire Ascension gating:** Ascension unlocks per character after beating any Act 3 boss; each win unlocks the next level, and any unlocked level stays selectable. "Ascend 0 — Unlock Ascension mode" is held by 66.4%. — [StS Wiki: Ascension](https://slaythespire.wiki.gg/wiki/Ascension); [Steam global achievements: StS](https://steamcommunity.com/stats/646570/achievements)
- **Hades:** the Pact of Punishment (difficulty ladder) unlocks only after defeating Hades once. — [Wikipedia: Hades](https://en.wikipedia.org/wiki/Hades_(video_game)); [RPGSite](https://www.rpgsite.net/feature/10287-hades-pact-of-punishment-heat-modifiers-and-how-to-maximize-your-rewards)
- **Dead Cells gates modes behind in-run milestones:** the Daily Challenge needs the Challenger Rune from The Concierge (held by 72.8%), and Boss Stem Cell levels open new doors and biomes. — [Dead Cells Wiki: Daily Challenge](https://deadcells.wiki.gg/wiki/Daily_Challenge); [Steam global achievements: Dead Cells](https://steamcommunity.com/stats/588650/achievements); [Dead Cells Wiki: BSC](https://deadcells.wiki.gg/wiki/Boss_Stem_Cells)
- **Dead Cells and community feedback:**
  - The first Early Access build was about 30–40% complete but had tight combat.
  - Lead designer Sébastien Bénard estimated that 40–50% of the final game's features came from Early Access feedback.
  - Source: [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- **Risk of Rain 2 secret characters:** Paul Morse described the Heretic as "a truly hidden character" meant to be discovered naturally, which is why it was not made selectable from a menu. — [Prima Games interview](https://primagames.com/gaming/risk-of-rain-2-interview-paul-morse) [search summary]
- **DMD onboarding complaints:**
  - Players report no tutorial, confusing progression, an unclear UI and unclear sources of difficulty.
  - Reliance on the external wiki scores 50 in aggregated sentiment.
  - Source: [Vaporlens](https://vaporlens.app/app/2334730/death_must_die.md)
- **Slay the Spire 2** (Early Access 2026-03-05): progression unlocks "fragments of the Spire's timeline and lore through repeated climbs". — [Steam: Slay the Spire 2](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)

### Inferences
- **Proposed pacing:**
  - About 60–70% of items and skills available on run one.
  - A burst of 5–10 trivial unlocks in the first hour (VS's 97.8% "survive 1 minute" pattern).
  - Then one new mechanic or system roughly every 1–2 hours up to hour ~10: new god, hero, rune or biome branch, difficulty ladder after the first full win, daily after a mid-game boss.
  - From hour ~10–20 onward, the ladder and collections take over.
- **Make unlock conditions readable:** show them in-game (a Balatro-style collection with condition text), so the player always knows the next 1–3 goals; this answers DMD's clarity complaints.
- **Day-one onboarding checklist:**
  - A 2–3 minute interactive tutorial.
  - Tooltips explaining difficulty sources and stat math.
  - A practice or sandbox room (see §9).
  - An "unlock all" toggle for veterans and content creators.

### Gaps
- No published unlock-timing curves (hours-to-unlock X) for any of these games.
- No first-run tutorial research (completion of tutorials or first-hour churn) was found.

---

## 4. Difficulty ladders: structure, level count, named rules versus numeric scaling, effect on playtime

### Takeaway
The canonical ladders have 5–20 cumulative tiers:
- Dead Cells: 5 Boss Stem Cells.
- Balatro: 8 stakes.
- Risk of Rain 2: 8 Eclipse levels.
- Slay the Spire: 20 Ascension levels.
- Hades: a free-form menu of ~15–16 named conditions worth about 60+ Heat.
- DMD: point-buy "skulls" up to Difficulty 30.

The best-regarded ladders mix numeric scaling with named rules that change decisions: fewer heals, fewer choices, Eternal/Rental Jokers, double bosses, lethal fall damage. Progress is usually tracked per character or deck, multiplying content life. Steam data shows only about 5–15% of players reach the top, but they are the long-tail audience.

### Cited Findings
- **Slay the Spire Ascension:** 20 cumulative levels, unlocked per character; each Act 3 win unlocks the next. The levels:
  1. More elites (~60% more)
  2. Normal enemies deal more damage
  3. Elites deal more damage
  4. Bosses deal more damage
  5. Heal only 75% after bosses
  6. Start each run with 10% HP missing
  7. Normal enemies get more HP/Block
  8. Elites get more HP/Block
  9. Bosses get more HP/Block
  10. Start with the Ascender's Bane curse
  11. One fewer potion slot
  12. Upgraded cards appear half as often in Acts 2–3
  13. Bosses drop 25% less gold
  14. Lower max HP
  15. Worse event outcomes
  16. Shop prices +10%
  17. Normal enemies get tougher movesets
  18. Elites get tougher movesets
  19. Bosses get tougher movesets
  20. Two bosses at the end of Act 3

  Source: [StS Wiki: Ascension](https://slaythespire.wiki.gg/wiki/Ascension)
- **Hades Pact of Punishment** (RPGSite, 2020-09-25):
  - Unlocked after first beating Hades.
  - Named conditions with ranks, for example:
    - Hard Labor: 5 ranks, +20% enemy damage per rank
    - Lasting Consequences: −25% healing per rank
    - Convenience Fee: +40% shop prices
    - Jury Summons: +20% enemies
    - Extreme Measures: arms the bosses
    - Forced Overtime: enemies 20% faster
    - Routine Inspection: disables Mirror talents
    - Approval Process: −1 boon choice
    - Tight Deadline: per-region timer
  - Heat is tracked per weapon. Each new Heat level per weapon pays one set of Titan Blood, Diamonds and Ambrosia, and skipping levels forfeits the skipped rewards.
  - Source: [RPGSite](https://www.rpgsite.net/feature/10287-hades-pact-of-punishment-heat-modifiers-and-how-to-maximize-your-rewards)
- **Hades maximum Heat — sources disagree:**
  - RPGSite's table (15 conditions) sums to 61 by my calculation.
  - FAQ aggregators claim 63 outside Hell Mode and say bounties stop paying after about Heat 20 per weapon.
  - Sources: [calendar-uk FAQ](https://www.calendar-uk.co.uk/frequently-asked-questions/how-many-hours-is-100-hades) [search summary; low-quality aggregator]
- **Dead Cells Boss Stem Cells:** 5 cumulative levels, toggled in the starting area.
  - Each is earned by beating the final boss with the current number active; Aspects block earning.
  - BSC1 (Hard): fewer Health Fountains, forge cap raised.
  - BSC2 (Very Hard): no fountains, 1 flask charge per passage, 2× cell drops.
  - BSC3 (Expert): +1 item level, cursed-chest upgrades.
  - BSC4 (Nightmare): enemies detect from farther and teleport, no passage flasks, 3× cells, +3 item levels.
  - BSC5 (Hell): adds Malaise.
  - BSC levels also gate doors and biomes, new blueprints, enemy-exclusive drops and endgame areas such as the Astrolab and Collector.
  - Source: [Dead Cells Wiki: BSC](https://deadcells.wiki.gg/wiki/Boss_Stem_Cells)
- **Balatro stakes:** 8 cumulative stakes, unlocked per deck by winning the current stake. Each also unlocks a new deck.
  1. White: base
  2. Red: Small Blind gives no reward money
  3. Green: score requirement scales faster
  4. Black: Eternal Jokers can appear
  5. Blue: −1 discard
  6. Purple: score requirement scales faster again
  7. Orange: Perishable Jokers
  8. Gold: Rental Jokers

  Wins put a stake-coloured sticker on every Joker used and on the deck. — [Balatro Wiki: Stakes](https://balatrowiki.org/w/Stakes)
- **Risk of Rain 2 Eclipse:** 8 cumulative levels on Monsoon base difficulty, tracked per survivor.
  - E1: allies start at half health
  - E2: teleporter radius halved
  - E3: fall damage doubled and lethal
  - E4: enemies 40% faster
  - E5: ally healing halved
  - E6: enemies drop 20% less gold
  - E7: enemy cooldowns halved
  - E8: permanent max-HP damage
  - Artifacts are disabled. Clearing E8 earns a gold icon on the select screen; there are "no unlocks or logbook entries".
  - Source: [RoR2 Wiki: Eclipse](https://riskofrain2.wiki.gg/wiki/Eclipse)
- **Death Must Die:**
  - Difficulty modifiers are set at the Star Crux in the hub; harder settings increase item rewards, and players count difficulty in "skulls" with a top level of 30. — [Steam discussions](https://steamcommunity.com/app/2334730/discussions/0/4036976577927254244) [search summary]
  - The November 21 [2023] patch retuned Difficulty 30 to be beatable by more builds. — [Prima Games](https://primagames.com/tips/death-must-die-november-21-update-full-patch-notes-listed) [search summary]
  - Steam: "A Stake For A Vampire — Defeat the Baron" 67.9%; "Obsidian — Win at 5 Difficulty" 43.8%; "A Black Heart — Win at 10 Difficulty" 38.6%. — [Steam global achievements: DMD](https://steamcommunity.com/stats/2334730/achievements)
- **Rogue Legacy 2 New Game+:** "you choose how the game gets harder" through biome hazards or special boss fights. — [Steam: RL2](https://store.steampowered.com/app/1253920/Rogue_Legacy_2/)
- **Ladders and playtime:**
  - LocalThunk links stakes and other meta systems to Balatro's playtime. — [80.lv](https://80.lv/articles/localthunk-shares-the-most-important-inspiration-of-balatro)
  - Mega Crit's GDC 2019 talk is summarised as crediting weekly updates, Discord feedback, Ascension levels and streamer insights with shaping balance and engagement. — [Podwise summary](https://podwise.ai/episodes/1459444), [Glasp summary](https://glasp.co/youtube/7rqfbvnO_H0) [search summary of third-party talk summaries]
- **Ladder funnels on Steam:**
  - StS: Ironclad win 56.0% → Ascension 10 14.8% → Ascension 20 7.3%. — [Steam: StS](https://steamcommunity.com/stats/646570/achievements)
  - Balatro: win a run 71.6% → Red+ 43.7% → Black+ 30.3% → Gold 12.2%. — [Steam: Balatro](https://steamcommunity.com/stats/2379780/achievements)
  - Dead Cells (API names):
    - First Hand of the King kill 40.4%
    - Boss cells: 1st 41.1%, 2nd 23.0%, 3rd 15.2%, 4th 11.8%, 5th 8.5%
    - Collector (5-BSC final boss) 5.8%
    - Source: [Steam API: Dead Cells](https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v0002/?gameid=588650&format=json)
  - Hades (API names; display names not captured): AchClearHeatGate 17.1%, AchPactUpgradesClears 7.7%. — [Steam API: Hades](https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v0002/?gameid=1145360&format=json)

### Inferences
- **Ladder design for the new game:** 8–12 named, cumulative tiers, tracked per hero.
  - The first tier unlocks immediately after the first full clear.
  - About half the tiers should change decisions: fewer heals or flasks, one fewer reward choice, elite frequency, curse at start, double final boss. The rest should be numeric.
  - Each tier pays a visible cosmetic badge (Balatro stickers, RoR2 gold icon, Dead Cells BSC-tier boss outfits) and occasionally gates content (Dead Cells doors and biomes).
- **For DMD-style point-buy difficulty:** keep it, but ship a curated "ladder" preset path alongside it, since free-form menus confuse newcomers (§3). Tie item-quality rewards to difficulty as DMD and Dead Cells (forge/S-items) do.
- **Expected funnel:** about 30–45% of buyers clear once; about 10–20% enter the ladder seriously; about 5–10% reach the top. Budget content accordingly.

### Gaps
- No primary source for Hades' exact max Heat or per-condition list as of the final patch (RPGSite may omit a condition).
- The DMD difficulty modifier list and reward formula are undocumented; the DMD wiki was not found and the community discussions are dated 2024.
- No data on how much ladders extend median playtime; only achievement-based proxies.

---

## 5. Build variety and synergy design: pool sizes, rarity, discovery, reroll/banish/skip, bad-luck protection, "broken but balanced"

### Takeaway
Content benchmarks:
- Balatro: 150 Jokers.
- Slay the Spire: ~75 cards per character.
- Hades: >100 boon effects (by achievement proxy).
- Halls of Torment: 60 items × rarity variants plus 1,000+ traits.
- Brotato: "hundreds" of items and weapons.

Typical rarity ratios are about 70/25/5 plus special-source legendaries. Synergy discovery works best with explicit recipes (Vampire Survivors evolutions) and named combos. Pool-control tools (reroll, skip, banish, seal, Custom Mode) are themselves unlocks. Balance comes from telemetry (pick rate, win rate) and from tolerating rare overpowered combos in single-player while nerfing anything that "cannibalises" adjacent strategies.

### Cited Findings
- **Balatro Joker pool:**
  - 150 Jokers: Common 61, Uncommon 64, Rare 20, Legendary 5.
  - Shop odds are Common 70%, Uncommon 25%, Rare 5%. Legendaries never appear in shops; they come only from The Soul card, which has a 0.3% chance to appear in Spectral/Arcana packs.
  - Editions: Foil +50 Chips, Holographic +10 Mult, Polychrome ×1.5 Mult, Negative +1 Joker slot.
  - Source: [Balatro Wiki: Jokers](https://balatrowiki.org/w/Jokers)
- **LocalThunk on balance** (Rogueliker, 2024-03-07):
  - Jokers were tuned to "encourage varied gameplay while still remaining interesting".
  - "Sometimes I release a card and observe that it's just too good and cannibalises all the adjacent strategies around it."
  - "almost all balance changes can be made by changing a number (cost, rarity, chip bonus, mult bonus, etc)."
  - "There is a lot of randomness, possibly too much, but the metastrategy for Balatro is around mitigating risk."
  - Source: [Rogueliker interview](https://rogueliker.com/balatro-interview/)
- **Slay the Spire telemetry:**
  - Early Access targeted about 75 cards per character pool.
  - The team balanced with in-client metrics: rarely picked cards were judged too weak, and cards appearing too often in winning decks were judged overpowered.
  - Giovannetti credited streamers, who "probably don't even know you're there most of the time".
  - Source: [Wikipedia: Slay the Spire](https://en.wikipedia.org/wiki/Slay_the_Spire)
- **Mega Crit GDC 2019 talk** (as summarised):
  - The team used an internal Slack channel and a metrics server, and warned that metrics can mislead.
  - "Every card should have its place."
  - Being single-player lets them keep rare overpowered combos without harming others.
  - Sources: [Podwise](https://podwise.ai/episodes/1459444), [Glasp](https://glasp.co/youtube/7rqfbvnO_H0) [search summary]
- **Slay the Spire 2 telemetry:** its Early Access collects anonymised metrics and includes an in-game feedback tool. — [Steam: StS2](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)
- **Vampire Survivors evolutions (discoverable recipes):**
  - Requirements: base weapon at max level, its counterpart passive, then an evolution chest, usually from bosses at or after 10:00.
  - Unions merge maxed weapons and free slots; Gifts and Morphs are further categories.
  - Source: [VS Wiki: Evolution](https://vampire.survivors.wiki/w/Evolution)
- **Vampire Survivors launch roadmap:** 24 characters, 48 weapons and evolutions, 20 power-ups, 5 regular and 5 special stages. — [PC Invasion](https://www.pcinvasion.com/?p=315397) [search summary]
- **Vampire Survivors pool-control tools, all bought as meta PowerUps:**
  - Reroll: new level-up options, 2 per rank, 10 at max.
  - Skip: take XP instead of a choice.
  - Banish: remove an item for the rest of the run.
  - Seal I–III/All: permanently banish 1–4 items from level-up pools via the Collection menu.
  - Source: [VS Wiki: PowerUps](https://vampire.survivors.wiki/w/PowerUps)
- **Dead Cells pool dilution fixes:**
  - Custom Mode (December 2018) removes unwanted items from shops and chests. — [PCGamesN](https://www.pcgamesn.com/dead-cells/custom-game-mode-patch)
  - v1.5 enlarged the Flask Room "to accommodate for the growing item pool" and added broken flasks to show items locked via Custom Mode. — [Nintendo Everything](https://nintendoeverything.com/dead-cells-update-out-now-version-1-5-corrupted-update/) [search summary]
  - The developers wanted players to try new weapon and skill combinations rather than settle on one, cited *The Binding of Isaac* as an item-driven influence, and tuned balance in Early Access "so no playstyle was unfairly punished". — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- **Hades breadth (achievement proxies):**
  - "Blessed by the Gods — Choose 100 different Olympian Boon effects" 33.0%, which implies ≥100 boon effects.
  - "Tools of the Architect — Choose 50 different Daedalus Hammer enchantments" 20.2%.
  - The Pact condition "Approval Process" (−1 boon choice) uses the choice count itself as a difficulty lever.
  - Sources: [Steam: Hades](https://steamcommunity.com/stats/1145360/achievements); [RPGSite](https://www.rpgsite.net/feature/10287-hades-pact-of-punishment-heat-modifiers-and-how-to-maximize-your-rewards)
- **Halls of Torment:** 60 unique items plus 240 higher-rarity variants, 74 abilities and upgrades, 1,000+ traits, 30 artifacts, 11 characters and 35+ bosses. — [Steam: Halls of Torment](https://store.steampowered.com/app/2218750/Halls_of_Torment/)
- **Brotato:** "dozens of characters", "hundreds of items and weapons", up to 6 weapons at once. — [Steam: Brotato](https://store.steampowered.com/app/1942280/Brotato/)
- **Death Must Die:**
  - 8 gods offering "hundreds" of blessings, 6 heroes with talent trees, and gear with rarity tiers and affixes.
  - Finding relics empowers Death's minions, a built-in risk/reward trade-off.
  - Players praise build variety but cite RNG-dependent builds and balance as frequent frustrations.
  - Sources: [Steam: DMD](https://store.steampowered.com/app/2334730/Death_Must_Die/); [Vaporlens](https://vaporlens.app/app/2334730/death_must_die.md)
- **Risk of Rain 2:** more than 75 items to unlock. — [Wikipedia/GamesRadar via search](https://www.gamesradar.com/risk-of-rain-2/cheats/) [search summary]

### Inferences
- **Launch content targets for a DMD/Dead Cells hybrid:**
  - ~6–8 "gods" or boon sources with ~15–25 boons each (≈120–150 boons), counting Hades-style duo or cross-god synergies separately.
  - ~40–60 weapons and skills.
  - ~60–100 items with 3–4 rarity tiers and affixes (Diablo-like, as DMD does).
  - 4–6 heroes at launch, growing to 8–10.
  - Expand through updates and DLC, Vampire Survivors-style.
- **Rarity:** start near Balatro's 70/25/5 and gate legendaries behind special events or sources rather than raw odds, so they feel earned.
- **Explicit synergy recipes:** show recipes like VS evolutions or Hades duo boons in a codex once discovered, with silhouettes of undiscovered combos as goals.
- **Pool control:** ship reroll, skip and banish as early meta unlocks, add a late "seal" tool, and offer a custom-run mode. Pool dilution from unlocks is a known Dead Cells problem.
- **Telemetry from the first playtest:** log pick rate and win-rate-when-picked per boon, item and weapon, as Mega Crit did. Fix outliers by "changing a number" (LocalThunk), and allow rare broken combos in single-player.

### Gaps
- No sourced numbers for Hades' total boon or duo-boon count, Dead Cells' weapon count, or Death Must Die's item and blessing counts.
- No sourced bad-luck-protection (pity) mechanics; the StS rarity-offset page returned 404, so this is unverified.

---

## 6. Long-term features and their evidence: dailies, leaderboards, achievements, codex, challenge runs, endless, events, mods, co-op

### Takeaway
The long tail in this genre comes from free updates and cheap DLC over years, plus dailies, collections and achievements, endless and boss-rush modes, and, increasingly in 2023–2026, co-op. Examples:
- Dead Cells: 35 patches and 4 paid DLC; over 10M copies by June 2023.
- Vampire Survivors: 8 DLC from 2022 to 2026.
- Balatro: free cosmetic crossover updates.

About a quarter of owners engage with dailies, judging by achievements. Modding (Steam Workshop) created at least one spin-off product.

### Cited Findings
- **Dailies:**
  - Dead Cells: 23.4% completed at least one daily run (API name FEAT_COMPLETE_1_DAILYRUN). — [Steam API: Dead Cells](https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v0002/?gameid=588650&format=json)
  - Slay the Spire: "My Lucky Day — Win a Daily Climb" 24.0%. — [Steam: StS](https://steamcommunity.com/stats/646570/achievements)
  - A 2019 Motion Twin producer listed "speed running daily challenges and leaderboards" among wished-for post-launch content. — [GamingBolt interview](https://gamingbolt.com/dead-cells-interview-post-launch-content-gameplay-mechanics-and-the-studios-next-project) [search summary]
- **Achievement counts:**
  - Vampire Survivors 243; Risk of Rain 2 205; Dead Cells 84; Death Must Die 59; Hades II 50; Hades 49; Slay the Spire 46; Balatro 31.
  - These are page-header totals; the VS and RoR2 pages show inconsistent section counts.
  - Sources: Steam global achievement pages ([VS](https://steamcommunity.com/stats/1794680/achievements), [RoR2](https://steamcommunity.com/stats/632360/achievements), [Dead Cells](https://steamcommunity.com/stats/588650/achievements), [DMD](https://steamcommunity.com/stats/2334730/achievements), [Hades II](https://steamcommunity.com/stats/1145350/achievements), [Hades](https://steamcommunity.com/stats/1145360/achievements), [StS](https://steamcommunity.com/stats/646570/achievements), [Balatro](https://steamcommunity.com/stats/2379780/achievements))
  - LocalThunk treats achievements as part of the meta-game that guides exploration. — [80.lv](https://80.lv/articles/localthunk-shares-the-most-important-inspiration-of-balatro)
- **Collections and codex as goals:**
  - Balatro: discover every Tarot 60.9%, every Spectral 29.6%, every Planet 21.2%, every Voucher 6.0%. — [Steam: Balatro](https://steamcommunity.com/stats/2379780/achievements)
  - RoR2: "Bookworm — collect 10 monster or environment logs" 62.9%. — [Steam: RoR2](https://steamcommunity.com/stats/632360/achievements)
  - Dead Cells: collection-count unlocks, with 16 outfits unlocking Sewing Scissors and 51 unlocking Giant Comb. — [Dead Cells Wiki: Outfits](https://deadcells.wiki.gg/wiki/Outfits)
- **Challenge runs:**
  - Balatro: "Rule Bender — complete any challenge run" 14.0%. — [Steam: Balatro](https://steamcommunity.com/stats/2379780/achievements)
  - Dead Cells in-run Challenge Rifts: 48.2%. — [Steam: Dead Cells](https://steamcommunity.com/stats/588650/achievements)
  - RoR2 unlocks Artifacts (run modifiers) through challenges. — [Wikipedia: RoR2](https://en.wikipedia.org/wiki/Risk_of_Rain_2)
- **Endless and extra modes:**
  - Balatro's Endless continues past ante 8 until scores overflow to NaN around ante 39. — [Wikipedia: Balatro](https://en.wikipedia.org/wiki/Balatro)
  - Vampire Survivors has Endless, Hyper and Limit Break modifiers. — [Wikipedia: VS](https://en.wikipedia.org/wiki/Vampire_Survivors)
  - Dead Cells added Boss Rush on 2022-10-26. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- **Post-launch cadence:**
  - **Dead Cells:**
    - Free updates: Rise of the Giant (2019), Practice Makes Perfect (2021-09-16), Everyone Is Here (2021-11-22), Break the Bank (2022-03), Boss Rush (2022-10-26), Everyone Is Here Vol. 2 (2022-11).
    - Paid DLC: The Bad Seed (2020-02-11), Fatal Falls (2021-01-26), The Queen and the Sea (2022-01-07), Return to Castlevania (2023-03-06).
    - The 35th patch (2024-08-19) ended creative work.
    - Sales: 2M (May 2019), 5M (March 2021), 10M+ (June 2023).
    - Source: [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
  - **Vampire Survivors DLC** (prices as listed on 2026-10-25, possibly discounted):
    - Legacy of the Moonspell (2022-12-15, €1.09)
    - Tides of the Foscari (2023-04-13, €1.99)
    - Emergency Meeting (Among Us, 2023-12-18, €2.49)
    - Operation Guns (Contra, 2024-05-09, €2.49)
    - Ode to Castlevania (2024-10-31, €3.99)
    - Emerald Diorama (SaGa, 2025-04-10, free)
    - Ante Chamber (Balatro, 2025-10-28, free)
    - Legacy of the Bloodmoon (2026-08-28, €1.09)
    - Source: [Steam: VS DLC](https://store.steampowered.com/dlc/1794680/); [Wikipedia: VS](https://en.wikipedia.org/wiki/Vampire_Survivors)
  - **Slay the Spire:** weekly patches throughout Early Access (November 2017 to January 2019). — [Wikipedia: StS](https://en.wikipedia.org/wiki/Slay_the_Spire)
  - **Hades:** about 2 years of Early Access with a team of about 20. — [Wikipedia: Hades](https://en.wikipedia.org/wiki/Hades_(video_game))
  - **Hades II:** about 16 months of Early Access with three major updates (2024-10-16, 2025-02-19, 2025-06-17), 1.0 on 2025-09-25. — [Wikipedia: Hades II](https://en.wikipedia.org/wiki/Hades_II)
  - **Risk of Rain 2:** paid DLC in 2022, 2024, 2025 and 2026 (€14.99 each). — [Steam: RoR2 DLC](https://store.steampowered.com/dlc/632360/)
- **Co-op trend:**
  - Vampire Survivors added local co-op for up to 4 players (2023-08-17), online co-op beta (August 2025) and full online co-op (October 2025). — [Wikipedia: VS](https://en.wikipedia.org/wiki/Vampire_Survivors)
  - Slay the Spire 2 launched Early Access (2026-03-05) with up to 4-player co-op and co-op-specific cards. — [Steam: StS2](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)
  - RoR2 supports up to 4 online players. — [Wikipedia: RoR2](https://en.wikipedia.org/wiki/Risk_of_Rain_2)
  - Brotato has local co-op for up to 4 players. — [Steam: Brotato](https://store.steampowered.com/app/1942280/Brotato/)
  - Cult of the Lamb's free co-op update "Unholy Alliance" was well received. — [Well-Played](https://www.well-played.com.au/cult-of-the-lamb-is-taking-a-break-from-free-updates-but-there-is-more-content-on-the-way-creative-director-teases-new-features/)
  - DMD is single-player only. — [Steam: DMD](https://store.steampowered.com/app/2334730/Death_Must_Die/)
- **Mods:**
  - Slay the Spire added Steam Workshop support in early 2019, and the fan mod *Downfall* later inspired a board-game expansion (announced September 2025). — [Wikipedia: StS](https://en.wikipedia.org/wiki/Slay_the_Spire)
  - Brotato lists Steam Workshop. — [Steam: Brotato](https://store.steampowered.com/app/1942280/Brotato/)
- **Cross-promotion:** DMD sells bundles with *20 Minutes Till Dawn*, *Brotato* and *Sworn* (7 bundles in total). — [Steam: DMD](https://store.steampowered.com/app/2334730/Death_Must_Die/)
- **Communication risk:** DMD's most-weighted negative theme is "lack of updates and communication" (weight 0.31), with fears of abandonment. — [Vaporlens](https://vaporlens.app/app/2334730/death_must_die.md)

### Inferences
- **Priority for a solo developer (highest ROI first):**
  1. A codex/collection with silhouettes and unlock conditions, plus Steam achievements mapped to unlocks (VS, Balatro).
  2. A daily seeded run with a fixed hero and loadout and a leaderboard (Dead Cells, StS); about 23–24% of owners engage.
  3. Endless and boss-rush modes (cheap reuse of content).
  4. A visible public roadmap with regular update notes. DMD shows that perceived silence is the top review complaint.
  5. Local co-op before online co-op (VS sequence); online co-op is a large networking and QA cost.
  6. Mod support later (data-driven content definitions make it cheaper).
- **Seasonal events:** no evidence they matter in premium roguelites; low priority.

### Gaps
- No evidence was found on seasonal or limited-time events in premium roguelites.
- No numbers on how dailies or co-op change retention or sales.
- Mod scene sizes (Workshop item counts, Thunderstore downloads) were not collected.

---

## 7. Cosmetics in premium roguelites: earned, crossover and paid

### Takeaway
The accepted norm is mostly earned cosmetics tied to achievement and difficulty:
- Dead Cells: ~150 outfits, including per-boss tiers per Boss Stem Cell level and "Flawless" variants.
- Risk of Rain 2: Mastery skins for beating Monsoon.
- Balatro: stake stickers.
- Hades: House Contractor decor.

Free crossover cosmetics work as marketing (Balatro's Friends of Jimbo, Dead Cells' Everyone Is Here). Small, cheap, clearly optional paid cosmetic packs are well received when sold alongside substantial free content:
- Cult of the Lamb packs: 91–96% positive.
- Halls of Torment Supporter Pack: 95% positive.

The worst reception in this sample was a content DLC tied to a troubled update (RoR2 Seekers of the Storm, 55% positive), not cosmetics.

### Cited Findings
**Earned cosmetics**
- **Dead Cells outfits:**
  - About 150 outfits (approximate count of the wiki list; the wiki gives no total) from enemy blueprints (~0.4% or 1.7% drop rates, often gated by BSC level), boss drops (up to 6 per boss, one per BSC tier plus one "Flawless" for a no-damage kill), Collector purchases (1–2,000 cells), feats, secrets, lore rooms and DLC.
  - Mostly cosmetic, with exceptions: the Cultist outfit opens an area, collection counts unlock weapons, and some outfits trigger unique dialogue.
  - Source: [Dead Cells Wiki: Outfits](https://deadcells.wiki.gg/wiki/Outfits)
- **Risk of Rain 2 skins:** alternate skins are "typically unlocked by completing certain Challenges". Each survivor's Mastery skin unlocks by completing a run or obliterating on Monsoon with that survivor. Skins are "purely cosmetic". — [RoR2 Wiki: Skins](https://riskofrain2.wiki.gg/wiki/Skins)
- **Ladder badges:** Balatro stake-coloured stickers on Jokers and decks; RoR2 Eclipse 8 earns a gold icon. — [Balatro Wiki: Stakes](https://balatrowiki.org/w/Stakes); [RoR2 Wiki: Eclipse](https://riskofrain2.wiki.gg/wiki/Eclipse)
- **Hades hub decoration:** "Home Makeover — Pay for 50 jobs from the House Contractor" (API name AchManyCosmetics) 23.7%. — [Steam: Hades](https://steamcommunity.com/stats/1145360/achievements); [Steam API](https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v0002/?gameid=1145360&format=json)

**Free crossovers**
- **Balatro "Friends of Jimbo":** four free cosmetic face-card updates (2024-08-27, 2024-10-24, 2024-12-12, 2025-02-24). Partners include The Witcher, Among Us, Vampire Survivors, Dave the Diver, Cyberpunk 2077, Stardew Valley, The Binding of Isaac, Slay the Spire, Assassin's Creed and Fallout. — [Wikipedia: Balatro](https://en.wikipedia.org/wiki/Balatro); [Gematsu](https://www.gematsu.com/2024/08/balatro-friends-of-jimbo-update-now-available); [Push Square](https://pushsquare.com/news/2024/10/balatro-deals-more-free-crossover-cards-featuring-cyberpunk-stardew-valley-more) [search summary]
- **Dead Cells "Everyone Is Here"** (2021-11-22, free): weapons and outfits from Hollow Knight, Blasphemous, Skul, Guacamelee, Hyper Light Drifter and Curse of the Dead Gods. Outfits unlock through play, with a "book of clues" pointing to the feats. — [NME](https://www.nme.com/news/gaming-news/dead-cells-free-update-includes-six-game-crossover-next-week-3100271) [search summary]; [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
- **Vampire Survivors free crossover content DLC:** Emerald Diorama (SaGa, 2025-04-10) and Ante Chamber (Balatro, 2025-10-28). — [Steam: VS DLC](https://store.steampowered.com/dlc/1794680/)

**Paid crossover and content DLC (for comparison)**
- **Vampire Survivors:**
  - Ode to Castlevania (€3.99): Overwhelmingly Positive, 97% of 1,902 reviews.
  - Emergency Meeting (Among Us, €2.49): Very Positive, 93% of 1,055.
  - Sources: [Steam: Ode to Castlevania](https://store.steampowered.com/app/3210350/Vampire_Survivors_Ode_to_Castlevania/); [Steam: Emergency Meeting](https://store.steampowered.com/app/2690330/Vampire_Survivors_Emergency_Meeting/)
- **Risk of Rain 2 Seekers of the Storm** (€14.99, 2024-08-27): all-time Mixed, 55% of 9,649 English reviews; recent reviews Very Positive, 86% of 46. The store page does not explain the cause. — [Steam: Seekers of the Storm](https://store.steampowered.com/app/2306620/Risk_of_Rain_2_Seekers_of_the_Storm/)

**Paid cosmetic and supporter packs**
- **Cult of the Lamb:**
  - Cultist Pack (€4.99, 2022-08-11): 5 Follower forms, 2 outfits, 7 decorations; Very Positive, 91% of 296.
  - Heretic Pack (€4.99, 2023-04-24): sold explicitly as a "cosmetic set"; 96% of 137.
  - Pilgrim Pack (€6.99, 2024-08-12): cosmetics plus a story questline; 94% of 220.
  - Sinful Pack (€4.99, 2024-01-16).
  - Sources: [Steam: Cultist Pack](https://store.steampowered.com/app/2015880/Cult_of_the_Lamb_Cultist_Pack/); [Steam: Heretic Pack](https://store.steampowered.com/app/2331540/Cult_of_the_Lamb_Heretic_Pack/); [Steam: Pilgrim Pack](https://store.steampowered.com/app/2890190/Cult_of_the_Lamb_Pilgrim_Pack/); [Steam: CotL DLC](https://store.steampowered.com/dlc/1313140/)
- **Cult of the Lamb creative director Julian Wilton** (2024-09-10), on ending free updates: "I think it's going to be the last of our free updates for now… We have given a lot of love, and now it's time to get some money from people." — [Well-Played](https://www.well-played.com.au/cult-of-the-lamb-is-taking-a-break-from-free-updates-but-there-is-more-content-on-the-way-creative-director-teases-new-features/)
- **Halls of Torment Supporter Pack** (€3.99, 2025-02-05):
  - Contents: one supporter skin per character, three alternative HUD styles, and a "Training Grounds" practice area.
  - The skins are cosmetic, with abilities and stats matching the base characters.
  - Reviews: Very Positive, 95% of 71.
  - Source: [Steam: HoT Supporter Pack](https://store.steampowered.com/app/3386980/Halls_of_Torment_Supporter_Pack/)
- **Soundtracks as low-friction support purchases:** RoR2 €5.70, Vampire Survivors €2.99, Halls of Torment €2.99, Cult of the Lamb €9.99, and Cult of the Lamb's metal re-imagining *Hymns of the Unholy* €9.99. — [Steam: RoR2 DLC](https://store.steampowered.com/dlc/632360/); [Steam: VS DLC](https://store.steampowered.com/dlc/1794680/); [Steam: HoT DLC](https://store.steampowered.com/dlc/2218750/); [Steam: CotL DLC](https://store.steampowered.com/dlc/1313140/)

**Monetisation stances**
- **LocalThunk:**
  - He spent development money only on the soundtrack.
  - He tweeted sarcastically about adding microtransactions or loot boxes to lower the PEGI rating.
  - He opposes Balatro being used for gambling.
  - Source: [Wikipedia: Balatro](https://en.wikipedia.org/wiki/Balatro)
- **Vampire Survivors mobile:** free-to-play with optional ads, fast-tracked to counter unauthorised clones. — [Wikipedia: VS](https://en.wikipedia.org/wiki/Vampire_Survivors)
- **Death Must Die:** lists no cosmetic DLC or supporter pack. — [Steam: DMD](https://store.steampowered.com/app/2334730/Death_Must_Die/)

### Inferences
- **Recommended cosmetic stack:**
  - Earned skins per hero for milestones: first win, each ladder tier, flawless boss kills, collection completion.
  - Hub decoration bought with soft currency (Hades House Contractor).
  - Ladder badges (Balatro stickers, RoR2 gold icon).
  - Pixel-art palette swaps and sprite variants are comparatively cheap to produce. This is an inference; check the cost per animation set.
- **Paid add-ons after launch:**
  - A €3–5 supporter pack (skins, HUD styles, maybe a practice room, as in Halls of Torment) plus a soundtrack.
  - Ship only after one or two meaningful free updates.
  - Label everything as "cosmetic, no gameplay effect".
  - No loot boxes, no premium currency, no gambling iconography.
- **Free crossovers:** cross-promotion with peer indie games (Balatro, Dead Cells, Vampire Survivors) costs little and drives visibility. It requires business development and IP approvals, which is a solo-dev bandwidth constraint.

### Gaps
- No systematic data on player rejection of paid cosmetics in premium roguelites; I found no negative examples of cosmetic-only DLC in this sample.
- No public revenue split for supporter packs or soundtracks.
- The cause of the RoR2 Seekers of the Storm backlash is not documented in the fetched sources; general reporting attributes it to a buggy accompanying base-game update, but I did not verify this.

---

## 8. Narrative across deaths (the Hades model): cost versus benefit, lighter alternatives

### Takeaway
Hades proves that narrative reactive to death massively deepens engagement and acclaim: it won GOTY at GDC, DICE and BAFTA and was the first video game to win a Hugo. It is extraordinarily expensive: ~300,000 words, 21–22k voiced lines and ~10 hours of dialogue, produced by a ~20-person team over ~2 years of Early Access. Only ~23% of owners reach its main story ending and ~8% its epilogue. Lighter, cheaper alternatives are lore fragments unlocked by runs, hub-NPC reactions to milestones, and environmental lore rooms.

### Cited Findings
- **Kasavin's principle:** "If the player doesn't forget their deaths and learns from them, so should the protagonist." — [Inlander](https://inlander.com/culture/hades-writer-greg-kasavin-on-how-he-made-video-game-deaths-drive-a-feel-good-story-22725237) [search summary; page geo-blocked, HTTP 451]
- **Narrative design approach:** an ongoing narrative evolving between runs, with many channels of permanent progression, so deaths feel like part of the structure. — [Tom's Guide](https://www.tomsguide.com/uk/features/hades-exclusive-interview-supergiant) [search summary]
- **Hades scale:**
  - Kasavin and his writers drafted about 10 hours of dialogue across many chained events.
  - The team was about 20 people, and Early Access ran on one platform for about 2 years.
  - About 700k copies sold in Early Access and 1M+ shortly after 1.0.
  - Awards: Game of the Year at GDC Awards, DICE and BAFTA, and a 2021 Hugo (the first for a video game).
  - Source: [Wikipedia: Hades](https://en.wikipedia.org/wiki/Hades_(video_game))
- **Voiced lines and words:**
  - Supergiant's infographic: Zagreus alone has 8,500 lines (2020-12-29). — [Nintendo Everything](https://nintendoeverything.com/?p=698667)
  - Search summaries cite "over 21,000" voiced lines in the infographic and ">22,000 lines of dialogue" in the GDC talk *Breathing Life into Greek Myth* (Kasavin and Darren Korb). — [GDC Vault](https://gdcvault.com/play/1026975/Breathing-Life-into-Greek-Myth) [search summary; Vault was in maintenance]
  - Kasavin is quoted saying the script exceeds 300,000 words. — [Multiplayer.it](https://multiplayer.it/articoli/hades-tre-segreti-successo.html) [search summary]
- **Narrative completion rates (Hades, Steam):**
  - "Grown Close — forge a bond with any character" 26.2%.
  - "The Family Secret — complete the main quest" 23.3%.
  - Epilogue (API AchReachedEpilogue) 8.4%.
  - First escape 46.7%.
  - Sources: [Steam: Hades](https://steamcommunity.com/stats/1145360/achievements); [Steam API](https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v0002/?gameid=1145360&format=json)
- **Hades II:** a new protagonist, because Kasavin said the team knew early it did not want Zagreus as the lead again; God Mode returns. Early Access sold 2M copies over 16 months, including 1.5M in the first two months (Alinea Analytics), with a peak of 103,567 concurrent players at Early Access launch. — [Wikipedia: Hades II](https://en.wikipedia.org/wiki/Hades_II); [Notebookcheck, 2025-09-28](https://www.notebookcheck.net/Supergiant-Games-director-says-Hades-3-isn-t-a-sure-thing-despite-the-success-of-Hades-2.1125776.0.html)
- **Lighter models:**
  - Dead Cells planned no story at first and added a minor plot after players found the world compelling. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)
  - Dead Cells uses lore rooms, which also carry crossover weapon unlocks. — [Dead Cells Wiki: Outfits](https://deadcells.wiki.gg/wiki/Outfits); [NME](https://www.nme.com/news/gaming-news/dead-cells-free-update-includes-six-game-crossover-next-week-3100271) [search summary]
  - Slay the Spire 2 unlocks "fragments of the Spire's timeline and lore through repeated climbs". — [Steam: StS2](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)
  - Rogue Legacy 2 makes lineage the story device: persistent upgrades "and so are your dead heirs". — [Steam: RL2](https://store.steampowered.com/app/1253920/Rogue_Legacy_2/)
- **Cost of difficulty options:** Kasavin also warned about the cost of difficulty systems: "I don't underestimate the extraordinary amount of work it takes to implement an easy mode of some sort." — [Can I Play That](https://caniplaythat.com/2021/08/11/hades-god-mode-explained-by-supergiant-games)

### Inferences
- **A full Hades-style voiced, highly reactive script is out of scope for a solo developer.** AI-assisted text drafting lowers the writing cost, but voice, localisation and reactivity QA (combinatorial triggers) remain heavy.
- **Proposed "Hades-lite":**
  - A hub with 3–5 NPCs whose short unvoiced or barked lines key off milestones: first death to each boss, first win, new hero, ladder tier, collection percentage.
  - A priority queue so the freshest relevant line plays.
  - Lore fragments (codex pages) dropped by bosses and secret rooms.
  - A short ending, plus a "true ending" gated by the ladder (Dead Cells' Collector at 5 BSC).
- **Track narrative completion** as a separate funnel; expect it to be reached by roughly a fourth of buyers, as in Hades.

### Gaps
- No public budget or headcount split for Hades' writing, VO and localisation.
- No evidence comparing retention of lighter narrative models against Hades' approach.

---

## 9. QoL and accessibility for chaotic action roguelites

### Takeaway
The best-documented practices:
- **Opt-in assist modes** that keep achievements but exclude leaderboards (Dead Cells' 2022 "Breaking Barriers").
- **Scaling assist power** that never shames the player (Hades God Mode: 20% damage resistance +2% per death, up to 80%; adjustable in Hades II).
- **Granular visual toggles:** damage numbers, flashing VFX, screen shake, blood, particle reduction, outlines, non-colour cues.
- **Remapping and hold-to-act controls.**

Visual clutter in late-game survivors-likes remains an unsolved complaint. Players ask for VFX opacity or density sliders, and the Hades II reviewer called VFX busyness a barrier.

### Cited Findings
- **Dead Cells "Breaking Barriers"** (2022-06-23), from a Game Developer list:
  - Assist mode: "continue mode" (restart from the current level), auto-targeting of nearby enemies, adjustable trap damage, slower parry window and traps.
  - Controls: hold-to-jump/roll, remapping of stick and D-pad functions.
  - Visuals: HUD size and transparency, Arial font, recoloured Brutality/Tactics/Survival indicators plus extra non-colour data, enhanced outlines, fewer particles, no-blood option.
  - Audio: sound-priority systems and per-category SFX volume.
  - Source: [Game Developer](https://gamedeveloper.com/design/dead-cells-devs-drop-surprise-accessibility-update)
- **Dead Cells assist details from other coverage:**
  - Lives selectable at 1, 3, 7 or infinite; enemy health and damage sliders; full map reveal.
  - Options were co-designed and tested with AbleGamers and community members, and enabling them does not disable achievements.
  - Sources: [TechRaptor](https://techraptor.net/gaming/news/dead-cells-update-adds-accessibility-options-and-assist-mode), [GoNintendo](https://gonintendo.com/contents/5463-dead-cells-first-accessibility-update-breaking-barriers-now-live) [search summary]
  - Assist runs are not posted to Daily Challenge leaderboards. — [Dead Cells Wiki: Daily Challenge](https://deadcells.wiki.gg/wiki/Daily_Challenge)
- **Hades God Mode:**
  - An optional setting that makes the hero stronger after each failed run so struggling players can see the story.
  - 20% base damage resistance, +2% per failed run, maximum 80%; the same in Hades II.
  - Source: [Wikipedia: Hades](https://en.wikipedia.org/wiki/Hades_(video_game)); [Wikipedia: Hades II](https://en.wikipedia.org/wiki/Hades_II)
  - Turning it off keeps the accumulated percentage for later; it is not available in Hell Mode. — [search summary of FAQ/forum sources](https://www.calendar-uk.co.uk/frequently-asked-questions/how-many-hours-is-100-hades)
- **Supergiant on God Mode:**
  - Kasavin: "It's not a one size fits all solution" and "It's not easy to tune a video game."
  - Supergiant (2020-09-25): "With Hades, we wanted to open up the thrilling experience of rogue-like games to more players."
  - Source: [Can I Play That](https://caniplaythat.com/2021/08/11/hades-god-mode-explained-by-supergiant-games)
- **Hades II accessibility** (review 2025-11-05):
  - God Mode can now be manually adjusted, because the fixed version "tended to over-correct".
  - Full controller remapping.
  - Aim assist, auto-fire and auto-sprint appear in the settings.
  - Cons: VFX can hide key elements, and resources are harder to gather.
  - Source: [Can I Play That: Hades II](https://caniplaythat.com/2025/11/05/hades-ii-accessibility-review/)
- **Hades (original) accessibility:** the Family Gaming Database lists 28 documented accessibility features, strongest in Controls and Getting Started; the original Hades can highlight the mouse cursor in busy fights. — [Family Gaming Database](https://www.familygamingdatabase.com/accessibility/Hades); [Can I Play That 2020](https://caniplaythat.com/2020/11/04/indie-spotlight-hades/) [search summary]
- **Vampire Survivors options:**
  - Display: Damage Numbers toggle, Flashing VFX toggle, Disable blood, Weapons ScreenShake toggle, Pixel Font, Show Defanged Enemies.
  - Co-op: per-player colours, co-op indicators, permanent outlines, Sequential Chest Mode, joystick type.
  - Source: [VS Wiki: Options](https://vampire.survivors.wiki/w/Options)
  - Limits: the screen stays "full of bright and dark pulsating areas" in late game, and players ask for an effects opacity/density slider. — [Family Gaming Database: VS](https://www.familygamingdatabase.com/accessibility/Vampire+Survivors); [Steam forum](https://steamcommunity.com/app/1794680/discussions/0/4631482569784862180) [search summary]
- **Difficulty sliders and practice:**
  - Brotato: adjustable enemy health, damage and speed. — [Steam: Brotato](https://store.steampowered.com/app/1942280/Brotato/)
  - Rogue Legacy 2 House Rules (see §2). — [Steam: RL2](https://store.steampowered.com/app/1253920/Rogue_Legacy_2/)
  - Halls of Torment: Training Grounds (horde room plus single-target room) with a DPS tracker. — [Steam: HoT Supporter Pack](https://store.steampowered.com/app/3386980/Halls_of_Torment_Supporter_Pack/)
- **Game Accessibility Guidelines relevant to the genre:**
  - Basic:
    - Allow controls to be remapped.
    - Offer a wide choice of difficulty levels.
    - Include an option to adjust game speed.
    - Avoid flickering images and repetitive patterns.
    - Ensure no essential information is conveyed by colour alone.
    - Provide subtitles.
    - Use a readable default font size.
    - Provide high text/UI contrast.
    - Include interactive tutorials.
    - Make every UI area reachable with the same input method as gameplay.
  - Intermediate:
    - Allow difficulty to be changed during play.
    - Provide an option to hide background movement.
    - Include assist modes such as auto-aim.
    - Provide an option to adjust contrast.
    - Provide autosave and manual save.
    - Include practice without failure.
    - Support more than one input device.
  - Advanced:
    - Avoid sudden unexpected movement.
    - Allow font size adjustment.
    - Provide an option to hide all non-interactive elements.
    - Do not make precise timing essential.
  - Source: [Game Accessibility Guidelines](https://gameaccessibilityguidelines.com/full-list/)
- **DMD platform and localisation signals:**
  - It runs very well on Steam Deck (score 10) and is described as well optimised. — [Vaporlens](https://vaporlens.app/app/2334730/death_must_die.md)
  - Interface and subtitles are English-only. — [Steam: DMD](https://store.steampowered.com/app/2334730/Death_Must_Die/)
  - Missing localisations (Spanish, Chinese, French, Russian, Portuguese) are a top complaint (weight 0.18). — [Vaporlens](https://vaporlens.app/app/2334730/death_must_die.md)

### Inferences
- **Day-one options checklist for the new game:**
  - **Visual:** sliders for damage numbers, screen shake, flash intensity, own-VFX opacity, ally/enemy VFX opacity, and particle density; enemy telegraph outlines; colour-blind-safe palettes plus shape and icon cues.
  - **Controls:** full remapping; hold-to-act alternatives; aim assist and auto-attack options.
  - **Pacing and saves:** game speed from 50% to 100%; pause anywhere; suspend-save.
  - **Assist mode:** God-Mode-style scaling damage resistance plus sliders (enemy HP and damage, lives or continue-from-biome). It keeps achievements but flags runs off-leaderboard.
  - **Practice:** a training room with a DPS meter.
  - **Platform:** Steam Deck verification and controller-first UI.
  - **Localisation:** at least EFIGS + zh-Hans + ru + pt-BR (DMD's missing languages are a top complaint).
- **Build clutter controls in from the start:** in a pixel-art survivors-like, VFX readability is a core design constraint, not an afterthought. The VS late-game and Hades II complaints show that the problem grows with build power.

### Gaps
- No usage telemetry for assist modes (share of players using God Mode or Assist Mode).
- No sourced Steam Deck playtime-share data for these games.

---

## 10. Published metrics: completion rates, playtime, retention

### Takeaway
No public retention curves were found for these premium roguelites. The best proxies are Steam global achievement percentages, fetched 2026-10-25:
- About 40–47% of owners ever finish a first full run in Hades and Dead Cells.
- About 56–72% finish one in Slay the Spire and Balatro.
- About 5–15% reach the top of the difficulty ladders.
- About 23–24% engage with dailies.

SteamSpy no longer returns playtime data.

### Cited Findings
- **Hades** (Steam, 49 achievements):
  - Clear Tartarus 81.8% → Asphodel 72.2% → Elysium 58.8% → first full escape ("Is There No Escape?") 46.7%.
  - Main story ending 23.3%; epilogue (API) 8.4%.
  - Max any weapon aspect 31.6%; "Home Makeover" 23.7%.
  - Sources: [Steam: Hades](https://steamcommunity.com/stats/1145360/achievements); [Steam API](https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v0002/?gameid=1145360&format=json)
- **Hades II** (50 achievements): Clear Erebus 68.5%; "Death to Chronos" 27.3%; first Gift of the Veil 24.7%. — [Steam: Hades II](https://steamcommunity.com/stats/1145350/achievements)
- **Dead Cells** (84 achievements; API names):
  - Reach the 2nd biome 92.0%; kill the first boss (Concierge) 72.3%; reach the Throne Room 47.9%.
  - First win (Hand of the King) 40.4%; boss cells 1→5: 41.1 / 23.0 / 15.2 / 11.8 / 8.5%; Collector 5.8%.
  - Completed a daily 23.4%.
  - Source: [Steam API: Dead Cells](https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v0002/?gameid=588650&format=json)
- **Slay the Spire:**
  - Unlock Ascension 66.4%.
  - Wins: Ironclad 56.0%, Silent 50.4%, Defect 44.0%, Watcher 26.0%.
  - Ascension 10 14.8%; Ascension 20 7.3%; Daily Climb win 24.0%; "Amethyst+ — Complete the Ending with the Watcher" 13.2%.
  - Source: [Steam: StS](https://steamcommunity.com/stats/646570/achievements)
- **Balatro:** Ante 4 89.8%, Ante 8 74.5%, win 71.6%, Red+ 43.7%, Black+ 30.3%, Gold 12.2%, any challenge 14.0%. — [Steam: Balatro](https://steamcommunity.com/stats/2379780/achievements)
- **Vampire Survivors:**
  - "Hollow Heart — survive 1 minute" 97.8%.
  - "Inlaid Library — reach level 20 in the Mad Forest", which unlocks the 2nd stage, 88.4%.
  - "Holy Wand — evolve the Magic Wand" 78.1%.
  - Source: [Steam: VS](https://steamcommunity.com/stats/1794680/achievements)
- **Risk of Rain 2:** complete a teleporter 86.9%; survive 30 minutes 77.3%; "Washed Away — Beat the game" 52.1% (as labelled on the page). — [Steam: RoR2](https://steamcommunity.com/stats/632360/achievements)
- **Death Must Die:** defeat the Baron 67.9%; win at Difficulty 5 43.8%; win at Difficulty 10 38.6%. — [Steam: DMD](https://steamcommunity.com/stats/2334730/achievements)
- **Playtime:** SteamSpy's API returned 0 for average and median playtime for Hades (owners band 5–10M) on 2026-10-25, so it is not usable for playtime. — [SteamSpy API](https://steamspy.com/api.php?request=appdetails&appid=1145360)
- **DMD content horizon:** endgame reached in about 7 hours; base content 5–10 hours. — [Vaporlens](https://vaporlens.app/app/2334730/death_must_die.md); [Steam: DMD](https://store.steampowered.com/app/2334730/Death_Must_Die/)
- **Review health:**
  - Vampire Survivors: 98% of 128,404 (€4.99).
  - Brotato: 96% of 32,671.
  - Halls of Torment: 95% of 21,329 (€6.66).
  - Death Must Die: 91% of 14,051 (€6.89; recent 81% of 61).
  - Rogue Legacy 2: 91% of 7,734.
  - Slay the Spire 2: 91% of 71,473 (recent 72% of 3,743).
  - Sources: Steam store pages ([VS](https://store.steampowered.com/app/1794680/Vampire_Survivors/), [Brotato](https://store.steampowered.com/app/1942280/Brotato/), [HoT](https://store.steampowered.com/app/2218750/Halls_of_Torment/), [DMD](https://store.steampowered.com/app/2334730/Death_Must_Die/), [RL2](https://store.steampowered.com/app/1253920/Rogue_Legacy_2/), [StS2](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/))
- **Sales and concurrency benchmarks:**
  - Slay the Spire: 1M by June 2018, 1.5M by March 2019. — [Wikipedia: StS](https://en.wikipedia.org/wiki/Slay_the_Spire)
  - Risk of Rain 2: 1M within a month of Early Access, 4M+ on PC by March 2021. — [Wikipedia: RoR2](https://en.wikipedia.org/wiki/Risk_of_Rain_2)
  - Balatro: 1M in the first month, 5M+ by January 2025; mobile made $4.4M in two months. — [Wikipedia: Balatro](https://en.wikipedia.org/wiki/Balatro)
  - Vampire Survivors: peak above 70k concurrent players in February 2022. — [Wikipedia: VS](https://en.wikipedia.org/wiki/Vampire_Survivors)
  - Hades II: 2M copies in Early Access. — [Notebookcheck](https://www.notebookcheck.net/Supergiant-Games-director-says-Hades-3-isn-t-a-sure-thing-despite-the-success-of-Hades-2.1125776.0.html)
  - Dead Cells: 10M+ by June 2023. — [Wikipedia: Dead Cells](https://en.wikipedia.org/wiki/Dead_Cells)

### Inferences
- **Read the percentages as relative funnels.** Steam does not document the denominator on these pages; it is commonly understood to include owners who never launched the game. The absolute numbers are therefore lower bounds on completion among people who actually played.
- **Planning targets for the new game:**
  - ≥80% clear Act 1.
  - ~45–60% get a first win.
  - ~25–40% enter the difficulty ladder.
  - ~5–10% reach the top tier.
  - ~20–25% try the daily.
  - Instrument the same funnel with Steam achievements from day one so it can be compared directly with these benchmarks.

### Gaps
- No public day-1/day-7/day-30 retention curves or median-playtime figures for premium roguelites were found. Steam does not publish them, SteamSpy no longer tracks playtime, and GameDiscoverCo or Gamalytic data could not be searched after the search budget ran out.
- The Steam denominator definition is not confirmed by a primary Valve source in these notes.
