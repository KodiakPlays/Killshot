using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Navigation: one ship's movement. It holds the ship's position, velocity, heading, ordered heading and
/// throttle, and moves the ship once per board tick. Our ship and every enemy carry one (the enemy is
/// our ship with other stats). Owner 2026-10-26: movement lives in one place, here.
/// Rules from board-logic.js (thrDown, setAtt, setApMode, apRangeUp/Down, onKey, gLoadNum, stabNum,
/// the heading swing in tickPower), Step 41 (the ship moves and slides) and Step 63 (5x speeds).
/// </summary>
public class NavSystem : MonoBehaviour
{
    [Header("Where the ship is, units (x east, y north)")]
    public double x, y;
    [System.NonSerialized] public double vx, vy; // units/s

    [Header("Heading and throttle")]
    [Tooltip("Heading at load. Owner 2026-10-24: 000, bow straight up (the design's demo had 083).")]
    [Range(0, 359)] public int heading = 0;
    [Range(0, 359)] public int orderedHeading = 0;
    [Tooltip("Throttle %, held to what the ENG bus carries. Owner 2026-10-25: 0 at load (power is cold; the design's demo had 40); if ENG can no longer carry it, it drops to 0. "
           + "Owner 2026-11-02: -100 is full reverse, 100 full ahead.")]
    [Range(-100, 100)] public int throttle = 0;

    [Header("Attitude hold and approach (the autopilot's settings)")]
    [Tooltip("NOSE, STBD, PORT, TAIL, or empty for none.")]
    public string attitude = "NOSE";
    [Tooltip("DIRECT, FLANK or EVASIVE.")]
    public string approachMode = "DIRECT";
    [Tooltip("Index into the approach ranges.")]
    public int approachRange = 2;
    [Tooltip("Approach hold ranges, units.")]
    public int[] approachRanges = { 500, 1000, 1500, 2000, 3000, 5000 };

    /// <summary>The ship this belongs to. Navigation reads its ENG bus and its engine room.</summary>
    [System.NonSerialized] public ShipState ship;

