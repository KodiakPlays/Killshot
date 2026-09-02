// Starwolf — simulation core. No DOM in this file.
// Every number here is meant to be tuned. They live in TUNING, nowhere else.

export const TUNING = {
  // movement
  MAX_SPEED: 240, SPEED_STEP: 15, ACCEL_RATE: 80, BRAKE_RATE: 150,
  REVERSE_ACCEL: 55, REVERSE_SPEED_FACTOR: 0.42,
  TURN_RATE: 58,                 // degrees per second at full engine power
  SPACE_DRAG: 0.9992,            // per 1/60s, applied to the whole velocity
  DRIFT_DECAY: 0.988,            // per 1/60s, applied to the sideways component
  LATERAL_ACCEL: 42, LATERAL_MAX: 90,

  // stability
  STAB_MAX: 100, STAB_REGEN: 10, STAB_TURN_DRAIN: 22,
  DODGE_COST: 45, DODGE_DISTANCE: 260, DODGE_TIME: 0.35,

  // power
  REACTOR_MAX: 10, SYSTEM_MAX: 5,
  DRAW_SECONDS_PER_POINT: 2,     // × number of systems drawing
  VENT_POINTS_PER_SECOND: 1,

  // sensors
  RADAR_RADIUS: 40000, SONAR_RADIUS: 8000,
  RADAR_INTERVAL: 3.0,           // seconds per full sweep
  CONFIDENCE_DECAY: 0.06,
  DROP_TRACK_AFTER: 45,

  // weapons
  SLUG_SPEED: 4800, RAILGUN_RANGE: 38000,
  ARM_SECONDS: 4, MIN_ARM_POWER: 2, SOLUTION_LIFETIME: 18,

  // being seen
  AWARENESS: [
    { dist: 18000, sig: 2.0, level: 1, name: 'UNEASY' },
    { dist: 12500, sig: 3.0, level: 2, name: 'SEARCHING' },
    { dist: 8500,  sig: 4.5, level: 3, name: 'ALERT' },
  ],

  ARENA_RADIUS: 50000,
};

// ── bearings ───────────────────────────────────────────────────────────────
// 0 is +Y, increasing clockwise. Screen angle = bearing - 90.
export const norm = d => ((d % 360) + 360) % 360;
export const angDiff = (a, b) => ((a - b + 540) % 360) - 180;
export const bearingTo = (from, to) => norm(Math.atan2(to.x - from.x, to.y - from.y) * 180 / Math.PI);
export const vecFrom = b => ({ x: Math.sin(b * Math.PI / 180), y: Math.cos(b * Math.PI / 180) });
export const dist = (a, b) => Math.hypot(b.x - a.x, b.y - a.y);

// ── clock ──────────────────────────────────────────────────────────────────
export class Clock {
  constructor() { this.t = 0; this.paused = false; this.rate = 1; }
  step(realDt) {
    if (this.paused) return 0;
    const dt = Math.min(realDt, 0.05) * this.rate;   // clamped, prototype behaviour
    this.t += dt;
    return dt;
  }
}

// ── power ──────────────────────────────────────────────────────────────────
// Ten points. Five systems, five each. You can fully run two.
// Points are allocated, not spent — they sit either in the reactor or in a system.
export const SYSTEMS = ['ENGINES', 'WEAPONS', 'SCANNER', 'COMMS', 'AUXILIARY'];

export class Power {
  constructor() {
    this.points = {}; this.mode = {};
    for (const s of SYSTEMS) { this.points[s] = 0; this.mode[s] = 'standby'; }
    this.points.ENGINES = 2;
    this.fill = {};                       // partial progress toward the next point
    for (const s of SYSTEMS) this.fill[s] = 0;
  }
  get allocated() { return SYSTEMS.reduce((n, s) => n + this.points[s], 0); }
  get reactorFree() { return TUNING.REACTOR_MAX - this.allocated; }
  efficiency(s) { return this.points[s] / TUNING.SYSTEM_MAX; }

