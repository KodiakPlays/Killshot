# Starwolf — Specification

**Version** 0.3 · 2026-10-27
**Status** Describes the tactical board as built on 2026-10-27. Sections marked **Not built yet**
describe design intent.

Each ship's numbers are in its stats file: `Assets/Ships/Asura.asset` (our ship) and
`Assets/Ships/Garuda.asset` (the enemy). Change them with **Starwolf → Tuning**. The owner's
decisions are in `Assets/Scenes/Tactical/BUILD_LOG.md`, section 4.

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

Built so far: an intro, a menu and a briefing, then the board with one enemy, the Garuda. The
mission, the win and the escape are not built yet (§15).

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
| **Shots take time** | A railgun slug takes about ten seconds to reach 25,000 km. A missile takes minutes. You aim where the target will be. |

---

## Features, one at a time

Everything below breaks down system by system. Read in order for the whole picture, or jump.

| § | System | What it covers |
|---|---|---|
| 0 | Sources | Which source is newest |
| 1 | Conventions | Bearings, kilometres, the board step |
| 2 | Time | Pause, the four rates, game time |
| 3 | Power | The reactor, four buses, draw and vent |
| 4 | Movement | Throttle, drift, stability, the combat autopilot (not built yet) |
| 5 | Being seen | Our signal; detection, disguise and awareness (not built yet) |
| 6 | Auxiliary | The orders station, its AUX pool and the alert condition |
| 7 | Reflex | The cloak (not built yet) |
| 8 | Weapons | Two railgun turrets, two PDC groups, the missile launcher, damage |
| 9 | Sensing | DRADIS, the sweep, the active scan, what the plot shows of a ship |
| 10 | Comms | Tuning and the cipher |
| 11 | The board | Layout, plot, rails, colour, type, the periscope |
| 12 | Hangar | Interceptors, assault boats, launching |
| 13 | Damage | Four sides, six compartments, conditions, repairs |
| 14 | Failure | How our ship is lost |
| 15 | Mission | How a mission runs, how you win, the level (not built yet) |
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
kilometres. The rules step once per board step, in a fixed order.

### 1.1 Bearing

The battle space is a **2D plane**. Positions are `(x, y)` in km, x east and y north.

- `0°` is `+y`, increasing **clockwise**. `090°` is `+x`.
- Shown zero-padded to three digits: `062°`.
- **Screen angle = bearing − 90°.**

```
angDiff(a, b) = ((a - b + 540) mod 360) - 180        // (-180, 180]
```

> **Rule 1.1.a** Every visual that comes from a heading — the ship picture's rotation, the `HDG`
> readout, a PDC arc — comes from **one** bearing value per object per frame.

### 1.2 Units

Distances are **kilometres**. Speeds are km/s. Times are game seconds.

| Number | Value |
|---|---|
| Battle space | 100,000 × 100,000 km (set with Starwolf → Level) |
| DRADIS reach | 15,600 km at gain 1, 30,000 km at gain 5 |
| Visual range | 10,000 km |
| Railgun reach | 25,000 km |
| Missile reach | 35,000 km |
| PDC reach | 10,000 km |
| Top speed | 12.5 km/s |
| Plot scale at zoom 1 | 60.9 px per 1,000 km |

> **Rule 1.2.a** One world unit is one kilometre (owner).

### 1.3 Loop

The board steps every 0.42 s of real time. Each step moves the game on by the rate: 0.25, 1, 5 or
10 game seconds (§2).

One step runs in this order: the mission clock, power, the AUX pool, our ship's movement, the
battle space check, slugs and missiles, fighters and boats, enemy missiles and our PDCs, damage
control, fire, vacuum and radiation, the launch stages, the weapons capacitor, sensors and signal.
Then each enemy thinks and moves.

> **Rule 1.3.a** A stalled frame never makes the board catch up in a burst. Missed steps are
> dropped.

Between steps, ships are drawn gliding on along their velocity (§9.6.a).

---

## 2 · Time

**What it is.** Two clock states, running and paused, plus four rates for how fast the running
clock moves.

**How it plays.** You can stop the world to think and run it fast when nothing is happening. There
is no plan-then-execute step: orders take effect as you give them.

**How it's built.** One board clock drives everything. Every rate in the game is per game second,
so at x10 everything runs ten times as fast (owner, 2026-10-24).

| State | Behaviour |
|---|---|
| **Running** | The game advances at the rate. |
| **Paused** | Nothing real changes. Controls still work. |

The rates are x0.25, x1, x5 and x10, on four keys on the time control. The board loads paused.
Pressing a rate key also starts the clock. The pause key stops and starts it. Crossing out of the
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

An EMP empties every bus at once. The power stays out for 60 game seconds (§8.1a).

### 3.4 What a unit is worth

| Bus | What its units do |
|---|---|
| ENG | The throttle can go up to 20% per unit (§4.2). With no ENG units, the ship drifts. |
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
bends round slowly after it.

**How it's built.** Every ship has an ordered heading and throttle, and an actual heading and
velocity. Each board step, the bow swings toward the ordered heading, and speed along the nose
chases the throttle. The sideways part of the velocity bleeds off. The numbers are in each ship's
stats file, under navigation.

### 4.1 Ordered and actual

Every ship has ordered state (heading, throttle) and actual state (heading, velocity, position).
Actual moves toward ordered at capped rates whenever the clock runs.

