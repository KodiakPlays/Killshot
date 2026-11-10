# Script notes audit — the tidy pass

The notes are checked for facts. Now make the English clean and crisp. The owner: "Humans have to read this.
Stop writing fluff. Clean, crisp sentences."

Read first:
1. The rule sheet: /Users/Shagun/Documents/GitHub/Killshot/docs/script-notes/BRIEF.md (layout and English rules)
2. The owner's English rules: /Users/Shagun/.claude/projects/-Users-Shagun-Documents-GitHub-Killshot/memory/shagun-wants-plain-english.md

For each note in your list, one at a time, in order (NOTES/<script path>.txt,
NOTES = /Users/Shagun/Documents/GitHub/Killshot/docs/script-notes/notes):

1. Read the note. Rewrite only what needs it:
   - **Short sentences.** Aim for 15 words or fewer. **None over 20 words.**
   - **Long lists:** split them into two or three short sentences, or keep the main items and drop the small
     ones. The note is a gist. It does not need every item.
   - **Cut fluff:** filler openers ("This is a script that..."), "also", "simply", "actually", "basically",
     "in order to", "note that", words that repeat the heading, and sentences that say nothing new.
   - One idea per sentence. Plain words. Say what happens.
2. **Keep the facts.** Never change a fact. Never add a fact. If you are unsure what a line means, open the
   script and check, or leave the line as it is.
3. Keep the layout exactly: rule lines, the file name on line 2, "What it does:", "How it works:", bullets,
   `//` on every line, ASCII only, 100 characters at most per line, 3-space indent on wrapped bullet lines.
4. Save the note (Edit or Write on the .txt only).

When your list is done, run both checks from the project root and fix what they report:

    python3 docs/script-notes/notes_tool.py check <your list>
    python3 docs/script-notes/notes_tool.py lengths <your list>

`check` must show 0 errors. `lengths` must show 0 sentences over 20 words. (It splits sentences at full stops
and colons.)

Rules: never edit any file in the project except the .txt notes. No Unity tools. No git commands that change
anything. Reply in three lines: notes changed, notes left as they were, and anything you were unsure of.
