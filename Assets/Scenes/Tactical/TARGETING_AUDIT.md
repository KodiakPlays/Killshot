# Targeting audit — what already exists

> **Note.** The `web-prototype/` project was deleted. Its simulation core was kept and
> now lives in `docs/reference-sim/`. Paths below have been updated. The React UI is gone.


Read this before building the targeting computer or the contact plot.

## Summary

The Unity C# code has almost nothing you can reuse. It has one weapon at a time, a cooldown
timer, and a radar script that copies enemy transforms straight onto a UI circle. It has no
firing arcs, no per-turret arming, no track, no bearing, no uncertainty, and no way to turn a
click on a plot into a world position.

The mechanic the owner describes is **already written**, but in TypeScript, in `docs/reference-sim/`.
`docs/reference-sim/weapons.ts` has the turret arming state machine with arc centre and half
arc. `docs/reference-sim/radar.ts` has the full track model with bearing-only contacts,
two-bearing fixes, estimated heading and speed, age and uncertainty. `the deleted prototype's plot/render.ts`
has a plot camera with span and centre. This is the thing not to rebuild. Port it.

The older `References/starwolf-prototype.html` is behind the TypeScript one on every question
here, and its ARM control is the ship-wide toggle the owner has explicitly rejected.

One correction to the brief: `Assets/Scenes/Tactical/02_Grid.unity` contains **no** `BogieLoad`
and **no** `Canvas-BogieShipStats`. Those live in the old `Prototype` scenes. See section 4.

---

## 1. Weapon mounts

### The handoff asks for six things per mount

| Handoff field | In Unity C#? | Where |
|---|---|---|
| Arc centre | **No** | nothing anywhere |
| Half arc | **No** | one hardcoded dot-product test, see below |
| Max range | Yes | `WeaponBase.cs:6` `range` |
| Loaded count | Almost no | one `bool` on one weapon |
| Magazine count | Yes | `WeaponBase.cs:9` `maxAmmo`, `:14` `currentAmmo` |
| Payload list | **No** | nothing anywhere |

The spec already names these fields the way the handoff does —
`Assets/TACTICAL_BOARD_SPEC.md:740-742` writes the validity rule as
`track.range <= mount.range AND |angDiff(track.bearing, mount.arcCentre)| <= mount.halfArc`.
No C# type has `arcCentre` or `halfArc`.

Also note a count mismatch: the handoff says five mounts, `Assets/TACTICAL_BOARD_SPEC.md:645`
says three railgun turrets, and `docs/reference-sim/weapons.ts:81` builds three. Settle this
before writing the type.

### `WeaponBase` — `Assets/Scripts/Weapons/WeaponBase.cs`

| Line | Field | What it does |
|---|---|---|
| 6 | `range` | max range. Real, used. |
| 7 | `angleOfFire` | **declared and never read.** Only ever defaulted, at `LaserWeapon.cs:21` and `MissileLauncher.cs:22`. Nothing tests an angle against it. |
| 8 | `reloadTime` | doubles as the shot cooldown |
| 9 | `maxAmmo`, 14 `currentAmmo` | one magazine, no loaded/stowed split |
| 15 | `isReloading` | set nowhere except `Reload()` |
| 16 | `lastFireTime` | drives the cooldown |
| 23 | `CanFire()` | `!isReloading && currentAmmo > 0 && Time.time - lastFireTime >= reloadTime` |
| 34 | `GetReloadProgress()` | 0..1 from the cooldown clock |
| 47 | `abstract Fire(Vector3 target)` | takes a **world point**, not a bearing |

There is no ready flag, no arming, no aim bearing, no solution.

### `WeaponManager` — `Assets/Scripts/Weapons/WeaponManager.cs`

This is a weapon **switcher**, not a mount board. `:11` holds `List<WeaponSlot> weaponSlots`,
`:14` `activeWeaponIndex`, and `:76` `SwitchToWeapon(int)` makes exactly one weapon current.
`:151` `FireActiveWeapon()` fires that one. Five mounts acting at once does not fit this shape.

