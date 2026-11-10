# Mission setup plan

Talked through with the owner on 2026-11-09 and 2026-11-10, in the Mission Setup Tool chat. The
window is **not built**. MILK RUN's flow is built: see below. Lines marked "my pick" are my calls.
Any of them can change.

## MILK RUN, built 2026-11-10

Mission 1, MILK RUN, now plays the owner's whole flow (Step 192 in `docs/BUILD_LOG.md`). The level
file is `Assets/Scenes/Levels/Orientation.unity`. Its 23 checkpoints are on the Mission object, on
the Drill list.

1. Power: ENG on STANDBY.
2. Fly into the ring at (−1,650, −2,950).
3. Come round into the ring at (300, −2,000).
4. Slide left for 3 s.
5. Slide right for 3 s.
6. Zoom out to x0.2.
7. Zoom in to x0.8.
8. Zoom out to x0.12. The map grows to 16,000.
9. DRADIS on.
10. ACTIVE SCAN on.
11. Scan one ship of the group.
12. ENG to 5. The map grows to 24,000.
13. Speed 45.
14. Fly into the ring at (300, 1,900), at the rocks.
15. Go through the rocks. Stop in the ring at (0, 5,900), at the station.
16. Scan the station, s1.
17. Turn DRADIS gain up to 4. Until then the pirate corvette, p1, is hidden in the wreckage.
18. Scan p1. It runs.
19. Condition 2. This checkpoint frees the weapons. COMMAND: "You pursue that immediately, Captain.
    We'll stay here to take care of survivors."
20. Lock p1.
21. Open one side of p1's armour with torpedoes. It stops dead.
22. The marines capture p1.
23. Wait 20 s with the last note. Then MISSION COMPLETE.

The railgun is off for the whole mission. Fire Control reads RAILGUN HOLD. p1 can't be destroyed.
Its crew can't fight off our marines (my call; the owner can say no).

**Who did what.** Codex: the name, the briefing, and the words of checkpoints 1 to 11. The Tutorial
Mission Manager chat: the station, the wrecks, the escape pods, p1's place, the group's path and our
new spawn. This chat: the new checks, Weapons Free, Railgun Off, Cannot Be Destroyed, the corvette's
run, checkpoints 12 to 23, and new places for rings 2 and 3.

