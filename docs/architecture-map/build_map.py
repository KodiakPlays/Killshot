"""Builds the Starwolf architecture map page from the newest commit, and records what changed.

It reads every C# script as it is in the newest commit, so half-finished edits never show. It works out
which script uses which, takes each script's "What it does" from the note at its top, and fills page.html
with that and with map.json (the columns, the 15 systems and their links, the screens). It compares the
result with the last published map and writes a short list of what changed.

    python3 docs/architecture-map/build_map.py build [--say "a line for the change list"] ...
    python3 docs/architecture-map/build_map.py due      # prints this week's date and exits 0 if this week's update hasn't run
    python3 docs/architecture-map/build_map.py done     # records the last build as published
    python3 docs/architecture-map/build_map.py commit   # commits this folder's files, and nothing else

Run it from the project root. The built page goes to docs/architecture-map/out/ (git ignores it). The
record of what was published is in ~/.claude/starwolf-map/. The weekly job that runs it is described in
WEEKLY.md, next to this file.
"""
import datetime, hashlib, json, os, re, subprocess, sys, tempfile
from zoneinfo import ZoneInfo

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "out", "starwolf-architecture-map.html")
STATE = os.path.expanduser("~/.claude/starwolf-map")
IST = ZoneInfo("Asia/Kolkata")
RULE = "// " + "=" * 97


def git(*args, inp=None):
    return subprocess.run(["git", *args], input=inp, capture_output=True, text=True, check=True).stdout


def load(name, default=None):
    p = os.path.join(STATE, name)
    return json.load(open(p)) if os.path.exists(p) else default


def save(name, data):
    os.makedirs(STATE, exist_ok=True)
    json.dump(data, open(os.path.join(STATE, name), "w"), indent=1)


def strip(src):
    """The code with comments and strings blanked out, so only real code names count as links."""
    src = re.sub(r"/\*.*?\*/", " ", src, flags=re.S)
    src = re.sub(r'@"(?:[^"]|"")*"', '""', src)
    src = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', src)
    src = re.sub(r"'(?:\\.|[^'\\\n])'", "''", src)
    return re.sub(r"//[^\n]*", " ", src)


def what_it_does(src):
    lines = src.split("\n")
    if not lines or lines[0].lstrip("﻿") != RULE or "// What it does:" not in lines: return ""
    out = []
    for l in lines[lines.index("// What it does:") + 1:]:
        if l.strip() == "//" or l == RULE: break
        out.append(l[2:].strip())
    return " ".join(out)


def read_scripts(commit):
    paths = [p for p in git("ls-tree", "-r", "--name-only", commit, "--", "Assets").splitlines() if p.endswith(".cs")]
    return {p: git("show", f"{commit}:{p}").lstrip("﻿") for p in paths}


def build(say):
    cfg = json.load(open(os.path.join(HERE, "map.json")))
    commit = git("rev-parse", "HEAD").strip()
    src = read_scripts(commit)
    name = lambda p: os.path.basename(p)[:-3]
    code = {name(p): strip(s) for p, s in src.items()}
    path = {name(p): p for p in src}
    # Top-level types per script. A nested type (Campaign.Mission) is not a link to a script of that name.
    owner = {}
    for n, c in code.items():
        ns = 1 if re.search(r"\bnamespace\b", c) else 0
        for m in re.finditer(r"\b(?:class|struct|enum|interface)\s+([A-Za-z_]\w*)", c):
            pre = c[:m.start()]
            if pre.count("{") - pre.count("}") <= ns: owner.setdefault(m.group(1), set()).add(n)
    ignore = {tuple(x) for x in cfg["ignore_links"]}
    uses = {}
    for n, c in code.items():
        words = set(re.findall(r"[A-Za-z_]\w*", c))
        uses[n] = sorted({o for w in words for o in owner.get(w, ()) if len(owner[w]) == 1 and o != n and (n, o) not in ignore})
    used_by = {n: sorted(a for a in uses if n in uses[a]) for n in code}

    # Columns: keep the hand-placed order, drop scripts that are gone, add new scripts by their folder.
    views, placed, report = {}, {}, []
    for v, cols in cfg["views"].items():
        views[v] = [{"key": c["key"], "label": c["label"], "items": [n for n in c["items"] if n in code]} for c in cols]
        for c in views[v]:
            for n in c["items"]: placed[n] = (v, c["key"])
    for n in sorted(set(code) - set(placed)):
        key = next(k for folder, k in cfg["new_scripts_go_to"] if path[n].startswith(folder))
        col = next(c for c in views["board"] if c["key"] == key)
        col["items"].append(n); placed[n] = ("board", key)
        report.append(f"PLACED BY FOLDER: {n} ({path[n]}) went under \"{col['label']}\". Move it in map.json if another column fits better.")
    scripts = {n: {"path": path[n], "what": what_it_does(src[path[n]]), "view": placed[n][0], "group": placed[n][1],
                   "uses": uses[n], "usedBy": used_by[n]} for n in sorted(code)}
    for k, s in cfg["systems"].items():
        for n in s["s"]:
            if n not in code: report.append(f"MISSING: the {s['n']} system lists {n}, and that script no longer exists.")
    for n, sc in scripts.items():
        if not sc["what"]: report.append(f"NO NOTE: {n} has no \"What it does\" note at its top.")

    data = {"scripts": scripts, "views": views, "systems": cfg["systems"], "links": cfg["links"],
            "screens": cfg["screens"], "main_scripts": cfg["main_scripts"], "url": cfg["url"]}
    page = open(os.path.join(HERE, "page.html")).read()
    digest = hashlib.sha256((json.dumps(data, sort_keys=True) + page).encode()).hexdigest()

    # What changed since the last published map, in plain words.
    last, changes = load("last.json"), []
    if last:
        old = last["scripts"]
        for n in sorted(set(scripts) - set(old)):
            label = next(c["label"] for c in views[scripts[n]["view"]] if c["key"] == scripts[n]["group"])
            changes.append(f"New script: {n}, under \"{label}\".")
        changes += [f"Removed script: {n}." for n in sorted(set(old) - set(scripts))]
        changes += [f"{n} has a new description." for n in sorted(set(scripts) & set(old)) if scripts[n]["what"] != old[n]["what"]]
        links = []
        for n in sorted(set(scripts) & set(old)):
            links += [f"{n} now uses {m}." for m in scripts[n]["uses"] if m not in old[n]["uses"]]
            links += [f"{n} no longer uses {m}." for m in old[n]["uses"] if m not in scripts[n]["uses"] and m in scripts]
        changes += links[:12] + ([f"...and {len(links) - 12} more changes to which script uses which."] if len(links) > 12 else [])
        if any(last.get(k) != data[k] for k in ("systems", "links", "screens", "main_scripts")) and not say:
            changes.append("The systems map was updated.")
    changes += say
    data["changes"] = {"date": datetime.datetime.now(IST).strftime("%-d %B %Y"), "items": changes}

    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    open(OUT, "w").write(page.replace("/*DATA*/null", json.dumps(data, separators=(",", ":"))))
    save("pending.json", {"commit": commit, "hash": digest, "data": {k: v for k, v in data.items() if k != "changes"}})
    same = (load("state.json") or {}).get("hash") == digest
    print(f"Commit {commit[:7]}. {len(scripts)} scripts: {sum(len(c['items']) for c in views['board'])} battle board, "
          f"{sum(len(c['items']) for c in views['older'])} older.")
    for line in changes: print("CHANGE: " + line)
    for line in report: print(line)
    print(f"Page: {OUT}")
    print("STATUS: no changes" if same else "STATUS: changes")


