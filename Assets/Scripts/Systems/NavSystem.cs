using UnityEngine;

/// <summary>
/// Navigation: one ship's movement. It holds the ship's position, velocity, heading, ordered heading and
/// throttle, and moves the ship once per board tick. Our ship and every enemy carry one (the enemy is
/// our ship with other stats). Owner 2026-10-26: movement lives in one place, here.
/// Rules from board-logic.js (thrDown, setAtt, setApMode, apRangeUp/Down, onKey, gLoadNum, stabNum,
/// the heading swing in tickPower), Step 41 (the ship moves and slides) and Step 63 (5x speeds).
/// </summary>
public class NavSystem : MonoBehaviour
{
    [Header("Where the ship is, km (x east, y north)")]
    public double x, y;
    [System.NonSerialized] public double vx, vy; // km/s

    [Header("Heading and throttle")]
    [Tooltip("Heading at load. Owner 2026-10-24: 000, bow straight up (the design's demo had 083).")]
    [Range(0, 359)] public int heading = 0;
    [Range(0, 359)] public int orderedHeading = 0;
    [Tooltip("Throttle %, held to what the ENG bus carries. Owner 2026-10-25: 0 at load (power is cold; the design's demo had 40); if ENG can no longer carry it, it drops to 0.")]
    [Range(0, 100)] public int throttle = 0;

    [Header("Attitude hold and approach (the autopilot's settings)")]
    [Tooltip("NOSE, STBD, PORT, TAIL, or empty for none.")]
    public string attitude = "NOSE";
    [Tooltip("DIRECT, FLANK or EVASIVE.")]
    public string approachMode = "DIRECT";
    [Tooltip("Index into the approach ranges.")]
    public int approachRange = 2;
    [Tooltip("Approach hold ranges, km.")]
    public int[] approachRanges = { 2000, 4000, 8000, 12000, 18000, 24000 };

    [Header("Turning")]
    [Tooltip("Ordered heading step for the A and D keys, degrees.")]
    [Tune("Navigation")] public int headingKeyStep = 3;
    [Tooltip("Degrees the bow swings per game second at full engine-room integrity (the design: 4 per tick at x1; Step 63: x5).")]
    [Tune("Navigation")] public float turnRate = 20f;

    // Step 41 (owner 2026-10-18): actual chases commanded at capped rates; the hull turns, the velocity
    // follows, and the sideways part bleeds off (the ship slides). The prototype's numbers (MAX_SPEED 240,
    // ACCEL_RATE 80, BRAKE_RATE 150, DRIFT_DECAY .988, SPACE_DRAG .9992 per 1/60 s) were slowed to a 2.5 km/s
    // top speed. Step 63 (owner 2026-10-24, measured with docs/scale-sim): speeds and turns x5, acceleration
    // x25 and the keep rates to the 5th power. A fight then takes about 7 minutes at x1 instead of 35.
    [Header("Movement (Step 41; 5x in Step 63)")]
    [Tooltip("Speed at 100% throttle, km/s (Step 63: 5x the design's 2.5).")]
    [Tune("Navigation")] public double topSpeedKms = 12.5;
    [Tooltip("Speeding up, km/s per mission second (Step 63: 25x, so full speed comes 5x sooner).")]
    [Tune("Navigation")] public double accelKms2 = 0.2175;
    [Tooltip("Slowing down, km/s per mission second (Step 63: 25x).")]
    [Tune("Navigation")] public double brakeKms2 = 0.4075;
    [Tooltip("Share of sideways speed kept each mission second: after a turn the ship slides, then lines up (Step 63: 0.9925^5).")]
    [Tune("Navigation")] public double driftKeepPerS = 0.96307;
    [Tooltip("Share of all speed kept each mission second (Step 63: 0.9995^5).")]
    [Tune("Navigation")] public double dragKeepPerS = 0.997502;

    /// <summary>The ship this belongs to. Navigation reads its ENG bus and its engine room.</summary>
    [System.NonSerialized] public HandoffBoardState ship;
    // the bow turns whole degrees; fractions of a degree bank here so x0.25 turns at the right speed too
    [System.NonSerialized] private double turnBank;
    // the board's clock, for drawing between ticks (Step 75)
    private HandoffBoardControls board;

    // Step 75 (owner 2026-10-26): the rules step once per board tick; between ticks the ship is drawn gliding on
    // along its velocity, its bow still swinging, so a ship in view moves smoothly. Only the drawing; x and y stay true.
    // ponytail: straight-line guess, so a speeding-up or turning ship settles back a little each tick; predict the next tick if that shows
    double Ahead => board ? board.SinceTick : 0;
    /// <summary>Where the ship is drawn now, km: its true spot, carried on by the time since the last tick.</summary>
    public double DrawnX => x + vx * Ahead;
    public double DrawnY => y + vy * Ahead;
    /// <summary>The bow as drawn now, degrees: still swinging toward the ordered heading.</summary>
    public double DrawnHeading
    {
        get
        {
            int d = TurnLeft;
            if (d == 0 || ship == null) return heading;
            return heading + System.Math.Sign(d) * System.Math.Min(System.Math.Abs(d), TurnPerS * Ahead);
        }
    }

