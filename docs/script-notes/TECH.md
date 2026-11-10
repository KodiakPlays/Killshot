# Script notes audit — the Technical pass

Each script's note gets a third part, "Technical:". Each script's old note is then deleted. The old note is the
`/// <summary>` block above the script's first class. Its useful technical facts move into the Technical part
first. This pass also brings the whole note up to date with today's code.

Read first, in full:
1. The rule sheet: /Users/Shagun/Documents/GitHub/Killshot/docs/script-notes/BRIEF.md (layout, English, and
   "The Technical part")
2. The tidy rules: /Users/Shagun/Documents/GitHub/Killshot/docs/script-notes/TIDY.md
3. The owner's English rules: /Users/Shagun/.claude/projects/-Users-Shagun-Documents-GitHub-Killshot/memory/shagun-wants-plain-english.md

Work from /Users/Shagun/Documents/GitHub/Killshot. NOTE = docs/script-notes/notes/<script path>.txt.

## Writers: for each script in your list, one at a time, in order

1. Read the **whole** script, every line. Over 2,000 lines: read in parts to the end.
2. Read NOTE. Then compare it with the note inside the script (the lines between the first two `// =====`
   rule lines). If the script's note has a point NOTE lacks, another chat added it: keep its meaning.
3. See the old note that will be deleted:
   `python3 docs/script-notes/notes_tool.py showold <(echo <script path>)`
4. Update NOTE:
   - "What it does" and "How it works": fix anything the code no longer matches. Keep the rest.
   - Add "Technical:" as BRIEF.md says. Check every caller with grep. Move in each old-note fact that is
     technical and still true. Leave out history and prototype function names.
5. Save NOTE (the .txt only). Then the next script.

When your list is done, run and fix until clean (0 errors, 0 sentences over 20 words):

    python3 docs/script-notes/notes_tool.py check <your list> --tech
    python3 docs/script-notes/notes_tool.py lengths <your list>

Reply in three lines: notes done, old-note facts you chose to drop and why (short), anything unsure.

## Verifiers: for each script in your list, one at a time, in order

1. Read NOTE, the **whole** script, and the old note (`showold`, as above).
2. Check every fact in all three parts against the code. Check callers with grep. Check the English.
3. Check the old note: is any technical fact in it still true, useful, and missing from NOTE? Add it.
4. Fix NOTE in place if needed. Add one line to `docs/script-notes/verdicts/tech-<your list name>.txt`:
   `<script path> | PASS` or `<script path> | FIXED: <a few words>`.
5. If you find a bug in the code, add it to the verdict file as `<script path> | BUG: <a few words>`. Do not
   fix it.

Then run both checks above and fix until clean. Reply: PASS count, FIXED count, bugs found, worst three
problems (one line each).

## Rules for everyone

- Never edit any file in the project except the .txt notes and verdict files. Never edit a script.
- No Unity tools. No git commands that change anything.
- Other chats are editing some scripts today. Read each script as it is on disk when you reach it.
