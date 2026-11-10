# Script notes audit — the rules every agent follows

Project: /Users/Shagun/Documents/GitHub/Killshot (a Unity game, Starwolf). The owner wants a plain-English
note at the very top of every C# script: what the script does, and a gist of how it works.

## The job

- You get a list of scripts. Work through them **one at a time, in order**. For each one:
  1. Read the **whole** script, every line. Files over 2,000 lines: read in parts with offset/limit until the end.
  2. Find who uses it: `grep -rn "ClassName" Assets --include=*.cs`. To see if it is on anything in a scene or
     prefab, take the guid from its `.cs.meta` file and run
     `grep -rl <guid> Assets --include=*.unity --include=*.prefab --include=*.asset`.
  3. Write the note (rules below) to the notes folder, at the script's path plus `.txt`:
     `NOTES/Assets/Scripts/Game/GasCloud.cs.txt` for `Assets/Scripts/Game/GasCloud.cs`.
     NOTES = /Users/Shagun/Documents/GitHub/Killshot/docs/script-notes/notes
- If a note already exists for a script, skip that script. Earlier writers stopped part-way through.
- **Never edit, move or save any file in the project.** Not the scripts, not docs, nothing. You only write `.txt`
  files in the notes folder. Another step puts the notes into the scripts and checks the code is untouched.
- Several other sessions share this project and its Unity Editor. Do not use any Unity tools. Do not run git
  commands that change anything (`git add`, `checkout`, `stash`, `reset`, `commit`). Reading with `git log`/`git show` is fine.

## The note's shape

Every line starts with `//`. Plain ASCII only (no em dashes, no curly quotes, no special symbols). At most 100
characters per line, counting the `//`. Exactly this layout:

```
// =================================================================================================
// GasCloud.cs
//
// What it does:
// A gas cloud is a box of cold gas placed in a level. Ships fly through it and take no damage.
// Our ship cools down while it is inside. An enemy can only see a ship in the cloud from close by.
//
// How it works:
// - The cloud's width and height are set in the Inspector, in units.
// - Other scripts ask it two things: is a point inside the box, and how much of a ship's box is
//   inside it (0 to 1). For the second, it cuts the ship's box into 100 small squares and counts
//   the ones inside.
// - TacticalWorld finds the clouds and does the cooling and the hiding.
// - In the Scene view it draws a blue box with the cloud's name.
// =================================================================================================
```

(That example shows the shape and the tone. Check every fact for yourself in the real script.)

- After "How it works" comes a third part, "Technical:" (see below), then the closing rule line.
- The `=` rule lines are exactly `// ` plus 97 `=` signs (100 characters).
- Line 2 is the file name only.
- **What it does:** 1 to 4 short sentences. What the thing is in the game or the editor, for someone who knows
  nothing. Say where it runs: on our ship, on every enemy ship, on the board, in a menu scene, an editor window,
  and so on. If the battle board does not use it, say so in one plain sentence: "The battle board does not use
  this script." Never call a script dead, junk, legacy or unused rubbish.
- **How it works:** 3 to 10 bullet points (up to 15 for scripts over 1,000 lines). Each bullet is what happens,
  in the order it happens where that makes sense: what it reads, what it changes, when it runs (every frame,
  every second of game time, on a click), and which other scripts it talks to. Wrap long bullets with a
  3-space indent (`//   `) as in the example.
- No history: no step numbers, no dates, no "owner said", no "was", no "used to". Say what it does now.
- No code pasted in. Name other scripts by their class name (TacticalWorld, DamageSystem) when it helps.
- Only write what the code really does. If you are not sure of something, leave it out.
- If a script has commented-out code or a part that never runs, you may say so plainly in one bullet.

## The Technical part (owner 2026-11-10)

The owner: "it's also meant to be a technical gist, so you're not wasting time relearning where things go."
It comes after "How it works", with a blank `//` line before it:

```
//
// Technical:
// - Data: x and y are the true position in units (x east, y north). vx and vy are the speed.
// - Tick(dt) steers, then moves, once per board step. BoardManager calls it for our ship.
// - Numbers: NavSystem.Stats, in the navigation part of each ship's stats file (Assets/Ships).
```

- 3 to 10 bullets (up to 15 for scripts over 1,000 lines). Short sentences, none over 20 words.
- Here, code names are welcome: classes, methods, fields, files. That is the point of this part.
- Cover what a developer needs to find their way:
  - the main data it keeps, with units;
  - the methods other scripts call, and which scripts call them (check with grep);
  - what it calls in other scripts;
  - where its numbers are set: a section of the ship stats file, a `[Tune]` section on the Tuning window's
    Board tab, or the Inspector;
  - traps: anything that would surprise someone changing it.
- No history: no dates, owner quotes, step numbers. No function names from the old browser prototype
  (board-logic.js) or design-file section numbers.

## The owner's English rules (very important — the owner checks this)

Read `/Users/Shagun/.claude/projects/-Users-Shagun-Documents-GitHub-Killshot/memory/shagun-wants-plain-english.md` first.
In short:

- Plain English. Short sentences. One idea per sentence. No stacked clauses. Avoid "which" and "since".
- No jargon, no consultant voice, no clever or flowery verbs, no drama.
- Banned words: live/lives (as in "is located"), sit/sits, hold/holds (as in "contains" — "hold Space" as a key
  press is fine), own/owns (as in "is in charge of"), claim, home, carry/carries, land/lands, teach, earn,
  doctrine, normative, authoritative, monotonic, load-bearing, presentational, leverage, robust, seamless,
  orchestrate, encapsulate, handle/handles (say what it does), manage(s) (say what it does).
  Say "the numbers are in file X", not "the numbers live in file X".
- No "X, not Y" contrasts. State the fact once.
- No justification framing: no "because", "this is why", "so that", "so nothing...", "in order to". Say the action only.
- Code words: keep them few. Say what happens instead of "instantiates", "singleton", "callback", "coroutine",
  "serialized", "lerp", "tick", "component", "instance", "delegate", "enum", "struct". The Unity words the
  owner uses are fine: scene, prefab, Inspector, Scene view, Game view, play mode, the Tuning window
  (Starwolf > Tuning), the Level window. "Script" is fine. Class names of other scripts are fine.
- Numbers: two decimal places at most.

## Game words (use these exact words)

- **DRADIS** is the ship's sensor. Never write "radar", even when the code says Radar (write "the old DRADIS
  sweep, named Radar in the code" if you must).
- **Torpedoes** are what the code often calls missiles. Write "torpedo". Only say "missile" when naming the
  code file or class itself.
- Our ship is the **Asura-class destroyer** ("our ship" is fine). The enemy is the **Garuda-class cruiser**
  ("the Garuda" or "the enemy ship"). Never call our ship "the Starwolf".
- **The railgun** (one, fixed on our ship's nose). **PDCs** are the point defence guns.
- **The board** / **the battle board** is the main play screen with its panels (Helm, Power, Plot, Sensors,
  Fire Control, Comms, Hangar, Engineering, Orders, Periscope). The **plot** is the map in the middle.
- Distances are in **units**. Time is **game time**: "every second of game time", "per game second".
- Old scripts written earlier by Soham (ships, guns and shots as physics objects) are kept. The battle board
  does not use most of them. Describe them plainly and fairly. Do not judge them.
- Do not say "shields", "boarding pods" or "cockpit" are game features. If an old script has them, describe
  what the script does and add: "The battle board does not use this script."
