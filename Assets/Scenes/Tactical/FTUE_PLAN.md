# The first mission (FTUE) and the first enemy — plan

**Summary.** NEW GAME drops you straight into one mission, which is also the tutorial. The ship is
already under way. Your XO talks to you through a small comms box on the plot and walks you through
every station on the board, one at a time, in a fixed order. Nothing you have not been shown yet
will work. You practise on derelicts first. Then one enemy, the Garuda-class cruiser, far away, that patrols, hears
you, comes to look, sweeps, and fights. You kill it, the XO says "let's get out of here", and the
mission ends. No reinforcements.

Planned with the owner on 2026-10-18. This is a pickup document for whoever builds it. The build
order is in §9. Build one step, show it, wait. Do not run ahead (BUILD_LOG decision 17).

**What is built (checked 2026-10-29).** The enemy half of this plan is built, as the mission Wolf
Hunt (Steps 95 to 99). The Garuda hunts us, hears us, searches, and fights back with railguns and
missiles. The tutorial half is not built: no XO, no comms box, no gating, no walkthrough, no
derelicts.

**Changed by the owner since (2026-10-27).** Two rules in this plan are replaced:

- **Reinforcements are in.** When an enemy ship reaches ALERT, a 3:00 countdown starts, then Naga
  frigates arrive.
- **The mission ends at the jump point.** Killing or capturing the Garuda opens a jump point.
  Reaching it wins.

How both work is in `Assets/TACTICAL_BOARD_SPEC.md` §5.3, §5.4 and §15.2. The rest of this plan
stands until the owner says otherwise.

Detailed status is not kept here. `BUILD_LOG.md` is the single living record; log each step there.

---

## 1 · What the owner decided (2026-10-18)

| Question | Decision |
|---|---|
| Who guides the player | **One XO.** Text only, in a comms-style box. "If you are Picard, this is Riker." No voice. |
| Which stations the walkthrough covers | **All of them.** Power, helm, plot tools, sensors, comms, engineering, hangar, orders, periscope, fire control, time control. Engineering, comms and hangar must really exist, not be skipped. |
| What the enemy is | **Our ship with different stats.** Same power, signature, sensors, helm, weapons and damage. A brain drives it instead of the board. One ship model, two drivers. §4. |
| What a scan shows | **The enemy's ship state, drawn as ours is.** Class, hull sides, compartments and integrity, power and signal. Our hits update it. Side-to-room map: `DAMAGE_README.md` (owner 2026-10-24). |
| Which weapons | **The handoff's.** Two railgun turrets, one on either side of the ship, two PDC groups, the VLS, with the design's arcs, ranges and arming rules. The GDD's weapon list is dated. |
| Where the FTUE ends | **At the kill.** The XO's last line is "let's get out of here." The escape to a jump point is a later addition. **Replaced (owner, 2026-10-27):** the jump point is built; reaching it wins. |
| Does the enemy shoot back | **Yes** (§9, step J). **Built, Step 97:** railgun slugs and HE missile salvos once it is ALERT. **No reinforcements**, ever, in this mission. **Replaced (owner, 2026-10-27):** reinforcements are in. |
| Strict or loose | **Strict.** You go through every station before you fight. A control you have not been shown is refused. |
| What you fight first | **Derelicts** before the live enemy. |
| Distance and length | Not decided. Ten minutes of travel is acceptable. Tune later. |
| One sector or a flag | **One flow.** NEW GAME → load → this mission. There is no separate tutorial. |
| Where the comms box sits | Owner does not mind. This plan picks **top-left of the plot, under the alarm strip** (§6). |

Open questions for the owner are in §10.

---

## 2 · Where this fits

The owner's wiring order (BUILD_LOG §8) is: power, plot tools, press and click interactions, helm,
sensors, periscope and fire control, fire, hit, then **5. enemy AI and the alert net**, **6. railgun
in the world and damage**, **7. mission, win and lose**. Everything up to the hit is done (Steps
40–48). Railgun hits and damage are done (Steps 48, 49 and 57–59).

This plan is items 5 and 7, with the alert net removed. It needs:

