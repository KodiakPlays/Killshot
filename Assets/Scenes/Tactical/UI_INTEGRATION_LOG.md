# UI Integration Log

This log records how the Claude Design handoff is being built into Unity. The handoff lives in
`Assets/UI2/Claude Design handoff/export/`. It is a full picture of the tactical board, cut into
images, fonts and measurements. It is being built into a new scene,
`Assets/Scenes/Tactical/02.1_Grid.unity`. `BUILD_LOG.md` stays the pickup point for the whole
tactical board. This file is the detailed record for the handoff import.

---

## 1 · The goal

Every asset and every piece of UI from the handoff is in the scene. All of it is visible and
clickable. Each control shows how it behaves: it opens, slides, lights up or switches. None of it is
wired to the game yet.

Wiring to game systems comes after that.

---

## 2 · The plan, in order

| Step | What it is | Status |
|---|---|---|
| 28 | New scene and flow | **Done** — checked in play mode |
| 29 | Import settings for images and fonts | **Done** — verified |
| 30 | Board builder script | **Done** — verified on the third pass |
| 31 | Static board built, and compared to the reference picture | **Done** — verified on the third pass |
| 32 | Movement: drawers, helm, orders, periscope, loops | **Done** — verified, all 10 checks pass |
| 33 | Clickable controls showing their states, no game wiring. Split into 33a–33j | **Done** — the board build, Steps 28–33, is complete and verified |
| 33a | The board frame's controls | **Done** — verified on the second pass |
| 33b | The power drawer | **Done** — verified, all 7 checks pass |
| 33c | The sensors drawer | **Done** — verified on the second pass |
| 33d | The comms drawer | **Done** — verified on the second pass |
| 33e | The engineering drawer | **Done** — verified on the fourth pass |
| 33f | The hangar drawer | **Done** — verified on the second pass |
| 33g | The fire control drawer | **Done** — verified on the second pass |
| 33h | The inside of helm, orders and periscope | **Done** — 33h-1, the inside of the helm: verified on the second pass. 33h-2, the orders station: verified on the second pass. 33h-3, the periscope: verified on the first pass |
| 33i | Plot tools: POINT, RULER, COMPASS and ERASE marks, contact picking, lookAt from the passive bearing labels | **Done** — verified on the second pass |
| 33j | Control transitions: knobs, rockers, fader, bars, helm ring animate instead of snapping | **Done** — verified on the first pass |
| — | Final end-to-end QA pass | **Done** — FINAL QA: PASS. The board integration, Steps 28–33 plus final QA, is **COMPLETE** |
| — | `CLAUDE_DESIGN_NEEDS.md` — the verified list of everything Claude Design must provide | **Done** — verified |
| — | Ours-to-fix items (mark dashes, SENSOR RANGE label, ▸▶◀ font, gap-row corrections) | **Done** — verified |
| — | Audit of existing project UI assets against the needs list | **Done** — verified (AUDIT: ACCURATE). `ASSET_AUDIT.md`: of 28 items in buckets 1–3, 5 fit as-is, 7 fit after a small change, 10 get partial help, 6 have nothing. Nothing used yet; each use needs the owner's yes |
| — | Claude Design request (from `CLAUDE_DESIGN_NEEDS.md`, less what the audit lets us fill), then wiring to game systems | **Next**, when the owner asks |

---

## 3 · Decisions

Made by the owner on 2026-10-04. Do not reopen them.

- **The handoff wins on weapons and power.** Two railgun turrets, two PDC groups, one missile
  launcher. Four power buses: ENG, WEP, SEN, AUX. All of these can be changed later.
- **The fire sequence is wanted.** LOAD, ANGLE, BEARING, ARM, then the fire lever. The missile salvo
  knob is wanted too.
- **DRADIS stays as the handoff has it, for now.** The owner wants position and bearing plotted over
  time. Expect this to change.
- **Distances are in kilometres.** One world unit is one kilometre.
- **Alarm text stays as it is, for now.**
- **The grid tint switches between white and amber.** Not white and red.
- **Many values must become inspector settings.** Do not hard-code what someone will want to tune.
- **Build order:** static board, then movement, then clicks without wiring, then wiring.

Made by the owner on 2026-10-04, during Step 33i:

- **The board starts undamaged.** In the owner's words: "When the game starts, nothing is damaged.
  The UI did that to verify behaviour functionality. The state on load is all systems green, no
  damage, all rooms sealed."
  - `HandoffBoardState` compartments now load at integrity 5, sealed, with no fire or radiation.
  - The handoff's start state (MAG vented at 4, HAB RAD at 3, HGR FIRE at 2) is now only a test
    setup.
  - Any verifier's model must use the undamaged start.
  - In the Editor, before play, the drawer state groups still show the design's damaged snapshot
    text. The first refresh in play shows the real undamaged state.

---

## 4 · Step 28 — new scene

`02.1_Grid.unity` was made with:

- **Main Camera.** Orthographic. Clears to `#0A0C0E`. Has the AudioListener.
- **EventSystem** with a StandaloneInputModule.
- **`Board` canvas.** Screen Space - Camera, plane distance 5. CanvasScaler at 1920 × 1080, match
  0.5, reference pixels per unit 100.

It was added to Build Settings after `02_Grid`. In `01_Briefing`, `ScreenFlow.nextScene` was changed
from `02_Grid` to `02.1_Grid`. That is one line. `02_Grid` was not touched.

Checked in play mode: `Advance()` from `01_Briefing` lands in `02.1_Grid`. No console errors.

---

## 5 · Step 29 — import settings

New script: `Assets/Editor/HandoffImport.cs`. Menu: **Starwolf / Handoff / 1 · Apply import
settings**. It is safe to run again.

### Images

- All 228 PNGs in the package: Sprite, sRGB, alpha is transparency, bilinear, no compression, no
  mipmaps, clamp, full-rect mesh.
- Pixels per unit is 200 for `sprites/@2x`, and 100 for everything else. This is the README's rule.
- 9-slice borders are copied from `asset-map.csv`.
- **Changed in Step 31:** the board's sprites are now imported with sRGB **off**. This goes against
  the README's "sRGB on" on purpose. See Step 31, problem (a).

### Fonts

Four TextMesh Pro font assets, in `Assets/UI2/Generated/Fonts/`:

- Barlow Condensed Regular, Medium and SemiBold.
- Share Tech Mono Regular.

Each is dynamic SDF, sampling size 72, a 1024 atlas. Each holds 101 characters: printable ASCII plus
— − ° ± · ×. The fallback is LiberationSans.

### Helper shapes

The package does not ship these, so they were made in `Assets/UI2/Generated/Shapes/`:

- **`round_1`, `round_2`, `round_3`, `round_4`, `round_6`.** White rounded fills for drawn shapes
  with rounded corners. Drawn at 4× and imported at 400 pixels per unit.
- **`circle`.** For shapes with a 50% corner radius.
- **`plot_glow` and `plot_vignette`.** A radial ellipse centred at 50%, 48%, as `plot-grid.json`
  describes.
- **`scanline`.** One dark line every 3 px, tiled.

### Problems hit

1. **The first run crashed.** A new TMP font asset has an empty (null) fallback list. Fixed.
2. **Fonts looked broken on a second run. They were not.** TMP's `TryAddCharacters` reports
   characters that are already there as missing. Each font was checked: 101 characters each. The log
   message now counts characters instead.

3. **The helper shapes could not be used (found by the verifier).** They were imported in Multiple
   sprite mode with no sprites cut, so no Image could use them. Fixed: `WriteSprite` in
   `HandoffImport.cs` now sets Single mode.
4. **The 1 px rounded shape had an uneven corner (found by the verifier).** It was off by one pixel.
   Fixed: the corner is now measured from pixel centres.

### Checked

An independent verifier checked Step 29. It passed on the second pass.

- **First pass:** 7 of 8 checks passed. Check 5 failed: the helper shapes problem above.
- **Second pass:** all checks pass.
  - 228 package images with the right settings. 110 at 200 pixels per unit, 118 at 100.
  - All 227 borders from `asset-map.csv` match. 68 of them are not zero.
  - 4 fonts, 101 characters each.
  - 9 helper shapes, each a single sprite, with the right sizes and borders.
  - `.meta` files present.
  - No console errors.

### Still open

**▸ (U+25B8) is in no font in the project**, not even TMP's fallback. Only two Engineering status
lines use it. **▶ and ◀ are missing too**; the import script asks for them as well. Add all three to
the Claude Design request.

---

## 6 · Step 30 — board builder

**Done. Verified on the third pass by an independent reviewer.** Pass 1 found five defects. Pass 2
found three more. Pass 3: all checks pass. The defects are listed under "Checked" below.

### Files

- **`Assets/Editor/HandoffBoardBuilder.cs`.** Menu: **Starwolf / Handoff / 2 · Build static board in
  02.1_Grid**.
- **`Assets/Scripts/Tactical/HandoffPlotGrid.cs`.** Draws the plot's grid lines as one UI mesh.
  - Every default comes from `plot-grid.json`.
  - The tint switches White / Amber. Amber uses the red alphas, .34 and .62.
  - Zoom, the step list, line width and colours are inspector fields.

### What the builder does

- **It rebuilds the board from scratch.** It opens `02.1_Grid` and deletes any old board. It builds a
  new one under the `Board` canvas, named `HandoffBoard`, 1920 × 1080. It is safe to run again. A
  re-export of the package can be rebuilt the same way.
- **The board as it loads is visible.** This is the "default" state. It is split into panels, drawn
  back to front: Frame, PlotRing, Plot, AlarmStrip, TimeControl, ToolRail, SystemsRail, Console,
  Orders, Helm, Periscope.
- **Every other state is built but hidden.** That is the six drawers, helm open, orders open and
  periscope lowered. Each sits under its own hidden group, "State · name".
- **Panels come from a table of index runs.** The layout lists each record type in page order, so a
  run of records maps to one panel. The table was read off this export. If a re-export changes the
  page, the table must be updated. Ask Claude Design for a panel tag on every record.
- **The plot is rebuilt from `plot-grid.json`.** Its parts, back to front:
  - Ground: black, 6 px corner.
  - Grid: the new `HandoffPlotGrid` component.
  - Glow: `plot_glow`, ink.data at .05.
  - Scale: the axis numbers.
  - Mockup: the fixed fake contacts, such as own ship, FTR-2, BC-01 and Track 07. They sit in their
    own group, so one click hides them all.
  - Scanlines: tiled, black at .306.
  - Vignette: black at .437.
  - Inner edge: 1 px, white at .13.
  - Outside the plot: a 6 px dark ring, black at .35.
  - **Not built:** the plot's inset blur shadow. There is no blur tool.
- **The periscope is clipped.** Its parts sit inside a clip box 44 px tall, as the board does when
  the periscope is housed.
- **Sprites use the `@2x` files.**
- **Shadow margins** are chosen in this order:
  1. A per-side table, for the helm, the periscope housing and the orders bodies.
  2. The "N px margin" in the `asset-map.csv` notes.
  3. For 9-sliced sprites: a table of transparent pads measured in the PNGs (see D1 below).
  4. Image size minus element size. Only for sprites that are not sliced, and only when the x and y
     margins agree.
  5. None.
- **The three conduits are fixed compositions.** They are drawn at their own size from a measured
  top-left corner, not stretched to a box.
