# Fight calculator

**Summary.** A script that plays whole Starwolf fights as maths, many times over, on the game's own rules
and numbers. It reads the ship files, the level scenes and the board scene itself. It plays every level
with a fight or a chase, three kinds of player, the first moves on contact (fighters only if our ship's file gives it any), at x1 and x5. It reports time
to first contact, to the kill, to be killed and to finish, dead time, shots and hits each way, damage
taken and the dodge chance. Player 1 runs it (`docs/team-reports/player-1/`). It decided the 5× speed
scale in Step 63. Re-run it after any change to speeds, ranges, weapons, damage or the enemy.

**Run** (plain Python, no packages):

| Command | What it does | Time |
|---|---|---|
| `python3 docs/scale-sim/scalesim.py` | every case, two seed sets of 12 fights, on every core; 48 more fights for a case whose two sets differ by over 25 points | 10 to 20 minutes, by how busy the Mac is |
| `... --fights 4` | the same, fewer fights | about 3 minutes |
| `... --csv FILE` | also appends one row per case (the `numbers.csv` columns) | |
| `... --check` | prints every number it read, and the rules still copied by hand | instant |
| `... --hand` | one fight step by step, to check by hand | instant |
| `... --tweaks` | Wolf Hunt with changed numbers | about 5 minutes |
| `... --extras` | torpedoes first; backing onto the Garuda's port side | about 2 minutes |
| `... --longshot` | long shots with the spinal railgun: flight time, hit chance by range, drift, stopping and turning | 1 second |
| `... --torpedo` | torpedo flight (Step 184): hit chance and time by range against a still, cruising, running target and MILK RUN's corvette; the Garuda's salvo against our PDCs at x1 and x5, before and after the stream (Step 197) | about 5 minutes |
| `... --jobs N` | use N cores (default: all) | |
| `... --label WORDS` | add words to the settings column of the `--csv` rows | |

All times are game seconds; at x1 a game second is a real second. The table's last columns: found% (the
Garuda went ALERT), died%, and dead2 (the longest dead time after first contact). A `*` marks a case whose two
seed sets differ by more than 25 points; since 2026-11-11 it marks a `+` and plays 48 more fights for it by itself.
New columns (2026-11-11): port (our hits on the Garuda's PORT face; it was called rct), blind (our slug hits nobody saw, so no report), torpIn and torpHit (enemy torpedoes fired at us and hits),
stream (hits by an enemy torpedo a PDC stream was already walking onto), pdc (torpedoes our PDCs shot down), tOut and tHit% (our
torpedoes and the share that hit), push (units our ship was pushed back by its own railgun).

## What it reads from the game

- **Ships:** `Assets/Ships/Asura.asset` (us), `Garuda.asset`, `Frigate.asset` (the reinforcements). Every
  number: movement, turning, ENG steps, sensors, signal per power unit, weapons, mounts, ammunition, hard
  points, the lock, the gun's turn, slug and torpedo flight, damage, armour, crew, conditions, the hangar (and how many
  fighters it carries: Hangar → Fighters; our ship has none since Step 170, so the fighter first move is dropped). A railgun
  with Half Arc 0 is played as a fixed gun (our spinal railgun); the slow turn's speed is Fine Turn Deg Per S.
- **The code's defaults** (since 2026-11-11): a number a ship file leaves out takes the default written in the code, as the game
  does until the file is saved. The calculator reads them from the Stats part of `WeaponSystem.cs`, `HangarSystem.cs` and
  `DamageSystem.cs`. Today the Garuda and the frigate take the push back, the PDC stream, the catapult, the hard break and the
  room rows this way (`--check` lists them).
- **The board scene** `Assets/Scenes/Tactical/02.1_Grid.unity`: the Garuda's and the frigate's brain
  settings (`EnemyBrain`, as set in the scene, not the code's defaults: cruise 22, aim error 3°, reload
  12–17 s and so on), the mission (`QuestSystem`: jump point, reinforcements), the rock interference,
  the board tick (0.42 real s), the scope fit, the shot costs (railgun 70, torpedo 40) and the launch
  stages.
- **Levels** `Assets/Scenes/Levels/*.unity`: map size, the goal, weapons off, our start, the enemy's
  start (or its random box), whether it hunts, its cruise speed, its route and radio, every rock (from
  the terrain prefabs placed in the scene, size = its scale) and gas cloud, and the jump point.

## What it copies from the game's code

Each tick, in the board's order (`BoardManager`, then `TacticalWorld.OnTick`), with the board's tick
length at each rate (0.42 game s at x1, 2.1 at x5):

- **Movement** (`NavSystem`): steer in whole degrees, thrust, braking, drift, drag, reverse, and the ENG
  bus setting top speed, acceleration and turn rate. Step 170: the part of a degree (`fine`), and the lock's turn onto the
  spinal railgun's lead (`aimHeading`): whole degrees at the usual rate, then the last part at the slow turn's 2 a second.
  A whole-degree turn drops the part of a degree.