- **The helm wired** (our ship really moves) before the enemy can be approached. Done. Enemy patrol
  (step A) is done too (Step 51).
- **Sensors wired** before the "it hears you" and "it sweeps" steps mean anything on screen. Done.
- **Fire, hit and damage wired** before the derelict fight and the kill. Done.
- Every other station wired before its walkthrough step can be built. All the stations in §8 are
  wired now.

"Power P3: the enemy notices you (UNEASY, SEARCHING, ALERT)", parked in BUILD_LOG, is step B here.

---

## 3 · The world of the first mission

Everything goes under the `World` root in `02.1_Grid`, as `WorldContact`s, the way the Garuda
(`Garuda · Cruiser`) is today. Where each one starts is set by a spawn point in the level scene
(`Assets/Scenes/Levels/Level_01.unity`, for example `Enemy Spawn · Garuda`). Units are the board's
km (`TacticalWorld`: 1 unit = 1 km). The board's own numbers already use this scale: DRADIS range
12,000 + 3,600 km per gain step, railguns 25,000 km, missiles 35,000 km, PDCs 10,000 km. So the
spec's distances carry over as they are.

| Thing | Count | Where | What it does |
|---|---|---|---|
| Our ship | 1 | One edge of the sector | Already under way at mission start: a throttle and a heading set before the first tick. **The clock starts running** (see §10, Q1). |
| Derelicts | 2 | Near the route, 8,000 to 15,000 from the start | Dead hulks. Do not move. Can be heard, scanned, targeted, hit and destroyed. Scanning says DERELICT. |
| The Garuda | 1 | Far away, the other side of the sector | The enemy, a Garuda-class cruiser. §4. |

Nothing else. No nebula, no asteroids, no second enemy. Add when the real mission needs them.

---

## 4 · The enemy

**Owner, 2026-10-18: the enemy is basically our ship with different stats. It works on the same
principles.** Same power, same signature, same sensors, same helm, same weapons, same damage. The
only difference is who drives it: the player drives ours through the board, a small brain drives
theirs. There is **one ship model** in this game, and the enemy is a second copy of it.

This is the rule for everything below. Whenever a question comes up about how the enemy does
something, the answer is: the way we do it.

### 4.1 One ship, two drivers

A ship is an object with eight system components: `NavSystem`, `PowerSystem`, `SensorSystem`,
`CommsSystem`, `WeaponSystem`, `HangarSystem`, `DamageSystem` and `OrdersSystem`
(`Assets/Scripts/Systems/`). It also has a `Ship` component, which points at its `ShipStats` file in
`Assets/Ships`. Each system reads its numbers from its section of that file. Our ship and the Garuda
are built the same way.

- **The Garuda has its own stats file**, `Assets/Ships/Garuda.asset`. Since Step 96 its numbers
  differ from ours (`Asura.asset`): it is bigger, slower, better armoured, hits harder and reaches
  less far. The Naga frigate is `Frigate.asset`. A new class, such as DERELICT, is a new stats file.
- **`WorldContact` builds a `ShipState` for the enemy** from its eight system components.
  That state has the ship's contacts (`tracks`). The systems are reached through
  it: `nav`, `power`, `sensors`, `comms`, `weapons`, `hangar`, `damage`, `orders`.
- **The same code runs it.** Each system has its own tick: `PowerSystem.Tick`, `NavSystem.Tick`,
  `SensorSystem.Tick`, `WeaponSystem.TickCharge` and the rest. Today `TacticalWorld` runs the
  Garuda's `DamageSystem.TickConditions`, then its brain, then its `NavSystem.Tick`, on the board
  tick, after ours. Its power, sensors and weapons ticks are not run yet. Nothing moves while
  paused.
- **The brain only writes orders.** `nav.orderedHeading`, `nav.throttle`, bus modes,
  `sensors.dradis`, `sensors.active` and the cone bearing, the target, the arming switches, fire.
  It touches nothing else. If the brain wants something the player cannot do, the brain is wrong.
