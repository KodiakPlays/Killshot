# UIController — Parts List

What each tactical-board element is made of. For every panel: the `UIController` field, the canvas,
the camera, the render texture, and the exact GameObject path.

Everything here was read out of `Assets/Prefabs/UI.prefab`, `Assets/Prefabs/Canvas.prefab` and
`Assets/Scenes/Prototype Dev.unity`, and checked against the live Editor.

**Never edit `UI.prefab`.** See `Assets/Scenes/Tactical/BUILD_LOG.md` for the reuse rule.

---

## 1 · How the cockpit is put together

There is one prefab, `Assets/Prefabs/UI.prefab`, root object `UI`.

`UI` has two kinds of child:

1. **The panel canvases** — `Canvas-Power`, `Canvas-PlayerShipStats`, `Canvas-BogieShipStats`,
   `Canvas-Weapon`, `Canvas-Navagation`. Each is **Screen Space - Camera**. Each has its own camera.
   That camera does not draw to the screen. It draws to a **render texture**.
2. **`Canvas-Interior`** — the cockpit shell. This is a nested instance of
   `Assets/Prefabs/Canvas.prefab`, renamed. It holds the metal, the switches and the buttons.
   The render textures from step 1 are pasted onto flat images inside it.

So the picture on a screen and the buttons under that screen live in **different places**. The picture
comes from a panel canvas. The buttons are physical parts of `Canvas-Interior`.

`UIController` itself sits on **`Assets/Prefabs/PlayerShip.prefab`**, not on the UI. In
`Prototype Dev.unity` it is the PlayerShip instance's component, and all its UI fields are set as
prefab-instance overrides pointing into the `UI.prefab` instance.

`UIController.cs` has a singleton `Awake` (`Assets/UI/UI_Script/UIController.cs:19`). Two of them in a
scene and one destroys itself.

---

## 2 · Cameras and render textures

| Camera GameObject | Renders which canvas | Into which render texture | Shown on |
|---|---|---|---|
| `UI/Camera-Power` | `UI/Canvas-Power` | `Assets/Textures/Screen-Power.renderTexture` | `Canvas-Interior/.../Power_Cont/Power/Power_Screen` |
| `UI/Camera-Weapon` | `UI/Canvas-Weapon` | `Assets/Textures/Screen-Weapon.renderTexture` | a `RawImage` inside `Canvas-Interior` |
| `UI/Camera-ShipStats` | `UI/Canvas-Navagation` | `Assets/Textures/Screen-PlayerShip.renderTexture` | `Canvas-Interior/.../ShipDiagnostic/Top` |
| `UI/Camera-World` | (world view) | `Assets/Textures/WorldMap.renderTexture` | — |
| `UI/PlayerShipLoad/Camera-PlayerShipStats` | the player ship model | `Assets/Textures/Screen-PlayerShip.renderTexture` | `Canvas-PlayerShipStats/Top/PlayerShipHealth/Left/GameObject/Ship` |
| `UI/BogieLoad/Camera-BogieShipStats` | the scanned enemy model | `Assets/Textures/Scaner.renderTexture` | `Canvas-BogieShipStats/Top/0_BogieShipInfo/Left/ShipSides/Ship` |

`Radar.renderTexture` is shown on `UI/Canvas-Navagation/MiddleScreenMask/ScreenWorld`.

The camera list on `UIController` is `sc` (`Assets/UI/UI_Script/UIController.cs:131`); the canvas list
is `canvasUI` (`:134`).

`canvasUI` order in `Prototype Dev.unity`:
`[0]` Canvas-Interior · `[1]` Canvas-Navagation · `[2]` Canvas-PlayerShipStats ·
`[3]` Canvas-BogieShipStats · `[4]` Canvas-Power · `[5]` Canvas-Weapon.

---

## 3 · Power

### The picture

| Field | Line | What it points at |
|---|---|---|
| `imgPowerMet` (`List<Image>`) | `UIController.cs:31` | the five bars plus the reactor bar |
| `powerNodeImg` (`Image[]`) | `UIController.cs:139` | the six arrow/node graphics under the bars |
| `shaPowerMet` | `UIController.cs:38` | `Assets/UI/Shader/UI_GradeDisplay.shadergraph` |
| `shaReactorMet` | `UIController.cs:39` | `Assets/UI/Shader/UI_GradeDisplayReactor.shadergraph` |
| `powerNodeSha` | `UIController.cs:138` | `Assets/UI/Shader/UI_PowerArrow.shadergraph` |
| `staticImg[4]` | `UIController.cs:183` | `UI/Canvas-Power/4_Power_ScreenLines` |

