# Tactical Scenes — Build Log

A running record of the new scene flow being built under `Assets/Scenes/Tactical/`.

**Read this first if you are a new session, or this session after a compaction.** It tells you what
exists, what is wired to what, which step we are on, and what to do next. You should not have to
work any of that out again.

Last updated: 2026-10-17. Unity 6000.5.10f1. Editor instance `Killshot@99ec1537206126d5`.

---

## 1 · Where we are

**Steps 1 to 64 are done, plus a final QA pass.**

The game now opens on the SECTION 36 intro, which runs itself through to a menu. NEW GAME leads to
the briefing, Enter leads to the board in `02.1_Grid`.

The board looks and clicks right. **Wiring to real game systems began in Step 34.** One dummy enemy
now sits in a real world at TRACK 07's spot, and the board reads it. It does not move or fire. There
is no mission yet. Since Step 35, power makes the ship loud (signal), and nothing real changes while
paused. Since Step 36 the plot tools mark world positions; since Step 37 our ship shows on the plot as
its 3D model seen from above. Every tunable value is in **Starwolf → Tuning** (Step 38). The plot
carries no standing text (Step 39). Switches slide, refuse and click (Step 40). Our ship moves and the
plot pans (Step 41). The wiring order is at the top of section 8.

The older board in `02_Grid` (Steps 5–26) is out of the play flow. It had an amber grid, corner
panels, two icon rails, a targeting drawer, power, WASD movement and five plot tools.

**Changed direction on 2026-10-04.** A new Claude Design handoff arrived at
`Assets/UI2/Claude Design handoff/export/`. It is now fully built into `02.1_Grid`, and the final QA
passed. Every control is clickable and shows its states. Wiring to game systems began in Step 34. The
flow is `00_Start` → `01_Briefing` → `02.1_Grid`. `02_Grid` is still in Build Settings, but nothing
leads to it. The full record is `Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md`.

**Next up: sensors**, then periscope and fire control, fire, hit — the owner's wiring order, top of
section 8. Power P1 (Step 35), the plot tools (Step 36), press and click (Step 40) and the helm
(Step 41) are done. The UI asset audit is done (`ASSET_AUDIT.md`). The Claude Design request still
stands, but placeholder art is fine, so it does not block a demo.

The owner is building this one step at a time and checking each step before starting the next one.
Do not run ahead. Do one step, show it, wait.

| Step | What it is | Status |
|---|---|---|
| 1 | Folder, three empty scenes, Build Settings | **Done** |
| 2 | Build `00_Start` — camera, canvas, title, START GAME control | **Done** — checked on screen |
| 3 | Build `01_Briefing` — same canvas, work-in-progress text, "Press Enter" prompt | **Done** — checked on screen |
| 4 | Write `ScreenFlow.cs`, wire both intro scenes, test the chain in play mode | **Done** — ran twice in play mode |
| 5 | Build `02_Grid` — the tactical grid surface | **Done** — checked on screen |
| 6 | Corner panels fixed and verified | **Done** — checked in play mode |
| 7 | Linear colour space fix on the grid | **Done** |
| 8 | Art profile ScriptableObject created | **Done** |
| 9 | Tool icons corrected to the handoff | **Done** |
| 10 | Grid turned amber, with a third tier of lines | **Done** |
| 11 | The orange panel bug, and the layer that fixes it | **Done** |
| 12 | The two icon rails | **Done** |
| 13 | Power controls brought over from the cockpit | **Done** |
| 14 | The board's data layer, and the targeting drawer | **Done** — checked in play mode |
| 15 | The outer metal frame | **Done** |
| 16 | Frame thinned | **Done** |
| 17 | Engine power and WASD movement | **Done** — power verified, keys not |
| 18 | Three bugs on the plot, and the plot camera | **Done** |
| 19 | Tuning asset | **Done** |
| 20 | Why the power bars never moved | **Done** — checked in play mode |
| 21 | Plot layer, part one: camera, zoom range and input | **Done** |
| 22 | Own ship glyph: minimum size instead of constant size | **Done** |
| 23 | Plot layer, part two: annotation rendering and the tools | **Done** — every tool checked in play mode |
| 24 | Cut to five tools, MOUSE as default, active-tool highlight | **Done** — checked on screen |
| 25 | Plot labels — measurements shown on the board | **Done** — checked in play mode |
| 26 | Ship readout — speed and heading | **Done** — checked in play mode |
| 27 | Intro and menu wired into the flow, and four rendering bugs in the intro package | **Done** — flow checked in play mode; CRT treatment not yet seen on screen |
| 28 | New scene `02.1_Grid` for the new handoff, wired after the briefing | **Done** — flow checked in play mode |
| 29–33 | The new handoff built into `02.1_Grid`: import, builder, static board, movement, clickable controls (33a–33j), final QA | **Done** — FINAL QA: PASS. Detail in `UI_INTEGRATION_LOG.md` |
| 34 | Wiring begins: the dummy enemy exists and the board reads it | **Done** — verified (STEP 34: VERIFIED on the second pass) |
| 35 | Power makes the ship loud (signal), and nothing real changes while paused | **Done** — verified (STEP 35: VERIFIED, first pass) |
| 36 | Plot tools: marks stay put in space | **Done** — verified (STEP 36: VERIFIED, third pass) |
| 37 | Our ship on the plot, as the 3D model seen from above | **Done** — verified (STEP 37: VERIFIED, second pass) |
| 38 | Every tunable value in one window (Starwolf → Tuning) | **Done** — checked; one defect fixed and re-checked |
| 39 | Nothing standing on the plot | **Done** — checked after a rebuild and a playtest |
| 40 | Press and click: switches slide, refuse, and click | **Done** — playtest 48/48, slide measured in play |
| 41 | The helm: our ship moves, and the plot pans | **Done** — playtest 57/57 |
| 42 | The sensor circle: DRADIS's hearing range around our ship | **Done** — playtest 60/60 |
| 43 | The radar sweep | **Done** — playtest 62/62 |
| 44 | Hearing the enemy: bearing lines, the alert strip, the tracked blink | **Done** — playtest 68/68 |
| 45 | The bearing trail: plotting a passive contact's range | **Done** — playtest 70/70 |
| 46 | Intercept helpers: time to reach, tracks, weapon reach | **Done** — playtest 76/76 |
| 47 | The firing solution dialled by hand | **Done** — playtest 81/81 |
| 48 | Fire and hit: the railgun slug | **Done** — playtest 93/93 |
| 49 | Damage: armour, compartments, the kill | **Done** — playtest 99/99 |
| 50 | Engineering's ship diagram | **Done** — playtest 103/103 |
| 51 | The enemy moves: TRACK 07 patrols | **Done** — playtest 107/107 |
| 52 | Leading a moving target | **Done** — playtest 113/113 |
| 53 | Missiles: the launcher, homing, three warheads | **Done** — playtest 121/121 |
| 54 | Fighters: launch, fly out, attack runs, recall | **Done** — playtest 130/130 |
| 55 | PDCs, and the enemy fires back | **Done** — playtest 138/138 |
| 56 | The intro: flat black, typed in place, straight cuts | **Done** — checked in play mode |
| 57 | Everything in game time | **Done** — playtest 138/138 |
| 58 | The audit's side map; our ship can die | **Done** — playtest 139/139 |
| 59 | Conditions: fire, vacuum, radiation | **Done** — playtest 153/153 |
| 60 | Boarding: the assault boats | **Done** — playtest 160/160 |
| 61 | Scan to know what you hit; smoothing; heading 000 | **Done** — playtest 161/161 |
| 62 | Weapon reach, the sweep's inner circle, the enemy's picture | **Done** — playtest passed with Step 63 |
| 63 | Scale: everything that moves 5× faster; ACTIVE scans; the gain comes back | **Done** — playtest 163/163 |
| 64 | Lose power, lose the setting | **Done** — playtest 163/163 |

Steps 6 to 28 and Steps 34 to 64 are written up in section 6. Steps 29 to 33 are written up in `UI_INTEGRATION_LOG.md`. What is still not done is in **Open items**, at the very
bottom of this file. Read that before you pick up any work.

---

## 2 · Scenes

All scenes live in `Assets/Scenes/Tactical/`. The play order is now `00_Start` → `01_Briefing` →
`02.1_Grid`. It was `02_Grid` until Step 28. They were added to Build Settings in that order and all three enabled.

| Scene | GUID | State |
|---|---|---|
| `00_Start.unity` | `e50db2cd323304012ba3e7afb4e531cb` | Built |
| `01_Briefing.unity` | `326d3f3159fcb4965bebcf61495595c6` | Built |
| `02_Grid.unity` | `7590f86ac5cb446509ffb65be8bb2360` | Built — no longer in the play flow |
| `02.1_Grid.unity` | — | Built — the new handoff board, Steps 28–33. In the play flow after `01_Briefing` |

### `00_Start` — built in Step 2

- `Main Camera` — orthographic, clears to `#0A0C0E`.
- `EventSystem` with a `StandaloneInputModule`. **This is not optional.** A Button does nothing at
  all without one in the scene.
- `Canvas` — Screen Space **Camera**, not Overlay. CanvasScaler on Scale With Screen Size,
  1920x1080, match 0.5.
- `Title` — TextMeshPro, reads "STARWOLF", 110pt.
- `StartButton` — an Image with a Button on it, and a child `Label` reading "START GAME".
- `ScreenFlow` — added in Step 4, see section 3.

### `01_Briefing` — built in Step 3

Same camera, EventSystem and Canvas setup as `00_Start`.

- `BriefingText` — "Welcome to Starwolf Tactical Grid Test. / Gameplay, everything here is a work in
  progress. / Thank you for playing."
- `EnterPrompt` — "PRESS ENTER TO GO AHEAD", in amber `#E8A33D`.
- `ScreenFlow` — added in Step 4, see section 3.

### `02_Grid` — built in Step 5

This is the game board. The grid surface plus the four corner panels. See section 6 for how the
panels are put together.

- `TacticalGrid` — a Quad laid out in world XY, scaled to 140,000 units across, with the material
  below on it. Its collider was removed.
- `Main Camera` — at `(0, 0, -1000)`, orthographic, size 30,000, near clip 1, far clip 20,000,
  clears to void `#0A0C0E`.
- The old `Placeholder` text label was deleted.
- Four corner panels, added in Step 6: `ShipStats`, `Power`, `Weapons`, `Scanner`. Each one is a real
  `UIController` canvas rendered to a RenderTexture and shown on a RawImage.

The scene stays 3D and top-down. That is deliberate — collision and physics will work later without
having to redo any of this.

### `02.1_Grid` — made in Step 28, board built in Steps 29–33, World added in Step 34

The board from the new Claude Design handoff. The briefing's Enter now leads here. The board is
built and verified. Step 34 added the first game state: a `World` root object (below).

- Built by two menus: `Starwolf/Handoff/1 · Apply import settings` (`Assets/Editor/HandoffImport.cs`)
  and `Starwolf/Handoff/2 · Build static board in 02.1_Grid` (`Assets/Editor/HandoffBoardBuilder.cs`).
- Runtime code: `Assets/Scripts/Tactical/Handoff*.cs`, `GammaBoardPresenter.cs`, `CssEase.cs`.
- The full record: `Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md`.
- What the package left out: `Assets/Scenes/Tactical/HANDOFF_GAPS.md` (106 rows).
- What Claude Design must still provide, verified: `Assets/Scenes/Tactical/CLAUDE_DESIGN_NEEDS.md`.

The setup from Step 28:

- `Main Camera` — orthographic, clears to `board.ground` `#0A0C0E`, with an `AudioListener`.
- `EventSystem` with a `StandaloneInputModule`.
- `Board` — Canvas, Screen Space **Camera**, plane distance 5, CanvasScaler 1920x1080, match 0.5,
  reference pixels per unit 100. These are the handoff README's canvas numbers.

Added in Step 34:

- `World` — a root object, **outside** the Board canvas, with `TacticalWorld` on it. Children:
  - `OwnShip` at (0, 0, 0), with a `Model` child: `StarWolf_Destroyer_PlayerShip.fbx`, same model
    and placement as the old `Prefabs/PlayerShip.prefab`.
  - `Dummy · TRACK 07` at (18058.42, 16259.87, 0), with `WorldContact` (trackKey `t07`) and a
    `Model` child: `StarWolf_Cruiser.fbx`.
  - Both models at scale 0.11612, about 12–14 km long. Models only, no old scripts.
- The 26 mock contact art objects on the plot are hidden, not deleted.

### Deleted in Step 1

These were leftovers from an earlier attempt that was abandoned. They are gone on purpose. Do not
bring them back.

- `Assets/Scenes/Sandbox/` and `TacticalGrid_Test01.unity` inside it.
- The old `Assets/Scripts/Tactical/` and the three unfinished files in it: `Bearing.cs`, `Track.cs`,
  `TacticalRadar.cs`. (The folder exists again now, but only for `ScreenFlow.cs`.)
- `TacticalRadar.cs` was deleted rather than repaired. It referenced the old cockpit `UIController`,
  which must stay out of these scenes (see Decisions).

---

## 3 · Scripts, shaders and wiring

### `ScreenFlow.cs` — new in Step 4

Lives at `Assets/Scripts/Tactical/ScreenFlow.cs`. It is small and reusable.

- Two fields you set in the inspector: `nextScene` (the scene name, as text) and `advanceOnEnter`
  (a tick box).
- One public method, `Advance()`, which loads `nextScene`.
- It has a `going` guard so it cannot fire twice.
- It logs an error if `nextScene` is left empty.

Where it is attached:

| Scene | Sits on | `nextScene` | `advanceOnEnter` | How it fires |
|---|---|---|---|---|
| `00_Start` | Its own root object, named `ScreenFlow` | `01_Briefing` | off | `StartButton`'s Button `onClick` calls `Advance()` |
| `01_Briefing` | Its own root object | `02.1_Grid` (was `02_Grid` until Step 28) | on | Enter key |

Note the script sits on **its own root GameObject**, not on the button or the canvas.

The chain was run in play mode twice in Step 4: `00_Start` → click → `01_Briefing` → advance →
`02_Grid`. Both times it worked. Since Step 28 the chain ends at `02.1_Grid` instead, also checked in
play mode.

### The handoff board — new in Steps 29–33

Everything on the `02.1_Grid` board. Detail is in `UI_INTEGRATION_LOG.md`; do not copy it here.

| File | What it is |
|---|---|
| `Assets/Editor/HandoffImport.cs` | Menu `Starwolf/Handoff/1`. Import settings for the handoff images and fonts. |
| `Assets/Editor/HandoffBoardBuilder.cs` | Menu `Starwolf/Handoff/2`. Builds the board into `02.1_Grid`. |
| `Assets/Scripts/Tactical/HandoffBoardState.cs` | The board's data. A port of the handoff's `board-logic.js`. Every panel reads and writes only this. |
| `Assets/Scripts/Tactical/HandoffBoardControls.cs` | Runs the 0.42 s tick and the board's own clock. |
| `Assets/Scripts/Tactical/Handoff*Panel.cs` | One per panel: power, sensors, comms, engineering, hangar, fire control, helm, orders, periscope, plot. |
| `Assets/Scripts/Tactical/Handoff*.cs` (the rest) | Shared pieces: motion, easing, hit testing, shapes, marks, plot grid, scope sight. |
| `Assets/Scripts/Tactical/GammaBoardPresenter.cs` | Shows the board in gamma space. See decision 15. |
| `Assets/Scripts/Tactical/CssEase.cs` | The handoff's CSS easing curves. |

Until Step 34 nothing here touched a game system. Now the World feeds `HandoffBoardState` (below).
Since Step 35, `HandoffBoardState` turns power into `signal`, and `HandoffBoardControls` holds the
whole tick while paused. Details in section 6, Step 35.

### `TacticalWorld.cs` and `WorldContact.cs` — new in Step 34

| File | On what | What it is |
|---|---|---|
| `Assets/Scripts/Tactical/TacticalWorld.cs` | the `World` root object | Owns the game state. Each board tick it writes true bearing and range into `HandoffBoardState.tracks`. |
| `Assets/Scripts/Tactical/WorldContact.cs` | each contact in the World | Holds `trackKey`, the board track it feeds. |

Changed for it:

- `HandoffBoardControls.Ticked` — fires once per board tick, after the state ticks, before the
  panels redraw. The world steps inside the board's single timer.
- `HandoffBoardState.KeepTracks` — drops tracks with no world contact, and prunes identified and
  classified.
- `HandoffPlotPanel` — draws identified hostile contacts at their true km position.
- `HandoffBoardBuilder` — new `Contacts (drawn)` and `Contact labels` layers, hides the mock contact
  art, and `RelinkWorld` re-points the World at the new board after every rebuild.

Details in section 6, Step 34.

### Grid shader and material — new in Step 5

- `Assets/UI/Shader/TacticalGrid.shader` — world-space, unlit, transparent. The grid lines are worked
  out from world XY.
  **This entry used to claim `fwidth` held the stroke to one pixel. That was wrong.** `fwidth` was
  only feeding the antialias. The width was in world units until Step 18 fixed it. See Known
  issues #19.
- `Assets/UI/Shader/TacticalGrid.mat` — cell size 2,000 world units. A heavier line every 5 cells,
  so every 10,000 units. Line width 12. Background plot-glass `#0A1719`. Minor lines sensor cyan
  `#3FA9C9` at 10%, major lines the same cyan at 26%. Centre crosshair in orders amber `#E8A33D`
  at 55%.

**`Assets/UI/Shader/NavGrid.shader` and `NavGrid.mat` are still on disk but nothing uses them.**
NavGrid is a UI shader with stencil properties on it. It is the wrong tool for a plane sitting in the
world. Do not try to go back to it.

### `TacticalArtProfile.cs` — new in Step 8, cut back in Step 14

Lives at `Assets/Scripts/Tactical/TacticalArtProfile.cs`. It is now the **theme only**: frames, hull
glyphs, and the colour set. The per-button icons moved onto `BoardAction` assets in Step 14.

### The board's data layer — new in Step 14

The rule: **a ScriptableObject holds design data that is true regardless of scene. A MonoBehaviour
holds scene wiring and runtime state.** Render textures are assets, so a `BoardAction` can hold one.
Cameras and canvases are scene objects, so it cannot.

| File | Asset | What it is |
|---|---|---|
| `Assets/Scripts/Tactical/RailStyle.cs` | `Assets/Art/Board/RailStyle.asset` | The rail geometry. Both rails read this one asset. |
| `Assets/Scripts/Tactical/BoardAction.cs` | 13 assets in `Assets/Art/Board/Actions/` | One per rail button. |
| `Assets/Scripts/Tactical/IconRail.cs` | — | Builds a rail from a `RailStyle` and a list of actions. |
| `Assets/Scripts/Tactical/DrawerPanel.cs` | — | Slides a panel in and out. |
| `Assets/Scripts/Tactical/BoardController.cs` | — | On a root `Board` object. Routes rail clicks to drawers. |

Details in section 6.

### `PlotCamera.cs` — new in Step 18

Lives at `Assets/Scripts/Tactical/PlotCamera.cs`, on `Main Camera` in `02_Grid`. Scroll wheel zoom,
plus `ScreenToPlot` and `PlotToScreen` — the screen-to-world bridge the tools and the targeting
computer both need. **This is the first piece of the plot layer.** Details in section 6.

### The plot layer — new in Step 21

| File | On what | What it is |
|---|---|---|
| `Assets/Scripts/Tactical/PlotGridFitter.cs` | the `TacticalGrid` quad | Scales the quad to cover the view, so the plot never runs out at the edges. |
| `Assets/Scripts/Tactical/PlotSurface.cs` | child 0 of the Canvas | Catches clicks, drags and hovers on the board and reports them in **world** coordinates. |
| `Assets/Scripts/Tactical/PlotAnnotation.cs` | — | One mark: Point, Line, Ruler or Ring. Stored in **world** coordinates. Added in Step 23. |
| `Assets/Scripts/Tactical/PlotOverlay.cs` | `Canvas/PlotOverlay`, child 1 | Draws every mark as one mesh, one draw call. Added in Step 23. |
| `Assets/Scripts/Tactical/PlotTools.cs` | the `Board` object | The five tool modes and the two immediate actions. Added in Step 23. |

`PlotCamera.ScreenToPlot` and `PlotToScreen` are **the single screen-to-world conversion**. Nothing
else should write its own. Details in section 6.

### `PowerMeterBinding.cs` — new in Step 20

`Assets/Scripts/Tactical/PowerMeterBinding.cs`, on a `PowerMeters` child of PlayerShip. It drives the
power bars **straight from `PowerManager`**, reusing the existing `UI_GradeDisplay` art, and does not
go through `UIController` at all. Details in section 6.

### `TacticalTuning.cs` — new in Step 19

`Assets/Scripts/Tactical/TacticalTuning.cs`, asset at `Assets/Art/Board/TacticalTuning.asset`, with
`Assets/Editor/TacticalTuningEditor.cs` for the buttons and the `Starwolf > Tuning` menu.
**It is not a runtime indirection** — Apply writes real serialized values into the scene, Capture
reads them back. Nothing reads the asset while the game runs. Details in section 6.

### Grid material changes since Step 5

The centre crosshair is gone — `_AxisEnable` is 0 on `TacticalGrid.mat`, at the owner's request. All
colours set from code now get `.linear` on them. See Step 7 in section 6, and Known issues #6.

