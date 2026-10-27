# Starwolf — HTML Prototype Baseline

> **Written from the prototype at 4,472 lines / 226,767 bytes.**
> If that no longer matches `References/starwolf-prototype.html`, re-check before trusting
> the numbers here. The prototype is authoritative; this document is derived.

**What this is.** A reference for `References/starwolf-prototype.html` (4,472 lines), the **playable
build**. Companion to `Assets/CODEBASE_BASELINE.md`, which documents Soham's older Unity scripts.
The tactical board's code is in neither file.

**Authority.** The prototype is the owner's design expressed as working code. Where it and the GDD
disagree, that is a decision for the owner, not a defect. The Unity board is ahead of the prototype
on many systems, and differs from it on purpose in places. Where they disagree, the newer source
wins: the board and the owner's later rulings. The prototype is a floor for the finished game.

**Two corrections applied here** that intermediate analysis got wrong — see §9.

---

## 1. Shape of the file

| Lines | Contents |
|---|---|
| 1–309 | CSS. Fixed 1920×1080 stage, letterboxed by a JS transform. Monochrome white-on-black plus three accents. |
| 311–560 | DOM. The control panel: 4 permanent panels + 7 swappable centre views. |
| 561–2460 | Simulation. Constants, state, input, power, aux, physics, five weapons, sensors, damage, AI. |
| 2460–3805 | Rendering. Seven view renderers, CRT, discovery. |
| 3805–4471 | Helpers, HUD updates, power UI construction, game loop. |

One `requestAnimationFrame` loop, `dt = min((ts-lastTs)/1000, 0.05)`, ~25 update functions in a fixed
order, then exactly one render call for the active view, then `updateUI()`.

---

## 2. Panel layout

`.body` is a CSS grid: `300px 1fr 320px` × two equal rows.

| Panel | Cell | Always visible |
|---|---|---|
| POWER MGT | col 1 row 1 | yes |
| SHIP STATS | col 1 row 2 | yes |
| CONTACTS | col 3 row 1 | yes |
| WEAPONS | col 3 row 2 | yes |
| SONAR / RADAR / MAP / SCANNER / ENGINEERING / TARGETING / COMMS | col 2, both rows | one at a time |

Views cycle with `T`. **SCANNER and COMMS pause the simulation** — their tabs are amber and carry a
`⏸` glyph. Pausing is `manualPaused || activeView==='SCANNER' || activeView==='COMMS'`; the cockpit
keeps animating while the sim is frozen.

---

## 3. Core constants

| | |
|---|---|
| `REACTOR_MAX` 10 · `SYSTEM_MAX` 5 | reactor is an **allocation cap**, not a wallet — bars are reserved, not spent |
| `SONAR_RADIUS` 8000 · `RADAR_RADIUS` 40000 · `MAP_RADIUS` 50000 | three sensor scales |
| `MAX_SPEED` 240 · `TURN_RATE` 58 · `SPEED_STEP` 15 | throttle is **stepped**, not held |
| `ACCEL_RATE` 80 · `BRAKE_RATE` 150 · `REVERSE_ACCEL` 55 · `REVERSE_SPEED_FACTOR` 0.42 | |
| `SPACE_DRAG` 0.9992 · `DRIFT_DECAY` 0.988 | lateral inertia bleed — the ship slides |
| `LATERAL_ACCEL` 42 · `LATERAL_MAX` 90 | Q/E are **held thrusters**, 4 stability/s |
| `STAB_REGEN` 10/s | drain by speed band: `5.0 / 2.1 / 0.7` × 22 × dt |
| `GUN_ARC` 15° · `GUN_SPEED` 900 · `LOCK_TIME` 2.0 | |
| `RADAR_INTERVAL` 3.0 | discrete pulse; loadout tunes 2.4–3.4 |
| `LIFE_SUPPORT_TIME` 90 | counted, **never resolved** — see §9 |
| `SECTOR_STRETCH` 1.8 | applied once at load to every world coordinate |

Player hull: **prow 50, port 50, stbd 50, aft 40** (190 total).
Carrier hull: prow 90, port 110, stbd 110, aft 80 (390), crew 70.

---

## 4. Power — the spine

Five systems: **ENGINES, WEAPONS, SCANNER, COMMS, AUXILIARY**, keys `1`–`5`.

Switch cycles `standby → draw → vent → standby` (three states; vent **is** cancellable).
Draw fills one bar per `2 × (number of systems drawing)` seconds each — aggregate throughput is
constant, so splitting the draw only splits it. Vent sheds 1 bar/s and auto-returns to standby at 0.