> **Rule 4.1.a** Our ship and the enemy move by the same rules. The enemy is our ship with other
> stats. Fighters and assault boats fly straight to where they are going at a fixed speed (§12).

### 4.2 Throttle

You drag the throttle on the helm, from 0 to 100%. It loads at 0.

```
throttle cap = ENG units × 20 × (engine room integrity ÷ 5)
target speed = throttle ÷ 100 × 12.5 km/s
```

Speed along the nose rises at 0.2175 km/s² and falls at 0.4075 km/s². There is no reverse.

> **Rule 4.2.a** If the ENG bus can no longer support the throttle, the throttle drops to 0 (owner,
> 2026-10-25). You set it again.

### 4.3 Drift — the ship slides

> **Rule 4.3.a** `A` and `D` step the ordered heading by 3° per press. The bow turns toward it at
> 20° per game second, slower when the engine room is damaged (never below 1°). The velocity keeps
> its old direction and bends round slowly. Pressing A or D also clears the helm's attitude key.

Each game second, velocity splits into a part along the nose and a sideways part:

```
sideways speed × 0.96307
all speed      × 0.997502
```

### 4.4 Stability

The helm shows G load and stability. Nothing uses them yet.

```
G load    = 1 + throttle ÷ 100 + (FLANK 0.5, EVASIVE 1.2, DIRECT 0)
stability = 100 − 45 × (G load − 1), from 0 to 100
```

### 4.5 Collision

Not built yet. Ships pass through ruins, debris and each other.

### 4.6 Autopilot (combat)

**Not built yet.** The helm's attitude keys, approach range and approach mode can be set. They do
not steer the ship yet. The helm's header reads MANUAL, or AUTO and the approach mode while an
attitude key is lit.

**Owner, 2026-10-25.** A combat autopilot. It only works on a target. Its settings are on the helm.

- **Target:** fire control's selected target. It must be identified by an active scan, because an
  autopilot cannot hold range on a ship you can't see.
