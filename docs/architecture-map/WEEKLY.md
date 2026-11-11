# The weekly map update

You're updating the Starwolf architecture map, a private page at the URL in `map.json`. The page has two
maps. The systems map shows the 15 parts of the game the player uses, and how they affect each other. The
code map shows every script and which other scripts it uses.

This runs every Thursday at 9 pm IST, or at the next login after that if the Mac was off. The builder has
already run. Its report is at the end of this prompt. Work from the project root.

## What to do

1. Read the report. It lists new and removed scripts, new descriptions, changes to which script uses which,
   and anything the builder couldn't sort out by itself.
2. Check that the systems map still fits, using `docs/architecture-map/map.json`:
   - A new script was put in a column by its folder. If another column fits it better, move it.
   - If a new script is part of one of the 15 systems, add it to that system's `s` list. If a system lists a
     script that no longer exists, take it out.
   - If a new description shows that a system now works differently, update that system's text (`d`) or its
     links. Read the script's note with `git show HEAD:<path>` when you need more. Only change what the notes
     and the code show. Don't invent anything.
3. If you changed `map.json`, build again and say what you changed, one `--say` for each change:
   `python3 docs/architecture-map/build_map.py build --say "CampaignWindow is now part of the Campaign system."`
4. Publish the page. First read the page with the Artifact tool (action `read`, the URL from `map.json`).
   Then publish `docs/architecture-map/out/starwolf-architecture-map.html` to that same URL.
5. Run `python3 docs/architecture-map/build_map.py done`.
6. Run `python3 docs/architecture-map/build_map.py commit`. It commits only this folder's files.
7. Finish with a few plain sentences on what changed this week.

## Rules

- Don't edit any file except `docs/architecture-map/map.json`. Don't use Unity. Don't run any git command
  that changes anything; `build_map.py commit` does the one commit.
- Write the way the owner wants: normal, natural sentences, the way a person would explain it out loud. Short
  is good, but don't chop sentences into fragments, and don't invent phrases. Never use these words: live,
  sit, hold, own, claim, carry, land, handle, manage. No "X, not Y". Say DRADIS, never radar, and torpedo,
  never missile.
