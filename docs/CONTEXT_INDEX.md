# Starwolf — Start Here

**Read this first. It is short on purpose.**

This file exists so you do not have to read everything, and so you do not reopen decisions that are
already closed. If you only read one file in this repo, read this one.

The game is **Starwolf**. `Killshot` is the old repository name. Same project.

---

## 1 · What the game is

Starwolf is a **top-down 2D real-time tactical space combat game**. You command one ship. You enter
a sector of space, hunt down the Traitor Lord's flagship, kill it, and escape through the jump point
at the map edge. Killing the flagship is not winning. An extraction marker appears at a far edge and
you have to get there through everything you stirred up.

**You play the entire game through a control panel.** There is no cockpit and no view of the outside,
apart from a small forward screen. You read the world through instruments — navigation, weapons,
sensors, troops, signals — and you act through them. **Your ship is a stealth destroyer.** It wins by
patience and position, not a straight fight. You arrive broadcasting a false ID, so every enemy reads
you as friendly and **all of them are passive at the start**. The game is about how long you can keep
that lie going, and picking the moment to drop it. Detection is not failure. It is the rhythm: hide,
gather, act, get found, break away, gather again.

**What is built today** is a campaign. NEW GAME opens the save slots and a sector map with one real
mission so far, **MILK RUN**, and a throwaway TEST MISSION after it. SKIRMISH plays **Wolf Hunt**, where a
Garuda-class cruiser hunts you. The old training levels (FLIGHT DRILL, COLD RUN, WEAPONS FREE, NO
PEEKING, FINDERS KEEPERS and INTERCEPT) are switched off and kept. Where the game is today, and what
comes next, is in `docs/BUILD_LOG.md`, section 1. How Wolf Hunt was built is in
`docs/BUILD_HISTORY.md`, "Steps 94 to 101".

---

## 2 · Which source wins — READ THIS BEFORE ANYTHING ELSE

**The sources are a timeline. Newest wins. Always.**

```
oldest ──────────────────────────────────────────────────────────────► newest

GDD + Art Spec  →  Unity build  →  prototype  →  Silent Shark  →  handoff  →  owner, in session
the original       a partial       the latest    how it should    the current   the definitive
vision. Why the    older build.    working       present itself.  visual        word. Beats
game exists.       Historical.     expression.                    system.       everything.
```

| Rank | Source | Decides |
|---|---|---|
| **1** | **What the owner says, most recently** | **Everything.** Overrides all of the below. |
| 2 | The newer Claude Design handoff. Its reference files are in `Assets/UI/Board/Design/` | The visual system, and the board built into `02.1_Grid`. The owner ruled it wins on weapons, power and distances |
| 3 | `References/Tactical spaceship UI planning.zip` (the older Claude Design handoff) | The visual system — colour, type, layout, marker shapes — where the newer export is silent |
| 4 | Silent Shark (released tactical submarine sim) | How the game should present itself |
| 5 | `References/starwolf-prototype.html` | How systems behave, and **every number** |
| 6 | `References/Starwolf GDD.pdf` | **Why** the game exists. Old. Never numbers. |
| 7 | `References/Starwolf_ Art Spec.pdf` | Ship design language only. Superseded on visuals. |
| 8 | The Unity build | What code exists today. **Decides nothing.** |

**The GDD is the oldest document, not the authority.** It is the first thing that was written, 74
pages, and most of it has been overtaken.

> **Never "correct" a newer value back to an older one.** If the prototype and the GDD disagree on a
> number, the prototype is right. If the owner ruled in session, the owner is right, over both. When
> the Unity build differs from the spec, the Unity build is behind — fix the code, not the spec.

### To know X, read Y

