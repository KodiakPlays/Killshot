# Verification — Tactical board claims

Independent check. Read-only. Scene: `Assets/Scenes/Tactical/02_Grid.unity`, not in play mode.
Scene facts were read live out of the Editor. Code facts are file:line.

## Verdict table

| # | Claim | Status | Evidence in one line |
|---|---|---|---|
| 1 | WASD works, none of it newly written | CONFIRMED | `PlayerShip.cs` is clean in git; W/S at :531/:536, A/D via "Horizontal"; all six axes it asks for exist in `InputManager.asset` |
| 2 | Turning is multiplied by engine power | CONFIRMED | `PlayerShip.cs:619`; two further gates exist (stability, railgun standby) |
| 3 | Engine power fills to 5 of 5, Power panel bars show it | CONFIRMED | `engines.maxPower` is 5 in the scene; five bars wired to real Images that reach the screen through the `Screen-Power` render texture; material instancing is correct |
| 4 | UIController threw four times a frame and that stopped the power sync | PARTLY TRUE — fix incomplete | Three guards are real, but three unguarded null refs still sit before `SyncPowerBarsFromManager`. `Update` still aborts at `UIController.cs:263` |
| 5 | The seven plot tools all work | PARTLY TRUE | All seven ids route, UNDO/CLEAR are real actions, bearing maths is right, the rail chain is wired end to end — but `BoardController` has no `OnEnable`, so a disable/enable kills every button |
| 6 | World coords, screen-space drawing, constant pixel width | CONFIRMED (one caveat) | `PlotAnnotation.cs:17`, `PlotOverlay.cs:104`; widths are constant in *reference* pixels, not real ones, off 1920x1080 |
| 7 | Zoom 300 to 250,000, grid covers the view at max | CONFIRMED | Scene has minSize 300 / maxSize 250000; fitter gives 1,200,000 span against 888,889 needed |
| 8 | Ship glyph has a minimum size, true scale above it | CONFIRMED but overstated | `ScreenConstantScale.cs:89`; true scale only exists between zoom 300 and ~369, so in practice it is always a 170px glyph |
| 9 | Tuning asset applies to the scene and captures back | CONFIRMED, with gaps | Every field is applied and every target field name exists; but one field is a dead knob, and most new-component values are not in the asset at all |
| 10 | Anything broken and unclaimed | 5 real defects found | See "What is actually broken" |

---

## 1. WASD movement

CONFIRMED.

`git status` shows `Assets/Scripts/PlayerShip.cs` unmodified. Nothing in it was newly written.

- W is forward thrust — `PlayerShip.cs:531`
- S is reverse — `PlayerShip.cs:536`
- A and D turn, through `Input.GetAxis("Horizontal")` — `PlayerShip.cs:612`

Every axis the script asks for is defined in `ProjectSettings/InputManager.asset`:
`Horizontal`, `ControllerThrust`, `LeftTrigger`, `RightTrigger`, `DPadX`, `DPadY`. The
`Horizontal` axis lists `a` and `d` as its alt buttons (`InputManager.asset:14-15`), so A/D
really do drive it. The `EventSystem` uses `StandaloneInputModule`, so the legacy input
backend is live in this scene.

## 2. Turning is multiplied by engine power

CONFIRMED.

`PlayerShip.cs:619`

    targetTurnSpeed = turnInput * turnRate * enginePower * subsystemTurnMult;

`enginePower` is `currentPower / maxPower` (`PowerManager.cs:258`), so it is exactly 0 with no
engine bars. `targetTurnSpeed` goes to 0 and `currentTurnSpeed` decays to 0 at `:629`. The ship
cannot turn.

Two other things gate turning, which the claim does not mention:

- `PlayerShip.cs:614` — turning is skipped entirely when `stability.IsStabilityDepleted()`.
- `PlayerShip.cs:519` — `if (IsOnStandby) return;` bails out of the whole of `FixedUpdate`
  during railgun standby, so no turning and no thrust at all.

Worth knowing: with zero engine power the ship still crawls forward at 5 units
(`PlayerShip.cs:553`). So it drifts but cannot steer.

## 3. Power fills to 5 of 5 and the bars show it

CONFIRMED.

Scene values: `engines.maxPower` = 5, `reactorMaxPower` = 8, `powerFillRate` = 0.5.
`PowerManager.cs:110-127` fills a bar at a time while the reactor has 0.5 to spend, and stops
at `maxPower`. So 5 of 5 is reachable.