The grid is amber, not cyan, and has three tiers of lines. The shader was rewritten for it in
Step 10. **The colours come from `Assets/Art/TacticalArtProfile.asset`** — that asset is the source
of truth, the material is only where the values land.

### Existing things this flow will reuse

Reference these. Never edit them.

| Thing | Why we keep it |
|---|---|
| `QuestSystem` | Its stages — `NotStarted` → `LocateTarget` → `EliminateTarget` → `ExitRegion` → `MissionComplete` / `Failed` — already match the design's win condition. Keep as-is. |
| `WorldBoundary` | Reused. |
| `PowerManager` | Used by `02_Grid` only. Its five systems clash with the new board's four buses. **Not revived for `02.1_Grid`** (decision 16). |
| UI shaders: `UI_PowerArrow`, `UI_ShipHack`, `ScanLines`, `UI_GradeDisplay*` | Reused. |
| Rail icons in `Assets/Art/Icons/` | 15 icons already extracted. Step 8 uses them. |

### Things to leave alone

| Thing | Why |
|---|---|
| The old cockpit `UIController` | **Keep it out of the new scenes.** Its `Awake` destroys its own GameObject when it finds a duplicate, and that deletes the player ship. |
| `GameManager.cs` | **Never delete this file.** `ReadOnlyAttribute` is declared at the bottom of it. Removing the file breaks `[ReadOnly]` everywhere in the project. |

---

## 4 · Decisions already made

These are settled. Do not reopen them.

1. **Three separate scenes, not one scene with canvases toggled on and off.**
2. **New code for the scene flow.** Existing managers get referenced when needed. They do not get
   edited.
3. **`QuestSystem` stays as it is.** Its stages already fit the design.
4. **The old cockpit `UIController` stays out.** See the table above for why.
5. **`GameManager.cs` is never deleted.** See the table above for why.
6. **Legacy input for these scenes** — `Input.GetKeyDown`. Both input backends are enabled in this
   project, so both compile. This flow picks the legacy one deliberately.
7. **Canvas setup for the intro scenes:** Screen Space **Camera**, CanvasScaler on Scale With Screen
   Size, reference resolution 1920x1080, match 0.5. (This started out as Overlay in the Step 1 plan.
   It was built as Screen Space Camera and that is what it stays. See Known issues #2 for the catch
   that comes with it.)
8. **Colours come from the design handoff tokens.** Do not invent new ones.
   - Background / void `#0A0C0E`
   - Text `#E8EDF0`
   - Prompts and orders, amber `#E8A33D`
   - Sensor cyan `#3FA9C9`
   - Plot glass `#0A1719`
9. **The grid is a world-space plane, not a UI element.** `TacticalGrid.shader`, not `NavGrid`.
10. **The scene stays 3D top-down**, so collision and physics work later without rework.
11. **The new handoff wins on weapons and power** (owner, 2026-10-04, editable later): two railgun
    turrets, two PDC groups, one missile launcher, four power buses (ENG, WEP, SEN, AUX). Its fire
    sequence and the missile salvo knob are wanted.
12. **Grid tint toggles white and amber**, not white and red. Something to tune.
13. **Distances are kilometres. One world unit is one kilometre.**
14. **The handoff is built in order:** static board first, then movement, then interactions with
    no game wiring, then the wiring.
15. **The board loads undamaged** (all green, rooms sealed) **and is gamma-space only.** See
    `UI_INTEGRATION_LOG.md`, section 3, and its Step 31.
16. **A new World object owns the game state and feeds the board** (owner, 2026-10-17). The old
    `Radar.cs`, `EnemyShip`, `PlayerShip` and `PowerManager` scripts are not revived.
17. **Wire one feature at a time** (owner, 2026-10-17). Plan each feature with the owner. Don't
    invent: follow the sources. Each feature must work before the next starts.
18. **The North Star is a dummy enemy at a fixed position** (owner, 2026-10-17). It sits at TRACK
    07's spot, bearing 048, 24,300 km. It does not move or fire. It is there to check the radar,
    moving around it, and shooting at it. Its plot dot shows only once identified.
19. **Ships are real 3D objects seen top-down** (owner, 2026-10-17), so future external viewport
    cameras can look at them. Their real scale is decided when those cameras come.
20. **Power makes the ship loud** (owner, 2026-10-17). Every unit drawn to a bus adds signal, and so
    does ACTIVE SCAN. The internal value is **signal**, on a 0–100.0 scale. Every amount is an
    Inspector field. It climbs and falls gradually, and never jumps straight to 100. The player sees
    only a whole percent. Details in Step 35.
21. **Nothing real changes while paused** (owner, 2026-10-17). The board tick, the world and the
    mission clock all wait until RUN. Controls still work.
22. **Plot marks stay put in space** (owner, 2026-10-17). POINT is a timestamp, RULER a distance,
    COMPASS sits around our ship. Plots feed fire control later. Details in Step 36.
23. **Every tunable value is in one window**, Starwolf → Tuning (owner, 2026-10-17). Tag every new
    tunable with `[Tune("Section")]`. Details in Step 38.
24. **Nothing standing on the plot** (owner, 2026-10-18). No always-on labels. Readouts come from
    using a tool. The PDC arcs show once the PDCs are activated; the alarm strip shows only for a
    real alert. Details in Step 39.
25. **Switches slide, refuse and click** (owner, 2026-10-18). Rocker pads slide as the design
    animates them. A locked switch nudges when clicked (owner addition). Click sounds come from the
    prototype. Unity audio is on. A refused order keeps the design's reaction, plus the refused
    sound. Details in Step 40.
26. **Our ship moves across a fixed grid** (owner, 2026-10-18). Top speed about 2.5 km/s. The plot
    pans by dragging the grid or with the arrow keys. Details in Step 41.
27. **Plotting feeds the firing solution** (owner, 2026-10-23). A passive contact gives a bearing
    only, and the bearing trail finds its range. The solution is dialled by hand, aiming at a point
    in space; TRACK needs an active scan. Details in Steps 45–47.
28. **Railgun: a 50 km/s slug that hits within a class radius, aimed in whole degrees** (owner,
    2026-10-23). Whole degrees stay on purpose: from ~24,000 km even the best aim can just miss a
    cruiser. Close in for a sure shot. Details in Step 48.
29. **Fighters fly and fight on the plot; RECALL brings them home** (owner, 2026-10-23). 8 km/s,
    attack runs of 10 armour per fighter, wedges in a V with no labels. They land when they reach
    us. Details in Step 54.
30. **PDCs: arm the ladder for AUTO, a chance per missile; the enemy fires back when hit** (owner,
    2026-10-24). Enemy PDCs wait for the ship AI. Details in Step 55.

### The layout of the tactical grid — agreed, this is the direction

The old cockpit had five separate screens. In `UIController`'s naming they were `Canvas-Power`
(PowerManager), `Canvas-Navagation` (radar and sonar), `Camera-World` → `ScreenWorld` (world map),
`Canvas-PlayerShipStats` (ship stats), `Canvas-Weapon` (weapons) and `Canvas-BogieShipStats` (the
enemy scanner).

The new layout throws that split away:

- **The grid fills the whole screen.**
- **Radar, nav and the world map are all the same grid at different zoom levels.** They are not
  separate screens any more.
- **Ship stats sit top-left and are always visible.** Never behind a click.
- **Power runs along the bottom.** A condensed readout that is always there, plus a button that
  expands the full draw and vent controls. Power is the thing the player touches most, so it must
  never be fully hidden.
- **Weapons, the enemy scanner and comms sit behind icon buttons.** They open as panels over
  the grid.
- **Always visible, never behind a click:** signature, hull by side, alert state, heading, speed.

---

## 5 · Known issues and traps

1. ~~Build Settings not on disk.~~ **Fixed.** All three tactical scenes are now written into
   `ProjectSettings/EditorBuildSettings.asset`. Worth remembering why it looked broken before: that
   file only catches up when Unity saves the project, so a scene you just added in the Editor will
   not show up on disk until then.

2. **TextMeshPro made from a script shows nothing at all.** This one bit us. If you set the font by
   writing `m_fontAsset` through serialization, `m_sharedMaterial` stays null and `characterCount`
   stays 0 — you get an object with no visible text. The font's material is a sub-asset inside the
   font file, so it does not come along for the ride. Set it through the C# properties instead:

   ```
   tmp.font = font;
   tmp.fontSharedMaterial = font.material;
   tmp.ForceMeshUpdate(true);
   ```

   The project font is `Assets/UI/Fonts/Optimus SDF.asset`.

3. **Children of a Screen Space-Camera canvas come out the wrong size.** The canvas itself sits at
   scale 0.01, and anything created under it while it is in that mode inherits a nonsense
   `localScale` — we got 132.86 in the briefing scene and 1.59 in the start screen. `00_Start` was
   quietly 59% too big before we caught it. **Fix: set every UI child's `localScale` to
   `Vector3.one` right after you create it.** Both these bugs will come back in Steps 6, 7 and 8.

4. **The Enter key itself has not actually been pressed.** `Input.GetKeyDown` cannot be faked from
   outside the editor, so the play-mode test called `Advance()` directly. That is the same code path,
   so the flow is proven — but the owner should press Enter by hand once to be sure.

5. **Unity UI draws children on top of their parents.** So a frame that is a child of the thing it
   frames will cover it. The corner panels had this exact bug in Step 6. `Frame` and `Screen` must be
   **siblings**, with `Frame` first in the hierarchy.

6. **This project renders in Linear colour space.** Anything you hand to `Material.SetColor` is
   treated as a linear value. Pass an sRGB number straight in and it comes out washed out — ours went
   grey-green. **Fix: append `.linear` to every colour you set from code.** This will bite again on
   any new material.

7. **Render textures in `UI.prefab` are all 1024x1024, square.** Show one on a 16:9 RawImage and the
   picture stretches. The corner RawImages are 300x300 for that reason. They are not 420x236.

8. **Do not unzip anything in `References/`.** The design handoff zip gets extracted to the session
   scratchpad and read from there. `References/` is owner-authored input and is never edited.

9. **Any new world-space object in `02_Grid` must go on the `TacticalPlot` layer (layer 8).** If it
   does not, the UI cameras will photograph it. This is what turned the ShipStats panel orange in
   Step 11: every camera had a culling mask of everything, so `Camera-PlayerShipStats` was picking up
   the 140,000-unit grid quad as a backdrop behind the ship.

10. **Reparenting a UI subtree collapses it to 0x0 unless you set `sizeDelta` yourself.** The
    original objects sized themselves from stretch anchors against a parent they no longer have. Set
    the size the layout groups expect, then `LayoutRebuilder.ForceRebuildLayoutImmediate`, then
    `Canvas.ForceUpdateCanvases()`, then rebuild again. This cost time in Step 13.

11. **System icons must not be tinted.** They carry their own colour and the handoff says so
    explicitly. Tool icons are drawn white and do get tinted. That is what `tintIcons` is for.

12. **A solid `Image` is a fill, not a border.** The targeting drawer's frame was one, and it turned
    the empty lower half of the drawer into an amber slab. A border is four thin edge Images under a
    `Border` child. The corner panels get away with a solid fill only because their render texture
    covers it completely.

13. **Unity rescales a non-power-of-two texture on import.** `frame_metal.png` came in as 2048x1024,
    which broke the exact 1920x1080 mapping the frame bands depend on. Set
    `npotScale = TextureImporterNPOTScale.None`.

14. **Setting `textureType = Sprite` on its own produces no Sprite subasset.** `spriteImportMode`
    must also be set to `Single`, or `LoadAssetAtPath<Sprite>` returns null and tells you nothing.

15. **An asset reimport can bounce the editor back into play mode.** A scene build ran during that
    and was silently discarded. **Check `Application.isPlaying` before any scene edit**, not just
    once at the start of a session.

16. **`PlayerShip.Awake` only constrains a Rigidbody it created itself** (`PlayerShip.cs:84`). A ship
    that carries one from the prefab gets no constraints at all, and will drift in Z and tumble in X
    and Y on what is meant to be a flat top-down plot. **Any new ship added to a Tactical scene needs
    `FreezePositionZ | FreezeRotationX | FreezeRotationY` set on it by hand.**

17. **`PlayerSettings.runInBackground` must stay on.** With it off, play mode freezes solid whenever
    the editor loses focus — `Time.frameCount` sat at 1 across repeated probes. **This is why an
    automated play-mode test looks like it does nothing.** Even with it on, driving play mode over
    time from outside the editor proved unreliable. Held keys have to be checked by hand.

18. **Grid line width was in world units, not pixels — and an earlier note in this file claimed
    `fwidth` already handled that. The note was wrong.** `fwidth` was only feeding the antialias.
    `_LineWidth` sat at 12 world units, which is invisible at strategic zoom and a 24 pixel amber bar
    at orthographic 600. All three tiers pass through x=0 and y=0, so it read as a bright amber cross
    and was mistaken for the centre crosshair, which was already off. `lineCoverage` now takes a
    width in **pixels** and multiplies by `fwidth(coord)`. `_LineWidth` is 1.1.

19. **`PlayerShip` hijacks `Camera.main` as a chase cam.** `PlayerShip.Start` (`PlayerShip.cs:133`)
    grabs it whenever `cameraTransform` is empty, then `:663-684` writes ship position **and ship
    rotation** onto it every FixedUpdate — which pivots the whole plot as the ship turns. **Any
    Tactical scene containing PlayerShip must fill in `cameraTransform`, or the plot camera gets
    hijacked.**

20. **`ScreenConstantScale` used to compound itself to nothing.** It measured its base size in
    `Awake`, so every domain reload took the already-scaled transform as the new base — the first run
    produced a scale of 0.12 instead of about 16. `baseScale` and `baseHeight` are now
    `[SerializeField, HideInInspector]` and measured once. Use `Remeasure(trueScale)` to start over
    deliberately.

21. **An exception early in `UIController.Update` silently disables everything after it.** This is
    the general lesson from Step 20: `UpdateCompass` (`:242`) and `ScanMovment()` (`:259`) threw a
    NullReferenceException every frame in a Tactical scene, so `SyncPowerBarsFromManager()` at `:315`
    never ran at all. The power system was fine; the display was dead. **When a panel in a Tactical
    scene looks frozen or static, check the console for a per-frame exception before assuming the
    underlying system is broken.**

22. **`PlotCamera` caches its `Camera` in `Awake`.** Calling `ScreenToPlot` from an editor script
    before play mode means setting that field first, or it has nothing to convert with.

23. **`ScreenConstantScale` no longer holds a constant size. The name is now misleading.** Since
    Step 22 it gives a **minimum** size and draws at true world scale above it. Renaming the class
    would break the scene reference, so it stays. **Trust the behaviour, not the name.**

24. **Never name a public MonoBehaviour method `Invoke`.** `PlotTools.Invoke(string)` collided with
    `MonoBehaviour.Invoke(string, float)`, and every reflection lookup threw an
    `AmbiguousMatchException`. It is now `Press`.

25. **UNDO and CLEAR ALL are actions, not modes.** Treat them as tools you can select and the board
    gets stuck in a tool that does nothing. `PlotTools` never makes them the active tool.

26. **A board rebuild drops the World's link to the board.** The World sits outside the canvas the
    builder replaces. `HandoffBoardBuilder.RelinkWorld` re-points it after every rebuild. The first
    Step 34 check caught this.

27. **Keep QA evidence in `Logs/`, not `Temp/`.** `Logs/` is git-ignored and survives Unity
    restarts. `Temp/` does not.

28. **The board loads PAUSED.** Since Step 35 nothing real changes until RUN: no fills, no charge,
    no signal, no clock. A check that forgets to press RUN sees nothing move.

29. **The reactor serves buses first come, first served.** A bus sent to DRAW late can find it
    already spent. The playtest sets every bus to DRAW while paused, so the reactor shares out evenly.

30. **A playtest control with no argument is saved as "", not null.** Check for empty, not null.
31. **Never call `InternalEditorUtility.ReadScreenPixel` from MCP `execute_code`.** On this Mac it
  crashed Unity outright (NSBitmapImageRep, `cgImage != NULL`) while a checker tried to photograph
  the tuning window (2026-10-18). Use the Game view or a camera render texture for pictures.
32. **An editor window must draw the same controls in Unity's Layout and Repaint passes.** Decide
    what to draw on the Layout pass only (play mode, the list). The tuning window broke this once
    when the playtest entered play between the two passes.

33. **The playtest video shows eased motion unevenly.** Board motion follows real time
    (`Time.unscaledTime`), so it keeps moving while the board is paused. The recorder spaces video
    frames on its own clock, so a 0.14 s slide shows as a few small steps and then a jump. In play it
    is smooth: logged frame by frame in Step 40. Judge feel in play, not from the video.

34. **A playtest screenshot shows the end of the frame.** `ScreenCapture` writes the frame once
    it is finished, so anything the playtest does before its next wait lands in the picture. Wait
    after a shot (`yield return 0.2f`) before the next action. Step 42 caught a "DRADIS on"
    shot that showed DRADIS off.

35. **A control can have more than one click target.** A drawer has its rail key and its own latch,
    and the latch stays active while the drawer slides away. Run `2026-10-23_090517` and run
    `221323` failed "blocked: topmost … is nothing": the playtest clicked the FIRE latch while it
    was still off the right edge, closing. (I first blamed someone touching the Unity window; that
    was wrong.) Since Step 53, the playtest's `Click` tries every matching target and uses the
    first one on top at its centre.

36. Nothing else. The Unity console is clean: no errors, no warnings.

---

## 6 · Steps 6 to 28, and 34 to 64, in detail

### Step 6 — corner panels fixed and verified

The four corner panels are `ShipStats`, `Power`, `Weapons` and `Scanner`.

- They are **real `UIController` canvases from `UI.prefab`**, rendered to RenderTextures and shown as
  RawImages. **They are not rebuilt UI.** Nobody remade these screens by hand, and nobody should.
- Each panel is `Frame` + `Screen` as **siblings**, with `Frame` first in the hierarchy. Unity UI
  draws children on top of parents, so while the frame was a child of the RawImage it simply covered
  the content. Making them siblings is what fixed it.
- The RawImages are **300x300**, not 420x236. Every render texture in `UI.prefab` is 1024x1024 —
  square — so a 16:9 RawImage stretched the picture.
- Confirmed working in play mode.

### Step 7 — linear colour space fix on the grid

- The project renders in **Linear** colour space. Values handed to `Material.SetColor` are treated as
  linear, so the sRGB numbers we were passing came out washed-out grey-green.
- **Fix: append `.linear` to every colour set from code** on `TacticalGrid.mat`.
- The amber centre crosshair was removed at the owner's request. `_AxisEnable` is set to 0 on the
  material.

### Step 8 — art profile ScriptableObject created

New file: `Assets/Scripts/Tactical/TacticalArtProfile.cs`. One ScriptableObject holding every sprite
and colour the tactical board uses, so art can be swapped later without touching the scene or any
code.

Create one from `Assets > Create > Starwolf > Tactical Art Profile`.

What is on it:

| Group | Fields |
|---|---|
| System icons (6) | targeting, sensors, reactor, hangar, comms, engineering |
| Tool icons (7) | point, line, ruler, compass, erase, undo, clearAll |
| Frames (2) | metalPlate, panelFrame |
| Hull glyphs (2) | hullFilled, hullOutline |
| Board colours | plot background, grid minor, grid major, icon tint, icon tint active, frame tint |

Why it exists: the owner said missing art is fine, so long as there is **one place** to swap the
assets in later.

### Step 9 — tool icons corrected to the handoff

- The first set of tool icons was generated by guesswork. The owner pointed out the real ones are in
  the design handoff.
- Source: `References/Tactical spaceship UI planning.zip`, file `Star Wolf Asset Kit.dc.html`,
  section **6.2 TOOL BUTTON**. The glyphs there are **CSS shapes, not SVGs**, drawn at 19x19 with a
  1.5px stroke.
- The exact shapes recorded there:
  - **POINT** — a full-width crosshair.
  - **LINE** — a single stroke at -42 degrees.
  - **RULER** — a 20x5 outlined bar at -42 degrees.
  - **COMPASS** — a circle with a needle from top centre.
  - **ERASE** — an X at plus and minus 45 degrees.
  - **CLEAR** — a dashed rectangle.
- The handoff zip is **never unzipped in place**. Extract it to the session scratchpad and read it
  from there. `References/` is owner-authored input and is never edited.

### Step 10 — grid turned amber, with a third tier of lines

- `TacticalGrid.mat` colours now come from `Assets/Art/TacticalArtProfile.asset`, converted with
  `.linear`. **The profile is the source of truth. The material is just where the values land.**
- `TacticalGrid.shader` was rewritten to three tiers of lines: fine at 400 units, minor at 2,000,
  major at 10,000.
- **Each tier fades on its own.** A tier is invisible once its lines crowd closer together than
  `_FadeMin` (6) pixels, and full above `_FadeMax` (26). So zooming out thins the grid instead of
  turning it to mush, and zooming in reveals finer detail.
- New shader properties: `_FineColor`, `_FineDivisions`, `_FineWidthScale`, `_FadeMin`, `_FadeMax`.
- The corner panel frames changed from cyan to the profile's `frameTint` amber.

### Step 11 — the orange panel bug, and the layer that fixes it

- **Symptom:** once the grid went amber, the ShipStats panel filled with orange.
- **Cause:** every camera in the scene had a culling mask of -1, meaning everything.
  `Camera-PlayerShipStats` was photographing the 140,000-unit grid quad as a backdrop behind the
  ship model.