| To know… | Read |
|---|---|
| Why the game exists, the fantasy, the tone | `References/Starwolf GDD.pdf` |
| What a system does and what number it uses | `docs/TACTICAL_BOARD_SPEC.md`, then `References/starwolf-prototype.html` |
| What the board looks like — colour, type, spacing, markers | `Assets/UI/Board/Design/README.md`, then the older handoff zip's `README.md`. Two of the first README's rules are wrong in this project: sprites are imported with sRGB **off**, and colours are not typed onto an image as the token's hex. The token's hex goes in the palette file, `Assets/UI/Board/Palette.asset`. |
| How each control on the new board behaves, state by state | `Assets/UI/Board/Design/reference/UIStateSpec.md` and `Assets/UI/Board/Design/reference/board-logic.js` |
| Where every element is on the new board, in pixels | `Assets/UI/Board/Design/layout/layout.json` |
| What the handoff package left out | `docs/finished-plans/HANDOFF_GAPS.md` |
| What Claude Design must still provide, verified | `docs/finished-plans/CLAUDE_DESIGN_NEEDS.md` |
| How the prototype actually behaves, without opening 4,472 lines | `docs/PROTOTYPE_BASELINE.md` |
| What the game is, how to run it, the controls | `README.md` (repo root) |
| What Soham's older scripts do, read line by line as of 2026-09-01 | `docs/CODEBASE_BASELINE.md` |
| What the board's scripts are and what they are wired to | `docs/BUILD_LOG.md`, section 3 |
| What any C# script does | `docs/SCRIPT_LIST.md` lists all 109. Each script also starts with a note in plain English on what it does and how it works. The rules for those notes are in `docs/script-notes/BRIEF.md`. |
| How the systems and the scripts connect | Two maps on one page: https://claude.ai/artifact/Tk285DVMpwzAfb44Ph2Abr. "Systems map" shows the board's systems as the player meets them, and what changes what. "Code map" shows every script and which other scripts it uses. The page is private until the owner shares it. |
| What is built in the tactical scene, and the traps already hit | `docs/BUILD_LOG.md`. The traps are in section 5. |
| What is left to do, sorted | `docs/BUILD_LOG.md`, section 8. It starts with the MILK RUN build checklist. |
| Every known bug that is not fixed | `docs/BUILD_LOG.md`, section 8, "Bug list" |
| Every finished step, and closed items, with their full write-ups | `docs/BUILD_HISTORY.md` |
| Plans that are finished (room layout, the Claude Design gap lists, the 2026-11-09 tuning audit) | `docs/finished-plans/` |
| How the architecture map page is rebuilt (its builder and the weekly job) | `docs/architecture-map/` (`WEEKLY.md`) |
| A ship's numbers: speed, power, weapons, armour, crew per room | `Assets/Ships/Asura.asset` (our ship), `Assets/Ships/Garuda.asset` (the enemy cruiser) and `Assets/Ships/Frigate.asset` (the Naga-class frigates). Edit them in **Starwolf → Tuning**, on the Ship and Enemy tabs. Each room's row from bow to stern (Step 186), and each side's armour, are also on the Hard Points tab → ROOMS. |
| The keys, and how to change them | `README.md`, Controls. Change them on the Keys tab of **Starwolf → Tuning**. |
| How damage works: armour, rooms, fire, vacuum, radiation | `docs/DAMAGE_README.md` |
| How boarding works: the assault boats and their orders | `docs/BOARDING_README.md` |
| The design audit the damage model is built on | `References/Star Wolf - Design Audit.dc.html` |
| How to make or change a level | Menu **Starwolf → Level** (`Assets/Editor/LevelWindow.cs`). The levels are in `Assets/Scenes/Levels/`. |
| Whether a change to speeds, ranges or weapons still gives a fight of the right length | `docs/scale-sim/README.md` |
| How to turn the repo into NotebookLM sources | `Tools/notebooklm/README.md` |
| How the green phosphor screen works (intro, welcome page, INITIALISING), and how to put it on a new screen | `Assets/StarwolfIntro/README.md` |
| The seven levels: what each one asks, and how to win or lose it | `docs/TACTICAL_BOARD_SPEC.md` §15.5. Each level's numbers are in `BUILD_LOG.md`, section 2 (MILK RUN, Wolf Hunt), and in `docs/BUILD_HISTORY.md`, section 2 (the old training levels). |
| FLIGHT DRILL: its 14 steps, and how to change one | `docs/BUILD_HISTORY.md`, section 2, `FlightDrill`. The notes, rings and amounts are in the Mission box in `Assets/Scenes/Levels/FlightDrill.unity`. |
| What ENG does to the ship's movement, and the side thrusters | `docs/TACTICAL_BOARD_SPEC.md` §4.2 and §4.3. The numbers are in each ship's stats file in `Assets/Ships`. |
| The sniper ship: the spinal railgun on the nose (built, Step 170), the flare, sensor drones, the sight in fire control (planned) | `docs/SNIPER_PLAN.md` |
| Where each ship's rooms are: a row from bow to stern per room, set in Tuning's Hard Points tab → ROOMS (built, Step 186) | `docs/finished-plans/ROOM_LAYOUT_PLAN.md`. The rules for a hit are in `docs/DAMAGE_README.md`. |
| The training levels and how they were built; the plan to split WEAPONS FREE | `docs/FTUE_PLAN.md` §0 |
| The later tutorial mission, with the XO walking you through each station. It is a plan only | `docs/FTUE_PLAN.md` §1 to §9 |
| Mission setup: writing a mission's words (name, briefing, checkpoint talk, ship call signs) in a Starwolf → Mission window. It is a plan only | `docs/MISSION_SETUP_PLAN.md` |
| Where a panel's prefab and art are, and how to swap its art | `docs/BOARD_UI.md` |
| How a shot looks on the plot and in the close-up: the railgun's charge glow, flash and vent gas, the shot line, the far shot's IMPACT countdown, craft leaving, PDC streams (Steps 184 and 197), and which Tuning value changes each | `docs/BOARD_UI.md`, Plot. The rules are in `docs/TACTICAL_BOARD_SPEC.md` §8.4 (push back, far shot), §8.6 (PDC streams), §12.3 (catapult and hard break) and §18.2 (the countdown). The owner's picks are on the "Ship Actions Lab" page, https://claude.ai/artifact/KXZhwnSU7hoG7aaEqThx6g |
| Reference clips for how weapon fire should feel (The Expanse, Battlestar Galactica, real navy footage), each watched, with a GIF of the moment | The "Ship Actions Lab" page, Learning map tab: https://claude.ai/artifact/KXZhwnSU7hoG7aaEqThx6g (34 clips, 2026-11-11) |
| The campaign's SECTOR MAP and ship's rooms: their parts, and how to change their art | `docs/BOARD_UI.md`, "Sector map" and "Ship rooms". The steps are 167 to 169 and 172 in `docs/BUILD_HISTORY.md`. |
| The green sector map (in the game since 2026-11-11, first tried as a test): the green map picture and how to remake it | `Assets/Scenes/Tactical/Sector Map Phosphor.unity`. The picture is made by `docs/sector-map-experiment/make_phosphor_map.py`. See `docs/BOARD_UI.md`, "Sector map", at its end. Step 201 in `docs/BUILD_LOG.md`. |
| The Escape menu on SAVE SLOTS, the sector map and the ship's rooms: its prefab, and how to change its look | `Assets/UI/Menus/Escape Menu.prefab`. See `docs/BOARD_UI.md`, "Escape menu". Step 190 in `docs/BUILD_HISTORY.md`. |
| How to set up a campaign mission: its name, level, briefing and dot on the sector map, its order, and switching it on or off | Menu **Starwolf → Campaign** (`Assets/Editor/CampaignWindow.cs`). It edits `Assets/Campaign/Campaign.asset`. Press + to add a mission and drag its level scene into Level. Drag a row to change the order. Switch a mission off to keep it out of the game; do not delete it. The level itself (checklist steps, rings, help text, battle group path) is made in its scene in `Assets/Scenes/Levels/`, with **Starwolf → Level** and the Mission part on it. The owner's four-step flow is in `docs/CAMPAIGN_FLOW_PLAN.md`, "Making a mission". Steps 183 and 205 in `BUILD_LOG.md`. |
| Between missions: the travel bar, repairs and restocks, the end card, the Escape menu, the ship rooms. The owner's answers and the build order | `docs/CAMPAIGN_FLOW_PLAN.md` |
| Where to write the campaign's closing line, and the end card | The line: **Starwolf → Campaign**, under the missions. It is saved in `Assets/Campaign/Campaign.asset` → Closing Line. The end card: `Assets/Scenes/Tactical/EndCard.unity`. MISSION SELECT after the last mission always opens it. With the line empty (as now), it goes straight to the title, where only STARWOLF flickers in. See `docs/BOARD_UI.md`, "End card". Steps 194 and 203 in `docs/BUILD_LOG.md`. |
| Spoken orders, and how an outside program (or Claude) could drive the built game — an idea, not built | `docs/voice-command-exploration.md` |
| Ship silhouettes, class size table, obstacle tiers | `References/Starwolf_ Art Spec.pdf` |
| Working rules — MCP, C# style, repo hygiene | `CLAUDE.md` |

