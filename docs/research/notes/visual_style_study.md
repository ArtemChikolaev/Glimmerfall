# Visual style study: Death Must Die, Dead Cells and stylish pixel-art action games (what is on screen, observed 2026-10-25)

Notes for the report writer. The whole study is built on images I opened and inspected in the built-in browser: Steam screenshots (`path_full`), Steam "extras" clips and HLS trailers, plus the live aegis-draft site. Every claim below links the image, clip or page where it was seen. Pixel-scale and colour numbers are **my own measurements** of those images (method below). They are not developer statements unless a developer source is cited. WebSearch was unavailable this session (the shared search budget was used up), so developer interviews about Death Must Die's art pipeline are missing. Only primary pages fetched directly by URL are cited.

## 1. Death Must Die: camera, resolution, palette, lighting, VFX, HUD, hub

### Takeaway
Death Must Die (DMD) renders at 640×360 and is shown at exactly 3× on 1080p. Its look comes from darkness and focus. In gameplay shots 35–82% of pixels are near-black, and a pool of light follows the player, so the centre is 2–3× brighter than the edges. The environment is a muted field in one hue (dark moss green, indigo or ochre), and almost all saturation goes to attacks, pickups and light sources. Sprites are small (about 35–40 native px, ≈10% of screen height). The UI is a thin black gothic frame with serif type and painted god portraits.

