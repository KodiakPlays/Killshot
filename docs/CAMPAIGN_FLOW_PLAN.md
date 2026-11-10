# Between missions plan

Everything between two missions: the save slots, the sector map, the ship's rooms, the travel bar, the
end card and the Escape menu. The Campaign Flow chat owns this work (owner, 2026-11-10). What happens
inside a mission belongs to the Tutorial and Mission Manager chat.

Talked through with the owner on 2026-11-10. The design page with game pictures:
https://claude.ai/artifact/UhfPpB8aJoY8zJj3RziEa4

## Where it stands

| Part | State |
|---|---|
| Save slots, sector map, ship rooms | Built (Steps 167 to 172). |
| Missions set up in the Campaign file | Built (Step 183). |
| 1. The Escape menu | Built (Step 190). |
| 2. The end card, and the percent on the save slot | Next to build. |
| 3. Supplies carry over, and the travel bar | Planned. Some numbers to settle before building. |

## The owner's answers, 2026-11-10

1. **Supplies carry over to every mission.** Torpedoes, railgun slugs, PDC rounds and marines, the
   same as damage does today. Owner: "the whole point of Starwolf also is that you will actually raid
   for supplies… Every mission, indeed, it must carry over." Raiding for supplies inside a mission is
   a later design talk.
2. **One repair is one fix, not everything.** Owner's examples: remove a condition, repair a breach,
   restore some ammo ("10% ammo, 20% ammo"). So conditions and breaches must carry over too. Today
   they don't: only side armour and room integrity carry over.
3. **Armour is the cheaper fix.** Cost 1.
4. **Extra ammo does not refill to full.** Owner: "You have to generate the ammo, so you'll get some
   ammo back." Each action gives back a share. The share is a Tuning number.
5. **Each action lives in its room.** Owner: "That's the whole point." ENGINEERING has the repairs.
   HANGAR has torpedoes, ammo and drones. QUARTERS has the marines. BRIDGE has talk.
6. **The end card uses the game's own intro.** After the story's closing line, the STARWOLF intro
   plays exactly as at the start: "Same flicker, same everything. Don't change that." Then the title
   menu.
7. **Build order:** the Escape menu, then the end card and the save slot percent, then supplies
   carrying over with the travel bar. Owner: "Build order is perfectly fine."

**A damage rule the owner gave at the same time.** When a room is hit, the most power its system can
take goes down, and the lost power steps show crossed out. Today only the reactor does this, and it
lowers the ship's total (`PowerSystem.ReactorCap`). This is a rule inside missions, for the chat that
owns damage. It matters here because repairs give those steps back.

## Agreed earlier (owner, 2026-11-09)

- The travel bar: about 10 units across the top of the ship's rooms. Think of it as travel time to
  the next mission. Each action fills its cost. When the bar is full, you go on to the next mission.
- Costs: repair the reactor 3, repair another room 2, restock torpedoes 2, heal the marines 2,
  restock drones 1, extra ammo 1. Armour 1 (2026-11-10). The bar's length and every cost go in
  Starwolf → Tuning.
- No LAUNCH in the rooms. You launch from the sector map.
- The campaign has 10 missions. After the last one, the end card, then the title.
- The save slot reads CAMPAIGN COMPLETE and 100%.
- Escape on the save slots, the map and the rooms opens RESUME, MAIN MENU, QUIT GAME.

My own calls, told to the owner: SECTOR MAP stays usable any time, and leaving early loses the unused
bar. When the bar is full, the actions lock and the SECTOR MAP tab flashes. The free REPAIR ALL goes.
Restock drones stays hidden until drones are built. The save slot's percent is missions won out of
the missions in the Campaign file.

## Still to settle, before step 3

- How much each fix gives back: the share of ammo, of armour, of a room's integrity.
- Which conditions carry over between missions (fires, breaches, radiation), and what removing each
  one costs.
- How much the torpedo restock gives back, if it is not to full either.

## The rooms' art

Only the HANGAR has its painting and person (`Assets/UI/Rooms`). BRIDGE, ENGINEERING and QUARTERS,
and the XO, the Chief Engineer and the Marine Sergeant, are grey stand-ins. The art goes in the Art
List on the rooms screen, one slot per painting and per person.
