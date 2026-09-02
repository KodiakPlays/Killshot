import { TUNING } from './tuning';

export const SYSTEMS = ['ENGINES', 'WEAPONS', 'SCANNER', 'COMMS', 'AUXILIARY'] as const;
export type SystemName = typeof SYSTEMS[number];

export type PowerMode = 'standby' | 'draw' | 'vent';

/**
 * Ten reactor points. Five systems, five each. You can fully run two of them.
 *
 * Points are allocated, not spent — they sit either in the reactor or in a
 * system, and the total never changes. Moving them costs time, which is what
 * makes a configuration a commitment rather than a menu.
 */
export class Power {
  readonly points: Record<SystemName, number>;
  readonly mode: Record<SystemName, PowerMode>;
  private readonly fill: Record<SystemName, number>;
  /** Draw -> standby -> vent is the cycle. You always pass back through standby. */
  private readonly drawnOnce: Record<SystemName, boolean>;

  constructor() {
    this.points = {} as Record<SystemName, number>;
    this.mode = {} as Record<SystemName, PowerMode>;
    this.fill = {} as Record<SystemName, number>;
    this.drawnOnce = {} as Record<SystemName, boolean>;
    for (const s of SYSTEMS) {
      this.points[s] = 0;
      this.mode[s] = 'standby';
      this.fill[s] = 0;
      this.drawnOnce[s] = false;
    }
    this.points.ENGINES = 2;   // you start able to move, and nothing else
  }

  get allocated(): number {
    let n = 0;
    for (const s of SYSTEMS) n += this.points[s];
    return n;
  }

  get reactorFree(): number { return TUNING.REACTOR_MAX - this.allocated; }

  /** What a consumer reads. 0..1. */
  efficiency(s: SystemName): number { return this.points[s] / TUNING.SYSTEM_MAX; }

  /** Standby -> Draw -> Standby -> Vent -> Standby. */
  cycle(s: SystemName): void {
    const m = this.mode[s];
    if (m === 'draw') { this.mode[s] = 'standby'; this.drawnOnce[s] = true; return; }
    if (m === 'vent') return;                       // a vent cannot be interrupted
    if (this.drawnOnce[s] && this.points[s] > 0) { this.mode[s] = 'vent'; this.drawnOnce[s] = false; return; }
    this.mode[s] = 'draw';
  }

  /** Dump everything. Used by Emergency Vent. */
  ventAll(): void {
    for (const s of SYSTEMS) if (this.points[s] > 0) this.mode[s] = 'vent';
  }

  step(dt: number): void {
    const drawing = SYSTEMS.filter(s => this.mode[s] === 'draw');
    // Total intake is constant. Drawing on three systems splits it three ways —
    // you choose what fills first, not how much arrives.
    const perSystem = drawing.length
      ? 1 / (TUNING.DRAW_SECONDS_PER_POINT * drawing.length)
      : 0;

    for (const s of SYSTEMS) {
      const mode = this.mode[s];

      if (mode === 'draw') {
        if (this.points[s] >= TUNING.SYSTEM_MAX || this.reactorFree <= 0) {
          this.mode[s] = 'standby';
          this.drawnOnce[s] = true;
          this.fill[s] = 0;
          continue;
        }
        this.fill[s] += perSystem * dt;
        while (this.fill[s] >= 1 && this.points[s] < TUNING.SYSTEM_MAX && this.reactorFree > 0) {
          this.fill[s] -= 1;
          this.points[s] += 1;
        }
      } else if (mode === 'vent') {
        // Once a vent starts it runs to zero. It cannot be stopped.
        this.fill[s] -= TUNING.VENT_POINTS_PER_SECOND * dt;
        while (this.fill[s] < 0 && this.points[s] > 0) {
          this.fill[s] += 1;
          this.points[s] -= 1;
        }
        if (this.points[s] === 0) { this.fill[s] = 0; this.mode[s] = 'standby'; }
      } else {
        this.fill[s] = 0;
      }
    }
  }

  /** 0..1 progress toward the next point, for the bar animation. */
  fillProgress(s: SystemName): number {
    return this.mode[s] === 'standby' ? 0 : Math.abs(this.fill[s]) % 1;
  }
}