- **Its track list is what it knows about us.** `SensorSystem.Heard` and `ConeCovers`, run from
  its side, decide whether it hears us. A track is what you know, not what is true. **Built
  differently (Step 97):** `EnemyBrain.Listen` hears us out to its DRADIS reach times our signal,
  and remembers where it last heard us. It does not use a track list or the cone.

The board-only parts of the state (the periscope and the plot marks) are unused on the Garuda's
copy. That is fine.

### 4.2 Awareness

> **Built in Step 97, differently from the table below.** COLD hunts the map instead of patrolling
> a loop. The brain does not change its bus modes or go active; it raises its DRADIS gain as it
> grows more aware. Inside its visual range it sees us, and a hit puts it on ALERT. What is built is
> in `Assets/TACTICAL_BOARD_SPEC.md` §5.3. The table is the plan as written.

The spec's ladder stays (§5.3). What changes is **how** each rung is reached: not a fixed distance,
but **its own sensors hearing our signal**, with the same rules ours use.

| Awareness | Name | Raised when | What the brain orders |
|---|---|---|---|
| 0 | COLD | start | **Patrol.** Low power: one unit on ENG, one on SEN, DRADIS on, passive. Loops a short list of waypoints at a cruising throttle. Quiet, because it is not expecting anyone. |
| 1 | UNEASY | its passive sensors hear us faintly (our signal is above a low threshold inside its passive range) | Keeps the patrol. Remembers the bearing and range it heard us at. Nothing visible changes. |
| 2 | SEARCHING | it hears us clearly (a higher threshold), or hears us twice | **Investigate.** ENG bus to DRAW for speed. Orders a heading for the last place it heard us. Loiters there. |
| 3 | ALERT | it hears us loud and close (the top threshold), or we fire | **Sweep, then engage.** SEN bus up, cone onto our last bearing, **active on**: a dedicated sweep. It now has a firm track. WEP bus to DRAW. Then it closes and fights (§4.4). |

> Awareness never goes down (spec Rule 5.3.a). You can re-hide. It does not forget.

The three thresholds are on the board's own 0 to 100 `signal` scale, not the prototype's units.
Start from the spec's 2.0 / 3.0 / 4.5 mapped onto that scale, then tune. All three are
`[Tune("Enemy")]`. Its passive range is whatever its SEN power buys it, same formula as ours.

**Why this is better than fixed distances.** Going dark now works for a real reason: a ship with
nothing powered is below its hearing threshold. Its active sweep is loud for the same reason ours
is, so the player hears it wake up. And a frigate with weak sensors is genuinely easier to sneak up
on than a destroyer with good ones. Stats, not scripts.

### 4.3 What the player sees

Only through instruments, like everything else.

- **Patrol:** a quiet contact that moves between sweeps. Low signal. Reads as a patrol if scanned.
- **Investigate:** its signal rises (engines drawing), it turns toward where we were, it speeds up.
- **Sweep:** its signal jumps (sensors up, active on). Once the sensors step builds the sweep ring,
  a ring on its contact. This is the moment the disguise is gone, and the player can hear it.
- **Engage:** weapons power shows in its signal. It turns to put a mount on us. Then it fires.

### 4.4 How the enemy fights (step J)

**It uses the same weapons we do, under the same rules.** And the weapons are **the handoff's**
(owner, 2026-10-18: the design's weapon systems, with their behaviours and rules; the GDD's weapon
list is dated). That means the handoff's mounts, arcs, ranges, the arming ladder LOAD, ANGLE,
BEARING, ARM, the solution that needs a known track, the charge, the fire lever. Not the GDD's
cannons and boarding pods, and not the prototype's broadside guns.

The handoff's fit is ours and the Garuda's. The numbers are in `WeaponSystem.Stats.mounts`, in
each ship's stats file (`Assets/Ships/Asura.asset`, `Garuda.asset`):

| Mount | What | Where it points | Covers | Reach (km) |
|---|---|---|---|---|
| RG-A | Railgun turret A | **starboard side**, arc centre 42° | ±80° | 25,000 |
| RG-B | Railgun turret B | **port side**, arc centre 250° | ±80° | 25,000 |
| PDC-A, PDC-B | Point defence groups | 090° and 270°: the starboard half and the port half (owner, 2026-10-29) | ±90° | 10,000 |
| VLS | Missile cells | all round | ±180° | 35,000 |

