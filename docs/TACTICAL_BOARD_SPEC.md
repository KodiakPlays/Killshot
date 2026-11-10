# Starwolf — Specification

**Version** 0.4 · 2026-10-29
**Status** Describes the tactical board as built on 2026-10-29. Sections marked **Not built yet**
describe design intent.

Each ship's numbers are in its stats file: `Assets/Ships/Asura.asset` (our ship) and
`Assets/Ships/Garuda.asset` (the enemy). Change them with **Starwolf → Tuning**. The owner's
decisions are in `docs/BUILD_LOG.md`, section 4.

---

## Summary

> *The Emperor is dead, betrayed by the traitor lords. You are the Captain, revived from stasis in a
> secret spacedock. You are deep behind enemy lines; nowhere is safe. You command a single ship.
> Carry out the Emperor's final edict: revenge on the traitors.*

**Starwolf is a top-down 2D real-time tactical space combat game.** You command one starship. You
enter a sector of space, hunt down the Traitor Lord's flagship, kill it, and escape.

**The novelty is that you play the entire game through a control panel.** Apart from very select
moments, there is no 3D view of the outside. You read the world through instruments — navigation,
weapons, troops, sensors, signals intelligence — and act through them.

**Your ship is an Asura-class destroyer, built for stealth.** It wins by patience and position. You
arrive broadcasting a false ID; every enemy reads you as friendly. The game is about how long you
can keep that lie going, and picking the moment to drop it (§5, not built yet).

### Where the fun is

Straight from the GDD:

- Managing the position of your ship
- Using sensors to scan enemy ships for weakness and gather complete data
- **Using that data to disguise yourself as an enemy ship**, hiding in gas clouds or behind asteroids
- Managing your power to get into range and angle of fire
- Getting the perfect shot, or laying the perfect trap

### Tone references

**Highfleet** for the mechanical UI · **In Other Waters** for the sonar-style interface ·
**Battlefleet Gothic** for combat · **Sunless Skies** for movement

---

## How it plays

### The shape of a session

`Title → pick a Traitor → pre-launch → hyperspace → the level → escape → credits`

You drop in at one edge of a handcrafted region of space, with **old, unverified Imperial
records** that mark a few landmarks. Everything else you have to find.

Built so far: an intro, a menu and a MISSIONS list with eight levels (§15.5). The first six are short,
with one goal each. The seventh is the full mission, Wolf Hunt, in `Level_01`. The Garuda hunts you.
Once it finds you, more enemy ships come. Kill or capture it, then reach the jump point to win
(§15.2). The eighth, LONG SHOT, is a test range for the spinal railgun (§8.4).

### Order of operations

From the GDD, in the order you actually do them:

1. **Explore.** Move around. Find the enemy. Find points of interest. Work out where to set up a
   strike. There is no fuel and no clock pressure yet.
2. **Read the world with DRADIS.** Figure out what is going on and decide **what to target**.
3. **Calibrate power** to optimise your approach — range, angle, and how loud you are.
4. **Engage** with weapons and systems.
5. **Take care of yourself** while doing all of it.

### The three stages of a mission

| Stage | Done when |
|---|---|
| **Locate the target** | You confirm the flagship |
| **Eliminate the target** | It is destroyed |
| **Exit the region** | You reach the jump point at the map edge and jump out |

**Killing the flagship is not winning.** An extraction marker appears at a far edge and you have to
get there, through whatever you stirred up (§15).

### The five things that make it hard

| | |
|---|---|
| **Power is the whole decision** | Ten reactor units across four buses that each take five. You can fully run two. A unit left in the reactor makes no noise. A unit on a bus does. |
| **Everything you do makes you heard** | Every unit of power makes you louder. So do an active scan and every shot. |
| **You drift** | Turning points the ship. It does not change where you are going. |
| **Your armour is a direction** | No shields. A bow, a stern and two sides, each with its own armour. |
| **Shots take time** | A spinal railgun slug takes 20 seconds to reach 30,000 units. The Garuda's railgun slug takes about 7 seconds to reach 2,000. A torpedo takes 15 to 18 seconds to reach 3,000. Its motor lights 3 to 7 seconds in, once it has curved onto the target. You aim where the target will be. |

---

## Features, one at a time

Everything below breaks down system by system. Read in order for the whole picture, or jump.

| § | System | What it covers |
|---|---|---|
| 0 | Sources | Which source is newest |
| 1 | Conventions | Bearings, kilometres, the board step |
| 2 | Time | Pause, the four rates, game time |
| 3 | Power | The reactor, four buses, draw and vent |
| 4 | Movement | Throttle, drift, side thrusters, stability, the combat autopilot (not built yet) |
| 5 | Being seen | Our signal; how the enemy hears us; reinforcements; the disguise (not built yet) |
| 6 | Auxiliary | The orders station, its AUX pool and the alert condition |
| 7 | Reflex | The cloak (not built yet) |
| 8 | Weapons | The spinal railgun, four PDC groups, two torpedo launchers, damage; the Garuda's two railgun turrets |
| 9 | Sensing | DRADIS, the sweep, the active scan, what the plot shows of a ship |
| 10 | Comms | Tuning and the cipher |
| 11 | The board | Layout, plot, rails, colour, type, the periscope |
| 12 | Hangar | Interceptors, assault boats, launching |
| 13 | Damage | Four sides, six compartments, conditions, repairs |
| 14 | Failure | How our ship is lost |
| 15 | Mission | How a mission runs, how you win, the level |
| 16 | Prototype defects | Bugs in the prototype not to copy |
| 17 | Open items | What is still to decide or build |
| 18 | Plotting tools | The tools and the firing solution |
| 19 | Station panels | The six drawers and three stations |
| 20 | Fire control | The Fire Control drawer, in full |
| A | Appendix | How the older scripts differ from the board |

---

## About this document

## 0 · Which source is newest

**Newest wins.** These sources are a timeline, and later always beats earlier. The GDD is the
**oldest** of them — it explains what the game was originally for, not what it is now.

```
oldest ────────────────────────────────────────────────────────────────► newest

GDD + Art Spec  →  Unity build  →  prototype  →  Silent Shark  →  handoff  →  owner, in session
the original        a partial       the latest     how it should    the current   the definitive
vision. Why the     older build.    working        be presented.    visual        word. Beats
game exists.        Historical.     expression.                     system.       everything.
```

| Rank | Source | Decides |
|---|---|---|
| **1** | **What the owner says, most recently** | **Everything.** Overrides all of the below. |
| 2 | Claude Design handoff | The visual system — colour, type, layout, marker shapes |
| 3 | Silent Shark | How the game should present itself |
| 4 | `References/starwolf-prototype.html` | How systems behave, and their numbers |
| 5 | `References/Starwolf GDD.pdf` | **Why** the game exists, and its original intent. Old. |
| 6 | `References/Starwolf_ Art Spec.pdf` | Ship design language only. Superseded on visuals. |
| 7 | The Unity build | What code exists today. Decides nothing. |

> **Rule 0.a** **Read the GDD for intent, not for numbers.** Where the GDD and the prototype give
> different values for the same thing, **the prototype is later and wins**. Where the owner has ruled
> in session, that wins over both.

> **Rule 0.b** The GDD's own pages 68–74 are marked *"notes and unfinished concepts — not part of
> spec"*. Treat anything from them as a sketch, not a rule.

### What the GDD is still the best source for

Not numbers. These:

- **Why you are out there** — the Emperor is dead, you are a captain revived from stasis, this is revenge.
- **What the game is about** — position, scanning for weakness, disguising yourself as an enemy ship, getting into range and angle, the perfect shot or the perfect trap.
- **That you are a stealth destroyer, not a brawler**, and that being detected is a rhythm rather than a failure.
- **What winning is** — locate the flagship, kill it, escape through the jump point.

## 1 · Conventions

**What it is.** The shared rules for angles, distance and the update loop. Every other system uses
them.

**How it plays.** The player reads one bearing style everywhere — three digits, `062°` — and every
dial, ship picture and readout agrees with every other one.

**How it's built.** One bearing value per object per frame feeds every heading visual. Distances are
units. The rules step once per board step, in a fixed order.

### 1.1 Bearing

The battle space is a **2D plane**. Positions are `(x, y)` in units, x east and y north.

- `0°` is `+y`, increasing **clockwise**. `090°` is `+x`.
- Shown zero-padded to three digits: `062°`.
- **Screen angle = bearing − 90°.**

```
angDiff(a, b) = ((a - b + 540) mod 360) - 180        // (-180, 180]
```

> **Rule 1.1.a** Every visual that comes from a heading — the ship picture's rotation, the `HDG`
> readout, a PDC arc — comes from **one** bearing value per object per frame.

### 1.2 Units

Distances are **units**. Speeds are units per second. Times are game seconds. At x1 a game second is a real second (Step 130).