    /// <summary>This ship's numbers for movement: its section of the ship's stats file (Assets/Ships,
    /// Starwolf → Tuning).</summary>
    [System.Serializable]
    public class Stats
    {
        [Header("Turning")]
        [Tooltip("Ordered heading step for the A and D keys, degrees.")]
        public int headingKeyStep = 3;
        [Tooltip("Degrees the bow swings per game second at full ENG and full engine-room integrity (the design: 4 per tick at x1; Step 63: x5).")]
        public float turnRate = 20f;
        [Tooltip("The swing at ENG 1, degrees per game second (owner 2026-11-01: more power, tighter turns). Full ENG gives Turn Rate, in equal steps.")]
        public float baseTurnRate = 10f;
        [Tooltip("Owner 2026-11-08: the slow turn for aiming the spinal railgun, Shift held with A or D, degrees per game second. It turns in fractions of a degree.")]
        public float fineTurnDegPerS = 2f;
        // Step 41 (owner 2026-10-18): actual chases commanded at capped rates; the hull turns, the velocity
        // follows, and the sideways part bleeds off (the ship slides). The prototype's numbers (MAX_SPEED 240,
        // ACCEL_RATE 80, BRAKE_RATE 150, DRIFT_DECAY .988, SPACE_DRAG .9992 per 1/60 s) were slowed to a 2.5 units/s
        // top speed. Step 63 (owner 2026-10-24, measured with docs/scale-sim): speeds and turns x5, acceleration
        // x25 and the keep rates to the 5th power. A fight then takes about 7 minutes at x1 instead of 35.
        [Header("Movement (Step 41; 5x in Step 63)")]
        [Tooltip("Speed at 100% throttle, units/s, with ENG full (owner 2026-11-01, Step 130: 50 on our ship).")]
        [FormerlySerializedAs("topSpeedKms")] public double topSpeedUnits = 12.5;
        [Tooltip("The speed limit at ENG 1, units/s (owner 2026-11-01, Step 130: 25 on our ship). Full ENG gives the top speed, in equal steps. ENG 0: no thrust.")]
        public double baseSpeedUnits = 2.5;
        [Tooltip("Speeding up at full ENG, units/s per mission second (Step 63: 25x, so full speed comes 5x sooner).")]
        [FormerlySerializedAs("accelKms2")] public double accelUnits = 0.2175;
        [Tooltip("Speeding up at ENG 1, units/s per mission second (owner 2026-11-01: more power, more acceleration). Full ENG gives Accel Units, in equal steps. Slowing down and the side thrusters rise by the same share.")]
        public double baseAccelUnits = 0.10875;
        [Tooltip("Slowing down at full ENG, units/s per mission second (Step 63: 25x).")]
        [FormerlySerializedAs("brakeKms2")] public double brakeUnits = 0.4075;
        [Tooltip("Owner 2026-11-02: reverse is slower than ahead. Full reverse gives this share of the top speed, and speeds up backwards at this share of Accel Units (the prototype: 0.42).")]
        [Range(0f, 1f)] public double reverseShare = 0.42;
        [Tooltip("Share of sideways speed kept each mission second: after a turn the ship slides, then lines up (Step 63: 0.9925^5).")]
        public double driftKeepPerS = 0.96307;
        [Tooltip("Share of all speed kept each mission second (Step 63: 0.9995^5).")]
        public double dragKeepPerS = 0.997502;
        [Header("Side thrusters (owner 2026-11-01: Q and E)")]
        [Tooltip("Sideways speed with the key held, units/s, at full ENG. The nose stays where it is.")]
        public double sideSpeedUnits = 5;
        [Tooltip("The side thrusters' full push with the key held, units/s per mission second, at full ENG. Owner 2026-11-09: let go, and the slide keeps going and fades (Drift Keep Per S).")]
        public double sideAccelUnits = 1;
        [Tooltip("Owner 2026-11-09: one tap of Q or E adds this much sideways speed, units/s, at full ENG.")]
        public double sideTapUnits = 2;
        [Tooltip("Owner 2026-11-09: held, Q or E push gently at first and reach the full push after this many mission seconds.")]
        public double sideRampSeconds = 2;
    }
    /// <summary>The numbers this part plays by: its section of the ship's stats file (a copy while the game runs).</summary>
    public Stats stats => shipStats ? shipStats.Stats.navigation : (shipStats = GetComponent<Ship>()).Stats.navigation;
    [System.NonSerialized] Ship shipStats;
    // the bow turns whole degrees; fractions of a degree bank here so x0.25 turns at the right speed too
    [System.NonSerialized] private double turnBank;
    /// <summary>The side thrusters, from the Q and E keys: -1 slides to port, 1 to starboard, 0 neither.</summary>
    [System.NonSerialized] public int slide;
    /// <summary>Owner 2026-11-09: taps of Q (-1) and E (+1) since the last board tick. Each gives a small push, so a quick tap is never missed.</summary>
    [System.NonSerialized] public int slideTaps;
    // mission seconds Q or E has been held, for the push that grows
    [System.NonSerialized] private double slideHeld;
    /// <summary>Owner 2026-11-08: the slow turn for aiming, from Shift with A or D: -1 to port, 1 to starboard, 0 neither.</summary>
    [System.NonSerialized] public int fineTurn;
    /// <summary>The part of a degree the slow turn has put on the bow, -0.5 to 0.5. The heading stays in whole degrees for
    /// everything else; a fixed gun fires along the heading plus this.</summary>
    [System.NonSerialized] public double fine;
    /// <summary>The bow to the fraction of a degree, 0 to 360.</summary>
    public double TrueHeading => ((heading + fine) % 360 + 360) % 360;
    /// <summary>Owner 2026-11-09: while the spinal railgun is locked on, the bow turns itself onto this heading, degrees, to the part of a
    /// degree (WeaponSystem.TickGuns sets it each tick). NaN: no lock, the helm is yours.</summary>
    [System.NonSerialized] public double aimHeading = double.NaN;
    // the board's clock, for drawing between ticks (Step 75)
    private BoardManager board;

