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

Read what's relevant before non-trivial work. These sources are **rolling** — treat them as current, and never mirror their contents into this file.

| What | Where |
|---|---|
| **Verified code baseline** (every script read line by line) | `Assets/CODEBASE_BASELINE.md` |
| Code structure & main systems | `Assets/DOCUMENTATION.md` |
| Game overview & design intent | `Assets/STAKEHOLDER_DOCUMENTATION.md` |
| Weapon system architecture | `Assets/Scripts/Weapons/WEAPON_SYSTEM_README.md` |
| Railgun setup | `Assets/Scripts/Weapons/RAILGUN_SETUP.md` |
| Engine version | `ProjectSettings/ProjectVersion.txt` |
| Packages & render pipeline | `Packages/manifest.json` |
| Input backend, serialization mode | `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/EditorSettings.asset` |
| Live scene, play mode, compile state | Unity MCP resource `mcpforunity://editor/state` |
| Prototypes, key art, design references | `References/` |

Docs may lag the code. **Code and live Editor state win.**

### `References/`

`References/` holds prototypes, key art, and loose design notes the owner drops in.

**Killshot is not a port.** The HTML prototype is a starting point — it establishes core feel and mechanics, and getting it into Unity is an early step, not the goal. The finished game goes far beyond it. Read these references as a **floor, not a ceiling.**

- **Do** read the relevant reference before building or changing a system it covers. A prototype is the owner's intent expressed as working code — feel, pacing, numbers, UI layout, state flow.
- **Do** treat a prototype as a **specification of behaviour, not of implementation.** Rebuild the mechanic in idiomatic Unity and the conventions below; never transliterate its DOM, canvas, or global-variable structure into C#.
- **Do** expect the Unity build to move past a reference and stay there. Where the two differ, the Unity build is usually **ahead** — never "correct" it back toward the prototype, and never scope a feature down to what the prototype happened to do. Ask the owner if the intent is genuinely unclear.
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
- **Don't** edit `Assembly-CSharp.csproj`, `Assembly-CSharp-Editor.csproj`, or `Killshot.slnx`. Unity regenerates them and git ignores them.
- **Don't** commit build outputs or archives. They belong outside the repo.

## C# conventions

These describe the existing codebase. Match them; do not introduce a competing style.

- **No namespaces.** Every type sits in the global namespace. Don't add one to a new file.
- **No asmdefs.** Runtime code compiles into `Assembly-CSharp`; code under `Assets/Editor/` into `Assembly-CSharp-Editor`. Adding an `.asmdef` silently severs existing references — treat it as a deliberate architectural decision, not a tidy-up.
- **Prefer `[SerializeField] private`** over public fields for inspector-exposed state.
- **Damage flows through `IDamageable`** (`Assets/Scripts/Interfaces/IDamageable.cs`). Anything damageable implements it; damage dealers depend on the interface, never on concrete types.
- **New weapons extend `WeaponBase`** and are driven by `WeaponManager`. `Weapons.cs` is the superseded monolith — don't extend it.
- **Editor-only code lives in `Assets/Editor/`.** Never `using UnityEditor;` from a runtime script outside `#if UNITY_EDITOR`.
- Use the existing `[ReadOnly]` attribute for inspector-visible but non-editable fields; the drawer already exists.

## Anti-patterns

- `FindObjectOfType` / `FindObjectsOfType` in `Update` or any per-frame path — cache in `Awake` / `Start`.
- `GetComponent` per frame — cache the reference.
- Hand-editing scene YAML to "just change one value."
- Picking an input API ad hoc. Both the legacy Input Manager and the new Input System are enabled, so both compile — check what the target scene already uses before adding to it.
- Copying a system doc's contents into code comments or into this file. Link the path instead.
