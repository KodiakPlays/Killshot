# Star Wolf — Unity UI handoff

The tactical command board as a set of assets and measurements for Unity. Generated 2026-09-27 from `Star Wolf Board Live.dc.html` by `handoff/tools/gen-readme.js`; every count below is computed from the files in this package.

Target: Unity 6000.5.10f1, URP 17.5, Linear colour space, uGUI 2.5, TextMesh Pro. The package holds no code and no Unity files.

## Contents

| Path | What it is |
|---|---|
| `sprites/`, `sprites/@2x/` | 110 sprites at 1× and 2×, cut from the board's own markup |
| `textures/` | 7 textures: `steel_frame.png` and six drawer bodies (`casting_*.png`), plus `plot-grid.json` |
| `asset-map.csv` | 227 rows: every image with its size, 9-slice borders, tint token and placement notes |
| `tokens.json` | 129 colours, one per RGB value, named by job; 48 are applied at runtime |
| `layout/layout.json` | Position, size, rotation, tint and state of every sprite, lamp, glyph, text run and live-drawn area |
| `layout/board-1920x1080.png` | The board as it loads, for comparison |
| `fonts/` | Barlow Condensed (Regular, Medium, SemiBold) and Share Tech Mono (Regular), .ttf, with their OFL licences |
| `reference/UIStateSpec.md` | State, interlocks, the power tick, input and motion, generated from the board |
| `reference/board-logic.js` | The board's logic, verbatim, for porting |

## Canvas and import

- **Canvas:** Screen Space - Camera, Canvas Scaler *Scale With Screen Size*, reference 1920 × 1080, match 0.5, Reference Pixels Per Unit 100. All coordinates in this package are px at that reference size.
- **Sprites:** Texture Type *Sprite (2D and UI)*, sRGB on, Alpha Is Transparency on, Filter *Bilinear*, compression *None* (the parts carry fine bevels). Use the @2x files with **Pixels Per Unit 200** so they land at 1× size; use the 1× files with 100.
- **9-slice:** borders are in `asset-map.csv` in px of that file (the @2x rows are already doubled). Set them in the Sprite Editor and use Image Type *Sliced*. Mesh Type *Full Rect* for sliced sprites.
- **Tint:** sprites whose `tint_token` names a token are white masks. Set `Image.color` to that token's hex. `state` means the colour follows state: lamps take a `lamp.*`, `accent.*` or `phosphor.*` token (the default per lamp is in layout.json).
- **Colour:** token hex values are sRGB, as Unity's colour pickers expect in a Linear project.
- **Shadows are baked.** Each sprite includes its outer shadow in a transparent margin; the margin is listed in the asset notes. Position by the element, not by the image edge (see Layout).

## Fonts

- **Share Tech Mono Regular** for all data, stencils and legends (447 text runs). **Barlow Condensed** for panel titles and buttons: SemiBold is weight 600, Medium 500, Regular 400 (14 text runs). Each text run in layout.json gives font and weight.
- Make one TextMesh Pro font asset (SDF) per file. Besides ASCII, the board uses — − ° ± · × in Share Tech Mono and × in Barlow Condensed; include them in the atlas.
- **▸ (U+25B8) is not in Share Tech Mono.** The board uses it in two engineering status lines, where the browser substitutes a system font. Give the Share Tech Mono asset a fallback font that has it, or it will draw as a missing glyph.

## Layout

`layout/layout.json` was measured on the running board: every visible element in the default state, then with each drawer, the helm and the orders station opened in turn. Coordinates are px from the board's top-left corner, y down.

