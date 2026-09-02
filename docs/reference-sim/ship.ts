import { TUNING } from './tuning';
import { Power } from './power';
import { angDiff, bearing, clamp, unit, type Bearing, type Vec2 } from './geometry';

export type HullSide = 'prow' | 'port' | 'starboard' | 'aft';

export interface Hull {
  prow: number; port: number; starboard: number; aft: number;
}

/**
 * The ship holds two sets of state: commanded (what you have told it) and
 * actual (what it is doing). Actual converges on commanded at a physical rate.
 * That is the whole order model — pause just stops the clock.
 */
export class Ship {
  pos: Vec2;
  vel: Vec2 = { x: 0, y: 0 };

  heading: Bearing;             // where the nose actually points
  commandedHeading: Bearing;    // where you have told it to point
  targetSpeed = 0;              // stepped, ±SPEED_STEP per press

  stability: number = TUNING.STAB_MAX;
  hull: Hull = {
    prow: TUNING.HULL_PROW,
    port: TUNING.HULL_PORT,
    starboard: TUNING.HULL_STARBOARD,
    aft: TUNING.HULL_AFT,
  };

  private dodge: { dir: -1 | 1; t: number } | null = null;
  private lastTurnAmount = 0;

  constructor(x = 0, y = 0, heading = 0) {
    this.pos = { x, y };
    this.heading = bearing(heading);
    this.commandedHeading = bearing(heading);
  }

  get speed(): number { return Math.hypot(this.vel.x, this.vel.y); }

  /** Speed along the nose. Negative means going backwards. */
  get forwardSpeed(): number {
    const f = unit(this.heading);
    return this.vel.x * f.x + this.vel.y * f.y;
  }

  /** How much of our motion is sideways. This is the drift you can feel. */
  get lateralSpeed(): number {
    const f = unit(this.heading);
    return this.vel.x * -f.y + this.vel.y * f.x;
  }

  get isDodging(): boolean { return this.dodge !== null; }
  get turnLoad(): number { return this.lastTurnAmount; }

  /** W and S step the commanded speed. It is not a held input. */
  throttle(steps: number): void {
    const max = TUNING.MAX_SPEED;
    const min = -max * TUNING.REVERSE_SPEED_FACTOR;
    this.targetSpeed = clamp(this.targetSpeed + steps * TUNING.SPEED_STEP, min, max);
  }

  /** A and D move the commanded heading. They do not rotate the hull directly. */
  steer(dir: -1 | 1, dt: number, enginePower: number): void {
    const rate = TUNING.TURN_RATE * Math.max(TUNING.ENGINE_CRAWL, enginePower);
    this.commandedHeading = bearing((this.commandedHeading as number) + dir * rate * dt);
  }

  /** Not a turn. The whole ship shifts sideways, and it is expensive. */
  tryDodge(dir: -1 | 1): boolean {
    if (this.dodge || this.stability < TUNING.DODGE_COST) return false;
    this.stability -= TUNING.DODGE_COST;
    this.dodge = { dir, t: 0 };
    return true;
  }

  takeHit(side: HullSide, amount: number): void {
    this.hull[side] = Math.max(0, this.hull[side] - amount);
  }

  /** Which plate an impact from this direction lands on. */
  sideFacing(from: Bearing): HullSide {
    const rel = angDiff(from as number, this.heading as number);
    const a = Math.abs(rel);
    if (a <= 45) return 'prow';
    if (a >= 135) return 'aft';
    return rel > 0 ? 'starboard' : 'port';
  }

  step(dt: number, power: Power): void {
    const eng = Math.max(TUNING.ENGINE_CRAWL, power.efficiency('ENGINES'));

    // ── heading: actual chases commanded ───────────────────────────────────
    const error = angDiff(this.commandedHeading as number, this.heading as number);
    const maxTurn = TUNING.TURN_RATE * eng * dt;
    const applied = clamp(error, -maxTurn, maxTurn);
    this.heading = bearing((this.heading as number) + applied);

    // ── stability: turning costs, and costs more at speed ──────────────────
    const turnLoad = maxTurn > 0 ? Math.abs(applied) / maxTurn : 0;
    this.lastTurnAmount = turnLoad;
    const speedBand = Math.abs(this.forwardSpeed) / TUNING.MAX_SPEED;
    if (turnLoad > 0.01) {
      this.stability -= TUNING.STAB_TURN_DRAIN * turnLoad * (0.7 + 4.3 * speedBand) * dt;
    } else {
      this.stability += TUNING.STAB_REGEN * dt;
    }
    this.stability = clamp(this.stability, 0, TUNING.STAB_MAX);

    // ── thrust along the nose ──────────────────────────────────────────────
    const cap = TUNING.MAX_SPEED * eng;
    const wanted = clamp(this.targetSpeed, -cap * TUNING.REVERSE_SPEED_FACTOR, cap);
    const fwd = unit(this.heading);
    const side = { x: -fwd.y, y: fwd.x };

    const along = this.forwardSpeed;
    const rate = Math.abs(wanted) > Math.abs(along) ? TUNING.ACCEL_RATE : TUNING.BRAKE_RATE;
    const delta = wanted - along;
    const newAlong = along + Math.sign(delta) * Math.min(rate * dt, Math.abs(delta));

    // ── drift: the ship slides ─────────────────────────────────────────────
    // Turning points you. It does not change where you are going. The sideways
    // part of your velocity bleeds off slowly, and engine power helps kill it.
    const frames = dt * 60;
    let lateral = this.lateralSpeed;
    lateral *= Math.pow(TUNING.DRIFT_DECAY + TUNING.ENGINE_DRIFT_BONUS * eng, frames);

    this.vel.x = fwd.x * newAlong + side.x * lateral;
    this.vel.y = fwd.y * newAlong + side.y * lateral;

    const drag = Math.pow(TUNING.SPACE_DRAG, frames);
    this.vel.x *= drag;
    this.vel.y *= drag;

    // ── dodge: a pure sideways shove ───────────────────────────────────────
    if (this.dodge) {
      this.dodge.t += dt;
      const step = TUNING.DODGE_DISTANCE * (dt / TUNING.DODGE_TIME);
      this.pos.x += side.x * this.dodge.dir * step;
      this.pos.y += side.y * this.dodge.dir * step;
      if (this.dodge.t >= TUNING.DODGE_TIME) this.dodge = null;
    }

    this.pos.x += this.vel.x * dt;
    this.pos.y += this.vel.y * dt;
  }
}
