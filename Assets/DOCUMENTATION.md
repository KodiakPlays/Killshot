# Starwolf — Codebase Documentation

> **Scope:** All game scripts under `Assets/Scripts/` and the `UIController` (`Assets/UI/UI_Script/UIController.cs`).

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

---

## Architecture Overview

Starwolf is a top-down 2D space combat game built in Unity. The game world is oriented on the XY plane — the ship moves forward along its local `transform.up` axis and the Z axis is frozen for all Rigidbodies.

**Key design rules present throughout the code:**
- Damage is always **directional** — it hits the hull quadrant facing the impact.
- **Power** is a shared reactor resource distributed among engines, weapons, and sensors.
- **Stability** is consumed by sharp turns and dodging; depleting it prevents further maneuvers.
- **Mission failure** can be triggered by player destruction, desertion, time expiry, or life support failure.
- **`UIController`** is a singleton accessed everywhere via `UIController.Instance`. Backend scripts call it to push data into the HUD — the UI does not poll the game state.

---

## UIController — The UI Bridge

**File:** `Assets/UI/UI_Script/UIController.cs`

`UIController` is the central singleton that owns every piece of on-screen UI. Backend game scripts call its methods to update the HUD; the UI itself does not poll game state.

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
| `UpdateCompass(float angle)` | `PlayerShip` | Rotates the compass ring to match the ship's Z rotation. |
| `WorldGridRotUpdate(float r)` | `PlayerShip` | Rotates the world grid overlay with the ship. |
| `WorldGridLocUpdate(Vector2 pos)` | `PlayerShip` | Moves the ship icon on the world grid. |
| `updateShipHit(float duration)` | `UIController` (weapon coroutines) | Triggers a screen-shake coroutine driven by an `AnimationCurve`. |
| `ShowCommsPanel(bool)` | `CommsManager` | Shows or hides the signal intercept panel. |
| `SetCommsBand(int)` | `CommsManager` | Highlights the active frequency band indicator. |
| `SetCommsFrequency(float)` | `CommsManager` | Updates the frequency slider and text display. |
| `SetCommsSignalStrength(Color)` | `CommsManager` | Tints the signal strength indicator (grey/red/green). |
| `AddCommsLog(string)` | `CommsManager` | Writes an intercepted message to the comms log. |
| `FrequancyTune(float speed)` | `CommsManager` | Rotates the analogue frequency dial and moves the tuner marker. |
| `CreateOrGetRadarBlip(target)` | `Radar` | Spawns or retrieves the `RectTransform` blip for a `RadarTarget`. |
| `DestroyRadarBlip(target)` | `Radar` | Destroys the blip and removes it from the internal dictionary. |
| `RadarBlipRadius` | `Radar` | Read-only property; the pixel radius of the radar circle. |
| `UpdateWeaponDisplay(...)` | `WeaponUIDisplay` | Updates the weapon name, ammo, recharge bar, and icon all at once. |
| `GetWeaponIcon(WeaponType)` | `WeaponUIDisplay` | Returns the `Sprite` assigned for a given weapon type. |
| `LaserFire()` | UIController (test, `p` key) | Plays the laser beam animation on the weapon screen toward the current target. |
| `LaserFireEnemy(int i)` | UIController (test, `0`–`4` keys) | Plays a laser fire animation originating from bogie `i`. |
| `RailFire()` | UIController (test, `r` key) | Plays the full railgun charge-and-fire beam animation on the weapon screen. |
| `WorldGridZoom(int i)` | UI buttons | Switches between the three zoom levels (0 = 10×, 1 = 100×, 2 = 1000×). |
| `BogeySpot(float degree)` | UIController radar test | Briefly lights up a bogey blip on the radar at the given angle. |
| `ScanNewTarget()` | UI button / `]` key | Cycles `currentBogieTarget` to the next entry in `bogieList`. |

### World Grid Zoom Levels

| Index | Scale | Camera Size |
|---|---|---|
| 0 | 10× (tactical) | 50 units |
| 1 | 100× (sector) | 500 units |
| 2 | 1000× (strategic) | 5000 units |

---

## Core Game Systems

### GameManager

**File:** `Assets/Scripts/GameManager.cs`

The central singleton that bootstraps the game session. It persists across scene loads via `DontDestroyOnLoad`.

**Responsibilities:**
- Spawns 5 enemy ships at random positions on start.
- Initialises and wires together the three mission systems: `GameClock`, `QuestSystem`, and `WorldBoundary`.
- Listens for mission success/failure events and logs them.
- Handles the **Escape** key to toggle the strategic pause via `GameClock`.