`imgPowerMet` GameObject paths, in order:

| # | Path in `UI.prefab` |
|---|---|
| 0 | `UI/Canvas-Power/Power_Screen/GameObject/Power_ChargeMeters/Power_ChargeMeters_ENG/Power_ChargeMeters_ENG_Mat` |
| 1 | `.../Power_ChargeMeters/Power_ChargeMeters_WEP/Power_ChargeMeters_WEP_Mat` |
| 2 | `.../Power_ChargeMeters/Power_ChargeMeters_CRW/Power_ChargeMeters_CRW_Mat` |
| 3 | `.../Power_ChargeMeters/Power_ChargeMeters_SHD/Power_ChargeMeters_SHD_Mat` |
| 4 | `.../Power_ChargeMeters/Power_ChargeMeters_SEN/Power_ChargeMeters_SEN_Mat` |
| 5 | `UI/Canvas-Power/Power_Screen/GameObject/Power_Reactor/Power_Reactor_Mat` |

`powerNodeImg` paths, in order — all under
`UI/Canvas-Power/Power_Screen/GameObject (2)/GameObject (1)/`:
`0_ENG`, `1_WEP`, `2_CRW`, `3_SHO`, `4_SEN`, `5_Reactor`.

Each of these gets a fresh `Material` at runtime. Nothing is a shared `.mat` asset.
`UIPowerClass` (`Assets/UI/UI_Script/UIPowerClass.cs:10`) is the little wrapper that owns one bar's
material and pushes `_PowerCur` / `_PowerMax` / `_Charge` into it.

### The green buttons

These are **not** inside the render texture. They are physical switches on `Canvas-Interior`, sitting
under and above the power screen.

**Five toggle switches**, field `btnPowerBool` (`UIController.cs:28`), with their images in
`btnPowerBoolImage` (`UIController.cs:46`). Path prefix:
`UI/Canvas-Interior/MoveShip/BK/Screen/LeftScreen/LeftBot/Power_Cont/Power/Buttons/Power_Bools/`

| # | GameObject | onClick, in order |
|---|---|---|
| 0 | `Btn_ENG/Btn_ENG` | `UIController.ChargeBtn(0)` then `PowerManager.ToggleEngines()` |
| 1 | `Btn_WEP/Btn_WEP` | `UIController.ChargeBtn(1)` then `PowerManager.ToggleWeapons()` |
| 2 | `Btn_CRW/Btn_CRW` | `UIController.ChargeBtn(2)` then `PowerManager.ToggleCrew()` |
| 3 | `Btn_SHD/Btn_SHD` | `UIController.ChargeBtn(3)` then `PowerManager.ToggleShields()` |
| 4 | `Btn_SEN/Btn_SEN` | `UIController.ChargeBtn(4)` then `PowerManager.ToggleSensors()` |

Each has a label child, `key_ENG`, `key_WEP`, `key_CRW`, `key_SHD`, `key_SEN`.

`btnPowerBool[5]` is the reactor switch:
`.../Power_Cont/Power/Buttons/Btn_Reactor_cont/Btn_Reactor` → `UIController.VentBtn()`.

There is a second vent control inside the screen picture:
`UI/Canvas-Power/Power_Screen/Power_Reactor/Power_Reactor (1)/turnSwitchCont/turnSwitchBtn`
→ `UIController.VentBtn()`.

**The two green plates on top**, under
`UI/Canvas-Interior/MoveShip/BK/Screen/LeftScreen/LeftBot/Power_Cont/power_special_btns/`:

| GameObject | Sprite | onClick |
|---|---|---|
| `btn_emergencyVent` (+ label `tmp_emergencyVent`) | `Assets/UI/Button/btn_green00.png` | **nothing wired** |
| `btn_blackAlert` (+ label `tmp_blackAlert`) | `Assets/UI/Button/btn_green00.png` | **nothing wired** |

`UIController.EmergencyVentBtn()` exists at `UIController.cs:917` and nothing calls it. Wiring
`btn_emergencyVent` to it is a one-line fix in the Inspector. `btn_blackAlert` has no method at all.

In `Canvas.prefab` this object is named `Power (2)`; the nested instance in `UI.prefab` renames it to
`power_special_btns`.

### Sprites

| Field | Line | Asset |
|---|---|---|
| `uiSprite[0]` — switch OFF | `UIController.cs:43` | `Assets/UI/Button/btn_bool-0.png` |
| `uiSprite[1]` — switch ON | `UIController.cs:43` | `Assets/UI/Button/btn_bool.png` |
| the five `Btn_*` images, at rest | — | `Assets/UI/Button/btn_bool.png` |
| `btn_emergencyVent`, `btn_blackAlert` | — | `Assets/UI/Button/btn_green00.png` |

