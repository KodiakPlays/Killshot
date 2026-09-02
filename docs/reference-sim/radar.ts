import { TUNING } from './tuning';
import { bearing, bearingTo, dist, unit, type Bearing, type Vec2 } from './geometry';

export type Allegiance = 'unknown' | 'hostile' | 'friendly' | 'neutral';

/** One sighting: a direction, taken from a known place, at a known time. */
export interface Observation {
  bearing: Bearing;
  from: Vec2;      // where we were when we saw it
  at: number;      // mission time
}

export interface Contact { pos: Vec2; id: number }

/**
 * A track is what you KNOW, not what is true.
 *
 * The sweep gives you a bearing, not a position. One bearing is a line you sit
 * on — it tells you which way, never how far. You only get a position once you
 * have two bearings taken from far enough apart, which means you had to move
 * between them. Information costs time and travel.
 */
export class Track {
  readonly id: number;
  readonly designator: string;
  readonly entity: Contact;

  observations: Observation[] = [];
  resolved = false;          // do we have a position, or only bearings?
  fixPos: Vec2 | null = null;
  lastFix = -Infinity;

  estHeading: Bearing = bearing(0);
  estSpeed = 0;
  confidence = 0;
  allegiance: Allegiance = 'unknown';
  scanned = false;
  shipClass: string | null = null;

  constructor(id: number, entity: Contact) {
    this.id = id;
    this.designator = `TRACK ${String(id).padStart(2, '0')}`;
    this.entity = entity;
  }

  get lastBearing(): Bearing | null {
    return this.observations.length ? this.observations[this.observations.length - 1]!.bearing : null;
  }

  age(now: number): number { return now - this.lastFix; }

  /** How far it could have moved since we last pinned it down. */
  uncertainty(now: number): number {
    if (!this.resolved) return TUNING.RADAR_RADIUS;   // unresolved: could be anywhere on the line
    return Math.max(0, this.estSpeed * this.age(now));
  }

  /** Where we think it is now, carried forward from the last fix. */
  predicted(now: number): Vec2 | null {
    if (!this.fixPos) return null;
    const v = unit(this.estHeading);
    const a = this.age(now);
    return { x: this.fixPos.x + v.x * this.estSpeed * a, y: this.fixPos.y + v.y * this.estSpeed * a };
  }

  /** The label IS the knowledge state. */
  label(rangeNow: number | null, awareness: number): string {
    if (this.scanned && this.shipClass) return this.shipClass;
    if (!this.resolved) return 'BEARING ONLY';
    if (rangeNow === null) return 'UNKNOWN';
    if (rangeNow < TUNING.SONAR_RADIUS) return 'LARGE WARSHIP RETURN';
    if (rangeNow < TUNING.RADAR_RADIUS) return awareness >= 1 ? 'LARGE MASS RETURN' : 'DISTANT MASS RETURN';
    return 'UNKNOWN';
  }

  observe(from: Vec2, truePos: Vec2, now: number): void {
    this.observations.push({ bearing: bearingTo(from, truePos), from: { ...from }, at: now });
    if (this.observations.length > 8) this.observations.shift();
    this.tryResolve(truePos, now);
    this.confidence = Math.min(1, this.confidence + (this.resolved ? 0.34 : 0.12));
  }

  /**
   * Two bearings from far enough apart cross at a point. That crossing is the fix.
   * Until you have moved, you have a direction and nothing else.
   */
  private tryResolve(truePos: Vec2, now: number): void {
    const n = this.observations.length;
    if (n < 2) return;
    const last = this.observations[n - 1]!;
    let baselineFound = false;
    for (let i = n - 2; i >= 0; i--) {
      if (dist(this.observations[i]!.from, last.from) >= TUNING.FIX_BASELINE) { baselineFound = true; break; }
    }
    if (!baselineFound) return;

    const previous = this.fixPos;
    const previousTime = this.lastFix;
    this.fixPos = { ...truePos };
    this.lastFix = now;
    this.resolved = true;

    if (previous && previousTime > -Infinity) {
      const dt = now - previousTime;
      const moved = dist(previous, truePos);
      if (dt > 0.01 && moved > 1) {
        this.estHeading = bearingTo(previous, truePos);
        this.estSpeed = moved / dt;
      }
    }
  }
}

export interface BlipFlash { bearing: Bearing; life: number }

/**
 * The sweep runs continuously. A contact is only seen when the beam crosses it,
 * so what is on screen between crossings is stale — that is the point.
 *
 * Having it on is the loudest thing you do. See signature() in world.ts.
 */
export class Radar {
  bearing: Bearing = bearing(0);
  active = true;
  readonly tracks = new Map<number, Track>();
  flashes: BlipFlash[] = [];
  private nextId = 1;

  step(dt: number, ownPos: Vec2, contacts: readonly Contact[], now: number): void {
    for (const [id, t] of this.tracks) {
      t.confidence = Math.max(0, t.confidence - TUNING.CONFIDENCE_DECAY * dt);
      if (t.age(now) > TUNING.DROP_TRACK_AFTER && t.observations.length === 0) this.tracks.delete(id);
    }
    for (const f of this.flashes) f.life -= dt;
    this.flashes = this.flashes.filter(f => f.life > 0);

    if (!this.active) return;   // dark: no sweep, no new information, no noise

    const previous = this.bearing as number;
    const next = previous + (360 / TUNING.RADAR_INTERVAL) * dt;
    this.bearing = bearing(next);

    for (const c of contacts) {
      const range = dist(ownPos, c.pos);
      if (range > TUNING.RADAR_RADIUS) continue;
      const b = bearingTo(ownPos, c.pos);
      if (!crossed(previous, next, b as number)) continue;

      let track = this.findByEntity(c);
      if (!track) { track = new Track(this.nextId++, c); this.tracks.set(track.id, track); }
      track.observe(ownPos, c.pos, now);
      this.flashes.push({ bearing: b, life: TUNING.BLIP_FLASH_TIME });
    }
  }

  findByEntity(c: Contact): Track | undefined {
    for (const t of this.tracks.values()) if (t.entity === c) return t;
    return undefined;
  }

  get list(): Track[] { return [...this.tracks.values()]; }
}

/** Did the beam pass over this bearing during the step? Handles the wrap past 0. */
function crossed(from: number, to: number, target: number): boolean {
  let t = target;
  while (t < from) t += 360;
  return t <= to;
}