- **Fix:** a new layer 8, `TacticalPlot`. `TacticalGrid` was moved onto it, and the layer was removed
  from every camera's culling mask except `Main Camera`.
- An opaque black `Backdrop` Image was also added as child 0 of all four panels. Panel order is now
  `Backdrop, Frame, Screen`. Nothing behind a panel can bleed through it any more.
- **The trap worth remembering: any new world-space object in this scene must go on the
  `TacticalPlot` layer, or the UI cameras will photograph it.**

### Step 12 — the two icon rails

- New file `Assets/Scripts/Tactical/IconRail.cs`. It builds its own buttons from a
  `TacticalArtProfile`, so adding or reordering a system is a profile edit, not scene surgery. Call
  `Rebuild()`.
- Metrics from the handoff: 64 wide, 52x52 cells, 5px gaps, 6px padding, 26px inset from the screen
  edge.
- `Canvas/Rails/LeftRail_Tools` — 7 tools, 406 tall. `Canvas/Rails/RightRail_Systems` — 6 systems,
  349 tall. Both vertically centred.
- **System icons carry their own colour and must not be tinted.** The handoff says so explicitly.
  Tool icons are drawn white and do get tinted. That is the `tintIcons` flag.
- Icon size is **34 inside a 52 cell**. At 26 the glyphs read as specks.
- Rail backing plates are RawImages with a cropped `uvRect` onto `steel_plate.png`, not stretched
  Images, so the brushed grain stays at its own scale.

### Step 13 — power controls brought over from the cockpit

- The Power controls were never in `Canvas-Power`. They live in `Assets/Prefabs/Canvas.prefab` under
  `Power_Cont`, and were lost when `Canvas-Interior` was deleted from the scene.
- `Power_Cont/Power/Buttons` (five system toggles plus the reactor dial) and `Power_Cont/Power (2)`
  (the two green plates, Emergency Vent and Black Alert) were reparented into `Canvas/Panels/Power`
  as `Controls` and `Green`. The working render-texture screen was kept between them.
- The panel is now 300x420: green plates on top, screen in the middle, toggles at the bottom. The
  source cluster is 855 wide, so everything is scaled by 300/855.
- **Trap: reparenting a UI subtree collapses it to 0x0 if you do not set `sizeDelta` explicitly.**
  The originals sized themselves from stretch anchors against their old parent. Set the size the
  layout groups expect (855x150 and 855x170), then call `LayoutRebuilder.ForceRebuildLayoutImmediate`,
  then `Canvas.ForceUpdateCanvases()`, then rebuild again.
- The panel grew to 420 tall and collided with the tool rail, so it moved right to clear it.

Final boxes, in screen pixels at 1920x1080. No overlaps.

| Thing | x | y |
|---|---|---|
| ShipStats | 30–330 | 750–1050 |
| Power | 110–410 | 30–450 |
| Weapons | 1590–1890 | 750–1050 |
| Scanner | 1590–1890 | 30–330 |
| LeftRail | 26–90 | 337–743 |
| RightRail | 1830–1894 | 366–715 |

### Step 14 — the board's data layer, and the targeting drawer

The owner asked for two things: the rails must be uniform on both sides, and the weapons panel must
open when its rail button is clicked. The design handoff is explicit on each.

- Handoff section 4: "Geometrically identical to the left rail — same 64px width, same 52px cells,
  same 5px gaps, same centring — mirrored to right:26px. This symmetry matters; the two rails must
  never drift apart in size or rhythm."
- Handoff section 5: the weapons panel is the targeting computer. A slide-out drawer, 376x830 at
  right:104 top:100, that pulls out from the rail over 0.42s on `cubic-bezier(.2,.85,.25,1)`.

**New ScriptableObjects.**

- `RailStyle.cs`, asset at `Assets/Art/Board/RailStyle.asset`. Holds the rail geometry: width 64,
  cell 52, gap 5, padding 6, edge inset 26, icon size 34, button face sprite, plate texture and tint.
  **Both rails read this one asset, so they cannot drift apart. That is the whole point of it.**
- `plateCells` is 7. Both rails get a plate that tall whatever they hold, so a 7-tool rail and a
  6-system rail still read as a mirrored pair. Both are now 64x418.
- `BoardAction.cs`, thirteen assets under `Assets/Art/Board/Actions/`. One per rail button: id,
  label, icon, kind (Tool or System), whether the icon gets tinted, and what clicking it opens
  (panel mode, size, offset, render texture, slide time). **Adding a system is a new asset dropped
  into a rail's list, never a scene edit.**
- `TacticalArtProfile.cs` was cut back to the theme: frames, hull glyphs, and the colour set (plot
  background, grid fine/minor/major, icon tint, icon tint active, frame tint, panel well). The
  per-button icon fields moved onto the `BoardAction` assets, because per-button is the right
  granularity for swapping art.

**New MonoBehaviours.** The rule used: a ScriptableObject holds design data that is true regardless
of scene; a MonoBehaviour holds scene wiring and runtime state. Render textures are assets so a
`BoardAction` can hold one; cameras and canvases are scene objects so it cannot.

- `IconRail.cs` rewritten. Takes a `RailStyle` plus a list of `BoardAction`s, positions itself
  against its own screen edge, and raises a `Clicked` event.
- `DrawerPanel.cs`. Slides a panel in and out, easing on the handoff's `cubic-bezier(.2,.85,.25,1)`
  solved by Newton iteration. Uses `Time.unscaledDeltaTime` so the drawer still works when the board
  is paused. Fades a `CanvasGroup` slightly ahead of the slide and toggles `blocksRaycasts`.
- `BoardController.cs`, on a root `Board` GameObject. Rebuilds both rails in `Start` and subscribes
  before anything can be pressed, then routes clicks. Keeps one drawer open at a time. A button whose
  action has no panel logs and does nothing.

**Scene changes.**

- `Canvas/Drawers/TargetingComputer` added, with a `Panel` child that slides. The order under
  `Canvas` is Panels, Drawers, Rails, so the drawer appears to emerge from behind the rail.
- The `Weapons` corner panel was deleted. That content is the drawer now.
- Verified in play mode: clicking TARGETING slides the drawer open, and a second click closes it.
  Clicking SENSORS or a tool logs and does nothing, as intended.

**Trap worth recording: a solid `Image` is a fill, not a border.** The drawer's frame was one, which
turned the empty lower half into an amber slab. The border is now four thin edge Images under a
`Border` child. The corner panels get away with a solid fill only because their render texture covers
it completely.

### Step 15 — the outer metal frame

Built from the handoff's own spec: Asset Kit section **3.1 BEZEL BUILD-UP** and **3.2 FASTENERS &
SEAMS**.

Three textures were generated into `Assets/Art/Frames/`:

- `frame_metal.png`, 1920x1080. The handoff's 158-degree gradient — `#33383E` at 0 percent, `#23282D`
  at 40 percent, `#181C20` at 100 percent, "cool grey, never warm" — with the brushed grain from the
  handoff's own `figma/steel.png` multiplied over it.
- `rivet.png`, 64px. A radial gradient at 34/30 percent, `#6B747C` out to `#1B1F23` at 72 percent,
  with the lit 1px inner ring.
- `vignette.png`. The handoff's `inset 0 0 160px rgba(0,0,0,.75)` baked as an alpha ramp.

Scene: `Canvas/Frame` as **child 0**, so it sits behind the panels, drawers and rails. It holds a
vignette, four 118px metal bands, five bevel strips and four corner rivets. 118 was chosen so the
rails, which run from 26 to 102, sit **on** the frame rather than beside it.

**The bands are one continuous sheet, not four repeats.** Each band is a RawImage whose `uvRect` maps
to its own slice of the single 1920x1080 texture, so the brushed grain runs round the frame with no
seams and no repetition.

The bevel follows 3.1: a lit top lip on the outer shell, and a plot well that is **recessed with a
dark inner edge** rather than given a plain border. Rivets sit at the four shell corners only — the
handoff says never as decorative runs.

**Three traps worth recording.**

1. **Unity rescales a non-power-of-two texture on import.** `frame_metal.png` came in as 2048x1024,
   which broke the exact 1920x1080 mapping the bands depend on. Set
   `npotScale = TextureImporterNPOTScale.None`.
2. **Setting `textureType = Sprite` alone produces no Sprite subasset.** `spriteImportMode` must also
   be set to `Single`, or `LoadAssetAtPath<Sprite>` returns null with no error.
3. **An asset reimport can bounce the editor back into play mode.** A scene build ran during that and
   was silently discarded. Check `Application.isPlaying` before any scene edit, not just at the start
   of a session.

### Step 16 — frame thinned

The 118px band was too heavy. The frame was rebuilt at **44px**. The rails, which run from 26 to 102,
now overhang the frame and float over the plot the same way the corner panels do. Rivets scaled down
to 9px and moved to sit inside the narrower band.

Everything else about the frame is unchanged — same continuous `uvRect` sheet, same bevel.

### Step 17 — engine power and WASD movement

The goal was small on purpose: get power flowing to engines only, so the ship can move under WASD,
before building anything else.

**Nothing new was written. It all already existed.** The audit trail:

- `PlayerShip.FixedUpdate` at `Assets/Scripts/PlayerShip.cs:517`. W is `:531`, S is `:536`, A and D
  are `Input.GetAxis("Horizontal")` at `:612`. Velocity is locked to `transform.up` at `:596`, so the
  hull never slides sideways.
- The engine power gate is at `:526` — `powerManager.GetSystemEfficiency("engines")`.
- **Pressing 1 toggles engine power.** `PlayerShip.cs:241` calls
  `PowerManager.ToggleSystemState(engines)`.
- All six input axes the ship asks for exist in `ProjectSettings/InputManager.asset`: Horizontal,
  ControllerThrust, LeftTrigger, RightTrigger, DPadX, DPadY.
- `PowerManager.GetSystemByName` lowercases its argument, so the lowercase `"engines"` lookup
  resolves.

**The important behaviour to record: turning is multiplied by engine power.** `PlayerShip.cs:619` is
`targetTurnSpeed = turnInput * turnRate * enginePower`. With no power the ship cannot turn at all,
and thrust is capped at a 5 unit/s crawl (`:553`). **Power is not optional for movement. It gates
both.**

Power fills at 0.5 bars per second, split across every drawing system, so engines alone take about
10 seconds to reach 5 of 5. Each bar costs 0.5 reactor. Reactor reports 8 max here, not the
serialized 15, because `GetMaxReactorPower` scales by the `InternalSubsystems` reactor multiplier.

**A real bug found and fixed: the Rigidbody had no constraints.** `PlayerShip.Awake` at `:84` only
sets `useGravity`, `constraints` and damping **when it creates the Rigidbody itself**. The scene's
ship carries one from the prefab, so it was left completely unconstrained — free to drift in Z and
tumble in X and Y on a flat top-down plot. It is now set on the scene instance to
`FreezePositionZ | FreezeRotationX | FreezeRotationY`, with interpolation on. **Any new ship added to
a Tactical scene needs the same, or it will tumble.**

Camera zoom was temporarily changed from orthographic 30,000 to **600**. At 30,000 the ship drew
**0.26 pixels tall** and was invisible. At 600 it is about 13 pixels. **This is a stopgap.** The plot
layer will draw own ship as a fixed-size glyph, the way the prototype does, so zoom stops affecting
it. The grid looks sparse at this zoom because only the 400-unit fine tier is in range.

`PlayerSettings.runInBackground` was turned **on**. It was off, which meant play mode froze
completely whenever the editor lost focus — `Time.frameCount` stayed at 1 across repeated probes.
**This is why an automated play-mode test appears to do nothing.** Even with it on, driving play mode
over time from outside the editor proved unreliable in this session. Held keys have to be verified by
hand in the editor.

- **Verified:** engine power reaches 5 of 5 with efficiency 1.0, and the ship produces velocity along
  its own nose.
- **Not verified:** W, S, A and D under a real held keypress. That needs a human at the keyboard.

### Step 18 — three bugs on the plot, and the plot camera

**Bug 1: grid line width was in world units, not pixels.** `_LineWidth` was 12 world units. At a
strategic zoom that is invisibly thin. Zoomed in to orthographic 600 it became a 24 pixel amber bar,
and because the fine (400), minor (2,000) and major (10,000) lines all pass through x=0 and y=0, it
read as a bright amber cross through the middle of the plot. That was mistaken for the centre
crosshair, which was already off. **A note recorded earlier in this file claimed `fwidth` held the
stroke to one pixel. That was wrong** — `fwidth` was only feeding the antialias, not the width.
`lineCoverage` now takes a width in **pixels** and multiplies by `fwidth(coord)`, which is world
units per pixel, so the stroke is constant on screen at any zoom. `_LineWidth` is now 1.1 pixels.

**Bug 2: `PlayerShip` was hijacking the plot camera as a chase cam.** `PlayerShip.Start` at
`Assets/Scripts/PlayerShip.cs:133` grabs `Camera.main` whenever its `cameraTransform` field is empty.
`:663-684` then writes ship position plus an offset, **and ship rotation plus a roll offset**,
straight onto that camera every FixedUpdate. On a top-down plot that made the whole grid pivot as the
ship turned. Fixed by giving it a dummy `LegacyChaseCamRig` GameObject to drive instead, and setting
`maxCameraRollOffset` to 0. **Any Tactical scene containing PlayerShip must fill in `cameraTransform`,
or the plot camera gets hijacked.**

**Bug 3: `ScreenConstantScale` compounded itself to nothing.** It measured its base size in `Awake`,
which meant every domain reload took the already-scaled transform as the new base. The first run
produced a scale of 0.12 instead of about 16. `baseScale` and `baseHeight` are now
`[SerializeField, HideInInspector]` and measured exactly once, with a `Remeasure(trueScale)` to start
over deliberately.

**New: `Assets/Scripts/Tactical/PlotCamera.cs`**, on `Main Camera`.

- Scroll wheel zoom between 400 and 30,000, eased rather than snapped, anchored on the cursor so the
  world point under the pointer stays put.
- An optional follow target, currently **off**. The owner wants the camera fixed to the world map,
  not chasing the ship.
- It exposes `ScreenToPlot` and `PlotToScreen`, the screen-to-world bridge the tools and the
  targeting computer both need. **That is the first piece of the plot layer.**

Also in this step: the camera is back to orthographic 30,000 at `(0, 0, -1000)`, rotation identity.
The own ship glyph was raised to 170 pixels at the owner's request. The Scanner (comms/sensor) corner
panel is disabled.

### Step 19 — tuning asset

`Assets/Art/Board/TacticalTuning.asset`, from `Assets/Scripts/Tactical/TacticalTuning.cs`, with
`Assets/Editor/TacticalTuningEditor.cs` providing the buttons and the `Starwolf > Tuning` menu.

It holds the movement, power, stability, plot camera, ship glyph and grid numbers in one place.

**It is not a runtime indirection.** Apply writes the numbers into the scene as real serialized
values. Capture reads the scene back into the asset. Nothing reads the asset at runtime, so what you
see on a component is what the game uses, and there is no hidden lookup to debug.

**Two scene overrides were making movement feel dead** and have been corrected:

| Field | Was | Now | Why it mattered |
|---|---|---|---|
| `turnRate` | 20 | 90 | An 18 second full circle. |
| `rateOfAcceleration` | 5 | 50 | 20 seconds to reach top speed. |

Both were **scene overrides** of the script defaults, not the script values themselves.

### Step 20 — why the power bars never moved

The owner reported the engine bars not filling. **Power was working. The display was not.**

**The audit.** `UIController` has 137 object references in this scene. **104 of them, 76 percent, are
unassigned.** Whole arrays are empty: `imgPowerMet` 6 of 6, `powerNodeImg` 6 of 6, `btnPowerBool`
6 of 6, `btnPowerBoolImage` 5 of 5, `sc` 7 of 7, `canvasUI` 6 of 6, and ten more.

**The blocker.** `UIController.Update` calls `UpdateCompass` at `:242` and `ScanMovment()` at `:259`.
Both threw a NullReferenceException every frame — `compassRect` and `scannerSlider` belong to
`Canvas-Navagation` and the scanner canvas, and the Tactical scenes load neither. An exception aborts
the rest of the method, so `SyncPowerBarsFromManager()` at `:315` **never ran at all.** Four
exceptions a frame, sixty times a second.

**Fixes applied.**

1. Three null guards added to `UIController` — `ScanMovment` (`:427`), `UpdateCompass(float)`
   (`:1033`), `StabilityMeterStart` (`:1138`). Each returns early when the UI it needs is not loaded.
   **These are the only edits made to that file, and they are purely defensive.**
2. `imgPowerMet` and `powerNodeImg` wired, 6 of 6 each, to the real objects inside
   `UI/Canvas-Power`.
3. New `Assets/Scripts/Tactical/PowerMeterBinding.cs`, on a `PowerMeters` child of PlayerShip. It
   drives the meters **straight from `PowerManager`**, reusing the existing `UI_GradeDisplay`
   shader-graph art from the UI prefab, and does not go through `UIController` at all. The shader
   properties are `_PowerCur` and `_PowerMax`. **It instances each material in `Awake`** — otherwise
   every bar shares one material and they all show the same value.

Verified in play mode: with engines at 5 of 5 and arms at 3 of 5, the shader reads `_PowerCur` 5 and
3, and the panel shows five and three lit cells.

**Why a separate binding rather than repairing UIController's path:** UIController's power sync sits
behind two calls that reference UI the Tactical scenes do not have. A direct binding has nothing in
between that can break. UIController still runs and still owns the other panels.

**The general lesson, worth carrying forward:** an exception anywhere early in `UIController.Update`
silently disables everything after it. When a panel in a Tactical scene appears frozen or static,
check the console for a per-frame exception before assuming the underlying system is broken.

### Step 21 — plot layer, part one: camera, zoom range and input

This is the foundation the tools and the targeting computer both sit on. **Nothing owned the plot
before.** There was no way to turn a screen position into a world position, and clicks on the board
went nowhere.

**Zoom range widened a long way.** `PlotCamera` now runs from orthographic **300 to 250,000**, up
from 400 to 30,000. At maximum the view is 500,000 units tall against a roughly 50,000 unit arena,
which is what the owner asked for. Zoom stays cursor-anchored and eased.

**New `Assets/Scripts/Tactical/PlotGridFitter.cs`**, on the `TacticalGrid` quad. The quad was a fixed
140,000 across, so past about orthographic 35,000 the plot simply ran out and went black at the
edges. The fitter tracks the camera and scales the quad to cover the view with a 1.35x margin. At
orthographic 250,000 the quad sizes itself to 1,200,000. **The grid pattern does not move when the
quad does** — the shader computes it from world position, so the quad only decides how much of the
grid exists, never where it is.

Confirmed by render at maximum zoom: only the 10,000-unit major tier survives, about 21 pixels apart.
The finer tiers fade themselves out. That is the shader's fade behaviour working as intended.

**New `Assets/Scripts/Tactical/PlotSurface.cs`**, as child 0 of the Canvas, so it sits behind every
panel, drawer and rail and never steals their clicks. It is an `Image` with zero alpha and
`raycastTarget` on — the standard way to receive pointer events without drawing anything. The graphic
is created in the scene rather than only in `Awake`, so it is inspectable.

It exposes four events, all in **world** coordinates, and knows nothing about tools: `Clicked`,
`Dragged` (start and current), `DragEnded` (start and end), and `Hovered`.

**Panning** is on the middle button, with the right button as an alternative. **Left is deliberately
left free for the tools.** The pan works by dragging the world rather than the camera: it moves the
camera by however far the grabbed world point has slipped from under the cursor, so the point you
grabbed stays under the pointer.

**Verified: the screen-to-world round trip is exact.** Screen centre maps to the camera position, and
all four corners round-trip with 0.000 pixels of error at orthographic 30,000. **`ScreenToPlot` and
`PlotToScreen` are now the single conversion everything else must use. Nothing should write its
own.**

**Trap:** `PlotCamera` caches its `Camera` in `Awake`, so calling `ScreenToPlot` from an editor
script before play mode requires setting that field first.

### Step 22 — own ship glyph: minimum size instead of constant size

The owner asked for a minimum visible size, so the ship cannot become infinitesimally small when
zoomed far out.

**Measured first: it was not actually shrinking.** `ScreenConstantScale` was holding it at exactly
170 pixels at orthographic 1,000, 30,000, 120,000 and 250,000. But the request exposed a real flaw in
the other direction — **at constant size the ship also never grows when you zoom in**, which is wrong
for a real object on a plot.

`ScreenConstantScale` was reworked from constant size to a **floor**:

- `minPixels` (170) — never draw smaller than this.
- `maxPixels` (0 = no ceiling) — never draw larger than this.
- Above the floor it draws at **true world scale**.

Measured behaviour, with the ship 116 world units tall:

| ortho | true size | drawn | behaviour |
|---|---|---|---|
| 300 | 209 px | 209 px | true scale |
| 369 | 170 px | 170 px | crossover |
| 1,000 | 63 px | 170 px | floored |
| 30,000 | 2.1 px | 170 px | floored |
| 250,000 | 0.3 px | 170 px | floored |

So zoom in and the ship grows like a real object. Zoom out and it settles into a glyph. At maximum
zoom true scale would be a third of a pixel, so the floor is doing real work. **This matches how the
prototype handles plot symbols** — they shrink with zoom, but only to a limit.