`WeaponSlot` (`Assets/Scripts/Weapons/WeaponType.cs:20`) carries `weaponType`, `weaponInstance`,
`isActive`, `slotIndex`, `slotName`. No arc, no bearing, no range, no state.

### The individual weapons

| Weapon | Arc | Bearing | Range | Ammo | Reload / arming state |
|---|---|---|---|---|---|
| `Railgun.cs` | none | none | `:15` `maxRange = 2000` | inherited | `:38` `isCharging`, `:39` `isOnStandby`, `:16` `chargeTime = 2`, `:21` `standbyDuration = 4` |
| `BroadsideCannon.cs` | hardcoded, `:175` `Mathf.Abs(forwardDot) > 0.707f` skips anything outside 45°–135° off the bow | port/starboard `Transform`s only, `:8`, `:9` | inherited `range`, used at `:157`, `:171` | inherited | `:15`,`:16` per-side `lastFireTime`; `:23-27` `portLockProgress`/`portLocked`/`starboardLockProgress`/`starboardLocked`; `:12` `lockTime = 2.0` |
| `LaserWeapon.cs` | none | none | `:20` default 500 | inherited | `:13` `isRecharging`, `:14` `rechargeTimer` |
| `BoardingPodLauncher.cs` | none | none | `:10` `maxLaunchRange = 5000`, `:11` `minLaunchRange = 200` | inherited | `:13` `isLoaded`, `:27` `LoadPod()` |
| `Weapons.cs` (superseded) | none | none | none | `:22` `maxAmmo`, `:23` `reloadTime` | none |

`BroadsideCannon` is the only weapon in the project that tests an angle at all, and that test is
a literal in the middle of a targeting loop, not a field. It cannot be reconfigured per mount.

`Railgun.cs:212` returns `CanFire() => false` and `:214` `Fire()` is empty — the railgun bypasses
`WeaponBase` entirely and self-fires from its own `Update()` on a hold-and-release trigger.

`Weapons.cs:10` is marked `[System.Obsolete]` and just adds a `LaserWeapon` at runtime
(`Weapons.cs:37-51`). Ignore it.

`BoardingPodLauncher` is dead weight for another reason: no boarding pods is a settled decision.

### Missing entirely

Arc centre. Half arc as data. A per-mount aim bearing. Loaded count separate from magazine.
A payload list. Any mount that can hold state while another mount also holds state.

---

## 2. Turret arming

### Unity: nothing

There is no arming and no per-turret ready state. The three nearest things are all a different
mechanic:

- `Railgun.cs:38` `isCharging` — hold a key, charge builds, release fires. Not a commitment you
  set and keep.
- `BroadsideCannon.cs:24` `portLocked` / `:27` `starboardLocked` — a lock-on that decays when the
  target leaves the arc (`:118-120`). Not a loaded gun.
- `BoardingPodLauncher.cs:13` `isLoaded` — a single bool, set once in `Start()`.

`WeaponBase.CanFire()` at `:23` is a cooldown timer. A cooldown is not a ready state: it says
"enough time has passed", not "this mount is holding a shot at this bearing".

### `References/starwolf-prototype.html`: has arming, but as the toggle the owner rejects

State object, `References/starwolf-prototype.html:794`:

```
const cannon = {
  barrels:[0,0,0], arming:false, armTimer:0,
  viewfinder:false, lockTimer:0, locked:false,
  projectiles:[],
  ammo: WEAPON_STATS.CANNON.ammo
};
```

