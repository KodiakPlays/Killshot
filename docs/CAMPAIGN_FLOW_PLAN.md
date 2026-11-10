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
| Missions set up in the Campaign file | Built (Step 183). Set them up in Starwolf → Campaign (Step 205). |
| 1. The Escape menu | Built (Step 190). |
| 2. The end card, and the percent on the save slot | Built (Step 194). Since Step 203 the end card always opens after the last win. The closing line in `Assets/Campaign/Campaign.asset` → Closing Line stays empty for now (owner 2026-11-11), so the end card goes straight to the title, where only STARWOLF flickers in. |
| 3. Supplies carry over, and the travel bar | Done. Part 1 built (Step 196): supplies and crew are kept between missions. Part 2 built (Step 198): the travel bar and the actions in each room. |
| A test mission after MILK RUN | Built (Step 204), throwaway. Mission 2, TEST MISSION, is won 5 game seconds after time starts. A MILK RUN win now opens the ship's rooms and the travel bar. **Take it out before the player build:** in Starwolf → Campaign, switch TEST MISSION off (Step 205). Do not delete it. Its scene, `Assets/Scenes/Levels/TestMission.unity`, stays. It is still on today. |
| The Campaign window | Built (Step 205). **Starwolf → Campaign** edits `Assets/Campaign/Campaign.asset`: the missions in play order, an On switch for each, each mission's name, level, briefing and map dot, and the closing line. A mission switched off is skipped on the sector map, in the save slot's count and percent, and for the end card. See "Making a mission", below. |

**Step 2, as built (Step 194).** Win the campaign's last mission and press MISSION SELECT. The
screen goes black. The closing line types in, in the middle, in the sector map's green Share Tech
Mono. It stays up for 5 seconds. Then the title loads, and only STARWOLF flickers in. Any key or
click shows the whole line at once. Another one goes straight to the title. The scene is
`Assets/Scenes/Tactical/EndCard.unity`. Today the campaign has MILK RUN and a throwaway TEST
MISSION (Step 204). The closing line is empty. Since Step 203 an empty line goes straight to the
title. Since Step 204 a campaign win of MILK RUN opens the ship's rooms and the travel bar again. A
win of TEST MISSION goes to STARWOLF and the title. (Before Step 203, an empty line opened the
rooms.) A used save slot reads "CAMPAIGN · 1 OF 10 · 10%", or "CAMPAIGN COMPLETE · 100%" when every
mission in the Campaign file is won. Since Step 205 it counts only the missions switched on. Old test levels don't count. After a campaign win of MILK RUN
it reads CAMPAIGN · 1 OF 2 · 50%. After TEST MISSION it reads CAMPAIGN COMPLETE · 100%.

**Step 3 part 1, as built (Step 196).** Win a campaign mission and press MISSION SELECT. The next
mission starts with what our ship had left: the rounds in each gun and launcher, the marines aboard
and each room's crew. Marines still out on boats count as home; only marines killed are lost. Nothing goes above the full numbers in
`Assets/Ships/Asura.asset`. Fires, air loss and radiation are not kept. Armour and room damage are
kept, as before. The save is written only at MISSION SELECT. RESTART MISSION on the win screen
replays from the save before. Part 2 (Step 198) restocks them. Saved numbers match by name, so
renaming a gun's label or a room loses what was saved for it. The code is in `SaveGame.cs`.
HANGAR and QUARTERS now restock torpedoes, ammo and marines (part 2, Step 198).

**Step 3 part 2, as built (Step 198).** Win a campaign mission and press MISSION SELECT. The ship's
rooms show a travel bar at the top right: TRAVEL 0 / 10. A won mission gives a fresh bar of 10
units. ENGINEERING's DAMAGE REPORT lists each damaged part as a REPAIR button, with its cost: the
reactor 3, another room 2, one side's armour 1. HANGAR has a STORES box: RESTOCK TORPEDOES (cost 2)
and EXTRA AMMO (cost 1). Torpedoes go to the emptiest launcher first. Ammo rounds down. QUARTERS has
a MARINES box: HEAL MARINES (cost 2). BRIDGE has talk only. What each action gives is in the table
under "Settled for step 3". Each click adds its cost to the bar and saves at once. A button that
doesn't fit in the bar is greyed out. HANGAR's and QUARTERS' buttons always show, greyed out when
full. When the bar is full, every action locks. SECTOR MAP flashes amber, once a second. SECTOR MAP
works any time. Any units left are lost: the sector map shuts the bar whenever it opens, however the rooms were left. After a lost mission, the bar shows 10 / 10, locked. A new
campaign starts with the bar shut. The first bar comes with the first win. The free REPAIR ALL is
gone. The numbers are on the Board tab of Starwolf → Tuning, under Travel bar, with `Ship.unity`
open. The code is in `ShipRooms.cs`, `SaveGame.cs` and `SectorMap.cs`. Actions find their room by
its name: ENGINEERING, HANGAR, QUARTERS. A renamed room loses its actions. My calls, to report to
the owner: torpedoes to the emptiest launcher first, ammo rounding down, the stock always showing,
the boxes at the top right in the damage report's style, and no bar after a lost mission.

