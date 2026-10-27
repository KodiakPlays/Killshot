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
| 2 | `Assets/UI2/Claude Design handoff/export/` (the newer Claude Design handoff) | The visual system, and the board built into `02.1_Grid`. The owner ruled it wins on weapons, power and distances |
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
| What a system does and what number it uses | `Assets/TACTICAL_BOARD_SPEC.md`, then `References/starwolf-prototype.html` |
| What the board looks like — colour, type, spacing, markers | `Assets/UI2/Claude Design handoff/export/README.md`, then the older handoff zip's `README.md`. Two of the export README's rules are wrong in this project: sprites are imported with sRGB **off**, and colours are not typed in as the token's hex. |
| How each control on the new board behaves, state by state | `export/reference/UIStateSpec.md` and `export/reference/board-logic.js` |
| Where every element is on the new board, in pixels | `export/layout/layout.json` |
| What the handoff package left out | `Assets/Scenes/Tactical/HANDOFF_GAPS.md` |
| What Claude Design must still provide, verified | `Assets/Scenes/Tactical/CLAUDE_DESIGN_NEEDS.md` |
| How the prototype actually behaves, without opening 4,472 lines | `Assets/PROTOTYPE_BASELINE.md` |
| What the game is, how to run it, the controls | `README.md` (repo root) |
| What Soham's older scripts do, read line by line as of 2026-09-01 | `Assets/CODEBASE_BASELINE.md` |
| What the board's scripts are and what they are wired to | `Assets/Scenes/Tactical/BUILD_LOG.md`, section 3 |
| What is built in the tactical scene, and the traps already hit | `Assets/Scenes/Tactical/BUILD_LOG.md` |
| A ship's numbers: speed, power, weapons, armour | `Assets/Ships/Asura.asset` (our ship) and `Assets/Ships/Garuda.asset` (the enemy). Edit them in **Starwolf → Tuning**, on the Ship and Enemy tabs. |
| How damage works: armour, rooms, fire, vacuum, radiation | `Assets/Scenes/Tactical/DAMAGE_README.md` |
| How boarding works: the assault boats and their orders | `Assets/Scenes/Tactical/BOARDING_README.md` |
| The design audit the damage model is built on | `References/Star Wolf - Design Audit.dc.html` |
| How to make or change a level | Menu **Starwolf → Level** (`Assets/Editor/LevelWindow.cs`). The levels are in `Assets/Scenes/Levels/`. |
| Whether a change to speeds, ranges or weapons still gives a fight of the right length | `docs/scale-sim/README.md` |
| How to turn the repo into NotebookLM sources | `Tools/notebooklm/README.md` |
| How the green phosphor screen works (intro, welcome page, INITIALISING), and how to put it on a new screen | `Assets/StarwolfIntro/README.md` |
| The first mission, the tutorial walkthrough, and the first enemy AI — the plan | `Assets/Scenes/Tactical/FTUE_PLAN.md` |
| A mechanic that is already written, before you port it to C# | `docs/reference-sim/` |
| Where a panel's prefab and art are, and how to swap its art | `Assets/UI/Board/README.md` |
| Spoken orders, and how an outside program (or Claude) could drive the built game — an idea, not built | `docs/voice-command-exploration.md` |
| Ship silhouettes, class size table, obstacle tiers | `References/Starwolf_ Art Spec.pdf` |
| Working rules — MCP, C# style, repo hygiene | `CLAUDE.md` |

---

## 3 · The documents, one by one

### References — owner-authored input. Never edit anything in `References/`.

**`References/Tactical spaceship UI planning.zip`** — the older Claude Design handoff. **The newer
export in `Assets/UI2/Claude Design handoff/export/` is the one built into `02.1_Grid`.** A 430-line README
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

### The new Claude Design handoff — `Assets/UI2/Claude Design handoff/export/`

The newer handoff, and the one built into `02.1_Grid`. The full board cut into images, fonts and
measurements. Read it, but don't change what's in it. The art the board uses is being moved out of it,
one panel at a time, into `Assets/UI/Board/Art/` (owner, 2026-10-26). Once everything is out, the
owner deletes the export. See `Assets/UI/Board/README.md`.

- `README.md` — what the package is and how to use it.
- `reference/UIStateSpec.md` — every control's states.
- `reference/board-logic.js` — the board's behaviour, as code.
- `layout/layout.json` — where every element is, in pixels.

### Project documents — ours, and editable.

**`README.md`** (repo root) — what the game is, how to run it, and the controls. Short.

**`Assets/TACTICAL_BOARD_SPEC.md`** — ~1,750 lines. **The specification.** What the game does, system
by system, with numbered rules. Start at §1 Summary and §2 How it plays; jump to a system after that.
*Not good for:* why. It deliberately omits reasoning.

**`Assets/PROTOTYPE_BASELINE.md`** — ~335 lines. The prototype read line by line, so you do not have
to open the 4,472-line HTML.

