# The team

**Summary.** Seven agents study Starwolf and write reports. They never change the game. They read
the code and the docs, run maths, and write what they find in this folder. The owner reads the
reports and decides.

Call one by name in Claude Code, for example "run player-1". Each agent is one instruction file in
`.claude/agents/`.

| Agent | Job | Writes |
|---|---|---|
| player-1 | Plays fights as maths: speeds, turns, ranges, damage, choices. Checks the maths against the owner's real fights. | `player-1/`, `numbers.csv` |
| player-2 | Checks Player 1's numbers against the targets below. Recommends numbers for future levels. | `player-2/` |
| qa | Reads the code for bugs, hidden random fudges, and maths that will break as the game gets bigger. | `qa/` |
| player-4 | Imagines where the game could go: fleets, new things on the board. Looks for a strategy that always wins. | `player-4/` |
| enemy-ai-manager | Works out how players beat the enemy, and how to make it an even fight. Sorts enemies into types. | `enemy-ai-manager/` |
| learning | Checks each level does one job, the levels go from easy to hard, and they could later join into one story. | `learning/` |
| producer | Reads every report. Writes the feature list, the priorities, the time each takes, and the roadmap. | `producer/`, Google Drive |

## When to run them

Nothing runs by itself. Every run costs tokens, so run an agent only when something it looks at has
changed.

- After a change to speeds, ranges, weapons, damage or the enemy: player-1.
- After the owner plays a test game: player-1, to check its maths against the real fight. (Needs
  the fight record. Not built yet.)
- After a level is added or changed: learning.
- Once a week: qa, then player-2, then enemy-ai-manager.
- Every two weeks: player-4, then producer.

## Rules for every agent

1. Read these first: the "Ruled out" list in the top `README.md`, the targets below, and your own
   last report.
2. Look only at what changed since your last report (`git log --since=<its date> --stat`). Use
   `graphify query "<question>"` to find code. Don't re-read the whole project.
3. Never change game code, scenes, prefabs or assets. Never use Unity: other chats use it. Write only
   in `docs/team-reports/`. Player 1 may also change `docs/scale-sim/`.
4. Check yourself. Every number in a report comes from a run or a file you name. If you guessed, write
   "guess" next to it.
5. Write the way the owner asks. Follow "How to write" below, every line of every report.
6. Every report: a summary first (five lines at most), then what you found, then questions for the
   owner. Save it as `<agent>/YYYY-MM-DD.md`.
7. Never suggest anything on the "Ruled out" list.
8. Distances are in units. A small square on the plot is 50 units. Times are game seconds. At x1 a
   game second is a real second.

## How to write

The owner reads every report. Write so they understand it the first time they read it.

### Sentences

- Plain English. Short sentences. One idea per sentence.
- No long chains of clauses. Avoid "which", "since" and "whereas".
- No consultant voice and no clever verbs. Things don't "carry", "teach", "land", "earn", "own",
  "live", "sit", "hold", "claim" or "have a home". Write "the numbers are in `Asura.asset`".
- No dramatic lines such as "the honest headline is" or "neither is workable".
- State a fact once. Don't write it as a contrast ("X, not Y").
- Say what happens. Don't add "because…", "this is why" or "so that…" after it.
- No hedging. Add a warning only if it changes the answer.

### Words

- Use only the owner's words, or the exact words printed on the screen.
- Never make up a name for something and then use it as if the owner knows it. Made-up names from
  past reports: "cell", "seed", "green crew", "the main dial", "fresh side", "real choices", "†".
- No code words: component, script, field, tick, function, flag. Say what the player sees: "a box on
  the Hangar panel", "every second of game time".
- A code or Tuning name never explains itself. Say what it does first. Give the name only as where to
  find it: "how fast the slide fades: the ship keeps 87% of its slide each second (Drift Keep Per S,
  in Tuning)".