    public double SpeedKms => System.Math.Sqrt(vx * vx + vy * vy);
    /// <summary>Course over ground, degrees clockwise from north (the way the ship is actually going).</summary>
    public double CourseDeg => (System.Math.Atan2(vx, vy) * 180 / System.Math.PI + 360) % 360;
    /// <summary>Ordered heading minus heading, -180..179.</summary>
    public int TurnLeft => ((orderedHeading - heading + 540) % 360) - 180;

    double EngineRoom => ship.CompHealth("eng") / 5.0;
    /// <summary>Degrees the bow swings per game second now (the design's per-tick swing at x1).</summary>
    double TurnPerS => System.Math.Max(1, HandoffBoardState.JsRound(turnRate * EngineRoom));
    /// <summary>thrCap: ENG units x 20 x the engine room's integrity.</summary>
    public int ThrottleCap => HandoffBoardState.JsRound((double)ship.power.BusUnits("eng") * 20 * EngineRoom);
    /// <summary>gLoadNum: 1 G plus 1 G at full throttle, plus the approach mode's extra.</summary>
    public double GLoad => 1 + throttle / 100.0 + (approachMode == "FLANK" ? 0.5 : approachMode == "EVASIVE" ? 1.2 : 0);
    /// <summary>stabNum: 45 points lost per G over 1, 0..100.</summary>
    public double Stability => System.Math.Max(0, System.Math.Min(100, 100 - (GLoad - 1) * 45));

    /// <summary>thrDown: from the pointer's height in the fader well, top = 100, capped at the press.</summary>
    public void SetThrottle(double fromTop01, int cap) => throttle = System.Math.Max(0, System.Math.Min(System.Math.Min(100, cap), HandoffBoardState.JsRound((1 - fromTop01) * 100)));
    /// <summary>setAtt: click to hold, click again to release.</summary>
    public void ToggleAttitude(string k) => attitude = attitude == k ? "" : k;
    public void SetApproachMode(string k) => approachMode = k;
    public void StepApproachRange(int d) => approachRange = System.Math.Max(0, System.Math.Min(approachRanges.Length - 1, approachRange + d));
    /// <summary>onKey: A and D step the ordered heading and clear the attitude hold.</summary>
    public void StepOrderedHeading(int dir) { orderedHeading = ((orderedHeading + dir * headingKeyStep) % 360 + 360) % 360; attitude = ""; }

    /// <summary>Start the ship here, facing this way, throttle at zero (where a level puts it).</summary>
    public void PlaceAt(double atX, double atY, int headingDeg)
    {
        x = atX; y = atY;
        heading = orderedHeading = headingDeg;
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

    // The throttle falls to ENG units x 20 (no engine-room factor here, as the board does — HANDOFF_GAPS
    // row 32); the bow swings toward the ordered heading.
    void Steer(double dt)
    {
        int engCap = ship.power.BusUnits("eng") * 20;
        if (throttle > engCap) throttle = 0; // owner 2026-10-25: power lost, the throttle goes back to zero (the design held it at the cap); you set it again
        if (heading == orderedHeading) { turnBank = 0; return; }
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
        double h = heading * System.Math.PI / 180;
        double nx = System.Math.Sin(h), ny = System.Math.Cos(h); // the nose
        double fwd = vx * nx + vy * ny, side = vx * ny - vy * nx; // side: to starboard
        double keep = System.Math.Pow(dragKeepPerS, dt);
        fwd *= keep;
        side *= keep * System.Math.Pow(driftKeepPerS, dt);
        double target = throttle / 100.0 * topSpeedKms; // the throttle is already held to what ENG power allows (Steer)
        double step = (fwd < target ? accelKms2 : brakeKms2) * dt;
        fwd = System.Math.Abs(target - fwd) <= step ? target : fwd + System.Math.Sign(target - fwd) * step;
        vx = fwd * nx + side * ny;
        vy = fwd * ny - side * nx;
        x += vx * dt;
        y += vy * dt;
    }

    private void Awake() => board = FindAnyObjectByType<HandoffBoardControls>();

    // in Update, so the ship cameras (LateUpdate) follow the model where it is this frame
    private void Update() => PlaceModel();

    /// <summary>Put this ship's 3D model where it is drawn, facing its drawn heading (clockwise from north;
    /// about +z in the x/y plane that is −heading).</summary>
    public void PlaceModel()
    {
        transform.SetPositionAndRotation(new Vector3((float)DrawnX, (float)DrawnY, transform.position.z), Quaternion.Euler(0f, 0f, -(float)DrawnHeading));
    }
}
