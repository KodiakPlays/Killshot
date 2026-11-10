using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// The campaign's saves (owner 2026-11-08): three slots, one small file each, in the folder Unity gives the game.
/// On a Mac: ~/Library/Application Support/Shagun Shah/Starwolf. On Windows: AppData/LocalLow/Shagun Shah/Starwolf.
/// The folder's name comes from the Company Name and Product Name in Project Settings → Player: change either and
/// the game looks in a new, empty folder. The files stay when the game closes and when a new build is installed.
/// The game saves after each mission won and after a repair. It writes the new file beside the old one, then swaps
/// them, and keeps the one before as .bak: a crash while saving never spoils a save.
/// </summary>
public static class SaveGame
{
    /// <summary>What a save keeps. A later build may add fields: an older file still loads, and the new fields start
    /// at their defaults here. Raise Version when a field changes meaning.</summary>
    [System.Serializable]
    public class Data
    {
        public int version = Version;
        public string savedAt = "";
        public List<string> done = new List<string>(); // the levels won, by scene name
        public int dotsShown; // dots the sector map has shown: a newer one fades in the first time
        [Tooltip("Owner 2026-11-08: our ship's damage after the last mission won, kept for the next one; empty when it is whole.")]
        public List<Part> damage = new List<Part>();
    }

    /// <summary>One part of our ship: a side's armour, or a room's integrity (0 to 5).</summary>
    [System.Serializable]
    public class Part
    {
        public string name;
        public float now, most;
    }

    public const int Version = 1;
    public const int Slots = 3;

    /// <summary>The slot being played, 0 to 2, or -1 outside the campaign (the level list, or play mode started on the board).</summary>
    public static int Slot { get; private set; } = -1;
    public static Data Current { get; private set; }
    public static bool InCampaign => Slot >= 0 && Current != null;

    /// <summary>The Campaign file. The sector map hands it over when it opens; the end screen reads it.</summary>
    public static Campaign campaign;

    /// <summary>True when this level is the campaign's last mission and every mission is won: the end card comes next.</summary>
    public static bool FinishedWith(string level)
    {
        if (!InCampaign || campaign == null || campaign.missions.Length == 0) return false;
        return campaign.missions[campaign.missions.Length - 1].level == level && campaign.Won(Current.done) == campaign.missions.Length;
    }

    static string PathOf(int slot) => Path.Combine(Application.persistentDataPath, "slot" + (slot + 1) + ".json");
    public static bool Exists(int slot) => File.Exists(PathOf(slot));

    /// <summary>A slot's save, or null if it is empty or can't be read (then its .bak, if there is one).</summary>
    public static Data Read(int slot) => ReadFile(PathOf(slot)) ?? ReadFile(PathOf(slot) + ".bak");

    static Data ReadFile(string path)
    {
        try { return File.Exists(path) ? JsonUtility.FromJson<Data>(File.ReadAllText(path)) : null; }
        catch (System.Exception e) { Debug.LogWarning("[SaveGame] Can't read " + path + ": " + e.Message); return null; }
    }

    /// <summary>Play this slot: its save, or a new campaign if it is empty (written at once).</summary>
    public static void Play(int slot)
    {
        Slot = slot;
        Current = Read(slot);
        if (Current == null) { Current = new Data(); Save(); }
    }

    /// <summary>Back to the title: no campaign until a slot is picked again.</summary>
    public static void Leave() { Slot = -1; Current = null; }

    /// <summary>Write the save. False if it couldn't (a full disk, or the file in use): the game goes on, and the old save stays.</summary>
    public static bool Save()
    {
        if (!InCampaign) return false;
        Current.version = Version;
        Current.savedAt = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        string path = PathOf(Slot), fresh = path + ".new";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(fresh, JsonUtility.ToJson(Current, true));
            if (File.Exists(path)) File.Replace(fresh, path, path + ".bak");
            else File.Move(fresh, path);
            return true;
        }
        catch (System.Exception e) { Debug.LogWarning("[SaveGame] Can't save " + path + ": " + e.Message); return false; }
    }

    public static void Delete(int slot)
    {
        foreach (var p in new[] { PathOf(slot), PathOf(slot) + ".bak", PathOf(slot) + ".new" })
            try { if (File.Exists(p)) File.Delete(p); }
            catch (System.Exception e) { Debug.LogWarning("[SaveGame] Can't delete " + p + ": " + e.Message); }
        if (Slot == slot) Leave();
    }

    /// <summary>Owner 2026-11-08: in the campaign, damage stays on the ship from one mission to the next until it is repaired.
    /// A won mission keeps our ship's armour and rooms as they ended (fires and crew don't stay). The level list's levels
    /// still start whole.</summary>
    // A win's damage waits here until MISSION SELECT: RESTART MISSION on the win screen replays from the damage before it.
    private static List<Part> wonDamage;

    public static void KeepDamage(DamageSystem d)
    {
        if (!InCampaign || d == null) return;
        var parts = new List<Part>();
        for (int i = 0; i < DamageSystem.Sides.Length; i++) parts.Add(new Part { name = DamageSystem.Sides[i] + " ARMOUR", now = d.SideArmour[i], most = d.stats.sideArmourMax[i] });
        foreach (var c in d.compartments) parts.Add(new Part { name = c.name, now = Mathf.Max(1, c.integ), most = 5 }); // a room comes back at 1 at least: a reactor at 0 is a lost ship
        wonDamage = parts.Exists(p => p.now < p.most) ? parts : new List<Part>();
    }

    /// <summary>MISSION SELECT after a win: the win's damage goes into the save.</summary>
    public static void KeepWonDamage()
    {
        if (!InCampaign || wonDamage == null) return;
        Current.damage = wonDamage;
        wonDamage = null;
        Save();
    }

    /// <summary>A campaign mission starts with the damage kept from the last one.</summary>
    public static void PutDamage(DamageSystem d)
    {
        wonDamage = null; // a mission starting (or started again) drops a win's damage not yet kept
        if (!InCampaign || d == null) return;
        foreach (var p in Current.damage)
        {
            int side = System.Array.IndexOf(DamageSystem.Sides, p.name.Replace(" ARMOUR", ""));
            if (side >= 0) { d.SideArmour[side] = Mathf.Min(p.now, d.stats.sideArmourMax[side]); continue; }
            foreach (var c in d.compartments) if (c.name == p.name) c.integ = Mathf.Clamp(Mathf.RoundToInt(p.now), 1, 5);
        }
    }

    /// <summary>ENGINEERING's REPAIR ALL: the ship is whole again, and saved.</summary>
    public static void Repair()
    {
        if (!InCampaign) return;
        Current.damage.Clear();
        Save();
    }

    /// <summary>A mission won in the campaign: noted and saved. Outside the campaign it does nothing.</summary>
    public static void Won(string level)
    {
        if (!InCampaign || string.IsNullOrEmpty(level)) return;
        if (!Current.done.Contains(level)) Current.done.Add(level);
        Save();
    }
}