- **Lamps glow when lit.** The glow reaches 8 px past the lamp.
- **Drawing order inside a panel** is worked out, not given by the package. A record inside another's
  box draws after it. Then by type. Then by page order. **Check this on screen.**
- **Skipped on purpose:**
  - Drawn shapes with alpha 0.
  - The rail icons' vector parts, measured as rectangles. The icon images already carry them.
  - The plot ground and glow records. They are rebuilt from `plot-grid.json`.
- **Stand-ins.** The 36 "gradient" fills become flat grey: ink.stencilDim at 35% of their alpha. Each
  is named "STAND-IN …" so it is easy to find.
- **A reference picture for checking.** A hidden `ReferenceOverlay` holds the package's reference
  picture at 50%. Turn it on to compare side by side.

### Checked

**Pass 1 — five defects:**

- **D1. Margins were wrong for 9-sliced sprites.** The rule "image size minus element size" was used
  on them. But a sliced image's size is just the slice source. It is not the element plus its
  shadow. Fixed with a table of transparent pads, measured in the PNGs:
  - **2 px:** glass_r3, glass_r3_c, glass_r3_d, glass_r3_e, well_r2, well_r3, well_r4, bay_r4,
    bay_r4_b, rate_key_off, rate_key_on, alarm_strip.
  - **4 px:** key_flat_a, key_flat_b, plate_dark, plate_rail, plate_legend, bus_terminal,
    lever_slot_launch, well_reactor.
  - **8 px:** screen_thick_r3, screen_thick_r4.
  - The old rule now runs only for sprites that are not sliced, and only when the x and y margins
    agree.
- **D2. The conduit art was squashed.** The three conduits are fixed compositions. They are now drawn
  at their own size from a measured top-left corner.
- **D3. The sensors icon's second layer was too small.** It now gets the same 6 px margin as the
  first.
- **D4. One hangar shape had a 5 px corner, and there was no shape for it.** `round_5` was added.
- **D5. Unlit sensors lamps got a glow.** "lamp.phosphorOff" now counts as unlit.

**Pass 2 — three more:**

- **`periscope_carriage` lost its margin.** It now has a per-side margin of 12, 4, 12, 16. That is
  the measured inset of its body.
- **Two conduit positions were wrong.** The reviewer lined up each conduit's own pipes and clamps
  with the drawn records that measured them. Every anchor agreed to 1 px:
  - conduit_left (30, 708)
  - conduit_mid (146, 996)
  - conduit_right (1592, 702)
- **24 drawn records inside the conduits would have drawn grey stand-ins over the real art.** They
  are now skipped.

**Pass 3:** all checks pass.

---

## 7 · Step 31 — the static board

Built in `02.1_Grid`. **Verified by independent QA on the third pass.** Every text run is now 0.95–1.01
as bright as the reference. Mean difference from the reference picture is 3.65 out of 255. The QA
passes are in "Independent QA" below.

### Counts

All match the package:

- 436 sprites. That is 433, plus the 3 conduit compositions.
- 12 icons.
- 101 lamps. 36 are lit, with a glow.
- 461 text runs.
- 7 textures.
- 288 drawn shapes.

**Not built, by rule:** 12 drawn shapes with alpha 0. 13 rail-icon parts. 24 conduit parts. 2 plot
records, rebuilt from `plot-grid.json`.

**Stand-ins:** 26 gradient fills.

**Not built, for lack of data:** 28 drawn shadows. The plot's inset blur shadow.

No missing sprites. No unknown colours. No records left without a panel.

### Problems found on screen, and fixed

**(a) Every see-through pixel was about three times too bright.**

- Why: Unity in Linear mode blends see-through pixels in linear light. A browser blends them in
  gamma.
- Measured: a thin grid line read 80, against 22 in the reference. The alarm strip read 88, against
  29.
- The owner chose **"fix in the board only."** No project-wide change.
- How it works:
  - A new **Board Camera** draws the Board canvas into a texture the size of the screen. The texture
    is in a linear format.
  - The board's sprites are imported with sRGB **off**. This goes against the README's "sRGB on", on
    purpose.
  - A **Board Present** overlay canvas shows that texture through a small shader,
    `Starwolf/UI/BoardGammaToLinear`. The shader converts it once.
  - Main Camera now draws nothing itself.
- New files:
  - `Assets/Scripts/Tactical/GammaBoardPresenter.cs`
  - `Assets/UI/Shader/BoardGammaToLinear.shader`
  - `Assets/UI2/Generated/BoardGammaToLinear.mat`
- Clicks still line up, because the texture is the size of the screen.

**(b) After that, every tinted colour was one conversion too dark.** This hit text, icons and lamps.

- Measured: icon grey 168 read 100. That is exactly 168 converted to linear.
- Why: Unity converts UI vertex colours to linear once, whatever the canvas setting says.
- Fix: every board colour is written pre-converted (`Color.gamma`). Unity's own conversion then
  lands on the design's value.
- The plot grid component has a **"gammaBoard"** switch for this. It is on by default.
- Result: the clock, HELM, PAUSED, every rail icon and the lamps now match the reference to within 1
  step in 255.

**(c) Two-line labels broke in the wrong place.** TextMesh Pro wraps text differently from a
browser.

- In the design, each two-line label was two separate lines joined into one record.
- Fix: the builder now picks the line break whose widest line matches the measured width. It spaces
  the lines to fill the measured height.
- All six now break exactly as in the reference. For example "HDG 062° / V 1.84 km/s",
  "BOARD / POINT ALPHA" and "ASTEROID FIELD / RNG 27,000 km".

**(d) The axis legends read "X [1]000 KM".**

- Why: "X ( )" was one record. "1,000 KM" was a separate record inside it.
- Fix: the builder draws "X (" at the start of the box and ")" at the end.
- Both legends now match the reference.

### How close it is

Mean difference from the reference picture, out of 255:

- **18.3** before the colour fix.
- **About 5** after.

The worst areas are all in the plot centre. They are the stand-ins and the mockup glyphs that are
only rectangles: the radar sweep, own ship, the fighter wing and the predicted ghost.

### Independent QA

**Pass 1 found two real problems.**

- **D1 — the alarm strip read "3NEW" and sat 7 px left.** The package trims a leading space from
  text.
  - Fix: a start-aligned, one-line run is right-aligned in its box when both are true:
    - its box has at least 0.8 of a character of spare room;
    - another run in the same state ends exactly at its left edge.
  - So the spare room is in front of the text. "SUBJECT —" in the periscope has its spare room after
    it, so it is left alone.
- **D2 — small text was dim.** Text at 7–11 px peaked at about 0.72 of the reference.

Also done, from QA's suggestions:

