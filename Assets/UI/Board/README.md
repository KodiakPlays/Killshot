# The board's panels and art

Each panel of the tactical board is a prefab in `Panels/`. Its art sits in `Art/`. To change a
panel's look, open its prefab and swap sprites in the **Art** list on its script. You don't need
to touch code.

Done so far: the helm and the power, sensors, comms, fire control and hangar drawers. The other panels are still built into the scene
(`Assets/Scenes/Tactical/02.1_Grid.unity`, under `Board/HandoffBoard`), and their art is still in
`Assets/UI2/Claude Design handoff/export/sprites/@2x/`. They move here one at a time.

## Folders

```
Panels/          one prefab per panel (Helm, Power, Sensors, Comms, Fire Control, Hangar)
Art/Helm/        art only the helm uses; the same for each panel (Art/Power, Art/Fire Control …)
Art/Buttons/     push buttons
Art/Lamps/       indicator lamps and their glow
Art/Bolts/       round and slotted bolts
Art/Knobs/       rotary knobs, their pointers and tick marks
Art/Switches/    rocker switches (up, locked, thrown) and their housings
Art/Latches/     drawer latches, latch tracks and sliders
Art/Screens/     screen glass with its bezel
Art/Plates/      label plates
Art/Wells/       recessed wells that controls sit in
Art/Shapes/      plain rounded shapes the code colours (key faces, cells, the dial ring)
```

Art one panel uses lives in that panel's folder. Art more than one panel uses lives in the folder
for its kind.

## Changing a panel's art

1. Open the prefab, for example `Panels/Helm.prefab`.
2. Select the top object (`Helm`). Its script (`Helm Panel`) has an **Art** list.
3. Each entry is one piece of art: a name, a sprite, and every image that shows it. Drop a new
   sprite into the entry, and all of its images change.
4. Save the prefab.

An entry marked "stand-in" is a placeholder shape the code colours. It is waiting for real art.

Art the code swaps in play, like a rocker switch that flips from up to thrown, isn't in the list.
Each of those sprites has its own field on the script (for example **Rocker Up** and **Rocker
Thrown** on the power drawer).

Colours that change in play (lamps lighting up, keys selected) are set by the code. They are in
the script's **Colours** section, not in the art.

## How a panel is laid out

The helm has two states, each its own object:
- `Closed`: the header strip shown when the helm is folded down.
- `Open`: the whole helm.

A drawer, like `Power`, is one object. It slides out from behind the frame. On the board it sits
under `Drawers`, switched off until it opens.

Each control's parts sit under one parent, named after the control. For example:
- Helm `Open/01 Flight/Throttle`: the well, the cap and the lit track.
- Helm `Open/03 Approach/DIRECT`: the row, its lamp and its labels.
- Power `Buses/ENG/Selector`: the knob, its ticks and its pointer.

Order matters: a part lower in the list draws on top. Keep each part's position in the list when
you edit.

Each group is the same size as the panel it sits in (for a drawer, the whole board), so the parts
keep their positions. Don't resize or move a group. Move the parts inside it.

## Fields that are empty in the prefab

Some panels also drive things outside themselves. The sensors drawer fills the signal box in the
frame's top-right and the plot's SENSOR RANGE label. A prefab can't link to objects outside it,
so those fields show as empty when you open the prefab. They are set on the panel's copy in the
board scene. Leave them alone in the prefab.