---

## 3 · The documents, one by one

### References — owner-authored input. Never edit anything in `References/`.

**`References/Tactical spaceship UI planning.zip`** — the older Claude Design handoff. **The newer
handoff is the one built into `02.1_Grid`. Its reference files are in `Assets/UI/Board/Design/`.** A 430-line README
plus four `.dc.html` reference builds, a metal-plate WebGL shader, and a baked PNG.
*Good for:* exact hex values, type, px layout, easings, marker shapes, icon designs.
*Not good for:* the time model. Its README describes a paused-time, plan-then-EXECUTE game. **That is
not the design** — see §4. Also not an asset bundle: everything is drawn in CSS, so there is almost
no portable art in it.

**`References/starwolf-prototype.html`** — 4,472 lines, one file, playable. The GDD expressed in code
with changes made along the way.
*Good for:* behaviour and every number — speeds, ranges, damage, timers, power rates.
*Not good for:* UI. Its four-panel-plus-swappable-views layout is replaced by the board. It also has
known bugs listed in the spec's "Prototype defects" section — do not port those.

**`References/Starwolf GDD.pdf`** — 74 pages. The first document written.
*Good for:* why you are out there, what the game is about, that you are a stealth destroyer, what
winning is.
*Not good for:* **any number at all.** Its pages 68–74 are marked "notes and unfinished concepts —
not part of spec". Treat those as sketches.