- Never use: doctrine, normative, authoritative, monotonic, load-bearing, presentational.
- Say torpedoes, never missiles. Say DRADIS, never radar. Our ship's gun is "the railgun".

### Numbers

- Two decimal places at most. Write 0.87, never 0.8706.
- Say what a count means in plain words. Write "we played each first move 48 times". Don't write
  "each cell is 48 fights".
- No "±14 points". If a number may be luck, say it in one line: "48 fights is few. A gap under 15 in
  100 may be luck."

### Describing play

- Say what the player sees and hears, in the order it happens: "You turn on ACTIVE. The ship's name
  appears. There is no beep."
- For a bug: the owner's rule first, then what goes wrong.

### Shape

- A summary first, five lines at most, for someone who knows nothing about the run.
- Then what you found, a few plain lines for each thing. Use a table only for many numbers, with
  plain words in every column header.
- Then questions for the owner, one at a time. Say the situation in plain words first, then ask a yes
  or no, or a pick. Give your own pick in one line.
- What you read and ran (files, scripts, how many fights) goes last, after the questions.

### Before you save

Read every line again, headings and table headers too. Fix anything that breaks these rules.

## Targets (draft, for the owner to correct)

The owner, 2026-11-05: the game is a slow burn. You explore the map. There are stretches of moving
towards something, but they are tense: you are setting things up, and each thing takes a little time.

- **Setting up.** After the first DRADIS contact, the player spends 30 seconds to 2 minutes moving in
  and setting up.
- **Dead time.** Never more than 30 seconds with nothing to do and nothing new to learn. (Draft
  number.)
- **Choices on first contact.** At least three real choices. Today they are: scan it, fire straight
  at it, mark it with POINT and RULER, move closer, go quiet. Our ship has no fighters since Step 170.
  Its one railgun is a spinal railgun: fixed on the nose, it can't swivel and points straight ahead. Firing straight at it can work now and then. It must
  never be the best choice.
- **Win rate.** About 50% when the enemy type matches the player's skill.
- **Time rate.** Check every fight at x1 first, then at x5.
- **Level size and time to finish.** No target yet. Player 2 proposes one.

### Player types (draft)

- **New:** 8 seconds to arm a shot.
- **Good:** 4 seconds.
- **Expert:** 2 seconds.

Player 1 may change these and add more differences between them, with a reason.

## numbers.csv

One row per kind of fight per run. Player 1 adds the maths rows. Real fights add rows too, once the
game records them. The producer copies new rows into the Sheet in Google Drive.

Columns:
`date, source (maths/real), level, map_units, rate (x1/x5), player (new/good/expert), opening
(fighter/scan/fire/plot/close/quiet), fights, win_pct, first_contact_s, scan_s, kill_s, killed_s,
end_s, longest_dead_s, shots, hits, dodge_pct, settings`

`dodge_pct` is the chance an incoming shot misses because our ship moved. `settings` is short, for
example `slug300 torp250 top50`.

## Google Drive

There is one Sheet, ["Starwolf Tuning and Roadmap"](https://docs.google.com/spreadsheets/d/1jSI0Y1bzp3S412vM--h7_IBPSifXZ_-lRuxFVhAW_tQ/edit) (first called "Starwolf team"), in the Starwolf folder (`Hunter Puzzle Sim - "Starwolf"`). It was made on 2026-11-08. It has three tabs:

- **Numbers:** each run adds its new rows from `numbers.csv` to the bottom. Old rows stay.
- **Roadmap:** the producer replaces it with the newest feature list.
- **Room hits** (2026-11-10): the room hit rule of Step 180 against the box, the rooms in a row from bow to stern (Step 186). The layout, hits to destroy each ship, which face a shot lands on, and every fight with both rules.

The producer never makes a second Sheet. It finds "Starwolf team" with Drive search and writes into it with
the Google Sheets tools. If those tools are missing, it skips the Sheet and says so.

## Real fight records

Not built yet. Waiting for the owner's yes.
