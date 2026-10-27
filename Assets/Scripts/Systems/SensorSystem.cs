using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sensors: one ship's DRADIS and active scan. It holds the gain, the band, the scan cone, the ship's own
/// signal (how loud it is), which contacts are identified, and the bearing trail. It decides what is heard,
/// what the cone covers and what is in sight. Our ship and every enemy carry one (the enemy is our ship with
/// other stats). Owner 2026-10-26: sensors live in one place, here. The contact list stays on the ship
/// state (HandoffBoardState.tracks); this reads it. Rules from board-logic.js (heard, coneCovers, spectra
/// count, trackers, startGain, moveCone, resolveLit, identify, getSig) and Steps 35, 42–45 and 75.
/// </summary>
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

    [Tooltip("Identified by sensors (idd).")]
    public List<string> identified = new List<string> { "bc1" };

    /// <summary>The ship this belongs to. Sensors read its contacts, its SEN bus and where it is.</summary>
    [System.NonSerialized] public HandoffBoardState ship;

    /// <summary>This ship's numbers for DRADIS, scan and signal: its section of the ship's stats file (Assets/Ships,
    /// Starwolf → Tuning).</summary>
    [System.Serializable]
    public class Stats
    {
        [Tooltip("Sensor range = base + per step x gain (senRangeKm). Owner 2026-10-26: 30,000 km at full gain (was 47,000).")]
        public float senRangeBaseKm = 12000f, senRangePerStepKm = 3600f;
        [Tooltip("Owner 2026-10-26: inside this range a contact is in sight: its model shows and glides. Outside it, "
               + "the plot shows it only where the sweep last touched it.")]
        public float visualRangeKm = 10000f;
        [Tooltip("UV hears to this fraction of the range (passiveRangeKm).")]
        public float uvRangeFactor = 0.55f;
        public int coneArcMin = 15, coneArcMax = 90, coneArcStep = 10;
        [Tooltip("Signal climbs this much per game second toward its target.")]
        public float signalRise = 9f;
        [Tooltip("Signal falls this much per game second toward its target.")]
        public float signalFall = 6f;
        [Tooltip("Signal ACTIVE SCAN adds while it is on. Default 35 (reference sim: a scan is worth 3.5 units).")]
        public float activeScanSignal = 35f;
        [Tooltip("Own emissions at or above this mean hostile sets have your range (sigNote, blink).")]
        public float signalTrackedAt = 45f;
        [Tooltip("A bearing line fades out over this much mission time (owner 2026-10-23: 2 minutes).")]
        public float bearingTrailSeconds = 120f;
    }
    /// <summary>The numbers this part plays by: its section of the ship's stats file (a copy while the game runs).</summary>
    public Stats stats => shipStats ? shipStats.Stats.sensors : (shipStats = GetComponent<Ship>()).Stats.sensors;
    [System.NonSerialized] Ship shipStats;

    public float SenRangeKm => stats.senRangeBaseKm + senBoost * stats.senRangePerStepKm;
    public float PassiveRangeKm => spectrum == "UV" ? SenRangeKm * stats.uvRangeFactor : SenRangeKm;
    public int GainCap => Mathf.Max(1, ship.power.BusUnits("sen"));

    /// <summary>DRADIS on, with at least 1 unit on the SEN bus (Step 82, owner: no power, no hearing).</summary>
    public bool Listening => dradis && ship.power.BusUnits("sen") >= 1;

    /// <summary>heard: DRADIS listening, the contact emits in the chosen band, and it is in range.</summary>
    public bool Heard(HandoffBoardState.Track t) => !t.own && !t.wreck && Listening && System.Array.IndexOf(t.sig, spectrum) >= 0 && t.rangeKm <= PassiveRangeKm;

    /// <summary>coneCovers: active scan on, inside the cone, inside sensor range.</summary>
    public bool ConeCovers(HandoffBoardState.Track t) => active && HandoffBoardState.AngDiff(t.bearing, coneBrg) <= coneArc && t.rangeKm <= SenRangeKm;

    /// <summary>Contacts in band and range for a spectrum key (spectra[].count).</summary>
    public int CountIn(string band)
    {
        float range = band == "UV" ? SenRangeKm * stats.uvRangeFactor : SenRangeKm;
        int n = 0;
        foreach (var t in ship.tracks) if (!t.own && System.Array.IndexOf(t.sig, band) >= 0 && t.rangeKm <= range) n++;
        return n;
    }

    /// <summary>trackers: hostile sets that have your range once emissions reach the threshold.</summary>
    public int Trackers()
    {
        if (signal < stats.signalTrackedAt) return 0;
        int n = 0;
        foreach (var t in ship.tracks) if (!t.own && t.hostile) n++;
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

    /// <summary>resolveLit: on releasing the cone dial, every contact inside the cone is identified.</summary>
    public void ResolveLit()
    {
        foreach (var t in ship.tracks) if (ConeCovers(t) && !identified.Contains(t.key)) identified.Add(t.key);
    }

    /// <summary>identify: a contact-board row identifies its contact if the cone covers it and it is heard.</summary>
    public void Identify(string key)
    {
        foreach (var t in ship.tracks)
            if (t.key == key && ConeCovers(t) && Heard(t) && !identified.Contains(key)) identified.Add(key);
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

    /// <summary>Step 45 (owner 2026-10-23): each radar sweep that passes a heard contact leaves its
    /// bearing line where our ship was. As we move, the lines cross at the target: that is how a
    /// passive contact's range is found. Exact bearings; the lines fade out.</summary>
    [System.Serializable]
    public class BearingFix { public string key; public double x, y, brg; public float t; }
    [System.NonSerialized] private List<BearingFix> bearingFixes;
    public List<BearingFix> BearingFixes => bearingFixes ?? (bearingFixes = new List<BearingFix>());

    /// <summary>True bearing from this ship to a contact, degrees, unrounded.</summary>
    public double ExactBearing(HandoffBoardState.Track t)
    {
        if (!t.hasWorld) return t.bearing;
        return (System.Math.Atan2(t.worldX - ship.nav.x, t.worldY - ship.nav.y) * 180 / System.Math.PI + 360) % 360;
    }

    public void LayBearing(HandoffBoardState.Track t, float now) { BearingFixes.Add(new BearingFix { key = t.key, x = ship.nav.x, y = ship.nav.y, brg = ExactBearing(t), t = now }); }

    /// <summary>Step 75 (owner 2026-10-26): the sweep touches a contact. Out of sight, the plot shows it here until the next pass.</summary>
    public void See(HandoffBoardState.Track t) { t.seen = true; t.seenX = t.worldX; t.seenY = t.worldY; t.seenBrg = ExactBearing(t); t.seenRangeKm = t.rangeKm; }
    /// <summary>Step 75: close enough to see with the eye.</summary>
    public bool InSight(HandoffBoardState.Track t) => t.hasWorld && t.rangeKm <= stats.visualRangeKm;

    public void PruneBearings(float now) { BearingFixes.RemoveAll(f => now - f.t >= stats.bearingTrailSeconds); }

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
            float t = active ? stats.activeScanSignal : 0f;
            for (int i = 0; i < ship.power.buses.Length; i++) t += ship.power.buses[i].fill * ship.power.stats.busSignalPerUnit[i];
            return Mathf.Clamp(t, 0f, 100f);
        }
    }
}
