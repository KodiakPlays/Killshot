# Starwolf — Baseline of Soham's Older Scripts

**What this covers.** Soham's older scripts, read line by line as they were on 2026-09-01. The
board's code is newer and is not in this file: `Assets/Scripts/Systems/`, `Assets/Scripts/Tactical/`,
`Assets/Scripts/UI/`, `Assets/Scripts/Game/` and `Assets/StarwolfIntro/`.
`Assets/Scenes/Tactical/BUILD_LOG.md` section 3 describes the board's code.

**Changes to Soham's scripts since 2026-09-01.** There are three:

- `UIController.cs` (2026-09-02): it returns early where a panel's canvas is missing, so its
  `Update` runs to the end in the scenes under `Assets/Scenes/Tactical/`.
- `PowerManager.cs` (2026-10-26): its serializable class `PowerSystem` is now called `PowerChannel`.
  `PowerSystem` is now the board's power code, in `Assets/Scripts/Systems/PowerSystem.cs`.
- `Editor/WireUIReferencesEditor.cs` (2026-10-27): deleted, with its two menus.

**Purpose.** A line-by-line reference for Soham's scripts, written so a future session can
understand the code without re-reading it. Every statement here comes from reading the file in full,
not from search results.

**Project naming.** The game is **Starwolf**. `Killshot` was the repository code name. Docs, the
`Starwolf/` editor menu, and the demo build all use the Starwolf name.

**Status of this document.** Describes the code as it is, not as it should be. Where behaviour
differs from a design document, that is recorded as an observation, not a defect.

**The code and the running Editor win over this file.** It will drift. See `CLAUDE.md`.

---

## 1. Source inventory

Soham's runtime code is in the global namespace and compiles into `Assembly-CSharp`. Editor code is
in `Assets/Editor/` and compiles into `Assembly-CSharp-Editor`. There are no asmdefs. Soham's code
uses no namespaces. The newer intro code in `Assets/StarwolfIntro/Runtime/` uses `namespace Starwolf.Intro`.

| Area | Path | Files |
|---|---|---|
| Mission / session | `Assets/Scripts/` | `GameManager`, `GameClock`, `QuestSystem`, `WorldBoundary` |
| Ship systems | `Assets/Scripts/` | `PlayerShip`, `PowerManager`, `HullSystem`, `InternalSubsystems`, `ShipStability`, `Shields`, `Autopilot` |
| Enemy / environment | `Assets/Scripts/` | `EnemyShip`, `Asteroid` |
| Sensors / comms | `Assets/Scripts/` | `Radar`, `RadarTarget`, `CommsManager` |
| Input feedback | `Assets/Scripts/` | `ControllerHaptics` |
| Targeting | `Assets/Scripts/Targeting/` | `TargetingSystem`, `Targetable`, `TargetingReticle` |
| Weapons | `Assets/Scripts/Weapons/` | `WeaponBase`, `WeaponManager`, `WeaponType`, `Railgun`, `LaserWeapon`, `Laser`, `Macrocannon`, `Shell`, `BroadsideCannon`, `MissileLauncher`, `Missile`, `PointDefenseCanon`, `PDCBullet`, `BoardingPod`, `BoardingPodLauncher`, `WeaponUIDisplay`, `WeaponSystemExample` |
| Legacy | `Assets/Scripts/` | `Weapons.cs` — `[Obsolete]` wrapper delegating to `LaserWeapon` |
| Interfaces | `Assets/Scripts/Interfaces/` | `IDamageable` |
| UI | `Assets/UI/UI_Script/` | `UIController`, `BogieClass`, `UIPowerClass` |
| Test scaffolding | `Assets/` | `TestUIKeyControler.cs` |
| Editor tooling | `Assets/Editor/` | `PowerManagerEditor`, `ReadOnlyDrawer` (`WireUIReferencesEditor` was deleted on 2026-10-27) |

---

## 2. Architecture

Four layers, loosely coupled by events and one UI singleton.

```
   MISSION        GameManager ──spawns──> PlayerShip prefab, EnemyShip prefabs
                  GameClock / QuestSystem / WorldBoundary
                       └─ QuestSystem.Start() finds the other two and subscribes to their
                          loss events. GameManager does NOT wire these three together.

   SHIP           PlayerShip (hub, on PlayerShip.prefab with everything below)
                    ├─ PowerManager ──────> reactor economy, 5 systems
                    ├─ HullSystem ────────> 4 armour quadrants
                    │     └─ OnHullDamaged ──> InternalSubsystems (7 internals)
                    ├─ ShipStability ─────> manoeuvre resource
                    ├─ Shields ───────────> pre-hull damage absorber
                    └─ WeaponManager ─────> weapon slots

   WEAPONS        WeaponManager ──> WeaponSlot ──> WeaponBase subclass
                       └─ spawns projectiles ──> IDamageable

   UI             UIController (singleton, ALSO on PlayerShip.prefab)
                       every system pushes into it; it does not poll back,
                       except power bars and the hull display which it polls on timers.
```

**Damage path.** Projectile → `Shields` (if present and charged) → `PlayerShip.TakeDamage(dmg, dir)`
→ `HullSystem.DetermineHitSide(dir)` → quadrant → once breached, overflow into `InternalSubsystems`.

**Coupling mechanisms in use**, in rough order of frequency: the `UIController.Instance` singleton;
C# `event Action` (hull, subsystems, clock, boundary, quest); the `IDamageable` interface;
`GetComponent` / `GetComponentInParent` caching in `Awake`/`Start`; `GameObject.FindGameObjectWithTag("Player")`;
and static self-registration (`Radar.RegisterTarget`).

---

## 3. Scene and prefab wiring

Determined by resolving every script `.meta` GUID against every scene and prefab file.

**`Assets/Prefabs/PlayerShip.prefab`** carries, on one GameObject:
`PlayerShip`, `PowerManager`, `HullSystem`, `InternalSubsystems`, `ShipStability`, `Shields`,
`Radar`, `WeaponManager`, `LaserWeapon`, `BroadsideCannon`, `Railgun`, `BoardingPodLauncher`,
and **`UIController`**.

Consequences worth knowing:
- `UIController` lives on the ship, so its `Update()` input handling runs as gameplay input.
- `UIController.Awake` does `Destroy(gameObject)` on a duplicate — which destroys the whole ship.
- `Macrocannon`, `MissileLauncher`, `PointDefenseCanon`, `WeaponUIDisplay`, `Autopilot`,
  `TargetingSystem`, and `Asteroid` are on no prefab and in no scene.

**Scenes**

This table is as of 2026-09-01. The board's scenes are newer: `Tactical/00_Start`, `Tactical/01_Briefing`,
`Tactical/02.1_Grid` and `Levels/Level_01`. The working scene is now `Tactical/02.1_Grid`. See
`Assets/Scenes/Tactical/BUILD_LOG.md` section 2.

| Scene | Contents | Role |
|---|---|---|
| `Prototype Dev.unity` | mission trio + ship, 328 property overrides on the ship instance | the working scene |
| `Demo.unity` | GameManager, PowerManager, UIController, ship | |
| `Prototype.unity`, `Canvas_CRT.unity` | GameManager, UIController, ship | |
| `UI.unity` | UIController + test driver, no ship | UI harness |
| `TestScene.unity` | GameManager, `CommsManager` | only scene with CommsManager |
| `Ship_Gallery.unity`, `vfx.unity` | no gameplay scripts | art scenes |

**`UIController` serialization vintages.** Field counts differ per instance, meaning each was last
saved against a different version of the script: scenes 5–27 fields, `Canvas.prefab` 64,
`PlayerShip.prefab` 99. Only the prefab reflects the current script.

**Unassigned inspector references in `Prototype Dev.unity`** (prefab nulls minus scene overrides):
`radarBlipContainer`, `radarBlipPrefab`, `speedometer`, `sensorIcon`, `shipAngleTarget`, `shipPlayer`,
`signalInterceptPanel`, `commsBandDisplay`, `commsFrequencySlider`, `commsSignalStrengthIndicator`,
`commsLockButton`, and `currentBogieTarget` (the last is runtime state, not an authoring field).

**Inspector-wired button handlers** found across scenes and prefabs: `FrequancyTune` (24 bindings),
`ChargeBtn` (15), `VentBtn` (5), `WorldGridZoom` (3), `ToggleWeapons`/`ToggleShields`/`ToggleSensors`/
`ToggleEngines`/`ToggleCrew` (2 each), `BtnBogieTab` (1).

---

## 4. Input map

Both the legacy Input Manager and the new Input System are enabled (`activeInputHandler: 2`).
All gameplay reads legacy `Input.*`. Only `ControllerHaptics` and `Railgun` use `UnityEngine.InputSystem`,
and only for gamepad rumble.

Custom axes the project depends on existing in the Input Manager: `RightTrigger`, `LeftTrigger`,
`DPadX`, `DPadY`, `ControllerThrust`, plus the built-in `Horizontal`.

| Key | `PlayerShip` | Also bound elsewhere |
|---|---|---|
| `W` / `S` | thrust forward / reverse | `TestUIKeyControler` speed test |
| `A` / `D` | turn (via `Horizontal`) | |
| `Q` / `E` | dodge left / right | `TestUIKeyControler` delegates to PlayerShip |
| `Space` | fire active weapon; charge Railgun | **`UIController` → `ScanPlaceHack()`** |
| `LeftCtrl` | fire active weapon | |
| `1` / `2` / `3` | toggle Engines / Arms / Sig | **`UIController` → `LaserFireEnemy(1/2/3)`**; `CommsManager` band select; `TestUIKeyControler` |
| `0` / `4` | — | `UIController` → `LaserFireEnemy(0/4)`; `TestUIKeyControler` scan tests |
| `V` | emergency vent | |
| `N` | black alert | |
| `LeftShift` | hyperspeed toggle | |
| `F1` | movement debug dump | |
| `F2` | weapon power override | |
| `R` | — | `Radar` range cycle; **`UIController` → `RailFire()`**; `TestUIKeyControler` |
| `P` | — | `UIController` → `LaserFire()` |
| `/` | — | `UIController` → `BogeySpot()` |
| `]` | — | `UIController` → `ScanNewTarget()` |
| `B` | — | `TestUIKeyControler` ship-hit shake |
| `←` / `→` | — | `CommsManager` frequency tune |

**Gamepad** (`PlayerShip`): RT fire, LT+RT charge Railgun, B hyperspeed, LB/RB dodge, Y cycle weapon,
D-pad power toggles (Up=ENG, Down=WEP, Left=CRW, Right=SEN), A toggle Support, X cycle world-grid zoom,
Select emergency vent.

---
## 5. Mission layer

### GameManager.cs

Session bootstrap. Plain singleton (`Instance` set in `Awake`, duplicates destroyed; **no**
`DontDestroyOnLoad` despite what `DOCUMENTATION.md` claims). `Start()` calls `SpawnPlayer()` then
`SpawnAllEnemyGroups()`.

`EnemyGroup` is a serializable inspector row: `prefab`, `count` (3), `patrol` (true),
`respawnOnDeath` (false), `respawnDelay` (10s), and a `[HideInInspector] activeCount`.

| Setting | Default |
|---|---|
| `minScatterDistance` / `maxScatterDistance` | 30 / 80 |
| `minSeparation` | 5 |
| `maxSpawnAttempts` | 20 |
| `winOnAllEnemiesDestroyed` | true |
| `endSequenceDelay` | 3s |

**Spawn placement.** `FindSpawnPosition()` picks a random angle and a distance in the scatter band
from `spawnCentre` (world origin if unassigned), and accepts it if `IsClearPosition` finds no existing
enemy within `minSeparation`. After `maxSpawnAttempts` failures it returns an unchecked ring position.

**Death tracking has two paths.** It first tries to subscribe to the spawned enemy's
`HullSystem.OnShipDestroyed`. `EnemyShip` carries its own integer health and no `HullSystem`, so in
practice every enemy falls through to `WatchEnemyDestroy`, a coroutine that yields every frame until the
GameObject reference goes null, then reports the death. `OnEnemyDestroyed` decrements the group and
global counts, optionally schedules a respawn, and calls `CheckWinCondition`.

**End of session.** All enemies dead → `EndSequence(won: true)`. Player hull destroyed →
`EndSequence(won: false)`. `EndSequence` sets `gameOver`, logs VICTORY or DEFEAT, waits
`endSequenceDelay`, and then does nothing — the body is explicitly an empty hook for scene loading
or an end screen.

**Observations.** No `Update()` and no reference anywhere to `GameClock`, `QuestSystem`, or
`WorldBoundary`; the Escape-to-pause and mission-wiring behaviour described in `DOCUMENTATION.md`
is not present. `public class ReadOnlyAttribute : PropertyAttribute { }` is declared at the bottom
of this file (line 285) rather than beside its drawer in `Assets/Editor/`.

### GameClock.cs

Singleton universal clock. `missionTimeLimitSeconds` 3600, `timeScale` 1.

`Update()` accumulates `elapsedGameTime += Time.deltaTime * timeScale` while `isMissionActive` and
not paused, firing `OnTimeUpdated(elapsed)` every frame. Crossing the limit sets `isMissionActive = false`
and fires `OnMissionTimerExpired` once.

`StrategicPause()` / `Resume()` set the internal flag **and** Unity's global `Time.timeScale` to 0 / 1,
firing `OnPaused` / `OnResumed`. `TogglePause()` flips between them.