def last_thursday_9pm(now):
    days = (now.weekday() - 3) % 7  # Thursday is 3
    t = (now - datetime.timedelta(days=days)).replace(hour=21, minute=0, second=0, microsecond=0)
    return t if t <= now else t - datetime.timedelta(days=7)


def due():
    now = datetime.datetime.now(IST)
    slot = last_thursday_9pm(now)
    last = (load("state.json") or {}).get("last_run")
    print(slot.strftime("%Y-%m-%d"))
    sys.exit(0 if not last or datetime.datetime.fromisoformat(last) < slot else 1)


def done():
    p = load("pending.json")
    save("last.json", p["data"])
    save("state.json", {"mapped_commit": p["commit"], "hash": p["hash"], "last_run": datetime.datetime.now(IST).isoformat()})
    print(f"Recorded: commit {p['commit'][:7]} is on the map.")


def commit():
    """Commit this folder's files from a private index, so nothing another chat staged goes in with them."""
    if os.environ.get("MAP_NO_COMMIT"): print("Commit skipped (MAP_NO_COMMIT is set)."); return
    old = git("rev-parse", "HEAD").strip()
    folder = os.path.relpath(HERE, git("rev-parse", "--show-toplevel").strip())
    files = [os.path.join(folder, f) for f in ("map.json", "page.html", "build_map.py", "WEEKLY.md", ".gitignore")
             if os.path.exists(os.path.join(HERE, f))]
    blobs = {f: git("hash-object", "-w", f).strip() for f in files}
    changed = [f for f in files if subprocess.run(["git", "rev-parse", "-q", "--verify", f"{old}:{f}"], capture_output=True, text=True).stdout.strip() != blobs[f]]
    if not changed: print("Nothing to commit."); return
    env = dict(os.environ, GIT_INDEX_FILE=os.path.join(tempfile.mkdtemp(), "index"))
    run = lambda *a, inp=None: subprocess.run(["git", *a], input=inp, capture_output=True, text=True, check=True, env=env).stdout.strip()
    run("read-tree", old)
    for f in changed: run("update-index", "--add", "--cacheinfo", f"100644,{blobs[f]},{f}")
    tree = run("write-tree")
    msg = f"Architecture map: weekly update, {datetime.datetime.now(IST).strftime('%-d %B %Y')}\n\nOnly docs/architecture-map/ changed: {', '.join(os.path.basename(f) for f in changed)}.\n"
    new = run("commit-tree", tree, "-p", old, inp=msg)
    git("update-ref", "HEAD", new, old)  # fails if anyone committed in between
    for f in changed:  # keep the shared index in step, unless another chat staged its own copy
        staged = git("ls-files", "-s", "--", f).split()
        if not staged or staged[1] == subprocess.run(["git", "rev-parse", "-q", "--verify", f"{old}:{f}"], capture_output=True, text=True).stdout.strip():
            git("update-index", "--add", "--cacheinfo", f"100644,{blobs[f]},{f}")
    print(f"Committed {new[:7]}: {', '.join(changed)}")


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else "build"
    if cmd == "build":
        args, say = sys.argv[2:], []
        while args:
            if args[0] == "--say" and len(args) > 1: say.append(args[1]); args = args[2:]
            else: sys.exit(f"Unknown argument: {args[0]}")
        build(say)
    elif cmd in ("due", "done", "commit"):
        {"due": due, "done": done, "commit": commit}[cmd]()
    else:
        sys.exit(__doc__)