| Number | Value |
|---|---|
| Battle space | 20,000 × 20,000 units, set per level with a Battle Space box (Starwolf → Level) |
| DRADIS reach | 5,000 units at gain 1, 7,500 at gain 5 |
| Visual range | 2,000 units |
| Railgun reach | 2,000 units (the Garuda's turrets) |
| Spinal railgun reach | 30,000 units (our ship, Step 170) |
| Torpedo reach | 3,000 units |
| PDC reach | 600 units |
| Speed limit | 25 units/s at ENG 1, 50 at ENG 5 |
| Plot scale at zoom 1 | 760 px per 1,000 units |

> **Rule 1.2.a** Distances are units. A small plot square is 50 units (owner, 2026-10-29).

### 1.3 Loop

The board steps every 0.42 s of real time. Each step moves the game on by 0.42 game seconds times
the rate: x0.25, x1 or x5 (§2). At x1 the game clock keeps real time.

One step runs in this order: the mission clock, power, the AUX pool, our ship's movement, the
battle space check, slugs and torpedoes, fighters and boats, enemy torpedoes and our PDCs, damage
control, fire, vacuum and radiation, the launch stages, the weapons capacitor, sensors and signal.
Then each enemy thinks and moves.

> **Rule 1.3.a** A stalled frame never makes the board catch up in a burst. Missed steps are
> dropped.

Between steps, a ship drawn smoothly glides on along its velocity (§9.6.a).

---

## 2 · Time

**What it is.** Two clock states, running and paused, plus three rates for how fast the running
clock moves.

**How it plays.** You can stop the world to think and run it fast when nothing is happening. There
is no plan-then-execute step: orders take effect as you give them.

**How it's built.** One board clock drives everything. Every rate in the game is per game second,
so at x5 everything runs five times as fast (owner, 2026-10-24).

| State | Behaviour |
|---|---|
| **Running** | The game advances at the rate. |
| **Paused** | Nothing real changes. Controls still work. |

The rates are x0.25, x1 and x5, on three keys on the time control. The board loads paused.
Pressing a rate key also starts the clock. The pause key, or P on the keyboard, stops and starts
it. Crossing out of the
battle space drops the rate to x1 (§14).

> **Rule 2.a** Pause and play rate are the **only** time controls. There is no commit or execute
> step. `DIVERGES` from the handoff, which asks for a paused-time commit loop.

> **Rule 2.b** Opening a station panel does **not** pause the game. From the GDD: *"the clock does
> not pause, even when systems are offline or the player is in a menu."* `DIVERGES` from the
> prototype, which pauses on SCANNER and COMMS.

> **Rule 2.e** The clock is universal. It drives the enemy, the DRADIS sweep, every rate and the
> mission clock.

> **Rule 2.c** The rules step on the board clock alone. No rule may read frame count or wall clock.

> **Rule 2.d** Ordered state (§4.1) can be set while paused. Actual state changes only while
> running (owner, 2026-10-17).

---

## 3 · Power — the central decision

**What it is.** The reactor gives ten units of power. Four buses take them: **ENG** (engines),
**WEP** (weapons), **SEN** (sensors) and **AUX** (auxiliary). Each bus takes up to five.

**How it plays.** Ten units across four buses that each take five means **you can fully power two
of them.** That is the whole game in one sentence. Every setting says what you are doing right now
and what you have given up.

**How it's built.** Each bus has a unit count, 0 to 5, and a selector: VENT, STANDBY or DRAW. You
set the selectors on the Power drawer. The numbers are in each ship's stats file, under power.

### 3.1 How it plays

> **Rule 3.1.a** Units stay on a bus until you vent them. Using a system uses no units. The reactor
> never runs down.

> **Rule 3.1.b** **Every unit on a bus makes you louder.** Each unit adds 10 to our signal, so a
> full reactor reads 100 (§5.0). Units left in the reactor add nothing. Running at ten is the most
> capable and the easiest to hear. Running at three is quiet and nearly helpless.

> **Rule 3.1.c** **Moving units takes time.** A bus gains or loses one unit per game second. Moving
> units from one bus to another means venting one and drawing on the other. You cannot swap setups
> mid-fight. Being caught in the wrong one is a real way to lose.

> **Rule 3.1.d** **Damage shrinks the pool.** A hurt reactor gives you fewer units (§3.2).

> **Rule 3.1.e** The board loads with power cold. Every bus is empty and on STANDBY (owner,
> 2026-10-25).

The intended feel: *sensors up and quiet, or weapons hot and heard — and switching costs you the
seconds you least want to spend.*

### 3.2 The numbers

10 reactor units · 5 per bus.

The reactor's cap falls with the reactor room's integrity, 0 to 5 (§13.3). A vented room counts as
0 here.

```
cap = round(reactor integrity ÷ 5 × 10), never below 2
```

| Reactor integrity | Cap |
|---|---|
| 5 | 10 |
| 4 | 8 |
| 3 | 6 |
| 2 | 4 |
| 1 or 0 | 2 |

Reactor integrity 0 destroys the ship (§13.4).

> **Rule 3.2.a** Over the cap, units come off the last buses first: AUX, then SEN, then WEP, then
> ENG. This happens at once.

### 3.3 Moving units

The selector steps one place per click: VENT ↔ STANDBY ↔ DRAW. At each end it turns back.

| State | Each game second |
|---|---|
| VENT | The bus loses one unit. At zero it stays on VENT. |
| STANDBY | Nothing changes. |
| DRAW | The bus gains one unit, if it is under 5 and the reactor has a unit free. |

Venting comes first, so units freed by a vent can be drawn in the same second.

> **Rule 3.3.a** Every bus on DRAW gains one unit per game second, until the reactor has no unit
> free.

> **Rule 3.3.b** A vent can be stopped. Click the selector back to STANDBY.

> **Rule 3.3.c** The **emergency load shed** puts every bus on VENT and locks the selectors.
> Releasing it puts every bus on STANDBY.

An EMP empties every bus at once. The power stays out for 60 game seconds (§8.1a). With no power
on ENG, the ship drifts (§4.3).

### 3.4 What a unit is worth

| Bus | What its units do |
|---|---|
| ENG | ENG 1 allows 25 units/s and ENG 5 allows 50, in equal steps. ENG 5 also gives twice ENG 1's acceleration, braking, turn rate and side thrust (§4.2, §4.3). With no ENG units, the ship drifts. |
| WEP | Feeds the weapons capacitor, 5 per unit per game second (§8.4). Weapons need at least 2 units. |
| SEN | Caps the DRADIS gain: up to the SEN units, never below 1 (§9.1). |
| AUX | Fills the orders pool, 3.2 per unit per game second (§6). The hangar's catapult needs at least 1 unit (§12.3). |

> **Rule 3.4.a** **Lose power, lose the setting** (owner, 2026-10-25). When a bus can no longer
> support a setting, the setting drops to zero or its lowest and stays there. A throttle above the
> new cap drops to 0. A gain above it drops to 1. You set it again.

---

## 4 · Movement

**What it is.** A ship you steer by an ordered heading and a throttle. The hull turns; the velocity
takes time to follow.

**How it plays.** The ship slides. Turning changes where your guns and your armour point. Your path
bends round slowly after it. The side thrusters slide the ship sideways with the nose still. More ENG
gives more of all of this: more speed, more thrust and tighter turns.

**How it's built.** Every ship has an ordered heading and throttle, and an actual heading and
velocity. Each board step, the bow swings toward the ordered heading, and speed along the nose
chases the throttle. The sideways part of the velocity bleeds off. The numbers are in each ship's
stats file, under navigation.

### 4.1 Ordered and actual

Every ship has ordered state (heading, throttle) and actual state (heading, velocity, position).
Actual moves toward ordered at capped rates whenever the clock runs.

> **Rule 4.1.a** Our ship and the enemy move by the same rules. The enemy is our ship with other
> stats. Fighters and assault boats fly at a fixed speed and turn gradually onto where they are
> going (§12.3).

### 4.2 Throttle

You drag the throttle on the helm, or use W and S. A tap of W or S moves it 1 (owner, 2026-11-09).
Keep the key down and it keeps counting, slowly at first, then faster. It moves 100 in Throttle Hold
Seconds (2), on the Helm tab of **Starwolf → Tuning**. It stays where you let go. If the throttle
changes some other way while the key is down, the key stops moving it. It runs from -100% (full
reverse) to +100% (full ahead), with 0 in the middle of the slider (owner, 2026-11-02). S goes down
through 0 into reverse. It loads at 0.

```
speed limit = 25 + (ENG units − 1) × 6.25 units/s, with no thrust at ENG 0
throttle cap = speed limit ÷ 50 × 100 × (engine room integrity ÷ 5), ahead and in reverse
target speed = throttle ÷ 100 × 50 units/s, times Reverse Share in reverse
```

ENG also sets how fast speed changes (owner, 2026-11-01). On our ship, speed along the nose rises at
1 unit/s each second at ENG 1 and at 2 at ENG 5, in equal steps. It falls at 1.875 at ENG 1 and at
3.75 at ENG 5. Base Accel Units in the stats file gives the ENG 1 number. Enemies run at full ENG.

> **Rule 4.2.a** If the ENG bus can no longer support the throttle, the throttle drops to 0 (owner,
> 2026-10-25). This is the same ahead and in reverse. You set it again.

> **Rule 4.2.b** Full reverse is slower than full ahead (owner, 2026-11-02). Reverse Share (0.42,
> from the HTML prototype) is in each ship's stats file, under Navigation. Reverse top speed is that
> share of the top speed. Speeding up backwards uses that share of the acceleration. Slowing down, in
> either direction, uses the brake rate. The enemy never uses reverse.

### 4.3 Drift — the ship slides

> **Rule 4.3.a** `A` and `D` step the ordered heading by 3° per press. The bow turns toward it at
> 10° per game second at ENG 1 and 20° at ENG 5, in equal steps (owner, 2026-11-01). ENG 0 turns at
> the ENG 1 rate. Base Turn Rate in the stats file gives the ENG 1 number. It is slower when the
> engine room is damaged (never below 1°). The velocity keeps its old direction and bends round
> slowly. Pressing A or D also clears the helm's attitude key.

Each game second, velocity splits into a part along the nose and a sideways part:

```
sideways speed × 0.96307
all speed      × 0.997502
```

> **Rule 4.3.b** With no units on the ENG bus there is no thrust and no braking. The ship drifts on
> at the same speed and direction (owner, 2026-10-29). This is true of every ship, so an EMP leaves
> a ship drifting too.

> **Rule 4.3.c** `Q` slides the ship to port and `E` to starboard (owner, 2026-11-01). The nose
> stays where it is. On our ship the slide reaches 20 units/s at ENG 5 and 10 at ENG 1, in equal
> steps. A slide adds no signal.
>
> A tap gives a small push (owner, 2026-11-09). It adds Side Tap Units (2 units/s) of side speed at
> ENG 5 and half that at ENG 1. Every tap counts, however short. Taps never push past full side
> speed. Two taps very close together count as one.
>
> Keep the key down and the push starts gentle. It grows to full over Side Ramp Seconds (2). The
> full push adds 4 units/s each second at ENG 5 and 2 at ENG 1. Our ship reaches 20 after about 7.5
> seconds.
>
> Let go, and the slide keeps going. It fades like any sideways drift. On our ship it halves in about
> 8 seconds (Drift Keep Per S). A tap the other way slows it. The owner's idea: tap left and right to
> steady the ship, most of all when coming to a full stop.
>
> Rule 4.3.b covers ENG 0: Q and E do nothing, and a slide already going doesn't stop. Side Speed
> Units, Side Accel Units, Side Tap Units and Side Ramp Seconds are in each ship's stats file. Change
> the keys on the Keys tab of **Starwolf → Tuning**.

> **Rule 4.3.d** Shift with `A` or `D` is the slow turn, for aiming the spinal railgun (owner,
> 2026-11-08; Step 170). While the keys are down, the bow turns 2° per game second, in parts of a
> degree. ENG power doesn't change it. The ordered heading follows the bow, and the helm's attitude
> key clears. The helm's HDG still shows whole degrees. A normal `A` or `D` turn drops the part of a
> degree. Fine Turn Deg Per S is in each ship's stats file, under Navigation. The key is Fine Turn
> Key (LeftShift), on the Keys tab.

> **Rule 4.3.e** With a lock on and the spinal railgun picked, our ship turns itself onto the lock's
> lead (owner, 2026-11-09; Step 170). It turns the whole degrees at its usual rate, then the last part
> of a degree at the slow turn's rate. `A` and `D` can't turn against it. The slow turn still can. When the lock
> runs out, the helm is the player's again (§18.4).

### 4.4 Stability

The helm shows G load and stability. Nothing uses them yet.

```
G load    = 1 + |throttle| ÷ 100 + (FLANK 0.5, EVASIVE 1.2, DIRECT 0)    (-80 and +80 both give 1.8)
stability = 100 − 45 × (G load − 1), from 0 to 100
```

### 4.5 Collision

Every ship is a solid box, length by width, turning with the ship (owner, 2026-11-01). Nothing passes
through a ship or a piece of debris. A ship that touches something is pushed clear, bounces, and
keeps 30% of its speed. It takes damage on the side that touched, by its speed. In a crash between
ships, both take damage. Pressing against a rock with the throttle on gives one small hit every 3
game seconds. The alarm strip reads `COLLISION · PROW ARMOUR 75%`. The numbers are in each ship's
stats file, under damage.

### 4.6 Autopilot (combat)

**Not built yet.** The helm's attitude keys, approach range and approach mode can be set. They do
not steer the ship yet. The helm's header reads MANUAL, or AUTO and the approach mode while an
attitude key is lit.

**Owner, 2026-10-25.** A combat autopilot. It only works on a target. Its settings are on the helm.

- **Target:** fire control's selected target. It must be identified by an active scan, because an
  autopilot cannot hold range on a ship you can't see.
- **Facing:** NOSE, STBD, PORT or TAIL (the helm's attitude keys). That side is kept toward the
  target.
- **Range:** the helm's approach range: 500, 1,000, 1,500, 2,000, 3,000 or 5,000 units. It tries
  to stay at that range.
- **Speed:** the approach mode. DIRECT, FLANK and EVASIVE each set a throttle, which is still
  capped by the ENG bus. The numbers are in the inspector, and the owner tunes them.

```
offset = { NOSE: 0, STBD: -90, PORT: +90, TAIL: 180 }[facing]
ordered heading = bearing to target + offset
```

With a beam facing, the heading turns a little inward when the ship is outside the range, and a
little outward when it is inside. So the ship settles onto a circle at that range.

> **Rule 4.6.a** A beam facing circles the target: STBD circles clockwise, PORT counterclockwise.
> The ship moves where its nose points, so the side sets the direction.

> **Rule 4.6.b** NOSE closes to the range and keeps the nose on the target. If the target stops, our
> ship stops. If the target moves, our ship keeps repositioning. TAIL is not defined yet.

> **Rule 4.6.c** Nothing moves until **A/P** is pressed. A/P is a push button with a lamp on the
> helm. Picking a facing, range or mode on its own does nothing. While A/P is engaged, a changed
> setting takes effect at once. The helm's status line reads AUTOPILOT ENGAGED or AUTOPILOT
> DISENGAGED.

> **Rule 4.6.d** A/P refuses to engage if any of these is missing: a target, a scan of the target, a
> facing, or power on ENG. The helm says which one is missing.

> **Rule 4.6.e** W, A, S or D, or dragging the throttle, disengages it at once. The ship keeps its
> heading and speed. The lamp goes out and a short tone plays. The settings stay, so one press of A/P
> resumes. There is no grace window.

> **Rule 4.6.f** If the target is destroyed, it disengages and says so. *Proposed, not confirmed:*
> if the target leaves sensor range, it disengages and reads TARGET LOST.

> **Rule 4.6.g** It does not avoid terrain.

> **Rule 4.6.h** `DIVERGES` — the prototype, the design handoff and the audit give the autopilot no
> behaviour. The handoff's controls only set values, and approach mode only adds G. Soham's
> `Autopilot.cs` is set aside: it cannot hold range while keeping a beam to the target, and it never
> backs off.

---

## 5 · Being seen

**Part built.** Our signal (§5.0), how the enemy hears us (§5.3) and the reinforcements (§5.4) are
built. The detection number (§5.1) and the disguise (§5.2) are design intent, not built yet.

**What it is.** How visible you are, and what the enemy does about it.

**How it plays.** You are a stealth destroyer, not a brawler. You start the level *disguised*.

**You will be detected.** That is not a failure state and not the end of the mission — it is the
rhythm of the game. You get found, you break contact, you hide, you move, you come back on your own
terms. Going dark and going fast are both answers, and knowing which one a moment calls for is the
skill.

The loop underneath it: **hide → gather → act → get found → break away → gather again.** Every tool
you have serves one of those beats. Scanning and boarding feed the gathering; power and speed and
the gas clouds feed the hiding; the disguise buys you the approach.

### 5.0 What is built: our signal

Our signal is a number from 0 to 100. The player sees it as a whole percent on the signal box, in
the frame's top-right, and on the Sensors drawer.

```
signal target = 10 per unit on any bus + 35 while ACTIVE is on        (at most 100)
```

The signal climbs toward its target at 9 per game second and falls at 6. Each railgun shot or
torpedo salvo adds 25 at once. As the signal crosses 45, the alert tone plays once. The numbers are
in each ship's stats file, under sensors and power.

The signal box says who can hear you. While an enemy ship hears or sees our ship, it reads
`DETECTED BY 1 HOSTILE` (or `DETECTED BY 2 HOSTILES`), at any signal. At 45 or more with nobody
hearing, it reads `UNDETECTED`. At 0 it reads `RUNNING SILENT`.

### 5.1 The detection number

From the GDD:

```
Detection = energy in systems + active ability + environment effect
```

Worked example, straight from the GDD: 3 points in WEAPONS, firing the gun (+2), inside a gas
cloud (−20%):

```
(3 + 2) − 20% × (3 + 2) = 5 − 1 = 4        // rounding up
```

> **Rule 5.1.a** **Power left in the reactor cannot be detected.** The reactor is shielded. Only
> power that has been *drawn to a bus* shows.

> **Rule 5.1.b** **Using a system costs more than powering it.** Flying fast and firing weapons add
> on top of the allocation.

> **Rule 5.1.c** Environment is a **percentage reduction**, not a subtraction. A gas cloud takes a
> cut of whatever you are putting out, so it hides a quiet ship completely and a loud one only
> partly.

> **Rule 5.1.d** Your signal is always on screen (§19.4.a). Enemy signals appear on your DRADIS
> the same way yours appears on theirs — it is one system, read from both ends.

> **Rule 5.1.e** Rocks make interference (owner 2026-11-02). One rule, for every ship in every level,
> ours and the enemy's alike.
>
> - Interference touches DRADIS and scans only. Comms is not touched.
> - Each rock on the straight line between a sensor and a ship takes Rock Interference Units (500) off
>   the reach. So does each rock close round that ship. Close means within Rock Close Units (500) of
>   the rock's edge. A large cluster therefore makes a ship very hard to pick up.
> - More power means more reach, so it pierces interference. It also makes the ship louder.
> - A rock on the line also blocks eyes fully, for our ship, the enemy and fighters. Interference does
>   nothing else to eyes.
> - Rocks throw no false dots, for us or for the enemy (owner 2026-11-02, Step 157). A rock is silent
>   until ACTIVE SCAN pings it (Rule 5.1.f).
>
> These two numbers are on the Board tab of **Starwolf → Tuning**, under Sensors.

> **Rule 5.1.f** Rocks show on the plot only within visual range (owner 2026-11-02, Step 157).
>
> - A rock shows within visual range of our ship or of one of our fighter flights.
> - Plain DRADIS doesn't show a rock. Only a ping from ACTIVE SCAN gets an echo off one.
> - As the DRADIS sweep passes a rock inside the ACTIVE SCAN cone, a clean, thin, grey hollow circle
>   shows at the rock (owner: "At best, they can be clean, hollow circles"). A big rock gives a bigger
>   circle, never a large one. The circle is bright as the ping passes and dims within about a second.
>   With no new ping it fades out over 30 game seconds.
> - Rule 5.1.e's interference cuts the ping's reach. The far side of a thick field stays dark unless
>   you add power.
> - A rock across the edge of visual range is cut at the circle. The part inside shows as the rock.
>   The part outside shows only as its hollow circle.
> - Gas clouds are unchanged by this rule.
>
> Rock Echo Seconds, Rock Echo Blink Seconds, Rock Echo Dim, Rock Echo Size, Rock Echo Min Px, Rock
> Echo Max Px and Rock Echo Ink are on the Board tab of **Starwolf → Tuning**, under Sensors.

### 5.2 The disguise

> **Rule 5.2.a** **Our ship begins the level broadcasting a false ID.** Every enemy reads it as
> friendly or neutral. **All enemy ships are passive at the start.**

> **Rule 5.2.b** An active scan of a ship (§9.4) returns its energy signature. **Matching your own
> output to that figure lets you pass as that class of ship** and move in close.

> **Rule 5.2.c** The disguise breaks on any of three things: **getting too close**, **using
> weapons**, or **a failed deception** — an output that does not match what you are pretending to be.

> **Rule 5.2.d** Powering everything down and hiding in a gas cloud or debris field is the other way
> to stay unseen. Disguise is for getting close; hiding is for not being there at all.

This is the centre of the game. The GDD names the fun as *"using that data to disguise yourself as
an enemy ship, hiding in gas clouds or behind asteroids"* — the scan is not reconnaissance, it is
how you get your costume.

### 5.3 Awareness

Once provoked, an enemy climbs a ladder. It comes back down only when it gives up (Rule 5.3.a).

**Built (Step 97, owner 2026-10-27).** Every enemy ship listens with its own DRADIS. It hears our
ship out to its DRADIS reach times our signal (0 to 100%), so a silent ship is never heard. Inside
its visual range it sees us, whatever our signal. Our fighters are loud to it too.

| Level | Raised when | What it does |
|---|---|---|
| COLD | At the start | Hunts the map. It picks points across the battle space, moving from its own half toward ours. With a patrol route, it flies the route (Rule 5.3.b) |
| UNEASY | It hears us faintly | Turns for where it heard us |
| SEARCHING | It hears us more clearly, or hears us again after a gap | Speeds up and searches round that spot |
| ALERT | It hears us loud and close, sees us, or we hit it | Closes to its railguns' reach, turns a turret's side to us, and fires railgun slugs and torpedo salvos while it knows where we are. The reinforcements' countdown starts (§5.4) |

The code is `EnemyBrain`. Its numbers are on the Board tab of **Starwolf → Tuning**, under Enemy.

The prototype's ladder, in the prototype's own units, for reference:

| Condition | Raises to |
|---|---|
| `dist < 18000 && sig > 2.0` | 1 — UNEASY |
| `dist < 12500 && sig > 3.0` | 2 — SEARCHING |
| `dist < 8500 && sig > 4.5` | 3 — ALERT |
| Alert active, reinforcements away | 4 — GENERAL ALARM |

> **Rule 5.3.a** An enemy gives up after Give Up Seconds (90) of hearing nothing, in every level
> (owner 2026-11-02). It goes calm and back to its patrol.

> **Rule 5.3.b** A patrol route is set on the level's Enemy Spawn, and its points can be dragged on the
> Level map. At each point, Turn Back Chance (25%) is the chance that the ship turns back
> (owner 2026-11-02).

> **Rule 5.3.c** An enemy ship never leaves the battle space (owner 2026-11-02). This covers the
> Garuda and the frigates. It never heads for a point in the edge warning band (Rule 14.a). A point
> there moves in to the band's inner edge. In the band it never heads farther out. It runs along the
> edge instead, even while it chases us or turns a side to us to fire. If it is past the edge, it
> heads straight for the middle. It does the same in the band if it is heading straight out or into a
> corner. At full speed, at worst, the Garuda drifts about 520 units out while it turns, and a frigate about 750. There is no new
> setting.

### 5.4 The alert net

**Built (Step 99, owner 2026-10-27).** The moment an enemy ship reaches ALERT, the alarm strip reads
REINFORCEMENTS INBOUND with a 3:00 countdown in game time, and the alert tone plays. At 0:00 two
Naga-class frigates enter at random edges of the battle space, told roughly where we are. One more
comes every 2 game minutes until we jump out. The strip then reads ENEMY SHIPS IN THE SECTOR and the
count. Each frigate starts SEARCHING and hunts with the same mind as the Garuda (§5.3). The frigate's
numbers are in `Assets/Ships/Frigate.asset`. The countdown and the waves are on `QuestSystem`, on the
Board tab of **Starwolf → Tuning**, under Mission.

The prototype's version, for reference: firing any weapon triggers it. A 59-second window, then
two reinforcements at once, then one every 14 s, alternating DESTROYER / FRIGATE, entering at the
arena edge and closing at 92 units/s, ramming for 400 damage.

> **Rule 5.4.a** On the board, firing does not start the countdown by itself. Only an enemy ship
> reaching ALERT does. A shot adds 25 to our signal, which makes us easier to hear (§5.0). A hit on
> an enemy puts it on ALERT at once.

> **Rule 5.4.b** Reinforcements do not stop. The alert is a **soft fail timer**: once it is running,
> finish and leave.

## 6 · Auxiliary — the orders station

**What it is.** The AUX bus fills a pool of charge on the orders station. You spend it on damage
control orders. The station also sets the alert condition.

**How it plays.** Nothing soaks damage for you. Recovery is something you choose to spend on.
Charge builds slowly, and an order you give now is one you cannot give a moment later.

**How it's built.** One pool, 0 to 100. It grows by 3.2 per AUX unit per game second. Each order
checks its cost and the room's state, then spends the charge and acts. The numbers are in each
ship's stats file, under orders.

> **Rule 6.a** Auxiliary **replaces shields**. Starwolf has no shields. The AUX bus is the recovery
> resource.

| Order | Cost | What it does |
|---|---|---|
| PARTY | 30 | Sends the damage control party to the room (§13.6) |
| SEAL | 25 | Shuts the doors of an airless room |
| VENT | 15 | Dumps the room's air. Any fire goes out. The crew and any party in the room die |

The orders act on the room selected on the Engineering drawer. Engineering's REPAIR, SEAL and VENT
spend the same pool, at the same cost (§13.6). FIRE, BLKHD and SCRUB are blank plates. They are not
fitted.

> **Rule 6.b** An order is refused when the pool is short, or when the room's state blocks it
> (§13.6). The refusal shows for 0.42 s, and the refused sound plays.

### 6.1 Alert condition

The alert selector steps CONDITION 3 → 2 → 1 → 3. The board loads at CONDITION 3.

| Condition | Effect |
|---|---|
| 3 | Weapons and flight operations are not released |
| 2 | Weapons and flight operations are released |
| 1 | As 2, and a damage control party repairs 2 integrity instead of 1 |

---

## 7 · Reflex

**Not built yet.**

`DIVERGES` — an owner addition. No prototype equivalent.

**What it is.** A cloak you switch on and leave on. While it is on you are quiet, and you are
also blind and unarmed.

**How it plays.** You trade information for cover. Everything you knew freezes where it was and
starts to go out of date in front of you, as a circle that grows around each old fix. Coming out is
how you find out what actually happened.

**How it's built.** One engaged flag. On engage, each track stores its last fix — position, speed
estimate and `lastFixTime` — and the sensors stop updating. Power is drawn from the SEN bus.

| Effect | Rule |
|---|---|
| Signature | Drops to `E_floor`, **tunable** |
| Sensors | Dead. No new returns of any kind |
| Weapons | Blocked. No firing, no locks |
| Throttle | `targetSpeed` clamped to `reflexSpeedMax`, **tunable** |
| Power | Draws from the SEN bus |

> **Rule 7.a** Reflex hides you and blinds you by the same amount. It trades information for cover.
> The stealth is never free.

On engaging, every track freezes at its last fix:

```
uncertaintyRadius = track.estimatedSpeed × (t − track.lastFixTime)
```

> **Rule 7.b** Frozen tracks stay at their last known position, with a circle of uncertainty that
> grows where you can see it.

On disengaging, sensors restart and tracks come back at their true positions. Tracks that are not
picked up again are marked stale and dropped after `staleTrackTimeout` (`TBD`).

---

## 8 · Weapons

**What it is.** Our ship has three weapon types: a spinal railgun, fixed on the nose; four
PDC groups; and two torpedo launchers (VLS-A and VLS-B). The Garuda keeps two railgun turrets (owner,
2026-11-08; Step 170). The slug is the fastest thing on the board. A torpedo is slower and hits
harder.

**How it plays.** The spinal railgun fires where the nose points. You aim it by turning the ship. Which way
the Garuda faces decides what its turrets can reach. PDCs are the only answer
to a torpedo, and each group covers a fixed arc. Every shot spends charge from the weapons
capacitor, and every slug and torpedo is counted.

**How it's built.** Fire control works one mount at a time. A shot needs a firing solution, the
arming ladder and enough charge (§20). The spinal railgun needs no solution (§8.4). Slugs and torpedoes fly across the plot. They hit a ship
they pass close enough to. The numbers are in each ship's stats file, under weapons.

### 8.1 Damage

What each hit does. The numbers are the firing ship's: in its Weapons group, or its Hangar group for
fighters (owner, 2026-10-27; BUILD_LOG decision 49).

| Weapon | Against armour | Through an open side |
|---|---|---|
| Railgun slug | Takes 50 armour off the side it hits | Takes 2 integrity off the room behind; 50% chance to breach it |
| HE torpedo | Takes 100 armour | Takes 2 integrity off the room behind; 60% chance of fire |
| PENETRATOR torpedo | Goes straight through | Takes 2 integrity off the room behind; 40% chance of fire |
| EMP torpedo | No damage | No damage. The ship's power goes out for 60 game seconds, and it drifts (§4.3) |
| Fighter run | Takes 10 armour per fighter | Takes 1 integrity per fighter off the room behind |

These are our ship's numbers. The Garuda's slugs and HE torpedoes hit 1.5 times as hard: a slug takes
75 armour and 3 integrity, an HE torpedo 150 armour.

Any hit on the reactor room, except a fighter's, has a 50% chance to leak radiation. That chance
is in the Damage group of the ship being hit. What damage does to a ship is in §13.

> **Rule 8.1.a** On our ship, two of our slugs open the prow, starboard or port side, and one opens
> the aft. The Garuda's armour is 1.5 times ours (§13.2), so our slugs need three hits on its prow,
> starboard or port side, and two on its aft. One HE torpedo opens any of our sides. Our HE
> torpedoes need two hits on the Garuda's prow, starboard or port side, and one on its aft.

### 8.1a Warheads

The torpedo launchers fire one of three warheads: HE, EMP or PENETRATOR. The board starts on HE.
The warhead selector is not drawn on the Fire Control drawer yet.

### 8.2 Terrain

Not built yet. Slugs and torpedoes pass through ruins and debris. Whether terrain should stop a
shot is not decided.

### 8.3 What you have

There are no broadside guns. Arcs are measured from the bow, clockwise.

| Mount | Arc | Covers | Reach | At the start |
|---|---|---|---|---|
| RG, spinal railgun, on the nose | 000°, fixed | Straight ahead only | 30,000 units | 12 slugs |
| PDC-A, PDC group A, bow right | 045° ±135° | 270° → 180° | 600 units | 2,400 rounds |
| PDC-B, PDC group B, bow left | 315° ±135° | 180° → 090° | 600 units | 2,400 rounds |
| PDC-C, PDC group C, stern left | 225° ±135° | 090° → 000° | 600 units | 2,400 rounds |
| PDC-D, PDC group D, stern right | 135° ±135° | 000° → 270° | 600 units | 2,400 rounds |
| VLS-A, torpedo launcher, left side | All round | All round | 3,000 units | 8 cells |
| VLS-B, torpedo launcher, right side | All round | All round | 3,000 units | 8 cells |

This is our ship's file, `Asura.asset`, as the owner set it on 2026-11-01. Step 170 (2026-11-09) put
the spinal railgun on a new dot at the nose, (0, 100), and took off the two side railguns, RG-A and
RG-B. Their dots on the long sides stay, empty, for lasers later. Each weapon stands on a hard point
on the Hard Points tab of Starwolf → Tuning.

> **Rule 8.3.a** Fire control works one mount at a time. You pick the system (RAILGUN, PDC or
> TORPEDO), then the mount. Torpedoes are the exception: the launcher picks itself (§8.5).

> **Rule 8.3.b** **Ammunition is a real count** (owner, 2026-10-27). It only goes down. There is no
> reload. With nothing left, LOAD refuses.

> **Rule 8.3.c** On the Garuda, each railgun turret covers one side of the ship, 90° wide (owner,
> 2026-11-01). Nothing covers the bow or the stern. Turning the ship brings a turret to bear.

> **Rule 8.3.d** Each of the four PDC groups covers 270°. Any bearing is covered by three of them.

> **Rule 8.3.e** A railgun with Half Arc 0 is fixed to the hull (Step 170). It fires straight along
> its arc. Our spinal railgun is one: it fires straight out of the nose. You aim it by turning the
> ship (Rule 4.3.d).

The armour is PROW 100, STBD 100, AFT 50, PORT 100 (§13.2). The aft is the weakest side.

The Garuda has two railgun turrets, RG-A at 090° ±45° and RG-B at 270° ±45°, each 2,000 units with
22 slugs. It also has two PDC groups and one launcher on its centre line, with our reach. It has 4 torpedo cells, and its PDC-B has no rounds. A Naga frigate has one railgun, RG-A, with 12 slugs, and no PDCs or torpedoes. Their
numbers are in `Assets/Ships/Garuda.asset` and `Assets/Ships/Frigate.asset`.

### 8.4 Railgun

Each ship's slug speed is in its stats file. The Garuda's slug flies at **300 units/s**, out to 2,000
units. It takes about 7 s to reach full range (Step 130).

**The spinal railgun** (owner, 2026-11-08; Step 170). The owner, 2026-11-09: "it is a spinal railgun
in this current configuration. It means it cannot swivel. It just points straight." Our slug flies
at **1,500 units/s**, out to 30,000 units. A full-reach shot takes 20 s.

- It can fire with no target, no lock and no firing solution. It fires straight along the nose's
  line, to the part of a degree. You aim it by eye, with the slow turn (Rule 4.3.d).
- A lock turns the ship onto the target for you (Rule 4.3.e). Only a lock does this. A TRACK
  solution from the plot does not.
- It needs no turret turn and no periscope bearing.

**Drift** (owner, 2026-11-08; Step 170). Every slug keeps the motion of the ship that fired it, ours
and the Garuda's. A drifting ship's shot drifts off the line, more the further it flies. A ship
sliding 5 units/s sideways puts a spinal railgun slug 100 units off at 30,000. You can fire while moving. You
just miss. The Garuda's aim allows for its own motion. So does our fire control's lead.

Every slug, ours and the Garuda's:

- A shot hits if it touches the target's box made **25% longer and 25% wider** (owner, 2026-11-01;
  Step 128). That is 437.5 × 125 on the Garuda. It is the same rule for torpedoes.
- A slug that reaches its gun's reach without a hit counts as a miss.
- Each shot spends 70 charge from the weapons capacitor and adds 25 to our signal.
- A hand-dialled bearing is in whole degrees. At 2,000 units one degree is about 35 units sideways.
- When the solution has a course, the shot is aimed where the target will be when the slug arrives
  (§18.3).

The weapons capacitor:

```
each game second: charge + 5 × WEP units, up to 100
with no WEP units: charge − 3 per game second
```

| Shot | Charge |
|---|---|
| Railgun | 70 |
| Torpedo salvo | 40 |
| PDC (to put a group on AUTO) | 25 |

> **Rule 8.4.a** The capacitor sets the rate of fire. At 5 WEP units it refills a railgun shot in
> under 3 s.

> **Rule 8.4.b** **One shot per arming.** Firing releases ARM. Throw ARM again to fire again, once
> the capacitor has the charge.

### 8.5 Torpedo

Owner, 2026-11-01: a torpedo is always slower than a slug. It leaves the ship slowly, then goes.
Owner, 2026-11-10 (Step 184): "drop, drift, flare", pushed out about 300 units so it shows when zoomed out.

1. It is pushed out of the launcher's side at 250 units/s, tilted 30° toward the stern.
2. It drifts cold for 3.5 game seconds, slowing fast at first, down to 5 units/s. It drifts about
   300 units. It doesn't keep the ship's motion, so a moving ship leaves it behind.
3. While it drifts, only its nose turns, onto the target, at up to 90° per game second. A turn takes
   0.3 game seconds to build up and to ease off.
4. When the drift ends its motor bursts on. It flies where its nose points. It gains 1,100 units/s
   each game second up to its full speed of 250 units/s, and turns toward its target at up to 140°
   per game second.

Owner, 2026-11-02: a line can be drawn out of a launcher on the Hard Points tab of Starwolf →
Tuning. With a line, the torpedo drifts along it in place of step 2, slowing the same way, and its
motor lights at the line's end. No ship has a torpedo line now (owner, 2026-11-10).

If its target is gone, it drifts on, then its motor lights anyway and it flies on. It flies in short
steps of 0.05 game seconds. The curve is the same at every game speed.

Each flies at most 3,000 units, the drift included. That takes about 15 game seconds. Enemy torpedoes follow the same
rules.

- The launchers reach all round, so nothing picks one (owner, 2026-11-01). The one on the target's
  side fires first.
- A salvo leaves one torpedo at a time, 0.3 game seconds apart (owner, 2026-11-10). It uses both
  launchers by turns, starting with the one on the target's side. With one launcher empty, the other
  fires them all. Torpedoes from the same launcher fan 20° apart. With our ship facing 000, a salvo of
  2 leaves on headings 120 (right side) and 240 (left side). A launcher on the centre line, as on the
  Garuda, sends them out of each side by turns.
- A torpedo needs a target identified by an active scan (§9.4).
- Each torpedo uses one cell. The salvo size is set in the stats file. The salvo knob is not drawn
  yet.
- A salvo spends 40 charge and adds 25 to our signal.
- A torpedo hits by the same rule as a slug (§8.4), ours and the enemy's.

The numbers are in each ship's stats file, under Weapons: `torpedoLaunchSpeedUnits` 250,
`torpedoTiltBackDeg` 30, `torpedoCoastSeconds` 3.5, `torpedoCoastSpeedUnits` 5,
`torpedoCurveDegPerS` 90, `torpedoTurnBuildSeconds` 0.3, `torpedoBurnUnits` 1,100,
`torpedoSpeedUnits` 250, `torpedoTurnDegPerS` 140, `torpedoSpreadDeg` 20 and `torpedoRippleSeconds` 0.3.

The look (Step 184): in the close-up a drifting torpedo is a small pale body along its nose. When its
motor lights, a soft flare grows and fades there, and on the table a small amber ring grows where it
lit. The sizes are on the Board tab, under Plot & our ship.

### 8.6 PDC

A PDC group shoots at enemy torpedoes by itself once it is on **AUTO**. You never fire it.

- Select the group on Fire Control and throw the arming ladder. ARM puts the group on AUTO.
  Releasing the ladder takes it off. Both groups can be on AUTO at once.
- A group on AUTO gets one go at each enemy torpedo inside its arc and 600 units. Each go has a
  70% chance to shoot the torpedo down, and spends 100 rounds.
- A group with fewer than 100 rounds left drops off AUTO.
- PDCs need their autoloaders on. They also need what every weapon needs: at least 2 WEP units,
  the magazine at integrity 3 or more, and weapons released (§6.1). The LOADERS button is not drawn
  yet. The autoloaders are on at load.
- Each group on AUTO shows its arc on the plot as a dashed sector.

> **Rule 8.6.a** PDCs are the only defence against a torpedo. Nothing soaks the damage.

### 8.7 Arc validity

```
in arc = |angDiff(solution's bearing from the bow, mount's arc centre)| <= mount's half-arc
```

> **Rule 8.7.a** Arc centres are **fixed to the hull**. Turning the ship turns every arc.

> **Rule 8.7.b** Having nothing you can shoot at is a normal state. It means turn the ship.

> **Rule 8.7.c** ANGLE, the second rocker of the arming ladder, throws only when the solution's
> bearing is inside the mount's arc. Range is not checked. A shot past the mount's reach runs out
> and counts as a miss.

---

## 9 · Sensing

**What it is.** How you find things. DRADIS listens. The sweep turns round our ship and updates
what DRADIS hears. The active scan identifies a ship.

**How it plays.** You never quite have a live picture. A contact you only hear is a short red bar on
the DRADIS circle. It gives the bearing only. To find its range, plot it with POINT and RULER, or scan
it. An active scan names it and lets you target it, but makes you louder.

**How it's built.** DRADIS hears a contact by band and range. Each pass of the sweep updates where
each contact was last seen. The ACTIVE SCAN cone also updates a scanned ship, on every frame. With
ACTIVE on, a contact inside the scan cone is identified as the sweep passes it. The numbers are in
each ship's stats file, under sensors.

### 9.1 DRADIS and the sweep

DRADIS hears a contact when all four are true:

- DRADIS is on.
- The SEN bus has at least 1 unit (Step 82, owner: no power, no hearing).
- The contact gives off the band you have picked: THERM, RAD, UV or EM. The Garuda gives off THERM
  and RAD.
- It is within range.

```
range = 4,375 units + 625 units × gain        (gain 1 to 5)
UV hears to 0.55 of that range
```

You set the gain on the Sensors drawer. It can go up to the SEN units, never below 1. It loads at 1.

> **Rule 9.1.a** The sweep turns once every `5.4 − 0.5 × gain` game seconds: 4.9 s at gain 1,
> 2.9 s at gain 5. It stops while paused, while DRADIS is off, and while the SEN bus is empty.

> **Rule 9.1.b** Between passes, a contact out of sight stays where the sweep last touched it. A
> locked ship and a scanned ship under the ACTIVE SCAN cone move smoothly instead (§9.6). A heard
> contact's bar turns smoothly too (Rule 9.1.c).

> **Rule 9.1.c** A heard contact that is not identified shows as a short red bar, curved along the
> DRADIS circle, in its direction. Its label, BRG and the angle (for example BRG 053), is just
> outside the bar. The bar first shows at the sweep's first touch. After that it turns in real time:
> every frame it follows the contact's bearing as either ship moves, and the BRG number changes with
> it. It gives no range. It covers the same number of degrees of the circle at every zoom, 6 to start
> with. When the DRADIS circle is off the plot, so is the bar. Nothing is left behind when the sweep
> passes (owner, 2026-11-02).

While DRADIS is on and SEN has power, the plot shows its hearing range as a circle round our ship,
and the sweep. The buses are empty at load, so there is no sweep until SEN has power. The DRADIS
switch stays where the player set it.

### 9.2 How things are drawn as you zoom

There is one plot. You zoom it in and out.

> **Rule 9.2.a** Everything is drawn at its **true position**. Distance on screen always matches
> real distance, at whatever zoom you are on.

> **Rule 9.2.b** Ship pictures **shrink as you zoom out, but only down to a minimum size.** Once a
> picture reaches that minimum it stops shrinking, so a distant ship is still visible. The minimum
> is **tunable**.

> **Rule 9.2.c** Text never shrinks. Labels keep the same size at every zoom level.

### 9.3 In sight

A ship within 2,000 units is in sight. Its model shows on the plot, seen from above, and glides
(§9.6). Seeing a ship does not identify it. Its name stays BOGEY until an active scan.

### 9.4 Active scan

ACTIVE puts out a cone from our ship. You set its bearing with the cone dial. NARROW and WIDEN set
its half-width, from 15° to 90° in 10° steps. It starts at 40°. It reaches the full DRADIS range,
in every band.

- With ACTIVE on, the sweep identifies each contact inside the cone as it passes. Every pass over a
  contact in the cone plays a beep (owner, 2026-10-25).
- Letting go of the cone dial also identifies every contact inside the cone.
- Clicking a row on the contact board identifies that contact, if the cone covers it and DRADIS
  hears it.
- ACTIVE adds 35 to our signal while it is on.
- The plot shows the cone: two bright edges and a faint fill.

> **Rule 9.4.a** A scan identifies a ship and reads its course and speed at that moment (owner,
> 2026-11-01). An identified ship shows its name. You can fire torpedoes at it, board it, and read
> what your hits do to it (§20, §13.5). A lock takes the scan's course and speed for its shot
> (§18.4).

> **Rule 9.4.b** Identification is kept. It never resets. The course and speed go out of date if the
> ship turns, so you scan again. Each pass of the sweep over a ship in the cone is a new scan.

### 9.5 Contact names

> **Rule 9.5.a** The name **is** the knowledge state. One rule gives it, and every part of the board
> shows the same name.

```
identified by an active scan  → GARUDA
not identified                → BOGEY T07
destroyed                     → no label on the plot; it stays as a wreck
```

### 9.6 What the plot shows of a ship

**Owner, 2026-10-26.** How a ship shows on the plot depends on how you know it.

- **Our ship** glides smoothly across the plot.
- **Heard, not identified:** a short red bar, curved along the DRADIS circle, in its direction, with
  its BRG label just outside. It appears when the sweep first touches the contact. Then it turns in
  real time as either ship moves (owner, 2026-11-02). While the periscope is on the contact, the bar
  is full red and its label is amber. Otherwise both are dimmer.
- **Identified, outside visual range:** a dot where the sweep last touched it, labelled with the
  range at that moment. The dot hops when the sweep passes again. While the ACTIVE SCAN cone covers
  it, the dot moves smoothly and shows where the ship is now (owner, 2026-11-01). With ACTIVE SCAN
  off, it stops there and hops again.
- **Inside visual range:** the ship's own model, seen from above, gliding. It is red once
  identified. Until then it is amber and labelled BOGEY with its track number, and you cannot
  target it.
- **Locked:** the ship is drawn smoothly, where it is now (owner, 2026-11-01).
- **Destroyed:** inside visual range its model turns grey, breaks in two where the shot struck, and
  the halves drift apart (Step 131). Outside visual range it is a dim dot, which fades away over 120
  game seconds. Back in visual range it shows again.
- **Shots, torpedoes and fighters** always slide across the plot (owner, 2026-11-01). Just after
  firing, a shot's dot can go up to twice its true speed. It doesn't slow for a hit: it goes on at
  full speed to the hit and stops there, and its trail runs on into it.

> **Rule 9.6.a** The rules step once per board tick. Between ticks, a ship drawn smoothly moves on
> along its velocity, up to one tick ahead. Rules use the position from the last tick.
> Shots and torpedoes slide across the plot toward where they will be at the next tick. A dot can
> reach a ship up to one tick before the hit lands. Fighters slide up to one tick behind (owner,
> 2026-11-01), on a smooth curve (owner, 2026-11-02).

> **Rule 9.6.b** Seeing a ship does not identify it. Only identification gives its name and lets you
> target it.

> **Rule 9.6.c** A wreck stays where it is in space, and shows again when it is back in sight.

> **Rule 9.6.d** The numbers on the board (Step 130): visual range 2,000 units. DRADIS reaches 5,000
> units at gain 1 and 7,500 units at gain 5. Railguns reach 2,000 units, torpedoes 3,000 and PDCs 600.
> Since Step 170 our spinal railgun reaches 30,000.

---

## 10 · Comms

**What it is.** Two puzzles in a row: tune in a signal, then break its cipher.

**How it plays.** You find the carrier signal on the right band and frequency, and lock it. The
message comes through enciphered. You turn four dials until it reads in plain English.

**How it's built.** The radio has a band, a frequency, a lock and four cipher dials. It is the same
in every level. It starts quiet: NO TRAFFIC.

**Tuning.** Band 1 to 3. Frequency 1 to 100, in steps of 0.1. Each message has its own band and
frequency. LOCK takes only within 0.3 of the signal. Changing the band or the frequency drops the
lock.

The receiver scope shows the band. Its bars peak near the signal. The signal lamp turns amber
within 12 of the signal, and green inside the lock window.

> **Rule 10.a** Tuning is by eye. The scope and the signal lamp show how close you are.

**Cipher.** A Vigenère cipher with a four-letter key. Each message has a new key. Four dials, A to Z.
Click a dial to step it forward; Shift-click steps it back. The dials turn only while the signal is
locked.

The status line reads INCOMING SIGNAL, then DECIPHERING once locked, then MESSAGE READ when the
dials match the key.

> **Rule 10.b** The old fixed signal is gone from play (owner 2026-11-02). Every level uses real
> signals. A level whose Garuda sends none shows NO TRAFFIC.

**Real messages (Steps 137, 140 and 148).** Signal Seconds on a level's Enemy Spawn sets how often its
ship sends one. INTERCEPT and Wolf Hunt use 60 seconds. At 0 the ship sends none.

The ship sends a message every so often. Each one is written from where the ship really is, all in
words, so every part of it is in code:

`GARUDA HEADING SOUTHWEST SPEED TWENTY BEARING NORTH`

It is an intercepted message, so it doesn't speak to us. It gives which way the sender is heading,
its speed to the nearest 5, and its bearing from our ship. Directions are one of eight words. The
bearing is always a direction word such as NORTHWEST, never a number. A ship with no patrol route
takes a new course halfway to its next message, so the heading keeps changing. A ship on a patrol
route flies the route and may turn back at a point (Rule 5.3.b). A Garuda that has heard us stops
doing this, and hunts us instead. Each message comes in on a new band and frequency, in a new code,
and the dials go back to A. A soft tone plays. The alarm strip says INCOMING SIGNAL with the seconds
left to lock it, as the last line on the strip. Comms hears a ship from a very large distance, so
anywhere on the 20,000 map for now. Rocks do not touch comms (Rule 5.1.e).

You find the peak, tune, lock, and turn the four dials until the message starts with the sender's
name. Once it reads, it stays on the screen until the next one comes in. You have 60 seconds to lock
it. 30 to 60 is good, and it is set as On Air Seconds on each level's Enemy Spawn. If it isn't locked
in time, it is lost: SIGNAL LOST. Only one message is on the air at a time. The wait for the next
starts when the last is read or lost.

> **Rule 10.c** Comms draws nothing on the plot: no bearing line, no quadrants, no markings on the grid
> (owner 2026-10-29). The player works out where the ship is from the words.

> **Rule 10.d** A message is guidance (owner 2026-11-02). It gives the player a reason to move in a
> direction and think. It doesn't have to be a real decision by the enemy ship in the story.

---

## 11 · The board

**What it is.** The whole screen: a metal frame with one big plot in the middle. Two rails of keys,
six drawers, the helm, the orders station and the periscope are set into the frame.

**How it plays.** You look at one picture the whole game. Drawers slide out over the edges of it,
but the battle space is always there. Everything you do reads back on the plot.

**How it's built.** A fixed 1920 × 1080 layout from the Claude Design handoff. Each panel is a
prefab. The prefabs, their art and their layout are described in `docs/BOARD_UI.md`.

### 11.1 Layout

Design resolution 1920 × 1080.

- The **metal frame** is the front layer. Drawers slide out from behind it (owner, 2026-10-25).
- The **plot** fills the middle.
- The **tool rail** has the plot tools (§18).
- The **systems rail** has one key per drawer: FIRE, SENS, PWR, HGR, ENGR and COMM (§19).
- The **helm** and the **orders station** open and shut with a latch.
- The **periscope** lowers into the board and goes back up.
- The **time control** has the pause key, the three rate keys and the mission clock. The briefing
  icon is next to it.
- The **console** has a power lamp, the SCALE knob and the grid tint knob.
- The **alarm strip** shows one line, only when there is one. First a battle space warning, then
  what just hit us, then ROCK AHEAD, then enemy fire in flight (INCOMING TORPEDOES), then the mission's
  line (REINFORCEMENTS INBOUND, JUMP POINT), then a new contact, then INCOMING SIGNAL.
- The **signal box**, in the frame's top-right, shows our signal.

> **Rule 11.1.a** The plot is the subject. Everything else comes second.

> **Rule 11.1.b** No cockpit, no outside camera. Drawers open over the plot; they never replace it.

### 11.2 Plot

The plot is a grid. Its tint is white or amber, set with the console's tint knob (WHT or R-O).

- Scale: 760 px per 1,000 units at zoom 1.
- Zoom: drag the SCALE knob (×10 per 160 px) or turn the mouse wheel (×1.12 per notch). Zoom is
  rounded to 0.01. Zoom 1 is the closest. Zooming out stops at x0.058, where a 20,000-unit map fits
  the plot's height (Step 130).
- The plot follows our ship as it moves (owner, 2026-10-29). Pan it, by dragging the grid or with
  the arrow keys, and it stops following. `C` centres it on our ship and follows again.
- The numbers on the top and left edges measure from our ship: 0 level with it, then every 100
  units at x1, further apart as you zoom out (Step 135). They move with the ship (owner, 2026-10-26).

> **Rule 11.2.a** Text keeps its size at every zoom. Ship pictures shrink with zoom, down to a
> minimum size.

> **Rule 11.2.b** No standing text on the plot (owner, 2026-10-18). Our ship and the DRADIS circle
> have no labels. Readouts come from using a tool.

### 11.3 Rails

The tool rail: CURSOR, POINT, RULER, COMPASS and ERASE (§18). The systems rail: one key per drawer
(§19). Each key has a lamp.

> **Rule 11.3.a** The two rails are exactly the same size and spacing. They must stay that way.

> **Rule 11.3.b** A lit lamp means the tool is in use, or the drawer is out.

### 11.4 What the plot draws

> **Rule 11.4.a** **Colour says whose side it is on.** White is ours. Red is an identified enemy.
> Amber is a ship not yet identified. Grey is a wreck.

| Thing | How it shows |
|---|---|
| Our ship | Its 3D model seen from above, in white. It glides and never blinks (owner, 2026-10-25). |
| A contact | A red bar on the DRADIS circle, a dot with its range, or its model (§9.6) |
| Our fighters | Small white triangles in a V, 10 px, nose pointing the way they fly, with no labels. The V is shut where they appear beside our ship. It opens as they fly out and closes as they land (owner, 2026-11-02). Zoomed in, each is a small 3D model, 10 units long, and its triangle fades out. The models' V spreads so they never overlap (owner, 2026-11-11, Step 199) |
| Our slugs and torpedoes | A white dot with a fading trail: 120 px on a slug, less on anything slower, so a torpedo's grows as its motor lights |
| Enemy torpedoes | A red dot with a short trail |
| A ship our marines secured | A dot |
| DRADIS | The hearing circle and the sweep; with ACTIVE on, the scan cone |
| Visual range | A thin ring round our ship, always on. Inside it you see ships with your eyes (§9.3) |
| Rocks | Grey line art within visual range. Past it, a blinking grey hollow circle where ACTIVE SCAN pinged one (Rule 5.1.f) |
| Fire control | The selected mount's reach as a dashed ring while the drawer is out; each PDC group on AUTO as a dashed sector |
| Marks | POINT, RULER and COMPASS (§18) |
| Jump point | Once the target is down: a green ring, the size of the distance that counts as there, with JUMP POINT beside it (§15.2) |

> **Rule 11.4.b** Clicking a contact's label or a bearing label on the plot does nothing (owner,
> 2026-10-29). The periscope picks the target (§20).

