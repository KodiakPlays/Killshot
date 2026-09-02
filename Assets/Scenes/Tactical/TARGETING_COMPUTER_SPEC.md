# Targeting Computer — Build Spec

## What it is

The targeting computer is a drawer that slides out from the right edge of the tactical board. It is how the player shoots. The player picks one of five weapon mounts, and the plot immediately shows which enemy contacts that mount can reach. The player clicks one of those contacts to designate it. The drawer fills in the range and bearing, and the player can trim both by hand. Then the player throws three physical toggle switches — one at a time, like a real weapons console — and only then does the FIRE button come alive. It is a deliberate, multi-step ritual. Nothing fires by accident.

This document covers README sections **5b (mount selection)**, **5c (firing solution inputs)**, **5d (interlocks)** and **5e (FIRE)**. Section **5a (the periscope scope)** is a separate later step; its space is reserved below and its container is specified, but its contents are not.

**Source of every number here:** `References/Tactical spaceship UI planning.zip` → `design_handoff_tactical_command_board/`, files `README.md` and `Star Wolf Board Live.dc.html`. The markup in `Star Wolf Board Live.dc.html` lines 302–503 is the assembly. All pixel heights below were measured by rendering that file in Chrome with the real Google fonts loaded. Where the README and the markup disagree, the disagreement is called out.

---

## 1. The drawer shell

Design canvas is 1920 × 1080. The drawer is an overlay, not part of the layout flow.

| Property | Value |
|---|---|
| Position | `right: 104px, top: 100px` — so x 1440 → 1816, y 100 → 930 |
| Size | 376 × 830 |
| Clip container | `overflow: hidden`, does not take input |
| Panel background | linear gradient 158°, `#23282D` → `#101315` |
| Border | 1px `rgba(255,255,255,.16)` |
| Corner radius | 7px |
| Top lip | inset shadow `0 1px 0 rgba(255,255,255,.14)` |
| Cast shadow | `-18px 0 50px rgba(0,0,0,.8)` |
| Layout | vertical stack, blocks in the order below |

**Rivets.** Four, one per corner. 8 × 8px, inset 7px from each edge. Radial gradient, centre at 34% 28%, `#99A3AB` → `#0F1214` at 72%. Drop shadow `0 1px 1px rgba(0,0,0,.7)`.

**Slide animation.** Closed = translated +376px on X and opacity 0. Open = 0 and opacity 1. Transform takes 0.42s on `cubic-bezier(.2,.85,.25,1)`. Opacity takes 0.3s ease. It pulls out; it does not pop.

**Header.**

| Property | Value |
|---|---|
| Height | 38px (9px padding, 19px line box, 9px padding, 1px bottom border) |
| Padding | 9px 16px |
| Background | `rgba(224,75,60,.07)` |
| Bottom border | 1px `rgba(255,255,255,.1)` |
| Title | `TARGETING COMPUTER`, Barlow Condensed 16px / 600, letter-spacing .11em, `#E8EDF0` |
| Mount code | `RG-A` etc., Share Tech Mono 10px, letter-spacing .14em, `#E04B3C`, 10px gap, baseline-aligned to the title |
| Close | `×`, 15px, `rgba(200,210,218,.5)`, 3px horizontal padding, right-aligned |

---

## 2. Vertical layout budget

Two budgets, because the reference does not add up.

The blocks in the drawer want **886px** of height. The drawer is **830px**. Flexbox absorbs the 56px shortfall entirely out of the scope, because the scope is the only block with a fixed height and `overflow: hidden` (everything else is held open by its own content). So the scope renders at **170px**, not the 226px the README states.

### As rendered (what the reference actually looks like)

| Y (from drawer top) | Block | Height |
|---|---|---|
| 0 | Top border | 1 |
| 1 | Header | 38 |
| 39 | Gap (scope margin-top) | 12 |
| 51 | **Scope — 5a, reserved** | **170** |
| 221 | Gap (scope margin-bottom) | 12 |
| 233 | Solution readout strip | 38 |
| 271 | **5b — Mount selection** | **196** |
| 467 | **5c — Firing solution** | **119** |
| 586 | **5d — Interlocks** | **140** |
| 726 | **5e — FIRE** | **103** |
| 829 | Bottom border | 1 |
| **830** | **Total** | |

### As authored (scope at its declared 226px)

Same table with the scope at 226 and everything after it pushed down 56px. Total **886px**. The FIRE block would run from y 782 to 885.

