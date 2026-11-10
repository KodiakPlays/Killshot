// =================================================================================================
// SensorSystem.cs
//
// What it does:
// One ship's sensors: DRADIS, the ACTIVE SCAN cone, and the ship's signal (how easy it is to
// hear). Every ship has one. The Sensors panel on the board shows and sets our ship's sensors.
// It decides what other ships are heard, scanned and in sight.
//
// How it works:
// - DRADIS hears a ship when DRADIS is on and the SEN bus has power. The ship must be in range and
//   give off the picked band: THERM, RAD, UV or EM. UV hears less far. Rocks on the line cut the
//   range.
// - The range grows with the gain, from 1 to 5. The gain cannot go above the units on the SEN
//   bus. If the SEN power drops below the gain, the gain goes back to 1.
// - ACTIVE SCAN covers a cone. The player turns it and makes it wider or narrower. Letting go of
//   the cone dial scans every ship inside it. Clicking a heard ship's row scans just that one, if
//   it is inside the cone.
// - A scan names the ship and notes its course, its speed and the time. WeaponSystem uses that
//   note for its shot.
// - A ship is in sight when its near side is inside visual range and no rock is on the line. It
//   takes half the box's shorter side off the range. It is in sight near one of our fighters too.
// - When the DRADIS sweep touches a ship, the plot asks it to note the ship's place and the time.
//   Out of sight, the plot shows the ship at that place until the next pass.
// - It counts each ship the first time it is heard or named. BoardManager shows the count on the
//   alarm strip until the player clears it.
// - Every board step the signal moves slowly toward its target. Each power unit on a bus adds to
//   it. ACTIVE SCAN or a weapons lock adds more. In a gas cloud the ship keeps only a share.
// - The ranges, signal rates and gas cloud numbers come from the ship's stats file (the Tuning
//   window).
//
// Technical:
// - Data: senBoost is the gain, 1 to 5. spectrum is the band. coneBrg and coneArc are degrees.
//   coneArc is half the cone's width. signal is 0 to 100. identified lists scanned track keys.
//   It starts with bc1, our boat's track, set in the Inspector.
// - Contacts are ShipState.tracks. Heard, ConeCovers and InSight test one Track. A Track's
//   interference, from rocks, cuts the range.
// - SensorsPanel calls ToggleDradis, ToggleActive, SetGain, SetSpectrum, SetCone, ConeWiden,
//   ConeNarrow, ResolveLit and Identify.
// - PlotPanel calls See as the sweep passes a ship, and NoteScan. It reads Heard and InSight.
// - WeaponSystem reads ScanOf (course and speed at the scan), Heard and InSight for its shot.
// - BoardManager calls Tick, NoteNewContacts and AckContacts. Tick runs for our ship only.
// - TacticalWorld sets inCloud each step. EnemyBrain reads our signal and inCloud.
// - InSight uses the other ship's NavSystem.BoxHalf, and our fighters through
//   HangarSystem.SeesShip.
// - SignalTarget adds each bus's units times PowerSystem's busSignalPerUnit. ACTIVE SCAN or a lock
//   (WeaponSystem.lockKey) adds activeScanSignal.
// - Numbers: SensorSystem.Stats, the sensors part of each ship's stats file: ranges, cone steps,
//   signal rates and the gas cloud numbers.
// =================================================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SensorSystem : MonoBehaviour
{
    [Header("DRADIS and active scan — the board's initial state")]
    public bool dradis = true;
    public bool active;
    [Tooltip("DRADIS gain, 1–5, capped by the SEN bus. Owner 2026-10-25: at load it is at its lowest (power is cold); "
           + "if the SEN bus can no longer carry it, it drops back to 1 and stays there until you raise it again.")]
    [Range(1, 5)] public int senBoost = 1;
    public string[] spectra = { "THERM", "RAD", "UV", "EM" };
    public string spectrum = "THERM";
    [Tooltip("0..360: the board stores a rounded 360 as 360, not 0 (moveCone).")]
    [Range(0, 360)] public int coneBrg = 62;
    [Tooltip("Half-width of the active scan cone, degrees; NARROW / WIDEN step it.")]
    public int coneArc = 40;

    [Header("Signal — how detectable this ship is, 0–100 (Step 35, owner 2026-10-17)")]
    [Tooltip("Our signal: how detectable we are, 0–100. Internal; the player sees a whole percent on the OWN "
           + "EMISSIONS meter, the cells and the alert. It climbs and falls gradually toward SignalTarget.")]
    [Range(0, 100)] public float signal;

    /// <summary>Owner 2026-11-01: this ship is in a gas cloud (TacticalWorld sets it each tick).</summary>
    [System.NonSerialized] public bool inCloud;

    [Tooltip("Identified by sensors (idd).")]
    public List<string> identified = new List<string> { "bc1" };

    /// <summary>The ship this belongs to. Sensors read its contacts, its SEN bus and where it is.</summary>
    [System.NonSerialized] public ShipState ship;

    /// <summary>This ship's numbers for DRADIS, scan and signal: its section of the ship's stats file (Assets/Ships,
    /// Starwolf → Tuning).</summary>
    [System.Serializable]
    public class Stats
    {
        [Tooltip("Sensor range = base + per step x gain (senRangeUnits). Owner 2026-10-26: 30,000 units at full gain (was 47,000).")]
        [FormerlySerializedAs("senRangeBaseKm")] public float senRangeBaseUnits = 12000f;
        [FormerlySerializedAs("senRangePerStepKm")] public float senRangePerStepUnits = 3600f;
        [Tooltip("Owner 2026-10-26: inside this range a contact is in sight: its model shows and glides. Outside it, "
               + "the plot shows it only where the sweep last touched it.")]
        [FormerlySerializedAs("visualRangeKm")] public float visualRangeUnits = 10000f;
        [Tooltip("UV hears to this fraction of the range (passiveRangeUnits).")]
        public float uvRangeFactor = 0.55f;
        public int coneArcMin = 15, coneArcMax = 90, coneArcStep = 10;
        [Tooltip("Signal climbs this much per game second toward its target.")]
        public float signalRise = 9f;
        [Tooltip("Signal falls this much per game second toward its target.")]
        public float signalFall = 6f;
        [Tooltip("Signal ACTIVE SCAN adds while it is on. Default 35 (reference sim: a scan is worth 3.5 units).")]
        public float activeScanSignal = 35f;
        [Tooltip("At or above this our signal shows red and the alert tone sounds once. Whether an enemy hears us is its own "
               + "brain's call (EnemyBrain.HearsUs, owner 2026-10-28).")]
        public float signalTrackedAt = 45f;
        [Tooltip("Gas clouds (owner 2026-11-01): this share of the ship's box must be in a cloud for it to count as inside.")]
        [Range(0f, 1f)] public float cloudInsideShare = 0.9f;
        [Tooltip("Inside a gas cloud the ship keeps this share of its signal.")]
        [Range(0f, 1f)] public float cloudSignalShare = 0.3f;
        [Tooltip("Inside a gas cloud an enemy sees the ship only from this close, units.")]
        public float cloudSeenUnits = 400f;
    }
    /// <summary>The numbers this part plays by: its section of the ship's stats file (a copy while the game runs).</summary>
    public Stats stats => shipStats ? shipStats.Stats.sensors : (shipStats = GetComponent<Ship>()).Stats.sensors;
    [System.NonSerialized] Ship shipStats;

    public float SenRangeUnits => stats.senRangeBaseUnits + senBoost * stats.senRangePerStepUnits;
    public float PassiveRangeUnits => spectrum == "UV" ? SenRangeUnits * stats.uvRangeFactor : SenRangeUnits;
    public int GainCap => Mathf.Max(1, ship.power.BusUnits("sen"));

    /// <summary>DRADIS on, with at least 1 unit on the SEN bus (Step 82, owner: no power, no hearing).</summary>
    public bool Listening => dradis && ship.power.BusUnits("sen") >= 1;

    /// <summary>heard: DRADIS listening, the contact emits in the chosen band, and it is in range.</summary>
    public bool Heard(ShipState.Track t) => !t.own && !t.wreck && Listening && System.Array.IndexOf(t.sig, spectrum) >= 0 && t.rangeUnits <= PassiveRangeUnits - t.interference;

    /// <summary>coneCovers: active scan on, inside the cone, inside sensor range.</summary>
    public bool ConeCovers(ShipState.Track t) => active && ShipState.AngDiff(t.bearing, coneBrg) <= coneArc && t.rangeUnits <= SenRangeUnits - t.interference;
    /// <summary>The same for a spot at this bearing and range (owner 2026-11-02: a rock answers a ping).</summary>
    public bool ConeCovers(double brg, double range) => active && ShipState.AngDiffD(brg, coneBrg) <= coneArc && range <= SenRangeUnits;

    /// <summary>Contacts in band and range for a spectrum key (spectra[].count).</summary>
    public int CountIn(string band)
    {
        float range = band == "UV" ? SenRangeUnits * stats.uvRangeFactor : SenRangeUnits;
        int n = 0;
        foreach (var t in ship.tracks) if (!t.own && System.Array.IndexOf(t.sig, band) >= 0 && t.rangeUnits <= range - t.interference) n++;
        return n;
    }

    public void ToggleDradis() { dradis = !dradis; }
    public void ToggleActive() { active = !active; }

    /// <summary>startGain: 1..5, capped by the SEN bus.</summary>
    public void SetGain(int v) { senBoost = Mathf.Max(1, Mathf.Min(5, Mathf.Min(GainCap, v))); }

    public void SetSpectrum(string band) { spectrum = band; }
    public void SetCone(int brg) { coneBrg = Mathf.Clamp(brg, 0, 360); }
    public void ConeWiden() { coneArc = Mathf.Min(stats.coneArcMax, coneArc + stats.coneArcStep); }
    public void ConeNarrow() { coneArc = Mathf.Max(stats.coneArcMin, coneArc - stats.coneArcStep); }

    /// <summary>resolveLit: on releasing the cone dial, every contact inside the cone is scanned.</summary>
    public void ResolveLit()
    {
        foreach (var t in ship.tracks) if (ConeCovers(t)) NoteScan(t);
    }

    /// <summary>identify: a contact-board row scans its contact if the cone covers it and it is heard.</summary>
    public void Identify(string key)
    {
        foreach (var t in ship.tracks)
            if (t.key == key && ConeCovers(t) && Heard(t)) NoteScan(t);
    }

    /// <summary>What a scan read (owner 2026-11-01: a scan helps the shot): a ship's course and speed at that moment, and
    /// when. It goes out of date if the ship turns.</summary>
    public class ScanFix { public float course, speed, t; }
    [System.NonSerialized] private Dictionary<string, ScanFix> scanFixes;
    public Dictionary<string, ScanFix> ScanFixes => scanFixes ?? (scanFixes = new Dictionary<string, ScanFix>());
    /// <summary>The last scan of a contact still identified, or null.</summary>
    public ScanFix ScanOf(string key) { ScanFix f; return identified.Contains(key) && ScanFixes.TryGetValue(key, out f) ? f : null; }

    /// <summary>A scan reaches a contact: it is identified, and its course and speed now are noted.</summary>
    public void NoteScan(ShipState.Track t)
    {
        if (!identified.Contains(t.key)) identified.Add(t.key);
        ScanFixes[t.key] = new ScanFix { course = (float)t.course, speed = t.speed, t = ship.missionNow };
    }

    /// <summary>Contacts first heard or identified since the alarm strip was last acknowledged
    /// (Step 44; owner 2026-10-18: an alert shows only for a real alert). The design raised the strip
    /// once at load and never again.</summary>
    [System.NonSerialized] public int newContacts;
    [System.NonSerialized] private HashSet<string> seenContacts;

    /// <summary>Count each contact once: the first time DRADIS hears it or a scan identifies it.
    /// Returns true when there is a new one.</summary>
    public bool NoteNewContacts()
    {
        if (seenContacts == null) seenContacts = new HashSet<string>();
        int before = newContacts;
        foreach (var t in ship.tracks)
            if (!t.own && (Heard(t) || identified.Contains(t.key)) && seenContacts.Add(t.key)) newContacts++;
        return newContacts > before;
    }

    public void AckContacts() { newContacts = 0; }

    /// <summary>True bearing from this ship to a contact, degrees, unrounded.</summary>
    public double ExactBearing(ShipState.Track t)
    {
        if (!t.hasWorld) return t.bearing;
        return (System.Math.Atan2(t.worldX - ship.nav.x, t.worldY - ship.nav.y) * 180 / System.Math.PI + 360) % 360;
    }

    /// <summary>Step 75 (owner 2026-10-26): the sweep touches a contact. Out of sight, the plot shows it here until the next pass.</summary>
    public void See(ShipState.Track t) { t.seen = true; t.seenAt = ship.missionNow; t.seenX = t.worldX; t.seenY = t.worldY; t.seenBrg = ExactBearing(t); t.seenRangeUnits = t.rangeUnits; }
    /// <summary>Step 75: close enough to see with the eye.</summary>
    public bool InSight(ShipState.Track t)
    {
        if (!t.hasWorld) return false;
        // owner 2026-11-10: in sight once its near side is in visual range, not its middle: a station thousands of units across
        // showed as a dot while we were beside it. The near side is its smaller half-size (a ship's half width, a few tens of units).
        float near = t.nav ? Mathf.Min(t.nav.BoxHalf.x, t.nav.BoxHalf.y) : 0f;
        if (!t.rockOnLine && t.rangeUnits - near <= stats.visualRangeUnits) return true;
        return ship.hangar && ship.hangar.SeesShip(t.worldX, t.worldY, stats.visualRangeUnits); // Step 101: our fighters scout
    }

    /// <summary>One board tick of dt game seconds (it reads the bus fill this tick committed): the gain
    /// falls back if SEN power can no longer carry it, and the signal moves toward its target.</summary>
    public void Tick(double dt)
    {
        int senCap = Mathf.Max(1, ship.power.BusUnits("sen"));
        if (senBoost > senCap) senBoost = 1; // owner 2026-10-25: power lost, the setting goes back to the bottom; you set it again
        // every unit drawn makes us louder, and so does ACTIVE SCAN (GDD p18, spec 3.1.b, prototype getSig)
        float want = SignalTarget;
        if (signal < want) signal = Mathf.Min(want, signal + stats.signalRise * (float)dt);
        else if (signal > want) signal = Mathf.Max(want, signal - stats.signalFall * (float)dt);
    }

    /// <summary>Where our signal is heading: each bus's units × its signal per unit, plus ACTIVE SCAN, 0–100.</summary>
    public float SignalTarget
    {
        get
        {
            // owner 2026-11-01: a lock is as loud as ACTIVE SCAN: quick, but it can be heard; plotting is slow and silent
            float t = active || (ship.weapons != null && ship.weapons.lockKey != null) ? stats.activeScanSignal : 0f;
            for (int i = 0; i < ship.power.buses.Length; i++) t += ship.power.buses[i].fill * ship.power.stats.busSignalPerUnit[i];
            t = Mathf.Clamp(t, 0f, 100f);
            return inCloud ? t * stats.cloudSignalShare : t; // owner 2026-11-01: a gas cloud cools the ship; a shot's spike still goes on in full
        }
    }
}
