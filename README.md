# Starwolf

A top-down tactical space combat game, played entirely through a control panel.

There is no cockpit and no view out of a window. You command a ship from its
instruments: a plotting board showing where things are, panels for power and
weapons, and a set of tools for measuring and marking up the board. Everything
runs in real time, and you can pause.

The tension is that you cannot do everything at once. The reactor makes ten
units of power. Four buses share it: engines (ENG), weapons (WEP), sensors (SEN)
and auxiliary (AUX). A bus takes at most five units, so you can fully run only
two. More power to DRADIS means less power for the guns. Every unit you draw
makes the ship louder. Every advantage is bought by giving something up.

The first mission is **Wolf Hunt**. A Garuda-class cruiser hunts you through a
field of debris. Find it, then destroy it or capture it. A jump point then opens
at a far edge of the map. Reach it to win. Once the Garuda finds you, a
countdown starts and Naga-class frigates come in from the edges.

## Running it

Open the project in **Unity 6000.5.10f1** and load:

```
Assets/Scenes/Tactical/00_Start.unity
```

Press play. The intro runs by itself to a menu. Pick NEW GAME. Two title cards
show, MISSION ONE and then WOLF HUNT. Then INITIALISING... and the tactical
board. The board loads the level, `Assets/Scenes/Levels/Level_01.unity`, on top
of itself. To skip straight to the board, open `02.1_Grid.unity` instead.

The Mac app is built outside the repo, at
`~/Documents/GitHub/Killshot-Builds/Mac/Starwolf.app`.

## Controls

Almost everything is done with the mouse, on the board's switches. The keyboard
does a few things.

| Input | What it does |
|---|---|
| `Enter` | Skip ahead on the start screens. On the menu, the arrow keys or `Tab` move and `Enter` or `Space` picks. |
| `P`, or the PAUSE key | Stop or start time. The board opens paused. |
| Rate keys | Run time at x0.25, x1, x5 or x10. |
| Power panel, bus selectors | Click to step a bus between VENT, STANDBY and DRAW. A bus on DRAW gains one unit per game second. A bus on VENT loses one. |
| Helm, throttle fader | Drag to set the throttle. |
| `W` / `S` | Raise or lower the throttle 10% a press. Hold to repeat. |
| `A` / `D` | Step the ordered heading 3° to port or starboard. Hold to repeat. The bow swings round to it. |
| Scroll wheel over the plot | Zoom in and out. The SCALE knob does the same when dragged. |
| Drag the empty grid, or the arrow keys | Pan the plot. With POINT, RULER or COMPASS picked, a drag draws a mark instead. |
| `C` | Centre the plot on our ship. |
| Left rail | Pick a tool: CURSOR, POINT, RULER, COMPASS, ERASE. |
| Click or drag on the plot | Use the tool you picked. |
| Right rail, or `1` to `6` | Open or close a drawer: 1 fire control, 2 sensors, 3 power, 4 hangar, 5 engineering, 6 comms. |

To change a key, open **Starwolf → Keys** in the Unity editor. The keys do
nothing while you type in a text box.

What each panel's switches do is in `Assets/UI/Board/README.md`.

**Engine power sets your top throttle.** Each unit on the ENG bus allows 20%
throttle, so five units allow 100%. With no ENG power the ship has no thrust.
Put the ENG bus on DRAW first and let it fill. If the ENG bus drops, the
throttle goes back to zero and you set it again. Turning does not need power. A
damaged engine room turns the ship more slowly and lowers the throttle limit.

**Stability** is shown on the helm. Throttle lowers it. The FLANK and EVASIVE
approach modes lower it more. Nothing else in the game reads it so far.

**Clear the board** by pressing ERASE a second time while it is already
selected.

## Where to read next

| You want | Read |
|---|---|
| What is built, what is left, and every trap hit | `Assets/Scenes/Tactical/BUILD_LOG.md` |
| Which document answers your question | `Assets/CONTEXT_INDEX.md` |
| How the game is meant to work | `Assets/TACTICAL_BOARD_SPEC.md` |
| Rules for working in this repo | `CLAUDE.md` |

## A note on the sources

`References/` holds the original design document, a playable HTML prototype, and
a design handoff. They are a **timeline, not rivals**: the GDD says what the game
is and why, the prototype shows how systems behave, and the newest material wins
on specifics. See `CLAUDE.md` for the full rule.
