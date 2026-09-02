# Starwolf — Unity Parity Plan

> **Written before any of this work started.**
> Every work item assumes nothing has been implemented yet. Check git history for the files
> an item names before starting it.

**Goal.** Bring the Unity build to functional parity with `References/starwolf-prototype.html` while
preserving what Unity has and the prototype does not: the shader cockpit, gamepad and haptics, the 3D
physics and camera, the editor tooling, and the mission-failure scaffolding.

**Framing.** This is not a rewrite. The prototype is the same game with a different *systems spine*.
Most Unity classes survive with edits; the rewrites concentrate in four files — `PowerManager`,
`PlayerShip`, `InternalSubsystems`, `Radar` — plus three concepts Unity has no equivalent for at all:
energy signature, auxiliary charge, and threat escalation.

Read alongside `Assets/CODEBASE_BASELINE.md` (what Unity does today) and
`Assets/PROTOTYPE_BASELINE.md` (what the target behaviour is).

---

## Dependency order

```
A. SPINE          WI-1 PowerManager reshape  ──┬──> WI-2 EnergySignature
   (nothing else                               ├──> WI-3 Internals (graded)
    is correct                                 └──> WI-4 Movement (drift)
    until these land)

B. COMBAT         WI-5 damage service + PEN/TOUGH ──> WI-6..10 per weapon

C. SENSING        WI-11 ThreatDirector (needs WI-2)   WI-12 Enemy rework
                  WI-13 Sensors + discovery           WI-14 Scanner backend
                  WI-15 Comms cipher                  WI-16 Auxiliary

D. SESSION        WI-17 Pause service (must precede WI-14/15)
                  WI-18 Mission flow + loadouts
```

**Nothing in phases B–D should start before WI-1 to WI-4 are merged**, or every one of them needs
re-tuning against new power and movement numbers.

---

## Phase A — Spine

### WI-1 · Reshape `PowerManager` — LARGE

The highest-leverage item. Everything downstream reads power by system id, and energy signature is
literally the sum of all bars.

Three sub-changes:

1. **Identity.** Unity `engines/arms/bay/support/sig` → prototype `ENGINES/WEAPONS/SCANNER/COMMS/AUXILIARY`.
   `bay` and `support` have no prototype counterpart; `SCANNER`, `COMMS`, `AUXILIARY` have no Unity one.
2. **Reactor model.** Unity is a *wallet* (15 units, 0.5/bar, refunded on vent). The prototype is an
   *allocation cap* (10 bars, `avail = effMax − Σ bars`). Bars are reserved, not spent.
3. **Switch cycle.** Unity's five-step `Standby→Draw→Standby→Vent→Standby` with a `readyToVent` latch
   and uncancellable vent → the prototype's flat three-cycle. Fill and vent rates already match.

**Files:** `PowerManager.cs`; `UIController.cs` **line 208** (the single line binding the UI to system
identity — everything else is index-based, so renaming is cheap in code and expensive only in
serialized data and label art); `UIController.StartPower` / `SyncPowerBarsFromManager` /
`ChargeNodeCheck`; `WireUIReferencesEditor.cs`; every `GetSystemEfficiency` caller.

**Fix here, not later:** `ChargeNodeCheck` calls `PowerNodes(5, 7)` unconditionally and has **no
reactor-full test** — the power-arrow defect the UI author reported. "Reactor full" only becomes
definable once the allocation-cap model lands.

**Reused:** `PowerState`, `[DefaultExecutionOrder(-10)]`, the whole `UIPowerClass` shader pipeline,
`PowerManagerEditor`'s play-mode repaint (which is how this gets debugged).

### WI-2 · `EnergySignature` — SMALL

`Σ bars × 0.5 if distorting × 0.6 if flushed`, then `× nebulaFactor` at the consumption site. Tiny to
write, but it is the input to the entire threat layer. Build it immediately after WI-1.

### WI-3 · Internals: graded compartments — MEDIUM

Compartment set and hull-side assignment already match exactly — that is the expensive part and it is
right. What changes: 100-point floats → 3 hp discrete with stepped effect curves; `breachInternal`
must pick among **living** compartments and **spill ship-wide** once a side is gutted; life support
90 s counting up at half rate under Damage Control; reactor drain 8 s and **bar shedding** on overdraw.