| Thing | Where | Number |
|---|---|---|
| Barrels | `:794` `barrels:[0,0,0]` | 3, each `0` empty or `2` armed |
| Arming time | `:1699` `if(cannon.armTimer>=2)` | **2 seconds per barrel** |
| Power gate | `:1697` `if(cannon.arming&&wpn>0)` and `:1769` `if(power.systems['WEAPONS'].bars<1)return` | needs 1 WEAPONS bar |
| The control | `:1766` `toggleArm()`, button at `:4217` `id="wp-arm"` | one ship-wide toggle |
| Status text | `:3942` | `'LOADING'` / `'ARMED'` / `'STBY'` |

Read `:1697-1700`: while `arming` is true it finds the first empty barrel and fills it, one at a
time, then sets `arming = false` when all three are full. So it is a queue driven by one toggle.
The barrels do **not** hold their own bearing — the fire code at `:1755-1764` picks one
`fireAngle` for the ship. This is exactly the design the owner has ruled out.

### `docs/reference-sim/weapons.ts`: this **is** the owner's mechanic, already written

The file's own comment at `:6-12` states the rule: "Three turrets, one shot each. A turret holds
its OWN aim bearing. There is no volley button."

| Line | Name | What it is |
|---|---|---|
| 4 | `type TurretState = 'empty' \| 'arming' \| 'armed'` | the three states |
| 13 | `class Turret` | one mount |
| 16 | `arcCentre` | degrees from own nose, 0 dead ahead |
| 17 | `halfArc` | the half-arc |
| 19 | `state` | which of the three |
| 20 | `armProgress` | 0..1 |
| 21 | `aimBearing` | **per turret**, nullable |
| 22 | `solutionExpiry` | mission time the solution dies |
| 31 | `bears(rel)` | `Math.abs(angDiff(rel, this.arcCentre)) <= this.halfArc` |
| 36 | `arm(weaponsPoints)` | only from `empty`, only above the power floor |
| 45 | `disarm()` | instant and free |
| 52 | `step(dt, now, weaponsPoints)` | advances arming; **stalls without power, does not reset** (`:54`); clears a stale `aimBearing` at `:63` |
| 71 | `get ready` | `armed` **and** has a bearing |
| 73 | `fire(from)` | makes a `Slug`, then calls `disarm()` — **one shot per arming, enforced** |
| 81 | `makeTurrets()` | `FWD LEFT (0, 90)`, `FWD RIGHT (0, 90)`, `AFT (180, 90)` |

Numbers, all in `docs/reference-sim/tuning.ts`:

| Constant | Line | Value |
|---|---|---|
| `ARM_SECONDS` | `:52` | 4 (per turret, one at a time) |
| `MIN_ARM_POWER` | `:53` | 2 WEAPONS points |
| `SOLUTION_LIFETIME` | `:54` | 18 seconds |
| `SLUG_SPEED` | `:50` | 4800 |
| `RAILGUN_RANGE` | `:51` | 38000 |

The volley falls out of it rather than being a button: `docs/reference-sim/world.ts:123`
`fireReady()` loops every turret and fires whichever ones are ready. `world.ts:114` `solutionFor(target)`
returns the list of turrets that bear on a point, with bearing, range and flight time.

---

## 3. Contacts and tracks

### Unity: a blip mapper, not a sensor

`Assets/Scripts/Radar.cs` is 124 lines and does one job — draw UI dots.

| Line | Thing |
|---|---|
| 6 | `static Radar Instance` singleton |
| 12 | `rangeSteps = { 500, 1000, 2000, 4000, 8000 }` |
| 13 | `toggleKey = KeyCode.R` cycles the range |
| 16 | `List<RadarTarget> targets` |
| 49 | `UpdateBlips()` — runs every frame |
| 71 | `playerTransform.InverseTransformPoint(target.transform.position)` |
| 77 | beyond range, hide the blip |
| 108 | `RegisterTarget` / `116` `UnregisterTarget` |

Line 71 is the whole problem: it reads the target's true transform every frame. There is no
sensing, no staleness, no error. Everything on the radar is perfectly known at all times.
It also flattens to `(x, y)` at `:72` while the rest of the game is 3D.

