# Starwolf — Specification

**Version** 0.2 · 2026-09-02
**Status** Specification. Describes intended behaviour. Not yet implemented.

Why each choice was made, what else was considered, and migration notes live in
`Assets/TACTICAL_BOARD_PLAN.md`. They are deliberately not repeated here.

---

## 1 · Summary

> *The Emperor is dead, betrayed by the traitor lords. You are the Captain, revived from stasis in a
> secret spacedock. You are deep behind enemy lines; nowhere is safe. You command a single ship.
> Carry out the Emperor's final edict: revenge on the traitors.*

**Starwolf is a top-down 2D real-time tactical space combat game.** You command one starship. You
enter a sector of space, hunt down the Traitor Lord's flagship, kill it, and escape.

**The novelty is that you play the entire game through a control panel.** Apart from very select
moments, there is no 3D view of the outside. You read the world through instruments — navigation,
weapons, troops, sensors, signals intelligence — and act through them.

**Your ship is a stealth destroyer.** It relies on patience and position, not a straight fight. You
arrive broadcasting a false ID; every enemy reads you as friendly. The game is about how long you
can keep that lie going, and picking the moment to drop it.

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

## 2 · How it plays

### The shape of a session

`Title → pick a Traitor → pre-launch → hyperspace → the level → escape → credits`

You drop in at one edge of a handcrafted region of space, carrying **old, unverified Imperial
records** that mark a few landmarks. Everything else you have to find.

### Order of operations

From the GDD, in the order you actually do them:

1. **Explore.** Move around. Find the enemy. Find points of interest. Work out where to set up a
   strike. There is no fuel and no clock pressure yet.
2. **Read the world with radar.** Figure out what is going on and decide **what to target**.
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
| **Power is the whole decision** | Ten reactor points across five systems that each want five. You can fully run two. Power in the reactor is invisible; power in a system is not. |
| **Everything you do makes you visible** | `Detection = energy in systems + active ability + environment`. Flying fast and firing both add to it. |
| **You drift** | Turning points the ship. It does not change where you are going. |
| **Your armour is a direction** | No shields. A bow, a stern and two sides, each with its own hit points. |
| **Shots take time** | A railgun slug can be eight seconds in the air. You always shoot at a guess. |

---

## 3 · Features, one at a time

Everything below breaks down system by system. Read in order for the whole picture, or jump.

| § | System | What it covers |
|---|---|---|
| 4 | Conventions | Bearings, units, the update loop |
| 5 | Time | Real time, pause, play rate, the universal clock |
| 6 | Power | The ten points, five systems, draw and vent |
| 7 | Movement | Stepped throttle, drift, stability, collision, autopilot |
| 8 | Being seen | Detection, the disguise, the awareness ladder, the alert net |
| 9 | Auxiliary | Repair, reload, distort, flush, damage control |
| 10 | Reflex | The cloak |
| 11 | Weapons | Three railgun turrets, four PDCs, missiles, arcs, damage |
| 12 | Sensing | The radar pulse, zoom, discovery, the scanner, contact labels |
| 13 | Comms | Tuning, the cipher, what it buys you |
| 14 | The board | Layout, plot, rails, markers, colour, type |
| 15 | Hangar | Starfighters and assault boats |
| 16 | Damage | Four hull sides, internals, kill conditions |
| 17 | Failure | The four ways to lose |
| 18 | Mission | How a mission runs, how you win, the level |
| 19 | Tools | The seven plotting instruments and the firing solution |
| 20 | Panels | How a station panel behaves |
| 21 | Targeting | The one panel specified in full |

> **Note on numbering.** The section numbers in the body below have not yet been renumbered to match
> this table. That is a mechanical pass still to do.

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
dial, wedge and readout agrees with every other one.

**How it's built.** One bearing value per object per frame feeds every heading visual. Distances are
world units. One fixed-order update pass per tick, with a clamped `dt`.

### 1.1 Bearing

The battlespace is a **2D plane**. Positions are `(x, y)`.

- `0°` is `+y`, increasing **clockwise**. `090°` is `+x`.
- Shown zero-padded to three digits: `062°`.
- **Screen angle = bearing − 90°.**

```
angDiff(a, b) = ((a - b + 540) mod 360) - 180        // (-180, 180]
```

> **Rule 1.1.a** Every visual that comes from a heading — hull rotation, heading vector, `HDG`
> readout, arc wedge — comes from **one** bearing value per object per frame.

### 1.2 Units

Distances are **world units**, matching the prototype. Speeds are units per second.

| Prototype scale | Value |
|---|---|
| `SONAR_RADIUS` | 8,000 |
| `RADAR_RADIUS` | 40,000 |
| `MAP_RADIUS` | 50,000 |
| Opening player↔carrier separation | 85,900 |
| `MAX_SPEED` | 240 |

> **Rule 1.2.a** The kilometre figures in the design handoff are for display only. They do not set
> the numbers the game runs on.

### 1.3 Loop

One fixed-order update pass per tick, then one render.

```
dt = min(realDelta, 0.05)          // hard clamp, prototype behaviour
```

> **Rule 1.3.a** `dt` is clamped. A long frame must never move the world a long way in one step.

---

## 2 · Time

**What it is.** Two clock states, running and paused, plus a speed multiplier for how fast the
running clock moves.

**How it plays.** You can stop the world to think and run it fast when nothing is happening. There
is no plan-then-execute step: orders take effect as you give them.

**How it's built.** One clock drives everything. Systems read `dt` and nothing else.

| State | Behaviour |
|---|---|
| **Running** | Simulation advances at `playRate`. |
| **Paused** | Simulation does not advance. |

`playRate ∈ {0.5, 1, 2, 5, 10}` — **tunable**, default `1`.

> **Rule 2.a** Pause and play rate are the **only** time controls. There is no commit or execute
> step. `DIVERGES` from the handoff, which asks for a paused-time commit loop.

> **Rule 2.b** Opening a station panel does **not** pause the game. From the GDD: *"the clock does
> not pause, even when systems are offline or the player is in a menu."* `DIVERGES` from the
> prototype, which pauses on SCANNER and COMMS.

> **Rule 2.e** The clock is universal. It drives enemy movement, ETAs, scanner sweeps and mission
> timing, and every intercepted message quotes it — so arrival times and rendezvous points can be
> backtracked into positions.

> **Rule 2.c** All state comes from `dt` alone. No system may read frame count or wall clock.

> **Rule 2.d** Commanded state (§4.1) can be set while paused. Actual state changes only while
> running.

---

## 3 · Power — the central decision

**What it is.** You have ten reactor points. Five systems want them. Each system can hold five.

**How it plays.** Ten points across five systems that each want five means **you can fully power two
of them.** That is the whole game in one sentence. Every setting is a statement about what you are
doing right now and what you have given up.

**How it's built.** Each system holds a point count `0..5` and one of three states: `standby`,
`draw`, `vent`. Consumers read `points / SYSTEM_MAX`. Signature reads the total (§5.1).

### 3.1 How it plays

**ENGINES · WEAPONS · SCANNER · COMMS · AUXILIARY**, keys `1`–`5`.

> **Rule 3.1.a** Points are **moved, not spent**. The reactor is not fuel you burn — it is attention
> you divide. Points stay where you put them until you move them.

> **Rule 3.1.b** **Every point you allocate makes you louder.** Signature is the sum of all
> allocated points (§5.1). The reactor is both your capability budget and your visibility budget,
> and they move together. Running at ten is the most capable and the most easily seen. Running at
> three is quiet and nearly helpless.

> **Rule 3.1.c** **Moving points takes time.** Taking points out of one system and putting them into
> another means venting and then drawing, and both take seconds. You cannot swap setups mid-fight.
> Picking a setting is a commitment, and being caught in the wrong one is a real way to lose.

> **Rule 3.1.d** **Damage shrinks the pool.** A hurt reactor gives you fewer points, so the puzzle
> tightens as the fight goes badly.

The intended feel: *sensors up and quiet, or weapons hot and seen — and switching costs you the
seconds you least want to spend.*

### 3.2 The numbers

`REACTOR_MAX` 10 · `SYSTEM_MAX` 5 per system.

Point ceiling by reactor health:

| Reactor hp | Ceiling |
|---|---|
| 3 | 10 |
| 2 | 8 |
| 1 | 6 |
| 0 | 4 |

> **Rule 3.2.a** When allocated points sit above the ceiling — because the reactor took damage —
> points are dropped from the **lowest-priority systems first**, last to first in the system order.