**`Assets/CODEBASE_BASELINE.md`** — ~1,700 lines. Soham's older scripts read line by line, as they
were on 2026-09-01. The board's code in `Assets/Scripts/Systems/`, `Tactical/`, `UI/`, `Game/` and
`Assets/StarwolfIntro/` is newer and is not in it. `BUILD_LOG.md` section 3 describes the board's code.

### The tactical scene — `Assets/Scenes/Tactical/`

**`BUILD_LOG.md`** — ~4,500 lines. The build history of the tactical board, step by step: what exists,
what is wired to what, which step we are on, what to do next. It ends with a long **Known issues**
list of traps already hit. **Read it before touching that scene.**

**`HANDOFF_GAPS.md`** — 106 rows of what the handoff package left out.

**`CLAUDE_DESIGN_NEEDS.md`** — the verified, bucketed list of everything Claude Design must still
provide. Checked against every source. The basis for the Claude Design request.

**`DAMAGE_README.md`** — how damage works. Armour on each face, the rooms behind it, fire, vacuum and
radiation, and the Engineering buttons that deal with them. A reactor at 0 destroys any ship.

**`BOARDING_README.md`** — how boarding works. The assault boats take marines to a nearly stopped
enemy with an open face. If the marines win, they DESTROY, DISABLE or SECURE the ship.

### Levels — `Assets/Scenes/Levels/`

Each level is a small scene. The board loads one on top of itself at play (`TacticalWorld.cs`,
`levelScene`). `Level_01.unity` is the first. The spawn points, the debris fields and the broken
starbase are placed in the level. Make and edit levels with the menu **Starwolf → Level** (`Assets/Editor/LevelWindow.cs`).
The Level tool is described in `BUILD_LOG.md` section 3.

### `docs/reference-sim/` — reference only. Nothing compiles it.

The simulation core of the shelved TypeScript prototype. The rest of that project was broken and has
been deleted. This part was kept because it already expresses the mechanics that were agreed.

- `weapons.ts` — turrets with `arcCentre`, `halfArc`, and a state of `empty | arming | armed`. Each
  turret has its own `aimBearing`. `fire()` disarms it, so one arming is one shot, and a volley is a
  consequence rather than a button.
- `radar.ts` — the full track model: `resolved`, `lastFix`, estimated heading and speed, confidence,
  uncertainty, two-bearing triangulation.
- `ship.ts`, `power.ts`, `tuning.ts`, `geometry.ts`, `world.ts` — the supporting model.
- `README.md` — what each file is and why it was kept.

Read it when porting a mechanic to C#. Do not re-derive these from scratch.

**`CLAUDE.md`** — how to work in this repo: Unity MCP rules, C# conventions, repo hygiene. Not design.

### `docs/scale-sim/README.md`

A small Python script that plays whole fights with the board's rules and numbers. It reports how long
a win takes and how many shots are traded. It set the 5× speed scale in Step 63. Re-run it after any
change to speeds, ranges, weapons or the enemy.

### `Tools/notebooklm/README.md`

A Python script that turns the repo into a few documents to load into NotebookLM. The output goes to
`Build/notebooklm/`, which git ignores.

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
| **No broadside cannon, no spinal railgun** | The board has neither. There is no gun that fires down the length of the ship. Soham's old `Weapons/BroadsideCannon.cs` and `Weapons/Railgun.cs` are kept as old code. The board does not use them. |
| **Two railgun turrets** | RG-A covers 042° ±80° from the bow. RG-B covers 250° ±80°. One each side. Reach 25,000 km, 22 rounds each. One shot per arming. The slug flies at 2,500 km/s, so it takes about 10 s to reach 25,000 km. |
| **Two PDC groups** | PDC-A covers 042° ±35° from the bow. PDC-B covers 200° ±35°. Reach 10,000 km, 2,400 rounds each. ARM puts a group on AUTO. It then fires by itself at enemy missiles entering its arc, with a 70% chance to kill each one. They are the only answer to a missile salvo. |
| **Missiles** | Fire in any direction, no arc. Fired from the VLS on the Fire Control panel, two to a salvo. Reach 35,000 km, 8 missiles. |
| **A volley is a consequence, not a mode** | There is no salvo button, no spread control, no fire pattern menu. Arm several turrets on the same bearing and they converge. Arm them on different bearings and you get a fan. The width of the fan is the difference between the numbers you typed. |
| **Density over time compression** | The empty-board problem is solved with a smaller, debris-dense arena, not by fast-forwarding. Modest play-rate range only. Collision is gameplay, not an error. |
| **Ten reactor units, four buses** | ENG · WEP · SEN · AUX, five units each at most. Ten units means **you can fully run two.** A bus on DRAW gains one unit per game second. A bus on VENT loses one. Every unit on a bus makes you louder. |
| **The plot is a square grid** | Not a circle. No elliptical mask anywhere. |
| **Colour is a code** | Shape says class, colour says allegiance, and a shape is never used for two sides. No new hues without extending the code. |
| **Panels do not pause the game** | The clock is universal and keeps running while you work a panel. |
| **The plot glyph never shows damage** | It stays a symbol. The periscope shows a target's damage. The Engineering panel shows ours on a picture of the hull. |
| **Everything runs in kilometres and game seconds** | Ranges, speeds and the battle space are in km. Every rate runs per game second, so x10 runs everything ten times as fast. |

