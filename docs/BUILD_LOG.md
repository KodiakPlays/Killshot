# Tactical Scenes — Build Log

A running record of the new scene flow being built under `Assets/Scenes/Tactical/`.

**Read this first if you are a new session, or this session after a compaction.** It tells you what
exists, what is wired to what, which step we are on, and what to do next. You should not have to
work any of that out again.

Last updated: 2026-11-10. Unity 6000.5.10f1. Editor instance `Killshot@99ec1537206126d5`.

---

## 1 · Where we are

**Every step in the table below is done.**

**The board refactor is mostly done** (section 8, "Board refactor"). All eight ship parts are
systems in `Assets/Scripts/Systems`. Each ship's numbers are in its stats file in `Assets/Ships`.
All eleven panels and seven frame pieces are prefabs in `Assets/UI/Board/Panels`. The board's colours
are in one palette file. The fonts are in `Assets/UI/Fonts`, with a Fonts list on each panel (Step
89). The "Handoff" names are gone (Step 90). Left: the Test Runner (refactor step 2), moving the last
scripts out of `Scripts/Tactical` (step 5) and the clock move (step 8). The order of all the remaining work is in the
owner-approved plan: settle, finish the refactor (new features paused), then the Tuning and Level
tools, then the mission scripter, then features.

The game now opens on the SECTION 36 intro, which runs itself through to a menu: NEW GAME, SKIRMISH
and SETTINGS (Step 178). The intro and loading screens have the richer green and every letter and number, and the loading screen has the corner text (Step 181). NEW GAME opens SAVE SLOTS, then the SECTOR MAP, where each mission is a dot and a win
shows the next dot (Step 167). After a campaign mission, the ship's rooms show, then the map
(Step 168). The game will be about 10 story missions, plus SKIRMISH (owner 2026-11-09). The map has
one so far: 1 ORIENTATION (Step 178). The map's missions are in the Campaign file,
`Assets/Campaign/Campaign.asset` (Step 183). SKIRMISH plays Wolf Hunt straight away. The MISSIONS list and
the other old test levels are off every list. Their scene files are kept and switched off. They are never deleted (owner).

The board looks and clicks right, and you can fight on it. Our ship is the Asura-class destroyer.
It starts at the level's Player Spawn. The enemy is the Garuda-class cruiser, `Garuda · Cruiser` in
the scene. It starts at a random point in its spawn box in `Level_01`, hunts us, and fights back
with railguns and missiles (Step 97). You can hear it on DRADIS, plot it and scan it. Our ship has
one big gun on its nose, the spinal railgun. You aim it by turning the ship (Step 170). You can also
fire torpedoes at the Garuda, and the PDCs shoot down its torpedoes. You can launch assault boats at
it. Our ship carries no fighters. You can destroy the Garuda or capture it. It can kill
our ship. The first mission, Wolf Hunt, is playable (Steps 92 to 107). **Starwolf → Tuning** edits each ship's stats file and
the board's own values. Each step's detail is in section 6.

**Changed direction on 2026-10-04.** A new Claude Design handoff arrived. Its
reference files are now in `Assets/UI/Board/Design`. It is fully built into `02.1_Grid`, and the final QA
passed. Every control is clickable and shows its states. The flow is `00_Start` → `01_Briefing` →
`02.1_Grid`, with `Level_01` loaded on top of the board. The full record is
`Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md` (deleted; `git show 12ccdd1:Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md`).

**Checked on 2026-11-01 (Step 143).** Everything built up to Step 142 has now been played on the final numbers, in the running game, by a script using the board's controls. Step 143 in the table has each result. Nobody pressed a key: keys 1 to 6, P, W, A, S, D and Escape are for the owner to try. The full playtest has not run since Step 141, so it hasn't yet run the new message wording, the fixed gas cloud check or Step 142. Pressing against a rock with the throttle on grinds: repeated small hits, one every 3 game seconds. The white corners stay (owner 2026-11-01).

**Smooth movement (Step 138), measured 2026-11-01.** The shots' dots were measured frame by frame. Nobody has watched them yet. A slug's dot never stops, and it reaches the hit area within 2 units as the hit lands, at x1 and at x5. Just after firing it goes about 340 units a second, then about 300. In the last tick before a hit it slows to 60 to 75. That slow last tick is the one thing to judge by eye. Fighters fly and land. A scanned ship under the ACTIVE SCAN cone moves on every frame, stops when ACTIVE SCAN goes off, then hops every 4.9 s. A ship not scanned showed as a bearing line, and the line hopped every 4.9 s. Since Step 161 it is a red bar on the DRADIS circle, and the bar turns on every frame. A short fight video is at `Logs/FightVideo/fight.mp4`: 16.5 seconds, no sound, Wolf Hunt, at x1. The `Logs/` folder is ignored by git, so the video is on this machine only and is not in the repo. A script staged the fight. It put the two ships 1,300 units apart, turned the power on, set Condition 1 and marked the Garuda as scanned. The script added our slugs, missiles and fighters. They were not fired through the switches. The Garuda chose its own moves and fired back by itself, and the hits, the damage and the kill came from the game's own rules. The Garuda dies 14 seconds in and splits in two on screen. The missiles are shown as they were before the owner's new missile changes. The script was run once in the Editor and is not saved in the repo. Nobody has watched the video yet.

**Next up:** everything left is sorted in **section 8, "Triage: everything left, sorted"**. Start there.

**Agreed on 2026-10-25, not built:** the combat autopilot. W/S throttle was built in Step 106. It is in section 8,
with the owner's ruling on Soham's old scripts.

The owner is building this one step at a time and checking each step before starting the next one.
Do not run ahead. Do one step, show it, wait.

| Step | What it is | Status |
|---|---|---|
| 1 | Folder, three empty scenes, Build Settings | **Done** |
| 2 | Build `00_Start` — camera, canvas, title, START GAME control | **Done** — checked on screen |
| 3 | Build `01_Briefing` — same canvas, work-in-progress text, "Press Enter" prompt | **Done** — checked on screen |
| 4 | Write `ScreenFlow.cs`, wire both intro scenes, test the chain in play mode | **Done** — ran twice in play mode |
| 5 | Build `02_Grid` — the tactical grid surface | **Done** — checked on screen |
| 6 | Corner panels fixed and verified | **Done** — checked in play mode |
| 7 | Linear colour space fix on the grid | **Done** |
| 8 | Art profile ScriptableObject created | **Done** |
| 9 | Tool icons corrected to the handoff | **Done** |
| 10 | Grid turned amber, with a third tier of lines | **Done** |
| 11 | The orange panel bug, and the layer that fixes it | **Done** |
| 12 | The two icon rails | **Done** |
| 13 | Power controls brought over from the cockpit | **Done** |
| 14 | The board's data layer, and the targeting drawer | **Done** — checked in play mode |
| 15 | The outer metal frame | **Done** |
| 16 | Frame thinned | **Done** |
| 17 | Engine power and WASD movement | **Done** — power verified, keys not |
| 18 | Three bugs on the plot, and the plot camera | **Done** |
| 19 | Tuning asset | **Done** |
| 20 | Why the power bars never moved | **Done** — checked in play mode |
| 21 | Plot layer, part one: camera, zoom range and input | **Done** |
| 22 | Own ship glyph: minimum size instead of constant size | **Done** |
| 23 | Plot layer, part two: annotation rendering and the tools | **Done** — every tool checked in play mode |
| 24 | Cut to five tools, MOUSE as default, active-tool highlight | **Done** — checked on screen |
| 25 | Plot labels — measurements shown on the board | **Done** — checked in play mode |
| 26 | Ship readout — speed and heading | **Done** — checked in play mode |
| 27 | Intro and menu wired into the flow, and four rendering bugs in the intro package | **Done** — flow checked in play mode; CRT treatment not yet seen on screen |
| 28 | New scene `02.1_Grid` for the new handoff, wired after the briefing | **Done** — flow checked in play mode |
| 29–33 | The new handoff built into `02.1_Grid`: import, builder, static board, movement, clickable controls (33a–33j), final QA | **Done** — FINAL QA: PASS. Detail in `UI_INTEGRATION_LOG.md` |
| 34 | Wiring begins: the dummy enemy exists and the board reads it | **Done** — verified (STEP 34: VERIFIED on the second pass) |
| 35 | Power makes the ship loud (signal), and nothing real changes while paused | **Done** — verified (STEP 35: VERIFIED, first pass) |
| 36 | Plot tools: marks stay put in space | **Done** — verified (STEP 36: VERIFIED, third pass) |
| 37 | Our ship on the plot, as the 3D model seen from above | **Done** — verified (STEP 37: VERIFIED, second pass) |
| 38 | Every tunable value in one window (Starwolf → Tuning) | **Done** — checked; one defect fixed and re-checked |
| 39 | Nothing standing on the plot | **Done** — checked after a rebuild and a playtest |
| 40 | Press and click: switches slide, refuse, and click | **Done** — playtest 48/48, slide measured in play |
| 41 | The helm: our ship moves, and the plot pans | **Done** — playtest 57/57 |
| 42 | The sensor circle: DRADIS's hearing range around our ship | **Done** — playtest 60/60 |
| 43 | The radar sweep | **Done** — playtest 62/62 |
| 44 | Hearing the enemy: bearing lines, the alert strip, the tracked blink | **Done** — playtest 68/68 |
| 45 | The bearing trail: plotting a passive contact's range | **Done** — playtest 70/70 |
| 46 | Intercept helpers: time to reach, tracks, weapon reach | **Done** — playtest 76/76 |
| 47 | The firing solution dialled by hand | **Done** — playtest 81/81 |
| 48 | Fire and hit: the railgun slug | **Done** — playtest 93/93 |
| 49 | Damage: armour, compartments, the kill | **Done** — playtest 99/99 |
| 50 | Engineering's ship diagram | **Done** — playtest 103/103 |
| 51 | The enemy moves: TRACK 07 patrols | **Done** — playtest 107/107 |
| 52 | Leading a moving target | **Done** — playtest 113/113 |
| 53 | Missiles: the launcher, homing, three warheads | **Done** — playtest 121/121 |
| 54 | Fighters: launch, fly out, attack runs, recall | **Done** — playtest 130/130 |
| 55 | PDCs, and the enemy fires back | **Done** — playtest 138/138 |
| 56 | The intro: flat black, typed in place, straight cuts | **Done** — checked in play mode |
| 57 | Everything in game time | **Done** — playtest 138/138 |
| 58 | The audit's side map; our ship can die | **Done** — playtest 139/139 |
| 59 | Conditions: fire, vacuum, radiation | **Done** — playtest 153/153 |
| 60 | Boarding: the assault boats | **Done** — playtest 160/160 |
| 61 | Scan to know what you hit; smoothing; heading 000 | **Done** — playtest 161/161 |
| 62 | Weapon reach, the sweep's inner circle, the enemy's picture | **Done** — playtest passed with Step 63 |
| 63 | Scale: everything that moves 5× faster; ACTIVE scans; the gain comes back (undone in Step 64) | **Done** — playtest 163/163 |
| 64 | Lose power, lose the setting | **Done** — playtest 163/163 |
| 65 | The battle space: a map with edges, the grid resizing inside it | **Done** — playtest 163/163 |
| 66 | Our ship stops blinking; drawers slide out from behind the frame | **Done** — playtest 163/163 |
| 67 | The Level tool: each level is a scene; the enemy spawn box | **Done** — playtest 163/163; spawn checked in play mode |
| 68 | The signal alarm: a box in the frame's top-right, and the alert tone | **Done** — playtest 165/165 |
| 69 | Each DRADIS sweep pass beeps | **Done** — its check passed; full run still to re-run |
| 70 | Intro colour: drawn through a Canvas (no double encode); dark green field | **Done** — measured in play mode against the reference shots |
| 71 | Welcome page on the phosphor screen (DEMO title); loading bar under INITIALISING | **Done** — checked on screen and by the owner; playtest not run (owner) |
| 72 | Mission clock and signal box lined up with the plot edges | **Done** — measured on screen |
| 73 | Ruins in the level: the starbase husk and a debris belt; a Player Spawn at the bottom, the enemy's at the top | **Done** — playtest 168/168 |
| 74 | Engineering's ship picture shows side armour; the room rings change colour | **Done** — playtest 168/168 |
| 75 | Ships glide between ticks; DRADIS shows a contact where the sweep last touched it; in sight (10,000 km) its model shows; wrecks stay, grey; engineering's hull redrawn; weapons and DRADIS reach 30,000 km | **Done** — playtest 170/170 |
| 76 | Axis numbers are distances from our ship; the axis titles and the frame stencils TCB-1 and ACC 07 are gone | **Done** — playtest 171/171 |
| 77 | The debris scatter tool: a Debris Field in a level (shape, pieces, count, size, gap, spread; Scatter and Reshuffle) | **Done** — layout check ok; tried in `Level_01` (not saved); playtest not needed (nothing runs at play) |
| 78 | The active scan cone on the plot: two bright edges and a faint fill, from the small circle round our ship out to the scan's range | **Done** — checked by screenshot; playtest not needed (visual only) |
| 79 | Ammunition is a real count; boarding goes on while the target moves; the boats' 15,000 km range | **Done** — playtest 191/191 |
| 80 | The Tuning window's tabs (Ship, Enemy, Board) and a stats file per ship | **Done** — playtest 191/191 |
| 81 | The Level window shows the level from above, with a preview of the picked debris field; the railgun slug at 2,500 km/s | **Done** — playtest 191/191 after the fixes |
| 82 | DRADIS needs SEN power: no unit on SEN, no hearing, no circle, no sweep | **Done** — playtest 193/193 |
| 83 | Enter on the briefing always leads on, even if INITIALISING can't start | **Done** — playtest 193/193 |
| 84 | Prototype Dev off the build list | **Done** — playtest 193/193 |
| 85 | Win, loss and restart: the mission script, the end screen, RESTART MISSION and MAIN MENU; SHIP LOST clears | **Done** — playtest 201/201 |
| 86 | The playtest checks the battle space's edge: zoom limit, axis numbers, pan limit, `C`, the warning, the x1 drop, the countdown | **Done** — playtest 209/209 |
| 87 | The Mac app | **Done**: the owner played the app on 2026-10-28. |
| 88 | A shot hits within 100 km of any ship; a hit's damage comes from the weapon that fired it | **Done** — playtest 209/209 |
| 89 | UI restructure steps 1 to 5: fonts in `Assets/UI/Fonts` with a Fonts list per panel; materials; two stencils and the plot's mock-up deleted; the old export's reference files kept, the rest of `UI2` deleted | **Done** — 0 pixels changed by the fonts; playtest 209/209 |
| 90 | UI restructure step 6: "Handoff" is gone from every script name; the board's root object is `Tactical Board` | **Done** — no missing scripts; playtest 209/209 |
| 91 | Each level has its own battle space: a Battle Space box, set in the Level window | **Done** — quick check in play mode |
| 92 | Ruins and debris on the plot: inside the DRADIS circle with DRADIS on, else within 10,000 km; line art on top of the circles | **Done** — playtest 209/209 (2026-10-28_012951) |
| 93 | Debris you can fly through: Level_01's crowded belts thinned out; running into a piece damages the ship, no push-out | **Done** — playtest 209/209 (2026-10-28_012951) |
| 94 | Wolf Hunt: WOLF HUNT, then INITIALISING, then the board; the DEMO page gone | **Done** — playtest 210/210 |
| 95 | Wolf Hunt: the Garuda starts at a random point in its spawn box | **Done** — playtest 210/210 |
| 96 | Wolf Hunt: the Garuda bigger, slower, tougher, harder-hitting, shorter reach | **Done** — playtest 210/210 |
| 97 | Wolf Hunt: the Garuda's mind: it hunts the map, hears us by our signal, searches, fights side-on with railguns and missiles | **Done** — checked in play mode; playtest 210/210 |
| 98 | Wolf Hunt: the jump point at the kill; reaching it wins | **Done** — playtest 210/210 |
| 99 | Wolf Hunt: the alarm, the 3:00 countdown, Naga frigates at the edges; click a contact to target it (undone in Step 106: the periscope picks the target) | **Done** — checked in play mode |
| 100 | Ruler: distance only, larger text; compass: graded 0–359 ring with the bearing; the jump point on the plot | **Done** — screenshot; playtest 210/210 |
| 101 | Fighters scout: ships and ruins near them show on our plot | **Done** — playtest 210/210 |
| 102 | Test every system, fix what is not fun | **Done** as Steps 105 to 107 |
| 103 | The Mac app of Wolf Hunt | **Done**: the owner played the app on 2026-10-28. |
| 104 | TextMesh Pro's example files deleted (owner's yes); its missing shader file `TMPro.cginc` put back: the 3 build errors are gone | **Done** — the shaders compile clean |
| 105 | A real play-through of Wolf Hunt in the Editor, and four fixes: a hit puts the Garuda on ALERT; "HOSTILE SET HAS YOUR RANGE" tells the truth; the alarm strip shows enemy fire coming and what just hit us, debris too | **Done** — checked in play mode; playtest 211/211 |
| 106 | The owner's play report: W/S throttle, P pause, the 10,000 km visual ring, MISSION ONE then WOLF HUNT, TRACK takes the plot, the periscope picks the target; bugs 7 and 8; the plot follows our ship; PDCs in two halves; drift with no ENG power; Level_02 gone; the bundle ID; with killshot-a2's 60,000 km Level_01 and bug 6 | **Done** — checked in play mode; playtest 214/214 |
| 107 | Wolf Hunt's level made smaller (60,000 km, the husk gone, 136 pieces); engineering pays AUX for its orders (bug 6); crew numbers in each ship's stats file | **Done** — playtest 214/214 (in Step 106's run); crew checked in play, COMPLEMENT 52 (7e82b40) |
| 108 | Keys 1–6 open the six drawers; Starwolf → Keys lists every board key and changes it | **Done** — compiled clean; drawers checked in play mode; the key presses themselves not yet pressed by a person |
| 109 | The periscope shows the enemy ship: the plot's top-down picture, level, pointing left or right the way it heads; zoom until it fills the brackets, then CLASS | **Done**. Checked in play mode (before and after shots: the red lines are unchanged); playtest 215/216 (2026-10-29_015659; my check passes; the one failure is #56 RECALL, frame timing, not this work) |
| 110 | The plot zooms out to x0.1; the grid carries on past the battle space, with a heavier line at its edge | **Done**. Compiled clean; checked in play mode at x0.1, white and amber (`Logs/GridCheck`); playtest section 25b updated, not run |
| 111 | Units: at x1.0 a small square is 50 units and our ship (100 units) is two squares long; the Garuda is 150; the plot zooms out to x0.01 | **Done**. Compiled clean; checked in play mode at x1.0 and x0.01 (`Logs/UnitsCheck`); playtest 218/218 (2026-10-29_021252) |
| 112 | Turning after TRACK no longer spoils the aim: the firing solution is a point in space; RNG and BRG follow the bow; ANGLE drops if the turn takes the target out of arc | **Done**. Compiled clean; checked in play mode (a still and a crossing target, both hit after a turn; the old way missed); full playtest not run |
| 113 | Everything in the game's workings is in units: every km name in the code is now units (range, speed, battle space and the rest); the numbers are the same; the screen still shows km | **Done**. Compiled clean; every saved value carried over (94 checked); playtest 213/218 (2026-10-29_100527): #205–206 the zoom was moved off x1.0 during the run (rechecked in play mode, holds), #89 and #111 follow Step 112's new aim, #56 the old RECALL timing |
| 114 | Training levels: NEW GAME opens the MISSIONS list (FIRST SHOT, SILENT SHOT, PRIZE, WOLF HUNT); each level has a Mission box with its goal; in the training levels the Garuda doesn't hunt | **Done**. Compiled clean; every goal's win and loss checked in play mode (`Logs/LevelListCheck`); playtest 217/218 (2026-10-29_110455): #5, the new list check, was wrong itself (fixed, rechecked in play mode); not yet played through by hand |
| 115 | The training levels renamed: WEAPONS FREE, NO PEEKING, FINDERS KEEPERS; a placeholder help bubble on the board says what to do in each level, Wolf Hunt too | **Done**. Compiled clean; checked in play mode (`Logs/HelpBubbleCheck`): the list, the bubble in Wolf Hunt and WEAPONS FREE, the longest text fits, a click closes it; playtest not run |
| 116 | Training-level play-through fixes: the Garuda no longer fires back in the training levels (two missile salvos had wrecked our magazine, so WEAPONS FREE could not be won); the bubbles no longer say "turn on DRADIS" (it starts on, so that switched it off); the mouse over a list row picks it (two rows were lit); the DRADIS sweep line is unseen (owner: it spins wildly at x10; Sensors tab, sweep opacity 0, still works) | **Done**. Compiled clean; WEAPONS FREE played through the real switches to two hits (found the bugs); after the fix a hit drew no missiles in 80 game s; NO PEEKING and FINDERS KEEPERS not yet played through; full playtest not run |
| 117 | A QUIT · ESC key on the board and the MISSIONS list. **Removed the same day** (owner: "I don't want any other quit feature"): Escape alone quits, as built before Step 116 | Removed; the board and the list are as before Step 117. Playtest check #5 still counts only the level rows |
| 118 | DRADIS starts off: the player turns it on (owner); the bubbles say "turn on DRADIS" again | **Done**. Compiled clean; checked in play mode (ours off, the Garuda's on, no circle, the bubble's line); playtest: a new check that it starts off; not run |
| 119 | The railgun turrets' cones on the plot while Fire Control is open (owner: "the guns need cones… they are not visible… I can't figure out how to shoot"): each turret's arc as a dashed sector out to its reach, the selected one bright, the other dimmer (PlotPanel railArcAlpha 1 / 0.45, on the Periscope & fire control tab; written into Plot.prefab with sweepOpacity 0). WEAPONS FREE filmed: Logs/LevelVideos/WeaponsFree.mp4, 2:23, x5 throughout, fly in, port beam, six hits, MISSION COMPLETE (before the cones) | **Done**. Compiled clean; cones seen in play mode (`Logs/LevelPlay/31_turret_cones_bright.png`); full playtest not run |
| 120 | The grid forms as the board appears (owner approved the look 2026-10-26; asked for again 2026-11-01, slower): the plot starts empty, the big-square lines glide in, vertical lines in turn from the top and bottom, horizontal lines from the left and right, then the small lines; then our ship, the axis numbers and the rest of the plot fade in. The briefing box waits until this is done. Grid Form Seconds 4 and Plot Fade Seconds 1.5, on the Board tab of Starwolf → Tuning, under Plot & our ship | **Done**. Compiled clean; seen in play mode (`Logs/GridForm`); see Step 125 for the playtest |
| 121 | Railguns redone (owner 2026-11-01: the cones were "too wide, too weird"). Each ship has hard points on its rectangle, in its stats file (`WeaponSystem.Stats.hardPoints`), units from the centre. RG-A is in the middle of the right side, RG-B the middle of the left side, and the dot on the nose is empty. Each gun has a 90° firing arc straight out of its side (was 160°), a soft dotted cone on the plot. Inside it a solid bar shows the gun. After TRACK the selected gun's bar turns at 3° a game second onto the target (Gun Turn Deg Per Second, in the stats file). The switches work as before. The shot flies down the bar from the hard point, so firing early misses. The Garuda and the frigate have the same layout. New tools: **Starwolf → Hard Points** (the ship as a rectangle, dots to drag, marks on each side, add and remove dots, what is on each dot, its direction and cone width, ship width) and **Starwolf → Ranges** (every reach as a ring to drag, with the plot's view at any zoom and the battle space to compare). Both are tabs of the Tuning window. WEAPONS FREE and NO PEEKING tell you to turn your side to the target | **Done**. Compiled clean; cones and bar seen in play mode, the bar turned 30° in 10 game s; the owner dragged dots in the tool; see Step 125 for the playtest |
| 122 | Any number of weapons (owner 2026-11-01). Each weapon in a ship's stats file has a kind: railgun, PDC or missile launcher. **Starwolf → Hard Points** makes new ones (New railgun, New PDC, New missile launcher: a copy of the first of its kind, on an empty dot), edits any weapon (name, short name, reach, ammunition, direction, cone) and deletes one. Every weapon fires from its dot: slugs, missiles and PDC arcs. A weapon on no dot can't fire: the drawer says NO HARD POINT. On the Asura, PDC-A and PDC-B went on the owner's dots at the back of each side, and the VLS on a new dot at the back centre; the Garuda and the frigate got the same layout. The Fire Control knob has two places: with more weapons they show the pair the selected one is in. The Garuda's brain uses all its weapons | Built, compiled clean; a new railgun checked in play, on and off a dot; see Step 125 |
| 123 | Ships are solid boxes (owner 2026-11-01). Every ship is a rectangle round its model, length by width from its stats file, turning with it: Asura 100 x 64, Garuda 150 x 75, frigate 75 x 37 (measured from the models; the owner's dots moved out with the edges). Shots hit the box, on the side they reach, within Hit Margin Units (25; was a 100-unit circle round the centre). Nothing passes through a ship or a piece of debris: ours, the enemies, ship against ship. A ship that touches something is pushed clear, bounces, keeps 30% of its speed, and takes damage on the side that touched, by its speed. Both ships take damage in a ship crash. Pieces are checked as their box seen from above, and a moving ship is checked every 20 units of its way, so it can't skip through at x10. The Garuda looks 600 units ahead and steers round rocks and ships. New values: Crashes on the Board tab, Avoid Look Units under Enemy, Crash Full Speed and Crash Nudge Speed in the stats file, Show Ship Boxes under Plot & our ship draws the boxes on the plot | Built, compiled clean; 21 checks of the sums pass in the editor; crashes and the Garuda's steering seen in play (Step 125's checks) |
| 124 | The grid fades, and our ship is 200 units (owner 2026-11-01, from a designer's brief). A small square is always 50 units and a big square always 10 x 10 of them, 500 units. The lines are counted from the world's centre. Zooming out, the small lines fade from x0.43 to x0.21 and the big ones stay; zooming in brings them back. A big line no longer has a small line under it, and White Major and Amber Major were raised to the same brightness as before. Small Full Px (16) and Small Gone Px (8) set the fade, on the Board tab under Plot & our ship. Zoom Out Limit is x0.1. The edge numbers are one per big square; from x0.26 to x0.16 every second number fades out (Axis All Numbers Px 100, Axis Half Numbers Px 60), leaving one per 1,000 units at x0.1. Our ship (`Asura.asset`) is 200 x 128, four small squares at x1, and all nine of the owner's dots moved out with it. Hits and crashes use that box (Step 123), so the ship is a bigger target. The ship picture's limit went from 120 to 240 px. The Garuda (150) and the frigate (75) are unchanged until the owner sets them | **Done**. Compiled clean. Play mode: 12 zooms in and out, 635 lines checked while panning: every line on its 50-unit mark, none doubled; ship 4.0 squares at x1; no new errors; `Logs/GridFade`. Playtest section 25b updated; full playtest not run; no fight played with the bigger ship |
| 125 | The targeting computer lock (owner 2026-11-01; flow agreed with the owner). **Scan** tells you what a ship is and aims nothing. **Lock**: lower the targeting computer, get the ship in the sights and zoom until it fits the target box, now 120 px at the sight's centre. The sight says SLEW LEFT, SLEW RIGHT, ZOOM IN, ZOOM OUT, TOO FAR, LOCKING with a percentage, and LOCKED, and beeps as the ship fits and as the lock comes on. The bar under the sight (it was the design's convergence bar, which only showed range) fills over Lock Seconds, 10 game seconds; out of the sights it falls back over Lock Fall Seconds, 5. The selected gun turns toward the ship as it fills. **Locked**: the ship is the target, tracked on the plot with a ring and LOCKED on its label, and the firing solution stays up to date with no TRACK and no SLEW TO. The lock stays while DRADIS hears the ship or it is in sight, with the periscope up or down. **Fire**: the four switches and the lever; next shot, ARM and the lever. BEARING now means the lock is still on. Hand dials and TRACK remain for a shot from the plot. CLASS is gone: its button is switched off in `Periscope.prefab`, and a scan gives the class. A lock is as loud as ACTIVE SCAN, and in NO PEEKING a lock loses as a scan does. Missiles fire on a lock too. Save button on the Board and Keys tabs of Starwolf → Tuning | **Done in play**: WEAPONS FREE played through the switches: DRADIS, lock, turn, four switches, hit from the gun's dot. Red lines unchanged. Crash into a rock (bounce, 12 to 3.6 units/s, front armour 100 to 76%), ship against ship (both damaged, right sides), a new railgun on and off a dot, and the Garuda across the rock field at x10 with no crash all seen in play. Full playtest 221 of 222 (`Logs/Playtests/2026-11-01_173816`, covering Steps 120 to 125). The one failure was the test's own drag, too short at the current zoom; the drag is lengthened and that one check has not been run again. A run plays by fixed numbers on the ships' running copies, so the owner's tuning can't fail a check; the files are untouched, and its waits now go by the game clock because filming holds the game behind real time. Crashes are not in the playtest: it switches them off, and they were checked by hand in play |
| 126 | Ship sizes set (owner 2026-11-01): our ship 200 x 50, four small squares long; the Garuda 350 x 100, trial numbers that may change; the frigate stays 75 x 37 for now. Each ship's hard points moved to the new edges, every weapon on the same dot. The ship picture's limit went from 240 to 300 px, as the Garuda is 266 px at x1. The picture is still the old model shape, so it is wider than the box: about 128 units on our ship, about 175 on the Garuda. Show Ship Boxes draws the real box | **Done**. Compiled clean; values read back; playtest check updated to 350; not seen in play mode. Asura's VLS has no hard point (it had one when Step 124 ran; not removed by this work), so our missiles can't fire until the owner says what they want |
| 127 | The frigate is 100 x 25, its hard points moved to the new edges, every weapon on the same dot. Hit Margin Units is 0 on all three ships (owner 2026-11-01: the box is what's important). A shot only counts if it touches the box, so our ship's target is exactly 200 x 50. Shots fired with the targeting computer locked still hit; hand-plotted shots get much harder. **Shots hit within** in Starwolf → Ranges changes it. The owner confirmed the plot picture doesn't need to match the box, and that the missing VLS dot on our ship will be solved later | **Done**. Values read back in play mode. Rammed our ship into the Garuda's side at 60 units a second: it stopped where the boxes touched, and the two ended up apart. Playtest before this change: 221/222 (killshot-e7); the playtest runs on fixed numbers, so it does not test ship sizes, and crashes are off in it |
| 128 | Hit area 25% bigger than the box (owner 2026-11-01: this replaces Step 127's zero). The box is unchanged. A shot counts if it touches the box made 25% longer and 25% wider: 250 x 62.5 for our ship. Hit Margin Percent (`WeaponSystem.Stats.hitMarginPercent`, was Hit Margin Units) is per firing ship. **Hit area, % bigger than the box** in Starwolf → Ranges changes it, and the file saves at once | **Done**. Compiled clean. Read back in edit mode: our ship 250 x 62.5, the Garuda 437.5 x 125, the frigate 125 x 31.25. No shot fired in the running game. The ship files keep an old unused Hit Margin Units line until they next save |
| 129 | Gas Clouds (owner 2026-11-01). A cloud is a box in a level with no collider: a ship flies in and takes no damage. `GasCloud.cs` has its width and height. With at least 90% of our ship's box in a cloud, our signal heads for 30% of what it would be, and an enemy sees us only from 400 units or closer. The owner agreed 2,000; 400 keeps the same share of the new 2,000 visual range. Firing from inside still adds the full 25 signal. The three numbers are in each ship's stats file, under Sensors. The signal box says IN GAS CLOUD. On the plot a cloud is a faint soft box behind the ruins, shown by the same rule as debris. Its picture is `Assets/UI/Board/Art/Plot/GasCloud.png`, on the Plot panel's Art list. Cloud Alpha on the Board tab, under Plot & our ship, sets how strong it shows. `Level_01` has three clouds, placed for the 20,000 map (2,400 x 1,400, 2,000 x 1,200 and 1,600 x 1,000). The Level window has a Gas cloud button, and shows clouds on its map as blue boxes; a click picks one. Starwolf → Keys, Hard Points and Ranges are gone from the menu: they are tabs of Starwolf → Tuning. Enemies don't hide in clouds yet, and our DRADIS still hears a ship in a cloud | **Done**. Compiled clean. Play mode: 3 clouds found; fully in, 100% and signal target 35 to 10.5; 10% over the edge, 90% and inside; half over the edge, 50% and outside; the Garuda at 2,995 units stayed COLD, at 378 units it went ALERT; IN GAS CLOUD showed; the cloud showed on the plot. `Logs/GasClouds`. Playtest section 25c added; full playtest not run. The Level window opened with no errors; its map not checked by eye |
| 130 | The designer's spacing table (owner 2026-11-01). **One clock:** at x1 a game second is a real second, up from 2.38 game seconds a real second. `BoardManager.TickGameSeconds` is the game seconds in one tick, and Game Seconds At X1 (1) is on the Board tab under Board & time. **Speed:** ENG 1 allows 25 units/s and ENG 5 allows 50, in equal steps; ENG 0 is still no thrust. Base Speed Units is new in the stats file, under Navigation. Top speed crosses the map in about 7 minutes and takes 27 s to reach. **DRADIS** 5,000 at gain 1 to 7,500 at gain 5. **Visual range** 2,000. **Railgun** 2,000, **PDC** 600, **missile** 3,000. **Slug** 300 units/s, about 7 s to full reach. **Missile** 400 units/s, turning 120° a second. Slug and missile speeds are their own numbers (owner). **Enemies:** the Garuda and the frigate have our ranges; the Garuda goes 17.5 to 35, the frigate 30 to 60, the same shares of our speed as before. Garuda cruise 22. **Fighters** 100, **boats** 75, boat range 4,000. Crash Full Speed 50. **Map:** every level is 20,000 x 20,000. Level_01's 136 pieces moved to a third of their distance from the centre, same size; our start 0,-8000, the Garuda's 0,8000. Exit, reinforcement and enemy search distances were cut to fit. Scope Fit Units 1000, so MAG 2 fits a ship at railgun reach and MAG 8 at 8,000. **Plot:** Zoom Out Limit x0.058, where the whole map fits the plot's height; every 5,000 units a big line is brighter (Sector Every on `PlotGrid`). **Training levels:** Enemy Spawn has a new Cruise Units value. WEAPONS FREE and FINDERS KEEPERS have no engines, so their Garuda starts 1,500 away and stays still; WEAPONS FREE's shows its reactor side. NO PEEKING starts 4,000 away with the Garuda at 8 units/s. **PDCs** now check the closest point of a missile's flight each tick: at x5 a missile crossed their 600-unit reach inside one tick. The close-range weapon in the table is left out until the owner designs it. A newer designer brief for stepped grid squares is waiting for the owner's yes. Every other time in the game now means real seconds at x1, so these are slower than they were: lock 10 s (was 4), gun 3° a second (was 7), ship turn 20° a second (was 48), power one unit a second (was 2.4), enemy reload 12 to 17 s. Replaces decisions 13, 26, 28, 29, 40, 42, 44, 46, 47 and 50 on those numbers, and Step 63's scale. docs/scale-sim, the spec, `CONTEXT_INDEX.md`, `FTUE_PLAN.md` and the READMEs have the new numbers | **Done**. Compiled clean. Measured in play at x1: 5 s on the clock in 5 real seconds; 50.4 units/s at top speed. Scripted play through the real lock and switches, no person: WEAPONS FREE to MISSION COMPLETE in 48 s, 7 shots, no engines; NO PEEKING heard the Garuda at gain 1; FINDERS KEEPERS started with it in reach; Wolf Hunt first contact at 3:14, frigates exactly 3 minutes after the alarm, PDCs shot down 4 missiles, the jump point opened and flying to it won (the Garuda's reactor was set to 0 by hand for this). In the two Wolf Hunt fights played to the end, the scripted player failed to kill the Garuda: it ran out of slugs once and lost its magazine once, with no repairs and no steering round the reactor side. Full playtest 225 of 225 (`Logs/Playtests/2026-11-01_183621`), which runs on the old numbers and clock, on purpose, so it proves the code and not the balance. No person has played it. x5 and x10 not tuned (owner: x1 first) |
| 131 | Crash shake and ships that break in two (owner 2026-11-01). **Shake:** a crash or a hit on us jolts the plot by random amounts that die away, as the old UI's `ShipShake` did: Shake Seconds 0.5, Shake Px 7, on the Board tab; a slow crash shakes less. **Break:** a killed ship in sight is cut in two across the ship, where the shot struck (kept between 30% and 70% along it). The halves drift apart and turn slowly: Wreck Drift Units 5, Wreck Spin Deg 3, under Plot & our ship. This cuts the ship's picture on the plot, with no shader and no second model, so it is a clean straight cut. **Fade:** out of sight a wreck is a dim dot, which now fades away over Wreck Dot Fade Seconds, 120 game seconds; back in sight it shows again | **Done**. Compiled clean. Play mode: the Garuda killed from code at 570 units, the halves apart after 8 game s (`Logs/ShipBreak`); shake on, with nothing showing past the plot's edge behind the frame; the dot read 0.45, 0.23, then gone at 120 game s. Not checked: a real slug's cut position, or how it feels at speed. Full playtest not run |
| 132 | The plot as a command table (owner 2026-11-01). **Corners:** a locked ship has the targeting computer's white corners on the plot, round its length, in place of the ring; only once the lock is full, as a part-filled lock must not give its position away. **Old markers:** a contact out of sight is a marker where it was last fixed. It fades to Stale Alpha (0.35) over Stale Seconds (60 game seconds), and from Stale Label Seconds (10) its label says how old it is: `RNG 3,855 km · 3 MIN AGO`. **Ruler:** line 3 px, end dots 10 px, distance label 17 pt, white, and clear of the line. **Collision warning:** `ROCK AHEAD · 19 S` on the alarm strip with the alert tone, when our ship is on course to hit a rock within Rock Warn Seconds (30 game seconds), by the box's true path. Rocks only: warning of a ship would give away ships we can't see. Also the owner-approved label words NEW CONTACT and NEW CONTACTS, and a fix: POINT times showed fractions of a second after the clock change | **Done**. Played on the final numbers in WEAPONS FREE: lock, corners, a hit from the gun's dot, the warning at 19 s, a crash into a rock (front armour 100 to 69%), the marker at 3 minutes. Full playtest 225 of 225 (`Logs/Playtests/2026-11-01_185428`) |
| 133 | Military labels and x5 as the top rate (owner 2026-11-01). The signal box reads UNDETECTED, DETECTED BY 1 HOSTILE, DETECTED BY 2 HOSTILES, EMISSIONS DECAYING or EMISSIONS LOW. The alarm strip reads NEW CONTACT or NEW CONTACTS. Fire control reads CAPACITOR CHARGED or CAPACITOR CHARGING +n/S. The ARM switch's hint reads ARM READY, and a switch that can't be thrown reads INTERLOCK. Every other label is unchanged. The x10 key and its label are switched off in `TimeControl.prefab`, which leaves a gap to the right of x5. x10 stays in the rates list and the playtest sets it directly, to keep the run quick | **Done**. Compiled clean. Full playtest 225 of 225 with the labels in (`Logs/Playtests/2026-11-01_185428`), run before the x10 key went. Play mode: three rate keys, x5 sets the rate to 5, the playtest's direct x10 works, and every new label measured no wider than the words it replaced, except INTERLOCK and ARM READY, which are narrower than ROUND IN RAIL in the same size of box. Full playtest not run since the x10 key went |
| 134 | The briefing icon (owner 2026-11-01). A small amber page, `Briefing Icon` in `02.1_Grid`, next to the time control. It appears with the briefing box and stays on the board. A click opens the box, and another closes it, with the switch sound. A level with no briefing has no icon. The picture is `Assets/UI/Board/Art/Briefing/icon_briefing.png`, generated, in the briefing box's colours. **Calls made without the owner:** its place on the top bar, where nothing covers it, and its 44 px size | **Done**. Compiled clean. Play mode in Wolf Hunt: it appears when the grid has formed, two clicks close and reopen the box, two switch sounds, and it is the top thing under the mouse. `Logs/LabelCheck/briefing_icon_2.png`. Checked by calling the click, not with a real mouse. Full playtest not run; the other three levels not opened |
| 135 | The grid's squares change size as you zoom (owner 2026-11-01, from a designer's brief; this replaces Step 124's fixed squares). The square on screen is 50 units at x1, then 100 from x0.78, 250 from x0.39, 500 from x0.16 and 1,000 from x0.08. Each size fades as its squares shrink from 35 to 25 px, and the next takes over; the lines are still counted from the world's centre, so a line in both sizes stays solid. 100 to 250 doesn't line up, so those new lines fade in between the old ones. 5,000 is in the list, but by this rule it would only become the square past x0.05, so the 5,000 lines are the brightest lines at every zoom; 500 lines are a little brighter down to about x0.2. The edge numbers use the same sizes with more room: every 100 at x1, every 1,000 at x0.1, every 5,000 at x0.058. Step Units, Big Units, Sector Units, Step Full Px, Step Gone Px, Big Full Px and Big Gone Px are on the Board tab under Plot & our ship; Axis All Numbers Px and Axis Half Numbers Px are now 60 and 40; White Sector and Amber Sector are new colours on `PlotGrid` | **Done in play**. Compiled clean. Swept x1 to x0.058: squares as listed, no solid lines closer than 35 px, at most 81 lines across, the 1,000 and 5,000 lines stayed at full strength. Looked at x0.55, x0.29 and x0.058 (`Logs/Spacing`). The plot formed with no new errors. Playtest section 25b updated; full playtest not run |
| 136 | The Level window edits on the map (owner 2026-11-01). Click anything to pick it: a spawn, a gas cloud, a debris field, or a rock. Wolf Hunt has no debris fields; its rocks are in plain groups (Asteroid field 1, Wrecks, Debris · far), so a click on a rock picks its group, and Option-click picks that one rock. Drag to move. Drag a white square to resize: a cloud, a spawn box or a debris field changes size, a group's rocks spread out or draw in and keep their own sizes, and one rock grows or shrinks. A resized debris field scatters again on letting go, which replaces hand-moved pieces in that field. The Position numbers and the settings of whatever is picked show under the map, with a Delete button, and everything can be undone. The list above them shows everything in the level. A Save button stays at the top of the window and reads Save Level_01 or Level_01 saved. The map has grid lines with the plot's numbers, every 4,000 units on a 20,000 map. The window no longer pushes spawns 3,000 units in from the edge: the margin is now the board's warning band (1,000) plus 500, so Wolf Hunt's spawns stay at 8,000. The map does not zoom yet (the owner did not ask for it) | **Done**. Compiled clean. Checked in the real window with real mouse events on `Level_01`: rock picks its group; group moved 1,000 units; group spread with rock sizes unchanged; Option-click moved one rock and no other; cloud moved and resized; settings and Delete shown; Delete and five undos put everything back; Save cleared the unsaved state; spawns stay at 8,000. Resizing a debris field and its re-scatter not tried: Wolf Hunt has none. Full playtest not run. `Logs/GasClouds` |
| 137 | Real radio messages, first version, in one new level (owner 2026-11-01: "start building it"; plan of 2026-10-29). New level **INTERCEPT**, row 4 on the MISSIONS list; WOLF HUNT is row 5. The Garuda starts at a random point beyond DRADIS and cruises at 20. It sends a message 15 game seconds in, then about once a minute. Each message is written from where it really is: "GARUDA NORTH OF YOU HEADING SOUTHWEST SPEED TWENTY". A soft tone plays and the alarm strip says INCOMING SIGNAL with the seconds left to lock it. Each message has a new band, a new frequency and a new code. Tune, LOCK, then turn the four dials until it starts GARUDA. Not locked in 60 game seconds, it is lost. The wait for the next starts when the last is read or lost. Read one, then hear the Garuda on DRADIS: level won. Three new values on **Enemy Spawn**: Signal Seconds (0 is no messages), First Signal Seconds and On Air Seconds. Signal Sound is on the Sound tab of Starwolf → Tuning. Every other level, Wolf Hunt too, keeps the old fixed signal | **Done in play**, with the board's own clicks: 1st message matched the Garuda's real direction, heading and speed; a lock 2 off was refused and one on the signal held; two dials read "GA", four read it all; the 2nd message had a new band, frequency and code and said NORTHWEST as the Garuda had moved; ignored, it was lost; 2,996 units away with DRADIS on, MISSION COMPLETE, GARUDA FOUND BY SIGNAL; the row opens the level. `Logs/CommsSignals`. The tuning knob's drag and flying there were not played by hand. Playtest section 25d added; see Step 140 for the run |
| 138 | Smooth movement on the plot (owner 2026-11-01). Shots and missiles, ours and the enemy's, no longer jump every 0.42 s. Each frame the dot slides toward where the rules will put it at the next tick (`BoardManager.Slide`, `WeaponSystem.StepAhead`, `PlotPanel.ShotAt`), and it reaches a ship as the hit lands. Just after firing it can go up to twice its true speed while it catches up, and it can slow in the last tick before a hit. Fighters slide up to one tick behind their flight. **A scanned ship out of sight**, while the ACTIVE SCAN cone covers it, moves smoothly and shows where it is now. With ACTIVE SCAN off it stops there and hops each time the sweep touches it (owner's choice). A ship not scanned still hops or shows a bearing line, a ship in sight or locked is smooth as before, and a ship no sensor reaches stops and fades as in Step 132. Only the drawing changed; hits, damage and every rule are the same. The hit tests moved out of `TickRounds` and `TickIncoming` into `FirstStruck` and `ReachesUs`, and the plot uses them too | **Done in play**. Compiled clean. Measured frame by frame in Wolf Hunt (`Logs/SmoothMotion`), with the shots added from code: slug at x1 steady at 300 units/s, at x5 touching the Garuda's hit box as the hit landed; enemy missile 406 and fighters 102 units/s (true 400 and 100); no still frames in any of them; an enemy slug stopped on the edge of our hit box, SLUG HIT; a scanned ship moved on all 291 frames with ACTIVE SCAN on and four times in 11 s with it off. Screenshot shows the slug, its trail, the fighters and the red enemy slug. Not fired through the real switches here. One new playtest check, in the slug section, not yet run; full playtest not run. Enemy missiles hitting us are drawn the same way but their hit report was not read |
| 139 | A big square is 5 x 5 again (owner 2026-11-01: "revert to the 5x5 grid instead of that 10x10"), and the grid is quieter. The squares still fade from size to size as in Step 135, now 50, 100, 200, 500 and 1,000 units: 200 replaces the designer's 250, so the big lines are round numbers. Big lines are every fifth line at every zoom, 250 units at x1, then 500, 1,000, 2,500 and 5,000, and they fade across with the squares. There are two strengths only, so the 5,000 lines are no longer a third, brighter level; Sector Units, Big Units, Big Full Px, Big Gone Px and the two sector colours are gone. Big Every (5) is on the Board tab. First-try strengths, killshot-dd's suggestion, for the owner to judge: White Minor 8% (was 13%), White Major 24% (was 36%), the amber pair in the same proportion | **Done in play**. Compiled clean. Swept x1 to x0.058: squares 50, 100, 200, 500, 1,000 from x1, x0.78, x0.39, x0.19 and x0.08; big squares 190 to 367 px at each change; the 5,000 line stayed at full strength. Before and after at x1 and x0.1 are in `Logs/Spacing`. No new errors. Full playtest not run since Step 135 |
| 140 | Real radio messages in Wolf Hunt (owner 2026-11-01: "60s"). On `Level_01`'s Enemy Spawn: Signal Seconds 60, First Signal Seconds 15, On Air Seconds 60. The reinforcement frigates send nothing. INCOMING SIGNAL is now the last line on the alarm strip, under the mission's lines and NEW CONTACT, so it never hides a danger or the jump point. A message not locked in time now goes off the air even if the Garuda is dead. The playtest turns messages off for its run, as a tone would upset its sound checks; section 25d checks them on a spare radio | **Done in play**, Wolf Hunt from the board: first message "GARUDA NORTH OF YOU HEADING SOUTH SPEED TWENTY", tone and strip with the seconds left; a mission line stayed on top of it; with the Garuda dead it was still lost on time. Direction and speed matched; the Garuda had turned by the time its heading was read. Full playtest 235 of 235 (`Logs/Playtests/2026-11-01_194534`), section 25d included, but on the old "NORTH OF YOU" wording and with messages off in Wolf Hunt, as a run has them off. The new wording was seen decoded in play in INTERCEPT (`Logs/CommsSignals`, picture 6); 25d's new text has not run. A fight with messages coming in has not been played |
| 141 | Scan, plot and lock (owner 2026-11-01). **A lock is short:** it lasts 10 game seconds (Lock Hold Seconds, in each ship's stats file, under Weapons). The sight's bar runs down, the sight counts the seconds, and fire control's source line counts them too. Then a tone sounds and the lock drops, with its firing solution and the switches, and the bar starts again from empty. **A lock gives no course:** it gives exact range and bearing. A scan gives course and speed at that moment, and each sweep pass over a ship in the cone is a new scan. A plotted track gives them too. The lock takes the newer. Fire control reads `LOCK 7 S · SCAN COURSE`, `· PLOT COURSE` or `· NO COURSE`. With no course the shot goes where the ship is now. **No scan, no boarding:** the hangar refuses an unscanned target, TARGET NOT SCANNED, for the boats only. WEAPONS FREE's briefing now has you bring the Garuda inside a gun's cone before you lock (owner: the ship doesn't need to be side-on, only to have the Garuda inside the arc; the first wording said to turn your side, and that was wrong); FINDERS KEEPERS and WOLF HUNT say the new rules; "throw" is gone from the briefings (owner). `QuestSystem` is unchanged: a lock still loses NO PEEKING. **Calls made without the owner:** the wording of the source line, the refused tone for a lost lock, the newer of scan and plot winning, fighters needing no scan | Compiled clean. Full playtest 235 of 235 (`Logs/Playtests/2026-11-01_194534`), six new checks: no course with neither, the scan's course used even when it's out of date, a newer plot taking over, a lock running out, its tone, the boats refused. The run uses a long stand-in lock outside those checks. Played on the real numbers, driving the game's own controls from code (no mouse): in WEAPONS FREE a lock came on, ran 10 s, dropped and refilled; side-on first, the switches and a shot fit inside the 10 s and it hit; locked first and turned after, the lock had 1.6 s left with the gun still turning. In NO PEEKING a lock ended the level, MISSION FAILED. **Open, for the owner: a missing course doesn't yet change a shot at the Garuda.** WEAPONS FREE's and FINDERS KEEPERS' Garuda doesn't move (Step 130, Cruise Units 0), and that stays: each training level tests one thing, and not every level is a full fight (owner). A moving Garuda travels along its own length when it crosses, and its hit area is 437 long: with cruise 22 set by hand, a NO COURSE shot from 1,945 units hit. By the sums, a shot with no course misses the Garuda only past 2,980 units at cruise 22 (the gun reaches 2,000) and past 1,875 at top speed 35. A frigate is 125 long, so it misses past 625 at 30 and past 310 at 60. The slug stays at 300 (owner: about 7 seconds to full reach, and the railgun is the fastest thing on the board), so the fix has to be something else. Glancing hits were put to the owner and turned down, and nothing was built: a hit strikes a side, breaks through its armour, then damages the systems behind it, as built |
| 142 | The message wording and a Garuda that keeps changing direction (owner 2026-11-01). It is an intercepted message, so it doesn't speak to us: "GARUDA HEADING NORTHEAST SPEED TWENTY BEARING NORTHWEST". The bearing is the Garuda's from our ship, one of the eight words. In a level with messages, the Garuda takes a new course halfway to its next message, so the heading is true when it is sent and the next message differs. That runs in INTERCEPT, and in Wolf Hunt only while the Garuda is COLD. In INTERCEPT we start in the middle of the map, and the Garuda starts anywhere in a box round us, at least 8,000 away (Away From Us Units on Enemy Spawn, new). The owner rejected that start, and Step 145 took it out | **Done in play**: six messages in a row in INTERCEPT said SOUTHEAST, EAST, NORTHEAST, NORTH, NORTH, WEST, each matching its real heading; the bearing went from SOUTHWEST to WEST as it moved, with our ship standing still. The test used the real 60-second gap with On Air Seconds cut to 5. 5,000 starts landed on all eight sides, never closer than 8,000; the corners come up about twice as often as north, south, east and west. The new wording seen decoded on the drawer (`Logs/CommsSignals`, picture 6). The new text in the briefing not seen on screen. Wolf Hunt's course change not seen in play. Full playtest not run since |
| 143 | Hygiene pass (owner 2026-11-01: everything built must be done and checked before a new level). **Missiles fire again:** choosing MISSILE now picks the first launcher on a dot, VLS-A. It fires to the left side only, and no control picks VLS-B yet. **NO PEEKING's** bubble says "Set the four switches". **Playtest:** check #225 puts power on before it reads, so it reads 30 to 9 in place of 0 to 0. Only the newest two runs keep video and shots, and every report stays (tried on spare folders; the next run clears the old ones). **`Level_02`** came off the build list; the file stays until the owner says. **Not bugs:** WEAPONS FREE's switches stayed off in one script because it never clicked the alert from C3 to C2. A fighter's hit doesn't lead the Garuda to us: after a hit its last fix was 0 units from the fighters and 5,227 from our ship | **Done**. Compiled clean. Played on the final numbers by a script using the board's controls; no key pressed. **Slug** at x1 and x5: no still frame, about 300 units a second, the dot within 2 units of the hit area at the hit. **VLS-A:** two missiles left its dot and hit. **Fighters:** launched through the switches and lever, hit, came home on RECALL, four ready, none left on the plot. **Enemy missile:** the strip read `MISSILE HIT · STBD · HE · HGR −3 · FIRE`. **Crash shake:** 1.9 px at 10 units a second, 3.0 at 25, 5.4 at 50. **A slug kill's cut:** 0.637 along the ship, as worked out; the end screen covered the picture, so the number is checked and the picture is not. **Save** on the Board and Keys tabs wrote to the file, pressed with sent mouse events. **Briefing icon** works in all five levels. **Debris field** in FirstShot, sent mouse events: radius 1,250 to 3,170, all 40 pieces scattered again inside it, one undo put everything back, not saved. **NO PEEKING** won with two POINTs, a RULER, TRACK and the switches. **FINDERS KEEPERS** won by boarding: four slugs opened the right side, fighters on DISABLE, boats on SECURE, `GARUDA SECURED`. **INTERCEPT** won start to end: tuned, locked, decoded by reading the text, flown along the bearing, heard at 4,912. The longest message fits in four rows. The briefing fits at the smallest text size. **Wolf Hunt:** the Garuda killed from a fresh start with no help, 8 missiles then 8 slugs. The first salvo wrecked its magazine and it never fired again, so that fight was cheap. **Messages:** a tone each time; INCOMING SIGNAL shows with its seconds in a quiet moment; in a fight the danger lines stay on top. A new course 32 s after a message while COLD, none after. **Garuda across rocks at x1:** 9,175 units, steering round them, 0 armour lost. The crash between ships and the rocks at x5 were checked the same day. **Found, for the owner:** in NO PEEKING, from about 1,220 to 1,650 units, SIGHT and SLEW TO with MAG untouched starts a lock by itself; at 1,500 the level was lost after 10.5 game seconds. Once reinforcements are on the strip, INCOMING SIGNAL doesn't show: only the tone says a message came. Full playtest not run |
| 144 | **FLIGHT DRILL**, a new level that teaches flying only (owner 2026-11-01). Scene `FlightDrill`. It is row 1 on the MISSIONS list and the first pick. The rows are renumbered 1 to 6 and are 118 px apart (was 136). There is nothing to fight: the Garuda is parked still and silent at the far corner. A briefing note changes at each of 13 steps, with a soft tone. The end screen reads MISSION COMPLETE, FLIGHT DRILL PASSED. Section 2 lists the steps. **Side thrusters, in every level:** press and keep down `Q` and the ship slides to port, `E` to starboard, with the nose still. Let go, and the thrusters stop the slide. The keys are on the Keys tab of Starwolf → Tuning. Side Speed Units and Side Accel Units are in each ship's stats file: 20 and 4 on our ship at full ENG. Enemy ships have the numbers, and nothing slides them yet. **ENG now raises acceleration, braking, turn rate and side thrust**, on top of top speed. Base Accel Units and Base Turn Rate in the stats file give the ENG 1 numbers. Our ship, ENG 1 to ENG 5: acceleration 1 to 2, braking 1.875 to 3.75, turn 10 to 20° a second, side speed 10 to 20. ENG 0 still turns at the ENG 1 rate and has no thrust. Enemies run at full ENG, and their numbers don't change. **New mission goal Flight:** the steps are a list on the Mission box, in its inspector. A step's ring is the jump point's ring on the plot, labelled CHECKPOINT, with its bearing and distance on the alarm strip. Rings are not drawn on the Level window's map. Their numbers are typed in the Mission box. **Alarm strip:** its text is now left-aligned in `02.1_Grid`. Long lines ran over the lamp, JUMP POINT's too. **Briefing box:** 230 px tall in a flight drill (Drill Note Height, Board tab, Mission). The other levels keep 370. **Calls made without the owner** are in section 8, "FLIGHT DRILL: calls for the owner to check" | **Done in play**, by script. Played from the list row, through the board's real power, DRADIS and rate controls, with steering, throttle, zoom and the slide set from code: two full runs at x5. Second run: 13 steps, MISSION COMPLETE, 4 min 50 s of game time, no crashes, 0 armour lost (`Logs/FlightDrill`). No person has played it. `Q` and `E` have not been pressed on a real keyboard: a script can't press keys. The playtest has two new checks: full ENG gives twice ENG 1's acceleration and turn rate, and `Q` slides the ship with its nose still and the slide stops. Full playtest 237 of 237 (`Logs/Playtests/2026-11-01_212814`), with both new checks, the six-level list, the new radio wording and Step 143's check on the gas cloud's signal. It ran before four small changes, each checked in play: the first note gained "Press P to start the clock" (the board opens paused; pressed P, ENG 2 on STANDBY, step 2 opened); Accel Units 0 no longer breaks the ship; the zoom-in step passes at exactly x0.80; a slide cut off by a power loss no longer starts again when power returns. The code review found no serious bugs |
| 145 | INTERCEPT's start put back (owner 2026-11-01: we will not start in the middle of the map, and the Garuda will not start on any side; the course changes stay). It now starts like Wolf Hunt: us at (0, −8,000) facing north, the Garuda's box 8,000 × 2,400 at (0, 8,000). Away From Us Units and its retry are gone from `EnemySpawn`. The briefing no longer says "on any side of you". Wolf Hunt's start was not changed. With this start the first message always says BEARING NORTH; later ones change as the Garuda moves | **Done in play**. Compiled, with no errors from `EnemySpawn`. We started at (0, −8,000), the Garuda at (3,959, 7,803). The first message, 15 s in: GARUDA HEADING SOUTHWEST SPEED FIFTEEN BEARING NORTH. 46 s in, its real heading and bearing still matched, and its speed had reached 20. The whole briefing shows inside its box at the smallest text size, though the text is 4 px taller than its area (`Logs/CommsSignals`, picture 7). Decoding, the win and later messages not tried again on this start. Full playtest not run since |
| 146 | **COLD RUN**, the sneak level (owner 2026-11-01). It is row 2 on the MISSIONS list, after FLIGHT DRILL. The scene is `Assets/Scenes/Levels/ColdRun.unity`. Weapons are off. We start bottom left facing east. The jump point is top right and shows from the start. Win by coming within 700 units of it. Lose when our ship is destroyed. The map is 20,000 x 20,000 and shaped like an S: three lanes split by two winding rock walls. The Garuda patrols the corridor at 22 units/s and hunts us if it finds us. The level has 10 gas clouds, 13 route points on the enemy spawn, 6 wall Debris Fields (3 South Wall, 3 North Wall), 2 wall end fields, 4 Big Rocks fields and about 480 rocks. The jump point is an instance of `Assets/Prefabs/JumpPoint.prefab`, and the Mission's Jump Point box points at it (`Mission.jumpPoint`, which replaces the typed `jumpPointAt`). The prefab is sized for the 700-unit ring. The MISSIONS list has seven rows now, 100 tall and 106 apart, in `01_Briefing.unity`. The Level window shows J for the jump point and you can drag it. It draws the enemy's route as a red line with dots. Click the enemy and the route points become white squares to drag. The jump point is also in the "In this level" list. Add or remove route points in the enemy spawn's settings. The patrol route, weapons off and rocks blocking sensors have their own entries | Checked in play, picked from the real list. The Garuda flew to the third lane, found us and destroyed us. The win shows GARUDA EVADED · JUMPED OUT. All 4,000 test lines across the walls were blocked, under the full block that Step 148 replaced. Not played by hand with the real controls. Full playtest not run |
| 147 | Squadrons leave from the side and curve (owner 2026-11-02). **Leaving:** a squadron, fighters or assault boats, starts at the hull, on the side of our ship facing fire control's target. With no target, or a target with no position, it starts on the starboard side. It flies straight out, tilted 15° toward the stern, for 200 units, or further if the small circle round our ship is bigger at the current zoom. **Turning:** then it turns onto where it is going. The turn builds up, reaches at most 90° a game second, and eases off as it lines up. All its flying uses these turns, on the way back too. **The plot:** the V opens as the squadron leaves our ship and closes as it lands. The plot draws a smooth curve between ticks. The rules still step on the tick. **New numbers** in each ship's stats file, under Hangar: `launchTiltBackDeg` 15, `launchRunUnits` 200, `turnDegPerS` 90, `turnEase` 180. Boats use the same numbers. **Fault fixed:** a squadron used to stop short of its goal each tick. At high game speed it could chase a moving ship without ever landing. It now lands or attacks as soon as it is within reach. **The launch bearing box** no longer changes where they go. Its 60° refusal, BEARING OUTSIDE LAUNCH CONE, is still in the game. Whether to remove the box is an open question (section 8) | Built. A throwaway path test passed from every direction, at x1, x2, x5 and x10, ship still or moving. In a play check in `02.1_Grid`, the fighters left at heading 105 with the ship facing 000 and curved onto the target. The drawn heading changed by at most 0.54° a frame. A recall at x5 with our ship at 50 units/s landed. Two playtest checks were changed: launch side and tilt, and gradual turn at x1. They compile but have not been run. Full playtest not run |
| 148 | Rocks and sensors, the enemy giving up, INTERCEPT on the sneak map, and the radio in every level (owner 2026-11-01 and 2026-11-02). **Rocks:** one rule in every level, the same for us and the enemy. Rocks make interference for DRADIS and scans only. Comms is not touched. Each rock on the straight line between a sensor and a ship, and each rock close round that ship, takes Rock Interference Units (500) off the reach. Close means within Rock Close Units (500) of the rock's edge. A large cluster therefore makes a ship very hard to pick up. More power means more reach, so it pierces interference. It also makes the ship louder. A rock on the line also blocks eyes fully, for our ship, the enemy and fighters. Interference does nothing else to eyes. This replaces Step 146's rocks, which blocked sensors outright. **False dots:** as the DRADIS sweep passes a rock in reach, it may throw a false dot near that rock. The chance is False Dot Chance (0.15) divided by the gain squared. A false dot lasts False Dot Seconds (8) and fades. False dots show on the plot only, as red dots with no label. Whether the enemy is fooled by them is an open question (section 8). **The enemy:** it gives up after Give Up Seconds (90) of hearing nothing, in every level. It goes calm and back to its patrol. On a patrol route, Turn Back Chance (25%) is the chance at each point that it turns back. These numbers are on the Board tab of Starwolf → Tuning, under Sensors and Enemy. **Levels:** every rule is the same in all levels. A level sets only its map, route, goal and which tools the tutorial takes away. Weapons off (`Mission.weaponsOff`) is only a tutorial restriction. It stops guns, missiles, PDCs and flight ops. **INTERCEPT** is now a copy of COLD RUN's map (Step 146) with the radio on (section 2). You sneak past the Garuda to the jump point and use the radio to work out where it is going. This replaced its first goal of finding the Garuda on DRADIS. `MissionGoal.FindBySignal` still exists but no level uses it now. **Radio:** the old fixed signal is gone from play. Every level uses real signals, and a level whose Garuda sends none shows NO TRAFFIC. The bearing in a message is always a direction word such as NORTHWEST, never a number | Compiled clean before the last two small edits (false dot chance, radio in every level). Those two are not compiled yet. INTERCEPT scripted run: the first message at 15 s read GARUDA HEADING WEST SPEED TWENTY BEARING NORTHEAST and matched the real position. A later one read HEADING EAST after the Garuda turned back at a route point. Tuned, locked and decoded by script, through the comms controls. The strip showed INCOMING SIGNAL, then JUMP POINT · BRG 046° after reading. Reaching the jump point gave MISSION COMPLETE. Measured with interference at 500: across a wall, lane to lane 4,000 apart, 600 lines: heard on 15 at gain 1, 28 at gain 3, 103 at gain 5. 45 to 47 false dots at once at gain 1 with 220 rocks in reach, seen on the plot (`Logs/CommsSignals`, picture 10). That was before the change to gain squared, so gain 5's count is not measured. Full playtest 242 of 245 (`Logs/Playtests/2026-11-02_004234`). The three failures are the new torpedo and fighter checks (#127, #128, #145), which belong to that work. The rock checks and the seven-row list passed. That run used 2,000 per rock, because the board scene had saved the old number. It is 500 in the scene now. Two later full playtests ran at 500, both 245 of 245: `Logs/Playtests/2026-11-02_005126` and `Logs/Playtests/2026-11-02_005945`. Not done: no person has played it. Fighters' eyes against rocks not tried in play |
| 149 | Photon torpedoes (owner 2026-11-02). **The name:** the owner calls these weapons photon torpedoes, so "missile" is now "torpedo" on the board: on screen (TORPEDO on the Fire Control selector, TORPEDO VLS A, INCOMING TORPEDO, TORPEDO DOWN, TORPEDO HIT), in the board's code and in each ship's stats file. "Launcher" still means the thing that fires them. Soham's old scripts in `Assets/Scripts/Weapons` (`Missile.cs`, `MissileLauncher.cs`), `Assets/Prefabs/Missile.prefab`, `Assets/Scripts/ControllerHaptics.cs` and `Assets/Scripts/InternalSubsystems.cs` were left alone on purpose. **How it flies:** it now leaves like the fighters. It leaves the launcher out of the ship's side, tilted 15° toward the stern. A salvo of two fans either side of that line. It still flies out slowing, as the owner asked on 2026-11-01. Then it curves onto the target. The turn builds up, reaches its sharpest and eases off as it lines up, over 0.5 game seconds. Its motor lights once it is within 3° of lined up, and it zips to full speed. It flies in short steps of 0.05 game seconds. The curve is the same at every game speed. Enemy torpedoes follow the same rules. **New numbers** in each ship's stats file, under Weapons: `torpedoTiltBackDeg` 15, `torpedoTurnBuildSeconds` 0.5. All the other numbers were renamed from missile... to torpedo... with the same values. **The small number box on the Hangar panel** can no longer stop a launch. BEARING OUTSIDE LAUNCH CONE no longer exists. The box starts at 62. With the nose facing north, every launch used to be blocked until someone typed a new number. The box and its red number are still on the panel and do nothing else. This closes the question from Step 147 (section 8). **Ship files:** the four fighter numbers from Step 147 are now really in them, under Hangar | Built. A throwaway path test of 10,368 shots hit with 21 misses, all at the frigate at x5 and x10, where the torpedo ran out of range. In a live play check at x1, the salvo left on headings 95 and 115 with the ship facing 000. The lead torpedo slowed to 15 units/s, curved, lit its motor, reached 250 units/s and hit at 11.2 s. The ship files kept every value after the rename (speed 250, coast 2.5). At heading 000 with the box at 62, nothing blocks a launch. On screen the Fire Control panel reads TORPEDO and TORPEDO VLS A. One new playtest check was added: "The salvo leaves the side tilted toward the stern". The full playtest has not been reported yet |
| 150 | Fighters and boats start at 100% (owner 2026-11-02: they will gain damage from PDCs and ships firing, but they do not start with it). Every fighter and assault boat starts the game at 100% and READY, on our ship and on every enemy ship. Before, their bars on the Hangar panel were uneven (96, 88, 41, 0 and so on) and some were marked REARM or DOWN. The panel now reads 6 OF 6 READY and 4 OF 4 READY. **A launch** now sends 6 interceptors and 4 assault boats (it was 4 and 3). Each fighter does its own attack run. One attack run now takes 60 armour off a side (it was 40). The 4 boats share the same 18 marines. **Still to be built:** nothing damages a fighter yet, and nothing in the game changes those bars yet. The code calls this number fuel and the owner calls it health. The bars show one number. **Correction to Step 147:** a squadron's straight flight out is always 200 units, at every zoom. Only an ESCORT can fly further, past the small circle round our ship | Built. The full playtest passed 245 of 245 with no failures (`Logs/Playtests/2026-11-02_005126/report.md`). #144: six interceptors took off. #148: one attack run took the Garuda's prow from 150 to 90. #168: four boats launched with 18 marines |
| 151 | The enemy is fooled by false dots too (owner 2026-11-02: "Yeah, obviously"). This answers the question left open in Step 148. While the enemy is calm or uneasy and hears nothing, about every False Look Seconds (120), it picks a rock within its DRADIS reach that it has a clear line to, and goes to look. The wait varies between half and one and a half times that number. It goes UNEASY and the DETECTED line doesn't count it. It does reset the time since it last heard us, so hearing us for real 20 to 60 s later takes it straight to SEARCHING. If it hears nothing for Give Up Seconds (90), it goes calm and back to its patrol. That gives the player a chance to slip past. **The rate:** the enemy's rate is deliberately not the same as our false dot rate. At our rate it would chase rocks all the time. **Levels:** the rule is in every level where the enemy hunts. Hunts is off in the training levels, so the enemy doesn't listen there. False Look Seconds is on the Board tab of **Starwolf → Tuning**, under Enemy | Compiled with no errors. In INTERCEPT at x5 in play, from a script: at 123 s it went UNEASY and headed for a false contact at (1,756, 5,663). At 214 s, 91 s later, it went calm and back to its route point (1,500, 7,000). Full playtest 245 of 245 (`Logs/Playtests/2026-11-02_005945`). It is the first full run with this rule and this code, and it used 500 per rock in the scene. No playtest check covers the false look itself. No person has played it |
| 152 | The ship in the targeting computer stays flat during a turn (owner's rule since Step 109: always flat, pointing left or right by the way it is going, never tilted, always in the box). **The bug:** the picture was straightened once per game update, every 0.42 seconds. The ship model turned every frame. While a ship turned, the picture tilted and then snapped back flat. On the Garuda the worst tilt was 5.87° at x1 and 29.38° at x5. A ship that was not turning was flat. **The fix:** the picture now turns every frame, in `LateUpdate` in `PeriscopePanel.cs`, from `NavSystem.DrawnHeading`. The model is drawn at the same heading. `Refresh` still picks left or right once per update. **Open, for the owner:** only the Garuda has a camera for this picture (`Contact View Camera · Garuda` in `02.1_Grid`). A Naga frigate shows no ship picture in the sight at all. That comes from reading the files, and was not seen in play. Nothing was changed for it | Compiled with no errors. In play on Wolf Hunt's board: the Garuda was placed 1,500 units away and made to turn through every heading, at x1 and x5. The picture was 0.00° from flat on all 2,124 frames. Before and after shots: the red lines of the sight are unchanged. Full playtest not run: a small change, and the owner's rule is quick checks for small changes. No person has played it |
| 153 | Two fixes: a freeze no longer uses up SECTION 36, and the MISSIONS list ignores a pick in its first half second (owner 2026-11-02). **SECTION 36, the bug:** the owner pressed play on `00_Start` and SECTION 36 never showed. When play starts in the Editor, the picture freezes for about 4 to 6 seconds on the first frame. The intro's clock kept counting through the freeze. SECTION 36 lasts 4 seconds. It was over before the first picture appeared. **The fix:** each frame now adds at most `Time.maximumDeltaTime` (a third of a second) to the clock, in `Update` in `StarwolfIntroController.cs`. INITIALISING... uses the same clock and gets the same fix. **The playtest:** its check "Intro reaches the menu by itself, after 8.0 s" could not catch this bug. It reads the same clock. **The MISSIONS list, the bug:** the level selector went by, and the game started on COLD RUN with no level picked. On a 1920 x 1080 screen, the COLD RUN row runs from 412 to 512 px down the screen. The menu's NEW GAME click area runs from 404 to 500. A click on NEW GAME could also hit COLD RUN and start that level. COLD RUN became row 2 in Step 146. The log shows COLD RUN was picked in the owner's run. Nobody knows if it took one click or two. **The fix:** the list ignores a pick in its first half second, in `Play` in `ScreenFlow.cs`. The half second counts from when `01_Briefing` loads. **Made without the owner:** half a second for the wait, as a plain number in the code, with no setting in the Tuning window | Compiled with no errors. **SECTION 36, in play:** before the fix, about 6 seconds of a freeze went into the clock. After the fix, in a run with a 4-second freeze, the clock read 13.05 when real time read 17.04. The freeze was left out. In a replay of the intro from its start, a shot at clock 3.00 shows SECTION 36 fully typed, a shot was taken at 7.00 in STARWOLF, and the menu came at 8.00. The shot at 3.00 came from the replay. It was not taken in the first seconds after pressing play. **The MISSIONS list, in play:** the row's click was called from code. At 0.00 s the pick was ignored and the list stayed open. At 1.00 s the pick worked and the board opened on COLD RUN. A real mouse click on NEW GAME has not been tested. Full playtest not run: two small changes, and the owner's rule is quick checks for small changes. No person has seen either fix. A person still needs to confirm both by pressing play |
| 154 | The end screen's second button is now MISSION SELECT (owner 2026-11-02: "You go back to mission select"). The owner picked the label. It used to say MAIN MENU and load `00_Start`: SECTION 36, STARWOLF, then the menu. Now it loads `01_Briefing`, and the MISSIONS list opens. RESTART MISSION is unchanged. **Files:** Menu Scene is now `01_Briefing`, in `EndScreenPanel.cs` and in `End Screen.prefab`. In the prefab, the button and its Label are renamed MISSION SELECT. Its click action is still `end.menu`. **The playtest:** its end-screen check is now "MISSION SELECT loads the mission list". It waits for `01_Briefing` | Compiled with no errors. In play on Wolf Hunt's board (`02.1_Grid`): RUN was pressed through the board's PAUSE/RUN key. Our reactor was set to 0 from code. SHIP LOST showed, with the buttons RESTART MISSION and MISSION SELECT. The label fits the button. MISSION SELECT was clicked through its click spot with a sent mouse event. The MISSIONS list opened and stayed open, with no level picked. 14 s later, FLIGHT DRILL was picked from code, and the board opened on FLIGHT DRILL. Pictures: `Logs/EndScreen/endscreen_mission_select-1.png` and `Logs/EndScreen/mission_list_after_end.png`. Not done: the changed playtest check has not been run. Full playtest not run: a small change, and the owner's rule is quick checks for small changes. No person has clicked it with a real mouse |
| 155 | The Garuda can't leave the battle space either (owner 2026-11-02: "the Garuda can also not leave the battle space"). This covers every enemy ship with a mind, the reinforcement frigates too. **Its course:** it never heads for a point in the edge warning band. A hunt point, a route point, where it heard us or a search point moves in to the band's inner edge. **In the band:** it never heads farther out. It runs along the edge instead, even while it chases us or turns a side to us to fire. **Past the edge:** it heads straight for the middle. It does the same in the band if it is heading straight out or into a corner. **The band:** the same band that warns our ship, Arena Warn Units. There is no new setting. `02.1_Grid` sets it to 1,000 units. The default in the code is 2,500. At full speed, at worst, the Garuda drifts about 520 units out while it turns, and a frigate about 750 (worked out from `Garuda.asset` and `Frigate.asset`). **Files:** `KeepInside` and `Inside` in `EnemyBrain.cs`, called at the end of `Steer` | Compiles clean. Played in Wolf Hunt (one-time test, nothing kept): our ship just past the east edge, the Garuda chasing. Heading straight out at full speed, it turned back at the band and came no closer than 684 units to the edge, then turned back and forth just inside the band. Closing in to fight, it turned side-on and ran along the edge, 680 units from it. No errors |
| 156 | Escape opens a pause menu in a level (owner 2026-11-02: "when I press the Escape button during a level, nothing happens ... when you press Escape, it should pause the game and load that menu to do things like mission select, quit game", then "Escape should not quit at once"). The owner picked the four buttons. **The bug:** Escape on the board called quit. Quit does nothing in the Unity editor. The owner saw nothing happen. **What the player sees now:** in a level, Escape stops the game. The end screen opens over the board, titled PAUSED in white. The reason line is empty. The target, armour and time lines are hidden. It has four buttons. RESUME and QUIT GAME are on the top row. RESTART MISSION and MISSION SELECT are below. Escape again, or RESUME, closes it. Time goes back to how it was: running, or still paused if the player had pressed P before. While the menu is open, P and the drawer keys 1 to 6 do nothing. Clicks can't reach the board behind it. RESTART MISSION plays the level again. MISSION SELECT opens the MISSIONS list (`01_Briefing`). QUIT GAME quits the game. In the Unity editor it stops play mode. On the intro and the MISSIONS list, Escape still quits at once. That code is in `ScreenFlow.cs`, unchanged. **Files:** in `BoardManager.cs`, the key is now **Menu Key** (was Quit Key) on the Keys tab of **Starwolf → Tuning**. It is still Escape. A key set before is kept. Escape sends the click action `end.pause`. `Pause()` can now also set time running again. `EndScreenPanel.cs` is the pause menu too. Its new slots are Resume Button, Quit Button and Rows. Its new click actions are `end.pause` (Escape and RESUME) and `end.quit` (QUIT GAME). `End Screen.prefab` has two new buttons, RESUME and QUIT GAME. They are copies of RESTART MISSION, at y −70 on the Plate, x −160 and +160. Their labels were added to the panel's Barlow Condensed SemiBold font slot. **The end-of-mission screen:** on MISSION COMPLETE or SHIP LOST, Escape does nothing. That screen keeps only RESTART MISSION and MISSION SELECT (owner 2026-11-02: "you just go to mission select. It's okay. Escape doesn't have to do anything there") | Compiled with no errors, in Unity and outside it. In play on Wolf Hunt's board (`02.1_Grid`), with clicks sent from code (the same action Escape sends): with time running, Escape showed the menu. The game paused, Rows was hidden and all four buttons showed. Escape again closed the menu, and the game ran. P to pause, then Escape and RESUME: still paused. QUIT GAME stopped play mode. No errors in the console. One screenshot was taken to check the layout. Nobody has pressed the real Escape key: a script can't press keys. Full playtest not run: a small change, and the owner's rule is quick checks for small changes. No person has played it |
| 157 | Reverse thrust, rocks seen only in visual range, and no false dots (owner 2026-11-02). **Reverse:** the throttle now runs from -100 (full reverse) to +100 (full ahead). It used to run from 0 to 100. S steps down through 0 into reverse. W steps up. The step is still Throttle Key Step (10). Full reverse is slower than full ahead. **Reverse Share** (0.42, from the HTML prototype) is new in each ship's stats file, under Navigation. Reverse top speed is that share of the top speed. Speeding up backwards uses that share of the acceleration. Slowing down, in either direction, uses the brake rate, as before. **The slider** (HELM, 01 FLIGHT, THRUST): 0 is in the middle, full ahead at the top and full reverse at the bottom. The lit bar runs from the middle to the handle. A press on the slider gives +100 at the top, 0 in the middle and -100 at the bottom. The header shows a negative number in reverse, for example THR -80%. G load counts reverse the same as ahead: -80 gives 1.8 G. Losing ENG power still drops the throttle to 0, in either direction. The ENG cap limits both directions. The enemy never uses reverse. **Rocks:** the owner: "the Dradis only shows signals: dots or triangles or something... to get into visual range is when it renders." Rocks now show on the plot only within visual range (2,000 units) of our ship or of one of our fighter flights. Step 92 showed them out to the DRADIS circle while DRADIS was on. Plain DRADIS doesn't show a rock. Only a ping from ACTIVE SCAN gets an echo off one. As the DRADIS sweep passes a rock inside the ACTIVE SCAN cone, a clean, thin, grey hollow circle shows at the rock. It uses the plot's solid ring line (owner: "At best, they can be clean, hollow circles"). The first build drew a soft dot that faded at the edge. The owner found it too large and too faded, and it was removed. The circle's radius is the rock's half size times Rock Echo Size, from Rock Echo Min Px (3) to Rock Echo Max Px (10). A big rock gives a bigger circle, never a large one. The circle is bright as the ping passes and drops to dim within about a second. With no new ping it fades out over 30 game seconds. The Step 148 interference rule cuts the ping's reach, as it does for ships. The far side of a thick field stays dark unless you add power. A rock across the edge of visual range is cut at the circle. The part inside shows as the rock. The part outside shows only as its blinking hollow circle. Each fighter flight's circle works the same way, up to 3 flights plus our ship. Gas clouds are unchanged by this step. **False dots removed** (owner: "the rock should be silent until you ping... those false red dots don't make sense"). A rock is silent until ACTIVE SCAN pings it. Our plot no longer shows red false dots near rocks (Step 148). The enemy no longer goes to look at a rock (Step 151). False Dot Chance, False Dot Seconds and False Look Seconds are gone from **Starwolf → Tuning**. Rock interference on reach and on eyes is unchanged. False dots from radiation are a later idea (section 8, For later talks). **Tuning:** on the Board tab of **Starwolf → Tuning**, under Sensors: Rock Echo Seconds (30), Rock Echo Blink Seconds (1), Rock Echo Dim (0.45), Rock Echo Size (1, times the rock's half size), Rock Echo Min Px (3), Rock Echo Max Px (10) and Rock Echo Ink (grey 8A959D, alpha B0). **Files:** `NavSystem.cs` (throttle range, `reverseShare`, `GLoad`, `SetThrottle`, `Steer`, `Move`). `HelmPanel.cs` (the S key, the slider). **Track Base** and **Track Span** were deleted from Helm Panel. The tooltip on the throttle's cap explains the slider. `TacticalWorld.cs` (`SeeRuins` uses visual range for rocks, `SweepRocks` records `RockEchoes`). `SensorSystem.cs` (`ConeCovers` for a bearing and range). `PlotPanel.cs` (the hollow circles and `ClipToSight`). The circles are drawn with the "circle" shape on `Rock Echoes`, a copy of `Contacts (drawn)` made at run time, just under the contacts. `MarksGraphic.cs` (`World`, a board point in world space, for `ClipToSight`). `BoardLinearImage.shader` (cut at up to 4 circles). Every other image using that shader is unaffected. False dots: `TacticalWorld.cs` (`FalseDots`, `FalseContact` and the false dot half of `SweepRocks` removed), `EnemyBrain.cs` (`falseLookSeconds` and the false look removed), `PlotPanel.cs` (the false dot drawing removed) | Checked in play on Wolf Hunt at x5. **Reverse:** at -80 with full ENG the ship backed straight astern, heading 000 and course 180. It settled at 16.8 units/s: 0.8 x 50 x 0.42 = 16.8. Screenshots `Logs/RockEchoes/05_reverse_helm.png` and `Logs/RockEchoes/06_fader_crop.png` (the slider). **Rocks:** zoom x0.30, our ship 2,000 west of a large rock, ACTIVE SCAN east, 90° arc, gain 5. This was with the first soft dots. The edge rock was cut at the visual range circle, with its outline inside and a grey glow outside. Echoes showed past the circle to the east, and none to the west outside the cone. Screenshots `Logs/RockEchoes/04_echoes.png` and `Logs/RockEchoes/07_after_fix.png`. One bug found and fixed: `Rock Echoes` is a copy of the contacts layer, and it took the grid's fade-in with it at 0. The echoes were invisible. **Hollow circles:** checked in play, screenshot `Logs/RockEchoes/10_hollow_circles.png`. **False dots removed:** compiled and checked in play. With plain DRADIS, no false dots and no echoes showed (`Logs/RockEchoes/08_passive_silent.png`). With ACTIVE SCAN on, echoes showed (`Logs/RockEchoes/09_active_echoes.png`). **Playtest:** the helm check (section 15) changed. A press in the middle of the slider now sets the throttle to 0. The check tests that, then sets a forward setting with the same rule the press uses. The full playtest passed 246 of 246 (`Logs/Playtests/2026-11-02_224549`). It ran before the change to hollow circles. No person has played it |
| 158 | Gas clouds look like clouds, and DRADIS shows them as hollow circles (owner 2026-11-02: the clouds looked like "weird light rectangles"; they should look like clouds, and on DRADIS a hollow circle). **Within visual range** (2,000 units of our ship or of one of our fighter flights): the cloud shows as a soft, ragged grey-blue cloud. It fills the cloud's box. The pictures are flipped different ways: left to right, top to bottom, or both. The three Wolf Hunt clouds look different. **Past visual range, DRADIS on:** a cloud inside the DRADIS circle shows as a thin hollow circle at the cloud, in the ruins' grey. Its radius is the average of the cloud's half width and half height. Plain DRADIS is enough. ACTIVE SCAN is not needed. **DRADIS off:** a cloud past visual range does not show. **Inside a cloud:** unchanged. The game still uses the cloud's box: at least 90% of our ship's box in it. The picture fills that box. The edge you see is close to the real edge. **Art:** `Assets/UI/Board/Art/Plot/GasCloud.png` is now a 640 x 384 placeholder cloud, white with soft edges. The plot colours it. Its stretch borders are gone (they were 40 px). The `Gas cloud (copied for each cloud)` image on `Plot.prefab` is now Simple (was Sliced). To change the look, replace the PNG with one of the same name, or swap the sprite on the Gas cloud entry of the Plot panel's Art list. **Tuning:** on the Board tab of **Starwolf → Tuning**. Cloud Alpha, under Plot & our ship, is now 0.45 (was 0.16). At 0.16 the new cloud was almost invisible. New under Sensors: Cloud Ring Alpha (0.5, how strong the circle is) and Cloud Ring Size (1, times the cloud's average half size). The circle is never smaller than 6 px. **Files:** `GasCloud.cs` has a new flag, `inSight`: in visual range now. `seen` still means in the DRADIS circle or in sight. `TacticalWorld.SeeRuins` sets both. `PlotPanel.ShowClouds` shows the picture only when `inSight`. `PlotPanel.Refresh` draws the circle on `Contacts (drawn)`, just before the enemy torpedo dots. **Made without the owner:** the circle shows on plain DRADIS, with no scan. The circle's size is the cloud's average half size. The picture is stand-in art. Cloud Alpha went up to 0.45 | Compiled with no errors. In play on Wolf Hunt, once: our ship was moved by script to (2,000, -3,500), DRADIS on, zoom x0.12 and x0.20. Gas Cloud Middle showed as the cloud picture. Gas Cloud East showed as a hollow circle, in the right place and at the right size. No errors in the console. Screenshots: `Logs/GasClouds/2026-11-02_cloud_and_dradis_ring.png` (Cloud Alpha 0.45) and `Logs/GasClouds/2026-11-02_before_alpha_raise.png` (0.16, too faint). Not checked: COLD RUN's 10 clouds, and a fighter's view. Full playtest not run. No person has played it |
| 159 | Turning with A and D goes the right way (owner 2026-11-02). **The bug:** with A or D held, the ship swung back and forth and turned the wrong way. Each press moves the ordered heading 3°. The order could get more than halfway round ahead of the bow. The ship then took the shorter way round. That was the other direction. **The fix:** the order now stops 179° ahead of the bow. Hold D and the ship turns to starboard until you let go. Hold A and it turns to port. When you let go, the ship keeps turning until it reaches the order, as before. **The heading ring:** the ring on the helm slides to each new heading over 0.12 s. From 359 to 000 it used to slide the long way, a full spin backwards. Now it turns the short way across north. **Files:** `StepOrderedHeading` in `NavSystem.cs`. The heading ring (**Hdg Ring**) in `HelmPanel.cs` | Tested in play mode. A one-time test held D for 20 s from heading 300. The bow turned to starboard the whole time and crossed north once. The heading ring never jumped. Full playtest not run: a small change, and the owner's rule is quick checks for small changes. No person has played it |
| 160 | Fighters are small triangles and appear beside the ship (owner 2026-11-02: "fighters should be small triangles, not long dots or those long weird things, and they shouldn't be spawning in the centre of the ship. They should emerge from the side of the ship, maybe 10 units off the side, and then smoothly curve."). **The look:** our fighters on the plot are small white triangles, 10 px, nose pointing the way they fly. They used to be `Assets/Art/Hulls/hull_wedge_filled.png` drawn 8 x 16 px: a long thin streak. The picture is now `Assets/UI/Sprite/triangle-128.png`. It is on the Plot panel's Art list, on the entry now called Fighter triangle (was Fighter wedge). That entry puts it on the `Fighter (template)` image in `Plot.prefab`. **Fighter Px** on the Plot panel is now 10 (was 16). It is the triangle's size on screen. **Fighter Gap Px** is unchanged (10). **Where they appear:** a squadron now appears just beside the ship you see on the plot. Its leader starts past the widest part of the drawn model (the wings), plus half a fighter, plus **Launch Side Gap Units** (10 units). **The bug:** they used to start at the side of the ship's box, 25 units from the centre. The box is 50 units wide. The model's wings are much wider: the wingspan is about 64% of the length. Zoomed out, the plot also draws our ship at least 30 px long, far bigger than its true size. Either way, the fighters appeared inside the ship you see, near the middle. **The V:** the six triangles start on one spot beside the ship. The V opens as they fly out. No triangle is drawn over the ship. It closes again as they land, as before. **Unchanged:** they fly straight out for 200 units, tilted 15° toward the stern, then turn smoothly toward the target (Launch Tilt Back Deg, Launch Run Units, Turn Deg Per S, Turn Ease). **New number:** Launch Side Gap Units, in each ship's stats file, under Hangar (**Starwolf → Tuning**). **Files:** `HangarSystem.cs` (new `launchSideGapUnits` in Stats; `launchSideUnits`, set by the plot each frame; `LaunchSquad` uses both). `PlotPanel.cs` (how far out the side of the drawn ship is; the V opening; each triangle drawn as wide as it is long). `ShipView.cs` (new `WidthShare`: the model's width as a share of its length, measured once from the model). `Plot.prefab` (the Art list entry, the template's picture, Fighter Px). `PlaytestRunner.cs` (the check label now says "six triangles on the plot"). **Made without the owner:** the 10 px size. The 10 units are counted from the side of the ship as drawn, plus half a fighter. Torpedoes were not changed | Compiled with no errors. In play on Wolf Hunt (`02.1_Grid`), fighters were launched from code, with the target ahead to the north. At zoom x1, rate x1: the leader started 80.5 units from our ship's centre. The wing tip is about 64 units out. At zoom x0.10, rate x5: it started 201.9 units out, beside the drawn ship. In both, six small triangles came out beside the right wing, opened into a V and curved north. Screenshots: `Logs/Fighters/01_before_under_wing_x1.png` (first try, still under the wing), `Logs/Fighters/02_launch_zoom_x1.png`, `Logs/Fighters/03_launch_zoom_x0.10_full.png` and `Logs/Fighters/04_launch_zoom_x0.10_close.png`. Full playtest not run: a small change, and the owner's rule is quick checks for small changes. No person has played it |
| 161 | A heard ship is a red bar on the DRADIS circle, and the bearing trail is gone (owner 2026-11-02: the bearing line was "not a very nice-looking line"; "I'd be very happy if there was a little dot or some kind of small little bar that bordered the circumference of the [DRADIS]"). The owner was offered the plot's edge for when the circle is off the screen. The answer was "Circumference of the Dradis". **What the player sees:** a ship DRADIS hears but we haven't scanned shows as a short red bar, curved along the DRADIS circle, in its direction. Its label, BRG and the angle (for example BRG 053), is just outside the bar. Before, a line ran from our ship toward the contact, with the label partway along it (Step 44). When the DRADIS circle is off the plot, so is the bar. At normal zoom the circle is often off the screen. **Red and curved** (owner: "I guess it should be red"; "The bar should be curved along the edge of the Dradis"). The bar is the board's red (alarm). It is dimmer (0.6) most of the time, and full while the periscope is on that contact. The BRG label keeps its old colours: amber while the periscope is on that contact, dimmer otherwise. The bar bends along the circle, end to end. It covers Passive Bar Deg (6) degrees of the circle. Zoom in and it grows. Zoom out and it shrinks. **It turns in real time** (owner: "That bar will keep rotating... It rotates in real time, so it's not a clicky-clicky thing"). Every frame, it follows the contact's bearing as either ship moves. The BRG number changes with it. The bar still first shows at the sweep's first touch. Before, a heard contact's bearing stayed put until the next sweep (Step 75). That no longer applies to the bar. **The picture** (owner: "this is a UI asset, so figure out where to go"): `Assets/UI/Board/Art/Plot/DradisContactBar.png`, a placeholder white bar with rounded ends, 256 x 32. It is in the Plot panel's **Passive Bar Art** slot, under Passive bearings, in `Plot.prefab`. To change the look, an artist draws a new one in white. The game colours it red. After seeing it, the owner said: "I think it looks pretty good". **The bearing trail is gone (Step 45).** Each sweep used to leave a line from where our ship was. The lines faded over 2 minutes and crossed at the contact. The lines and the code that recorded them are deleted. To find how far away a heard ship is, plot it (POINT, RULER) or scan it. **ERASE** (pressed twice) clears the plot's marks. It used to clear the trail too. **Tuning:** on the Board tab of **Starwolf → Tuning**, under Sensors. New: Passive Bar Deg (6, how many degrees of the circle the bar covers) and Passive Bar Width Px (4, how thick it is). Removed: Passive Label Px and Trail Alpha. The first version had Passive Bar Px (18) and a width of 3. Passive Bar Deg replaced Passive Bar Px. Bearing Trail Seconds is removed from each ship's stats file, Sensors section. **The playtest:** section 18 checked that two trail lines crossed at the Garuda and that a line was gone after two minutes. Now check 82 looks for the bar on `Contact bars (drawn)`, with its picture set. It also checks that no lines are drawn on `Passive bearings (drawn)`. It takes screenshot `23_bearing_bar` (was `23_bearing_trail`). It still runs two minutes at x10 for the checks after it. That is one check fewer. The section 17 check (77) now reads "a bearing bar and label point at it". It compares the label with the live bearing, within 1°. **Files:** `PlotPanel.cs` (the passive bearings block draws a "sector" on the ring at the drawn DRADIS radius; the bearing is worked out each frame from where both ships are drawn; the bars are on their own layer, `Contact bars (drawn)`, a copy of `Passive bearings (drawn)` made while the game runs; the trail drawing is gone; the new settings and the Passive Bar Art slot). `MarksGraphic.cs` (a picture, `art`; the "sector" shape now maps the picture along the slice). `Plot.prefab` (Passive Bar Art). `DradisContactBar.png` (new). `SensorSystem.cs` (`BearingFix`, `BearingFixes`, `LayBearing`, `PruneBearings` and `bearingTrailSeconds` deleted). `BoardManager.cs` (ERASE no longer clears the trail). `PlaytestRunner.cs` (section 18; the `Cross` helper deleted). **Made without the owner:** 6 degrees and 4 px. Both are on Tuning. The 6 px gap between the bar and its label. The gap is a fixed number in `PlotPanel.cs`. The placeholder picture's look. The owner picked red, curved, real time and on the circle only | **First version:** checked in play on Wolf Hunt: our ship about 5,000 units from the Garuda, gain 5, zoom x0.10. A bar showed on the DRADIS circle at BRG 053, with the label next to it, and no lines. Screenshots `Logs/RockEchoes/11_bearing_bar.png` and the close-up `Logs/RockEchoes/12_bar_crop.png`. Its full playtest passed 245 of 245 (`Logs/Playtests/2026-11-02_231040`). Check 77 (BRG 049) and check 82 (one "sector" shape, no lines) passed. **The red, curved bar that turns in real time:** checked in play. Screenshots `Logs/RockEchoes/14_red_bar_east.png` and the close-up `Logs/RockEchoes/15_red_bar_closeup.png`. The full playtest passed 245 of 245 (`Logs/Playtests/2026-11-02_232840`): check 77 (BRG 049) and check 82 (1 bar, picture DradisContactBar, no lines) passed. The owner looked at the screenshots: "I think it looks pretty good". No person has played it |
| 162 | You can draw the line torpedoes and fighters follow out of the ship (owner 2026-11-02: "having a little path that I can draw in the editor as we're tuning the level, setting radii on the ship and everything. Similarly, I can click on a missile node and literally just draw a small path out of it. That smoothens, which then correctly explains the actual way I want my missile asset to move when it's exiting the ship, and similarly for fighters." Then: "I would like to have a hangar's dots, and I'd like to draw these similar paths for them as well."). **The tool:** on the Hard Points tab of **Starwolf → Tuning**. New green dots labelled HANGAR, one on each long side. On our ship (Asura) they are 50 units back from the middle, at (−25, −50) and (25, −50). Dragged, they stay on the long sides and snap to the marks. New button **Add a hangar dot**: it adds a dot in the middle of the long side that has fewer. **Remove this dot** removes either kind of dot. To draw a line, hold Shift and drag out of a torpedo launcher dot or a hangar dot. When you let go, the line is spaced into 32 even points and smoothed. New slider **Line smoothing** (0 to 12, default 4) sets how much. At 0 the line stays as drawn. The line shows in the dot's colour, with a small dot at its end. For a selected launcher or hangar dot, **Line out of it** shows the line's length in units. With no line it says "None: they fly straight out. Shift-drag out of the dot to draw one." **Clear the line** removes the line. The help text at the top of the tab explains all of this. **Where the data is:** each hard point has a new list, `path`: the line's points, in units from the dot (x right, y forward). It is in `WeaponSystem.HardPoint.path`, in the ship's stats file. The hangar dots are in the stats file's Hangar section, `dots` (`HangarSystem.Stats.dots`). This step drew no lines in the ship files. The owner draws them. **Torpedoes:** a torpedo from a launcher with a line flies along the line. It slows as before: from launch speed to coast speed over the coast seconds. At the end of the line it turns onto its target as before. Then its motor lights. With no line it does what it did before: straight out, tilted 15° toward the stern, then the turn. A launcher on the centre line sends every other torpedo along the mirror of the line. In a salvo, each torpedo's copy of the line is turned by the salvo's fan (Torpedo Spread Deg, 20 in each ship's file). Enemy torpedoes use the lines in their own ship's file. None are drawn there. **Fighters and assault boats:** a squadron leaves from the hangar dot on the side facing the target. With no target, it uses the starboard dot. With only one dot, it uses that dot. It appears beside the ship as drawn on the plot, by the Step 160 rule: past the wing tip, plus half a fighter, plus Launch Side Gap Units. It is level with the dot along the ship. Then it follows the shape of the line. At the end of the line it turns onto the target as before. An ESCORT flies on straight past the end of the line until its whole V is clear of the small circle. With no line it does what it did before: straight out 200 units, tilted 15° back, then the turn. It now starts level with the hangar dot, 50 units back from the middle. **In space:** the line is placed in space at the moment of launch. It is turned to the ship's heading at that moment. Then it stays put. If the ship moves on, the line does not move with it. Launches already worked this way. **Made without the owner:** the hangar dots are 50 units back from the middle. The middle of each side has the railguns. Dots there overlapped them and couldn't be clicked. 32 points, and smoothing 4. The mirror for centre-line launchers. Fighters start beside the drawn ship and keep the line's shape. The line stays fixed in space after launch. Shift-drag draws the line. Without a line, fighters now start 50 units back from the middle, at the hangar dot. **Files:** `WeaponSystem.cs` (`HardPoint.path`; new `Laid`, `Along`, `LengthOf` and `LineOf`; `Round.line` and `Round.along`; `NewTorpedo`, `Clear` and `LaunchAt` take the line). `HangarSystem.cs` (`Stats.dots`; `Flight.line` and `Flight.along`; `LaunchSquad` picks the dot and places the line; `Fly` follows it). `EnemyBrain.cs` (passes its launcher's line). `StarwolfTuningWindow.cs` (the Hard Points tab). `Asura.asset` (the two hangar dots). The file also caught up with fields from Steps 157, 160 and 161 when it was saved: `reverseShare` added, `launchSideGapUnits` added, `bearingTrailSeconds` removed. Their values didn't change | Compiled with no errors. A one-time logic test in the editor (nothing went into the game): a shaky S-shaped stroke was smoothed to 32 points. A torpedo flew it, stayed within 1.1 units of the line, and was clear at its end. A fighter flew it, then turned toward the target. In play on Wolf Hunt (`02.1_Grid`): test lines were put on the play copy only, and the ship file was not changed. The fighters' line went out left and swept back. The torpedoes' line went out left, swung forward, then came back. Both were launched from code. The screenshots were stacked into one picture, `Logs/LaunchLines/04_trail_close.png`. Also `03_trail_all_frames.png` and the frames `seq_00` to `seq_13`. The torpedoes arc out of the left launcher along their line. The fighters come out beside the left wing and sweep back along theirs. The Hard Points tab: mouse events were sent from code. A Shift-drag made a 32-point line. Screenshot `Logs/LaunchLines/05_hard_points_tab.png`. It is upside down, from how it was grabbed. The test line was cleared afterwards. Full playtest not run: a small change, and the owner's rule is quick checks for small changes. Nobody has drawn a line with the real mouse. No person has played it |
| 163 | The training levels describe the new DRADIS (owner 2026-11-02: "what about the update on FTOE?"; then "yeah, make the fixes, please"). Steps 157 and 161 changed DRADIS. Rocks past visual range show only when ACTIVE SCAN pings them, as grey hollow circles. A heard ship that is not scanned is a red bar on the DRADIS circle. The bearing lines and their trail are gone. Three training levels still described the old DRADIS. The texts are in each level scene, on the Mission box. **FLIGHT DRILL** (`FlightDrill.unity`). The THROTTLE note now reads: "W raises the throttle. S lowers it. Below 0 is reverse. ENG sets the limit. Fly into the CHECKPOINT ring ahead." The DRADIS note now reads: "Rocks ahead. You see rocks only inside your visual ring, 2,000 KM. DRADIS hears ships. Rocks make no sound. Press 3 and put SEN on DRAW. Press 2 and switch DRADIS on." It used to say DRADIS shows rocks out to its circle. A new step 12, ACTIVE SCAN, comes after it: "A ping shows rocks past your visual ring, as grey circles. It also makes you loud. Switch ACTIVE SCAN on. Drag the cone dial to point ahead." It ends when ACTIVE SCAN is on with DRADIS listening. ROCK FIELD and LAST CHECKPOINT are now steps 13 and 14, unchanged. The drill has 14 steps. Section 2, `FlightDrill`, lists them. **New drill check ACTIVE ON** (`DrillCheck.ActiveOn` in `Mission.cs`, checked in `QuestSystem.Passed`). It is added at the end of the list. The checks already saved in the scenes keep their numbers. **WEAPONS FREE** (`FirstShot.unity`), briefing steps 4 and 5 now read: "SENSORS (key 2): turn on DRADIS. Zoom out until you see the DRADIS circle. A red bar on it points at the Garuda. 5. Turn on ACTIVE SCAN. Drag the cone dial onto the bar and let go." Before: wait for a bearing line, then drag the cone onto the line. **NO PEEKING** (`SilentShot.unity`), briefing steps 3 to 5 now read: "SENSORS (key 2): turn on DRADIS. Zoom out until you see the DRADIS circle. A red bar on it points at the Garuda. 4. Fly toward the bar with W, A and D. When the Garuda comes into sight, zoom back in. 5. Put a POINT on the Garuda. A minute later, put another." Steps 6 to 8 (RULER, TRACK, fire) are unchanged. Before, you moved until the bearing lines crossed at the Garuda. Now you follow the red bar until you can see the Garuda. **Files:** `FlightDrill.unity`, `FirstShot.unity`, `SilentShot.unity` (the Mission box), `Mission.cs`, `QuestSystem.cs`. **Made without the owner:** the exact wording. The owner saw a summary and said "I think this is actually pretty good" | Checked in play: FLIGHT DRILL from the DRADIS step. DRADIS on moved it to ACTIVE SCAN. ACTIVE SCAN on moved it to ROCK FIELD. The two briefings were changed by an exact text swap and saved. Full playtest not run: no playtest check covers these texts. No person has played the levels since |
| 164 | Zoom in close and the ships and rocks show as lit 3D models (owner 2026-11-08: "when you zoom in close enough, you see the actual 3D model of the ship... only when you zoom out it becomes wireframe, and when it leaves the visual range it becomes a signal"; then: "give the ships a shader, a grey metallic shader... a light source somewhere on the map that illuminates from a direction, so you have this sense of light glistening"). **What the player sees:** zoom in on the plot, and the ships and rocks inside visual range turn into real 3D models. Ships are grey metal. Rocks are dark rock. One sun lights them. The bright spots have a soft glow. Zoom out, and they fade back into the line pictures. Past visual range nothing changes: DRADIS shows signals as before. **The fade:** the close-up starts to show at zoom x1, where the plot used to stop. At x1.5 it has fully replaced the line pictures. A level still opens at x1 with the line pictures, red, amber and white as before. Close Up From Zoom (1) and Close Up Full Zoom (1.5) set these. They are on the Board tab of **Starwolf → Tuning**, under Plot & our ship. **Closer zoom:** the plot now zooms in to x4. It used to stop at x1. At x4 our ship is 608 px long. Zoom In Limit (4) is on the Board tab, under Board & time. The SCALE knob now turns from full zoom-out to x4. The scale box shows x1.0 up to x4.0. **The sun:** the sun is the light called Sun in the board scene `02.1_Grid`. It was called Ship Light. It lights ships and rocks. Sun From Deg (300) is the way the sunlight comes from, in degrees clockwise from north. Sun Height Deg (35) is how high it is above the plot. At 90 it shines straight down. Both are on the Board tab, under Plot & our ship. The sun's colour and brightness are on the light itself. It is the board scene's Sun Source (Lighting window). **The look:** the files are in `Assets/Art/Close Up`. `Ship Metal.mat` is the grey metal for every ship. `Rock.mat` is for every rock and wreck piece of the level. `Close Up Look.asset` is the glow. Open it to change the glow's strength. The renderer is `Assets/Settings/Close Up Renderer.asset`. It is a copy of the game's renderer without the line look. It paints ships with Ship Metal and rocks with Rock. It is number 1 in the renderer list on `Assets/Settings/PC_RPAsset.asset`. The Plot panel's Close Up Renderer value (1) points at it. PC_RPAsset now also lets the glow keep the see-through background (Alpha Processing on). **On the plot:** a new picture, `Close-up (3D view)`, in `Plot.prefab`. It is just above `Ground` and under `Grid`. The grid lines, marks, labels, shots, fighters and torpedoes draw on top of it. New values on the Plot panel: Close Up Image and Close Up Look. The close-up is cut at the visual range circles round our ship and our fighter flights, the same way the rocks are cut. It fades in with the rest of the plot when the board appears. **Ship sizes:** the ship models were tiny, about 13 units long. They are now their real length: our Asura 200 units, the Garuda 350, the frigate 100. The model inside each ship object in `02.1_Grid` was scaled. The two cameras that film the line pictures (Ship View Camera and Contact View Camera · Garuda) were moved back by the same amount. Their depth range was widened by the same amount. The line pictures look exactly as before. They were checked side by side. Each of those cameras now films only its own ship: with the models this big, a ship close by showed inside the other's picture. Own Ship Max Px went from 300 to 1200: a ship's drawn size now follows its model up to x4. The small circle round our ship, the start of the scan cone, the labels and the fighters' landing use that size. The close-up is filmed at the screen's pixels, so it is sharp on the 2880 x 1864 screen. **Out of sight stays out:** the close-up shows a ship only when it is in sight by the game's own rule. A ship behind a rock is left out, even inside 2,000 units. **Unchanged:** every ship is the same grey in the close-up. A ship's white, red or amber colour shows only in the line pictures. The name labels and the white lock corners still show. A destroyed ship keeps its line picture, broken in two, at every zoom. The close-up leaves wrecks out. Fighters, shots and torpedoes are still drawn flat on top. Gas clouds are unchanged. **Made without the owner:** the fade zooms x1 and x1.5. The x4 limit. The metal and rock colours. The glow strength. The sun's direction and height. **Files:** `PlotPanel.cs` (`ShowCloseUp`, `CloseUp`, the fades in `Refresh`, `ClipToSight`). `BoardManager.cs` (`zoomInLimit`, `SetZoom`). `PlotGrid.cs` (zoom up to x10). `ShipView.cs` (films only its own ship). `Ship.cs` (`All`, `Models`, `Nav`). `Plot.prefab`. `02.1_Grid.unity`. `PC_RPAsset.asset`. `Close Up Renderer.asset`. `Ship Metal.mat`, `Rock.mat` and `Close Up Look.asset` in `Assets/Art/Close Up`. The old cutscene experiment's camera (an experiment not in git) was moved from renderer 1 to 2 | **Done**. Compiled with no errors. Checked in play on Wolf Hunt, once. Our ship was moved by script next to the Garuda. At x1 both ships showed as lit grey metal (with the first fade numbers, x0.5 to x0.8). At x3 the Garuda's metal caught the light. At x0.65 the two looks were half and half. With the close-up switched off, the line picture of our ship matched the picture from before the change exactly. A destroyed Garuda kept its two broken line halves, with no model behind them. A QA agent then read the code and found six problems: a ship behind a rock showed; the 300 px cap; the sun angle values did nothing (the scene had no Sun Source); the start zoom; a ship close by inside the other's picture; a blurry picture. All six were fixed and checked in play again: the Garuda behind a rock inside 2,000 units was left out (`08_ship_behind_rock_x15`); the Garuda 142 units from our ship, each line picture showed only its own ship (`09_two_ships_close_lines_x08`); the sun turned to the Tuning values. No errors in the console. Screenshots in `Logs/CloseUp/`: `01_before_x1`, `02_closeup_x1`, `03_closeup_x3`, `04_lines_after_x1`, `05_ship_lines_before_after`, `06_wreck_x1`, `07_crossfade_x065`, `08` and `09`. Full playtest not run. No person has played it. Next: a nebula under the ships, then real shots and explosions |
| 165 | A nebula under the ships in the close-up (owner 2026-11-08: "create maybe some nebula effects at the bottom using some colour, so it looks like a rich, detailed area of space and not just black space". The plan put to the owner, and accepted: a nebula picture under everything, two or three layers at different depths, far ones moving slower as you pan, star dots the same way, and the metal reflecting the nebula's colours. Shown only in the close-up). **What the player sees:** zoom in past x1, and the space under the ships is no longer black. There are soft violet and teal gas clouds, stars, and thin warm and cold wisps of gas. The grid, marks and labels draw on top. Zoom out to x1 and the nebula fades away with the close-up. Past visual range it is cut at the circle, like the ships. **The layers:** three pictures in `Plot.prefab`, back to front: `Nebula far`, `Stars` and `Nebula near`. They are between `Ground` and `Close-up (3D view)`. Each one repeats in every direction. Far layers move less as you pan. At x1, Nebula far moves at 15% of the grid's speed, Stars at 30%, Nebula near at 50%. Zoomed in further they move less again. They also grow less as you zoom. What is at the plot's centre stays put while you zoom. **On the Plot panel:** a new list, Nebula, with one entry per layer. Image is the layer's picture. Depth is how fast it moves with the grid: 0.15, 0.3 and 0.5. At 1 it moves with the grid. Tile Units is how many units across one copy of the picture is at x1: 9,000, 1,300 and 5,000. Alpha is how strongly it shows: 1, 0.9 and 0.8. **The pictures:** stand-in art made by a script for this step. They are 1024 x 1024 and repeat without seams: `Assets/Art/Close Up/Nebula Far.png`, `Stars.png` and `Nebula Near.png`. To change the look, put another picture in the layer's Texture. Or replace the PNG with one of the same name. A new picture must repeat without seams. Its Wrap Mode must be Repeat. Nebula Near and Stars need see-through parts. **The metal reflects the nebula:** the board scene `02.1_Grid` now uses `Assets/Art/Close Up/Nebula Reflection.png` for reflections. It is set in the Lighting window, under Environment Reflections, Source Custom. It is a copy of Nebula Far, imported as a cube. If you change Nebula Far, change this copy too. The scene's ambient light is now a flat dim violet. It is set in the Lighting window, under Environment Lighting. The line pictures do not change. **Made without the owner:** the stand-in pictures and their colours. The three depths, sizes and strengths. The violet ambient light. **Files:** `PlotPanel.cs` (`NebulaLayer`, `nebula`, in `ShowCloseUp` and `Start`). `Plot.prefab`. `02.1_Grid.unity`. The four pictures in `Assets/Art/Close Up` | **Done**. Compiled with no errors. Checked in play on Wolf Hunt, once, at x1.8 beside the Garuda. Stars, gas and wisps showed under both ships, with the grid on top (`Logs/CloseUp/10_nebula_x18.png`). A pan of 500 units moved the layers 15%, 30% and 50% of the grid's move, measured (with the first version of the sums). A QA agent then found the layers slid sideways as you zoomed away from the map's centre. Fixed: zoomed from x1.5 to x4 with the view 9,000 units from the centre, each layer's centre stayed exactly where it was. Tile Units can't go below 1. No errors in the console. Full playtest not run. No person has played it. Next: real shots and explosions in the close-up |
| 166 | Shots and blasts in the close-up (owner 2026-11-08: "the idea is that when you're firing a beam, it actually looks like a beam that's firing"). **What the player sees:** zoom in past x1, and every shot in flight is a glowing streak. Ours are blue-white. The enemy's are red. A torpedo's streak is wider than a railgun slug's. The flat dot and trail still draw on top. A streak has the same length in units at every zoom. Where a shot hits a ship, there is a flash: a burst of orange sparks with a white centre. The same flash shows when a torpedo is shot down. It shows only in the close-up. Flashes and blasts run in game time: they stop when the board is paused, and run five times as fast at x5. When a ship is destroyed, there is a blast: a big white flash, sparks and slow embers. Its model then splits in two where the shot struck, the same as its line picture. The two halves drift apart and turn slowly. They follow the same rules as the line picture's halves: Wreck Drift Units and Wreck Spin Deg, already on the Board tab. The halves are a duller, darker grey metal. Zoomed out, the broken line picture shows as before. Between x1 and x1.5 the two fade into each other. **Tuning:** four new values on the Board tab of **Starwolf → Tuning**, under Plot & our ship. Slug Width Units (6) and Torpedo Width Units (11) are the streak's width at the front. Our Shot Glow and Their Shot Glow are the two streak colours. **The look:** the files are in `Assets/Art/Close Up`. `Glow.mat` is every streak. Its Colour sets how bright every glow is. It is now 3. `Glow.png` is the soft dot every streak, spark and flash is drawn with. `Glow.shader` is the glow. It adds light. `Hit Flash.prefab` is the hit's sparks and centre flash. `Kill Blast.prefab` is the blast: Flash, sparks and Embers. `Wreck Metal.mat` is the halves. New values on the Plot panel point at them: Glow, Hit Flash, Kill Blast and Wreck Look. **How it is set up:** the shots, blasts and halves are on Unity's TransparentFX layer. Only the close-up camera films it. The Sun in `02.1_Grid` now lights it too. The close-up picture now lets its light add over the nebula. A glow over empty space shows. `Assets/UI/Shader/BoardLinearImage.shader` has a new hidden value for this. Every other picture on the board is unchanged. The model file of the Garuda and the frigate (`Assets/3D Assets/UpdatedStarships/StarWolf_Cruiser_mesh.fbx`) now has Read/Write on. The game reads its shape to cut it. Our ship's model does not need it. A model that can't be read is not cut: that wreck keeps its broken line picture in the close-up. Each half keeps only its own part of the model. Cutting the Garuda took 102 ms, once, on the frame of the kill. The glow in `Close Up Look.asset` is now smoother: High Quality Filtering on, Scatter 0.55, Threshold 0.9. The first blast showed a blocky halo. **Made without the owner:** the streak colours and widths. The look of the flash and the blast. The duller wreck grey. The glow settings. **Not done:** PDC fire has no streak yet. Fighters are still flat triangles. **Files:** `PlotPanel.cs` (`ShotAt`, `ShowShotsAndBlasts`, `Break`, `NewStreak`). `BoardLinearImage.shader`. `Plot.prefab`. `02.1_Grid.unity`. `StarWolf_Cruiser_mesh.fbx.meta`. `Close Up Look.asset`. `Glow.mat`, `Glow.png`, `Glow.shader`, `Hit Flash.prefab`, `Kill Blast.prefab` and `Wreck Metal.mat` in `Assets/Art/Close Up` | **Done**. Compiled with no errors. Checked in play on Wolf Hunt, once. Shots were added by script: three slugs at the Garuda, one of them wide. The slugs showed as blue-white streaks. An enemy torpedo showed as a red streak (`Logs/CloseUp/14_streaks_and_hit.png`). A slug hit showed a flash on the Garuda's side (`11_shots_in_flight.png`). The Garuda was destroyed by script. The blast showed (`15_kill_blast.png`). Then two grey halves drifted apart (`16_wreck_halves.png`). At x1.2 the 3D halves were checked against the line halves twice: at the kill, and 23 seconds later. Both times they had the same turn, 298° and then 180°. Both times they were within about 20 px of the line halves (`18_halves_age0_crossfade.png`, `19_halves_later_crossfade.png`). The effects' clock was slowed for the pictures. Nothing in the game was changed for that. A QA agent then found: a crash on RESTART if a model could not be cut; such a wreck would vanish in the close-up; the halves kept the whole model each; flashes ran in real time; the streak changed shape with zoom; two typed-in numbers; a new list every frame. All fixed. Checked again in play: a hit flash froze when the board was paused (stopped at 0.46 s); the cut halves kept 146,857 and 263,823 points of the model's 410,258 (`20_halves_after_fixes.png`). No errors in the console. Full playtest not run. No person has played it |
| 167 | A campaign: three save slots and a sector map of missions (owner 2026-11-08: "you click New Game... We'll have a page which says Save Slots. You have three save slots. Then you have a screen which shows a grid or a sector of space, and a little dot appears. You click that, and that's your first mission. Later, when you solve that, you go to the next mission, which is another dot." Owner answers: three slots. The game saves only between missions. The first dots are today's seven levels, in the level list's order. The level list stays for testing, from a second row on the title. Four new scripts OK'd: SaveGame, SaveSlots, SectorMap and ShipRooms. The sector map has a stand-in look until the owner sends a reference). **The title:** the menu now reads NEW GAME, LEVELS, SETTINGS. LEVELS was LOAD GAME, which did nothing. It now opens the MISSIONS list. The title's lettering had no V. A V was added to `Assets/StarwolfIntro/Resources/StarwolfIntro/glyphs.json`. It is built from 2 x 2 blocks, like the other letters. **SAVE SLOTS:** NEW GAME opens it. It has three rows. Each row starts with its number. An empty row reads EMPTY, and under it START A NEW CAMPAIGN. A used row reads CAMPAIGN · 1 MISSION DONE (or MISSIONS DONE), and under it SAVED and the time. Click a row. An empty row starts a new campaign. A used row continues its campaign. Either one opens the SECTOR MAP. A used row has DELETE to its right. The first click turns it into SURE? CLICK AGAIN. The second click empties the slot. Escape quits the game, as on the MISSIONS list. **SECTOR MAP:** the title SECTOR MAP, a faint green grid, and the campaign's missions as dots. At the start one dot shows: 1 FLIGHT DRILL. The open dot slowly grows and shrinks. Click a dot. A box opens with the mission's number and name, its briefing line and LAUNCH. LAUNCH shows INITIALISING... and opens the board with that level, as the MISSIONS list does. Escape quits the game here too. **After a mission:** win it, and the save notes it at once. MISSION SELECT on MISSION COMPLETE goes back to the SECTOR MAP. So does MISSION SELECT on the pause menu. For a level started from the MISSIONS list, both still go to the MISSIONS list. On the map the finished dot is dimmer, with a tick after its name. A line leads to the next dot. The new dot fades in the first time it shows. A finished mission can be played again. Lose, and the dot stays open to try again. **The missions:** the seven levels, in the MISSIONS list's order: FLIGHT DRILL, COLD RUN, WEAPONS FREE, NO PEEKING, FINDERS KEEPERS, INTERCEPT, WOLF HUNT. Each one opens when the one before it is won. The list is on the Sector Map Script object in `SectorMap.unity`. Each mission has a Title, a Level (the level scene's name), a Brief and an At. At is where its dot is, in px from the middle of the 1920 x 1080 screen. **The save files:** one small file per slot, `slot1.json` to `slot3.json`. They are in the folder Unity gives the game. On a Mac it is `~/Library/Application Support/Shagun Shah/Starwolf`. On Windows it is `AppData/LocalLow/Shagun Shah/Starwolf`. The folder's name comes from Company Name and Product Name in Project Settings → Player. Change either, and the game looks in a new, empty folder. A save keeps the missions won and how many dots the map has shown. The game writes the new file beside the old one, then swaps them. It keeps the one before as a .bak. If a file can't be read, the .bak is used. A later build can add things to a save. Older files still load. **Made without the owner:** the second row's name, LEVELS. The V's shape. The wording on the slot rows and SURE? CLICK AGAIN. The map's layout, dot places, grid and box. MISSION SELECT going to the map in a campaign. Playing a finished mission again. Escape quitting on these two screens. **Files:** new `SaveGame.cs` (the save files), `SaveSlots.cs` (the SAVE SLOTS screen) and `SectorMap.cs` (the map), all in `Assets/Scripts/Game`. New scenes `Assets/Scenes/Tactical/SaveSlots.unity` and `SectorMap.unity`. They are copies of `01_Briefing.unity`, with the same green look. Both are in the build list. New picture `Assets/Art/Sector Map/Grid Square.png`, the map's grid (stand-in). Changed: `ScreenFlow.cs` (`Go`). `QuestSystem.cs` (a win is saved). `EndScreenPanel.cs` (Campaign Scene, "SectorMap"). `StarwolfIntroController.cs` (Second Row Text). `00_Start.unity`. `glyphs.json`. `PlaytestRunner.cs` (its step 3 now presses LEVELS; new section 32 checks the campaign: slot 3 opens the map with one dot, a win is saved, the map then shows two, DELETE empties the slot. It skips itself if slot 3 holds a real save). **Also in this step:** the full playtest after Steps 164 to 166 passed 244 of 245 (`Logs/Playtests/2026-11-08_221749`). The one failure was an old check. Step 162 (drawn launch lines) changed how fighters leave the ship. No full playtest ran after it. The check now allows a drawn hangar line | **Done**. Compiled with no errors. Played the real flow once in the editor. The game's buttons were pressed by script. The title showed NEW GAME, LEVEL LIST, SETTINGS (`Logs/Campaign/02_title_menu.png`; the row was renamed LEVELS after QA). NEW GAME opened SAVE SLOTS with three empty rows (`03_save_slots_empty.png`). Slot 3 made `slot3.json` and opened the map with one dot (`04_sector_map_first_dot.png`). The dot opened the box (`05_mission_box.png`). LAUNCH opened the board on FLIGHT DRILL. The mission was ended as won by script (`06_mission_complete.png`). The save then listed FlightDrill. MISSION SELECT opened the map with FLIGHT DRILL ticked and COLD RUN new (`07_sector_map_second_dot.png`). SAVE SLOTS showed slot 3 used. DELETE asked first. The second click emptied the slot and its .bak (`08_save_slots_used_delete_asks.png`). A QA agent then found: a failed save (a full disk, or a file in use) was not caught, and the map came up empty; playing `SectorMap.unity` straight from the editor wrote into the real slot 1; LEVEL LIST was wider than its click area; the pulse size was typed into the code, and the dot's name grew with it. Fixed: a failed save is logged and the game goes on with the old save; the map with no slot picked opens SAVE SLOTS (checked: no file made); the row is now LEVELS; Pulse Size is on the Sector Map Script, and only the dot grows. Not changed: nothing yet marks the end when every mission is done; nothing leads back to the title, so a picked slot stays in play until the game quits. No errors in the console. Full playtest not run since. No person has played it. Next: after a mission, the ship's rooms (BRIDGE, ENGINEERING, HANGAR, QUARTERS), then repairs |
| 168 | The ship's rooms between missions (owner 2026-11-08: "When you finish the level, we'll use the same border edge of our tactical grid, but we'll have some other tabs there, and you can go to different rooms on the ship. These are 2D pieces of art, and you may click on characters and speak to them. That'll open a panel where they talk to you"; then: "If you click on any of the bridge, engineering, hangar, or quarters, then you can just give me some Unity sprites or boxes. There's no need for final art. I just need a placeholder, or even, in the art manager, if that exists as yet, I need a place where I can put an image in there."). **What the player sees:** in a campaign, MISSION SELECT now opens the ship. It does this on MISSION COMPLETE, on SHIP LOST and on the pause menu. The board's metal frame shows. Along its top are five tabs: BRIDGE, ENGINEERING, HANGAR, QUARTERS and SECTOR MAP ›. The picked tab is gold. The others are grey. BRIDGE shows first. **The rooms:** each room is a stand-in painting. It is a dark coloured panel with the room's name in faint letters. Each room has one person in it: a grey box with their name under it. The XO is on the BRIDGE. The CHIEF ENGINEER is in ENGINEERING. The FLIGHT LEADER is in the HANGAR. The MARINE SERGEANT is in QUARTERS. These names and their lines are stand-ins until the story is written. The owner will plan the story separately. **Talking:** click a person, and the talk box opens at the bottom. It shows their picture, their name in gold and what they say. Up to three answers are on the right. An answer leads to another line, or ends the talk. A line with no answers shows LEAVE. Picking another tab closes the talk. SECTOR MAP › opens the sector map. **The art:** the `Ship` object in `Ship.unity` has an Art List. It has one slot per room painting: BRIDGE painting, ENGINEERING painting, HANGAR painting and QUARTERS painting. It has one slot per person: XO picture, CHIEF ENGINEER picture, FLIGHT LEADER picture and MARINE SERGEANT picture. Put a sprite in a slot, and the room or the person shows it. The talk box shows the same picture as the person in the room. **The words:** the same `Ship` object has the Ship Rooms script, with its Rooms list. Each room has a Name, a View (the painting), a Tab and People. Each person has a Name, a Picture (the box you click) and Lines. Each line has a Text and up to three Replies. Each reply has a Text and a Next. Next is the line it leads to, counting from 0. At -1 it ends the talk. **How it is made:** `Ship.unity` is a copy of the board scene `02.1_Grid.unity`. Only the board's camera, its picture and the `Frame` are kept. The rooms are drawn on top of the frame, where the plot is on the board. The tabs are in the frame's top band. **Made without the owner:** the four people and their stand-in lines. The room colours. The tab look. The talk box's layout. MISSION SELECT going to the ship after a lost mission too. **Files:** new `Assets/Scripts/Game/ShipRooms.cs`. New scene `Assets/Scenes/Tactical/Ship.unity`, in the build list. Changed: `EndScreenPanel.cs` (Campaign Scene is now "Ship"). `End Screen.prefab` | **Done**. Compiled with no errors. Played in the editor. The buttons were pressed by script: SAVE SLOTS, slot 3, the map, LAUNCH on FLIGHT DRILL. The mission was ended as won by script. MISSION SELECT opened the ship. The HANGAR tab showed the HANGAR. The FLIGHT LEADER's talk box opened (`Logs/Campaign/11_ship_after_win_hangar.png`). On the BRIDGE, the XO's answer ANY NEWS? led to the next line. CARRY ON. closed the talk (`10_ship_bridge_talk.png`). SECTOR MAP › opened the map with two dots. Slot 3 was emptied after the test. A QA agent then found: Escape did nothing on this screen; art put on the Art List would be tinted by the stand-in grey; a tab with no text would crash; the scene still loaded the nebula reflection. Fixed: Escape quits, as on SAVE SLOTS and the map (a ScreenFlow object); a room or person with a sprite shows it in its own colours; the tab text is checked; the reflection is cleared. Checked in play again: no errors. Note for art: import paintings with sRGB off, like the board's other art. No errors in the console. Full playtest not run. No person has played it. Next: repairs in ENGINEERING. Damage will stay on the ship between campaign missions |
| 169 | Repairs in ENGINEERING: damage stays on the ship between campaign missions (owner 2026-11-08: "maybe you can undertake some research, repair actions"; then: "I don't really need research per se. I think repairs are fine." The owner's answers before building: damage stays on the ship between missions in the campaign only. The test levels still start undamaged). **What the player sees:** in a campaign, win a mission, and our ship keeps its damage. The next campaign mission starts with it. On the ship, ENGINEERING shows a DAMAGE REPORT beside the CHIEF ENGINEER. It lists each part that is not whole. Each line shows what is left and the most it can be: PROW ARMOUR 25 / 100, MAGAZINE 3 / 5. With no damage it reads ALL PARTS WHOLE. REPAIR ALL is under the report. It makes the ship whole and saves at once. It shows only while there is damage. **What stays:** each side's armour: PROW, STBD, AFT and PORT. Each room's integrity: MAGAZINE, BRIDGE, HABITATION, HANGAR BAY, REACTOR and ENGINE ROOM. **What ends with the mission:** fires, radiation, vented rooms and crew losses. A room that ends at 0 is kept, and shown, at 1. A reactor at 0 would destroy the ship at once. **When it is kept:** a win's damage goes into the save when you press MISSION SELECT. RESTART MISSION on the win screen replays the mission from the damage it started with. **A lost mission keeps nothing:** the ship is as it was before that mission. Levels from LEVELS (the MISSIONS list) always start whole. **Repairs:** they cost nothing for now and take no time. A question for the owner: should repairs take time, crew or something else? **The save file:** a new list, damage. It has one entry per part, with its name, now and most. Empty means whole. Older saves without it load as whole. **In the scene:** a new panel, Damage report, in Room · ENGINEERING in `Ship.unity`. It has Title (DAMAGE REPORT), Report and REPAIR ALL. Ship Rooms has two new values under Repairs: Damage Report and Repair All. **Made without the owner:** repairs are free and instant. One button repairs everything. Fires, radiation, vented rooms and crew don't stay. A room at 0 comes back at 1. A lost mission keeps nothing. **Files:** `SaveGame.cs` (`KeepDamage`, `PutDamage`, `Repair`, `Part`). `QuestSystem.cs` (a campaign mission starts with the kept damage; a win keeps it). `ShipRooms.cs` (Damage Report, Repair All). `Ship.unity` | **Done**. Compiled with no errors. Played in the editor, once, on slot 3. The buttons were pressed by script. FLIGHT DRILL started whole: PROW 100, MAGAZINE 5. During it, by script, the prow armour was set to 25 and the magazine to 3. The mission was ended as won by script. The save then listed PROW ARMOUR 25/100 and MAGAZINE 3/5. ENGINEERING showed them (`Logs/Campaign/12_engineering_damage_report_b.png`). COLD RUN was launched without a repair. It started with PROW 25 and MAGAZINE 3. It was won. REPAIR ALL then showed ALL PARTS WHOLE, hid the button and emptied the save's damage (`13_engineering_after_repair.png`). WEAPONS FREE then started with PROW 100 and MAGAZINE 5. Slot 3 was emptied after the test. A QA agent then found: RESTART MISSION on the win screen replayed with the win's damage, so each replay started worse; a room at 0 was shown as 0 but came back at 1; armour like 29.7 of 30 showed as 30 / 30. Fixed: the win's damage is kept at MISSION SELECT; a room at 0 is kept as 1; the report rounds down. Checked in play again: after a win with damage, RESTART MISSION started whole (STBD 100, HANGAR BAY 5); won again with STBD 29.7 and HANGAR BAY 0, MISSION SELECT kept them, and ENGINEERING read STBD ARMOUR 29 / 100 and HANGAR BAY 1 / 5. No errors from the game in the console. Full playtest after Steps 167 to 169: 246 of 248 (`Logs/Playtests/2026-11-08_225218`). Every game check passed. The two failures were the new campaign checks (section 32): they waited 0.6 real seconds, and while the playtest records its video the game's clock can run behind, so the half-second click guard on SAVE SLOTS was still shut. The checks now wait in game time. They were then checked by hand in play: slot 3 opened the map with one dot, a win showed two, DELETE emptied the slot. No person has played it |
| 170 | The spinal railgun: our ship becomes a sniper (owner 2026-11-08: "I think I want to set the starship up into a sniper configuration"; "The railgun will be a spinal weapon, so it'll be at the nose of the ship. It has no swivel… you need to rotate the ship fully to aim it"; "You have to slow the ship completely. It has to be at zero drift"; "an assault destroyer should have boarding capability… But it shouldn't have fighter jets it can deploy"; "maybe we should call it Nuclear Lance". The plan is `docs/SNIPER_PLAN.md`. This is its step 1. Owner 2026-11-09: "please go", and "don't worry about the tuning. You can use your judgement to put a value". Then, the same day: "you can get a target as well… you can ballpark the thing, or you can get into the correct radius range and do all the steps to get a proper target lock. At which point, your ship also just starts auto-adjusting, and then you can fire". On the name, the same day: "it is a spinal railgun… We don't have to call it nuclear lance. We don't want to be stupid. Keep it railgun… it is a spinal railgun in this current configuration. It means it cannot swivel. It just points straight." Then: "the mount does not need to say SPINAL RAILGUN. It will also continue to say RAILGUN."). **What the player sees:** our ship has one gun on its nose, the spinal railgun. The weapon picker still reads RAILGUN. The gun doesn't swivel. It points straight ahead. You aim it by turning the ship. Open fire control, and a thin line runs out of the nose to the railgun's full reach, 30,000. There are two ways to aim. By eye: turn the ship yourself. Hold Shift with A or D, and it turns slowly, 2° a second. With a lock: lock the target with the targeting computer, the usual way. While the lock is on, the ship turns itself onto where the target will be. Then set the four switches and pull the lever. Lay a RULER, and its label also says how long a slug takes to fly it: "18,000 km · SLUG 12 S". The slug leaves the nose and flies straight along the line at 1,500 a second. If the ship is drifting, the slug drifts too, and misses. The hangar has no fighters. A new test level, LONG SHOT, is row 8 on the MISSIONS list. **The spinal railgun:** in our ship's file, `Asura.asset`. One railgun: id SRG, label RAILGUN, short name RG, kind RG, Arc 0, Half Arc 0, reach 30,000, 12 slugs. It is on a new hard point dot at the nose's centre, (0, 100). The side railguns RG-A and RG-B are gone from our ship. Their two dots on the long sides stay, empty, for lasers later. Our slug speed is now 1,500 a second. It was 300. A slug does the same damage as before. A shot still costs 70% of the capacitor. The Garuda's file is unchanged: two railguns that turn, slug speed 300. **The rule:** a railgun with Half Arc 0 is fixed to the hull. It fires straight along its arc, to the part of a degree. It can fire with no target, no lock, no firing solution, no turret turn and no periscope bearing. LOAD, ANGLE and BEARING open with nothing set. ARM still needs the capacitor's charge. The four switches stay for now. Step 3 of the plan takes ANGLE and BEARING away. **The lock turns the ship:** while a lock is on and the railgun is picked, the ship turns itself onto the lead. It turns the whole degrees at its usual turn rate. Then it turns the last part of a degree at the slow turn's 2° a second. The lead uses the lock's course: from a scan or from a plotted track, whichever is newer. With neither, it points at where the target is now (NO COURSE). When the lock runs out after 10 seconds, the ship stops turning itself, and the helm is the player's again. While it turns itself, it clears the helm's attitude key, and A and D can't turn against it. Shift with A or D still works. Only a lock turns the ship. A TRACK solution from the plot does not. **Fire control:** the weapon picker's printed label still reads RAILGUN. The mount screen reads RAILGUN, FIXED 000°, 30,000 km and SLUGS 12. With the four switches set, the ready line reads MOUNT READY. A gun that turns still reads SOLUTION LOCKED — MOUNT READY. For the spinal railgun, the ready line never asks for a target or a lock. It goes on to WEAPONS NOT RELEASED or ARMING SEQUENCE INCOMPLETE. **The plot:** with Fire Control open, the railgun's solid bar runs from the nose dot out to 30,000. A fixed gun has no dotted cone. The reach ring at 30,000 still shows. With the spinal railgun picked, a RULER's label adds the slug's flight time. A ruler from POINT to POINT (a track) still shows course and speed. **The slow turn:** hold Shift with A or D. The ship turns 2° a game second, in parts of a degree. ENG power doesn't change it. The ordered heading follows the bow. It clears the helm's attitude key, as A and D do. A normal A or D turn drops the part of a degree. The helm's HDG still shows whole degrees. The ship on the plot turns by the part of a degree. The speed is Fine Turn Deg Per S (2), on the Ship tab of **Starwolf → Tuning**, under Navigation. The key is Fine Turn Key (LeftShift), on the Keys tab. **Drift:** every slug now keeps the motion of the ship that fired it. This is true for us and for the Garuda. A ship sliding 5 units a second puts a spinal railgun slug 100 units off the line at 30,000. You can fire while moving. You just miss. The Garuda's aim now allows for its own motion. So does our fire control's lead. **Fighters:** a new number in each ship's file: Hangar → Fighters, 0 to 6, default 6. The Asura's is 0. The Garuda's is 6, as before. At load the ship keeps only that many fighters. Nothing is deleted. With none, the hangar starts on the assault boats. The Hangar panel's INTERCEPTOR row shows nothing. Assault boats are unchanged. **LONG SHOT:** `Assets/Scenes/Levels/LongShot.unity`, made from a copy of ColdRun, now in the build list. Battle space 40,000 × 40,000. We start at (0, −14,000), facing north. The Garuda patrols east and west between (−15,000, −5,000) and (15,000, −5,000), at its usual cruise speed. Hunts is off: it doesn't hunt us and doesn't fire. Goal: Destroy. Three gas clouds near our start: (0, −11,000), 2,400 × 1,400; (−7,000, −10,000), 2,000 × 1,200; (7,000, −12,500), 2,000 × 1,400. Four rock groups, ColdRun's Big Rocks moved: (−4,000, −9,000), (5,000, −8,000), (−9,000, 2,000) and (9,000, 3,000). The briefing's eight steps: power up, go to Condition 2, turn on DRADIS, find the Garuda's course (ACTIVE SCAN, or POINT and RULER once it is in sight), stop the ship, open fire control, turn the nose onto where the Garuda will be (Shift with A or D turns slowly), set the four switches and pull the lever. Row 8 on the MISSIONS list reads 8 LONG SHOT, DESTROY THE GARUDA WITH THE SPINAL RAILGUN, FROM FAR AWAY. The list's eight rows are now closer together: 98 px apart, the first at 206. They fit under the title. LONG SHOT is not on the campaign's sector map. There are no sensor drones yet (plan step 4). So you find the Garuda only inside DRADIS reach, 5,000 to 7,500. Long shots past DRADIS come later. **The playtest:** it still plays by its own written numbers: the two side turrets and slug speed 2,500. Its turret checks are unchanged. It puts six fighters back on our ship for its fighter checks. The level list check now expects eight rows, with LONG SHOT. Section 21 stops our ship before its shot. The ruler check zooms out and centres the plot first. New section 25f, "The spinal railgun", checks the gun from the ship's real file: one fixed gun on the nose; LOAD, ANGLE and BEARING open with no solution; Shift with D turns in parts of a degree; a drifting ship's slug drifts; the ruler reads SLUG 10 S for 15,000; a still Garuda, nose on, 15,000 away at bearing 10.45°: aimed at 10° the railgun misses, aimed to the part of a degree it hits; on a lock (a stand-in locked solution, stepped by hand) the ship turns itself onto the target to the part of a degree; with no lock it stops. **Fixed: the ship in the targeting computer tilted** (owner 2026-11-09: "the position of the ships within the targeting computer… Once again, they were tilting up and down." The owner's rule: the ship in the targeting computer always lies flat, bow left or right). The fault was older than this step. When a ship is wrecked, the camera that films it turns with the wreck (`ShipView.WreckHeading`, set by `PlotPanel.ShowBroken` since Step 131). It never turns back. The sight didn't allow for that. The playtest wrecks the Garuda with the marines (21f), then brings it back as a stand-in. Its 28g screenshot has shown the ship pointing up ever since. Now the sight's picture takes the camera's turn off as well (`PeriscopePanel.LateUpdate`). New `PeriscopePanel.PictureBowDeg`: where the bow points in the sight, 90 or 270 when it lies flat. A new playtest check, just before the 28g shot: "The ship in the targeting computer lies flat, bow left or right". **Not changed:** the briefings of WEAPONS FREE and NO PEEKING, and WEAPONS FREE's row on the MISSIONS list, still talk about the railgun, turrets and cones. The owner said all levels are test levels. The sector map lists the first seven levels only. **Made without the owner:** 12 slugs. Slug speed 1,500. The slow turn at 2° a second, on Shift. The spinal railgun can fire with no target or lock. Only a lock turns the ship; a TRACK solution does not. The ready line MOUNT READY. FIXED 000° on the mount screen. The slow turn clears the attitude key, and a normal turn drops the part of a degree. HDG stays in whole degrees. Every slug keeps its ship's motion, the Garuda's too. The Fighters number on each ship. LONG SHOT's layout, its row text, and its Garuda not hunting. The MISSIONS rows moved closer together. **Files:** `WeaponSystem.cs` (`FixedGun`, `LeadBearing`, `TickGuns` sets the lock's turn, a slug leaves with its ship's motion). `NavSystem.cs` (`fineTurnDegPerS`, `fineTurn`, `fine`, `TrueHeading`, `aimHeading`, in `Steer`). `HelmPanel.cs` (`fineTurnKey`). `FireControlPanel.cs` (FIXED, MOUNT READY). `PlotPanel.cs` (no cone for a fixed gun; the ruler's SLUG time). `HangarSystem.cs` (`fighters`, `KeepFighters`). `HangarPanel.cs`. `Ship.cs`. `EnemyBrain.cs` (`LeadOn`). `Asura.asset`. `01_Briefing.unity` (the MISSIONS list). New `Assets/Scenes/Levels/LongShot.unity`. `EditorBuildSettings.asset`. `PeriscopePanel.cs` (`LateUpdate`, `PictureBowDeg`). `PlaytestRunner.cs` (`WrittenNumbers`, section 25f, the ruler check, the flat-sight check before 28g) | **Done**. Compiled with no errors. Played in the editor on LONG SHOT. Fire control showed LANCE, NUCLEAR LANCE, FIXED 000°, 30,000 km and SLUGS 12 (`Logs/SniperCheck/01_lance_line.png`). These checks ran before the owner's name change. The screens still showed the first name, the Nuclear Lance. The line ran from the nose. LOAD opened with no target. A shot left the nose dot going straight north at 1,500. The slow turn went 0.5°, 1.0°, 1.5° in three quarter-second steps. A shot fired while sliding 5 units a second went 0.19° off the line: 100 units at 30,000. The ruler read "18,000 km · LANCE 12 S" (`03_ruler.png`). The hangar started on ASSAULT BOAT, with INTERCEPTOR at 0 craft. A real hit: our ship stopped 6,000 units from a Garuda moving east at 22. The nose was turned by hand to the lead bearing, 337.81°, using the part of a degree. The slug took 4.2 s and reported "HIT · BOGEY T07". The close-up (zoom past x1) still draws (`04_close_up.png`). The MISSIONS list shows all eight rows under the title (`05_level_list.png`). The scale sums, `python3 docs/scale-sim/scalesim.py --sniper` (written by the study team): at slug speed 1,500, aiming in whole degrees, a plotted course gave a 92% hit chance at 15,000 and 65% at 30,000. The slow turn's parts of a degree remove the whole-degree limit. The lock's turn has its own checks in the playtest's section 25f. The new names were checked on screen: the picker read RAILGUN, the ruler read "18,000 km · SLUG 12 S", and LONG SHOT's briefing fits its box (`Logs/SniperCheck/07_spinal_railgun_plot.png`). The sight fix was checked in play: with the Garuda's camera left turned 37°, the sight showed it flat, bow at 270° (`Logs/SniperCheck/10_sight_flat_after_wreck_camera.png`). First full playtest: 254 of 256 (`Logs/Playtests/2026-11-09_004916`). The two failures were checks, not the game. The ruler check read a label that was off the plot; it now zooms out and centres first. Step 171's slide-fade check was fixed by its own session. Second full playtest: 256 of 256 passed (`Logs/Playtests/2026-11-09_005653`). The sight fix and its check came after the second run. They have not been through a full run yet. No person has played it. Next: plan step 2, the flare |
| 171 | Q, E, W and S: a tap is a small nudge, holding builds up (owner 2026-11-09: tap left and right to steady the ship, most of all when coming to a full stop). **Q and E, the side thrusters:** they slide the ship left or right with the nose still. A tap gives a small sideways push: 2 units/s of side speed at full ENG. The game never misses a tap now. Before, Q and E were only checked about twice a second, and a quick tap often did nothing at all. Taps never push past full side speed. Two taps very close together count as one. Held, the push starts gentle and grows to full over 2 seconds. Our ship reaches full side speed, 20, after about 7.5 seconds. Let go, and the ship keeps drifting. The drift fades by itself, to half in about 8 seconds (Drift Keep Per S, as before). A tap the other way slows it. Before, letting go made the side thrusters stop the slide. With no ENG power, Q and E do nothing, as before. **W and S, the throttle:** a tap moves the throttle 1. It used to move 10. Held, the throttle keeps counting, 1, 2, 3, slowly at first, then faster. It has moved 100 after 2 seconds. It stays where you let go. S still goes past 0 into reverse. It stops at what ENG allows. If the throttle changes some other way while the key is down (power lost, the slider dragged), the key stops moving it. **Fixed on the way:** holding W and then tapping A no longer stops the throttle climbing. **Tuning:** in each ship's stats file, under Side thrusters: Side Tap Units (2) and Side Ramp Seconds (2). Side Accel Units is now the full push with the key down. On the Helm tab of **Starwolf → Tuning**: Throttle Hold Seconds (2). Throttle Key Step (10) is gone. **Made without the owner:** a tap is 2 units/s, the push grows over 2 seconds, and the throttle goes from 0 to 100 in 2 seconds. Each can be changed in Tuning. **Files:** `NavSystem.cs` (`Move`, and the Side thrusters part of `Stats`). `HelmPanel.cs` (`Update`, and the new `Throttle`). `PlaytestRunner.cs` (section 25e) | **Done**. A quick check in play mode. One Q tap gave 2 units/s to port. Two E taps after it gave about 0. Holding Q gave 2.1 at 1.3 s, 6.4 at 2.5 s and 14.2 at 5 s, then 20 from 7.6 s on. After letting go: 15.7 at 2.5 s and 9.7 at 7.6 s. The heading stayed 000. With ENG off, a tap did nothing. The playtest check in section 25e now tests the tap, the hold and the fade: "A tap of Q is a small push; held, Q slides the ship to port with its nose still; let go, and the slide fades". W and S need a person: a script can't press keys. No person has played it |
| 172 | The sector map's new look (owner 2026-11-09: bring in the Sector Operations Map that Codex made. Codex's two pictures of the screen, the first mission and four missions won, were the target look). **What the player sees:** a painted console fills the screen. On the left is a BRIEFING screen. It is headed STARWOLF / DESTROYER / OPERATIONS, then MISSION RECORD. Under that are the picked mission's number and name, and its text. On the right is an OPERATIONAL MAP screen, headed STARWOLF / SECTOR PLOT. It is a star chart with the mission dots joined by lines. **The MISSION ARCHIVE box:** it is on the map and has three buttons. BRIEFING shows the mission's briefing text on the left screen. DEBRIEF works only on a finished mission. It shows MISSION COMPLETE above the briefing text. LAUNCH plays the mission. On a finished mission it reads REPLAY MISSION and plays the mission again. **The dots:** a finished mission's dot is a tick picture. The open mission's dot is a green dot that grows and shrinks. When the map opens, the newest dot is already picked, so the box and the left screen show it at once. The dots moved to new places on the chart. The mission order is the same: FLIGHT DRILL, COLD RUN, WEAPONS FREE, NO PEEKING, FINDERS KEEPERS, INTERCEPT, WOLF HUNT. The first dot is still FLIGHT DRILL. **In the scene:** the console is `Operational Map Console`, a Raw Image over the whole 1920 x 1080 screen. The headings, the star chart and the three empty button frames are painted into the picture. The game writes the mission's number, name and text, the button words, the dots and their names over it. The Sector Map Script has three new slots: Launch Label (the LAUNCH text), Debrief Button and Completed Node (the tick picture). It has two new button calls: ShowBriefing and ShowDebrief. The old typed SECTOR MAP title from Step 167, `SECTOR MAP card`, is still in the scene. The console picture draws over it, so it does not show. Every part, slot and picture is listed in `docs/BOARD_UI.md`, "Sector map". **The pictures:** `Assets/UI/Board/Art/Briefing/OperationalMapConsole.png` (1840 x 1035) and `CompletedMissionNode.png` (44 x 44). Codex's import settings came with them: sRGB (Color Texture) on, Compression None. The old grid picture, `Assets/Art/Sector Map/Grid Square.png`, is no longer used. It was not deleted. **Files:** `SectorMap.cs` (Launch Label, Debrief Button, Completed Node, `ShowBriefing`, `ShowDebrief`; the newest dot is picked when the map opens; the notes at the top and the tooltips describe the new screen). `SectorMap.unity`. The two new pictures and their `.meta` files | **Done**. Compiled with no errors. Played in the editor through the real campaign path. SAVE SLOTS, then slot 3: the map showed one dot, FLIGHT DRILL. LAUNCH opened FLIGHT DRILL. MISSION COMPLETE, then MISSION SELECT, opened the ship's rooms. SECTOR MAP › opened the map with two dots, FLIGHT DRILL ticked. Clicking FLIGHT DRILL: DEBRIEF showed MISSION COMPLETE, BRIEFING showed the brief, and the button read REPLAY MISSION. With four missions won, the map matched Codex's picture of it. REPLAY MISSION on COLD RUN opened COLD RUN. No errors from the map in the console. The playtest's campaign checks (section 32 of `PlaytestRunner.cs`) still match the new map. Full playtest not run since. No person has played it |
| 173 | The close-up comes in sooner: the full 3D look at zoom x1.1, not x1.5 (owner 2026-11-09: "the zoom range shouldn't start at 1.5x. It should actually be when you're at maybe even 1x or maybe 1.1x. We need to find a way that you get some satisfaction from what you're watching, as well as not be so zoomed in that you can't see anything"). **What the player sees:** a level still opens at x1 with the line pictures. One notch of the scroll wheel zooms x1.12, so one notch in shows the full 3D look. Between x1 and x1.1 the line pictures and the 3D models fade into each other. **Tuning:** on the Plot panel in `Plot.prefab`, **Close Up From Zoom** stays 1 and **Close Up Full Zoom** went from 1.5 to 1.1. Both are on the Board tab of **Starwolf → Tuning**, under Plot & our ship. To open levels in the 3D look, set them to 0.8 and 1. The code's default is still 1.5. The prefab's number is the one the game uses. Step 164's row keeps the old numbers as history. **Files:** `Plot.prefab` | **Done**. Only the prefab number changed. Checked in play mode on 2026-11-09 (Step 177): at x1, Wolf Hunt shows the line look. One wheel notch in (x1.12) shows the metal ship and the nebula. Screenshots: `Logs/CloseUp/step173_x1.png` and `step173_x112.png`. No person has played it |
| 174 | SECTION 36 flickers in and turns into STARWOLF (owner 2026-11-09, on the title screen: "instead of having it fade in from left to right, is it possible that it just flickers into being in its position, and then the same for Starwolf?" Offered two ways, the owner picked the second: "this flicking in is maybe fast-ish, stays for a few seconds, and then quickly flickers into Starwolf. I like that. one word should turn into the other." Then: "When you're doing that transition and flicker in, it might be cool to have a little bit of horizontal diffusion grain"). **What the player sees:** in the full intro (`00_Start`), SECTION 36 flickers into being where it stands. It comes on square by square, in a quick scattered order, over about 0.4 s. It no longer types in left to right. It stays (Section Duration is 4 s in all). Then it turns into STARWOLF over about 0.6 s. Every square lit in both words stays lit: the S (both words start with S) and most of the box. Squares only SECTION 36 uses flicker out. Squares only STARWOLF uses flicker on. (Step 177: the box now stays still.) While a word flickers in or over, thin bands slip sideways, and the light smears and grains along them. It dies away as the squares settle. It never shows on the menu. STARWOLF stays (Title Duration 4 s), then the same straight cut to the menu as before. **Where the words are:** so the S's line up, both words share one left edge. (Step 177: both words now centred, sized to each other.) **Not changed:** INITIALISING... and the title-only pages (SAVE SLOTS, SECTOR MAP, MISSIONS and the mission cards) still type in left to right, in the middle. **Tuning:** three new settings on the StarwolfIntroController, on the `00_Start` object: Flicker In Seconds (0.4), Change Seconds (0.6) and Diffusion (0.7: 0 = none, 1 = strong, up to 2). Type Share now only sets INITIALISING... and the title-only pages. The scene was not changed, so it uses these defaults. **Made without the owner:** the times, 0.4 s and 0.6 s. The Diffusion strength, 0.7. The shared left edge. **Files:** `StarwolfIntroController.cs` (`flickerInSeconds`, `changeSeconds`, `diffusion`; the shared left edge). `PhosphorDisplay.shader` (`settled`: the flicker in and the change; the sideways slip, the smear and the stronger grain) | **Done**. Checked in play mode. Frames were saved in `Logs/Intro` (`sheet.png`, `grain/sheet.png`, `grain07/sheet.png`). QA agent finding fixed: the intro clock now starts when the picture shows, and a slow first frame can no longer skip the quick flicker-in (in play, frame 5 was at 0.08 s). No person has seen it moving yet |
| 175 | A and D turns start smoothly (owner 2026-11-09, after playing: "turning moves in steps instead of smoothly in the very, very beginning if you press A and D"). **What the player sees:** press A or D, and the bow starts to turn at once. It turns from where it is. It moves at a steady rate. There is no jump and no snap back. Keep the key down, and the bow keeps turning with no stop. After half a second the key repeats, as before. A quick tap still turns 3°. A press a little longer than a quick tap turns a little more than 3°. That is new. The railgun lock's turn, the Shift slow turn and the flat ship in the targeting computer are unchanged. **What was wrong:** two faults, both in the first half-second of a turn. First: the moment you pressed A or D, the ship's picture jumped up to 4° at once. The ship's rules move it 2.4 times a second. Between moves, it is drawn gliding. The glide was counted from the last move, and the turn order came in partway through. Also, at every move the bow snapped back by the part of a degree it had already turned. Second: a tap orders 3°. The bow reached it and stopped. A held key only starts repeating after half a second. So a held key turned the ship, stopped it, then turned it again. **The fix:** a turn now starts from the moment of the order. The drawn bow counts the part of a degree already turned. While A or D is down, the next 3° are ordered the moment the drawn bow reaches the last ones. The lock's turn, the slow turn and the ship in the targeting computer use the same turning code. **Made without the owner:** a press a little longer than a quick tap now turns more than 3°. **Files:** `NavSystem.cs` (`DrawnHeading` now counts the part of a degree already turned, `turnBank`; `StepOrderedHeading` starts a new turn from the moment of the order). `HelmPanel.cs` (`Update`: while A or D is down, the next step is ordered as the drawn bow reaches the last one) | **Done**. Compiled with no errors. Played in the editor on the board (02.1_Grid), ENG 1 (10° a second). The test pressed D in the middle of a move and held it for 1.5 seconds, the same way the key does, then let go. The bow moved on the first frame after the press. It kept to a steady 10° a second, never more than 0.12° off the steady line. The biggest single-frame change off the line was 0.08°. It never went backward. It stopped cleanly at 18°. Full playtest not run since. No person has played it. A person needs to press the real keys to feel it |
| 177 | SECTION 36 and STARWOLF centred and sized to each other (owner 2026-11-09: first asked for the two words to be "sized to each other", then: "fix everything. Make sure nothing is slipped. Film a video if needed."). Fixes Step 174's off-centre words. **What was wrong:** to line the S's up, Step 174 gave both words one left edge. So SECTION 36 was 36 px right of the middle and STARWOLF 36 px left of it, and the box's right side moved in during the change-over. **What the player sees:** in the full intro (`00_Start`), both words fill the same box, centred on the screen. STARWOLF is narrower (120 squares to SECTION 36's 136), so its letters are spread out to fill the box. The gaps between its letters grow from 4 squares to 6 or 7. The S's still line up. The box stays still during the change-over: only the letters inside change. **Not changed:** INITIALISING... and the title-only pages. They keep normal spacing, in the middle. The flicker times and the Diffusion are the same as Step 174. **Made without the owner:** STARWOLF's letters spread out to match SECTION 36, rather than SECTION 36 squeezed in. The extra space is shared as evenly as it can be between the gaps. **Files:** `PhosphorData.cs` (`Cells` and `MakeMask` take a width to fill; the extra squares go between the letters). `StarwolfIntroController.cs` (on start, the full intro sizes the two words to each other; the left edge is now half the box's width). `PhosphorDisplay.shader` (`content` draws both words in the middle; `_IdentX` and `_TitleX` removed; the "potentially uninitialized variable" warning is gone) | **Done**. Checked by filming the real intro with the Unity Recorder: `Logs/Intro/video/intro.mp4` (30 fps, 2112 x 1188, 22.6 s), with picture sheets beside it (`sheet_in.png`, `sheet_change2.png`, `sheet_cut.png`). In the video the lit area is centred to within 3 px. The change-over runs from frame 143 to 166 (about 0.75 s, smear included). STARWOLF then stays 119 frames (4.0 s), with a clean cut to the menu at frame 262. The video runs slightly ahead of the intro's clock: the intro counts real seconds, and Unity ran a little faster than 30 fps while filming. That comes from the filming, not the game. Step 173 was also checked in play (see its row). No person has seen it moving yet |
| 178 | Story missions replace the test levels. Mission 1, ORIENTATION (owner 2026-11-09: "Too many levels, and they're not very good." The game will be about 10 story missions, "the Hunt for Red October in space", plus one SKIRMISH mode. The old test levels come off every list. What each one tested goes into the story missions. Mission 1 is the first: a combat briefing with your battle group, then an orientation checklist. Each part of the checklist you finish opens more of the map. What each mission needs is copied out of the old level scenes. Owner, later the same day: never delete the old levels, only switch them off). **What the player sees:** the title menu reads NEW GAME, SKIRMISH and SETTINGS. SKIRMISH is the old LEVELS row. It plays Wolf Hunt straight away: chase and kill the Garuda (`Level_01`). The title's block letters got a K for it. The LEVELS list (`01_Briefing`) can no longer be reached. NEW GAME, then SAVE SLOTS, then the SECTOR MAP now shows one dot: 1 ORIENTATION. Its briefing box reads: "COMBAT BRIEFING. Commander, welcome to the battle group. INDRA, VARUNA and MARUT are with you. A quick mop-up patrol. Standard thing. Before we move, run the orientation checklist: flying, sensors, power. Each part you finish opens more of the patrol area. Then take point to the target zone." In a skirmish, MISSION SELECT on the end screen and on the pause menu goes back to the title. In the campaign it still goes to the ship's rooms, then the sector map. **In ORIENTATION:** weapons are off. The map starts 10,000 x 10,000 units. Our ship starts near the bottom, facing north. Our battle group is round it: three ships, all facing north. INDRA · CARRIER is to port, VARUNA · BATTLESHIP to starboard and MARUT · CORVETTE behind. Before a scan, each shows as an amber dot labelled BOGEY F1, F2 or F3, like any unknown ship. After an ACTIVE SCAN passes over it, its name shows in blue, and its dot turns blue. Zoomed in past x1.1, the close-up shows them as full 3D models. They can't be targeted. They are solid, like every ship. **The battle group flies with us** (owner, later the same day: "The ships will fly around you and with you. You can deviate from them, but basically, they fly in a loose formation with you. If you fall off, then you have to catch up back to them... They're not sticking to you." And then: "they're not supposed to follow you. They pursue their path, and you have to sort of follow them."). The group flies its own path, the level's **Group Route**, at 15 units/s, and waits at its last point. It never follows us. Our place is the middle of the formation: the centre starts on our ship. Each ship flies to its place round the centre, turned the way the group flies, and drifts gently round that place. In ORIENTATION the path runs north from the start to (0, −1,000), north-west to (−1,500, 1,000), then to (0, 2,800) at the south edge of the rock field. The THROTTLE note says "The group is under way. Keep station in the middle of the formation." From TAKE POINT on, we leave them there and go through the rocks alone; the note says "The group holds south of the rock field." The ships steer round rocks and ships the way the enemy does, and they stay inside the battle space. **The checklist:** 16 steps. Each is a note from COMMAND in the briefing box, with the same box and chime as FLIGHT DRILL. Each note's title starts "COMMAND ·". In order: power, throttle, heading, the two side thrusters, zoom out, zoom back in. Then PATROL AREA OPEN: the map grows to 16,000. Then sensors, ACTIVE SCAN, and a SENSOR CHECK: scan one ship. Then TAKE POINT: the map grows to 24,000, and a ring shows the target zone to the north. Then more power, full throttle, the rock field, and the target zone: stop inside its ring. Then the mission is won. The end screen says CHECKLIST COMPLETE. It said FLIGHT DRILL PASSED before. Every step is in section 2, under `Orientation`. **The map grows:** when a step opens more of the map, the map grows from the middle to its new size, square, over 3 real seconds. The grid draws out to the new edge as it grows. The edge rule is unchanged: a warning near the edge, then 30 seconds to turn back. **How it works:** ships can now be placed in a level scene. Each one joins the world when the level loads (`TacticalWorld.JoinLevelShips` → `Join`). New **Friendly** tick box on `WorldContact`. New `friendly` flag on a board track (`ShipState.Track`). A friendly ship's name and class come from its object's name: `Indra · Carrier` shows as INDRA, CARRIER. `PlotPanel` colours a scanned friendly ship in the palette's `ownForce` blue. New **Battle group** settings on `EnemyBrain`, on the Board tab of **Starwolf → Tuning** under Battle group: **Escorts** (on or off), **Slot Units** (its place: x to our right, y ahead), **Group Speed Units** (15), **Sway Units** (150), **Sway Seconds** (60), **Keep Clear Units** (500), **Make Room Share** (0.3), **Catch Up Per Unit** (0.05) and **In Place Units** (100). On `TacticalWorld`, under Crashes: **Touching Units** (50). All are logged in `docs/TUNING_LOG.md`. With Escorts on, the brain flies the ship in the formation (`EnemyBrain.Escort`). The formation's centre starts on our ship and flies the level's `Mission.groupRoute` at Group Speed, then waits at the last point; with no route it stays put. Each ship flies to its Slot round that centre, turned the way the group flies, and drifts by up to Sway Units round it over Sway Seconds. It keeps Keep Clear Units from our ship, and goes round us to reach its place; closer than that, it moves away first. (The first film run stuck: as we turned north, INDRA crossed our bow to its new place, and the two ships pinned each other.) The second run found the same with rocks: INDRA and VARUNA touched rocks at the edge of the rock field, found every way blocked, and kept pushing. `TacticalWorld.ClearHeading` (the enemy's steering too) now heads straight away from whatever a ship touches when every way is blocked, then looks again. The third run showed a leash idea failing: the group stopped 3,000 units back and was stranded at the rock field when we took point at full speed. The leash was removed. The fourth run: the carrier and the battleship could not get through the rock field (rocks about 700 apart; they need about 900) and paced along its south edge, so their path ends south of the rocks. Runs three to five had the group head for each checklist ring and follow us between rings; the owner then said they must not follow us, so the group now flies its own route and we follow them. It steers with the enemy's steering (`ClearHeading`, `KeepInside`). `WorldContact`'s Friendly tooltip now says its brain should have Hunts off and Escorts on. A drill step has a new **Map Units** box (`DrillStep.mapUnits`). When the step starts, the map grows to that size, square. At 0 the map stays as it is. The grow time is **Map Grow Seconds** (3), on the Mission part of **Starwolf → Tuning**. New drill check SCANNED: that many ships scanned (named). A won flight drill's end screen line is now CHECKLIST COMPLETE, in every level with the Flight goal. **The mission scene:** `Assets/Scenes/Levels/Orientation.unity`, copied from `FlightDrill`, then changed. It is in the build list. `Player Spawn` at (0, −3,000), facing north. The Garuda is parked at (0, 30,000), far outside the map. It never shows. The rock field is moved to (0, 5,000). The three ships are under `Battle Group`. INDRA · CARRIER at (−700, −2,900), 450 x 230 units, prefab `Assets/Prefabs/BattleGroup/Indra.prefab`, stats `Assets/Ships/Carrier.asset` (Indra-class carrier), model `StarWolf_Carrier_mesh`. VARUNA · BATTLESHIP at (700, −3,050), 400 x 190, `Varuna.prefab`, `Assets/Ships/Battleship.asset`, model `StarWolf_Battleship_mesh`. MARUT · CORVETTE at (0, −3,700), 120 x 80, `Marut.prefab`, `Assets/Ships/Corvette.asset`, model `StarWolf_Corvette_mesh`. Each is a copy of the board's `Frigate (template)` with a different model and stats file. Friendly is ticked, Hunts is off, Escorts is on, and it never fires. Its Slot Units: INDRA (−700, 100), on our left. VARUNA (700, −50), on our right. MARUT (0, −700), behind us. The sector map's Missions list has one mission, ORIENTATION, with the briefing above. The old levels' scene files and `01_Briefing` are kept, switched off, never deleted (owner). The story and where each old level goes are at the top of `docs/FTUE_PLAN.md`. **The playtest** (`PlaytestRunner.cs`): the title's second row check now expects SKIRMISH to open the board on `Level_01`. MISSION SELECT outside the campaign expects the title. The campaign check wins "Orientation" and expects the map to still show one dot. **My calls, for the owner to veto:** the ship names INDRA, VARUNA and MARUT, and their sizes. The mission name ORIENTATION. The battle group shows as BOGEY until scanned: the scan step teaches naming a ship. The battle group's numbers: Group Speed 15, Sway 150 over 60 s, Keep Clear 500, each ship's place, and the group's path (it ends at the south edge of the rock field). The map edge stays as it is in every level: a warning, then 30 s. Weapons are off in mission 1. Weapons come in mission 2, the pirates. **Files:** `TacticalWorld.cs` (`JoinLevelShips`, `Join`). `WorldContact.cs` (`friendly` and its tooltip). `EnemyBrain.cs` (the Battle group settings, `Escort`). `ShipState.cs` (`Track.friendly`). `PlotPanel.cs` (a scanned friendly ship in `ownForce` blue). `Mission.cs` (`DrillStep.mapUnits`, `DrillCheck.Scanned`). `QuestSystem.cs` (`GrowMap`, `mapGrowSeconds`, the SCANNED check). `EndScreenPanel.cs` (CHECKLIST COMPLETE; Menu Scene is now `00_Start`). `End Screen.prefab`. `00_Start.unity` (the SKIRMISH row). `glyphs.json` (the K). `SectorMap.unity` (the Missions list). New `Orientation.unity`. New `Indra.prefab`, `Varuna.prefab` and `Marut.prefab`. New `Carrier.asset`, `Battleship.asset` and `Corvette.asset`. `EditorBuildSettings.asset`. `PlaytestRunner.cs` | **Done** — checked in play, 2026-11-09. A script flew the whole mission on the board by setting its controls (power, throttle, heading, Q and E, zoom, DRADIS, ACTIVE SCAN, the cone). Nobody pressed keys. All 16 steps passed in order, and the end screen said MISSION COMPLETE, CHECKLIST COMPLETE, our ship at 100% armour. The three ships joined with their names. At x1.3 they showed as 3D models. The scan turned INDRA and MARUT blue, and VARUNA stayed BOGEY F2. The map grew to 16,000, then 24,000, while zoomed out. Step 16 first stuck: the script held the ship in the ring at 2.5 speed, and the step wants 2 or less. Throttle 0 in the ring passed it. That was the script's flying, not the game. The title draws SKIRMISH with the new K, and SKIRMISH opens Wolf Hunt on Level_01. The sector map shows one dot, 1 ORIENTATION, and the combat briefing fits. No errors in the console. The film is `Logs/Orientation/Starwolf - Mission 1 Orientation.mp4` (5 min 46 s, the hover in the last ring cut). That film was made before the battle group flew. The battle group's flying was checked in play: while our ship flew east at 20 units/s, the three ships stayed 450 to 1,200 units from us. With our ship moved 7,000 units away, they stayed near where they were. Brought back, they caught up within about 10 seconds. Full mission film, sixth run (the first five found the pinning, the stranding, the rock field and following us, above): all 16 steps passed; the group flew its path north and our ship kept station in the middle, 500 to 1,000 units from each ship, through the checklist; from TAKE POINT we went through the rocks alone while the group went on to the field's south edge; our ship scraped one rock, so the end screen read 94% armour; 6 min 30 s, `Logs/Orientation/Starwolf - Mission 1 Orientation.mp4`, with `run.log`. The full playtest has not run since this step. |
| 179 | The 3D models follow the Length number (owner 2026-11-09: "Make the three models for the number", then: "3D models should be scaled uniformly to follow the length number."). **What the player sees:** nothing new at today's numbers. The models were already their Length, scaled by hand in Step 164: our Asura 200 units, the Garuda 350, the frigate 100. **What changed:** each ship's 3D model now sizes itself to its Length when a level starts. Its longest side, seen from above, is the Length. It is scaled evenly, so the width grows or shrinks with it and the shape stays the same. Change Length during play and the model follows at once. Length is at the top of the Ship tab (our ship) and the Enemy tab of **Starwolf → Tuning**. **Width does not change the model.** It still only sets the width of the hit box. The model's width follows its Length, in the model's own shape. **The line pictures:** the camera that films each ship's line picture follows Length too. How much it frames, and how far back it is, come from the Length. So the line picture stays whole at any Length. **Removed:** Height on the Board tab of Starwolf → Tuning, under Plot & our ship (how far above the ship that camera was). It is now worked out from Length. The old values set by hand in `02.1_Grid` are no longer used. **New ships:** no scaling by hand. Set Length in the ship's stats file in `Assets/Ships`. **Made without the owner:** the camera's distance (twice the Length) and how deep it sees. The model grows or shrinks about the ship's centre. **Files:** `Ship.cs` (`FitModel`, `WidthShare`, a `LateUpdate` that fits the model again when Length changes). `ShipView.cs` (frame and camera from Length; Height removed; `WidthShare` now comes from `Ship`). `StarwolfTuningWindow.cs` (a Length edit reaches every ship on that file) | **Done**. Compiled with no errors. Checked in play on Wolf Hunt. Our ship's Length went from 200 to 300 during play. The model became 300 units long, in the same shape. In the close-up at x2 it showed 1.5 times bigger. Zoomed out to x0.8, the line picture was 300 units long (96 px, against the grid's 32 px per 100 units) and whole. Then our ship at 120 and the Garuda at 500: both models matched, and their cameras followed. A frigate arriving as a copy of the template sized itself to 100. The width-to-length share did not change (our ship 0.639, Garuda 0.499). No errors in the console. The test did not change the stats files. Screenshots: `Logs/ShipLength/01_length200_x2.png`, `02_length300_x2.png`, `03_length300_lines_x08.png`. A QA agent then read the code and found no problem in it. It found one gap next to it: a Length edit in Tuning reached only that one ship, so a second frigate on the same stats file kept the old size. Fixed: the edit now reaches every ship on that file, as the Hard Points tab already did. Compiled with no errors. Full playtest not run. No person has played it. Next: the owner picks which ship gets a new length, and what length |
| 181 | The loading screen gets corner text and clearance squares, the font gets every letter and number, and the green gets richer (owner 2026-11-10, answering a page about the look of the screens before the board: "Yes, add the new letters to the game font… Mission names and numbers will need them." On the 5 keeping the cut right corners that the B and D already have: "I don't mind." On the loading screen: "small text on the screen and clearance squares… dim, in Share Tech Mono." On colour: "keep the game's, but the references had a lovely, slightly richer green. If you can get to that effect, I'll be happier."). **What the player sees:** on every green dot screen (SECTION 36, STARWOLF, the menu, the page titles, INITIALISING), each lit square is pale green in the middle and fades to a deeper green at its edge. The glow round the letters is pure green and stronger, with a wider faint halo. Before, each square was one flat pale colour, and the glow was the same pale colour, which looked grey-green. On INITIALISING, once Corner Text is ticked, small dim text in Share Tech Mono shows in the four corners of the picture. Top left: UC-FLEET, and // CLASSIFIED below it. Top right: S36-PR-001, REV A and 1993.11.04 (the reference says S38; the owner picked S36 to match SECTION 36, 2026-11-10). Bottom left: RESEARCH / DEVELOPMENT / OPERATIONS. Bottom right: CLEARANCE LEVEL 3, with five small squares under it. The first three are lit, the other two hollow. The text stays in the corners of the 16:9 picture at any window size. Mission names can now use every letter A to Z, the numbers 0 to 9, the colon and the dash. No screen uses the new characters yet. **The font:** 15 new characters in `glyphs.json`: J, Q, X, Y, Z, 0, 1, 2, 4, 5, 7, 8, 9, the colon and the dash. The font now has A to Z, 0 to 9, the full stop, the colon, the dash and space. All pass the script's own check of two-square strokes. The 5 has the cut right corners of the B and D at its middle and bottom. The S has square corners there. **The richer green:** measured from the reference picture `References/Company Logo.png` against the game. The middles of the lit squares were already the same: reference (161, 252, 174), game (160, 246, 158). The edges were the difference. The reference fades to (60, 145, 73) at each square's edge. The game had a hard edge with no change of colour. Now it fades to about (63, 147, 67). Past the strokes, the reference shows a faint glow, (0, 25, 1). The game showed the bare background there, (0, 16, 0). Two new colours in `glyphs.json`: `rim` #3CA050 (the square's edge) and `glow` #1AFF2E (the glow). The three glow strengths are now in the shader's Properties: **Glow next to a square** 0.02 (was 0.015, written in code), **Glow a little further out** 0.012 (was 0.009) and **Wide halo round the strokes** 0.003 (new). Nothing in code writes them. The new green was tried on a copy outside Unity first. **The corners:** new **Loading screen corners** fields on `StarwolfIntroController`: a **Corner Text** tick box, the words for each corner, **Clearance Lit** (how many of the five squares are lit, 0 to 5, default 3), the font (`ShareTechMono-Regular SDF`), the material (left empty: the font's own material), the colour (#73E18A at 80%) and the size (24). With the box ticked, the script makes four texts in the corners. They are laid out on the 1920 × 1080 screen and scaled with it. The shader draws the clearance squares, only on INITIALISING and only with the box ticked. The box is off by default. **Made without the owner:** the corner words and their places, copied and measured from `References/Company Logo.png`. The shapes of the new characters, the 5 included. The colour values, measured from the same picture. The corner text size 24 and its colour, #73E18A at 80%. **Files:** `glyphs.json` (15 characters, `rim`, `glow`). `PhosphorData.cs` (reads and checks `rim` and `glow`). `PhosphorDisplay.shader` (new Properties `_Rim`, `_Glow`, `_Corners`, `_ClearanceLit`, `_GlowNear`, `_GlowFar`, `_GlowWide`; the square edges; the pure green glow and its wide halo; `clearance()` draws the squares). `StarwolfIntroController.cs` (the Loading screen corners fields, `BuildCorners`, `Corner`; writes the two colours, `_Corners` and `_ClearanceLit` to the shader). `docs/TUNING_LOG.md` (six rows). `Assets/StarwolfIntro/README.md` | **Done.** Compiled outside Unity and in Unity: no errors. Corner Text is on both loading screens: `INITIALISING` in `01_Briefing`, and `INITIALISING` in `SectorMap`, which shows after the campaign's LAUNCH. The `SectorMap.unity` change goes in the Campaign Manager chat's commit, as agreed with it. The corner text uses the font's own material. The Phosphor Glow material made it too thick: 18% of the corner area lit, against the reference's 11%. Now 10%. Top right reads S36-PR-001: the owner's pick, to match SECTION 36 (the reference says S38). The owner said to leave the green as it is. Filmed the real path in play mode: the intro, NEW GAME, save slot 1, the sector map, LAUNCH, INITIALISING with the corner text, then the board (`Logs/Intro/v181/real-path.mp4`). The film re-saved the owner's slot 1 save; it was put back from its backup. Test runs use a hidden slot from now on. Full playtest not run. Nothing left to do. |
| 183 | The campaign's missions are in one file, the Campaign file (owner 2026-11-10: "Is there something we can drag those things into?", then, on the plan: "that's absolutely fine"). **What the player sees:** nothing new. The sector map shows the same one dot, 1 ORIENTATION, with the same briefing. **What changed:** the list of missions moved out of the sector map scene into a new file, `Assets/Campaign/Campaign.asset`. It is the one Campaign file. It has the missions in play order. Each mission has a name (**Title**), a **Level** slot, a briefing (**Brief**, the text on the map's left screen) and its dot's place (**At**, in px from the middle of the 1920 x 1080 screen). Drag the level's scene into the Level slot. The slot keeps the scene's name. It also adds the scene to the build's scene list, so the game can load it. Today the file has one mission, ORIENTATION, carried over exactly from the sector map scene: the same name, level, briefing and dot place. The Sector Map Script in `SectorMap.unity` has a new **Campaign** slot, with this file in it. The scene's own Missions list is gone. **Unchanged** (owner restated it on 2026-11-10): a mission's dot shows on the map only after the mission before it is won. **How to set up a mission now:** select `Assets/Campaign/Campaign.asset` in the Project window. In Missions, add a mission. Type its name and briefing. Drag its level scene into Level. Set where its dot goes. The level itself is still made in its own scene in `Assets/Scenes/Levels`, with **Starwolf → Level** and the Mission part on it: its checklist steps, rings, help text and battle group path. **Not built yet, planned on top of this file:** the end card's closing line; CAMPAIGN COMPLETE and the percent on the save slot; the Mission Setup Tool chat's work (call signs, who speaks, a line for each checkpoint, a Starwolf → Mission window). No Starwolf menu item was added. **Files:** new `Assets/Scripts/Game/Campaign.cs` (the Campaign file and its missions). `Assets/Editor/ReadOnlyDrawer.cs` (new `SceneDrawer`: the Level slot you drag a scene into). `SectorMap.cs` (reads its missions from the Campaign slot). `SectorMap.unity` (the Campaign slot filled; the Missions list gone). New `Assets/Campaign/Campaign.asset`. `docs/TUNING_LOG.md` (a row: moved, not changed) | **Done**. Checked in play with a spare test save, deleted afterwards: the map showed "1 ORIENTATION" and the briefing on the left screen. Full playtest not run since. No person has played it |
| 187 | Every note moved into `docs/` (owner 2026-11-10: "can you please stop saving random READMEs in random places? Why does the scene folder tactical have READMEs in there?"). **What the player sees:** nothing new. **What the owner sees:** the Tactical scene folder and the Assets folder have no notes in them now. The 8 notes from `Assets/Scenes/Tactical`, the 4 from the top of `Assets` and the board guide are in `docs/`, with the same names. The board guide is now `docs/BOARD_UI.md`. Every link to them points to the new place. Rule from now on: never make a note inside `Assets`. Add to a note that exists, or put a new one in `docs/`. Two READMEs stay where they are, because each one describes the files next to it: the intro package (`Assets/StarwolfIntro`) and the design export (`Assets/UI/Board/Design`). |

**Old status notes, corrected (2026-11-01).** The table and section 6 keep what was true when each
step was written. These notes have changed since:

- Step 17 "keys not": W/S were built in Step 106, and the owner played the app.
- Step 27 "CRT treatment not yet seen": the flat intro of Steps 56 and 70 replaced it. The same goes
  for "Not yet seen on screen" in its write-up.
- "Full run still to re-run" or "playtest not run" on Steps 69, 71, 110, 112, 115, 116, 119, 131, 133,
  134, 135, 136 and 139: the full run `2026-11-01_194534` came after all of them. Its #216 to #222
  cover the grid.
- Step 118 "not run": report #16 passed.
- Step 124 "no fight played with the bigger ship": fights were played in Step 130.
- Step 125 "that one check has not been run again": it passed in run `2026-11-01_194534`, as #221.
- Step 126 "wider than the box": the owner said in Step 127 the picture needn't match.
- Step 128 "no shot fired in the running game": Step 132 hit from the gun's dot. No ship file has the
  old Hit Margin Units line now.
- Step 129 "playtest not run": #223 to #225 ran. #225 read 0 to 0, so it proves nothing yet. "Map not
  checked by eye": Step 136 used the real window.
- Step 130 "a newer designer brief is waiting": built in Steps 135 and 139.
- Step 138 "not yet run": report #107 passed. "Full playtest not run": run `2026-11-01_194534` came after it.
  "Not fired through the real switches" and "hit report was not read": Step 143 did both.
- Section 3, the `PlotPanel` line, and section 4, decision 40: since Step 138, "where the sweep last touched
  it" has two exceptions, a locked ship and a scanned ship under the ACTIVE SCAN cone.
- Step 119's screenshots in `Logs/LevelPlay` and Step 138's in `Logs/SmoothMotion` were deleted on 2026-11-01
  (owner). The numbers in those rows stand.
- Step 141 has no "Done" in its status. It is built and checked.
- Section 6, Step 63, the throttle trap: Step 64's rule replaced it. Lose power, and the throttle
  goes to zero and stays there (`NavSystem.Steer`).
- Section 6, doc check of Steps 56 to 66: the heading "Bugs, not fixed yet" no longer exists.
- Section 6, Step 105's questions: answered and built in Steps 106 and 107.
- Section 6, Step 109, the white reticle: the owner said on 2026-11-01 that the white corners stay.
- "Missile" in older steps and sections means torpedo (owner 2026-11-02, Step 149). The old text keeps
  the word it was written with.
- Step 147, BEARING OUTSIDE LAUNCH CONE and the open question: Step 149 removed the message and closed
  the question.
- Steps 147 and 149, "have not been run", "Full playtest not run" and "has not been reported yet": the
  full playtest of 2026-11-02 checks both steps. It passed 245 of 245 (run `2026-11-02_005126`; #127,
  #145 and #146).
- Step 85, in the table and in section 6, MAIN MENU: since Step 154 the button says MISSION SELECT.
  It loads `01_Briefing`, the MISSIONS list.
- Step 92, in the table and in section 6, ruins inside the DRADIS circle while DRADIS is on: Step 157
  replaced it.
- Section 6, Step 41, "There is no reverse" and "0–100%": Step 157 added reverse.
- Steps 148 and 151, false dots and the enemy's false look: Step 157 removed them.
- Step 129, a cloud on the plot as a faint soft box: Step 158 replaced it. In visual range a cloud now
  shows as a cloud picture. Past it, with DRADIS on, it shows as a hollow circle.
- Step 44, in the table and in section 6, the bearing line from our ship: Step 161 replaced it. A heard
  ship that is not scanned is now a short red bar on the DRADIS circle. The same goes for the bearing
  line in the Step 75 write-up. Step 75's rule that its bearing stays put until the next sweep no
  longer applies: the bar and its BRG turn on every frame.
- Step 45, in the table and in section 6, the bearing trail: Step 161 deleted it. ERASE no longer
  clears a trail. Step 105's question 2, Step 106's "cross the bearing lines" and the playtest shot
  `23_bearing_trail` describe the old board.
- Step 144, in the table and in section 8, "13 steps": Step 163 added an ACTIVE SCAN step. FLIGHT
  DRILL has 14 steps now.
- Steps 114 to 116, the WEAPONS FREE and NO PEEKING briefings: Step 163 changed them. They now say to
  find the red bar on the DRADIS circle. They no longer say to wait for a bearing line or to cross the
  bearing lines.
- Steps 106 and 157, and section 6, Step 106, W and S step the throttle 10 (Throttle Key Step): Step
  171 replaced it. A tap moves the throttle 1. Held, it counts faster and faster.
- Step 144, in the table and in section 8, "let go, and the thrusters stop the slide": Step 171
  replaced it. Let go, and the slide keeps going and fades. A tap of Q or E is now a small push.
- Step 167, in the table, the sector map's stand-in look: the title SECTOR MAP, the faint green grid,
  the box with the briefing line and LAUNCH, and the dimmer dot with a ✓ after its name. Step 172
  replaced it. The box now shows the newest dot as soon as the map opens.
- Steps 167, 172 and 178, in the table, the Missions list on the Sector Map Script in
  `SectorMap.unity`: Step 183 moved it into the Campaign file, `Assets/Campaign/Campaign.asset`. The
  scene no longer has a Missions list.

Steps 6 to 28 and Steps 34 to 118 are written up in section 6. Steps 29 to 33 were written up in `UI_INTEGRATION_LOG.md` (deleted; see below). What is still not done is in **Open items**, at the very
bottom of this file. Read that before you pick up any work.

**Old docs deleted (owner, 2026-10-27).** These described deleted code, the old `02_Grid` or the
old cockpit game: `Assets/DOCUMENTATION.md`, `Assets/STAKEHOLDER_DOCUMENTATION.md`,
`Assets/TACTICAL_BOARD_PLAN.md`, `Assets/PARITY_PLAN.md`, the two READMEs in
`Assets/Scripts/Weapons`, and in `Assets/Scenes/Tactical`: `VERIFICATION.md`, `ASSET_AUDIT.md`,
`TARGETING_AUDIT.md`, `TARGETING_COMPUTER_SPEC.md`, `UICONTROLLER_PARTS.md` and
`UI_INTEGRATION_LOG.md`. This log still names them in old steps. To read one, use git:
`git show 12ccdd1:Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md`.

---

## 2 · Scenes

The intro, briefing and board scenes are in `Assets/Scenes/Tactical/`. Levels are in
`Assets/Scenes/Levels/`. The play order is `00_Start` → `01_Briefing` → `02.1_Grid`, with a level
loaded on top of the board. Build Settings list nine scenes: `00_Start`, `01_Briefing`, `02.1_Grid`,
`Level_01`, the training levels `FirstShot`, `SilentShot` and `Prize` (Step 114), `Intercept` (Step
137) and `FlightDrill` (Step 144). Prototype Dev came off in Step 84, and `Level_02` on 2026-11-01.

| Scene | GUID | State |
|---|---|---|
| `00_Start.unity` | `e50db2cd323304012ba3e7afb4e531cb` | Built |
| `01_Briefing.unity` | `326d3f3159fcb4965bebcf61495595c6` | Built |
| `02.1_Grid.unity` | `db225074494dd4259bc8aab05e48399e` | Built — the board. In the play flow after `01_Briefing` |
| `Levels/Level_01.unity` | `4cf76e81d39984c4db973e752f8bef63` | Built — Wolf Hunt, loaded on top of `02.1_Grid` |
| `Levels/FirstShot.unity`, `SilentShot.unity`, `Prize.unity` | (see their `.meta`) | Built in Step 114 — the training levels WEAPONS FREE, NO PEEKING and FINDERS KEEPERS |
| `Levels/Intercept.unity` | `d5a257d6a5fd342c895b7ae05b7bdaa2` | Built in Step 137 — INTERCEPT |
| `Levels/FlightDrill.unity` | `64fcc2628baee492ca931c555bdaaa24` | Built in Step 144 — FLIGHT DRILL |
| `Levels/LongShot.unity` | `133d1996e814e4006bb14050901900a4` | Built in Step 170 — LONG SHOT, the test range for the spinal railgun |
| `Levels/Orientation.unity` | `e8095fc79760047e4a1fa59c5faa65ba` | Built in Step 178 — ORIENTATION, mission 1 of the story |

### `00_Start` — built in Step 2

- `Main Camera` — orthographic, clears to `#0A0C0E`.
- `EventSystem` with a `StandaloneInputModule`. **This is not optional.** A Button does nothing at
  all without one in the scene.
- `Canvas` — Screen Space **Camera**, not Overlay. CanvasScaler on Scale With Screen Size,
  1920x1080, match 0.5.
- `Title` — TextMeshPro, reads "STARWOLF", 110pt.
- `StartButton` — an Image with a Button on it, and a child `Label` reading "START GAME".
- `Title` and `StartButton` are switched off.
- `STARWOLF Intro`: the intro and menu. NEW GAME calls `ScreenFlow.Advance()`.
- `ScreenFlow` — added in Step 4, see section 3.

### `01_Briefing` — built in Step 3

Same camera, EventSystem and Canvas setup as `00_Start`.

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
- What the package left out: `docs/HANDOFF_GAPS.md` (106 rows).
- What Claude Design must still provide, verified: `docs/CLAUDE_DESIGN_NEEDS.md`.

The setup from Step 28:

- `Main Camera` — orthographic, clears to `board.ground` `#0A0C0E`, with an `AudioListener`.
- `EventSystem` with a `StandaloneInputModule`.
- `Board` — Canvas, Screen Space **Camera**, plane distance 5, CanvasScaler 1920x1080, Screen Match
  Mode Expand (Step 87), reference pixels per unit 100. These are the handoff README's canvas numbers.

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
  - Both models are about 12–14 km long. Models only, no old scripts.
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

### `FirstShot`, `SilentShot`, `Prize` — made in Step 114

The training levels, on the list as WEAPONS FREE, NO PEEKING and FINDERS KEEPERS (Step 115). The numbers
are Step 130's. Each: `Battle Space` 20,000 × 20,000; `Player Spawn` at (0, −6,000) facing north;
`Enemy Spawn · Garuda` at a fixed spot (box 0 × 0), Hunts off; one `Debris Field`, a circle of 40 pieces;
a `Mission` box.

| Level | Goal | The Garuda starts | Debris |
|---|---|---|---|
| `FirstShot` | Destroy | (0, −4,500), 1,500 ahead, facing west, still | (−4,500, −2,000) |
| `SilentShot` | HitUnseen | (2,400, −2,800), 4,000 away, facing west, at 8 units/s | (−4,000, 1,000) |
| `Prize` | Capture | (−900, −4,800), 1,500 away, facing east, still | (4,500, −1,500) |

### `Intercept` — made in Step 137, moved onto COLD RUN's map in Step 148

The training level for real radio messages. A copy of `ColdRun`'s map with the radio on. `Battle Space`
20,000 × 20,000 with the S corridor; `Player Spawn` at (−7,500, −7,000) facing east; `Enemy Spawn · Garuda`
at (6,500, 6,800), box 0 × 0, Hunts on, 13 route points, Signal Seconds 60, First Signal Seconds 15, On
Air Seconds 60; a `Jump Point` at (7,500, 7,500); a `Mission` box with the goal JumpPoint and Weapons
Off on.

### `FlightDrill` — made in Step 144

The level that teaches flying. `Battle Space` 20,000 × 20,000; `Player Spawn` at (0, −8,000) facing
north; `Enemy Spawn · Garuda` at (−9,000, 9,000), box 0 × 0, Hunts off, Cruise Units 0, no messages. It
is 12,657 units from the last ring, and DRADIS reaches 7,500. One `Debris Field` at (0, −2,100), a Band
9,000 long and 2,000 wide, with 39 rocks. A `Mission` box with the goal Flight, the 14 steps below and
no help text.

The player flies north. Each step shows its own note and plays a soft tone. A step with a ring shows
the ring on the plot, labelled CHECKPOINT, with its bearing and distance on the alarm strip. The ring
is a circle: its size is its radius, in units.

| Step | The note's title | What ends the step | Ring: centre, radius |
|---|---|---|---|
| 1 | POWER FIRST | ENG on STANDBY with 1 to 3 units | None |
| 2 | THROTTLE | Inside the ring | (0, −7,550), 150 |
| 3 | HEADING | Inside the ring | (800, −7,500), 150 |
| 4 | SIDE THRUSTERS | `Q` down for 3 game seconds, with ENG above 0 | None |
| 5 | SIDE THRUSTERS | `E` down for 3 game seconds, with ENG above 0 | None |
| 6 | ZOOM | Plot zoom at x0.20 or less | (600, −5,300), 200 |
| 7 | ZOOM | Plot zoom at x0.80 or more | The same |
| 8 | MORE POWER | ENG at 5 | The same |
| 9 | FULL THROTTLE | Speed at 45 or more | The same |
| 10 | None | Inside the ring | The same |
| 11 | DRADIS | DRADIS on, with SEN power | None |
| 12 | ACTIVE SCAN (Step 163) | ACTIVE SCAN on, with DRADIS listening | None |
| 13 | ROCK FIELD | Inside the ring | (0, −600), 250 |
| 14 | LAST CHECKPOINT | Inside the ring, speed at 2 or less | (0, 100), 300 |

Steps 6 to 9 show step 10's ring, and reaching it doesn't end them. The notes, rings and amounts are
typed in the Mission box. The Level window's map doesn't draw the
rings, and its list doesn't show the Mission box: pick the `Mission` object in the Hierarchy. To lose
this level, lose the ship, as in any level (spec §14).

### `Orientation` — made in Step 178

Mission 1 of the story, ORIENTATION, and the one dot on the sector map. A copy of `FlightDrill`,
changed. Weapons are off. The map starts 10,000 × 10,000. `Player Spawn` at (0, −3,000) facing north.
`Enemy Spawn · Garuda` at (0, 30,000), far outside the map. The Garuda is parked there and never shows.
The `Debris Field` is moved to (0, 5,000). A `Mission` box with the goal Flight and the 16 steps below.

Under `Battle Group` are three ships of our battle group, all facing north at the start:

- `Indra · Carrier` at (−700, −2,900), 450 × 230 units. Prefab `Assets/Prefabs/BattleGroup/Indra.prefab`,
  stats `Assets/Ships/Carrier.asset`.
- `Varuna · Battleship` at (700, −3,050), 400 × 190. `Varuna.prefab`, `Battleship.asset`.
- `Marut · Corvette` at (0, −3,700), 120 × 80. `Marut.prefab`, `Corvette.asset`.

Each has Friendly ticked, Hunts off and Escorts on, and never fires. Before a scan each shows as
BOGEY F1, F2 or F3. Once scanned, its name shows in blue.

They fly their own path in a loose formation, and we follow them. Each ship's place (Slot Units,
x to the right, y ahead, turned the way the group flies): INDRA (−700, 100), VARUNA (700, −50),
MARUT (0, −700). Our place is the middle. The path (Group Route, on the Mission) runs from the
start to (0, −1,000), (−1,500, 1,000) and (0, 2,800), at 15 units/s; they wait at the last point,
south of the rock field, while we take point through the rocks. The
Battle group settings on `EnemyBrain` set this (Step 178).

Each step is a note from COMMAND, with the same box and chime as FLIGHT DRILL. Each title starts
"COMMAND ·". The table gives the rest of the title.

| Step | The note's title | What ends the step | The map grows to |
|---|---|---|---|
| 1 | WELCOME, COMMANDER / POWER | ENG on STANDBY with 1 to 3 units | |
| 2 | THROTTLE | Inside the ring at (0, −2,450), radius 150 | |
| 3 | HEADING | Inside the ring at (700, −2,250), radius 150 | |
| 4 | SIDE THRUSTERS | `Q` down for 3 seconds | |
| 5 | SIDE THRUSTERS | `E` down for 3 seconds | |
| 6 | ZOOM | Plot zoom at x0.20 or less. The note: "you'll see the group round you, and the edge of the patrol area" | |
| 7 | ZOOM | Plot zoom at x0.80 or more | |
| 8 | PATROL AREA OPEN | Plot zoom at x0.12 or less | 16,000 |
| 9 | SENSORS | SEN on DRAW, DRADIS on | |
| 10 | ACTIVE SCAN | ACTIVE SCAN on | |
| 11 | SENSOR CHECK | One ship scanned (its name shows in blue) | |
| 12 | TAKE POINT | Plot zoom at x0.08 or less. The target zone is north: step 15's ring shows at (0, 6,800) | 24,000 |
| 13 | MORE POWER | ENG at 5 | |
| 14 | FULL THROTTLE | Speed at 45 or more | |
| 15 | ROCK FIELD | Inside the ring at (0, 6,800), radius 250 | |
| 16 | TARGET ZONE | Inside the ring at (0, 9,000), radius 300, speed at 2 or less | |

When a step with a number in its **Map Units** box starts, the map grows from the middle to that
size, square, over 3 real seconds (**Map Grow Seconds**, on the Mission part of **Starwolf → Tuning**).
The grid draws out to the new edge as it grows. The edge rule is the same as in every level. After
step 16 the mission is won, and the end screen says CHECKLIST COMPLETE.

### `LongShot` — made in Step 170

The test range for the spinal railgun. A copy of `ColdRun`, changed. `Battle Space` 40,000 × 40,000;
`Player Spawn` at (0, −14,000) facing north; `Enemy Spawn · Garuda` patrolling east and west between
(−15,000, −5,000) and (15,000, −5,000) at its usual cruise speed, Hunts off; three gas clouds near our
start; ColdRun's four Big Rocks groups, moved; a `Mission` box with the goal Destroy and an eight-step
briefing. The numbers are in Step 170 in the table above.

### Deleted in Step 1

These were leftovers from an earlier attempt that was abandoned. They are gone on purpose. Do not
bring them back.

- `Assets/Scenes/Sandbox/` and `TacticalGrid_Test01.unity` inside it.
- The old `Assets/Scripts/Tactical/` and the three unfinished files in it: `Bearing.cs`, `Track.cs`,
  `TacticalRadar.cs`. (The folder exists again now, with the board's scripts.)
- `TacticalRadar.cs` was deleted rather than repaired. It referenced the old cockpit `UIController`,
  which must stay out of these scenes (see Decisions).

---

## 3 · Scripts, shaders and wiring

### `ScreenFlow.cs` — new in Step 4

The file is `Assets/Scripts/Tactical/ScreenFlow.cs`. It is small and reusable.

- Fields you set in the inspector: `nextScene` (the scene name, as text), `advanceOnEnter`
  (a tick box), `showFirst`, `advanceAfterSeconds` and `cards` (the title cards, shown in turn).
- One public method, `Advance()`, which loads `nextScene`.
- It has a `going` guard so it cannot fire twice.
- It logs an error if `nextScene` is left empty.

Where it is attached:

| Scene | On | `nextScene` | `advanceOnEnter` | How it fires |
|---|---|---|---|---|
| `00_Start` | Its own root object, named `ScreenFlow` | `01_Briefing` | off | NEW GAME calls `Advance()` |
| `01_Briefing` | Its own root object | `02.1_Grid` (was `02_Grid` until Step 28) | off | Step 114: it waits. A row on the level list calls `Play(level)`: it sets `TacticalWorld.chosenLevel` and shows INITIALISING, whose `finished` calls `Advance()`. (Until Step 114: every 3 s or on Enter, the next card.) |

The script is on **its own root GameObject**, not on the button or the canvas.

The chain was run in play mode twice in Step 4: `00_Start` → click → `01_Briefing` → advance →
`02_Grid`. Both times it worked. Since Step 28 the chain ends at `02.1_Grid` instead, also checked in
play mode.

### The board — new in Steps 29–33

Everything on the `02.1_Grid` board. Detail is in `UI_INTEGRATION_LOG.md` (deleted; `git show 12ccdd1:Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md`); do not copy it here.

| File | What it is |
|---|---|
| `Assets/Scripts/Game/BoardManager.cs` | The board script (was `HandoffBoardControls`). Runs the 0.42 s tick and the board's own clock, and takes every click (`Click`). |
| `Assets/Scripts/Systems/ShipState.cs` | One ship's state (was `HandoffBoardState`). It has the contact list, what the periscope has classified, the plot marks, the battle space, destroyed and captured, the EMP clock, the dice and kills. It reaches the ship's parts through `nav`, `power`, `sensors`, `comms`, `weapons`, `hangar`, `damage` and `orders`. |
| `Assets/Scripts/UI/Panel.cs` | The base class for every panel, with the Art list (`ApplyArt`) and the Fonts list (`ApplyFonts`). |
| `Assets/Scripts/UI/Panels/PeriscopePanel.cs`, `PlotPanel.cs` | The periscope and the plot. |
| `Assets/Scripts/UI/Panels/*Panel.cs` | The other eight panels: helm, power, sensors, comms, fire control, hangar, engineering, orders. Each shows its ship part and passes clicks to it. |
| `Assets/Scripts/UI/Panels/EndScreenPanel.cs` | The end screen (Step 85): how the mission ended, RESTART MISSION and MISSION SELECT. MISSION SELECT loads `01_Briefing`, the MISSIONS list (Step 154). It is the pause menu too (Step 156): Escape in a level opens it, titled PAUSED, with RESUME and QUIT GAME added. |
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

### The Tuning window and other editor files — Tuning new in Step 38, tabs in Step 80

| File | What it is |
|---|---|
| `Assets/Editor/StarwolfTuningWindow.cs` | Menu **Starwolf → Tuning**. Six tabs: **Ship** (our stats file), **Enemy** (the Garuda's), **Board** (every `[Tune]` value in the open scene), **Keys** (Step 108; `Q` and `E` are on it since Step 144), **Hard Points** and **Ranges**. |
| `Assets/Scripts/Tactical/TuneAttribute.cs` | `[Tune("Section")]`. Marks a board value (the plot, sounds, the clock) for the Board tab. Ship numbers don't use it. `[Tune("Keys")]` marks the board's keys. |
| `Assets/Editor/Playtest/PlaytestRunner.cs` | The playtest (menu **Starwolf → Playtest → Run power-up playtest**). See "The playtest" below. |
| `Assets/Editor/ReadOnlyDrawer.cs` | Draws `[ReadOnly]` fields in the inspector, greyed out. |
| `Assets/Editor/PowerManagerEditor.cs` | Soham's inspector for his old `PowerManager`. The board doesn't use it. |

### The playtest and the Game view

**Standing end-of-feature check — the playtest (working, 2026-10-17).** Run it after a big code
move, or once at the end of a batch (owner, 2026-10-27). For a small change, check only that
change in play mode. Menu **Starwolf → Playtest → Run power-up playtest** (`Assets/Editor/Playtest/PlaytestRunner.cs`).
It:

- goes through the user flow: intro → menu NEW GAME → briefing → board;
- sets up the ship: opens Power from the systems rail, sends ENG/WEP/SEN/AUX to DRAW, waits for fill;
- checks readings at each step, and fails a step if its control is blocked on screen;
- films the run with Unity Recorder (package `com.unity.recorder` 5.1.7, owner's choice).

Each run writes `Logs/Playtests/<date_time>/`: `playtest.mp4`, `report.md` (pass/fail per step)
and `shots/`. It leaves play mode and reopens the scene you were in. When a run starts, it deletes
every earlier run's `playtest.mp4`. Screenshots stay for the newest three runs. Every `report.md`
stays (Step 75). First good run: 18/18 PASS,
22 s of 1080p video. Since Step 35: 27/27 PASS, 35 s, and it also checks pause, RUN, signal and
LOAD SHED. Since Step 36: 37/37 PASS, 37 s, and it also drops a POINT, a RULER and a COMPASS, moves
our ship and checks the marks stay put. Since Step 40: 48/48 PASS. It also checks the click sounds,
the LOAD SHED slide, a refused locked rocker, and the TRACK and RECALL presses. Since Step 41: 57/57.
It also drives the helm (ENG power, throttle, a turn, pause) and pans the plot. Now 214
(Step 106); each step's count is in the table in section 1. It grows as features land: add steps to `PowerUpRun()`. There is only one
Unity editor, so it runs when no other agent is using Unity.

What it can't do yet: the intro menu and the briefing's Enter read Unity's old Input class, which
a script cannot press, so it calls what those presses call (`Activate(0)`, `ScreenFlow.Advance`)
and says so in the report. Board controls are clicked for real, through the EventSystem.

Traps found setting it up: the recorder needs a moment after it stops before play mode ends, or
the MP4 has no index and won't play; and it must record at a **constant** frame rate — variable
rate fed the Mac encoder duplicate timestamps and it dropped frames. The playtest films the Game
view at its own 16:9 size (owner, 2026-10-26). MP4 needs an even width and height, so an odd window
is trimmed by one pixel. The playtest notes the Game view's size choice before the run and puts it
back afterwards.

**Game view size (owner, 2026-10-18).** Use **16:9 Aspect** in the Game view's size menu (top
left of the Game view). The board draws at the window's real pixel size, and its layout is designed
for 16:9. **Free Aspect** fills any window, but other shapes have not been checked. A fixed size like
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

### Things to leave alone

| Thing | Why |
|---|---|
| The old cockpit `UIController` | **Keep it out of the new scenes.** Its `Awake` destroys its own GameObject when it finds a duplicate, and that deletes the player ship. |
| `GameManager.cs` | **Never delete this file.** `ReadOnlyAttribute` is declared at the bottom of it. Removing the file breaks `[ReadOnly]` everywhere in the project. |

---

## 4 · Decisions already made

These are settled. Do not reopen them.

1. **Three separate scenes, not one scene with canvases toggled on and off.**
2. **New code for the scene flow.** Existing managers get referenced when needed. They do not get
   edited. Now: `QuestSystem` was rewritten for the board in Step 85 and moved to `Scripts/Game`.
   Its stages are kept.
3. **`QuestSystem` stays as it is.** Its stages already fit the design. Now: rewritten for the board
   in Step 85 and moved to `Scripts/Game`. Its stages are kept.
4. **The old cockpit `UIController` stays out.** See the table above for why.
5. **`GameManager.cs` is never deleted.** See the table above for why.
6. **Legacy input for these scenes** — `Input.GetKeyDown`. Both input backends are enabled in this
   project, so both compile. This flow picks the legacy one deliberately.
7. **Canvas setup for the intro scenes:** Screen Space **Camera**, CanvasScaler on Scale With Screen
   Size, reference resolution 1920x1080, match 0.5. (This started out as Overlay in the Step 1 plan.
   It was built as Screen Space Camera and that is what it stays. See Known issues #2 for the catch
   that comes with it.)
8. **Colours come from the design handoff tokens.** Do not invent new ones.
   - Background / void `#0A0C0E`
   - Text `#E8EDF0`
   - Prompts and orders, amber `#E8A33D`
   - Sensor cyan `#3FA9C9`
   - Plot glass `#0A1719`
9. **The grid is a world-space plane, not a UI element.** `TacticalGrid.shader`, not `NavGrid`.
   Now: the board's grid is `PlotGrid`, a UI mesh on the plot.
10. **The scene stays 3D top-down**, so collision and physics work later without rework.
11. **The new handoff wins on weapons and power** (owner, 2026-10-04, editable later): two railgun
    turrets, two PDC groups, one missile launcher, four power buses (ENG, WEP, SEN, AUX). Its fire
    sequence and the missile salvo knob are wanted.
12. **Grid tint toggles white and amber**, not white and red. Something to tune.
13. **Distances are kilometres. One world unit is one kilometre.** Replaced in Step 111: the owner
    thinks in units now (a small square at x1.0 is 50 units, our ship 100). The game's numbers did not
    change, so one old km is one unit. Step 113 renamed the code to units too. The screen still shows
    km; what the player reads is decided later. Now: our ship is 200 x 50 units, the Garuda 350 x 100
    and the frigate 100 x 25 (Steps 126 and 127).
14. **The handoff is built in order:** static board first, then movement, then interactions with
    no game wiring, then the wiring.
15. **The board loads undamaged** (all green, rooms sealed) **and is gamma-space only.** See
    `UI_INTEGRATION_LOG.md`, section 3, and its Step 31 (deleted; `git show 12ccdd1:Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md`).
16. **A new World object owns the game state and feeds the board** (owner, 2026-10-17). The old
    `Radar.cs`, `EnemyShip`, `PlayerShip` and `PowerManager` scripts are not revived.
17. **Wire one feature at a time** (owner, 2026-10-17). Plan each feature with the owner. Don't
    invent: follow the sources. Each feature must work before the next starts.
18. **The North Star is a dummy enemy at a fixed position** (owner, 2026-10-17). It sits at TRACK
    07's spot, bearing 048, 24,300 km. It does not move or fire. It is there to check the radar,
    moving around it, and shooting at it. Its plot dot shows only once identified. Since Step 75,
    inside visual range its model shows before it is identified (decision 40). Now: the Garuda. It
    starts at a random point in its spawn box (Step 95), hunts us, and fights with railguns and
    missiles (Step 97).
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
23. **Every tunable value is in one window**, Starwolf → Tuning (owner, 2026-10-17). Tag every new
    tunable with `[Tune("Section")]`. Details in Step 38. **Being replaced by 31:** the window goes
    when each ship's numbers move into its stats file. Now: the window stays, with six tabs: Ship,
    Enemy, Board, Keys, Hard Points and Ranges (Steps 80, 108, 121 and 129). `[Tune]` is for board values only. Ship numbers go in each part's
    `Stats`, in the ship's stats file.
24. **Nothing standing on the plot** (owner, 2026-10-18). No always-on labels. Readouts come from
    using a tool. The PDC arcs show once the PDCs are activated; the alarm strip shows only for a
    real alert. Details in Step 39.
25. **Switches slide, refuse and click** (owner, 2026-10-18). Rocker pads slide as the design
    animates them. A locked switch nudges when clicked (owner addition). Click sounds come from the
    prototype. Unity audio is on. A refused order keeps the design's reaction, plus the refused
    sound. Details in Step 40.
26. **Our ship moves across a fixed grid** (owner, 2026-10-18). Top speed about 2.5 km/s. The plot
    pans by dragging the grid or with the arrow keys. Details in Step 41. Now: top speed is
    50 units/s at ENG 5 and 25 at ENG 1 (Step 130).
27. **Plotting feeds the firing solution** (owner, 2026-10-23). A passive contact gives a bearing
    only, and the bearing trail finds its range. The solution is dialled by hand, aiming at a point
    in space; TRACK needs an active scan. Details in Steps 45–47. Now: TRACK takes the plot, the
    newest POINT and the newest POINT-to-POINT RULER (Step 106). A lock gives the solution otherwise
    (Steps 125 and 141). Now: the bearing trail is gone (Step 161). To find a heard ship's range,
    plot it (POINT, RULER) or scan it.
28. **Railgun: a 50 km/s slug that hits within a class radius, aimed in whole degrees** (owner,
    2026-10-23). Whole degrees stay on purpose: from ~24,000 km even the best aim can just miss a
    cruiser. Close in for a sure shot. Details in Step 48. Now: the slug flies at 300 units/s,
    about 7 s to its 2,000 reach (Step 130).
29. **Fighters fly and fight on the plot; RECALL brings them home** (owner, 2026-10-23). 40 km/s (8 before Step 63's 5×),
    attack runs of 10 armour per fighter, wedges in a V with no labels. They land when they reach
    us. Details in Step 54. Now: 100 units/s (Step 130). Now: small white triangles that appear
    beside our ship (Step 160).
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

    Now: the menu has only `Starwolf/Tuning`, `Starwolf/Level` and
    `Starwolf/Playtest/Run power-up playtest`. Keys, Hard Points and Ranges are tabs of Tuning (Step
    129). The panels are prefabs, and the guide for UI artists is `docs/BOARD_UI.md`.
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
38. **Ruins are in the level, sized for play** (owner, 2026-10-25/26). Placed copies go in the
    level scene; the prefabs are shared in `Assets/Prefabs/Terrain/`; what is the same in every level
    (drawing ruins on the plot, the "found" rule) stays on the board. The starbase's hard core is at
    the grid centre and debris gets thicker toward it. Our ship starts at one edge (Level_01: the
    bottom), the enemy at the other (the top): you each come through the core to find the other.
    Ships are really 100–500 m long, but ruins are sized for how the ship plays (gaps of a few hundred
    km), not for the 13 km models, which stay until the outside cameras come. Details in Step 73.
    Now: Level_01 has no husk (Step 107).
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
42. **Each weapon has its own reach** (owner, 2026-10-27). Railguns **25,000 km**, missiles
    **35,000 km**, PDCs **10,000 km**, on our ship and the Garuda alike. A slug takes about 10 s
    to reach 25,000 km (decision 46). Missiles are
    slower but home, so they reach further. Was 30,000 km for railguns and missiles (the DRADIS at
    full gain) and 4,000 km for PDCs. The numbers are in the Weapons group of each ship's stats
    file (`WeaponSystem.Stats.mounts`). The plot's weapon ring follows the selected mount. Now:
    railguns 2,000, missiles 3,000, PDCs 600 (Step 130).
43. **Ammunition is a real count** (owner, 2026-10-27). It only goes down; there is no reload.
    Each railgun shot takes one slug from its turret. Each PDC go at a missile takes 100 rounds
    from its group. Each missile takes one cell. Every mount starts full: railguns 22 and 22, PDCs
    2400 and 2400, missiles 8. The fire control line reads "SLUGS 22", with no "RAIL 2 / 2". With
    nothing left, LOAD refuses. Details in Step 79.
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
46. **The railgun slug is the fastest thing on the board** (owner, 2026-10-27). **2,500 km/s**:
    about 10 s to its 25,000 km reach. The owner's words: you know where the target is, because you
    scanned it and did the maths. Was 250 km/s (about 100 s). The number is `slugSpeedKms` in the
    Weapons group of each ship's stats file. Details in Step 81. Now: 300 units/s, about 7 s to its
    2,000 reach, and the number is `slugSpeedUnits`. Slug and missile speeds are separate numbers, and
    the missile is faster, at 400 (owner, Step 130).
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

---

- **Notes go in `docs/`** (owner 2026-11-10). Never make a .md note inside `Assets`. Add to a note that exists first. Step 187.
## 5 · Known issues and traps

1. ~~Build Settings not on disk.~~ **Fixed.** The scenes are now written into
   `ProjectSettings/EditorBuildSettings.asset`. Worth remembering why it looked broken before: that
   file only catches up when Unity saves the project, so a scene you just added in the Editor will
   not show up on disk until then.

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

13. **The old `TacticalGrid.shader` (unused) takes its line width in pixels, not world units.** Step 18 has the story.

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

---

## 6 · Steps 6 to 28, and 34 to 118, in detail

### Step 6 — corner panels fixed and verified

The four corner panels are `ShipStats`, `Power`, `Weapons` and `Scanner`.

- They are **real `UIController` canvases from `UI.prefab`**, rendered to RenderTextures and shown as
  RawImages. **They are not rebuilt UI.** Nobody remade these screens by hand, and nobody should.
- Each panel is `Frame` + `Screen` as **siblings**, with `Frame` first in the hierarchy. Unity UI
  draws children on top of parents, so while the frame was a child of the RawImage it simply covered
  the content. Making them siblings is what fixed it.
- The RawImages are **300x300**, not 420x236. Every render texture in `UI.prefab` is 1024x1024 —
  square — so a 16:9 RawImage stretched the picture.
- Confirmed working in play mode.

### Step 7 — linear colour space fix on the grid

- The project renders in **Linear** colour space. Values handed to `Material.SetColor` are treated as
  linear, so the sRGB numbers we were passing came out washed-out grey-green.
- **Fix: append `.linear` to every colour set from code** on `TacticalGrid.mat`.
- The amber centre crosshair was removed at the owner's request. `_AxisEnable` is set to 0 on the
  material.

### Step 8 — art profile ScriptableObject created

New file: `Assets/Scripts/Tactical/TacticalArtProfile.cs`. One ScriptableObject holding every sprite
and colour the tactical board uses, so art can be swapped later without touching the scene or any
code.

Create one from `Assets > Create > Starwolf > Tactical Art Profile`.

What is on it:

| Group | Fields |
|---|---|
| System icons (6) | targeting, sensors, reactor, hangar, comms, engineering |
| Tool icons (7) | point, line, ruler, compass, erase, undo, clearAll |
| Frames (2) | metalPlate, panelFrame |
| Hull glyphs (2) | hullFilled, hullOutline |
| Board colours | plot background, grid minor, grid major, icon tint, icon tint active, frame tint |

Why it exists: the owner said missing art is fine, so long as there is **one place** to swap the
assets in later.

### Step 9 — tool icons corrected to the handoff

- The first set of tool icons was generated by guesswork. The owner pointed out the real ones are in
  the design handoff.
- Source: `References/Tactical spaceship UI planning.zip`, file `Star Wolf Asset Kit.dc.html`,
  section **6.2 TOOL BUTTON**. The glyphs there are **CSS shapes, not SVGs**, drawn at 19x19 with a
  1.5px stroke.
- The exact shapes recorded there:
  - **POINT** — a full-width crosshair.
  - **LINE** — a single stroke at -42 degrees.
  - **RULER** — a 20x5 outlined bar at -42 degrees.
  - **COMPASS** — a circle with a needle from top centre.
  - **ERASE** — an X at plus and minus 45 degrees.
  - **CLEAR** — a dashed rectangle.
- The handoff zip is **never unzipped in place**. Extract it to the session scratchpad and read it
  from there. `References/` is owner-authored input and is never edited.

### Step 10 — grid turned amber, with a third tier of lines

- `TacticalGrid.mat` colours now come from `Assets/Art/TacticalArtProfile.asset`, converted with
  `.linear`. **The profile is the source of truth. The material is just where the values land.**
- `TacticalGrid.shader` was rewritten to three tiers of lines: fine at 400 units, minor at 2,000,
  major at 10,000.
- **Each tier fades on its own.** A tier is invisible once its lines crowd closer together than
  `_FadeMin` (6) pixels, and full above `_FadeMax` (26). So zooming out thins the grid instead of
  turning it to mush, and zooming in reveals finer detail.
- New shader properties: `_FineColor`, `_FineDivisions`, `_FineWidthScale`, `_FadeMin`, `_FadeMax`.
- The corner panel frames changed from cyan to the profile's `frameTint` amber.

### Step 11 — the orange panel bug, and the layer that fixes it

- **Symptom:** once the grid went amber, the ShipStats panel filled with orange.
- **Cause:** every camera in the scene had a culling mask of -1, meaning everything.
  `Camera-PlayerShipStats` was photographing the 140,000-unit grid quad as a backdrop behind the
  ship model.
- **Fix:** a new layer 8, `TacticalPlot`. `TacticalGrid` was moved onto it, and the layer was removed
  from every camera's culling mask except `Main Camera`.
- An opaque black `Backdrop` Image was also added as child 0 of all four panels. Panel order is now
  `Backdrop, Frame, Screen`. Nothing behind a panel can bleed through it any more.
- **The trap worth remembering: any new world-space object in this scene must go on the
  `TacticalPlot` layer, or the UI cameras will photograph it.**

### Step 12 — the two icon rails

- New file `Assets/Scripts/Tactical/IconRail.cs`. It builds its own buttons from a
  `TacticalArtProfile`, so adding or reordering a system is a profile edit, not scene surgery. Call
  `Rebuild()`.
- Metrics from the handoff: 64 wide, 52x52 cells, 5px gaps, 6px padding, 26px inset from the screen
  edge.
- `Canvas/Rails/LeftRail_Tools` — 7 tools, 406 tall. `Canvas/Rails/RightRail_Systems` — 6 systems,
  349 tall. Both vertically centred.
- **System icons carry their own colour and must not be tinted.** The handoff says so explicitly.
  Tool icons are drawn white and do get tinted. That is the `tintIcons` flag.
- Icon size is **34 inside a 52 cell**. At 26 the glyphs read as specks.
- Rail backing plates are RawImages with a cropped `uvRect` onto `steel_plate.png`, not stretched
  Images, so the brushed grain stays at its own scale.

### Step 13 — power controls brought over from the cockpit

- The Power controls were never in `Canvas-Power`. They live in `Assets/Prefabs/Canvas.prefab` under
  `Power_Cont`, and were lost when `Canvas-Interior` was deleted from the scene.
- `Power_Cont/Power/Buttons` (five system toggles plus the reactor dial) and `Power_Cont/Power (2)`
  (the two green plates, Emergency Vent and Black Alert) were reparented into `Canvas/Panels/Power`
  as `Controls` and `Green`. The working render-texture screen was kept between them.
- The panel is now 300x420: green plates on top, screen in the middle, toggles at the bottom. The
  source cluster is 855 wide, so everything is scaled by 300/855.
- **Trap: reparenting a UI subtree collapses it to 0x0 if you do not set `sizeDelta` explicitly.**
  The originals sized themselves from stretch anchors against their old parent. Set the size the
  layout groups expect (855x150 and 855x170), then call `LayoutRebuilder.ForceRebuildLayoutImmediate`,
  then `Canvas.ForceUpdateCanvases()`, then rebuild again.
- The panel grew to 420 tall and collided with the tool rail, so it moved right to clear it.

Final boxes, in screen pixels at 1920x1080. No overlaps.

| Thing | x | y |
|---|---|---|
| ShipStats | 30–330 | 750–1050 |
| Power | 110–410 | 30–450 |
| Weapons | 1590–1890 | 750–1050 |
| Scanner | 1590–1890 | 30–330 |
| LeftRail | 26–90 | 337–743 |
| RightRail | 1830–1894 | 366–715 |

### Step 14 — the board's data layer, and the targeting drawer

The owner asked for two things: the rails must be uniform on both sides, and the weapons panel must
open when its rail button is clicked. The design handoff is explicit on each.

- Handoff section 4: "Geometrically identical to the left rail — same 64px width, same 52px cells,
  same 5px gaps, same centring — mirrored to right:26px. This symmetry matters; the two rails must
  never drift apart in size or rhythm."
- Handoff section 5: the weapons panel is the targeting computer. A slide-out drawer, 376x830 at
  right:104 top:100, that pulls out from the rail over 0.42s on `cubic-bezier(.2,.85,.25,1)`.

**New ScriptableObjects.**

- `RailStyle.cs`, asset at `Assets/Art/Board/RailStyle.asset`. Holds the rail geometry: width 64,
  cell 52, gap 5, padding 6, edge inset 26, icon size 34, button face sprite, plate texture and tint.
  **Both rails read this one asset, so they cannot drift apart. That is the whole point of it.**
- `plateCells` is 7. Both rails get a plate that tall whatever they hold, so a 7-tool rail and a
  6-system rail still read as a mirrored pair. Both are now 64x418.
- `BoardAction.cs`, thirteen assets under `Assets/Art/Board/Actions/`. One per rail button: id,
  label, icon, kind (Tool or System), whether the icon gets tinted, and what clicking it opens
  (panel mode, size, offset, render texture, slide time). **Adding a system is a new asset dropped
  into a rail's list, never a scene edit.**
- `TacticalArtProfile.cs` was cut back to the theme: frames, hull glyphs, and the colour set (plot
  background, grid fine/minor/major, icon tint, icon tint active, frame tint, panel well). The
  per-button icon fields moved onto the `BoardAction` assets, because per-button is the right
  granularity for swapping art.

**New MonoBehaviours.** The rule used: a ScriptableObject holds design data that is true regardless
of scene; a MonoBehaviour holds scene wiring and runtime state. Render textures are assets so a
`BoardAction` can hold one; cameras and canvases are scene objects so it cannot.

- `IconRail.cs` rewritten. Takes a `RailStyle` plus a list of `BoardAction`s, positions itself
  against its own screen edge, and raises a `Clicked` event.
- `DrawerPanel.cs`. Slides a panel in and out, easing on the handoff's `cubic-bezier(.2,.85,.25,1)`
  solved by Newton iteration. Uses `Time.unscaledDeltaTime` so the drawer still works when the board
  is paused. Fades a `CanvasGroup` slightly ahead of the slide and toggles `blocksRaycasts`.
- `BoardController.cs`, on a root `Board` GameObject. Rebuilds both rails in `Start` and subscribes
  before anything can be pressed, then routes clicks. Keeps one drawer open at a time. A button whose
  action has no panel logs and does nothing.

**Scene changes.**

- `Canvas/Drawers/TargetingComputer` added, with a `Panel` child that slides. The order under
  `Canvas` is Panels, Drawers, Rails, so the drawer appears to emerge from behind the rail.
- The `Weapons` corner panel was deleted. That content is the drawer now.
- Verified in play mode: clicking TARGETING slides the drawer open, and a second click closes it.
  Clicking SENSORS or a tool logs and does nothing, as intended.

**Trap worth recording: a solid `Image` is a fill, not a border.** The drawer's frame was one, which
turned the empty lower half into an amber slab. The border is now four thin edge Images under a
`Border` child. The corner panels get away with a solid fill only because their render texture covers
it completely.

### Step 15 — the outer metal frame

Built from the handoff's own spec: Asset Kit section **3.1 BEZEL BUILD-UP** and **3.2 FASTENERS &
SEAMS**.

Three textures were generated into `Assets/Art/Frames/`:

- `frame_metal.png`, 1920x1080. The handoff's 158-degree gradient — `#33383E` at 0 percent, `#23282D`
  at 40 percent, `#181C20` at 100 percent, "cool grey, never warm" — with the brushed grain from the
  handoff's own `figma/steel.png` multiplied over it.
- `rivet.png`, 64px. A radial gradient at 34/30 percent, `#6B747C` out to `#1B1F23` at 72 percent,
  with the lit 1px inner ring.
- `vignette.png`. The handoff's `inset 0 0 160px rgba(0,0,0,.75)` baked as an alpha ramp.

Scene: `Canvas/Frame` as **child 0**, so it sits behind the panels, drawers and rails. It holds a
vignette, four 118px metal bands, five bevel strips and four corner rivets. 118 was chosen so the
rails, which run from 26 to 102, sit **on** the frame rather than beside it.

**The bands are one continuous sheet, not four repeats.** Each band is a RawImage whose `uvRect` maps
to its own slice of the single 1920x1080 texture, so the brushed grain runs round the frame with no
seams and no repetition.

The bevel follows 3.1: a lit top lip on the outer shell, and a plot well that is **recessed with a
dark inner edge** rather than given a plain border. Rivets sit at the four shell corners only — the
handoff says never as decorative runs.

**Three traps worth recording.**

1. **Unity rescales a non-power-of-two texture on import.** `frame_metal.png` came in as 2048x1024,
   which broke the exact 1920x1080 mapping the bands depend on. Set
   `npotScale = TextureImporterNPOTScale.None`.
2. **Setting `textureType = Sprite` alone produces no Sprite subasset.** `spriteImportMode` must also
   be set to `Single`, or `LoadAssetAtPath<Sprite>` returns null with no error.
3. **An asset reimport can bounce the editor back into play mode.** A scene build ran during that and
   was silently discarded. Check `Application.isPlaying` before any scene edit, not just at the start
   of a session.

### Step 16 — frame thinned

The 118px band was too heavy. The frame was rebuilt at **44px**. The rails, which run from 26 to 102,
now overhang the frame and float over the plot the same way the corner panels do. Rivets scaled down
to 9px and moved to sit inside the narrower band.

Everything else about the frame is unchanged — same continuous `uvRect` sheet, same bevel.

### Step 17 — engine power and WASD movement

The goal was small on purpose: get power flowing to engines only, so the ship can move under WASD,
before building anything else.

**Nothing new was written. It all already existed.** The audit trail:

- `PlayerShip.FixedUpdate` at `Assets/Scripts/PlayerShip.cs:517`. W is `:531`, S is `:536`, A and D
  are `Input.GetAxis("Horizontal")` at `:612`. Velocity is locked to `transform.up` at `:596`, so the
  hull never slides sideways.
- The engine power gate is at `:526` — `powerManager.GetSystemEfficiency("engines")`.
- **Pressing 1 toggles engine power.** `PlayerShip.cs:241` calls
  `PowerManager.ToggleSystemState(engines)`.
- All six input axes the ship asks for exist in `ProjectSettings/InputManager.asset`: Horizontal,
  ControllerThrust, LeftTrigger, RightTrigger, DPadX, DPadY.
- `PowerManager.GetSystemByName` lowercases its argument, so the lowercase `"engines"` lookup
  resolves.

**The important behaviour to record: turning is multiplied by engine power.** `PlayerShip.cs:619` is
`targetTurnSpeed = turnInput * turnRate * enginePower`. With no power the ship cannot turn at all,
and thrust is capped at a 5 unit/s crawl (`:553`). **Power is not optional for movement. It gates
both.**

Power fills at 0.5 bars per second, split across every drawing system, so engines alone take about
10 seconds to reach 5 of 5. Each bar costs 0.5 reactor. Reactor reports 8 max here, not the
serialized 15, because `GetMaxReactorPower` scales by the `InternalSubsystems` reactor multiplier.

**A real bug found and fixed: the Rigidbody had no constraints.** `PlayerShip.Awake` at `:84` only
sets `useGravity`, `constraints` and damping **when it creates the Rigidbody itself**. The scene's
ship carries one from the prefab, so it was left completely unconstrained — free to drift in Z and
tumble in X and Y on a flat top-down plot. It is now set on the scene instance to
`FreezePositionZ | FreezeRotationX | FreezeRotationY`, with interpolation on. **Any new ship added to
a Tactical scene needs the same, or it will tumble.**

Camera zoom was temporarily changed from orthographic 30,000 to **600**. At 30,000 the ship drew
**0.26 pixels tall** and was invisible. At 600 it is about 13 pixels. **This is a stopgap.** The plot
layer will draw own ship as a fixed-size glyph, the way the prototype does, so zoom stops affecting
it. The grid looks sparse at this zoom because only the 400-unit fine tier is in range.

`PlayerSettings.runInBackground` was turned **on**. It was off, which meant play mode froze
completely whenever the editor lost focus — `Time.frameCount` stayed at 1 across repeated probes.
**This is why an automated play-mode test appears to do nothing.** Even with it on, driving play mode
over time from outside the editor proved unreliable in this session. Held keys have to be verified by
hand in the editor.

- **Verified:** engine power reaches 5 of 5 with efficiency 1.0, and the ship produces velocity along
  its own nose.
- **Not verified:** W, S, A and D under a real held keypress. That needs a human at the keyboard.

### Step 18 — three bugs on the plot, and the plot camera

**Bug 1: grid line width was in world units, not pixels.** `_LineWidth` was 12 world units. At a
strategic zoom that is invisibly thin. Zoomed in to orthographic 600 it became a 24 pixel amber bar,
and because the fine (400), minor (2,000) and major (10,000) lines all pass through x=0 and y=0, it
read as a bright amber cross through the middle of the plot. That was mistaken for the centre
crosshair, which was already off. **A note recorded earlier in this file claimed `fwidth` held the
stroke to one pixel. That was wrong** — `fwidth` was only feeding the antialias, not the width.
`lineCoverage` now takes a width in **pixels** and multiplies by `fwidth(coord)`, which is world
units per pixel, so the stroke is constant on screen at any zoom. `_LineWidth` is now 1.1 pixels.

**Bug 2: `PlayerShip` was hijacking the plot camera as a chase cam.** `PlayerShip.Start` at
`Assets/Scripts/PlayerShip.cs:133` grabs `Camera.main` whenever its `cameraTransform` field is empty.
`:663-684` then writes ship position plus an offset, **and ship rotation plus a roll offset**,
straight onto that camera every FixedUpdate. On a top-down plot that made the whole grid pivot as the
ship turned. Fixed by giving it a dummy `LegacyChaseCamRig` GameObject to drive instead, and setting
`maxCameraRollOffset` to 0. **Any Tactical scene containing PlayerShip must fill in `cameraTransform`,
or the plot camera gets hijacked.**

**Bug 3: `ScreenConstantScale` compounded itself to nothing.** It measured its base size in `Awake`,
which meant every domain reload took the already-scaled transform as the new base. The first run
produced a scale of 0.12 instead of about 16. `baseScale` and `baseHeight` are now
`[SerializeField, HideInInspector]` and measured exactly once, with a `Remeasure(trueScale)` to start
over deliberately.

**New: `Assets/Scripts/Tactical/PlotCamera.cs`**, on `Main Camera`.

- Scroll wheel zoom between 400 and 30,000, eased rather than snapped, anchored on the cursor so the
  world point under the pointer stays put.
- An optional follow target, currently **off**. The owner wants the camera fixed to the world map,
  not chasing the ship.
- It exposes `ScreenToPlot` and `PlotToScreen`, the screen-to-world bridge the tools and the
  targeting computer both need. **That is the first piece of the plot layer.**

Also in this step: the camera is back to orthographic 30,000 at `(0, 0, -1000)`, rotation identity.
The own ship glyph was raised to 170 pixels at the owner's request. The Scanner (comms/sensor) corner
panel is disabled.

### Step 19 — tuning asset

`Assets/Art/Board/TacticalTuning.asset`, from `Assets/Scripts/Tactical/TacticalTuning.cs`, with
`Assets/Editor/TacticalTuningEditor.cs` providing the buttons and the `Starwolf > Tuning` menu.

It holds the movement, power, stability, plot camera, ship glyph and grid numbers in one place.

**It is not a runtime indirection.** Apply writes the numbers into the scene as real serialized
values. Capture reads the scene back into the asset. Nothing reads the asset at runtime, so what you
see on a component is what the game uses, and there is no hidden lookup to debug.

**Two scene overrides were making movement feel dead** and have been corrected:

| Field | Was | Now | Why it mattered |
|---|---|---|---|
| `turnRate` | 20 | 90 | An 18 second full circle. |
| `rateOfAcceleration` | 5 | 50 | 20 seconds to reach top speed. |

Both were **scene overrides** of the script defaults, not the script values themselves.

### Step 20 — why the power bars never moved

The owner reported the engine bars not filling. **Power was working. The display was not.**

**The audit.** `UIController` has 137 object references in this scene. **104 of them, 76 percent, are
unassigned.** Whole arrays are empty: `imgPowerMet` 6 of 6, `powerNodeImg` 6 of 6, `btnPowerBool`
6 of 6, `btnPowerBoolImage` 5 of 5, `sc` 7 of 7, `canvasUI` 6 of 6, and ten more.

**The blocker.** `UIController.Update` calls `UpdateCompass` at `:242` and `ScanMovment()` at `:259`.
Both threw a NullReferenceException every frame — `compassRect` and `scannerSlider` belong to
`Canvas-Navagation` and the scanner canvas, and the Tactical scenes load neither. An exception aborts
the rest of the method, so `SyncPowerBarsFromManager()` at `:315` **never ran at all.** Four
exceptions a frame, sixty times a second.

**Fixes applied.**

1. Three null guards added to `UIController` — `ScanMovment` (`:427`), `UpdateCompass(float)`
   (`:1033`), `StabilityMeterStart` (`:1138`). Each returns early when the UI it needs is not loaded.
   **These are the only edits made to that file, and they are purely defensive.**
2. `imgPowerMet` and `powerNodeImg` wired, 6 of 6 each, to the real objects inside
   `UI/Canvas-Power`.
3. New `Assets/Scripts/Tactical/PowerMeterBinding.cs`, on a `PowerMeters` child of PlayerShip. It
   drives the meters **straight from `PowerManager`**, reusing the existing `UI_GradeDisplay`
   shader-graph art from the UI prefab, and does not go through `UIController` at all. The shader
   properties are `_PowerCur` and `_PowerMax`. **It instances each material in `Awake`** — otherwise
   every bar shares one material and they all show the same value.

Verified in play mode: with engines at 5 of 5 and arms at 3 of 5, the shader reads `_PowerCur` 5 and
3, and the panel shows five and three lit cells.

**Why a separate binding rather than repairing UIController's path:** UIController's power sync sits
behind two calls that reference UI the Tactical scenes do not have. A direct binding has nothing in
between that can break. UIController still runs and still owns the other panels.

**The general lesson, worth carrying forward:** an exception anywhere early in `UIController.Update`
silently disables everything after it. When a panel in a Tactical scene appears frozen or static,
check the console for a per-frame exception before assuming the underlying system is broken.

### Step 21 — plot layer, part one: camera, zoom range and input

This is the foundation the tools and the targeting computer both sit on. **Nothing owned the plot
before.** There was no way to turn a screen position into a world position, and clicks on the board
went nowhere.

**Zoom range widened a long way.** `PlotCamera` now runs from orthographic **300 to 250,000**, up
from 400 to 30,000. At maximum the view is 500,000 units tall against a roughly 50,000 unit arena,
which is what the owner asked for. Zoom stays cursor-anchored and eased.

**New `Assets/Scripts/Tactical/PlotGridFitter.cs`**, on the `TacticalGrid` quad. The quad was a fixed
140,000 across, so past about orthographic 35,000 the plot simply ran out and went black at the
edges. The fitter tracks the camera and scales the quad to cover the view with a 1.35x margin. At
orthographic 250,000 the quad sizes itself to 1,200,000. **The grid pattern does not move when the
quad does** — the shader computes it from world position, so the quad only decides how much of the
grid exists, never where it is.

Confirmed by render at maximum zoom: only the 10,000-unit major tier survives, about 21 pixels apart.
The finer tiers fade themselves out. That is the shader's fade behaviour working as intended.

**New `Assets/Scripts/Tactical/PlotSurface.cs`**, as child 0 of the Canvas, so it sits behind every
panel, drawer and rail and never steals their clicks. It is an `Image` with zero alpha and
`raycastTarget` on — the standard way to receive pointer events without drawing anything. The graphic
is created in the scene rather than only in `Awake`, so it is inspectable.

It exposes four events, all in **world** coordinates, and knows nothing about tools: `Clicked`,
`Dragged` (start and current), `DragEnded` (start and end), and `Hovered`.

**Panning** is on the middle button, with the right button as an alternative. **Left is deliberately
left free for the tools.** The pan works by dragging the world rather than the camera: it moves the
camera by however far the grabbed world point has slipped from under the cursor, so the point you
grabbed stays under the pointer.

**Verified: the screen-to-world round trip is exact.** Screen centre maps to the camera position, and
all four corners round-trip with 0.000 pixels of error at orthographic 30,000. **`ScreenToPlot` and
`PlotToScreen` are now the single conversion everything else must use. Nothing should write its
own.**

**Trap:** `PlotCamera` caches its `Camera` in `Awake`, so calling `ScreenToPlot` from an editor
script before play mode requires setting that field first.

### Step 22 — own ship glyph: minimum size instead of constant size

The owner asked for a minimum visible size, so the ship cannot become infinitesimally small when
zoomed far out.

**Measured first: it was not actually shrinking.** `ScreenConstantScale` was holding it at exactly
170 pixels at orthographic 1,000, 30,000, 120,000 and 250,000. But the request exposed a real flaw in
the other direction — **at constant size the ship also never grows when you zoom in**, which is wrong
for a real object on a plot.

`ScreenConstantScale` was reworked from constant size to a **floor**:

- `minPixels` (170) — never draw smaller than this.
- `maxPixels` (0 = no ceiling) — never draw larger than this.
- Above the floor it draws at **true world scale**.

Measured behaviour, with the ship 116 world units tall:

| ortho | true size | drawn | behaviour |
|---|---|---|---|
| 300 | 209 px | 209 px | true scale |
| 369 | 170 px | 170 px | crossover |
| 1,000 | 63 px | 170 px | floored |
| 30,000 | 2.1 px | 170 px | floored |
| 250,000 | 0.3 px | 170 px | floored |

So zoom in and the ship grows like a real object. Zoom out and it settles into a glyph. At maximum
zoom true scale would be a third of a pixel, so the floor is doing real work. **This matches how the
prototype handles plot symbols** — they shrink with zoom, but only to a limit.

A `TruePixels` property was also added, so other code can ask how big something would be at true
scale without duplicating the maths.

`TacticalTuning` updated: `shipGlyphPixels` is replaced by `shipMinPixels` (170) and `shipMaxPixels`
(0). `TacticalTuningEditor` maps both. Apply and Capture were round-tripped against the new fields
and verified.

**Note the class name is now slightly misleading.** It gives a minimum size, not a constant one.
Renaming it would break the scene reference, so it stays for now. Recorded here so nobody trusts the
name over the behaviour.

### Step 23 — plot layer, part two: annotation rendering and the seven tools

**Zoom and pan feel, measured first.** Zoom drift is 0.015 world units over twelve notches out, which
is 0 pixels. Pan slip is 0.004 world units. The full 300 to 250,000 range is 34 scroll notches end to
end. Both are effectively exact.

Two feel problems were fixed by inspection rather than measurement:

1. `PlotCamera` now carries `[DefaultExecutionOrder(-100)]`. `PlotGridFitter` and
   `ScreenConstantScale` both read camera state in `LateUpdate`, and with no fixed order they used
   last frame's zoom and lagged a frame. That reads as jitter while scrolling.
2. Zoom used to anchor on the raw `Input.mousePosition`, so it would lurch toward a point hidden
   behind a panel, or toward a stale position once the pointer left the window. `PlotSurface` now
   reports the pointer through `PlotCamera.SetPointer` on move and on exit, and zoom anchors on the
   cursor **only when it is genuinely over the plot**. Otherwise it zooms about the centre.

**New `Assets/Scripts/Tactical/PlotAnnotation.cs`.** One mark: Point, Line, Ruler or Ring. Positions
are stored in **world** coordinates. It carries `Range` and `Bearing`, where bearing is compass
degrees — **0 is +Y north, 90 is +X east** — not atan2's maths convention.

**New `Assets/Scripts/Tactical/PlotOverlay.cs`**, a `MaskableGraphic` that rebuilds one mesh each
frame from world coordinates projected to screen. **World coordinates stored, screen space drawn.**
That is what keeps a mark pinned to the world while its lines stay 1.5 pixels wide instead of
ballooning as you zoom in. One Graphic for every annotation, so the whole layer is a single draw
call. It also owns `Undo`, `Clear` and `EraseNear`, which picks the nearest mark within a pixel
radius using true point-to-segment and point-to-circle distance.

**New `Assets/Scripts/Tactical/PlotTools.cs`.** Five modes — POINT, LINE, RULER, COMPASS, ERASE — and
two immediate actions, UNDO and CLEAR ALL. **The actions never become the active tool.** That was the
trap flagged during planning: treat them as modes and the board gets stuck in a tool that does
nothing. Pressing the live tool again puts it away. Colours follow the handoff: green for
measurement, amber for orders and marks. A drag shorter than four pixels is treated as a click, not a
mark.

Scene: `Canvas/PlotOverlay` added at child index 1, between `PlotSurface` and `Frame`, with
`raycastTarget` **off** so it can never eat a click. `PlotTools` sits on the `Board` object.
`BoardController` now routes any `BoardAction` of kind Tool to `PlotTools.Press(id)` instead of
looking for a drawer.

**Verified in play mode, every tool through the same path a rail click takes:**

- POINT drops a mark and reads out `X 4,000   Y 2,000`.
- RULER north reads `BRG 000`, east `BRG 090`, south-west `RNG 14,142 u   BRG 225` — 14,142 being
  10,000 root two, so the range maths is right.
- COMPASS reads `RADIUS 8,000 u`.
- ERASE removes the nearest mark.
- UNDO and CLEAR leave the active tool untouched.
- Re-pressing the live tool sets it to None.

Confirmed by render: two range rings, a dashed green ruler with end crosses, a solid amber line and
amber point crosses, all crisp.

**Trap: do not name a public MonoBehaviour method `Invoke`.** `PlotTools.Invoke(string)` collided
with `MonoBehaviour.Invoke(string, float)` and every reflection lookup threw an
`AmbiguousMatchException`. It was renamed to `Press`.

---

### Step 24 — Cut to five tools, MOUSE as the default

The owner cut the tool set down, twice, firmly. LINE merged into RULER — a line that does not tell
you its range and bearing is decoration. UNDO and CLEAR ALL were dropped entirely.

**Clear-all is nested in the eraser.** Press ERASE a second time while it is already live and the
plot wipes. It does not get a button.

Five buttons: MOUSE, POINT, RULER, COMPASS, ERASE. MOUSE is the default and means "no tool" — the
plot is something you look at. `line.asset`, `undo.asset` and `clear.asset` were deleted.

`RailStyle.plateCells` set to **6**, the larger rail's count, so both plates stay the same height and
the rails read as a mirrored pair. Both are now 64 x 361.

Active state follows the handoff: a lighter face plus a 3px left bar in the icon's own colour. Built
once per button and shown or hidden by `IconRail.SetActiveId`, driven from `BoardController` off
`PlotTools.ToolChanged`.

### Step 25 — Plot labels

`PlotLabels.cs`. A pool of TextMeshProUGUI objects, repositioned every frame from world coordinates,
so a label stays stuck to its mark through pan and zoom. Text cannot come out of `PlotOverlay`'s mesh
— that is one Graphic drawing lines.

POINT reads `P1  T+0:13`, RULER reads `RNG 34,000 u / BRG 118°`, COMPASS reads `R 18,000 u`.

`MissionClock.cs` added as the one place that answers what time it is. It is `Time.timeSinceLevelLoad`
for now. **When there is a real clock that can pause, only this changes** — otherwise pausing would
drift every timestamp against the clock the player sees.

**Two traps.** `RectTransformUtility.ScreenPointToLocalPointInRectangle` with a null camera silently
returns zero on a Screen Space - Camera canvas, and every label stacks in the middle — pass
`canvas.worldCamera`. And a point and a ruler end can share a coordinate exactly, so labels now stack
rather than printing over each other.

### Step 26 — Ship readout

`ShipReadout.cs`. One TMP line at the top of the board: `SPD 5 u/s    HDG 062°`.

**The UIController speedometer is dead and was not reused.** `UpdateSpeedometer` has its only line
commented out, and `velocityMeterImg` points into the interior canvas these scenes do not load.
`PlayerShip.GetCurrentSpeed()` already exists, so one label reading it was cheaper than extracting a
meter that never worked.

Heading is compass degrees off `transform.eulerAngles.z` — Unity's 298 is heading 062.


### Step 27 — The intro and menu, wired into the flow

The owner asked for one chain: SECTION 36, then STARWOLF, both on their own with no keypress, then a
menu, then NEW GAME to the beta message, then Enter to the board.

**Almost none of this needed new code.** `Assets/StarwolfIntro/` already held the whole sequence —
`StarwolfIntroController` plays SECTION 36 → INITIALISING... → STARWOLF → a pull-back → a menu of
NEW GAME / LOAD GAME / SETTINGS, on its own clock, and exposes a `newGame` UnityEvent. So the work
was wiring, not building. **No new script was written for this step.**

What went into `00_Start`:

- `STARWOLF Intro`, a root object with `StarwolfIntroController` on it, with `glyphData`,
  `displayShader` and `hardwareHum` assigned from `Assets/StarwolfIntro/`.
- `newGame` wired to the existing `ScreenFlow.Advance()`, which already pointed at `01_Briefing`.
- `loadGameAvailable` off. There is no save system, so LOAD GAME draws dimmed and is skipped when
  you arrow through the menu.
- An `AudioListener` on `Main Camera`. There was none, so the hardware hum had nothing to play into.
- `Title` and `StartButton` **deactivated, not deleted.** The intro draws its own title and menu over
  the top. They are left in the scene so the old screen can come back if the intro is dropped.
  Delete them once the owner is happy.

The chain was walked in play mode: intro → menu → NEW GAME → `01_Briefing` → `02_Grid`.

**Pacing.** The package shipped at 2.5 / 1.2 / 2.0 / 3.0 seconds, 8.7 in total, which the owner found
too fast. Now 4 / 2 / 4 / 4, 14 in total. These are inspector fields on `STARWOLF Intro`.

#### Four rendering bugs in the intro package

The package was written against a Gamma project. This one is Linear, and it showed.

1. **Double gamma encode.** The render target was sRGB, so the blit encoded on write and the
   backbuffer encoded again. The `#010601` CRT field measured `(63,113,63)` on screen — mid green.
   Fixed by making the target linear. It is now `ARGBHalf`, not `ARGB32`, because the unlit field
   sits a few thousandths above black and eight bits cannot hold that.
2. **Grain on the black field.** `grain * .003` is below what eight bits can represent near black, so
   it quantised up to a visible green on *every unlit pixel*. That, not the background colour, was
   most of the green. Grain now modulates the lit phosphor instead.
3. **Shader uniforms dropped on reload.** `_Pitch`, `_Phosphor`, `_Background` and the six mask sizes
   were never declared in the shader's `Properties` block. Any shader reload dropped them, the mask
   sizes came back zero, `mask()` divided by zero and **the whole intro drew pure black.** This is
   why SECTION 36 looked missing. They are now declared *and* re-applied every frame by
   `ApplyStaticUniforms`. Declaring alone is not enough — a reload resets a declared property to its
   default, and a default mask size of zero is just as fatal.
4. **The grid washing the screen.** The package draws a full-screen phosphor grid at a 9px pitch. The
   pull-back zooms it 5x, which puts the cells below one pixel, and it aliased into a flat green
   field. Two changes: each tier now fades out as its lines close to within a pixel, and the grid
   only appears during the pull-back, swelling and going again.

#### Why it still does not look like the reference

`References/Company Logo.png` is the canonical image. Measured against our render:

| | reference | ours |
|---|---|---|
| phosphor peak | `(214,255,229)` | `(213,255,212)` |
| unlit field | `(0,19,1)` | `(0,0,0)` before Step 27's CRT work |

**The colour was never the real difference.** The reference is not a logo, it is a whole terminal:
CRT bezel, screen curvature, strong scanlines, a vignette, a faintly lit field, and four corners of
microtext (`UC-FLEET`, `// CLASSIFIED`, `S38-PR-001 / REV A / 1993.11.04`,
`RESEARCH / DEVELOPMENT / OPERATIONS`, `CLEARANCE LEVEL 3` with five boxes, three lit).

The package renders only the core mark. That is defensible — `References/LogoREADME.md` §7 separates
the "core studio mark" from "terminal presentation" and says the bezel, microtext, revision codes,
clearance levels, scanlines and curvature are **not** part of the permanent logo. But it is why the
two look nothing alike side by side.

Added in this step: a lit field, scanlines across the whole screen, a stronger vignette, and a
rounded screen edge, as four tunable shader properties — `_ScanDepth`, `_FieldGlow`, `_Vignette`,
`_ScreenInset`. **This is tone only. No geometry is warped**, deliberately, because `HandleMenu`
hit-tests the menu against fixed rectangles in 1920x1080 space and warping the picture would move
the text away from its own hit boxes.

**Not yet seen on screen.** The Unity MCP bridge dropped before this could be checked. Verify the
CRT treatment and the menu-on-black change before trusting either.

#### Traps

- **`execute_code` and any asset refresh reload shaders, which used to blank the intro.** Bug 3
  above. Fixed now, but it is why so many screenshots in this step came back black.
- **A tool round trip takes roughly fifteen seconds.** You cannot screenshot a beat that lasts four.
  To photograph one, set its duration to 120 at runtime and call `RestartIntro()`.
- **Do not hand-author new glyphs into `glyphs.json`.** The microtext needs `U P V H / - 0 1 4 8 9`,
  none of which exist. `LogoREADME.md` §5 says the glyph tables are canonical and must match the web
  implementation exactly. §11 allows TextMeshPro for terminal metadata — use that, not new glyphs.
- The intro package sits in `Starwolf.Intro`, a namespace. The rest of the project has none. It is
  vendored, owner-supplied code. Left as it is on purpose; do not restyle it.

### Step 28 — A new scene for the new handoff

The owner chose the new Claude Design handoff (`Assets/UI2/Claude Design handoff/export/`) as the
board to build. It was read in full first: README, asset map, tokens, layout, state spec and logic.

`02.1_Grid.unity` was made with a camera, an EventSystem and a `Board` canvas set to the handoff's
numbers. It was added to Build Settings after `02_Grid`. The briefing's `ScreenFlow.nextScene` was
changed from `02_Grid` to `02.1_Grid` — one line in `01_Briefing`.

Checked in play mode: `ScreenFlow.Advance()` in `01_Briefing` lands in `02.1_Grid`, no console
errors.

Steps 29 to 33 are written up in `UI_INTEGRATION_LOG.md`.

### Step 34 — Wiring begins: the dummy enemy exists and the board reads it

**Owner direction (2026-10-17).** Start wiring real game systems, one feature at a time. Plan each
feature with the owner. Don't invent: follow the sources. Each feature must work before the next
starts. The North Star is a dummy enemy at a fixed position. It doesn't move or fire. We use it to
check the radar, moving around it, and shooting at it.

**Owner decisions for this step.**

- The dummy sits at TRACK 07's spot: bearing 048, 24,300 km.
- Its plot dot shows only once identified. That is the board's rule.
- The other four mock contacts (CONTACT 17, CONTACT 22, FTR-2, BC-01) are removed. Their plot art is
  hidden, not deleted.
- A new World object owns the game state and feeds the board. The old `Radar.cs`, `EnemyShip`,
  `PlayerShip` and `PowerManager` scripts are not revived.
- 1 unit = 1 km, already ruled (decision 13).
- Keep real 3D objects seen top-down, for future external viewport cameras.

**What was built.**

- `TacticalWorld.cs` on a root `World` object, outside the Board canvas. Children: `OwnShip` at
  (0, 0, 0) and `Dummy · TRACK 07` at (18058.42, 16259.87, 0).
  - It uses the x/y plane, x east, y north. This matches the old build's 3D plane (rotation about z,
    +y forward).
  - Each tick it writes true bearing (whole degrees, `JsRound`) and range (whole km) into
    `HandoffBoardState.tracks`.
- `WorldContact.cs`, which holds a `trackKey`.
- `HandoffBoardControls.Ticked`: fires once per board tick, after the state ticks, before the panels
  redraw. The world steps inside the board's single timer.
- `HandoffBoardState.KeepTracks` drops tracks with no world contact, and prunes identified and
  classified.
- `HandoffPlotPanel` draws identified hostile contacts at their true km position. For now that is a
  red dot plus a `NAME / RNG n km` label.
- `HandoffBoardBuilder`:
  - new `Contacts (drawn)` and `Contact labels` layers;
  - hides the mock contact art (26 objects);
  - `RelinkWorld` re-points the World at the new board after every rebuild. The first check caught
    the link being dropped.
- 3D models as `Model` children. OwnShip uses `StarWolf_Destroyer_PlayerShip.fbx`, the same model and
  placement as the old `Prefabs/PlayerShip.prefab`. TRACK 07 uses `StarWolf_Cruiser.fbx`. Scale
  0.11612, so about 12–14 km long at 1 unit = 1 km. Their real scale is to be decided when viewport
  cameras come. Models only, no old scripts.

**Verified.**

- Only t07 is tracked.
- Bearing and range through real ticks: 048 / 24,300 at load, and 000 / 090 / 180 / 270 at the four
  compass points.
- Contact board: unidentified at SEN gain 2, `TRACK 07 · 048 · 24k` after the real cone identify,
  gone with SEN empty.
- The dot is exact to 0.0001 px at zoom 1, 0.5 and 0.1, and clipped to the plot.
- The RULER from our ship to the dot reads `RNG 24,300 km · BRG 048°`.
- No mock contacts anywhere.
- The board outside the plot is pixel-identical to before.
- Fire control TRACK and periscope SLEW use the real numbers.
- One control per drawer works.
- The World survives two rebuilds, still linked.
- Console clean.
- Evidence is in `Logs/HandoffQA/step34/`. `Logs/` is git-ignored and survives Unity restarts;
  `Temp/` does not.

**How to see it in play.**

1. Power: SEN to DRAW until 2 units.
2. Sensors: gain to 2.
3. Active scan on, cone to 048, release.
4. Zoom the plot to about ×0.4. The red TRACK 07 dot is top right.

### Step 35 — Power makes the ship loud (signal), and nothing real changes while paused

**Owner decisions (2026-10-17).**

- Every unit drawn to a bus makes us louder, and so does ACTIVE SCAN. Sources: GDD p18, spec
  3.1.b, and the prototype's `getSig`. The design board only counted ACTIVE SCAN; the owner
  overrode that.
- The internal value is called **signal**, on a 0–100.0 scale.
  - Each action adds a tweakable amount. Every amount is an Inspector field.
  - It never jumps straight to 100.
  - The decimals are internal only. The player sees a whole percent on the OWN EMISSIONS meter,
    the 10 cells (white, then amber, then red), the blinking lamp and the alert line.
- The meter climbs and falls gradually.
- Freeze while paused: nothing real changes until RUN. Controls still work.

**What was built.**

- `HandoffBoardState`:
  - `signal`, renamed from `ownSig`. `FormerlySerializedAs` keeps saved values.
  - `signalRise` 9 and `signalFall` 6 per tick.
  - `signalTrackedAt` 45.
  - `Bus.signalPerUnit`, default 10 per bus, so a full 10-unit reactor reads 100.
  - `activeScanSignal` 35 (the reference sim's 3.5 units).
  - `SignalTarget` = clamp(Σ fill × signalPerUnit + scan, 0, 100). `TickSensors` moves toward it
    with no overshoot.
- The sensors note:
  - "RUNNING SILENT" at 0.
  - "EMISSIONS DECAYING — NOT YET RESOLVED" while falling.
  - "POWER SIGNATURE — NOT YET RESOLVED" while rising or steady below 45. This is a stand-in: the
    design has no line for it (`HANDOFF_GAPS.md` row 106).
  - "N HOSTILE SET(S) HAVE YOUR RANGE" at 45 or more.
- `HandoffBoardControls`: the whole tick waits while paused. That includes the world's `Ticked`
  event and the mission clock. The design ran its power tick while paused; the owner overrode that.

**Verified.** STEP 35: VERIFIED, first pass.

- 54 ticks matched an independent model exactly: fill mixes, scan, LOAD SHED, the reactor cap of
  4, and the clamps.
- Tunable values take effect on the next tick.
- The meter, note and blink states are correct.
- Pause froze everything for about 25 s: fills, charges, signal, heading, party, launch, clock and
  the world sync. Everything resumed on RUN.
- Step 34's checks, sensors, fire control and the drawers are unchanged.
- Console clean.
- Evidence is in `Logs/HandoffQA/step35/`.

**Known.**

- "1 HOSTILE SET HAS YOUR RANGE" counts every hostile once we are loud, even far away. The
  enemy-notices-you feature (Power P3, parked) fixes that.
- The board loads PAUSED, so nothing fills until the player presses RUN.
- The reactor serves buses first come, first served. A bus sent to DRAW late can find it already
  spent.

**Playtest grown.** `Assets/Editor/Playtest/PlaytestRunner.cs` now also checks that:

- the board starts paused;
- buses set to DRAW don't fill while paused;
- RUN starts time;
- the signal climbs to its target and reads "1 HOSTILE SET HAS YOUR RANGE";
- LOAD SHED vents everything back to "RUNNING SILENT".

It runs 27/27 checks with a 35 s video. Two playtest fixes went in:

- A control with no argument is saved as "", not null.
- All buses are now set to DRAW while paused, so the reactor shares out evenly.

### Step 36 — Plot tools: marks stay put in space

**Owner decisions (2026-10-17).**

- **POINT** is a timestamp. It stays where it was dropped and shows the mission time at the drop.
- **RULER** draws a distance, typically between two points.
- **COMPASS** sits around our own ship.
- Marks stay put in space. They do not move with our ship.
- Plots feed fire control later. That belongs to the fire-control step.

**What was built.**

- `HandoffBoardState`:
  - Marks store world km (`x, y, x2, y2`), not screen px.
  - `ownX` / `ownY`, our ship's world position, written by `TacticalWorld` every tick.
  - `MarkDown(tool, x, y, missionT)`. A COMPASS always starts at our ship.
  - `MarkUp(minLenKm)` keeps a mark of exactly the minimum length (a tiny tolerance).
- `HandoffPlotPanel` turns km into screen px through the zoom, both ways. A POINT label is its
  mission time. A RULER label is its range and bearing. A COMPASS ring is re-measured from our
  ship.
- Contact dots are drawn at the contact's exact world position, not its rounded bearing and range.
- The first check found three defects, all fixed: a ruler of exactly the minimum length was
  dropped; the contact dot used rounded numbers; the ruler label's rounding. The second check found
  the label still 1 px off on exact half pixels (km to px leaves float dust). It now trims to 6
  decimals before rounding.

**Verified.** STEP 36: VERIFIED on the third pass (2026-10-18).

- 2,016 rulers and 672 compass rings over 4 ship positions and 3 zooms: none wrong. Rulers of
  exactly 4 px are kept.
- A replay of 914 design-board steps matched label text, shapes and hit boxes. No ruler label was
  off (935 were before the last fix).
- The contact dot sits within 0.0001 px of its true world spot.
- Marks stay put when the ship moves; the COMPASS re-measures.
- Evidence: `Logs/HandoffQA/step37/v/` and `Logs/HandoffQA/step38/`.

**Playtest grown.** It now drops a POINT, a RULER and a COMPASS, moves our ship 5,000 km east
(a stand-in for the helm), and checks the POINT and RULER stay put in space with their labels
unchanged while the COMPASS is re-measured. ERASE clears them. 37/37 PASS.

**Known, for the helm step.**

- The grid does not scroll with the world yet.
- While moving, marks step at the board tick (2.4 a second), not smoothly.
- The COMPASS erase area is a square.

### Step 37 — Our ship on the plot, as the 3D model seen from above

**Owner direction (2026-10-17).** Our ship must appear on the plot. It is the 3D model seen from
above. Its size is true to scale, with a minimum and a maximum so it never vanishes or swamps the
plot. The design handoff already specified the glyph (34 × 76 hull, turned to heading, shrinks
with zoom). It had been left as placeholder rectangles. That was our miss.

**What was built.**

- `ShipView.cs` on a new `Ship View Camera`. It looks straight down at `OwnShip`, north up, and
  renders only the `WorldShips` layer into a 256 px texture. It measures the ship once, so the
  ship fills 80% of the frame.
- `OwnShip` turns to the board's heading every tick (`TacticalWorld`).
- A `Ship Light`, lighting only `WorldShips`.
- `Own ship (3D view)`, a RawImage at the plot centre. Size: 76 px at zoom 1, scaled by zoom,
  held between 30 and 120 px.
- `BoardLinearImage.shader` and its material. The board works in sRGB values, so the shader
  converts the camera image first. The VectorModel pass draws lines on black, so it makes black
  see-through.
- The builder hides the old placeholder rectangles and adds the image, switched off. It turns on in
  play once the ship view has a picture. An image with no picture draws a white square.
- Both ship models are turned 180° about their length. The camera looks from below the world
  plane, so unturned we saw the belly.
- The bow is the wide hammerhead end (turrets point that way; the Art Spec says "hammerhead
  prow"). It leads along the heading.

**Verified.** STEP 37: VERIFIED on the second pass (2026-10-18).

- Red line art at the plot centre, no square, in play. Nothing in edit mode.
- Size matches clamp(76 × zoom, 30, 120) at every zoom tried.
- The top side shows, and the bow leads at six headings, within 0.5°. The ship holds still while
  paused.
- Marks and the contact dot draw over it; the periscope covers it; nothing leaks out of the plot.
- It survives a rebuild.
- Evidence: `Logs/HandoffQA/step37/v/` and `Logs/HandoffQA/step38/`.

**Our ship is white** (owner, 2026-10-18). The camera image is drawn by the old build's VectorModel
full-screen effect as red lines on black, and red means "hostile" in the design palette. The effect
itself stays red: other cameras use it. `BoardLinearImage.shader` instead uses the lines as a mask
and fills them with the image's colour. That colour is `HandoffPlotPanel.ownShipColor`, in the
tuning window under Plot & our ship. Its default is the design's white, `ink.data` #E8EDF0, the
colour of the design's own-ship glyph.

### Step 38 — Every tunable value in one window

**Owner direction (2026-10-17).** Every tunable value lives in the Inspector, gathered in one
place, so the owner doesn't have to hunt through objects. A web app or spreadsheet was raised; one
window inside Unity was chosen: nothing to sync, and it edits the real values.

**What was built.**

- `TuneAttribute.cs`: `[Tune("Section")]` marks a value as tunable.
- `Assets/Editor/StarwolfTuningWindow.cs`, menu **Starwolf → Tuning**. It lists every tagged value
  on the open scene's components, including inside lists (each bus's signal per unit), grouped by
  section. Edits go to the real values and can be undone.
- In play mode it warns that changes are lost when play stops. Tick **Keep play-mode changes** to
  copy them back afterwards. Then save the scene.
- 49 declarations tagged, 65 values shown, in nine sections: Power & signal, Sensors, Helm, Plot
  tools, Plot & our ship, Periscope & fire control, Engineering & orders, Board & time, Motion.

**Rule from now on:** tag every new tunable value with `[Tune("Section")]`.

**Verified.** STEP 38 checked on 2026-10-18.

- 65 values; every one resolves. Labels are unique within each section. Each tag shows exactly once.
- Edits change the real value and undo cleanly.
- In play: without the tick, changes go back when play stops. With it, they are kept. A change
  takes effect on the next board tick.
- The checker found one defect: with the window open, a playtest put two layout errors in the
  console. The window decided what to draw separately in Unity's two drawing passes. It now
  decides once per frame. Re-checked by running the playtest with the window open: 37/37 PASS,
  console clean.

**Known.** `ShipView`'s three values are read when play starts. Changing them mid-play does
nothing until the next run.

### Step 39 — Nothing standing on the plot

**Owner decision (2026-10-18).** The plot carries no standing text or mock art. "It's too much."
The player learns these things through the tools.

- **Gone for good:**
  - our ship's name and call sign (STARWOLF, SW-01);
  - the mock heading and speed line under it (HDG 062°, V 1.84 km/s);
  - the SENSOR RANGE label on the envelope ring (the ring stays);
  - BOARD POINT ALPHA and its ×, a mock waypoint;
  - `drawn#33`, an empty circle that drew nothing;
  - `STAND-IN drawn#35`, a frozen grey line pointing north-west. It is probably part of the
    design's radar sweep: it has the same −58° angle as the sweep disc `drawn#34`, already hidden
    (`HANDOFF_GAPS.md` rows 7, 12, 105). The design's sweep turns round our ship while DRADIS
    (passive sweep) is on, which is the default. It is built for real in the sensors step.
- **Hidden until real:**
  - The PDC GROUP A arc and its label show only once the PDCs are activated. They are wired in
    the fire-control step.
  - The alarm strip ("3 NEW CONTACTS SINCE COMMIT") shows only for a real alert. The board loads
    quiet, as if already acknowledged. The sensors step gives it a real trigger: a new contact
    identified.
- The builder hides them (`WirePlot`, and the alarm strip in the time-control wiring), so a
  rebuild keeps them hidden.

**Checked.** After a rebuild, nothing under `Plot/Mockup` or `AlarmStrip` shows. The World is
still linked and the scene saved clean. Playtest: 37/37 PASS, console clean, our ship white (`Logs/Playtests/2026-10-18_225433/`).

### Step 40 — Press and click: switches slide, refuse, and click

**Owner decisions (2026-10-18).** Bring in the design's press and click interactions; the handoff
is the newest source.

- Rocker switches slide their pad, built now from the cut-out art.
- A locked switch that is clicked gives a small "refused" nudge. This is the owner's addition; the
  design has nothing for it.
- Click sounds, from the prototype. The design board has no sound.
- Unity's audio was switched off for the whole project (since August 2025, no reason recorded).
  It is now on. The intro's hum is now audible too.
- A refused order keeps the design's own reaction (it throws, the lamp goes red for 0.42 s, it
  flicks back), plus the refused sound.

**What was built.**

- `HandoffRockers.cs` on the board, covering all 21 clickable rockers in seven drawers.
  - When a panel swaps a rocker's sprite, a cover the colour of the well (`sprite.well.01`) hides
    the baked pad. Two cut-outs of the old and new pads slide over it, and the new one fades in.
    The slide uses the design's spring curve, cubic-bezier(.3, 1.5, .5, 1), and the fade uses
    ease. A slide lasts 0.14 s on the 30×38 and 24×32 rockers and 0.12 s on the 22×30 and REBOOT
    (UIStateSpec §9.1).
  - At rest the three layers are off, so the board looks exactly as before. The edit-mode board
    renders pixel-identical before and after the rebuild.
  - A change made while a drawer is shut isn't animated when it opens.
  - Pad boxes and travel were measured in the art. The thrown pad carries a glow 2 px wider, so
    the cut-outs include it. The 30×38 thrown pad sits 11 px lower; the spec says 10 (top 3 → 13).
    The art wins, so the slide lands exactly on the baked pad.
  - Refused nudge: if the rocker shows its locked art when clicked, the pad dips 2 px and springs
    back over 0.12 s.
- Click sounds, made in code from the prototype's `playTone` (no audio files):
  - tab (560 Hz square, 35 ms): tool rail, systems keys and latches, helm, orders and periscope
    headers;
  - switch (310 Hz square, 45 ms): every other control that takes a click;
  - refused (150 Hz sawtooth, 80 ms): locked rockers and refused orders;
  - drag-only controls and drawing on the plot make no sound.
- TRACK (fire control) and its lamp drop 1 px while held. The design gives no ease, so it's
  instant.
- RECALL (hangar) eases into its pressed art over 0.1 s and back.
- Tuning window: Motion gains the four slide times, the nudge distance and time, the RECALL
  press, and the TRACK drop. A new **Sound** section holds the three cues: pitch, length,
  volume, and square or saw.

**Checked.**

- The edit-mode board renders pixel-identical before and after the rebuild (`Logs/HandoffQA/step40/`).
- In play, the LOAD SHED pad was logged frame by frame (`slide_frames.txt`): 17 → 28.9 px with the
  spring's small overshoot, then rest at 28, over 0.13 s. The new pad fades in alongside.
- Playtest: 48/48 PASS, console clean (`Logs/Playtests/2026-10-18_234125/`). New checks:
  - the tab and switch clicks;
  - the LOAD SHED slide;
  - drawing on the plot makes no sound;
  - a locked arming rocker nudges with the refused sound and stays locked;
  - TRACK drops 1 px and returns;
  - RECALL eases down and back.

**Known.**

- The cover is one flat colour. The real well has a slight gradient and the thrown pad's red
  glow. It only shows in the strip the pad uncovers during a 0.12–0.14 s slide.
- RECALL's press is a crossfade between the up and pressed art. The design moves the button's
  top 3 → 7 px; the pressed art already has the button lower, so it reads as a press.
- Tuning window: "Keep play-mode changes" does not keep edits to the three sounds or the RECALL
  press. It only keeps single numbers, colours and switches.

### Step 41 — The helm: our ship moves, and the plot pans

**Owner decisions (2026-10-18).**

- Top speed is on the design's scale: about 2.5 km/s at full throttle. The design's contacts run
  0.4–2.6 km/s. Reaching the dummy, 24,300 km away, takes about 7 minutes at ×10.
- The grid stays put and our ship moves across it. The design kept our ship at the plot centre;
  the owner overrode that.
- You can drag across the grid to pan the plot, and use the arrow keys.

**What was built.**

- `HandoffBoardState.TickMove`, run every board tick for `rate` mission seconds (so ×10 moves ten
  times as far per tick). Spec §4:
  - the throttle sets a target speed along the nose, and the ship speeds up and slows down at
    capped rates;
  - the hull turns, but the ship keeps sliding on its old course while the sideways speed bleeds
    off.
  - The numbers are the prototype's, slowed to the owner's top speed: time stretches by
    240 / 2.5 = 96. That gives 0.9 G accelerating and 1.7 G braking, which matches the design
    helm's "1.0 G". Full speed takes about 5 minutes of mission time.
  - All five values are in the tuning window under Helm.
  - Throttle is still held to what ENG power allows, as before. There is no reverse; the board's
    throttle runs 0–100%.
- The board now owns our ship's position. `TacticalWorld` reads the model's start position once,
  then places the model where the board has it every tick. Bearings and ranges to contacts use
  that position.
- `HandoffPlotPanel` has a view centre, which starts on our ship. Marks, contacts and the grid are
  drawn around it. Our ship's image sits where the ship actually is.
  - Dragging the empty grid pans the plot. The plot ground's hit is now `plot.pan`; the mouse wheel
    still zooms there.
  - The marking tools keep their drags for drawing.
  - Holding an arrow key pans at 600 screen px a second (tunable), except while typing in a
    field.
- `HandoffPlotGrid.OffsetPx` shifts the grid lines, so the grid stays fixed in the world.

**Checked.** Playtest 57/57 PASS, console clean (`Logs/Playtests/2026-10-19_000009/`). New checks:

- Putting our ship 5,000 km east moves its image 304.5 px across the plot; marks don't move on
  screen, and the COMPASS re-measures.
- With ENG powered and the throttle at 50%, the ship gathers speed (0.77 km/s after 4 s at ×10)
  and moves along its heading.
- After a 90° turn the course lags the heading (heading 173°, course 140°).
- Paused, the ship stops dead.
- Dragging the grid 100 px pans the view by exactly 100 px worth of km.

**Known.**

- The playtest sets the ordered heading directly; the helm dial is a drag it doesn't do yet.
  Arrow-key panning can't be pressed by a script (the old Input class), so it is untested.
- Turning still runs at the design's 4° per board tick at every time rate, while movement scales
  with the rate. So at ×10 the ship slides further during a turn.
- The axis numbers on the plot's edge (−30 … +30) are fixed. They read as distance from the plot
  centre, not world position. (Fixed in Step 65.)
- Nothing recentres the plot on our ship. Panning is the only way to follow it. (Fixed in Step 65:
  the `C` key.)

### Step 42 — The sensor circle: DRADIS's hearing range around our ship

**Owner direction (2026-10-18).** When DRADIS is on, a circle shows around our ship. Its radius is
the sensor range.

**What happened before.** The design's sensor ring was `drawn#33`, a 398 px circle round the plot
centre: 150 + gain × 48 layer px, the radius the SENSOR RANGE label rode on. The export gave it no
fill and no line, so it never drew in Unity. It was removed in Step 39 as "an empty circle"; nothing
visible was lost.

**What was built.**

- A `Sensor ring (drawn)` layer under our ship (a `HandoffMarksGraphic`, which gains a solid
  `circle` and a filled `disc`).
- `HandoffPlotPanel` draws it around our ship's true position at the true hearing range,
  `PassiveRangeKm`: the range at which `Heard()` takes a contact while DRADIS is on. That is
  12,000 km + 7,000 km per gain step, ×0.55 on UV. So 19,000 km at gain 1.
- It is drawn only while DRADIS is on.
- The colours are the design's `senEdge` and `senGlow`: the edge in data white at 26% and a faint
  fill at 4%. On the amber grid, alarm red at 30% and 5%.
- It eases to a new size over 0.5 s when the gain changes (UIStateSpec §9.1, lines 204 and 210).
  The ease is in the tuning window under Sensors.
- The radius is true to scale, not the design's fixed px. At ×1 a 19,000 km circle is wider than
  the plot, so zoom out to see it (×0.3 fits it).

**Checked.** Playtest 60/60 PASS, console clean (`Logs/Playtests/2026-10-19_001410/`). The circle matches the hearing range with DRADIS on, goes with
DRADIS off, and comes back. A screenshot at ×0.3 shows it centred on our ship.

### Step 43 — The radar sweep

**What the design has.** `rd-sweep`: while DRADIS is on, a sweep turns once every
(5.4 − 0.5 × gain) seconds (UIStateSpec §9.2; board-logic.js `sweepDur`). It has a bright leading
edge (`sweepEdge`, data white at 70%) and a faint tail (`sweepTail`, 9%). On the amber grid they
are red at 75% and 11%. The export only had a frozen gradient stand-in for it (`drawn#34`, hidden;
`drawn#35`, removed in Step 39).

**What was built.**

- `HandoffSweepGraphic.cs`: draws the edge, a line from our ship to the rim pointing north, and
  the tail, a fan fading to nothing 60° behind it. It is drawn once; turning it costs nothing.
- `HandoffPlotPanel` sizes it to the sensor circle and places it on our ship. It turns it
  clockwise at the design's rate, and hides it while DRADIS is off.
- It holds still while the board is paused. The design kept it turning; the owner's rule is that
  nothing moves while paused.
- Tuning window, Sensors section: the tail angle (the design gives none) and the turn time.
- A `Sensor sweep (drawn)` layer between the sensor circle and our ship (builder).

**Checked.** Playtest 62/62 PASS, console clean (`Logs/Playtests/2026-10-23_000343/`). The sweep turned 39° in
0.5 s at gain 1; the design rate gives 37°. It held still while paused and went with DRADIS off. The
×0.3 screenshot shows the edge and its fading tail inside the circle.

**Known.** On the amber grid the tail uses the edge's colour (rgb 255, 150, 130). The design's tail
there is a deeper red (224, 75, 60), at 11%.

### Step 44 — Hearing the enemy: bearing lines, the alert strip, the tracked blink

**Owner direction.** Alerts appear only when there is a real alert (2026-10-18). This step also
delivers Power P2, which was parked here: hearing on the plot.

**What was built.**

- **Bearing lines.** Every contact DRADIS hears but hasn't identified gets a line fading out from
  our ship along its bearing, to the sensor circle's edge, with a "BRG 047" label. A passive
  contact gives a bearing only, no range.
  - Lines and labels are data white at 60%. While the periscope is on the contact, the line is at
    100% and the label turns amber, as the design's `passives` do.
  - Clicking the label still swings the periscope onto the contact (`plot.look`).
  - The label sits 200 px out along the line, or at the circle if that is nearer (tunable). The
    line is a new `fade` kind in `HandoffMarksGraphic`, on a `Passive bearings (drawn)` layer.
  - The export's only passive label (BRG 149, for the removed CONTACT 22) is now the template
    for every heard contact's label.
- **The alert strip.** The board counts each contact once, the first time DRADIS hears it or a
  scan identifies it (`HandoffBoardState.NoteNewContacts`, run every tick). A new one raises the
  strip and sets its count: "1 NEW CONTACT SINCE COMMIT", plural for more. It also plays the
  prototype's alert tone, 170 Hz sawtooth for 0.42 s, tunable under Sound. Clicking the strip
  acknowledges it and clears the count. The design raised the strip once at load and never
  again; this trigger is ours, after the owner's direction.
- **The tracked blink.** While our signal is at or above the tracked line (45%), our ship blinks:
  full for the first half of each 1.1 s, then at 20% (the design's `bd-blink` on its own-ship
  glyph). (Removed in Step 66.)

**Checked.** Playtest 68/68 PASS, console clean (`Logs/Playtests/2026-10-23_000923/`).

- At load nothing is heard and the strip is quiet.
- With SEN powered and gain 2, the hearing range is 26,000 km and DRADIS hears TRACK 07 at
  24,127 km.
- Its line and label read "BRG 047".
- The strip raised "1 NEW CONTACT SINCE COMMIT" with the alert tone. Clicking it cleared it.
- At 100% signal our ship blinked.

**Known.**

- The playtest sets the sensor gain directly; the gain fader is a drag it doesn't do yet.
- The enemy noticing us (Power P3) is still to come. It is the first-mission plan's step B
  (`FTUE_PLAN.md`).

### Step 45 — The bearing trail: plotting a passive contact's range

**Owner decisions (2026-10-23).** The plot tools exist to build an accurate firing solution by hand
(Silent Shark style). A passive contact gives a bearing only. To find its range:

- each sweep leaves a bearing line where our ship was;
- the lines fade out over **2 minutes** of mission time;
- as we move, they cross at the target.

The design's buttons, drawers and flows are unchanged; this is drawing on the plot only.

**What was built.**

- `HandoffBoardState.BearingFixes`: one fix each time the radar sweep passes a contact DRADIS hears
  but hasn't identified. A fix holds our position, the exact bearing (not rounded) and the
  mission time. `bearingTrailSeconds` (120) is under Sensors in the tuning window. Old fixes are
  pruned.
- The plot draws each fix as a line fading out from where we were, out to the hearing range. Its
  strength fades with age; a fresh line is at 45% (tunable).
- The live bearing line now uses the exact bearing too. Its label still shows whole degrees.
- Pausing freezes the sweep, so nothing new is laid. Pressing ERASE twice clears the trail along
  with the marks.

**Checked.** Playtest 70/70 PASS, console clean (`Logs/Playtests/2026-10-23_005602/`). At ×10 two
lines laid 140 km apart crossed 0.0 km from TRACK 07. The first line was gone 140 s later.

**Known.** At 24,000 km a short baseline gives near-parallel lines. The player has to cover distance
or turn across the bearing to get a clear cross. Bearings are exact: no sensor error yet.

### Step 46 — Intercept helpers: time to reach, tracks, weapon reach

**Owner decisions (2026-10-23).**

- A RULER label adds the time to cover its length at our current speed. Nothing while stopped.
- A RULER drawn from one POINT to another snaps to them. It becomes a target's track: course and
  speed, with the course projected **5 minutes** ahead. This settles the old open question of
  whether a RULER snaps to a POINT.
- The selected weapon's reach shows as a **plain dashed amber ring** round our ship, while Fire
  Control is open.
- The design's buttons and flows are unchanged. The RULER keeps its drag; its label carries more.

**What was built.**

- `HandoffBoardState`: `Mark.ptA` / `ptB`, the POINTs a ruler's ends snapped to. Also
  `PointNear` and `TrackLeg`: course runs from the earlier POINT to the later, and speed is
  distance over time.
- `HandoffPlotPanel`:
  - a RULER end within 12 screen px of a POINT snaps to it (tunable);
  - a track ruler's label reads "CRS 063° · 1.10 km/s", and a dashed line runs from the later
    POINT along the course for 300 mission seconds of travel (tunable);
  - other rulers add " · 26 MIN" (or "45 S", or "1 H 12 MIN");
  - while the FIRE drawer is open, the selected mount's `rangeKm` is a dashed ring at 70% amber
    (railgun 30,000 km, PDC 4,000 km, VLS 40,000 km).
  - A drawer click now refreshes the panels, so the ring appears at once.
- `HandoffMarksGraphic`: a dashed line and a plain dashed ring. Builder: a `Weapon ring (drawn)`
  layer.

**Checked.** Playtest 76/76 PASS, console clean (`Logs/Playtests/2026-10-23_010133/`).

- A RULER read "RNG 1,970 km · BRG 090° · 26 MIN" at 1.25 km/s.
- A RULER between two POINTs 40 s apart snapped to both. It read "CRS 063° · 18.36 km/s", the
  exact course and speed of the test points, and drew its projection.
- The railgun ring showed only while FIRE was open.

**Known.** With POINTs close together, the track label can overlap the POINTs' time labels.

### Step 47 — The firing solution dialled by hand

**Owner decisions (2026-10-23).**

- The player dials the firing solution by hand from the plot. TRACK only works after an active
  scan identifies the target, as the design already had it.
- A hand-dialled solution aims at **a point in space**, not a locked contact. You can shoot a
  contact you have only heard.
- With no solution, turning a knob starts one: on the periscope's bearing, at 10,000 km, speed 0.
- How close a solution must be to hit is decided in the fire step.

This changes the design's fire control in one place. Its RNG / BRG / SPD knobs only adjusted a
solution TRACK had made; now they also start one. The layout and controls are unchanged.

**What was built.**

- `Solution.manual`. `NudgeSol` with no solution starts one: `manualStartRangeKm` (10,000,
  tunable), relative bearing from the periscope, speed 0. Then it applies the turn.
- For a manual solution:
  - BEARING (`OnBearing`) wants the periscope on the dialled bearing (heading + relative);
  - SLEW TO puts it there;
  - `AllSet` doesn't need a locked contact.
  TRACK solutions behave exactly as before.
- The readout shows "MANUAL / DIALLED BY HAND". The status line skips "NO TARGET DESIGNATED" and
  "NO TRACKING ID" for a manual solution. Only the words change.

**Checked.** Playtest 81/81 PASS, console clean (`Logs/Playtests/2026-10-23_011103/`).

- TRACK refused a heard-only contact.
- Ten steps of the RNG knob started a manual solution at 11,000 km on the periscope's bearing
  (REL −111°), at speed 0. The readout said MANUAL.
- BEARING was off 10° away from the dialled bearing. SLEW TO put the periscope on it, and it read
  ON BEARING.

**Known.**

- The knobs step 100 km per 8 px of drag, the design's fine-tune pitch. Dialling a long range from
  10,000 km takes long drags; a coarse mode may be wanted.
- The solution holds range, relative bearing and speed but no course. Leading a moving target
  needs one. That belongs to the fire step.
- The relative bearing is against our heading when it is dialled, as the design stores it.
  Turning afterwards moves the aim.

### Step 48 — Fire and hit: the railgun slug

**Owner decisions (2026-10-23).**

- A railgun shot is a **slug that flies at 50 km/s**: the prototype's 4,800 slowed ×96, like our ship.
  It takes about 8 minutes of mission time to reach TRACK 07, about 20 s at ×10.
- It **hits** if it passes within the ship's **hit radius**, set per class from the prototype (km):
  - frigate 95;
  - destroyer 135;
  - cruiser 210 (TRACK 07);
  - carrier 600;
  - anything else 150.
- On a hit, **fire control reports it with the side struck**: "HIT · TRACK 07 · PORT". Otherwise
  "MISS". Each has a sound. The slug shows on the plot as a small dot with a short trail, no label.
  Damage comes in the damage step.
- **Firing spikes our signal**: +25, tunable (GDD p.42). The enemy reacting to it comes with the
  enemy AI.

**From the spec, not new calls:**

- one shot per arming: firing releases ARM (spec 8.4.b);
- no shot below the charge it costs;
- turret arcs are fixed to the hull (spec 8.7.a);
- a slug dies at its weapon's range.

**Settled by a source:** no lead in this step. The dummy doesn't move. Leading from a POINT-to-POINT
track comes with a moving enemy.

**What was built.**

- `HandoffBoardState`:
  - `Fire()` refuses below the shot cost, releases ARM, and for the railgun adds a `Round`: our
    position, the bearing heading + BRG, and the mount's range. It spikes the signal.
  - `TickRounds(dt)` runs every board tick for `rate` mission seconds. It tests each slug's whole
    path that tick, so a 500 km jump at ×10 can't skip past a ship. The first ship passed within
    its hit radius is hit.
  - The side struck is where the slug came from, against the ship's bow: PROW, STBD, AFT or PORT
    (reference sim `sideFacing`). The bow comes from the model's heading (`Track.worldHdg`, set by
    `TacticalWorld`).
  - `InArc`: the solution's bearing from our bow must lie inside the mount's arc. **The ANGLE
    rocker now locks out of arc, with the hint "OUT OF ARC"**: turn the ship, or pick the other
    turret. The rule is new; the switch is the design's.
- Fire control's status line: ROUNDS AWAY while the slug flies, then the report.
- The plot draws slugs in our own-force blue.
- Sounds: firing plays the prototype's railgun cue (90 Hz saw). A hit is a 680 Hz tone and a miss
  the refused buzz; those two are my choice, tunable under Sound.
- Tuning window: slug speed, hit radii by class, the signal spike and the slug's trail length.

**Checked.** Playtest 93/93 PASS, console clean (`Logs/Playtests/2026-10-23_013213/`). Weapons released at CONDITION 2 from the orders station. SEN vented
to free power for WEP, and the gun charged. A hand-dialled solution on TRACK 07's bearing, nearest
whole degree. Turret A was out of arc (ANGLE locked); turret B bore. All four rockers set, and the
lever fired one slug and released ARM. Firing spiked the signal +25. Paused, the slug held still.
The test predicts each shot from the geometry and checks the game agrees:

- In run `2026-10-23_012910` the best whole-degree aim passed 192 km from TRACK 07: **"HIT · TRACK 07
  · PORT"**, with the hit cue.
- In run `2026-10-23_013213` the ship sat a little further off. The best aim passed 223 km away,
  and the game said **"MISS"**, as predicted.
- A second shot 2° off: **"MISS"** both times.

**Known.**

- Aiming is in whole degrees. At ~24,000 km half a degree is ~213 km, just over a cruiser's
  hit radius, so a dead-on shot from that far may still miss. Close in for a sure shot. **The owner
  kept whole degrees on purpose (decision 28).**
- Only the railgun fires a slug. PDC and VLS still only spend charge.
- Our own speed isn't added to the slug. There is no lead and no aim scatter.
- With high power drawn our signal is already at 100%, so the spike has nothing to add.

### Step 49 — Damage: armour, compartments, the kill

**Owner decisions (2026-10-23).**

- **Armour by side.** The prow and flanks take 2 railgun slugs to open (armour 100, a slug takes
  50). The aft takes 1 (armour 50), the weak stern.
- Once a side is open, **each slug takes 2 integrity** (of 5) off one compartment behind it, at
  random among those still standing.
- The target's damage shows on **fire control's report line** for now. A proper target readout
  waits for art.
- **The reactor at zero destroys the ship.** It becomes **a cold wreck**: no longer hostile or
  heard, a dim dot where it died.

Already ruled in `FTUE_PLAN.md` §4.4 (2026-10-18):

- the side facing the shot takes it;
- what sits behind each side: prow BRI; aft RCT and ENG; starboard MAG; port HAB and HGR;
- the enemy uses our damage model and our kill condition.

**What was built.**

- `HandoffBoardState`, the model both ships use:
  - `sideArmourMax` (PROW, STBD, AFT, PORT), `slugArmourDamage`, `slugCompartmentDamage`, all
    under Damage in the tuning window;
  - `TakeSlug(side)`: armour first, then a compartment behind the side;
  - `destroyed`, when the reactor reaches 0.
- `WorldContact.Ship`: each contact has its own `HandoffBoardState`, undamaged at load.
  `TacticalWorld` hands it to the board (`ShipOf`), with our damage numbers from the tuning window.
- `TickRounds`: the struck ship takes the hit on its own model. The report reads "HIT · TRACK 07 ·
  AFT OPEN", "… · AFT ARMOUR 50%", "… · AFT · RCT −2", or "TRACK 07 DESTROYED".
- A destroyed track becomes a wreck: not heard (`Heard` skips wrecks), not hostile, and slugs pass
  through it. The plot draws a dim dot where it died.
- The prototype's kill cue (120 Hz saw) plays for a kill.

**Checked.** Playtest 99/99 PASS, console clean (`Logs/Playtests/2026-10-23_080215/`). TRACK 07
started undamaged. The fight, from 10,000 km behind its stern (stopped, side-on so turret B bore):

1. "HIT · TRACK 07 · AFT OPEN"
2. "HIT · TRACK 07 · AFT · RCT −2"
3. "HIT · TRACK 07 · AFT · RCT −2"
4. "TRACK 07 DESTROYED"

Its reactor went to 0, its engines stayed at 5, and nothing else was touched. It went cold, and the
kill cue sounded. The earlier long-range shot correctly missed by 224 km.

**Known.**

- The fight test puts our ship in place directly, standing in for flying there with the helm.
- Hits on our own ship come when the enemy shoots back (`FTUE_PLAN.md` step J). Our own sides and
  compartments use the same code.
- The plot view doesn't follow our ship: after a long move you pan to find it.

### Step 50 — Engineering's ship diagram

**Owner decisions (2026-10-23).** The same picture of our ship as on the plot appears in the
engineering drawer:

- it replaces the design's line-and-blocks diagram;
- it is always bow up;
- a lamp on the hull shows each compartment, green, then amber, then red as its integrity drops.

**What was built.**

- The builder hides the design's diagram (`drawn#230` line, `#231`–`#236` blocks) and keeps its
  dark box (`#228`/`#229`). Over the box it adds `Ship diagram`: a RawImage of the plot's ship
  view, plus six lamps (`lamp_dot`).
- `HandoffEngineeringPanel.ShowDiagram`, on every refresh:
  - the picture shows `ShipView`'s texture in our ship's white;
  - it is turned back by our heading, so the bow is always up (no second camera);
  - its size is set by `diagramShipPx` (190);
  - lamps follow `CompHealth`: 4–5 green, 2–3 amber, 0–1 red;
  - lamp spots: bridge at the bow, magazine starboard, habitation and hangar port, reactor and
    engines at the stern. They follow the side-to-compartment map in `FTUE_PLAN.md` §4.4.
  Spots and size are tunable under Engineering & orders.

**Checked.** Playtest 103/103 PASS, console clean (`Logs/Playtests/2026-10-23_081716/`).

- With the drawer open, the picture showed our ship turned back 90° for heading 090°, bow up.
- Six lamps were green.
- With HAB at 2 and HGR at 1 (a stand-in for enemy hits), their lamps went amber and red, the
  others stayed green. Restored, they went green again.

**Known.**

- The wide hammerhead spills a little past the old dark box, still inside the screen.
- The lamp spots are placed by eye on the model (tunable).
- The target's own diagram (seeing the enemy's damage this way) waits for the target readout.

### Step 51 — The enemy moves: TRACK 07 patrols

**Owner decisions (2026-10-23).**

- The order from here: an enemy that moves; leading a moving target; then missiles, fighters,
  PDCs and the hangar launch.
- **TRACK 07 moves.** It **patrols a loop of waypoints** at **1.1 km/s** (the design's speed for
  it) and **does not react to us yet**.

This is `FTUE_PLAN.md` step A, done the plan's way: a brain that only gives orders, and the ship's own
`HandoffBoardState` moved by the same code as ours.

**What was built.**

- `EnemyBrain.cs`, on `Dummy · TRACK 07`:
  - its route is a loop of points, km from where it starts. The default diamond runs (0,0),
    (−6000,−4000), (0,−8000), (6000,−4000), from its start toward our side and back;
  - `cruiseKms` (1.1) and `arriveKm` (300);
  - `Think()` sets the ordered heading toward the current waypoint and the throttle for cruise
    speed, and moves on to the next waypoint on arrival.
  All three are under Enemy in the tuning window.
- `TacticalWorld`:
  - at load, each contact's ship starts where its model is, facing its way, engines powered;
  - each board tick, for each contact with a brain, it runs `Think`, `TickHelm` and
    `TickMove(rate)` — the same turning, acceleration and drift as ours — then places its model;
  - the track's `speed` is now its real speed, which TRACK reads;
  - a destroyed ship cuts its throttle and coasts to a stop.
- Paused, there is no tick, so it holds still.

**Checked.** Playtest 107/107 PASS, console clean (`Logs/Playtests/2026-10-23_085004/`).

- After 3 s at ×10, TRACK 07 had gone 34 km and was making 0.71 km/s, still speeding up.
- It was ordered onto 236° for its first waypoint and was still turning (heading 308°).
- Its model followed its ship model, and the board's track read its real speed.
- The older checks (the bearing trail crossing at the target, the hit, the fight) still assume a
  target that holds still. Until leading lands in Step 52, the playtest stops TRACK 07 after the
  patrol check, and holds our own ship still during the patrol burst.

**Known.** At load our throttle sits at the design's 40%, so once ENG is powered our ship gets under
way on its own.

### Step 52 — Leading a moving target

**Owner decisions (2026-10-23).**

- A hand-dialled shot is led by **fire control, from your plotted track**: the latest
  POINT-to-POINT RULER gives the target's course and speed. Your plot's accuracy decides the hit.
- **TRACK leads exactly** once an active scan has identified the target, as the reward for going
  loud.
- No new controls. Drawing a track copies its speed into the existing SPD readout.

**What was built.**

- A `Solution` now carries:
  - `hasCourse` and `course`;
  - where the target was (`wx`, `wy`) and when (`t`, mission seconds) as the solution was set.
- The solution is set in three places:
  - a hand-dialled solution records the dialled point at each knob turn (`Fix`);
  - the latest track (`ApplyTrack`) gives it course and speed, when the solution starts or when a
    track is drawn;
  - TRACK takes the target's true position, course and speed from the lock.
- `LeadBearing()`, at the moment of firing:
  - it reckons the target forward from the solution's point and time to now;
  - it solves where the 50 km/s slug meets it, and fires on that bearing;
  - without a course, the shot flies straight down the dialled bearing, as before.
- `HandoffBoardControls` keeps the board state's clock current (`missionNow`). `TacticalWorld` gives
  each track its real course over the ground (`Track.course`).

**Checked.** Playtest 113/113 PASS, console clean (`Logs/Playtests/2026-10-23_090720/`). Run against the patrolling TRACK 07, steady on its leg, from 12,000 km
off its starboard beam:

- Without a track, the solution had no course.
- Two POINTs on it two minutes apart, joined by a RULER, fed the solution exactly the track's
  course and speed.
- The led shot hit. Aimed straight, the same shot would have passed 295 km off and missed; led, it
  passed 113 km off. The plotted course read 250°, against 244° at that instant: after its turn the
  ship was still sliding onto its new course, so the plot shows the average. (In an earlier run: 438
  km unled, 33 km led.)
- After a lock, TRACK carried the target's course, and its led shot hit too.

In the first run TRACK 07 was still turning onto its leg while it was plotted. The track then gives
its average course over the two minutes (256° against 248° at that instant), and the shot still hit
(`2026-10-23_085847`). The test now waits for a steady target.

**Known.**

- The playtest places the two POINTs on the target's true positions, standing in for good
  plotting; the plot itself is tested in Step 46. It dials the range directly.
- If the target turns after it was plotted, the lead is wrong until it is re-plotted, as at sea.
- The solution's point is fixed when the knob is last turned: an old solution is reckoned forward
  on the plotted course.

### Step 53 — Missiles: the launcher, homing, three warheads

**Owner decisions (2026-10-23).**

- Missiles fly at **20 km/s**: slower than a slug, but they home. About 10 minutes to 12,000 km.
- They **home on a locked target**, which needs an active-scan identification (GDD p.45). No
  leading needed.
- **The warheads split:**
  - HE hits armour hard;
  - PENETRATOR ignores armour and goes straight to a compartment behind the side;
  - EMP does no hull damage but knocks the power out.
- **Salvos stay** (1–8, using the launcher's 8 cells).
- The design's warhead selector and salvo stepper were never in the export, so:
  - **the turret knob picks the warhead** while MISSILE is selected (there is only one launcher);
  - **the salvo is set in the tuning window** (default 2) until the stepper has art.

**What was built.**

- `Round` is now a slug or a missile: it carries its speed, whether it is a missile, its target
  and its warhead.
- `Fire()` with MISSILE (`MS`):
  - it refuses without a lock or a cell (`MissileReady`), before spending charge;
  - it launches `salvo` missiles, fanned 3° apart, and counts the cells down;
  - it spikes the signal.
- In `TickRounds` a missile steers toward its target, at up to 1.3° per mission second (the
  prototype's 2.2 rad/s slowed ×96). The path test and hit radius are the slug's.
- `TakeWarhead` on the struck ship:
  - HE takes 100 armour, which opens any side in one hit, then compartments −2;
  - PEN takes a compartment behind the side −2, whatever the armour;
  - EMP drains every bus to 0 for 60 mission seconds (`empUntil`). The ship's power can't refill
    until then (`TickPower` holds it at 0; a contact's engines come back when it ends).
- Reports read "HIT · TRACK 07 · STBD · HE · MAG −2", "… · EMP · POWER DOWN", "… · PEN · MAG −2".
- Fire control:
  - with MISSILE the turret knob cycles HE → EMP → PENETRATOR, which also clears the ladder, as
    the design says;
  - the readout shows "HE · SALVO 2 OF 6" (cells left).
- Tuning window: missile speed, turn rate and spread, HE armour damage, EMP time, salvo.

**Checked.** Playtest 121/121 PASS, console clean (`Logs/Playtests/2026-10-23_221627/`). Against the
patrolling TRACK 07, with a lock (stand-in for an active scan):

- An HE salvo of two left the launcher (6 cells left). Both homed in and hit its already-open
  starboard side, and its magazine went 5 → 1.
- The knob picked EMP: "POWER DOWN". With no power it slowed from 0.93 to 0.27 km/s.
- PENETRATOR: "PEN · MAG −2".
- Four cells used.

**Known.**

- Missiles are drawn like slugs: a dot and a trail.
- Nothing can shoot them down yet. PDCs come later.
- The knob doesn't turn to show the warhead; the readout line does.
- No reload: 8 cells per mission.

### Step 54 — Fighters: launch, fly out, attack runs, recall

**Owner decisions (2026-10-23).**

- Fighters fly at **8 km/s**, about three times our top speed.
- **Attack runs:**
  - DESTROY: every 30 mission seconds each fighter takes 10 armour off the side facing it. Once
    that side is open, it takes 1 off a compartment behind it.
  - DISABLE: the same, but always at the engines (AFT armour, then ENG).
  - ESCORT: the squadron stays with our ship.
- **On the plot:** one small wedge per fighter, in own-force blue, in a loose V, pointing where
  they fly. No labels.
- **RECALL brings them home.** They turn back to our ship and land; the squadron is then ready
  again. Fuel comes later.

**What was built.**

- `HandoffBoardState`:
  - `LaunchSquad()` now also puts a `Flight` in the air: the squadron's ready craft, its order,
    fire control's target, starting at our ship on the launch bearing.
  - `TickFlights()`, each board tick:
    - the flight flies toward its target, or home, or with us on ESCORT;
    - inside 200 km of the target it makes its attack runs (`TakeFighterRun` on the target's own
      ship model);
    - if the target is destroyed, it turns home;
    - home and inside 50 km, it lands: the squadron is aboard and the launch stages clear.
  - `RecallSquad()` with a flight out turns it home instead of removing it at once.
  - `FlightStatus()` gives the hangar's telemetry line: "OUTBOUND TO TRACK 07 · 1,592 KM",
    "ATTACKING TRACK 07 · PROW ARMOUR 60%", "RETURNING · 1,820 KM", "ESCORTING".
  - A destroyed target is marked a wreck through the same `Kill()` as a slug.
- `HandoffPlotPanel` draws the fighters from a hidden template (`fighterTemplate`, the builder's
  "Fighters" layer, `hull_wedge_filled`). Size and V gap are in the Tuning window.
- `HandoffHangarPanel`'s telemetry line shows `FlightStatus` while the squadron is out.
- Tuning window, "Hangar & fighters": speed, attack range, seconds between runs, armour and
  compartment damage, wedge size and gap.

**Behaviour change, as the owner decided.** Before, RECALL ended a launch at once. Now it turns the
fighters home and they land when they reach us. The telemetry line used to read "SQUADRON AIRBORNE —
RECALL TO RECOVER" while out; it now says what the flight is doing.

**Checked.** Playtest 130/130 PASS, console clean (`Logs/Playtests/2026-10-23_231846/`). With TRACK 07
held still and our ship 2,000 km off its bow:

- ENG and WEP were drawing all 10 units, so the catapult refused ("NO AUX UNITS"). Venting ENG
  (we were stopped) gave AUX 3 units.
- The six launch stages ran to 100% and the launch lever put four interceptors on the plot.
- They flew out at 8.00 km/s ("OUTBOUND TO TRACK 07").
- Their first run took PROW armour 100 → 60, which is 4 × 10 ("ATTACKING TRACK 07 · PROW ARMOUR 60%").
- RECALL turned them home; they landed, the stages cleared, and there were no runs after the recall.
- The fight to the kill that follows still passed unchanged.

**Known.**

- The outline wedge (`hull_wedge_outline`) vanishes at 16 px, as its lines go under a pixel, so the
  filled one is used. Same shape.
- The fighters fly to the target's true position even when we only hear it. The plot shows them
  flying down the bearing line.
- The playtest doesn't run DISABLE or ESCORT; their code paths are simple branches of the same
  tick. It launches with a stand-in for typing the launch bearing.
- No fuel, no losses, no enemy PDCs shooting them down yet.
- The assault boats' SECURE order attacks like DESTROY (boarding is out, decision in memory).

### Step 55 — PDCs, and the enemy fires back

**Owner decisions (2026-10-24).**

- **TRACK 07 fires back.** When we hit it, and we are inside its launcher's range, it fires a
  missile salvo at us. This is the first small piece of the ship AI.
- **Arming the ladder puts a PDC group on AUTO.** Select PDC, pick group A or B with the turret
  knob, and arm the ladder. The group then shoots by itself at anything in its arc until you drop
  ARM. Its arc shows on the plot while it is on. No new controls.
- **A chance per missile.** Each enemy missile that enters an AUTO group's arc and range gets one
  roll: 70% it is shot down (Tuning window).
- **Enemy PDCs come later,** with the ship AI. Our missiles and fighters work as before.

**What was built.**

- `HandoffBoardState`:
  - `Incoming`: enemy missiles flying at us. `LaunchAt()` adds a fanned salvo; `TickIncoming()`
    lets the PDCs try, then steers each missile at us. One that passes within `ownHitRadiusKm`
    (210) strikes the side facing it through the same `TakeWarhead` as our missiles
    ("MISSILE HIT · PROW · HE · BRI −2").
  - `pdcAuto[2]`, for PDC-A and PDC-B. Throwing ARM with a group selected turns it on; dropping
    any rung turns it off. Switching weapons leaves it on; coming back to that group shows its
    ladder still armed.
  - `RungGated()`: one place for the ladder's gates, used by `Flip` and by the fire-control panel.
    A PDC group needs only WEP power and its autoloaders: no solution, no ANGLE or BEARING gate.
  - `PdcStops()`: one roll per group per missile, needing WEP and the autoloaders.
  - Sides now come from one helper, `SideAt`, for slugs, fighters and incoming missiles.
- `EnemyBrain`: if its armour or a compartment dropped since the last tick, it fires `fireBackSalvo`
  (2, HE) at us, at most once every `fireBackSeconds` (120), while it has cells. Its switch
  `firesBack` is in the Tuning window.
- `HandoffPlotPanel`: enemy missiles show as red dots with a trail. Each AUTO group draws its arc as
  a dashed amber sector out to its range (the design's PDC arc colour, accent.caution at .45). No
  label.
- Fire control's status line reads "PDC-A AUTO · LAST: PDC-A · MISSILE DOWN".
- Being hit plays the alert tone.

**Behaviour changes, flagged.**

- **Autoloaders start ENGAGED** (`loaders = true`). The design starts them off, but its LOADERS
  button was never in the export (HANDOFF_GAPS row 73). Without it PDCs could never arm. The switch
  is in the Tuning window until the button has art.
- **With PDC selected, the fire lever does nothing.** The guns fire by themselves on AUTO.
- **PDC's ladder no longer needs a firing solution, ANGLE or BEARING.**

**Checked.** Playtest 138/138 PASS, console clean (`Logs/Playtests/2026-10-24_002347/`). With our
ship stopped 6,000 km off TRACK 07's bow, turned so it bears 042° (PDC-A's arc):

- The ladder armed on PDC-A with no solution, and PDC-A went AUTO.
- A stand-in hit on TRACK 07 made it fire two HE missiles at us (its cells went 8 → 6).
- PDC-A shot both down, at a stand-in certain chance; nothing hit us.
- On the railgun PDC-A stayed on AUTO. Back on PDC-A, its ladder still showed armed.
- With ARM dropped, the next salvo got through: two hits ("MISSILE HIT · PROW · HE · BRI −2"),
  with the alert tone.
- The fight to the kill and the engineering checks that follow still pass. The test repairs our
  ship first, as a stand-in for damage control.

**Known.**

- The playtest stands in for the railgun hits that trigger the salvo, and for a certain PDC; the
  70% chance itself isn't tested.
- Enemy missiles show on the plot even when our sensors couldn't see them.
- Nothing happens if our reactor reaches 0: no game over yet.
- TRACK 07 fires whatever its own power state is.
- PDC-B covers astern-port (200° ±35°); nothing covers dead ahead or dead astern, as the design
  has it.

### Step 56 — The intro: flat black, typed in place, straight cuts

**Owner direction (2026-10-17, restated 2026-10-24).** The intro screen must be flat black. Each
line appears in place, left to right, as if typed. Then a straight cut to the next thing. No
zoom: the old pull-back shrank STARWOLF in place.

**What was wrong.** The green was the intro package itself. Step 27 gave its shader a faint lit
field (`_FieldGlow` .006 of the phosphor, plus the `#010601` background). On screen it measured
(45, 77, 45), a mid green: the picture is still encoded twice on its way to the screen, which
brightens anything near black.

**What changed** (`Assets/StarwolfIntro/`, owner-supplied, changed at the owner's request):

- `PhosphorDisplay.shader`:
  - the field is pure black; `_FieldGlow` and `_Background` are gone;
  - SECTION 36, INITIALISING... and STARWOLF each type in by cell column (`typed()`, driven by
    `_Reveal`), the newest column fading up;
  - the pull-back zoom and its grid are gone; the menu draws alone.
- `StarwolfIntroController`:
  - `pullbackDuration` is gone; the menu follows the title directly;
  - `typeShare` (0.5): each line types over this share of its time, then holds;
  - the old fade in and out is gone.
- Timing in `00_Start` is unchanged: 4 / 2 / 4 s, then the menu (10 s, was 14). After the reorder
  below, INITIALISING... no longer plays here, so the menu comes at 8 s.

**Checked** in play mode, screenshots measured: the background is (0, 0, 0) on all three lines
and on the menu. SECTION 36 shows half typed, STARWOLF fully typed in place, and the menu has no
title behind it.

**Known.**

- The double encode is still there, so the text is a little paler than the reference
  (`References/Company Logo.png`). Only black is immune to it.
- `00_Start` still holds an unused `pullbackDuration: 4` line; Unity drops it on the next save.

**The order, changed the same day (owner 2026-10-24).** The flow is now:

**SECTION 36 → STARWOLF → menu → NEW GAME → beta page → Enter → INITIALISING... → battle map.**

- `StarwolfIntroController.initialisingOnly`: with it ticked, the controller types only the
  INITIALISING... line, holds, then calls its new `finished` event. Without it (in `00_Start`),
  the intro is SECTION 36 then STARWOLF then the menu: 4 + 4 s.
- `01_Briefing` has a new inactive object, `INITIALISING`, with that controller (2 s), and
  `finished` wired to `ScreenFlow.Advance()`.
- `ScreenFlow.showFirst`: on Enter, it shows that object first instead of loading at once.
- Checked in play mode: shots at 2.5 s (SECTION 36), 5.2 s (STARWOLF) and 8.3 s (the menu). In
  `01_Briefing`, INITIALISING... typed in over black, then `02.1_Grid` loaded.

A trap: a screenshot taken right after `SeekPreview()` lands about half a second later than the
time asked for (the editor runs a few frames first). A shot asked for at 3.5 s showed STARWOLF's
first letter, not SECTION 36.

### Step 57 — Everything in game time

**Owner decision (2026-10-24).** Every rate runs per game second, charging included. At x10
everything is ten times as fast, together.

**Why.** The board's per-tick amounts (weapon charge, AUX, bus fill, the bow's swing, launch stages,
the damage-control party, signal) were applied once per 0.42 s tick whatever the rate. At x10, guns
recharged ten times faster in game time than ships moved.

**What changed** (`HandoffBoardState`):

- `TickPower`, `TickAux`, `TickHelm`, `TickWeapons`, `TickLaunch`, `TickParty` and `TickSensors`
  take the tick's game seconds (`rate`), as `TickMove` already did.
- Whole-unit steps (a bus filling one unit, the bow swinging whole degrees) bank the fractions, so
  x0.25 is exact too. The reactor cap still holds at once.
- The party works `partySeconds` (6) game seconds; it was `partyTicks`. The engineering line reads
  "n OF 6" in seconds.
- `TacticalWorld` passes the rate to the enemy's helm. The radar sweep turns in game time, so x10
  sweeps ten times as fast.
- **x1 is unchanged:** a tick at x1 is one game second, so the design's numbers now read as per game
  second. (Step 63 later made everything that moves 5x.)

**Checked.** Playtest 138/138 PASS (`Logs/Playtests/2026-10-24_104850/`).

Two runs failed first:

- One because someone clicked the engineering key during the run; the fire-control drawer closed
  under the test.
- One because game time changed our ship's path in the helm step, so an earlier shot hit TRACK
  07's bow. The fighter check had assumed an untouched bow; it now works from whatever the bow
  holds.

**Damage plan.** The damage model is written up in `DAMAGE_README.md`, next to this file. It is
built in the order given there: side map, conditions, teams, breakdowns, displays. Boarding comes
right after conditions.

### Step 58 — The audit's side map; our ship can die

**Owner decisions (2026-10-24).**

- **The design audit's map** replaces the 18 Oct one: bow → magazine, stern → engine room, port →
  reactor, starboard → hangar. One fixed room per face. Bridge and habitation sit inboard and are
  never hit directly.
- **A reactor at 0 destroys any ship, ours included.** This overrides the audit's "never
  unrecoverable".

**What changed.**

- `HandoffBoardState.BehindSide` is one room per face. `Behind()` has no random pick; a wrecked room
  reports "· RCT WRECKED" and takes no more.
- **Destroyed** means the reactor's structure is at 0. It used to read `CompHealth`, which counts a
  vented room as 0, so venting the reactor would have destroyed the ship. (Step 65: staying outside
  the battle space too long also sets `destroyed`, with `deserted`.)
- Our ship, when destroyed, goes dark: `TickPower` holds every bus at 0. With no power there is no
  thrust and no weapons. DRADIS still listens at the lowest gain: hearing needs no power. A destroyed
  enemy becomes a cold wreck instead.
- Fighters on DISABLE still go for the stern, which is now the engine room alone.
- The engineering diagram's lamps follow the deck plan in `DAMAGE_README.md`.
- `FTUE_PLAN` §4.4 points to the README.

**Checked.** Playtest 139/139 PASS (`Logs/Playtests/2026-10-24_105807/`):

- The fight to the kill now comes off TRACK 07's port beam: "PORT ARMOUR 50% / PORT OPEN /
  PORT · RCT −2 / PORT · RCT −2 / DESTROYED". Nothing else on it was touched.
- Missiles from its starboard beam hit the hangar ("STBD · HE · HGR −2").
- New check: our port side opened and three slugs into our reactor destroy our ship, and its power
  drops to 0. This is a stand-in for enemy fire.

**Known.** Nothing happens on screen when our ship is destroyed beyond going dark. The game-over
screen is still to be designed (`DAMAGE_README.md`, still open).

### Step 59 — Conditions: fire, vacuum, radiation

**Owner decisions (2026-10-24).** The rules are in `DAMAGE_README.md`, with the design audit §09.5 as
the base:

- The three conditions spread over the owner's deck plan.
- **The weapon and the room decide what a hit starts:**
  - HE and penetrators start fires;
  - slugs breach a room;
  - reactor hits leak radiation.
- **More hits on a wrecked room** still start conditions.
- **The audit's rules, on the design's own buttons.** DUMMY 1 becomes **RECALL**, with the REPAIR /
  SEAL / VENT rocker art and its own legend. DUMMY 2 stays as it is.

**What was built** (`HandoffBoardState`; the same model runs both ships):

- **Room state.** Each room carries `fire`, `rad` and `doorsShut` next to `vented`. The design's
  `cond` is derived from them: FIRE before RAD.
- **What a hit starts.** `Strike()` rolls the condition chances on every hit that reaches a room.
  The report says what started: "· MAG −2 · FIRE", "· HGR −2 · BREACH", "· RCT −2 · RADIATION".
- **`TickConditions(dt)`**, per game second:
  - fire takes 1 integrity every 30 s;
  - radiation kills one crew member every 20 s;
  - all three spread to neighbours: fire only into rooms with air; vacuum only from rooms whose
    doors are open; radiation only while the reactor leaks (below integrity 3, not vented);
  - radiation fades 90 s after the reactor is contained;
  - a reactor burned to 0 destroys the ship.
- **A breach** vents the room, puts out its fire, kills half its crew, and loses any team working
  there.
- **Engineering's rockers (and the orders station, now on the same rules):**
  - REPAIR sends the team. It puts a fire out, or restores the air in a sealed room, or repairs.
    It no longer needs crew in the room; it refuses an open airless room with "SEAL THE DOORS FIRST".
  - SEAL shuts an airless room's doors.
  - VENT dumps the air: the fire goes out, all crew inside die, and the doors shut.
  - REBOOT adds 1 and no longer clears a condition.
  - RECALL calls the team back.
- **The enemy's ship** runs the same conditions with our Tuning numbers (`CopyDamageTuning`). One
  whose reactor burns out becomes a wreck (`NoteWrecks`).
- Tuning window, new section **"Damage control"**: every chance, rate and time in the README.

**Behaviour changes to the design, flagged.**

- SEAL no longer restores air at once.
- REBOOT no longer clears conditions.
- REPAIR no longer needs crew aboard.
- The orders station's VENT no longer refuses with crew aboard, and its PARTY can work an airless
  room once sealed.
- DUMMY 1 is RECALL.

**Checked.**

- A quick check on a bare ship model first, all rules.
- Then playtest 153/153 PASS (`Logs/Playtests/2026-10-24_111146/`), new step 24. Chances and spread
  are set to certain for exact results, and the hits are stand-ins:
  - HE through the open bow set the magazine on fire. It spread to the bridge, reactor and hangar,
    and not to the engines or habitation. It burned the magazine 3 → 2.
  - A team put it out. VENT put out the bridge fire and its 9 crew died.
  - A slug breached the hangar ("HGR −2 · BREACH", crew 7 → 3). REPAIR refused until SEAL; with
    the doors shut nothing spread; the team restored the air.
  - RECALL called a team back with no repair landing.
  - Two slugs into the reactor leaked radiation into the magazine and bridge, not the hangar, and
    it killed magazine crew. Contained, it faded.
- The engineering screen shows it with the design's own readouts (ATMO VENT, COND FIRE, crew 0, "3
  COMPARTMENTS IN HAZARD").
- Steps 1–23 run with the condition chances at 0 (a stand-in), so they stay as written.

**Known.**

- A room with both fire and radiation shows FIRE only, and doors shut on an airless room still show
  VENT. Both wait for the displays step.
- Teams take no travel time and no crew yet (the teams step).
- Crew killed in a room are not replaced or moved.
- The enemy does not fight its own fires: its rooms burn until we finish it, or a fire reaches its
  reactor.

### Step 60 — Boarding: the assault boats

**Owner decisions (2026-10-24).** The rules are in `BOARDING_README.md`, next to this file.

- **The design's three orders, exactly:** DESTROY blows up its reactor; DISABLE wrecks its engines
  and magazine; SECURE captures it.
- **Boarding needs** one face open and the ship nearly stopped: fight it down first.
- **The fight aboard is time and odds:** 60 s, marines against a quarter of its crew, with losses
  on both sides.

**What was built** (`HandoffBoardState`):

- **Launch.** The ASSAULT BOAT squadron flies as a `Flight`, like the fighters, at 6 km/s. Each boat
  takes 6 marines from the ship's 18. The launch refuses with "NO MARINES ABOARD" when none are left.
- **`TickBoarding`:**
  - the boats hold off with a reason ("HOLDING OFF TRACK 07 · NO OPEN SIDE" or "TARGET UNDER WAY");
  - they dock and fight ("BOARDING TRACK 07 · 42 S");
  - they win or are repelled ("BOARDING REPELLED"), then fly home with the outcome on the hangar
    line;
  - survivors rejoin the pool when they land.
- **SECURE** marks the ship and its track captured. The enemy stops and holds still
  (`TacticalWorld`), and the plot shows it as an own-force dot (placeholder).
- **RECALL** while aboard withdraws the marines with no result.
- Tuning window, "Hangar & fighters": boat speed, marines, docking distance, speed limit, fight time,
  defender share, losses.

**Checked.**

- On bare ship models, all three orders: SCUTTLED (wreck), DISABLED (ENG and MAG 0, alive), SECURED
  (captured, not hostile). In each case the boats held off with no face open, and the marines came
  home.
- Playtest 160/160 PASS (`Logs/Playtests/2026-10-24_120447/`), new step 21f, through the hangar's
  controls:
  - three boats launched on DISABLE with 18 marines;
  - they held off while no face was open (a stand-in), and boarded once one was;
  - they won (stand-in: no defenders, so the result is certain) and disabled TRACK 07;
  - they flew home with "TRACK 07 DISABLED · RETURNING" and landed with all 18 marines;
  - the fight to the kill that follows still passed.

**Known.**

- Salvage and what a prize is for, fuel, and enemy boarders are not built (`BOARDING_README.md`,
  still open).
- The boats are drawn as the fighters' wedges.

### Step 61 — Scan to know what you hit; smoothing; heading 000 at load

**Owner (2026-10-24).**

- "You have to scan the ship to even be able to understand what you're damaging."
- Use what is there, with no new art: engineering shows only our own ship, and the periscope is
  really the targeting computer.
- Separately: the room rings and bars looked grainy; and the ship should start pointing up.

**What changed.**

- **One naming rule, as the design's sensors board already had it.**
  - A contact is "BOGEY T07" until an active scan identifies it, then "TRACK 07" (`NameOf`,
    `Scanned`).
  - Hit reports, the kill line, the hangar's lines and boarding outcomes all follow it.
  - Unscanned, a hit reads only "HIT · BOGEY T07"; scanned, it gives side, room and condition.
- **The targeting computer's SUBJECT box,** for a scanned subject, uses its own two lines:
  - "TRACK 07 · CRUISER" (the class once CLASS is pressed);
  - its hull: "BOW 100 · STBD ▸ HGR 3 · AFT 50 · PORT 50". Each face shows its armour %; through an
    open face (▸, as engineering's lines use it) it shows the room behind and its integrity.
  - The name under the brackets follows the scan rule.
- **Smoothing.** The board was drawn into a render texture with no anti-aliasing, and the board
  camera had MSAA off. So every shape the board draws itself came out hard-edged and grainy next to
  the browser design: the compartment rings, bars, plot lines, the sweep, wedges. Now 4× MSAA
  (`GammaBoardPresenter.msaaSamples`; the builder allows MSAA). The design's art and text are
  unchanged.
- **Heading 000 at load** (the design's demo had 083).

**Checked.** Playtest 161/161 PASS (`Logs/Playtests/2026-10-24_154633/`):

- The first two hits read "HIT · BOGEY T07", before any scan.
- After a stand-in scan, the targeting computer read "TRACK 07" / "BOW ▸ MAG 0 · STBD ▸ HGR 0 · AFT
  100 · PORT 100".
- Rings checked zoomed in: smooth edges.

Two runs before it failed: the window was used mid-run, then Unity was restarted mid-run.

**Trap.** The playtest wait loop picked the newest run by modification time. The old run folders
were touched when their videos were deleted, so it read an old report. Pick the newest run by name.


### Step 62 — Weapon reach, the sweep's inner circle, the enemy's picture

**Owner decisions (2026-10-24).**

- **Weapon reach equals the DRADIS at full gain:** railgun and missiles reach 47,000 km (12,000 +
  5 × 7,000). It doesn't depend on the gain set now. You can dumb-fire that far; a lock still needs
  a scan. The design had 30,000 and 40,000. PDCs stay at 4,000.
- **A small circle round our ship.** The sweep runs from it to the DRADIS edge, not from the ship.
- **Enemy picture after CLASS.** Once scanned and classified, a contact shows on the plot as its
  own 3D model seen from above, in red, turned to its heading, instead of its dot.

**What changed.**

- `HandoffBoardState.mounts`: RGA, RGB and VLS reach 47,000 km.
- `HandoffSweepGraphic.SetInner` draws the band and the edge from an inner radius.
- `HandoffPlotPanel` draws the inner circle, half our ship's drawn length plus `innerRingMarginPx`
  (8). It also shows `contactShipImage` for a classified contact, with its label pushed clear.
- **New in `02.1_Grid` (World):** "Contact View Camera · TRACK 07", a copy of our ship's view camera
  aimed at TRACK 07's model (StarWolf_Cruiser). The builder adds a "Contact ship (3D view)" layer.
- `ShipView.Own()`: our ship's picture (plot and engineering) now finds our own camera, not any
  ShipView.

**Checked** in play mode, with a screenshot:

- the inner circle and the sweep starting from it;
- our ship pointing up;
- TRACK 07 drawn as a red cruiser, turned to its heading.

**Playtest:** first run with Step 63 (163/163). The owner's laptop had died before it could run here. It has two new
checks (CLASS shows the picture; the sweep starts at the inner circle) and the updated range check.
Run it first next session.

### Step 63 — Scale: everything that moves 5× faster; ACTIVE scans; the gain comes back (undone in Step 64)

**Why (owner, 2026-10-24).** After playing: the ships felt too slow, plotting gave no data, and the
active scan "didn't seem to be working".

**Played and measured.**

- **In the game, before the change:**
  - at x1, 30 real seconds at full throttle moved our ship 3 px and TRACK 07 4 px;
  - powering up quietly dropped the DRADIS gain from 2 to 1 for good, so TRACK 07 (24,300 km)
    fell out of the 19,000 km DRADIS;
  - ACTIVE on identified nothing: only letting go of the cone dial, or clicking a contact-board
    row, did.
- **The scale study.** `docs/scale-sim/` (new, kept as a tool; README there) played whole fights at
  several speed scales:
  - at the old speeds a win took 35 minutes at x1, and two marks read a course only to ±20°;
  - at 5×, about 7 minutes and ±4°; at 10×, about 4 minutes;
  - engagement range barely mattered;
  - at x5 and x10, people arm too slowly and TRACK 07 can win.

**Owner decisions.** **5×** ("slow but not tediously slow"). ACTIVE scans while it is on. The gain
comes back once the power does. (Step 64 reversed this: lose power, the gain drops to 1.)

**What changed.**

- **Everything that moves, 5×** (`HandoffBoardState`; `EnemyBrain` default and the scene's value):
  - our top speed 12.5 km/s;
  - acceleration and braking ×25, so full speed comes 5× sooner;
  - drift and drag keep rates to the 5th power;
  - turn 20°/s; TRACK 07 cruises at 5.5 km/s;
  - slugs 250 km/s, missiles 100 km/s (turn 6.5°/s), fighters 40, assault boats 30;
  - boarding speed limit 0.5;
  - the SPD knob steps 0.25 (`spdStep`).
  - The map, sensor and weapon ranges, charge and timings are unchanged.
- **The hit test follows the target's motion within a tick** (`TickRounds`). At these speeds and
  x10 a ship moves further in one tick than its hit radius.
- **ACTIVE scan:** with ACTIVE on, a contact inside the cone and DRADIS range is identified as the
  sweep passes it (`HandoffPlotPanel`). The alarm strip and alert tone sound only for a
  contact DRADIS had not already heard; one already heard is identified silently. Letting go of the
  cone dial and clicking a row still work.
- **Gain:** `gainSet` remembers the gain you set. While the SEN bus is short of power the gain is
  held down, then comes back to it. **Superseded by Step 64:** `gainSet` is gone; the gain drops to 1
  and stays.

**Checked.**

- In the game after the change, the same opening: the gain held at 2, ACTIVE identified TRACK 07
  on its own, and TRACK 07 moved 19 px in 30 s at x1.
- Playtest 163/163 PASS (`Logs/Playtests/2026-10-25_184602/`), including Step 62's checks. Tests
  adjusted for the new speeds:
  - the gain starts at 1 so hearing still comes at step 17;
  - a marginal aim may land either way;
  - fighters' speed is measured at x1;
  - the second salvo waits out TRACK 07's reload.

**Found, not changed (for the owner).**

- The throttle has the same trap the gain had. Set full throttle while the ENG bus is still filling
  and it is clamped to 20%, then stays there: the design's "the lever stops where the bus stops".
- With all four buses on DRAW, ENG gets 3 of 10 units, so throttle stops at 60%. That is the
  power trade-off as designed.
- The mission clock starts at 2:03:32, the design's demo time.

### Step 64 — Lose power, lose the setting

**Owner decision (2026-10-25).** "If the power goes, you have to redo everything. It just sets back
to zero." This replaces Step 63's "the gain comes back".

**What changed** (`HandoffBoardState`):

- **Gain:** if the SEN bus can no longer carry it, the gain drops to 1 (its lowest) and stays there.
  `gainSet` is gone.
- **Throttle:** if the ENG bus can no longer carry it, the throttle drops to 0 and the ship brakes to
  a stop. The design held it at what the bus could carry.
- **At load** power is cold, so the gain starts at 1 and the throttle at 0. The design's demo had 2
  and 40. Powering up no longer knocks anything back without telling you. Set them once the power
  is on.

**Checked.** Playtest 163/163 PASS (`Logs/Playtests/2026-10-25_185453/`).

### Step 65 — The battle space: a map with edges, the grid resizing inside it

**Owner decisions (2026-10-25).** A first try used fixed 1,000 km squares and a 10 x 10 square
map, with zoom stopping when the map filled the plot's height. It left black down both sides. The
owner wants: the squares resize as you zoom, as before; the map has a separate width and height;
zoom stops when the **sides** fill, and you pan up and down for the rest.

**Where to change it.** `Board/HandoffBoard` → Handoff Board Controls → State → Battle space:
**Arena Width Km** and **Arena Height Km** (100,000 each to start: 20 x 20 big squares at x1),
**Arena Warn Km** (2,500), **Arena Return Seconds** (30; owner, 2026-10-25, was 150).

**What changed.**

- `HandoffBoardState`: `arenaWidthKm`, `arenaHeightKm`. Near the edge a warning; past it a
  countdown; not back in time and the ship is lost. Crossing out drops the rate to x1.
- `HandoffPlotGrid`: the minor square is again the step nearest 61 px (1,000 km down to x0.67, then
  2,000 down to the x0.35 limit; 5,000 only below x0.29). Lines start from the map's corner, so its edge is always a
  line if each side is a multiple of 50,000 km. Drawn only inside the map, and square.
- `HandoffBoardControls.MinZoom`: full zoom-out is **Zoom Out Limit** on Handoff Board Controls,
  x0.35 (owner). If the map is too narrow to fill the plot's width there (under about 80,000 km
  wide), zoom stops sooner, where it fills, so the sides are never black. A 100,000 km map would fill
  at x0.28, so the limit applies. At x0.35 the squares are
  2,000 km and the map is wider than the plot, so you pan side to side as well.
- `HandoffPlotPanel`: panning stops at the map's edges. If our ship is outside, you can pan out
  to it.
- The alarm strip shows the edge warning and the countdown.
- **Axis numbers follow the grid** (owner agreed). One on each big-square line, along the top and
  left edges of the plot, in thousands of km from the world centre (+ east and north). They slide
  when you pan and spread out when you zoom. A number that would sit on the "X (1,000 KM)" or
  "Y (1,000 KM)" title is skipped. The design's bottom row is gone: the ORDERS panel covers its
  left end, and the top row already gives the same numbers.
- **The axis numbers' look is on the inspector** (owner): `Plot` → Handoff Plot Panel → Axis
  numbers: the layer they are made under (`Plot/Scale`), the two titles, font (Share Tech Mono),
  material (its thickened "Small" one), size (11) and colour. The design's 12 fixed number labels
  were deleted from `02.1_Grid`. The board builder (`WirePlot`) did the same on a rebuild: it
  deleted them and filled the slots from their look. Checked on the full rebuild of 2026-10-25 (Step 68's
  notes): 12 deleted, every slot filled, live numbers in play.
- **`C` centres the plot on our ship** (owner asked for a key). Near the map's edge the view stops
  at the edge, so the ship can sit off-centre.

**Checked** in play mode, with screenshots (before the x0.35 limit): at x0.28 the grid fills the plot's width; panning is
locked side to side and stops at the top and bottom edges; at x0.5 and x1 it stops at all four.
Axis numbers checked in play mode at x1 and x0.35, panned: each number sits on its line, our ship
reads 0, 0. The `C` key: MCP can't press a key; the owner pressed it in play and it works (2026-10-25). Playtest 163/163
(`Logs/Playtests/2026-10-25_221753/`); it has no battle-space checks yet.

### Step 66 — Our ship stops blinking; drawers slide out from behind the frame

**Owner decisions (2026-10-25).**

- **Our ship no longer blinks when we're tracked.** The owner saw it as a bug. The warning is the
  red E-SIG lamp and meter on the Sensors panel (and the SIG lamp on Comms). Step 68 copied that
  whole row into a box in the frame's top-right. Step 44's blink is gone from `HandoffPlotPanel`. The
  playtest now checks that the ship does **not** blink.
- **The metal frame is in front of the drawers**, so a drawer slides out from behind it.

**How the frame is in front.** The frame is one solid steel sheet under everything; the plot covers
its middle. So the frame isn't moved. The six drawers are moved under a new
`Board/HandoffBoard/Drawers` object, just above `Plot`. It clips the drawers to the frame's opening
(`RectMask2D`, padding 98, 84, 98, 94: left, bottom, right, top). Everything on the frame (alarm
strip, clock, rails, Console, Orders, Helm, periscope, and since Step 68 the signal box) now draws
on top of the drawers. The drawers' cast shadows no longer spill onto the frame.

`HandoffBoardBuilder.TuckDrawersUnderFrame` did this near the end of the build, so a rebuild kept
it. It had to run after every Wire step, because they found the drawers as direct children of
`HandoffBoard`. From Step 68, `BuildSignalBox` ran after it.

The draw order in `UI_INTEGRATION_LOG.md` ("What the builder does") is from before this step. The
drawers now sit in `Drawers`, between `Plot` and `AlarmStrip`.

**Checked** by rendering the Board Camera with Sensors half open (cut cleanly at the frame's inner
edge, under the rail) and fully open (unchanged). Not looked at by eye in play mode. Playtest 163/163 (`Logs/Playtests/2026-10-25_221753/`), including "Tracked,
our ship does not blink" and every drawer click through the new mask.

### Doc check — Steps 56 to 66 (2026-10-25)

**What.** Four independent, read-only checks compared the write-ups of Steps 56 to 66,
`DAMAGE_README.md`, `BOARDING_README.md` and `docs/scale-sim/` with the code, claim by claim, with
file and line evidence. Most claims held. The scale sim still matches the game's numbers.

**Docs fixed.**

- Earlier steps now point to the later step that changed them: Step 57's speeds, Step 58's
  "destroyed", Step 63's gain, decision 29's fighter speed, Step 41's axis numbers and recentring,
  Step 44's blink.
- Step 58: with no power, DRADIS still listens. It was written as "no sensors".
- Step 63: the sweep's identification sounds the alert only for a contact not already heard.
- Step 65: the grid steps and zoom limit as they are at x0.35. Decision 32 names the limit and the
  countdown. Steps 65 and 66 had passed the playtest (163/163).
- `BOARDING_README`: boats 30 km/s, boarding below 0.5 km/s (Step 63's numbers); names follow the
  scan rule.
- `DAMAGE_README`: radiation starts on a slug, HE or penetrator hit, not a fighter's; REBOOT adds 1;
  the button reads RECALL; boarding sits in the build order where it was built; where the team's
  time is tuned.

**Code fixed.**

- **A disabled enemy still drove and fired.** Its brain set cruise and fired back without looking
  at its own engine room or magazine. Now its throttle is held to what its engine room allows
  (`ThrCap`, as our lever is), and it fires back only with its magazine whole (`EnemyBrain`). This
  also makes "fight it down" for boarding work: wreck its engine room and it stops. New playtest
  check after step 21f.
- **The targeting computer named a classified ship before it was scanned.** It now uses the same
  rule as everywhere else, BOGEY until scanned (`HandoffPeriscopePanel`).
- The battle space's return countdown is 30 game seconds (owner). The tooltip's fastest return is
  corrected from about 110 to about 75 game seconds.

**Owner answers (2026-10-25).**

- A ship with no power should not hear on DRADIS. Logged as a bug in Open items.
- Coming back inside for a moment restarts the 30 seconds. That is fine.
- 30 game seconds is 12.6 real seconds at x1. Straight out at full speed, the fastest return is
  about 75 game seconds: brake, then turn. The 2,500 km warning is the time to turn.

**Answered later the same day.** You turn on ACTIVE and the sweep passes a ship. Every pass beeps.
Step 69.

**Not yet.** The bugs this check found that are not fixed are in Open items, "Bugs, not fixed yet".

**Checked.** With Step 68: playtest 165/165, including "Disabled, its brain cannot drive it"
(`Logs/Playtests/2026-10-25_224013/`).

### Step 67 — The Level tool: each level is a scene; the enemy spawn box

**Owner decisions (2026-10-25).** Each level is its own small scene, loaded on top of the board.
**Starwolf → Level** is the tool. To start, the enemy spawn is just a box: seen in the Scene view so
it can be moved, never on the board. Debris (not built yet) will be the same every play. The
mission scripter gets its own menu item later. Decision 33.

**What changed.**

- `EnemySpawn` (new): a filled box with a bow line and its name, the same size on screen at any
  zoom. At play, in `Start`, it calls `TacticalWorld.Spawn`, which moves its contact there, facing
  the box's way. The patrol route goes with it: `EnemyBrain` takes its home on its first think.
- `TacticalWorld`: `levelScene` (default `Level_01`) loads on top at play, unless a level is
  already open in the editor; then that one plays. New `Spawn(key, at, zDeg)`. The ship start code
  moved into `StartShip`.
- `LevelWindow` (new): **Open** loads the board with the level on top and shows the map from above,
  north up. **New level** makes the next `Level_NN` and adds it to Build Settings. **Enemy spawn**
  adds a box where the Scene view is looking.
- The board stays the active scene, so its lighting and settings are the ones that play.

**Checked.** Playtest 163/163 (`Logs/Playtests/2026-10-25_221753/`), with `Level_01` loading on
top. In play mode: with only the board open, `Level_01` loads by itself; with the spawn moved (not
saved) to (0, 20,000) facing 090, the enemy started there, heading 090.

**Known.**

- The dummy's model still sits at its old spot in `02.1_Grid` in the editor. At play it jumps to
  the box. Only the box matters.
- Something you add by hand goes into the active scene (the board), not the level. Use the window's
  buttons, or drag it into the level in the Hierarchy.
- The box was made screen-sized after the playtest (drawing only) and checked on screen: at the
  whole-map view it shows as a filled red square, its bow line and its name.

### Step 68 — The signal alarm: a box in the frame's top-right, and the alert tone

**Owner decisions (2026-10-25).** After Step 66 removed our ship's blink, the signal needed a
warning you can see with the Sensors drawer shut. The owner chose a box in the empty top-right of
the frame, opposite the mission clock, and the alert tone once when you become trackable.

**What it shows.** The Sensors drawer's E-SIG row, copied: the E-SIG label, the 10 cells (white,
then amber, then red), the percent, the lamp (amber with any signal, red and blinking once tracked),
and the note ("RUNNING SILENT", or "1 HOSTILE SET HAS YOUR RANGE" once tracked).

**How.**

- `Board/HandoffBoard/SignalBox`, just after `TimeControl`. The row's dark backing (`well_r3`)
  sits at board px 1519, 30 (356 x 50), clear of the corner bolt. Copies are named `Signal · well`, `· label`, `· cell 1`–`10`, `· glow`,
  `· lamp`, `· percent`, `· note`.
- `HandoffSensorsPanel` fills in the box exactly as it fills in its own row: same rules, same
  blink. Its `Box …` slots (under "Signal box" in its inspector) point at the box's cells, lamp,
  glow, percent and note. The well and label are plain copies. The panel sits on
  `Board/HandoffBoard`, so it keeps working with the drawer shut.
- No new script (owner, 2026-10-25). A first version used a separate `HandoffMirror` script to
  copy the row; the owner found it confusing, and it was removed the same day. Playtest after
  removing HandoffMirror: 166/166 (`Logs/Playtests/2026-10-25_233126/`).
- `HandoffBoardControls`: after `TickSensors`, the alert tone plays once as the signal crosses
  `signalTrackedAt` (45). It plays again only after the signal drops below and comes back.
- `HandoffBoardBuilder.BuildSignalBox` built it, after `TuckDrawersUnderFrame`, so a rebuild kept
  it. It copied from `Drawers/State · sensors`.
- Playtest: a new check after "Tracked, our ship does not blink". The box reads the signal and
  the trackers line.

**Checked.** Rendered the Board Camera at rest: the box sits in the top-right band, clear of the
periscope housing and the corner bolt, and reads 0% and RUNNING SILENT. Playtest 165/165 PASS
(`Logs/Playtests/2026-10-25_224013/`): 163 plus two new checks, this step's signal box and the doc
check's "Disabled, its brain cannot drive it". Tracked at 100%, the box read "100%" and "1 HOSTILE
SET HAS YOUR RANGE"; the run's shot `23_bearing_trail.png` shows it red in the top-right.

**Full rebuild checked (owner asked).** `Starwolf/Handoff/2` rebuilt the whole board with this
step's code and the axis-number change. Every setting on the board, the World and the cameras was
listed before and after: no value changed. The only additions were 2 text sub-objects TMP makes by
itself for the "▸" glyph. The build counts show "deleted: fixed axis number: 12". Playtest after
the rebuild: 164/165 (`2026-10-25_230225`, check 140 failed, trap 23), then 165/165 on a re-run
(`2026-10-25_230646`), with live axis numbers on the plot.

**Known.** The box only exists in `02.1_Grid`. The tone isn't checked by the playtest: other cues
(new contact, edge) use the same tone, so `LastSound` can't tell them apart.

### Step 69 — Each DRADIS sweep pass beeps

**Owner decision (2026-10-25).** "Every time that thing is tagged by the sweep, that's when that
beep should come." With ACTIVE on, the DRADIS sweep tags a ship in the cone on every pass, and every
tag beeps. Naming the ship has no sound of its own.

**What changed.**

- `HandoffPlotPanel`: the sweep looks at ships already scanned too. A ship in the cone and in DRADIS
  range is tagged on every pass: a beep, and it is scanned if it was not.
- `HandoffBoardControls`: the beep is `tagSound` (880 Hz, 0.05 s), in **Starwolf → Tuning →
  Sound**. `Tags` counts the beeps apart from the click cues, so a beep never stands in for the cue
  a check waits on.
- Playtest: a new check where our ship is put 2,000 km off TRACK 07's bow (step 21d). ACTIVE on and
  the cone on TRACK 07: two passes, two beeps.

**Checked.** The new check passed: 2 beeps (`Logs/Playtests/2026-10-25_232343/`). That run then
stopped at check 158: the engineering BRI cell click landed off the board, at x 3375. It ran while
another session was rebuilding the board, and this step does not touch engineering. Re-run the
full playtest once that work is in. An earlier try stopped at step 15 on a reload.

### Step 70 — Intro colour: drawn through a Canvas; dark green field

**Owner direction (2026-10-25).** Match two reference shots of the intro (SECTION 36 and STARWOLF
on a green CRT): deep black-green field, strong green dots, a faint glow, lit screen edges,
scanlines. This replaces the flat black of Step 56. Colour first; the frame, subtitle, corner text
and screen surface come after.

**Measured from the shots.** Lit dots (150, 249, 163): the existing `#A2F9A0` already matches.
Field (0, 17, 0), with faint scanline bands. Gaps between dots ~60. Screen rim ~80, fading inward;
outside the screen ~4. The paleness in Unity was only the double encode.

**The cause.** `OnGUI`'s `GUI.DrawTexture` sRGB-encoded the picture twice, and cut it to 8 bits
before that. Undoing one encode in the shader was tried first: the dots came right, but anything
that dark rounds to zero before the encodes, so the field stayed black. (Also: `GammaToLinearSpace`
is a fit that is ~6x too dark near black; use the `Exact` form if you ever need it.) The same
picture on a uGUI `RawImage` came out encoded once, with the field intact.

**What changed** (`Assets/StarwolfIntro/`):

- `StarwolfIntroController`:
  - `BuildCanvas()` makes a child `Phosphor screen`: an overlay canvas (sort order 32767), a black
    backing, and a 16:9 `Display` `RawImage` showing the render target. Hidden while suspended or
    disabled.
  - `OnGUI` now only handles menu input; it no longer draws. The hit rectangles are unchanged
    (`ScreenRect()` is the same rectangle as the 16:9 fit).
  - sets `_Background` from the glyph data.
- `PhosphorDisplay.shader`: `_Background` is back, under the lit text. The near glow is .015 (was
  .027) to match the gaps in the shots.
- `glyphs.json`: `background` is `#001100` (was `#010601`).

**Checked** in play mode, screenshots measured against the shots:

| | Unity | Reference |
|---|---|---|
| Lit dots | (158, 244, 156) | (150, 249, 163) |
| Field | (0, 15, 0) | (0, 17, 0) |
| Gaps between dots | 55–62 | 50–64 |

The menu draws on the same field. Playtest after the canvas change: 166/166 PASS
(`Logs/Playtests/2026-10-26_000216/`).

**Still to do (owner's order).** The frame with open sides, the STRATEGIC SYSTEMS DIVISION line,
the corner text and clearance squares, and the screen surface (lit rim, visible scanline bands).

### Step 71 — The welcome page on the phosphor screen; a loading bar under INITIALISING

**Owner direction (2026-10-26).** The page after NEW GAME gets the intro's look: the same screen, a
short dot title, the lines in green monospace with the same glow, the Enter prompt green. The title
is **DEMO** (owner: "demo" or "WIP"). The first line is "Welcome to Starwolf." (was "Welcome to
Starwolf Tactical Grid Test."). INITIALISING... gets a loading bar under it that fills left to right.

**What changed.**

- `StarwolfIntroController`, four new fields:
  - `titleOnly`: types the title in, then holds it; no menu, no `finished`.
  - `titleText` (default STARWOLF), `titleOffset` (pixels up), `canvasOrder` (default 32767).
  - sends `_Progress` (share of the line's time gone) to the shader.
- `PhosphorDisplay.shader`: `_TitleOffset` moves the title; `bar()` draws the loading bar under
  INITIALISING... in the same dots (one-cell outline, one-cell gap, fill by `_Progress`).
- `01_Briefing`:
  - new `WELCOME Screen`: a `StarwolfIntroController` copied from `INITIALISING`, with `titleOnly`,
    `titleText` DEMO, `titleOffset` 300, `canvasOrder` 0, `titleDuration` 2.
  - `Canvas` is now Screen Space Overlay, sort order 1, so its text draws over the phosphor screen.
    Its two texts had z 1195.77 left over from camera mode, which hid them; set to 0.
  - `BriefingText` (30 pt) and `EnterPrompt` (26 pt): Share Tech Mono, `#A2F9A0`, upper case,
    letter-spaced, on the new material below.
- New `Assets/StarwolfIntro/Materials/ShareTechMono-Regular SDF Phosphor Glow.mat`: the font's mobile
  shader with a soft green underlay as the glow.
- `INITIALISING` keeps sort order 32767, so it covers the welcome page when Enter is pressed.

**Checked** in play mode with screenshots: the welcome page (title, lines, prompt, glow), and
INITIALISING over it with the bar part filled.

**Traps.**

- The full `TextMeshPro/Distance Field` shader draws magenta blocks here. Stay on the mobile shader;
  its underlay is the glow.
- A canvas switched from camera to overlay keeps its children's z. Zero it or the text vanishes.

### Step 72 — Mission clock and signal box lined up with the plot

**Owner direction (2026-10-26).** The mission clock's left edge lines up with the plot's left edge,
and the signal box's right edge with the plot's right edge.

**What changed** (`02.1_Grid`, set in the scene; the board builder is retired):
`TimeControl` x +72 (left edge 32 → 104 px) and `SignalBox` x −59 (right edge 1,875 → 1,816 px).
The plot runs 104–1,816 px.

**Checked** by measuring the screenshot: both edges sit on the plot's edges.

**Trap.** In edit mode the board is drawn by `Board Camera` into a render texture, which does not
redraw on its own. Call `Render()` on that camera before an edit-mode screenshot, or it shows the
old layout.

### Step 73 — Ruins in the level: the starbase husk, a debris belt, and where each ship starts

**What it is.** Space has stuff in it now. `Level_01` holds a broken starbase at the grid centre and
766 pieces of wreckage, rock and dead ships around it, thicker the closer you get. Our ship starts at
the bottom edge, the enemy at the top, and the core is between you. Nothing of it shows on the plot or
blocks anything yet: this step places it.

**Owner direction (2026-10-25/26).** The prototype and the GDD both call for ruined starbases and big
debris. The **hard core of the starbase sits at the grid centre**, the debris is **radial and thicker
toward the core**, and you **start at one edge and work inward**. The enemy starts at the other edge:
"You have to get through the core. It'll come to the core as well. You're going to circle through
them and try to find each other. It's going to hunt you. You're going to hunt it." (The hunt is not
built; see Open items.)

**Scale (owner).** Ships are really 100–500 m long, by tonnage. Ruins are sized for **how the ship
plays**, not for the 13 km models: a full-speed 90° turn takes about 300 km, stopping about 180 km,
and a slug hits within 95–600 km. So gaps of a few hundred km let you fly through, hide behind, and
play chicken with collision. The base husk is **double the prototype's**, about 13,000 km across.
The ship models stay as they are until the outside cameras come; nothing in play reads their size.

**What was built.**

- **Layer `WorldTerrain`** (slot 10). No board camera draws it and no live code does physics queries,
  so the terrain changes nothing in play yet.
- **The terrain prefabs** (section 3): four kit pieces, `BaseHusk`, `Rock`, four `Wreck_` prefabs.
  The husk's core is four blocks of 500–1,400 km with cracks of 200–300 km.
- **`Level_01` → `Terrain`:** the husk at (0, 0); 766 pieces thinning outward, gaps about 350 km
  inside the husk, 500 km at its rim and thousands near the battle space's edges; four asteroid fields;
  five wrecks named after the prototype's, WRECK-2, -4, -7, -9 and -11. Station wreckage near the core,
  mostly rock far out. Placed by a one-off script, so it is the same every play; edit it by hand in the
  Scene view (Starwolf → Level → Open).
- **`PlayerSpawn`** (section 3) and the Level window's **Player spawn** button. `Level_01`'s Player
  Spawn is at (0, −45,000), facing north, clear of the 2,500 km edge warning. The enemy spawn moved
  to (0, +45,000), facing south. The ships start 90,000 km apart, beyond either one's hearing.
- **Playtest check 3b:** our ship starts at the level's Player Spawn, facing its way. Then both ships
  go back to the board's own spots (ours (0, 0) heading 000, TRACK 07 at (18058, 16260)), so every
  later check keeps its numbers whatever the level does.

**Checked.** Playtest 168/168 (`Logs/Playtests/2026-10-26_003801/`): our ship started at
(0, −45,000), heading 000, and the plot opened on it just above the map's bottom edge. The saved
scene holds 767 prefab copies and 826 colliders.

**Traps.**

- The ship and rock models import turned 270° on X and scaled ×100 (made in Blender). Unturned, a
  ship's length points straight out of the map. The terrain prefabs replace that turn.
- Soham's `Asteroid.cs` (health, breaking apart) is not on the rock. Asteroids can't be destroyed yet,
  and it sets an "Asteroid" tag this project doesn't have, which throws.
- The playtest's TRACK 07 checks assume its old spot; check 3b puts it back there.
- Several chats shared Unity during this step. Each terrain edit was one call that opened `Level_01`
  beside whatever was open, saved it and closed it again, only while Unity was idle.

### Step 74 — Engineering's ship picture shows side armour; the room rings change colour

**Owner direction (2026-10-26).** The engineering drawer's ship picture shows **side armour**. It
uses the four-part models in `Assets/3D Assets/UpdatedStarships/`. Each model is cut into four parts,
each with its own material slot: `Prow(front)`, `Port(left)`, `Star(right)`, `Aft(back)` (on the
Carrier the front slot is just `Front`). These match the damage model's four faces
(`HandoffBoardState.Sides`: PROW, STBD, AFT, PORT).

- Each part takes its side's armour colour: **white** while full, **amber** once hit, **red** once
  open (armour 0, so hits now reach the room behind). The owner's pick. Claude agreed: the room
  rings are white when whole, and our ship is white on the plot.
- The six compartment dots are gone. The room rings on the right of the drawer already show the
  rooms.
- The room rings change colour too. Lit segments are white at health 4–5, amber at 2–3, red at 0–1
  (`CompHealth`, so a vented room counts as 0). Same thresholds as the dots had, white in place of
  green.
- The plot ship stays plain white and is not touched. This lives on engineering only.
- Four flat top-down pictures, not a live 3D camera. The drawer's ship is always bow up, so it
  never turns.

With today's numbers, one slug turns the prow, port or starboard amber, and a second opens it. The
aft has half the armour, so one slug opens it. A fighter run takes 10% of a side's armour, so one
run turns that side amber.

**What changed.**

- New `Assets/Art/Board/Hull/hull_prow.png`, `hull_stbd.png`, `hull_aft.png`, `hull_port.png`
  (512 px, with .meta files).
  - How they were made: `StarWolf_Destroyer_PlayerShip_mesh.fbx` was drawn in an editor preview
    scene, copying the plot's `Ship View Camera`: orthographic, looking along +z, ship filling 0.8
    of the frame, model turned (0, 180, 0), `Ship Light` at (325, 25, 0) with intensity 1.2. It
    drew through `PC_Renderer`, so the VectorModel line pass applied.
  - Each part was drawn alone. The other three slots got an invisible transparent material: the
    line pass reads depth and normals, which transparent objects don't write.
  - Import: sRGB **off** (saved straight from the linear camera image, like the live ship view), no
    mipmaps, clamp, uncompressed.
  - Shown through the existing `BoardLinearImage` material, the same one the plot ship uses: black
    becomes see-through, and the lines take the image's colour.
  - Port lines centre at x 0.40 of the frame and starboard at 0.60, so port is on the left.
- `HandoffEngineeringPanel.cs`:
  - Removed `shipPicture`, `compartmentLamps`, `lampKeys`, `lampOffsets` and the `ShipView` lookup.
  - Added `hullSides` (four RawImages: PROW, STBD, AFT, PORT) and `pictureFill` (0.8). Kept
    `diagramShipPx` (190, under "Engineering & orders" in Tuning) and `shipInk`.
  - `ShowDiagram` sizes the four pictures and paints each from `SideArmour` against
    `sideArmourMax`.
  - The ring loop colours lit segments by `CompHealth`.
- `02.1_Grid`, under `Drawers/State · engineering/Ship diagram` (set in the scene; the board
  builder is retired):
  - The six `Lamp · …` objects are deleted.
  - `Ship (bow up)` became `Ship · PROW`, plus three copies: `Ship · STBD`, `Ship · AFT`,
    `Ship · PORT`. RawImages, 237.5 px square, with `BoardLinearImage`, wired to `hullSides`. They
    show in edit mode too, not only in play.
  - `Assets/Editor/HandoffBoardBuilder.cs` still names `shipPicture` and `compartmentLamps` by
    string. Not updated: the builder no longer runs.
- `PlaytestRunner.cs`, check 23 rewritten:
  - all four parts white, rings white;
  - STBD at half armour, AFT open, HAB at 2, HGR at 1: STBD amber, AFT red, PROW and PORT white,
    HAB ring amber, HGR ring red; screenshot;
  - repaired: everything white again.

**Checked.** Playtest **PASS, 168/168** (`Logs/Playtests/2026-10-26_003801/`). The engineering
checks are rows 150–154. The screenshot `shots/30_engineering_diagram.png` shows starboard amber,
aft red, prow and port white, the HAB ring amber and the HGR ring red. The same run covered Step 73.

**Traps.**

- Don't colour the shared `ShipView` render. The plot uses it, and the plot ship must stay white.
- The pictures must import with sRGB off, or the lines come out the wrong brightness through
  `BoardLinearImage`.
- To remake the pictures for a new model: draw each material slot alone through `PC_Renderer` (for
  the line pass), with the other slots invisible. Find the slots by name. Their order differs per
  model: the Battleship's is Prow, Aft, Port, Star.
- The ship is about 120 px wide in a 112 px box, so the side pods poke slightly past the dark box.
  It was the same before; not new.

### Step 75 — Ships glide; DRADIS shows the sweep's last touch; visual range; wrecks stay; ranges 30,000 km

**Owner direction (2026-10-26).** Before this step, our ship jumped across the plot at every 0.42 s
tick. The owner asked for this:

- Our ship glides smoothly across the plot.
- An enemy known only through DRADIS moves on the plot only when the sweep touches it.
- Inside visual range, the enemy's full model shows and glides. You see its shape. Its name shows
  only once it is identified, and you cannot target it before that.
- A destroyed ship turns grey and drifts. It stays on the plot.
- Guns reach 30,000 km. DRADIS at full gain reaches 30,000 km. Visual range is 10,000 km. These are
  starting numbers, to be tuned.
- The engineering hull picture looked greasy: every panel line of the model showed as a hairline.
  The owner wanted the panel lines grey and the outline bright. (A solid grey fill was built first by
  mistake; the owner turned it down the same day.)

**What changed.**

- **The glide.** Only the drawing changed. The rules still step once per tick.
  - `HandoffBoardControls.SinceTick` is the game time since the last tick. It goes up to one tick's
    worth. It is 0 while paused.
  - `NavSystem.DrawnX`, `DrawnY` and `DrawnHeading` are where the ship is drawn: its spot moved on
    along its velocity for that time, with the bow still turning toward the ordered heading.
    `PlaceModel` puts the model there every frame, in `Update`. The ship cameras follow in
    `LateUpdate`.
  - `HandoffPlotPanel` redraws every frame while time runs. Our ship's picture, the sensor circle,
    the sweep, the weapon ring and the PDC arcs are drawn at our ship's drawn spot.
  - `TacticalWorld.Sync` reads each contact's spot and heading from its `NavSystem`, and gives the
    track its `nav` (trap 25).
- **What DRADIS shows.**
  - The sweep passes a contact that is heard or in the cone. `HandoffBoardState.See` stores where
    the contact is at that moment (`Track.seen`, `seenX`, `seenY`, `seenBrg`, `seenRangeKm`).
  - Out of visual range, an identified hostile shows as a dot at that spot. Its label gives the
    range at that moment.
  - A heard contact that is not identified gets a bearing line at the sweep's first touch. The line
    keeps that bearing until the next pass.
- **In visual range** (`visualRangeKm`, 10,000 km, in Tuning → Sensors; `InSight`).
  - The contact shows as its model seen from above, gliding.
  - It is red once identified. Until then it is amber and labelled **BOGEY T07**. It stays
    unidentified, so the targeting computer can't use it.
  - Being in visual range brings up the picture. Pressing CLASS no longer does.
- **Wrecks.** In visual range, a wreck's model is grey (`wreckInk`, ink.secondary `#8A959D`) and
  drifts. Out of visual range it is the same dim dot as before, now drawn where the wreck drifts.
- **Ranges.**
  - `senRangePerStepKm` went from 7,000 to 3,600. DRADIS now reaches 15,600 km at gain 1 and
    30,000 km at gain 5.
  - RGA, RGB and VLS reach 30,000 km. That is still the DRADIS range at full gain (Step 62).
  - The numbers are set in the code and in `02.1_Grid`'s saved board state. The enemy ships use the
    code's numbers.
- **Engineering hull pictures.** The four images in `Assets/Art/Board/Hull/` were redrawn from the
  Step 74 line pictures.
  - Each part is the gap it leaves in the other three.
  - A 3 px outline is at full brightness, edges smoothed at 4×. Inside it, the Step 74 panel lines
    are at 40% brightness, so they read grey (or dim amber or red). No fill. The lines stop 3 px
    short of the outline.
  - The frame and the file names did not change, so nothing else had to.
  - To remake them for a new model, make the line pictures as in Step 74, then redraw them this way.
    `BoardLinearImage` encodes first and then uses brightness as alpha. So store each brightness as
    the shader's target run back through the sRGB curve.
- `PlaytestRunner.cs`:
  - TRACK 07 starts 17,500 km out on 048° (was 24,300), so gain 2 still hears it.
  - The bearing-line check waits for the sweep's first touch.
  - "Its 3D model follows it" allows one tick's glide.
  - The CLASS check now needs the contact in visual range. A new check: out of visual range it is a
    dot again.
  - New check: the wreck stays on the plot, in visual range, as its model.
  - The ship-shift stand-in moves 5,000 km west. When it moved east, DRADIS at gain 1 heard TRACK 07
    too early.
  - Videos: when a run starts, it deletes every earlier run's `playtest.mp4` (owner, 2026-10-26: a
    video is not needed once its run is checked). Screenshots stay for the newest three runs. Every
    `report.md` stays. The recording keeps the Game view's 16:9 shape (owner).

**Checked.** Playtest **PASS, 170/170** (`Logs/Playtests/2026-10-26_011622/`). The first run
(`2026-10-26_011116`) failed only "Nothing heard yet": the east stand-in, fixed above. The
screenshot `30_engineering_diagram.png` shows the hull: starboard amber, aft red, prow and
port white. That run had the solid fill. The grey-line version was checked by hand:
`Logs/Checks/hull_inner_lines.png` (starboard half, aft open).

Then, with the GARUDA renames and the fighter take-off change in the same build: playtest **PASS,
171/171** (`Logs/Playtests/2026-10-26_013236/`).

Checked by hand in play mode (2026-10-26):
- The glide. At x10, over half a second, our ship's picture moved on 73 of 77 frames. The game clock
  ticked once. At that tick the true spot jumped 125 km and the drawn model did not jump.
- The wreck. GARUDA destroyed, 8,000 km from our ship. Its model shows in grey (`wreckInk`), next to
  our white ship. Screenshot: `Logs/Checks/step75_wreck.png`.

**Traps.**

- Trap 25: a ship's model can be up to one tick ahead of the ship. Rules read `NavSystem`.
- The glide is a straight-line guess. A ship that speeds up or turns slips back a little at each
  tick (about 0.2 km at x1). If that shows at high zoom or at x10, predict the next tick instead.
- The plot redraws every frame while time runs, as it already did while panning. If that costs too
  much, redraw only what moves.
- There is one contact picture (`contactShipImage`). A second contact in visual range shows as a dot
  if it is identified, and not at all if it is not. A second wreck shows as a dim dot.

### Step 76 — Axis numbers measured from our ship; old titles and stencils gone

**Owner direction (2026-10-26).** The numbers along the top and left of the plot should change as
our ship moves and as you zoom. The "X (1,000 KM)" and "Y (1,000 KM)" titles go. The frame stencils
"TCB-1 / REV D" and "ACC 07" go.

**What changed.**

- `HandoffPlotPanel.ShowAxes`: each number is a distance from our ship, in km.
  - 0 is level with our ship. Then one number per big square: 5,000, 10,000 and on at x1.
  - The numbers move with the ship and spread out or close up with the zoom.
  - They redraw every frame while time runs (Step 75).
- `HandoffPlotPanel`: the `axisXTitle` and `axisYTitle` fields are gone. The retired board builder
  no longer sets them.
- `02.1_Grid`, deleted: under `Plot/Scale`, `text#21 X ( )`, `text#21 X ( ) (close)`,
  `text#22 1,000 KM`, `text#23 Y ( )`, `text#23 Y ( ) (close)` and `text#24 1,000 KM`; under
  `Frame`, `text#4 TCB-1 / REV D` and `text#3 ACC 07`.

**Checked.** Playtest **PASS, 171/171** (`Logs/Playtests/2026-10-26_015950/`). In its shots
`11_ship_moved.png` and `18_panned.png` the 0 sits on our ship, and moves with it. That run still
showed the two closing brackets. They were deleted after it. A play-mode screenshot after that,
`Logs/Checks/step76_axis.png`, shows 10,000, 5,000, 0, 5,000, 10,000 across the top, and no titles,
brackets or stencils.

**Still on the frame:** the stencils "CL II EQUIP — 480V DC" and "CONSOLE HARNESS — DO NOT SERVICE
UNDER LOAD". The owner has not decided on them.

### Step 77 — The debris scatter tool

**What it is.** **Starwolf → Level → Debris field** adds a Debris Field to the open level and fills it
at once. You can then lay debris out by shape instead of by hand. Step 73's hand-placed debris in
`Level_01` is unchanged.

**Owner direction (2026-10-25/26).** A pattern maker in the level tool: pick a shape and scatter
debris in it. The same every play; it changes only on Reshuffle. A tool in the Level window and the
inspector, not in the menu bar (decision 31).

**How to use it.** Select the field. In its inspector:

- **Shape:** Circle, Ring, Band or Line, with its sizes in km. The field's outline shows in the Scene
  view. Move and turn the field to place it.
- **Pieces:** the prefabs to pick from (the button starts with Block, Drum, Girder, Plate and Rock;
  add wrecks or anything else), **Count**, **Size Km** (each piece's size across, between two
  numbers), and **Gap Km**, the smallest gap between two pieces' middles (400 to start).
- **Spread:** Even, Thicker In Middle, or Clumps (with how many).
- **Scatter** lays the pieces out again from the seed. **Reshuffle** picks a new seed. Both replace
  every piece under the field, hand-moved ones too, in one undo. If the gap leaves no more room, it
  places fewer than asked and says so.

**Calls made without the owner (can be changed):** Scatter and Reshuffle replace hand-moved pieces
(the inspector warns). A Line is a band one piece wide. Pieces turn only around the axis we look
down, so they stay flat to the plot.

**Checked.** `DebrisFieldEditor.Check()` (run through MCP): for every shape and spread, every piece
is inside the shape, no two are closer than the gap, and the same seed gives the same layout: ok. In
`Level_01` (closed without saving): a Circle of 120 pieces, then a Ring with Clumps (106 of 200 fit at
a 400 km gap), and Scatter again gave the same layout. Playtest not run: nothing in the field runs at
play, and `Level_01` is unchanged.

### Step 78 — The active scan cone on the plot

**Owner direction (2026-10-26).** The active scan had no cone on the plot; it showed only on the
sensors drawer's dial. Now the plot shows it: a wedge from our ship out to the DRADIS range, two
bright edge lines and a faint fill. Like the sweep, it starts at the small circle round our ship.

**What was built.**

- `HandoffMarksGraphic`: two new shape kinds. "sector" is a filled slice of a ring (`Shape.inner`
  is its inner radius in px, `Shape.half` its half-width in degrees). "line" is a plain line.
- `HandoffPlotPanel.DrawSensorRing()` adds the cone to the sensor circle's shapes while DRADIS and
  ACTIVE are both on. It turns with the cone dial and widens with NARROW / WIDEN. It takes the
  circle's colour (amber on the amber grid).
- Tuning window, Sensors: `coneEdgeAlpha` (0.6) and `coneFillAlpha` (0.07).
- No scene, prefab or new script.

**Calls made without the owner (can be changed):** the cone reaches the scan's range
(`SenRangeKm`, what `ConeCovers` uses), not the hearing circle. They are the same except on UV,
where the circle shrinks to 55% and the cone goes past it. Its reach eases like the circle's (0.5 s).
With DRADIS off and ACTIVE on, no cone shows: the sweep identifies, and it only runs with DRADIS.

**Checked.** Screenshot in play mode, cone at 048 ± 40: the edges run at 008 and 088 from the small
circle out to the DRADIS circle. Inside the cone the plot reads 21–30 (of 255), just outside 6–8.
Playtest not run (visual only).

### Step 79 — Ammunition is a real count; boarding goes on while the target moves

**Owner direction (2026-10-27).** The owner asked for checks on missiles, PDCs, ammunition and
boarding. Railgun and PDC ammunition was fixed text that never went down. Decisions 43 and 44.

**What was built.**

- `WeaponSystem`: each mount's `ammo` is now a whole number that only goes down. A railgun shot
  takes one slug from its turret. Each PDC go at an enemy missile takes `pdcRoundsPerGo` (100)
  from its group. `vlsCells` now reads the launcher's `ammo`. Every mount starts full: 22, 22,
  2400, 2400, 8. `loaded` ("2 / 2", "BELT") is gone.
- With nothing left, LOAD refuses, and the lever does nothing. The last shot drops the ladder. A
  PDC group too low for another go drops off AUTO.
- Fire control's line reads "SLUGS 21", "2200 RDS · LOADERS ENGAGED" or "HE · SALVO 2 OF 6". It
  turns caution amber when the mount is empty.
- `HangarSystem`: once the marines are aboard, the boats ride with the target and the fight runs
  on while it moves. The boats go no further than `boatRangeKm` (15,000 km) from our ship.
  Outbound past it, they turn home: "OUT OF RANGE · RETURNING". Coming home with our ship out of
  range, they are stranded: the alert tone, and "STRANDED · CLOSE IN · 87 S". After
  `boatStrandSeconds` (120) they and their marines are lost. The boats show LOST, in red.
- The three new numbers are in the Tuning window. `02.1_Grid` saved with both ships full.
  `BOARDING_README.md` updated.

**Calls made without the owner (can be changed):** the stranded clock stops while the boats are
in range, but does not go back. It sounds the alert once when it starts and once on the loss.
Fighters have no range yet. A PDC group with fewer than 100 rounds left cannot fire at all.

**Checked.** Playtest 191/191 (19 new checks). Slugs, PDC-A and PDC-B rounds, missile cells, and
LOAD refusing at zero. A ship moving with marines aboard; SECURE and DESTROY on the Garuda (each put
back for the fight after); the boats giving up past 15,000 km; stranded and coming home; stranded
too long and lost.

### Step 80 — The Tuning window's tabs, and a stats file per ship

**What it is.** **Starwolf → Tuning** opens a window with three tabs:

- **Ship:** our ship (Asura-class destroyer), its file `Assets/Ships/Asura`. Its numbers are grouped
  by ship part: Navigation, Power, Sensors, Weapons, Hangar, Damage, Orders. Each group opens and
  closes.
- **Enemy:** the Garuda (Garuda-class cruiser), `Assets/Ships/Garuda`, with the same groups.
- **Board:** everything else marked for tuning: the plot, sounds, the clock, the battle space.

**Owner direction (2026-10-25/27).** One stats file per ship (decision 31), and the window as
sketched: tabs for Ship, Enemy and Board, numbers grouped by ship part. Decision 45.

**How it works.**

- Each ship part has a `Stats` section with its numbers (`NavSystem.Stats` and so on).
  `ShipStats` (`Systems/ShipStats.cs`) holds one of each, plus the ship's class. The files are in
  `Assets/Ships`.
- A new `Ship` component (`Systems/Ship.cs`) on `OwnShip` and `Garuda · Cruiser` points at its file.
  Every part reads its numbers through it (`nav.stats.topSpeedKms` and so on).
- While the game runs, each ship plays by a copy of its file, so nothing a run does changes the file.
  An edit in the Tuning window goes to the file and to the copy: it shows at once and is kept.
- The weapon mounts (reach, arcs, ammunition at the start) are in the Weapons group. The running
  game counts its copy's ammunition down.
- Three values change as the ship plays: the salvo, the PDC loaders, the marines aboard. The file
  holds where they start.
- Signal per unit is one number per bus in the Power group (it was on each bus).
- Every `[Tune]` number on the ship parts moved, including Step 79's PDC rounds per go, boat range
  and stranded clock.

**Calls made without the owner (can be changed):** the Garuda uses its own damage numbers instead of
copying ours each tick (`CopyDamageTuning` is gone). Both files start with the same numbers.

**Fixed on the way.**

- `Starwolf/Tuning` never opened: it clashed with the old `Starwolf/Tuning/Apply` and `/Capture`
  menu items (a menu entry can't be a window and a sub-menu at once). Those two are gone, as
  decision 31 said.
- The Level window threw errors when used with the game running. It now says "Stop the game to open
  or edit a level."
- The playtest switches off the Garuda's fire, breach and radiation chances too, as it does ours
  (before, the Garuda copied ours).

**Checked.** Quick checks in play mode: each ship reads its own file through a copy; a file edit
reaches the running game; the files kept their numbers after a run. The window opened, with all three
tabs, through a start and stop of the game, with no errors. Playtest 191/191 (run
2026-10-27_004921). The run before it failed from check 114: the Garuda's own fire chances were on
and an HE hit burned it out, which the playtest change above fixes.

### Step 81 — The Level window shows the level from above; the railgun slug at 2,500 km/s

**What it is.**

- **Starwolf → Level**, with a level open, shows the level from above under the list of levels: the
  battle space's edge as a line, every piece of debris as a grey dot, the enemy spawn as a red box
  (E) and the player spawn as a white box (P).
- Under the map is a list of the level's debris fields. Pick one there, or click it on the map. Its
  outline turns amber, and its settings show under the list, with Scatter and Reshuffle.
- Change a setting and amber dots show at once where Scatter would put the pieces. The field's real
  pieces stay as they are, in grey, until you press Scatter.
- The railgun slug flies at 2,500 km/s: about 10 s to 25,000 km (decision 46).

**Owner direction (2026-10-27).** The map and preview as sketched (Level tool item 7). For the slug:
"10 seconds is good."

**How it works.**

- `Assets/Editor/LevelWindow.cs` draws the map. Its size is the board's battle space
  (`arenaWidthKm`, `arenaHeightKm`), or 100,000 km if no board is open.
- A grey dot is any renderer on the `WorldTerrain` layer in the open level, drawn at its size.
- The amber dots come from `DebrisField.Layout()`, the same code Scatter uses. They are worked out
  again only when a setting changes.
- The settings under the map are the Debris Field's own inspector (`DebrisFieldEditor`).
- The slug: `slugSpeedKms` is 2,500 in both ship files and in the code. `docs/scale-sim` has the new
  slug speed and the 25,000 km railgun reach.

**Calls made without the owner (can be changed):** the playtest's first railgun shot fires at x1. At
x10 one tick is 10 game seconds, and the slug lands inside it, before the pause check can see it.

**Checked.**

- The window on Level_01, with a test field: the preview showed 120 pieces, then 240 when the count
  doubled, then 60 amber dots beside 5 placed pieces (screenshot). No errors. The test fields were
  not saved; Level_01 is unchanged.
- The scale study, run again with the new slug. Its DRADIS reach was also wrong (7,000 km per
  gain step); it now uses the game's 3,600. At today's speeds a fight takes about 11½ minutes at
  x1, with either slug. Our ship fires 5 slugs for its 5 hits; with the old slug it fired 6.
- Playtest, run 2026-10-27_012242: 188 passed, 3 failed. Two failures came from the faster slug:
  at x10 it landed before "Paused, the slug holds still" (check 98), and the cue check (100) then
  heard the pause key. The first shot now fires at x1, and the in-flight screenshot is taken
  sooner. The third failure, "RECALL eases down while held" (53), passed in the 7 runs before;
  it looks like a timing flake.
- The run after the fixes (2026-10-27_012852) hung after screenshot 28f (boats holding) and was
  stopped, with no report.
- The next run (2026-10-27_084153) passed 191/191. The fixes are confirmed.

**Owner, after Step 81:** a shot hits within 100 km of any ship (decision 47).

### Step 82 — DRADIS needs SEN power

**What it is.**

- With no power on the SEN bus, DRADIS hears nothing. The plot shows no sensor circle, no sweep and
  no scan cone, the same as with DRADIS off.
- The DRADIS switch stays where the player set it.
- The buses are empty at load, so the plot has no sweep until SEN has power.

**Owner direction (2026-10-25).** No power, no hearing. The calls in "Playable end to end" (call 7)
were made for the owner, who said to make them without asking.

**How it works.**

- `SensorSystem.Listening`: DRADIS on, and at least 1 unit on the SEN bus. `Heard()` uses it in
  place of the DRADIS switch.
- `HandoffPlotPanel` reads `Listening` in four places: `SensorRingKm`, the sweep in `Update`, and
  `DrawSensorRing` twice.

**What it does to play.** The reactor gives 10 units and a bus holds 5. With ENG and SEN both
full, WEP gets nothing. To charge the guns, the player takes units off SEN or ENG. Take SEN to 0
and DRADIS goes deaf.

**The playtest.**

- A new check at load (power cold): the gain raised by hand to 2, which reaches TRACK 07; DRADIS
  hears nothing, and the plot shows no circle and no sweep.
- After LOAD SHED, the playtest puts SEN back on DRAW with ENG (the DRADIS circle checks need it).
  Step 17 no longer clicks SEN on; it is already on.
- Before the first shot, SEN gives back all but one unit (a stand-in for VENT, then STANDBY at 1),
  so DRADIS still hears through the fight. It used to give back all five.

### Step 83 — Enter on the briefing always leads on

**What it is.** If the INITIALISING screen can't start (a missing shader), the game goes straight
on to the board. Before, Enter did nothing.

**How it works.** `StarwolfIntroController.Awake`: if it fails and `initialisingOnly` is ticked, it
calls `finished` before it switches itself off. The briefing's `ScreenFlow` listens to `finished`
and loads the board.

**The playtest.** At the briefing it makes an INITIALISING controller with no shader, switches it
on, and checks that it calls `finished`. That logs one error, which is expected.

### Step 84 — Prototype Dev off the build list

The build list is now `00_Start`, `01_Briefing`, `02.1_Grid` and `Level_01`. `Prototype Dev` (Soham's
old cockpit test scene) is off it. The scene file stays. Nothing loaded it. Nothing loads a scene by
its build number.

### Step 85 — Win, loss and restart; SHIP LOST clears

**What it is.**

- You win when the Garuda is destroyed, or when our marines take it (SECURE).
- You lose when our reactor reaches 0, or when our ship is lost at the battle space's edge.
- When the mission ends, the game pauses at once. 2 seconds later the end screen covers the board.
- The end screen says MISSION COMPLETE in green, or SHIP LOST in red. The line under it says
  GARUDA DESTROYED, GARUDA CAPTURED, REACTOR DESTROYED or LEFT THE BATTLE SPACE. Three rows
  follow: TARGET (GARUDA · DESTROYED, CAPTURED or AFLOAT), OUR SHIP (its armour left, as a
  percent, or LOST) and TIME (game time since the board opened).
- RESTART MISSION loads the board again, with its level. It skips the intro and the briefing.
  MAIN MENU loads `00_Start`: the intro, then the menu.
- After a restart, SHIP LOST is gone. The restart loads the scene again, so everything starts
  fresh.

**Owner direction (2026-10-27).** Playable from start to finish. The calls in "Playable end to
end" (calls 1 to 5) were made for the owner, who said to make them without asking.

**How it works.**

- `Assets/Scripts/Game/QuestSystem.cs` (moved from `Assets/Scripts`). It is on `World` in
  `02.1_Grid`. Each board tick (`Ticked`) it checks our ship, then the target (`t07`). Our sensors
  identifying the target moves it from LocateTarget to EliminateTarget. At the end it keeps the
  stage, the reason and the real time, and calls `HandoffBoardControls.Pause()`. Soham's old
  version (tags, the jump point, `WorldBoundary`, `GameClock`, `InternalSubsystems`) is in git
  history. The two enums are kept.
- `Assets/Scripts/UI/Panels/EndScreenPanel.cs`, on `End Screen` (`Assets/UI/Board/Panels/End
  Screen.prefab`), the last child of `HandoffBoard`. It shows the screen once the mission is over
  and **Delay Seconds** (2, Board & time on the Tuning window) have passed.
- The screen is placeholder: plain colours, Barlow Condensed for the title and buttons, Share Tech
  Mono for the rest. There is no art yet.

**The playtest.** The run kills both ships part way and tests on, so the mission is switched off
just after the board opens, and on again at the end. A new last section:
1. Mission on: MISSION COMPLETE at the next tick, and the game pauses. The end screen comes up 2 s
   later: MISSION COMPLETE, GARUDA DESTROYED. Screenshot `29_mission_complete`.
2. RESTART MISSION, clicked for real: a fresh board, paused, power cold, the Garuda back, the
   mission at its start, no end screen.
3. The battle space's edge (Step 86), ending with our ship lost at the edge:
   SHIP LOST, LEFT THE BATTLE SPACE, our ship LOST. Screenshot `30_ship_lost_edge`.
4. RESTART MISSION: not lost, no countdown, and the alarm strip does not say SHIP LOST after a tick.
5. Our reactor set to 0 (a stand-in for enemy fire): SHIP LOST, REACTOR DESTROYED. Screenshot
   `31_ship_lost_reactor`.
6. MAIN MENU: `00_Start` loads.

### Step 86 — The playtest checks the battle space's edge

**What it is.** The playtest now checks the battle space (Step 65). Nothing in the game changed.
The checks run on the board after the first RESTART MISSION, with our ship at the centre and the
Garuda's fire off:

- At x1 the axis numbers show 0 level with our ship. Every other number is a whole number of big
  squares of 5,000 km.
- The mouse wheel, turned far out, stops the zoom at x0.35. The big squares are then 10,000 km.
- The grid, dragged far past a corner, stops with the plot's edge on the map's edge.
- `C` brings the view back to our ship. The playtest calls `CentreOnOurShip`, which is what `C`
  calls. A script can't press keys read by Unity's old Input class.
- Our ship 1,250 km inside the east edge, then RUN: the alarm strip says APPROACHING THE EDGE OF
  THE BATTLE SPACE.
- At x10, our ship just outside: the rate drops to x1. The strip says EXITING THE BATTLE SPACE —
  TURN BACK, and its count shows 30, then counts down.
- x10 again: at 0 our ship is lost, and the end screen says SHIP LOST, LEFT THE BATTLE SPACE. This
  replaces Step 85's 3-second stand-in clock.

### Step 88 — A shot hits within 100 km of any ship; a hit's damage comes from the weapon that fired it

**What it is.**

- A slug or a missile hits a ship it passes within **100 km** of. This is the same for every ship,
  ours and the Garuda (decision 47). Before, it was 95 to 600 km by class (the Garuda 210 km), and
  210 km for missiles hitting us.
- How hard a hit lands is now set by the ship that fired it (decision 49). On **Starwolf → Tuning**,
  the Ship tab's Weapons group sets how hard our slugs and missiles hit. The Enemy tab's sets the
  Garuda's.

**Owner direction (2026-10-27).** First: no hit radius, a slug must hit the ship itself. Told that
the BRG knob turns in whole degrees (about 190 km at 11,000 km), the owner chose 100 km, with the
knobs as they are. The damage split as proposed: "Yes, move them."

**How it works.**

- Each ship file's Weapons group has `hitRadiusKm` (100) and the moved numbers: `slugArmourDamage`,
  `slugCompartmentDamage`, `heArmourDamage`, `empSeconds`, `fireChanceHE`, `fireChancePEN`,
  `breachChanceSlug`. The Damage group keeps armour, the radiation chance, spread, crew losses and
  repairs.
- `DamageSystem.TakeSlug` and `TakeWarhead` take the firing ship's numbers, as `TakeFighterRun`
  already did.
- An enemy missile carries the Garuda's numbers (`Round.by`, set by `LaunchAt`). Its speed and fan
  are the Garuda's too; before, they were ours (the same values).
- `docs/scale-sim` uses 100 km.

**Calls made without the owner (can be changed):** the hit distance is in the firing ship's file, as
the damage is. Both files say 100.

**Checked.** Compiled with no errors. Both ship files re-saved through the Editor. Playtest 209/209
(run 2026-10-27_104545, run by the Worker session while this session's Unity link was down): the
first slug hits with its aim 30 km off, the led shot 6 km off; HE, EMP and PEN hits land; the
Garuda's missiles hit us; the step-22 kill and MISSION COMPLETE still pass. The scale study: a fight
still takes about 11 minutes at x1, 5 slugs for 5 hits.

### Step 89 — UI restructure, steps 1 to 5: fonts, materials, two stencils, the mock-up, the old export

**What it is.**

- The board's fonts are in `Assets/UI/Fonts`, one folder per family, with their `.ttf` files and
  licences: `BarlowCondensed`, `ShareTechMono` and `NotoSansSymbols2`.
- Each panel and frame piece has a **Fonts** list beside its Art list. Swap a font in the list and
  every text under it changes. Each entry is a font, a material preset if the texts use one (Share
  Tech Mono's "Small"), and the texts.
- The board's two materials are in `Assets/UI/Board/Materials`.
- The frame stencils CL II EQUIP and CONSOLE HARNESS are deleted. BUS A-14 and VENT PLENUM 3 are
  left.
- The plot's design mock-up: 43 unused parts are deleted. Kept: the passive label template, and the
  SENSOR RANGE label and value. Their entries in `HandoffBoardMotion` (one drift, one pulse) are
  removed.
- The old design export's reference files are in `Assets/UI/Board/Design`: the README, the asset
  map, the tokens, `layout/layout.json`, the 1920 × 1080 reference picture (the switched-off
  `ReferenceOverlay` uses it), `UIStateSpec.md`, `board-logic.js` and `plot-grid.json`. The rest of
  `Assets/UI2` is deleted, the folder too.

**How it works.** `HandoffPanel.FontSlot` and `ApplyFonts`, called from `OnValidate` in the editor.
`ArtList` uses the same. A material made for another font is ignored, and the font's own material is
used. Every move went through the Editor, so every link stayed.

**Checked.**

- The board rendered before and after the fonts: 0 pixels changed.
- After the stencils and the mock-up: only the two stencils' pixels changed.
- No broken links in the board scene or in any panel prefab.
- Playtest 2026-10-27_105858: 209/209.

**Left:** steps 6 to 8 of the "UI restructure — to do" list.

### Step 90 — UI restructure, step 6: "Handoff" is gone from every script name

**What it is.** Only names changed. The game looks and plays the same.

| Old name | New name | Folder |
|---|---|---|
| `HandoffBoardControls` | `BoardManager` | `Scripts/Game` |
| `HandoffBoardState` | `ShipState` | `Scripts/Systems` |
| `HandoffHit` | `ClickSpot` | `Scripts/UI` |
| `HandoffPanel` | `Panel` | `Scripts/UI` |
| `HandoffBoardMotion`, `HandoffRockers`, `HandoffPlotGrid`, `HandoffMarksGraphic`, `HandoffSweepGraphic`, `HandoffScopeSight`, `HandoffWedge`, `HandoffPolygon`, `HandoffEased` | the same without "Handoff" | `Scripts/UI` |
| `HandoffPlotPanel`, `HandoffPeriscopePanel` | `PlotPanel`, `PeriscopePanel` | `Scripts/UI/Panels` |

The board's root object in `02.1_Grid` is now `Tactical Board` (was `HandoffBoard`). The code that
finds it by name (the playtest and `GammaBoardPresenter`) uses the new name.

**How it was done.** Each file moved with `git mv`, its `.meta` too, so every scene and prefab kept
its link. Then the names were replaced, whole words only, in 39 `.cs` files.

**Checked.** It compiles with no errors. No missing scripts on the board (1,741 scripts, as before).
The board renders the same (36 pixels off by 1 step in 255, scattered: rounding after the reload).
Playtest 2026-10-27_232526: 209/209.

**Trap.** Scene and prefab files still show the old names in their `m_EditorClassIdentifier` lines.
Those are only labels: Unity links scripts by their GUID, which did not change. Unity rewrites a label
when that part is next edited. Don't hand-edit them.

### Step 91 — Each level has its own battle space

**What it is.** The battle space is the area you may fight in. Leave it and you get the turn-back
warning and the countdown. Until now every level had the same one, 100,000 × 100,000 km. Now each
level sets its own.

1. Open a level in **Starwolf → Level**.
2. Press **Battle space**. Two boxes appear under the map: Width, km and Height, km.
3. Type the size. The map's edge follows as you type.
4. Play the level: the board uses that size. The edge, the turn-back warning, the zoom-out limit
   and how far you can scroll all follow it.

A level without a Battle Space keeps the board's size, 100,000 × 100,000 km. The battle space is
always centred on the grid's centre.

**Owner direction (2026-10-27).** Decision 50, as proposed: "Yes, a box per level."

**How it works.**

- `Assets/Scripts/Tactical/BattleSpace.cs`, beside the spawn boxes. At play it sets the board's
  `arenaWidthKm` and `arenaHeightKm` (`ShipState`) and clamps the zoom again. In the Scene view it
  draws the edge with its size.
- The Level window's map uses the level's Battle Space, or the board's size without one. It no
  longer draws outside its frame.
- **Spawns always stay inside** (owner, 2026-10-27). Shrink the battle space past a spawn, or drag a
  spawn out, and the Level window moves it to 3,000 km inside the edge, just clear of the turn-back
  warning (which starts 2,500 km from the edge). One undo puts it back.
- `Level_01` has a Battle Space at 100,000 × 100,000 km, so it plays as before.

**Calls made without the owner (can be changed):** how far inside a moved spawn goes (3,000 km).

**Checked.** Quick check in play mode, with Level_01 set to 60,000 × 80,000 km for the test (not
saved): the board took that size, the zoom-out limit went from 0.35 to 0.47, and the grid's edge
followed. Our ship started at the player spawn, 45,000 km south, so it was outside, as expected. The
window and map by screenshot. Then, at 80,000 km tall, both spawns moved from 45,000 to 37,000 km
from the centre. Level_01 saved at 100,000 × 100,000 km. No new console errors.

### Step 92 — Ruins and debris on the plot, in range

**What it is.** The level's ruins and debris show on the plot only while they are in range of our
ship. With DRADIS on and power on SEN, that is every piece inside the DRADIS circle. With DRADIS off,
it is every piece within visual range, 10,000 km. Out of range a piece disappears again. They are
drawn in grey line art, true to scale, on top of the DRADIS circle, sweep and weapon ring, and under
our ship, contacts and marks. They move with the plot's pan and zoom.

**Owner direction (2026-10-27).** First built as "found by DRADIS, then stays". The owner played it:
"The field … is not meant to be constantly active. It should not be visible unless it's within
your visual range", "not meant to be constantly visible and way beyond the visibility of DRADIS",
"the debris should not be drawn underneath the circles. The debris has to be visible when your
sensors are on." This replaces the rules in Ruins item 1.

**How it works.**

- `TacticalWorld` hides every renderer on the `WorldTerrain` layer when a level loads. Each board
  tick it shows the pieces within range plus half the piece's size, and hides the rest. The range is
  the DRADIS circle (`SensorSystem.PassiveRangeKm`) while DRADIS listens (`Listening`), and visual
  range (`Stats.visualRangeKm`) otherwise. `RuinsInSight` counts them.
- `PlotPanel` makes a camera that looks straight down at the `WorldTerrain` layer, framed exactly
  as the plot (its centre and km per pixel), into a picture the plot's size. The picture is
  `Plot/Ruins (3D view)`, just above `Sensor sweep (drawn)` and below `Own ship (3D view)`.
- The ruins come out as line art by the same full-screen pass as our ship's picture: red lines on
  black. The `BoardLinearImage` material keys the black out and tints the lines with the palette's
  `secondary` grey. The ruins camera has HDR off, as the ship views do.
- Nothing else draws the `WorldTerrain` layer, so a hidden piece is hidden everywhere.

**Calls made without the owner (can be changed):** the grey (`secondary`); a piece counts as in sight
by its centre plus half its size.

**Trap.** A camera picture shown on the board needs the `BoardLinearImage` material. With the
default UI material, the line-art pass shows as red lines on solid black and covers the grid.

**Checked.** Quick check in play mode on Level_01: 15 pieces in sight at the player spawn; 111 at
14,000 km south of the centre, in the near belt; back at the spawn, 15 again. There, DRADIS on with
3 units on SEN (circle 15,600 km): 354 in sight, none outside the circle, drawn over it. Screenshot
`Logs/Shots/ruins_dradis_circle_on_top.png`.

**The hang, solved (2026-10-28, killshot-81).** Two full runs hung with `NullReferenceException` in
`BoardManager.Update` (`state.orders`) and in `Eased.Set`. The cause was not this step's code. Unity's
"Script Changes While Playing" was "Recompile And Continue Playing", so a script saved by another
session during a run recompiled the game mid-run, and the board lost its links to the ship's systems.
It is now "Recompile After Finished Playing" (an Editor preference on this Mac, not in the project).
Run 2026-10-28_012951: 209/209.

### Step 93 — Debris you can fly through; running into a piece damages the ship

**What it is.**

- Level_01's crowded debris is thinned out. Pieces were removed at random, and the shape of each
  belt is kept. The near belt has 50 pieces (was 216), the debris inside the husk 26 (was 81), the
  middle belt 140 (was 228). The far belt and the asteroid fields are unchanged.
- Running our ship into a piece damages it: 25 armour off the side facing the piece. With that side
  open, 1 integrity off the room behind. It hits once each time the ship goes in; staying inside does
  nothing more. The ship is not pushed back out.

**Owner direction (2026-10-26/27).** "If it hits it takes damage. It doesn't get pushed out."
"There's no way that you could even fly through it." Asked whether to spread the debris out: "Yes."

**How it works.**

- `TacticalWorld` checks each board tick whether our ship's centre is inside a piece (the piece's own
  box, level with the piece). On the way in, `DamageSystem.TakeCollision` hits the side facing the
  piece, like a fighter's run (no fire, breach or radiation). `collisionReport` holds the last one
  ("COLLISION · PROW ARMOUR 75%"); `collisions` counts them.
- The numbers are in each ship's stats file, Damage group: `collisionArmourDamage` (25) and
  `collisionCompartmentDamage` (1).
- The Debris Field tool already has a minimum gap (its Gap setting).

**Calls made without the owner (can be changed):** the damage (25 armour, 1 integrity); our ship
counts as a point (its real size is 100 to 500 m); how many pieces each belt kept; only our ship
collides for now (the Garuda's patrol does not steer round debris); no sound yet.

**Measured.** Gap to the nearest piece, median: near belt about 1,000 km before, 1,300 km after
(many of its pieces are next to the husk); middle belt 1,950 km before, 2,180 km after; far belt
4,380 km. Our ship's picture on the plot is about 1,250 km long at full zoom, so in the near belt
it can still look bigger than a gap.

**Checked.** Quick check in play mode: put on a near-belt piece, the prow went to 75%; staying on
it, no second hit; out and back in, the prow went to 50%. Not committed: see Step 92.

### Steps 94 to 101 — Wolf Hunt: the hunt, the fight, the reinforcements, the exit, the tools

Built overnight (2026-10-28) by killshot-81 while the owner slept ("I want a Mac playable at the end").
Every call made without the owner is listed at the end, and can be changed.

**What it plays like.**

1. NEW GAME. "WOLF HUNT" types in green on the phosphor screen, holds, then INITIALISING... and the
   board. No Enter press (Step 94).
2. Our Asura starts at the Player Spawn. The Garuda starts at a random point in its spawn box, facing a
   random way, every game (Step 95). The box stays inside the battle space.
3. The Garuda is bigger and slower than us and hits harder, with shorter reach (Step 96). See its
   numbers below.
4. The Garuda hunts the map (Step 97). It picks points across the battle space, drifting from its
   half toward ours over 15 game minutes. It listens with its own DRADIS. It hears us out to its
   DRADIS reach times our signal: silent, never; at full signal, from about 21,000 km when it is
   calm, 28,000 km when alert. Inside 10,000 km it sees us, whatever our signal. Our fighters are loud
   to it (a signal of 60) and lead it to them.
   - UNEASY: it heard us faintly. It turns for where it heard us.
   - SEARCHING: heard more clearly, or heard again after a gap. It speeds up and searches round that
     spot.
   - ALERT: heard loud and close, seen, or hit. It closes to its railguns' reach, turns a turret's side
     to us, and fires railgun slugs (they fly straight at 2,500 km/s; PDCs cannot stop them) and HE
     missile salvos while it has a fix on us. Out of power (an EMP), it cannot fire.
5. The moment it is ALERT, the alarm strip says REINFORCEMENTS INBOUND, with a 3:00 countdown in game
   time (Step 99). At 0, two Naga-class frigates enter at random edges of the battle space, at least
   25,000 km from us, told roughly where we are. One more comes every 2 game minutes. The strip then
   says ENEMY SHIPS IN THE SECTOR: n. Each frigate hunts with the same mind.
6. Clicking a contact's label on the plot turns the periscope to it and makes it fire control's target
   (Step 99). Before, the target was always the Garuda. A ship in sight that we have not identified
   shows as an amber dot labelled BOGEY (it showed nothing before).
7. The Garuda destroyed or captured: a jump point opens at the middle of the battle space's edge
   farthest from us, 5,000 km in (Step 98). The plot shows it as a green ring (the 2,000 km that
   counts as there) with JUMP POINT beside it. The alarm strip gives its bearing and distance. Reaching
   it wins: MISSION COMPLETE, "GARUDA DESTROYED · JUMPED OUT".
8. Tools (Step 100): the RULER shows only the distance, in larger, sharper text. The COMPASS ring is
   graded like a compass: a tick every 10°, a longer one every 30° with its number, 000 at the top,
   and its label gives the distance and the bearing. The text size is `markLabelSize` (14 pt) on the
   plot, Board tab of Starwolf → Tuning, under Plot tools.
9. Fighters scout (Step 101): any ship or ruin within 10,000 km of one of our fighter flights shows on
   our plot, kept up to date while they are near.

**The numbers.**

| | Asura (ours) | Garuda | Naga frigate |
|---|---|---|---|
| Top speed, km/s | 12.5 | 8.75 | 15 |
| Turn, °/s | 20 | 14 | 30 |
| Side armour (prow, stbd, aft, port) | 100, 100, 50, 100 | 150, 150, 75, 150 | 50, 50, 25, 50 |
| Railguns | 2, 25,000 km | 2, 18,000 km, hits 1.5× | 1, 18,000 km, 12 slugs |
| Missile cells | 8 | 4, HE hits 1.5× | none |
| PDC groups | 2 | 1 (PDC-B emptied) | none |
| DRADIS base reach, km | 12,000 | 10,000 | 10,000 |
| Marines | 18 | none | none |

The Garuda's file is `Assets/Ships/Garuda.asset`; the frigate's is the new `Assets/Ships/Frigate.asset`.

**How it is built.**

- `EnemyBrain` (rewritten): awareness COLD, UNEASY, SEARCHING, ALERT (never goes down); `hunts` turns
  the hunting on (the playtest's older checks set it off); `Brief(where, now)` for an arriving ship.
  Its numbers are on the Board tab of Starwolf → Tuning, under Enemy.
- `WeaponSystem.ShootAt`: an enemy slug into our `Incoming` list. `TickIncoming` tests a slug against
  our own motion through the tick. `Retarget(key)`: a new target resets the ladder and solutions.
- `QuestSystem`: the jump point (`ExitOpen`, `ExitAt`, `ExitReachKm`), the alarm and countdown
  (`AlarmAt`, `Arrived`), the arrivals. `BoardManager.missionAlarm` / `missionCount` put its line on
  the alarm strip; `BoardManager.Alert()` sounds the tone.
- `TacticalWorld.AnyAlert`, `TacticalWorld.AddContact(template, key, at, hdg, brief, now)`;
  `ShipState.AddTrack`. The frigate template is `World/Frigate (template)` in `02.1_Grid`, switched
  off: a copy of the Garuda with the frigate's stats file and its model at 0.6 scale.
- `ScreenFlow.advanceAfterSeconds` (4 s on the briefing). `EnemySpawn.sizeKm` (40,000 × 12,000 km).
- `PlotPanel`: the compass grading (`Grade`), the jump point (`ShowJumpPoint`), contact labels with a
  `plot.look` ClickSpot. `SensorSystem.InSight` and `TacticalWorld`'s ruin sighting count our
  fighters (`HangarSystem.Sees`).

**Checked.**

- A reviewer agent applied every patch to copies and compiled the whole project with Unity's own
  compiler: 0 errors. It found ten problems; the real ones were fixed before anything went in (stale
  alarm strip text, a stuck 0:00, a spawn box past the edge, EMP, PDC AUTO on retarget, identified
  contacts not clickable, unknown contacts in sight not drawn, the awareness middle steps barely
  used).
- In play mode, by hand (stand-ins for the helm): the Garuda started at (-445, 40,038), heading 250.
  With our ship loud and 18,000 km off, it went UNEASY and turned toward us; at 11,865 km it went
  ALERT, turned side-on, fired five slugs and a missile salvo and hit us four times ("SLUG HIT · PROW
  · MAG −3"). The strip said REINFORCEMENTS INBOUND 1:40. Two frigates arrived when it ended, then one
  every 2 game minutes (NAGA 1 to NAGA 6), each SEARCHING where it was told we were. Killing the
  Garuda opened the jump point at (0, 45,000) with "JUMP POINT · BRG 000° · 90,000 KM" on the strip;
  reaching it: MISSION COMPLETE, "GARUDA DESTROYED · JUMPED OUT". No errors in the console.
- Screenshot `Logs/Shots/wolfhunt_tools_check.png`: the graded compass, the ruler's distance, the jump
  point ring.
- The playtest: two checks failed once (PDC-B), because the Garuda's harder HE hits left our magazine
  below the 3 it needs. The playtest now repairs us before PDC-B (a stand-in for damage control).

**Calls made without the owner (can be changed).**

- Clicking a contact's label targets it. The frigate's name (Naga-class), model (the Garuda's at 0.6)
  and numbers above.
- The Garuda's hearing: DRADIS gain 3 when calm, 4 searching, 5 alert; SEARCHING from 0.25 of its
  hearing distance, ALERT from 0.5; a fix lasts 30 game s; railgun reload 12–17 game s; aim off by up
  to 0.4°; a salvo every 90 game s once ALERT.
- The jump point's place and 2,000 km reach; arrivals at a random edge, at least 25,000 km from us,
  told where we are to within 5,000 km.
- The compass grades in our marks' blue; numbers only when the ring is over 40 px.
- The nav wheel art was not used for the compass: the ring is drawn, so it scales with the zoom.

### Step 104 — TextMesh Pro's examples deleted; its shader file put back

The owner said yes to deleting `Assets/TextMesh Pro/Examples & Extras` (2026-10-28). One example font was
in use, by Soham's old `TestScene` and `Prefabs/UI.prefab`: Electronic Highway Sign. It moved, with its
links kept, to `Assets/UI/Fonts/ElectronicHighwaySign`.

The 3 build errors were not the examples. They came from TextMesh Pro's own shaders, which need
`Assets/TextMesh Pro/Shaders/TMPro.cginc`, deleted in February in b47111e ("Removed old project from main").
It is back, taken from the TextMesh Pro package that comes with this Unity. The board's fonts use the Mobile
shader, which did not need it, so the board looked the same before.

### Step 105 — Playing Wolf Hunt for real, and four fixes

The owner asked (2026-10-28): did anyone actually play it, and do the systems work together as a flow? Until
now, no: the playtest checks every control, but with the Garuda's hunting switched off and our ship moved
by hand. So killshot-81 played a game in the Editor through the board's own switches (clicks and drags
sent through the EventSystem, as the playtest does). Only two stand-ins: the A key (called as the key
calls it) and, once, the scroll wheel's zoom.

**What happened.**

1. Power: ENG and SEN on DRAW, throttle up, DRADIS gain 5 (30,000 km). Signal 100%.
2. Flew north for about 7.5 real minutes at x10. The Garuda drifted toward us.
3. Heard it at 29,900 km, bearing 336: the alarm strip, the contact board and the bearing line all came
   up. It had not heard us.
4. It heard us faintly at 20,700 km (UNEASY), and came our way.
5. Cut ENG to give WEP the power. The ship stopped dead. Signal stayed 100% (WEP drew what ENG gave back).
6. CONDITION 2, ACTIVE SCAN, the cone dial onto 321: identified. TRACK, turret B, the ladder. BEARING
   refused until the periscope was on it; clicking its label on the plot did that. Fired at 16,000 km:
   **HIT · GARUDA · PROW ARMOUR 67%.** This all worked, and felt good.
7. It fired two missiles back. Both point-defence groups on AUTO fired nothing: the missiles came from
   our front-left, which neither group covers. Both hit: our magazine down to 2, on fire.
8. Engineering: VENT put the fire out; the repair party's first trip only brought the air back.
9. At 12,200 km it went ALERT: REINFORCEMENTS INBOUND 3:00 came up. It fired a slug every 15 game
   seconds and hit every time (we were stopped). The second slug wrecked the magazine: we could not
   fire again. The breach let the vacuum spread through every room; the crew began to die, the reactor
   fell to 2 units, and DRADIS went deaf. The fight was lost about 70 game seconds after its first hit.

**Fixed (bugs).**

- **A hit did not put the Garuda on ALERT** while it could also hear us faintly: it stayed UNEASY,
  fired only its hit salvo, and the reinforcements' countdown did not start. Now a hit always means ALERT
  (`EnemyBrain.Listen`).
- **"1 HOSTILE SET HAS YOUR RANGE" was false.** It counted every hostile contact once our signal passed
  45%, even 88,000 km away. Now it counts the enemy ships that really hear or see us
  (`EnemyBrain.HearsUs`, `TacticalWorld.HearingUs`), at any signal. Loud and unheard, it says
  NO HOSTILE SET HAS YOUR RANGE. `SensorSystem.Trackers()` is gone.
- **Nothing showed enemy fire coming.** The alarm strip now shows INCOMING MISSILES · BRG 321 (or
  INCOMING FIRE when slugs are in it) with the count, and the alert tone sounds once as it starts.
- **Nothing showed what hit us.** "SLUG HIT · PROW · MAG WRECKED" and Tooling's "COLLISION · PROW ARMOUR
  75%" were written but never shown. Both now take the strip for 4 real seconds
  (`BoardManager.Struck`, **Hit Line Seconds**), with the alert tone. Flying through the starbase
  had cost us half our bow and stern armour, silently.

The strip's order is now: the battle space's edge, what just hit us, enemy fire in flight, the mission's
line, new contacts. The playtest has one new check: the strip warns of the Garuda's salvo. Playtest
2026-10-28_081651: 211/211.

**Questions for the owner (not changed).** These are game feel, so they are yours:

1. **The plot does not follow our ship.** At speed it leaves the screen in seconds; you press C again and
   again. Should the plot follow our ship while under way, until you pan it yourself?
2. **Bearing lines fade after 2 game minutes.** That is 12 real seconds at x10, and our ship moves only
   about 1,500 km in that time, so at 20,000 km the lines are nearly parallel and never cross usefully.
   Make them last longer (say 10 game minutes)?
3. **The PDCs cover only 140° of 360°**: 7°–77° (front-right) and 165°–235° (back-left). Missiles from
   ahead or from the left always get through. Keep it (turning to defend is a skill), or widen the arcs?
   Either way, should the plot show which way to turn?
4. **Noise is total power drawn.** WEP is as loud as ENG, so you cannot be armed and quiet. Intended?
5. **Cutting ENG power stops the ship dead.** It doesn't drift. Intended?
6. **The periscope stays down** over the middle of the plot after you look at a target, hiding our ship.
   Raise it by itself after a few seconds?
7. **The first salvo decides the fight.** The magazine sits behind the bow; two hits there (HE at 1.5×)
   and we can never fire again. A breach then spreads vacuum through every room in under a minute, the
   reactor crew die and power falls to 2 units. Soften it: slower spread, a sturdier magazine, or the
   Garuda's hits back to 1×?
8. **The opening is long:** 88,000 km apart, first contact after about 7.5 real minutes at x10. Start
   closer, or add x20?

The owner's answers to these are in section 8, "The owner's answers (2026-10-29)". Step 106 builds them.

### Step 106 — The owner's play report: keys, the ring, the title, the plot feeds the gun, and the Step 105 answers

The owner played the Mac app (2026-10-28): "the app works", but the instruments did not lead to the
enemy or to a shot, W/S did not work, and P did not pause. Built by the Worker (was killshot-81), the
owner's answers taken first.

**Keys.** W and S step the throttle **Throttle Key Step** (10%) a press, held keys repeating, up to
what the ENG bus allows (`HelmPanel`, Tuning: Helm). P pauses and runs, as the PAUSE key does
(`BoardManager`). Neither works while typing in a text field.

**The visual range ring.** A thin ring round our ship at the visual range (10,000 km, the ship's stats
file), always on: inside it you see ships with your eyes (`PlotPanel`, **Visual Ring Alpha**, Tuning:
Sensors; 0 hides it). The owner chose this look.

**The title.** Two cards, MISSION ONE, then WOLF HUNT, 3 s each, then INITIALISING. `ScreenFlow` has a
list of **Cards** shown in turn. The title screen's letters have no digit 1, so it is spelled out.

**The plot feeds the gun (owner's yes, 2026-10-29).** TRACK now chooses what to lock, in this order: a
scanned enemy the periscope is on (within 4°); else the target, if scanned; else **your plot**: the
newest POINT is where it was, and the newest POINT-to-POINT RULER is its course and speed, reckoned
forward from the POINT's time to now. Fire control's ID reads PLOT, "FROM YOUR PLOT". The knobs then
fine-tune it. So the quiet way works: hear it, cross the bearing lines, drop POINTs, RULER between them,
TRACK, SLEW TO, arm, fire. No long knob drags. (`WeaponSystem.ComputeSol`, `Solution.fromPlot`.)

**The periscope picks the target (owner, 2026-10-29: no clicking the plot to aim).** Clicking a
contact's label or a bearing label does nothing now; the labels let clicks through to the plot. You turn
the periscope by hand (SLEW), or SLEW TO puts it on the solution's bearing. TRACK on a scanned ship
under the periscope makes it the target (the Step 99 call that clicking a label targets it is undone).
The owner said no to plain-word hints on the firing switches.

**Bugs 7 and 8 (Tooling's).** A thrown LOAD whose gate fails (WEP power, the magazine, CONDITION 2,
rounds) or a thrown ANGLE out of arc now drops, with every rung after it, on the next tick; the lever
checks the gates again before it fires (`WeaponSystem.TickCharge`, `Fire`). BEARING is left alone: a
moving target drifts off it slowly.

**The Step 105 answers built here.** (1) The plot follows our ship as it moves; panning stops it, C
follows again (`PlotPanel`). (3) PDC-A covers the starboard half (000–180), PDC-B the port half
(180–360), in the code's defaults and all three stats files, set through the Editor. (5) With no power on
the ENG bus there is no thrust and no braking: the ship drifts on (`NavSystem.Move`). This is every
ship's rule, so an EMP'd enemy drifts too, at full speed (before, it slowed); boarding it then waits
for it to be slowed some other way. **A call made without the owner: change it if EMP should still stop
a ship.** **Owner, 2026-10-29: keep it.** "An EMP should kill the ship's engines. It should shut it all
off, so it will be drifting." (An EMP already empties every bus for its time, `PowerSystem.BlackOut`.)

**Also.** `Level_02` is off the build list and out of the project (the owner: "it can be chucked
away"); a copy is in `~/Documents/GitHub/Killshot-Builds/removed-2026-10-29`. The app's bundle ID is now
`com.shagunshah.starwolf`. For killshot-a2, whose Unity link was down, the Worker ran its Level_01
script (the 60,000 km square: the husk gone, 136 pieces kept, spawns at ±22,000 km; its write-up is
killshot-a2's).

**Playtest changes.** Step 15's check is now "the plot follows our ship" (its image holds still, the
grid moves). A new check: not scanned, TRACK takes the plot. The EMP check is now "without power it
drifts". The periscope stand-in replaces the label click. Two old checks assumed a still plot (marks stay put on screen) and the 100,000 km map's zoom limit (x0.35); both now check the new rules. Playtest 2026-10-29_011619: **214/214**, with killshot-a2's bug 6 checks and Level_01.

### Step 107 — A smaller Wolf Hunt; engineering pays AUX; crew in the stats file

Three of the owner's answers from 2026-10-29. Built by killshot-a2. Its Unity link was down, so the
Worker ran the level script and compiled the code.

**1. The level is smaller.** The opening took about 7.5 real minutes at x10 before the first contact
(Step 105). The owner: "If we need to make a smaller level, let's make a smaller level. Let's even
remove the big piece of debris and have a little bit of debris." The owner chose a 60,000 km square
with a few small debris fields.

- `Base Husk` and `Debris · in the husk` are deleted. `BaseHusk.prefab` stays for later levels.
- Everything else moved toward the centre by 0.6 (100,000 km to 60,000 km), so the layout keeps its
  shape. Pieces keep their size. The asteroid fields moved as a whole, so their rocks keep their
  spacing. Each wreck moved on its own.
- The near, middle and far belts kept about a quarter of their pieces (12 of 50, 35 of 140, 50 of
  202), in a fixed shuffled order. Every kept piece is at least 1,200 km from every other, and none
  is within 3,000 km of our start, so we start in open space.
- The Battle Space is 60,000 × 60,000 km. Player Spawn (0, −22,000); Enemy Spawn (0, +22,000), its
  box 24,000 × 7,200 km. The ships start about 44,000 km apart (the check: 43,592). DRADIS hears
  30,000 km out, so first contact comes after about 2 real minutes at x10 at the Step 105 pace.
- 136 pieces, none outside the battle space. Nothing in code changed: the Garuda's hunt, the jump
  point and the frigates' arrival all work from the battle space's size. The plot now zooms out to
  x0.47 (was x0.35).
- How: a script for Unity's `execute_code`, first tried in Python on the real positions. Unity checked
  it in play mode: no errors.

**2. Engineering pays AUX (bug 6).** Engineering's REPAIR, SEAL and VENT cost the same AUX as the
orders station's (REPAIR 30, SEAL 25, VENT 15). `DamageSystem.EngBlock` now gives the room's reason
first, then "INSUFFICIENT AUX — N% REQUIRED", the orders station's words. The orders station asks
`EngBlock` too, so the two can't disagree. `EngRun` spends the AUX. The rocker locks and engineering's
note shows the reason, as it already did for SEAL THE DOORS FIRST. Playtest: two new checks, REPAIR
spends AUX (100 → 70%), and with no AUX engineering refuses REPAIR. The five engineering orders in a
row cost 130, so the playtest fills the pool to 100 twice as a stand-in.

**3. Crew in the stats file** (owner: "crew numbers need to be a tuning variable and should not be
hard-coded"). The Damage group of each ship's stats file has **Crew**: six numbers, one per room,
in order MAG, BRI, HAB, HGR, RCT, ENG. Each starts at today's 4, 9, 18, 7, 6, 8 (52). Tune the Garuda
and the frigates on the Enemy tab. `Ship.Awake` fills each room's crew from the file, as it does the
marines. The rooms' list in `DamageSystem` no longer has crew numbers.

**Checked.** Parts 1 and 2: playtest 2026-10-29_011619, 214/214 (with Step 106). Part 3: compiled by
the Worker, written into Asura, Garuda and Frigate through the Editor, and COMPLEMENT 52 in play
(commit 7e82b40).

### Step 108 — Keys for the drawers, and a window to change every key

The owner, 2026-10-29: number keys open the systems, and a tool in the Starwolf menu changes the keys.
Built by killshot-76.

**What the player gets.** 1 Fire Control, 2 Sensors, 3 Power, 4 Hangar, 5 Engineering, 6 Comms. A key
opens its drawer, or closes it if it is already out, the same as clicking the systems key. Like P and
WASD, the keys do nothing while you type in a text box (the hangar's launch bearing).

**What the owner gets.** **Starwolf → Keys** opens the Tuning window on a new **Keys** tab. It lists
all 16 board keys: the six drawers, Pause Run (P), Turn Port (A), Turn Starboard (D), Throttle Up (W),
Throttle Down (S), Centre On Ship (C) and the four Pan keys (arrows). Pick a new key from each list.
The Board tab no longer shows these.

**Where the keys are.** The drawer and pause keys are on `BoardManager`, the helm's on `HelmPanel`,
the plot's on `PlotPanel`, each marked `[Tune("Keys")]`. A change made in edit mode goes into the
board scene, so save the scene. A change made in play mode is lost unless "Keep play-mode changes"
is ticked.

**Checked.** Unity compiled clean. All 16 keys are on the Keys tab with their starting keys. In play
mode, through the same call the keys make, each drawer opens in turn and comms closes the second
time; no errors in 636 frames. Nobody
has pressed the keys by hand yet; Unity can't fake a key press here.

### Step 109 — The periscope shows the enemy ship

The owner, 2026-10-29: as you slew the periscope (the targeting computer), the Garuda appears in it,
and you zoom in or out to capture it. Built by Targeting Computer View.

**What the player sees.** Lower the periscope and slew it. When DRADIS hears a ship and it is within
27° of the sight, the ship slides into view, in the sight's red. It is the plot's top-down picture,
always level: if it heads to our right it points straight right, otherwise straight left. Far away it
is small. Turn the MAG knob to grow it. The red lines stay as they were (owner: very important).

**CLASS needs the right zoom.** CLASS works only when the ship fills the brackets: within 15% either
way. Too little zoom and it is too small. Too much and it spills out. At 1× a close ship may spill out
and still count, because the MAG goes no lower. The CLASS lamp glows only when CLASS will work. At 1×
a ship 3,750 km away fills the brackets; at 2×, one 7,500 km away; at 8×, 30,000 km (DRADIS's reach).

**Tuning.** Scope Fit Km (3,750) and Scope Fit Slack (0.15) are in Periscope & fire control. Periscope
Panel has Ship Picture, Bracket Px (42) and Ship Ink.

**How it is built.** `PeriscopePanel` shows the plot's existing camera picture of the contact
(`Contact View Camera · Garuda`) in `Sight/Mask/Ship picture`, sized by zoom and range.
`ShipState.ScopeFill` and `ScopeFits` set the rule, and `Classify` uses it. The picture uses
`BoardLinearImage`, which now has the UI Mask's stencil so the round sight clips it. Nothing that used
it before is under a Mask, so they look the same. No new camera or scene change.

**Checked.** Compiled clean. In play mode the Garuda was put 7,000 km away on 048°, heading 186°, at
MAG 1.9×. It showed inside the brackets, pointing right. CLASS was refused at 8×. The red lines
matched the before shot. The plot's Garuda picture was unchanged. Shots are in `Logs/PeriscopeCheck`.
Pointing left was checked too (heading 338°). The playtest gained one check before the CLASS click:
215/216 in run 2026-10-29_015659, and it passes.

**Owner calls made on the fly.** The CLASS lamp glows only at the right zoom. The brackets stay small
(42 px), so a ship that fits is small in the sight.

**Open, for the owner.** The owner said "I don't want the white reticle." Targeting Computer View asked
whether that means the four white corner marks (the brackets) in the sight; no answer before they went
to sleep. The brackets are unchanged. CLASS uses them: the ship must fill them.

### Step 110 — Zoom out to x0.1; the grid carries on past the battle space

The owner, 2026-10-29: zooming out stopped too soon, and it was hard to see enough. Built by Grid
Visual.

**What the player sees.** The SCALE knob and the mouse wheel go out to x0.1. Past the battle space
the grid carries on: the same big squares, with 5 × 5 small ones inside. A thicker, brighter line
marks the battle space's edge, white on the white grid and amber on the amber grid. At x0.1 Wolf
Hunt's 60,000 km battle space is about a fifth of the plot's width, with grid all round it. Panning
still stops at the edge (owner's pick). Once the whole battle space fits, it stays in the middle.

**What changed.**

- `BoardManager.MinZoom` is just **Zoom Out Limit**, now 0.1, saved in `02.1_Grid`. Step 65's
  second rule is gone. It stopped zooming sooner, where the map filled the plot's width: x0.47 for
  Wolf Hunt.
- `PlotGrid` draws its lines across the whole plot, still counted from the battle space's corner.
  Then it draws the edge on top. **Edge Width Px** (3), **White Edge** (0.6) and **Amber Edge** (0.9)
  are on Plot Grid in the inspector.
- Below about x0.29 the small squares are 5,000 km and the big ones 25,000 km. At x0.1 a small square
  is 30 px.
- Playtest section 25b: the zoom stops at x0.1, the big squares there are 25,000 km, then it zooms in
  to x0.5 before the drag-past-a-corner check. Earlier steps that zoom to 0.25, 0.3 and 0.4 for
  screenshots now really get there. Before, they stopped at x0.47.

**Checked.** Compiled clean. In play mode the mouse wheel stopped at x0.1. Screenshots at x0.1, white
and amber, are in `Logs/GridCheck`. The full playtest was not run, because KM to Units changes the
scale next and will update section 25b again.

**Seen on the way.** The axis numbers count from our ship. At x0.1 they don't sit on the grid lines,
because 60,000 km is not a whole number of 25,000 km squares. This was already the case whenever our
ship was off a line.

**Calls made on the fly.** x0.1 is the owner's first number. The edge is 3 px, at 0.6 white and 0.9
amber. The grid outside is as bright as inside. 5,000 km stays the biggest small square.

**Next.** KM to Units changes the plot to units (760 px per 1,000 units) and takes the zoom limit to
x0.01.

### Step 111 — Units: a small square is 50 units, our ship two squares; zoom out to x0.01

The owner, 2026-10-29: a ship can't be hundreds of km long. Think in units. At x1.0 a small square is
50 units and our ship is 2 squares long. "This does not mean you scale down the ship models. It means
you adjust the size of the squares." Then: "zoom out properly to 0.1 or even 0.01". Built by KM to
Units.

**What the player sees.** At x1.0 the small squares are 38 px, down from 61, and each one is 50 units.
A big square is 250. Our ship is drawn 76 px long, as before, which is 2 small squares: 100 units. The
Garuda is drawn 1.5 times as long: 150 units, 3 squares. Before, both were drawn the same size. The
SCALE knob and the mouse wheel now go out to x0.01. There the small squares are 5,000 and the big ones
25,000, and all of Wolf Hunt's 60,000-unit battle space fits, with grid round it. The readout shows
three places below x0.1 (x0.010).

**What changed.**

- Every ship's stats file has a **Length** in units: Asura 100, Garuda 150, Naga frigate 75. It is in
  Starwolf → Tuning. The plot draws each ship at its length, between 30 and 120 px.
- The plot's scale is 760 px per 1,000 units at x1.0 (`PlotGrid` **Px Per 1000 Units**), up from 60.9
  per 1,000 km: 12.5 times bigger. **Square At Full Zoom** (50) sets the small square at x1.0.
- No game number changed. Speeds, ranges and the battle space are the same numbers, read as units.
  So x0.08 now shows what x1.0 showed before.
- **Zoom Out Limit** is 0.01, saved in `02.1_Grid`. The SCALE knob's sweep covers two steps of ten
  (x1 to x0.01).
- The plot's rings and dashed lines draw only the part that shows on the plot. At x1.0 the
  visual-range ring alone was about 64,000 points, past what one picture can hold, and Unity showed an
  error.
- Playtest section 25b checks, at x1.0: a small square is 50 and a big one 250; our ship is drawn 2
  small squares long and is 100; the Garuda is 150. Then the zoom stops at x0.01, where the big squares
  are 25,000. Steps that zoomed to old values for screenshots now zoom to the same view as before.

**Checked.** Compiled clean. In play mode: at x1.0 a square is 38 px and our ship 76 px; at x0.01 the
whole battle space shows. Screenshots are in `Logs/UnitsCheck` (x1.png, x0.01.png). Full playtest
218/218 (2026-10-29_021252). That run showed the too-many-points error once, without failing a check.
The fix came after it and was checked in play mode: a ring 22,800 px across now takes 5,660 points,
and no errors.

**Calls made on the fly.** The Naga frigate is 75 units. Code names still say km. The board starts at
x1.0, so the Garuda is off screen at the start. The ship picture stays between 30 and 120 px.

**Next.** What zooming out means, then speeds, then ranges (section 8, "Distances in units"). What the
player reads comes last. The owner may try our ship at 4 squares at x1.0.

### Step 112 — Turning after TRACK no longer spoils the aim

The owner, 2026-10-29: "can we fix the ship turn after placing track?" If you turn after TRACK, the
ship fires where the target was worked out to be. It does not keep tracking it.

**The bug.** A firing solution kept its bearing as an angle from our bow. Turn the ship after TRACK
and the angle turned with it. ANGLE checked the old angle, so it could stay OUT OF ARC after you
turned. A shot with no course (a still target, a hand-dialled one, or a POINT with no RULER) went off
by as much as you had turned.

**What the player sees now.**
- TRACK takes the target's position, course and speed at that moment.
- The gun aims where the target should be by now, if it kept going straight. Turning or moving our
  ship doesn't change that.
- It doesn't follow the target after TRACK. If the target turns, the shot misses. Press TRACK again.
- The RNG and BRG readouts change as we turn and move. They show the range and bearing from our bow
  to where the target should be now.
- Turn far enough that the target leaves the turret's arc, and ANGLE drops (with BEARING and ARM).
- Turning the RNG or BRG knob after TRACK moves the aim point. Before, against a moving target, these
  knobs changed only the readout. SPD changes only the speed.

**What changed.** Only `WeaponSystem.cs`. Each game tick, `Follow` works out each solution's range and
bearing from our bow to its point (`Reckon`: the point, moved on along its course and speed). The slug
aims at that point (`LeadBearing`), not along the old angle. `NudgeSol` moves the point for RNG and
BRG on a TRACK solution too.

**Checked.** Compiled clean, no warnings. In play mode on Level_01, with test slugs:
- A still Garuda 46,000 km away, aimed with no course, then we turn 60°. BRG went from -1 to -61, the
  gun still aimed at it, and the slug hit. Fired along the old angle (the way before the fix), it
  missed.
- A Garuda crossing at 8.75 km/s. TRACK, then 20 game seconds and a 60° turn. The slug hit. With the
  Garuda turned north after TRACK, the same solution missed.
- Bow-on, turret A bears. After a 90° turn to starboard, it doesn't.
Full playtest 2026-10-29_100527, run with Step 113: two checks failed because they expected the old aim.
- #89 wanted the dialled solution's RNG and REL BRG unchanged. The point stays put in space, so they
  drift a little as our ship moves on (RNG 10,994 for 11,000, REL BRG off by 1°). It now allows 100
  units and 1°.
- #111 took "unled" as BRG from the bow. BRG now follows the point on along the track, so that shot
  passed 44 off, inside the 100 hit radius. "Unled" is now straight at where the first POINT put it,
  two minutes before, as the check meant.
Both changes are in `PlaytestRunner.cs`. They compiled clean. The playtest has not been run again.

**Fixed after (KM to Units, 2026-10-29; found by killshot-fb).** Each tick BRG is rounded to a whole
degree. A RNG or BRG click rebuilt the aim point from that rounded BRG. So after flying away from a
point, one RNG click moved it 100 out and up to half a degree sideways: 104 units at 24,700, more than
the hit radius. Now a click rebuilds the point from the exact bearing to it. A BRG click still turns it
by exactly 1°. Checked in play mode, hand-dialled and TRACK: a point 25,276 out, after flying away and
turning, RNG +1 gave +100.0 out and 0.00 sideways; BRG +1 gave +1.000°. Full playtest 217/218 (2026-10-29_105333): #89, #111 and the x1.0 checks #205-206 pass; the one failure is #56 RECALL, the old frame timing.

**Call made on the fly.** The RNG and BRG knobs move a TRACK solution's aim point (above).

**Found for the owner.** The helm shows "AUTO — DIRECT" at the start, but no autopilot is built.
Nothing steers the ship. The attitude key NOSE starts lit.

### Step 113 — Everything in the game's workings is in units

The owner, 2026-10-29 (passed on by the hotkeys chat): "speed and range should also be changed to
[units]. Everything should be [units] so that then we can build back from there. This is all backend,
okay? Visually, we can decide if we still want to show kilometres." Built by KM to Units.

**What the player sees.** Nothing new. The board still shows KM.

**What the owner sees.** The Tuning and Level windows say units where they said km: Range Units, Top
Speed Units, Visual Range Units, Width, units, and so on. Tooltips say units and units/s.

**What changed.**

- Every km name in the code is now units, 74 names in 21 files. `rangeKm` is `rangeUnits`,
  `topSpeedKms` is `topSpeedUnits`, `accelKms2` is `accelUnits`, `KmPerPx` is `UnitsPerPx`, and so on.
  Comments and tooltips say units. On-screen text is unchanged.
- No number changed. One old km is one unit.
- Values saved in the ship files, `02.1_Grid`, `Level_01` and the plot prefab keep working: each renamed
  value that is saved somewhere has a tag with its old name (`FormerlySerializedAs`). The files are not
  rewritten. Unity writes the new names the next time each one is saved.
- The rename was done by a script and checked by hand. Two lines that declared two values at once
  were split, so each tag sits on the right value.

**Checked.** Compiled clean, no warnings. All 94 saved values read back the same under their new
names: the three ship files, and every piece of the board scene. That includes values that differ
from the code's defaults (the Garuda's arrive distance 300, the frigate's guns 18,000 and 25,000).
Level_01's battle space came through too: in the playtest the jump point opened at
-25,000, 5,000 in from its 30,000 edge.

Full playtest 213/218 (2026-10-29_100527). None of the five failures is the rename:

- #205 and #206 (square and ship size at x1.0): the shot shows SCALE x0.19. The zoom was moved during
  the run (the owner was in Unity then). No code zooms out by itself. Rechecked in play mode: at x1.0
  the zoom holds, a square is 50 units and 38 px, and our ship is 76 px.
- #89 (the RNG knob's hand-dialled solution: RNG 10,994, REL BRG -28°) and #111 (the unled shot,
  44 off): these checks expect the old aim. Step 112 makes a solution a point in space, with RNG and
  BRG following the bow, so the checks need updating for Step 112. Passed to killshot-fb.
- #56 RECALL: the old frame-timing failure (Step 109).

**Calls made on the fly.** Names end in Units, speeds too (`topSpeedUnits`, units a second). The Level
window's labels and the battle space's label in the Scene view say units, since they are the owner's
tools. The playtest's report text is unchanged.

**Next.** What zooming out means, then speeds, then ranges. The docs checklist under "Distances in
units" in section 8 is still to do in one go.

### Step 114 — Training levels and the MISSIONS list

The owner, 2026-10-29: the first version of the tutorial is a set of short levels. Each has one small
goal and a different mix of tools. The player picks one from a list, and NEW GAME opens the list. The
plan is in `FTUE_PLAN.md` §0. Built by the FTUE chat.

**What the player sees.** NEW GAME opens a green screen with MISSIONS typed in at the top. Under it
are four rows:

1. FIRST SHOT: scan the Garuda and destroy it with the railgun.
2. SILENT SHOT: hit the Garuda once without scanning it. A scan loses.
3. PRIZE: capture the Garuda with the marines. Destroying it loses.
4. WOLF HUNT: as before.

The arrow keys or the mouse pick a row. Enter or a click plays it: INITIALISING..., then the board.
In the first three, the Garuda does not hunt us. It cruises the map, never listens for us and never
fires. Each is won or lost the moment its goal is met. There is no jump point. A lost training goal
says MISSION FAILED, not SHIP LOST. RESTART MISSION plays the same level again.

**What the owner sees.** Three new levels in Starwolf → Level: FirstShot, SilentShot and Prize
(section 2 has where everything starts). The Level window has a Mission button. The Mission box sets
the level's goal. An Enemy Spawn has a Hunts tick box.

**What changed.**

- `Game/Mission.cs` (new): the level's goal. At the start it tells QuestSystem. A level without one
  plays Wolf Hunt, so `Level_01` is unchanged.
- `QuestSystem`: the goals (`MissionGoal`: WolfHunt, Destroy, HitUnseen, Capture) and two fail
  reasons (Scanned, PrizeDestroyed). Wolf Hunt's own rules are untouched.
- `EnemySpawn`: Hunts, on by default. At the start it sets that ship's brain (`TacticalWorld.Hunts`).
- `TacticalWorld.chosenLevel`: the level picked on the list. Empty, the board loads its own
  `levelScene` (`Level_01`), so playing the board straight from the editor is unchanged.
- `ScreenFlow.Play(level)`: what each row calls.
- `EndScreenPanel`: MISSION FAILED and its two reasons. A SILENT SHOT win reads "GARUDA HIT · NEVER
  SCANNED".
- `01_Briefing` is the list (section 2). Build Settings have the three levels.
- Playtest check #5 picks WOLF HUNT on the list. It used to call `ScreenFlow.Advance` straight.

**Checked** in play mode (`Logs/LevelListCheck`):

- The list shows, with FIRST SHOT picked. Its click loads `FirstShot` with goal Destroy, the Garuda's
  Hunts off, both ships on their spawns.
- 230 game seconds at x10 with our signal held at 100: the Garuda stayed COLD, fired nothing and
  cruised.
- Every goal through the end screen: FIRST SHOT destroyed → complete; SILENT SHOT scanned → failed,
  hit → complete; PRIZE destroyed → failed, captured → complete. RESTART MISSION reloads the level.
- WOLF HUNT from the list loads `Level_01` with Wolf Hunt's goal and the Garuda hunting.
- The wins were set by hand, not fought. The playtest's own checks show a shot plotted from two
  POINTs and a RULER hitting the moving, unscanned Garuda.

Full playtest 217/218 (2026-10-29_110455). The one failure is #5, the new list check. It read the
WOLF HUNT row after the board had loaded, when the row was gone. The game did the right thing: the
board opened on `Level_01`, and every check after it passed. The check now reads the row first. It
was rechecked in play mode (four rows, WOLF HUNT found, `Level_01` chosen), not in a full run. #56
RECALL passed this time.

**Calls made on the fly.**

- The title is MISSIONS, not LEVELS: the green screen's letters (`glyphs.json`) have no V.
- In SILENT SHOT a scan loses (`FTUE_PLAN.md` §0, open question 3).
- FIRST SHOT also counts a capture as a win.
- After a pick: INITIALISING..., then the board, with no title card. The MISSION ONE and WOLF HUNT
  cards are kept in the scene, switched off.
- A lost training goal says MISSION FAILED.

**Not done.** No level has been played through by hand, start to finish. Nothing on the board says
the goal once a level has started. (Step 115 added the help bubble.)

### Step 115 — The training levels' names, and a help bubble

The owner, 2026-10-29: "don't call it FirstShot. Need a more cavalier military banter name for each
level, with a subheading that briefly explains what you have to do." And: "a dummy text bubble which
will teach you what to do in each level." Built by killshot-fb, with the FTUE chat's yes.

**What the player sees.**
- The MISSIONS list: 1 WEAPONS FREE, 2 NO PEEKING, 3 FINDERS KEEPERS, 4 WOLF HUNT. Each keeps its
  line under it saying what to do.
- On the board, at the start of every level, a dark box with an amber edge sits at the plot's lower
  left. It is titled BRIEFING and lists what to do, step by step. A click anywhere on it closes it.
  It does not come back until the level starts again.

**What the owner sees.** Each level's Mission box has a **Help** text box. That is the bubble's
text. Change it in the level scene. Empty means no bubble. `Level_01` now has a Mission box too
(goal Wolf Hunt), for its bubble. The bubble is `Help Bubble` under `Tactical Board` in `02.1_Grid`.

**What changed.**
- `Mission.cs`: the Help text; at the start it hands it to `QuestSystem.ShowHelp`.
- `QuestSystem.cs`: links to the bubble and its text; `ShowHelp`; a click on the bubble hides it.
- `02.1_Grid`: the `Help Bubble` object (Image, Button, Outline; Title, Close hint, Text with
  auto-size 14 to 20). Saving the scene also wrote Step 113's new units names into it; the numbers
  are the same.
- `01_Briefing`: three rows renamed (text and object names). The WOLF HUNT row is unchanged; playtest
  check #5 finds it by name.
- The four level scenes: the Help text. The scene files keep their names (`FirstShot` and so on).

**Checked** in play mode (`Logs/HelpBubbleCheck`): the list shows the new names; WEAPONS FREE loads
`FirstShot` with goal Destroy and its bubble; Wolf Hunt shows its bubble; NO PEEKING's text, the
longest, fits at size 20; a click hides the bubble. Compiled clean. Playtest not run.

**Calls made on the fly.** The names. The title BRIEFING. The bubble's place and look. The steps'
words. The rows' lines under the names were kept as FTUE wrote them.

**Not done.** The bubble can't be brought back once closed. It doesn't move on step by step as the
player does each thing; that would be the XO walkthrough (FTUE_PLAN §6).

**The Mac app, rebuilt 2026-10-29 12:17** with Steps 112 to 115, for the owner's demo. Apple Silicon
(the setting had gone back to Intel again; set to ARM64 first). 0 errors. Started once: no errors in
its log, checked only to the menu. The app before it is kept at
`~/Documents/GitHub/Killshot-Builds/Old/Starwolf-2026-10-29-0850.app`.

**Rebuilt again, 12:23, after two owner asks.**
- `Level_02` is gone. The owner made it and deleted it, but the file had stayed. It was taken off the
  build list, and the file and its `.meta` were moved to the Mac's Trash.
- **Escape quits the game at once, at any time:** on the intro, the MISSIONS list and the board,
  even while typing. On the board it is **Quit Key** on the Keys tab of Starwolf → Tuning (`BoardManager`); the intro
  and the list use `ScreenFlow`, with Escape fixed. It does nothing in the editor. Not yet pressed in
  the app: a session can't press keys in it.
- The Mac build is now Apple Silicon in `ProjectSettings` (c3044fa), so it no longer falls back to
  Intel.

### Step 118 — DRADIS starts off

The owner, 2026-10-29: "dradis should be off. you have to start it." Built by killshot-fb.

- At the start of every level our DRADIS rocker is off: no circle, no sweep, no hearing. The player
  turns it on in the sensors drawer (key 2), and it needs SEN power to hear (Step 82). This is
  `OwnShip`'s Sensor System **Dradis** in `02.1_Grid`, now unticked. The Garuda and the frigate
  template keep theirs on: they still listen for us.
- The bubbles say "turn on DRADIS" again (WEAPONS FREE, NO PEEKING). Step 116 had changed them to
  "DRADIS is already on". Wolf Hunt's says "Turn on DRADIS (key 2)".
- Playtest: a new check at step 5, "DRADIS starts off", then a stand-in turns it on, since the
  checks after it expect it on.

**Played in Unity before the final app (2026-10-29, killshot-fb).** WEAPONS FREE, from the MISSIONS
list, through the board's own switches as the bubble says: P; SEN and WEP to DRAW; C2; DRADIS on;
heard at 10,226 units; ACTIVE SCAN and the cone named it GARUDA; SIGHT, SLEW TO, TRACK; the four
rockers; the lever (set by code, as a drag). Hits opened its prow, then its port side. When it
turned past turret A's arc, ANGLE dropped and turret B took over. Then MISSION COMPLETE · GARUDA
DESTROYED. No errors. Screenshot: `Logs/FinalCheck/weapons_free_won.png`.
- **Found and fixed:** our hits stopped the Garuda's engines, but re-arming without a fresh TRACK
  kept leading it at its old speed, so three shots missed (Step 112: the solution doesn't follow the
  target after TRACK). WEAPONS FREE's step 8 said "throw ARM again, pull again". It now says "Each
  new shot: SLEW TO, TRACK, the four switches, the lever." Shots hit again once TRACK was pressed
  each time.
- Not played this time: NO PEEKING, FINDERS KEEPERS (FTUE played those in Step 116) and Wolf Hunt.

**The final app for the owner's demo: built 2026-10-29 12:44**, from e2322b8, at
`~/Documents/GitHub/Killshot-Builds/Mac/Starwolf.app` (Apple Silicon, 0 errors). Started once: no errors in
its log, checked only to the menu.

### Board refactor, 2026-10-26 to 27 (moved from section 8)

- **Navigation (2026-10-26).** All ship movement is in `Systems/NavSystem.cs`: position, heading,
  throttle, turning, speed, drift and placing the 3D model. It replaced `TickHelm` and `TickMove`
  in `HandoffBoardState`, and the model placing in `TacticalWorld`.
  - `OwnShip` and `Garuda · Cruiser` (was `Dummy · TRACK 07`) each carry a NavSystem. `BoardManager.nav` points at
    ours, and every ship state reaches its own through `.nav`.
  - `HandoffHelmPanel` became `UI/Panels/HelmPanel`. It only shows the NavSystem and passes the
    throttle, the A and D keys and the clicks to it.
  - No behaviour change. The playtest passed 166/166 after the move (run 2026-10-26_002152), and
    again after the rename to NavSystem (run 2026-10-26_002808).
- **The enemy is GARUDA (2026-10-26).** Track t07 is named GARUDA, a Garuda-class cruiser, in code and
  in the scene. The contact is `Garuda · Cruiser`. Its model is the StarWolf cruiser mesh.
  Level_01's spawn is `Enemy Spawn · Garuda`. Playtest 171/171 (run 2026-10-26_013236).
- **Helm panel prefab (2026-10-26).** `Assets/UI/Board/Panels/Helm.prefab`, made from the helm in the
  scene. The builder was not used. How to use it: `docs/BOARD_UI.md`.
  - The helm is one object, `Helm`, under `Tactical Board`. Its children are `Closed` and `Open`
    (were `Helm` and `State · helm`). `HelmPanel` moved onto it. The board now finds panels on
    child objects too.
  - Every part is named after its control. Each control's parts are under one parent, for
    example `Open/01 Flight/Throttle/Cap`. The draw order is the same as before. Screenshots
    before and after match pixel for pixel, closed and open.
  - The helm's art is a list on `HelmPanel` (**Art**, 20 entries). Swap a sprite there and every
    image under it changes. The list is in `Panel`, and every panel has one.
  - Art files moved, and their links still work. Art only the helm uses went to
    `Assets/UI/Board/Art/Helm`. Art other panels use too went to the folder for its kind
    (`Art/Lamps`, `Art/Bolts` …). Four shapes went to `Art/Shapes`. Only the @2x files the board
    uses moved. The 1x copies are still in the export.
  - Playtest 171/171 (run 2026-10-26_014759).
  - The palette asset came later (see "The shared colour palette" below).
- **Power drawer prefab (2026-10-26).** `Assets/UI/Board/Panels/Power.prefab`, the same way as the
  helm. Owner 2026-10-26: panels go in system order, and each system's rules move after its panel.
  - `Drawers/State · power` became `Drawers/Power`, and `HandoffPowerPanel` moved onto it. The
    playtest now finds the drawer by its script.
  - Parts named and grouped: `Frame`, `Header`, `Reactor`, `Load shed`, `Buses/ENG` … `Buses/AUX`
    (each with `Cells` and `Selector`). Screenshots before and after match pixel for pixel.
  - Art list: 20 entries. The load-shed rocker is not in it. The code swaps its sprite. Its two
    sprites are the panel's **Rocker Up** and **Rocker Thrown** fields.
  - Art moved: 7 power-only files to `Art/Power` (the casting among them), and 8 that other panels
    use too to the folder for their kind.
  - The playtest now films the Game view at the view's 16:9 size. It used to film at a fixed
    1920 × 1080 (owner 2026-10-26; written by another session, committed here).
  - Playtest 171/171 (run 2026-10-26_021347).
- **Power (2026-10-26).** The reactor and the four buses are in `Systems/PowerSystem.cs`: each bus's
  units, selector and signal per unit, the load shed, the reactor cap, and filling and draining once
  per tick. It replaced the buses, `CycleBus`, `ToggleShed` and `TickPower` in `HandoffBoardState`.
  - `OwnShip` and `Garuda · Cruiser` each carry a PowerSystem, with the same numbers the board had
    (10 units, 5 a bus). `BoardManager.power` points at ours, and every ship state reaches
    its own through `.power`. The enemy's engines start powered (`PowerEngines`), as before.
  - `HandoffPowerPanel` became `UI/Panels/PowerPanel` (its link kept, so the prefab still holds it).
    It only shows the PowerSystem and passes the shed and selector clicks to it.
  - **Name clash.** Soham's old `PowerManager.cs` already had a class called `PowerSystem` (one power
    bar). Owner 2026-10-26: his became `PowerChannel`. Only the name changed, in his three files;
    nothing was deleted.
  - Kept though nothing uses them: `Climbing` and `Available` (the design's p1–p5). The audit decides.
  - No behaviour change. Playtest 171/171 (run 2026-10-26_075922).
- **Sensors drawer prefab (2026-10-26).** `Assets/UI/Board/Panels/Sensors.prefab`, the same way.
  - `Drawers/State · sensors` became `Drawers/Sensors`, and `HandoffSensorsPanel` moved onto it.
    The playtest now finds every panel with `GetComponentInChildren`. It works before and after
    each panel moves.
  - Parts grouped: `Frame`, `Header`, `Masters/DRADIS` and `Masters/ACTIVE SCAN`, `E-SIG`,
    `01 Sensor array` (`Gain`, `Bands/THERM` …), `02 Active scan` (`Cone dial`, `Narrow`, `Widen`),
    `03 Contact board` (`Row 1` … `Row 3`). Screenshots before and after match pixel for pixel.
  - Art list: 19 entries. The code swaps the two master rockers' sprites. Those sprites are still
    the **Rocker Up** and **Rocker Thrown** fields.
  - **Links outside the drawer.** The panel also fills the signal box in the frame's top-right and
    the plot's SENSOR RANGE label. A prefab can't link to objects outside it. Those 17 fields are
    empty in the prefab file. They are set on the board's copy. Open the board scene to see them.
  - Art moved: 7 sensors-only files to `Art/Sensors`, `well_r3` (the signal box uses it too) to
    `Art/Wells`, `round_2` to `Art/Shapes`.
  - **No folder called "Shared"** (owner 2026-10-26: name a folder after what is in it). Art more
    than one panel uses is now in a folder for its kind: `Art/Lamps`, `Bolts`, `Knobs`, `Switches`,
    `Latches`, `Screens`, `Plates`, `Wells` and `Shapes`.
  - Playtest 171/171 (run 2026-10-26_080658).
- **Sensors (2026-10-26).** DRADIS and the active scan are in `Systems/SensorSystem.cs`: gain, band,
  scan cone, the ship's own signal, which contacts are identified, the new-contact count, the bearing
  trail, and what is heard, covered by the cone or in sight. It replaced those fields and rules in
  `HandoffBoardState`, and `TickSensors`.
  - `OwnShip` and `Garuda · Cruiser` each carry a SensorSystem, with the board's numbers.
    `BoardManager.sensors` points at ours, and every ship state reaches its own through
    `.sensors`.
  - The contact list (`tracks`) stays on the ship state, and the sensors read it. `classified` stays
    too: the periscope fills it, and it moves with the periscope.
  - `HandoffSensorsPanel` became `UI/Panels/SensorsPanel` (its link kept, so the prefab and the 17
    links on the board's copy still hold).
  - No behaviour change. Playtest 171/171 (run 2026-10-26_081854).
- **Comms drawer prefab (2026-10-26).** `Assets/UI/Board/Panels/Comms.prefab`, the same way.
  - `Drawers/State · comms` became `Drawers/Comms`, and `HandoffCommsPanel` moved onto it.
  - Parts grouped: `Frame`, `Header`, `01 Receiver` (`Band selector`, `Scope` with its 56 bars,
    `Tuned`, `Lock`), `02 Decrypt` (`Message` with its 59 letters, `Key dial 1` … `4`). The bars
    are in their original order inside `Scope`. Some of them draw over the tuning knob.
    Screenshots before and after match pixel for pixel.
  - Art list: 20 entries. The lock rocker's sprites are still separate fields on the panel.
  - Two band-lamp glows are empty, as before: the design gives only the first band lamp a glow.
  - Art moved: 6 comms-only files to `Art/Comms`, `rocker_30x38_locked` to `Art/Switches`.
  - Playtest 171/171 (run 2026-10-26_082511).
- **Comms (2026-10-26).** The radio is in `Systems/CommsSystem.cs`: band, frequency, the carrier lock,
  the four cipher dials, the message and its cipher. It replaced those fields and rules in
  `HandoffBoardState`.
  - `OwnShip` and `Garuda · Cruiser` each carry a CommsSystem, with the board's numbers.
    `BoardManager.comms` points at ours, and every ship state reaches its own through `.comms`.
  - `HandoffCommsPanel` became `UI/Panels/CommsPanel` (its link kept, so the prefab still holds it).
  - No behaviour change. Playtest 171/171 (run 2026-10-26_084735). The run before it failed 3 checks
    (our ship moving on the plot, and the POINT and RULER staying put) and passed on the re-run. It
    had waited about 11 minutes on a Unity save box first.
  - **Trap.** Twice, starting the playtest froze Unity on a "save changes?" box while Unity was
    behind another window, even though the scene had just been saved. With Unity in front, it
    started at once. Bring Unity to the front before running the playtest from another program.
- **Fire control drawer prefab (2026-10-26).** `Assets/UI/Board/Panels/Fire Control.prefab`.
  - `Drawers/State · fire control` became `Drawers/Fire Control`, and `HandoffFireControlPanel`
    moved onto it.
  - Parts grouped: `Frame`, `Header` (with `Sight`), `Scope bearing`, `01 Weapon system` (`System
    selector`, `Mount selector`), `02 Firing solution` (`Track button`, `Range knob`, `Bearing knob`,
    `Speed knob`), `03 Capacitor` (`Cells`), `04 Arming sequence` (`Housings`, `LOAD`, `ANGLE`,
    `BEARING`, `ARM`), `Fire lever`. The RAILGUN, PDC and MISSILE labels are outside the selector's
    group. They still draw over the mount knob they touch. Screenshots before and after match
    pixel for pixel.
  - From this panel on, a group stretches to fill its parent. Before, a group had the board's
    fixed size. For a drawer the result is the same. The periscope needs groups that stretch: its
    height changes as it moves.
  - Art list: 19 entries. The four arming rockers' sprites are still separate fields on the panel.
  - Four lamp glows are empty, as before: the design gives those lamps none.
  - The playtest found the TRACK lamp by its old name (`lamp#82`) and crashed. It now looks for
    `Lamp` beside the button, in the `Track button` group.
  - Art moved: 4 files to `Art/Fire Control`; `glass_r3_c` and `screen_r4` to `Art/Screens`,
    `knob_knurl` to `Art/Knobs`, `rocker_housing` to `Art/Switches`, `button_push` to a new
    `Art/Buttons`.
  - Playtest 171/171 (run 2026-10-26_223837).
- **Hangar drawer prefab (2026-10-26).** `Assets/UI/Board/Panels/Hangar.prefab`. Done before the
  weapons system. The owner had paused Tooling.
  - `Drawers/State · hangar` became `Drawers/Hangar`, and `HandoffHangarPanel` moved onto it.
  - Parts grouped: `Frame`, `Header`, `01 Bay status` (`Doors selector`, `Doors`, `Pressure`,
    `Catapult`, `Cat charge`), `02 Squadrons` (`Squadron 1`, `Squadron 2`), `03 Launch order`
    (`Ship selector`, `Bearing`, `Command selector`), `04 Launch sequence` (`Housings`, `CREW` …
    `CATAPULT`, `Telemetry`), `Launch lever`, `Recall`. Screenshots before and after match pixel
    for pixel.
  - The selection bezel moves between the two squadron rows in play. The squadron groups come after
    it in the list. It still draws under both rows, as before.
  - Art list: 19 entries. The six stage rockers and the RECALL button (with its pressed overlay)
    are not in it. The code swaps their sprites.
  - Squadron 2's selection lamp has no glow, as before.
  - Art moved: 6 files to `Art/Hangar`, the 24 × 32 rockers to `Art/Switches`, `glass_r3` to
    `Art/Screens`, `round_5` to `Art/Shapes`.
  - Playtest 171/171 (run 2026-10-26_225037). The two runs before it (22:44:33 and 22:45:40)
    don't count: another session recompiled and entered play mode during them.
- **Weapons (2026-10-26).** Fire control and everything it fires are in `Systems/WeaponSystem.cs`: the
  weapon groups (railgun, PDC, missile), mounts, warheads, the arming ladder, the firing solutions,
  picking a target, salvo, PDC loaders and AUTO, the capacitor and the fire lever; leading a moving
  target; slugs and missiles in flight; enemy missiles coming in and our PDCs' chance at them.
  - `OwnShip` and `Garuda · Cruiser` each carry a WeaponSystem, with the board's numbers.
    `BoardManager.weapons` points at ours, and every ship state reaches its own through
    `.weapons`. The Garuda's fire-back reads its own launcher and fires at our ship, as before.
  - Stays on the ship state: the contact list, damage (`TakeSlug`, `TakeWarhead`), kills, the
    periscope and the plot marks. The weapons read them.
  - The PDCs still roll the ship's shared dice (`NextDie`), so every PDC and damage roll falls as
    before.
  - The weapon group type is `WeaponSystem.Group` (was `HandoffBoardState.WeaponSystem`). The
    capacitor's tick is `TickCharge` (was `TickWeapons`).
  - `HandoffFireControlPanel` became `UI/Panels/FireControlPanel` (its link kept, so the prefab still
    holds it).
  - **Owner 2026-10-26: the board's weapons stay as they are, not rebuilt on Soham's `WeaponBase`.**
    His railgun fires an instant beam through Unity physics in real time. The board's slug flies in
    game time and has to be led. Hitting debris can be added to the board's own code. `CLAUDE.md`
    now says where the board's weapons are.
  - No behaviour change. Playtest 171/171 (run 2026-10-26_231109).
- **Hangar (2026-10-26).** The hangar is in `Systems/HangarSystem.cs`: the squadrons and their craft,
  the launch stages, the bay doors, the launch bearing and lever, squadrons in flight, the fighters'
  attack runs, and the assault boats with the marines they carry.
  - `OwnShip` and `Garuda · Cruiser` each carry a HangarSystem, with the board's numbers.
    `BoardManager.hangar` points at ours, and every ship state reaches its own through
    `.hangar`.
  - Stays on the ship state: `captured` (the ship's own status, like `destroyed`), the crew
    (`TotalCrew`, `KillCrew`), damage, kills and the alert gates. Boarding still rolls the ship's
    shared dice (`Roll`), so every roll falls as before.
  - The fire lever's threshold is the hangar's `leverFireAt` (0.9), as it always was; fire control now
    reads it as `hangar.leverFireAt`.
  - `HandoffHangarPanel` became `UI/Panels/HangarPanel` (its link kept, so the prefab still holds it).
  - No behaviour change. Playtest 171/171 (run 2026-10-26_232330).
- **Engineering drawer prefab (2026-10-26).** `Assets/UI/Board/Panels/Engineering.prefab`.
  - `Drawers/State · engineering` became `Drawers/Engineering`, and `HandoffEngineeringPanel`
    moved onto it.
  - Parts grouped: `Frame`, `Header`, `Compartments` (`Ship picture`, and one group per room:
    `MAG`, `BRI`, `HAB`, `HGR`, `RCT`, `ENG`), `Status`, `Acting on` (`Selector`), `Repairs`
    (`Housings`, `REBOOT`, `REPAIR`, `SEAL`, `VENT`, `RECALL`, `SPARE`). The old row highlight
    stays, switched off, as `Compartments/Old row highlight (off)`. Screenshots before and after
    match pixel for pixel.
  - Art list: 17 entries. The six repair rockers are not in it. Each one has an up, a locked and a
    thrown sprite on the panel.
  - Art moved: the casting and the REBOOT rocker sprites to `Art/Engineering`; the 22 × 30 rockers,
    the blank rocker and `rocker_housing_r3` to `Art/Switches`; `plate_dark` to `Art/Plates`.
  - Playtest 171/171 (run 2026-10-26_232931).
- **Damage (2026-10-26).** Damage is in `Systems/DamageSystem.cs`: the six compartments (integrity,
  crew, air, fire and radiation), the armour on each side, what a slug, a warhead or a fighter run does,
  fire, vacuum and radiation spreading, the crew, and damage control (the selected compartment, the
  repair orders and the team out).
  - `OwnShip` and `Garuda · Cruiser` each carry a DamageSystem, with the board's numbers.
    `BoardManager.damage` points at ours, and every ship state reaches its own through
    `.damage`. The Garuda took our damage numbers each tick (`CopyDamageTuning`). Since Step 80 it
    uses its own, in `Garuda.asset`.
  - Stays on the ship state: whether it is `destroyed` or `captured`, its EMP clock (`empUntil`, which
    power reads), and its dice (`Roll`, `NextDie`), so every roll falls as before. The orders station,
    the alert condition and the rockers that flick back stay too, for the orders step; the orders
    station's repairs call `damage.EngBlock` and `damage.ApplyOrder`.
  - `HandoffEngineeringPanel` became `UI/Panels/EngineeringPanel` (its link kept, so the prefab still
    holds it).
  - No behaviour change. Playtest 171/171 (run 2026-10-26_233713, after the owner cleared a Unity save
    box that held it).
- **Orders station prefab (2026-10-26).** `Assets/UI/Board/Panels/Orders.prefab`, the same way as the
  helm.
  - The orders station is one object, `Orders`, under `Tactical Board`. It has `Closed` and `Open`
    (they were `Orders` and `State · orders`). `HandoffOrdersPanel` moved onto it.
  - Parts are grouped: `Bolts`, `Header`, `Alert` (with `Selector`), and in `Open` also `AUX pool`
    (with `Cells`) and `Order switches` (`Housings`, `PARTY`, `SEAL`, `VENT`, `FIRE`, `BLKHD`,
    `SCRUB`). Screenshots before and after match pixel for pixel, closed and open.
  - Art list: 17 entries. The three working order rockers are not in it. Their sprites are the
    panel's **Rocker Up**, **Rocker Locked** and **Rocker Thrown** fields.
  - Art moved: the two body plates and `latch_track_orders` to `Art/Orders`.
- **Fix: the hangar's launch bearing box (2026-10-26).** Moving the hangar script onto `Hangar`
  broke the box's two links to it. Typing a bearing did nothing. The docs agent and the checker
  agent both found this. The links point at `HangarPanel` again, in the prefab and on the board's
  copy. The playtest has a new check: it types a bearing into the box and reads it back. No other
  panel had links like these.
  - Playtest 172/172 (run 2026-10-26_234914). The two runs before it (23:43 and 23:46) don't count:
    the board was in use during them.
- **Orders (2026-10-26).** The orders station is in `Systems/OrdersSystem.cs`: the alert condition, the
  AUX order pool, what each order costs and why one is refused, and the repair and order rockers that
  flick back after a throw.
  - `OwnShip` and `Garuda · Cruiser` each carry an OrdersSystem, with the board's numbers.
    `BoardManager.orders` points at ours, and every ship state reaches its own through
    `.orders`. The weapons, hangar and damage systems read its alert (`WeaponsReleased`,
    `FlightOpsReleased`, the repair rate). Its repairs are damage control's (`damage.EngBlock`,
    `damage.ApplyOrder`).
  - `HandoffOrdersPanel` became `UI/Panels/OrdersPanel` (its link kept, so the prefab still holds it).
  - No behaviour change. Playtest 172/172 (run 2026-10-26_235522).
  - **The ship state now holds only what is the ship's own or shared:** the contact list, whether it is
    destroyed or captured, the EMP clock, the dice, kills, the periscope, the plot marks and the
    battle space. Every ship part is a system.
- **Periscope prefab (2026-10-27).** `Assets/UI/Board/Panels/Periscope.prefab`.
  - `Periscope` is a clip window. It grows from 44 px (housed) to 656 px (lowered). The periscope
    script `PeriscopePanel` is now on it.
  - Parts are grouped: `Carriage`, `Bolts`, `Sight` (with `Brackets`), `Readout`, `Mag knob`,
    `Convergence`, `Slew button`, `Subject` and `Classify button`. The mount plate, caption, screen
    and text stay at the top level. In a group they would draw under the aperture, which overlaps
    them.
  - The groups stretch with the clip, so the parts stay put as it grows.
  - The prefab is saved lowered, so the whole periscope shows when you open it. The copy on the
    board is housed. Screenshots before and after match pixel for pixel, housed and lowered.
  - Art list: 15 entries. The aperture is not in it. The code swaps its idle and slewing sprites.
  - Art moved: 8 periscope-only files to `Art/Periscope`.
  - `State · periscope` (the LOWERED label) stays on the board. The board's motion script shows it.
- **Plot prefab (2026-10-27).** `Assets/UI/Board/Panels/Plot.prefab`. `PlotPanel` is now on
  `Plot`.
  - The plot's parts already had plain names (`Ground`, `Grid`, `Marks (drawn)` …). They keep them.
  - `Mockup` is now `Design mock-up (off)`. Its 46 parts are the design's sample picture, all
    switched off. Three are still used: `Passive label template` (the plot copies it for passive
    bearing labels), and the SENSOR RANGE label and value (the sensors drawer links to them).
    The board's motion script also animates some of them. Nothing was deleted. Step 89 later
    deleted the other 43.
  - Art list: 5 entries (ground, glow, fighter wedge, scanlines, vignette). Art moved: the glow,
    scanlines and vignette to `Art/Plot`, `round_6` to `Art/Shapes`. The fighter wedge stays in
    `Assets/Art/Hulls`.
  - The whole board matches pixel for pixel before and after.
- **Owner 2026-10-27: no full playtest per change.** Each change gets a quick look in play mode.
  The periscope and the plot were checked that way, then tested together in one full run:
  172/172 (run 2026-10-27_001637).
- **Frame pieces as prefabs (2026-10-27).** `Frame`, `ToolRail`, `SystemsRail`, `TimeControl`,
  `Console`, `AlarmStrip` and `SignalBox` are prefabs in `Assets/UI/Board/Panels`. They keep their
  object names: the board script and the playtest find some of them by name.
  - They have no panel script. Owner 2026-10-27: each gets a small new script, `UI/ArtList.cs`,
    that has only the Art list. The list code is shared with the panels (`Panel.ApplyArt`).
    The pause key and the four rate keys are not in it: the board script swaps their sprites.
  - Parts are named after what they are. `Frame`, `TimeControl` and `Console` have groups
    (`Conduits`, `Corner bolts`, `Stencils`; `Clock`, `Pause`, `Rate keys`; `Power`, `Scale`,
    `Tint`). The rails have no groups. Each key overlaps the next by 3 px, and its plate and lamp
    draw later. A group per key would put a plate under the next key. `SignalBox` keeps its
    readable names, and its ten cells are in `Cells`.
  - The playtest finds the alarm strip's words as `AlarmStrip/Words` now.
  - Art moved: each piece's own art to `Art/Frame`, `Art/ToolRail`, `Art/SystemsRail`,
    `Art/TimeControl`, `Art/Console` and `Art/AlarmStrip`. Art both rails use went to `Art/Rails`
    (the rail frame), `Art/Buttons`, `Art/Lamps` and `Art/Plates`.
  - The whole board matches pixel for pixel before and after. A look in play mode: the clock, rate
    keys, tool rail and periscope all respond. Playtest 191/191 (run 2026-10-27_010011).
- **Fix: the periscope's convergence bar.** The docs agent found that the script's convergence bar
  was the part named `Subject/Rule short`. It is now `Convergence/Bar`, with its track
  `Convergence/Track`. The group that was called `Convergence` is now `Slew readout`: the bearing,
  the SLEW label and its marker.
- **New art needs one import setting.** The board's sprites have **sRGB (Color Texture)** off. The
  board blends in gamma space. The old import tool set this. Tooling deleted that tool with the
  builder (owner 2026-10-27). Set it off by hand on any new board art.
- **The shared colour palette (2026-10-27).** The board's colours are in one file:
  `Assets/UI/Board/Palette.asset` (script `UI/BoardPalette.cs`). It has 22 colours, each with a
  note on what it is for: green for working, amber for caution, red for alarm, the text greys, the
  unlit lamps and cells, and white for our ship, fighters and slugs.
  - Every panel and the board script read their colours from it. Change a colour in the file, and
    every panel that uses it changes. A test in play mode turned the green to magenta: the power
    readout, the core lamps, the helm's STEADY and lamp and the orders condition all changed at once.
  - Before the change, all 106 colour values on the board matched the palette. Nothing was lost.
  - Playtest 191/191 (run 2026-10-27_010831).
  - Five colours belong to one panel each and stay on it: the plot's axis numbers and amber sweep,
    the sensors' band and contact row tints, and the periscope's rail legend.
  - Our ship's colour on the plot was in the Tuning window's Board tab. It is in the palette now
    (**Our ship**).
  - A panel with no palette file set uses the design's own colours.

---

## 7 · How to keep this log

This file is the handoff. Update it at the end of every step:

- Move the step to Done in the table in section 1.
- Fill in what actually went into the scene, in section 2.
- Add any new script, shader or material and what it is attached to, in section 3.
- Add any decision that was made, in section 4, so nobody reopens it.
- Add anything broken, half-finished, or any trap that cost you time, in section 5.
- Write up the step itself in section 6.
- Update **Open items** at the bottom: take out what you finished, add anything you left behind.

Nothing else in the repo gets written by the log keeper. Only this file.

---

## 8 · Open items

Still not done. **This is the list to work from.** Read it before picking up any work.

### Open after Step 178: mission 1 and the battle group (2026-11-10)

- **Rename mission 1.** Owner 2026-11-10: "The mission should not be called Orientation." No new
  name yet. Ask the owner. The title is in `Assets/Campaign/Campaign.asset` → Missions → Element 0 →
  Title. Its brief there also says "orientation checklist". Update this log, the board README and
  `README.md` to the new name. The level file `Assets/Scenes/Levels/Orientation.unity` can keep its
  name: players never see it.
- **Upload the mission 1 video to Google Drive** (owner: "put it in our Google Drive in Starwolf
  … verify that it's actually uploaded on Google Drive in Chrome"). The file is
  `Logs/Orientation/Starwolf - Mission 1 Orientation.mp4` (6 min 30 s, 1920 × 1080, 57 MB). Not done:
  Claude in Chrome was not connected. Upload it through Chrome, then find it with a Drive search
  before saying it is there.
- **Run the full playtest.** Not run since Step 178. Its checks for SKIRMISH, MISSION SELECT and the
  sector map were changed but not yet run.
- **Mission 1's talk lines and a Starwolf → Mission window**: the Mission Setup Tool chat is
  planning them with the owner. Keep two facts true in any rewrite: the group is under way and we
  keep station in its middle (THROTTLE), and the group waits south of the rock field (TAKE POINT).
- **The battle group stops south of the rock field** and never reaches the target zone: the carrier
  and battleship don't fit between the rocks. Owner to say if that is right.
- **Missions 2 to 10 are not built.** Next is mission 2: pirates in a debris field, with weapons on.
  Talk it through with the owner before building. The old levels stay switched off and are never
  deleted. Each mission copies the pieces it needs from them.
- **The owner's save in slot 1** has wins for the old level names, so its sector map starts again at
  mission 1.
- **Shared index:** `README.md`, `docs/BOARD_UI.md` and `docs/CONTEXT_INDEX.md` are staged
  by an unknown chat from a HEAD older than Step 178. Committed as they are, they would undo lines
  from Steps 178 and 183. Rebuild them on the current HEAD first.
- **Docs not yet brought in line with Step 178:** in this log, section 1's "The first mission, Wolf
  Hunt, is playable", section 2's flow (00_Start → 01_Briefing → 02.1_Grid) and its `00_Start` and
  `01_Briefing` entries, and section 3's ScreenFlow and MISSION SELECT lines. In the board README, the
  Briefing Icon's "In FLIGHT DRILL it is 230 px". In `docs/CONTEXT_INDEX.md`, the MISSIONS list
  mentions.

### The small number box on the Hangar panel: closed (Step 149, 2026-11-02)

The box can no longer stop a launch. BEARING OUTSIDE LAUNCH CONE no longer exists. The box and its
red number are still on the panel and do nothing else. The owner did not recognise the box. Do not
ask about it again.

### False dots: removed (Step 157, 2026-11-02)

Steps 148 and 151 built false dots near rocks, on our plot and for the enemy. Step 157 removed them
for both sides (owner: "the rock should be silent until you ping... those false red dots don't make
sense"). A rock is silent until ACTIVE SCAN pings it. Do not ask about it again. False dots from
radiation are a later idea (For later talks, below).

### Radio countdown on the strip: not fixed (2026-11-02)

A small known problem. The strip shows the radio countdown jammed against its text, like
59INCOMING SIGNAL.

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
5. **Full playtest** not run since Step 175.

### FLIGHT DRILL: calls for the owner to check (Step 144, 2026-11-01)

The owner gave the session a free hand. These calls were made without asking. Each can be changed.

1. **The name.** FLIGHT DRILL.
2. **Its place.** First on the MISSIONS list, and the first pick.
3. **ENG 1 is half of full ENG** for acceleration and turn rate: 1 to 2, and 10 to 20° a second.
4. **ENG 0 still turns**, at the ENG 1 rate. It has no thrust.
5. **Side speed** is 20 at full ENG.
6. **A slide stops by itself** when the key is let go. The owner changed this in Step 171: the slide
   keeps going and fades.
7. **A slide adds no signal.**
8. **The 13 steps and their wording** (section 2, `FlightDrill`).
9. **The parked Garuda.** It is still and silent at the far corner, beyond DRADIS reach of the course.
10. **No speed readout.** The full-throttle step ends at speed 45, and the player can't read the speed.
    The note says to take the throttle to 100.
11. **A hard check at step 1.** ENG must be on STANDBY with 1 to 3 units. Past 3, the player has to
    VENT down first.

The board opens paused, and ENG gains nothing until time runs. The first note says to press `P`, as WEAPONS FREE's and INTERCEPT's briefings do.

A very short tap of `Q` or `E` used to do nothing. Fixed in Step 171: every tap counts.

### Review of the game, and the owner's answers (2026-11-01)

A session reviewed the game as a designer and as a gamer. It used the playtest shots, the level text
and the code. Nobody played the game by hand for this review. The owner answered the gamer's list
and agreed the first jobs.

**The jobs, in order.**

1. **A briefing button.** Built in Step 134: a small icon next to the time control.
2. **Split WEAPONS FREE.** It takes about 19 actions to reach the first shot. The plan is in
   `FTUE_PLAN.md` §0.
3. **Real sounds.** Every board sound is a generated tone. The only sound file in the game is the
   intro hum. Start with the switches, the lever, the railgun, hits and the kill. Where the sounds
   come from is not decided.
4. **A bigger hit and kill.** Built in Step 131: the shake on a hit or crash, and the ship breaking in
   two.
5. **Military labels.** Owner: the labels "ARE weird". The words must be real military words, short
   and clear. The owner said yes to the wording below. Built in Step 133.
6. **x5 is the top rate (owner: fine for now).** Built in Step 133. Pause stays. Crossing the whole
   20,000 map at top speed takes about 80 seconds at x5.

**Left as they are (owner).**

- The lowered periscope covers our ship and its gun cones in the normal view.
- DUMMY 2 and stability, which are not built yet.

**Scan, plot and lock: built in Step 141.** The owner's words: a scan has to help the shot, you can't
board what you can't scan, locks are short, and you plot because you don't know where the ship is
going. The rules are in `TACTICAL_BOARD_SPEC.md` §9.4 and §18.4. killshot-e7 handed these rules to
killshot-d2 on 2026-11-01, with the hand checks at the top of this file.

**Dropped from the review.** It said most shots were at a dot and that the flight to the jump point
was long. The ship files changed while it was being written, and both points are out of date. At
18:26 the files read: top speed 50 (25 at base power), railgun 2,000, missile 3,000, PDC 600, visual
range 2,000, DRADIS 5,000 to 7,500, and `Level_01` 20,000 x 20,000.

**Label wording (owner's yes, 2026-11-01).** Only these labels change. Every other label stays as it
is. The owner rejected THROW THE SWITCHES, NOT YET and LOUD · NOT HEARD YET. Don't use those or
anything like them.

Signal box:

- NO HOSTILE SET HAS YOUR RANGE → UNDETECTED
- 1 HOSTILE SET HAS YOUR RANGE → DETECTED BY 1 HOSTILE
- 2 HOSTILE SETS HAVE YOUR RANGE → DETECTED BY 2 HOSTILES
- EMISSIONS DECAYING — NOT YET RESOLVED → EMISSIONS DECAYING
- POWER SIGNATURE — NOT YET RESOLVED → EMISSIONS LOW

Alarm strip:

- NEW CONTACT SINCE COMMIT → NEW CONTACT
- NEW CONTACTS SINCE COMMIT → NEW CONTACTS

Fire control:

- CHARGED — GATE WILL ARM → CAPACITOR CHARGED
- CHARGING AT +N/S — GATE LOCKED → CAPACITOR CHARGING +N/S
- GATE LIVE, under ARM → ARM READY
- LOCKED, under the switches → INTERLOCK. The lock on a ship uses the word LOCKED too. The design's
  spec calls these interlocks.

Playtest checks 38, 77 and 80 read these words. Step 133 measured each new label in its box.

### Triage: everything left, sorted (2026-10-29, killshot-a2)

The owner asked for everything left over, with nothing missed. This list sorts every open item in
this file into: blocks play, hurts play, owner only, and later. The detail stays in each item's own
section below. **Who:** "Worker" is the session that was killshot-81.

**Blocks play** (a new player can't get through Wolf Hunt)

Nothing open. Item 1 (the instruments) was built in Step 106: the ring, the plot feeds the gun, the
periscope picks the target. Item 2 (the first salvo) was answered "no change".

**Hurts play**

3. The Step 105 questions: answered and built in Steps 106 and 107.
4. Bugs 7 and 8: fixed in Step 106.
5. Bug 6: fixed in Step 107.
6. W/S, P and the title: done in Step 106.
7. **Seven font files rewrite themselves** whenever Unity runs. See trap 28 in section 5.

**Owner only**

8. The Mac app play-through: done by the owner on 2026-10-28.
9. `Level_02`: deleted in Step 106. An empty scene of that name came back on 2026-10-29, and came off
   the build list on 2026-11-01.
10. Optional: give the terminal Accessibility permission, so a session can play the app itself.

**Traps for the next build.** The disk is nearly full (about 4.4 GB free on 2026-11-01; each app is
about 340 MB). Never launch the app with a window size: it saves it. The ARM64 setting is saved in the
project since c3044fa, so it no longer goes back to Intel.

**Later, in the owner's order: refactor, then tools, then the mission scripter, then features**

- **Refactor** ("Board refactor" below). Step 2: the playtest into the Test Runner. Step 5: move the
  last scripts out of `Scripts/Tactical`. Step 8: the clock, pause and tick from `BoardManager` into
  `GameManager` (move `ReadOnlyAttribute` out of the old `GameManager.cs` first). Step 3: the unused
  files were deleted on 2026-10-29 (below); left: `PowerSystem.Climbing` and `.Available`, which
  nothing calls.
- **UI restructure step 8, the check: done (2026-10-29).** No missing scripts in the four build
  scenes or the board's prefabs; nothing points into `UI2`; the last playtest passed 211/211.
- **Tools:** the mission scripter (Starwolf → Mission). Plan it with the owner first. The Mission
  Setup Tool chat is talking it through (call signs, who speaks, a line for each checkpoint). It is
  planned on top of the Campaign file, `Assets/Campaign/Campaign.asset` (Step 183). No Starwolf →
  Mission menu item exists yet.
- **Features, each planned with the owner:** the combat autopilot (A/P); ruins block shots and hide
  us; what is left of the hangar launch; a faster way to dial long ranges; salvage, fuel and range;
  enemy boarders; the conditions page abilities and the FIRE, BLKHD and SCRUB switches; a verdict on
  Soham's 8 unused scripts (read each in full first); spoken orders; the XO walkthrough, derelicts,
  the disguise and fog of war; the old build's helm art (the shader the owner liked and the orange
  nav wheel). Done since: the grid forming (Step 120), the periscope showing the real enemy (Step
  109), the plot grid touch-up (Step 139).
- **Small jobs:** Board follow-ups 3 and 4 (helm, orders and periscope don't slide; the wreck's grey;
  one contact model at a time). From Step 27: the corner microtext; `Title` and `StartButton` in `00_Start` are switched
  off, not deleted (delete with the owner's yes); SETTINGS does nothing; LOAD GAME is dimmed. The
  Claude Design request (the owner: send it soon). The whole-project folder cleanup (the unused files
  went on 2026-10-29).

The owner's answers (2026-10-29) are built in Steps 106 and 107.

**The owner's hygiene answers (2026-10-29).**
- **Delete the unused things** (git keeps every one): the two old demo builds and `node_modules` out
  of git (the builds moved to `~/Documents/GitHub/Killshot-Builds/`); the unused game files
  (`Assets/Screenshots`, `Art/Icons`, `Art/Frames`, `hull_wedge_outline.png`, `ui_square.png`,
  `TacticalGrid.shader` and `.mat`, three unused materials, `PlotAnnotation.cs`, `MissionClock.cs`,
  six empty folders); the old QA folders in `Logs`; `docs/reference-sim`. See "Deleted on 2026-10-29"
  below for what is done.
- **`Starship_Silhouette.png` stays deleted** (the owner's ChatGPT art; git has it at `499909d`).
- **The Claude Design request: send it soon.** Next: bring `CLAUDE_DESIGN_NEEDS.md` up to date (PDC
  quadrants are now two halves; contact clicking changed in Step 106), draft the request, the owner
  sends it. Keep `HANDOFF_GAPS.md` and `CLAUDE_DESIGN_NEEDS.md` until then.
- **The XO walkthrough and the derelicts are still wanted, later**, once Wolf Hunt feels right.
- New plans this day, below: distances in units, the plot zoom-out, comms intercepts.

**Deleted on 2026-10-29** (owner's yes; checked first: no scene, prefab, asset or material refers to
any of them, and no script names them; git keeps them all):
- In `Assets`: `Screenshots` (19 PNGs), `Art/Icons` (23), `Art/Frames` (4),
  `Art/Hulls/hull_wedge_outline.png`, `UI/ui_square.png`, `UI/Shader/TacticalGrid.shader` and `.mat`,
  `UIScreenIcons.mat`, `Weapon.mat`, `Shader Graphs_GridSpace.mat`, `Scripts/Tactical/PlotAnnotation.cs`
  and `MissionClock.cs`, and six empty folders (`_TransferTmp`, `Font`, `UI/Scene`, `Plugins`,
  `Resources`, `Settings/Build Profiles`).
- Out of git: `Starwolf_Demo_Build_v0.1` and `Starwolf_Demo_Build_v0.2.zip` (moved to
  `~/Documents/GitHub/Killshot-Builds/Old`; `.gitignore` now keeps builds out), `node_modules`,
  `package.json` and `package-lock.json` (an unused animation library), and `docs/reference-sim`.
- On disk only: the old QA folders in `Logs` (`HandoffQA`, `Shots`, `Checks`, `FighterArtTest`), and
  the videos and shots of every playtest run but the newest two. Every `report.md` is kept.
- Checked after: Unity refreshed and compiled with 0 errors and 0 warnings (Targeting Computer View).
  Commits cca9051 (the deletions) and fba384c (this log).

**Marked done on 2026-10-29** (these were still listed as open): the Garuda hunts you (Step 97); the
enemy notices us, P3 (Step 97); hitting debris hurts the ship (Step 93); the battle space per level
(Step 91); the UI restructure check (above).

### For later talks (owner, 2026-11-01)

The owner asked for these to be kept for later discussion. None of them is built.

- **Signal is really heat.** The ship has radiators. The more power it uses, the more heat it
  radiates. Gas clouds cool it down. The board keeps the words signal and emissions for now. When
  the UI is next updated, decide whether it says HEAT instead.
- **An art and UI asset manager.** The owner wants one place to swap all the art and UI assets.
  Today the art files are in `Assets/UI/Board/Art`, and each panel has its own Art list.
- **The level editor.** Done in Step 136: drag, resize, edit what you click, Save, and grid numbers.
  Zooming the map was offered and left out.
- **The sneak level is built.** Step 146 built it as COLD RUN, an S-shaped map. All today's levels are tests and get deleted in the end.
  Wolf Hunt is not the level that matters. Weapons are off. A far jump point is the goal, and it shows
  from the start. The map is one long winding corridor made of debris. The enemy patrols the corridor
  on a route set on the Level map. The player watches its path and moves from gas cloud to gas cloud.
  You lose when your ship is destroyed. There is no separate "caught" rule. Since Step 148, rocks cut
  the enemy's hearing and block its sight by the same rule as ours.
- **INTERCEPT is built on the sneak level** (owner 2026-11-01, built in Step 148). It is a copy of COLD
  RUN's map with the radio on. You sneak past the Garuda to the jump point and use the radio to work
  out where it is going. This replaced its first goal, finding the Garuda on DRADIS.
- **Interference from radiation** (owner 2026-11-02). A place in a level that gives off radiation.
  Inside it, DRADIS shows false dots, for us and for the enemy. Not built. Step 157 removed the false
  dots near rocks.

### Play-through 2: scan, fighters, missiles, boarding against the live Garuda (2026-10-29, Worker)

The owner asked whether fighters, active scan, missiles and boarding were ever tested for real. In the
playtest they are, with stand-ins (the Garuda held still, our ship placed by hand, the scan faked, the
marines sure to win). So the Worker played a second game in the Editor, on the 60,000 km Level_01,
through the board's switches. PlaytestRunner step 21b now scans for real: the ACTIVE rocker and the
cone dial onto GARUDA's bearing identify it (was a stand-in). Commit e69e8b5. Full playtest
2026-10-29_015659, with all of that day's work: 215/216. The one failure, #56 "RECALL eases down while
held (.1 s)", is a frame-timing check that has failed the same way before on a busy machine, not a code
fault.

**Worked, in order.** First contact at 29,960 km after about 2 real minutes at x10 (it was 7.5 on the old
map). With ENG cut, the ship drifted on. Fighters: CONDITION 2, doors, the six stages, the lever, four
interceptors away. They scouted (the Garuda showed on our plot at 17,800 km) and made attack runs:
its bow to 30, its stern **open**. The fighters' hit put it on ALERT, and the strip warned of its
missiles. ACTIVE SCAN with the cone dial identified it; TRACK, SLEW TO, the missile ladder and the lever
sent an HE salvo of two. PDC-B went on AUTO.

**Found (for the owner):**

1. **Fighters always know where the enemy is.** They fly to its true position even when we have only
   heard its bearing. The launch bearing only sets where they take off; there is no "scout along a
   bearing" order. So scouting can't fail, and the hunt loses its search.
2. **A fighter's hit tells the Garuda where our ship is,** not where the fighters are: it gets a fix on
   us and opens fire on us (`EnemyBrain.Listen`, the hit branch uses our position). Probably a bug.
3. **One magazine hit leaves us defenceless.** The magazine sits behind the bow. A wrecked magazine stops
   the railguns and missiles, and the PDCs too: PDC fire needs `WepReady`, which needs the magazine,
   though PDC rounds are counted per mount. Should PDCs keep firing with the magazine gone?
4. **Its railguns don't miss.** Drifting straight at 14,000 km, we took a slug every 15 game seconds:
   three opened the bow, wrecked and breached the magazine, and the vacuum spread to every room; the
   reactor fell to 2 units and DRADIS went deaf. The same as the first game. Our missiles take about
   150 game seconds to cover 15,000 km; its slugs, 6. (The owner chose "whoever shoots first"; here our
   fighters shot first, and still lost the duel.)
5. **"HANGAR BAY DAMAGED — CANNOT SPOT CRAFT"** shows when the hangar is only airless (integrity 5/5).
   Wrong words, or the wrong rule.
6. ~~**The plot can't show the fight.** At its widest zoom (x0.47 on the 60,000 km map) it shows about
   14,000 km each way, so a contact at 15,000–30,000 km, inside our DRADIS circle and our railgun's
   reach, is off the screen.~~ Fixed in Step 130: the map is 20,000, and at x0.058 all of it fits the
   plot's height.
7. **Boarding is still untested in a real game.** The Garuda's stern was open, but by then our hangar
   was airless and the boats could not launch.

None of these were changed: they are game feel or game rules, so the owner decides.

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

- At x1.0, one small plot square is 50 units. Our ship is drawn 76 px long, as now, and is 2 squares
  long: 100 units. The squares shrink to fit the ship: 38 px each, down from 61. The ship art stays the
  same size.
- The Garuda is 1.5 times our ship: 150 units, 3 squares. Today the plot draws it the same size as ours.
  That changes with this work.
- Order: sizes at x1.0 first, then what zooming out means, then speeds, then ranges. What the player
  reads (km or anything else) comes last.
- Build the sizes step together with the plot zoom-out below. On its own, the plot at x1.0 shows only
  about 2,250 units, and zoom-out stops at x0.35. DRADIS and the Garuda would be off screen.
- This replaces decision 13 ("one world unit is one kilometre"). `scopeFitKm` (periscope, 3,750) is
  `scopeFitUnits` since Step 113.
- **Checklist for the docs: done (2026-11-01).** This log's decisions, section 2 and Wolf Hunt's
  numbers; `docs/CONTEXT_INDEX.md`; `docs/scale-sim`; `docs/TACTICAL_BOARD_SPEC.md`;
  `docs/FTUE_PLAN.md`.

### Plot zoom-out (owner, 2026-10-29). Built in Step 110; the owner's answers built too

From Grid Visual. Built: zoom out to x0.1, the grid past the battle space, a heavier edge line, panning
still stops at the edge. Step 111 took the limit to x0.01 with the change to units. Step 124 set it to
x0.1 and replaced the square rule below (decision 53). Step 130 set the limit to x0.058, and Steps 135
and 139 set the squares as they are now.

- **Still to do, owner's call (2026-11-01):** the Garuda's 350 x 100 is a trial. Our ship's old
  VLS has no hard point. Choosing MISSILE now picks the first launcher on a dot, VLS-A (Step 143). It
  fires to the left side only. No control picks VLS-B yet. If a ship's size changes, move its hard
  points to the new edges too.
- **Done in Step 130:** at x0.058 the whole 20,000 map fits the plot's height, with the railgun and
  missile rings.

- The owner's answers (2026-10-29), all settled:
  - Panning stops at the edge, never past it. As built.
  - The grid outside the battle space is not dimmer. As built.
  - The edge is amber on the white grid and red on the amber grid (amber would not show there). Built:
    `PlotGrid` White Edge and Amber Edge (commit 365056f), and written into `Plot.prefab` (767786d).
    Checked in play mode at x0.02, both tints (`Logs/GridCheck/edge_*.png`).
  - Replaced in Step 124: a big square held 5 x 5 small squares, and what each square was worth
    changed as you zoomed. It was 10 x 10 from Step 124. Since Step 139 it is 5 x 5 again, and the
    squares change size as you zoom.
- **Trap: Unity keeps old values in a loaded prefab after a recompile.** A new default in a script does
  not reach a prefab that is already loaded, even when the prefab file doesn't list the field. The
  edge stayed white in play mode until `Plot.prefab` was saved with the colours in it.
- Also from Grid Visual: the spec has "Body cam — not built yet" under §12.2 (commit b8dffe9), an idea
  for a marine helmet-camera window during boarding. Not to be built yet.

### Comms intercepts (owner, 2026-10-29). Built in Steps 137, 140 and 148. Every level uses them now

**Still to do.**

- A read message goes when the next one comes in. There is nowhere to look back at old ones, and the
  plan is to do the mathematics across several messages.

**Calls made while building, confirmed by the owner on 2026-11-02.** These were never answered in the
2026-10-29 talk, and the owner said to start building:

- A soft tone plays when a message comes in.
- 60 seconds to lock. 30 to 60 is good. It is adjustable as On Air Seconds on each level's Enemy
  Spawn. Locked in time, a message stays for as long as you need.
- A missed message is lost (GDD p. 31).
- The bearing in a message is always a direction word such as NORTHWEST, never a number.
- Comms hears a ship from a very large distance, so it hears the Garuda anywhere on the 20,000 map for
  now. The exact number doesn't matter yet.

**A message is guidance (owner 2026-11-02).** It gives the player a reason to move in a direction and
think. It doesn't have to be a real decision by the enemy ship in the story.

The speed is rounded to the nearest 5. Only one message is on the air at a time.

**The wording (owner 2026-11-01).** It is an intercepted message, so it doesn't speak to us:
"GARUDA HEADING NORTHEAST SPEED TWENTY BEARING NORTHWEST". The bearing is the Garuda's from our ship.
It is one of the eight words, as the owner said on 2026-10-29. The "NORTH OF YOU" wording in Steps 137
and 140 is replaced.

**The plan as agreed.** The 30,000 km and "SPEED NINETY" below are older than the Step 130 numbers. The
5 minutes below is replaced: a message about every 60 seconds (owner 2026-11-01).

From the hotkeys chat. Comms gets real signals. It becomes the main way to find the enemy beyond
DRADIS's 30,000 km.

- The Garuda sends a radio message about once every 5 game minutes.
- LOCK does not draw a bearing line on the plot.
- Every message has a new code. The player keeps solving codes over time with the four dials. Each
  message starts with the sender's full name, GARUDA (six letters). The code is made new for each
  message.
- Decoded, the message gives three things: which way the Garuda is from us, which way it is heading,
  and its speed. The player uses them to work out where it is. Directions are in words, eight of them:
  north, northeast, east, southeast, south, southwest, west, northwest.
- No quadrants, and no markings on the grid (the owner first wanted "QUADRANT 2", then dropped it the
  same day).
- Everything in the message is in words, the speed too (for example "SPEED NINETY"), so the whole
  message is in code and has to be decoded. "Which way it is" is a word like "northeast". There are no
  digits in the message. Nothing about comms is open now.
- In WEAPONS FREE, NO PEEKING and FINDERS KEEPERS, comms has one fixed made-up signal (band 2, 72.2) with a fixed message,
  and nothing reads the decoded text.

### Found by two audits (2026-10-29, Worker)

The owner asked for the old logs to be gone through, so nothing they asked for slips, and for a check
that the restructure was really done. Two agents read every owner message in the 26 past sessions,
and checked the restructure against the code. Read-only; nothing was changed. Items already in the
triage above are left out.

**Owner requests never done, or only partly** (not in the triage above):

- **Supercruise switch on the helm** (owner, 2026-10-25). Never answered, never logged.
- **Set the heading by bearing or with the mouse** (2026-09-01; the old orange nav wheel on the helm,
  2026-10-04 and 10-17). Only A/D work, 3° a step.
- **A stale firing solution expires after about 20 s** (2026-09-01). Not built, never logged. Maybe
  dropped when the handoff's fire sequence came in; nobody confirmed.
- **Salvo, PDC quadrants, LOADERS and warhead controls on fire control** (2026-10-04, 10-23). Salvo
  is in the Tuning window only; the others are not drawn.
- ~~**The comms cipher does nothing in play** (2026-09-01: "keep the comms game"). It solves on the
  panel; nothing reads it.~~ Real messages built in Steps 137 and 140, in INTERCEPT and Wolf Hunt.
- **Ruins hide us; ruins block shots** (2026-09-01). Listed under features; not built. Gas clouds hide
  us since Step 129.
- **The cloak (Reflex)** (2026-09-01: "I like the cloak"). Only the spec mentions it.
- **Waypoints** (2026-09-01: "later"). Not tracked anywhere until now.
- **Engineering's missing displays**: conditions per room, broken parts, team roster, door state
  (owner 2026-10-24: "request + placeholder"). Only the armour picture is done (Step 74). Damage build
  steps 5–7 (teams, breakdowns, displays) are open.
- **The board fits the MacBook's 2880 × 1864 screen** with dark bands above and below (Step 87). Filling
  them is "a later UI job".
- **Fighter loadouts and fuel; salvage and intel from boarding** (2026-09-01). Not built.

**Owner questions never answered** (beyond the triage): the autopilot's TAIL facing; the asset audit's
28 items (`ASSET_AUDIT.md`, in git at 12ccdd1); the interceptor art on the hangar panel; the calls made
overnight in Steps 94–101 (none confirmed yet). Answered since: our slugs glide (Step 138); stability
is left as it is (owner, 2026-11-01); every `report.md` is kept (owner).

**Answered by the owner but not acted on:** the damage displays (10-24). The grid glide-in was built in
Step 120. (`Assets/Screenshots`, `PlotAnnotation.cs`, `TacticalGrid.shader/.mat` and the
other unused files were deleted on 2026-10-29, with the owner's yes.)

**Records that disagree with the code:**

- ~~Step 96 says the Garuda is "bigger". No stats file has a size; every hit uses one 100 km radius.
  Bigger only in its numbers, not on the plot or to a slug (not checked in the scene).~~ Fixed: ships
  have sizes and hit boxes since Steps 123 and 126.
- The playtest keeps only the newest run's video, and screenshots from 3 runs. The owner asked for the
  last two runs to keep both (`PlaytestRunner.cs`, around line 102).
- ~~`FTUE_PLAN` still lists the Garuda's hearing, searching and firing back as not built (Step 97 built them).~~ Fixed: it reads "Done, Step 97".
- ~~`docs/BOARD_UI.md` says the palette has 23 colours.~~ Fixed 2026-10-29 (22).
- ~~The knowledge graph (`graphify-out`) was last built on 2026-10-27, before the Handoff renames.~~ Rebuilt 2026-11-01.

**Restructure: done**, apart from what the triage lists (refactor steps 2, 5 and 8). Also found:
`BattleSpace.cs` belongs on step 5's list of scripts to move out of `Scripts/Tactical`; the first plan
renamed `TacticalWorld` to `WorldManager`, and no record says that was dropped (owner's call); the
unused files from the old 02_Grid commit (4e43ea9) were deleted on 2026-10-29; one object still has a record-number name, `text#406 LOWERED` in
`02.1_Grid` (rename in the Editor); `HangarSystem.cs` scuttle sets the reactor to 0 directly instead
of through `DamageSystem`; `PlotPanel.cs` asks for a text field every frame (minor).

### Wolf Hunt — the first level. Built in Steps 92 to 107

**What it is.** The first level, called WOLF HUNT. Our Asura starts at the bottom of the map. The
Garuda starts somewhere at the top. It patrols and does not know where we are. We hunt it, and it
hunts us. We stay quiet, scan, send fighters ahead to scout, and pick our moment: guns,
a missile salvo, or a broadside followed by marines. If it finds us, it calls for help, and a
countdown starts. When the countdown ends, enemy frigates arrive. Kill or capture the Garuda, and a
jump point appears. Reach it to win.

**Since Step 130** the level is a 20,000-unit square with no husk, a little debris and three gas
clouds; the ships start about 16,000 units apart. The km numbers below are older than Step 130. Now:
the ranges and speeds are in Step 130's row. The jump point is 1,700 units in and reached within 700.
The arrivals come 700 in, told where we are to within 1,700. The Garuda searches within 2,000.

**The owner's goal for today:** a playable Mac app of this level, and game feel. "The fun of the
game is the hunt." Every tool must make sense.

**How it plays.**

1. Pick WOLF HUNT on the MISSIONS list. INITIALISING..., then the board.
2. We start at the Player Spawn, at the bottom. The Garuda starts at a random point in its spawn box
   at the top, facing a random way. We only know it is somewhere up there.
3. We move through the ruins. The rocks show only within visual range. Past it, ACTIVE SCAN shows
   them as blinking hollow circles (Step 157). We keep our signal low. Fighters scout ahead: what they see shows on our plot.
4. The Garuda patrols the ruins. It listens with its own DRADIS. If it hears us, it grows uneasy,
   then searches where it heard us, then sweeps actively, closes and fights side-on.
5. When the Garuda finds us (ALERT), the alarm says REINFORCEMENTS INBOUND, and a 3-minute
   countdown starts (game time).
6. At 0, two frigates enter at the battle space's edges and hunt us. One more comes every 2 minutes
   until we leave.
7. The Garuda destroyed or captured: a jump point appears on the plot at a far edge. We fly to it.
   Reaching it wins: MISSION COMPLETE.
8. We lose if our reactor reaches 0, or if we stay outside the battle space too long.

**The owner's answers (2026-10-27).**

- The countdown starts when the Garuda finds us (its ALERT). Stay hidden, and it never starts.
- When it runs out, real enemy ships arrive: 2 frigates, then 1 more every 2 minutes. Countdown 3
  game minutes. Frigates: small, fast, weak, one railgun each. Placeholder models are fine.
- The exit is revealed at the kill (or capture), at a far edge. Reaching it wins.
- The Garuda against our Asura: speed 70% of ours, turns slower; armour 1.5×; railgun hits 1.5×
  harder, but its reach is 18,000 km (ours 25,000); 4 missile cells; one PDC group (ours two); no
  fighters and no boats; DRADIS hears a little less far. All tunable after the owner plays it.
- The Garuda starts at a random point in its box, every game.
- The start: WOLF HUNT, then INITIALISING with its loading bar. The DEMO page goes. No Enter press.
- Fighters as scouts: any ship or ruin within 10,000 km of a fighter shows on our plot, kept up to
  date while they are near. Their engines are loud, so the Garuda can hear them too.
- Ruler: distance only, in bigger, clearer text. No bearing.
- Compass: a graded ring, 0 at the top, 359 to the left, 1 to the right, with the bearing and the
  distance. Use the nav wheel art if it fits.
- Debris: the gaps must be wide enough to fly through, at the plot's zoom limits. Hitting a piece
  damages the ship, with no push-out (owner, 2026-10-26).

**Calls made without the owner (can be changed).**

- ~~Clicking a hostile contact on the plot also makes it fire control's target (`ShipState.LookAt`).~~
  **Undone in Step 106:** `LookAt` only aims the periscope now; TRACK on a scanned ship under the
  periscope makes it the target.
- The frigate is the "Naga-class frigate" (NAGA 1, NAGA 2…), a placeholder name. It uses the Garuda's model
  at 0.6 scale. Numbers: top speed 15 km/s, turns 30°/s, armour 50/50/25/50, one railgun (12 slugs,
  18,000 km), no missiles, no PDCs.
- The Garuda's second PDC group is emptied rather than deleted (the code expects two groups).
- Its awareness: SEARCHING from 0.25 of its hearing distance, ALERT from 0.5, a fix lasts 30 game s;
  railgun reload 12–17 game s; aim off by up to 0.4°; a missile salvo every 90 game s once ALERT. Our
  fighters are as loud to it as a signal of 60. All on the Board tab of Starwolf → Tuning, under Enemy.
- The jump point: the middle of the battle space edge farthest from our ship, 5,000 km in; reaching
  within 2,000 km wins. The arrivals: a random edge, 2,000 km in, told where we are to within 5,000 km.

**This replaces:** the sample game: Wolf Hunt is the sample game. FTUE_PLAN §1's
"no reinforcements" and "ends at the kill" (owner, 2026-10-27: the newer rule wins). The XO
walkthrough and the derelicts (FTUE_PLAN §5–8) are not in today's build. The mission
scripter stays for later: Wolf Hunt's countdown, arrivals and exit go straight into `QuestSystem`
today. The scripter will turn them into editable steps.

**Not today:** fog of war beyond "ruins in range" (Step 92); the disguise mechanic; the XO; derelicts;
enemies that use engineering, comms or the plot tools.

### Board follow-ups

From `UI_INTEGRATION_LOG.md`, section 10 (deleted; `git show 12ccdd1:Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md`). In order:

1. **Audit the project's existing UI assets** against `CLAUDE_DESIGN_NEEDS.md`. **Done 2026-10-05,
   verified:** `ASSET_AUDIT.md` (deleted; `git show 12ccdd1:Assets/Scenes/Tactical/ASSET_AUDIT.md`). Of 28 items, 5 fit as-is, 7 fit after a small change, 10 get partial
   help, 6 have nothing. Nothing is used yet; each use needs the owner's yes.
2. **The short Claude Design request**, when the owner asks. Drop row 2.7 of
   `CLAUDE_DESIGN_NEEDS.md` (own-ship blink): decision 34 says our ship never blinks.
3. **Helm, Orders and the periscope don't slide yet** (decision 35). They open at once, or drop
   inside their own clip. If they are made to slide, clip them to the frame's opening and keep them
   under the rails, as Step 66 did for the drawers.
4. **Left from Step 75.**
   - A wreck slows to a stop. From full speed it takes about 30 game seconds. Owner 2026-10-26:
     keep it that way, or a little quicker. A wreck must not coast on at its speed. The rate is
     `NavSystem.Stats.brakeKms2`, in each ship's stats file, shared with live ships.
   - The wreck's grey (`wreckInk`, #8A959D) is pale next to our white ship. Tune it if it is hard to
     tell apart.
   - Only one contact can show as its model at a time (`contactShipImage`). A second one in visual
     range shows as an amber dot.
5. **To do after Steps 75–76** (owner, 2026-10-26). All done. The two frame stencils were deleted in
   Step 89, and the visual range ring was drawn in Step 106. Step 130 tuned visual range and DRADIS
   reach, and brought `scalesim.py` up to date. The spec's numbers were updated on 2026-11-01.
6. **A little more life in the plot's grid** (owner, 2026-10-28). Dropped: the owner chose a quieter
   grid instead (Step 139). The full green phosphor plot is also **dropped** (owner, 2026-10-28):
   don't build it.

### Board refactor (owner, 2026-10-25). In progress (paused and restarted by the owner, 2026-10-26)

**Where it stands (2026-10-27).** Done: all eight ship parts are systems in `Scripts/Systems`, and
each ship's numbers are in its stats file (Step 80). All eleven panels and seven frame pieces are
prefabs in `Assets/UI/Board/Panels`, placed in `02.1_Grid`. The colours are in one palette file. The
builder and the import menu are deleted. The fonts and the "Handoff" rename are done (Steps 89 and
90). Left: the Test Runner (step 2), the last scripts out of `Scripts/Tactical` (step 5) and the
clock move (step 8).

Replaces the 2026-10-24 "clean-up once the game is working" list.

**How it is being done (owner, 2026-10-26).** Not everything at once. One ship system at a time, all
the way through, then a playtest and a commit. The scripts go into three folders:
- **`Scripts/Systems`:** the ship's parts. Each has rules only, nothing on screen. Our ship and the
  enemy carry the same ones.
- **`Scripts/UI`:** the panels, and the pieces they are made from. A panel shows its system and passes
  input to it.
- **`Scripts/Game`:** everything bigger than one ship or one panel: the game manager, the clock, the
  world, levels, the mission and enemy brains.

So far `Scripts/Game` has `BoardManager.cs`, `DebrisField.cs`, `GasCloud.cs`, `Mission.cs` and
`QuestSystem.cs`. The world and the enemy brain are still in `Scripts/Tactical`.

The board builder is deleted. The scene is edited directly.

What was done, part by part, is at the end of section 6 ("Board refactor, 2026-10-26 to 27").

**Why.** On 2026-10-25 the board was hard to follow:
- One 1,915-line class (`HandoffBoardState`, now `ShipState`) had every rule. Now the rules are in the eight ship
  parts, and that class is about 400 lines.
- The builder made the scene from code and deleted it on every run, so hand edits were lost. Now
  the builder is deleted, and the scene is edited directly.
- Drawer parts were named by export record (`sprite#178 rocker_30x38_locked`). Now each part is
  named after its control.
- The code used "Handoff" names. They are gone (Step 90).

On 2026-10-25, three read-only checks found that a UI artist could not work on the board without a
programmer. The guide for UI artists is now `docs/BOARD_UI.md`.

**Rules.** Do the steps in order, one step per commit. The playtest must pass before the next step.
Confirm each step with the owner before starting it. Managers are scene-wide, one of each. Systems
are ship parts, one set per ship.

1. **Commit the open work.** Done.
2. **Move the playtest and the intro letter check into Unity's Test Runner** (decision 31). Left.
   There are no Test Runner tests in `Assets` yet.
3. **Find what is used and what is not.** An agent traces every script, scene, prefab and asset,
   and writes a list. The owner picks what to delete. **Nothing is deleted without the owner's yes,
   Soham's old scripts included.** Done for the old `02_Grid` board: the scene and its scripts are
   deleted (commits bb530a3 and 87676e1). Its unused files, `PlotAnnotation.cs`, `MissionClock.cs` and
   `TacticalGrid.shader` / `.mat`, were deleted on 2026-10-29.
4. **Drop "Handoff" from the names.** Done in Step 90. Nothing else changed.
5. **Move files into folders:** `Scripts/Systems`, `Scripts/UI`, `Scripts/UI/Panels` and
   `Scripts/Game`. Move only. Part done: the ship parts and the eight panels are in place. The rename
   moved the "Handoff" ones (Step 90). Left in `Scripts/Tactical`: `TacticalWorld`, `WorldContact`,
   `EnemyBrain`, the spawns, `BattleSpace`, `ScreenFlow`, `GammaBoardPresenter`, `ShipView`,
   `TuneAttribute` and `CssEase`.
6. **Make the panels prefabs** (decision 31). Done (2026-10-26 and 27):
   - Each panel's sprites are an Art list in its own inspector. Fonts: a Fonts list beside it (Step
     89).
   - Shared colours are in one palette file that every panel points at.
   - Each object is named after its control, and each control's parts are under one parent.
   - The builder and the import menu are deleted.
   - The guide for UI artists is `docs/BOARD_UI.md`.
7. **Split `HandoffBoardState` (now `ShipState`) into ship parts,** one component each: power, sensors, comms, helm,
   weapons, hangar, damage and orders. Done: the eight systems (2026-10-26) and a stats file per
   ship (Step 80). Where the board already worked, its rules stayed.
8. **Move the clock, pause and tick out of `BoardManager` into `GameManager`.** First move
   `ReadOnlyAttribute` out of the old `GameManager.cs`, then delete that file. Left.
   `ReadOnlyAttribute` is still in `GameManager.cs`.

### UI restructure (owner, 2026-10-27)

All eight steps done (Steps 89 and 90, the docs, the check on 2026-10-29).

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
  number on the helm. Open: should it be a mechanic?

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

**W and S.** Built in Step 106, changed in Step 171. A tap moves the throttle 1. Held, it keeps
counting, slowly at first, then faster, and moves 100 in 2 seconds (Throttle Hold Seconds, Tuning:
Helm). Still capped by ENG.

Neither the design package nor the audit gives the autopilot any behaviour. Its controls only set
values, and approach mode only adds G.

### Ruins and the level, next (from Step 73, 2026-10-26)

Left unfinished when the Step 73 chat closed. Plan each with the owner before building. Done and
dropped from this list: ruins on the plot (Step 92), hitting a piece damages the ship (Step 93), the
Garuda hunts you (Step 97), the grid forming (Step 120).

1. **What ruins do, still to come.** Block shots (the prototype's penetration against toughness:
   asteroid 3, structure 4, ruins 3, starbase 5); hide us from sensors (line of sight).
2. **Smaller.** The wrecks are whole ships (break some apart later). Soham's `Asteroid.cs` could make rocks
   breakable later (add the "Asteroid" tag first). WRECK-4 and WRECK-11 use the same cruiser model
   as the Garuda. The ship models go to 100–500 m when the outside cameras come.
