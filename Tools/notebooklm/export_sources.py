#!/usr/bin/env python3
"""Render the Killshot repo into NotebookLM-ready source documents.

NotebookLM has no upload API, so this produces a small set of self-contained
files you drag into a notebook by hand. Re-run it after meaningful build
changes and replace the sources in the notebook.

    python3 Tools/notebooklm/export_sources.py

Output: Build/notebooklm/ (gitignored). See README.md in this folder.
"""

import argparse
import os
import re
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
DEFAULT_OUT = REPO / "Build" / "notebooklm"

# Source files that are ours, as opposed to vendored or generated.
CODE_EXCLUDE = ("TextMesh Pro", "Assets/Plugins/", "Library/", "Temp/")

BUNDLES = [
    # (output stem, title, [globs relative to repo])
    ("04-source-core", "Core Gameplay Source", ["Assets/Scripts/*.cs"]),
    ("05-source-weapons", "Weapons, Targeting & Interfaces Source",
     ["Assets/Scripts/Weapons/*.cs", "Assets/Scripts/Targeting/*.cs",
      "Assets/Scripts/Interfaces/*.cs"]),
    ("06-source-ui-editor", "UI & Editor Source",
     ["Assets/UI/UI_Script/*.cs", "Assets/Editor/*.cs", "Assets/TestUIKeyControler.cs"]),
]


# --------------------------------------------------------------------------
# repo facts
# --------------------------------------------------------------------------

def git(*args, default=""):
    try:
        return subprocess.run(["git", "-C", str(REPO), *args],
                              capture_output=True, text=True, check=True).stdout.strip()
    except (subprocess.CalledProcessError, FileNotFoundError):
        return default


def unity_version():
    f = REPO / "ProjectSettings" / "ProjectVersion.txt"
    if not f.exists():
        return "unknown"
    m = re.search(r"m_EditorVersion:\s*(\S+)", f.read_text())
    return m.group(1) if m else "unknown"


def packages():
    f = REPO / "Packages" / "manifest.json"
    if not f.exists():
        return []
    import json
    deps = json.loads(f.read_text()).get("dependencies", {})
    return sorted(deps.items())


def input_backend():
    f = REPO / "ProjectSettings" / "ProjectSettings.asset"
    if not f.exists():
        return "unknown"
    m = re.search(r"activeInputHandler:\s*(\d+)", f.read_text(errors="ignore"))
    return {"0": "Legacy Input Manager only",
            "1": "New Input System only",
            "2": "Both (legacy + new Input System)"}.get(m.group(1) if m else "", "unknown")


def code_files():
    out = []
    for p in sorted(REPO.rglob("*.cs")):
        rel = p.relative_to(REPO).as_posix()
        if any(x in rel for x in CODE_EXCLUDE):
            continue
        out.append(rel)
    return out


# --------------------------------------------------------------------------
# lightweight C# surface extraction
# --------------------------------------------------------------------------

STRIP_STRINGS = re.compile(r'@?"(?:[^"\\]|\\.)*"|\'(?:[^\'\\]|\\.)*\'')
TYPE_DECL = re.compile(
    r'^\s*(?:\[[^\]]*\]\s*)*'
    r'(?P<mods>(?:public|internal|private|protected|abstract|sealed|static|partial|readonly|record|\s)*?)'
    r'\b(?P<kind>class|struct|interface|enum)\s+(?P<name>\w+)'
    r'\s*(?:<[^>]*>)?\s*(?::\s*(?P<bases>[^{]+?))?\s*(?:\{|$)')
MEMBER = re.compile(
    r'^\s*(?P<mods>(?:public|private|protected|internal|static|virtual|override|abstract|sealed|'
    r'async|extern|new|readonly|const|unsafe|partial|event|\s)*)'
    r'(?P<type>[\w<>\[\],\.\?]+)\s+(?P<name>\w+)\s*(?P<tail>[\(\{=;])')
UNITY_MESSAGES = {"Awake", "Start", "Update", "FixedUpdate", "LateUpdate", "OnEnable",
                  "OnDisable", "OnDestroy", "OnCollisionEnter", "OnCollisionEnter2D",
                  "OnTriggerEnter", "OnTriggerEnter2D", "OnTriggerExit2D", "OnGUI",
                  "OnDrawGizmos", "OnDrawGizmosSelected", "OnValidate"}
KEYWORDS = {"if", "for", "foreach", "while", "switch", "return", "using", "else",
            "do", "try", "catch", "finally", "lock", "case", "get", "set", "new"}


ATTR = re.compile(r'^\[[^\]]*\]\s*')