**Enemy spawning on Start:**
```csharp
for (int i = 0; i < 5; i++)
{
    Vector3 randomPos = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), 0);
    SpawnEnemyShip(randomPos, Quaternion.identity);
}
```

**Subscribing to mission events:**
```csharp
questSystem.OnMissionFailed  += (reason) => Debug.Log($"Mission Failed: {reason}");
questSystem.OnMissionComplete += () => Debug.Log("Mission Complete!");
```

**Key method:**
| Method | Description |
|---|---|
| `SpawnEnemyShip(position, rotation)` | Instantiates an enemy ship prefab at a given location. |

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

- Speed changes are gradual via `rateOfAcceleration` — there is no instant speed change.
- Maximum speed scales with Engine power and any Engines/Bridge subsystem debuffs.
- **No automatic deceleration** — the ship holds its current speed when no thrust key is held.
- Dodge is a quick lateral position shift (not a rotation) using smooth interpolation over `dodgeDuration` seconds.

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

#### Weapons

| Control | Action |
|---|---|
| `Space` or `Left Ctrl` | Fire active weapon |
| `1` | Toggle Engine power state |
| `2` | Toggle Weapon power state |
| `3` | Toggle Sensor power state |
| `F2` | Toggle test mode (fire weapons without checking power) |

**Weapon firing with power check:**
```csharp
float weaponPower = powerManager.GetSystemEfficiency("weapons");
if (weaponPower > 0.2f)
    weaponManager.FireActiveWeapon(transform.position + transform.up * 1000f, weaponPower);
```
> Minimum 20% weapon efficiency required. Test mode (`F2`) bypasses this entirely.

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
| Green | ≤ 1.0° | 0.1× |
| Yellow | ≤ 1.5° | 0.5× |
| Red | > 1.5° | 2.0× |

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
| `CalculateTurnStabilityDrain(angle, speed)` | Applies drain and returns the amount drained. |
| `IsStabilityCritical()` | Returns true if stability is below 10%. |

---

### PowerManager

**File:** `Assets/Scripts/PowerManager.cs`

Manages a shared reactor power pool distributed across three systems: **Engines**, **Weapons**, and **Sensors**. Each system has a capacity of 5 bars; the reactor pool starts at 8 bars.

**Power states per system:**

| State | Meaning |
|---|---|
| `Standby` | System neither draws nor vents. |
| `Draw` | System actively draws power from the reactor. |
| `Vent` | System releases its stored power back to the reactor. |

Toggling a system cycles: Standby → Draw → Vent → Standby.

**Cycling a system's state (player input in PlayerShip):**
```csharp
// Keys 1/2/3 in PlayerShip.Update()
if (Input.GetKeyDown(KeyCode.Alpha1)) powerManager.ToggleSystemState(powerManager.engines);
if (Input.GetKeyDown(KeyCode.Alpha2)) powerManager.ToggleSystemState(powerManager.weapons);
if (Input.GetKeyDown(KeyCode.Alpha3)) powerManager.ToggleSystemState(powerManager.sensors);
```

**Checking power efficiency (used before firing weapons and applying thrust):**
```csharp
float engineEfficiency = powerManager.GetSystemEfficiency("engines"); // 0.0 – 1.0
float weaponEfficiency = powerManager.GetSystemEfficiency("weapons");
```

**UI integration:** The UIController drives the power UI via its own `ChargeBtn(int)` and `VentBtn()` methods (called from UI buttons), which animate the power meter shaders and node chain visuals. The `PowerManager` is the authoritative data source; `UIController` reflects its state visually.

- When multiple systems are in `Draw`, the fill rate is shared equally.
- If the Reactor subsystem is damaged, `effectiveMaxReactor` is reduced proportionally.
- The game starts with 3 bars of Engine power pre-loaded.

**Key methods:**
| Method | Description |
|---|---|
| `ToggleSystemState(system)` | Advances the state cycle for the given system. |
| `GetSystemEfficiency(name)` | Returns 0.0–1.0 for `"engines"`, `"weapons"`, or `"sensors"`. |
| `ToggleEngines()` / `ToggleWeapons()` / `ToggleSensors()` | Convenience wrappers for UI buttons. |
| `EmergencyVent()` | Instantly vents all systems to Standby. |

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

A simple AI ship that detects, pursues, orbits, and fires at the player.

**AI behaviour (runs in FixedUpdate):**

| Condition | Action |
|---|---|
| Player outside detection radius | Idle |
| Player detected, too far (> 1.1× optimal distance) | Thrust toward player |
| Player detected, too close (< 0.5× optimal distance) | Thrust away from player |
| Player at optimal distance | Apply perpendicular force to orbit the player |

