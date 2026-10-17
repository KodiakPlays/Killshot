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
| Why a design decision was made, and what was rejected | `Assets/TACTICAL_BOARD_PLAN.md` |
| What the board looks like — colour, type, spacing, markers | `Assets/UI2/Claude Design handoff/export/README.md`, then the older handoff zip's `README.md` |
| How each control on the new board behaves, state by state | `export/reference/UIStateSpec.md` and `export/reference/board-logic.js` |
| Where every element sits on the new board, in pixels | `export/layout/layout.json` |
| How the new handoff was built into `02.1_Grid`, step by step | `Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md` |
| What the handoff package left out | `Assets/Scenes/Tactical/HANDOFF_GAPS.md` |
| What Claude Design must still provide, verified | `Assets/Scenes/Tactical/CLAUDE_DESIGN_NEEDS.md` |
| Which of those needs our own assets can fill | `Assets/Scenes/Tactical/ASSET_AUDIT.md` |
| How the prototype actually behaves, without opening 4,472 lines | `Assets/PROTOTYPE_BASELINE.md` |
| What C# exists today | `Assets/CODEBASE_BASELINE.md` |
| What is built in the tactical scene, and the traps already hit | `Assets/Scenes/Tactical/BUILD_LOG.md` |
| Which canvas, camera, render texture and GameObject feeds each panel and button | `Assets/Scenes/Tactical/UICONTROLLER_PARTS.md` |
| What exists for weapons, tracks and contacts, and what still has to be built | `Assets/Scenes/Tactical/TARGETING_AUDIT.md` |
| How the targeting drawer is built, to the pixel | `Assets/Scenes/Tactical/TARGETING_COMPUTER_SPEC.md` |
| A mechanic that is already written, before you port it to C# | `docs/reference-sim/` |
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

**`References/UI Killshot documentation.pdf`** — 4 pages. The UI author's API contract for the old
`UIController`. Historical, tied to the cockpit UI that is being retired.

**Silent Shark** — not a file. A released tactical submarine sim the team is adapting toward. What we
take from it is written up in `Assets/TACTICAL_BOARD_PLAN.md`: the plot is inhabited not consulted,
instruments are stations not a HUD, information costs time, uncertainty is drawn. What we reject: its
10000× time compression.

### The new Claude Design handoff — `Assets/UI2/Claude Design handoff/export/`

The newer handoff, and the one built into `02.1_Grid`. The full board cut into images, fonts and
measurements. Treat it as owner input: read it, do not edit it.

- `README.md` — what the package is and how to use it.
- `reference/UIStateSpec.md` — every control's states.
- `reference/board-logic.js` — the board's behaviour, as code.
- `layout/layout.json` — where every element sits, in pixels.

### Project documents — ours, and editable.

**`Assets/TACTICAL_BOARD_SPEC.md`** — ~1,750 lines. **The specification.** What the game does, system
by system, with numbered rules. Start at §1 Summary and §2 How it plays; jump to a system after that.
*Not good for:* why. It deliberately omits reasoning.

**`Assets/TACTICAL_BOARD_PLAN.md`** — ~830 lines. The reasoning behind the spec: what was considered,
what was rejected, migration notes, traps. Read it when you want to know *why* before changing
something.
*Not good for:* current behaviour — the spec is the cleaner read for that.

**`Assets/PROTOTYPE_BASELINE.md`** — ~335 lines. The prototype read line by line, so you do not have
to open the 4,472-line HTML.

**`Assets/CODEBASE_BASELINE.md`** — ~1,690 lines. Every Unity script read line by line. Describes the
code as it is, not as it should be.

**`Assets/PARITY_PLAN.md`** — earlier plan to bring Unity to prototype parity. Its UI sections are
superseded by `TACTICAL_BOARD_PLAN.md`. Its systems work items still stand.

**`Assets/DOCUMENTATION.md`**, **`Assets/STAKEHOLDER_DOCUMENTATION.md`** — older Unity-era docs. Treat
as historical.

### The tactical scene — `Assets/Scenes/Tactical/`