**Two turrets, one on either side of the ship** (owner, 2026-10-18). Each sees most of its own
side and nothing of the other. So a ship fights **side-on**, and which side it shows is both its
armour and its gun. The Garuda has the same layout. Its numbers differ (Step 96): see
`Assets/Ships/Garuda.asset`. The table gives ours.

Rounds, flight, hits and damage are whatever the fire, hit and damage steps build for us, used from
the other end. A railgun slug flies at 2,500 km/s, about 10 s to its 25,000 km reach (owner,
2026-10-27). The Garuda uses the same code.

The brain's tactic, from the prototype, fits this layout exactly:

- **Close** to inside turret range, then **circle to keep one turret's arc on us**: target heading
  is the bearing to us offset by that turret's arc centre, so its side faces us.
- **Arm** when the arc bears and the ladder allows. **Fire** when armed. Re-arm and repeat. The
  ladder's own timings are its reload.
- **Back off** if the range closes too far for its arcs. (That is not retreat. It still fights.)

What the prototype adds that we keep as tunables: it fires only when the arc bears within a cone
(22°), a short retry (0.4 s) when it does not, and a random pause between shots (2.5 to 4.5 s) so
it is not metronomic. The "enemy hits a little softer" factor of 0.65 (spec §8.1) stays, as one
tunable on its damage.

> **Built differently (Step 97).** It fires a turret when we are inside that turret's arc, with a
> pause of 12 to 17 game seconds between shots and its aim off by up to 0.4°. It fires an HE
> salvo every 90 game seconds once ALERT. It hits 1.5 times as hard as we do. The numbers are in
> its stats file. It does not use the arming ladder. The settings are on `EnemyBrain`.

**Where its hits land on us.** The side facing it takes the hit. Once a side is open, each further
hit damages the room behind that side. That is what makes engineering real.

> **Replaced 2026-10-24 (owner).** The 18 Oct table (prow BRI; aft RCT, ENG; starboard MAG; port HAB,
> HGR, at random) gave way to the design audit's: **bow MAG, stern ENG, port RCT, starboard HGR**, one
> fixed room per face; bridge and habitation inboard. See `DAMAGE_README.md`, next to this file.
> The spec's seven internals still do not match the handoff's six compartments; damage lands on
> compartments.

**And our hits land on it the same way.** Its compartments take damage through the side we hit.
Its kill conditions are ours. Its reactor compartment at zero kills it, as it would us.


**Seeing it (owner, 2026-10-18).** When we scan a ship we get data on it, and **we must be able to
see that data the way we see our own ship**: its class, its hull sides, its compartments and their
integrity, its power and signal. Because the enemy is our ship, this is the same picture the
engineering drawer paints of us, drawn for the selected track. A scan fills it in, more detail with
a better scan (GDD scanner intent, spec §9.4). Our hits update it, so the player watches a side open
and picks the next shot. Unscanned, a track shows nothing but a bearing, a range and a class guess.
Since Step 61 the periscope is the targeting computer. Its SUBJECT box shows a scanned ship's name
and class, then each hull face and, through an open face, the room behind it. **Not on the plot**
(owner, Step 39).
**What the player sees.** No outside view. A hit on us is the alarm strip, a compartment's INTEG
dropping in the engineering drawer, and the side and room colours on engineering's ship picture
(Step 74). Its round in flight is a fast small contact on the plot, so a watchful player sees it
coming.

**Defence.** Three things, all already in the design: turn a fresh side to it, open the range, and
the PDCs. A PDC group on AUTO gets one chance at each incoming missile inside its arc and reach,
with a 70% kill chance (Step 55). That is the PDC's tutorial moment.

### 4.5 How it is built

- A `ShipStats` file per ship in `Assets/Ships` (Step 80): navigation, power, sensors, weapons,
  hangar, damage and orders. `Asura.asset` is ours and `Garuda.asset` is the enemy's. A DERELICT
  needs a file of its own.