A `TruePixels` property was also added, so other code can ask how big something would be at true
scale without duplicating the maths.

`TacticalTuning` updated: `shipGlyphPixels` is replaced by `shipMinPixels` (170) and `shipMaxPixels`
(0). `TacticalTuningEditor` maps both. Apply and Capture were round-tripped against the new fields
and verified.

**Note the class name is now slightly misleading.** It gives a minimum size, not a constant one.
Renaming it would break the scene reference, so it stays for now. Recorded here so nobody trusts the
name over the behaviour.

### Step 23 — plot layer, part two: annotation rendering and the seven tools

**Zoom and pan feel, measured first.** Zoom drift is 0.015 world units over twelve notches out, which
is 0 pixels. Pan slip is 0.004 world units. The full 300 to 250,000 range is 34 scroll notches end to
end. Both are effectively exact.

Two feel problems were fixed by inspection rather than measurement:

1. `PlotCamera` now carries `[DefaultExecutionOrder(-100)]`. `PlotGridFitter` and
   `ScreenConstantScale` both read camera state in `LateUpdate`, and with no fixed order they used
   last frame's zoom and lagged a frame. That reads as jitter while scrolling.
2. Zoom used to anchor on the raw `Input.mousePosition`, so it would lurch toward a point hidden
   behind a panel, or toward a stale position once the pointer left the window. `PlotSurface` now
   reports the pointer through `PlotCamera.SetPointer` on move and on exit, and zoom anchors on the
   cursor **only when it is genuinely over the plot**. Otherwise it zooms about the centre.

**New `Assets/Scripts/Tactical/PlotAnnotation.cs`.** One mark: Point, Line, Ruler or Ring. Positions
are stored in **world** coordinates. It carries `Range` and `Bearing`, where bearing is compass
degrees — **0 is +Y north, 90 is +X east** — not atan2's maths convention.

**New `Assets/Scripts/Tactical/PlotOverlay.cs`**, a `MaskableGraphic` that rebuilds one mesh each
frame from world coordinates projected to screen. **World coordinates stored, screen space drawn.**
That is what keeps a mark pinned to the world while its lines stay 1.5 pixels wide instead of
ballooning as you zoom in. One Graphic for every annotation, so the whole layer is a single draw
call. It also owns `Undo`, `Clear` and `EraseNear`, which picks the nearest mark within a pixel
radius using true point-to-segment and point-to-circle distance.

**New `Assets/Scripts/Tactical/PlotTools.cs`.** Five modes — POINT, LINE, RULER, COMPASS, ERASE — and
two immediate actions, UNDO and CLEAR ALL. **The actions never become the active tool.** That was the
trap flagged during planning: treat them as modes and the board gets stuck in a tool that does
nothing. Pressing the live tool again puts it away. Colours follow the handoff: green for
measurement, amber for orders and marks. A drag shorter than four pixels is treated as a click, not a
mark.

Scene: `Canvas/PlotOverlay` added at child index 1, between `PlotSurface` and `Frame`, with
`raycastTarget` **off** so it can never eat a click. `PlotTools` sits on the `Board` object.
`BoardController` now routes any `BoardAction` of kind Tool to `PlotTools.Press(id)` instead of
looking for a drawer.

**Verified in play mode, every tool through the same path a rail click takes:**

- POINT drops a mark and reads out `X 4,000   Y 2,000`.
- RULER north reads `BRG 000`, east `BRG 090`, south-west `RNG 14,142 u   BRG 225` — 14,142 being
  10,000 root two, so the range maths is right.
- COMPASS reads `RADIUS 8,000 u`.
- ERASE removes the nearest mark.
- UNDO and CLEAR leave the active tool untouched.
- Re-pressing the live tool sets it to None.

Confirmed by render: two range rings, a dashed green ruler with end crosses, a solid amber line and
amber point crosses, all crisp.

**Trap: do not name a public MonoBehaviour method `Invoke`.** `PlotTools.Invoke(string)` collided
with `MonoBehaviour.Invoke(string, float)` and every reflection lookup threw an
`AmbiguousMatchException`. It was renamed to `Press`.

---

### Step 24 — Cut to five tools, MOUSE as the default

The owner cut the tool set down, twice, firmly. LINE merged into RULER — a line that does not tell
you its range and bearing is decoration. UNDO and CLEAR ALL were dropped entirely.

**Clear-all is nested in the eraser.** Press ERASE a second time while it is already live and the
plot wipes. It does not get a button.

Five buttons: MOUSE, POINT, RULER, COMPASS, ERASE. MOUSE is the default and means "no tool" — the
plot is something you look at. `line.asset`, `undo.asset` and `clear.asset` were deleted.

`RailStyle.plateCells` set to **6**, the larger rail's count, so both plates stay the same height and
the rails read as a mirrored pair. Both are now 64 x 361.

Active state follows the handoff: a lighter face plus a 3px left bar in the icon's own colour. Built
once per button and shown or hidden by `IconRail.SetActiveId`, driven from `BoardController` off
`PlotTools.ToolChanged`.

### Step 25 — Plot labels

`PlotLabels.cs`. A pool of TextMeshProUGUI objects, repositioned every frame from world coordinates,
so a label stays stuck to its mark through pan and zoom. Text cannot come out of `PlotOverlay`'s mesh
— that is one Graphic drawing lines.

POINT reads `P1  T+0:13`, RULER reads `RNG 34,000 u / BRG 118°`, COMPASS reads `R 18,000 u`.

`MissionClock.cs` added as the one place that answers what time it is. It is `Time.timeSinceLevelLoad`
for now. **When there is a real clock that can pause, only this changes** — otherwise pausing would
drift every timestamp against the clock the player sees.

**Two traps.** `RectTransformUtility.ScreenPointToLocalPointInRectangle` with a null camera silently
returns zero on a Screen Space - Camera canvas, and every label stacks in the middle — pass
`canvas.worldCamera`. And a point and a ruler end can share a coordinate exactly, so labels now stack
rather than printing over each other.

### Step 26 — Ship readout

`ShipReadout.cs`. One TMP line at the top of the board: `SPD 5 u/s    HDG 062°`.

**The UIController speedometer is dead and was not reused.** `UpdateSpeedometer` has its only line
commented out, and `velocityMeterImg` points into the interior canvas these scenes do not load.
`PlayerShip.GetCurrentSpeed()` already exists, so one label reading it was cheaper than extracting a
meter that never worked.

Heading is compass degrees off `transform.eulerAngles.z` — Unity's 298 is heading 062.


### Step 27 — The intro and menu, wired into the flow

The owner asked for one chain: SECTION 36, then STARWOLF, both on their own with no keypress, then a
menu, then NEW GAME to the beta message, then Enter to the board.

**Almost none of this needed new code.** `Assets/StarwolfIntro/` already held the whole sequence —
`StarwolfIntroController` plays SECTION 36 → INITIALISING... → STARWOLF → a pull-back → a menu of
NEW GAME / LOAD GAME / SETTINGS, on its own clock, and exposes a `newGame` UnityEvent. So the work
was wiring, not building. **No new script was written for this step.**

What went into `00_Start`:

- `STARWOLF Intro`, a root object with `StarwolfIntroController` on it, with `glyphData`,
  `displayShader` and `hardwareHum` assigned from `Assets/StarwolfIntro/`.
- `newGame` wired to the existing `ScreenFlow.Advance()`, which already pointed at `01_Briefing`.
- `loadGameAvailable` off. There is no save system, so LOAD GAME draws dimmed and is skipped when
  you arrow through the menu.
- An `AudioListener` on `Main Camera`. There was none, so the hardware hum had nothing to play into.
- `Title` and `StartButton` **deactivated, not deleted.** The intro draws its own title and menu over
  the top. They are left in the scene so the old screen can come back if the intro is dropped.
  Delete them once the owner is happy.

The chain was walked in play mode: intro → menu → NEW GAME → `01_Briefing` → `02_Grid`.

**Pacing.** The package shipped at 2.5 / 1.2 / 2.0 / 3.0 seconds, 8.7 in total, which the owner found
too fast. Now 4 / 2 / 4 / 4, 14 in total. These are inspector fields on `STARWOLF Intro`.

#### Four rendering bugs in the intro package

The package was written against a Gamma project. This one is Linear, and it showed.

1. **Double gamma encode.** The render target was sRGB, so the blit encoded on write and the
   backbuffer encoded again. The `#010601` CRT field measured `(63,113,63)` on screen — mid green.
   Fixed by making the target linear. It is now `ARGBHalf`, not `ARGB32`, because the unlit field
   sits a few thousandths above black and eight bits cannot hold that.
2. **Grain on the black field.** `grain * .003` is below what eight bits can represent near black, so
   it quantised up to a visible green on *every unlit pixel*. That, not the background colour, was
   most of the green. Grain now modulates the lit phosphor instead.
3. **Shader uniforms dropped on reload.** `_Pitch`, `_Phosphor`, `_Background` and the six mask sizes
   were never declared in the shader's `Properties` block. Any shader reload dropped them, the mask
   sizes came back zero, `mask()` divided by zero and **the whole intro drew pure black.** This is
   why SECTION 36 looked missing. They are now declared *and* re-applied every frame by
   `ApplyStaticUniforms`. Declaring alone is not enough — a reload resets a declared property to its
   default, and a default mask size of zero is just as fatal.
4. **The grid washing the screen.** The package draws a full-screen phosphor grid at a 9px pitch. The
   pull-back zooms it 5x, which puts the cells below one pixel, and it aliased into a flat green
   field. Two changes: each tier now fades out as its lines close to within a pixel, and the grid
   only appears during the pull-back, swelling and going again.

#### Why it still does not look like the reference

`References/Company Logo.png` is the canonical image. Measured against our render:

| | reference | ours |
|---|---|---|
| phosphor peak | `(214,255,229)` | `(213,255,212)` |
| unlit field | `(0,19,1)` | `(0,0,0)` before Step 27's CRT work |

**The colour was never the real difference.** The reference is not a logo, it is a whole terminal:
CRT bezel, screen curvature, strong scanlines, a vignette, a faintly lit field, and four corners of
microtext (`UC-FLEET`, `// CLASSIFIED`, `S38-PR-001 / REV A / 1993.11.04`,
`RESEARCH / DEVELOPMENT / OPERATIONS`, `CLEARANCE LEVEL 3` with five boxes, three lit).

The package renders only the core mark. That is defensible — `References/LogoREADME.md` §7 separates
the "core studio mark" from "terminal presentation" and says the bezel, microtext, revision codes,
clearance levels, scanlines and curvature are **not** part of the permanent logo. But it is why the
two look nothing alike side by side.

Added in this step: a lit field, scanlines across the whole screen, a stronger vignette, and a
rounded screen edge, as four tunable shader properties — `_ScanDepth`, `_FieldGlow`, `_Vignette`,
`_ScreenInset`. **This is tone only. No geometry is warped**, deliberately, because `HandleMenu`
hit-tests the menu against fixed rectangles in 1920x1080 space and warping the picture would move
the text away from its own hit boxes.

**Not yet seen on screen.** The Unity MCP bridge dropped before this could be checked. Verify the
CRT treatment and the menu-on-black change before trusting either.

#### Traps

- **`execute_code` and any asset refresh reload shaders, which used to blank the intro.** Bug 3
  above. Fixed now, but it is why so many screenshots in this step came back black.
- **A tool round trip takes roughly fifteen seconds.** You cannot screenshot a beat that lasts four.
  To photograph one, set its duration to 120 at runtime and call `RestartIntro()`.
- **Do not hand-author new glyphs into `glyphs.json`.** The microtext needs `U P V H / - 0 1 4 8 9`,
  none of which exist. `LogoREADME.md` §5 says the glyph tables are canonical and must match the web
  implementation exactly. §11 allows TextMeshPro for terminal metadata — use that, not new glyphs.
- The intro package sits in `Starwolf.Intro`, a namespace. The rest of the project has none. It is
  vendored, owner-supplied code. Left as it is on purpose; do not restyle it.

### Step 28 — A new scene for the new handoff

The owner chose the new Claude Design handoff (`Assets/UI2/Claude Design handoff/export/`) as the
board to build. It was read in full first: README, asset map, tokens, layout, state spec and logic.

`02.1_Grid.unity` was made with a camera, an EventSystem and a `Board` canvas set to the handoff's
numbers. It was added to Build Settings after `02_Grid`. The briefing's `ScreenFlow.nextScene` was
changed from `02_Grid` to `02.1_Grid` — one line in `01_Briefing`.

Checked in play mode: `ScreenFlow.Advance()` in `01_Briefing` lands in `02.1_Grid`, no console
errors.

Steps 29 to 33 are written up in `UI_INTEGRATION_LOG.md`.

### Step 34 — Wiring begins: the dummy enemy exists and the board reads it

**Owner direction (2026-10-17).** Start wiring real game systems, one feature at a time. Plan each
feature with the owner. Don't invent: follow the sources. Each feature must work before the next
starts. The North Star is a dummy enemy at a fixed position. It doesn't move or fire. We use it to
check the radar, moving around it, and shooting at it.

**Owner decisions for this step.**

- The dummy sits at TRACK 07's spot: bearing 048, 24,300 km.
- Its plot dot shows only once identified. That is the board's rule.
- The other four mock contacts (CONTACT 17, CONTACT 22, FTR-2, BC-01) are removed. Their plot art is
  hidden, not deleted.
- A new World object owns the game state and feeds the board. The old `Radar.cs`, `EnemyShip`,
  `PlayerShip` and `PowerManager` scripts are not revived.
- 1 unit = 1 km, already ruled (decision 13).
- Keep real 3D objects seen top-down, for future external viewport cameras.

**What was built.**

- `TacticalWorld.cs` on a root `World` object, outside the Board canvas. Children: `OwnShip` at
  (0, 0, 0) and `Dummy · TRACK 07` at (18058.42, 16259.87, 0).
  - It uses the x/y plane, x east, y north. This matches the old build's 3D plane (rotation about z,
    +y forward).
  - Each tick it writes true bearing (whole degrees, `JsRound`) and range (whole km) into
    `HandoffBoardState.tracks`.
- `WorldContact.cs`, which holds a `trackKey`.
- `HandoffBoardControls.Ticked`: fires once per board tick, after the state ticks, before the panels
  redraw. The world steps inside the board's single timer.
- `HandoffBoardState.KeepTracks` drops tracks with no world contact, and prunes identified and
  classified.
- `HandoffPlotPanel` draws identified hostile contacts at their true km position. For now that is a
  red dot plus a `NAME / RNG n km` label.
- `HandoffBoardBuilder`:
  - new `Contacts (drawn)` and `Contact labels` layers;
  - hides the mock contact art (26 objects);
  - `RelinkWorld` re-points the World at the new board after every rebuild. The first check caught
    the link being dropped.
- 3D models as `Model` children. OwnShip uses `StarWolf_Destroyer_PlayerShip.fbx`, the same model and
  placement as the old `Prefabs/PlayerShip.prefab`. TRACK 07 uses `StarWolf_Cruiser.fbx`. Scale
  0.11612, so about 12–14 km long at 1 unit = 1 km. Their real scale is to be decided when viewport
  cameras come. Models only, no old scripts.

**Verified.**

- Only t07 is tracked.
- Bearing and range through real ticks: 048 / 24,300 at load, and 000 / 090 / 180 / 270 at the four
  compass points.
- Contact board: unidentified at SEN gain 2, `TRACK 07 · 048 · 24k` after the real cone identify,
  gone with SEN empty.
- The dot is exact to 0.0001 px at zoom 1, 0.5 and 0.1, and clipped to the plot.
- The RULER from our ship to the dot reads `RNG 24,300 km · BRG 048°`.
- No mock contacts anywhere.
- The board outside the plot is pixel-identical to before.
- Fire control TRACK and periscope SLEW use the real numbers.
- One control per drawer works.
- The World survives two rebuilds, still linked.
- Console clean.
- Evidence is in `Logs/HandoffQA/step34/`. `Logs/` is git-ignored and survives Unity restarts;
  `Temp/` does not.

**How to see it in play.**

1. Power: SEN to DRAW until 2 units.
2. Sensors: gain to 2.
3. Active scan on, cone to 048, release.
4. Zoom the plot to about ×0.4. The red TRACK 07 dot is top right.

### Step 35 — Power makes the ship loud (signal), and nothing real changes while paused

**Owner decisions (2026-10-17).**

- Every unit drawn to a bus makes us louder, and so does ACTIVE SCAN. Sources: GDD p18, spec
  3.1.b, and the prototype's `getSig`. The design board only counted ACTIVE SCAN; the owner
  overrode that.
- The internal value is called **signal**, on a 0–100.0 scale.
  - Each action adds a tweakable amount. Every amount is an Inspector field.
  - It never jumps straight to 100.
  - The decimals are internal only. The player sees a whole percent on the OWN EMISSIONS meter,
    the 10 cells (white, then amber, then red), the blinking lamp and the alert line.
- The meter climbs and falls gradually.
- Freeze while paused: nothing real changes until RUN. Controls still work.

**What was built.**