    // Step 75 (owner 2026-10-26): the rules step once per board tick; between ticks the ship is drawn gliding on
    // along its velocity, its bow still swinging, so a ship in view moves smoothly. Only the drawing; x and y stay true.
    // ponytail: straight-line guess, so a speeding-up or turning ship settles back a little each tick; predict the next tick if that shows
    double Ahead => board ? board.SinceTick : 0;
    /// <summary>Where the ship is drawn now, units: its true spot, carried on by the time since the last tick.</summary>
    public double DrawnX => x + vx * Ahead;
    public double DrawnY => y + vy * Ahead;
    /// <summary>The bow as drawn now, degrees: still swinging toward the ordered heading.</summary>
    public double DrawnHeading
    {
        get
        {
            if (fineTurn != 0 && ship != null) return heading + fine + fineTurn * stats.fineTurnDegPerS * Ahead;
            int d = TurnLeft;
            if (d == 0 || ship == null) return heading + fine;
            return heading + System.Math.Sign(d) * System.Math.Min(System.Math.Abs(d), TurnPerS * Ahead);
        }
    }

    /// <summary>Owner 2026-11-01: the ship's collision box, a rectangle round its model: half its width (x) and half
    /// its length (y), units. It turns with the ship.</summary>
    public Vector2 BoxHalf { get { var s = (shipStats ? shipStats : (shipStats = GetComponent<Ship>())).Stats; return new Vector2(s.width / 2f, s.length / 2f); } }
    /// <summary>Where TacticalWorld last had this ship: it checks the whole way from there, so a fast ship can't skip through.</summary>
    [System.NonSerialized] public double wasX, wasY;

    public double SpeedUnits => System.Math.Sqrt(vx * vx + vy * vy);
    /// <summary>Course over ground, degrees clockwise from north (the way the ship is actually going).</summary>
    public double CourseDeg => (System.Math.Atan2(vx, vy) * 180 / System.Math.PI + 360) % 360;
    /// <summary>Ordered heading minus heading, -180..179.</summary>
    public int TurnLeft => ((orderedHeading - heading + 540) % 360) - 180;

    double EngineRoom => ship.damage.CompHealth("eng") / 5.0;
    /// <summary>How far ENG is up its range: 0 at ENG 1 or less, 1 at full (owner 2026-11-01: more power, more of everything).</summary>
    double EngStep => System.Math.Max(0, ship.power.BusUnits("eng") - 1) / (double)System.Math.Max(1, ship.power.stats.busMax - 1);
    /// <summary>Speeding up now, units/s per second.</summary>
    double Accel => stats.baseAccelUnits + (stats.accelUnits - stats.baseAccelUnits) * EngStep;
    /// <summary>The share of full thrust ENG gives now. Slowing down and the side thrusters use it.</summary>
    double Thrust => stats.accelUnits > 0 ? Accel / stats.accelUnits : 1;
    /// <summary>Degrees the bow swings per game second now (the design's per-tick swing at x1).</summary>
    double TurnPerS => System.Math.Max(1, ShipState.JsRound((stats.baseTurnRate + (stats.turnRate - stats.baseTurnRate) * EngStep) * EngineRoom));
    /// <summary>The share of top speed the ENG bus allows: ENG 1 gives baseSpeedUnits, full ENG topSpeedUnits, in equal steps; ENG 0 nothing.</summary>
    double EngShare
    {
        get
        {
            int eng = ship.power.BusUnits("eng"), full = ship.power.stats.busMax;
            if (eng <= 0) return 0;
            return (stats.baseSpeedUnits + (stats.topSpeedUnits - stats.baseSpeedUnits) * (eng - 1) / System.Math.Max(1, full - 1)) / stats.topSpeedUnits;
        }
    }
    /// <summary>thrCap: what ENG allows x the engine room's integrity.</summary>
    public int ThrottleCap => ShipState.JsRound(EngShare * 100 * EngineRoom);
    /// <summary>gLoadNum: 1 G plus 1 G at full throttle, plus the approach mode's extra.</summary>
    public double GLoad => 1 + System.Math.Abs(throttle) / 100.0 + (approachMode == "FLANK" ? 0.5 : approachMode == "EVASIVE" ? 1.2 : 0);
    /// <summary>stabNum: 45 points lost per G over 1, 0..100.</summary>
    public double Stability => System.Math.Max(0, System.Math.Min(100, 100 - (GLoad - 1) * 45));