`PowerMeterBinding` sits on `PlayerShip/PowerMeters`, is enabled, and `power` points at
`PlayerShip`. All five meters are wired to real Images:

    engines -> UI/Canvas-Power/Power_Screen/GameObject/Power_ChargeMeters/Power_ChargeMeters_ENG/..._Mat
    arms    -> ..._WEP/..._Mat
    bay     -> ..._CRW/..._Mat
    support -> ..._SHD/..._Mat
    sig     -> ..._SEN/..._Mat

All five are active in the hierarchy and all five use the `Shader Graphs/UI_GradeDisplay`
shader, which is what `_PowerCur` / `_PowerMax` belong to.

Those Images do reach the screen. `Canvas-Power` is rendered by `UI/Camera-Power` into the
render texture `Screen-Power`, which is drawn by the RawImage at `Canvas/Panels/Power/Screen`.
That object is active. So these are the bars the player sees.

Material instancing is correct. `PowerMeterBinding.cs:46-51` replaces each bar's material with
`new Material(...)` in `Awake`. This matters: right now all five bars point at the same shared
`UI_GradeDisplay` asset, so without that line every bar would show the same number. The fix is
the right one.

Two small things it gets wrong, neither fatal:

- The new Materials are never destroyed. One small leak per play session.
- If `UIController.Update` is ever unblocked, `SyncPowerBarsFromManager` will write the same
  two shader properties on the same materials. Two writers, one material.
  `PowerMeterBinding` runs in `LateUpdate` so it would win, but it is a trap.

## 4. UIController exceptions

PARTLY TRUE. The diagnosis is right in spirit. The fix does not finish the job.

**The call-order claim is wrong.** `UpdateCompass` is not called from `Update`.
`ScanMovment` is (`UIController.cs:259`). The `UpdateCompass(float angle)` overload that
received a guard at `:1037` is never called from `Update` at all. The compass call that *is* on
the `Update` path is the six-argument overload at `:1009`, reached through `PollHullDisplay`
(`:239`, called at `:312`), and that one has no guard.

**The three guards are real and correct.** They are the only changes in the file:

- `UIController.cs:429` — `if (scannerSlider == null) return;`
- `UIController.cs:1037` — `if (compassRect == null) return;`
- `UIController.cs:1144` — `if (stabilityImg == null || stabilityShader == null) return;`

**Three unguarded null references still sit between the top of `Update` and
`SyncPowerBarsFromManager` at `:315`.** All three fields are NULL in this scene:

1. `:263` `RadarTest()` -> `:1204` `radarImg.material.SetFloat(...)`. `radarImg` is NULL.
   This is the first one hit, so `Update` aborts here on frame one.
2. `:305` `UpdateVelocity()` -> `:1081` `velocityMeterImg[0].material...`. The array has two
   slots and both are NULL. `playerShipRef` is also unassigned until `:1051` runs.
3. `:312` `PollHullDisplay()` -> `:1015` `sensorLoadInfo[0].text`. All four slots are NULL.
   `PollHullDisplay` guards on `hullSystem`, but `HullSystem` *is* on `PlayerShip`, so the
   guard does not stop it.

Net result: the power sync at `:315` still never runs. The three guards moved the throw from
line 259 to line 263 and changed nothing else. The Power panel works only because
`PowerMeterBinding` is a separate component on a separate path.

## 5. The seven plot tools

PARTLY TRUE.

**All seven ids route somewhere real.** `PlotTools.cs:56-69` handles `point`, `line`, `ruler`,
`compass`, `erase`, `undo`, `clear`. The scene's `Canvas/Rails/LeftRail_Tools` holds exactly
those seven `BoardAction` assets, all with `kind = Tool` and all with an icon.

**UNDO and CLEAR are genuinely actions, not modes.** `PlotTools.cs:63-69` calls
`overlay.Undo()` / `overlay.Clear()` and never touches `SetTool`, so `active` is unchanged.
CONFIRMED.

**The bearing maths is right.** `PlotAnnotation.cs:33` uses `Mathf.Atan2(d.x, d.y)`, arguments
in that order. A vector of (0,1) gives 0 degrees (north) and (1,0) gives 90 (east). CONFIRMED.

**The chain from rail button to `PlotTools.Press` is complete.** Traced:

- `IconRail.cs:95` — each generated Button gets
  `onClick.AddListener(() => { if (Clicked != null) Clicked(captured); })`
- `BoardController.cs:27-36` — `Start` calls `Hook`, which calls `rail.Rebuild()` and then
  subscribes `OnAction` to `rail.Clicked`