**`References/Starwolf_ Art Spec.pdf`** — superseded on visuals by the handoff.
*Still the only source for:* ship design language, the class size table, obstacle tiers.

**`References/Star Wolf - Design Audit.dc.html`** — the design audit, "Design Audit & Fix Register".
Open it in a browser. Its §09 is the base of the damage model (see `DAMAGE_README.md`).

**`References/UI Killshot documentation.pdf`** — 4 pages. The UI author's API contract for the old
`UIController`. Historical, tied to the cockpit UI that is being retired.

**Silent Shark** — not a file. A released tactical submarine sim the team is adapting toward. What we
take from it: the plot is inhabited not consulted,
instruments are stations not a HUD, information costs time, uncertainty is drawn. What we reject: its
10000× time compression.

### The new Claude Design handoff — `Assets/UI/Board/Design/`

The newer handoff, and the one built into `02.1_Grid`. It came as the full board cut into images,
fonts and measurements. The art the board uses is in `Assets/UI/Board/Art/`. The fonts are in
`Assets/UI/Fonts/`. The rest of the export is deleted (Step 89). Its reference files are in
`Assets/UI/Board/Design/`. Read them, but don't change them. See `docs/BOARD_UI.md`.

- `README.md` — what the package is and how to use it.
- `asset-map.csv` — every piece of art in the package. `tokens.json` — the design's colours and sizes.
- `reference/UIStateSpec.md` — every control's states.
- `reference/board-logic.js` — the board's behaviour, as code.
- `layout/layout.json` — where every element is, in pixels.
- `layout/board-1920x1080.png` — the whole board as one picture.

### Project documents — ours, and editable.

**`README.md`** (repo root) — what the game is, how to run it, and the controls. Short.

**`CLAUDE.md`** (repo root) — how to work in this repo: Unity MCP rules, C# conventions, repo hygiene. Not design.

**`docs/TACTICAL_BOARD_SPEC.md`** — ~2,000 lines. **The specification.** What the game does, system
by system, with numbered rules. Start at "Summary" and "How it plays"; jump to a system after that.
*Not good for:* why. It deliberately omits reasoning.

**`docs/PROTOTYPE_BASELINE.md`** — ~335 lines. The prototype read line by line, so you do not have
to open the 4,472-line HTML.

**`docs/CODEBASE_BASELINE.md`** — ~1,700 lines. Soham's older scripts read line by line, as they
were on 2026-09-01. The board's code in `Assets/Scripts/Systems/`, `Tactical/`, `UI/`, `Game/` and
`Assets/StarwolfIntro/` is newer and is not in it. `BUILD_LOG.md` section 3 describes the board's code.

### The tactical scene — `Assets/Scenes/Tactical/`

**`BUILD_LOG.md`** — about 1,700 lines. Where the game is today (section 1), what exists and what is
wired to what (sections 2 and 3), the decisions (section 4), the traps already hit (section 5) and
what is left (section 8, starting with the MILK RUN build checklist). **Read it before touching that
scene.** The finished steps and closed items are in **`BUILD_HISTORY.md`**.

**`docs/finished-plans/HANDOFF_GAPS.md`** — 106 rows of what the handoff package left out.

**`docs/finished-plans/CLAUDE_DESIGN_NEEDS.md`** — the verified, bucketed list of everything Claude Design must still
provide. Checked against every source. The basis for the Claude Design request.

**`DAMAGE_README.md`** — how damage works. Armour on each face, the rooms behind it, fire, vacuum and
radiation, and the Engineering buttons that deal with them. A reactor at 0 destroys any ship.