- Each ship has the eight system components plus `Ship`, which points at its stats file.
  `WorldContact` builds the ship's `ShipState` from those components. An `EnemyBrain` on
  the same object drives it.
- `TacticalWorld` runs on `board.Ticked` and knows every contact. Each tick it runs each contact's
  damage conditions, then its brain, then its `NavSystem.Tick`, then syncs. Nothing runs in
  `Update`.
- `EnemyBrain`: one component, one file. It has the hunt, the hearing, the fighting settings and a
  `Think()` that writes orders into its own ship. Awareness and last-heard are built (Step 97). The
  movement maths is the same as ours (`NavSystem.Tick`).
- Hearing: run `SensorSystem.Heard` / `ConeCovers` with the Garuda as the listener and us as the
  track. Do not write a second hearing rule. **Not followed:** Step 97 wrote its own rule in
  `EnemyBrain.Listen` (§4.1).
- Weapons and damage: whatever the fire, hit and damage steps build for us, called with the Garuda
  as the shooter or the target. If it needs a parameter to say "which ship", add the parameter.
  Do not fork the code.
- Do not use the old `EnemyShip` / `Radar` / `PlayerShip` scripts (owner 2026-10-17).

### 4.6 Deliberately left out

- **Retreat** (the GDD lists it). The easy enemy does not run. Add for the real mission.
- **The alert net and reinforcements** (spec §5.4). Owner: none in this mission. **Replaced (owner,
  2026-10-27):** they are in, built in Step 99.
- **The disguise mechanic** (matching your output to a scanned class). For now the Garuda reads us
  as neutral because its awareness is 0. The shared model makes the real version cheap later: its
  scan of us returns our class, and the lie holds while our signal matches.
- **An enemy that uses engineering, comms, the hangar or the plot tools.** It has the systems. The
  brain does not touch them. Later brains can.
- **Forgetting.** Never.

---

## 5 · Derelicts

A `WorldContact` with no brain, `klass = DERELICT`, a hull value, and nothing else. It is there so
the player can take the whole sensors → fire control → fire → hit chain on something that does not
care. Two of them, so the second shot can be a timed one on the player's own. When one is destroyed
its track reads WRECK and stays on the plot.

Derelicts cannot damage us. Engineering therefore needs a drill (§10, Q2).

---

## 6 · The XO and the comms box

**The XO.** One person. Executive officer. Calm, short sentences, talks to you as "Captain". No name
yet (§10, Q3). Shown as a small placeholder portrait or a callsign tile. Placeholder art is fine.

**The box.** A small panel inside the plot, **top-left, below the alarm strip and right of the tool
rail**. The bottom of the plot is taken: the orders station bottom-left, the helm bottom-centre,
the console strip bottom-right. Top-left is free. The owner does not mind where it goes, so move it
if it fights something.

What it shows:

1. The speaker tile (icon or initials) and the label `XO`.
2. The line. Two or three short sentences at most.
3. One dim hint line underneath: what the game is waiting for, in the board's own words.
   Example: `WAITING — ENG BUS TO DRAW`.

How it behaves:

- A line appears and **stays until its step is done**. No timers, no "press to continue", except
  for pure talk lines, which clear on the next click anywhere.
- Done, the box swaps to the next line. A short typed-in reveal is nice and cheap. Nothing else.
- **No quest journal, no checklist, no progress bar.** Spec §15.2: stages run in the background with
  no popups.
- It uses the board's existing type and colours. Build it in the handoff's language (stencil ink
  for the label, data ink for the line, note ink for the hint). It is a uGUI panel under the plot
  canvas, like every other panel.

---

## 7 · Strict gating

"Strict" means: every control the XO has not introduced yet is refused.

How, in one place:

- The FTUE keeps **one allow-list of action names** (the `action` strings the board already
  dispatches: `pause`, `rate`, `tool`, `drawer`, `helm`, `power.bus`, and so on). Read them from
  the panels' `Click` cases and `UIStateSpec.md` §10.
