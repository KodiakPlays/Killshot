"""Check script notes, and put them on top of their scripts with proof the code is unchanged.

  python3 notes_tool.py check <batch.txt>   # format + banned-word scan, changes nothing
  python3 notes_tool.py apply <batch.txt>   # one script at a time: prepend, then prove the rest is byte-identical
  python3 notes_tool.py proof <batch.txt>   # after apply: re-prove every script against its saved 'before' copy
Run from the project root.
"""
import os, re, sys

AUDIT = os.path.dirname(os.path.abspath(__file__))
NOTES, BEFORE = os.path.join(AUDIT, "notes"), os.path.join(AUDIT, "before")
RULE = "// " + "=" * 97
BANNED = re.compile(
    r"\b(lives?|living|sits?|sitting|holds|owns?|owned|owning|claims?|home|carr(y|ies)|lands?|teach(es)?|earns?|"
    r"doctrine|normative|authoritative|monotonic|load-bearing|presentational|leverag\w*|robust|seamless\w*|"
    r"orchestrat\w*|encapsulat\w*|handles?|handling|manages?|managing|because|so that|in order to|this is why|"
    r"instantiat\w*|singleton|callbacks?|coroutines?|serializ\w*|lerps?|ticks?|components?|instances?|delegates?|"
    r"enums?|structs?|radar|missiles?|starwolf|step \d+|owner|dead|legacy|junk|used to|"
    r"which|since)\b", re.I)


def note_path(cs): return os.path.join(NOTES, cs + ".txt")


def check(cs):
    """Hard errors (block apply) and soft flags (a person reads them)."""
    errs, flags = [], []
    p = note_path(cs)
    if not os.path.exists(p):
        return ["no note file"], flags
    raw = open(p, "rb").read()
    try:
        text = raw.decode("ascii")
    except UnicodeDecodeError:
        return ["not plain ASCII"], flags
    lines = text.rstrip("\n").split("\n")
    for i, l in enumerate(lines, 1):
        if not l.startswith("//"): errs.append(f"line {i} does not start with //")
        if len(l) > 100: errs.append(f"line {i} is {len(l)} chars")
        if "\r" in l or "\t" in l: errs.append(f"line {i} has a tab or CR")
        for m in BANNED.finditer(l):
            flags.append(f"line {i}: '{m.group(0)}' in: {l[3:].strip()}")
        if re.search(r",\s*not\s", l): flags.append(f"line {i}: possible 'X, not Y': {l[3:].strip()}")
    if lines[0] != RULE or lines[-1] != RULE: errs.append("first/last line is not the = rule")
    if len(lines) < 2 or lines[1] != "// " + os.path.basename(cs): errs.append("line 2 is not the file name")
    if "// What it does:" not in lines: errs.append("no 'What it does:'")
    if "// How it works:" not in lines: errs.append("no 'How it works:'")
    if "--tech" in sys.argv and "// Technical:" not in lines: errs.append("no 'Technical:'")
    return errs, flags


def scripts(batch): return [l.strip() for l in open(batch) if l.strip()]


def cmd_check(batch):
    bad = 0
    for cs in scripts(batch):
        errs, flags = check(cs)
        if errs or flags:
            print(f"{cs}: {'ERRORS ' + '; '.join(errs) if errs else 'ok format'}")
            for f in flags: print("    flag " + f)
        bad += bool(errs)
    print(f"{bad} with errors")


def cmd_apply(batch):
    for cs in scripts(batch):
        errs, _ = check(cs)
        if errs: print(f"SKIP {cs}: {'; '.join(errs)}"); continue
        note = open(note_path(cs), "rb").read().rstrip(b"\n") + b"\n"
        orig = open(cs, "rb").read()
        if orig.startswith(RULE.encode()): print(f"SKIP {cs}: already has a note"); continue
        keep = os.path.join(BEFORE, cs); os.makedirs(os.path.dirname(keep), exist_ok=True)
        open(keep, "wb").write(orig)
        open(cs, "wb").write(note + b"\n" + orig)
        print(("OK   " if prove(cs) else "FAIL ") + cs)


def prove(cs):
    """True if the script is: note + blank line + exactly the saved 'before' bytes."""
    now, orig = open(cs, "rb").read(), open(os.path.join(BEFORE, cs), "rb").read()
    note = open(note_path(cs), "rb").read().rstrip(b"\n") + b"\n"
    return now == note + b"\n" + orig


