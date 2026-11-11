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

The game will be about ten story missions, plus SKIRMISH. NEW GAME starts the
story. Mission 1, ORIENTATION, is built. It is the first dot on the SECTOR MAP.
For now a second dot, TEST MISSION, is a throwaway test. For the player build
it is switched off in **Starwolf → Campaign**. That window sets up the
campaign's missions in the Unity editor.
It starts with a combat briefing. Your battle group is with you: INDRA, VARUNA
and MARUT. Then you run a checklist of 16 steps: power, throttle, heading, the
side thrusters, zoom, DRADIS, ACTIVE SCAN, scanning one of the group, a field
of rocks, and stopping inside a ring. Each part you finish opens more of the
map. Weapons are off.

SKIRMISH plays WOLF HUNT (7, below) straight away.

The other old test levels below are off every list. Their scenes are kept and
switched off. They are never deleted. What each one tested goes into the story
missions. The enemy is the Garuda, a Garuda-class cruiser. In levels 1 and 3
to 5 it does not hunt you and does not fire. In COLD RUN and INTERCEPT it
patrols and hunts you if it finds you.

1. **FLIGHT DRILL.** Training. Learn to fly the ship, with nothing to fight. A
   note gives you each of 14 steps: power, throttle, heading, the side
   thrusters, zoom, DRADIS, ACTIVE SCAN, a belt of rocks, and stopping inside
   a ring.
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
8. **LONG SHOT.** A test range for the spinal railgun, the gun on our ship's
   nose. The Garuda patrols east and west and doesn't hunt you. Find its
   course, stop the ship, turn the nose onto where it will be, and fire.

## Running it

Open the project in **Unity 6000.5.10f1** and load:

```
Assets/Scenes/Tactical/00_Start.unity
```

Press play. The intro runs by itself to a menu: NEW GAME, SKIRMISH and
SETTINGS. The full intro plays once per run. Each later return to the title
shows only STARWOLF flickering in, then the menu.

NEW GAME opens SAVE SLOTS. There are three slots. An empty slot starts a new
campaign. A used slot continues its campaign. The SECTOR MAP opens: a console
with a BRIEFING screen on the left and a star chart on the right. Each mission
is a dot. The newest dot is already picked. Click LAUNCH. Win the mission, and
the next dot shows. A finished mission's dot shows a tick. Click it: DEBRIEF
shows MISSION COMPLETE. Its LAUNCH is greyed out: a won mission can't be
played again.
After a won mission, the ship's rooms show before the map. Damage stays on the
ship between campaign missions. The rounds, torpedoes, marines and crew left
after a win stay too. Each win gives a travel bar of 10 units, at the top right
of the rooms. Each room's actions fill it: repairs in ENGINEERING, torpedoes and
ammo in HANGAR, marines in QUARTERS. Each action costs units and gives back a
share. When the bar is full, the actions lock and SECTOR MAP flashes. SECTOR MAP
works any time; units left are lost.
The game saves after each win. DELETE on a used slot empties it.

SKIRMISH plays WOLF HUNT straight away. In a skirmish, MISSION SELECT goes
back to the title.

INITIALISING... shows, then the tactical board. The board
loads your level from `Assets/Scenes/Levels/` on top of itself. To skip straight
to the board, open `02.1_Grid.unity` instead. It plays Wolf Hunt.

## Building the game

Builds go outside the repo, in `~/Documents/GitHub/Killshot-Builds/`:

| Game | Folder |
|---|---|
| Mac | `Killshot-Builds/Mac/Starwolf-Mac-dd-mm-yyyy.zip` (only the zip is kept) |
| Windows | `Killshot-Builds/Windows/Starwolf-Win-64bit-dd-mm-yyyy.zip` (only the zip is kept) |

**Every build's name ends with the day it was made, as dd-mm-yyyy** (owner, 10 November
2026). A Windows build made on 10 November 2026 is `Starwolf-Win-64bit-10-11-2026.zip`. In
the commands below, `$(date +%d-%m-%Y)` puts in today's date by itself.

### The Windows game must be Intel 64-bit

Almost every Windows PC has an Intel or AMD chip. The Windows game must be built
for those chips. In Unity this is called **Intel 64-bit**. The folder's and the
zip's names start `Starwolf-Win-64bit` to show it.