### 11.5 Colour

The board's colours are in one file, `Assets/UI/Board/Palette.asset`.

| Colour | Hex | Means |
|---|---|---|
| Green | `#8FC33A` | Working, on, ready |
| Amber | `#E0A020` | Needs attention |
| Bright amber | `#FFC44D` | About to become a problem |
| Red | `#E04B3C` | Failed, refused, hostile |
| Blue | `#4A9FD8` | Our own forces on the panels: fighters out, the damage control party |
| Bright green | `#B6F04A` | Screen glow, a thrown switch |
| White | `#E8EDF0` | Our ship, fighters and slugs on the plot; readouts |

> **Rule 11.5.a** Colour is a code. A new colour goes into the palette file first.

### 11.6 Periscope

The periscope looks along one bearing. Lower it to use it. Drag across it to slew it. Turn MAG to
zoom, from 1× to 8×. Press SLEW TO to swing it onto a hand-dialled or plotted solution's bearing,
or else onto the target. It stays down until you raise it (owner, 2026-10-29). The CLASS key was
removed in Step 125.

The periscope picks the target. TRACK on a scanned enemy under the periscope, within 4°, makes it
fire control's target (§20).

Its bearing matters to fire control. BEARING, the third rocker of the arming ladder, needs the
periscope within 4° of the target (§20).

