# Reference simulation

The simulation core of the shelved TypeScript prototype. The rest of that
project — its React UI — was broken and has been deleted. This part was not.

It is kept because it already expresses the mechanics that were decided on,
and re-deriving them would risk getting them wrong again:

- `weapons.ts` — turrets with `arcCentre`, `halfArc`, and a state of
  `empty | arming | armed`. Each turret carries its own `aimBearing` and
  `solutionExpiry`. `fire()` disarms the turret, so one arming is one shot.
  A volley is what happens when several turrets are ready at once — it is a
  consequence of the firing solutions, not a button.
- `radar.ts` — the track model. `resolved`, `lastFix`, `estHeading`,
  `estSpeed`, `confidence`, `uncertainty()`, `predicted()`, and a two-bearing
  triangulation to resolve a contact.
- `ship.ts` — commanded versus actual state. Turning points the hull; it does
  not turn the velocity. Drift bleeds off separately.
- `power.ts`, `tuning.ts`, `geometry.ts`, `world.ts` — supporting model.
- `_sim_draft.js` — an earlier draft of the same core, all in one JavaScript file. Nothing uses it.

**This is reference, not build output.** Nothing compiles it and nothing
imports it. Read it when porting a mechanic to C#.
