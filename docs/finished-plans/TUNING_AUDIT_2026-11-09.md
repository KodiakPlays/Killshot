# Tuning audit: hard-coded game numbers, and recent number changes

Read-only check, 2026-11-09. Nothing in the project was changed.

**How to read this.** "In Tuning" means you can change the number in Starwolf → Tuning:
- the Ship and Enemy tabs (each ship's stats file in `Assets/Ships`),
- the Board and Keys tabs (fields marked `[Tune]`),
- the Hard Points and Ranges tabs.

"Level window" means Starwolf → Level shows it. "Inspector only" means it is a field you can only change in Unity's Inspector. "Code" means it is typed straight into a script, so only a code edit changes it.

Line numbers are from the files on disk today. `EnemyBrain.cs` and `TacticalWorld.cs` have uncommitted edits from another session, so their line numbers include those edits.

---

## A. Game numbers that are not in Tuning

### A0. The lock limit of 9,412 units (the lead you gave me)

Confirmed. It comes from three numbers working together:

| Where | Value | In Tuning? |
|---|---|---|
| `Assets/Scripts/Systems/ShipState.cs:321` (`SetZoom`) | periscope zoom stops at **8** (and starts at 1) | **No, code** |
| `ShipState.cs:90` `scopeFitUnits` | 1000 in the board scene (code default 3750) | Yes, Board tab |
| `ShipState.cs:93` `scopeFitSlack` | 0.15 | Yes, Board tab |

A lock needs the ship to fill the sight's brackets to at least 85% (`ShipState.cs:297-299`). At zoom 8 that works out as 8 × 1000 ÷ 0.85 = 9,412 units. Past that, no lock can start.

The 8 is typed in three places: `ShipState.cs:321` (the limit), `UI/Panels/PeriscopePanel.cs:113` (shows "TOO FAR" at zoom 8), and `PeriscopePanel.cs:134` (the MAG dial is scaled for 1 to 8). To make the limit tunable, all three need to read one number.

A lock also needs the periscope within **4°** of the ship. That 4 is typed into `ShipState.cs:290` (and `:293`). It is a different number from `onBearingDeg` (also 4), which is in Tuning. Changing the Tuning one does not change the lock.

### A1. Top 10 to move into Tuning

| # | File:line | Value | What it does | Want to tune? |
|---|---|---|---|---|
| 1 | `Systems/ShipState.cs:321` (+ `PeriscopePanel.cs:113`, `:134`) | zoom max 8 | Caps how far away a lock can start (9,412 units today). | High |
| 2 | `Systems/DamageSystem.cs:177` `BehindSide` | `{ "mag", "hgr", "eng", "rct" }` | Which room is behind PROW, STBD, AFT and PORT. Same for every ship. | High |
| 3 | `Systems/DamageSystem.cs:78-86` `compartments` | six rooms, each integrity 5 | The rooms themselves and their starting integrity. A field on the DamageSystem part, so Inspector only, not in the stats file. | High |
| 4 | Max room integrity **5**, typed in about 12 places: `DamageSystem.cs:61, 106, 130, 133, 164, 362`; `NavSystem.cs:140`; `PowerSystem.cs:58`; `Game/SaveGame.cs:104, 126`; `EngineeringPanel.cs:143`; `PowerPanel.cs:79` | 5 | The top of every room's health bar. Changing it means editing all of these. | High |
| 5 | `Systems/DamageSystem.cs:103-105` | 3 | A room works at 3 or more: MAG to load and fire, HGR to launch, BRI to use TRACK. | High |
| 6 | `Systems/WeaponSystem.cs:219-224` `systems[].shot` | RAILGUN 70, PDC 25, TORPEDO 40 | Capacitor charge each shot costs. A field on the WeaponSystem part, Inspector only. | High |
| 7 | `Systems/WeaponSystem.cs:308` `WepReady` | 2 | Weapons need at least 2 units on the WEP bus. | High |
| 8 | `Systems/PowerSystem.cs:109` | 1 unit per game second | How fast a bus fills or empties. | High |
| 9 | `Tactical/EnemyBrain.cs:298` | 0.8 | The enemy's speed while SEARCHING, as a share of top speed. | High |
| 10 | `Systems/HangarSystem.cs:113-118` `stages[].rate` | CREW 22, FUEL 18, ORD 14, EVAC 20, DOORS 26, CAT 12 (% per game second) | How long each launch step takes (about 34 s in all). A field on the Hangar part, Inspector only. | High |

### A2. Everything found, by system (high first)

**Damage**

| File:line | Value | What it does | Want to tune? |
|---|---|---|---|
| `DamageSystem.cs:177` | `{mag, hgr, eng, rct}` | Room behind each side (see A1). | High |
| `DamageSystem.cs:78-86` | 6 rooms, integrity 5 | The rooms (see A1). Inspector only. | High |
| `DamageSystem.cs:103-105` | 3 | A room works at 3+ (see A1). | High |
| `DamageSystem.cs:243-248` `Neighbours` | deck plan | Which rooms touch, so where fire, vacuum and radiation can spread. | High |
| `DamageSystem.cs:108` `RepairRate` | 2 at CONDITION 1, else 1 | Integrity a repair team gives back. | High |
| `DamageSystem.cs:164` | +1 | What REBOOT gives a room. | Low |
| `DamageSystem.cs:203` | side 2 (AFT) | A fighter DISABLE run always hits the stern. | Low |
| `DamageSystem.cs:233`, `:305` | 0 | A reactor at 0 destroys the ship (an owner rule). | Low |

**Weapons**

| File:line | Value | What it does | Want to tune? |
|---|---|---|---|
| `WeaponSystem.cs:219-224` | 70 / 25 / 40 | Charge per shot (see A1). Inspector only. | High |
| `WeaponSystem.cs:308` | 2 | WEP units needed to fire (see A1). | High |
| `WeaponSystem.cs:834` `MotorLinedUpDeg` | 3° | A torpedo lights its motor once within 3° of its target line. | High |
| `WeaponSystem.cs:965` | 100 | Capacitor maximum. | Low |
| `WeaponSystem.cs:503` | 100 units | One click of the RNG knob. | Low |
| `WeaponSystem.cs:504` | 1° | One click of the BRG knob. | Low |
| `WeaponSystem.cs:384` | 1 to 8 | Salvo size limits. | Low |
| `WeaponSystem.cs:767` | 25 × 25 box | Hit box for a track with no ship behind it. | Low |
| `WeaponSystem.cs:712` `TorpedoStepSeconds` | 0.05 | How finely a torpedo's flight is worked out. | Low |
| `UI/Panels/FireControlPanel.cs:240` | "+5/S" | The capacitor text uses a typed 5, not `wepFeedPerUnit`. If you tune the feed in the stats file, this text will be wrong. | High (bug risk) |
| `FireControlPanel.cs:118` | uses `hangar.leverFireAt` (0.9) | The fire lever borrows the hangar lever's 0.9 trigger point. Inspector only. | Low |

**Power and orders**

| File:line | Value | What it does | Want to tune? |
|---|---|---|---|
| `PowerSystem.cs:109` | 1 unit/s | Bus fill rate (see A1). | High |
| `PowerSystem.cs:58` | 2 | The reactor still gives at least 2 units when wrecked. | High |
| `OrdersSystem.cs:91` | 100 | AUX order pool maximum. | Low |
| `OrdersSystem.cs:96-97` | alert ≥ 1 | Weapons and flight ops are free from CONDITION 2 up. | Low |

**Movement and helm**

| File:line | Value | What it does | Want to tune? |
|---|---|---|---|
| `NavSystem.cs:162` | FLANK +0.5 G, EVASIVE +1.2 G | G-load added by the approach mode. | Low |
| `NavSystem.cs:164` | 45 | Stability lost per G over 1. | Low |
| `NavSystem.cs:33` `approachRanges` | 500, 1000, 1500, 2000, 3000, 5000 | Autopilot range steps. Inspector only. | Low (autopilot is set aside) |
| `NavSystem.cs:148` | 1°/s | The slowest the bow can ever turn. | Low |
| `NavSystem.cs:178` | 179° | The furthest A or D can order the bow ahead. | Low |
| `UI/Panels/HelmPanel.cs:155` | 0.6° | A held A or D orders the next step when the bow is this close. | Low |
| `HelmPanel.cs:176` | 1 + 99 × t² | How a held W or S speeds up. The hold time is in Tuning; the curve is not. | Low |

**Sensors and periscope**

| File:line | Value | What it does | Want to tune? |
|---|---|---|---|
| `ShipState.cs:321` | zoom 1 to 8, 0.1 steps | Periscope zoom (see A0). | High |
| `ShipState.cs:290`, `:293` | 4° | Periscope must be this close to a ship to lock (see A0). | High |
| `SensorSystem.cs:104` (+ `SensorsPanel.cs:137`, `:232`) | 1 to 5 | DRADIS gain steps. | Low |
| `SensorSystem.cs:26` `coneArc` | 40 | Scan cone half-width at load. Inspector only. | Low |
| `TacticalWorld.cs:568` | THERM and RAD | Every ship that joins during play (reinforcements, the battle group) gives off these two bands, so DRADIS hears it only on THERM or RAD. | High |
| `ShipState.cs:56-63` `tracks[].sig` | e.g. Garuda THERM, RAD | The bands each starting ship gives off. Inspector only, on the board. | High |
| `TacticalWorld.cs:304` | 0.5 units/s | No ROCK AHEAD warning below this speed. | Low |

**Comms**

| File:line | Value | What it does | Want to tune? |
|---|---|---|---|
| `CommsSystem.cs:22` `lockWindow` | 0.3 | How close you must tune to lock a message. Inspector only. | High |
| `CommsSystem.cs:66-68` | band 1–3, frequency 5.0–95.0, key letters 1–25 | Where a new message lands and how hard its code is. | High |
| `CommsSystem.cs:16`, `:23` | 4 dials | Code length. | Low |
| `CommsSystem.cs:49` | nearest 5, max 95 | How speed is said in a message. | Low |

**Hangar and boarding**

| File:line | Value | What it does | Want to tune? |
|---|---|---|---|
| `HangarSystem.cs:113-118` | 22/18/14/20/26/12 | Launch step speeds (see A1). Inspector only. | High |
| `HangarSystem.cs:94-107` `squads` | 6 fighters, 4 assault boats | Craft in each squadron. Fighters are cut down by the stats file's `fighters`; boats are not. Inspector only. | High |
| `HangarSystem.cs:335` | marines ÷ 2 | Defenders killed when a boarding fails. | Low |
| `HangarSystem.cs:331-332` | ENG and MAG to 0 | What a won DISABLE boarding does. | Low |
| `HangarSystem.cs:221` | 300 units | Closest an escort squadron flies to us. | Low |
| `HangarSystem.cs:161` | 50 units | Closest point where a squadron lands. | Low |
| `HangarSystem.cs:279` | 0.05 s | How finely a flight is worked out. | Low |

**Enemy**

| File:line | Value | What it does | Want to tune? |
|---|---|---|---|
| `EnemyBrain.cs:298` | 0.8 | SEARCHING speed share (see A1). | High |
| `EnemyBrain.cs:212` | 20 to 60 s | Heard again in this window counts as SEARCHING. | High |
| `EnemyBrain.cs:214` | 0.25 | How far off its guess of our position is, as a share of distance × (1 − how clearly it heard). | High |
| `EnemyBrain.cs:216` | 0.15 | Its guess error when it was only hit. | High |
| `EnemyBrain.cs:165` | warhead 0 (HE) | Enemy torpedoes are always HE. | High |
| `EnemyBrain.cs:403` | 0.85 | It hunts within 85% of the map. | Low |
| `EnemyBrain.cs:184` | half the message gap | When it changes course between messages. | Low |
| `EnemyBrain.cs:381` | 10,000 units | Reach it assumes with no railgun ammo. | Low |
| `TacticalWorld.cs:418` | up to 150°, 30° steps | How far it tries to turn to get round a rock. | Low |
| `Tactical/EnemySpawn.cs:42` | 1,000 units | A random spawn stays this far inside the edge. | Low |

**Mission, campaign and time**

| File:line | Value | What it does | Want to tune? |
|---|---|---|---|
| `Game/QuestSystem.cs:335` | 8,000 units | Reinforcements arrive at least this far from us. | High |
| `QuestSystem.cs:312` | 1 | Ships in each wave after the first. | High |
| `Game/SaveGame.cs:104`, `:126` | 1 to 5 | In the campaign a room comes back with at least 1 integrity. | Low |
| `Game/BoardManager.cs:153` `rates` | 0.25, 1, 5, 10 | Time speed buttons. Inspector only. | Low |
| `BoardManager.cs:166` `hitLineSeconds` | 4 s | How long "what hit us" stays on the alarm strip. Inspector only. | Low |
| `BoardManager.cs:136` `missionSeconds` | 7412 | The clock's start time. Inspector only. | Low |
| `ShipState.cs:346` | seed 49 | The dice give the same rolls every game. | Low |

### A3. Level numbers

**In the Level window (editable, listed apart as asked):** Battle Space width and height; Enemy Spawn box size, hunts, cruise speed, message gap, first message time, time on air, patrol route; Gas Cloud width and height; Debris Field shape, size, count, piece size, gap, clumps, seed; Player Spawn position.

**Not in the Level window:** the **Mission** part. The window only shows its jump point. So a drill's step numbers (`Amount`, ring position, ring size, and the new uncommitted `mapUnits`) and `weaponsOff` are Inspector only (`Game/Mission.cs:14-24`).

### A4. Stats files missing newer fields

`Garuda.asset` and `Frigate.asset` were last saved on 2026-11-02. They do not have six newer fields, so those ships run on the code default. You see these in Tuning, but the file has nothing saved for them until someone edits them:

| Field | Code default used |
|---|---|
| `hangar.fighters` | **6** (the Garuda and frigate "carry" 6 fighters; nothing launches them today) |
| `navigation.reverseShare` | 0.42 |
| `navigation.fineTurnDegPerS` | 2 |
| `navigation.sideTapUnits` | 2 |
| `navigation.sideRampSeconds` | 2 |
| `hangar.launchSideGapUnits` | 10 |

`Frigate.asset` also still has `bearingTrailSeconds: 120`, a field that was deleted in Step 161. It does nothing.

---

## B. Number changes since 2026-11-01

### B1. Stats files (all can be changed in Tuning)

There were 4 commits. They made **264 number changes** (hard point positions included). All of these fields are in Tuning.

#### f8216c9, 2026-11-01, Step 130: one clock and the designer's spacing table
Many fields were also renamed from "Km" to "Units" here. Old → new is shown across the rename.
- **Asura**
  - size: length 100 → 200; width new = 50
  - navigation: topSpeedUnits 12.5 → 50; accelUnits 0.2175 → 2; brakeUnits 0.4075 → 3.75; dragKeepPerS 0.997502 → 0.99406; driftKeepPerS 0.96307 → 0.9143; new: baseSpeedUnits 25, baseAccelUnits 1, baseTurnRate 10, sideSpeedUnits 20, sideAccelUnits 4
  - sensors: senRangeBaseUnits 12000 → 4375; senRangePerStepUnits 3600 → 625; visualRangeUnits 10000 → 2000; new: cloudInsideShare 0.9, cloudSignalShare 0.3, cloudSeenUnits 400
  - weapons: slugSpeedUnits 2500 → 300; missile speed 100 → 400; missileTurnDegPerS 6.5 → 120; manualStartRangeUnits 10000 → 2000; spdStep 0.25 → 1; hitRadius 100 removed, hitMarginPercent new = 25; new: gunTurnDegPerSecond 3, lockSeconds 10, lockFallSeconds 5, lockHoldSeconds 10
  - mounts: RGA arc 42 → 90, halfArc 80 → 45, range 25000 → 2000; RGB arc 250 → 270, halfArc 80 → 45, range 25000 → 2000; PDA arc 90 → 45, halfArc 90 → 135, range 10000 → 600; PDB arc 270 → 315, halfArc 90 → 135, range 10000 → 600; VLS range 35000 → 3000; new PDC (arc 225, halfArc 135, range 600, ammo 2400); new PDD (arc 135, halfArc 135, range 600, ammo 2400); new MSA (arc 270, halfArc 90, range 3000, ammo 8); new MSB (arc 90, halfArc 90, range 5000, ammo 8)
  - hard points new: MSB (25,50), RGA (25,0), RGB (−25,0), PDD (25,−100), PDC (−25,−100), PDA (25,100), PDB (−25,100), MSA (−25,50)
  - damage: new crashFullSpeed 50, crashNudgeSpeed 2
  - hangar: fighterSpeedUnits 40 → 100; boatSpeedUnits 30 → 75; boatRangeUnits 15000 → 4000
- **Garuda**
  - size: length 150 → 350; width new = 100
  - navigation: topSpeedUnits 8.75 → 35; accelUnits 0.152 → 1.4; brakeUnits 0.285 → 2.6; dragKeepPerS 0.997502 → 0.99406; driftKeepPerS 0.96307 → 0.9143; new: baseSpeedUnits 17.5, baseAccelUnits 0.7, baseTurnRate 7, sideSpeedUnits 14, sideAccelUnits 2.8
  - sensors: senRangeBaseUnits 10000 → 4375; senRangePerStepUnits 3600 → 625; visualRangeUnits 10000 → 2000; cloud fields new (0.9, 0.3, 400)
  - weapons: the same changes as Asura (slug 2500 → 300, missile 100 → 400, turn 6.5 → 120, manual start 10000 → 2000, spdStep 0.25 → 1, hit margin 25, lock 10/5/10, gun turn 3)
  - mounts: RGA and RGB arc 42 → 90 and 250 → 270, halfArc 80 → 45, range 18000 → 2000; PDA, PDB range 10000 → 600; VLS range 35000 → 3000
  - hard points new: empty (0,175), RGA (50,0), RGB (−50,0), PDA (50,−87.5), PDB (−50,−87.5), VLS (0,−175)
  - damage and hangar: the same as Asura
- **Frigate**
  - size: length 75 → 100; width new = 25
  - navigation: topSpeedUnits 15 → 60; accelUnits 0.3 → 2.8; brakeUnits 0.55 → 5.2; drag and drift the same as Asura; new: baseSpeedUnits 30, baseAccelUnits 1.4, baseTurnRate 15, sideSpeedUnits 24, sideAccelUnits 5.6
  - sensors and weapons: the same as Garuda
  - mounts: RGA range 18000 → 2000; RGB range 25000 → 2000; arcs as Garuda; PDA, PDB 10000 → 600; VLS 35000 → 3000
  - hard points new: empty (0,50), RGA (12.5,0), RGB (−12.5,0), PDA (12.5,−25), PDB (−12.5,−25), VLS (0,−50)
  - damage and hangar: the same as Asura

#### 1c19d9c, 2026-11-02, Steps 147, 149, 150: torpedoes and squadrons leave the side and curve; missile renamed torpedo
- **Asura, Garuda, Frigate (the same in all three):** torpedoSpeedUnits 400 → 250; torpedoSpreadDeg 3 → 20; torpedoTurnDegPerS stays 120; new: torpedoLaunchSpeedUnits 80, torpedoCoastSeconds 2.5, torpedoCoastSpeedUnits 15, torpedoTiltBackDeg 15, torpedoCurveDegPerS 32, torpedoTurnBuildSeconds 0.5, torpedoBurnUnits 300; hangar new: launchTiltBackDeg 15, launchRunUnits 200, turnDegPerS 90, turnEase 180
- **Asura only:** VLS launcher removed (was range 3000, ammo 8); MSA halfArc 90 → 180; MSB halfArc 90 → 180, range 5000 → 3000

#### fc356b7, 2026-11-03, Step 162: drawn launch lines
- **Asura:** new launchSideGapUnits 10; new reverseShare 0.42; bearingTrailSeconds removed (was 120); new hangar dots at (−25,−50) and (25,−50), each with a drawn line; drawn lines added to the two torpedo launchers (MSB and MSA)

#### a674637, 2026-11-09, Steps 170, 171: spinal railgun; tap-and-hold Q, E, W, S
- **Asura:** turrets RGA and RGB removed (each was range 2000, ammo 22); new SRG spinal railgun (arc 0, halfArc 0, range 30000, ammo 12) on a new hard point at (0,100); slugSpeedUnits 300 → 1500; new fighters 0; new fineTurnDegPerS 2, sideTapUnits 2, sideRampSeconds 2

#### Uncommitted
Three new stats files, not yet tracked by git: `Corvette.asset` (Marut-class, 120 × 80), `Carrier.asset` (Indra-class, 450 × 230), `Battleship.asset` (Varuna-class, 400 × 190). All their numbers past the size match `Frigate.asset`, plus the newer fields at their defaults (fighters 6). All are in Tuning.

### B2. Code changes to game numbers

"Tuning?" says whether the field shows in Tuning. A code default for a stats field only matters for a new file or a field the file is missing. The file's own value wins.

| Date | Commit (Step) | File | Field or rule | Old → new | Tuning? |
|---|---|---|---|---|---|
| 11-01 | f8216c9 (130) | NavSystem.cs | `approachRanges` | 2000…24000 → 500, 1000, 1500, 2000, 3000, 5000 | **No** (Inspector only) |
| 11-01 | f8216c9 (130) | NavSystem.cs | `baseSpeedUnits` | new, 2.5 | Yes (files: 25 / 17.5 / 30) |
| 11-01 | f8216c9 (130) | NavSystem.cs | throttle cap rule | ENG units × 20 → worked out from base and top speed | **No** (code) |
| 11-01 | f8216c9 (130) | BoardManager.cs | `zoomOutLimit` | 0.01 → 0.1 | Yes (the scene keeps 0.058) |
| 11-01 | f8216c9 (130) | BoardManager.cs | `gameSecondsAtX1` | new, 1 (tooltip says it was 2.38) | Yes |
| 11-01 | f8216c9 (130) | QuestSystem.cs:335 | reinforcement minimum distance | 25,000 → 8,000 | **No** (code) |
| 11-01 | f0a1d3a (125, 141) | WeaponSystem.cs | `lockSeconds`, `lockFallSeconds`, `lockHoldSeconds` | new: 10, 5, 10 | Yes |
| 11-01 | f0a1d3a (125, 141) | PeriscopePanel.cs:113 | "TOO FAR" at zoom 8 | new | **No** (code) |
| 11-01 | 158a08a (121-122) | WeaponSystem.cs | default RGA arc, halfArc; RGB arc, halfArc | 42 → 90, 80 → 45; 250 → 270, 80 → 45 | Yes |
| 11-01 | 158a08a (121-122) | WeaponSystem.cs | default `hardPoints`; `gunTurnDegPerSecond` | new; new, 3 | Yes |
| 11-01 | b4e281d (123-131) | DamageSystem.cs | `crashFullSpeed`, `crashNudgeSpeed` | new: 12.5, 0.5 | Yes (files: 50, 2) |
| 11-01 | b4e281d | ShipStats.cs | `width` | new, 64 | Yes |
| 11-01 | b4e281d | WeaponSystem.cs | `hitRadiusUnits` → `hitMarginPercent` | 100 units → 25% | Yes |
| 11-01 | b4e281d | EnemyBrain.cs | `avoidLookUnits` | new, 600 | Yes |
| 11-01 | b4e281d | TacticalWorld.cs | `crashKeepSpeed`, `crashRepeatSeconds`, `rockWarnSeconds`, `crashCheckUnits` | new: 0.3, 3, 30, 20 | Yes |
| 11-01 | b4e281d | WeaponSystem.cs:767 | hit box with no ship | new, 25 × 25 | **No** (code) |
| 11-01 | b4e281d | TacticalWorld.cs:418 | enemy detour search | new, up to 150° in 30° steps | **No** (code) |
| 11-01 | 4cdbf84 (129) | SensorSystem.cs | `cloudInsideShare`, `cloudSignalShare`, `cloudSeenUnits` | new: 0.9, 0.3, 400 | Yes |
| 11-01 | 4cdbf84 (129) | GasCloud.cs | `widthUnits`, `heightUnits` | new: 2000, 1200 | Level window |
| 11-01 | 06cb712 (132, 138) | PlotPanel.cs | `staleSeconds`, `staleLabelSeconds` | new: 60, 10 | Yes |
| 11-01 | 7e7b6c9 (137-142) | EnemySpawn.cs | `firstSignalSeconds`, `onAirSeconds` | new: 15, 60 | Level window |
| 11-01 | 7e7b6c9 | CommsSystem.cs:66-68 | new message band, frequency, key | new: 1–3, 5.0–95.0, 1–25 | **No** (code) |
| 11-01 | 7e7b6c9 | CommsSystem.cs:49 | speed in words | new: nearest 5, max 95 | **No** (code) |
| 11-01 | 536f0d2 (144) | NavSystem.cs | `baseTurnRate`, `baseAccelUnits`, `sideSpeedUnits`, `sideAccelUnits` | new: 10, 0.10875, 5, 1 | Yes |
| 11-02 | 0fdc52d (148, 151) | EnemyBrain.cs | `turnBackChance`, `giveUpSeconds`, `falseLookSeconds` | new: 0.25, 90, 120 | Yes |
| 11-02 | 0fdc52d | TacticalWorld.cs | `rockInterferenceUnits`, `rockCloseUnits`, `falseDotChance`, `falseDotSeconds` | new: 500, 500, 0.15, 8 | Yes |
| 11-02 | 1c19d9c (147-150) | WeaponSystem.cs | missile fields renamed to torpedo (speed 100, turn 6.5, spread 3); new `torpedoLaunchSpeedUnits` 80, `torpedoCoastSeconds` 1.5, `torpedoCoastSpeedUnits` 30, `torpedoTiltBackDeg` 15, `torpedoCurveDegPerS` 60, `torpedoTurnBuildSeconds` 0.5, `torpedoBurnUnits` 300 | as listed | Yes (files differ, see B3) |
| 11-02 | 1c19d9c | HangarSystem.cs | `launchTiltBackDeg`, `launchRunUnits`, `turnDegPerS`, `turnEase` | new: 15, 200, 90, 180 | Yes |
| 11-02 | 1c19d9c | WeaponSystem.cs | `TorpedoStepSeconds`, `MotorLinedUpDeg` | new: 0.05, 3 | **No** (code) |
| 11-02 | 1c19d9c | HangarSystem.cs:279 | flight step | new, 0.05 | **No** (code) |
| 11-02 | 1c19d9c | HangarSystem.cs `squads` | starting craft fuel and state | 96/88/41/0, 74/62, some REARM or DOWN → all 100 and READY | **No** (Inspector only) |
| 11-02 | f2f8cc2 (155) | NavSystem.cs:178 | A/D order limit | new, 179° ahead of the bow | **No** (code) |
| 11-02 | 02c1f6c (157) | NavSystem.cs | `reverseShare` | new, 0.42 | Yes |
| 11-02 | 02c1f6c | NavSystem.cs `throttle` | range | 0..100 → −100..100 | **No** (Inspector range) |
| 11-02 | 02c1f6c | EnemyBrain / TacticalWorld | `falseLookSeconds`, `falseDotChance`, `falseDotSeconds` | removed (false dots dropped) | Yes (were in Tuning) |
| 11-02 | 02c1f6c | PlotPanel.cs | `rockEchoSeconds` | new, 30 | Yes |
| 11-02 | ef5a747 (160) | HangarSystem.cs | `launchSideGapUnits` | new, 10 | Yes |
| 11-02 | bad1f3b (161) | SensorSystem.cs | `bearingTrailSeconds` | 120 → removed | Yes (was in Tuning) |
| 11-08 | ebe7a21 (164) | BoardManager.cs | `zoomInLimit` | new, 4 | Yes |
| 11-08 | ebe7a21 (164) | PlotPanel.cs | `closeUpFromZoom`, `closeUpFullZoom` | new: 1, 1.5 | Yes |
| 11-08 | 6454c54 (169) | SaveGame.cs:104, 126 | campaign room integrity | new: at least 1, at most 5 | **No** (code) |
| 11-09 | a674637 (170-171) | HangarSystem.cs | `fighters` | new, 6 | Yes (Asura file 0; Garuda and Frigate use 6) |
| 11-09 | a674637 | NavSystem.cs | `fineTurnDegPerS`, `sideTapUnits`, `sideRampSeconds` | new: 2, 2, 2 | Yes |
| 11-09 | a674637 | HelmPanel.cs | `throttleKeyStep` → `throttleHoldSeconds` | 10 removed → 2 s new | Yes |
| 11-09 | a674637 | HelmPanel.cs:176 | W/S hold curve | new, 1 + 99 × t² | **No** (code) |
| 11-09 | 6bc36a8 (175) | HelmPanel.cs:155 | held A/D re-order point | new, 0.6° | **No** (code) |

Changes that only affect looks are left out: grid steps and colours, wreck drift and spin, shake, plot ship size, shot widths and trails, nebula, cloud alpha, sounds, and the sector map. Commits 46bc410 (172), 86436cc (168), 2cc8cce (167), 5fd4bce (166), f9a71a4 (165), d795e97 (163), a6be37d (152), 6407be2 (154), ac6afa1, a98d9fb (133-134), dd5b538 (120-139) and 66a3155 (179) changed no game numbers. 66a3155 sizes the 3D model from the stats file's Length, which is in Tuning.

**Uncommitted (another session's work, not yet committed):**

| File | Field | Value | Tuning? |
|---|---|---|---|
| EnemyBrain.cs:85-96 | battle group: `groupSpeedUnits`, `swayUnits`, `swaySeconds`, `keepClearUnits`, `makeRoomShare`, `catchUpPerUnit`, `inPlaceUnits` (plus `slotUnits` and `escorts` per ship) | 40, 150, 60, 500, 0.3, 0.05, 100 | Yes ("Battle group") |
| TacticalWorld.cs:60 | `touchingUnits` | 50 | Yes ("Crashes") |
| QuestSystem.cs:208 | `mapGrowSeconds` | 3 | Yes ("Mission") |
| Mission.cs:22 | `DrillStep.mapUnits` | per step | **No** (Inspector only; not in the Level window) |
| EnemyBrain.cs (Escort) | sway shape | × 0.7 | **No** (code; looks only) |

### B3. Where the code default and the file disagree (the file wins)

Since Step 130 the stats files and the code defaults have drifted far apart. Play is not affected, because the files win. But **a brand-new stats file would start with the old numbers.**

| Field | Code default | Asura / Garuda / Frigate files |
|---|---|---|
| `topSpeedUnits` | 12.5 | 50 / 35 / 60 |
| `senRangeBaseUnits`, `senRangePerStepUnits` | 12000, 3600 | 4375, 625 |
| `visualRangeUnits` | 10000 | 2000 |
| `slugSpeedUnits` | 2500 | 1500 / 300 / 300 |
| mount ranges (RG / PD / VLS) | 25000 / 10000 / 35000 | 2000 / 600 / 3000 (Asura's spinal gun 30000) |
| `crashFullSpeed`, `crashNudgeSpeed` | 12.5, 0.5 | 50, 2 |
| `torpedoSpeedUnits`, `torpedoTurnDegPerS`, `torpedoSpreadDeg` | 100, 6.5, 3 | 250, 120, 20 |
| `torpedoCoastSeconds`, `torpedoCoastSpeedUnits`, `torpedoCurveDegPerS` | 1.5, 30, 60 | 2.5, 15, 32 |
| `baseSpeedUnits`, `baseAccelUnits`, `sideSpeedUnits`, `sideAccelUnits` | 2.5, 0.109, 5, 1 | about 10× the defaults |
| `manualStartRangeUnits`, `spdStep` | 10000, 0.25 | 2000, 1 |
| `boatRangeUnits`, `boatSpeedUnits`, `fighterSpeedUnits` | 15000, 30, 40 | 4000, 75, 100 |

The reverse also happens: Garuda and Frigate files are missing six fields, so the code default rules for them (see A4). Board-tab example: `zoomOutLimit` code default 0.1, scene value 0.058, and the scene wins.

### B4. Count

- **In Tuning:** 264 stats-file changes, 67 code changes, and 9 uncommitted code changes. That is **340** in all, plus 3 new stats files.
- **Through the Level window:** 4.
- **Not in Tuning:** **20** (19 committed, 1 uncommitted; the sway shape only affects looks, so it is not counted). These are code literals and Inspector-only fields: the 8,000 reinforcement distance, the zoom-8 "TOO FAR", comms randomness, the torpedo motor 3°, craft fuel, campaign room limits, helm key feel, and the others marked **No** above.

**The short answer to "is this all in Tuning?":** most recent number changes are in Tuning. About 20 are not. Older hard-coded rules (section A) are also not in Tuning, and the most important of them are the zoom-8 lock limit, the room behind each side, max room integrity, shot costs and launch step speeds.
