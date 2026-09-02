// Every tunable number in the game lives here and nowhere else.
// Nothing below may be repeated as a literal anywhere in the codebase.

export const TUNING = {
  // ── movement ─────────────────────────────────────────────────────────────
  MAX_SPEED: 240,
  SPEED_STEP: 15,             // throttle is stepped, not held
  ACCEL_RATE: 80,
  BRAKE_RATE: 150,
  REVERSE_SPEED_FACTOR: 0.42,
  TURN_RATE: 58,              // degrees per second at full engine power
  SPACE_DRAG: 0.9992,         // per 1/60s, on the whole velocity
  DRIFT_DECAY: 0.988,         // per 1/60s, on the sideways component only
  ENGINE_DRIFT_BONUS: 0.006,  // engine power helps you kill sideways drift

  // ── hull ─────────────────────────────────────────────────────────────────
  // Four sides. The stern is thinner on purpose — the reactor and engines sit
  // behind it, so turning to run exposes your weakest plate over your most
  // critical systems. Tune freely.
  HULL_PROW: 50,
  HULL_PORT: 50,
  HULL_STARBOARD: 50,
  HULL_AFT: 20,

  // ── stability ────────────────────────────────────────────────────────────
  STAB_MAX: 100,
  STAB_REGEN: 10,             // per second, when not turning
  STAB_TURN_DRAIN: 22,        // scaled by how hard and how fast you turn
  DODGE_COST: 45,             // two dodges leaves you at 10
  DODGE_DISTANCE: 260,
  DODGE_TIME: 0.35,

  // ── power ────────────────────────────────────────────────────────────────
  REACTOR_MAX: 10,
  SYSTEM_MAX: 5,              // ten across five means you can fully run two
  DRAW_SECONDS_PER_POINT: 2,  // multiplied by the number of systems drawing
  VENT_POINTS_PER_SECOND: 1,
  ENGINE_CRAWL: 0.05,         // a powerless ship is stranded, not frozen

  // ── sensors ──────────────────────────────────────────────────────────────
  RADAR_RADIUS: 40000,
  SONAR_RADIUS: 8000,
  RADAR_INTERVAL: 3.0,        // seconds for one full sweep
  FIX_BASELINE: 900,          // how far you must move between two bearings to solve a position
  CONFIDENCE_DECAY: 0.06,     // per second, without a new fix
  DROP_TRACK_AFTER: 45,       // seconds unseen before we forget it
  BLIP_FLASH_TIME: 0.6,

  // ── weapons ──────────────────────────────────────────────────────────────
  SLUG_SPEED: 4800,           // eight seconds to cross maximum range
  RAILGUN_RANGE: 38000,
  ARM_SECONDS: 4,             // per turret, one at a time
  MIN_ARM_POWER: 2,           // WEAPONS points needed before a turret can arm
  SOLUTION_LIFETIME: 18,      // seconds, then the solution is gone

  // ── being seen ───────────────────────────────────────────────────────────
  // Active radar is the loudest thing you do. Looking costs you.
  SIG_RADAR_ACTIVE: 3.5,
  SIG_SPEED_WEIGHT: 2,        // how much running fast adds
  SIG_FIRING_SPIKE: 2,        // what a shot adds, briefly
  SIG_SPIKE_DECAY: 4,         // seconds for a firing spike to fade

  // ── the arena ────────────────────────────────────────────────────────────
  ARENA_RADIUS: 50000,
  BOUNDARY_WARNING: 5000,
  DESERTION_SECONDS: 30,
} as const;

/** Detection gates. Once awareness rises it never comes back down. */
export const AWARENESS_GATES = [
  { distance: 18000, signature: 2.0, level: 1, name: 'UNEASY' },
  { distance: 12500, signature: 3.0, level: 2, name: 'SEARCHING' },
  { distance: 8500, signature: 4.5, level: 3, name: 'ALERT' },
] as const;

export const AWARENESS_NAMES = ['CLEAR', 'UNEASY', 'SEARCHING', 'ALERT', 'GENERAL ALARM'] as const;

/** Colour is a code. It says whose side something is on, never decoration. */
export const COLOUR = {
  hostile: '#E04B3C',
  friendly: '#4A9FD8',
  ownSystems: '#8FC33A',
  orders: '#E8A33D',
  ordersBright: '#F0A020',
  unclassified: '#9B7FD4',
  sensor: '#3FA9C9',
  ownShip: '#E8EDF0',
  plotGlass: '#0A1719',
  void: '#0A0C0E',
  gridLine: 'rgba(63,169,201,.065)',
  label: '#6E7B85',
} as const;