- **Placing a record in uGUI:** anchor and pivot top-left (0, 1), `anchoredPosition = (x, -y)`, `sizeDelta = (w, h)`. For a sprite with a baked shadow margin, grow the rect by that margin on each side.
- **Rotation** is degrees clockwise. For rotated records the rect is the rotated bounding box; knob pointers and ticks also carry their unrotated `size`.
- **`state`** is where a record first appears: `default` is the board as it loads; any other value is the drawer or station that must be open.
- **Sections:** `textures` (7), `sprites` (436), `lamps` (101), `icons` (12), `text` (461), `drawn` (339 areas Unity draws itself: fills, bars, rings), `canvases` (2), `swaps` (sprites that replace each other with state).
- **Text:** TextMesh Pro, font as given, size in px. Letter spacing is given in px; TMP character spacing is in 1/100 em, so `characterSpacing = trackingPx / size × 100`. Colour is a token name plus alpha.
- **Drawer textures:** `x, y, w, h` is the drawer body; `place` is where the whole texture goes, so its margin can hold the drawer's shadows.

### Regions that open

| State | Body at | Size | Body image |
|---|---|---|---|
| power | 1440, 255 | 378 × 570.5 | `casting_power` |
| sensors | 1440, 166 | 378 × 748 | `casting_sensors` |
| comms | 1440, 272 | 378 × 536.5 | `casting_comms` |
| engineering | 1440, 265 | 376 × 550 | `casting_engineering` |
| hangar | 1440, 132.5 | 378 × 815 | `casting_hangar` |
| fire control | 1440, 189.5 | 378 × 701.5 | `casting_fire_control` |
| helm | 610, 874.5 | 700 × 205.5 | `helm_body` |
| orders | 104, 790.5 | 392 × 267.5 | `orders_body_open` |

Fixed regions, always present: the plot (104, 100, 1712 × 890), the tool rail at the left, the systems rail at the right, time control and the alarm strip at the top, the periscope carriage at top centre, the collapsed helm at bottom centre, the stowed orders station at bottom left and the console strip at bottom right. layout.json holds each at its measured position.

## Interactive states

Sprites that swap with state:

| Family | Sprites |
|---|---|
| rocker_30x38 | `rocker_30x38_up`, `rocker_30x38_locked`, `rocker_30x38_thrown` |
| rocker_24x32 | `rocker_24x32_up`, `rocker_24x32_locked`, `rocker_24x32_thrown` |
| rocker_22x30 | `rocker_22x30_up`, `rocker_22x30_locked`, `rocker_22x30_thrown` |
| rocker_reboot | `rocker_reboot_up`, `rocker_reboot_locked`, `rocker_reboot_thrown` |
| button_recall | `button_recall_up`, `button_recall_pressed` |
| periscope_aperture | `periscope_aperture_idle`, `periscope_aperture_slewing` |
| pause_key | `pause_key_run`, `pause_key_paused` |
| rate_key | `rate_key_off`, `rate_key_on` |
| orders_body | `orders_body_closed`, `orders_body_open` |

- **Rockers:** `up` can be thrown; `locked` is refused, and the lamp beside it plus the glass readout carry the reason; `thrown` sits low with a red glow. The paddle stays red in every state; its lamp carries the meaning. The 30×38 set serves the sensor masters, the arming ladder, the load shed and the comms LOCK; the 22×30 set serves the orders station and the engineering repairs; REBOOT (90×30) is its own variant. Orders and engineering rockers are momentary and flick back after 400 ms.
- **Pressed:** four controls react while held, and the board has no hover states. The RECALL key swaps to `button_recall_pressed` (cap 4 px lower, tighter shadow, 0.1 s). The three fire control pushbuttons with a pressed state (quadrant select, LOADERS, COMPUTE) keep `button_push` and move 1 px down.
- **Rail keys:** the selected tool shows `rail_key_selected` and a lit lamp; a systems key shows its drawer open by its lamp alone. One drawer is out at a time.
- **Lamps:** `lamp_dot` (or `rail_lamp_lens` in a rail key) tinted by state, with `lamp_glow` behind a lit lamp.
- **Knobs:** the body is one sprite; the pointer rotates about the knob centre and snaps between detents. Pointer and tick positions for every knob are in layout.json.
- **Levers and faders:** the cap travels along its slot; the lit track behind it is drawn live. Pitches, ranges and thresholds are in `reference/UIStateSpec.md` §8.2.
- **Gating:** which control is gated, by what, and the reason it shows are in `reference/UIStateSpec.md` §4 and §7.

