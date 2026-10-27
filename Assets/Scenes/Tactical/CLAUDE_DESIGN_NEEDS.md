# What Claude Design Still Needs to Give Us

This is everything the Claude Design handoff package left out, grouped by what Claude Design has
to make or decide. It is built from `HANDOFF_GAPS.md` (rows 1–106) and was checked against the
package, the reference picture and the live scene. The "Gap" column gives the `HANDOFF_GAPS.md`
row, where the full detail is. "New" means the check found it, and it is not in that file yet.

**Package:** `Assets/UI2/Claude Design handoff/export/`, generated 2026-09-27.
**Board in Unity:** `Assets/Scenes/Tactical/02.1_Grid.unity`, first built in Steps 28–33. Its panels
are prefabs in `Assets/UI/Board/Panels`, edited directly.

**Note (updated 2026-10-05, asset audit):** `sprites/Starship_Silhouette.png` in the export was made by the owner, with ChatGPT image tools. It is usable, but it is **not** a Claude Design package file.

**Priority:**
- **High:** a player sees it as broken or missing, or a control cannot be used.
- **Medium:** it looks wrong or flat next to the design.
- **Low:** a small detail or tidy data.

---

## Bucket 1 — Ship and contact sprites (the things on the plot)

None of this art was exported. Every plot mock-up from the package is hidden in Unity now. Our ship
is its 3D model seen from above (Step 37). A contact is a bearing line, a dot, or its 3D model inside
visual range: red once identified, amber while BOGEY, grey as a wreck (Steps 44 and 75). Fighters
and boats are drawn from a wedge template (Step 54).