  // Standby -> Draw -> Standby -> Vent -> Standby. You pass back through standby.
  cycle(s) {
    const m = this.mode[s];
    if (m === 'standby') this.mode[s] = this._wasDrawing?.[s] ? 'vent' : 'draw';
    else if (m === 'draw') { this.mode[s] = 'standby'; (this._wasDrawing ||= {})[s] = true; }
    else if (m === 'vent') this.mode[s] = 'standby';
    if (this.mode[s] === 'vent') (this._wasDrawing ||= {})[s] = false;
  }
  setMode(s, m) { this.mode[s] = m; }

  step(dt) {
    const drawing = SYSTEMS.filter(s => this.mode[s] === 'draw');
    // Total intake is constant — drawing on three splits the same throughput three ways.
    const perSystem = drawing.length ? 1 / (TUNING.DRAW_SECONDS_PER_POINT * drawing.length) : 0;

    for (const s of SYSTEMS) {
      if (this.mode[s] === 'draw') {
        if (this.points[s] >= TUNING.SYSTEM_MAX || this.reactorFree <= 0) { this.mode[s] = 'standby'; continue; }
        this.fill[s] += perSystem * dt;
        while (this.fill[s] >= 1 && this.points[s] < TUNING.SYSTEM_MAX && this.reactorFree > 0) {
          this.fill[s] -= 1; this.points[s] += 1;
        }
      } else if (this.mode[s] === 'vent') {
        // Once started, a vent runs to zero. It cannot be stopped.
        this.fill[s] -= TUNING.VENT_POINTS_PER_SECOND * dt;
        while (this.fill[s] < 0 && this.points[s] > 0) { this.fill[s] += 1; this.points[s] -= 1; }
        if (this.points[s] === 0) { this.fill[s] = 0; this.mode[s] = 'standby'; }
      } else this.fill[s] = 0;
    }
  }

  ventAll() { for (const s of SYSTEMS) if (this.points[s] > 0) this.mode[s] = 'vent'; }
}

// ── the ship ───────────────────────────────────────────────────────────────
export class Ship {
  constructor(x = 0, y = 0, heading = 0) {
    this.pos = { x, y };
    this.vel = { x: 0, y: 0 };
    this.heading = heading;            // where the nose points
    this.commandedHeading = heading;   // where you have told it to point
    this.targetSpeed = 0;              // stepped, ±SPEED_STEP per press
    this.stability = TUNING.STAB_MAX;
    this.hull = { prow: 50, port: 50, starboard: 50, aft: 40 };
    this.turning = 0;                  // -1, 0, +1 this frame
    this.dodge = null;
  }

  get speed() { return Math.hypot(this.vel.x, this.vel.y); }
  get forwardSpeed() {
    const f = vecFrom(this.heading);
    return this.vel.x * f.x + this.vel.y * f.y;
  }

  throttle(delta) {
    const max = TUNING.MAX_SPEED;
    const min = -max * TUNING.REVERSE_SPEED_FACTOR;
    this.targetSpeed = Math.max(min, Math.min(max, this.targetSpeed + delta * TUNING.SPEED_STEP));
  }

  nudgeHeading(dir, dt, enginePower) {
    const rate = TUNING.TURN_RATE * Math.max(0.15, enginePower);
    this.commandedHeading = norm(this.commandedHeading + dir * rate * dt);
    this.turning = dir;
  }

