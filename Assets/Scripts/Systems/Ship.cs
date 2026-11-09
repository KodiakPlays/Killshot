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