`Assets/Scripts/RadarTarget.cs` is 18 lines: `icon`, `color`, `trackRotation`. That is the entire
contact model.

| Track property asked for | In Unity? |
|---|---|
| Bearing | No |
| Last-seen time | No |
| Estimated heading | No |
| Estimated speed | No |
| Uncertainty | No |
| Designator / allegiance / confidence | No |

`Assets/Scripts/Targeting/TargetingSystem.cs` is the other half: `:63` raycasts from screen
centre, `:100` locks whatever it hits, `:101` hands the transform to `WeaponManager.LoadActiveWeapon`.
Instant, perfect, one target, aimed by pointing the ship. `Targetable.cs` is a marker with an
optional `targetPoint`.

### `References/starwolf-prototype.html`: a list rebuilt from ground truth

`getScanContacts()` at `:3467` (map version) and `:4136` (panel version) builds a fresh array
each call:

```
contacts.push({id:e.id, label:e.type, type:'WARSHIP', x:e.x, y:e.y, radius:e.radius, scanned:false, targetRef:e})
```

Visibility is gated by distance only — `:683` `SONAR_RADIUS = 8000`, `:684` `RADAR_RADIUS = 40000`,
`:768` `discoveredSet` remembers what you have been near, `:3600` adds to it. The label is chosen
by range at `:1161`. The sweep bar at `:1302` `sweepAng` and `:2584` is decoration; it does not
gate observations.

No last-seen time, no estimated heading or speed, no uncertainty. Contacts have exact `x, y`.

### `docs/reference-sim/radar.ts`: the real thing

Its own comment at `:15-22` states the design: "A track is what you KNOW, not what is true."

| Line | Name | What it is |
|---|---|---|
| 7 | `interface Observation` | `bearing`, `from` (where we were), `at` (mission time) |
| 13 | `interface Contact` | `pos`, `id` — the ground truth |
| 23 | `class Track` | the knowledge |
| 29 | `resolved` | do we have a position, or only bearings? |
| 30 | `fixPos`, `31` `lastFix` | last solved position and when |
| 33 | `estHeading`, `34` `estSpeed` | estimated course |
| 35 | `confidence`, `36` `allegiance`, `37` `scanned`, `38` `shipClass` | |
| 42 | `designator` | `TRACK 01` |
| 46 | `get lastBearing` | last sighting direction |
| 50 | `age(now)` | seconds since the last fix |
| 53 | `uncertainty(now)` | `RADAR_RADIUS` if unresolved, else `estSpeed * age` |
| 59 | `predicted(now)` | dead-reckoned position |
| 67 | `label(...)` | `BEARING ONLY` / `LARGE WARSHIP RETURN` / etc — the label is the knowledge state |
| 76 | `observe(from, truePos, now)` | records one bearing |
| 87 | `tryResolve(...)` | two bearings taken `FIX_BASELINE` apart cross at a fix |
| 129 | `Radar.step(...)` | sweeps; a contact is observed **only** when the beam crosses it (`:147`, `crossed` at `:165`) |
| 132 | drop | forgets a track after `DROP_TRACK_AFTER` |

Numbers, `docs/reference-sim/tuning.ts`: `RADAR_RADIUS 40000` (`:41`), `SONAR_RADIUS 8000` (`:42`),
`RADAR_INTERVAL 3.0` (`:43`), `FIX_BASELINE 900` (`:44`), `CONFIDENCE_DECAY 0.06` (`:45`),
`DROP_TRACK_AFTER 45` (`:46`), `BLIP_FLASH_TIME 0.6` (`:47`).

### Which is further along

1. **`docs/reference-sim/radar.ts`** — by a long way. It is the only place that models
   knowledge separately from truth.
2. `References/starwolf-prototype.html` — has range gating and discovery, nothing else.
3. **Unity — last.** `Radar.cs` and `RadarTarget.cs` contain nothing worth keeping for a
   tactical plot.

