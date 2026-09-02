# Tactical Scenes — Build Log

A running record of the new scene flow being built under `Assets/Scenes/Tactical/`.

**Read this first if you are a new session, or this session after a compaction.** It tells you what
exists, what is wired to what, which step we are on, and what to do next. You should not have to
work any of that out again.

Last updated: 2026-09-02. Unity 6000.5.10f1. Editor instance `Killshot@99ec1537206126d5`.

---

## 1 · Where we are

**Steps 1 to 23 are done.**

All three scenes are built. The intro flow runs end to end. The tactical grid is on screen in amber,
with the corner panels and both icon rails on top of it, and the outer metal frame around all of it.
The targeting drawer slides open from the right rail. The ship draws power and moves. The rest of the
board is not wired up yet.

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
| 23 | Plot layer, part two: annotation rendering and the seven tools | **Done** — every tool checked in play mode |

Steps 6 to 23 are written up in section 6. What is still not done is in **Open items**, at the very
bottom of this file. Read that before you pick up any work.

---

## 2 · Scenes

All three scenes live in `Assets/Scenes/Tactical/`. The play order is `00_Start` → `01_Briefing` →
`02_Grid`. They were added to Build Settings in that order and all three enabled.

| Scene | GUID | State |
|---|---|---|
| `00_Start.unity` | `e50db2cd323304012ba3e7afb4e531cb` | Built |
| `01_Briefing.unity` | `326d3f3159fcb4965bebcf61495595c6` | Built |
| `02_Grid.unity` | `7590f86ac5cb446509ffb65be8bb2360` | Built — grid surface only |

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
| `01_Briefing` | Its own root object | `02_Grid` | on | Enter key |

Note the script sits on **its own root GameObject**, not on the button or the canvas.

The chain was run in play mode twice: `00_Start` → click → `01_Briefing` → advance → `02_Grid`.
Both times it worked.

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
| `PowerManager` | Reused. Step 7 will read from it. |
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

26. Nothing else. The Unity console is clean: no errors, no warnings.

---

## 6 · Steps 6 to 23, in detail

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

Still not done. This is the list to work from.

1. **`UIController` references still need reconnecting.** The menu item is
   `Starwolf/Wire UI References`.
2. **The Scanner panel is still a corner panel.** Only Weapons has been moved to a drawer.
3. **The targeting drawer's lower half is empty.** The handoff specifies mount rows, three
   interlocks and a FIRE button there.
4. **Five of the six systems have no panel:** SENSORS, REACTOR, HANGAR, COMMS, ENGINEERING. The
   handoff lists these as open items too.
5. **Annotations have no on-plot text labels.** Range and bearing only come out through the
   `Readout` event, and nothing displays it yet.
6. **The tool rail buttons do not show an active state.** Nothing on the rail tells you which tool is
   live.
7. **Two pre-existing bugs, found and deliberately not fixed:**
   - The green plates `btn_emergencyVent` and `btn_blackAlert` have empty `onClick` lists, and
     `UIController.EmergencyVentBtn()` is never called.
   - Each power toggle fires two different state changes per click:
     `PowerManager.ToggleDrawState` via `ChargeBtn`, plus `ToggleSystemState` via a second
     persistent call.
8. **`sc[3]` and `sc[4]` both point at `Camera-Power`.** That looks like a mis-drag.
9. **W, S, A and D are not yet confirmed under a real keypress.** Someone has to press them in the
    editor.
10. **`UIController` still has roughly 100 unassigned references in `02_Grid`.** Other panels driven
    by it may be showing static placeholder art for the same reason the power bars were. **The
    Weapons and ShipStats panels have not been checked.**
11. **The handoff's industrial layer is not built:** the left gutter conduit bundle, the right gutter
    access hatch, and the frame stencils `TCB-1 / REV D` and `CL II EQUIP — 480V DC`. All of it is
    20 to 26 percent opacity detail, confined to the frame gutters.
