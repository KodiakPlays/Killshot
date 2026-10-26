# The board's UI

## Summary

The tactical board is the game's main screen. It is made of panels and frame pieces.

The panels are the plot, the helm, the orders station, the periscope and six drawers (power,
sensors, comms, fire control, hangar and engineering). The frame pieces are the metal frame and
the smaller pieces on it: two rails of keys, the time control, the console, the alarm strip and
the signal box.

Each panel and each frame piece is a prefab file in `Panels/`. You can open a prefab by itself
and change its art. You don't need to touch code.

The art files are in `Art/`.

The shared colour palette and the fonts are not done yet. See [Not done yet](#not-done-yet).

## Overview

### Panels and scripts

There is one prefab per panel or frame piece. The top object of each panel has the panel's
script. For example, the script **Power Panel** is on the object `Power`. The script shows the
ship's state on the panel and passes clicks to the game.

The panel scripts are in `Assets/Scripts/UI/Panels/`. The periscope and plot scripts are in
`Assets/Scripts/Tactical/`. The frame pieces have no panel script. The board script runs them
(see [Frame pieces](#frame-pieces)).

### The Art list

Each panel script has an **Art** list. Each frame piece has the same list, on its **Art List**
script. Each entry is one piece of art: a name, a sprite, and every image that shows it. Change
the sprite in an entry, and all of its images change.

### Art folders

Art that only one panel or frame piece uses is in a folder named after it. Art that more than one uses is
in a folder for its kind.

```
Panels/            one prefab per panel or frame piece
Art/Helm/          art only the helm uses; the same for each panel (Art/Power, Art/Fire Control …)
Art/Frame/         the metal frame: the steel texture and the conduits
Art/ToolRail/      the tool rail's icons and its selected-key frame
Art/SystemsRail/   the systems rail's icons
Art/TimeControl/   the time control's plate, pause key and rate keys
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
```

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

The board is in the scene `Assets/Scenes/Tactical/02.1_Grid.unity`, under `Board/HandoffBoard`.
- The helm is `HandoffBoard/Helm`. It has two states. `Closed` is the header strip shown when the
  helm is folded down. `Open` is the whole helm.
- The orders station is `HandoffBoard/Orders`. Like the helm, it has `Closed` and `Open`.
- The plot is `HandoffBoard/Plot`.
- The periscope is `HandoffBoard/Periscope`. It is housed until you lower it. Its LOWERED label,
  `HandoffBoard/State · periscope`, is outside the prefab.
- The drawers are under `HandoffBoard/Drawers`. A drawer slides out from behind the metal frame.
  It is switched off until it opens.
- The frame pieces are under `HandoffBoard`: `Frame`, `ToolRail`, `SystemsRail`, `TimeControl`,
  `Console`, `AlarmStrip` and `SignalBox`.

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
  outside the Art list. The rockers also slide in play. The script for that is **Handoff
  Rockers**, on `HandoffBoard` in the scene. It uses the rocker sprites too.
- Colours that change in play, such as lamps lighting up. The code sets them from the script's
  **Colours** section. A colour you set on the image is replaced in play.
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

Ask a technical artist before you change these:
- The code moves the throttle's `Cap` and stretches `STAND-IN Lit track`. **Thr Well** is the
  well's place in board pixels (652, 941, 44 × 78). **Cap Base**, **Cap Span**, **Cap Height**,
  **Track Base** and **Track Span** place the cap and the track in the well. Dragging the
  throttle also reads **Thr Well**.
- The code turns the heading ring (**Hdg Ring**).
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

`Panels/Periscope.prefab`. Script: **Handoff Periscope Panel**, on `Periscope`. The periscope
looks along one bearing. You slew it, zoom it with the MAG knob, and classify what is in the
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
- `Sight`: `Aperture`, `Mask` with the sight picture, `Vignette`, the target `Brackets`, four
  ticks and `Target name`
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
  **Scope Lowered Px** (656), on **Handoff Board Motion** on `HandoffBoard`.
- The code draws the sight picture every frame, on `Sight/Mask/Sight (drawScope)`. Its colours
  are fields on that part's script.
- The code moves the target brackets sideways and hides them when no target is near:
  **Brackets**, **Bracket Home X** (887.2), **Sight Centre X** (960), **Px Per Deg**,
  **Max Shift** and **Visible Deg**.
- The code stretches `Convergence/Bar` from the left: **Conv Bar**, **Conv Left** (777),
  **Conv Width** (366) and **Parent Left** (750, the periscope's left edge).
- The code turns the MAG knob's pointer (**Mag Pointer**).
- The code sets the colours of the off-axis readout, the SLEW label, and the SLEW and CLASS lamps.

### Plot

`Panels/Plot.prefab`. Script: **Handoff Plot Panel**, on `Plot`. The plot is the map in the
middle of the board. It shows our ship, the contacts, the sensor and weapon rings, and the marks
made with the plot tools.

Most of what the plot shows is drawn by code while the game runs. Parts named `… (drawn)` are
drawn this way.

The plot is 1712 × 890 px. Its top-left corner is at board x 104, y 100. Its parts' positions
count from that corner.

The prefab has these parts, from back to front:
- `Ground`, `Grid` and `Glow`: the plot's background, the grid and its glow
- `Scale`: the axis numbers, made by code
- `Design mock-up (off)`: the design's sample picture, 46 parts, all switched off. Three of
  them are still used:
  - `Passive label template`: the plot copies it for each passive bearing label
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

Its art files are in `Art/Plot`, with `Art/Shapes/round_6` for the ground. The Art list has 5
entries: the ground, glow, fighter wedge, scanlines and vignette. The fighter wedge is
`Assets/Art/Hulls/hull_wedge_filled.png`.

Ask a technical artist before you change these:
- The code copies four templates in play: `Passive label template`, `Fighter (template)`,
  `Contact label (template)` and `Mark label (template)`. Keep them switched off.
- Two fields place everything the code draws: **Plot Centre** (960, 545, in board pixels) and
  **Px Per 1000 Km** (60.9). Moving or resizing the plot means changing them.
- The code sizes our ship's picture: **Own Ship Px At Full Zoom**, **Own Ship Min Px** and
  **Own Ship Max Px**.
- The code sizes and spaces the fighters: **Fighter Px** and **Fighter Gap Px**.
- The code makes the axis numbers under `Scale`: **Axis Font**, **Axis Material**,
  **Axis Font Size** and **Axis Colour**.
- The code places labels with **Point Label Offset**, **Ruler Label Offset**,
  **Ring Label Offset** and **Contact Label Offset**.
- The code sets the colours of our ship (**Own Ship Color**) and wrecks (**Wreck Ink**). The
  **Colours** section sets the colours of contacts, rings, the sweep and marks.

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
  `PDC` and `MISSILE` labels are outside the selector's group. They draw over the mount knob.
- `02 Firing solution`: `Track button`, `Range knob`, `Bearing knob`, `Speed knob` and the
  tracking ID
- `03 Capacitor`: `Cells`, a lamp and the charge readouts
- `04 Arming sequence`: `Housings`, and the `LOAD`, `ANGLE`, `BEARING` and `ARM` rockers
- `Fire lever`: `Slot`, `Lit track` and `Cap`

The PDC and missile controls are not drawn yet.

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
- `03 Launch order`: `Ship selector`, `Bearing` (a text field for the launch bearing) and
  `Command selector`
- `04 Launch sequence`: `Housings`, the six stage rockers (`CREW`, `FUEL`, `ORDNANCE`, `EVAC`,
  `DOORS`, `CATAPULT`) and `Telemetry`
- `Launch lever`
- `Recall`: the RECALL button

Its art files are in `Art/Hangar` and the folders for each kind. The Art list has 19 entries.
Two are stand-ins: the selection bezel and the knob cap.

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
- The bearing box's two events call **Hangar Panel**. Keep those links.
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

## Frame pieces

The frame pieces are the metal frame and the smaller pieces on it. Each one is a prefab in
`Panels/`, under `HandoffBoard` on the board. Each one has the **Art List** script on its top
object. It has no panel script.

The board script, **Handoff Board Controls** on `HandoffBoard`, runs the frame pieces. Its
fields for them are on `HandoffBoard` in the scene, outside the prefabs. Its colours for them
are there too.

The prefabs:
- `Frame`: the metal frame. Its parts are `Steel frame`, `Black backing`, `Conduits`,
  `Corner bolts` and `Stencils` (the four painted labels). The art files are in `Art/Frame`. The
  Art list has 5 entries.
- `ToolRail`: the keys for the plot tools, CURSOR, POINT, RULER, COMPASS and ERASE, with their
  lamps, plates, icons and labels. The art files are in `Art/ToolRail` and `Art/Rails`. The Art
  list has 13 entries.
- `SystemsRail`: one key per drawer, FIRE, SENS, PWR, HGR, ENGR and COMM. The art files are in
  `Art/SystemsRail` and `Art/Rails`. The Art list has 13 entries.
- `TimeControl`: `Plate`, `Clock`, `Pause`, `Rate keys` and `Rate caption`. The art files are in
  `Art/TimeControl`. The Art list has 4 entries.
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
- The pause key and the four rate keys are not in the Art list. The board script swaps their
  sprites: **Pause Key Run**, **Pause Key Paused**, **Rate Key On** and **Rate Key Off**.
- The board script moves the tool rail's `Selected frame` and `Selected bar` to the chosen tool
  key (**Selected Frame**, **Selected Bar**, **Tool Keys**).
- The board script turns the console's two pointers: **Scale Dial** and **Tint Dial**.
- The board script shows and hides the alarm strip (**Alarm Parts**). It writes its words and
  count.
- The sensors drawer fills the signal box.
- The playtest finds `AlarmStrip/Words`, `SignalBox/Signal · percent` and
  `SignalBox/Signal · note` by name. The board script and the playtest find some frame pieces by name too. Don't rename
  them.
- The board script sets the colours of the rail lamps (**Tool Lamp On**, **Tool Lamp Off**),
  the pause label and lamp, the rate key labels, and the tint lamps and labels.

## Not done yet

- The shared colour palette. Each panel's colours are still fields in its script's **Colours**
  section. The frame pieces' colours are fields on the board script. The plan is one palette
  asset that every panel uses.
- The fonts. They are still in `Assets/UI2/Generated/Fonts`.
- The small 1x art copies. Only the @2x art the board uses has moved. The 1x copies are still in
  `Assets/UI2/Claude Design handoff/export/sprites/`.
