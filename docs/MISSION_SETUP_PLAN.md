# Mission setup plan

Talked through with the owner on 2026-11-09 and 2026-11-10, in the Mission Setup Tool chat. **Not
built.** Lines marked "my pick" are my calls. Any of them can change.

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
  scanned).
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
| Checkpoint messages (16) | `Assets/Scenes/Levels/Orientation.unity`, the Mission object, Drill list, each step's Note box. Each starts "COMMAND ·". |
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
  INDRA, VARUNA and MARUT. Codex is still working on the lines.

## Keep true when rewriting mission 1

From the owner's story (`campaign-story-arc`, passed on by the Tutorial chat):

- A combat briefing with your battle group nearby. No hypersleep. The commander's voice: "welcome,
  commander… quick mop-up operation… orientation checklist".
- You learn to fly, scan and manage power. Each part finished makes the map bigger.
- The group flies its own path at 15. You follow them; they never follow you (step 2, THROTTLE).
- The group stops south of the rock field (step 12, TAKE POINT).
- Steps 8 and 12 grow the map (16,000 and 24,000). Step 11 is the scan check.

## Steps

1. **Owner picks** our ship's call sign, the others' call signs (or OKs stand-ins), and mission 1's
   name.
2. **Owner tells** how mission 1 goes, in their own words.
3. **Build** (ask once more before making the window: it is a new script). Add the names and the
   people who talk to `Campaign.cs`. Add checkpoints with talk lines to each mission. Move mission
   1's 16 checkpoints in, words unchanged. Make the board show the lines one by one. Make the
   Starwolf → Mission window.
4. **Write** mission 1's talk for every checkpoint, and its briefing. The owner edits in the window.
5. **Swap the names in** for the battle group's ship names and GARUDA in code. Message the Ship
   Layout Manager chat before touching `ShipState.cs`.

Later, not now: lines that play when something happens (you hit a rock, you scan a ship), and a
reminder line if the player is stuck for a while.
