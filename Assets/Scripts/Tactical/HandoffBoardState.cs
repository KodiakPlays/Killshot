using UnityEngine;

/// <summary>
/// The handoff board's own state and rules, ported from reference/board-logic.js — what the
/// board's controls do before any game system is wired in. Plain data: one instance lives on
/// HandoffBoardControls and every panel reads it. Grows one drawer at a time (Step 33b on).
/// Every starting value and constant is from the board's initial state and data tables.
/// </summary>
[System.Serializable]
public class HandoffBoardState
{
    public enum BusMode { Vent, Standby, Draw } // BUS_MODES order

    [System.Serializable]
    public class Bus
    {
        public string key, label;
        public int fill;
        public BusMode mode = BusMode.Standby;
        [Tooltip("Direction the selector steps next: +1 toward DRAW, -1 toward VENT (busDir).")]
        public int dir = 1;
        [Tooltip("Signal each unit on this bus adds, on the 0–100 scale (Step 35, owner 2026-10-17). "
               + "Default 10: a full reactor of 10 units reads 100 (prototype getSig).")]
        [Tune("Power & signal")]
        public float signalPerUnit = 10f;
    }

    [System.Serializable]
    public class Compartment
    {
        public string key, tag, name;
        [Range(0, 5)] public int integ;
        public int crew;
        public bool vented;
        [Tooltip("What the design's row shows: FIRE, RAD or empty. Step 59: derived from fire and rad below.")]
        public string cond;
        [Tooltip("Step 59 (DAMAGE_README.md): burning; integrity falls, and it spreads to neighbours with air.")]
        public bool fire;
        [Tooltip("Step 59: radiation in the room; its crew die while they stay.")]
        public bool rad;
        [Tooltip("Step 59: its doors are shut (SEAL on an airless room, or VENT), so vacuum cannot spread from it.")]
        public bool doorsShut;
        [System.NonSerialized] public double fireT, radT, fadeT;
    }

    [Header("Power — REACTOR_UNITS, BUS_MAX, BUSES")]
    [Tune("Power & signal")]
    public int reactorUnits = 10;
    [Tune("Power & signal")]
    public int busMax = 5;
    public Bus[] buses =
    {
        new Bus { key = "eng", label = "ENG" },
        new Bus { key = "wep", label = "WEP" },
        new Bus { key = "sen", label = "SEN" },
        new Bus { key = "aux", label = "AUX" },
    };
    public bool shed;

    [Header("Compartments — COMPARTMENTS. Load state: all green, no damage, all sealed (owner, 2026-10-04)")]
    [Tooltip("The design loads damaged (MAG vented, HAB RAD, HGR FIRE) only to show its behaviours; the game starts undamaged.")]
    public Compartment[] compartments =
    {
        new Compartment { key = "mag", tag = "MAG", name = "MAGAZINE", integ = 5, crew = 4 },
        new Compartment { key = "bri", tag = "BRI", name = "BRIDGE", integ = 5, crew = 9 },
        new Compartment { key = "hab", tag = "HAB", name = "HABITATION", integ = 5, crew = 18 },
        new Compartment { key = "hgr", tag = "HGR", name = "HANGAR BAY", integ = 5, crew = 7 },
        new Compartment { key = "rct", tag = "RCT", name = "REACTOR", integ = 5, crew = 6 },
        new Compartment { key = "eng", tag = "ENG", name = "ENGINE ROOM", integ = 5, crew = 8 },
    };

    [System.Serializable]
    public class Track
    {
        public string key, name, klass;
        public int bearing;
        public float rangeKm, speed;
        public bool hostile, own;
        [Tooltip("Bands it emits in: THERM, RAD, UV, EM.")]
        public string[] sig;
        [Tooltip("Exact world position (km), written by TacticalWorld. The plot draws the contact here; "
               + "bearing and range above are the rounded readout values.")]
        [System.NonSerialized] public bool hasWorld;
        [Tooltip("Step 60: taken by our marines (SECURE).")]
        [System.NonSerialized] public bool captured;
        [System.NonSerialized] public double worldX, worldY;
        [Tooltip("Step 48: which way the contact's bow points, degrees (from its model), for the side a slug strikes.")]
        [System.NonSerialized] public double worldHdg;
        [Tooltip("Step 52: which way it is actually going, degrees (its course over the ground).")]
        [System.NonSerialized] public double course;
        [Tooltip("Step 49: destroyed; cold, no longer hostile or heard, a wreck where it died.")]
        [System.NonSerialized] public bool wreck;
        [Tooltip("Step 75: where the sweep last touched it (See). Out of sight, the plot shows it here.")]
        [System.NonSerialized] public bool seen;
        [System.NonSerialized] public double seenX, seenY, seenBrg;
        [System.NonSerialized] public float seenRangeKm;
        [Tooltip("Step 75: its navigation, written by TacticalWorld; in sight, the plot draws it where its model glides.")]
        [System.NonSerialized] public NavSystem nav;
    }

    [Header("Contacts — TRACKS and CLASSES (the board's fixed mock-up contacts)")]
    public Track[] tracks =
    {
        new Track { key = "t07", name = "TRACK 07", klass = "CRUISER", bearing = 48, rangeKm = 24300, speed = 1.10f, hostile = true, sig = new[] { "THERM", "RAD" } },
        new Track { key = "c17", name = "CONTACT 17", klass = "CORVETTE", bearing = 325, rangeKm = 31000, speed = 0.82f, hostile = true, sig = new[] { "RAD", "EM" } },
        new Track { key = "c22", name = "CONTACT 22", klass = "FRIGATE", bearing = 149, rangeKm = 16800, speed = 1.44f, hostile = true, sig = new[] { "THERM", "EM" } },
        new Track { key = "ftr", name = "FTR-2", klass = "FIGHTER", bearing = 241, rangeKm = 12200, speed = 2.60f, own = true, sig = new[] { "THERM", "UV" } },
        new Track { key = "bc1", name = "BC-01", klass = "BOAT", bearing = 71, rangeKm = 9500, speed = 0.41f, own = true, sig = new[] { "UV", "EM" } },
    };

    [Header("Sensors — the board's initial state")]
    public bool dradis = true;
    public bool active;
    [Tooltip("DRADIS gain, 1–5, capped by the SEN bus. Owner 2026-10-25: at load it is at its lowest (power is cold); "
           + "if the SEN bus can no longer carry it, it drops back to 1 and stays there until you raise it again.")]
    [Range(1, 5)] public int senBoost = 1;
    [Tooltip("Sensor range = base + per step x gain (senRangeKm). Owner 2026-10-26: 30,000 km at full gain (was 47,000).")]
    [Tune("Sensors")]
    public float senRangeBaseKm = 12000f, senRangePerStepKm = 3600f;
    [Tooltip("Owner 2026-10-26: inside this range a contact is in sight: its model shows and glides. Outside it, "
           + "the plot shows it only where the sweep last touched it.")]
    [Tune("Sensors")]
    public float visualRangeKm = 10000f;
    [Tooltip("UV hears to this fraction of the range (passiveRangeKm).")]
    [Tune("Sensors")]
    public float uvRangeFactor = 0.55f;
    public string[] spectra = { "THERM", "RAD", "UV", "EM" };
    public string spectrum = "THERM";
    [Tooltip("0..360: the board stores a rounded 360 as 360, not 0 (moveCone).")]
    [Range(0, 360)] public int coneBrg = 62;
    [Tooltip("Half-width of the active scan cone, degrees; NARROW / WIDEN step it.")]
    public int coneArc = 40;
    [Tune("Sensors")]
    public int coneArcMin = 15, coneArcMax = 90, coneArcStep = 10;
    [Header("Signal — how detectable our ship is, 0–100 (Step 35, owner 2026-10-17)")]
    [Tooltip("Our signal: how detectable we are, 0–100. Internal; the player sees a whole percent on the OWN "
           + "EMISSIONS meter, the cells and the alert. It climbs and falls gradually toward SignalTarget.")]
    [UnityEngine.Serialization.FormerlySerializedAs("ownSig")]
    [Range(0, 100)] public float signal;
    [Tooltip("Signal climbs this much per game second toward its target.")]
    [UnityEngine.Serialization.FormerlySerializedAs("signalRise")]
    [Tune("Power & signal")]
    public float signalRise = 9f;
    [Tooltip("Signal falls this much per game second toward its target.")]
    [UnityEngine.Serialization.FormerlySerializedAs("signalFall")]
    [Tune("Power & signal")]
    public float signalFall = 6f;
    [Tooltip("Signal ACTIVE SCAN adds while it is on. Default 35 (reference sim: a scan is worth 3.5 units).")]
    [Tune("Power & signal")]
    public float activeScanSignal = 35f;
    [Tooltip("Own emissions at or above this mean hostile sets have your range (sigNote, blink).")]
    [UnityEngine.Serialization.FormerlySerializedAs("signalTrackedAt")]
    [Tune("Power & signal")]
    public float signalTrackedAt = 45f;
    [Tooltip("Identified by sensors (idd) and classified by the periscope (cls).")]
    public System.Collections.Generic.List<string> identified = new System.Collections.Generic.List<string> { "bc1" };
    public System.Collections.Generic.List<string> classified = new System.Collections.Generic.List<string> { "bc1" };

    /// <summary>JS Math.round: halves round up. Mathf.RoundToInt rounds halves to even
    /// (4.5 → 4), which differs from the board at exact halves (Step 33c verification).</summary>
    public static int JsRound(float x) => Mathf.FloorToInt(x + 0.5f);

    [Header("Comms — SIGNAL, CIPHER_KEY, MESSAGE and the board's initial state")]
    [Range(1, 3)] public int chan = 1;
    [Tooltip("Doubles, as in the browser: float error moved the ±0.3 lock window at 72.5 (Step 33d verification).")]
    public double freq = 34.0; // 1–100 (Unity's Range slider does not take doubles)
    public bool locked;
    public int[] dials = { 0, 0, 0, 0 };
    [Tooltip("The dial just stepped, highlighted for a moment (activeDial); -1 for none.")]
    public int activeDial = -1;
    public int signalChan = 2;
    public double signalFreq = 72.2;
    [Tooltip("LOCK holds when tuned within this of the carrier (lockSignal).")]
    public double lockWindow = 0.3;
    public int[] cipherKey = { 22, 14, 11, 5 };
    public string message = "HOSTILE FLEET INBOUND VECTOR TWO SEVEN NINE HOLD STATION AWAIT RELIEF";

    const string AZ = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>vigenere: shift each letter by the key in turn; dir +1 enciphers, -1 deciphers.</summary>
    public static string Vigenere(string text, int[] key, int dir)
    {
        var sb = new System.Text.StringBuilder(text.Length);
        int li = 0;
        foreach (char ch in text)
        {
            int i = AZ.IndexOf(ch);
            if (i < 0) { sb.Append(ch); continue; }
            int k = key[li % key.Length];
            li++;
            sb.Append(AZ[((i + dir * k) % 26 + 26) % 26]);
        }
        return sb.ToString();
    }

    public string Ciphertext => Vigenere(message, cipherKey, 1);

    /// <summary>What the decrypt screen shows: the ciphertext, or once locked, deciphered by the dials.</summary>
    public string Plaintext => locked ? Vigenere(Ciphertext, dials, -1) : Ciphertext;

    public bool Solved
    {
        get { for (int i = 0; i < dials.Length; i++) if (dials[i] != cipherKey[i]) return false; return true; }
    }

    /// <summary>tuneOff: distance from the carrier; 99 when on the wrong band.</summary>
    public double TuneOff => chan != signalChan ? 99.0 : System.Math.Abs(freq - signalFreq);

    [Header("Engineering and damage control — the board's initial state")]
    public string engSel = "rct";
    [Tooltip("ALERTS index: 0 = CONDITION 3, 1 = CONDITION 2, 2 = CONDITION 1 (set on the orders station).")]
    [Range(0, 2)] public int alert;
    [Tooltip("PARTY_TICKS: a damage-control party works this many game seconds (the design's 6 ticks at x1), then the repair lands.")]
    [Tune("Engineering & orders")]
    [UnityEngine.Serialization.FormerlySerializedAs("partyTicks")]
    public float partySeconds = 6f;
    [Tooltip("The party's compartment and how long it has worked, game seconds; -1 when no party is out.")]
    public string partyComp;
    public float partyT = -1;
    [Tooltip("A thrown repair or order rocker flicks back after this long (flickBack).")]
    [Tune("Engineering & orders")]
    public float flickBackSeconds = 0.4f;
    public System.Collections.Generic.List<string> dcThrown = new System.Collections.Generic.List<string>();
    [Header("Orders station — AUX order pool (ENG_ORDERS)")]
    [Tooltip("AUX order pool, 0..100 (auxCharge).")]
    public double auxCharge;
    [Tooltip("Pool gained per AUX unit per game second.")]
    [Tune("Power & signal")]
    public double auxPerUnit = 3.2;
    [Tune("Engineering & orders")]
    public int orderCostParty = 30, orderCostSeal = 25, orderCostVent = 15;
    [Tooltip("The refused order (orderFail) shows this long.")]
    [Tune("Engineering & orders")]
    public float orderFailSeconds = 0.42f;
    public string orderFail;
    /// <summary>Orders refused so far (Step 40: the board sounds a refusal).</summary>
    [System.NonSerialized] public int orderRefusals;
    [System.NonSerialized] float orderFailUntil;
    [System.NonSerialized] readonly System.Collections.Generic.Dictionary<string, float> flickUntil = new System.Collections.Generic.Dictionary<string, float>();

