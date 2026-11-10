# Script notes audit — the verifier's job

Another agent wrote a plain-English note for each script in your list. You check each note before it goes
into the script. The owner says this is very important. Be strict.

Read first, in full:
1. The rule sheet: /Users/Shagun/Documents/GitHub/Killshot/docs/script-notes/BRIEF.md
2. The owner's English rules: /Users/Shagun/.claude/projects/-Users-Shagun-Documents-GitHub-Killshot/memory/shagun-wants-plain-english.md

For each script in your list, one at a time, in order:

1. Read the note: NOTES/<script path>.txt
   (NOTES = /Users/Shagun/Documents/GitHub/Killshot/docs/script-notes/notes)
2. Read the **whole** script in the project (/Users/Shagun/Documents/GitHub/Killshot/<script path>), every line.
   Files over 2,000 lines: read in parts until the end.
3. Check the note, line by line:
   - **English.** Every rule in the rule sheet and the English rules file. Banned words, "X, not Y", "because",
     "so that", stacked clauses, code words, invented names, flowery or clever verbs, history (step numbers,
     dates, "was", "used to", "owner"). Would the owner understand every sentence on first read?
   - **Truth.** Every fact in the note must match the code. Check names of other scripts, keys, numbers, what
     runs when. Check "the battle board does not use this script" claims with the guid check in the rule sheet.
     A missing important part of the script (a big feature the note does not mention) also counts.
   - **Shape.** The exact layout in the rule sheet: rule lines, file name on line 2, the two headings, `//` on
     every line, ASCII only, 100 characters at most per line.
4. If anything is wrong, fix the note file in place (Edit or Write on the .txt only). Keep what is right.
5. Add one line to your verdict file VERDICTS/<your batch name>.txt:
   `<script path> | PASS` or `<script path> | FIXED: <what you changed, a few words>`
   (VERDICTS = /Users/Shagun/Documents/GitHub/Killshot/docs/script-notes/verdicts)

When all are done, run the format and word check and fix anything it reports as ERRORS. Read every "flag" line
too: most are fine in context ("since" meaning time, a class named Radar), but fix any real breach.

    cd /Users/Shagun/Documents/GitHub/Killshot && python3 /Users/Shagun/Documents/GitHub/Killshot/docs/script-notes/notes_tool.py check <your batch file>

Rules for you:
- **Never edit, move or save any file in the project.** Only the .txt notes and your verdict file.
- No Unity tools. No git commands that change anything.
- Reply at the end with: how many PASS, how many FIXED, and the three worst problems you found (one line each).