| # | Need | Where | Now in Unity | Priority | Gap |
|---|---|---|---|---|---|
| 1.1 | **Own ship glyph** (Starwolf, SW-01) as a sprite, with its heading line and arrowhead. | Plot centre. | **Done, Step 37:** our ship is its 3D model seen from above. | Done | 1, New |
| 1.2 | **Resolved hostile contact glyphs**: TRACK 07, CONTACT 17 and CONTACT 22, with how each looks while unresolved, resolved and targeted. Without them there is nothing to click to pick a target. | Plot. | Mock-ups hidden. A contact is a bearing line, a dot, or its 3D model (Steps 44, 75). | High | 95 |
| 1.3 | **Friendly wing mark** (FTR-2 fighter wing): one sprite per craft mark, plus the arrowhead on its vector. | Plot, lower left. | Mock-up hidden. Our fighters are drawn from a wedge template (Step 54). | Medium | 2, New |
| 1.4 | **BC-01 boat mark** (the diamond). | Plot, right. | Mock-up hidden. Our boats use the fighters' wedge. | Medium | 4, 23 |
| 1.5 | **Predicted-position ghost.** | Plot, lower right. | Mock-up hidden. | Medium | 3 |
| 1.6 | **Asteroid field rock marks.** | Plot, lower right. | Mock-up hidden. | Low | 5 |
| 1.7 | **Contact vectors** (movement lines): drawn #50, #52 and #53. They need colour stops and angles, or sprites. | Plot. | Mock-ups hidden. | Medium | 9 |
| 1.8 | **The purple boarding vector** (#9B7FD4). It shows only once contact 22 is resolved, and has no record at all. Sources disagree on whose it is: the README says Contact 22's; tokens.json says BC-01's course vector. Say whose it is, give its layout, and give a palette colour or keep the purple. | Plot. | Missing. | Medium | 38 |

---

## Bucket 2 — DRADIS, sensors and firing arcs on the plot

| # | Need | Where | Now in Unity | Priority | Gap |
|---|---|---|---|---|---|
| 2.1 | **Sensor range ring** (drawn #33). The package gives its colours (`senEdge`, `senGlow` in `board-logic.js` and `tokens.json`). **Missing:** its shape (a 1 px edge on a 396 px ring), the glow size, and an edge field on the record. The size follows array gain (300 + gain × 96 px) and eases over 0.5 s. | Plot centre. | **Done, Step 42:** drawn by the code from the package's colours, on the km scale. | Done | 6, 104, New |
| 2.2 | **The ring is not drawn to the map scale.** It is 300 + gain × 96 px across: 396 px at gain 1, a 198 px radius. The range it stands for is 12,000 + gain × 7,000 km. At 60.9 px per 1,000 km, 19,000 km is a 1,157 px radius. It also ignores zoom. Should it follow the km scale? Same question as 8.1, which asks it for the contacts. (updated 2026-10-05, asset audit) | Plot centre. | **Done, Step 42:** the ring follows the km scale and the zoom. | Done | New |
| 2.3 | **DRADIS sweep** (drawn #34, and probably #35, which has the same −58° turn). The package gives its colours (`sweepInk`, `sweepTail`, `sweepEdge`). **Missing:** its colour stops, or a sprite. It turns once every (5.4 − gain × 0.5) s and shows only while DRADIS is on. | Plot centre. | **Done, Step 43:** drawn and turned by the code. | Done | 6, 105, 12 |
| 2.4 | **Active scan cone on the plot.** It was never measured: active scan is off when the board loads, so no record exists. The cone's shape and size are in the package: `coneBrg` and `coneArc` in `board-logic.js`, sized like the ring (300 + gain × 96 px). Its plot colour is probably `coneInk`, but that is proven only on the sensors dial. Only its layout and look on the plot are missing. (updated 2026-10-05, asset audit) | Plot centre. | **Done, Step 78:** two bright edges and a faint fill, from the small circle round our ship out to the scan's range. | Done | 7 (corrected), 104 |
| 2.5 | **Firing arcs, one per mount.** The board has 5 mounts: 2 railgun turrets, 2 PDC groups and 1 missile launcher. The plot shows only PDC GROUP A: its edges are drawn #39 and #40 (170 px lines at bearings 7° and 77°), and its fill is drawn #38 (a 340 px disc). The fill is hidden in Unity, so the amber wedge is missing. Give arc art for every mount, and the rule for when each shows. Target picking depends on these arcs. No arc size per mount is given: the PDC arc is a fixed 340 px, not on the km scale. The all-round missile arc has no look at all. (updated 2026-10-05, asset audit) | Plot centre. | **Done, Steps 46 and 55:** the mock-up lines are hidden. With Fire Control open, a dashed ring shows the selected mount's reach. Each PDC group on AUTO shows its arc as a dashed sector. Railgun arcs are not drawn. | Done | 8 and 12 (corrected), New |
| 2.6 | **Passive bearing lines.** BRG 149 is drawn #36 (a 198 px dashed line) plus a small end ring, drawn #37. Give them as a repeatable template: bearing, length, dash, colour, opacity, and where the 0.6 opacity sits. BRG 048 (TRACK 07) has no line and no label at all. The package already gives most of the template in `passives`: length, colours, label and click. Only the dash size is missing; the picture shows about a 9 px dash and a 9 px gap. (updated 2026-10-05, asset audit) | Plot centre. | **Done, Step 44:** a bearing line on each heard contact, drawn by the code. The mock-up is hidden. | Done | 8 and 96 (corrected), 101, 14 |
| 2.7 | **Own-ship blink.** UIStateSpec §9.2 has the own ship blinking while it is tracked, but does not say which part blinks. | Plot centre. | **Done, Step 66:** the owner removed the blink. Our ship never blinks. | Done | New |
| 2.8 | **74 plot records were dropped from the export.** `layout.json` `meta.droppedAsPlotAnimation` lists 58 drawn shapes and 16 text runs. Say what they were, or export them. | Plot. | Missing. | Medium | New |
| 2.9 | **Inner dashed ring.** The reference picture shows a dashed ring about 300 px across, inside the sensor ring. It has no name and no record. Say what it is, and give its layout. (updated 2026-10-05, asset audit) | Plot centre. | Step 62 added a small circle with an unbroken line, just outside our ship. The DRADIS sweep and the scan cone start from it. It is not known if this is the ring in the picture. | Medium | New |

---

## Bucket 3 — Control art (the hardware in the drawers and stations)

| # | Need | Where | Now in Unity | Priority | Gap |
|---|---|---|---|---|---|
| 3.1 | **PDC and MISSILE controls.** The fire control drawer was laid out in RAILGUN mode only. Missing: the PDC quadrant buttons (FWD, STBD, AFT, PORT), the LOADERS button, the missile warhead selector, the SALVO knob, the missile mount's single tick, and the shot-cost mark's place for PDC and MISSILE. The hardware and sizes are already in `UIStateSpec.md` (warhead rotary selector, SALVO knob, LOADERS button, quadrant push buttons, the −90° tick). Only their positions on the board are missing. (updated 2026-10-05, asset audit) | Fire control drawer. | Not drawn. PDCs and missiles work through the system selector, the mount selector and the arming rockers (Steps 53, 55). The mount selector picks the warhead. The loaders and the salvo start from the ship's stats file. Their state shows in the readouts. | **High** | 73, 76 |
| 3.2 | **Engineering ship outline** (the schematic). Also a damage look for each compartment on it: fire, vented and radiation. | Engineering drawer. | **Outline done, Steps 50, 74 and 75:** our ship in four parts, coloured by side armour; the room rings change colour by health. Still needed: the fire, vacuum and radiation looks. | Medium | 62 |
| 3.3 | **Rockers as separate layers** (housing and pad). *Downgraded 2026-10-18 (Step 40): the pad now slides from cut-outs over a flat cover. Separate layers would let us drop the cover and keep the well's gradient and the thrown glow during the slide.* | Every rocker. | Slides from cut-outs. | Low | 102 |
| 3.4 | ~~**Push-button pressed state** (art or offsets).~~ *Not needed (2026-10-18): `button_recall_pressed` exists and is used. Only the .1 s ease is missing, which is ours.* | Hangar buttons. | Art swaps; no ease yet. | — | 103 |
| 3.5 | **Lit-cell looks**: power bus cells (lit and pending), capacitor cells, AUX cells, and the unlit stability cells. Also say which way the bus cells fill. | Power, Fire Control, Orders, Helm. | Stand-ins. | Medium | 47, 50, 75, 83, 79 |
| 3.6 | **Fill bars and hazard tracks**: hangar stage bars, CAT CHARGE, the launch lever's green track, and the fire lever's red track (size and colour). | Hangar, Fire Control. | Built from the code's colours. Lever tracks are stand-ins. | Medium | 66, 75 |
| 3.7 | **Gradients with no colours.** 26 shapes say "gradient" and give no stops: the gain fader track, the squadron bezel (the unselected one is missing entirely), the heading ring, the throttle track, the bearing tape and others. The sensors cone dial and the integrity rings have working versions built from the code, but they need the real stops. | Most drawers, helm, periscope. | 24 grey stand-ins, 2 hidden. | Medium | 12, 53, 54, 64, 71 |
| 3.8 | **Shadows with no values.** 63 shapes say they cast a shadow and give none. Most important is the whole-board edge shadow: without it the frame edges read 25–35% too bright. Also say which invisible shapes exist only to cast a shadow. Also needed: the plot's inner shadow, baked as a sprite, since Unity cannot blur it. | Frame, drawers, helm, plot edge. | Not built. | Medium | 13, 44, 10 |
| 3.9 | **Edge (outline) data on drawn shapes.** Records give only size, fill, radius and shadow. So any ring or outline drawn with an edge draws nothing. Examples: the sensor ring, the BRG end ring, and the inset edges on band rows, key dials, compartment cells and held attitude keys. Those inset edges are box-shadows in the package, not missing edge fields. Give edge width, colour and dash on every record. Also give the glows on the comms key dials and on the held attitude keys ("0 0 9px rgba(143,195,58,.45)"), or say they can be dropped. | Plot, Sensors, Comms, Engineering, Helm. | Some edges built by hand. Glows not built. | Medium | 55, 59, 63, 80, New |
| 3.10 | **Lamp glows.** Give a glow on every lamp that can light, with its size and strength. Several light with no glow in the export: the comms bands, the hangar lamps, the C2/C1 alert lamps, the CLASS lamp, and the rail lamps' selected glow. Settle the blank order lamps: the export gives them a glow, the code gives none. | Many drawers, both rails. | Glows copied or missing. Size estimated. | Medium | 15, 45, 57, 82, 86, 91 |
| 3.11 | **Periscope bearing tape ticks**: spacing, size, labels and colour. They scroll as the scope turns. | Periscope. | Stand-ins. Only the number moves. | Medium | 88 |

---

## Bucket 4 — Layout records that are missing

| # | Need | Where | Priority | Gap |
|---|---|---|---|---|
| 4.1 | The "CORE DERATED — N UNITS LOST TO DAMAGE" note: position, font, size and colour. | Power drawer. | Medium | 51 |
| 4.2 | The low-stability hazard indicator, and what the `pw-core` blink targets. | Helm · 04 STABILITY. | Medium | 28 |
| 4.3 | Contact board rows: row height, pitch, and the lit row's look. Only one row was measured. | Sensors drawer. | Medium | 52 |
| 4.4 | Engineering compartment rows: the background size and pitch for every row. | Engineering drawer. | Low | 63 |
| 4.5 | Craft 06's fuel bar, and the launch-bearing input box and its text style. | Hangar drawer. | Low | 67, 68 |
| 4.6 | Order costs ("30%", "25%", "15%", "RESVD"). Say where they go, or confirm they are dropped. | Orders station. | Low | 85 |
| 4.7 | Drawers measured in the wrong state: Power with the shed thrown, Engineering damaged (the game loads undamaged). Measure each drawer in its starting state, or say which state each was measured in. | Power, Engineering, all drawers. | Medium | 46, New |

---

## Bucket 5 — Layout rules (how rows behave when text changes)

| # | Need | Where | Priority | Gap |
|---|---|---|---|---|
| 5.1 | **How status rows lay out when text grows**: the engineering party line, the hangar readouts, the helm header row and the AUX row. At full charge the AUX row's "100%" is unreadable. | Engineering, Hangar, Helm, Orders. | **High** for the AUX row | 65, 69, 77, 84 |
| 5.2 | **Mark label positions**: where the POINT, RULER and COMPASS labels sit. Also, what `markScale` scales, and the COMPASS erase area. (The dash and dot sizes are in the package; see the end of this file.) | Plot marks. | Low | 97, 99, 100 |
| 5.3 | **Wrapper labels** ("X ( )", "ARC °", "REL BRG °"): export the wrapper text and the value as separate pieces, in order. | Plot axes, Sensors, Fire Control. | Low | 26, 56, 74 |

---

## Bucket 6 — Data format fixes (so a re-export doesn't break the build)

| # | Need | Gap |
|---|---|---|
| 6.1 | A **panel** field on every record. | 17 |
| 6.2 | A **draw-order (z)** number on every record. | 18 |
| 6.3 | **Shadow margins** for every image: four columns (left, top, right, bottom) in `asset-map.csv`. | 16 |
| 6.4 | **Placement** of the three conduit compositions. Drop the duplicate conduit and rail-icon records. | 19, 20, 21 |
| 6.5 | The **true (unturned) size** on every turned record. | 23 |
| 6.6 | **Raw text**, with leading spaces and line breaks kept. | 24, 25 |
| 6.7 | The **real turn angle** on every text record. | 27 |
| 6.8 | **Effective opacity**, with parent fades included. | 14 |
| 6.9 | **Correct state tags** on the periscope's inner parts. | 22 |
| 6.10 | **Edge fields** on drawn records (see 3.9). | New |

---

## Bucket 7 — Fonts

| # | Need | Priority | Gap |
|---|---|---|---|
| 7.1 | **▸ ▶ ◀** — **done (2026-10-05)**: they now draw from the free Noto Sans Symbols 2 fallback font. Ask only if the designer wants different glyphs. | Done, ours | 30, 36 |
| 7.2 | Optional: a 2× reference picture to tune the Share Tech Mono weight against. | Low | 31 |

---

## Bucket 8 — Decisions and confirmations (no art, just answers)

| # | Question | Gap |
|---|---|---|
| 8.1 | Contacts sit at fixed screen spots that ignore their range and bearing, so the ruler won't read their true range. Should contacts sit on the km scale? If so, give the placement rule. **Done, Step 34:** contacts are drawn at their true km position. | 33 |
| 8.2 | The own-ship label reads "HDG 062° / V 1.84 km/s" while the helm says 083°. Is it live, or a fixed mock-up? **Done, Step 39:** the owner removed the label. | 34 |
| 8.3 | Thrust cap: the tick and the throttle use different rules. Which is right? | 32 |
| 8.4 | Which element does `gInk` colour? Which does `classifyInk`? Does the AUX rate text change colour while feeding? | 78, 89, 87 |
| 8.5 | Confirm the bindings we inferred: the hangar bay lamps (door, press, cat), the periscope bracket label (the target name), and the comms blink (the SIG lamp). | 72, 94, 60 |
| 8.6 | `sigLamp` is defined twice, so the sensors emissions lamp follows the comms tuning. Which is intended? | 61 |
| 8.7 | Four render values are computed and never used, and two palette colours never render. Use them or drop them? | 41, 42 |
| 8.8 | TRACK 07's class is named two ways: "destroyer" in UIStateSpec, CRUISER in `CLASSES`. Which? **Done, Step 75 (BUILD_LOG decision 39):** the owner made it a Garuda-class cruiser. It reads GARUDA once scanned and BOGEY T07 before. | New |
| 8.9 | Grid tint: the owner chose white and amber, not red. Confirm the amber values, and drop red (including the red sweep). | 39 |
| 8.10 | The PDC arc label's dim amber (#8A6A2A) is outside the palette. Keep it, or give the token. | 40 |
| 8.11 | Should the plot scale labels (−30, −20 … "1,000 KM") change with zoom? **Done, Step 76:** the axis numbers are distances in km from our ship. They move with the ship and spread out as you zoom in. | 48 |
| 8.12 | Periscope sight: keep it drawn live? What drift speed? Is the aperture's round clip hard-edged or soft? | 11, 92, 93 |
| 8.13 | Mount short label colour in the periscope header: static, or a rule? | 90 |
| 8.14 | Four 4 px bolts render square. Round or square? | 35 |
| 8.15 | Do the alarm strip and the orders header sit inside the plot, so the wheel zooms over them? | 49 |
| 8.16 | Are the periscope rods meant to overlap the "PERISCOPE / HOUSED" labels? | 29 |
| 8.17 | Craft tag numbers are the same colour as their fuel bars, so a full bar hides the tag. Is that intended? | 70 |
| 8.18 | Comms receiver bar geometry: we worked it out (rim radius 63, width 2). Please confirm. | 58 |
| 8.19 | The A/D key repeat rate. This is our game-feel call; ask only for the design intent. | 81 |
| 8.20 | The drawer latch tooltip text. "STOW" breaks the design rules. | 37 |
| 8.21 | Please state in the README that the design blends colours in gamma space, and how Unity must match it. | 43 |
| 8.22 | (added 2026-10-17, Step 35) The OWN EMISSIONS note has no line for signal above 0, below the tracked line, and rising or steady. This is new: the owner made every unit of power add signal, where the design only counted ACTIVE SCAN. Our stand-in is "POWER SIGNATURE — NOT YET RESOLVED". Please give the line. Also: should the meter label stay "OWN EMISSIONS" or become "SIGNAL", to match the owner's term? (Still open with the owner.) | 106 |

---

## Worth asking for at the same time

These were never promised, but the finished game needs them. The same designer should make them so
they match.

- **Weapon tracks on the plot**: our missile in flight, an incoming hostile missile, PDC intercepts, and a railgun round. With no outside view, the plot is the only place combat can be seen.
- **Ship class glyphs**: hostile cruiser, corvette and frigate, plus our own fighter and boat. The design names these classes (`CLASSES`) but has no art for them.
- **Contact states**: new contact, lost contact (last known position), and destroyed or wreck.
- **A contact glyph template for any contact**, so contacts other than the five mock-ups can be shown.
- **Periscope target silhouettes**, one per hull class, for the CLASS readout. The owner's `Starship_Silhouette.png` covers our own ship, for the hangar and other large views. The periscope only shows other ships, so it still needs one silhouette per hostile class. (updated 2026-10-05, asset audit)

---

## Ours to fix, not Claude Design's

The check found these. They are Unity work, so they stay off the request.

- **Plot marks**: the package gives the mark dash as 3 px with a 2 px gap (`mark_dash`), the point dot as 7 px and the end dots as 6 px (`asset-map.csv`). Unity uses dash 4.5 with gap 3. Match the package. Corrects gap row 98. (done 2026-10-05)
- **The "SENSOR RANGE" label** should move with gain (`senCapTop`). (done 2026-10-05) The owner removed the label in Step 39.
- **The ▸ ▶ ◀ glyphs** (7.1): add an OFL fallback font. (done 2026-10-05)
- **`HANDOFF_GAPS.md` rows 7, 8, 12 and 44** have the plot records wrong. Correct them as in 2.4, 2.5 and 2.6 above. Row 44 treats #33 and #37 as shadow-only, but they are the visible sensor ring and the BRG 149 end ring, drawn with an edge the package did not export. (done 2026-10-05)