`getEffectiveReactorMax()` degrades with reactor damage: hp 3→10, 2→8, 1→6, 0→4. When total bars
exceed the cap, `updateInternals` **sheds from the lowest-priority systems** (last-to-first).

### Energy signature — why power costs you

```js
getSig() = Σ bars  ×0.5 if distorting  ×0.6 if reactor-flushed   then ×nebulaFactor at use
```

This is the central stealth currency. Drawing power is what gets you seen. Consumed by the awareness
ladder, the carrier AI, and the targeting solution.

### Auxiliary — replaces shields

Charge accrues at `[0, 2, 5, 9, 14, 20]` per second by AUX bars, capped 100.

| Key | Action | Cost | Effect |
|---|---|---|---|
| `Z` | Repair | 60 | +1 hp to the highest-priority damaged internal (lifesupport→reactor→engines→sensors→magazine→bridge→crew); if none damaged, +10 to the weakest hull side |
| `X` | Reload | 45 | Cannon barrels refilled / railgun recal cleared / missile reload cleared. Fails if magazine dead |
| `B` | Signal Distort | 50 | 10 s, signature ×0.5 |
| `N` | Reactor Flush | 40 | Vents one bar from each venting system, −3 s railgun recal, 8 s signature ×0.6 |
| `M` | Damage Control | 30 | 20 s: life-support countdown at half rate, and `breachInternal` negated 40% of the time |

---

## 5. Movement

`W`/`S` step `targetSpeed` by ±15, clamped to `(ENGINES bars / 5) × getEffectiveMaxSpeed()`.
`A`/`D` **rotate the hull only** — velocity keeps its old vector. Each frame velocity is decomposed
into nose-aligned and lateral components; the lateral part bleeds at `DRIFT_DECAY + 0.006×engRatio`
per 1/60 s, the whole by `SPACE_DRAG`. That is the drift model.

`Q`/`E` are held lateral thrusters requiring ≥1 engine bar.

Turning costs stability by speed band and regenerates at a flat 10/s when not turning. A hard turn at
speed drains 110/s against a 100 maximum — **turning is a resource decision**.

Collision with the world pushes the ship out, reflects velocity ×1.2, costs up to 40 stability, and
above 35 u/s impact deals `round((speed−30) × 0.25)` hull damage to the contact quadrant.

---

## 6. Weapons — five, no laser

Shared damage formula:

```js
dmg = round( max(baseDmg, distanceTravelled × perUnit) × typeMod × dmgMod )
```

| | type | typeMod | pen | baseDmg | perUnit | ammo |
|---|---|---|---|---|---|---|
| CANNON | KINETIC | 0.85 | 2 | 10 | 0.010 | 15 |
| RAILGUN | KINETIC | 1.10 | 5 | 4200 | 0.180 | 4 |
| MISSILE | EXPLOSIVE | 1.20 | 3 | 35 | 0 | 4 |

Enemy fire is applied at ×0.65.

**Penetration vs toughness.** `OBJ_TOUGH = {asteroid 3, structure 4, ruins 3, starbase 5}`.
`pen <= tough` absorbs the shell; otherwise `dmgMod *= max(0.2, 1 − (tough/5)×0.6)`. Cannon pen 2 is
below every toughness, so cannon shells never penetrate cover.

**CANNON** — 3 barrels, 2 s each to arm, fires from a loaded barrel along `heading ± 90` within a 15°
half-arc, auto-selecting the nearer beam. `V` toggles a viewfinder whose 2 s lock is aiming assist
only — firing does not require it.

**RAILGUN** — a **travelling slug** at 4800 u/s over 38,000 u, one in flight at a time. Charging
requires `speed <= 8` and aborts if you accelerate; 4 s charge maps to `pen = 1..5`. Firing sets every
powered system to vent and starts a 6 s recalibration. Fired from the TARGETING view it uses the
firing solution's bearing plus score-derived scatter.

**MISSILE** — 3 s lock, 320 u/s, 2.2 rad/s turn, 12,000 u range, 8 s reload, flat 42 damage.

**BOARDING** — see §9; the live implementation is not the one it first appears to be.

**PDC** — a toggle (`O`). Kills the nearest fighter within 2600 u, else intercepts the nearest enemy
projectile within 1800 u, cooldown `max(0.12, 0.38 − 0.045×bars)` / `max(0.14, 0.42 − 0.05×bars)`.

---

## 7. Sensing and knowledge

**Three scales, three frames.** SONAR 8000 u ship-relative heading-up, terrain at **true world
scale**. RADAR 40,000 u ship-relative, **uniform fixed-pixel triangles**. MAP 50,000 u **north-up**,
uniform chart symbols. Stated in the comments as a deliberate doctrine: judge *size* on sonar,
*position* on radar, *layout* on the map.