## Motion

Transitions (56 properties on 37 elements), grouped:

| Panel | Section | Properties | Duration | Easing | Driven by |
|---|---|---|---|---|---|
| Plot | SENSOR ENVELOPE | width, height, margin | .5s | `ease` | `senBoost` |
| Plot | ACTIVE SCAN CONE | width, height, margin | .5s | `ease` | `senBoost` |
| Power | — | transform | .42s | `cubic-bezier(.2,.85,.25,1)` | `powerOpen` |
| Power | — | opacity | .3s | `ease` | `powerOpen` |
| Power | EMERGENCY LOAD SHED | top | .14s | `cubic-bezier(.3,1.5,.5,1)` | `shed` |
| Power | EMERGENCY LOAD SHED | background | .14s | `ease` | `shed` |
| Power | BUS COLUMNS | transform | .18s | `cubic-bezier(.3,1.5,.5,1)` | `busMode` |
| Sensors | — | transform | .42s | `cubic-bezier(.2,.85,.25,1)` | `senOpen` |
| Sensors | — | opacity | .3s | `ease` | `senOpen` |
| Sensors | SET MASTERS | top | .14s | `cubic-bezier(.3,1.5,.5,1)` | `dradis`, `active` |
| Sensors | SET MASTERS | background | .14s | `ease` | `dradis`, `active` |
| Sensors | 01 ARRAY GAIN | top | .12s | `ease` | `senBoost` |
| Comms | — | transform | .42s | `cubic-bezier(.2,.85,.25,1)` | `commsOpen` |
| Comms | — | opacity | .3s | `ease` | `commsOpen` |
| Comms | 01 RECEIVER | transform | .2s | `cubic-bezier(.3,1.5,.5,1)` | `chan` |
| Comms | 01 RECEIVER | top | .14s | `cubic-bezier(.3,1.5,.5,1)` | `locked` |
| Comms | 01 RECEIVER | background | .14s | `ease` | `locked` |
| Comms | 02 DECRYPT | transform | .14s | `ease` | `dials` |
| Engineering | — | transform | .42s | `cubic-bezier(.2,.85,.25,1)` | `engOpen` |
| Engineering | — | opacity | .3s | `ease` | `engOpen` |
| Engineering | THE SCREEN | transform | .2s | `cubic-bezier(.3,1.5,.5,1)` | `engSel` |
| Engineering | Engineering runs its own set | top | .12s | `cubic-bezier(.3,1.5,.5,1)` | `dcThrown` |
| Engineering | Engineering runs its own set | background | .12s | `ease` | `dcThrown` |
| Hangar | — | transform | .42s | `cubic-bezier(.2,.85,.25,1)` | `hangarOpen` |
| Hangar | — | opacity | .3s | `ease` | `hangarOpen` |
| Hangar | 01 BAY STATUS | transform | .2s | `cubic-bezier(.3,1.5,.5,1)` | `doorsOpen` |
| Hangar | 01 BAY STATUS | width | .4s | `ease` | `lprog` |
| Hangar | 03 LAUNCH ORDER | transform | .2s | `cubic-bezier(.3,1.5,.5,1)` | `squad`, `order` |
| Hangar | 04 LAUNCH SEQUENCE | top | .14s | `cubic-bezier(.3,1.5,.5,1)` | `lsw` |
| Hangar | 04 LAUNCH SEQUENCE | background | .14s | `ease` | `lsw` |
| Hangar | 04 LAUNCH SEQUENCE | width | .4s | `linear` | `lprog` |
| Hangar | 04 LAUNCH SEQUENCE | top, box-shadow | .1s | `ease` | press |
| Periscope | — | height | .5s | `cubic-bezier(.24,.9,.26,1)` | `scopeDown` |
| Fire control | — | transform | .42s | `cubic-bezier(.2,.85,.25,1)` | `weaponsOpen` |
| Fire control | — | opacity | .3s | `ease` | `weaponsOpen` |
| Fire control | 01 WEAPON SYSTEM | transform | .2s | `cubic-bezier(.3,1.5,.5,1)` | `sys`, `mount`, `sys`, `warhead` |
| Fire control | 03 CAPACITOR | top | .14s | `cubic-bezier(.3,1.5,.5,1)` | `sw` |
| Fire control | 03 CAPACITOR | background | .14s | `ease` | `sw` |
| Console strip | — | transform | .2s | `cubic-bezier(.3,1.5,.5,1)` | `gridTint` |
| Orders | — | left | .22s | `cubic-bezier(.3,1.4,.5,1)` | `ordersOpen` |
| Orders | ALERT CONDITION | transform | .2s | `cubic-bezier(.3,1.5,.5,1)` | `alert` |
| Orders | The repair orders | top | .12s | `cubic-bezier(.3,1.5,.5,1)` | `dcThrown` |
| Orders | The repair orders | background | .12s | `ease` | `dcThrown` |
| Helm | Collapsed piece | left | .3s | `cubic-bezier(.3,1.2,.5,1)` | `helmOpen` |
| Helm | 01 FLIGHT | transform | .12s | `linear` | `hdg` |