def _strip_attrs(text):
    """Peel leading [Attribute(...)] groups off a declaration, returning
    (attributes, remainder). Unity puts them inline as often as on their own line."""
    attrs = []
    while True:
        m = ATTR.match(text)
        if not m:
            return attrs, text
        attrs.append(m.group(0).strip())
        text = text[m.end():]


def _clean_doc(parts):
    """Flatten an XML doc comment into a sentence, dropping the tag scaffolding."""
    text = " ".join(parts)
    text = re.sub(r'</?summary>|</?remarks>', ' ', text)
    text = re.sub(r'<param name="([^"]*)">', r' (param \1: ', text)
    text = re.sub(r'<returns>', ' (returns: ', text)
    text = re.sub(r'</param>|</returns>', ') ', text)
    text = re.sub(r'<[^>]+>', ' ', text)
    return " ".join(text.split()).strip()


def _trim_sig(sig, kind):
    """Drop a member's body so the index stays a surface map, not a code dump."""
    if kind in ("method", "unity message", "property"):
        for marker in ("=>", "{"):
            i = sig.find(marker)
            if i > 0:
                return sig[:i].strip()
    return sig.rstrip("{;").strip()


def scan_csharp(path: Path):
    """Return [{name, kind, bases, doc, members:[...]}] for one file. Regex-based
    and deliberately shallow: it maps the public surface, it is not a parser."""
    types, stack = [], []
    depth = 0
    pending_attrs, pending_doc = [], []
    in_block_comment = False

    for raw in path.read_text(errors="ignore").splitlines():
        # `code` blanks string literals so braces inside them cannot skew the
        # depth count; `disp` keeps them so [Header("...")] survives into output.
        code, disp = STRIP_STRINGS.sub('""', raw.rstrip()), raw.rstrip()

        if in_block_comment:
            if "*/" in code:
                in_block_comment = False
                code, disp = code.split("*/", 1)[1], disp.split("*/", 1)[1]
            else:
                continue
        if "/*" in code:
            before, _, after = code.partition("/*")
            if "*/" in after:
                code = before + after.split("*/", 1)[1]
                b, _, a = disp.partition("/*")
                disp = b + a.split("*/", 1)[1] if "*/" in a else b
            else:
                in_block_comment = True
                code, disp = before, disp.partition("/*")[0]

        if code.strip().startswith("///"):
            pending_doc.append(raw.strip().lstrip("/ ").strip())
            continue
        code = re.sub(r'//.*$', '', code)
        disp = re.sub(r'//.*$', '', disp)
        stripped = code.strip()

        if stripped.startswith("[") and stripped.endswith("]"):
            pending_attrs.append(disp.strip())
            continue

        if stripped:
            m = TYPE_DECL.match(code)
            if m and m.group("name") not in KEYWORDS:
                types.append({
                    "name": m.group("name"),
                    "kind": m.group("kind"),
                    "mods": " ".join(m.group("mods").split()),
                    "bases": " ".join((m.group("bases") or "").split()),
                    "doc": _clean_doc(pending_doc),
                    "attrs": list(pending_attrs),
                    "members": [],
                    "depth": depth,
                    "entered": False,
                })
                stack.append(types[-1])
                pending_attrs, pending_doc = [], []
            elif stack and stack[-1]["entered"] and depth == stack[-1]["depth"] + 1:
                if stack[-1]["kind"] == "enum":
                    ev = re.match(r'^\s*(\w+)\s*(?:=\s*[^,]+)?\s*,?\s*$', code)
                    if ev:
                        stack[-1]["members"].append({
                            "kind": "value", "sig": disp.strip().rstrip(","),
                            "doc": _clean_doc(pending_doc), "attrs": []})
                    pending_attrs, pending_doc = [], []
                    depth += code.count("{") - code.count("}")
                    continue

                inline, body = _strip_attrs(stripped)
                inline_disp, body_disp = _strip_attrs(disp.strip())
                attrs = pending_attrs + inline_disp
                mm = MEMBER.match(body)
                if mm and mm.group("name") not in KEYWORDS and mm.group("type") not in KEYWORDS:
                    mods = " ".join(mm.group("mods").split()).split()
                    serialized = any("SerializeField" in a for a in attrs)
                    # Interface members carry no access modifier but are public.
                    public = (stack[-1]["kind"] == "interface"
                              or any(k in mods for k in ("public", "protected", "internal")))
                    tail = mm.group("tail")
                    kind = {"(": "method", "{": "property"}.get(tail, "field")
                    if kind == "method" and mm.group("name") in UNITY_MESSAGES:
                        kind = "unity message"
                    if public or serialized or kind == "unity message":
                        stack[-1]["members"].append({
                            "kind": kind,
                            "sig": _trim_sig(" ".join(body_disp.split()), kind),
                            "doc": _clean_doc(pending_doc),
                            "attrs": [a for a in attrs
                                      if any(k in a for k in ("SerializeField", "Header",
                                                              "Range", "Tooltip", "ReadOnly"))],
                        })
                pending_attrs, pending_doc = [], []
            else:
                pending_attrs, pending_doc = [], []

        depth += code.count("{") - code.count("}")
        if stack and not stack[-1]["entered"] and depth > stack[-1]["depth"]:
            stack[-1]["entered"] = True
        while stack and stack[-1]["entered"] and depth <= stack[-1]["depth"]:
            stack.pop()

    return types


