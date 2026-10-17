# Asset Audit — what we already have for the Claude Design list

Checked 2026-10-05 against `CLAUDE_DESIGN_NEEDS.md`, buckets 1–3 (28 items). Revised the same day after an independent check.

## Summary

- **12 of 28 items can be filled from what we already have.**
  - 5 fit as they are: 1.3, 1.4, 1.5, 1.7, 2.6.
  - 7 fit after a small change: 1.1, 1.2, 2.1, 2.3, 2.4, 2.5, 3.2.
- **10 items get partial help.** We have part of the answer, and the rest stays on the request: 1.6, 1.8, 2.7, 2.8, 3.1, 3.6, 3.7, 3.8, 3.10, 3.11.
- **6 items have nothing.** 2.2, 2.9, 3.3, 3.4, 3.5, 3.9.
- Totals: 5 + 7 + 10 + 6 = 28.

Most help comes from two places.

1. **The current package already holds many of the numbers.** It lacks only the drawn art. The sizes and colours are in its records (`layout.json`), its game logic (`board-logic.js`) and its state spec (`UIStateSpec.md`).
2. **Claude Design's older handoff** (`References/Tactical spaceship UI planning.zip`). Its README and "Asset Kit" page describe each plot mark: shape, size and colour. The marks are built in page code. The zip holds just one image, a steel texture (`figma/steel.png`, 924×540). Some of the old marks match the new picture: the friendly triangles, the BC-01 diamond and the ghost. Others do not. The own ship is a different shape. The new picture shows no hostiles at all. Where old and new differ, the new package wins.

Some of the drawing code already exists. `HandoffWedge.cs` draws pie-slice arcs. `PlotOverlay.cs` and `HandoffMarksGraphic.cs` draw lines, dashed lines and rings. There is a limit: our polygon drawer fills shapes only. It cannot draw a thin outline, and it cannot draw a shape with notches. So outlined ship marks need a new drawer, or sprites.

Contact sheet: `Temp/HandoffQA/asset_audit/contact_sheet.png`.
Previews: `Temp/HandoffQA/asset_audit/previews/`. File names below are in that folder.

- `proc_*.png`: drawn by script from the old handoff's numbers, using the new palette colours.
- `oldhandoff_*.png`: crops from the old handoff pages.
- `asset_*.png`: our own images on the board's dark background. Plain white shapes were tinted with palette colours.
- `mesh_*.png`: top-down outlines traced from our own 3D ship models.

**About colour settings.** The new board needs its images imported with one colour setting off ("sRGB"). Most of our older images have it on. Plain white or black shapes look the same either way. Anything with grey or coloured pixels would need a copy imported with the setting off.

Some contact sheet tiles are out of date. Use the tables below, not the sheet, for verdicts.

- **The glyph-set tile** shows the old filled own ship and the old purple circle.
- **"2.1/2.4 rings"**: the inner dashed ring is really item 2.9. Item 2.4 is a cone, not a ring.
- **"old kit 5.1 marker glyphs"**: 5.1 is the old kit's own section number, not needs item 5.1.
- **The 1.1 and 1.2 hull tiles** show old looks too: the filled wedge and the sword outline.

---

## Bucket 1 — Ship and contact sprites