Looping animations:

6 keyframe sets, every one in use. All loop.

| Name | Keyframes | Line | Where | Timing | Runs |
|---|---|---|---|---|---|
| `rd-sweep` | `to transform:rotate(360deg)` | 216 | Plot · RADAR SWEEP | `rd-sweep {{ sweepDur }} linear infinite` | while `dradisOn` shows; `sweepDur` = `(5.4 - s.senBoost * 0.5).toFixed(1) + 's'` |
| `pw-core` | `0%,100% opacity:.5 · 50% opacity:1` | 2009 | Helm · 04 STABILITY | `pw-core .62s steps(1,end) infinite` | while `helmOpen` shows |
|  |  | 2010 | Helm · 04 STABILITY | `pw-core .62s steps(1,end) infinite` | while `helmOpen` shows |
| `bd-blink` | `0%,48% opacity:1 · 52%,100% opacity:.2` | 148 | Alarm strip · — | `bd-blink 1.1s steps(1) infinite` | while `unacked` shows |
|  |  | 253 | Plot · Own ship | `bd-blink 1.1s steps(1) infinite` | while `tracked` shows |
|  |  | 675 | Sensors · OWN EMISSIONS | `bd-blink 1.1s steps(1) infinite` | `s.ownSig >= 45 ? 'bd-blink 1.1s steps(1) infinite' : 'none'` |
|  |  | 893 | Comms · 01 RECEIVER | `bd-blink 1.1s steps(1) infinite` | `s.ownSig >= 45 ? 'bd-blink 1.1s steps(1) infinite' : 'none'` |
| `bd-drift-a` | `0%,100% transform:translate(0,0) · 50% transform:translate(9px,-11px)` | 288 | Plot · TRACK 07 destroyer | `bd-drift-a 17s ease-in-out infinite` | while `res_t07` shows |
| `bd-drift-b` | `0%,100% transform:translate(0,0) · 50% transform:translate(-12px,7px)` | 308 | Plot · FTR-2 fighter wing | `bd-drift-b 21s ease-in-out infinite` | always |
| `bd-ghost` | `0%,100% opacity:.38 · 50% opacity:.62` | 351 | Plot · Predicted position ghost | `bd-ghost 4s ease-in-out infinite` | always |

One-shot resets:

4 timeouts, each resetting a transient display state.

