# Starwolf — Codebase Documentation

> **Scope:** All game scripts under `Assets/Scripts/`, the `UIController` (`Assets/UI/UI_Script/UIController.cs`), and supporting UI classes.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [UIController — The UI Bridge](#uicontroller--the-ui-bridge)
3. [Core Game Systems](#core-game-systems)
   - [GameManager](#gamemanager)
   - [GameClock](#gameclock)
   - [QuestSystem](#questsystem)
   - [WorldBoundary](#worldboundary)
4. [Player Ship](#player-ship)
   - [PlayerShip](#playership)
   - [ShipStability](#shipstability)
   - [PowerManager](#powermanager)
   - [HullSystem](#hullsystem)
   - [InternalSubsystems](#internalsubsystems)
   - [Shields](#shields)
   - [Autopilot](#autopilot)
5. [Enemy](#enemy)
   - [EnemyShip](#enemyship)
6. [Weapons System](#weapons-system)
   - [WeaponBase](#weaponbase)
   - [WeaponManager](#weaponmanager)
   - [WeaponUIDisplay](#weaponuidisplay)
   - [Laser / LaserWeapon](#laser--laserweapon)
   - [Missile / MissileLauncher](#missile--missilelauncher)
   - [Railgun](#railgun)
   - [BroadsideCannon](#broadsidecannon)
   - [Macrocannon](#macrocannon)
   - [PointDefenseCanon / PDCBullet](#pointdefensecanon--pdcbullet)
   - [BoardingPod / BoardingPodLauncher](#boardingpod--boardingpodlauncher)
   - [Shell](#shell)
7. [Radar System](#radar-system)
   - [Radar](#radar)
   - [RadarTarget](#radartarget)
8. [Communications System](#communications-system)
   - [CommsManager / Signal](#commsmanager--signal)
9. [Environment](#environment)
   - [Asteroid](#asteroid)
10. [Interfaces](#interfaces)
    - [IDamageable](#idamageable)
11. [UI References](#ui-references)
    - [BogieClass](#bogieclass)
    - [UIPowerClass](#uipowerclass)
    - [Shader Property Reference](#shader-property-reference)
    - [TestUIKeyController](#testuikeycontroller)

---

## Architecture Overview

Starwolf is a top-down 2D space combat game built in Unity. The game world is oriented on the XY plane — the ship moves forward along its local `transform.up` axis and the Z axis is frozen for all Rigidbodies.

**Key design rules present throughout the code:**
- Damage is always **directional** — it hits the hull quadrant facing the impact.
- **Power** is a shared reactor resource distributed among five systems: **Engines**, **Arms**, **Bay**, **Support**, and **Sig**. The reactor passively regenerates and is the single source of truth; the UI always mirrors inspector values.
- **Stability** is consumed by sharp turns and dodging; depleting it prevents further maneuvers.
- **Mission failure** can be triggered by player destruction, desertion, time expiry, or life support failure.
- **`UIController`** is a singleton accessed everywhere via `UIController.Instance`. Backend scripts call it to push data into the HUD — the UI does not poll the game state.

---

## UIController — The UI Bridge

**File:** `Assets/UI/UI_Script/UIController.cs`

`UIController` is the central singleton that owns every piece of on-screen UI. Backend game scripts call its methods to update the HUD; the UI itself does not poll game state (except for power bars and hull display, which are polled on timers).

**Singleton access pattern used throughout the codebase:**
```csharp
UIController.Instance?.ShowCommsPanel(true);
```
The `?.` null-conditional is used defensively — the UI may not exist in test scenes.

### Key Methods Used by Other Systems

| Method | Called by | Description |
|---|---|---|
| `StabilityMeterStart(cur, max)` | `ShipStability` | Initialises the stability bar shader with starting values. |
| `StabilityMeterUpdate(cur)` | `ShipStability` | Pushes the current stability value to the bar every frame. |
| `SetAlertActive(bool)` | `ShipStability` | Shows/hides the stability alert text. |
| `UpdateCompass(float angle)` | `PlayerShip` | Rotates the compass ring to match the ship's Z rotation. |
| `UpdateCompass(loaded, icon, port, aft, prow, starboard)` | Scanner system | Updates compass with hull quadrant data from a scanned target. |
| `WorldGridRotUpdate(float r)` | `PlayerShip` | Rotates the world grid overlay with the ship. |
| `WorldGridLocUpdate(Vector2 pos)` | `PlayerShip` | Moves the ship icon on the world grid. |
| `UpdateSpeedometer(float speed)` | `TestUIKeyController` / auto | Updates the speedometer text display. |
| `UpdateVelocity()` | Internal | Updates velocity meter shaders for player and bogie. |
| `updateShipHit(float duration)` | Damage system | Triggers a screen-shake coroutine driven by an `AnimationCurve`. |
| `ShowCommsPanel(bool)` | `CommsManager` | Shows or hides the signal intercept panel. |
| `SetCommsBand(int)` | `CommsManager` | Highlights the active frequency band indicator. |
| `SetCommsFrequency(float)` | `CommsManager` | Updates the frequency slider and text display. |
| `SetCommsSignalStrength(Color)` | `CommsManager` | Tints the signal strength indicator (grey/red/green). |
| `AddCommsLog(string)` | `CommsManager` | Writes an intercepted message to the comms log. |
| `SetupCommsListeners(onFreqChanged, onLock)` | `CommsManager` | Wires slider/button callbacks for comms panel. |
| `FrequancyTune(float speed)` | `CommsManager` | Rotates the analogue frequency dial and moves the tuner marker. |
| `CreateOrGetRadarBlip(target)` | `Radar` | Spawns or retrieves the `RectTransform` blip for a `RadarTarget`. |
| `DestroyRadarBlip(target)` | `Radar` | Destroys the blip and removes it from the internal dictionary. |
| `RadarBlipRadius` | `Radar` | Read-only property; the pixel radius of the radar circle. |
| `RadarRange(float)` | `Radar` | Sets the visual radar range via shader property. |
| `UpdateWeaponDisplay(...)` | `WeaponUIDisplay` | Updates the weapon name, type, icon, status, status colour, ammo, recharge bar, and recharge colour. |
| `GetWeaponIcon(WeaponType)` | `WeaponUIDisplay` | Returns the `Sprite` assigned for a given weapon type. |
| `BtnWeaponFire()` | UI button | Plays the weapon UI animation and fires the active weapon. |
| `BtnWeaponLoad()` | UI button | Primes the active weapon (e.g. missile lock, macrocannon arm). |
| `LaserFire()` | UIController (test, `p` key) | Plays the laser beam animation on the weapon screen toward the current target. |
| `LaserFireEnemy(int i)` | UIController (test, `0`–`4` keys) | Plays a laser fire animation originating from bogie `i`. |
| `RailFire()` | `Railgun` (on fire) / UIController (test, `r` key) | Plays the full railgun charge-and-fire beam animation on the weapon screen. |
| `WorldGridZoom(int i)` | UI buttons | Switches between the three zoom levels (0 = 10×, 1 = 100×, 2 = 1000×). |
| `BogeySpot(float degree)` | UIController radar test | Briefly lights up a bogey blip on the radar at the given angle. |
| `ScanNewTarget()` | UI button / `]` key | Cycles `currentBogieTarget` to the next entry in `bogieList`. |
| `ScanTargetSize(int)` | Scanner | Sets the target mesh display scale. |
| `ScanTargetLoc(int)` | Scanner | Sets the target's location indicator. |
| `ScanMesh(GameObject)` | Scanner | Loads the target's mesh into the 3D target display. |
| `AddBogie(BogieClass)` | Enemy detection | Adds a new enemy to the bogie list. |
| `RemoveBogie(BogieClass)` | Enemy destroyed | Removes a bogie from the list. |
| `PollHullDisplay()` | Internal (timed) | Polls `HullSystem` and updates hull quadrant shader values. |
| `GlitchStart()` | Various (damage, effects) | Triggers a static glitch effect on UI screens. |
| `GlitchEffect(t, o, s)` | Coroutine | Runs the glitch with time, opacity, and line size params. |
| `SyncPowerBarsFromManager()` | Internal (`Update()`) | Pushes power bar values from `PowerManager` to shaders every frame. |
| `ChargeBtn(int)` | UI button | Toggles a power system's state. |
| `VentBtn()` | UI button | Triggers the vent animation and vents all systems. |
| `BtnBogieTab()` | UI button | Toggles the bogie (enemy) tab panel. |
| `BtnScannerCloke()` | UI button | Toggles scanner cloak mode. |
| `BtnRepair()` | UI button | Triggers repair action. |
| `SignalGhost()` | UI button | Places a ghost signal on radar. |
| `BtnWepTab()` | UI button | Cycles through weapon display tabs. |
| `BtnTunerTab(tran, cur, max)` | UI button | Cycles through tuner frequency tabs. |

### Power System UI

The power UI displays five system bars (Engines, Arms, Bay, Support, Sig) plus a reactor bar. Each bar is driven by a shader material via `UIPowerClass`.

**UI elements:**
- `btnPowerBool` — List of 5 `Button` components for toggling system states.
- `imgPowerMet` — List of `Image` components holding the power bar shader materials.
- `btnPowerBoolImage` — Display images on the power toggle buttons.

`SyncPowerBarsFromManager()` runs every `Update()`, unconditionally pushing `currentPower`/`maxPower` from the `PowerManager` inspector values to the `_PowerCur` and `_PowerMax` shader properties.

### Speedometer & Velocity Display

**UI elements:**
- `speedometer` — `TextMeshProUGUI` showing current ship speed.
- `velocityMeterImg` — Array of `Image` components (player and bogie velocity bars).
- `velocityMeterTMP` — Array of `TextMeshProUGUI` showing velocity numbers.

When `autoUpdateSpeedometer` is true, a coroutine polls `PlayerShip` speed at `speedometerUpdateRate` intervals.

### Scanner / Sensor System

**UI elements:**
- `sensorLoad` — Transform container for the scanner display.
- `sensorLoadInfo` — 4 `TextMeshProUGUI` elements for hull section data (Port, Aft, Prow, Starboard).
- `sensorIcon` — `Image` for the scan target type icon.
- `sensorLoadSprites` — Sprites for different target types.
- `scannerImg` / `scannerSha` / `scannerSlider` — Scanner sweep display.
- `targetGO` — GameObject for the 3D target mesh display.
- `bogieMesh` — `MeshFilter` for rendering the target ship model.
- `shipPartsString` — `{ "_Port", "_Aft", "_Prow", "_Star" }` shader property names.

The scanner uses a slider-based sweep mechanic with configurable "pips" (hit zones). Successful scans reveal hull data.

### Weapon Screen (Firing VFX)

The weapon screen renders combat visuals using shader-driven animations:

**UI elements:**
- `weaponScreenImage` — `Image` with the weapon screen shader material.
- `screenWepGO` — The weapon screen container GameObject.
- `shipAngleTarget` — Transform tracking the ship's orientation for screen effects.
- `screenEnemyWeapon` — `RectTransform` for enemy weapon origin on screen.

**Weapon tab system:**
- `tabSprite` — Array of 2 sprites (inactive, active) for weapon tab states.
- `tabWepName` — `TextMeshProUGUI` array for weapon slot names.
- `tabFrame` — `Image` array for weapon slot frame highlights.
- `weaponNameText` / `weaponTypeText` / `weaponAmmoText` / `weaponStatusText` — Text displays.
- `weaponRechargeBar` — `Image` fill bar for reload/recharge.
- `weaponIconImage` — `Image` for the weapon type icon.
- Weapon icon sprites: `laserIcon`, `macrocannonIcon`, `missileIcon`, `pointDefenseIcon`, `boardingPodIcon`, `railgunIcon`.

### Bogie Management

**UI elements:**
- `bogieList` — `List<BogieClass>` of tracked enemies.
- `currentBogieTarget` — The currently selected enemy `GameObject`.
- `bogieTabTran` — `Transform[]` for bogie tab UI positions.
- `bogieTabBtn` — Tab button transform.

### World Grid Zoom Levels

| Index | Scale | Camera Size |
|---|---|---|
| 0 | 10× (tactical) | 50 units |
| 1 | 100× (sector) | 500 units |
| 2 | 1000× (strategic) | 5000 units |

**UI elements:**
- `gridImg` — `Image` with the grid shader.
- `sc` — Array of `Camera` instances for each zoom level.
- `viewTex` — Array of `Texture` for each zoom view.
- `canvasUI` — Array of `Canvas` layers.
- `screenWorld` — `RawImage` for the world view.

### Visual Effects

- **Screen shake:** `updateShipHit(float)` → `ShipShake()` coroutine using `shipHitCurve` AnimationCurve.
- **Compass:** `compassRect` RectTransform rotated to match ship heading.
- **Frequency dial:** `frequancyTrans` Transform + `tunerTrans` markers animated by `FrequancyTune()`.
- **Static glitch:** `staticImg` Image array with `_Glitch`, `_GlitchOpacity`, `_LineSize` shader properties.

---

## Core Game Systems

### GameManager

**File:** `Assets/Scripts/GameManager.cs`

The central singleton that bootstraps the game session. It persists across scene loads via `DontDestroyOnLoad`.

**Responsibilities:**
- Spawns enemy ships in configurable groups using the `EnemyGroup` system.
- Spawns the player ship at a designated spawn point.
- Initialises and wires together the three mission systems: `GameClock`, `QuestSystem`, and `WorldBoundary`.
- Listens for mission success/failure events and logs them.
- Handles the **Escape** key to toggle the strategic pause via `GameClock`.
- Tracks total enemies alive and checks win conditions.

#### EnemyGroup Configuration

Enemy spawning is driven by a list of `EnemyGroup` entries, each configuring a prefab, count, patrol behaviour, and respawn settings:

```csharp
[System.Serializable]
public class EnemyGroup
{
    public GameObject prefab;
    public int count;
    public bool patrol;
    public bool respawnOnDeath;
    public float respawnDelay;
    [ReadOnly] public int activeCount;
}
```

**Enemy spawning on Start:**
```csharp
SpawnAllEnemyGroups(); // Iterates enemyGroups, calling SpawnEnemy for each entry
```

Enemies spawn within a configurable scatter radius (`minScatterDistance` / `maxScatterDistance`) from `spawnCentre`, with a minimum `minSeparation` between ships. The system retries up to `maxSpawnAttempts` times to find a valid position via `FindSpawnPosition()` and `IsClearPosition()`.

**Win condition:** When `winOnAllEnemiesDestroyed` is true, `CheckWinCondition()` fires when `totalEnemiesAlive` reaches 0. The `EndSequence(bool won)` coroutine runs after `endSequenceDelay` seconds.

**Subscribing to mission events:**
```csharp
questSystem.OnMissionFailed  += (reason) => Debug.Log($"Mission Failed: {reason}");
questSystem.OnMissionComplete += () => Debug.Log("Mission Complete!");
```

**Key methods:**
| Method | Description |
|---|---|
| `SpawnPlayer()` | Instantiates the player ship at `playerSpawnPoint`. |
| `SpawnAllEnemyGroups()` | Spawns all configured enemy groups. |
| `SpawnEnemy(EnemyGroup)` | Spawns a single enemy from a group configuration. |
| `SpawnEnemyFromGroup(int)` | Spawns an enemy by group index (for respawn). |
| `SpawnEnemyShip(position, rotation)` | Instantiates an enemy ship prefab at a given location. |
| `FindSpawnPosition()` | Finds a valid spawn position with scatter and separation checks. |
| `IsClearPosition(Vector3)` | Returns true if a position is far enough from existing entities. |
| `CheckWinCondition()` | Checks if all enemies are dead and triggers end sequence. |
| `OnPlayerDestroyed()` | Handles player death — sets `playerAlive = false`. |
| `EndSequence(bool)` | Coroutine that triggers win/loss sequence after a delay. |
| `GetTotalEnemiesAlive()` | Returns the current count of living enemies. |
| `IsGameOver()` | Returns true if the game is over. |
| `IsPlayerAlive()` | Returns true if the player ship is alive. |

---

### GameClock

**File:** `Assets/Scripts/GameClock.cs`

A singleton clock that drives all time-sensitive systems (enemy ETAs, scanner sweeps, mission timers).

**Rules:**
- The clock advances every frame using a configurable `timeScale` multiplier.
- It **stops** during a strategic pause (triggered by Escape → `GameManager` → `GameClock.TogglePause()`).
- When the mission time limit is reached, `OnMissionTimerExpired` fires, which `QuestSystem` treats as a loss condition.

**Subscribing to clock events:**
```csharp
GameClock.Instance.OnTimeUpdated       += (elapsed) => UpdateTimerDisplay(elapsed);
GameClock.Instance.OnMissionTimerExpired += HandleTimerExpired;
GameClock.Instance.OnPaused            += () => ShowPauseBanner(true);
GameClock.Instance.OnResumed           += () => ShowPauseBanner(false);
```

**Calculating an ETA:**
```csharp
float eta = GameClock.Instance.CalculateETA(playerPos, targetPos, shipSpeed);
string display = GameClock.FormatTime(eta); // "00:08:32"
```

**Key methods:**
| Method | Description |
|---|---|
| `StrategicPause()` | Pauses the clock and sets `Time.timeScale = 0`. |
| `Resume()` | Resumes the clock and restores `Time.timeScale = 1`. |
| `TogglePause()` | Convenience wrapper to flip pause state. |
| `GetElapsedTime()` | Returns total elapsed seconds since mission start. |
| `GetRemainingTime()` | Returns seconds remaining; `-1` if there is no time limit. |
| `CalculateETA(from, to, speed)` | Returns travel time in seconds between two positions at a given speed. |
| `FormatTime(seconds)` | Static helper that returns a `HH:MM:SS` string. |
| `IsPaused()` | Returns true if the clock is currently paused. |
| `IsMissionActive()` | Returns true if a mission is running. |
| `GetTimeScale()` | Returns the current time scale multiplier. |
| `SetTimeScale(float)` | Sets the time scale multiplier. |

**Events:**
| Event | When it fires |
|---|---|
| `OnTimeUpdated` | Every frame (passes elapsed time). |
| `OnMissionTimerExpired` | When the timer runs out. |
| `OnPaused` / `OnResumed` | On pause/resume. |

---

### QuestSystem

**File:** `Assets/Scripts/QuestSystem.cs`

A singleton mission state machine. There is no quest journal UI — this is purely backend logic.

**Mission stages (in order):**

| Stage | Condition to advance |
|---|---|
| `NotStarted` | `StartMission()` is called. |
| `LocateTarget` | Player gets within `locateRegionRadius` of the target, or `ConfirmTargetOnRadar()` is called. |
| `EliminateTarget` | The target ship's GameObject is destroyed. |
| `ExitRegion` | Player reaches the jump point (within 500 units). |
| `MissionComplete` | Jump point reached. |
| `MissionFailed` | Player destroyed, deserted, or time/life support expired. |

**Loss condition sources wired automatically in `Start()`:**
```csharp
boundary.OnDesertion          += () => FailMission(MissionFailReason.Desertion);
clock.OnMissionTimerExpired   += () => FailMission(MissionFailReason.TimeExpired);
subsystems.OnLifeSupportFailed += () => FailMission(MissionFailReason.LifeSupportFailed);
```

**Starting a mission:**
```csharp
// Pass in the Transform of the enemy ship the player must destroy
QuestSystem.Instance.StartMission(enemyShipTransform);
```

**Jump point:** When the target is destroyed, a jump point is spawned at a random edge of the playable area (radius 45,000 units) and the position is broadcast via `OnJumpPointRevealed`.

**Key methods:**
| Method | Description |
|---|---|
| `StartMission(target)` | Begins the mission with the given ship as the objective. |
| `ConfirmTargetOnRadar()` | Manual trigger to advance from *Locate* to *Eliminate* (e.g. when scanner confirms target). |
| `UpdateLocateStage()` | Checks distance to target for auto-advancement. |
| `UpdateEliminateStage()` | Checks if target is destroyed. |
| `UpdateExitStage()` | Checks if player reached the jump point. |
| `RevealJumpPoint()` | Spawns a jump point at a random edge of the playable area. |
| `CompleteMission()` | Marks mission as complete and fires `OnMissionComplete`. |
| `FailMission(reason)` | Marks mission as failed with a `MissionFailReason`. |
| `GetCurrentStage()` | Returns the current `QuestStage` enum value. |
| `IsTargetLocated()` | Returns true if the target has been found. |
| `IsTargetDestroyed()` | Returns true if the target ship is destroyed. |
| `GetJumpPointPosition()` | Returns the world position of the jump point. |

---

### WorldBoundary

**File:** `Assets/Scripts/WorldBoundary.cs`

Enforces the playable area (a circle of configurable radius centred on the world origin).

**Behaviour:**
1. When the player enters the **warning zone** (within `warningDistance` of the boundary), `OnBoundaryWarning` fires.
2. When the player **crosses the boundary**, a 30-second desertion countdown begins (`OnOutOfBounds`).
3. If the player **returns** before the timer expires, everything resets (`OnReturnedToBounds`).
4. If the timer **reaches zero**, `TriggerDesertion()` fires `OnDesertion`, destroys the player ship, and the mission fails.

**Listening to boundary events (e.g. from a UI warning script):**
```csharp
WorldBoundary boundary = FindFirstObjectByType<WorldBoundary>();
boundary.OnBoundaryWarning       += ShowBoundaryWarning;
boundary.OnOutOfBounds           += StartDeserterCountdown;
boundary.OnDesertionTimerTick    += (remaining) => UpdateCountdownDisplay(remaining);
boundary.OnReturnedToBounds      += HideCountdownDisplay;
```

**Key events:**
| Event | Description |
|---|---|
| `OnBoundaryWarning` | Player is approaching the edge. |
| `OnOutOfBounds` | Player has left the play area; countdown started. |
| `OnDesertionTimerTick` | Fires every frame while out of bounds; passes remaining seconds. |
| `OnReturnedToBounds` | Player returned in time. |
| `OnDesertion` | Timer expired; ship is destroyed. |

**Public getters:**
| Method | Description |
|---|---|
| `IsOutOfBounds()` | Returns true if the player is outside the boundary. |
| `IsWarning()` | Returns true if the player is in the warning zone. |
| `GetDesertionTimeRemaining()` | Returns seconds left on the desertion timer. |
| `GetPlayableAreaRadius()` | Returns the radius of the playable area. |
| `HasDeserted()` | Returns true if the player has deserted. |

---

## Player Ship

### PlayerShip

**File:** `Assets/Scripts/PlayerShip.cs`

The main player-controlled component. Requires `ShipStability` on the same GameObject and auto-adds all other ship systems if missing.

#### Movement

| Control | Action |
|---|---|
| `W` | Accelerate forward |
| `S` | Decelerate / reverse |
| `A` / `D` | Turn left / right |
| `Q` | Emergency dodge left |
| `E` | Emergency dodge right |

- Speed changes are gradual via `rateOfAcceleration` (default 50) — there is no instant speed change.
- Maximum speed scales with Engine power and any Engines/Bridge subsystem debuffs.
- **No automatic deceleration** — the ship holds its current speed when no thrust key is held.
- Dodge is a quick lateral position shift (not a rotation) using smooth interpolation over `dodgeDuration` seconds (default 0.2).

**Core movement logic (FixedUpdate — simplified):**
```csharp
// Thrust input
float maxThrust = 100f * enginePower * internalSubsystems.GetSpeedMultiplier();
targetSpeed = Input.GetKey(KeyCode.W) ? maxThrust : -maxThrust;

// Smooth speed change
currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rateOfAcceleration * Time.fixedDeltaTime);
rb.linearVelocity = transform.up * currentSpeed;

// Turning
targetTurnSpeed = Input.GetAxis("Horizontal") * turnRate * enginePower;
currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, targetTurnSpeed, turnRate * 2f * Time.fixedDeltaTime);
rb.MoveRotation(rb.rotation * Quaternion.Euler(0, 0, -currentTurnSpeed * Time.fixedDeltaTime));
```

**Dodge execution:**
```csharp
// Q/E key → TryDodge(-1 or +1)
Vector3 dodgeOffset = transform.right * direction * dodgeDistance;
dodgeTargetPosition = transform.position + dodgeOffset;
// Smooth interpolation applied in FixedUpdate while isDodging == true
```

#### Hyperspeed

When engine efficiency exceeds `hyperspeedEngineThreshold` (default 0.8), the ship can enter hyperspeed mode. While active:
- Speed is multiplied by `hyperspeedSpeedMultiplier` (default 4×).
- Engine power drains at `hyperspeedDrainRate` (default 2) per second.
- Auto-disengages when engine efficiency drops below the threshold.

```csharp
// Public property:
bool inHyperspeed = playerShip.IsInHyperspeed;
```

#### Drift Physics

The ship accumulates lateral drift when turning at speed. Drift is the perpendicular velocity component that persists after direction changes.

| Field | Default | Description |
|---|---|---|
| `driftCompensationRate` | 8 | Rate at which lateral drift is corrected. |
| `maxLateralDrift` | 40 | Maximum allowed lateral velocity. |
| `driftStabilityDrainRate` | 0.4 | Stability drain applied while drifting. |

#### Weapons

| Control | Action |
|---|---|
| `Space` or `Left Ctrl` | Fire active weapon (not Railgun — see below) |
| `1` | Toggle Engine power state |
| `2` | Toggle Arms (weapon) power state |
| `3` | Toggle Sig (sensor) power state |
| `F2` | Toggle test mode (fire weapons without checking power) |

**Weapon firing with power check:**
```csharp
float weaponPower = powerManager.GetSystemEfficiency("arms");
if (weaponPower > 0.2f)
    weaponManager.FireActiveWeapon(transform.position + transform.up * 1000f, weaponPower);
```
> Minimum 20% arms efficiency required. Test mode (`F2`) bypasses this entirely.

**Railgun standby:** When the railgun fires, `PlayerShip` enters a read-only standby state for the reboot duration — all movement and weapon input is blocked:
```csharp
// Called by Railgun.PostFireStandby()
playerShip.EnterRailgunStandby(float duration);

// Guards in Update() / FixedUpdate():
if (IsOnStandby) return;
```

#### Camera

The camera is **not** a child of the ship. `PlayerShip` manually follows the ship each frame.

**Camera follow and UIController calls:**
```csharp
// Every FixedUpdate:
cameraTransform.position = transform.position + cameraOffset;

Vector3 cameraEuler = cameraTransform.eulerAngles;
cameraEuler.z = transform.eulerAngles.z;
cameraTransform.eulerAngles = cameraEuler;

// Push to UI
UIController.Instance.UpdateCompass(cameraEuler.z);       // Rotates the compass ring
UIController.Instance.WorldGridRotUpdate(cameraEuler.z);  // Rotates the grid overlay
UIController.Instance.WorldGridLocUpdate(cameraTransform.position); // Moves ship icon on grid
```

#### Damage

`PlayerShip` implements `IDamageable`. The directional overload determines the hit quadrant before delegating to `HullSystem`:

```csharp
// Projectile calls this with its travel direction
public void TakeDamage(float damage, Vector3 hitDirection)
{
    HullSide side = hullSystem.DetermineHitSide(hitDirection);
    hullSystem.TakeDamage(side, damage);
}
```
The legacy single-argument overload always hits the Prow.

**Key properties and methods:**
| Member | Description |
|---|---|
| `IsOnStandby` | Property; true during railgun standby. |
| `IsInHyperspeed` | Property; true when hyperspeed is active. |
| `GetCurrentSpeed()` | Returns the current forward speed. |
| `GetCurrentSpeedInBars()` | Returns speed normalised to a bar display range. |
| `GetTurnRate()` | Returns the current turn rate. |
| `SetCameraTransform(Transform)` | Assigns the camera for the follow system. |
| `EnterRailgunStandby(float)` | Blocks all input for the given duration (called by Railgun). |

---

### ShipStability

**File:** `Assets/Scripts/ShipStability.cs`

Tracks a stability meter (0–100) that is consumed by sharp turns and dodging, and recovers over time.

**UIController integration — initialisation and live updates:**
```csharp
// Called once on Start:
UIController.Instance?.StabilityMeterStart(currentStability, maxStability);

// Called every frame while stability is recovering:
UIController.Instance?.StabilityMeterUpdate(currentStability);
// StabilityMeterUpdate internally calls StabilityMeterColor() which tints the bar:
//   White  → stability ≥ 25%
//   Amber  → stability between 10% and 25%
//   Red    → stability < 10%
```

**Turn zones:**

| Zone | Degrees rotated this frame | Drain multiplier |
|---|---|---|
| Green | ≤ 0.6° | 0.1× |
| Yellow | ≤ 1.1° | 0.5× |
| Red | > 1.1° | 2.0× |

High speed further amplifies drain within each zone.

**Drain calculation (called each FixedUpdate by PlayerShip):**
```csharp
float drain = turnStabilityDrainMultiplier
            * drainMultiplier          // zone factor above
            * (1f + normalizedSpeed * speedStabilityDrainMultiplier)
            * Time.fixedDeltaTime;

currentStability = Mathf.Max(0, currentStability - drain);
```

**Dodge:**
- Costs 45 stability points.
- Has a 0.5-second cooldown between dodges.
- `CanPerformDodge()` returns false if stability < dodge cost or cooldown is active.

**Recovery:**
- Gradually refills at `stabilityRecoveryRate` per second.
- Recovery slows to 30% of normal when stability is critically low (below 10%).

**Key methods:**
| Method | Description |
|---|---|
| `CanPerformDodge()` | Returns true if cooldown is done and stability is sufficient. |
| `ApplyDodge()` | Consumes stability and starts cooldown. |
| `ResetDodge()` | Resets the dodge cooldown timer. |
| `CalculateTurnStabilityDrain(angle, speed, powerDecay)` | Applies drain with zone-based multiplier and power decay factor. Returns the amount drained. |
| `IsStabilityCritical()` | Returns true if stability is below 10%. |
| `IsStabilityDepleted()` | Returns true if stability is at 0. |
| `GetStabilityPercentage()` | Returns 0.0–1.0 representing current/max stability. |
| `GetCurrentStability()` | Returns the raw current stability value. |
| `GetMaxStability()` | Returns the max stability value. |
| `CanDodgeAgain()` | Returns true if the dodge cooldown has elapsed. |
| `ApplyStabilityDrain(float)` | Directly drains the given amount from stability. |
| `ShowAlert()` | Triggers the stability alert UI via `UIController`. |

---

### PowerManager

**File:** `Assets/Scripts/PowerManager.cs`

`[DefaultExecutionOrder(-10)]` — guaranteed to initialise before `UIController`.

Manages a shared reactor power pool distributed across **five** systems: **Engines**, **Arms**, **Bay**, **Support**, and **Sig**. Each system has a capacity of 5 bars; the reactor max is 15 bars. At game start all five systems are in Draw state at **1 bar each**, with the reactor holding **3 free bars** (8 total in the system). As the reactor regenerates, drawing systems fill automatically. UI buttons toggle individual systems between Draw/Standby/Vent so the player controls where reactor power flows.

**Power states per system:**

| State | Meaning |
|---|---|
| `Standby` | System neither draws nor vents. |
| `Draw` | System actively draws power from the reactor. |
| `Vent` | System releases its stored power back to the reactor. |

The toggle cycle is fixed: **Standby → Draw → Standby → Vent → Standby**. Venting cannot be cancelled once triggered.

**Cycling a system's state (player input in PlayerShip):**
```csharp
// Keys 1/2/3 in PlayerShip.Update()
if (Input.GetKeyDown(KeyCode.Alpha1)) powerManager.ToggleSystemState(powerManager.engines);
if (Input.GetKeyDown(KeyCode.Alpha2)) powerManager.ToggleSystemState(powerManager.arms);
if (Input.GetKeyDown(KeyCode.Alpha3)) powerManager.ToggleSystemState(powerManager.sig);
```

**Checking power efficiency (used before firing weapons and applying thrust):**
```csharp
float engineEfficiency = powerManager.GetSystemEfficiency("engines"); // 0.0 – 1.0
float armsEfficiency   = powerManager.GetSystemEfficiency("arms");
```

**Reactor passive regeneration:** The reactor always ticks back up toward `reactorMaxPower` at `reactorRegenRate` bars/sec (default 1), which in turn feeds any systems in Draw state. Regeneration is suspended while `reactorOnline == false` (during railgun post-fire reboot).

**UI integration:** `ChargeBtn(int)` calls `ToggleSystemState()` on the corresponding `PowerSystem`. `SyncPowerBarsFromManager()` runs every `Update()` in `UIController`, unconditionally pushing `currentPower`/`maxPower` to the power bar shaders — the inspector is always the source of truth.

- When multiple systems are in `Draw`, the fill rate is divided equally among them.
- If the Reactor internal subsystem is damaged, `GetReactorMultiplier()` caps the effective reactor max.
- Partial bars in progress when switching to Standby are completed before the system holds.

**Bonus effects per system (applied per bar above the minimum of 1):**
| System | Bonus per extra bar |
|---|---|
| Engines | +10% acceleration; −5% stability decay; MAX = Supercruise |
| Arms | −2.5% cannon load time |
| Bay | +10% boarding pod range |
| Support | −5% ability cooldown |
| Sig | +1% scanner perfect-hit window; +1 s comms intercept |

**Key methods:**
| Method | Description |
|---|---|
| `ToggleSystemState(system)` | Advances the fixed state cycle for the given system. |
| `GetSystemEfficiency(name)` | Returns 0.0–1.0 for `"engines"`, `"arms"`, `"bay"`, `"support"`, or `"sig"`. |
| `ToggleEngines()` / `ToggleArms()` / `ToggleBay()` / `ToggleSupport()` / `ToggleSig()` | Convenience wrappers for UI buttons. |
| `VentAllSystems()` | Sets all powered systems to Vent state. |
| `EmergencyVent()` | Forces an immediate vent of all systems. |
| `BlackAlert()` | Coroutine that vents all systems and auto-engages Engines + Arms. |
| `DrainAllPowerInstantly()` | Instantly zeros all system power **and** the reactor; sets `reactorOnline = false` (used by railgun on fire). |
| `DrainArmsPower(int)` | Drains a specific number of bars from the Arms system. |
| `RebootReactor()` | Sets `reactorOnline = true`, re-enables regen (called after railgun standby ends). |
| `AddPower(system)` / `RemovePower(system)` | Manual single-bar allocation from UI. |
| `GetReactorPower()` / `GetMaxReactorPower()` | Current and maximum reactor bar values. |
| `GetSystemState(name)` | Returns the `PowerState` of a named system. |
| `GetSystemPower(name)` | Returns the current power of a named system. |
| `GetSystemMaxPower(name)` | Returns the max power of a named system. |

**Bonus calculation methods (per bar above minimum 1):**
| Method | Description |
|---|---|
| `BonusBars(system)` | Returns extra bars above the base 1. |
| `GetEngineAccelerationMultiplier()` | +10% per bar. |
| `GetEngineStabilityDecayMultiplier()` | −5% per bar. |
| `IsSupercruiseUnlocked()` | Returns true if Engines is at max power. |
| `GetCannonLoadTimeMultiplier()` | −2.5% per bar. |
| `GetBoardingPodRangeMultiplier()` | +10% per bar. |
| `GetAbilityCooldownMultiplier()` | −5% per bar. |
| `GetScannerPerfectHitBonus()` | +1% per bar. |
| `GetCommsInterceptTimeBonus()` | +1 s per bar. |

---

### HullSystem

**File:** `Assets/Scripts/HullSystem.cs`

Divides the ship hull into four independent **quadrants**, each with its own health pool.

| Quadrant | Direction | Default max HP |
|---|---|---|
| Prow | Front | 40 |
| Port | Left | 50 |
| Starboard | Right | 50 |
| Aft | Rear | 10 |

**Determining which side was hit:**
```csharp
// Convert incoming projectile direction to the correct hull side:
HullSide side = hullSystem.DetermineHitSide(projectileVelocity.normalized);
hullSystem.TakeDamage(side, damageAmount);
```

**Internally, the side is determined by dot products in local space:**
```csharp
Vector3 localDir = transform.InverseTransformDirection(hitDirection.normalized);
float forward = Vector3.Dot(localDir, Vector3.up);   // Prow(+) vs Aft(-)
float right   = Vector3.Dot(localDir, Vector3.right); // Starboard(+) vs Port(-)
// Whichever axis has the larger component wins
```

**Damage flow:**
1. Damage is applied to the correct `HullQuadrant` only.
2. If a quadrant reaches 0 HP it is **breached** — `OnHullBreached` fires once.
3. Further damage to a breached side overflows into `InternalSubsystems`.
4. When all four quadrants are breached, `OnShipDestroyed` fires.

**Key events:**
| Event | Description |
|---|---|
| `OnHullBreached(side)` | Fires the first time a quadrant is breached. |
| `OnHullDamaged(side, damage, remaining)` | Fires on every hit. |
| `OnShipDestroyed` | All quadrants breached. |

---

### InternalSubsystems

**File:** `Assets/Scripts/InternalSubsystems.cs`

Manages seven internal ship components. When a hull quadrant is breached, any subsequent damage to that side randomly damages one of its associated subsystems.

**Subsystem → Hull side associations:**

| Subsystem | Hull Side | Effect when destroyed |
|---|---|---|
| Bridge | Prow | Movement lag; loss of steering |
| Sensors | Prow | Scanner disabled; radar range reduced |
| Reactor | Aft | Limits total power; periodically drains a random active system |
| Engines | Aft | Reduces max speed; increases turn time |
| Magazine | Starboard | Damages weapons |
| Life Support | Port | Starts a 120-second game-over countdown |
| Crew | Port | Disables special abilities |

**How hull overflow becomes subsystem damage:**
```csharp
// InternalSubsystems subscribes to HullSystem.OnHullDamaged
private void HandleHullDamage(HullSide side, float damage, float remainingHealth)
{
    if (remainingHealth > 0f) return; // Hull not breached yet — ignore

    Subsystem[] candidates = GetSubsystemsForSide(side);
    Subsystem target = candidates[Random.Range(0, candidates.Length)];
    target.TakeDamage(damage);
    // Fires OnSubsystemDamaged / OnSubsystemDestroyed events
}
```

**Reactor drain mechanic:** While the Reactor is damaged but not destroyed, every 5 seconds it drains power from a random `Draw`-state system:
```csharp
if (reactor.isDamaged && !reactor.isDestroyed)
{
    reactorDrainTimer += Time.deltaTime;
    if (reactorDrainTimer >= reactorDrainInterval) // default 5s
    {
        DrainRandomSystem();
        reactorDrainTimer = 0f;
    }
}
```

**Checking multipliers in PlayerShip / PowerManager:**
```csharp
float speed   = 100f * enginePower * internalSubsystems.GetSpeedMultiplier();
float turning = turnRate * enginePower * internalSubsystems.GetTurnRateMultiplier();
int   maxReactor = Mathf.RoundToInt(reactorMaxPower * internalSubsystems.GetReactorMultiplier());
```

**Key events:**
| Event | Description |
|---|---|
| `OnSubsystemDamaged(type, health)` | A subsystem took damage. |
| `OnSubsystemDestroyed(type)` | A subsystem hit 0 HP. |
| `OnLifeSupportCritical` | Life support destroyed; countdown started. |
| `OnLifeSupportFailed` | Countdown expired — game over. |

---

### Shields

**File:** `Assets/Scripts/Shields.cs`

An energy shield layer in front of the hull. While active, all incoming damage hits the shield first.

**Damage flow:**
```csharp
public void TakeDamage(float damage)
{
    if (currentShieldHealth > 0)
    {
        currentShieldHealth = Mathf.Max(0, currentShieldHealth - damage);
        StartCoroutine(FlashShield()); // Brief visual flash
        if (currentShieldHealth <= 0)
            DeactivateShield();
    }
    else
    {
        // Shield down — pass damage directly to the ship hull
        playerShip.TakeDamage(damage);
    }
}
```

**Visual states:**
- **Normal hit:** Shield renderer flashes visible for 0.5 s, then hides again.
- **Critical (< 30 HP):** Shield renderer blinks continuously.
- **Depleted:** Renderer is hidden entirely; damage passes directly to `PlayerShip.TakeDamage()`.

**Recharge:** After `rechargeDelay` seconds (default 3 s) with no incoming damage, the shield recharges at `rechargeRate` HP/s.

**Shield colour:** Interpolates from red (low) to cyan (full) based on health percentage using `Color.Lerp`.

---

### Autopilot

**File:** `Assets/Scripts/Autopilot.cs`

Automates ship movement in two modes. Disengages **instantly** the moment any manual movement input (WASD) is detected.

#### Navigation Mode
The ship turns to face a target world position and applies thrust. It does **not** decelerate automatically — the player must brake manually after arriving.

```csharp
// Activate from a world map coordinate selection:
autopilot.EngageNavigation(new Vector3(5000f, 12000f, 0f));
```

#### Combat Mode
The ship orients itself relative to a target ship and maintains a chosen engagement range.

```csharp
// Engage combat autopilot at 10,000 units range, presenting the port side:
autopilot.EngageCombat(enemyShipTransform, 10000f, AutopilotOrientation.Port);

// Change range band mid-combat (0=0, 1=5000, 2=10000, 3=18000, 4=25000):
autopilot.SetRange(3); // 18,000 units
```

**Manual input check (fires every FixedUpdate):**
```csharp
private bool HasManualInput()
{
    return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)
        || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
}
// Any of these → Disengage() is called immediately
```

**Key methods:**
| Method | Description |
|---|---|
| `EngageNavigation(target)` | Start navigating toward a world position. |
| `EngageCombat(target, range, orientation)` | Start combat autopilot against a target. |
| `SetRange(rangeIndex)` | Change the desired engagement range (index 0–4). |
| `SetOrientation(orient)` | Change which side faces the target. |
| `Disengage()` | Stop autopilot immediately. |

---

## Enemy

### EnemyShip

**File:** `Assets/Scripts/EnemyShip.cs`

An AI ship with typed presets, a state machine, patrol waypoints, and obstacle avoidance.

#### Enemy Types

```csharp
public enum EnemyType { Patrol, StationaryDefender, Tank, Interceptor, Sniper }
```

Each type applies stat overrides via `ApplyTypePreset()` on `Start()`:

| Type | Health | Thrust | Max Speed | Rotation | Fire Rate | Detection | Combat Range | Notes |
|---|---|---|---|---|---|---|---|---|
| Patrol | 3 | 8 | 15 | 90°/s | 0.5 s | 30 | 15 | Balanced default |
| StationaryDefender | 20 | 0 | 0 | 90°/s | 0.2 s | 50 | 15 | Static turret |
| Tank | 50 | 4 | 6 | 45°/s | 1.2 s | 30 | 15 | Slow, heavy |
| Interceptor | 2 | 18 | 28 | 150°/s | 0.35 s | 30 | 15 | Fast, fragile |
| Sniper | 2 | 8 | 15 | 90°/s | 0.8 s | 80 | 40 | Long range |

#### AI State Machine

```csharp
private enum EnemyState { Patrol, Chase, Combat }
```

**State transitions (runs in FixedUpdate via `UpdateState()`):**

| From | Condition | To |
|---|---|---|
| Patrol | Player within detection radius | Chase |
| Chase | Player within combat distance × 1.1 | Combat |
| Chase | Player leaves detection radius | Patrol |
| Combat | Player outside detection radius | Patrol |

**Patrol behaviour:** When `patrolEnabled`, the ship picks random waypoints within `patrolRadius` of its spawn origin. It navigates to each waypoint and picks a new one when within `waypointReachedThreshold`.

**Obstacle avoidance:** A 5-ray fan cast (`obstacleAvoidanceDistance`, `obstacleLayerMask`) steers the ship around obstacles via `GetObstacleAvoidedDirection()`.

**Combat behaviour (runs in `UpdateCombat()`):**

| Condition | Action |
|---|---|
| Player too far (> 1.1× optimal distance) | Thrust toward player |
| Player too close (< 0.5× optimal distance) | Thrust away from player |
| Player at optimal distance | Apply perpendicular force to orbit the player |

**Firing condition:**
```csharp
bool inCombatRange = distanceToPlayer <= detectionRadius;
bool facingPlayer  = Vector3.Dot(transform.up, directionToPlayer) > 0.7f; // ~45°
if (inCombatRange && facingPlayer && Time.time - lastFireTime > fireRate)
    FireLaser();
```

**When destroyed:** `Explode()` spawns a particle system completely in code (no prefab required) and destroys the GameObject. The particle explosion destroys itself after 2 seconds.

Implements `IDamageable` with both `TakeDamage(int)` and `TakeDamage(float)` overloads. When health reaches 0, `Explode()` is called.

**Key methods:**
| Method | Description |
|---|---|
| `ApplyTypePreset()` | Applies stat overrides based on `EnemyType`. |
| `UpdateState(float)` | Main state machine tick. |
| `UpdatePatrol()` | Navigate to patrol waypoints. |
| `UpdateChase()` | Pursue the player. |
| `UpdateCombat(float)` | Combat manoeuvring and firing. |
| `PickNewPatrolWaypoint()` | Picks a random waypoint within patrol radius. |
| `GetObstacleAvoidedDirection(Vector3)` | Returns a steered direction to avoid obstacles. |
| `RotateTowards(Vector3)` | Smoothly rotates toward a target direction. |
| `EnforceSpeedLimit()` | Caps velocity to `maxSpeed`. |
| `FireLaser()` | Spawns a laser projectile. |
| `Explode()` | Destroys the ship with a procedural particle effect. |

---

## Weapons System

### WeaponBase

**File:** `Assets/Scripts/Weapons/WeaponBase.cs`

Abstract base class every weapon inherits from.

```csharp
public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float range = 500f;
    [SerializeField] protected float angleOfFire = 45f;
    [SerializeField] protected float reloadTime = 1f;
    [SerializeField] protected int   maxAmmo = 30;
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float damageModifier = 1f;

    protected float fireRate = 0.2f; // 5 shots/sec
    protected int   currentAmmo;
    protected bool  isReloading;
    protected float lastFireTime;

    public virtual bool CanFire()
        => !isReloading && currentAmmo > 0 && Time.time - lastFireTime >= reloadTime;

    public int   GetCurrentAmmo()    => currentAmmo;
    public int   GetMaxAmmo()        => maxAmmo;
    public float GetReloadProgress() => /* 0..1, where 1 = fully ready */;

    public abstract void Fire(Vector3 target);
}
```

`CanFire()` returns true when: not reloading, ammo > 0, and `reloadTime` has elapsed. Subclasses may add extra conditions (e.g. lock achieved, spin-up complete).

---

### WeaponManager

**File:** `Assets/Scripts/Weapons/WeaponManager.cs`

Manages a list of `WeaponSlot`s and exposes a unified firing interface to `PlayerShip`.

**`WeaponSlot`** links a `WeaponType` enum, a `WeaponBase` instance, an active flag, an index, and a display name.

**WeaponType enum:**
```csharp
public enum WeaponType { Laser, Macrocannon, Missile, PointDefense, BoardingPod, Railgun, Broadside }
```

**Firing the active weapon (called from PlayerShip):**
```csharp
// PlayerShip.TryFireWeapons():
weaponManager.FireActiveWeapon(
    transform.position + transform.up * 1000f, // "target" far ahead
    powerManager.GetSystemEfficiency("weapons") // 0.0–1.0
);
```

**Switching weapons at runtime:**
```csharp
weaponManager.SwitchToWeapon(2);                       // by slot index
weaponManager.SwitchToWeaponType(WeaponType.Railgun);  // by type
weaponManager.SwitchToNextWeapon();                    // cycle forward
```

**Special case:** `LaserWeapon` uses `TryFire(powerEfficiency)` instead of the standard `Fire()` path so power level can gate firing:
```csharp
if (currentWeapon.weaponInstance is LaserWeapon laserWeapon)
    return laserWeapon.TryFire(weaponPowerEfficiency);
```

**Key methods:**
| Method | Description |
|---|---|
| `FireActiveWeapon(target, powerEfficiency)` | Fires the currently selected weapon if powered and ready. |
| `SwitchToWeapon(index)` | Selects a specific weapon slot. |
| `SwitchToNextWeapon()` / `SwitchToPreviousWeapon()` | Cycles through active slots. |
| `SwitchToWeaponType(type)` | Switches to the first active slot of a given type. |
| `AddWeapon(type, instance)` | Dynamically adds a weapon at runtime. |
| `RemoveWeapon(index)` | Removes a slot and re-indexes. |
| `SetWeaponActive(index, active)` | Enables or disables a slot. |
| `GetActiveWeapon()` | Returns the current `WeaponBase` instance. |
| `GetActiveWeaponSlot()` | Returns the current `WeaponSlot`. |
| `GetAllWeaponSlots()` | Returns the full list of weapon slots. |
| `GetWeaponsByType(type)` | Returns all `WeaponBase` instances matching a type. |
| `RefreshWeaponDisplay()` | Pushes current weapon stats to UIController. |
| `CanActiveWeaponFire()` | Returns true if the active weapon can fire. |
| `LoadActiveWeapon(target)` | Primes/targets the active weapon (e.g. missile lock, macrocannon arm). |
| `GetActiveWeaponIndex()` | Returns the current weapon slot index. |
| `GetWeaponCount()` | Returns the total number of weapon slots. |
| `GetActiveWeaponType()` | Returns the `WeaponType` of the active weapon. |

---

### WeaponUIDisplay

**File:** `Assets/Scripts/Weapons/WeaponUIDisplay.cs`

A component that periodically polls `WeaponManager` and pushes the active weapon's stats to `UIController.Instance.UpdateWeaponDisplay()`.

**Inspector fields:**
| Field | Default | Description |
|---|---|---|
| `weaponManager` | — | Reference to the ship's `WeaponManager`. |
| `autoFindWeaponManager` | true | Auto-locates the manager on the same GameObject if not assigned. |
| `updateInterval` | 0.1 s | How often the display refreshes. |

**Key methods:**
| Method | Description |
|---|---|
| `SetWeaponManager(WeaponManager)` | Assigns the weapon manager at runtime. |
| `ForceUpdate()` | Immediately refreshes the weapon display. |

Handles display formatting for `LaserWeapon`, `Macrocannon`, `MissileLauncher`, and `Railgun` with type-specific status strings and colours.

---

### Laser / LaserWeapon

**Files:** `Assets/Scripts/Weapons/Laser.cs` / `LaserWeapon.cs`

**`Laser`** is the projectile. It moves via `Rigidbody` velocity and damages on trigger contact.

**Collision logic (tag-based friendly-fire prevention):**
```csharp
private void OnTriggerEnter(Collider other)
{
    if (CompareTag("EnemyProjectile"))
    {
        // Try shields first, then hull with directional damage
        Shields shields = other.GetComponent<Shields>();
        if (shields != null) { shields.TakeDamage(damage); Destroy(gameObject); return; }

        PlayerShip ps = other.GetComponent<PlayerShip>();
        if (ps != null) { ps.TakeDamage(damage, rb.linearVelocity.normalized); Destroy(gameObject); }
    }
    else if (CompareTag("PlayerProjectile"))
    {
        EnemyShip es = other.GetComponent<EnemyShip>();
        if (es != null) { es.TakeDamage(Mathf.RoundToInt(damage)); Destroy(gameObject); }
    }
}
```

**`LaserWeapon`** spawns `Laser` prefabs and manages ammo + recharge. Requires ≥ `minPowerToFire` (default 0.2) weapon efficiency.

**UIController — weapon-screen visual effects:**
When a laser fires, the weapon screen animation is driven entirely from `UIController`:
```csharp
// Player fires → shows laser beam from ship to target on weapon screen
UIController.Instance.LaserFire();

// Enemy bogie fires → shows laser beam from that bogie's position
UIController.Instance.LaserFireEnemy(bogieIndex);
```
These calls animate a shader on `weaponScreenImage` using `_LaserFire`, `_LaserDegree`, `_LaserStart`, `_LaserEnd`, and `_LaserSize` material properties.

---

### Missile / MissileLauncher

**Files:** `Assets/Scripts/Weapons/Missile.cs` / `MissileLauncher.cs`

**`Missile`** is a homing projectile with a proximity fuse. It steers toward its assigned target each fixed frame using `Quaternion.RotateTowards` and applies continuous forward thrust.

**Inspector fields:**
| Field | Default | Description |
|---|---|---|
| `thrust` | 1000 | Forward force applied each frame. |
| `turnRate` | 180°/s | Maximum steering rate. |
| `armedDistance` | 100 | Distance travelled before the fuse is active. |
| `proximityFuseRadius` | 50 | Detonation proximity threshold. |
| `maxLifetime` | 30 s | Self-destruct timer. |
| `explosionRadius` | 100 | AoE damage radius on detonation. |

**Initialisation:**
```csharp
missile.Initialize(targetTransform, damageAmount);
```

**Detonation (area-of-effect with distance falloff):**
```csharp
private void Detonate()
{
    Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
    foreach (Collider hit in hits)
    {
        IDamageable target = hit.GetComponent<IDamageable>();
        if (target != null)
        {
            float dist       = Vector3.Distance(transform.position, hit.transform.position);
            float multiplier = 1f - (dist / explosionRadius); // Linear falloff
            target.TakeDamage(damage * multiplier);
        }
    }
    Destroy(gameObject);
}
```

**`MissileLauncher`** manages multiple independent launch tubes:

```csharp
// Begin locking a tube onto a target:
missileLauncher.AttemptLock(enemyTransform, tubeIndex: 0);

// Lock all tubes at once (called by UI load button):
missileLauncher.LockAllTubes(enemyTransform);

// Fire all tubes that have achieved lock:
missileLauncher.Fire(Vector3.zero); // target parameter unused for lock-based launcher
```

- Each tube holds one missile loaded from the ammo pool.
- Lock requires `lockOnTime` seconds of continuous tracking within valid range.
- After firing, tubes auto-reload after `reloadTime` seconds.

---

### Railgun

**File:** `Assets/Scripts/Weapons/Railgun.cs`

A charge-and-release weapon gated on Arms power. Manages its own input in `Update()` — bypasses the standard `WeaponManager.FireActiveWeapon()` path entirely (`CanFire()` always returns `false`; `Fire()` is a no-op).

**Full firing lifecycle:**

| Step | What happens |
|---|---|
| Hold `Space` | `TryStartCharging()` — requires Arms efficiency > 90%. Calls `VentAllSystems()` so power is drained during the hold. |
| Release `Space` | `FireRailgun()` is called immediately (no extra delay). |
| Fire | Sequential penetrating raycast fires in `shipTransform.up` direction. Enemies hit take 9999 damage (instant kill). `DrainAllPowerInstantly()` zeros all system and reactor power; reactor goes offline. |
| Post-fire standby | `PostFireStandby()` coroutine blocks all ship input for `standbyDuration` seconds (default 4 s) via `PlayerShip.EnterRailgunStandby()`. |
| Reboot | After standby, `PowerManager.RebootReactor()` brings the reactor back online, enabling passive regen and putting Engines + Arms into Draw. |

**Penetrating raycast loop:**
```csharp
while (remainingRange > 0 && penetrationCount <= maxPenetrations)
{
    if (Physics.Raycast(currentOrigin, fireDirection, out RaycastHit hit, remainingRange))
    {
        ApplyDamage(hit, currentDamage);
        currentDamage *= (1f - damageDropoffPerPenetration); // 15% less per hit

        if (ShouldStopBeam(hit)) break;  // HeavyArmor / Station tags stop the beam

        currentOrigin = hit.point + fireDirection * 0.1f; // Step past the hit object
        remainingRange -= hit.distance + 0.1f;
        penetrationCount++;
    }
    else break;
}
```

**Visual beam:** A `LineRenderer` fades out over `beamDuration` seconds.

**UIController — weapon-screen visual:**
```csharp
// Called automatically in FireRailgun() after the beam is drawn
UIController.Instance.RailFire();
```

**`FireInstant()`** triggers `FireRailgun()` immediately if not already charging or on standby (for AI / testing).

**Public state accessors (for UI):**
```csharp
bool  charging  = railgun.IsCharging();
bool  standby   = railgun.IsOnStandby();
float progress  = railgun.GetChargeProgress();    // 0.0–1.0
float chargeT   = railgun.GetChargeTime();        // configured charge seconds
int   maxPen    = railgun.GetMaxPenetrations();    // max objects the beam penetrates
float maxRange  = railgun.GetMaxRange();           // beam range in units
int   curAmmo   = railgun.GetCurrentAmmo();
int   maxAmmo   = railgun.GetMaxAmmo();
```

---

### BroadsideCannon

**File:** `Assets/Scripts/Weapons/BroadsideCannon.cs`

Fires `Shell` projectiles from **both** port and starboard fire points simultaneously at an auto-targeted nearby enemy.

**Targeting sequence:**
1. While the fire button is held, `FindTarget()` uses `Physics.OverlapSphere` within `lockRadius` to find the nearest `EnemyShip`.
2. A lock timer counts up over `lockTime` seconds.
3. *Lock is lost if the fire button is released for more than 0.2 s.*
4. Once `isLocked == true`, `Shoot()` fires a shell from each side.

**Target auto-detection:**
```csharp
Collider[] colliders = Physics.OverlapSphere(transform.position, lockRadius);
currentTarget = colliders
    .Select(c => c.GetComponentInParent<EnemyShip>())
    .Where(e => e != null)
    .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
    .FirstOrDefault()?.transform;
```

**UI helper (for an optional lock progress bar):**
```csharp
float progress = broadsideCannon.GetLockProgress(); // 0.0 – 1.0
bool  locked   = broadsideCannon.IsLocked();
```

---

### Macrocannon

**File:** `Assets/Scripts/Weapons/Macrocannon.cs`

A multi-barrel gun that requires a **manual arming** step before each salvo.

**Usage flow:**
```csharp
// 0. Enter the viewfinder (optional — for targeting UI):
macrocannon.EnterViewfinder();

// 1. Arm the weapon (loads one shell per call, 1 second each):
macrocannon.ArmWeapon(); // Repeat up to numBarrels times

// 2. Fire an armed shell:
macrocannon.Fire(targetPosition);

// 3. Optionally try to acquire a targeting lock first:
macrocannon.AttemptLock(enemyTransform);
if (macrocannon.HasValidLock()) macrocannon.Fire(targetPosition);
```

Damage scales with distance: `damage = Mathf.Max(baseDamage, distance * damagePerUnit) * damageModifier`.

Once all loaded shells are spent, the cannon is unarmed and must be re-armed.

---

### PointDefenseCanon / PDCBullet

**Files:** `Assets/Scripts/Weapons/PointDefenseCanon.cs` / `PDCBullet.cs`

A rapid-fire defensive weapon designed to intercept incoming threats.

**Spin-up requirement:**
```csharp
pdc.SpinUp();   // Starts a 0.5s delay coroutine; CanFire() returns false until ready
// ...
pdc.SpinDown(); // Immediately disables firing
```

**Firing with target tracking:**
```csharp
pdc.TrackTarget(incomingMissileTransform); // Smoothly rotates toward target at maxTrackingSpeed°/s
pdc.Fire(incomingMissileTransform.position); // Fires one bullet per call (respects fire rate)
```

Bullets rotate with random spread (configurable degrees) applied via `Quaternion.Euler` before firing.

**`PDCBullet`:** Moves at high velocity, damages any `IDamageable` on trigger contact, and auto-destroys after 3 seconds.

---

### BoardingPod / BoardingPodLauncher

**Files:** `Assets/Scripts/Weapons/BoardingPod.cs` / `BoardingPodLauncher.cs`

A non-kinetic weapon that physically attaches to an enemy ship and deals damage after a boarding timer.

**`BoardingPod` lifecycle:**
```csharp
// 1. Launched toward target
pod.Initialize(targetTransform, initialVelocity, damageAmount);

// 2. On collision with target: attaches (becomes child, Rigidbody goes kinematic)
transform.parent = target;
rb.isKinematic = true;

// 3. Boarding timer counts up; when complete:
IDamageable damageable = target.GetComponent<IDamageable>();
damageable?.TakeDamage(damage); // Damage applied after boardingTime seconds
Destroy(gameObject);
```

If the pod collides with anything *other* than the target, it is destroyed immediately.

**`BoardingPodLauncher`:**
```csharp
launcher.SetTarget(enemyTransform);  // Validates distance (200–5000 units)
launcher.Fire(Vector3.zero);         // Launches if target is valid and pod is loaded
```

After firing, the launcher auto-reloads after `reloadTime` seconds if ammo remains.

---

### Shell

**File:** `Assets/Scripts/Weapons/Shell.cs`

Kinetic projectile used by `BroadsideCannon` and `Macrocannon`. Moves by directly advancing `transform.position` each frame (no Rigidbody physics simuation).

```csharp
// Initialised by the weapon that spawns it:
shell.Initialize(transform.forward * shellVelocity, damageAmount);

// In Update():
transform.position += velocity * Time.deltaTime;
```

Damages any `IDamageable` on trigger entry. Auto-destroys after 15 seconds.

---

## Radar System

### Radar

**File:** `Assets/Scripts/Radar.cs`

A singleton that tracks all registered `RadarTarget` objects and positions their blips on the radar UI.

**Range steps:** 500 / 1,000 / 2,000 / 4,000 / 8,000 units. Cycles with the `R` key.

**Blip update loop (every frame via `UpdateBlips()`):**
```csharp
foreach (RadarTarget target in targets)
{
    RectTransform blipRect = UIController.Instance.CreateOrGetRadarBlip(target);

    // Convert world position to player-local 2D
    Vector3 relPos  = playerTransform.InverseTransformPoint(target.transform.position);
    Vector2 radarPos = new Vector2(relPos.x, relPos.y);

    float scale = radarPos.magnitude / currentRange; // 0 = at player, 1 = at radar edge
    if (scale > 1f) { blipRect.gameObject.SetActive(false); continue; }

    blipRect.anchoredPosition = radarPos.normalized * scale * UIController.Instance.RadarBlipRadius;

    if (target.trackRotation)
    {
        float angle = target.transform.eulerAngles.z - playerTransform.eulerAngles.z;
        blipRect.localEulerAngles = new Vector3(0, 0, angle);
    }
}
```

**UIController integration:**
| Call | Purpose |
|---|---|
| `UIController.Instance.CreateOrGetRadarBlip(target)` | Spawns a blip prefab under `radarBlipContainer` (or returns existing one). Applies the target's icon and colour. |
| `UIController.Instance.DestroyRadarBlip(target)` | Destroys the blip GameObject and clears the dictionary entry. |
| `UIController.Instance.RadarBlipRadius` | Read-only property giving the pixel radius of the radar circle for positioning calculations. |

**Static helpers (called by `RadarTarget` components):**
```csharp
Radar.RegisterTarget(this);    // Called on OnEnable
Radar.UnregisterTarget(this);  // Called on OnDisable
```

---

### RadarTarget

**File:** `Assets/Scripts/RadarTarget.cs`

A small component added to any GameObject that should appear on the radar.

```csharp
public class RadarTarget : MonoBehaviour
{
    public Sprite icon;          // Sprite shown as the blip
    public Color  color = Color.white; // Tint applied to the blip image
    public bool   trackRotation; // If true, blip rotates with the target

    private void OnEnable()  => Radar.RegisterTarget(this);
    private void OnDisable() => Radar.UnregisterTarget(this);
}
```

Adding this component to an enemy ship, asteroid, or waypoint is all that is needed to make it appear on the radar.

---

## Communications System

### CommsManager / Signal

**File:** `Assets/Scripts/CommsManager.cs`

Handles signal interception — a mini-game where the player tunes to an incoming radio transmission.

**`Signal` data class:**
```csharp
[System.Serializable]
public class Signal
{
    public float  frequency;   // Target frequency (1.0 – 99.9)
    public int    bandType;    // Required band: 1, 2, or 3
    public string message;     // Text revealed on success
    public bool   isEncrypted; // Reserved (decryption not yet implemented)
    public float  timeLimit;   // Seconds to intercept before failure
}
```

**Triggering an interception:**
```csharp
// From any external system (scripted event, scanner contact, etc.):
commsManager.ReceiveSignal(new Signal
{
    frequency  = 47.3f,
    bandType   = 2,
    message    = "==|RAJA|==\n8 Hrs to Delta Point\nSpeed 90",
    timeLimit  = 30f
});

// Or generate a random test signal:
commsManager.GenerateTestSignal();
```

**Full interception flow with UIController calls:**
```csharp
// 1. Signal received → pause game and show UI
Time.timeScale = 0f;
UIController.Instance?.ShowCommsPanel(true);

// 2. Player presses 1/2/3 → update band display
UIController.Instance?.SetCommsBand(currentBand);

// 3. Player holds Left/Right arrow → adjust frequency
currentFrequency = Mathf.Clamp(currentFrequency + delta, 1.0f, 99.9f);
UIController.Instance?.SetCommsFrequency(currentFrequency);

// 4. Every frame: update signal strength colour
float distance = Mathf.Abs(currentFrequency - currentSignal.frequency);
float strength = Mathf.Clamp01(1f - distance / 10f);
UIController.Instance?.SetCommsSignalStrength(Color.Lerp(Color.red, Color.green, strength));

// 5. Signal tone pitch rises with strength
signalToneSource.pitch = 0.5f + strength;

// 6. Lock achieved (correct band + frequency within ±1.0)
interceptedMessages.Add(currentSignal.message);
UIController.Instance?.AddCommsLog(currentSignal.message);

// 7. Close panel and resume
UIController.Instance?.ShowCommsPanel(false);
Time.timeScale = 1f;
```

**Frequency tuner dial** (driven by arrow keys → `FrequancyTune`):
```csharp
// CommsManager.FrequancyTune calls:
UIController.Instance?.FrequancyTune(speed);
// UIController animates the mechanical dial (frequancyTrans.rotation) and
// slides the tuner marker between its left/right stops.
```

---

## Environment

### Asteroid

**File:** `Assets/Scripts/Asteroid.cs`

A destructible environment object that implements `IDamageable`.

**Damage and destruction:**
```csharp
public float TakeDamage(float damage)
{
    currentHealth -= Mathf.Min(damage, currentHealth);

    if (currentHealth <= maxHealth * damageThreshold && currentHealth > 0)
        ShowDamage(); // Swap to damagedMaterial if assigned

    if (currentHealth <= 0)
        DestroyAsteroid();

    return damage;
}

private void DestroyAsteroid()
{
    Instantiate(destructionEffect, transform.position, Quaternion.identity);

    int debrisCount = Random.Range(minDebris, maxDebris + 1);
    for (int i = 0; i < debrisCount; i++)
    {
        GameObject debris = Instantiate(debrisPieces[Random.Range(0, debrisPieces.Length)],
                                        transform.position + Random.insideUnitSphere * 0.5f,
                                        Random.rotation);
        debris.GetComponent<Rigidbody>()?.AddForce(Random.insideUnitSphere * debrisForce, ForceMode.Impulse);
        Destroy(debris, 10f); // Auto-clean debris
    }
    Destroy(gameObject);
}
```

Automatically tagged `"Asteroid"` on `Start()`. The Railgun uses this tag to decide whether its beam should **penetrate** (asteroids) or **stop** (other solid objects).

---

## Interfaces

### IDamageable

**File:** `Assets/Scripts/Interfaces/IDamageable.cs`

A shared interface implemented by any object that can take damage.

```csharp
public interface IDamageable
{
    /// <summary>Apply damage. Returns actual damage dealt (after resistances).</summary>
    float TakeDamage(float amount);

    float GetCurrentHealth();
    float GetMaxHealth();

    /// <summary>Returns false if already destroyed — prevents double-damage.</summary>
    bool CanBeDamaged();
}
```

**Implemented by:** `PlayerShip`, `EnemyShip`, `Asteroid`.

Using this interface allows any projectile to damage any valid game object without knowing its specific type:

```csharp
// From Missile.Detonate(), Shell.OnTriggerEnter(), etc.:
IDamageable target = hitCollider.GetComponent<IDamageable>();
if (target != null && target.CanBeDamaged())
    target.TakeDamage(damage);
```

---

## UI References

### BogieClass

**File:** `Assets/UI/UI_Script/BogieClass.cs`

A serialisable data structure representing an enemy ship on the UI. Each tracked enemy has a `BogieClass` instance in `UIController.bogieList`.

```csharp
[System.Serializable]
public class BogieClass
{
    public GameObject go;          // The enemy GameObject in the scene
    public Mesh       mesh;        // The enemy's mesh (for scanner 3D display)
    public GameObject wepImageGo;  // Weapon screen UI element for this bogie
    public Material   matWep;      // Weapon screen material instance

    public BogieClass(GameObject go, Mesh mesh, GameObject wepImageGo, Material matWep);
}
```

**`WeapStart()`** initialises the weapon material with default shader values:
```csharp
public void WeapStart()
{
    matWep.SetFloat("_LaserFire", 0);
    matWep.SetFloat("_RangeVisOn", 0);
    matWep.SetFloat("_StabilityBool", 0);
    matWep.SetColor("_RangeColor", Color.white);
}
```

---

### UIPowerClass

**File:** `Assets/UI/UI_Script/UIPowerClass.cs`

A MonoBehaviour that binds a power bar `Image` to its shader material and manages charge animations.

**Public fields:**
| Field | Type | Description |
|---|---|---|
| `mat` | `Material` | Shader material for the power bar. |
| `max` | `int` | Maximum power level for this system. |
| `cur` | `int` | Current power level. |
| `pwr` | `float` | Raw power value. |
| `charge` | `bool` | Whether this system is currently charging. |
| `pwrShift` | `bool` | Whether a power shift is in progress. |

**Key methods:**
| Method | Description |
|---|---|
| `UpdateMat()` | Pushes `cur` and `max` to `_PowerCur` and `_PowerMax` shader properties. |
| `Charge(bool)` | Sets the `_Charge` shader property to 1 (charging) or 0 (idle). |

**Shader properties driven:**
- `_PowerCur` — Current power level (int).
- `_PowerMax` — Maximum power level (int).
- `_ChargeVelocity` — Controls the speed of the charge animation.
- `_Charge` — Boolean flag (0 or 1) for charge visual state.

---

### Shader Property Reference

A complete reference of shader properties used across the UI.

#### Power System Shaders
| Property | Type | Used By | Description |
|---|---|---|---|
| `_PowerCur` | float | Power bars, velocity meters, stability | Current value. |
| `_PowerMax` | float | Power bars, velocity meters, stability | Maximum value. |
| `_Charge` | float | Power bars | 0/1 flag — is charging. |
| `_ChargeVelocity` | float | Power bars | Animation speed for charge effect. |
| `_DegreeHor` | float | Power bar layout | Horizontal degree offset. |
| `_DegreeVert` | float | Power bar layout | Vertical degree offset. |
| `_End` | float | Power bar layout | End position. |
| `_On` | float | Power bars | System active flag. |
| `_Reactor` | float | Reactor bar | Reactor power level. |

#### Radar Shaders
| Property | Type | Description |
|---|---|---|
| `_RangeDegree` | float | Angular position of a radar element. |
| `_Bogey` | float | Bogey presence flag. |
| `_Show` | float | Show/hide flag. |
| `_RadarColor` | Color | Tint colour for blips. |
| `_RadarRange` | float | Current radar range value. |
| `_VisualRange` | float | Visual range indicator scale. |
| `_LineSizeV2` | Vector2 | Line size for radar sweep. |

#### Weapon Screen Shaders
| Property | Type | Description |
|---|---|---|
| `_LaserDegree` | float | Angle of laser beam on the weapon screen. |
| `_LaserFire` | float | 0/1 flag — laser beam visible. |
| `_LaserStart` | float | Start position of laser beam. |
| `_LaserEnd` | float | End position of laser beam. |
| `_LaserSize` | float | Thickness of the laser beam. |
| `_GunLoc` | float | Gun location on screen. |
| `_RangeVisOn` | float | Range indicator visibility. |
| `_StabilityBool` | float | Stability visual flag. |
| `_RangeColor` | Color | Range indicator colour. |
| `_Stability` | float | Stability value for display. |

#### World Grid Shaders
| Property | Type | Description |
|---|---|---|
| `_ShipLocV2` | Vector2 | Ship position on the grid. |
| `_ShipRotation` | float | Ship rotation angle. |
| `_WorldView` | float | Current zoom level. |
| `_CellSize` | float | Grid cell size. |
| `_GridAmmount` | float | Grid line count. |
| `_GridThickness` | float | Grid line thickness. |

#### Static / Glitch Shaders
| Property | Type | Description |
|---|---|---|
| `_Glitch` | float | Glitch intensity. |
| `_GlitchOpacity` | float | Glitch opacity. |
| `_LineSize` | float | Scan line size for glitch effect. |

#### Stability Shader
| Property | Type | Description |
|---|---|---|
| `_PowerCur` | float | Current stability value. |
| `_PowerMax` | float | Maximum stability value. |
| `_OnColor` | Color | Bar tint (white/amber/red by threshold). |

#### Scanner / Hull Display
| Property | Type | Description |
|---|---|---|
| `_Port` | float | Port hull integrity (0.0–1.0). |
| `_Aft` | float | Aft hull integrity (0.0–1.0). |
| `_Prow` | float | Prow hull integrity (0.0–1.0). |
| `_Star` | float | Starboard hull integrity (0.0–1.0). |

---

### TestUIKeyController

**File:** `Assets/TestUIKeyControler.cs`

A test-only script for driving UI elements from keyboard input without the full game loop.

**Key bindings:**
| Key | Action |
|---|---|
| `R` | Trigger railgun fire UI (`RailFire()`) |
| `W` / `S` | Increase/decrease speed and update speedometer |
| `B` | Trigger ship-hit screen shake and apply test damage |
| `Q` / `E` | Left/right dodge (delegated to `PlayerShip`) |
| `1` / `2` / `3` | Toggle power systems (delegated to `PlayerShip`) |
| `0` | Update compass with unloaded state |
| `4` | Scan target with 25% hull on all sides (asteroid) |
| `5` | Scan target with 50% hull on all sides (orbiter) |
| `6` | Scan target with 100% hull on all sides (enemy ship) |

**References:**
- `playerShipRef` — `PlayerShip` instance for delegated input.
- `playerShipTrans` — Ship `Transform` for position/rotation queries.
- `cameraTransform` — Camera `Transform` for compass tracking.

Tracks camera rotation changes to update the compass display and reads the actual Rigidbody velocity from the `PlayerShip` for speedometer updates.