`btn_green01.png` is used elsewhere in the cockpit (map zoom, frequency tuner), not on the power
panel.

### The methods

| Method | Line | What it does |
|---|---|---|
| `StartPower()` | `UIController.cs:554` | Builds one `UIPowerClass` per bar. Seeds max/current from `PowerManager`. Sizes everything off `btnPowerBool.Count`. |
| `ChargeBtn(int i)` | `UIController.cs:666` | **What a green switch calls.** Only calls `powerManager.ToggleDrawState(_pmSystems[i])`. It touches no visuals. |
| `SyncPowerBarsFromManager()` | `UIController.cs:678` | Called every frame from `Update` (`:315`). Copies `PowerManager` state into the bars and swaps the switch sprites. **This owns all power visuals.** |
| `ChargeOn(int i)` / `ChargeOff(int i)` | `:741` / `:755` | Direct on/off. Sets the sprite, then `ChargeNodeCheck()`. |
| `ChargeNodeCheck()` | `:768` | Recomputes which arrow nodes light up. |
| `VentNodeCheck()` | `:816` | Same, for the vented state. |
| `PowerNodes(int i, int state)` | `:584` | Sets the arrow shader. States 0–8: off, left, left+charge, end of chain, right, right+charge, end, reactor left, reactor right. |
| `ChargeOnAnim(int i)` | `:864` | The charge-up animation coroutine. |
| `VentBtn()` | `:885` | Stops every charge, resets every switch sprite, then `powerManager.VentAllSystems()`. |
| `EmergencyVentBtn()` | `:917` | Same but instant — `powerManager.EmergencyVentSystemsOnly()`. **Not wired to any button.** |

The index → system map is `_pmSystems`, built once in `Start` (`UIController.cs:207`):
`0 engines · 1 arms · 2 bay · 3 support · 4 sig`. `PowerManager.cs:295-305` holds the toggle methods.

**Read this order carefully.** A click runs `ChargeBtn(i)` *and* the `PowerManager.Toggle*()` call on
the same button. `ChargeBtn` calls `ToggleDrawState`, the second call runs `ToggleSystemState`. They
are different methods (`PowerManager.cs:147` and `:192`). Two state changes per click.

---

## 4 · ShipStats

Canvas `UI/Canvas-PlayerShipStats`. Ship picture comes from
`UI/PlayerShipLoad/Camera-PlayerShipStats` → `Screen-PlayerShip.renderTexture`.

| Field | Line | GameObject path |
|---|---|---|
| `velocityMeterImg[0]` | `UIController.cs:77` | `UI/Canvas-PlayerShipStats/Top/PlayerShipHealth/Right/velosityMeter` |
| `velocityMeterTMP[0]` | `:78` | `UI/Canvas-PlayerShipStats/Top/PlayerShipHealth/Right/velosityText` |
| `staticImg[1]` | `:183` | `UI/Canvas-PlayerShipStats/1_Ship_ScreenLines` |
| `stabilityImg` | `:82` | inside the nested `Canvas-Interior` |
| `stabilityTxt` | `:83` | set on a loose scene object, not in `UI.prefab` |
| `compassRect` | `:64` | `UI/Canvas-Navagation/MiddleScreenMask/Compass` |
| `speedometer` | `:65` | not set |

The ship render target is `UI/Canvas-PlayerShipStats/Top/PlayerShipHealth/Left/GameObject/Ship`.
Hull numbers are pushed by `PollHullDisplay()` (`UIController.cs:239`), which reads `HullSystem`
and calls `UpdateCompass`.

---

## 5 · Weapons

Canvas `UI/Canvas-Weapon`, camera `UI/Camera-Weapon` → `Screen-Weapon.renderTexture`.
All paths below start `UI/Canvas-Weapon/Left/`.

| Field | Line | GameObject path |
|---|---|---|
| `weaponNameText` | `UIController.cs:165` | `GameObject (3)/GameObject/top/Right/wep/wepName` |
| `weaponTypeText` | `:166` | `GameObject (3)/GameObject/top/Right/WepType/wepType` |
| `weaponAmmoText` | `:167` | `GameObject (3)/GameObject/weponInfo/Ammo/AmmoAmmount (1)` |
| `weaponStatusText` | `:168` | `GameObject (3)/GameObject/weponInfo/Status/StatusAmmount` |
| `weaponRechargeBar` | `:169` | `GameObject (3)/GameObject (1)/GameObject` |
| `weaponIconImage` | `:170` | `GameObject (3)/GameObject/top/weponIcon` |
| `tabFrame[0..3]` | `:179` | `topBar/0_LaserTabImage`, `topBar/1_RailTabImage `, `topBar/2_MisslesTabImage`, `topBar/3_PodsTabImage (1)` |
| `tabWepName[0..3]` | `:178` | the `*Text (TMP)` child of each tab above |
| `staticImg[3]` | `:183` | `UI/Canvas-Weapon/3_Weapon_ScreenLines` |
| `weaponScreenImage` | `:117` | `UI/Canvas-Navagation/MiddleScreenMask/ScreenWeapon` |
| `screenWepGO` | `:118` | `UI/Canvas-Navagation/MiddleScreenMask/ScreenRadar` |