| In | Delay | Resets | Re-trigger |
|---|---|---|---|
| `stepDial` | 1100 ms | `activeDial` | restarts the delay |
| `runOrder` | 420 ms | `orderFail` | restarts the delay |
| `runOrder` | 420 ms | `orderFail` | restarts the delay |
| `flickBack` | 400 ms | `dcThrown` | restarts the delay |

- One animation-frame loop, started in `componentDidMount`: each frame adds 0.0009 to `scopeT` and calls `drawScope`, which draws the periscope canvas. It writes no state. The increment is per frame, so its speed depends on frame rate.
- One interval: `tickPower` every 420 ms (section 5).

## Drawn live, not assets

- **Plot:** black ground with two grid layers, a glow and tube effects. `textures/plot-grid.json` holds every value: 60.9 px per 1000 km at 1×, zoom 0.1 to 1, grid steps 5000 / 2000 / 1000 / 500 / 200 / 100 / 50 / 20 km with the step nearest 61 px on screen, major lines every 5 cells.
- **Periscope sight:** drawn every frame on a canvas inside the aperture, in `accent.alarm` red on `periscope.ground`. The drawing code is `drawScope` in `reference/board-logic.js`.
- **Steel frame:** `textures/steel_frame.png` is the board's shader output; stretch it to the full board.
- **Everything whose value changes:** bar cells, lit fader tracks, lever hazard fills, spectrum bars, the radar sweep and sensor envelope, contact markers and vectors, the helm heading ring, all text, and the plot marks placed with the rail tools (`mark_*` sprites; rules in `reference/UIStateSpec.md` §8.4). Their colours are tokens; their positions are in layout.json where they exist at load.

## Behaviour

`reference/UIStateSpec.md` is generated from the board and is the reference for porting: 67 click, 14 press-and-drag, 1 wheel and 1 text-input sites in the template, the power tick every 420 ms, every gate with its source, and how the panels reach each other. §10 lists every control with its hardware, action, the state it writes, its gates and their refusal text; §11 walks through each panel's sequences with their timings (for example, the hangar launch stages and their loading bars, the fire control arming ladder, comms tune-lock-decrypt). `reference/board-logic.js` is the logic itself.

## Open questions

Found while building this package and left as the board has them:

- Thrust cap diverges between the tick and the gate (found by simulation in gen-spec, UIStateSpec §7.3). `thrCap()` = ENG fill × 20 × engFactor and limits the throttle drag; the tick clamps an existing setting to ENG fill × 20 with no engFactor. 25 of 36 fill/integrity states differ: engine-room damage does not pull a set throttle down. Sensor gain agrees in all 36.
- Contacts sit at fixed plot percentages that ignore their range and bearing (TRACK 07 at 24,300 km is placed at 71% / 15%). The plot marks use true km scale, so a ruler to a contact will not read that contact's range.
- Own-ship label is hard-coded: "HDG 062° / V 1.84 km/s" and the ship glyph rotated 62°, while helm state hdg is 83°. Invented telemetry by the board's own rules.
- Four 4 px bolts on the board carry a broken style (`margin-border-radius:50%`) and render square. layout.json places them as found.
- Two engineering status lines use ▸ (U+25B8), which Share Tech Mono does not contain; the browser falls back to a system font. See Fonts.
- The drawer latches carry the tooltip "STOW", a verb on hardware, which the design rules forbid. Tooltips are not exported.
- Contact 22 boarding vector is purple #9B7FD4, outside the palette. Exported as found.
- The red grid uses #FF6E50, #FF7C58 and #FF6E46, and the red sweep #FF8C78 and #FF9682, not the grid tint #E04B3C named in the design rules. Exported as found.
- The PDC arc label uses a dim amber #8A6A2A. Exported as found.
- Four render values are computed and never read by the template: setToggles[].ink, compartments[].lamp, orderSwitches[].costInk, orderSwitches[].ink.
- The design rules list #6B757C (section headers) and #3E464C (well stencils). Neither renders on the board.
