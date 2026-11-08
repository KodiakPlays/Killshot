# The board's UI

## Summary

The tactical board is the game's main screen. It is made of panels and frame pieces.

The panels are the plot, the helm, the orders station, the periscope and six drawers (power,
sensors, comms, fire control, hangar and engineering), and the end screen. The frame pieces are the metal frame and
the smaller pieces on it: two rails of keys, the time control, the console, the alarm strip and
the signal box.

Each panel and each frame piece is a prefab file in `Panels/`. You can open a prefab by itself
and change its art and its fonts. You don't need to touch code.

The art files are in `Art/`. The fonts are in `Assets/UI/Fonts`. The board's colours are in one
file, `Palette.asset`.

## Overview

### Panels and scripts

There is one prefab per panel or frame piece. The top object of each panel has the panel's
script. For example, the script **Power Panel** is on the object `Power`. The script shows the
ship's state on the panel and passes clicks to the game.

The panel scripts are in `Assets/Scripts/UI/Panels/`. The frame pieces have no panel script. The
board script runs them (see [Frame pieces](#frame-pieces)).

### The Art list and the Fonts list

Each panel script has an **Art** list. Each frame piece has the same list, on its **Art List**
script. Each entry is one piece of art: a name, a sprite, and every image that shows it. Change
the sprite in an entry, and all of its images change.

Beside the Art list is a **Fonts** list. It works the same way. Each entry is one font: a name, a
font, a material, and every text that uses them. Change the font in an entry, and all of its
texts change.
- Leave **Material** empty to use the material that comes with the font.
- Share Tech Mono has a second material, "Small". The texts that use it have a separate entry.
- A material made for another font is ignored. The text gets the material that comes with the
  font.

### Colours

The board's colours are in one file, `Palette.asset`, in this folder. It has 22 colours. Each one
has a note on what it is for: green for working, amber for caution, red for alarm, the greys for
text, the unlit lamps and cells, and white for our ship.

Every panel and the board script take their colours from this file. Change a colour in it, and
every part that uses it changes.

Six colours are used by one panel only. They are fields on that panel's script:
- the plot's axis numbers (**Axis Colour**) and its amber sweep (**Sweep Ink Amber**)
- the sensors drawer's band rows and contact rows (**Band Row On**, **Contact Row Lit**)
- the periscope's rail legend (**Rail Legend**)
- the periscope's ship picture (**Ship Ink**, the same red as the sight's lines)

### Folders

Art that only one panel or frame piece uses is in a folder named after it. Art that more than one uses is
in a folder for its kind.

```
Panels/            one prefab per panel or frame piece
Art/Helm/          art only the helm uses; the same for each panel (Art/Power, Art/Fire Control …)
Art/Frame/         the metal frame: the steel texture and the conduits
Art/ToolRail/      the tool rail's icons and its selected-key frame
Art/SystemsRail/   the systems rail's icons
Art/TimeControl/   the time control's plate, pause key and rate keys
Art/Briefing/      the briefing icon
Art/Console/       the console's casting and lamp bezel
Art/AlarmStrip/    the alarm strip
Art/Rails/         the rail frame, for both rails
Art/Buttons/       push buttons and rail keys
Art/Lamps/         indicator lamps, their glow, and the rail lamp bezels and lenses
Art/Bolts/         round and slotted bolts
Art/Knobs/         rotary knobs, their pointers and tick marks
Art/Switches/      rocker switches (up, locked, thrown) and their housings
Art/Latches/       drawer latches, latch tracks and sliders
Art/Screens/       screen glass with its bezel
Art/Plates/        label plates
Art/Wells/         recessed wells for controls
Art/Shapes/        plain rounded shapes the code colours (key faces, cells, the dial ring)
Materials/         the board's two materials
Design/            the design's reference files: its README, asset map, tokens, layout, board logic,
                   state spec (UIStateSpec.md) and grid colours (plot-grid.json)
Palette.asset      the board's colours
```

The fonts are in `Assets/UI/Fonts`, one folder per font: `BarlowCondensed`, `ShareTechMono` and
`NotoSansSymbols2`. Each folder has the font asset, its `.ttf` file and its licence. Keep the `.ttf`
next to its font asset. The font asset adds letters from it as text needs them.
`ElectronicHighwaySign` is an old font. The board does not use it.

### Groups and draw order

Each control's parts are under one object named after the control. For example, the helm's
`Open/01 Flight/Throttle` has the throttle's well, cap and lit track.

A part lower in the Hierarchy draws on top of the parts above it.

### Board pixels

The board is 1920 × 1080 pixels. The helm, the orders station, every drawer and every frame piece
are the size of the whole board. Every group in them is that size too. In these, a part's Rect
Transform gives its place on the board:
- Pos X is the board x of the part's centre.
- Pos Y is the board y of the part's centre, as a minus number.

Two frame pieces are moved. `TimeControl` is moved 72 px right, and `SignalBox` 59 px left. In
those two, Pos X is off by that much.

The periscope and the plot are smaller. Their parts' positions count from the panel's top-left
corner. Their sections below give that corner.

Some script fields are positions in board pixels too. In those, y counts down from the top edge,
without the minus.

### On the board

The board is in the scene `Assets/Scenes/Tactical/02.1_Grid.unity`, under `Board/Tactical Board`.
- The helm is `Tactical Board/Helm`. It has two states. `Closed` is the header strip shown when the
  helm is folded down. `Open` is the whole helm.
- The orders station is `Tactical Board/Orders`. Like the helm, it has `Closed` and `Open`.
- The plot is `Tactical Board/Plot`.
- The periscope is `Tactical Board/Periscope`. It is housed until you lower it. Its LOWERED label,
  `Tactical Board/State · periscope`, is outside the prefab.
- The drawers are under `Tactical Board/Drawers`. A drawer slides out from behind the metal frame.
  It is switched off until it opens.
- The frame pieces are under `Tactical Board`: `Frame`, `ToolRail`, `SystemsRail`, `TimeControl`,
  `Console`, `AlarmStrip` and `SignalBox`.
- `PlotRing`, a dark rounded shape behind the plot, is an image on `Tactical Board`, outside every
  prefab. `ReferenceOverlay` is the design's picture, switched off.

## How to change a panel's art

1. Double-click the prefab to open it, for example `Panels/Power.prefab`.
2. Select the top object (`Power`).
3. On its script (**Power Panel**), open the **Art** list.
4. Find the entry, for example **Bolt**. Drop the new sprite into its **Sprite** field. Every
   image in its **Images** list changes.
5. Save the prefab.

A frame piece works the same way. Its **Art** list is on its **Art List** script.

Put a new art file in the panel's folder. If more than one panel uses it, put it in the folder
for its kind.

Set up each new art file before you use it:
1. Select the file in the Project window.
2. In the Inspector, set **Texture Type** to **Sprite (2D and UI)**.
3. Under **Advanced**, turn **sRGB (Color Texture)** off. Every board sprite has it off.
4. Click **Apply**.

To give one image a different sprite, take the image out of its entry's **Images** list. Then add
a new entry with that image and the new sprite.

An entry marked "(stand-in)" uses a placeholder shape. It is waiting for real art. In the
Hierarchy, stand-in parts have names that start with `STAND-IN`.

## Rules for everyone

- Edit the prefab file. A change made to the copy on the board stays on that copy only.
- Don't move or resize groups. Every group is the size of its panel. Move the parts inside it.
- Keep each part in its place in the list. A part lower in the list draws on top.
- Keep `Block · body` as the first part. It is an invisible image at the top of each drawer, the
  periscope, the helm's `Open`, and the orders station's `Closed` and `Open`. It stops clicks
  going through the panel to the plot.
- Change a listed image's sprite through the Art list only. If you set it on the image by hand,
  the Art list puts the listed sprite back the next time anything on the script changes.
- Don't rename parts or art files. The playtest and the board script find some of them by name.
- Some fields show empty in a prefab.
  - The sensors drawer links to two things outside it: the signal box, and the plot's SENSOR
    RANGE label. A prefab can't link to objects outside it. Those 17 fields are empty in the
    prefab file. They are set on the board's copy in the scene. Leave them empty in the prefab.
  - Some lamp glow fields are empty on purpose. The design gives those lamps no glow: two band
    lamps in comms, four lamps in fire control, and squadron 2's selection lamp in the hangar.
- The code switches some parts on and off in play, such as lamp glows and unused contact rows.
  The prefab shows them as they were when it was saved.

## What you can change

### Safe to change in every panel

- sprites, through the Art list
- the words on fixed labels, such as titles and captions (the code writes over readouts, such as
  numbers and states)
- the position of parts the code does not move (each panel's section below names the ones it
  moves)

### Ask a technical artist first

- Parts the code moves, turns or resizes. Script fields set where they go, mostly in board
  pixels. If you move one of these parts, those fields must change too.
- Knob pointers. The code turns each pointer around its knob. Each dial field on the script has a
  **Centre** (the knob's centre) and a **Home Position** (the pointer's start). If you move a
  knob, these must change too.
- Sprites the code swaps, such as rocker switches. These sprites are fields on the script,
  outside the Art list. The rockers also slide in play. The script for that is **Rockers**, on
  `Tactical Board` in the scene. It uses the rocker sprites too.
- Colours that change in play, such as lamps lighting up. The code takes them from the palette
  (see [Colours](#colours)). A colour you set on the image is replaced in play.
- Links to objects outside the prefab. Only the sensors drawer has these.

## Panel by panel

### Helm

`Panels/Helm.prefab`. Script: **Helm Panel**, on `Helm`. The helm sets the ship's throttle,
heading and autopilot.

The prefab has these parts:
- `Closed`: the folded-down header strip, with heading, turn, throttle, G load and autopilot state
- `Open`: the whole helm
  - `Header`: the same readouts as `Closed`
  - `01 Flight`: `Throttle` and `Heading dial`
  - `02 Attitude hold`: four keys, `NOSE`, `STBD`, `PORT` and `TAIL`
  - `03 Approach`: `Range down`, `Range up` and three mode rows, `DIRECT`, `FLANK` and `EVASIVE`
  - `04 Stability`: the nine stability cells
  - `Plates` and `Bolts`: the helm's body

Its art files are in `Art/Helm` and the folders for each kind. The Art list has 20 entries. Two
are stand-ins: the throttle's lit track and the heading ring.

The throttle slider (01 FLIGHT, THRUST) runs from full reverse to full ahead (Step 157). 0 is in
the middle. Full ahead (+100) is at the top, and full reverse (-100) is at the bottom. The lit bar
runs from the middle to the handle: up when going ahead, down in reverse. In reverse the header
shows a negative number, for example THR -80%.

A and D turn the ship (Step 159). Each press moves the ordered heading 3°. The order stops 179°
ahead of the bow. Hold D and the ship turns to starboard until you let go. Hold A and it turns to
port. When you let go, the ship keeps turning until it reaches the order.

Ask a technical artist before you change these:
- The code moves the throttle's `Cap` and stretches `STAND-IN Lit track`. **Thr Well** is the
  well's place in board pixels (652, 941, 44 × 78). **Cap Base**, **Cap Span** and **Cap Height**
  place the cap in the well. The lit track follows the cap. The tooltip on the cap explains the
  slider. Dragging the throttle also reads **Thr Well**: top +100, middle 0, bottom -100.
- The code turns the heading ring (**Hdg Ring**). The ring slides to each new heading over 0.12 s.
  It turns the short way across north: from 359 to 000 it moves one small step.
- The code lines up the header readouts in `Closed` and `Open`: `Turn`, `THR caption`,
  `THR value`, `THR percent sign` and `G load`, from left to right. The row starts at the left
  edge of `Turn`. **Gap To Thr**, **Gap To Value** and **Gap To G** set the gaps. Moving the
  others sideways has no effect.
- The code sets the colours of the lamps, the attitude keys, the range keys, the mode rows, the
  stability cells, and the turn and G load text.

### Orders

`Panels/Orders.prefab`. Script: **Orders Panel**, on `Orders`. The orders station sets the alert
condition and gives damage control orders. Each order needs AUX charge.

The prefab has these parts:
- `Closed`: the shut station, with its header and the alert selector
- `Open`: the whole station
  - `Header`: the title plate, latch track and title
  - `Alert`: the alert `Selector` (C3, C2, C1), its screen, and the condition and release lines
  - `AUX pool`: ten charge cells, the room the orders act on, the charge and the feed rate
  - `Order switches`: `Housings` and six switches, `PARTY`, `SEAL`, `VENT`, `FIRE`, `BLKHD` and
    `SCRUB`. `FIRE`, `BLKHD` and `SCRUB` are blank plates.
  - `Body plate` and `Bolts`: the station's body
- `Header click area`, in `Closed` and `Open`: clicking it opens and shuts the station

Its art files are in `Art/Orders` and the folders for each kind. The Art list has 17 entries.

Ask a technical artist before you change these:
- The code swaps the sprites of `PARTY`, `SEAL` and `VENT`: **Rocker Up**, **Rocker Locked** and
  **Rocker Thrown**. The blank plates don't change.
- The code turns the alert selector's pointer in `Closed` and in `Open`: **Stowed** →
  **Pointer** and **Open** → **Pointer**.
- The code shows as many AUX cells as the charge fills (**Aux Cells**).
- The code sets the colours of the C3, C2 and C1 lamps and labels, the condition and release
  lines, the charge readout, the order lamps and the note.

### Periscope

`Panels/Periscope.prefab`. Script: **Periscope Panel**, on `Periscope`. The periscope
looks along one bearing. You slew it, zoom it with the MAG knob, and lock onto what is in the
sight.

`Periscope` is a clip window. It shows only the part of the periscope that has come down. Housed,
it is 44 px tall. Lowered, it is 656 px tall. The prefab is saved lowered. You see the whole
periscope when you open it. The copy on the board is housed. The groups stretch with the clip,
and the parts keep their places as it grows.

The periscope is 420 px wide. Its top-left corner is at board x 750, y 0. Pos X of a part is its
board x less 750.

The prefab has these parts:
- `Carriage`: the plate, lamp and title, and `Housed label` (shown only while housed)
- `Housing` and `Bolts`: the periscope's body
- `Sight`: `Aperture`, `Mask` with the sight picture and `Ship picture` (the enemy ship as we
  see it), `Vignette`, the target `Brackets`, four ticks and `Target name`
- `Readout`: the off-axis and zoom readouts
- `Mag knob`
- `Slew readout`: the bearing, the SLEW label and its marker, and two stand-in parts
- `Slew button` and `Classify button`
- `Subject`: the name and class of what is in the sight
- `Convergence`: the convergence bar (`Bar`) and its track (`Track`)
- `Mount plate`, `Mount screen`, `Mount caption` and `Mount`: the mount readout. These four are
  not in a group. Their places in the list set which of them draw over the aperture. Keep them
  there.

Its art files are in `Art/Periscope` and the folders for each kind. The Art list has 15 entries.

Ask a technical artist before you change these:
- The code swaps the aperture's sprite: **Aperture Idle**, and **Aperture Slewing** (the amber rim
  while slewing).
- The board's motion script sets the clip's height: **Scope Housed Px** (44) and
  **Scope Lowered Px** (656), on **Board Motion** on `Tactical Board`.
- The code draws the sight picture every frame, on `Sight/Mask/Sight (drawScope)`. Its colours
  are fields on that part's script.
- The code moves the target brackets sideways and hides them when no target is near:
  **Brackets**, **Bracket Home X** (887.2), **Sight Centre X** (960), **Px Per Deg**,
  **Max Shift** and **Visible Deg**.
- The code shows the enemy ship in `Sight/Mask/Ship picture` when DRADIS hears it and the sight
  is near its bearing. It moves the picture sideways like the brackets, and sizes it by zoom and
  range. A lock builds only while the ship fits the brackets. **Ship Picture**, **Bracket Px** (120,
  the brackets' size) and **Ship Ink** (the colour) are on **Periscope Panel**. The picture is the
  plot's top-down view of the ship, from `World/Contact View Camera · Garuda`. It always lies
  level: the ship points straight right if it heads to our right, else straight left. It stays
  flat during a turn too. The picture turns every frame, from the heading the model is drawn at
  (`LateUpdate` in `PeriscopePanel.cs`, from `NavSystem.DrawnHeading`). `Refresh` picks left or
  right once per update. The picture uses the
  `BoardLinearImage` material, as the plot's ship pictures do, which makes the black around the
  ship see-through. The round sight cuts it off at its edge. How far away a ship
  fills the brackets is **Scope Fit Units**, on the Board tab of **Starwolf → Tuning**, under
  Periscope & fire control.
- The code stretches `Convergence/Bar` from the left: **Conv Bar**, **Conv Left** (777),
  **Conv Width** (366) and **Parent Left** (750, the periscope's left edge).
- The code turns the MAG knob's pointer (**Mag Pointer**).
- The code sets the colours of the off-axis readout, the SLEW label, and the SLEW and CLASS lamps.

The periscope is also the targeting computer. When a ship is in the sight, four white corners show the
target box at the centre. The player zooms until the ship fits it, and the `Lock hint` text under the name says
what to do. The bar in `Convergence` is the lock bar: it fills as the lock builds, then runs down over the 10
seconds the lock lasts, and the hint counts the seconds. `Classify button` is switched
off: CLASS was removed on 2026-11-01. Don't delete anything in it without asking. The red lines in the sight must
not change.

### Plot

`Panels/Plot.prefab`. Script: **Plot Panel**, on `Plot`. The plot is the map in the
middle of the board. It shows our ship, the contacts, the sensor and weapon rings, and the marks
made with the plot tools.

Most of what the plot shows is drawn by code while the game runs. Parts named `… (drawn)` are
drawn this way.

The plot is 1712 × 890 px. Its top-left corner is at board x 104, y 100. Its parts' positions
count from that corner.

The prefab has these parts, from back to front:
- `Ground`, `Grid` and `Glow`: the plot's background, the grid and its glow. `Close-up (3D view)`
  is between `Ground` and `Grid`: the real look of ships and rocks when you zoom in (see below)
- `Scale`: the axis numbers, made by code
- `Design mock-up (off)`: switched off. It has the three parts that were kept from the design's
  sample picture:
  - `Passive label template`: the plot copies it for each BRG label beside a bearing bar
  - `SENSOR RANGE label (off)` and `SENSOR RANGE value (off)`: the sensors drawer links to them

  Leave this group as it is. Don't delete anything in it.
- `Sensor ring (drawn)`, `Weapon ring (drawn)`, `Passive bearings (drawn)` and
  `Sensor sweep (drawn)`
- `Own ship (3D view)` and `Contact ship (3D view)`: pictures of the 3D ship models seen from
  above, made while the game runs
- `Fighters`, `Contact labels` and `Mark labels`: each has one template, switched off. The code
  copies it for each fighter or label.
- `Contacts (drawn)` and `Marks (drawn)`
- `Marks capture` and `Mark hits (ERASE)`: click areas for the plot tools
- `Scanlines`, `Vignette` and `Edge`: the screen effects and the four edges

Its art files are in `Art/Plot`, with `Art/Shapes/round_6` for the ground. The Art list has 6
entries: the ground, glow, fighter triangle, scanlines, vignette and gas cloud. The fighter triangle
is `Assets/UI/Sprite/triangle-128.png`. It is a small white triangle, nose pointing the way the
fighter flies.

A gas cloud shows on the plot in two ways. Within visual range (2,000 units) of our ship or of a
fighter flight, it shows as a soft, ragged grey-blue cloud. It fills the cloud's box. The
pictures are flipped different ways: left to right, top to bottom, or both. The three clouds in Wolf
Hunt look different. Past visual range, while DRADIS is on, a cloud inside the DRADIS circle shows as
a thin hollow circle in the ruins' grey. The circle is about the cloud's size. Its radius is the
average of the cloud's half width and half height. ACTIVE SCAN is not needed. With DRADIS off, a
cloud past visual range does not show.

Our ship is in a cloud when at least 90% of its box is in the cloud's box. The picture fills that
box. The edge you see is close to the real edge.

The picture is `Art/Plot/GasCloud.png`. It is placeholder art: a white cloud with soft edges,
640 x 384. The plot colours it. To change the look, replace the PNG with one of the same name, or
swap the sprite on the Gas cloud entry of the Art list. The picture stretches to the cloud's box. It
has no stretch borders. `Gas cloud (copied for each cloud)` in the prefab is switched off. The code
copies it for each cloud. The circle is drawn on `Contacts (drawn)`.

Three settings are on the Board tab of **Starwolf → Tuning**. **Cloud Alpha** (0.45, under Plot &
our ship) sets how strong the cloud shows. **Cloud Ring Alpha** (0.5, under Sensors) sets how strong
the circle shows. **Cloud Ring Size** (1, under Sensors) sets the circle's size. At 1 the radius is
the cloud's average half size. The circle is never smaller than 6 px.

The jump point placeholder picture is `Assets/UI/Board/Art/Plot/JumpPoint.png`. It is a white ring
with a dot in the middle, 256 x 256. The prefab is `Assets/Prefabs/JumpPoint.prefab`. It is one
sprite, sized so the ring is 700 units from its centre (the distance that wins). It is on the
Default layer. It must stay off the WorldTerrain layer, because anything drawn on that layer in a
level turns into a solid rock. To change the look, replace the PNG with one of the same name. It is
placed in COLD RUN. The Mission's Jump Point box points at it. At play the prefab is switched off, and it is seen only in the Scene view at edit time. The plot still draws its own JUMP POINT ring
(`PlotPanel.ShowJumpPoint`).

When the board appears, the plot starts empty. The grid's lines glide in, big squares first, then
small. Then every layer from `Scale` to `Mark hits (ERASE)` fades in. **Grid Form Seconds** and
**Plot Fade Seconds** set the speed. They are on the Board tab of **Starwolf → Tuning**, under
Plot & our ship.

The squares on the plot change size as you zoom: 50 units at x1, then 100, 200, 500 and 1,000.
Each size fades out as its squares close up, and the next size takes over. A line in both sizes stays
solid. **Step Units** lists the sizes. **Step Full Px** (35) and **Step Gone Px** (25) set when a size
fades. Your ship stays 200 x 50 units: four squares long at x1, two on the 100-unit squares.

A big square is always 5 x 5 of the squares on screen, set by **Big Every**. Its lines are slightly
whiter, and they move up as the squares change size: every 250 units at x1, then 500, 1,000, 2,500 and
5,000. The lines are colours on the `PlotGrid` part of `Plot.prefab`: White Minor, White Major, Amber
Minor and Amber Major.

To make the grid lines brighter or fainter:

1. Stop play mode.
2. In the Project window, double-click `Assets/UI/Board/Panels/Plot.prefab`.
3. In the Hierarchy, click **Grid**, which is under Plot.
4. In the Inspector, find **Plot Grid**, then the **Tint** section.
5. Click the colour bar you want and change **A**. Higher is brighter. The other numbers change the
   colour itself. If the box runs from 0 to 1, divide the number by 255.
6. Press Cmd+S, then click the back arrow at the top of the Hierarchy.

| Colour | What it changes | A |
|---|---|---|
| White Major | big lines on the white grid | 61 |
| White Minor | small lines on the white grid | 20 |
| Amber Major | big lines on the amber grid | 126 |
| Amber Minor | small lines on the amber grid | 54 |

To compare numbers quickly, press Play, click **Grid** under Plot in the Hierarchy, and change the
colour there. The plot updates straight away. That change is lost when you stop, so enter the number
you like in the prefab afterwards. These colours are not in the Tuning window.

The numbers along the top and left edges show the distance from our ship. They use the same sizes
as the squares, with more room: every 100 units at x1, every 1,000 at x0.1. **Axis All Numbers Px**
(60) and **Axis Half Numbers Px** (40) set when one size of number gives way to the next. All of these
are under Plot & our ship. **Zoom Out Limit** sets how far you can zoom out, on the same tab under
Board & time. It is x0.058, where a 20,000-unit battle space just fits the plot's height.

A crash or a hit on us shakes the plot for half a second. **Shake Seconds** and **Shake Px** set it, under Board & time.

A killed ship in sight breaks in two where the shot struck, and the halves drift apart and turn slowly. Out of sight it is a dim dot, and the dot fades away. **Wreck Drift Units**, **Wreck Spin Deg** and **Wreck Dot Fade Seconds** set this, under Plot & our ship. The break is the ship's picture cut in two by `PlotPanel`, so it needs no extra art.

Rocks show on the plot only within visual range (2,000 units) of our ship or of a fighter flight.
They show as grey line art, on `Ruins (3D view)`. Past visual range, DRADIS shows a rock only as a
signal. Plain DRADIS doesn't show a rock. As the DRADIS sweep passes a rock inside the ACTIVE SCAN
cone, a clean, thin, grey hollow circle shows at the rock. It uses the plot's solid ring line. A big
rock gives a bigger circle, never a large one: the radius stays between **Rock Echo Min Px** (3) and
**Rock Echo Max Px** (10). The circle is bright as the ping passes and dims within about a second.
With no new ping it fades out over 30 game seconds. Rocks in the way cut the ping's reach, as they
do for ships. A rock across the edge of visual range is cut at the circle. The part inside shows as
the rock. The part outside shows only as its hollow circle. The code draws the circles on
`Rock Echoes`, a copy of `Contacts (drawn)` made while the game runs, just under the contacts. They
need no art. **Rock Echo Seconds** (30), **Rock Echo Blink Seconds** (1), **Rock Echo Dim** (0.45),
**Rock Echo Size** (1, times the rock's half size), **Rock Echo Min Px** (3), **Rock Echo Max Px**
(10) and **Rock Echo Ink** (the grey) set them. They are on the Board tab of **Starwolf → Tuning**,
under Sensors. `BoardLinearImage` cuts the rocks and the hollow circles at up to 4 circles: our
ship's and up to 3 fighter flights'. Every other picture using that material is unaffected. Rocks throw no false dots (Step 157).

Zoom in close and the ships and rocks inside visual range show as real 3D models. Ships are grey
metal. Rocks are dark rock. One sun lights them. The bright spots have a soft glow. Zoom out and they
fade back into the line pictures. Past visual range nothing changes: DRADIS shows signals as before.

The close-up starts to show at zoom x1. At x1.5 it has fully replaced the line pictures. A level
opens at x1, with the line pictures. **Close Up From Zoom** (1) and **Close Up Full Zoom** (1.5) set these. They are on the Board tab of
**Starwolf → Tuning**, under Plot & our ship. The plot zooms in to x4. At x4 our ship is 608 px long.
**Zoom In Limit** (4) sets this, on the same tab under Board & time. The SCALE knob turns from full
zoom-out to x4. The scale box shows x1.0 up to x4.0.

The close-up shows a ship only when it is in sight. A ship behind a rock is left out. Every ship is
the same grey in the close-up. A ship's white, red or amber colour shows only in the
line pictures. The name labels and the white lock corners still show. A destroyed ship keeps its line
picture, broken in two, at every zoom. The close-up leaves wrecks out. Fighters, shots and torpedoes
are drawn flat on top. Gas clouds are unchanged.

The close-up is the picture `Close-up (3D view)` in `Plot.prefab`. It is just above `Ground` and
under `Grid`. The grid lines, marks, labels, shots, fighters and torpedoes draw on top of it. It is
cut at the visual range circles round our ship and our fighter flights, the same way the rocks are
cut. It fades in with the rest of the plot when the board appears. **Close Up Image** on the Plot
panel points at it. **Close Up Look** points at the glow file.

The sun is the light called `Sun` in the board scene `02.1_Grid`. It lights ships and rocks.
**Sun From Deg** (300) is the way the sunlight comes from, in degrees clockwise from north.
**Sun Height Deg** (35) is how high it is above the plot. At 90 it shines straight down. Both are on
the Board tab, under Plot & our ship. To change the sun's colour or brightness, click `Sun` in the
scene and change them on the light. It is the scene's Sun Source, in the Lighting window.

The look files are in `Assets/Art/Close Up`:
- `Ship Metal.mat`: the grey metal for every ship.
- `Rock.mat`: every rock and wreck piece of the level.
- `Close Up Look.asset`: the glow. Open it to change the glow's strength.

Ask a technical artist before you change the renderer. It is `Assets/Settings/Close Up Renderer.asset`,
a copy of the game's renderer without the line look. It paints ships with Ship Metal and rocks with
Rock. It is number 1 in the renderer list on `Assets/Settings/PC_RPAsset.asset`. **Close Up Renderer**
(1) on the Plot panel points at it. Alpha Processing is on in PC_RPAsset. It lets the glow keep the
see-through background.

The ship models are their real length: our Asura 200 units, the Garuda 350, the frigate 100. The two
cameras that film the line pictures, Ship View Camera and Contact View Camera · Garuda, are moved back
to match. Each films only its own ship. **Own Ship Max Px** (1200) lets a ship's drawn size follow its
model up to x4.

While Fire Control is open, each railgun shows a soft dotted cone and a solid bar on
`Weapon ring (drawn)`. The bar is the gun. **Rail Arc Alpha** and **Gun Bar Alpha** set how strong
they are. Where the guns are on the ship is set on the Hard Points tab of **Starwolf → Tuning**.

A locked ship has white corners round it on the plot, the same as the targeting computer's box. A contact out of
sight is a marker where it was last seen. It fades as it gets old, and its label says how old it is. **Stale
Seconds**, **Stale Alpha** and **Stale Label Seconds** are on the Board tab of **Starwolf → Tuning**, under Sensors.
The RULER has a thicker line and white distance text: **Ruler Width** and **Ruler Dot** are under Plot tools.

A ship DRADIS hears but we haven't scanned shows as a short red bar, curved along the DRADIS circle, in its
direction. Its label, BRG and the angle (for example BRG 053), is just outside the bar. The bar first shows when
the sweep first touches the ship. After that it turns on every frame, following the ship's bearing as either
ship moves. The BRG number changes with it. When the DRADIS circle is off the plot, so is the bar. Nothing is
left behind as the sweep passes.

The bar is the board's red. It is dimmer (0.6) most of the time, and full while the periscope is on that ship.
The BRG label is amber while the periscope is on that ship, and dimmer otherwise.

The bar is a picture: `Art/Plot/DradisContactBar.png`. It is placeholder art: a white bar with rounded ends,
256 x 32. The game bends it along the circle, end to end, and colours it red. To change the look, draw a new one
in white and replace the PNG with one of the same name. Or put a new picture in the **Passive Bar Art** slot on
the Plot panel, under Passive bearings. That slot is in `Plot.prefab`. It is not on the Art list.

The code draws the bars on `Contact bars (drawn)`, a copy of `Passive bearings (drawn)` made while the game
runs. **Passive Bar Deg** (6) sets how many degrees of the circle the bar covers. Zoom in and the bar grows. Zoom
out and it shrinks. **Passive Bar Width Px** (4) sets how thick it is. Both are on the Board tab of
**Starwolf → Tuning**, under Sensors. The label is 6 px outside the bar. That gap is set in the code.

Ships and shots move on the plot in two ways. Some move smoothly, on every frame. Others hop: they stop, then
jump to a new spot each time the DRADIS sweep passes them.

- Shots, torpedoes and fighters always move smoothly.
- Anything inside visual range moves smoothly. So does a locked ship.
- A ship on DRADIS that has not been scanned is a red bar on the DRADIS circle. The bar turns smoothly, on every frame.
- A scanned ship moves smoothly while the ACTIVE SCAN cone covers it. When ACTIVE SCAN goes off or the cone
  moves away, the ship stops there and hops once per sweep.
- A scanned ship that no sensor reaches stops at its last spot and fades.

The game's rules still step every 0.42 s. Only the drawing is smooth. Hits and damage are the same. Just after
firing, a shot's dot can go up to twice its true speed. It can slow in the last step before a hit, and it
reaches the ship's hit box as the hit lands. `BoardManager.Slide` moves the drawn spot, `PlotPanel.ShotAt` draws
shots, and `WeaponSystem.StepAhead` stops a drawn shot at the hit box.

Ask a technical artist before you change these:
- The code copies four templates in play: `Passive label template`, `Fighter (template)`,
  `Contact label (template)` and `Mark label (template)`. Keep them switched off.
- Two fields place everything the code draws: **Plot Centre** (960, 545, in board pixels) and
  **Px Per 1000 Units** (760, on `PlotGrid`). Moving or resizing the plot means changing them.
- The code sizes our ship's picture: **Own Ship Px At Full Zoom**, **Own Ship Min Px** and
  **Own Ship Max Px**.
- The code sizes and spaces the fighters: **Fighter Px** (10) and **Fighter Gap Px** (10).
  Fighter Px is the triangle's size on screen. Fighter Gap Px is the gap between fighters in the V.
- The code makes the axis numbers under `Scale`: **Axis Font**, **Axis Material**,
  **Axis Font Size** and **Axis Colour**.
- The code places labels with **Point Label Offset**, **Ruler Label Offset**,
  **Ring Label Offset** and **Contact Label Offset**.
- The code sets the colours of our ship, wrecks, contacts, rings, the sweep and marks. They come
  from the palette. The amber sweep's colour is on the script (**Sweep Ink Amber**).

### Power

`Panels/Power.prefab`. Script: **Power Panel**, on `Power`. The power drawer shows the reactor
and its four power buses.

The prefab has these parts:
- `Frame`: the casting, bolts and hinge
- `Header`: the strip with the latch, title and load readout
- `Reactor`: the ten unit cells, the five core lamps and their captions
- `Load shed`: the emergency load shed rocker, its lamp and its note
- `Buses`: one group per bus, `ENG`, `WEP`, `SEN` and `AUX`, each with `Cells` and `Selector`

Its art files are in `Art/Power` and the folders for each kind. The Art list has 20 entries.

Ask a technical artist before you change these:
- The code swaps the load shed rocker's sprite: **Rocker Up** and **Rocker Thrown**.
- The code turns each bus selector's pointer: **Columns** → **Selector**.
- The code sets the colours of the load readout, unit cells, core lamps, the shed lamp and note,
  and each bus's cells, lamp and mode text.

### Sensors

`Panels/Sensors.prefab`. Script: **Sensors Panel**, on `Sensors`. The sensors drawer controls
DRADIS and the active scan.

The prefab has these parts:
- `Frame`, and `Header` with the latch, title and range readout
- `Masters`: the `DRADIS` and `ACTIVE SCAN` master rockers
- `E-SIG`: our ship's signal, with ten cells, a lamp, a percent and a note
- `01 Sensor array`: the `Gain` fader, and `Bands` with `THERM`, `RAD`, `UV` and `EM`
- `02 Active scan`: `Cone dial`, `Narrow`, `Widen`, and the bearing and arc readouts
- `03 Contact board`: `Row 1` to `Row 3`

Its art files are in `Art/Sensors` and the folders for each kind. The Art list has 19 entries.
Two are stand-ins: the gain's lit track and the cone dial's face.

Ask a technical artist before you change these:
- The code swaps the master rockers' sprites: **Rocker Up** and **Rocker Thrown**.
- The code moves the gain fader's cap (**Gain Cap**) and stretches its lit track
  (**Gain Track**). The numbers are in board pixels: **Well Top** (375.5), **Well Height** (136),
  **Track Bottom** (502.5), **Cap Top At Five**, **Step Px** and **Cap Half**. Dragging the gain
  also reads **Well Top**, **Well Height** and **Well Stop**.
- The code turns the cone dial's needle (**Needle**) and redraws the cone (**Cone**). Dragging
  the cone reads **Dial Centre** (1505, 609).
- The drawer links to objects outside the prefab. The signal box: **Box Pct**, **Box Note**,
  **Box Cells**, **Box Lamp** and **Box Glow**. The plot's SENSOR RANGE label:
  **Envelope Label** and **Envelope Range**. The code also moves that label up and down with the
  gain (**Envelope Label Home Gain**, **Envelope Label Px Per Gain**).
- The code sets the colours of the lamps, signal cells, band rows, contact rows and most
  readouts.

### Comms

`Panels/Comms.prefab`. Script: **Comms Panel**, on `Comms`. The comms drawer tunes the radio and
decrypts messages.

The prefab has these parts:
- `Frame`, and `Header` with the latch, title and state
- `01 Receiver`:
  - `Band selector`
  - `Scope`: the round receiver scope, with its rim, glass, tuning knob and 56 bars
  - `Tuned`: the frequency
  - `Lock`: the LOCK rocker and its lamps
- `02 Decrypt`: `Message` (59 letters), `Key dial 1` to `Key dial 4`, and the solved readout

The radio is the same in every level. It starts on NO TRAFFIC. A level's Enemy Spawn sets how often
its ship sends a message (**Signal Seconds**, 0 for none) and how long you have to lock it
(**On Air Seconds**). A soft tone plays when one comes in. A missed message is lost. The bearing in a
message is always a word such as NORTHWEST.

The code moves the letters. Each message goes on a grid of 21 letters across, with whole words to a
line. **Letter Start**, **Letter Step** and **Letter Columns** on the Comms Panel set the grid.

Its art files are in `Art/Comms` and the folders for each kind. The Art list has 20 entries.
Three are stand-ins: the band selector's face and knob, and the scope's rim.

Ask a technical artist before you change these:
- The code swaps the LOCK rocker's sprite: **Rocker Up**, **Rocker Locked** and
  **Rocker Thrown**.
- The code turns **Band Pointer**, **Tune Indicator**, **Ring Marker**, and each key dial's
  **Pointer**.
- The code places, sizes and turns all 56 scope bars around **Scope Centre** (1590, 422.5).
  **Rim Radius**, **Bar Width**, **Bar Base** and **Bar Span** set their size. Turning the
  tuning knob also reads **Scope Centre**. Keep the bars in their order inside `Scope`. Some of
  them draw over the tuning knob.
- The code writes each message letter and sets its colour.
- The code sets the colours of the band lamps, the LOCK and SIG lamps, the scope bars, the
  letters, and the key dials' letters and boxes.

### Fire Control

`Panels/Fire Control.prefab`. Script: **Fire Control Panel**, on `Fire Control`. The fire
control drawer aims and fires the ship's weapons.

The prefab has these parts:
- `Frame`, and `Header` with the latch, title, mount readout and `Sight` (the SIGHT key)
- `Scope bearing`: the periscope's bearing and offset
- `01 Weapon system`: `System selector`, `Mount selector` and the mount screen. The `RAILGUN`,
  `PDC` and `TORPEDO` labels are outside the selector's group. They draw over the mount knob.
- `02 Firing solution`: `Track button`, `Range knob`, `Bearing knob`, `Speed knob` and the
  tracking ID
- `03 Capacitor`: `Cells`, a lamp and the charge readouts
- `04 Arming sequence`: `Housings`, and the `LOAD`, `ANGLE`, `BEARING` and `ARM` rockers
- `Fire lever`: `Slot`, `Lit track` and `Cap`

PDCs and torpedoes work through the same system selector, mount selector and arming rockers
(Steps 53 and 55). With TORPEDO selected, the mount selector picks the warhead, and its lamps show
which launcher will fire. The launcher picks itself. The design's own
PDC controls (quadrants, LOADERS) and torpedo controls (warhead, SALVO) are not drawn yet. The
loaders and the salvo start from each ship's stats file.

When a salvo is fired, this is what shows on the plot:

1. Two torpedoes leave the launcher out of the ship's side, tilted a little toward the stern. They
   fan out, one either side of that line.
2. They fly straight and slow down.
3. They curve onto the target. The turn starts gently, gets tighter, then eases off as they line
   up.
4. Their motors light and they zip to full speed.

Enemy torpedoes do the same. The curve is the same at every game speed.

A line can be drawn out of each launcher (see Drawing the line, below). With a line, the line
replaces steps 1 and 2. The torpedoes fly along the line and slow down as before. At the end of the line they
curve onto the target. Then their motors light. Each torpedo in a salvo flies its own copy of the
line, turned by the salvo's fan (**Torpedo Spread Deg**). A launcher on the centre line, as on the
Garuda, sends every other torpedo along the mirror of the line. Enemy torpedoes use the lines in
their own ship's file. With no line, they leave as in step 1.

To change this, use the Ship tab of **Starwolf → Tuning**, under Weapons. Each ship has its own
numbers:
- **Torpedo Launch Speed Units** (80): how fast it leaves the launcher.
- **Torpedo Tilt Back Deg** (15): how far it tilts toward the stern as it leaves.
- **Torpedo Spread Deg** (20): how far apart the two in a salvo fan.
- **Torpedo Coast Seconds** (2.5): how long it flies straight out, slowing, in game seconds.
- **Torpedo Coast Speed Units** (15): how slow it gets. It stays at this speed as it curves.
- **Torpedo Curve Deg Per S** (32): the sharpest it curves before its motor lights, in degrees a
  game second.
- **Torpedo Turn Build Seconds** (0.5): how long a turn takes to build up and to ease off, in game
  seconds.
- **Torpedo Burn Units** (300): the speed its motor adds each game second.
- **Torpedo Speed Units** (250): its full speed.
- **Torpedo Turn Deg Per S** (120): the sharpest it turns with its motor on, in degrees a game
  second.

**Drawing the line.** The line torpedoes and fighters follow out of the ship is drawn on the Hard
Points tab of **Starwolf → Tuning**. Pick the ship at the top. The picture shows the ship from
above, with the nose at the top. A line can be drawn out of a torpedo launcher dot or a hangar dot.
- The green dots labelled HANGAR are where fighters and assault boats leave the ship. There is one
  on each long side. On our ship they are 50 units back from the middle, at (−25, −50) and
  (25, −50). Drag one to move it. It stays on a long side and snaps to the marks.
- **Add a hangar dot** adds one in the middle of the long side that has fewer. **Remove this dot**
  removes the selected dot, of either kind.
- To draw a line, hold Shift and drag out of the dot. When you let go, the line is spaced into 32
  even points and smoothed. A new line replaces the old one.
- **Line smoothing** (0 to 12, default 4) sets how much a line is smoothed when you let go. At 0 it
  stays as drawn. It changes the next line you draw.
- The line shows in the dot's colour, with a small dot at its end.
- Select a launcher or a hangar dot. **Line out of it** shows the line's length in units. With no
  line it says "None: they fly straight out." **Clear the line** removes the line.
- Each change is saved to the ship's stats file when you let go of the mouse. Cmd+Z undoes it.

The line's points are in the ship's stats file. Each hard point and hangar dot has a list, `path`,
in units from the dot (x right, y forward). The hangar dots are in the file's Hangar section, under
`dots`.

In the game, the line is placed in space at the moment of launch, turned to the ship's heading.
Then it stays put. If the ship moves on, the line does not move with it.

Its art files are in `Art/Fire Control` and the folders for each kind. The Art list has 19
entries.

Ask a technical artist before you change these:
- The code swaps the arming rockers' sprites: **Rocker Up**, **Rocker Locked** and
  **Rocker Thrown**.
- The code turns **System Selector**, **Mount Selector**, **Rng Knob**, **Brg Knob** and
  **Spd Knob**.
- The code moves the TRACK button (**Track Key**) and its lamp down by **Track Press Px** while
  it is pressed.
- The code moves the shot-cost mark (**Cap Mark**) to the edge of a cell in **Cap Cells**.
- The code places the fire lever's `Cap` and stretches its `Lit track`, measured from `Slot`:
  **Lever Slot**, **Lever Cap**, **Lever Fill**, **Cap Left Pct**, **Cap Travel Pct**,
  **Lever Travel Px**, **Slot Margin** and **Cap Margin**.
- The code sets the colours of the lamps, selector labels, readouts, capacitor cells and the
  rockers' hints.

### Hangar

`Panels/Hangar.prefab`. Script: **Hangar Panel**, on `Hangar`. The hangar drawer launches and
recalls squadrons.

The prefab has these parts:
- `Frame`, and `Header` with the latch, title and ready readout
- `01 Bay status`: `Doors selector`, `Doors`, `Pressure`, `Catapult` and `Cat charge`
- `02 Squadrons`: `Squadron 1`, `Squadron 2` and the selection bezel
- `03 Launch order`: `Ship selector`, `Bearing` (the small number box) and `Command selector`
- `04 Launch sequence`: `Housings`, the six stage rockers (`CREW`, `FUEL`, `ORDNANCE`, `EVAC`,
  `DOORS`, `CATAPULT`) and `Telemetry`
- `Launch lever`
- `Recall`: the RECALL button

Its art files are in `Art/Hangar` and the folders for each kind. The Art list has 19 entries.
Two are stand-ins: the selection bezel and the knob cap.

Every fighter and assault boat starts the game at 100% and READY. The panel reads 6 OF 6 READY and
4 OF 4 READY. This is the same on our ship and on every enemy ship.

A launch sends all 6 interceptors or all 4 assault boats. Each fighter does its own attack run.
With all 6 out, one attack run takes 60 armour off a side. The 4 boats share the same 18 marines.

Each craft has one bar on the panel. The bar shows one number. The code calls it fuel, and the
owner calls it health. Craft will gain damage from PDCs and ships firing. This is still to be
built. Nothing damages a fighter yet, and nothing in the game changes these bars yet.

When the launch lever is pulled, this is what shows on the plot:

1. The squadron appears just beside our ship, on the side facing fire control's target. With no
   target, it appears on the starboard side. It starts past the widest part of the ship you see,
   the wings. Zoomed out, the plot draws our ship bigger than its true size. The squadron still
   appears beside the ship you see. All its craft start on one spot. None is drawn over the ship.
2. It flies straight out, tilted a little toward the stern. The V opens as it goes.
3. It curves round onto where it is going. The turn starts gently, gets tighter, then eases off as
   it lines up.

Fighters and assault boats both do this. They turn the same way on all their flying, on the way
back too. The V closes as they land.

A squadron leaves from a hangar dot (the green dots on the Hard Points tab). It uses the dot on
the side facing the target. With no target, it uses the starboard dot. With only one dot, it uses
that dot. It starts level with the dot along the ship. On our ship that is 50 units back from the
middle. If a line is drawn out of the dot (see Drawing the line, under Fire Control), the line
replaces step 2. The squadron flies the shape of the line. Then it turns onto where it is going, as
in step 3. An ESCORT
flies on straight past the end of the line until its whole V is clear of the small circle round
our ship. With no line, it flies straight out as in step 2.

To change this, use the Ship tab of **Starwolf → Tuning**, under Hangar. Each ship has its own
numbers:
- **Launch Side Gap Units** (10): the gap between the side of our ship and where it appears. The
  side is the widest part of the ship as the plot draws it. Half a fighter is added to the gap.
- **Launch Tilt Back Deg** (15): how far it tilts toward the stern as it leaves.
- **Launch Run Units** (200): how far it flies straight out before it turns, at every zoom. Only
  an ESCORT can fly further: if the small circle round our ship is bigger at that zoom, the ESCORT
  flies out past that circle.
- **Turn Deg Per S** (90): the sharpest it can turn, in degrees a game second.
- **Turn Ease** (180): how fast a turn builds up and eases off. Lower is gentler.

The small number box on the Hangar panel can't stop a launch or change where they go. The box and
its red number are still on the panel and do nothing else.

Ask a technical artist before you change these:
- The code swaps the stage rockers' sprites: **Rocker Up**, **Rocker Locked** and
  **Rocker Thrown**.
- The code swaps the RECALL button's sprite between **Recall Up** and **Recall Pressed**, and
  fades **Recall Overlay** between them.
- The code grows the header screen with its readout: **Header Screen**, **Header Left**
  (1522.5), **Header Min Width** (106.5), **Header Pad** and **Header Margin**.
- The code moves the selection bezel between the squadron rows: **Bezel** and **Squad Pitch**
  (87). Keep it above both squadron groups in the list. It draws under both rows.
- The code turns **Door Selector**, **Ship Selector** and **Command Selector**.
- The code sizes each bar inside its well: the catapult charge (**Cat Fill**, **Cat Well**), the
  six stage bars (**Stages** → **Fill**, **Well**) and each craft's fuel (**Squad Rows** →
  **Fuel**, **Cells**).
- The small number box's two events call **Hangar Panel**. Keep those links.
- The code places the launch lever the same way as the fire lever: **Lever Slot**,
  **Lever Cap**, **Lever Fill**, **Cap Left Pct**, **Cap Travel Pct**, **Lever Travel Px**,
  **Slot Margin** and **Cap Margin**.
- The code sets the colours of the lamps, labels, craft tags, bars and readouts.

### Engineering

`Panels/Engineering.prefab`. Script: **Engineering Panel**, on `Engineering`. The engineering
drawer shows damage to the ship and orders repairs.

The prefab has these parts:
- `Frame`, and `Header` with the latch and title
- `Compartments`: the screen, `Ship picture` (our ship seen from above), and one group per room:
  `MAG`, `BRI`, `HAB`, `HGR`, `RCT` and `ENG`. `Old row highlight (off)` is switched off. Leave
  it off.
- `Status`: the hazard, degraded and party lines
- `Acting on`: the `Selector` knob, a lamp and the room name
- `Repairs`: `Housings`, and six rockers: `REBOOT`, `REPAIR`, `SEAL`, `VENT`, `RECALL` and
  `SPARE`

Its art files are in `Art/Engineering` and the folders for each kind. The Art list has 17
entries. One is a stand-in: the ring face.

The four pictures of our ship are outside the Art list. They are textures in
`Assets/Art/Board/Hull`: `hull_prow`, `hull_stbd`, `hull_aft` and `hull_port`. Each one is set on
a Raw Image under `Ship picture/Hull`.

Ask a technical artist before you change these:
- The code swaps the repair rockers' sprites. Each **Repairs** entry has an **Up**, a **Locked**
  and a **Thrown** sprite.
- The code sizes the four ship pictures (**Hull Sides**, **Diagram Ship Px**,
  **Picture Fill**). It colours each one by its side's armour: white, amber or red.
- The code draws each room's five ring segments: **Rows** → **Ring**, with **Segment Deg**,
  **Gap Deg** and **From Deg**.
- The code sizes the degraded line and places it in the status row: **Row Left** (1474),
  **Row Right** (1782), **Row Gap** and **Degraded Max**.
- The code turns the ACTING ON pointer (**Acting On**).
- The code sets the colours of the room rows and rings, the status lines, the lamps and the
  rocker labels.

### End Screen

`Panels/End Screen.prefab`. Script: **End Screen Panel**, on `End Screen`. It covers the board
when the mission is won or lost. It says MISSION COMPLETE or SHIP LOST, and why. It shows the
target, our ship and the time taken. Its two buttons are RESTART MISSION and MISSION SELECT.
RESTART MISSION plays the level again. MISSION SELECT opens the MISSIONS list.

It is the last part under `Tactical Board`, so it draws over everything else. Keep it last.

The prefab has these parts:
- `Screen`: switched off while the mission runs. The script switches it on. Leave it off.
- `Shade`: a dark layer over the whole board. It takes the clicks, so nothing under it can be
  pressed.
- `Plate`: the box in the middle, with `Title`, `Reason`, `Rows` (`Labels` and `Values`) and
  the four buttons. RESUME and QUIT GAME are copies of RESTART MISSION, at y −70, x −160 and
  +160. Their labels are on the panel's Barlow Condensed SemiBold font slot.

It has no art yet. The shade, the plate and the buttons are plain colours with an outline. The
Art list is empty. To give it art, add the sprites to the Art list.

Ask a technical artist before you change these:
- The script writes the title, the reason and the values. It colours the title green or red.
  On the pause menu it writes PAUSED in white and hides `Rows`.
- Each button has a **Click Spot** with its action (`end.restart`, `end.menu`, `end.pause`,
  `end.quit`). MISSION SELECT uses `end.menu`. RESUME uses `end.pause`, the same action Escape
  sends. QUIT GAME uses `end.quit`.
- **Resume Button**, **Quit Button** and **Rows** are the parts the script shows and hides.
  RESUME and QUIT GAME show only on the pause menu. `Rows` is hidden on it.
- **Menu Scene** (`01_Briefing`) is the scene MISSION SELECT loads.
- **Delay Seconds** (2) is the wait between the end and the screen, so the last hit is seen
  first. It is on the Board tab of **Starwolf → Tuning**, under Board & time.

## Frame pieces

The frame pieces are the metal frame and the smaller pieces on it. Each one is a prefab in
`Panels/`, under `Tactical Board` on the board. Each one has the **Art List** script on its top
object, with its Art list and its Fonts list. It has no panel script.

The board script, **Board Manager** on `Tactical Board`, runs the frame pieces. Its
fields for them are on `Tactical Board` in the scene, outside the prefabs. Its colours for them
come from the palette.

The prefabs:
- `Frame`: the metal frame. Its parts are `Steel frame`, `Black backing`, `Conduits`,
  `Corner bolts` and `Stencils` (two painted labels, BUS A-14 and VENT PLENUM 3). The art files
  are in `Art/Frame`. The Art list has 5 entries.
It is the pause menu too. In a level, Escape stops the game and opens it. The title says PAUSED in
white. The reason line is empty. The target, our ship and the time are hidden. Two more buttons
show on the top row: RESUME and QUIT GAME. RESTART MISSION and MISSION SELECT are below them.
Escape again, or RESUME, closes it. Time goes back to how it was: running, or still paused if the
player had pressed P before. While it is open, P and the drawer keys 1 to 6 do nothing. QUIT GAME
quits the game. In the Unity editor it stops play mode. On the MISSION COMPLETE or SHIP LOST
screen, Escape does nothing, and RESUME and QUIT GAME are hidden.

- `ToolRail`: the keys for the plot tools, SEL (cursor), PNT (point), RUL (ruler), CMP (compass)
  and ERS (erase), with their
  lamps, plates, icons and labels. The art files are in `Art/ToolRail` and `Art/Rails`. The Art
  list has 13 entries.
- `SystemsRail`: one key per drawer, FIRE, SENS, PWR, HGR, ENGR and COMM. The art files are in
  `Art/SystemsRail` and `Art/Rails`. The Art list has 13 entries.
- `TimeControl`: `Plate`, `Clock`, `Pause`, `Rate keys` and `Rate caption`. The art files are in
  `Art/TimeControl`. The Art list has 4 entries.
- `Briefing Icon`: the small page next to the time control. It opens and closes `Help Bubble`, the
  briefing box. Both are in the scene `02.1_Grid`, under `Tactical Board`; they are not prefabs. The
  picture is `Art/Briefing/icon_briefing.png`. The box is 370 px tall. In FLIGHT DRILL it is 230 px, on
  the same bottom edge: Drill Note Height, on the Board tab of Starwolf → Tuning, under Mission.
- `Console`: `Casting`, two bolts, `Power` (a power lamp), `Scale` (the plot's SCALE knob and
  zoom readout) and `Tint` (the grid tint knob, with WHT and R-O lamps). The art files are in
  `Art/Console`. The Art list has 11 entries.
- `AlarmStrip`: `Strip`, a lamp, `Words` and `Count`. The strip's art file is in
  `Art/AlarmStrip`. The Art list has 3 entries.
- **Menu Key** (Escape) opens and closes the pause menu. It is on the Board Manager, on the Keys
  tab of **Starwolf → Tuning**.
- `SignalBox`: our ship's signal, shown with the sensors drawer shut. Its parts are
  `Signal · well`, `Signal · label`, `Cells` (ten cells), a lamp, `Signal · percent` and
  `Signal · note`. Its art files are in the folders for each kind. The Art list has 4 entries.

The rails have no groups. Each key overlaps the next by 3 px. Its plate and lamp come later in
the list. Keep the parts in their order.

Ask a technical artist before you change these:
- The pause key and the rate keys are not in the Art list. The board script swaps their sprites:
  **Pause Key Run**, **Pause Key Paused**, **Rate Key On** and **Rate Key Off**. The board shows
  three rate keys. `x10 key` and `x10 label` are switched off in the prefab (owner 2026-11-01).
- The board script moves the tool rail's `Selected frame` and `Selected bar` to the chosen tool
  key (**Selected Frame**, **Selected Bar**, **Tool Keys**).
- The board script turns the console's two pointers: **Scale Dial** and **Tint Dial**.
- The board script shows and hides the alarm strip (**Alarm Parts**). It writes its words and
  count. The words start at the left, just after the count, and long lines run on to the right.
- The sensors drawer fills the signal box.
- The playtest finds `AlarmStrip/Words`, `SignalBox/Signal · percent` and
  `SignalBox/Signal · note` by name. The board script and the playtest find some frame pieces by name too. Don't rename
  them.
- The board script sets the colours of the rail lamps, the pause label and lamp, the rate key
  labels, and the tint lamps and labels. It takes them from the palette.