- **Power and signal** (`PowerSystem`, `SensorSystem`): buses fill one unit a second up to the reactor's
  10; signal is 10 a unit, plus 35 while ACTIVE SCAN or a lock is on, times 0.3 in a gas cloud; it rises
  9 and falls 6 a second; each shot adds 25. The weapons charge (5 per WEP unit a second).
- **Our sensors:** DRADIS hears the Garuda inside 4,375 + 625 × gain, less rock interference. A scan
  reads its course and speed. The lock (`WeaponSystem.TickLock`): fill 10 s, hold 10 s, then drop and
  start again; the lock's lead uses the newest scan or plot, however old. A lock starts only on a ship DRADIS hears that
  fills the sight's brackets (`ShipState.ScopeFits`): at MAG 8 that is 8 × 1,000 / 0.85 = 9,412 units at most.
- **Push back** (`NavSystem.Kick`, `Recoil`, Step 197): each railgun shot, ours and the enemy's, moves the ship back along the
  shot, 40 a second for 0.4 s, then slowing to a stop over 0.4 s: 24 units, worked out exactly at any tick length. It moves the ship
  only; its speed is untouched.
- **Our guns:** a turret's bar turns 3° a second inside its cone. The spinal railgun (`WeaponSystem.FixedGun`) fires
  along the nose, to the part of a degree, with no target, lock or solution; ARM needs only the charge. While a lock
  holds, the ship turns itself onto the lead. The lead allows for our own motion (`LeadBearing`). Every slug keeps its
  ship's motion (`WeaponSystem.Slug`). The box hit test (`ShipState.EntersBox`) with the shooter's 25% margin; which
  face it enters. A held lock running out clears the switches (`DropLock`).
- **The far shot** (`WeaponSystem.Watched`, Step 197): a slug hit on a ship nobody sees (in sight from its near side, heard on
  DRADIS, or locked) still does its damage, but no HIT is reported; the slug flies on to its reach and reports IMPACT ?, as a miss.
  NO PEEKING is won only by a hit that is reported (`QuestSystem`: `lastShotHit`). Torpedoes always report.
- **Plot shots:** POINT, POINT, RULER, TRACK (`WeaponSystem.ComputeSol`): a straight-line reckoning from
  the newest POINT. TRACK clears the switches.
- **Torpedoes** (`WeaponSystem.NewTorpedo`, `Drift`, `SteerAt`, Step 184): out of the side, tilted back; drift with
  the motor off, slowing fast at first, while only the nose swings onto the target; the motor lights when the drift
  ends; then it flies where its nose points and turns onto the target, in 0.05 s steps. A salvo ripples, one torpedo
  every Torpedo Ripple Seconds. Ours use the two launchers by turns, starting with the target's side. A torpedo
  waiting its turn waits at its launch spot (the game lets it out from where its launcher is by then). Every torpedo of an
  enemy salvo leaves (the game launched only the first from Step 184 until Step 197; the calculator never had that fault).
  Our four PDC groups (`PdcStops`), one 70% chance each, arc and reach checked. Step 197: on a kill the stream walks onto the
  torpedo and it goes down Pdc Stream Seconds (0.5) later, counted once a board tick, so 0.84 game s at x1 and 2.1 at x5; it flies
  on meanwhile and can still hit us.