**Radar is a discrete pulse.** Every `radar.interval` it rebuilds `radar.blips` as a snapshot of
world positions. Between pulses those positions are stale — but they are re-projected every frame
through the live ship transform, so contacts lag while your own parallax stays smooth. This is the
single most distinctive sensor behaviour in the build.

**Discovery.** A world object enters `discoveredSet` at `dist < SONAR_RADIUS + obj.r × 0.5`, so large
landmarks announce themselves earlier. Monotonic and permanent. Consumed by exactly two places: the
MAP's object list, and the TARGETING contact list (OR'd with a live range check).

**Scanner minigame.** A marker oscillates 0→1 at 0.85/s. `F` inside the zone gives +0.22 progress,
re-randomises the zone to a narrower half-width (`max(0.08, 0.14 − progress×0.10)`) at a new centre,
and speeds the marker by +0.04 — it gets harder as it succeeds. A miss costs 0.04. Passive trickle
`0.005 × (bars + loadoutBonus)` per second. Range by bars: `[0, 6500, 8500, 11000, 13500, 16000]`.
Completion writes hull, crew, heading, signature and all 7 internals, then a 12 s cooldown.

**Contact classification** — one function feeding five surfaces, and the strongest piece of
information design in the build:

```
destroyed        → CARRIER WRECK
scanned          → VAIA ASURAN // COMMAND CARRIER
cipher decoded   → COMMAND CARRIER
within 8000      → LARGE WARSHIP RETURN
within 40000     → awareness≥1 ? LARGE MASS RETURN : DISTANT MASS RETURN
else             → UNKNOWN
```

The label string *is* the knowledge state.