### 11.7 Typography

**Barlow Condensed** (400/500/600) — titles, names, labels, designators.
**Share Tech Mono** — *all* numerics, bearings, ranges, timers, ticks.

> **Rule 11.7.a** Labels uppercase. No italics. Units set at the same size as their number.
> **Numerics never animate their digits — they snap.**

### 11.8 Time control and console

Pause key · three rate keys (×0.25, ×1, ×5) · mission clock · power lamp · SCALE knob · grid
tint knob.

> **Rule 11.8.c** There are no time-nudge keys. An earlier draft drew `−10s / −1s / +1s / +10s`;
> they contradict Rule 2.a and are removed.

> **Rule 11.8.a** No EXECUTE. No EMERGENCY ABORT.

> **Rule 11.8.b** Hazard striping is kept for actions you cannot undo, and is currently unused. It
> may not be used for decoration.

---

## 12 · Hangar

`DIVERGES` — the prototype fires boarding pods like a weapon. Starwolf drops those and does
everything through the hangar instead.

**What it is.** Where you launch interceptors and assault boats. Our ship carries assault boats
only (Step 170).

**How it plays.** You give a squadron a job and launch it. It flies out to fire control's target
and gets on with it. Boarding comes last: fight a ship down first, then send the marines.

**How it's built.** Two squadrons. A launch runs six stages in order, then the launch lever. Craft
fly at a fixed speed and turn gradually onto their goal. The numbers are in each ship's stats file,
under hangar.

