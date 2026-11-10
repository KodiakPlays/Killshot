// =================================================================================================
// Ship.cs
//
// What it does:
// The main script on every ship object: our ship, the Garuda and the battle group ships. It points
// at the ship's stats file in Assets/Ships. The ship's systems (NavSystem, PowerSystem and the
// rest) read their numbers through it. It sizes the ship's 3D model.
//
// How it works:
// - In the editor the systems read the stats file itself. In play mode they read a copy of it.
//   Nothing a game does changes the file.
// - When the Tuning window changes a number in play mode, it calls this script. The copy then
//   takes the file's new numbers at once.
// - When the ship starts, it sets the numbers that change in play from the file: how many
//   torpedoes fire together, the PDC loaders, the marines, the number of fighters, and the crew in
//   each room.
// - It scales the 3D model evenly. Its longest side then matches the Length in the stats file. It
//   checks again every frame and rescales if the Length changes.
// - It notes the model's width as a share of its length. The plot uses it to place fighters.
// - It keeps a list of every ship in play. The plot and the ship cameras use the list.
//
// Technical:
// - Data: statsFile is the ShipStats file in Assets/Ships. Stats gives the file in the editor. In
//   play mode it gives a copy, made on first read.
// - Each system's stats reads Ship.Stats: NavSystem, PowerSystem, SensorSystem, WeaponSystem,
//   HangarSystem, DamageSystem and OrdersSystem.
// - StarwolfTuningWindow calls PullFromFile. It copies the file onto the copy with JsonUtility,
//   then calls WeaponSystem.KeepSelection. Trap: rounds left are in the copy. A Tuning edit in
//   play mode fills them up again.
// - Awake sets WeaponSystem salvo and loaders, HangarSystem marines and KeepFighters, and each
//   room's crew.
// - All lists every enabled ship. PlotPanel and ShipView read All, Models and WidthShare.
// - FitModel scales each child with a Renderer, size and place. It measures the ship unturned.
//   LateUpdate refits when Length changes.
// - Trap: the model's longer side, across or along, is set to Length.
// - Numbers: all in the stats file. The only Inspector setting is the file itself.
// =================================================================================================

using UnityEngine;

public class Ship : MonoBehaviour
{
    [Tooltip("This ship's numbers (Assets/Ships). Starwolf → Tuning edits them.")]
    public ShipStats statsFile;
    [System.NonSerialized] ShipStats live;

    /// <summary>The numbers this ship plays by: its file, or the copy of it while the game runs.</summary>
    public ShipStats Stats => !Application.isPlaying ? statsFile : live ? live : (live = Instantiate(statsFile));

    /// <summary>Copy the file's numbers into the running copy (the Tuning window calls this after an edit in play).</summary>
    public void PullFromFile()
    {
        if (!live) return;
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(statsFile), live);
        var w = GetComponent<WeaponSystem>();
        if (w) w.KeepSelection();
    }

    /// <summary>Every ship in play. Each ship's camera films only its own model; the close-up leaves some out.</summary>
    public static readonly System.Collections.Generic.List<Ship> All = new System.Collections.Generic.List<Ship>();
    private Renderer[] models;
    /// <summary>The parts of the ship's 3D model.</summary>
    public Renderer[] Models => models ??= GetComponentsInChildren<Renderer>(true);
    public NavSystem Nav { get; private set; }
    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);

    /// <summary>The model's width as a share of its length, wings and all (owner 2026-11-02: fighters come out beside it).</summary>
    public float WidthShare { get; private set; } = 0.5f;
    float fitted;
    private void LateUpdate() { if (Stats.length != fitted) FitModel(); }

    // Owner 2026-11-09: the 3D model is scaled evenly so its longest side is the Length number, and follows it when
    // Tuning changes it. Measured with the ship unturned; the model grows or shrinks about the ship's centre.
    void FitModel()
    {
        fitted = Stats.length;
        var rot = transform.rotation;
        transform.rotation = Quaternion.identity;
        Bounds b = default;
        bool any = false;
        foreach (var r in Models)
            if (r && r.gameObject.activeInHierarchy) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        transform.rotation = rot;
        float now = any ? Mathf.Max(b.size.x, b.size.y) : 0f;
        if (now <= 0f || fitted <= 0f) return;
        WidthShare = Mathf.Min(b.size.x, b.size.y) / now;
        float k = fitted / now;
        foreach (Transform c in transform)
            if (c.GetComponentInChildren<Renderer>(true)) { c.localScale *= k; c.localPosition *= k; }
    }

    // Values the ship changes as it plays start from the file: the salvo, the PDC loaders, the marines aboard,
    // the crew in each room.
    private void Awake()
    {
        Nav = GetComponent<NavSystem>();
        var w = GetComponent<WeaponSystem>();
        if (w) { w.salvo = Stats.weapons.salvo; w.loaders = Stats.weapons.loaders; }
        var h = GetComponent<HangarSystem>();
        if (h) { h.marines = Stats.hangar.marines; h.KeepFighters(Stats.hangar.fighters); }
        var d = GetComponent<DamageSystem>();
        if (d) for (int i = 0; i < d.compartments.Length && i < Stats.damage.crew.Length; i++) d.compartments[i].crew = Stats.damage.crew[i];
        FitModel();
    }
}