| # | Candidate | What it is · size | Preview | Verdict | Origin |
|---|---|---|---|---|---|
| 1.1 | Current package records #41–#45 in `layout.json` | A 34×76 px rounded hull. A white fill sits under a black fill, so only a white edge shows. An inner line runs at 75%. A heading line about 150 px long sits at −28°. | crop of `layout/board-1920x1080.png` | **Fits with a tweak.** Build the ship from these records. They give the size, colour and heading line. The soft glow and the arrowhead still need hand work. | Claude Design, current package |
| 1.1 | `Assets/Art/Hulls/hull_wedge_outline.png` | White outline wedge, 512×1024. | `asset_hull_wedge_outline.png` | **Stop-gap only.** Use it until the hull above is built. Do not use the filled wedge or the old "sword" shape. | Ours (generated, Step 8) |
| 1.2 | Old handoff hostile marks + current `board-logic.js` | In the new logic, an unresolved contact has no mark, only a bearing line. A mark appears once the contact resolves. The contact board shows amber `#E0A020` while resolving and red `#E04B3C` once resolved. The plot mark's colours are still open. A red ring plus a "LOCK" label mean targeted. | `oldhandoff_live_track07_lock.png`, `oldhandoff_live_contact17.png` | **Fits with a tweak.** The states are settled. The shape is not. The old README calls its sword hulls placeholders. It asks for a simple glyph of 12–20 corners, readable at 30 px. Our polygon drawer cannot draw it: it fills only, and covers about 2.7× the true area on that shape. | Claude Design, older + current |
| 1.2 | Corner brackets (old handoff) | Amber corner brackets around a contact. | `oldhandoff_live_track07_lock.png` | **Fits with a tweak.** They mean "in range and in arc", not "targeted". Use the new amber `#E0A020`, not the old `#E8A33D`. Draw the eight short lines in code. | Claude Design, older handoff |
| 1.2 | `Assets/UI/Sprite/targetScan.png` | Four corner brackets, 1024×1024. | `asset_targetScan.png` | **Doesn't fit as well as code.** Low risk (team-made). But drawing the lines is easier. | Ours (phranq, Feb 2026) |
| 1.3 | Old handoff friendly marks | Four 15×17 px triangles. Each has a 25% blue `#4A9FD8` fill and a 1 px edge. A dashed movement line ends in an arrowhead. | `oldhandoff_live_ftr2.png` | **Fits as-is.** Matches the new picture. The thin edge needs a drawer that can outline, or a small sprite. | Claude Design, older handoff |
| 1.3 | `Assets/UI/Sprite/triangle-128.png`, `Assets/UI/Dialog/ui_nav_playerShip.png` | White solid triangles, 128 px and 1024 px. | `asset_triangle128.png`, `asset_ui_nav_playerShip.png` | **Doesn't fit.** Solid with no edge. `triangle-128.png` is also of unknown origin. | Unknown / ours |
| 1.4 | Old handoff BC-01 mark | An amber diamond outline with a 25% amber fill, about 17 px across. A fine dashed line leads to it. | `oldhandoff_live_bc01.png` | **Fits as-is.** Use the new amber `#E0A020`. | Claude Design, older handoff |
| 1.5 | Old handoff predicted-position ghost | Hull shape, 22×52 px. The new picture shows a 14% fill with no dashes visible. It fades in and out over 4 s. | `oldhandoff_live_predicted.png` | **Fits as-is.** Our polygon drawer has the same limit as in 1.2. A sprite is safer. | Claude Design, older handoff |
| 1.6 | Old handoff asteroid field | 7 grey ellipses at 22–30% opacity. | — | **Partial.** Gives the colour and count, not a rock shape. | Claude Design, older handoff |
| 1.6 | `Assets/3D Assets/Obstacles/Asteroids/*.prefab` | Six 3D asteroid prefabs. | — | **Partial.** Rock outlines could be traced from them. Not yet done. | Ours |
| 1.6 | `Assets/UI/icon/test_asteroid.png` | Orange filled rock, 1024×1024. | `asset_test_asteroid.png` | **Doesn't fit.** Orange test art. | Ours (test art) |
| 1.7 | Old handoff line styles | Four line types. Heading: solid with an arrow. Threat: dashed, in the contact's colour. Order line: fine dash, no head. Trail: fades from 75% to 16%. Arrow length shows speed. | `oldhandoff_kit_vectors_leaders.png` | **Fits as-is.** Swap in the new colours: orders amber `#E0A020` instead of `#E8A33D`, and own-force blue `#4A9FD8` instead of the old sensor cyan `#3FA9C9`. | Claude Design, older handoff |
| 1.8 | Old handoff Contact 22 | A 12 px purple circle labelled "POSSIBLE MISSILE". | `oldhandoff_live_contact22.png` | **Partial.** In the old package, purple meant "unclassified contact". There was no purple vector. Old material cannot settle whose vector it is. Stays open. | Claude Design, older handoff |
| — | `Assets/UI/icon/test_enemyShip.png`, `test_playerShip.png` | Orange filled ship drawings on black, 1024×1024. | `asset_test_enemyShip.png` | **Doesn't fit.** Orange test art. | Ours (test art) |

