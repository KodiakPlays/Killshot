# Starwolf — Tactical Command Board Plan

**Status:** planning. Nothing in this document has been implemented.
**Decisions recorded:** 2026-09-01, owner-confirmed in session.
**Supersedes:** the UI portions of `Assets/PARITY_PLAN.md` (see [Reconciliation](#reconciliation)).

**What this is.** The Unity build is moving from a six-screen 3D cockpit to a single full-window
top-down tactical plot with station icons on the frame. This document records the design decisions
behind that move, reconciles three references that disagree, maps the change onto the existing
codebase, and lists the work in dependency order.

Read alongside `Assets/CODEBASE_BASELINE.md` (what Unity does today) and the design handoff
(see [Sources](#sources)).

### Section status

Being written and reviewed one section at a time. Do not treat a `DRAFT` section as agreed.

| # | Section | Status |
|---|---|---|
| 0 | Overview | **REVIEW** |
| 1 | World, scale & density | **REVIEW** — owner-specified 2026-09-01, awaiting sign-off |
| 2 | Time & the order model | DRAFT |
| 3 | Sensors, radar & emissions | NOT STARTED |
| 4 | Weapons & mounts | NOT STARTED |
| 4a | No shields | **DECIDED** 2026-09-02 |
| 5 | Hangar & squadrons | **REVIEW** — owner-specified 2026-09-02 |
| 6 | The board — layout & tokens | DRAFT |
| 7 | Codebase mapping | DRAFT |
| 8 | Work items & traps | DRAFT |
| 9 | Art & assets | **REVIEW** — written 2026-09-02 |

---

## 0 · Overview

### What Starwolf is

A single-ship tactical space combat game, played entirely from a **top-down plotting board**. No
cockpit, no exterior camera. The board is the window onto the battlespace, and every instrument is
a station opened over it.

The player commands one capital ship in a bounded, **debris-dense** arena. Combat runs in real time
and can be paused at will. Distances and speeds are large enough that the game is deliberate rather
than twitchy — but the arena is small and cluttered enough that something is always within reach.

### The core loop

**Find → Plan → Position → Commit → Resolve**

1. **Find.** Radar and sensors are active choices with a cost — emitting reveals you. Contacts
   arrive as returns of varying confidence, not as labelled enemies.
2. **Plan.** Read the plot. Measure with the instrument rail. Decide which mount can reach.
3. **Position.** Order a heading and speed. The ship converges on the order over time. Terrain
   blocks line of sight, gives cover, and hazards you. **Which hull side you present is your only
   armour** — there are no shields.
4. **Commit.** Bring a target into a mount's firing arc, complete the solution, fire.
5. **Resolve.** Watch it play out, or pause and reassess.

The fun is in step 3 into 4: **getting a hostile inside an arc while not being inside theirs.**

### Pillars

| Pillar | Meaning |
|---|---|
| **The plot is the game** | Not a map you consult. The surface you play on. Everything else overlays it. |
| **Information costs something** | Radar reveals you. Sensors are directional. Knowledge is bought with exposure. |
| **Geometry is the puzzle** | Arcs, ranges, obstacles, detection radii. Position is the primary verb. |
| **Space has stuff in it** | Debris and wreckage are cover, hazard, and landmark — not backdrop. |
| **The player does the work** | Manual solutions and manual identification, where the manual step *is* the gameplay. |

---

### What we take from Silent Shark

Basis: full-length frame pass of *"Silent Shark Is a New Tactical Submarine Sim With a Unique
Twist"* (Wolfpack345, 33:27), sampled across the whole runtime with a drill-down at 12:20–14:20.
Commentary audio deliberately ignored.

Seven structural lessons, in rough order of how much they matter:

1. **The plot is inhabited, not consulted.** Most games put a tactical map behind a key you press.
   Silent Shark makes it the only screen and stacks panels *over* it. Nothing ever replaces the
   plot. **Adopt wholesale.**

2. **Information is a resource you spend time to acquire.** You are never shown a contact. You are
   shown a *bearing*, and bearings accumulate. Successive bearing lines fan out from your position
   until a track resolves. The waiting is the cost, and it makes each contact feel earned.
   **Adopt** — via radar/sensor exposure trade-offs rather than literal bearing triangulation.

3. **Instruments are stations, not a HUD.** The right rail is a set of *jobs* — periscope,
   hydrophone, TDC, tubes, contacts, log, radio. Each opens, gets used, and closes. A HUD shows you
   everything at once; a station rail makes you choose what to look at. **Adopt directly** — this
   is exactly the icon rail replacing the four fixed screens.

4. **Deliberately withheld automation.** The game could compute angle-on-bow and identify the ship.
   It refuses. You rotate a silhouette by hand and pick the hull from a recognition manual, and
   getting it wrong ruins your solution. **The manual step is the gameplay.** Adopt the principle —
   but Starwolf's manual step should be *arc and approach geometry*, not data entry.

5. **Uncertainty is rendered, not hidden.** Solid vs dashed, sensor cones, fading history trails,
   "NO VISUAL CONTACT" states. What you *know* and what you *guess* are visually distinct at a
   glance. **Adopt** — the marker vocabulary in §6 already encodes this.

6. **Constraints come from position, not rules.** Radio fails at depth. The hydrophone degrades at
   speed. Vision requires periscope depth. None of these are cooldowns or unlock gates — they are
   consequences of where you chose to be. **Adopt** — this is exactly how emissions, radar, and
   terrain occlusion should work in §3.

7. **Time compression is a concession to emptiness.** 0.5× to 10000× exists because the ocean has
   nothing in it. It is a patch on a design problem, not a feature. **Reject** — see §1. Starwolf
   answers emptiness with density instead.

### Where Starwolf deliberately diverges

| Silent Shark | Starwolf |
|---|---|
| Empty ocean, fixed by 10000× compression | Dense arena; modest compression, mostly unnecessary |
| 30-day patrol, long silences | Bounded engagement, continuous pressure |
| Bow/stern tubes — weak arc constraint | Four PDCs, two railgun turrets, omni missiles — **arc geometry is the central puzzle** |
| Depth layers; nothing to collide with | Debris and wreckage you can hit, hide behind, and be blocked by |
| Slow hunt, brief violence | Positioning and violence interleaved |

The last two rows are where Starwolf is doing something Silent Shark cannot. Depth is a scalar;
**terrain is a plane you navigate**, and it turns firing arcs from a maths check into a real
tactical problem.

---

## 5 · Hangar & squadrons

Owner-specified 2026-09-02.

### The governing rule: you command, you do not fly

**Nothing in the hangar is manually piloted.** The player selects a target — with the mouse if that
is the easiest gesture — assigns an objective, and launches. The craft executes on its own.

This keeps Starwolf a command game rather than an RTS, and it is consistent with the ship's own
order model in §2: set commanded state, let actual converge. Squadrons are more instances of the
same mechanism, not a second control scheme.

> "You don't manually control any of those things. You select a target literally with your mouse,
> if need be, and I think that makes a huge difference." — owner, 2026-09-02

### Two craft types

#### Starfighters

A squadron held as a **count**, not as individually tracked ships.

| Property | Note |
|---|---|
| Squadron size | 5–10. Exact number unimportant, tunable. |
| Fuel | Limits sortie duration / range. |
| Ammo | Limits engagements per sortie. |
| Health (per craft) | Attrition reduces the count. |
| DPS (per craft) | Damage output scales with surviving count. |

**Roles:** intercept enemy fighters and bombers that have identified you; strike enemy ships.

**Loadout** — chosen before launch, not mid-flight:

- **Anti-hull lasers** — good at burning through hulls.
- **Bomber configuration** — a loadout item, heavier ordnance against capital ships.

Recommendation: keep loadout a **pre-launch planning decision**. It fits the "plan then position"
pillar, and a mid-flight swap would imply a control fidelity the design deliberately avoids.

#### Assault boats

**Two** boats, each carrying a squad of marines. Both may be deployed at once.

- Each requires an **assigned target**.
- **Boarding eligibility requires a prior scan.** The vessel must have been scanned before boarding
  is offered.
- Objectives are assignable per boat — e.g. *disable the engines*, rather than a generic capture.

**Design note.** The scan prerequisite gives sensors an *offensive* payoff, not merely a detective
one: scan → eligibility → board → capture or cripple. That is a genuine reward loop for spending
emissions on a scan, and it is worth protecting.

### What exists in code

| Thing | Reality |
|---|---|
| `BoardingPod.cs` | Exists — `turnRate`, `maxLifetime`, `attachmentForce`, `boardingTime = 5f`. Flies to a target, attaches, then **applies damage** with a `TODO` for boarding-party effects. |
| `BoardingPodLauncher.cs` | Exists as `: WeaponBase`, with `SetTarget(Transform)`, `maxLaunchRange = 5000f`. |
| Starfighters | **Nothing.** No squadron, no carrier logic, no craft. |

**The gap is not delivery, it is outcome.** A boarding pod currently reaches a hull and deals
damage. Marines, squads, objectives, and capture do not exist.

**Architectural problem:** assault boats are currently modelled as a **weapon**
(`BoardingPodLauncher : WeaponBase`, and `WeaponManager` carries a "Borading Pod" slot). Under this
spec they are a **hangar asset with orders**, not a mount. `WeaponBase` gives them a reload, an
ammo count, and an arc — none of which apply. This wants moving out of the weapon hierarchy.

### Open in this section

1. **Fighter loadout count** — two options named (anti-hull lasers, bomber). Is there a third
   (anti-fighter / interceptor), or do the two cover it?
2. **Attrition persistence** — do lost fighters return between missions, or is the squadron a
   campaign resource?
3. **Do fighters need orders beyond a target?** Assault boats get objectives ("disable the
   engines"). Fighters currently get only a target.
4. **Recovery** — must craft be recovered to rearm, and does that gate a second sortie?

---

## 4a · No shields

**Decided 2026-09-02: Starwolf has no shields.** Not deferred, not a lean.

Owner: *"I think it's actually more interesting to have zero shields. These are a little harder
sci-fi… I never wanted shields in the original game anyway."*

The existing `Shields` system was never part of the intended design. It is removed, not reworked.

### Why this is the right call

`HullSystem` already tracks **four independent health pools** — port, starboard, prow, aft — with
`DetermineHitSide()`. Today this is largely flavour, because shields absorb damage first.

Remove shields and **hull facing becomes the entire defensive system.** Which side you present *is*
the armour decision. That makes the autopilot orientation setting (§2) a tactical commitment rather
than a camera preference: holding "18,000 units, port side to target" means choosing which plate
eats the next salvo, and your railgun turret arcs follow from it.

It also raises the stakes on PDCs — with nothing to absorb a missile salvo, shooting it down is the
only answer, which is exactly what PDCs are for.

**This collapses three systems the owner specified separately — hull sides, autopilot orientation,
and PDC role — into one decision.** That is the strongest argument for removal.

### Removal scope

Five files reference shields: `Shields.cs`, `PowerManager.cs`, `Weapons/Railgun.cs`,
`Weapons/Laser.cs`, `Editor/WireUIReferencesEditor.cs`.

Removal also frees two slots that must be **reassigned, not left empty**:

| Freed | Reassign to |
|---|---|
| `SHD` node in the five-node power chain | Open — a candidate for the Reflex/cloak system, which needs a power draw of its own. |
| `SHIELD` bar in the handoff status bar | Open — hull-by-side readout is the obvious replacement, since facing is now the defence. |

**Trap:** `WireUIReferencesEditor` is load-bearing — it is the only automated path that connects
the HUD, and `UnityEvent` persistent calls are scene data. Any rename or removal must be mirrored
there **in the same commit**, or HUD wiring breaks silently for everyone.

**Sequencing:** removal is cheap but touches `PowerManager`, which `PARITY_PLAN` WI-1 is already
scheduled to reshape. Do both in one pass rather than editing that file twice.

---

## 1 · World, scale & density

### Design principle

> "The whole point of space is that there is shit over there. Silent Shark just has you floating
> through water." — owner, 2026-09-01

**Density, not compression.** The empty-board problem is solved by making the battlefield *smaller
and full of terrain*, not by fast-forwarding through emptiness. A dense arena means something is
always within reach at `x1`, so real time stays interesting without a time machine.

**You can bang into things.** Collision is gameplay, not an error state.

### What this reverses

An earlier recommendation in this session argued for a dual-path simulation — real physics below
~`x10`, analytic integration above it — because `Time.timeScale = 10000` cannot drive `FixedUpdate`
without either locking the editor or making collision meaningless.

**That recommendation is withdrawn.** It solved a problem this design does not have. With a small
dense arena, compression stops being load-bearing: a modest ceiling (roughly `x1–x50`) is enough,
`Time.timeScale` handles it natively, and physics stays real throughout — which collision-as-gameplay
requires anyway. One simulation path, not two.

Compression range remains a tunable. If a later playtest wants more, revisit; do not build for it now.

### Terrain

Two named environment types so far:

- **Debris fields** — scattered, navigable at cost.
- **Starbase graveyard** — "a large, massive section of the world like a graveyard of a giant
  starbase." Large static hulks, dense enough to route around.

Terrain earns its place by doing four jobs at once:

| Job | Effect |
|---|---|
| **Line-of-sight** | Blocks radar and sensor returns. Ties directly to §3 — hide behind a hulk to break detection. |
| **Cover** | Blocks PDC and railgun fire. Makes firing arcs a positioning puzzle rather than a geometry check. |
| **Hazard** | Collision damage. Rewards deliberate movement, punishes full burn through a field. |
| **Landmark** | Gives a featureless grid orientation and makes the plot legible at a glance. |

**This is what makes firing arcs interesting.** Arcs alone are a maths test. Arcs plus obstacles
plus a detection radius is a tactical problem.

### What already exists

- `Assets/Scripts/Asteroid.cs` — implements `IDamageable`, with `maxHealth`, destruction effects,
  a debris-spawn list (`minDebris`/`maxDebris`/`debrisForce`), and a damaged-material swap at a
  health threshold. `[RequireComponent(typeof(Collider))]`. **Collidable, damageable gameplay
  already.**
- Six prefabs at graduated scales: `Asteroid_5`, `_10`, `_50`, `_100`, `_1000`, plus
  `Asteroid_Cluster`.

**Gap:** nothing for static hulks or starbase wreckage. Asteroids are round, tumbling, destructible;
a starbase graveyard wants large, angular, mostly-indestructible geometry that reads as
*architecture* on the plot. Different art and different collision treatment.

### Tunables

Owner requirement: **all of these live in one central inspector-exposed home, and get tuned by hand.**
`GameManager` is the natural host — it already carries spawn tuning (`minScatterDistance`,
`maxScatterDistance`, `minSeparation`, `maxSpawnAttempts`) and an `Instance` singleton.

| Tunable | Current value / location | Note |
|---|---|---|
| Arena size | `WorldBoundary.playableAreaRadius = 50000f` | Shrink until the board feels dense. The "25,000" discussed was an example. |
| Boundary warning distance | `WorldBoundary.warningDistance = 5000f` | Scale with arena. |
| Desertion timer | `WorldBoundary.desertionTimerDuration = 30f` | |
| Play-rate ceiling | *does not exist* | Start `x0.5–x10`, raise only if playtesting asks. |
| Debris density | *does not exist* | Objects per unit area, per field type. |
| Field placement | *does not exist* | Count, size, and distribution of debris fields and graveyards. |
| Collision damage scale | *does not exist* | Damage per unit of impact velocity. |
| km ↔ world unit | *does not exist* | **Blocks every range number.** See §Open. |

Nothing above may be a literal inside a method body.

### Open in this section

1. **Arena size** — needs a first number to build against. Anything is fine as a starting point
   since it is tunable, but the grid tick scale and zoom range derive from it.
2. **km ↔ world unit convention.** The handoff talks in kilometres (mounts 4,000–40,000 km, plot
   ±30,000 km, speeds 1.10–1.84 km/s); Unity is unitless (`rangeSteps` 500–8,000,
   `engagementRanges` to 25,000, `playableAreaRadius` 50,000). One convention must be fixed before
   any range, arc, or tick number is authored.
3. **Are starbase hulks destructible?** Asteroids are. Hulks probably should not be, or cover
   evaporates mid-fight — but partial destruction is a real option worth deciding deliberately.
4. **Does terrain block sensors, radar, or both?** Affects §3 directly.

---

## Sources

| Source | What it is | Authority |
|---|---|---|
| `References/Tactical spaceship UI planning.zip` | Design handoff — 430-line README plus four `.dc.html` reference builds. Exact hex, px, easings, state model, interaction chain. | **Normative for the board's look and layout.** Its *time model* is not — see below. |
| `References/starwolf-prototype.html` | 4,472 lines, five Canvas2D contexts. Real-time with incidental pause. | Behavioural reference for combat feel. Superseded on UI. |
| Unity build | 6,799 LOC runtime + 2,052-line `UIController`. | Wins on all conflicts. Code and live Editor state are ground truth. |
| `Starwolf — Figma` (`TuqtljpqRIIAHlQewXTULH`) | **Not a source.** An HTML paste — one "Imported HTML" frame, every layer named `div`/`span`, no components, no variables, no styles. The import is lossy: the periscope's perspective corridor flattened to rectangles, rail icons became coloured blocks, the brushed-metal plate dropped entirely. | Ignore. Use the zip. |

### Reconciliation

The handoff README opens by describing a **paused-time** game — *"combat is fought with time
paused… plan then commit… this is the core verb."* **This is not the design.**

Owner decision, 2026-09-01: **Starwolf is real-time with optional pause.** The player may pause at
will or play straight through; distances and speeds are large and slow enough that real time is
viable. Pause is a convenience, not the core verb.

Consequences:

- `PARITY_PLAN` Phase A survives. WI-4 (movement model) stays valid — the earlier concern that it
  was building the wrong thing does not apply.
- `PlayerShip`'s real-time `FixedUpdate` movement survives.
- The handoff's **EXECUTE** button is removed (see [Time controls](#time-controls)).
- The handoff's open item #5 ("delta channel") drops in priority — it was justified by *"in a
  paused-time game the question at unpause is what changed while I wasn't looking."* Still useful,
  no longer urgent.

Everything else in the handoff stands as written.

---

## Confirmed design decisions

All owner-confirmed 2026-09-01.

### Frame and layout

- **No cockpit.** The 3D cockpit interior is removed. The plot is the whole window — "think of it
  as a window."
- **Station icons pinned to the frame edge**, not physical screens in a 3D scene. Left rail =
  plotting instruments (annotations, no consequences). Right rail = ship systems (consequences).
- **The plot is a square grid, not a circle.** No elliptical mask anywhere.
- Design resolution **1920 × 1080**, **104px** shared side margin. Vertical bands per the handoff:
  status bar `y26–88`, plot well `y100–834`, order row `y844–944`, hardware row `y954–1058`.
- The plot occupies ~68% of board height. This is deliberate and should not erode.

### Time controls

**Only two controls own time:**

| Control | Behaviour |
|---|---|
| **PAUSE** | Stops the clock. Commanded state can still be set while paused. |
| **PLAY RATE** | `0.5×` to `10000×`. |

**Removed entirely:** `EXECUTE` and `EMERGENCY ABORT`.

`EXECUTE` was the handoff's commit-and-resume verb. With real-time-with-pause it degenerates into a
play button — its own sub-label read `RESUME TIME` — and duplicated the hardware row's PAUSE and
PLAY RATE. Removing it also resolves a contradiction: the handoff reserves hazard-stripe guarding
for *"destructive or irreversible actions"*, and unpausing is neither.

With both guarded controls gone, **hazard striping is now unused and reserved.** Spend it only on a
genuinely irreversible action later (scuttle, self-destruct). Do not reintroduce it as decoration.

### Order model — commanded vs actual

This is the core movement architecture, and it is owner-specified.

> The ship holds a **commanded** state and an **actual** state. Input sets commanded. Actual
> converges toward commanded at a physical rate, whenever the clock is running.

Worked example: while paused, the player sets a bearing — by typing it, or by nudging with `A`/`D`.
Nothing moves. That is a queued future order. On resume, the ship slews steadily toward the
commanded bearing at its turn rate; the plot shows actual heading converging on commanded.

**Why this is the right model:** pause and real-time need **no separate code path**. Pause is the
clock stopping. There is no commit step, no order-queue mechanism, and no second simulation mode to
keep in sync. One mechanism, two clock states.

**It also makes time compression tractable** — commanded/actual state is a pure function of `dt`
and integrates analytically. A physics-driven ship does not. See
[the compression trap](#trap-1-timescale-will-not-reach-10000).

### Tuning

**Every number is inspector-tunable.** No gameplay or layout constant may be a literal inside a
method body. Preference is a `ScriptableObject` tuning profile so variants can be swapped without
touching scene data.

Arena size uses **the current world state** — `WorldBoundary.playableAreaRadius = 50000f`. The
"25,000" figure discussed in session was an example, not a spec.

---

## Design tokens

Verbatim from the handoff. **Colour is a coding system, not decoration** — it states allegiance or
condition and is never applied for visual interest. Do not add hues without extending the system.

| Token | Hex | Meaning |
|---|---|---|
| `hostile` | `#E04B3C` | Enemy tracks, threat vectors, hull breach, targeting computer |
| `friendly` | `#4A9FD8` | Own fighters, friendly craft, shield, comms, hangar |
| `own-systems` | `#8FC33A` | Fire arcs, reactor, alert-green, measurement annotations |
| `orders` | `#E8A33D` | Queued actions, pending commits, target brackets, own deployed assets |
| `orders-bright` | `#F0A020` | Active interlocks, on-bearing state |
| `unclassified` | `#9B7FD4` | Unresolved returns, possible missiles |
| `sensor` | `#3FA9C9` | Range rings, scan envelopes, grid lines — sensor data only |
| `own-ship` | `#E8EDF0` | Own hull, heading vector, primary values |

Surfaces: `void #0A0C0E`, `metal-lit #33383E`, `metal-mid #23282D`, `metal-shadow #101315`,
`plot-glass #0A1719`, `scope-glass #0B0405`.

Text tiers: primary `#E8EDF0`, secondary `#C9D2D8`, tertiary `#A8B2B9`, label `#6E7B85`,
disabled `#5B666F` / `#4E5860`.

Type: **Barlow Condensed** (400/500/600) for titles, names, labels. **Share Tech Mono** for *all*
numerics, bearings, ranges, timers, ticks. Both Google Fonts, SIL OFL — bundle locally. Labels
always uppercase, no italics, units set at the same size as their number, **numerics never animate
their digits — they snap.**

Geometry: 4px base unit. Radii 7px outer panels, 6px wells/rails, 4–5px buttons, 3px inner fields.
Grid **60.9 × 58.2px** minor cells at `rgba(63,169,201,.065)`.

**Marker vocabulary — glyph states class, colour states allegiance. A shape is never reused across
two allegiances.** Filled wedge = own ship. Outlined wedge = classified hostile. Diamond outline =
unclassified hostile. Triangle = friendly craft. Filled diamond = own deployed asset. Circle
outline = unresolved return. Dashed wedge = predicted position.

---

## Codebase mapping

### Keeps — real, and directly reusable

| Asset | Role |
|---|---|
| `WorldBoundary` | `playableAreaRadius = 50000f`, warning zone, desertion timer, five events. The bounded arena already exists. |
| `TargetingSystem` / `Targetable` / `TargetingReticle` | Lock acquisition, reticle, `WeaponManager` hookup. |
| `PlayerShip` speed model | `targetSpeed` → `currentSpeed` via `Mathf.MoveTowards` at `rateOfAcceleration`. **This is already the commanded/actual model, for speed.** |
| `Autopilot` | `EngageNavigation(Vector3)`, `navigationTarget`, `UpdateNavigationAutopilot()`. Heading-seeking logic that currently only runs under autopilot — the seed for commanded heading. |
| `PowerManager` | 5 subsystems × 5 states. Backs the REACTOR readout and system rails. |
| `GameClock`, `QuestSystem` | Mission time, objectives. |
| `WeaponManager` / `WeaponBase` | The mount rack. Extensible per `CLAUDE.md`. |
| `ControllerHaptics` | 14 presets. |
| `WireUIReferencesEditor` | Load-bearing. Mirror every rename here in the same commit. |

### Stubs — declared but not implemented

| Thing | Reality |
|---|---|
| **Firing arcs** | `WeaponBase.angleOfFire` is declared and *assigned* in exactly two places — `MissileLauncher` sets `360`, `LaserWeapon` sets `45` — and **never tested against anything.** There is no arc check anywhere in the codebase. There is also no arc *centre*. This is the mechanic the owner named as the core fun. |
| `WeaponBase.range` | Only `BroadsideCannon` uses it, via one `OverlapSphere`. |
| Commanded heading | Does not exist. Turning is rate-based: `A`/`D` set `targetTurnSpeed`, the ship spins while held and stops wherever released. |
| Lateral drift | `maxLateralDrift`, `driftCompensationRate` are serialized but velocity is hard-locked to facing: `rb.linearVelocity = transform.up * currentSpeed`. |

### Retires

The six world-space canvases mounted as physical screens in the 3D cockpit — `Canvas-Interior`,
`Canvas-Navagation`, `Canvas-PlayerShipStats`, `Canvas-BogieShipStats`, `Canvas-Power`,
`Canvas-Weapon` — along with their per-canvas cameras and RenderTextures.

**Do not delete blindly.** `UIController` (2,052 lines) holds shader-driving logic worth keeping:
the 30-pip hull scanner (`UI_ShipHack`), the power node arrow chain, the comms tuner dial, and the
glitch/scanline effects. Harvest before removing.

### Scale mismatch — unresolved

The handoff speaks in **kilometres**: mount ranges 4,000–40,000 km, plot spanning ±30,000 km,
velocities 1.10–1.84 km/s. Unity speaks in **unitless world units**: weapon ranges 500–2,000,
`Radar.rangeSteps` `{500, 1000, 2000, 4000, 8000}`, `Autopilot.engagementRanges` up to 25,000,
`playableAreaRadius` 50,000.

**No km ↔ world-unit convention exists.** One must be chosen and written down before any range,
arc, or grid-tick number is authored, or every number will need re-deriving later.

---

## Open decisions

1. **Physics / compression split threshold.** See [Trap 1](#trap-1-timescale-will-not-reach-10000).
   Blocks the movement rewrite.
2. **km ↔ world-unit scale.** Blocks all range and arc authoring.
3. **What fills the order row** now that EXECUTE's 196px is free.
4. **Mount loadout.** Handoff specifies RAILGUN A/B, PDC A/B, MISSILE VLS. Current `WeaponManager`
   slots are Lasers, Railgun, Missile, Boarding Pod, with `BroadsideCannon` and
   `PointDefenseCanon` also present. These need aligning.
5. **Hull art.** All silhouettes in the handoff are placeholder wedges. Real hulls need two tiers —
   a detailed version for inspect/hangar, and a *separately drawn* 12–20 point glyph legible at
   30px on the plot, not an automatic reduction.

Inherited open items from the handoff, still open: HANGAR panel, SENSORS / REACTOR / ENGINEERING /
COMMS panels (icons only), two-tier contact labels, growable order queue,
annotation-vs-command visual distinction.

---

## Work items — dependency order

```
0. FOUNDATION      WI-A scale convention  ──┬──> everything with a range in it
                   WI-B tuning ScriptableObject

1. MOVEMENT        WI-C commanded heading ──> WI-D compression path
                   (needs the Trap 1 decision first)

2. PLOT            WI-E plot surface + continuous zoom
                   WI-F contact rendering + marker vocabulary

3. COMBAT          WI-G firing arcs (needs WI-A) ──> WI-H targeting chain

4. CHROME          WI-I rails ──> WI-J drawers ──> WI-K hardware row
```

### WI-A · Scale convention — SMALL, blocks almost everything
Choose and document km ↔ world unit. Reconcile `Radar.rangeSteps`,
`Autopilot.engagementRanges`, `playableAreaRadius`, and every `WeaponBase.range`.

### WI-B · Tuning ScriptableObject — SMALL
Lift hardcoded literals: `maxThrust = 100f` inside `PlayerShip.FixedUpdate`, and every zoom
constant in `UIController.WorldGridZoom` (`5f`, `20f`, `20f`, `.02f`, `.04f`, `.025f`).

### WI-C · Commanded heading — MEDIUM
Add `commandedHeading`. `A`/`D` nudge the commanded bearing rather than applying turn rate
directly; bearing entry on the plot writes the same field; actual slews toward commanded at
`turnRate`. Lift `Autopilot`'s seeking logic into the normal movement path.
**Write movement as a pure function of `dt`** so WI-D can reuse it. Note this will fight
`Autopilot` as currently written — `PARITY_PLAN` already flags that.

### WI-D · Time compression — MEDIUM–LARGE, needs Trap 1 decided
Dual-path stepping. Blocked on WI-C's pure-`dt` movement function.

### WI-E · Plot surface + continuous zoom — MEDIUM
`#0A1719` well, `60.9 × 58.2px` grid at `rgba(63,169,201,.065)`, fixed edge axis labels in
thousands of km, scroll-wheel zoom `×1.12` per notch clamped `0.5–3.0` rounded to 2dp.
Replaces `WorldGridZoom`'s three discrete 10× steps with continuous zoom plus LOD thresholds.
`Assets/UI/Shader/NavGrid.shader` exists as a scaffold in
`Assets/Scenes/Sandbox/TacticalGrid_Test01.unity`, with a placeholder palette to be replaced by
real tokens.

**Label counter-scale:** the handoff keeps text at constant size while geometry scales, via a
`--lbl: 1/zoom` counter-scale. Unity needs an equivalent — labels must not scale with the plot.

### WI-F · Contact rendering — MEDIUM
Data-driven from the simulation's entity list, **never hand-placed**. Per-track data the sim must
supply: id, display name, class, bearing, range, allegiance, speed, hull/section damage, scan
confidence, predicted position at +t. Vector length encodes speed at a fixed scale.

### WI-G · Firing arcs — SMALL–MEDIUM, the core mechanic
Add `arcCentre` and `halfArc` to `WeaponBase`. Validity test:
`range ≤ mount.range` **and** `angularDifference(track.bearing, mount.arcCentre) ≤ mount.halfArc`.
Draw the conic arc wedge on the plot. Report `n IN ENVELOPE`.

**Zero-envelope is a legitimate state, not an error** — it tells the player to manoeuvre. But the
default mount on load **must** have at least one valid target, or the feature reads as broken on
arrival.

### WI-H · Targeting chain — MEDIUM
Mount select → valid set recompute → designate → slew to bearing → interlocks → fire.
Resets: changing mount clears target, payload, trim, all interlocks. Changing target clears trim
and interlocks. Slewing off-bearing clears SOLUTION only.

### WI-I/J/K · Rails, drawers, hardware row — MEDIUM each
Rails are geometrically identical and mirrored — **this symmetry matters and must not drift.**
Drawers *pull out*, 0.42s `cubic-bezier(.2,.85,.25,1)`.

---

## Traps

### Trap 1 · `Time.timeScale` will not reach 10000×
`FixedUpdate` runs on `fixedDeltaTime` of *scaled* time. At `timeScale = 10000` either the editor
locks up running 10,000× the physics steps, or `fixedDeltaTime` is scaled up to compensate and
ships teleport thousands of km per step — collision detection becomes meaningless. `PlayerShip` is
a `Rigidbody` with a `BoxCollider`; PhysX will not survive this.

**Recommended split:**
- **≤ ~x10** — real physics, `Time.timeScale` as normal. This is combat.
- **x100 – x10000** — **stop simulating physics.** Integrate analytically:
  `position += velocity × dt`, `heading += turnRate × dt`. Step in a plain loop, watch for events
  (contact enters sensor range, waypoint reached, closure to weapons range), drop back to real time
  when one fires.

This is why the commanded/actual model matters: it is trivially integrable. **Decide this before
WI-C is written** — movement must be a pure `dt` function usable by both paths, which is cheap now
and painful to retrofit.

### Trap 2 · Real time at x1 is inert
By the handoff's own numbers — own ship 1.84 km/s, plot spanning 60,000 km:

| | at x1 | at x10 |
|---|---|---|
| Cross the visible plot | **9.1 h** | 54 min |
| Reach a contact at 24,300 km | 3.7 h | 22 min |
| Mutual closure at 2.94 km/s | **2.3 h** | 14 min |

x1 is a still image. The working default needs to be roughly **x60–x300** for engagements to
resolve in 30 s – 2 min. Do not ship `x1` as "normal speed".

### Trap 3 · One bearing value, three consumers
From the handoff, and worth repeating: **screen angle = bearing − 90°.** Own hull, its heading
vector, and its `HDG` label must all derive from **one** bearing value. Three independent angles is
a bug that reads as a broken plot.

With commanded/actual heading there are now *two* legitimate bearings — commanded and actual — and
each must be unambiguous about which it draws. The plot should show both: actual as the solid hull
and vector, commanded as an `orders`-coloured ghost.

### Trap 4 · `_ShipRotation` is already set in two different units
Pre-existing bug. `UIController.cs:1402` and `:1432` pass `shipPlayer.localRotation.z` — a
quaternion component, roughly −1..1 — while `PlayerShip.cs:693` passes `transform.eulerAngles.z` in
degrees. The per-frame path overwrites the zoom-time value, so it is invisible today. It will bite
whoever touches the zoom path. Fix during WI-E.

### Trap 5 · `.dc.html` files are reference, not source
The handoff says it plainly: *"design references created in HTML… not production code to copy
directly."* The plot in the prototype is absolutely-positioned DOM with CSS transforms; in Unity it
must be **data-driven from the entity list**. The brushed-metal frame is a WebGL fragment shader
(source in `figma/steel-plate.html`, baked at `figma/steel.png`) — port the shader or use the bake.

### Trap 6 · Do not edit `References/`
Owner-authored input. The handoff zip stays as delivered. Extract elsewhere to read it.

---

## 9 · Art & assets

Written 2026-09-02.

### Governing rule: every art reference is inspector-swappable

**No art asset is referenced directly from a script or hard-wired into a scene.** All of it hangs
off a **`ScriptableObject` art profile** — one asset holding every sprite, material, prefab and font
reference the board and the simulation use.

Swap the profile asset, swap the entire look. No scene edits, no code changes, no prefab surgery.
This is the same reasoning as the tuning profile in §1: the owner tunes by hand in the inspector,
so nothing may be a literal or a buried reference.

Suggested shape — one profile, grouped:

| Group | Holds |
|---|---|
| **Hulls — plot tier** | Per-class glyph sprite, 12–20 points, legible at 30px |
| **Hulls — detail tier** | Per-class detailed silhouette for the forward screen and hangar |
| **Markers** | The seven glyph shapes (see below) |
| **Rail icons** | 7 instrument + 8 system icons |
| **Surfaces** | Metal material, plot-glass material, scanline/grain material |
| **Terrain** | Asteroid prefabs, debris prefabs, starbase hulk prefabs |
| **Type** | Barlow Condensed and Share Tech Mono font assets |

### What the handoff actually gives us

**Audited 2026-09-02. The handoff is a specification, not an asset bundle.** Its README states it
plainly: *"No raster art. Every icon, hull and instrument is drawn in CSS."* Verified — across all
four `.dc.html` files there are **zero `<img>` tags, zero data URIs, one `<svg>` total**, and 37
`clip-path` declarations doing all the drawing.

| Asset | Status |
|---|---|
| `figma/steel-plate.html` | **The most valuable asset in the package.** Full GLSL fragment shader for the brushed-metal plate — anisotropic noise stretched along X, sheen from upper-left, two highlight bands, sparse scratches, plate vignette. GLSL→HLSL is near-mechanical. Port it. |
| `figma/steel.png` | Baked fallback. **Defect: the README claims 1920×1080; the file is 924×540.** Re-bake from the shader rather than upscaling. |
| Barlow Condensed, Share Tech Mono | Real, usable. Google Fonts, SIL OFL — bundle locally for a shipped game. |
| Everything else | Specification only. |

**The Figma file provides nothing.** It is an HTML paste with every layer named `div`/`span`, and
the import is lossy — the periscope's perspective corridor flattened to rectangles, rail icons
became colour blocks, the metal plate dropped entirely.

### Explicitly placeholder — must be replaced

Flagged by the handoff's own open items:

1. **All hull silhouettes.** Currently a 10-point "sword" wedge via `clip-path`. *"Do not treat as
   final."*
2. **The asteroid field.** Scattered ellipses.

### Hulls need two separately drawn tiers

The handoff is emphatic that these are **not** one asset at two sizes:

| Tier | Where it appears | Requirement |
|---|---|---|
| **Detail** | Forward screen, hangar, inspect | Full silhouette. Damage must register visibly on it. |
| **Plot glyph** | The tactical plot | 12–20 points, legible at **30px**. **Drawn separately — never an automatic reduction of the detail tier.** |

**The forward screen depends on this.** §4's forward viewer — a grainy negative image where you
watch hits land — has no reason to exist without the detail tier. It is that tier's home, and the
argument for building it.

### Marker glyphs

Seven shapes, and the rule is absolute: **glyph states class, colour states allegiance, and a shape
is never reused across two allegiances.**

Filled wedge (own ship) · outlined wedge (classified hostile) · diamond outline (unclassified
hostile) · triangle (friendly craft) · filled diamond (own deployed asset) · circle outline
(unresolved return) · dashed wedge (predicted position).

The dashed wedge does double duty — it is also the **Reflex cloak's last-known-position ghost**, so
no new marker is needed for cloak. See §3.

### Rail icons

Each icon sits in a **20 × 20 optical box at 1.5px stroke**, `#A8B2B9`. Specified precisely in the
handoff, drawn in CSS, therefore not portable as art.

**Left rail — 7 instruments** (all specified): POINT (crosshair with centre square) · LINE · RULER ·
COMPASS · ERASE · UNDO · CLEAR ALL (dashed).

**Right rail — 8 systems.** Six are specified in the handoff; **two are new and must be designed**:

| System | Icon | Colour | Status |
|---|---|---|---|
| Targeting computer | Missile silhouette, nose up, tail fins | `#E04B3C` | Specified |
| Sensors | Three concentric emitter arcs from lower-left | `#3FA9C9` | Specified |
| Reactor | Atom — three ellipses at 0/60/−60°, glowing nucleus | `#8FC33A` | Specified |
| Hangar | Small plane, top-down | `#4A9FD8` | Specified |
| Engineering | Open-ended wrench, canted 38° | `#E8A33D` | Specified |
| Comms | Traffic panel with three transmission waves | `#4A9FD8` | Specified |
| **Forward screen** | — | — | **NEW — needs design** |
| **Reflex / cloak** | — | — | **NEW — needs design** |

Both new icons must be drawn in the established 20×20 / 1.5px idiom, or the rail's rhythm breaks.

**Route into Unity — recommend SVG, not PNG.** The plot zooms continuously (§1), so raster icons
will soften. **`com.unity.vectorgraphics` (the SVG importer) is NOT installed** — the manifest only
carries `com.unity.modules.vectorgraphics`, which is the built-in module, not the importer. Adding
the importer is a prerequisite. The fallback is rendering each icon from the reference HTML at high
resolution and exporting PNGs, accepting the softening.

### Terrain art — the largest gap

§1 calls for debris fields and a starbase graveyard. What exists:

- `Asteroid.cs` with `IDamageable`, plus six prefabs at scales 5 / 10 / 50 / 100 / 1000 and a
  cluster. **Usable.**
- **Nothing for starbase wreckage.** Asteroids are round, tumbling and destructible; a graveyard
  wants large, angular, mostly-indestructible geometry that reads as *architecture* on the plot —
  different silhouette language, different collision treatment, and it must be identifiable at plot
  zoom as a landmark rather than a rock.

### Shaders we can lean on

Existing and reusable: `ScanLines.shadergraph` (the forward screen's grain and the plot's `scan`
overlay), `UI_ShipHack.shadergraph` (the 30-pip hull scanner), `UI_GradeDisplay*` (bars and meters),
`UI_PowerArrow.shadergraph` (the power node chain), `NavGrid.shader` (square plot grid, scaffolded
2026-09-01, palette still placeholder).

To be ported: the brushed-metal plate from `steel-plate.html`.

**Cleanup owed:** `Assets/UI/Shader/` contains `GridSpaceUI-Bogie(deleateMe).mat` and
`GridSpaceUI-PlayerShip(deleateMe).mat`. `Assets/UI/icon/` is almost entirely `test_*` placeholders.

### Trap · re-baking the metal plate

From the handoff, and easy to lose: the WebGL context must be created with
`preserveDrawingBuffer: true, alpha: false`, or the buffer is discarded after compositing and the
plate exports blank. Repaint on `webglcontextrestored`.

### Resolved 2026-09-02

1. **Damage renders on the forward screen only.** The plot glyph never shows damage — it stays a
   symbol. *"It's like looking at something through a periscope."* This is the forward screen's
   exclusive job and the whole argument for building it.
2. **Terrain is world-scale, not glyph-scale.** Wreckage scales with zoom like all world geometry;
   only *labels* counter-scale (`--lbl: 1/zoom`). Zoomed out a graveyard is a mass to route around;
   zoomed in you thread individual pieces. No special silhouette language needed.
3. **Do not add the SVG importer.** Author in SVG, ship supersampled PNGs. Rail icons are frame
   chrome and never zoom — they only span 1080p→4K. Plot geometry spans the `0.5–3.0` zoom clamp,
   so 6×; a 4× design-size export covers it. This avoids a preview-package dependency and
   `SVGImage`'s missing features (no sprite slicing, no fill methods). Revisit only if the zoom
   clamp widens.
4. **Prototype scope is one ship versus one ship.** Ship-class art count is not a question yet.
5. **Placeholder hulls generated** — `Assets/Art/Hulls/hull_wedge_filled.png` (own ship) and
   `hull_wedge_outline.png` (classified hostile). 512×1024, white with alpha so allegiance colour
   is applied as a tint in-engine — one sprite serves any faction. Ten-point sword wedge: nose,
   forward sponson, waist, engine flare, aft. **Placeholder — replace before ship.**

### Still open

- **Who draws the real hulls**, when the prototype proves out. Two tiers per class remains the
  largest art commitment in the project.
- **Starbase hulk prefabs** — still nothing. §1's largest art gap.
