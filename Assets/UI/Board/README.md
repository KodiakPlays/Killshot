# The board's UI

## Summary

The tactical board is the game's main screen. Its controls are in panels: the helm, the orders
station and six drawers (power, sensors, comms, fire control, hangar and engineering).

Each of these panels is a prefab file in `Panels/`. You can open a prefab by itself and change
its art. You don't need to touch code.

The art files for these panels are in `Art/`.

The rest of the board is not a prefab yet. See [Not done yet](#not-done-yet).

## Overview

**Panels.** There is one prefab per panel. The top object of each prefab has the panel's script.
For example, the script **Power Panel** is on the object `Power`. The script shows the ship's
state on the panel and passes clicks to the game. The scripts are in `Assets/Scripts/UI/Panels/`.

**The Art list.** Each panel script has an **Art** list. Each entry is one piece of art: a name,
a sprite, and every image that shows it. Change the sprite in an entry, and all of its images
change.

**Art folders.** Art that only one panel uses is in that panel's folder. Art that more than one
panel uses is in a folder for its kind.

```
Panels/          one prefab per panel
Art/Helm/        art only the helm uses; the same for each panel (Art/Power, Art/Fire Control …)
Art/Buttons/     push buttons
Art/Lamps/       indicator lamps and their glow
Art/Bolts/       round and slotted bolts
Art/Knobs/       rotary knobs, their pointers and tick marks
Art/Switches/    rocker switches (up, locked, thrown) and their housings
Art/Latches/     drawer latches, latch tracks and sliders
Art/Screens/     screen glass with its bezel
Art/Plates/      label plates
Art/Wells/       recessed wells for controls
Art/Shapes/      plain rounded shapes the code colours (key faces, cells, the dial ring)
```

**Groups.** Each control's parts are under one object named after the control. For example,
the helm's `Open/01 Flight/Throttle` has the throttle's well, cap and lit track.

**Draw order.** A part lower in the Hierarchy draws on top of the parts above it.

**Board pixels.** The board is 1920 × 1080 pixels. Every panel and every group is the size of
the whole board. A part's Rect Transform gives its place on the board:
- Pos X is the board x of the part's centre.
- Pos Y is the board y of the part's centre, as a minus number.

Some script fields are positions in board pixels too. In those, y counts down from the top edge,
without the minus.

**On the board.** The board is in the scene `Assets/Scenes/Tactical/02.1_Grid.unity`, under
`Board/HandoffBoard`.
- The helm is `HandoffBoard/Helm`. It has two states. `Closed` is the header strip shown when
  the helm is folded down. `Open` is the whole helm.
- The orders station is `HandoffBoard/Orders`. Like the helm, it has `Closed` and `Open`.
- The drawers are under `HandoffBoard/Drawers`. A drawer slides out from behind the metal frame.
  It is switched off until it opens.

## How to change a panel's art

1. Double-click the prefab to open it, for example `Panels/Power.prefab`.
2. Select the top object (`Power`).
3. On its script (**Power Panel**), open the **Art** list.
4. Find the entry, for example **Bolt**. Drop the new sprite into its **Sprite** field. Every
   image in its **Images** list changes.
5. Save the prefab.

Put a new art file in the panel's folder. If more than one panel uses it, put it in the folder
for its kind.

To give one image a different sprite, take the image out of its entry's **Images** list. Then add
a new entry with that image and the new sprite.

An entry marked "(stand-in)" uses a placeholder shape. It is waiting for real art. In the
Hierarchy, stand-in parts have names that start with `STAND-IN`.

## Rules for everyone

- **Edit the prefab file.** A change made to the copy on the board stays on that copy only.
- **Don't move or resize groups.** Every group is the size of the board. Move the parts inside it.
- **Keep each part in its place in the list.** A part lower in the list draws on top.
- **Keep `Block · body` as the first part.** It is an invisible image at the top of each drawer,
  the helm's `Open`, and the orders station's `Closed` and `Open`. It stops clicks going
  through the panel to the plot.
- **Change a listed image's sprite through the Art list only.** If you set it on the image by
  hand, the Art list puts the listed sprite back the next time anything on the script changes.
- **Don't rename parts or art files.** The playtest finds some of them by name.
- **Some fields show empty in a prefab.**
  - The sensors drawer links to two things outside it: the signal box in the frame's top-right,
    and the plot's SENSOR RANGE label. A prefab can't link to objects outside it. Those 17
    fields are empty in the prefab file. They are set on the board's copy in the scene. Leave
    them empty in the prefab.
  - Some lamp glow fields are empty on purpose. The design gives those lamps no glow: two band
    lamps in comms, four lamps in fire control, and squadron 2's selection lamp in the hangar.
- **The code switches some parts on and off in play,** such as lamp glows and unused contact
  rows. The prefab shows them as they were when it was saved.

## What you can change

**Safe to change, in every panel:**
- sprites, through the Art list
- the words on fixed labels, such as titles and captions (the code writes over readouts, such as
  numbers and states)
- the position of parts the code does not move (each panel's section below names the ones it
  moves)

**Ask a technical artist first:**
- **Parts the code moves, turns or resizes.** Script fields set where they go, mostly in board
  pixels. If you move one of these parts, those fields must change too.
- **Knob pointers.** The code turns each pointer around its knob. Each dial field on the script
  has a **Centre** (the knob's centre) and a **Home Position** (the pointer's start). If you move
  a knob, these must change too.
- **Sprites the code swaps,** such as rocker switches. These sprites are fields on the script,
  outside the Art list. The rockers also slide in play. The script for that is **Handoff
  Rockers**, on `HandoffBoard` in the scene. It uses the rocker sprites too.
- **Colours that change in play,** such as lamps lighting up. The code sets them from the
  script's **Colours** section. A colour you set on the image is replaced in play.
- **Links to objects outside the prefab.** Only the sensors drawer has these.

## Panel by panel

### Helm

`Panels/Helm.prefab`. Script: **Helm Panel**, on `Helm`. The helm sets the ship's throttle,
heading and autopilot.

In it:
- `Closed`: the folded-down header strip, with heading, turn, throttle, G load and autopilot state
- `Open`: the whole helm
  - `Header`: the same readouts as `Closed`
  - `01 Flight`: `Throttle` and `Heading dial`
  - `02 Attitude hold`: four keys, `NOSE`, `STBD`, `PORT` and `TAIL`
  - `03 Approach`: `Range down`, `Range up` and three mode rows, `DIRECT`, `FLANK` and `EVASIVE`
  - `04 Stability`: the nine stability cells
  - `Plates` and `Bolts`: the helm's body

Art: `Art/Helm`, plus the folders for each kind. The Art list has 20 entries. Two are stand-ins:
the throttle's lit track and the heading ring.

Ask a technical artist first:
- **Throttle.** The code moves `Cap` and stretches `STAND-IN Lit track`. **Thr Well** is the
  well's place in board pixels (652, 941, 44 × 78). **Cap Base**, **Cap Span**, **Cap Height**,
  **Track Base** and **Track Span** place the cap and the track in the well. Dragging the
  throttle also reads **Thr Well**.
- **Heading ring.** The code turns it (**Hdg Ring**).
- **Header readouts,** in `Closed` and `Open`. The code lines up `Turn`, `THR caption`,
  `THR value`, `THR percent sign` and `G load` from left to right. The row starts at the left
  edge of `Turn`. **Gap To Thr**, **Gap To Value** and **Gap To G** set the gaps. Moving the
  others sideways has no effect.
- **Colours set by the code:** lamps, the attitude keys, the range keys, the mode rows, the
  stability cells, and the turn and G load text.

### Orders

`Panels/Orders.prefab`. Script: **Orders Panel**, on `Orders`. The orders station sets
the alert condition and gives damage control orders. Each order needs AUX charge.

In it:
- `Closed`: the shut station, with its header and the alert selector
- `Open`: the whole station
  - `Header`: the title plate, latch track and title
  - `Alert`: the alert `Selector` (C3, C2, C1), its screen, and the condition and release lines
  - `AUX pool`: ten charge cells, the room the orders act on, the charge and the feed rate
  - `Order switches`: `Housings` and six switches, `PARTY`, `SEAL`, `VENT`, `FIRE`, `BLKHD` and
    `SCRUB`. `FIRE`, `BLKHD` and `SCRUB` are blank plates.
  - `Body plate` and `Bolts`: the station's body
- `Header click area`, in `Closed` and `Open`: clicking it opens and shuts the station

Art: `Art/Orders`, plus the folders for each kind. The Art list has 17 entries.

Ask a technical artist first:
- **Order rockers.** The code swaps the sprites of `PARTY`, `SEAL` and `VENT`: **Rocker Up**,
  **Rocker Locked** and **Rocker Thrown**. The blank plates don't change.
- **Alert selectors.** The code turns the pointer in `Closed` and in `Open`: **Stowed** →
  **Pointer** and **Open** → **Pointer**.
- **AUX cells.** The code shows as many cells as the charge fills (**Aux Cells**).
- **Colours set by the code:** the C3, C2 and C1 lamps and labels, the condition and release
  lines, the charge readout, the order lamps and the note.

### Power

`Panels/Power.prefab`. Script: **Power Panel**, on `Power`. The power drawer shows the reactor
and its four power buses.

In it:
- `Frame`: the casting, bolts and hinge
- `Header`: the strip with the latch, title and load readout
- `Reactor`: the ten unit cells, the five core lamps and their captions
- `Load shed`: the emergency load shed rocker, its lamp and its note
- `Buses`: one group per bus, `ENG`, `WEP`, `SEN` and `AUX`, each with `Cells` and `Selector`

Art: `Art/Power`, plus the folders for each kind. The Art list has 20 entries.

Ask a technical artist first:
- **Load shed rocker.** The code swaps its sprite: **Rocker Up** and **Rocker Thrown**.
- **Bus selectors.** The code turns each pointer: **Columns** → **Selector**.
- **Colours set by the code:** the load readout, unit cells, core lamps, the shed lamp and note,
  and each bus's cells, lamp and mode text.

### Sensors

`Panels/Sensors.prefab`. Script: **Sensors Panel**, on `Sensors`. The sensors drawer controls
DRADIS and the active scan.

In it:
- `Frame`, and `Header` with the latch, title and range readout
- `Masters`: the `DRADIS` and `ACTIVE SCAN` master rockers
- `E-SIG`: our ship's signal, with ten cells, a lamp, a percent and a note
- `01 Sensor array`: the `Gain` fader, and `Bands` with `THERM`, `RAD`, `UV` and `EM`
- `02 Active scan`: `Cone dial`, `Narrow`, `Widen`, and the bearing and arc readouts
- `03 Contact board`: `Row 1` to `Row 3`

Art: `Art/Sensors`, plus the folders for each kind. The Art list has 19 entries. Two are
stand-ins: the gain's lit track and the cone dial's face.

Ask a technical artist first:
- **Master rockers.** The code swaps their sprites: **Rocker Up** and **Rocker Thrown**.
- **Gain fader.** The code moves the cap (**Gain Cap**) and stretches the lit track
  (**Gain Track**). The numbers are in board pixels: **Well Top** (375.5), **Well Height** (136),
  **Track Bottom** (502.5), **Cap Top At Five**, **Step Px** and **Cap Half**. Dragging the gain
  also reads **Well Top**, **Well Height** and **Well Stop**.
- **Cone dial.** The code turns the needle (**Needle**) and redraws the cone (**Cone**).
  Dragging the cone reads **Dial Centre** (1505, 609).
- **Links outside the prefab.** The signal box: **Box Pct**, **Box Note**, **Box Cells**,
  **Box Lamp** and **Box Glow**. The plot's SENSOR RANGE label: **Envelope Label** and
  **Envelope Range**. The code also moves that label up and down with the gain
  (**Envelope Label Home Gain**, **Envelope Label Px Per Gain**).
- **Colours set by the code:** lamps, signal cells, band rows, contact rows and most readouts.

### Comms

`Panels/Comms.prefab`. Script: **Comms Panel**, on `Comms`. The comms drawer tunes the radio and
decrypts messages.

In it:
- `Frame`, and `Header` with the latch, title and state
- `01 Receiver`:
  - `Band selector`
  - `Scope`: the round receiver scope, with its rim, glass, tuning knob and 56 bars
  - `Tuned`: the frequency
  - `Lock`: the LOCK rocker and its lamps
- `02 Decrypt`: `Message` (59 letters), `Key dial 1` to `Key dial 4`, and the solved readout

Art: `Art/Comms`, plus the folders for each kind. The Art list has 20 entries. Three are
stand-ins: the band selector's face and knob, and the scope's rim.

Ask a technical artist first:
- **Lock rocker.** The code swaps its sprite: **Rocker Up**, **Rocker Locked** and
  **Rocker Thrown**.
- **Knob pointers.** The code turns **Band Pointer**, **Tune Indicator**, **Ring Marker**, and
  each key dial's **Pointer**.
- **Scope bars.** The code places, sizes and turns all 56 bars around **Scope Centre**
  (1590, 422.5). **Rim Radius**, **Bar Width**, **Bar Base** and **Bar Span** set their size.
  Turning the tuning knob also reads **Scope Centre**. Keep the bars in their order inside
  `Scope`. Some of them draw over the tuning knob.
- **Message letters.** The code writes each letter and sets its colour.
- **Colours set by the code:** band lamps, the LOCK and SIG lamps, scope bars, letters, and the
  key dials' letters and boxes.

### Fire Control

`Panels/Fire Control.prefab`. Script: **Fire Control Panel**, on `Fire Control`. The fire
control drawer aims and fires the ship's weapons.

In it:
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

Art: `Art/Fire Control`, plus the folders for each kind. The Art list has 19 entries.

Ask a technical artist first:
- **Arming rockers.** The code swaps their sprites: **Rocker Up**, **Rocker Locked** and
  **Rocker Thrown**.
- **Knob pointers.** The code turns **System Selector**, **Mount Selector**, **Rng Knob**,
  **Brg Knob** and **Spd Knob**.
- **TRACK button.** The code moves the button (**Track Key**) and its lamp down by
  **Track Press Px** while it is pressed.
- **Capacitor mark.** The code moves the shot-cost mark (**Cap Mark**) to the edge of a cell in
  **Cap Cells**.
- **Fire lever.** The code places `Cap` and stretches `Lit track`, measured from `Slot`:
  **Lever Slot**, **Lever Cap**, **Lever Fill**, **Cap Left Pct**, **Cap Travel Pct**,
  **Lever Travel Px**, **Slot Margin** and **Cap Margin**.
- **Colours set by the code:** lamps, selector labels, readouts, capacitor cells and the
  rockers' hints.

### Hangar

`Panels/Hangar.prefab`. Script: **Hangar Panel**, on `Hangar`. The hangar drawer launches and
recalls squadrons.

In it:
- `Frame`, and `Header` with the latch, title and ready readout
- `01 Bay status`: `Doors selector`, `Doors`, `Pressure`, `Catapult` and `Cat charge`
- `02 Squadrons`: `Squadron 1`, `Squadron 2` and the selection bezel
- `03 Launch order`: `Ship selector`, `Bearing` (a text field for the launch bearing) and
  `Command selector`
- `04 Launch sequence`: `Housings`, the six stage rockers (`CREW`, `FUEL`, `ORDNANCE`, `EVAC`,
  `DOORS`, `CATAPULT`) and `Telemetry`
- `Launch lever`
- `Recall`: the RECALL button

Art: `Art/Hangar`, plus the folders for each kind. The Art list has 19 entries. Two are
stand-ins: the selection bezel and the knob cap.

Ask a technical artist first:
- **Stage rockers.** The code swaps their sprites: **Rocker Up**, **Rocker Locked** and
  **Rocker Thrown**.
- **RECALL button.** The code swaps **Recall Up** and **Recall Pressed**, and fades
  **Recall Overlay** between them.
- **Header screen.** It grows with its readout: **Header Screen**, **Header Left** (1522.5),
  **Header Min Width** (106.5), **Header Pad** and **Header Margin**.
- **Selection bezel.** The code moves it between the squadron rows: **Bezel** and
  **Squad Pitch** (87). Keep it above both squadron groups in the list. It draws under both
  rows.
- **Knob pointers.** The code turns **Door Selector**, **Ship Selector** and
  **Command Selector**.
- **Bars.** The code sizes each bar inside its well: the catapult charge (**Cat Fill**,
  **Cat Well**), the six stage bars (**Stages** → **Fill**, **Well**) and each craft's fuel
  (**Squad Rows** → **Fuel**, **Cells**).
- **Bearing box.** Its two events call **Hangar Panel**. Keep those links.
- **Launch lever.** It works like the fire lever: **Lever Slot**, **Lever Cap**, **Lever Fill**,
  **Cap Left Pct**, **Cap Travel Pct**, **Lever Travel Px**, **Slot Margin** and **Cap Margin**.
- **Colours set by the code:** lamps, labels, the craft tags, bars and readouts.

### Engineering

`Panels/Engineering.prefab`. Script: **Engineering Panel**, on `Engineering`. The engineering
drawer shows damage to the ship and orders repairs.

In it:
- `Frame`, and `Header` with the latch and title
- `Compartments`: the screen, `Ship picture` (our ship seen from above), and one group per room:
  `MAG`, `BRI`, `HAB`, `HGR`, `RCT` and `ENG`. `Old row highlight (off)` is switched off. Leave
  it off.
- `Status`: the hazard, degraded and party lines
- `Acting on`: the `Selector` knob, a lamp and the room name
- `Repairs`: `Housings`, and six rockers: `REBOOT`, `REPAIR`, `SEAL`, `VENT`, `RECALL` and
  `SPARE`

Art: `Art/Engineering`, plus the folders for each kind. The Art list has 17 entries. One is a
stand-in: the ring face.

The four pictures of our ship are outside the Art list. They are textures in
`Assets/Art/Board/Hull`: `hull_prow`, `hull_stbd`, `hull_aft` and `hull_port`. Each one is set on
a Raw Image under `Ship picture/Hull`.

Ask a technical artist first:
- **Repair rockers.** The code swaps their sprites. Each **Repairs** entry has an **Up**, a
  **Locked** and a **Thrown** sprite.
- **Ship picture.** The code sizes the four pictures (**Hull Sides**, **Diagram Ship Px**,
  **Picture Fill**). It colours each one by its side's armour: white, amber or red.
- **Room rings.** The code draws each room's five ring segments: **Rows** → **Ring**, with
  **Segment Deg**, **Gap Deg** and **From Deg**.
- **Degraded line.** The code sizes it and places it in the status row: **Row Left** (1474),
  **Row Right** (1782), **Row Gap** and **Degraded Max**.
- **Knob pointer.** The code turns **Acting On**.
- **Colours set by the code:** the room rows and rings, the status lines, lamps and rocker
  labels.

## Not done yet

- **The periscope and plot.** They are still built into the scene, under `Board/HandoffBoard`
  (`Periscope`, `State · periscope` and `Plot`). They become prefabs one at a time.
- **The frame pieces.** The tool rails (`ToolRail`, `SystemsRail`), the time control
  (`TimeControl`), the console (`Console`), the alarm strip (`AlarmStrip`) and the signal box
  (`SignalBox`) are in the scene too, under `HandoffBoard`.
- **Their art.** Art that only these parts use is still in
  `Assets/UI2/Claude Design handoff/export/sprites/@2x/`.
- **The shared colour palette.** Each panel's colours are still fields in its script's
  **Colours** section. The plan is one palette asset that every panel uses.
- **The fonts.** They are still in `Assets/UI2/Generated/Fonts`.
- **The small 1x art copies.** Only the @2x art the board uses has moved. The 1x copies are
  still in `Assets/UI2/Claude Design handoff/export/sprites/`.