**AI movement logic:**
```csharp
if (distanceToPlayer > optimalCombatDistance * 1.1f)
    rb.AddForce(transform.up * thrustForce, ForceMode.Force);       // Approach
else if (distanceToPlayer < optimalCombatDistance * 0.5f)
    rb.AddForce(-transform.up * thrustForce, ForceMode.Force);      // Retreat
else
{
    Vector3 orbitDir = Vector3.Cross(directionToPlayer, Vector3.forward).normalized;
    rb.AddForce(orbitDir * orbitSpeed, ForceMode.Force);            // Orbit
}
```

**Firing condition:**
```csharp
bool inCombatRange = distanceToPlayer <= detectionRadius;
bool facingPlayer  = Vector3.Dot(transform.up, directionToPlayer) > 0.7f; // ~45°
if (inCombatRange && facingPlayer && Time.time - lastFireTime > fireRate)
    FireLaser();
```

**When destroyed:** `Explode()` spawns a particle system completely in code (no prefab required) and destroys the GameObject. The particle explosion destroys itself after 2 seconds.

Implements `IDamageable`. When health reaches 0, `Explode()` is called.

**Inspector settings:**
| Field | Default | Description |
|---|---|---|
| `detectionRadius` | 30 | Range at which the enemy notices the player |
| `optimalCombatDistance` | 15 | Preferred engagement range |
| `fireRate` | 0.5 s | Minimum time between shots |
| `maxHealth` | 3 | Hit points |
| `thrustForce` | 8 | Engine force applied each frame |
| `maxSpeed` | 15 | Speed cap |

---

## Weapons System

### WeaponBase

**File:** `Assets/Scripts/Weapons/WeaponBase.cs`

Abstract base class every weapon inherits from.

```csharp
public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float range;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected int   maxAmmo;
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float damageModifier = 1f;

    protected int   currentAmmo;
    protected bool  isReloading;
    protected float lastFireTime;

    public virtual bool CanFire()
        => !isReloading && currentAmmo > 0 && Time.time - lastFireTime >= reloadTime;

    public abstract void Fire(Vector3 target);
}
```

`CanFire()` returns true when: not reloading, ammo > 0, and `reloadTime` has elapsed. Subclasses may add extra conditions (e.g. lock achieved, spin-up complete).

---

### WeaponManager

**File:** `Assets/Scripts/Weapons/WeaponManager.cs`

Manages a list of `WeaponSlot`s and exposes a unified firing interface to `PlayerShip`.

**`WeaponSlot`** links a `WeaponType` enum, a `WeaponBase` instance, an active flag, an index, and a display name.

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

**`Missile`** is a homing projectile. It steers toward its assigned target each fixed frame using `Quaternion.RotateTowards` and applies continuous forward thrust.

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

// Fire all tubes that have achieved lock:
missileLauncher.Fire(Vector3.zero); // target parameter unused for lock-based launcher
```

- Each tube holds one missile loaded from the ammo pool.
- Lock requires `lockOnTime` seconds of continuous tracking within valid range.
- After firing, tubes auto-reload after `reloadTime` seconds.

---

### Railgun

**File:** `Assets/Scripts/Weapons/Railgun.cs`

A high-damage, slow-firing weapon that fires an **instant beam** capable of penetrating multiple targets.

**Firing sequence:**
1. `Fire()` starts a charge coroutine (default 2 seconds).
2. After charging, `FireRailgun()` casts sequential raycasts, piercing objects.
3. Each hit object takes damage reduced by 15% per prior penetration.
4. Penetration stops at `maxPenetrations` (default 5) or when the beam is blocked.

**Penetrating raycast loop:**
```csharp
while (remainingRange > 0 && penetrationCount <= maxPenetrations)
{
    if (Physics.Raycast(currentOrigin, fireDirection, out RaycastHit hit, remainingRange))
    {
        ApplyDamage(hit, currentDamage);
        currentDamage *= (1f - damageDropoffPerPenetration); // 15% less per hit

        if (ShouldStopBeam(hit)) break;  // Non-asteroid solids stop the beam

        currentOrigin = hit.point + fireDirection * 0.1f; // Step past the hit object
        remainingRange -= hit.distance;
        penetrationCount++;
    }
    else break; // Nothing else in range
}
```

**Visual beam:** A `LineRenderer` is enabled briefly (`beamDuration` seconds) after firing.

**UIController — weapon-screen visual:**
```csharp
// Triggers the animated railgun beam shader on the weapon screen
UIController.Instance.RailFire();
```
The `RailFireCo` coroutine in `UIController` animates `_LaserFire`, `_LaserSize`, `_LaserStart`, and `_LaserEnd` shader properties with an `AnimationCurve` for the charge-up and fade effects.

**`FireInstant()`** skips the charge phase (for AI or testing use).

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