    [System.Serializable]
    public class Craft { public string tag, pilot, state; [Range(0, 100)] public int fuel; }

    [System.Serializable]
    public class Squad
    {
        public string key, name, shortName, type, role;
        public string[] orders, orderLabels;
        public Craft[] craft;
    }

    [System.Serializable]
    public class Stage { public string id, label; public float rate; public bool on; public float progress; }

    [Header("Hangar — SQUADS, LAUNCH_STAGES and the board's initial state")]
    public Squad[] squads =
    {
        new Squad { key = "gw", name = "INTERCEPTOR", shortName = "INTCP", type = "STARFIGHTER", role = "INTERCEPT / ESCORT",
            orders = new[] { "destroy", "disable", "escort" }, orderLabels = new[] { "DESTROY", "DISABLE", "ESCORT" },
            craft = new[] {
                new Craft { tag = "01", pilot = "HALLORAN", fuel = 100, state = "READY" }, new Craft { tag = "02", pilot = "VOSS", fuel = 100, state = "READY" },
                new Craft { tag = "03", pilot = "IDE", fuel = 96, state = "READY" }, new Craft { tag = "04", pilot = "MARCHETTI", fuel = 88, state = "READY" },
                new Craft { tag = "05", pilot = "REYES", fuel = 41, state = "REARM" }, new Craft { tag = "06", pilot = "OKONKWO", fuel = 0, state = "DOWN" } } },
        new Squad { key = "ih", name = "ASSAULT BOAT", shortName = "ASSLT", type = "ASSAULT BOAT", role = "BOARDING / SECURE",
            orders = new[] { "destroy", "disable", "secure" }, orderLabels = new[] { "DESTROY", "DISABLE", "SECURE" },
            craft = new[] {
                new Craft { tag = "11", pilot = "DRAKOS", fuel = 100, state = "READY" }, new Craft { tag = "12", pilot = "SANTOS", fuel = 100, state = "READY" },
                new Craft { tag = "13", pilot = "ELLERY", fuel = 74, state = "READY" }, new Craft { tag = "14", pilot = "BRENNAN", fuel = 62, state = "REARM" } } },
    };
    public int squad;                 // index into squads ("gw")
    public string order = "destroy";
    [Tooltip("Squads that are out, and the order each launched on (launched).")]
    public System.Collections.Generic.List<string> launchedSquads = new System.Collections.Generic.List<string>();
    public System.Collections.Generic.List<string> launchedOrders = new System.Collections.Generic.List<string>();
    public Stage[] stages =
    {
        new Stage { id = "crew", label = "CREW", rate = 22 }, new Stage { id = "fuel", label = "FUEL", rate = 18 },
        new Stage { id = "ord", label = "ORDNANCE", rate = 14 }, new Stage { id = "evac", label = "EVAC", rate = 20 },
        new Stage { id = "doors", label = "DOORS", rate = 26 }, new Stage { id = "cat", label = "CATAPULT", rate = 12 },
    };
    public bool doorsOpen;
    [Range(0, 359)] public int lbrg = 62;
    [Tooltip("Launch refused while the launch bearing is more than this from the heading (launchBlock).")]
    public float launchConeDeg = 60f;
    [Tooltip("The launch lever (llever), 0..1; launches once past leverFireAt.")]
    [Range(0f, 1f)] public float launchLever;
    public float leverFireAt = 0.9f;

    [System.Serializable]
    public class Mount { public string id, label, shortName, loaded, ammo; public int arc, halfArc; public float rangeKm; }

    [System.Serializable]
    public class WeaponSystem { public string id, label; public string[] mounts; public float shot; }

    [System.Serializable]
    public class Solution
    {
        public bool set; public float rangeKm; public int brg; public float spd;
        [Tooltip("Step 47: dialled by hand from the plot, not by TRACK. It aims at a point in space, not a locked contact.")]
        public bool manual;
        [Tooltip("Step 52: the target's course is known (from a plotted track, or TRACK's lock), so the shot is led.")]
        public bool hasCourse;
        public float course;
        [Tooltip("Where the target was (world km) and when (mission seconds) as the solution was set; the fire computer reckons it forward.")]
        public double wx, wy;
        public float t;
    }
    [Tooltip("A knob turned with no solution starts one here, on the periscope's bearing at speed 0 (owner 2026-10-23).")]
    [Tune("Periscope & fire control")] public float manualStartRangeKm = 10000f;
    /// <summary>The mission clock as of the last tick (HandoffBoardControls writes it).</summary>
    [System.NonSerialized] public float missionNow;

    [Header("Fire control — MOUNTS, SYSTEMS, WARHEADS, SWITCHES and the board's initial state")]
    // Owner 2026-10-24: railgun and missile reach = the DRADIS at full gain (12,000 + 5 × 3,600 km since 2026-10-26), not
    // tied to the gain set now (the design had 30,000 and 40,000). You can dumb-fire that far; a lock still needs a scan.
    public Mount[] mounts =
    {
        new Mount { id = "RGA", arc = 42, halfArc = 80, rangeKm = 30000, label = "RAILGUN TURRET A", shortName = "RG-A", loaded = "2 / 2", ammo = "22" },
        new Mount { id = "RGB", arc = 250, halfArc = 80, rangeKm = 30000, label = "RAILGUN TURRET B", shortName = "RG-B", loaded = "1 / 2", ammo = "19" },
        new Mount { id = "PDA", arc = 42, halfArc = 35, rangeKm = 4000, label = "PDC GROUP A", shortName = "PDC-A", loaded = "BELT", ammo = "2400" },
        new Mount { id = "PDB", arc = 200, halfArc = 35, rangeKm = 4000, label = "PDC GROUP B", shortName = "PDC-B", loaded = "BELT", ammo = "2180" },
        new Mount { id = "VLS", arc = 0, halfArc = 180, rangeKm = 30000, label = "MISSILE VLS", shortName = "VLS", loaded = "8 / 8", ammo = "8" },
    };
    public WeaponSystem[] systems =
    {
        new WeaponSystem { id = "RG", label = "RAILGUN", mounts = new[] { "RGA", "RGB" }, shot = 70 },
        new WeaponSystem { id = "PD", label = "PDC", mounts = new[] { "PDA", "PDB" }, shot = 25 },
        new WeaponSystem { id = "MS", label = "MISSILE", mounts = new[] { "VLS" }, shot = 40 },
    };
    public string[] warheads = { "HE", "EMP", "PENETRATOR" };
    public string[] switches = { "load", "angle", "bearing", "arm" };
    public string sys = "RG";
    public string mount = "RGA";
    public string target = "t07";
    [Tooltip("The arming ladder: LOAD, ANGLE, BEARING, ARM (sw).")]
    public bool[] sw = new bool[4];
    [Tooltip("A firing solution per system, in SYSTEMS order (sol).")]
    public Solution[] sol = { new Solution(), new Solution(), new Solution() };
    public int warhead;
    [Tooltip("Missiles launched together (owner 2026-10-23: set here until the design's salvo stepper has art).")]
    [Tune("Periscope & fire control")] [Range(1, 8)] public int salvo = 2;
    [Tooltip("PDC autoloaders. Step 55: engaged at load (the design starts them off) and set here until the design's LOADERS pushbutton has art (HANDOFF_GAPS row 73); without them PDCs cannot arm.")]
    [Tune("Periscope & fire control")] public bool loaders = true;
    [Tooltip("PDC quadrants FWD, STBD, AFT, PORT (quads).")]
    public bool[] quads = { true, true, false, false };
    [Tooltip("Step 55: PDC groups on AUTO (PDC-A, PDC-B). Arming the ladder with a group selected puts it on AUTO; it then shoots by itself at enemy missiles in its arc.")]
    public bool[] pdcAuto = new bool[2];
    public bool fired;
    [Range(0f, 100f)] public float wepCharge;
    [Tune("Power & signal")]
    public float wepFeedPerUnit = 5f, wepBleed = 3f;
    [Tooltip("The fire lever, 0..1; fires once past leverFireAt with the ladder set.")]
    [Range(0f, 1f)] public float fireLever;
    [Header("Periscope (Step 33h-3)")]
    [Tooltip("Periscope bearing, degrees 0..360 (scopeBrg); the board reads an unset bearing as 62.")]
    public double scopeBrg = 62;
    [Tooltip("Sight magnification, 1..8 in 0.1 steps (scopeZoom).")]
    public double scopeZoom = 1.4;
    [Tooltip("Degrees per px of slew drag (startSlew).")]
    [Tune("Periscope & fire control")]
    public double slewDegPerPx = 0.34;
    [Tooltip("A slew drag ignores the first few px.")]
    [Tune("Periscope & fire control")]
    public double slewDeadPx = 3;
    [Tooltip("px of MAG knob drag per 1x of zoom (magDown).")]
    [Tune("Periscope & fire control")]
    public double magPxPerX = 40;
    public bool slewing;
    public double slewDx;
    [Tooltip("BEARING needs the periscope within this of the target (onBearing).")]
    [Tune("Periscope & fire control")]
    public float onBearingDeg = 4f;

    /// <summary>This ship's navigation: its position, heading, throttle and movement (owner 2026-10-26).</summary>
    [System.NonSerialized] public NavSystem nav;
    /// <summary>Give this ship its navigation. It reads this ship's ENG bus and engine room.</summary>
    public void UseNav(NavSystem n) { nav = n; if (n) n.ship = this; }
    // The ship's position and heading, read by the rules below. Only its NavSystem changes them.
    double ownX => nav.x;
    double ownY => nav.y;
    int hdg => nav.heading;

    // ---- gates ---------------------------------------------------------------------------

    /// <summary>compHealth: a vented compartment counts as integrity 0.</summary>
    public int CompHealth(string key)
    {
        foreach (var c in compartments) if (c.key == key) return c.vented ? 0 : c.integ;
        return 0;
    }

    /// <summary>reactorCap: reactor damage caps the units, floored at 2.</summary>
    public int ReactorCap => Mathf.Max(2, JsRound(CompHealth("rct") / 5f * reactorUnits));

    public int TotalFill
    {
        get { int t = 0; foreach (var b in buses) t += b.fill; return t; }
    }

    /// <summary>Units the core can still give.</summary>
    public int Available => ReactorCap - TotalFill;

    /// <summary>A bus in DRAW that will gain a unit on the next tick (the board's p1–p5).</summary>
    public bool Climbing(Bus b) => b.mode == BusMode.Draw && b.fill < busMax && TotalFill < ReactorCap;

    public int BusUnits(string key) { foreach (var b in buses) if (b.key == key) return b.fill; return 0; }

    public float SenRangeKm => senRangeBaseKm + senBoost * senRangePerStepKm;
    public float PassiveRangeKm => spectrum == "UV" ? SenRangeKm * uvRangeFactor : SenRangeKm;
    public int GainCap => Mathf.Max(1, BusUnits("sen"));

    static int AngDiff(int a, int b) => Mathf.Abs(((a - b) % 360 + 540) % 360 - 180);

    /// <summary>heard: DRADIS on, the contact emits in the chosen band, and it is in range.</summary>
    public bool Heard(Track t) => !t.own && !t.wreck && dradis && System.Array.IndexOf(t.sig, spectrum) >= 0 && t.rangeKm <= PassiveRangeKm;

    /// <summary>coneCovers: active scan on, inside the cone, inside sensor range.</summary>
    public bool ConeCovers(Track t) => active && AngDiff(t.bearing, coneBrg) <= coneArc && t.rangeKm <= SenRangeKm;

    /// <summary>Contacts in band and range for a spectrum key (spectra[].count).</summary>
    public int CountIn(string band)
    {
        float range = band == "UV" ? SenRangeKm * uvRangeFactor : SenRangeKm;
        int n = 0;
        foreach (var t in tracks) if (!t.own && System.Array.IndexOf(t.sig, band) >= 0 && t.rangeKm <= range) n++;
        return n;
    }

    /// <summary>trackers: hostile sets that have your range once emissions reach the threshold.</summary>
    public int Trackers()
    {
        if (signal < signalTrackedAt) return 0;
        int n = 0;
        foreach (var t in tracks) if (!t.own && t.hostile) n++;
        return n;
    }

    public Compartment Comp(string key) { foreach (var c in compartments) if (c.key == key) return c; return null; }
    public int CompIndex(string key) { for (int i = 0; i < compartments.Length; i++) if (compartments[i].key == key) return i; return 0; }
    public bool PartyOut => partyT >= 0;
    public bool MagOk => CompHealth("mag") >= 3;   // shells and warheads are struck up from here
    public bool HgrOk => CompHealth("hgr") >= 3;   // craft cannot be spotted in a wrecked bay
    public bool BriOk => CompHealth("bri") >= 3;   // the plot computer lives on the bridge
    public float EngFactor => CompHealth("eng") / 5f;
    /// <summary>repairRate: Condition 1 doubles repair.</summary>
    public int RepairRate => alert == 2 ? 2 : 1;

