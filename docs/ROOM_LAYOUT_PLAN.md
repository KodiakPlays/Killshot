# Room layout plan

Built on 2026-11-09, in Step 180 (the owner said yes to the six steps below). The idea is the owner's. Lines
marked "my pick" are my calls. Any of them can change.

## Summary

Each ship has six rooms: the magazine, bridge, habitation, hangar bay, reactor and engine room. You
set where each room is in **Starwolf → Tuning**, on the Hard Points tab. You give each room two
things:

- **A side:** PROW, STBD, AFT or PORT.
- **An order number:** 1 is right behind that side's armour. 2 is behind that, and so on. Two rooms
  can have the same number. Then they are side by side, on the same line.

Next to the picture you tune each side's armour. Each ship has its own layout, in its ship file in
`Assets/Ships`. You don't drag rooms around.

Owner, 2026-11-09: "I really just need to do two things in a ship layout… a sequence in which they
exist, so an order, which is non-exclusive… two compartments can be 1 and 2, for example, or 2 and
2, which means that they are on the same line… you are pegging a compartment to a side, which is
one of four sides, and you're setting a sequence… I don't need to drag things around."

## How it plays

Owner, 2026-11-09: "If the magazine is destroyed, then it should hit something else… once a side
is open, then what is exposed? … if a room is destroyed, then what comes after it?"

A hit on a side follows these steps:

1. **The armour.** While the side has armour, the hit damages only the armour.
2. **The front room.** Once the armour is gone, the side is open. The hit goes to the room with the
   lowest order number on that side. This is the exposed room.
3. **A wrecked room is a hole.** A room at 0 integrity takes no more hits. The hit goes through it
   to the next order number on that side.
4. **Rooms on the same line take turns.** If two rooms on that side have the same order number,
   the hit goes to the one with more integrity left. If they are level, it goes to the one higher in
   the list. So they take turns. When one is wrecked, the other takes them all.
5. **Through the ship.** If every room on that side is wrecked, the hit goes on into the rooms of
   the opposite side, from the inside out. PROW goes on into AFT. STBD goes on into PORT. The
   highest order number on that side comes first, and its order 1 room comes last.
6. **Out the other side.** If every room on both sides is wrecked, the hit does nothing more.

One hit damages one room. Damage left over doesn't carry on to the next room.

**An example, with the starting layout below.** Shots keep hitting the PROW. First the PROW armour
goes. Then the magazine is wrecked. Then the bridge. Then the hit goes on into the AFT side: first
habitation (AFT 2), then the engine room (AFT 1). After that, PROW hits do nothing.

A shot through STBD wrecks the hangar, then goes on into the reactor (PORT 1). The reactor at 0
destroys the ship.

The targeting computer's hull line ("STBD ▸ HGR 3") shows the room the next hit would damage.

Later, boarding uses the same steps. The marines come in through the open side, into the exposed
room. That is for the boarding talk, not this plan.

## What you see in Tuning

- The Hard Points tab gets a switch at the top: **WEAPONS** or **ROOMS**. WEAPONS is the tab as it
  is today.
- ROOMS shows the same rectangle, nose up, with no dots. Each side shows its armour number.
- Inside the rectangle, each room's short name (MAG, BRI, HAB, HGR, RCT, ENG) is written along its
  side. Order 1 is next to the edge. Order 2 is further in. Rooms on the same line are side by side.
- Under the picture there is one row per room: its name, a side to pick, and its order number (1 to 6).
- Under that, the armour for each side.
- A side with no rooms shows a note: a hit through that side goes straight on into the opposite side's rooms.
- Every change can be undone, and it is saved in the ship file, as the dots are.

## The starting layout

Every ship starts with this layout. It plays the same as today.

| Room | Side | Order |
|---|---|---|
| Magazine | PROW | 1 |
| Bridge | PROW | 2 |
| Engine room | AFT | 1 |
| Habitation | AFT | 2 |
| Hangar bay | STBD | 1 |
| Reactor | PORT | 1 |

Today the bridge and habitation are inside the ship and are never hit. Here they are second in line.
So with the steps above they can now be hit. Today a wrecked room takes every later hit, and
nothing behind it is hurt. This changes fights: Player 1 runs the fight sums again after the build.

## Fight sums (2026-11-10)