## Making a mission (owner, 2026-11-11)

The owner's flow, in four steps:
1. **Starwolf → Level**: draw the map.
2. **Starwolf → Mission**: build the mission inside the level. Planned by the Mission Setup Tool
   chat. Not built.
3. Save the level. The map and the mission are in one scene file.
4. **Starwolf → Campaign**: put it in the campaign (Step 205).

The split, agreed with the Mission Setup Tool chat (2026-11-11):
- **The Campaign window** sets up the mission list: its order, adding and removing, the On switch,
  each mission's name, level, briefing and map dot, and the closing line.
- **The Mission window** (planned) sets up what happens inside one mission: its checkpoints, the
  lines and who says them. These are saved in the level's Mission object. It picks the mission from
  the same list.
- There is one mission list, in `Assets/Campaign/Campaign.asset`.
- The ship call signs for the whole campaign and the title word are for the Mission Setup Tool chat
  to set up with the owner later, in `Campaign.asset`. Not built.

The travel bar's numbers stay in Starwolf → Tuning, on the Board tab under Travel bar, with
`Ship.unity` open. The Campaign window has a note pointing there.

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
   plays: "Same flicker, same everything. Don't change that." Then the title menu. The owner also
   told the Loading Screen chat (2026-11-10): "ensure that the end campaign screen also loads just
   the Starwolf logo flickering in." Since Step 188, every return to the title shows only STARWOLF
   flickering in, not SECTION 36. So the end card loads the title scene, `00_Start`, and gets that
   flicker unchanged.
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
- Replays are off for now (owner 2026-11-11). A won mission's LAUNCH on the sector map is greyed
  out. Owner: "You're not going to get a better result or new assets or new anything." If replays
  come back, they are just for fun and give no travel bar.
- STARWOLF must never be in the bottom half of the screen (owner 2026-11-11). On the title it is in
  the middle. An end card with a closing line must not put STARWOLF below the line. The design
  page drew it below the line; that is ruled out.

My own calls, told to the owner: SECTOR MAP stays usable any time, and leaving early loses the unused
bar. When the bar is full, the actions lock and the SECTOR MAP tab flashes. The free REPAIR ALL goes.
Restock drones stays hidden until drones are built. The save slot's percent is missions won out of
the missions in the Campaign file.

## Settled for step 3 (owner, 2026-11-10)

- **Fires, radiation and hull breaches do not carry over.** Owner: "I don't think you should come
  into a level leaking something." They end with the mission. The room damage they did stays. So
  there are no actions to clear them between missions. This replaces answer 2's "remove a
  condition, repair a breach".
- **Crew losses carry over.** Owner: "If you lose crew, you lose crew." The crew in each room come
  back as they ended. Only the marines can be healed.
- **What one action gives back** (my starting numbers; the owner didn't change them). All are
  Tuning numbers, logged when built:

| Action | Cost | Gives back |
|---|---|---|
| Repair the reactor | 3 | 2 of its 5 points |
| Repair another room | 2 | 2 of its 5 points |
| Repair one side's armour | 1 | A quarter of that side's full armour |
| Extra ammo | 1 | 20% of full: 2 railgun slugs, 480 rounds per PDC group |
| Restock torpedoes | 2 | 4 torpedoes (16 carried) |
| Heal the marines | 2 | 6 marines, one boat's worth (18 carried) |
| Restock drones | 1 | Hidden until drones are built |

Step 3 is built in two parts: first the supplies and crew carry over, then the travel bar and the
actions in the rooms.

## The rooms' art

Only the HANGAR has its painting and person (`Assets/UI/Rooms`). BRIDGE, ENGINEERING and QUARTERS,
and the XO, the Chief Engineer and the Marine Sergeant, are grey stand-ins. The art goes in the Art
List on the rooms screen, one slot per painting and per person.
