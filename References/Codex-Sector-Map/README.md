# Starwolf Sector Operations Map

Prepared by Codex for Claude to integrate into the Killshot Unity project.

This screen separates campaign missions. Keep the verified console framing and star chart. Opening.png and Replay.png show the intended opening and completed-mission states.

From the Killshot repository root, review the patch and check it against the current working tree:

```sh
git apply --stat References/Codex-Sector-Map/sector-map.patch
git apply --check References/Codex-Sector-Map/sector-map.patch
```

When ready, apply the patch with `git apply References/Codex-Sector-Map/sector-map.patch`. If the check fails, reconcile the five affected files with concurrent work. Preserve the scene's existing Unity GUIDs and campaign flow.

In Unity, enter the campaign through a test save slot. Verify the first mission, selection of a completed node, Briefing, Debrief, and Replay Mission. Briefing and Debrief display text records. The patch contains the scene, controller changes, console artwork and texture import settings. Codex verified compilation and captured the two scene states in an isolated project.
