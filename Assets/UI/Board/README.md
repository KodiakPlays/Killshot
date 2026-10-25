# The board's panels and art

Each panel of the tactical board is a prefab in `Panels/`. Its art sits in `Art/`. To change a
panel's look, open its prefab and swap sprites in the **Art** list on its script. You don't need
to touch code.

Done so far: the helm and the power drawer. The other panels are still built into the scene
(`Assets/Scenes/Tactical/02.1_Grid.unity`, under `Board/HandoffBoard`), and their art is still in
`Assets/UI2/Claude Design handoff/export/sprites/@2x/`. They move here one at a time.

## Folders

```
Panels/          one prefab per panel (Helm.prefab, Power.prefab)
Art/Helm/        art only the helm uses
Art/Power/       art only the power drawer uses
Art/Shared/      art several panels use: lamps, bolts, label plates, screens, latches
Art/Shared/Shapes/  plain rounded shapes the code colours (key faces, stability cells, the dial ring)
```

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

Each group covers the whole board (1920 × 1080, top-left corner) so that the parts keep their
positions. Don't resize or move a group. Move the parts inside it.