### Which to build

Pick one and commit. Either:

- **Drawer 886px tall, scope 226px.** Honours the README. Bottom edge lands at y 986, which collides with the hardware row (y 954 → 1058).
- **Drawer 830px tall, scope 170px.** Matches what the reference renders. Recommended, unless the scope work in 5a needs the extra height.

There are no vertical gaps between blocks 5b–5e. Each block carries its own 12px top padding, so the gaps you see are padding, not margin.

**Known defect in the reference, for information:** the drawer's bottom-right corner (x 1620 → 1816, y 844 → 930) sits underneath the EXECUTE plate in the order row, which is drawn later and therefore on top. In the reference at 1920×1080 with the drawer open, the FIRE button is completely hidden. Killshot has no EXECUTE button (settled decision), so this should not recur, but do check whatever occupies that corner.

---

## 3. Reserved: 5a, the scope

Do not build the contents. Do build the container, so the space is held.

| Property | Value |
|---|---|
| Margin | 12px on all four sides |
| Size | 350 × 226 declared (170 as rendered — see section 2) |
| Corner radius | 4px |
| Background | `#0B0405` (token `scope-glass`) |
| Inner ring | inset `0 0 0 1px rgba(224,75,60,.3)` |
| Recess | inset `0 4px 26px rgba(0,0,0,.95)` |
| Clipping | `overflow: hidden` |
| Cursor | `ew-resize` — the whole panel is a horizontal drag surface |

**Boundary.** The scope block occupies y 51 → 221 (or → 277 at 226px). Everything below is in this spec. One behaviour crosses the boundary: **slewing the scope off the target drops the SOLUTION interlock**, and the SOLUTION interlock cannot be thrown unless the scope is ON BEARING (|offset| ≤ 4°). See section 7.

---

## 4. Solution readout strip

Sits between the scope and the mount list. The README does not number it. It is not part of 5a and not part of 5b, but it is in the budget so it is specified here.

| Property | Value |
|---|---|
| Margin | 0 12px (width 350) |
| Padding | 7px 13px |
| Height | 38px |
| Corner radius | 3px |
| Background | `#0B0405` |
| Inner ring | inset `0 0 0 1px rgba(224,75,60,.22)` |

| Element | Type | Value |
|---|---|---|
| Range (left) | Share Tech Mono 22px, letter-spacing .16em, `#E04B3C`, glow `0 0 12px rgba(224,75,60,.6)` | `24,300` — thousands-separated, or `—————` with no target |
| TOF (right) | Share Tech Mono 10px, letter-spacing .12em, `rgba(224,75,60,.7)` | `TOF 93 s` — computed as `round(range / 260)` |
| BRG (right) | same | `BRG 048` — three digits, zero-padded |
| Gap | 14px between TOF and BRG | |

---

## 5. 5b — Mount selection

### Block

| Property | Value |
|---|---|
| Padding | 12px 12px 0 |
| Layout | vertical stack, 5px gap |
| Content width | 350px |
| Block height | 196px = 12 padding + 9 label + 5 gap + (5 × 30 rows) + (4 × 5 gaps) |
| Section label | `01  MOUNT` — Share Tech Mono 8px, letter-spacing .2em, `#6E7B85`, 9px line box |

Note: the label is `01` then two spaces then `MOUNT` (the source uses a double non-breaking space).

### Row

| Property | Value |
|---|---|
| Size | 350 × **30** (16px content + 12px padding + 2px border) |
| Padding | 6px 10px |
| Border | 1px `rgba(255,255,255,.08)` |
| Corner radius | 3px |
| Background | `rgba(22,26,30,.7)` |
| Layout | name left, range right, vertically centred |
| Gap to next row | 5px |
| Cursor | pointer |

> The README says rows are **26px** tall. The markup renders **30px**. No 26px value appears anywhere in the source. Build 30px.

| Text | Font | Size | Weight | Letter-spacing | Colour |
|---|---|---|---|---|---|
| Mount name | Barlow Condensed | 13px | 500 | .09em | `#C9D2D8` |
| Max range | Share Tech Mono | 10px | 400 | none (0) | `#6E7B85` |

### Selected state

Three additions, all drawn over the row:

1. **Outline** — a 1px `#E04B3C` border at inset −1px, radius 3px. It sits exactly on the row's border box, replacing the grey border visually.
2. **Inner glow** — inset `0 0 14px rgba(224,75,60,.22)`.
3. **Left bar** — 3px wide, `#E04B3C`, full height of the row's padding box (28px), corner radius `3px 0 0 3px` (rounded on the left only).

Text colours do not change when selected.

### The five mounts

Only **Mount** and **Max range** are shown in the row. The other columns are data the mount carries; they drive the plot's fire arcs, the payload cycler and the mount-config panel in the order row.

| Mount | Short | Arc centre | Half-arc | Max range | Loaded | Magazine | Payloads | Detail |
|---|---|---|---|---|---|---|---|---|
| RAILGUN TURRET A | RG-A | 042° | ±80° | 30,000 km | 2 / 2 | 22 | SLUG, SABOT | DORSAL · TWIN |
| RAILGUN TURRET B | RG-B | 250° | ±80° | 30,000 km | 1 / 2 | 19 | SLUG, SABOT | VENTRAL · TWIN |
| PDC GROUP A | PDC-A | 042° | ±35° | 4,000 km | BELT | 2400 | FLECHETTE | STBD · AUTO |
| PDC GROUP B | PDC-B | 200° | ±35° | 4,000 km | BELT | 2180 | FLECHETTE | PORT · AUTO |
| MISSILE VLS | VLS | — | all-round (±180°) | 40,000 km | 8 / 8 | 8 | HE, EMP, PENETRATOR | SALVO OF 4 |

Range is displayed thousands-separated with a ` km` suffix. Arc is displayed elsewhere as `ARC 042° ±80°`, or `ALL ROUND` when half-arc ≥ 180.

### Behaviour

Clicking a row selects that mount and **resets everything downstream**: target cleared, payload index back to 0, trim cleared, all three interlocks off, fired flag cleared.

A hostile contact is valid for the selected mount when `range ≤ mount.range` **and** `angularDifference(track.bearing, mount.arcCentre) ≤ mount.halfArc`. Zero valid targets is a legitimate state, not an error.

---

## 6. 5c — Firing solution inputs

### Block

| Property | Value |
|---|---|
| Padding | 12px 12px 0 |
| Layout | vertical stack, 5px gap |
| Block height | 119px = 12 padding + 9 label + 5 gap + 45 row + 5 gap + 43 payload |
| Section label | `02  FIRING SOLUTION` — Share Tech Mono 8px, letter-spacing .2em, `#6E7B85` |

### RANGE and BEARING

Side by side, 6px gap. RANGE takes flex 1, BEARING takes flex .82 — measured widths **187px** and **157px**.

| Property | Value |
|---|---|
| Height | 45px |
| Padding | 6px 9px |
| Background | `#080A0B` |
| Border | 1px `rgba(255,255,255,.11)` |
| Corner radius | 3px |
| Layout | label on top, value row below, 3px gap |

| Element | Font | Size | Letter-spacing | Colour | Line box |
|---|---|---|---|---|---|
| Label (`RANGE km`, `BEARING`) | Share Tech Mono | 8px | .16em | `#6E7B85` | 9px |
| Value | Share Tech Mono | 15px | none | `#E8EDF0` | 17px |

Value formats: RANGE is thousands-separated with no unit in the value (the unit is in the label). BEARING is three digits zero-padded with a `°` suffix. With no target, RANGE shows `—————` and BEARING shows `———`.

### Stepper

| Property | Value |
|---|---|
| Glyphs | ▲ (U+25B2) above, ▼ (U+25BC) below |
| Font size | 9px, line-height 1 |
| Colour | `#8A959D` |
| Each glyph box | 9 × 9px |
| Gap between them | 1px |
| Total stack | 19px tall, 9px wide |
| Position | right end of the value row, vertically centred |

| Field | Step per press |
|---|---|
| RANGE | ±100 km |
| BEARING | ±1° |

Both fields seed from the designated target and carry a manual trim offset on top. The stepper adjusts the trim, not the base value. Pressing a stepper clears the fired flag.

### PAYLOAD

| Property | Value |
|---|---|
| Size | 350 × 43 |
| Padding | 6px 9px |
| Background | `#080A0B` |
| Border | 1px `rgba(255,255,255,.11)` |
| Corner radius | 3px |
| Layout | label + value in a left column (3px gap), counter on the right, vertically centred |
| Cursor | pointer — the whole field is the button |