> **Rule 12.a** Nothing in the hangar is flown by hand. Pick the squadron and its order, then
> launch. The squadron goes for fire control's target.

Every fighter and assault boat starts the game at 100% and READY, on our ship and on every enemy
ship (owner, 2026-11-02). Each craft has one bar on the Hangar panel. The bar shows one number. The
code calls it fuel, and the owner calls it health.

Craft will gain damage from PDCs and ships firing. This is still to be built. Nothing damages a
fighter yet, and nothing in the game changes these bars yet.

### 12.1 Interceptors

Six starfighters. Their orders are DESTROY, DISABLE and ESCORT. Only craft marked READY launch. The
Hangar panel reads 6 OF 6 READY, and a launch sends all six.

> **Rule 12.1.a** Our ship carries no fighters (owner, 2026-11-08: "an assault destroyer should have
> boarding capability… But it shouldn't have fighter jets it can deploy"; Step 170). Each ship's
> stats file has a number, Fighters (0 to 6, default 6), under Hangar. The Asura's is 0. The
> Garuda's is 6. At load the ship keeps only that many. Nothing is deleted. With none, the hangar
> starts on the assault boats, and the INTERCEPTOR row on the Hangar panel shows nothing.

- They fly at 100 units/s.
- Within 200 units of the target, every fighter makes an attack run every 30 game seconds.
- Each run takes 10 armour off the side facing the fighter. Once that side is open, each run takes
  1 integrity off the room behind it.
- Each fighter does its own run. With all six out, one attack run takes 60 armour off a side.
- DISABLE always attacks the stern, and so the engine room.
- ESCORT stays with our ship.
- Fighters scout. Any ship or ruin within visual range of a flight shows on our plot while they are
  near (Step 101).
- Enemy ships hear our fighters, and go to look (§5.3).

### 12.2 Assault boats — marines

Four boats. The Hangar panel reads 4 OF 4 READY, and a launch sends all four. Our ship has 18
marines aboard. The four boats share them, and a boat takes at most 6. The boats' orders are
DESTROY, DISABLE and SECURE.

| Order | If the marines win |
|---|---|
| **DESTROY** | The ship is scuttled. Its reactor goes to 0. |
| **DISABLE** | Its engine room and magazine go to 0. |
| **SECURE** | The ship is captured. It stops moving and is no longer hostile. |

- The boats fly at 75 units/s and dock within 50 units of the target.
- They wait until the target has an open side and is slower than 0.5 units/s (owner: fight it down
  first). The hangar reads HOLDING OFF, and why.
- Aboard, the marines fight for 60 game seconds. The boats stay with the ship while it moves and
  fights (owner, 2026-10-27).
- The defenders are a quarter of the target's crew, rounded up. The marines win with odds of
  `marines ÷ (marines + defenders)`. A win kills the defenders and costs half a marine per
  defender. A loss kills every marine aboard, and half as many of the enemy crew.
- The boats go no further than 4,000 units from our ship (Step 130). Outbound past that,
  they turn back. On the way back, out of range, a 120 game-second clock runs, with a warning. Close
  in to collect them. If the clock runs out, the boats and their marines are lost.

> **Rule 12.2.c** **There are no boarding pods.** Boarding runs through the hangar. A boat launches,
> flies to the target on its own, and its marines do the work.

> **Rule 12.2.d** Boarding needs a scan (owner, 2026-11-01): the boats won't launch at a ship you
> haven't scanned. The hangar says TARGET NOT SCANNED. It also needs an open side and a slow target.

#### Body cam — not built yet

An idea from the owner (2026-10-29). Don't build it yet.

While the marines fight aboard, a small window on the board shows one marine's helmet camera. You
watch the minute of fighting: a corridor, gunfire, a door blown open. The picture has static and
flicker, with the marine's name and heart rate on it.

The feed follows the real fight. When a marine dies, that feed cuts to static and the window
switches to another marine. A win ends on the order being carried out. A loss ends in static. Today
the fight is decided once, at the end of its 60 seconds (`HangarSystem`). For feeds to drop out one
by one, the losses would have to come during the minute.

Three ways to fake the picture, least work first:

1. **Short video clips** played in the window, with static, name and heart rate on top. The
   suggested start.
2. **Still pictures** shaken and cut between, with muzzle flashes and static. Less art, looks cheaper.
3. **A small 3D corridor** with a camera walking through it, matched to the fight. The most work.
   Move to this if the clips feel repetitive.

Open questions for the owner:

- Whose marines: ours aboard the enemy, the enemy's aboard our ship (not built), or both.
- Where the window goes: over the plot, in the hangar drawer, or its own small screen.
- Where the clips come from.

### 12.3 Launching

Six stages, each a rocker: CREW, FUEL, ORDNANCE, EVAC, DOORS and CATAPULT. Each runs to 100% over
4 to 9 game seconds once thrown. Each needs the one before it at 100%. Releasing a stage releases
every stage after it.

| Stage | Refused when |
|---|---|
| CREW | Flight operations are not released (CONDITION 3, §6.1) |
| FUEL | The hangar bay is below integrity 3 |
| ORDNANCE | The magazine is below integrity 3 |
| DOORS | The bay doors are shut |
| CATAPULT | There are no AUX units |

Then pull the launch lever. Nothing launches if flight operations are not released, the bay is
below integrity 3, no craft is ready, or there are no marines for the boats.

The small number box on the Hangar panel can't stop a launch or change where the squadron goes. The
box and its red number are still on the panel and do nothing else (owner, 2026-11-02).

A squadron, fighters or assault boats, then flies like this (owner, 2026-11-02):

1. It appears just beside our ship, on the side facing fire control's target. With no target, or
   a target with no position, it appears on the starboard side. The leader starts past the widest
   part of our ship as the plot draws it (the wings), plus half a fighter, plus 10 units. Zoomed
   out, the plot draws our ship bigger than its true size. The start moves out with the drawn ship.
   It is never closer than the side of the ship's box.
2. It flies straight out, tilted 15° toward the stern, for 200 units, at every zoom. Only an ESCORT
   can fly further: if the small circle round our ship is bigger at the current zoom, the ESCORT
   flies out past that circle. A squadron always flies its full distance, even after RECALL or with
   its target in reach.
3. It turns onto where it is going. The turn builds up, reaches at most 90° a game second, and
   eases off as it lines up.
4. It lands, docks or attacks as soon as it is within reach, at every game speed.

Owner, 2026-11-02: it leaves from the hangar dot on that side, level with the dot along the ship. If
a line is drawn out of that dot on the Hard Points tab of Starwolf → Tuning, it flies the line's
shape in place of step 2.

All its flying uses these turns, on the way back too. RECALL turns it back. It lands when it
reaches the small circle round our ship.

On the plot, the V is shut where the squadron appears: all its craft start on one spot. It opens
as the squadron flies out and closes as it lands. No craft is drawn over our ship. The plot draws a
smooth curve between ticks. The rules still step on the tick (Rule 9.6.a).

The numbers are in each ship's stats file, under hangar: `launchSideGapUnits` 10,
`launchTiltBackDeg` 15, `launchRunUnits` 200, `turnDegPerS` 90 and `turnEase` 180. Boats use the
same numbers.

---

## 13 · Damage

**What it is.** Four armoured sides and six compartments per ship. No shields. Every ship uses the
same model, ours too.

**How it plays.** The side you present is your armour, so which way you face is a defensive choice
as well as a firing one. Once a side is open, hits reach the room behind it. Rooms catch fire, lose
their air and leak radiation.

**How it's built.** Four armour values and six rooms. Each room has integrity 0 to 5, crew and air.
A hit strikes the side facing where it came from. The numbers are in each ship's stats file, under
damage. The full rules are in `docs/DAMAGE_README.md`.

### 13.1 No shields

> **Rule 13.1.a** There are no shields. Armour is the only buffer. Auxiliary (§6) is the recovery
> resource.

### 13.2 Four sides

| Side | Armour | Room behind it |
|---|---|---|
| PROW | 100 | MAG, the magazine |
| STBD | 100 | HGR, the hangar bay |
| AFT | 50 | ENG, the engine room |
| PORT | 100 | RCT, the reactor |

These are our ship's. The Garuda's armour is 150, 150, 75 and 150. A Naga frigate's is 50, 50, 25
and 50. Each ship's armour is in its stats file, under damage.

The bridge (BRI) and habitation (HAB) are inboard. No hit reaches them directly.

```
side hit = the side facing the direction the shot came from
```

> **Rule 13.2.a** **The side you present is your armour.** Facing is the defensive decision, and the
> reason the autopilot has a facing setting (§4.6).

> **Rule 13.2.b** **The aft is the weak side.** It has half the armour, and the engine room is
> behind it. Turning to run puts your thinnest plate over your engines.

> **Rule 13.2.c** The reactor is behind the port side. Reactor integrity 0 destroys the ship
> (§13.4).

> **Rule 13.2.d** At 0 armour a side is **open**. Hits through it damage the room behind (§8.1).

### 13.3 Compartments

Six rooms per ship, 52 crew in all. The crew in each room is a number in each ship's stats file,
under damage (owner, 2026-10-29). The table gives the starting numbers, the same for every ship.

| Room | Crew | What damage does |
|---|---|---|
| **MAG**, magazine | 4 | Below 3: no weapon can load, PDCs stop, and launches get no ordnance |
| **BRI**, bridge | 9 | Below 3: TRACK cannot compute a solution |
| **HAB**, habitation | 18 | Nothing yet, apart from crew lost |
| **HGR**, hangar bay | 7 | Below 3: no launches, and no fuel for them |
| **RCT**, reactor | 6 | Every point lost cuts the reactor's cap (§3.2). At 0 the ship is destroyed |
| **ENG**, engine room | 8 | Every point lost slows the bow's turn and cuts the throttle cap (§4) |

#### Fire, vacuum and radiation

Every rate here is per game second.

- **Fire.** An HE hit through an open side has a 60% chance to start a fire; a PENETRATOR, 40%. A
  fire takes 1 integrity off its room every 30 game seconds. Each second it has a 1% chance to
  spread into each neighbouring room that has air. A vented room cannot burn.
- **Vacuum.** A slug through an open side has a 50% chance to breach the room. A breached room
  loses its air and half its crew. While its doors are open, the vacuum has a 1% chance each second
  to spread into each neighbour.
- **Radiation.** A hit on the reactor room, except a fighter's, has a 50% chance to leak radiation.
  Radiation kills one crew member in its room every 20 game seconds. While the reactor leaks, it
  has a 1% chance each second to spread into each neighbour. The leak stops once the reactor is
  repaired to 3 or vented. Then each room's radiation fades after 90 game seconds.

A room already at 0 takes no more integrity, but a hit can still start a fire, a breach or a leak
there.

### 13.4 How a ship is destroyed

- Its reactor room's integrity reaches 0. This is true of every ship, ours too (owner,
  2026-10-24).