- `BoardManager.Click`, `Down` and `Up` ask the FTUE first. Not on the list → the board
  refuses the same way it refuses a locked rocker today: the "refused" nudge the press-and-click
  step is building, and no state change. The XO box does not need to react. The hint line already
  says what is wanted.
- Each walkthrough step adds its controls to the list when it starts. Once the walkthrough is over,
  the list is ignored and everything works.
- **Opening a drawer is always allowed** once that drawer's station has been introduced, so the
  player can look around. Only the controls inside are gated.

Do not build a second lock system. Do not put the gate inside each panel. One check, before
dispatch.

---

## 8 · The walkthrough

Each row is one step: what the XO says (a draft, the owner will rewrite), what the player does, and
how the code knows it is done. Done checks read `ShipState` (`State`), our ship's systems
through it (`State.power`, `State.nav` and the rest), and the board (`board`), nothing else. Field
names are today's; check them when you build.

The order follows the owner's wiring order, so each step can be built as its station comes online.

| # | Station | XO says (draft) | Player does | Done when |
|---|---|---|---|---|
| 1 | — | "Captain. XO here. We're under way and we're alone out here. Let's get this ship together. First: the clock. Everything on this board waits when the clock is stopped." | Pauses, then runs again | `board.Paused` has been true, then false |
| 2 | Time | "Time keys next to it. Push the clock when nothing is happening. Pull it back before anything does." | Picks a faster rate, then back to 1 | `board.Rate` changed from 1 and returned |
| 3 | Power | "Raw power is everything. Open the power drawer. Put the engine bus on DRAW." | Opens POWER, ENG bus to DRAW | `power.BusUnits("eng") >= 1` |
| 4 | Power | "Watch the signal meter as that fills. Power in a system is power the enemy can hear. The reactor itself is silent." | Nothing. Talk line. | next click |
| 5 | Power | "Put sensors on DRAW too. Then take engines back to STANDBY when it's where you want it." | SEN to DRAW, ENG to STANDBY | `power.BusUnits("sen") >= 1` and `eng` bus mode Standby |
| 6 | Helm | "Helm. Open it. Order a new heading and bring the throttle up. She's slow to answer. Mind that." | Opens the helm, changes `nav.orderedHeading`, changes `nav.throttle` | `nav.orderedHeading != start` and `nav.throttle != start`, and `nav.heading` has moved |
| 7 | Plot | "Your plot tools, left rail. Drop a POINT on our own position. That's a timestamp. Drop another in a minute and you have our speed." | POINT twice | two `point` marks, at least 30 s apart |
| 8 | Plot | "RULER between them. COMPASS on us. Erase when you're done. Marks are notes, nothing more." | RULER, COMPASS, ERASE | a ruler mark and a compass mark have existed; an erase has happened |
| 9 | Sensors | "Sensors. DRADIS on. We only hear what has power in it, and only inside our range. Boost the gain." | Opens SENSORS, DRADIS on, gain up | `sensors.dradis` and `sensors.senBoost` raised |
| 10 | Sensors | "There. Something dead ahead, close. Point the cone at it and go active. Active is loud. Remember that." | Cone onto derelict 1, active scan | `sensors.ConeCovers(derelict1)` and `sensors.active` for one full tick cycle |
| 11 | Sensors | "A derelict. Dead hull. Look at the readout: its sides, its compartments, what is left of them. A scan shows you a ship the way you see your own. Now listen: every time you go active, anyone out there hears you. Switch active off." | Opens the target readout, active off | derelict 1 scanned and its compartments shown; `!sensors.active` |
| 12 | Comms | "Comms. There's chatter on the net. Find the channel, tune it, lock it, and work the dials. The key is our own callsign." | Tune, LOCK, decrypt | `comms.Solved` |
| 13 | Comms | "That's a hostile patrol order. One cruiser, their side of the sector. So we know who's out here, and roughly where." | Talk line. Also drops a rough marker on the plot, the way the spec's comms payoff does. | next click |
| 14 | Orders | "Orders station, bottom left. Set CONDITION 2. Weapons and flight ops are released from Condition 2 up. AUX pays for orders; watch the pool." | `orders.alert` to 1 | `orders.alert >= 1` |
| 15 | Engineering | "Engineering. I've staged a drill: the hangar bay reads INTEG 4. Select it and send a repair party. Six ticks and it lands." | Select HGR, PARTY | `damage.CompHealth("hgr") == 5` again |
| 16 | Engineering | "REBOOT, SEAL and VENT live here too. You'll need them when we take a hit. Not today." | Talk line | next click |
| 17 | Hangar | "Hangar. Spot the interceptors, DESTROY order, run the launch stages, open the doors, launch. Then RECALL. Flight ops cost AUX." | Launch the `gw` squad, recall | `hangar.launchedSquads` contained `gw`, now empty |
| 18 | Periscope | "The scope. Slew to the derelict's bearing and look at it. This is the only window you have." | Opens the scope, slews on bearing | `ScopeOn(derelict1, scopeDown)` |
| 19 | Fire control | "Fire control. Weapons bus to two units. Railgun, turret A, target the derelict. LOAD. ANGLE. BEARING. ARM. The ladder refuses out of order, and refuses a target it can't reach." | WEP ≥ 2, select, ladder | `weapons.WepReady` and `weapons.AllSet` on derelict 1 |
| 20 | Fire control | "Fire." | Fire lever | `weapons.fired` and a hit lands on derelict 1 |
| 21 | Fire control | "Hit. Hull side matters: you hit what faces you. Finish it." | Fires until destroyed | derelict 1 destroyed |
| 22 | — | "The second derelict is yours, Captain. Your choice of weapon. Take it when you like." | Any kill | derelict 2 destroyed |
| 23 | Enemy | "That cruiser is still out there. Set a course. Go dark on the way in: power down what you don't need. It's listening. When you are close enough, scan it before it knows you are there. Know where its reactor sits before you shoot." | Steers toward the Garuda, scans it | range to the Garuda < 20,000 and the Garuda scanned (its compartments shown) |
| 24 | Enemy | *(fires when the Garuda reaches SEARCHING)* "It heard something. It's turning toward where we were. Decide, Captain: go dark and wait, or go loud and fast." | Nothing | Garuda awareness ≥ 2 |
| 25 | Enemy | *(fires on ALERT)* "Active sweep. They have us. No more hiding. Weapons free." | Fights | Garuda awareness == 3 |
| 26 | Enemy | "Kill it." | Kills the Garuda | Garuda destroyed |
| 27 | — | "Good shooting. Let's get out of here." | — | **Mission ends.** Short end card, then the menu (§10, Q4). |

