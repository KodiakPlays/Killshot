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
    }

    public const int Version = 1;
    public const int Slots = 3;

    /// <summary>The slot being played, 0 to 2, or -1 outside the campaign (the level list, or play mode started on the board).</summary>
    public static int Slot { get; private set; } = -1;
    public static Data Current { get; private set; }
    public static bool InCampaign => Slot >= 0 && Current != null;

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

    /// <summary>A mission won in the campaign: noted and saved. Outside the campaign it does nothing.</summary>
    public static void Won(string level)
    {
        if (!InCampaign || string.IsNullOrEmpty(level)) return;
        if (!Current.done.Contains(level)) Current.done.Add(level);
        Save();
    }
}