    /// <summary>engBlock: why an engineering repair is refused, or null. Step 59 (DAMAGE_README.md): the team
    /// comes from elsewhere, so it needs no crew in the room; it fights a fire, restores the air once the doors
    /// are shut, or repairs. SEAL shuts an airless room's doors. RECALL (the design's DUMMY 1) calls the team back.</summary>
    public string EngBlock(string id)
    {
        var c = Comp(engSel);
        switch (id)
        {
            case "party": return PartyOut ? "DC PARTY COMMITTED \u2014 " + Mathf.CeilToInt(partySeconds - partyT) + " TO RUN"
                                          : c.vented && !c.doorsShut ? "SEAL THE DOORS FIRST"
                                          : !c.fire && !c.vented && c.integ >= 5 ? "COMPARTMENT AT FULL INTEGRITY" : null;
            case "seal": return !c.vented ? "COMPARTMENT ALREADY SEALED" : c.doorsShut ? "DOORS ALREADY SHUT" : null;
            case "vent": return c.vented ? "COMPARTMENT ALREADY VENTED" : null;
            case "reboot": return c.integ < 5 ? null : "SYSTEMS NOMINAL \u2014 NOTHING TO RESTART";
            case "recall": return PartyOut ? null : "NO TEAM OUT";
        }
        return "NOT FITTED";
    }

    // ---- handlers ------------------------------------------------------------------------

    public void CycleComp() { engSel = compartments[(CompIndex(engSel) + 1) % compartments.Length].key; }
    public void PickComp(string key) { if (Comp(key) != null) engSel = key; }

    /// <summary>engRun: refused silently when blocked; otherwise apply and flick back.</summary>
    public bool EngRun(string id, float now)
    {
        if (EngBlock(id) != null) return false;
        ApplyOrder(id);
        FlickBack("eng_" + id, now);
        return true;
    }

    /// <summary>applyOrder: a repair is dispatched, not applied. Step 59 (DAMAGE_README.md): SEAL shuts an
    /// airless room's doors (the air comes back with a repair); VENT dumps the air, so the fire goes, and the
    /// crew and any team inside die; REBOOT adds 1 and no longer clears a condition; RECALL calls the team back.</summary>
    public void ApplyOrder(string id)
    {
        var c = Comp(engSel);
        switch (id)
        {
            case "party": partyComp = engSel; partyT = 0; break;
            case "seal": c.doorsShut = true; break;
            case "vent": Breach(c, 1f); c.doorsShut = true; break;
            case "reboot": c.integ = Mathf.Min(5, c.integ + 1); break;
            case "recall": partyT = -1; partyComp = null; break;
        }
        SyncCond();
    }

    public void FlickBack(string id, float now)
    {
        if (!dcThrown.Contains(id)) dcThrown.Add(id);
        flickUntil[id] = now + flickBackSeconds;
    }

    /// <summary>Let thrown rockers fall back; true if any did.</summary>
    public bool ExpireFlicks(float now)
    {
        bool changed = false;
        if (!string.IsNullOrEmpty(orderFail) && now >= orderFailUntil) { orderFail = null; changed = true; }
        for (int i = dcThrown.Count - 1; i >= 0; i--)
        {
            float until;
            if (!flickUntil.TryGetValue(dcThrown[i], out until) || now >= until) { flickUntil.Remove(dcThrown[i]); dcThrown.RemoveAt(i); changed = true; }
        }
        return changed;
    }


    // ---- plot tools (Step 33i) ---------------------------------------------------------------

    [System.Serializable]
    public class Mark
    {
        public int id;
        [Tooltip("point, ruler or ring.")]
        public string kind;
        [Tooltip("World km, x east, y north (Step 36: marks stay put in space). A ring is centred on our "
               + "ship wherever it is, so its x, y are unused; x2, y2 is its far point.")]
        public double x, y, x2, y2;
        [Tooltip("Mission clock at placement (points).")]
        public float t;
        [Tooltip("Step 46: the POINT ids a RULER's ends snapped to (0 = none).")]
        public int ptA, ptB;
    }

    // ---- the battle space (Step 65, owner 2026-10-25) --------------------------------------------
    // A rectangle centred on the world origin, 100,000 x 100,000 km to start (20 x 20 big squares of 5,000 km at x1).
    // Near the edge a warning; past it, a countdown to get back inside, or the ship is lost (the GDD's desertion rule;
    // Assets/Scripts/WorldBoundary.cs in the old build). Crossing drops the time rate to x1 (HandoffBoardControls).
    [Header("Battle space (Step 65)")]
    [Tooltip("East to west, km, centred on the world origin. Zooming out stops when it fills the plot's width. A multiple of 50,000 keeps the edge on a grid line at every zoom.")]
    [Tune("Arena")] public double arenaWidthKm = 100000;
    [Tooltip("North to south, km. Less than about half the width (the plot's shape) leaves black above and below it at full zoom-out. A multiple of 50,000 keeps the edge on a grid line.")]
    [Tune("Arena")] public double arenaHeightKm = 100000;
    public double ArenaHalfW => arenaWidthKm / 2;
    public double ArenaHalfH => arenaHeightKm / 2;
    [Tooltip("The edge warning shows this far inside the edge, km (half a big square).")]
    [Tune("Arena")] public double arenaWarnKm = 2500;
    [Tooltip("Game seconds to get back inside once out (owner: 30). Straight out at full speed the fastest return takes about 75 (brake, then turn); 30 is enough below about 4.4 km/s or at a shallow crossing. The warning is the time to turn.")]
    [Tune("Arena")] public float arenaReturnSeconds = 30f;
    /// <summary>Game seconds spent outside the battle space, or -1 while inside.</summary>
    [System.NonSerialized] public float outsideFor = -1f;
    /// <summary>Lost for leaving the battle space and not coming back in time.</summary>
    [System.NonSerialized] public bool deserted;
    public bool OutsideArena => System.Math.Abs(ownX) > ArenaHalfW || System.Math.Abs(ownY) > ArenaHalfH;
    public bool NearArenaEdge => !OutsideArena && System.Math.Min(ArenaHalfW - System.Math.Abs(ownX), ArenaHalfH - System.Math.Abs(ownY)) <= arenaWarnKm;

    /// <summary>Each tick, after we move: count the time outside, and lose the ship when it runs out.
    /// True on the tick we cross out.</summary>
    public bool TickArena(double dt)
    {
        if (destroyed) return false;
        if (!OutsideArena) { outsideFor = -1f; return false; }
        bool crossed = outsideFor < 0f;
        outsideFor = crossed ? 0f : outsideFor + (float)dt;
        if (outsideFor >= arenaReturnSeconds) { destroyed = true; deserted = true; }
        return crossed;
    }

    [Header("Plot marks (§8.4)")]
    public System.Collections.Generic.List<Mark> marks = new System.Collections.Generic.List<Mark>();
    [System.NonSerialized] public Mark draft;
    public int markSeq;
    [Tooltip("A RULER or COMPASS drag shorter than this (layer px) is discarded.")]
    [Tune("Plot tools")]
    public double markMinLen = 4;

    /// <summary>plotDown: POINT stamps a mark; RULER starts a line at the press; COMPASS a ring on own ship.</summary>
    public bool MarkDown(string tool, double x, double y, float missionT)
    {
        if (tool == "POINT") { marks.Add(new Mark { id = ++markSeq, kind = "point", x = x, y = y, t = missionT }); return false; }
        if (tool == "RULER") draft = new Mark { kind = "ruler", x = x, y = y, x2 = x, y2 = y };
        else if (tool == "COMPASS") draft = new Mark { kind = "ring", x = ownX, y = ownY, x2 = x, y2 = y };
        else return false;
        return true;
    }
    public void MarkMove(double x, double y) { if (draft != null) { draft.x2 = x; draft.y2 = y; } }
    /// <summary>plotUp: a draft shorter than 4 layer px (given in km by the plot) is dropped; otherwise it becomes a mark.</summary>
    public void MarkUp(double minLenKm)
    {
        var d = draft;
        draft = null;
        if (d == null || MarkLen(d) < minLenKm - 1e-6) return; // tolerance: the km conversion can land a hair under an exact 4 px (Step 36 verification)
        d.id = ++markSeq;
        marks.Add(d);
        if (Sol.set && Sol.manual) ApplyTrack(Sol); // Step 52: a new track feeds the hand-dialled solution
    }
    /// <summary>A mark's start in world km: a ring starts at our ship, wherever it is now.</summary>
    public double MarkX(Mark m) => m.kind == "ring" ? ownX : m.x;
    public double MarkY(Mark m) => m.kind == "ring" ? ownY : m.y;
    /// <summary>Length in km.</summary>
    public double MarkLen(Mark m) { double dx = m.x2 - MarkX(m), dy = m.y2 - MarkY(m); return System.Math.Sqrt(dx * dx + dy * dy); }
    public void EraseMark(int id) => marks.RemoveAll(m => m.id == id);

    /// <summary>Step 46: the nearest POINT within reach of a spot (km), or null — a RULER snaps to it.</summary>
    public Mark PointNear(double x, double y, double withinKm)
    {
        Mark best = null;
        double bestD = withinKm;
        foreach (var m in marks)
        {
            if (m.kind != "point") continue;
            double d = System.Math.Sqrt((m.x - x) * (m.x - x) + (m.y - y) * (m.y - y));
            if (d <= bestD) { best = m; bestD = d; }
        }
        return best;
    }

    /// <summary>Step 46 (owner 2026-10-23): a RULER from one POINT to another, with different times, is a
    /// target's track. Course runs from the earlier POINT to the later; speed is distance over time.</summary>
    public bool TrackLeg(Mark m, out Mark from, out Mark to, out double courseDeg, out double speedKms)
    {
        from = to = null; courseDeg = speedKms = 0;
        if (m.kind != "ruler" || m.ptA == 0 || m.ptB == 0) return false;
        var a = marks.Find(k => k.id == m.ptA);
        var b = marks.Find(k => k.id == m.ptB);
        if (a == null || b == null || a.t == b.t) return false;
        if (a.t > b.t) { var swap = a; a = b; b = swap; }
        double dx = b.x - a.x, dy = b.y - a.y;
        from = a; to = b;
        courseDeg = (System.Math.Atan2(dx, dy) * 180 / System.Math.PI + 360) % 360;
        speedKms = System.Math.Sqrt(dx * dx + dy * dy) / (b.t - a.t);
        return true;
    }

    /// <summary>validSet: hostile contacts inside the current mount's range and arc.</summary>
    public bool ValidTarget(string key)
    {
        var m = MountOf(mount);
        var t = TrackOf(key);
        return m != null && t != null && t.hostile && t.rangeKm <= m.rangeKm && AngDiff(t.bearing, m.arc) <= m.halfArc;
    }
    /// <summary>pickTarget: a valid contact becomes the target; the ladder, lever and fired reset.</summary>
    public void PickTarget(string key)
    {
        if (!ValidTarget(key)) return;
        target = key;
        fired = false;
        for (int i = 0; i < sw.Length; i++) sw[i] = false;
        fireLever = 0f;
    }
    /// <summary>lookAt: lower the scope (by the caller) and put it on the contact's bearing.</summary>
    public bool LookAt(string key) { var t = TrackOf(key); if (t == null) return false; scopeBrg = t.bearing; slewDx = 0; return true; }

    // ---- orders station (Step 33h-2) -------------------------------------------------------

    /// <summary>doAlert: CONDITION 3 → 2 → 1 → 3.</summary>
    public void DoAlert() { alert = (alert + 1) % 3; }

    /// <summary>ENG_ORDERS cost for a fitted order, or -1 (FIRE, BLKHD, SCRUB are not fitted).</summary>
    public int OrderCost(string id) => id == "party" ? orderCostParty : id == "seal" ? orderCostSeal : id == "vent" ? orderCostVent : -1;

    /// <summary>orderBlock: the orders station's refusals. Step 59: the same rules as engineering's rockers
    /// (the design's two sets disagreed on crew; DAMAGE_README.md has one set of rules).</summary>
    public string OrderBlock(string id) => id == "party" || id == "seal" || id == "vent" ? EngBlock(id) : null;

    /// <summary>runOrder: refused (too little AUX, or orderBlock) → the failure shows for 420 ms;
    /// otherwise spend the charge and apply. The rocker flicks back either way.</summary>
    public void RunOrder(string id, float now)
    {
        int cost = OrderCost(id);
        if (cost < 0) return;
        if (auxCharge < cost || OrderBlock(id) != null) { orderFail = id; orderFailUntil = now + orderFailSeconds; orderRefusals++; }
        else { auxCharge = System.Math.Max(0, auxCharge - cost); ApplyOrder(id); }
        FlickBack(id, now);
    }

    /// <summary>The tick: AUX charges by units x 3.2 per game second to 100 (owner 2026-10-24: every rate in game time).</summary>
    public void TickAux(double dt)
    {
        double rate = BusUnits("aux") * auxPerUnit * dt;
        if (rate > 0 && auxCharge < 100) auxCharge = System.Math.Min(100, auxCharge + rate);
    }

