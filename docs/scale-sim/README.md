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
- **Firing:** a slug at 300 units/s, the slug's path hit test against a 100-unit hit radius (a
  stand-in for each ship's box), and the TRACK button's exact lead.
- **Defence:** PDC-A's 70% chance per missile, assumed to cover half the approaches.
- **The person:** about 4 real seconds to arm each shot (rockers and lever).

`BASE` and the numbers under it at the top of the script are copies of the game's numbers. The
game's numbers are in the ship stats files (`Assets/Ships/Asura.asset`, `Garuda.asset`) and in
`EnemyBrain`. **Keep the script in step with them** when a number changes. The scale `k`
multiplies every speed: `k = 1` is today. All times are real seconds at x1, where one game second
is one real second (Step 130).

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

## What it found (Step 130, the designer's table)

Speed 50, DRADIS 6,250, railgun 2,000, slug 300, missile 400, map 20,000. From first contact to the
kill takes about 2.5 minutes at x1, at any engagement range up to the railgun's reach. Reaching
first contact takes about 4 minutes at top speed. The script leaves out the lock, the gun's slow
turn and the enemy's railguns, so a real fight takes longer.

## Limits

- **Our player is simple:** it flies at the Garuda at 60% throttle until it has a contact, keeps
  station off the Garuda's port beam and fires whenever the reactor face is turned to it. No
  evasion, no missiles, no fighters.
- **Missiles fly straight at us.**
- **It doesn't model** sensors beyond the DRADIS range, conditions, or engineering.
- **It flies at full ENG only.** Since Step 144, ENG sets acceleration, braking and turn rate as well
  as top speed. At ENG 1 our ship has half of each. `BASE` has the full ENG numbers (2, 3.75 and 20),
  and both ships use them all the time. Below full ENG the real ship speeds up and turns more slowly
  than the script's. The script has not been re-run with this.
- **It is a paper model, not the game.** Check a chosen setting in Unity, as Step 63 did.
