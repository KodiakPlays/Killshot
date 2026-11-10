"""Fight calculator: Starwolf fights played as maths, many times, on the game's own rules and numbers.

Run:  python3 docs/scale-sim/scalesim.py              every level, player type and first move, at x1 and x5
      python3 docs/scale-sim/scalesim.py --fights 8   fights per seed set (default 12; two seed sets per case)
      python3 docs/scale-sim/scalesim.py --csv FILE   also append one row per case to FILE (numbers.csv columns)
      python3 docs/scale-sim/scalesim.py --tweaks     changed numbers, Wolf Hunt only
      python3 docs/scale-sim/scalesim.py --longshot   long shots with the spinal railgun: flight, hit chance by range, drift, stop and turn
      python3 docs/scale-sim/scalesim.py --hand       one fight step by step, to check by hand
      python3 docs/scale-sim/scalesim.py --check      the game numbers it reads, and the ones still copied by hand
Plain Python, no packages. It reads Assets/Ships/*.asset, Assets/Scenes/Levels/*.unity and the enemy, mission and
board settings in Assets/Scenes/Tactical/02.1_Grid.unity itself, and never writes to them. README.md says what it
copies and what it leaves out. Units and game seconds throughout; at x1 a game second is a real second.
"""
import math, random, re, os, sys, glob

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
def P(*a): return os.path.join(ROOT, *a)

# ---------------------------------------------------------------- reading the game's files

def _val(s):
    s = s.strip()
    if s.startswith('{') and s.endswith('}'):
        return {k.strip(): _val(v) for k, _, v in (p.partition(':') for p in s[1:-1].split(',')) if k.strip()}
    if len(s) > 12 and s.isdigit(): return s           # packed int arrays (crew), not numbers
    try: return float(s)
    except ValueError: return s.strip("'\"")

def read_yaml(text):
    """The small part of Unity's YAML a ship file uses: maps, lists (dash at the parent's indent), {x, y}, numbers."""
    rows = [(len(l) - len(l.lstrip(' ')), l.strip()) for l in text.split('\n') if l.strip() and not l.startswith(('%', '---'))]
    def block(i, ind):
        if rows[i][1].startswith('-'):
            out = []
            while i < len(rows) and rows[i][0] == ind and rows[i][1].startswith('-'):
                item = rows[i][1][1:].strip()
                if re.match(r'^\w+:(\s|$)', item): rows[i] = (ind + 2, item); v, i = block(i, ind + 2); out.append(v)
                else: out.append(_val(item)); i += 1
            return out, i
        out = {}
        while i < len(rows) and rows[i][0] == ind and not rows[i][1].startswith('-'):
            k, _, rest = rows[i][1].partition(':'); i += 1
            if rest.strip(): out[k] = _val(rest)
            elif i < len(rows) and (rows[i][0] > ind or rows[i][0] == ind and rows[i][1].startswith('-')): out[k], i = block(i, rows[i][0])
            else: out[k] = None
        return out, i
    return block(0, 0)[0]

def ship_file(name): return read_yaml(open(P('Assets/Ships', name + '.asset'), encoding='utf-8').read())['MonoBehaviour']
def _guid(rel): return re.search(r'guid: (\w+)', open(P(rel + '.meta')).read()).group(1)
def _docs(path):
    out = []
    for d in re.split(r'\n--- !u!', open(path, encoding='utf-8').read()):
        m = re.match(r'(\d+) &(-?\d+)( stripped)?', d)
        if m: out.append((m.group(1), m.group(2), d, bool(m.group(3))))
    return out
def _num(t, key, default=None):
    m = re.search(r'\n\s*' + key + r': (-?[\d.]+(?:[eE]-?\d+)?)[ \t]*(?=\n|$)', t)
    return float(m.group(1)) if m else default
def _script(t): m = re.search(r'm_Script: \{fileID: \d+, guid: (\w+)', t); return m.group(1) if m else None
def _go(t): m = re.search(r'm_GameObject: \{fileID: (\d+)', t); return m.group(1) if m else None
def _hexints(h): return [int.from_bytes(bytes.fromhex(h[i:i + 8]), 'little') for i in range(0, len(h), 8)]

BRAIN_KEYS = ('cruiseUnits arriveUnits driftSeconds legMinUnits searchUnits giveUpSeconds turnBackChance fighterSignal searchingAt '
              'alertAt fixSeconds fightAtShare avoidLookUnits fightSpeedShare railgunReloadSeconds railgunJitterSeconds aimErrorDeg '
              'salvoSeconds fireBackSalvo fireBackSeconds').split()

def read_board():
    """The enemy brains, the mission, the world and the board, as set in the board scene (not the code's defaults)."""
    D = _docs(P('Assets/Scenes/Tactical/02.1_Grid.unity'))
    mb = [(_script(t), _go(t), t) for c, f, t, s in D if c == '114']
    g = lambda rel: _guid('Assets/Scripts/' + rel + '.cs')
    def on_ship(asset):
        sg, ag = g('Systems/Ship'), _guid('Assets/Ships/' + asset + '.asset')
        return next(o for s, o, t in mb if s == sg and ag in t)
    def comp(rel, go=None): return next(t for s, o, t in mb if s == g(rel) and (go is None or o == go))
    out = {}
    for who, asset in (('Garuda', 'Garuda'), ('Frigate', 'Frigate')):
        t = comp('Tactical/EnemyBrain', on_ship(asset))
        out[who] = {k: _num(t, k) for k in BRAIN_KEYS}
        out[who]['gain'] = _hexints(re.search(r'gainByAwareness: (\w+)', t).group(1))
    q = comp('Game/QuestSystem')
    out['quest'] = {k: _num(q, k) for k in ('exitInsetUnits', 'exitReachUnits', 'reinforceSeconds', 'firstWave', 'reinforceEverySeconds', 'arriveInsetUnits', 'briefErrorUnits')}
    w = comp('Tactical/TacticalWorld'); out['world'] = {k: _num(w, k) for k in ('rockInterferenceUnits', 'rockCloseUnits')}
    bm = comp('Game/BoardManager')
    out['board'] = {k: _num(bm, k) for k in ('tickSeconds', 'scopeFitUnits', 'scopeFitSlack', 'onBearingDeg', 'arenaWarnUnits')}
    ws = comp('Systems/WeaponSystem', on_ship('Asura'))
    out['shot'] = {m[0]: float(m[1]) for m in re.findall(r'- id: (\w+)\n\s+label: [^\n]*\n\s+shot: ([\d.]+)', ws)}
    return out

GOALS = ['WolfHunt', 'Destroy', 'HitUnseen', 'Capture', 'FindBySignal', 'Flight', 'JumpPoint']

def read_level(name):
    """A level's layout from its scene: map size, spawns, the enemy's route, rocks, clouds, the jump point, the goal."""
    D = _docs(P('Assets/Scenes/Levels', name + '.unity'))
    tf, go_tf = {}, {}
    for c, f, t, s in D:
        if c == '4' and not s:
            p = re.search(r'm_LocalPosition: \{x: (\S+), y: (\S+),', t)
            r = re.search(r'm_LocalRotation: \{x: \S+, y: \S+, z: (\S+), w: (\S+)\}', t)
            tf[f] = (float(p.group(1)), float(p.group(2)), math.degrees(2 * math.atan2(float(r.group(1)), float(r.group(2)))),
                     re.search(r'm_Father: \{fileID: (\d+)', t).group(1))
            go_tf[_go(t)] = f
    def up(fid, x, y, a=0.0):            # a point local to transform fid's children, in world units (parent scale taken as 1)
        while fid in tf:
            px, py, pa, fa = tf[fid]; c, s = math.cos(math.radians(pa)), math.sin(math.radians(pa))
            x, y, a, fid = px + x * c - y * s, py + x * s + y * c, a + pa, fa
        return x, y, a
    def placed(go): x, y, a, fa = tf[go_tf[go]]; return up(fa, x, y, a)
    G = {k: _guid('Assets/Scripts/' + k + '.cs') for k in ('Tactical/EnemySpawn', 'Tactical/PlayerSpawn', 'Tactical/BattleSpace', 'Game/Mission', 'Game/GasCloud')}
    terrain = {re.search(r'guid: (\w+)', open(m).read()).group(1) for m in glob.glob(P('Assets/Prefabs/Terrain/**/*.prefab.meta'), recursive=True)}
    jump_g = _guid('Assets/Prefabs/JumpPoint.prefab')
    lv = dict(name=name, w=100000.0, h=100000.0, goal='WolfHunt', weapons_off=False, clouds=[], rocks=[], jump=None, enemy=None, player=None)
    for c, f, t, s in D:
        if c == '114':
            sg, go = _script(t), _go(t)
            if sg == G['Tactical/BattleSpace']: lv['w'], lv['h'] = _num(t, 'widthUnits'), _num(t, 'heightUnits')
            elif sg == G['Game/Mission']: lv['goal'] = GOALS[int(_num(t, 'goal', 0))]; lv['weapons_off'] = _num(t, 'weaponsOff', 0) == 1
            elif sg == G['Game/GasCloud']: x, y, _ = placed(go); lv['clouds'].append((x, y, _num(t, 'widthUnits'), _num(t, 'heightUnits')))
            elif sg == G['Tactical/PlayerSpawn']: x, y, a = placed(go); lv['player'] = (x, y, jsround(-a) % 360)
            elif sg == G['Tactical/EnemySpawn']:
                x, y, a = placed(go); size = re.search(r'sizeUnits: \{x: (\S+), y: (\S+)\}', t)
                route = [(float(u), float(v)) for u, v in re.findall(r'\n\s+- \{x: (\S+), y: (\S+)\}', t.split('route:')[1])] if 'route:' in t else []
                lv['enemy'] = dict(x=x, y=y, hdg=jsround(-a) % 360, box=(float(size.group(1)), float(size.group(2))), hunts=_num(t, 'hunts') == 1,
                                   cruise=_num(t, 'cruiseUnits', -1), signal=_num(t, 'signalSeconds', 0), first_signal=_num(t, 'firstSignalSeconds', 15),
                                   on_air=_num(t, 'onAirSeconds', 60), route=route)
        elif c == '1001':
            src = re.search(r'm_SourcePrefab: \{fileID: \d+, guid: (\w+)', t)
            if not src or src.group(1) not in terrain and src.group(1) != jump_g: continue
            mods = dict(re.findall(r'propertyPath: (m_Local\w+\.\w)\n\s+value: (\S+)', t))
            x, y, _ = up(re.search(r'm_TransformParent: \{fileID: (\d+)', t).group(1), float(mods.get('m_LocalPosition.x', 0)), float(mods.get('m_LocalPosition.y', 0)))
            if src.group(1) == jump_g: lv['jump'] = (x, y)
            else: lv['rocks'].append((x, y, float(mods.get('m_LocalScale.x', 100)) / 2))   # scale = size across (a stand-in for its outline)
    return lv

# Rules copied by hand from the game's code: each with the line it comes from. --check prints them.
CODE = dict(torpedo_step_s=0.05,         # WeaponSystem.TorpedoStepSeconds
            searching_speed_share=0.8,   # EnemyBrain.Steer
            heard_guess_share=0.25,      # EnemyBrain.Listen: where it thinks we are, off by up to this share of the distance
            hit_guess_share=0.15,        # EnemyBrain.Listen: hit but not heard
            twice_window=(20, 60),       # EnemyBrain.Listen
            hunt_area=(0.85, 0.5),       # EnemyBrain.NewHuntPoint
            no_ammo_reach=10000,         # EnemyBrain.Reach with no slugs left
            scope_zoom=(1, 8),           # ShipState.SetZoom
            exit_min_spawn=8000,         # QuestSystem.Arrive
            party_rate=1)                # DamageSystem.RepairRate at alert 0

# ---------------------------------------------------------------- small maths

def jsround(x): return math.floor(x + 0.5)
def brg(dx, dy): return math.degrees(math.atan2(dx, dy)) % 360
def ibrg(dx, dy): return jsround(brg(dx, dy)) % 360
def angdiff(a, b): return abs((a - b + 540) % 360 - 180)
def delta(a, b): return (b - a + 540) % 360 - 180
def clamp(v, lo, hi): return lo if v < lo else hi if v > hi else v
def side_at(from_deg, bow):
    rel = (from_deg - bow) % 360
    return 0 if rel < 45 or rel >= 315 else 1 if rel < 135 else 2 if rel < 225 else 3
SIDES = ('PROW', 'STBD', 'AFT', 'PORT')
ROOMS = ('mag', 'bri', 'hab', 'hgr', 'rct', 'eng')
ROOM_ROW = (1, 2, 4, 4, 3, 5)   # DamageSystem.Stats.roomRow default (Step 186), for ship files with no Room Row saved
NEIGH = dict(mag=('bri', 'rct', 'hgr'), bri=('mag', 'hab', 'rct', 'hgr'), hab=('bri', 'eng', 'rct', 'hgr'),
             eng=('hab', 'rct', 'hgr'), rct=('mag', 'bri', 'hab', 'eng'), hgr=('mag', 'bri', 'hab', 'eng'))

def enters_box(fx, fy, tx, ty, bow, hw, hl):
    """ShipState.EntersBox: where a path from f to t (relative to the box's centre) first touches the box, the side, and how far
    along the ship that spot is, 0 the bow to 1 the stern (WeaponSystem: 1 - breakAt)."""
    b = math.radians(bow); s, c = math.sin(b), math.cos(b)
    ax, ay, bx, by = fx * c - fy * s, fx * s + fy * c, tx * c - ty * s, tx * s + ty * c
    def slab(st, mv, half):
        if abs(mv) < 1e-9: return (-1e18, 1e18) if abs(st) <= half else (1e18, -1e18)
        p, q = (-half - st) / mv, (half - st) / mv; return min(p, q), max(p, q)
    ix, ox = slab(ax, bx - ax, hw); iy, oy = slab(ay, by - ay, hl)
    at = max(0, ix, iy)
    if at > min(1, ox, oy): return None
    beam = ix >= iy if (ix > 0 or iy > 0) else abs(ax) / hw >= abs(ay) / hl
    return at, ((1 if ax > 0 else 3) if beam else (0 if ay > 0 else 2)), clamp(0.5 - (ay + (by - ay) * at) / (2 * hl), 0.0, 1.0)

def intercept(tx, ty, vx, vy, s):
    """Seconds until a shot at speed s meets a target at (tx, ty) moving (vx, vy); None if it can't."""
    a = vx * vx + vy * vy - s * s; b = 2 * (tx * vx + ty * vy); c = tx * tx + ty * ty
    disc = b * b - 4 * a * c
    if a >= 0 or disc < 0: return None
    t = (-b - math.sqrt(disc)) / (2 * a)
    return t if t > 0 else None

# ---------------------------------------------------------------- the world: rocks, clouds, the map