# --------------------------------------------------------------------------
# document rendering
# --------------------------------------------------------------------------

def header(title, stamp, note=""):
    return (f"# Killshot — {title}\n\n"
            f"> Generated from the Killshot Unity repository.\n"
            f"> {stamp}\n"
            + (f">\n> {note}\n" if note else "")
            + "\n---\n\n")


def write(out_dir, stem, text):
    p = out_dir / f"{stem}.md"
    p.write_text(text)
    words = len(text.split())
    return p, words


def build_manifest(stamp_lines, files_written, dirty):
    body = ["# Killshot — Source Bundle Manifest\n",
            "This notebook's sources were generated from the Killshot Unity repository. "
            "Read this file first: it says exactly which build state the other sources "
            "describe. If the notebook's answers disagree with the game, check the commit "
            "below against the current repo.\n",
            "## Build state\n"]
    body += [f"- {l}" for l in stamp_lines]
    if dirty:
        body.append("")
        body.append("> **Warning — the working tree was dirty when these sources were generated.**")
        body.append("> The following files had uncommitted changes, so their content here does "
                    "not correspond to any commit:")
        body.append("")
        body += [f">   - `{f}`" for f in dirty[:40]]
        if len(dirty) > 40:
            body.append(f">   - …and {len(dirty) - 40} more")
    body.append("\n## Sources in this bundle\n")
    body.append("| File | Contents | Words |")
    body.append("|---|---|---|")
    for stem, title, words in files_written:
        body.append(f"| `{stem}.md` | {title} | {words:,} |")
    body.append("\n## How to read these sources\n")
    body.append("- **API index** is generated directly from the C# and is always in sync with "
                "the commit above.")
    body.append("- **Source bundles** are the complete C# text, so answers can quote real code.")
    body.append("- Every code section is preceded by a `FILE:` line giving its repository path. "
                "Cite that path when answering questions about where something lives.")
    return "\n".join(body) + "\n"