- **Facing:** NOSE, STBD, PORT or TAIL (the helm's attitude keys). That side is kept toward the
  target.
- **Range:** the helm's approach range: 2,000, 4,000, 8,000, 12,000, 18,000 or 24,000 km. It tries
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

**Not built yet.** Our signal is built (§5.0). Nothing in the enemy reacts to it yet. Everything
after §5.0 is design intent.

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
missile salvo adds 25 at once. At 45 or more, the signal box reads `1 HOSTILE SET HAS YOUR RANGE`
and the alert tone plays once. The numbers are in each ship's stats file, under sensors and power.

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

Once provoked, an enemy climbs a ladder. **It does not come back down.** The numbers below are the
prototype's, in its own units.

| Condition | Raises to |
|---|---|
| `dist < 18000 && sig > 2.0` | 1 — UNEASY |
| `dist < 12500 && sig > 3.0` | 2 — SEARCHING |
| `dist < 8500 && sig > 4.5` | 3 — ALERT |
| Alert active, reinforcements away | 4 — GENERAL ALARM |

> **Rule 5.3.a** Awareness never decreases. You can re-hide, but you cannot make them forget.

### 5.4 The alert net

Firing any weapon triggers it. In the prototype: a 59-second window, then two reinforcements at
once, then one every 14 s, alternating DESTROYER / FRIGATE, entering at the arena edge and closing
at 92 units/s, ramming for 400 damage.

> **Rule 5.4.a** `TBD` — in the prototype this fires *before* the validity check, so a dry trigger
> pull wakes the alert. Keep or fix.

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

The orders act on the room selected on the Engineering drawer. FIRE, BLKHD and SCRUB are blank
plates. They are not fitted.

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

**What it is.** Three weapon types: two railgun turrets, two PDC groups and one missile launcher
(the VLS).

**How it plays.** Which way you face decides what the railguns can reach. PDCs are the only answer
to a missile, and each group covers a fixed arc. Every shot spends charge from the weapons
capacitor, and every slug and missile is counted.

**How it's built.** Fire control works one mount at a time. A shot needs a firing solution, the
arming ladder and enough charge (§20). Slugs and missiles fly across the plot. They hit a ship
they pass close enough to. The numbers are in each ship's stats file, under weapons.

### 8.1 Damage

What each hit does. The numbers are the firing ship's: in its Weapons group, or its Hangar group for
fighters (owner, 2026-10-27; BUILD_LOG decision 49).

| Weapon | Against armour | Through an open side |
|---|---|---|
| Railgun slug | Takes 50 armour off the side it hits | Takes 2 integrity off the room behind; 50% chance to breach it |
| HE missile | Takes 100 armour | Takes 2 integrity off the room behind; 60% chance of fire |
| PENETRATOR missile | Goes straight through | Takes 2 integrity off the room behind; 40% chance of fire |
| EMP missile | No damage | No damage. The ship's power goes out for 60 game seconds |
| Fighter run | Takes 10 armour per fighter | Takes 1 integrity per fighter off the room behind |

Any hit on the reactor room, except a fighter's, has a 50% chance to leak radiation. That chance
is in the Damage group of the ship being hit. What damage does to a ship is in §13.

> **Rule 8.1.a** Two slugs open the prow, starboard or port side. One slug opens the aft. One HE
> missile opens any side.

### 8.1a Warheads

The missile launcher fires one of three warheads: HE, EMP or PENETRATOR. The board starts on HE.
The warhead selector is not drawn on the Fire Control drawer yet.

### 8.2 Terrain

Not built yet. Slugs and missiles pass through ruins and debris. Whether terrain should stop a
shot is not decided.

### 8.3 What you have

There are no broadside guns. Arcs are measured from the bow, clockwise.

| Mount | Arc | Covers | Reach | At the start |
|---|---|---|---|---|
| RG-A, railgun turret A | 042° ±80° | 322° → 122° | 25,000 km | 22 slugs |
| RG-B, railgun turret B | 250° ±80° | 170° → 330° | 25,000 km | 22 slugs |
| PDC-A, PDC group A | 042° ±35° | 007° → 077° | 10,000 km | 2,400 rounds |
| PDC-B, PDC group B | 200° ±35° | 165° → 235° | 10,000 km | 2,400 rounds |
| VLS, missile launcher | All round | All round | 35,000 km | 8 cells |

> **Rule 8.3.a** Fire control works one mount at a time. You pick the system (RAILGUN, PDC or
> MISSILE), then the mount.

> **Rule 8.3.b** **Ammunition is a real count** (owner, 2026-10-27). It only goes down. There is no
> reload. With nothing left, LOAD refuses.

> **Rule 8.3.c** The railgun arcs leave a gap from 123° to 169°, on the starboard quarter. They
> overlap from 322° to 330°, on the port bow. Turning the ship brings a turret to bear.

The armour is PROW 100, STBD 100, AFT 50, PORT 100 (§13.2). The aft is the weakest side.

### 8.4 Railgun

A slug flies at **2,500 km/s**, out to 25,000 km. It takes about 10 s to reach full range (owner,
2026-10-27).

- A slug hits a ship it passes within **100 km** of. The distance is the same for every ship, and
  for missiles (owner, 2026-10-27; BUILD_LOG decision 47).
- A slug that reaches 25,000 km without a hit counts as a miss.
- Each shot spends 70 charge from the weapons capacitor and adds 25 to our signal.
- A hand-dialled bearing is in whole degrees. At 25,000 km one degree is about 436 km sideways, so
  a hand-dialled shot can just miss a cruiser.
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
| Missile salvo | 40 |
| PDC (to put a group on AUTO) | 25 |

> **Rule 8.4.a** The capacitor sets the rate of fire. At 5 WEP units it refills a railgun shot in
> under 3 s.

> **Rule 8.4.b** **One shot per arming.** Firing releases ARM. Throw ARM again to fire again, once
> the capacitor has the charge.

### 8.5 Missile

The launcher fires a salvo of 2 missiles, fanned 3° apart. Each flies at 100 km/s and turns toward
its target at up to 6.5° per game second. Each flies at most 35,000 km.

- A missile needs a target identified by an active scan (§9.4).
- Each missile uses one cell. The salvo size is set in the stats file. The salvo knob is not drawn
  yet.
- A salvo spends 40 charge and adds 25 to our signal.
- A missile hits within the same 100 km (§8.4), ours and the enemy's.

### 8.6 PDC

A PDC group shoots at enemy missiles by itself once it is on **AUTO**. You never fire it.

- Select the group on Fire Control and throw the arming ladder. ARM puts the group on AUTO.
  Releasing the ladder takes it off. Both groups can be on AUTO at once.
- A group on AUTO gets one go at each enemy missile inside its arc and 10,000 km. Each go has a
  70% chance to shoot the missile down, and spends 100 rounds.
- A group with fewer than 100 rounds left drops off AUTO.
- PDCs need their autoloaders on. They also need what every weapon needs: at least 2 WEP units,
  the magazine at integrity 3 or more, and weapons released (§6.1). The LOADERS button is not drawn
  yet. The autoloaders are on at load.
- Each group on AUTO shows its arc on the plot as a dashed sector.

> **Rule 8.6.a** PDCs are the only defence against a missile. Nothing soaks the damage.

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

**How it plays.** You never quite have a live picture. A contact you only hear is a bearing line.
Moving your ship and laying more bearing lines finds its range. An active scan names it and lets
you target it, but makes you louder.

**How it's built.** DRADIS hears a contact by band and range. Each pass of the sweep updates where
each contact was last seen. With ACTIVE on, a contact inside the scan cone is identified as the
sweep passes it. The numbers are in each ship's stats file, under sensors.

### 9.1 DRADIS and the sweep

DRADIS hears a contact when all four are true:

- DRADIS is on.
- The SEN bus has at least 1 unit (Step 82, owner: no power, no hearing).
- The contact gives off the band you have picked: THERM, RAD, UV or EM. The Garuda gives off THERM
  and RAD.
- It is within range.

```
range = 12,000 km + 3,600 km × gain        (gain 1 to 5)
UV hears to 0.55 of that range
```

You set the gain on the Sensors drawer. It can go up to the SEN units, never below 1. It loads at 1.

> **Rule 9.1.a** The sweep turns once every `5.4 − 0.5 × gain` game seconds: 4.9 s at gain 1,
> 2.9 s at gain 5. It stops while paused, while DRADIS is off, and while the SEN bus is empty.

> **Rule 9.1.b** Between passes, a contact out of sight stays where the sweep last touched it
> (§9.6).

> **Rule 9.1.c** Each pass over a heard contact that is not identified leaves a bearing line from
> where our ship was. The lines fade out over 120 game seconds. As our ship moves, the lines cross
> at the contact. That is how you find a passive contact's range (owner, 2026-10-23).

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

A ship within 10,000 km is in sight. Its model shows on the plot, seen from above, and glides
(§9.6). Seeing a ship does not identify it.

The periscope's CLASS key puts a heard contact under its sight on the contact board. The contact's
name stays BOGEY until an active scan.

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

> **Rule 9.4.a** Identification is what an active scan gives you. An identified ship shows its
> name. You can lock TRACK on it, fire missiles at it, and read what your hits do to it (§20,
> §13.5).

> **Rule 9.4.b** Identification is kept. It never resets.

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
- **Heard, not identified:** a bearing line from our ship. It appears when the sweep first touches
  the contact. It keeps that bearing until the sweep passes again.
- **Identified, outside visual range:** a dot where the sweep last touched it, labelled with the
  range at that moment. The dot moves when the sweep passes again.
- **Inside visual range:** the ship's own model, seen from above, gliding. It is red once
  identified. Until then it is amber and labelled BOGEY with its track number, and you cannot
  target it.
- **Destroyed:** it stays on the plot. Inside visual range its model turns grey and drifts. Outside
  visual range, a dim dot drifts.

> **Rule 9.6.a** The rules step once per board tick. Between ticks, each ship is drawn moving on
> along its velocity, up to one tick ahead. Rules use the position from the last tick.

> **Rule 9.6.b** Seeing a ship does not identify it. Only identification gives its name and lets you
> target it.

> **Rule 9.6.c** A destroyed ship never leaves the plot.

> **Rule 9.6.d** Starting numbers on the board, to be tuned: visual range 10,000 km. DRADIS reaches
> 12,000 km plus 3,600 km per gain step: 15,600 km at gain 1, 30,000 km at gain 5. Railguns reach
> 25,000 km, missiles 35,000 km and PDCs 10,000 km (owner, 2026-10-27).

---

## 10 · Comms

**What it is.** Two puzzles in a row: tune in a signal, then break its cipher.

**How it plays.** You find the carrier signal on the right band and frequency, and lock it. The
message comes through enciphered. You turn four dials until it reads in plain English.

**How it's built.** The radio has a band, a frequency, a lock and four cipher dials. The signal is
there from the start.

**Tuning.** Band 1 to 3; the signal is on band 2. Frequency 1 to 100, in steps of 0.1; the signal
is at 72.2. LOCK takes only within 0.3 of the signal. Changing the band or the frequency drops the
lock.

The receiver scope shows the band. Its bars peak near the signal. The signal lamp turns amber
within 12 of the signal, and green inside the lock window.

> **Rule 10.a** Tuning is by eye. The scope and the signal lamp show how close you are.

**Cipher.** A Vigenère cipher with the key `WOLF`, the ship's callsign. Four dials, A to Z. Click a
dial to step it forward; Shift-click steps it back. The dials turn only while the signal is
locked. The message is:

`HOSTILE FLEET INBOUND VECTOR TWO SEVEN NINE HOLD STATION AWAIT RELIEF`

The status line reads INCOMING SIGNAL, then DECIPHERING once locked, then MESSAGE READ when the
dials match the key.

> **Rule 10.b** Not built yet: what a read message gives you. Today it changes nothing in the game.

---

## 11 · The board

**What it is.** The whole screen: a metal frame with one big plot in the middle. Two rails of keys,
six drawers, the helm, the orders station and the periscope are set into the frame.

**How it plays.** You look at one picture the whole game. Drawers slide out over the edges of it,
but the battle space is always there. Everything you do reads back on the plot.

**How it's built.** A fixed 1920 × 1080 layout from the Claude Design handoff. Each panel is a
prefab. The prefabs, their art and their layout are described in `Assets/UI/Board/README.md`.

### 11.1 Layout

Design resolution 1920 × 1080.

- The **metal frame** is the front layer. Drawers slide out from behind it (owner, 2026-10-25).
- The **plot** fills the middle.
- The **tool rail** has the plot tools (§18).
- The **systems rail** has one key per drawer: FIRE, SENS, PWR, HGR, ENGR and COMM (§19).
- The **helm** and the **orders station** open and shut with a latch.
- The **periscope** lowers into the board and goes back up.
- The **time control** has the pause key, the four rate keys and the mission clock.
- The **console** has a power lamp, the SCALE knob and the grid tint knob.
- The **alarm strip** shows new contacts and battle space warnings, only when there is one.
- The **signal box**, in the frame's top-right, shows our signal.

> **Rule 11.1.a** The plot is the subject. Everything else comes second.

> **Rule 11.1.b** No cockpit, no outside camera. Drawers open over the plot; they never replace it.

### 11.2 Plot

The plot is a grid. Its tint is white or amber, set with the console's tint knob (WHT or R-O).

- Scale: 60.9 px per 1,000 km at zoom 1.
- Zoom: drag the SCALE knob (×10 per 160 px) or turn the mouse wheel (×1.12 per notch). Zoom is
  rounded to 0.01. Zoom 1 is the closest. Zooming out stops at 0.35, or sooner, where the battle
  space's left and right edges reach the plot's sides (owner, 2026-10-25).
- Pan: drag the grid, or use the arrow keys. `C` centres the plot on our ship again.
- The numbers on the top and left edges measure from our ship: 0 level with it, then 5,000,
  10,000 km and on. They move with the ship (owner, 2026-10-26).

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
| A contact | A bearing line, a dot with its range, or its model (§9.6) |
| Our fighters | White wedges in a V, with no labels |
| Our slugs and missiles | A white dot with a short trail |
| Enemy missiles | A red dot with a short trail |
| A ship our marines secured | A dot |
| DRADIS | The hearing circle and the sweep; with ACTIVE on, the scan cone |
| Fire control | The selected mount's reach as a dashed ring while the drawer is out; each PDC group on AUTO as a dashed sector |
| Marks | POINT, RULER and COMPASS (§18) |

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
zoom, from 1× to 8×. Press SLEW to swing it onto the target. Press CLASS to classify what is in the
sight (§9.3).

Its bearing matters to fire control. BEARING, the third rocker of the arming ladder, needs the
periscope within 4° of the target (§20).

### 11.7 Typography

**Barlow Condensed** (400/500/600) — titles, names, labels, designators.
**Share Tech Mono** — *all* numerics, bearings, ranges, timers, ticks.

> **Rule 11.7.a** Labels uppercase. No italics. Units set at the same size as their number.
> **Numerics never animate their digits — they snap.**

### 11.8 Time control and console

Pause key · four rate keys (×0.25, ×1, ×5, ×10) · mission clock · power lamp · SCALE knob · grid
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

**What it is.** Where you launch interceptors and assault boats.

**How it plays.** You give a squadron a job and launch it. It flies out to fire control's target
and gets on with it. Boarding comes last: fight a ship down first, then send the marines.

**How it's built.** Two squadrons. A launch runs six stages in order, then the launch lever. Craft
fly straight to their goal at a fixed speed. The numbers are in each ship's stats file, under
hangar.

> **Rule 12.a** Nothing in the hangar is flown by hand. Pick the squadron and its order, then
> launch. The squadron goes for fire control's target.

### 12.1 Interceptors

Six starfighters. Their orders are DESTROY, DISABLE and ESCORT. Only craft marked READY launch.

- They fly at 40 km/s.
- Within 200 km of the target, every fighter makes an attack run every 30 game seconds.
- Each run takes 10 armour off the side facing the fighter. Once that side is open, each run takes
  1 integrity off the room behind it.
- DISABLE always attacks the stern, and so the engine room.
- ESCORT stays with our ship.

### 12.2 Assault boats — marines

Four boats. Our ship has 18 marines aboard, and each boat takes 6. The boats' orders are DESTROY,
DISABLE and SECURE.

| Order | If the marines win |
|---|---|
| **DESTROY** | The ship is scuttled. Its reactor goes to 0. |
| **DISABLE** | Its engine room and magazine go to 0. |
| **SECURE** | The ship is captured. It stops moving and is no longer hostile. |

- The boats fly at 30 km/s and dock within 50 km of the target.
- They wait until the target has an open side and is slower than 0.5 km/s (owner: fight it down
  first). The hangar reads HOLDING OFF, and why.
- Aboard, the marines fight for 60 game seconds. The boats stay with the ship while it moves and
  fights (owner, 2026-10-27).
- The defenders are a quarter of the target's crew, rounded up. The marines win with odds of
  `marines ÷ (marines + defenders)`. A win kills the defenders and costs half a marine per
  defender. A loss kills every marine aboard, and half as many of the enemy crew.
- The boats go no further than 15,000 km from our ship (owner, 2026-10-27). Outbound past that,
  they turn back. On the way back, out of range, a 120 game-second clock runs, with a warning. Close
  in to collect them. If the clock runs out, the boats and their marines are lost.

> **Rule 12.2.c** **There are no boarding pods.** Boarding runs through the hangar. A boat launches,
> flies to the target on its own, and its marines do the work.

> **Rule 12.2.d** Boarding needs no scan. It needs an open side and a slow target.

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

Then pull the launch lever. The launch is refused if flight operations are not released, the bay is
below integrity 3, no craft is ready, there are no marines for the boats, or the launch bearing is
more than 60° from our heading. The squadron takes off on the launch bearing, just outside the
small circle round our ship. RECALL turns it back. It lands when it reaches our ship.

---

## 13 · Damage

**What it is.** Four armoured sides and six compartments per ship. No shields. Every ship uses the
same model, ours too.

**How it plays.** The side you present is your armour, so which way you face is a defensive choice
as well as a firing one. Once a side is open, hits reach the room behind it. Rooms catch fire, lose
their air and leak radiation.

**How it's built.** Four armour values and six rooms. Each room has integrity 0 to 5, crew and air.
A hit strikes the side facing where it came from. The numbers are in each ship's stats file, under
damage. The full rules are in `Assets/Scenes/Tactical/DAMAGE_README.md`.

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

Six rooms per ship, 52 crew in all.

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

Engineering acts on one selected room. Its rockers cost no charge:

| Rocker | What it does | Refused when |
|---|---|---|
| REBOOT | Adds 1 integrity | The room is at full integrity |
| REPAIR | Sends the party. After 6 game seconds it puts out a fire; or, in a sealed airless room, brings the air back; or adds 1 integrity (2 at CONDITION 1) | A party is already out; the room is airless with its doors open; or the room is at full integrity with air and no fire |
| SEAL | Shuts an airless room's doors | The room has air, or its doors are already shut |
| VENT | Dumps the room's air. Any fire goes out. The crew and any party in the room die | The room is already vented |
| RECALL | Calls the party back | No party is out |

The orders station gives PARTY, SEAL and VENT for AUX charge (§6).

---

## 14 · Failure

**What it is.** How our ship is lost.

**How it plays.** Losing comes down to the reactor. Flying out of the fight loses the ship too.

**How it's built.** Two checks run each board step. Either one ends it.

| Condition | Trigger |
|---|---|
| **Destroyed** | Our reactor room's integrity reaches 0 |
| **Left the battle space** | Our ship stays outside the battle space for 30 game seconds. The alarm strip reads SHIP LOST — LEFT THE BATTLE SPACE |

> **Rule 14.a** Within 2,500 km of the edge, the alarm strip reads APPROACHING THE EDGE OF THE
> BATTLE SPACE. Outside, it reads EXITING THE BATTLE SPACE — TURN BACK and counts down the seconds
> left. Crossing out drops the rate to x1 and plays the alert tone.

> **Rule 14.b** A lost ship goes dark. Its power is out for good. There is no defeat screen yet.

---

## 15 · Mission

**Built so far:** the first mission's win and loss (§15.2, Step 85) and the level's ruins (§15.3). The
rest of this section is the full design, not built yet.

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

**Built now (Step 85).** The first mission ends at the kill (`FTUE_PLAN.md` §1, owner 2026-10-18).
Rule 15.2.a, the escape, comes later.

- You win when the Garuda is destroyed, or when our marines take it (SECURE).
- You lose when our reactor reaches 0, or when our ship stays outside the battle space until its
  countdown ends.
- Locate target ends when our sensors identify the Garuda.
- When the mission ends, the game pauses. 2 s later the end screen covers the board: MISSION
  COMPLETE or SHIP LOST, the reason, then the target, our ship and the time taken.
- RESTART MISSION plays the level again, from the board. MAIN MENU goes back to the intro.
- The code is `QuestSystem` (on `World`) and `EndScreenPanel` (`End Screen.prefab`).

### 15.3 The level

> **Rule 15.3.a** **There is no true deep space.** Every level is full of structure — the rings of a
> shattered moon, a dead fleet's wreckage, a broken starbase. These are functional: they change
> DRADIS returns, scan noise and where you can move.

Built so far: each level is its own scene, loaded on top of the board. `Level_01` has a starbase
husk at the centre and a debris belt, thicker toward the centre. Our ship starts at the bottom
facing north; the Garuda starts at the top facing south, 90,000 km away. The ruins do not yet stop
ships, shots or sensors.

You arrive with **old, unverified Imperial records** marking a few landmarks — a listening post, an
abandoned dock, a comms tower. They must be scanned or approached to confirm.

> **Rule 15.3.b** The first phase of every mission is **exploration**, not combat. There is no fuel
> mechanic; you are free to wander.

> **Rule 15.3.c** The flagship is hidden and protected by forces and misdirection. Finding out
> *where it is and how it is guarded* is the puzzle.

### 15.4 Loadouts

| Loadout | railgun | missile | hull/side | sensors | DRADIS sweep | aux start |
|---|---|---|---|---|---|---|
| STANDARD KILL | 3 | 4 | +0 | +0 | 3.0 s | 0 |
| GHOST KILL | 5 | 2 | +0 | +1 | 2.4 s | 20 |
| BRAWLER | 2 | 5 | +12 | +0 | 3.4 s | 10 |

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

1. **The escape** (§15.2). The first mission ends at the kill (Step 85). The jump point and the
   escape are not built.
2. **The combat autopilot** (§4.6). TAIL is not defined. TARGET LOST is only proposed.
3. **Being seen** (§5). The signal is built. Nothing in the enemy hears it.
4. **The enemy's mind.** The Garuda patrols a loop at 5.5 km/s. When hit, it fires two HE missiles
   back if we are within 35,000 km, at most once every 120 game seconds. Hearing and hunting are not
   built. Its settings are on its `EnemyBrain` in the board scene.
5. **Picking a target** (§20). Fire control's target is always the Garuda, the one enemy.
6. **Undrawn controls** (§8). The PDC quadrant keys, LOADERS, the warhead selector and the salvo
   knob are not drawn on Fire Control.
7. **Stability** (§4.4). It is shown, but nothing uses it.
8. **Terrain** (§4.5, §8.2). Ships and shots pass through ruins and debris.
9. **What comms gives you** (§10). A read message changes nothing.
10. **Reflex numbers** (§7): `E_floor`, `reflexSpeedMax`, `staleTrackTimeout`.
11. **The dry trigger pull** (§5.4.a). Whether it should raise the alert.

## 18 · Plotting tools and the firing solution

The tool rail has five tools (§11.3). They draw on the plot and change nothing in the world.

**What it is.** CURSOR and four marking tools — POINT, RULER, COMPASS and ERASE — and the firing
solution they help you build.

**How it plays.** You work the target out yourself. Lay bearing lines, drop POINTs, wait, drop
more, and run a RULER between them to read the target's course and speed. Then dial the solution in
by hand. Once an active scan has identified the target, TRACK does it for you.

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

The board shows bearing on RULER and COMPASS. Fire control's BRG is a relative bearing. Angle on bow
is not shown yet.

### 18.2 The tools

#### POINT

Click to drop a point. Each point is stamped with the mission time it was placed, and shows that
time.

> **Rule 18.2.a** Points record a timestamp. That is what makes a target's speed measurable — see
> §18.3.

#### RULER

Drag from one spot to another. It shows `RNG` (distance), `BRG` (bearing), and the time to cover
that distance at our current speed.

A RULER end snaps to a POINT nearby. A RULER from one POINT to another, placed at different times,
is a **track**. It shows the target's course and speed instead, such as `CRS 210° · 5.50 km/s`. A
dashed line shows where the target will be 300 game seconds on.

> **Rule 18.2.b** A track's course runs from the earlier POINT to the later one. Its speed is the
> distance over the time between them.

#### COMPASS

Drag out from our ship. It draws a ring round our ship and shows the ring's `RNG` and `BRG`. The
ring moves with our ship.

> **Rule 18.2.c** The compass measures around our ship (owner, 2026-10-17).

#### ERASE

With ERASE on, click a mark to remove it. Press ERASE again to clear every mark and the bearing
trail, and go back to CURSOR.

### 18.3 How you work out a firing solution

This is the loop the tools exist to serve (owner, 2026-10-23).

**1 · Find the range.** A contact you only hear is a bearing line (§9.1). Move your ship. The
bearing trail's lines cross at the contact.

**2 · Find the course and speed.** Drop a POINT where the lines cross. Wait. Drop another POINT on
the contact later. Run a RULER from the first POINT to the second. It reads the course and speed
(§18.2).

**3 · Dial the solution.** Turn a solution knob on Fire Control: RNG (100 km a step), BRG (1° a
step) or SPD (0.25 km/s a step). The first turn starts a hand-dialled solution at 10,000 km, on the
periscope's bearing, at speed 0. The latest track gives it a course and speed.

**4 · Or press TRACK.** Once an active scan has identified the target, TRACK fills the solution
from the sensor lock: its true range, bearing, course and speed. TRACK needs the bridge (§13.3).

**5 · Check the arc.** If the solution's bearing is outside the mount's arc, ANGLE will not throw.
Turn the ship.

**6 · Arm and fire** (§20).

> **Rule 18.3.a** A solution with a course is led. The shot is aimed where the target will be when
> the slug arrives.

> **Rule 18.3.b** A hand-dialled solution aims at a point in space. It needs no identified target.
> Missiles still need one (§8.5).

### 18.4 The targeting computer

**Not built yet.** Today TRACK gives an exact solution from the sensor lock, with no error cone.

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

The slug flies at 2,500 km/s (owner, 2026-10-27). The Garuda cruises at 5.5 km/s; top speed is
12.5 km/s.

| Range | Flight time | A ship at 5.5 km/s moves | A ship at 12.5 km/s moves |
|---|---|---|---|
| 2,000 km | 0.8 s | 4 km | 10 km |
| 10,000 km | 4 s | 22 km | 50 km |
| 25,000 km | 10 s | 55 km | 125 km |

A missile at 100 km/s takes 100 s to cover 10,000 km.

> **Rule 18.6.a** The slug is the fastest thing on the board (owner, 2026-10-27). You scanned the
> target and did the maths, so you know where it is. A slug hits within 100 km (§8.4). In the 10 s
> flight the Garuda moves up to 125 km, so a slug aimed straight at it can miss.

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
drawer closes. Each panel is a prefab (`Assets/UI/Board/README.md`).

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
| Closes on | Its rail key again, or its latch |

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
| **Hangar** | Bay doors, squadron and order, launch bearing, the six launch stages, the lever, RECALL | §12 |
| **Engineering** | The ship picture, the six rooms, repairs | §13 |
| **Comms** | Band, frequency, LOCK, the cipher dials | §10 |

| Station | What you do in it | Section |
|---|---|---|
| **Helm** | Throttle, heading, attitude keys, approach range and mode, stability | §4 |
| **Orders** | The alert condition, the AUX pool, orders | §6 |
| **Periscope** | Slew, MAG, SLEW, CLASS | §11.6 |

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
| New contacts and battle space warnings, when there are any | The alarm strip |
| DRADIS hearing circle and sweep, while DRADIS is on | Plot |
| The scan cone, while ACTIVE is on | Plot |
| PDC arcs, while a group is on AUTO | Plot |

> **Rule 19.4.a** **Our signal is always visible.** The player never has to open a drawer to find
> out how loud they are.

---

## 20 · Fire control

**What it is.** The Fire Control drawer: pick a weapon, set a firing solution, arm and fire. The
whole job is in one drawer, since only one drawer can be out at a time (§19.1.d).

**How it plays.** You pick a weapon and a mount. You set a solution by hand from your plot, or with
TRACK once the target is identified. You throw the arming ladder one rocker at a time. A locked
rocker tells you why it is locked. Then you pull the lever.

**How it's built.** Fire control has a system (RAILGUN, PDC or MISSILE), a mount, one solution per
system, the four-rocker arming ladder, the capacitor and the fire lever. All of it stays when the
drawer closes. Fire control's target is the Garuda, the one enemy. There is no way to pick another
target yet.

> **Rule 20.a** PDCs use no solution and no lever. Their ladder puts them on AUTO (§8.6).

### 20.1 Layout, top to bottom

#### Header

The periscope's bearing, and whether it is on the target's bearing: ON BEARING, or OFF and by how
much.

#### 01 Weapon system

| Control | Shows |
|---|---|
| System selector | RAILGUN, PDC or MISSILE |
| Mount selector | A or B, for the railgun and the PDCs |
| Mount readout | Its name, its arc (`ARC 042° ±80°`, or `ALL ROUND`) and its reach |
| State | `SLUGS 22`; or the PDC rounds and the loaders; or the missile cells |

#### 02 Firing solution

| Control | Shows |
|---|---|
| TRACK key | Fills the solution from the sensor lock |
| Track ID | `TK-048-T07` from a lock, `MANUAL` when dialled by hand, or `NO TRACK` |
| Source | FROM SENSOR LOCK, DIALLED BY HAND, or NOT IDENTIFIED ON SENSORS |
| RNG, BRG, SPD | The solution, with a knob each (§18.3) |

> **Rule 20.1.a** TRACK needs an identified target and the bridge at integrity 3 or more.

#### 03 Capacitor

Cells and a percent (§8.4). The note reads CHARGING AT +n/S — GATE LOCKED, or CHARGED — GATE WILL
ARM, or WEP BUS DEAD — CAPACITOR BLEEDING OFF.

#### 04 Arming ladder

Four rockers, thrown in order:

| Rocker | Throws when |
|---|---|
| LOAD | At least 2 WEP units, the magazine at 3 or more, weapons released (CONDITION 2 or 1), ammunition left, and a solution. For a PDC group: its autoloaders on, in place of a solution |
| ANGLE | The solution's bearing is inside the mount's arc (§8.7). Free for PDCs |
| BEARING | The periscope is within 4° of the target's bearing, or of the dialled bearing. Free for PDCs |
| ARM | The capacitor has the shot's charge: railgun 70, missile 40, PDC 25 |

> **Rule 20.1.b** Each rocker needs the one before it. Releasing a rocker releases it and every one
> after it.

> **Rule 20.1.c** A locked rocker shows why: NOT RELEASED, MAGAZINE HIT, NO WEP BUS, OUT OF ARC,
> CHARGING or LOCKED.

#### 05 Fire lever

Pull the lever past 90% with the ladder set. The railgun fires one slug. The missile launcher fires
a salvo. For a PDC group the lever does nothing.

> **Rule 20.1.d** A status line says what is missing. It shows the first that applies, in this
> order:
>
> `NO TARGET DESIGNATED` → `NO TRACKING ID — NOT IDENTIFIED` → `BRIDGE DAMAGED — PLOT COMPUTER
> OFFLINE` → `NO FIRING SOLUTION` → `WEAPONS NOT RELEASED — CONDITION 2 REQUIRED` → `MAGAZINE
> DAMAGED — NO ROUNDS STRUCK UP` → `PDC AUTOLOADERS OFF` → `ARMING SEQUENCE INCOMPLETE`
>
> The first two are skipped for a hand-dialled solution. With the ladder set it reads `SOLUTION
> LOCKED — MOUNT READY`. After a shot it reads `ROUNDS AWAY`, then `HIT · …` or `MISS`. A PDC group
> on AUTO reads `PDC-A AUTO — WATCHING ITS ARC`, then its last result.

### 20.2 What the panel puts on the plot

Per §19.2.a, everything here draws on the plot too.

| Panel action | Plot shows |
|---|---|
| Fire Control open | The selected mount's reach, a dashed ring round our ship |
| A PDC group on AUTO | Its arc, a dashed sector off our hull |
| Shot fired | The slug in flight, at true speed: a white dot with a short trail, white like our ship and our fighters (owner 2026-10-26) |
| Missile salvo fired | Each missile in flight, also a white dot with a short trail |

### 20.3 State and persistence

> **Rule 20.3.a** Closing the drawer drops nothing. The solution, the ladder and the capacitor's
> charge stay.

| Event | Clears |
|---|---|
| System or mount changed | The ladder |
| Warhead changed | The ladder |
| Solution changed by TRACK or a knob | The ladder |
| Shot fired | ARM only. The solution stays |
| Last round gone | The ladder |
| The ship turns out of arc | Nothing |

> **Rule 20.3.b** Turning the ship after ANGLE is thrown does not release it. You can turn away,
> turn back, and still be loaded.

> **Rule 20.3.c** A solution stays until it is replaced.

### 20.4 Weapons power and releasing

> **Rule 20.4.a** LOAD needs at least 2 WEP units. Below that, LOAD is locked.

> **Rule 20.4.b** The cost is a **threshold**. A loaded weapon uses no units. The charge for each
> shot comes from the capacitor (§8.4).

> **Rule 20.4.c** Losing WEP units after LOAD does not release the ladder. A shot still needs the
> capacitor's charge, and with no WEP units the capacitor loses 3 per game second.

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
| §4.2 Throttle | `W`/`S` set a continuous target while pressed | The throttle is dragged on the helm |
| §8.7 Arcs | `WeaponBase.angleOfFire` is declared and set in two places but never checked. No arc centre exists | ANGLE checks the solution against the mount's arc |
| §13.1 Shields | `Shields.cs` exists with recharge, colours and a renderer | There are no shields. Auxiliary is the recovery resource |
| §9.1 DRADIS | `Radar.cs` places every registered target every frame | DRADIS updates a contact only when the sweep passes it |

---
