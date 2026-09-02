import { TUNING, AWARENESS_GATES } from './tuning';
import { Power } from './power';
import { Ship } from './ship';
import { Radar, type Contact } from './radar';
import { Slug, Turret, makeTurrets } from './weapons';
import { bearing, bearingTo, dist, toRelative, unit, type Bearing, type Vec2 } from './geometry';

/** The clock stops when you pause. There is no fast-forward — density answers emptiness. */
export class Clock {
  t = 0;
  paused = false;
  step(realDt: number): number {
    if (this.paused) return 0;
    const dt = Math.min(realDt, 0.05);   // a long frame must never make a big step
    this.t += dt;
    return dt;
  }
}

export type EnemyMode = 'patrol' | 'investigate' | 'engage';

/** One hostile. Patrols until something makes it curious, then comes looking. */
export class Enemy implements Contact {
  readonly id: number;
  pos: Vec2;
  heading: Bearing;
  speed = 60;
  mode: EnemyMode = 'patrol';
  awareness = 0;
  lastKnownPlayer: Vec2 | null = null;
  private waypoint: Vec2;

  constructor(id: number, x: number, y: number) {
    this.id = id;
    this.pos = { x, y };
    this.heading = bearing(0);
    this.waypoint = this.newWaypoint();
  }

  private newWaypoint(): Vec2 {
    const a = Math.random() * Math.PI * 2;
    const r = TUNING.ARENA_RADIUS * (0.2 + Math.random() * 0.5);
    return { x: Math.cos(a) * r, y: Math.sin(a) * r };
  }

  step(dt: number, playerPos: Vec2, playerSignature: number): void {
    const range = dist(this.pos, playerPos);

    // Awareness only ever goes up. You cannot make them forget.
    for (const gate of AWARENESS_GATES) {
      if (range < gate.distance && playerSignature > gate.signature) {
        this.awareness = Math.max(this.awareness, gate.level);
      }
    }
    if (this.awareness >= 1) this.lastKnownPlayer = { ...playerPos };

    if (this.awareness >= 3) this.mode = 'engage';
    else if (this.awareness >= 1) this.mode = 'investigate';
    else this.mode = 'patrol';

    let goal: Vec2;
    if (this.mode === 'engage') goal = playerPos;
    else if (this.mode === 'investigate' && this.lastKnownPlayer) goal = this.lastKnownPlayer;
    else {
      goal = this.waypoint;
      if (dist(this.pos, goal) < 1200) this.waypoint = this.newWaypoint();
    }

    // Same commanded/actual model as the player, just driven by AI.
    const want = bearingTo(this.pos, goal);
    const err = ((want as number) - (this.heading as number) + 540) % 360 - 180;
    const turn = Math.max(-40 * dt, Math.min(40 * dt, err));
    this.heading = bearing((this.heading as number) + turn);

    const v = unit(this.heading);
    const speed = this.mode === 'patrol' ? this.speed : this.speed * 1.6;
    this.pos.x += v.x * speed * dt;
    this.pos.y += v.y * speed * dt;
  }
}

export class World {
  readonly clock = new Clock();
  readonly power = new Power();
  readonly ship = new Ship(-30000, 20000, 120);
  readonly radar = new Radar();
  readonly turrets: Turret[] = makeTurrets();
  readonly enemies: Enemy[] = [];
  slugs: Slug[] = [];

  /** A shot is a brief, very loud spike. It decays. */
  private firingSpike = 0;

  constructor() {
    this.enemies.push(new Enemy(1, 12000, -9000));
    this.enemies.push(new Enemy(2, -8000, -18000));
  }

  /** Detection = energy in systems + what you are doing + environment. */
  get signature(): number {
    let sig = this.power.allocated;
    if (this.radar.active) sig += TUNING.SIG_RADAR_ACTIVE;   // looking is the loudest thing you do
    sig += Math.abs(this.ship.forwardSpeed) / TUNING.MAX_SPEED
         * this.power.efficiency('ENGINES') * TUNING.SIG_SPEED_WEIGHT;
    sig += this.firingSpike;
    return sig;
  }

  get awareness(): number {
    return this.enemies.reduce((m, e) => Math.max(m, e.awareness), 0);
  }

  /** Which turrets can currently reach this point, and at what bearing. */
  solutionFor(target: Vec2): { turret: Turret; bearing: Bearing; range: number; flightTime: number }[] {
    const b = bearingTo(this.ship.pos, target);
    const rel = toRelative(b, this.ship.heading);
    const range = dist(this.ship.pos, target);
    return this.turrets
      .filter(t => t.bears(rel) && range <= TUNING.RAILGUN_RANGE)
      .map(t => ({ turret: t, bearing: b, range, flightTime: Slug.flightTime(range) }));
  }

  fireReady(): number {
    let fired = 0;
    for (const t of this.turrets) {
      const slug = t.fire(this.ship.pos);
      if (slug) { this.slugs.push(slug); fired++; }
    }
    if (fired > 0) this.firingSpike = TUNING.SIG_FIRING_SPIKE;
    return fired;
  }

  step(realDt: number): number {
    const dt = this.clock.step(realDt);
    if (dt === 0) return 0;

    this.power.step(dt);
    this.ship.step(dt, this.power);

    // Radar is only worth having on if you accept being heard.
    this.radar.step(dt, this.ship.pos, this.enemies, this.clock.t);

    const sig = this.signature;
    for (const e of this.enemies) e.step(dt, this.ship.pos, sig);

    for (const s of this.slugs) s.step(dt);
    this.slugs = this.slugs.filter(s => s.alive);

    for (const t of this.turrets) t.step(dt, this.clock.t, this.power.points.WEAPONS);

    this.firingSpike = Math.max(0, this.firingSpike - dt / TUNING.SIG_SPIKE_DECAY * TUNING.SIG_FIRING_SPIKE);

    return dt;
  }
}
