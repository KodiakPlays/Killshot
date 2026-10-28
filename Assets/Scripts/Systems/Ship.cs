using UnityEngine;

/// <summary>
/// A ship: the object that carries its parts (NavSystem, PowerSystem and the rest) and points at its stats
/// file (owner, 2026-10-27). Our ship and every enemy have one. Each part reads its numbers through here.
/// While the game runs, the ship plays by a copy of its file, so nothing a run does changes the file;
/// Starwolf → Tuning writes to the file and to that copy, so an edit shows at once and is kept.
/// </summary>
public class Ship : MonoBehaviour
{
    [Tooltip("This ship's numbers (Assets/Ships). Starwolf → Tuning edits them.")]
    public ShipStats statsFile;
    [System.NonSerialized] ShipStats live;

    /// <summary>The numbers this ship plays by: its file, or the copy of it while the game runs.</summary>
    public ShipStats Stats => !Application.isPlaying ? statsFile : live ? live : (live = Instantiate(statsFile));

    /// <summary>Copy the file's numbers into the running copy (the Tuning window calls this after an edit in play).</summary>
    public void PullFromFile() { if (live) JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(statsFile), live); }

    // Values the ship changes as it plays start from the file: the salvo, the PDC loaders, the marines aboard,
    // the crew in each room.
    private void Awake()
    {
        var w = GetComponent<WeaponSystem>();
        if (w) { w.salvo = Stats.weapons.salvo; w.loaders = Stats.weapons.loaders; }
        var h = GetComponent<HangarSystem>();
        if (h) h.marines = Stats.hangar.marines;
        var d = GetComponent<DamageSystem>();
        if (d) for (int i = 0; i < d.compartments.Length && i < Stats.damage.crew.Length; i++) d.compartments[i].crew = Stats.damage.crew[i];
    }
}