- `BoardController.cs:50-55` — a `Tool` action calls `tools.Press(a.id)`
- Scene: `BoardController.tools` = `Board` (the same object as `PlotTools`). `PlotTools.surface`
  = `Canvas/PlotSurface`, `.overlay` = `Canvas/PlotOverlay`, `.plot` = `Main Camera`. None null.
- `Canvas` has a `GraphicRaycaster`, `PlotSurface` is Canvas child index 0 (behind the rails),
  has a zero-alpha raycastable Image, and the `EventSystem` has a `StandaloneInputModule`.

The listener timing is fine. `Rebuild` happens before the subscribe, but the button's lambda
reads the `Clicked` field at click time, not at creation time, so nothing is lost.

**But the listeners are attached in `Start` and removed in `OnDisable`, with no `OnEnable`.**
See the defects list. This is the one real break in the chain.

## 6. World coordinates, screen-space drawing

CONFIRMED, one caveat.

`PlotAnnotation.a` and `.b` are `Vector2` in world units (`PlotAnnotation.cs:17-18`).
`PlotOverlay.ToLocal` (`:104-111`) runs each point through `plot.PlotToScreen` and then
`RectTransformUtility.ScreenPointToLocalPointInRectangle`. Widths (`lineWidth` 1.5,
`pointSize` 9) are applied after that projection, in rect-local units, so zooming does not
thicken anything. That is exactly what the claim says.

Caveat: the Canvas is `ScaleWithScreenSize` at 1920x1080, so "pixels" here means reference
pixels. On a game view that is not 1080 tall, a 1.5-unit line is 1.5 x (height / 1080) real
pixels.

Related, and slightly inconsistent: `PlotTools.erasePixelRadius` (14) and the drag-vs-click
threshold at `PlotTools.cs:116` both go through `PlotCamera.WorldPerPixel`, which uses the
camera's real `pixelHeight`. So the erase hit radius and the drawn mark size are measured in
two different kinds of pixel and drift apart off 1080.

## 7. Zoom range and grid coverage

CONFIRMED.

Serialized in the scene on `Main Camera`: `minSize` = 300, `maxSize` = 250000. The
`TacticalTuning` asset at `Assets/Art/Board/TacticalTuning.asset` agrees (300 / 250000, default
30000), so a Capture has been run.

`PlotGridFitter` on `TacticalGrid` has `plotCamera` = `Main Camera` and `margin` = 1.35. Its
current scale is 144000, which is exactly `30000 * 1.7778 * 2 * 1.35` — the formula is live.
At `maxSize` 250000 and aspect 1.7778 it gives a span of 1,200,000 against 888,889 of visible
width. It covers.

Ordering is right too. `PlotCamera` is `[DefaultExecutionOrder(-100)]` (`PlotCamera.cs:14`), so
its `LateUpdate` runs before the fitter's, and the fitter sees this frame's zoom.

Note: the *defaults* in `TacticalTuning.cs:43-45` are still 400 and 30000. A freshly created
asset would not match the scene.

## 8. Ship glyph minimum size

CONFIRMED but overstated.

`ScreenConstantScale` sits on `PlayerShip/3D`. Serialized: `plotCamera` = `Main Camera`,
`minPixels` = 170, `maxPixels` = 0 (no ceiling), `minScale` 0.05, `maxScale` 200000,
`baseHeight` 116.2967, `baseScale` (1,1,1).

The code does draw at true scale above the floor — `ScreenConstantScale.cs:89-91` only
overrides `wantedPixels` when `truePixels < minPixels`. And it is demonstrably live: the
object's current `localScale` is (81.21, 81.21, 81.21), which is exactly what the formula
gives at zoom 30000 on a 1080-tall view.

Where the claim is overstated: the crossover is at ortho size about 369
(116.2967 / 170 x 1080 / 2). The zoom floor is 300. So "true scale" only exists between 300 and
369 — a sliver at the very bottom of a 300-to-250,000 range. For every practical zoom the ship
is a fixed 170-pixel glyph, which is about 16% of screen height.

## 9. Tuning asset applies and captures

CONFIRMED, with gaps.

Every one of the asset's 29 fields is written by `Apply`, and every target field name exists.
Checked by name against the real scripts:

- `PlayerShip`: `rateOfAcceleration`, `turnRate`, `dodgeDistance`, `dodgeDuration`,
  `enginePowerDrainRate` — all present
- `PowerManager`: `powerFillRate`, `ventRate`, `reactorMaxPower` — all present
- `ShipStability`: all five stability fields — present
- `PlotCamera`: `minSize`, `maxSize`, `stepPerNotch`, `smoothing`, `followSmoothing` — present
- `ScreenConstantScale`: `minPixels`, `maxPixels` — present, and `ship.transform.Find("3D")`
  does resolve in this scene
