using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The standing end-of-feature playtest (owner, 2026-10-17): plays the real user flow — intro, menu
/// NEW GAME, briefing, board — then sets up the ship, checking each step, and films the run with
/// Unity Recorder. Output: Logs/Playtests/&lt;time&gt;/ — playtest.mp4, report.md, shots/*.png. Each run
/// deletes every earlier run's video; screenshots stay for the newest KeepRunsWithShots runs; every report stays.
/// Board controls are clicked where they are on screen, through the EventSystem, after checking the
/// control is the topmost thing there (so a blocked control fails). The intro menu and the
/// briefing's Enter are read through the old Input class, which a script cannot press, so the run
/// calls what those presses call (StarwolfIntroController.Activate(0), ScreenFlow.Advance) and the
/// report says so. Grows as features land.
/// </summary>
[InitializeOnLoad]
public static class PlaytestRunner
{
    const string PendingKey = "Starwolf.Playtest.Run", ReturnSceneKey = "Starwolf.Playtest.ReturnScene";
    const string GameViewSizeKey = "Starwolf.Playtest.GameViewSize";
    const string StartScene = "Assets/Scenes/Tactical/00_Start.unity";

    static IEnumerator routine;
    static object waiting;
    static float waitUntil;
    static string runDir;
    static RecorderController recorder;
    static readonly StringBuilder report = new StringBuilder();
    static int passed, failed;

    static PlaytestRunner()
    {
        EditorApplication.playModeStateChanged += OnPlayMode;
    }

    [MenuItem("Starwolf/Playtest/Run power-up playtest")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("[Playtest] Stop play mode first."); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        SessionState.SetString(ReturnSceneKey, SceneManager.GetActiveScene().path);
        SessionState.SetInt(GameViewSizeKey, GameViewSize(-1));
        EditorSceneManager.OpenScene(StartScene, OpenSceneMode.Single);
        SessionState.SetString(PendingKey, DateTime.Now.ToString("yyyy-MM-dd_HHmmss"));
        EditorApplication.isPlaying = true;
    }

