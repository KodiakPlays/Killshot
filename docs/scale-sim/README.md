# Scale simulator

**Summary.** A small script that plays whole fights with the board's own rules and numbers, and
reports how long a win takes, how many shots are traded, and whether the plot tools can read
anything. It decided the 5× speed scale in Step 63. Re-run it after any change to speeds, ranges,
weapons or the enemy, and compare the tables.

**Run:** `python3 docs/scale-sim/scalesim.py`. Plain Python, no packages, under a second.

## What it copies from the game

- **Ships:** our ship's movement and turning (`NavSystem.Tick`, which runs `Steer` and `Move`).
- **The Garuda:** its patrol (`EnemyBrain`), and its fire-back: 2 HE missiles when hit, at most every
  120 s, 8 cells.
- **Damage:** the side map: the reactor is behind the port face (`DAMAGE_README.md`). A slug
  takes 50 armour, then 2 off the reactor.
- **Firing:** a slug at 2,500 km/s (BUILD_LOG decision 46), the slug's path hit test against a
  210 km hit radius, and the TRACK button's exact lead.
- **Defence:** PDC-A's 70% chance per missile, assumed to cover half the approaches.
- **The person:** about 4 real seconds to arm each shot (rockers and lever).

`BASE` and the numbers under it at the top of the script are copies of the game's numbers. The
game's numbers are in the ship stats files (`Assets/Ships/Asura.asset`, `Garuda.asset`) and in
`EnemyBrain`. **Keep the script in step with them** when a number changes. The scale `k`
multiplies every speed: `k = 1` is today, `k = 0.2` the old speeds.

## What it reports

1. **Fights.** For each scale and engagement range: time from detection to kill in real minutes at
   x1, x5 and x10; our shots fired, hits on the reactor face, and hits on other faces; their
   missiles fired and landed. "n/a" means no kill: usually the Garuda won.
2. **Plot read.** With the fight framed on screen: the zoom needed, how far the Garuda moves between
   two marks 30 real seconds apart, how well that line reads its course, and how far apart two
   bearing lines laid 2 game minutes apart are.

## What it found (Step 63)

At the old speeds a fight took 35 minutes at x1, and two marks read a course only to ±20°. At 5×:
about 7 minutes, and ±4°. The engagement range barely changes the time; getting round to the
reactor face takes most of it. At x5 and x10 a person can't arm as fast as the game runs, and the
Garuda can win.

These results used the Step 63 slug speed of 250 km/s. The slug has flown at 2,500 km/s since Step
81. Re-run the script for new numbers.

## Limits

- **Our player is simple:** it keeps station off the Garuda's port beam and fires whenever the
  reactor face is turned to it. No evasion, no missiles, no fighters.
- **Missiles fly straight at us.**
- **It doesn't model** sensors beyond the DRADIS range, conditions, or engineering.
- **It is a paper model, not the game.** Check a chosen setting in Unity, as Step 63 did.