Also here: `HullSystem` lines 121–125 are an `if (wasBreached && overflow > 0f)` block whose body is
entirely comments — decide whether to route overflow through it or delete it. Quadrant maxima need a
tune: Unity's aft is 10 of 150 (6.7%), the prototype's is 40 of 190 (21%).

### WI-4 · Movement model — LARGE

Unity hard-assigns `rb.linearVelocity = transform.up × currentSpeed`, so the ship cannot slide.
The prototype rotates the hull only and bleeds lateral velocity. `W`/`S` become stepped, `Q`/`E`
become held thrusters, three accel rates replace one, and `ShipStability` shifts from
degrees-per-frame bands to speed bands with flat 10/s regen.

`PlayerShip`'s `Drift Settings` block already exists and is unused — someone intended this.

**Trap:** `Autopilot.ApplyThrust` also hard-assigns `rb.linearVelocity` and never touches
`ShipStability`. It will fight the drift integrator.

---

## Phase B — Combat

### WI-5 · Damage service + PEN/TOUGH — MEDIUM

One shared formula replaces per-projectile constants:
`round(max(baseDmg, distanceTravelled × perUnit) × typeMod × dmgMod)`, enemy fire ×0.65.
`Macrocannon.Fire` already implements this shape — it needs `typeMod` and distance *travelled*.

New: projectile `pen` vs `OBJ_TOUGH`, and — critically — an **owner/team field**. `Shell`,
`PDCBullet` and `Missile`'s blast currently have no owner concept at all and damage the firing ship.
This must land before WI-11 puts fighters inside 1000 units of the player.

### WI-6..WI-10 · Per weapon — MEDIUM each

- **CANNON** — merge `Macrocannon` and `BroadsideCannon`; tighten the wedge from ±45° to ±15°; fire at
  the selected beam rather than always `±transform.right`.
- **RAILGUN** — hitscan raycast → travelling slug (4800 u/s, 38,000 u), charge gated on `speed ≤ 8`,
  PEN 1→5, vent-all rather than drain-all, 6 s recal.
- **MISSILE** — multi-tube salvo → one guided round; removes the never-arms bug, the coroutine
  stacking, and the loaded-but-unlocked `CanFire` hole.
- **BOARDING** — flying pod → the prototype's gate-chain-as-UI. **Decide D11 first.**
- **PDC** — spin-up turret → toggle + range check. Deletes more code than it adds; Unity's is inert
  anyway because nothing calls `SpinUp`.

---

## Phase C — Sensing, minigames, threat

### WI-11 · `ThreatDirector` — MEDIUM

Awareness 0–4, the 59-second alert net, reinforcement waves. Entirely new, entirely dependent on WI-2.
This is what makes the game a stealth game rather than a shooter. Reuses
`GameManager.FindSpawnPosition` for edge spawning and `UIController`'s comms log for narration.

### WI-12 · Enemy rework — LARGE

The prototype has a heterogeneous authored fleet with per-side hull, crew and internals on the
carrier. Unity's `EnemyShip` carries a single int and does not use `HullSystem` at all — which is why
`GameManager` death tracking falls through to a per-frame null-poll coroutine.

Recommended: give `EnemyShip` an optional `HullSystem` + `InternalSubsystems` for capitals and keep
the integer path for fighters. Least churn to existing prefabs.

### WI-13 · Sensors, pulse radar, discovery — MEDIUM *(architecture decision D7)*