    public bool FlightOpsReleased => alert >= 1;
    public bool WeaponsReleased => alert >= 1;
    public Squad CurrentSquad => squads[squad];
    public bool Launched(Squad q) => launchedSquads.Contains(q.key);
    public string LaunchedOrder(Squad q) { int i = launchedSquads.IndexOf(q.key); return i >= 0 ? launchedOrders[i] : null; }
    public int SquadReady(Squad q) { int n = 0; foreach (var c in q.craft) if (c.state == "READY") n++; return n; }
    public bool AllStagesDone { get { foreach (var g in stages) if (g.progress < 100f) return false; return true; } }
    /// <summary>launchDev: how far the launch bearing is from the heading.</summary>
    public int LaunchDev => Mathf.Abs(((lbrg - hdg + 540) % 360) - 180);

    public string StageBlock(string id)
    {
        if (id == "crew" && !FlightOpsReleased) return "FLIGHT OPS NOT RELEASED \u2014 HELD AT DC-1 ALERT CONDITION 3";
        if (id == "fuel" && !HgrOk) return "HANGAR BAY DAMAGED \u2014 FUEL LINES DOWN";
        if (id == "ord" && !MagOk) return "MAGAZINE HIT \u2014 NO ORDNANCE";
        if (id == "doors" && !doorsOpen) return "BAY DOOR MASTER SHUT";
        if (id == "cat" && BusUnits("aux") < 1) return "NO AUX UNITS \u2014 CATAPULT WILL NOT CHARGE";
        return null;
    }

    public string LaunchBlock()
    {
        var q = CurrentSquad;
        if (q == null) return "NO SQUADRON SELECTED";
        if (Launched(q)) return null;
        if (!FlightOpsReleased) return "FLIGHT OPS NOT RELEASED \u2014 HELD AT DC-1 ALERT CONDITION 3";
        if (!HgrOk) return "HANGAR BAY DAMAGED \u2014 CANNOT SPOT CRAFT";
        if (SquadReady(q) == 0) return "NO CRAFT READY";
        if (q.key == "ih" && marines < 1) return "NO MARINES ABOARD"; // Step 60
        if (LaunchDev > launchConeDeg) return "BEARING OUTSIDE LAUNCH CONE";
        return null;
    }

    // ---- fire control ----------------------------------------------------------------------

    public WeaponSystem Sys { get { foreach (var y in systems) if (y.id == sys) return y; return systems[0]; } }
    public int SysIndex { get { for (int i = 0; i < systems.Length; i++) if (systems[i].id == sys) return i; return 0; } }
    public Mount MountOf(string id) { foreach (var m in mounts) if (m.id == id) return m; return null; }
    public Track TrackOf(string key) { foreach (var t in tracks) if (t.key == key) return t; return null; }
    /// <summary>Keep only the tracks a world contact drives (Step 34, TacticalWorld).</summary>
    public void KeepTracks(System.Collections.Generic.ICollection<string> keys)
    {
        tracks = System.Array.FindAll(tracks, t => keys.Contains(t.key));
        identified.RemoveAll(k => !keys.Contains(k));
        classified.RemoveAll(k => !keys.Contains(k));
    }
    public Solution Sol => sol[SysIndex];
    public float ShotCost => Sys != null ? Sys.shot : 50f;
    public bool WepReady => BusUnits("wep") >= 2 && MagOk && WeaponsReleased;
    public string TrackIdFor()
    {
        var t = TrackOf(target);
        if (t == null || !identified.Contains(t.key)) return null;
        return "TK-" + t.bearing.ToString("000") + "-" + t.key.ToUpperInvariant();
    }
    /// <summary>offBearing: target bearing minus periscope bearing, wrapped to ±180.</summary>
    public double? OffBearing() { var t = TrackOf(target); if (t == null) return null; return ((t.bearing - scopeBrg + 540) % 360) - 180; }
    public bool OnBearing
    {
        get
        {
            // a hand-dialled solution aims at a point: BEARING wants the periscope on the dialled bearing
            if (Sol.set && Sol.manual) return AngDiffD(hdg + Sol.brg, scopeBrg) <= onBearingDeg;
            var o = OffBearing(); return o.HasValue && System.Math.Abs(o.Value) <= onBearingDeg;
        }
    }

    // ---- periscope -----------------------------------------------------------------------

    static double AngDiffD(double a, double b) => System.Math.Abs(((a - b) % 360 + 540) % 360 - 180);
    /// <summary>scopeOn: the scope is down and within 4° of the contact.</summary>
    public bool ScopeOn(Track t, bool scopeDown) => scopeDown && AngDiffD(t.bearing, scopeBrg) <= 4;
    /// <summary>scopeOffDeg: degrees from the target to the sight; 999 with no target.</summary>
    public double ScopeOffDeg { get { var t = TrackOf(target); return t == null ? 999 : AngDiffD(t.bearing, scopeBrg); } }
    public bool ScopeOnAxis => ScopeOffDeg <= 4;
    /// <summary>scopeSubject: the first heard contact in the sight.</summary>
    public Track ScopeSubject(bool scopeDown) { foreach (var t in tracks) if (Heard(t) && ScopeOn(t, scopeDown)) return t; return null; }
    public void Classify(bool scopeDown) { var t = ScopeSubject(scopeDown); if (t != null && !classified.Contains(t.key)) classified.Add(t.key); }

    /// <summary>moveSlew: bearing = start + dx x 0.34, wrapped; ignores the first 3 px. Any slew
    /// releases BEARING and ARM (not the rungs after them), rests the lever and clears fired.</summary>
    public void Slew(double brg0, double dx)
    {
        if (System.Math.Abs(dx) < slewDeadPx) return;
        scopeBrg = ((brg0 + dx * slewDegPerPx) % 360 + 360) % 360;
        slewDx = dx;
        sw[2] = false; sw[3] = false;
        fireLever = 0f;
        fired = false;
    }
    public void EndSlew() { slewing = false; slewDx = 0; }
    /// <summary>slewTo: jump to the target's bearing; the switches are left as they are.</summary>
    public void SlewTo()
    {
        if (Sol.set && Sol.manual) { scopeBrg = ((hdg + Sol.brg) % 360 + 360) % 360; slewing = false; slewDx = 0; return; } // onto the dialled bearing
        var t = TrackOf(target); if (t == null) return; scopeBrg = t.bearing; slewing = false; slewDx = 0;
    }
    /// <summary>magDown: zoom = start + dx / 40, 1..8, rounded to 0.1.</summary>
    public void SetZoom(double z0, double dx) => scopeZoom = JsRound(System.Math.Max(1, System.Math.Min(8, z0 + dx / magPxPerX)) * 10) / 10.0;
    public bool AllSet
    {
        get
        {
            if (!Sol.set || (!Sol.manual && TrackOf(target) == null)) return false; // a hand-dialled shot needs no locked contact
            if (sys == "PD" && !loaders) return false;
            foreach (var b in sw) if (!b) return false;
            return true;
        }
    }

    // Step 55: a PDC group on AUTO keeps its ladder armed whenever it is the one selected
    void ClearSwitches() { bool auto = PdcGroup >= 0 && pdcAuto[PdcGroup]; for (int i = 0; i < sw.Length; i++) sw[i] = auto; }
    /// <summary>The selected PDC group (0 = PDC-A, 1 = PDC-B), or -1 when PDC is not selected.</summary>
    int PdcGroup => sys == "PD" ? System.Array.IndexOf(Sys.mounts, mount) : -1;

    /// <summary>pickSystem via cycleSystem: next system; keep the mount if it belongs, else its first.</summary>
    public void CycleSystem()
    {
        var next = systems[(SysIndex + 1) % systems.Length];
        sys = next.id;
        if (System.Array.IndexOf(next.mounts, mount) < 0) mount = next.mounts[0];
        ClearSwitches(); fired = false;
    }

    public void CycleMount()
    {
        var y = Sys;
        if (y.mounts.Length < 2) return;
        int i = System.Array.IndexOf(y.mounts, mount);
        mount = y.mounts[(i + 1) % y.mounts.Length];
        ClearSwitches(); fired = false;
    }

    public void CycleWarhead() { warhead = (warhead + 1) % warheads.Length; ClearSwitches(); }
    public void StepSalvo(int d) { salvo = Mathf.Max(1, Mathf.Min(8, salvo + d)); }
    public void ToggleLoaders() { loaders = !loaders; if (!loaders) pdcAuto[0] = pdcAuto[1] = false; ClearSwitches(); }
    public void ToggleQuad(int i) { quads[i] = !quads[i]; }

    /// <summary>computeSol: needs an identified target and the bridge; stores range, relative
    /// bearing and speed for the current system and clears the ladder.</summary>
    public void ComputeSol()
    {
        var t = TrackOf(target);
        if (t == null || !identified.Contains(t.key) || !BriOk) return;
        int rel = ((t.bearing - hdg + 540) % 360) - 180;
        // Step 52: an active lock knows its true course and speed, and where it is now, so TRACK's shot is led
        sol[SysIndex] = new Solution { set = true, rangeKm = t.rangeKm, brg = rel, spd = t.speed,
            hasCourse = t.speed > 0.01f, course = (float)t.course, wx = t.worldX, wy = t.worldY, t = missionNow };
        ClearSwitches(); fired = false;
    }

    [Tooltip("The SPD knob's step, km/s (the design's 0.05, x5 with the speeds in Step 63).")]
    [Tune("Periscope & fire control")] public float spdStep = 0.25f;

    /// <summary>nudgeSol: RNG ±100 km, BRG ±1° (±180), SPD ±spdStep (2 dp; the design had 0.05, x5 in Step 63); clears the ladder.</summary>
    public void NudgeSol(string field, int d)
    {
        var cur = Sol;
        if (!cur.set)
        {
            // Step 47 (owner 2026-10-23): a knob starts a hand-dialled solution, from the plot
            cur.set = true; cur.manual = true;
            cur.rangeKm = manualStartRangeKm;
            cur.brg = (JsRound(scopeBrg) - hdg + 540) % 360 - 180; // relative, as TRACK stores it
            cur.spd = 0f;
            ApplyTrack(cur);
        }
        if (field == "rng") cur.rangeKm = Mathf.Max(0f, cur.rangeKm + d * 100f);
        if (field == "brg") cur.brg = Mathf.Max(-180, Mathf.Min(180, cur.brg + d));
        if (field == "spd") cur.spd = Mathf.Max(0f, (float)System.Math.Round(cur.spd + d * spdStep, 2, System.MidpointRounding.AwayFromZero));
        if (cur.manual) Fix(cur); // the dialled point is where the target is now
        ClearSwitches();
    }

    /// <summary>flip: releasing a switch releases it and every one after (and the lever);
    /// throwing needs the one before, and LOAD/BEARING/ARM their gates.</summary>
    public void Flip(int i)
    {
        if (sw[i])
        {
            for (int j = i; j < sw.Length; j++) sw[j] = false;
            fired = false; fireLever = 0f;
            if (PdcGroup >= 0) pdcAuto[PdcGroup] = false; // Step 55: dropping the ladder takes the group off AUTO
            return;
        }
        if (i > 0 && !sw[i - 1]) return;
        if (RungGated(i)) return;
        sw[i] = true; fired = false;
        if (i == 3 && PdcGroup >= 0) pdcAuto[PdcGroup] = true; // Step 55: ARM puts the PDC group on AUTO
    }

    /// <summary>A rung's gate: LOAD needs the WEP bus and a solution, ANGLE the turret to bear (Step 48),
    /// BEARING the scope on axis, ARM the charge. Step 55: a PDC group needs no solution or bearing, only its
    /// autoloaders: on AUTO it fires by itself at what enters its arc.</summary>
    public bool RungGated(int i)
    {
        bool pd = sys == "PD";
        return (i == 0 && (!WepReady || (pd ? !loaders : !Sol.set)))
            || (i == 1 && !pd && !InArc) || (i == 2 && !pd && !OnBearing) || (i == 3 && wepCharge < ShotCost);
    }

    /// <summary>The lever passing 0.9 with the ladder set: rounds away, charge spent (drawWep).
    /// Step 48: refused below the shot's cost; firing releases ARM, one shot per arming (spec 8.4.b);
    /// a railgun shot is a slug that flies (TickRounds); firing spikes our signal (GDD p.42).</summary>
    public void Fire()
    {
        if (sys == "PD") return; // Step 55: PDCs on AUTO fire by themselves; the lever does nothing
        if (!AllSet) return; // moveSlew fires on every new pull while allSet() holds; the panel's per-pull latch stops repeats (Step 33g verification)
        if (wepCharge < ShotCost) return;
        if (sys == "MS" && !MissileReady) return;
        fired = true;
        wepCharge = Mathf.Max(0f, wepCharge - ShotCost);
        sw[3] = false;
        shotsFired++;
        var m = MountOf(mount);
        if (sys == "RG" && m != null)
        {
            double b = LeadBearing() * System.Math.PI / 180;
            Rounds.Add(new Round { x = ownX, y = ownY, dx = System.Math.Sin(b), dy = System.Math.Cos(b), maxKm = m.rangeKm, speed = slugSpeedKms });
            shotReport = null;
            signal = Mathf.Min(100f, signal + fireSignalSpike);
        }
        if (sys == "MS" && m != null)
        {
            // a salvo, spread a little either side of the target's bearing; each missile then homes on it
            var tgt = TrackOf(target);
            int n = Mathf.Min(salvo, vlsCells);
            for (int i = 0; i < n; i++)
            {
                double b = (tgt.bearing + (i - (n - 1) / 2.0) * missileSpreadDeg) * System.Math.PI / 180;
                Rounds.Add(new Round { x = ownX, y = ownY, dx = System.Math.Sin(b), dy = System.Math.Cos(b), maxKm = m.rangeKm,
                                       speed = missileSpeedKms, missile = true, targetKey = tgt.key, warhead = warhead });
            }
            vlsCells -= n;
            shotReport = null;
            signal = Mathf.Min(100f, signal + fireSignalSpike);
        }
    }