**`BOARDING_README.md`** — how boarding works. The assault boats take marines to a nearly stopped
enemy with an open face. If the marines win, they DESTROY, DISABLE or SECURE the ship.

### Levels — `Assets/Scenes/Levels/`

Each level is a small scene. The board loads one on top of itself at play: the level picked on the
MISSIONS list (`TacticalWorld.chosenLevel`), or `Level_01` if the board was opened by itself. The
training levels keep their old scene names.

| On the list | Scene |
|---|---|
| 1 FLIGHT DRILL | `FlightDrill.unity` |
| 2 COLD RUN | `ColdRun.unity` |
| 3 WEAPONS FREE | `FirstShot.unity` |
| 4 NO PEEKING | `SilentShot.unity` |
| 5 FINDERS KEEPERS | `Prize.unity` |
| 6 INTERCEPT | `Intercept.unity` |
| 7 WOLF HUNT | `Level_01.unity` |

Each level has a Mission box with its goal and its briefing. The spawn points, debris and gas clouds
are placed in the level. Since Step 107 `Level_01` has no broken starbase. Make and edit levels with the menu **Starwolf → Level** (`Assets/Editor/LevelWindow.cs`).
The Level tool is described in `BUILD_LOG.md` section 3.

### `docs/reference-sim/` — deleted (owner, 2026-10-29)

The TypeScript simulation core of the shelved web prototype. Most of its mechanics are now in the
game's C#. Git has it: `git show 218acc8:docs/reference-sim/<file>` (`weapons.ts`, `radar.ts`,
`ship.ts`, `power.ts`, `tuning.ts`, `geometry.ts`, `world.ts`).

### `docs/scale-sim/README.md`

