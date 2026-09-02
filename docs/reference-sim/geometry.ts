// Angles are the easiest thing to get wrong in this game, so they are typed.
// A Bearing is absolute: 0 is +Y, increasing clockwise, 090 is +X.
// A Relative is measured from our own nose: 0 is dead ahead, +90 is starboard.
// Screen angle = bearing - 90.

export type Bearing = number & { readonly __bearing: unique symbol };
export type Relative = number & { readonly __relative: unique symbol };

export interface Vec2 { x: number; y: number }

export const vec = (x: number, y: number): Vec2 => ({ x, y });

/** Wrap into 0..360 and tag as an absolute bearing. */
export const bearing = (deg: number): Bearing =>
  (((deg % 360) + 360) % 360) as Bearing;

/** Tag a value as measured from our own nose, wrapped into (-180, 180]. */
export const relative = (deg: number): Relative =>
  (((deg % 360) + 540) % 360 - 180) as Relative;

/** Smallest signed difference between two angles, in (-180, 180]. */
export const angDiff = (a: number, b: number): number =>
  ((a - b + 540) % 360) - 180;

export const bearingTo = (from: Vec2, to: Vec2): Bearing =>
  bearing(Math.atan2(to.x - from.x, to.y - from.y) * 180 / Math.PI);

export const unit = (b: Bearing): Vec2 => {
  const r = (b as number) * Math.PI / 180;
  return { x: Math.sin(r), y: Math.cos(r) };
};

export const dist = (a: Vec2, b: Vec2): number => Math.hypot(b.x - a.x, b.y - a.y);

/** Where a bearing sits relative to our nose. This is what turret arcs are measured in. */
export const toRelative = (target: Bearing, ownHeading: Bearing): Relative =>
  relative(angDiff(target as number, ownHeading as number));

/**
 * Angle on bow: what aspect the target is showing us.
 * 0 means it is pointing straight at us, 180 means it is running away.
 */
export const angleOnBow = (targetHeading: Bearing, ownPos: Vec2, targetPos: Vec2): number =>
  Math.abs(angDiff(bearingTo(targetPos, ownPos) as number, targetHeading as number));

/** Rotation to draw something at this bearing on a screen whose up is 0. */
export const screenAngle = (b: Bearing): number => (b as number) - 90;

export const clamp = (v: number, lo: number, hi: number) => Math.max(lo, Math.min(hi, v));