- `TacticalGrid.mat`: all eight `_Cell/_Fine/_Major/_Line/_Fade` properties exist on the
  material

`Apply` and `Capture` are symmetric. There is no component value that `Apply` writes and
`Capture` fails to read back.

The gaps:

1. **One dead knob.** `enginePowerDrainRate` is written by `Apply`
   (`TacticalTuningEditor.cs:102`) into `PlayerShip.cs:15`, which is declared and never read
   anywhere in the file. `PlayerShip.cs:643` says so outright: "Engine power is NOT depleted by
   travelling — only hyperspeed drains it." Turning that dial does nothing. Meanwhile
   `hyperspeedDrainRate` (`PlayerShip.cs:20`), which *is* live, is not in the asset.
2. **Most of the new components are not covered at all.** Not in the asset:
   `PlotGridFitter.margin`, `PlotCamera.anchorZoomOnCursor`, `PlotOverlay.lineWidth` /
   `pointSize` / `ringSegments` / `dashLength` / `dashGap`, `PlotTools.erasePixelRadius` /
   `measureColour` / `markColour`, `ScreenConstantScale.minScale` / `maxScale`,
   `PowerMeterBinding`'s three node colours.
3. **`Apply` cannot be undone.** It calls `EditorUtility.SetDirty` but never
   `Undo.RecordObject` on the scene components. `Capture` does record the asset.
4. **`Apply` and `Capture` use `GameObject.Find`**, which only finds active objects, plus the
   hard-coded child name `"3D"`. Rename or deactivate either and it silently half-applies.

---

## What is actually broken

Real defects only, with file:line.

1. **`UIController.Update` still throws on frame one, and the power sync still never runs.**
   `UIController.cs:263` calls `RadarTest()`, which reaches `UIController.cs:1204`
   `radarImg.material.SetFloat(...)`. `radarImg` is NULL in `02_Grid`. That aborts `Update`
   before `SyncPowerBarsFromManager` at `:315`. Two more unguarded null refs sit behind it on
   the same path: `UIController.cs:1081` (`velocityMeterImg[0]`, both slots NULL) and
   `UIController.cs:1015` (`sensorLoadInfo[0..3]`, all NULL, reached through `PollHullDisplay`
   at `:312`).
   The Power panel only works because `PowerMeterBinding` bypasses all of this.

2. **Every rail button dies if the Board object is disabled and re-enabled.**
   `BoardController.cs:39-43` has `OnDisable` removing the `Clicked` listener, but there is no
   `OnEnable` to add it back. `Start` (`:23`) runs once and never again. Disable/enable the
   `Board` GameObject, or reload it, and no rail button does anything — silently, with no
   error.

3. **`PlotTools` fails silently when `surface` is unassigned.** `PlotTools.cs:75` —
   `if (surface == null) return;` in `OnEnable`, with no warning. Nothing subscribes, no tool
   responds to a click, and nothing in the console says why. Contrast `BoardController.cs:53`,
   which does log when `tools` is missing.

4. **`PlotTools` has no null guards on `overlay` or `plot`.** `PlotTools.cs:42`
   (`overlay.SetPreview`), `:64` (`overlay.Undo`), `:67` (`overlay.Clear`), `:116`
   (`plot.WorldPerPixel`). Both are assigned in `02_Grid`, so this does not throw today. It
   will the moment a tool is dropped into a scene where they are not.

5. **`PowerMeterBinding` leaks a Material per bar per play session.**
   `PowerMeterBinding.cs:46-51` creates six `new Material(...)` and never destroys them. Small,
   but it is a real leak, and the instances are also a latent conflict with
   `UIController.SyncPowerBarsFromManager`, which writes the same `_PowerCur` / `_PowerMax` on
   the same Images if that code is ever unblocked.

Cosmetic, listed for completeness, not counted above: `IconRail.Rebuild`
(`IconRail.cs:45-49`) uses deferred `Destroy` in play mode and then immediately builds new
children, so the rail draws two overlapping sets of buttons for one frame at `Start`.

Not defects, checked and clean: no missing scripts anywhere in `02_Grid`;
`PlotTools.OnEnable` / `OnDisable` subscribe and unsubscribe symmetrically; `PlayerShip.Awake`
(`:84-118`) resolves `powerManager`, `stability`, `hullSystem` and `internalSubsystems` by
`GetComponent` and adds them if absent, so the unguarded uses at `PlayerShip.cs:526` and `:614`
cannot throw.