A small Python script that plays whole fights with the board's rules and numbers. It reports how long
a win takes and how many shots are traded. It set the 5× speed scale in Step 63 and checked the designer's table in Step 130. Re-run it after any
change to speeds, ranges, weapons or the enemy. Step 180's room hit rule and the rooms in a row from bow to stern (Step 186), side by side
(2026-11-10): the "Room hits" tab of the Google Sheet
[Starwolf Tuning and Roadmap](https://docs.google.com/spreadsheets/d/1jSI0Y1bzp3S412vM--h7_IBPSifXZ_-lRuxFVhAW_tQ/edit).

### `Tools/notebooklm/README.md`

A Python script that turns the repo into a few documents to load into NotebookLM. The output goes to
`Build/notebooklm/`, which git ignores. It covers Soham's older code. It leaves out the board's code.

---

## 4 · DECISIONS ALREADY MADE — DO NOT REOPEN

These are settled, owner-confirmed. Do not re-argue them, do not "restore" them from an older
document, and do not quietly design around them.

| Decision | The rule |
|---|---|
| **No cockpit** | The 3D cockpit interior is gone. The plot is the whole window. Station panels slide over it and never replace it. |
| **No shields** | The board has no shields. Soham's old `Assets/Scripts/Shields.cs` is still in the repo, and the board does not use it. **The hull side you present is your only armour** — prow 100, starboard 100, aft 50, port 100 (`Assets/Ships/Asura.asset`). Facing is the defensive decision. |
| **No boarding pods** | Boarding runs through the **hangar**: two assault boats with marines, each with a target and a job. A target must have been scanned first. The pod-as-a-weapon is dropped. |
| **No EXECUTE, no emergency abort** | The only time controls are **PAUSE** and **PLAY RATE**. There is no commit step and no order queue. Orders take effect as you give them. |
| **Real time with optional pause** | Pause is a convenience, not the core verb. The handoff's paused-time framing is wrong for this game. |
| **No broadside cannon** | The board has none. Since Step 170 (owner 2026-11-08) our ship has a spinal railgun: one gun fixed on the nose, aimed by turning the ship (`docs/SNIPER_PLAN.md`). The Garuda keeps its two swivel railguns. Soham's old `Weapons/BroadsideCannon.cs` and `Weapons/Railgun.cs` are kept as old code. The board does not use them. |
| **The Garuda's two railgun turrets** | Our ship has one fixed spinal railgun instead (row above). On the Garuda, RG-A covers 090° ±45° from the bow, RG-B 270° ±45°. One in the middle of each long side, on hard points you drag on the Hard Points tab of Starwolf → Tuning. A solid bar turns slowly onto the target inside each cone, and the shot flies down the bar. Reach 2,000 units, 22 rounds each. One shot per arming. The slug flies at 300 units/s, so it takes about 7 s to reach 2,000 units. |
| **Four PDC groups** | One at each corner of the ship, each covering 270°, so three cover any bearing. Reach 600 units, 2,400 rounds each. ARM puts a group on AUTO. It then fires by itself at enemy torpedoes entering its arc, with a 70% chance to kill each one. Each go is a stream of rounds that walks onto the torpedo on a kill (Step 197). They are the only answer to a torpedo salvo. |
| **Torpedoes** | Two launchers, VLS-A and VLS-B, one on each side, 8 torpedoes each. They fire in any direction, two to a salvo. A salvo leaves one torpedo at a time, 0.3 game seconds apart, from both launchers by turns, starting with the one on the target's side. A torpedo is pushed out of the ship's side, tilted 30° toward the stern. It drifts about 300 units with its motor off, slowing, while its nose swings onto the target. Then its motor bursts on with a flare, and it speeds up to full speed (Step 184). Reach 3,000 units, top speed 250 units/s: always slower than a slug. |
| **A volley is a consequence, not a mode** | There is no salvo button, no spread control, no fire pattern menu. Arm several turrets on the same bearing and they converge. Arm them on different bearings and you get a fan. The width of the fan is the difference between the numbers you typed. |
| **Density over time compression** | The empty-board problem is solved with a smaller, debris-dense arena, not by fast-forwarding. Modest play-rate range only. Collision is gameplay, not an error. |
| **Ten reactor units, four buses** | ENG · WEP · SEN · AUX, five units each at most. Ten units means **you can fully run two.** A bus on DRAW gains one unit per game second. A bus on VENT loses one. Every unit on a bus makes you louder. |
| **The plot is a square grid** | Not a circle. No elliptical mask anywhere. |
| **Colour is a code** | Shape says class, colour says allegiance, and a shape is never used for two sides. No new hues without extending the code. |
| **Panels do not pause the game** | The clock is universal and keeps running while you work a panel. |
| **The plot glyph never shows damage** | It stays a symbol. The periscope shows a target's damage. The Engineering panel shows ours on a picture of the hull. |
| **Everything runs in units and game seconds** | Ranges, speeds and the battle space are in units. At x1 a game second is a real second (Step 130). Every rate runs per game second, so x10 runs everything ten times as fast. |

The weapon, power and armour rows above match the ship stats files in `Assets/Ships/`.
The owner's rulings since 2026-10-04 are in `docs/BUILD_LOG.md`, section 4.

---

## 5 · How the owner wants to work

**Write in plain English. Short sentences. No jargon.** If a sentence needs a glossary, rewrite it.

**Structure every document the same way.** Start with a summary of what the game is. Then how it
plays. Then feature by feature, one system at a time. A reader should be able to stop after the
summary and still know what the game is.

**Every tunable number goes in the inspector. Never a literal in a method body.** Speeds, ranges,
timers, damage, layout constants, arena size — all of it. The owner tunes by hand. Art references
work the same way: swappable, nothing hard-wired. Each ship has one stats file in `Assets/Ships/`:
`Asura.asset` for our ship, `Garuda.asset` for the enemy cruiser and `Frigate.asset` for the Naga
frigates (`ShipStats`, read through `Ship.cs`). Edit them in **Starwolf → Tuning**, on the Ship and
Enemy tabs. The board's numbers are fields marked `[Tune]` on its scripts, on the Board tab. The keys
are on the Keys tab of **Starwolf → Tuning**. Each panel prefab has an **Art** list with its sprites (see
`docs/BOARD_UI.md`).

**Do not invent design to fill a gap.** If a document is silent, say it is open and stop. Mark it
`TBD` or put it in an "Open" list. Guessing and writing it down as if it were decided is the worst
thing you can do here.

**Do not scope a feature down to what the prototype happened to do.** The prototype is a floor, not a
ceiling.

---

## 6 · Where the work is right now

Where the game is today, and what comes next, is in `docs/BUILD_LOG.md`, section 1. This file does
not keep a status of its own.

**Read `docs/BUILD_LOG.md` before you touch the tactical scene.** The owner builds one step at a time
and checks each step on screen before the next one starts. Do one step, show it, wait.

The rest of the Unity build is an older attempt at a different UI, a six-screen 3D cockpit. It
decides nothing about design. `docs/CODEBASE_BASELINE.md` describes those older scripts, as they were
on 2026-09-01.

---

## 7 · What is genuinely still open

The open design questions are in `docs/BUILD_LOG.md`, section 8, "Open design questions". Do not treat
any of them as decided. Ask the owner.