    /// <summary>Step 53 (GDD p.45): missiles need a locked target — identified by an active scan — and a cell left.</summary>
    public bool MissileReady { get { var t = TrackOf(target); return t != null && !t.wreck && identified.Contains(t.key) && vlsCells > 0; } }

    // ---- leading a moving target (Step 52, owner 2026-10-23) ---------------------------------

    /// <summary>The latest plotted track (a RULER from one POINT to another) gives the solution its course,
    /// and its speed into SPD.</summary>
    public void ApplyTrack(Solution cur)
    {
        Mark latest = null;
        Mark f, g; double crs, spd;
        foreach (var m in marks) if (m.kind == "ruler" && (latest == null || m.id > latest.id) && TrackLeg(m, out f, out g, out crs, out spd)) latest = m;
        if (latest == null) return;
        TrackLeg(latest, out f, out g, out crs, out spd);
        cur.hasCourse = true;
        cur.course = (float)crs;
        cur.spd = (float)System.Math.Round(spd, 2, System.MidpointRounding.AwayFromZero);
    }

    // the dialled range and bearing as a world point, now
    void Fix(Solution cur)
    {
        double b = (hdg + cur.brg) * System.Math.PI / 180;
        cur.wx = ownX + System.Math.Sin(b) * cur.rangeKm;
        cur.wy = ownY + System.Math.Cos(b) * cur.rangeKm;
        cur.t = missionNow;
    }

    /// <summary>Where to point the slug. With a course, reckon the target forward from where the solution
    /// put it to now, then aim where it will be when the slug arrives. Otherwise straight down the dialled
    /// bearing, as before.</summary>
    public double LeadBearing()
    {
        var cur = Sol;
        double straight = hdg + cur.brg;
        if (!cur.hasCourse || cur.spd <= 0.001f) return straight;
        double c = cur.course * System.Math.PI / 180, vx = cur.spd * System.Math.Sin(c), vy = cur.spd * System.Math.Cos(c);
        double age = System.Math.Max(0, missionNow - cur.t);
        double tx = cur.wx + vx * age - ownX, ty = cur.wy + vy * age - ownY;
        double a = vx * vx + vy * vy - slugSpeedKms * slugSpeedKms, bq = 2 * (tx * vx + ty * vy), cq = tx * tx + ty * ty;
        double disc = bq * bq - 4 * a * cq;
        if (a >= 0 || disc < 0) return straight; // can't catch it
        double tHit = (-bq - System.Math.Sqrt(disc)) / (2 * a);
        if (tHit <= 0) return straight;
        return (System.Math.Atan2(tx + vx * tHit, ty + vy * tHit) * 180 / System.Math.PI + 360) % 360;
    }

    // ---- rounds in flight (Step 48, owner 2026-10-23) ------------------------------------------

    [System.Serializable]
    public class Round
    {
        public double x, y, dx, dy, travelled, maxKm;
        public double speed = 50;
        [Tooltip("Step 53: a missile homes on its target and carries a warhead (an index into warheads).")]
        public bool missile;
        public string targetKey;
        public int warhead;
        [Tooltip("Step 55: PDC groups that have had their one chance at this missile (bit 0 = PDC-A, bit 1 = PDC-B).")]
        public int pdcTried;
    }
    [System.Serializable]
    public class HitRadius { public string klass; public float km; }

    [System.NonSerialized] private System.Collections.Generic.List<Round> rounds;
    public System.Collections.Generic.List<Round> Rounds => rounds ?? (rounds = new System.Collections.Generic.List<Round>());
    [Tooltip("Railgun slug speed, km/s (Step 63: 5x the 50 that matched the prototype's slowed ship). About 100 s to 24,300 km.")]
    [Tune("Periscope & fire control")] public double slugSpeedKms = 250;
    [Tooltip("Missile speed, km/s (owner 2026-10-23: slower than a slug, but it homes; Step 63: x5). About 2 minutes to 12,000 km.")]
    [Tune("Periscope & fire control")] public double missileSpeedKms = 100;
    [Tooltip("How fast a missile can turn toward its target, degrees per mission second (Step 63: x5).")]
    [Tune("Periscope & fire control")] public double missileTurnDegPerS = 6.5;
    [Tooltip("A salvo leaves the launcher fanned this many degrees apart.")]
    [Tune("Periscope & fire control")] public double missileSpreadDeg = 3;
    [Tooltip("VLS cells left (the design's 8 / 8).")]
    public int vlsCells = 8;
    [Tooltip("A slug hits a ship it passes within this distance of, by class (the prototype's radii, km). Others: 150.")]
    [Tune("Periscope & fire control")] public HitRadius[] hitRadii =
    {
        new HitRadius { klass = "FRIGATE", km = 95 }, new HitRadius { klass = "DESTROYER", km = 135 },
        new HitRadius { klass = "CRUISER", km = 210 }, new HitRadius { klass = "CARRIER", km = 600 },
    };
    [Tooltip("Firing the railgun spikes our signal by this much (GDD p.42); it falls back at the usual rate.")]
    [Tune("Power & signal")] public float fireSignalSpike = 25f;
    /// <summary>The last shot's outcome: "HIT · TRACK 07 · STBD", "MISS", or null while none has landed.</summary>
    [System.NonSerialized] public string shotReport;
    [System.NonSerialized] public bool lastShotHit;
    [System.NonSerialized] public int shotsFired, shotResults;

    /// <summary>The damage model of the ship behind a track (TacticalWorld supplies it), or null.</summary>
    [System.NonSerialized] public System.Func<string, HandoffBoardState> ShipOf;
    [System.NonSerialized] public int kills;

    // ---- hull sides and damage (Step 49, owner 2026-10-23) --------------------------------------
    // The same model for every ship, ours included (the enemy is our ship with other stats).

    public static readonly string[] Sides = { "PROW", "STBD", "AFT", "PORT" };
    // Step 58 (owner 2026-10-24, design audit §09.4): one fixed room behind each face — bow magazine,
    // starboard hangar, stern engines, port reactor. Bridge and habitation sit inboard and are never hit
    // directly. Replaces the 18 Oct map (FTUE_PLAN §4.4). DAMAGE_README.md.
    static readonly string[] BehindSide = { "mag", "hgr", "eng", "rct" };
    /// <summary>The room behind face i (PROW, STBD, AFT, PORT).</summary>
    public static string RoomBehind(int i) => BehindSide[i];
    [Tooltip("Armour per side: PROW, STBD, AFT, PORT. At 0 the side is open (owner: prow and flanks take 2 slugs, the aft 1).")]
    [Tune("Damage")] public float[] sideArmourMax = { 100f, 100f, 50f, 100f };
    [Tooltip("Armour one railgun slug takes off the side it hits.")]
    [Tune("Damage")] public float slugArmourDamage = 50f;
    [Tooltip("Integrity one slug through an open side takes off one compartment behind it (owner: 2 of 5).")]
    [Tune("Damage")] public int slugCompartmentDamage = 2;
    [System.NonSerialized] private float[] sideArmour;
    public float[] SideArmour => sideArmour ?? (sideArmour = (float[])sideArmourMax.Clone());
    /// <summary>Destroyed: its reactor is at zero (the kill condition, FTUE §4.4), or it stayed
    /// outside the battle space too long (deserted).</summary>
    [System.NonSerialized] public bool destroyed;
    [System.NonSerialized] private System.Random dice;