Helpers: `GetElapsedTime()`, `GetRemainingTime()` (−1 when no limit), `IsPaused()`, `IsMissionActive()`,
`GetTimeScale()`, `SetTimeScale(float)` (clamped ≥ 0), static `FormatTime(seconds)` → `HH:MM:SS`,
and `CalculateETA(from, to, speed)` → distance/speed, or −1 when speed ≤ 0.

**Observation.** Nothing calls `TogglePause()`; there is no pause key bound anywhere in the project.

### WorldBoundary.cs

Circular playable area centred on the world origin. `playableAreaRadius` 50000, `warningDistance` 5000,
`desertionTimerDuration` 30s. Finds the player by the `"Player"` tag in `Start` if not assigned.

`Update()` computes `distFromBoundary = playableAreaRadius − distance(player, origin)`, then:
- inside the warning band and still in bounds → fire `OnBoundaryWarning` once, set `isWarning`
- beyond the warning band → `ReturnToBounds()` if previously warning or out of bounds
- `distFromBoundary <= 0` → fire `OnOutOfBounds` once, reset the timer, then decrement it every frame
  and fire `OnDesertionTimerTick(remaining)` each frame
- timer ≤ 0 → `TriggerDesertion()`

`ReturnToBounds()` clears both flags, resets the timer, fires `OnReturnedToBounds`.
`TriggerDesertion()` sets `hasDeserted`, fires `OnDesertion`, and calls
`Destroy(playerTransform.gameObject)` — the cowardice self-destruct.

Getters: `IsOutOfBounds`, `IsWarning`, `GetDesertionTimeRemaining`, `GetPlayableAreaRadius`, `HasDeserted`.

### QuestSystem.cs

Singleton mission state machine. No UI of any kind — pure logic.

`QuestStage`: `NotStarted`, `LocateTarget`, `EliminateTarget`, `ExitRegion`, `MissionComplete`, `MissionFailed`.
`MissionFailReason`: `None`, `Destroyed`, `Desertion`, `TimeExpired`, `LifeSupportFailed`.
`locateRegionRadius` 5000.

`Start()` is where the mission systems actually get wired together — it locates
`WorldBoundary`, `GameClock.Instance`, and `InternalSubsystems` in the scene and subscribes their
loss events to `FailMission` with the matching reason. This, not `GameManager`, is the integration point.

`StartMission(Transform target)` stores the target and moves to `LocateTarget`.

Per-stage `Update()`:
- **Locate** — finds the player by tag each frame; when distance to target ≤ `locateRegionRadius`,
  advance. `ConfirmTargetOnRadar()` is the manual alternative for a scanner confirmation.
- **Eliminate** — advances when `targetShip` becomes null, calling `RevealJumpPoint()` first.
- **Exit** — fails with `Destroyed` if the player object is gone; completes when the player is
  within 500 units of the jump point.

`RevealJumpPoint()` places the point at a random angle on a **45000**-unit circle (inside the 50000
boundary), moves and activates `jumpPointMarker` if assigned, and fires `OnJumpPointRevealed(position)`.

Events: `OnStageChanged(QuestStage)`, `OnMissionFailed(MissionFailReason)`, `OnMissionComplete`,
`OnJumpPointRevealed(Vector3)`.

**Observation.** Nothing in the codebase calls `StartMission`, so the chain never leaves `NotStarted`
unless something external supplies a target.

---

## 6. Player ship

### PlayerShip.cs

`[RequireComponent(typeof(ShipStability))]`. Implements `IDamageable`. The hub for input, movement,
camera, and damage entry.

**Assembly in `Awake`.** Adds a `Rigidbody` if absent, configured with no gravity, frozen Z position
and X/Y rotation, and zero linear and angular damping. Then `GetComponent`-or-`AddComponent` for
`PowerManager`, `ShipStability`, `HullSystem`, `InternalSubsystems`, `WeaponManager`; subscribes
`HullSystem.OnShipDestroyed → HandleShipDestroyed`; `FindObjectOfType<TargetingSystem>()`; and falls
back to `Camera.main` for `cameraTransform`, logging an error if the camera is a child of the ship.

| Setting | Default |
|---|---|
| `rateOfAcceleration` | 50 units/s² |
| `turnRate` | 90 °/s |
| `dodgeDistance` / `dodgeDuration` | 10 units / 0.2 s |
| `hyperspeedEngineThreshold` | 0.8 engine efficiency |
| `hyperspeedSpeedMultiplier` | 4× |
| `hyperspeedDrainRate` | 2 bars/s |
| `cameraOffset` | (0, 10, −15) |
| `maxCameraRollOffset` / `cameraRollSpeed` | 4° / 5 |
| `SPEED_UNITS_PER_BAR` | 20 |

**Movement (`FixedUpdate`).** `maxThrust = 100 × engineEfficiency × subsystemSpeedMultiplier ×
hyperspeedMultiplier`. `currentSpeed` moves toward the target by `rateOfAcceleration × fixedDeltaTime`.
Velocity is then hard-assigned as `transform.up * currentSpeed`, so the ship never slides sideways and
turning instantly redirects travel. With no thrust input the target is held at the current speed —
there is no auto-braking. With zero engine power the ship crawls at a fixed **5** units/s as a hint.

**Turning.** `targetTurnSpeed = Horizontal × turnRate × enginePower × subsystemTurnMultiplier`, gated on
stability not being depleted; ramped toward at `turnRate × 2 × fixedDeltaTime` so turns carry momentum.
Each frame of rotation calls `ShipStability.CalculateTurnStabilityDrain(degrees, speed, decayMultiplier)`
where the multiplier comes from `PowerManager.GetEngineStabilityDecayMultiplier()`.

**Hyperspeed.** Engages on LeftShift or gamepad B when engine efficiency exceeds the threshold; drains
one engine bar per `1/hyperspeedDrainRate` seconds via an accumulator; auto-disengages at zero efficiency
with a haptic cue. Dodge and weapons are locked out while active. Continuous low rumble while engaged.

**Dodge.** `TryDodge(±1)` checks `ShipStability.CanPerformDodge()`, applies the cost, and sets a target
position `transform.right × direction × dodgeDistance` away. `FixedUpdate` interpolates to it with
`SmoothStep` over `dodgeDuration` using `rb.MovePosition`. Facing is unchanged.

**Firing.** `TryFireWeapons()` reads `PowerManager.GetSystemEfficiency("weapons")` — which resolves to
the `arms` system — and requires > 0.2. If `TargetingSystem` has a locked target it fires at that
Transform, otherwise at `transform.position + transform.up * 1000`. `F2` bypasses the power check
entirely and fires at full power.

**Railgun standby.** `EnterRailgunStandby(duration)` starts a coroutine that sets `IsOnStandby`, zeroes
speed and target speed, waits, then calls `PowerManager.RebootReactor()` and clears the flag. `Update()`
returns immediately while on standby; `FixedUpdate` lets the ship coast on existing momentum.

**Damage.** `TakeDamage(float, Vector3)` resolves the quadrant via `HullSystem.DetermineHitSide` and
delegates, then triggers haptics. `TakeDamage(float)` always hits Prow. The explicit `IDamageable`
implementation delegates to the Prow overload and returns the full amount.
`GetCurrentHealth`/`GetMaxHealth` proxy the hull totals; `CanBeDamaged` is simply "hull exists".
`HandleShipDestroyed` destroys the GameObject.

**Camera.** Not a child of the ship. Each `FixedUpdate` the ship sets the camera position to
`transform.position + cameraOffset`, computes a roll offset that leans into the turn
(`−normalisedTurn × maxCameraRollOffset`, lerped at `cameraRollSpeed`), and applies ship rotation plus
roll to the camera's Z. It then pushes the ship's **true** Z rotation — not the rolled angle — to
`UIController.UpdateCompass`, `WorldGridRotUpdate`, and passes the **camera position** to
`WorldGridLocUpdate`.

**Stability floor.** While `ShipStability.IsStabilityDepleted()`, the ship takes
`5 × fixedDeltaTime` damage per frame to the side determined from `−transform.up`, i.e. the rear.

**Observations.** The `Drift Settings` block (`driftCompensationRate` 8, `maxLateralDrift` 40,
`driftStabilityDrainRate` 0.4) is declared but unused — the movement model is forward-locked.
`PowerManager.IsSupercruiseUnlocked()` exists and is never called; hyperspeed uses the 0.8 efficiency
threshold instead. Weapon cycling is bound only to gamepad Y, with no keyboard equivalent.

---

## 7. Power

### PowerManager.cs

`[DefaultExecutionOrder(-10)]`, guaranteeing its `Start` runs before `UIController.Start`.

`PowerState`: `Standby`, `Draw`, `Vent`.
`PowerChannel` (serializable; called `PowerSystem` until 2026-10-26): `name`, `currentState`, `maxPower` (5), `currentPower` (0),
`readyToVent`, `finishingCurrentBar`.

Five systems: **engines, arms, bay, support, sig**.

| Setting | Default |
|---|---|
| `powerFillRate` | 0.5 bars/s, divided across drawing systems |
| `ventRate` | 1 bar/s |
| `reactorMaxPower` | 15 |
| `currentReactorPower` | 15 (float) |

**The reactor is a closed loop.** Drawing a bar costs **0.5** reactor units; venting one returns 0.5.
So a full reactor funds 30 system bars against a theoretical 25 of capacity. There is **no passive
regeneration** — power returns only by venting. (`DOCUMENTATION.md` describes a `reactorRegenRate`;
no such field exists.)

`Start()` puts every system in Standby at 0 power and fills the reactor.

**`UpdatePowerSystems()` each frame:**
1. Clamp `currentReactorPower` to `reactorMaxPower × InternalSubsystems.GetReactorMultiplier()`.
2. Partition systems: `Draw` and `finishingCurrentBar` go into the active list; `Vent` is processed immediately.
3. `baseRate = powerFillRate / drawingCount`. A system merely finishing a partial bar uses the full
   solo rate instead.
4. For each active system, if the reactor is online and holds ≥ 0.5 and the system isn't full,
   accumulate `rate × deltaTime`; on reaching 1.0, add a bar, deduct 0.5 from the reactor, clear the
   finishing flag, and if the system just hit max, drop it to Standby.

**State cycle.** `ToggleSystemState` implements the fixed spec loop
`Standby → Draw → Standby → Vent → Standby`. Leaving Draw sets `readyToVent` and clears the partial bar.
Pressing again from Standby vents — unless the system is at max power, in which case it just clears
`readyToVent` so the next press draws. Vent ignores input entirely: it cannot be cancelled.

`ToggleDrawState` is the simpler Draw↔Standby flip used by the UI buttons and the gamepad D-pad;
it never enters Vent.

`VentPower` drains one bar per second, returning 0.5 to the reactor each time, and returns the system
to a clean Standby when it hits zero.

**Bonus getters**, all computed from `BonusBars = max(0, currentPower − 1)`:

| Method | Effect |
|---|---|
| `GetEngineAccelerationMultiplier` | +10% per bonus bar |
| `GetEngineStabilityDecayMultiplier` | −5% per bonus bar |
| `IsSupercruiseUnlocked` | engines at max |
| `GetCannonLoadTimeMultiplier` | −2.5% per bonus bar (arms) |
| `GetBoardingPodRangeMultiplier` | +10% per bonus bar (**arms**, not bay) |
| `GetAbilityCooldownMultiplier` | −5% per bonus bar (support) |
| `GetScannerPerfectHitBonus` | +1% per bonus bar (sig) |
| `GetCommsInterceptTimeBonus` | +1 s per bonus bar (sig) |

**Special operations.**
- `DrainAllPowerInstantly()` — zeroes every system, returns the power to the reactor, clamps, and sets
  `reactorOnline = false`. Used by the Railgun on fire.
- `RebootReactor()` — sets `reactorOnline = true` and nothing else; all systems stay in Standby and
  must be restarted by hand.
- `VentAllSystems()` / `EmergencyVent()` — puts every powered system into Vent (gradual).
- `EmergencyVentSystemsOnly()` — instant zero of systems, power returned, reactor stays **online**.
- `BlackAlert()` — vents everything, then next frame forces engines and arms into Draw.
- `DrainArmsPower(bars)`, `DrainSystemPower(name)`, `AddPower`/`RemovePower` plus per-system wrappers.

**Name resolution.** `GetSystemByName` accepts `"engines"`, `"arms"`/`"weapons"`, `"bay"`,
`"support"`, `"sig"`/`"sensors"`. Button aliases `ToggleWeapons`→arms, `ToggleSensors`→sig,
`ToggleCrew`→support, `ToggleShields`→**bay** exist to match the UI panel's ENG/WEP/CRW/SHD/SEN labels.

`GetSystemEfficiency(name)` returns `currentPower / maxPower` as a 0–1 float. `GetReactorPower()`
floors the float; `GetMaxReactorPower()` applies the subsystem damage multiplier.

### InternalSubsystems.cs

Seven internals, each a `Subsystem` with `maxHealth` (100), `currentHealth`, and derived
`isDamaged` (health < max) and `isDestroyed` (health ≤ 0).

| Subsystem | Hull side | Effect when destroyed |
|---|---|---|
| Bridge | Prow | turn-rate multiplier ×0.2 |
| Sensors | Prow | sensor range ×0, scanner disabled |
| Reactor | Aft | reactor capacity ×0.2, drains a random system every 5 s while damaged |
| Engines | Aft | speed ×0.3, turn rate ×0.3 |
| Magazine | Starboard | weapon multiplier ×0 |
| Life Support | Port | starts a 120 s game-over countdown |
| Crew | Port | abilities disabled |