**The tools, in order (owner, 2026-11-11: "I'm okay with starting like this. Then we'll evaluate,
and it builds as we go").** First, MILK RUN's play-through fixes and the player build. Then:
1. A Starwolf → Mission window. The campaign's missions on the left (name, briefing, map dot). The
   open mission's checkpoints on the right, as cards: lines (speaker and words), the order, done
   when, the ring. A Names page: our ship, the battle group, the enemy and the title word.
2. The Level window draws and places ships, stations and wrecks, the numbered checkpoint rings and
   the group's route.
3. Play from checkpoint N.
Talk before each one. The owner is still weighing whether a mission should be part of its level.
Today it already is: the checkpoints are on the Mission object in the level scene. The window
would show and edit them there, so either way still works.

**Who decides (owner, 2026-11-11).** The Mission Setup Tool chat has the final say on everything in
a mission, Codex's words included. Codex helps with the writing. The chat checks Codex's lines,
changes them when a mission needs it, and says what it changed.

**Still open.**
- Codex rewrites the words of checkpoints 12 to 23. The ones there now are stand-ins.
- Nobody has played it through. A script checked the parts. Not checked: a real lock, a real
  torpedo hit, a real boarding.
- The group's shuttles to the escape pods: not built.
- The call signs and our ship's name (see "Names").
- The checkpoints are still on the level. They move into the Campaign file with the window (see
  "Steps").

## Summary

The owner writes and changes every word in a mission without touching code. That means the mission's
name and briefing, what people say at each checkpoint, and the ships' names. Everything is edited in
one window, **Starwolf → Mission**, and kept in the Campaign file, `Assets/Campaign/Campaign.asset`
(Step 183). Mission 1 comes first.

Owner, 2026-11-09: "I need a mission setup tool… start setting up dialogues under checkpoints. Focus
just on orientation." And: "It's now level scripting."

## How it plays

At each checkpoint a chime plays. The lines appear one by one in the box at the bottom of the
board, each with who is speaking, about 2 seconds apart. The order (what to do) comes last and stays
up until it is done. Then the next checkpoint starts. (My pick. The gap between lines is a Tuning
number.)

## What the owner asked for (2026-11-09)

- **Checkpoint talk.** Several lines per checkpoint, each with who says it.
- **The briefing on the sector map.** The owner calls it the campaign description.
- **The mission's name.**
- **The title word STARWOLF.**
- **Ship names.** Our ship gets a name, and every ship's name can change.
- The owner tells me in their own words how mission 1 goes. I write the talk for every checkpoint
  and rewrite the briefing. Then the owner changes what they want in the window. Together we agree
  one standard for every mission.

## The standard (proposed; the owner said "that's what I'm talking about")

The **Campaign file** gains:

- The title word (STARWOLF).
- The call signs: our ship, the carrier, the battleship, the corvette, the enemy.
- The people who talk: COMMAND, the XO, and so on.

Each **mission** in the Campaign file has its name, level, briefing and dot (built, Step 183), and
its **checkpoints**, in order. Each checkpoint has:

- **Talk:** lines, each with who says it and the words.
- **Order:** what the player must do, in plain words.
- **Done when:** what finishes it (today's checks: ENG on STANDBY, ENG at, inside the ring, speed
  at, slide left or right, zoom out or in to, DRADIS on, stop in the ring, ACTIVE SCAN on, ships
  scanned; since Step 192 also DRADIS gain at, condition at, locked, near a ship, armour breached,
  captured, and a wait in game seconds).
- **Ring:** where it is and its size, or none.
- **Map grows to:** a size, or no change.

**Names are typed once**, in the Campaign file. In any briefing or line you type [CARRIER] or
[OUR SHIP], and the game shows the call sign. Rename a ship once and every briefing and line changes
with it.

**The level keeps the map only:** rocks, spawn points, the battle group and its path. Today the
checkpoints are in the level, on the Mission object's Drill list. They move into the Campaign file,
with their words unchanged.

One window, **Starwolf → Mission**: pick a mission from a list, see its checkpoints top to bottom,
and type into them. A second tab shows the names. The Campaign Manager chat built the Campaign file
and added no menu item. The window is this plan's job. Extend `Campaign.cs`. Never make a second
mission list.

## Where the words are today (2026-11-10)

| Words | Where |
|---|---|
| Checkpoint messages (23) | `Assets/Scenes/Levels/Orientation.unity`, the Mission object, Drill list, each step's Note box. Each line starts with who speaks, like "COMMAND ·". |
| Mission name and briefing | `Assets/Campaign/Campaign.asset` → Missions → Element 0 → Title and Brief |
| STARWOLF | The Title Text box on the intro in `00_Start`, `01_Briefing` and `SectorMap`. Every letter, 0 to 9, the colon and the dash can be drawn (Step 181). |
| INDRA, VARUNA, MARUT | The names of the three ships in `Assets/Prefabs/BattleGroup`, written like "Indra · Carrier" |
| GARUDA | In code: the track list in `Assets/Scripts/Systems/ShipState.cs` (about line 58) and the radio report in `Assets/Scripts/Tactical/EnemyBrain.cs` (about line 184) |
| Our ship | No name on screen |
| The crew's talk between missions | `Assets/Scenes/Tactical/Ship.unity`, the rooms list |

## Names (owner 2026-11-10)

- Ship names are **call signs, in English, with an Indic feel**, like Red October. **Not Indian
  names.** The owner rejected RAHU, VRITRA, VRIKA, RED KARTIK, AMAVASYA, PRALAYA and MAHABALI.
- **INDRA, VARUNA and MARUT must go.** STARWOLF is the game's name only, not the ship's.
- Offered, no answer yet. Our ship: RED MONSOON (my pick), BLACK MONSOON, THIRD EYE, SAFFRON NIGHT,
  SILENT TIGER, IRON LOTUS. The others: carrier BANYAN, battleship TUSKER, corvette MONGOOSE, enemy
  COBRA. Owner: "got to play with this".
- The class names Asura and Garuda were not raised. They stay as they are.
- **Mission 1 must not be called ORIENTATION** (owner, told to the Tutorial chat, 2026-11-10). The
  level file stays `Orientation.unity`.
- **Codex's changes, found on 2026-11-10 (owner: "they've updated the text"; not committed):** mission
  1 is now **MILK RUN**. Its new briefing: "We'll take on supplies at the station before we start our
  patrol. Merchant crews have reported pirate activity along the border. Once we've loaded stores,
  we'll head out together and check those reports." Checkpoints 1 and 2 have new talk (CAPTAIN,
  COMMAND, then a POWER or HELM order). Checkpoints 3 to 16 are unchanged; checkpoint 12 still names
  INDRA, VARUNA and MARUT. Codex is still working on the lines. Later that day Codex's words covered
  checkpoints 1 to 11, and no checkpoint names INDRA, VARUNA or MARUT now.

## Keep true when rewriting mission 1

From the owner's story (`campaign-story-arc`, passed on by the Tutorial chat):

- A combat briefing with your battle group nearby. No hypersleep. The commander's voice: "welcome,
  commander… quick mop-up operation… orientation checklist".
- You learn to fly, scan and manage power. Each part finished makes the map bigger.
- The group flies its own path at 15. You follow them; they never follow you (step 2, THROTTLE).
- The group goes round the rocks to the station. We take the short way through (checkpoints 14 and
  15). Before Step 192 the group stopped south of the rock field (step 12, TAKE POINT).
- Steps 8 and 12 grow the map (16,000 and 24,000). Step 11 is the scan check.

## Steps

1. **Owner picks** our ship's call sign, the others' call signs (or OKs stand-ins), and mission 1's
   name.
2. **Owner tells** how mission 1 goes, in their own words. Done 2026-11-10. Built in Step 192.
3. **Build** (ask once more before making the window: it is a new script). Add the names and the
   people who talk to `Campaign.cs`. Add checkpoints with talk lines to each mission. Move mission
   1's 23 checkpoints in, words unchanged. Make the board show the lines one by one. Make the
   Starwolf → Mission window.
4. **Write** mission 1's talk for every checkpoint, and its briefing. The owner edits in the window.
5. **Swap the names in** for the battle group's ship names and GARUDA in code. Message the Ship
   Layout Manager chat before touching `ShipState.cs`.

Later, not now: lines that play when something happens (you hit a rock, you scan a ship), and a
reminder line if the player is stuck for a while.