---

## 4. The bogey

**The brief's premise is wrong.** `Assets/Scenes/Tactical/02_Grid.unity` has no `BogieLoad`
under `UI`, and no `Canvas-BogieShipStats`. A case-insensitive search for `bogie` and `bogey`
across that file returns zero hits. The only three script types in the whole scene are
`IconRail`, `DrawerPanel` and `BoardController` (all in `Assets/Scripts/Tactical/`). It is a UI
blockout with rails, drawers and a camera.

### Where the bogey actually lives

| Thing | Path |
|---|---|
| The prefab | `Assets/Prefabs/BogieLoad.prefab` — children `LoadMesh` (`:16`), `BogieLoad` (`:121`), `Camera-BogieShipStats` (`:157`) |
| That camera | orthographic, size **5** (`BogieLoad.prefab:212-213`) |
| Used in | `Assets/Scenes/Prototype.unity`, `Assets/Scenes/Prototype Dev.unity`, `Assets/Prefabs/UI.prefab` |
| Documented at | `Assets/Scenes/Tactical/UICONTROLLER_PARTS.md:47`, `:217-219` |

Per `UICONTROLLER_PARTS.md:47`, `UI/BogieLoad/Camera-BogieShipStats` renders into
`Assets/Textures/Scaner.renderTexture`, which is displayed on
`Canvas-BogieShipStats/Top/0_BogieShipInfo/Left/ShipSides/Ship`.

**So: it is a portrait rig.** A mesh sitting in front of a tiny orthographic camera, feeding a
render texture into a scanner panel. Its transform has no relationship to where anything is in
the world.

### Is there anything in the world to draw?

| Thing | Verdict |
|---|---|
| `BogieClass.cs` (`Assets/UI/UI_Script/BogieClass.cs`) | Art references only — `go`, `mesh`, `wepImageGo`, `matWep`. Its own comment at `:12` says "need more data like stats etc". No position, no bearing, no heading, no speed. |
| `UIController.cs:104` `bogieList` | Populated by `AddBogie()` at `:1238`, which is called **only from commented-out test code** (`:1205-1236`). In practice the list is empty. |
| `UIController.cs:1200` `BogeySpot(float d)` | Takes a bearing in degrees and plays a shader flash on the radar image. Nothing is stored. |
| `UIController.cs:1415`, `:1445`, `:1482` | Toggle `bogieList[j].go`'s `MeshRenderer` and a child icon per zoom step — so bogies *are* GameObjects with transforms in the old Prototype scenes. |
| `Assets/Scripts/EnemyShip.cs` | A real 3D ship: transform, patrol, `detectionRadius` (`:29`), `maxHealth` (`:34`), implements `IDamageable`. This is the only genuine world-space enemy in the project. |

**Answer.** For the tactical board there is nothing in the world to draw. The bogey there is a
UI readout only — a mesh in a render texture. `EnemyShip` is a real world object, but nothing
connects it to the plot, and it does not appear in any Tactical scene.

---

## 5. Screen-to-world, zoom, pan

### Screen to world: does not exist

The only screen-to-world call anywhere in the project is
`Assets/Scripts/Targeting/TargetingSystem.cs:63`:

```csharp
Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
```

Fixed screen centre, no pointer input, a 3D physics raycast. Nothing takes a mouse position.
There is no `ScreenToWorldPoint` and no `ScreenPointToLocalPointInRectangle` in any script.

### Pan: does not exist

Nothing moves a camera transform for a plot.

### Zoom: three hardcoded steps inside `UIController`

| Line | Zoom step | Effect |
|---|---|---|
| `Assets/UI/UI_Script/UIController.cs:1410-1411` | 1x | `canvasUI[1].worldCamera = sc[6]`, `sc[6].orthographicSize = 50f` |
| `:1440-1441` | 100x | `sc[6].orthographicSize = 500f` |
| `:1477-1478` | 1000x | `canvasUI[1].worldCamera = sc[5]`, `sc[5].orthographicSize = 5000f` |