- Marines scuttle it (§12.2).
- Our ship only: it stays outside the battle space for 30 game seconds (§14).

### 13.5 What shows

> **Rule 13.5.a** Damage shows on Engineering's ship picture (owner, 2026-10-26). Our ship is drawn
> in four parts, one per side. Each part is white while its armour is full, amber once hit, and red
> once open. Each room's ring is white, amber or red by its integrity. The plot ship stays plain
> white.

> **Rule 13.5.b** A hit report names the side and what the hit did, once the target is identified:
> `HIT · GARUDA · STBD ARMOUR 50%`. Before that it reads `HIT · BOGEY T07`.

### 13.6 Damage control

Engineering acts on one selected room. REPAIR, SEAL and VENT cost AUX charge, the same as at the
orders station: 30, 25 and 15 (owner, 2026-10-29). REBOOT and RECALL are free. With too little
charge, the rocker is refused and engineering's note reads `INSUFFICIENT AUX — 30% REQUIRED`. The
room's own reason, from the table below, shows first.

| Rocker | What it does | Refused when |
|---|---|---|
| REBOOT | Adds 1 integrity | The room is at full integrity |
| REPAIR | Sends the party. After 6 game seconds it puts out a fire; or, in a sealed airless room, brings the air back; or adds 1 integrity (2 at CONDITION 1) | A party is already out; the room is airless with its doors open; or the room is at full integrity with air and no fire |
| SEAL | Shuts an airless room's doors | The room has air, or its doors are already shut |
| VENT | Dumps the room's air. Any fire goes out. The crew and any party in the room die | The room is already vented |
| RECALL | Calls the party back | No party is out |

The orders station gives PARTY, SEAL and VENT for the same AUX charge, under the same rules (§6).

An enemy ship with its magazine below integrity 3 can't fire, and sends its party there by itself,
at no AUX cost (owner, 2026-11-01). The party works as ours does. A fire comes first, and a hit on the
room can kill the party, so the magazine may not come back.

---

## 14 · Failure

**What it is.** How our ship is lost.

**How it plays.** Losing comes down to the reactor. Flying out of the fight loses the ship too.

**How it's built.** Two checks run each board step. Either one ends it.

| Condition | Trigger |
|---|---|
| **Destroyed** | Our reactor room's integrity reaches 0 |
| **Left the battle space** | Our ship stays outside the battle space for 30 game seconds. The alarm strip reads SHIP LOST — LEFT THE BATTLE SPACE |

> **Rule 14.a** Within 1,000 units of the edge, the alarm strip reads APPROACHING THE EDGE OF THE
> BATTLE SPACE. Outside, it reads EXITING THE BATTLE SPACE — TURN BACK and counts down the seconds
> left. Crossing out drops the rate to x1 and plays the alert tone.

> **Rule 14.b** A lost ship goes dark. Its power is out for good. The end screen reads SHIP LOST
> (§15.2).

> **Rule 14.c** Only our ship can be lost at the edge. Enemy ships never leave the battle space
> (Rule 5.3.c).

---

## 15 · Mission

**Built so far:** eight levels on the MISSIONS list (§15.5). The first six are short, with one goal
each. The seventh is the full mission, Wolf Hunt: the hunt, the win, the loss and the jump point
(§15.2, Steps 85 to 99), and the level's ruins (§15.3). The eighth, LONG SHOT, is a test range for
the spinal railgun. The rest of this section is the full design,
not built yet.

**What it is.** One handcrafted region of space, one Traitor Lord's flagship hidden in it, one way
out.

### 15.1 How a mission runs

`Title → mission card → pre-launch sequence → hyperspace → level → escape → credits`

The player picks a Traitor from a card interface, enters an arrow-key sequence to spin up the
hyperdrive, and drops into the level at one edge of the map.

### 15.2 How you win

Three stages, run in the background with no popups and no quest journal:

| Stage | Starts when | Ends when |
|---|---|---|
| **Locate target** | You enter the level | You scan or otherwise confirm the flagship |
| **Eliminate target** | The flagship is confirmed on DRADIS | It is destroyed |
| **Exit region** | The flagship dies | You reach the jump point and jump out |

> **Rule 15.2.a** **Killing the flagship is not winning.** When it dies, an extraction marker appears
> at a far edge of the map. **The mission ends only when you reach it and jump out.**

> **Rule 15.2.b** The escape is played under whatever heat you generated getting the kill. Anything
> still alive is still hunting you.

> **Rule 15.2.c** There are no cutscenes. You reach the edge and disappear.

**Built now: Wolf Hunt (Steps 85 and 98, owner 2026-10-27).** Rule 15.2.a is built. The FTUE
plan's "ends at the kill" (`FTUE_PLAN.md` §1) is replaced.

- Locate target ends when our sensors identify the Garuda.
- When the Garuda is destroyed, or our marines take it (SECURE), a jump point opens. It is in the
  middle of the battle space's edge farthest from our ship, a little way in. The alarm strip gives its
  bearing and distance, and the alert tone plays. The plot shows it as a green ring (§11.4).
- You win when our ship reaches the jump point: MISSION COMPLETE, `GARUDA DESTROYED · JUMPED OUT`.
- You lose when our reactor reaches 0, or when our ship stays outside the battle space until its
  countdown ends.
- The reinforcements keep coming while you run for it (§5.4).
- When the mission ends, the game pauses. 2 s later the end screen covers the board: MISSION
  COMPLETE or SHIP LOST, the reason, then the target, our ship and the time taken.
- RESTART MISSION plays the level again, from the board. MISSION SELECT opens the MISSIONS list
  (`01_Briefing`).
- The code is `QuestSystem` (on `World`) and `EndScreenPanel` (`End Screen.prefab`).

### 15.3 The level

> **Rule 15.3.a** **There is no true deep space.** Every level is full of structure — the rings of a
> shattered moon, a dead fleet's wreckage, a broken starbase. These are functional: they change
> DRADIS returns, scan noise and where you can move.

Built so far: each level is its own scene, loaded on top of the board. `Level_01` has a few small
groups of debris and three gas clouds; its starbase husk was taken out in Step 107. Our ship starts
at the bottom facing north. The Garuda starts at a random point in its spawn box at the top, facing
a random way, every game (Step 95), about 16,000 units away. The ruins do not stop shots. Rocks cut DRADIS and scans, and block eyes (Rule 5.1.e).
A ship that flies into one bounces and takes damage (§4.5).

You arrive with **old, unverified Imperial records** marking a few landmarks — a listening post, an
abandoned dock, a comms tower. They must be scanned or approached to confirm.

> **Rule 15.3.b** The first phase of every mission is **exploration**, not combat. There is no fuel
> mechanic; you are free to wander.

> **Rule 15.3.c** The flagship is hidden and protected by forces and misdirection. Finding out
> *where it is and how it is guarded* is the puzzle.

### 15.4 Loadouts

**Not built yet.** These are the prototype's loadouts, in its own numbers. The board's sweep time
and weapons are different (§8, §9.1).

| Loadout | railgun | torpedo | hull/side | sensors | DRADIS sweep | aux start |
|---|---|---|---|---|---|---|
| STANDARD KILL | 3 | 4 | +0 | +0 | 3.0 s | 0 |
| GHOST KILL | 5 | 2 | +0 | +1 | 2.4 s | 20 |
| BRAWLER | 2 | 5 | +12 | +0 | 3.4 s | 10 |

### 15.5 The levels

LEVELS opens the MISSIONS list. All eight levels are open from the start. A click, or the arrow keys
and Enter, picks one. INITIALISING... shows, then the board loads that level. NEW GAME's sector map
has the first seven only.