  step(dt, power) {
    const eng = power.efficiency('ENGINES');

    // Heading: actual converges on commanded at the turn rate.
    const err = angDiff(this.commandedHeading, this.heading);
    const maxTurn = TUNING.TURN_RATE * Math.max(0.15, eng) * dt;
    const applied = Math.max(-maxTurn, Math.min(maxTurn, err));
    this.heading = norm(this.heading + applied);

    // Turning costs stability, and it costs more the faster you are going.
    const turnAmount = Math.abs(applied) / Math.max(0.0001, maxTurn);
    const speedBand = Math.abs(this.forwardSpeed) / TUNING.MAX_SPEED;
    if (turnAmount > 0.01) {
      this.stability -= TUNING.STAB_TURN_DRAIN * turnAmount * (0.7 + 4.3 * speedBand) * dt;
    } else {
      this.stability += TUNING.STAB_REGEN * dt;
    }
    this.stability = Math.max(0, Math.min(TUNING.STAB_MAX, this.stability));

    // Thrust along the nose only.
    const cap = TUNING.MAX_SPEED * Math.max(0.05, eng);
    const wanted = Math.max(-cap * TUNING.REVERSE_SPEED_FACTOR, Math.min(cap, this.targetSpeed));
    const fwd = vecFrom(this.heading);
    const along = this.forwardSpeed;
    const rate = wanted > along ? TUNING.ACCEL_RATE : TUNING.BRAKE_RATE;
    const newAlong = along + Math.sign(wanted - along) * Math.min(rate * dt, Math.abs(wanted - along));

    // The ship slides. Split velocity into nose-aligned and sideways, then bleed
    // the sideways part — turning points you, it does not change where you are going.
    const side = { x: -fwd.y, y: fwd.x };
    let lateral = this.vel.x * side.x + this.vel.y * side.y;
    const frames = dt * 60;
    lateral *= Math.pow(TUNING.DRIFT_DECAY + 0.006 * eng, frames);

    this.vel.x = fwd.x * newAlong + side.x * lateral;
    this.vel.y = fwd.y * newAlong + side.y * lateral;
    const drag = Math.pow(TUNING.SPACE_DRAG, frames);
    this.vel.x *= drag; this.vel.y *= drag;

    if (this.dodge) {
      this.dodge.t += dt;
      const k = Math.min(1, this.dodge.t / TUNING.DODGE_TIME);
      const step = TUNING.DODGE_DISTANCE * (dt / TUNING.DODGE_TIME);
      this.pos.x += side.x * this.dodge.dir * step;
      this.pos.y += side.y * this.dodge.dir * step;
      if (k >= 1) this.dodge = null;
    }

    this.pos.x += this.vel.x * dt;
    this.pos.y += this.vel.y * dt;
    this.turning = 0;
  }

  // Not a turn. The whole ship shifts sideways, and it costs 45% stability.
  tryDodge(dir) {
    if (this.dodge || this.stability < TUNING.DODGE_COST) return false;
    this.stability -= TUNING.DODGE_COST;
    this.dodge = { dir, t: 0 };
    return true;
  }
}

// ── tracks ─────────────────────────────────────────────────────────────────
// What you know, not what is true. Fixed only when the sweep crosses it.
export class Track {
  constructor(id, entity) {
    this.id = id; this.entity = entity;
    this.designator = 'TRACK ' + String(id).padStart(2, '0');
    this.fixPos = { ...entity.pos };
    this.lastFix = -999; this.estHeading = 0; this.estSpeed = 0;
    this.confidence = 0; this.scanned = false; this.allegiance = 'unknown';
  }
  age(now) { return now - this.lastFix; }
  uncertainty(now) { return Math.max(0, this.estSpeed * this.age(now)); }
  predicted(now) {
    const v = vecFrom(this.estHeading), a = this.age(now);
    return { x: this.fixPos.x + v.x * this.estSpeed * a, y: this.fixPos.y + v.y * this.estSpeed * a };
  }
  recordFix(truePos, now) {
    const dt = now - this.lastFix;
    if (this.lastFix > 0 && dt > 0.01) {
      const moved = dist(this.fixPos, truePos);
      if (moved > 0.5) { this.estHeading = bearingTo(this.fixPos, truePos); this.estSpeed = moved / dt; }
    }
    this.fixPos = { ...truePos };
    this.lastFix = now;
    this.confidence = Math.min(1, this.confidence + 0.34);
  }
  label(rangeNow, awareness) {
    if (this.scanned) return this.shipClass || 'IDENTIFIED';
    if (rangeNow < TUNING.SONAR_RADIUS) return 'LARGE WARSHIP RETURN';
    if (rangeNow < TUNING.RADAR_RADIUS) return awareness >= 1 ? 'LARGE MASS RETURN' : 'DISTANT MASS RETURN';
    return 'UNKNOWN';
  }
}

// ── radar ──────────────────────────────────────────────────────────────────
// The sweep runs. A contact is fixed only when the beam crosses it, so what you
// see between crossings is stale. That is the point.
export class Radar {
  constructor() { this.bearing = 0; this.prev = 0; this.active = true; this.tracks = new Map(); this.nextId = 1; this.flashes = []; }