Three discrete presets. No continuous zoom, no zoom-to-cursor, no clamping, and each step also
rewrites grid shader properties, radar ranges and per-bogey renderers in the same block. Buried
in a very large file. Not reusable as a camera controller.

### The scene camera has no controller

`Assets/Scenes/Tactical/02_Grid.unity:3350-3351` — the `Main Camera` is orthographic at size
**30000**, far clip 20000. It has four components and none of them is a script. Nothing drives it.

### the reference sim has half of it

| Line | Thing |
|---|---|
| `the deleted prototype's plot/render.ts:6-10` | `interface Camera { span; centre }` — span is world units across the plot width |
| `render.ts:16-19` | `toScreen(p)` — world to screen |
| `render.ts:35-56` | grid that picks a 1/2/5/10 cell size to keep ~12 lines on screen at any zoom |
| the deleted prototype's `ui/Board.tsx` | `camRef` starting span 60000 (UI was not kept) |
| `Board.tsx:62-63` | smooth follow of own ship |
| `Board.tsx:86` | wheel zoom, factor 1.12, clamped 4000..140000 |

But there is **no inverse** even here. `Board.tsx:101` — the canvas `onClick` is a no-op stub
(`onClick={() => setSelected(selected)}`), and AIM at `Board.tsx:107-113` picks the first
resolved track in the list rather than anything the user clicked. Screen-to-world has not been
written in any language in this repo.

---

## What must be built from scratch

Only the items below have no existing implementation anywhere — not in Unity, not in either
prototype. Everything else on this page exists somewhere and should be ported, not reinvented.

1. **Screen-to-world on the plot.** The inverse of `render.ts:16-19`. Nothing in the repo does
   it. Needed before a click can become a bearing.
2. **Pointer picking on the plot** — hit-testing a click against tracks, arcs and grid.
   `Board.tsx:101` is a stub.
3. **A plot camera controller in C#** — pan, continuous zoom with clamps, zoom under cursor,
   follow. `UIController`'s three presets are not a starting point. The `02_Grid` camera has no
   script at all.
4. **Contacts as real world objects in a Tactical scene.** There is currently nothing in
   `02_Grid` for a plot to draw. `EnemyShip` exists but is unconnected and lives in other scenes.
5. **The wiring between mounts, tracks and the plot** — designation, the valid-mount set from
   `TACTICAL_BOARD_SPEC.md:740-742`, the recompute on mount change (`:751`).
6. **Time-of-flight and lead readout in the UI.** The maths exists (`weapons.ts:118`
   `Slug.flightTime`, `world.ts:114` `solutionFor`); the readout does not.
7. **Whether there are three mounts or five.** The handoff says five, the spec (`:645`) and both
   prototypes say three. This is a decision, not code, and it blocks the mount type.

### Port rather than rebuild

- Turret arming, arcs, per-turret bearing, solution expiry → `docs/reference-sim/weapons.ts`
- Tracks, bearing-only contacts, two-bearing fixes, age, uncertainty, sweep gating →
  `docs/reference-sim/radar.ts`
- Bearing / relative / angle-on-bow maths → `docs/reference-sim/geometry.ts`
- Every tunable number → `docs/reference-sim/tuning.ts`
- Plot drawing, grid cell sizing, world-to-screen → `the deleted prototype's plot/render.ts`

### Do not reuse

`Assets/Scripts/Radar.cs`, `Assets/Scripts/RadarTarget.cs`,
`Assets/Scripts/Targeting/TargetingSystem.cs`, `Assets/Scripts/Weapons.cs`,
`Assets/UI/UI_Script/BogieClass.cs`. None of them model what the targeting computer needs, and
`Radar.cs:71` reads ground truth every frame, which is the opposite of the game's core idea.
