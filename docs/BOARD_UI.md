# The board's UI

## Summary

The tactical board is the game's main screen. It is made of panels and frame pieces.

The panels are the plot, the helm, the orders station, the periscope and six drawers (power,
sensors, comms, fire control, hangar and engineering), and the end screen. The frame pieces are the metal frame and
the smaller pieces on it: two rails of keys, the time control, the console, the alarm strip and
the signal box.

Each panel and each frame piece is a prefab file in `Panels/`. You can open a prefab by itself
and change its art and its fonts. You don't need to touch code. To see and change every picture
in one window, open **Starwolf → Art** (see [How to change a panel's art](#how-to-change-a-panels-art)).

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
Art/Briefing/      the briefing icon, and the sector map's console picture and tick picture
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

On a 16:9 screen the board is 1920 × 1080 pixels. On other screens it is bigger. See
[Screens of other shapes](#screens-of-other-shapes).

The helm, the orders station, every drawer and every frame piece are drawn on a 1920 × 1080
sheet. Every group in them is that size too. In these, a part's Rect Transform gives its place on
the sheet. On a 16:9 screen the sheet is the whole board:
- Pos X is the board x of the part's centre.
- Pos Y is the board y of the part's centre, as a minus number.

Two frame pieces are moved. `TimeControl` is moved 72 px right, and `SignalBox` 59 px left. In
those two, Pos X is off by that much.

The periscope and the plot are smaller. Their parts' positions count from the panel's top-left
corner. Their sections below give that corner.

Some script fields are positions in board pixels too. They count on the panel's own sheet. In
those, y counts down from the top edge, without the minus.

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

### Screens of other shapes

The battle board fills any screen: a Mac, a Windows PC, a 16:9 monitor, a 16:10 laptop or a wide
21:9 monitor. There are no black bands. This is new on 2026-11-10. The owner played a Mac build on
the MacBook and saw black space above and below the board.

What the player sees:
- On a 16:9 screen the board looks the same as before.
- On a 16:10 screen, such as 2880 × 1800, the plot is taller. It is 1712 × 1010 board pixels. At
  16:9 it is 1712 × 890.
- On a wide 21:9 screen, such as 3440 × 1440, the plot is wider. It is 2372 × 890 board pixels.
- Panels, buttons and text keep their size. The plot and the frame round it grow. More of the map
  shows. On a 21:9 screen a second bogey showed. At 16:9 it was off the edge.

How it works:
- The board's canvas is `Board` in `02.1_Grid`. Its settings are the same as before: Scale With
  Screen Size, 1920 × 1080, Screen Match Mode Expand. On a screen that isn't 16:9, Expand makes
  the canvas bigger than 1920 × 1080 one way: taller on 16:10, wider on 21:9.
- `Tactical Board` stretches to fill the canvas.
- Each panel is a 1920 × 1080 sheet. The panel is drawn on one part of it. Each sheet is tied
  (anchored) to the edge of the board where its panel is. When the board grows, each sheet moves
  with its edge. Nothing inside a sheet moves.

| Edge | Sheets tied to it |
|---|---|
| Top left | `TimeControl`, `Briefing Icon`, `Help Bubble` |
| Top middle | `AlarmStrip`, `Periscope`, `State · periscope` |
| Top right | `SignalBox` |
| Left middle | `ToolRail` |
| Right middle | `SystemsRail`, and the six drawers under `Drawers` |
| Bottom left | `Orders` |
| Bottom middle | `Helm` |
| Bottom right | `Console` |

These grow with the board:
- The plot. It keeps the same gaps to the board's edges: 104 px left, 100 top, 104 right and 90
  bottom. Its layers that were 1712 × 890 stretch with it. Its four edge lines are tied to their own
  sides. The code sizes the sensor sweep and the 3D views every frame.
- `PlotRing`, the dark shape behind the plot.
- `Drawers`. It is a clip box. It hides a drawer until the drawer slides out past the frame. Its
  padding (RectMask2D: 98 left, 84 bottom, 98 right, 94 top) matches the plot opening at any size.
- The frame and its `Black backing`. The frame is `Frame` (see [Frame pieces](#frame-pieces)). The
  plot is drawn over it.

The frame grows without pulling anything painted longer (owner 2026-11-10: "repeat, don't
stretch"). Corners and fixed parts keep their size. Short plain pieces of the edges repeat along
each side. At 16:9 every part is exactly where it was.
- The metal sheet, `Steel frame`, shows `Art/Frame/steel_frame_1920x1080.png`. At 16:9 it fits
  exactly once. On a bigger screen its four big corner pieces stay as drawn. Across, a 200 px piece
  from the middle repeats. Up and down, a strip only 4 px tall repeats (rows 543 to 547 of the
  picture). The metal's grain runs up and down. The strip continues with no step in shade. Its
  sprite border is 860 left, 533 bottom, 860 right and 543 top. Its Image Type is Tiled.
- The four corner bolts, under `Corner bolts`: each one is tied to its own corner.
- The left pipe and its two bolts, under `Conduits`: tied to the left edge, middle height, beside
  the tool rail.
- The right pipe and its two bolts: tied to the right edge, middle height, beside the systems rail.
- The middle pipe runs along the bottom, between ORDERS and the console. It is tied to the bottom.
  It is 1398 px long at 16:9 and 2058 px at 21:9. Its two ends stay as drawn. Its middle repeats,
  two clamp spaces at a time. The clamps keep the same spacing at any length. Its picture is
  `conduit_mid.png`: sprite border 16 left and 1324 right, Image Type Tiled.
- The stencil words, under `Stencils`. BUS A-14 is on the left pipe. It is tied to the left edge,
  middle height. VENT PLENUM 3 is tied to the bottom right, near the middle pipe's right end.

The plot's middle moves as the plot grows. The code works it out from the plot grid's size: half
its width and half its height, counted from the plot's top-left corner. At 16:9 the middle is at
board x 960, y 545. At 2880 × 1800 it is at x 960, y 605.

The drawers:
- The drawers slide in and out from the right. The code for that is in `BoardMotion.cs`.
- Open is x 0. Shut is x 376 (**Drawer Travel Px**).
- Each drawer sheet has its pivot at (1, 0.5) and its position at (0, 0). At x 0 the drawer's
  right side is on the board's right edge.
- A new drawer under `Drawers` needs the same anchor (right middle) and the same pivot (1, 0.5).

Controls you drag:
- Some controls turn or slide around a fixed point written in their code: the comms tuning dial
  (centre 1590, 422.5), the helm's throttle well, and the sensors dial and gain fader. These points
  are on the panel's own sheet. When the sheet moves to its edge, the points move with it.
- The code for this is `PanelPoint`, in `BoardManager.cs`. It turns the mouse point into a point
  on the panel's own 1920 × 1080 sheet. Mouse down, mouse up and drag all go through it.
- A new control of this kind needs `PanelPoint` too.

Not done yet:
- The other screens still show dark bands on screens that aren't 16:9: the title, SAVE SLOTS,
  SECTOR MAP, the ship's rooms and the MISSIONS list.
- There is no limit for screens wider than 21:9. On those, the plot keeps getting wider.
- The test scene `Fire Control Codex` was not changed. Its board is still a fixed 1920 × 1080 box.
- No person has dragged a control with a real mouse on a 16:10 or wide screen yet.

## How to change a panel's art

There are two ways. The Art window is the easier one.

### In the Art window

Open **Starwolf → Art**. It shows every picture on the board in one window, and the pictures in
the open scenes. Nothing moved. Each picture is still in its prefab or scene. The window changes
it there. New on 2026-11-10. The owner: "Please, please don't keep art in tuning. Tuning is for
systems."

What you see, from the top:
- **Find**. Type a panel, a part or a file name. Only the pictures that match show.
- **Waiting for art**. Tick it to show only empty boxes and stand-ins.
- The line "A scene's own pictures show when it is open:", then one button for each scene in
  `Assets/Scenes/Tactical/` that is in the build list and not open. A button opens that scene. If
  a scene has unsaved changes, it asks you to save them first.
- One fold for each prefab in `Panels/` and in `Assets/UI/Menus/` (the Escape menu). It shows the
  panel's name and how many pictures it has.
- One fold for each open scene, named "Scene ·" and the scene's name.

While Find has text, or Waiting for art is ticked, every fold with a match is open.

Each row is one picture. Its name is in bold. The file name is under it. If the same picture is in
more than one row, "used in" and the number of rows follow the file name, for example "used in 6
places". The picture's box is on the right.

To change a picture:
1. Find its row.
2. Drop the new picture onto its box. Or click **Select** on the box and pick one.
3. If the old picture is in other rows too, a box asks: **Only here**, **Cancel** or
   **Everywhere** (with the number of rows). Everywhere changes every row that showed the old
   picture. Many stand-ins use `Shapes/circle.png`. To replace one stand-in, pick **Only here**.
4. In a panel, the change is saved into the prefab at once. In a scene, the scene is marked as
   changed. Save the scene to keep it.

A change to an Art list entry still puts the picture on all of the entry's images at once.

A panel's fold lists:
- Every Art list entry, by its name.
- Every picture the panel's code swaps. For example **Rocker Up**, **Rocker Locked** and
  **Rocker Thrown**. In Engineering each repair rocker shows by its name: "reboot · Up",
  "reboot · Locked", "reboot · Thrown". The Hangar has **Recall Up** and **Recall Pressed**. The
  Periscope has **Aperture Idle** and **Aperture Slewing**. The Plot has **Passive Bar Art**.
- Pictures on raw images: Engineering's four ship pictures, and the stars and nebulas behind the
  plot.

A scene's fold lists:
- Pictures on objects that are not copies of the panel prefabs. In `02.1_Grid`, on
  `Tactical Board`: **Pause Key Run**, **Pause Key Paused**, **Rate Key On**, **Rate Key Off**, and
  the **Thrown** and **Locked** picture for each rocker size (the **Rockers** script). In `Ship`,
  the ship rooms' Art list: four paintings and four people. In `SectorMap`, **Completed Node** and
  the console picture.
- Images in the scene that have a picture and are on no Art list. For example `PlotRing`,
  `Briefing Icon` and `ReferenceOverlay`.
- The menu screens' Art lists (Step 189). All their entries are empty.
  - SAVE SLOTS (`SaveSlots`), on `Slot List`: **Slot button** (Slot 1 to 3) and **Delete button**
    (Delete 1 to 3).
  - The briefing (`01_Briefing`), on `Level List`: **Mission button** (the 8 buttons). No script
    opens this scene any more.
  - The sector map (`SectorMap`), on `Sector Map`: **Button** and **Mission box**. See Sector map.

  SAVE SLOTS and the briefing are green words on black, like the intro. The box behind each button
  can't be seen. It shows as a faint green box only when you point at it or press it. A picture
  there shows only then too. The start screen (`00_Start`) has no Art list. The intro's code draws
  it.

In play mode the boxes are greyed out. The window says "Stop play mode to change pictures."

Not in the window:
- The rocker images themselves. Their code sets the picture. Their states are listed.
- The ships drawn from their 3D models, and other pictures the game draws while it runs.
- Fonts. They are still on each panel's **Fonts** list.

### In the prefab

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
  outside the Art list. **Starwolf → Art** lists them, one row per state. The rockers also slide
  in play. The script for that is **Rockers**, on
  `Tactical Board` in the scene. It uses the rocker sprites too.
- Colours that change in play, such as lamps lighting up. The code takes them from the palette
  (see [Colours](#colours)). A colour you set on the image is replaced in play.
- Links to objects outside the prefab. Only the sensors drawer has these.
- The anchor and pivot of a panel's sheet on the board. Each sheet is tied to a board edge (see
  [Screens of other shapes](#screens-of-other-shapes)).

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
port. When you let go, the ship keeps turning until it reaches the order. A turn starts smoothly
from where the ship is, the moment you press the key (Step 175). Held, the ship keeps turning
without a stop.

W and S move the throttle (Step 171). A tap moves it 1. Keep the key down and it keeps counting,
slowly at first, then faster. It moves 100 in 2 seconds. It stays where you let go. S goes on past
0 into reverse. The throttle stops at what ENG allows. The 2 seconds is **Throttle Hold Seconds**,
on the Helm tab of **Starwolf → Tuning**.

Q and E slide the ship to port or starboard (Step 171). The nose stays still. A tap gives a small
push. Keep the key down and the push starts gentle, then grows to full in 2 seconds. Let go, and the
ship keeps sliding. The slide fades by itself. A tap the other way slows it. With no ENG power, Q
and E do nothing. The numbers are in the ship's stats file, under Side thrusters: **Side Tap Units**
and **Side Ramp Seconds**.

Shift with A or D is the slow turn, for aiming the spinal railgun (Step 170). While both keys are
down, the ship turns 2° a game second, in parts of a degree. ENG power doesn't change it. The order
follows the bow, and the attitude key clears. The HDG readout still shows whole degrees. The ship on
the plot turns by the part of a degree. A normal A or D turn drops the part of a degree. The speed
is **Fine Turn Deg Per S** (2), on the Ship tab of **Starwolf → Tuning**, under Navigation. The key
is **Fine Turn Key** (LeftShift), on the Keys tab.

With a lock on and the spinal railgun picked in Fire Control, the ship turns itself onto the
target's lead (Step 170). It turns the whole degrees at its usual rate, then the last part of a degree at the
slow turn's rate. A and D can't turn against it. Shift with A or D still works. When the lock runs
out, the ship stops turning itself.

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
- **Campaign** (`Campaign.asset`): the file the map reads its missions from.

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
  right once per update. The picture also takes off the turn of the camera that films the ship
  (Step 170). A wrecked ship's camera turns with the wreck (`ShipView.WreckHeading`, set by
  `PlotPanel.ShowBroken`) and never turns back. Before this fix, a ship brought back after a wreck
  showed tilted. `PeriscopePanel.PictureBowDeg` is where the bow points in the sight, in degrees
  counter-clockwise from up: 90 or 270 when it lies flat. The playtest reads it. The picture uses the
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

On a 16:9 screen the plot is 1712 × 890 px. On other screens it is bigger (see
[Screens of other shapes](#screens-of-other-shapes)). Its top-left corner is at board x 104, y 100.
Its parts' positions count from that corner.

The prefab has these parts, from back to front:
- `Ground`, `Grid` and `Glow`: the plot's background, the grid and its glow. `Nebula far`, `Stars`,
  `Nebula near` and `Close-up (3D view)` are between `Ground` and `Grid`, in that order. The three
  nebula pictures are the space under the ships when you zoom in. `Close-up (3D view)` is the real
  look of ships and rocks when you zoom in (see below)
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

The close-up starts to show at zoom x1. At x1.1 it has fully replaced the line pictures. A level
opens at x1, with the line pictures. One notch of the scroll wheel zooms x1.12, so one notch in
gives the full 3D look. **Close Up From Zoom** (1) and **Close Up Full Zoom** (1.1) set these. They are on the Board tab of
**Starwolf → Tuning**, under Plot & our ship. To open levels in the 3D look, set them to 0.8 and 1.
The plot zooms in to x4. At x4 our ship is 608 px long.
**Zoom In Limit** (4) sets this, on the same tab under Board & time. The SCALE knob turns from full
zoom-out to x4. The scale box shows x1.0 up to x4.0.

The close-up shows a ship only when it is in sight. A ship behind a rock is left out. Every ship is
the same grey in the close-up. A ship's white, red or amber colour shows only in the
line pictures. The name labels and the white lock corners still show. A destroyed ship breaks in two
in the close-up too. Shots and blasts are below. Our craft show as small 3D models (below). Shots and torpedoes
keep their flat dot and trail on top. Gas clouds are unchanged.

**Our craft in the close-up** (Step 199; owner 2026-11-11). Zoomed in, each craft we launch, fighter
or assault boat, is a small grey 3D fighter model. Its nose points the way it flies. Its white
triangle fades out as the close-up comes in. Zoomed out, only the triangles show. The models fly in
the same V as the triangles. Zoomed right in, the V spreads so the craft are never closer than one
model length, so they never overlap. The triangles' V does not change. **Fighter Model Units** (10)
is a model's length in units. It is on the Board tab of **Starwolf → Tuning**, under Hangar &
fighters. **Fighter Model** on the Plot panel points at the model,
`Assets/3D Assets/Starships/StarWolf_LightFighter.fbx`. Every craft uses this one model (owner). It
is a sample; the owner will find a better one. Leave the slot empty and the triangles show at every
zoom. The models wear the ships' grey metal, **Torpedo Look** (`Assets/Art/Close Up/Ship Metal.mat`).
They are on the TransparentFX layer, like the torpedo models, so the small ship pictures don't show
them.

The close-up is the picture `Close-up (3D view)` in `Plot.prefab`. It is just above the three
nebula pictures and under `Grid`. The grid lines, marks, labels, shots, fighters and torpedoes draw on top of it. It is
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

Each ship's 3D model is as long as its **Length** number (Step 179). Length is in the ship's stats
file. It is at the top of the Ship tab (our ship) and the Enemy tab of **Starwolf → Tuning**. When a
level starts, the model is scaled so its longest side, seen from above, is the Length. It is scaled
evenly, so the width grows or shrinks with it and the shape stays the same. Change Length during play
and the model follows at once. Today our Asura is 200 units, the Garuda 350, the frigate 100.
**Width** does not change the model. It only sets the width of the hit box. A new ship needs no
scaling by hand: set its Length in its stats file in `Assets/Ships`.

The two cameras that film the line pictures, Ship View Camera and Contact View Camera · Garuda, follow
Length too. How much each one frames, and how far back it is (twice the Length), come from the
Length. So the line picture stays whole at any Length. The old **Height** value on the Board tab is
gone. Each camera films only its own ship. **Own Ship Max Px** (1200) lets a ship's drawn size follow
its model up to x4.

In the close-up, the space under the ships is a nebula. There are soft violet and teal gas clouds,
stars, and thin warm and cold wisps of gas. The grid, marks and labels draw on top. Zoom out to x1
and the nebula fades away with the close-up. Past visual range it is cut at the circle, like the
ships.

The nebula is three pictures in `Plot.prefab`, back to front: `Nebula far`, `Stars` and
`Nebula near`. They are between `Ground` and `Close-up (3D view)`. Each one repeats in every
direction. Far layers move less as you pan. They also grow less as you zoom. What is at the plot's
centre stays put while you zoom. **Nebula** on the Plot
panel is a list with one entry per layer:
- **Image**: the layer's picture in `Plot.prefab`.
- **Depth**: how fast it moves with the grid, at x1. At 1 it moves with the grid. Nebula far is 0.15,
  Stars 0.3, Nebula near 0.5.
- **Tile Units**: one copy of the picture is this many units across at x1. Nebula far is 9,000,
  Stars 1,300, Nebula near 5,000.
- **Alpha**: how strongly it shows. Nebula far is 1, Stars 0.9, Nebula near 0.8.

The pictures are stand-in art, made by a script. They are 1024 x 1024 and repeat without seams.
They are `Nebula Far.png`, `Stars.png` and `Nebula Near.png` in `Assets/Art/Close Up`. To change the
look, click the layer in `Plot.prefab` and put another picture in its Texture. Or replace the PNG
with one of the same name. A new picture must repeat without seams. Its Wrap Mode must be Repeat.
Nebula Near and Stars need see-through parts.

The metal reflects the nebula's colours. The board scene `02.1_Grid` uses
`Assets/Art/Close Up/Nebula Reflection.png` for reflections. It is set in the Lighting window, under
Environment Reflections, Source Custom. It is a copy of Nebula Far, imported as a cube. If you
change Nebula Far, change this copy too. The scene's ambient light is a flat dim violet. It is set
in the Lighting window, under Environment Lighting. The line pictures do not change.

In the close-up, every shot in flight is a glowing streak. Ours are blue-white. The enemy's are red.
A torpedo's streak is wider than a railgun slug's. The flat dot and trail still draw on top. PDC fire
is a thin stream (below). Flashes and blasts run in game time: they stop when the board is paused.

A torpedo drifting with its motor off is a small metal cylinder along its nose, with no glow (Step
184). Its metal is **Torpedo Look** on the Plot panel (`Assets/Art/Close Up/Ship Metal.mat`, the same
as the ships). When its motor bursts on, a big soft flare grows and fades: blue-white for ours, pale
red for the enemy's. Then the torpedo is a streak with the cylinder on it. Zoomed in, the table's
white dot and trail fade out for torpedoes, so the cylinder shows. On the table, a small amber ring
grows and fades where the motor lit.

**Our railgun** (Step 197; owner 2026-11-10, C2 and C3 on the Ship Actions page). Nothing glows along
our ship: the ship is built around the gun. In the close-up:
- While the railgun is armed, a blue-white charge glow pulses at the gun's mouth. It grows over
  about a second. **Muzzle Charge Units** (30) is its size across when full.
- When it fires, a bright flash shows at the mouth and fades. **Muzzle Flash Units** (90) is its
  size across. **Muzzle Flash Seconds** (0.3) is how long it lasts, in game seconds.
- At the same time, two puffs of pale grey vent gas blow out of the sides of the bow, spread and
  fade. **Vent Gas Seconds** (1.1) is how long they last. Their size follows Muzzle Charge Units.

On the table, at every zoom:
- A white ring closes in on the gun's mouth while it is armed, and a ring opens there as it fires.
  Their size follows **Torpedo Ring Px**.
- Our shot leaves a thin white line from where it left the gun to the slug. The line fades over
  **Shot Line Seconds** (5) game seconds. The owner: "part of the charm". The enemy's shots leave
  no line. The owner hasn't answered whether they should.

**The far shot's countdown** (Step 197; owner 2026-11-10, D2). A POINT near our slug's path shows a
countdown in place of its time: `IMPACT 4 S`, `IMPACT 3 S`… down to `IMPACT 1 S`. Then `IMPACT ?`
blinks, and the POINT shows its time again. **Impact Near Units** (500) is how close to the slug's
path a POINT must be. **Impact Blink Seconds** (3) is how long `IMPACT ?` blinks, in game seconds.
The rule for when a hit is reported is in `docs/TACTICAL_BOARD_SPEC.md`, Rule 8.4.d.

**Our boats and fighters leaving** (Step 197; owner 2026-11-10, B3). In the close-up, a light
blue-white streak trails our squadron's leader while the catapult's push lasts. It shrinks as the
craft slows. **Craft Streak Units** (60) is its length at full push. When the craft breaks hard
onto its target, a thruster flash shows on the outside of the turn. **Break Flash Units** (40) is
its size across. **Break Flash Seconds** (0.3) is how long it lasts. Only our ship launches craft;
no other ship has a hangar that launches.

**PDC streams** (Step 197; owner 2026-11-10, E2). When a PDC group fires at an enemy torpedo, a
stream of rounds runs from the gun toward it. On the table it is a white dotted line. In the
close-up it is a thin blue-white line, brightest at the torpedo's end. It starts **Pdc Walk Units**
(60) off the torpedo. On a kill it walks onto the torpedo, the torpedo goes down, and the hit flash
shows. On a miss it stays off. The stream lasts **Pdc Stream Seconds** (0.5) game seconds, plus a
short fade. That number is in each ship's stats file, on the Ship tab of **Starwolf → Tuning**,
under Weapons, because it also sets when the torpedo goes down.

Every value in these four parts, except Pdc Stream Seconds, is on the Board tab of **Starwolf →
Tuning**, under Plot & our ship. They need no art: the glows use the same `Glow.mat` as the shots,
and the table marks are drawn by code. Every change is in `docs/TUNING_LOG.md`.

Where a shot hits a ship, there is a flash: a burst of orange sparks with a white centre. The same
flash shows when a torpedo is shot down. It shows only in the close-up.

When a ship is destroyed, there is a blast: a big white flash, sparks and slow embers. Its model then
splits in two where the shot struck, the same as its line picture. The two halves drift apart and
turn slowly. **Wreck Drift Units** and **Wreck Spin Deg** set this for both pictures. The halves are a
duller, darker grey metal. Zoomed out, the broken line picture shows as before. Between x1 and x1.1
the two fade into each other.

**Slug Width Units** (6) and **Torpedo Width Units** (11) set the streak's width at the front.
**Our Shot Glow** and **Their Shot Glow** are the two streak colours. **Torpedo Body Units** (14) and
**Torpedo Body Ink** (pale grey, 70%) set the torpedo body's length and colour. **Torpedo Flare Units**
(60) is the flare's size across as it starts. It grows to one and a half times that. **Torpedo Flare
Seconds** (0.6) is how long the flare and the ring take to fade, in game seconds. **Torpedo Ring Px**
(16) is how much the ring grows on the table. These and the two wreck values
are on the Board tab of **Starwolf → Tuning**, under Plot & our ship.

The shot and blast files are in `Assets/Art/Close Up`:
- `Glow.mat`: every streak. Its Colour sets how bright every glow is. It is now 3.
- `Glow.png`: the soft dot every streak, spark and flash is drawn with.
- `Glow.shader`: the glow. It adds light.
- `Hit Flash.prefab`: the hit's sparks and centre flash.
- `Kill Blast.prefab`: the blast. It has Flash, sparks and Embers.
- `Wreck Metal.mat`: the halves.

**Glow**, **Hit Flash**, **Kill Blast** and **Wreck Look** on the Plot panel point at them. The glow
in `Close Up Look.asset` has High Quality Filtering on, Scatter 0.55 and Threshold 0.9. The old
settings showed a blocky halo round the blast.

The shots, blasts and halves are on Unity's TransparentFX layer. Only the close-up camera films it.
The `Sun` lights it too. The close-up picture lets its light add over the nebula. A glow over empty
space shows. `Assets/UI/Shader/BoardLinearImage.shader` has a hidden value for this. Every other
picture on the board is unchanged.

The game reads a model's shape to cut it in two. A model that can't be read is not cut: that wreck keeps
its broken line picture in the close-up. The model
file of the Garuda and the frigate, `Assets/3D Assets/UpdatedStarships/StarWolf_Cruiser_mesh.fbx`,
has Read/Write on. Our ship's model does not need it. For a new enemy model, turn on Read/Write in
its import settings.

While Fire Control is open, each railgun shows a soft dotted cone and a solid bar on
`Weapon ring (drawn)`. The bar is the gun. **Rail Arc Alpha** and **Gun Bar Alpha** set how strong
they are. Where the guns are on the ship is set on the Hard Points tab of **Starwolf → Tuning**.
A fixed gun, such as our spinal railgun, has no cone (Step 170). Its bar runs from its dot on the
nose out to its full reach, 30,000. The reach ring still shows.

A locked ship has white corners round it on the plot, the same as the targeting computer's box. A contact out of
sight is a marker where it was last seen. It fades as it gets old, and its label says how old it is. **Stale
Seconds**, **Stale Alpha** and **Stale Label Seconds** are on the Board tab of **Starwolf → Tuning**, under Sensors.
The RULER has a thicker line and white distance text: **Ruler Width** and **Ruler Dot** are under Plot tools.
With the spinal railgun picked, a RULER's label also shows how long its slug takes to fly its length, for
example `18,000 km · SLUG 12 S` (Step 170). A ruler from one POINT to another (a track) still shows course and speed.

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

**Our battle group** (Step 178). A level can have friendly ships. In ORIENTATION they are INDRA,
VARUNA and MARUT, round our ship at the start. Inside visual range, each shows in blue, scanned or
not: its line picture, its dot and its label. Before a scan, the label reads BOGEY F1, F2 or F3.
After an ACTIVE SCAN passes over it, the label shows its name. Past visual range, a scanned friendly
ship is a blue dot. One not scanned shows like any unknown ship: a red bar on the DRADIS circle,
with its bearing. The blue is **Own Force** in `Palette.asset`. Zoomed in past x1.1, the
close-up shows them as full 3D models. They can't be targeted. They are solid, like every ship.

They fly their own path in a loose formation, and we follow them. They never follow us. The path
is the level's **Group Route** (on its Mission): points they fly to in order, at 15 units a second.
They wait at the last point. With no route, they stay where they start. Our place is the middle of
the formation: its centre starts on our ship. Each ship flies to its place round that centre,
turned the way the group flies, and drifts gently round that place. In ORIENTATION the path ends at
(0, 2,800), south of the rock field: the carrier and battleship are too big to get through the
rocks, so from TAKE POINT on we go through alone.
They steer round rocks and ships the way the enemy does, and they stay inside the battle space.
They keep at least 500 units from our ship and go round it to reach their places. If we get
closer than that, they move away first.
To change how they fly, open the level and go to the Board tab of **Starwolf → Tuning**, under
Battle group. **Escorts** turns the formation flying on. **Slot Units** is the ship's place: x to
the right, y ahead. INDRA is on the left (−700, 100), VARUNA on the right (700, −50) and MARUT
behind (0, −700). **Group Speed Units** (15) is how fast the formation flies its path. **Sway Units** (150) and **Sway Seconds** (60) set
how far and how slowly each ship drifts round its place. **Keep Clear Units** (500) is how close
they come to our ship. **Make Room Share** (0.3) is how fast they move away when closer than that.
**Catch Up Per Unit** (0.05) is how much faster a ship flies when it is away from its place.
**In Place Units** (100): this close to its place, a ship turns the way the group flies.

To add one to a level, drag its prefab from `Assets/Prefabs/BattleGroup` into the level scene. Its
name on the plot comes from the object's name: `Indra · Carrier` shows as INDRA, CARRIER. Each
prefab has **Friendly** ticked on its World Contact. Its Enemy Brain has **Hunts** off and
**Escorts** on.

Ask a technical artist before you change these:
- The code copies four templates in play: `Passive label template`, `Fighter (template)`,
  `Contact label (template)` and `Mark label (template)`. Keep them switched off.
- Two things place everything the code draws: the plot's middle and **Px Per 1000 Units** (760,
  on `PlotGrid`). The code works out the middle from the plot grid's size, counted from the plot's
  top-left corner. At 16:9 it is at board x 960, y 545. Until 2026-11-10 it was a fixed field,
  **Plot Centre**. It is no longer in the inspector. Changing the plot's gaps to the board's edges
  (104, 100, 104, 90) means changing the code.
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
which launcher will fire. The launcher picks itself. A salvo uses the other launcher too, by turns
(Step 184). The design's own
PDC controls (quadrants, LOADERS) and torpedo controls (warhead, SALVO) are not drawn yet. The
loaders and the salvo start from each ship's stats file.

Our ship's railgun is a spinal railgun, fixed on the nose (Step 170). It can't swivel. It points
straight ahead. The game treats any railgun with Half Arc 0 as fixed. You aim it by turning the
ship. For a fixed gun:
- The mount screen reads its name, FIXED and its arc, its reach and its slugs: RAILGUN,
  FIXED 000°, 30,000 km, SLUGS 12.
- It can fire with no target and no lock. LOAD, ANGLE and BEARING open with nothing set. ARM still
  needs the capacitor's charge.
- A lock turns the ship onto the target for you (see Helm). Only a lock does this. A TRACK solution
  does not.
- With the four switches set, the ready line reads MOUNT READY. A gun that turns reads SOLUTION
  LOCKED — MOUNT READY. For a fixed gun the ready line never asks for a target or a lock. It goes
  on to WEAPONS NOT RELEASED or ARMING SEQUENCE INCOMPLETE.
- The slug leaves the nose and flies straight along the nose's line, to the part of a degree. It
  keeps the ship's motion. A drifting ship's shot drifts off the line.

When a salvo is fired, this is what shows on the plot (Step 184, owner 2026-11-10: "drop, drift,
flare" and the ripple salvo):

1. The torpedoes leave one at a time, 0.3 game seconds apart. Ours come from both launchers by
   turns, starting with the one on the target's side. A salvo of two is one from each launcher.
   Each leaves from where its launcher is when its turn comes.
2. Each is pushed out of the ship's side, tilted 30° toward the stern. Torpedoes from the same
   launcher fan out either side of that line.
3. It drifts with its motor off. It slows fast at first, then hardly moves. It drifts about 300
   units in 3.5 game seconds. Its nose swings onto the target as it drifts. The turn starts gently,
   gets tighter, then eases off as the nose lines up.
4. Its motor bursts on. It speeds up to full speed in about 0.2 game seconds. It flies where its
   nose points and turns onto the target.

Enemy torpedoes do the same. An enemy's salvo comes from one launcher. Each of its torpedoes leaves
from where that launcher was when it fired. The flight is the same at every game speed.

A line can be drawn out of each launcher (see Drawing the line, below). With a line, the torpedo
drifts down the line in place of the straight push out. It slows the same way. Its motor lights at
the line's end. Since Step 184 no ship file has a torpedo line (owner 2026-11-10: "A2 wins. Take
the lines off"). Each torpedo in a salvo flies its own copy of the
line, turned by the salvo's fan (**Torpedo Spread Deg**). A launcher on the centre line, as on the
Garuda, sends every other torpedo along the mirror of the line. Enemy torpedoes use the lines in
their own ship's file. With no line, they leave as in step 1.

To change this, use the Ship tab of **Starwolf → Tuning**, under Weapons. Each ship has its own
numbers:
- **Torpedo Launch Speed Units** (250): how fast it is pushed out of the launcher.
- **Torpedo Tilt Back Deg** (30): how far it tilts toward the stern as it leaves.
- **Torpedo Spread Deg** (20): how far apart torpedoes from the same launcher fan.
- **Torpedo Ripple Seconds** (0.3): the time between one torpedo of a salvo leaving and the next,
  in game seconds.
- **Torpedo Coast Seconds** (3.5): how long it drifts with its motor off, in game seconds. Its motor
  lights at the end.
- **Torpedo Coast Speed Units** (5): its speed at the end of the drift.
- **Torpedo Curve Deg Per S** (90): the fastest its nose swings while it drifts, in degrees a game
  second.
- **Torpedo Turn Build Seconds** (0.3): how long a turn takes to build up and to ease off, in game
  seconds.
- **Torpedo Burn Units** (1,100): the speed its motor adds each game second.
- **Torpedo Speed Units** (250): its full speed.
- **Torpedo Turn Deg Per S** (140): the sharpest it turns with its motor on, in degrees a game
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

### Fire Control - Codex (test panel)

`Panels/Fire Control - Codex.prefab`. Codex's art for the fire control drawer (owner,
2026-11-10). It is a prefab variant of `Fire Control.prefab`. It uses the same script, **Fire
Control Panel**, and the same controls and wiring. It is only in the test scene
`Assets/Scenes/Tactical/Fire Control Codex.unity`. The real board still uses `Fire Control.prefab`.
The owner has not decided yet whether this one replaces it.

How it plays:
- Press FIRE on the systems rail. The drawer slides in from the right, like the other drawers.
  Press FIRE again, and it slides out.
- The top window reads FIRE CONTROL and TARGETING COMPUTER. The small window on its right shows the
  mount's short name.
- The big window under it is the monitor. Top left is the target's name: BOGEY T07 until it is
  scanned, then its name in amber, such as GARUDA. With no target it reads NO TARGET. Top right is
  the SIGHT key.
- Press SIGHT. The periscope comes down, and its round red view shows in the middle of the monitor.
  The SLEW key is on its left. The MAG knob is on its right.
- With SIGHT up, a scanned Garuda shows as a pale wireframe picture. It shows only once the ship is
  scanned, and only for a ship whose name starts with GARUDA.
- Under the view is PERISCOPE BEARING, with the bearing and offset. Then the tracking ID, such as
  TK-014-T07, and where the track comes from, such as SCANNED, NOT LOCKED.
- The firing solution window shows RANGE in km, REL BRG and TGT SPD. Under them is the ready line,
  such as WEAPONS NOT RELEASED — CONDITION 2 REQUIRED, MOUNT READY or ROUNDS AWAY.
- The control bank has four sections. Their numbers, 1 to 4, are in the small windows on the right
  edge:
  - 01 WEAPON SYSTEM: the system and mount knobs, then the mount's name, arc, range and slugs.
  - 02 FIRING SOLUTION: the RANGE, BEARING and SPEED knobs, and the TRACK key.
  - 03 CAPACITOR: ten cells, the shot-cost mark, the charge %, the note and % PER SHOT.
  - 04 ARMING SEQUENCE: the LOAD, ANGLE, BEARING and ARM switches, drawn at three quarters size.
    Under them is the FIRE button.
- The FIRE button is in the drag lever's place. While a press would not fire, it is dark, with a
  dim FIRE. When a press would fire, it turns bright red, glows and reads FIRE  FIRE  FIRE. It drops
  1 px while it is pressed.
- One press fires one shot. The rules are the same as the lever's: the four switches set, enough
  charge, not PDC, and a target for torpedoes. Firing drops ARM. A second press does nothing until
  ARM is thrown again.

The art:
- The housing is one picture: `Art/Fire Control/InspectorHousing.png` (403 × 1080). Its windows are
  see-through glass. It is in `Housing`. The base prefab's frame, latch, screens, label plates,
  rocker housings and fire lever are switched off. The picture draws them.
- The Garuda's wireframe is `Art/Fire Control/Garuda-profile.png`, in `Contact profile`.
- Both pictures use the board's sprite import settings.
- The drawer is 890 px tall, the same as the plot. On the 1920 × 1080 board it is drawn from x 1484
  to 1816 and from y 100 to 990.
- The variant adds `Housing`, `Contact name`, `Contact profile`, `Fire button` (with `Fire glow` and
  `Label`), the four `Tab` windows, and labels for the knobs and windows.

The test scene is a copy of `02.1_Grid`. It is not in Build Settings or in the game flow. Wolf Hunt
(Level_01) loads on top, as usual. It differs from `02.1_Grid` in three ways:
- The fire control drawer is `Fire Control - Codex`. The old one is removed.
- The periscope is inside the fire control drawer, scaled to fit the monitor. Its housing bar,
  readouts, slew bar, subject and convergence bar are switched off. Its round rim is see-through.
  The LOWERED label at the top of the board is switched off. Nothing shows at the top of the board
  where the periscope was.
- The **Rockers** list on `Tactical Board` points at the new drawer's four switches.

Ask a technical artist before you change these:
- **Contact Name**, **Contact Profile** and **Profile Name** (GARUDA) fill the monitor. The
  picture shows only for a scanned ship whose name starts with Profile Name.
- **Fire Key**, **Fire Face**, **Fire Glow** and **Fire Label** are the FIRE button's parts. The
  code moves, colours and writes them. The button sends the click `fc.fire`.
- These fields are empty on `Fire Control.prefab`. With them empty, the panel works as before.
- **Fire Glow Seconds** (0.8) is how long one pulse of the glow takes. It is on the Board tab of
  **Starwolf → Tuning**, under Motion, with the test scene open.
- The button's glow and `WeaponSystem.Fire()` both read `WeaponSystem.ReadyToFire`. It is the one
  list of the fire rules. Change the rules there.

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

Our ship carries no fighters (Step 170). Each ship's file has a number, **Fighters** (0 to 6,
default 6), on the Ship tab of **Starwolf → Tuning**, under Hangar. The Asura's is 0. The Garuda's
is 6. At load the ship keeps only that many fighters. With none, the hangar starts on the assault
boats. A squadron the ship doesn't carry shows nothing in its row: the INTERCEPTOR row on our ship
is empty.

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
RESTART MISSION plays the level again. In a skirmish, MISSION SELECT goes back to the title. In a
campaign it opens the ship's rooms (see Ship rooms, below). After a win of the campaign's last
mission, it opens the end card instead (see End card, below).

It is the pause menu too. In a level, Escape stops the game and opens it. The title says PAUSED in
white. The reason line is empty. The target, our ship and the time are hidden. Two more buttons
show on the top row: RESUME and QUIT GAME. RESTART MISSION and MISSION SELECT are below them.
Escape again, or RESUME, closes it. Time goes back to how it was: running, or still paused if the
player had pressed P before. While it is open, P and the drawer keys 1 to 6 do nothing. QUIT GAME
quits the game. In the Unity editor it stops play mode. On the MISSION COMPLETE or SHIP LOST
screen, Escape does nothing, and RESUME and QUIT GAME are hidden.

It is the last part under `Tactical Board`, so it draws over everything else. Keep it last.

The prefab has these parts:
- `Screen`: switched off while the mission runs. The script switches it on. Leave it off.
- `Shade`: a dark layer over the whole board. It takes the clicks, so nothing under it can be
  pressed.
- `Plate`: the box in the middle, with `Title`, `Reason`, `Rows` (`Labels` and `Values`) and
  the four buttons. RESUME and QUIT GAME are copies of RESTART MISSION, at y −70, x −160 and
  +160. Their labels are on the panel's Barlow Condensed SemiBold font slot.

Its Art list has three entries (Step 189). They also show in **Starwolf → Art**.
- **Shade**: empty. The shade is a plain dark colour.
- **Plate**: `Plates/plate_dark.png`, the board's dark plate. A stand-in.
- **Button**: `Plates/plate_legend.png`, the board's bevelled key, on all four buttons. A stand-in.

The plate and the buttons are set to Image Type Sliced. Their colours are the same as before.

Ask a technical artist before you change these:
- The script writes the title, the reason and the values. It colours the title green or red.
  On the pause menu it writes PAUSED in white and hides `Rows`.
- Each button has a **Click Spot** with its action (`end.restart`, `end.menu`, `end.pause`,
  `end.quit`). MISSION SELECT uses `end.menu`. RESUME uses `end.pause`, the same action Escape
  sends. QUIT GAME uses `end.quit`.
- **Resume Button**, **Quit Button** and **Rows** are the parts the script shows and hides.
  RESUME and QUIT GAME show only on the pause menu. `Rows` is hidden on it.
- **Menu Scene** (`00_Start`, the title) is the scene MISSION SELECT loads outside a campaign
  (Step 178). It was the MISSIONS list, `01_Briefing`, before.
- **Menu Key** (Escape) opens and closes the pause menu. It is on the Board Manager, on the Keys
  tab of **Starwolf → Tuning**.
- **Delay Seconds** (2) is the wait between the end and the screen, so the last hit is seen
  first. It is on the Board tab of **Starwolf → Tuning**, under Board & time.

### Ship rooms

Scene: `Assets/Scenes/Tactical/Ship.unity`. Script: **Ship Rooms**, on `Ship`. In a campaign,
MISSION SELECT on the End Screen opens it. It shows the board's metal frame, with five tabs along
the top: BRIDGE, ENGINEERING, HANGAR, QUARTERS and SECTOR MAP ›. The picked tab is gold. The others
are grey. BRIDGE shows first. SECTOR MAP › opens the sector map.

Each room is a painting with people in it. Click a person, and the talk box opens at the bottom.
It shows their picture, their name in gold and what they say. Up to three answers are on the
right. An answer leads to another line, or ends the talk. A line with no answers shows LEAVE.
Picking another tab closes the talk.

The BRIDGE, ENGINEERING and QUARTERS paintings are stand-ins: dark coloured panels with the room's
name in faint letters. The people there are grey boxes with their name under them.

The HANGAR has Codex's art (2026-11-10). The painting is `Assets/UI/Rooms/HangarRoom.png` (1672 ×
941). The FLIGHT LEADER's picture is `Assets/UI/Rooms/FlightLeader.png` (305 × 392). It is cut
from the painting at left 30, top 344. They are in the Art list slots HANGAR painting and FLIGHT
LEADER picture. Both use the board's sprite import settings. The painting keeps its shape in the
1712 × 890 room, with thin dark bars left and right. The FLIGHT LEADER button is exactly over her
in the painting. Click her, and her talk opens with her picture. GOOD closes it. A click anywhere
else on the painting does nothing. The grey stand-in title HANGAR is switched off in this room. The
talk box picture keeps its shape for every person. The people and their lines are stand-ins too:
the XO on the BRIDGE, the CHIEF ENGINEER in ENGINEERING, the FLIGHT LEADER in the HANGAR and the
MARINE SERGEANT in QUARTERS.

ENGINEERING shows a DAMAGE REPORT beside the CHIEF ENGINEER. In a campaign, our ship keeps its
damage after a won mission. The next campaign mission starts with it. The report lists each part
that is not whole as a REPAIR button, with what is left and the most it can be: REPAIR PROW ARMOUR
25 / 100, REPAIR MAGAZINE 3 / 5. Each repair fills the travel bar (see below). With no damage it
reads ALL PARTS WHOLE. The armour on
each side and each room's integrity stay. Fires, radiation, vented rooms and crew losses end with
the mission. A room that ends at 0 is kept at 1. A win's damage goes into the save when you press
MISSION SELECT; RESTART MISSION on the win screen replays from the damage before it. A lost mission
keeps nothing. A skirmish always starts whole. The panel is
Damage report, in Room · ENGINEERING. Ship Rooms writes ALL PARTS WHOLE in it through Damage
Report, under Repairs. The REPAIR buttons go in its `Actions`.

**The travel bar and the actions** (Step 198). In a campaign, the rooms show a travel bar at the
top right: TRAVEL 0 / 10. Each won mission gives a fresh bar of 10 units. Each room shows its
actions as buttons, in a box at the top right, in the damage report's style. A button reads what it
does, how it stands, what it gives and its cost: REPAIR REACTOR   1 / 5   +2   COST 3.
- ENGINEERING, in the DAMAGE REPORT: one REPAIR button for each damaged part. The reactor costs 3
  and gives back 2 of its 5 points. Another room costs 2 and gives back 2 points. One side's armour
  costs 1 and gives back a quarter of that side's full armour.
- HANGAR, in the STORES box: RESTOCK TORPEDOES shows both launchers together, for example 7 / 16.
  It costs 2 and gives 4 torpedoes, to the emptiest launcher first. EXTRA AMMO shows the railgun's
  rounds and the PDC groups' rounds. It costs 1. It gives the railgun and each PDC group 20% of its
  full rounds, rounded down: 2 slugs, and 480 rounds a group.
- QUARTERS, in the MARINES box: HEAL MARINES costs 2 and gives 6 marines.
- BRIDGE: talk only.

Each click adds its cost to the bar and saves at once. A button that doesn't fit in the bar is
greyed out. HANGAR's and QUARTERS' buttons always show, greyed out when full. The stock can still
be read on them. When the bar is full, every action locks. The SECTOR MAP › tab flashes amber, once
a second. SECTOR MAP › works any time. Any units left are lost. After a lost mission, the bar shows
10 / 10, locked. A new campaign starts with the bar shut. The first bar comes with the first win.

The numbers, on the Board tab of **Starwolf → Tuning**, under Travel bar, with `Ship.unity` open:
- **Travel Units** (10): the units in the bar.
- **Reactor Cost** (3), **Room Cost** (2), **Armour Cost** (1), **Ammo Cost** (1), **Torpedo Cost**
  (2) and **Marine Cost** (2): what each action costs.
- **Room Points** (2): the points one repair gives a room or the reactor, out of 5.
- **Armour Share** (0.25): the share of a side's full armour one repair gives back.
- **Ammo Share** (0.2): the share of full rounds one EXTRA AMMO gives the railgun and each PDC
  group, rounded down.
- **Torpedoes** (4) and **Marines** (6): what one RESTOCK TORPEDOES and one HEAL MARINES give.
- **Flash Per Second** (1): how fast SECTOR MAP › flashes once the bar is full.

The parts, under `Ship`:
- `Travel bar`: the bar. Its `Fill` grows from the left as the bar fills.
- `Damage report` (ENGINEERING), `Stores` (HANGAR) and `Marines` (QUARTERS): the three boxes. Each
  has an `Actions` inside, where its buttons go.
- `Action`: switched off. Every action button is a copy of it. It was the REPAIR ALL button.

To give it art, use the Art list on `Ship`. It has one slot per room painting: BRIDGE painting,
ENGINEERING painting, HANGAR painting and QUARTERS painting. It has one slot per person: XO
picture, CHIEF ENGINEER picture, FLIGHT LEADER picture and MARINE SERGEANT picture. Put a sprite in
a slot, and the room or the person shows it. The talk box shows the same picture as the person in
the room. While the game runs, a picture with a sprite shows in its own colours; the grey of the
stand-in box goes. Import a new painting the way the board's other art is imported: sRGB (Color
Texture) off. With it on, the painting looks too dark. Escape opens the Escape menu on this screen, as on
SAVE SLOTS and the SECTOR MAP (see Escape menu, below).

To change the words, use the **Rooms** list on Ship Rooms:
- Each room has a **Name**, a **View** (the painting), a **Tab**, **People** and **Actions** (where
  its action buttons go; empty means no actions in that room).
- Each person has a **Name**, a **Picture** (the box you click) and **Lines**. The talk starts at
  the first line.
- Each line has a **Text** and up to three **Replies**.
- Each reply has a **Text** and a **Next**. Next is the line it leads to, counting from 0. At -1
  it ends the talk.

The scene is a copy of the board scene `02.1_Grid.unity`. Only the board's camera, its picture
and the `Frame` are kept. The rooms are drawn on top of the frame, where the plot is on the board.
The tabs are in the frame's top band.

Ask a technical artist before you change these:
- **Campaign Scene** (`Ship`) on the End Screen Panel is the scene MISSION SELECT loads in a
  campaign.
- **Map Tab** is the tab that leaves the ship. **Map Scene** (`SectorMap`) is the scene it loads.
- **Talk Box**, **Talk Picture**, **Talk Name**, **Talk Line** and **Reply Buttons** are the talk
  box parts the script fills, shows and hides.
- **Tab Picked** and **Tab Not** are the tab colours. The script sets them.
- **Our Ship** (`Assets/Ships/Asura.asset`) tells the saved torpedoes, railgun slugs and PDC rounds
  apart.
- **Travel Fill**, **Travel Text** and **Action Template** are the bar's fill, its words and the
  button every action copies.
- The actions find their room by its **Name**: ENGINEERING, HANGAR, QUARTERS. Rename a room, and it
  loses its actions.

### Sector map

Scene: `Assets/Scenes/Tactical/SectorMap.unity`. Script: **Sector Map**, on `Sector Map Script`.
It is the campaign's map of missions. SAVE SLOTS opens it. So does SECTOR MAP › in the ship's
rooms. Each mission is a dot. Win a mission, and the next dot shows. Codex made the look (Step
172). Since Step 178 the map has one story mission. It was 1 ORIENTATION; it is now 1 MILK RUN. More story missions come later.
For now a second dot, 2 TEST MISSION, follows it (Step 204). It is a throwaway test, won 5 game
seconds after time starts. It comes out before the player build: in **Starwolf → Campaign**,
switch TEST MISSION off (Step 205). Do not delete it.

How it plays:
- A painted console fills the screen. On the left is the BRIEFING screen. Under MISSION RECORD it
  shows the picked mission's number and name, and its text. On the right is the OPERATIONAL MAP
  screen: a star chart with the mission dots joined by lines.
- When the map opens, the newest dot is already picked. Click a dot to pick it.
- The MISSION ARCHIVE box on the map has three buttons. BRIEFING shows the mission's briefing
  text. DEBRIEF works only on a finished mission. It shows MISSION COMPLETE above the briefing
  text. LAUNCH shows INITIALISING... and opens the board with the mission's level. On a finished
  mission LAUNCH is greyed out and does nothing: no replays for now (owner 2026-11-11, Step 198).
- A finished mission's dot is a tick picture. The open mission's dot is a plain green dot. It keeps
  one size (Step 176). A new dot, its name and its line fade in the first time they show.
- Escape opens the Escape menu (see Escape menu, below).

The pictures are both in `Art/Briefing/`:
- `OperationalMapConsole.png` (1840 x 1035) is the whole console. It is in the Texture slot of
  `Operational Map Console`, a Raw Image that fills the 1920 x 1080 screen. These are painted
  into it: the BRIEFING and OPERATIONAL MAP plates, STARWOLF / DESTROYER / OPERATIONS, MISSION
  RECORD, CAMPAIGN ARCHIVE, STARWOLF / SECTOR PLOT, the star chart, MISSION ARCHIVE and the three
  empty button frames. To change any of these words, change the picture.
- `CompletedMissionNode.png` (44 x 44) is the tick on a finished dot. It is in **Completed Node**
  on the script. With the slot empty, a finished dot is a dimmer round dot with a ✓ after its name.
- To change the art, replace a PNG with one of the same name. Its import settings stay: sRGB
  (Color Texture) on, Compression None. A new console picture needs its two screens and three
  button frames in the same places. Or move the parts below to match it.

The game writes these over the picture: the mission's number, name and text on the left screen,
the three button words, and the dots, their names and the lines between them. All of it uses
Share Tech Mono.

The parts, and the script slot for each. All are under `Sector Map` in the Hierarchy, except
`SECTOR MAP card` and `ScreenFlow`, which are on their own at the top:
- `Operational Map Console`: the console picture. No slot.
- `SECTOR MAP card`: the typed SECTOR MAP title from Step 167. The console picture draws over it,
  so it does not show. No slot.
- `Map`: the dots and lines are made in here. Slot: **Map**.
- `Dot (copied for each mission)`: switched off. The script copies it for each mission. Its
  picture is Unity's round Knob sprite, coloured by the script. Its `Text` is the mission's number
  and name. Slot: **Dot Template**.
- `Line (copied between dots)`: switched off. The script copies it for each line between two dots.
  It is 3 px high. Slot: **Line Template**.
- `Mission Box`: the box's fill and outline are switched off, so the box draws nothing. Slot:
  **Box**. In it:
  - `Title`: the mission's number and name, on the left screen. Slot: **Box Title**.
  - `Briefing`: the mission's text, on the left screen. Slot: **Box Brief**.
  - `BRIEFING`, `DEBRIEF` and `LAUNCH`: the three buttons, placed over the painted button frames
    on the map. Each has a faint teal fill and a `Text`. **Debrief Button** is `DEBRIEF`.
    **Launch Label** is the `Text` in `LAUNCH`.
- `ScreenFlow`: shows INITIALISING... and loads the level. Slot: **Flow**.

The colours and timings are under Look on the script:
- **Open Ink**: the open dot and its name. **Done Ink**: a finished dot's name, and the dot when
  Completed Node is empty. **Line Ink**: the lines.
- **Pulse Seconds** (1.6) and **Pulse Size** (0.25): how often the open dot grows and shrinks, and
  how much it grows, as a share of its size.
- **Fade In Seconds** (1.5): how long a new dot, its name and its line take to fade in.

The missions are in the Campaign file, `Assets/Campaign/Campaign.asset` (Step 183). The script's
**Campaign** slot has this file in it. The scene has no mission list of its own. To set up a
mission, open **Starwolf → Campaign** (Step 205). It edits this file. The missions are in play
order. Drag a row to move it. + adds a mission, and − removes the picked one. Each mission has an
On switch. A mission switched off gets no dot, and the save slot's count skips it. A
mission's dot shows only after the mission before it is won. Each mission has a name (**Title**), a
**Level** slot, a briefing (**Brief**, the text on the left screen) and an **At**. Drag the level's
scene into Level. The slot keeps the scene's name and adds the scene to the build's scene list. At
is where its dot is, in px from the middle of the 1920 x 1080 screen. Measured on the picture, the map screen runs from x −190 to 770 and from y −455
to 305. The MISSION ARCHIVE box covers x −155 to 125 and y −405 to −120. Keep each dot and its
name inside the screen and off the box.

Ask a technical artist before you change these:
- The buttons' On Click calls: **ShowBriefing**, **ShowDebrief** and **Launch** on Sector Map.
- **Slots Scene** (`SaveSlots`): with no save slot picked, the map opens this scene instead.
- **Map Scene** (`SectorMap`) on Ship Rooms is the scene SECTOR MAP › loads. See Ship rooms, above.

### End card

Scene: `Assets/Scenes/Tactical/EndCard.unity`. Script: **End Card** (Step 194). It shows after a win
of the campaign's last mission, when you press MISSION SELECT. The screen is black. The story's
closing line types in, in the middle, in the sector map's green Share Tech Mono. The whole line
stays up, then the title loads, where only STARWOLF flickers in. Any key or click shows the whole
line at once. Another one goes straight to the title. The end card leaves the campaign. The save
was already written at the win.

The closing line is in the Campaign file: `Assets/Campaign/Campaign.asset` → **Closing Line**.
Type it in **Starwolf → Campaign**, under the missions (Step 205). It stays empty for now (owner
2026-11-11). With the line empty, the end card goes straight on to the title, where only STARWOLF
flickers in, in the middle of the screen (Step 203). Today the last mission is the throwaway TEST
MISSION (Step 204). A campaign win of MILK RUN opens the ship's rooms and the travel bar. A win of
TEST MISSION goes to the end card and the title.

A rule for later (owner 2026-11-11): STARWOLF must never be in the bottom half of the screen. An
end card with a closing line must not put STARWOLF below the line.

Two numbers, on the Board tab of **Starwolf → Tuning**, under End card, with `EndCard.unity` open:
- **Letters Per Second** (30): how fast the line types in.
- **Hold Seconds** (5): how long the whole line stays up before the title.

The save slot: on SAVE SLOTS, a used slot reads "CAMPAIGN · 1 OF 10 · 10%". That is missions won,
out of the missions switched on in the Campaign file (Step 205), and the percent. With every mission won it reads
"CAMPAIGN COMPLETE · 100%". Old test levels don't count.

Ask a technical artist before you change these:
- **End Card Scene** (`EndCard`) on the End Screen Panel is the scene MISSION SELECT loads after
  the last win. If that scene is not in the build's scene list, MISSION SELECT opens the rooms.
- **Campaign** on Save Slots, in `SaveSlots.unity`, is the file the percent is counted from.

### Escape menu

`Assets/UI/Menus/Escape Menu.prefab` (Step 190). On SAVE SLOTS, the SECTOR MAP and the ship's
rooms, Escape opens it. It reads PAUSED, then RESUME, MAIN MENU and QUIT GAME. Escape again, or
RESUME, closes it. MAIN MENU goes back to the title. QUIT GAME quits the game. In the Unity editor
it stops play mode. On the intro, Escape still quits at once.

It has a separate canvas, drawn on top of everything. A copy is in `SaveSlots.unity`,
`SectorMap.unity` and `Ship.unity`, switched off. It looks like the pause menu in a mission: dark
buttons, white Barlow Condensed letters, and an amber tint when the mouse is over a button.

The parts:
- `Dim`: a dark see-through layer over the whole screen. It takes the clicks, so nothing under it
  can be pressed.
- `Box`: the dark box in the middle.
- `Title`: PAUSED.
- `RESUME`, `MAIN MENU` and `QUIT GAME`: the three buttons. Each has its words in a `Label`.

To change its look, open the prefab and change it there. The change shows in all three scenes.
- The box's colour is on the Image of `Box`.
- A button's grey is Normal and Selected, under Colors on its Button. The amber tint is
  Highlighted and Pressed.
- The letters are Barlow Condensed SemiBold, on each `Label` and on `Title`.

Its Art list is on `Escape Menu`, the prefab's top object (Step 191). It also shows in
**Starwolf → Art**.
- **Dim**: empty. The dim layer is a plain dark colour.
- **Box**: `Plates/plate_dark.png`, the board's dark plate. A stand-in.
- **Button**: `Plates/plate_legend.png`, the board's bevelled key, on RESUME, MAIN MENU and QUIT
  GAME. A stand-in.

The box and the buttons are set to Image Type Sliced. They match the pause menu. The box is the
pause menu plate's grey (0.31, 0.34, 0.36). The buttons are the pause menu's grey (0.40, 0.42,
0.44).

`Sector Map` has an Art list (Step 189). It has two empty entries: **Button** (BRIEFING, DEBRIEF
and LAUNCH) and **Mission box**. The Mission box's image is switched off. Switch it on to see a
picture there. Both entries also show in **Starwolf → Art**.

Ask a technical artist before you change these:
- Each button's On Click calls ScreenFlow in the same scene: **Resume**, **MainMenu** or **Quit**.
  Each scene's copy is wired to that scene's `ScreenFlow` object.
- **Escape Menu** on ScreenFlow is the copy Escape opens. Left empty, Escape quits the game at once.
- **Main Menu Scene** (`00_Start`) on ScreenFlow is the scene MAIN MENU loads.

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
  are in `Art/Frame`. The Art list has 5 entries. It is the frame in `02.1_Grid` and in the ship's
  rooms (`Ship.unity`).
- `Frame - Fire Control`: Codex's frame, a prefab variant of `Frame` (2026-11-10). It is the frame
  in the test scene `Fire Control Codex` only. The owner keeps `02.1_Grid` on `Frame` (2026-11-10). Its `Steel frame` shows
  `Art/Frame/FireControlSurround.png`: one 1920 × 1080 picture of a heavy metal casting with pipes.
  It has a see-through hole for the plot, 1712 × 890 at 16:9, at (104, 100). The pipes and bolts are
  in the picture. `Conduits` and `Corner bolts` are switched off. The frame stretches with the board.
  The image is set to Tiled, with Fill Center off. The picture's sprite border is 860 left, 440
  bottom, 860 right and 450 top. The corners stay the same size. Short pieces from the middle of
  each edge repeat. `Black backing` stretches too. The two stencil words are tied to corners:
  BUS A-14 bottom left, VENT PLENUM 3 bottom right. The Art list's Steel frame entry names
  `FireControlSurround.png`. Change the picture there. In Codex's file as delivered, that entry
  named the old steel picture, and the new picture did not show.
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
