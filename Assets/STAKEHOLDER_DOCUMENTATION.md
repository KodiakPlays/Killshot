# Starwolf — Game Overview & Stakeholder Documentation

> **Last updated:** April 2026
> **Engine:** Unity (2D top-down space combat)

---

## Table of Contents

1. [Game Summary](#game-summary)
2. [Core Gameplay Loop](#core-gameplay-loop)
3. [Player Ship Systems](#player-ship-systems)
   - [Movement & Maneuvering](#movement--maneuvering)
   - [Power Management](#power-management)
   - [Hull & Damage](#hull--damage)
   - [Internal Subsystems](#internal-subsystems)
   - [Shields](#shields)
   - [Stability](#stability)
   - [Autopilot](#autopilot)
4. [Weapons Arsenal](#weapons-arsenal)
5. [Enemy Ships](#enemy-ships)
6. [HUD & User Interface](#hud--user-interface)
7. [Radar & Scanning](#radar--scanning)
8. [Communications & Signal Interception](#communications--signal-interception)
9. [Mission Structure](#mission-structure)
10. [World Rules & Boundaries](#world-rules--boundaries)
11. [Environment](#environment)
12. [Controls Reference](#controls-reference)

---

## Game Summary

Starwolf is a top-down 2D space combat game where the player commands a warship through combat missions in deep space. The game emphasises tactical resource management — the player must balance power between engines, weapons, sensors, and support systems while engaging hostile ships, navigating hazards, and completing mission objectives.

The game draws on capital-ship combat themes: directional armour, reactor power allocation, subsystem damage, signal interception, and multi-weapon loadouts. Combat is deliberate rather than twitchy — positioning, power decisions, and weapon selection matter more than reflexes alone.

---

## Core Gameplay Loop

1. **Mission Start** — The player's ship is deployed into a sector with a primary objective (e.g., locate and destroy a target).
2. **Locate** — Using radar and sensors, the player searches for the target within a large playable area.
3. **Engage** — Once found, the player manoeuvres into weapons range and fights the target (and any escorts).
4. **Eliminate** — The target must be destroyed to advance.
5. **Extract** — After the target is down, a hyperspace jump point appears at the edge of the sector. The player must reach it to complete the mission.

The mission can **fail** if:
- The player's ship is destroyed.
- The player leaves the operational area and deserts (30-second warning countdown).
- The mission timer runs out.
- Life support systems are knocked out and the countdown expires.

---

## Player Ship Systems

### Movement & Maneuvering

The ship accelerates and decelerates gradually — there is no instant speed change. The player controls thrust (forward/reverse), turning, and emergency lateral dodges. The ship holds its current speed when no thrust input is given; there is no auto-deceleration.

**Hyperspeed:** When engines are powered above 80% efficiency, the ship can engage a high-speed mode that multiplies speed by 4× at the cost of rapid engine power drain.

**Drift:** Turning at high speed causes lateral drift. The ship naturally compensates over time, but sharp manoeuvres at speed can make the ship slide sideways temporarily, draining stability.

### Power Management

The ship's reactor is a **shared energy pool** that feeds five systems:

| System | What it powers |
|---|---|
| **Engines** | Ship speed, acceleration, and maneuvering |
| **Arms** | Weapons — required to fire |
| **Bay** | Boarding pods and auxiliary craft |
| **Support** | Ability cooldowns and repair functions |
| **Sig** | Sensors, scanner accuracy, and comms interception |

Each system can hold up to 5 bars of power. The reactor passively regenerates and distributes energy to systems that are set to "Draw." The player toggles each system between three states:

- **Standby** — System holds its current power level.
- **Draw** — System actively pulls power from the reactor.
- **Vent** — System dumps its power back into the reactor.

When multiple systems draw simultaneously, the reactor output is split evenly between them. This creates a core strategic tension: the player cannot max out every system at once and must prioritise based on the situation.

**Bonus effects** scale with power invested beyond the minimum:

| System | Bonus per extra bar |
|---|---|
| Engines | +10% acceleration, −5% stability decay; full power unlocks supercruise |
| Arms | −2.5% weapon load time |
| Bay | +10% boarding pod range |
| Support | −5% ability cooldown |
| Sig | +1% scanner accuracy, +1 second comms intercept window |

### Hull & Damage

The ship's hull is divided into **four directional quadrants**, each with its own health pool:

| Quadrant | Direction | Armour |
|---|---|---|
| **Prow** | Front | Medium (40 HP) |
| **Port** | Left | Heavy (50 HP) |
| **Starboard** | Right | Heavy (50 HP) |
| **Aft** | Rear | Light (10 HP) |

All damage is **directional** — a hit from the left damages the Port quadrant, a hit from behind damages the Aft. This means ship orientation during combat is critical: exposing the weak rear armour to the enemy is dangerous, while presenting a broadside absorbs more punishment.

When a quadrant is breached (reduced to 0 HP), further hits to that side damage the ship's **internal subsystems** instead.

When all four quadrants are breached, the ship is destroyed.

### Internal Subsystems

Seven internal components represent the ship's critical infrastructure. They only take damage after the hull quadrant protecting them has been breached. Each subsystem is tied to a specific hull side:

| Subsystem | Protected by | Effect when destroyed |
|---|---|---|
| **Bridge** | Prow | Sluggish controls, loss of precise steering |
| **Sensors** | Prow | Scanner disabled, radar range reduced |
| **Reactor** | Aft | Limits total available power; randomly drains active systems |
| **Engines** | Aft | Reduced speed, slower turning |
| **Magazine** | Starboard | Weapons damaged or disabled |
| **Life Support** | Port | Starts a 2-minute game-over countdown |
| **Crew** | Port | Special abilities disabled |

This creates escalating consequences: early hull damage is manageable, but once armour is breached, every additional hit risks crippling a vital system. Life Support destruction is especially dangerous — if the player cannot complete the mission within 2 minutes, the crew is lost.

### Shields

An energy shield absorbs incoming damage before it reaches the hull. The shield:
- Flashes on hit to give visual feedback.
- Blinks rapidly when critically low (< 30 HP).
- Recharges automatically after 3 seconds without taking damage.
- Changes colour from red (low) to cyan (full) to indicate health at a glance.

Once the shield is fully depleted, all damage passes directly to the hull.

### Stability

Stability is a maneuvering resource (0–100) that is consumed by sharp turns and emergency dodges, and recovers over time.

- **Gentle turns** drain very little stability.
- **Hard turns** drain stability quickly, especially at high speed.
- **Emergency dodges** cost a flat 45 stability points with a 0.5-second cooldown.
- When stability is critically low (below 10%), recovery slows to 30% of normal.
- At zero stability, the ship cannot dodge or make sharp turns.

The HUD stability bar changes colour as a warning:
- **White** — Healthy (above 25%)
- **Amber** — Caution (10%–25%)
- **Red** — Critical (below 10%)

### Autopilot

The ship features two autopilot modes to reduce tedium during long transits:

- **Navigation Mode** — The ship automatically turns toward and flies to a selected map coordinate. It does not auto-brake — the player must slow down manually on arrival.
- **Combat Mode** — The ship maintains a chosen engagement range and orientation relative to a target (e.g., present the port broadside at 10,000 units distance).

Autopilot **disengages instantly** the moment the player touches any movement key, ensuring the player always has immediate manual control.

---

## Weapons Arsenal

The ship carries a diverse loadout of seven weapon types, each suited to different tactical situations:

| Weapon | Type | Summary |
|---|---|---|
| **Laser** | Energy beam | Fast-firing energy weapon. Requires at least 20% Arms power. The bread-and-butter weapon for sustained engagements. |
| **Macrocannon** | Heavy kinetic | Multi-barrel gun requiring manual arming before each salvo. Damage increases with distance. Rewards patient, long-range gunnery. |
| **Missile** | Guided explosive | Homing projectile with lock-on requirement. Area-of-effect damage with distance falloff on detonation. Multiple independent launch tubes. |
| **Railgun** | Superweapon | A devastating penetrating beam that kills in one hit and pierces through multiple targets. Costs **all ship power** to fire and leaves the ship in a 4-second reboot standby with no controls. High risk, high reward. |
| **Broadside Cannon** | Dual kinetic | Fires shells from both port and starboard simultaneously at a nearby auto-targeted enemy. Requires holding fire to build a targeting lock. |
| **Point Defense** | Defensive | Rapid-fire interceptor designed to shoot down incoming missiles and threats. Requires a spin-up period before firing. |
| **Boarding Pod** | Special | Launches a crew pod that physically attaches to an enemy ship. After a boarding timer, it deals damage. Non-traditional but effective against tough targets. |

The player switches between weapons freely and fires with a single button. Each weapon has its own ammo pool, reload timer, and unique firing mechanics.

---

## Enemy Ships

Enemies come in five distinct types, each presenting a different tactical challenge:

| Type | Threat Profile |
|---|---|
| **Patrol** | Balanced all-rounder. Medium health, speed, and firepower. The standard encounter. |
| **Stationary Defender** | A fixed turret with high health and very rapid fire. Cannot move but has long detection range. Controls space denial. |
| **Tank** | Slow and heavily armoured with powerful but slow weapons. Punishes sustained close-range fights. |
| **Interceptor** | Extremely fast and agile with rapid fire, but fragile. Dangerous in packs, easy to kill individually. |
| **Sniper** | Long detection and combat range with moderate firepower. Engages before the player can see them on short-range radar. |

**AI Behaviour:** Enemy ships operate on a three-state system:
1. **Patrol** — Roaming between random waypoints within their assigned area.
2. **Chase** — Pursuing the player once detected.
3. **Combat** — Maneuvering to maintain optimal distance while firing. Ships strafe around the player rather than flying straight at them.

Enemies use obstacle avoidance to navigate around asteroids and other hazards.

**Spawning:** Enemies are configured in groups with settings for count, patrol behaviour, and whether they respawn after destruction (with configurable delay). A win condition can be set to trigger when all enemies in the sector are eliminated.

---

## HUD & User Interface

The game features a rich, shader-driven HUD that presents ship data across multiple display panels:

| HUD Element | Information Displayed |
|---|---|
| **Compass** | Ship heading, rotates with the ship |
| **Speedometer** | Current ship speed |
| **Stability Bar** | Remaining maneuver stability with colour-coded warnings |
| **Power Bars** | Five system power levels + reactor charge, with charge animations |
| **Radar** | Nearby contacts with range rings and directional blips |
| **Weapon Display** | Active weapon name, type, ammo count, reload progress, and status |
| **Scanner / Sensor Panel** | Target ship model, hull integrity per quadrant, and classification |
| **Comms Panel** | Signal interception interface with frequency tuner and message log |
| **World Grid** | Zoomed-out strategic map showing ship position at three zoom levels |
| **Bogie Tabs** | List of tracked enemy ships |
| **Weapon Screen** | Visual combat display showing laser beams, railgun shots, and other weapon effects in real-time |

**Screen effects:**
- **Screen shake** on taking damage, with intensity driven by an animation curve.
- **Static glitch** effect on UI screens during damage events or system failures.

**World Grid Zoom Levels:**

| Level | Scale | View |
|---|---|---|
| Tactical | 10× | Immediate surroundings |
| Sector | 100× | Mid-range area |
| Strategic | 1000× | Full operational theatre |

The UI is designed as a "push" system — game systems send updates to the display rather than the UI constantly polling for changes. This keeps the interface responsive and in sync with gameplay.

---

## Radar & Scanning

**Radar** displays all nearby contacts as blips on a circular display. The player can cycle through five range settings: 500 / 1,000 / 2,000 / 4,000 / 8,000 units. Contacts beyond the current range are hidden.

Blips are positioned relative to the player's ship — they rotate with the player's heading so "up" on the radar always means "ahead." Each blip shows a distinct icon and colour for identification.

**Scanning** allows the player to inspect a selected contact in detail. The scanner uses a sweep mechanic with hit zones ("pips") — successful scans reveal the target's:
- 3D ship model
- Hull integrity per quadrant (Port, Aft, Prow, Starboard)
- Ship classification and size
- Location indicator

The Sig power system improves scanner accuracy, giving the player a wider perfect-hit window at higher power levels.

---

## Communications & Signal Interception

The comms system presents a **signal interception mini-game** when a transmission is detected:

1. The game pauses and the comms panel opens.
2. The player selects the correct frequency band (1, 2, or 3).
3. Using a tuning dial, the player adjusts frequency to match the signal.
4. A signal strength indicator changes colour from red (far) to green (close) as the player approaches the correct frequency.
5. An audio tone rises in pitch as the player gets closer.
6. When the correct band and frequency (within ±1.0) are matched, the message is intercepted and logged.

Intercepted messages provide narrative context, intel on enemy movements, or mission-relevant information. The Sig power system extends the interception time window, giving the player more margin for error.

---

## Mission Structure

Missions follow a linear progression through defined stages:

```
Not Started → Locate Target → Eliminate Target → Reach Jump Point → Mission Complete
```

| Stage | Objective |
|---|---|
| **Locate Target** | Find the mission target using radar/sensors. Auto-advances when the player gets within detection range, or can be manually confirmed via scanner. |
| **Eliminate Target** | Destroy the designated enemy ship. |
| **Reach Jump Point** | After the target is destroyed, a hyperspace jump point appears at a random edge of the operational area. The player must fly to it. |
| **Mission Complete** | Player reaches the jump point — mission success. |

**Failure conditions** are wired automatically:
- Ship destroyed → Mission Failed
- Desertion (leaving the play area) → Mission Failed
- Timer expired → Mission Failed
- Life support destroyed and countdown expires → Mission Failed

The game clock runs with a configurable time scale and supports strategic pausing (Escape key), which freezes all game systems for the player to assess the situation.

---

## World Rules & Boundaries

The playable area is a large circular zone centred on the world origin. If the player strays too far:

1. **Warning zone** — An alert is displayed as the player nears the edge.
2. **Out of bounds** — A 30-second desertion countdown begins.
3. **Return** — If the player returns in time, the countdown resets.
4. **Desertion** — If the countdown expires, the player's ship is destroyed and the mission fails.

This keeps the gameplay focused within the operational theatre while giving the player fair warning and a chance to correct course.

---

## Environment

The game world contains **destructible asteroids** that serve as both navigational hazards and cover:

- Asteroids have their own health pools and can be destroyed by weapons fire.
- Destroyed asteroids break apart into physics-driven debris.
- A visual damage state shows wear before destruction.
- The Railgun's beam penetrates through asteroids, while other solid objects stop it.

Enemy ships use obstacle avoidance to navigate around asteroids, and the player can use asteroid fields as tactical cover during engagements.

---

## Controls Reference

| Key | Action |
|---|---|
| **W** | Accelerate forward |
| **S** | Decelerate / reverse |
| **A** / **D** | Turn left / right |
| **Q** | Emergency dodge left |
| **E** | Emergency dodge right |
| **Space** / **Left Ctrl** | Fire active weapon |
| **Hold Space** (Railgun selected) | Charge railgun; release to fire |
| **1** | Toggle Engine power state |
| **2** | Toggle Arms power state |
| **3** | Toggle Sig (sensor) power state |
| **R** | Cycle radar range |
| **]** | Cycle to next scan target |
| **Left/Right Arrow** | Tune comms frequency |
| **1/2/3** (during comms) | Select frequency band |
| **Escape** | Strategic pause |