**`BUILD_LOG.md`** — ~1,200 lines. The build history of the tactical board, step by step: what exists,
what is wired to what, which step we are on, what to do next. It ends with a long **Known issues**
list of traps already hit. **Read it before touching that scene.**

**`UICONTROLLER_PARTS.md`** — which `UIController` field, canvas, camera and render texture feeds each
panel and button.

**`TARGETING_AUDIT.md`** — what already exists for weapons, tracks and contacts, and what has to be
built. Short answer: almost nothing is reusable, and the mechanic is already written in
`docs/reference-sim/`.

**`UI_INTEGRATION_LOG.md`** — the full record of building the new handoff into `02.1_Grid`, Steps
28–33 and the final QA. `BUILD_LOG.md` stays the pickup point.

**`HANDOFF_GAPS.md`** — 105 rows of what the handoff package left out.

**`CLAUDE_DESIGN_NEEDS.md`** — the verified, bucketed list of everything Claude Design must still
provide. Checked against every source. The basis for the Claude Design request.

**`TARGETING_COMPUTER_SPEC.md`** — the measured build spec for the targeting drawer. Every pixel
figure comes from rendering the handoff's `Star Wolf Board Live.dc.html` in Chrome.

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

---

## 4 · DECISIONS ALREADY MADE — DO NOT REOPEN

These are settled, owner-confirmed. Do not re-argue them, do not "restore" them from an older
document, and do not quietly design around them.

| Decision | The rule |
|---|---|
| **No cockpit** | The 3D cockpit interior is gone. The plot is the whole window. Station panels slide over it and never replace it. |
| **No shields** | `Shields.cs` is removed, not reworked. **The hull side you present is your only armour** — prow 50, port 50, starboard 50, aft 40. Facing is the defensive decision. |
| **No boarding pods** | Boarding runs through the **hangar**: two assault boats with marines, each with a target and a job. A target must have been scanned first. The pod-as-a-weapon is dropped. |
| **No EXECUTE, no emergency abort** | The only time controls are **PAUSE** and **PLAY RATE**. There is no commit step and no order queue. Orders take effect as you give them. |
| **Real time with optional pause** | Pause is a convenience, not the core verb. The handoff's paused-time framing is wrong for this game. |
| **No broadside cannon, no spinal railgun** | Delete both. There is no gun that fires down the length of the ship. |
| **Three railgun turrets** | Two forward on the spine (rest `000°`, ±90°), one aft (rest `180°`, ±90°). One barrel each, armed one at a time, one shot per arming. Coverage is 360°; **firepower is two barrels forward, one aft.** |
| **Four PDCs, player-aimed** | One at each corner. You switch it on and point its **cone**; it then fires by itself at anything entering. ~80° cone, ~100° traverse. They are the only answer to a missile salvo. |
| **Missiles** | Fire in any direction, no arc. |
| **A volley is a consequence, not a mode** | There is no salvo button, no spread control, no fire pattern menu. Arm several turrets on the same bearing and they converge. Arm them on different bearings and you get a fan. The width of the fan is the difference between the numbers you typed. |
| **Density over time compression** | The empty-board problem is solved with a smaller, debris-dense arena, not by fast-forwarding. Modest play-rate range only. Collision is gameplay, not an error. |
| **Ten reactor points, five systems** | ENGINES · WEAPONS · SCANNER · COMMS · AUXILIARY, five points each. Ten points means **you can fully run two.** Points are moved, not spent, and moving them takes seconds. Every allocated point makes you louder. |
| **The plot is a square grid** | Not a circle. No elliptical mask anywhere. |
| **Colour is a code** | Shape says class, colour says allegiance, and a shape is never used for two sides. No new hues without extending the code. |
| **Panels do not pause the game** | The clock is universal and keeps running while you work a panel. |
| **Damage shows on the forward screen only** | The plot glyph never shows damage. It stays a symbol. |
| **Units are world units** | The kilometre figures in the handoff are display only. They do not set the numbers the game runs on. |