- **Damage** (`DamageSystem`): armour per face, then the next room (`NextRoom`, Step 186, the box): each room's row
  from bow to stern comes from the ship file (Hard Points → ROOMS; files with none saved get the code's layout). PROW: the
  front room still standing; AFT: the back one; STBD or PORT: the room at the spot the shot landed (the hit test gives it),
  else the nearest still standing; rooms on the same row take turns. Fighters hit the middle of a side. HE fire 60%, slug breach 50%; fire burns a room
  1 every 30 s; fire and vacuum spread 1% a second to each neighbour; a party takes 6 s; reactor at 0
  destroys the ship. Magazine below 3: no firing. Engine room: less speed and turn.
- **The enemy** (`EnemyBrain.Think`, as the game has it today): hunt points or route (turning back 25%),
  radio messages and course changes, hearing by its DRADIS reach × our signal, eyes at 2,000 (400 if we
  are in a cloud, none past a rock), UNEASY, SEARCHING, ALERT, giving up after 90 s, closing to 1,600 then
  turning the nearer side to us at 14 units a second, railgun every 12–17 s with a ±3° error on our true
  position and speed, led on our motion less its own, its slug keeping its own motion (Step 170), torpedoes when hit and every 90 s with a fix, any loss of hull counted as a hit,
  magazine repair, rock and ship avoidance, the edge band. Reinforcement frigates 180 s after its alarm.
- **Fighters and boats** (`HangarSystem`): six launch stages (34.4 game s), fighters at 100 a second, a
  run every 30 s by each craft on the face toward them; boats, boarding odds. Step 197: the catapult throws craft out at 560 a
  second, falling to their own speed over 1.2 s (about 290 units gained). They fly straight at the target; the straight run out
  of the side and the hard break are left out.

## The person

Each player type has hands times in real seconds; at x5 each takes five times the game seconds. The
team README sets arming (8, 4, 2 s). Everything else is a guess, listed in the player's report with
reasons. A player can do one thing at a time. With the spinal railgun the person stops at a stand-off range, a share of our
DRADIS reach at SEN 3 (new 0.48, good 0.72, expert 0.88: 3,000, 4,500 and 5,500 today, guess). Past the lock's reach
they plot with POINT and RULER and aim by hand. Careful players arm before the lock (so the lock's end can't clear a half-set
ladder), lets the lock turn the nose, and fires once the nose is on and the ship isn't sliding (Q and E stop a slide).
Quick hands (arming in 5 game s or less) also fire once on the aim the lock left. NO PEEKING allows no lock: the nose is
turned by hand with Shift, which moves once a board tick (0.84° steps at x1, 4.2° at x5), to within the person's click
error. When the slugs run out the person fires torpedoes (from 2,600; they launch at 2,900 or closer). Scan and plot first
moves head for a point off the Garuda's nearer side at the stand-off range (since 2026-11-11; before, always its port side,
which since Step 186 gains nothing: a hit at the middle of either side reaches the reactor). The six first moves on contact are scripted: fighter,
scan, fire, plot, close, quiet. Chases (COLD RUN, INTERCEPT) follow a path round the rocks that prefers
gas clouds.

## What it leaves out

- Collisions and bounces (both ships steer round rocks and each other, as the brain does).
- The drawn launch lines; fighters fly straight to the target. Since Step 184 no ship file has a torpedo line.
- The DRADIS sweep: a contact is where it is, not where the sweep last touched it.
- Crew loss from radiation, AUX charge for damage-control orders, comms decoding as a puzzle (a fixed
  hands time instead), the periscope's MAG knob (a fixed sight time).
- Gun hard-point offsets for the lead (the slug starts at the hard point, as in the game).
- The game's one dice sequence (seeded 49 on every ship); the calculator rolls fresh dice each fight.
- The Garuda's PDCs: the game never fires them, so neither does the calculator.
- Step 171's tap-and-hold keys: Q and E push at full strength at once (no 2 s build-up, no taps); the throttle is set,
  not counted up. After a shot only ARM needs setting again; the calculator charges the whole arming time.
- POINT is placed on the Garuda even when rocks hide its dot (the person script never asks whether the dot shows).
- An incoming torpedo's hit test allows for our ship's motion; the game's (`ReachesUs`) uses our spot only. A few units.
- MILK RUN (mission 1): its corvette chase is a checklist with a ship that can't be destroyed. Only its torpedo shot is
  played (`--torpedo`, the corvette row).

## History

- **Step 63:** at the old speeds a fight took 35 minutes at x1. At 5× about 7.
- **Step 130 (the designer's table):** about 2.5 minutes from contact to kill, with a much simpler model.
- **2026-11-08 (Player 1's first run):** rebuilt. Reads the files itself. The old version gave the Garuda
  our speed, 8 torpedoes, weaker armour and hits, and gave us one weak PDC (QA, 2026-11-08). Results in
  `docs/team-reports/player-1/2026-11-08.md`.
- **2026-11-09 (Steps 170 and 171):** the spinal railgun, the slow turn, the lock turning the ship, slugs that keep their
  ship's motion (both sides), no fighters on our ship, LONG SHOT (40,000, run up to 3,600 s). `--sniper` became
  `--longshot`. Results in `docs/team-reports/player-1/2026-11-09.md`.
- **2026-11-10 (Step 180):** room sides and order from the ship files; hits go on through wrecked rooms. Fighters on
  DISABLE go for the engine room's side. A player who reads the hit reports goes round only when nothing is left to hit
  on that side. Old rule against new rule, same files and dice: the "Room hits" tab of the Starwolf Tuning and Roadmap sheet.
- **2026-11-10 (Step 186, the box):** rooms in a row from bow to stern; each shot hits the room at the spot it landed.
  The Step 180 rule against the box, same files, dice and torpedo flight: the same sheet tab.
- **2026-11-10 (Step 184):** the torpedo flight: drop, drift and flare, and the ripple salvo.
- **2026-11-11 (Step 197):** the push back, the far shot, the PDC stream, the catapult; numbers a ship file leaves out come from
  the code; the person goes to the Garuda's nearer side, not always its port side; every core; `--torpedo`. Results in
  `docs/team-reports/player-1/2026-11-11.md`.