    static void OnPlayMode(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredPlayMode)
        {
            string stamp = SessionState.GetString(PendingKey, "");
            if (stamp.Length == 0) return;
            SessionState.EraseString(PendingKey);
            Begin(stamp);
        }
        else if (change == PlayModeStateChange.ExitingPlayMode && routine != null)
        {
            Finish("stopped early (play mode exited)");
        }
        else if (change == PlayModeStateChange.EnteredEditMode)
        {
            string back = SessionState.GetString(ReturnSceneKey, "");
            if (back.Length > 0) { SessionState.EraseString(ReturnSceneKey); EditorSceneManager.OpenScene(back, OpenSceneMode.Single); }
            int size = SessionState.GetInt(GameViewSizeKey, -1);
            if (size >= 0) { SessionState.EraseInt(GameViewSizeKey); GameViewSize(size); }
        }
    }

    // If the recorder has to switch the Game view to its own size (an odd window), it leaves it there; give the owner
    // theirs back. Reads the Game view's size choice (set < 0) or sets it. Internal API, so guarded.
    static int GameViewSize(int set)
    {
        var t = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        var views = t != null ? Resources.FindObjectsOfTypeAll(t) : null;
        if (views == null || views.Length == 0) return -1;
        const System.Reflection.BindingFlags F = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        var index = t.GetProperty("selectedSizeIndex", F);
        if (index == null) return -1;
        if (set < 0) return (int)index.GetValue(views[0]);
        var pick = t.GetMethod("SizeSelectionCallback", F); // what the size menu itself calls
        if (pick != null) pick.Invoke(views[0], new object[] { set, null });
        else index.SetValue(views[0], set);
        ((EditorWindow)views[0]).Repaint();
        return set;
    }

    // ---- run --------------------------------------------------------------------------------

    // A run's video is about 120 MB and is not watched again once the run is checked (owner, 2026-10-26): every
    // earlier run's video goes when a new run starts. Screenshots stay for the newest few runs (owner, 2026-10-24).
    // Every report.md stays: BUILD_LOG cites runs as evidence.
    const int KeepRunsWithShots = 3;

    static void PruneOldRuns(string playtests)
    {
        if (!Directory.Exists(playtests)) return;
        var runs = Directory.GetDirectories(playtests);
        Array.Sort(runs, StringComparer.Ordinal); // folder names are timestamps, so oldest first
        for (int i = 0; i < runs.Length; i++)
        {
            try
            {
                File.Delete(Path.Combine(runs[i], "playtest.mp4"));
                string shots = Path.Combine(runs[i], "shots");
                if (i < runs.Length - (KeepRunsWithShots - 1) && Directory.Exists(shots)) Directory.Delete(shots, true); // this run makes the last one
            }
            catch (Exception e) { Debug.LogWarning("[Playtest] could not clear " + runs[i] + ": " + e.Message); }
        }
    }

    static void Begin(string stamp)
    {
        string playtests = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/Playtests"));
        PruneOldRuns(playtests);
        runDir = Path.Combine(playtests, stamp);
        Directory.CreateDirectory(Path.Combine(runDir, "shots"));
        report.Length = 0; passed = failed = 0;
        report.AppendLine("# Playtest — power-up run, " + stamp).AppendLine();
        report.AppendLine("| # | Step | Result | Detail |").AppendLine("|---|---|---|---|");
        StartRecording();
        routine = PowerUpRun();
        waiting = null;
        EditorApplication.update += Tick;
    }

    static void Tick()
    {
        if (routine == null) return;
        if (waiting is Func<bool> cond)
        {
            if (!cond() && Time.realtimeSinceStartup < waitUntil) return;
        }
        else if (waiting != null && Time.realtimeSinceStartup < waitUntil) return;
        waiting = null;
        bool more;
        try { more = routine.MoveNext(); }
        catch (Exception e) { Check("run", false, "exception: " + e.Message); Finish("crashed"); return; }
        if (!more) { Finish("complete"); return; }
        waiting = routine.Current;
        if (waiting is float secs) waitUntil = Time.realtimeSinceStartup + secs;
        else if (waiting is Func<bool>) waitUntil = Time.realtimeSinceStartup + 15f; // timeout
    }

    static void Finish(string how)
    {
        EditorApplication.update -= Tick;
        routine = null;
        if (recorder != null && recorder.IsRecording()) recorder.StopRecording();
        recorder = null;
        report.AppendLine().AppendLine("**Result: " + (failed == 0 ? "PASS" : "FAIL") + "** — " + passed + " passed, " + failed + " failed. Run " + how + ".");
        report.AppendLine().AppendLine("Video: `playtest.mp4`. Screenshots: `shots/`.");
        File.WriteAllText(Path.Combine(runDir, "report.md"), report.ToString());
        Debug.Log("[Playtest] " + (failed == 0 ? "PASS" : "FAIL") + " (" + passed + " passed, " + failed + " failed). Report: " + runDir + "/report.md");
        // the recorder finishes writing the MP4 after it stops; leave play mode only once it has
        exitAt = Time.realtimeSinceStartup + 2f;
        EditorApplication.update += ExitWhenDone;
    }

    static float exitAt;
    static void ExitWhenDone()
    {
        if (Time.realtimeSinceStartup < exitAt) return;
        EditorApplication.update -= ExitWhenDone;
        if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
    }

    static void StartRecording()
    {
        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        var movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movie.name = "Playtest";
        movie.Enabled = true;
        movie.EncoderSettings = new CoreEncoderSettings { Codec = CoreEncoderSettings.OutputCodec.MP4, EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High };
        // Film the Game view as the owner has it — 16:9 at 1x — never a fixed 1920x1080 (owner, 2026-10-26).
        // MP4 refuses odd sizes, so an odd window is trimmed by a pixel; only then does the recorder resize the view.
        var view = new GameViewInputSettings();
        int w = view.OutputWidth & ~1, h = view.OutputHeight & ~1;
        if (w != view.OutputWidth || h != view.OutputHeight) { view.OutputWidth = w; view.OutputHeight = h; }
        movie.ImageInputSettings = view;
        movie.CaptureAudio = false;
        movie.OutputFile = Path.Combine(runDir, "playtest");
        settings.AddRecorderSettings(movie);
        settings.SetRecordModeToManual();
        // constant: the game steps one video frame at a time, so the film shows game time exactly. Variable
        // rate fed the Mac encoder duplicate timestamps and it rejected frames (first run, 2026-10-17).
        settings.FrameRatePlayback = FrameRatePlayback.Constant;
        settings.FrameRate = 30f;
        settings.CapFrameRate = true;
        recorder = new RecorderController(settings);
        recorder.PrepareRecording();
        if (!recorder.StartRecording()) Debug.LogWarning("[Playtest] the recorder did not start; the run goes on without video");
    }

    // ---- the script -------------------------------------------------------------------------

    static IEnumerator PowerUpRun()
    {
        // 1. Intro runs itself to the menu
        var intro = UnityEngine.Object.FindAnyObjectByType<Starwolf.Intro.StarwolfIntroController>();
        Check("Intro scene loads", intro != null, SceneManager.GetActiveScene().name);
        if (intro == null) yield break;
        yield return (Func<bool>)(() => intro.MenuVisible);
        Check("Intro reaches the menu by itself", intro.MenuVisible, "after " + intro.Elapsed.ToString("0.0") + " s");
        Shot("01_menu");
        yield return 1.5f;

        // 2. NEW GAME (the menu reads the old Input class; call what the press calls)
        intro.Activate(0);
        yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "01_Briefing");
        Check("NEW GAME opens the briefing", SceneManager.GetActiveScene().name == "01_Briefing", "called Activate(0), as pressing NEW GAME does");
        yield return 2f;
        Shot("02_briefing");
        yield return 0.2f;

        // 2b. Step 83: if the INITIALISING screen can't start (here, no shader), it still calls finished, so Enter
        //     leads on to the board. It logs one error, which is expected.
        var probe = new GameObject("Playtest INITIALISING probe");
        probe.SetActive(false);
        var probeIntro = probe.AddComponent<Starwolf.Intro.StarwolfIntroController>();
        probeIntro.initialisingOnly = true; probeIntro.displayShader = null;
        bool probeDone = false;
        probeIntro.finished.AddListener(() => probeDone = true);
        probe.SetActive(true);
        Check("INITIALISING can't start (no shader): it still calls finished, so Enter leads on", probeDone && !probeIntro.enabled, "");
        UnityEngine.Object.Destroy(probe);

        // 3. Briefing → board (Step 114, owner 2026-10-29: the briefing is the level list; pick WOLF HUNT — call what a
        //    click or Enter on its row calls)
        var rows = System.Array.ConvertAll(System.Array.FindAll(UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.InstanceID),
                                           b => b.onClick.GetPersistentMethodName(0) == "Play"), b => b.name); // the level rows, not QUIT
        var wolfRow = GameObject.Find("WOLF HUNT");
        bool wolfFound = wolfRow; // read now: the row is gone once the board loads
        if (wolfFound) wolfRow.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "02.1_Grid");
        Check("The level list has its four levels; picking WOLF HUNT opens the board on Level_01",
              rows.Length == 4 && wolfFound && SceneManager.GetActiveScene().name == "02.1_Grid" && TacticalWorld.chosenLevel == "Level_01",
              string.Join(", ", rows) + "; called the WOLF HUNT row's onClick, as a click or Enter does");
        yield return 1.5f;
        var board = UnityEngine.Object.FindAnyObjectByType<BoardManager>();
        Check("The board is running", board != null, "");
        if (board == null) yield break;
        var s = board.State;
        var quest = UnityEngine.Object.FindAnyObjectByType<QuestSystem>();
        Check("The mission is on the board, at its start", quest && quest.Stage == QuestStage.LocateTarget, quest ? quest.Stage.ToString() : "no QuestSystem");
        if (!quest) yield break;
        quest.enabled = false; // stand-in: the run kills both ships part way and tests on; the mission comes back on at the end (25)
        Shot("03_board");
        yield return 0.2f;

        // 3b. Step 73: the level starts the player's ship at its Player Spawn (Level_01: centre bottom, facing north). The
        //     checks below were written for the board's own spots, so both ships go back there before time starts.
        var playerSpawn = UnityEngine.Object.FindAnyObjectByType<PlayerSpawn>();
        int startHdg = playerSpawn ? ShipState.JsRound(((-playerSpawn.transform.eulerAngles.z) % 360 + 360) % 360) % 360 : -1;
        Check("The player's ship starts at the level's Player Spawn, facing its way",
              playerSpawn && System.Math.Abs(s.nav.x - playerSpawn.transform.position.x) < 0.5 && System.Math.Abs(s.nav.y - playerSpawn.transform.position.y) < 0.5 && s.nav.heading == startHdg,
              playerSpawn ? "at " + s.nav.x.ToString("N0") + ", " + s.nav.y.ToString("N0") + " km, heading " + s.nav.heading.ToString("000") + "°" : "no Player Spawn in the level");
        var world = UnityEngine.Object.FindAnyObjectByType<TacticalWorld>();
        world.SpawnPlayer(Vector3.zero, 0f);
        // Step 75: 17,500 units on the board's bearing 048° (it was 24,300): DRADIS reaches 15,600 units at gain 1 and 19,200 at gain 2 now
        world.Spawn("t07", new Vector3(13002.06f, 11707.11f, 0f), 0f);

        // 4. Starting state: undamaged, dummy present, power cold
        bool undamaged = true;
        foreach (var c in s.damage.compartments) if (c.integ != 5 || c.vented || !string.IsNullOrEmpty(c.cond)) undamaged = false;
        Check("Loads undamaged (all rooms green and sealed)", undamaged, "");
        Check("The dummy enemy is the only contact", s.tracks.Length == 1 && s.tracks[0].key == "t07",
              s.tracks.Length > 0 ? s.tracks[0].name + " at " + s.tracks[0].bearing.ToString("000") + "°, " + s.tracks[0].rangeUnits.ToString("N0") + " km" : "none");
        Check("Power starts cold (every bus empty, on STANDBY)", BusesAre(s, PowerSystem.BusMode.Standby, 0), Buses(s));

        // 5. Paused at load; signal silent
        Check("The board starts paused", board.Paused, "");
        Check("Signal starts at 0 — RUNNING SILENT", s.sensors.signal == 0f && SigNote(board) == "RUNNING SILENT", s.sensors.signal.ToString("0.0") + ", \"" + SigNote(board) + "\"");
        Check("DRADIS starts off: the player turns it on (owner 2026-10-29)", !s.sensors.dradis, "dradis " + s.sensors.dradis);
        s.sensors.ToggleDradis(); // stand-in for the DRADIS rocker (its drawer is shut here; the rocker is clicked in step 16)

        // 5b. Step 82 (owner): no power on the SEN bus, DRADIS hears nothing. Stand-in for the gain fader: gain 2
        //     reaches TRACK 07 (19,200 units), but the bus is empty.
        var plot0 = board.GetComponentInChildren<PlotPanel>(true);
        int gainWas = s.sensors.senBoost;
        s.sensors.senBoost = 2;
        plot0.Refresh();
        Check("No SEN power: DRADIS hears nothing, and the plot shows no circle and no sweep",
              s.sensors.dradis && !s.sensors.Listening && s.tracks[0].rangeUnits <= s.sensors.PassiveRangeUnits && !s.sensors.Heard(s.tracks[0]) && plot0.SensorRingUnits == 0f && !plot0.SweepShown,
              "DRADIS " + (s.sensors.dradis ? "on" : "off") + ", SEN " + s.power.BusUnits("sen") + ", TRACK 07 at " + s.tracks[0].rangeUnits.ToString("N0") + " of " + s.sensors.PassiveRangeUnits.ToString("N0") + " km");
        s.sensors.senBoost = gainWas;
        plot0.Refresh();

        // 6. Open the Power drawer from the systems rail
        int snd = board.Sounds;
        if (!Click("drawer", "power", "PWR key")) yield break;
        Check("A drawer key sounds the tab click (Step 40)", board.Sounds == snd + 1 && board.LastSound == "tab", (board.Sounds - snd) + " cue(s), last \"" + board.LastSound + "\"");
        yield return 0.8f;
        var drawer = board.GetComponentInChildren<PowerPanel>(true).gameObject;
        Check("The Power drawer opens", drawer != null && drawer.activeInHierarchy, "");
        Shot("04_power_open");
        yield return 0.2f;

        // 7. While paused, set every bus to DRAW. Power waits (owner 2026-10-17: nothing real changes
        //    while paused). Setting all four first also shares the reactor evenly: it serves buses
        //    first come, first served, so a bus sent to DRAW late can find the reactor already spent.
        string[] names = { "ENG", "WEP", "SEN", "AUX" };
        snd = board.Sounds;
        for (int i = 0; i < 4; i++)
        {
            if (!Click("power.bus", i.ToString(), names[i] + " selector")) yield break;
            Check(names[i] + " bus set to DRAW", s.power.buses[i].mode == PowerSystem.BusMode.Draw, s.power.buses[i].mode.ToString());
            yield return 0.3f;
        }
        Check("Each selector sounds the switch click", board.Sounds == snd + 4 && board.LastSound == "switch", (board.Sounds - snd) + " cue(s), last \"" + board.LastSound + "\"");
        yield return 1.5f;
        Check("While paused, the buses do not fill", s.power.TotalFill == 0, "total fill " + s.power.TotalFill + " after 1.5 s paused");
        Shot("05_all_draw");
        yield return 0.2f;

        s.sensors.SetGain(1); // stand-in: the gain fader down, so TRACK 07 is first heard when step 17 raises it
        // 8. RUN
        if (!Click("pause", null, "PAUSE/RUN key")) yield break;
        Check("RUN starts time", !board.Paused, "");

        // 9. Let the reactor fill the buses
        yield return (Func<bool>)(() => s.power.TotalFill >= s.power.ReactorCap || AllFull(s));
        yield return 1f;
        Check("The reactor fills the buses up to its output", s.power.TotalFill == Mathf.Min(s.power.ReactorCap, s.power.stats.busMax * 4), "total " + s.power.TotalFill + " of " + s.power.ReactorCap + " — " + Buses(s));
        Shot("06_powered");
        yield return 0.2f;

        //     TRACK 07 patrols (Step 51): its brain orders a heading and a throttle; its own ship model
        //     moves it with the same maths as ours.
        var brain = GameObject.Find("World").GetComponentInChildren<EnemyBrain>();
        if (brain) brain.firesBack = false; // stand-in: it holds its fire until the point-defence step (21e)
        if (brain) brain.hunts = false; // stand-in: it does not listen for us; Wolf Hunt's own checks turn it on
        // stand-in: no hit starts a condition until the conditions step (24), so the steps before stay as written
        float[] condChances = { s.weapons.stats.fireChanceHE, s.weapons.stats.fireChancePEN, s.weapons.stats.breachChanceSlug, s.damage.stats.radChanceReactor }; // the first three are the shooter's (Weapons)
        s.weapons.stats.fireChanceHE = s.weapons.stats.fireChancePEN = s.weapons.stats.breachChanceSlug = s.damage.stats.radChanceReactor = 0f;
        // stand-in: the run flies through Level_01's debris, and a collision (Step 93) would dent the armour the checks expect
        s.damage.stats.collisionArmourDamage = 0f; s.damage.stats.collisionCompartmentDamage = 0;
        var t07Ship = brain ? brain.GetComponent<WorldContact>().Ship : null;
        // the Garuda has its own stats file (Step 80), no longer a copy of ours: its hits start no condition either
        if (t07Ship != null) t07Ship.weapons.stats.fireChanceHE = t07Ship.weapons.stats.fireChancePEN = t07Ship.weapons.stats.breachChanceSlug = t07Ship.damage.stats.radChanceReactor = 0f;
        Check("GARUDA has a patrol brain", brain && brain.enabled, "");
        if (!brain) yield break;
        double ex0 = t07Ship.nav.x, ey0 = t07Ship.nav.y;
        s.nav.throttle = 0; // stand-in: our helm at zero throttle, so our own ship holds still while TRACK 07 moves
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return 3f;
        if (!Click("rate", "1", "x1 rate key")) yield break;
        yield return 0.1f; // Step 75: the models are drawn at the new rate from the next frame
        s.nav.vx = s.nav.vy = 0;
        double movedUnits = System.Math.Sqrt((t07Ship.nav.x - ex0) * (t07Ship.nav.x - ex0) + (t07Ship.nav.y - ey0) * (t07Ship.nav.y - ey0));
        var wp = brain.Waypoint;
        int toWp = TacticalWorld.Bearing(new Vector2((float)(wp.x - t07Ship.nav.x), (float)(wp.y - t07Ship.nav.y)));
        Check("GARUDA gets under way on its patrol, toward its next waypoint", movedUnits > 1 && t07Ship.nav.SpeedUnits > 0.05 && System.Math.Abs(((t07Ship.nav.orderedHeading - toWp + 540) % 360) - 180) <= 1,
              movedUnits.ToString("0") + " km, " + t07Ship.nav.SpeedUnits.ToString("0.00") + " km/s, ordered " + t07Ship.nav.orderedHeading.ToString("000") + "°, heading " + t07Ship.nav.heading.ToString("000") + "°");
        var dummyT = brain.transform;
        //     Step 75: the model glides on between ticks, never more than one tick's travel ahead.
        float glideUnits = (float)(t07Ship.nav.SpeedUnits * board.Rate) + 0.05f;
        Check("Its 3D model follows it", Mathf.Abs(dummyT.position.x - (float)t07Ship.nav.x) < glideUnits && Mathf.Abs(dummyT.position.y - (float)t07Ship.nav.y) < glideUnits,
              "model " + new Vector2(dummyT.position.x - (float)t07Ship.nav.x, dummyT.position.y - (float)t07Ship.nav.y).magnitude.ToString("0.00") + " km off (one tick's glide " + glideUnits.ToString("0.00") + ")");
        Check("The board's track reads its real speed", Mathf.Abs(s.TrackOf("t07").speed - (float)t07Ship.nav.SpeedUnits) < 0.01f, s.TrackOf("t07").speed.ToString("0.00") + " km/s");
        //     Stand-in: hold it still for the static-target checks below, until leading lands (Step 52).
        brain.enabled = false;
        t07Ship.nav.vx = t07Ship.nav.vy = 0;

        // 10. What the power now drives (board rules)
        Check("ENG power raises the throttle limit", s.nav.ThrottleCap > 0, "throttle cap " + s.nav.ThrottleCap + "%");
        Check("SEN power raises the sensor gain limit", s.sensors.GainCap > 1, "gain limit " + s.sensors.GainCap + " (range is " + s.sensors.SenRangeUnits.ToString("N0") + " km until the gain is turned up — sensors feature)");
        float wep0 = s.weapons.wepCharge; double aux0 = s.orders.auxCharge;
        yield return 2f;
        Check("WEP power charges the capacitor", s.weapons.wepCharge > wep0 || s.weapons.wepCharge >= 100f, wep0.ToString("0") + " → " + s.weapons.wepCharge.ToString("0") + "%");
        Check("AUX power charges the order pool", s.orders.auxCharge > aux0 || s.orders.auxCharge >= 100, aux0.ToString("0") + " → " + s.orders.auxCharge.ToString("0") + "%");

        // 11. Power makes us loud (Step 35): signal climbs to its target on the OWN EMISSIONS meter
        if (!Click("drawer", "sensors", "SENS key")) yield break;
        yield return 0.8f;
        float target = s.sensors.SignalTarget;
        Check("Power sets the signal target", Mathf.Approximately(target, Mathf.Min(100f, s.power.TotalFill * 10f)), "target " + target.ToString("0.0") + " from " + s.power.TotalFill + " units");
        Shot("07_signal_climbing");
        yield return 0.2f;
        yield return (Func<bool>)(() => s.sensors.signal >= target);
        Check("The signal climbs to its target", s.sensors.signal >= target, s.sensors.signal.ToString("0.0") + ", \"" + SigNote(board) + "\"");
        Check("Loud enough to be tracked", s.sensors.signal < s.sensors.stats.signalTrackedAt || SigNote(board).Contains("YOUR RANGE"), "\"" + SigNote(board) + "\"");
        Shot("08_signal_loud");
        yield return 0.2f;

        // 12. LOAD SHED: every bus vents, the signal falls back to silent
        if (!Click("drawer", "power", "PWR key")) yield break;
        yield return 0.8f;
        var rockers = board.GetComponent<Rockers>();
        var shed = Hit("power.shed", null).GetComponent<Image>();
        var shed0 = shed.sprite;
        if (!Click("power.shed", null, "EMERGENCY LOAD SHED")) yield break;
        Check("LOAD SHED vents every bus", BusesAreMode(s, PowerSystem.BusMode.Vent), Buses(s));
        bool slid = false;
        float until = Time.realtimeSinceStartup + 0.6f;
        yield return (Func<bool>)(() => (slid |= rockers.Moving("power.shed", null)) || Time.realtimeSinceStartup > until);
        yield return (Func<bool>)(() => !rockers.Moving("power.shed", null));
        Check("The LOAD SHED pad slides down (Step 40)", slid && shed.sprite != shed0 && shed.sprite.name.EndsWith("_thrown") && !shed.transform.Find("Pad cover").GetComponent<Image>().enabled,
              shed0.name + " → " + shed.sprite.name + (slid ? ", slide seen, now at rest" : ", no slide seen"));
        yield return (Func<bool>)(() => s.power.TotalFill == 0);
        if (!Click("drawer", "sensors", "SENS key")) yield break;
        yield return (Func<bool>)(() => s.sensors.signal <= 0f);
        Check("With no power out, the signal falls to silent", s.sensors.signal <= 0f && SigNote(board) == "RUNNING SILENT", s.sensors.signal.ToString("0.0") + ", \"" + SigNote(board) + "\"");
        Shot("09_silent");
        yield return 0.2f;

        // 13. Plot tools (Step 36): POINT is a timestamp, RULER a distance, COMPASS around our ship.
        //     Marks stay put in space when our ship moves.
        if (!Click("drawer", "sensors", "SENS key (close the drawer)")) yield break;
        yield return 0.6f;
        board.SetZoom(X1); // the old x1 view: these checks were written at it
        int plotSounds = 0;
        if (!Click("tool", "POINT", "POINT tool")) yield break;
        snd = board.Sounds;
        if (!PlotDrag(board, new Vector2(1100, 400), new Vector2(1100, 400), "drop a POINT")) yield break;
        plotSounds += board.Sounds - snd;
        yield return 0.2f;
        if (!Click("tool", "RULER", "RULER tool")) yield break;
        snd = board.Sounds;
        if (!PlotDrag(board, new Vector2(700, 700), new Vector2(820, 640), "draw a RULER")) yield break;
        plotSounds += board.Sounds - snd;
        yield return 0.2f;
        if (!Click("tool", "COMPASS", "COMPASS tool")) yield break;
        snd = board.Sounds;
        if (!PlotDrag(board, new Vector2(1200, 545), new Vector2(1300, 445), "draw a COMPASS")) yield break;
        plotSounds += board.Sounds - snd;
        Check("Drawing on the plot makes no click", plotSounds == 0, plotSounds + " cue(s)");
        yield return 0.3f;
        Check("Three marks placed (POINT, RULER, COMPASS)", s.marks.Count == 3, s.marks.Count + " marks");
        if (s.marks.Count != 3) yield break;
        string pt0 = MarkLabel(board, 0), ru0 = MarkLabel(board, 1), co0 = MarkLabel(board, 2);
        Vector2 ptPos0 = MarkLabelPos(board, 0), ruPos0 = MarkLabelPos(board, 1);
        Check("POINT shows the mission time", pt0.Contains(":") && !pt0.Contains("RNG"), "\"" + pt0 + "\"");
        Check("RULER reads only a distance (owner 2026-10-27)", ru0.EndsWith(" km") && !ru0.Contains("BRG"), "\"" + ru0 + "\"");
        Check("COMPASS reads range and bearing from our ship", co0.Contains(" km · BRG ") && co0.EndsWith("°"), "\"" + co0 + "\"");
        Shot("10_marks");
        yield return 0.2f;

        //     Put our ship 5,000 units west (the board owns its position since Step 41). Owner 2026-10-29: the plot
        //     follows our ship, so it holds still on screen and the world slides east under it; marks stay put in
        //     space, so they slide with it. West, away from TRACK 07: east took it inside gain 1's 15,600 units
        //     (Step 75), and it was heard before the hearing step.
        var own = GameObject.Find("World").transform.Find("OwnShip");
        var shipImg = (RectTransform)GameObject.Find("Own ship (3D view)").transform;
        double home = s.nav.x;
        Vector2 ship0 = shipImg.anchoredPosition;
        s.nav.x = home - 5000.0;
        double shift = 5000.0 * PlotScale / 1000.0 * board.ZoomExact; // px the world slides east on the plot
        yield return 1.2f; // a few board ticks
        string pt1 = MarkLabel(board, 0), ru1 = MarkLabel(board, 1), co1 = MarkLabel(board, 2);
        Vector2 ptPos1 = MarkLabelPos(board, 0), ruPos1 = MarkLabelPos(board, 1);
        Check("The 3D model follows the board's position", Mathf.Abs(own.position.x - (float)(home - 5000.0)) < 0.01f, "model x " + own.position.x.ToString("0") + " km");
        Check("The plot follows our ship: it holds still on screen", (shipImg.anchoredPosition - ship0).magnitude < 0.5f,
              "moved " + (shipImg.anchoredPosition - ship0).magnitude.ToString("0.0") + " px");
        Check("The POINT stays put in space: it slides with the world", Mathf.Abs(ptPos1.x - ptPos0.x - (float)shift) < 1f && Mathf.Abs(ptPos1.y - ptPos0.y) < 1f && pt1 == pt0,
              "slid " + (ptPos1.x - ptPos0.x).ToString("0.0") + " px (expected " + shift.ToString("0.0") + "), label unchanged");
        Check("The RULER stays put in space", Mathf.Abs(ruPos1.x - ruPos0.x - (float)shift) < 1f && Mathf.Abs(ruPos1.y - ruPos0.y) < 1f && ru1 == ru0, // labels sit on whole px
              "slid " + (ruPos1.x - ruPos0.x).ToString("0.0") + " px, still \"" + ru1 + "\"");
        Check("The COMPASS re-measures from our ship", co1 != co0 && co1.Contains(" km · BRG "), "\"" + co0 + "\" → \"" + co1 + "\"");
        Shot("11_ship_moved");
        yield return 0.2f;
        s.nav.x = home;
        yield return 1.2f;
        Check("Moving back restores every label", MarkLabel(board, 0) == pt0 && MarkLabel(board, 1) == ru0 && MarkLabel(board, 2) == co0, "");

        //     ERASE pressed twice clears the plot
        if (!Click("tool", "ERASE", "ERASE tool")) yield break;
        if (!Click("tool", "ERASE", "ERASE tool (again)")) yield break;
        yield return 0.3f;
        Check("ERASE twice clears every mark", s.marks.Count == 0, s.marks.Count + " marks left");
        Shot("12_cleared");
        yield return 0.5f;

        // 14. Press and click (Step 40). With no firing solution set, the first arming rocker is locked.
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.8f;
        var rung = Hit("fc.flip", "0").GetComponent<Image>();
        Check("The first arming rocker is locked (no solution yet)", rung.sprite.name.EndsWith("_locked"), rung.sprite.name);
        snd = board.Sounds;
        if (!Click("fc.flip", "0", "arming rocker 1 (locked)")) yield break;
        bool nudged = false;
        until = Time.realtimeSinceStartup + 0.6f;
        yield return (Func<bool>)(() => (nudged |= rockers.Moving("fc.flip", "0")) || Time.realtimeSinceStartup > until);
        Check("A locked rocker refuses: the pad nudges and the refused cue sounds", !s.weapons.sw[0] && nudged && board.LastSound == "refused" && board.Sounds == snd + 1,
              "thrown " + s.weapons.sw[0] + ", nudge " + (nudged ? "seen" : "not seen") + ", last cue \"" + board.LastSound + "\"");
        yield return (Func<bool>)(() => !rockers.Moving("fc.flip", "0"));
        Check("After the nudge the rocker is back at rest, still locked", rung.sprite.name.EndsWith("_locked") && !rung.transform.Find("Pad cover").GetComponent<Image>().enabled, rung.sprite.name);
        Shot("13_refused");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action

        var track = (RectTransform)Hit("fc.track", null).transform;
        var trackLamp = (RectTransform)track.parent.Find("Lamp"); // the TRACK button's group: Button, Plate, Label, Lamp
        float keyY = track.anchoredPosition.y, lampY = trackLamp.anchoredPosition.y;
        if (!Click("fc.track", null, "TRACK (held)", up: false)) yield break;
        Check("TRACK and its lamp drop 1 px while held", Mathf.Approximately(track.anchoredPosition.y, keyY - 1f) && Mathf.Approximately(trackLamp.anchoredPosition.y, lampY - 1f),
              "key " + (track.anchoredPosition.y - keyY).ToString("0.##") + " px, lamp " + (trackLamp.anchoredPosition.y - lampY).ToString("0.##") + " px");
        Shot("14_track_held");
        yield return 0.3f;
        if (!Click("fc.track", null, "TRACK (released)", down: false)) yield break;
        Check("TRACK comes back up on release, with the switch click", Mathf.Approximately(track.anchoredPosition.y, keyY) && Mathf.Approximately(trackLamp.anchoredPosition.y, lampY) && board.LastSound == "switch",
              "last cue \"" + board.LastSound + "\"");
        yield return 0.3f;

        if (!Click("drawer", "hangar", "HGR key")) yield break;
        yield return 0.8f;
        var recall = Hit("hangar.recall", null).GetComponent<Image>();
        var overlay = recall.transform.Find("Recall pressed (easing)").GetComponent<Image>();
        bool eased = false;
        if (!Click("hangar.recall", null, "RECALL (held)", up: false)) yield break;
        yield return (Func<bool>)(() => (eased |= overlay.enabled && overlay.color.a > 0f && overlay.color.a < 1f) && recall.sprite.name == "button_recall_pressed" && !overlay.enabled);
        Check("RECALL eases down while held (.1 s)", eased && recall.sprite.name == "button_recall_pressed" && !overlay.enabled, recall.sprite.name + (eased ? ", ease seen" : ", no ease seen"));
        Shot("15_recall_held");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("hangar.recall", null, "RECALL (released)", down: false)) yield break;
        yield return (Func<bool>)(() => recall.sprite.name == "button_recall_up" && !overlay.enabled);
        Check("RECALL comes back up on release", recall.sprite.name == "button_recall_up" && !overlay.enabled, recall.sprite.name);
        yield return 0.5f;

        // 15. The helm (Step 41): power the engines, throttle up, and the ship moves across a fixed grid.
        if (!Click("drawer", "power", "PWR key")) yield break;
        yield return 0.8f;
        if (!Click("power.shed", null, "LOAD SHED (release)")) yield break;
        if (!Click("power.bus", "0", "ENG selector")) yield break;
        if (!Click("power.bus", "2", "SEN selector")) yield break; // Step 82: DRADIS hears only with SEN power
        Check("ENG and SEN back on DRAW after the shed", !s.power.shed && s.power.buses[0].mode == PowerSystem.BusMode.Draw && s.power.buses[2].mode == PowerSystem.BusMode.Draw, Buses(s));
        yield return (Func<bool>)(() => s.nav.ThrottleCap >= 60);
        if (!Click("rate", "10", "x10 rate key")) yield break;
        if (!Click("helm", null, "HELM header")) yield break;
        yield return 0.8f;
        if (!Click("helm.thr", null, "throttle (press mid-well)")) yield break;
        Check("The throttle takes a setting", s.nav.throttle > 0, "throttle " + s.nav.throttle + "%, target " + (s.nav.throttle / 100.0 * s.nav.stats.topSpeedUnits).ToString("0.00") + " km/s");
        double x0 = s.nav.x, y0 = s.nav.y;
        Vector2 img0 = shipImg.anchoredPosition, grid0 = board.GetComponentInChildren<PlotGrid>().OffsetPx;
        yield return 4f;
        double moved = System.Math.Sqrt((s.nav.x - x0) * (s.nav.x - x0) + (s.nav.y - y0) * (s.nav.y - y0));
        Check("The ship gathers speed", s.nav.SpeedUnits > 0.01, s.nav.SpeedUnits.ToString("0.000") + " km/s after 4 s at x10");
        Check("It moves along its heading", moved > 0 && System.Math.Abs(((s.nav.CourseDeg - s.nav.heading + 540) % 360) - 180) < 2,
              moved.ToString("0.0") + " km, course " + s.nav.CourseDeg.ToString("000") + "°, heading " + s.nav.heading.ToString("000") + "°");
        Check("The plot follows our ship (owner 2026-10-29): its image holds still, the grid moves under it",
              (shipImg.anchoredPosition - img0).magnitude < 0.5f && board.GetComponentInChildren<PlotGrid>().OffsetPx != grid0, "");
        Shot("16_under_way");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action

        //     A hard turn: the hull swings, the ship slides on its old course, then lines up.
        s.nav.orderedHeading = (s.nav.heading + 90) % 360; // stand-in: set the ordered heading (the helm dial is a drag)
        yield return (Func<bool>)(() => s.nav.heading == s.nav.orderedHeading);
        double lag = System.Math.Abs(((s.nav.CourseDeg - s.nav.heading + 540) % 360) - 180);
        Check("After a turn the ship slides: course lags the heading", lag > 5, "heading " + s.nav.heading.ToString("000") + "°, course " + s.nav.CourseDeg.ToString("000") + "°");
        Shot("17_sliding");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action

        //     Pause freezes the ship.
        if (!Click("pause", null, "PAUSE key")) yield break;
        double px = s.nav.x, py = s.nav.y;
        yield return 2f;
        Check("Paused, the ship stops where it is", s.nav.x == px && s.nav.y == py, "");
        if (!Click("pause", null, "RUN key")) yield break;

        //     Pan the plot: drag the empty grid; marks, contacts, the ship and the grid all move together.
        if (!Click("helm", null, "HELM header (close)")) yield break;
        yield return 0.6f;
        if (!Click("tool", "CURSOR", "CURSOR tool")) yield break;
        var plotPanel = board.GetComponentInChildren<PlotPanel>(true);
        Vector2 v0 = plotPanel.View, gridA = board.GetComponentInChildren<PlotGrid>().OffsetPx;
        if (!PlotDrag(board, new Vector2(700, 300), new Vector2(800, 300), "drag the grid 100 px right", "plot.pan")) yield break;
        float dragUnits = (float)(100 / (PlotScale / 1000.0 * board.ZoomExact));
        Check("Dragging the grid pans the plot", Mathf.Abs(plotPanel.View.x - (v0.x - dragUnits)) < 1f && Mathf.Abs(board.GetComponentInChildren<PlotGrid>().OffsetPx.x - gridA.x - 100f) < 0.5f,
              "view moved " + (plotPanel.View.x - v0.x).ToString("0") + " km (expected " + (-dragUnits).ToString("0") + ")");
        Shot("18_panned");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("rate", "1", "x1 rate key")) yield break;

        // 16. The sensor circle (Step 42): while DRADIS is on, a circle round our ship at the hearing range.
        Check("DRADIS on: the sensor circle shows at the hearing range", s.sensors.dradis && Mathf.Abs(plotPanel.SensorRingUnits - s.sensors.PassiveRangeUnits) < 1f,
              plotPanel.SensorRingUnits.ToString("N0") + " km (hearing range " + s.sensors.PassiveRangeUnits.ToString("N0") + " km)");
        //     The radar sweep (Step 43) turns inside the circle, and holds still while paused.
        float sw0 = plotPanel.SweepDeg;
        yield return 0.5f;
        float sw1 = plotPanel.SweepDeg;
        Check("The radar sweep turns while DRADIS is on", plotPanel.SweepShown && Mathf.Abs(Mathf.DeltaAngle(sw0, sw1)) > 5f,
              "turned " + Mathf.DeltaAngle(sw0, sw1).ToString("0") + "° in 0.5 s");
        if (!Click("pause", null, "PAUSE key")) yield break;
        float sw2 = plotPanel.SweepDeg;
        yield return 0.5f;
        Check("Paused, the sweep holds still", plotPanel.SweepDeg == sw2, "");
        if (!Click("pause", null, "RUN key")) yield break;
        if (!Click("drawer", "sensors", "SENS key")) yield break;
        board.SetZoom(0.3 * X1); // 19,000 units is wider than the plot at the old x1; zoom out to see the whole circle
        yield return 0.8f;
        Shot("19_sensor_circle");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("sensors.dradis", null, "DRADIS rocker (off)")) yield break;
        yield return 0.3f;
        Check("DRADIS off: the circle and the sweep go", !s.sensors.dradis && plotPanel.SensorRingUnits == 0f && !plotPanel.SweepShown, "dradis " + s.sensors.dradis);
        Shot("20_dradis_off");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("sensors.dradis", null, "DRADIS rocker (on)")) yield break;
        yield return 0.8f;
        Check("DRADIS on again: the circle is back", s.sensors.dradis && Mathf.Abs(plotPanel.SensorRingUnits - s.sensors.PassiveRangeUnits) < 1f, plotPanel.SensorRingUnits.ToString("N0") + " km");
        Shot("21_dradis_on_again");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        board.SetZoom(X1);
        yield return 0.5f;

        // 17. Hearing (Step 44): power SEN, raise the gain, and TRACK 07 comes into hearing range.
        var alarmWords = GameObject.Find("Board").transform.Find("Tactical Board/AlarmStrip/Words").GetComponent<TMPro.TMP_Text>();
        Check("Nothing heard yet: the alarm strip is quiet", s.sensors.newContacts == 0 && !alarmWords.gameObject.activeInHierarchy, s.sensors.newContacts + " new");
        if (!Click("drawer", "power", "PWR key")) yield break;
        yield return 0.8f;
        yield return (Func<bool>)(() => s.sensors.GainCap >= 2); // SEN has been on DRAW since the helm step (Step 82)
        s.sensors.SetGain(2); // stand-in: the gain fader is a drag
        var t07 = s.TrackOf("t07");
        yield return (Func<bool>)(() => s.sensors.Heard(t07) && s.sensors.newContacts > 0);
        Check("At gain 2 DRADIS hears GARUDA", s.sensors.Heard(t07), "range " + t07.rangeUnits.ToString("N0") + " km, hearing range " + s.sensors.PassiveRangeUnits.ToString("N0") + " km");
        //     Step 75: the line comes with the sweep's first touch, and holds that bearing until the next pass.
        yield return (Func<bool>)(() => t07.seen);
        yield return 0.1f;
        var pl = GameObject.Find("Passive label 0");
        string plText = pl ? pl.GetComponent<TMPro.TMP_Text>().text : "none";
        Check("When the sweep touches it, a bearing line and label point at it", pl && plText == "BRG " + (ShipState.JsRound(t07.seenBrg) % 360).ToString("000") && Mathf.Abs(Mathf.DeltaAngle((float)t07.seenBrg, t07.bearing)) <= 1f,
              "\"" + plText + "\", bearing now " + t07.bearing.ToString("000"));
        Check("The alarm strip raises: 1 NEW CONTACT, with the alert tone", s.sensors.newContacts == 1 && alarmWords.gameObject.activeInHierarchy && alarmWords.text == "NEW CONTACT SINCE COMMIT",
              "\"" + alarmWords.text + "\", last cue \"" + board.LastSound + "\"");
        Shot("22_heard");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("ack", null, "alarm strip (acknowledge)")) yield break;
        Check("Clicking the strip acknowledges it", s.sensors.newContacts == 0 && !alarmWords.gameObject.activeInHierarchy, "");

        //     Loud enough to be tracked: our ship stays solid (owner 2026-10-25: the lamp and meter warn, not the ship).
        yield return (Func<bool>)(() => s.sensors.signal >= s.sensors.stats.signalTrackedAt);
        var shipRaw = GameObject.Find("Own ship (3D view)").GetComponent<UnityEngine.UI.RawImage>();
        bool dim = false;
        float blinkUntil = Time.realtimeSinceStartup + 1.3f;
        yield return (Func<bool>)(() => { if (shipRaw.color.a < 0.9f) dim = true; return Time.realtimeSinceStartup > blinkUntil; });
        Check("Tracked, our ship does not blink", !dim, "signal " + s.sensors.signal.ToString("0") + "%");
        //     Step 68: the signal box in the frame's top-right shows it too, with the Sensors drawer shut.
        var sigBox = GameObject.Find("Board/Tactical Board/SignalBox").transform;
        var boxPct = sigBox.Find("Signal · percent").GetComponent<TMPro.TMP_Text>();
        var boxNote = sigBox.Find("Signal · note").GetComponent<TMPro.TMP_Text>();
        yield return (Func<bool>)(() => boxPct.text == ShipState.JsRound(s.sensors.signal) + "%");
        Check("Tracked, the top-right signal box reads the signal and the trackers", boxPct.text == ShipState.JsRound(s.sensors.signal) + "%" && boxNote.text.EndsWith("YOUR RANGE"),
              "\"" + boxPct.text + "\", \"" + boxNote.text + "\"");

        // 18. The bearing trail (Step 45): each sweep leaves TRACK 07's bearing where we were. As we
        //     move, the lines cross at it. They fade out over two minutes of mission time.
        if (!Click("rate", "10", "x10 rate key")) yield break;
        s.sensors.BearingFixes.Clear();
        yield return (Func<bool>)(() => s.sensors.BearingFixes.FindAll(f => f.key == "t07").Count >= 2);
        var fixes = s.sensors.BearingFixes.FindAll(f => f.key == "t07");
        var fa = fixes[0]; var fb = fixes[fixes.Count - 1];
        double baseUnits = System.Math.Sqrt((fb.x - fa.x) * (fb.x - fa.x) + (fb.y - fa.y) * (fb.y - fa.y));
        double crossX, crossY;
        bool crosses = Cross(fa, fb, out crossX, out crossY);
        double missUnits = crosses ? System.Math.Sqrt((crossX - t07.worldX) * (crossX - t07.worldX) + (crossY - t07.worldY) * (crossY - t07.worldY)) : -1;
        Check("Two bearing lines from different spots cross at GARUDA", crosses && missUnits < 0.01 * t07.rangeUnits,
              fixes.Count + " lines, " + baseUnits.ToString("N0") + " km apart; they cross " + missUnits.ToString("N1") + " km from the target");
        Shot("23_bearing_trail");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        float firstLaid = fa.t;
        yield return (Func<bool>)(() => board.MissionSeconds - firstLaid > s.sensors.stats.bearingTrailSeconds + 2f);
        yield return 0.5f;
        Check("A bearing line is gone after two minutes", !s.sensors.BearingFixes.Contains(fa), (board.MissionSeconds - firstLaid).ToString("0") + " s after it was laid");
        if (!Click("rate", "1", "x1 rate key")) yield break;

        // 19. Intercept helpers (Step 46). Our ship is under way.
        if (!Click("drawer", "power", "PWR key (close the drawer)")) yield break;
        yield return 0.6f;
        s.marks.Clear();
        if (!Click("tool", "RULER", "RULER tool")) yield break;
        if (!PlotDrag(board, new Vector2(500, 300), new Vector2(620, 300), "draw a RULER")) yield break;
        yield return 0.3f;
        string timed = MarkLabel(board, 0);
        Check("Under way, a RULER still shows only the distance (owner 2026-10-27: no travel time)", timed.EndsWith(" km") && !timed.Contains("MIN"), "\"" + timed + "\" at " + s.nav.SpeedUnits.ToString("0.00") + " km/s");

        //     Two timed POINTs, and a RULER between them, make a track: course, speed, projection.
        if (!Click("tool", "POINT", "POINT tool")) yield break;
        if (!PlotDrag(board, new Vector2(1300, 300), new Vector2(1300, 300), "POINT A")) yield break;
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return 1.5f;
        if (!Click("rate", "1", "x1 rate key")) yield break;
        if (!PlotDrag(board, new Vector2(1340, 280), new Vector2(1340, 280), "POINT B")) yield break;
        var pa = s.marks[s.marks.Count - 2]; var pb = s.marks[s.marks.Count - 1];
        if (!Click("tool", "RULER", "RULER tool")) yield break;
        if (!PlotDrag(board, new Vector2(1305, 304), new Vector2(1336, 283), "RULER from near A to near B")) yield break;
        yield return 0.3f;
        var leg = s.marks[s.marks.Count - 1];
        double expSpd = System.Math.Sqrt((pb.x - pa.x) * (pb.x - pa.x) + (pb.y - pa.y) * (pb.y - pa.y)) / (pb.t - pa.t);
        string legLabel = MarkLabel(board, s.marks.Count - 1);
        Check("A RULER between two POINTs snaps to them", leg.ptA == pa.id && leg.ptB == pb.id && leg.x == pa.x && leg.y2 == pb.y, "ends on POINTs " + leg.ptA + " and " + leg.ptB);
        Check("It reads the target's course and speed", legLabel == "CRS 063° · " + expSpd.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km/s",
              "\"" + legLabel + "\" (" + (pb.t - pa.t).ToString("0") + " s between the POINTs)");
        Shot("24_track_projection");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action

        //     The selected weapon's reach shows while Fire Control is open.
        Check("No weapon ring with Fire Control shut", plotPanel.WeaponRingUnits == 0f, "");
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.6f;
        Check("Fire Control open: the railgun's reach shows round our ship, as far as the mount reaches (owner 2026-10-26: 25,000 km)", plotPanel.WeaponRingUnits == s.weapons.MountOf(s.weapons.mount).rangeUnits && plotPanel.WeaponRingUnits == 25000f, plotPanel.WeaponRingUnits.ToString("N0") + " km");
        //     Owner 2026-10-29: the plot feeds the gun. GARUDA isn't scanned, so TRACK takes the newest POINT (B), with
        //     the POINT-to-POINT RULER's course and speed, reckoned forward to now.
        if (!Click("fc.track", null, "TRACK (not scanned: from the plot)")) yield break;
        double legRad = System.Math.Atan2(pb.x - pa.x, pb.y - pa.y), since = s.missionNow - pb.t;
        double aimX = pb.x + System.Math.Sin(legRad) * expSpd * since - s.nav.x, aimY = pb.y + System.Math.Cos(legRad) * expSpd * since - s.nav.y;
        double aimUnits = System.Math.Sqrt(aimX * aimX + aimY * aimY);
        var plotSol = s.weapons.Sol;
        Check("Not scanned, TRACK takes the plot: the newest POINT, led along the track", plotSol.set && plotSol.fromPlot && plotSol.hasCourse && System.Math.Abs(plotSol.rangeUnits - aimUnits) < 1 && System.Math.Abs(plotSol.spd - expSpd) < 0.006,
              "RNG " + plotSol.rangeUnits.ToString("N0") + " km (expected " + aimUnits.ToString("N0") + "), SPD " + plotSol.spd.ToString("0.00") + ", from plot " + plotSol.fromPlot);
        s.weapons.sol[s.weapons.SysIndex] = new WeaponSystem.Solution(); // stand-in: clear it for the hand-dialled steps below (the board has no clear)
        board.SetZoom(0.25 * X1);
        yield return 0.4f;
        Shot("25_weapon_ring");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        board.SetZoom(X1);
        if (!Click("drawer", "fire control", "FIRE key (close)")) yield break;
        yield return 0.4f;
        Check("Fire Control shut: the ring goes", plotPanel.WeaponRingUnits == 0f, "");
        if (!Click("tool", "ERASE", "ERASE tool")) yield break;
        if (!Click("tool", "ERASE", "ERASE tool (again)")) yield break;

        // 20. A firing solution dialled by hand (Step 47). TRACK 07 is heard, not identified.
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.6f;
        Check("No firing solution yet", !s.weapons.Sol.set, "");
        if (!Click("fc.track", null, "TRACK (contact not identified)")) yield break;
        Check("TRACK refuses a contact that is only heard", !s.weapons.Sol.set, "");
        int expBrg = (ShipState.JsRound(s.scopeBrg) - s.nav.heading + 540) % 360 - 180;
        var rngKnob = Hit("fc.knob", "rng");
        Vector2 knobAt = board.BoardPoint(RectTransformUtility.WorldToScreenPoint(rngKnob.GetComponentInParent<Canvas>().rootCanvas.worldCamera,
                                         rngKnob.transform.TransformPoint(((RectTransform)rngKnob.transform).rect.center)));
        if (!PlotDrag(board, knobAt, knobAt + new Vector2(80, 0), "turn the RNG knob (10 steps)", "fc.knob")) yield break;
        yield return 0.3f;
        //     Step 112: the dialled point stays put in space, so as our ship moves on, its RNG and REL BRG drift a little
        Check("The RNG knob starts a hand-dialled solution on the periscope's bearing",
              s.weapons.Sol.set && s.weapons.Sol.manual && System.Math.Abs(s.weapons.Sol.brg - expBrg) <= 1 && s.weapons.Sol.spd == 0f && Mathf.Abs(s.weapons.Sol.rangeUnits - (s.weapons.stats.manualStartRangeUnits + 1000f)) < 100f,
              "RNG " + s.weapons.Sol.rangeUnits.ToString("N0") + " km, REL BRG " + s.weapons.Sol.brg + "°, SPD " + s.weapons.Sol.spd.ToString("0.00"));
        var fc = board.GetComponentInChildren<FireControlPanel>(true);
        var tidField = typeof(FireControlPanel).GetField("trackId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var tidText = tidField != null ? tidField.GetValue(fc) as TMPro.TMP_Text : null;
        Check("The readout says MANUAL", tidText && tidText.text == "MANUAL", tidText ? "\"" + tidText.text + "\"" : "no readout");
        //     BEARING follows the dialled bearing (a point in space), not a locked contact.
        double dialled = ((s.nav.heading + s.weapons.Sol.brg) % 360 + 360) % 360;
        s.scopeBrg = (dialled + 10) % 360;
        bool offOk = !s.weapons.OnBearing;
        s.SlewTo();
        Check("BEARING wants the periscope on the dialled bearing; SLEW TO puts it there", offOk && s.weapons.OnBearing && System.Math.Abs(s.scopeBrg - dialled) < 0.01,
              "dialled " + dialled.ToString("000") + "°, periscope " + s.scopeBrg.ToString("000") + "°");
        Shot("26_manual_solution");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("drawer", "fire control", "FIRE key (close)")) yield break;
        yield return 0.5f;

        // 21. Fire and hit (Step 48): aim at TRACK 07 by hand, arm, fire a slug, watch it land.
        var dummyShip = GameObject.Find("World").GetComponentInChildren<WorldContact>().Ship;
        bool whole = true;
        foreach (var c in dummyShip.damage.compartments) if (c.integ != 5) whole = false;
        foreach (var a in dummyShip.damage.SideArmour) if (a <= 0f) whole = false;
        Check("GARUDA starts undamaged", whole, "");
        //     The periscope onto the contact, by its bearing label (it is still heard).
        s.LookAt("t07"); // stand-in for SLEW, a drag (owner 2026-10-29: clicking the plot no longer aims the periscope)
        if (board.Motion && !board.Motion.ScopeDown) board.Motion.SetScopeDown(true);
        yield return 0.6f;
        //     Weapons released: CONDITION 2 from the orders station.
        if (!Click("orders", null, "ORDERS header")) yield break;
        yield return 0.6f;
        if (!Click("orders.alert", null, "CONDITION dial")) yield break;
        Check("CONDITION 2: weapons released", s.orders.WeaponsReleased, "alert " + s.orders.alert);
        if (!Click("orders", null, "ORDERS header (close)")) yield break;
        yield return 0.6f;
        //     Power for the guns: SEN gives back all but one unit, WEP draws and charges.
        //     Step 82: with no unit on SEN, DRADIS goes deaf, so SEN keeps one.
        if (!Click("drawer", "power", "PWR key")) yield break;
        yield return 0.8f;
        if (!Click("power.bus", "2", "SEN selector (STANDBY)")) yield break;
        s.power.buses[2].fill = 1; // stand-in: SEN on VENT until one unit is left, then back to STANDBY
        if (!Click("power.bus", "1", "WEP selector (DRAW)")) yield break;
        yield return (Func<bool>)(() => s.weapons.WepReady && s.weapons.wepCharge >= s.weapons.ShotCost);
        Check("WEP charged for a shot", s.weapons.WepReady && s.weapons.wepCharge >= s.weapons.ShotCost, s.weapons.wepCharge.ToString("0") + "% of " + s.weapons.ShotCost + "%");
        //     A fresh hand-dialled solution on the periscope's bearing (one RNG step starts it).
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.6f;
        s.weapons.sol[s.weapons.SysIndex] = new WeaponSystem.Solution(); // stand-in: clear the old solution (the board has no clear)
        if (!PlotDrag(board, knobAt, knobAt + new Vector2(8, 0), "turn the RNG knob (1 step)", "fc.knob")) yield break;
        yield return 0.3f;
        //     Whole-degree aim: dial the degree nearest the target (half a degree is ~213 units out here).
        var brgKnob = Hit("fc.knob", "brg");
        Vector2 brgAt = board.BoardPoint(RectTransformUtility.WorldToScreenPoint(brgKnob.GetComponentInParent<Canvas>().rootCanvas.worldCamera,
                                         brgKnob.transform.TransformPoint(((RectTransform)brgKnob.transform).rect.center)));
        int best = 0;
        for (int d = -1; d <= 1; d++) if (MissUnits(s, t07, s.nav.heading + s.weapons.Sol.brg + d) < MissUnits(s, t07, s.nav.heading + s.weapons.Sol.brg + best)) best = d;
        if (best != 0 && !PlotDrag(board, brgAt, brgAt + new Vector2(6 * best, 0), "turn the BRG knob to the nearest degree", "fc.knob")) yield break;
        s.SlewTo(); // stand-in for the periscope's SLEW TO key: onto the dialled bearing
        double predicted = MissUnits(s, t07, s.nav.heading + s.weapons.Sol.brg);
        bool expectHit = predicted <= s.weapons.stats.hitRadiusUnits;
        Check("Turret A can't bear on it: ANGLE is out of arc", s.weapons.mount == "RGA" && !s.weapons.InArc, "REL BRG " + s.weapons.Sol.brg + "°, heading " + s.nav.heading + "°");
        if (!Click("fc.mount", null, "turret selector (RG-B)")) yield break;
        Check("Turret B bears", s.weapons.mount == "RGB" && s.weapons.InArc, "");
        for (int i = 0; i < 4; i++) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1))) yield break; yield return 0.2f; }
        Check("The ladder is set: LOAD, ANGLE, BEARING, ARM", s.weapons.AllSet, string.Join(",", System.Array.ConvertAll(s.weapons.sw, b => b ? "1" : "0")));
        //     Fire at x1 (Step 81: at x10 the slug lands within one tick).
        if (!Click("rate", "1", "x1 rate key")) yield break;
        var lever = Hit("fc.lever", null);
        Vector2 leverAt = board.BoardPoint(RectTransformUtility.WorldToScreenPoint(lever.GetComponentInParent<Canvas>().rootCanvas.worldCamera,
                                           lever.transform.TransformPoint(((RectTransform)lever.transform).rect.center)));
        s.sensors.signal = 50f; // stand-in: room for the spike to show (we are at 100% with this much power drawn)
        float sig0 = s.sensors.signal;
        var rgb = s.weapons.MountOf("RGB");
        int slugs0 = rgb.ammo;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "pull the fire lever", "fc.lever")) yield break;
        Check("The lever fires one slug and releases ARM", s.weapons.Rounds.Count == 1 && !s.weapons.sw[3], s.weapons.Rounds.Count + " in flight");
        Check("Firing spikes our signal", Mathf.Approximately(s.sensors.signal, sig0 + s.weapons.stats.fireSignalSpike), sig0.ToString("0") + " → " + s.sensors.signal.ToString("0"));
        yield return 0.4f;
        Check("The fire cue sounds", board.Sounds > 0 && (board.LastSound == "railgun" || board.LastSound == "hit"), "last cue \"" + board.LastSound + "\"");
        //     Owner 2026-10-27: the slugs are a real count; the drawer shows it.
        var fcPanel = board.GetComponentInChildren<FireControlPanel>(true);
        var sysStateField = typeof(FireControlPanel).GetField("sysState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Func<string> sysText = () => { var t = sysStateField.GetValue(fcPanel) as TMPro.TMP_Text; return t ? t.text : ""; };
        Check("The shot takes one slug from turret B, and the drawer shows it", slugs0 == 22 && rgb.ammo == slugs0 - 1 && sysText() == "SLUGS " + (slugs0 - 1),
              slugs0 + " → " + rgb.ammo + ", \"" + sysText() + "\"");
        //     Paused, the slug stops.
        if (!Click("pause", null, "PAUSE key")) yield break;
        double rx = s.weapons.Rounds.Count > 0 ? s.weapons.Rounds[0].travelled : -1;
        yield return 0.6f;
        Check("Paused, the slug holds still", s.weapons.Rounds.Count == 1 && s.weapons.Rounds[0].travelled == rx, (rx >= 0 ? rx.ToString("N0") + " km flown" : "none"));
        if (!Click("pause", null, "RUN key")) yield break;
        if (board.Motion && board.Motion.ScopeDown) board.Motion.SetScopeDown(false); // the periscope covers the plot's middle
        board.SetZoom(0.4 * X1);
        yield return 0.8f;
        Shot("27_slug_in_flight");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        board.SetZoom(X1);
        yield return (Func<bool>)(() => s.weapons.Rounds.Count == 0);
        yield return (Func<bool>)(() => s.weapons.Rounds.Count == 0); // 15 s each; the flight is about 2 s at x1
        bool marginal = System.Math.Abs(predicted - s.weapons.stats.hitRadiusUnits) < 0.05 * s.weapons.stats.hitRadiusUnits; // either way is right this close
        if (marginal) expectHit = s.weapons.shotReport == "HIT · BOGEY T07";
        Check("The slug lands as the aim says: " + (expectHit ? "a HIT, no detail on a ship we have not scanned" : "a MISS") + (marginal ? " (a marginal aim)" : ""),
              expectHit ? s.weapons.shotReport == "HIT · BOGEY T07" : s.weapons.shotReport == "MISS",
              "\"" + s.weapons.shotReport + "\"; the aim passes " + predicted.ToString("N0") + " km from it (hit radius " + s.weapons.stats.hitRadiusUnits + " km)");
        yield return 0.3f;
        Check("Fire control reports it, with its cue", board.LastSound == (expectHit ? "hit" : "miss"), "last cue \"" + board.LastSound + "\"");
        Shot("28_hit");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        //     A second shot 2° off: a miss.
        if (!PlotDrag(board, brgAt, brgAt + new Vector2(12, 0), "turn the BRG knob (+2°)", "fc.knob")) yield break;
        s.SlewTo(); // stand-in for the periscope's SLEW TO key: onto the new dialled bearing
        yield return (Func<bool>)(() => s.weapons.wepCharge >= s.weapons.ShotCost);
        for (int i = 0; i < 4; i++) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (again)")) yield break; yield return 0.2f; }
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "pull the fire lever (again)", "fc.lever")) yield break;
        yield return (Func<bool>)(() => s.weapons.Rounds.Count == 0);
        yield return (Func<bool>)(() => s.weapons.Rounds.Count == 0);
        Check("2° off, it misses", s.weapons.shotReport == "MISS", "\"" + s.weapons.shotReport + "\"");
        //     Out of slugs (owner 2026-10-27). Stand-in: turret B down to its last one.
        int slugsWas = rgb.ammo;
        rgb.ammo = 1;
        yield return (Func<bool>)(() => s.weapons.wepCharge >= s.weapons.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.weapons.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (last slug)")) yield break; yield return 0.2f; }
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "fire the last slug", "fc.lever")) yield break;
        yield return 0.6f;
        Check("The last slug fired: the ladder drops, and the drawer reads SLUGS 0", rgb.ammo == 0 && !System.Array.Exists(s.weapons.sw, b => b) && sysText() == "SLUGS 0",
              rgb.ammo + " left, ladder " + string.Join(",", System.Array.ConvertAll(s.weapons.sw, b => b ? "1" : "0")) + ", \"" + sysText() + "\"");
        if (!Click("fc.flip", "0", "LOAD (no slugs)")) yield break;
        Check("No slugs: LOAD refuses", !s.weapons.sw[0] && board.LastSound == "refused", "last cue \"" + board.LastSound + "\"");
        yield return (Func<bool>)(() => s.weapons.Rounds.Count == 0);
        yield return (Func<bool>)(() => s.weapons.Rounds.Count == 0);
        rgb.ammo = slugsWas;
        if (!Click("rate", "1", "x1 rate key")) yield break;

        // 21b. Leading a moving target (Step 52). TRACK 07 gets under way on its patrol again.
        brain.enabled = true;
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => t07Ship.nav.SpeedUnits > 1.0);
        yield return (Func<bool>)(() => t07Ship.nav.heading == t07Ship.nav.orderedHeading); // steady on its leg before we plot it
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Stand-in for the helm: 12,000 units off its starboard beam, stopped, on its course, so turret B bears.
        double beam = (t07Ship.nav.CourseDeg + 90) * System.Math.PI / 180;
        s.nav.x = t07Ship.nav.x + System.Math.Sin(beam) * 12000; s.nav.y = t07Ship.nav.y + System.Math.Cos(beam) * 12000;
        s.nav.vx = s.nav.vy = 0; s.nav.throttle = 0;
        s.nav.heading = s.nav.orderedHeading = ShipState.JsRound(t07Ship.nav.CourseDeg) % 360;
        yield return 1f;
        //     Its track: two POINTs on it two minutes apart (stand-in for accurate plotting; the plot itself was tested above).
        s.marks.Clear();
        var trackA = new ShipState.Mark { id = ++s.markSeq, kind = "point", x = t07Ship.nav.x, y = t07Ship.nav.y, t = board.MissionSeconds };
        s.marks.Add(trackA);
        //     The solution: the periscope on it, the RNG knob starts one, dialled to its range (stand-in for reading the plot).
        s.LookAt("t07");
        s.weapons.sol[s.weapons.SysIndex] = new WeaponSystem.Solution();
        if (!PlotDrag(board, knobAt, knobAt + new Vector2(8, 0), "turn the RNG knob (1 step)", "fc.knob")) yield break;
        s.weapons.Sol.rangeUnits = (float)System.Math.Sqrt((t07Ship.nav.x - s.nav.x) * (t07Ship.nav.x - s.nav.x) + (t07Ship.nav.y - s.nav.y) * (t07Ship.nav.y - s.nav.y));
        s.weapons.NudgeSol("rng", 0);
        Check("Without a track the solution has no course", s.weapons.Sol.set && s.weapons.Sol.manual && !s.weapons.Sol.hasCourse, "");
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => board.MissionSeconds - trackA.t >= 120f);
        if (!Click("rate", "1", "x1 rate key")) yield break;
        var trackB = new ShipState.Mark { id = ++s.markSeq, kind = "point", x = t07Ship.nav.x, y = t07Ship.nav.y, t = board.MissionSeconds };
        s.marks.Add(trackB);
        s.MarkDown("RULER", trackA.x, trackA.y, board.MissionSeconds); s.draft.ptA = trackA.id;
        s.MarkMove(trackB.x, trackB.y); s.draft.ptB = trackB.id;
        s.MarkUp(0);
        // the plot gives the course and speed between its two POINTs
        float legCrs = (float)((System.Math.Atan2(trackB.x - trackA.x, trackB.y - trackA.y) * 180 / System.Math.PI + 360) % 360);
        float legSpd = (float)(System.Math.Sqrt((trackB.x - trackA.x) * (trackB.x - trackA.x) + (trackB.y - trackA.y) * (trackB.y - trackA.y)) / (trackB.t - trackA.t));
        Check("A POINT-to-POINT track feeds the solution its course and speed",
              s.weapons.Sol.hasCourse && Mathf.Abs(Mathf.DeltaAngle(s.weapons.Sol.course, legCrs)) < 0.01f && Mathf.Abs(s.weapons.Sol.spd - legSpd) < 0.006f,
              "CRS " + s.weapons.Sol.course.ToString("000") + "°, SPD " + s.weapons.Sol.spd.ToString("0.00") + " (its true course now " + t07Ship.nav.CourseDeg.ToString("000") + "°, speed " + t07Ship.nav.SpeedUnits.ToString("0.00") + ")");
        //     Arm and fire, led. The same shot unled would miss.
        if (s.weapons.mount != "RGB" && !Click("fc.mount", null, "turret selector (RG-B)")) yield break;
        yield return (Func<bool>)(() => s.weapons.wepCharge >= s.weapons.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.weapons.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (led shot)")) yield break; yield return 0.15f; }
        double tvx = t07Ship.nav.vx, tvy = t07Ship.nav.vy;
        //     Unled: straight at where the first POINT put it, two minutes ago (Step 112: BRG now follows the point on along the track)
        double unled = PassUnits(s.nav.x, s.nav.y, (System.Math.Atan2(s.weapons.Sol.wx - s.nav.x, s.weapons.Sol.wy - s.nav.y) * 180 / System.Math.PI + 360) % 360,
                                 s.weapons.stats.slugSpeedUnits, t07Ship.nav.x, t07Ship.nav.y, tvx, tvy, 600);
        double led = PassUnits(s.nav.x, s.nav.y, s.weapons.LeadBearing(), s.weapons.stats.slugSpeedUnits, t07Ship.nav.x, t07Ship.nav.y, tvx, tvy, 600);
        if (!Click("rate", "10", "x10 rate key")) yield break;
        int landedLed = s.weapons.shotResults;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "fire (led)", "fc.lever")) yield break;
        yield return (Func<bool>)(() => s.weapons.shotResults > landedLed);
        Check("Led from the track, it hits the moving GARUDA (unscanned: a bare HIT)", s.weapons.lastShotHit && s.weapons.shotReport == "HIT · BOGEY T07",
              "\"" + s.weapons.shotReport + "\"; aimed at the first POINT it would pass " + unled.ToString("N0") + " km off, led " + led.ToString("N0") + " km (hit radius " + s.weapons.stats.hitRadiusUnits + ")");
        Check("Unled, the same shot would have missed", unled > s.weapons.stats.hitRadiusUnits, unled.ToString("N0") + " km");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     An active-scan lock: TRACK knows its true course and speed and leads exactly.
        //     A real active scan (owner 2026-10-29: it was faked here): ACTIVE on, the cone dial onto GARUDA, let go.
        if (!Click("drawer", "sensors", "SENS key")) yield break;
        yield return 0.8f;
        if (!Click("sensors.active", null, "ACTIVE SCAN rocker (on)")) yield break;
        double coneRad = t07.bearing * System.Math.PI / 180;
        var conePt = new Vector2(1505f + (float)System.Math.Sin(coneRad) * 40f, 609f - (float)System.Math.Cos(coneRad) * 40f); // the dial's centre, Sensors Panel
        if (!PlotDrag(board, conePt, conePt, "the cone dial onto GARUDA's bearing", "sensors.cone")) yield break;
        Check("ACTIVE SCAN on, the cone on its bearing, let go: GARUDA is identified", s.sensors.identified.Contains("t07") && ShipState.AngDiff(s.sensors.coneBrg, t07.bearing) <= 1,
              "cone " + s.sensors.coneBrg + "°, GARUDA at " + t07.bearing + "°");
        if (!Click("sensors.active", null, "ACTIVE SCAN rocker (off)")) yield break;
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.6f;
        if (!Click("fc.track", null, "TRACK (locked)")) yield break;
        Check("TRACK's solution carries the target's course", s.weapons.Sol.set && !s.weapons.Sol.manual && s.weapons.Sol.hasCourse, "CRS " + s.weapons.Sol.course.ToString("000") + "°, SPD " + s.weapons.Sol.spd.ToString("0.00"));
        s.LookAt("t07");
        yield return (Func<bool>)(() => s.weapons.wepCharge >= s.weapons.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.weapons.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (TRACK shot)")) yield break; yield return 0.15f; }
        if (!Click("rate", "10", "x10 rate key")) yield break;
        int landedTrack = s.weapons.shotResults;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "fire (TRACK)", "fc.lever")) yield break;
        yield return (Func<bool>)(() => s.weapons.shotResults > landedTrack);
        Check("TRACK's led shot hits", s.weapons.shotReport != null && s.weapons.shotReport.StartsWith("HIT · GARUDA"), "\"" + s.weapons.shotReport + "\"");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Hold it still again for the fight below; clear the plot.
        brain.enabled = false; t07Ship.nav.vx = t07Ship.nav.vy = 0; s.marks.Clear(); s.sensors.identified.Remove("t07");

        // 21c. Missiles (Step 53): a lock, the launcher, three warheads, homing on the moving TRACK 07.
        brain.enabled = true;
        s.sensors.identified.Add("t07"); // stand-in for an active-scan lock
        for (int k = 0; k < 3 && s.weapons.sys != "MS"; k++) { if (!Click("fc.system", null, "weapon selector")) yield break; yield return 0.2f; }
        s.weapons.salvo = 2; // stand-in for the Tuning window (its default is 2)
        Check("MISSILE selected: the launcher, HE", s.weapons.sys == "MS" && s.weapons.mount == "VLS" && s.weapons.warheads[s.weapons.warhead] == "HE", s.weapons.sys + " " + s.weapons.mount + " " + s.weapons.warheads[s.weapons.warhead]);
        if (!Click("fc.track", null, "TRACK (locked)")) yield break;
        s.LookAt("t07");
        yield return (Func<bool>)(() => s.weapons.wepCharge >= s.weapons.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.weapons.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (missiles)")) yield break; yield return 0.15f; }
        Func<ShipState, float> hull = sh => { float h = 0; foreach (var a in sh.damage.SideArmour) h += a; foreach (var c in sh.damage.compartments) h += c.integ; return h; };
        int cells0 = s.weapons.vlsCells, landed0 = s.weapons.shotResults;
        float hull0 = hull(dummyShip);
        if (!Click("rate", "10", "x10 rate key")) yield break;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "launch (HE salvo of 2)", "fc.lever")) yield break;
        Check("A salvo of two leaves the launcher", s.weapons.Rounds.FindAll(r => r.missile).Count == 2 && s.weapons.vlsCells == cells0 - 2, s.weapons.vlsCells + " cells left");
        yield return (Func<bool>)(() => s.weapons.shotResults >= landed0 + 2);
        yield return (Func<bool>)(() => s.weapons.shotResults >= landed0 + 2);
        Check("Both home in on the moving GARUDA and the HE does damage", s.weapons.lastShotHit && s.weapons.shotReport.Contains(" · HE") && hull(dummyShip) < hull0,
              "\"" + s.weapons.shotReport + "\", armour + integrity " + hull0 + " → " + hull(dummyShip));
        //     EMP: no hull damage, its power goes down and it slows.
        if (!Click("fc.mount", null, "warhead selector (EMP)")) yield break;
        Check("The turret knob picks the warhead", s.weapons.warheads[s.weapons.warhead] == "EMP", s.weapons.warheads[s.weapons.warhead]);
        s.weapons.salvo = 1;
        s.LookAt("t07");
        yield return (Func<bool>)(() => s.weapons.wepCharge >= s.weapons.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.weapons.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (EMP)")) yield break; yield return 0.15f; }
        int landed1 = s.weapons.shotResults;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "launch (EMP)", "fc.lever")) yield break;
        yield return (Func<bool>)(() => s.weapons.shotResults > landed1);
        yield return (Func<bool>)(() => s.weapons.shotResults > landed1);
        double spdHit = t07Ship.nav.SpeedUnits;
        Check("EMP: its power goes down", s.weapons.shotReport != null && s.weapons.shotReport.EndsWith(" · EMP · POWER DOWN") && dummyShip.power.buses[0].fill == 0, "\"" + s.weapons.shotReport + "\"");
        yield return 2f;
        Check("Without power it drifts: no thrust, no braking, the same speed (owner 2026-10-29)", System.Math.Abs(t07Ship.nav.SpeedUnits - spdHit) < 0.01, spdHit.ToString("0.00") + " → " + t07Ship.nav.SpeedUnits.ToString("0.00") + " km/s");
        //     PENETRATOR: straight to a compartment behind the side.
        if (!Click("fc.mount", null, "warhead selector (PENETRATOR)")) yield break;
        s.LookAt("t07");
        yield return (Func<bool>)(() => s.weapons.wepCharge >= s.weapons.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.weapons.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (PEN)")) yield break; yield return 0.15f; }
        int landed2 = s.weapons.shotResults;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "launch (PENETRATOR)", "fc.lever")) yield break;
        yield return (Func<bool>)(() => s.weapons.shotResults > landed2);
        yield return (Func<bool>)(() => s.weapons.shotResults > landed2);
        Check("PENETRATOR: straight through to a compartment", s.weapons.shotReport != null && s.weapons.shotReport.Contains(" · PEN · "), "\"" + s.weapons.shotReport + "\"");
        Check("Four cells used", s.weapons.vlsCells == cells0 - 4, s.weapons.vlsCells + " left");
        Check("The drawer shows the cells left", sysText().EndsWith(" OF " + (cells0 - 4)), "\"" + sysText() + "\"");
        Shot("28b_missiles");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        //     No cells left (owner 2026-10-27). Stand-in: the launcher emptied.
        s.weapons.vlsCells = 0;
        if (s.weapons.sw[0] && !Click("fc.flip", "0", "LOAD (drop it)")) yield break;
        yield return 0.6f;
        if (!Click("fc.flip", "0", "LOAD (no cells)")) yield break;
        Check("No cells: LOAD refuses, and the drawer shows 0", !s.weapons.sw[0] && board.LastSound == "refused" && sysText().EndsWith(" OF 0"), "last cue \"" + board.LastSound + "\", \"" + sysText() + "\"");
        s.weapons.vlsCells = cells0 - 4;
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Back to the railgun; hold TRACK 07 still for the fight.
        if (!Click("fc.system", null, "weapon selector (RAILGUN)")) yield break;
        brain.enabled = false; t07Ship.nav.vx = t07Ship.nav.vy = 0; s.sensors.identified.Remove("t07");

        s.sensors.identified.Add("t07"); // stand-in for an active scan: from here on we see what our hits do (Step 61)
        // 21d. Fighters (Step 54): the interceptors launch from the hangar, fly to TRACK 07, make attack runs
        //     on the side facing them, and RECALL brings them home to land.
        //     Stand-in for the helm: 2,000 units ahead of its bow (never hit yet), stopped, facing it.
        double sbd = t07.worldHdg * System.Math.PI / 180;
        s.nav.x = t07.worldX + System.Math.Sin(sbd) * 2000; s.nav.y = t07.worldY + System.Math.Cos(sbd) * 2000;
        s.nav.vx = s.nav.vy = 0; s.nav.throttle = 0;
        s.nav.heading = s.nav.orderedHeading = ((int)System.Math.Round(t07.worldHdg) + 180) % 360;
        s.weapons.target = "t07";
        //     Step 69 (owner 2026-10-25): with ACTIVE on, every pass of the sweep over TRACK 07 beeps, scanned or not.
        //     Stand-in for the ACTIVE key and the cone dial.
        yield return 0.5f; // a tick, so TRACK 07's bearing and range are measured from here
        s.sensors.active = true; s.sensors.coneBrg = t07.bearing;
        int tags0 = board.Tags;
        yield return (Func<bool>)(() => board.Tags > tags0);
        yield return (Func<bool>)(() => board.Tags > tags0 + 1);
        Check("ACTIVE on: each sweep pass over GARUDA beeps", board.Tags >= tags0 + 2, (board.Tags - tags0) + " beeps");
        s.sensors.active = false;
        var vxField = typeof(PlotPanel).GetField("viewX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var vyField = typeof(PlotPanel).GetField("viewY", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        vxField.SetValue(plotPanel, (s.nav.x + t07.worldX) / 2); vyField.SetValue(plotPanel, (s.nav.y + t07.worldY) / 2); // stand-in for panning the plot
        var ilField = typeof(HangarPanel).GetField("interlock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hangarPanel = board.GetComponentInChildren<HangarPanel>(true);
        Func<string> interlockText = () => { var t = ilField.GetValue(hangarPanel) as TMPro.TMP_Text; return t ? t.text : ""; };
        //     The launch bearing box: typing in it sets the bearing (its links to the panel broke once, unseen)
        var lbBox = hangarPanel.GetComponentInChildren<TMPro.TMP_InputField>(true);
        int lb0 = s.hangar.lbrg;
        lbBox.text = lb0 == 123 ? "124" : "123"; // as a player types it
        Check("Typing a launch bearing sets it", s.hangar.lbrg == (lb0 == 123 ? 124 : 123), "box \"" + lbBox.text + "\", bearing " + s.hangar.lbrg.ToString("000"));
        lbBox.text = lb0.ToString("000");
        if (board.Motion && board.Motion.ScopeDown) board.Motion.SetScopeDown(false); // the periscope covers the plot's middle
        //     Power for the catapult: ENG and WEP hold all ten units, so ENG gives its back (we are stopped) and AUX draws.
        if (!Click("drawer", "power", "PWR key")) yield break;
        yield return 0.8f;
        for (int k = 0; k < 3 && s.power.buses[0].mode != PowerSystem.BusMode.Vent; k++) if (!Click("power.bus", "0", "ENG selector (toward VENT)")) yield break;
        for (int k = 0; k < 3 && s.power.buses[3].mode != PowerSystem.BusMode.Draw; k++) if (!Click("power.bus", "3", "AUX selector (toward DRAW)")) yield break;
        yield return (Func<bool>)(() => s.power.BusUnits("aux") >= 1);
        if (!Click("drawer", "hangar", "HGR key")) yield break;
        yield return 0.8f;
        Check("The interceptors are selected, on DESTROY", s.hangar.CurrentSquad.key == "gw" && s.hangar.order == "destroy" && s.hangar.SquadReady(s.hangar.CurrentSquad) == 4, s.hangar.CurrentSquad.key + " " + s.hangar.order + ", " + s.hangar.SquadReady(s.hangar.CurrentSquad) + " ready");
        s.hangar.SetLbrg(s.nav.heading.ToString()); // stand-in for typing the launch bearing: dead ahead
        if (!s.hangar.doorsOpen && !Click("hangar.doors", null, "bay door master")) yield break;
        Check("The catapult can charge (AUX powered)", s.hangar.StageBlock("cat") == null, (s.hangar.StageBlock("cat") ?? "") + " AUX " + s.power.BusUnits("aux") + " units, ENG " + s.power.buses[0].mode);
        for (int i = 0; i < s.hangar.stages.Length; i++)
        {
            if (!Click("hangar.stage", i.ToString(), s.hangar.stages[i].label + " stage")) yield break;
            int k = i;
            yield return (Func<bool>)(() => s.hangar.stages[k].progress >= 100f);
        }
        Check("Every launch stage at 100%", s.hangar.AllStagesDone && s.hangar.LaunchBlock() == null, s.hangar.LaunchBlock() ?? "");
        var hLever = Hit("hangar.lever", null);
        Vector2 hLeverAt = board.BoardPoint(RectTransformUtility.WorldToScreenPoint(hLever.GetComponentInParent<Canvas>().rootCanvas.worldCamera,
                                            hLever.transform.TransformPoint(((RectTransform)hLever.transform).rect.center)));
        if (!PlotDrag(board, hLeverAt, hLeverAt + new Vector2(240, 0), "pull the launch lever", "hangar.lever")) yield break;
        yield return 0.6f;
        Check("Four interceptors take off: four wedges on the plot", s.hangar.Flights.Count == 1 && s.hangar.Flights[0].craft == 4 && plotPanel.FightersShown == 4 && s.hangar.Launched(s.hangar.CurrentSquad),
              s.hangar.Flights.Count + " flight, " + (s.hangar.Flights.Count > 0 ? s.hangar.Flights[0].craft : 0) + " craft, " + plotPanel.FightersShown + " drawn");
        var flight = s.hangar.Flights.Count > 0 ? s.hangar.Flights[0] : null;
        if (flight == null) yield break;
        double out0 = System.Math.Sqrt((flight.x - s.nav.x) * (flight.x - s.nav.x) + (flight.y - s.nav.y) * (flight.y - s.nav.y));
        Check("They take off outside the small circle round our ship, not on its hull", s.hangar.launchClearUnits > 0 && out0 >= s.hangar.launchClearUnits * 0.95,
              out0.ToString("N0") + " km out; the V clears the circle at " + s.hangar.launchClearUnits.ToString("N0") + " km");
        double fx0 = flight.x, fy0 = flight.y; float ft0 = board.MissionSeconds;
        yield return 1f; // at x1: a few game seconds, well before they arrive
        double flown = System.Math.Sqrt((flight.x - fx0) * (flight.x - fx0) + (flight.y - fy0) * (flight.y - fy0)), took = board.MissionSeconds - ft0;
        if (!Click("rate", "10", "x10 rate key")) yield break;
        Check("They fly toward GARUDA at " + s.hangar.stats.fighterSpeedUnits + " km/s", took > 0 && System.Math.Abs(flown / took - s.hangar.stats.fighterSpeedUnits) < 0.5 && interlockText().StartsWith("OUTBOUND TO GARUDA"),
              (flown / took).ToString("0.00") + " km/s; \"" + interlockText() + "\"");
        yield return (Func<bool>)(() => System.Math.Sqrt((flight.x - s.nav.x) * (flight.x - s.nav.x) + (flight.y - s.nav.y) * (flight.y - s.nav.y)) > 1000); // clear of our ship's picture
        Shot("28c_fighters_outbound");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        float prow0 = dummyShip.damage.SideArmour[0];
        yield return (Func<bool>)(() => s.hangar.flightRuns >= 1);
        float prowAfter = Mathf.Max(0f, prow0 - 4 * s.hangar.stats.fighterArmourDamage);
        Check("In range, one attack run: 10 armour per fighter off the side facing them, its bow", s.hangar.flightRuns == 1 && prow0 > 4 * s.hangar.stats.fighterArmourDamage
              && s.hangar.flightReport == " PROW ARMOUR " + ShipState.JsRound(prowAfter / dummyShip.damage.stats.sideArmourMax[0] * 100f) + "%"
              && Mathf.Approximately(dummyShip.damage.SideArmour[0], prowAfter),
              "\"" + s.hangar.flightReport + "\"; PROW armour " + prow0 + " → " + dummyShip.damage.SideArmour[0]);
        Check("The hangar reports the attack", interlockText().StartsWith("ATTACKING GARUDA"), "\"" + interlockText() + "\"");
        Shot("28d_fighters_attacking");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("hangar.recall", null, "RECALL")) yield break;
        Check("RECALL turns them home", flight.returning && interlockText().StartsWith("RETURNING"), "\"" + interlockText() + "\"");
        int runs = s.hangar.flightRuns;
        yield return (Func<bool>)(() => s.hangar.Flights.Count == 0);
        yield return (Func<bool>)(() => s.hangar.Flights.Count == 0); // 15 s each; the way home is about 11 s at x10
        Check("Home, they land: the squadron is aboard and its stages clear", s.hangar.Flights.Count == 0 && !s.hangar.Launched(s.hangar.CurrentSquad) && !s.hangar.AllStagesDone && plotPanel.FightersShown == 0 && s.hangar.flightRuns == runs,
              s.hangar.Flights.Count + " flights, " + plotPanel.FightersShown + " drawn, " + (s.hangar.flightRuns - runs) + " runs after the recall");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.6f;

        // 21e. Point defence (Step 55): hit, TRACK 07 fires missiles back; PDC-A on AUTO shoots them down;
        //     with its ladder dropped, the next salvo gets through and hits us.
        //     Stand-in for the helm: 6,000 units off its bow, stopped, turned so it bears 042° from our bow (PDC-A's arc).
        double pdb = t07.worldHdg * System.Math.PI / 180;
        s.nav.x = t07.worldX + System.Math.Sin(pdb) * 6000; s.nav.y = t07.worldY + System.Math.Cos(pdb) * 6000;
        s.nav.vx = s.nav.vy = 0; s.nav.throttle = 0;
        s.nav.heading = s.nav.orderedHeading = (((int)System.Math.Round(t07.worldHdg) + 180 - 42) % 360 + 360) % 360;
        vxField.SetValue(plotPanel, (s.nav.x + t07.worldX) / 2); vyField.SetValue(plotPanel, (s.nav.y + t07.worldY) / 2); // stand-in for panning the plot
        brain.enabled = true; // it thinks again (it holds its fire for a tick, so old damage doesn't count)
        yield return 1f;
        for (int k = 0; k < 3 && s.weapons.sys != "PD"; k++) { if (!Click("fc.system", null, "weapon selector")) yield break; yield return 0.2f; }
        Check("PDC selected: group A, autoloaders engaged", s.weapons.sys == "PD" && s.weapons.mount == "PDA" && s.weapons.loaders, s.weapons.sys + " " + s.weapons.mount + ", loaders " + s.weapons.loaders);
        bool noSol = !s.weapons.Sol.set;
        yield return (Func<bool>)(() => s.weapons.wepCharge >= s.weapons.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.weapons.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (PDC-A)")) yield break; yield return 0.15f; }
        Check("The ladder arms with no solution: PDC-A is on AUTO", noSol && System.Array.TrueForAll(s.weapons.sw, b => b) && s.weapons.pdcAuto[0] && !s.weapons.pdcAuto[1], string.Join(",", System.Array.ConvertAll(s.weapons.sw, b => b ? "1" : "0")));
        //     We hit it (stand-in for a railgun hit, tested above): it fires back.
        s.weapons.stats.pdcKillChance = 1f; // stand-in: a sure PDC, so the result is certain (the real chance is in the Tuning window)
        brain.firesBack = true;
        int enemyCells = dummyShip.weapons.vlsCells, kills0 = s.weapons.pdcKills, taken0 = s.weapons.hitsTaken;
        var pdcA = s.weapons.MountOf("PDA"); var pdcB = s.weapons.MountOf("PDB");
        int pdcA0 = pdcA.ammo, pdcB0 = pdcB.ammo;
        dummyShip.damage.TakeSlug("PROW", s.weapons.stats);
        yield return (Func<bool>)(() => s.weapons.Incoming.Count > 0);
        float salvo1At = board.MissionSeconds;
        Check("Hit, GARUDA fires a salvo of two back at us", s.weapons.Incoming.Count == 2 && dummyShip.weapons.vlsCells == enemyCells - 2, s.weapons.Incoming.Count + " incoming, its cells " + enemyCells + " → " + dummyShip.weapons.vlsCells);
        yield return 0.5f;
        Check("The alarm strip warns of them, with their bearing (owner 2026-10-28)", alarmWords.gameObject.activeInHierarchy && alarmWords.text.StartsWith("INCOMING MISSILES · BRG "), "\"" + alarmWords.text + "\"");
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => s.weapons.Incoming.Count == 0 || System.Math.Sqrt((s.weapons.Incoming[0].x - s.nav.x) * (s.weapons.Incoming[0].x - s.nav.x) + (s.weapons.Incoming[0].y - s.nav.y) * (s.weapons.Incoming[0].y - s.nav.y)) < 4600);
        Shot("28e_incoming_pdc_arc");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        yield return (Func<bool>)(() => s.weapons.Incoming.Count == 0);
        yield return (Func<bool>)(() => s.weapons.Incoming.Count == 0); // 15 s each; the flight is about 13 s at x10
        Check("PDC-A shoots both down; nothing hits us", s.weapons.pdcKills == kills0 + 2 && s.weapons.hitsTaken == taken0 && s.weapons.pdcReport == "PDC-A · MISSILE DOWN",
              (s.weapons.pdcKills - kills0) + " down, " + (s.weapons.hitsTaken - taken0) + " hits on us, \"" + s.weapons.pdcReport + "\"");
        Check("Each go took " + s.weapons.stats.pdcRoundsPerGo + " rounds from PDC-A, and the drawer shows it; PDC-B untouched",
              pdcA0 == 2400 && pdcA.ammo == pdcA0 - 2 * s.weapons.stats.pdcRoundsPerGo && pdcB.ammo == pdcB0 && sysText().StartsWith(pdcA.ammo + " RDS"),
              "PDC-A " + pdcA0 + " → " + pdcA.ammo + ", PDC-B " + pdcB0 + " → " + pdcB.ammo + ", \"" + sysText() + "\"");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Changing weapons leaves PDC-A on AUTO; back on PDC-A its ladder shows armed.
        for (int k = 0; k < 3 && s.weapons.sys != "RG"; k++) { if (!Click("fc.system", null, "weapon selector (to RAILGUN)")) yield break; yield return 0.2f; }
        Check("On the railgun, PDC-A stays on AUTO", s.weapons.sys == "RG" && s.weapons.pdcAuto[0], s.weapons.sys);
        for (int k = 0; k < 3 && s.weapons.sys != "PD"; k++) { if (!Click("fc.system", null, "weapon selector (back to PDC)")) yield break; yield return 0.2f; }
        Check("Back on PDC-A, its ladder is still armed", s.weapons.sys == "PD" && s.weapons.mount == "PDA" && System.Array.TrueForAll(s.weapons.sw, b => b), "");
        //     Dropping ARM takes it off AUTO: the next salvo gets through.
        if (!Click("fc.flip", "3", "ARM rocker (drop)")) yield break;
        Check("Dropping ARM takes PDC-A off AUTO", !s.weapons.sw[3] && !s.weapons.pdcAuto[0], "");
        var ourArmour = (float[])s.damage.SideArmour.Clone();
        var ourInteg = System.Array.ConvertAll(s.damage.compartments, c => c.integ);
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => board.MissionSeconds - salvo1At > 125f); // its 120 s between salvos
        dummyShip.damage.TakeSlug("PROW", s.weapons.stats); // stand-in for another railgun hit
        yield return (Func<bool>)(() => s.weapons.Incoming.Count > 0);
        yield return (Func<bool>)(() => s.weapons.Incoming.Count > 0); // its 120 s between salvos
        yield return (Func<bool>)(() => s.weapons.Incoming.Count == 0);
        yield return (Func<bool>)(() => s.weapons.Incoming.Count == 0);
        yield return 0.4f;
        Check("With no PDC on AUTO, both hit us, with the alert tone", s.weapons.hitsTaken == taken0 + 2 && s.weapons.incomingReport != null && s.weapons.incomingReport.StartsWith("MISSILE HIT · ") && board.LastSound == "alert",
              (s.weapons.hitsTaken - taken0) + " hits, last \"" + s.weapons.incomingReport + "\", last cue \"" + board.LastSound + "\"");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Step 96: GARUDA's HE hits 1.5x harder now, and two hits leave our magazine below the 3 it needs to fire.
        //     Stand-in for damage control: our ship as it was before that salvo.
        System.Array.Copy(ourArmour, s.damage.SideArmour, ourArmour.Length);
        for (int c = 0; c < s.damage.compartments.Length; c++) s.damage.compartments[c].integ = ourInteg[c];
        //     PDC-B (owner 2026-10-27), on its last go. Stand-in for the helm: turned so GARUDA bears 200° (PDC-B's arc).
        s.nav.heading = s.nav.orderedHeading = ((t07.bearing - 200) % 360 + 360) % 360; // it has been patrolling since 21e
        if (!Click("fc.mount", null, "PDC selector (PDC-B)")) yield break;
        int pdbWas = pdcB.ammo;
        pdcB.ammo = s.weapons.stats.pdcRoundsPerGo; // stand-in: one go left
        yield return (Func<bool>)(() => s.weapons.wepCharge >= s.weapons.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.weapons.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (PDC-B)")) yield break; yield return 0.15f; }
        Check("PDC-B arms and goes on AUTO", s.weapons.mount == "PDB" && s.weapons.pdcAuto[1] && !s.weapons.pdcAuto[0], s.weapons.mount + ", AUTO " + s.weapons.pdcAuto[0] + "/" + s.weapons.pdcAuto[1]);
        int killsB = s.weapons.pdcKills, takenB = s.weapons.hitsTaken;
        s.weapons.LaunchAt(dummyShip.weapons.stats, t07.worldX, t07.worldY, 2, 0, 35000); // stand-in for GARUDA's salvo (its own firing is checked above)
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => s.weapons.Incoming.Count == 0);
        yield return (Func<bool>)(() => s.weapons.Incoming.Count == 0);
        yield return 0.4f;
        Check("PDC-B downs one on its last go, runs dry and drops off AUTO; the other hits us",
              s.weapons.pdcKills == killsB + 1 && s.weapons.hitsTaken == takenB + 1 && pdcB.ammo == 0 && !s.weapons.pdcAuto[1] && !s.weapons.sw[0] && s.weapons.pdcReport == "PDC-B · MISSILE DOWN",
              (s.weapons.pdcKills - killsB) + " down, " + (s.weapons.hitsTaken - takenB) + " hits, " + pdcB.ammo + " rounds, AUTO " + s.weapons.pdcAuto[1] + ", \"" + s.weapons.pdcReport + "\"");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        if (!Click("fc.flip", "0", "LOAD (PDC-B dry)")) yield break;
        Check("Dry, PDC-B's LOAD refuses", !s.weapons.sw[0] && board.LastSound == "refused", "last cue \"" + board.LastSound + "\"");
        pdcB.ammo = pdbWas;
        //     Our ship can die too (Step 58): its port side open, three slugs into the reactor (stand-in for enemy fire).
        s.damage.SideArmour[3] = 0f;
        for (int k = 0; k < 3; k++) s.damage.TakeSlug("PORT", dummyShip.weapons.stats);
        yield return 1f;
        Check("Our reactor at 0: our ship is destroyed and goes dark", s.destroyed && s.damage.Comp("rct").integ == 0 && s.power.TotalFill == 0,
              "RCT " + s.damage.Comp("rct").integ + ", power drawn " + s.power.TotalFill);
        s.destroyed = false;
        //     Clean up for the steps below: TRACK 07 held still and quiet, our ship repaired (stand-in for damage control).
        brain.firesBack = false; brain.enabled = false; t07Ship.nav.vx = t07Ship.nav.vy = 0;
        s.weapons.stats.pdcKillChance = 0.7f;
        System.Array.Copy(ourArmour, s.damage.SideArmour, ourArmour.Length);
        for (int c = 0; c < s.damage.compartments.Length; c++) s.damage.compartments[c].integ = ourInteg[c];
        for (int k = 0; k < 3 && s.weapons.sys != "RG"; k++) { if (!Click("fc.system", null, "weapon selector (RAILGUN)")) yield break; yield return 0.2f; }

        // 21f. Boarding (Step 60, BOARDING_README.md): the assault boats, on DISABLE, carry marines to TRACK 07,
        //     hold off while no face is open, board, wreck its engines and magazine, and come home. DISABLE keeps it
        //     alive for the fight below; DESTROY and SECURE were checked on the bare ship model.
        //     Stand-in for the helm: 1,500 units off its starboard beam, stopped.
        double sb2 = (t07.worldHdg + 90) * System.Math.PI / 180;
        s.nav.x = t07.worldX + System.Math.Sin(sb2) * 1500; s.nav.y = t07.worldY + System.Math.Cos(sb2) * 1500;
        s.nav.vx = s.nav.vy = 0; s.nav.throttle = 0;
        s.nav.heading = s.nav.orderedHeading = ((int)System.Math.Round(t07.worldHdg) + 270) % 360;
        s.weapons.target = "t07";
        vxField.SetValue(plotPanel, (s.nav.x + t07.worldX) / 2); vyField.SetValue(plotPanel, (s.nav.y + t07.worldY) / 2);
        if (!Click("drawer", "hangar", "HGR key")) yield break;
        yield return 0.8f;
        if (s.hangar.CurrentSquad.key != "ih" && !Click("hangar.squad", null, "squadron selector (ASSAULT BOAT)")) yield break;
        for (int k = 0; k < 3 && s.hangar.order != "disable"; k++) if (!Click("hangar.order", null, "order knob")) yield break;
        Check("The assault boats are selected, on DISABLE", s.hangar.CurrentSquad.key == "ih" && s.hangar.order == "disable" && s.hangar.SquadReady(s.hangar.CurrentSquad) == 3, s.hangar.CurrentSquad.key + " " + s.hangar.order + ", " + s.hangar.SquadReady(s.hangar.CurrentSquad) + " ready");
        s.hangar.SetLbrg(s.nav.heading.ToString()); // stand-in for typing the launch bearing
        float defenders = s.hangar.stats.defenderShare;
        s.hangar.stats.defenderShare = 0f; // stand-in: an undefended ship, so the marines win for certain (the odds are in the Tuning window)
        var armourWas = (float[])dummyShip.damage.SideArmour.Clone();
        for (int k = 0; k < 4; k++) dummyShip.damage.SideArmour[k] = Mathf.Max(dummyShip.damage.SideArmour[k], 10f); // stand-in: no face open, so the boats must hold off at first
        for (int i = 0; i < s.hangar.stages.Length; i++)
        {
            if (!Click("hangar.stage", i.ToString(), s.hangar.stages[i].label + " stage (boats)")) yield break;
            int k = i;
            yield return (Func<bool>)(() => s.hangar.stages[k].progress >= 100f);
        }
        int marinesBefore = s.hangar.marines;
        if (!PlotDrag(board, hLeverAt, hLeverAt + new Vector2(240, 0), "pull the launch lever (boats)", "hangar.lever")) yield break;
        yield return 0.4f;
        var boats = s.hangar.Flights.Find(f => f.squad == "ih");
        Check("Three boats launch with 18 marines", boats != null && boats.craft == 3 && boats.marines == 18 && s.hangar.marines == marinesBefore - 18,
              boats != null ? boats.craft + " boats, " + boats.marines + " marines, " + s.hangar.marines + " left aboard" : "no flight");
        if (boats == null) yield break;
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => boats.holding != null);
        yield return (Func<bool>)(() => boats.holding != null);
        Check("No face open: they hold off", interlockText() == "HOLDING OFF GARUDA · NO OPEN SIDE", "\"" + interlockText() + "\"");
        Shot("28f_boats_holding");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        dummyShip.damage.SideArmour[1] = 0f; // stand-in for a slug opening its starboard side
        yield return (Func<bool>)(() => boats.aboard);
        Check("A face open, the ship stopped: they board", boats.aboard && interlockText().StartsWith("BOARDING GARUDA · "), "\"" + interlockText() + "\"");
        //     Owner 2026-10-27: under way again with marines aboard, it fights on and the boats ride with it.
        //     Stand-in for its engines: top speed along its heading, its brain on (the world moves it only then).
        double bh = t07.worldHdg * System.Math.PI / 180;
        t07Ship.nav.vx = System.Math.Sin(bh) * t07Ship.nav.stats.topSpeedUnits; t07Ship.nav.vy = System.Math.Cos(bh) * t07Ship.nav.stats.topSpeedUnits;
        brain.enabled = true;
        float boardT0 = boats.boardT, boardAt = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds - boardAt >= 15f); // two ticks at x10
        // the ship moves after the boats in a tick, so they trail it by up to one tick of its way
        double boatGap = System.Math.Sqrt((boats.x - t07.worldX) * (boats.x - t07.worldX) + (boats.y - t07.worldY) * (boats.y - t07.worldY));
        Check("Under way, the marines fight on and the boats ride with it", boats.aboard && t07Ship.nav.SpeedUnits > s.hangar.stats.boardMaxSpeedUnits
              && boats.boardT - boardT0 >= board.MissionSeconds - boardAt - 0.01f && boatGap <= s.hangar.stats.dockUnits + t07Ship.nav.stats.topSpeedUnits * board.Rate
              && interlockText().StartsWith("BOARDING GARUDA · "),
              "its speed " + t07Ship.nav.SpeedUnits.ToString("0.0") + " km/s, fight clock +" + (boats.boardT - boardT0).ToString("0") + " s in " + (board.MissionSeconds - boardAt).ToString("0") + " s, boats "
              + boatGap.ToString("0") + " km off, \"" + interlockText() + "\"");
        yield return (Func<bool>)(() => !boats.aboard);
        Check("The marines win and disable it: engines and magazine wrecked, the ship still alive", boats.outcome == "GARUDA DISABLED" && dummyShip.damage.Comp("eng").integ == 0 && dummyShip.damage.Comp("mag").integ == 0 && !dummyShip.destroyed && !t07.wreck,
              "\"" + boats.outcome + "\", ENG " + dummyShip.damage.Comp("eng").integ + ", MAG " + dummyShip.damage.Comp("mag").integ);
        Check("The hangar reports it on the way home", interlockText().StartsWith("GARUDA DISABLED · RETURNING"), "\"" + interlockText() + "\"");
        yield return (Func<bool>)(() => s.hangar.Flights.Count == 0);
        yield return (Func<bool>)(() => s.hangar.Flights.Count == 0);
        Check("They land: the marines are back aboard", s.hangar.Flights.Count == 0 && s.hangar.marines == marinesBefore && !s.hangar.Launched(s.hangar.CurrentSquad), s.hangar.marines + " marines aboard");
        //     SECURE and DESTROY, in the run (owner 2026-10-27). Each is put back after, for the fight below.
        //     Stand-in for the stage rockers and launch lever (checked above).
        Action<string> launchBoats = order => { s.hangar.order = order; foreach (var g in s.hangar.stages) { g.on = true; g.progress = 100f; } s.hangar.LaunchSquad(); };
        yield return (Func<bool>)(() => t07Ship.nav.SpeedUnits <= s.hangar.stats.boardMaxSpeedUnits); // its engines wrecked, it slows to a stop
        brain.enabled = false; t07Ship.nav.vx = t07Ship.nav.vy = 0;
        launchBoats("secure");
        boats = s.hangar.Flights.Find(f => f.squad == "ih");
        if (boats == null) { Check("The boats launch on SECURE", false, s.hangar.LaunchBlock() ?? ""); yield break; }
        yield return (Func<bool>)(() => boats.outcome != null);
        yield return (Func<bool>)(() => boats.outcome != null);
        Check("SECURE: the marines take it, and it is no longer hostile", boats.outcome == "GARUDA SECURED" && dummyShip.captured && t07.captured && !t07.hostile,
              "\"" + boats.outcome + "\", captured " + dummyShip.captured + ", hostile " + t07.hostile);
        yield return (Func<bool>)(() => s.hangar.Flights.Count == 0);
        yield return (Func<bool>)(() => s.hangar.Flights.Count == 0);
        dummyShip.captured = t07.captured = false; t07.hostile = true;
        int rctWas = dummyShip.damage.Comp("rct").integ, killsWas = s.kills;
        launchBoats("destroy");
        boats = s.hangar.Flights.Find(f => f.squad == "ih");
        if (boats == null) { Check("The boats launch on DESTROY", false, s.hangar.LaunchBlock() ?? ""); yield break; }
        yield return (Func<bool>)(() => boats.outcome != null);
        yield return (Func<bool>)(() => boats.outcome != null);
        Check("DESTROY: the marines blow its reactor, and it is a wreck", boats.outcome == "GARUDA SCUTTLED" && dummyShip.destroyed && t07.wreck && s.kills == killsWas + 1,
              "\"" + boats.outcome + "\", destroyed " + dummyShip.destroyed + ", wreck " + t07.wreck);
        dummyShip.destroyed = false; t07.wreck = false; t07.hostile = true; dummyShip.damage.Comp("rct").integ = rctWas;
        yield return (Func<bool>)(() => s.hangar.Flights.Count == 0);
        yield return (Func<bool>)(() => s.hangar.Flights.Count == 0);
        Check("Both runs home, every marine back", s.hangar.Flights.Count == 0 && s.hangar.marines == marinesBefore, s.hangar.marines + " marines aboard");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Range (owner 2026-10-27): the boats chase only inside 15,000 units of our ship; coming home out of range,
        //     a clock runs, with the alert tone; at its end the boats and their marines are lost.
        //     Stand-in for the helm: our ship jumps 18,000 units from GARUDA, toward the middle of the battle space.
        double homeX = s.nav.x, homeY = s.nav.y, mx = -t07.worldX, my = -t07.worldY, ml = System.Math.Sqrt(mx * mx + my * my);
        if (ml < 1) { mx = 1; my = 0; ml = 1; }
        double farX = t07.worldX + mx / ml * 18000, farY = t07.worldY + my / ml * 18000;
        int warned0 = s.hangar.boatWarnings;
        launchBoats("disable");
        boats = s.hangar.Flights.Find(f => f.squad == "ih");
        if (boats == null) { Check("The boats launch on DISABLE", false, s.hangar.LaunchBlock() ?? ""); yield break; }
        s.nav.x = farX; s.nav.y = farY;
        yield return (Func<bool>)(() => s.hangar.boatWarnings > warned0);
        yield return 0.2f;
        Check("GARUDA past 15,000 km: the boats give up the chase", boats.returning && boats.outcome == "OUT OF RANGE", "\"" + boats.outcome + "\"");
        Check("Our ship past 15,000 km of them: stranded, the clock runs, with the alert tone",
              boats.stranded && boats.strandT > 0 && interlockText().StartsWith("OUT OF RANGE · STRANDED · CLOSE IN · ") && s.hangar.boatWarnings == warned0 + 1 && board.LastSound == "alert",
              "\"" + interlockText() + "\", last cue \"" + board.LastSound + "\"");
        //     Stand-in for closing in: our ship 10,000 units from them, inside their range.
        double bx = farX - boats.x, by = farY - boats.y, bl = System.Math.Sqrt(bx * bx + by * by);
        s.nav.x = boats.x + bx / bl * 10000; s.nav.y = boats.y + by / bl * 10000;
        yield return 1f;
        float strandAt = boats.strandT;
        yield return 1f;
        Check("Back in range, the clock stops and they come home", !boats.stranded && boats.strandT == strandAt && interlockText().StartsWith("OUT OF RANGE · RETURNING · "),
              "clock " + boats.strandT.ToString("0.0") + " s, \"" + interlockText() + "\"");
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => s.hangar.Flights.Count == 0);
        yield return (Func<bool>)(() => s.hangar.Flights.Count == 0); // 15 s each; 10,000 units is about 14 s at x10
        Check("They land: every marine back", s.hangar.Flights.Count == 0 && s.hangar.marines == marinesBefore, s.hangar.marines + " marines aboard");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        float strandSecs = s.hangar.stats.boatStrandSeconds;
        s.hangar.stats.boatStrandSeconds = 5f; // stand-in: a short clock
        s.nav.x = homeX; s.nav.y = homeY;
        launchBoats("disable");
        s.nav.x = farX; s.nav.y = farY;
        yield return (Func<bool>)(() => s.hangar.Flights.Count == 0);
        yield return 0.2f;
        var boatCraft = s.hangar.squads[s.hangar.squad].craft;
        int lostCraft = System.Array.FindAll(boatCraft, c => c.state == "LOST").Length;
        Check("Left too long, the boats and their marines are lost, with the alert tone",
              s.hangar.Flights.Count == 0 && lostCraft == 3 && s.hangar.marines == marinesBefore - 18 && s.hangar.boatWarnings == warned0 + 3 && board.LastSound == "alert",
              lostCraft + " boats lost, " + s.hangar.marines + " marines aboard, last cue \"" + board.LastSound + "\"");
        Check("No boats left to launch", s.hangar.SquadReady(s.hangar.CurrentSquad) == 0 && s.hangar.LaunchBlock() == "NO CRAFT READY", s.hangar.LaunchBlock() ?? "");
        foreach (var c in boatCraft) if (c.state == "LOST") c.state = "READY"; // put back: boats and marines resupplied
        s.hangar.marines = marinesBefore; s.hangar.stats.boatStrandSeconds = strandSecs;
        s.nav.x = homeX; s.nav.y = homeY;
        System.Array.Copy(armourWas, dummyShip.damage.SideArmour, armourWas.Length); // its armour as it was
        s.hangar.stats.defenderShare = defenders;
        brain.Think(dummyShip, s); // one thought, brain still off: with its engine room wrecked it makes no way
        Check("Disabled, its brain cannot drive it", dummyShip.nav.throttle == 0, "throttle " + dummyShip.nav.throttle + "%");
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.6f;

        // 22. A fight to the kill (Step 49; Step 58: the reactor sits behind the PORT face): close in off
        //     TRACK 07's port side and shoot until it is a wreck.
        var before = (float[])dummyShip.damage.SideArmour.Clone(); // the shots above may have marked its sides
        var integBefore = System.Array.ConvertAll(dummyShip.damage.compartments, c => c.integ);
        //     Stand-in for the helm: 10,000 units off its port beam, stopped, side-on so turret B (port) bears.
        double tb = (t07.worldHdg - 90) * System.Math.PI / 180;
        s.nav.x = t07.worldX + System.Math.Sin(tb) * 10000; s.nav.y = t07.worldY + System.Math.Cos(tb) * 10000;
        s.nav.vx = s.nav.vy = 0; s.nav.throttle = 0;
        s.nav.heading = s.nav.orderedHeading = ((int)System.Math.Round(t07.worldHdg) + 180) % 360;
        //     Step 82: our ship's death in 21e emptied every bus; SEN needs a unit for DRADIS to hear (stand-in for the power drawer)
        s.power.buses[2].fill = 1;
        yield return 1f;
        s.LookAt("t07");
        s.weapons.sol[s.weapons.SysIndex] = new WeaponSystem.Solution(); // stand-in: clear the old solution
        if (!PlotDrag(board, knobAt, knobAt + new Vector2(8, 0), "turn the RNG knob (1 step)", "fc.knob")) yield break;
        best = 0;
        for (int d = -1; d <= 1; d++) if (MissUnits(s, t07, s.nav.heading + s.weapons.Sol.brg + d) < MissUnits(s, t07, s.nav.heading + s.weapons.Sol.brg + best)) best = d;
        if (best != 0 && !PlotDrag(board, brgAt, brgAt + new Vector2(6 * best, 0), "turn the BRG knob to the nearest degree", "fc.knob")) yield break;
        s.SlewTo();
        //     The targeting computer (the periscope) shows a scanned ship's hull in its SUBJECT box (Step 61).
        if (board.Motion) board.Motion.SetScopeDown(true);
        yield return 1.2f;
        var scope = board.GetComponentInChildren<PeriscopePanel>(true);
        var subjName = typeof(PeriscopePanel).GetField("subjectName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(scope) as TMPro.TMP_Text;
        var subjHull = typeof(PeriscopePanel).GetField("subjectClass", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(scope) as TMPro.TMP_Text;
        string hullNow = PeriscopePanel.HullLine(dummyShip);
        Check("Scanned, the targeting computer shows its hull: each face, and the room behind an open one", subjName && subjHull && subjName.text.StartsWith("GARUDA") && subjHull.text == hullNow && hullNow.StartsWith("BOW ") && hullNow.Contains(" \u25B8 "),
              subjName && subjHull ? "\"" + subjName.text + "\" / \"" + subjHull.text + "\"" : "no readout");
        //     CLASS in the targeting computer names its class (owner 2026-10-24). Step 75 (owner 2026-10-26): the plot shows
        //     TRACK 07 as its own ship, in red, because it is in sight (10,000 units), not because of CLASS.
        //     Step 109 (owner 2026-10-29): the sight shows the Garuda's model; CLASS works only once it fills the brackets.
        var shipPic = typeof(PeriscopePanel).GetField("shipPicture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(scope) as UnityEngine.UI.RawImage;
        s.scopeZoom = 8; // stand-in for the MAG knob: zoomed right in, the ship spills out of the brackets
        if (!Click("scope.classify", null, "CLASS key, zoomed in too far")) yield break;
        yield return 0.3f;
        Check("The sight shows the Garuda; zoomed in too far it spills out of the brackets, and CLASS does nothing", shipPic && shipPic.enabled && shipPic.texture && !s.classified.Contains("t07"),
              "picture " + (shipPic && shipPic.enabled) + ", fill " + s.ScopeFill(t07).ToString("0.00") + ", " + t07.rangeUnits.ToString("N0") + " km");
        s.scopeZoom = System.Math.Max(1, System.Math.Min(8, System.Math.Round(t07.rangeUnits / s.scopeFitUnits, 1))); // stand-in: the MAG knob to fill the brackets
        yield return 0.3f;
        if (!Click("scope.classify", null, "CLASS key")) yield break;
        yield return 0.6f;
        Check("Classified and in sight: GARUDA shows on the plot as its own ship model, in place of its dot", s.classified.Contains("t07") && s.sensors.InSight(t07) && plotPanel.ContactShipShown && subjName.text == "GARUDA · CRUISER",
              "\"" + subjName.text + "\", " + t07.rangeUnits.ToString("N0") + " km, picture " + plotPanel.ContactShipShown);
        float sightWas = s.sensors.stats.visualRangeUnits;
        s.sensors.stats.visualRangeUnits = t07.rangeUnits - 1000f; // stand-in for drawing off out of sight
        yield return 0.6f;
        Check("Out of sight it is a dot again, where the sweep last touched it", !plotPanel.ContactShipShown && t07.seen, "picture " + plotPanel.ContactShipShown);
        s.sensors.stats.visualRangeUnits = sightWas;
        yield return 0.6f;
        //     The sweep runs from a small circle round our ship, not from the ship itself.
        var sweepInner = (float)typeof(SweepGraphic).GetField("innerPx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                .GetValue(plotPanel.GetComponentInChildren<SweepGraphic>(true));
        Check("The sweep starts at a small circle just outside our ship", sweepInner > 15f, sweepInner.ToString("0") + " px");
        Shot("28g_targeting_computer");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (board.Motion) board.Motion.SetScopeDown(false);
        yield return 0.6f;
        if (s.weapons.mount != "RGB" && !Click("fc.mount", null, "turret selector (RG-B)")) yield break;
        if (!Click("rate", "10", "x10 rate key")) yield break;
        var reports = new List<string>();
        for (int shot = 0; shot < 10 && !t07.wreck; shot++)
        {
            yield return (Func<bool>)(() => s.weapons.wepCharge >= s.weapons.ShotCost);
            for (int i = 0; i < 4; i++)
                if (!s.weapons.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (shot " + (shot + 1) + ")")) yield break; yield return 0.15f; }
            int landed = s.weapons.shotResults;
            if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "fire (shot " + (shot + 1) + ")", "fc.lever")) yield break;
            yield return (Func<bool>)(() => s.weapons.shotResults > landed);
            reports.Add(s.weapons.shotReport);
        }
        string log = string.Join(" | ", reports);
        int opened = reports.FindIndex(r => r == "HIT · GARUDA · PORT OPEN");
        bool armourFirst = opened >= 0;
        for (int i = 0; i < opened; i++) if (!reports[i].StartsWith("HIT · GARUDA · PORT ARMOUR ")) armourFirst = false;
        Check("The first hits open its port side", armourFirst, log);
        bool reactorOnly = opened >= 0;
        for (int i = opened + 1; i < reports.Count - 1; i++) if (reports[i] != "HIT · GARUDA · PORT · RCT −2") reactorOnly = false;
        Check("Then each hit takes 2 off the reactor behind it, the same room every time", reports.Count - opened - 2 > 0 && reactorOnly, reports.Count + " shots");
        Check("Its reactor gone, it is destroyed: a cold wreck", t07.wreck && dummyShip.destroyed && dummyShip.damage.Comp("rct").integ == 0 && !s.sensors.Heard(t07) && reports[reports.Count - 1] == "GARUDA DESTROYED",
              "RCT " + dummyShip.damage.Comp("rct").integ + ", last \"" + reports[reports.Count - 1] + "\"");
        bool untouched = dummyShip.damage.SideArmour[0] == before[0] && dummyShip.damage.SideArmour[1] == before[1] && dummyShip.damage.SideArmour[2] == before[2];
        for (int c = 0; c < dummyShip.damage.compartments.Length; c++) if (dummyShip.damage.compartments[c].key != "rct" && dummyShip.damage.compartments[c].integ != integBefore[c]) untouched = false;
        Check("Nothing else on it was touched", untouched, "");
        yield return 0.4f;
        Check("The wreck stays on the plot: in sight, its model shows (grey) and drifts on (owner 2026-10-26)", s.sensors.InSight(t07) && plotPanel.ContactShipShown, t07.rangeUnits.ToString("N0") + " km, picture " + plotPanel.ContactShipShown);
        Check("The kill cue sounds", board.LastSound == "kill", "last cue \"" + board.LastSound + "\"");
        Shot("29_destroyed");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("rate", "1", "x1 rate key")) yield break;
        if (!Click("drawer", "fire control", "FIRE key (close)")) yield break;
        yield return 0.5f;

        // 23. Engineering's ship diagram (Step 74): our ship in four parts, each side coloured by its armour;
        //     the room rings coloured by their integrity. White whole, amber hurt, red open or nearly gone.
        if (!Click("drawer", "engineering", "ENGR key")) yield break;
        yield return 0.8f;
        var eng = board.GetComponentInChildren<EngineeringPanel>(true);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var sides = typeof(EngineeringPanel).GetField("hullSides", flags).GetValue(eng) as UnityEngine.UI.RawImage[];
        var engRows = typeof(EngineeringPanel).GetField("rows", flags).GetValue(eng) as EngineeringPanel.Row[];
        System.Func<string, Wedge> ring = k => { foreach (var r in engRows) if (r.key == k) return r.ring[0]; return null; };
        Color white = new Color32(0xE8, 0xED, 0xF0, 0xFF), amber = new Color32(0xE0, 0xA0, 0x20, 0xFF), red = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
        // the pictures are pre-encoded for the board (Paint); the ring segments encode themselves
        System.Func<UnityEngine.UI.Graphic, Color, bool> lit = (g, c) => g && Mathf.Abs(g.color.r - c.r) < 0.01f && Mathf.Abs(g.color.g - c.g) < 0.01f && Mathf.Abs(g.color.b - c.b) < 0.01f;
        System.Func<int, Color, bool> sideLit = (i, c) => lit(sides[i], c.gamma);
        bool allWhite = sides != null && sides.Length == 4;
        if (allWhite) for (int i = 0; i < 4; i++) if (!sides[i] || !sides[i].enabled || !sides[i].texture || !sideLit(i, white)) allWhite = false;
        Check("Engineering shows our ship in four parts, all white: armour full", allWhite, sides != null ? sides.Length + " parts" : "none");
        Check("The room rings are white: undamaged", lit(ring("hab"), white) && lit(ring("hgr"), white), "");
        //     Stand-in for enemy fire: starboard hit once, the aft open; habitation hurt, hangar nearly gone.
        var armour = s.damage.SideArmour;
        armour[1] = s.damage.stats.sideArmourMax[1] / 2f; armour[2] = 0f;
        s.damage.Comp("hab").integ = 2; s.damage.Comp("hgr").integ = 1;
        yield return 0.6f;
        Check("A hit side turns amber, an open side red", sideLit(1, amber) && sideLit(2, red) && sideLit(0, white) && sideLit(3, white), "STBD half, AFT open");
        Check("A hurt room's ring turns amber, a nearly-gone one red", lit(ring("hab"), amber) && lit(ring("hgr"), red) && lit(ring("mag"), white), "HAB 2, HGR 1");
        Shot("30_engineering_diagram");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        armour[1] = s.damage.stats.sideArmourMax[1]; armour[2] = s.damage.stats.sideArmourMax[2];
        s.damage.Comp("hab").integ = 5; s.damage.Comp("hgr").integ = 5;
        yield return 0.6f;
        Check("Repaired, they go white again", sideLit(1, white) && sideLit(2, white) && lit(ring("hab"), white) && lit(ring("hgr"), white), "");

        // 24. Conditions (Step 59, DAMAGE_README.md): fire, vacuum and radiation on our ship, handled with
        //     engineering's own rockers. Hits are stand-ins (enemy fire comes with the ship AI); chances and
        //     spread are set to certain so each result is exact; the real odds are in the Tuning window.
        var armour0 = (float[])s.damage.SideArmour.Clone();
        var rooms0 = System.Array.ConvertAll(s.damage.compartments, c => new[] { c.integ, c.crew });
        float spreadF = s.damage.stats.fireSpreadPerSecond, spreadV = s.damage.stats.vacuumSpreadPerSecond, spreadR = s.damage.stats.radSpreadPerSecond;
        s.damage.stats.fireSpreadPerSecond = s.damage.stats.vacuumSpreadPerSecond = s.damage.stats.radSpreadPerSecond = 0f;
        var recallRocker = System.Array.Find((EngineeringPanel.Repair[])typeof(EngineeringPanel).GetField("repairs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(eng), r => r.id == "recall");
        Check("DUMMY 1 is now RECALL, with the repair rockers' art", recallRocker != null && recallRocker.fitted && recallRocker.rocker.sprite.name.StartsWith("rocker_22x30") && recallRocker.label.text == "RECALL",
              recallRocker != null ? recallRocker.rocker.sprite.name + ", \"" + recallRocker.label.text + "\"" : "missing");
        //     An HE hit through our open bow sets the magazine on fire.
        s.damage.SideArmour[0] = 0f; s.weapons.stats.fireChanceHE = 1f; // stand-in shooter: our own numbers (the same as the Garuda's)
        string heHit = s.damage.TakeWarhead("PROW", "HE", s.weapons.stats);
        Check("HE through the open bow: the magazine catches fire", heHit.EndsWith(" · FIRE") && s.damage.Comp("mag").fire && s.damage.Comp("mag").cond == "FIRE", "\"" + heHit + "\"");
        //     The fire spreads to the magazine's neighbours, and only those.
        s.damage.stats.fireSpreadPerSecond = 1f;
        float tickF = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds > tickF); // exactly one tick of spread
        s.damage.stats.fireSpreadPerSecond = 0f;
        Check("It spreads to the rooms next to it (bridge, reactor, hangar), not the others", s.damage.Comp("bri").fire && s.damage.Comp("rct").fire && s.damage.Comp("hgr").fire && !s.damage.Comp("eng").fire && !s.damage.Comp("hab").fire,
              "BRI " + s.damage.Comp("bri").fire + ", RCT " + s.damage.Comp("rct").fire + ", HGR " + s.damage.Comp("hgr").fire + ", ENG " + s.damage.Comp("eng").fire + ", HAB " + s.damage.Comp("hab").fire);
        //     A fire burns its room down.
        if (!Click("rate", "10", "x10 rate key")) yield break;
        int magBurn = s.damage.Comp("mag").integ; float burnFrom = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds - burnFrom >= s.damage.stats.fireDamageSeconds + 1);
        if (!Click("rate", "1", "x1 rate key")) yield break;
        Check("A fire takes integrity off its room over time", s.damage.Comp("mag").integ < magBurn, "MAG " + magBurn + " → " + s.damage.Comp("mag").integ);
        //     REPAIR sends the team to the magazine: it puts the fire out.
        if (!Click("eng.pick", "mag", "MAG cell")) yield break;
        s.orders.auxCharge = 100; // stand-in: a full AUX pool. Engineering's orders cost AUX since bug 6 (owner 2026-10-29), and this part runs five (130 in all)
        if (!Click("eng.run", "party", "REPAIR rocker (MAG)")) yield break;
        Check("Engineering's REPAIR spends AUX, as the orders station does (bug 6)", s.orders.auxCharge <= 100 - s.orders.OrderCost("party") + 0.5,
              "100 \u2192 " + s.orders.auxCharge.ToString("0") + "%");
        yield return (Func<bool>)(() => !s.damage.PartyOut);
        Check("The team puts the magazine's fire out", !s.damage.Comp("mag").fire && s.damage.Comp("mag").cond != "FIRE", "");
        //     VENT the bridge: the fire goes, and so does its crew.
        if (!Click("eng.pick", "bri", "BRI cell")) yield break;
        if (!Click("eng.run", "vent", "VENT rocker (BRI)")) yield break;
        Check("VENT puts the bridge's fire out, and its crew die", !s.damage.Comp("bri").fire && s.damage.Comp("bri").vented && s.damage.Comp("bri").crew == 0 && s.damage.Comp("bri").doorsShut, "crew " + s.damage.Comp("bri").crew);
        Shot("31_conditions");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        //     The reactor and hangar fires: out by stand-in, to keep the reactor whole for what follows.
        s.damage.Comp("rct").fire = false; s.damage.Comp("hgr").fire = false;
        //     A slug through our open starboard side breaches the hangar: half its crew die, and vacuum spreads.
        s.damage.SideArmour[1] = 0f; s.weapons.stats.breachChanceSlug = 1f;
        int hgrCrew = s.damage.Comp("hgr").crew;
        string slugHit = s.damage.TakeSlug("STBD", s.weapons.stats);
        Check("A slug through the open starboard side breaches the hangar: half its crew die", slugHit.EndsWith(" · BREACH") && s.damage.Comp("hgr").vented && !s.damage.Comp("hgr").doorsShut && s.damage.Comp("hgr").crew == hgrCrew - Mathf.CeilToInt(hgrCrew * s.damage.stats.breachCrewLoss),
              "\"" + slugHit + "\", crew " + hgrCrew + " → " + s.damage.Comp("hgr").crew);
        if (!Click("eng.pick", "hgr", "HGR cell")) yield break;
        Check("REPAIR waits until the doors are shut", s.damage.EngBlock("party") == "SEAL THE DOORS FIRST", s.damage.EngBlock("party") ?? "open");
        s.orders.auxCharge = 100; // stand-in, as above: SEAL and two REPAIRs follow
        if (!Click("eng.run", "seal", "SEAL rocker (HGR)")) yield break;
        s.damage.stats.vacuumSpreadPerSecond = 1f;
        float tickV = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds > tickV);
        s.damage.stats.vacuumSpreadPerSecond = 0f;
        Check("SEAL shuts its doors: the vacuum does not spread", s.damage.Comp("hgr").doorsShut && !s.damage.Comp("mag").vented && !s.damage.Comp("hab").vented && !s.damage.Comp("eng").vented, "");
        if (!Click("eng.run", "party", "REPAIR rocker (HGR)")) yield break;
        yield return (Func<bool>)(() => !s.damage.PartyOut);
        Check("The team restores the hangar's air", !s.damage.Comp("hgr").vented && !s.damage.Comp("hgr").doorsShut, "");
        //     RECALL calls a team back before it finishes.
        s.damage.Comp("eng").integ = 3;
        if (!Click("eng.pick", "eng", "ENG cell")) yield break;
        double auxHold = s.orders.auxCharge; s.orders.auxCharge = 0; // stand-in: an empty AUX pool
        string noAux = "INSUFFICIENT AUX \u2014 " + s.orders.OrderCost("party") + "% REQUIRED";
        Check("With no AUX, engineering refuses REPAIR, as the orders station does (bug 6)", s.damage.EngBlock("party") == noAux, s.damage.EngBlock("party") ?? "open");
        s.orders.auxCharge = auxHold;
        if (!Click("eng.run", "party", "REPAIR rocker (ENG)")) yield break;
        if (!Click("eng.run", "recall", "RECALL rocker")) yield break;
        yield return 2f;
        Check("RECALL brings the team back: no repair lands", !s.damage.PartyOut && s.damage.Comp("eng").integ == 3, "ENG " + s.damage.Comp("eng").integ);
        //     Two slugs into our open port side: the reactor leaks radiation.
        s.damage.SideArmour[3] = 0f; s.weapons.stats.breachChanceSlug = 0f; s.damage.stats.radChanceReactor = 1f;
        s.damage.Comp("rct").integ = 5; // the fires above burned it a little; two slugs now leave it at 1
        string radHit = s.damage.TakeSlug("PORT", s.weapons.stats) + " /" + s.damage.TakeSlug("PORT", s.weapons.stats);
        s.damage.stats.radSpreadPerSecond = 1f;
        float tickR = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds > tickR);
        s.damage.stats.radSpreadPerSecond = 0f;
        Check("The reactor leaks radiation into the rooms next to it", radHit.Contains("RADIATION") && s.damage.Comp("rct").rad && s.damage.Comp("mag").rad && s.damage.Comp("bri").rad && !s.damage.Comp("hgr").rad,
              "\"" + radHit + "\", RCT " + s.damage.Comp("rct").integ);
        if (!Click("rate", "10", "x10 rate key")) yield break;
        int magCrew = s.damage.Comp("mag").crew; float radFrom = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds - radFrom >= s.damage.stats.radCrewLossSeconds + 1);
        Check("Radiation kills crew while they stay", s.damage.Comp("mag").crew < magCrew, "MAG crew " + magCrew + " → " + s.damage.Comp("mag").crew);
        s.damage.Comp("rct").integ = s.damage.stats.reactorContainedAt; // stand-in for two repairs
        yield return (Func<bool>)(() => !s.damage.Comp("rct").rad && !s.damage.Comp("mag").rad);
        Check("Reactor repaired: the radiation fades", !s.damage.Comp("rct").rad && !s.damage.Comp("mag").rad && !s.damage.Comp("bri").rad, "");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Put our ship back (stand-in for damage control) and the real odds back.
        System.Array.Copy(armour0, s.damage.SideArmour, armour0.Length);
        for (int c = 0; c < s.damage.compartments.Length; c++)
        {
            var room = s.damage.compartments[c];
            room.integ = rooms0[c][0]; room.crew = rooms0[c][1]; room.fire = room.rad = room.vented = room.doorsShut = false; room.cond = null;
        }
        s.damage.stats.fireSpreadPerSecond = spreadF; s.damage.stats.vacuumSpreadPerSecond = spreadV; s.damage.stats.radSpreadPerSecond = spreadR;
        s.weapons.stats.fireChanceHE = condChances[0]; s.weapons.stats.fireChancePEN = condChances[1]; s.weapons.stats.breachChanceSlug = condChances[2]; s.damage.stats.radChanceReactor = condChances[3];
        if (t07Ship != null) { t07Ship.weapons.stats.fireChanceHE = condChances[0]; t07Ship.weapons.stats.fireChancePEN = condChances[1]; t07Ship.weapons.stats.breachChanceSlug = condChances[2]; t07Ship.damage.stats.radChanceReactor = condChances[3]; }
        if (!Click("drawer", "engineering", "ENGR key (close)")) yield break;
        yield return 1f;

        // 25. The mission (Step 85). It has been off since the board started, because the run above kills both ships
        //     part way. On now: GARUDA is a wreck since step 22, so the next tick opens the jump point (Step 98).
        var endPanel = board.GetComponentInChildren<EndScreenPanel>(true);
        quest.enabled = true;
        yield return (Func<bool>)(() => quest.ExitOpen);
        var exitWords = GameObject.Find("Board").transform.Find("Tactical Board/AlarmStrip/Words").GetComponent<TMPro.TMP_Text>();
        yield return (Func<bool>)(() => exitWords.gameObject.activeInHierarchy && exitWords.text.StartsWith("JUMP POINT"));
        Check("GARUDA destroyed: a jump point opens at a far edge, and the alarm strip gives its bearing and range",
              quest.Stage == QuestStage.ExitRegion && !board.Paused && exitWords.text.StartsWith("JUMP POINT · BRG ") && exitWords.text.EndsWith(" KM")
              && (quest.ExitAt - new Vector2((float)s.nav.x, (float)s.nav.y)).magnitude > 20000f,
              quest.Stage + ", \"" + exitWords.text + "\", at " + quest.ExitAt);
        s.nav.PlaceAt(quest.ExitAt.x, quest.ExitAt.y - 500f, 0); s.nav.vx = s.nav.vy = 0; // stand-in for the flight there
        yield return (Func<bool>)(() => quest.Over);
        Check("At the jump point: MISSION COMPLETE, and the game pauses at once", quest.Stage == QuestStage.MissionComplete && board.Paused && !endPanel.Showing,
              quest.Stage + ", paused " + board.Paused + ", end screen " + endPanel.Showing);
        yield return (Func<bool>)(() => endPanel.Showing);
        Check("The end screen comes up 2 s later: MISSION COMPLETE, GARUDA DESTROYED · JUMPED OUT",
              endPanel.Showing && Time.unscaledTime - quest.EndedAt >= 1.99f && EndText(endPanel, "title") == "MISSION COMPLETE" && EndText(endPanel, "reason") == "GARUDA DESTROYED · JUMPED OUT"
              && EndText(endPanel, "values").StartsWith("GARUDA · DESTROYED\n"),
              "\"" + EndText(endPanel, "title") + "\" / \"" + EndText(endPanel, "reason") + "\" / \"" + EndText(endPanel, "values").Replace("\n", " / ") + "\"");
        Shot("29_mission_complete");
        yield return 0.2f;

        //     RESTART MISSION, clicked for real: the board scene loads again, with its level.
        var oldBoard = board;
        if (!Click("end.restart", null, "RESTART MISSION")) yield break;
        yield return (Func<bool>)(() => !oldBoard && UnityEngine.Object.FindAnyObjectByType<BoardManager>());
        yield return 1.5f; // the level loads on top a frame later
        board = UnityEngine.Object.FindAnyObjectByType<BoardManager>();
        s = board.State;
        quest = UnityEngine.Object.FindAnyObjectByType<QuestSystem>();
        endPanel = board.GetComponentInChildren<EndScreenPanel>(true);
        var garuda = s.ShipOf != null ? s.ShipOf("t07") : null;
        Check("RESTART MISSION: a fresh board, paused, power cold, GARUDA back, the mission at its start, no end screen",
              board.Paused && BusesAre(s, PowerSystem.BusMode.Standby, 0) && garuda != null && !garuda.destroyed && !s.TrackOf("t07").wreck && quest.Stage == QuestStage.LocateTarget && !endPanel.Showing,
              "paused " + board.Paused + ", " + Buses(s) + ", GARUDA " + (garuda == null ? "missing" : garuda.destroyed ? "destroyed" : "afloat") + ", " + quest.Stage);
        var freshBrain = GameObject.Find("World").GetComponentInChildren<EnemyBrain>();
        if (freshBrain) freshBrain.firesBack = false; // stand-in: a missile must not lose our ship before the edge does

        // 25b. The battle space's edge (Step 86), on the fresh board: our ship at the centre, GARUDA's fire off.
        s.nav.PlaceAt(0, 0, 0); s.nav.vx = s.nav.vy = 0;
        var plotP = board.GetComponentInChildren<PlotPanel>(true);
        var plotGrid = board.GetComponentInChildren<PlotGrid>(true);
        var axisNums = typeof(PlotPanel).GetField("axisLabels", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(plotP) as List<TMPro.TextMeshProUGUI>;
        plotP.CentreOnOurShip();
        board.SetZoom(1.0);
        yield return 0.6f;
        float cellPx;
        double smallUnits = plotGrid.CellUnits(out cellPx), bigUnits = smallUnits * plotGrid.MajorEvery;
        var shown = axisNums.FindAll(lbl => lbl.gameObject.activeSelf).ConvertAll(lbl => lbl.text);
        Check("At x1 a small square is 50 units; the axis numbers: 0 level with our ship, then whole big squares of 250 (owner 2026-10-29)",
              smallUnits == 50 && AxisWhole(shown, bigUnits) && System.Math.Abs(bigUnits - 250) < 1e-6,
              "small square " + smallUnits.ToString("N0") + ", big " + bigUnits.ToString("N0") + ": " + string.Join(" ", shown.ToArray()));
        var ownView = ShipView.Own();
        float ownLenPx = ((RectTransform)GameObject.Find("Own ship (3D view)").transform).sizeDelta.x * ownView.ShipFill;
        Check("At x1 our ship (100 units) is drawn two small squares long", ownView.Ship.Stats.length == 100f && Mathf.Abs(ownLenPx - 2f * cellPx) < 0.5f,
              ownLenPx.ToString("0.0") + " px; a square is " + cellPx.ToString("0.0") + " px");
        var garudaShip = GameObject.Find("World").GetComponentInChildren<EnemyBrain>().GetComponent<Ship>();
        Check("The Garuda is 1.5 times our ship's length (150 units)", garudaShip && garudaShip.Stats.length == 150f, garudaShip ? garudaShip.Stats.length + " units" : "no Garuda");
        Shot("25b_x1");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        for (int n = 0; n < 60; n++) board.Scroll("plot", null, -1f); // the mouse wheel, out past the limit
        yield return 0.6f;
        Check("Zoomed out past the limit, the zoom stops at x0.01 (owner 2026-10-29)", board.ZoomExact == board.MinZoom && System.Math.Abs(board.MinZoom - 0.01) < 1e-6,
              "x" + board.ZoomExact.ToString("0.000") + ", limit x" + board.MinZoom.ToString("0.000"));
        Shot("25b_zoomed_out"); // the grid carries on past the battle space's heavier edge
        double cellUnits = plotGrid.CellUnits(out cellPx);
        bigUnits = cellUnits * plotGrid.MajorEvery;
        shown = axisNums.FindAll(lbl => lbl.gameObject.activeSelf).ConvertAll(lbl => lbl.text);
        Check("The axis numbers at x0.01: 0 level with our ship, then whole big squares of 25,000", AxisWhole(shown, bigUnits) && System.Math.Abs(bigUnits - 25000) < 1e-6,
              "big square " + bigUnits.ToString("N0") + ": " + string.Join(" ", shown.ToArray()));
        board.SetZoom(0.5 * X1); // in again, where the battle space is bigger than the plot
        yield return 0.6f;
        cellUnits = plotGrid.CellUnits(out cellPx);
        if (!Click("tool", "CURSOR", "CURSOR tool")) yield break;
        if (!PlotDrag(board, new Vector2(700, 300), new Vector2(2700, 2300), "drag the grid far past a corner", "plot.pan")) yield break;
        yield return 0.6f;
        Rect gridRect = plotGrid.rectTransform.rect;
        double edgeX = System.Math.Max(0, s.ArenaHalfW - gridRect.width / 2 * (cellUnits / cellPx)), edgeY = System.Math.Max(0, s.ArenaHalfH - gridRect.height / 2 * (cellUnits / cellPx));
        Check("Dragged far past a corner, the view stops with the plot's edge on the map's edge",
              System.Math.Abs(System.Math.Abs(plotP.View.x) - edgeX) < 1 && System.Math.Abs(System.Math.Abs(plotP.View.y) - edgeY) < 1,
              "view " + plotP.View.x.ToString("N0") + ", " + plotP.View.y.ToString("N0") + " km; limit ±" + edgeX.ToString("N0") + ", ±" + edgeY.ToString("N0"));
        plotP.CentreOnOurShip(); // what C calls: a script can't press keys read by Unity's old Input class
        yield return 0.6f;
        Check("C brings the view back to our ship", System.Math.Abs(plotP.View.x - s.nav.x) < 1 && System.Math.Abs(plotP.View.y - s.nav.y) < 1,
              "view " + plotP.View.x.ToString("N0") + ", " + plotP.View.y.ToString("N0") + " km");
        var edgeWords = GameObject.Find("Board").transform.Find("Tactical Board/AlarmStrip/Words").GetComponent<TMPro.TMP_Text>();
        var edgeCount = GameObject.Find("Board").transform.Find("Tactical Board/AlarmStrip/Count").GetComponent<TMPro.TMP_Text>();
        s.nav.PlaceAt(s.ArenaHalfW - 1250, 0, 90); s.nav.vx = s.nav.vy = 0;
        if (!Click("pause", null, "PAUSE/RUN key")) yield break;
        float edgeAt = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds > edgeAt);
        Check("1,250 km inside the east edge: APPROACHING THE EDGE OF THE BATTLE SPACE", edgeWords.gameObject.activeInHierarchy && edgeWords.text == "APPROACHING THE EDGE OF THE BATTLE SPACE",
              "\"" + (edgeWords.gameObject.activeInHierarchy ? edgeWords.text : "") + "\"");
        if (!Click("rate", "10", "x10 rate key")) yield break;
        s.nav.PlaceAt(s.ArenaHalfW + 100, 0, 90); s.nav.vx = s.nav.vy = 0; // just outside, at x10
        yield return (Func<bool>)(() => s.outsideFor >= 0f);
        Check("Just outside at x10: the rate drops to x1; EXITING THE BATTLE SPACE — TURN BACK, the count at 30",
              board.Rate == 1f && edgeWords.text == "EXITING THE BATTLE SPACE — TURN BACK" && edgeCount.text == "30",
              "x" + board.Rate + ", \"" + edgeWords.text + "\", count " + edgeCount.text);
        yield return (Func<bool>)(() => s.outsideFor >= 2f);
        Check("The count goes down", edgeCount.text == Mathf.CeilToInt(s.arenaReturnSeconds - s.outsideFor).ToString() && int.Parse(edgeCount.text) < 30, "count " + edgeCount.text);

        //     x10 again: at 0 our ship is lost. SHIP LOST, LEFT THE BATTLE SPACE.
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => quest.Over);
        yield return (Func<bool>)(() => endPanel.Showing);
        Check("Lost at the edge: SHIP LOST, LEFT THE BATTLE SPACE, our ship LOST",
              quest.Stage == QuestStage.MissionFailed && quest.FailReason == MissionFailReason.Desertion && EndText(endPanel, "title") == "SHIP LOST"
              && EndText(endPanel, "reason") == "LEFT THE BATTLE SPACE" && EndText(endPanel, "values").Contains("\nLOST\n"),
              "\"" + EndText(endPanel, "title") + "\" / \"" + EndText(endPanel, "reason") + "\" / \"" + EndText(endPanel, "values").Replace("\n", " / ") + "\"");
        Shot("30_ship_lost_edge");
        yield return 0.2f;

        //     RESTART again: SHIP LOST is gone (it outlasted nothing before, because nothing restarted).
        oldBoard = board;
        if (!Click("end.restart", null, "RESTART MISSION")) yield break;
        yield return (Func<bool>)(() => !oldBoard && UnityEngine.Object.FindAnyObjectByType<BoardManager>());
        yield return 1.5f;
        board = UnityEngine.Object.FindAnyObjectByType<BoardManager>();
        s = board.State;
        quest = UnityEngine.Object.FindAnyObjectByType<QuestSystem>();
        endPanel = board.GetComponentInChildren<EndScreenPanel>(true);
        freshBrain = GameObject.Find("World").GetComponentInChildren<EnemyBrain>();
        if (freshBrain) freshBrain.firesBack = false;
        if (!Click("pause", null, "PAUSE/RUN key")) yield break;
        float restartAt = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds > restartAt);
        var stripWords = GameObject.Find("Board").transform.Find("Tactical Board/AlarmStrip/Words").GetComponent<TMPro.TMP_Text>();
        Check("After RESTART, SHIP LOST is gone: not lost, no countdown, and the alarm strip does not say it, after a tick",
              !s.deserted && !s.destroyed && s.outsideFor < 0f && !(stripWords.gameObject.activeInHierarchy && stripWords.text.StartsWith("SHIP LOST")),
              "deserted " + s.deserted + ", destroyed " + s.destroyed + ", outside " + s.outsideFor + ", strip \"" + (stripWords.gameObject.activeInHierarchy ? stripWords.text : "") + "\"");

        //     Our reactor at 0 (stand-in for enemy fire): SHIP LOST, REACTOR DESTROYED.
        s.damage.Comp("rct").integ = 0; s.destroyed = true;
        yield return (Func<bool>)(() => quest.Over);
        yield return (Func<bool>)(() => endPanel.Showing);
        Check("Reactor at 0: SHIP LOST, REACTOR DESTROYED, our ship LOST",
              quest.FailReason == MissionFailReason.Destroyed && EndText(endPanel, "title") == "SHIP LOST" && EndText(endPanel, "reason") == "REACTOR DESTROYED"
              && EndText(endPanel, "values").Contains("\nLOST\n"),
              "\"" + EndText(endPanel, "title") + "\" / \"" + EndText(endPanel, "reason") + "\" / \"" + EndText(endPanel, "values").Replace("\n", " / ") + "\"");
        Shot("31_ship_lost_reactor");
        yield return 0.2f;

        //     MAIN MENU: the intro, then the menu.
        if (!Click("end.menu", null, "MAIN MENU")) yield break;
        yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "00_Start");
        Check("MAIN MENU loads the intro", SceneManager.GetActiveScene().name == "00_Start", SceneManager.GetActiveScene().name);
        yield return 1f;
    }

    // How close a slug fired from (px,py) on this bearing passes a target moving at (vx,vy), units.
    static double PassUnits(double px, double py, double aimDeg, double speed, double tx, double ty, double vx, double vy, double maxT)
    {
        double a = aimDeg * System.Math.PI / 180, wx = vx - speed * System.Math.Sin(a), wy = vy - speed * System.Math.Cos(a);
        double rx = tx - px, ry = ty - py, ww = wx * wx + wy * wy;
        double t = ww > 0 ? System.Math.Max(0, System.Math.Min(maxT, -(rx * wx + ry * wy) / ww)) : 0;
        return System.Math.Sqrt((rx + wx * t) * (rx + wx * t) + (ry + wy * t) * (ry + wy * t));
    }

    // How far a straight shot from our ship on this bearing passes from a contact, units (huge if it's behind us).
    static double MissUnits(ShipState s, ShipState.Track t, double aimDeg)
    {
        double a = aimDeg * System.Math.PI / 180, px = t.worldX - s.nav.x, py = t.worldY - s.nav.y;
        double along = px * System.Math.Sin(a) + py * System.Math.Cos(a);
        return along < 0 ? double.MaxValue : System.Math.Abs(px * System.Math.Cos(a) - py * System.Math.Sin(a));
    }

    // Where two bearing lines meet (world units; bearings clockwise from north). False if near parallel.
    static bool Cross(SensorSystem.BearingFix a, SensorSystem.BearingFix b, out double x, out double y)
    {
        double ra = a.brg * System.Math.PI / 180, rb = b.brg * System.Math.PI / 180;
        double ax = System.Math.Sin(ra), ay = System.Math.Cos(ra), bx = System.Math.Sin(rb), by = System.Math.Cos(rb);
        double den = ax * by - ay * bx;
        x = y = 0;
        if (System.Math.Abs(den) < 1e-9) return false;
        double u = ((b.x - a.x) * by - (b.y - a.y) * bx) / den;
        x = a.x + u * ax; y = a.y + u * ay;
        return true;
    }

    /// <summary>Press at board px a, release at board px b, on the plot, through the EventSystem.
    /// The plot's capture surface must be on top there.</summary>
    // The plot's scale, board px per 1,000 units at x1. Owner 2026-10-29: x1 now shows 50-unit squares, 12.5 times
    // closer than the x1 most checks were written at (60.9 px per 1,000); X1 is the zoom that shows what that x1 did.
    static float PlotScale => UnityEngine.Object.FindAnyObjectByType<PlotGrid>().PxPer1000Units;
    static double X1 => 60.9 / PlotScale;

    static bool PlotDrag(BoardManager board, Vector2 a, Vector2 b, string label, string expect = "plot")
    {
        var es = EventSystem.current;
        Vector2 sa = BoardToScreen(board, a), sb = BoardToScreen(board, b);
        var ped = new PointerEventData(es) { position = sa, pressPosition = sa, button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>();
        es.RaycastAll(ped, hits);
        var top = hits.Count > 0 ? ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject) : null;
        var hh = top ? top.GetComponent<ClickSpot>() : null;
        if (hh == null || hh.action != expect)
        {
            Check(label, false, "blocked: topmost at board " + a + " is " + (hits.Count > 0 ? hits[0].gameObject.name : "nothing"));
            return false;
        }
        ped.pointerCurrentRaycast = ped.pointerPressRaycast = hits[0];
        ExecuteEvents.Execute(top, ped, ExecuteEvents.pointerDownHandler);
        ped.position = sb; // released without a drag event: ClickSpot sends the release point as the last drag
        ExecuteEvents.Execute(top, ped, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(top, ped, ExecuteEvents.pointerClickHandler);
        return true;
    }

    static Vector2 BoardToScreen(BoardManager board, Vector2 bp)
    {
        var rt = (RectTransform)board.transform;
        var local = new Vector3(bp.x - rt.rect.width / 2f, rt.rect.height / 2f - bp.y, 0f);
        var cam = board.GetComponentInParent<Canvas>().rootCanvas.worldCamera;
        return RectTransformUtility.WorldToScreenPoint(cam, rt.TransformPoint(local));
    }

    static TMPro.TMP_Text MarkLabelText(BoardManager board, int i)
    {
        var f = typeof(PlotPanel).GetField("labels", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var list = f.GetValue(board.GetComponentInChildren<PlotPanel>(true)) as List<TMPro.TMP_Text>;
        return list != null && i < list.Count ? list[i] : null;
    }
    // Step 86: the axis shows 0 (level with our ship), and every number is a whole number of big squares.
    static bool AxisWhole(List<string> shown, double bigUnits)
    {
        if (!shown.Contains("0")) return false;
        foreach (var x in shown)
        {
            double v;
            if (!double.TryParse(x.Replace(",", ""), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v) || System.Math.Abs(v % bigUnits) > 1e-6) return false;
        }
        return true;
    }

    static string EndText(EndScreenPanel p, string field)
    {
        var t = typeof(EndScreenPanel).GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(p) as TMPro.TMP_Text;
        return t ? t.text : "?";
    }
    static string MarkLabel(BoardManager board, int i) { var t = MarkLabelText(board, i); return t ? t.text : "?"; }
    static Vector2 MarkLabelPos(BoardManager board, int i) { var t = MarkLabelText(board, i); return t ? t.rectTransform.anchoredPosition : Vector2.zero; }

    // ---- helpers ----------------------------------------------------------------------------

    static bool BusesAre(ShipState s, PowerSystem.BusMode mode, int fill)
    {
        foreach (var b in s.power.buses) if (b.mode != mode || b.fill != fill) return false;
        return true;
    }

    static bool BusesAreMode(ShipState s, PowerSystem.BusMode mode)
    {
        foreach (var b in s.power.buses) if (b.mode != mode) return false;
        return true;
    }

    static string SigNote(BoardManager board)
    {
        var panel = board.GetComponentInChildren<SensorsPanel>(true);
        var f = typeof(SensorsPanel).GetField("sigNote", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var t = panel && f != null ? f.GetValue(panel) as TMPro.TMP_Text : null;
        return t ? t.text : "?";
    }

    static bool AllFull(ShipState s)
    {
        foreach (var b in s.power.buses) if (b.fill < s.power.stats.busMax) return false;
        return true;
    }

    static string Buses(ShipState s)
    {
        var parts = new List<string>();
        foreach (var b in s.power.buses) parts.Add(b.key.ToUpperInvariant() + " " + b.mode.ToString().ToUpperInvariant() + " " + b.fill);
        return string.Join(", ", parts);
    }

    static ClickSpot Hit(string action, string arg)
    {
        foreach (var h in UnityEngine.Object.FindObjectsByType<ClickSpot>(FindObjectsSortMode.None))
            if (h.action == action && (h.arg ?? "") == (arg ?? "") && h.isActiveAndEnabled) return h;
        return null;
    }

    static string FirstChildName(Transform parent, string prefix)
    {
        foreach (Transform c in parent) if (c.name.StartsWith(prefix)) return c.name;
        return prefix;
    }

    /// <summary>Click a board control where it is on screen, through the EventSystem. Fails if
    /// the control can't be found or something else is on top of it. down: false or up: false
    /// sends only the release (up and click) or only the press, to hold a key.</summary>
    static bool Click(string action, string arg, string label, bool down = true, bool up = true)
    {
        // A control can have several click targets (a rail key and its drawer's latch, which may still be
        // sliding away). Use the first one that is actually on top at its own centre.
        string why = "control not found";
        foreach (var h in UnityEngine.Object.FindObjectsByType<ClickSpot>(FindObjectsSortMode.None))
        {
            if (h.action != action || (h.arg ?? "") != (arg ?? "") || !h.isActiveAndEnabled) continue; // Unity saves a null arg as ""
            var rt = (RectTransform)h.transform;
            var canvas = h.GetComponentInParent<Canvas>().rootCanvas;
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rt.TransformPoint(rt.rect.center));
            var es = EventSystem.current;
            var ped = new PointerEventData(es) { position = screen, pressPosition = screen, button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>();
            es.RaycastAll(ped, hits);
            GameObject top = hits.Count > 0 ? hits[0].gameObject : null;
            var handler = top ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(top) : null;
            if (handler != h.gameObject) { why = "blocked: topmost at " + screen + " is " + (top ? top.name : "nothing"); continue; }
            ped.pointerCurrentRaycast = ped.pointerPressRaycast = hits[0];
            if (down) ExecuteEvents.Execute(handler, ped, ExecuteEvents.pointerDownHandler);
            if (up)
            {
                ExecuteEvents.Execute(handler, ped, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(handler, ped, ExecuteEvents.pointerClickHandler);
            }
            return true;
        }
        Check("Click " + label, false, why);
        return false;
    }

    static void Check(string step, bool ok, string detail)
    {
        if (ok) passed++; else failed++;
        report.AppendLine("| " + (passed + failed) + " | " + step + " | " + (ok ? "PASS" : "**FAIL**") + " | " + detail.Replace("|", "/") + " |");
        if (!ok) Debug.LogWarning("[Playtest] FAIL: " + step + " — " + detail);
    }

    static void Shot(string name)
    {
        ScreenCapture.CaptureScreenshot(Path.Combine(runDir, "shots", name + ".png"));
    }
}