The fight sums (`docs/scale-sim/scalesim.py`) now play the six steps. Every fight was run twice on
the same ship files, levels and dice: once with the old rule, once with the six steps. The full
numbers are in the "Room hits" tab of the Google Sheet
[Starwolf Tuning and Roadmap](https://docs.google.com/spreadsheets/d/1jSI0Y1bzp3S412vM--h7_IBPSifXZ_-lRuxFVhAW_tQ/edit).

- **Only STBD and PORT reach the reactor.** With the starting layout, STBD gets there after the
  hangar. PROW and AFT hits wreck the other four rooms, then do nothing. Slug hits to destroy the
  Garuda: PORT 6, STBD 9 (old rule: PORT 6, STBD never). Our ship: PORT 4, STBD 6 (old rule: PORT 4,
  STBD never).
- **Most hits land on a side.** The Garuda is 350 long and 100 wide. A shot aimed at its centre hits
  PROW or AFT only within 16° of dead ahead or dead astern. Shots spread evenly over the ship, from
  every direction, land on STBD or PORT 71% of the time (our ship 73%).
- **Wins, old rule to new.** Wolf Hunt x1 14% to 41% (expert players 17–54% to 71–92%); x5 still 0%.
  LONG SHOT x1 40% to 100%, x5 19% to 55%. Players who came in on the Garuda's STBD side used to
  spend every shot on a wrecked hangar. Now the hits go on into its reactor. COLD RUN x1 47% to 34%,
  x5 26% to 19%. INTERCEPT x1 47% to 34%, x5 27% to 20%. WEAPONS FREE, NO PEEKING and FINDERS
  KEEPERS: no change.
- **Why the chases got harder.** Weapons are off there, and the Garuda hits our STBD side most as we
  run. Hits now go through our wrecked hangar into our reactor.
- Our ship's file has the bridge at AFT 2, saved after the build. The fights used it as saved. It is
  an open item in `BUILD_LOG.md` §8.

## Other rules (my picks)

- On the same line, there are no dice: more integrity first, then the room higher in the list.
- Fire, vacuum and radiation spread as today. Which rooms touch each other is a fixed list in the
  code. Moving a room doesn't change which rooms it touches.
- The Garuda's fighters on DISABLE always hit the AFT today, because the engine room is there. They
  will go for the side the engine room is on.
- What each room does stays the same. The magazine still feeds the guns. The reactor at 0 still
  destroys the ship.

## Later: a separate setup for conditions

Owner, 2026-11-09: "over time, we need a separate conditions setup. I think we have to figure that
out." Fire, vacuum and radiation: which rooms touch, how they spread, what starts them. Not part of
this plan. To be talked through first.

## Owner's answers

1. Build the six steps in "How it plays" as written? Yes (2026-11-09): "It should start getting
   breached… these fights are meant to be short. Your weapons are lethal… Just as you can get killed
   very quickly by the Garuda or the Cruiser, you can wreck it as well."

## How to build it

For the session that builds it.

- `DamageSystem.Stats` (the Damage section of the ship file) gets a side and an order for each
  room. They go in the same room order as Crew: MAG, BRI, HAB, HGR, RCT, ENG. They replace the fixed
  `BehindSide` list in `DamageSystem.cs`.
- `DamageSystem.NextRoom` picks the room by the six steps in "How it plays": lowest order on that
  side with integrity above 0; on the same line more integrity first, then list order (no counter);
  then the opposite side from the highest order down; then nothing. `Behind` uses it.
- `RoomBehind` and `BehindSide` are removed. `PeriscopePanel.HullLine` uses `NextRoom`.
- `TakeFighterRun` on DISABLE uses the engine room's side.
- `StarwolfTuningWindow.DrawHardPoints` gets the WEAPONS / ROOMS switch and the ROOMS picture and rows.
- `docs/TUNING_LOG.md`: one row for the move. Old value: fixed in code, MAG behind PROW, HGR behind
  STBD, ENG behind AFT, RCT behind PORT, BRI and HAB inside and never hit. New value: the starting
  layout above, in each ship file.
- Playtest: the damage checks must still pass with the starting layout. Add one check: a room moved
  to another side takes the hit on that side.
- Docs: `DAMAGE_README.md` (rooms and sides), a step row in `BUILD_LOG.md`, `CONTEXT_INDEX.md`.
- Unity is shared. Message the other chats before compiling or entering play mode.
