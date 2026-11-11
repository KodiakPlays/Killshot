# Tactical Scenes — Build Log

A running record of the new scene flow being built under `Assets/Scenes/Tactical/`.

**Read this first if you are a new session, or this session after a compaction.** It tells you what
exists, what is wired to what, which step we are on, and what to do next. You should not have to
work any of that out again.

Last updated: 2026-11-11 (Step 206). Unity 6000.5.10f1. Editor instance `Killshot@99ec1537206126d5`.

This file says where the game is today and what comes next. The finished steps and closed items are
in `docs/BUILD_HISTORY.md`.

---

## 1 · Where we are

*Brought up to date on 2026-11-11.*

**The goal now (owner, 2026-11-11).** Get every system working properly, get the key information
through to the player, and make one proper story level, MILK RUN, that is fun to play. Then put it in
front of people and hear what they think. There is no paid art or sound yet, and placeholder is fine.

**The next job is the MILK RUN player build.** Its checklist is in section 8, "Next: the MILK RUN player build".

**How the game flows.** The game opens on the intro: SECTION 36 flickers in and turns into STARWOLF.
Then comes a menu with NEW GAME, SKIRMISH and SETTINGS. NEW GAME goes to SAVE SLOTS and then the
SECTOR MAP, where each mission is a dot. The map is green, in our phosphor look. LAUNCH starts the mission on the battle board (`02.1_Grid`,
with the level loaded on top). After a win you see MISSION SELECT, then the ship's rooms with the
travel bar, then the map again. After the last mission the end card plays, then the title. SKIRMISH
plays Wolf Hunt straight away. In a mission, Escape pauses the game. On SAVE SLOTS, the map and the
rooms, Escape opens a small menu: RESUME, MAIN MENU and QUIT GAME.

**The campaign.** The missions are listed in `Assets/Campaign/Campaign.asset`, and you set them up in
Starwolf → Campaign. There are two so far: 1 MILK RUN (level file `Orientation.unity`) and 2 TEST
MISSION, a throwaway that has to be switched off before the build. About ten missions are planned.
Rounds, torpedoes, marines, crew and damage go on with you to the next mission, and a won mission
can't be played again. The closing line on the end card isn't written yet.

**MILK RUN.** It has 23 checkpoints, from turning the power on to capturing the pirate corvette's
captain. The railgun is off in this mission. A script played it through with the real controls on
2026-11-11, and it passed. No person has played it all the way through yet.

**Our ship and the enemy.** Our ship is an Asura-class destroyer. It has one railgun fixed on its
nose, and you aim it by turning the ship. It also has 4 PDC groups, 2 torpedo launchers and assault
boats with marines, and no fighters. The enemy is the Garuda-class cruiser. It is built like our
ship, with different numbers and its own brain.

**The board.** The battle board fills any screen shape. Zoomed in, ships and shots show as lit 3D
models. Zoomed out, they are lines. Past visual range you only see what DRADIS picks up.

**The owner's tools** are on the Starwolf menu: Tuning, Level, Art and Campaign. Game numbers change
only through Tuning, and every change gets a row in `docs/TUNING_LOG.md` with its old value.

**Old test levels.** Wolf Hunt, the other test levels and the MISSIONS list are switched off. They are
kept and never deleted.

