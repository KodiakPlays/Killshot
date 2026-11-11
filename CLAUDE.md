# CLAUDE.md

Operating rules for Killshot. **Frozen content only** — see [Scope](#scope-of-this-file).

## Purpose

<!-- OWNER-AUTHORED. Claude: do not write or edit this section. -->

_To be filled in by the project owner._

## Scope of this file

This file holds only **hardened, slow-changing rules**: do's, don'ts, patterns, anti-patterns.

Deliberately **not** here, because it would rot:

- Version numbers (engine, packages, SDK) — see the paths in Orientation
- Scene / GameObject / prefab / script inventories — query the live Editor over Unity MCP
- System designs and feature walkthroughs — see the docs in Orientation
- Current tasks, status, roadmap, or anything in flight

**Test before adding anything here: if a human has to remember to update it, it does not belong in this file.** Put it in the relevant doc and link the path instead.

## Orientation

**Building on the tactical board? Start with `docs/BUILD_LOG.md`.** It is the
pickup point: what exists, what is wired to what, which step we are on, and what to do next. It is
kept current as the single living record — do not start a second status document. Finished steps
and closed items are in `docs/BUILD_HISTORY.md`. Read it only when you need the past.

**For anything else, start with `docs/CONTEXT_INDEX.md`.** It routes you to the right document for
the question you have.

**A new chat for one piece of work?** Read `BUILD_LOG.md` section 1 (where we are) and section 8
(Open items, the list to work from). Each feature's plan file is linked from section 8 and from
`CONTEXT_INDEX.md`. The owner's standing decisions are in Claude's memory notes, which load by
themselves. Other chats share this Unity Editor: message them before you compile or enter play mode.

### The sources are a timeline, not rivals

```
GDD  →  HTML prototype  →  Silent Shark  →  the spec
```

**Newest wins.** What the owner says now beats everything. The prototype beats the GDD on how things behave and on every number. Never "correct" a newer choice back to an older source.

- **`References/Starwolf GDD.pdf`** — the oldest document, the original vision. **Read it for why the game exists, never for numbers.** Read it before designing anything. (`pdftotext` is not installed; use `uv run --with pypdf python`.)
- **`References/starwolf-prototype.html`** — that vision as working code. **How systems behave**, with real numbers.
- **Silent Shark** — a released tactical submarine sim the team is adapting toward. **How the game should be presented.** Not a change of direction; the GDD already asks for a control-panel game with no outside view.
- **The Unity build** — a partial, older attempt. It tells you what exists. **It decides nothing.**

A session once wrote the whole specification without opening the GDD. The result had no win condition and missed the game's central mechanic. Don't repeat it.

Read what's relevant before non-trivial work. These sources are **rolling** — treat them as current, and never mirror their contents into this file.

| What | Where |
|---|---|
| **What the game is, how to run it, the controls** | `README.md` |
| **Tactical board pickup point** — state, next step, traps | `docs/BUILD_LOG.md` |
| Finished steps and closed items | `docs/BUILD_HISTORY.md` |
| Finished plans | `docs/finished-plans/` |
| **Start here** — routes you to everything else | `docs/CONTEXT_INDEX.md` |
| **The specification** (how the game works) | `docs/TACTICAL_BOARD_SPEC.md` |
| **Verified code baseline** (every script read line by line) | `docs/CODEBASE_BASELINE.md` |
| **What every script does** | `docs/SCRIPT_LIST.md` |
| **Prototype baseline** (the playable build, read line by line) | `docs/PROTOTYPE_BASELINE.md` |
| Engine version | `ProjectSettings/ProjectVersion.txt` |
| Packages & render pipeline | `Packages/manifest.json` |
| Input backend, serialization mode | `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/EditorSettings.asset` |
| Live scene, play mode, compile state | Unity MCP resource `mcpforunity://editor/state` |
| Prototypes, key art, design references | `References/` |

Docs may lag the code. **Code and live Editor state win.**

### `References/`

`References/` holds prototypes, key art, the owner's hand-offs for Codex, and loose design notes the owner drops in.

**Killshot is not a port.** The HTML prototype is a starting point — it establishes core feel and mechanics, and getting it into Unity is an early step, not the goal. The finished game goes far beyond it. Read these references as a **floor, not a ceiling.**

- **Do** read the relevant reference before building or changing a system it covers. A prototype is the owner's intent expressed as working code — feel, pacing, numbers, UI layout, state flow.
- **Do** treat a prototype as a **specification of behaviour, not of implementation.** Rebuild the mechanic in idiomatic Unity and the conventions below; never transliterate its DOM, canvas, or global-variable structure into C#.
- **Do** treat references as a floor. Never scope a Unity feature down to what a reference happened to do, and never "correct" the Unity build back toward a prototype just because they differ.
- **Don't** assume either build is ahead — check per system. Unity and the prototype were built **in parallel**. Unity was intended to lead but is unfinished; the prototype is a fully playable experience and currently holds much of the core playability.
- **Don't** mirror a reference's contents into this file or into docs. Link the path.
- **Don't** edit anything in `References/`. It is owner-authored input, not a build artifact.

## Unity Editor (via MCP)

- **Do** read `mcpforunity://editor/state` and confirm `data.advice.ready_for_tools` is true before any mutating tool call.
- **Do** make scene, prefab, component, and asset changes through MCP tools rather than by hand.
- **Do** call `refresh_unity` after editing `.cs` files on disk, then check `read_console` for compile errors before continuing.
- **Don't** mutate project state while play mode is active or a compile / domain reload is in flight. Edits made during play mode are discarded on exit.
- **Don't** hand-edit `.unity`, `.prefab`, `.asset`, or `.mat` YAML. Drive the Editor instead.
- **Don't** build a resource URI by reshaping a resource *name* — `editor_state` is `mcpforunity://editor/state`. Look unknown ones up in the resource list.

## Repo hygiene

- **Do** commit the `.meta` file alongside every asset you add, move, or delete. A missing or orphaned `.meta` silently breaks references for everyone else.
- **Do** keep asset serialization on **Force Text**. Text YAML is what makes diffs and merges possible at all.
- **Don't** commit or edit generated and machine-local directories: `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`, `Build*/`.
- **Don't** edit `Assembly-CSharp.csproj`, `Assembly-CSharp-Editor.csproj`, or `Killshot.slnx`. Unity regenerates them. Git ignores the `.csproj` files; it tracks `Killshot.slnx`.
- **Don't** commit build outputs or archives. They belong outside the repo.
- **Don't** put a `.md` note inside `Assets/` (owner, 2026-11-10). Notes go in `docs/`. Add to a note that exists before making a new one.

## Keeping the project tidy (owner, 2026-11-11)

- **Do** commit your own finished work before you stop, using explicit paths (`git commit -- <paths>`). Several chats share one working folder, so never use a plain `git commit` and never leave finished files loose overnight.
- **Do** delete a job's working folders when the job ends (lists, verdicts, before and after copies). Keep only what the owner asked to keep.
- **Do** keep `BUILD_LOG.md` section 1, "Where we are", to one screen that is true today. When a step or an open item is finished, move its write-up to `docs/BUILD_HISTORY.md` in the same edit.
- **Do** move a finished plan doc to `docs/finished-plans/` and fix the links to it in `docs/CONTEXT_INDEX.md`.
- **Do** keep Claude's memory notes for the owner's lasting rules and decisions only. Build history goes in `BUILD_HISTORY.md`, not in memory. Add to a note that exists before making a new one, and keep `MEMORY.md` under its size limit.

## C# conventions

These describe the existing codebase. Match them; do not introduce a competing style.

- **Do** keep the plain-English note at the top of every script (owner, 2026-11-10): "What it does", then "How it works", then "Technical", above the `using` lines. When you change what a script does, update its note in the same edit. A new script gets a note too. A new or deleted script also gets its line added to or removed from `docs/SCRIPT_LIST.md`. The layout and the English rules are in `docs/script-notes/BRIEF.md`.

- **No namespaces.** Every type is in the global namespace, except the intro package (`Assets/StarwolfIntro`, namespace `Starwolf.Intro`). Don't add one to a new file.
- **No asmdefs.** Runtime code compiles into `Assembly-CSharp`; code under `Assets/Editor/` into `Assembly-CSharp-Editor`. Adding an `.asmdef` silently severs existing references — treat it as a deliberate architectural decision, not a tidy-up.
- **Prefer `[SerializeField] private`** over public fields for inspector-exposed state.
- **The board's damage goes through `DamageSystem`** (`Assets/Scripts/Systems/DamageSystem.cs`): armour, compartments, conditions, crew. Add new damage there. `IDamageable` (`Assets/Scripts/Interfaces/IDamageable.cs`) is for Soham's physics weapons and ships only.
- **The tactical board's weapons are in `Assets/Scripts/Systems/WeaponSystem.cs`** (owner, 2026-10-26): fire control, slugs, missiles and PDCs, worked out in km and game time. Add weapons there. Soham's `WeaponBase` / `WeaponManager` (shots as physics objects in real time) are kept but the board doesn't use them. `Weapons.cs` is the superseded monolith — don't extend it.
- **Editor-only code lives in `Assets/Editor/`.** Never `using UnityEditor;` from a runtime script outside `#if UNITY_EDITOR`.
- Use the existing `[ReadOnly]` attribute for inspector-visible but non-editable fields; the drawer already exists.

## Game numbers (owner rule, 2026-11-09)

- **Do** change a game number (a range, speed, damage, timer, rate, armour, layout) through what **Starwolf → Tuning** already shows: the ship stats files in `Assets/Ships`, and fields marked `[Tune]`. Look there first.
- **Do** write every number change in `docs/TUNING_LOG.md`: the date, the number, its **old value**, its new value, where it lives, and who asked. A change with no old value written down is not allowed. Nothing is lost.
- **Don't** put a game number in code where Tuning can't reach it. If the number has no Tuning field yet, add one (a stats-file field or `[Tune]`) and log it.

## Anti-patterns

- `FindObjectOfType` / `FindObjectsOfType` in `Update` or any per-frame path — cache in `Awake` / `Start`.
- `GetComponent` per frame — cache the reference.
- Hand-editing scene YAML to "just change one value."
- Picking an input API ad hoc. Both the legacy Input Manager and the new Input System are enabled, so both compile — check what the target scene already uses before adding to it.
- Copying a system doc's contents into code comments or into this file. Link the path instead.

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
