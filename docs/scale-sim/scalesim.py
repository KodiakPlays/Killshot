"""Scale study: whole fights on the board's own rules, at different speed scales.

Run: python3 docs/scale-sim/scalesim.py   (no dependencies). README.md next to this file says what it measures.
BASE holds the game's numbers as of Step 63 (5x speeds); scale k multiplies them (k = 0.2 is the old 1x).
Keep BASE in step with HandoffBoardState / EnemyBrain whenever a number changes.

Mirrors (as of Step 63): TickMove / TickHelm physics, EnemyBrain's patrol, DRADIS hearing range, the side
map (port face -> reactor), slug and HE damage numbers, the slug path hit test, TRACK's exact lead,
the enemy's fire-back (2 HE every 120 s while it has cells), PDC-A's 70 % roll.
Speed scale k: every speed x k (ships, slugs, missiles), turns x k, time to top speed / k (motion as
if time ran k times faster); ranges, map, charge and fire-back interval unchanged.
One board tick = 0.42 s real = `rate` game seconds. Arming a shot takes a human ~4 real seconds."""
import math, random

TICK_REAL = 0.42
BASE = dict(top=12.5, acc=0.2175, brk=0.4075, drift=0.96307, drag=0.997502, turn=20.0,
            cruise=5.5, slug=250.0, missile=100.0, mturn=6.5)
HIT_R, OUR_HIT_R = 210.0, 210.0
WEAPON_RANGE = 47000.0
DRADIS = 12000.0 + 2 * 7000.0      # gain 2
ENEMY_HOME = (18058.42, 16259.874)
ROUTE = [(0, 0), (-6000, -4000), (0, -8000), (6000, -4000)]
PX_PER_KM = 0.0609                  # plot px per km at zoom 1 (the plot's maximum)
PLOT_W = 1712
ARM_REAL_S = 4.0                    # rockers + lever, per shot
CHARGE_S = 70 / (3 * 5)             # WEP 3 units: 70 % in 4.7 game s
SALVO_GAP, SALVO, CELLS = 120.0, 2, 8
PDC_P, PDC_COVER = 0.7, 0.5         # chance per missile, share of approaches PDC-A's arc covers

def brg(dx, dy): return math.degrees(math.atan2(dx, dy)) % 360
def side_at(from_deg, bow):
    rel = (from_deg - bow) % 360
    return "PROW" if rel < 45 or rel >= 315 else "STBD" if rel < 135 else "AFT" if rel < 225 else "PORT"

class Ship:
    def __init__(s, P, x, y, hdg):
        s.P, s.x, s.y, s.hdg, s.ord, s.thr, s.vx, s.vy, s.bank = P, x, y, hdg, hdg, 0, 0.0, 0.0, 0.0
        s.armour = {"PROW": 100.0, "STBD": 100.0, "AFT": 50.0, "PORT": 100.0}
        s.rct, s.dead = 5, False
    def tick(s, dt):
        P = s.P
        if s.hdg != s.ord:
            s.bank += P['turn'] * dt
            r = int(math.floor(s.bank + 1e-9))
            if r >= 1:
                s.bank -= r
                d = ((s.ord - s.hdg + 540) % 360) - 180
                s.hdg = s.ord if abs(d) <= r else (s.hdg + (1 if d > 0 else -1) * r) % 360
        else: s.bank = 0
        h = math.radians(s.hdg); nx, ny = math.sin(h), math.cos(h)
        fwd = s.vx * nx + s.vy * ny; side = s.vx * ny - s.vy * nx
        keep = P['drag'] ** dt; fwd *= keep; side *= keep * P['drift'] ** dt
        target = s.thr / 100 * P['top']
        step = (P['acc'] if fwd < target else P['brk']) * dt
        fwd = target if abs(target - fwd) <= step else fwd + math.copysign(step, target - fwd)
        s.vx = fwd * nx + side * ny; s.vy = fwd * ny - side * nx
        s.x += s.vx * dt; s.y += s.vy * dt
    def take(s, face, kind):
        """A slug (50 armour, then reactor -2 through PORT) or HE (100 armour, then -2)."""
        dmg = 50 if kind == "slug" else 100
        if s.armour[face] > 0: s.armour[face] = max(0.0, s.armour[face] - dmg); return
        if face == "PORT": s.rct = max(0, s.rct - 2); s.dead = s.rct <= 0

