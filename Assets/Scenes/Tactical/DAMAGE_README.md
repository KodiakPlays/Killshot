# Damage and damage control

**Summary.** Once the armour on one face of a ship is gone, hits land in the room behind it (a penetrator gets through anyway). Rooms
can catch fire, lose their air (vacuum), or take radiation, and all three spread to neighbouring
rooms. Engineering handles them with the design's existing buttons. A reactor at 0 destroys any
ship, ours included. This is built one step at a time, in the order at the bottom.

**Sources.** The design audit §09 (`References/Star Wolf - Design Audit.dc.html`) is the base. The
handoff (`Assets/UI/Board/Design/reference/board-logic.js`) gives the buttons. The
owner's decisions of 2026-10-24 fill the gaps. Where these disagree, this file says what we do.
Every rate is per game second, so x10 runs everything ten times as fast.

---

## Faces and rooms (audit §09.4)

| Face | Room behind it | What is lost |
|---|---|---|
| Bow | MAG magazine | arming ladder, ordnance |
| Stern | ENG engine room | thrust |
| Port | RCT reactor | power units; at 0 the ship is destroyed |
| Starboard | HGR hangar bay | launch |

- **One fixed room per face**, with no random pick, so you can learn the ship and turn to protect it.
- **Bridge (BRI) and habitation (HAB) are inside.** They are never hit directly; only conditions
  spreading in can hurt them.
- **A hit on a wrecked room** (integrity 0) does no more structural damage, but it can still start a
  condition there.

## Deck plan (owner's choice)

```
          BOW
        [ MAG ]
  [RCT] [ BRI ] [HGR]
  [RCT] [ HAB ] [HGR]
        [ ENG ]
          AFT
```

**Neighbours:**

- MAG: BRI, RCT, HGR
- BRI: MAG, HAB, RCT, HGR
- HAB: BRI, ENG, RCT, HGR
- ENG: HAB, RCT, HGR
- RCT: MAG, BRI, HAB, ENG
- HGR: MAG, BRI, HAB, ENG

## Conditions (audit §09.5)

| | Fire | Vacuum | Radiation |
|---|---|---|---|
| **Starts** | an HE or penetrator hit, by chance | a slug hit, by chance; or VENT | a slug, HE or penetrator hit on the reactor room, by chance (not a fighter's) |
| **Spreads** | to neighbours that have air | to neighbours, unless this room's doors are shut | outward from the reactor, room by room, while it leaks |
| **Does** | integrity −1 every 30 s; no repair work there except fighting the fire | the room can't be crewed, so its system counts as integrity 0; crew caught in it die (half on a breach, all on VENT) | crew in the room die, one every 20 s |
| **Cleared by** | a team (REPAIR), or VENT | SEAL to stop the spread, then REPAIR to restore the air | repair the reactor to 3 or more (or vent it); then it fades from each room after 90 s |

## Buttons (the design's own)

| Button | Does |
|---|---|
| REPAIR | Sends the team. When it finishes: puts a fire out; or, in a sealed airless room, restores the air; otherwise adds integrity (2 at Condition 1). |
| SEAL | Shuts a vented room's doors, so vacuum stops spreading. No crew needed. |
| VENT | Dumps the room's air: the fire goes out, and the crew and any team inside die. |
| REBOOT | Adds 1 integrity to a damaged room; no longer clears a condition. It restarts broken parts in the breakdowns step. |
| DUMMY 1 | Becomes **RECALL**: brings the team back before it finishes. |

REPAIR, SEAL and VENT cost AUX: 30, 25 and 15 (owner, 2026-10-29; BUILD_LOG decision 52). REBOOT
and RECALL are free. With too little AUX the button is refused: `INSUFFICIENT AUX — 30% REQUIRED`.
The room's own reason shows first. The orders station's PARTY, SEAL and VENT do the same, for the
same AUX.

**Changes to the design's buttons:**

- REBOOT no longer clears a condition.
- SEAL no longer restores air at once.
- VENT kills crew.
- DUMMY 1 is given a job.
- REPAIR no longer needs crew in the room: the team comes from elsewhere.
- The orders station follows the same rules as engineering. In the design, its VENT refused with crew
  aboard and its PARTY refused any airless room.

## Numbers

All are in each ship's stats file (`Assets/Ships/Asura.asset`, `Garuda.asset`, `Frigate.asset`). Edit them in **Starwolf → Tuning**, on the Ship or Enemy tab. These are the starting values.

The fire and breach chances are the firing ship's, in its Weapons section (BUILD_LOG decision 49):

| | |
|---|---|
| Fire chance: HE hit / penetrator hit | 60% / 40% |
| Breach chance per slug hit | 50% |

The rest are the damaged ship's, in its Damage section:

| | |
|---|---|
| Radiation chance per reactor hit | 50% |
| Fire: integrity loss | 1 every 30 s |
| Spread chance, per neighbour per second | fire 1%, vacuum 1%, radiation 1% |
| Crew lost on a breach | half the room |
| Radiation: crew loss | 1 every 20 s |
| Radiation fades after the reactor is fixed | 90 s |
| Team: time to finish | 6 s |
| Crew per room: MAG, BRI, HAB, HGR, RCT, ENG | 4, 9, 18, 7, 6, 8 (52 in all; decision 51) |

The AUX costs are in the Orders section: REPAIR 30, SEAL 25, VENT 15.

## Build order

1. **Game time:** every rate per game second, charging included. **Done, Step 57.**
2. **Side map:** the audit's faces and rooms; a reactor at 0 destroys our ship too. **Done, Step 58.**
3. **Conditions:** this file, with RECALL. **Done, Step 59.**
4. **Boarding:** `BOARDING_README.md`. **Done, Step 60.**
5. **Teams** (audit §09.9): travel time, crew taken off stations, recall, losses; the alert level
   sets crew at stations.
6. **Breakdowns** (audit §09.6): parts jam (needs a team) or overheat (clears when you ease off);
   REBOOT.
7. **Displays** (audit §09.10): armour per face, conditions per room, broken parts, team roster,
   door state. Request the art from Claude Design; working placeholders meanwhile.
   **Armour per face started, Step 74:** engineering's ship picture colours each side white
   (full), amber (hit) or red (open), and the room rings go white, amber or red by health
   (`BUILD_LOG.md`, Step 74). Since Step 75 each side is a bright 3 px outline with its panel lines
   at 40% brightness. There is no fill.

## Still open

- What happens when our ship is destroyed: since Step 85 a plain end screen reads SHIP LOST, with
  RESTART MISSION and MAIN MENU. Whether it needs more is still open.
- A room can hold fire and radiation at once, but the design's row shows one condition. The
  displays step settles how both show.