Every level is 20,000 × 20,000 units, except LONG SHOT, which is 40,000 × 40,000. Each has a Mission
box with its goal and its briefing, which opens by itself and comes back with the briefing icon.
Every level but Wolf Hunt ends the moment the goal is won or lost. Only COLD RUN, INTERCEPT and Wolf Hunt have a jump point. In every other level the Garuda does not hunt us,
does not listen for us and does not fire. Any level is also lost as §14 says.

| # | Level | Scene | You win when | You lose when | Where the Garuda is |
|---|---|---|---|---|---|
| 1 | FLIGHT DRILL | `FlightDrill` | All 14 steps are done, in order | §14 only | Parked at the far corner, (−9,000, 9,000), still and silent, out of DRADIS reach of the course |
| 2 | COLD RUN | `ColdRun` | We reach the jump point, shown from the start. Our weapons are off | §14 only | At the far end of an S-shaped corridor, patrolling it at 22 units/s. It hunts us if it finds us |
| 3 | WEAPONS FREE | `FirstShot` | It is destroyed or captured | §14 only | 1,500 units away, still, showing its reactor side |
| 4 | NO PEEKING | `SilentShot` | A shot hits it | It is scanned, or a lock comes on | 4,000 units away, moving at 8 units/s |
| 5 | FINDERS KEEPERS | `Prize` | It is captured | It is destroyed | 1,500 units away, still |
| 6 | INTERCEPT | `Intercept` | We reach the jump point, shown from the start. Our weapons are off | §14 only | At the far end of COLD RUN's S-shaped corridor, patrolling it and sending radio messages. It hunts us if it finds us |
| 7 | WOLF HUNT | `Level_01` | It is destroyed or captured, and we reach the jump point (§15.2) | §14 only | At the top of the map, hunting us |
| 8 | LONG SHOT | `LongShot` | It is destroyed or captured | §14 only | Patrolling east and west, 9,000 units north of our start, at 22 units/s. It doesn't hunt us |

INTERCEPT and Wolf Hunt send real radio messages. In the other six the radio shows NO TRAFFIC (§10).
The levels' numbers are in `docs/BUILD_LOG.md`, section 2.

**COLD RUN and INTERCEPT** have the mission goal JumpPoint. The jump point shows from the start, and
reaching it wins. The end screen says GARUDA EVADED · JUMPED OUT. Weapons off (`Mission.weaponsOff`) is
only a tutorial restriction. It stops guns, torpedoes, PDCs and flight ops.

**INTERCEPT** is a copy of COLD RUN's map with the radio on. We start at (−7,500, −7,000). The jump
point is at (7,500, 7,500). You sneak past the Garuda to the jump point and use the radio to work out
where it is going. This replaced its first goal of finding the Garuda on DRADIS.
`MissionGoal.FindBySignal` still exists, but no level uses it now.

**FLIGHT DRILL** teaches flying only, with nothing to fight. A short note in the briefing box gives
each step. When a step is done, a soft tone plays and the next note opens. The player:

1. Puts ENG on DRAW, then on STANDBY with 1 to 3 units.
2. Raises the throttle and flies into the CHECKPOINT ring ahead.
3. Turns onto the next ring's bearing and flies into it.
4. Keeps `Q` down for 3 game seconds.
5. Keeps `E` down for 3 game seconds.
6. Zooms the plot out to x0.20 or less.
7. Zooms it back in to x0.80 or more.
8. Takes ENG to 5.
9. Raises the throttle until the speed reaches 45.
10. Flies into the next ring.
11. Puts SEN on DRAW and switches DRADIS on.
12. Switches ACTIVE SCAN on, with DRADIS on.
13. Flies through a belt of 39 rocks to the next ring.
14. Stops inside the last ring, at speed 2 or less.

A checkpoint is the jump point's ring on the plot, labelled CHECKPOINT. Its bearing and distance are on
the alarm strip. The end screen reads MISSION COMPLETE, FLIGHT DRILL PASSED.

## 16 · Prototype defects — do not reproduce

A reference list. These are known bugs in the prototype, written down so a port does not copy them
faithfully.

- SONAR range labels read `1250 / 2500 / 5000` for rings at 2000/4000/6000/8000 — every label is
  0.625× the true value.
- The RADAR compass rose is painted at fixed screen angles under a heading-up projection, so it
  reads ship-relative rather than true.
- A comment says nebulae hide themselves from the RADAR view; the loop pushes them anyway.
- A comment describes last-known-position display for the enemy; no such system exists — once
  discovered, the carrier is drawn at its true position forever, at any range. **§9.6 builds what
  this comment describes.**
- `extractionDiscovered` is set and never read.
- MAP passes a white CRT tint despite a comment asking for amber.
- Neutral ships are decoration — left out of collision and targeting; the briefing's civilian
  objective has no implementation.
- `.barrel.loading` has CSS and render support but is never assigned.
- Reactor-damage cells write inline styles that later class writes cannot clear.
- The nebula is a hard on/off at the circle edge — no falloff, no hysteresis.

---

## 17 · Open items

**Not built yet**

1. **The mission flow** (§15.1). The Traitor card, the pre-launch sequence, hyperspace and credits.
   Wolf Hunt is built (§15.2).
2. **The combat autopilot** (§4.6). TAIL is not defined. TARGET LOST is only proposed.
3. **The disguise** (§5.1, §5.2). The enemy hears our signal (§5.3), but there is no false ID. Rocks
   make interference and block eyes, for the enemy too (Rule 5.1.e). They throw no false dots. Gas clouds are built for our ship (Step 129); enemies don't hide in them yet.
4. **The enemy's mind, what is left.** It never retreats. It does not use engineering, comms or the
   hangar.
5. **Picking a target** (§20). Built: the periscope picks it, with TRACK. Fire control has no
   other way to pick one.
6. **Undrawn controls** (§8). The PDC quadrant keys, LOADERS, the warhead selector and the salvo
   knob are not drawn on Fire Control.
7. **Stability** (§4.4). It is shown, but nothing uses it.
8. **Terrain** (§4.5, §8.2). Ships and shots pass through ruins and debris. Only our ship takes
   damage flying into one.
9. **What comms gives you** (§10). A read message is guidance for the player (Rule 10.d). It changes nothing else.
10. **Reflex numbers** (§7): `E_floor`, `reflexSpeedMax`, `staleTrackTimeout`.

## 18 · Plotting tools and the firing solution

The tool rail has five tools (§11.3). They draw on the plot and change nothing in the world.

**What it is.** CURSOR and four marking tools — POINT, RULER, COMPASS and ERASE — and the firing
solution they help you build.

**How it plays.** You work the target out yourself. Drop POINTs, wait, drop more, and run a
RULER between them to read the target's course and speed. Press TRACK and it takes
the solution from your plot, and the gun and the sight go to where the target should be. Then
fine-tune it by hand, or lock it with the targeting computer (§18.4). A lock lasts 10 seconds, so
line up on where the target will be before you lock.

**How it's built.** Marks are stored in world kilometres. Each POINT has a mission-time stamp.
Exactly one tool is active. A RULER from one POINT to another, placed at different times, is a
target's track. The latest track feeds a hand-dialled solution.

> **Rule 18.a** The tools are **measuring instruments**. They never move the ship, fire anything, or
> change a contact. Marks made with them are the player's own notes.

> **Rule 18.b** Exactly one tool is active at a time. CURSOR is the default. It makes no marks.

> **Rule 18.c** Marks stick to **world positions** (owner, 2026-10-17). Zoom and pan move them with
> everything else.

### 18.1 The three angles you need

These are three different numbers and the player needs all three:

| Angle | What it means | What it is for |
|---|---|---|
| **Bearing** | The compass direction from you to the target. `0°` is up. | Where it is |
| **Relative bearing** | Bearing minus your own heading. Where the target is compared to your nose. | **Whether a turret can reach it** — arcs are measured from the hull (§8.7.a) |
| **Angle on bow** | The angle between the target's own heading and the line from it back to you. | **What aspect it is showing** — coming at you, running away, or crossing |

Angle on bow is the one that tells you how much to lead. A target coming straight at you needs
almost no lead. A target crossing your line of sight needs a lot.

The board shows bearing on COMPASS. Fire control's BRG is a relative bearing. Angle on bow
is not shown yet.

### 18.2 The tools

#### POINT

Click to drop a point. Each point is stamped with the mission time it was placed, and shows that
time.

> **Rule 18.2.a** Points record a timestamp. That is what makes a target's speed measurable — see
> §18.3.

#### RULER

Drag from one spot to another. It shows the distance only, such as `12,400 km` (owner,
2026-10-27).

A RULER end snaps to a POINT nearby. A RULER from one POINT to another, placed at different times,
is a **track**. It shows the target's course and speed instead, such as `CRS 210° · 5.50 km/s`. A
dashed line shows where the target will be 300 game seconds on.

With the spinal railgun picked in Fire Control, a RULER also shows how long its slug takes to fly its
length, such as `18,000 km · SLUG 12 S` (Step 170). A track still shows course and speed.

> **Rule 18.2.b** A track's course runs from the earlier POINT to the later one. Its speed is the
> distance over the time between them.

#### COMPASS

Drag out from our ship. It draws a ring round our ship and shows its distance and bearing, such as
`8,000 km · BRG 045°`. The ring moves with our ship. It is marked like a compass: a tick every 10°,
and a longer tick every 30° with its number, 000 at the top (Step 100).

> **Rule 18.2.c** The compass measures around our ship (owner, 2026-10-17).

#### ERASE

With ERASE on, click a mark to remove it. Press ERASE again to clear every mark and go back to
CURSOR.

### 18.3 How you work out a firing solution

This is the loop the tools exist to serve (owner, 2026-10-23).

**1 · Find the range.** A contact you only hear is a red bar on the DRADIS circle, with its bearing
(§9.1). The bar gives no range. Plot it with POINT and RULER, or scan it.

**2 · Find the course and speed.** Drop a POINT on the contact. Wait. Drop another POINT on
the contact later. Run a RULER from the first POINT to the second. It reads the course and speed
(§18.2).

**3 · Press TRACK.** TRACK needs the bridge (§13.3). It fills the solution from your plot. The
newest POINT is where the target was. The newest POINT-to-POINT RULER gives its course and speed.
TRACK works out where it is now. Fire control's ID reads PLOT. TRACK does nothing while a lock is on.

**4 · Fine-tune it by hand.** Turn a solution knob on Fire Control: RNG (100 units a step), BRG (1° a
step) or SPD (1 unit/s a step). With no solution yet, the first turn starts a hand-dialled solution
at 2,000 units, on the periscope's bearing, at speed 0. The latest track gives it a course and speed.

**5 · Check the arc.** If the solution's bearing is outside the mount's arc, ANGLE will not throw.
Turn the ship.

**6 · Arm and fire** (§20).

> **Rule 18.3.a** A solution with a course is led. The shot is aimed where the target will be when
> the slug arrives.

> **Rule 18.3.b** A hand-dialled or plotted solution aims at a point in space. It needs no
> identified target. Torpedoes still need one (§8.5).

### 18.4 The targeting computer

**The lock (owner, 2026-11-01).** Lower the targeting computer, get the ship in the sights and zoom
until it fits the box. The bar fills over 10 game seconds. Then the ship is LOCKED: it is the target,
it is tracked on the plot, and fire control has its exact range and bearing. A lock is as loud as
ACTIVE SCAN.