On a Mac with an Apple chip, Unity sets the Windows game to **ARM 64-bit** by
itself. An ARM game opens only on rare ARM laptops. On a normal PC, Windows
refuses to open it and says the version is wrong. This happened in November 2026.

Unity saves this choice on each computer, not in the repo. A new computer, or a
fresh copy of the project, starts on ARM 64-bit again. Check it before every
Windows build.

**Unity checks this for you.** When you click Build for Windows, Unity first
checks Architecture. If it is not Intel 64-bit, the build stops before anything
is built. The Console shows a red error:

```
Windows build stopped. Architecture is ARM64, and it must be Intel 64-bit.
Set it in File > Build Profiles > Windows > Architecture. ...
```

Set Architecture to Intel 64-bit and click Build again. The check is in
`Assets/Editor/WindowsBuildCheck.cs`. It works the same on every computer.

### Building the Windows game, step by step

1. Open the project in Unity.
2. Open **File → Build Profiles**.
3. In the list on the left, click **Windows**. If Windows is not the active
   platform, click **Switch Platform** and wait. This can take several minutes.
4. On the right, set **Architecture** to **Intel 64-bit**. Not ARM 64-bit. Not
   Intel 32-bit.
5. Leave **Development Build** off.
6. Check the scene list at the top. Every scene in it is ticked, and
   `Tactical/00_Start` is first. The game starts from the first scene.
7. If a build from today is already there, delete its folder and its zip first. Old
   files left in the folder can end up in the new zip.