`Start()` subscribes to `HullSystem.OnHullDamaged`. `HandleHullDamage(side, damage, remainingHealth)`
**returns immediately unless `remainingHealth <= 0`** — internals are only reachable once that side's
armour is gone. It then picks uniformly at random among that side's subsystems, applies the full damage
amount, fires `OnSubsystemDamaged(type, health)`, and on a fresh kill fires `OnSubsystemDestroyed(type)`
and applies the destroyed effect.

Damaged-but-alive Reactor accumulates a timer and calls `DrainRandomSystem()` every
`reactorDrainInterval` (5 s), which drains one bar from a uniformly random one of the five power systems.

**Debuff queries** other systems call: `GetSpeedMultiplier` (destroyed 0.3, else lerp 0.5→1 by health),
`GetTurnRateMultiplier` (bridge and engines multiply together), `GetReactorMultiplier`
(destroyed 0.2, else lerp 0.3→1), `GetSensorRangeMultiplier`, `GetWeaponMultiplier`,
`AreAbilitiesDisabled()`, `IsScannerDisabled()`, `GetLifeSupportTimeRemaining()`.

`RepairSubsystem(type, amount)` heals and, if Life Support comes back from destroyed, cancels the
countdown.

Events: `OnSubsystemDamaged`, `OnSubsystemDestroyed`, `OnLifeSupportCritical`, `OnLifeSupportFailed`.

**Observations.** `GetWeaponMultiplier` and `AreAbilitiesDisabled` have no callers in the project.
Life Support's countdown fires `OnLifeSupportFailed`, which only `QuestSystem` listens for.

---
## 8. Enemy and sensors

### EnemyShip.cs

Implements `IDamageable`. Self-contained AI with type presets, a three-state machine, and
force-based movement. Does **not** use `HullSystem` — it carries a single integer health.

`Awake` adds a Rigidbody if absent (no gravity, Z position and X/Y rotation frozen), finds the player
by the `"Player"` tag, applies the type preset, sets `currentHealth = maxHealth`, records
`patrolOrigin` at its spawn position, and picks a first waypoint.

**Type presets** (`ApplyTypePreset`, run in `Awake`, overwriting inspector values):

| Type | Health | Thrust | Max speed | Rotation | Fire rate | Detection | Combat dist | Patrol radius |
|---|---|---|---|---|---|---|---|---|
| Patrol (default) | 3 | 8 | 15 | 90 | 0.5 | 30 | 15 | 40 |
| StationaryDefender | 20 | 0 | 0 | 90 | 0.2 | 50 | 20 | patrol off |
| Tank | 50 | 4 | 6 | 45 | 1.2 | 30 | 10 | 20 |
| Interceptor | 2 | 18 | 28 | 150 | 0.35 | 40 | 8 | 60 |
| Sniper | 2 | 6 | 10 | 90 | 0.8 | 80 | 40 | 40 |

Other defaults: `waypointReachedThreshold` 3, `obstacleAvoidanceDistance` 8, `obstacleLayerMask` all,
`orbitForce` 5 (declared but not used in the combat maths).

**State machine** (`UpdateState`, called every `FixedUpdate` from distance to player):

| From | Condition | To |
|---|---|---|
| Patrol | distance ≤ `detectionRadius` | Chase |
| Chase | distance > `detectionRadius × 1.15` | Patrol (re-anchors `patrolOrigin` to current position) |
| Chase | distance ≤ `optimalCombatDistance` | Combat |
| Combat | distance > `detectionRadius × 1.15` | Patrol (re-anchors) |
| Combat | distance > `optimalCombatDistance × 1.2` | Chase |

With no player reference at all it patrols (if enabled) and does nothing else.

**Patrol** — random point in a circle of `patrolRadius` around `patrolOrigin`; steer with obstacle
avoidance; thrust at **half** force. New waypoint on arrival.
**Chase** — steer straight at the player with avoidance, full thrust.
**Combat** — closer than `optimalCombatDistance × 0.7` steers directly away; otherwise
`moveDir = normalize(dirToPlayer + tangent × 0.5)` where `tangent = cross(dirToPlayer, forward)`,
producing an inward spiral/orbit. Fires when `dot(transform.up, dirToPlayer) > 0.7` (≈ within 45°)
and the fire-rate timer has elapsed.

**Obstacle avoidance** — five rays at −60°, −30°, 0°, +30°, +60° around the desired heading, each
`obstacleAvoidanceDistance` long against `obstacleLayerMask`, ignoring triggers. Picks the ray with
the greatest clearance; returns the original heading when the centre ray wins.

**Movement helpers** — `RotateTowards` uses `Quaternion.LookRotation(Vector3.forward, direction)` and
`rb.MoveRotation` at `rotationSpeed × fixedDeltaTime`. `EnforceSpeedLimit` clamps the XY velocity
magnitude to `maxSpeed` every frame.

**Firing** — instantiates `laserPrefab` at `firePoint` (or its own position) rotated toward the player,
then calls `Laser.Fire(direction, true)` with `true` marking it an enemy projectile.

**Damage and death** — `TakeDamage(int = 1)` and the explicit `IDamageable.TakeDamage(float)` both
decrement and call `Explode()` at ≤ 0. `Explode()` builds a `ParticleSystem` entirely in code
(start size 2, speed 5, lifetime 1, max 100 particles, a single 50-particle burst, sphere shape radius
0.1), schedules its destruction after the particle lifetime, and destroys the ship.

A comment records that `OnTriggerEnter` was deliberately removed from this class because it caused
double damage and race conditions with `Destroy`; collision handling lives in the projectile scripts.

Gizmos draw detection (yellow), combat (red), and patrol (cyan) radii plus the active waypoint (green).

### Radar.cs

Singleton. `rangeSteps` = {500, 1000, 2000, 4000, 8000}, cycled by `KeyCode.R`. Finds the player by tag.

`UpdateBlips()` runs every frame. For each registered `RadarTarget` it asks
`UIController.CreateOrGetRadarBlip(target)`, skipping the target when that returns null. It converts
the target's world position into player-local space via `InverseTransformPoint`, takes the XY as a
2D offset, computes `scale = magnitude / currentRange`, hides the blip when `scale > 1`, and otherwise
places it at `direction.normalized × scale × UIController.RadarBlipRadius`. Targets flagged
`trackRotation` get the blip rotated by `target.eulerAngles.z − player.eulerAngles.z`.
Null targets are collected and unregistered at the end of the pass.

Static `RegisterTarget` / `UnregisterTarget`; the latter also calls `UIController.DestroyRadarBlip`.

**Observation.** `Radar` never calls `UIController.RadarRange(float)`, so the range-ring shader value
is not updated when the range step changes.

### RadarTarget.cs

Three fields — `icon` (Sprite), `color` (white), `trackRotation` (false). Calls
`Radar.RegisterTarget(this)` in `OnEnable` and `Radar.UnregisterTarget(this)` in `OnDisable`.
Present on `EnemyShip1.prefab` and `EnemyShip2.prefab`.

### CommsManager.cs

The signal-interception minigame. `maxSignalRange` 200000 (declared, unused),
`signalLockTolerance` 1.0. Audio fields for a continuous tone plus success and failure clips.

`Signal` (serializable): `frequency`, `bandType` (1–3), `message`, `isEncrypted`, `timeLimit` (30 s).

`Start()` hides the comms panel and caches the `PlayerShip`.

`ReceiveSignal(signal)` refuses if already intercepting or if the ship is in hyperspeed. Otherwise it
stores the signal, shows the panel, sets **`Time.timeScale = 0`** — the whole game freezes — and starts
the interception timer coroutine.

`Update()` while intercepting: `1`/`2`/`3` select the band; held Left/Right arrows adjust frequency by
±0.1 per frame, clamped to 1.0–99.9; then `UpdateSignalStrength()` runs.

`UpdateSignalStrength()` — wrong band gives a grey indicator and stops the tone. Right band computes
`strength = clamp01(1 − |currentFreq − targetFreq| / 10)`, pushes
`Color.Lerp(red, green, strength)` to the indicator, and sets the tone pitch to `0.5 + strength`.

`TryLockSignal()` succeeds on correct band **and** frequency within `signalLockTolerance`, playing the
success clip, appending the message to `interceptedMessages`, and pushing it to the UI log. Otherwise
it plays the failure clip. Either way the panel closes and `Time.timeScale` returns to 1.

The timer coroutine counts down on `Time.unscaledDeltaTime` because the game is paused, and closes the
panel as a failure on expiry.

`GenerateTestSignal()` builds a random signal with a hard-coded RAJA message.
`FrequancyTune(speed)` forwards to `UIController.FrequancyTune`.

**Observations.** `TryLockSignal` is private with no caller inside the file — it is intended to be
registered through `UIController.SetupCommsListeners`, which nothing calls. Band keys `1`/`2`/`3`
are the same keys `PlayerShip` uses for power toggles. `CommsManager` appears only in `TestScene.unity`.
The GDD's Decrypt (cipher wheels) and Broadcast (disguise) stages have no implementation anywhere.

### ControllerHaptics.cs

Singleton wrapper over `UnityEngine.InputSystem` gamepad rumble. Self-bootstraps via
`[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`, creating a
`[ControllerHaptics]` GameObject and marking it `DontDestroyOnLoad`. Every method is a no-op when no
gamepad is present. `OnDestroy` zeroes the motors.

`Pulse(lowFreq, highFreq, duration)` starts a coroutine that sets motor speeds, waits, then zeroes.
Pulses stack rather than queue.

Static one-liners, each a preset `(low, high, duration)`: `LaserFired` (0, .25, .06),
`PDCFired` (.05, .38, .04), `CannonFired` (.55, .25, .18), `MacrocannonFired` (.75, .30, .22),
`MissileFired` (.45, .15, .16), `BroadsideFired` (.45, .20, .14), `BoardingPodFired` (.30, .10, .20),
`DodgeExecuted` (.60, .80, .10), `HyperspeedOn` (.35, .55, .30), `HyperspeedOff` (.20, .10, .10),
`HyperspeedLost` (.70, .40, .20), `TookDamage` (.85, .60, .15), `PowerToggled` (0, .20, .05),
`WeaponSwitched` (0, .15, .05). Plus `SetContinuous(low, high)` and `StopAll()` for sustained effects
where the caller owns stopping.

Called from `PlayerShip` (damage, power toggles, dodge, hyperspeed, weapon switch, hyperspeed rumble),
`WeaponManager` (per weapon type on fire), `LaserWeapon`, and `BroadsideCannon`. Not documented in
either `DOCUMENTATION.md` or `STAKEHOLDER_DOCUMENTATION.md`.

### Targeting/TargetingSystem.cs

Raycasts from the **screen centre** through `cam` (defaults to `Camera.main`) up to `maxDistance`
(1000) against `targetLayers` (all), ignoring triggers, looking for a `Targetable` via
`GetComponentInParent`. On a hit it activates a world-space reticle instance and positions it at the
target's `targetPoint` (or its transform). `lockKey` is `T`: pressing it locks the current target and
calls `WeaponManager.LoadActiveWeapon(targetTransform)`, pressing it again on the same target unlocks
and calls `LoadActiveWeapon(null)`. `GetLockedTargetTransform()` is what `PlayerShip` reads when firing.

Falls back to `weaponManager.targetingReticlePrefab` when no reticle prefab is assigned. Uses the
obsolete `FindObjectOfType` in `Start` (once, not per frame).

**Observations.** Not present on any prefab or in any scene. Screen-centre raycasting in a top-down
game with an overhead camera points at the player's own position.

---

## 9. Weapons framework

### IDamageable.cs

```csharp
float TakeDamage(float amount);   // returns damage actually applied
float GetCurrentHealth();
float GetMaxHealth();
bool  CanBeDamaged();
```
Implemented by `PlayerShip`, `EnemyShip`, and `Asteroid`.

### Weapons/WeaponBase.cs

Abstract MonoBehaviour base. Serialized: `range` 500, `angleOfFire` 45, `reloadTime` 1,
`maxAmmo` 30, `baseDamage` 10, `damageModifier` 1. Protected: `fireRate` 0.2, `currentAmmo`,
`isReloading`, `lastFireTime`.

`Start()` (virtual) sets `currentAmmo = maxAmmo`.
`CanFire()` (virtual) = not reloading AND ammo > 0 AND `Time.time − lastFireTime >= reloadTime`.
`GetReloadProgress()` returns `clamp01((Time.time − lastFireTime) / reloadTime)`, 1 when reloadTime ≤ 0.
`Reload()` (virtual) refills ammo. `Fire(Vector3 target)` is abstract.

Note `reloadTime` doubles as the between-shots gate; `fireRate` is declared but the base never uses it.

### Weapons/WeaponManager.cs

Owns `List<WeaponSlot>` and exposes one firing interface. `requiresPower` true,
`minPowerForWeapons` 0.1. Also holds `targetingReticlePrefab`, which `TargetingSystem` reads.

`Start()` assigns slot indices and default names, then selects `activeWeaponIndex`.
`Update()` calls `RefreshWeaponDisplay()` every `DisplayRefreshInterval` (0.1 s).

`SwitchToWeapon(index)` rejects out-of-range, null, or inactive slots and logs a warning; on success it
sets the active slot and calls the virtual `OnWeaponSwitched`, which logs and refreshes the display.
`SwitchToNextWeapon` / `SwitchToPreviousWeapon` walk the list until a slot accepts, stopping if they
return to the start. `SwitchToWeaponType(type)` picks the first active slot of that type.