A lock is short. It lasts 10 game seconds (Lock Hold Seconds, in each ship's stats file). The bar runs
down and the sight and fire control count the seconds. Then a tone sounds and the lock drops, with
its firing solution and the switches. The bar starts again from empty.

A lock knows where the ship is. It doesn't know where the ship is going:

- A scan gives the course and speed at the moment of the scan (§9.4).
- A plotted track gives them too, quietly (§18.3).
- The lock takes the newer of the two. Fire control's source reads `LOCK 7 S · SCAN COURSE` or
  `LOCK 7 S · PLOT COURSE`.
- With neither it reads `LOCK 7 S · NO COURSE`. The shot goes where the ship is now, and misses if
  the ship has moved far enough by the time the slug arrives.

A slug takes about 7 seconds to reach 2,000 units, and the gun turns 3° a second. If you haven't
lined up on where the target will be, the lock runs out first.

With the spinal railgun picked, a lock turns our ship onto the lead (owner, 2026-11-09: "you can
ballpark the thing, or you can get into the correct radius range and do all the steps to get a
proper target lock. At which point, your ship also just starts auto-adjusting, and then you can
fire"; Step 170). The lead uses the lock's course, as above. With NO COURSE it points at where the
target is now. When the lock drops, the ship stops turning itself (Rule 4.3.e).

**Not built yet.** An error cone. Today a lock's range and bearing are exact. From the plot, the
solution is as good as your POINTs and RULER.

You are a starship. The computer predicts. What it cannot do is predict from information you have
not gathered.

> **Rule 18.4.a** **The prediction is only as good as the sensor picture.** The computer always
> gives you an answer. How tight that answer is depends on what you have spent to get it.

Four things drive how good the solution is:

| Input | Effect on the solution |
|---|---|
| **SEN power** | More units, tighter prediction |
| **Time since the last sweep pass** | The sweep passes every 3 to 5 s (§9.1). A fresh fix is sharp; an old one has drifted |
| **Track confidence** | A well-established track predicts better than a new return |
| **Whether the target has been identified** | An identified target's speed and heading are known. Otherwise they are guessed |

The computer reports how unsure it is as an **error cone** on the plot — narrow when the picture is
good, wide when it is not.

> **Rule 18.4.b** The error cone is always shown. The player can see how confident the computer is
> before committing a shot.

> **Rule 18.4.c** Firing into a wide cone is a fair choice. It is a low-odds shot, and sometimes a
> low-odds shot is the one you have.

**When to plot by hand.** When the computer's picture is poor — SEN starved, DRADIS off, target not
identified — you can plot the target yourself with POINT and RULER and **beat the computer's
estimate by hand**. Entering your own figures overrides its guess.

> **Rule 18.4.d** This is the real decision: **spend reactor units on SEN for a good automatic
> solution, or spend them elsewhere and do the plotting yourself.** It ties the firing problem
> straight back to the power settings in §3.

> **Rule 18.4.e** A solution goes out of date. If the target changes course or speed after the last
> fix, the aim point is wrong. The error cone widens as the fix ages, which is the only warning you
> get.

### 18.5 Volleys

Dropped. Fire control works one mount at a time, one shot per arming (§8.3.a, §8.4.b).

### 18.6 Flight time changes the weapon

The Garuda's slug flies at 300 units/s (Step 130). This table is for its turrets. Our spinal railgun's
slug flies at 1,500 units/s (Step 170). The Garuda cruises at 22 units/s, and its top speed is 35.

| Range | Flight time | A ship at 22 units/s moves | A ship at 35 units/s moves |
|---|---|---|---|
| 500 units | 1.7 s | 37 units | 58 units |
| 1,000 units | 3.3 s | 73 units | 117 units |
| 2,000 units | 6.7 s | 147 units | 233 units |

A torpedo takes 15 to 18 s to cover 3,000 units (§8.5).

> **Rule 18.6.a** Slugs and torpedoes have separate speeds (owner, 2026-11-01). In a slug's 7 s
> flight the Garuda moves up to 233 units. Its hit area is 437.5 units long (§8.4). By the sums, a
> slug aimed straight at it misses only past 2,980 units at cruise and past 1,875 at top speed
> (Step 141). The gun reaches 2,000.

### 18.7 What the tools do not do

> **Rule 18.7.a** No tool follows a contact. A POINT dropped on a moving ship stays where it was put
> — that is the whole reason it can be used to measure movement.

> **Rule 18.7.b** No tool reveals anything. Marking an unknown contact does not identify it.

---

## 19 · Station panels

The systems rail has one key per drawer (§11.3). Clicking a key opens that drawer.

**What it is.** Six drawers, one per ship system, and three stations: the helm, the orders station
and the periscope.

**How it plays.** You work a system in its drawer and read the result on the plot. Only one drawer
is out at a time, so choosing what to look at is itself a cost. The world keeps moving while you
look.

**How it's built.** One drawer slot. A drawer slides 376 px out from behind the metal frame over
0.42 s, and fades in over 0.3 s. What you set in a drawer is the ship's state, so it stays when the
drawer closes. Each panel is a prefab (`docs/BOARD_UI.md`).

### 19.1 How a drawer behaves

> **Rule 19.1.a** A drawer **slides out from behind the metal frame** (owner, 2026-10-25). It does
> not pop or appear centred.

> **Rule 19.1.b** Drawers open over the plot. **The plot is never covered entirely and never
> replaced.** You can always see the battle space.

> **Rule 19.1.c** Opening a drawer **does not pause the game**. The world keeps moving while you
> work.

| Property | Value |
|---|---|
| Travel | 376 px |
| Slide time | 0.42 s |
| Easing | `cubic-bezier(.2, .85, .25, 1)` — fast out, settles slowly |
| Fade | 0.3 s |
| Closes on | Its rail key again, its number key again, or its latch |
| Number keys | 1 Fire Control, 2 Sensors, 3 Power, 4 Hangar, 5 Engineering, 6 Comms. Change them on the Keys tab of **Starwolf → Tuning** |

> **Rule 19.1.d** **One drawer at a time.** Opening a second closes the first.

> **Rule 19.1.e** The open drawer's lamp on the systems rail is lit.

> **Rule 19.1.f** A drawer keeps its state while closed. Reopening it returns you to what you were
> doing.

The helm and the orders station open and shut with their latches. They do not slide out from
behind the frame yet. The periscope lowers and rises over 0.5 s.

### 19.2 Drawers do not take the plot's place

> **Rule 19.2.a** Whatever a drawer does, **its result is drawn on the plot** too. Fire Control
> shows the selected mount's reach. ACTIVE shows the scan cone. A PDC group on AUTO shows its arc.

The drawer is where you work the system. The plot is where you read it.

> **Rule 19.2.b** A drawer may dim the part of the plot directly behind it, but nothing important
> may be placed there while it is open.

### 19.3 The drawers and stations

| Drawer | What you do in it | Section |
|---|---|---|
| **Fire Control** | Pick a weapon and mount, set the solution, arm, fire | §8, §18, §20 |
| **Sensors** | DRADIS and ACTIVE on or off, gain, band, the scan cone, the contact board, our signal | §9 |
| **Power** | Each bus's selector, the emergency load shed, the reactor | §3 |
| **Hangar** | Bay doors, squadron and order, the small number box, the six launch stages, the lever, RECALL | §12 |
| **Engineering** | The ship picture, the six rooms, repairs | §13 |
| **Comms** | Band, frequency, LOCK, the cipher dials | §10 |

| Station | What you do in it | Section |
|---|---|---|
| **Helm** | Throttle, heading, attitude keys, approach range and mode, stability | §4 |
| **Orders** | The alert condition, the AUX pool, orders | §6 |
| **Periscope** | Slew, MAG, SLEW TO | §11.6 |

> **Rule 19.3.b** **The tool rail keys open nothing.** Clicking a tool key makes it the active tool.
> You then work directly on the plot — click to place, drag to measure. Any readout a tool produces
> is drawn on the plot beside the mark (§18).

### 19.4 What is always visible

Some things are too important to hide behind a click.

| Always on screen | Where |
|---|---|
| Our signal | The signal box, in the frame's top-right |
| Mission clock, paused or running, the rate | The time control |
| Heading, turn, throttle, G load, autopilot state | The helm's header |
| Alert condition | The orders station's header |
| Battle space warnings, hits on us, incoming fire, the mission's line and new contacts, when there are any (§11.1) | The alarm strip |
| DRADIS hearing circle and sweep, while DRADIS is on | Plot |
| The scan cone, while ACTIVE is on | Plot |
| PDC arcs, while a group is on AUTO | Plot |

> **Rule 19.4.a** **Our signal is always visible.** The player never has to open a drawer to find
> out how loud they are.

---

## 20 · Fire control

**What it is.** The Fire Control drawer: pick a weapon, set a firing solution, arm and fire. The
whole job is in one drawer, since only one drawer can be out at a time (§19.1.d).

**How it plays.** You pick a weapon and a mount. You press TRACK to take a solution from your plot
and fine-tune it by hand, or you lock the target with the targeting computer (§18.4). You throw the
arming ladder one rocker at a time. A locked rocker tells you why it is locked. Then you pull the
lever.

**How it's built.** Fire control has a system (RAILGUN, PDC or TORPEDO), a mount, one solution per
system, the four-rocker arming ladder, the capacitor and the fire lever. All of it stays when the
drawer closes. Fire control's target starts as the Garuda. TRACK on a scanned enemy under the
periscope makes that ship the target, and clears the ladder and every solution (owner, 2026-10-29).
Clicking a ship on the plot does nothing (§11.4.b).

> **Rule 20.a** PDCs use no solution and no lever. Their ladder puts them on AUTO (§8.6).

### 20.1 Layout, top to bottom

#### Header

The periscope's bearing, and whether it is on the target's bearing: ON BEARING, or OFF and by how
much.

#### 01 Weapon system

| Control | Shows |
|---|---|
| System selector | RAILGUN, PDC or TORPEDO |
| Mount selector | The mount, for the PDCs. Our ship has one railgun |
| Mount readout | Its name, its arc (`ARC 042° ±80°`, `FIXED 000°` or `ALL ROUND`) and its reach. Our spinal railgun reads `RAILGUN`, `FIXED 000°`, `30,000 km` |
| State | `SLUGS 12`; or the PDC rounds and the loaders; or the torpedo cells |

#### 02 Firing solution

| Control | Shows |
|---|---|
| TRACK key | Fills the solution from your plot (§18.3) |
| Track ID | `TK-048-T07` from a scan or a lock, `PLOT` from your plot, `MANUAL` when dialled by hand, or `NO TRACK` |
| Source | LOCK n S · SCAN COURSE, LOCK n S · PLOT COURSE, LOCK n S · NO COURSE, FROM YOUR PLOT, DIALLED BY HAND, SCANNED, NOT LOCKED, or NOT IDENTIFIED ON SENSORS |
| RNG, BRG, SPD | The solution, with a knob each (§18.3) |

> **Rule 20.1.a** TRACK needs the bridge at integrity 3 or more. It also needs an identified target,
> or a POINT on the plot.

#### 03 Capacitor

Cells and a percent (§8.4). The note reads CAPACITOR CHARGING +n/S, or CAPACITOR CHARGED, or WEP BUS
DEAD — CAPACITOR BLEEDING OFF.

#### 04 Arming ladder

Four rockers, thrown in order:

| Rocker | Throws when |
|---|---|
| LOAD | At least 2 WEP units, the magazine at 3 or more, weapons released (CONDITION 2 or 1), ammunition left, and a solution. For a PDC group: its autoloaders on, in place of a solution |
| ANGLE | The solution's bearing is inside the mount's arc (§8.7). Free for PDCs |
| BEARING | The periscope is within 4° of the target's bearing, or of the dialled bearing. Free for PDCs |
| ARM | The capacitor has the shot's charge: railgun 70, torpedo 40, PDC 25 |

For a fixed gun, such as our spinal railgun, LOAD needs no solution, and ANGLE and BEARING are free (Rule
8.3.e). Step 3 of `docs/SNIPER_PLAN.md` takes ANGLE and BEARING away.

> **Rule 20.1.b** Each rocker needs the one before it. Releasing a rocker releases it and every one
> after it.

> **Rule 20.1.c** A locked rocker shows why: NOT RELEASED, MAGAZINE HIT, NO WEP BUS, OUT OF ARC,
> CHARGING or INTERLOCK.

#### 05 Fire lever

Pull the lever past 90% with the ladder set. The railgun fires one slug. The torpedo launcher fires
a salvo. For a PDC group the lever does nothing.

> **Rule 20.1.d** A status line says what is missing. It shows the first that applies, in this
> order:
>
> `NO TARGET DESIGNATED` → `NO TRACKING ID — NOT IDENTIFIED` → `BRIDGE DAMAGED — PLOT COMPUTER
> OFFLINE` → `NO FIRING SOLUTION` → `WEAPONS NOT RELEASED — CONDITION 2 REQUIRED` → `MAGAZINE
> DAMAGED — NO ROUNDS STRUCK UP` → `PDC AUTOLOADERS OFF` → `ARMING SEQUENCE INCOMPLETE`
>
> The first two are skipped for a hand-dialled solution. For a fixed gun the first four are skipped.
> With the ladder set it reads `SOLUTION LOCKED — MOUNT READY`, or `MOUNT READY` for a fixed gun. After a shot it reads `ROUNDS AWAY`, then `HIT · …` or `MISS`. A PDC group
> on AUTO reads `PDC-A AUTO — WATCHING ITS ARC`, then its last result.

### 20.2 What the panel puts on the plot

Per §19.2.a, everything here draws on the plot too.

| Panel action | Plot shows |
|---|---|
| Fire Control open | The selected mount's reach, a dashed ring round our ship. For the spinal railgun, also a solid line from the nose dot out to its full reach, with no cone (Step 170) |
| A PDC group on AUTO | Its arc, a dashed sector off our hull |
| Shot fired | The slug in flight, at true speed: a white dot with a short trail, white like our ship and our fighters (owner 2026-10-26). The dot can go up to twice that speed just after firing, and it can slow before a hit (§9.6) |
| Torpedo salvo fired | Each torpedo in flight, also a white dot with a short trail |

### 20.3 State and persistence

> **Rule 20.3.a** Closing the drawer drops nothing. The solution, the ladder and the capacitor's
> charge stay.

| Event | Clears |
|---|---|
| System or mount changed | The ladder |
| Warhead changed | The ladder |
| Solution changed by TRACK or a knob | The ladder |
| A new target, from TRACK | The ladder and every solution |
| Shot fired | ARM only. The solution stays |
| Last round gone | The ladder |
| LOAD's needs fail: WEP units, the magazine, CONDITION, rounds | The ladder, on the next board step |
| The ship turns out of arc | ANGLE and every rocker after it, on the next board step |

> **Rule 20.3.b** A thrown rocker whose need fails drops on the next board step, with every rocker
> after it (Step 106). This is true of LOAD and ANGLE. BEARING is not checked again. A moving
> target drifts off it slowly. The lever checks LOAD and ANGLE once more before it fires.

> **Rule 20.3.c** A solution stays until it is replaced.

### 20.4 Weapons power and releasing

> **Rule 20.4.a** LOAD needs at least 2 WEP units. Below that, LOAD is locked.

> **Rule 20.4.b** The cost is a **threshold**. A loaded weapon uses no units. The charge for each
> shot comes from the capacitor (§8.4).

> **Rule 20.4.c** Dropping below 2 WEP units after LOAD releases the whole ladder on the next board
> step (Rule 20.3.b). With no WEP units the capacitor loses 3 per game second.

> **Rule 20.4.d** Any rocker can be released at any time. Releasing is immediate and free.

> **Rule 20.4.e** Releasing is how you get out of a commitment.

---

## Appendix A · How the older scripts differ from the board

Not part of the specification. Soham's older scripts are kept in the project, but the board does
not use them. This list shows where they work differently from the board.

| Spec section | The older script | The board |
|---|---|---|
| §3 Power | `PowerManager.AddPower` takes 0.5 off `currentReactorPower` per bar — the reactor runs down like fuel | Units move between buses. The reactor never runs down |
| §3 Power | Systems are named `engines / arms / bay / support / sig` | Four buses: ENG, WEP, SEN, AUX |
| §4.3 Drift | `rb.linearVelocity = transform.up * currentSpeed` — velocity is locked to facing, so the ship never slides. `maxLateralDrift` and `driftCompensationRate` are serialized but unused | Turning leaves the velocity on its old direction; the sideways part bleeds off |
| §4.2 Throttle | `W`/`S` set a continuous target while pressed | The throttle is dragged on the helm, or `W`/`S` step it 10% a press, from full reverse to full ahead |
| §8.7 Arcs | `WeaponBase.angleOfFire` is declared and set in two places but never checked. No arc centre exists | ANGLE checks the solution against the mount's arc |
| §13.1 Shields | `Shields.cs` exists with recharge, colours and a renderer | There are no shields. Auxiliary is the recovery resource |
| §9.1 DRADIS | `Radar.cs` places every registered target every frame | DRADIS updates a contact only when the sweep passes it, or while the ACTIVE SCAN cone covers a scanned ship |

---