def params(k):
    P = dict(BASE)
    for key in ("top", "cruise", "slug", "missile", "turn", "mturn"): P[key] = BASE[key] * k
    P['acc'], P['brk'] = BASE['acc'] * k * k, BASE['brk'] * k * k
    P['drift'], P['drag'] = BASE['drift'] ** k, BASE['drag'] ** k
    return P

def fight(k, rate, engage=15000.0, seed=1, cap_game_s=8 * 3600):
    P = params(k); rnd = random.Random(seed)
    us = Ship(P, 0, 0, 0); en = Ship(P, *ENEMY_HOME, 0); leg = 1
    dt = 1.0; t = 0.0   # fine steps; `rate` only changes the human's arming time in game seconds
    log = dict(detect=None, kill=None, fired=0, hits=0, wasted=0, salvos=0, missiles_in=0, missile_hits=0, our_rct=5)
    slugs, missiles = [], []; next_shot = 0.0; next_salvo = 0.0; cells = CELLS
    arm_game = ARM_REAL_S / TICK_REAL * rate
    health0 = None
    while t < cap_game_s and not en.dead and not us.dead:
        # enemy patrol (EnemyBrain)
        wx, wy = ENEMY_HOME[0] + ROUTE[leg % 4][0], ENEMY_HOME[1] + ROUTE[leg % 4][1]
        if math.hypot(wx - en.x, wy - en.y) <= 300: leg = (leg + 1) % 4
        wx, wy = ENEMY_HOME[0] + ROUTE[leg % 4][0], ENEMY_HOME[1] + ROUTE[leg % 4][1]
        en.ord = round(brg(wx - en.x, wy - en.y)) % 360
        en.thr = round(P['cruise'] / P['top'] * 100)
        r = math.hypot(en.x - us.x, en.y - us.y)
        if log['detect'] is None and r <= DRADIS: log['detect'] = t
        if log['detect'] is not None:
            # our helm: hold a station off its PORT beam at the engage range, matching its course
            course = math.atan2(en.vx, en.vy) if math.hypot(en.vx, en.vy) > 1e-6 else math.radians(en.hdg)
            port = course - math.pi / 2
            sx, sy = en.x + math.sin(port) * engage, en.y + math.cos(port) * engage
            d = math.hypot(sx - us.x, sy - us.y)
            us.ord = round(brg(sx - us.x, sy - us.y)) % 360
            us.thr = 100 if d > 1500 else max(0, min(100, round(math.hypot(en.vx, en.vy) / P['top'] * 100)))
            if d <= 1500: us.ord = en.hdg
            # fire when the reactor face is towards us, in reach, charged and armed (TRACK leads exactly)
            face = side_at(brg(us.x - en.x, us.y - en.y), en.hdg)
            if face == "PORT" and r <= WEAPON_RANGE and t >= next_shot:
                tx, ty, vx, vy = en.x - us.x, en.y - us.y, en.vx, en.vy
                a = vx * vx + vy * vy - P['slug'] ** 2; b = 2 * (tx * vx + ty * vy); c = tx * tx + ty * ty
                disc = b * b - 4 * a * c
                th = (-b - math.sqrt(disc)) / (2 * a) if disc >= 0 else r / P['slug']
                aim = math.atan2(tx + vx * th, ty + vy * th)
                slugs.append([us.x, us.y, math.sin(aim), math.cos(aim), 0.0])
                log['fired'] += 1; next_shot = t + max(CHARGE_S, arm_game)
        # slugs: path test against the enemy's real motion
        for sl in list(slugs):
            step = min(P['slug'] * dt, WEAPON_RANGE - sl[4])
            px, py = en.x - sl[0], en.y - sl[1]
            u = max(0, min(step, px * sl[2] + py * sl[3])); cx, cy = px - sl[2] * u, py - sl[3] * u
            if cx * cx + cy * cy <= HIT_R ** 2:
                face = side_at(brg(-sl[2], -sl[3]), en.hdg)
                if face == "PORT": log['hits'] += 1
                else: log['wasted'] += 1
                en.take(face, "slug"); slugs.remove(sl)
                if t >= next_salvo and cells > 0:   # it fires back when hit
                    for _ in range(min(SALVO, cells)): missiles.append([en.x, en.y, 0.0])
                    cells -= SALVO; next_salvo = t + SALVO_GAP; log['salvos'] += 1; log['missiles_in'] += SALVO
                continue
            sl[0] += sl[2] * step; sl[1] += sl[3] * step; sl[4] += step
            if sl[4] >= WEAPON_RANGE - 1e-6: slugs.remove(sl)
        # enemy missiles home on us (straight-line pursuit is close enough at these turn rates)
        for m in list(missiles):
            d = math.hypot(us.x - m[0], us.y - m[1]); step = P['missile'] * dt
            if d <= step + OUR_HIT_R:
                missiles.remove(m)
                if rnd.random() < PDC_P * PDC_COVER: continue
                log['missile_hits'] += 1; us.take(side_at(brg(m[0] - us.x, m[1] - us.y), us.hdg), "HE"); continue
            m[0] += (us.x - m[0]) / d * step; m[1] += (us.y - m[1]) / d * step
        us.tick(dt); en.tick(dt); t += dt
    if en.dead: log['kill'] = t
    log['our_rct'] = us.rct; log['we_died'] = us.dead
    return log

