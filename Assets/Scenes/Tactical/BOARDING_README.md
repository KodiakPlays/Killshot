# Boarding

**Summary.** The assault boats carry marines to an enemy ship that has an open face and is nearly
stopped. They fight aboard for a minute. If they win, they carry out the order set on the hangar's
order knob: **DESTROY** blows it up, **DISABLE** wrecks its engines and magazine, **SECURE**
captures it. Then they fly home. No design source defined these orders; the owner did on
2026-10-24. Since 2026-10-27, a boarded ship keeps moving and fighting, and the boats go no further
than 15,000 km from our ship. Leave them out of range too long and they are lost.

**Sources.**

- The handoff's hangar (`board-logic.js` SQUADS): the ASSAULT BOAT squadron, its four boats, and the
  orders DESTROY, DISABLE and SECURE. These are kept exactly as the design has them.
- The owner's decisions of 2026-10-24 for what each order does and how boarding works.
- Damage, crew and conditions are in `DAMAGE_README.md`.

---

## The run

1. **Launch** as for fighters: hangar stages, launch lever, a target from fire control. Each ready
   boat carries 6 marines from the ship's 18.
2. **Fly out** at 30 km/s, drawn as the fighters' wedges.
3. **Hold off** until the target can be boarded. The hangar line says why it is waiting:
   - "HOLDING OFF GARUDA · NO OPEN SIDE": no face of its armour is broken;
   - "HOLDING OFF GARUDA · TARGET UNDER WAY": it is moving faster than 0.5 km/s.

   You have to fight it down first: wreck its engine room, or knock its power out with EMP.
   Targets are named as everywhere else: "BOGEY T07" until an active scan identifies it, then
   "GARUDA".
4. **Board.** The boats dock and the marines fight for 60 game seconds: "BOARDING GARUDA · 42 S".
   Once they are aboard, the ship may get under way again (its EMP wears off). It keeps moving and
   firing until the fight is over. The boats ride with it, and the fight goes on (owner 2026-10-27).
5. **The odds:** marines ÷ (marines + defenders). A quarter of the enemy's crew defend.
   - **Win:** the defenders die; we lose half a marine per defender; the order is carried out.
   - **Lose:** all our marines aboard die; they kill half their number of the enemy's crew.
6. **Home.** The boats fly back to our ship and land. The surviving marines rejoin the pool. With no
   marines left, the launch refuses: "NO MARINES ABOARD".

## Range (owner, 2026-10-27)

The boats go no further than **15,000 km** from our ship.

- **Outbound:** they chase the target while it is inside that range. If it gets further away, they
  give up and turn home: "OUT OF RANGE · RETURNING · 3,000 KM".
- **Coming home:** if our ship is more than 15,000 km from them, they are stranded. The alert tone
  sounds and a clock runs: "STRANDED · CLOSE IN · 87 S". Close in to collect them. The clock stops
  while they are in range, but it does not go back.
- **Lost:** when the clock runs out, the boats and the marines in them are gone. The alert sounds
  again. The boats show as LOST, in red, and cannot launch.

**RECALL** at any time turns the boats home. Marines already aboard withdraw with no result.

## The orders

| Order | When the marines win |
|---|---|
| DESTROY | They blow its reactor: the ship is destroyed, a wreck. "GARUDA SCUTTLED" |
| DISABLE | They wreck its engine room and magazine (integrity 0): it comes to a stop, alive, and cannot fire. "GARUDA DISABLED" |
| SECURE | They take it: it stops fighting and is a prize, shown on the plot as an own-force dot. "GARUDA SECURED" |

## Numbers

All are in each ship's stats file (`Assets/Ships/Asura.asset`, `Garuda.asset`), in the Hangar
section. Edit them in **Starwolf → Tuning**, on the Ship or Enemy tab.

| | |
|---|---|
| Boat speed | 30 km/s |
| Marines: ship's pool / per boat | 18 / 6 |
| Docking distance | 50 km |
| Target speed to board below | 0.5 km/s |
| Fight aboard | 60 s |
| Share of enemy crew defending | 25% |
| Marines lost per defender, on a win | 0.5 |
| Boat range from our ship | 15,000 km |
| Clock out of range, coming home | 120 s |

## Still open

- Salvage from a secured prize, and what a prize is for.
- Fuel: the boats have a range and a clock, not fuel. The design gives each craft a fuel figure.
  Fighters have no range yet.
- Enemy boats boarding us.
- The prize's own glyph on the plot.