## Bucket 2 — Radar, sensors and firing arcs

| # | Candidate | What it is · size | Preview | Verdict | Origin |
|---|---|---|---|---|---|
| 2.1 | Current colours (`senEdge` 26%, `senGlow` 4%) + `PlotOverlay.cs` ring | The new ring is a solid 1 px line. `PlotOverlay` already draws a solid ring. | `proc_sensor_ring_and_scan_ring.png` (outer ring) | **Fits with a tweak.** The glow size is our guess, about 10 px. | Claude Design, current package |
| 2.2 | — | — | — | **No candidate.** This is a map-scale decision. | — |
| 2.3 | Prototype sweep code | `References/starwolf-prototype.html` lines 3012–3020 and 2584–2595. A trail that fades over 90°, with a 1 px leading edge. | `proc_radar_sweep_from_prototype.png` | **Fits with a tweak.** Keep the prototype's fade. Use the new sweep colours (`sweepTail`, `sweepInk`, `sweepEdge`) and spin speed. | Ours (the prototype) |
| 2.4 | Current package cone values + `HandoffWedge.cs` | It is a cone, not a ring. It is centred on `coneBrg` (62 at load), with half-width `coneArc` (40; 15–90 in steps of 10). It is sized like the ring, 300 + gain × 96 px, and coloured `coneInk` (green at 20% and 5%). `HandoffWedge` already draws it on the sensors dial. | — | **Fits with a tweak.** Reuse the same drawer on the plot. Only its place and look on the plot are missing. Do not use the old 300 px "ACTIVE SCAN" ring. | Claude Design, current package |
| 2.5 | `MOUNTS` table in `board-logic.js` + `HandoffWedge.cs` | Arc centre and width for all 5 mounts: RG-A 42°±80°, RG-B 250°±80°, PDC-A 42°±35°, PDC-B 200°±35°, missile all-round. Fill 15%, two edge lines at 45%. | `proc_firing_arc_wedge.png` | **Fits with a tweak; keep on the request.** No arc size per mount is given. The PDC arc is a fixed 340 px, not on the map scale, and railgun range would run off the plot. The all-round missile arc has no look. | Claude Design, older + current |
| 2.5 | When arcs show | Old rule: show the selected mount's arc. But the new board loads with RG-A selected and still shows PDC-A. The prototype's rule (`starwolf-prototype.html` near line 2749): the arc shows while weapons have power, brightens when locked, and has dashed edges. | — | **Partial.** The prototype gives a usable rule. The design's intent is still open. | Ours (prototype) / Claude Design |
| 2.5 | `Assets/UI/Shader/WeaponAiming.shadergraph` | Old weapon-arc shader from the earlier 3D board. | — | **Doesn't fit.** Built for the old colour setup. It would look wrong on the new board. | Ours (team, 2025) |
| 2.6 | `passives` in `board-logic.js` + our dashed-line code | Most of the template is in the package. Length: half the sensor ring (×0.55 when listening on UV). Colour: white at 60%, or amber at full when the periscope is on that bearing. Label: "BRG nnn". Clicking turns the periscope to it. | `proc_bearing_line.png` | **Fits as-is.** Only the dash size is missing. The picture's BRG 149 line measures about a 9 px dash and a 9 px gap; use that. `PlotOverlay`'s 9 and 6 is close. Our 3 and 2 is too fine. BRG 048 is a second copy. | Claude Design, current package |
| 2.7 | Current package `bd-blink` | 1.1 s cycle, dropping to 20%, on "Plot · Own ship". | — | **Partial.** Gives the rhythm. Which part of the ship blinks is still open. | Claude Design, current package |
| 2.8 | Old handoff `Star Wolf Board Live.dc.html` | The older plot page, with contact trails and arrowheads. | `oldhandoff_live_full.png` | **Partial lead.** It may show some of what was dropped. Compare it piece by piece before relying on it. | Claude Design, older handoff |
| 2.9 | Inner dashed ring (new picture) | A dashed ring about 300 px across, inside the sensor ring. No document names it, and it has no record. | `proc_sensor_ring_and_scan_ring.png` (inner ring) | **No candidate.** Easy to draw. But we don't know what it stands for. | Claude Design, current picture |
| — | `ScreenRadar.shadergraph`, `ui_compass.png`, `ui_turnRatio.png` | Old range-ring shader, a 2048 px tick ring, a dashed half-arc. | `asset_ui_compass.png`, `asset_ui_turnRatio.png` | **Doesn't fit** the plot. Wrong scale and style. | Ours (team, 2025) |