### 3.3 Moving points

Each system cycles `standby → draw → vent → standby`.

| State | Behaviour |
|---|---|
| `standby` | Holds what it has. |
| `draw` | Gains one point every `2 × (systems currently drawing)` seconds. |
| `vent` | Sheds one point per second; returns to `standby` at zero. |

> **Rule 3.3.a** **Total intake is fixed.** Drawing on three systems at once does not fill faster
> than drawing on one — it splits the same supply three ways. You choose what fills *first*, not
> how much arrives.

> **Rule 3.3.b** Vent can be cancelled. A vent started by mistake can be stopped.

### 3.4 What a point is worth

```
efficiency(system) = points / SYSTEM_MAX        // 0..1
```

Every consumer reads this ratio. Zero points kills a system dead, except ENGINES, which keeps a
**tunable** crawl minimum so a powerless ship is stranded rather than frozen.

---

## 4 · Movement

**What it is.** A ship you steer by hull angle and a stepped throttle. The hull turns; the velocity
takes time to follow.

**How it plays.** The ship slides. Turning does not turn your movement, it only turns where your
guns and your armour point. Turning hard also drains stability, so it costs you something.

**How it's built.** Every mobile unit holds commanded state and actual state, and actual chases
commanded at a capped rate. Velocity is split into a nose-aligned part and a sideways part each
frame, and the sideways part bleeds off.

### 4.1 Commanded and actual

Every mobile unit holds commanded state (`heading`, `targetSpeed`) and actual state (`heading`,
`velocity`, `position`). Actual moves toward commanded at a capped rate whenever the clock runs.

> **Rule 4.1.a** The same for all unit types. Squadron craft are extra instances of this, not a
> separate control path.

### 4.2 Throttle is stepped

> **Rule 4.2.a** `W` and `S` **step** `targetSpeed` by `SPEED_STEP` (15) per press. Throttle is not
> a held analogue input.

```
targetSpeed clamped to (efficiency(ENGINES)) × effectiveMaxSpeed
```

`MAX_SPEED` 240 · `ACCEL_RATE` 80 · `BRAKE_RATE` 150 · `REVERSE_ACCEL` 55 ·
`REVERSE_SPEED_FACTOR` 0.42.

### 4.3 Drift — the ship slides

> **Rule 4.3.a** `A`/`D` rotate **the hull only**. Velocity keeps its old vector.

Each frame, velocity splits into a nose-aligned part and a sideways part:

```
lateral *= DRIFT_DECAY + 0.006 × efficiency(ENGINES)     // per 1/60 s
velocity *= SPACE_DRAG
```

`SPACE_DRAG` 0.9992 · `DRIFT_DECAY` 0.988.

`Q` / `E` are **held** side thrusters and need at least 1 ENGINES bar.
`LATERAL_ACCEL` 42 · `LATERAL_MAX` 90 · cost 4 stability/s.

### 4.4 Stability

`STAB_REGEN` 10/s when not turning. Turning drains it by speed band: `5.0 / 2.1 / 0.7` × 22 × dt.

> **Rule 4.4.a** A hard turn at speed drains about 110/s against a maximum of 100. **Turning costs
> you something. It is not free.**

### 4.5 Collision

Collision pushes the ship out, reflects velocity × 1.2, costs up to 40 stability, and above 35 u/s
impact deals `round((speed − 30) × 0.25)` hull damage to the quadrant that hit.

### 4.6 Autopilot (combat)

**Owner, 2026-10-25.** A combat autopilot. It only works on a target. Its settings are on the helm.

- **Target:** fire control's selected target. It must be identified by an active scan, because an
  autopilot cannot hold range on a ship you can't see.