`FireActiveWeapon(Vector3 target, float powerEfficiency = 1)` returns false when there is no weapon,
when `requiresPower` and efficiency is below `minPowerForWeapons`, or when `CanFire()` is false.
Otherwise it calls `Fire(target)` and triggers a per-type haptic pulse (PDC, Macrocannon, Missile,
BoardingPod, Broadside; Laser and Railgun handle their own). A `Transform` overload forwards the position.

`AddWeapon(type, instance, slotName = "")`, `RemoveWeapon(index)` (re-indexes and switches to slot 0 if
the active index falls off the end), `SetWeaponActive(index, active)` (switches away if the active slot
is deactivated), `GetWeaponsByType`, `GetActiveWeapon`, `GetActiveWeaponSlot`, `GetAllWeaponSlots`,
`CanActiveWeaponFire`, `GetActiveWeaponIndex`, `GetWeaponCount`,
`GetActiveWeaponType()` (defaults to `Laser` when nothing is selected).

`RefreshWeaponDisplay()` assembles name, type string, icon from `UIController.GetWeaponIcon`,
ammo as `current/max` (or `---` when max is 0), reload progress, and a status of
**Offline** (red, slot inactive or instance null), **Ready** (green, `CanFire()`), or
**Reloading** (yellow), then pushes all of it to `UIController.UpdateWeaponDisplay`.

`LoadActiveWeapon(Transform target = null)` dispatches by concrete type: `MissileLauncher` →
`LockAllTubes(target)`; `Macrocannon` → `ArmWeapon()`; `BoardingPodLauncher` → `SetTarget(target)`.
Missing targets log a warning. Laser, Railgun, and PointDefense are explicit no-ops.

### Weapons/Railgun.cs

Extends `WeaponBase` but **opts out of the manager's firing path**: `CanFire()` returns `false`
unconditionally and `Fire(Vector3)` has an empty body, so `WeaponManager.FireActiveWeapon` can never
fire it. It drives its own input in `Update()`, and only while `WeaponManager.GetActiveWeapon() == this`.

| Setting | Default |
|---|---|
| `maxRange` | 2000 (also assigned into the base `range` in `Start`) |
| `chargeTime` | 2 s — comment says "visual reference for charge-progress UI" |
| `maxPenetrations` | 10 |
| `damageDropoffPerPenetration` | 0.15 |
| `standbyDuration` | 4 s |
| `beamDuration` / `beamWidth` / `beamColor` | 0.3 s / 0.5 / cyan |

`Start()` resolves `PowerManager`, `ShipStability`, `PlayerShip`, and `WeaponManager` via
`GetComponentInParent`, creates an `AudioSource` if clips are assigned, and builds a `LineRenderer`
child named `RailgunBeam` when none is assigned (Sprites/Default material, tapered width, world space,
two positions), leaving it disabled.

**Input** — `Space` down starts charging, `Space` up fires. On gamepad, LT+RT together start the
charge and releasing RT while LT is still held fires.

**`TryStartCharging()`** requires `PowerManager.GetSystemEfficiency("arms") > 0.9`. On success it sets
the charging flag and start time, calls **`VentAllSystems()`** so the ship bleeds power for the whole
hold, enables the charge effect, plays the charge sound, and starts an escalating rumble coroutine
(low motor 0.05→0.55, high motor 0.1→0.4 across `chargeTime`).

**`FireRailgun()`** clears charging, stamps `lastFireTime`, stops the charge rumble and starts a fire
rumble (full power 0.15 s, then 0.4/0.2 for 0.25 s), plays the fire sound, calls
**`DrainAllPowerInstantly()`**, then casts a sequential penetrating raycast from `firePoint` along
`playerShip.transform.up`. Each iteration applies damage, spawns an impact effect, multiplies the
running damage by `(1 − 0.15)`, breaks if `ShouldStopBeam` (tags `HeavyArmor` or `Station`), otherwise
steps the origin 0.1 past the hit and reduces the remaining range. It then draws the beam to the last
hit (or full range), starts `PostFireStandby()`, and calls `UIController.RailFire()`.

**`ApplyDamage` priority**: `EnemyShip` → `TakeDamage(9999)` and return (instant kill, ignoring the
computed falloff damage entirely); then `PlayerShip` (guarded by a tag comparison); then `Shields`;
then any `IDamageable` that `CanBeDamaged()`; finally, objects tagged `Asteroid` or `Debris` are
destroyed outright.

**`PostFireStandby()`** sets its own standby flag, calls `PlayerShip.EnterRailgunStandby(standbyDuration)`,
waits, clears the flag, and calls `PowerManager.RebootReactor()`.

`DrawBeam` fades alpha and narrows both widths across `beamDuration`, then disables the renderer.
`CancelCharge()`, `FireInstant()`, and accessors `IsCharging`, `IsOnStandby`, `GetChargeProgress`,
`GetChargeTime`, `GetMaxPenetrations`, `GetMaxRange`, and `new` hides of `GetCurrentAmmo`/`GetMaxAmmo`.

**Observations.** `chargeTime` gates only the progress bar; releasing fires immediately regardless of
how long the charge was held. The 9999 instant kill means the penetration falloff never affects ships.
`RAILGUN_SETUP.md` documents a 100-damage falloff model and a `CanFire()`/`Fire()` standalone path,
neither of which matches this code.

---
## 10. Hull, shields, stability, autopilot, environment

### HullSystem.cs

Three types in one file: `HullSide` enum, `HullQuadrant` serializable class, `HullSystem` MonoBehaviour.

`HullSide` ordinals are load-bearing (used as array indices): `Port` 0, `Starboard` 1, `Prow` 2, `Aft` 3.

`HullQuadrant` holds `side`, `maxHealth`, `currentHealth`, and a computed `isBreached` property
(`currentHealth <= 0`) — a live property, not a latch, so a repaired quadrant un-breaches.
`TakeDamage(float)` subtracts, clamps at zero, and **returns the overflow** (the amount that would
have gone past zero). `Repair(float)` adds up to max. `GetHealthPercentage()` is current/max.

Serialized quadrant maxima: **Port 50, Starboard 50, Prow 40, Aft 10** — 150 total. The Aft is
deliberately a quarter of the Prow.

`Awake()` constructs the four quadrants at full health. There is no `Update` of any kind — the class
is purely reactive.

**`TakeDamage(HullSide, float)` order of operations:**
1. Resolve the quadrant; return if null.
2. Snapshot `wasBreached`.
3. Apply damage, capturing the overflow return.
4. Fire `OnHullDamaged(side, damage, quadrant.currentHealth)` — unconditionally, including when the
   quadrant was already at zero and absorbed nothing. The third argument is **remaining health**, not
   damage absorbed. This is the signal `InternalSubsystems` keys on.
5. On a fresh breach, set `breachNotified[(int)side]` and fire `OnHullBreached(side)`.
6. An `if (wasBreached && overflow > 0f)` block (lines 121–125) whose body is **entirely comments** —
   the computed overflow is never forwarded anywhere. Overflow damage to internals is left for
   `OnHullDamaged` subscribers to derive from the remaining-health argument.
7. Re-evaluate all four breached → fire `OnShipDestroyed`.

`DetermineHitSide(Vector3)` normalises the direction, converts to local space, then compares
`|dot(localDir, up)|` against `|dot(localDir, right)|`. The dominant axis wins: positive forward →
Prow, negative → Aft; positive right → Starboard, negative → Port. Exact 45° diagonals fall to the
left/right branch.

Other members: `GetQuadrant`, `IsBreached`, `GetTotalIntegrityPercentage` (denominator taken from the
serialized fields, not the quadrants), `GetTotalHealth`, `GetTotalMaxHealth`.

**Observations.** `breachNotified` is written and never read — dead state; first-breach detection uses
the local `wasBreached` snapshot instead. `OnShipDestroyed` is not one-shot and re-fires on every
subsequent hit once all four are breached. The total-health getters dereference the quadrants with no
null guard, so calling them before `Awake` throws. Nothing in the file guards against negative damage,
which would heal past max.

### Shields.cs

A regenerating pre-hull absorber that also owns its own sprite presentation. No public fields;
exactly one public method, `TakeDamage(float)`.

| Setting | Default |
|---|---|
| `maxShieldHealth` | 100 |
| `rechargeRate` | 10 HP/s |
| `rechargeDelay` | 3 s after last damage |
| `criticalShieldThreshold` | **30 absolute HP**, not a fraction |
| `blinkRate` | 0.5 s per toggle |
| `FLASH_DURATION` | const 0.5 s |
| `fullShieldColor` / `deplatedShieldColor` | cyan / red (field name is misspelled in source) |