Unity recomputes every blip every frame and never calls `RadarRange`. The prototype's radar is a
stale snapshot re-projected through a live transform. Plus discovery, nebula masking, and six tiers of
contact classification — none of which Unity has anywhere to store (`RadarTarget` has three fields;
`BogieClass` carries the author's own note `//need more data like stats etc`).

### WI-14 · Scanner backend — MEDIUM

**Unity is ahead here.** Its 30-slot hull-side pip track on the `UI_ShipHack` shader is richer and
more on-theme than the prototype's 1-D bar. The work is to *back* it, not replace it: gate on SCANNER
bars, apply the range table, track progress and cooldown, and produce a scan record that raises the
contact's classification tier and feeds the targeting solution.

### WI-15 · Comms cipher — MEDIUM

Unity implements Receive only. `TryLockSignal` is private with no caller — it is meant to be registered
through `SetupCommsListeners`, which nothing calls. Meanwhile `FrequancyTune` has **24 inspector
bindings** and a physical rotating dial already built, driving nothing. Add the Vigenère stage and the
intel payoff; `AddCommsLog` also needs to append rather than overwrite (the prototype keeps 16 rolling
entries with timestamps).

### WI-16 · `AuxiliarySystem` — SMALL–MEDIUM

Entirely new. Five actions, charge table, status-string feedback. `UIController.StabilityMeter(float)`
exists with no callers and `UI_GradeDisplayShipStability` is the closest existing shader to reuse for
the charge meter.

---

## Phase D — Session

### WI-17 · Pause service — SMALL, but blocks WI-14/15

`Time.timeScale = 0` freezes the cockpit's own animations and every `WaitForSeconds`. Build a
`SimulationPaused` flag that gameplay bodies check, leave `timeScale` at 1, and repoint `GameClock`.

### WI-18 · Mission flow + loadouts — MEDIUM

`QuestSystem` is **already the right shape** — stages, jump point, typed failures wired in `Start()`.
The only gap: nothing calls `StartMission`, and `EndSequence` has an empty body. Add three loadouts as
a ScriptableObject applied during `SpawnPlayer`, and the MISSION → LOADOUT → WORMHOLE → GAME phases.

---

## Decisions for the owner

| # | Question | Notes |
|---|---|---|
| **D1** | Rename power systems to match the prototype? | Natural mapping: `bay`(SHD, "not in use")→AUXILIARY, `support`(CRW)→COMMS, `sig`(SEN)→SCANNER. Against: fields are serialized on the prefab **and carried by 328 overrides** in `Prototype Dev.unity`; a rename without `[FormerlySerializedAs]` zeroes them. Three label sprites need new art. Third option: keep field names, add a `systemId` used by new code. |
| **D2** | Reactor: wallet or allocation cap? | **Recommend the cap.** The UI author's power-arrow defect has no clean fix under the wallet model, and reactor damage becomes legible. |
| **D3** | Keep `Shields.cs`? | Prototype has none; GDD p47 says none. It is also broken — absorbs the entire hit with no overflow, and never regenerates below full. Options: delete and edit the two damage-path branches; or repurpose the colour-lerp/blink coroutines as the AUX distortion visual. **Not** "disable and keep" — it will be re-enabled by accident. |
| **D4** | Keep the Laser? | Not in the prototype, but it is the primary weapon on the prefab and drives `UIController`'s beam animation. **Trap:** `WeaponType.Laser` is ordinal **0** — removing it remaps every serialized `WeaponSlot`. Retire by marking obsolete, never delete. |
| **D5** | `Macrocannon` or `BroadsideCannon`? | Merge. `Macrocannon` has the damage formula; `BroadsideCannon` has the arc targeting and is the one on the prefab. |
| **D6** | Railgun: beam or slug? | The slug makes the targeting solution and its scatter matter. But `UIController.RailFire()` animates an instantaneous beam and needs a flight-time variant. |
| **D7** | **Sensor screens: orthographic cameras or contact-list rendering?** | The deepest question here. Unity points real cameras at world geometry — showing everything that exists. The prototype's whole sensor design is a *knowledge model*: discovery, stale pulses, classification tiers. There is no cheap way to render "a 3-second-old position of a contact you have detected but not classified" from a live camera. **Answer this with the UI author present.** |
| **D8** | Keep hyperspeed/supercruise? | Prototype has neither; Unity has hyperspeed (4× at 0.8 efficiency); GDD says 3× at max power. Three sources, three answers. |
| **D9** | `Q`/`E`: dodge or lateral thrusters? | Different verbs on the same keys. Keeping both means finding two more bindings on a scheme where `1`/`2`/`3` are already triple-booked. |
| **D10** | Boarding: flying pod or abstract resolution? | The prototype's gate-chain reads better as a cockpit interaction; the pod has a prefab and three known bugs. |
| **D11** | **Which boarding implementation is intended?** | The richer five-pod/marine version is **shadowed at runtime** by a simpler carrier-only one. Porting the wrong one ships behaviour you have never played. |
| **D12** | When GDD, prototype and code disagree, which wins? | One ruling removes a dozen later arguments. |
| **D13** | Fleet: spawner or hand-placed? | `GameManager` scatters prefabs in a 30–80 unit band; the prototype hand-places a carrier, three named escorts on orbit phases, and 46 world objects. |
| **D14** | **Does the game need a lose condition?** | The prototype has none — no defeat state anywhere. Unity has four typed failures already wired. This is the one place to keep Unity's design outright. |

---

## Risks and sequencing traps

1. **Serialization.** `PowerSystem` fields are serialized on `PlayerShip.prefab` and masked by 328
   property overrides in `Prototype Dev.unity`. Use `[FormerlySerializedAs]` on every rename and
   verify in the live Editor.
2. **`WeaponType` ordinals are load-bearing.** Append only; retire by marking obsolete.
3. **`UIController` serialization vintages differ** — scenes hold 5–27 fields, `Canvas.prefab` 64,
   `PlayerShip.prefab` 99. Only the prefab reflects the current script. Do UI wiring from the prefab.
4. **`UIController.Awake` destroys the ship.** It calls `Destroy(gameObject)` on a duplicate and lives
   on `PlayerShip.prefab`. Fix before WI-18 adds scenes and additive loading.
5. **Clear input collisions first.** `1`/`2`/`3` are simultaneously power toggles, `LaserFireEnemy`,
   comms band select and test stubs. `Space` is fire, railgun charge **and** `ScanPlaceHack`. `R` is
   radar range, `RailFire` **and** a per-frame test `GetKey`. Put `TestUIKeyControler` and
   `WeaponSystemExample` behind `#if UNITY_EDITOR` before any input work.
6. **`AddBogie` throws on every call** — line 1253, `=` instead of `==`. Nothing calls it today, which
   is the *only* reason `WorldGridZoom`'s unguarded bogie loop, `LaserFireEnemy`'s unbounded index and
   `ScanNewTarget`'s off-by-one are harmless. **The first work item that populates contacts detonates
   all four at once.** Fix them as a prerequisite to WI-11/WI-13.
7. **Radar blips are unwired.** `radarBlipContainer` and `radarBlipPrefab` are null in
   `Prototype Dev.unity`, and `CreateOrGetRadarBlip` null-guards and returns null. Assign before WI-13
   or the work will look broken when it is merely unwired.
8. **`Time.timeScale = 0` is not the pause you want.** WI-17 before WI-14/WI-15.
9. **Friendly fire.** Add owner/team in WI-5 before WI-11 puts fighters near the player.
10. **Physics stays 3D.** Port the prototype's scalar 2D maths into Unity's existing convention
    (Z frozen, `transform.up` as the bow). Do not switch to 2D physics.
11. **`UIController` is jointly owned in practice.** 2,052 lines, 13 blocks of commented-out code, and
    the UI author's shader work throughout. Coordinate edits.

---

## What Unity keeps

| Asset | Role in the target design |
|---|---|
| The shader cockpit and `Canvas.prefab` | The product. Map prototype *data* onto existing shader properties; never build a parallel widget. |
| The 30-pip hull-side scanner (`UI_ShipHack`) | Better than the prototype's 1-D bar. Keep and back it. |
| The power node arrow chain (9 states) | No prototype equivalent. Keep. |
| The comms tuner dial (24 bindings) | Already built, drives nothing. WI-15 gives it a job. |
| `ControllerHaptics` (14 presets) + the gamepad map | Prototype is keyboard-only. Keep and extend to aux and alert escalation. |
| `WorldBoundary`, `GameClock`, `QuestSystem` | The prototype's missing half — failure, timing, mission state. |
| `WireUIReferencesEditor` | Load-bearing, not a convenience. `UnityEvent` persistent calls are scene data; this is the only automated path that connects the HUD. Mirror every rename here in the same commit. |
| `Autopilot` | Prototype has none; the GDD does. Post-parity — and note it will fight WI-4. |