    /// <summary>thrDown: from the pointer's height in the fader well, top = 100, middle 0, bottom -100 (owner 2026-11-02), capped at the press both ways.</summary>
    public void SetThrottle(double fromTop01, int cap) { cap = System.Math.Min(100, cap); throttle = System.Math.Max(-cap, System.Math.Min(cap, ShipState.JsRound((1 - 2 * fromTop01) * 100))); }
    /// <summary>setAtt: click to hold, click again to release.</summary>
    public void ToggleAttitude(string k) => attitude = attitude == k ? "" : k;
    public void SetApproachMode(string k) => approachMode = k;
    public void StepApproachRange(int d) => approachRange = System.Math.Max(0, System.Math.Min(approachRanges.Length - 1, approachRange + d));
    /// <summary>onKey: A and D step the ordered heading and clear the attitude hold. The order stops 179° ahead of the bow:
    /// past 180 the short way round flips, and a held key swung the ship back and forth.</summary>
    public void StepOrderedHeading(int dir)
    {
        int lead = System.Math.Max(-179, System.Math.Min(179, TurnLeft + dir * stats.headingKeyStep));
        orderedHeading = ((heading + lead) % 360 + 360) % 360;
        attitude = "";
    }

    /// <summary>Start the ship here, facing this way, throttle at zero (where a level puts it).</summary>
    public void PlaceAt(double atX, double atY, int headingDeg)
    {
        x = atX; y = atY;
        heading = orderedHeading = headingDeg;
        fine = 0;
        throttle = 0;
        PlaceModel();
    }

    /// <summary>One board tick of dt mission seconds: steer, move, then put the model where the ship is.</summary>
    public void Tick(double dt)
    {
        Steer(dt);
        Move(dt);
        PlaceModel();
    }

    // The throttle falls to what ENG allows (no engine-room factor here, as the board does — HANDOFF_GAPS
    // row 32); the bow swings toward the ordered heading.
    void Steer(double dt)
    {
        int engCap = ShipState.JsRound(EngShare * 100);
        if (System.Math.Abs(throttle) > engCap) throttle = 0; // owner 2026-10-25: power lost, the throttle goes back to zero (the design held it at the cap); you set it again
        if (fineTurn != 0)
        {
            // owner 2026-11-08: the slow turn for aiming. Whole degrees pass into the heading, the order follows the bow, and the autopilot lets go
            fine += fineTurn * stats.fineTurnDegPerS * dt;
            int whole = (int)System.Math.Floor(fine + 0.5);
            heading = ((heading + whole) % 360 + 360) % 360;
            fine -= whole;
            orderedHeading = heading;
            attitude = "";
            turnBank = 0;
            return;
        }
        if (!double.IsNaN(aimHeading))
        {
            // owner 2026-11-09: a lock on the spinal railgun turns the ship itself: whole degrees at the usual rate, then the last part of a degree at the slow turn's
            int whole = (int)System.Math.Floor(aimHeading + 0.5);
            orderedHeading = (whole % 360 + 360) % 360;
            attitude = "";
            if (heading == orderedHeading)
            {
                turnBank = 0;
                double want = aimHeading - whole, most = stats.fineTurnDegPerS * dt;
                fine = System.Math.Abs(want - fine) <= most ? want : fine + System.Math.Sign(want - fine) * most;
                return;
            }
        }
        if (heading == orderedHeading) { turnBank = 0; return; }
        fine = 0; // a whole-degree turn drops the slow turn's part of a degree
        // banked so the bow turns whole degrees
        turnBank += TurnPerS * dt;
        int step = (int)System.Math.Floor(turnBank + 1e-9);
        if (step < 1) return;
        turnBank -= step;
        int d = TurnLeft;
        heading = System.Math.Abs(d) <= step ? orderedHeading : ((heading + System.Math.Sign(d) * step) % 360 + 360) % 360;
    }