**Checks.** The last full playtest is `Logs/Playtests/2026-11-11_015627`. 256 checks passed and one
failed (#194). The steps after 197 haven't been through a full playtest yet.

**The refactor** is done apart from steps 2, 5 and 8 (section 8).

**Working rules.** Talk to the owner before building anything new, and do one step at a time.
Several chats share one Unity Editor, so message them before you compile or press play. Every known
bug is in section 8, "Bug list". Finished steps and closed items are in `docs/BUILD_HISTORY.md`.

### Steps not finished

These steps are built but not finished: they are tests only, not committed, or waiting for a check.
Every finished step is in the step table in `docs/BUILD_HISTORY.md`. A new step takes the next
number after the highest one in either table.

| Step | What it is | Status |
|---|---|---|
| 193 | Codex's art, tried out in a test scene (owner 2026-11-10: "Make a new scene for this… It doesn't have to be in the flow. I just want to see this in action." Then: the new Fire Control "needs to appear when you click Fire Control"; "the periscope goes away, and that red visual camera comes into this Fire Control"; the drag lever "should go away, and we should have a large button instead, which just starts glowing and says 'Fire, Fire, Fire'". Then: the hangar painting goes "into the in-between mission flow so you can see the hangar room"). **What the player sees:** in the test scene `Assets/Scenes/Tactical/Fire Control Codex.unity`, FIRE on the systems rail slides in Codex's Fire Control from the right. It is as tall as the plot. The top window reads FIRE CONTROL and TARGETING COMPUTER, with the mount's short name. The monitor shows the target's name: BOGEY T07 until it is scanned, then GARUDA in amber. Under it are the periscope bearing, the tracking ID and where the track comes from. SIGHT lowers the periscope, and its round red view shows in the monitor, with SLEW on its left and MAG on its right. With SIGHT up, a scanned Garuda shows as a pale wireframe picture. Below the monitor are the firing solution and the ready line. Then come four sections numbered 1 to 4: the weapon system, the solution knobs and TRACK, the capacitor, and the four arming switches. At the bottom is the FIRE button. It is dark and reads FIRE until a press would fire. Then it turns red, glows and reads FIRE  FIRE  FIRE. One press fires one shot. The periscope at the top of the board is gone in this scene. Between missions, the HANGAR room shows Codex's painting. Click the flight leader to talk. GOOD closes the talk. **What changed:** new `Fire Control - Codex.prefab`, a variant of `Fire Control.prefab`, with Codex's housing art. `FireControlPanel.cs` has the contact name and picture, and the FIRE button (the click `fc.fire`). The board's own Fire Control leaves them empty. `WeaponSystem.cs`: the checks in `Fire()` moved into `ReadyToFire`. The rules did not change. In the test scene the periscope is moved inside the drawer and scaled to the monitor. Codex's frame, `Frame - Fire Control.prefab`, is in the test scene. Its Art list now names its own picture. `Ship.unity`: the HANGAR painting and the FLIGHT LEADER picture (in `Assets/UI/Rooms`). The painting keeps its shape. Her button is over her in the painting. The grey HANGAR stand-in title is off. The talk picture keeps its shape. **Made without the owner:** SIGHT still lowers the periscope by hand, so the lock rules are unchanged. The drawer is plot height, on the right. The switches are drawn at three quarters size. The button's colours and glow. **A mistake, undone:** Codex's frame was put on `02.1_Grid` too. The owner said the work was for the test scene only. It was taken out the same night. **Files:** `Fire Control Codex.unity`, `Fire Control - Codex.prefab`, `Frame - Fire Control.prefab`, `Art/Fire Control/InspectorHousing.png`, `Art/Fire Control/Garuda-profile.png`, `Art/Frame/FireControlSurround.png`, `Assets/UI/Rooms/HangarRoom.png`, `Assets/UI/Rooms/FlightLeader.png`, `Ship.unity`, `FireControlPanel.cs`, `WeaponSystem.cs`, `docs/TUNING_LOG.md` (Fire Glow Seconds), `docs/BOARD_UI.md` | **Done, as a test.** One-time Play Mode checks, nothing kept. Every Fire Control control took clicks where it is drawn. A locked switch refused with the refused sound. TRACK dropped 1 px while held. FIRE on the rail opened and closed the drawer. SIGHT brought the view into the monitor, and SLEW and MAG took clicks. With weapons released, WEP powered and the four switches set, the button glowed. One press fired one slug (charge 100% to 30%). A second press did nothing. The test marked the Garuda as scanned. The hangar talk opened and closed. Screenshots in `Logs/FireControlCodex/`. Not a full playtest. The owner still decides whether this Fire Control replaces the board's (section 8). |
| 201 | The sector map in phosphor green, as a test (owner 2026-11-11: "in the operations map, is it possible to slowly start changing the image they have of their sector map ... with maybe some of our phosphor effects? ... as an experiment ... use some of our font also over there in small, replacing the font." Then: "that whole map ... I want a test copy. And I mean the square, I think, block letters, but okay, fine. If you feel a thin font, then we'll do a thin font." Then, after seeing it: "I don't want 'Destroyer Navigation STARWOLF Sector Plot.' We have to simplify it because we're not using the name STARWOLF in the actual game ... Similarly, I don't think I need 'Destroyer Navigation' as text." Then, asked about rubbing out STARWOLF and DESTROYER / OPERATIONS on the left screen: "yes, please make that green if you don't mind."). **What the player sees:** in the test scene `Assets/Scenes/Tactical/Sector Map Phosphor.unity`, both screens of the console look like the intro's green screen. On the right-hand map, the grid, the contour lines, the ellipses, the stars and the MISSION ARCHIVE box are phosphor green, with a soft glow round each line. Each screen gets darker towards its edges. Faint dark scanlines run across both screens, over the mission dots, the briefing text and the BRIEFING, DEBRIEF and LAUNCH keys too. The top of the map reads only SECTOR PLOT. SECTOR PLOT and MISSION ARCHIVE are in the square block letters of the STARWOLF title. DESTROYER NAVIGATION is rubbed out, with no new words. The small labels (RELATIVE BEARING, NAV FIX 03, PRIMARY, OUTER DRIFT) are in Share Tech Mono, the thin letters in the loading screen's corners. The mission dots and their names are the intro's green too. On the left screen, STARWOLF and DESTROYER / OPERATIONS are rubbed out, with nothing written back. The top of the left screen is empty above its first line. MISSION RECORD is in block letters. CAMPAIGN ARCHIVE is in the thin letters. The screen's rounded corners keep Codex's paint. The metal, the cables and the BRIEFING and OPERATIONAL MAP plates look as Codex painted them. **What changed:** a new picture, `OperationalMapConsole Phosphor.png` (3680 x 2070, twice the size of Codex's). Only its two screens are different from Codex's. A new 1 x 4 px strip, `Scanlines.png`. In the test scene, two new objects, `Scanlines (map)` and `Scanlines (briefing)`, repeat the strip over each screen's glass. They are the last two under `Sector Map` and draw over everything on the screens. They do not take clicks. On the test scene's `Sector Map Script`, Open Ink and Done Ink are the intro's green (Done Ink is faded, A 0.45). A tool makes both pictures: `docs/sector-map-experiment/make_phosphor_map.py` (how to run it is in `docs/BOARD_UI.md`, "Sector map"). The real `SectorMap.unity` and Codex's `OperationalMapConsole.png` are unchanged. The test scene is not in the build list, and nothing in the game opens it. No code changed. **Made without the owner:** the green shades and the glow strength, how faint the grid is, which words got block letters and which got thin ones, on both screens (the owner leaned to block letters; the small labels are thin, to compare the two), the scanlines' strength and spacing, and the space between block letters (2 squares; the intro uses 4). **Changed after the owner saw it:** the top first read STARWOLF / SECTOR PLOT, with DESTROYER NAVIGATION in thin letters under it. Both went, as the owner asked, and the picture was made again. Then the owner said yes to a green left screen. Its glass got the same green as the map, and its painted words were redone. The rub-out changed too: it now fills each word's whole box from the picture round it. Before, faint ghosts of the old words showed. **Not done yet:** the slight flicker. The tick picture on a won mission. **Files:** `Sector Map Phosphor.unity`, `Art/Briefing/OperationalMapConsole Phosphor.png`, `Art/Briefing/Scanlines.png`, `docs/sector-map-experiment/make_phosphor_map.py`, `docs/BOARD_UI.md`, `docs/CONTEXT_INDEX.md` | **Done, as a test.** Played once in the test scene, on the hidden test save slot 8 (deleted after). MILK RUN's dot, its name, the BRIEFING, DEBRIEF and LAUNCH buttons and the briefing text showed, all green. No errors in the console on a clean run. Screenshot: `Logs/SectorMapPhosphor/01_phosphor_map.png` (not in git). Checked again once the left screen was green, on slot 8 (deleted after): both screens green, MILK RUN's briefing on the left, no errors. Screenshot: `Logs/SectorMapPhosphor/02_both_screens_green.png` (not in git). No compile. The owner saw the first version. **In the game:** the owner asked for the green map in the MILK RUN build. The real SectorMap scene now uses the green picture, the two scanline overlays (Scanlines (map), Scanlines (briefing)) and green dots (Open Ink and Done Ink). Codex's original picture is kept but no longer used (941942f). Not done yet: the slight flicker on the green screens; the won-mission tick (`CompletedMissionNode.png`) is still Codex's teal one. |
| 204 | A test mission after MILK RUN, to see the ship's rooms and the travel bar (owner 2026-11-11: "I think it's important to go to the ship rooms and the travel bar. Set up a dummy mission... pick one of the older levels... even if you just give me one checkpoint and it says, 'Okay, congratulations, you won,' that's good enough... You see the flow."). **What the player sees:** the sector map has a second dot, 2 TEST MISSION, at (470, -40). Its briefing says it is a test. It is won after 5 seconds. It must come out before the player build. MILK RUN is no longer the last mission. A campaign win of MILK RUN opens the ship's rooms and the travel bar again. SECTOR MAP then shows the TEST MISSION dot. LAUNCH opens the board with its level. The board starts paused, as in every level. Time starts with x1 or the PAUSED switch. The 5 seconds count from then. TEST MISSION is the last mission now. After its win, MISSION SELECT goes to the end card, then the title, where STARWOLF flickers in. A used save slot reads "CAMPAIGN · 1 OF 2 · 50%" after MILK RUN. It counts the missions in the Campaign file (from the code; not seen on screen). **Must come out before the player build** (Mission Setup Tool chat; section 8, "The player build": the owner wants that build to end with MILK RUN). To take it out, delete the second entry in `Assets/Campaign/Campaign.asset` → Missions, or ask the Campaign Flow chat. The scene can stay in the project. Old levels are never deleted. **What changed:** the Campaign file has a second mission, TEST MISSION, with level `TestMission`. A new scene, `Assets/Scenes/Levels/TestMission.unity`, is a copy of `FlightDrill.unity`. The copy's Mission object (the flight drill goal) has one checkpoint: WAIT 5 game seconds, with the note "TEST MISSION. Nothing to do. Mission complete in 5 seconds." `FlightDrill.unity` is unchanged. The new scene is in the build's scene list. No game numbers changed. No code changed. **Files:** `Assets/Campaign/Campaign.asset`, `Assets/Scenes/Levels/TestMission.unity`, `ProjectSettings/EditorBuildSettings.asset` | **Done, as a test.** Checked in play mode on 2026-11-11 with the hidden test slot (slot 9), deleted after. With MILK RUN won, the map showed MILK RUN ticked and TEST MISSION as the new dot, and LAUNCH worked. LAUNCH loaded the board with TestMission. 5 game seconds after time started, the mission was complete, and the save listed both wins. MISSION SELECT went to the end card, then the title. |
| 206 | Enemy slugs we can see leave a line on the table (owner 2026-11-11, on the "Ship Actions Lab" page, https://claude.ai/artifact/KXZhwnSU7hoG7aaEqThx6g: asked whether enemy shots should leave a line too, the owner picked "Only ones we can see"). **What the player sees:** an enemy slug we can see leaves a faint red line on the table, like our railgun's line (Step 197). It fades over 5 game seconds from when we first saw it. We see a slug when it is inside our visual range, or inside DRADIS's range while DRADIS is listening. The line runs only along the part of the slug's path we saw. It starts where the slug first came into sight or DRADIS range, and stops where it left our sight. So a line never shows where an unseen ship fired from. Enemy torpedoes leave no line. **Tuning:** no new number. On the Board tab of **Starwolf → Tuning**, under Plot & our ship, **Shot Line Seconds** (5) now sets both our line and the enemy's. Its tooltip says so. **How it is set up:** `PlotPanel.cs`: `TrackEnemyShots`, called after the incoming rounds are drawn, starts a line for each enemy slug the first time we can see it. It moves the line's end with the slug while we still see it, and fades it out. `SeesSpot` is the "can see" test. The script's top note is brought up to date. **Made without the owner (open point, section 8):** the "can see" test goes by distance only. A rock between us and the slug doesn't hide it, and DRADIS's band isn't checked. **Files:** `PlotPanel.cs`, `docs/TACTICAL_BOARD_SPEC.md` (Rule 8.4.e; §8.4, §11.4, §20.2), `docs/BOARD_UI.md` (Plot) | Compiled; play check pending. |

---

## 2 · Scenes

The intro, save slots, sector map, ship's rooms, end card and board scenes are in
`Assets/Scenes/Tactical/`. Levels are in `Assets/Scenes/Levels/`.

A campaign mission goes `00_Start` → `SaveSlots` → `SectorMap` → `02.1_Grid`, with the mission's
level loaded on top of the board. After the mission, MISSION SELECT opens `Ship`, the ship's rooms,
and from there the player goes back to the map. After the last mission it opens `EndCard`, then
`00_Start`. SKIRMISH goes from `00_Start` straight to `02.1_Grid` with `Level_01` (Wolf Hunt).

Build Settings list 17 scenes: `00_Start`, `01_Briefing`, `SectorMap`, `Ship`, `SaveSlots`,
`02.1_Grid`, `Level_01`, `FirstShot`, `SilentShot`, `Prize`, `Intercept`, `FlightDrill`, `ColdRun`,
`LongShot`, `Orientation`, `EndCard` and `TestMission`. Nothing in the game's menus opens
`01_Briefing` or the old test levels now. They are kept and never deleted (owner).

| Scene | GUID | State |
|---|---|---|
| `00_Start.unity` | `e50db2cd323304012ba3e7afb4e531cb` | Built |
| `01_Briefing.unity` | `326d3f3159fcb4965bebcf61495595c6` | Built — the MISSIONS list. Nothing opens it now |
| `02.1_Grid.unity` | `db225074494dd4259bc8aab05e48399e` | Built — the board. A mission's level loads on top of it, after `SectorMap` or, for SKIRMISH, straight from `00_Start` |
| `Levels/Level_01.unity` | `4cf76e81d39984c4db973e752f8bef63` | Built — Wolf Hunt, loaded on top of `02.1_Grid` |
| `Levels/FirstShot.unity`, `SilentShot.unity`, `Prize.unity` | (see their `.meta`) | Built in Step 114 — the training levels WEAPONS FREE, NO PEEKING and FINDERS KEEPERS |
| `Levels/Intercept.unity` | `d5a257d6a5fd342c895b7ae05b7bdaa2` | Built in Step 137 — INTERCEPT |
| `Levels/FlightDrill.unity` | `64fcc2628baee492ca931c555bdaaa24` | Built in Step 144 — FLIGHT DRILL |
| `Levels/LongShot.unity` | `133d1996e814e4006bb14050901900a4` | Built in Step 170 — LONG SHOT, the test range for the spinal railgun |
| `Levels/Orientation.unity` | `e8095fc79760047e4a1fa59c5faa65ba` | Built in Step 178 — MILK RUN (called ORIENTATION until 2026-11-10), mission 1 of the story |
| `SaveSlots.unity` | `329767fd99cb64233a5ceacac829b9d5` | Built in Step 167 — SAVE SLOTS |
| `SectorMap.unity` | `4daec8ab8da984ec4ac730a7f02c04be` | Built in Step 167 — the sector map |
| `Ship.unity` | `3940d09bb1247441a982896486e963d8` | Built in Step 168 — the ship's rooms and the travel bar |
| `EndCard.unity` | `5e3a0c3161232446aa53ffbc8e9f1ec0` | Built in Step 194 — the end card |
| `Levels/ColdRun.unity` | `6d60a6452483448d18cadfac1e3253ac` | Built in Step 146 — COLD RUN |
| `Levels/TestMission.unity` | `aae2049f0df35468bbe874ee1de78a4e` | Built in Step 204 — TEST MISSION, a throwaway test |

### `00_Start` — built in Step 2

- `Main Camera` — orthographic, clears to `#0A0C0E`.
- `EventSystem` with a `StandaloneInputModule`. **This is not optional.** A Button does nothing at
  all without one in the scene.
- `Canvas` — Screen Space **Camera**, not Overlay. CanvasScaler on Scale With Screen Size,
  1920x1080, match 0.5.
- `Title` — TextMeshPro, reads "STARWOLF", 110pt.
- `StartButton` — an Image with a Button on it, and a child `Label` reading "START GAME".
- `Title` and `StartButton` are switched off.
- `STARWOLF Intro`: the intro and menu. NEW GAME opens SAVE SLOTS. SKIRMISH calls `ScreenFlow.Advance()`,
  which loads `02.1_Grid` with Wolf Hunt.
- `ScreenFlow` — added in Step 4, see section 3.

### `01_Briefing` — built in Step 3

Same camera, EventSystem and Canvas setup as `00_Start`.

- Nothing in the game opens this scene now (Step 178). It is kept.
- Since Step 114 it is the MISSIONS list. `MISSIONS card` (on): the green screen, MISSIONS typed at the
  top (a copy of the WOLF HUNT card; the green screen's letters have no V, so not LEVELS). `Level List`:
  a canvas over it (sort order 1) with five rows, each a Button whose click calls `ScreenFlow.Play` with
  its level. The EventSystem selects WEAPONS FREE first, so the arrow keys and Enter work at once.
- `MISSION ONE card` (off) and `WOLF HUNT card` (off) are kept, unused. `INITIALISING` (off), `ScreenFlow`.

### `02.1_Grid` — made in Step 28, board built in Steps 29–33, World added in Step 34

The board from the new Claude Design handoff. The briefing's Enter now leads here. The board is
built and verified. Step 34 added the first game state: a `World` root object (below).

- The scene is edited directly. The eleven panels and seven frame pieces are prefabs in
  `Assets/UI/Board/Panels`, with one copy of each on the board. How to work on them:
  `docs/BOARD_UI.md`.
- Runtime code: `Assets/Scripts/Game/` (the board script, `BoardManager`),
  `Assets/Scripts/Systems/` (the ship state, `ShipState`, and the ship parts), `Assets/Scripts/UI/`
  (the shared board pieces, `ArtList`, `BoardPalette`), `Assets/Scripts/UI/Panels/` (every panel)
  and `Assets/Scripts/Tactical/` (the world, the spawns and the rest). See section 3.
- The full record: `Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md` (deleted; `git show 12ccdd1:Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md`).
- What the package left out: `docs/finished-plans/HANDOFF_GAPS.md` (106 rows).
- What Claude Design must still provide, verified: `docs/finished-plans/CLAUDE_DESIGN_NEEDS.md`.

The setup from Step 28:

- `Main Camera` — orthographic, clears to `board.ground` `#0A0C0E`, with an `AudioListener`.
- `EventSystem` with a `StandaloneInputModule`.
- `Board` — Canvas, Screen Space **Camera**, plane distance 5, CanvasScaler 1920x1080, Screen Match
  Mode Expand (Step 87), reference pixels per unit 100. These are the handoff README's canvas numbers.
  Since Step 182, `Tactical Board` stretches to fill the canvas, and each panel sheet is tied to its
  screen edge.

Added in Step 34:

- `World` — a root object, **outside** the Board canvas, with `TacticalWorld` on it. Children:
  - `OwnShip` at (0, 0, 0): our ship. It has a `Ship` (pointing at `Assets/Ships/Asura`) and the
    eight ship parts. Its `Model` child is `StarWolf_Destroyer_PlayerShip.fbx`, same model and
    placement as the old `Prefabs/PlayerShip.prefab`. At play it moves to the level's Player Spawn.
  - `Garuda · Cruiser` at (18058.42, 16259.87, 0): the enemy. It has `WorldContact` (trackKey
    `t07`), `EnemyBrain`, a `Ship` (pointing at `Assets/Ships/Garuda`) and the eight ship parts.
    Its `Model` child is `3D Assets/UpdatedStarships/StarWolf_Cruiser_mesh.fbx`. At play it moves
    to the level's `Enemy Spawn · Garuda`.
  - `Contact View Camera · Garuda`: a `ShipView` camera that draws the Garuda's model for the plot.
  - `Ship View Camera`: a `ShipView` camera that draws our ship's model for the plot.
  - Each model is scaled to its ship's Length (Step 179). Models only, no old scripts.
- The design's sample picture on the plot was `Plot/Design mock-up (off)`. Step 89 deleted all but
  three parts the code uses. The group is switched off.

Added in Steps 66 and 68 (under `Board/Tactical Board`):

- `Drawers` — just above `Plot`. The six drawers are under it: `Power`, `Sensors`, `Comms`,
  `Fire Control`, `Hangar` and `Engineering`. It clips them to the frame's opening with a
  `RectMask2D` (Step 66).
- `SignalBox` — just after `TimeControl`, in the frame's top-right. A prefab that shows the
  Sensors E-SIG row. `SensorsPanel` fills it in (Step 68).
- `State · periscope` is the periscope's LOWERED label. It is the one object left with an old
  `State · …` name.

Changed in Step 74 (now under `Board/Tactical Board/Drawers/Engineering/Compartments/Ship picture/Hull`):

- The six `Lamp · …` objects are gone.
- Four RawImages, one per side: `Ship · PROW`, `Ship · STBD`, `Ship · AFT`, `Ship · PORT`. Each
  shows one part of our ship from `Assets/Art/Board/Hull/`, through `BoardLinearImage`.

Added in Step 85:

- `End Screen` — the last child of `Board/Tactical Board`, so it draws over everything. A prefab
  (`Assets/UI/Board/Panels/End Screen.prefab`) with `EndScreenPanel`. Its `Screen` is off until
  the mission ends.
- `QuestSystem` on `World`: the mission, win and loss.

### `Level_01` — made in Step 67

- In `Assets/Scenes/Levels/` and in Build Settings. Loaded on top of `02.1_Grid` at play.
- `Battle Space`: 20,000 × 20,000 units since Step 130 (60,000 from Step 107, 100,000 before).
- `Enemy Spawn · Garuda` with `EnemySpawn` (trackKey `t07`). Since Step 130 at (0, +8,000), centre
  top, facing south (z 180), its box 8,000 × 2,400 units. Signal Seconds 60, First Signal Seconds 15,
  On Air Seconds 60 (Step 140).
- `Player Spawn` (Step 73) with `PlayerSpawn`, at (0, −8,000) since Step 130, centre bottom, facing
  north. (Named `Our Ship Start` until the owner renamed it.)
- `Terrain`: since Step 107 no husk. Groups Wrecks (5), Asteroid field 1–4 (34 rocks), Debris · near
  (12), · middle (35), · far (50): 136 pieces, all `WorldTerrain`. (Step 73 had `Base Husk` at (0, 0)
  and Debris · in the husk; Step 93 thinned the belts; Step 130 moved them to a third of their distance
  from the centre.)
- `Gas Cloud West`, `Gas Cloud Middle` and `Gas Cloud East` (Step 129).
- No camera and no light: the board's are used.
- A `Mission` box with the goal WolfHunt and the briefing.

### `Orientation` — MILK RUN, mission 1 (made in Step 178, rebuilt in Steps 192 and 200)

This scene is mission 1 of the story, MILK RUN, and the first dot on the sector map. It kept its old
name, `Orientation`. Weapons are off until checkpoint 19, and the railgun stays off for the whole
mission. The 23 checkpoints are on the `Mission` object, with Codex's COMMAND lines. Step 192 in
`docs/BUILD_HISTORY.md` lists them, and Step 200 has the fixes from the play-through.

Our battle group is under `Battle Group`: a carrier, a battleship and a corvette, with Friendly ticked.
Their stats files are `Assets/Ships/Carrier.asset`, `Battleship.asset` and `Corvette.asset`. They still
have the stand-in call signs the owner wants gone (section 8). Three shuttles go back and forth between
the group and the escape pods. The ORIENTATION set-up of Step 178, and the old training levels, are in
`docs/BUILD_HISTORY.md`.

**The MILK RUN layout:** our arrival at (−2,500, −3,800), heading 45; the group waits at
(0, −1,500) and flies (−1,200, 1,800), (−3,000, 4,000), (−2,600, 6,200); a small rock field at
(1,000, 3,600), radius 1,200, 22 rocks; the station s1 (`Station · Ruin`, `Assets/Ships/Station.asset`,
a layered square turned 45 degrees, made of Unity shapes) at (0, 8,500), with ten wrecked ships and ten
escape pods round it; the pirate corvette p1 at (2,600, 7,200) inside seven wrecks: from (0, 5,900) DRADIS
at gain 2 does not hear it, at gain 4 it does (range 2,907, interference 3,500).

---

## 3 · Scripts, shaders and wiring

### `ScreenFlow.cs` — new in Step 4

The file is `Assets/Scripts/Tactical/ScreenFlow.cs`. It is small and reusable.

- Fields you set in the inspector: `nextScene` (the scene name, as text), `advanceOnEnter`
  (a tick box), `showFirst`, `advanceAfterSeconds` and `cards` (the title cards, shown in turn).
- One public method, `Advance()`, which loads `nextScene`.
- It has a `going` guard so it cannot fire twice.
- It logs an error if `nextScene` is left empty.
- The Escape menu (Step 190): **Escape Menu** is the menu Escape opens and closes. Left empty,
  Escape quits the game at once. It is set on SAVE SLOTS, the sector map and the ship's rooms.
  **Main Menu Scene** (`00_Start`) is the scene MAIN MENU loads. The menu's buttons call
  `Resume()`, `MainMenu()` and `Quit()`. The menu is `Assets/UI/Menus/Escape Menu.prefab`.

Where it is attached:

| Scene | On | `nextScene` | `advanceOnEnter` | How it fires |
|---|---|---|---|---|
| `00_Start` | A separate root object, named `ScreenFlow` | `02.1_Grid` | off | SKIRMISH calls `Advance()`. NEW GAME loads `SaveSlots` instead |
| `01_Briefing` | A separate root object | `02.1_Grid` (was `02_Grid` until Step 28) | off | Step 114: it waits. A row on the level list calls `Play(level)`: it sets `TacticalWorld.chosenLevel` and shows INITIALISING, whose `finished` calls `Advance()`. (Until Step 114: every 3 s or on Enter, the next card.) Nothing in the game opens `01_Briefing` now (Step 178). |

The script is on **its own root GameObject**, not on the button or the canvas.

### The board — new in Steps 29–33

Everything on the `02.1_Grid` board. Detail is in `UI_INTEGRATION_LOG.md` (deleted; `git show 12ccdd1:Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md`); do not copy it here.

| File | What it is |
|---|---|
| `Assets/Scripts/Game/BoardManager.cs` | The board script (was `HandoffBoardControls`). Runs the 0.42 s tick and the board's own clock, and takes every click (`Click`). |
| `Assets/Scripts/Systems/ShipState.cs` | One ship's state (was `HandoffBoardState`). It has the contact list, what the periscope has classified, the plot marks, the battle space, destroyed and captured, the EMP clock, the dice and kills. It reaches the ship's parts through `nav`, `power`, `sensors`, `comms`, `weapons`, `hangar`, `damage` and `orders`. |
| `Assets/Scripts/UI/Panel.cs` | The base class for every panel, with the Art list (`ApplyArt`) and the Fonts list (`ApplyFonts`). |
| `Assets/Scripts/UI/Panels/PeriscopePanel.cs`, `PlotPanel.cs` | The periscope and the plot. |
| `Assets/Scripts/UI/Panels/*Panel.cs` | The other eight panels: helm, power, sensors, comms, fire control, hangar, engineering, orders. Each shows its ship part and passes clicks to it. |
| `Assets/Scripts/UI/Panels/EndScreenPanel.cs` | The end screen (Step 85): how the mission ended, RESTART MISSION and MISSION SELECT. MISSION SELECT goes to `00_Start` outside a campaign. In a campaign it opens `Ship`, the ship's rooms, or `EndCard` after the last mission (Steps 168, 194 and 203). It is the pause menu too (Step 156): Escape in a level opens it, titled PAUSED, with RESUME and QUIT GAME added. |
| `Assets/Scripts/Game/QuestSystem.cs` | The mission (Step 85): win and loss, checked each tick. It pauses the board at the end. On `World`. Wolf Hunt's jump point and reinforcement countdown (Steps 98, 99). Step 114: the level's goal (`MissionGoal`); the training goals are won or lost at once. Step 144: the flight drill's steps (`StartDrill`, `Passed`), and the ring on the plot, which uses the jump point's ring. |
| `Assets/Scripts/Game/Mission.cs` | Step 114. A box in a level scene: the level's goal (Destroy, HitUnseen, Capture, FindBySignal, Flight or WolfHunt). At the start it tells QuestSystem. A level without one plays Wolf Hunt. Step 144: with the goal Flight, the box has the drill's steps, each with a check, an amount, a ring and a note. |
| `Assets/Scripts/UI/BoardMotion.cs`, `Eased.cs`, `ClickSpot.cs`, `Polygon.cs`, `Wedge.cs`, `MarksGraphic.cs`, `PlotGrid.cs`, `SweepGraphic.cs`, `Rockers.cs`, `ScopeSight.cs` | Shared pieces: motion, easing, click spots, shapes, marks, plot grid, sweep, rockers, scope sight. |
| `Assets/Scripts/UI/ArtList.cs` | The Art list and the Fonts list for the frame pieces, which have no panel script. |
| `Assets/Scripts/UI/BoardPalette.cs` | The board's colours. The file is `Assets/UI/Board/Palette.asset`. Every panel reads its colours from it. |
| `Assets/UI/Board/Panels/*.prefab` | The eleven panels (Helm, Power, Sensors, Comms, Fire Control, Hangar, Engineering, Orders, Periscope, Plot, End Screen) and the seven frame pieces (Frame, ToolRail, SystemsRail, TimeControl, Console, AlarmStrip, SignalBox). Guide: `docs/BOARD_UI.md`. |
| `Assets/Scripts/Tactical/GammaBoardPresenter.cs` | Shows the board in gamma space. See decision 15. |
| `Assets/Scripts/Tactical/CssEase.cs` | The handoff's CSS easing curves. |

Since Step 34 the World feeds the board (below). Since Step 35 power makes the ship loud (`signal`,
now in `SensorSystem`), and `BoardManager` stops the whole tick while paused. Details in
section 6, Step 35.

The campaign's scripts are in `Assets/Scripts/Game/`: `Campaign`, `SaveGame`, `SaveSlots`, `SectorMap`,
`ShipRooms` and `EndCard`. `docs/SCRIPT_LIST.md` says what each one does.

### The ship parts — new in the board refactor (2026-10-26) and Step 80

`OwnShip` and `Garuda · Cruiser` each have a `Ship` and all eight parts. Each part has rules only,
nothing on screen. Its numbers are in its `Stats` section, in the ship's stats file.

| File | What it is |
|---|---|
| `Assets/Scripts/Systems/NavSystem.cs` | Movement: position, heading, throttle, turning, speed, drift, the side thrusters (Step 144), and placing the model. ENG sets top speed, acceleration, braking, turn rate and side thrust. `HelmPanel` reads `Q` and `E`: `slide` while a key is down, and each tap in `slideTaps` (Step 171). The slow turn: `fineTurn` from Shift with A or D, `fine` for the part of a degree, and `aimHeading` for a lock's turn onto the spinal railgun's lead (Step 170). |
| `Assets/Scripts/Systems/PowerSystem.cs` | The reactor, the four buses and the load shed. |
| `Assets/Scripts/Systems/SensorSystem.cs` | DRADIS and the active scan: gain, band, cone, the ship's signal, what is heard, identified or in sight. |
| `Assets/Scripts/Systems/CommsSystem.cs` | The radio: band, frequency, carrier lock, cipher dials, the message. |
| `Assets/Scripts/Systems/WeaponSystem.cs` | Fire control: mounts, arming, firing solutions, slugs, missiles, PDCs, the capacitor, ammunition. `FixedGun` is a railgun with Half Arc 0, such as our spinal railgun (Step 170). |
| `Assets/Scripts/Systems/HangarSystem.cs` | Squadrons, launches, fighters' attack runs, assault boats and marines. `KeepFighters` keeps only the ship's Fighters number (Step 170). |
| `Assets/Scripts/Systems/DamageSystem.cs` | Compartments, side armour, fire, vacuum, radiation, crew, damage control. |
| `Assets/Scripts/Systems/OrdersSystem.cs` | The alert condition, the AUX order pool, and the orders. |
| `Assets/Scripts/Systems/Ship.cs` | On each ship. Points at its stats file. The parts read their numbers through it. In play each ship uses a copy of its file. At the start it fills in what the ship then uses up: the salvo, the PDC loaders, the marines and each room's crew (Step 107). |
| `Assets/Scripts/Systems/ShipStats.cs` | One ship's numbers: one section per part. |
| `Assets/Ships/Asura.asset`, `Assets/Ships/Garuda.asset` | The stats files: our ship and the Garuda. |
| `Assets/Ships/Frigate.asset` | The stats file for the Naga frigates (Step 99). |
| `Assets/Ships/Carrier.asset`, `Battleship.asset`, `Corvette.asset`, `Station.asset`, `Shuttle.asset` | The stats files for the other ships in MILK RUN (Steps 178 and 192). |

### The Tuning window and other editor files — Tuning new in Step 38, tabs in Step 80

| File | What it is |
|---|---|
| `Assets/Editor/StarwolfTuningWindow.cs` | Menu **Starwolf → Tuning**. Six tabs: **Ship** (our stats file), **Enemy** (the Garuda's), **Board** (every `[Tune]` value in the open scene), **Keys** (Step 108; `Q` and `E` are on it since Step 144), **Hard Points** and **Ranges**. |
| `Assets/Editor/StarwolfArtWindow.cs` | Menu **Starwolf → Art** (Step 185). Every picture in one window: one fold for each board panel prefab, then one for each open scene. Drop a new picture onto a box to change it. The change is made in the prefab or scene the picture is in. |
| `Assets/Editor/CampaignWindow.cs` | Menu **Starwolf → Campaign** (Step 205). Edits the Campaign file, `Assets/Campaign/Campaign.asset`: the missions in play order, an On switch for each, each mission's name, level, briefing and map dot, and the closing line. |
| `Assets/Scripts/Tactical/TuneAttribute.cs` | `[Tune("Section")]`. Marks a board value (the plot, sounds, the clock) for the Board tab. Ship numbers don't use it. `[Tune("Keys")]` marks the board's keys. |
| `Assets/Editor/Playtest/PlaytestRunner.cs` | The playtest (menu **Starwolf → Playtest → Run power-up playtest**). See "The playtest" below. |
| `Assets/Editor/ReadOnlyDrawer.cs` | Draws `[ReadOnly]` fields in the inspector, greyed out. |
| `Assets/Editor/PowerManagerEditor.cs` | Soham's inspector for his old `PowerManager`. The board doesn't use it. |

### The playtest and the Game view

**Standing end-of-feature check — the playtest (working, 2026-10-17).** Run it after a big code
move, or once at the end of a batch (owner, 2026-10-27). For a small change, check only that
change in play mode. Menu **Starwolf → Playtest → Run power-up playtest** (`Assets/Editor/Playtest/PlaytestRunner.cs`).
It:

- goes through the user flow: intro → menu SKIRMISH → the board with Wolf Hunt;
- sets up the ship: opens Power from the systems rail, sends ENG/WEP/SEN/AUX to DRAW, waits for fill;
- checks readings at each step, and fails a step if its control is blocked on screen;
- films the run with Unity Recorder (package `com.unity.recorder` 5.1.7, owner's choice).

Each run writes `Logs/Playtests/<date_time>/`: `playtest.mp4`, `report.md` (pass/fail per step)
and `shots/`. It leaves play mode and reopens the scene you were in. Only the newest two runs keep
their video and screenshots (Step 143). Every `report.md` stays. The newest full run,
`Logs/Playtests/2026-11-11_015627` (Step 197), had 257 checks: 256 passed and #194 failed. Add
steps to `PowerUpRun()` as features are added. There is only one Unity editor, so it runs when no
other agent is using Unity.

What it can't do yet: the intro menu and the briefing's Enter read Unity's old Input class, which
a script cannot press, so it calls what those presses call (`Activate(1)` for SKIRMISH, `ScreenFlow.Advance`)
and says so in the report. Board controls are clicked for real, through the EventSystem.

Traps found setting it up: the recorder needs a moment after it stops before play mode ends, or
the MP4 has no index and won't play; and it must record at a **constant** frame rate — variable
rate fed the Mac encoder duplicate timestamps and it dropped frames. The playtest films the Game
view at its own 16:9 size (owner, 2026-10-26). MP4 needs an even width and height, so an odd window
is trimmed by one pixel. The playtest notes the Game view's size choice before the run and puts it
back afterwards.

**Game view size (owner, 2026-10-18).** Use **16:9 Aspect** in the Game view's size menu (top
left of the Game view). The board draws at the window's real pixel size. Since Step 182 the battle
board fills any screen shape, but playtests and checks stay at 16:9. A fixed size like
1920x1080 gets shrunk to fit. For a bigger view, set the Game view's **Play Focused** menu to **Play
Maximized**. For true full screen, make a build: Player Settings are already Fullscreen Window at
the display's native resolution.

### `TacticalWorld.cs` and `WorldContact.cs` — new in Step 34

| File | On what | What it is |
|---|---|---|
| `Assets/Scripts/Tactical/TacticalWorld.cs` | the `World` root object | Runs the world. Loads the level and puts each ship at its spawn. Each board tick it lets each enemy brain give its orders, moves the enemy ships, and writes each contact's true bearing and range into our ship's contact list (`tracks`). |
| `Assets/Scripts/Tactical/WorldContact.cs` | each contact in the World | Has `trackKey`, the board track it feeds. Makes the contact's ship state from its eight parts. |
| `Assets/Scripts/Tactical/EnemyBrain.cs` | `Garuda · Cruiser` and each Naga frigate | Steps 51 and 97. It hunts the map, hears us by our signal, searches, and on ALERT turns a side to us and fires railguns and missiles. |
| `Assets/Scripts/Tactical/ShipView.cs` | `Ship View Camera` and `Contact View Camera · Garuda` | Draws a ship's model for the plot. |

Changed for it:

- `BoardManager.Ticked` — fires once per board tick, after the state ticks, before the
  panels redraw. The world steps inside the board's single timer.
- `ShipState.KeepTracks` — drops tracks with no world contact, and prunes identified and
  classified.
- `PlotPanel` — draws identified hostile contacts at their true km position. Since Step 75,
  a contact out of visual range is drawn where the sweep last touched it. In visual range it is
  drawn as its model.

Details in section 6, Step 34.

### The Level tool — new in Step 67

| File | On what | What it is |
|---|---|---|
| `Assets/Scripts/Tactical/EnemySpawn.cs` | a node in a level scene | Where an enemy ship starts. A box in the Scene view only, never on the board. Step 114: Hunts (on by default) sets whether that ship's brain hunts us. |
| `Assets/Editor/LevelWindow.cs` | menu **Starwolf → Level** | Lists, opens and makes levels; adds an enemy spawn, the one Player Spawn (Step 73), a Debris Field (Step 77), the Battle Space box (Step 91) and a Mission box (Step 114). Shows the open level from above: the battle space's edge, debris as dots, the spawn boxes. Pick a debris field to see its settings and a preview of Scatter. |
| `Assets/Scripts/Game/DebrisField.cs` | a node in a level scene | Step 77. A shape filled with pieces; the pieces are saved under it. `Layout()` places them from the seed. Nothing runs at play. |
| `Assets/Scripts/Game/GasCloud.cs` | a node in a level scene | Step 129. A box with a width and height and no collider. `ShareInside` says how much of a ship's box is in it. `TacticalWorld` finds the clouds; `PlotPanel` draws them. |
| `Assets/Editor/DebrisFieldEditor.cs` | the Debris Field's inspector | Step 77. Scatter and Reshuffle; `Check()` is the layout's own test. |
| `Assets/Scripts/Tactical/PlayerSpawn.cs` | a node in a level scene | Step 73. Where our ship starts: a white box in the Scene view only. At play our ship starts there facing its way (`TacticalWorld.SpawnPlayer`), and the plot opens on it (`PlotPanel.CentreOnOurShip`, which the C key uses too). One per level. |
| `Assets/Scripts/Tactical/BattleSpace.cs` | a node in a level scene | Step 91. The level's battle space box: its width and height. At play it sets the board's battle space. |

### Terrain prefabs — new in Step 73

All in `Assets/Prefabs/Terrain/`, all on layer `WorldTerrain` (slot 10). Each is 1 unit along its
longest side; a level stretches each copy to its size in units. Placeholder shapes: change a piece's
mesh and every copy changes. No script on any of them.

| Prefab | What it is |
|---|---|
| `Pieces/Girder`, `Pieces/Plate`, `Pieces/Block`, `Pieces/Drum` | The kit: Unity cubes and a cylinder, one collider each. |
| `BaseHusk` | The broken starbase, ~13,000 × 12,000 km: 60 kit pieces in groups (Core, Ring, Spokes, Dock arm, Drydock, Spine, Pylon, Antenna, Debris). |
| `Rock` | The meteor model (`3D Assets/metore.fbx`), capsule collider. |
| `Wreck_Carrier`, `Wreck_Cruiser`, `Wreck_Battleship`, `Wreck_Corvette` | `UpdatedStarships` models laid flat, top toward the plot camera, box collider. Whole ships. |

`TacticalWorld` loads the level at play (`levelScene`) and moves a contact to its spawn (`Spawn`).
Details in section 6, Step 67.

### The plot grid

The board's grid is drawn by `PlotGrid` (`Assets/Scripts/UI/PlotGrid.cs`), a UI
mesh in the Plot prefab. The old board's `TacticalGrid.shader`, `TacticalGrid.mat`, `PlotAnnotation.cs`
and `MissionClock.cs` were deleted on 2026-10-29.

### Old things the board does not use

Soham's code. Keep it until the owner decides.

| Thing | Why we keep it |
|---|---|
| `WorldBoundary` | Not used. |
| `PowerManager` | No tactical scene uses it. Its five systems clash with the new board's four buses. **Not revived for `02.1_Grid`** (decision 16). Its class for one power bar is `PowerChannel`. |
| UI shaders: `UI_PowerArrow`, `UI_ShipHack`, `ScanLines`, `UI_GradeDisplay*` | Not used. |
| Rail icons in `Assets/Art/Icons/` | 23 icons from the old board. The board doesn't use them. |

Checked 2026-11-11: Soham wrote 42 of the 109 scripts. None of them is in any scene the game uses,
and none of the newer scripts calls them. This was checked against git history, the game's scenes
and prefabs, and the code. `ControllerHaptics` starts itself when the game opens and keeps running
the whole time, but only Soham's old ship and gun scripts ask it for a rumble, so in the game it
never rumbles. `ReadOnlyDrawer` (editor only) was started by Soham, but most of it is new now. It
greys out `[ReadOnly]` settings in the Inspector and draws the scene slots in the Campaign file.
`GameManager.cs` has to stay because `ReadOnlyDrawer` needs a small part of it, and the rest of it
does nothing in the game. `QuestSystem` started as Soham's script, but it was rewritten for the
battle board in Step 85, and only 16 of its 425 lines are still his. His other 39 scripts are only
on old prefabs (`PlayerShip`, `EnemyShip1`, `EnemyShip2`, `Canvas`, `Weapon_Laser` and `Missile`),
and those prefabs only appear in older test scenes that aren't in the build.

### Things to leave alone

| Thing | Why |
|---|---|
| The old cockpit `UIController` | **Keep it out of the new scenes.** Its `Awake` destroys its own GameObject when it finds a duplicate, and that deletes the player ship. |
| `GameManager.cs` | **Never delete this file.** `ReadOnlyAttribute` is declared at the bottom of it. Removing the file breaks `[ReadOnly]` everywhere in the project. |

---

## 4 · Decisions already made

These are settled. Do not reopen them.

1. **Three separate scenes, not one scene with canvases toggled on and off.**
2. **New code for the scene flow.** The scene flow uses existing managers where it needs them, and
   does not edit them. `QuestSystem` is the one exception (decision 3).
3. **`QuestSystem` was rewritten for the board in Step 85** and moved to `Scripts/Game`. Its stages
   (locate, eliminate, exit) are kept.
4. **The old cockpit `UIController` stays out.** See the table above for why.
5. **`GameManager.cs` is never deleted.** See the table above for why.
6. **Legacy input for these scenes** — `Input.GetKeyDown`. Both input backends are enabled in this
   project, so both compile. This flow picks the legacy one deliberately.
7. **Canvas setup for the intro scenes:** Screen Space **Camera**, CanvasScaler on Scale With Screen
   Size, reference resolution 1920x1080, match 0.5. (This started out as Overlay in the Step 1 plan.
   It was built as Screen Space Camera and that is what it stays. See Known issues #2 for the catch
   that comes with it.)
8. **Colours come from the board's palette file**, `Assets/UI/Board/Palette.asset`. Every panel reads
   its colours from it, and the spec lists them (§11.5). Do not invent new ones. A new colour goes
   into the palette file first.
9. **The plot's grid is `PlotGrid`, a UI mesh in the Plot prefab** (`Assets/Scripts/UI/PlotGrid.cs`).
   The old world-space grid, `TacticalGrid.shader`, was deleted on 2026-10-29.
10. **The scene stays 3D top-down**, so collision and physics work later without rework.
11. **Our ship's weapons and power** (owner; Step 170 replaced the handoff's set). One spinal railgun
    on the nose, four PDC groups and two torpedo launchers. No fighters. Four power buses: ENG, WEP,
    SEN and AUX. The enemy uses the same rules with different numbers.
12. **Grid tint toggles white and amber**, not white and red. Something to tune.
13. **Distances are units** (owner, Step 111). At zoom x1 a small square is 50 units. The code says
    units everywhere (Step 113). The screen still shows km, and what the player reads is decided
    later. Our ship is 200 x 50 units, the Garuda 350 x 100 and the frigate 100 x 25 (Steps 126 and
    127).
14. **The handoff is built in order:** static board first, then movement, then interactions with
    no game wiring, then the wiring.
15. **The board loads undamaged** (all green, rooms sealed) **and is gamma-space only.** See
    `UI_INTEGRATION_LOG.md`, section 3, and its Step 31 (deleted; `git show 12ccdd1:Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md`).
16. **A new World object owns the game state and feeds the board** (owner, 2026-10-17). The old
    `Radar.cs`, `EnemyShip`, `PlayerShip` and `PowerManager` scripts are not revived.
17. **Wire one feature at a time** (owner, 2026-10-17). Plan each feature with the owner. Don't
    invent: follow the sources. Each feature must work before the next starts.
18. **The enemy is the Garuda** (owner, 2026-10-17, then Steps 95 and 97). It began as a still dummy
    called the North Star. In Wolf Hunt the Garuda starts at a random point in its spawn box, hunts
    us, and fights back with railguns and torpedoes.
19. **Ships are real 3D objects seen top-down** (owner, 2026-10-17), so future external viewport
    cameras can look at them. Their real scale is decided when those cameras come.
20. **Power makes the ship loud** (owner, 2026-10-17). Every unit drawn to a bus adds signal, and so
    does ACTIVE SCAN. The internal value is **signal**, on a 0–100.0 scale. Every amount is an
    Inspector field. It climbs and falls gradually, and never jumps straight to 100. The player sees
    only a whole percent. Details in Step 35.
21. **Nothing real changes while paused** (owner, 2026-10-17). The board tick, the world and the
    mission clock all wait until RUN. Controls still work.
22. **Plot marks stay put in space** (owner, 2026-10-17). POINT is a timestamp, RULER a distance,
    COMPASS sits around our ship. Plots feed fire control later. Details in Step 36.
23. **Every number you can tune is in Starwolf → Tuning** (owner, 2026-10-17). Its tabs include Ship,
    Enemy, Board, Keys, Hard Points and Ranges. `[Tune("Section")]` marks board values only. Ship
    numbers go in each part's `Stats`, in the ship's stats file. Art is not in Tuning: it is in
    Starwolf → Art (decision 31).
24. **Nothing standing on the plot** (owner, 2026-10-18). No always-on labels. Readouts come from
    using a tool. The PDC arcs show once the PDCs are activated; the alarm strip shows only for a
    real alert. Details in Step 39.
25. **Switches slide, refuse and click** (owner, 2026-10-18). Rocker pads slide as the design
    animates them. A locked switch nudges when clicked (owner addition). Click sounds come from the
    prototype. Unity audio is on. A refused order keeps the design's reaction, plus the refused
    sound. Details in Step 40.
26. **Our ship moves across a fixed grid** (owner, 2026-10-18). Its top speed is 50 units a second at
    ENG 5 and 25 at ENG 1 (Step 130). You pan the plot by dragging the grid or with the arrow keys.
27. **Plotting feeds the firing solution** (owner, 2026-10-23). A heard ship gives a bearing only. To
    find its range, plot it (POINT, RULER) or scan it. TRACK takes the plot: the newest POINT and the
    newest POINT-to-POINT RULER (Step 106). Otherwise a lock gives the solution (Steps 125 and 141).
28. **A railgun shot takes time to arrive** (owner, 2026-10-23). Our ship's spinal railgun fires a
    slug at 1,500 units a second, with a reach of 30,000 (Step 170). The numbers are in the Weapons
    group of each ship's stats file. A shot hits a ship's box made 25% bigger (decision 47).
29. **Fighters** (owner, 2026-10-23). Our ship has had no fighters since Step 170. The rules stay for
    any ship that has them: they fly at 100 units a second as small white triangles, appear beside
    the ship (Step 160), make attack runs, and come back when RECALL is pressed.
30. **PDCs: arm the ladder for AUTO, a chance per missile; the enemy fires back when hit** (owner,
    2026-10-24). Enemy PDCs wait for the ship AI. Details in Step 55.
31. **The Starwolf menu holds only the owner's tools** (owner, 2026-10-25): **Tuning**, **Level**,
    and later **Mission** (the mission scripter). Build and test helpers don't get menu items. The
    rest of the `Starwolf` and `Tools/Starwolf` menus are removed as the board refactor reaches them:
    - **Tuning** has three tabs: **Ship**, **Enemy**, **Board**. Numbers only. Ship and Enemy each
      edit that ship's stats file (a ScriptableObject, one per ship, same fields).
    - **Level** is a separate tool: where things are (debris, spawn points, the battle space).
    - **Art:** on each panel's own inspector. Panels are prefabs. Replaces the board builder and
      the import menu. When the prefabs are made (owner, 2026-10-25):
      - Each object is named after its control ("LOCK rocker"), not its export record
        (`sprite#178 rocker_30x38_locked`).
      - Each control's parts (body, pointer, lamp, label) go under one parent, so they move together.
      - Once the prefabs exist, one guide for UI artists is written. It covers where each panel's
        art files are, how to swap a sprite, font or colour, the import settings, and the two
        export README rules that do not apply here (sRGB, token hex).
    - **Checks:** the playtest and the intro's letter check run in Unity's Test Runner.
    - **Deleted outright:** `Starwolf/Tuning/Apply` and `/Capture` (nothing reads that asset), Soham's
      `Wire UI References` / `Wire Button OnClicks` (old UI only), `Tools/Starwolf/Create Intro
      Scene` (the intro lives in `00_Start`).

    Now: the menu has `Starwolf/Tuning`, `Starwolf/Level`, `Starwolf/Art` (Step 185),
    `Starwolf/Campaign` (Step 205) and `Starwolf/Playtest/Run power-up playtest`. Keys, Hard Points
    and Ranges are tabs of Tuning (Step 129). Art is changed in the Art window, never in Tuning
    (owner 2026-11-10). The panels are prefabs, and the guide for UI artists is `docs/BOARD_UI.md`.
    The checks are not in the Test Runner yet.
32. **The map has edges; the grid still resizes** (owner, 2026-10-25). The battle space is a
    rectangle with its own width and height. Zooming out stops at the Zoom Out Limit (x0.35), or
    sooner where its **left and right** edges reach the plot's sides, never the top and bottom: no
    black down the sides, and you pan up and down for the rest. Leaving it starts a 30 game-second
    countdown. Details in Step 65. Now: the limit is x0.058, where the whole map fits the plot's
    height (Step 130).
33. **Each level is its own scene, loaded on top of the board** (owner, 2026-10-25). Levels are in
    `Assets/Scenes/Levels` and are made and opened with **Starwolf → Level**. The enemy spawn is a
    box seen only in the Scene view. Debris will be the same every play; it changes only on
    Reshuffle. The mission scripter will be its own menu item, later. Details in Step 67.
34. **Our ship never blinks; the signal warning is on the frame** (owner, 2026-10-25). The ship on
    the plot stays solid. The signal shows in a box in the frame's top-right, always visible, and the
    alert tone plays once as it crosses the tracked line. Details in Steps 66 and 68.
35. **The metal frame is in front** (owner, 2026-10-25). Anything a button opens should slide out
    from behind it: clipped to the frame's opening, under the rails and stations. Done for the six
    drawers. Helm, Orders and the periscope don't slide yet (Open items). Details in Step 66.
36. **Spoken orders, once the board is finished** (owner, 2026-10-26). The captain can speak orders
    ("come about to port", "all ahead full", "sweep bearing 045", "sweep there" while pointing) with
    push-to-talk. Voice sits beside the buttons, never replacing them. The crew takes a moment to
    carry an order out and answers with a quiet "roger". Speech is heard offline, on the player's
    machine. Firing stays on buttons. Not built yet. Until then, every action keeps going through
    `BoardManager.Click`, so a spoken order can use the same path and its refusals.
    Details in `docs/voice-command-exploration.md`.
37. **Engineering shows side armour on the ship picture** (owner, 2026-10-26). The picture is our
    ship in four parts, one per side. Each part is a bright outline with its panel lines in grey (Step 75). Each
    part is white while its armour is full, amber once hit, red once open. No compartment dots: the
    room rings show the rooms, white, amber or red by health. The plot ship stays plain white.
    Details in Steps 74 and 75.
38. **Ruins are in the level, sized for play** (owner, 2026-10-25/26). Placed copies go in the level
    scene, and the prefabs are shared in `Assets/Prefabs/Terrain/`. How ruins are drawn on the plot,
    and when they count as found, is the same in every level and is set on the board. Ruins are sized
    for how the ship plays, not for the models. `Level_01` has had no starbase husk since Step 107.
39. **Ship names** (owner, 2026-10-26). Our ship is an **Asura-class destroyer**, not "the
    Starwolf" (that is the game's name). The enemy is a **Garuda-class cruiser**, not "TRACK 07":
    once scanned it reads **GARUDA · CRUISER**; before a scan it stays **BOGEY T07**. Its model is the
    `UpdatedStarships` cruiser. Not applied yet: see Open items. Now: applied (2026-10-26). Track
    t07 is named GARUDA, the contact is `Garuda · Cruiser` with the `UpdatedStarships` cruiser
    model, and the spawn is `Enemy Spawn · Garuda`.
40. **Motion and sight** (owner, 2026-10-26).
    - Our ship glides smoothly across the plot.
    - An enemy known only through DRADIS shows where the sweep last touched it. It moves on the plot
      only when the sweep passes it again.
    - Visual range is 10,000 km. Inside it, the enemy's own model shows on the plot and glides. It is
      red once identified. Until then it is amber and labelled **BOGEY T07**, and you cannot target
      it.
    - A destroyed ship stays on the plot. In visual range its model turns grey and drifts. Outside
      visual range, a dim dot drifts.
    - DRADIS at full gain reaches 30,000 km. (The weapons' reach is now decision 42.)
    - These are starting numbers, to be tuned. Details in Step 75.
    - Now: visual range 2,000 and DRADIS 5,000 at gain 1 to 7,500 at gain 5 (Step 130). A killed ship
      in sight breaks in two, and a wreck's dot fades over 120 game seconds (Step 131).
    - Now: a heard ship that is not scanned is a red bar on the DRADIS circle. The bar turns on every
      frame as either ship moves (Step 161).
41. **Axis numbers measure from our ship** (owner, 2026-10-26). The numbers on the top and left
    edges of the plot read 0 level with our ship, then 5,000, 10,000 and on, in km. They move with
    the ship and spread out as you zoom in. There are no axis titles. Details in Step 76. Now: every
    100 units at x1, further apart as you zoom out (Step 135).
42. **Each weapon has a separate reach** (owner, 2026-10-27). It is in the Weapons group of each
    ship's stats file (`WeaponSystem.Stats.mounts`), and the plot's weapon ring follows the selected
    mount. Our spinal railgun reaches 30,000 units (Step 170).
43. **Ammunition is a real count** (owner, 2026-10-27). In a mission it only goes down: there is no
    reload. Each railgun shot takes one slug. Each go a PDC group has at a torpedo takes 100 rounds.
    Each torpedo takes one cell. With nothing left, LOAD refuses. Outside a campaign every mount
    starts full, from the ship's stats file. In a campaign, what is left is kept for the next mission
    (Step 196), and the ship's rooms can restock it (Step 198).
44. **Boarding goes on while the target moves** (owner, 2026-10-27). Once the marines are aboard,
    the ship keeps moving and fighting until they win or lose; the boats ride with it. The boats
    go no further than **15,000 km** from our ship. Outbound past that, they turn home. Coming
    home out of range, a clock runs with a warning; you must close in to collect them. If the
    clock runs out, the boats and their marines are lost. Details in Step 79. Now: the boats go no
    further than 4,000 units (Step 130).
45. **Each ship's numbers are in its own stats file** (owner, 2026-10-27). `Assets/Ships/Asura`
    (ours) and `Assets/Ships/Garuda`. **Starwolf → Tuning** has six tabs: **Ship** (our file),
    **Enemy** (the Garuda's file), **Board** (the plot, sounds and clock), **Keys** (Step 108), **Hard
    Points** and **Ranges** (Step 121). A ship's file is
    grouped by ship part: navigation, power, sensors, weapons, hangar, damage, orders. Change a
    number in play and the game takes it at once, and the file keeps it. The Garuda uses its own
    damage numbers; it no longer copies ours. Details in Step 80.
46. **The railgun slug is the fastest thing on the board** (owner, 2026-10-27). Its speed is in the
    Weapons group of each ship's stats file. Our spinal railgun's slug flies 1,500 units a second
    (Step 170).
47. **A shot hits a ship it passes within 100 km of, any ship** (owner, 2026-10-27). First the owner
    said a slug must hit the ship itself: you lead the target, fill in the targeting computer, get
    the lock and fire. Real ships are 100 to 500 m long, though, and the BRG knob turns in whole
    degrees (about 190 km at 11,000 km). The owner chose 100 km for play, with the knobs as they
    are. It replaces the hit radius by class (frigate 95 km up to carrier 600 km) and the 210 km for
    shots at our ship. Slugs and missiles alike. Built in Step 88. Now: a shot hits the ship's box made
    25% longer and 25% wider (Steps 123, 127 and 128).
48. **No power on SEN, no hearing** (owner, 2026-10-25; built in Step 82). DRADIS hears only with
    at least 1 unit on the SEN bus. With none, the plot shows no circle and no sweep. The DRADIS
    switch stays where the player set it.
49. **A hit's damage comes from the weapon that fired it** (owner, 2026-10-27; built in Step 88).
    These numbers are the firing ship's, in its Weapons group: slug armour damage (50), slug or
    warhead damage to the room behind (2), HE armour damage (100), EMP time (60 s), the fire chances
    (HE 60%, penetrator 40%) and the slug's breach chance (50%). Fighter damage was already the
    shooter's (Hangar). These stay with the ship being hit, in its Damage group: armour per side,
    the reactor's radiation chance, how fire, vacuum and radiation spread, crew losses and repairs.
    Tune how hard our weapons hit on the Ship tab, and the Garuda's on the Enemy tab.
50. **Each level has its own battle space** (owner, 2026-10-27; built in Step 91). A Battle Space box
    in the level, like the spawn boxes, with a width and height in km. The Level window sets them
    and its map shows the edge. The board uses them when the level loads. A level without a box
    uses 100,000 × 100,000 km. Now: every level is 20,000 × 20,000 units (Step 130).
51. **Crew numbers are tuning numbers, in each ship's stats file** (owner, 2026-10-29: "crew numbers
    need to be a tuning variable and should not be hard-coded"; built in Step 107). The Damage group
    has **Crew**, one number per room. Never put a crew number in code.
52. **Engineering and the orders station charge the same AUX for the same order** (owner, 2026-10-29,
    bug 6; built in Step 107). REPAIR 30, SEAL 25, VENT 15, from the ship's stats file. REBOOT and
    RECALL are free.
53. **A big square is 10 x 10 small squares, and the small lines fade as you zoom out** (owner,
    2026-11-01; built in Step 124). This replaces "a big square always holds 5 x 5" and the squares
    that changed worth as you zoom. A small square is 50 units and a big one 500, at every zoom. Our
    ship is 200 x 50 units. Zoom runs from x1 to x0.1. Lines are counted from the world's centre. The
    designer's third size, 5,000 units, was left out: it is only needed past about x0.04. Now
    replaced: a big square is 5 x 5 again, the squares step 50, 100, 200, 500 and 1,000 units as you
    zoom, and zoom runs to x0.058 (Steps 130, 135 and 139).

54. **Notes go in `docs/`** (owner 2026-11-10). Never make a .md note inside `Assets`. Add to a note that exists first. Step 187.
55. **Every script starts with a plain-English note** (owner 2026-11-10). It says what the script does and how it works. It goes above the script's `using` lines. The rules and the layout are in `docs/script-notes/BRIEF.md`.
56. **Codex's Fire Control** (owner 2026-11-10). FIRE on the systems rail opens it as a drawer. It never stays fixed on the side. The periscope's red view goes inside its monitor, and the periscope at the top goes. A big FIRE button that glows FIRE FIRE FIRE replaces the drag lever. So far it is built in the test scene only (Step 193).
57. **Tests stay in their test scene** (owner 2026-11-10). Once a test scene is made for some work, every follow-up for it goes there. Change `02.1_Grid` or a level only when the owner names that scene.
58. **Never stretch painted art** (owner 2026-11-10). On other screen shapes, corners keep their size and plain pieces of the edges repeat (Step 182).
59. **No more videos** of checks (owner 2026-11-10, while MILK RUN was being built: "you're filming
    junk videos").
60. **STARWOLF is never in the bottom half of the screen** (owner 2026-11-11). On the title it is in
    the middle. An end card with a closing line must not put STARWOLF below the line.
61. **Only the board labels the owner names get changed** (owner 2026-11-01). The words are real
    military words, short and clear. The owner turned down THROW THE SWITCHES, NOT YET and LOUD · NOT
    HEARD YET. Don't use those or anything like them. The labels changed so far are in
    `docs/BUILD_HISTORY.md`, under "Review of the game, and the owner's answers (2026-11-01)".
62. **The small number box on the Hangar panel can't stop a launch** (Step 149). The owner did not
    recognise the box. Do not ask about it again.
63. **A rock is silent until ACTIVE SCAN pings it** (owner, Step 157). There are no false dots near
    rocks, for us or for the enemy. Do not ask about it again.
64. **New board pieces fit any screen shape** (owner 2026-11-10, Step 182). A new panel sheet is
    pinned to its screen edge. A new drawer under Drawers has anchor right-middle and pivot (1, 0.5).
    A frame picture repeats and never stretches (decision 58). See `docs/BOARD_UI.md`, "Screens of
    other shapes".
65. **When a ship's size changes, move its hard points to the new edges too** (2026-11-01).

## 5 · Known issues and traps

Traps 1 and 13 are fixed. They are in `docs/BUILD_HISTORY.md`.

2. **TextMeshPro made from a script shows nothing at all.** This one bit us. If you set the font by
   writing `m_fontAsset` through serialization, `m_sharedMaterial` stays null and `characterCount`
   stays 0 — you get an object with no visible text. The font's material is a sub-asset inside the
   font file, so it does not come along for the ride. Set it through the C# properties instead:

   ```
   tmp.font = font;
   tmp.fontSharedMaterial = font.material;
   tmp.ForceMeshUpdate(true);
   ```

   The old project font is `Assets/UI/Fonts/Optimus SDF.asset`. The board's fonts are in
   `Assets/UI/Fonts`, one folder per font (Step 89).

3. **Children of a Screen Space-Camera canvas come out the wrong size.** The canvas itself sits at
   scale 0.01, and anything created under it while it is in that mode inherits a nonsense
   `localScale` — we got 132.86 in the briefing scene and 1.59 in the start screen. `00_Start` was
   quietly 59% too big before we caught it. **Fix: set every UI child's `localScale` to
   `Vector3.one` right after you create it.**

4. **Scripts can't press old-Input keys; the playtest calls what the key calls. Keys 1–6 not yet
   pressed by hand.**

5. **Unity UI draws children on top of their parents.** So a frame that is a child of the thing it
   frames will cover it. The corner panels had this exact bug in Step 6. `Frame` and `Screen` must be
   **siblings**, with `Frame` first in the hierarchy.

6. **This project renders in Linear colour space.** Anything you hand to `Material.SetColor` is
   treated as a linear value. Pass an sRGB number straight in and it comes out washed out — ours went
   grey-green. **Fix: append `.linear` to every colour you set from code.** This will bite again on
   any new material.

7. **Do not unzip anything in `References/`.** The design handoff zip gets extracted to the session
   scratchpad and read from there. `References/` is owner-authored input and is never edited.

8. **Reparenting a UI subtree collapses it to 0x0 unless you set `sizeDelta` yourself.** The
   original objects sized themselves from stretch anchors against a parent they no longer have. Set
   the size the layout groups expect, then `LayoutRebuilder.ForceRebuildLayoutImmediate`, then
   `Canvas.ForceUpdateCanvases()`, then rebuild again. This cost time in Step 13.

9. **Unity rescales a non-power-of-two texture on import.** `frame_metal.png` came in as 2048x1024,
   which broke the exact 1920x1080 mapping the frame bands depend on. Set
   `npotScale = TextureImporterNPOTScale.None`.

10. **Setting `textureType = Sprite` on its own produces no Sprite subasset.** `spriteImportMode`
    must also be set to `Single`, or `LoadAssetAtPath<Sprite>` returns null and tells you nothing.

11. **An asset reimport can bounce the editor back into play mode.** A scene build ran during that
    and was silently discarded. **Check `Application.isPlaying` before any scene edit**, not just
    once at the start of a session.

12. **`PlayerSettings.runInBackground` must stay on.** With it off, play mode freezes solid whenever
    the editor loses focus — `Time.frameCount` sat at 1 across repeated probes. **This is why an
    automated play-mode test looks like it does nothing.** Even with it on, driving play mode over
    time from outside the editor proved unreliable. Held keys have to be checked by hand.

14. **Keep QA evidence in `Logs/`, not `Temp/`.** `Logs/` is git-ignored and survives Unity
    restarts. `Temp/` does not.

15. **The board loads PAUSED.** Since Step 35 nothing real changes until RUN: no fills, no charge,
    no signal, no clock. A check that forgets to press RUN sees nothing move.

16. **The reactor serves buses first come, first served.** A bus sent to DRAW late can find it
    already spent. The playtest sets every bus to DRAW while paused, so the reactor shares out evenly.

17. **A playtest control with no argument is saved as "", not null.** Check for empty, not null.
18. **Never call `InternalEditorUtility.ReadScreenPixel` from MCP `execute_code`.** On this Mac it
  crashed Unity outright (NSBitmapImageRep, `cgImage != NULL`) while a checker tried to photograph
  the tuning window (2026-10-18). Use the Game view or a camera render texture for pictures.
19. **An editor window must draw the same controls in Unity's Layout and Repaint passes.** Decide
    what to draw on the Layout pass only (play mode, the list). The tuning window broke this once
    when the playtest entered play between the two passes.

20. **The playtest video shows eased motion unevenly.** Board motion follows real time
    (`Time.unscaledTime`), so it keeps moving while the board is paused. The recorder spaces video
    frames on its own clock, so a 0.14 s slide shows as a few small steps and then a jump. In play it
    is smooth: logged frame by frame in Step 40. Judge feel in play, not from the video.

21. **A playtest screenshot shows the end of the frame.** `ScreenCapture` writes the frame once
    it is finished, so anything the playtest does before its next wait lands in the picture. Wait
    after a shot (`yield return 0.2f`) before the next action. Step 42 caught a "DRADIS on"
    shot that showed DRADIS off.

22. **A control can have more than one click target.** A drawer has its rail key and its own latch,
    and the latch stays active while the drawer slides away. Run `2026-10-23_090517` and run
    `221323` failed "blocked: topmost … is nothing": the playtest clicked the FIRE latch while it
    was still off the right edge, closing. (I first blamed someone touching the Unity window; that
    was wrong.) Since Step 53, the playtest's `Click` tries every matching target and uses the
    first one on top at its centre.

23. **The periscope classify check ("Scanned, the targeting computer shows its hull") can fail by chance.**
    It failed once (`2026-10-25_230225`, "NO SUBJECT ON BEARING") and passed on a re-run with the
    same scene and code. The periscope names a ship only if it is heard and on bearing, and the
    run's power spread varies a little with timing (that run had AUX at 2 units, not 3). If it
    fails, re-run before hunting a bug.
24. **Engineering's hull pictures are four images in `Assets/Art/Board/Hull/`.** They are separate
    from the `ShipView` render. Never colour `ShipView`: the plot uses it, and the plot ship must
    stay white. The images must import with sRGB off, or they come out the wrong brightness. To
    remake them for a new model, see Steps 74 and 75.
25. **A ship's model can be up to one tick ahead of the ship (Step 75).** Between ticks, each model
    moves on along its ship's velocity. Where a ship is, and which way it faces, is
    `NavSystem.x`, `y` and `heading`. A rule must read those, or the track's `worldX` and `worldY`
    (`TacticalWorld.Sync` copies them from `NavSystem`). Never read a ship's transform for a rule.
    The same goes for a shot's or fighter's `drawn` (Step 138): it is for the plot only; rules read `x` and `y`.
26. **The playtest can switch off Unity's background shader loading (found 2026-10-26).** The
    playtest films each run with Unity's Recorder. While filming, the Recorder switches off
    **Asynchronous Shader Compilation** (Edit → Project Settings → Editor), and switches it back on
    when filming ends. If a run is cut short, it stays off, and Unity saves "off" into
    `ProjectSettings/EditorSettings.asset`. The project's setting is **on**. If `git status` shows
    that file changed, switch it back on in Unity. Changing the file while Unity is open doesn't
    stick.
27. **Scene and prefab files still show the old "Handoff" script names (Step 90).** They are in the
    `m_EditorClassIdentifier` lines. These are only labels. Unity links each script by its GUID,
    and the GUIDs did not change. Unity rewrites a label when that part is next edited. Don't
    hand-edit them, and don't search the YAML for a script by these labels.
28. **Seven font files rewrite themselves whenever Unity runs:** the Barlow (Medium, SemiBold), Share
    Tech Mono, Noto, Sixtyfour and Electronic Highway Sign SDF assets, and TextMesh Pro's
    `LiberationSans SDF - Fallback`. They show as
    changed in every `git status`. Put them back; don't commit them. A lasting fix (a TextMesh Pro
    setting) is for later.
29. **The close-up zoom number in `PlotPanel.cs` is not the one the game uses.** The code says 1.5.
    The Plot prefab sets 1.1, and the prefab wins. Change it on the prefab.
30. **The `[ReadOnly]` mark for the Inspector is in `Assets/Scripts/GameManager.cs`,** an older
    script. Look there for it.
31. **An Art list puts the old picture back.** You change an image's sprite by hand, but a panel's Art list (or an `ArtList`) has an entry for that image. The editor puts the entry's sprite back. Change the sprite in the Art list entry instead. Codex's frame did not show as delivered for this reason (Step 193).
32. **Two Fire Control panels in one scene fight.** The board gives a press and a drag to the first panel that takes it. A second Fire Control takes the TRACK press and the lever from the one you see, even when it is switched off. Delete the old one (Step 193).
33. **In the test scene `Fire Control Codex`, the periscope is inside the Fire Control drawer.** With SIGHT down, closing the drawer leaves the periscope down out of sight, and a lock can still build. Raise SIGHT before closing. Not fixed: the board's own Fire Control has no periscope inside it.
34. **The campaign save matches supplies and crew by name.** It keeps rounds by each gun's or launcher's label, and crew by each room's name. Rename a label or a room, and what was saved for it is lost. That gun or room starts the next mission full (Step 196).
35. **The actions in the ship's rooms find their room by its name.** The repairs go in ENGINEERING, the torpedoes and ammo in HANGAR, and the marines in QUARTERS. The name is each room's **Name** in the Rooms list on Ship Rooms, in `Ship.unity`. Rename a room, and it loses its actions (Step 198).
36. **Unity keeps old values in a loaded prefab after a recompile.** A new default in a script does not
    reach a prefab that is already loaded, even when the prefab file doesn't list the field. The plot's
    edge stayed white in play mode until `Plot.prefab` was saved with the colours in it (found with the
    plot zoom-out, 2026-10-29).

---

## 6 · Steps 6 to 28, and 34 to 118, in detail

Moved to `docs/BUILD_HISTORY.md`, with the step table.

---

## 7 · How to keep this log

There are two files. This one, `docs/BUILD_LOG.md`, says where the game is today and what is left to
do. `docs/BUILD_HISTORY.md` keeps every finished step and every closed item. Nothing is deleted from
either file.

At the end of every step:

- Give the step the next number after the highest step in either step table.
- While the step is a test, not committed, or waiting for a check, its row goes in "Steps not
  finished" in section 1. When it is done, move the row to the step table in `docs/BUILD_HISTORY.md`.
- Write what the player sees and what changed in the step's row.
- Change section 1 so it says how things stand now. Keep it to one screen, and only what is true today.
- Change section 2 if a scene changed, and section 3 if a script, shader or material was added or
  changed.
- Add any decision the owner made to section 4, so nobody asks about it again.
- Add any trap that cost you time to section 5.
- In section 8, add what you left unfinished. Move what you finished to `docs/BUILD_HISTORY.md`, under
  the same heading, with the step that finished it.
- Every known bug goes in section 8, "Bug list", even a bug in old code. Write the bug out in full
  there, not as a pointer to another heading. When it is fixed, move it to `docs/BUILD_HISTORY.md`
  with the step that fixed it.
- Change your row in section 8's master list in the same edit: when you take an item, finish it or
  park it. A new owner ask gets a row the same day, with your chat as its owner. Nothing may be
  tracked only in a plan doc, a memory note or a chat (owner, 2026-11-11).

A docs agent also updates the other docs a step changes, such as `docs/BOARD_UI.md`,
`docs/TUNING_LOG.md` and `docs/SCRIPT_LIST.md`.

---

## 8 · Open items

Still not done. **This is the list to work from.** Read it before picking up any work.

### Master list: every open item and who owns it (Producer, 2026-11-11)

One line per item. The detail stays where the last column says. **NONE** in the owner column means no
chat is working on it: the item was dropped and needs a chat. "Parked" means the owner paused it, so
nobody starts it. A chat title in the owner column is the chat that answers for it.

Keep it current (owner, 2026-11-11). A new owner ask gets a row the same day, with the chat that owns
it. When a chat takes, finishes or parks an item, it changes the row in the same edit; a finished row
moves to `docs/BUILD_HISTORY.md`. Nothing may be tracked only in a plan doc, a memory note or a chat.
The Producer chat checks this list against the chats and the docs.

Status words: not started · designed · part built · built, not checked · waiting for the owner ·
blocked · parked.

| # | Item | What the owner asked | Status | Owner chat | Next step | Detail |
|---|---|---|---|---|---|---|
| | **The MILK RUN player build (now)** | | | | | |
| M1 | The MILK RUN player build: Mac and Windows x64 zips, named with the date | "one proper story level, MILK RUN, that is fun to play", then put it in front of people (2026-11-11) | blocked: waits on the owner's go, and on M3, M5, M6 and M7 | Mission Setup Tool | Owner says go; the chats take Unity in turn; build | "Next: the MILK RUN player build" |
| M2 | At build time: switch TEST MISSION off, and set the closing line to "To be continued." | "just 'To be continued.' It's absolutely fine." (2026-11-11) | not started | Mission Setup Tool | Both in Starwolf → Campaign, just before the zips | same; "Campaign: still to do" |
| M3 | The playtest still checks the old ship (two side railguns, six fighters), and check #194 fails every run | owner rule: one full playtest per batch | not started | **NONE: dropped** | Rewrite the checks for the nose railgun and the boats, then run it before the build | "Bug list" (playtest); "Codex's Fire Control test, and what no person has played yet" |
| M4 | Not seen by a person yet: the rooms and end card after MISSION COMPLETE; checkpoint notes 4–10, 12 and 16; the real Escape key on both menus; a full campaign win | none | not started | Mission Setup Tool (rooms, notes); the owner (by hand) | The 10-minute check before the build; the owner plays the build | "Next: the MILK RUN player build"; BUILD_HISTORY Steps 156, 189–191 |
| M5 | MILK RUN's AI flying fix: big ships stop swerving for small craft, battleship turn 12, carrier 10, shuttle routes, plain ship names | owner, 2026-11-11 | part built: saved, not compiled. The tuning log has the turn rows; the ship files don't yet | Tutorial Mission Setup | 15 minutes of Unity on the owner's go, then commit | "Open after Step 178" |
| M6 | Enemy slugs we can see leave a line on the table (Step 206) | "Only ones we can see" (2026-11-11) | built, not checked: not played, not committed | Ship Visual Artefact | 5-minute play check, then commit | section 1, "Steps not finished" |
| M7 | "Test: play from here" in Starwolf → Campaign | none written down | built, not checked: not tried, not committed | Campaign Flow | Try it in Unity, docs, commit | Campaign Flow chat |
| M8 | The bugs a MILK RUN player meets (16 in the Bug list). First: no turning with ENG at 0 | none | waiting for the owner's go on the first fix | Bug Fixes | Owner says go; then the rest in its order | "Bug list" |
| M9 | Checkpoint 13 asks for speed 45, but the helm shows no speed | none | waiting for the owner | Mission Setup Tool | Owner picks: change the note, or add a speed readout | "FLIGHT DRILL: calls for the owner to check", call 10 |
| M10 | SETTINGS on the main menu does nothing | none | not started | **NONE: dropped** | Owner picks: hide it for the build, or build it | "Triage", small jobs |
| M11 | "KM" shows after distances that are really units | "Visually, we can decide" (2026-10-29) | waiting for the owner | Bug Fixes | Owner picks the word | "Bug list"; "Distances in units" |
| M47 | MILK RUN's note says torpedoes reach 3,000. Against a moving ship, with the drift, the real reach is about 2,400 | none (fight sums, 2026-11-11) | not started | Mission Setup Tool (Tutorial Mission Setup was told) | Change the note before the build | `team-reports/player-1/2026-11-11.md` |
| M48 | Uncommitted: Player 1's report, 420 rows in `numbers.csv` and the fight program's changes (Ship Visual Artefact); our ship's speed 60 and base 30 in `Asura.asset`, with their rows in `TUNING_LOG.md` (Memory Cleanup wrote the rows) | owner rule: commit your finished work before you stop (2026-11-11) | built, not committed | Ship Visual Artefact; Memory Cleanup | Each commits its own lines | `git status` |
| M12 | Call signs for the carrier, the battleship, the corvette and our ship | "English with an Indic feel" (2026-11-10) | waiting for the owner | Mission Setup Tool | Owner picks from the list | `MISSION_SETUP_PLAN.md` "Names"; "Open after Step 178" |
| | **The sniper plan (being built)** | | | | | |
| M13 | The E-SIG circle round each ship, and the hit rule: a hit from outside it tells only the direction | "It's how much the eSig expands"; 2,500 to 20,000, +25 a shot, cools 2 a second (2026-11-09) | designed; being built | Sniper Plan | Build it, with four numbers in Tuning | `SNIPER_PLAN.md` "The E-SIG circle". The "15,000 waits for a yes" line in "The sniper ship and weapons" is out of date |
| M14 | Sensor drones in the hangar's INTERCEPTOR row. They unlock D3 (a far hit shows when a drone watches) and restock drones | "we're replacing interceptors with drones that need to be deployed" (2026-11-09) | waiting for the owner: how you deploy them (a design page is coming) | Sniper Plan | Owner picks on the page; then build | `SNIPER_PLAN.md` "Sensor drones" |
| M15 | Lasers on the two side dots | "you get in, you zap things" (2026-11-09); the sides "are going to be lasers again" (2026-11-11) | designed; the owner sees the look first | Sniper Plan | After the drones | `SNIPER_PLAN.md` "Lasers" |
| M16 | DRADIS reaches further, and a lock covers all of DRADIS (the MAG cap rises with it) | DRADIS "does need to reach a little further" (2026-11-09) | waiting for the owner: the number | Sniper Plan | Ask with the drones | "The sniper ship and weapons" |
| M17 | How drone and telemetry marks look on the plot | none yet | designed by Information Hierarchy, sent to Sniper Plan | Sniper Plan | Show the owner with the drones | "Open: the information hierarchy" |
| | **The enemy follows our rules** | | | | | |
| M18 | The Garuda, and MILK RUN's corvette, follow all our weapon rules: a lock, a charge, crew arming each shot, a slow gun, torpedoes need a scan or a lock, PDCs fire | "Otherwise this is never going to work. You have to have a pretty fair chance of winning." (2026-11-09) | not started | **NONE: dropped** | A new chat, after M13 (same hit code). Talk first: it changes MILK RUN | "The Garuda does not follow our rules"; `SNIPER_PLAN.md` "The Garuda" |
| M19 | The enemy is heard by its own signal, hides in gas clouds, fights fires in every room, turns a fresh side when one is worn | "the enemy is our ship with different numbers" (settled) | not started | **NONE: dropped** | Same chat as M18 | "The Garuda does not follow our rules"; `team-reports/enemy-ai-manager/2026-11-08.md` |
| M49 | Balance in Wolf Hunt (SKIRMISH): firing straight at the Garuda now ties for the best first move (the team target says it must never be), and at x5 nobody wins; none of 15 changed numbers fixed it | none (fight sums, 2026-11-11) | not started | **NONE: dropped** | Suggested: the Enemy Rules chat, since M18 changes these numbers; rerun the fight sums after it | `team-reports/player-1/2026-11-11.md` |
| M20 | What the enemy does past Wolf Hunt: what mission 2's enemy needs | none | parked until mission 2 is talked through | none (parked) | With M37 | "Open design questions", 1 |
| | **Damage and conditions** | | | | | |
| M21 | A hit room lowers the top power of its system, and the lost steps show crossed out. Only the reactor does this today | given with the campaign answers (2026-11-10) | not started | **NONE: dropped** | A new damage chat: write it into `DAMAGE_README.md`, then plan it | `CAMPAIGN_FLOW_PLAN.md` "A damage rule the owner gave" |
| M22 | A separate setup for conditions: which rooms touch, how fire, vacuum and radiation start and spread. Since Step 186 they still spread by the old fixed list | "over time, we need a separate conditions setup" (2026-11-09) | not started; a talk first | **NONE: dropped** | Same chat: a design page | `DAMAGE_README.md` "Deck plan"; `finished-plans/ROOM_LAYOUT_PLAN.md` "Later" |
| M23 | Torpedo blast: a torpedo that misses still hurts a ship near it | "a torpedo that misses still hurts a ship near the blast" (2026-11-09) | not started. How big, and whether it hurts us, was never asked | **NONE: dropped** | Same chat: options on the design page | "The sniper ship and weapons" |
| M24 | Rocks and ruins stop slugs | "ruins block shots" (2026-09-01) | not started | **NONE: dropped** | Same chat | "Found by two audits"; spec §8.2 |
| M25 | Damage-control teams, breakdowns and engineering's displays (damage steps 5 to 7) | "conditions first, then damage-control parties, then breakdowns" (2026-10-24) | part built: one team with a timer | **NONE: dropped** | Same chat, after M22 | `DAMAGE_README.md` "Build order" |
| M26 | A new damage type for the railgun | "once the owner says what it should be" | waiting for the owner | none (parked) | none | `SNIPER_PLAN.md`, build order 6 |
| | **Numbers through Tuning** | | | | | |
| M27 | Numbers still typed into the code: the TUNING_AUDIT list, the W tap and its curve, code defaults far below the ship files | every game number goes through Tuning (owner rule, 2026-11-09) | not started. The Bug list holds only part of it | **NONE: dropped** | Give it to Bug Fixes after the MILK RUN bugs | `finished-plans/TUNING_AUDIT_2026-11-09.md` A1, A2, B3; "Bug list" |
| | **How the ship feels** | | | | | |
| M28 | Engine flare while you hold W; thruster puffs for slide and turn (Q, E, A, D) and on a torpedo's nose swing | the owner wants to see the engine fire (2026-11-09); the A2 pick (2026-11-10) | not started; talk first | **NONE: dropped** | A new chat: a design page with the looks | "Ship movement feel" |
| M29 | Feel the 5-second slide | "let's make it 5 seconds for the slide" (2026-11-09) | built; the owner hasn't played it | the owner | Play the build | "Ship movement feel" |
| | **Between missions** | | | | | |
| M30 | Sector map: the slight flicker; the won-mission tick is still Codex's teal | the owner asked for "our phosphor effects" (2026-11-11) | not started | **NONE: dropped** (Sector Map Phosphor is closing) | Suggested: Campaign Flow, which owns the sector map | BUILD_HISTORY, Step 201 |
| M31 | The other screens fill any shape: rooms, SAVE SLOTS, the title, the sector map (owner picks option 1, 2 or 3) | "start" (2026-11-10) | not started | **NONE: dropped** | Suggested: Campaign Flow (its screens), rooms first | "Open after Step 182" |
| M32 | A mission needs torpedoes or marines, but the ship has none left after repairs | none | waiting for the owner | Campaign Flow | Owner: a minimum at the start, or the player's problem? | Campaign Flow chat |
| M33 | Room art and people (Bridge, Engineering, Quarters); the FLIGHT LEADER still talks about fighters | the owner makes the art | waiting for the owner | the owner | none | "Open after Step 185: art" |
| | **Information on the plot** | | | | | |
| M34 | The information hierarchy, the rest: start as an unknown ping; blue circles next to grey rock circles; when a ship becomes a bogey; ACTIVE SCAN as a long narrow telescope | the owner's main priority (2026-11-10) | waiting for the owner | Information Hierarchy | Owner answers on its page | "Open: the information hierarchy" |
| M35 | Look back at old comms messages | the owner was unhappy it was left behind (2026-10-29) | not started | **NONE: dropped** | Suggested: Information Hierarchy | "Comms intercepts" |
| | **Fire Control** | | | | | |
| M36 | Codex's Fire Control: does it replace the board's? Does the periscope lower itself? The FLIGHT LEADER's words? Unpack the Hangar package? | "Make a new scene for this… It doesn't have to be in the flow" (2026-11-10) | waiting for the owner | none (parked; the Fire Control Codex chat is done) | Owner decides after the build | "Codex's Fire Control test" |
| | **Missions, tools and code (after the build)** | | | | | |
| M37 | Missions 2 to 10 | "Red October in space", about 10 missions (2026-11-09) | MILK RUN built; 2 to 10 not started | none (parked until people play MILK RUN) | Talk mission 2 through | "Campaign: still to do"; `FTUE_PLAN.md` |
| M38 | Mission tools: the Starwolf → Mission window; the Level window draws ships, rings and the route; play from checkpoint N | "I need a mission setup tool" (2026-11-09); the order agreed (2026-11-11) | designed | Mission Setup Tool, then a new chat after the build | Talk first | `MISSION_SETUP_PLAN.md` "The tools, in order" |
| M39 | Refactor steps 2, 5 and 8: the playtest into the Test Runner, the last files out of `Scripts/Tactical`, the clock into `GameManager` | the owner's order (2026-10-25 and 26) | part built | none (after the build) | none | "Board refactor" |
| M40 | Combat autopilot | "Autopilot agreed" (2026-10-25) | designed; TAIL facing not answered | none (after the build) | none | "Combat autopilot and W/S"; spec §4.6 |
| | **Writing and docs** | | | | | |
| M41 | Rewrite all 109 script notes and `SCRIPT_LIST.md` in proper sentences | the owner said yes (2026-11-11) | on hold until the memory was fixed; it is fixed now | **NONE: dropped** | A new chat on the owner's go. No Unity needed | "Script notes: what is left", item 7 |
| M42 | Rewrite the team reports in plain English, producer first | "badly written… rubbish" (2026-11-10) | not started | **NONE: dropped** | Same chat as M41 | `docs/team-reports/` |
| M43 | Out-of-date lines: the spec (six places), BOARDING_README's numbers, DAMAGE_README, ROOM_LAYOUT_PLAN, FTUE_PLAN §0, BOARD_UI "230 px", CONTEXT_INDEX "MISSIONS list" | none | not started | Producer | A docs agent | Producer chat |
| | **Waiting on the owner, no build work** | | | | | |
| M44 | Calls chats made that change play, for the owner to check: Step 178 (battle group sizes; BOGEY until scanned), Step 192 (marines always win on a ship that can't be destroyed), Step 197 (four calls), Step 198 (travel bar calls), Step 206 ("can see" goes by distance only); and from the fight sums: a hit at the middle of a side reaches the reactor only within about 35° of square on, so further round a kill takes 12 hits, not 6 | none: the chats made them | waiting for the owner | none | Owner checks them in the build | BUILD_HISTORY Steps 178, 192, 197, 198; section 1, Step 206; `team-reports/player-1/2026-11-11.md` |
| M45 | Parked by the owner: the sight in fire control; controller support (don't raise it); real sounds; a save during a mission; HEAT for signal; radiation false dots; raiding and salvage; enemy boarders; the disguise; the XO walkthrough and derelicts; the cloak; spoken orders; the old helm art; the body cam; Soham's scripts | the owner parked each one | parked | none (parked) | Wait for the owner | "For later talks"; "Triage"; spec §12.2 |
| M46 | Probably overtaken, for the owner to drop: the WEAPONS FREE split and the old level text fixes; the FTUE XO plan (MILK RUN's COMMAND checkpoints may replace it); the Claude Design request; "is x5 for fighting?"; recording real fights | none | waiting for the owner | none | Owner says drop or keep | "The level rework"; "Triage"; `FTUE_PLAN.md` |

### Next: the MILK RUN player build

The owner's goal (2026-11-11): every system working, the key information reaching the player, and one
proper, fun story level, MILK RUN, that people can play and give their views on. There is no paid art
or sound yet. These items come before the player build.

- **The player build:** not made yet. It is next, now MILK RUN's fixes are in (Step 200). The build
  ends with MILK RUN (owner). Two things first (Mission Setup Tool chat, paused for the night on
  2026-11-11): (1) check what shows after MISSION COMPLETE in MILK RUN: the ship's rooms and the end
  card. Nobody has looked yet. (2) Agree when to build with the other chats. Several have unfinished
  work in the same scripts (railgun effects, the plot's new contact shapes, the campaign rooms), and a
  build made now would ship it half done. Each chat commits its work or says it is safe to ship.
  **Also first: take TEST MISSION out** (Step 204). In Starwolf → Campaign, switch TEST MISSION
  off (Step 205). Then MILK RUN is the last mission again.
  Then make the Mac and Windows (x64) zips, named with the date.
- **A person plays MILK RUN through by hand.** A script played it through on 2026-11-11 (Step 200),
  but nobody has played it with the keys and the mouse.
- Not checked: what shows after MISSION COMPLETE. Not checked on screen: the notes of
  checkpoints 4 to 10, 12 and 16, which the script cleared in about 1 s each.
- **Run the full playtest once more with this batch.** The newest full run is
  `Logs/Playtests/2026-11-11_015627` (Step 197): 256 passed and #194 failed. Steps 198 to 206 came
  after it.
- **Shared index:** `README.md`, `docs/BOARD_UI.md` (was `Assets/UI/Board/README.md`) and
  `docs/CONTEXT_INDEX.md` may still be staged by another chat from a HEAD older than Step 178.
  Committed as they are, they would undo lines from Steps 178 and 183.
- **The steps not finished** (section 1): decide with the owner which of them go into the build.
  Step 202's plot marks are done and committed (dc3b494): the owner played them in SKIRMISH and
  said yes for the build. Nobody has played Step 206's enemy shot lines yet.

**What a MILK RUN player sees that is still wrong or undecided.** The detail is further down.

- The battle group's stand-in call signs show once a ship is scanned ("Open after Step 178").
- The station picture's corners are clipped ("Open after Step 178").
- "KM" shows after distances in units, on the checkpoint lines and the boats' lines ("Bug list").
- The mission clock starts at 2:03:32, and the helm shows AUTO — DIRECT with no autopilot ("Bug list").
- When a lock runs out, it clears the armed switches, and hangar launch distances change with the
  zoom ("Bug list").
- SETTINGS on the main menu does nothing ("Triage", "Small jobs").
- Checkpoint 13 asks for speed 45, and the player may have no way to read the speed ("FLIGHT DRILL:
  calls for the owner to check").
- The title, SAVE SLOTS and the sector map show dark bands on screens that aren't 16:9, and the owner
  hasn't played the board full screen on the MacBook ("Open after Step 182").
- The sector map is green in the game now (941942f, owner). Not done: the slight flicker, and the
  won-mission tick is still Codex's teal (master list M30).
- How the ship feels to fly: the slide, the W tap and the engine flare ("Ship movement feel").
- Every sound is a generated tone, and some art is a stand-in. That is expected for now.

### Script notes: what is left (owner 2026-11-10)

- **Left to do:**
  2. Go through the script notes' bugs in "Bug list" with the owner. Only the enemy torpedo salvo bug
     is fixed in code.
  6. Open (2026-11-11): some scripts have uncommitted changes from other chats. Each chat checks its
     script's top note before it commits. `git status` shows which scripts they are.
  7. Open (2026-11-11): the "What it does" text on the code map comes from the note at the top of
     each script, and those notes are written in the same clipped style the owner disliked on the
     page. The owner said yes: all 109 notes and `docs/SCRIPT_LIST.md` get rewritten in proper,
     flowing sentences, by the chat that made the map. It's on hold until a memory problem is fixed
     first. The owner also wants the map kept up to date as new scripts and systems are added.
- The map of the game's systems and scripts (2026-11-11) is a private page:
  https://claude.ai/artifact/Tk285DVMpwzAfb44Ph2Abr. It shows the code as it was on 2026-11-11.

### Open: the information hierarchy, a talk (2026-11-10, the owner's main priority)

The Information Hierarchy chat is working out how the plot shows what we know about a ship, from
nothing to everything: a ping, a DRADIS contact, a bogey, a scanned ship, a locked ship, the
telemetry dot, and later drones, comms and boarding. The owner's aim: settle the shape of each icon
and what each shape means (hollow circle, full circle, hollow triangle, full triangle). **The first try is
built (Step 202).** Build nothing more until the owner says go.

- **The page:** https://claude.ai/artifact/2mUFc4kKo7NitkEv72UK6T. It shows each stage as it looks
  today, with screenshots, then three ways to shape it: **A** circle to triangle (my pick), **B** one
  dot filling in, **C** navy frames (square unknown, diamond enemy, circle ours). Then three looks for
  the telemetry dot. The owner: "This is excellent." To change it, read it back with the Artifact
  tool and publish to the same link. The full screenshots are in `Logs/InfoHierarchy/` (this Mac
  only, not in git).
- **The owner's picks (2026-11-11):** Option A as a whole is out ("kind of weird"), but its rule
  stays: a circle is a signal, a triangle is a ship, hollow is a guess, filled is confirmed. The
  triangle must not point along the course. The base is Option C, navy frames. The DRADIS contact
  is the bar from Option B (today's bar). The owner is happy to experiment and change it as we go.
  **The draft ladder**, now at the top of the page: ping a blinking hollow grey circle; DRADIS
  contact the bar; bogey a hollow amber square; scanned enemy a filled red equilateral triangle,
  point up, never turning; scanned friendly a filled blue circle; locked adds the white corners;
  telemetry a hollow red triangle; lost keeps its filled shape and fades. **The first try I
  proposed:** the new shapes only, on the plot. Rule changes wait. Built in Step 202.
  The page's five points below are replaced by a new list of five on the page ("Still open").
- **First try built (Step 202, 2026-11-11), and in the MILK RUN build:** past visual range, a
  scanned enemy is a filled red triangle with equal sides, point up. A ship we can't name is a
  hollow amber square, locked or not. Our own ships are filled blue circles. A scanned ship's label
  reads GARUDA · CRUISER. The detail is in `docs/BUILD_HISTORY.md`, Step 202.
- **Pick up here (2026-11-11, the owner went to sleep):**
  1. ~~The owner plays Step 202.~~ **Done.** The owner played SKIRMISH: "I think everything is
     absolutely fine."
  2. ~~Commit Step 202.~~ **Done.** Committed as dc3b494.
  3. Then the page's "Still open" list, one by one. Done in Step 202: GARUDA · CRUISER on the
     label, and the amber bogey square. **Still open:**
     - Should a ship start as an unknown grey ping circle? It changes a rule.
     - Are our blue circles clear enough next to grey rock circles?
     - The telemetry and drone marks (below).
  4. The page's source file is in a scratch folder and can be lost. To change the page, read it back
     with the Artifact tool and publish to the same link.
- **Telemetry and drone marks (sent to the Sniper Plan chat):** a second-hand fix is the hollow
  version of the shape we know: a hollow red triangle, or a hollow amber square. A drone itself is a
  small blue circle with a faint dashed blue ring. The owner hasn't seen this for drones.
- **The owner's new idea (2026-11-11), not built:** "would it be more compelling if the active scan
  actually extends beyond Dradis range… like having an omnidirectional piece of information, then
  you have a narrow little telescope that you can magnify." It is a sensor rule. The owner said it
  is "maybe not your job to fix". The Information Hierarchy chat's view, given to the owner:
  - The narrower the cone, the further it reaches.
  - A long ping is loud and can give us away.
  - A far find is a red triangle that fades until the next ping.
  - It would give LONG SHOT a way to see past DRADIS, beside drones.
  - Next: take it to the Sniper Plan chat with the drones decision.
- **What the plot does today** (found by this talk, in `PlotPanel.Refresh`):
  - A ship is never an unknown ping. One scan names it at once.
  - A ship that is heard but not scanned shows only a red bar on the DRADIS circle, with BRG.
  - A bogey shows only inside visual range, or when locked without a scan.
  - A scanned ship and an incoming torpedo are both red dots.
  - Colour does nearly all the work. Hollow grey circles already mean rock echoes. White triangles
    already mean our fighters.
- **After the owner answers:** write the answers into this log and Claude's memory notes. Send the
  telemetry dot's look to the Sniper Configuration chat, which builds the dot (`SNIPER_PLAN.md`).
  Message the Tutorial and Campaign Manager chat before touching `PlotPanel.cs`. It has its own
  uncommitted lines there (friendly ships turn blue once scanned).

### Open after Step 185: art (2026-11-10)

The art master list is `docs/team-reports/art/2026-11-09.md`. **Starwolf → Art** shows the same
pictures as they are now. Set-up: `docs/BOARD_UI.md`, "How to change a panel's art".

- **Look at Starwolf → Art by eye.** The owner opened it on 2026-11-10 and saw the small pictures.
  Still to check: whether each box has a **Select** button for picking from a list. Dropping a
  picture onto a box was tested and works.
- **Waiting for art.** 10 stand-ins on the board (tick **Waiting for art** in the window to see
  them). 6 empty slots in the ship's rooms: the BRIDGE, ENGINEERING and QUARTERS paintings, and the
  XO, CHIEF ENGINEER and MARINE SERGEANT pictures (the HANGAR's two came from the Codex art test).
- **The menu screens (Step 189).** SAVE SLOTS, the briefing and the sector map have Art lists with
  empty slots. They are waiting for art. The end screen and pause menu have stand-in board art: the
  dark plate and the bevelled keys. The start screen has no boxes to give art. The intro's code
  draws its green letters.
- **Fonts are not in the Art window.** They are still on each panel's Fonts list. Add them only if
  the owner asks.
- **Owner question:** should the art audit become a team agent that can be run again, or was once
  enough?

### Open after Step 182: the board on screens of other shapes (2026-11-10)

Step 182 is built and checked. The battle board now fills any screen. Pictures of it are in
`Logs/ScreenFit` (on this Mac only, not in git). `before_after_macbook.png` shows the board before and
after at the MacBook's shape.

- **The owner hasn't seen it on the MacBook.** Make a Mac build and have the owner play it full screen.
  Then drag the comms dial and the helm throttle with a real mouse on the MacBook. That has only been
  checked in code.
- **The other screens still show dark bands** on screens that aren't 16:9: the title, SAVE SLOTS,
  SECTOR MAP, the ship's rooms and the MISSIONS list. The owner said to start (2026-11-10). Nothing is
  built yet. What the files show, not yet checked with a picture:
  - **Ship's rooms** (`Ship.unity`): the same frame and layout as the battle board. Do it the same way
    as Step 182: `Tactical Board` and the Frame stretch; `Ship` stretches like the plot (104 / 90 /
    104 / 100 in from the edges); `Rooms` to the middle, `Talk box` to the bottom middle, `Room tabs`
    to the top middle. No script uses fixed screen sizes. No owner call needed.
  - **SAVE SLOTS:** a plain dark background with no picture. Probably fine already. Check with a picture.
  - **Title** (`00_Start`): the intro's code draws a fixed 16:9 picture, with black round it. The
    background is almost black. Keep the "Shagun Shah" mark in the bottom-right, menu only
    (Loading Screen chat, Step 188).
  - **Sector map: owner call needed.** The console picture (`Briefing/OperationalMapConsole.png`) has
    detail right up to its edges, so repeating its edges won't work. On a MacBook today it's cut a
    little at the sides and has thin bands above and below. Options to show the owner with pictures:
    1. Enlarge it to fill, cutting about 6% off each side on the MacBook, slightly into the copper
       frame on the left.
    2. Keep it whole and fill the bands with the console's dark steel colour.
    3. The owner makes a taller version in ChatGPT, with more cables and metal above and below. Each
       screen shows as much as fits.

    The chat's view: 1 now, 3 when the owner has time for the art.
- **Very wide screens:** no limit past 21:9. On wider screens the plot keeps getting wider.
- **Rule for new board pieces:** a new panel sheet must be pinned to its screen edge. A new drawer
  under Drawers needs anchor right-middle and pivot (1, 0.5). A frame picture must repeat, never
  stretch (owner: "stretching is really ugly"). See `docs/BOARD_UI.md`, "Screens of other
  shapes".
- **The Fire Control art chat's test scene** (`Fire Control Codex.unity`) still has a fixed 1920 × 1080
  board.

### Open after Step 178: mission 1 and the battle group (2026-11-10)

**Still open for MILK RUN:**
- The station picture's side corners are clipped a little: its camera frame fits its length, not its
  corner-to-corner width.

**Other open items:**
- **The ship call signs:** INDRA, VARUNA and MARUT must go (owner). No new names chosen yet: see
  `docs/MISSION_SETUP_PLAN.md`, "Names".
- **Missions 2 to 10 are not built.** Talk each through with the owner before building. The old
  levels stay switched off and are never deleted.
- **The owner's save in slot 1** has wins for the old level names, so its map starts again at mission 1.
- **Docs not yet in line with Step 178:** in `docs/BOARD_UI.md`, the Briefing Icon's "In FLIGHT DRILL
  it is 230 px". In `docs/CONTEXT_INDEX.md`, the MISSIONS list mentions. The lines in this log were
  put right when it was split in two (2026-11-11).

### Codex's Fire Control test, and what no person has played yet

**Codex art test (2026-11-10, Step 193).**

- **Codex's Fire Control, a test in `Assets/Scenes/Tactical/Fire Control Codex.unity` (Step 193).** Open:
  - The owner has not decided whether this Fire Control replaces the board's. Until then the board
    keeps `Fire Control.prefab` and the periscope at the top.
  - Codex's Hangar package, `References/Codex-Starwolf-Art/03-Hangar/Starwolf-Hangar.unitypackage` (the
    assault-boat roster for the board's Hangar drawer), is not unpacked.
  - The FLIGHT LEADER still says "Every fighter is fuelled and armed". Our ship has no fighters
    since Step 170. The owner picks the new words.
  - Should the periscope come down by itself when this Fire Control opens? Asked 2026-11-10, no answer
    yet. Today SIGHT does it by hand, and the lock rules are as before.
  - If this Fire Control goes into the game: a lowered periscope must come up when the drawer closes
    (section 5, trap 33). The drawer sheet must also be tied to the right edge with pivot (1, 0.5),
    like the other drawers since Step 182.

**Playtest and play checks (2026-11-09).**

- The playtest still tests the old two side turrets and slug speed 2,500 (all but section 25f). It
  puts six fighters back on our ship. So it can't catch the briefing and fighter problems below.
- No person has played Steps 160 to 176, except the owner's try of the new keys. That led to Step
  175 and to "Ship movement feel", below. Step 175 itself and the intro flicker (Step 174) need a
  person. Step 173 had no check in play mode. Step 176 had a filmed play check on 2026-11-09: the
  newest dot keeps one size (`Logs/SectorMapCheck/sector-map.mp4`).
- No person has played Steps 184, 197, 199, 202 or 206 either. Step 196's rule that marines still out
  on boats count as back aboard hasn't been played, and Step 202 didn't check a friendly ship past
  visual range.

### Campaign: still to do (Steps 167 to 172; owner 2026-11-08 and 09)

- **How to set up a campaign mission** (Step 183, 2026-11-10; the window is Step 205). The missions are in the Campaign
  file, `Assets/Campaign/Campaign.asset`. Open **Starwolf → Campaign**. Press + to add a
  mission. Type its name and briefing. Drag its level scene into Level. Set where its dot goes. The
  level itself is made in its own scene in `Assets/Scenes/Levels`, with **Starwolf → Level** and the
  Mission part on it. The end card's closing line is in this file too, under Closing Line (Step
  194). The window shows it under the missions. To keep a mission out of the game, switch it off
  in the window. Do not delete it. Planned on top of this file, not built: the Mission Setup Tool chat's work (call signs, who
  speaks, a line for each checkpoint, a Starwolf → Mission window).
- **Mission setup: the words in a mission** (owner 2026-11-09 and 10; talked through; the window
  is **not built**; MILK RUN's flow is built, Step 192). The plan is `docs/MISSION_SETUP_PLAN.md`. The owner writes and changes every word in a
  mission in one window, **Starwolf → Mission**: the mission's name, its briefing on the sector map,
  several lines of talk at each checkpoint (each with who says it), the title word STARWOLF, and the
  ships' call signs. All of it goes in the Campaign file. Extend `Campaign.cs`; never a second
  mission list. Waiting on the owner, in this order:
  1. Our ship's call sign and the others'. Owner 2026-11-10: call signs in English with an Indic
     feel, like Red October, not Indian names. INDRA, VARUNA and MARUT must go. Offered, no answer:
     RED MONSOON (my pick), BLACK MONSOON, THIRD EYE, SAFFRON NIGHT, SILENT TIGER, IRON LOTUS; carrier
     BANYAN, battleship TUSKER, corvette MONGOOSE, enemy COBRA.
  2. Mission 1's new name: **MILK RUN** (Codex, 2026-11-10, with a new briefing). Codex wrote the
     talk for all 23 checkpoints. Committed in 49f9f74 (Step 200). See the plan.
  3. The owner told how mission 1 goes (2026-11-10). **MILK RUN's flow is built (Step 192):** 23
     checkpoints, from power to the corvette's capture and the intel. Done: Codex's words for
     checkpoints 12 to 23 are in, and it was played through on 2026-11-11 and passed (Step 200). The
     group's shuttles to the escape pods are built (see "Open after Step 178"). Still open: the call signs and our ship's name
     (item 1). Still to do: the window and the checkpoint talk in the Campaign file (ask once more
     before the new window script). The checkpoints are still on the level's Mission object.
- **The travel bar (Step 198):** restock drones stays hidden until drones are built.
- **The campaign's end (Steps 194 and 203):** the closing line is "To be continued." (owner
  2026-11-11, later the same day as "keep it empty": "just 'To be continued.' It's absolutely fine.").
  It goes in `Assets/Campaign/Campaign.asset` → Closing Line at build time (master list M2). Nobody
  has played a full campaign win through the real win screen yet.
- **A SAVE during a mission** (the pause menu): parked by the owner, "mention it later". The owner
  liked checkpoints better: a save at one or two set moments in a mission. Not built.
- **The story** (owner 2026-11-09, still being talked through): about 10 missions, "Red October in
  space". Mission 1, MILK RUN (called ORIENTATION until 2026-11-10), and SKIRMISH are built (Step 178). Missions 2 to 10 are not. The old
  test levels are switched off and kept, never deleted. Each mission copies the pieces it needs.
- **The ship rooms' four people and their lines are stand-ins** (Step 168). The owner plans the story
  separately.
- **Left over from Step 172:** the hidden `SECTOR MAP card` in `SectorMap.unity`, and
  `Assets/Art/Sector Map/Grid Square.png`, now unused. Also `References/Codex-Sector-Map/`, a patch folder
  no longer needed. Delete only with the owner's yes.

### The sniper ship and weapons: still to do (Step 170 on)

- **Sniper plan steps 2 to 6** (tracked only in `SNIPER_PLAN.md`): the flare is next (its 15,000 size
  waits for the owner's yes). The sight in fire control is paused by the owner (2026-11-09); it would
  take away ANGLE and BEARING. Then sensor drones, lasers and a new damage type.
- **LONG SHOT can't be a long shot yet.** A lock reaches only as far as DRADIS (7,500). The gun
  reaches 30,000. It waits on the drones. The owner wants DRADIS to reach "a little further", with no
  number yet.
- **Torpedoes should do area damage** (owner 2026-11-09). A torpedo that misses still hurts a ship
  near the blast. Not built: `WeaponSystem` and `DamageSystem` have no blast or area damage.
- **A shot with no course.** Step 141 left "a missing course doesn't yet change a shot at the Garuda"
  for the owner. Since Step 170 (slug 1,500 a second, reach 30,000), by Step 141's sums a NO COURSE
  shot should miss past about 15,000. Nobody has checked it again or asked.
- **The close-range weapon** in the designer's table (Step 130) was "left out until the owner designs
  it". Nothing since. The sniper plan's lasers (step 5) may be the answer: **unsure**.
- **The close-up (Step 166):** ~~PDC fire has no streak.~~ (Step 197 added PDC streams.) ~~The Garuda's fighters stay flat triangles.~~
  **Done, Step 199:** zoomed in, our craft show as small 3D models. (The Garuda launches no craft. Only our
  ship's hangar launches, `HangarSystem`.) The models are samples; the owner will find better ones.
  The fighter model is heavy, about 812,000 vertices each. Fine with five craft; look here first if the
  close-up stutters with many.
  Cutting the Garuda's model took 102 ms on the frame of the kill.
- **Step 173:** the code's default for Close Up Full Zoom is still 1.5. The prefab uses 1.1, and the
  game uses the prefab's number.
- **The owner's picks from the firing looks page** ("Ship Actions Lab", 2026-11-10). Built: the
  torpedoes, A2 and A3 (Step 184). ~~Still to build, in this order:~~ **Built in Step 197:** C2,
  C3, D2, B3 and E2. Left:
  - **D3: a hit shows only if a drone watches.** It waits on the drones.
  - ~~**For the owner: should enemy shots leave a line on the table too?** Not answered. Today only
    ours do (C3).~~ **Answered and built, Step 206.** Owner 2026-11-11: "Only ones we can see". An
    enemy slug we can see leaves a faint red line, along only the part of its path we saw. Enemy
    torpedoes leave none. **A call made in Step 206, for the owner to check:** "can see" goes by
    distance only. A rock between us and the slug doesn't hide it, and DRADIS's band isn't checked.
  - **Next, when the owner is back:** ~~the answer on enemy shot lines~~ (answered, Step 206); D3
    once the drones exist; a torpedo model for the close-up; Player 1 to run the fight sums again
    after Steps 184 and 197.
  - **Calls made in Step 197, for the owner to check.** (1) With no ENG power, the push back was
    first left as drift. In play, one shot sent the ship backwards for good, out of the battle space.
    Now the thrusters always stop it. (2) A hit on a ship we can't see still does its damage. Only
    the report is hidden. (3) A POINT's countdown runs to IMPACT ? even when we saw the slug hit a
    ship before it reached the POINT. (4) A doomed torpedo keeps flying for the 0.5 seconds while
    the stream walks onto it. A torpedo first caught very close could still hit us in that time.
  - **Player 1 runs the fight sums again** (Steps 184 and 197). The torpedo flight changed, the
    railgun now pushes the ship back, enemy salvos now fire every torpedo, and a PDC kill now lands
    0.5 seconds later. `docs/scale-sim/scalesim.py` has none of the Step 197 changes yet.
  - **Where the looks are:** the page https://claude.ai/artifact/KXZhwnSU7hoG7aaEqThx6g (read it
    with the Artifact tool). Each option's numbers are in its script: B3 leaves at 560, breaks at
    0.35 s at up to 240° a second with a thruster flash, slows to 170 in the turn, then burns to
    300, three craft 0.35 s apart. C2 kicks back 40 units/s for 0.4 s, then the stern thrusters stop
    it (about 24 units in all). The owner's picks are in Claude's memory note "Ship actions look".
- **Step 184:** the torpedo model is a plain cylinder (owner 2026-11-10: "instantiate a little
  cylinder… It's so tiny. And as soon as it leaves the visual range, it becomes an icon anyway").
  It uses the ships' metal, set as Torpedo Look on the Plot panel. A real model can replace it later.
- **Step 184:** the side-thruster puffs that A2 showed while the nose swings are not built.
- **Step 184, the enemy's salvo:** each torpedo leaves from where its launcher was when it fired. The
  enemy moves about 10 units between them.
- **Owner's idea: exposed rooms take a condition by chance** (2026-11-10). Once a side's armour is
  gone, the rooms behind it could catch a fire, lose air or take radiation by random chance. For the
  conditions talk. Not built.
- **Owner's idea: active scanning improves the hit** (2026-11-10). Scanning over time could improve
  the odds, or allow a hit aimed at one room, such as the bridge. Owner: "That becomes the value of
  active scanning." A talk first. Not built.
  Owner, later the same day, unsure: "If you do a scan on a ship, then you should be able to target a room. I don't
  know. I'm just thinking out loud." Not to build until the owner says so.

### The Garuda does not follow our rules (QA, 2026-11-09)

From the QA report, `docs/team-reports/qa/2026-11-08.md`. The README's "Ruled out" list says the
enemy is our ship with different numbers and a brain. It never gets separate rules.

- Its guns need no charge, no lock and no slow turn. It aims at our true position.
- It never fires its PDCs, so our torpedoes and boats are never shot down. `EnemyBrain` never turns
  on `pdcAuto`, and `WeaponSystem`'s PDC stops run only on rounds coming at us. Step 55 said "Enemy
  PDCs come later, with the ship AI". MILK RUN's corvette has PDCs too (`Corvette.asset`), so a fix
  here changes whether MILK RUN's torpedoes get through. The Garuda's PDC-B has 0 rounds in `Garuda.asset`.
- Firing doesn't make it louder. It is always heard at full reach (`SensorSystem.Heard` never reads its
  signal). MILK RUN's corvette too.
- It never hides in gas clouds (`TacticalWorld.InClouds`: "Enemies don't hide in clouds yet").
- It fights fires only in its magazine (`EnemyBrain.Think`). Fire, vacuum and radiation
  elsewhere go unchecked (Step 59).
- With no slugs left, its reach falls back to an old 10,000 (`EnemyBrain.Reach`), and it creeps
  straight at us at 40% speed.
- A fire or a crash on the Garuda itself counts as a hit from us (`EnemyBrain.Think`: a hit is any
  drop in its health). It makes it ALERT, tells it roughly where we are and lets it fire a torpedo
  salvo (QA 3). The reinforcement countdown starts once, on the first ALERT.
- When an EMP wears off, its ENG comes back to full at once. Ours comes back one unit a second.
  Reactor damage never lowers its ENG, because its power step never runs (checked 2026-11-11).
- Removed on 2026-11-11, not Garuda-only: turning with no ENG power is the same for every ship (see
  `NavSystem.cs`, `Steer` in the Bug list), and our ship also works out its lead from the centre.

### Bug list: every known bug, not fixed

Every known bug that is not fixed is listed here. Add a bug when you find one. Take it out when it
is fixed.

**Checked against the code on 2026-11-11** (the Bug Fixes chat). Every entry below was read again in
the code. Fixed entries were taken out, and wrong wording was put right. "MILK RUN" after an entry
means a player can meet it in mission 1.

Found by the audit (2026-11-09):

- **For the owner: a lock can start without zooming.** At the starting MAG (1.4), a ship 1,220 to
  1,650 units away already fits the sight, so the lock begins as soon as SIGHT is on it
  (`ShipState.ScopeFits`; `scopeFitUnits` 1,000 in `02.1_Grid`). In NO PEEKING that loses the
  level. Step 143's row says SIGHT and SLEW TO were used. MILK RUN: yes, but there a lock is the goal.
- **INCOMING SIGNAL never shows once a mission line is on the alarm strip** (the reinforcement
  countdown). Only the tone plays (Step 143; `BoardManager.ShowAlarm` puts the signal last). This is
  not the "59INCOMING SIGNAL" spacing bug below.
- **Enemy torpedoes and slugs are drawn on our plot even when our sensors can't see them**
  (`PlotPanel`, the loop over `s.weapons.Incoming`). This goes against "DRADIS shows signals only".
- **The mission clock starts at 2:03:32,** the design's demo time (`missionSeconds: 7412`, saved in
  `02.1_Grid.unity`). It shows under MISSION on the time panel, and every POINT is stamped with it.
  Step 63 logged it "for the owner". No answer. MILK RUN.
- **The helm reads "AUTO — DIRECT" with NOSE lit at load,** but there is no autopilot. The code and
  the scenes start `attitude` at NOSE, and nothing steers by it (`NavSystem`, `HelmPanel`). MILK RUN.
- **Hangar landing and launch distances change with the plot's zoom** (`PlotPanel`: `landClearUnits`
  and `launchSideUnits` use the zoom; QA 4). MILK RUN: the boarding boats.
- **At x5 ships move in big steps** (x10 is off for players). Slug and torpedo hits are tested along
  the path, but assume the target keeps its last course. The PDC check is a straight line once a tick.
  Shift turns once a tick: 0.84° at x1 and 4.2° at x5 (QA 5, Player 1). MILK RUN: the turn steps.
- **One fixed dice, seed 49, for every ship and every game** (`ShipState.NextDie`; QA 11). MILK RUN.
- **For the owner: when a lock runs out, it clears the armed switches** (`TickLock` → `DropLock` →
  `ClearSwitches`: LOAD, ANGLE, BEARING and ARM). The code means it to, and its Tuning note says so.
  Player 1 flagged it. MILK RUN.
- **The Tuning window's "Keep play-mode changes" drops some values:** the 12 sounds in
  `BoardManager`, the two eased presses (such as RECALL) and the arrays `stepUnits` and
  `gainByAwareness` (`StarwolfTuningWindow.Write` handles only plain values; Step 40).
- **Tuning during play refills ammunition** (`Ship.PullFromFile`), and a campaign win then saves the
  full rounds (Step 196). Only the Tuning window can do it, so a player never meets it.
- **Numbers typed into the code, out of Tuning's reach** (QA 10, 12, 13 and 14, checked 2026-11-11):
  the sight's 4° (`ShipState.ScopeOn`, `ScopeOnAxis`; the `[Tune]` `onBearingDeg` is not used there);
  RNG 100 a click (`WeaponSystem.NudgeSol`); the 20/60 s heard-twice window and the 0.25 and 0.15
  guess errors (`EnemyBrain.Listen`); SEARCHING speed 0.8 (`EnemyBrain.Steer`); 0.85 and 0.5
  (`EnemyBrain.NewHuntPoint`); 8,000 units (`QuestSystem.Arrive`); 1,000 from the edge
  (`EnemySpawn.Start`); 50 and 300 (`HangarSystem`, `LandUnits` and an escort); 0.5 units a second
  (`TacticalWorld.SecondsToRock`); 30° steps to 150° (`TacticalWorld.ClearHeading`).
- **The fire lever reads the hangar's lever setting** (`FireControlPanel`: `s.hangar.leverFireAt`).
  Both are 0.9 today, so nothing shows.
- **Every radio message signs as GARUDA** (`EnemyBrain.Send`).
- **A coast time of 0 breaks a torpedo flying a drawn line** (`WeaponSystem.Drift` divides 0 by 0).
  Every ship file has 3.5 today.
- **For the owner: the two hangar dots' drawn lines in `Asura.asset`.** The torpedo launchers' lines
  were taken out in Step 184. The hangar dots still have their fc356b7 lines: smooth curves out
  sideways, then forward. They look drawn by hand. The boarding boats fly them (MILK RUN). Did the
  owner draw them?
- **`Corvette.asset`'s class name is one of the stand-in names the owner ruled out** (2026-11-11).
  MILK RUN's corvette shows its object's name, so it isn't seen there. A corvette added by
  `AddContact` would show it.
- Taken out on 2026-11-11: the Naga frigate's missing picture (`TacticalWorld.Join` now gives every
  ship one; what is left is the `PeriscopePanel` entry below) and `PlotPanel`'s list of ship views
  (fixed in 2c3153f).

Found by the script notes (2026-11-10):

- **`Assets/UI/UI_Script/UIController.cs`, `AddBogie`:** the line
  `if (bogieList[j].wepImageGo = null)` sets the picture to empty. It was meant to check whether the
  picture is empty. The next line would then crash. Nothing calls `AddBogie` today. This old UI is
  only in the older scenes: Demo, Prototype, Prototype Dev, UI and Canvas_CRT. The battle board does
  not use it.
- **Older scripts the battle board does not use.** They are on the PlayerShip prefab and in the older
  scenes.
  - `PointDefenseCanon.cs`: nothing calls `SpinUp`. The gun never fires.
  - `Macrocannon.cs`: nothing calls `AttemptLock`. The gun never locks.
  - `CommsManager.cs`: nothing calls `TryLockSignal`.
  - `MissileLauncher.cs`: once the spare torpedoes run out, torpedoes still in the tubes can never be
    fired.
  - `GameClock.cs`: the clock stops at its time limit. Nothing ends the mission.
  - `WeaponManager.cs`: on the PlayerShip prefab, two gun slots have a type and a name that do not
    match the gun in them.

Found by the Technical pass of the script notes (2026-11-10):

- `BoardManager.cs`, `TickAux` and the steps beside it: AUX, power, sensors, charge and launches move
  on for our ship only. Other ships' AUX never fills. `EnemyBrain` repairs its magazine with
  `DamageSystem.ApplyOrder` for free, skipping the AUX check and cost. It does this even with
  `firesBack` off, so MILK RUN's corvette does it too. This is not yet in "The Garuda does not follow
  our rules".
- `QuestSystem.cs`, the HitUnseen goal: any hit we see wins, on any ship. It should need a hit on the
  target. SilentShot has only one enemy today. The bug does not show there yet.
- **For the owner: "KM" or "km" after distances in units, in many places (checked 2026-11-11).**
  MILK RUN. What should the screen say? Every place: `QuestSystem.Point` (checkpoint, jump point and
  NEAR lines); `SensorsPanel` (DRADIS range); `HelmPanel` ("k KM"); `HangarSystem.FlightStatus`
  (RETURNING, OUTBOUND); `FireControlPanel` (mount range); `PlotPanel` (RULER and COMPASS, track
  speed "km/s", contact label "RNG … km"). Placeholder text in `Plot.prefab`, `Fire Control.prefab`
  and `Sensors.prefab` has km too. The two entries below are part of this.
- `QuestSystem.cs`, `Point`: the alarm strip shows "KM" after a distance in units. This is on the
  jump point, checkpoint and NEAR lines.
- `HangarSystem.cs`, `FlightStatus`: the RETURNING and OUTBOUND lines show "KM". The distances are in
  units.
- `NavSystem.cs`, `Steer`: there is no check for ENG power. Any ship still turns with ENG at 0 or under
  EMP, ours too. MILK RUN: its first lesson is power, then movement.
- `HangarSystem.cs`, the boarding order that disables a ship: it sets the enemy's ENG and MAG to 0
  directly, the same as the scuttle below. It should go through `DamageSystem` (found 2026-11-11).
- `PeriscopePanel.cs`, `ViewOf`: the list of ship pictures is made once. A ship that joins later, such
  as a reinforcement, never gets a picture in the targeting computer, if the sight was used before it
  arrived (`views` is filled on first use). `PlotPanel`'s list was fixed in 2c3153f.
- **Older scripts the battle board does not use.** One line each:
  - `UIController.cs`, `Update`: keys 0 to 4 read an empty contact list and crash. P sends an empty
    target to `LaserFireCo` and crashes. This happens in every scene with this old UI.
  - `UIController.cs`, `StartSpeedometerUpdates`: it logs "PlayerShip not found", then reads the
    ship's turn rate anyway and crashes. `Start` stops there. The UI scene has no PlayerShip.
  - `Asteroid.cs`, `Start`: the "Asteroid" tag is missing from the project's tag list. Setting it
    gives an error.
  - `Autopilot.cs`: Port and Starboard are swapped. Port puts the target on the right.
  - `Autopilot.cs`, `FixedUpdate`: the left click is read with `GetMouseButtonDown` in the physics
    step. Clicks can be missed.
  - `CommsManager.cs`: the arrow keys move the frequency 0.1 each frame. A faster frame rate tunes
    faster.
  - `EnemyShip.cs`: in open space all five feelers score the same. It takes the first one and steers
    60° off its goal.
  - `Laser.cs` and `HullSystem.cs`, `DetermineHitSide`: the laser passes the way it travels.
    `HullSystem` reads that as the way to the hit. A nose hit counts as aft. On the PlayerShip prefab,
    `Shields` takes laser hits first and hides this.
  - `PlayerShip.cs`: it calls `ControllerHaptics.StopAll` every physics step outside hyperspeed. Any
    other controller buzz stops within one step.
  - `PowerManager.cs`, Black Alert: it vents nothing. Its end-of-frame step writes over the Vent
    settings in the same frame.
  - `Shields.cs` and `Railgun.cs`, `ApplyDamage`: the railgun checks for `PlayerShip` before
    `Shields`. On the PlayerShip prefab both are on one object. Railgun hits skip the shield.
  - `Shields.cs`: a hit while the shield refills stops the refill. It starts again only after the
    shield breaks again.
  - `ShipStability.cs`: turning drains at most 3 a second (0.15 on the prefab). The meter refills at
    5 a second. Turning alone never empties a meter above 10. The prefab's zone limits are 60 and 90.
    The code's are 0.6 and 1.1. Every turn on the prefab counts as gentle.
  - `BoardingPodLauncher.cs`, `LoadPod`: loading takes the pod from the ammo. `CanFire` needs ammo
    above 0. The last pod loads but never fires.
  - `BroadsideCannon.cs`: ammo never refills. Nothing calls `Reload`. On the PlayerShip prefab, range
    and ammo are 0. It can never fire there.
  - `Macrocannon.cs`, `ArmWeapon`: it counts the barrels before the 1-second load ends. Four quick
    requests load four shells. The next shot reads a barrel that does not exist and errors.
  - `Macrocannon.cs`: loading takes ammo. `CanFire` needs ammo above 0. Shells loaded with the last
    ammo never fire.
  - `Missile.cs`: blast damage falls off by the distance to each object's centre. An object whose
    centre is past the 100-unit blast gets negative damage. It is healed.
  - `MissileLauncher.cs`: a second lock request while a tube is locking runs two counts at once. The
    lock comes in about 1 second.
  - `Railgun.cs`: the tags HeavyArmor, Station, Asteroid and Debris are missing from the project's tag
    list. The checks that stop the beam and remove rocks never match. Unity may also log a tag error
    on each hit. Not checked.
  - `Railgun.cs`, `Update`: switching guns while charging leaves `Update` early. The charge never
    fires or ends. The charge rumble keeps going.
  - `WeaponManager.cs`, `SwitchToNextWeapon` and `SwitchToPreviousWeapon`: with no gun slots, they
    divide by 0 and error. `PlayerShip` can add an empty `WeaponManager`.
  - `WeaponUIDisplay.cs`: the Broadside bar divides ammo by max ammo. With max 0, as on the
    PlayerShip prefab, the bar gets a broken number (NaN).

Found earlier in this log. The detail is under the heading named:

- **The radio countdown is jammed against its text** on the strip, like 59INCOMING SIGNAL (2026-11-02).
  The Count box in `AlarmStrip.prefab` is 7 px wide, so two digits run into the words. The battle
  space edge's 30-second countdown has the same fault ("30EXITING…"). MILK RUN: the edge countdown.
- **The Garuda breaks our ship's rules in eight ways.** For example, a fire on the Garuda itself
  tells it where we are. See "The Garuda does not follow our rules".
- **The game stops for 102 ms when a wreck is cut in two** (Step 166). It happens on the first frame
  the close-up shows the wreck: the kill, if zoomed in, otherwise the moment you zoom in
  (`PlotPanel.ShowShotsAndBlasts` → `Break`). See "The sniper ship and weapons: still to do".
- **The FLIGHT LEADER in the HANGAR room still says "Every fighter is fuelled and armed".** Our ship
  has no fighters. The owner picks the new words. The text is in `Assets/Scenes/Tactical/Ship.unity`.
  MILK RUN: the rooms after MISSION COMPLETE. See "Codex's Fire Control test, and what no person has played yet".
- Taken out on 2026-11-11: the white dot over a drifting torpedo (fixed in Step 184), LONG SHOT at x5
  (closed by the owner on 2026-11-10) and the six sRGB pictures (settled on 2026-11-10, no change).
- **The old levels' briefings have wrong text since Step 170.** Leave it for the level rework. See
  "The level rework".
- **The COMPASS eraser area is a square** (Step 36). See "Smaller, not built".
- **On the amber grid, the sweep's tail uses the edge colour.** The design has a deeper red (Step
  43). See "Smaller, not built".
- **"HANGAR BAY DAMAGED — CANNOT SPOT CRAFT" shows when the hangar is only airless.** See
  "Play-through 2", item 5.
- **The Garuda starts facing a random way.** The owner's rule: at the top, facing south. See "Found
  by two audits", "Added on 2026-11-09".
- **`HangarSystem.cs`'s scuttle sets the reactor to 0 directly.** It should go through
  `DamageSystem`. See "Found by two audits", "Restructure: done".
- **`PlotPanel.cs` asks for a text field every frame.** Small. See "Found by two audits",
  "Restructure: done".
- **Unsure: the playtest's periscope check can fail by chance.** See section 5, trap 23. Probably
  fixed: it has passed in all 39 runs since e433666 (2026-10-27).
- **The playtest's RECALL check (#56 on 2026-10-29) can fail on a busy machine.** See "Play-through
  2". It samples a 0.1 s ease once a frame. Rare: all 21 runs since 2026-10-29 passed.
- **A playtest cut short leaves Unity's Asynchronous Shader Compilation off.** See section 5, trap
  26. Nothing saves or restores the setting. It is on today.
- **Eight font files rewrite themselves whenever Unity runs,** not seven: Barlow Regular is the
  eighth. See section 5, trap 28. Five of them were committed in 6f75cb2 (2026-11-11), which the trap
  says not to do.

Found while this log was split in two (2026-11-11). Each was written under another heading before:

- **MILK RUN's station picture is clipped a little at its side corners.** Its camera frame fits the
  station's length, not its corner-to-corner width.
- **Playtest check #194 fails:** the Garuda's picture doesn't show on the plot when it is locked and in
  sight. It failed on 2026-11-10 (with #186) and again in the full run of 2026-11-11
  (`Logs/Playtests/2026-11-11_015627`). Step 197 put it down to another chat's change to the contact
  pictures.
- **In the test scene `Fire Control Codex`, closing the drawer with SIGHT down leaves the periscope
  down out of sight, and a lock can still build.** Raise SIGHT before closing (section 5, trap 33).
- **An enemy salvo's torpedoes each leave from where the launcher was when it fired.** The enemy moves
  about 10 units between them (Step 184).
- **Unsure: the playtest may still test the old two side turrets and slug speed 2,500, and put six
  fighters back on our ship.** That was true on 2026-11-09. Step 197 brought some checks up to date.
- **`NavSystem.Steer` caps the throttle by ENG units without the engine room's state.** The W/S cap
  and the fader include it. Harmless today (`HANDOFF_GAPS` row 32).
- **The assault boats' stranded clock never resets** (a call made in Step 79, never put to the owner).
- **The close-up zoom has two numbers.** `PlotPanel.cs` says 1.5 for Close Up Full Zoom, and the Plot
  prefab says 1.1. The prefab wins, so the game is right (section 5, trap 29).
- **The W tap size (1) and the throttle's curve are typed into `HelmPanel.Throttle`.** Tuning can't
  reach them, which breaks the rule on game numbers ("Ship movement feel", item 2).
- **Unsure: a slug's dot may slow down just before a hit.** It was measured at 60 to 75 units a second
  in the last moment, against about 300 before (Step 138). Nobody has watched it.

### Open design questions (moved from CONTEXT_INDEX §7, 2026-11-11)

Do not treat any of these as decided. Ask the owner.

**Blocking**

1. **Enemy AI beyond Wolf Hunt.** `Assets/Scripts/Tactical/EnemyBrain.cs` drives the Garuda. It
   hunts the map, hears us by our signal, searches, then turns a side to us and fires railguns and
   torpedoes (Step 97). It never leaves the battle space (Step 155). Anything past that one brain is still unspecified in every document, including
   the GDD.

**Not blocking**

2. Squadron size and fuel. Whether fighters need orders beyond a target. Whether craft must be
   recovered to rearm. Whether losses persist between missions.
3. Whether starbase hulks are destructible.
4. Whether a dry trigger pull should wake the alert net.
5. Who draws the real hulls. Two tiers per class — a detailed silhouette and a separately drawn
   plot glyph — is the largest art commitment in the project. The broken starbase
   (`Assets/Prefabs/Terrain/BaseHusk.prefab`) is placeholder shapes. No level uses it since Step 107.

### The level rework (owner 2026-11-09): wrong text in the old levels

The owner said on 2026-11-09: too many tutorials. After the current work is in, the levels get
reworked as a set, and the mission map is cleaned up. Until then, don't fix small text in the old
levels. These items are for that rework, not to fix now.

- **Since Step 170 (one nose gun, no fighters), briefings are wrong.** WEAPONS FREE
  (`FirstShot.unity:2584`) and NO PEEKING (`SilentShot.unity:2623`) talk about each gun's dotted cone.
  FINDERS KEEPERS (`Prize.unity:2610`) says to pick the interceptors. WOLF HUNT
  (`Level_01.unity:10629` and `10633`) says the fighters help, and talks about a gun's cone.
- **From the Learning report** (`docs/team-reports/learning/2026-11-08.md`): INTERCEPT's row still
  says "FIND IT ON DRADIS" (`01_Briefing.unity:182`). COLD RUN asks for C2. In NO PEEKING, ZOOM IN
  leads to a lock, and a lock loses.
- **FLIGHT DRILL's step 5** still says the slide stops when you let go. Left on purpose (owner).
- **COLD RUN:** check its walls under the interference rule (Step 148 replaced the full block, so the
  old result no longer counts). Have someone fly it with the real keys.
- Splitting WEAPONS FREE and the FLIGHT DRILL calls (both below) probably belong to this rework now.

### Smaller, not built (found by the audit, 2026-11-09)

- **Nothing damages a fighter** (Step 150), and its bar never changes. The code calls the bar fuel.
  The owner calls it health. Low while our ship has no fighters.
- **Step 144:** the Level window doesn't draw FLIGHT DRILL's checkpoint rings. Enemy ships have
  side-thruster numbers, but nothing slides them.
- **Step 148:** no level uses `MissionGoal.FindBySignal` (`QuestSystem.cs:244`). A small clean-up.
- **Step 125:** the playtest switches crashes off, so crashes are never tested.
- **Step 70, the owner's intro list:** the corner text and clearance squares are built (Step 181,
  owner 2026-11-10). The STRATEGIC SYSTEMS DIVISION line is not wanted: `References/LogoREADME.md`
  says the logo has no subtitle. Still open: a frame with open sides and a lit rim on the screen.
  Not built. **Unsure** whether still wanted after Step 174.
- **Step 114:** the MISSION ONE and WOLF HUNT cards are kept switched off in `01_Briefing`.
- **Step 36:** the COMPASS eraser area is a square (`PlotPanel.cs`, about line 613). **Step 43:** on
  the amber grid, the sweep's tail uses the edge colour, not the design's deeper red.
- **The spec's §18 "error cone"** is not built, and was not listed until now.
- **Clean how-to notes, one per board section.** The owner said later. Wait for their go.
- **Maybe later:** a signal readout at the top of the board. The cutscene experiment's next pass:
  bigger height gaps, top-to-top passes.

### Ship movement feel: open questions (Steps 171 and 175, 2026-11-09)

The owner played the new keys. Turning in steps at the start was fixed in Step 175. These are still
open:

1. **The drift felt too strong. Done (owner 2026-11-09: "let's make it 5 seconds for the slide").**
   After Q or E, and after a turn, the slide now halves in 5 seconds (it was about 8). Drift Keep Per
   S on our ship went from 0.9143 to 0.87 (Ship tab → Navigation): the ship keeps 87% of its slide each second. The other ship files keep
   0.9143. Logged in `docs/TUNING_LOG.md`. Not yet felt in play.
2. **Tapping W.** Owner 2026-11-09: a tap stays at 1 ("even tapping W is gradual… I don't think you
   need to go to 5"). "Let's explore. Maybe it's on a curve." It already is: held, the throttle counts
   slowly, then faster, to 100 after 2 seconds. Only the 2 seconds is in Tuning. **To do:** move the
   tap size (1) and the curve's shape, typed into `HelmPanel.Throttle`, into Tuning so the owner can
   try other values.
3. **An engine flare.** The owner wants to see the engine fire when holding W. Talk through the look
   before building it. Side puffs for Q and E could go with it.
4. **Numbers made without the owner**, all in Starwolf → Tuning: a Q or E tap is 2 units a second;
   the push grows over 2 seconds; the throttle goes 0 to 100 in 2 seconds. Held a little longer
   than a quick tap, A or D now turns more than 3 degrees.

### FLIGHT DRILL: calls for the owner to check (Step 144, 2026-11-01)

MILK RUN's level was copied from FLIGHT DRILL, so the calls below apply to MILK RUN too. FLIGHT DRILL
itself is switched off. The owner hasn't checked these calls.

3. **ENG 1 is half of full ENG** for acceleration and turn rate: 1 to 2, and 10 to 20° a second.
4. **ENG 0 still turns**, at the ENG 1 rate. It has no thrust.
5. **Side speed** is 20 at full ENG.
7. **A slide adds no signal.**
10. **No speed readout.** The full-throttle step ends at speed 45, and the player can't read the speed.
    The note says to take the throttle to 100.
11. **A hard check at step 1.** ENG must be on STANDBY with 1 to 3 units. Past 3, the player has to
    VENT down first.

### Review of the game, and the owner's answers (2026-11-01)

**The jobs, in order.**

2. **Split WEAPONS FREE.** It takes about 19 actions to reach the first shot. The plan is in
   `FTUE_PLAN.md` §0. **Probably overtaken (unsure):** the owner said on 2026-11-09 that the levels
   get reworked as a set later ("The level rework", above).
3. **Real sounds.** Every board sound is a generated tone. The only sound file in the game is the
   intro hum. Start with the switches, the lever, the railgun, hits and the kill. Where the sounds
   come from is not decided.

### Triage: everything left, sorted (2026-10-29, killshot-a2)

10. Optional: give the terminal Accessibility permission, so a session can play the app itself.

**Traps for the next build.** The disk is nearly full (about 4.4 GB free on 2026-11-01; each app is
about 340 MB). Never launch the app with a window size: it saves it. The ARM64 setting is saved in the
project since c3044fa, so it no longer goes back to Intel.

**Later, after the MILK RUN build.** The owner's goal of 2026-11-11, at the top of this section, comes
first. These come after it, each planned with the owner:

- **Refactor** ("Board refactor" below). Step 2: the playtest into the Test Runner. Step 5: move the
  last scripts out of `Scripts/Tactical`. Step 8: the clock, pause and tick from `BoardManager` into
  `GameManager` (move `ReadOnlyAttribute` out of the old `GameManager.cs` first). Step 3: the unused
  files were deleted on 2026-10-29 (below); left: `PowerSystem.Climbing` and `.Available`, which
  nothing calls.
- **Tools:** the mission scripter (Starwolf → Mission). Plan it with the owner first. The Mission
  Setup Tool chat is talking it through (call signs, who speaks, a line for each checkpoint). It is
  planned on top of the Campaign file, `Assets/Campaign/Campaign.asset` (Step 183). No Starwolf →
  Mission menu item exists yet.
- **Features, each planned with the owner:** the combat autopilot (A/P); ruins block shots; salvage,
  fuel and range; enemy boarders; the conditions page abilities and the FIRE, BLKHD and SCRUB
  switches; a verdict on Soham's 8 unused scripts (read each in full first); spoken orders; the XO
  walkthrough, derelicts, the disguise and fog of war; the old build's helm art (the shader the owner
  liked and the orange nav wheel). Done since: the grid forming (Step 120), the periscope showing the
  real enemy (Step 109), the plot grid touch-up (Step 139), ruins hide us (Step 148: rocks cut DRADIS
  and scans, and block sight, both sides). **Probably done or overtaken (unsure, 2026-11-09):** what
  is left of the hangar launch (Steps 147, 149, 160 and 162); a faster way to dial long ranges (Step
  170's fixed gun needs no ANGLE or BEARING).
- **Small jobs:** Board follow-ups 3 and 4 (helm, orders and periscope don't slide; the wreck's grey;
  one contact model at a time, now only past the close-up: since Step 164, at x1.1 and closer, every
  ship shows as its model). From Step 27: `Title` and `StartButton` in `00_Start` are switched off,
  not deleted (delete with the owner's yes); SETTINGS does nothing. The whole-project folder cleanup
  (the unused files went on 2026-10-29). Done since: LOAD GAME is now LEVELS, and opens the MISSIONS
  list (Step 167). **Probably overtaken (unsure, 2026-11-09):** the corner microtext (it belonged to
  the CRT look the flat intro replaced); the Claude Design request (the owner now brings art from
  ChatGPT and Codex).

- **The XO walkthrough and the derelicts are still wanted, later**, once Wolf Hunt feels right.

### For later talks (owner, 2026-11-01)

The owner asked for these to be kept for later discussion. None of them is built.

- **Signal is really heat.** The ship has radiators. The more power it uses, the more heat it
  radiates. Gas clouds cool it down. The board keeps the words signal and emissions for now. When
  the UI is next updated, decide whether it says HEAT instead.
- **Interference from radiation** (owner 2026-11-02). A place in a level that gives off radiation.
  Inside it, DRADIS shows false dots, for us and for the enemy. Not built. Step 157 removed the false
  dots near rocks.

### Play-through 2: scan, fighters, missiles, boarding against the live Garuda (2026-10-29, Worker)

**Found (for the owner):**

1. **Fighters always know where the enemy is.** They fly to its true position even when we have only
   heard its bearing. The launch bearing only sets where they take off; there is no "scout along a
   bearing" order. So scouting can't fail, and the hunt loses its search. **Paused** while our ship
   carries no fighters (Step 170: the Asura has 0, and the enemy brain launches none). Keep it for
   when fighters come back.
3. **One magazine hit leaves us defenceless.** The magazine is behind the bow. Since Step 180 the owner
   can move it in Tuning (Hard Points tab → ROOMS). A wrecked magazine stops
   the railguns and missiles, and the PDCs too: PDC fire needs `WepReady`, which needs the magazine,
   though PDC rounds are counted per mount. Should PDCs keep firing with the magazine gone?
   **Answered in part** (owner 2026-11-01): a hit magazine can't fire, and its crew repair it
   (`EnemyBrain.cs:129`). **Still open, unsure:** whether a wrecked magazine should also stop the
   PDCs was never asked as a separate question.
4. **Its railguns don't miss.** Drifting straight at 14,000 km, we took a slug every 15 game seconds:
   three opened the bow, wrecked and breached the magazine, and the vacuum spread to every room; the
   reactor fell to 2 units and DRADIS went deaf. The same as the first game. Our missiles take about
   150 game seconds to cover 15,000 km; its slugs, 6. (The owner chose "whoever shoots first"; here our
   fighters shot first, and still lost the duel.)
5. **"HANGAR BAY DAMAGED — CANNOT SPOT CRAFT"** shows when the hangar is only airless (integrity 5/5).
   Wrong words, or the wrong rule.

Two more for the owner, from the hygiene pass (the Worker's notes): `NavSystem.Steer` caps the throttle
by ENG units without the engine room's state, while the W/S cap and the fader include it (the board did
it that way, HANDOFF_GAPS row 32; harmless today). And `WeaponSystem`'s PDC `quads` flags are the
design's quadrant switches, never drawn; with PDCs in two halves they are probably not needed. Both are
the owner's call.

### Distances in units, not km (owner, 2026-10-29). Sizes built in Step 111, speeds and ranges in Step 130; what the player reads still to do

From KM to Units. The sizes step and the zoom-out to x0.01 are built (Step 111). The code's names are
all units (Step 113, the owner: "Everything should be [units]… This is all backend"). Speeds and
ranges came in Step 130, and zooming out in Steps 130, 135 and 139. Left: what the player reads on
screen, km or anything else. The ship sizes below are replaced by Steps 126 and 127.

### Plot zoom-out (owner, 2026-10-29). Built in Step 110; the owner's answers built too

- **Still to do, owner's call (2026-11-01):** the Garuda's 350 x 100 is a trial. ~~Our ship's old
  VLS has no hard point. Choosing MISSILE now picks the first launcher on a dot, VLS-A (Step 143). It
  fires to the left side only. No control picks VLS-B yet.~~ Done in Steps 147 to 150: the launcher on
  the target's side fires (`WeaponSystem.PickLauncher`, commit 1c19d9c). If a ship's size changes,
  move its hard points to the new edges too.

### Comms intercepts (owner, 2026-10-29). Built in Steps 137, 140 and 148

**Still to do.**

- A read message goes when the next one comes in. There is nowhere to look back at old ones, and the
  plan is to do the mathematics across several messages.

### Found by two audits (2026-10-29, Worker)

**Owner requests never done, or only partly** (not in the triage above):

- **Supercruise switch on the helm** (owner, 2026-10-25). Never answered, never logged.
- **Set the heading by bearing or with the mouse** (2026-09-01; the old orange nav wheel on the helm,
  2026-10-04 and 10-17). Only A/D work, 3° a step.
- **A stale firing solution expires after about 20 s** (2026-09-01). Not built, never logged. Maybe
  dropped when the handoff's fire sequence came in; nobody confirmed. **Probably overtaken (unsure,
  2026-11-09):** Step 141's lock lasts 10 s, which covers most of it. A plot TRACK never expires.
- **Salvo, PDC quadrants and LOADERS on fire control** (2026-10-04, 10-23). Salvo is in the Tuning
  window only; the others are not drawn. ~~Warhead controls~~: wrong. The mount knob picks the
  warhead (`FireControlPanel.cs:89`, since Step 53).
- **~~Ruins hide us;~~ ruins block shots** (2026-09-01). Listed under features. Gas clouds hide us
  since Step 129. Ruins hide us since Step 148 (rocks cut DRADIS and scans, and block sight, both
  sides). Ruins blocking shots is not built: `WeaponSystem` has no rock check.
- **The cloak (Reflex)** (2026-09-01: "I like the cloak"). Only the spec mentions it.
- **Waypoints** (2026-09-01: "later"). Not tracked anywhere until now.
- **Engineering's missing displays**: conditions per room, broken parts, team roster, door state
  (owner 2026-10-24: "request + placeholder"). Only the armour picture is done (Step 74). Damage build
  steps 5–7 (teams, breakdowns, displays) are open.
- **Fighter loadouts and fuel; salvage and intel from boarding** (2026-09-01). Not built. Fighter
  loadouts and fuel are **paused** while our ship carries no fighters (Step 170). Keep them for when
  fighters come back.

**Owner questions never answered** (beyond the triage): the autopilot's TAIL facing; the asset audit's
28 items (`ASSET_AUDIT.md`, in git at 12ccdd1); the interceptor art on the hangar panel (**paused**
while our ship carries no fighters, Step 170); the calls made overnight in Steps 94–101 (none
confirmed yet). Answered since: our slugs glide (Step 138); stability is left as it is (owner,
2026-11-01); every `report.md` is kept (owner).

**Added on 2026-11-09** (the audit of the whole log). Calls made without the owner that change how
the game plays. None was confirmed later.

- Step 158: a gas cloud's hollow circle shows on plain DRADIS with no scan. Rocks need an ACTIVE SCAN
  ping.
- Step 141: the newer of scan and plot sets the course. Fighters need no scan.
- Step 170: only a lock turns the ship, not a plot TRACK. LONG SHOT's Garuda neither hunts nor fires.
- Step 169: fires, radiation, vented rooms and crew losses don't carry over between missions. A room
  at 0 comes back at 1. A lost mission keeps no damage.
- Step 168: MISSION SELECT goes to the ship's rooms after a lost mission too.
- Steps 171 and 175, the movement numbers: see "Ship movement feel", item 4, near the top of
  section 8.
- Step 162: the hangar dots are 50 units back from the middle. With no line drawn, craft start level
  with the dot.
- Steps 77, 79, 80, 88, 91 to 93, 110, 111 and 113 to 115: calls never put to the owner. Still in
  force: Step 79, a PDC group under 100 rounds can't fire, and the stranded clock never resets. Step
  91, a spawn past the edge moves 3,000 inside. (Steps 88, 93 and 111 are overtaken.)
- Step 139: the grid strengths, 8% and 24%, were left for the owner to judge.
- The Garuda's random facing (QA) against the start rule of 2026-11-01: top, facing south. "Wolf
  Hunt", below, still says random.
- Player 1 (`docs/team-reports/player-1/2026-11-09.md`): three Garuda hits on our nose wreck the
  magazine and stop the spinal railgun. Nobody wins Wolf Hunt at x5. LONG SHOT has a 5 to 9 minute
  trip with nothing new.
- The producer's roadmap (2026-11-08) waits on 11 owner questions. They are in
  `docs/team-reports/producer/2026-11-08.md`.

**Answered by the owner but not acted on:** the damage displays (10-24). The grid glide-in was built in
Step 120. (`Assets/Screenshots`, `PlotAnnotation.cs`, `TacticalGrid.shader/.mat` and the
other unused files were deleted on 2026-10-29, with the owner's yes.)

**Restructure: done**, apart from what the triage lists (refactor steps 2, 5 and 8). Also found:
`BattleSpace.cs` belongs on step 5's list of scripts to move out of `Scripts/Tactical`; the first plan
renamed `TacticalWorld` to `WorldManager`, and no record says that was dropped (owner's call); the
unused files from the old 02_Grid commit (4e43ea9) were deleted on 2026-10-29; one object still has a record-number name, `text#406 LOWERED` in
`02.1_Grid` (rename in the Editor); `HangarSystem.cs` scuttle sets the reactor to 0 directly instead
of through `DamageSystem`; `PlotPanel.cs` asks for a text field every frame (minor).

### Board follow-ups

3. **Helm, Orders and the periscope don't slide yet** (decision 35). They open at once, or drop
   inside their own clip. If they are made to slide, clip them to the frame's opening and keep them
   under the rails, as Step 66 did for the drawers.
4. **Left from Step 75.**
   - A wreck slows to a stop. From full speed it takes about 30 game seconds. Owner 2026-10-26:
     keep it that way, or a little quicker. A wreck must not coast on at its speed. The rate is
     `NavSystem.Stats.brakeKms2`, in each ship's stats file, shared with live ships.
   - The wreck's grey (`wreckInk`, #8A959D) is pale next to our white ship. Tune it if it is hard to
     tell apart.

### Board refactor (owner, 2026-10-25). In progress (paused and restarted by the owner, 2026-10-26)

**Rules.** Do the steps in order, one step per commit. The playtest must pass before the next step.
Confirm each step with the owner before starting it. Managers are scene-wide, one of each. Systems
are ship parts, one set per ship.

2. **Move the playtest and the intro letter check into Unity's Test Runner** (decision 31). Left.
   There are no Test Runner tests in `Assets` yet.
5. **Move files into folders:** `Scripts/Systems`, `Scripts/UI`, `Scripts/UI/Panels` and
   `Scripts/Game`. Move only. Part done: the ship parts and the eight panels are in place. The rename
   moved the "Handoff" ones (Step 90). Left in `Scripts/Tactical`: `TacticalWorld`, `WorldContact`,
   `EnemyBrain`, the spawns, `BattleSpace`, `ScreenFlow`, `GammaBoardPresenter`, `ShipView`,
   `TuneAttribute` and `CssEase`.
8. **Move the clock, pause and tick out of `BoardManager` into `GameManager`.** First move
   `ReadOnlyAttribute` out of the old `GameManager.cs`, then delete that file. Left.
   `ReadOnlyAttribute` is still in `GameManager.cs`.

### Soham's old scripts (owner, 2026-10-25)

`Assets/Scripts/` (outside `Tactical/`, `Systems/`, `UI/` and `Game/`) is Soham Aradhye's work. The board doesn't use it, but that
does not make it dead code. **Keep all of it until the owner decides.** Read a script in full before
judging it. `CODEBASE_BASELINE.md` has a line-by-line read of each one.

Where the board already works, the board's version stays. That covers power, damage, sensors, comms,
the weapon rules, the hangar and the game-time tick.

His code has things the board lacks:
- **The mission:**
  - `GameManager`: spawning, winning and losing.
  - `GameClock`: the mission time limit.
  - `QuestSystem`: locate, eliminate, exit.
  - `WorldBoundary`: leaving the map. It overlaps the battle-space edge from Step 65.

  These scripts need the board's tick and its "ship destroyed" flag instead of Unity frame time and
  deleted objects.
- **One component per ship part,** talking to each other through events.
- **One class per weapon** on a shared base (`WeaponBase`, `WeaponManager`). The board's weapons
  stay in `WeaponSystem.cs` (owner, 2026-10-26).
- **Enemy types with their own stats, and patrol, chase and combat states** (`EnemyShip`). Move these
  into `EnemyBrain`, which gives orders instead of using physics.
- **Contacts that sign themselves up** (`RadarTarget`), for enemies spawned mid-mission.
- **A stability that drains on hard turns** (`ShipStability`). Today the board's stability is only a
  number on the helm. ~~Open: should it be a mechanic?~~ The owner said on 2026-11-01 to leave it as
  it is.

Don't copy:
- Unity frame time.
- Physics bodies.
- Finding the player by tag.
- Pausing by freezing Unity's time.

`Autopilot.cs` is set aside (see below).

### Combat autopilot and W/S (owner, 2026-10-25). Autopilot agreed, not built; W/S built in Step 106

The full rules are in `TACTICAL_BOARD_SPEC.md` §4.6. In short:

**Settings**
- The target is fire control's selected target. It must be identified by an active scan.
- The facing (NOSE, STBD, PORT, TAIL) is the side kept toward the target.
- It tries to stay at the approach range.
- The approach mode (DIRECT, FLANKING, EVASIVE) sets the speed. The owner tunes the speeds.

**How it moves**
- A beam facing circles the target: STBD clockwise, PORT counterclockwise.
- NOSE closes to the range and keeps the nose on the target. If the target stops, ours stops too.
- TAIL is not defined yet.

**Engaging and disengaging**
- Nothing moves until a new **A/P** key on the helm is pressed. The status line then reads
  AUTOPILOT ENGAGED.
- W, A, S or D, or dragging the throttle, disengages it at once. The settings stay, so one press of
  A/P resumes.

Neither the design package nor the audit gives the autopilot any behaviour. Its controls only set
values, and approach mode only adds G.

### Ruins and the level, next (from Step 73, 2026-10-26)

1. **What ruins do, still to come.** Block shots (the prototype's penetration against toughness:
   asteroid 3, structure 4, ruins 3, starbase 5). `WeaponSystem` has no rock check yet. ~~Hide us
   from sensors (line of sight).~~ Done in Step 148: rocks cut DRADIS and scans, and block sight,
   both sides.
2. **Smaller.** The wrecks are whole ships (break some apart later). Soham's `Asteroid.cs` could make rocks
   breakable later (add the "Asteroid" tag first). WRECK-4 and WRECK-11 use the same cruiser model
   as the Garuda. ~~The ship models go to 100–500 m when the outside cameras come.~~ Done in Step
   164: the models are 200, 350 and 100 units.