## Bucket 3 — Control art

| # | Candidate | What it is · size | Preview | Verdict | Origin |
|---|---|---|---|---|---|
| 3.1 | `UIStateSpec.md` (current package) | Gives the hardware and sizes. Warhead: 36×36 rotary selector, pointer at 60/90/120°. SALVO: 30×30 knurled knob, 14 px per step, values 1–8. LOADERS: 40×22 push button. Four quadrant push buttons (1 px press). The missile mount's single tick at −90°. | — | **Partial.** Every part can be built from sprites we already have. Only their positions on the board are missing. The old "PAYLOAD SLUG 1/2" box is the old railgun look; don't use it. | Claude Design, current package |
| 3.2 | Our ship models, traced top-down | 11 models in `Assets/3D Assets/Starships/` and 5 in `UpdatedStarships/`. Outlines traced at 512 px, any size. | `mesh_outline_StarWolf_Destroyer_PlayerShip_mesh.png` and others | **Fits with a tweak.** Pick the Starwolf hull, then save a clean outline image. No damage looks. | Ours (KodiakPlays, 2025–26) |
| 3.2 | `export/sprites/Starship_Silhouette.png` | Detailed Starwolf outline, 2172×724. Made by the owner; not a Claude Design file (see Origin). | — | **Usable.** Best use: the engineering outline, turned nose-up; a long hull suits compartments from nose to tail. Not for the own-ship mark at plot size. Before use, it needs importing as a board sprite with the colour setting off and the size limit raised. Do that only with the owner's yes. | Owner, made with ChatGPT image tools |
| 3.3 | — | — | — | **No candidate.** | — |
| 3.4 | — | The old handoff says only "raised face, top lip, cast shadow". | — | **No candidate.** | — |
| 3.5 | `UI_GradeDisplay*.shadergraph` | Old segmented-bar shader. | — | **Doesn't fit.** Old colour setup, different look. | Ours (team, 2025) |
| 3.6 | Old handoff hazard border | Amber and black striped frame around EXECUTE. | `oldhandoff_kit_execute_hazard.png` | **Partial.** A stripe pattern for the fire lever's track; recolour it red. No sizes for the other bars. | Claude Design, older handoff |
| 3.7 | Old handoff knob shading | Knob face: grey `#464C52` to `#131619`, lit from the upper left. | `oldhandoff_kit_knobs.png` | **Partial.** Real colours for knob faces only. None for the faders, heading ring or bearing tape. | Claude Design, older handoff |
| 3.8 | `Assets/Art/Frames/vignette.png` | A baked inner shadow, 480×270. | `asset_vignette.png` | **Partial.** The method works for the plot's inner shadow; bake it again at the plot's values. No values for the whole-board edge shadow. | Ours (Step 15) |
| 3.9 | — | — | — | **No candidate.** This is data. | — |
| 3.10 | `Assets/Sprites/Circle1.PNG` | Soft round fade, 256 px. Dark in the middle, so it needs flipping. | `asset_Circle1_glow.png` | **Partial.** A usable glow shape. The size and strength per lamp must still come from design. | Ours (team, 2025) |
| 3.11 | Old README bearing tape | Two repeating tick scales, 24 px small and 48 px large, scrolled by bearing. Amber centre mark. | — | **Partial.** Spacing settled. Tick height, labels and colour are not. | Claude Design, older handoff |
| — | `Assets/UI/Button/btn_*.png` | Glossy green toggles, dial and buttons, 1024 px. | `asset_btn_tuner.png`, `asset_btn_bool.png` | **Doesn't fit.** Cartoon-gloss style, nothing like the steel panel. | Ours (team, 2025) |