def cmd_proof(batch):
    for cs in scripts(batch):
        if not os.path.exists(os.path.join(BEFORE, cs)): print(f"NOT APPLIED {cs}"); continue
        if prove(cs): print("OK   " + cs); continue
        now = open(cs, "rb").read(); rule = RULE.encode() + b"\n"; end = now.find(rule, len(rule))
        code_same = end > 0 and now[end + len(rule) + 1:] == open(os.path.join(BEFORE, cs), "rb").read()
        print(("OLD NOTE, CODE SAME " if code_same else "CODE CHANGED BELOW NOTE ") + cs)


def cmd_lengths(batch):
    """List every sentence over 20 words in the notes."""
    total = over = 0
    for cs in scripts(batch):
        if not os.path.exists(note_path(cs)): continue
        body = open(note_path(cs)).read().splitlines()[3:-1]
        text = re.sub(r"\s+", " ", " ".join(l[2:].strip().removeprefix("- ") for l in body))
        for s in re.split(r"(?<=[.!?:])\s+", text):
            n = len(s.split())
            if n < 3: continue
            total += 1
            if n > 20: over += 1; print(f"{n:3} words  {cs}: {s}")
    print(f"{over} of {total} sentences are over 20 words")


def cmd_reapply(batch):
    """Swap the note already at the top of each script for the current note file. The code below stays as it is."""
    for cs in scripts(batch):
        errs, _ = check(cs)
        if errs: print(f"SKIP {cs}: {'; '.join(errs)}"); continue
        now = open(cs, "rb").read()
        rule = RULE.encode() + b"\n"
        end = now.find(rule, len(rule))  # the closing rule line
        if not now.startswith(rule) or end < 0 or now[end + len(rule):end + len(rule) + 1] != b"\n":
            print(f"SKIP {cs}: no note at the top to swap"); continue
        code = now[end + len(rule) + 1:]
        note = open(note_path(cs), "rb").read().rstrip(b"\n") + b"\n"
        keep = os.path.join(BEFORE, cs); os.makedirs(os.path.dirname(keep), exist_ok=True)
        touched = os.path.getmtime(cs) > os.path.getmtime(note_path(cs)) + 2  # the script changed after its note was last checked
        if touched and "--checked" not in sys.argv:
            print(f"SKIP {cs}: changed after its note was last checked; check by hand, then rerun with --checked"); continue
        open(keep, "wb").write(code)
        open(cs, "wb").write(note + b"\n" + code)
        print(("OK   " if prove(cs) else "FAIL ") + cs)


TYPE = re.compile(r"^\s*(?:\[[^\]]*\]\s*)*(?:(?:public|internal|private|protected|sealed|static|abstract|partial|readonly)\s+)*(?:class|struct|interface|enum)\s+\w")


def old_note(lines):
    """Line indexes of the old '///' note directly above the file's first type (attribute lines may sit between)."""
    for i, l in enumerate(lines):
        if TYPE.match(l):
            j = i - 1
            while j >= 0 and re.match(r"^\s*\[.*\]\s*$", lines[j]): j -= 1
            k = j
            while k >= 0 and lines[k].lstrip().startswith("///"): k -= 1
            return list(range(k + 1, j + 1))
    return []


def cmd_showold(batch):
    for cs in scripts(batch):
        lines = open(cs).read().split("\n"); idx = old_note(lines)
        print(f"=== {cs}: {'no old note' if not idx else f'old note, {len(idx)} lines'}")
        for i in idx: print(lines[i])


def cmd_dropold(batch):
    """Delete each script's old '///' note above its first type. Proves only those lines went."""
    for cs in scripts(batch):
        raw = open(cs, "rb").read(); lines = raw.decode("utf-8").split("\n"); idx = old_note(lines)
        if not idx: print(f"NONE {cs}"); continue
        if not all(lines[i].lstrip().startswith("///") for i in idx): print(f"FAIL {cs}: block has a non-/// line"); continue
        new = [l for i, l in enumerate(lines) if i not in set(idx)]
        out = "\n".join(new).encode("utf-8")
        # proof: putting the removed lines back gives the file exactly as it was
        back = new[:idx[0]] + [lines[i] for i in idx] + new[idx[0]:]
        if "\n".join(back).encode("utf-8") != raw: print(f"FAIL {cs}: proof failed, nothing written"); continue
        open(cs, "wb").write(out)
        rule = RULE.encode() + b"\n"; end = out.find(rule, len(rule))
        keep = os.path.join(BEFORE, cs); os.makedirs(os.path.dirname(keep), exist_ok=True)
        open(keep, "wb").write(out[end + len(rule) + 1:])
        print(f"OK   {cs}: removed {len(idx)} old note lines")


if __name__ == "__main__":
    {"check": cmd_check, "apply": cmd_apply, "proof": cmd_proof, "reapply": cmd_reapply, "lengths": cmd_lengths, "showold": cmd_showold, "dropold": cmd_dropold}[sys.argv[1]](sys.argv[2])