- `HandoffBoardState`:
  - `signal`, renamed from `ownSig`. `FormerlySerializedAs` keeps saved values.
  - `signalRise` 9 and `signalFall` 6 per tick.
  - `signalTrackedAt` 45.
  - `Bus.signalPerUnit`, default 10 per bus, so a full 10-unit reactor reads 100.
  - `activeScanSignal` 35 (the reference sim's 3.5 units).
  - `SignalTarget` = clamp(Σ fill × signalPerUnit + scan, 0, 100). `TickSensors` moves toward it
    with no overshoot.
- The sensors note:
  - "RUNNING SILENT" at 0.
  - "EMISSIONS DECAYING — NOT YET RESOLVED" while falling.
  - "POWER SIGNATURE — NOT YET RESOLVED" while rising or steady below 45. This is a stand-in: the
    design has no line for it (`HANDOFF_GAPS.md` row 106).
  - "N HOSTILE SET(S) HAVE YOUR RANGE" at 45 or more.
- `HandoffBoardControls`: the whole tick waits while paused. That includes the world's `Ticked`
  event and the mission clock. The design ran its power tick while paused; the owner overrode that.

**Verified.** STEP 35: VERIFIED, first pass.

- 54 ticks matched an independent model exactly: fill mixes, scan, LOAD SHED, the reactor cap of
  4, and the clamps.
- Tunable values take effect on the next tick.
- The meter, note and blink states are correct.
- Pause froze everything for about 25 s: fills, charges, signal, heading, party, launch, clock and
  the world sync. Everything resumed on RUN.
- Step 34's checks, sensors, fire control and the drawers are unchanged.
- Console clean.
- Evidence is in `Logs/HandoffQA/step35/`.

**Known.**

- "1 HOSTILE SET HAS YOUR RANGE" counts every hostile once we are loud, even far away. The
  enemy-notices-you feature (Power P3, parked) fixes that.
- The board loads PAUSED, so nothing fills until the player presses RUN.
- The reactor serves buses first come, first served. A bus sent to DRAW late can find it already
  spent.

**Playtest grown.** `Assets/Editor/Playtest/PlaytestRunner.cs` now also checks that:

- the board starts paused;
- buses set to DRAW don't fill while paused;
- RUN starts time;
- the signal climbs to its target and reads "1 HOSTILE SET HAS YOUR RANGE";
- LOAD SHED vents everything back to "RUNNING SILENT".

It runs 27/27 checks with a 35 s video. Two playtest fixes went in:

- A control with no argument is saved as "", not null.
- All buses are now set to DRAW while paused, so the reactor shares out evenly.

### Step 36 — Plot tools: marks stay put in space

**Owner decisions (2026-10-17).**

- **POINT** is a timestamp. It stays where it was dropped and shows the mission time at the drop.
- **RULER** draws a distance, typically between two points.
- **COMPASS** sits around our own ship.
- Marks stay put in space. They do not move with our ship.
- Plots feed fire control later. That belongs to the fire-control step.

**What was built.**

- `HandoffBoardState`:
  - Marks store world km (`x, y, x2, y2`), not screen px.
  - `ownX` / `ownY`, our ship's world position, written by `TacticalWorld` every tick.
  - `MarkDown(tool, x, y, missionT)`. A COMPASS always starts at our ship.
  - `MarkUp(minLenKm)` keeps a mark of exactly the minimum length (a tiny tolerance).
- `HandoffPlotPanel` turns km into screen px through the zoom, both ways. A POINT label is its
  mission time. A RULER label is its range and bearing. A COMPASS ring is re-measured from our
  ship.
- Contact dots are drawn at the contact's exact world position, not its rounded bearing and range.
- The first check found three defects, all fixed: a ruler of exactly the minimum length was
  dropped; the contact dot used rounded numbers; the ruler label's rounding. The second check found
  the label still 1 px off on exact half pixels (km to px leaves float dust). It now trims to 6
  decimals before rounding.

**Verified.** STEP 36: VERIFIED on the third pass (2026-10-18).

- 2,016 rulers and 672 compass rings over 4 ship positions and 3 zooms: none wrong. Rulers of
  exactly 4 px are kept.
- A replay of 914 design-board steps matched label text, shapes and hit boxes. No ruler label was
  off (935 were before the last fix).
- The contact dot sits within 0.0001 px of its true world spot.
- Marks stay put when the ship moves; the COMPASS re-measures.
- Evidence: `Logs/HandoffQA/step37/v/` and `Logs/HandoffQA/step38/`.

**Playtest grown.** It now drops a POINT, a RULER and a COMPASS, moves our ship 5,000 km east
(a stand-in for the helm), and checks the POINT and RULER stay put in space with their labels
unchanged while the COMPASS is re-measured. ERASE clears them. 37/37 PASS.

**Known, for the helm step.**

- The grid does not scroll with the world yet.
- While moving, marks step at the board tick (2.4 a second), not smoothly.
- The COMPASS erase area is a square.

### Step 37 — Our ship on the plot, as the 3D model seen from above

**Owner direction (2026-10-17).** Our ship must appear on the plot. It is the 3D model seen from
above. Its size is true to scale, with a minimum and a maximum so it never vanishes or swamps the
plot. The design handoff already specified the glyph (34 × 76 hull, turned to heading, shrinks
with zoom). It had been left as placeholder rectangles. That was our miss.

**What was built.**

- `ShipView.cs` on a new `Ship View Camera`. It looks straight down at `OwnShip`, north up, and
  renders only the `WorldShips` layer into a 256 px texture. It measures the ship once, so the
  ship fills 80% of the frame.
- `OwnShip` turns to the board's heading every tick (`TacticalWorld`).
- A `Ship Light`, lighting only `WorldShips`.
- `Own ship (3D view)`, a RawImage at the plot centre. Size: 76 px at zoom 1, scaled by zoom,
  held between 30 and 120 px.
- `BoardLinearImage.shader` and its material. The board works in sRGB values, so the shader
  converts the camera image first. The VectorModel pass draws lines on black, so it makes black
  see-through.
- The builder hides the old placeholder rectangles and adds the image, switched off. It turns on in
  play once the ship view has a picture. An image with no picture draws a white square.
- Both ship models are turned 180° about their length. The camera looks from below the world
  plane, so unturned we saw the belly.
- The bow is the wide hammerhead end (turrets point that way; the Art Spec says "hammerhead
  prow"). It leads along the heading.

**Verified.** STEP 37: VERIFIED on the second pass (2026-10-18).

- Red line art at the plot centre, no square, in play. Nothing in edit mode.
- Size matches clamp(76 × zoom, 30, 120) at every zoom tried.
- The top side shows, and the bow leads at six headings, within 0.5°. The ship holds still while
  paused.
- Marks and the contact dot draw over it; the periscope covers it; nothing leaks out of the plot.
- It survives a rebuild.
- Evidence: `Logs/HandoffQA/step37/v/` and `Logs/HandoffQA/step38/`.

**Our ship is white** (owner, 2026-10-18). The camera image is drawn by the old build's VectorModel
full-screen effect as red lines on black, and red means "hostile" in the design palette. The effect
itself stays red: other cameras use it. `BoardLinearImage.shader` instead uses the lines as a mask
and fills them with the image's colour. That colour is `HandoffPlotPanel.ownShipColor`, in the
tuning window under Plot & our ship. Its default is the design's white, `ink.data` #E8EDF0, the
colour of the design's own-ship glyph.

### Step 38 — Every tunable value in one window

**Owner direction (2026-10-17).** Every tunable value lives in the Inspector, gathered in one
place, so the owner doesn't have to hunt through objects. A web app or spreadsheet was raised; one
window inside Unity was chosen: nothing to sync, and it edits the real values.

**What was built.**

- `TuneAttribute.cs`: `[Tune("Section")]` marks a value as tunable.
- `Assets/Editor/StarwolfTuningWindow.cs`, menu **Starwolf → Tuning**. It lists every tagged value
  on the open scene's components, including inside lists (each bus's signal per unit), grouped by
  section. Edits go to the real values and can be undone.
- In play mode it warns that changes are lost when play stops. Tick **Keep play-mode changes** to
  copy them back afterwards. Then save the scene.
- 49 declarations tagged, 65 values shown, in nine sections: Power & signal, Sensors, Helm, Plot
  tools, Plot & our ship, Periscope & fire control, Engineering & orders, Board & time, Motion.

**Rule from now on:** tag every new tunable value with `[Tune("Section")]`.

**Verified.** STEP 38 checked on 2026-10-18.

- 65 values; every one resolves. Labels are unique within each section. Each tag shows exactly once.
- Edits change the real value and undo cleanly.
- In play: without the tick, changes go back when play stops. With it, they are kept. A change
  takes effect on the next board tick.
- The checker found one defect: with the window open, a playtest put two layout errors in the
  console. The window decided what to draw separately in Unity's two drawing passes. It now
  decides once per frame. Re-checked by running the playtest with the window open: 37/37 PASS,
  console clean.

**Known.** `ShipView`'s three values are read when play starts. Changing them mid-play does
nothing until the next run.

### Step 39 — Nothing standing on the plot

**Owner decision (2026-10-18).** The plot carries no standing text or mock art. "It's too much."
The player learns these things through the tools.

- **Gone for good:**
  - our ship's name and call sign (STARWOLF, SW-01);
  - the mock heading and speed line under it (HDG 062°, V 1.84 km/s);
  - the SENSOR RANGE label on the envelope ring (the ring stays);
  - BOARD POINT ALPHA and its ×, a mock waypoint;
  - `drawn#33`, an empty circle that drew nothing;
  - `STAND-IN drawn#35`, a frozen grey line pointing north-west. It is probably part of the
    design's radar sweep: it has the same −58° angle as the sweep disc `drawn#34`, already hidden
    (`HANDOFF_GAPS.md` rows 7, 12, 105). The design's sweep turns round our ship while DRADIS
    (passive sweep) is on, which is the default. It is built for real in the sensors step.
- **Hidden until real:**
  - The PDC GROUP A arc and its label show only once the PDCs are activated. They are wired in
    the fire-control step.
  - The alarm strip ("3 NEW CONTACTS SINCE COMMIT") shows only for a real alert. The board loads
    quiet, as if already acknowledged. The sensors step gives it a real trigger: a new contact
    identified.
- The builder hides them (`WirePlot`, and the alarm strip in the time-control wiring), so a
  rebuild keeps them hidden.

**Checked.** After a rebuild, nothing under `Plot/Mockup` or `AlarmStrip` shows. The World is
still linked and the scene saved clean. Playtest: 37/37 PASS, console clean, our ship white (`Logs/Playtests/2026-10-18_225433/`).

### Step 40 — Press and click: switches slide, refuse, and click

**Owner decisions (2026-10-18).** Bring in the design's press and click interactions; the handoff
is the newest source.

- Rocker switches slide their pad, built now from the cut-out art.
- A locked switch that is clicked gives a small "refused" nudge. This is the owner's addition; the
  design has nothing for it.
- Click sounds, from the prototype. The design board has no sound.
- Unity's audio was switched off for the whole project (since August 2025, no reason recorded).
  It is now on. The intro's hum is now audible too.
- A refused order keeps the design's own reaction (it throws, the lamp goes red for 0.42 s, it
  flicks back), plus the refused sound.

**What was built.**

- `HandoffRockers.cs` on the board, covering all 21 clickable rockers in seven drawers.
  - When a panel swaps a rocker's sprite, a cover the colour of the well (`sprite.well.01`) hides
    the baked pad. Two cut-outs of the old and new pads slide over it, and the new one fades in.
    The slide uses the design's spring curve, cubic-bezier(.3, 1.5, .5, 1), and the fade uses
    ease. A slide lasts 0.14 s on the 30×38 and 24×32 rockers and 0.12 s on the 22×30 and REBOOT
    (UIStateSpec §9.1).
  - At rest the three layers are off, so the board looks exactly as before. The edit-mode board
    renders pixel-identical before and after the rebuild.
  - A change made while a drawer is shut isn't animated when it opens.
  - Pad boxes and travel were measured in the art. The thrown pad carries a glow 2 px wider, so
    the cut-outs include it. The 30×38 thrown pad sits 11 px lower; the spec says 10 (top 3 → 13).
    The art wins, so the slide lands exactly on the baked pad.
  - Refused nudge: if the rocker shows its locked art when clicked, the pad dips 2 px and springs
    back over 0.12 s.
- Click sounds, made in code from the prototype's `playTone` (no audio files):
  - tab (560 Hz square, 35 ms): tool rail, systems keys and latches, helm, orders and periscope
    headers;
  - switch (310 Hz square, 45 ms): every other control that takes a click;
  - refused (150 Hz sawtooth, 80 ms): locked rockers and refused orders;
  - drag-only controls and drawing on the plot make no sound.
- TRACK (fire control) and its lamp drop 1 px while held. The design gives no ease, so it's
  instant.
- RECALL (hangar) eases into its pressed art over 0.1 s and back.
- Tuning window: Motion gains the four slide times, the nudge distance and time, the RECALL
  press, and the TRACK drop. A new **Sound** section holds the three cues: pitch, length,
  volume, and square or saw.

**Checked.**

- The edit-mode board renders pixel-identical before and after the rebuild (`Logs/HandoffQA/step40/`).
- In play, the LOAD SHED pad was logged frame by frame (`slide_frames.txt`): 17 → 28.9 px with the
  spring's small overshoot, then rest at 28, over 0.13 s. The new pad fades in alongside.
- Playtest: 48/48 PASS, console clean (`Logs/Playtests/2026-10-18_234125/`). New checks:
  - the tab and switch clicks;
  - the LOAD SHED slide;
  - drawing on the plot makes no sound;
  - a locked arming rocker nudges with the refused sound and stays locked;
  - TRACK drops 1 px and returns;
  - RECALL eases down and back.

**Known.**

- The cover is one flat colour. The real well has a slight gradient and the thrown pad's red
  glow. It only shows in the strip the pad uncovers during a 0.12–0.14 s slide.
- RECALL's press is a crossfade between the up and pressed art. The design moves the button's
  top 3 → 7 px; the pressed art already has the button lower, so it reads as a press.
- Tuning window: "Keep play-mode changes" does not keep edits to the three sounds or the RECALL
  press. It only keeps single numbers, colours and switches.

### Step 41 — The helm: our ship moves, and the plot pans

**Owner decisions (2026-10-18).**

- Top speed is on the design's scale: about 2.5 km/s at full throttle. The design's contacts run
  0.4–2.6 km/s. Reaching the dummy, 24,300 km away, takes about 7 minutes at ×10.
- The grid stays put and our ship moves across it. The design kept our ship at the plot centre;
  the owner overrode that.
- You can drag across the grid to pan the plot, and use the arrow keys.

**What was built.**

- `HandoffBoardState.TickMove`, run every board tick for `rate` mission seconds (so ×10 moves ten
  times as far per tick). Spec §4:
  - the throttle sets a target speed along the nose, and the ship speeds up and slows down at
    capped rates;
  - the hull turns, but the ship keeps sliding on its old course while the sideways speed bleeds
    off.
  - The numbers are the prototype's, slowed to the owner's top speed: time stretches by
    240 / 2.5 = 96. That gives 0.9 G accelerating and 1.7 G braking, which matches the design
    helm's "1.0 G". Full speed takes about 5 minutes of mission time.
  - All five values are in the tuning window under Helm.
  - Throttle is still held to what ENG power allows, as before. There is no reverse; the board's
    throttle runs 0–100%.
- The board now owns our ship's position. `TacticalWorld` reads the model's start position once,
  then places the model where the board has it every tick. Bearings and ranges to contacts use
  that position.
- `HandoffPlotPanel` has a view centre, which starts on our ship. Marks, contacts and the grid are
  drawn around it. Our ship's image sits where the ship actually is.
  - Dragging the empty grid pans the plot. The plot ground's hit is now `plot.pan`; the mouse wheel
    still zooms there.
  - The marking tools keep their drags for drawing.
  - Holding an arrow key pans at 600 screen px a second (tunable), except while typing in a
    field.
- `HandoffPlotGrid.OffsetPx` shifts the grid lines, so the grid stays fixed in the world.

**Checked.** Playtest 57/57 PASS, console clean (`Logs/Playtests/2026-10-19_000009/`). New checks:

- Putting our ship 5,000 km east moves its image 304.5 px across the plot; marks don't move on
  screen, and the COMPASS re-measures.
- With ENG powered and the throttle at 50%, the ship gathers speed (0.77 km/s after 4 s at ×10)
  and moves along its heading.
- After a 90° turn the course lags the heading (heading 173°, course 140°).
- Paused, the ship stops dead.
- Dragging the grid 100 px pans the view by exactly 100 px worth of km.

**Known.**

- The playtest sets the ordered heading directly; the helm dial is a drag it doesn't do yet.
  Arrow-key panning can't be pressed by a script (the old Input class), so it is untested.
- Turning still runs at the design's 4° per board tick at every time rate, while movement scales
  with the rate. So at ×10 the ship slides further during a turn.
- The axis numbers on the plot's edge (−30 … +30) are fixed. They read as distance from the plot
  centre, not world position.
- Nothing recentres the plot on our ship. Panning is the only way to follow it.

### Step 42 — The sensor circle: DRADIS's hearing range around our ship

**Owner direction (2026-10-18).** When DRADIS is on, a circle shows around our ship. Its radius is
the sensor range.

**What happened before.** The design's sensor ring was `drawn#33`, a 398 px circle round the plot
centre: 150 + gain × 48 layer px, the radius the SENSOR RANGE label rode on. The export gave it no
fill and no line, so it never drew in Unity. It was removed in Step 39 as "an empty circle"; nothing
visible was lost.

**What was built.**

- A `Sensor ring (drawn)` layer under our ship (a `HandoffMarksGraphic`, which gains a solid
  `circle` and a filled `disc`).
- `HandoffPlotPanel` draws it around our ship's true position at the true hearing range,
  `PassiveRangeKm`: the range at which `Heard()` takes a contact while DRADIS is on. That is
  12,000 km + 7,000 km per gain step, ×0.55 on UV. So 19,000 km at gain 1.
- It is drawn only while DRADIS is on.
- The colours are the design's `senEdge` and `senGlow`: the edge in data white at 26% and a faint
  fill at 4%. On the amber grid, alarm red at 30% and 5%.
- It eases to a new size over 0.5 s when the gain changes (UIStateSpec §9.1, lines 204 and 210).
  The ease is in the tuning window under Sensors.
- The radius is true to scale, not the design's fixed px. At ×1 a 19,000 km circle is wider than
  the plot, so zoom out to see it (×0.3 fits it).

**Checked.** Playtest 60/60 PASS, console clean (`Logs/Playtests/2026-10-19_001410/`). The circle matches the hearing range with DRADIS on, goes with
DRADIS off, and comes back. A screenshot at ×0.3 shows it centred on our ship.

### Step 43 — The radar sweep

**What the design has.** `rd-sweep`: while DRADIS is on, a sweep turns once every
(5.4 − 0.5 × gain) seconds (UIStateSpec §9.2; board-logic.js `sweepDur`). It has a bright leading
edge (`sweepEdge`, data white at 70%) and a faint tail (`sweepTail`, 9%). On the amber grid they
are red at 75% and 11%. The export only had a frozen gradient stand-in for it (`drawn#34`, hidden;
`drawn#35`, removed in Step 39).

**What was built.**

- `HandoffSweepGraphic.cs`: draws the edge, a line from our ship to the rim pointing north, and
  the tail, a fan fading to nothing 60° behind it. It is drawn once; turning it costs nothing.
- `HandoffPlotPanel` sizes it to the sensor circle and places it on our ship. It turns it
  clockwise at the design's rate, and hides it while DRADIS is off.
- It holds still while the board is paused. The design kept it turning; the owner's rule is that
  nothing moves while paused.
- Tuning window, Sensors section: the tail angle (the design gives none) and the turn time.
- A `Sensor sweep (drawn)` layer between the sensor circle and our ship (builder).

**Checked.** Playtest 62/62 PASS, console clean (`Logs/Playtests/2026-10-23_000343/`). The sweep turned 39° in
0.5 s at gain 1; the design rate gives 37°. It held still while paused and went with DRADIS off. The
×0.3 screenshot shows the edge and its fading tail inside the circle.

**Known.** On the amber grid the tail uses the edge's colour (rgb 255, 150, 130). The design's tail
there is a deeper red (224, 75, 60), at 11%.

### Step 44 — Hearing the enemy: bearing lines, the alert strip, the tracked blink

**Owner direction.** Alerts appear only when there is a real alert (2026-10-18). This step also
delivers Power P2, which was parked here: hearing on the plot.

**What was built.**

- **Bearing lines.** Every contact DRADIS hears but hasn't identified gets a line fading out from
  our ship along its bearing, to the sensor circle's edge, with a "BRG 047" label. A passive
  contact gives a bearing only, no range.
  - Lines and labels are data white at 60%. While the periscope is on the contact, the line is at
    100% and the label turns amber, as the design's `passives` do.
  - Clicking the label still swings the periscope onto the contact (`plot.look`).
  - The label sits 200 px out along the line, or at the circle if that is nearer (tunable). The
    line is a new `fade` kind in `HandoffMarksGraphic`, on a `Passive bearings (drawn)` layer.
  - The export's only passive label (BRG 149, for the removed CONTACT 22) is now the template
    for every heard contact's label.
- **The alert strip.** The board counts each contact once, the first time DRADIS hears it or a
  scan identifies it (`HandoffBoardState.NoteNewContacts`, run every tick). A new one raises the
  strip and sets its count: "1 NEW CONTACT SINCE COMMIT", plural for more. It also plays the
  prototype's alert tone, 170 Hz sawtooth for 0.42 s, tunable under Sound. Clicking the strip
  acknowledges it and clears the count. The design raised the strip once at load and never
  again; this trigger is ours, after the owner's direction.
- **The tracked blink.** While our signal is at or above the tracked line (45%), our ship blinks:
  full for the first half of each 1.1 s, then at 20% (the design's `bd-blink` on its own-ship
  glyph).

**Checked.** Playtest 68/68 PASS, console clean (`Logs/Playtests/2026-10-23_000923/`).

- At load nothing is heard and the strip is quiet.
- With SEN powered and gain 2, the hearing range is 26,000 km and DRADIS hears TRACK 07 at
  24,127 km.
- Its line and label read "BRG 047".
- The strip raised "1 NEW CONTACT SINCE COMMIT" with the alert tone. Clicking it cleared it.
- At 100% signal our ship blinked.

**Known.**

- The playtest sets the sensor gain directly; the gain fader is a drag it doesn't do yet.
- The enemy noticing us (Power P3) is still to come. It is the first-mission plan's step B
  (`FTUE_PLAN.md`).

### Step 45 — The bearing trail: plotting a passive contact's range

**Owner decisions (2026-10-23).** The plot tools exist to build an accurate firing solution by hand
(Silent Shark style). A passive contact gives a bearing only. To find its range:

- each sweep leaves a bearing line where our ship was;
- the lines fade out over **2 minutes** of mission time;
- as we move, they cross at the target.

The design's buttons, drawers and flows are unchanged; this is drawing on the plot only.

**What was built.**

- `HandoffBoardState.BearingFixes`: one fix each time the radar sweep passes a contact DRADIS hears
  but hasn't identified. A fix holds our position, the exact bearing (not rounded) and the
  mission time. `bearingTrailSeconds` (120) is under Sensors in the tuning window. Old fixes are
  pruned.
- The plot draws each fix as a line fading out from where we were, out to the hearing range. Its
  strength fades with age; a fresh line is at 45% (tunable).
- The live bearing line now uses the exact bearing too. Its label still shows whole degrees.
- Pausing freezes the sweep, so nothing new is laid. Pressing ERASE twice clears the trail along
  with the marks.

**Checked.** Playtest 70/70 PASS, console clean (`Logs/Playtests/2026-10-23_005602/`). At ×10 two
lines laid 140 km apart crossed 0.0 km from TRACK 07. The first line was gone 140 s later.

**Known.** At 24,000 km a short baseline gives near-parallel lines. The player has to cover distance
or turn across the bearing to get a clear cross. Bearings are exact: no sensor error yet.

### Step 46 — Intercept helpers: time to reach, tracks, weapon reach

**Owner decisions (2026-10-23).**

- A RULER label adds the time to cover its length at our current speed. Nothing while stopped.
- A RULER drawn from one POINT to another snaps to them. It becomes a target's track: course and
  speed, with the course projected **5 minutes** ahead. This settles the old open question of
  whether a RULER snaps to a POINT.
- The selected weapon's reach shows as a **plain dashed amber ring** round our ship, while Fire
  Control is open.
- The design's buttons and flows are unchanged. The RULER keeps its drag; its label carries more.

**What was built.**

- `HandoffBoardState`: `Mark.ptA` / `ptB`, the POINTs a ruler's ends snapped to. Also
  `PointNear` and `TrackLeg`: course runs from the earlier POINT to the later, and speed is
  distance over time.
- `HandoffPlotPanel`:
  - a RULER end within 12 screen px of a POINT snaps to it (tunable);
  - a track ruler's label reads "CRS 063° · 1.10 km/s", and a dashed line runs from the later
    POINT along the course for 300 mission seconds of travel (tunable);
  - other rulers add " · 26 MIN" (or "45 S", or "1 H 12 MIN");
  - while the FIRE drawer is open, the selected mount's `rangeKm` is a dashed ring at 70% amber
    (railgun 30,000 km, PDC 4,000 km, VLS 40,000 km).
  - A drawer click now refreshes the panels, so the ring appears at once.
- `HandoffMarksGraphic`: a dashed line and a plain dashed ring. Builder: a `Weapon ring (drawn)`
  layer.

