# Art & Animation Production Pipelines for an Original, High-Quality Pixel-Art Action Roguelite (tiny team with Blender/Python sprite-rendering experience)

*Research notes compiled 2026-10-25. Scope: production approaches, resolution/scaling, animation & game feel, VFX, lighting, tools (incl. AI), policy/copyright, costs, audio (brief), recommended pipeline. The web-search budget ran out near the end, so a few items are marked unverified in the Gaps. Facts about the owner's current toolchain come from the local doc `/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md`, which I read but did not modify.*

---

## 1. Production approaches compared: (a) hand-drawn, (b) 3D→pixel pre-rendered, (c) real-time 3D rendered as pixel art, (d) hybrids, (e) AI-assisted

### Takeaway
For a solo developer with a working Blender→sprite-sheet pipeline, the **3D→pixel pre-render route (the Dead Cells / Hades model)** gives the most animation quality per hour. Retakes are cheap, models and rigs are reusable, and extra directions cost only render time. The price is lower detail, residual "flickering pixels", and art direction has to be designed in rather than painted in. Hand-drawn art has the highest fidelity, but its cost grows linearly with frames × directions. Real-time 3D-as-pixel (t3ssel8r-style) is elegant but hard: you have to solve camera snapping, sub-pixel smoothing and horde-scale skinning. The proven commercial pattern is a **hybrid**: 3D-rendered characters plus hand-made environments and VFX.

### Cited Findings