- **Facing:** NOSE, STBD, PORT or TAIL (the helm's attitude keys). That side is kept toward the
  target.
- **Range:** the helm's approach range: 2,000, 4,000, 8,000, 12,000, 18,000 or 24,000 km. It tries
  to stay at that range.
- **Speed:** the approach mode. DIRECT, FLANKING and EVASIVE each set a throttle, which is still
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

**What it is.** How visible you are, and what the enemy does about it.

**How it plays.** You are a stealth destroyer, not a brawler. You start the level *disguised*.

**You will be detected.** That is not a failure state and not the end of the mission — it is the
rhythm of the game. You get found, you break contact, you hide, you move, you come back on your own
terms. Going dark and going fast are both answers, and knowing which one a moment calls for is the
skill.

The loop underneath it: **hide → gather → act → get found → break away → gather again.** Every tool
you have serves one of those beats. Scanning and boarding feed the gathering; power and speed and
the gas clouds feed the hiding; the disguise buys you the approach.

### 5.1 The detection number

From the GDD:

```
Detection = energy in systems + active ability + environment effect
```

Worked example, straight from the GDD: 3 points in WEAPONS, firing the gun (+2), sitting in a gas
cloud (−20%):

```
(3 + 2) − 20% × (3 + 2) = 5 − 1 = 4        // rounding up
```

> **Rule 5.1.a** **Power sitting in the reactor cannot be detected.** The reactor is shielded. Only
> power that has been *allocated to a system* shows.

> **Rule 5.1.b** **Using a system costs more than holding it.** Flying fast and firing weapons add
> on top of the allocation.

> **Rule 5.1.c** Environment is a **percentage reduction**, not a subtraction. A gas cloud takes a
> cut of whatever you are putting out, so it hides a quiet ship completely and a loud one only
> partly.

> **Rule 5.1.d** Your signature is always on screen (§19.4.a). Enemy signatures appear on your radar
> the same way yours appears on theirs — it is one system, read from both ends.

### 5.2 The disguise

> **Rule 5.2.a** **The Star Wolf begins the level broadcasting a false ID.** Every enemy reads it as
> friendly or neutral. **All enemy ships are passive at the start.**

> **Rule 5.2.b** Scanning a ship (§9.4) returns its energy signature. **Matching your own output to
> that figure lets you pass as that class of ship** and move in close.

> **Rule 5.2.c** The disguise breaks on any of three things: **getting too close**, **using
> weapons**, or **a failed deception** — an output that does not match what you are pretending to be.

> **Rule 5.2.d** Powering everything down and hiding in a gas cloud or debris field is the other way
> to stay unseen. Disguise is for getting close; hiding is for not being there at all.

This is the centre of the game. The GDD names the fun as *"using that data to disguise yourself as
an enemy ship, hiding in gas clouds or behind asteroids"* — the scan is not reconnaissance, it is
how you get your costume.

### 5.3 Awareness

Once provoked, an enemy climbs a ladder. **It does not come back down.**

| Condition | Raises to |
|---|---|
| `dist < 18000 && sig > 2.0` | 1 — UNEASY |
| `dist < 12500 && sig > 3.0` | 2 — SEARCHING |
| `dist < 8500 && sig > 4.5` | 3 — ALERT |
| Alert active, reinforcements away | 4 — GENERAL ALARM |

> **Rule 5.3.a** Awareness never decreases. You can re-hide, but you cannot make them forget.

### 5.4 The alert net

Firing any weapon triggers it. A 59-second window, then two reinforcements at once, then one every
14 s, alternating DESTROYER / FRIGATE, entering at the arena edge and closing at 92 u/s, ramming for
400 damage.

> **Rule 5.4.a** `TBD` — in the prototype this fires *before* the validity check, so a dry trigger
> pull wakes the alert. Keep or fix.

> **Rule 5.4.b** Reinforcements do not stop. The alert is a **soft fail timer**: once it is running,
> finish and leave.

## 6 · Auxiliary

**What it is.** A charge pool you spend on five fixed actions. It is the ship's repair and recovery
system, and it replaces shields.

**How it plays.** Nothing soaks damage for you, so recovery is something you choose to spend on.
Charge builds slowly, and every action you take is one you cannot take a moment later.

**How it's built.** One charge value `0..100`. It grows each second by a rate looked up from AUX
points. Each key check-and-subtracts its cost, then applies its effect or starts its timer.

> **Rule 6.a** Auxiliary **replaces shields**. Starwolf has no shields. The AUXILIARY power system
> is the defensive and recovery resource.

Charge builds by AUX points at `[0, 2, 5, 9, 14, 20]` per second, capped at 100.

| Key | Action | Cost | Effect |
|---|---|---|---|
| `Z` | Repair | 60 | +1 hp to the highest-priority damaged internal (lifesupport → reactor → engines → sensors → magazine → bridge → crew); if none damaged, +10 to the weakest hull side |
| `X` | Reload | 45 | Clear railgun recalibration / clear missile reload. Fails if magazine destroyed |
| `B` | Signal Distort | 50 | 10 s, signature × 0.5 |
| `N` | Reactor Flush | 40 | Vents one bar from each venting system, −3 s railgun recalibration, 8 s signature × 0.6 |
| `M` | Damage Control | 30 | 20 s: life-support countdown at half rate; internal breach negated 40% of the time |

> **Rule 6.b** Signal Distort (`B`) is the prototype's way of hiding. The Reflex cloak (§7) is an
> owner addition layered on top of it, not a replacement.

---

## 7 · Reflex

`DIVERGES` — an owner addition. No prototype equivalent.

**What it is.** A cloak you hold on, not a timed burst. While it is on you are quiet, and you are
also blind and unarmed.

**How it plays.** You trade information for cover. Everything you knew freezes where it was and
starts to go out of date in front of you, as a circle that grows around each old fix. Coming out is
how you find out what actually happened.

**How it's built.** One engaged flag. On engage, each track stores its last fix — position, speed
estimate and `lastFixTime` — and the sensor pipeline stops writing. Power is drawn from SCANNER.

| Effect | Rule |
|---|---|
| Signature | Drops to `E_floor`, **tunable** |
| Sensors | Dead. No new returns of any kind |
| Weapons | Blocked. No firing, no locks |
| Throttle | `targetSpeed` clamped to `reflexSpeedMax`, **tunable** |
| Power | Draws from SCANNER |

> **Rule 7.a** Reflex hides you and blinds you by the same amount. It trades information for cover;
> it is not free stealth.

On engaging, every track freezes at its last fix:

```
uncertaintyRadius = track.estimatedSpeed × (t − track.lastFixTime)
```

> **Rule 7.b** Frozen tracks draw as the **dashed ghost wedge** (§11.4) at the last known position,
> with a circle of uncertainty that grows where you can see it. No new marker shapes are added.

On disengaging, sensors restart and tracks come back at their true positions. Tracks that are not
picked up again are marked stale and dropped after `staleTrackTimeout` (`TBD`).

---

## 8 · Weapons

**What it is.** Three weapon types and nothing else: three railgun turrets, four PDCs, and missiles.

**How it plays.** Which way you face decides what you can shoot. Bow-on you have two railgun
barrels; stern-on you have one. PDCs are the only answer to a missile salvo, and they only cover
where you have pointed them.

**How it's built.** Every shot is checked against range, arc and line of sight before it is allowed.
Damage runs through one formula, then a penetration-versus-toughness check against terrain.

### 8.1 Damage

```
dmg = round( max(baseDmg, distanceTravelled × perUnit) × typeMod × dmgMod )
```

> **Rule 8.1.a** Kinetic damage **grows with distance travelled**. A railgun slug hits harder the
> further it flies.

Enemy fire is applied at × 0.65.

### 8.1a Damage types

**In use** — from the prototype, which is later than the GDD:

| Type | Modifier | Used by |
|---|---|---|
| **KINETIC** | 0.85 | Railgun |
| **EXPLOSIVE** | 1.20 | Missile |

**From the GDD, historical.** Laser, Plasma and N-Ray are not in the prototype and not in the
current loadout. Kept because N-Ray in particular is a distinct role — it ignores armour entirely
and only matters once a side is already breached — and it may be worth reviving.

| Type | Against armour | PEN | Note |
|---|---|---|---|
| **Kinetic** | 90% | 2 | Weaker against armour, and does not really pass through it |
| **Laser** | 120% | 4 | Burns through armour and reaches key systems easily |
| **Plasma** | 100% | 3 | Average against armour, but **erodes** |
| **N-Ray** | 0% — **ignores hull** | 1 | No effect on armour. Capitalises on existing damage to destroy internals |

**Keywords:**

- **Charge** — mainly the railgun. Its PEN rises as it charges.
- **Erode** — damage over time on the target. **Refreshes and stacks** with more hits.
- **Ignore** — skips the stat completely.

> **Rule 8.1a.a** N-Ray is a distinct role, not a weaker gun: it does nothing to armour and only
> matters once a side is already breached.

### 8.2 Penetration versus toughness

**In use** — the prototype's rule:

```
if pen <= tough:  absorbed
else:             dmgMod *= max(0.2, 1 - (tough / 5) × 0.6)
```

**From the GDD, historical.** Its version scaled the loss by how far PEN beat TOUGH, and let an
equal match through at half damage. That rewarded overmatch, which the prototype's version does not.
`TBD` — worth reviving.

Compare the weapon's PEN against the obstacle's toughness (TOUGH). **Both cap at 5.**

| Difference | Passes? | Damage kept |
|---|---|---|
| `PEN < TOUGH` | No — absorbed | — |
| `PEN = TOUGH` | **Yes** | 50% |
| `PEN − TOUGH = 1–2` | Yes | 50% |
| `PEN − TOUGH = 2–3` | Yes | 75% |
| `PEN − TOUGH > 3` | Yes | 90% |

> **Rule 8.2.a** **Overmatch buys you a cleaner shot.** The bigger the gap between PEN and TOUGH,
> the less the round loses going through — and the further it keeps travelling afterwards with its
> damage intact.

> **Rule 8.2.b** Equal PEN and TOUGH **passes**, at half damage. It is not absorbed.

`OBJ_TOUGH = { asteroid 3, structure 4, ruins 3, starbase 5 }` — `PROPOSED`, the GDD gives no
obstacle values. Starbase 5 is unpassable by anything except a fully charged railgun.

### 8.3 What you carry

Three weapon types. Nothing else. **There are no broadside guns.**

#### Railgun turrets — three

Think of the ship as a rectangle with the bow at the top.

| Turret | Where | Rests at | Movement | Covers |
|---|---|---|---|---|
| Forward left | Front of the spine | `000°` (bow) | ±90° | `270°` → `090°` |
| Forward right | Front of the spine | `000°` (bow) | ±90° | `270°` → `090°` |
| Aft | Stern | `180°` | ±90° | `090°` → `270°` |

Each turret has one barrel and is armed on its own. You arm a turret, get a firing solution, and
fire it.

> **Rule 8.3.a** Turrets are armed one at a time. Arming three takes three times as long as arming
> one.

> **Rule 8.3.b** There is no railgun that fires down the length of the ship. The spine is just where
> two of the turrets sit.

**Coverage goes all the way round, but firepower does not.**

The two forward turrets both cover the front half. The single aft turret covers the back half. Add
them up and you can hit anything — but in front you can bring **two barrels** to bear, and behind you
only **one**.

> **Rule 8.3.c** Facing a target bow-on gives you double the railgun weight of facing it stern-on.

This lines up with the armour. The bow has 50 hull and the stern has 40 (§13.2), so **your strongest
firing side and your strongest armour are the same direction, and so are your weakest.** Turning to
run costs you a gun and shows your thinnest plate at the same time.

#### PDCs — four, one at each corner

Four corners of the rectangle: front-left, front-right, back-left, back-right.

You switch a PDC on and **point its cone** where the missiles are coming from. Once it is on and
pointed, it shoots anything that enters the cone by itself. You do not fire it.

| Setting | Starting value | What it does |
|---|---|---|
| Cone width | ~80° | How wide an area one PDC watches |
| Traverse | ~100° | How far you can swing the cone from its resting angle |
| Rest angle | Points out from its corner | Where the cone sits by default |

Both numbers are **inspector controls** and are expected to be tuned by hand.

> **Rule 8.3.d** A PDC's cone is aimed by the player and can be moved within its traverse limit.
> This is what makes it different from a railgun: you aim the *area*, not the shot.

> **Rule 8.3.e** Traverse is limited on purpose. A PDC cannot cover the whole ship. Which corner a
> missile comes at matters.

> **Rule 8.3.f** A PDC that is off does nothing. Being caught with your PDCs off, or pointed the
> wrong way, is how a salvo gets through.

Usually one or two are on at a time. Ranges and fire rates are in §8.6.

#### Missiles

Fire in any direction. No arc. Numbers in §8.5.

### 8.4 Railgun

A **travelling slug** at 4,800 u/s over 38,000 u.

> **Rule 8.4.b** **One shot per arming, per turret.** An armed turret holds exactly one slug. Firing
> empties it and it must be armed again. The limit is per turret, not per ship — three armed turrets
> can have three slugs in the air at once.

- Charging needs `speed <= 8` and **stops if you accelerate**.
- 4 s charge maps to `pen = 1..5`.
- Firing sets **every powered system to vent** and starts a 6 s recalibration.

> **Rule 8.4.a** The railgun asks you to be almost stopped and leaves you powerless afterwards.
> That cost is the point.

### 8.5 Missile

3 s lock · 320 u/s · 2.2 rad/s turn · 12,000 u range · 8 s reload · flat 42 damage.

### 8.6 PDC

Toggle. Kills the nearest fighter within 2,600 u, otherwise shoots down the nearest enemy
projectile within 1,800 u.

```
cooldown = max(0.12, 0.38 - 0.045 × points)      // fighters
cooldown = max(0.14, 0.42 - 0.05  × points)      // projectiles
```

> **Rule 8.6.a** PDCs are the only defence against a missile salvo. Nothing soaks the damage.

### 8.7 Arc validity

```
valid(track, mount) =
       track.range <= mount.range
   AND |angDiff(track.bearing, mount.arcCentre)| <= mount.halfArc
   AND lineOfSight(own.position, track.position)
```

> **Rule 8.7.a** Arc centres are **fixed to the hull**. Rotating the ship rotates every arc.

> **Rule 8.7.b** Having nothing you can shoot at is a normal state, not an error. It means turn the
> ship.

> **Rule 8.7.c** Picking a mount recomputes the valid set at once. Valid tracks get designation
> brackets and the count reads `n IN ENVELOPE`. Clicks outside the valid set do nothing.

---

## 9 · Sensing

**What it is.** How you find things: a radar that pulses, a close-in discovery range, and a scanner
minigame that reads a ship in detail.

**How it plays.** You never quite have a live picture. Radar gives you a snapshot every three
seconds and everything between pulses is old. Getting more than a blip costs you time at the
scanner, and that scan is what unlocks boarding.

**How it's built.** Radar rebuilds its blip set on an interval and re-projects the stale set every
frame. Discovery is a distance test. Scan progress is a `0..1` value with a moving hit zone.

### 9.1 Radar is a discrete pulse

> **Rule 9.1.a** Every `RADAR_INTERVAL` (3.0 s; loadout tunes 2.4–3.4) radar rebuilds its blip set
> as a **snapshot** of world positions. Between pulses those positions are **old**.

> **Rule 9.1.b** Old blips are **re-projected every frame through the live ship transform**, so
> contacts lag while your own ship's movement stays smooth.

This is the most distinctive sensor behaviour in the build and must be kept.

### 9.2 How things are drawn as you zoom

There is one plot. You zoom it in and out.

> **Rule 9.2.a** Everything sits at its **true position**. Distance on screen always matches real
> distance from you, at whatever zoom you are on.

> **Rule 9.2.b** Symbols **shrink as you zoom out, but only down to a minimum size.** Once a symbol
> hits that minimum it stops shrinking, so a distant contact is still visible instead of vanishing
> into a dot. The minimum is **tunable**.

> **Rule 9.2.c** Text never shrinks. Labels hold the same size at every zoom level.

Reference radii from the prototype, useful as starting numbers: close range 8,000 · radar range
40,000 · whole sector 50,000.

### 9.3 Discovery

An object joins the discovered set at `dist < SONAR_RADIUS + obj.radius × 0.5`.

> **Rule 9.3.a** Big things show up sooner than small ones. Once you have found something, you keep
> it — discovery never resets.

### 9.4 Scanner

A marker slides `0→1` at 0.85/s. `F` inside the zone gives `+0.22` progress, moves the zone to a new
random centre with a narrower half-width `max(0.08, 0.14 − progress × 0.10)`, and speeds the marker
up by `+0.04`. A miss costs `0.04`. Passive trickle `0.005 × (points + loadoutBonus)` per second.

Range by points: `[0, 6500, 8500, 11000, 13500, 16000]`.

> **Rule 9.4.a** The minigame **gets harder as it succeeds**.

Finishing a scan writes hull, crew, heading, signature and all seven internals, then a 12 s cooldown.

> **Rule 9.4.b** A finished scan sets **boarding eligibility** (§12.2).

### 9.5 Contact classification

> **Rule 9.5.a** The label text **is** the knowledge state. One function produces it, and every part
> of the UI shows what that function returns.

```
destroyed         → CARRIER WRECK
scanned           → VAIA ASURAN // COMMAND CARRIER
cipher decoded    → COMMAND CARRIER
within 8,000      → LARGE WARSHIP RETURN
within 40,000     → awareness >= 1 ? LARGE MASS RETURN : DISTANT MASS RETURN
otherwise         → UNKNOWN
```

---

## 10 · Comms

**What it is.** Two puzzles in a row — tune a signal, then break its cipher — that together tell you
roughly where the enemy is.

**How it plays.** You start 85,900 units from the carrier. Comms is how you find it without flying
the whole way. You tune the signal by ear, decode the message by hand, and get an approximate
marker, not a live track.

**How it's built.** A three-stage state machine. The signal spawns at mission time 10 s. Tuning
holds a channel and a frequency; the cipher holds four wheel positions. Success sets
`enemyDiscovered` and drops a one-shot world marker.

**Tuning.** Channel 1–3 (correct: 2) plus frequency 1–100 (target 73, tolerance ±2), `A`/`D` at
28/s. Feedback is a beep whose gap closes from 1.05 s to 0.10 s and whose pitch rises as you get
closer.

> **Rule 10.a** The signal is tunable **by ear alone**. Audio feedback is a mechanic, not
> decoration.

**Cipher.** A real Vigenère over `CARRIER NORTHSKY REPORTS GRID 09F VECTOR NORTH WEST`, key `WOLF` —
the ship's own callsign, hinted as `HINT: CALLSIGN`. Four wheels; `Q`/`E` select, `A`/`D` rotate,
`F` submits. The preview turns white the instant the guess is right.

**Payoff.** Sets `enemyDiscovered` whatever the range, and drops a `PROBABLE CARRIER ZONE` marker at
`enemy + (900, −700)` with radius 2,600.

> **Rule 10.b** The marker is **deliberately rough** and is a one-shot snapshot. It does not follow
> the target.

> **Rule 10.c** Comms is the only way to find the enemy without flying close enough to see it.

---

## 11 · The board

`DIVERGES` — the prototype uses four fixed panels plus seven swappable centre views. The board
replaces that with one plot and two icon rails.

**What it is.** The whole screen: a status bar, one big plot, an order row, a hardware row, and two
rails of icons down the sides.

**How it plays.** You look at one picture the whole game. Panels slide over the edges of it but the
battlespace is always there. Everything you do reads back on the plot.

**How it's built.** A fixed 1920 × 1080 layout of four horizontal bands. World geometry scales with
zoom; text and markers counter-scale by `1/zoom` so they stay readable.

### 11.1 Layout

Design resolution 1920 × 1080. All bands share a 104px side margin, `x104 → x1816`.

| Band | Y | Height |
|---|---|---|
| Status bar | 26 → 88 | 62 |
| Plot well | 100 → 834 | 734 |
| Order row | 844 → 944 | 100 |
| Hardware row | 954 → 1058 | 104 |

> **Rule 11.1.a** The plot takes about 68% of board height. It is the subject; everything else comes
> second.

> **Rule 11.1.b** No cockpit, no outside camera. Station panels sit over the plot; they never replace
> it.

### 11.2 Plot

`#0A1719`. Grid `60.9 × 58.2px` at `rgba(63,169,201,.065)`. Axis labels pinned to all four edges.

Zoom: scroll wheel, `× 1.12` per notch, clamped `0.5 – 3.0`, 2 dp.

> **Rule 11.2.a** World geometry scales with zoom. **Text and markers counter-scale by `1/zoom`.**

### 11.3 Rails

64px wide, 52 × 52px cells, 5px gaps, centred on the plot midpoint. Left at 26px, right mirrored.

> **Rule 11.3.a** The two rails are exactly the same size and spacing. They must stay that way.

**Left — instruments, no consequence.** Five modes — POINT · LINE · RULER · COMPASS · ERASE — of
which one is active at a time, plus two buttons, UNDO and CLEAR ALL (§18.b, §18.c).

**Right — systems, with consequence.** Targeting · Sensors · Reactor · Hangar · Engineering ·
Comms · Forward screen · Reflex.

> **Rule 11.3.b** A lit icon means "this will do something."

### 11.4 Markers

> **Rule 11.4.a** **Shape says what class it is. Colour says whose side it is on. A shape is never
> used for two different sides.**

Filled wedge — own ship · Outlined wedge — classified hostile · Diamond outline — unclassified
hostile · Triangle — friendly craft · Filled diamond — own deployed asset · Circle outline —
unresolved return · Dashed wedge — predicted or last-known position.

> **Rule 11.4.b** Vector length shows speed at a fixed scale across all tracks.

### 11.5 Colour

| Token | Hex | States |
|---|---|---|
| `hostile` | `#E04B3C` | Enemy tracks, threat vectors, breach, targeting |
| `friendly` | `#4A9FD8` | Own craft, comms, hangar |
| `own-systems` | `#8FC33A` | Fire arcs, reactor, measurement |
| `orders` | `#E8A33D` | Commanded state, target brackets, deployed assets |
| `orders-bright` | `#F0A020` | Active interlocks, on-bearing |
| `unclassified` | `#9B7FD4` | Unresolved returns |
| `sensor` | `#3FA9C9` | Range rings, scan envelopes, grid |
| `own-ship` | `#E8EDF0` | Own hull, heading vector, primary values |

> **Rule 11.5.a** Colour is a code. New hues may not be added without extending the code.

> **Rule 11.5.b** Commanded heading draws in `orders`; actual heading in `own-ship`. Both draw at
> once whenever they differ.

### 11.6 Forward screen

Draws the designated target as a **grainy negative image** — inverted, grain, scanlines.

> **Rule 11.6.a** The forward screen is the **only** place where damage can be seen. The plot glyph
> never shows damage; it stays a symbol.

> **Rule 11.6.b** It does no targeting.

### 11.7 Typography

**Barlow Condensed** (400/500/600) — titles, names, labels, designators.
**Share Tech Mono** — *all* numerics, bearings, ranges, timers, ticks.

> **Rule 11.7.a** Labels uppercase. No italics. Units set at the same size as their number.
> **Numerics never animate their digits — they snap.**

### 11.8 Hardware row

Power lamp · brightness knob · grid-scale knob driven by live zoom · `PAUSE` · play-rate knob.

> **Rule 11.8.c** There are no time-nudge keys. An earlier draft drew `−10s / −1s / +1s / +10s`;
> they contradict Rule 2.a and are removed.

> **Rule 11.8.a** No EXECUTE. No EMERGENCY ABORT.

> **Rule 11.8.b** Hazard striping is kept for actions you cannot undo, and is currently unused. It
> may not be used for decoration.

---

## 12 · Hangar

`DIVERGES` — the prototype fires boarding pods like a weapon. Starwolf drops those and does
everything through the hangar instead.

**What it is.** Where you launch starfighters and assault boats.

**How it plays.** You give craft a target and a job and let them get on with it. Boarding is the
payoff for having scanned something, so the scanner minigame and the hangar are linked.

**How it's built.** Starfighters are a count, not individual craft. Assault boats are two objects
with a target and an objective each. All of them move with §4.1. Launch and recovery draw from BAY
power.

> **Rule 12.a** Nothing in the hangar is flown by hand. Designate a target, give it a job, launch.
> Craft carry it out on their own using §4.1.

### 12.1 Starfighters

Held as a **count**, not as individual craft.

`squadronSize` (`TBD`, 5–10) · `fuel` · `ammo` · `healthPerCraft` · `dpsPerCraft`. Squadron output
scales with how many are still alive.

**Loadout**, fixed at launch: anti-hull lasers, or bomber.
**Roles:** intercept hostile craft; strike hostile ships.

### 12.2 Assault boats — marines

Two boats, each carrying a squad of marines. Both can go out at once.

**What they are for.** Marines are how you reach inside another ship. Boarding is not a way to kill
something — it is a way to **change what that ship is**, and to **come back with what it knows**.

| Order | What the marines do |
|---|---|
| **Demolition** | Blow something up. Take a system off the board. |
| **Sabotage** | Leave it running but broken — engines, weapons, sensors, comms. |
| **Grab intel** | Come back with what that ship knew. |
| More | `TBD` — the GDD's "a whole slew of other stuff". Capture, prisoners, planting a beacon. |

> **Rule 12.2.a** **Intel is the reason to board.** What the marines bring back feeds the rest of the
> game — where the flagship is, how it is guarded, and the energy signatures you need to keep your
> disguise working (§5.2).

> **Rule 12.2.b** Boarding is the deep end of the same loop as scanning. A scan reads a ship from
> outside; marines read it from inside and can change it while they are there.

> **Rule 12.2.c** **There are no boarding pods.** Boarding runs through the hangar. A boat launches,
> flies to the target on its own, and its marines do the work. The prototype's boarding-pod weapon
> is dropped.

> **Rule 12.2.d** A target must have been scanned (§9.4) before a boat can be sent.

| Property | Behaviour |
|---|---|
| Target | Required, one per boat |
| Eligibility | Must have been scanned first |
| Order | Set per boat, from the table above |

Launch and recovery draw from **AUXILIARY**.

---

## 13 · Damage

**What it is.** Four hull sides and seven internal systems per ship. No shields.

**How it plays.** The side you present is your armour, so which way you face is a defensive
decision as well as a firing one. Damage does not just take a number down — it takes systems away.

**How it's built.** Four hull pools plus seven internal health values. An impact direction picks one
side. Each internal applies a multiplier to the system it controls. Death is a set of conditions,
any one of which is enough.

### 13.1 No shields

> **Rule 13.1.a** There are no shields. Hull is the only pool. Auxiliary (§6) is the recovery
> resource.

### 13.2 Four hull sides

Player: **prow 50 · port 50 · starboard 50 · aft 40** (190 total) — the prototype's values, which
are later than the GDD's 40/50/50/10.

> **Rule 13.2.d** `TBD` — the GDD made the stern a **glass jaw** at 10, a quarter of the sides. The
> prototype softened it to 40. The thin-stern version is what makes facing a real decision, so this
> is worth a deliberate call rather than defaulting to the newer number.

> **Rule 13.2.b** **The stern is a glass jaw on purpose.** Aft armour is a quarter of the sides, and
> the reactor and engines sit directly behind it. Turning to run puts your thinnest plate over your
> most critical systems.

**What sits behind each side:**

| Side | Internals exposed once breached |
|---|---|
| Prow | Bridge, Sensors |
| **Aft** | **Reactor, Engines** |
| Starboard | Magazine |
| Port | Life Support, Crew |

> **Rule 13.2.c** Once a side is breached, damage through it hits one of that side's internals **at
> random**. Damage to a breached prow hits either the Bridge or the Sensors.
Carrier: prow 90 · port 110 · starboard 110 · aft 80 (390), crew 70.

```
hitSide(impactDirection) = the side whose outward normal is most opposed to impactDirection
```

> **Rule 13.2.a** **The hull side you present is the only armour.** Facing is the defensive decision
> and the reason autopilot takes a `facing` setting (§4.6).

### 13.3 Internals

Seven per ship, each with its own effect. **These are not all multipliers.**

| Internal | What breaking it does |
|---|---|
| **Bridge** | Movement lag or loss of steering, **and loss of access to the navigation panel** |
| **Magazine** | Damages guns, missiles, PDCs and pods |
| **Life Support** | **Starts a game-over timer** unless repaired |
| **Engines** | Reduces max speed, increases turn time |
| **Reactor** | Limits total power, and **drains power from a random active system** |
| **Sensors** | Disables the scanner, reduces radar range |
| **Crew** | **Disables abilities** |

> **Rule 13.3.a** Three of these are lockouts, not degradations — a dead Bridge takes the navigation
> panel away, a dead Crew takes abilities away, and a dead Reactor actively steals power. They cannot
> be modelled as a single efficiency ratio.

### 13.4 Kill conditions

Any one of these kills a ship — hull ≤ 0, **or** reactor destroyed, **or** bridge + engines
destroyed, **or** crew ≤ 0, **or** all internals destroyed.

### 13.5 Rendering

> **Rule 13.5.a** Damage draws on the forward screen only (§11.6.a).

---

## 14 · Failure

**What it is.** Four named ways to lose.

**How it plays.** Losing is not only about hull. A clock can kill you, and so can flying away from
the fight, which keeps the pressure on even when nothing is shooting at you.

**How it's built.** Four checks run each tick. Two read a value, two read a countdown. Any one of
them ends the mission.

> **Rule 14.a** `DIVERGES` — the prototype **has no way to lose**. `lifeSupTimer` counts up and is
> printed but is never compared against `LIFE_SUPPORT_TIME`; there is no defeat state and no code
> path reads player hull for a loss check.

Starwolf has named ways to lose, following the Unity build — the one axis on which Unity is ahead:

From the GDD's own rules slide: **"You win by successfully shooting the enemy ship."** Three ways to
lose.

| Condition | Trigger |
|---|---|
| **Die in battle** | Player hull reaches zero |
| **Desert the battlespace** | Outside the playable area past the warning timer. The player is **charged with cowardice and the ship self-destructs** |
| **Run out of time** | Mission timer expires |

Plus one that arrives through damage rather than as a top-level rule: a destroyed **Life Support**
starts a game-over countdown unless repaired (§13.3).

---

## 15 · Mission

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
| **Eliminate target** | The flagship is confirmed on radar | It is destroyed |
| **Exit region** | The flagship dies | You reach the jump point and jump out |

> **Rule 15.2.a** **Killing the flagship is not winning.** When it dies, an extraction marker appears
> at a far edge of the map. **The mission ends only when you reach it and jump out.**

> **Rule 15.2.b** The escape is played under whatever heat you generated getting the kill. Anything
> still alive is still hunting you.

> **Rule 15.2.c** There are no cutscenes. You reach the edge and disappear.

### 15.3 The level

> **Rule 15.3.a** **There is no true deep space.** Every level is full of structure — the rings of a
> shattered moon, a dead fleet's wreckage, a broken starbase. These are functional: they change radar
> returns, scanner noise and where you can move.

You arrive with **old, unverified Imperial records** marking a few landmarks — a listening post, an
abandoned dock, a comms tower. They must be scanned or approached to confirm.

> **Rule 15.3.b** The first phase of every mission is **exploration**, not combat. There is no fuel
> mechanic; you are free to wander.

> **Rule 15.3.c** The flagship is hidden and protected by forces and misdirection. Finding out
> *where it is and how it is guarded* is the puzzle.

### 15.4 Loadouts

| Loadout | railgun | missile | hull/side | scanner | radar interval | aux start |
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
- A comment claims nebulae hide themselves from radar; the loop pushes them anyway.
- A comment describes last-known-position display for the enemy; no such system exists — once
  discovered, the carrier draws live forever at any range. **§7 builds what this comment
  describes.**
- `extractionDiscovered` is set and never read.
- MAP passes a white CRT tint despite a comment asking for amber.
- Neutral ships are decoration — left out of collision and targeting; the briefing's civilian
  objective has no implementation.
- `.barrel.loading` has CSS and render support but is never assigned.
- Reactor-damage cells write inline styles that later class writes cannot clear.
- The nebula is a hard on/off at the circle edge — no falloff, no hysteresis.

---

## 17 · Open items

**Blocking implementation**

1. **The win condition.** §14 gives four ways to lose and the document gives no way to win. See §15.
2. **§8.4 vs §18.5** — does firing one railgun really vent every system? If so a three-turret volley
   is impossible and §18.5 is wrong. If not, say what firing actually costs.
3. **§3.3** — where does a drawn point come from? Is there an unallocated pool, must a point be
   vented from elsewhere first, and what happens if you press draw with all ten already placed?
4. **§4.4** — what happens when stability reaches zero? Four sections spend numbers on it and none
   says what it is for. The speed bands behind `5.0 / 2.1 / 0.7` are also undefined.
5. **§21** — PDC aiming. Rule 20.b forwards to a section that does not exist.
6. **Missiles have no control surface.** §8.5 stats them; nothing says where they are fired from.
7. **`minArmPower`** (§20.4.a) has no value.
8. **§11.2** — nothing maps world units to plot pixels, so no one can tell what zoom 1.0 shows.

**Non-blocking**

9. `E_floor`, `reflexSpeedMax`, `staleTrackTimeout` (§7).
10. Squadron size, fuel and ammo scales (§12.1).
11. Whether a dry trigger pull should raise the alert (§5.3).
12. Terrain: hulk destructibility, and partial line-of-sight for clouds and debris fields.
13. Enemy AI is unspecified in every document, including the GDD.

## 18 · Plotting tools and the firing solution

The left rail holds seven tools (§11.3). They draw on the plot and change nothing in the world.

**What it is.** Seven drawing and measuring tools — POINT, LINE, RULER, COMPASS, ERASE, UNDO, CLEAR
ALL — and the firing solution they exist to help you build.

**How it plays.** You work the target out yourself. Drop marks, wait, drop more, measure between
them, and read off the target's course and speed. The computer will do this for you, but only as
well as your sensor picture allows, so plotting by hand is the fallback when you have spent your
power elsewhere.

**How it's built.** Marks are stored in world coordinates with a mission-time stamp. Exactly one
tool is active. The ruler computes range, bearing, closing speed and time of flight from your ship
and the cursor. The targeting computer produces an aim point plus an error cone whose width comes
from four sensor inputs (§18.4).

> **Rule 18.a** The tools are **measuring instruments**. They never move the ship, fire anything, or
> change a contact. Marks made with them are the player's own notes.

> **Rule 18.b** Five of the seven are **modes** — POINT, LINE, RULER, COMPASS, ERASE. Exactly one
> mode is active at a time.

> **Rule 18.c** UNDO and CLEAR ALL are **buttons, not modes**. Pressing one acts immediately and
> leaves the active mode alone.

> **Rule 18.c** Marks stick to **world positions**, not screen positions. Zoom and pan move them
> with everything else.

### 18.1 The three angles you need

These are three different numbers and the player needs all three:

| Angle | What it means | What it is for |
|---|---|---|
| **Bearing** | The compass direction from you to the target. `0°` is up. | Where it is |
| **Relative bearing** | Bearing minus your own heading. Where the target sits compared to your nose. | **Whether a turret can reach it** — arcs are measured from the hull (§8.7.a) |
| **Angle on bow** | The angle between the target's own heading and the line from it back to you. | **What aspect it is showing** — coming at you, running away, or crossing |

Angle on bow is the one that tells you how much to lead. A target coming straight at you needs
almost no lead. A target crossing your line of sight needs a lot.

### 18.2 The tools

#### POINT

Click to drop a marked point.

Each point shows its **bearing and range from your ship**, and **stamps the mission time it was
placed**.

> **Rule 18.2.a** Points record a timestamp. That is what makes a target's speed measurable — see
> §18.3.

#### LINE

Click twice to draw a straight segment. Reports **length** and **bearing** along it.

Use it to join two things, project a course forward, or mark a line you do not want to cross.

#### RULER

Drag from your ship to a target. Reports four values at once:

| Value | Meaning |
|---|---|
| `RNG` | Distance |
| `BRG` | Bearing |
| `ΔV` | Closing speed — how fast the gap is shrinking or growing |
| `TOF` | Time of flight — how long a railgun slug would take to cover that distance |

> **Rule 18.2.b** `TOF` is the firing tool. At 4,800 u/s a slug takes nearly **8 seconds** to cross
> maximum range (§8.4), and a target at full speed moves roughly 1,900 units in that time. You
> cannot aim at where something is.

#### COMPASS

Place the centre on a contact. It draws a bearing ring around that contact and reports:

- The contact's **relative bearing** from your hull
- The contact's **angle on bow**, if its heading is known

> **Rule 18.2.c** The compass is how you read aspect. It does the job of the angle-on-bow dial in a
> submarine's fire control.

#### ERASE · UNDO · CLEAR ALL

- **ERASE** removes the one mark under the cursor.
- **UNDO** removes the most recent mark.
- **CLEAR ALL** removes everything. It asks first.

### 18.3 How you actually work out a firing solution

This is the loop the tools exist to serve.

**1 · Find the target's course and speed.** Drop a POINT on the contact. Wait. Drop another POINT on
it later. Draw a LINE between the two marks.

- The **bearing of that line** is the target's heading.
- The **length of that line, divided by the time between the two stamps**, is the target's speed.

> **Rule 18.3.a** This is the manual route. You do not have to use it — the targeting computer will
> estimate course and speed for you (§18.4) — but when its picture is poor, plotting by hand beats
> it.

**2 · Check it is reachable.** Use the COMPASS to read relative bearing. If it falls outside the
selected turret's arc, there is no solution until you turn the ship.

**3 · Read range and flight time.** RULER from ship to target.

**4 · Let the targeting computer solve it.** Select a turret and designate the target. The computer
takes range, bearing, target speed and target heading and produces an **aim point** — where the slug
and the target will meet.

### 18.4 The targeting computer

You are a starship, not a submarine. The computer predicts. What it cannot do is predict from
information you have not gathered.

> **Rule 18.4.a** **The prediction is only as good as the sensor picture.** The computer always
> gives you an answer. How tight that answer is depends on what you have spent to get it.

Four things drive how good the solution is:

| Input | Effect on the solution |
|---|---|
| **SCANNER power** | More points, tighter prediction |
| **Time since the last radar pulse** | Radar is a pulse every ~3 s (§9.1). A fresh fix is sharp; an old one has drifted |
| **Track confidence** | A well-established track predicts better than a new return |
| **Whether the target has been scanned** | A scanned target's speed and heading are known rather than guessed |

The computer reports how unsure it is as an **error cone** on the plot — narrow when the picture is
good, wide when it is not.

> **Rule 18.4.b** The error cone is always shown. The player can see how confident the computer is
> before committing a shot.

> **Rule 18.4.c** Firing into a wide cone is a fair choice, not a mistake. It is a low-odds shot,
> and sometimes a low-odds shot is the one you have.

**Where the manual tools still earn their place.** When the computer's picture is poor — scanner
starved, radar off, target unscanned — you can plot the target yourself with POINT and LINE and
**beat the computer's estimate by hand**. Entering your own figures overrides its guess.

> **Rule 18.4.d** This is the real decision: **spend reactor points on SCANNER for a good automatic
> solution, or spend them elsewhere and do the plotting yourself.** It ties the firing problem
> straight back to the reactor allocation in §3.

> **Rule 18.4.e** A solution goes out of date. If the target changes course or speed after the last
> fix, the aim point is wrong. The error cone widens as the fix ages, which is the only warning you
> get.

### 18.5 Volleys

> **Rule 18.5.a** **A volley is not a mode you select. It is what happens when you have set up more
> than one turret.** There is no salvo button, no spread control, and no fire-pattern menu.

Each turret holds one shot and carries **its own bearing**. What you get depends entirely on the
solutions you built:

| What you set up | What you get |
|---|---|
| Three turrets, **the same bearing** | Three shots converging on one point. The most damage available — and if the estimate is wrong, all three miss together |
| Three turrets, **slightly different bearings** | A fan across where the target might be. At most one lands, but a bad estimate no longer costs you everything |
| One turret armed, fire, observe, arm again | The slow way. Each shot teaches you something the next one uses |
| Two armed, one held back | A shot kept in reserve |

> **Rule 18.5.b** A "spread" is simply **different bearings entered on different turrets**. The
> width of the fan is the difference between the numbers you typed. Nothing computes it for you.

> **Rule 18.5.c** Each turret is armed separately and arming takes time (§8.3.a), so setting up
> three turrets costs three times as long as one. **The decision to volley is made before the shot
> presents itself**, not when it arrives.

> **Rule 18.5.d** Only the two forward turrets bear forward, so a two-shot volley is the most
> available ahead and a single shot is all there is astern (§8.3.c).

> **Rule 18.5.e** Turrets fire together when more than one is armed and ready. To fire one at a
> time, arm one at a time.

### 18.6 Flight time changes the weapon

The slug travels at 4,800 u/s.

| Range | Flight time | How it plays |
|---|---|---|
| 2,000 | ~0.4 s | Effectively instant. Point and shoot |
| 12,000 | ~2.5 s | Lead matters. A poor estimate misses |
| 38,000 | ~7.9 s | A prediction problem. The target may change course mid-flight |

> **Rule 18.6.a** The same weapon plays differently at different distances. Close in it is a duelling
> gun; at maximum range it is guesswork about the future.

### 18.7 What the tools do not do

> **Rule 18.7.a** No tool follows a contact. A POINT dropped on a moving ship stays where it was put
> — that is the whole reason it can be used to measure movement.

> **Rule 18.7.b** No tool reveals anything. Marking an unknown return does not identify it.

---

## 19 · Station panels

The right rail holds the ship's systems (§11.3). Clicking an icon opens that system's panel.

**What it is.** Seven sliding panels, one per ship system, plus two rail icons that open nothing.

**How it plays.** You work a system in its panel and read the result on the plot. Only one panel is
open at a time, so choosing what to look at is itself a cost — and the world keeps moving while you
look.

**How it's built.** A single open-panel slot. Each panel is docked to its rail, 376 px wide, full
plot height, and slides in over 0.42 s. Panel state survives closing. Every panel writes its own
overlay to the plot.

### 19.1 How a panel behaves

> **Rule 19.1.a** A panel **slides in from the side it is docked to**. It does not pop, fade in, or
> appear centred.

> **Rule 19.1.b** Panels open over the plot. **The plot is never covered entirely and never
> replaced.** You can always see the battlespace.

> **Rule 19.1.c** Opening a panel **does not pause the game**. The world keeps moving while you work.

| Property | Value |
|---|---|
| Docked to | The rail the icon sits on — right rail panels slide in from the right |
| Width | 376 px |
| Height | Full plot height, `y100 → y930` |
| Slide time | 0.42 s |
| Easing | `cubic-bezier(.2, .85, .25, 1)` — fast out, settles slowly |
| Closes on | The panel's own `×`, clicking its rail icon again, or `Esc` |

> **Rule 19.1.d** **One panel at a time.** Opening a second closes the first. There is no stacking
> and no dragging panels around.

> **Rule 19.1.e** The open panel's rail icon shows an open state — an outline in the system's colour
> and a bar down its left edge.

> **Rule 19.1.f** A panel keeps its state while closed. Reopening it returns you to what you were
> doing, not to a reset.

### 19.2 Panels do not steal the plot

> **Rule 19.2.a** Whatever a panel does, **its result is drawn on the plot**, not only inside the
> panel. Selecting a turret draws its arc on the plot. Switching radar on draws its ring on the plot.
> Aiming a PDC draws its cone on the plot.

The panel is where you work the system. The plot is where you read it.

> **Rule 19.2.b** A panel may dim the part of the plot directly behind it, but nothing important may
> be placed there while it is open.

### 19.3 The panels

Seven panels, plus two rail icons that are not panels.

| Panel | Colour | What you do in it | Status |
|---|---|---|---|
| **Targeting** | `#E04B3C` | Pick a turret, designate a target, set the solution, arm, fire | Spec'd — §8, §18 |
| **Sensors** | `#3FA9C9` | Radar on/off, aperture, scan a contact | Spec'd — §9 |
| **Reactor** | `#8FC33A` | Move the ten points between the five systems | Spec'd — §3 |
| **Hangar** | `#4A9FD8` | Launch starfighters and assault boats, give them targets and jobs | Spec'd — §12 |
| **Comms** | `#4A9FD8` | Tune the signal, break the cipher | Spec'd — §10 |
| **Engineering** | `#E8A33D` | Hull by side, internals, auxiliary actions | `TBD` |
| **Forward screen** | `TBD` | Watch the target and its damage | Spec'd — §11.6 |

> **Rule 19.3.a** **Reflex has no panel.** It is a single on/off with no settings — the rail icon is
> the whole control. Clicking it turns the cloak on or off at once. The icon lights while it is on;
> everything else about the state reads on the plot (§7.b).

> **Rule 19.3.b** **The left-rail tools have no panels.** Clicking a tool icon makes it the active
> tool and nothing opens. You then work directly on the plot — click to place, drag to measure. Any
> readout a tool produces is drawn on the plot beside the mark, not in a panel (§18).

### 19.4 What is always visible, without a panel

Some things are too important to hide behind a click.

| Always on screen | Where |
|---|---|
| Hull by side | Status bar |
| Alert state | Status bar |
| Mission time | Status bar |
| Reactor points allocated | Status bar |
| Own heading, speed, signature | Order row |
| Radar ring, when radar is on | Plot |
| PDC cones, when a PDC is on | Plot |
| Selected turret's arc | Plot |
| The distance at which your current signature would trip the enemy's first awareness gate | Plot, as a ring |

> **Rule 19.4.a** **Signature is always visible.** The player must never have to open a panel to
> find out how loud they are.

### 19.5 Rail icons that are not panels

| Icon | Behaviour |
|---|---|
| **Reflex** | Toggles the cloak on click. No panel. Lights while it is on. |
| **All seven left-rail tools** | Selects the active tool on click. No panel. You work on the plot. |

### 19.6 Open

1. What goes in the Engineering panel, and how it overlaps the auxiliary actions in §6.

---

## 20 · The targeting panel

**What it is.** One panel holding the targeting computer **and** the firing command. They are one
continuous job — pick a target, get a solution, arm, fire — and splitting them across two panels
would mean opening two things to do one thing, when only one panel can be open at a time (§19.1.d).

**How it plays.** You pick what to kill, form an estimate of what it is doing, then set up as many
turrets against it as you have time for. Firing more than one at once is not a button — it is what
happens when you armed more than one. The panel always tells you why you cannot fire yet, so the
next move is never a guess.

**How it's built.** The panel holds the designated track and one shared estimate of the target
(speed, heading, error cone). Everything else is **per turret**: its own aim bearing, its own aim
point, its own arm state, and its own expiry clock. There is no volley mode and no spread value —
the fan is the difference between the bearings the player entered. All of it survives the panel
closing.

> **Rule 20.a** The panel is organised **target first**, not turret first. You choose what to kill,
> then decide which guns to spend on it.

This differs from the submarine reference on purpose. A submarine picks a tube. Starwolf has three
turrets that **combine**, so the interesting decision is how to divide them across one target.

> **Rule 20.b** PDCs are **not** in this panel. They are not fired, and they are aimed by direction,
> so they live on the plot itself (§21).

### 20.1 Layout, top to bottom

#### 1 — Track

The designated target.

| Field | Note |
|---|---|
| Designator | e.g. `TRACK 07` |
| Class | `UNKNOWN` until scanned (§9.5) |
| Range | Live |
| Relative bearing | From your hull — this is what decides arcs |
| Angle on bow | What aspect it is showing |
| Confidence | How solid the track is |

Designate by clicking a contact on the plot. Clicking a contact no turret can reach does nothing
(§8.7.c).

#### 2 — Estimate

What the computer thinks the target is doing, and what you think.

| Field | Source |
|---|---|
| Target speed | Computer estimate, **or your entered figure** |
| Target heading | Computer estimate, **or your entered figure** |
| Error cone | How unsure the computer is (§18.4) |

> **Rule 20.1.a** Both estimates are **editable**. Typing your own figure overrides the computer, and
> the error cone is redrawn to match your confidence, not its own.

> **Rule 20.1.b** The panel shows **why** the estimate is as good or bad as it is — scanner power,
> time since last radar pulse, scanned or not. The player must be able to see what to fix.

> **Rule 20.1.c** This block is about the *target*, not about any one gun. It is shared by all three
> turrets.

#### 3 — Battery

Three rows, one per turret. **Each turret carries its own bearing and its own solution.**

| Column | Shows |
|---|---|
| Turret | Forward left · Forward right · Aft |
| Bears? | Whether the target is inside this turret's arc right now |
| Bearing | **This turret's aim bearing.** Seeded from the estimate, editable per turret |
| Solution | Time left on this turret's solution clock (§20.4.f) |
| State | Empty · Arming · Armed |
| Arm | Starts arming this turret |

> **Rule 20.1.d** **A solution belongs to a turret, not to the ship.** Three turrets can hold three
> different bearings against the same target at the same time. This is the whole of the volley
> mechanic (§18.5) — there is no separate volley control anywhere in this panel.

> **Rule 20.1.e** Each turret's bearing is seeded from the shared estimate when it is armed, and can
> then be nudged on that turret alone. Setting the same number on three turrets converges the shots;
> setting different numbers fans them.

> **Rule 20.1.f** A turret that cannot bear is shown greyed with the reason — *outside arc* — not
> hidden. The player must be able to see that turning the ship would fix it.

> **Rule 20.1.g** Turrets arm **one at a time** and arming takes time (§8.3.a). The panel shows each
> turret's arming progress separately.

> **Rule 20.1.h** Each turret's solution clock runs independently. A turret armed first expires
> first, so a three-turret set-up has a **narrowing window** in which all three are still valid.

#### 4 — Fire

A single command. Dead unless the conditions are met.

> **Rule 20.1.i** **Fire releases every turret that is armed and bearing.** To fire one at a time,
> arm one at a time. There is no fire-mode selector.

> **Rule 20.1.j** Above the fire control, a status line says **why it is unavailable**, in this
> order:
>
> `NO TARGET` → `NO TURRET BEARS — MANOEUVRE` → `INSUFFICIENT POWER` → `ARM A TURRET` → `READY`

> **Rule 20.1.k** When more than one turret is ready, the control reports how many will fire — e.g.
> `FIRE — 3 READY`. The player should never have to count rows to know what a press will do.

### 20.2 What the panel puts on the plot

Per §19.2.a, everything here draws on the plot too.

| Panel action | Plot shows |
|---|---|
| Target designated | Lock ring and label on the contact |
| Turret selected | That turret's arc as a wedge from your hull |
| Solution computed | Aim point marker, and the error cone spreading from your ship |
| A second turret set to a different bearing | A second aim point, so the fan is something you see rather than dial in |
| Shot fired | The slug in flight, at true speed |

> **Rule 20.2.a** The **error cone is drawn on the plot**, not just reported as a number. How unsure
> the computer is should be something you see over the battlespace, not read in a box.

### 20.3 State and persistence

> **Rule 20.3.a** Closing the panel does not drop the target, the solution, or the arming state. The
> guns stay armed and the solution stays loaded.

**Resets:**

| Event | Clears |
|---|---|
| Target changed | The shared estimate, your manual figures, and **every turret's solution** |
| Turret loses its arc because the ship turned | Nothing. It stays armed and keeps its bearing |
| A turret's solution clock expires | **That turret's solution only.** The others keep running |
| Shot fired | That turret's armed state and its solution |

> **Rule 20.3.b** Turning the ship out of arc does **not** disarm a turret. You can turn away, turn
> back, and still be loaded.

### 20.4 Arming, disarming, and the solution clock

Decided 2026-09-02.

#### Arming needs a floor of weapons power

> **Rule 20.4.a** A turret cannot be armed unless WEAPONS holds at least `minArmPower` points
> (**tunable**). Below that floor the arm control is dead.

> **Rule 20.4.b** The cost is a **threshold, not a drain**. Arming does not eat points over time.
> What it does is force you to keep WEAPONS fed if you want guns available at all — so a
> scanner-heavy build cannot arm.

> **Rule 20.4.c** If WEAPONS drops below `minArmPower` while a turret is armed — because you vented,
> or the reactor took damage — the turret **stays armed but cannot fire.** The panel says
> `INSUFFICIENT POWER`.

#### Turrets can be disarmed

> **Rule 20.4.d** An armed turret can be disarmed at any time. Disarming is immediate and free.

> **Rule 20.4.e** Disarming is how you get out of a commitment. Arming took time; giving it up does
> not.

#### The solution expires

> **Rule 20.4.f** A firing solution has a life of `solutionLifetime` — **15 to 22 seconds**,
> starting at **18 s**, **tunable**. When it runs out the solution is **gone**, not merely degraded.
> You must build a new one.

> **Rule 20.4.g** The time left is shown as a countdown next to the solution, and the aim point
> marker on the plot fades as it runs down.

> **Rule 20.4.h** Expiry does not disarm the turrets. The guns stay loaded; only the solution is
> lost.

This is what stops a solution being banked forever. You measure, you commit, you fire — or you start
again.

---

## Appendix A · Where the Unity build is wrong

Not part of the specification. A reference list, written down so nobody ports a defect while
building the real thing.

| Spec section | Unity does | Correct behaviour |
|---|---|---|
| §3 Power | `PowerManager.AddPower` decrements `currentReactorPower` by 0.5 per bar — the reactor drains like fuel | The reactor is a ceiling on what you can allocate. Points are held, not spent. |
| §3 Power | Systems are named `engines / arms / bay / support / sig` | `ENGINES · WEAPONS · SCANNER · COMMS · AUXILIARY` |
| §4.3 Drift | `rb.linearVelocity = transform.up * currentSpeed` — velocity is hard-locked to facing, so the ship never slides. `maxLateralDrift` and `driftCompensationRate` are serialized but unused | Rotating the hull leaves velocity on its old vector; the sideways part bleeds off |
| §4.2 Throttle | `W`/`S` set a continuous target from held input | Throttle is stepped, ±15 per press |
| §8.7 Arcs | `WeaponBase.angleOfFire` is declared and assigned in two places but never tested against anything. No arc centre exists | Arc validity is range ∧ arc ∧ line of sight |
| §13.1 Shields | `Shields.cs` exists with recharge, colours and a renderer | There are no shields. Auxiliary is the recovery resource |
| §9.1 Radar | `Radar.cs` projects every registered target every frame — a continuous read | Radar is a discrete pulse every ~3 s; blips go stale between pulses |

---