**Owner rulings on 2026-10-04 override parts of this table** for the new board: the new handoff wins
on weapons and power, and distances are kilometres. See `Assets/Scenes/Tactical/BUILD_LOG.md`,
section 4.

---

## 5 · How the owner wants to work

**Write in plain English. Short sentences. No jargon.** If a sentence needs a glossary, rewrite it.

**Structure every document the same way.** Start with a summary of what the game is. Then how it
plays. Then feature by feature, one system at a time. A reader should be able to stop after the
summary and still know what the game is.

**Every tunable number goes in the inspector. Never a literal in a method body.** Speeds, ranges,
timers, damage, layout constants, arena size — all of it. The owner tunes by hand. That means a
`ScriptableObject` tuning profile — see `Assets/Scripts/Tactical/TacticalTuning.cs`. Art references
work the same way — a swappable profile, `Assets/Scripts/Tactical/TacticalArtProfile.cs`, nothing
hard-wired.

**Do not invent design to fill a gap.** If a document is silent, say it is open and stop. Mark it
`TBD` or put it in an "Open" list. Guessing and writing it down as if it were decided is the worst
thing you can do here.

**Do not scope a feature down to what the prototype happened to do.** The prototype is a floor, not a
ceiling.

---

## 6 · Where the work is right now

**The current effort is the Unity tactical board, in `Assets/Scenes/Tactical/`.**

The flow is `00_Start` → `01_Briefing` → `02.1_Grid`. The new Claude Design handoff is fully built
into `02.1_Grid` and passed its final QA. Every control is clickable and shows its states. Nothing is
wired to game systems yet. The older board in `02_Grid` is still in Build Settings, but nothing leads
to it. Next: an audit of existing UI assets against `CLAUDE_DESIGN_NEEDS.md`, then the Claude Design
request, then wiring.

**Read `Assets/Scenes/Tactical/BUILD_LOG.md` before you touch that scene.** The owner builds one step
at a time and checks each step on screen before the next one starts. Do one step, show it, wait.

The rest of the Unity build is an older attempt at a different UI — a six-screen 3D cockpit. It
decides nothing about design. `Assets/CODEBASE_BASELINE.md` records what C# exists.

There was also a web prototype in TypeScript. It was abandoned and deleted. Its simulation core was
kept at `docs/reference-sim/`, as reference only.

Placeholder hull art exists at `Assets/Art/Hulls/` — white with alpha, tinted per allegiance in
engine. Placeholder. Replace before ship.

---

## 7 · What is genuinely still open

Do not treat any of these as decided. Ask the owner.

**Blocking**

1. **Enemy AI.** Unspecified in every document, including the GDD. The largest hole in the design.
2. **What firing a railgun really costs.** §8.4 says firing sets every powered system to vent. If
   that is true, a three-turret volley is impossible and the volley rules are wrong. One of the two
   has to give.
3. **Where a drawn power point comes from.** Is there an unallocated pool? Must a point be vented
   from somewhere else first? What happens when you press draw with all ten already placed?
4. **What happens when stability reaches zero.** Four sections spend numbers on it; none says what it
   is for.
5. **World units to plot pixels.** Nothing maps them, so nobody can say what zoom 1.0 shows.
6. **PDC aiming** has no written section, and **missiles have no control surface** — nothing says
   where they are fired from.
7. **Arena size.** Needs a first number. Grid ticks and zoom range derive from it.

**Not blocking**

8. Squadron size, fuel and ammo scales. Whether fighters need orders beyond a target. Whether craft
   must be recovered to rearm. Whether losses persist between missions.
9. Whether the stern stays a glass jaw (GDD: 10 hp) or the softer prototype value (40).
10. Whether starbase hulks are destructible, and whether terrain blocks radar, sensors, or both.
11. Whether a dry trigger pull should wake the alert net.
12. What goes in the Engineering panel, and what replaces the freed shield readout.
13. Who draws the real hulls. Two tiers per class — a detailed silhouette and a separately drawn
    plot glyph — is the largest art commitment in the project. Starbase wreckage art does not exist
    at all.