def real_min(game_s, rate): return game_s / rate * TICK_REAL / 60

def plot_read(k, engage, rate, interval_real_s=30):
    """The fight framed on screen: zoom to fit, and what two timestamps `interval` apart show."""
    zoom = min(1.0, 0.6 * PLOT_W / (engage * 2 * PX_PER_KM))   # both ships and the gap on 60 % of the plot
    v = BASE['cruise'] * k
    game_s = interval_real_s / TICK_REAL * rate
    moved_px = v * game_s * PX_PER_KM * zoom
    course_err = math.degrees(math.atan2(1, moved_px)) if moved_px > 0 else 90
    # bearing lines 2 game-min apart from our ship crossing at our top speed
    base_km = BASE['top'] * k * 120
    ang = math.degrees(math.atan2(base_km, engage))
    return zoom, moved_px, course_err, ang

if __name__ == "__main__":
    print("Fights (detect -> kill), engaging off TRACK 07's PORT beam; 3 seeds averaged")
    print(f"{'scale':>5} {'engage km':>9} | {'x1 real':>7} | {'x5 real':>7} | {'x10 real':>8} | ours fired / hit / other face | their missiles in / landed")
    for k in (0.2, 0.6, 1, 2):
        for eng in (8000, 15000, 25000):
            res = {}
            for rate in (1, 5, 10):
                runs = [fight(k, rate, eng, seed) for seed in (1, 2, 3)]
                done = [r for r in runs if r['kill']]
                res[rate] = (sum(r['kill'] - r['detect'] for r in done) / len(done) if done else None, runs)
            g = lambda rt: f"{real_min(res[rt][0], rt):6.1f}m" if res[rt][0] else "   n/a"
            R = res[1][1]
            f = sum(r['fired'] for r in R) / 3; h = sum(r['hits'] for r in R) / 3; w = sum(r['wasted'] for r in R) / 3
            mi = sum(r['missiles_in'] for r in R) / 3; ml = sum(r['missile_hits'] for r in R) / 3
            print(f"{k:>5} {eng:>9,} | {g(1):>7} | {g(5):>7} | {g(10):>8} |   {f:4.1f} / {h:3.1f} / {w:3.1f}          |   {mi:3.1f} / {ml:3.1f}")
    print()
    print("Plot read (fight framed to fit the screen; contact at cruise; two marks 30 real s apart)")
    print(f"{'scale':>5} {'engage km':>9} | zoom | x1: moved / course err | x10: moved / course err | bearing lines 2 game-min apart")
    for k in (0.2, 0.6, 1, 2):
        for eng in (8000, 15000, 25000):
            z, m1, e1, ang = plot_read(k, eng, 1); _, m10, e10, _ = plot_read(k, eng, 10)
            print(f"{k:>5} {eng:>9,} | {z:4.2f} | {m1:6.1f} px / +-{e1:4.1f} deg | {m10:7.1f} px / +-{e10:4.1f} deg | {ang:5.1f} deg")