class World:
    def __init__(w, lv, B):
        w.lv, w.hw, w.hh, w.band = lv, lv['w'] / 2, lv['h'] / 2, B['board']['arenaWarnUnits']
        w.unit, w.close = B['world']['rockInterferenceUnits'], B['world']['rockCloseUnits']
        w.rocks, w.cell, w.grid = lv['rocks'], 1000.0, {}
        w.maxr = max([r for _, _, r in w.rocks] or [0])
        for rk in w.rocks: w.grid.setdefault((int(rk[0] // w.cell), int(rk[1] // w.cell)), []).append(rk)
    def near(w, x0, y0, x1, y1, m):
        out = []
        for i in range(int((min(x0, x1) - m) // w.cell), int((max(x0, x1) + m) // w.cell) + 1):
            for j in range(int((min(y0, y1) - m) // w.cell), int((max(y0, y1) + m) // w.cell) + 1):
                out.extend(w.grid.get((i, j), ()))
        return out
    def interference(w, ax, ay, bx, by):
        """TacticalWorld.Interference: units off DRADIS and scan reach from a to a ship at b, and whether a rock is on the line."""
        if not w.rocks: return 0.0, False
        dx, dy = bx - ax, by - ay; L2 = dx * dx + dy * dy or 1e-9; on = close = 0
        for rx, ry, r in w.near(ax, ay, bx, by, w.maxr + w.close):
            if (rx - bx) ** 2 + (ry - by) ** 2 <= (r + w.close) ** 2: close += 1
            u = clamp(((rx - ax) * dx + (ry - ay) * dy) / L2, 0, 1); px, py = ax + dx * u - rx, ay + dy * u - ry
            if px * px + py * py <= r * r: on += 1
        return (on + close) * w.unit, on > 0
    def clear_heading(w, sh, wanted, look, ships):
        """TacticalWorld.ClearHeading: the first heading 0, 30, 60 ... 150 degrees off that has no rock or ship in the way."""
        clear = math.hypot(*sh.half); first = 1 if delta(wanted, sh.hdg) >= 0 else -1
        rocks = w.near(sh.x, sh.y, sh.x, sh.y, look + w.maxr + clear)
        if not rocks and not ships: return wanted
        def way(h):
            ax, ay = math.sin(math.radians(h)), math.cos(math.radians(h))
            for ox, oy, wd in [(rx - sh.x, ry - sh.y, r + clear) for rx, ry, r in rocks] + [(o.x - sh.x, o.y - sh.y, math.hypot(*o.half) + clear) for o in ships]:
                along = ox * ax + oy * ay
                if -wd < along < look + wd and abs(ox * ay - oy * ax) < wd: return False
            return True
        for off in range(0, 151, 30):
            for sd in (first, -first):
                h = (wanted + sd * off) % 360
                if way(h): return h
        return wanted
    def in_cloud(w, sh):
        for cx, cy, cw, ch in w.lv['clouds']:
            if abs(sh.x - cx) <= cw / 2 - sh.half[0] * 0.9 and abs(sh.y - cy) <= ch / 2 - sh.half[1] * 0.9: return True
        return False
    def inside(w, x, y):
        hw, hh = max(0, w.hw - w.band), max(0, w.hh - w.band); return clamp(x, -hw, hw), clamp(y, -hh, hh)
    def keep_inside(w, x, y, heading):
        """EnemyBrain.KeepInside."""
        ex, ey = w.inside(x, y)
        if (ex, ey) == (x, y): return heading
        if abs(x) > w.hw or abs(y) > w.hh: return ibrg(-x, -y)
        wx, wy = math.sin(math.radians(heading)), math.cos(math.radians(heading))
        if ex != x and wx * x > 0: wx = 0
        if ey != y and wy * y > 0: wy = 0
        return ibrg(-x, -y) if wx * wx + wy * wy < 0.01 else ibrg(wx, wy)

# ---------------------------------------------------------------- a ship: movement and damage (NavSystem, DamageSystem)

class Ship:
    def __init__(s, st, x, y, hdg, rnd, name):
        s.st, s.nv, s.wp, s.dm, s.name, s.rnd = st, st['navigation'], st['weapons'], st['damage'], name, rnd
        s.x, s.y, s.hdg, s.ord, s.thr, s.vx, s.vy, s.bank = x, y, int(hdg) % 360, int(hdg) % 360, 0, 0.0, 0.0, 0.0
        s.eng, s.bus_max = 5, int(st['power']['busMax'])
        s.half = (st['width'] / 2, st['length'] / 2)
        s.armour = [float(a) for a in s.dm['sideArmourMax']]; s.comp = dict.fromkeys(ROOMS, 5)
        crew = _hexints(s.dm['crew']) if isinstance(s.dm['crew'], str) else [4, 9, 18, 7, 6, 8]
        s.crew = dict(zip(ROOMS, crew))
        def rooms(key, dflt): v = s.dm.get(key); return dict(zip(ROOMS, _hexints(v) if isinstance(v, str) and v else dflt))
        s.room_row = rooms('roomRow', ROOM_ROW); s.rows = max(s.room_row.values())
        s.vented, s.shut, s.fire, s.party = set(), set(), {}, None
        s.dead = s.captured = False
        s.mounts = {m['id']: m for m in s.wp['mounts']}; s.ammo = {m['id']: int(m['ammo']) for m in s.wp['mounts']}
        s.dots = {h['mount']: (h['at']['x'], h['at']['y']) for h in s.wp['hardPoints'] if h.get('mount')}
        s.in_cloud, s.signal = False, 0.0
        s.fine, s.aim, s.null_slide, s.on_aim = 0.0, None, False, False      # NavSystem.fine (the part of a degree) and aimHeading (a lock's turn, or the person's Shift turn)
    def true_hdg(s): return (s.hdg + s.fine) % 360
    def roll(s, p): return s.rnd.random() < p
    def health(s, k): return 0 if k in s.vented else s.comp[k]
    def mag_ok(s): return s.health('mag') >= 3
    def step_share(s): return max(0, s.eng - 1) / max(1, s.bus_max - 1)
    def accel(s): n = s.nv; return n['baseAccelUnits'] + (n['accelUnits'] - n['baseAccelUnits']) * s.step_share()
    def turn_per_s(s): n = s.nv; return max(1, jsround((n['baseTurnRate'] + (n['turnRate'] - n['baseTurnRate']) * s.step_share()) * s.health('eng') / 5))
    def eng_share(s):
        n = s.nv
        return 0 if s.eng <= 0 else (n['baseSpeedUnits'] + (n['topSpeedUnits'] - n['baseSpeedUnits']) * (s.eng - 1) / max(1, s.bus_max - 1)) / n['topSpeedUnits']
    def throttle_cap(s): return jsround(s.eng_share() * 100 * s.health('eng') / 5)
    def speed(s): return math.hypot(s.vx, s.vy)
    def gun_at(s, mid):
        ax, ay = s.dots.get(mid, (0, 0)); b = math.radians(s.hdg); c, sn = math.cos(b), math.sin(b)
        return s.x + ax * c + ay * sn, s.y + ay * c - ax * sn
    def tick(s, dt):
        n = s.nv
        if abs(s.thr) > jsround(s.eng_share() * 100): s.thr = 0
        turned = s.on_aim = False
        if s.aim is not None:          # NavSystem.Steer: whole degrees at the usual rate, then the last part at the slow turn's rate
            whole = math.floor(s.aim + 0.5); s.ord = whole % 360
            if s.hdg == s.ord:
                s.bank = 0; want, most = s.aim - whole, n.get('fineTurnDegPerS', 0) * dt
                s.on_aim = abs(want - s.fine) <= most       # the bow has caught up with the aim it was given
                s.fine = want if s.on_aim else s.fine + math.copysign(most, want - s.fine); turned = True
        if turned: pass
        elif s.hdg != s.ord:
            s.fine = 0.0               # a whole-degree turn drops the part of a degree
            s.bank += s.turn_per_s() * dt; step = math.floor(s.bank + 1e-9)
            if step >= 1:
                s.bank -= step; d = (s.ord - s.hdg + 540) % 360 - 180
                s.hdg = s.ord if abs(d) <= step else (s.hdg + (1 if d > 0 else -1) * step) % 360
        else: s.bank = 0
        if s.eng == 0: s.x += s.vx * dt; s.y += s.vy * dt; return
        h = math.radians(s.hdg); nx, ny = math.sin(h), math.cos(h)
        fwd, side = s.vx * nx + s.vy * ny, s.vx * ny - s.vy * nx
        keep = n['dragKeepPerS'] ** dt; fwd *= keep; side *= keep * n['driftKeepPerS'] ** dt
        if s.null_slide and side:      # Q and E held against the slide, full push (the 2 s ramp of Step 171 left out)
            most = n.get('sideAccelUnits', 0) * s.eng_share() * dt; side = 0.0 if abs(side) <= most else side - math.copysign(most, side)
        target = s.thr / 100 * n['topSpeedUnits'] * (n['reverseShare'] if s.thr < 0 else 1)
        up = (fwd >= 0) if target > fwd else (fwd <= 0)
        acc = s.accel(); rate = n['brakeUnits'] * acc / n['accelUnits'] if not up else acc * n['reverseShare'] if target < 0 else acc
        fwd = target if abs(target - fwd) <= rate * dt else fwd + math.copysign(rate * dt, target - fwd)
        s.vx, s.vy = fwd * nx + side * ny, fwd * ny - side * nx
        s.x += s.vx * dt; s.y += s.vy * dt
    # damage, as DamageSystem: armour first, then the next room (NextRoom); a reactor at 0 destroys the ship
    def take_slug(s, side, by, at=0.5):
        if s.dead: return
        if s.armour[side] > 0: s.armour[side] = max(0.0, s.armour[side] - by['slugArmourDamage']); return
        s.behind(side, int(by['slugCompartmentDamage']), 'slug', by, at)
    def take_he(s, side, by, at=0.5):
        if s.dead: return
        if s.armour[side] > 0: s.armour[side] = max(0.0, s.armour[side] - by['heArmourDamage']); return
        s.behind(side, int(by['slugCompartmentDamage']), 'HE', by, at)
    def take_run(s, side, disable, arm, comp):
        if s.dead: return
        i = (2 if s.room_row['eng'] - 0.5 > s.rows / 2 else 0) if disable else side   # DISABLE: the end the engine room is nearer
        if s.armour[i] > 0: s.armour[i] = max(0.0, s.armour[i] - arm); return
        s.behind(i, comp, 'fighter', None)
    def next_room(s, i, at=0.5):
        """DamageSystem.NextRoom (Step 186, the box): rooms in a row from bow to stern. PROW: the front room still standing;
        AFT: the back one; STBD or PORT: the room at the spot it landed (at: 0 the bow to 1 the stern; rows share the length
        equally), else the nearest still standing. Ties: more integrity, then list order. None when every room is wrecked."""
        left = [k for k in ROOMS if s.comp[k] > 0]
        if not left: return None
        if i == 0: key = lambda k: s.room_row[k]
        elif i == 2: key = lambda k: -s.room_row[k]
        else: key = lambda k: round(abs((s.room_row[k] - 0.5) / s.rows - at) * 10000)
        return min(left, key=lambda k: (key(k), -s.comp[k], ROOMS.index(k)))
    def behind(s, i, dmg, weapon, by, at=0.5):
        k = s.next_room(i, at)
        if k is None: return
        fc = 0 if by is None else by['fireChanceHE'] if weapon == 'HE' else 0
        if k not in s.vented and k not in s.fire and s.roll(fc): s.fire[k] = 0.0
        if weapon == 'slug' and k not in s.vented and s.roll(by['breachChanceSlug']): s.breach(k, s.dm['breachCrewLoss'])
        s.comp[k] = max(0, s.comp[k] - dmg)
        if s.comp['rct'] <= 0: s.dead = True
    def breach(s, k, share):
        s.vented.add(k); s.fire.pop(k, None); s.crew[k] -= math.ceil(s.crew[k] * share)
    def tick_conditions(s, dt):
        if s.dead: return
        d = s.dm
        pf, pv = 1 - (1 - d['fireSpreadPerSecond']) ** dt, 1 - (1 - d['vacuumSpreadPerSecond']) ** dt
        for k in list(s.fire):
            for m in NEIGH[k]:
                if m not in s.fire and m not in s.vented and s.roll(pf): s.fire[m] = 0.0
        for k in [k for k in s.vented if k not in s.shut]:
            for m in NEIGH[k]:
                if m not in s.vented and s.roll(pv): s.breach(m, d['breachCrewLoss'])
        for k in list(s.fire):
            if k in s.vented: del s.fire[k]; continue
            s.fire[k] += dt
            while s.fire[k] >= d['fireDamageSeconds']: s.fire[k] -= d['fireDamageSeconds']; s.comp[k] = max(0, s.comp[k] - 1)
        if s.comp['rct'] <= 0: s.dead = True
    def tick_party(s, dt):
        if not s.party: return
        k, t = s.party; t += dt
        if t < s.dm['partySeconds'] - 1e-4: s.party = (k, t); return
        if k in s.fire: del s.fire[k]
        elif k in s.vented: s.vented.discard(k); s.shut.discard(k)
        else: s.comp[k] = min(5, s.comp[k] + CODE['party_rate'])
        s.party = None
    def hp(s): return sum(s.armour) + sum(s.comp.values())
    def damage_taken(s): return sum(float(a) for a in s.dm['sideArmourMax']) - sum(s.armour) + 5 * len(ROOMS) - sum(s.comp.values())

# ---------------------------------------------------------------- shots in flight (WeaponSystem rounds)

class Round:
    __slots__ = ('x', 'y', 'dx', 'dy', 'speed', 'gone', 'maxu', 'by', 'torp', 'coast', 'motor', 'turn', 'target', 'tried', 'shooter', 'fired', 'cf', 'nose', 'wait')
    def __init__(r, x, y, b, speed, maxu, by, torp=False, target=None, shooter=None, fired=0.0, cf=None):
        r.x, r.y, r.dx, r.dy = x, y, math.sin(math.radians(b)), math.cos(math.radians(b))
        r.speed, r.gone, r.maxu, r.by, r.torp, r.target, r.shooter, r.fired, r.cf = speed, 0.0, maxu, by, torp, target, shooter, fired, cf
        r.coast, r.motor, r.turn, r.tried, r.nose, r.wait = 0.0, False, 0.0, 0, b, 0.0

def slug(x, y, b, speed, vx, vy, maxu, by, **kw):
    """WeaponSystem.Slug (owner 2026-11-08): a slug leaves on bearing b at its speed, plus the firing ship's motion."""
    r = Round(x, y, b, speed, maxu, by, **kw); sx, sy = r.dx * speed + vx, r.dy * speed + vy; v = math.hypot(sx, sy)
    r.dx, r.dy, r.speed = sx / v, sy / v, v; return r

def margin(by): return 1 + by['hitMarginPercent'] / 100

def slug_step(r, dt, targets):
    """One tick of a slug: the first ship box its whole path reaches (against each ship's own motion), or None; 'miss' at its reach."""
    step = min(r.speed * dt, r.maxu - r.gone); flight = step / r.speed; best = None
    for t in targets:
        if t.dead or t.captured: continue
        fx, fy = r.x - t.x, r.y - t.y; mx, my = (r.dx * r.speed - t.vx) * flight, (r.dy * r.speed - t.vy) * flight
        m = margin(r.by); e = enters_box(fx, fy, fx + mx, fy + my, t.hdg, t.half[0] * m, t.half[1] * m)
        if e and (best is None or e[0] < best[0]): best = (e[0], t, e[1], e[2])
    if best: return best[1], best[2], best[3]
    r.x += r.dx * step; r.y += r.dy * step; r.gone += step
    return 'miss' if r.gone >= r.maxu - 1e-6 else None

def torp_launch(by, x, y, heading, arc, i, n, maxu, target, shooter, now):
    """WeaponSystem.NewTorpedo, without the drawn launch line: out of the side, tilted toward the stern, fanned."""
    side = (1 if i % 2 == 0 else -1) if arc % 180 == 0 else (1 if arc % 360 < 180 else -1)
    fan = (i - (n - 1) / 2) * by['torpedoSpreadDeg']
    return Round(x, y, heading + side * (90 + by['torpedoTiltBackDeg']) + fan, by['torpedoLaunchSpeedUnits'], maxu, by, True, target, shooter, now)

def torp_step(r, dt, target):
    """A torpedo for one tick, in 0.05 s steps (owner 2026-11-10, drop, drift, flare): wait its turn in a ripple salvo; drift cold, slowing fast at
    first, while its nose swings onto the target; then the motor lights and it flies where its nose points, homing. Returns (ship, side, spot along it), None or 'miss'."""
    by, gone = r.by, 0.0
    while gone < dt - 1e-9:
        h = min(dt - gone, CODE['torpedo_step_s']); gone += h
        if r.wait > 0: r.wait -= h; continue   # ponytail: it waits at its launch spot; the game lets it out where its launcher is by then
        if not r.motor:   # WeaponSystem.Drift
            if r.coast >= by['torpedoCoastSeconds']: r.motor = True; hd = math.radians(r.nose); r.dx, r.dy = math.sin(hd), math.cos(hd)   # WeaponSystem.Light
            else:
                r.coast = min(by['torpedoCoastSeconds'], r.coast + h); left = 1 - r.coast / by['torpedoCoastSeconds']
                r.speed = by['torpedoCoastSpeedUnits'] + (by['torpedoLaunchSpeedUnits'] - by['torpedoCoastSpeedUnits']) * left * left
        if target is not None and not target.dead:   # WeaponSystem.SteerAt: cold, only the nose turns
            now = math.degrees(math.atan2(r.dx, r.dy)) if r.motor else r.nose; off = delta(now, brg(target.x - r.x, target.y - r.y))
            most = by['torpedoTurnDegPerS'] if r.motor else by['torpedoCurveDegPerS']; build = max(0.01, by['torpedoTurnBuildSeconds'])
            r.turn += clamp(clamp(off / build, -most, most) - r.turn, -most / build * h, most / build * h)
            head = now + r.turn * h
            if r.motor: hd = math.radians(head); r.dx, r.dy = math.sin(hd), math.cos(hd)
            else: r.nose = head
        if r.motor: r.speed = min(by['torpedoSpeedUnits'], r.speed + by['torpedoBurnUnits'] * h)   # WeaponSystem.Burn
        step = min(r.speed * h, r.maxu - r.gone)
        if target is not None and not target.dead:
            fx, fy = r.x - target.x, r.y - target.y; m = margin(by)
            e = enters_box(fx, fy, fx + r.dx * step - target.vx * h, fy + r.dy * step - target.vy * h, target.hdg, target.half[0] * m, target.half[1] * m)
            if e: return target, e[1], e[2]
        r.x += r.dx * step; r.y += r.dy * step; r.gone += step
        if r.gone >= r.maxu - 1e-6: return 'miss'
    return None

# ---------------------------------------------------------------- the enemy's mind (EnemyBrain, as the game has it today)

COLD, UNEASY, SEARCHING, ALERT = range(4)

class Brain:
    def __init__(b, ship, cfg, spawn, world, rnd):
        b.s, b.c, b.w, b.rnd = ship, cfg, world, rnd
        b.hunts = b.fires = spawn['hunts']
        b.cruise = spawn['cruise'] if spawn['cruise'] >= 0 else cfg['cruiseUnits']
        b.route, b.route_at, b.route_way = spawn['route'] or None, 0, 1
        b.signal_s, b.first_signal, b.on_air = spawn['signal'], spawn['first_signal'], spawn['on_air']
        b.level, b.started, b.health = COLD, False, -1.0
        b.waypoint, b.waypoint_at, b.last_heard, b.last_fix, b.last_known = (0.0, 0.0), -1e9, -1e9, -1e9, (0.0, 0.0)
        b.next_salvo, b.next_shot, b.next_signal, b.next_course, b.hears_us = 0.0, {}, 0.0, 0.0, False
        b.alert_at = None
    def raise_to(b, lv, now):
        if lv > b.level: b.level = lv
        if b.level == ALERT and b.alert_at is None: b.alert_at = now
    def heard(b, where, now, fix): b.last_known, b.last_heard = where, now; b.last_fix = now if fix else b.last_fix
    def off(b, r):
        a, rr = b.rnd.random() * 2 * math.pi, r * math.sqrt(b.rnd.random()); return math.sin(a) * rr, math.cos(a) * rr
    def has_fix(b, now): return now - b.last_fix <= b.c['fixSeconds']
    def reach(b):
        r = max([b.s.mounts[i]['rangeUnits'] for i in b.s.mounts if b.s.mounts[i]['kind'] == 'RG' and b.s.ammo[i] > 0] or [0])
        return r if r > 0 else CODE['no_ammo_reach']
    def think(b, us, fighters, sim, now):
        s = b.s; at = (s.x, s.y); dist = math.hypot(us.x - s.x, us.y - s.y)
        if not b.started:
            b.started, b.home, b.started_at = True, at, now; b.next_signal = now + b.first_signal; b.new_hunt_point(us)
        if b.signal_s > 0 and now >= b.next_signal:     # a radio message: its course changes halfway to the next (EnemyBrain.Send)
            sim.message(b, now); b.next_signal = now + b.on_air + b.signal_s; b.next_course = now + b.signal_s / 2
        hp = s.hp(); hit = b.health >= 0 and hp < b.health; b.health = hp      # any loss of hull counts as a hit (fire too)
        b.hears_us = False
        if b.hunts and not us.dead: b.listen(us, fighters, dist, now, hit)
        fix = b.level == ALERT and b.has_fix(now)
        if not s.mag_ok() and not s.party: s.party = ('mag', 0.0)
        if b.fires and not us.dead and s.mag_ok():
            vls = next((m for m in s.wp['mounts'] if m['kind'] == 'MS' and s.ammo[m['id']] > 0 and dist <= m['rangeUnits'] and m['id'] in s.dots), None)
            if (hit or fix) and now >= b.next_salvo and vls:
                n = min(int(b.c['fireBackSalvo']), s.ammo[vls['id']]); gx, gy = s.gun_at(vls['id'])
                for i in range(n):   # WeaponSystem.LaunchAt: a ripple, one at a time
                    r = torp_launch(s.wp, gx, gy, s.hdg, int(vls['arc']), i, n, vls['rangeUnits'], us, s, now); r.wait = i * s.wp['torpedoRippleSeconds']; sim.incoming.append(r)
                s.ammo[vls['id']] -= n; sim.m['torps_in'] += n; b.next_salvo = now + (b.c['salvoSeconds'] if fix else b.c['fireBackSeconds']); sim.event(now, 'torpedoes in')
            if fix:
                for mid, m in s.mounts.items():
                    if m['kind'] == 'RG': b.shoot(us, mid, m, dist, now, sim)
        b.steer(us, dist, now, fix, sim)
    def listen(b, us, fighters, dist, now, hit):
        s, st = b.s, b.s.st['sensors']; gain = b.c['gain'][min(b.level, len(b.c['gain']) - 1)]
        reach = st['senRangeBaseUnits'] + gain * st['senRangePerStepUnits']
        key = int(now)
        if getattr(b, '_ik', None) != key: b._ik, b._iv = key, b.w.interference(s.x, s.y, us.x, us.y)
        interf, on_line = b._iv
        eyes = 0 if on_line else min(st['visualRangeUnits'], us.st['sensors']['cloudSeenUnits']) if us.in_cloud else st['visualRangeUnits']
        if dist <= eyes:
            b.raise_to(ALERT, now); b.heard((us.x, us.y), now, True); b.hears_us = True; return
        hear = max(0, reach - interf) * us.signal / 100
        clear = 0 if hear <= 0 or dist > hear else 1 - dist / hear
        b.hears_us = clear > 0
        if hit: b.raise_to(ALERT, now)
        if clear > 0:
            twice = CODE['twice_window'][0] < now - b.last_heard <= CODE['twice_window'][1]
            b.raise_to(ALERT if clear >= b.c['alertAt'] else SEARCHING if clear >= b.c['searchingAt'] or twice else UNEASY, now)
            ox, oy = b.off(dist * (1 - clear) * CODE['heard_guess_share']); b.heard((us.x + ox, us.y + oy), now, b.level == ALERT)
        elif hit: ox, oy = b.off(dist * CODE['hit_guess_share']); b.heard((us.x + ox, us.y + oy), now, False)
        for f in fighters:
            fd = math.hypot(f.x - s.x, f.y - s.y); fi, _ = b.w.interference(s.x, s.y, f.x, f.y)
            fh = max(0, reach - fi) * b.c['fighterSignal'] / 100; c = 0 if fh <= 0 or fd > fh else 1 - fd / fh
            if c <= 0 or clear > 0: continue
            b.raise_to(SEARCHING if c >= b.c['searchingAt'] else UNEASY, now); b.heard((f.x, f.y), now, False)
    def shoot(b, us, mid, m, dist, now, sim):
        s = b.s
        if s.ammo[mid] <= 0 or now < b.next_shot.get(mid, 0) or dist > m['rangeUnits'] or mid not in s.dots: return
        rvx, rvy = us.vx - s.vx, us.vy - s.vy
        t = intercept(us.x - s.x, us.y - s.y, rvx, rvy, s.wp['slugSpeedUnits'])
        lead = brg(us.x - s.x + rvx * t, us.y - s.y + rvy * t) if t else brg(us.x - s.x, us.y - s.y)
        rel = jsround((lead - s.hdg) % 360) % 360
        if angdiff(rel, m['arc']) > m['halfArc']: return
        gx, gy = s.gun_at(mid)
        r = slug(gx, gy, lead + (b.rnd.random() * 2 - 1) * b.c['aimErrorDeg'], s.wp['slugSpeedUnits'], s.vx, s.vy, m['rangeUnits'], s.wp, shooter=s, fired=now,
                 cf=(us.x, us.y, us.vx, us.vy, us.hdg))
        sim.incoming.append(r); s.ammo[mid] -= 1; sim.their_shots += 1; sim.event(now, 'slug in')
        b.next_shot[mid] = now + b.c['railgunReloadSeconds'] + b.rnd.random() * b.c['railgunJitterSeconds']
    def steer(b, us, dist, now, fix, sim):
        s, c, w = b.s, b.c, b.w; top = s.nv['topSpeedUnits']; speed = b.cruise
        if not b.hunts and b.cruise <= 0: s.thr = 0; return
        if b.level > COLD and now - b.last_heard > c['giveUpSeconds']:
            b.level, fix, b.waypoint_at = COLD, False, now
            if b.route: b.back_to_route()
            else: b.new_hunt_point(us)
        if fix:
            bb = ibrg(us.x - s.x, us.y - s.y)
            if dist > c['fightAtShare'] * b.reach(): heading, speed = bb, top
            else: heading, speed = b.side_on(bb), top * c['fightSpeedShare']
        else:
            news = b.level >= UNEASY and b.last_heard > b.waypoint_at
            new_course = not b.route and b.level == COLD and b.next_course > 0 and now >= b.next_course
            if news: b.waypoint, b.waypoint_at = b.last_known, now
            elif math.hypot(b.waypoint[0] - s.x, b.waypoint[1] - s.y) <= c['arriveUnits'] or new_course:
                if b.level >= SEARCHING: ox, oy = b.off(c['searchUnits']); b.waypoint = (b.last_known[0] + ox, b.last_known[1] + oy)
                else: b.new_hunt_point(us)
                b.waypoint_at, b.next_course = now, 0.0
            b.waypoint = w.inside(*b.waypoint)
            heading = ibrg(b.waypoint[0] - s.x, b.waypoint[1] - s.y)
            speed = top if b.level == ALERT else top * CODE['searching_speed_share'] if b.level == SEARCHING else b.cruise
        heading = w.clear_heading(s, heading, c['avoidLookUnits'], [o for o in sim.ships if o is not s and not o.dead])
        s.ord = w.keep_inside(s.x, s.y, heading)
        s.thr = clamp(jsround(speed / top * 100), 0, s.throttle_cap())
    def side_on(b, bb):
        best, turn = bb, 999
        for mid, m in b.s.mounts.items():
            if m['kind'] != 'RG' or b.s.ammo[mid] <= 0: continue
            h = (bb - int(m['arc'])) % 360; t = angdiff(h, b.s.hdg)
            if t < turn: turn, best = t, h
        return best
    def new_hunt_point(b, us):
        if b.route: b.next_on_route(); return
        hw, hh = b.w.hw * CODE['hunt_area'][0], b.w.hh * CODE['hunt_area'][0]
        drift = clamp((b.s.now - b.started_at) / b.c['driftSeconds'], 0, 1) if b.c['driftSeconds'] > 0 else 1
        side = 1 if b.home[1] >= 0 else -1
        centre = side * hh * CODE['hunt_area'][1] + (-side * hh * CODE['hunt_area'][1] - side * hh * CODE['hunt_area'][1]) * drift
        for _ in range(8):
            b.waypoint = ((b.rnd.random() * 2 - 1) * hw, clamp(centre + (b.rnd.random() * 2 - 1) * hh * CODE['hunt_area'][1], -hh, hh))
            if math.hypot(b.waypoint[0] - b.s.x, b.waypoint[1] - b.s.y) >= b.c['legMinUnits']: break
    def next_on_route(b):
        if b.rnd.random() < b.c['turnBackChance']: b.route_way = -b.route_way
        if not 0 <= b.route_at + b.route_way < len(b.route): b.route_way = -b.route_way
        b.route_at += b.route_way; b.waypoint = b.route[b.route_at]
    def back_to_route(b):
        d = lambda p: (p[0] - b.s.x) ** 2 + (p[1] - b.s.y) ** 2
        for i, p in enumerate(b.route):
            if d(p) < d(b.route[b.route_at]): b.route_at = i
        b.waypoint = b.route[b.route_at]

# ---------------------------------------------------------------- the person at the board

# Hands times are real seconds; at x5 each takes five times as many game seconds. arm is the team README's draft.
# Everything else is a guess, marked so in the report. Reasons for the other differences are in the report.
PLAYERS = {
    'new':    dict(arm=8, react=5, sight=6, scan=6, click=2.0, plot_gap=60, plot_err=40, setup=30, dc=None, gun='near', loud=True, weave=False, refresh=60, pdc=0, settle=False, lock_tries=2, port_seek=False, fresh=False, break_off=False, standoff=0.48),
    'good':   dict(arm=4, react=2, sight=3, scan=3, click=1.0, plot_gap=45, plot_err=25, setup=15, dc=8, gun='stbd', loud=False, weave=False, refresh=40, pdc=2, settle=True, lock_tries=0, port_seek=False, fresh=True, break_off='scanned', standoff=0.72),
    'expert': dict(arm=2, react=1, sight=1.5, scan=1.5, click=0.5, plot_gap=30, plot_err=15, setup=8, dc=4, gun='stbd', loud=False, weave=True, refresh=25, pdc=4, settle=True, lock_tries=0, port_seek=True, fresh=True, break_off=True, standoff=0.88),
}
OPENINGS = ('fighter', 'scan', 'fire', 'plot', 'close', 'quiet')
# power plans: ENG, WEP, SEN, AUX units (each unit is 10 signal)
POWER = dict(loud=(5, 2, 3, 0), search=(3, 0, 3, 0), fight=(3, 4, 3, 0), quiet=(1, 0, 2, 0), launch=(3, 0, 3, 1), run=(5, 0, 3, 0), sneak=(2, 0, 3, 0))
PDC_ORDER = ('PDA', 'PDD', 'PDB', 'PDC')
DECODE = dict(new=30, good=15, expert=8)   # real seconds to lock and decode a radio message (guess)
FIGHT_RANGE = 1500   # inside the Garuda's 0.8 x 2,000, so it turns side-on instead of charging

class Fighter:
    def __init__(f, x, y, n, order, target): f.x, f.y, f.n, f.order, f.target, f.next_run, f.attacking, f.aboard, f.board_t = x, y, n, order, target, 0.0, False, False, 0.0

class Player:
    def __init__(p, sim, kind, opening, rate):
        p.sim, p.k, p.op, p.rate = sim, PLAYERS[kind], opening, rate
        p.kind = kind
        p.busy_until, p.job = 0.0, None
        p.mode, p.power = 'setup', 'search'
        p.contact_t = p.know_t = None
        p.scan = None; p.marks = []; p.ruler = None; p.course = None
        p.active = False
        rg = [m['id'] for m in sim.us.wp['mounts'] if m['kind'] == 'RG']
        p.gun = rg[0]; p.bar = dict.fromkeys(rg, 0.0)
        p.fixed = sim.us.mounts[p.gun]['halfArc'] == 0          # a spinal railgun (WeaponSystem.FixedGun): aimed by turning the ship
        sn = sim.us.st['sensors']; p.sen = p.k.get('sen', 3)
        p.engage = p.k['standoff'] * (sn['senRangeBaseUnits'] + p.sen * sn['senRangePerStepUnits']) if p.fixed else 2300   # where the fight starts: a share of our DRADIS reach
        p.lock_reach = CODE['scope_zoom'][1] * sim.B['board']['scopeFitUnits'] / (1 - sim.B['board']['scopeFitSlack'])     # ScopeFits at MAG 8
        p.hand_aim = None; p.aim_ready = False; p.aim_ready_t = 0.0; p.locked_once = False
        p.sighted = None; p.lock_fill = 0.0; p.lock_left = 0.0; p.locking = False; p.lock_fails = 0
        p.sol = None; p.armed = False; p.use_track = False
        p.charge = 0.0; p.pdc = set(PDC_ORDER[:p.k['pdc']])
        p.hold_t = None; p.boats = False; p.fighters_out = False
        p.weave_t = 0.0; p.decoded = None; p.msg = None; p.path = None; p.path_i = 0; p.plan_t = None
        p.dc_t = None; p.breaking = False; p.missed = False
    def g(p, real): return real * p.rate                     # hands time in game seconds
    def hands(p, real, fn, now, what):
        if now < p.busy_until: return False
        p.busy_until, p.job = now + p.g(real), (fn, what); p.sim.event(now, what); return True
    def finish_job(p, now):
        if p.job and now >= p.busy_until:
            fn, what = p.job; p.job = None; fn(now); p.sim.event(now, what + ' done')
    def free(p, now): return now >= p.busy_until

    # what our sensors give
    def dradis_range(p, us): st = us.st['sensors']; return st['senRangeBaseUnits'] + min(p.sim.units[2], 5) * st['senRangePerStepUnits']
    def heard(p, e):
        us = p.sim.us
        if p.sim.units[2] < 1: return False
        d = math.hypot(e.x - us.x, e.y - us.y); i, _ = p.sim.interf_us(e)
        return d <= p.dradis_range(us) - i
    def in_sight(p, e):
        us = p.sim.us; d = math.hypot(e.x - us.x, e.y - us.y); _, on = p.sim.interf_us(e)
        if not on and d <= us.st['sensors']['visualRangeUnits']: return True
        return any(math.hypot(f.x - e.x, f.y - e.y) <= us.st['sensors']['visualRangeUnits'] for f in p.sim.fighters)

    def mark(p, e, now):
        err = p.k['plot_err'] if p.in_sight(e) else p.k['plot_err'] * 1.5   # a DRADIS dot is coarser than the ship in sight (guess)
        p.marks.append((e.x + p.sim.rnd.gauss(0, err), e.y + p.sim.rnd.gauss(0, err), now, e.hdg)); p.missed = False
        if len(p.marks) >= 2:
            (ax, ay, at, _), (bx, by, bt, _) = p.marks[-2], p.marks[-1]
            if bt > at: p.ruler = (brg(bx - ax, by - ay), math.hypot(bx - ax, by - ay) / (bt - at), bt)
    def know(p, now, how):
        if p.know_t is None: p.know_t = now; p.sim.m['scan_s'] = now
    def est(p, now):
        """Where we think it is going: (x, y, vx, vy) now, from the newest of the scan and the plot."""
        e = p.sim.enemy
        src = None
        if p.scan and (not p.ruler or p.scan[2] >= p.ruler[2]): src = p.scan
        elif p.ruler: src = p.ruler
        if not src: return None
        c, v = math.radians(src[0]), src[1]
        if src is p.ruler:
            x, y, t, _ = p.marks[-1]; return x + math.sin(c) * v * (now - t), y + math.cos(c) * v * (now - t), math.sin(c) * v, math.cos(c) * v
        return e.x, e.y, math.sin(c) * v, math.cos(c) * v       # a scan reads course and speed; DRADIS gives where it is

    # power and signal
    def set_power(p, plan): p.power = plan
    def tick_power(p, dt, now):
        sim, us = p.sim, p.sim.us; want = list(POWER[plan_of(p)])
        if p.sen != 3 and want[2] == 3:      # more SEN: from spare units, then WEP down to 2, then ENG
            want[2] = p.sen; extra = sum(want) - us.st['power']['reactorUnits']
            while extra > 0 and want[1] > 2: want[1] -= 1; extra -= 1
            while extra > 0 and want[0] > 1: want[0] -= 1; extra -= 1
        if sim.lv['weapons_off']: want[1] = 0; want[3] = 0
        sim.bank += dt
        while sim.bank >= 1 - 1e-9:
            sim.bank -= 1
            for i in range(4):
                if sim.units[i] > want[i]: sim.units[i] -= 1
            for i in range(4):
                if sim.units[i] < want[i] and sum(sim.units) < us.st['power']['reactorUnits']: sim.units[i] += 1
        us.eng = sim.units[0]
        sens = us.st['sensors']
        tgt = (sens['activeScanSignal'] if p.active or p.locking else 0) + sum(u * k for u, k in zip(sim.units, us.st['power']['busSignalPerUnit']))
        tgt = clamp(tgt, 0, 100) * (sens['cloudSignalShare'] if us.in_cloud else 1)
        us.signal = min(tgt, us.signal + sens['signalRise'] * dt) if us.signal < tgt else max(tgt, us.signal - sens['signalFall'] * dt)
        p.charge = min(100.0, p.charge + sim.units[1] * us.wp['wepFeedPerUnit'] * dt) if sim.units[1] > 0 else max(0.0, p.charge - us.wp['wepBleed'] * dt)
    def wep_ready(p): return p.sim.units[1] >= 2 and p.sim.us.mag_ok() and not p.sim.lv['weapons_off']

    # the lock: sight, fill, hold (WeaponSystem.TickLock)
    def tick_lock(p, dt, now):
        sim, us, e = p.sim, p.sim.us, p.sim.enemy; B = sim.B['board']; wp = us.wp
        d = math.hypot(e.x - us.x, e.y - us.y)
        fits = p.sighted is not None and abs(d - p.sighted) <= B['scopeFitSlack'] * p.sighted and p.heard(e) and not e.dead and d <= p.lock_reach
        if p.lock_left > 0:
            p.lock_left -= dt
            if p.lock_left > 0: p.locked_once = True
            if p.lock_left <= 0 or not p.heard(e) or e.dead:
                if p.fixed and us.on_aim and not e.dead and p.g(p.k['arm']) <= 5: p.aim_ready, p.aim_ready_t = True, now   # quick hands: fire on the aim the lock left
                p.lock_left = 0; p.lock_fill = 0; p.locking = False; p.sol = None
                if p.job and p.job[1] in ('arm', 'arm torpedo'): p.job = None; p.busy_until = now; p.lock_fails += 1; sim.event(now, 'lock lost while arming')
                p.armed = False; sim.event(now, 'lock dropped')
            else: p.sol = p.lock_solution(now)
            return
        if fits and p.locking:
            p.lock_fill = min(1.0, p.lock_fill + dt / wp['lockSeconds'])
            if p.lock_fill >= 1: p.lock_left = wp['lockHoldSeconds']; p.sol = p.lock_solution(now); sim.event(now, 'locked')
        elif p.locking:
            p.lock_fill = max(0.0, p.lock_fill - dt / wp['lockFallSeconds'])
            if p.lock_fill <= 0: p.locking = False
    def lock_solution(p, now):
        e = p.sim.enemy; src = None
        plot_at = p.ruler[2] if p.ruler else -1
        if p.scan and p.scan[2] >= plot_at: src = p.scan
        elif p.ruler: src = p.ruler
        return dict(x=e.x, y=e.y, t=now, course=src[0] if src else 0, spd=src[1] if src else 0, lock=True)
    def track_solution(p, now):
        x, y, t, _ = p.marks[-1]; c, v, _ = p.ruler
        return dict(x=x, y=y, t=t, course=c, spd=v, lock=False)
    def lead(p, now, speed):
        us, s = p.sim.us, p.sol; c = math.radians(s['course'])
        tx = s['x'] + math.sin(c) * s['spd'] * (now - s['t']) - us.x; ty = s['y'] + math.cos(c) * s['spd'] * (now - s['t']) - us.y
        if s['spd'] <= 0.001: return brg(tx, ty)
        vx, vy = math.sin(c) * s['spd'] - us.vx, math.cos(c) * s['spd'] - us.vy     # its motion seen from us: the slug keeps ours (LeadBearing)
        t = intercept(tx, ty, vx, vy, speed)
        return brg(tx + vx * t, ty + vy * t) if t else brg(tx, ty)
    def tick_guns(p, dt, now):
        us = p.sim.us
        if p.fixed:       # WeaponSystem.TickGuns: while locked, the ship turns itself onto the lead; else the person's own turn, or none
            us.aim = (p.lead(now, us.wp['slugSpeedUnits']) - us.mounts[p.gun]['arc']) % 360 if p.sol and p.lock_left > 0 else p.hand_aim
            return
        for mid in p.bar:
            if mid not in us.mounts: continue
            m = us.mounts[mid]; want = 0.0
            if mid == p.gun and (p.sol or p.locking):
                lb = p.lead(now, us.wp['slugSpeedUnits']) if p.sol else brg(p.sim.enemy.x - us.x, p.sim.enemy.y - us.y)
                want = clamp(delta(us.hdg + m['arc'], lb), -m['halfArc'], m['halfArc'])
            step = us.wp['gunTurnDegPerSecond'] * dt
            p.bar[mid] = want if abs(want - p.bar[mid]) <= step else p.bar[mid] + math.copysign(step, want - p.bar[mid])
    def in_arc(p, now):
        us, m = p.sim.us, p.sim.us.mounts[p.gun]
        if p.fixed: return True
        if not p.sol: return False
        c = math.radians(p.sol['course']); dt_ = now - p.sol['t']
        x, y = p.sol['x'] + math.sin(c) * p.sol['spd'] * dt_, p.sol['y'] + math.cos(c) * p.sol['spd'] * dt_
        rel = (jsround(brg(x - us.x, y - us.y)) - us.hdg + 540) % 360 - 180
        return angdiff(rel % 360, int(m['arc'])) <= m['halfArc']
    def settled(p, now):
        us, m = p.sim.us, p.sim.us.mounts[p.gun]
        if p.fixed:       # the nose is on the aim, and (careful players) the ship isn't sliding sideways
            h = math.radians(us.hdg); side = abs(us.vx * math.cos(h) - us.vy * math.sin(h))
            return (us.aim is None or us.on_aim) and side < 1
        return abs(p.bar[p.gun] - clamp(delta(us.hdg + m['arc'], p.lead(now, us.wp['slugSpeedUnits'])), -m['halfArc'], m['halfArc'])) < 1.5
    def fire(p, now):
        sim, us = p.sim, p.sim.us; m = us.mounts[p.gun]
        if not ((p.sol or p.fixed) and p.wep_ready() and us.ammo[p.gun] > 0 and p.charge >= sim.B['shot']['RG'] and p.in_arc(now)): return False
        p.charge -= sim.B['shot']['RG']; us.ammo[p.gun] -= 1; p.armed = False
        sim.m['shots_lock' if p.lock_left > 0 or p.aim_ready else 'shots_hand'] += 1
        gx, gy = us.gun_at(p.gun)
        sim.ours.append(slug(gx, gy, us.true_hdg() + m['arc'] + p.bar[p.gun], us.wp['slugSpeedUnits'], us.vx, us.vy, m['rangeUnits'], us.wp, shooter=us, fired=now))
        us.signal = min(100.0, us.signal + us.wp['fireSignalSpike']); sim.m['shots'] += 1; sim.event(now, 'shot')
        if sim.m['first_shot_s'] is None: sim.m['first_shot_s'] = now
        return True
    def fire_torps(p, now):
        sim, us, e = p.sim, p.sim.us, p.sim.enemy
        rel = (ibrg(e.x - us.x, e.y - us.y) - us.hdg) % 360
        ls = [m for m in us.wp['mounts'] if m['kind'] == 'MS' and us.ammo[m['id']] > 0 and m['id'] in us.dots]
        if not ls or not p.sol or not p.wep_ready() or p.charge < sim.B['shot']['MS'] or not (p.scan or p.lock_left > 0): return False
        m = min(ls, key=lambda m: angdiff(rel, int(m['arc'])))
        # WeaponSystem.Fire (owner 2026-11-10, ripple salvo): one at a time, from the target's side and the other launcher by turns
        o = next((l for l in ls if l is not m), None); order, left, more = [], us.ammo[m['id']], (us.ammo[o['id']] if o else 0)
        while len(order) < int(us.wp['salvo']) and left + more > 0:
            nxt = more > 0 and (len(order) % 2 == 1 or left == 0); order.append(o if nxt else m)
            if nxt: more -= 1
            else: left -= 1
        n, nm, km, ko = len(order), sum(1 for l in order if l is m), 0, 0
        for i, l in enumerate(order):
            if l is m: k, of, km = km, nm, km + 1
            else: k, of, ko = ko, n - nm, ko + 1
            gx, gy = us.gun_at(l['id'])
            r = torp_launch(us.wp, gx, gy, us.hdg, int(l['arc']), k, of, l['rangeUnits'], e, us, now); r.wait = i * us.wp['torpedoRippleSeconds']; sim.ours.append(r)
            us.ammo[l['id']] -= 1
        p.charge -= sim.B['shot']['MS']; us.signal = min(100.0, us.signal + us.wp['fireSignalSpike'])
        sim.m['shots'] += n; sim.event(now, 'torpedoes out'); p.armed = False
        if sim.m['first_shot_s'] is None: sim.m['first_shot_s'] = now
        return True

    # the helm
    def steer_to(p, x, y, thr):
        us = p.sim.us; h = ibrg(x - us.x, y - us.y)
        us.ord = p.sim.world.clear_heading(us, h, 800, [o for o in p.sim.ships if o is not us and not o.dead])
        us.thr = clamp(thr, -100, us.throttle_cap())
    def broadside(p, e, now, rng=FIGHT_RANGE, thr=50):
        us = p.sim.us; d = math.hypot(e.x - us.x, e.y - us.y); b = brg(e.x - us.x, e.y - us.y)
        arc = int(us.mounts[p.gun]['arc']); adj = clamp((d - rng) / 300, -1, 1) * 35
        us.ord = jsround((b - arc + (adj if arc == 90 else -adj)) % 360) % 360
        if p.k['weave'] and p.sim.brain.fires: thr = 100 if (now // 8) % 2 == 0 else 20          # weave: full and slow by turns, 8 s each (guess)
        b_us = (brg(us.x - e.x, us.y - e.y) - e.hdg) % 360
        if p.k['port_seek'] and p.sim.brain.level == ALERT and (b_us < 60 or b_us > 300):
            # it is bow-on, about to turn a side to us: move toward its port side (its left), in reverse if we must
            pd = math.radians(e.hdg - 90); fw = math.radians(us.hdg)
            thr = 80 if math.sin(fw) * math.sin(pd) + math.cos(fw) * math.cos(pd) > 0 else -100
        us.thr = clamp(thr, -us.throttle_cap(), us.throttle_cap())
    def pick_gun(p, e):
        us = p.sim.us
        if p.fixed or len(p.bar) < 2: return
        if p.k['gun'] == 'stbd': p.gun = 'RGA'; return
        b = brg(e.x - us.x, e.y - us.y)
        p.gun = min(('RGA', 'RGB'), key=lambda g: angdiff(jsround(b - us.mounts[g]['arc']) % 360, us.hdg))

    # one tick of the person
    def tick(p, dt, now):
        sim, us, e, lv = p.sim, p.sim.us, p.sim.enemy, p.sim.lv
        p.finish_job(now)
        heard = p.heard(e) and not e.dead
        if heard and not p.was_heard: sim.event(now, 'contact')
        if not heard and p.was_heard: sim.event(now, 'contact lost')
        p.was_heard = heard
        if heard and sim.m['first_contact_s'] is None: sim.m['first_contact_s'] = now; sim.m['contact_d'] = math.hypot(e.x - us.x, e.y - us.y)
        if p.mode == 'setup':
            if p.free(now) and not p.setup_done: p.hands(p.k['setup'], lambda t: setattr(p, 'setup_done', True), now, 'power up')
            if p.setup_done: p.mode = 'search'; p.set_power('loud' if p.k['loud'] else 'search')
            return
        if p.dc_t is None or now >= p.dc_t: p.damage_control(now)
        goal = lv['goal']
        if goal == 'JumpPoint': return p.sneak(dt, now, heard)
        if goal == 'WolfHunt' and e.dead: return p.go_home(now)
        if p.mode == 'search':
            if heard: p.mode = 'react'; p.contact_t = now; p.react_until = now + p.g(p.k['react'])
            elif p.msg and p.decoded is not None and now >= p.decoded:     # the decoded bearing: fly down it
                b = math.radians(p.msg[0]); p.steer_to(us.x + math.sin(b) * 5000, us.y + math.cos(b) * 5000, 100)
            else: p.steer_to(e_guess(sim)[0], e_guess(sim)[1], 100)
            return
        if p.mode == 'react':
            if now >= p.react_until: p.mode = 'open'; p.open_t = now; p.opening(now)
            return
        if p.mode in ('open', 'approach', 'fight', 'hold'): p.play(dt, now, heard)

    def opening(p, now):
        sim, e, op, goal = p.sim, p.sim.enemy, p.op, p.sim.lv['goal']
        p.stage = 'start'
        if op == 'fire': p.set_power('fight')
        elif op == 'quiet': p.set_power('quiet')
        elif op == 'fighter': p.set_power('launch')
        else: p.set_power('loud' if p.k['loud'] else 'search')

    def play(p, dt, now, heard):
        sim, us, e, op, goal = p.sim, p.sim.us, p.sim.enemy, p.op, p.sim.lv['goal']
        d = math.hypot(e.x - us.x, e.y - us.y)
        if p.mode == 'open':
            if op == 'scan' and p.stage == 'start':
                if p.free(now):
                    p.active = True
                    def done(t):
                        p.active = False
                        if p.heard(e): p.scan = (brg(e.vx, e.vy) if e.speed() > 0.01 else e.hdg, e.speed(), t); sim.identified = True; p.know(t, 'scan')
                    p.hands(p.k['scan'], done, now, 'scan'); p.stage = 'scanning'
                return
            if op == 'scan' and p.stage == 'scanning':
                if p.free(now): p.mode = 'approach' if p.scan else 'fight'
                return
            if op == 'plot':
                if p.stage == 'start' and p.free(now): p.hands(p.k['click'], lambda t: p.mark(e, t), now, 'POINT'); p.stage = 'p1'; p.p1 = now
                elif p.stage == 'p1' and now - p.p1 >= p.g(p.k['click']) + p.k['plot_gap'] and p.free(now):
                    p.hands(p.k['click'] * 2, lambda t: (p.mark(e, t - p.g(p.k['click'])), p.know(t, 'plot')), now, 'POINT and RULER'); p.stage = 'p2'
                elif p.stage == 'p2' and p.free(now) and p.ruler: p.mode = 'approach'
                if p.stage != 'p2': p.steer_to(e.x, e.y, 30)
                return
            if op == 'fighter':
                if p.stage == 'start':
                    if not sim.lv['weapons_off'] and p.free(now):
                        p.hands(p.k['click'] * 8, lambda t: None, now, 'launch stages'); p.stage = 'stages'; p.launch_at = now + max(p.g(p.k['click'] * 8), sum(100 / st['rate'] for st in LAUNCH_STAGES))
                    us.thr = 0; return
                if p.stage == 'stages':
                    us.thr = 0
                    if now >= p.launch_at and sim.units[3] >= 1: sim.launch_fighters(now); p.stage = 'out'
                    return
                if p.stage == 'out':
                    us.thr = 0
                    close = [f for f in sim.fighters if p.in_sight(e)]
                    if close and p.know_t is None and p.free(now):
                        p.hands(p.k['click'], lambda t: p.mark(e, t), now, 'POINT (fighters see it)'); p.fp = now
                    if p.marks and p.know_t is None and now - p.marks[-1][2] >= p.k['plot_gap'] / 2 and p.free(now):
                        p.hands(p.k['click'] * 2, lambda t: (p.mark(e, t - p.g(p.k['click'])), p.know(t, 'fighters')), now, 'POINT and RULER')
                    if p.know_t is not None or d < 3500 or (sim.fighters and sim.fighters[0].attacking and now - p.open_t > 180):
                        p.mode = 'approach' if p.ruler else 'fight'; p.set_power('loud' if p.k['loud'] else 'search')
                    return
            if op == 'quiet':
                us.thr = 0
                if d <= 3000 or now - p.open_t >= 120 or sim.brain.level == ALERT: p.mode = 'fight'; p.set_power('fight')
                return
            if op == 'close':
                p.steer_to(e.x, e.y, 100)
                if d <= p.engage: p.mode = 'fight'; p.set_power('fight')
                return
            if op == 'fire':
                p.steer_to(e.x, e.y, 100)
                if d <= p.engage or goal in ('Destroy', 'Capture', 'HitUnseen'): p.mode = 'fight'
                return
            p.mode = 'fight'; return
        if p.mode == 'approach':
            est = p.est(now)
            if d <= p.engage + 300: p.set_power('fight')
            if est is None or sim.brain.level == ALERT or (d <= p.engage and not p.fixed): p.mode = 'fight'; p.set_power('fight'); return
            # a still target already showing its port side, in range: no need to go round (a moving one would pass and show its stern)
            shows_port = side_at(brg(us.x - e.x, us.y - e.y), e.hdg) == 3 and math.hypot(est[2], est[3]) < 3
            if p.fixed and ((d <= p.engage and shows_port) or p.port_point(now, est, d)): p.mode = 'fight'; p.set_power('fight')
            return
        # the fight
        if p.fixed: return p.nose_fight(now, d)
        if p.gun_picked is False: p.pick_gun(e); p.gun_picked = True
        if us.ammo[p.gun] <= 0 and len(p.bar) > 1 and us.ammo['RGB' if p.gun == 'RGA' else 'RGA'] > 0 and not p.k['break_off']:   # the port gun shows them our reactor
            p.gun = 'RGB' if p.gun == 'RGA' else 'RGA'; p.sighted = None; p.armed = False; sim.event(now, 'switch to ' + p.gun)
        knows = p.k['break_off'] is True or p.k['break_off'] == 'scanned' and sim.identified   # hit reports name the room only once scanned
        if knows and goal == 'WolfHunt':
            shows = side_at(brg(us.x - e.x, us.y - e.y), e.hdg)
            if not p.breaking and shows == 1 and e.comp['hgr'] <= 0 and e.armour[3] > 0 and p.sim.brain.level == ALERT:
                p.breaking = True; p.sol = None; p.armed = False; p.locking = False; p.lock_left = 0; p.sighted = None; sim.event(now, 'break off')
            if p.breaking:
                p.set_power('quiet' if d > 2000 else 'run')
                p.steer_to(us.x + (us.x - e.x), us.y + (us.y - e.y), 100)
                if now - p.sim.brain.last_fix > p.sim.brain.c['fixSeconds'] + 5 and d > 2600: p.breaking = False; p.set_power('fight'); sim.event(now, 'come back in')
                return
        if d > 2300: p.steer_to(e.x, e.y, 100)
        elif goal in ('Destroy', 'Capture', 'HitUnseen') and e.speed() < 1 and d < FIGHT_RANGE + 400:
            p.broadside(e, now, rng=d, thr=0)          # a still target in range: turn a gun onto it and stay
        else: p.broadside(e, now)
        if goal == 'Capture' and p.capture(now, d): return
        if goal == 'HitUnseen' and op == 'fire': return p.dial_cycle(now, d)
        if goal == 'HitUnseen': p.use_track = True
        if not p.use_track and p.arm_lock_g() + sim.dt >= sim.B_hold and p.lock_fails >= p.k['lock_tries']: p.use_track = True
        if p.use_track: p.track_cycle(now, d)
        else: p.lock_cycle(now, d)
    def arm_lock_g(p): return p.g(p.k['arm'])

    def torp_now(p):
        """Torpedoes: after a scan for torpedo-first players, or once the railgun is empty."""
        us = p.sim.us; left = any(us.ammo.get(m, 0) > 0 for m in ('MSA', 'MSB'))
        return left and (p.sim.torp_first and p.sim.identified or us.ammo[p.gun] <= 0)
    def port_point(p, now, est, d):
        """Head for a point off its port side (where its reactor is), as far off as the fight. True once there."""
        us, e = p.sim.us, p.sim.enemy; ex, ey, vx, vy = est; tau = d / max(20, us.nv['topSpeedUnits'] * us.eng_share())
        c = math.atan2(vx, vy) if vx * vx + vy * vy > 1 else math.radians(e.hdg); port = c - math.pi / 2
        off = p.engage if p.fixed else FIGHT_RANGE
        px, py = ex + vx * tau + math.sin(port) * off, ey + vy * tau + math.cos(port) * off
        if p.fixed and math.hypot(px - us.x, py - us.y) < 600: return True
        p.steer_to(px, py, 100); return False
    def nose_fight(p, now, d):
        """The spinal railgun (Step 170): come to the stand-off range, stop, let a lock turn the nose onto the lead, arm, fire.
        Careful players kill any sideways slide with Q and E first. NO PEEKING allows no lock: the nose is turned by hand."""
        sim, us, e, goal = p.sim, p.sim.us, p.sim.enemy, p.sim.lv['goal']
        torp = p.torp_now(); rng = 2600 if torp else p.engage
        us.null_slide = p.k['settle']
        # a player who reads the hit reports (scanned, or expert) won't shoot where nothing is left to hit: go round to its port side
        knows = p.k['break_off'] is True or p.k['break_off'] == 'scanned' and sim.identified
        side = side_at(brg(us.x - e.x, us.y - e.y), e.hdg); est = p.est(now)
        if knows and not torp and side != 3 and e.armour[side] <= 0 and e.next_room(side) is None and est and goal != 'Capture':
            if not p.breaking: p.breaking = True; sim.event(now, 'go round to its port side')
            p.armed = p.armed if p.armed == 'MS' else False; p.port_point(now, est, d); return
        p.breaking = False
        if d > rng + 200: p.steer_to(e.x, e.y, 100)
        else:
            us.thr = 0
            if us.aim is None and not p.aim_ready and angdiff(us.hdg, ibrg(e.x - us.x, e.y - us.y)) > 20: us.ord = ibrg(e.x - us.x, e.y - us.y)
        if goal == 'Capture' and p.capture(now, d): return
        if goal == 'HitUnseen': return p.dial_cycle(now, d) if p.op == 'fire' else p.track_cycle(now, d)
        if d > p.lock_reach and not torp and p.lock_left <= 0: p.use_track = True; return p.track_cycle(now, d)   # too far to lock: POINT, RULER, aim by hand
        p.nose_lock(now, d, torp)
    def nose_lock(p, now, d, torp):
        sim, us, e = p.sim, p.sim.us, p.sim.enemy
        if d > (2900 if torp else p.engage + 500) or not p.heard(e): return
        if p.aim_ready and not torp:          # the lock turned the nose and ran out: arm, and fire on that aim
            if not p.armed and p.free(now): p.hands(p.k['arm'], lambda t: setattr(p, 'armed', 'RG'), now, 'arm (lock gone)')
            if p.armed == 'RG' and (not p.k['settle'] or p.settled(now)) and p.fire(now): p.aim_ready = False
            if now - p.aim_ready_t > 60: p.aim_ready = False
            return
        if p.lock_left <= 0 and not p.armed and p.stale(now): return
        if (p.k['settle'] or p.lock_fails >= p.k['lock_tries']) and not torp and p.locked_once and not p.armed and p.lock_left <= 0:
            p.locking = False; p.lock_fill = 0.0          # off the sight while the hands arm: only a held lock's end clears the switches
            if p.free(now): p.hands(p.k['arm'], lambda t: setattr(p, 'armed', 'RG'), now, 'arm before the lock')
            return
        if p.sighted is None or abs(d - p.sighted) > sim.B['board']['scopeFitSlack'] * p.sighted:
            if p.free(now): p.hands(p.k['sight'], lambda t: (setattr(p, 'sighted', math.hypot(e.x - us.x, e.y - us.y)), setattr(p, 'locking', True)), now, 'sight')
            return
        if not p.locking and p.lock_left <= 0: p.locking = True
        if p.lock_left > 0 and p.free(now) and not p.armed:
            if p.k['settle'] and p.lock_left < p.g(p.k['arm']) + 1: return    # it won't fit: let the lock turn the nose, then arm
            p.hands(p.k['arm'], lambda t, w=('MS' if torp else 'RG'): setattr(p, 'armed', w), now, 'arm torpedo' if torp else 'arm')
        if p.armed == 'MS' and p.sol: p.fire_torps(now); return
        if p.armed == 'RG' and p.lock_left > 0 and (not p.k['settle'] or p.settled(now)): p.fire(now)
    def nose_by_hand(p, now, want):
        """The person turns the nose with Shift and A or D (2 a game second, read once a board tick), from a whole-degree order.
        They put the line on the spot to within their click error (guess), then can stop only on a whole tick of the turn."""
        us, e = p.sim.us, p.sim.enemy; d = max(1.0, math.hypot(e.x - us.x, e.y - us.y))
        want += math.degrees(p.sim.rnd.gauss(0, p.k['plot_err']) / d)
        step = us.nv.get('fineTurnDegPerS', 0) * p.sim.dt; whole = jsround(want)
        p.hand_aim = (whole + (step * round((want - whole) / step) if step > 0 else 0)) % 360

    def lock_cycle(p, now, d):
        sim, us, e = p.sim, p.sim.us, p.sim.enemy
        torp = p.sim.torp_first and p.sim.identified and any(us.ammo.get(m, 0) > 0 for m in ('MSA', 'MSB'))
        if d > (2900 if torp else 2300) or not p.heard(e): return
        if p.lock_left <= 0 and not p.armed and p.stale(now): return
        if p.sighted is None or abs(d - p.sighted) > sim.B['board']['scopeFitSlack'] * p.sighted:
            if p.free(now) and not p.armed: p.hands(p.k['sight'], lambda t: (setattr(p, 'sighted', math.hypot(e.x - us.x, e.y - us.y)), setattr(p, 'locking', True)), now, 'sight')
            return
        if not p.locking and p.lock_left <= 0: p.locking = True
        if p.lock_left > 0 and p.free(now) and not p.armed:
            wants_torp = torp
            if p.k['settle'] and p.lock_left < p.g(p.k['arm']): return     # not enough lock left: wait for the next
            p.hands(p.k['arm'], lambda t: setattr(p, 'armed', 'MS' if wants_torp else 'RG'), now, 'arm torpedo' if wants_torp else 'arm')
        if p.armed and p.sol:
            if p.armed == 'MS': p.fire_torps(now); return
            if (not p.k['settle'] or p.settled(now)) and p.fire(now): pass
    def dial_cycle(p, now, d):
        """Fire straight at it with no scan, lock or plot: dial BRG and RNG onto where it is now, speed 0 (WeaponSystem.NudgeSol)."""
        e = p.sim.enemy
        if d > (p.engage if p.fixed else 2000): return
        if p.fixed:       # a fixed gun has no BRG to dial: turn the nose onto it by eye
            us = p.sim.us
            if p.hand_aim is None and p.free(now): p.hands(p.k['sight'], lambda t: p.nose_by_hand(t, brg(e.x - us.x, e.y - us.y)), now, 'nose on by hand')
            elif p.hand_aim is not None and not p.armed and p.free(now): p.hands(p.k['arm'], lambda t: setattr(p, 'armed', 'RG'), now, 'arm')
            elif p.armed and (not p.k['settle'] or p.settled(now)) and p.fire(now): p.hand_aim = None
            return
        if p.sol is None and p.free(now): p.hands(p.k['click'] * 4, lambda t: setattr(p, 'sol', dict(x=e.x, y=e.y, t=t, course=0, spd=0, lock=False)), now, 'dial BRG and RNG')
        elif p.sol and not p.armed and p.free(now): p.hands(p.k['arm'], lambda t: setattr(p, 'armed', 'RG'), now, 'arm')
        elif p.armed and (not p.k['settle'] or p.settled(now)):
            if p.fire(now): p.sol = None
    def stale(p, now):
        """A good player knows an old course spoils a lock's lead: scan again, or ERASE an old plot."""
        if not p.k['fresh'] or not p.free(now): return False
        sc = p.scan and (not p.ruler or p.scan[2] >= p.ruler[2])
        if sc and now - p.scan[2] > p.k['refresh']:
            e = p.sim.enemy; p.active = True
            p.hands(p.k['scan'], lambda t: (setattr(p, 'active', False), p.heard(e) and setattr(p, 'scan', (brg(e.vx, e.vy) if e.speed() > 0.1 else e.hdg, e.speed(), t))), now, 'scan again')
            return True
        if not sc and p.ruler and now - p.ruler[2] > p.k['refresh']:
            p.hands(p.k['click'], lambda t: setattr(p, 'ruler', None), now, 'ERASE old plot'); return True
        return False
    def track_cycle(p, now, d):
        sim, us, e = p.sim, p.sim.us, p.sim.enemy
        if d > p.engage + (500 if p.fixed else 0): return
        refresh = max(p.k['refresh'], p.g(p.k['click'] * 3 + p.k['arm']) + 5)   # a person can't plot again faster than their hands
        # the plot is out of date once it has turned (the model shows its heading in sight) or it moves and the plot is old
        fresh = p.marks and not p.missed and angdiff(e.hdg, p.marks[-1][3]) <= 15 and (e.speed() < 2 or now - p.marks[-1][2] <= refresh)
        if not fresh or not p.ruler:
            if p.free(now) and (not p.marks or now - p.marks[-1][2] >= max(p.k['plot_gap'] / 3, p.g(p.k['click']) + 5)):
                p.hands(p.k['click'], lambda t: p.mark(e, t), now, 'POINT')
            if not p.ruler or not p.armed or not p.in_arc(now): return
        if p.sol is None or p.sol.get('t') != p.marks[-1][2]:
            if p.free(now): p.hands(p.k['click'] * 2, lambda t: (setattr(p, 'sol', p.track_solution(t)), setattr(p, 'armed', False), setattr(p, 'hand_aim', None)), now, 'RULER and TRACK')   # TRACK clears the ladder
            return
        if p.fixed and p.hand_aim is None:     # TRACK doesn't turn the ship: the person turns the nose onto the lead
            if p.free(now): p.hands(p.k['sight'], lambda t: p.sol and p.nose_by_hand(t, p.lead(t, us.wp['slugSpeedUnits'])), now, 'nose on by hand')
            return
        torp = p.sim.torp_first and p.sim.identified and any(us.ammo.get(m, 0) > 0 for m in ('MSA', 'MSB'))
        if not p.armed and p.free(now): p.hands(p.k['arm'], lambda t: setattr(p, 'armed', 'MS' if torp else 'RG'), now, 'arm')
        if p.armed == 'MS': p.fire_torps(now)
        elif p.armed and (not p.k['settle'] or p.settled(now)) and p.fire(now) and p.fixed: p.hand_aim = None

    def capture(p, now, d):
        """FINDERS KEEPERS: scan, open a side with the railgun, then the boats (HangarSystem.TickBoarding)."""
        sim, e = p.sim, p.sim.enemy
        if not sim.identified:
            if p.free(now):
                p.active = True
                p.hands(p.k['scan'], lambda t: (setattr(p, 'active', False), setattr(sim, 'identified', True), p.know(t, 'scan')), now, 'scan')
            return True
        if any(a <= 0 for a in e.armour):
            p.sol = None; p.armed = False; p.locking = False; p.lock_left = 0
            if not p.boats and p.free(now):
                p.set_power('launch'); p.hands(p.k['click'] * 8, lambda t: None, now, 'boat stages'); p.boats = True
                p.boat_at = now + max(p.g(p.k['click'] * 8), sum(100 / st['rate'] for st in LAUNCH_STAGES))
            if p.boats and not sim.boats and now >= getattr(p, 'boat_at', 1e18) and sim.units[3] >= 1: sim.launch_boats(now)
            return True
        return False

    def damage_control(p, now):
        us = p.sim.us
        if p.k['dc'] is None or us.party: return
        # fires first, then seal airless rooms, then air back into sealed ones, then repair the rooms the fight needs
        need = list(us.fire) + [k for k in us.vented if k not in us.shut] + [k for k in us.vented if k in us.shut] + \
               [k for k in ('mag', 'rct', 'eng', 'bri') if 0 < us.comp[k] < 5 and k not in us.vented]
        if not need: return
        if p.dc_t is None: p.dc_t = now + p.g(p.k['dc']); return
        k = need[0]
        if k in us.vented and k not in us.shut and k not in us.fire: us.shut.add(k)
        else: us.party = (k, 0.0)
        p.dc_t = None; p.sim.event(now, 'damage control')

    def go_home(p, now):
        sim = p.sim; ex, ey = sim.exit
        p.set_power('run'); p.sol = None; p.locking = False; p.lock_left = 0
        p.steer_to(ex, ey, 100)

    def sneak(p, dt, now, heard):
        """COLD RUN and INTERCEPT: weapons off. Follow a path to the jump point, cloud to cloud, and keep away from the Garuda."""
        sim, us, e = p.sim, p.sim.us, p.sim.enemy
        if p.path is None: p.path = plan_path(sim.world, (us.x, us.y), sim.exit); p.path_i = 0; p.set_power('sneak')
        while p.path_i < len(p.path) - 1 and math.hypot(p.path[p.path_i][0] - us.x, p.path[p.path_i][1] - us.y) < 400: p.path_i += 1
        wx, wy = p.path[p.path_i]
        if sim.brain.level == ALERT and sim.brain.has_fix(now):              # found: run for it
            p.set_power('run'); p.steer_to(wx, wy, 100); return
        known = heard or (p.decoded is not None and now - p.decoded < 60)
        if heard and p.contact_t is None:
            p.contact_t = now; p.react_until = now + p.g(p.k['react'])
        d = math.hypot(e.x - us.x, e.y - us.y)
        if known and p.contact_t is not None and now >= p.react_until:
            if p.op == 'scan' and p.scan is None and heard and p.free(now):
                p.active = True
                p.hands(p.k['scan'], lambda t: (setattr(p, 'active', False), setattr(p, 'scan', (brg(e.vx, e.vy), e.speed(), t)), p.know(t, 'scan')), now, 'scan')
            if p.op == 'plot' and heard and p.free(now) and (not p.marks or now - p.marks[-1][2] >= p.k['plot_gap']):
                p.hands(p.k['click'], lambda t: (p.mark(e, t), p.ruler and p.know(t, 'plot')), now, 'POINT')
            if p.op == 'quiet' and d < 4500: p.set_power('quiet'); us.thr = 0; us.ord = us.hdg; return
            if p.op == 'quiet': p.set_power('sneak')
            course = p.est(now) if p.op in ('scan', 'plot') else None
            if course is None and p.decoded is not None and p.msg and now >= p.decoded:      # INTERCEPT: the decoded heading and speed
                c = math.radians(p.msg[1]); course = (e.x, e.y, math.sin(c) * p.msg[2], math.cos(c) * p.msg[2])
            if course and p.op != 'close':
                ex, ey, vx, vy = course
                closing = (ex - us.x) * (vx - us.vx) + (ey - us.y) * (vy - us.vy) < 0
                if closing and d < 4000:      # it is coming our way: back off
                    us.ord = sim.world.clear_heading(us, jsround(brg(us.x - e.x, us.y - e.y)) % 360, 800, []); us.thr = clamp(60, 0, us.throttle_cap()); return
                if d < 2600: us.thr = 0; return            # too close to pass: wait for it to go

        p.steer_to(wx, wy, 100)

def plan_of(p): return p.power

def e_guess(sim):
    """Where a player looks for the enemy before contact: its spawn box (the briefing shows the top of the map)."""
    sp = sim.lv['enemy']; return sp['x'], sp['y']

def plan_path(world, start, goal, cell=250.0):
    """A route to the jump point round the rocks, preferring gas clouds (a person reading the map). Grid A*."""
    import heapq
    hw, hh = world.hw, world.hh; nx, ny = int(world.lv['w'] // cell), int(world.lv['h'] // cell)
    def ij(x, y): return clamp(int((x + hw) // cell), 0, nx - 1), clamp(int((y + hh) // cell), 0, ny - 1)
    block = set()
    for rx, ry, r in world.rocks:
        R = r + 180
        for i in range(int((rx - R + hw) // cell), int((rx + R + hw) // cell) + 1):
            for j in range(int((ry - R + hh) // cell), int((ry + R + hh) // cell) + 1):
                cx, cy = -hw + (i + 0.5) * cell, -hh + (j + 0.5) * cell
                if math.hypot(cx - rx, cy - ry) <= R + cell * 0.5: block.add((i, j))
    cloud = set()
    for cx, cy, cw, ch in world.lv['clouds']:
        for i in range(int((cx - cw / 2 + hw) // cell), int((cx + cw / 2 + hw) // cell) + 1):
            for j in range(int((cy - ch / 2 + hh) // cell), int((cy + ch / 2 + hh) // cell) + 1): cloud.add((i, j))
    s, g = ij(*start), ij(*goal); block.discard(s); block.discard(g)
    openq, came, cost = [(0, s)], {s: None}, {s: 0}
    while openq:
        _, c = heapq.heappop(openq)
        if c == g: break
        for di, dj in ((1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1)):
            n = (c[0] + di, c[1] + dj)
            if not (0 <= n[0] < nx and 0 <= n[1] < ny) or n in block: continue
            nc = cost[c] + math.hypot(di, dj) * (0.6 if n in cloud else 1.0)
            if nc < cost.get(n, 1e18): cost[n] = nc; came[n] = c; heapq.heappush(openq, (nc + 0.6 * math.hypot(g[0] - n[0], g[1] - n[1]), n))
    if g not in came: return [goal]
    path, c = [], g
    while c: path.append((-hw + (c[0] + 0.5) * cell, -hh + (c[1] + 0.5) * cell)); c = came[c]
    path.reverse()
    return path[::3] + [goal]

LAUNCH_STAGES = None   # filled from the board scene: the hangar's stage rates

# ---------------------------------------------------------------- one fight

class Sim:
    def __init__(sim, level, player, opening, rate, seed, tweak=None, B=None, torp_first=False, trace=False):
        sim.B, sim.lv = B or BOARD, LEVELS[level]
        sim.rnd = random.Random(seed); sim.trace = trace; sim.torp_first = torp_first
        tw = tweak or {}
        asura, garuda = apply_tweak(ship_file('Asura'), tw.get('us', {})), apply_tweak(ship_file('Garuda'), tw.get('them', {}))
        sim.frigate_st = apply_tweak(ship_file('Frigate'), tw.get('frigate', {}))
        lv = dict(sim.lv)
        if 'map' in tw: f = tw['map']; lv = scale_level(lv, f)
        sim.lv = lv
        sim.world = World(lv, sim.B)
        px, py, ph = lv['player']; sp = lv['enemy']
        sim.us = Ship(asura, px, py, ph, random.Random(seed * 7 + 1), 'us')
        ex, ey, eh = sp['x'], sp['y'], sp['hdg']
        if sp['box'][0] > 0 and sp['box'][1] > 0:     # EnemySpawn with a size: a random spot and facing every game
            ex += (sim.rnd.random() - 0.5) * sp['box'][0]; ey += (sim.rnd.random() - 0.5) * sp['box'][1]
            ex, ey = clamp(ex, -lv['w'] / 2 + 1000, lv['w'] / 2 - 1000), clamp(ey, -lv['h'] / 2 + 1000, lv['h'] / 2 - 1000); eh = sim.rnd.randrange(360)
        sim.enemy = Ship(garuda, ex, ey, eh, random.Random(seed * 7 + 2), 'Garuda')
        sim.brain = Brain(sim.enemy, dict(sim.B['Garuda'], **tw.get('brain', {})), sp, sim.world, random.Random(seed * 7 + 3))
        sim.brains = [sim.brain]; sim.ships = [sim.us, sim.enemy]
        sim.dt = sim.B['board']['tickSeconds'] * rate; sim.rate = rate
        sim.B_hold = asura['weapons']['lockHoldSeconds']
        sim.units = [0, 0, 0, 0]; sim.bank = 0.0
        sim.ours, sim.incoming, sim.fighters, sim.boats = [], [], [], []
        sim.identified = False; sim.their_shots = 0
        sim.exit = lv['jump'] if lv['goal'] == 'JumpPoint' else None
        sim.alarm_at = None; sim.next_arrival = None; sim.arrived = 0
        sim._interf = {}
        sim.m = dict(first_contact_s=None, scan_s=None, kill_s=None, killed_s=None, end_s=None, win=False, shots=0, hits=0, reactor_hits=0,
                     their_shots=0, their_hits=0, dodged=0, torps_in=0, torps_hit=0, pdc_kills=0, damage=0.0, found=False, found_s=None,
                     dead_s=0.0, dead_after=0.0, first_shot_s=None, last_event=0.0, why='', contact_d=None, heard_s=None, heard_d=None, found_d=None,
                     shots_lock=0, shots_hand=0)
        sim.p = Player(sim, player, opening, rate); sim.p.was_heard = False; sim.p.setup_done = False; sim.p.gun_picked = False
        sim.now = 0.0
    def event(sim, now, what):
        gap = now - sim.m['last_event']
        if gap > sim.m['dead_s']: sim.m['dead_s'] = gap; sim.m['dead_at'] = (sim.m['last_event'], what)
        if sim.m['first_contact_s'] is not None and sim.m['last_event'] >= sim.m['first_contact_s'] and gap > sim.m['dead_after']: sim.m['dead_after'] = gap
        sim.m['last_event'] = now
        if sim.trace: print(f"{now:7.1f}  {what:28s} range {math.hypot(sim.enemy.x - sim.us.x, sim.enemy.y - sim.us.y):6.0f}  "
                            f"Garuda armour {[int(a) for a in sim.enemy.armour]} RCT {sim.enemy.comp['rct']}  ours {[int(a) for a in sim.us.armour]} RCT {sim.us.comp['rct']}  charge {sim.p.charge:3.0f}")
    def message(sim, brain, now):
        """A radio message from the Garuda (EnemyBrain.Send). Good and expert players decode it (DECODE, guess); it gives the
        Garuda's bearing from us in 8 directions, its heading in 8 directions and its speed."""
        if brain is not sim.brain: return
        sim.event(now, 'radio message')
        if sim.p.kind == 'new' and sim.lv['goal'] != 'JumpPoint': return
        e, us = sim.enemy, sim.us
        sim.p.decoded = now + sim.p.g(DECODE.get(sim.p.kind, DECODE['good']))
        sim.p.msg = (round(brg(e.x - us.x, e.y - us.y) / 45) * 45 % 360, round((brg(e.vx, e.vy) if e.speed() > 0.1 else e.hdg) / 45) * 45 % 360, e.speed(), now)
    def interf_us(sim, e):
        key = (id(e), int(sim.now))
        if key not in sim._interf: sim._interf = {key: sim.world.interference(sim.us.x, sim.us.y, e.x, e.y)}
        return sim._interf[key]
    def launch_fighters(sim, now):
        us = sim.us; hg = us.st['hangar']
        sim.fighters.append(Fighter(us.x, us.y, 6, 'destroy', sim.enemy)); sim.event(now, 'fighters launched')
    def launch_boats(sim, now):
        us = sim.us
        sim.boats.append(Fighter(us.x, us.y, 4, 'secure', sim.enemy)); sim.boats[-1].marines = int(us.st['hangar']['marines']); sim.event(now, 'boats launched')
    def tick_flights(sim, dt, now):
        hg = sim.us.st['hangar']
        for f in sim.fighters + sim.boats:
            t = f.target; boat = f in sim.boats
            if t.dead or t.captured: continue
            reach = hg['dockUnits'] if boat else hg['fighterAttackUnits']; sp = hg['boatSpeedUnits'] if boat else hg['fighterSpeedUnits']
            d = math.hypot(t.x - f.x, t.y - f.y)
            if d > reach and not f.aboard:
                mv = min(sp * dt, d - reach * 0.9); f.x += (t.x - f.x) / d * mv; f.y += (t.y - f.y) / d * mv
                continue
            if f.aboard: f.x, f.y = t.x, t.y
            if boat: sim.board(f, t, dt, now); continue
            if not f.attacking: f.attacking = True; f.next_run = now; sim.event(now, 'fighters attack')
            if now < f.next_run: continue
            f.next_run = now + hg['fighterRunSeconds']
            side = side_at(brg(f.x - t.x, f.y - t.y), t.hdg)
            for _ in range(f.n):
                if not t.dead: t.take_run(side, f.order == 'disable', hg['fighterArmourDamage'], int(hg['fighterCompartmentDamage']))
            sim.event(now, 'fighter run ' + SIDES[side])
    def board(sim, f, t, dt, now):
        hg = sim.us.st['hangar']
        if not f.aboard:
            if not any(a <= 0 for a in t.armour) or t.speed() > hg['boardMaxSpeedUnits']: return
            f.aboard, f.board_t = True, 0.0; sim.event(now, 'boats aboard')
        f.board_t += dt
        if f.board_t < hg['boardSeconds'] or f.n == 0: return
        defenders = math.ceil(sum(t.crew.values()) * hg['defenderShare'])
        won = f.marines > 0 and (defenders == 0 or sim.us.roll(f.marines / (f.marines + defenders)))
        if won: t.captured = True; sim.event(now, 'captured')
        else: sim.event(now, 'boarding repelled'); sim.m['why'] = 'repelled'
        f.n = 0
    def reinforce(sim, now):
        q = sim.B['quest']
        if sim.lv['goal'] != 'WolfHunt': return
        if sim.alarm_at is None:
            if sim.brain.level != ALERT: return
            sim.alarm_at = now; sim.next_arrival = now + q['reinforceSeconds']
        if now >= sim.next_arrival:
            for _ in range(int(q['firstWave']) if sim.arrived == 0 else 1): sim.arrive(now)
            sim.next_arrival = now + q['reinforceEverySeconds']
    def arrive(sim, now):
        q = sim.B['quest']; hw, hh = sim.world.hw - q['arriveInsetUnits'], sim.world.hh - q['arriveInsetUnits']; us = sim.us
        for _ in range(8):
            r, edge = sim.rnd.uniform(-1, 1), sim.rnd.randrange(4)
            at = [(r * hw, hh), (r * hw, -hh), (hw, r * hh), (-hw, r * hh)][edge]
            if math.hypot(at[0] - us.x, at[1] - us.y) >= CODE['exit_min_spawn']: break
        sim.arrived += 1
        f = Ship(sim.frigate_st, at[0], at[1], ibrg(-at[0], -at[1]), random.Random(sim.rnd.random()), 'Frigate')
        b = Brain(f, sim.B['Frigate'], dict(hunts=True, cruise=-1, route=[], signal=0, first_signal=0, on_air=0), sim.world, random.Random(sim.rnd.random()))
        a = sim.rnd.random() * 2 * math.pi; rr = q['briefErrorUnits'] * math.sqrt(sim.rnd.random())
        b.raise_to(SEARCHING, now); b.heard((us.x + math.cos(a) * rr, us.y + math.sin(a) * rr), now, False)
        sim.brains.append(b); sim.ships.append(f); sim.event(now, 'frigate arrives')
    def run(sim, cap):
        us, e, p, dt, m = sim.us, sim.enemy, sim.p, sim.dt, sim.m
        goal = sim.lv['goal']
        while sim.now < cap:
            now = sim.now; us.now = e.now = now
            for o in sim.ships: o.now = now
            # our board, in BoardManager's order
            p.tick_power(dt, now); was = us.in_cloud; us.in_cloud = sim.world.in_cloud(us)
            if was != us.in_cloud: sim.event(now, 'into a cloud' if us.in_cloud else 'out of the cloud')
            p.tick(dt, now)
            us.tick(dt)
            for r in list(sim.ours):
                res = torp_step(r, dt, r.target) if r.torp else slug_step(r, dt, [o for o in sim.ships if o is not us])
                if res is None: continue
                sim.ours.remove(r)
                if res == 'miss': sim.event(now, 'MISS'); p.missed = True; continue   # the person sees MISS and plots again
                ship, side, at = res
                (ship.take_he if r.torp else ship.take_slug)(side, us.wp, at); m['hits'] += 1
                if ship is e and side == 3: m['reactor_hits'] += 1
                sim.event(now, 'HIT ' + SIDES[side]); p.last_hit = True
            sim.tick_flights(dt, now)
            for r in list(sim.incoming):
                if r.torp and p.pdc_stops(r, dt, now): sim.incoming.remove(r); continue
                if r.torp: res = torp_step(r, dt, us)
                else:
                    save = (us.x, us.y); us.x -= us.vx * dt; us.y -= us.vy * dt
                    res = slug_step(r, dt, [us]); us.x, us.y = save
                if res is None: continue
                sim.incoming.remove(r)
                if res == 'miss':
                    if not r.torp and dodge_check(r, us): m['dodged'] += 1
                    continue
                ship, side, at = res
                if r.torp: us.take_he(side, r.by, at); m['torps_hit'] += 1
                else: us.take_slug(side, r.by, at); m['their_hits'] += 1
                sim.event(now, 'we are hit ' + SIDES[side])
            us.tick_party(dt); us.tick_conditions(dt)
            p.tick_lock(dt, now) if not sim.lv['weapons_off'] else None
            p.tick_guns(dt, now)
            # the enemies (TacticalWorld.OnTick)
            for b in list(sim.brains):
                s = b.s
                s.tick_party(dt); s.tick_conditions(dt)
                if s.dead or s.captured: s.thr = 0
                else:
                    was = b.level; b.think(us, sim.fighters, sim, now)
                    dd = math.hypot(us.x - s.x, us.y - s.y)
                    if b is sim.brain and b.level > COLD and m['heard_s'] is None: m['heard_s'], m['heard_d'] = now, dd
                    if b is sim.brain and b.level == ALERT and not m['found']: m['found'] = True; m['found_s'] = now; m['found_d'] = dd; sim.event(now, 'DETECTED (it is alert)')
                s.tick(dt)
            sim.reinforce(now)
            if e.dead and m['kill_s'] is None: m['kill_s'] = now; sim.event(now, 'Garuda destroyed')
            if us.dead:
                m['killed_s'] = now; m['end_s'] = now; m['why'] = 'destroyed'; break
            # the goal (QuestSystem)
            if goal in ('WolfHunt',) and e.dead and sim.exit is None:
                q = sim.B['quest']; hw, hh = sim.world.hw - q['exitInsetUnits'], sim.world.hh - q['exitInsetUnits']
                sim.exit = max([(0, hh), (0, -hh), (hw, 0), (-hw, 0)], key=lambda c: (c[0] - us.x) ** 2 + (c[1] - us.y) ** 2)
            if sim.exit and math.hypot(sim.exit[0] - us.x, sim.exit[1] - us.y) <= sim.B['quest']['exitReachUnits']:
                m['win'] = True; m['end_s'] = now; break
            if goal == 'Destroy' and e.dead: m['win'] = True; m['end_s'] = now; break
            if goal == 'Capture' and (e.captured or e.dead or m['why'] == 'repelled'):
                m['win'] = e.captured; m['end_s'] = now; m['why'] = m['why'] or ('captured' if e.captured else 'destroyed it'); break
            if goal == 'HitUnseen':
                if sim.identified or p.lock_left > 0: m['end_s'] = now; m['why'] = 'scanned or locked'; break
                if m['hits'] > 0: m['win'] = True; m['end_s'] = now; break
            sim.now += dt
        if m['end_s'] is None: m['end_s'] = sim.now; m['why'] = m['why'] or 'time'
        sim.event(m['end_s'], 'end')
        m['their_shots'] = sim.their_shots; m['damage'] = us.damage_taken()
        return m

def dodge_check(r, us):
    """An incoming slug missed. Would it have hit if our ship had kept the course and speed it had when the slug was fired?"""
    x, y, vx, vy, hdg = r.cf; sx, sy = r.x - r.dx * r.gone, r.y - r.dy * r.gone
    t_end = r.maxu / r.speed; m = margin(r.by)
    fx, fy = sx - x, sy - y
    e = enters_box(fx, fy, fx + (r.dx * r.speed - vx) * t_end, fy + (r.dy * r.speed - vy) * t_end, hdg, us.half[0] * m, us.half[1] * m)
    return e is not None

def pdc_stops(p, r, dt, now):
    """WeaponSystem.PdcStops: each PDC group on AUTO, loaded and powered, gets one chance at each torpedo in its arc and reach."""
    us = p.sim.us
    if not p.wep_ready() or not p.pdc: return False
    step = r.speed * dt
    along = clamp((us.x - r.x) * r.dx + (us.y - r.y) * r.dy, 0, step); px, py = r.x + r.dx * along, r.y + r.dy * along
    for g, mid in enumerate(PDC_ORDER):
        if mid not in p.pdc or mid not in us.mounts or r.tried & (1 << g) or us.ammo[mid] < us.wp['pdcRoundsPerGo']: continue
        gx, gy = us.gun_at(mid); m = us.mounts[mid]
        rel = jsround((brg(px - gx, py - gy) - us.hdg) % 360) % 360
        if (px - gx) ** 2 + (py - gy) ** 2 > m['rangeUnits'] ** 2 or angdiff(rel, int(m['arc'])) > m['halfArc']: continue
        r.tried |= 1 << g; us.ammo[mid] -= int(us.wp['pdcRoundsPerGo'])
        if us.roll(us.wp['pdcKillChance']): p.sim.m['pdc_kills'] += 1; p.sim.event(now, 'PDC: torpedo down'); return True
    return False
Player.pdc_stops = pdc_stops

def apply_tweak(st, tw):
    """Change numbers in a ship file copy: {'navigation.topSpeedUnits': x, ...}; a value 'x1.5' multiplies."""
    for path, v in tw.items():
        d = st; keys = path.split('.')
        for k in keys[:-1]: d = d[k] if not isinstance(d, list) else d[int(k)]
        if keys[-1] == '*rangeRG':
            for m in st['weapons']['mounts']:
                if m['kind'] == 'RG': m['rangeUnits'] = v
            continue
        d[keys[-1]] = d[keys[-1]] * float(v[1:]) if isinstance(v, str) and v.startswith('x') else v
    return st

def scale_level(lv, f):
    lv = dict(lv); lv['w'] *= f; lv['h'] *= f
    sc = lambda x, y: (x * f, y * f)
    lv['player'] = sc(*lv['player'][:2]) + (lv['player'][2],)
    e = dict(lv['enemy']); e['x'], e['y'] = sc(e['x'], e['y']); e['route'] = [sc(*q) for q in e['route']]; lv['enemy'] = e
    lv['rocks'] = [(x * f, y * f, r) for x, y, r in lv['rocks']]; lv['clouds'] = [(x * f, y * f, w, h) for x, y, w, h in lv['clouds']]
    if lv['jump']: lv['jump'] = sc(*lv['jump'])
    return lv

# ---------------------------------------------------------------- cases and tables

CAPS = dict(WolfHunt=1800, Destroy=900, HitUnseen=900, Capture=900, JumpPoint=3000)
FIGHT_LEVELS = None
def cap_of(level): lv = LEVELS[level]; return CAPS[lv['goal']] * max(1.0, lv['w'] / 20000) ** 2     # LONG SHOT (40,000): 3,600 s

def load():
    global BOARD, LEVELS, LAUNCH_STAGES, FIGHT_LEVELS
    BOARD = read_board()
    LEVELS = {os.path.basename(f)[:-6]: read_level(os.path.basename(f)[:-6]) for f in sorted(glob.glob(P('Assets/Scenes/Levels/*.unity')))}
    t = open(P('Assets/Scenes/Tactical/02.1_Grid.unity'), encoding='utf-8').read()
    LAUNCH_STAGES = [dict(id=i, rate=float(r)) for i, r in re.findall(r'- id: (\w+)\n\s+label: [^\n]+\n\s+rate: ([\d.]+)', t)][:6]
    FIGHT_LEVELS = [n for n, lv in LEVELS.items() if lv['enemy'] and lv['goal'] in CAPS]
    return BOARD, LEVELS

def applies(level, opening):
    lv = LEVELS[level]
    if lv['weapons_off'] and opening in ('fighter', 'fire'): return False
    if opening == 'fighter' and int(ship_file('Asura')['hangar'].get('fighters', 6)) <= 0: return False    # Step 170: our ship carries none
    return True

def play(level, player, opening, rate, seeds, tweak=None, torp_first=False):
    out = []
    for sd in seeds:
        out.append(Sim(level, player, opening, rate, sd, tweak, torp_first=torp_first).run(cap_of(level)))
    return out

def avg(rows, k, only=None):
    v = [r[k] for r in rows if r[k] is not None and (only is None or only(r))]
    return sum(v) / len(v) if v else None

def summary(rows):
    n = len(rows); g = lambda k, f=None: avg(rows, k, f)
    shots_in = sum(r['their_shots'] for r in rows)
    return dict(fights=n, win_pct=100 * sum(r['win'] for r in rows) / n, first_contact_s=g('first_contact_s'), scan_s=g('scan_s'),
                kill_s=g('kill_s'), killed_s=g('killed_s'), end_s=g('end_s'), longest_dead_s=g('dead_s'), dead_after_s=g('dead_after'), shots=g('shots'), hits=g('hits'),
                reactor_hits=g('reactor_hits'), their_shots=g('their_shots'), their_hits=g('their_hits'), torps_in=g('torps_in'),
                torps_hit=g('torps_hit'), pdc_kills=g('pdc_kills'), damage=g('damage'),
                dodge_pct=100 * sum(r['dodged'] for r in rows) / shots_in if shots_in else None,
                found_pct=100 * sum(r['found'] for r in rows) / n, killed_pct=100 * sum(r['killed_s'] is not None for r in rows) / n,
                first_shot_s=g('first_shot_s'))

def fmt(v, w=6, d=0):
    return f"{'-':>{w}}" if v is None else f"{v:>{w}.{d}f}"

def settings_label(tw=None):
    a, gd = ship_file('Asura'), ship_file('Garuda')
    return (f"slug{a['weapons']['slugSpeedUnits']:.0f} torp{a['weapons']['torpedoSpeedUnits']:.0f} top{a['navigation']['topSpeedUnits']:.0f} "
            f"gtop{gd['navigation']['topSpeedUnits']:.0f} rg{a['weapons']['mounts'][0]['rangeUnits']:.0f}{'nose' if a['weapons']['mounts'][0]['halfArc'] == 0 else ''} aim{BOARD['Garuda']['aimErrorDeg']:.0f}"
            + (' ' + tw if tw else ''))

def all_cases(nf, csv_path=None):
    seeds_a, seeds_b = list(range(1, nf + 1)), list(range(1001, 1001 + nf))
    rows_csv = []
    print(f"Every case: {nf} fights with seeds 1-{nf} (A) and {nf} with seeds 1001-{1000 + nf} (B). Times in game seconds from the start.")
    hdr = (f"{'level':10} {'map':>6} {'rate':>4} {'player':6} {'open':7} | {'win%':>5} {'A/B':>9} | {'contact':>7} {'scan':>5} {'1stshot':>7} {'kill':>5} {'killed':>6} "
           f"{'end':>5} {'dead':>5} | {'shots':>5} {'hits':>4} {'rct':>4} | {'in':>4} {'hit':>4} {'dodge%':>6} {'dmg':>5} {'found%':>6} {'died%':>5} {'dead2':>5}")
    print(hdr)
    for level in FIGHT_LEVELS:
        lv = LEVELS[level]
        for rate in (1, 5):
            for player in PLAYERS:
                for op in OPENINGS:
                    if not applies(level, op):
                        if op == 'fighter' and int(ship_file('Asura')['hangar'].get('fighters', 6)) <= 0: continue      # no fighters on our ship: no row
                        rows_csv.append([TODAY, 'maths', level, int(lv['w']), f'x{rate}', player, op, 0] + ['n/a'] * 10 + [settings_label('weapons off')])
                        continue
                    A = play(level, player, op, rate, seeds_a); Bb = play(level, player, op, rate, seeds_b)
                    sa, sb, s = summary(A), summary(Bb), summary(A + Bb)
                    flag = ' *' if abs(sa['win_pct'] - sb['win_pct']) > 25 else ''
                    print(f"{level:10} {lv['w']:6.0f} {'x' + str(rate):>4} {player:6} {op:7} | {s['win_pct']:5.0f} {sa['win_pct']:4.0f}/{sb['win_pct']:<4.0f}{flag:2}| "
                          f"{fmt(s['first_contact_s'], 7)} {fmt(s['scan_s'], 5)} {fmt(s['first_shot_s'], 7)} {fmt(s['kill_s'], 5)} {fmt(s['killed_s'], 6)} {fmt(s['end_s'], 5)} "
                          f"{fmt(s['longest_dead_s'], 5)} | {fmt(s['shots'], 5, 1)} {fmt(s['hits'], 4, 1)} {fmt(s['reactor_hits'], 4, 1)} | {fmt(s['their_shots'], 4, 1)} "
                          f"{fmt(s['their_hits'], 4, 1)} {fmt(s['dodge_pct'], 6)} {fmt(s['damage'], 5)} {fmt(s['found_pct'], 6)} {fmt(s['killed_pct'], 5)} {fmt(s['dead_after_s'], 5)}")
                    rows_csv.append([TODAY, 'maths', level, int(lv['w']), f'x{rate}', player, op, s['fights'], round(s['win_pct']),
                                     *[('' if s[k] is None else round(s[k])) for k in ('first_contact_s', 'scan_s', 'kill_s', 'killed_s', 'end_s', 'longest_dead_s')],
                                     ('' if s['shots'] is None else round(s['shots'], 1)), ('' if s['hits'] is None else round(s['hits'], 1)),
                                     ('' if s['dodge_pct'] is None else round(s['dodge_pct'])), settings_label()])
            print()
    if csv_path:
        new = not os.path.exists(csv_path)
        with open(csv_path, 'a') as f:
            if new: f.write('date,source,level,map_units,rate,player,opening,fights,win_pct,first_contact_s,scan_s,kill_s,killed_s,end_s,longest_dead_s,shots,hits,dodge_pct,settings\n')
            for r in rows_csv: f.write(','.join(str(x) for x in r) + '\n')

TODAY = __import__('datetime').date.today().isoformat()

TWEAKS = [
    ('as the game is', {}),
    ('Garuda aim error 1 deg (was 3)', {'brain': {'aimErrorDeg': 1}}),
    ('Garuda aim error 6 deg', {'brain': {'aimErrorDeg': 6}}),
    ('lock holds 20 s (was 10)', {'us': {'weapons.lockHoldSeconds': 20}}),
    ('lock fills in 5 s (was 10)', {'us': {'weapons.lockSeconds': 5}}),
    ('gun turns 6 deg/s (was 3)', {'us': {'weapons.gunTurnDegPerSecond': 6}}),
    ('our slug 600/s (was 300)', {'us': {'weapons.slugSpeedUnits': 600}}),
    ('Garuda slug 600/s (was 300)', {'them': {'weapons.slugSpeedUnits': 600}}),
    ('railguns reach 3,000 (both)', {'us': {'weapons.*rangeRG': 3000}, 'them': {'weapons.*rangeRG': 3000}}),
    ('Garuda top speed 50 (was 35)', {'them': {'navigation.topSpeedUnits': 50}}),
    ('Garuda reloads in 20-25 s (was 12-17)', {'brain': {'railgunReloadSeconds': 20}}),
    ('Garuda sees 1,500 (was 2,000)', {'them': {'sensors.visualRangeUnits': 1500}}),
    ('our top speed 35 (was 50)', {'us': {'navigation.topSpeedUnits': 35}}),
    ('our slug damage 75 armour, 3 room', {'us': {'weapons.slugArmourDamage': 75, 'weapons.slugCompartmentDamage': 3}}),
    ('Garuda armour x0.67 (100/100/50/100)', {'them': {'damage.sideArmourMax': [100, 100, 50, 100]}}),
    ('map 30,000 (x1.5)', {'map': 1.5}),
    ('map 12,000 (x0.6)', {'map': 0.6}),
]

def tweaks(nf):
    seeds = list(range(1, nf + 1)) + list(range(1001, 1001 + nf))
    ops = ('fire', 'scan', 'quiet')
    print(f"Changed numbers on Wolf Hunt (Level_01), good player, {2 * nf} fights per cell. win% x1 / x5, and kill time at x1 (game s)")
    print(f"{'change':38} | " + ' | '.join(f"{op:^15}" for op in ops))
    for name, tw in TWEAKS:
        cells = []
        for op in ops:
            r1, r5 = summary(play('Level_01', 'good', op, 1, seeds, tw)), summary(play('Level_01', 'good', op, 5, seeds, tw))
            cells.append(f"{r1['win_pct']:3.0f}/{r5['win_pct']:<3.0f} {fmt(r1['kill_s'], 4)}s")
        print(f"{name:38} | " + ' | '.join(cells))

def extras(nf):
    seeds = list(range(1, nf + 1)) + list(range(1001, 1001 + nf))
    print(f"Extras on Wolf Hunt, {2 * nf} fights each. win% / kill s / killed% / our hits on the reactor face")
    def row(name, pl, op, rate, **kw):
        s = summary(play('Level_01', pl, op, rate, seeds, **kw))
        print(f"  {name:44} x{rate}: win {s['win_pct']:3.0f}%  kill {fmt(s['kill_s'], 5)}  killed {s['killed_pct']:3.0f}%  reactor hits {fmt(s['reactor_hits'], 4, 1)}  shots {fmt(s['shots'], 4, 1)}")
    for pl in PLAYERS:
        for rate in (1, 5): row(f"{pl}: scan, then torpedoes first", pl, 'scan', rate, torp_first=True)
    PLAYERS['good+'] = dict(PLAYERS['good'], port_seek=True, break_off=True)
    for op in ('fire', 'scan', 'quiet'):
        for rate in (1, 5): row(f"good, plus backing onto its port side: {op}", 'good+', op, rate)
    del PLAYERS['good+']

def hand():
    print("One fight, step by step: WEAPONS FREE (FirstShot), good player, opening 'fire', x1, seed 1.")
    lv = LEVELS['FirstShot']; e = lv['enemy']
    print(f"Layout read from the scene: us at {lv['player']}, Garuda at ({e['x']:.0f}, {e['y']:.0f}) heading {e['hdg']}, still, not firing.")
    m = Sim('FirstShot', 'good', 'fire', 1, 1, trace=True).run(900)
    print({k: (round(v, 1) if isinstance(v, float) else v) for k, v in m.items() if k not in ('dead_at',)})

def check():
    a, g, f = ship_file('Asura'), ship_file('Garuda'), ship_file('Frigate')
    print("Read from the ship files:")
    for nm, st in (('Asura', a), ('Garuda', g), ('Frigate', f)):
        n, w, d = st['navigation'], st['weapons'], st['damage']
        print(f"  {nm:8} top {n['topSpeedUnits']:.0f} acc {n['accelUnits']} brake {n['brakeUnits']} turn {n['turnRate']:.0f} box {st['width']:.0f}x{st['length']:.0f} "
              f"armour {[int(x) for x in d['sideArmourMax']]} slug {w['slugSpeedUnits']:.0f} dmg {w['slugArmourDamage']:.0f}/{w['slugCompartmentDamage']:.0f} HE {w['heArmourDamage']:.0f} "
              f"mounts {[(m['id'], int(m['ammo'])) for m in w['mounts']]}")
    print("Read from the board scene:")
    for k, v in BOARD.items(): print(f"  {k}: {v}")
    print("Launch stages:", LAUNCH_STAGES, "total", round(sum(100 / s['rate'] for s in LAUNCH_STAGES), 1), "game s")
    print("Levels:")
    for n, lv in LEVELS.items():
        e = lv['enemy'] or {}
        print(f"  {n:11} {lv['goal']:10} map {lv['w']:.0f}x{lv['h']:.0f} weapons off {lv['weapons_off']} us {lv['player']} enemy "
              f"{(round(e.get('x', 0)), round(e.get('y', 0)), e.get('hdg'), e.get('box'), 'hunts' if e.get('hunts') else 'passive', e.get('cruise'), len(e.get('route', [])), 'route pts')} "
              f"rocks {len(lv['rocks'])} clouds {len(lv['clouds'])} jump {lv['jump']}")
    print("Copied by hand from the game's code (no file to read them from):")
    for k, v in CODE.items(): print(f"  {k}: {v}")

# ---------------------------------------------------------------- long shots with the spinal railgun (Step 170)

def longshot(n=4000):
    """Long shots with the spinal railgun as built (Step 170): fixed on the nose, aimed by turning the ship. A lock turns the
    nose itself onto the lead, to the part of a degree, and its lead allows for our own drift. By hand, Shift with A or D turns
    2 a game second, read once a board tick: steps of 0.84 at x1 and 4.2 at x5. A slug keeps the ship's motion."""
    a, g = ship_file('Asura'), ship_file('Garuda'); B = BOARD['Garuda']; an = a['navigation']
    v = a['weapons']['slugSpeedUnits']; reach = max(m['rangeUnits'] for m in a['weapons']['mounts'] if m['kind'] == 'RG')
    hw, hl = g['width'] / 2 * 1.25, g['length'] / 2 * 1.25      # its hit box (our hitMarginPercent 25)
    cruise, top = B['cruiseUnits'], g['navigation']['topSpeedUnits']
    dr_max = a['sensors']['senRangeBaseUnits'] + 5 * a['sensors']['senRangePerStepUnits']
    steps = {r: an['fineTurnDegPerS'] * BOARD['board']['tickSeconds'] * r for r in (1, 5)}
    print(f"Spinal railgun: slug {v:.0f} a second, reach {reach:,.0f}. Our DRADIS reaches {dr_max:,.0f} at most, and a lock needs it, so today")
    print(f"nothing past {dr_max:,.0f} can be seen or locked. The numbers past that are for when something else can see it (the plan's drones).")
    print(f"Shift turn: {an['fineTurnDegPerS']:.0f} a game second, read once a board tick: {steps[1]:.2f} a step at x1, {steps[5]:.1f} at x5.")
    print()
    print(f"{'range':>7} | flight | Garuda moves at {cruise:.0f} / {top:.0f}")
    for R in (5000, 10000, 20000, 30000): print(f"{R:>7,} | {R / v:5.1f} s | {R / v * cruise:5.0f} / {R / v * top:5.0f}")
    print()
    rnd = random.Random(8)
    def shot(R, how, drift=0.0, turns=True):
        """One shot at a Garuda R away, cruising on a random course. how: lock (exact to the part of a degree, course from a scan),
        hand1 / hand5 (course from two POINTs 45 s apart, nose turned by hand at x1 / x5). drift: our leftover speed, any direction."""
        course = rnd.uniform(0, 360); c = math.radians(course)
        ang = rnd.uniform(0, 360); tx, ty = math.sin(math.radians(ang)) * R, math.cos(math.radians(ang)) * R
        da = rnd.uniform(0, 2 * math.pi); dvx, dvy = math.sin(da) * drift, math.cos(da) * drift
        if how == 'lock': ec, es, ex, ey = course, cruise, tx, ty
        else:     # two POINTs 45 s apart on dots up to 5 s old, 25 units click error
            l1, l2 = rnd.uniform(0, 5), rnd.uniform(0, 5)
            ex, ey = tx - math.sin(c) * cruise * l2 + rnd.gauss(0, 25), ty - math.cos(c) * cruise * l2 + rnd.gauss(0, 25)
            bx, by = tx - math.sin(c) * cruise * (45 + l1) + rnd.gauss(0, 25), ty - math.cos(c) * cruise * (45 + l1) + rnd.gauss(0, 25)
            ec, es = brg(ex - bx, ey - by), math.hypot(ex - bx, ey - by) / 45
        ce = math.radians(ec); evx, evy = math.sin(ce) * es, math.cos(ce) * es
        if how == 'lock': evx, evy = evx - dvx, evy - dvy      # the lock's lead allows for our drift; the line on the plot doesn't
        t = intercept(ex, ey, evx, evy, v) or R / v
        aim = brg(ex + evx * t, ey + evy * t)
        if how != 'lock':      # the person puts the line on the spot (25 units, guess), then stops on a whole step of the turn
            aim += math.degrees(rnd.gauss(0, 25) / R); st = steps[int(how[-1])]; w = jsround(aim)
            aim = w + (st * round((aim - w) / st) if st < 1 else 0)
        T = R / v; dist = cruise * T; leg_left = rnd.uniform(0, 6000) if turns else 1e9
        if dist > leg_left:   # it reaches its next point and turns: back the way it came 25% of the time, else somewhere new
            turn = 180 if rnd.random() < 0.25 else rnd.uniform(-120, 120); c2 = math.radians(course + turn)
            px, py, hdg = tx + math.sin(c) * leg_left + math.sin(c2) * (dist - leg_left), ty + math.cos(c) * leg_left + math.cos(c2) * (dist - leg_left), (course + turn) % 360
        else: px, py, hdg = tx + math.sin(c) * dist, ty + math.cos(c) * dist, course
        sx, sy = math.sin(math.radians(aim)) * v + dvx, math.cos(math.radians(aim)) * v + dvy
        return enters_box(-px, -py, -px + sx * T * 1.3, -py + sy * T * 1.3, hdg, hw, hl) is not None
    pct = lambda R, how, **kw: f"{100 * sum(shot(R, how, **kw) for _ in range(n)) / n:5.0f}%"
    print(f"Hit chance, {n:,} shots each. The Garuda cruises at {cruise:.0f} on a patrol (legs end 0-6,000 on, 25% turn back). Box ±{hw:.1f} x ±{hl:.1f}.")
    print(f"{'range':>7} | lock, scanned course | by hand x1, plotted | by hand x5, plotted | lock, straight leg (LONG SHOT)")
    for R in (5000, 10000, 15000, 20000, 25000, 30000):
        print(f"{R:>7,} | {pct(R, 'lock'):>20} | {pct(R, 'hand1'):>19} | {pct(R, 'hand5'):>19} | {pct(R, 'lock', turns=False):>12}")
    print()
    print("Leftover drift, any direction, when the slug leaves (it keeps it). A lock's lead allows for it; aimed by hand it doesn't.")
    print(f"{'drift':>6} | {'lock 10k':>8} {'20k':>6} {'30k':>6} | {'hand x1 10k':>11} {'20k':>6} {'30k':>6} | off the line at 30,000")
    for dr in (0, 1, 2, 5, 10):
        print(f"{dr:>6} | {pct(10000, 'lock', drift=dr):>8} {pct(20000, 'lock', drift=dr):>6} {pct(30000, 'lock', drift=dr):>6} | "
              f"{pct(10000, 'hand1', drift=dr):>11} {pct(20000, 'hand1', drift=dr):>6} {pct(30000, 'hand1', drift=dr):>6} | up to {dr * 30000 / v:4.0f} units")
    print()
    print("Stopping and turning onto the aim (our ship's file; a 90-degree turn; game seconds):")
    for eng in (5, 3):
        s = Ship(a, 0, 0, 0, random.Random(1), 'us'); s.eng = eng; s.thr = jsround(s.eng_share() * 100); s.vy = s.nv['topSpeedUnits'] * s.eng_share()
        sp0 = s.vy; s.thr = 0; tt = 0.0
        while s.speed() > 0.5: s.tick(0.42); tt += 0.42
        tb = tt; s.aim = 90.3
        while not s.on_aim: s.tick(0.42); tt += 0.42
        s2 = Ship(a, 0, 0, 0, random.Random(1), 'us'); s2.eng = eng; s2.vy = sp0; s2.thr = 0; s2.aim = 90.3; t2 = 0.0
        while not s2.on_aim: s2.tick(0.42); t2 += 0.42
        side = s2.speed(); s2.null_slide = True; t3 = t2
        while s2.speed() > 0.5: s2.tick(0.42); t3 += 0.42
        keep = an['driftKeepPerS'] * an['dragKeepPerS']
        print(f"  ENG {eng} ({sp0:.1f} a second): brake to under 0.5 in {tb:.0f} s, then turn on in {tt - tb:.0f} s: {tt:.0f} s. Turn first: on in {t2:.0f} s but "
              f"sliding {side:.0f} a second; Q and E stop it in {t3 - t2:.0f} s more ({t3:.0f} s); left to bleed, under 0.5 takes {math.log(0.5 / max(side, 0.5)) / math.log(keep):.0f} s.")

# ---------------------------------------------------------------- main

if __name__ == '__main__':
    args = sys.argv[1:]
    load()
    nf = int(args[args.index('--fights') + 1]) if '--fights' in args else 12
    if '--check' in args: check()
    elif '--hand' in args: hand()
    elif '--longshot' in args or '--sniper' in args: longshot()
    elif '--tweaks' in args: tweaks(nf)
    elif '--extras' in args: extras(nf)
    else: all_cases(nf, args[args.index('--csv') + 1] if '--csv' in args else None)