| Element | Font | Size | Letter-spacing | Colour |
|---|---|---|---|---|
| Label `PAYLOAD` | Share Tech Mono | 8px | .16em | `#6E7B85` |
| Value (`SLUG`) | Share Tech Mono | 15px | none | `#E8EDF0` |
| Counter (`1 / 2  ▶`) | Share Tech Mono | 9px | .12em | `#6E7B85` |

Clicking cycles to the next payload in the mount's list and wraps. The counter reads `1 / n` when the mount has more than one payload, and `FIXED` when it has exactly one. The ▶ is U+25B6, separated from the counter by two spaces.

> In the reference the counter is hardcoded to `1 / n` — it does not track the current index. That is a bug. Show the real index.

---

## 7. 5d — Interlocks

Three skeuomorphic bat-handle toggles in a recessed tray. They are the safety ritual: all three must be up before FIRE goes live.

### Block

| Property | Value |
|---|---|
| Padding | 12px 14px 0 |
| Layout | vertical stack, 7px gap |
| Block height | 140px = 12 padding + 9 label + 7 gap + 112 tray |
| Section label | `03  INTERLOCKS — ALL THREE` — Share Tech Mono 8px, letter-spacing .2em, `#6E7B85` |

### Tray

| Property | Value |
|---|---|
| Size | 346 × 112 |
| Padding | 9px 6px |
| Corner radius | 4px |
| Background | `rgba(0,0,0,.34)` |
| Top lip | inset `0 1px 0 rgba(255,255,255,.07)` |
| Layout | three equal columns, 8px gap; each column 106px wide |
| Column | vertical, centred, 6px gap: bezel then label |

### Bezel plate

| Property | Value |
|---|---|
| Size | 46 × 64 |
| Corner radius | 4px |
| Background | linear gradient 158°, `#3A4046` → `#14171A` |
| Lit top lip | inset `0 1px 0 rgba(255,255,255,.2)` |
| Shadowed base | inset `0 -1px 0 rgba(0,0,0,.7)` |
| Cast shadow | `0 3px 7px rgba(0,0,0,.7)` |
| Cursor | pointer |