`Awake` caches `PlayerShip` via `GetComponent`, resolves `shieldRenderer` via `GetComponentInChildren`
if unassigned (which searches self first, so on a ship whose own sprite is a `SpriteRenderer` it binds
the ship's renderer), fills health to max, and hides the renderer. `Start()` is empty apart from a comment.

**`TakeDamage`** stamps `lastDamageTime`, cancels any recharge, and if the shield has health:
absorbs the **entire** hit with `Mathf.Max(0, health - damage)` — there is **no overflow to the hull**,
so a 500-damage hit on a 10 HP shield deals nothing further that frame. It restarts the flash coroutine
when the post-hit health is still above 30, updates visuals, and either deactivates at zero or starts
the blink coroutine when at or below 30. When the shield is already at zero it forwards the full damage
to `playerShip.TakeDamage(damage)` — a direct concrete-type call, **not** through `IDamageable`, and
**not** through `HullSystem`'s directional API.

`UpdateShieldVisuals` lerps colour red→cyan by health fraction and overrides alpha from 0.2 (empty) to
0.8 (full).

**Observation on recharge.** `Update` only calls `StartRecharge()` when `!isShieldActive`, and that flag
is cleared solely by `DeactivateShield()` at exactly zero health. A partially depleted shield therefore
**never regenerates** — only a fully collapsed one does.

**Other observations.** `shieldRenderer` is null-guarded in `Awake`, `UpdateShieldVisuals`, and
`DeactivateShield`, but dereferenced unguarded in `StartRecharge`, `RechargeShield`, `FlashShield`, and
`BlinkShield`. `playerShip` is dereferenced unguarded on the depleted path. `criticalShieldThreshold`
being absolute means a max below 30 would leave the shield permanently critical.

### ShipStability.cs

The manoeuvre resource. No public fields.

| Setting | Default |
|---|---|
| `maxStability` | 100 |
| `stabilityRecoveryRate` | 5 /s |
| `turnStabilityDrainMultiplier` | 1 |
| `speedStabilityDrainMultiplier` | 0.5 |
| `dodgeStabilityCost` | 45 |
| `greenZoneThreshold` / `yellowZoneThreshold` | 0.6 / 1.1 degrees **per physics frame** |
| `DODGE_COOLDOWN` | const 0.5 s |

`Start` fills to max, calls `UIController.StabilityMeterStart(cur, max)` and `SetAlertActive(false)`.

**`Update`** only runs while below max: recovery is 5/s, dropping to **1.5/s** (×0.3) while
`IsStabilityCritical()` (≤ 10% of max). It pushes `StabilityMeterUpdate` every frame. The alert clears
only at **exactly full** stability, so it stays lit through the entire recovery — roughly 6.7 s through
the critical band plus 18 s above it, about 25 s from zero.

**`CalculateTurnStabilityDrain(turnAngleThisFrame, currentSpeed, powerDecayMultiplier = 1f)`**
both computes and applies the drain, and also returns it. Zones:

| Turn this frame | Multiplier | Zone |
|---|---|---|
| ≤ 0.6° | 0.1 | Green |
| ≤ 1.1° | 0.5 | Yellow |
| > 1.1° | 2.0 | Red |

```csharp
normalizedSpeed = clamp01(currentSpeed / 100f);
speedMultiplier = 1f + normalizedSpeed * 0.5f;            // 1.0 at rest, 1.5 at speed >= 100
drain = turnStabilityDrainMultiplier * drainMultiplier * speedMultiplier
      * powerDecayMultiplier * Time.fixedDeltaTime;
```

At 50 Hz and full speed a sustained Red turn drains 3.0/s; Green at rest drains 0.1/s, far below the
5/s recovery, so gentle turns never make net progress. Drains above 0.01 also fire `ShowAlert()` and a
per-frame `Debug.Log` of zone, angle, speed, drain and stability.

`ApplyDodge()` re-checks `CanPerformDodge()`, subtracts 45, clears the cooldown flag, and schedules
`ResetDodge` with `Invoke` after 0.5 s. `CanPerformDodge()` requires both the cooldown flag and ≥ 45
stability; `CanDodgeAgain()` returns only the cooldown flag.

**Observations.** `lastDodgeTime` is written and never read — the cooldown is implemented with `Invoke`.
`Time.fixedDeltaTime` is baked into the drain formula, so the method is only correct when driven from
`FixedUpdate`. The zone doc comments name 60° and 45° arcs, which contradict each other and the actual
degrees-per-frame units. Null handling mixes `if (… != null)` and `?.` in the same method.

### Autopilot.cs

Two modes, neither currently reachable — nothing in the project calls `EngageNavigation` or `EngageCombat`.

`AutopilotOrientation`: `Port`, `Starboard`.
`engagementRanges` = `{0, 5000, 10000, 18000, 25000}`; `desiredRange` defaults to 10000.

`Awake` resolves `PlayerShip`, `Rigidbody`, `PowerManager`, `InternalSubsystems` from the same GameObject.

**`FixedUpdate`** returns immediately when inactive; then checks `HasManualInput()` **before any
steering** and disengages on the same frame; then dispatches to combat (if `isCombatMode` and the
target is alive) or navigation.

**Navigation** — arrival radius is **1 world unit**, at which point it simply disengages; velocity is
untouched, so the ship coasts through at full speed. It rotates and thrusts in the same frame despite a
comment stating the ship must turn before burning.

**Combat** — the desired heading is the bearing to the target rotated ±90° so a broadside faces it.
Range control uses a hard-coded ±500 dead zone. Too far → thrust, but **only while
`dot(transform.up, dirToTarget) > 0`**, which conflicts with deliberately holding the nose 90° off the
bearing, so closing thrust is intermittent. Too close → the branch body is **entirely comments**; the
autopilot can close range but never open it.

**`RotateToward`** converts the desired direction to a Z angle (`atan2 … − 90°`), takes the shortest
signed delta, scales `playerShip.turnRate` by `PowerManager.GetSystemEfficiency("engines")` and
`InternalSubsystems.GetTurnRateMultiplier()`, clamps to the per-frame budget, and applies via
`rb.MoveRotation`. It does **not** call `ShipStability`, so autopilot turns cost no stability.

**`ApplyThrust`** assigns `rb.linearVelocity = transform.up * (100f * enginePower * subsysMult)`
directly — instant full speed with no acceleration curve, overwriting existing momentum. The base
speed 100 is hard-coded here rather than read from `PlayerShip`.

**`HasManualInput`** returns true on W/A/S/D held or `GetMouseButtonDown(0)`. The mouse check is an
edge event polled from `FixedUpdate` and can be missed or double-counted.

**Observations.** `EngageNavigation` does not clear `combatTarget` and `EngageCombat` does not clear
`hasNavigationTarget`, so a combat target that dies can silently revert to an old navigation coordinate.
`EngageCombat` accepts any float range, bypassing the bands, and logs `target.name` unguarded.
`GetEngagementRanges()` returns the serialized array by reference.

### Asteroid.cs

`[RequireComponent(typeof(Collider))]` (3D). Implements `IDamageable`. Doc comment describes it as an
example destructible obstacle.

| Setting | Default |
|---|---|
| `maxHealth` | 50 |
| `minDebris` / `maxDebris` | 2 / 5 |
| `debrisForce` | 5 |
| `damageThreshold` | 0.5 (show damage at 50% health) |

`Start` fills health, caches the `Renderer`, captures `originalMaterial`, and **force-assigns the
`"Asteroid"` tag** if not already set — the tag the Railgun's penetration check keys on. This requires
the tag to exist in the tag manager or it throws.

`TakeDamage(float)` returns `Mathf.Min(damage, currentHealth)` — the damage **actually consumed**,
which is the value a penetrating weapon would use to decide how much of its budget was spent. It logs
every hit, swaps to `damagedMaterial` below the threshold, and destroys at zero.

`DestroyAsteroid()` instantiates the destruction effect (with **no lifetime** — cleanup is the prefab's
responsibility), then spawns 2–5 debris pieces at random offsets within a 0.5-unit sphere with random
rotations, applying both `AddForce` and `AddTorque` impulses of `Random.insideUnitSphere * 5`, and
destroys each piece after **10 s**. Then destroys itself.

**Observations.** `originalMaterial` is captured and never used; reading `.material` in `Start`
instantiates a per-object material clone for every asteroid whether or not it is ever hit.
`ShowDamage()` is called on every hit below the threshold rather than once on crossing it.
There is no death event of any kind. `OnDrawGizmosSelected` draws a health-scaled wire sphere that is
zero-radius in edit mode.

### Cross-cutting note on physics

`Autopilot`, `Asteroid`, `Laser`, `Shell`, `Railgun`, `EnemyShip`, and `PlayerShip` all use **3D**
physics types — `Rigidbody`, `Collider`, `ForceMode`, `Physics.Raycast`, `Physics.OverlapSphere`,
`OnCollisionEnter`/`OnTriggerEnter` — while all heading maths is 2D on the Z axis with `transform.up`
as the bow and Z position frozen. This is a project-wide convention, not a per-file inconsistency.

---
## 11. Weapon implementations

### WeaponType.cs

Declarative only. `WeaponType` enum in declaration order — `Laser` 0, `Macrocannon` 1, `Missile` 2,
`PointDefense` 3, `BoardingPod` 4, `Railgun` 5, `Broadside` 6. Ordinals are load-bearing: inserting a
member mid-list would silently remap every serialized slot.

`WeaponSlot` is a `[System.Serializable]` plain class (serialized only through `WeaponManager`):
`weaponType`, `weaponInstance` (`WeaponBase`), `isActive` (true), `slotIndex`, `slotName`. Nothing
enforces that a slot tagged `Laser` actually holds a `LaserWeapon`.

### LaserWeapon.cs

The primary weapon. Applies "zero means unset" defaults **before** `base.Start()` so the base sees the
corrected magazine: `maxAmmo` 30, `range` 500, `angleOfFire` 45, `reloadTime` **3**, `baseDamage` 10 —
each only when the inspector value is exactly 0. `fireRate` is set to **1.0 unconditionally**,
overwriting any inspector value: one shot per second.

Two entry points with different gates:
- **`TryFire(efficiency)`** — the power path. Rejects on no ammo, `Time.time - lastFireTime < fireRate`,
  recharging, `efficiency < minPowerToFire` (0.2), or a null prefab. Fires along **`firePoint.up`**,
  ignoring any aim target.
- **`Fire(target)`** — the `WeaponManager` path. Gated on `CanFire()` only; aims at the target.
  **No power check on this path.**

`FireLaser` stamps the fire time, decrements ammo, calls `ControllerHaptics.LaserFired()`, and spawns
the prefab with rotation `LookRotation(forward, dir) * Euler(90,0,0)` — the 90° twist orients a
Z-forward mesh into the top-down plane. If the instance has a `Laser` it calls `Laser.Fire(dir, false)`;
otherwise a legacy fallback adds a Rigidbody, applies a 50-unit impulse, tags it `"PlayerProjectile"`,
and destroys it after 5 s.

**Recharge** is a separate state machine in `Update`, independent of the base `Reload()`: at zero ammo
it starts a 3 s timer and then refills the **entire** magazine. No partial reload, no manual trigger.

**Observations.** `baseDamage` and `damageModifier` are inert — `FireLaser` never writes damage onto
the spawned projectile, so actual damage is the prefab's `Laser.damage` (20), not the weapon's 10.
`laserSpeed` (50) is likewise unused whenever the prefab carries a `Laser` (speed comes from
`Laser.speed`, 30). `GetCurrentAmmo`/`GetMaxAmmo` are declared with neither `override` nor `new`, so
they **hide** rather than override the base members — a call through a `WeaponBase` reference resolves
to the base implementation. `isReloading` is checked in `CanFire()` but never set by this class.

### Laser.cs

The projectile. Public fields (not `[SerializeField] private`): `speed` 30, `lifeTime` 3, `damage` 20.

`Awake` caches the Rigidbody, adding one if absent with `useGravity = false`, continuous collision, and
non-kinematic — **but those settings are applied only on the add path**, so a prefab that already
carries a Rigidbody keeps whatever it was authored with.

`Fire(direction, isEnemyProjectile)` sets `rb.linearVelocity = direction * speed` and tags the object
`"EnemyProjectile"` or `"PlayerProjectile"`. The direction is not normalised inside `Fire`.
`Start` schedules an unconditional `Destroy` after `lifeTime` — 3 s, giving ~90 units of reach at
default speed and ~300 at the broadside's 100 u/s.

`OnCollisionEnter` (physical collision, not trigger) resolves by tag:

| Projectile tag | Order | Call | Lookup |
|---|---|---|---|
| `EnemyProjectile` | 1 | `Shields.TakeDamage(damage)` | `GetComponent` — same collider only |
| `EnemyProjectile` | 2 (only if no Shields) | `PlayerShip.TakeDamage(damage, velocity.normalized)` | `GetComponent` |
| `PlayerProjectile` | — | `EnemyShip.TakeDamage(RoundToInt(damage))` | **`GetComponentInParent`** |

Shields strictly pre-empt the hull. Friendly fire is prevented purely by the tag branch — there is no
owner reference and no layer filtering.

Self-destruct condition (line 78) is `shouldDamage || !other.CompareTag("PlayerProjectile") &&
!other.CompareTag("EnemyProjectile")`; `&&` binds tighter, so it reads "destroy if damage landed, or if
the thing hit is not itself a projectile". Laser-on-laser collisions therefore pass through.

**Observations.** Does **not** use `IDamageable` — it depends on the three concrete types directly, and
the `EnemyShip` call takes an `int`. Lookup depth is asymmetric: `GetComponent` for the player,
`GetComponentInParent` for enemies, so a `PlayerShip` with colliders on children would not be found.
`damage` is a per-prefab constant no weapon ever writes to. Four `Debug.Log` calls fire per hit.

### Macrocannon.cs

Multi-barrel, manually armed, **damage increases with range**.

| Setting | Default |
|---|---|
| `numBarrels` | 3 |
| `shellVelocity` | 1000 |
| `maxRange` | 12500 (assigned into `range` in `Start`) |
| `damagePerUnit` | 0.01 |

**Arming** — `ArmWeapon()` starts a `LoadShell` coroutine that sets `isArmed` immediately, waits **1 s**,
then increments `loadedShells` and decrements `currentAmmo`. Ammo is spent at load time, not fire time.

**Locking** — `AttemptLock(target)` starts a 1 s `LockSequence`; on completion, if the target still
exists and is within `maxRange`, `hasLock = true`.

**Firing** — gated on `base.CanFire() && loadedShells > 0 && isArmed`. Damage is computed from the
distance to the *target argument*:

```csharp
damageFromDistance = distance * damagePerUnit;          // distance * 0.01
totalDamage = Mathf.Max(baseDamage, damageFromDistance) * damageModifier;
```

So `max(10, distance × 0.01)` — the distance term only beats the floor past 1000 units, reaching **125**
at max range. The shell spawns at `barrels[numBarrels - loadedShells]` (barrels consumed front to back)
and is launched along **`transform.forward`**.

**Observations.** The lock subsystem is entirely decoupled from firing — neither `CanFire()` nor `Fire()`
reads `hasLock`, so locking currently affects nothing but the `HasValidLock()` readout. `Fire` sends the
shell along `transform.forward` (world +Z in this project's convention), using the `target` argument
**only** to compute damage magnitude. Because ammo is decremented at load and `base.CanFire()` requires
`currentAmmo > 0`, the final shell loaded from the last unit of ammo can never be fired.
`ArmWeapon` guards against the *current* `loadedShells`, so rapid repeated calls can stack coroutines
past `numBarrels` and drive `barrels[numBarrels - loadedShells]` negative. `EnterViewfinder()` is an
empty stub. `barrels` and `shellPrefab` are dereferenced unguarded.

### Shell.cs

The heavy kinetic projectile, used by `Macrocannon` and by `BroadsideCannon`'s non-`Laser` branch.
**No serialized fields at all** — nothing is inspector-visible or externally readable.

`Initialize(initialVelocity, damageAmount)` stores a pre-multiplied velocity vector, the damage, and the
creation time. `Update` integrates `transform.position += velocity * Time.deltaTime` — no Rigidbody, no
interpolation. `maxLifetime` is a private non-serialized **15 s**.

`OnTriggerEnter` does `GetComponent<IDamageable>()` on the collider, applies damage if found, and then
destroys itself **unconditionally** — including on triggers that are not targets at all (detection
volumes, spawn regions).

**Observations.** This is the only projectile in the project that routes through `IDamageable`, and the
only one with **no friendly-fire prevention whatsoever** — no tag, no owner, no layer filter. It damages
the firing ship as readily as an enemy. At 1000 u/s it advances ~16.7 units per 60 Hz frame with no
sweep, so tunnelling past ordinary colliders is expected. The `IDamageable` lookup is `GetComponent`,
not `GetComponentInParent`.

### BroadsideCannon.cs

A dual-side auto-targeting battery. Each side independently acquires, locks, and fires.

| Setting | Default |
|---|---|
| `shellSpeed` | 100 |
| `lockTime` | 2.0 s |
| hold-window grace | 0.2 s (magic literal, line 39) |

`Fire(targetPos)` **only** stamps `lastFireInputTime`; the argument is unused. `Update` treats the
weapon as live while `Time.time - lastFireInputTime <= 0.2`, so a single `Fire` call keeps it running
for 200 ms. On release, `ClearLocks()` wipes both sides.

**Target selection** — `Physics.OverlapSphere(transform.position, range)` per side per frame, **no layer
mask**, allocating a fresh array each call. Filters: must have an `EnemyShip` in its parents; must be
within range; must satisfy `|dot(transform.up, toEnemy.normalized)| <= 0.707` (a ±45° wedge centred on
the beam); side is chosen by the sign of `dot(transform.right, toEnemy)`. Among survivors it picks the
one **nearest the perpendicular centreline**, not the nearest by distance.

**Lock and fire** — a changed target resets that side's progress. Progress accumulates to `lockTime`,
then the side latches. Firing requires locked, ammo, and `Time.time - sideLastFireTime >= reloadTime`
(base 1 s), then fires along `±transform.right`, decrements the **shared** ammo pool, triggers haptics,
and resets that side's lock so it must re-paint. Effective cadence is `max(reloadTime, lockTime)` = 2 s.

The spawned prefab is handled two ways: with a `Laser` component it sets `laser.speed = shellSpeed` and
calls `Laser.Fire(dir, false)` → 20 damage from the prefab; otherwise it sets Rigidbody velocity and/or
calls `Shell.Initialize(dir * shellSpeed, baseDamage * damageModifier)` → 10 damage.

**Observations.** Shells always travel exactly `±transform.right` regardless of where the locked target
is inside the wedge — the lock gates *whether* the side fires, not *where* it aims, and nothing leads
the target. `portTarget`/`starboardTarget` are used only for lock bookkeeping and the UI getters, never
for aim. Damage differs by an order of magnitude depending on which prefab shape is used. If a prefab
has both a `Rigidbody` and a `Shell`, velocity is applied twice — physics plus per-frame transform
integration — giving 200 u/s. `GetPortLockProgress`/`GetStarboardLockProgress` divide by `lockTime`
without the zero guard that `GetLockProgress` has. `using System.Collections.Generic` and `System.Linq`
are both present and unused.

### MissileLauncher.cs

Multi-tube guided launcher with per-tube load/lock state.

| Setting | Default |
|---|---|
| `lockOnTime` | 2 s |
| `minLockRange` / `maxLockRange` | 500 / 25000 (max also assigned into `range`) |
| zero-defaults applied in `Start` | `maxAmmo` 5, `angleOfFire` 360, `reloadTime` 5, `baseDamage` **150** |

`Start` allocates four parallel arrays sized to `launchTubes.Length` and **pre-loads every tube**,
decrementing `currentAmmo` per tube — so `currentAmmo` represents *reserve* rounds, not available ones.

`AttemptLock(target, tube)` requires the tube loaded and unlocked and the distance inside the window,
then runs `LockSequence`, which accumulates `Time.deltaTime` to `lockOnTime` while re-validating the
target each frame (**max range only** — `minLockRange` is not re-checked).

`CanFire()` = `base.CanFire() && any tube loaded` — **a lock is not required to pass**. `Fire(target)`
ignores its argument and empties every tube that is loaded **and** locked **and** has a live target, so
it is a salvo weapon. Each missile gets `Initialize(target, baseDamage * damageModifier)` = 150.
`ReloadTube` waits `reloadTime` then reloads from reserve; once reserve is exhausted the tube stays
empty permanently.

**Observations.** `CanFire` reports true on loaded-but-unlocked tubes, so `WeaponManager` can
"successfully" fire and produce nothing. Repeated `LockAllTubes` calls on a still-locking tube start
**additional concurrent coroutines** on that tube, each adding to the same progress accumulator — N
stacked coroutines lock roughly N× faster. `AttemptLock` does not null-check the target or bounds-check
the tube index. `GetLockProgress` divides by `lockOnTime` with no guard.

### Missile.cs

| Setting | Default |
|---|---|
| `thrust` | 1000 (ForceMode.Force, every physics step, uncapped) |
| `turnRate` | 180 °/s |
| `armedDistance` | 100 |
| `proximityFuseRadius` | 50 |
| `maxLifetime` | 30 s |
| `explosionRadius` | 100 |

`FixedUpdate` order: lifetime expiry → **`Detonate()` in place** (damaging whatever is nearby, not a
quiet despawn); target null → silent destroy; arming check; steering via `RotateTowards` written to
`rb.rotation`; `AddForce(transform.forward * thrust)`; proximity fuse. `OnCollisionEnter` detonates
**only if armed** — an unarmed impact does nothing and the missile continues.

`Detonate` runs `Physics.OverlapSphere(position, explosionRadius)` with **no layer mask**, and for each
`IDamageable` applies `damage * (1 - distance/explosionRadius)` — linear falloff to zero at 100 units.

**Observations.** The arming check measures distance **to the target**, not distance travelled. A
missile launched at a target beyond 100 units arms on its first physics step; one launched at a target
*closer* than 100 units **never arms**, so both its fuse and its collision detonation are permanently
disabled and it orbits until the 30 s lifetime detonation. Falloff is unclamped and uses the collider's
*pivot*, so a large ship whose pivot lies outside the radius receives **negative** damage. No
friendly-fire exclusion — the blast damages the firing ship within 100 units. `// TODO: Spawn explosion
effect` at line 85.

### PointDefenseCanon.cs

(Class name spelled with one "n".)

| Setting | Default |
|---|---|
| `rateOfFire` | 30 rounds/s → `fireRate` overwritten to 0.0333 |
| `spread` | ±2° independently on X and Y (a square cone) |
| `bulletVelocity` | 2000 |
| `maxTrackingSpeed` | 45 °/s |
| `spinUpTime` | 0.5 s — **private, not serialized** |

`SpinUp()` starts a 0.5 s coroutine that sets `isSpunUp`; `SpinDown()` clears it instantly.
`CanFire()` requires `isSpunUp`. `Update` slews `transform.rotation` toward `currentTarget` at 45 °/s
every frame regardless of spin-up or ammo — so if the component sits on the ship root, **it rotates the
ship**. `Fire(target)` ignores its argument and fires along `transform.forward`, cycling barrels
round-robin and decrementing ammo at fire time.

**Observations.** Nothing in the project calls `SpinUp`, `SpinDown`, or `TrackTarget` —
`WeaponManager.LoadActiveWeapon` has no dispatch case for the PDC. `isSpunUp` therefore stays false and
`CanFire()` is permanently false: **as wired, the PDC is inert**. Base `range` 500 is untouched while
bullets travel 6000 units. `gunBarrels` is dereferenced with no empty guard, making
`% gunBarrels.Length` a divide-by-zero. The comment at line 81 claims the tracking rotation is
unimplemented; it is implemented, in `Update`.

### PDCBullet.cs

No serialized fields. `Initialize(velocity, damage)` takes a pre-multiplied velocity.
`Update` (not `FixedUpdate`) integrates the transform directly; `maxLifetime` is a hard-coded 3 s,
giving 6000 units of reach at 2000 u/s. `OnTriggerEnter` applies damage to any `IDamageable` on the
collider and then destroys itself **unconditionally**.

**Observations.** ~33 units of travel per frame at 60 fps with no sweep — tunnelling is expected.
`GetComponent`, not `GetComponentInParent`. No friendly-fire exclusion. Destroys itself on any trigger.

### BoardingPod.cs / BoardingPodLauncher.cs

**Launcher** — envelope `[minLaunchRange 200, maxLaunchRange 5000]`, `podVelocity` 500,
`range` set to 5000. It applies **no zero-defaults**, so it inherits `reloadTime` 1 s, `maxAmmo` 30,
`baseDamage` 10. One pod is chambered at `Start` (ammo decremented at load, as with missiles).
`SetTarget` validates the distance **once, at call time**, and never re-checks. `CanFire()` requires
loaded and a valid target. `Fire` ignores its argument and launches along the **launcher's**
`transform.forward` while spawning at `launchTube.rotation` — the two can disagree.

**Pod** — `turnRate` 45 °/s, `maxLifetime` 20 s, `attachmentForce` 1000, `boardingTime` 5 s.
`FixedUpdate` checks lifetime **first**, then attached, then target-lost, then steers and re-asserts
`rb.linearVelocity = transform.forward * velocity.magnitude` (constant speed from the launch vector).
`OnCollisionEnter` compares `collision.gameObject.transform == target` by **reference identity**; on a
match it applies a 1000-impulse to the struck ship at the contact point, reparents to the target, and
goes kinematic. Any other collision destroys the pod. `HandleAttached` counts to `boardingTime` and
then applies `target.GetComponent<IDamageable>().TakeDamage(damage)`.

**Observations.** The 20 s lifetime is checked before the attached branch and is never extended, so a
pod that attaches after t≈15 s is destroyed before its 5 s boarding completes and deals **no damage**.
Flight at 500 u/s over the 5000-unit maximum takes ~10 s, leaving a narrow margin. The reference-identity
collision test means a ship whose collider sits on a child (turret, hull section, hitbox) fails the
match and the pod self-destructs on the very ship it was aimed at. `currentTarget`/`hasValidTarget` are
**not cleared after firing**, so the next pod (1 s later) launches at the same stored target regardless
of whether it is still valid. `// TODO: Trigger boarding party effects/animations` at line 81.

### WeaponUIDisplay.cs

A polling adapter between `WeaponManager` and `UIController`. Owns no widgets. `updateInterval` 0.1 s;
the timer resets to exactly 0 rather than subtracting the interval, so the cadence drifts with frame
time rather than holding a true 10 Hz.

Baseline per tick: status `"READY"`/`"RELOADING"` (green/yellow) from `CanActiveWeaponFire()`, ammo
`"ARMED"`, fill 1, fill colour white. With no slot or instance it pushes
`("NO WEAPON", "---", null, "OFFLINE", red, "---", 0, white)`.

| Weapon | ammo | status | fill / colour |
|---|---|---|---|
| Broadside, locked | `cur/max` | `LOCKED BOTH` / `LOCKED PORT` / `LOCKED STBD` (green) | ammo ratio, cyan |
| Broadside, painting | `cur/max` | `PAINTING …` (yellow) | `GetLockProgress()`, yellow |
| Broadside, neither | `cur/max` | `SCANNING` (**red**) | ammo ratio, cyan |
| Macrocannon | `LOCKED` / `SCANNING` | baseline | 1, white |
| MissileLauncher | `MISSILES` | baseline | `GetLockProgress(0)`, **white** |
| Railgun, charging | `CHARGING nn%` | `CHARGING` (yellow) | charge progress, yellow |
| Railgun, idle | `cur/max` | `READY` / `COOLDOWN` | 1, green |
| everything else | `ARMED` | baseline | 1, white |

**Observations.** There is **no branch for `PointDefenseCanon`, `BoardingPodLauncher`, or
`LaserWeapon`** — all three display as a generic full white `"ARMED"` bar, even though `LaserWeapon`
exposes exactly the recharge accessors the `UpdateWeaponDisplay` signature is named for. The missile
branch hard-codes tube 0, which throws every 0.1 s if the launcher has no tubes. `ForceUpdate()` does
not null-check `weaponManager`, unlike `Update`. This is the one file that switches on concrete weapon
types, so adding a weapon requires editing it.

### WeaponSystemExample.cs

**Sample/demo code, not live gameplay.** Its own summary says "Example implementation showing how to use
the modular weapon system". It uses legacy `Input`/`KeyCode` with hard-coded bindings (Space fire,
E/Q cycle, 1–4 jump to Laser/Macrocannon/Missile/PointDefense), auto-adds a `WeaponManager` at runtime
with a warning if none is found, and formats a debug status string. It duplicates in cruder form what
`WeaponUIDisplay` does through `UIController`. Treat it as a reference harness for how `WeaponManager`
is meant to be driven. Note its Q/E/Space/1–4 bindings collide with real gameplay keys.

---
## 12. UIController.cs

2,052 lines, roughly 45% of all gameplay code, and the single point every system routes through.
Sits on `PlayerShip.prefab`, not on a canvas. `Awake` is a singleton guard that does
`Destroy(gameObject)` on a duplicate — which destroys the entire ship it is attached to.

`Start()` order: resolve `shipPlayer` (see below) → find `PowerManager` and cache the five
`PowerChannel` references in UI button order (engines, arms, bay, support, sig) → `StartPower()` →
`WorldGridStart()` → `RadarStart()` → `WorldGridZoom(0)` → start speedometer polling → `NewScan()` →
`ScanTargetLoc(0)` → `ScanTargetSize(0)` → `BtnBogieTab()` → `GlitchStart()` → find `WeaponManager` →
find `HullSystem` and populate the hull display once.

`Update()` runs, in order: the test-key block (see the input map in §4), `ScanMovment()`,
`RadarTest()`, `UpdateVelocity()`, a 0.1 s-throttled `PollHullDisplay()`, and
`SyncPowerBarsFromManager()`.

### Subsystems

**Power display.** Five bars plus a reactor bar, each an `Image` whose material is a per-instance clone
of `shaPowerMet` / `shaReactorMet`, wrapped in a `UIPowerClass`. `SyncPowerBarsFromManager()` runs every
frame and unconditionally pushes `currentPower`/`maxPower` into `_PowerCur`/`_PowerMax`, mirrors the
charging state (`Draw` or `finishingCurrentBar`), swaps the switch sprite, and starts or stops the
charge animation coroutine. `ChargeBtn(i)` now only calls `PowerManager.ToggleDrawState` — the comment
states that `SyncPowerBarsFromManager` owns all visuals. `ChargeOn`/`ChargeOff` are superseded by that
inline logic and have no callers.

**Power node arrows.** `PowerNodes(i, state)` drives a second shader with nine states (0 off, 1–3
left/charge-up/end-of-chain, 4–6 right/charge-down/end, 7–8 reactor left/right).
`ChargeNodeCheck()` and `VentNodeCheck()` walk the bar list to decide which node is the end of the
chain. `ChargeNodeCheck` calls `PowerNodes(5, 7)` unconditionally at the top and turns the reactor node
off only when **no** system is drawing — there is **no reactor-full test anywhere**, which is the
specific symptom the UI author reported.

**Scanner minigame.** `NewScan()` clears state, clones the scanner shader, sets `_NibMax` to a
hard-coded 30-slot track, fills `scanPips` with the empty marker 5, then for each of the four hull
sides picks a random size 1–4 and searches (up to 50 attempts) for a contiguous free run, writing the
side index into `scanPips` and the `(start, end)` pair into `_PortV2`/`_AftV2`/`_ProwV2`/`_StarV2`.
`ScanMovment()` sweeps `scannerSlider.value` back and forth by `±1/scanSpeed` per frame.
`ScanPlaceHack()` reads the slider and, if it falls inside a pip range, sets that side's shader colour
to red. `shipPartsString` is `{ "_Port", "_Aft", "_Prow", "_Star" }`; `pipCol` is amber / red / green.

**Bogie system.** `bogieList` is a `List<BogieClass>` and `currentBogieTarget` a `GameObject`, both
`[SerializeField] public`. `AddBogie` appends, then attempts to create the weapon-screen image for the
entry. `RemoveBogie` destroys the image and removes the entry. `ScanNewTarget()` cycles to the next
entry, updates the target location and scale for the current zoom, then calls `ScanMesh` and `NewScan`.
`ScanMesh(go)` copies `go.GetComponent<MeshFilter>().mesh` into `bogieMesh`.
`ScanTargetLoc(i)` handles only `i == 0` (parent to the player) and `i == 1` (parent to the bogie).
`ScanTargetSize(i)` maps 0/1/2/3 to scale 0 / 1 / 4 / 50.

**Radar.** `RadarStart()` clones the radar shader and starts with the bogey and show flags off.
`RadarUpdate(d)` sets `_RangeDegree`; `RadarRange(r)` sets `_RadarRange`. `RadarTest()` sweeps a
degree counter 360→0 every frame and feeds `RadarUpdate` — the sweep animation is driven permanently
by a test method. `BogeySpot(d)` starts `BogeyStart`, which clones `radarGO`, parents it under
`radarBogeyGO`, clones the radar shader, sets `_RangeDegree`, `_LineSizeV2` and a red `_RadarColor`,
fades the colour to black over 3 s, and destroys it.
`CreateOrGetRadarBlip(target)` returns a cached blip or instantiates `radarBlipPrefab` under
`radarBlipContainer`, applying the target's icon and colour — it **null-guards both fields and returns
null** when either is unassigned. `DestroyRadarBlip` removes and destroys.
`RadarBlipRadius` is a read-only property over a serialized 100-unit default.

**Weapon screen.** `RailFire()` runs a three-phase coroutine that sets `_LaserDegree` to 90 (or the
ship's heading at max zoom), `_GunLoc`, `_LaserStart`/`_LaserEnd`, then widens the beam, calls
`updateShipHit(.5f)`, holds, and retracts. `LaserFire()` fires `LaserFireCo(currentBogieTarget, -1)`;
`LaserFireEnemy(i)` fires `LaserFireCo(bogieList[i].go, i)` — **no bounds check**. `LaserFireCo`
computes a screen-space angle and distance from ship to bogie, scales by the current zoom's
`screenDist`, and animates `_LaserFire`/`_LaserDegree`/`_LaserSize`/`_LaserStart`/`_LaserEnd` on either
`weaponScreenImage.material` (player, `s < 0`) or `bogieList[s].matWep` (enemy).
`StabilityMeter(float)` writes `_Stability` on the weapon-screen material and has no callers.

**World grid.** Three zoom levels driven by `WorldGridZoom(i)`: 10× (`sc[6]` orthographic size 50,
`screenDist` 50, range-visualiser on), 100× (size 500, `screenDist` 500), 1000× (`sc[5]` size 5000,
`screenDist` 5000, radar `_Show` off, world-view texture `viewTex[5]`). Each level also iterates
`bogieList` toggling each bogie's `MeshRenderer` and its first child's icon and scale, and toggles the
player's own mesh against `shipiconGO`. `WorldGridLocUpdate(shipV2)` divides the position by 100 at
zoom 0 and by 1000 at zoom 1, and re-parents the scan target when a bogie is selected.
`WorldGridRotUpdate(r)` sets `_ShipRotation` only below zoom 2.

**Compass and hull.** `UpdateCompass(float)` rotates `compassRect`. The six-argument overload writes
the four quadrant values as `"x/ 100"` strings and slides the sensor panel to the origin, or blanks
them and parks it at x=2000. `PollHullDisplay()` reads `HullSystem`'s four quadrant percentages ×100
and feeds that overload every 0.1 s.

**Speedometer, velocity, stability.** `StartSpeedometerUpdates` finds the `PlayerShip`, starts a
polling coroutine, and builds the two velocity-meter materials, seeding the player's max from
`turnRate * 2`. The poll reads the Rigidbody's velocity magnitude and negates it when travel opposes
`transform.forward`. `UpdateVelocity()` (every frame) pushes rounded speed into `_PowerCur` and the
text label. `StabilityMeterStart/Update/Color` drive a bar with a colour table of white ≥25%,
amber 10–25%, red <10%.

**Comms panel.** `ShowCommsPanel`, `SetCommsBand` (text plus green/grey indicator array),
`SetCommsFrequency` (slider and text), `SetCommsSignalStrength` (indicator tint), `AddCommsLog`
(overwrites, does not append), and `SetupCommsListeners` (configures the slider range 1.0–99.9 and
registers the frequency and lock callbacks). All six are null-guarded.
`FrequancyTune(speed)` accumulates an angle clamped to 0…1800, rotates `frequancyTrans`, writes the
numeric text, and lerps the tuner marker between two stops. It carries the author's trailing comment
`//not in game currently`, though `CommsManager` and 24 inspector bindings now call it.

**Weapon display.** `UpdateWeaponDisplay(...)` writes eight values into six null-guarded widgets.
`GetWeaponIcon(WeaponType)` is a switch expression over six sprites (no `Broadside` case → null).
`BtnWepTab()` advances a tab index, recolours the tab name and frame sprites, rotates the tab dial via
`BtnTunerTab`, and calls `WeaponManager.SwitchToWeapon(index)`. `BtnWeaponFire()` picks a target from
`currentBogieTarget` or a fallback, plays the laser or railgun animation for those two types, then calls
`FireActiveWeapon(targetPos, 1f)`. `BtnWeaponLoad()` forwards `currentBogieTarget` to
`WeaponManager.LoadActiveWeapon`.

**Screen effects.** `GlitchStart()` clones the static shader onto each `staticImg`, setting `_LineSize`
to 100 for index 0 and 50 for the rest, and logs each material. `GlitchEffect(t, o, s)` sets `_Glitch`,
ramps `_GlitchOpacity` down from `o`, holds for `t`, then clears. `updateShipHit(duration)` runs
`ShipShake`, which fires a glitch on every static image with randomised opacity and duration, then
offsets `shipHit.localPosition` by `Random.insideUnitSphere * shipHitCurve.Evaluate(t)` each frame.

**Ability stubs.** `BtnScannerCloke()`, `BtnRepair()`, and `SignalGhost()` have empty bodies containing
only a comment. Buttons call all three. The deleted `WireUIReferencesEditor` wired those buttons.

### Observations

- **`AddBogie` throws on every call.** Line 1253 is `if (bogieList[j].wepImageGo = null)` — a single
  `=`. It compiles because `UnityEngine.Object` defines an implicit `bool` operator. The effect is that
  the field is assigned null, the condition evaluates false so the `Instantiate` never runs, and line
  1258 immediately dereferences the null it just wrote. Nothing in the project calls `AddBogie`, so the
  bogie list is permanently empty.
- **`LaserFireEnemy(i)` indexes `bogieList` with no bounds check**, and `Update` binds it to keys 0–4.
  Keys 1/2/3 are also the power toggles, so a power toggle throws while the list is empty.
- **`RemoveBogie`** defaults `j` to 0 when the entry is not found, removing the wrong element.
- **`ScanNewTarget`** loops `j < bogieList.Count - 1`, so with exactly one bogie the body never runs and
  execution falls through to `ScanMesh(currentBogieTarget)` with a possibly-null argument.
  `ScanMesh` has no null guard on either the GameObject or the `MeshFilter`.
- **`WorldGridZoom`** iterates `bogieList` calling `GetComponent<MeshRenderer>().enabled` and
  `transform.GetChild(0)` unguarded — currently harmless only because the list is always empty.
- **`ScanTargetLoc`** is called with 2 and 3 from `WorldGridLocUpdate` but only implements 0 and 1.
- **`BtnWeaponFire`** falls back to `transform.forward` (+Z) rather than `transform.up`, aiming out of
  the 2D plane; `PlayerShip` uses `transform.up` correctly.
- **13 blocks of commented-out code**, all authored by the same account per `git blame`, across four
  commits between January and April 2026. The notable ones: `UpdateSpeedometer`'s entire body
  (line 1040); the `FindGameObjectWithTag("Player")` lookup replaced by `this.gameObject` (line 199,
  which makes `shipPlayer` the UIController's own transform); `sensorIcon.sprite` assignment in both
  `UpdateCompass` branches (1011, 1021); a retired `NewBogie()` test scaffold (1205–1236) labelled
  "test for now" and superseded by `AddBogie`; and the bogie-tab layout loop (1773–1788).
- `_RangeColor` is set through `SetVector` from a `Vector3`, which widens with **w = 0** — alpha zero
  if the shader reads it as RGBA.

## 13. Targeting, UI helpers, editor tooling, test scaffolding

### Targeting/Targetable.cs

A marker component. One public field, `targetPoint`, with a tooltip saying the object's transform is
used when it is null. Both `Reset()` (editor) and `Awake()` self-heal it to `this.transform`. No other
members. Discovery is entirely raycast-driven — it registers with nothing.

### Targeting/TargetingReticle.cs

The view half of targeting. `unlockedColor` white, `lockedColor` red, `targetRenderer` auto-resolved via
`GetComponentInChildren` in both `Reset` and `Awake`. `SetLocked(bool)` writes
`targetRenderer.material.color`; `SetPosition(worldPos, cam)` moves the reticle and calls `LookAt` with
the default world up, so it aligns with the camera's forward but does **not** follow camera roll — it is
not a full billboard. Reading `.material` (not `.sharedMaterial`) instantiates a per-renderer material
clone. Nothing sets the colour at start, so a fresh reticle keeps its authored material colour until the
first `SetLocked` call.

**Note:** `Assets/Prefabs/TargetReticle.prefab` contains only a Transform and a SpriteRenderer — the
`TargetingReticle` component is **not** on it, so `TargetingSystem.reticleScript` resolves to null and
the lock colour never changes.

### UI_Script/BogieClass.cs

A `[System.Serializable]` plain class — the per-contact record in `UIController.bogieList`. Fields:
`go` (the tracked GameObject), `mesh`, `wepImageGo` (weapon-screen overlay object), `matWep` (its
material). A four-argument constructor with no validation and the trailing comment
`//need more data like stats etc`. `WeapStart()` resets four shader properties: `_LaserFire` 0,
`_RangeVisOn` 0, `_StabilityBool` 0, and `_RangeColor` to `Vector3(1,0,0)` — which widens to
`(1,0,0,0)`, alpha zero. `matWep` is dereferenced with no null check. Because the class declares an
explicit constructor it has **no parameterless constructor**. `using UnityEngine.UI` is unused.

### UI_Script/UIPowerClass.cs

Declared as a `MonoBehaviour` but written as a data class. Fields: `mat`, `max`, `cur`, `pwr`, `charge`,
`pwrShift`. `UpdateMat()` calls `Charge(false)` then pushes `_PowerCur`, `_PowerMax`, and a hard-coded
`_ChargeVelocity` of 5. `Charge(bool)` writes `_Charge` as 0 or 1.

**Observations.** It derives from `MonoBehaviour` yet its only meaningful initialisation is a
parameterised constructor — which Unity forbids calling with `new`. `UIController` does exactly that
(`new UIPowerClass(...)`), which logs Unity's "You are trying to create a MonoBehaviour using the 'new'
keyword" warning and yields an object attached to no GameObject; it still functions as a data holder
because nothing in the class uses Unity lifecycle callbacks. Line 25 is `this.pwrShift = pwrShift;` —
a self-assignment; there is no such parameter. `else if (chg)` is a tautology. `pwr` is never read.
`UpdateMat()` silently clears the charge visual as its first act. Five `using` directives are unused,
two of which collide with UnityEngine types, which is why the file carries `using Debug = UnityEngine.Debug`.

### TestUIKeyControler.cs

**Test scaffolding**, living at the root of `Assets/` (class name misspells "Controller"). It applies
no forces and moves nothing; it only reads state and pushes it into `UIController`.

It calls `FindFirstObjectByType<PlayerShip>()` **every frame**, unconditionally — the anti-pattern
CLAUDE.md names explicitly.

| Key | Detection | Call | Collides with |
|---|---|---|---|
| `R` | `GetKey` — **every frame held** | `RailFire()` | Radar range cycle |
| `W` | `GetKey` | `UpdateSpeedometer(velocity.magnitude)` | thrust forward |
| `S` | `GetKey` | `UpdateSpeedometer(signed magnitude)` | thrust reverse |
| `W`/`S` up | `GetKeyUp` | `UpdateSpeedometer(signed magnitude)` | |
| `B` | `GetKeyDown` | `updateShipHit(1f)` **and** `PlayerShip.TakeDamage(10f)` | — |
| `Q`, `E`, `1`, `2`, `3` | `GetKeyDown` | *empty bodies* | dodge, power toggles |
| `0` | `GetKeyDown` | `UpdateCompass(false, 3, 0,0,0,0)` | — |
| `4` | `GetKeyDown` | `UpdateCompass(true, 0, 25,25,25,25)` — asteroid | — |
| `5` | `GetKeyDown` | `UpdateCompass(true, 1, 50,50,50,50)` — orbiter | — |
| `6` | `GetKeyDown` | `UpdateCompass(true, 2, 100,100,100,100)` — enemy ship | — |

**Observations.** The camera-rotation block calls `UpdateSpeedometer` with a **yaw angle in degrees**,
not a compass method, despite its comment — and it runs after the W/S speed push in the same frame, so
while the camera turns the yaw value overwrites the speed. `playerShipRef` and `playerShipTrans` are
public inspector fields that nothing reads. The former A/D handlers and the `gridMat` shader writes are
commented out, making `rotation` and `movmentSpeed` dead fields. There is no `#if UNITY_EDITOR` guard,
so this ships in the runtime assembly and runs in a build if left on a GameObject.

### Editor/WireUIReferencesEditor.cs — deleted

Deleted on 2026-10-27 (owner), with its two menus, `Starwolf/Wire UI References` and
`Starwolf/Wire Button OnClicks`. The first menu set `Radar.playerTransform` and
`WorldBoundary.playerTransform`. The second repaired and added the old HUD's button calls. The calls
and references it set are saved in the scenes and prefabs. No runtime code sets them. Any new ones are
set by hand in the inspector.

### Editor/PowerManagerEditor.cs

`[CustomEditor(typeof(PowerManager))]`. Subscribes `ForceRepaint` to `EditorApplication.update` in
`OnEnable` and unsubscribes in `OnDisable`; the handler calls `Repaint()` only while
`Application.isPlaying`, so the five `PowerChannel` blocks stay up to date in the inspector during play.
`OnInspectorGUI` is overridden to call `DrawDefaultInspector()` — functionally identical to not
overriding it at all, so the repaint subscription is the class's only real effect. No
`[CanEditMultipleObjects]`.

### Editor/ReadOnlyDrawer.cs

`[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]`. `OnGUI` sets `GUI.enabled = false`, draws the
property with `includeChildren: true`, then restores `GUI.enabled = true`. `GetPropertyHeight` mirrors
the same `includeChildren` flag so multi-line properties are not clipped.

**Observation.** It restores `GUI.enabled` to a hard-coded `true` rather than the previous value, so a
`[ReadOnly]` field drawn inside an already-disabled scope re-enables the GUI for everything drawn after
it in that scope.

---
## 14. Cross-cutting patterns

These recur across many files and are worth knowing before touching anything.

**Physics is 3D throughout.** `Rigidbody`, `Collider`, `ForceMode`, `Physics.Raycast`,
`Physics.OverlapSphere`, `OnCollisionEnter(Collision)`, `OnTriggerEnter(Collider)` — never the 2D
equivalents. All heading maths is 2D on the Z axis with `transform.up` as the bow and Z position frozen
on the rigidbodies. This is a project-wide convention, not a per-file inconsistency.

**Three parallel damage paths coexist.**
1. `IDamageable` — used by `Shell`, `PDCBullet`, `Missile`, `BoardingPod`, and `Railgun`'s fallback.
2. Concrete type checks — `Laser` tests `Shields`, then `PlayerShip`, then `EnemyShip` directly, and
   calls `EnemyShip.TakeDamage(int)`, a different signature from the interface.
3. Direct call — `Shields` forwards to `PlayerShip.TakeDamage(float)` when depleted, bypassing both the
   interface and `HullSystem`'s directional API.

**Component lookup depth is inconsistent.** `Laser` uses `GetComponentInParent` for enemies but
`GetComponent` for the player. `Shell`, `PDCBullet`, `Missile`, and `BoardingPod` all use `GetComponent`
on the struck collider, so a ship whose script sits on the root while its colliders sit on children will
not be found by them.

**Friendly fire is handled unevenly.** `Laser` prevents it with tags. `Shell`, `PDCBullet`, and
`Missile`'s blast have no owner concept at all and will damage the firing ship. `BoardingPod` uses
reference identity against its target.

**`WeaponBase` members are widely unused.** `angleOfFire` is set by every weapon and checked by none.
`Reload()` is never called anywhere. `isReloading` is read but never written. `fireRate` is declared in
the base but only some subclasses use it. `MissileLauncher` and `BoardingPodLauncher` run their own
`WaitForSeconds(reloadTime)` coroutines without setting the base flag, so inherited
`GetReloadProgress()` does not reflect their reloads.

**Two ammo conventions.** `MissileLauncher` and `BoardingPodLauncher` decrement `currentAmmo` at *load*
time, so `GetCurrentAmmo()` under-reports by the number of chambered rounds. Everything else decrements
at fire time.

**`Fire(Vector3 target)` ignores its argument** in `MissileLauncher`, `PointDefenseCanon`,
`BoardingPodLauncher`, and `BroadsideCannon`. Aim comes from a separately-set `Transform`, from
`transform.forward`, or from a fixed beam direction. `Macrocannon` uses the argument only to compute
damage magnitude, not direction. `Railgun` ignores it entirely (`Fire` is empty).

**Dead or written-never-read state**, confirmed by reading: `HullSystem.breachNotified`;
`HullSystem`'s empty overflow branch; `ShipStability.lastDodgeTime`; `Asteroid.originalMaterial`;
`Shields.Start()`; `Autopilot`'s empty "too close" branch; `UIPowerClass.pwr` and its `pwrShift`
self-assignment; `PlayerShip`'s entire `Drift Settings` block; `TestUIKeyControler.playerShipRef` and
`playerShipTrans`; `EnemyShip.orbitForce`.

**Components present in code but on no prefab or scene:** `Macrocannon`, `MissileLauncher`,
`PointDefenseCanon`, `WeaponUIDisplay`, `Autopilot`, `TargetingSystem`, `Targetable`,
`TargetingReticle`, `Asteroid`, `WeaponSystemExample`. `CommsManager` appears only in `TestScene.unity`.

**Entry points with no callers anywhere in the project:** `QuestSystem.StartMission`,
`GameClock.TogglePause`, `Autopilot.EngageNavigation`/`EngageCombat`, `PointDefenseCanon.SpinUp`/
`SpinDown`/`TrackTarget`, `UIController.AddBogie`/`RemoveBogie`/`BogeySpot`/`LaserFire`/
`LaserFireEnemy`/`RadarRange`/`SetupCommsListeners`/`StabilityMeter`/`BtnWeaponFire`/`BtnWeaponLoad`/
`BtnWepTab`/`EmergencyVentBtn`/`ForceSpeedometerUpdate`/`SetAutoUpdateSpeedometer`,
`InternalSubsystems.GetWeaponMultiplier`/`AreAbilitiesDisabled`, `PowerManager.IsSupercruiseUnlocked`,
`WeaponUIDisplay.SetWeaponManager`/`ForceUpdate`, `WeaponSystemExample.GetWeaponStatus`,
`MissileLauncher.ClearLock`, `BoardingPodLauncher.ClearTarget`, `Macrocannon.EnterViewfinder`/
`AttemptLock`/`ClearLock`/`HasValidLock`.

**Test code runs in the shipping build.** `UIController.Update()` carries the UI author's test key
bindings, and `UIController` is on the player ship prefab. `TestUIKeyControler` has no editor guard.
`RadarTest()` drives the radar sweep permanently.

---

## 15. Documentation inventory

This section is as of 2026-09-01. There are many more documents now, and the newest source wins. For
the current list and which source wins, see `Assets/CONTEXT_INDEX.md`. The four markdown files below
(`DOCUMENTATION.md`, `STAKEHOLDER_DOCUMENTATION.md` and the two weapon READMEs) were deleted on
2026-10-27 (owner). Git history has them.

On 2026-09-01 there were eight documents. Authority ran GDD → Art Spec → UI doc → the markdown files.

| Document | Size | What it is |
|---|---|---|
| `References/Starwolf GDD.pdf` | 74 pp | The design bible. Concept, scope, power, detection, movement, sensors, scanner, comms (Receive/Decrypt/Broadcast), weapons, combat and damage types, stats, autopilot, world map, clock, quests, art direction. The Enemy AI section is empty. |
| `References/Starwolf_ Art Spec.pdf` | 18 pp | Art direction, ship design language, obstacle taxonomy, ship class sizes (Frigate 1 → Dreadnought 25). |
| `References/UI Killshot documentation.pdf` | 4 pp | The UI author's own API contract for `UIController`. Names the functions gameplay is expected to call. |
| `References/starwolf-prototype.html` | 4,472 lines | **The playable build.** Made independently and ahead of the Unity implementation. |
| `Assets/DOCUMENTATION.md` | 1,753 lines | Per-class code reference. Broadly good; several stale claims listed below. |
| `Assets/STAKEHOLDER_DOCUMENTATION.md` | 340 lines | Behaviour-facing overview. Describes the code, not the GDD. |
| `Assets/Scripts/Weapons/WEAPON_SYSTEM_README.md` | 279 lines | Weapon architecture guide. Code examples do not compile. |
| `Assets/Scripts/Weapons/RAILGUN_SETUP.md` | 212 lines | Railgun setup guide. Substantially inaccurate. |

### Known documentation drift

Verified against the code, not inferred:

- `DOCUMENTATION.md` states `GameManager` initialises and wires `GameClock`, `QuestSystem`, and
  `WorldBoundary`, and handles Escape-to-pause. `GameManager` references none of the three and has no
  `Update`. `QuestSystem.Start()` does the wiring.
- `DOCUMENTATION.md` describes a `reactorRegenRate` and passive reactor regeneration. No such field
  exists; the reactor only refills by venting.
- `DOCUMENTATION.md` documents a `LaserWeapon.TryFire` special case inside `WeaponManager`. Not present.
- `DOCUMENTATION.md` attributes `updateShipHit` to the damage system. No damage code calls it; it is
  driven by `RailFireCo`, `LaserFireCo`, and `TestUIKeyControler`.
- `DOCUMENTATION.md` says a player laser shot calls `UIController.LaserFire()`. Nothing calls it.
- `DOCUMENTATION.md` says `GameManager` persists via `DontDestroyOnLoad`. It does not.
- `DOCUMENTATION.md` tells you to open `Starwolf.sln`. The solution is `Killshot.slnx`.
- `WEAPON_SYSTEM_README.md` places `WeaponType` in `WeaponManager.cs` (it is in `WeaponType.cs`) and
  lists five weapon types (there are seven). Its examples assign `laser.laserPrefab` and
  `laser.firePoint`, both of which are `[SerializeField] private`; use `rb.velocity`, renamed to
  `linearVelocity` in this Unity version; and call `shipPowerSystem.GetWeaponPowerEfficiency()`, which
  does not exist.
- `RAILGUN_SETUP.md` documents a 100-damage model with 15% falloff per penetration. The code deals a
  flat 9999 to any `EnemyShip`, ignoring the computed falloff entirely. Its "Method 2: Standalone"
  section instructs `if (railgun.CanFire()) railgun.Fire(target)`, but `CanFire()` returns a hardcoded
  false and `Fire()` is empty. Its controls table is wrong on every line: there is no Alpha5 binding,
  Q/E are the dodge keys, and weapon cycling is gamepad-only. Its `IDamageable` example does not match
  the real interface in any member.
- Neither `DOCUMENTATION.md` nor `STAKEHOLDER_DOCUMENTATION.md` mentions `ControllerHaptics`, the
  gamepad control scheme, `PowerManager.ToggleDrawState`, or the `Assets/Scripts/Targeting/` system.

### Where the code diverges from the GDD

Recorded as facts, not as defects — several are likely deliberate.

| Topic | GDD | Code |
|---|---|---|
| Shields | p47: ships have none; armour only | `Shields.cs` is on the player prefab |
| Laser | not one of the five weapons | `LaserWeapon` is the primary weapon |
| Cannon | p40: one weapon mounted both sides | `Macrocannon` and `BroadsideCannon` are separate classes |
| Power systems | four: Engines, Weapons, Sensors, Support | five — adds `bay`; the UI's fifth slot is `SHD`, marked "not in use" |
| Boarding pod range bonus | under Weapons | under `arms`, matching the GDD, though a `bay` system also exists |
| Reactor | "a capped wallet" | matches — closed loop, no regen |
| Supercruise | 3× at max engine power | 4× at 0.8 efficiency; `IsSupercruiseUnlocked()` unused |
| Sensors | two modes: Radar 25,000 and Sonar 5,000 | one mode; steps 500–8,000; no sonar anywhere |
| Damage model | Kinetic/Laser/Plasma/N-Ray with PEN vs TOUGH | flat float damage; no types, no penetration stats |
| Detection / Energy Signature | the core stealth mechanic | absent entirely |
| Comms | Receive, Decrypt (4 cipher wheels), Broadcast | Receive only |
| Scanner | rhythm minigame revealing hull, power, signature | built in `UIController`, no backend drives it |
| PDC | 4-quadrant threat-tagging minigame | spin-up and tracking turret, unwired |
| Missiles | require a 100% scan, target a ship system | lock-on time only, no system targeting |
| Stability at zero | causes internal damage | `PlayerShip` applies 5/s rear hull damage — matches |
| Boarding | room-by-room tactical map, Destroy/Capture, Retrieve | pod attaches, timer, one lump of damage |
| Front end | title, mission cards, hyperdrive sequence, credits | none |
| Enemy AI | section is empty in the GDD | Patrol/Chase/Combat with five archetypes |

---

## 16. How to use this document

- Trust it for **what Soham's scripts do**, as they were on 2026-09-01. It was produced by reading
  every line of every runtime and editor script listed in §1. The changes since then are listed at
  the top of this file. It does not cover the board's code.
- Do **not** trust it as a specification. The GDD is oriented at the Unity build; the HTML prototype is
  the playable game and runs ahead of both.
- Anything marked "Observation" is a factual note about behaviour, not a request to change it. Several
  are probably deliberate or unfinished-on-purpose.
- Before acting on any wiring claim, re-confirm against the live Editor — the serialized-state analysis
  in §3 was done by reading YAML with the Editor closed.

**Not covered by this document:** `References/starwolf-prototype.html`. It is read line by line in
`Assets/PROTOTYPE_BASELINE.md`.
