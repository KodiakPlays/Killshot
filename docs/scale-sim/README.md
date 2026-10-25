# Scale simulator

**Summary.** A small script that plays whole fights with the board's own rules and numbers, and
reports how long a win takes, how many shots are traded, and whether the plot tools can read
anything. It decided the 5× speed scale in Step 63. Re-run it after any change to speeds, ranges,
weapons or the enemy, and compare the tables.

**Run:** `python3 docs/scale-sim/scalesim.py`. Plain Python, no packages, under a second.

## What it copies from the game

- **Ships:** our ship's movement and turning (`HandoffBoardState.TickMove`, `TickHelm`).
- **TRACK 07:** its patrol (`EnemyBrain`), and its fire-back: 2 HE missiles when hit, at most every
  120 s, 8 cells.
- **Damage:** the side map, where the reactor sits behind the port face (`DAMAGE_README.md`). A slug
  takes 50 armour, then 2 off the reactor.
- **Firing:** the slug's path hit test against a 210 km hit radius, with TRACK's exact lead.
- **Defence:** PDC-A's 70% chance per missile, assumed to cover half the approaches.
- **The person:** about 4 real seconds to arm each shot (rockers and lever).

`BASE` at the top holds the game's numbers. **Keep it in step with the game** when a number
changes. The scale `k` multiplies every speed: `k = 1` is today, `k = 0.2` the old speeds.

## What it reports

1. **Fights.** For each scale and engagement range: time from detection to kill in real minutes at
   x1, x5 and x10; our shots fired, hits on the reactor face, and hits on other faces; their
   missiles fired and landed. "n/a" means no kill: usually TRACK 07 won.
2. **Plot read.** With the fight framed on screen: the zoom needed, how far TRACK 07 moves between
   two marks 30 real seconds apart, how well that line reads its course, and how far apart two
   bearing lines laid 2 game minutes apart are.

## What it found (Step 63)

At the old speeds a fight took 35 minutes at x1, and two marks read a course only to ±20°. At 5×:
about 7 minutes, and ±4°. The engagement range barely changes the time; getting round to the
reactor face takes most of it. At x5 and x10 a person can't arm as fast as the game runs, and TRACK
07 can win.

## Limits

- **Our player is simple:** it holds station off TRACK 07's port beam and fires whenever the
  reactor face is turned to it. No evasion, no missiles, no fighters.
- **Missiles fly straight at us.**
- **It doesn't model** sensors beyond the DRADIS range, conditions, or engineering.
- **It is a paper model, not the game.** Check a chosen setting in Unity, as Step 63 did.
