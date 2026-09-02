import { TUNING } from './tuning';
import { angDiff, bearing, unit, type Bearing, type Relative, type Vec2 } from './geometry';

export type TurretState = 'empty' | 'arming' | 'armed';

/**
 * Three turrets, one shot each. A turret holds its OWN aim bearing.
 *
 * There is no volley button. Put the same bearing on three turrets and the shots
 * converge; put different bearings on them and they fan. The spread is the
 * difference between the numbers you typed.
 */
export class Turret {
  readonly name: string;
  /** Arc centre, measured from our own nose. 0 is dead ahead. */
  readonly arcCentre: number;
  readonly halfArc: number;

  state: TurretState = 'empty';
  armProgress = 0;
  aimBearing: Bearing | null = null;
  solutionExpiry = 0;

  constructor(name: string, arcCentre: number, halfArc: number) {
    this.name = name;
    this.arcCentre = arcCentre;
    this.halfArc = halfArc;
  }

  /** Can this turret reach something sitting at this angle off our nose? */
  bears(rel: Relative): boolean {
    return Math.abs(angDiff(rel as number, this.arcCentre)) <= this.halfArc;
  }

  /** Arming needs a floor of weapons power. It is a threshold, not a drain. */
  arm(weaponsPoints: number): boolean {
    if (this.state !== 'empty') return false;
    if (weaponsPoints < TUNING.MIN_ARM_POWER) return false;
    this.state = 'arming';
    this.armProgress = 0;
    return true;
  }

  /** Giving up a commitment is instant and free. */
  disarm(): void {
    this.state = 'empty';
    this.armProgress = 0;
    this.aimBearing = null;
    this.solutionExpiry = 0;
  }

  step(dt: number, now: number, weaponsPoints: number): void {
    if (this.state === 'arming') {
      if (weaponsPoints < TUNING.MIN_ARM_POWER) return;   // stalls, does not reset
      this.armProgress += dt / TUNING.ARM_SECONDS;
      if (this.armProgress >= 1) {
        this.armProgress = 1;
        this.state = 'armed';
        this.solutionExpiry = now + TUNING.SOLUTION_LIFETIME;
      }
    }
    // The solution goes stale on its own. The gun stays loaded; the answer does not.
    if (this.state === 'armed' && now > this.solutionExpiry) this.aimBearing = null;
  }

  solutionLeft(now: number): number {
    if (this.state !== 'armed' || !this.aimBearing) return 0;
    return Math.max(0, this.solutionExpiry - now);
  }

  get ready(): boolean { return this.state === 'armed' && this.aimBearing !== null; }

  fire(from: Vec2): Slug | null {
    if (!this.ready || !this.aimBearing) return null;
    const slug = new Slug(from, this.aimBearing);
    this.disarm();
    return slug;
  }
}

export function makeTurrets(): Turret[] {
  return [
    new Turret('FWD LEFT', 0, 90),
    new Turret('FWD RIGHT', 0, 90),
    new Turret('AFT', 180, 90),
  ];
}

/**
 * A slug takes real time to arrive. At maximum range that is nearly eight
 * seconds, and the target moves in that time — so you are always shooting at a
 * guess, never at where something is.
 */
export class Slug {
  pos: Vec2;
  readonly bearing: Bearing;
  travelled = 0;
  alive = true;
  readonly trail: Vec2[] = [];

  constructor(from: Vec2, b: Bearing) {
    this.pos = { ...from };
    this.bearing = b;
  }

  step(dt: number): void {
    const v = unit(this.bearing);
    const d = TUNING.SLUG_SPEED * dt;
    this.trail.push({ ...this.pos });
    if (this.trail.length > 24) this.trail.shift();
    this.pos.x += v.x * d;
    this.pos.y += v.y * d;
    this.travelled += d;
    if (this.travelled >= TUNING.RAILGUN_RANGE) this.alive = false;
  }

  /** How long until it reaches a given range, for the time-of-flight readout. */
  static flightTime(range: number): number { return range / TUNING.SLUG_SPEED; }
}

export const bearingOf = bearing;