Steps 24 and 25 are **event lines**, not gates. They fire when the enemy changes state. If the
player is already fighting, they still fire. The gate is step 26.

The strict allow-list opens as the rows go: step 1 adds `pause`; step 2 adds `rate`; step 3 adds
the power drawer and bus switches; and so on. From step 22 on, everything is open.

**Branching.** None. If the player kills derelict 2 before step 22, step 22 is already done and
skips. Done checks are state checks, not event listeners, so order mistakes self-heal.

---

## 9 · Build order

Small steps, each shown to the owner before the next. Each has its own BUILD_LOG entry.

| Step | What | Needs first | Done when the owner can see |
|---|---|---|---|
| **A** | **Done, Step 51. Since Step 97 it hunts the map instead of looping a route.** The Garuda patrols. Its own `ShipState`, built by `WorldContact` from its system components and its stats file, ticked by `TacticalWorld` after ours; `EnemyBrain` orders waypoints through `nav.orderedHeading` and `nav.throttle`. | The helm (so we can go and look). | The contact moves between sweeps and loops its route. Stops while paused. |
| **B** | **Done, Step 97** (its own hearing rule, §4.1). The Garuda hears. Awareness 0 → 3 from its own sensors (`SensorSystem.Heard`, `ConeCovers`) reading our `sensors.signal`. Thresholds in Tuning. A debug readout in the Tuning window or the console. | A. Power P1 (done). | Going loud near it raises the number. It never drops. |
| **C** | **Done, Step 97, without the sweep:** it does not go active, so no sweep shows on our board. Investigate and sweep. SEARCHING steers to last-heard; ALERT runs the sweep, then closes. The sweep shows on our board. | B. The sensors step. | It turns toward where we were. On ALERT its signal jumps and it comes straight at us. |
| **D** | Derelicts. Two static contacts from a DERELICT class, with hull sides and compartments. Scanning one fills in the target readout: its sides and compartments, drawn as ours are. | Sensors. | They show, scan, read DERELICT, and the readout shows their insides. |
| **E** | The comms box and the step runner, with steps 1 to 5 only. No gating yet. | Nothing. Can be built now. | The box appears top-left, walks power, and clears on each done check. |
| **F** | Strict gating. The allow-list, checked once before dispatch, refusing with the existing nudge. | E. The press-and-click step (the nudge). | A control not yet introduced nudges and does nothing. |
| **G** | The rest of the walkthrough, one station at a time as each gets wired: helm, plot, sensors, comms, orders, engineering (with the drill), hangar, periscope. | Each station. | Each row in §8 clears in play. |
| **H** | Fire on the derelicts, hit, destroy, WRECK. Steps 19 to 22. | Fire, hit, damage from the wiring order. | Both derelicts can be killed. |
| **I** | Fight the Garuda. Steps 23 to 27: scan it, the kill, the end card, back to the menu. Our hits on it update its readout live. | A to H. | NEW GAME to "let's get out of here" in one sitting. |
| **J** | **Done, Steps 55 and 97.** The Garuda fights back. When we hit it, it fires an HE missile salvo of 2 at us, at most once every 120 s, if we are inside its VLS reach. Once ALERT it fires both turrets and a salvo every 90 s. Its turrets do not use our arming ladder (§4.4). | I. Our own fire and damage model. | It hurts. Engineering is now real. |