**Rivets.** Two, 3 × 3px, radial gradient centre 34% 28%, `#99A3AB` → `#0F1214` at 72%. **No drop shadow** (unlike the drawer's corner rivets). Positions: one at left 3px / top 3px, one at right 3px / bottom 3px — diagonally opposed, not four corners.

### Slot

Recessed, cut into the bezel.

| Property | Value |
|---|---|
| Inset | 11px left and right, 9px top and bottom |
| Resulting size | 24 × 46 |
| Corner radius | 11px (fully round ends) |
| Colour | `#090B0C` |
| Inner shadow | inset `0 2px 5px rgba(0,0,0,.9)` |

### Bat lever

16 × 24px. It physically moves between the two ends of the slot. Travel is 16px. Horizontally it is at left 15px, centred in the 46px bezel and sitting 4px inside each side of the slot.

| | Off | On |
|---|---|---|
| Vertical position | `bottom: 12px` → y 28–52 from bezel top | `top: 12px` → y 12–36 from bezel top |
| Corner radius | `4px 4px 7px 7px` (rounder at the bottom) | `7px 7px 4px 4px` (rounder at the top) |
| Fill | linear gradient 158°, `#B9C2C9` → `#5C666E` | linear gradient 158°, `#F0A020` → `#8A5A10` |
| Cast shadow | `0 -2px 5px rgba(0,0,0,.6)` — **upward** | `0 3px 7px rgba(0,0,0,.6)` — **downward** |
| Glow | none | `0 0 14px rgba(240,160,32,.5)` |
| Top highlight | inset `0 1px 0 rgba(255,255,255,.5)` | inset `0 1px 0 rgba(255,255,255,.45)` |

The shadow direction is the whole trick. Off, the lever is at the bottom and throws its shadow up the slot. On, it is at the top and throws its shadow down. Get this backwards and the lever reads flat.

### Labels

Two lines, centred, under the bezel. Share Tech Mono 8px, letter-spacing .13em, line-height 1.5 (12px per line, 24px total). Main line `#8A959D`, sub-label `#5B666F`.

| Interlock | ID | Main label | Sub-label | Precondition |
|---|---|---|---|---|
| Gyro spin | `pwr` | `GYRO SPIN` | `MOUNT STAB` | none |
| Solution | `sol` | `SOLUTION` | `FEED MOUNT` | **refuses to throw unless the scope is ON BEARING** (\|offset\| ≤ 4°); drops automatically if the scope slews off |
| Safety | `sfy` | `SAFETY` | `INTERLOCK` | none |

> The reference's JS carries longer sub-labels (`MOUNT STABILISED`, `FEED TO MOUNT`, `INTERLOCK CLEAR`) in an unused constant. The rendered markup and the README both use the short forms. Use the short forms.

### Behaviour

- Clicking a bezel toggles that interlock.
- SOLUTION only accepts being turned **on** when on bearing. It can always be turned off.
- Slewing the scope while SOLUTION is on turns SOLUTION off. The other two are unaffected.
- Any toggle clears the fired flag.
- Changing mount or target turns all three off.

---

## 8. 5e — FIRE

### Block

| Property | Value |
|---|---|
| Padding | 11px 12px 14px |
| Layout | vertical stack, 7px gap; pinned to the bottom of the drawer |
| Block height | 103px = 11 padding + 10 status + 7 gap + 61 button + 14 padding |

### Status line

Share Tech Mono 9px, letter-spacing .14em, `#8A959D`, centred, 10px line box, full 350px width.

| Message | Shown when |
|---|---|
| `DESIGNATE A TARGET` | no target designated |
| `THROW ALL THREE INTERLOCKS` | target designated, but at least one interlock is down |
| `SOLUTION LOCKED — MOUNT READY` | target designated **and** all three interlocks up |

The dash in the third message is an em dash (U+2014).

### Button — dead state

This is the base face. It is always drawn; the live and fired states are overlays on top of it.

| Property | Value |
|---|---|
| Size | 350 × 61 |
| Padding | 15px 0 |
| Corner radius | 5px |
| Background | linear gradient 158°, `#2A1512` → `#160809` |
| Border | 1px `rgba(224,75,60,.3)` |
| Top lip | inset `0 1px 0 rgba(255,255,255,.09)` |
| Text | `FIRE`, Barlow Condensed 24px / 600, letter-spacing .26em, centred |
| Text colour | `rgba(224,75,60,.4)` |

The dead face is identical whether or not a target exists. Only the status line above it changes.

### Button — live state

Requires a designated target **and** all three interlocks up. Two overlays go on top of the dead face:

**Overlay 1 — the frame (this is what blinks).**

| Property | Value |
|---|---|
| Corner radius | 5px |
| Border | **2px** `#E04B3C` |
| Fill | `rgba(224,75,60,.16)` |
| Outer glow | `0 0 26px rgba(224,75,60,.6)` |
| Inner glow | inset `0 0 26px rgba(224,75,60,.28)` |
| Blink | 0.9s period, `steps(1)` — a hard cut, not a fade |

Blink keyframes: opacity **1.0** from 0% to 48% of the cycle, then opacity **0.2** from 52% to 100%. At 0.9s that is 432ms bright, 468ms dim, switching instantly.

**Overlay 2 — the text.** `FIRE`, Barlow Condensed 24px / 600, letter-spacing .26em, colour `#FF6A58`, centred. **This does not blink.** Only the frame does.

### Button — ROUNDS AWAY

Set when FIRE is clicked while live. A solid overlay replaces the whole face.

| Property | Value |
|---|---|
| Fill | solid `#E04B3C` |
| Corner radius | 5px |
| Text | `ROUNDS AWAY`, Barlow Condensed **17px** / 600, letter-spacing **.2em** |
| Text colour | `#140506` |
| Animation | none — it is static |

The reference has no exit timing for this state. It clears only when the player does something else: flips an interlock, presses a stepper, slews the scope, or picks a different mount or target. Killshot will need to decide how long it holds.

### Behaviour

Clicking FIRE does nothing unless all conditions are met. There is no error feedback on a dead press.

---

## 9. Colours

Tokens are the handoff's own names where it has one.

### Semantic

| Token | Value | Used in the drawer for |
|---|---|---|
| `hostile` | `#E04B3C` | Header mount code, selected mount outline and left bar, scope ring, solution readout number, live FIRE border, ROUNDS AWAY fill |
| `orders-bright` | `#F0A020` | Interlock lever ON gradient start, lever glow |
| `own-ship` / Primary text | `#E8EDF0` | Header title, field values |
| Secondary text | `#C9D2D8` | Mount row names |
| Label text | `#6E7B85` | Section labels, field labels, mount range values, payload counter |
| Disabled | `#5B666F` | Interlock sub-labels |
| Disabled (alt) | `#4E5860` | Not used in the drawer |

### Surfaces

| Token | Value | Used for |
|---|---|---|
| `metal-mid` | `#23282D` | Drawer gradient start |
| `metal-shadow` | `#101315` | Drawer gradient end |
| `scope-glass` | `#0B0405` | Scope background, solution readout background |
| — | `#080A0B` | Firing solution field backgrounds |
| — | `#090B0C` | Interlock slot |
| — | `#3A4046` | Interlock bezel gradient start |
| — | `#14171A` | Interlock bezel gradient end |
| — | `#2A1512` | FIRE dead gradient start |
| — | `#160809` | FIRE dead gradient end |
| — | `rgba(22,26,30,.7)` | Mount row background |
| — | `rgba(0,0,0,.34)` | Interlock tray background |

### Specific values

| Value | Where |
|---|---|
| `#99A3AB` | Rivet highlight (gradient start) |
| `#0F1214` | Rivet shadow (gradient end, at 72%) |
| `#8A959D` | Stepper arrows, interlock main labels, FIRE status line |
| `#B9C2C9` | Lever OFF gradient start |
| `#5C666E` | Lever OFF gradient end |
| `#8A5A10` | Lever ON gradient end |
| `#FF6A58` | Live FIRE text |
| `#140506` | ROUNDS AWAY text |
| `rgba(255,255,255,.16)` | Drawer border |
| `rgba(255,255,255,.14)` | Drawer top lip |
| `rgba(255,255,255,.11)` | Firing solution field borders |
| `rgba(255,255,255,.1)` | Header bottom border |
| `rgba(255,255,255,.08)` | Mount row border |
| `rgba(255,255,255,.09)` | FIRE dead top lip |
| `rgba(255,255,255,.07)` | Interlock tray top lip |
| `rgba(255,255,255,.2)` | Bezel lit top lip |
| `rgba(255,255,255,.5)` / `.45` | Lever top highlight, off / on |
| `rgba(200,210,218,.5)` | Header close × |
| `rgba(224,75,60,.07)` | Header band |
| `rgba(224,75,60,.16)` | Live FIRE fill |
| `rgba(224,75,60,.22)` | Selected mount inner glow, solution readout ring |
| `rgba(224,75,60,.28)` | Live FIRE inner glow |
| `rgba(224,75,60,.3)` | FIRE dead border, scope ring |
| `rgba(224,75,60,.4)` | FIRE dead text |
| `rgba(224,75,60,.6)` | Live FIRE outer glow, readout number glow |
| `rgba(224,75,60,.7)` | Readout TOF / BRG text |
| `rgba(240,160,32,.5)` | Lever ON glow |
| `rgba(0,0,0,.34)` / `.6` / `.7` / `.9` / `.95` | Tray fill, lever shadows, bezel base and cast shadow, slot inner shadow, scope recess |

---

## 10. Type

Two families only. No italics. Labels are always uppercase. Numbers snap, never tween.

**Barlow Condensed** — used for names and titles. Weights 400, 500, 600.

| Use | Size | Weight | Letter-spacing | Colour |
|---|---|---|---|---|
| Header title `TARGETING COMPUTER` | 16px | 600 | .11em | `#E8EDF0` |
| Header close `×` | 15px | 400 | none | `rgba(200,210,218,.5)` |
| Mount row name | 13px | 500 | .09em | `#C9D2D8` |
| Stepper arrows ▲ ▼ | 9px | 400 | none (line-height 1) | `#8A959D` |
| FIRE label | 24px | 600 | .26em | `rgba(224,75,60,.4)` dead / `#FF6A58` live |
| ROUNDS AWAY | 17px | 600 | .2em | `#140506` |

**Share Tech Mono** — used for every number, code and label. Regular weight only.

| Use | Size | Letter-spacing | Colour |
|---|---|---|---|
| Header mount code | 10px | .14em | `#E04B3C` |
| Solution readout range | 22px | .16em | `#E04B3C` |
| Solution readout TOF / BRG | 10px | .12em | `rgba(224,75,60,.7)` |
| Section labels (`01 MOUNT` etc.) | 8px | .2em | `#6E7B85` |
| Mount row range | 10px | none | `#6E7B85` |
| Field labels (`RANGE km`, `BEARING`, `PAYLOAD`) | 8px | .16em | `#6E7B85` |
| Field values | 15px | none | `#E8EDF0` |
| Payload counter | 9px | .12em | `#6E7B85` |
| Interlock labels | 8px | .13em (line-height 1.5) | `#8A959D` main / `#5B666F` sub |
| FIRE status line | 9px | .14em | `#8A959D` |

Both fonts are Google Fonts under the SIL Open Font License. Bundle them locally.

---

## 11. State and reset rules

State the drawer owns:

```
mount        enum   RGA | RGB | PDA | PDB | VLS
target       id | null
sw           { pwr, sol, sfy : bool }
payloadIdx   int    index into the mount's payload list
trim         { range, bearing : number }   manual offsets
fired        bool
scopeBrg     number  0-360  (5a)
```

Derived: `onBearing` = |target bearing − scope bearing| ≤ 4°. `allSet` = target exists **and** all three switches on.

| Action | Clears |
|---|---|
| Change mount | target, payloadIdx, trim, all three switches, fired |
| Change target | trim, all three switches, fired |
| Slew scope off bearing | SOLUTION only, and fired |
| Press a stepper | fired |
| Toggle any interlock | fired |
| Cycle payload | nothing |

Clicks on contacts outside the current mount's envelope are ignored — not rejected with feedback, just ignored.

---

## 12. Unity notes

Things that do not port straight from CSS to Unity UI. Every one of these needs a decision before the first prefab.

**1. A solid Image is a fill, not a border.** UGUI has no border property. Every bordered box here — the mount rows (1px), the firing solution fields (1px), the FIRE button (1px dead, 2px live) — needs a 9-sliced sprite with the stroke baked into the edge pixels, tinted at runtime. Build one sprite per (radius, stroke width) pair. The set needed is: r3/1px, r4/1px, r5/1px, r5/2px, r7/1px. Do not use `Outline` — it is a text effect that duplicates the mesh, and it will not give you a clean 1px rectangle.

**2. There is no border-radius.** Same answer: 9-sliced sprites. Radii in use are 3, 4, 5, 7 and 11px. The interlock lever needs two sprites with mixed radii (`4/4/7/7` off, `7/7/4/4` on) — author one and flip it vertically for the other.

**3. `box-shadow` has no equivalent at all.** Four different effects hide behind that one property here, and each needs a different technique:

- *Four-sided bevel* (`inset 0 1px 0 white20%, inset 0 -1px 0 black60%, …`). Bake it into the 9-slice sprite. Or, if the sprite must stay flat-tinted, add two 1px-tall child Images stretched across the top and bottom edges. Bake it — it is cheaper and there are a lot of these.
- *Cast shadow* (`0 3px 7px rgba(0,0,0,.7)` on the bezel, `-18px 0 50px rgba(0,0,0,.8)` on the drawer). A separate blurred 9-slice sprite drawn behind, offset by the shadow's x/y and grown by roughly the blur radius. The drawer's shadow is large and one-sided; author it as its own sprite rather than deriving it.
- *Outer glow* (`0 0 26px rgba(224,75,60,.6)` on live FIRE, `0 0 14px rgba(240,160,32,.5)` on the lever). A soft radial 9-slice behind the element, larger than it by the glow radius, on an additive or premultiplied material. Do not tint it into the element itself.
- *Inner glow and recess* (`inset 0 0 26px` on live FIRE, `inset 0 4px 26px rgba(0,0,0,.95)` on the scope, `inset 0 2px 5px rgba(0,0,0,.9)` on the interlock slot). A 9-slice whose gradient runs from the edge inward, drawn **on top** of the fill. The recess variants are directional — the darkening is offset downward — so bake the offset into the sprite.

**4. There are no CSS gradients.** Every gradient here is 158°, which is a shallow diagonal running top-left to bottom-right. There are only five distinct gradient pairs (drawer, bezel, lever off, lever on, FIRE dead). Bake each as a small sprite — 64×64 at the right aspect is plenty — rather than reaching for a vertex-colour gradient component, because 158° is not axis-aligned and a two-colour vertex tint will not match it.

**5. Rivets are radial gradients.** Bake once. Two sizes needed: 8px (drawer corners, with a `0 1px 1px black70%` drop shadow baked in) and 3px (interlock bezels, no shadow). Reuse the same sprite everywhere.

**6. `text-shadow` on the big range number** (`0 0 12px rgba(224,75,60,.6)`). Use TextMeshPro's material Glow — set Glow Color to the hostile red, Glow Offset 0, Glow Power to taste. Do not use Underlay with a zero offset; it is a shadow, not a glow, and it will look muddy.

**7. Letter-spacing.** CSS uses em; TMP uses its own units where roughly `characterSpacing ≈ em × 100`. So .09em → ~9, .26em → ~26. That mapping is close but not exact across fonts — check the FIRE label and the mount row names visually against the reference screenshot, because those two carry the most tracking and are the most obvious if wrong.

**8. Do not use auto-layout for the vertical stack.** Every block height in section 2 is a known constant. Set explicit RectTransform heights and positions. Two reasons: `margin-top: auto` on the FIRE block has no VerticalLayoutGroup equivalent, and the reference's own flex-shrink behaviour (the scope silently losing 56px) is a bug you do not want to reproduce accidentally. Anchor the header to the top, the FIRE block to the bottom, and place the rest at fixed offsets.

**9. Line boxes.** All the pixel heights here come from CSS `line-height: normal`, which is the browser reading the font's own metrics. TMP will not land on the same numbers by default. Set explicit heights from the tables and let the text centre inside them; do not let TMP auto-size drive layout.

**10. `overflow: hidden`.** Two places need it: the drawer's clip container (so the panel can slide out from behind the edge) and the scope. Use `RectMask2D` on both — it is cheaper than `Mask` and these are plain rectangles.

**11. The slide-in easing.** `cubic-bezier(.2,.85,.25,1)` over 0.42s is not one of Unity's presets. Sample it into an `AnimationCurve` with keys at roughly t=0 (v=0, out-tangent steep), t=0.15 (v≈0.55), t=0.35 (v≈0.94), t=0.42 (v=1, flat). It is a fast-out, long-settle curve. Animate `anchoredPosition.x` from 376 to 0. The 0.3s opacity fade is separate and linear-ish; drive it with a `CanvasGroup`.

**12. The FIRE blink is `steps(1)`, a hard cut.** Do not use an animation curve — the discontinuity will get smoothed. Use a coroutine: alpha 1.0 for 0.432s, alpha 0.2 for 0.468s, repeat. And blink only the frame overlay, not the text.

**13. `pointer-events`.** The clip container ignores input; the panel accepts it. In UGUI, turn off Raycast Target on the clip container's Image (or give it no Image at all) and leave it on for the panel background.

**14. Colour space.** All values here are sRGB hex. If the project renders in Linear, enter them through Unity's colour picker in hex mode and let Unity convert. Do not hand-convert — the alpha values in particular will not survive it.

**15. Missing states.** The reference specifies no hover, pressed, focus or disabled visuals for the mount rows, steppers, payload field or close button. Only `cursor: pointer`. Those need designing. A safe default in this visual language is a small brightness lift on the row background for hover (say `rgba(22,26,30,.7)` → `rgba(30,35,40,.8)`) and nothing on press, since the selected state is already loud — but that is a decision, not something the handoff gives you.

---

## 13. Open questions

Things this spec could not resolve from the source. Listed so they get decided rather than guessed at during the build.

1. **Mount row height.** README says 26px, markup renders 30px. Spec above uses 30px.
2. **Scope height.** README says 226px, the reference renders 170px because the drawer over-subscribes by 56px. Spec above gives both.
3. **The solution readout strip** (range / TOF / BRG) has no README section number. It is specified in section 4 above but it is unowned.
4. **Interlock sub-labels** differ between the reference's JS constant and its markup. Spec uses the markup's short forms.
5. **Hover, pressed, focus and disabled states** for every clickable element. No colours, no numbers, nothing.
6. **How ROUNDS AWAY ends.** No duration, no animation, no return-to-idle. It clears only on the next unrelated interaction.
7. **Feedback on a dead FIRE press.** Nothing happens. No shake, no sound, no flash.
8. **Payload counter is a bug** in the reference — it shows `1 / n` regardless of the current index.
9. **Arc centre, half-arc, loaded and magazine are never displayed** in the drawer, despite being in the README's 5b table. They live in the mount data and surface elsewhere.
10. **Sound and haptics.** Not covered anywhere in the handoff, for a UI built entirely around the feel of throwing physical switches.
11. **The drawer occludes, and is occluded by, the order row's bottom-right corner** at 1920×1080. Whatever Killshot puts there needs a stacking decision.
12. **Scaling below 1920×1080.** The handoff is a fixed design resolution with no responsive rules at all.