**Checked.** Playtest 76/76 PASS, console clean (`Logs/Playtests/2026-10-23_010133/`).

- A RULER read "RNG 1,970 km · BRG 090° · 26 MIN" at 1.25 km/s.
- A RULER between two POINTs 40 s apart snapped to both. It read "CRS 063° · 18.36 km/s", the
  exact course and speed of the test points, and drew its projection.
- The railgun ring showed only while FIRE was open.

**Known.** With POINTs close together, the track label can overlap the POINTs' time labels.

### Step 47 — The firing solution dialled by hand

**Owner decisions (2026-10-23).**

- The player dials the firing solution by hand from the plot. TRACK only works after an active
  scan identifies the target, as the design already had it.
- A hand-dialled solution aims at **a point in space**, not a locked contact. You can shoot a
  contact you have only heard.
- With no solution, turning a knob starts one: on the periscope's bearing, at 10,000 km, speed 0.
- How close a solution must be to hit is decided in the fire step.

This changes the design's fire control in one place. Its RNG / BRG / SPD knobs only adjusted a
solution TRACK had made; now they also start one. The layout and controls are unchanged.

**What was built.**

- `Solution.manual`. `NudgeSol` with no solution starts one: `manualStartRangeKm` (10,000,
  tunable), relative bearing from the periscope, speed 0. Then it applies the turn.
- For a manual solution:
  - BEARING (`OnBearing`) wants the periscope on the dialled bearing (heading + relative);
  - SLEW TO puts it there;
  - `AllSet` doesn't need a locked contact.
  TRACK solutions behave exactly as before.
- The readout shows "MANUAL / DIALLED BY HAND". The status line skips "NO TARGET DESIGNATED" and
  "NO TRACKING ID" for a manual solution. Only the words change.

**Checked.** Playtest 81/81 PASS, console clean (`Logs/Playtests/2026-10-23_011103/`).

- TRACK refused a heard-only contact.
- Ten steps of the RNG knob started a manual solution at 11,000 km on the periscope's bearing
  (REL −111°), at speed 0. The readout said MANUAL.
- BEARING was off 10° away from the dialled bearing. SLEW TO put the periscope on it, and it read
  ON BEARING.

**Known.**

- The knobs step 100 km per 8 px of drag, the design's fine-tune pitch. Dialling a long range from
  10,000 km takes long drags; a coarse mode may be wanted.
- The solution holds range, relative bearing and speed but no course. Leading a moving target
  needs one. That belongs to the fire step.
- The relative bearing is against our heading when it is dialled, as the design stores it.
  Turning afterwards moves the aim.

### Step 48 — Fire and hit: the railgun slug

**Owner decisions (2026-10-23).**

- A railgun shot is a **slug that flies at 50 km/s**: the prototype's 4,800 slowed ×96, like our ship.
  It takes about 8 minutes of mission time to reach TRACK 07, about 20 s at ×10.
- It **hits** if it passes within the ship's **hit radius**, set per class from the prototype (km):
  - frigate 95;
  - destroyer 135;
  - cruiser 210 (TRACK 07);
  - carrier 600;
  - anything else 150.
- On a hit, **fire control reports it with the side struck**: "HIT · TRACK 07 · PORT". Otherwise
  "MISS". Each has a sound. The slug shows on the plot as a small dot with a short trail, no label.
  Damage comes in the damage step.
- **Firing spikes our signal**: +25, tunable (GDD p.42). The enemy reacting to it comes with the
  enemy AI.

**From the spec, not new calls:**

- one shot per arming: firing releases ARM (spec 8.4.b);
- no shot below the charge it costs;
- turret arcs are fixed to the hull (spec 8.7.a);
- a slug dies at its weapon's range.

**Settled by a source:** no lead in this step. The dummy doesn't move. Leading from a POINT-to-POINT
track comes with a moving enemy.

**What was built.**

- `HandoffBoardState`:
  - `Fire()` refuses below the shot cost, releases ARM, and for the railgun adds a `Round`: our
    position, the bearing heading + BRG, and the mount's range. It spikes the signal.
  - `TickRounds(dt)` runs every board tick for `rate` mission seconds. It tests each slug's whole
    path that tick, so a 500 km jump at ×10 can't skip past a ship. The first ship passed within
    its hit radius is hit.
  - The side struck is where the slug came from, against the ship's bow: PROW, STBD, AFT or PORT
    (reference sim `sideFacing`). The bow comes from the model's heading (`Track.worldHdg`, set by
    `TacticalWorld`).
  - `InArc`: the solution's bearing from our bow must lie inside the mount's arc. **The ANGLE
    rocker now locks out of arc, with the hint "OUT OF ARC"**: turn the ship, or pick the other
    turret. The rule is new; the switch is the design's.
- Fire control's status line: ROUNDS AWAY while the slug flies, then the report.
- The plot draws slugs in our own-force blue.
- Sounds: firing plays the prototype's railgun cue (90 Hz saw). A hit is a 680 Hz tone and a miss
  the refused buzz; those two are my choice, tunable under Sound.
- Tuning window: slug speed, hit radii by class, the signal spike and the slug's trail length.

**Checked.** Playtest 93/93 PASS, console clean (`Logs/Playtests/2026-10-23_013213/`). Weapons released at CONDITION 2 from the orders station. SEN vented
to free power for WEP, and the gun charged. A hand-dialled solution on TRACK 07's bearing, nearest
whole degree. Turret A was out of arc (ANGLE locked); turret B bore. All four rockers set, and the
lever fired one slug and released ARM. Firing spiked the signal +25. Paused, the slug held still.
The test predicts each shot from the geometry and checks the game agrees:

- In run `2026-10-23_012910` the best whole-degree aim passed 192 km from TRACK 07: **"HIT · TRACK 07
  · PORT"**, with the hit cue.
- In run `2026-10-23_013213` the ship sat a little further off. The best aim passed 223 km away,
  and the game said **"MISS"**, as predicted.
- A second shot 2° off: **"MISS"** both times.

**Known.**

- Aiming is in whole degrees. At ~24,000 km half a degree is ~213 km, just over a cruiser's
  hit radius, so a dead-on shot from that far may still miss. Close in for a sure shot. **The owner
  kept whole degrees on purpose (decision 28).**
- Only the railgun fires a slug. PDC and VLS still only spend charge.
- Our own speed isn't added to the slug. There is no lead and no aim scatter.
- With high power drawn our signal is already at 100%, so the spike has nothing to add.

### Step 49 — Damage: armour, compartments, the kill

**Owner decisions (2026-10-23).**

- **Armour by side.** The prow and flanks take 2 railgun slugs to open (armour 100, a slug takes
  50). The aft takes 1 (armour 50), the weak stern.
- Once a side is open, **each slug takes 2 integrity** (of 5) off one compartment behind it, at
  random among those still standing.
- The target's damage shows on **fire control's report line** for now. A proper target readout
  waits for art.
- **The reactor at zero destroys the ship.** It becomes **a cold wreck**: no longer hostile or
  heard, a dim dot where it died.

Already ruled in `FTUE_PLAN.md` §4.4 (2026-10-18):

- the side facing the shot takes it;
- what sits behind each side: prow BRI; aft RCT and ENG; starboard MAG; port HAB and HGR;
- the enemy uses our damage model and our kill condition.

**What was built.**

- `HandoffBoardState`, the model both ships use:
  - `sideArmourMax` (PROW, STBD, AFT, PORT), `slugArmourDamage`, `slugCompartmentDamage`, all
    under Damage in the tuning window;
  - `TakeSlug(side)`: armour first, then a compartment behind the side;
  - `destroyed`, when the reactor reaches 0.
- `WorldContact.Ship`: each contact has its own `HandoffBoardState`, undamaged at load.
  `TacticalWorld` hands it to the board (`ShipOf`), with our damage numbers from the tuning window.
- `TickRounds`: the struck ship takes the hit on its own model. The report reads "HIT · TRACK 07 ·
  AFT OPEN", "… · AFT ARMOUR 50%", "… · AFT · RCT −2", or "TRACK 07 DESTROYED".
- A destroyed track becomes a wreck: not heard (`Heard` skips wrecks), not hostile, and slugs pass
  through it. The plot draws a dim dot where it died.
- The prototype's kill cue (120 Hz saw) plays for a kill.

**Checked.** Playtest 99/99 PASS, console clean (`Logs/Playtests/2026-10-23_080215/`). TRACK 07
started undamaged. The fight, from 10,000 km behind its stern (stopped, side-on so turret B bore):

1. "HIT · TRACK 07 · AFT OPEN"
2. "HIT · TRACK 07 · AFT · RCT −2"
3. "HIT · TRACK 07 · AFT · RCT −2"
4. "TRACK 07 DESTROYED"

Its reactor went to 0, its engines stayed at 5, and nothing else was touched. It went cold, and the
kill cue sounded. The earlier long-range shot correctly missed by 224 km.

**Known.**

- The fight test puts our ship in place directly, standing in for flying there with the helm.
- Hits on our own ship come when the enemy shoots back (`FTUE_PLAN.md` step J). Our own sides and
  compartments use the same code.
- The plot view doesn't follow our ship: after a long move you pan to find it.

### Step 50 — Engineering's ship diagram

**Owner decisions (2026-10-23).** The same picture of our ship as on the plot appears in the
engineering drawer:

- it replaces the design's line-and-blocks diagram;
- it is always bow up;
- a lamp on the hull shows each compartment, green, then amber, then red as its integrity drops.

**What was built.**

- The builder hides the design's diagram (`drawn#230` line, `#231`–`#236` blocks) and keeps its
  dark box (`#228`/`#229`). Over the box it adds `Ship diagram`: a RawImage of the plot's ship
  view, plus six lamps (`lamp_dot`).
- `HandoffEngineeringPanel.ShowDiagram`, on every refresh:
  - the picture shows `ShipView`'s texture in our ship's white;
  - it is turned back by our heading, so the bow is always up (no second camera);
  - its size is set by `diagramShipPx` (190);
  - lamps follow `CompHealth`: 4–5 green, 2–3 amber, 0–1 red;
  - lamp spots: bridge at the bow, magazine starboard, habitation and hangar port, reactor and
    engines at the stern. They follow the side-to-compartment map in `FTUE_PLAN.md` §4.4.
  Spots and size are tunable under Engineering & orders.

**Checked.** Playtest 103/103 PASS, console clean (`Logs/Playtests/2026-10-23_081716/`).

- With the drawer open, the picture showed our ship turned back 90° for heading 090°, bow up.
- Six lamps were green.
- With HAB at 2 and HGR at 1 (a stand-in for enemy hits), their lamps went amber and red, the
  others stayed green. Restored, they went green again.

**Known.**

- The wide hammerhead spills a little past the old dark box, still inside the screen.
- The lamp spots are placed by eye on the model (tunable).
- The target's own diagram (seeing the enemy's damage this way) waits for the target readout.

### Step 51 — The enemy moves: TRACK 07 patrols

**Owner decisions (2026-10-23).**

- The order from here: an enemy that moves; leading a moving target; then missiles, fighters,
  PDCs and the hangar launch.
- **TRACK 07 moves.** It **patrols a loop of waypoints** at **1.1 km/s** (the design's speed for
  it) and **does not react to us yet**.

This is `FTUE_PLAN.md` step A, done the plan's way: a brain that only gives orders, and the ship's own
`HandoffBoardState` moved by the same code as ours.

**What was built.**

- `EnemyBrain.cs`, on `Dummy · TRACK 07`:
  - its route is a loop of points, km from where it starts. The default diamond runs (0,0),
    (−6000,−4000), (0,−8000), (6000,−4000), from its start toward our side and back;
  - `cruiseKms` (1.1) and `arriveKm` (300);
  - `Think()` sets the ordered heading toward the current waypoint and the throttle for cruise
    speed, and moves on to the next waypoint on arrival.
  All three are under Enemy in the tuning window.
- `TacticalWorld`:
  - at load, each contact's ship starts where its model is, facing its way, engines powered;
  - each board tick, for each contact with a brain, it runs `Think`, `TickHelm` and
    `TickMove(rate)` — the same turning, acceleration and drift as ours — then places its model;
  - the track's `speed` is now its real speed, which TRACK reads;
  - a destroyed ship cuts its throttle and coasts to a stop.
- Paused, there is no tick, so it holds still.

**Checked.** Playtest 107/107 PASS, console clean (`Logs/Playtests/2026-10-23_085004/`).

- After 3 s at ×10, TRACK 07 had gone 34 km and was making 0.71 km/s, still speeding up.
- It was ordered onto 236° for its first waypoint and was still turning (heading 308°).
- Its model followed its ship model, and the board's track read its real speed.
- The older checks (the bearing trail crossing at the target, the hit, the fight) still assume a
  target that holds still. Until leading lands in Step 52, the playtest stops TRACK 07 after the
  patrol check, and holds our own ship still during the patrol burst.

**Known.** At load our throttle sits at the design's 40%, so once ENG is powered our ship gets under
way on its own.

### Step 52 — Leading a moving target

**Owner decisions (2026-10-23).**

- A hand-dialled shot is led by **fire control, from your plotted track**: the latest
  POINT-to-POINT RULER gives the target's course and speed. Your plot's accuracy decides the hit.
- **TRACK leads exactly** once an active scan has identified the target, as the reward for going
  loud.
- No new controls. Drawing a track copies its speed into the existing SPD readout.

**What was built.**

- A `Solution` now carries:
  - `hasCourse` and `course`;
  - where the target was (`wx`, `wy`) and when (`t`, mission seconds) as the solution was set.
- The solution is set in three places:
  - a hand-dialled solution records the dialled point at each knob turn (`Fix`);
  - the latest track (`ApplyTrack`) gives it course and speed, when the solution starts or when a
    track is drawn;
  - TRACK takes the target's true position, course and speed from the lock.
- `LeadBearing()`, at the moment of firing:
  - it reckons the target forward from the solution's point and time to now;
  - it solves where the 50 km/s slug meets it, and fires on that bearing;
  - without a course, the shot flies straight down the dialled bearing, as before.
- `HandoffBoardControls` keeps the board state's clock current (`missionNow`). `TacticalWorld` gives
  each track its real course over the ground (`Track.course`).

**Checked.** Playtest 113/113 PASS, console clean (`Logs/Playtests/2026-10-23_090720/`). Run against the patrolling TRACK 07, steady on its leg, from 12,000 km
off its starboard beam:

- Without a track, the solution had no course.
- Two POINTs on it two minutes apart, joined by a RULER, fed the solution exactly the track's
  course and speed.
- The led shot hit. Aimed straight, the same shot would have passed 295 km off and missed; led, it
  passed 113 km off. The plotted course read 250°, against 244° at that instant: after its turn the
  ship was still sliding onto its new course, so the plot shows the average. (In an earlier run: 438
  km unled, 33 km led.)
- After a lock, TRACK carried the target's course, and its led shot hit too.

In the first run TRACK 07 was still turning onto its leg while it was plotted. The track then gives
its average course over the two minutes (256° against 248° at that instant), and the shot still hit
(`2026-10-23_085847`). The test now waits for a steady target.

**Known.**

- The playtest places the two POINTs on the target's true positions, standing in for good
  plotting; the plot itself is tested in Step 46. It dials the range directly.
- If the target turns after it was plotted, the lead is wrong until it is re-plotted, as at sea.
- The solution's point is fixed when the knob is last turned: an old solution is reckoned forward
  on the plotted course.

### Step 53 — Missiles: the launcher, homing, three warheads

**Owner decisions (2026-10-23).**

- Missiles fly at **20 km/s**: slower than a slug, but they home. About 10 minutes to 12,000 km.
- They **home on a locked target**, which needs an active-scan identification (GDD p.45). No
  leading needed.
- **The warheads split:**
  - HE hits armour hard;
  - PENETRATOR ignores armour and goes straight to a compartment behind the side;
  - EMP does no hull damage but knocks the power out.