8. Click **Build**. Pick the `Killshot-Builds/Windows/Starwolf-Win-64bit-dd-mm-yyyy`
   folder (today's date) and name the file `Starwolf.exe`.
9. Check the chip. In Terminal, run:

   ```
   file ~/Documents/GitHub/Killshot-Builds/Windows/Starwolf-Win-64bit-$(date +%d-%m-%Y)/Starwolf.exe
   ```

   It must say **x86-64**. If it says **Aarch64**, the game is ARM 64-bit and
   won't open on normal PCs. Go back to step 4.
10. Zip the folder. Leave out the folder whose name ends in `_DoNotShip`. It is
    only for finding crashes, and testers don't need it. In Terminal:

    ```
    cd ~/Documents/GitHub/Killshot-Builds/Windows
    B=Starwolf-Win-64bit-$(date +%d-%m-%Y)
    zip -r -X $B.zip $B -x "*DoNotShip*" -x "*.DS_Store"
    ```

11. Delete the build folder. Keep only the zip. The folder is the same game
    unzipped, and it takes up disk space. In Terminal:

    ```
    rm -rf ~/Documents/GitHub/Killshot-Builds/Windows/Starwolf-Win-64bit-$(date +%d-%m-%Y)
    ```

12. Upload the zip to the team's Google Drive, in
    Hunter Puzzle Sim - "Starwolf" → Builds.

### What testers do on Windows

1. Unzip the whole zip. Keep every file together in its `Starwolf-Win-64bit-…` folder.
   `Starwolf.exe` does not work on its own.
2. Double-click `Starwolf.exe`.
3. Windows may show "Windows protected your PC", because the game is not signed.
   Click **More info**, then **Run anyway**.

### The Mac game

The Mac game is built for Apple chips only (**Apple silicon** in
Build Profiles → macOS → Architecture). Older Macs with Intel chips can't open it.
To make one game that opens on both kinds of Mac, pick
**Intel 64-bit + Apple silicon**. The game file gets about twice as big.

To build it: in Build Profiles click **macOS** (Switch Platform if needed), then Build.
Pick the `Killshot-Builds/Mac/Starwolf-Mac-dd-mm-yyyy` folder (today's date) and name
the game `Starwolf.app`. Then zip it and delete the folder. `ditto` keeps the app's
inside links working, where `zip` can break them:

```
cd ~/Documents/GitHub/Killshot-Builds/Mac
B=Starwolf-Mac-$(date +%d-%m-%Y)
ditto -c -k --keepParent $B/Starwolf.app $B.zip && rm -rf $B
```

## Controls

Almost everything is done with the mouse, on the board's switches. The keyboard
does a few things.

| Input | What it does |
|---|---|
| Menu | The arrow keys or `Tab` move. `Enter` or `Space` picks. A click works too. |
| `Esc` | In a level, stop the game and open the menu: RESUME, QUIT GAME, RESTART MISSION and MISSION SELECT. `Esc` again, or RESUME, closes it. On SAVE SLOTS, the SECTOR MAP and the ship's rooms, open a small menu: RESUME, MAIN MENU and QUIT GAME. `Esc` again, or RESUME, closes it. MAIN MENU goes back to the title. On the intro, quit at once, with no confirm box. There it does nothing in the Unity editor. On the MISSION COMPLETE or SHIP LOST screen, it does nothing. |
| `P`, or the PAUSE key | Stop or start time. The board opens paused. |
| Rate keys | Run time at x0.25, x1 or x5. |
| Briefing icon, next to the rate keys | Open or close the briefing. |
| Power panel, bus selectors | Click to step a bus between VENT, STANDBY and DRAW. A bus on DRAW gains one unit per game second. A bus on VENT loses one. |
| Helm, throttle fader | Drag to set the throttle. The middle is 0. Above it is ahead, below it is reverse. |
| `W` / `S` | Raise or lower the throttle. A tap moves it 1. Keep the key down and it keeps counting, slowly at first, then faster. It moves 100 in 2 seconds. It stays where you let go. `W` goes ahead. `S` goes down through 0 into reverse. Full reverse is slower than full ahead. ENG sets the limit. |
| `A` / `D` | Step the ordered heading 3° to port or starboard. Hold to repeat. The bow swings round to it. |
| `Shift` + `A` / `D` | Turn slowly while the keys are down: 2° a game second, in parts of a degree. For aiming the spinal railgun, the gun on the nose. With a lock on and the railgun picked, the ship turns itself onto the target. `A` and `D` can't turn against it. `Shift` with `A` or `D` still can. |
| `Q` / `E` | Slide the ship to port or starboard. The nose stays still. A tap gives a small push. Keep the key down and the push starts gentle, then grows to full in 2 seconds. Let go, and the ship keeps sliding. The slide fades by itself, to half in about 8 seconds. A tap the other way slows it. Tap left and right to steady the ship. They need ENG power. |
| Scroll wheel over the plot | Zoom in and out. The SCALE knob does the same when dragged. |
| Drag the empty grid, or the arrow keys | Pan the plot. With POINT, RULER or COMPASS picked, a drag draws a mark instead. |
| `C` | Centre the plot on our ship. |
| Left rail | Pick a tool: CURSOR, POINT, RULER, COMPASS, ERASE. |
| Click or drag on the plot | Use the tool you picked. |
| Right rail, or `1` to `6` | Open or close a drawer: 1 fire control, 2 sensors, 3 power, 4 hangar, 5 engineering, 6 comms. |

To change a board key, open the Keys tab of **Starwolf → Tuning** in the Unity editor. The keys do
nothing while you type in a text box, except `Esc`. It still opens the menu.

What each panel's switches do is in `docs/BOARD_UI.md`.

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

## Ruled out

The owner has decided against these. Don't suggest them again.

- No cockpit and no view out of a window.
- No shields. The armour on the hull's four sides is the only protection.
- No boarding pods. Marines go by assault boat from the hangar.
- No EXECUTE button and no emergency abort. Pause and the time rate are the only time controls.
- No broadside cannon.
- No dodge button.
- The enemy is our ship with different numbers and a brain. It never gets its own movement, sensor
  or weapon rules.
- The same rules apply in every level. Only a training level may switch a system off.
- No clicking the plot to aim.
- No standing text on the plot, like the ship's name or speed.
- Our ship never blinks or fades.
- The sensor is DRADIS. Never call it radar.

## Where to read next

| You want | Read |
|---|---|
| What the study agents found, and when to run them | `docs/team-reports/README.md` |
| What is built, what is left, and every trap hit | `docs/BUILD_LOG.md` |
| Which document answers your question | `docs/CONTEXT_INDEX.md` |
| How the game is meant to work | `docs/TACTICAL_BOARD_SPEC.md` |
| What every script does | `docs/SCRIPT_LIST.md` |
| How the systems and the scripts connect, as two maps | https://claude.ai/artifact/Tk285DVMpwzAfb44Ph2Abr (private until the owner shares it) |
| Rules for working in this repo | `CLAUDE.md` |

## A note on the sources

`References/` holds the original design document, a playable HTML prototype, and
a design handoff. They are a **timeline, not rivals**: the GDD says what the game
is and why, the prototype shows how systems behave, and the newest material wins
on specifics. See `CLAUDE.md` for the full rule.