**Comms — three stages.** Signal spawns at mission time 10 s.
*Tuning*: channel 1–3 (correct is 2) plus frequency 1–100 (target 73, tolerance ±2), A/D at 28/s.
Feedback is a beep whose interval closes from 1.05 s to 0.10 s and whose pitch rises with closeness —
**you can tune by ear alone**.
*Cipher*: a real Vigenère over `CARRIER NORTHSKY REPORTS GRID 09F VECTOR NORTH WEST` with key `WOLF`
(the ship's own callsign, hinted as `HINT: CALLSIGN`). Four wheels; `Q`/`E` select, `A`/`D` rotate,
`F` submits. The preview turns white the instant the guess is correct.
*Payoff*: sets `enemyDiscovered` regardless of range and drops a `PROBABLE CARRIER ZONE` marker at
`enemy + (900, −700)` with radius 2600 — deliberately approximate, and a one-shot snapshot that does
not follow the carrier.

Comms is the **non-spatial discovery channel** — the only way to find the carrier without flying
within 8000 u of it, across an 85,900-unit opening separation.

---

## 8. Threat escalation

**Awareness ladder**, monotonic, never decays:

| Condition | Raises to |
|---|---|
| `dist < 18000 && sig > 2.0` | 1 UNEASY |
| `dist < 12500 && sig > 3.0` | 2 SEARCHING |
| `dist < 8500 && sig > 4.5` | 3 ALERT |
| alert active + reinforcements spawned | 4 GENERAL ALARM |

**Alert net.** `triggerAlert()` fires on *any* weapon discharge — and note it runs **before** the
validity check, so a dry trigger pull wakes it. 59-second response window, then two reinforcements at
once, then one every 14 s (7 s once the carrier dies), alternating DESTROYER / FRIGATE, entering from
a random map edge at 52,500 u and beelining at 92 u/s (210 after the carrier dies), ramming for 400
damage at 320 u.

**Fleet.** One CARRIER with four-sided hull, crew, and its own 7 internals; three named escorts
orbiting at 1500–2500 u; fighter pairs launched every 10 s while alerted. Escorts break orbit to
`huntState.lastKnown` at awareness ≥2 and to the player once alerted.

**Carrier AI.** PATROL (5 waypoints) → ALERT (`awareness ≥ 2 || (dist < 12500 && sig > 2.5)`) →
ENGAGE (`dist < 5200`). In ENGAGE it steers `bearing + 90°` to present a broadside and fires within
±22° of the beam at `dist < 7000`.

Kill conditions are compound — hull ≤ 0, **or** reactor destroyed, **or** bridge+engines destroyed,
**or** crew ≤ 0, **or** all internals destroyed.

---

## 9. Two things that are not what they look like

### Boarding: the visible implementation is dead code

`getBoardingSide`, `getBoardingStatus`, `fireBoardingPods`, `updateBoarding` and `updatePDC` are each
**declared twice** — at lines 2000–2075 and again at 2891–2943. JavaScript function declarations
hoist, so **the later definition wins**.

| | Earlier (lines 2000–2075) — DEAD | Later (2891–2943) — LIVE |
|---|---|---|
| Pods | all 5 committed at once | `pods--`, one per launch |
| Marines | 10 gundharva | none |
| Crew damage | `crewLoss = marines × 2` = 20 | **none** |
| Targets | carrier and escorts | carrier only |
| Outcome | crew casualties, possible crew-kill | 2× `breachInternal` + a **free full scan** |

Consequence: because live boarding never touches crew, the **crew-kill condition in
`checkEnemyKilled` is unreachable** — the carrier's 70 crew can never be reduced.

Launch gate (live): target alive → pods available → not already boarding → `dist < 900` → within an
**18° port-arc** → **that hull side already breached** → relative speed < 90. Then an 18 s timer.

Anyone porting this must decide which version is intended. The dead one looks like newer intent.

### There is no way to lose

`lifeSupTimer` exists in exactly three places: declared, incremented in `updateInternals`, and read
once to print `LIFESUP FAIL 90s` as a warning string. It is **never compared against
`LIFE_SUPPORT_TIME` for a game-over**. There is no `gameOver`, no defeat state, and no code path
anywhere reads player hull for a loss check. You can sit at 0/0/0/0 armour with every internal gutted
indefinitely.

Soham's older Unity code has four typed failure reasons (`QuestSystem.cs`). The tactical board does
not use it. On the board a ship is lost in two ways: its reactor room reaches integrity 0
(`DamageSystem.cs`), or it stays outside the battle space too long (`HandoffBoardState.TickArena`).

---

## 10. Other unfinished edges in the prototype

Recorded so a port does not faithfully reproduce them:

- SONAR range labels read `1250 / 2500 / 5000` for rings at 2000/4000/6000/8000 — every label is
  0.625× the true value, authored for a 5000 u scope.
- The RADAR compass rose is painted at fixed screen angles while the projection is heading-up, so it
  reads as ship-relative bearings, not true headings.
- `// Nebula masks itself from radar` sits above a loop that pushes nebulae unconditionally.
- `// Enemy — shows last known position when discovered` describes a system that does not exist; once
  discovered, the carrier is drawn live forever at any range.
- `extractionDiscovered` is set and never read.
- MAP passes a white CRT tint despite a comment specifying amber.
- Neutral ships are decorative — excluded from collision and targeting; the briefing's civilian
  objective has no implementation.
- The `.barrel.loading` state has CSS and render support but is never assigned.
- Cannon fighter kills increment `pdc.kills`.
- Reactor-damage cells write inline styles that later class writes cannot clear.
- The nebula is a hard binary at the circle edge — no falloff, no hysteresis.

---

## 11. Mission flow

`MISSION → LOADOUT → WORMHOLE (3.6 s) → GAME`, then a completion banner reporting armour remaining
and whether a general alarm was survived. `Enter` reloads.

| Loadout | cannon | railgun | missile | hull/side | scanner | radar interval | aux start |
|---|---|---|---|---|---|---|---|
| STANDARD KILL | 15 | 3 | 4 | +0 | +0 | 3.0 s | 0 |
| GHOST KILL | 9 | 5 | 2 | +0 | +1 | 2.4 s | 20 |
| BRAWLER | 24 | 2 | 5 | +12 | +0 | 3.4 s | 10 |

Sector: 46 authored world objects — 21 asteroids in three belts, 5 named wrecks, a compound base husk
of 7 sub-parts you can fly between, 2 starbases, 3 nebulae, 7 neutral traffic ships, 1 structure, 1
unknown contact. Player starts at (−37,800, 23,400); the carrier sits at (32,400, −26,100).

---

## 12. Full key map

| Key | Action |
|---|---|
| `W` / `S` | step target speed ±15 |
| `A` / `D` | turn; in COMMS, tune frequency / rotate cipher wheel |
| `Q` / `E` | lateral thrust; in COMMS cipher, select wheel |
| `1`–`5` | cycle power on ENGINES / WEAPONS / SCANNER / COMMS / AUXILIARY; in COMMS, select channel 1–3 |
| `F` | context fire — COMMS lock, SCANNER pulse, TARGETING force-railgun, else active weapon |
| `C` | railgun charge/abort, or missile lock |
| `V` | targeting zoom, else cannon viewfinder |
| `R` | cycle targeting contact |
| `T` | cycle centre view |
| `O` | toggle PDC |
| `P` | pause |
| `Z X B N M` | aux: repair, reload, distort, flush, damage control |