A, B, C and J are done (A is now a hunt). D to I, the tutorial half, are not built. E can
start today.

---

## 10 · Open questions for the owner

1. **Clock at start.** The board loads PAUSED today (the scene saves it that way; nothing fills
   until you press the pause key). The FTUE wants the ship under way at load, so the clock runs
   from the first frame. Confirm. If yes, the first XO line teaches pausing, as written.
2. **The engineering drill.** The load state is undamaged (owner, 2026-10-04). To teach a repair
   before anything has shot us, the XO stages a drill: one compartment drops to INTEG 4. Yes, or
   move engineering after the Garuda shoots back (step J)?
3. **The XO's name** and portrait. Any name, and a placeholder tile, unblocks step E.
4. **The end.** After "let's get out of here": a plain text end card and back to the menu? The GDD
   has a credits screen. Minimum is the card.
5. **The derelicts' story.** What are they? Imperial wrecks from the war? One line from the XO at
   step 11 is all it needs.

---

## 11 · Traps

- **Do not write a second ship.** No enemy-only movement, power, sensor, weapon or damage code. The enemy is a second set of the same system components, driven by a brain. If the shared code needs a "which ship" parameter, add the parameter.
- **Do not run anything in `Update`.** The world steps on `board.Ticked`, inside the single timer.
  Spec Rule 2.d: nothing real changes while paused.
- **Do not port the old scripts.** `EnemyShip`, `Radar`, `PlayerShip`, `Autopilot` are the older
  build. Owner 2026-10-17: the new world feeds the board.
- **Do not hardcode a number.** A ship's numbers go in its stats file (`Assets/Ships`), on the
  Ship and Enemy tabs of **Starwolf → Tuning**. Brain and board numbers are `[Tune]` fields, on
  the Board tab.
- **Do not put text on the plot.** The comms box is a panel, not a plot label (owner, Step 39).
- **Do not build a quest log.** One box, one line, one hint.
- **Do not add a second lock system.** One allow-list, one check, before dispatch.
- ~~**Do not add reinforcements**, even behind a flag. The owner ruled none.~~ Replaced (owner,
  2026-10-27): reinforcements are in (spec §5.4).
- **Do not hand-edit the scene.** Everything goes through the Editor over MCP.