Note `1_RailTabImage ` has a trailing space in its name.

Buttons: `BtnWepTab()` (`:1894`), `BtnWeaponFire()` (`:1931`), `BtnWeaponLoad()` (`:2006`).
Display refresh is `UpdateWeaponDisplay(...)` (`:2014`).

---

## 6 · Scanner

Canvas `UI/Canvas-BogieShipStats`. The scanned ship picture comes from
`UI/BogieLoad/Camera-BogieShipStats` → `Scaner.renderTexture`, shown on
`UI/Canvas-BogieShipStats/Top/0_BogieShipInfo/Left/ShipSides/Ship`.

| Field | Line | GameObject path |
|---|---|---|
| `scannerImg` | `UIController.cs:97` | `UI/Canvas-BogieShipStats/Top/FrequancyTuner/scanner` |
| `scannerSlider` | `:91` | `UI/Canvas-BogieShipStats/Top/FrequancyTuner/scanner/scannerBar` |
| `bogieTabTran[0]` | `:88` | `UI/Canvas-BogieShipStats/Top/0_BogieShipInfo` |
| `bogieTabTran[1]` | `:88` | `UI/Canvas-BogieShipStats/Top/1_Comm` |
| `bogieTabBtn` | `:90` | `turner_btn` inside the nested `Canvas-Interior`, onClick `BtnBogieTab()` |
| `velocityMeterImg[1]` | `:77` | `UI/Canvas-BogieShipStats/Top/0_BogieShipInfo/Right/bogieVelosityMeter` |
| `velocityMeterTMP[1]` | `:78` | `UI/Canvas-BogieShipStats/Top/0_BogieShipInfo/Right/bogieVelosityText` |
| `commsFrequencyDisplay` | `:150` | `UI/Canvas-BogieShipStats/Top/HeaderInfo/Frequancy` |
| `commsLogText` | `:153` | `UI/Canvas-BogieShipStats/Top/1_Comm/Top/Text (TMP)` |
| `staticImg[2]` | `:183` | `UI/Canvas-BogieShipStats/2_Bogie_ScreenLines` |

Methods: `NewScan()` (`:329`), `ScanNewTarget()` (`:448`), `ScanMesh()` (`:548`),
`BtnBogieTab()` (`:1749`), `BtnScannerCloke()` (`:1791`).

The frequency tuner fields — `frequancyTMP` (`:50`), `frequancyTrans` (`:51`), `tunerTrans` (`:54`)
— all point into the nested `Canvas-Interior`, not into `Canvas-BogieShipStats`.

---

## 7 · Things that will trip you up

1. **The buttons are not in the panel.** `Canvas-Power` is only a picture. Move it to a new scene on
   its own and every power switch is left behind in `Canvas-Interior`.
2. **`UI.prefab` nests `Canvas.prefab`** as `Canvas-Interior`. `BUILD_LOG.md` records dropping the
   interior. Dropping it drops the power switches, the tuner and `bogieTabBtn` with it. Replace them
   before you delete it.
3. **`Canvas.prefab` has its own disabled `UIController`** (`m_Enabled: 0`). Do not enable it — the
   singleton in `UIController.cs:19` will destroy one of the two UI trees.
4. **Two calls per power click.** `ChargeBtn` and `Toggle*` both change state. If you rewire the
   power buttons, decide which one survives.
5. **`btn_emergencyVent` and `btn_blackAlert` do nothing.** They look live and are not.
6. An earlier layout note listed a `Camera-Power` *and* a `Camera-PlayerShipStats` as siblings under `UI`.
   Only `Camera-Power`, `Camera-Weapon`, `Camera-World` and `Camera-ShipStats` are direct children.
   `Camera-PlayerShipStats` and `Camera-BogieShipStats` live inside the nested `PlayerShipLoad` and
   `BogieLoad` prefabs.
7. **`sc[3]` and `sc[4]` are both `UI/Camera-Power`.** Probably a wiring mistake. Check before use.