- **The two large grey stand-in discs are built but hidden.** They are the radar sweep (drawn #34)
  and the periscope sight (drawn #38). They come back when real colours arrive.
- **Lamp glows now use 0.75 of the lamp's alpha.** The board's glows are rgba box-shadows at .7–.8.

**Pass 2 found two more.**

- **R1 — Share Tech Mono at 12–17 px was still dim.** It read 0.72–0.91. Barlow matched.
  - Fix: every Share Tech Mono run uses a thickened copy of its material (TMP face dilate 0.3).
  - File: `Assets/UI2/Generated/Fonts/ShareTechMono-Regular SDF Small.mat`.
- **R2 — the hidden State groups were filled while switched off.** A switched-off TextMesh Pro
  measures about a tenth of its real width. So two-line labels in drawers were never split, and five
  runs were wrongly right-aligned.
  - Fix: each State group is filled while switched on, then hidden.

**Pass 3: all checks pass.**

- Every text run's brightness is 0.95–1.01 of the reference. The eight worst from pass 2 all now read
  1.00.
- Mean difference from the reference picture: **3.65** out of 255.
- 2.9% of pixels are off by more than 20. All are in the known gaps below.
- Placement is exact to 0.001 px.
- 0 records missing or duplicated.

**QA's cosmetic note — the owner's call.** Thickened Share Tech Mono at 12–17 px carries a little more
ink than the browser. The clock shows it most. A dilate of about 0.2 above 11 px would be closer.

**Known gaps, confirmed by QA:**

- Mockup glyphs that are only rectangles: own ship, FTR-2, the predicted ghost. Also the stand-in
  lines.
- Mockup group opacity was not exported. "+45 s / PREDICTED POSITION" and "BRG 149" are too bright.
- Frame edges are 25–35% too bright. The full-board edge shadow (drawn #0) has no values.
- Lamp halos are a little large.
- Not built: the plot's inset shadow, the periscope sight canvas, and ▸.

### Trap for the future

**The board now screenshots as black.** MCP's screenshot tool renders Main Camera directly and skips
overlay canvases. To capture the board, render Board Camera and read its target texture.

**Never fill a TextMesh Pro while it is switched off.** It measures about a tenth of its real width.
Fill it while on, then hide it. See R2 above.

**Unity rounds halves to even.** `Mathf.Round` and `Mathf.RoundToInt` turn 4.5 into 4. The browser
turns it into 5. Anything ported from the browser must round with `HandoffBoardState.JsRound`. See
Step 33c, D1.

---

## 8 · Step 32 — board movement

Written and tested by me. **Verified by an independent verifier: all 10 checks pass.**

### Files

- `Assets/Scripts/Tactical/CssEase.cs` — the browser's cubic-bezier timing curves. Checked against an
  independent calculation. All six curves agree to 4 decimals.
- `Assets/Scripts/Tactical/HandoffBoardMotion.cs` — sits on HandoffBoard. The builder wires it.

### What moves

All timings come from UIStateSpec §9. All can be tuned in the inspector.

- **The six drawers** slide in from 376 px to the right over .42 s. They fade in over .3 s. Only one
  is open at a time. Opening one closes the others.
- **The periscope's clip box** drops from 44 to 656 px over .5 s. HOUSED becomes LOWERED.
- **The helm and orders station** open at once. The closed panel is swapped for the open one, as the
  board does. Their header latch slides 15 px, over .3 s and .22 s, with a small overshoot.
- **Loops:**
  - the alarm lamp blinks (1.1 s, dim phase .2);
  - the FTR-2 mockup drifts (21 s);
  - the predicted-position ghost and its labels pulse (.38–.62, 4 s).

**Nothing is clickable yet.** Step 33 hooks the rail keys, latches and headers to these.

### Checked, in play mode

- Start state is correct. All drawers are shut and invisible. The periscope is 44 px. Helm and orders
  are closed.
- A drawer slide, slowed to 30 s, sampled at 6.74 s: x read 104.1 against 104.1 expected. Alpha read
  0.352 against 0.352.
- Opening sensors closed power.
- Periscope, helm and orders all opened, with the right panels showing.
- The alarm lamp and the ghost label matched their cycles exactly.
- Picture of everything open: `Temp/HandoffShots/step32_all_open.png`.

### Independent check

All 10 checks pass.

- Timings and curves match UIStateSpec §9, line by line.
- The easing helper matches an independent calculation to 6 millionths.
- Drawers, periscope, helm, orders and loops were all sampled mid-motion. Each matched its curve.

Two small fixes after the check:

- The alarm lamp now stays lit for 52% of each blink, as the design's keyframes say. It was 50%.
- A tween now applies its first frame at once. So a latch can never flash at its resting place for
  one frame.

Left out on purpose: the helm "STABILITY" blink (`pw-core`). The design ties it to a low-stability
warning, which is game state. See `HANDOFF_GAPS.md` row 28.

### Not in Step 32

These follow game state, so their wiring comes later:

- the sensor envelope size;
- the radar sweep (its stand-in is hidden);
- rocker paddles and knob turns (Step 33).

---

## 8a · Step 33 — clickable controls

### The split

Agreed with the owner. Each part is verified before the next starts.

- **33a** — the board frame's controls.
- **33b–33g** — one drawer each: power, sensors, comms, engineering, hangar, fire control.
- **33h** — the inside of helm, orders and periscope.
- **33i** — plot tools: marks for POINT, RULER and COMPASS.
- **33j** — control transitions. The design animates many controls (UIStateSpec §9.1):
  - knob pointers, .2 s;
  - rocker paddles, .12–.14 s;
  - the fader cap, .12 s;
  - bars' widths, .4 s;
  - the helm heading ring, .12 s.

  Every drawer so far snaps. One pass over all drawers, once 33b–33h are in. It uses the same
  easing helper as Step 32.

### Step 33a — the frame's controls

Clickable, with no game wiring. **Done. An independent verifier passed it on the second pass.**

#### Files

- `Assets/Scripts/Tactical/HandoffHit.cs` — marks an image as a clickable, draggable or scrollable
  control, and says which control it is.
- `Assets/Scripts/Tactical/HandoffBoardControls.cs` — what each control does. Every value comes from
  UIStateSpec and `board-logic.js`. Every colour and timing is an inspector field.

#### What is clickable

The builder marks exactly these:

- 5 tool keys and 6 systems keys;
- the 6 drawer latches;
- the pause key and 4 rate keys;
- the alarm strip;
- the periscope carriage;
- the helm header (both layouts);
- the orders header. This is an invisible strip over the top 31 px of the orders body, in both
  layouts;
- the SCALE knob and the grid-tint selector;
- the plot, for the mouse wheel.

#### What they do

- **Tool keys** move the selected frame and bar, and light that key's lamp. ERASE pressed twice
  returns to SELECT. Marks come in 33i.
- **Systems keys** open their drawer, one at a time. The key's lamp is lit while its drawer is out.
- **A latch** closes its drawer.
- **PAUSE** switches between PAUSED and RUNNING, with its colours and lamp.
- **A rate key** sets the rate, unpauses, and lights.
- **The mission clock** ticks every 0.42 s by the rate while running. This is the board's
  `tickPower`.
- **Clicking the alarm strip** acknowledges the alarm and hides the strip.
- **The carriage** drops or houses the periscope.
- **The headers** open and close helm and orders.
- **Dragging the SCALE knob** zooms the plot: ×10 per 160 px, from 0.1 to 1. The drag is measured
  from where the press started: zoom = start zoom × 10^(movement since press / 160). This is the
  design's `scaleDown`. The mouse wheel zooms
  ×1.12 per notch. The knob's pointer turns about the knob. The readout shows x1.0, x0.32 and so on.
- **The tint selector** switches the grid between white and amber, with its pointer, lamps and
  labels.

#### Checked, in play mode

Each control was clicked at its screen position, through Unity's event system.

- Every click landed on the right control, at a 2183 × 1228 Game view. So the board, drawn off
  screen, lines up at other sizes too.
- The clock went from 02:03:32 to 02:09:12, exactly, over 68 ticks at ×5.
- A latch closed its drawer.
- Zoom: a −80 px drag gave 0.32. One wheel notch gave ×1.12. The pointer angle matched.
- The tint switched to amber.
- Picture: `Temp/HandoffShots/step33a_after_clicks.png`.

#### Traps

- **Unity once entered play mode with nobody asking.** Most likely a play command queued by an
  earlier verifier ran late. It was stopped. The scene was clean.
- **Real mouse input in the Game view can change the board between automated steps.** Tests now set
  their starting state explicitly.

#### Independent check

**Pass 1: 7 of 8 checks passed.**

- All 31 clickable objects hit themselves at their centre. Nothing else on the board takes clicks.
- Tool rail, systems keys and latches: correct.
- Time control: correct. At ×10 the clock moved exactly 19 ticks × 10 s. While paused it did not
  move at all.
- Alarm, periscope, helm and orders headers: correct.
- After the run, the board in edit mode was unchanged, pixel for pixel.

**It failed one: the SCALE knob drag.** The drag used each frame's mouse movement. `SetZoom` rounds
to 0.01, and that rounding swallowed small steps. 160 slow 1 px moves from 0.1 stayed at 0.10.

**The fix:** the drag now measures from where the press started, as the design's `scaleDown` does.
zoom = start zoom × 10^(movement since press / 160).

**Pass 2: passed.**

- 160 × 1 px from 0.1 gave 1.00. 160 × −1 px from 1 gave 0.10.
- Half-pixel steps, out-and-back drags and drags past the limits were all exact.
- The pointer's angle, and its distance from the knob centre, were exact.
- Spot-checks of everything else passed.

**Notes, not defects:**

- Every control takes the mouse wheel. So the wheel does not zoom over the alarm strip or the
  orders header, though both sit over the plot. Check against the design when the plot is wired.
  See `HANDOFF_GAPS.md` row 49.
- The plot's scale labels (−30, −20 …) do not change with zoom. They are plot content, for later.
  See `HANDOFF_GAPS.md` row 48.

### Not built yet

- The rail lamps' selected glow. It is not in `layout.json`. See `HANDOFF_GAPS.md` row 45.

### Step 33b — the Power drawer

The Power drawer works on its own, from the board's own rules. No game wiring. **Done. Verified by an
independent verifier: all 7 checks pass, no defects.**

#### Files

- `Assets/Scripts/Tactical/HandoffBoardState.cs` — new. The board's own rules, ported from the
  design's `board-logic.js`. Plain data that every panel reads. It grows one drawer at a time. For
  now it holds:
  - the four power buses: fill, mode, and which way the selector steps next;
  - the load shed;
  - the six compartments, with the board's starting values (magazine vented, hangar on fire,
    habitation irradiated, and so on);
  - the reactor cap. Reactor damage lowers it, never below 2;
  - the power part of the board's 420 ms tick. First it drains venting buses. Then it fills drawing
    buses. Then, if over the cap, it trims from the last bus.
- `Assets/Scripts/Tactical/HandoffPanel.cs` — new. A small base for each drawer. It gets the clicks
  the frame does not handle. It redraws after each change and each tick.
- `Assets/Scripts/Tactical/HandoffPowerPanel.cs` — new. The Power drawer, drawn from the state:
  - the header "N / 10 UNITS": red when none are left, amber at 2 or fewer, green otherwise;
  - "N OF 10 UNITS AVAILABLE" and its ten core cells;
  - the core integrity lamps and "5 / 5";
  - the load-shed rocker, its lamp and its note;
  - per bus: the five cells, units, mode word, mode lamp and selector pointer.
- `HandoffBoardControls` — changed. It holds the state. It runs the power tick every 0.42 s, paused
  or not; the design says so. It passes clicks it does not own to the panels.
- `HandoffBoardBuilder` — changed.
  - It now builds a glow for every lamp that can glow. Lamps that start unlit have their glow hidden
    until lit. There are 26 of them.
  - It marks the shed rocker and the four bus knobs as clickable.

#### Checked, in play mode

Real clicks and real ticks.

- The drawer starts at "SHED READY", "0 / 10 UNITS", "10 OF 10 UNITS AVAILABLE".
- A knob steps STBY → DRAW → back. It reverses at the end.
- With ENG, WEP and AUX on DRAW, the buses filled to exactly ENG 4, WEP 3, AUX 3. That is 10, the
  reactor cap. The header turned red. The core cells emptied.
- Throwing the shed set every bus to VENT. The knobs refused. The buses drained to 0.
- Releasing it set every bus to STBY, and "SHED READY".
- Picture: `Temp/HandoffShots/step33b_pair.png` (drawing / after shed).

**Not a defect:** a click inside a drawer while it is still sliding in lands on the plot. That is
correct. The drawer is not there yet.

#### Choices made because the package is silent

Each one is in `HANDOFF_GAPS.md`.

- Lit bus cells use the token "white" (tokens.json: "lit data-bar cells"). Row 47.
- Empty cells keep the measured colour. Row 47.
- Cells fill from the bottom. Row 50.
- The "about to fill" (pending) look is not shown. Row 47.
- The drawer draws from the board's starting state, not from the measured snapshot, which had the
  shed thrown. Row 46.
- The "CORE DERATED" note has no layout record. Row 51.

#### Independent check

**Passed: all 7 checks, no defects.**

- The verifier wrote its own Python model of the power rules. After real clicks and real ticks,
  Unity's state matched it in four runs:
  - all four buses drawing;
  - a mixed set of modes;
  - the shed thrown, then released;
  - reactor damage set at runtime, to test the cap at 4 and at 2, and the floor of 2 when vented.
- Ticks come every 0.42 s, one unit per tick. They keep running while paused, as the design says.

**Two notes, not defects. Both fixed in 33c:**

- **(a) Open drawers let clicks and the wheel fall through to the plot.** Now an invisible blocker
  covers the face of every drawer body, the open helm, the orders station open and stowed, and the
  lowered periscope housing.
- **(b) After a long stall, every missed tick ran at once.** The browser drops missed ticks. Now we
  do too.

### Step 33c — the Sensors drawer

The Sensors drawer works on its own, from the board's own rules. No game wiring. **Done. Verified on
the second pass.**

#### Files

- `HandoffBoardState` — changed. It now holds the sensor rules:
  - the board's five fixed contacts and their classes;
  - DRADIS and active scan;
  - gain 1–5. Sensor range is 12,000 km + 7,000 km per gain step;
  - UV hears to 55% of range;
  - the four bands;
  - the scan cone: half-width 15–90°, in 10° steps;
  - own emissions: up 9 a tick while active, down 6 a tick when not;
  - identify, by the cone and by clicking a contact row;
  - the sensor part of the tick: gain is capped by the SEN bus, and emissions move.
- `Assets/Scripts/Tactical/HandoffSensorsPanel.cs` — new. Draws the whole drawer from the state.
- `Assets/Scripts/Tactical/HandoffWedge.cs` — new. Draws the active scan cone on the dial. The
  package gives no colours for the board's cone. So it uses the cone colour in the design's code,
  rgba(143,195,58,.2). The fan starts at bearing minus half-width and spans the full width. Row 53.
- The click component — changed. It now also reports where the pointer is, and when it is released.
  The gain fader and the cone dial need this. The board turns screen points into layout px.
- `HandoffBoardBuilder` — changed.
  - It wires the drawer.
  - Drawn records with alpha 0 are now built as invisible pieces, not skipped. The unselected band
    rows need them, to light up. The default board still renders pixel-identical to the verified
    33b board.
  - Three contact-board rows: the measured row, and two copies 24.5 px apart. That is the pitch of
    the spectrum rows in the same drawer. The package gives only one row. Row 52.

#### Checked, in play mode

Real clicks and real ticks.

- After one tick, every readout matched the layout's measured snapshot exactly: 19,000 KM; 0%;
  RUNNING SILENT; gain 1; SEN BUS ALLOWS 1 OF 5; band counts 1/0/0/1; 062; 80; SET OFF; 1 ON THERM;
  BOGEY C22 · 149 · — · —. The gain cap sat at its measured place.
- Then the SEN bus was set full directly, standing in for the Power drawer:
  - Pressing at the top of the fader gave gain 5 and 47,000 KM.
  - ACTIVE SCAN went on.
  - Dragging the cone to 048 and releasing identified TRACK 07.
  - Emissions rose to 100%. "3 HOSTILE SETS HAVE YOUR RANGE" showed, and the lamp blinked.
  - The contact board read TRACK 07 · 048 · 24k · RESOLVING (lit), and BOGEY C22 · 149.
- Picture: `Temp/HandoffShots/step33c_drawer.png`.

#### Choices made because the package is silent

Each one is in `HANDOFF_GAPS.md`.

- Contact board rows are 24.5 px apart. Row 52.
- The cone on the dial is a solid fan in the cone colour. Row 53.
- The gain fader's lit track is a flat stand-in. Row 54.
- The 1 px inset edge on band rows and dials is not built. Row 55.
- "ARC °" is a wrapper label. The label holds "ARC" and the value carries the "°". Row 56.

#### Verification pass 1

**Passed except for two defects. Both fixed. See pass 2.**

- The verifier wrote its own Python model of the sensor rules. It matched Unity's byte for byte, over
  98 rule steps and 37 real UI steps.
- The start state matched the measured snapshot exactly.
- Hit testing, readouts, visuals, regression and the console all passed.

**(D1) The emissions meter showed 4 cells at 45%, not 5.** Unity's `Mathf.RoundToInt` rounds halves
to even: 4.5 gives 4. The browser's `Math.round` rounds halves up: 4.5 gives 5. Emissions reach
exactly 45% on the fifth tick, every time active scan starts.

**The fix:** one JS-style rounding helper, `HandoffBoardState.JsRound`. Every rounding in the handoff
scripts now uses it. That includes the SCALE knob's zoom: `Mathf.Round` also rounds halves to even.
The cone bearing now keeps a rounded 360 as 360, as the JS does.

**(D2) The "ARC 80°" readout had no degree sign, and collided at three digits.** "ARC °" is a
wrapper label. See `HANDOFF_GAPS.md` row 56.

**The fix:** the label holds "ARC". The value carries the "°".

#### Verification pass 2

**Passed. Both fixes work. No defects.**

- The verifier's model and Unity agreed byte for byte. That held over 109 rule steps and the full
  click sequence.
- E-SIG at 45% now shows 5 cells.
- Gain and cone at exact half positions match the browser.
- ARC reads cleanly at 30°, 80°, 100° and 180°.
- The SCALE knob's zoom matches the browser's rounding, over 16 drags.
- The default board is pixel-identical.

**One note, not a defect.** Two values differed from the browser by millionths of a half: the cone
angle, and zoom × 100. That is float precision. Both were removed anyway in 33d. Those two sums are
now done in double precision, as the browser does.

### Step 33d — the Comms drawer

The Comms drawer works on its own, from the board's own rules. No game wiring. **Done. Verified on
the second pass.**

#### Files

- `HandoffBoardState` — changed. It now holds the comms rules:
  - band 1–3;
  - frequency 1–100, in 0.1 steps;
  - the carrier: band 2, at 72.2;
  - lock within ±0.3 of the carrier;
  - the four key dials. Each steps forward or back, only while locked;
  - the message, enciphered with the key 22, 14, 11, 5 ("WOLF"), and deciphered with the dials.
- `Assets/Scripts/Tactical/HandoffCommsPanel.cs` — new. Draws the whole drawer from the state:
  - the header state;
  - the band selector and its lamps;
  - the tuning knob. Turn it like a real knob: up to ±26 per press, as the design does;
  - the frequency readout, the tuning indicator and the ring marker;
  - the 56 receiver bars, with the carrier peak on band 2;
  - the LOCK rocker, its lamps and its note;
  - the decrypt screen and the key dials. The dial just stepped shows red for 1.1 s.
- `HandoffBoardBuilder` — changed.
  - It wires the drawer.
  - **The receiver bars.** The package records them as boxes only. Measuring them shows they hang
    inward from the scope rim, radius 63 px. That is exactly the scope glass's radius
    (`glass_round_126`). So each bar's length can follow the design's rule. Every rebuilt bar sits
    within 0.39 px of its measured box. Row 58.
  - **Correction.** This log first said radius 62.5 px, and "within 0.25 px". That was wrong. At
    62.5 px the worst bar was 0.85 px off. The verifier found it. See pass 1, (D3).
  - **The 59 decrypt letters** are the measured letter runs, recoloured and re-lettered in place.
  - **A lamp's glow is now optional.** The build failed on the way: band lamps 2 and 3 have no glow
    in the layout. Row 57.

#### Checked, in play mode

Real clicks and real ticks.

- The start matched the measured snapshot: INCOMING SIGNAL; 34.0; NO CARRIER ON THIS BAND; NO
  CARRIER; key A; and all 59 ciphertext letters.
- Then:
  - band 2;
  - two turns of the tuning knob: 34.0 → 59.9 → 72.2;
  - LOCK;
  - the dials stepped to W, O, L, F.
- The header read MESSAGE READ. The note read CARRIER LOCKED — TRAFFIC IS ENCIPHERED. The label read
  PLAIN. The screen read "HOSTILE FLEET INBOUND VECTOR TWO SEVEN NINE HOLD STATION AWAIT RELIEF".
- The default board is still pixel-identical after the change.
- Picture: `Temp/HandoffShots/step33d_drawer.png`.

#### Choices made because the package is silent

Each one is in `HANDOFF_GAPS.md`.

- Band lamps 2 and 3 have no glow, so they do not glow when lit. Row 57.
- The receiver bars' geometry was worked out by measuring. Row 58.
- The key dials' glow and edge are not built. Row 59.
- The SIG lamp blinks while own emissions are tracked. The spec does not say which element. Row 60.
- The sensors emissions lamp keeps the emissions colours, though `board-logic.js` overwrites its
  value with the comms one. Row 61.

#### Verification pass 1

**Passed except for three defects. All fixed. Re-verification is running.**

- 10,006 automated checks over 30 play-mode snapshots, with real clicks and drags.
- The verifier modelled the rules in JS-style doubles. Everything matched that model except the
  three defects below.

**(D1) The frequency and the lock window were single-precision floats.** Tuned to exactly 72.5,
|72.5 − 72.2| came out 0.30000305, not 0.3. So the lock was refused. The browser accepts it. The
same error lit the SIG lamp at 60.2.

**(D2) The receiver bars' "near the tuning" test had the same float error.** It showed at the ±2
edge: 87 cases over the 0.1 grid.

**(D3) The rim radius was wrong.** At 62.5 px the worst bar was 0.85 px off. At 63 px, the scope
glass's radius, every bar sits within 0.39 px. Row 58.

**The fixes:**

- Frequency, carrier, lock window and every comms comparison are now in double, as the browser
  does.
- Rim radius 63 px.

**Also changed, from the verifier's notes:**

- The SIG lamp now blinks while own emissions are tracked. UIStateSpec §9.2, line 893. The spec does
  not name the element. We took it from the order of the lines. Row 60.
- The dial just stepped stays red for 1.1 s. That timer now restarts on every dial click, even a
  refused one, as the JS does.

#### Verification pass 2

**Passed. All three fixes work. No defects.**

- Every 0.1 tuning step on all three bands was checked: 2,973 states. Each one matched the browser
  exactly. That covers the frequency, lock accepted or refused, the SIG lamp, and all 56 bar
  colours. 0 mismatches.
- 72.5 now locks.
- All 56 bars sit within 0.386 px of their measured boxes.
- The SIG lamp blink and the dial timer restart both behave as described.
- 26 snapshots, 8,670 element checks, 0 failures.

**Changed after verification:**

- The frequency field's inspector slider was removed. Unity's Range does not take doubles.
- A blinking lamp's glow now dims with the lamp, and keeps its 0.75 strength. In the browser, CSS
  opacity on the lamp dims its glow too. This applies in Sensors and Comms.

#### Traps

- **Browser numbers are doubles.** Anything compared against a threshold must be computed in double,
  not float. That means lock windows, ranges and rounding.

### Step 33e — the Engineering drawer

The Engineering drawer works on its own, from the board's own rules. No game wiring. **Done —
verified on the fourth pass.**

#### Files

- `HandoffBoardState` — changed. It now holds the engineering and damage-control rules:
  - the selected compartment;
  - the alert condition. It is set later, on the orders station. Condition 1 doubles repairs;
  - the damage-control party. It works 6 ticks, then adds integrity;
  - REBOOT, REPAIR, SEAL and VENT, each with the design's exact refusal reasons;
  - the 400 ms spring-back of a thrown rocker. It is timed in one place, so the orders station can
    share it;
  - the party's step in the board's tick.
- `Assets/Scripts/Tactical/HandoffEngineeringPanel.cs` — new. Draws the whole drawer from the
  state:
  - the compartment screen: each row's integrity ring, atmosphere, condition and crew;
  - the schematic cells;
  - the hazard line, the degraded line and the party line;
  - the ACTING ON selector;
  - the four repair rockers and their lamps;
  - the blanking plates.
- `HandoffBoardBuilder` — changed.
  - It wires the drawer.
  - **The integrity rings.** Each is now five wedges, 72° each with a 7° gap, starting at 9 o'clock,
    as the design's code draws it. They replace the grey stand-in. Row 64.
  - **Row backgrounds.** Every compartment row gets one, 40 px apart. Only the reactor row's was
    measured. Row 63.
  - Schematic cells, row backgrounds, the ACTING ON knob and the four fitted rockers are clickable.
- The default board is still pixel-identical after the change.

#### Checked, in play mode

Real clicks and real ticks.

- The start matched the measured snapshot exactly:
  - the six rows;
  - "3 COMPARTMENTS IN HAZARD · COMPLEMENT 52";
  - the degraded line;
  - "DC PARTY ▸ STANDBY";
  - REACTOR;
  - "VENT AVAILABLE";
  - the rocker states.
- Then:
  - Picked the burning hangar on the schematic. It read "REBOOT · REPAIR · VENT AVAILABLE".
  - Sent the repair party: "DC PARTY ▸ HANGAR BAY 0 OF 6". A second press was refused: "DC PARTY
    COMMITTED".
  - After 6 ticks the hangar went from integrity 2 to 3, and left the degraded line.
  - Vented the hangar. The fire went out. It came back on the degraded line as "flight ops down": a
    vented bay counts as zero.
  - Picked the magazine row. Sealed it. Rebooted it to integrity 5. Hazards dropped to 2.
  - The ACTING ON knob stepped to the next compartment.
- Picture: `Temp/HandoffShots/step33e_drawer.png`.

#### Known gaps, visible on screen

- The ▸ glyph does not draw. Row 30.
- The schematic's ship outline is only a dark box. Row 62.

#### Choices made because the package is silent

Each one is in `HANDOFF_GAPS.md`.

- The schematic is built from its two measured rectangles. Row 62.
- One row background per compartment row, 40 px apart. The cells' inset ring is not built. Row 63.
- The integrity rings are five wedges from the code's values. Row 64.
- The party line is right-aligned, so a long line grows leftward from the screen edge. Row 65.

#### Verification pass 1

**Passed except for two defects. Both fixed. Re-verification is running.**

- Rules, hit testing, readouts, regression and console all passed.
- The verifier built its own model of the rules. It matched Unity over a 77-step sequence, 78
  snapshots. That covered:
  - every repair on every compartment, and every refusal;
  - parties at alert 0 (+1 integrity) and alert 2 (+2);
  - vent putting out a fire;
  - seal and reboot;
  - the ACTING ON selector wrapping round.

**(D1) Runs of spaces.** The degraded line and the party line kept runs of three spaces from the
design's code. The browser collapses a run of spaces to one. So at the start "NO ORDNANCE" ran into
"DC PARTY".

**(D2) The party line ran off the screen.** With a party out, "DC PARTY ▸ HANGAR BAY 1 OF 6" ran
past the screen's edge. The package has no template for this. Row 65.

**The fixes:**

- One helper, `HandoffPanel.Html`, collapses runs of spaces as the browser does.
- The party line is the right-hand part of the status row. It is now right-aligned, and grows
  leftward from the screen edge.

#### Verification pass 2

**The space fix worked. The party fix caused a new defect. Fixed.**

- The degraded line now matches the layout, with single spaces. It fits its 204 px.
- Right-aligning the party line moved its overflow to the left. With a party out, "DC PARTY ▸
  HANGAR BAY 1 OF 6" printed over the end of the degraded line, by about 40 px.

**The fix:**

- The party line keeps its real width, at the right of the status row.
- The degraded line takes the room to the left of it, less the measured 10 px gap.
- If the degraded line does not fit, it ends in "…".

#### Verification pass 3

**That fix made the degraded line vanish in every state. Fixed.**

- TextMesh Pro's ellipsis mode also cuts vertically.
- The line's box is 9.00 px tall, as measured. That is 0.02 px shorter than the line itself. So TMP
  showed nothing.
- The edit-mode board never runs the panel. So the pixel check could not see it.

**The fix:** the line's box now grows to the line's own height. The box is centred, so the
baseline holds.

#### Verification pass 4

**Passed.**

- The degraded line draws in every state.
- When it does not fit, it ends in "…" before the party line, with a 13.5 px gap.
- It keeps its baseline.
- The 77-step model comparison still matches.
- Console clean. Scene clean.

#### Traps

- **The board's code sometimes builds strings with runs of spaces.** They never show in the
  browser, which collapses them. Pass panel text through `HandoffPanel.Html`.
- **TextMesh Pro's Ellipsis and Truncate modes cut vertically as well as horizontally.** A measured
  box a hair shorter than its text hides the text completely. Give such a box at least the text's
  preferred height.

### Step 33f — the Hangar drawer

The Hangar drawer works on its own, from the board's own rules. No game wiring. **Done. Verified on
the second pass.**

#### Files

- `HandoffBoardState` — changed. It now holds the hangar rules:
  - the two squadrons and their craft;
  - the launch order;
  - the six launch stages, each with its own fill rate;
  - the bay doors;
  - the launch bearing. It is typed. Non-digits are stripped, and it wraps to 0–359. It has a 60°
    cone;
  - the launch lever;
  - launch and recall;
  - the launch step in the board's tick;
  - the ship's heading, 083. The launch cone needs it. The helm (33h) will use it.
- `Assets/Scripts/Tactical/HandoffHangarPanel.cs` — new. Draws the whole drawer from the state.
- Pieces with no layout record were created, and flagged in `HANDOFF_GAPS.md`:
  - the six stage loading-bar fills, the CAT CHARGE fill, and the lever's green track. All are at 0
    in the snapshot. They sit in the measured wells, coloured from the code's inks. The lever track
    is a stand-in. Row 66.
  - craft 06's fuel bar. Its fuel is 0. Row 67.
  - the launch-bearing input field. The measurement skipped the input's value. It sits flush
    against its "°" label, 32 px wide, as the input site says. Row 68.
- Right-hand readouts whose text grows are right-aligned. For example the squadron state, "IN BAY"
  → "AIRBORNE — DESTROY", and the lever hint, "SAFE" → "AIRBORNE". Row 69.
- Two labels that a panel rewrites, and that wrap onto two lines in the design, now wrap inside
  their measured width:
  - the hangar interlock (text #356);
  - the sensors "SEN BUS ALLOWS N OF 5" (text #145). **This one had been running past its box since
    Step 33c.**

#### Checked, in play mode

Real clicks and real ticks.

- The start matched the measured snapshot:
  - 7 CRAFT READY, SEALED, 1.0 ATM, HOLD, 0%;
  - IN BAY, 4 OF 6 READY, 3 OF 4 READY;
  - INTERCEPTOR, 21°, DESTROY;
  - the interlock, SAFE, bearing 062;
  - the fuel bars, to the pixel.
- Then, with flight ops released, the hangar and magazine repaired, and AUX powered. These were set
  directly: their controls are on the orders station, engineering and power.
  - Opened the bay doors.
  - Threw the six stages in order. Each was refused until the one before reached 100%. Each filled
    at its own rate.
  - The catapult charged. "ALL SYSTEMS GO", "READY".
  - Dragged the lever to 95%. The interceptors launched. On release the lever sprang back.
  - The board read AIRBORNE — DESTROY, 7 READY / 1 AIRBORNE, and "SQUADRON AIRBORNE — RECALL TO
    RECOVER".
  - RECALL showed its pressed key while held. It brought the squadron home, and cleared the stages.
- Picture: `Temp/HandoffShots/step33f_drawer.png`.

#### Choices made because the package is silent

Each one is in `HANDOFF_GAPS.md`, rows 66–72.

- Fills with no record: built in the measured wells, from the code's inks. Row 66.
- Craft 06's fuel bar: built to match the others. Row 67.
- The launch-bearing input: placed by its "°" label. Row 68.
- Growing right-hand readouts: right-aligned. Row 69.
- Craft tags and full fuel bars share a colour. Left as measured. Row 70.
- The squadron bezels: the selected one is a stand-in; the unselected one is not recorded. Row 71.
- Bay status lamps #63–#65: driven by doorInk, pressInk and catInk, inferred from their colours.
  Row 72.
- Lamps #67–#75 light with no glow. The package recorded them unlit. Row 57.

#### Verification pass 1

**Passed except for one defect. Fixed. One board-wide problem also found and fixed.**

- The verifier built its own model of the hangar rules, in Python. It matched Unity exactly over
  128 steps.
- 83 readout dumps and 270 per-tick samples matched.
- Stage timing matched the design's table exactly: CREW 5 ticks, FUEL 6, ORDNANCE 8, EVAC 5,
  DOORS 4, CATAPULT 9.
- The lever fires once, past 0.9, and springs back.
- Hit testing, regression and console passed.

**(D1) The header ran off its screen.** With craft airborne, "7 READY / 1 AIRBORNE" ran about 37 px
past the edge of its screen.

**The fix:** the header screen now grows with its readout. It keeps the measured 9 px padding each
side, and stays anchored on the left.

**(D2) Click areas were too big, board-wide.** Every control's click area included its image's
baked shadow. That reached up to 14 px outside the visible edge.

**The fix:** the builder records each image's shadow margin. It trims the click area back to the
element, using Unity's raycast padding.

#### Verification pass 2

**Passed.**

- The header fits its screen in all three readouts.
- 81 controls were probed 1 px inside and 1 px outside every edge. That covered the frame, all six
  drawers, and the open helm, orders and periscope.
- No hit outside. No miss inside, except where another part of the board is really on top.
- Every control's click area matches its layout record within 0.6 px.
- Everything still clicks.

#### Traps

- **An image's click area includes its baked shadow** unless the builder trims it. Keep the
  recorded shadow margin when adding controls.

### Step 33g — the Fire Control drawer

The Fire Control drawer works on its own, from the board's own rules. No game wiring. **Built and
verified on the second pass.**

#### Files

- `HandoffBoardState` — changed. It now holds the fire control rules:
  - the five mounts and three weapon systems, as the owner decided: two railgun turrets, two PDC
    groups, one missile launcher;
  - the warheads;
  - the arming ladder: LOAD → ANGLE → BEARING → ARM;
  - a firing solution per system;
  - the PDC loaders and quadrants, and the missile salvo. State only;
  - the capacitor. It gains 5 a tick per WEP unit, up to 100. With no WEP it bleeds 3 a tick;
  - the fire lever;
  - the periscope bearing. 062 until the periscope is built in 33h;
  - the target. Track 07 until the plot is wired in 33i;
  - every gate from the design's code: WEP 2 or more, the magazine, Condition 2, a solution, the
    PDC loaders, the periscope within 4° of the target, and the charge at the shot cost.
- `Assets/Scripts/Tactical/HandoffFireControlPanel.cs` — new. Draws the whole drawer from the state.
  The SIGHT key lowers the periscope, through the movement component.

#### Checked, in play mode

Real clicks and real ticks.

- The start matched the measured snapshot exactly:
  - RG-A, 062, OFF −14.0°;
  - RAILGUN TURRET A, ARC 042° ±80°, 30,000 km, RAIL 2 / 2 · SLUGS 22;
  - NO TRACK, WEP BUS DEAD — CAPACITOR BLEEDING OFF, 70% PER SHOT;
  - NOT RELEASED / LOCKED ×3, NO TRACKING ID — NOT IDENTIFIED;
  - the shot mark at its measured place.
- Then, with Track 07 identified, Condition 2, the magazine sealed, WEP powered and the periscope on
  048. These were set directly: their controls are elsewhere, or come later.
  - TRACK computed TK-048-T07, 24,300 km, −35°, 1.10, ON BEARING.
  - LOAD, ANGLE and BEARING armed. ARM waited for the charge, then armed at 100%.
  - The board read SOLUTION LOCKED — MOUNT READY.
  - Pulled the lever past 90%. It fired: ROUNDS AWAY. The charge fell from 100% to 30%, one 70%
    railgun shot. The RAILGUN lamp went red. The lever sprang back.
- Cycled the system:
  - PDC: PDC-A/B, ARC 042° ±35°, 2400 RDS · LOADERS OFF, 25% per shot. The mark moved to cell 3.
    The ladder cleared.
  - MISSILE: the single VLS mount, ALL ROUND, 40,000 km, HE · SALVO 4 OF 8. The mark sat at cell 4.
  - Back to RAILGUN. Its solution was kept.
- The default board is pixel-identical.
- Picture: `Temp/HandoffShots/step33g_drawer.png`.

#### Choices made because the package is silent

Each one is in `HANDOFF_GAPS.md`, rows 73–76.

- The drawer was measured on RAILGUN only. So the PDC controls (the quadrants and LOADERS) and the
  missile controls (the warhead selector and the SALVO knob) have no records. They are not drawn.
  Their state still shows in the readouts. Row 73.
- The shot-cost mark moves to the boundary after the right cell for each system. Only the RAILGUN
  place is measured. Row 73.
- The REL BRG value carries its own "°". The separate "°" run is hidden, as with ARC in sensors.
  Row 74.
- Lit capacitor cells use the token "white". The fire lever's red track is a stand-in. Row 75.
- The one-mount missile system needs a single detent tick on the mount selector. It has no record.
  Row 76.

#### Verification pass 1

**NOT VERIFIED. Checks 2–7 passed. Check 1, port fidelity, failed on D1. Fixed.** Evidence is in
`Temp/HandoffQA/step33g/`.

**(D1) The lever could fire only once.** `HandoffBoardState.Fire()` returned early when `fired` was
already true. So a second pull did nothing while the ladder stayed set. In `board-logic.js`,
moveSlew fires on every new pull while allSet() holds. Only the per-pull `_shot` latch stops
repeats.

**The fix:** the guard is now `if (!AllSet) return;`. The panel's `shotThisPull` is the per-pull
latch.

**(D2) Rounding at exact halves. Minor, cannot happen in play, no action.** `sol.spd` is a float, so
a value like 1.115 can round differently from JS `toFixed(2)`. Every track speed has two decimals
and the steps are ±0.05, so it never occurs.

**(D3) The SIGHT lamp lagged.** Clicking the periscope carriage (`case "scope"` in
`HandoffBoardControls.Click`) updated the lamp only on the next tick, up to 0.42 s later.

**The fix:** that case now calls `RefreshPanels()`.

#### Verification pass 2

**STEP 33g: VERIFIED.**

- A pull fires exactly once. Dragging back and forth within the same pull does not fire again.
- A second pull fires again, drawing RG 70 / PD 25 / MS 40.
- The SIGHT lamp changes in the same call (666C71↔C5E383).
- The edit-mode board is pixel-identical (0 px differ) to `edit_33g.png`, before and after play. The
  new render is `Temp/HandoffQA/step33g/reverify_edit.png`.
- Console clean, play mode exited cleanly, scene not dirty.

#### Traps

- **Port per-pull latches exactly.** A JS latch reset on each pointer-down (`_shot`) is not the same
  as a state flag (`fired`). Guarding on the state flag blocks repeat actions the design allows.

### Step 33h-1 — the inside of the helm

The helm works on its own, from the board's own rules. No game wiring. **Done — verified on the
second pass.**

#### Files

- `Assets/Scripts/Tactical/HandoffHelmPanel.cs` — new. Draws the helm from the state.
- `HandoffBoardState` — changed. It now has a helm section:
  - `thr`, 40 at load;
  - `apAtt`, "NOSE" at load. "" means no hold;
  - `apMode`, and `apRange` 2 with its list `apRanges`;
  - `ThrCap`, `GLoad`, `Stab`, `TurnLeft`;
  - `SetThr`, `SetAtt`, `SetApMode`, `StepApRange`, `StepOrdHdg`, `TickHelm`;
  - two helpers: `JsRound(double)` and `JsToFixed`.
- `HandoffBoardControls` — changed. The tick calls `TickHelm`.
- `HandoffBoardBuilder` — changed. `WireHelm`, and `InsetEdge`, which builds a 1 px box-shadow
  inset edge as four child lines.

#### Controls

- The throttle fader: press and drag on the well. The cap is read once, at the press.
- The four attitude hold keys. Each toggles.
- Approach range − and +.
- The three approach mode rows.
- The A and D keys step the ordered heading by 3° and clear the hold. They are ignored while a text
  field has focus. A held key repeats after 0.5 s, then every 0.033 s.

#### Readouts

- The closed and open header rows: HDG, the turn label, THR, G, AUTO / MANUAL and the lamp.
- The heading ring and the dial text.
- The cap and the track.
- The key colours and edges.
- The mode rows.
- The range label and its ink.
- The nine stability cells, STAB % and G LOAD in gInk.

#### The tick

- The throttle is clamped to ENG × 20.
- The heading swings max(1, round(4 × engFactor)) a tick.

#### Checked, in play mode

The builder's own play test. Real clicks, keys and ticks.

- ENG 3, a drag to 75%: thr 60, the cap.
- STAB 73% at 1.6 G.
- EVASIVE: 2.8 G in amber, STAB 19%, 2 cells lit.
- Range + four times: 24k KM, with + dimmed.
- A four times: PORT 012° in amber. The header row reflowed.
- The heading swung to 071.
- The default edit-mode board is pixel-identical (0 px differ) to `edit_33g.png`. The render is
  `Temp/HandoffShots/edit_33h1.png`.
- Picture of the open helm: `Temp/HandoffShots/33h1/open_evasive_port.png`.

#### Choices made because the package is silent

Each one is in `HANDOFF_GAPS.md`, rows 77–81, and row 28.

- The header row has no template. Built as a left-to-right reflow by character count. Row 77.
- gInk drives the 04 STABILITY G LOAD readout only. The header G stays ink.data. Row 78.
- Unlit stability cells use lamp.offPanel #22262A. Row 79.
- The held attitude key's outer glow is not built. The 1 px inset edges are. Row 80.
- The A / D repeat is 0.5 s, then 0.033 s. Both are in the inspector. Row 81.
- `hazShow` (stab < 50) shows an element that is not in the export. Not built. Row 28.

#### Verification pass 1

**NOT VERIFIED. Checks 2–7 passed. Check 1, port fidelity, failed on D1. Fixed.** Evidence is in
`Temp/HandoffQA/step33h1/`.

**(D1) G LOAD toFixed did not match at 3 of 303 states.** DIRECT thr 45 showed 1.5, not 1.4.
DIRECT thr 95 and FLANK thr 45 showed 2.0, not 1.9. `JsToFixed` went through G17, which rounds
1.4499… up to 1.45.

**The fix:** `JsToFixed` now rounds on the double's exact binary value, with BigInteger.

**(D2) The heading swing read the engine room after a repair had landed. Minor.** JS reads it
before.

**The fix:** `TickHelm` now runs straight after `TickPower` and before `TickParty`.

**(D3) The attitude key glyphs were built as squares at 45°. Visual.** On screen they stand
upright.

**The fix:** the stems are built 6×16 and the arrows 7×7, upright. The arrow and its core are a
new `HandoffPolygon` graphic (`Assets/Scripts/Tactical/HandoffPolygon.cs`). It uses the AP_ATT
clip-path triangles.

#### Verification pass 2

**STEP 33h-1: VERIFIED.** Evidence is in `Temp/HandoffQA/step33h1/rerun/`.

- 0 mismatches over 4,001 random steps, and 0 of 303 across throttle × mode.
- The repair tick order matches JS: heading 2, 5, 8 …
- The glyph geometry matches the records and the clip polygons. Hit testing is unchanged.
- The default board differs from `edit_33g.png` by 0 px.
- Fire Control fires, and one control in each drawer works.
- Console clean, scene not dirty.
- Harness note: feeding board→screen→board points adds about 6e-5 px of float error. That tips
  exact half-percent ties. A real pointer cannot land on those ties exactly.

#### Traps

- **JS toFixed rounds the double's exact binary value** (1.45 is 1.4499… → "1.4"; 1.25 → "1.3").
  Neither C# `Math.Round` nor a G17 string does. Round on the exact value (`JsToFixed`,
  BigInteger).
- **Tick order matters when JS reads pre-tick state.** tickPower computes everything from the state
  before the tick, so a C# sub-tick that reads something another sub-tick changes must run first.

### Step 33h-2 — the orders station

The orders station works on its own, from the board's own rules. No game wiring. **Done — verified
on the second pass.**

#### Files

- `Assets/Scripts/Tactical/HandoffOrdersPanel.cs` — new. Draws the orders station from the state.
- `HandoffBoardState` — changed. It now has an orders section:
  - `auxCharge` (double), and `auxPerUnit` 3.2;
  - the order costs: PARTY 30, SEAL 25, VENT 15;
  - `orderFail`, which expires after 420 ms (in `ExpireFlicks`);
  - `DoAlert`, `OrderCost`, `OrderBlock`, `RunOrder`, `TickAux`.
- `HandoffBoardControls` — changed. `TickAux` runs after `TickPower`.
- `HandoffBoardBuilder` — changed. `WireOrders`.

#### Controls

- The alert selector cycles CONDITION 3 → 2 → 1. It works on both the stowed and the open station.
- The PARTY, SEAL and VENT order rockers. A refused order shows red for 420 ms with its reason. The
  rocker flicks back after 400 ms.
- FIRE, BLKHD and SCRUB are blanking plates.

#### Readouts

- The condition label and its ink, the release line, the knob pointer (60°, 90° or 120°), the lamps
  and their labels.
- ACTING ON: the engineering selection.
- AUX % and its ink, the rate ("+N.N/S" or NO FEED), and the 10 lit cells.
- The rocker sprites, lamps, labels and the order note.

#### Also

- The alert values set by hand in earlier steps' tests now have a real control.

#### Checked, in play mode

The builder's own play test. Real clicks and ticks.

- The alert cycles. C2 gives RELEASED in green.
- AUX at 2 units: +6.4/S.
- VENT at 28.4%: "6 CREW ABOARD", the lamp red, the rocker thrown.
- PARTY at 28.4%: "INSUFFICIENT AUX — 30% REQUIRED".
- PARTY at 64%: dispatched, 34% left.
- The default edit-mode board is pixel-identical (0 px differ) to `edit_33g.png`.
- Picture of the open station: `Temp/HandoffShots/33h2/open_c1_aux100.png`.

#### Choices made because the package is silent

Each one is in `HANDOFF_GAPS.md`, rows 82–87.

- Only the C3 alert lamps have a glow in the export. We copied it to the other alert lamps. Row 82.
- Lit AUX cells are a 1 px inset ink.data face, as on the stability cells. Row 83.
- The AUX row's right column is right-aligned at x 460. At 100% the "1" sits on the last lit cell
  and cannot be read. Row 84.
- The order costs have no layout record. Not shown. Row 85.
- The blank order lamps follow the JS: an inset shadow, no glow. Row 86.
- The AUX rate text stays ink.wellStencil. Row 87.

#### Verification pass 1

**NOT VERIFIED.** Evidence is in `Temp/HandoffQA/step33h2/`.

- Port fidelity: 4,000 random steps, 120,030 fields, 0 mismatches.
- Start state, hit testing, readouts, 22 renders, regression and console: all pass.

**(D1) A refused order painted its label #E8938A (`orderSwitches[].ink`).** The handoff README
(Open questions) and gap row 41 say the template never reads that value. In the design the label
stays #C9D2D8.

**The fix:** the label is always the label ink. The `failedInk` field is removed.

**(N1) "100%" starts at x 436 and sits on the last lit cell face, which ends at 438.5.** The "1"
cannot be read. Added to `HANDOFF_GAPS.md` row 84.

#### Verification pass 2

**STEP 33h-2: VERIFIED.**

- A fresh 4,000 steps, seed 34: 0 mismatches. Refused rockers appear in 2,770 snapshots. Each has
  a red lamp and glow, and the label at #C9D2D8.
- Refused-render check: VENT on REACTOR shows "6 CREW ABOARD".
- The default board differs from `edit_33g.png` by 0 px.
- Fire Control fires, and one control in each drawer works.
- Console clean, scene not dirty.

#### Traps

- **A `renderVals` value is not proof that something shows.** Check the README's open questions and
  the gaps list for values the template never reads (`orderSwitches[].ink`) before painting them.

### Step 33h-3 — the periscope

The periscope works on its own, from the board's own rules. No game wiring. **Done — verified on
the first pass.**

#### Files

- `Assets/Scripts/Tactical/HandoffPeriscopePanel.cs` — new. Draws the periscope from the state.
- `Assets/Scripts/Tactical/HandoffScopeSight.cs` — new. `drawScope` ported as a `MaskableGraphic`.
  It is redrawn every frame. The drift is time-based, 0.054 a second.
- `HandoffBoardState` — changed:
  - `scopeBrg` is now a double;
  - new fields: `scopeZoom`, `slewing`, `slewDx`;
  - new methods: `ScopeOn`, `ScopeSubject`, `Classify`, `EndSlew`, `SlewTo`, `OffLabel`;
  - `Slew`: 0.34° a pixel, with a 3 px dead zone. It releases BEARING and ARM, rests the lever and
    clears `fired`;
  - `SetZoom`: dx / 40, from 1 to 8, in 0.1 steps.
- Fire Control's bearing readouts now use these.
- `HandoffImport` — changed. A new 512 px "aperture_mask" shape.
- `HandoffBoardBuilder` — changed:
  - `WirePeriscope`;
  - a shared `CopyGlow` helper;
  - a circle `Mask` holding the sight. It sits above the aperture glass and under the vignette.

#### Controls

- Drag across the aperture to slew.
- Drag the MAG knob.
- SLEW: jump to the target.
- CLASS: classify the subject in view.

#### Readouts

- The off-axis label and its ink, MAG ×, the MAG pointer, the bearing (000).
- The SLEW label and its ink.
- The aperture sprite swaps to the slewing rim.
- The target brackets and the track name move with the off-bearing. They hide beyond 27°.
- The convergence bar.
- The subject's name and class.
- The SLEW and CLASS lamps and their glows.
- The mount short label.

#### Also

- The scope bearing values set by hand in earlier steps' tests now have real controls.

#### Checked, in play mode

The builder's own play test. Real drags and clicks.

- A drag of −40 px from 62: 48.4°, off "-0.4°" in green, "◀ SLEW" in amber, the slewing rim.
- Release: the idle rim is back.
- MAG +60 px: 2.9×, the pointer at −64°.
- SLEW: 048, and "0.0°".
- With SEN 2 (senBoost 2): "BOGEY T07 / HULL UNCLASSIFIED", with an amber lamp. CLASS changed it
  to "TRACK 07 / CRUISER HULL", with a green lamp.
- The default edit-mode board differs from `edit_33g.png` by 0 px.
- Renders: `Temp/HandoffShots/33h3/lowered_classified.png` and `edge_zoom.png`.

#### Choices made because the package is silent

Each one is in `HANDOFF_GAPS.md`, rows 88–93, and row 11.

- The bearing tape ticks are not drawn. The stand-ins #76 / #77 stay put. Only the 062 readout
  moves. Row 88.
- `classifyInk` is computed but not applied. Row 89.
- The mount short label (#63) stays static accent.alarm. Row 90.
- The CLASS lamp (#23) uses a copy of lamp #22's glow. Row 91.
- The drift runs at 0.054 a second, not 0.0009 a frame. It is in the inspector. Row 92.
- The sight is clipped by a circle stencil mask, with a hard 1 px edge. Row 93.
- The bracket label (#64) shows the target name. The binding is assumed. Row 94, added after
  verification.

#### Verification pass 1

**STEP 33h-3: VERIFIED.** Evidence is in `Temp/HandoffQA/step33h3/`.

- **Port fidelity:** 358 ops (1,091 steps) through the real controls, 0 mismatches. They cover the
  dead zone, wraps, zoom clamps, slewTo, classify, every target and every mount.
- **drawScope geometry:** 5 (t, zoom) pairs, 42 lines each. Endpoints match within 7e-5 px, widths
  within 1.2e-4 px. The ladder ticks are 1.4 px wide, and the glow matches.
- **Start state:** housed and lowered both match layout.json.
- **Hit testing:** 0 bad hits. Nothing on the scope can be hit while it is housed.
- **Real input:**
  - The 3 px dead zone works.
  - The slewing rim sprite swaps in.
  - A slew releases BEARING and ARM only, rests the lever and clears `fired`.
  - MAG clamps at its limits.
  - SLEW sets 048, and the echo reads ON BEARING.
- **Renders:** 12 states. Nothing is drawn beyond radius 171. The brackets hide at exactly 27°.
- **Regression:** the board at rest differs by 0 px. Fire Control fires after a real slew. The helm
  and orders still work.
- **Console and scene:** clean.

Note: the aperture hit area is the 340 px square. So the corners outside the circle also start a
slew. This matches the other round controls.

#### Follow-ups after verification

Both are **to be re-checked in the 33i verification**. The target can only change in 33i.

- **(a) The bracket label (text #64) now shows the target name:** `tgt.name`, or "—" with no target.
  The binding is assumed to be `renderVals` `targetName`, because the template is not exported.
  `HANDOFF_GAPS.md` row 94.
- **(b) The sight's far-aperture frame now has mitred corners, like ctx.rect.** The top and bottom
  edges extend by half the line width, and the sides fit between them. Before this there was a
  0.8 px notch at each corner.

After both, the default board still differs by 0 px, and the console is clean.

#### Traps

- **A `Graphic` added with `AddComponent` to a bare RectTransform got no CanvasRenderer here.** The
  resulting MissingComponentException in RectMask2D clipping, on every canvas update, rendered the
  WHOLE board black with no visible cause. Add `CanvasRenderer` explicitly first.
- **A sensor contact only counts as "heard" with enough SEN power.** At start senBoost is clamped to
  1 (19,000 km), and t07 is at 24,300 km. Test periscope subjects with SEN fill ≥ 2.

### Step 33i — the plot tools

The plot tools work on their own, from the board's own rules. No game wiring. **Done — verified on
the second pass.** The verification also re-checked the two 33h-3 follow-ups. Both pass.

**Owner decision during this step:** the board now starts undamaged. See section 3. Any verifier's
model must use the undamaged start.

#### Files

- `Assets/Scripts/Tactical/HandoffPlotPanel.cs` — new.
- `Assets/Scripts/Tactical/HandoffMarksGraphic.cs` — new.
- `HandoffBoardState` — changed:
  - marks, the draft and `markSeq`;
  - `MarkDown`, `MarkMove`, `MarkUp`. A draft shorter than 4 layer px is dropped;
  - `EraseMark`, `ValidTarget`, `PickTarget`, `LookAt`;
  - compartments load undamaged (section 3).
- `HandoffBoardControls` — changed:
  - `PickTool` clears the draft;
  - ERASE pressed twice clears all marks and returns to CURSOR;
  - the wheel zooms over "plot.*" too;
  - new: `MissionSeconds` and `GridAmber`.
- `HandoffBoardBuilder` — changed. `WirePlot` adds layers under Plot, after Mockup and before
  Scanlines:
  - the capture surface. It takes clicks only for POINT, RULER and COMPASS;
  - the drawn marks;
  - the mark labels, cloned from a hidden template of text #47;
  - the ERASE hit pool.
- BRG 149 (text #39) is wired to lookAt CONTACT 22.

#### What the tools do

- **POINT:** stamps a dot, labelled with the mission clock.
- **RULER:** press and drag a line. It is labelled "RNG n,nnn km · BRG nnn°" at its midpoint.
- **COMPASS:** drags a dashed ring centred on own ship, with a bearing line. It has the same label,
  at the pointer.
- **Drafts** show at 75% while dragging. A drag under 4 px is dropped.
- **ERASE:** click a mark to delete it. Press ERASE again to clear every mark.
- **Zoom:** marks keep their size on screen. Their positions scale with zoom. 60.9 px = 1,000 km at
  1×.
- **Clicking BRG 149** lowers the periscope onto 149°. The label turns amber while the scope is on
  it.
- **Target picking:** the rule is ported, but it has no click surface yet. Row 95.

#### Checked, in play mode

The builder's own play test.

- POINT at board (1100, 400) → mark (140, −145), labelled 02:03:32.
- A 2 px ruler was dropped.
- A ruler (−260, 155) → (−140, 95): "RNG 2,203 km · BRG 063°".
- A compass to (340, −100): "RNG 5,819 km · BRG 074°".
- lookAt c22: scopeBrg 149, with the scope down.
- The default board differs from `edit_33g.png` by 0 px.
- Render: `Temp/HandoffShots/33i/marks_z1.png`.

#### Choices made because the package is silent

Each one is in `HANDOFF_GAPS.md`, rows 95–100.

- The resolved contact glyphs have no records, so target picking has no click surface. Row 95.
- Only "BRG 149" is exported. It is wired to lookAt c22. BRG 048 and the passive line template are
  missing. Row 96.
- Mark label offsets: point 10 px right; ruler centred 12 px above the midpoint; ring 8 px right of
  the end. All tunable. Row 97.
- The ring's dash is 4.5, gap 3. The point dot and ruler end dots are round. Row 98.
- `markScale` is not applied. Row 99.
- The COMPASS erase hit area is the ring's square box. Row 100.
- The passive bearing line opacity (0.6, or 1 with the scope on the contact) is not applied. Row
  101, added after verification.

#### Verification pass 1

**NOT VERIFIED.** Evidence is in `Temp/HandoffQA/step33i/`.

- Port fidelity: 9,011 states, 0 mismatches.
- Geometry, hit testing, renders, regression and console: all pass.
- The 33h-3 follow-ups pass:
  - text #64 follows the target;
  - the frame is mitred.

**(D1) Short drags were lost. Failing.** With real input, a RULER or COMPASS drag under the
EventSystem's 10 px drag threshold was dropped. OnDrag never fired, and Up ignored the release
point. The same threshold hit every drag control.

**The fix:** `HandoffHit.OnPointerUp` now delivers the release point as a final Drag when no drag
started. This matches the browser's last mousemove. It fixes every drag control at once.

**(D2) A wheel turn during a drag changed the layer scale.** The browser keeps the zoom from the
press.

**The fix:** the plot panel holds the zoom from the press.

**(D3) The passive label was always shown.** The browser shows it only while the contact is heard
and not identified.

**The fix:** text #39 is active only while c22 is heard and not identified. The inspector bool
`passiveVisibility` controls this.

**(D4) The ruler label overlaps steep lines. Cosmetic.** It is tunable. Not changed.

#### Verification pass 2

**STEP 33i: VERIFIED.** Evidence is in `Temp/HandoffQA/step33i/r2/`. The EventSystem event order
was replayed through `ExecuteEvents`.

- Short moves now work like the browser for RULER, COMPASS, slew, MAG, the throttle, tune, gain,
  both levers (a short pull does not fire or launch) and the SCALE knob.
- Click-only controls still click exactly once.
- D2: 100 layer px after a wheel notch.
- D3: 80 combinations match.
- Seed 11: 7,750 states, 0 mismatches apart from the wheel rounding below.
- Regression: the board differs from `edit_33g.png` by 0 px. Fire Control fires.
- Console clean, scene not dirty.

#### After verification: wheel rounding

A Step 33a defect the verifier found.

- The wheel multiplied the zoom in float, so 0.70 zoomed out gave 0.63. The browser gives 0.62.
- **The fix:** `HandoffBoardControls` holds the zoom as a double (`ZoomExact`). `SetZoom` takes a
  double. The wheel step and the SCALE drag compute in double. The plot panel uses `ZoomExact`.
- Play check: 0.70 → 0.62. 1.0 out ×5 → 0.56, then in ×3 → 0.80. Both match the browser.

#### Traps

- **The EventSystem withholds OnDrag until a 10 px move; the browser sends every mousemove.**
  Deliver the release point as a last Drag in `OnPointerUp`, or short drags are lost on every drag
  control.
- **Keep any value the browser holds as a JS number in a double end to end.** A float in between
  (zoom 0.62f) shifts later rounding.

### Step 33j — the control transitions

**VERIFIED on the first pass.** Controls now animate where UIStateSpec §9.1 gives them a
transition, instead of snapping.

#### Files

- `Assets/Scripts/Tactical/HandoffEased.cs` — new. One CSS transition on one number.
  - Its seconds and curve can be set in the inspector.
  - A new target starts from the value on screen.
  - Zero seconds means the value changes instantly.
- `HandoffBoardControls` — changed. `Update` ticks all live transitions. They are cleared on
  destroy.
- `HandoffBoardBuilder` — changed. It sets each knob's duration and curve from §9.1.

#### What animates

- **Knob pointers (`Dial`) now ease:**
  - the selector spring, cubic-bezier(.3,1.5,.5,1), at .2 s: comms band, engineering ACTING ON,
    hangar doors, ship and command, Fire Control system and mount, orders alert (stowed and open),
    and console tint;
  - power bus selectors: the spring at .18 s;
  - comms key dials: .14 s ease.
- **Every other knob stays instant**, because the design gives it no transition: comms tune, the
  sensors cone needle, SCALE, MAG and the Fire Control solution knobs.
- **Other animated values:**
  - helm heading ring: .12 s linear;
  - sensors gain cap: .12 s ease;
  - hangar CATAPULT bar width: .4 s ease;
  - hangar stage bars: .4 s linear.
- **Already done in Step 32:** drawer slide and fade, periscope drop, helm and orders latches,
  blinks, drift and pulse.
- **Not animated, for lack of art or records:** `HANDOFF_GAPS.md` rows 102–105 (rocker pads, the
  hangar push-button press, the envelope and cone size, the radar sweep).

#### Checked, in play mode

The builder's own play test.

- Clicking ALERT keeps the pointer at 300° on the click frame. Two transitions go live (stowed and
  open).
- It settles at 270° with none still live.
- The default board differs from `edit_33g.png` by 0 px.

#### Verification pass 1

**STEP 33j: VERIFIED.** Evidence is in `Temp/HandoffQA/step33j/`.

- **Coverage.** All 56 properties in §9.1 are accounted for:
  - 19 built as specified;
  - 16 already done in Step 32;
  - 21 deliberately not done (rocker sprite swaps, the plot envelope and cone, the press button,
    the warhead selector).
- **Curve fidelity.** Matches an independent cubic-bezier within 0.004°.
  - The spring overshoots to 1.08 (doors peak at 124.77° on the way to 120°).
  - Retargeting starts from the value on screen.
  - The helm going from 359° to 0° interpolates numerically, as CSS does.
- **Instant controls stay instant.**
- **End states** equal the earlier instant values.
- **Live list.** It empties correctly. No errors when you close a drawer mid-flight, exit play, or
  re-enter play.
- **Regression.** The board differs by 0 px. Fire Control fires. Every panel works.
- **Console** clean. **Scene** not dirty.

#### After verification: two minor notes fixed

- **(1)** The CATAPULT bar width now uses `Math.round(cat)`, as spec line 1142 does.
- **(3)** Stage bar motions now follow the `stageBarSeconds` inspector value during play.

**(2) Left as is.** CSS shortens a transition that reverses mid-flight (the "reversing shortening
factor"). `HandoffEased` always runs the full duration. You only see the difference on a fast
double-click of a two-state selector.

### Final QA: FINAL QA: PASS

**PASS.** Evidence is in `Temp/HandoffQA/final/`, including `contact_sheet.png` and
`ref_vs_default.png`.

#### Results of each check

- **Scene flow.** 00_Start → 01_Briefing → 02.1_Grid. The old 02_Grid is still in the build list,
  but nothing leads to it.
- **Visible.** 10 states were rendered, and everything draws. Differences from the design are only
  at the plot centre, and those are `HANDOFF_GAPS.md` rows 1–9 and 104–105.
- **Clickable.** Every control is reachable in its state. All 51 action names have exactly one
  handler. Hidden controls take no clicks.
- **Full player session.** All 15 steps passed through real input:
  - alarm, power, condition, sensors identify;
  - comms decode;
  - fire control and periscope, then fire;
  - hangar launch;
  - helm;
  - an order spent after venting a room;
  - marks;
  - wheel, tint, time.
- **No interaction problems.** Opening a drawer closes the others. The panels stay in step.
- **Stability.**
  - 60 s of play time, plus 150 ticks.
  - The object count does not change.
  - About 8 KB per tick of short-lived garbage.
  - Re-entering play mode twice was clean.
  - The console was clean.
- **Baseline.** The edit board is 0 px different and the scene is not dirty.

#### Follow-ups

- **Fixed.** `HandoffBoardMotion.Update` made a new list every frame. It now reuses one list. This
  compiled clean.
- **Noted, not changed:**
  - The helm A and D keys use the old `Input` class. Move them to the new Input System if the old
    input is ever turned off.
  - Panel refreshes make about 8 KB of garbage per tick. Look at this again when the board is wired
    to the game.
  - The old 02_Grid scene is still in the build settings. Removing it is the owner's choice.

### Follow-up: ours-to-fix items (2026-10-05)

**Status: VERIFIED (OURS-TO-FIX: VERIFIED; evidence in Temp/HandoffQA/ourfix/).** Mark dashes measured 3.016 / 2.011 px; the SENSOR RANGE label steps 48 px per gain (1–5) with the live range; ▸ ▶ ◀ render from the Noto fallback; gap rows check out against the reference; board 0 px; play mode exited. These are the four "Ours to fix" items in `CLAUDE_DESIGN_NEEDS.md`.

1. **Mark dash** is now 3 px dash, 2 px gap, per `asset-map.csv` `mark_dash`. It was 4.5 / 3.
2. **SENSOR RANGE plot label** (texts #37, #38) now rises 48 px per gain step from its measured
   gain of 1 (`senCapTop` = −(150 + gain × 48) − 8 from the plot centre). #38 shows
   `senRangeLabel` ("19,000 KM" at gain 1). Changes: `HandoffSensorsPanel` `envelopeLabel` /
   `envelopeRange`; the builder wires them.
3. **▸ ▶ ◀** come from the Noto Sans Symbols 2 fallback (SIL OFL 1.1). `HandoffImport`
   `SymbolsFallback()`.
4. **`HANDOFF_GAPS.md` rows 7, 8, 12 and 44 corrected,** and rows 98, 30 and 36 marked Done.

#### Checked

- The default board is still 0 px different from `edit_33g.png`.
- The console is clean.

#### Note

A checker agent had left Unity in play mode. It was stopped before these changes. The owner did
not start it.

#### Traps

- Every agent that enters play mode must exit it before it finishes, and say so in its report.

---

## 9 · Problems found in the package

- **36 drawn shapes say fill "gradient" but give no colours.** Built as flat grey stand-ins for now.
  26 stand-ins are on the board after Step 31. Two of them, the radar sweep (drawn #34) and drawn
  #38, are built but hidden. #38 was first called the periscope sight disc. It is not: it is a
  plot shape, centred on the plot. The other 10 are removed by other rules:
  - 9 are parts of the conduits: default drawn #1, #2, #3, #4, #11, #12, #13, #14, #23. The conduit
    composition sprites now carry them.
  - 1 is the plot glow: default drawn #32, the same as the plot rectangle. It is rebuilt from
    `plot-grid.json` as the Glow layer.
  - 36 − 10 = 26.
- **Shadow margins are written several ways in the `asset-map.csv` notes, and some are missing.**
  See Step 30 for the order the builder uses.
- **A 9-sliced image's size says nothing about its shadow.** Its transparent pad had to be measured
  in each PNG. See D1 in Step 30.
- **The conduits are recorded twice.** Once as fixed compositions. Again as 24 drawn records of their
  own pipes and clamps. The drawn records are skipped.
- **The design's colours assume browser (gamma) blending.** Unity in Linear mode needs the board fix
  in Step 31, problems (a) and (b).
- **Two-line labels are two lines joined into one record, and "X ( )" legends hold a separate record
  inside them.** The builder handles both. See Step 31, problems (c) and (d).
- **The periscope's inner parts are tagged `default`, but sit inside the folded periscope.** They
  are clipped by a box 44 px tall when housed. This is how the board does it.
- **The rail icons are recorded twice.** Once as sprites. Again as their vector parts, measured as
  plain rectangles. The rectangles are skipped.
- **No panel tag on any record.** The builder uses a table of index runs, read off this export.
- **The own ship, contacts, radar sweep and predicted ghost on the plot exist only as plain
  rectangles.** Their vector shapes were not exported. These are the worst-matching parts of the
  board.
- **Drawn shapes marked `shadow: true` carry no shadow values.** 28 drawn shadows are not built.
- **Drawing order between record types is not in the data.** Within one type, the lists are in page
  order. The builder orders by what sits inside what, then by type, then by page order.
- **The plot's inset blur shadow cannot be built.** There is no blur tool.
- **▸, ▶ and ◀ are missing from every font.** See Step 29.
- **The README's own Open questions list** (`export/README.md`, last section).
- **Text has its leading space trimmed.** See D1 in Step 31.
- **Mockup group opacity was not exported.** Some mockup labels read too bright.
- **The full-board edge shadow (drawn #0) has no values.** Frame edges read 25–35% too bright.

---

## 10 · Open items

1. **Done: the final end-to-end QA pass. FINAL QA: PASS.** The board integration, Steps 28–33
   plus final QA, is complete. Open questions for Claude Design are all in `HANDOFF_GAPS.md`
   (105 rows). Follow-ups noted, not changed: the helm A and D keys on the old `Input` class,
   about 8 KB of garbage per tick from panel refreshes, and the old 02_Grid in the build settings.
2. **Done: `CLAUDE_DESIGN_NEEDS.md`,** the verified list of everything Claude Design must provide.
   Its "Ours to fix" items are done and verified. See "Follow-up: ours-to-fix items" in section 8a.
3. **Next: audit the project's existing UI assets** against `CLAUDE_DESIGN_NEEDS.md`, to find
   anything that can fill an item. Starting now.
4. **Then the short Claude Design request, when the owner asks for it. Then wiring the game systems
   in.**
5. **Owner's call: Share Tech Mono thickness.** Dilate 0.3 is a little heavy at 12–17 px. About 0.2
   above 11 px would be closer. See Step 31, QA's cosmetic note.
6. **What the Claude Design request must cover is in `CLAUDE_DESIGN_NEEDS.md`.** The raw list of
   gaps is in `HANDOFF_GAPS.md`. Main items:
7. Vector shapes for the own ship, contacts, radar sweep and predicted ghost. They are the worst
   areas on screen.
8. Colour stops for the gradient fills. They are flat grey stand-ins until then. The radar sweep (drawn #34)
   and the plot disc drawn #38 stay hidden until then.
9. Shadow values for drawn shapes marked `shadow: true`. 28 are not built. This includes the
   full-board edge shadow (drawn #0).
10. Group opacity for the mockup glyphs and their labels.
11. A panel tag and a drawing order number on every record. Until then, a re-export that changes the
    page needs the builder's table updated.
12. The plot's inset blur shadow. Not built; decide if it matters. The periscope sight canvas is
    built since Step 33h-3.

The font for ▸ ▶ ◀ is no longer needed. They now come from the Noto Sans Symbols 2 fallback.

---

## 11 · How to keep this log

Update it after every step:

- What was done.
- What was checked, and by whom.
- Problems hit.
- Open items: take out what is finished, add what was left behind.

Move the step's status in the table in section 2. Keep `BUILD_LOG.md` in step with it, since that is
still the pickup point.