- **Salvos stay** (1–8, using the launcher's 8 cells).
- The design's warhead selector and salvo stepper were never in the export, so:
  - **the turret knob picks the warhead** while MISSILE is selected (there is only one launcher);
  - **the salvo is set in the tuning window** (default 2) until the stepper has art.

**What was built.**

- `Round` is now a slug or a missile: it carries its speed, whether it is a missile, its target
  and its warhead.
- `Fire()` with MISSILE (`MS`):
  - it refuses without a lock or a cell (`MissileReady`), before spending charge;
  - it launches `salvo` missiles, fanned 3° apart, and counts the cells down;
  - it spikes the signal.
- In `TickRounds` a missile steers toward its target, at up to 1.3° per mission second (the
  prototype's 2.2 rad/s slowed ×96). The path test and hit radius are the slug's.
- `TakeWarhead` on the struck ship:
  - HE takes 100 armour, which opens any side in one hit, then compartments −2;
  - PEN takes a compartment behind the side −2, whatever the armour;
  - EMP drains every bus to 0 for 60 mission seconds (`empUntil`). The ship's power can't refill
    until then (`TickPower` holds it at 0; a contact's engines come back when it ends).
- Reports read "HIT · TRACK 07 · STBD · HE · MAG −2", "… · EMP · POWER DOWN", "… · PEN · MAG −2".
- Fire control:
  - with MISSILE the turret knob cycles HE → EMP → PENETRATOR, which also clears the ladder, as
    the design says;
  - the readout shows "HE · SALVO 2 OF 6" (cells left).
- Tuning window: missile speed, turn rate and spread, HE armour damage, EMP time, salvo.

**Checked.** Playtest 121/121 PASS, console clean (`Logs/Playtests/2026-10-23_221627/`). Against the
patrolling TRACK 07, with a lock (stand-in for an active scan):

- An HE salvo of two left the launcher (6 cells left). Both homed in and hit its already-open
  starboard side, and its magazine went 5 → 1.
- The knob picked EMP: "POWER DOWN". With no power it slowed from 0.93 to 0.27 km/s.
- PENETRATOR: "PEN · MAG −2".
- Four cells used.

**Known.**

- Missiles are drawn like slugs: a dot and a trail.
- Nothing can shoot them down yet. PDCs come later.
- The knob doesn't turn to show the warhead; the readout line does.
- No reload: 8 cells per mission.

### Step 54 — Fighters: launch, fly out, attack runs, recall

**Owner decisions (2026-10-23).**

- Fighters fly at **8 km/s**, about three times our top speed.
- **Attack runs:**
  - DESTROY: every 30 mission seconds each fighter takes 10 armour off the side facing it. Once
    that side is open, it takes 1 off a compartment behind it.
  - DISABLE: the same, but always at the engines (AFT armour, then ENG).
  - ESCORT: the squadron stays with our ship.
- **On the plot:** one small wedge per fighter, in own-force blue, in a loose V, pointing where
  they fly. No labels.
- **RECALL brings them home.** They turn back to our ship and land; the squadron is then ready
  again. Fuel comes later.

**What was built.**

- `HandoffBoardState`:
  - `LaunchSquad()` now also puts a `Flight` in the air: the squadron's ready craft, its order,
    fire control's target, starting at our ship on the launch bearing.
  - `TickFlights()`, each board tick:
    - the flight flies toward its target, or home, or with us on ESCORT;
    - inside 200 km of the target it makes its attack runs (`TakeFighterRun` on the target's own
      ship model);
    - if the target is destroyed, it turns home;
    - home and inside 50 km, it lands: the squadron is aboard and the launch stages clear.
  - `RecallSquad()` with a flight out turns it home instead of removing it at once.
  - `FlightStatus()` gives the hangar's telemetry line: "OUTBOUND TO TRACK 07 · 1,592 KM",
    "ATTACKING TRACK 07 · PROW ARMOUR 60%", "RETURNING · 1,820 KM", "ESCORTING".
  - A destroyed target is marked a wreck through the same `Kill()` as a slug.
- `HandoffPlotPanel` draws the fighters from a hidden template (`fighterTemplate`, the builder's
  "Fighters" layer, `hull_wedge_filled`). Size and V gap are in the Tuning window.
- `HandoffHangarPanel`'s telemetry line shows `FlightStatus` while the squadron is out.
- Tuning window, "Hangar & fighters": speed, attack range, seconds between runs, armour and
  compartment damage, wedge size and gap.

**Behaviour change, as the owner decided.** Before, RECALL ended a launch at once. Now it turns the
fighters home and they land when they reach us. The telemetry line used to read "SQUADRON AIRBORNE —
RECALL TO RECOVER" while out; it now says what the flight is doing.

**Checked.** Playtest 130/130 PASS, console clean (`Logs/Playtests/2026-10-23_231846/`). With TRACK 07
held still and our ship 2,000 km off its bow:

- ENG and WEP were drawing all 10 units, so the catapult refused ("NO AUX UNITS"). Venting ENG
  (we were stopped) gave AUX 3 units.
- The six launch stages ran to 100% and the launch lever put four interceptors on the plot.
- They flew out at 8.00 km/s ("OUTBOUND TO TRACK 07").
- Their first run took PROW armour 100 → 60, which is 4 × 10 ("ATTACKING TRACK 07 · PROW ARMOUR 60%").
- RECALL turned them home; they landed, the stages cleared, and there were no runs after the recall.
- The fight to the kill that follows still passed unchanged.

**Known.**

- The outline wedge (`hull_wedge_outline`) vanishes at 16 px, as its lines go under a pixel, so the
  filled one is used. Same shape.
- The fighters fly to the target's true position even when we only hear it. The plot shows them
  flying down the bearing line.
- The playtest doesn't run DISABLE or ESCORT; their code paths are simple branches of the same
  tick. It launches with a stand-in for typing the launch bearing.
- No fuel, no losses, no enemy PDCs shooting them down yet.
- The assault boats' SECURE order attacks like DESTROY (boarding is out, decision in memory).

### Step 55 — PDCs, and the enemy fires back

**Owner decisions (2026-10-24).**

- **TRACK 07 fires back.** When we hit it, and we are inside its launcher's range, it fires a
  missile salvo at us. This is the first small piece of the ship AI.
- **Arming the ladder puts a PDC group on AUTO.** Select PDC, pick group A or B with the turret
  knob, and arm the ladder. The group then shoots by itself at anything in its arc until you drop
  ARM. Its arc shows on the plot while it is on. No new controls.
- **A chance per missile.** Each enemy missile that enters an AUTO group's arc and range gets one
  roll: 70% it is shot down (Tuning window).
- **Enemy PDCs come later,** with the ship AI. Our missiles and fighters work as before.

**What was built.**

- `HandoffBoardState`:
  - `Incoming`: enemy missiles flying at us. `LaunchAt()` adds a fanned salvo; `TickIncoming()`
    lets the PDCs try, then steers each missile at us. One that passes within `ownHitRadiusKm`
    (210) strikes the side facing it through the same `TakeWarhead` as our missiles
    ("MISSILE HIT · PROW · HE · BRI −2").
  - `pdcAuto[2]`, for PDC-A and PDC-B. Throwing ARM with a group selected turns it on; dropping
    any rung turns it off. Switching weapons leaves it on; coming back to that group shows its
    ladder still armed.
  - `RungGated()`: one place for the ladder's gates, used by `Flip` and by the fire-control panel.
    A PDC group needs only WEP power and its autoloaders: no solution, no ANGLE or BEARING gate.
  - `PdcStops()`: one roll per group per missile, needing WEP and the autoloaders.
  - Sides now come from one helper, `SideAt`, for slugs, fighters and incoming missiles.
- `EnemyBrain`: if its armour or a compartment dropped since the last tick, it fires `fireBackSalvo`
  (2, HE) at us, at most once every `fireBackSeconds` (120), while it has cells. Its switch
  `firesBack` is in the Tuning window.
- `HandoffPlotPanel`: enemy missiles show as red dots with a trail. Each AUTO group draws its arc as
  a dashed amber sector out to its range (the design's PDC arc colour, accent.caution at .45). No
  label.
- Fire control's status line reads "PDC-A AUTO · LAST: PDC-A · MISSILE DOWN".
- Being hit plays the alert tone.

**Behaviour changes, flagged.**

- **Autoloaders start ENGAGED** (`loaders = true`). The design starts them off, but its LOADERS
  button was never in the export (HANDOFF_GAPS row 73). Without it PDCs could never arm. The switch
  is in the Tuning window until the button has art.
- **With PDC selected, the fire lever does nothing.** The guns fire by themselves on AUTO.
- **PDC's ladder no longer needs a firing solution, ANGLE or BEARING.**

**Checked.** Playtest 138/138 PASS, console clean (`Logs/Playtests/2026-10-24_002347/`). With our
ship stopped 6,000 km off TRACK 07's bow, turned so it bears 042° (PDC-A's arc):

- The ladder armed on PDC-A with no solution, and PDC-A went AUTO.
- A stand-in hit on TRACK 07 made it fire two HE missiles at us (its cells went 8 → 6).
- PDC-A shot both down, at a stand-in certain chance; nothing hit us.
- On the railgun PDC-A stayed on AUTO. Back on PDC-A, its ladder still showed armed.
- With ARM dropped, the next salvo got through: two hits ("MISSILE HIT · PROW · HE · BRI −2"),
  with the alert tone.
- The fight to the kill and the engineering checks that follow still pass. The test repairs our
  ship first, as a stand-in for damage control.

**Known.**

- The playtest stands in for the railgun hits that trigger the salvo, and for a certain PDC; the
  70% chance itself isn't tested.
- Enemy missiles show on the plot even when our sensors couldn't see them.
- Nothing happens if our reactor reaches 0: no game over yet.
- TRACK 07 fires whatever its own power state is.
- PDC-B covers astern-port (200° ±35°); nothing covers dead ahead or dead astern, as the design
  has it.

### Step 56 — The intro: flat black, typed in place, straight cuts

**Owner direction (2026-10-17, restated 2026-10-24).** The intro screen must be flat black. Each
line appears in place, left to right, as if typed. Then a straight cut to the next thing. No
zoom: the old pull-back shrank STARWOLF in place.

**What was wrong.** The green was the intro package itself. Step 27 gave its shader a faint lit
field (`_FieldGlow` .006 of the phosphor, plus the `#010601` background). On screen it measured
(45, 77, 45), a mid green: the picture is still encoded twice on its way to the screen, which
brightens anything near black.

**What changed** (`Assets/StarwolfIntro/`, owner-supplied, changed at the owner's request):

- `PhosphorDisplay.shader`:
  - the field is pure black; `_FieldGlow` and `_Background` are gone;
  - SECTION 36, INITIALISING... and STARWOLF each type in by cell column (`typed()`, driven by
    `_Reveal`), the newest column fading up;
  - the pull-back zoom and its grid are gone; the menu draws alone.
- `StarwolfIntroController`:
  - `pullbackDuration` is gone; the menu follows the title directly;
  - `typeShare` (0.5): each line types over this share of its time, then holds;
  - the old fade in and out is gone.
- Timing in `00_Start` is unchanged: 4 / 2 / 4 s, then the menu (10 s, was 14).

**Checked** in play mode, screenshots measured: the background is (0, 0, 0) on all three lines
and on the menu. SECTION 36 shows half typed, STARWOLF fully typed in place, and the menu has no
title behind it.

**Known.**

- The double encode is still there, so the text is a little paler than the reference
  (`References/Company Logo.png`). Only black is immune to it.
- `00_Start` still holds an unused `pullbackDuration: 4` line; Unity drops it on the next save.

**The order, changed the same day (owner 2026-10-24).** The flow is now:

**SECTION 36 → STARWOLF → menu → NEW GAME → beta page → Enter → INITIALISING... → battle map.**

- `StarwolfIntroController.initialisingOnly`: with it ticked, the controller types only the
  INITIALISING... line, holds, then calls its new `finished` event. Without it (in `00_Start`),
  the intro is SECTION 36 then STARWOLF then the menu: 4 + 4 s.
- `01_Briefing` has a new inactive object, `INITIALISING`, with that controller (2 s), and
  `finished` wired to `ScreenFlow.Advance()`.
- `ScreenFlow.showFirst`: on Enter, it shows that object first instead of loading at once.
- Checked in play mode: shots at 2.5 s (SECTION 36), 5.2 s (STARWOLF) and 8.3 s (the menu). In
  `01_Briefing`, INITIALISING... typed in over black, then `02.1_Grid` loaded.

A trap: a screenshot taken right after `SeekPreview()` lands about half a second later than the
time asked for (the editor runs a few frames first). A shot asked for at 3.5 s showed STARWOLF's
first letter, not SECTION 36.

### Step 57 — Everything in game time

**Owner decision (2026-10-24).** Every rate runs per game second, charging included. At x10
everything is ten times as fast, together.

**Why.** The board's per-tick amounts (weapon charge, AUX, bus fill, the bow's swing, launch stages,
the damage-control party, signal) were applied once per 0.42 s tick whatever the rate. At x10, guns
recharged ten times faster in game time than ships moved.

**What changed** (`HandoffBoardState`):

- `TickPower`, `TickAux`, `TickHelm`, `TickWeapons`, `TickLaunch`, `TickParty` and `TickSensors`
  take the tick's game seconds (`rate`), as `TickMove` already did.
- Whole-unit steps (a bus filling one unit, the bow swinging whole degrees) bank the fractions, so
  x0.25 is exact too. The reactor cap still holds at once.
- The party works `partySeconds` (6) game seconds; it was `partyTicks`. The engineering line reads
  "n OF 6" in seconds.
- `TacticalWorld` passes the rate to the enemy's helm. The radar sweep turns in game time, so x10
  sweeps ten times as fast.
- **x1 is unchanged:** a tick at x1 is one game second, so the design's numbers now read as per game
  second.

**Checked.** Playtest 138/138 PASS (`Logs/Playtests/2026-10-24_104850/`).

Two runs failed first:

- One because someone clicked the engineering key during the run; the fire-control drawer closed
  under the test.
- One because game time changed our ship's path in the helm step, so an earlier shot hit TRACK
  07's bow. The fighter check had assumed an untouched bow; it now works from whatever the bow
  holds.

**Damage plan.** The damage model is written up in `DAMAGE_README.md`, next to this file. It is
built in the order given there: side map, conditions, teams, breakdowns, displays. Boarding comes
right after conditions.

### Step 58 — The audit's side map; our ship can die

**Owner decisions (2026-10-24).**

- **The design audit's map** replaces the 18 Oct one: bow → magazine, stern → engine room, port →
  reactor, starboard → hangar. One fixed room per face. Bridge and habitation sit inboard and are
  never hit directly.
- **A reactor at 0 destroys any ship, ours included.** This overrides the audit's "never
  unrecoverable".

**What changed.**

- `HandoffBoardState.BehindSide` is one room per face. `Behind()` has no random pick; a wrecked room
  reports "· RCT WRECKED" and takes no more.
- **Destroyed** means the reactor's structure is at 0. It used to read `CompHealth`, which counts a
  vented room as 0, so venting the reactor would have destroyed the ship.
- A destroyed ship goes dark: `TickPower` holds every bus at 0. With no power there is no thrust, no
  weapons and no sensors.
- Fighters on DISABLE still go for the stern, which is now the engine room alone.
- The engineering diagram's lamps follow the deck plan in `DAMAGE_README.md`.
- `FTUE_PLAN` §4.4 points to the README.

**Checked.** Playtest 139/139 PASS (`Logs/Playtests/2026-10-24_105807/`):

- The fight to the kill now comes off TRACK 07's port beam: "PORT ARMOUR 50% / PORT OPEN /
  PORT · RCT −2 / PORT · RCT −2 / DESTROYED". Nothing else on it was touched.
- Missiles from its starboard beam hit the hangar ("STBD · HE · HGR −2").
- New check: our port side opened and three slugs into our reactor destroy our ship, and its power
  drops to 0. This is a stand-in for enemy fire.

**Known.** Nothing happens on screen when our ship is destroyed beyond going dark. The game-over
screen is still to be designed (`DAMAGE_README.md`, still open).

### Step 59 — Conditions: fire, vacuum, radiation

**Owner decisions (2026-10-24).** The rules are in `DAMAGE_README.md`, with the design audit §09.5 as
the base:

- The three conditions spread over the owner's deck plan.
- **The weapon and the room decide what a hit starts:**
  - HE and penetrators start fires;
  - slugs breach a room;
  - reactor hits leak radiation.
- **More hits on a wrecked room** still start conditions.
- **The audit's rules, on the design's own buttons.** DUMMY 1 becomes **RECALL**, with the REPAIR /
  SEAL / VENT rocker art and its own legend. DUMMY 2 stays as it is.

**What was built** (`HandoffBoardState`; the same model runs both ships):

- **Room state.** Each room carries `fire`, `rad` and `doorsShut` next to `vented`. The design's
  `cond` is derived from them: FIRE before RAD.
- **What a hit starts.** `Strike()` rolls the condition chances on every hit through open armour.
  The report says what started: "· MAG −2 · FIRE", "· HGR −2 · BREACH", "· RCT −2 · RADIATION".
- **`TickConditions(dt)`**, per game second:
  - fire takes 1 integrity every 30 s;
  - radiation kills one crew member every 20 s;
  - all three spread to neighbours: fire only into rooms with air; vacuum only from rooms whose
    doors are open; radiation only while the reactor leaks (below integrity 3, not vented);
  - radiation fades 90 s after the reactor is contained;
  - a reactor burned to 0 destroys the ship.
- **A breach** vents the room, puts out its fire, kills half its crew, and loses any team working
  there.
- **Engineering's rockers (and the orders station, now on the same rules):**
  - REPAIR sends the team. It puts a fire out, or restores the air in a sealed room, or repairs.
    It no longer needs crew in the room; it refuses an open airless room with "SEAL THE DOORS FIRST".
  - SEAL shuts an airless room's doors.
  - VENT dumps the air: the fire goes out, all crew inside die, and the doors shut.
  - REBOOT adds 1 and no longer clears a condition.
  - RECALL calls the team back.
- **The enemy's ship** runs the same conditions with our Tuning numbers (`CopyDamageTuning`). One
  whose reactor burns out becomes a wreck (`NoteWrecks`).
- Tuning window, new section **"Damage control"**: every chance, rate and time in the README.

**Behaviour changes to the design, flagged.**

- SEAL no longer restores air at once.
- REBOOT no longer clears conditions.
- REPAIR no longer needs crew aboard.
- The orders station's VENT no longer refuses with crew aboard, and its PARTY can work an airless
  room once sealed.
- DUMMY 1 is RECALL.

**Checked.**

- A quick check on a bare ship model first, all rules.
- Then playtest 153/153 PASS (`Logs/Playtests/2026-10-24_111146/`), new step 24. Chances and spread
  are set to certain for exact results, and the hits are stand-ins:
  - HE through the open bow set the magazine on fire. It spread to the bridge, reactor and hangar,
    and not to the engines or habitation. It burned the magazine 3 → 2.
  - A team put it out. VENT put out the bridge fire and its 9 crew died.
  - A slug breached the hangar ("HGR −2 · BREACH", crew 7 → 3). REPAIR refused until SEAL; with
    the doors shut nothing spread; the team restored the air.
  - RECALL called a team back with no repair landing.
  - Two slugs into the reactor leaked radiation into the magazine and bridge, not the hangar, and
    it killed magazine crew. Contained, it faded.
- The engineering screen shows it with the design's own readouts (ATMO VENT, COND FIRE, crew 0, "3
  COMPARTMENTS IN HAZARD").
- Steps 1–23 run with the condition chances at 0 (a stand-in), so they stay as written.

**Known.**

- A room with both fire and radiation shows FIRE only, and doors shut on an airless room still show
  VENT. Both wait for the displays step.
- Teams take no travel time and no crew yet (the teams step).
- Crew killed in a room are not replaced or moved.
- The enemy does not fight its own fires: its rooms burn until we finish it, or a fire reaches its
  reactor.

### Step 60 — Boarding: the assault boats

**Owner decisions (2026-10-24).** The rules are in `BOARDING_README.md`, next to this file.

- **The design's three orders, exactly:** DESTROY blows up its reactor; DISABLE wrecks its engines
  and magazine; SECURE captures it.
- **Boarding needs** one face open and the ship nearly stopped: fight it down first.
- **The fight aboard is time and odds:** 60 s, marines against a quarter of its crew, with losses
  on both sides.

**What was built** (`HandoffBoardState`):

- **Launch.** The ASSAULT BOAT squadron flies as a `Flight`, like the fighters, at 6 km/s. Each boat
  takes 6 marines from the ship's 18. The launch refuses with "NO MARINES ABOARD" when none are left.
- **`TickBoarding`:**
  - the boats hold off with a reason ("HOLDING OFF TRACK 07 · NO OPEN SIDE" or "TARGET UNDER WAY");
  - they dock and fight ("BOARDING TRACK 07 · 42 S");
  - they win or are repelled ("BOARDING REPELLED"), then fly home with the outcome on the hangar
    line;
  - survivors rejoin the pool when they land.
- **SECURE** marks the ship and its track captured. The enemy stops and holds still
  (`TacticalWorld`), and the plot shows it as an own-force dot (placeholder).
- **RECALL** while aboard withdraws the marines with no result.
- Tuning window, "Hangar & fighters": boat speed, marines, docking distance, speed limit, fight time,
  defender share, losses.

**Checked.**

- On bare ship models, all three orders: SCUTTLED (wreck), DISABLED (ENG and MAG 0, alive), SECURED
  (captured, not hostile). In each case the boats held off with no face open, and the marines came
  home.
- Playtest 160/160 PASS (`Logs/Playtests/2026-10-24_120447/`), new step 21f, through the hangar's
  controls:
  - three boats launched on DISABLE with 18 marines;
  - they held off while no face was open (a stand-in), and boarded once one was;
  - they won (stand-in: no defenders, so the result is certain) and disabled TRACK 07;
  - they flew home with "TRACK 07 DISABLED · RETURNING" and landed with all 18 marines;
  - the fight to the kill that follows still passed.

**Known.**

- Salvage and what a prize is for, fuel, and enemy boarders are not built (`BOARDING_README.md`,
  still open).
- The boats are drawn as the fighters' wedges.

### Step 61 — Scan to know what you hit; smoothing; heading 000 at load

**Owner (2026-10-24).**

- "You have to scan the ship to even be able to understand what you're damaging."
- Use what is there, with no new art: engineering shows only our own ship, and the periscope is
  really the targeting computer.
- Separately: the room rings and bars looked grainy; and the ship should start pointing up.

**What changed.**

- **One naming rule, as the design's sensors board already had it.**
  - A contact is "BOGEY T07" until an active scan identifies it, then "TRACK 07" (`NameOf`,
    `Scanned`).
  - Hit reports, the kill line, the hangar's lines and boarding outcomes all follow it.
  - Unscanned, a hit reads only "HIT · BOGEY T07"; scanned, it gives side, room and condition.
- **The targeting computer's SUBJECT box,** for a scanned subject, uses its own two lines:
  - "TRACK 07 · CRUISER" (the class once CLASS is pressed);
  - its hull: "BOW 100 · STBD ▸ HGR 3 · AFT 50 · PORT 50". Each face shows its armour %; through an
    open face (▸, as engineering's lines use it) it shows the room behind and its integrity.
  - The name under the brackets follows the scan rule.
- **Smoothing.** The board was drawn into a render texture with no anti-aliasing, and the board
  camera had MSAA off. So every shape the board draws itself came out hard-edged and grainy next to
  the browser design: the compartment rings, bars, plot lines, the sweep, wedges. Now 4× MSAA
  (`GammaBoardPresenter.msaaSamples`; the builder allows MSAA). The design's art and text are
  unchanged.
- **Heading 000 at load** (the design's demo had 083).

**Checked.** Playtest 161/161 PASS (`Logs/Playtests/2026-10-24_154633/`):

- The first two hits read "HIT · BOGEY T07", before any scan.
- After a stand-in scan, the targeting computer read "TRACK 07" / "BOW ▸ MAG 0 · STBD ▸ HGR 0 · AFT
  100 · PORT 100".
- Rings checked zoomed in: smooth edges.

Two runs before it failed: the window was used mid-run, then Unity was restarted mid-run.

**Trap.** The playtest wait loop picked the newest run by modification time. The old run folders
were touched when their videos were deleted, so it read an old report. Pick the newest run by name.


### Step 62 — Weapon reach, the sweep's inner circle, the enemy's picture

**Owner decisions (2026-10-24).**

- **Weapon reach equals the DRADIS at full gain:** railgun and missiles reach 47,000 km (12,000 +
  5 × 7,000). It doesn't depend on the gain set now. You can dumb-fire that far; a lock still needs
  a scan. The design had 30,000 and 40,000. PDCs stay at 4,000.
- **A small circle round our ship.** The sweep runs from it to the DRADIS edge, not from the ship.
- **Enemy picture after CLASS.** Once scanned and classified, a contact shows on the plot as its
  own 3D model seen from above, in red, turned to its heading, instead of its dot.

**What changed.**

- `HandoffBoardState.mounts`: RGA, RGB and VLS reach 47,000 km.
- `HandoffSweepGraphic.SetInner` draws the band and the edge from an inner radius.
- `HandoffPlotPanel` draws the inner circle, half our ship's drawn length plus `innerRingMarginPx`
  (8). It also shows `contactShipImage` for a classified contact, with its label pushed clear.
- **New in `02.1_Grid` (World):** "Contact View Camera · TRACK 07", a copy of our ship's view camera
  aimed at TRACK 07's model (StarWolf_Cruiser). The builder adds a "Contact ship (3D view)" layer.
- `ShipView.Own()`: our ship's picture (plot and engineering) now finds our own camera, not any
  ShipView.

**Checked** in play mode, with a screenshot:

- the inner circle and the sweep starting from it;
- our ship pointing up;
- TRACK 07 drawn as a red cruiser, turned to its heading.

**Playtest:** first run with Step 63 (163/163). The owner's laptop had died before it could run here. It has two new
checks (CLASS shows the picture; the sweep starts at the inner circle) and the updated range check.
Run it first next session.

### Step 63 — Scale: everything that moves 5× faster; ACTIVE scans; the gain comes back

**Why (owner, 2026-10-24).** After playing: the ships felt too slow, plotting gave no data, and the
active scan "didn't seem to be working".

**Played and measured.**

- **In the game, before the change:**
  - at x1, 30 real seconds at full throttle moved our ship 3 px and TRACK 07 4 px;
  - powering up quietly dropped the DRADIS gain from 2 to 1 for good, so TRACK 07 (24,300 km)
    fell out of the 19,000 km DRADIS;
  - ACTIVE on identified nothing: only letting go of the cone dial, or clicking a contact-board
    row, did.
- **The scale study.** `docs/scale-sim/` (new, kept as a tool; README there) played whole fights at
  several speed scales:
  - at the old speeds a win took 35 minutes at x1, and two marks read a course only to ±20°;
  - at 5×, about 7 minutes and ±4°; at 10×, about 4 minutes;
  - engagement range barely mattered;
  - at x5 and x10, people arm too slowly and TRACK 07 can win.

**Owner decisions.** **5×** ("slow but not tediously slow"). ACTIVE scans while it is on. The gain
comes back once the power does.

**What changed.**

- **Everything that moves, 5×** (`HandoffBoardState`; `EnemyBrain` default and the scene's value):
  - our top speed 12.5 km/s;
  - acceleration and braking ×25, so full speed comes 5× sooner;
  - drift and drag keep rates to the 5th power;
  - turn 20°/s; TRACK 07 cruises at 5.5 km/s;
  - slugs 250 km/s, missiles 100 km/s (turn 6.5°/s), fighters 40, assault boats 30;
  - boarding speed limit 0.5;
  - the SPD knob steps 0.25 (`spdStep`).
  - The map, sensor and weapon ranges, charge and timings are unchanged.
- **The hit test follows the target's motion within a tick** (`TickRounds`). At these speeds and
  x10 a ship moves further in one tick than its hit radius.
- **ACTIVE scan:** with ACTIVE on, a contact inside the cone and DRADIS range is identified as the
  sweep passes it (`HandoffPlotPanel`). The alarm strip and alert tone say so. Letting go of the
  cone dial and clicking a row still work.
- **Gain:** `gainSet` remembers the gain you set. While the SEN bus is short of power the gain is
  held down, then comes back to it.

**Checked.**

- In the game after the change, the same opening: the gain held at 2, ACTIVE identified TRACK 07
  on its own, and TRACK 07 moved 19 px in 30 s at x1.
- Playtest 163/163 PASS (`Logs/Playtests/2026-10-25_184602/`), including Step 62's checks. Tests
  adjusted for the new speeds:
  - the gain starts at 1 so hearing still comes at step 17;
  - a marginal aim may land either way;
  - fighters' speed is measured at x1;
  - the second salvo waits out TRACK 07's reload.

**Found, not changed (for the owner).**

- The throttle has the same trap the gain had. Set full throttle while the ENG bus is still filling
  and it is clamped to 20%, then stays there: the design's "the lever stops where the bus stops".
- With all four buses on DRAW, ENG gets 3 of 10 units, so throttle stops at 60%. That is the
  power trade-off as designed.
- The mission clock starts at 2:03:32, the design's demo time.

### Step 64 — Lose power, lose the setting

**Owner decision (2026-10-25).** "If the power goes, you have to redo everything. It just sets back
to zero." This replaces Step 63's "the gain comes back".

**What changed** (`HandoffBoardState`):

- **Gain:** if the SEN bus can no longer carry it, the gain drops to 1 (its lowest) and stays there.
  `gainSet` is gone.
- **Throttle:** if the ENG bus can no longer carry it, the throttle drops to 0 and the ship brakes to
  a stop. The design held it at what the bus could carry.
- **At load** power is cold, so the gain starts at 1 and the throttle at 0. The design's demo had 2
  and 40. Powering up no longer knocks anything back without telling you. Set them once the power
  is on.

**Checked.** Playtest 163/163 PASS (`Logs/Playtests/2026-10-25_185453/`).
---

## 7 · How to keep this log

This file is the handoff. Update it at the end of every step:

- Move the step to Done in the table in section 1.
- Fill in what actually went into the scene, in section 2.
- Add any new script, shader or material and what it is attached to, in section 3.
- Add any decision that was made, in section 4, so nobody reopens it.
- Add anything broken, half-finished, or any trap that cost you time, in section 5.
- Write up the step itself in section 6.
- Update **Open items** at the bottom: take out what you finished, add anything you left behind.

Nothing else in the repo gets written by the log keeper. Only this file.

---

## 8 · Open items

Still not done. **This is the list to work from.** Read it before picking up any work.

### Wiring order (owner, 2026-10-17)

One feature at a time, planned with the owner, each working before the next (decision 17). Step 34
put the dummy enemy in place. The goal: **use every tool to fight the dummy enemy for real.** The
order:

1. **Power P1.** "Raw power is the basis of everything." **Done in Step 35.**
2. **The plot tools.** **Done in Step 36.** Then, at the owner's request: our ship on the plot
   (Step 37), the tuning window (Step 38), nothing standing on the plot (Step 39).
3. **The design's press and click interactions.** **Done in Step 40.**
4. **The helm.** **Done in Step 41.**
5. **Next:** back into the sequence: sensors, then periscope and fire control against the
   dummy, then fire, then hit.
   - **Sensors step:** the sensor circle (Step 42), the radar sweep (Step 43), and hearing, with
     bearing lines, the alarm strip trigger and the tracked blink (Step 44), are done. Still to do:
     the enemy noticing us (P3, `FTUE_PLAN.md` step B).
   - **Plotting feeds the firing solution (owner, 2026-10-23).** Step 45, the bearing trail, is done.
     Step 46, the intercept helpers, is done too:
     - a RULER drawn between two POINTs snaps to them and becomes a track (course, speed, projected
       ahead);
     - the RULER label shows time to reach at our speed;
     - the selected weapon's range ring shows while Fire Control is open.
     Steps 47–49 are done: the hand-dialled solution, fire and hit, and damage. **The dummy can
     be found, plotted, shot and destroyed.**
   - **Owner's order (2026-10-23):** an enemy that moves (done, Step 51), leading a moving target
     (done, Step 52), missiles (done, Step 53), fighters (done, Step 54), PDCs and the enemy
     firing back (done, Step 55), **then the hangar launch** (what is left of it after Step 54:
     plan with the owner).
   - **Owner, 2026-10-24: damage and damage control next,** built in the order in
     `DAMAGE_README.md` (side map, conditions, teams, breakdowns, displays). **Boarding** is done
     (Step 60, `BOARDING_README.md`). Still open there: salvage, fuel and range, enemy boarders. Later: the
     periscope shows the real enemy ship (today it shows the design's decorative corridor).
   - **Owner's list (2026-10-23), each to be planned with the owner:**
     - ~~fighters: a launch puts a small triangle squadron on the plot, flying at the enemy~~ (done,
       Step 54);
     - ~~engineering shows the same top-down ship picture as the plot~~ (done, Step 50);
     - ~~missiles and PDCs~~ (done, Steps 53 and 55);
     - the hangar launch;
     - a ship AI that fights back (`FTUE_PLAN.md` steps A–C and J);
     - ~~leading a moving target from a POINT-to-POINT track~~ (done, Step 52);
     - a faster way to dial long ranges.
   - ~~**Fire-control step:** show the PDC arcs once the PDCs are activated~~ (done, Step 55; no
     label, per decision 24).

**Items 5 and 7 of the wiring order are planned.** The first mission, the XO walkthrough (FTUE), the
derelicts and the easy enemy AI are in `FTUE_PLAN.md`, next to this file (owner, 2026-10-18). Its
step E (the comms box) can start now; step A (enemy patrol) once the helm works.

**Parked.** Both fold into the sensors step later:

- Power P2: the hearing-range ring on the plot, plus the BRG bearing line.
- Power P3: the enemy notices you (UNEASY, SEARCHING, ALERT).

**The plot tools, as the owner defined them (2026-10-17):**

- **POINT** is a timestamp. It stays where it was dropped in space and shows the mission time at
  the drop. No range or bearing.
- **RULER** draws a distance, typically between two points.
- **COMPASS** sits around our own ship, to see where things are relative to us.
- Marks stay put in space. They are not pinned to our ship.
- Plots feed fire control later. For example, two timed points give a target's course and speed
  for the solution. That belongs to the fire-control step.

**For the tools step:** store marks in world km, so they stay put when the ship moves. Open
question to the owner: should a RULER snap to a nearby POINT?

For the helm later: the owner values the old build's **art**, not its code. That means a shader he
liked and the orange nav-wheel art.

**Standing end-of-feature check — the playtest (working, 2026-10-17).** Run it after every
feature: menu **Starwolf → Playtest → Run power-up playtest** (`Assets/Editor/Playtest/PlaytestRunner.cs`).
It:

- goes through the user flow: intro → menu NEW GAME → briefing → board;
- sets up the ship: opens Power from the systems rail, sends ENG/WEP/SEN/AUX to DRAW, waits for fill;
- checks readings at each step, and fails a step if its control is blocked on screen;
- films the run with Unity Recorder (package `com.unity.recorder` 5.1.7, owner's choice).

Each run writes `Logs/Playtests/<date_time>/`: `playtest.mp4`, `report.md` (pass/fail per step)
and `shots/`. It leaves play mode and reopens the scene you were in. First good run: 18/18 PASS,
22 s of 1080p video. Since Step 35: 27/27 PASS, 35 s, and it also checks pause, RUN, signal and
LOAD SHED. Since Step 36: 37/37 PASS, 37 s, and it also drops a POINT, a RULER and a COMPASS, moves
our ship and checks the marks stay put. Since Step 40: 48/48 PASS. It also checks the click sounds,
the LOAD SHED slide, a refused locked rocker, and the TRACK and RECALL presses. Since Step 41: 57/57.
It also drives the helm (ENG power, throttle, a turn, pause) and pans the plot. It grows as features land: add steps to `PowerUpRun()`. There is only one
Unity editor, so it runs when no other agent is using Unity.

What it can't do yet: the intro menu and the briefing's Enter read Unity's old Input class, which
a script cannot press, so it calls what those presses call (`Activate(0)`, `ScreenFlow.Advance`)
and says so in the report. Board controls are clicked for real, through the EventSystem.

Traps found setting it up: the recorder needs a moment after it stops before play mode ends, or
the MP4 has no index and won't play; and it must record at a **constant** frame rate — variable
rate fed the Mac encoder duplicate timestamps and it dropped frames. And the recorder switches
the Game view to its own "Recording Resolution (1920x1080)" and leaves it there, so the Game view
stopped filling the window. Since 2026-10-18 the playtest notes the Game view's size choice before
the run and puts it back afterwards.

**Game view size (owner, 2026-10-18).** Use **16:9 Aspect** in the Game view's size menu (top
left of the Game view). The board draws at the window's real pixel size, and its layout is designed
for 16:9. **Free Aspect** fills any window, but other shapes have not been checked. A fixed size like
1920x1080 gets shrunk to fit. For a bigger view, set the Game view's **Play Focused** menu to **Play
Maximized**. For true full screen, make a build: Player Settings are already Fullscreen Window at
the display's native resolution.

### Road to a playable demo — overview (2026-10-05)

**Superseded (2026-10-19).** Kept for its system inventory only. The live plan is the wiring order
above plus `FTUE_PLAN.md`. Where they differ, they win. In particular, the carrier and escorts, the
alert net, reinforcements, the jump point and a separate `WorldSim` are all out. The first mission
is one frigate, our ship with different stats, with `TacticalWorld` stepping inside the board tick.

**This is an overview only.** Each workstream gets its own deep dive later, before work starts on it.

**Where we are.** The `02.1_Grid` board looks and clicks right. Every panel reads and writes only
`HandoffBoardState`, and nothing touches a game system. There is no ship in a world, no enemies, no
mission and no way to win or lose. Contacts are fixed mock-ups. *(Since Step 34: a World with own
ship and one dummy enemy feeds the board; the mock contacts are gone.)* FIRE only drains the capacitor.
Hangar launch spawns nothing. The board's clock is its own and never pauses the game. *(Since Step 35: pause freezes the board
tick, the world and the clock.)* You cannot
pick a target from the plot (`PickTarget` has no caller, gap 95). PDC and missile controls are not
drawn (gaps 73, 76).

**What the demo level is.** One mission, about 15–25 minutes, from the GDD and the prototype:

- Drop in under a false ID.
- Find the flagship. In the prototype that is a command carrier with two destroyers and a cruiser.
- Kill it, then reach the jump point.
- The loop: hide, gather, act, get found, break away. Power raises your signature. Enemy awareness
  only goes up. Firing starts a 59 s alert net, then reinforcements arrive.
- You lose if the hull is gone, you stay outside the area for 30 s, the timer runs out, or life
  support fails.

**Workstreams.**

| Workstream | What it means | What we can reuse | Size | In demo? |
|---|---|---|---|---|
| World and contacts (DRADIS) | Real ships in a world, shown on the plot as tracks | `docs/reference-sim/radar.ts`, `world.ts`. Not `Radar.cs`, it reads true positions | L | Essential |
| Detection, signature, alert net | Enemies notice you from your signature; firing raises the alarm | `docs/reference-sim/` | M | Essential |
| Enemy AI and escorts | Patrol, alert, engage; escorts guard the flagship | Prototype behaviour. `EnemyShip.cs` is a skeleton only | M–L | Essential |
| Own ship moves from the helm | The helm and ENG power steer a real ship | `docs/reference-sim/ship.ts`. Not `PlayerShip` | M | Essential |
| Weapons in the world | Railgun slugs fly and hit; enemies shoot back | `docs/reference-sim/weapons.ts`. Not legacy `WeaponBase`/`WeaponManager` (see `Assets/Scenes/Tactical/TARGETING_AUDIT.md`) | L | Essential |
| Damage model | Hits damage the ship and show on the board | `HullSystem`, `InternalSubsystems` — but they do not match the board's 6 compartments | M, once chosen | Essential |
| Power wiring | Buses change speed, signature and charge | `HandoffBoardState` buses | S | Essential |
| AUX | The charge pool spent on abilities | `HandoffBoardState` | S | Owner decides |
| Comms payoff | A decoded intercept marks the carrier's zone | Comms panel | S | Essential |
| Mission script and level | Locate, eliminate, exit; place the ships | `QuestSystem` (nothing calls `StartMission` yet), `WorldBoundary` | S wire, M author | Essential |
| Win and lose screens | End the run and say why | `ScreenFlow` | S | Essential |
| Audio cues | Alerts, fire, hits | — | S | Essential |
| Hangar and fighters | Launch your own craft | Hangar panel only | M–L | Cut |
| Disguise | The false ID as a mechanic | — | — | Owner decides |

Also cut for a first demo: cloak, loadouts, the scanner minigame, terrain, most compartment side
effects, the mission timer, and missiles and PDC unless enemy fighters stay in.

Placeholder art is fine, so the 106 rows in `HANDOFF_GAPS.md` and the 8 buckets in
`CLAUDE_DESIGN_NEEDS.md` do not block a demo. The design blockers there are 8.1 (contacts on the km
scale), 8.2 and 8.3. The Claude Design request is still unsent.

**Clashes with the old `02_Grid` code.** Its rails and drawers (`BoardController`, `IconRail`,
`DrawerPanel`) are replaced; do not reuse them. `PlotTools` and its family duplicate the handoff's
marks; pick one. `PowerManager`'s five systems clash with the board's four buses. `PlayerShip`'s A/D
and Shift clash with the helm and comms. `PlotCamera`'s scroll wheel clashes with the board's zoom.
There are two clocks (`MissionClock` and the board's own), and `GameClock` is unused. `GameClock`
pauses through `Time.timeScale`, which would freeze the board's animation.

**Decisions the owner must make first.**

1. Damage model: hull sides, or the board's compartments?
2. Scale: one unit is one km, or the prototype's numbers? **Answered: 1 unit = 1 km (decision 13).**
3. What does firing cost?
4. Signature scale: the sum of the bars, or 0–100? **Answered 2026-10-17: signal, 0–100.0, fed by
   every unit of power plus ACTIVE SCAN (decision 20).**
5. Is the prototype's enemy AI the floor?
6. Which ship is the flagship: a carrier, or the board's cruiser, corvette or frigate?
7. The two alerts: the player's CONDITION and the enemy alert net. How do they relate?
8. Which lose conditions, and what shape is the arena?
9. Disguise: in or out?
10. Who owns game state? Recommended: a new world sim feeds `HandoffBoardState`. Do not revive
    `PowerManager` or `PlayerShip`. **Answered 2026-10-17: a new World object (decision 16).**
11. Does the CLAUDE.md `WeaponBase` rule still apply?
12. Do panels pause the game?

**Suggested order.** *Superseded by the owner's wiring order at the top of this section.*

1. The decisions above.
2. World sim skeleton, with contacts on the plot.
3. The helm drives the ship.
4. Power drives speed and signature.
5. Enemy AI and the alert net.
6. Railgun in the world, and damage.
7. Mission, win and lose.
8. Audio and polish.

### Unfinished from Step 27

1. **Nothing from Step 27's CRT work has been seen on screen.** The MCP bridge dropped first. Check
   the scanlines, the lit field, the screen edge, and that the menu now sits on black.
2. **The four corners of microtext are not built.** See Step 27 for why, and for the rule against
   inventing glyphs to do it.
3. **`Title` and `StartButton` in `00_Start` are deactivated, not deleted.** Delete them once the
   owner is happy with the intro.
4. **SETTINGS on the menu does nothing.** There is no settings panel. It logs and returns.
5. **LOAD GAME is dimmed** because `loadGameAvailable` is off. Turn it on when there is a save system.
6. `Assets/Screenshots/` collected a pile of verification PNGs during this step. Clear it out.

### Board follow-ups

From `UI_INTEGRATION_LOG.md`, section 10. In order:

1. **Audit the project's existing UI assets** against `CLAUDE_DESIGN_NEEDS.md`. **Done 2026-10-05,
   verified:** `ASSET_AUDIT.md`. Of 28 items, 5 fit as-is, 7 fit after a small change, 10 get partial
   help, 6 have nothing. Nothing is used yet; each use needs the owner's yes.
2. **The short Claude Design request**, when the owner asks.

### Clean-up once the game is working (owner, 2026-10-24)

Not now. Do these once the board plays end to end, in this order:

1. **Find what is in use and what is not.** Send an agent to trace every script, scene, prefab and
   asset: is it in a scene, used by code, or used by nothing? It writes a list. The owner picks what
   to delete. Nothing is deleted without the owner's yes.
2. **Drop the "Handoff" names.** `HandoffBoardState` becomes `BoardState`, `HandoffBoardControls`
   becomes `BoardControls`, and the panels lose the prefix too (`HandoffHelmPanel` becomes `HelmPanel`;
   the owner suggested "panel controls" for the shared base, `HandoffPanel`). Confirm the names
   with the owner first. Do step 1 first: the old `BoardController` (used only by `02_Grid`) would
   sit right next to the new `BoardControls`, which is confusing.

### Later — DRADIS (superseded for now)

**Superseded by the 2026-10-04 decision: DRADIS stays as the handoff has it, for now.** The notes
below are from before the handoff. Keep them for the deep dive.

Radar is called **DRADIS**. Use that name in the UI and in code.

- A circle around the ship with a radius. Anything inside gets tagged a **bogey**.
- It gives you a **position and a bearing**, refreshed on each sweep pass. **Not** bearing-only, so
  no two-bearing triangulation. Between passes the mark is stale — a ship doing 100 u/s has moved
  400 units since a four-second-old sweep. `docs/reference-sim/radar.ts` already has `predicted()`,
  `age()` and `uncertainty()` for this.
- **Switchable. Costs 1 power minimum. More power buys a bigger radius**, set by a dial on its pane.
- **Detection and identification are separate jobs.** DRADIS tells you *where*. Scanners tell you
  *what*. A contact stays an unidentified bogey until scanned.

### After that, in order

1. **Scanners** — identify a bogey that DRADIS has found.
2. **Targeting computer contents** — the old spec is
   `Assets/Scenes/Tactical/TARGETING_COMPUTER_SPEC.md`. The handoff's fire control drawer now covers
   most of it.
3. **Turrets and firing** — two railgun turrets, two PDC groups and one missile launcher (decision 11,
   2026-10-04; the handoff wins). Already written in TypeScript at `docs/reference-sim/weapons.ts`;
   needs porting to C# and changing to match.

### Carried over

1. **`UIController` references still need reconnecting.** The menu item is
   `Starwolf/Wire UI References`.
2. **Roughly 100 `UIController` references are unassigned in `02_Grid`.** The power bars were fixed
   by binding them directly. Other panels may still be showing static placeholder art for the same
   reason. **Weapons and ShipStats have not been checked.**
3. **The Scanner corner panel is switched off.** It will come back as the scanners system.
4. **Five of the six right-rail systems have no panel:** SENSORS, REACTOR, HANGAR, COMMS,
   ENGINEERING. The handoff lists these as open items too.
5. **The targeting drawer's lower half is empty.**
6. **Two pre-existing bugs, found and deliberately not fixed:**
   - The green plates `btn_emergencyVent` and `btn_blackAlert` have empty `onClick` lists, and
     `UIController.EmergencyVentBtn()` is never called.
   - Each power toggle fires two different state changes per click:
     `PowerManager.ToggleDrawState` via `ChargeBtn`, plus `ToggleSystemState` via a second
     persistent call.
7. **`sc[3]` and `sc[4]` both point at `Camera-Power`.** That looks like a mis-drag.
8. **The handoff's industrial layer is not built:** the left gutter conduit bundle, the right gutter
   access hatch, and the frame stencils `TCB-1 / REV D` and `CL II EQUIP — 480V DC`. All of it is
   20 to 26 percent opacity detail, confined to the frame gutters.

### Done since this list was last cut

Removed because they are no longer true: plot labels now exist, the rail shows the live tool, and
W/S/A/D were confirmed by the owner at the keyboard.
