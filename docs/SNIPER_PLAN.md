# Sniper ship plan

A plan. Step 1 is built (Step 170). The rest is not built yet. The ideas are the owner's, from
2026-11-08. The numbers are my picks, and any of them can change.

## Summary

Our ship, the Asura, becomes a sniper. It has one big gun on the nose, a spinal railgun. The
railgun reaches much further than DRADIS. The gun doesn't turn. You aim it by turning the whole ship.

To shoot that far you need eyes out there. You lay sensor drones, like laying mines. They show you
ships as dots far past your own DRADIS. You plot the enemy's course with POINT and RULER, stop the
ship dead, turn the nose to where the enemy will be, and fire.

Your E-SIG is a circle round your ship. Any ship inside it learns where you are. Firing makes it
jump out fast, and it shrinks back slowly. A long shot at a ship outside the circle hits it without
giving you away.

Up close, the ship has lasers, PDCs and torpedoes. It carries assault boats for boarding. It has
no fighters.

## How a long shot plays

1. You fly out and drop sensor drones where the enemy patrols. Or you send an assault boat to drop one.
2. You fly back out of reach and hide, for example in a gas cloud.
3. You turn power down everywhere except WEP. Your E-SIG drops.
4. A drone picks up the Garuda. It shows as a dot, again every few seconds.
5. You plot its course with POINT and RULER.
6. You stop the ship and kill all drift.
7. You pick RAILGUN. A thin line runs out of the nose to the railgun's full reach.
8. You measure with the RULER. It tells you how long the slug takes to fly that far.
9. You work out where the Garuda will be by then. You turn the nose onto that spot with the slow turn.
10. LOAD, ARM, pull the lever. Your E-SIG circle jumps out, and the slug flies.
11. If the Garuda was outside your E-SIG circle, it is hit and only knows which way the shot came from.

## The parts

### The spinal railgun

- The name (owner, 2026-11-09): the spinal railgun. It was first called the Nuclear Lance. The
  owner: "Keep it railgun… it is a spinal railgun in this current configuration."
- One gun, on the nose dot in Hard Points.
- It doesn't turn. It fires straight out along the line from the nose.
- Reach: 30,000. Slug speed: 1,500 a second. A full-reach shot takes 20 seconds.
- Damage: the same as today's railgun slug for now. A new damage type comes later.
- It uses the capacitor, the same as the railgun does today.
- The weapon picker says RAILGUN. The mount screen says RAILGUN and FIXED 000°.
- The two side railguns, RG-A and RG-B, come off our ship. The lasers go on their dots later.

### Aiming

- With the railgun picked, the plot shows a thin line from the nose out to 30,000. Its width is the
  slug's width.
- The RULER also shows how long a slug takes to fly the ruler's length.
- By eye, the game does not work out the lead for you. With a lock, it does, and it turns the ship
  onto it (Step 170).
- Slow turn for aiming: hold Shift while you turn, and the ship turns at 2° a second. Normal
  turning stays at 20° a second.

### Drift

- A slug keeps the ship's motion as it leaves the gun. If the ship drifts, the shot drifts too. The
  longer the shot, the further it misses.
- You can fire while moving. You just miss.
- The Garuda's slugs follow the same rule. Its aim is fixed to allow for it.

### The E-SIG circle (was "the flare")

**Owner, 2026-11-09:** "It's not the railgun's flare. It's how much the eSig expands." There is no
separate flare for each weapon. The old per-weapon sizes (15,000, 7,500, 3,000, 2,000, 1,000) are
dropped. There is one circle: your E-SIG.

- Your E-SIG is a circle round your ship. Any ship inside it learns where you are.
- It never gets smaller than a small minimum, even with everything off.
- It grows as you use systems: power on the buses, ACTIVE SCAN, a lock.
- It jumps out fast when you do a lot at once, such as firing. Then it cools and shrinks back slowly.
- The circle shows on the plot for now. We hide it later.
- A hit from outside the circle tells the enemy only which way the shot came from. It turns and
  searches down that line. A fire on board or a bump into a rock tells it nothing. (Owner's yes,
  2026-11-09.)
- A gas cloud cools the ship, as today: it shrinks the steady part, not a shot's jump.
- Rocks cut it the same way they cut DRADIS.
- Every number is in Starwolf → Tuning.
- The Garuda gets the same circle, as part of following our rules.

