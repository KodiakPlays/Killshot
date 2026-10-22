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
        [Tooltip("FIRE, RAD or empty.")]
        public string cond;
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
        [System.NonSerialized] public double worldX, worldY;
        [Tooltip("Step 48: which way the contact's bow points, degrees (from its model), for the side a slug strikes.")]
        [System.NonSerialized] public double worldHdg;
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
    [Range(1, 5)] public int senBoost = 2;
    [Tooltip("Sensor range = base + per step x gain (senRangeKm).")]
    [Tune("Sensors")]
    public float senRangeBaseKm = 12000f, senRangePerStepKm = 7000f;
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
    [Tooltip("Signal climbs this much per tick toward its target.")]
    [UnityEngine.Serialization.FormerlySerializedAs("signalRise")]
    [Tune("Power & signal")]
    public float signalRise = 9f;
    [Tooltip("Signal falls this much per tick toward its target.")]
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
    [Tooltip("PARTY_TICKS: a damage-control party works this many ticks, then the repair lands.")]
    [Tune("Engineering & orders")]
    public int partyTicks = 6;
    [Tooltip("The party's compartment and its tick count; tick -1 when no party is out.")]
    public string partyComp;
    public int partyT = -1;
    [Tooltip("A thrown repair or order rocker flicks back after this long (flickBack).")]
    [Tune("Engineering & orders")]
    public float flickBackSeconds = 0.4f;
    public System.Collections.Generic.List<string> dcThrown = new System.Collections.Generic.List<string>();
    [Header("Orders station — AUX order pool (ENG_ORDERS)")]
    [Tooltip("AUX order pool, 0..100 (auxCharge).")]
    public double auxCharge;
    [Tooltip("Pool gained per AUX unit per tick.")]
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
    }
    [Tooltip("A knob turned with no solution starts one here, on the periscope's bearing at speed 0 (owner 2026-10-23).")]
    [Tune("Periscope & fire control")] public float manualStartRangeKm = 10000f;

    [Header("Fire control — MOUNTS, SYSTEMS, WARHEADS, SWITCHES and the board's initial state")]
    public Mount[] mounts =
    {
        new Mount { id = "RGA", arc = 42, halfArc = 80, rangeKm = 30000, label = "RAILGUN TURRET A", shortName = "RG-A", loaded = "2 / 2", ammo = "22" },
        new Mount { id = "RGB", arc = 250, halfArc = 80, rangeKm = 30000, label = "RAILGUN TURRET B", shortName = "RG-B", loaded = "1 / 2", ammo = "19" },
        new Mount { id = "PDA", arc = 42, halfArc = 35, rangeKm = 4000, label = "PDC GROUP A", shortName = "PDC-A", loaded = "BELT", ammo = "2400" },
        new Mount { id = "PDB", arc = 200, halfArc = 35, rangeKm = 4000, label = "PDC GROUP B", shortName = "PDC-B", loaded = "BELT", ammo = "2180" },
        new Mount { id = "VLS", arc = 0, halfArc = 180, rangeKm = 40000, label = "MISSILE VLS", shortName = "VLS", loaded = "8 / 8", ammo = "8" },
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
    [Range(1, 8)] public int salvo = 4;
    public bool loaders;
    [Tooltip("PDC quadrants FWD, STBD, AFT, PORT (quads).")]
    public bool[] quads = { true, true, false, false };
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

    [Header("Helm (Step 33h-1)")]
    [Range(0, 359)] public int hdg = 83;
    [Range(0, 359)] public int ordHdg = 83;
    [Tooltip("Throttle %, 0..100 (thr).")]
    [Range(0, 100)] public int thr = 40;
    [Tooltip("Attitude hold: NOSE, STBD, PORT, TAIL, or empty for none (apAtt).")]
    public string apAtt = "NOSE";
    [Tooltip("Approach mode: DIRECT, FLANK, EVASIVE (apMode).")]
    public string apMode = "DIRECT";
    [Tooltip("Index into apRanges (apRange).")]
    public int apRange = 2;
    [Tooltip("Approach hold ranges, km (AP_RANGES).")]
    public int[] apRanges = { 2000, 4000, 8000, 12000, 18000, 24000 };
    [Tooltip("Ordered heading step for the A and D keys, degrees (onKey).")]
    [Tune("Helm")]
    public int headingKeyStep = 3;
    [Tooltip("Degrees the bow swings per tick at full engine-room integrity (tickPower).")]
    [Tune("Helm")]
    public float turnRate = 4f;

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
    public bool Heard(Track t) => !t.own && dradis && System.Array.IndexOf(t.sig, spectrum) >= 0 && t.rangeKm <= PassiveRangeKm;

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

    /// <summary>engBlock: why an engineering repair is refused, or null.</summary>
    public string EngBlock(string id)
    {
        var c = Comp(engSel);
        switch (id)
        {
            case "party": return PartyOut ? "DC PARTY COMMITTED \u2014 " + (partyTicks - partyT) + " TO RUN"
                                          : c.crew < 1 ? "NO CREW ABOARD" : c.integ >= 5 ? "COMPARTMENT AT FULL INTEGRITY" : null;
            case "seal": return !c.vented ? "COMPARTMENT ALREADY SEALED" : c.crew < 1 ? "NO CREW ABOARD" : null;
            case "vent": return c.vented ? "COMPARTMENT ALREADY VENTED" : null;
            case "reboot": return (!string.IsNullOrEmpty(c.cond) || c.integ < 5) ? null : "SYSTEMS NOMINAL \u2014 NOTHING TO RESTART";
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

    /// <summary>applyOrder: a repair is dispatched, not applied; sealing restores air but not a
    /// fire; venting takes the air, so a fire goes with it; reboot clears the condition and adds 1.</summary>
    public void ApplyOrder(string id)
    {
        var c = Comp(engSel);
        switch (id)
        {
            case "party": partyComp = engSel; partyT = 0; break;
            case "seal": c.vented = false; break;
            case "vent": c.vented = true; if (c.cond == "FIRE") c.cond = null; break;
            case "reboot": c.cond = null; c.integ = Mathf.Min(5, c.integ + 1); break;
        }
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

    [Header("Our ship in the world, km (x east, y north). The board moves it (TickMove); TacticalWorld places the model")]
    public double ownX, ownY;

    // Step 41 (owner 2026-10-18): the ship moves. Spec §4: actual chases commanded at capped rates;
    // the hull turns, the velocity follows, and the sideways part bleeds off (the ship slides). The
    // prototype's numbers (MAX_SPEED 240, ACCEL_RATE 80, BRAKE_RATE 150, DRIFT_DECAY .988 and
    // SPACE_DRAG .9992 per 1/60 s) are slowed to the owner's top speed: time stretches by 240/2.5 = 96,
    // so rates divide by 96 squared. That gives about 0.9 G, the design helm's 1.0 G.
    [Header("Movement (Step 41)")]
    [Tooltip("Speed at 100% throttle, km/s (owner: the design's scale; its contacts run 0.4–2.6 km/s).")]
    [Tune("Helm")] public double topSpeedKms = 2.5;
    [Tooltip("Speeding up, km/s per mission second (about 0.9 G).")]
    [Tune("Helm")] public double accelKms2 = 0.0087;
    [Tooltip("Slowing down, km/s per mission second (about 1.7 G).")]
    [Tune("Helm")] public double brakeKms2 = 0.0163;
    [Tooltip("Share of sideways speed kept each mission second: after a turn the ship slides, then lines up.")]
    [Tune("Helm")] public double driftKeepPerS = 0.9925;
    [Tooltip("Share of all speed kept each mission second.")]
    [Tune("Helm")] public double dragKeepPerS = 0.9995;
    [System.NonSerialized] public double vx, vy; // km/s

    public double SpeedKms => System.Math.Sqrt(vx * vx + vy * vy);
    /// <summary>Course over ground, degrees clockwise from north (the way we are actually going).</summary>
    public double CourseDeg => (System.Math.Atan2(vx, vy) * 180 / System.Math.PI + 360) % 360;

    /// <summary>Advance our ship by dt mission seconds: speed along the nose chases the throttle's
    /// target, sideways speed bleeds off, then the position moves.</summary>
    public void TickMove(double dt)
    {
        if (dt <= 0) return;
        double h = hdg * System.Math.PI / 180;
        double nx = System.Math.Sin(h), ny = System.Math.Cos(h); // the nose
        double fwd = vx * nx + vy * ny, side = vx * ny - vy * nx; // side: to starboard
        double keep = System.Math.Pow(dragKeepPerS, dt);
        fwd *= keep;
        side *= keep * System.Math.Pow(driftKeepPerS, dt);
        double target = thr / 100.0 * topSpeedKms; // thr is already held to what ENG power allows (TickHelm)
        double step = (fwd < target ? accelKms2 : brakeKms2) * dt;
        fwd = System.Math.Abs(target - fwd) <= step ? target : fwd + System.Math.Sign(target - fwd) * step;
        vx = fwd * nx + side * ny;
        vy = fwd * ny - side * nx;
        ownX += vx * dt;
        ownY += vy * dt;
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

    /// <summary>orderBlock: the orders station's own refusals (not engBlock's).</summary>
    public string OrderBlock(string id)
    {
        var c = Comp(engSel);
        if (id == "party")
        {
            if (c.integ >= 5) return "COMPARTMENT AT FULL INTEGRITY";
            if (c.vented) return "CANNOT WORK A VENTED COMPARTMENT";
            if (c.crew < 1) return "NO CREW ABOARD";
        }
        if (id == "seal" && !c.vented) return "COMPARTMENT ALREADY SEALED";
        if (id == "vent")
        {
            if (c.vented) return "ALREADY VENTED";
            if (c.crew > 0) return c.crew + " CREW ABOARD";
        }
        return null;
    }

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

    /// <summary>The tick: AUX charges by units x 3.2 to 100.</summary>
    public void TickAux()
    {
        double rate = BusUnits("aux") * auxPerUnit;
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

    void ClearSwitches() { for (int i = 0; i < sw.Length; i++) sw[i] = false; }

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
    public void ToggleLoaders() { loaders = !loaders; ClearSwitches(); }
    public void ToggleQuad(int i) { quads[i] = !quads[i]; }

    /// <summary>computeSol: needs an identified target and the bridge; stores range, relative
    /// bearing and speed for the current system and clears the ladder.</summary>
    public void ComputeSol()
    {
        var t = TrackOf(target);
        if (t == null || !identified.Contains(t.key) || !BriOk) return;
        int rel = ((t.bearing - hdg + 540) % 360) - 180;
        sol[SysIndex] = new Solution { set = true, rangeKm = t.rangeKm, brg = rel, spd = t.speed };
        ClearSwitches(); fired = false;
    }

    /// <summary>nudgeSol: RNG ±100 km, BRG ±1° (±180), SPD ±0.05 (2 dp); clears the ladder.</summary>
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
        }
        if (field == "rng") cur.rangeKm = Mathf.Max(0f, cur.rangeKm + d * 100f);
        if (field == "brg") cur.brg = Mathf.Max(-180, Mathf.Min(180, cur.brg + d));
        if (field == "spd") cur.spd = Mathf.Max(0f, (float)System.Math.Round(cur.spd + d * 0.05, 2, System.MidpointRounding.AwayFromZero));
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
            return;
        }
        if (i > 0 && !sw[i - 1]) return;
        if (i == 1 && !InArc) return; // ANGLE: the turret must bear (Step 48)
        if (i == 0 && (!WepReady || !Sol.set || (sys == "PD" && !loaders))) return;
        if (i == 2 && !OnBearing) return;
        if (i == 3 && wepCharge < ShotCost) return;
        sw[i] = true; fired = false;
    }

    /// <summary>The lever passing 0.9 with the ladder set: rounds away, charge spent (drawWep).
    /// Step 48: refused below the shot's cost; firing releases ARM, one shot per arming (spec 8.4.b);
    /// a railgun shot is a slug that flies (TickRounds); firing spikes our signal (GDD p.42).</summary>
    public void Fire()
    {
        if (!AllSet) return; // moveSlew fires on every new pull while allSet() holds; the panel's per-pull latch stops repeats (Step 33g verification)
        if (wepCharge < ShotCost) return;
        fired = true;
        wepCharge = Mathf.Max(0f, wepCharge - ShotCost);
        sw[3] = false;
        shotsFired++;
        var m = MountOf(mount);
        if (sys == "RG" && m != null)
        {
            double b = (hdg + Sol.brg) * System.Math.PI / 180; // straight out on the solution's bearing (no lead yet)
            Rounds.Add(new Round { x = ownX, y = ownY, dx = System.Math.Sin(b), dy = System.Math.Cos(b), maxKm = m.rangeKm });
            shotReport = null;
            signal = Mathf.Min(100f, signal + fireSignalSpike);
        }
    }

    // ---- rounds in flight (Step 48, owner 2026-10-23) ------------------------------------------

    [System.Serializable]
    public class Round { public double x, y, dx, dy, travelled, maxKm; }
    [System.Serializable]
    public class HitRadius { public string klass; public float km; }

    [System.NonSerialized] private System.Collections.Generic.List<Round> rounds;
    public System.Collections.Generic.List<Round> Rounds => rounds ?? (rounds = new System.Collections.Generic.List<Round>());
    [Tooltip("Railgun slug speed, km/s: the prototype's 4,800 slowed x96 like our ship. About 8 minutes to 24,300 km.")]
    [Tune("Periscope & fire control")] public double slugSpeedKms = 50;
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
    /// cannot skip past a ship: the first ship it passes within that ship's hit radius is hit.</summary>
    public void TickRounds(double dt)
    {
        var list = Rounds;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var r = list[i];
            double step = System.Math.Min(slugSpeedKms * dt, r.maxKm - r.travelled);
            Track struck = null;
            double first = double.MaxValue;
            foreach (var t in tracks)
            {
                if (t.own || !t.hasWorld) continue;
                double px = t.worldX - r.x, py = t.worldY - r.y;
                double u = System.Math.Max(0, System.Math.Min(step, px * r.dx + py * r.dy)); // closest point along the path
                double cx = px - r.dx * u, cy = py - r.dy * u, rad = HitRadiusKm(t.klass);
                if (cx * cx + cy * cy <= rad * rad && u < first) { struck = t; first = u; }
            }
            if (struck != null)
            {
                shotReport = "HIT · " + struck.name + " · " + SideStruck(struck, r);
                lastShotHit = true; shotResults++;
                list.RemoveAt(i);
                continue;
            }
            r.x += r.dx * step; r.y += r.dy * step; r.travelled += step;
            if (r.travelled >= r.maxKm - 1e-6) { shotReport = "MISS"; lastShotHit = false; shotResults++; list.RemoveAt(i); }
        }
    }

    // The side facing the shot (reference sim sideFacing): where it came from, against the ship's bow.
    static string SideStruck(Track t, Round r)
    {
        double from = (System.Math.Atan2(-r.dx, -r.dy) * 180 / System.Math.PI + 360) % 360;
        double rel = ((from - t.worldHdg) % 360 + 360) % 360;
        return rel < 45 || rel >= 315 ? "PROW" : rel < 135 ? "STBD" : rel < 225 ? "AFT" : "PORT";
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
    }

    public void RecallSquad()
    {
        int i = launchedSquads.IndexOf(CurrentSquad.key);
        if (i >= 0) { launchedSquads.RemoveAt(i); launchedOrders.RemoveAt(i); }
        ClearStages();
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

    public void TickPower()
    {
        int total = TotalFill, cap = ReactorCap;
        // drain first, so a bus dropping out of DRAW frees its units the same tick
        foreach (var b in buses) if (b.mode == BusMode.Vent && b.fill > 0) { b.fill--; total--; }
        foreach (var b in buses) if (b.mode == BusMode.Draw && b.fill < busMax && total < cap) { b.fill++; total++; }
        for (int i = buses.Length - 1; i >= 0 && total > cap; i--)
            while (buses[i].fill > 0 && total > cap) { buses[i].fill--; total--; }
    }

    // ---- the tick — weapons capacitor: +5 per WEP unit up to 100, bleeds 3 a tick with no WEP --

    public void TickWeapons()
    {
        float feed = BusUnits("wep") * wepFeedPerUnit;
        wepCharge = feed > 0f ? Mathf.Min(100f, wepCharge + feed) : Mathf.Max(0f, wepCharge - wepBleed);
    }

    // ---- the tick — launch telemetry: a thrown stage runs out over several ticks -------------

    public void TickLaunch()
    {
        foreach (var g in stages) g.progress = g.on ? Mathf.Min(100f, g.progress + g.rate) : 0f;
    }

    // ---- the tick — damage-control party ---------------------------------------------------

    public void TickParty()
    {
        if (!PartyOut) return;
        int t = partyT + 1;
        if (t >= partyTicks)
        {
            var c = Comp(partyComp);
            if (c != null) c.integ = Mathf.Min(5, c.integ + RepairRate);
            partyT = -1;
            partyComp = null;
        }
        else partyT = t;
    }

    // ---- helm ------------------------------------------------------------------------------

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

    /// <summary>thrCap: ENG units x 20 x engine-room factor.</summary>
    public int ThrCap => JsRound((double)BusUnits("eng") * 20 * EngFactorD);
    double EngFactorD => CompHealth("eng") / 5.0;

    /// <summary>gLoadNum: 1 G plus 1 G at full throttle, plus the approach mode's extra.</summary>
    public double GLoad => 1 + thr / 100.0 + (apMode == "FLANK" ? 0.5 : apMode == "EVASIVE" ? 1.2 : 0);
    /// <summary>stabNum: 45 points lost per G over 1, 0..100.</summary>
    public double Stab => System.Math.Max(0, System.Math.Min(100, 100 - (GLoad - 1) * 45));

    /// <summary>Ordered heading minus heading, -180..179.</summary>
    public int TurnLeft => ((ordHdg - hdg + 540) % 360) - 180;

    /// <summary>thrDown: from the pointer's height in the fader well, top = 100, capped at the press.</summary>
    public void SetThr(double fromTop01, int cap) => thr = System.Math.Max(0, System.Math.Min(System.Math.Min(100, cap), JsRound((1 - fromTop01) * 100)));
    /// <summary>setAtt: click to hold, click again to release.</summary>
    public void SetAtt(string k) => apAtt = apAtt == k ? "" : k;
    public void SetApMode(string k) => apMode = k;
    public void StepApRange(int d) => apRange = System.Math.Max(0, System.Math.Min(apRanges.Length - 1, apRange + d));
    /// <summary>onKey: A and D step the ordered heading and clear the attitude hold.</summary>
    public void StepOrdHdg(int dir) { ordHdg = ((ordHdg + dir * headingKeyStep) % 360 + 360) % 360; apAtt = ""; }

    /// <summary>The tick: the throttle falls to ENG units x 20 (no engine-room factor here, as the
    /// board does — HANDOFF_GAPS row 32); the bow swings toward the ordered heading.</summary>
    public void TickHelm()
    {
        int engCap = BusUnits("eng") * 20;
        if (thr > engCap) thr = engCap;
        if (hdg != ordHdg)
        {
            int d = TurnLeft;
            int rate = System.Math.Max(1, JsRound(turnRate * EngFactorD));
            hdg = System.Math.Abs(d) <= rate ? ordHdg : ((hdg + System.Math.Sign(d) * rate) % 360 + 360) % 360;
        }
    }

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

    public void PruneBearings(float now) { BearingFixes.RemoveAll(f => now - f.t >= bearingTrailSeconds); }

    public void TickSensors()
    {
        int senCap = Mathf.Max(1, BusUnits("sen"));
        if (senBoost > senCap) senBoost = senCap;
        // every unit drawn makes us louder, and so does ACTIVE SCAN (GDD p18, spec 3.1.b, prototype getSig)
        float want = SignalTarget;
        if (signal < want) signal = Mathf.Min(want, signal + signalRise);
        else if (signal > want) signal = Mathf.Max(want, signal - signalFall);
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