The weapon, power and armour rows above match the code and the ship stats files in `Assets/Ships/`.
The owner's rulings since 2026-10-04 are in `Assets/Scenes/Tactical/BUILD_LOG.md`, section 4.

---

## 5 · How the owner wants to work

**Write in plain English. Short sentences. No jargon.** If a sentence needs a glossary, rewrite it.

**Structure every document the same way.** Start with a summary of what the game is. Then how it
plays. Then feature by feature, one system at a time. A reader should be able to stop after the
summary and still know what the game is.

**Every tunable number goes in the inspector. Never a literal in a method body.** Speeds, ranges,
timers, damage, layout constants, arena size — all of it. The owner tunes by hand. Art references
work the same way: swappable, nothing hard-wired. Each ship has one stats file: `Assets/Ships/Asura.asset`
for our ship and `Assets/Ships/Garuda.asset` for the enemy (`ShipStats`, read through `Ship.cs`). Edit
them in **Starwolf → Tuning**, on the Ship and Enemy tabs. The board's numbers are fields marked
`[Tune]` on its scripts, on the Board tab. Each panel prefab has an **Art** list with its sprites (see
`Assets/UI/Board/README.md`).

**Do not invent design to fill a gap.** If a document is silent, say it is open and stop. Mark it
`TBD` or put it in an "Open" list. Guessing and writing it down as if it were decided is the worst
thing you can do here.

**Do not scope a feature down to what the prototype happened to do.** The prototype is a floor, not a
ceiling.

---

## 6 · Where the work is right now

**The current effort is the Unity tactical board, in `Assets/Scenes/Tactical/`.**

The flow is `00_Start` → `01_Briefing` → `02.1_Grid`. The new Claude Design handoff is fully built
into `02.1_Grid` and passed its final QA. Every control is clickable and shows its states. Wiring to
game systems started in Step 34 and is ongoing. The older
`02_Grid` board has been deleted. At play the board loads a level on top of itself, `Assets/Scenes/Levels/Level_01.unity`.
For the current step, read section 1 of `BUILD_LOG.md`.

**Read `Assets/Scenes/Tactical/BUILD_LOG.md` before you touch that scene.** The owner builds one step
at a time and checks each step on screen before the next one starts. Do one step, show it, wait.

The rest of the Unity build is an older attempt at a different UI — a six-screen 3D cockpit. It
decides nothing about design. `Assets/CODEBASE_BASELINE.md` describes those older scripts, as they were
on 2026-09-01.

There was also a web prototype in TypeScript. It was abandoned and deleted. Its simulation core was
kept at `docs/reference-sim/`, as reference only.

Placeholder hull art exists at `Assets/Art/Hulls/` — white with alpha, tinted per allegiance in
engine. Placeholder. Replace before ship.

---

## 7 · What is genuinely still open

Do not treat any of these as decided. Ask the owner.

**Blocking**

1. **Enemy AI beyond the first brain.** `Assets/Scripts/Tactical/EnemyBrain.cs` patrols a loop of
   waypoints. When we hit it, it fires 2 missiles back, at most once every 120 s. How an enemy hears
   us, hunts us and reacts is still unspecified in every document, including the GDD.
2. **What happens when stability reaches zero.** Four sections spend numbers on it; none says what it
   is for. On the board, stability is a meter on the helm, and nothing else reads it.

**Not blocking**

3. Squadron size and fuel. Whether fighters need orders beyond a target. Whether craft must be
   recovered to rearm. Whether losses persist between missions.
4. Whether starbase hulks are destructible, and whether terrain blocks DRADIS.
5. Whether a dry trigger pull should wake the alert net.
6. Who draws the real hulls. Two tiers per class — a detailed silhouette and a separately drawn
   plot glyph — is the largest art commitment in the project. The broken starbase is placeholder
   shapes for now (`Assets/Prefabs/Terrain/BaseHusk.prefab`).

**Settled by the code since this list was first written:**

- Firing a railgun spends 70 of the weapon charge and adds 25 to our signal. Nothing vents.
- A bus on DRAW takes units the reactor has spare. With all ten units placed, it waits until another
  bus vents.
- The plot shows 60.9 px per 1,000 km at zoom 1 (`HandoffPlotPanel.cs`, `pxPer1000Km`).
- PDC groups have fixed arcs and fire by themselves on AUTO. Missiles fire from the VLS on the Fire
  Control panel.
- Each level sets its battle space with a Battle Space box (Starwolf → Level, Step 91). Without one it is 100,000 × 100,000 km (`ShipState.arenaWidthKm`).
- Ammunition is a real count in the ship stats files.
- The aft armour is 50.
- The Engineering panel is built (`Assets/Scripts/UI/Panels/EngineeringPanel.cs`).