Starting numbers, **agreed by the owner 2026-11-09** ("I'm open to experimentation… make the
largest even 20,000"): smallest 2,500 (the enemy sees you inside 2,000 anyway); largest 20,000 at
full signal; a railgun shot adds 25 to the signal, as today, and a torpedo less; cooling 2 a second
(today 6), so a big jump takes about 50 s to calm down. All four go on the Ship tab of Starwolf →
Tuning, so the owner can change them. Log the old values in `docs/TUNING_LOG.md` when built.

### The sight in fire control

**Paused by the owner, 2026-11-09:** "let's put a pin in this, and we'll figure it out." Then: "I
don't want these things stuck together. Either we'll build one clean new UI, or let them be separate
for now in their existing and current incarnation." So the periscope and fire control stay as they
are now. Don't build this until the owner raises it. The plan below is kept as it was.

- The periscope no longer drops from the top of the board.
- Its sight moves into the fire control drawer, above the weapon picker. The drawer slides in from
  the side as it does now.
- With the railgun picked, the sight looks straight out of the nose. You turn the ship and watch the
  target slide into the middle. With torpedoes or PDCs picked, you slew it as today.
- Locks work as today, inside DRADIS. You can fire the railgun by eye. A lock turns the ship onto the
  target for you (built in Step 170).
- The red lines in the sight don't change. I take a screenshot before and after.
- The ANGLE and BEARING switches go. Every weapon fires with LOAD, ARM and the lever.
- TRACK and the dials stay for torpedoes. With the railgun picked they turn off.
- I show you a picture of the new drawer before I build it.

### Sensor drones

**Owner, 2026-11-09:** "we're replacing interceptors with drones that need to be deployed." The
drones take the hangar's empty INTERCEPTOR row, and you deploy them from the hangar. How deploying
works is still to be agreed with the owner. The DROP key and the boat drop below are the older idea.

- Our ship carries 4.
- A DROP key in the hangar drawer drops one where the ship is.
- Or you send an assault boat. Put a POINT on the plot, then give a boat the DRONE order in the
  hangar drawer. It flies to the newest POINT, drops one drone, and flies back. One drone per trip.
  Boats reach 4,000, as now.
- A drone hears ships in a 5,000 circle. Each ship it hears shows as a dot where the ship is, every
  5 seconds. The dots fade.
- Drones show dots only. You still need visual range to see a ship's picture.
- Rocks cut a drone's reach the same way they cut DRADIS.
- Drones last the whole level. The enemy can't find them for now.
- Later: drones that link to each other.

### Lasers (later)

- **Owner, 2026-11-09:** lasers are a new weapon type. They hit instantly, and you have to get
  pretty close to fire them.
- One on each side, on the dots the railguns leave.
- Instant hit. Reach 1,000. One shot every 3 seconds. About half a slug's damage.
- Firing one makes your E-SIG jump a little.

### Hangar

- Our ship launches no fighters. The Asura's fighter number goes to zero. Nothing is deleted.
- Assault boats stay as they are.

### The Garuda

- Its weapons don't change: two swivel railguns (slug speed 300, reach 2,000), PDCs and torpedoes.
- It follows the E-SIG circle and drift rules, the same as us.
- **Owner, 2026-11-09:** it follows all our weapon rules: a lock, a charge, crew arming each shot,
  a slow gun, torpedoes that need a scan or a lock, PDCs that fire. "Otherwise this is never going
  to work. You have to have a pretty fair chance of winning."

### A test level

- One new test level, LONG SHOT. The map is 40,000 across.
- The Garuda patrols a route. There is a gas cloud to hide in, and some rocks.
- What you do there: lay drones, hide, and make one long shot.

## Build order

1. The spinal railgun and aiming: the nose gun, reach, speed, drift, the slow turn, the line from the nose,
   and the ruler's slug time. The side railguns and the fighters come off our ship. The LONG SHOT
   test level.
   **Built in Step 170, 2026-11-09.** None of the plan's numbers changed. The ANGLE and BEARING
   switches still show; step 3 takes them away. LONG SHOT's Garuda doesn't hunt. Added on the owner's
   word (2026-11-09): a lock turns the ship onto the target by itself. Only a lock does this. A TRACK
   solution from the plot does not.
2. The E-SIG circle, shown on the plot, with the hit rule. (Was "the flare"; see "The E-SIG
   circle" above.)
3. The sight in fire control. ANGLE and BEARING go. **Paused by the owner, 2026-11-09** (see "The
   sight in fire control" above). ANGLE and BEARING stay until then.
4. Sensor drones.
5. Lasers.
6. A new damage type, once the owner says what it should be.

Before step 1, I run the scale sums (`docs/scale-sim/README.md`) with the new numbers. Each step
gets a quick check in play. One full run at the end.

## Older calls this changes

- "No spinal railgun" (2026-09). Our ship now has one on the nose.
- The side railguns' 90° cone and turning bar (Step 121 in `BUILD_LOG.md`). They stay on the
  Garuda only.
