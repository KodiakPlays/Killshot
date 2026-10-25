# Starwolf

A top-down tactical space combat game, played entirely through a control panel.

There is no cockpit and no view out of a window. You command a ship from its
instruments: a plotting board showing where things are, panels for power and
weapons, and a set of tools for measuring and marking up the board. Everything
runs in real time, and you can pause.

The tension is that you cannot do everything at once. The reactor makes a fixed
amount of power. Engines, weapons, sensors and the rest all want some of it.
Running the radar wide means less power for the guns. Turning hard costs
stability. Every advantage is bought by giving something up.

## Running it

Open the project in **Unity 6000.5.10f1** and load:

```
Assets/Scenes/Tactical/00_Start.unity
```

Press play. The start screen leads to a briefing, then to the tactical board.
To skip straight to the board, open `02_Grid.unity` instead.

## Controls

| Input | What it does |
|---|---|
| `1` | Draw power to engines. Five bars, about ten seconds to fill. |
| `W` / `S` | Forward and reverse thrust |
| `A` / `D` | Turn left and right |
| Scroll wheel | Zoom, anchored on the cursor |
| Middle-drag or right-drag | Pan the board |
| `C` | Centre the plot on our ship |
| Left rail | Pick a tool: mouse, point, ruler, compass, eraser |
| Left-click / drag on the board | Use the live tool |

**Engine power gates movement.** With no power the ship cannot turn at all and
crawls at 5 units per second. Press `1` first and wait for the bars.

**Clear the board** by pressing the eraser a second time while it is already
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