def main():
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("-o", "--out", type=Path, default=DEFAULT_OUT,
                    help=f"output directory (default: {DEFAULT_OUT.relative_to(REPO)})")
    args = ap.parse_args()

    out = args.out
    out.mkdir(parents=True, exist_ok=True)
    for old in out.glob("*.md"):
        old.unlink()

    now = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M UTC")
    sha, branch = git("rev-parse", "--short", "HEAD", default="unknown"), git("rev-parse", "--abbrev-ref", "HEAD", default="unknown")
    subject = git("log", "-1", "--pretty=%s", default="")
    commit_date = git("log", "-1", "--pretty=%cd", "--date=short", default="")
    dirty = [l[3:].strip().strip('"') for l in git("status", "--porcelain").splitlines()
             if l and not l.startswith("??")]

    stamp_lines = [
        f"Generated: {now}",
        f"Commit: `{sha}` on `{branch}` — {subject} ({commit_date})",
        f"Unity: {unity_version()}",
        f"Input backend: {input_backend()}",
        f"Working tree: {'DIRTY — ' + str(len(dirty)) + ' modified file(s)' if dirty else 'clean'}",
    ]
    stamp = " · ".join([f"Commit `{sha}` ({branch})", f"Unity {unity_version()}", now])

    written = []

    def emit(stem, title, text):
        _, words = write(out, stem, text)
        written.append((stem, title, words))

    # 03 — generated API index ----------------------------------------------
    files = code_files()
    parts = [header("Generated API Index", stamp,
                    "Extracted directly from the C# at the commit above. Lists every type, "
                    "its base classes, its public members, and its inspector-serialized "
                    "fields. Authoritative for 'what exists' and 'where does it live'.")]
    total_types = 0
    for rel in files:
        types = scan_csharp(REPO / rel)
        if not types:
            continue
        parts.append(f"\n## FILE: {rel}\n")
        for t in types:
            total_types += 1
            bases = f" : {t['bases']}" if t["bases"] else ""
            parts.append(f"\n### `{t['name']}` — {t['mods'] + ' ' if t['mods'] else ''}"
                         f"{t['kind']}{bases}\n")
            parts.append(f"- Defined in: `{rel}`\n")
            if t["doc"]:
                parts.append(f"- Summary: {t['doc']}\n")
            if t["attrs"]:
                parts.append(f"- Attributes: {' '.join(t['attrs'])}\n")
            if not t["members"]:
                parts.append("- No public or serialized members.\n")
                continue
            for group in ("value", "field", "property", "method", "unity message"):
                got = [m for m in t["members"] if m["kind"] == group]
                if not got:
                    continue
                label = {"value": "Values", "unity message": "Unity messages"}.get(
                    group, group.title() + "s")
                parts.append(f"\n**{label}**\n\n")
                for m in got:
                    attrs = (" ".join(m["attrs"]) + " ") if m["attrs"] else ""
                    parts.append(f"- `{attrs}{m['sig']}`"
                                 + (f" — {m['doc']}" if m["doc"] else "") + "\n")
    parts.insert(1, f"**{total_types} types across {len(files)} source files.**\n\n")
    emit("03-api-index", f"Generated API index ({total_types} types)", "".join(parts))

    # 04-06 — full source ----------------------------------------------------
    claimed = set()
    for stem, title, globs in BUNDLES:
        got = []
        for g in globs:
            for p in sorted(REPO.glob(g)):
                rel = p.relative_to(REPO).as_posix()
                if rel in claimed or any(x in rel for x in CODE_EXCLUDE):
                    continue
                claimed.add(rel)
                got.append(rel)
        if not got:
            continue
        parts = [header(title, stamp,
                        "Complete C# source. Each file is preceded by a `FILE:` line giving "
                        "its repository path — quote that path when citing code.")]
        parts.append(f"Files in this source: {', '.join('`' + g + '`' for g in got)}\n")
        for rel in got:
            parts.append(f"\n\n---\n\n## FILE: {rel}\n\n```csharp\n")
            parts.append((REPO / rel).read_text(errors="ignore").rstrip())
            parts.append("\n```\n")
        emit(stem, title, "".join(parts))

    # 07 — project configuration --------------------------------------------
    parts = [header("Project Configuration & Asset Inventory", stamp,
                    "Engine, package, scene and prefab facts. Regenerated each run — "
                    "never hand-edit.")]
    parts.append("## Engine & input\n\n")
    parts.append(f"- Unity: `{unity_version()}`\n")
    parts.append(f"- Input backend: {input_backend()}\n")
    parts.append("- Assembly layout: no asmdefs. Runtime code compiles into `Assembly-CSharp`; "
                 "code under `Assets/Editor/` into `Assembly-CSharp-Editor`.\n")
    parts.append("- No namespaces: every type sits in the global namespace.\n")
    parts.append("\n## Packages (`Packages/manifest.json`)\n\n")
    for name, ver in packages():
        parts.append(f"- `{name}` — {ver}\n")
    parts.append("\n## Scenes\n\n")
    for p in sorted(REPO.glob("Assets/Scenes/*.unity")):
        parts.append(f"- `{p.relative_to(REPO).as_posix()}`\n")
    parts.append("\n## Prefabs\n\n")
    for p in sorted(REPO.rglob("Assets/Prefabs/*.prefab")):
        parts.append(f"- `{p.relative_to(REPO).as_posix()}`\n")
    parts.append("\n## Source file inventory\n\n| File | Lines |\n|---|---|\n")
    for rel in files:
        n = len((REPO / rel).read_text(errors="ignore").splitlines())
        parts.append(f"| `{rel}` | {n} |\n")
    emit("07-project-config", "Project config & asset inventory", "".join(parts))

    # 00 — manifest (last, so it can report the others) ----------------------
    write(out, "00-MANIFEST", build_manifest(stamp_lines, written, dirty))

    total = sum(w for _, _, w in written)
    print(f"Wrote {len(written) + 1} sources to {out}")
    for stem, title, words in written:
        print(f"  {stem}.md  {words:>7,} words  — {title}")
    print(f"  00-MANIFEST.md              — build stamp + reading guide")
    print(f"\nTotal ~{total:,} words across {len(written)} content sources "
          f"(NotebookLM limit: 500,000 words per source).")
    if dirty:
        print(f"\nNote: {len(dirty)} file(s) had uncommitted changes; the manifest records this.",
              file=sys.stderr)


if __name__ == "__main__":
    main()