#### (a) Hand-drawn pixel art
- Dead Cells' artist says that on earlier hand-drawn projects retakes were too costly, which hurt the final gameplay. This was a main reason Motion Twin moved to 3D. — [Game Developer, T. Vasseur (2018, per brief)](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)
- Typical frame counts quoted by general (low-authority) guides: idle/breathing 2–4 frames, walk 4–8, run 6–8, attack/action 6–12. A subtle idle can be 2–3 drawings. — [Divoom guide](https://divoom.com/blogs/setup-ideas/pixel-art-animation-frame-rate); most idle loops in shipped games use 2–4 frames with long holds, and timing matters more than frame count — [Sprite-AI guide](https://www.sprite-ai.art/guides/how-to-animate-pixel-art); the same vendor blog claims Celeste's Madeline run cycle is 4 frames (unverified) — [Sprite-AI blog](https://www.sprite-ai.art/blog/sprite-animation-frames)
- Time per frame, from search-snippet-level sources I could not open in full:
  - ~10 min per frame for a 75×60 px hobby animation (4 frames ≈ 40 min) — [itch.io devlog](https://itch.io/devlog/1084357/devlog-for-v10.amp)
  - A freelancer rule of thumb of **15 min to 1 h per frame** depending on complexity and skill, with ~60 unique frames for a 4-animation character — [OpenGameArt comments](https://lpc.opengameart.org/user/68/comments?page=16)
- Smear frames, used sparingly, help very quick actions. Anticipation (wind-up) frames can be held as long as needed. — [Clip Studio tips (smear)](https://tips.clip-studio.com/en-us/articles/6170); [Sprite-AI guide](https://www.sprite-ai.art/guides/how-to-animate-pixel-art)

#### (b) 3D→pixel pre-rendered sprites
- **Dead Cells pipeline**, per the Game Developer deep dive:
  - Characters and skeletons were built in 3ds Max from a 2D pixel model sheet and exported to FBX.
  - A homebrew tool renders the mesh at very small size with **no anti-aliasing**.
  - In-game characters are **~50 px tall**, so detailed modeling was judged not cost-efficient.
  - Volume comes from **normal maps** with a basic **toon shader**.
  - Each frame is exported as a PNG **plus its normal map**, which feeds a lighting system that avoids hand-drawn shadows.
  - Frame sequences let blend modes be layered for effects.
  - Old models and components are reused for new monsters; the author calls this the most useful trick. Weapons and armor are attached to existing models.
  - Animations are **keyed by hand** on 3D skeletons: keyframes first with the fewest frames, interpolation frames only before or after keyframes (never between), attacks pose-to-pose. Movement and impact are conveyed with VFX rather than extra frames.
  - Timing can be retaken "dozens of times in minutes".
  - Open problem: "we still haven't found any solution for **flickering pixels**". Hand cleanup was rejected to keep the workflow fast, and the lower detail versus hand-drawn was accepted ("Movement is love, movement is life").
  - The ScarKrow prototype reportedly ran its animations at 30 fps; Dead Cells' own frame rate is not stated.
  - Source: [Game Developer](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)
- **Hades (Supergiant)** does not render 3D in real time. Many characters (e.g., Zagreus) are 3D-modeled and animated during development, then baked into "thousands upon thousands" of frames from many angles. Most environment art is hand-painted 2D. — [Supergiant developer on Steam forums](https://steamcommunity.com/app/1145360/discussions/0/1738883810796606862)
- Talk summaries say Supergiant's 3D artist builds characters from the 2D portrait concept (ZBrush → Maya → Substance), starting from base models of existing characters such as Thanatos and Megaera. These are secondary summaries. — [Tripo blog summary](https://www.tripo3d.ai/blog/collect/from--d-art-to--d-masterpiece--the-process-of-bringing-characters-to-life-in-hades-cyj6d1ifsqa); [DTF write-up (RU)](https://dtf.ru/id64954/238431-sozdanie-materi-nochi-process-modelirovaniya-nikty-iz-hades)
- Hades II roughly doubled the content of the original and needed a rethought, optimized rendering pipeline. — [DigiPen](https://www.digipen.edu/showcase/news/hades-2-graphic-programmer)
- **Blender tooling exists off the shelf:**
  - "True Pixel Art Generator" add-on: sprite sizes 32/64/128, colour reduction + dithering, an "Outline Pixels" final outline, lighting presets, an "Intelligent Scale Lock" to keep pixel density consistent while framing (anti-crawl), and sprite-sheet export with exact frame sampling. $1, Blender 4–5. — [Superhive docs](https://superhivemarket.com/products/true-pixel-art-generator-v2/docs)
  - Astropulse (author of Retro Diffusion) sells a "Blender to Pixels" tool. — [itch.io](https://astropulse.itch.io/blender-to-pixels)
  - A free "Pixel Art Shader" (T. Kushizaki) uses a palette texture to restrict colours. — [CG Channel](https://www.cgchannel.com/?p=67534)
  - A 2017 fully scripted Blender→spritesheet pipeline used ImageMagick touch-ups instead of custom shaders. — [FreeGameDev forum](https://forum.freegamedev.net/viewtopic.php?p=72603)
- For 3D models with pixel-art textures, artists cite **texel density** ("pixels per centimeter") and clean UVs as the key factors. — [80.lv](https://80.lv/articles/take-a-look-at-this-scene-of-pixel-style-snowy-japanese-town); see also the node-based pixelated-texture tutorial — [80.lv](https://80.lv/articles/tutorial-easy-3d-pixel-art-texture-in-blender/)

#### Lessons from the owner's existing Blender pipeline (aegis-draft, Dota models)
All points below come from [docs/arcade-dota-sprites.md](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md).
- **Pixel mode** renders with Workbench: flat texture colour, studio light, a black silhouette outline, cavity for edges, and **no anti-aliasing**.
  - Pilot frames were 48 px for heroes, 36 for creeps and 72 for trees. They were later 80 px (`dota_px`) / 160 px (`dota_px2`) frames with a 1.4 margin.
  - `--world` = 2× the frame, so the sprite lands 1:1 on pixels at internal scale ×2.
  - Palette: `pngquant --nofs 48` (48 colours, no dithering).
  - Camera pitch 42° "reads like RTS/DMD"; 58° is closer to Dota but gives worse silhouettes.
- **Directions and frame rate:** 8 directions for heroes, 4 enough for creeps, 12 fps.
  - **Death clips rendered in 1 direction** (enemy death always drawn facing the camera) saved ~17–25% of sheet weight.
  - **Mirroring 3 of 8 directions** would save ~37%, but the weapon switches hands.
- **Anti-jitter fixes that were needed:**
  - Root-motion cancellation by the *carrier* bone (heaviest torso bone), not the root or head.
  - Automatic loop-period detection (autocorrelation) so 12 frames cover exactly one run cycle. Otherwise the run "strobes" between two poses.
  - A shared framing (`--fit-from`) across layers.
  - Raw, un-quantized sheets for layer compositing, because different palettes create noise.
- **Dark textures** needed auto-exposure, and emissive masks were added via glow options.
- **QA scripts** measure strobe, jerk, silhouette gaps, drift to the frame edge, chunks, darkness and empty frames over all sheets.
- **Throughput:** re-rendering ~47 sheets takes ~30 min. Sheets are lossless WebP, with a median ~0.3–0.9 MB per sheet depending on set.

#### (c) Real-time 3D rendered as pixel art (t3ssel8r-style)
- A recent paper (c. 2026; exact date not confirmed) cites t3ssel8r's YouTube video "3D pixel art rendering in Unity" (ID d6tp43wZqps). It notes that most 3D pixel-art techniques (t3ssel8r, ProPixelizer) snap pixels to a screen grid, which **only works with an orthographic camera**. Its "Texel Splatting" method targets perspective cameras (open-source paper + code). — [ResearchGate](https://www.researchgate.net/publication/402481025_Texel_Splatting_Perspective-Stable_3D_Pixel_Art); [author's X post](https://x.com/dylan_ebert_/status/2033315367766884602)
- **UPixelator (Unity)**, a shader-less asset, shows the anatomy of the technique:
  - Render to a low-res texture, then upscale.
  - Snap the camera and tagged objects to a world-space pixel grid to stop "pixel creep".
  - Offset the upscaled image by the snap error so camera motion stays smooth.
  - Moving objects must also snap, which adds zig-zag motion.
  - Alpha-clipped edges can still flicker. Orthographic only.
  - Rendering to a low-res texture can even save GPU when ≤~25% of screen pixels are drawn.
  - Source: [UPixelator docs](https://github.com/Radivarig/UPixelator_Documentation)
- An open **Unity 6 URP isometric pipeline** renders at **640×360** and snaps the ortho camera to the texel grid. The snap error becomes a UV pan offset in a "sharp upscale" shader (fwidth/smoothstep). It also has:
  - Toon lighting quantized into bands (`_Cuts`) with optional 4×4 Bayer dithering.
  - Depth- and crease-based 1-px outlines at internal resolution.
  - ~35k GPU-instanced grass billboards.
  - Known artifacts: crease lines at object bases, bands collapsing under cloud shadows.
  - Source: [GitHub](https://github.com/bababuyyy/unity-isometric-pixel-pipeline)
- **Godot implementations:**
  - Robert-K's MIT project: pixel-perfect camera with "smooth planar subpixel movement", **stepped 3D animation playback that emulates spritesheets**, toon shader, volumetric godrays. — [GitHub](https://github.com/Robert-K/godot-3d-pixel-art)
  - Leo Peltola's MIT outline/highlight shader for Godot 4 (post-process or material). — [Godot Shaders](https://godotshaders.com/shader/3d-pixel-art-outline-highlight-post-processing-shader/)
  - A paid "t3ssel8r-style" Godot 4 kit. — [itch.io](https://brunich.itch.io/3d-pixel-art-shadergame-kit-t3ssel8r-style-renderer)
  - A Godot user building a t3ssel8r-style 3D game still had pixel swimming and camera jitter after adapting snapping code. — [Godot Forums](https://godotforums.org/d/36180-subpixel-snapping-in-a-3d-pixel-art-game)
- **Research:** Pixelat3D (2026, SBGames) adds priority maps for texture LOD by distance/angle plus post-process outlining, and was preferred by users in most scenarios. — [SBC paper](https://sol.sbc.org.br/index.php/sbgames/article/download/45414/45175/). An earlier UFPE thesis (2023) built a real-time top-down 3D→pixel pipeline. — [UFPE](https://attena.ufpe.br/handle/123456789/52665)
- **Horde cost:**
  - Godot docs: bones are animated on the CPU, so thousands of skinned objects become impractical. The documented route for thousands of animated instances is vertex animation in a shader + `MultiMeshInstance3D`. — [Godot docs](https://docs.godotengine.org/en/4.2/tutorials/performance/vertex_animation/animating_thousands_of_fish.html)
  - Godot 4 does process skeletons/blend shapes via compute and instances identical meshes. — [Godot 4.0 optimization report](https://godotengine.org/article/godot-40-optimization-progress-report/)
  - In Unity, vertex animation textures (VAT) restore batching/GPU instancing versus SkinnedMeshRenderers. — [Unity Discussions](https://discussions.unity.com/t/how-do-you-speed-up-skinned-mesh/804326)

#### (d) Hybrids
- Hades: 3D-baked character sprites plus hand-painted 2D environments. — [Supergiant developer](https://steamcommunity.com/app/1145360/discussions/0/1738883810796606862)
- Dead Cells: 3D-baked characters with per-frame normal maps, a dynamic lighting system, and blend-mode VFX layered on the frame sequences. — [Game Developer](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)

#### (e) AI-assisted
- A Sept-2026 comparison concludes that most image generators do not produce real pixel art "until you downscale, quantise and clean up by hand".
  - Dedicated tools (PixelLab, Retro Diffusion, Sprixen, Sprite-AI) output on a true grid and need less cleanup.
  - **Set-wide consistency and frame-to-frame coherence** are the hardest problems.
  - Source: [Cinevva guide](https://app.cinevva.com/guides/ai-pixel-art-generators)
  - Details are in §6–§7.

#### Reference game: Death Must Die (DMD)
- As fetched 2026-10-25, the Steam page shows:
  - Still **Early Access** (since 14 Nov 2023); Realm Archive is developer and publisher; the developers plan five Acts before full release.
  - The EA description lists 6 playable characters with talent trees, 8 Gods, 2 areas, 6 bosses, "dozens of map locations" and random items.
  - Tags: Pixel Graphics, Isometric, Dark Fantasy. Reviews: Very Positive (91% of 14,051 English reviews).
  - Source: [Steam](https://store.steampowered.com/app/2334730/Death_Must_Die/)
  - The description may lag content: a later Act 3 "Darkmoor" update added a shapeshifting Druid hero and new enemies/bosses. — [PCGamesN](https://www.pcgamesn.com/death-must-die/act-3)
- **Team:** a March-2023 developer post describes three friends — a programmer, a non-character artist/creative lead, and one "animations-character-artist and systems balancer". — [Steam forum thread](https://steamcommunity.com/app/2334730/discussions/0/5264193038182744058/?ctp=6)

### Inferences

| Approach | Visual ceiling | Marginal cost of +1 direction / +1 frame | Retake cost | Main technical risk | Fit for this team |
|---|---|---|---|---|---|
| (a) Hand-drawn | Highest (Blasphemous/Owlboy-class detail) | Linear: every frame × every unique direction is drawn | High | Throughput; consistency across artists | Low as the *only* method for 8-direction top-down; good for key frames, environments, VFX, icons |
| (b) 3D→pixel pre-render | High for motion, medium for detail (≈50 px Dead Cells chars) | Near zero (render time) | Very low (move keys, re-render) | Flicker/"boiling", lifeless "3D look", palette shifts | **Best fit**: reuses the owner's Blender/Python + QA toolchain |
| (c) Real-time 3D-as-pixel | High for environments (t3ssel8r) | Zero | Very low | Ortho-only snapping, sub-pixel smoothing, object zig-zag, outline noise, skinning cost for hordes | Risky for a horde game; possible later for environments |
| (d) Hybrid | High | Mixed | Mixed | Style mismatch between rendered and drawn parts | **Recommended** (3D chars + hand-made env/VFX/UI) |
| (e) AI-assisted | Inconsistent; needs cleanup | Low $ | Low | Consistency, legal/reputation (§7) | Concept/placeholder/ideation only |

- Hand-drawing a top-down 8-direction roster is the cost trap: 5 unique directions (with mirroring) multiply every animation ×5. Using the cited 15–60 min/frame, one hero with ~10 animations × ~8 frames × 5 directions (≈400 frames) is ≈100–400 hours. 3D pre-rendering collapses the direction multiplier to near zero.
- DMD's art was made by a single character artist/animator in a 3-person team. Whatever their method, a DMD-scale roster is achievable by one strong character animator plus one environment artist. The DMD pipeline itself (hand-drawn vs pre-rendered) remains unknown (see Gaps).

### Gaps
- No primary source (interview/postmortem) on **how Death Must Die's sprites were made**: hand-drawn vs 3D-rendered, sprite sizes, directions, fps. Searches found none.
- Dead Cells' internal resolution, sprite-sheet frame rate and per-character production time are not given in the article. The article is from 2018 and may not reflect later Motion Twin/Evil Empire practice.
- t3ssel8r's own explanation (video) could not be opened. The technique is reconstructed from derivative projects.
- No published Godot benchmark for "N skinned 3D enemies at 60 fps" for hordes.
- Concrete examples of 16-direction sprite usage (e.g., Diablo II) were not verified (search budget exhausted).
- Hand-drawn time-per-frame figures are from low-authority sources.

---

## 2. Resolution and scaling: internal resolutions, integer scaling (1080p/1440p/4K/Steam Deck), camera zoom, sub-pixel smoothing, UI

### Takeaway
**640×360** is the most flexible 16:9 internal resolution: integer ×2/×3/×4/×6 to 720p/1080p/1440p/4K, and ×2 on Steam Deck with 40-px letterbox bars. 480×270 and 384×216 miss 1440p, and 320×180 is cramped for a horde game. Keep the world on a pixel grid but smooth the camera with a sub-pixel offset of the upscaled image. Render UI and text at native resolution: DMD dropped its pixel font for readability, and Steam Deck needs ≥9 px (ideally 12 px) text at 1280×800.

### Cited Findings
- **480×270 vs 1440p:** a Godot user found 480×270 doesn't scale to 1440p or HD, so the engine falls back to the nearest smaller integer and leaves a black frame. — [Godot forum](https://forum.godotengine.org/t/dealing-with-black-frame-when-internal-resolution-doesnt-scale-properly-to-display-resolution/128587)
- **320×180** fits every common 16:9 resolution, but another user found it doesn't show enough world for navigation. — [Godot forum](https://forum.godotengine.org/t/how-to-get-scaling-to-work-properly-in-fullscreen/72894/5)
- An open Unity pixel pipeline uses **640×360** internal resolution with a sharp upscale shader (not plain nearest/bilinear). — [GitHub](https://github.com/bababuyyy/unity-isometric-pixel-pipeline)
- **Steam Deck:**
  - The panel is 1280×800 (16:10). — [Android Authority](https://androidauthority.com/steam-deck-resolution-3339456)
  - Valve recommends supporting 1280×800 (preferred) or 1280×720. The smallest on-screen font character must be ≥**9 px** at 1280×800 (12 px recommended), readable at 30 cm, and user-configurable text size is recommended. — [Steamworks: Steam Deck compatibility](https://partner.steamgames.com/doc/steamdeck/compat)
  - A developer's Feb-2026 write-up says the text-size requirement is what most often trips titles, so set font sizes early. — [Blendo Games](https://blendogames.com/news/post/2026-02-19-steamdeckverification/)
- **DMD's pixel font:** the developers tried pixelating the font and scrapped it because it wasn't readable at small or even medium sizes (June 2023). — [Steam forum](https://steamcommunity.com/app/2334730/discussions/0/5264193038182744058/?ctp=6)
- **Sub-pixel smoothing:**
  - Snap camera and objects to a world-pixel grid, then shift the upscaled image by the snap error. — [UPixelator docs](https://github.com/Radivarig/UPixelator_Documentation)
  - Same idea as a UV `_PixelPanOffset` in the upscale shader. — [Unity pipeline](https://github.com/bababuyyy/unity-isometric-pixel-pipeline)
  - For 2D, a Godot asset (PixelPerfectSmoother) tracks a float camera position and snaps only at render time. It is Camera2D-only. — [Godot Asset Library](https://godotengine.org/asset-library/asset/4846)
  - Godot's Camera2D smoothing for pixel art is a long-standing issue with workarounds only. — [Godot forum](https://forum.godotengine.org/t/how-to-set-up-smooth-sub-pixel-camera-movement-for-pixel-art-games/38765)
  - Basic hygiene: nearest filtering, integer/viewport stretch modes, and whole-pixel camera positions to avoid shimmer. — [bugnet.io](https://bugnet.io/blog/how-to-fix-godot-pixel-art-blurry)
- **Character scale reference:** Dead Cells characters are ~50 px tall in-game. — [Game Developer](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)

### Inferences
- **Integer-scaling arithmetic** (my calculation):

| Internal | 1280×720 | 1920×1080 | 2560×1440 | 3840×2160 | Steam Deck 1280×800 |
|---|---|---|---|---|---|
| 320×180 | ×4 | ×6 | ×8 | ×12 | ×4 → 1280×720, 40 px bars top/bottom |
| 384×216 | ×3.33 (×3 + bars) | ×5 | ×6.67 (×6 + bars) | ×10 | ×3 → 1152×648 (64 px side / 76 px top bars) |
| 480×270 | ×2.67 (×2 + bars) | ×4 | ×5.33 (×5 + bars) | ×8 | ×2 → 960×540 (160/130 px bars — poor use of the Deck) |
| **640×360** | **×2** | **×3** | **×4** | **×6** | ×2 → 1280×720, 40 px bars top/bottom |
| 640×400 (16:10) | — | — | — | — | ×2 exact (also 1920×1200 ×3, 2560×1600 ×4) |

- **Suggested setup:**
  - Base 640×360 for 16:9. On 16:10 displays (Deck, many laptops), extend the visible world to 640×400 instead of letterboxing. This is an aspect "expand" policy — show more arena, which is gameplay-neutral in an arena survivor if enemy spawn radii are defined in world units.
  - At 640×360, a ~40–56 px hero is 11–16% of screen height, comparable to Dead Cells' 50 px. That leaves room for hordes.
- **Camera zoom:** keep zoom to integer steps of the pixel grid, or treat "zoom" as switching between pre-authored internal resolutions (e.g., 640×360 ↔ 480×270 for a boss intro). Non-integer zoom of a pixel layer breaks the grid. Inference; no source tested.
- **UI:** draw UI on a separate native-resolution canvas. Use a crisp bitmap/pixel-style font scaled by integer factors, or a clean vector font for body text. Pixel-styled frames can stay at 2×–3× scale. At ×2 on Deck, a pixel font must be ≥5 internal px tall to pass 9 px, and ≥6 px for the 12 px recommendation. That is tight, which supports DMD's choice.
- **Moving sprites:** snapping moving sprites to the grid produces visible "stair-step" motion at slow speeds (the zig-zag UPixelator mentions). Options are to accept it (authentic look) or to draw only the player on the upscaled layer at sub-pixel positions (breaks purity). Inference.

### Gaps
- Native resolutions of specific shipped references (Death Must Die, Dead Cells, Hyper Light Drifter, Enter the Gungeon, Celeste) were not verified (search budget exhausted).
- Whether Godot 4.x's built-in integer scale mode plus "expand" aspect handles the 16:10 extension cleanly was not verified in this session.

---

## 3. Animation and game feel: anticipation/active/recovery, smears, squash & stretch, secondary motion, hit feedback, frame rates, animation counts

### Takeaway
Use pose-to-pose animation with few, strong keys and hold frames. Sell speed with smears and VFX rather than more frames (Dead Cells' rule). Tie animation phases to gameplay windows. Impact comes from hit-stop (a few frames; ~0.1–0.2 s for heavy hits), flash, knockback, shake and permanence. In a horde game these must be scoped so the screen never freezes or shakes constantly. A survivors-ARPG needs **few animations per character but many directions**. A side-scroller needs **many animations but only 2 directions**.

### Cited Findings
- **Dead Cells:** set keyframes first with the fewest frames possible; add interpolation only before or after keyframes (ease in/out), never between; attacks pose-to-pose; movement and impact via VFX. The 3D workflow allowed timing changes "dozens of times in minutes". — [Game Developer](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)
- **Stepped animation:** Robert-K's Godot project plays 3D animation stepped to emulate spritesheets, i.e., held poses rather than smooth interpolation. — [GitHub](https://github.com/Robert-K/godot-3d-pixel-art)
- **Gameplay-window alignment** (owner's pipeline):
  - Dota attack clips were trimmed to the strike (≈0.12–0.7 of clip length) because in-game damage lands at the start of the animation window. Long wind-ups and returns were cut.
  - Sheets run at 12 fps.
  - Death is drawn once, facing camera.
  - Source: [docs/arcade-dota-sprites.md](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md)
- **Anticipation and smears:** anticipation pose (pull back before a punch) and sparing smear frames for very fast moves; for very fast movement only a couple of frames are needed, with smears covering choppiness. — [Sprite-AI guide](https://www.sprite-ai.art/guides/how-to-animate-pixel-art); [Clip Studio tips](https://tips.clip-studio.com/en-us/articles/6170)
- **Hit-stop durations:**
  - Fighting games use frame counts. Capcom's Street Fighter V column uses 8 frames of hit stop as its worked example. — [Capcom](https://game.capcom.com/cfn/sfv/column/131545)
  - One analysis says most fighting games freeze ~9 / 11 / 13 frames for light / medium / heavy hits. — [Sonic Hurricane "Impact Freeze"](https://sonichurricane.com/?p=1043)
  - A subjective study of 3D action games found the optimum hit-stop ≈ **0.1–0.2 s**, and slow-motion ≈ 0.2–0.4 s. — [IEICE 2023](https://ken.ieice.org/ken/paper/20230315OCSt/eng/)
  - A 2024 eye-gaze study tried to find the pleasant/unpleasant boundary for hit-stop duration. — [SciTePress 2024](https://www.scitepress.org/Papers/2024/124614/124614.pdf)
  - Practitioner guidance: hit-stop that's too long feels like a hang, too short is imperceptible. — [bugnet.io](https://bugnet.io/blog/how-to-fix-hitstop-or-freeze-frame-feeling-off); usually "a handful of frames", tunable per attack weight — [Oregon State dev blog](https://blogs.oregonstate.edu/petekevi/2023/01/24/blog-post-2/)
- **Vlambeer "The Art of Screenshake"** (Jan Willem Nijman, ~40 min talk) explains why one game feels great and a similar one terrible. — [Kenney's must-see list](https://kenney.nl/knowledge-base/learning/must-see-videos-for-indie-developers)
  - A step-by-step recreation lists effects including bigger bullets, **hit pause** and **permanence**. — [DK Liao devlog](https://dkliao.itch.io/the-art-of-screenshake-recreation/devlog/451576/quick-breakdown-of-all-the-effects)
- **Screen-shake accessibility (May 2025):** scale shake to the moment (heavy for crits, explosions, boss entrances; light for regular damage) and give players a slider plus an on/off toggle. — [dev.to](https://dev.to/selfish_dev/the-art-of-screen-shake-33bi)

### Inferences
- **Animation budget, survivors-ARPG (top-down, 8 directions, 5 unique with mirroring):**
  - Player hero: idle (4–6 f), run (6–8 f), 1–2 basic attacks (6–10 f incl. anticipation hold), dash/dodge (4–6 f), cast/ability (6–10 f), death (8–12 f, 1 direction). Hit reaction often replaced by a flash. ≈6–8 animations.
  - Fodder enemy: move (4–8 f), attack (4–8 f), death (6–10 f, 1 direction or dissolve). Hit = flash + knockback. Often 4 directions or a left/right flip.
  - Elite: + a telegraphed special.
  - Boss: 8–15 animations including telegraphs and phase transitions, larger canvas.
- **Animation budget, side-scroller (Dead Cells-like, 2 directions via flip):** many more states (idle, run, jump/apex/fall/land, roll, ledge/wall, hit, death) and weapon-specific attack strings. Per-animation cost is per single direction, so the count can be 3–5× higher.
- **Frame rate:** author at 24–30 fps in Blender, but export only keyed poses with per-frame hold durations in the sheet metadata, i.e. "on 2s/3s", ~8–15 unique poses/sec. This gives the crisp, poppy pixel-art read. The owner's 12 fps sheets are a reasonable default.
- **Squash & stretch and smears in 3D→pixel:** scale bones (non-uniform scale keys) for squash/stretch; separate "smear meshes" (stretched weapon-trail geometry visible for 1–2 frames) or hand-drawn smear overlays in Aseprite on contact frames. Secondary motion (capes, hair) can be baked from Blender cloth/spring bones because rendering is offline.
- **Horde-safe hit feedback:**
  - Global hit-stop only on player-centric heavy events: player hit, crit, elite/boss kill, parry. Ordinary enemy hits get **per-enemy local freeze** (enemy sprite pauses 2–4 frames while the world continues), plus a 1–2 frame white flash, small knockback and pooled particles.
  - Shake budget with a decay cap so overlapping hits don't sum into nausea, plus the slider/toggle.
  - Permanence (corpses, decals, gold) is cheap juice but must fade for performance in hordes.
  - Starting values: 3–6 frames @60 fps (50–100 ms) for regular heavy hits, up to 0.1–0.2 s for big moments, per the IEICE optimum.

### Gaps
- Vlambeer's specific numeric parameters (e.g., a search snippet claimed a Nuclear Throne pistol shot kicks the camera 6 px and "adds 4 to the screenshake", weapon kick 2) could not be traced to a verifiable source page. Treat as unverified.
- "Juice it or lose it" (Jonasson & Purho) was not retrieved in this session.
- No sourced value for white-flash duration. The 1–2 frames above is convention, not a cited figure.
- No authoritative count of animations per Dead Cells or DMD character.
- The Divoom article on 8/12/24 fps was seen only as a title.

---

## 4. VFX: flipbook vs shader/particles, palette consistency, glow/bloom, readability

### Takeaway
Use **hand-made pixel flipbooks** for signature, art-directed effects (slashes, explosions, ultimates) and **pixel-snapped particles/shaders** for numerous, variable effects (sparks, embers, trails, AoE fields). Keep both inside the game palette. Test readability in grayscale, and budget overdraw because survivors-likes stack hundreds of effects.

### Cited Findings
- Dead Cells layers blend modes on the exported frame sequences for effects, and conveys movement and impact through VFX rather than extra animation frames. — [Game Developer](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)
- Hades' effects (Zeus' thunder, Poseidon's waves, Artemis' arrows) were made by Josh Barnett, who was also the game's UI artist. Only the intro of the 80.lv piece was retrievable. — [80.lv (Oct 2020)](https://80.lv/articles/a-behind-the-scenes-look-at-the-effects-in-hades/)
- **Flipbooks vs particles:**
  - A VFX guide recommends pre-rendered flipbooks for art-directed effects as cheap at runtime (vendor opinion). — [skills.sh VFX page](https://www.skills.sh/drawcall-ai/skills/vfx)
  - A pixel-VFX artist finds sprites + particles more versatile than flipbooks. They baked a particle system into a 256×256 nearest-filtered flipbook for a pixel look, but saw unresolved jitter. — [RealtimeVFX forum](https://realtimevfx.com/t/flohs-sketchbook/24366)
- **Overdraw:** overlapping particle layers redraw the same pixel repeatedly, and heavier shaders cost more per pixel. — [Polycount](https://polycount.com/discussion/comment/2063838)
- **Readability:** check value contrast first ("if it reads in grayscale, it reads everywhere"), keep effects restrained, and give every effect quality scaling. — [Playbooks VFX skill](https://playbooks.com/skills/omer-metin/skills-for-antigravity/vfx-realtime)
- GodotCon Amsterdam 2026 scheduled a talk on procedural retro VFX with shaders; content not retrieved. — [GodotCon 2026](https://talks.godotengine.org/godotcon-ams-2026/talk/NSNXFT.ics)

### Inferences
- **Palette-consistent VFX:**
  - Define 4–6 dedicated VFX ramps inside the master palette (fire, frost, lightning, poison, holy, shadow), each 4–6 values from near-white core to saturated edge to dark tail.
  - Quantize any procedural/particle output to those ramps in a post shader, so particles don't introduce off-palette colours.
  - Snap particles to the pixel grid (render particles into the low-res buffer, not the upscaled one).
- **Glow/bloom:** compute bloom from an emissive mask at internal resolution (or ½ internal), then upscale with the scene. Bloom on the upscaled buffer smears across pixel edges and looks "HD". Keep the bloom radius small; overuse kills readability in dark scenes.
- **Readability hierarchy for a survivors-like:**
  - Enemy projectiles/telegraphs get the highest value contrast and unique hues, possibly a pulsing outline.
  - Player damage effects are lower in value and alpha, can be dimmed by a "VFX opacity" setting, and cap the number of simultaneous instances per effect type.
  - Danger zones use a consistent shape language (e.g., red ground decals with a filling progress ring).
- **3D-rendered VFX:** Blender can render some effects as flipbooks through the same pixel pipeline (shockwaves, rock debris, ghosts), keeping lighting and palette consistent with the characters. Small sparks and slashes are usually faster to hand-pixel in Aseprite.

### Gaps
- No primary breakdown of DMD's or Dead Cells' VFX pipeline (tools, frame counts, palette rules) was found.
- The Hades VFX article body was not retrievable.
- Pixel Composer (node-based pixel VFX tool) was not verified (search budget exhausted).

---

## 5. Lighting: 2D normal-mapped lighting and dark-atmosphere techniques

### Takeaway
3D→pixel gives normal maps almost for free: render a normal pass per frame, as Dead Cells did. That enables dynamic 2D lights in Godot (`CanvasTexture`) for a dark-fantasy look without hand-drawn shadows. Hand-drawn assets can get approximate normals from tools like Laigter. Watch for smoothing artifacts on pixel-art light edges and keep light bands quantized.

### Cited Findings
- Dead Cells exported a normal map alongside every frame and used a lighting system so shadows didn't have to be hand-drawn. — [Game Developer](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)
- **Godot `CanvasTexture`** pairs a diffuse texture with a normal map. Normals only have visible effect under `Light2D`s, and Godot expects X+/Y+/Z+ conventions, so other tools' output may need flipping. — [Godot docs](https://docs.godotengine.org/en/4.5/classes/class_canvastexture.html)
- GDQuest's tutorial covers 2D normal-mapped lighting, with a `_n` suffix naming convention for normal maps. — [GDQuest](https://www.gdquest.com/library/lighting_with_normal_maps)
- **Laigter** (free, open-source) derives normal, parallax, specular and occlusion maps from a diffuse sprite, with presets and a command-line mode for batch processing. — [LinuxLinks](https://www.linuxlinks.com/laigter-generate-lighting-maps-2d-game-artwork/)
- A web generator estimates normals from brightness and offers Godot's Y-up vs Unity's Y-down conventions. — [Wayline](https://www.wayline.io/normal-map-generator)
- **Known issue:** blurry light edges on pixel-art `CanvasTexture` sprites even with Nearest filtering; changing normal-map compression changed but did not fix it. — [Godot forum](https://forum.godotengine.org/t/low-bit-pixel-art-light-texture-on-canvastexture-sprite-has-unexpected-smoothing-blurring/129394)
- **Light quantization:** a 3D-pixel pipeline quantizes diffuse light into bands (`_Cuts`) with optional 4×4 Bayer dithering to soften band edges. — [GitHub](https://github.com/bababuyyy/unity-isometric-pixel-pipeline)
- The owner's pipeline needed auto-exposure for near-black textures and explicit glow/self-illumination masks (`--glow-mask`, `--glow-from-alpha`), because Workbench doesn't render emissive or specular effects. — [docs/arcade-dota-sprites.md](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md)

### Inferences
- **Render passes per frame:** albedo (flat/toon), **normal** (screen-space, matching the engine's convention), **emissive mask**, optionally a **height/depth** pass for pseudo-shadows. Bake a mild fixed key light into albedo (consistent screen-space light direction across all 8 directions), then let dynamic lights add rim and colour.
- **Dark atmosphere recipe** (DMD's "Dark Fantasy" mood):
  - Low ambient via `CanvasModulate`-style global tint.
  - Warm/cool point lights on torches and spells.
  - Emissive eyes/weapons on enemies to preserve readability in darkness.
  - Strong silhouettes with a dark or selective-colour outline.
  - Fog/vignette layers.
  - Value-structured palette (darks reserved for background, mid-tones for actors, highlights for interactables/danger).
  - Quantize light falloff (stepped radial gradients) so lighting stays "pixel".
- **Device testing:** test darkness on the Steam Deck's screen and in bright rooms. Dark games often need a gamma/brightness slider.

### Gaps
- No sourced documentation for Unity URP 2D lights with normal maps was retrieved in this session (only general knowledge).
- SpriteIlluminator was not covered by any retrieved source.
- No DMD-specific lighting breakdown was found.

---

## 6. Tools (2026): pixel editors, Blender pixel-render add-ons, animation tools (Mixamo, Cascadeur), AI 2D tools (PixelLab, Retro Diffusion, Scenario, Ludo.ai) and AI 3D generators (Meshy, Tripo, Hunyuan3D, Rodin, TRELLIS)

### Takeaway
- **Core stack:** Aseprite ($19.99, Lua-scriptable) + Blender with the owner's own Python pipeline. Cascadeur is optional for faster physically-plausible keyframing (Indie tier capped at <$100k revenue). Mixamo only for placeholder locomotion (free, maintenance mode, no raw redistribution).
- **AI 2D tools:** PixelLab is the most animation-oriented (skeleton posing, 4/8-direction rotation, MCP for coding agents). Retro Diffusion is the most "pixel-native" and can run locally inside Aseprite. Scenario and Ludo are general asset generators. None is consistently production-ready without cleanup.
- **AI 3D generators:** usable at most for blockouts or reference. Licensing varies sharply (Hunyuan3D excludes EU/UK/South Korea; TRELLIS.2's MIT code depends on research-only NVIDIA libraries; Meshy free tier is CC BY).

### Cited Findings

**Pixel editors**
- Aseprite: **$19.99** one-time in 2026 listings (older articles say $15 — outdated). Source-available (not open-source) and has a Lua scripting API. — [gg.deals](https://gg.deals/ca/application/aseprite/); [Gappsy](https://gappsy.com/tools/aseprite/); [PC Gamer (older)](https://www.pcgamer.com/best-pixel-art-maker-tools/)
- Free alternatives: Piskel (browser), LibreSprite (open-source); Pixelorama and PixiEditor are pitched for open-source flexibility. — [Guideflow](https://www.guideflow.com/blog/pixel-art-software)
- PixelLab's own editor is Pixelorama ("PixelLab's Editor (Pixelorama)"). — [pixellab.ai](https://pixellab.ai/)

**Blender pixel-render add-ons / shaders**
- See §1b: True Pixel Art Generator ($1; outlines, colour reduction, scale lock, sprite sheets) — [Superhive](https://superhivemarket.com/products/true-pixel-art-generator-v2/docs); Astropulse "Blender to Pixels" — [itch.io](https://astropulse.itch.io/blender-to-pixels); Kushizaki's free palette shader — [CG Channel](https://www.cgchannel.com/?p=67534); a paid course on Blender sprites/tiles/UI with shaders and Grease Pencil — [GameDev.tv](https://gamedev.tv/courses/blender-sprites)

**Mixamo**
- A guide updated July 2026 says Mixamo is still online and free with an Adobe account but in maintenance mode. The auto-rigger works, but don't expect updates. — [Cinevva](https://app.cinevva.com/guides/free-character-animations-rigging)
- Use in commercial games is royalty-free, but redistributing raw character/animation files as asset packages is not allowed (Adobe FAQ text reposted in 2022). — [Adobe Community FAQ](https://community.adobe.com/questions-696/mixamo-faq-licensing-royalties-ownership-eula-and-tos-589400)

**Cascadeur** (plans page, fetched 2026-10-25)
| Plan | Price | Commercial use | Notes |
|---|---|---|---|
| Free | — | None | CASC export only; 300 frames / 120 joints per scene |
| Indie | €19/mo or €8/mo annual (includes a perpetual license with annual) | Only if aggregate revenue/funding < $100k/yr | Adds FBX/DAE/USD/glTF export |
| Pro | €49/mo or €33/mo annual | Unlimited | Adds retargeting, environment interaction, quadruped autoposing |
- Source: [cascadeur.com/plans](https://cascadeur.com/plans)
- Cascadeur 2025.1 (April 2025) added AI-based inbetweening. — [CG Channel](https://www.cgchannel.com/2025/04/nekki-releases-cascadeur-2025-1-with-ai-based-inbetweening/)

**AI 2D tools**

*PixelLab* (homepage fetched 2026-10-25)
- Features: one-click, skeleton-based and text-prompt animation; 4- and 8-directional rotation including isometric; reference-image style consistency; "true inpainting"; tilesets (isometric/hex/top-down); UI elements; API and an **MCP server ("Vibe coding")**.
- Does not store generated or input images unless stated. Pricing didn't render on the page. — [pixellab.ai](https://pixellab.ai/)
- Third-party pricing: Apprentice **$12/mo** (2,000 generations), Artisan **$24/mo** (priority, up to 400×400), Architect **$50/mo** (10,000 generations, 20 concurrent). ~25% off annually; 40 free trial generations. — [G2](https://ai.g2.com/marketplace/tools/pixellab); [Cinevva (Sept 2026)](https://app.cinevva.com/guides/ai-pixel-art-generators)

*Retro Diffusion*
- Aseprite extension, one-time **$65** (full) / **$20** (Lite), runs locally (needs a decent GPU).
- Web credits from $5 (250) to $50 (3,500).
- Per-generation API pricing ≈ **$0.015 (RD Fast) – $0.18 (RD Pro)** per image; **animations $0.07–$0.25**; tilesets $0.10.
- Sizes 16×16 up to 384×384 depending on style; a hosted MCP server exists. — [Cinevva](https://app.cinevva.com/guides/ai-pixel-art-generators); [MCP listing](https://mcpservers.org/servers/retro-diffusion/retro-diffusion-mcp)
- Commercial-use terms for outputs were not found. A user asked on itch.io; no reply was visible. — [itch.io comments](https://astropulse.itch.io/retrodiffusion/comments)

*Scenario* (official KB)
- Free 50 CU/day, "**personal and evaluation use only**".
- Starter from $15/mo (1,500 CU); **Pro from $45/mo (5,000 CU, custom model training)**; Max from $75/mo (10,000 CU); Enterprise from $125/user/mo. Annual saves 33%.
- No statement on paid-plan output ownership on that page. — [Scenario KB](https://help.scenario.com/en/articles/pricing-plans/)
- Not a pixel-art-specific tool; no animation/tilesets listed. — [Cinevva](https://app.cinevva.com/guides/ai-pixel-art-generators)

*Ludo.ai* (vendor comparison pages, mid-2026)
- Entry ~$15–20/mo, Pro ~$35–50/mo (MCP from $50/mo).
- Sprite animation of up to 4 s at 4–64 frames per sheet, frames up to 384 px, motion presets (walk/run/idle/attack). Exports PNG sheet + JSON atlas, GIF, MP4.
- Vendor claims generated assets are "yours to use in commercial games". — [Ludo](https://ludo.ai/compare/best-ai-sprite-generators); [Ludo vs Scenario](https://ludo.ai/compare/ludo-vs-scenario)
- A third party notes Ludo output usually needs a downscale pass to reach a true grid. — [Cinevva](https://app.cinevva.com/guides/ai-pixel-art-generators)

*Overall quality verdict (third party)*
- AI output is not game-ready pixel art without cleanup. Consistency across a set and frame-to-frame coherence are the hardest problems.
- Leonardo-type general tools are best treated as concept art. — [Cinevva](https://app.cinevva.com/guides/ai-pixel-art-generators)

**AI 3D generators**
- **Hunyuan3D 2.1** — "Tencent Hunyuan 3D 2.1 Community License", June 13 2025:
  - "**THIS LICENSE AGREEMENT DOES NOT APPLY IN THE EUROPEAN UNION, UNITED KINGDOM AND SOUTH KOREA**"; use outside the Territory is unlicensed.
  - Above **1M MAU** a separate license from Tencent is required.
  - "Tencent claims no rights in Outputs You generate." — [LICENSE](https://raw.githubusercontent.com/Tencent-Hunyuan/Hunyuan3D-2.1/main/LICENSE)
- **Meshy** (secondary sources; conflicting):
  - Free-plan outputs under **CC BY 4.0** (attribution); some say commercial use is still allowed with credit, others report a "commercial use disabled" restriction on Free.
  - Paid plans give private ownership unless published to the Meshy community.
  - Free downloads limited (10/month). — [3D AI Studio](https://www.3daistudio.com/blog/best-free-ai-3d-model-generators-2026); [Sorceress](https://sorceress.games/blog/best-free-ai-3d-model-generator-honest-2026-picks); [Kompozy](https://kompozy.io/guides/ai-generated-3d-models)
  - The official Meshy help article returned 404 at fetch time.
- **Tripo:** commercial use requires a paid plan; free tiers publish to a public gallery (CC BY attribution). Aggregator price range $0–$109.9/mo. — [Cinevva 3D timeline](https://app.cinevva.com/guides/ai-3d-model-generation-timeline-2026); [CostBench](https://costbench.com/compare/rodin-hyper3d-vs-tripo-ai/)
- **Rodin (Hyper3D):** Creator ~$30/mo, Business ~$120/mo; commercial use requires a paid plan (aggregator data). — [Cinevva (KO)](https://app.cinevva.com/ko/guides/ai-3d-model-generators); [CostBench](https://costbench.com/compare/rodin-hyper3d-vs-tripo-ai/)
- **TRELLIS.2 (Microsoft, Dec 2025)** is reported as MIT-licensed. A Japanese report flags that its rendering/material code depends on NVIDIA nvdiffrast/nvdiffrec (NVIDIA Source Code License, research/evaluation only), and it needs ~24 GB VRAM. — [CGWorld](https://cgworld.jp/flashnews/01-202601-TRELLIS2.html); [3D AI Studio](https://www.3daistudio.com/blog/is-trellis-2-free-pricing-how-to-get-started); [80aj](https://www.80aj.com/2025/12/19/microsoft-unveils-trellis-2-a-4b-parameter-3d-generation-model-en/)

### Inferences
- **Aseprite is the hub** for hand cleanup, key-frame polish, VFX flipbooks, tiles and icons. Its Lua API lets AI coding agents automate import/export, palette remaps and sheet slicing alongside the Blender Python scripts.
- **Animation authoring:**
  - Blender keyframing with the owner's scripts covers the pipeline.
  - Cascadeur Indie (€8–19/mo) can speed physically believable attacks, jumps and deaths, but the <$100k revenue cap means planning a Pro upgrade at launch.
  - Mixamo clips are mocap-like and generic; they can block in locomotion for prototypes, but snappy pixel-action timing needs hand keys.
- **AI 3D at 48–64 px sprite height:** mesh topology problems matter less than silhouette, proportions and texture read. Generated textures often contain baked lighting and noisy detail that fight a flat/toon shader and cause pixel flicker. AI meshes still need retopology/rigging for animation. Net value: fast blockouts and kitbash parts for prototyping, not final heroes or bosses.
- **AI 2D tools' MCP servers** (PixelLab, Retro Diffusion, Ludo) mean the owner's coding agents could auto-generate placeholder sprites during prototyping. This is useful for gameplay iteration, but it creates the "forgotten placeholder" risk described in §7.

### Gaps
- Not verified (search budget exhausted): Pyxel Edit's current status/price, PixelOver (3D/2D→pixel converter), Pixel Composer (pixel VFX).
- No independent hands-on evaluation of PixelLab or Retro Diffusion animation quality (8-direction consistency, frame coherence) was found. Only vendor claims and one comparison guide.
- Official commercial-use/ownership terms for PixelLab, Retro Diffusion, Scenario paid plans, Meshy, Tripo and Rodin were not retrieved from primary pages. All licensing statements above except Hunyuan3D are secondary and should be checked before use.

---

## 7. Policy and sentiment: Steam AI disclosure rules, backlash cases 2024–2026, copyright of AI outputs, safe usage

### Takeaway
- Steam allows AI-generated content but requires disclosure of AI content **that players consume**: pre-generated assets including marketing/concept, and live-generated content with guardrails. Since January 2026, behind-the-scenes "efficiency" tools need not be disclosed.
- Player sentiment is the bigger risk. Two of the highest-profile 2025–2026 cases were **forgotten placeholders** (Clair Obscur, Crimson Desert).
- In the US, purely AI-generated imagery is **not copyrightable** (Copyright Office Jan 2025; Supreme Court declined Thaler in March 2026), so AI-made final art weakens IP protection for an original-IP game.
- Safe use: ideation, mood boards, internal placeholders under strict provenance tracking. Ship human-made (or substantially human-modified) final art.

### Cited Findings

**Steam rules**
- **Steamworks Content Survey:**
  - "Pre-Generated" = content created with AI tools during development, reviewed like any other content.
  - "Live-Generated" = created while the game runs; it must also describe its **guardrails** against illegal content.
  - Developers promise no illegal or infringing content, and that the game matches its marketing.
  - Valve won't ship live-generated AI adult-only sexual content "at this time".
  - The page says efficiency gains from these tools are "not the focus". — [Steamworks docs](https://partner.steamgames.com/doc/gettingstarted/contentsurvey)
- **January 2026 revision** (first since 2024):
  - Developers no longer have to disclose AI development tools such as code helpers.
  - Disclosure is still expected when AI generates content players see or use, including marketing and concept assets.
  - Developers list which assets are AI-generated and tick a checkbox for whether players interact with AI-generated content during gameplay.
  - Reports describe the policy as voluntary and not enforced by moderation; another source says it is enforced. — [Gigazine (Jan 20 2026)](https://gigazine.net/gsc_news/en/20260120-steam-updates-ai-disclosure-guidelines); [VGC](https://videogameschronicle.com/news/valve-has-significantly-rewritten-steams-rules-for-how-developers-much-disclose-ai-use); [Outlook Respawn](https://respawn.outlookindia.com/amp/story/gaming/gaming-news/valve-clarifies-steam-ai-policy-focus-shifts-to-content-consumed); [Tech Insider (claims enforcement)](https://tech-insider.org/steam-ai-disclosure-2026)
- **Disclosure prevalence:**
  - As of July 13 2025, **7,818 of ~114,126** Steam games (~7%) disclosed generative AI, up from 1.1% in April 2024. Roughly **20% of 2025 releases** disclose it, and ~60% of disclosures concern visual asset generation. Disclosure is voluntary, so these are minimums. — [VGC](https://www.videogameschronicle.com/news/steam-games-disclosing-generative-ai-use-are-up-800-this-year/); [WN Hub](https://wnhub.io/news/stores-and-publishing/item-48292); [Insider Gaming](https://insider-gaming.com/nearly-20-of-new-steam-games-in-2025-use-generative-ai/)
  - A 2026 source claims ~1 in 5 listed games carry the label and 40% of one week's launches disclosed. Lower reliability, different methods. — [Tech Insider](https://tech-insider.org/steam-ai-disclosure-2026)

**Backlash cases**
- **Clair Obscur: Expedition 33**, Dec 2025: the Indie Game Awards **revoked** its Game of the Year and Debut Game honours. The studio had attested no generative AI was used, but AI-generated placeholder background textures (newspaper-clipping-like) shipped and were patched out within days. — [AV Club](https://www.avclub.com/clair-obscur-genai-iga-awards-rescinded); [Malay Mail](https://www.malaymail.com/news/tech-gadgets/2025/12/21/indie-game-awards-rescinds-honours-from-clair-obscur-expedition-33-for-genai-use/202718)
- **Larian**, Dec 2025: CEO Swen Vincke said gen-AI was used "to explore things" (placeholder text, presentations, early concept experiments) with nothing AI-generated in the new Divinity. This still drew backlash, including from former employees; Warhorse's Daniel Vávra called the criticism "AI hysteria". Commentators contrasted it with fan anger at The Alters over discovered AI use. — [Techdirt](https://www.techdirt.com/2025/12/22/larian-studios-the-latest-to-face-backlash-over-use-of-ai-to-make-games); [Nintendo Wire](https://nintendowire.com/news/2025/12/17/larian-studios-ceo-responds-to-backlash-about-ai-comments/); [Outlook Respawn](https://respawn.outlookindia.com/gaming/gaming-news/warhorse-co-founder-daniel-vávra-defends-larian-amid-ai-backlash)
- **Crimson Desert (Pearl Abyss)**, March 2026: AI-generated 2D paintings from "experimental AI generative tools" were "forgotten" placeholders. The studio apologized, audited all assets, added a Steam AI disclosure, and replaced them with hand-drawn paintings in patch 1.01. The game still sold 2M+ copies on launch day. — [Game Informer](https://gameinformer.com/2026/03/23/crimson-desert-apologizes-for-using-ai-art-and-issues-first-big-patch); [Push Square](https://www.pushsquare.com/news/2026/03/crimson-desert-dev-keeps-its-promise-replaces-ai-art-in-latest-patch); [Engadget](https://engadget.com/gaming/crimson-desert-developer-apologizes-and-promises-to-replace-ai-generated-art-183716439.html)
- **Vapor World**, 2026: replaced in-engine cutscenes with AI-generated ones, then after backlash restored the originals and pledged to remove all AI cutscenes. — [Everything Edinburgh](https://everythingedinburgh.com/games/news/vapor-world-steam-ai-cutscene-backlash/)
- **Hardest** (F2P roguelike card game, released 1 Jul 2025): the developer announced it would be delisted on 30 January (2026 implied by context) instead of replacing its AI card illustrations, calling its current form "a disgrace". This was a voluntary developer decision, not a Valve action. — [VGC](https://www.videogameschronicle.com/news/an-indie-developer-is-delisting-his-steam-game-because-his-new-girlfriend-convinced-him-its-use-of-ai-is-a-disgrace)

**US copyright**
- Copyright Office **Part 2 report (Jan 29 2025)**: prompts alone don't make the user an author. Protection is available where human authorship is perceptible in the output, or for creative selection, arrangement or modification. Case-by-case; no new sui generis right. — [Copyright Alliance summary](https://copyrightalliance.org/ai-report-part-2-copyrightability/); [USCO bulletin](https://content.govdelivery.com/accounts/USLOCCOPYRIGHT/bulletins/3ceae5f)
- **Thaler v. Perlmutter:** the D.C. Circuit (Mar 18 2025) held the Copyright Act requires a human author. The **Supreme Court denied certiorari on March 2, 2026**, leaving that ruling in place. — [Baker Donelson](https://www.bakerdonelson.com/supreme-court-denies-certiorari-in-thaler-v-perlmutter-ai-cannot-be-an-author-under-the-copyright-act); [Jones Day](https://www.jonesday.com/en/insights/2026/03/jones-day-talks-women-in-ip--supreme-court-denies-review-of-thaler-v-perlmutter-ai-authorship-questions-remain); [Reed Smith](https://www.reedsmith.com/our-insights/blogs/viewpoints/102mlpl/supreme-court-denies-certiorari-in-thaler-v-perlmutter-human-only-rule-for-ai/)

### Inferences
- **Safe-usage policy for this project:**
  1. AI image/3D tools allowed for ideation, mood boards, and throwaway placeholders in prototypes.
  2. Every AI-touched file is tagged in a provenance manifest (path, tool, date), e.g. a JSON registry checked by CI.
  3. A **release gate fails the build** if any tagged asset is still in the shipping build. This directly prevents the Clair Obscur / Crimson Desert failure mode.
  4. If any AI-derived content does ship (e.g., an AI-assisted inbetween that was then repainted), disclose it on Steam honestly and keep human-modification evidence (layers, versions) for copyrightability.
  5. Don't use AI for the game's signature visuals (heroes, bosses, key art, capsule). That's where backlash and IP risk concentrate.
- **Code is not the risk:** AI coding agents for code/tools don't require Steam disclosure after Jan 2026, so the owner's agent-driven engineering workflow is not a disclosure problem.

### Gaps
- Non-US copyright regimes (EU, UK, Russia/CIS) were not researched.
- The exact current Steam form wording after the Jan 2026 rewrite was only seen via secondary sources; the Steamworks page fetched did not show it.
- Older (2024) backlash cases were not collected.

---

## 8. Costs and sourcing: artist rates, outsourcing studios, asset packs, rough budget for a Death Must Die-scale game

### Takeaway
Freelance pixel animation runs roughly **$3–20 per frame** for small sprites, or **$20–40/h** on marketplaces with seniors at **$45–60/h**. Studios quote **$250–3,000 per 2D character** and **$100–1,200 per animation**, with **$15–60/h** regional rates. Fully outsourced hand-drawn 8-direction art for a DMD-scale game plausibly lands in the **low-to-mid six figures**. A 3D→pixel pipeline run by the owner converts most of that into internal time, with outsourcing limited to concept, key art, environment tiles and VFX polish.

### Cited Findings
- **RocketBrush studio price list** ("updated May 2025"):
  - 2D character $250–1,000 (simple) / $500–2,000 / $750–3,000 (premium).
  - 2D character animation $100–300 / $200–600 / $300–1,200.
  - 2D environment $500–2,000 / $1,000–4,000 / $1,500–6,000.
  - 2D prop $200–800 / $400–1,600 / $600–2,400.
  - UI per page $100–400 / $200–800 / $300–1,200.
  - Hourly: US studios $40–60, Eastern Europe $25–35, SE Asia $15–35.
  - Source: [RocketBrush](https://rocketbrush.com/blog/game-art-outsourcing-prices-complete-pricing-breakdown)
- Freelancers or small studios (often Eastern Europe/SE Asia) charge **$50–300 per asset**; premier Western studios **$500–2,000** per complex piece. — [Pixune](https://pixune.com/blog/game-art-outsourcing-price/)
- **Pixel artist hourly by experience** (methodology unclear): beginner (0–2 yrs) $18–25, senior (5+) $45–60. — [Pixune career guide](https://pixune.com/blog/pixel-artist/)
- **Upwork:** animators $20–38/h; illustrators $15/$23/$30 (beginner/intermediate/expert). The general 2026 guide puts intermediate professionals at $25–75/h. — [Upwork animators](https://www.upwork.com/hire/animators/cost/); [Upwork illustrators](https://www.upwork.com/hire/illustrators/cost/); [Upwork rates guide](https://www.upwork.com/guides/hourly-rates)
- **Individual listings** (single data points):
  - $10/frame unshaded and $20/frame shaded (2023 form). — [Jotform commission form](https://form.jotform.com/223061084472046)
  - $3–8 per frame, base character $15 (≤32 px) / $25 (48 px). — [itch.io profile](https://itch.io/profile/miagameart)
  - $20–70+ per character with idle/walk/attack, plus $10+ per extra animation. — [itch.io for-hire post](https://itch.io/t/4455928/for-hire-pixelart-and-animator-looking-for-paid-projectworkcommission)
  - Fiverr seller from $10 per small character, animation from $15. — [Fiverr](https://fiverr.com/rifoone/create-pixel-art-and-animation)
- An Aug-2026 Aseprite community post on pricing sanity checks gives no numbers. It notes a small animated 64×64 icon can cost more time than a larger static sprite. — [Aseprite community](https://community.aseprite.org/t/a-simple-pricing-sanity-check-for-pixel-art-commissions/28697)
- **DMD context:** made by a three-person team, still in Early Access, priced around €6.89 at time of fetch (regional). — [Steam](https://store.steampowered.com/app/2334730/Death_Must_Die/); [Steam forum](https://steamcommunity.com/app/2334730/discussions/0/5264193038182744058/?ctp=6)

### Inferences
**Parametric budget model.** These are my assumptions, not sourced counts.

Scope assumed for "DMD-scale": 8 heroes, ~40 regular + ~10 elite enemy types, 8 bosses, 4–5 biomes, ~150 skill/boon VFX, ~300 item/skill icons, ~20 UI screens, 8 god portraits.

| Bucket | Volume assumption | Hand-drawn outsourced (freelance per-frame rates) | Notes |
|---|---|---|---|
| Heroes | 8 × ~8 anims × ~8 f × 5 unique dirs ≈ 2,560 frames | $25k–50k @ $10–20/frame | Studio tiers could be 2–3× |
| Enemies | 50 × 3 anims × 6 f × 5 dirs ≈ 4,500 frames | $22k–68k @ $5–15/frame | 4-dir/flip enemies cut this ~40–60% |
| Bosses | 8 × 10 anims × 10 f × 5 dirs ≈ 4,000 large frames | $80k–160k @ $20–40/frame | Biggest line item; 3D pre-render helps most here |
| Environments | 5 biomes × (tileset + props + backdrops) | $15k–60k | RocketBrush env $1.5k–6k per piece × 2–3 per biome |
| VFX | 150 × ~8 f | $6k–18k | Plus procedural effects |
| UI + icons + portraits | 20 screens, 300 icons, 8 portraits | $8k–30k | RocketBrush UI $100–1,200/page |
| **Total** | | **≈ $160k–390k** | Fully outsourced, hand-drawn, 8-direction |

- With **3D→pixel in-house**, the direction multiplier and most per-frame costs disappear. What remains:
  - **Owner time:** model, rig, animate. Dead Cells reports fast retakes but gives no hours.
  - **Outsourced design work:** character concept/turnaround sheets (≈$250–1,000 each at studio rates).
  - **Environments, VFX polish, UI and icons:** ≈$30k–100k if outsourced.
  - Optional 3D modeling help (rates not researched).
- **Asset packs** (itch.io/Unity store): useful for prototyping or generic UI/SFX. For an original-IP commercial game, they risk an "asset-flip" look and license limits on redistribution (cf. Mixamo's no-raw-redistribution clause in §6). Read each pack's license.

### Gaps
- No authoritative 2025–2026 survey of pixel-art rates; only studio price lists, marketplace aggregates and individual listings.
- DMD's real roster counts (enemies, items, VFX) and its art spend are unknown.
- itch.io / Unity Asset Store license terms were not retrieved (search budget exhausted).
- 3D character modeling/rigging outsourcing rates were not researched.

---

## 9. Audio (brief): music/SFX sourcing and costs, FMOD/Wwise support in Godot/Unity, adaptive music

### Takeaway
Indie composers who charge per minute quote around **$400/min** (median $407, average $618, GameSoundCon 2025). FMOD is the common middleware choice for Unity and indie titles. A Godot Wwise integration exists, built by an Audiokinetic field engineer as a community effort. Free indie tiers exist for both FMOD and Wwise, but current thresholds should be re-verified.

### Cited Findings
- **GameSoundCon 2025 survey** (May 1–Jun 15 2025, 654 respondents):
  - Indie composers charging per minute: average **$618** (+5% vs 2023), median **$407** (−13%), most common rate $400, top $2,100.
  - Midcore average $998 (median $1,000); AAA average $1,871 (median $1,700).
  - 55% of indie composers charge per minute.
  - FMOD and Wwise are both common in midcore/indie games. Unity projects most often use FMOD; Unreal leans Wwise. Godot is not mentioned.
  - Source: [GameSoundCon](https://www.gamesoundcon.com/post/gamesoundcon-game-audio-industry-survey-2025)
- **Wwise for Godot:**
  - "Wwise 2024.1 for Godot" brings Unity/Unreal-like workflows (WwiseProjectDatabase, auto-defined SoundBanks). It is authored by Audiokinetic field engineer Alessandro Famà and called a community effort. — [Audiokinetic blog](https://audiokinetic.com/fr/blog/whats-new-in-wwise-2024.1-for-godot); [Audiokinetic recap Jan 2025](https://audiokinetic.com/en/blog/recap-january-2025)
  - As of May 2025, web export is not supported; Windows, macOS, Linux, iOS and Android are. — [Audiokinetic Q&A](https://www.audiokinetic.com/qa/13928/web-export-with-godot)
- **Free indie licenses** (older sources, 2020–2022; re-verify):
  - FMOD free for small developers, reportedly under ~$200k annual revenue and ~$500k development budget. Earlier terms were a $100k budget. — [Game Developer](https://www.gamasutra.com/audio/small-developers-and-creators-can-now-use-fmod-studio-for-free); [itch.io devlog](https://imiself.itch.io/jkr/devlog/520271/from-daw-to-dusk-why-i-personally-used-fmod-for-my-game-audio)
  - Wwise free Indie license for total production budgets under $250k, with no asset limits since April 2022. — [Audiokinetic blog](https://www.audiokinetic.com/en/blog/free-wwise-indie-license); [Audiokinetic (older)](https://blog.audiokinetic.com/free-wwise-for-indie-developers/)

### Inferences
- For ~30–45 min of original music (typical for a roguelite with 4–5 biomes plus bosses and menus), $400–620/min implies roughly **$12k–28k**, before stems for adaptive layering. Adaptive stems (intensity layers per biome, boss phases) usually raise the price or the minute count.
- A native engine choice tilts middleware: Unity → FMOD (most common); Godot → engine audio, community FMOD/Wwise integrations (check maintenance), or Wwise 2024.1 for Godot.
- **Adaptive music for a survivor-like:** vertical layering driven by horde density, elite/boss presence and player HP; stingers on level-up/boon pick; boss-phase transitions on bar boundaries.

### Gaps
- Not verified (web search budget exhausted): current FMOD licensing (fmod.com rendered empty), FMOD-for-Godot plugin status, SFX library pricing (e.g., commercial SFX bundles), and concrete adaptive-music implementations in Hades / Dead Cells / DMD.

---

## 10. Recommended pipeline for this team, and the main risks

### Takeaway
Choose a **"3D-core hybrid"** pipeline:
- **Characters, enemies and bosses:** original 3D models, hand-keyed in Blender, rendered through an evolved version of the owner's Python sprite pipeline (toon/flat + 1-px outline + normal and emissive passes + fixed-palette quantization + QA metrics). Hand-polish only the key frames in Aseprite.
- **Environments, VFX signatures, UI and icons:** hand-made pixel art.
- **Lighting:** dynamic 2D normal-mapped lighting for the dark mood.
- **Internal resolution:** 640×360, expanding to 640×400 on 16:10; UI at native resolution.
- **Generative AI:** only for ideation and tracked placeholders that never ship.

This mirrors Dead Cells/Hades, maximizes reuse of the owner's existing tooling and QA, and keeps AI risk out of the shipped game.

### Cited Findings
- 3D-rendered character pipelines shipped in acclaimed action roguelites: Dead Cells (3D→pixel with normal maps, fast retakes, reuse of models) and Hades (3D-rendered characters + hand-painted environments). — [Game Developer](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-); [Supergiant developer](https://steamcommunity.com/app/1145360/discussions/0/1738883810796606862)
- Dead Cells' unresolved "flickering pixels" is the known weakness of the approach. — [Game Developer](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)
- The owner already solved many 3D→sprite problems and built automated QA (strobe, jerk, gaps, drift, darkness metrics; root-motion and loop-period fixes; layer compositing). — [docs/arcade-dota-sprites.md](/Users/Shared/Coding/aegis-draft/docs/arcade-dota-sprites.md)
- Real-time 3D-as-pixel needs ortho-grid snapping, sub-pixel offsets and per-object snapping, and still shows flicker/zig-zag artifacts. Large skinned hordes need vertex animation and instancing. — [UPixelator](https://github.com/Radivarig/UPixelator_Documentation); [Godot docs](https://docs.godotengine.org/en/4.2/tutorials/performance/vertex_animation/animating_thousands_of_fish.html)
- AI placeholders that slip into release caused the most visible 2025–2026 backlash, and pure AI output lacks US copyright protection. — [AV Club](https://www.avclub.com/clair-obscur-genai-iga-awards-rescinded); [Game Informer](https://gameinformer.com/2026/03/23/crimson-desert-apologizes-for-using-ai-art-and-issues-first-big-patch); [Copyright Alliance](https://copyrightalliance.org/ai-report-part-2-copyrightability/)
- DMD dropped a pixel font for readability, and Steam Deck requires ≥9 px text at 1280×800 (12 px recommended). — [Steam forum](https://steamcommunity.com/app/2334730/discussions/0/5264193038182744058/?ctp=6); [Steamworks](https://partner.steamgames.com/doc/steamdeck/compat)

### Inferences

**Recommended pipeline (concrete)**

1. **Style bible first (week 1–2)**
   - Master palette of ~48–64 colours, with material ramps plus 4–6 VFX ramps.
   - Internal resolution 640×360. Hero ≈ 40–56 px tall, fodder 20–40 px, elites 48–72 px, bosses 96–192 px.
   - Camera pitch: the owner found ~42° reads like DMD/RTS.
   - Fixed screen-space key light (top-left); 1-px outline policy (dark or selective colour); shape language for danger.
   - Commission a human concept artist for hero/enemy turnarounds and key art if the owner lacks illustration skills. This is where original-IP identity is made.

2. **Characters, enemies, bosses: 3D→pixel**
   - **Models:** low-to-mid-poly originals in Blender (Dead Cells shows heavy detail is wasted at ~50 px). Kitbash from shared base meshes and rigs (Dead Cells' "most useful trick"; Hades' base-model reuse). Flat-colour or low-res texel-matched textures; avoid noisy high-frequency textures that boil.
   - **Animation:** pose-to-pose hand keys, with squash/stretch via scalable bones, smear meshes for 1–2 frames on fast attacks, and cloth/hair secondary motion baked offline. Cascadeur Indie is optional.
   - **Export:** stepped timing (unique poses ~8–15/s, variable hold durations in metadata). Attack phases (anticipation/active/recovery) are exported as tagged frame ranges, and gameplay hit windows are read from those tags, never hard-coded.
   - **Render:** orthographic camera at fixed pitch with integer pixels-per-unit. Render directly at target size with AA off (or a deterministic majority-vote downsample from 2×–4×). Toon 2–3 bands. 1-px outline after downscale.
   - **Passes:** albedo, normal (engine convention), emissive mask, optionally depth.
   - **Palette:** map to the **fixed master palette**, not a per-sheet adaptive palette, to avoid colour drift between sheets/frames. No dithering on characters.
   - **Directions:** 8 for heroes (render 5 + mirror 3 where the design is symmetric, or design asymmetric weapons per side); 4 or flip-only for fodder; 8 for bosses with large silhouettes. Deaths in 1 direction (owner's lesson).
   - **QA:** port the existing metrics (strobe, jerk, silhouette gaps, drift, darkness). Add a **flicker metric** (pixel-change ratio between consecutive frames of a static-camera idle). Gate CI on thresholds.
   - **Polish:** an Aseprite pass only on "money frames" (attack contact frames, idle first frame, boss telegraphs) and on any frame failing the flicker metric. This is where hand-made charm is injected. Budget ~10–20% of frames.

3. **Environments**
   - Hand-pixeled tilesets and props in Aseprite, with normal maps via Laigter CLI for key props.
   - Large set pieces (gates, altars) may go through the same 3D pipeline for consistency.
   - Separate background/midground/foreground layers; fog and vignette for depth.

4. **VFX**
   - Hand-animated flipbooks (Aseprite) for 20–40 signature effects.
   - Shader/particle effects rendered into the low-res buffer, quantized to VFX ramps.
   - Emissive-mask bloom at internal resolution.
   - Readability rules (enemy danger effects dominate value contrast), quality toggles, and caps on simultaneous instances.

5. **Lighting and atmosphere**
   - Dynamic 2D lights with the per-frame normal maps from the 3D renders; quantized light falloff.
   - Emissive enemy eyes/weapons for dark readability; brightness/gamma slider; Steam Deck screen tests.

6. **Game feel defaults**
   - Per-enemy local freeze (2–4 frames) plus 1–2 frame flash plus knockback for regular hits.
   - Global hit-stop 50–200 ms only for heavy, player-centric events.
   - Shake with decay cap plus slider/toggle; permanence with fade-out.

7. **UI**
   - Native-resolution canvas; readable font sized for ≥12 px on Deck at 1280×800; pixel-styled frames at integer scale.

8. **AI policy**
   - Use for ideation, mood boards and gameplay-prototype placeholders (MCP-driven PixelLab/Retro Diffusion is convenient for agents).
   - Provenance registry plus CI release gate (no AI-tagged assets in release builds).
   - No AI 3D generators for final models. If used for blockouts, check licenses (Hunyuan3D's EU/UK/KR exclusion; TRELLIS.2's NVIDIA dependencies; Meshy free CC BY).
   - Honest Steam disclosure if anything AI-derived ships.

9. **Validation before scaling content (2–4 week art vertical slice)**
   - Scope: 1 hero (8 directions, 6 animations), 3 enemies, 1 boss, 1 biome tileset, 10 VFX.
   - Measure hours per animation through the 3D pipeline, the flicker metric, and readability at 300+ enemies on screen.
   - Compare one enemy against a commissioned hand-drawn version (~$100–600 at the rates above) to calibrate style and the hybrid seam.

**Main risks and mitigations**
- **"3D look" / lifeless motion:** mitigate with strong pose-to-pose keys, stepped timing, smears, hand-polished money frames and limited palettes.
- **Pixel flicker/boiling** (Dead Cells never solved it): fixed palette, flat/low-frequency textures, consistent screen-space lighting, outline after downscale, flicker QA gate, targeted hand cleanup.
- **Style seam** between rendered characters and hand-drawn environments/VFX: shared palette, outline rules, light direction, texel density; test early in the vertical slice.
- **Art-direction bottleneck:** an original IP needs design talent the pipeline can't supply. Budget for a concept artist; the pipeline then multiplies their designs.
- **Readability in hordes:** silhouette-first enemy design, value hierarchy, VFX caps; test at maximum on-screen counts.
- **Scope creep** (bosses are the costliest assets): reuse rigs and animations across enemy families; fewer, better bosses.
- **AI backlash/IP:** provenance gate and no AI in signature visuals (see §7).
- **License traps:** Cascadeur Indie revenue cap, Mixamo redistribution limits, AI-tool tier terms, Hunyuan3D territory exclusion.
- **Steam Deck/accessibility:** text size, brightness, shake/flash toggles.
- **Tool-building time sink:** the owner's pipeline is Dota/VRF-specific (S2V export, items_game parsing). Port only the generic core (render, framing, root-motion cancel, loop detection, palette, sheet packing, QA) and resist re-building the cosmetic-layer system unless the design needs equipment visuals.

### Gaps
- No primary data on hours per animation in a 3D→pixel pipeline, from Dead Cells or elsewhere. The vertical slice must measure it.
- DMD's actual pipeline, sprite sizes and direction counts are unknown, so "DMD-likeness" must be judged visually by the owner.
- Engine-specific capabilities (Godot vs Unity 2D lighting, integer scaling, sprite batching for hordes) were only partially sourced. Verify in the engine chosen for the "maximally native" stack.