### Cited Findings
#### Measurement method (used for every game in this file)
- Each screenshot was decoded into a canvas. **Pixel scale:** I built a periodogram of the summed absolute luminance differences between adjacent columns and rows. The dominant period P is the upscale factor, so internal width = image width / P. I also measured "phase concentration": the max/mean edge energy over x mod s for s = 2…8. About 1.0 means no grid; 2–3.5 means a strictly locked pixel grid. **Colour:** mean HSV value (meanV); share of pixels with V < 0.15 ("dark%"); mean saturation of non-dark pixels (meanS); and a hue histogram in 30° bins (pixels with S > 0.25 and V > 0.2, weighted by S·V). Bin labels: 0° = red/orange, 30° = orange/yellow, 60° = yellow/olive, 90° = yellow-green, 120° = green, 150° = green-cyan, 180° = cyan, 210° = sky blue, 240° = blue, 270° = violet, 300° = magenta, 330° = pink/crimson. Lighting focus is the mean V of the central 30% box divided by the mean V of the outer 8% border ("centre/edge"). Screenshot URLs come from [Steam appdetails API, DMD](https://store.steampowered.com/api/appdetails?appids=2334730) and the equivalent endpoints for the other app IDs.

#### Resolution and scale
- All 9 DMD Steam screenshots are 1920×1080 with a dominant edge period of exactly **3.00 px** on both axes, and phase concentration at s = 3 is 1.10–2.07 (≈1.0 for other s). That puts the internal resolution at **640×360 at 3×**. Examples: [DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg), [DMD-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_a758004b4a3c3be6d48f13eec433acd75de9ce7c.1920x1080.jpg) (P = 3.00, phase 2.07).
- Small text is not on the 3× grid. Damage numbers ("12") and item labels render as smooth serif type over the pixel world: [DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg) (crop of the left-centre area).

#### Camera and sprite size
- Top-down ¾ (Diablo-like) camera that follows the player, who stays centred. The player is ≈117 screen px tall at 1080p, so ≈39 native px (≈11% of screen height) in a dynamic attack pose. Skeleton infantry are ≈30–37 native px including spears: [DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg) (crops 760,330,400×225 and 150,480,480×270).
- Elite and boss scale: a large dark armoured knight stands at roughly 1.5–2× the skeleton height at the top of [DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg).

#### Palette and value
- Gameplay screenshots are very dark:
  - [DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg): meanV 0.13, 82% dark, meanS 0.35, centre/edge 0.25/0.09, hues red 38% / green 21% / crimson 18%.
  - [DMD-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_144e73f2fbd60444935777044a322a9c87145096.1920x1080.jpg): meanV 0.19, 36% dark, centre/edge 0.38/0.14.
  - [DMD-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_fe6bef6f687f28d552fca8f2c605d5f8cbb06b61.1920x1080.jpg): meanV 0.11, 80% dark, centre/edge 0.26/0.07, yellow-green 44% + green 19%.
  - [DMD-ss6](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_9581b0eb51e4d50668e0394d44d22e25f3a56e62.1920x1080.jpg) (fire ring): meanV 0.27, centre/edge 0.48/0.16, orange 40% / red 26% / crimson 24%.
  - [DMD-ss7](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_728b001ddecc9ac52b3a40a73d8ffdf7fd1ab2da.1920x1080.jpg) (slime horde) is the exception, with centre/edge 0.24/0.25 because lime slimes fill the whole frame.
- Environment vs. actors:
  - The ground is dark moss green and grey-teal stone with dithered grass texture, and pine trees are near-black navy silhouettes with blue rim highlights. Skeletons are pale bone-white with dark grey shading. XP gems are small saturated lime-green crystals with a faint glow: [DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg).
  - Slimes are a carpet of translucent lime domes with dark skull cores, and the player's red-and-white spinning blades are the complementary accent: [DMD-ss7](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_728b001ddecc9ac52b3a40a73d8ffdf7fd1ab2da.1920x1080.jpg).
- Each biome has its own palette:
  - Desert/tomb: an orange torch-lit sand pool against indigo-blue obelisks and shadows, with pale cyan crescent projectiles ([DMD trailer @36.5 s](https://video.akamai.steamstatic.com/store_trailers/2334730/545241/d38673966116cba081951e39fa8077119021fd9d/1750812009/hls_264_master.m3u8)).
  - Fire biome: an orange-red floor lit by a huge white-yellow fire pillar, with blue shadow tiles (same trailer @56.0 s).
  - Ice biome: a bright cyan and white ground ([DMD store clip @8.9 s](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/extras/662c1a4d7e8fd7918143844e52b53cab.mp4)).

#### Outlines
- Sprites carry no uniform black outline. Shapes separate by value: bright bone or skin against the dark ground, dark-on-dark shapes lifted by a coloured rim light (blue on trees, peach/orange on the player): [DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg) (player crop).

#### Lighting
- A soft radial light pool surrounds the player in a dark forest, and everything outside it falls to near-black. A lantern on a rock is a separate light source: [DMD-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_fe6bef6f687f28d552fca8f2c605d5f8cbb06b61.1920x1080.jpg).
- Coloured point lights light the floor: warm torch pools on sand ([trailer @36.5 s](https://video.akamai.steamstatic.com/store_trailers/2334730/545241/d38673966116cba081951e39fa8077119021fd9d/1750812009/hls_264_master.m3u8)); a bloom-heavy shrine crystal ([DMD-ss6](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_9581b0eb51e4d50668e0394d44d22e25f3a56e62.1920x1080.jpg)); a lit circular stone dais ([DMD-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_144e73f2fbd60444935777044a322a9c87145096.1920x1080.jpg)).
- The developers treat darkness and light sources as a tuned system. Patch 0.8.5 (22 May 2025) says: "Decreased Act 3 global darkness so areas with 0 light sources won't be pitch black" — [Steam news API, Patch 0.8.5](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=1&maxlength=0&enddate=1747939686&format=json) ([announcement page](https://steamcommunity.com/ogg/2334730/announcements/detail/502825815722951266)).
- There is a post-process colour-grading layer. Patch 0.6.55: "Screen recolor effects disabled by default for MacOS (solving graphical glitch issue)" — [Steam news API, Patch 0.6.55](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=1&maxlength=0&enddate=1700685178&format=json).

#### VFX style and density
- Most attack VFX are pixel art on the same 3× grid:
  - A large red ribbon spiral with black thorn spikes and an orange crescent smear on the player's swing ([DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg)).
  - A white crescent slash with blue-white lightning sparks ([DMD-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_144e73f2fbd60444935777044a322a9c87145096.1920x1080.jpg)).
  - Rows of stylised orange and yellow pixel flames forming a fire wall ([DMD-ss6](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_9581b0eb51e4d50668e0394d44d22e25f3a56e62.1920x1080.jpg)).
  - Red and white pinwheel sawblades ([DMD-ss7](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_728b001ddecc9ac52b3a40a73d8ffdf7fd1ab2da.1920x1080.jpg)).
- Additive soft glows sit on top of the pixel VFX. In a late fight there are teal glowing orbs, lavender-white chain-lightning networks, white star-burst hit sparks with coloured rays, and a thin white sword arc ([trailer @22.2 s and @22.8 s, centre crops](https://video.akamai.steamstatic.com/store_trailers/2334730/545241/d38673966116cba081951e39fa8077119021fd9d/1750812009/hls_264_master.m3u8)). Other large effects: a green spirit-flame pillar, an orange fire dome and white star projectiles ([DMD store clip @2.6/5.8/4.7 s](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/extras/662c1a4d7e8fd7918143844e52b53cab.mp4)).
- Corpses persist as small grey bone piles on the ground: [DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg).

#### Readability in chaos, damage numbers, health bars
- Basic damage numbers are small, pale grey/white serif digits ("12") that never dominate the frame ([DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg)). In dense fights numbers are white (29, 38, 33) with some in yellow (27) ([trailer @22.2 s](https://video.akamai.steamstatic.com/store_trailers/2334730/545241/d38673966116cba081951e39fa8077119021fd9d/1750812009/hls_264_master.m3u8)).
- Thin dark-framed health bars appear only over some enemies (red/white, a few green) ([trailer @22.2/22.8 s](https://video.akamai.steamstatic.com/store_trailers/2334730/545241/d38673966116cba081951e39fa8077119021fd9d/1750812009/hls_264_master.m3u8)). A small red bar sits over an enemy in [DMD-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_fe6bef6f687f28d552fca8f2c605d5f8cbb06b61.1920x1080.jpg).
- Enemy telegraphs are deliberately animated:
  - Patch 0.8.4: "Wraith Dash now has a windup animation" and "Stone Golem now has proper Spawn & Death animation" — [Steam news API, Patch 0.8.4](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=1&maxlength=0&enddate=1746829496&format=json).
  - Patch 0.8.5: "Fungus plays windup animation before explosion" — [Patch 0.8.5](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=1&maxlength=0&enddate=1747939686&format=json).
- Item labels on the ground use white serif text ("Vigorous Breastplate of Sanctity"), and interactables get a key prompt ("E" + "Shrine" + an icon in a dark frame): [DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg), [DMD-ss6](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_9581b0eb51e4d50668e0394d44d22e25f3a56e62.1920x1080.jpg).
- Tooltip borders encode rarity. Patch 0.6.77: "Item comparison tooltip now has a border that corresponds to the rarity of the currently equipped item." — [Steam news API, Patch 0.6.77](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=1&maxlength=0&enddate=1701688229&format=json).

#### HUD and UI style
- The in-run HUD is minimal and hugs the edges ([DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg), [DMD-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_fe6bef6f687f28d552fca8f2c605d5f8cbb06b61.1920x1080.jpg)):
  - Bottom centre: a painted hero portrait in a small square frame, with a flame-edged red HP bar to the left ("38/40" above it), two gold winged pips to the right, and the level number in a small black disc below.
  - A thin black line with thorny ornaments runs along the bottom edge.
  - Top centre: the run timer in a dark tab ("7:27").
  - Top right: a circular minimap in a thorny dark frame.
  - Bottom corners: "Tab" and "B" key hints with small ornaments.
- Menus use ornate gothic frames, serif capitals and colour-coded god themes:
  - Boon cards: three tall dark cards with fire icons and an orange glow on the selected one ([DMD-ss4](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_df843bd57ff67a49427c59912cd025016e4f2255.1920x1080.jpg), "Summer, Goddess of Fire"; [trailer @30.0 s](https://video.akamai.steamstatic.com/store_trailers/2334730/545241/d38673966116cba081951e39fa8077119021fd9d/1750812009/hls_264_master.m3u8), "Krom, God of Conquest").
  - Shop and inventory: grids of item icons in dark slots over a blurred scene ([DMD-ss3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_959f249f759c4b9586cb8397c2413603d4429852.1920x1080.jpg)).
  - Dialogue: a large painted portrait with teal ghost wisps and crows beside a framed text box ([DMD-ss2](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_d2eec3ed8b34e3b16b9ee97dd47bb5c30ea6c2af.1920x1080.jpg), "Mort, Daughter of Death"). A large violet/blue line-art illustration of a Death-like figure: [DMD-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_a758004b4a3c3be6d48f13eec433acd75de9ce7c.1920x1080.jpg).
- The meta-progression screen "CONSTELLARIUM" is a vertical tree of monochrome line icons in dark square slots, with unlocked nodes framed in teal. Hooded statues with glowing teal eyes frame it, and the tooltip has a gold title and a green stat line ([trailer @10.5 s](https://video.akamai.steamstatic.com/store_trailers/2334730/545241/d38673966116cba081951e39fa8077119021fd9d/1750812009/hls_264_master.m3u8)).

#### Hub
- The hub is a huge engraved ritual circle on a stone floor in near-monochrome deep blue, lit by a moonlight shaft from above. Small characters (a knight, a magenta-robed figure) stand at its rim ([trailer @4.0 s](https://video.akamai.steamstatic.com/store_trailers/2334730/545241/d38673966116cba081951e39fa8077119021fd9d/1750812009/hls_264_master.m3u8)).

### Inferences
- The "DMD look" is a value design before it is a pixel-art design. The near-black environment plus the player-following light pool creates focus. Saturated colour is reserved for gameplay signals (red, orange, white attacks; lime pickups; teal and cyan magic). That is why hundreds of small sprites stay legible.
- The two gold winged pips next to the portrait are probably dash charges. This could not be confirmed from stills.
- Mixed resolution is deliberate: the world sits on a strict 3× grid, while text, some glows and the bloom are high-resolution. Readers will not perceive this as inconsistency because the categories are cleanly separated (all world art on the grid, all text off it).

### Gaps
- No primary source (interview, devlog) on DMD's art pipeline, light system, palette rules or sprite size was found, because web search was unavailable. Whether sprites are hand-drawn or pre-rendered is unconfirmed.
- Screen shake, hit-stop and enemy hit flash in DMD could not be verified from stills or sparse trailer frames.
- The function of the gold pips and the colour code of the green health bars are unknown.

## 2. Dead Cells: camera, resolution, palette, lighting, VFX, readability

### Takeaway
Dead Cells is also about 640×360 at about 3×, but stylistically it is DMD's opposite. Scenes are bright and very saturated (meanV 0.21–0.65, meanS up to 0.93), each biome is drenched in one dominant hue, dark flat foreground silhouettes frame hazy, lighter backgrounds, and smooth HD glows, arcs and bloom are layered over sprites made from 3D models rendered to pixels with normal maps and toon shading (about 50 px characters, pose-to-pose keys). Impact is sold by VFX: flashes, gore bursts and fire that relights the room.

### Cited Findings
#### Resolution and pipeline
- 10 of 13 Steam screenshots (mostly 1916×1080) peak at P = 2.99–3.00, so the internal width is ≈640 at ≈3×, e.g. [DC-ss2](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_7bde51ea6c8f6289e85ea1d8c1c941e1f8bfee91.1920x1080.jpg) (phase 1.90), [DC-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_b7cf2ca21fe3648c53f808b80393cc727815dcc5.1920x1080.jpg) (phase 1.97). Three shots come from other zooms: P = 4.01 in [DC-ss7](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_ffef382d2a1ace63b82184caf3f6c5b293c66483.1920x1080.jpg), 3.88 in [DC-ss6](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_21c61aca6a66745a2abb3f72b93553398fc7fe32.1920x1080.jpg) and 4.49/2.25 in [DC-ss12](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_65dde6f056018945351e18f55c3481fa2478547b.1920x1080.jpg). Phase concentration is often weak (1.02–1.2) where fog and bloom layers dominate, e.g. [DC-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_a8b0439ad7750cab1bdec86ecef0daa280e9f93f.1920x1080.jpg).
- Primary source, artist Thomas Vasseur ([Game Developer, "Art Design Deep Dive: Using a 3D pipeline for 2D animation in Dead Cells"](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)):
  - A pixel-art model sheet is drawn first, then a simple 3D model with a skeleton is built in 3ds Max and exported as FBX.
  - An in-house tool renders the mesh at very small size with no antialiasing.
  - In-game character height is about 50 px, so detailed 3D modelling was not worth it.
  - Animation is keyed like 2D with the fewest possible keyframes, and in-betweens are added only before or after keys (pose-to-pose).
  - Every frame is exported as a PNG with a normal map, and a basic toon shader renders volume.
  - Shadows come from the lighting system rather than hand painting.
  - VFX carry much of the sense of movement and impact.
  - Old assets are reused for new monsters.
  - Flickering pixels were accepted as a trade-off for speed.

#### Camera and sprite size
- Side-view platformer camera. The player is ≈14% of screen height (≈50 px of 360 per the article above) and ≈18% in a raised-hammer jump in [DC-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_ac28000ade40cc2fe5c128f32ac98ba33c008a7a.1920x1080.jpg).

#### Palette and value
- Each biome has one dominant hue:
  - Violet/magenta cave: [DC-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_ac28000ade40cc2fe5c128f32ac98ba33c008a7a.1920x1080.jpg) (blue 34% / magenta 26% / violet 16%).
  - Toxic green: [DC-ss2](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_7bde51ea6c8f6289e85ea1d8c1c941e1f8bfee91.1920x1080.jpg) (meanS 0.72).
  - Cyan caves: [DC-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_b7cf2ca21fe3648c53f808b80393cc727815dcc5.1920x1080.jpg) (cyan 78%, meanS 0.93).
  - Orange sunset ramparts: [DC-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_a109be71f065e166ff33ba43853fba0a0d4753d1.1920x1080.jpg) (meanV 0.65, red 60% / orange 24%).
  - Red forge: [DC-ss10](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_6f305b9603c17d31ddcbda4c73add319bf910a41.1920x1080.jpg) (red 59%).
  - Blue night bridge under a pale yellow moon: [DC-ss3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_e87e72a247918d8493892e035d5e1b4b84470d2f.1920x1080.jpg) (meanV 0.62, centre/edge 0.87/0.55).
- Depth layering: foreground rock and platforms are near-flat dark violet or black masses framing the play area, backgrounds are lighter and hazy with pink light shafts, and actors sit in the mid-value band ([DC-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_ac28000ade40cc2fe5c128f32ac98ba33c008a7a.1920x1080.jpg)). In a misty forest, blue-violet tree trunks fade with distance under vertical light shafts ([DC-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_a8b0439ad7750cab1bdec86ecef0daa280e9f93f.1920x1080.jpg)).
- Outlines: no black outline. Navy and indigo self-shadow edges and light-blue rim highlights separate the forms ([DC-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_ac28000ade40cc2fe5c128f32ac98ba33c008a7a.1920x1080.jpg), enemy crop).

#### VFX and readability
- VFX are mixed-resolution:
  - The enemy's big red crescent attack arc is a smooth, anti-aliased vector shape, not pixel art. The hammer swing leaves a soft yellow-white glow streak, and the enemy weapon a purple neon trail ([DC-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_ac28000ade40cc2fe5c128f32ac98ba33c008a7a.1920x1080.jpg), crops).
  - On the bridge, a bloom-heavy red jagged hit burst engulfs the boss and a fire status icon floats over its head. Cyan electric barriers mark the arena edges ([DC-ss3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_e87e72a247918d8493892e035d5e1b4b84470d2f.1920x1080.jpg)).
- Status and damage text is colour-coded and small:
  - Light-blue "Slowed" labels stack over enemies next to flame icons for burning, and small orange or red damage numbers appear inside a huge pink-white beam ([DC trailer @44.4 s](https://video.akamai.steamstatic.com/store_trailers/588650/231061/8fe407db503493211097e9898f58ae600559a2a3/1750557235/hls_264_master.m3u8)).
  - Small numbers ("756", "50") and a blue ice beam also appear in [DC store clip 3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/extras/088445c09499282f4fec56bc400670c6.mp4).
- HUD: none of the 13 Steam screenshots, the store clips or the accolade trailer frames I sampled show the gameplay HUD ([DC-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_ac28000ade40cc2fe5c128f32ac98ba33c008a7a.1920x1080.jpg), [trailer](https://video.akamai.steamstatic.com/store_trailers/588650/231061/8fe407db503493211097e9898f58ae600559a2a3/1750557235/hls_264_master.m3u8)).

### Inferences
- Dead Cells is the closest technical analogue to the owner's current pipeline (3D models rendered to small pixel sprites). The difference is in what Motion Twin adds after rendering: a fixed ~640×360 target, per-frame normal maps with toon shading, very few hand-placed keys, and VFX that carry the impact.
- The bright "Dead Cells look" comes from saturated lighting colour per biome plus strong figure–ground framing (dark foreground masks, hazy light backgrounds), not from bright sprites.

### Gaps
- The Dead Cells HUD layout and fonts were not visible in any Steam media sampled. A wiki or press-kit source is needed.
- The article states no frame rates or frame counts for Dead Cells animations (per the fetched summary). Hit-stop durations are unverified.

## 3. Other references: Halls of Torment, Children of Morta, Blasphemous, Skul, Hyper Light Drifter, Eastward, CrossCode, Absolum, Hades

### Takeaway
Most references lock to a low internal resolution: 640×360 is the modal choice (Blasphemous, Skul, Eastward, DMD, Dead Cells). Hyper Light Drifter uses 480×270, CrossCode 568×320, Halls of Torment 960×540 and Children of Morta 800×450 (a non-integer 2.4×). Main characters are about 7–14% of screen height in all the pixel games. Each scene leans on one dominant hue (up to 97% of saturated pixels). Readability comes from value contrast and reserved accent colours rather than outlines. Absolum and Hades show the non-pixel alternative: thick ink outlines with painted art.

### Cited Findings
#### Measurement table (my measurements; one representative shot linked per row)
| Game | Image size → period P | Internal res / scale | Grid strictness (phase) | Main character height | Representative shot |
|---|---|---|---|---|---|
| Death Must Die | 1920×1080 → 3.00 | 640×360, 3× | 1.10–2.07 | ≈35–40 px (≈10–11%) | [DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg) |
| Dead Cells | 1916×1080 → 2.99–3.00 | ≈640×360, ≈3× | 1.02–1.97 | ≈50 px (≈14%) per [article](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-) | [DC-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_b7cf2ca21fe3648c53f808b80393cc727815dcc5.1920x1080.jpg) |
| Halls of Torment | 1920×1080 → 2.00 | 960×540, 2× | 1.10–1.68 | ≈42 px (≈8%) | [HoT-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2218750/ss_75fc37b9c2a3b30c01c03d2f3cb86cf6a754ccc2.1920x1080.jpg) |
| Children of Morta | 1920×1080 → 2.40 (some 3.00/2.22) | ≈800×450, 2.4× (non-integer) | 1.0–1.4 | ≈12% of screen | [CoM-ss10](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/330020/ss_ffcc7c2898adaf56c8302e17d5a2297a5abc2bfc.1920x1080.jpg) |
| Blasphemous | 1920×1080/1041 → 3.00 (15 of 18) | 640×360, 3× | 1.7–2.75 (strictest) | ≈48 px incl. hood (≈13%) | [Blas-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_957b6216519c614984ee71c6f6a524d75ea6353b.1920x1080.jpg) |
| Skul | 1920×1080 → 3.00; 1280×720 → 2.00 | 640×360 | 1.05–1.53 | ≈26 px (≈7%) | [Skul-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1147560/ss_eb69096a8d5b1febdf0b0374e1ce513c2b3549d9.1920x1080.jpg) |
| Hyper Light Drifter | 1920×1080 → 4.00 | 480×270, 4× | 2.06–3.54 | ≈21 px (≈8%) | [HLD-ss4](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/257850/ss_b38d4392356a39a745fd18c032195a71c40d038b.1920x1080.jpg) |
| Eastward | 1920×1080 → 3.00 (some 3.60) | 640×360 (some shots ≈533×300) | 1.0–1.46 | ≈9–11% | [East-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/977880/ss_d33bc97134437987ed89194d04b39f2e4658df20.1920x1080.jpg) |
| CrossCode | 1136×640 → 2.00 | 568×320, 2× | mostly 1.56–1.97 | ≈32 px (≈10%) | [CC-ss2](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/368340/ss_7fecc64f6470652a1464371fa9e7a767a84fd95d.1920x1080.jpg) |
| Absolum | 1920×1080 → no grid | HD hand-drawn | ≈1.0 | ≈35–45% (close beat-'em-up camera) | [Abs-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1904480/ea0656207cbaf00003294eee4ce963b6df166916/ss_ea0656207cbaf00003294eee4ce963b6df166916.1920x1080.jpg) |
| Hades | 1920×1080 → no grid | HD painted | ≈1.0 | ≈10% | [Hades-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1145360/ss_c0fed447426b69981cf1721756acf75369801b31.1920x1080.jpg) |
- Several "1920x1080" Steam files are not that size: Skul has 1280×720 files and CrossCode 1136×640 files. Normalising by real image width gives a consistent 640×360 for Skul and 568×320 for CrossCode ([Skul-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1147560/ss_eb69096a8d5b1febdf0b0374e1ce513c2b3549d9.1920x1080.jpg) is 1280×720, [Skul-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1147560/ss_b5ed9f9f5c95df4e168107d024cd4107bc2223e3.1920x1080.jpg) is 1920×1080; [CC-ss2](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/368340/ss_7fecc64f6470652a1464371fa9e7a767a84fd95d.1920x1080.jpg) is 1136×640).

#### Halls of Torment (survivors-like, closest genre)
- Developer framing: "Old school pre-rendered art style" ([Steam appdetails 2218750](https://store.steampowered.com/api/appdetails?appids=2218750)).
- Look: a Diablo-1/2 style ¾ view with a very desaturated grey stone floor (meanS 0.18, meanV 0.23, centre/edge 0.41/0.19), skeleton hordes, white-blue shuriken projectiles and soft fuzzy glow blobs. Sprites look like soft-shaded low-res pre-renders at 2× ([HoT-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2218750/ss_75fc37b9c2a3b30c01c03d2f3cb86cf6a754ccc2.1920x1080.jpg)).
- Damage numbers are large serif digits with dark outlines: white for normal, yellow with a flame icon for burn, big red for crits ("1657"). They are dense across the screen ([HoT-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2218750/ss_75fc37b9c2a3b30c01c03d2f3cb86cf6a754ccc2.1920x1080.jpg)).
- HUD: stone and metal side panels frame a 4:3-like playfield. The left panel holds a vertical blue XP bar and a red health orb with numeric readouts. The right panel holds a red "MENU" button, painted ability icons, passive percentages, gold, kills and the timer ([HoT-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2218750/ss_75fc37b9c2a3b30c01c03d2f3cb86cf6a754ccc2.1920x1080.jpg)). The Game Over screen is gold-framed stats on red ([HoT-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2218750/ss_6e32e4aa9261c60115c89fc608c5e156839b2709.1920x1080.jpg)).
- Late-game chaos: purple orbs, shuriken, spiked flails, translucent purple AoE circles and red lasers fill the field, and the player is hard to locate ([HoT-ss4](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2218750/ss_7bb48f7c67941509b73fc36929440fe663b5b85d.1920x1080.jpg)).

#### Children of Morta
- Developer framing: "hand-painted pixel art and frame-by-frame animations partnered with modern lighting techniques" ([Steam appdetails 330020](https://store.steampowered.com/api/appdetails?appids=330020)).
- In combat, a pixel-art orange crescent smear with jagged edges sits next to smooth cyan glow trails (mixed resolution). One character is drawn as a solid red silhouette, a tint state. Enemy health bars are thin, purple pixel debris litters the floor, and the HUD uses teal ornamental frames with a diamond portrait, a pixel font, a centred objective bar and a minimap ([CoM-ss3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/330020/ss_7e1a090f480f8392e1e1ae019624425a8ded8484.1920x1080.jpg)).
- The hub is a "dollhouse" cross-section of the family house: dense ornamental carpets, a warm fireplace glow and a pixel-font dialogue line ([CoM-ss10](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/330020/ss_ffcc7c2898adaf56c8302e17d5a2297a5abc2bfc.1920x1080.jpg)).

#### Blasphemous
- Developer framing: "dark gothic world" and "pixel-perfect execution animations" ([Steam appdetails 774361](https://store.steampowered.com/api/appdetails?appids=774361)).
- Scenes are near-monochrome:
  - Red 97% in a sunset bridge scene ([Blas-ss6](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_959ffc1230d2ca57e18a9c96f3c56f5643b5cce7.1920x1080.jpg)).
  - Cyan 97% and meanV 0.89 in a snowfield ([Blas-ss14](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_721ef608d14271d08cd938015af8c7ba84c5f32e.1920x1080.jpg)).
  - Dark interiors at meanV 0.10 with 89% dark pixels ([Blas-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_957b6216519c614984ee71c6f6a524d75ea6353b.1920x1080.jpg)).
- The slash smear is fully pixel art on the grid: a big white crescent with light-cyan edges and dark speckles, plus floor sparks ([Blas-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_957b6216519c614984ee71c6f6a524d75ea6353b.1920x1080.jpg)).
- Huge bosses dwarf the player (several times its height) and get a boss name bar at the bottom in an ornate frame ([Blas-ss4](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_933510ac0fd13c6bd4ecc4c187e0506d520f2e70.1920x1080.jpg), [Blas-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_67e9de6ea4dc169d21aaacb3d8625e2e0b3e025d.1920x1080.jpg)). The top-left HUD is an ornate gold emblem with red and blue bars and flask icons, with a currency counter at top right (same shots).

#### Skul: The Hero Slayer
- Bright, cartoony palette (meanV 0.59, centre/edge 0.74/0.51) with pastel parallax forest backgrounds and saturated green foreground. The hero dashes with beige speed streaks. Red slash lines have white cores and soft red glow. Enemy health bars are red. The HUD has a skull portrait at bottom left with A/S/D skill icons, a purple HP bar ("60 / 100"), and a minimap at bottom right ([Skul-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1147560/ss_eb69096a8d5b1febdf0b0374e1ce513c2b3549d9.1920x1080.jpg)). A boss health bar sits at the top ([Skul-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1147560/ss_43e7cf01e7f6066fadf8bd725aa437311820a755.1920x1080.jpg)).

#### Hyper Light Drifter
- Developer framing: "in the vein of the best 16-bit classics" and "everything is lovingly hand-animated" ([Steam appdetails 257850](https://store.steampowered.com/api/appdetails?appids=257850)).
- Flat-colour graphic shapes with minimal texture. The palette is hot magenta and pink, teal and cyan, and deep indigo (meanS up to 0.76). An enemy's hot-pink laser stands out against an indigo floor. The HUD is only small green health pips and a pink bar at top left ([HLD-ss4](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/257850/ss_b38d4392356a39a745fd18c032195a71c40d038b.1920x1080.jpg)).
- Lighting is expressed as flat posterised shapes (a pale lilac floor patch), with no smooth gradients ([HLD-ss7](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/257850/ss_102dcb97e5b22daf206a3432e4f22415f135c46a.1920x1080.jpg)).

#### Eastward
- Very dense hand-made environment pixel art. Characters have chibi proportions with dark selective outlines. Lighting is soft: a theatre spotlight cone, a purple ambient and a vignette ([East-ss3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/977880/ss_2c7516dbcfba035a359a2eb7257356809a3f5eaa.1920x1080.jpg)).
- A blue night palette with saturated yellow-orange fire bursts and green lamp light; the complementary accent marks the combat VFX. The HUD has an illustrated portrait, a row of hearts and "Ammo"/"Bomb" boxes at bottom left, with currency at top right ([East-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/977880/ss_d33bc97134437987ed89194d04b39f2e4658df20.1920x1080.jpg)).

#### CrossCode
- Developer framing: "16-bit SNES-style graphics" ([Steam appdetails 368340](https://store.steampowered.com/api/appdetails?appids=368340)).
- In a boss fight, a large mechanical boss throws pixel-art ice-crystal eruptions with soft white mist. The HUD is a sleek angular sci-fi design: a hex emblem with "HP 749" and a green bar at top left, "RANK D" at top right, and a long skewed red "BOSS" bar at the bottom ([CC-ss2](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/368340/ss_7fecc64f6470652a1464371fa9e7a767a84fd95d.1920x1080.jpg)). Dialogues use high-resolution anime portraits over the pixel world ([CC-ss13](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/368340/ss_dd4ca2cbdf4f9339ff270428fc262494770010af.1920x1080.jpg)).

#### Absolum (non-pixel comparison)
- No pixel grid: hand-drawn 2D with thick black ink outlines and flat cel shading. Saturated complementary VFX include a lime beam with a yellow-white core. A "10 hits" combo counter sits next to a top-left circular portrait in a gold ring with red and blue bars ([Abs-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1904480/ea0656207cbaf00003294eee4ce963b6df166916/ss_ea0656207cbaf00003294eee4ce963b6df166916.1920x1080.jpg)). Developer framing: "Supamonks' best-in-class art and animation" ([Steam appdetails 1904480](https://store.steampowered.com/api/appdetails?appids=1904480)).

#### Hades (non-pixel comparison)
- Isometric hand-painted art ("spectacular hand-painted environments", per [Steam appdetails 1145360](https://store.steampowered.com/api/appdetails?appids=1145360)) with heavy blacks.
- In one combat shot ([Hades-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1145360/ss_c0fed447426b69981cf1721756acf75369801b31.1920x1080.jpg)):
  - A white-and-orange crescent slash.
  - A boss name and bar at top centre ("MEGAERA, THE FURY").
  - A boons column of diamond icons on the left.
  - HP "23/100" at bottom left.
  - A red glow along the screen edges at low HP (centre/edge 0.59/0.19).

### Inferences
- 640×360 at an integer scale is the industry-proven default for this genre: it maps to 3× at 1080p, 4× at 1440p and 6× at 4K. HLD (480×270) and CrossCode (568×320) trade field of view for chunkier pixels.
- Pixel games split into "strict grid" (Blasphemous, HLD: VFX and lighting stay on the grid as flat shapes) and "hybrid" (Dead Cells, CoM, Eastward, Skul, DMD: pixel sprites plus smooth glows, bloom and gradient light). Both read as premium. What matters is that each element category is consistent.
- Halls of Torment shows the risk for a survivors-like: when VFX and damage numbers are not budgeted, late-game frames become unreadable.

### Gaps
- I took no primary-source confirmation of the measured resolutions for HLD, Blasphemous, CrossCode, Skul, Eastward or CoM (web search unavailable). The values are measurements only. Mixed periods in Eastward and CoM suggest some marketing shots were captured at other window sizes or zooms.
- Eastward, CrossCode, Skul and HLD were judged from a few screenshots each. Their animation and juice was not sampled.

## 4. Animation and juice cues visible in screenshots, clips and trailer frames

### Takeaway
The juice vocabulary is consistent across the references:
- big crescent smears on swings (pixel or vector);
- hit flashes that turn the target pale or white for a few frames;
- star-burst sparks plus blood or debris on contact;
- telegraphed wind-ups for enemy attacks;
- full-screen flashes and room-relighting fire reserved for big moments;
- gore or death bursts that leave remains;
- dash afterimages or rings.

Hit-stop is suggested by duplicated frames in Blasphemous. Screen shake is visible only indirectly as frame-to-frame camera offsets in Dead Cells.

### Cited Findings
- **Smears and anticipation:**
  - DMD: an orange crescent smear framing the player's swing ([DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg)) and a white crescent slash ([DMD-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_144e73f2fbd60444935777044a322a9c87145096.1920x1080.jpg)).
  - Blasphemous: a white pixel crescent ([Blas-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_957b6216519c614984ee71c6f6a524d75ea6353b.1920x1080.jpg)).
  - CoM: an orange pixel crescent ([CoM-ss3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/330020/ss_7e1a090f480f8392e1e1ae019624425a8ded8484.1920x1080.jpg)).
  - Hades: a white/orange crescent ([Hades-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1145360/ss_c0fed447426b69981cf1721756acf75369801b31.1920x1080.jpg)).
  - Dead Cells: a smooth red enemy arc ([DC-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_ac28000ade40cc2fe5c128f32ac98ba33c008a7a.1920x1080.jpg)).
- **Enemy wind-up telegraphs as explicit design work:** DMD patches add wind-up animations to Wraith Dash and Fungus and spawn/death animations to Stone Golem ([Patch 0.8.4](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=1&maxlength=0&enddate=1746829496&format=json), [Patch 0.8.5](https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=2334730&count=1&maxlength=0&enddate=1747939686&format=json)). Blasphemous boss attacks run blue pixel fire along the floor ([Blasphemous boss clip @4.81 s](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/extras/19bc6758ba0cf580357b3a5b78098db9.mp4)).
- **Hit flash, with frames that suggest hit-stop:** in the Blasphemous boss clip (616×252), sampled every 0.067 s:
  - The boss sprite turns pale cream at 0.000–0.134 s and again at 0.335 s, and is its normal dark grey-brown at 0.201–0.268 s and 0.402–0.536 s.
  - Orange-yellow spark bursts and red blood spray appear at each contact.
  - The frames at 0.000 s and 0.067 s are visually identical (same player pose and sparks).
  - Source: [Blasphemous store clip](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/extras/19bc6758ba0cf580357b3a5b78098db9.mp4).
- **Hit sparks:** white star bursts with coloured rays, a lavender chain-lightning crackle and white damage numbers popping over targets ([DMD trailer @22.2/22.8 s](https://video.akamai.steamstatic.com/store_trailers/2334730/545241/d38673966116cba081951e39fa8077119021fd9d/1750812009/hls_264_master.m3u8)). Pink-orange burst clouds come with thin enemy health bars ([CoM-ss3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/330020/ss_7e1a090f480f8392e1e1ae019624425a8ded8484.1920x1080.jpg)).
- **Full-screen flash, shockwave and possible shake (Dead Cells):** in [DC store clip 4](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/extras/7bb33ad5a5362f15ff5872ccf737fe36.mp4) a red crescent (0.02 s) is followed by a full-screen lilac-white flash (0.16 s) that fades by 0.31 s, then an orange shockwave sphere (0.45 s). Platform positions shift between the 0.45, 0.59 and 0.73 s frames.
- **Big attacks relight the environment:** a fire breath floods the whole room orange (1.11–1.38 s) in [DC store clip 1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/extras/e0eba0c607cfe799ed7b0a9cb53b022a.mp4). A fire pillar lights the floor in DMD ([trailer @56.0 s](https://video.akamai.steamstatic.com/store_trailers/2334730/545241/d38673966116cba081951e39fa8077119021fd9d/1750812009/hls_264_master.m3u8)).
- **Death effects:**
  - Dead Cells: enemies burst into large dark-red/black gore clouds with flying chunks, and the debris lingers ([DC clip 1 @2.17–2.96 s](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/extras/e0eba0c607cfe799ed7b0a9cb53b022a.mp4); [DC trailer @16.4 s](https://video.akamai.steamstatic.com/store_trailers/588650/231061/8fe407db503493211097e9898f58ae600559a2a3/1750557235/hls_264_master.m3u8)).
  - DMD: skeletons leave bone piles ([DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg)).
- **Dashes:**
  - DMD: a white-cyan horizontal streak and afterimage ([DMD-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_fe6bef6f687f28d552fca8f2c605d5f8cbb06b61.1920x1080.jpg)).
  - HLD: white outlined ellipse rings on the floor plus a cyan ghost burst ([HLD-ss7](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/257850/ss_102dcb97e5b22daf206a3432e4f22415f135c46a.1920x1080.jpg)).
  - Skul: beige speed streaks ([Skul-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1147560/ss_eb69096a8d5b1febdf0b0374e1ce513c2b3549d9.1920x1080.jpg)).
- **UI juice:**
  - Halls of Torment: a wooden chest panel opens to reveal "Choose One Item" with three item icons and gold-bordered tooltips ([HoT store clip](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2218750/extras/f0c9400d9fab239e9c04135b05627220.mp4)).
  - Absolum: a combo counter ([Abs-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1904480/ea0656207cbaf00003294eee4ce963b6df166916/ss_ea0656207cbaf00003294eee4ce963b6df166916.1920x1080.jpg)).
  - Hades: a low-HP red edge glow ([Hades-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1145360/ss_c0fed447426b69981cf1721756acf75369801b31.1920x1080.jpg)).
- **Animation craft, primary source:** Dead Cells keys the fewest frames possible pose-to-pose, adds in-betweens only next to keys, and lets VFX convey impact ([Game Developer article](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)).

### Inferences
- The identical Blasphemous frames at 0.000 and 0.067 s, during a hit flash, are consistent with a hit-stop of roughly 60–130 ms. This is an inference from 15 fps sampling of a compressed clip, not a measurement of the game.
- The shifting platform positions in the Dead Cells clip are consistent with screen shake, though a camera follow move cannot be ruled out.
- A reusable "impact recipe" emerges: a wind-up (enemy) or smear (player), contact sparks in a white core with coloured rays, a 2–4 frame target flash, an optional brief freeze, then a death burst that leaves a remain, and a full-screen flash or relight only for ultimates and boss phases.

### Gaps
- I did not observe or measure DMD's screen shake, hit-stop or enemy hit-flash timings. That would need gameplay capture at 60 fps.
- The juice of HLD, CrossCode, Eastward and Skul was not sampled from video.

## 5. Common patterns: art-direction checklist and 2–3 style directions for a new original-IP pixel-art action roguelite

### Takeaway
Fix one internal resolution, preferably 640×360 at an integer scale, and keep the world on that grid. Design value first: a dark or desaturated world, with light and saturation reserved for the player, threats and rewards. Give each biome one dominant hue plus one complementary accent. Build a consistent impact recipe. Make the HUD and menus look like they belong to the world (themed frames, painted portraits) rather than to a web app. Three viable directions are gothic chiaroscuro (DMD/Blasphemous/HoT), saturated biome-pop (Dead Cells/Skul/HLD) and a painterly-lit hybrid (CoM/Eastward, or HD like Hades/Absolum).

### Cited Findings
- Internal resolution clusters at 640×360: DMD, Dead Cells, Blasphemous, Skul and most Eastward shots measure P = 3.00 at 1080p ([DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg), [DC-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_b7cf2ca21fe3648c53f808b80393cc727815dcc5.1920x1080.jpg), [Blas-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_67e9de6ea4dc169d21aaacb3d8625e2e0b3e025d.1920x1080.jpg), [Skul-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1147560/ss_b5ed9f9f5c95df4e168107d024cd4107bc2223e3.1920x1080.jpg), [East-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/977880/ss_d33bc97134437987ed89194d04b39f2e4658df20.1920x1080.jpg)). HLD uses 480×270 ([HLD-ss4](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/257850/ss_b38d4392356a39a745fd18c032195a71c40d038b.1920x1080.jpg)), CrossCode 568×320 ([CC-ss2](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/368340/ss_7fecc64f6470652a1464371fa9e7a767a84fd95d.1920x1080.jpg)) and Halls of Torment 960×540 ([HoT-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2218750/ss_75fc37b9c2a3b30c01c03d2f3cb86cf6a754ccc2.1920x1080.jpg)).
- Character height is about 21–50 native px, or 7–14% of screen height, across all the pixel references (table in §3). Dead Cells' artist explicitly sized characters at about 50 px ([article](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)).
- Dominant-hue scenes recur:
  - DMD: yellow-green 44% + green 19% ([DMD-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_fe6bef6f687f28d552fca8f2c605d5f8cbb06b61.1920x1080.jpg)).
  - Dead Cells: cyan 78% ([DC-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_b7cf2ca21fe3648c53f808b80393cc727815dcc5.1920x1080.jpg)).
  - Blasphemous: red 97% ([Blas-ss6](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_959ffc1230d2ca57e18a9c96f3c56f5643b5cce7.1920x1080.jpg)).
- Lit-centre composition is common: DMD centre/edge reaches 0.48/0.16 ([DMD-ss6](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_9581b0eb51e4d50668e0394d44d22e25f3a56e62.1920x1080.jpg)), Hades 0.59/0.19 ([Hades-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1145360/ss_c0fed447426b69981cf1721756acf75369801b31.1920x1080.jpg)) and HLD 0.79/0.19 ([HLD-ss7](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/257850/ss_102dcb97e5b22daf206a3432e4f22415f135c46a.1920x1080.jpg)).
- Themed UI frames are universal:
  - DMD: gothic thorn frames ([DMD-ss4](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_df843bd57ff67a49427c59912cd025016e4f2255.1920x1080.jpg)).
  - Halls of Torment: Diablo stone panels ([HoT-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2218750/ss_75fc37b9c2a3b30c01c03d2f3cb86cf6a754ccc2.1920x1080.jpg)).
  - CoM: teal ornament ([CoM-ss3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/330020/ss_7e1a090f480f8392e1e1ae019624425a8ded8484.1920x1080.jpg)).
  - Blasphemous: gold reliquary frames ([Blas-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_67e9de6ea4dc169d21aaacb3d8625e2e0b3e025d.1920x1080.jpg)).
  - CrossCode: angular sci-fi ([CC-ss2](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/368340/ss_7fecc64f6470652a1464371fa9e7a767a84fd95d.1920x1080.jpg)).
- Painted high-resolution portraits sit over pixel worlds for NPCs and gods: DMD ([DMD-ss2](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_d2eec3ed8b34e3b16b9ee97dd47bb5c30ea6c2af.1920x1080.jpg)), CrossCode ([CC-ss13](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/368340/ss_dd4ca2cbdf4f9339ff270428fc262494770010af.1920x1080.jpg)) and Eastward's HUD portrait ([East-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/977880/ss_d33bc97134437987ed89194d04b39f2e4658df20.1920x1080.jpg)).

### Inferences
#### Art-direction checklist: do
1. **Grid:** render the world to a fixed 640×360 target (or 480×270 for chunkier art) and upscale by an integer with nearest-neighbour; letterbox or extend the field of view instead of using fractional scales. Text and UI may be high-resolution but must be consistently so (DMD-style serif) or consistently pixel (CoM, CrossCode).
2. **Sprite budget:** player 32–48 native px tall (≈10–13% of screen height). Standard enemies 60–100% of the player. Elites 1.5–2×. Bosses 3–6× (Blasphemous scale drama).
3. **Value plan first:** environment in the darkest 20–40% of the value range with low saturation. The player gets a light pool or rim light. Threats and rewards use the brightest values and saturations. Check this with a greyscale screenshot.
4. **One hue per biome plus one complementary accent** (green forest with red/white attacks, indigo tomb with orange light). Rotate biome hues across the run so every act reads differently in a thumbnail.
5. **Coloured light:** warm point lights against a cool ambient, and darkness as a mood dial (DMD's "global darkness" plus light sources). Normal-mapped sprites (Dead Cells) or a simple light-radius shader are enough.
6. **Figure–ground layering:** dark flat foreground masks, a mid-value play layer, hazy lighter backgrounds (side view). In top-down, a low-contrast ground texture and props with clear silhouettes.
7. **VFX language:** big simple shapes (crescents, rings, pillars, beams) with a white-hot core and 1–2 colours. Use additive glow sparingly on top of the grid. Colour VFX by owner: player in warm or white tones, enemy telegraphs in one reserved colour (hot pink in HLD, red arcs in Dead Cells).
8. **Impact recipe:** wind-up or telegraph, then smear, then white core sparks, then a 2–4 frame target flash, then a short freeze on heavy hits, then a death burst with remains. Save full-screen flashes and relights for ultimates and bosses.
9. **Numbers and labels:** small and desaturated for normal hits (DMD), colour-coded by type, enlarged only for crits (HoT). Set a density cap so they never bury the player.
10. **Themed UI:** an edge-hugging minimal HUD (portrait, HP, charges, timer, minimap). Frames, ornaments and fonts come from the world's material culture. Boon, level-up and shop screens are the "showcase" UI with painted icons or portraits and rarity-coloured borders.
11. **Hub as a mood piece:** one strong light motif (DMD moonlit ritual circle, CoM fireplace house).

#### Art-direction checklist: don't
1. Don't mix pixel densities inside the world layer (fine 1 CSS px "art pixels" next to chunky tiles, or sprites scaled by non-integer factors).
2. Don't light everything evenly. A flat, uniformly lit ground kills focus.
3. Don't give ground textures detail at the same scale and contrast as character details. Noise competes with silhouettes.
4. Don't let auto-exposed 3D renders decide colour. Map sprites to a curated palette or ramp (the toon-shader step in Dead Cells).
5. Don't let late-game VFX and numbers cover the player (HoT-ss4 is the anti-example).
6. Don't use generic web-app UI (rounded pills, system sans, flat cards) for in-run HUD or reward screens in a game meant to sell on style.

#### Style directions
- **A. "Gothic chiaroscuro"** (DMD + Blasphemous + Halls of Torment). 640×360 at 3×. Near-black environments with a light radius, desaturated bone, ash and moss, saturated red, orange and teal magic, thorn-and-serif UI, painted god portraits. Strongest fit for a survivors-like with hordes, because value contrast scales with density. References: [DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg), [DMD-ss9](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_fe6bef6f687f28d552fca8f2c605d5f8cbb06b61.1920x1080.jpg), [DMD trailer @36.5 s](https://video.akamai.steamstatic.com/store_trailers/2334730/545241/d38673966116cba081951e39fa8077119021fd9d/1750812009/hls_264_master.m3u8), [Blas-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/774361/ss_957b6216519c614984ee71c6f6a524d75ea6353b.1920x1080.jpg), [HoT-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2218750/ss_75fc37b9c2a3b30c01c03d2f3cb86cf6a754ccc2.1920x1080.jpg).
- **B. "Saturated biome-pop"** (Dead Cells + Skul + HLD + CrossCode). Brighter scenes (meanV 0.3–0.65), each biome a strong single hue, HD glow arcs over pixel sprites, gore and flash juice, compact HUD. The 3D-to-pixel pipeline (Dead Cells) suits AI-assisted production with a Blender toolchain. References: [DC-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_ac28000ade40cc2fe5c128f32ac98ba33c008a7a.1920x1080.jpg), [DC-ss3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/588650/ss_e87e72a247918d8493892e035d5e1b4b84470d2f.1920x1080.jpg), [Skul-ss8](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1147560/ss_eb69096a8d5b1febdf0b0374e1ce513c2b3549d9.1920x1080.jpg), [HLD-ss4](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/257850/ss_b38d4392356a39a745fd18c032195a71c40d038b.1920x1080.jpg).
- **C. "Painterly-lit hybrid"** (Children of Morta + Eastward; HD fallback Hades/Absolum). Hand-painted, dense pixel art with modern dynamic light, god rays and bloom, a dollhouse hub, illustrated portraits. If pixel-art cost is too high, go HD-painted with thick outlines (Hades/Absolum) and keep the same value and hue rules. References: [CoM-ss3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/330020/ss_7e1a090f480f8392e1e1ae019624425a8ded8484.1920x1080.jpg), [CoM-ss10](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/330020/ss_ffcc7c2898adaf56c8302e17d5a2297a5abc2bfc.1920x1080.jpg), [East-ss3](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/977880/ss_2c7516dbcfba035a359a2eb7257356809a3f5eaa.1920x1080.jpg), [Hades-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1145360/ss_c0fed447426b69981cf1721756acf75369801b31.1920x1080.jpg), [Abs-ss5](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1904480/ea0656207cbaf00003294eee4ce963b6df166916/ss_ea0656207cbaf00003294eee4ce963b6df166916.1920x1080.jpg).

### Gaps
- Direction-specific production costs (frames per character, artist-hours) were not researched. The only primary pipeline source found is the Dead Cells article.
- No audience or market data links a given style to sales. That is outside the scope of this visual study.

## 6. Current aegis-draft Arcade: how it looks and what it lacks vs. the references

### Takeaway
The live Arcade (checked on 2026-10-25 for about 8 minutes: menu plus about 22 seconds of a Juggernaut run on "Forest Siege") is functional and legible but does not yet look like a stylish pixel-art game:
- It draws 1 art pixel per CSS pixel, so about 1238×560 art pixels are visible, nearly twice DMD's 640×360 across.
- The ground is an evenly lit olive-green field with no light falloff (centre/edge 0.24/0.22, vs DMD's up to 0.48/0.16).
- Sprites are small, low-contrast pre-renders without curated palettes.
- VFX are minimal.
- The UI and menus are a clean web-app design (sans fonts, rounded pills, flat cards) rather than themed game UI.
- Its Valve and Liberated Pixel Cup (LPC) assets are explicitly non-commercial or copyleft, so a commercial original-IP game must replace all of them.

### Cited Findings
- **Mode entry:** the "New run" list shows "06 Arcade — 2D survivors roguelike · one hero vs the jungle". The setup screen has:
  - the headline "Twenty minutes to the Ancient";
  - hero cards with official Dota portraits, "MELEE/RANGED" labels and "Skins: N" pills;
  - act tabs ("Full act · 20 min", "Warm-up · until 9:00");
  - gear slots, "Cosmetics · 0/299", ability rows with Dota icons, starting-trait pills, mastery chips and legacy bonuses with pips;
  - a seed field, a large pale-yellow "Into the jungle →" button, an "Arcade daily" card and a replay-code field.
  - Overall look: dark rounded cards and a geometric sans display font, text-heavy, with no key art or backdrop scene ([live site](https://artemchikolaev.github.io/aegis-draft/)).
- **In-run canvas:** 2476×1120 device px shown as 1238×560 CSS at DPR 2. Measured edge period 2.00 device px (strength 0.966), so one art pixel equals one CSS pixel. A secondary period of 8.00 device px matches 4-art-pixel ground grain. The rest of the colour profile: meanV 0.22, 13% dark pixels, meanS 0.51, centre/edge 0.24/0.22, and 88% of saturated pixels in the 60–90° (olive/yellow-green) hue bin ([live site](https://artemchikolaev.github.io/aegis-draft/), my canvas measurement).
- The code comment says the default factor aims for "~2 physical pixels per art pixel, like Death Must Die at 1080p" (factor 1 on DPR ≥ 1.5, factor 2 otherwise) — [web/src/features/arcade/pixelMode.ts](file:///Users/Shared/Coding/aegis-draft/web/src/features/arcade/pixelMode.ts). DMD's Steam screenshots measure 3 physical px per art pixel at 1080p, because a fixed 640×360 is scaled to the screen ([DMD-ss1](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2334730/ss_4c5dc0f9dde1a7dd5a73db9df0a7a6cc4f7b3ca9.1920x1080.jpg)).
- **World look:**
  - Large square ground tiles in 2–3 close dark-green shades, sparse grass tufts and grey stones, tree canopies drawn as flat green circles, a pond with lily pads inside a dashed white ring, a brown dirt patch.
  - The hero (≈10% of view height) has a yellow selection ring at his feet. Small brown kobold-like enemies are ≈5–6% of view height.
  - Bold white sans damage numbers with dark outlines ("24", "44"); one enemy shown in a white hit flash; a dropped item icon on a blue ring; off-screen arrow indicators.
  - Source: [live site](https://artemchikolaev.github.io/aegis-draft/), run at 0:11–0:22.
- **Up close:** the magnified canvas shows the ground as a camouflage-like olive noise of about 4-art-px blocks. The hero is a dark-brown silhouette with a speckled multicolour head texture and no outline or rim light; the yellow ring is the most legible element ([live site](https://artemchikolaev.github.io/aegis-draft/), 5.3× canvas crop around the hero).
- **HUD (DOM overlay):**
  - Top left: a large white timer "0:11", "Kills"/"Gold" counters, a "Forest Siege" pill, "HERALD ★" and six empty square item slots.
  - Top right: rounded "Build" and "Pause" pill buttons.
  - Bottom left: a Dota portrait, a wide rounded bright-green HP bar "616 / 620" and a thin level bar.
  - Bottom centre: six square ability buttons with Dota ability icons, key labels and "A" auto-cast badges.
  - Bottom right: a dark square minimap.
  - Source: [live site](https://artemchikolaev.github.io/aegis-draft/).
- **Level-up modal "LEVEL 2 · Pick one":** three dark rounded cards ("Skadi school · Attack — Frostbite", "Radiance school · Dash — Flame trail", "Ability · Q — Blade Fury") with small Dota icons, white and grey sans text, orange stat values, "Banish (3 left)" pills and "Other cards · 30 gold" ([live site](https://artemchikolaev.github.io/aegis-draft/)).
- **Licensing on the page itself:** "Heroes, creeps, Roshan, trees and attack sounds — Dota 2 models and sounds © Valve Corporation (non-commercial fan project). Other sprites and tiles: Liberated Pixel Cup … CC-BY-SA 3.0 / GPL 3.0." ([live site](https://artemchikolaev.github.io/aegis-draft/)).

### Inferences
Gaps against the references, ordered by visual impact:
1. **Pixel density.** About 1238 art px of width at a typical laptop window is 2× DMD's field of art pixels, so sprites read as tiny 3D renders rather than pixel art. A fixed internal resolution (e.g. 640×360) with integer upscaling would make each art pixel 3–4 physical px and give the chunky, deliberate look of the references. The pixelMode comment's DMD premise (2 px per art pixel) does not match the measured DMD screenshots (3 px at 1080p).
2. **Lighting and value.** There is no light pool, vignette, coloured point light or day/night mood; centre and edge are equally bright. DMD and Hades concentrate light on the action (centre/edge ≥ 2.5×).
3. **Palette.** The scene is one olive-green hue with low-contrast tiles, and actors are muddy brown. There is no reserved accent colour for player attacks, threats or pickups beyond the yellow ring.
4. **Silhouettes and readability.** There is no outline, rim light or curated ramp on sprites. Ground noise has similar contrast to sprite detail.
5. **VFX and juice.** In the sampled seconds I saw a white arc, rings, a white hit flash and damage numbers. I saw no smears, spark bursts, bloom, death bursts with remains or screen-level flashes. Late-game density was not observed.
6. **UI identity.** The HUD and menus are a well-made web-app design but carry no world-specific ornament, fonts or illustrated portraits. Reward cards are generic, whereas DMD's boon cards are a showcase moment.
7. **Commercial viability.** Every Dota and LPC asset must be replaced for an original-IP commercial game. The reusable parts are the code and pipeline lessons (Blender to sprite sheets, which matches the Dead Cells approach), not the art.

### Gaps
- Only about 22 seconds of an early run were observed (level 2, Herald, one hero, one map). Late-game density, bosses (Roshan, Tormentor, Ancient), other biomes and the wardrobe/cosmetic effects were not seen.
- The built-in browser pane was hidden, so screenshots were captured from a non-visible pane. Frames reflected states at capture time (a run auto-paused on the level-up modal). Animation smoothness and shake could not be judged.
- I left the run paused on the level-up screen and closed the tab. It may persist as a resumable run in the built-in browser's localStorage for artemchikolaev.github.io; the user's own browser is not affected.
