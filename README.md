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

There are seven levels. You pick one from the MISSIONS list. The enemy is the
Garuda, a Garuda-class cruiser. In levels 1 and 3 to 5 it does not hunt you
and does not fire. In COLD RUN and INTERCEPT it patrols and hunts you if it
finds you.

1. **FLIGHT DRILL.** Training. Learn to fly the ship, with nothing to fight. A
   note gives you each of 13 steps: power, throttle, heading, the side
   thrusters, zoom, DRADIS, a belt of rocks, and stopping inside a ring.
2. **COLD RUN.** Weapons are off. Sneak past the Garuda along an S-shaped
   map and come within 700 units of the jump point. Hide in gas clouds and
   behind big rocks.
3. **WEAPONS FREE.** Training. Scan the Garuda and destroy it with the railgun.
   It stays still, 1,500 units away.
4. **NO PEEKING.** Training. Hit the Garuda once with no scan and no lock. Use
   POINT and RULER. A scan or a lock loses the level.
5. **FINDERS KEEPERS.** Training. Capture the Garuda. Scan it, open one side,
   stop it, then send the marines. Destroying it loses the level.
6. **INTERCEPT.** COLD RUN's map, with the radio on. Weapons are off. Decode the
   Garuda's radio messages to work out where it is going, keep clear of it and
   reach the jump point.
7. **WOLF HUNT.** The full mission. The Garuda hunts you through a field of
   debris. Find it, then destroy it or capture it. A jump point then
   opens at a far edge of the map. Reach it to win. Once the Garuda finds you, a
   countdown starts and Naga-class frigates come in from the edges.

## Running it

Open the project in **Unity 6000.5.10f1** and load:

```
Assets/Scenes/Tactical/00_Start.unity
```

Press play. The intro runs by itself to a menu. Pick NEW GAME. The MISSIONS list
opens. Pick a level. INITIALISING... shows, then the tactical board. The board
loads your level from `Assets/Scenes/Levels/` on top of itself. To skip straight
to the board, open `02.1_Grid.unity` instead. It plays Wolf Hunt.

The Mac app is built outside the repo, at
`~/Documents/GitHub/Killshot-Builds/Mac/Starwolf.app`.

## Controls

Almost everything is done with the mouse, on the board's switches. The keyboard
does a few things.

| Input | What it does |
|---|---|
| Menu | The arrow keys or `Tab` move. `Enter` or `Space` picks. A click works too. |
| MISSIONS list | The arrow keys move. `Enter` picks. A click works too. |
| `Esc` | In a level, stop the game and open the menu: RESUME, QUIT GAME, RESTART MISSION and MISSION SELECT. `Esc` again, or RESUME, closes it. On the intro and the MISSIONS list, quit at once, with no confirm box. There it does nothing in the Unity editor. On the MISSION COMPLETE or SHIP LOST screen, it does nothing. |
| `P`, or the PAUSE key | Stop or start time. The board opens paused. |
| Rate keys | Run time at x0.25, x1 or x5. |
| Briefing icon, next to the rate keys | Open or close the briefing. |
| Power panel, bus selectors | Click to step a bus between VENT, STANDBY and DRAW. A bus on DRAW gains one unit per game second. A bus on VENT loses one. |
| Helm, throttle fader | Drag to set the throttle. The middle is 0. Above it is ahead, below it is reverse. |
| `W` / `S` | Raise or lower the throttle 10% a press. Hold to repeat. `W` goes ahead. `S` goes down through 0 into reverse. Full reverse is slower than full ahead. |
| `A` / `D` | Step the ordered heading 3° to port or starboard. Hold to repeat. The bow swings round to it. |
| `Q` / `E` | Slide the ship to port or starboard while the key is down. The nose stays still. Let go, and the side thrusters stop the slide. They need ENG power. |
| Scroll wheel over the plot | Zoom in and out. The SCALE knob does the same when dragged. |
| Drag the empty grid, or the arrow keys | Pan the plot. With POINT, RULER or COMPASS picked, a drag draws a mark instead. |
| `C` | Centre the plot on our ship. |
| Left rail | Pick a tool: CURSOR, POINT, RULER, COMPASS, ERASE. |
| Click or drag on the plot | Use the tool you picked. |
| Right rail, or `1` to `6` | Open or close a drawer: 1 fire control, 2 sensors, 3 power, 4 hangar, 5 engineering, 6 comms. |

To change a board key, open the Keys tab of **Starwolf → Tuning** in the Unity editor. The keys do
nothing while you type in a text box, except `Esc`. It still opens the menu.

What each panel's switches do is in `Assets/UI/Board/README.md`.

**Engine power sets your top speed, thrust and turn rate.** One unit on the ENG
bus allows 25 units a second, and five units allow 50. Five units also give
twice the acceleration, braking, turn rate and side thrust of one unit. With no
ENG power the ship has no thrust and no side thrusters, and it still turns, at
the one-unit rate. Put the ENG bus on DRAW first and let it fill. If the ENG bus
drops, the throttle goes back to zero and you set it again. A damaged engine
room turns the ship more slowly and lowers the throttle limit.

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
