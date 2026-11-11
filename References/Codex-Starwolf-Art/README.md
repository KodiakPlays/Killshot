# Starwolf art handoff for Claude

Start with 01-Fire-Control. The owner approved the Game Level 5 left-hand panel treatment and wants the existing playable systems to retain their inputs and feedback. Each folder below has a different integration status.

## 01 — Fire Control and targeting

The two Targeting-Fire-Control PNGs are review images with sample telemetry. Import InspectorHousing.png as the empty bezel, or use ContactMonitor.png and ControlBank.png separately. Their glass openings are transparent. Garuda-profile.png is the static pale wireframe study from the project cruiser mesh. Show a known target only when the game has identified it. The existing ShipView top-down renderer and VectorModel shader graph are available for a live profile.

Build a sibling variant of Assets/UI/Board/Panels/Fire Control.prefab. Preserve its FireControlPanel, ClickSpot inputs, pointer handlers, and binding to BoardManager. Reuse the existing system/mount selectors, sight toggle, TRACK key, range/bearing/speed knobs, capacitor charge and shot-cost mark, four arming rockers, spring-return lever and ready text. Bind the labels and indicators to the existing live state. Keep the per-mode rules for the fixed railgun, PDC and torpedoes. The study suggests a new visual hierarchy, without adding weapon rules or keyboard mappings.

Use existing board sprite import settings and the board’s gamma presenter. The operational console in the menu scene uses a different rendering path. Exercise pointer hit testing, held/pressed visuals, blocked actions, firing and spring return in Play Mode before adopting the variant. The handoff includes artwork; Claude owns playable Fire Control integration.

## 02 — Tactical board frame

Frame-for-Claude.zip contains the optional Frame - Fire Control prefab variant, housing PNG, metadata and notes. The PNG uses the same Game Level 5 housing as the targeting study. Open the variant alongside the original board frame. Keep its transparent aperture aligned to the existing 1712 × 890 plot and keep all board rails accessible.

## 03 — Hangar

HangarRoom.png is the revised standalone room illustration. Assign it as a background and place separate interaction targets over the commander and room controls as needed. Clicking the painting itself has no dialogue behavior. The telemetry silhouette uses the approved heavy-fighter geometry as art for assault boats. Asura carries four assault boats in the inspected playable configuration. Use the actual squad list and fuel/state fields; no per-boat hull percentage exists.

## Sector Operations — already integrated in the working copy

The enabled SectorMap scene is in ProjectSettings/EditorBuildSettings.asset. SaveSlots routes to SectorMap. SectorMap.unity references the checked-node sprite and Briefing/Debrief events; SectorMap.cs labels completed missions REPLAY MISSION. The live OperationalMapConsole.png is byte-identical to the corrected handoff asset. The repository has further edits to this scene and script. Do not reapply docs/Codex-Sector-Map/sector-map.patch. This check covered the working copy, not a packaged player build.

### Hangar entry points in the current project

In Assets/Scenes/Tactical/Ship.unity, the editable Art List has slots named HANGAR painting and FLIGHT LEADER picture. Open the HANGAR room and assign the new painting and FlightLeader.png there. The portrait is an exact rectangular crop from the room image (left 30, top 344, width 305, height 392 on the 1672 × 941 source). Align the existing person button to that region, accounting for image aspect and letterboxing. ShipRooms takes the dialogue portrait and its tint from the button image. Test opening the existing conversation and leaving it.

Starwolf-Hangar.unitypackage adds an editable Room Image prefab, roster prefab, optional Hangar - Assault Boats variant, boat sprite and a read-only telemetry component. It requires the current Killshot board and its original Hangar prefab. For the existing board scene, adding Boat Telemetry.prefab as the last child of the original Hangar instance preserves its scene references. Use that or the variant, once. Verify the four boat IDs, pilot names, fuel, lost-state display, the six preparation rockers, editable bearing, launch drag and recall press/release. Test the normal mission gates with a test save.

Control-states.png shows the isolated Play Mode test at rest, all stages complete, lever pulled, and Recall held. With the roster attached, EventSystem raycasts reached the door and order knobs, six rockers, lever and Recall. The launch bearing accepted text input. For the successful launch, the test supplied flight clearance, AUX power, a scanned target and accelerated stage timing. A full pull launched one assault flight, release returned the lever to zero, and Recall set the flight to returning. A simulated LOST craft state appeared in its telemetry label. The room conversation still needs wiring and testing after art assignment.