  step(dt, own, entities, now) {
    for (const [id, t] of this.tracks) {
      t.confidence = Math.max(0, t.confidence - TUNING.CONFIDENCE_DECAY * dt);
      if (t.age(now) > TUNING.DROP_TRACK_AFTER) this.tracks.delete(id);
    }
    this.flashes = this.flashes.filter(f => (f.t -= dt) > 0);
    if (!this.active) return;

    this.prev = this.bearing;
    this.bearing = norm(this.bearing + (360 / TUNING.RADAR_INTERVAL) * dt);

    for (const e of entities) {
      const r = dist(own.pos, e.pos);
      if (r > TUNING.RADAR_RADIUS) continue;
      const b = bearingTo(own.pos, e.pos);
      if (!this.crossed(this.prev, this.bearing, b)) continue;

      let track = [...this.tracks.values()].find(t => t.entity === e);
      if (!track) { track = new Track(this.nextId++, e); this.tracks.set(track.id, track); }
      track.recordFix(e.pos, now);
      this.flashes.push({ bearing: b, t: 0.6 });
    }
  }

  crossed(from, to, target) {
    if (to < from) to += 360;
    if (target < from) target += 360;
    return target >= from && target <= to;
  }
}

// ── turrets ────────────────────────────────────────────────────────────────
export class Turret {
  constructor(name, arcCentre, halfArc) {
    this.name = name; this.arcCentre = arcCentre; this.halfArc = halfArc;
    this.state = 'empty';        // empty | arming | armed
    this.armProgress = 0;
    this.bearing = null;         // this turret's own aim bearing
    this.solutionExpiry = 0;
  }
  bears(relativeBearing) { return Math.abs(angDiff(relativeBearing, this.arcCentre)) <= this.halfArc; }
  step(dt, now, weaponsPoints) {
    if (this.state === 'arming') {
      if (weaponsPoints < TUNING.MIN_ARM_POWER) return;
      this.armProgress += dt / TUNING.ARM_SECONDS;
      if (this.armProgress >= 1) { this.state = 'armed'; this.armProgress = 1; this.solutionExpiry = now + TUNING.SOLUTION_LIFETIME; }
    }
    if (this.state === 'armed' && now > this.solutionExpiry) this.bearing = null;
  }
  arm(weaponsPoints) { if (this.state === 'empty' && weaponsPoints >= TUNING.MIN_ARM_POWER) { this.state = 'arming'; this.armProgress = 0; } }
  disarm() { this.state = 'empty'; this.armProgress = 0; this.bearing = null; }
  get solutionLeft() { return Math.max(0, this.solutionExpiry - (this._now || 0)); }
}

export function makeTurrets() {
  return [
    new Turret('FWD LEFT', 0, 90),
    new Turret('FWD RIGHT', 0, 90),
    new Turret('AFT', 180, 90),
  ];
}

// ── slugs in flight ────────────────────────────────────────────────────────
export class Slug {
  constructor(pos, bearing) { this.pos = { ...pos }; this.bearing = bearing; this.travelled = 0; this.alive = true; }
  step(dt) {
    const v = vecFrom(this.bearing);
    const d = TUNING.SLUG_SPEED * dt;
    this.pos.x += v.x * d; this.pos.y += v.y * d; this.travelled += d;
    if (this.travelled > TUNING.RAILGUN_RANGE) this.alive = false;
  }
}

// ── being seen ─────────────────────────────────────────────────────────────
export function signature(power, ship, firing) {
  // Detection = energy in systems + active use + environment.
  let sig = power.allocated;
  sig += Math.abs(ship.forwardSpeed) / TUNING.MAX_SPEED * power.efficiency('ENGINES') * 2;
  if (firing) sig += 2;
  return sig;
}

export function awarenessFor(current, range, sig) {
  let level = current;
  for (const gate of TUNING.AWARENESS) {
    if (range < gate.dist && sig > gate.sig) level = Math.max(level, gate.level);
  }
  return level;   // never goes down
}