## Other buckets — one lead

- **5.1 (row layout).** The old README gives a rule for status rows. Cells never shrink or wrap. Values never break between number and unit. Units are the same size as their number. Columns share a top line. This is a partial answer, so 5.1 stays on the request.

## Worth asking for — what we have

| Extra | Candidate | Verdict |
|---|---|---|
| Ship class glyphs, periscope silhouettes | `Starship_Silhouette.png` (owner-made) covers the Starwolf for the hangar and other large views (the periscope only ever shows other ships). Our 3D models, traced, cover the other hulls. `Starships/`: TroopShip, Carrier, Corvette, Cruiser, Cruiser_2, Destroyer_2, Battleship, LightFighter, SuperCarrier, HeavyFighter, Destroyer_PlayerShip. `UpdatedStarships/`: Cruiser, Battleship, Destroyer_PlayerShip, Carrier, Corvette. Traced so far: 8 (`mesh_outline_*.png`). | **Fits with a tweak** for periscope and hangar views. Too detailed for a 30 px plot mark. |
| Weapon tracks | Prototype missile and round code, `starwolf-prototype.html` lines 2791–2860: bright dot, glow, fading trail. | **Partial.** The look is defined. Needs palette colours. |
| Contact states (new, lost, wreck) | — | **No candidate.** |

---

## Still on the Claude Design request (no candidate)

- **2.2** Should the sensor ring follow the map scale?
- **3.3** Rockers as separate housing and pad.
- **3.4** Push-button pressed state.
- **3.5** Lit-cell looks and which way the bus cells fill.
- **3.9** Edge data on drawn shapes, and the key glows.
- **2.9** The inner dashed ring inside the sensor ring. What is it?
- Contact states: new, lost and wreck.

The partial items (1.6, 1.8, 2.7, 2.8, 3.1, 3.6, 3.7, 3.8, 3.10, 3.11) stay on the request. 2.5 stays on too, for arc sizes and the missile arc. Our material only narrows those questions.

## Origin and licence

- **Claude Design's older handoff** (`References/Tactical spaceship UI planning.zip`). Same designer, safe to use, but an older version. Examples: the PDC arc was green, now amber. The own ship was a solid wedge, now a white outline. The amber and cyan changed too. The new package wins. The zip was unpacked into a scratch folder only. `References/` was not touched.
- **Ours:** `Assets/Art/*` (KodiakPlays), the 3D ship and asteroid models (KodiakPlays), most of `Assets/UI/*`, and `Assets/Sprites/Circle1.PNG` (team, 2025–26). `targetScan.png` was made by phranq in Adobe ImageReady: low risk. The prototype code is ours.
- **`export/sprites/Starship_Silhouette.png` was made by the owner**, with ChatGPT image tools. The owner confirmed this on 2026-10-05. It is usable.
  - It is **not** part of the Claude Design package. It was added after the package, so the README count (110 sprites) leaves it out. It has no @2x copy.
  - Best uses: 3.2, the engineering outline, turned nose-up. Also the hangar and other large views of the Starwolf. Not the periscope, which shows other ships. Not the 30 px plot mark.
  - It must be imported as a board sprite before use, and only with the owner's yes.
- **Flag: `Assets/Sprites/icons8-tie-fighter-100.png` and `icons8-triangle-100.png`** come from Icons8. The free licence needs a credit line, and the first is a Star Wars TIE fighter. Keep both out.
- **Flag: unknown origin.** `Assets/Sprites/gen-overhead-vector-art-wireframe-interceptor-fast-shi.png` looks AI-made. Also unknown: `Assets/Sprites/Interceptor.png`, `Assets/UI/Sprite/triangle-128.png` and `circle-xxl.png`. Don't use any of them.
- **Flag: the reference PDFs** (the Art Spec and the GDD) contain screenshots from other games, such as In Other Waters. Mood only; nothing in them is usable art.
- **TextMesh Pro example textures** are Unity's own. Nothing in them fits.
