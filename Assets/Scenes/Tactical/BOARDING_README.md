# Boarding

**Summary.** The assault boats carry marines to an enemy ship that has an open face and is nearly
stopped. They fight aboard for a minute. If they win, they carry out the order set on the hangar's
order knob: **DESTROY** blows it up, **DISABLE** wrecks its engines and magazine, **SECURE**
captures it. Then they fly home. No design source defined these orders; the owner did on
2026-10-24.

**Sources.**

- The handoff's hangar (`board-logic.js` SQUADS): the ASSAULT BOAT squadron, its four boats, and the
  orders DESTROY, DISABLE and SECURE. These are kept exactly as the design has them.
- The owner's decisions of 2026-10-24 for what each order does and how boarding works.
- Damage, crew and conditions are in `DAMAGE_README.md`.

---

## The run

1. **Launch** as for fighters: hangar stages, launch lever, a target from fire control. Each ready
   boat carries 6 marines from the ship's 18.
2. **Fly out** at 6 km/s, drawn as the fighters' wedges.
3. **Hold off** until the target can be boarded. The hangar line says why it is waiting:
   - "HOLDING OFF TRACK 07 · NO OPEN SIDE": no face of its armour is broken;
   - "HOLDING OFF TRACK 07 · TARGET UNDER WAY": it is moving faster than 0.1 km/s.

   You have to fight it down first.
4. **Board.** The boats dock and the marines fight for 60 game seconds: "BOARDING TRACK 07 · 42 S".
5. **The odds:** marines ÷ (marines + defenders). A quarter of the enemy's crew defend.
   - **Win:** the defenders die; we lose half a marine per defender; the order is carried out.
   - **Lose:** all our marines aboard die; they kill half their number of the enemy's crew.
6. **Home.** The boats fly back to our ship and land. The surviving marines rejoin the pool. With no
   marines left, the launch refuses: "NO MARINES ABOARD".

**RECALL** at any time turns the boats home. Marines already aboard withdraw with no result.

## The orders

| Order | When the marines win |
|---|---|
| DESTROY | They blow its reactor: the ship is destroyed, a wreck. "TRACK 07 SCUTTLED" |
| DISABLE | They wreck its engine room and magazine (integrity 0): it drifts, alive, and cannot fight. "TRACK 07 DISABLED" |
| SECURE | They take it: it stops fighting and is a prize, shown on the plot as an own-force dot. "TRACK 07 SECURED" |

## Numbers

All are in **Starwolf → Tuning**, under "Hangar & fighters".

| | |
|---|---|
| Boat speed | 6 km/s |
| Marines: ship's pool / per boat | 18 / 6 |
| Docking distance | 50 km |
| Target speed to board below | 0.1 km/s |
| Fight aboard | 60 s |
| Share of enemy crew defending | 25% |
| Marines lost per defender, on a win | 0.5 |

## Still open

- Salvage from a secured prize, and what a prize is for.
- Fuel and range: craft fly back to wherever our ship is, so nothing runs dry yet. The design gives
  each craft a fuel figure.
- Enemy boats boarding us.
- The prize's own glyph on the plot.
