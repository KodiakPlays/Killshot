# NotebookLM source export

Renders part of the Killshot repo into a small set of NotebookLM-ready documents.

**What it covers.** The full source and the docs it exports are Soham's older code: the scripts
at the top of `Assets/Scripts/`, `Weapons/`, `Targeting/`, `Interfaces/`, `UI/UI_Script/` and the
top of `Assets/Editor/`. The tactical board's code is left out of the source bundles:
`Assets/Scripts/Systems/`, `Assets/Scripts/Tactical/`, `Assets/Scripts/UI/`,
`Assets/Scripts/Game/`, `Assets/StarwolfIntro/` and `Assets/Editor/Playtest/`. Only the API index
(`03-api-index.md`) lists the board's code. So the notebook does not describe the board as it is
now.

```sh
python3 Tools/notebooklm/export_sources.py
```

Output lands in `Build/notebooklm/` (gitignored — regenerate, don't commit).
No dependencies beyond Python 3.

## Why a generator and not an integration

NotebookLM has **no public API**. Nothing can push sources into a notebook
programmatically, and nothing outside Google can query one. So the loop is:
this script renders the files, you drag them into the notebook by hand.

That manual step is the whole integration. Everything else here exists to make
it cheap: one command, eight files, drop them in, delete the old ones.

## What it produces

| Source | Contents |
|---|---|
| `00-MANIFEST.md` | Build stamp — commit, branch, Unity version, dirty-tree warning — plus a reading guide for the notebook |
| `01-game-overview.md` | `STAKEHOLDER_DOCUMENTATION.md`: the April 2026 cockpit build's design |
| `02-code-architecture.md` | `DOCUMENTATION.md` + the weapon system READMEs: Soham's cockpit code |
| `03-api-index.md` | **Generated** from the C#: every type, base class, public member, and `[SerializeField]` field |
| `04-source-core.md` | Full source of `Assets/Scripts/*.cs` |
| `05-source-weapons.md` | Full source of `Weapons/`, `Targeting/`, `Interfaces/` |
| `06-source-ui-editor.md` | Full source of `UI/UI_Script/`, `Editor/`, `TestUIKeyControler.cs` |
| `07-project-config.md` | Unity version, packages, input backend, script inventory, and the scenes at the top of `Assets/Scenes/` and prefabs in `Assets/Prefabs/` (it misses `Scenes/Tactical/` and `Scenes/Levels/`) |

Every code section is preceded by a `FILE: <path>` line, so the notebook cites
real repository paths in its answers instead of vague section references.

`03-api-index.md` is the one source that cannot rot: it is extracted from the
code at generation time, so where the prose docs disagree with it, it wins. The
manifest tells the notebook this explicitly.

## Refreshing the notebook

1. Commit your work first — the manifest flags a dirty tree, and a notebook
   built from uncommitted files can't be traced back to any build.
2. Run the script.
3. In NotebookLM, delete the previous generated sources and upload the new
   `Build/notebooklm/*.md`. Leave the hand-authored PDFs in `References/` alone;
   they are owner-authored input and are not regenerated.

Worth doing after any change that moves the architecture — a new weapon, a new
system, a refactor that renames types. Not worth doing per commit.

## Scope limits

The C# scanner is regex-based and deliberately shallow. It maps the public
surface — types, bases, public and serialized members — and does not attempt to
parse expressions, generics constraints, or nested generic types precisely. The
full source bundles are there for anything the index summarises too coarsely.

Scene contents (GameObject hierarchies, component wiring, inspector values) are
**not** exported. That is in `.unity` YAML and is best queried live through
Unity MCP rather than flattened into a document that goes stale immediately.
