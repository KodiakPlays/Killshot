// =================================================================================================
// SaveGame.cs
//
// What it does:
// The campaign's save files. There are three save slots, one small text file each, in the folder
// Unity gives the game on that computer. It is not on any object. The save slots screen, the
// sector map, the ship rooms, the end screen and QuestSystem call it.
//
// How it works:
// - A save keeps the levels won, the save time and our ship's damage. It keeps how many sector
//   map dots have shown. The damage is each side's armour and each room's state.
// - Picking a slot reads its file. An empty slot starts a new campaign and saves it at once.
//   Reading falls back to the .bak copy if the main file is missing or broken.
// - Saving writes a new file next to the old one, then swaps them. The file before is kept as
//   .bak. If saving fails, the game goes on and the old save stays.
// - At the start of a campaign mission it puts the saved damage back on our ship's DamageSystem.
// - On a win it notes the level as won and saves. It keeps the ship's damage aside. That
//   damage goes into the save when the player picks MISSION SELECT.
// - A room always comes back with at least 1 point. Fires, air loss and radiation are not kept.
// - On a win it also keeps the rounds left in each gun and launcher, the marines and each room's
//   crew. Marines still out on boats are lost. The next mission starts with those. Nothing goes
//   above the stats file's numbers.
// - REPAIR ALL in the ship rooms clears the saved damage and saves.
// - Delete removes a slot's file and its copies. Outside the campaign, every save step does
//   nothing.
// - The sector map hands it the Campaign file. FinishedWith tells the end screen when the last
//   mission is won. Then the end card comes next.
//
// Technical:
// - Data: SaveGame.Data has version, savedAt, done, dotsShown, damage and stores. done lists won
//   levels by scene name.
// - damage and stores are lists of Part: a name, now and most. stores names each gun or launcher
//   by its label, then MARINES, then each room's name plus CREW.
// - Files: slot1.json to slot3.json in Application.persistentDataPath, written with JsonUtility.
//   Older files still load. Raise Version when a field changes meaning.
// - The folder name comes from Company Name and Product Name in Project Settings, Player. Change
//   either and the game looks in a new, empty folder.
// - Slot is -1 outside the campaign. Every save step checks InCampaign first.
// - Callers: SaveSlots (Read, Play, Delete), SectorMap (campaign, Current, Save) and QuestSystem
//   (PutDamage, KeepDamage, Won).
// - More callers: EndScreenPanel (FinishedWith, KeepWonDamage), ShipRooms (Repair), EndCard and
//   ScreenFlow (Leave).
// - KeepDamage takes full rounds, marines and crew from Ship.statsFile. Full armour comes from
//   DamageSystem.stats. PutDamage only lowers the ship's numbers.
// - Traps: parts match by name. A renamed room or gun label loses what was saved for it.
// - Trap: a win's damage and stores wait in memory until KeepWonDamage. PutDamage drops them. So
//   does quitting. REPAIR ALL clears the damage only.
// =================================================================================================

using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