    /// <summary>A slug strikes this ship's side. Armour first; once the side is open, the room behind it
    /// loses integrity. Returns the report's tail (" OPEN", " · RCT −2").</summary>
    public string TakeSlug(string side)
    {
        int i = System.Array.IndexOf(Sides, side);
        if (i < 0 || destroyed) return "";
        var armour = SideArmour;
        if (armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - slugArmourDamage);
            return armour[i] <= 0f ? " OPEN" : " ARMOUR " + JsRound(armour[i] / sideArmourMax[i] * 100f) + "%";
        }
        return Behind(i);
    }

    /// <summary>One fighter's attack run on this ship (Step 54). DESTROY: the side facing it, then the room
    /// behind. DISABLE: always the stern, so the engine room (Step 58).</summary>
    public string TakeFighterRun(string side, bool disable, float armourDamage, int compartmentDamage)
    {
        if (destroyed) return "";
        int i = disable ? 2 : System.Array.IndexOf(Sides, side);
        if (i < 0) return "";
        var armour = SideArmour;
        if (armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - armourDamage);
            return " " + Sides[i] + (armour[i] <= 0f ? " OPEN" : " ARMOUR " + JsRound(armour[i] / sideArmourMax[i] * 100f) + "%");
        }
        return " " + Sides[i] + Behind(i, compartmentDamage, "fighter");
    }

    // The room behind side i loses the damage given. A wrecked room takes no more, but the hit can still start
    // a condition there (Step 59). The ship is destroyed when its reactor's structure reaches 0 (any ship, ours
    // too: owner 2026-10-24).
    string Behind(int i) => Behind(i, slugCompartmentDamage, "slug");
    string Behind(int i, int damage, string weapon)
    {
        var hit = Comp(BehindSide[i]);
        if (hit == null) return "";
        string started = Strike(hit, weapon);
        if (hit.integ <= 0) return " · " + hit.tag + " WRECKED" + started;
        hit.integ = Mathf.Max(0, hit.integ - damage);
        if (Comp("rct").integ <= 0) destroyed = true;
        return " · " + hit.tag + " −" + damage + started;
    }

    // ---- conditions: fire, vacuum, radiation (Step 59, owner 2026-10-24, DAMAGE_README.md) -------

    [Header("Conditions (Step 59, DAMAGE_README.md): every rate per game second")]
    [Tooltip("Chance a hit through open armour starts a fire: an HE warhead, a penetrator.")]
    [Tune("Damage control")] [Range(0f, 1f)] public float fireChanceHE = 0.6f, fireChancePEN = 0.4f;
    [Tooltip("Chance a railgun slug through open armour breaches the room (it loses its air).")]
    [Tune("Damage control")] [Range(0f, 1f)] public float breachChanceSlug = 0.5f;
    [Tooltip("Chance a hit on the reactor room (not a fighter's) leaks radiation.")]
    [Tune("Damage control")] [Range(0f, 1f)] public float radChanceReactor = 0.5f;
    [Tooltip("A fire takes 1 integrity off its room this often, game seconds.")]
    [Tune("Damage control")] public float fireDamageSeconds = 30f;
    [Tooltip("Chance per second that a fire, a vacuum or radiation spreads into each neighbouring room.")]
    [Tune("Damage control")] [Range(0f, 1f)] public float fireSpreadPerSecond = 0.01f, vacuumSpreadPerSecond = 0.01f, radSpreadPerSecond = 0.01f;
    [Tooltip("Share of a room's crew lost when it is breached (VENT loses all of them).")]
    [Tune("Damage control")] [Range(0f, 1f)] public float breachCrewLoss = 0.5f;
    [Tooltip("Radiation kills one crew member in its room this often, game seconds.")]
    [Tune("Damage control")] public float radCrewLossSeconds = 20f;
    [Tooltip("The reactor stops leaking once repaired to this integrity (or vented); then each room's radiation fades after radFadeSeconds.")]
    [Tune("Damage control")] public int reactorContainedAt = 3;
    [Tune("Damage control")] public float radFadeSeconds = 90f;
    /// <summary>Teams lost when their room vented around them.</summary>
    [System.NonSerialized] public int partiesLost;

    // The deck plan (owner 2026-10-24): which rooms touch which.
    static readonly System.Collections.Generic.Dictionary<string, string[]> Neighbours = new System.Collections.Generic.Dictionary<string, string[]>
    {
        { "mag", new[] { "bri", "rct", "hgr" } },        { "bri", new[] { "mag", "hab", "rct", "hgr" } },
        { "hab", new[] { "bri", "eng", "rct", "hgr" } }, { "eng", new[] { "hab", "rct", "hgr" } },
        { "rct", new[] { "mag", "bri", "hab", "eng" } }, { "hgr", new[] { "mag", "bri", "hab", "eng" } },
    };

    // What a hit through open armour starts: the weapon and the room decide (owner).
    string Strike(Compartment c, string weapon)
    {
        string started = "";
        float fireChance = weapon == "HE" ? fireChanceHE : weapon == "PEN" ? fireChancePEN : 0f;
        if (!c.vented && !c.fire && Roll(fireChance)) { c.fire = true; c.fireT = 0; started += " · FIRE"; }
        if (weapon == "slug" && !c.vented && Roll(breachChanceSlug)) { Breach(c, breachCrewLoss); started += " · BREACH"; }
        if (c.key == "rct" && weapon != "fighter" && !c.rad && Roll(radChanceReactor)) { c.rad = true; c.fadeT = 0; started += " · RADIATION"; }
        SyncCond();
        return started;
    }

    bool Roll(double p)
    {
        if (p <= 0) return false;
        if (dice == null) dice = new System.Random(49);
        return dice.NextDouble() < p;
    }

    // A room loses its air: the fire goes out, that share of its crew die, and a team working there is lost.
    void Breach(Compartment c, float crewLost)
    {
        c.vented = true;
        c.fire = false;
        c.crew -= Mathf.CeilToInt(c.crew * crewLost);
        if (PartyOut && partyComp == c.key) { partyT = -1; partyComp = null; partiesLost++; }
    }

    // The design's row shows one condition: fire before radiation.
    void SyncCond() { foreach (var c in compartments) c.cond = c.fire ? "FIRE" : c.rad ? "RAD" : null; }

    /// <summary>Each tick: fires burn their rooms, radiation kills crew, and all three spread to neighbours
    /// (fire only into rooms with air; vacuum only through open doors; radiation only while the reactor leaks).
    /// Radiation fades once the reactor is contained. A reactor burned to 0 destroys the ship.</summary>
    public void TickConditions(double dt)
    {
        if (destroyed) return;
        var rct = Comp("rct");
        bool leaking = rct != null && rct.rad && rct.integ < reactorContainedAt && !rct.vented;
        // spread from what stood at the start of the tick, so a condition moves one room per tick at most
        var burning = System.Array.FindAll(compartments, c => c.fire);
        var airless = System.Array.FindAll(compartments, c => c.vented && !c.doorsShut);
        var hot = leaking ? System.Array.FindAll(compartments, c => c.rad) : new Compartment[0];
        double pf = 1 - System.Math.Pow(1 - fireSpreadPerSecond, dt), pv = 1 - System.Math.Pow(1 - vacuumSpreadPerSecond, dt),
               pr = 1 - System.Math.Pow(1 - radSpreadPerSecond, dt);
        foreach (var c in burning)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.fire && !m.vented && Roll(pf)) { m.fire = true; m.fireT = 0; } }
        foreach (var c in airless)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.vented && Roll(pv)) Breach(m, breachCrewLoss); }
        foreach (var c in hot)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.rad && Roll(pr)) { m.rad = true; m.fadeT = 0; } }
        foreach (var c in compartments)
        {
            if (c.fire && c.vented) c.fire = false;
            if (c.fire)
                for (c.fireT += dt; c.fireT >= fireDamageSeconds; c.fireT -= fireDamageSeconds) c.integ = Mathf.Max(0, c.integ - 1);
            if (!c.rad) continue;
            if (c.crew > 0)
                for (c.radT += dt; c.radT >= radCrewLossSeconds && c.crew > 0; c.radT -= radCrewLossSeconds) c.crew--;
            if (leaking) c.fadeT = 0;
            else if ((c.fadeT += dt) >= radFadeSeconds) { c.rad = false; c.radT = 0; c.fadeT = 0; }
        }
        if (rct != null && rct.integ <= 0) destroyed = true;
        SyncCond();
    }

    /// <summary>A contact's ship takes damage by our numbers: one Tuning window for every ship (the enemy is our
    /// ship with other stats; these are not among them).</summary>
    public void CopyDamageTuning(HandoffBoardState from)
    {
        slugArmourDamage = from.slugArmourDamage; slugCompartmentDamage = from.slugCompartmentDamage;
        heArmourDamage = from.heArmourDamage; empSeconds = from.empSeconds;
        fireChanceHE = from.fireChanceHE; fireChancePEN = from.fireChancePEN; breachChanceSlug = from.breachChanceSlug;
        radChanceReactor = from.radChanceReactor; fireDamageSeconds = from.fireDamageSeconds;
        fireSpreadPerSecond = from.fireSpreadPerSecond; vacuumSpreadPerSecond = from.vacuumSpreadPerSecond;
        radSpreadPerSecond = from.radSpreadPerSecond; breachCrewLoss = from.breachCrewLoss;
        radCrewLossSeconds = from.radCrewLossSeconds; reactorContainedAt = from.reactorContainedAt; radFadeSeconds = from.radFadeSeconds;
    }

    /// <summary>A contact's ship destroyed without a hit landing (its reactor burned out) becomes a wreck too.</summary>
    public void NoteWrecks()
    {
        if (ShipOf == null) return;
        foreach (var t in tracks)
        {
            if (t.own || t.wreck) continue;
            var ship = ShipOf(t.key);
            if (ship != null && ship.destroyed) Kill(t);
        }
    }

    [Tooltip("Armour an HE warhead takes off the side it hits (owner: it opens a side fast).")]
    [Tune("Damage")] public float heArmourDamage = 100f;
    [Tooltip("An EMP knocks a ship's power out for this many mission seconds.")]
    [Tune("Damage")] public float empSeconds = 60f;
    /// <summary>Power is out until this mission time (EMP).</summary>
    [System.NonSerialized] public float empUntil;

    /// <summary>A missile's warhead strikes this ship (Step 53, owner 2026-10-23). HE hits the armour hard,
    /// then what is behind it; PENETRATOR goes straight to a compartment behind the side; EMP does no
    /// hull damage but knocks the power out.</summary>
    public string TakeWarhead(string side, string warheadName)
    {
        if (destroyed) return "";
        if (warheadName == "EMP")
        {
            foreach (var b in buses) b.fill = 0;
            empUntil = missionNow + empSeconds;
            return " · EMP · POWER DOWN";
        }
        int i = System.Array.IndexOf(Sides, side);
        if (i < 0) return "";
        var armour = SideArmour;
        if (warheadName == "HE" && armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - heArmourDamage);
            return " · HE" + (armour[i] <= 0f ? " OPEN" : " ARMOUR " + JsRound(armour[i] / sideArmourMax[i] * 100f) + "%");
        }
        string w = warheadName == "PENETRATOR" ? "PEN" : warheadName;
        return " · " + w + Behind(i, slugCompartmentDamage, w);
    }

    public float HitRadiusKm(string klass)
    {
        foreach (var h in hitRadii) if (h.klass == klass) return h.km;
        return 150f;
    }

    /// <summary>Hull-fixed arcs (spec 8.7.a): the solution's bearing from our bow must lie in the mount's arc.</summary>
    public bool InArc
    {
        get
        {
            var m = MountOf(mount);
            if (m == null || !Sol.set) return false;
            return AngDiff((Sol.brg % 360 + 360) % 360, m.arc) <= m.halfArc;
        }
    }

    /// <summary>Fly every round dt mission seconds. Its whole path this tick is tested, so a fast slug
    /// cannot skip past a ship: the first ship it passes within that ship's hit radius is hit. Step 63: the
    /// test follows the ship's own motion through the tick too (at 5x speeds and x10 a ship moves further
    /// in a tick than its hit radius).</summary>
    public void TickRounds(double dt)
    {
        var list = Rounds;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var r = list[i];
            if (r.missile) Steer(r, dt);
            double step = System.Math.Min(r.speed * dt, r.maxKm - r.travelled);
            double flight = r.speed > 0 ? step / r.speed : 0; // seconds of this tick the round is flying
            Track struck = null;
            double first = double.MaxValue;
            foreach (var t in tracks)
            {
                if (t.own || !t.hasWorld || t.wreck) continue;
                // the ship relative to the round, and how that changes: its velocity less the round's
                double c = t.course * System.Math.PI / 180;
                double wx = t.speed * System.Math.Sin(c) - r.dx * r.speed, wy = t.speed * System.Math.Cos(c) - r.dy * r.speed;
                double px = t.worldX - r.x, py = t.worldY - r.y, ww = wx * wx + wy * wy;
                double s = ww > 0 ? System.Math.Max(0, System.Math.Min(flight, -(px * wx + py * wy) / ww)) : 0; // closest moment
                double cx = px + wx * s, cy = py + wy * s, rad = HitRadiusKm(t.klass);
                if (cx * cx + cy * cy <= rad * rad && s < first) { struck = t; first = s; }
            }
            if (struck != null)
            {
                // Step 49: the struck ship takes the hit on its own model (its side, then what is behind it)
                string side = SideStruck(struck, r);
                var ship = ShipOf != null ? ShipOf(struck.key) : null;
                string damage = ship == null ? "" : r.missile ? ship.TakeWarhead(side, warheads[r.warhead]) : ship.TakeSlug(side);
                if (ship != null && ship.destroyed) Kill(struck);
                else { shotReport = "HIT · " + NameOf(struck) + (Scanned(struck) ? " · " + side + damage : ""); lastShotHit = true; shotResults++; } // Step 61: detail once scanned
                list.RemoveAt(i);
                continue;
            }
            r.x += r.dx * step; r.y += r.dy * step; r.travelled += step;
            if (r.travelled >= r.maxKm - 1e-6) { shotReport = "MISS"; lastShotHit = false; shotResults++; list.RemoveAt(i); }
        }
    }

    // A missile turns toward its target, no faster than missileTurnDegPerS. With no target left it flies on.
    void Steer(Round r, double dt)
    {
        var t = TrackOf(r.targetKey);
        if (t == null || !t.hasWorld || t.wreck) return;
        SteerAt(r, t.worldX, t.worldY, dt);
    }

    void SteerAt(Round r, double x, double y, double dt)
    {
        double want = System.Math.Atan2(x - r.x, y - r.y) * 180 / System.Math.PI;
        double now = System.Math.Atan2(r.dx, r.dy) * 180 / System.Math.PI;
        double turn = ((want - now) % 360 + 540) % 360 - 180, most = missileTurnDegPerS * dt;
        double head = (now + System.Math.Max(-most, System.Math.Min(most, turn))) * System.Math.PI / 180;
        r.dx = System.Math.Sin(head); r.dy = System.Math.Cos(head);
    }

    // The side facing the shot (reference sim sideFacing): where it came from, against the ship's bow.
    static string SideStruck(Track t, Round r) => SideAt(System.Math.Atan2(-r.dx, -r.dy) * 180 / System.Math.PI, t.worldHdg);

    // The side of a ship whose bow points bowDeg that faces a bearing fromDeg (both clockwise from north).
    static string SideAt(double fromDeg, double bowDeg)
    {
        double rel = ((fromDeg - bowDeg) % 360 + 360) % 360;
        return rel < 45 || rel >= 315 ? "PROW" : rel < 135 ? "STBD" : rel < 225 ? "AFT" : "PORT";
    }

    // ---- enemy missiles and our point defence (Step 55, owner 2026-10-24) -----------------------

    [System.NonSerialized] private System.Collections.Generic.List<Round> incoming;
    /// <summary>Enemy missiles flying at our ship.</summary>
    public System.Collections.Generic.List<Round> Incoming => incoming ?? (incoming = new System.Collections.Generic.List<Round>());
    [Tooltip("The chance a PDC group on AUTO shoots down an enemy missile that enters its arc (owner: a chance per missile).")]
    [Tune("Periscope & fire control")] [Range(0f, 1f)] public float pdcKillChance = 0.7f;
    [Tooltip("An enemy missile hits our ship if it passes within this distance, km (cruiser-sized, like TRACK 07).")]
    [Tune("Damage")] public float ownHitRadiusKm = 210f;
    /// <summary>The PDCs' last result ("PDC-A · MISSILE DOWN"), and the last enemy missile to hit us.</summary>
    [System.NonSerialized] public string pdcReport, incomingReport;
    [System.NonSerialized] public int pdcKills, hitsTaken;

    /// <summary>An enemy at (x, y) launches n missiles at us, fanned, each homing on our ship.</summary>
    public void LaunchAt(double x, double y, int n, int warheadIndex, double maxKm)
    {
        double brg = System.Math.Atan2(ownX - x, ownY - y) * 180 / System.Math.PI;
        for (int i = 0; i < n; i++)
        {
            double b = (brg + (i - (n - 1) / 2.0) * missileSpreadDeg) * System.Math.PI / 180;
            Incoming.Add(new Round { x = x, y = y, dx = System.Math.Sin(b), dy = System.Math.Cos(b), maxKm = maxKm,
                                     speed = missileSpeedKms, missile = true, warhead = warheadIndex });
        }
    }

    /// <summary>Each tick: our PDCs take their chances, then every enemy missile homes on us; one that
    /// passes within ownHitRadiusKm strikes the side facing it.</summary>
    public void TickIncoming(double dt)
    {
        var list = Incoming;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var r = list[i];
            if (PdcStops(r)) { list.RemoveAt(i); continue; }
            SteerAt(r, ownX, ownY, dt);
            double step = System.Math.Min(r.speed * dt, r.maxKm - r.travelled);
            double px = ownX - r.x, py = ownY - r.y;
            double u = System.Math.Max(0, System.Math.Min(step, px * r.dx + py * r.dy)); // closest point along the path
            double cx = px - r.dx * u, cy = py - r.dy * u;
            if (cx * cx + cy * cy <= ownHitRadiusKm * ownHitRadiusKm)
            {
                string side = SideAt(System.Math.Atan2(-r.dx, -r.dy) * 180 / System.Math.PI, hdg);
                incomingReport = "MISSILE HIT · " + side + TakeWarhead(side, warheads[r.warhead]);
                hitsTaken++;
                list.RemoveAt(i);
                continue;
            }
            r.x += r.dx * step; r.y += r.dy * step; r.travelled += step;
            if (r.travelled >= r.maxKm - 1e-6) list.RemoveAt(i);
        }
    }

    // A PDC group on AUTO, loaded and powered, gets one chance at each enemy missile inside its arc and range.
    bool PdcStops(Round r)
    {
        if (!loaders || !WepReady) return false;
        double dx = r.x - ownX, dy = r.y - ownY;
        int rel = JsRound(((System.Math.Atan2(dx, dy) * 180 / System.Math.PI - hdg) % 360 + 360) % 360) % 360;
        var groups = System.Array.Find(systems, y => y.id == "PD").mounts;
        for (int g = 0; g < groups.Length && g < pdcAuto.Length; g++)
        {
            var m = MountOf(groups[g]);
            if (!pdcAuto[g] || m == null || (r.pdcTried & (1 << g)) != 0) continue;
            if (dx * dx + dy * dy > (double)m.rangeKm * m.rangeKm || AngDiff(rel, m.arc) > m.halfArc) continue;
            r.pdcTried |= 1 << g;
            if (dice == null) dice = new System.Random(49);
            bool down = dice.NextDouble() < pdcKillChance;
            pdcReport = m.shortName + (down ? " · MISSILE DOWN" : " · MISSED");
            if (down) { pdcKills++; return true; }
        }
        return false;
    }

    public void ToggleDoors() { doorsOpen = !doorsOpen; }

    /// <summary>setLbrg: non-digits stripped, wrapped to 0–359.</summary>
    public void SetLbrg(string text)
    {
        var digits = System.Text.RegularExpressions.Regex.Replace(text ?? "", "[^0-9]", "");
        int n;
        lbrg = int.TryParse(digits, out n) ? ((n % 360) + 360) % 360 : 0;
    }

    void ClearStages() { foreach (var g in stages) { g.on = false; g.progress = 0f; } }

    /// <summary>cycleSquad: next squadron, its first order, and every launch stage cleared.</summary>
    public void CycleSquad()
    {
        squad = (squad + 1) % squads.Length;
        order = CurrentSquad.orders[0];
        ClearStages();
    }

    public void CycleOrder()
    {
        var list = CurrentSquad.orders;
        int i = System.Array.IndexOf(list, order);
        order = list[(i + 1) % list.Length];
    }

    /// <summary>pressStage: releasing a stage releases it and every stage after; throwing needs the
    /// one before at 100% and no stage block; refused once the squadron is out.</summary>
    public void PressStage(int i)
    {
        if (Launched(CurrentSquad)) return;
        if (stages[i].on)
        {
            for (int k = i; k < stages.Length; k++) { stages[k].on = false; stages[k].progress = 0f; }
            return;
        }
        if (i > 0 && stages[i - 1].progress < 100f) return;
        if (StageBlock(stages[i].id) != null) return;
        stages[i].on = true;
    }

    public void LaunchSquad()
    {
        var q = CurrentSquad;
        if (Launched(q) || LaunchBlock() != null || !AllStagesDone) return;
        launchedSquads.Add(q.key);
        launchedOrders.Add(order);
        // Step 54: the squadron's ready craft take off from our ship, for fire control's target
        var f = new Flight { squad = q.key, order = order, targetKey = target, craft = SquadReady(q), x = ownX, y = ownY, hdg = lbrg };
        if (q.key == "ih") { f.marines = Mathf.Min(marines, f.craft * marinesPerBoat); marines -= f.marines; } // Step 60: the boats take marines
        Flights.Add(f);
    }

    public void RecallSquad()
    {
        // Step 54 (owner 2026-10-23): RECALL turns the squadron home; it is back once it lands (TickFlights)
        var f = Flights.Find(k => k.squad == CurrentSquad.key);
        if (f != null) { f.returning = true; f.aboard = false; f.holding = null; return; } // marines aboard withdraw (Step 60)
        int i = launchedSquads.IndexOf(CurrentSquad.key);
        if (i >= 0) { launchedSquads.RemoveAt(i); launchedOrders.RemoveAt(i); }
        ClearStages();
    }

    // ---- fighters in flight (Step 54, owner 2026-10-23) ----------------------------------------

    [System.Serializable]
    public class Flight
    {
        public string squad, order, targetKey;
        public int craft;
        public double x, y, hdg;
        public bool returning, attacking;
        public float nextRun;
        [Tooltip("Step 60: assault boats — marines carried, whether they are aboard the target and for how long, why they are "
               + "holding off (null while not), and how it ended.")]
        public int marines;
        public bool aboard;
        public float boardT;
        public string holding, outcome;
    }
    [System.NonSerialized] private System.Collections.Generic.List<Flight> flights;
    public System.Collections.Generic.List<Flight> Flights => flights ?? (flights = new System.Collections.Generic.List<Flight>());
    [Tooltip("Fighter speed, km/s (owner: three times our top speed; Step 63: x5).")]
    [Tune("Hangar & fighters")] public double fighterSpeedKms = 40;
    [Tooltip("Inside this distance of the target the squadron makes its attack runs, km.")]
    [Tune("Hangar & fighters")] public double fighterAttackKm = 200;
    [Tooltip("One attack run by every fighter this often, mission seconds.")]
    [Tune("Hangar & fighters")] public float fighterRunSeconds = 30f;
    [Tooltip("Armour each fighter takes off the side it attacks, per run.")]
    [Tune("Hangar & fighters")] public float fighterArmourDamage = 10f;
    [Tooltip("Integrity each fighter takes off a compartment through an open side, per run.")]
    [Tune("Hangar & fighters")] public int fighterCompartmentDamage = 1;
    [Header("Assault boats (Step 60, BOARDING_README.md)")]
    [Tooltip("Assault boat speed, km/s (Step 63: x5).")]
    [Tune("Hangar & fighters")] public double boatSpeedKms = 30;
    [Tooltip("Marines aboard our ship, carried by the assault boats; those lost aboard an enemy are gone.")]
    [Tune("Hangar & fighters")] public int marines = 18;
    [Tooltip("Marines each assault boat carries.")]
    [Tune("Hangar & fighters")] public int marinesPerBoat = 6;
    [Tooltip("Boats dock inside this distance of the target, km.")]
    [Tune("Hangar & fighters")] public double dockKm = 50;
    [Tooltip("A ship can be boarded only with a face open and slower than this, km/s (owner: fight it down first).")]
    [Tune("Hangar & fighters")] public float boardMaxSpeedKms = 0.5f;
    [Tooltip("How long the marines fight aboard, game seconds.")]
    [Tune("Hangar & fighters")] public float boardSeconds = 60f;
    [Tooltip("Share of the enemy's crew that fights the boarders. The odds are marines ÷ (marines + defenders).")]
    [Tune("Hangar & fighters")] [Range(0f, 1f)] public float defenderShare = 0.25f;
    [Tooltip("Marines lost per defender, when they win.")]
    [Tune("Hangar & fighters")] public float marineLossPerDefender = 0.5f;
    /// <summary>Taken by our marines (SECURE): it stops fighting and is a prize.</summary>
    [System.NonSerialized] public bool captured;
    [System.NonSerialized] public int boardings;

    /// <summary>What the squadron is doing, for the hangar's status line.</summary>
    public string FlightStatus(string squadKey)
    {
        var f = Flights.Find(k => k.squad == squadKey);
        if (f == null) return null;
        if (f.returning) return (f.outcome != null ? f.outcome + " · " : "") + "RETURNING · " + JsRound(System.Math.Sqrt((f.x - ownX) * (f.x - ownX) + (f.y - ownY) * (f.y - ownY))).ToString("N0") + " KM";
        if (f.order == "escort") return "ESCORTING";
        var t = TrackOf(f.targetKey);
        string name = t != null ? NameOf(t) : "TARGET";
        if (f.aboard) return "BOARDING " + name + " · " + Mathf.CeilToInt(boardSeconds - f.boardT) + " S";
        if (f.holding != null) return "HOLDING OFF " + name + " · " + f.holding;
        if (f.attacking) return (f.order == "disable" ? "DISABLING " : "ATTACKING ") + name + (Scanned(t) && flightReport != null ? " ·" + flightReport : ""); // Step 61: detail once scanned
        double d = t != null && t.hasWorld ? System.Math.Sqrt((t.worldX - f.x) * (t.worldX - f.x) + (t.worldY - f.y) * (t.worldY - f.y)) : 0;
        return "OUTBOUND TO " + name + " · " + JsRound(d).ToString("N0") + " KM";
    }
    [System.NonSerialized] public string flightReport;
    [System.NonSerialized] public int flightRuns;

    /// <summary>Each tick: fly to the target (or home, or with us on ESCORT); inside attack range, every fighter
    /// makes a run every fighterRunSeconds; home and close, they land and the squadron is ready again.</summary>
    public void TickFlights(double dt)
    {
        var list = Flights;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var f = list[i];
            bool boats = f.squad == "ih"; // the assault boats carry marines (Step 60)
            var t = f.returning || f.order == "escort" ? null : TrackOf(f.targetKey);
            if (!f.returning && f.order != "escort" && (t == null || !t.hasWorld || t.wreck || t.captured)) { f.returning = true; f.attacking = false; f.aboard = false; t = null; }
            double gx = t != null ? t.worldX : ownX, gy = t != null ? t.worldY : ownY;
            double reach = f.returning ? 50 : f.order == "escort" ? 300 : boats ? dockKm : fighterAttackKm;
            double dx = gx - f.x, dy = gy - f.y, d = System.Math.Sqrt(dx * dx + dy * dy);
            if (d > reach)
            {
                double step = System.Math.Min((boats ? boatSpeedKms : fighterSpeedKms) * dt, d - reach * 0.9);
                f.x += dx / d * step; f.y += dy / d * step;
                f.hdg = (System.Math.Atan2(dx, dy) * 180 / System.Math.PI + 360) % 360;
                f.holding = null;
                continue;
            }
            if (f.returning)
            {
                list.RemoveAt(i);
                int k = launchedSquads.IndexOf(f.squad);
                if (k >= 0) { launchedSquads.RemoveAt(k); launchedOrders.RemoveAt(k); }
                marines += f.marines; // the survivors are home
                ClearStages();
                continue;
            }
            if (t == null) continue; // escorting: stay with us
            if (boats)
            {
                var target = ShipOf != null ? ShipOf(t.key) : null;
                if (target != null) TickBoarding(f, t, target, dt);
                continue;
            }
            if (!f.attacking) { f.attacking = true; f.nextRun = missionNow; }
            if (missionNow < f.nextRun) continue;
            f.nextRun = missionNow + fighterRunSeconds;
            var ship = ShipOf != null ? ShipOf(t.key) : null;
            if (ship == null) continue;
            string side = SideFrom(t, f.x, f.y), result = "";
            for (int c = 0; c < f.craft && !ship.destroyed; c++)
                result = ship.TakeFighterRun(side, f.order == "disable", fighterArmourDamage, fighterCompartmentDamage);
            flightRuns++;
            flightReport = result;
            if (ship.destroyed && !t.wreck) Kill(t);
        }
    }

    // Step 60 (owner 2026-10-24, BOARDING_README.md): the boats hold off until a face is open and the ship is nearly
    // stopped, then dock; the marines fight for boardSeconds; then they carry out the order, or are repelled.
    void TickBoarding(Flight f, Track t, HandoffBoardState ship, double dt)
    {
        if (!f.aboard)
        {
            bool open = false;
            foreach (var a in ship.SideArmour) if (a <= 0f) open = true;
            f.holding = !open ? "NO OPEN SIDE" : t.speed > boardMaxSpeedKms ? "TARGET UNDER WAY" : null;
            if (f.holding != null) return;
            f.aboard = true; f.boardT = 0;
        }
        f.boardT += (float)dt;
        if (f.boardT < boardSeconds) return;
        int defenders = Mathf.CeilToInt(ship.TotalCrew * defenderShare);
        bool won = f.marines > 0 && (defenders == 0 || Roll(f.marines / (double)(f.marines + defenders)));
        if (won)
        {
            ship.KillCrew(defenders);
            f.marines = Mathf.Max(0, f.marines - Mathf.RoundToInt(defenders * marineLossPerDefender));
            if (f.order == "destroy") { ship.Comp("rct").integ = 0; ship.destroyed = true; Kill(t); f.outcome = NameOf(t) + " SCUTTLED"; }
            else if (f.order == "disable") { ship.Comp("eng").integ = 0; ship.Comp("mag").integ = 0; f.outcome = NameOf(t) + " DISABLED"; }
            else { ship.captured = true; t.captured = true; t.hostile = false; f.outcome = NameOf(t) + " SECURED"; }
        }
        else { ship.KillCrew(f.marines / 2); f.marines = 0; f.outcome = "BOARDING REPELLED"; }
        flightReport = f.outcome;
        boardings++;
        f.aboard = false;
        f.returning = true;
    }

    public int TotalCrew { get { int n = 0; foreach (var c in compartments) n += c.crew; return n; } }

    /// <summary>Crew lost aboard (boarding): taken from the most crowded rooms first.</summary>
    public void KillCrew(int n)
    {
        for (; n > 0; n--)
        {
            Compartment most = null;
            foreach (var c in compartments) if (c.crew > 0 && (most == null || c.crew > most.crew)) most = c;
            if (most == null) return;
            most.crew--;
        }
    }

    // The side of a ship facing a point (where the attack comes from), against its bow.
    static string SideFrom(Track t, double x, double y) => SideAt(System.Math.Atan2(x - t.worldX, y - t.worldY) * 180 / System.Math.PI, t.worldHdg);

    /// <summary>Step 61 (owner 2026-10-24): a contact is named as the sensors board names it — "BOGEY T07" until an
    /// active scan identifies it, then "TRACK 07" — and what our hits do to it is reported only once it is scanned.</summary>
    public string NameOf(Track t) => Scanned(t) ? t.name : "BOGEY " + t.key.ToUpperInvariant();
    public bool Scanned(Track t) => t != null && identified.Contains(t.key);

    void Kill(Track t)
    {
        t.wreck = true; t.hostile = false;
        shotReport = NameOf(t) + " DESTROYED";
        kills++; shotResults++; lastShotHit = true;
    }

    public void ToggleDradis() { dradis = !dradis; }
    public void ToggleActive() { active = !active; }

    /// <summary>startGain: 1..5, capped by the SEN bus.</summary>
    public void SetGain(int v) { senBoost = Mathf.Max(1, Mathf.Min(5, Mathf.Min(GainCap, v))); }

    public void SetSpectrum(string band) { spectrum = band; }
    public void SetCone(int brg) { coneBrg = Mathf.Clamp(brg, 0, 360); }
    public void ConeWiden() { coneArc = Mathf.Min(coneArcMax, coneArc + coneArcStep); }
    public void ConeNarrow() { coneArc = Mathf.Max(coneArcMin, coneArc - coneArcStep); }

    /// <summary>cycleChan: band 1 → 2 → 3 → 1; drops any lock.</summary>
    public void CycleChan() { chan = chan >= 3 ? 1 : chan + 1; locked = false; }

    /// <summary>startTune: frequency 1–100 in 0.1 steps; any movement drops the lock.</summary>
    public void SetFreq(double f)
    {
        freq = System.Math.Floor(System.Math.Max(1.0, System.Math.Min(100.0, f)) * 10.0 + 0.5) / 10.0;
        locked = false;
    }

    /// <summary>lockSignal: locks only within the window of the carrier.</summary>
    public void LockSignal() { if (TuneOff <= lockWindow) locked = true; }

    /// <summary>stepDial: only while locked; a step forward, or back with shift, through A–Z.</summary>
    public bool StepDial(int i, bool back)
    {
        if (!locked || i < 0 || i >= dials.Length) return false;
        dials[i] = ((dials[i] + (back ? -1 : 1)) % 26 + 26) % 26;
        activeDial = i;
        return true;
    }

    /// <summary>resolveLit: on releasing the cone dial, every contact inside the cone is identified.</summary>
    public void ResolveLit()
    {
        foreach (var t in tracks) if (ConeCovers(t) && !identified.Contains(t.key)) identified.Add(t.key);
    }

    /// <summary>identify: a contact-board row identifies its contact if the cone covers it and it is heard.</summary>
    public void Identify(string key)
    {
        foreach (var t in tracks)
            if (t.key == key && ConeCovers(t) && Heard(t) && !identified.Contains(key)) identified.Add(key);
    }

    /// <summary>cycleBus: VENT ↔ STANDBY ↔ DRAW, reversing at each end; refused while shed.</summary>
    public bool CycleBus(int i)
    {
        if (shed || i < 0 || i >= buses.Length) return false;
        var b = buses[i];
        int m = (int)b.mode, d = b.dir == -1 ? -1 : 1;
        if (m + d > 2 || m + d < 0) d = -d;
        b.mode = (BusMode)(m + d);
        b.dir = d;
        return true;
    }

    /// <summary>toggleShed: every bus to VENT when thrown, back to STANDBY when released.</summary>
    public void ToggleShed()
    {
        foreach (var b in buses) b.mode = shed ? BusMode.Standby : BusMode.Vent;
        shed = !shed;
    }

    // ---- the tick (tickPower, every 420 ms) — power part ---------------------------------

    // Owner 2026-10-24: every rate runs in game time. The design's per-tick amounts were per game second
    // at x1 (a tick is `rate` game seconds), so they are now multiplied by the tick's game seconds; x1 is
    // unchanged. Whole-unit steps (a bus filling, the bow swinging) bank fractions so x0.25 is exact too.
    [System.NonSerialized] private double powerBank;

    public void TickPower(double dt)
    {
        // EMP: power stays down (Step 53). Destroyed (Step 58): the ship goes dark for good.
        if (missionNow < empUntil || destroyed) { foreach (var b in buses) b.fill = 0; return; }
        powerBank += dt;
        for (; powerBank >= 1 - 1e-9; powerBank -= 1) StepPower(); // a bus fills or drains one unit per game second
        // the reactor cap holds at once, whatever the rate
        int total = TotalFill, cap = ReactorCap;
        for (int i = buses.Length - 1; i >= 0 && total > cap; i--)
            while (buses[i].fill > 0 && total > cap) { buses[i].fill--; total--; }
    }

    void StepPower()
    {
        int total = TotalFill, cap = ReactorCap;
        // drain first, so a bus dropping out of DRAW frees its units the same tick
        foreach (var b in buses) if (b.mode == BusMode.Vent && b.fill > 0) { b.fill--; total--; }
        foreach (var b in buses) if (b.mode == BusMode.Draw && b.fill < busMax && total < cap) { b.fill++; total++; }
        for (int i = buses.Length - 1; i >= 0 && total > cap; i--)
            while (buses[i].fill > 0 && total > cap) { buses[i].fill--; total--; }
    }

    // ---- the tick — weapons capacitor: +5 per WEP unit per game second up to 100, bleeds 3 with no WEP --

    public void TickWeapons(double dt)
    {
        float feed = BusUnits("wep") * wepFeedPerUnit * (float)dt;
        wepCharge = feed > 0f ? Mathf.Min(100f, wepCharge + feed) : Mathf.Max(0f, wepCharge - wepBleed * (float)dt);
    }

    // ---- the tick — launch telemetry: a thrown stage runs out over several game seconds --------

    public void TickLaunch(double dt)
    {
        foreach (var g in stages) g.progress = g.on ? Mathf.Min(100f, g.progress + g.rate * (float)dt) : 0f;
    }

    // ---- the tick — damage-control party ---------------------------------------------------

    public void TickParty(double dt)
    {
        if (!PartyOut) return;
        partyT += (float)dt;
        if (partyT < partySeconds - 1e-4f) return;
        var c = Comp(partyComp);
        // Step 59: the team fights a fire first; in a sealed airless room it restores the air; otherwise it repairs
        if (c != null)
        {
            if (c.fire) { c.fire = false; c.fireT = 0; }
            else if (c.vented) { c.vented = false; c.doorsShut = false; }
            else c.integ = Mathf.Min(5, c.integ + RepairRate);
        }
        partyT = -1;
        partyComp = null;
        SyncCond();
    }

    // ---- number helpers (JS rounding, as the browser board does) ----------------------------

    /// <summary>JS Math.round on a double: half rounds up.</summary>
    public static int JsRound(double x) => (int)System.Math.Floor(x + 0.5);

    /// <summary>JS toFixed for a positive number: rounds on the double's exact binary value
    /// (1.45 is 1.4499…, so "1.4"); a true half rounds up, as toFixed does.</summary>
    public static string JsToFixed(double x, int digits)
    {
        long bits = System.BitConverter.DoubleToInt64Bits(x);
        int e = (int)((bits >> 52) & 0x7FF);
        long m = bits & ((1L << 52) - 1);
        if (e == 0) e = 1; else m |= 1L << 52;
        int exp = e - 1075; // x = m * 2^exp
        var num = new System.Numerics.BigInteger(m) * System.Numerics.BigInteger.Pow(10, digits);
        System.Numerics.BigInteger q;
        if (exp >= 0) q = num << exp;
        else
        {
            var den = System.Numerics.BigInteger.One << -exp;
            var r = System.Numerics.BigInteger.Remainder(num, den);
            q = System.Numerics.BigInteger.Divide(num, den);
            if (r * 2 >= den) q += 1;
        }
        string t = q.ToString().PadLeft(digits + 1, '0');
        return digits == 0 ? t : t.Substring(0, t.Length - digits) + "." + t.Substring(t.Length - digits);
    }

    /// <summary>offBearingLabel: "+1.5°" / "-14.0°" / "—" (toFixed(1), sign kept on a negative).</summary>
    public static string OffLabel(double? o) => o == null ? "\u2014" : (o.Value > 0 ? "+" : o.Value < 0 ? "-" : "") + JsToFixed(System.Math.Abs(o.Value), 1) + "\u00B0";

    // ---- the tick — sensors part (reads the fill this tick committed) --------------------

    /// <summary>Contacts first heard or identified since the alarm strip was last acknowledged
    /// (Step 44; owner 2026-10-18: an alert shows only for a real alert). The design raised the strip
    /// once at load and never again.</summary>
    [System.NonSerialized] public int newContacts;
    [System.NonSerialized] private System.Collections.Generic.HashSet<string> seenContacts;

    /// <summary>Count each contact once: the first time DRADIS hears it or a scan identifies it.
    /// Returns true when there is a new one.</summary>
    public bool NoteNewContacts()
    {
        if (seenContacts == null) seenContacts = new System.Collections.Generic.HashSet<string>();
        int before = newContacts;
        foreach (var t in tracks)
            if (!t.own && (Heard(t) || identified.Contains(t.key)) && seenContacts.Add(t.key)) newContacts++;
        return newContacts > before;
    }

    public void AckContacts() { newContacts = 0; }

    /// <summary>Step 45 (owner 2026-10-23): each radar sweep that passes a heard contact leaves its
    /// bearing line where our ship was. As we move, the lines cross at the target: that is how a
    /// passive contact's range is found. Exact bearings; the lines fade out.</summary>
    [System.Serializable]
    public class BearingFix { public string key; public double x, y, brg; public float t; }
    [System.NonSerialized] private System.Collections.Generic.List<BearingFix> bearingFixes;
    public System.Collections.Generic.List<BearingFix> BearingFixes => bearingFixes ?? (bearingFixes = new System.Collections.Generic.List<BearingFix>());
    [Tooltip("A bearing line fades out over this much mission time (owner 2026-10-23: 2 minutes).")]
    [Tune("Sensors")] public float bearingTrailSeconds = 120f;

    /// <summary>True bearing from our ship to a contact, degrees, unrounded.</summary>
    public double ExactBearing(Track t)
    {
        if (!t.hasWorld) return t.bearing;
        return (System.Math.Atan2(t.worldX - ownX, t.worldY - ownY) * 180 / System.Math.PI + 360) % 360;
    }

    public void LayBearing(Track t, float now) { BearingFixes.Add(new BearingFix { key = t.key, x = ownX, y = ownY, brg = ExactBearing(t), t = now }); }

    /// <summary>Step 75 (owner 2026-10-26): the sweep touches a contact. Out of sight, the plot shows it here until the next pass.</summary>
    public void See(Track t) { t.seen = true; t.seenX = t.worldX; t.seenY = t.worldY; t.seenBrg = ExactBearing(t); t.seenRangeKm = t.rangeKm; }
    /// <summary>Step 75: close enough to see with the eye.</summary>
    public bool InSight(Track t) => t.hasWorld && t.rangeKm <= visualRangeKm;

    public void PruneBearings(float now) { BearingFixes.RemoveAll(f => now - f.t >= bearingTrailSeconds); }

    public void TickSensors(double dt)
    {
        int senCap = Mathf.Max(1, BusUnits("sen"));
        if (senBoost > senCap) senBoost = 1; // owner 2026-10-25: power lost, the setting goes back to the bottom; you set it again
        // every unit drawn makes us louder, and so does ACTIVE SCAN (GDD p18, spec 3.1.b, prototype getSig)
        float want = SignalTarget;
        if (signal < want) signal = Mathf.Min(want, signal + signalRise * (float)dt);
        else if (signal > want) signal = Mathf.Max(want, signal - signalFall * (float)dt);
    }

    /// <summary>Where our signal is heading: each bus's units × its signal per unit, plus ACTIVE SCAN, 0–100.</summary>
    public float SignalTarget
    {
        get
        {
            float t = active ? activeScanSignal : 0f;
            foreach (var b in buses) t += b.fill * b.signalPerUnit;
            return Mathf.Clamp(t, 0f, 100f);
        }
    }
}