    // Speed along the nose chases the throttle's target, sideways speed bleeds off, then the ship moves.
    void Move(double dt)
    {
        if (dt <= 0) return;
        // owner 2026-10-29: with no power on the ENG bus there is no thrust and no braking: the ship drifts on
        if (ship.power.BusUnits("eng") == 0) { slideTaps = 0; slideHeld = 0; x += vx * dt; y += vy * dt; return; }
        double h = heading * System.Math.PI / 180;
        double nx = System.Math.Sin(h), ny = System.Math.Cos(h); // the nose
        double fwd = vx * nx + vy * ny, side = vx * ny - vy * nx; // side: to starboard
        double keep = System.Math.Pow(stats.dragKeepPerS, dt);
        fwd *= keep;
        side *= keep * System.Math.Pow(stats.driftKeepPerS, dt);
        double target = throttle / 100.0 * stats.topSpeedUnits * (throttle < 0 ? stats.reverseShare : 1); // the throttle is already held to what ENG power allows (Steer)
        // speeding up, ahead or astern, at the engines' rate (less astern); slowing down, either way, at the brake rate
        bool speedingUp = target > fwd ? fwd >= 0 : fwd <= 0;
        fwd = Toward(fwd, target, (!speedingUp ? stats.brakeUnits * Thrust : target < 0 ? Accel * stats.reverseShare : Accel) * dt);
        // owner 2026-11-01: Q and E push the ship sideways with the nose where it is. Owner 2026-11-09: a tap is a small push (one
        // per tick, never past the side speed); held, the push grows gently, then faster; let go, and the slide keeps going and fades
        double sideTop = stats.sideSpeedUnits * Thrust;
        int tap = System.Math.Sign(slideTaps);
        slideTaps = 0;
        if (tap != 0) side = tap * System.Math.Max(tap * side, System.Math.Min(tap * side + stats.sideTapUnits * Thrust, sideTop));
        slideHeld = slide != 0 ? slideHeld + dt : 0;
        if (slide != 0) side = Toward(side, slide * sideTop, stats.sideAccelUnits * Thrust * System.Math.Min(1, slideHeld / stats.sideRampSeconds) * dt);
        vx = fwd * nx + side * ny;
        vy = fwd * ny - side * nx;
        x += vx * dt;
        y += vy * dt;
    }

    static double Toward(double v, double target, double step) => System.Math.Abs(target - v) <= step ? target : v + System.Math.Sign(target - v) * step;

    private void Awake() => board = FindAnyObjectByType<BoardManager>();

    // in Update, so the ship cameras (LateUpdate) follow the model where it is this frame
    private void Update() => PlaceModel();

    /// <summary>Put this ship's 3D model where it is drawn, facing its drawn heading (clockwise from north;
    /// about +z in the x/y plane that is −heading).</summary>
    public void PlaceModel()
    {
        transform.SetPositionAndRotation(new Vector3((float)DrawnX, (float)DrawnY, transform.position.z), Quaternion.Euler(0f, 0f, -(float)DrawnHeading));
    }
}
