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
/// Unity Recorder. Output: Logs/Playtests/&lt;time&gt;/ — playtest.mp4, report.md, shots/*.png. Only the
/// newest KeepRunsWithMedia runs keep their video and shots; every report stays.
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

    // The recorder switches the Game view to its own 1920x1080 size and leaves it there; give the owner
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

    // Each run's video and screenshots are about 140 MB; keep them for the newest few runs only
    // (owner, 2026-10-24). Every report.md stays: BUILD_LOG cites runs as evidence.
    const int KeepRunsWithMedia = 3;

    static void PruneOldRuns(string playtests)
    {
        if (!Directory.Exists(playtests)) return;
        var runs = Directory.GetDirectories(playtests);
        Array.Sort(runs, StringComparer.Ordinal); // folder names are timestamps, so oldest first
        for (int i = 0; i < runs.Length - (KeepRunsWithMedia - 1); i++) // this run makes the last one
        {
            try
            {
                File.Delete(Path.Combine(runs[i], "playtest.mp4"));
                string shots = Path.Combine(runs[i], "shots");
                if (Directory.Exists(shots)) Directory.Delete(shots, true);
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
        movie.ImageInputSettings = new GameViewInputSettings { OutputWidth = 1920, OutputHeight = 1080 };
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

        // 3. Briefing → board (Enter; old Input class — call what Enter calls)
        var flow = UnityEngine.Object.FindAnyObjectByType<ScreenFlow>();
        if (flow) flow.Advance();
        yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "02.1_Grid");
        Check("Enter on the briefing opens the board", SceneManager.GetActiveScene().name == "02.1_Grid", "called ScreenFlow.Advance, as Enter does");
        yield return 1.5f;
        var board = UnityEngine.Object.FindAnyObjectByType<HandoffBoardControls>();
        Check("The board is running", board != null, "");
        if (board == null) yield break;
        var s = board.State;
        Shot("03_board");
        yield return 0.2f;

        // 4. Starting state: undamaged, dummy present, power cold
        bool undamaged = true;
        foreach (var c in s.compartments) if (c.integ != 5 || c.vented || !string.IsNullOrEmpty(c.cond)) undamaged = false;
        Check("Loads undamaged (all rooms green and sealed)", undamaged, "");
        Check("The dummy enemy is the only contact", s.tracks.Length == 1 && s.tracks[0].key == "t07",
              s.tracks.Length > 0 ? s.tracks[0].name + " at " + s.tracks[0].bearing.ToString("000") + "°, " + s.tracks[0].rangeKm.ToString("N0") + " km" : "none");
        Check("Power starts cold (every bus empty, on STANDBY)", BusesAre(s, HandoffBoardState.BusMode.Standby, 0), Buses(s));

        // 5. Paused at load; signal silent
        Check("The board starts paused", board.Paused, "");
        Check("Signal starts at 0 — RUNNING SILENT", s.signal == 0f && SigNote(board) == "RUNNING SILENT", s.signal.ToString("0.0") + ", \"" + SigNote(board) + "\"");

        // 6. Open the Power drawer from the systems rail
        int snd = board.Sounds;
        if (!Click("drawer", "power", "PWR key")) yield break;
        Check("A drawer key sounds the tab click (Step 40)", board.Sounds == snd + 1 && board.LastSound == "tab", (board.Sounds - snd) + " cue(s), last \"" + board.LastSound + "\"");
        yield return 0.8f;
        var drawer = GameObject.Find("State · power");
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
            Check(names[i] + " bus set to DRAW", s.buses[i].mode == HandoffBoardState.BusMode.Draw, s.buses[i].mode.ToString());
            yield return 0.3f;
        }
        Check("Each selector sounds the switch click", board.Sounds == snd + 4 && board.LastSound == "switch", (board.Sounds - snd) + " cue(s), last \"" + board.LastSound + "\"");
        yield return 1.5f;
        Check("While paused, the buses do not fill", s.TotalFill == 0, "total fill " + s.TotalFill + " after 1.5 s paused");
        Shot("05_all_draw");
        yield return 0.2f;

        // 8. RUN
        if (!Click("pause", null, "PAUSE/RUN key")) yield break;
        Check("RUN starts time", !board.Paused, "");

        // 9. Let the reactor fill the buses
        yield return (Func<bool>)(() => s.TotalFill >= s.ReactorCap || AllFull(s));
        yield return 1f;
        Check("The reactor fills the buses up to its output", s.TotalFill == Mathf.Min(s.ReactorCap, s.busMax * 4), "total " + s.TotalFill + " of " + s.ReactorCap + " — " + Buses(s));
        Shot("06_powered");
        yield return 0.2f;

        //     TRACK 07 patrols (Step 51): its brain orders a heading and a throttle; its own ship model
        //     moves it with the same maths as ours.
        var brain = GameObject.Find("World").GetComponentInChildren<EnemyBrain>();
        if (brain) brain.firesBack = false; // stand-in: it holds its fire until the point-defence step (21e)
        // stand-in: no hit starts a condition until the conditions step (24), so the steps before stay as written
        float[] condChances = { s.fireChanceHE, s.fireChancePEN, s.breachChanceSlug, s.radChanceReactor };
        s.fireChanceHE = s.fireChancePEN = s.breachChanceSlug = s.radChanceReactor = 0f;
        var t07Ship = brain ? brain.GetComponent<WorldContact>().Ship : null;
        Check("TRACK 07 has a patrol brain", brain && brain.enabled, "");
        if (!brain) yield break;
        double ex0 = t07Ship.ownX, ey0 = t07Ship.ownY;
        s.thr = 0; // stand-in: our helm at zero throttle, so our own ship holds still while TRACK 07 moves
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return 3f;
        if (!Click("rate", "1", "x1 rate key")) yield break;
        s.vx = s.vy = 0;
        double movedKm = System.Math.Sqrt((t07Ship.ownX - ex0) * (t07Ship.ownX - ex0) + (t07Ship.ownY - ey0) * (t07Ship.ownY - ey0));
        var wp = brain.Waypoint;
        int toWp = TacticalWorld.Bearing(new Vector2((float)(wp.x - t07Ship.ownX), (float)(wp.y - t07Ship.ownY)));
        Check("TRACK 07 gets under way on its patrol, toward its next waypoint", movedKm > 1 && t07Ship.SpeedKms > 0.05 && System.Math.Abs(((t07Ship.ordHdg - toWp + 540) % 360) - 180) <= 1,
              movedKm.ToString("0") + " km, " + t07Ship.SpeedKms.ToString("0.00") + " km/s, ordered " + t07Ship.ordHdg.ToString("000") + "°, heading " + t07Ship.hdg.ToString("000") + "°");
        var dummyT = brain.transform;
        Check("Its 3D model follows it", Mathf.Abs(dummyT.position.x - (float)t07Ship.ownX) < 0.05f && Mathf.Abs(dummyT.position.y - (float)t07Ship.ownY) < 0.05f, "");
        Check("The board's track reads its real speed", Mathf.Abs(s.TrackOf("t07").speed - (float)t07Ship.SpeedKms) < 0.01f, s.TrackOf("t07").speed.ToString("0.00") + " km/s");
        //     Stand-in: hold it still for the static-target checks below, until leading lands (Step 52).
        brain.enabled = false;
        t07Ship.vx = t07Ship.vy = 0;

        // 10. What the power now drives (board rules)
        Check("ENG power raises the throttle limit", s.ThrCap > 0, "throttle cap " + s.ThrCap + "%");
        Check("SEN power raises the sensor gain limit", s.GainCap > 1, "gain limit " + s.GainCap + " (range is " + s.SenRangeKm.ToString("N0") + " km until the gain is turned up — sensors feature)");
        float wep0 = s.wepCharge; double aux0 = s.auxCharge;
        yield return 2f;
        Check("WEP power charges the capacitor", s.wepCharge > wep0 || s.wepCharge >= 100f, wep0.ToString("0") + " → " + s.wepCharge.ToString("0") + "%");
        Check("AUX power charges the order pool", s.auxCharge > aux0 || s.auxCharge >= 100, aux0.ToString("0") + " → " + s.auxCharge.ToString("0") + "%");

        // 11. Power makes us loud (Step 35): signal climbs to its target on the OWN EMISSIONS meter
        if (!Click("drawer", "sensors", "SENS key")) yield break;
        yield return 0.8f;
        float target = s.SignalTarget;
        Check("Power sets the signal target", Mathf.Approximately(target, Mathf.Min(100f, s.TotalFill * 10f)), "target " + target.ToString("0.0") + " from " + s.TotalFill + " units");
        Shot("07_signal_climbing");
        yield return 0.2f;
        yield return (Func<bool>)(() => s.signal >= target);
        Check("The signal climbs to its target", s.signal >= target, s.signal.ToString("0.0") + ", \"" + SigNote(board) + "\"");
        Check("Loud enough to be tracked", s.signal < s.signalTrackedAt || SigNote(board).Contains("YOUR RANGE"), "\"" + SigNote(board) + "\"");
        Shot("08_signal_loud");
        yield return 0.2f;

        // 12. LOAD SHED: every bus vents, the signal falls back to silent
        if (!Click("drawer", "power", "PWR key")) yield break;
        yield return 0.8f;
        var rockers = board.GetComponent<HandoffRockers>();
        var shed = Hit("power.shed", null).GetComponent<Image>();
        var shed0 = shed.sprite;
        if (!Click("power.shed", null, "EMERGENCY LOAD SHED")) yield break;
        Check("LOAD SHED vents every bus", BusesAreMode(s, HandoffBoardState.BusMode.Vent), Buses(s));
        bool slid = false;
        float until = Time.realtimeSinceStartup + 0.6f;
        yield return (Func<bool>)(() => (slid |= rockers.Moving("power.shed", null)) || Time.realtimeSinceStartup > until);
        yield return (Func<bool>)(() => !rockers.Moving("power.shed", null));
        Check("The LOAD SHED pad slides down (Step 40)", slid && shed.sprite != shed0 && shed.sprite.name.EndsWith("_thrown") && !shed.transform.Find("Pad cover").GetComponent<Image>().enabled,
              shed0.name + " → " + shed.sprite.name + (slid ? ", slide seen, now at rest" : ", no slide seen"));
        yield return (Func<bool>)(() => s.TotalFill == 0);
        if (!Click("drawer", "sensors", "SENS key")) yield break;
        yield return (Func<bool>)(() => s.signal <= 0f);
        Check("With no power out, the signal falls to silent", s.signal <= 0f && SigNote(board) == "RUNNING SILENT", s.signal.ToString("0.0") + ", \"" + SigNote(board) + "\"");
        Shot("09_silent");
        yield return 0.2f;

        // 13. Plot tools (Step 36): POINT is a timestamp, RULER a distance, COMPASS around our ship.
        //     Marks stay put in space when our ship moves.
        if (!Click("drawer", "sensors", "SENS key (close the drawer)")) yield break;
        yield return 0.6f;
        board.SetZoom(1.0);
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
        Check("RULER reads a distance", ru0.StartsWith("RNG "), "\"" + ru0 + "\"");
        Check("COMPASS reads range and bearing from our ship", co0.StartsWith("RNG "), "\"" + co0 + "\"");
        Shot("10_marks");
        yield return 0.2f;

        //     Put our ship 5,000 km east (the board owns its position since Step 41). The grid stays
        //     put and the ship moves across it, so marks keep their place on screen.
        var own = GameObject.Find("World").transform.Find("OwnShip");
        var shipImg = (RectTransform)GameObject.Find("Own ship (3D view)").transform;
        double home = s.ownX;
        Vector2 ship0 = shipImg.anchoredPosition;
        s.ownX = home + 5000.0;
        double shift = 5000.0 * 60.9 / 1000.0 * board.ZoomExact; // px our ship travels on the plot
        yield return 1.2f; // a few board ticks
        string pt1 = MarkLabel(board, 0), ru1 = MarkLabel(board, 1), co1 = MarkLabel(board, 2);
        Vector2 ptPos1 = MarkLabelPos(board, 0), ruPos1 = MarkLabelPos(board, 1);
        Check("The 3D model follows the board's position", Mathf.Abs(own.position.x - (float)(home + 5000.0)) < 0.01f, "model x " + own.position.x.ToString("0") + " km");
        Check("Our ship moves across the plot", Mathf.Abs(shipImg.anchoredPosition.x - ship0.x - (float)shift) < 0.5f && Mathf.Abs(shipImg.anchoredPosition.y - ship0.y) < 0.5f,
              "moved " + (shipImg.anchoredPosition.x - ship0.x).ToString("0.0") + " px (expected " + shift.ToString("0.0") + ")");
        Check("The POINT stays put", (ptPos1 - ptPos0).magnitude < 0.01f && pt1 == pt0, "moved " + (ptPos1 - ptPos0).magnitude.ToString("0.00") + " px, label unchanged");
        Check("The RULER stays put", (ruPos1 - ruPos0).magnitude < 0.01f && ru1 == ru0, "moved " + (ruPos1 - ruPos0).magnitude.ToString("0.00") + " px, still \"" + ru1 + "\"");
        Check("The COMPASS re-measures from our ship", co1 != co0 && co1.StartsWith("RNG "), "\"" + co0 + "\" → \"" + co1 + "\"");
        Shot("11_ship_moved");
        yield return 0.2f;
        s.ownX = home;
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
        Check("A locked rocker refuses: the pad nudges and the refused cue sounds", !s.sw[0] && nudged && board.LastSound == "refused" && board.Sounds == snd + 1,
              "thrown " + s.sw[0] + ", nudge " + (nudged ? "seen" : "not seen") + ", last cue \"" + board.LastSound + "\"");
        yield return (Func<bool>)(() => !rockers.Moving("fc.flip", "0"));
        Check("After the nudge the rocker is back at rest, still locked", rung.sprite.name.EndsWith("_locked") && !rung.transform.Find("Pad cover").GetComponent<Image>().enabled, rung.sprite.name);
        Shot("13_refused");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action

        var track = (RectTransform)Hit("fc.track", null).transform;
        var trackLamp = (RectTransform)track.parent.Find(FirstChildName(track.parent, "lamp#82 "));
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
        Check("ENG back on DRAW after the shed", !s.shed && s.buses[0].mode == HandoffBoardState.BusMode.Draw, Buses(s));
        yield return (Func<bool>)(() => s.ThrCap >= 60);
        if (!Click("rate", "10", "x10 rate key")) yield break;
        if (!Click("helm", null, "HELM header")) yield break;
        yield return 0.8f;
        if (!Click("helm.thr", null, "throttle (press mid-well)")) yield break;
        Check("The throttle takes a setting", s.thr > 0, "throttle " + s.thr + "%, target " + (s.thr / 100.0 * s.topSpeedKms).ToString("0.00") + " km/s");
        double x0 = s.ownX, y0 = s.ownY;
        Vector2 img0 = shipImg.anchoredPosition, grid0 = board.GetComponentInChildren<HandoffPlotGrid>().OffsetPx;
        yield return 4f;
        double moved = System.Math.Sqrt((s.ownX - x0) * (s.ownX - x0) + (s.ownY - y0) * (s.ownY - y0));
        Check("The ship gathers speed", s.SpeedKms > 0.01, s.SpeedKms.ToString("0.000") + " km/s after 4 s at x10");
        Check("It moves along its heading", moved > 0 && System.Math.Abs(((s.CourseDeg - s.hdg + 540) % 360) - 180) < 2,
              moved.ToString("0.0") + " km, course " + s.CourseDeg.ToString("000") + "°, heading " + s.hdg.ToString("000") + "°");
        Check("Our ship's image moves; the grid stays put", shipImg.anchoredPosition != img0 && board.GetComponentInChildren<HandoffPlotGrid>().OffsetPx == grid0, "");
        Shot("16_under_way");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action

        //     A hard turn: the hull swings, the ship slides on its old course, then lines up.
        s.ordHdg = (s.hdg + 90) % 360; // stand-in: set the ordered heading (the helm dial is a drag)
        yield return (Func<bool>)(() => s.hdg == s.ordHdg);
        double lag = System.Math.Abs(((s.CourseDeg - s.hdg + 540) % 360) - 180);
        Check("After a turn the ship slides: course lags the heading", lag > 5, "heading " + s.hdg.ToString("000") + "°, course " + s.CourseDeg.ToString("000") + "°");
        Shot("17_sliding");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action

        //     Pause freezes the ship.
        if (!Click("pause", null, "PAUSE key")) yield break;
        double px = s.ownX, py = s.ownY;
        yield return 2f;
        Check("Paused, the ship stops where it is", s.ownX == px && s.ownY == py, "");
        if (!Click("pause", null, "RUN key")) yield break;

        //     Pan the plot: drag the empty grid; marks, contacts, the ship and the grid all move together.
        if (!Click("helm", null, "HELM header (close)")) yield break;
        yield return 0.6f;
        if (!Click("tool", "CURSOR", "CURSOR tool")) yield break;
        var plotPanel = board.GetComponent<HandoffPlotPanel>();
        Vector2 v0 = plotPanel.View, gridA = board.GetComponentInChildren<HandoffPlotGrid>().OffsetPx;
        if (!PlotDrag(board, new Vector2(700, 300), new Vector2(800, 300), "drag the grid 100 px right", "plot.pan")) yield break;
        float dragKm = (float)(100 / (60.9 / 1000.0 * board.ZoomExact));
        Check("Dragging the grid pans the plot", Mathf.Abs(plotPanel.View.x - (v0.x - dragKm)) < 1f && Mathf.Abs(board.GetComponentInChildren<HandoffPlotGrid>().OffsetPx.x - gridA.x - 100f) < 0.5f,
              "view moved " + (plotPanel.View.x - v0.x).ToString("0") + " km (expected " + (-dragKm).ToString("0") + ")");
        Shot("18_panned");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("rate", "1", "x1 rate key")) yield break;

        // 16. The sensor circle (Step 42): while DRADIS is on, a circle round our ship at the hearing range.
        Check("DRADIS on: the sensor circle shows at the hearing range", s.dradis && Mathf.Abs(plotPanel.SensorRingKm - s.PassiveRangeKm) < 1f,
              plotPanel.SensorRingKm.ToString("N0") + " km (hearing range " + s.PassiveRangeKm.ToString("N0") + " km)");
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
        board.SetZoom(0.3); // 19,000 km is wider than the plot at x1; zoom out to see the whole circle
        yield return 0.8f;
        Shot("19_sensor_circle");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("sensors.dradis", null, "DRADIS rocker (off)")) yield break;
        yield return 0.3f;
        Check("DRADIS off: the circle and the sweep go", !s.dradis && plotPanel.SensorRingKm == 0f && !plotPanel.SweepShown, "dradis " + s.dradis);
        Shot("20_dradis_off");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("sensors.dradis", null, "DRADIS rocker (on)")) yield break;
        yield return 0.8f;
        Check("DRADIS on again: the circle is back", s.dradis && Mathf.Abs(plotPanel.SensorRingKm - s.PassiveRangeKm) < 1f, plotPanel.SensorRingKm.ToString("N0") + " km");
        Shot("21_dradis_on_again");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        board.SetZoom(1.0);
        yield return 0.5f;

        // 17. Hearing (Step 44): power SEN, raise the gain, and TRACK 07 comes into hearing range.
        var alarmWords = GameObject.Find("Board").transform.Find("HandoffBoard/AlarmStrip/" + FirstChildName(GameObject.Find("Board").transform.Find("HandoffBoard/AlarmStrip"), "text#14 ")).GetComponent<TMPro.TMP_Text>();
        Check("Nothing heard yet: the alarm strip is quiet", s.newContacts == 0 && !alarmWords.gameObject.activeInHierarchy, s.newContacts + " new");
        if (!Click("drawer", "power", "PWR key")) yield break;
        yield return 0.8f;
        if (!Click("power.bus", "2", "SEN selector")) yield break;
        yield return (Func<bool>)(() => s.GainCap >= 2);
        s.senBoost = 2; // stand-in: the gain fader is a drag
        var t07 = s.TrackOf("t07");
        yield return (Func<bool>)(() => s.Heard(t07) && s.newContacts > 0);
        Check("At gain 2 DRADIS hears TRACK 07", s.Heard(t07), "range " + t07.rangeKm.ToString("N0") + " km, hearing range " + s.PassiveRangeKm.ToString("N0") + " km");
        var pl = GameObject.Find("Passive label 0");
        string plText = pl ? pl.GetComponent<TMPro.TMP_Text>().text : "none";
        Check("A bearing line and label point at it", pl && plText == "BRG " + t07.bearing.ToString("000"), "\"" + plText + "\"");
        Check("The alarm strip raises: 1 NEW CONTACT, with the alert tone", s.newContacts == 1 && alarmWords.gameObject.activeInHierarchy && alarmWords.text == "NEW CONTACT SINCE COMMIT",
              "\"" + alarmWords.text + "\", last cue \"" + board.LastSound + "\"");
        Shot("22_heard");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("ack", null, "alarm strip (acknowledge)")) yield break;
        Check("Clicking the strip acknowledges it", s.newContacts == 0 && !alarmWords.gameObject.activeInHierarchy, "");

        //     Loud enough to be tracked: our ship blinks (bd-blink).
        yield return (Func<bool>)(() => s.signal >= s.signalTrackedAt);
        var shipRaw = GameObject.Find("Own ship (3D view)").GetComponent<UnityEngine.UI.RawImage>();
        bool bright = false, dim = false;
        float blinkUntil = Time.realtimeSinceStartup + 1.3f;
        yield return (Func<bool>)(() => { if (shipRaw.color.a > 0.9f) bright = true; else if (shipRaw.color.a < 0.3f) dim = true; return Time.realtimeSinceStartup > blinkUntil; });
        Check("Tracked, our ship blinks", bright && dim, "signal " + s.signal.ToString("0") + "%");

        // 18. The bearing trail (Step 45): each sweep leaves TRACK 07's bearing where we were. As we
        //     move, the lines cross at it. They fade out over two minutes of mission time.
        if (!Click("rate", "10", "x10 rate key")) yield break;
        s.BearingFixes.Clear();
        yield return (Func<bool>)(() => s.BearingFixes.FindAll(f => f.key == "t07").Count >= 2);
        var fixes = s.BearingFixes.FindAll(f => f.key == "t07");
        var fa = fixes[0]; var fb = fixes[fixes.Count - 1];
        double baseKm = System.Math.Sqrt((fb.x - fa.x) * (fb.x - fa.x) + (fb.y - fa.y) * (fb.y - fa.y));
        double crossX, crossY;
        bool crosses = Cross(fa, fb, out crossX, out crossY);
        double missKm = crosses ? System.Math.Sqrt((crossX - t07.worldX) * (crossX - t07.worldX) + (crossY - t07.worldY) * (crossY - t07.worldY)) : -1;
        Check("Two bearing lines from different spots cross at TRACK 07", crosses && missKm < 0.01 * t07.rangeKm,
              fixes.Count + " lines, " + baseKm.ToString("N0") + " km apart; they cross " + missKm.ToString("N1") + " km from the target");
        Shot("23_bearing_trail");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        float firstLaid = fa.t;
        yield return (Func<bool>)(() => board.MissionSeconds - firstLaid > s.bearingTrailSeconds + 2f);
        yield return 0.5f;
        Check("A bearing line is gone after two minutes", !s.BearingFixes.Contains(fa), (board.MissionSeconds - firstLaid).ToString("0") + " s after it was laid");
        if (!Click("rate", "1", "x1 rate key")) yield break;

        // 19. Intercept helpers (Step 46). Our ship is under way.
        if (!Click("drawer", "power", "PWR key (close the drawer)")) yield break;
        yield return 0.6f;
        s.marks.Clear();
        if (!Click("tool", "RULER", "RULER tool")) yield break;
        if (!PlotDrag(board, new Vector2(500, 300), new Vector2(620, 300), "draw a RULER")) yield break;
        yield return 0.3f;
        string timed = MarkLabel(board, 0);
        Check("A RULER shows the time to cover it at our speed", timed.Contains(" MIN") || timed.EndsWith(" S"), "\"" + timed + "\" at " + s.SpeedKms.ToString("0.00") + " km/s");

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
        Check("No weapon ring with Fire Control shut", plotPanel.WeaponRingKm == 0f, "");
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.6f;
        Check("Fire Control open: the railgun's reach shows round our ship, as far as the DRADIS at full gain", plotPanel.WeaponRingKm == s.senRangeBaseKm + 5 * s.senRangePerStepKm, plotPanel.WeaponRingKm.ToString("N0") + " km");
        board.SetZoom(0.25);
        yield return 0.4f;
        Shot("25_weapon_ring");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        board.SetZoom(1.0);
        if (!Click("drawer", "fire control", "FIRE key (close)")) yield break;
        yield return 0.4f;
        Check("Fire Control shut: the ring goes", plotPanel.WeaponRingKm == 0f, "");
        if (!Click("tool", "ERASE", "ERASE tool")) yield break;
        if (!Click("tool", "ERASE", "ERASE tool (again)")) yield break;

        // 20. A firing solution dialled by hand (Step 47). TRACK 07 is heard, not identified.
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.6f;
        Check("No firing solution yet", !s.Sol.set, "");
        if (!Click("fc.track", null, "TRACK (contact not identified)")) yield break;
        Check("TRACK refuses a contact that is only heard", !s.Sol.set, "");
        int expBrg = (HandoffBoardState.JsRound(s.scopeBrg) - s.hdg + 540) % 360 - 180;
        var rngKnob = Hit("fc.knob", "rng");
        Vector2 knobAt = board.BoardPoint(RectTransformUtility.WorldToScreenPoint(rngKnob.GetComponentInParent<Canvas>().rootCanvas.worldCamera,
                                         rngKnob.transform.TransformPoint(((RectTransform)rngKnob.transform).rect.center)));
        if (!PlotDrag(board, knobAt, knobAt + new Vector2(80, 0), "turn the RNG knob (10 steps)", "fc.knob")) yield break;
        yield return 0.3f;
        Check("The RNG knob starts a hand-dialled solution on the periscope's bearing",
              s.Sol.set && s.Sol.manual && s.Sol.brg == expBrg && s.Sol.spd == 0f && Mathf.Abs(s.Sol.rangeKm - (s.manualStartRangeKm + 1000f)) < 0.01f,
              "RNG " + s.Sol.rangeKm.ToString("N0") + " km, REL BRG " + s.Sol.brg + "°, SPD " + s.Sol.spd.ToString("0.00"));
        var fc = board.GetComponent<HandoffFireControlPanel>();
        var tidField = typeof(HandoffFireControlPanel).GetField("trackId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var tidText = tidField != null ? tidField.GetValue(fc) as TMPro.TMP_Text : null;
        Check("The readout says MANUAL", tidText && tidText.text == "MANUAL", tidText ? "\"" + tidText.text + "\"" : "no readout");
        //     BEARING follows the dialled bearing (a point in space), not a locked contact.
        double dialled = ((s.hdg + s.Sol.brg) % 360 + 360) % 360;
        s.scopeBrg = (dialled + 10) % 360;
        bool offOk = !s.OnBearing;
        s.SlewTo();
        Check("BEARING wants the periscope on the dialled bearing; SLEW TO puts it there", offOk && s.OnBearing && System.Math.Abs(s.scopeBrg - dialled) < 0.01,
              "dialled " + dialled.ToString("000") + "°, periscope " + s.scopeBrg.ToString("000") + "°");
        Shot("26_manual_solution");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("drawer", "fire control", "FIRE key (close)")) yield break;
        yield return 0.5f;

        // 21. Fire and hit (Step 48): aim at TRACK 07 by hand, arm, fire a slug, watch it land.
        var dummyShip = GameObject.Find("World").GetComponentInChildren<WorldContact>().Ship;
        bool whole = true;
        foreach (var c in dummyShip.compartments) if (c.integ != 5) whole = false;
        foreach (var a in dummyShip.SideArmour) if (a <= 0f) whole = false;
        Check("TRACK 07 starts undamaged", whole, "");
        //     The periscope onto the contact, by its bearing label (it is still heard).
        if (!Click("plot.look", "t07", "BRG label (periscope onto TRACK 07)")) yield break;
        yield return 0.6f;
        //     Weapons released: CONDITION 2 from the orders station.
        if (!Click("orders", null, "ORDERS header")) yield break;
        yield return 0.6f;
        if (!Click("orders.alert", null, "CONDITION dial")) yield break;
        Check("CONDITION 2: weapons released", s.WeaponsReleased, "alert " + s.alert);
        if (!Click("orders", null, "ORDERS header (close)")) yield break;
        yield return 0.6f;
        //     Power for the guns: SEN gives its units back, WEP draws and charges.
        if (!Click("drawer", "power", "PWR key")) yield break;
        yield return 0.8f;
        if (!Click("power.bus", "2", "SEN selector (STANDBY)")) yield break;
        if (!Click("power.bus", "2", "SEN selector (VENT)")) yield break;
        if (!Click("power.bus", "1", "WEP selector (DRAW)")) yield break;
        yield return (Func<bool>)(() => s.WepReady && s.wepCharge >= s.ShotCost);
        Check("WEP charged for a shot", s.WepReady && s.wepCharge >= s.ShotCost, s.wepCharge.ToString("0") + "% of " + s.ShotCost + "%");
        //     A fresh hand-dialled solution on the periscope's bearing (one RNG step starts it).
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.6f;
        s.sol[s.SysIndex] = new HandoffBoardState.Solution(); // stand-in: clear the old solution (the board has no clear)
        if (!PlotDrag(board, knobAt, knobAt + new Vector2(8, 0), "turn the RNG knob (1 step)", "fc.knob")) yield break;
        yield return 0.3f;
        //     Whole-degree aim: dial the degree nearest the target (half a degree is ~213 km out here).
        var brgKnob = Hit("fc.knob", "brg");
        Vector2 brgAt = board.BoardPoint(RectTransformUtility.WorldToScreenPoint(brgKnob.GetComponentInParent<Canvas>().rootCanvas.worldCamera,
                                         brgKnob.transform.TransformPoint(((RectTransform)brgKnob.transform).rect.center)));
        int best = 0;
        for (int d = -1; d <= 1; d++) if (MissKm(s, t07, s.hdg + s.Sol.brg + d) < MissKm(s, t07, s.hdg + s.Sol.brg + best)) best = d;
        if (best != 0 && !PlotDrag(board, brgAt, brgAt + new Vector2(6 * best, 0), "turn the BRG knob to the nearest degree", "fc.knob")) yield break;
        s.SlewTo(); // stand-in for the periscope's SLEW TO key: onto the dialled bearing
        double predicted = MissKm(s, t07, s.hdg + s.Sol.brg);
        bool expectHit = predicted <= s.HitRadiusKm(t07.klass);
        Check("Turret A can't bear on it: ANGLE is out of arc", s.mount == "RGA" && !s.InArc, "REL BRG " + s.Sol.brg + "°, heading " + s.hdg + "°");
        if (!Click("fc.mount", null, "turret selector (RG-B)")) yield break;
        Check("Turret B bears", s.mount == "RGB" && s.InArc, "");
        for (int i = 0; i < 4; i++) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1))) yield break; yield return 0.2f; }
        Check("The ladder is set: LOAD, ANGLE, BEARING, ARM", s.AllSet, string.Join(",", System.Array.ConvertAll(s.sw, b => b ? "1" : "0")));
        //     Fire at x10.
        if (!Click("rate", "10", "x10 rate key")) yield break;
        var lever = Hit("fc.lever", null);
        Vector2 leverAt = board.BoardPoint(RectTransformUtility.WorldToScreenPoint(lever.GetComponentInParent<Canvas>().rootCanvas.worldCamera,
                                           lever.transform.TransformPoint(((RectTransform)lever.transform).rect.center)));
        s.signal = 50f; // stand-in: room for the spike to show (we are at 100% with this much power drawn)
        float sig0 = s.signal;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "pull the fire lever", "fc.lever")) yield break;
        Check("The lever fires one slug and releases ARM", s.Rounds.Count == 1 && !s.sw[3], s.Rounds.Count + " in flight");
        Check("Firing spikes our signal", Mathf.Approximately(s.signal, sig0 + s.fireSignalSpike), sig0.ToString("0") + " → " + s.signal.ToString("0"));
        yield return 0.4f;
        Check("The fire cue sounds", board.Sounds > 0 && (board.LastSound == "railgun" || board.LastSound == "hit"), "last cue \"" + board.LastSound + "\"");
        //     Paused, the slug stops.
        if (!Click("pause", null, "PAUSE key")) yield break;
        double rx = s.Rounds.Count > 0 ? s.Rounds[0].travelled : -1;
        yield return 0.6f;
        Check("Paused, the slug holds still", s.Rounds.Count == 1 && s.Rounds[0].travelled == rx, (rx >= 0 ? rx.ToString("N0") + " km flown" : "none"));
        if (!Click("pause", null, "RUN key")) yield break;
        if (board.Motion && board.Motion.ScopeDown) board.Motion.SetScopeDown(false); // the periscope covers the plot's middle
        board.SetZoom(0.4);
        yield return 3f;
        Shot("27_slug_in_flight");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        board.SetZoom(1.0);
        yield return (Func<bool>)(() => s.Rounds.Count == 0);
        yield return (Func<bool>)(() => s.Rounds.Count == 0); // 15 s each; the flight is about 20 s at x10
        Check("The slug lands as the aim says: " + (expectHit ? "a HIT, no detail on a ship we have not scanned" : "a MISS"),
              expectHit ? s.shotReport == "HIT · BOGEY T07" : s.shotReport == "MISS",
              "\"" + s.shotReport + "\"; the aim passes " + predicted.ToString("N0") + " km from it (hit radius " + s.HitRadiusKm(t07.klass) + " km)");
        yield return 0.3f;
        Check("Fire control reports it, with its cue", board.LastSound == (expectHit ? "hit" : "miss"), "last cue \"" + board.LastSound + "\"");
        Shot("28_hit");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        //     A second shot 2° off: a miss.
        if (!PlotDrag(board, brgAt, brgAt + new Vector2(12, 0), "turn the BRG knob (+2°)", "fc.knob")) yield break;
        s.SlewTo(); // stand-in for the periscope's SLEW TO key: onto the new dialled bearing
        yield return (Func<bool>)(() => s.wepCharge >= s.ShotCost);
        for (int i = 0; i < 4; i++) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (again)")) yield break; yield return 0.2f; }
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "pull the fire lever (again)", "fc.lever")) yield break;
        yield return (Func<bool>)(() => s.Rounds.Count == 0);
        yield return (Func<bool>)(() => s.Rounds.Count == 0);
        Check("2° off, it misses", s.shotReport == "MISS", "\"" + s.shotReport + "\"");
        if (!Click("rate", "1", "x1 rate key")) yield break;

        // 21b. Leading a moving target (Step 52). TRACK 07 gets under way on its patrol again.
        brain.enabled = true;
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => t07Ship.SpeedKms > 1.0);
        yield return (Func<bool>)(() => t07Ship.hdg == t07Ship.ordHdg); // steady on its leg before we plot it
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Stand-in for the helm: 12,000 km off its starboard beam, stopped, on its course, so turret B bears.
        double beam = (t07Ship.CourseDeg + 90) * System.Math.PI / 180;
        s.ownX = t07Ship.ownX + System.Math.Sin(beam) * 12000; s.ownY = t07Ship.ownY + System.Math.Cos(beam) * 12000;
        s.vx = s.vy = 0; s.thr = 0;
        s.hdg = s.ordHdg = HandoffBoardState.JsRound(t07Ship.CourseDeg) % 360;
        yield return 1f;
        //     Its track: two POINTs on it two minutes apart (stand-in for accurate plotting; the plot itself was tested above).
        s.marks.Clear();
        var trackA = new HandoffBoardState.Mark { id = ++s.markSeq, kind = "point", x = t07Ship.ownX, y = t07Ship.ownY, t = board.MissionSeconds };
        s.marks.Add(trackA);
        //     The solution: the periscope on it, the RNG knob starts one, dialled to its range (stand-in for reading the plot).
        s.LookAt("t07");
        s.sol[s.SysIndex] = new HandoffBoardState.Solution();
        if (!PlotDrag(board, knobAt, knobAt + new Vector2(8, 0), "turn the RNG knob (1 step)", "fc.knob")) yield break;
        s.Sol.rangeKm = (float)System.Math.Sqrt((t07Ship.ownX - s.ownX) * (t07Ship.ownX - s.ownX) + (t07Ship.ownY - s.ownY) * (t07Ship.ownY - s.ownY));
        s.NudgeSol("rng", 0);
        Check("Without a track the solution has no course", s.Sol.set && s.Sol.manual && !s.Sol.hasCourse, "");
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => board.MissionSeconds - trackA.t >= 120f);
        if (!Click("rate", "1", "x1 rate key")) yield break;
        var trackB = new HandoffBoardState.Mark { id = ++s.markSeq, kind = "point", x = t07Ship.ownX, y = t07Ship.ownY, t = board.MissionSeconds };
        s.marks.Add(trackB);
        s.MarkDown("RULER", trackA.x, trackA.y, board.MissionSeconds); s.draft.ptA = trackA.id;
        s.MarkMove(trackB.x, trackB.y); s.draft.ptB = trackB.id;
        s.MarkUp(0);
        // the plot gives the course and speed between its two POINTs
        float legCrs = (float)((System.Math.Atan2(trackB.x - trackA.x, trackB.y - trackA.y) * 180 / System.Math.PI + 360) % 360);
        float legSpd = (float)(System.Math.Sqrt((trackB.x - trackA.x) * (trackB.x - trackA.x) + (trackB.y - trackA.y) * (trackB.y - trackA.y)) / (trackB.t - trackA.t));
        Check("A POINT-to-POINT track feeds the solution its course and speed",
              s.Sol.hasCourse && Mathf.Abs(Mathf.DeltaAngle(s.Sol.course, legCrs)) < 0.01f && Mathf.Abs(s.Sol.spd - legSpd) < 0.006f,
              "CRS " + s.Sol.course.ToString("000") + "°, SPD " + s.Sol.spd.ToString("0.00") + " (its true course now " + t07Ship.CourseDeg.ToString("000") + "°, speed " + t07Ship.SpeedKms.ToString("0.00") + ")");
        //     Arm and fire, led. The same shot unled would miss.
        if (s.mount != "RGB" && !Click("fc.mount", null, "turret selector (RG-B)")) yield break;
        yield return (Func<bool>)(() => s.wepCharge >= s.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (led shot)")) yield break; yield return 0.15f; }
        double tvx = t07Ship.vx, tvy = t07Ship.vy;
        double unled = PassKm(s.ownX, s.ownY, s.hdg + s.Sol.brg, s.slugSpeedKms, t07Ship.ownX, t07Ship.ownY, tvx, tvy, 600);
        double led = PassKm(s.ownX, s.ownY, s.LeadBearing(), s.slugSpeedKms, t07Ship.ownX, t07Ship.ownY, tvx, tvy, 600);
        if (!Click("rate", "10", "x10 rate key")) yield break;
        int landedLed = s.shotResults;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "fire (led)", "fc.lever")) yield break;
        yield return (Func<bool>)(() => s.shotResults > landedLed);
        Check("Led from the track, it hits the moving TRACK 07 (unscanned: a bare HIT)", s.lastShotHit && s.shotReport == "HIT · BOGEY T07",
              "\"" + s.shotReport + "\"; aimed straight it would pass " + unled.ToString("N0") + " km off, led " + led.ToString("N0") + " km (hit radius " + s.HitRadiusKm(t07.klass) + ")");
        Check("Unled, the same shot would have missed", unled > s.HitRadiusKm(t07.klass), unled.ToString("N0") + " km");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     An active-scan lock: TRACK knows its true course and speed and leads exactly.
        s.identified.Add("t07"); // stand-in for an active scan identifying it
        yield return 0.6f;
        if (!Click("fc.track", null, "TRACK (locked)")) yield break;
        Check("TRACK's solution carries the target's course", s.Sol.set && !s.Sol.manual && s.Sol.hasCourse, "CRS " + s.Sol.course.ToString("000") + "°, SPD " + s.Sol.spd.ToString("0.00"));
        s.LookAt("t07");
        yield return (Func<bool>)(() => s.wepCharge >= s.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (TRACK shot)")) yield break; yield return 0.15f; }
        if (!Click("rate", "10", "x10 rate key")) yield break;
        int landedTrack = s.shotResults;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "fire (TRACK)", "fc.lever")) yield break;
        yield return (Func<bool>)(() => s.shotResults > landedTrack);
        Check("TRACK's led shot hits", s.shotReport != null && s.shotReport.StartsWith("HIT · TRACK 07"), "\"" + s.shotReport + "\"");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Hold it still again for the fight below; clear the plot.
        brain.enabled = false; t07Ship.vx = t07Ship.vy = 0; s.marks.Clear(); s.identified.Remove("t07");

        // 21c. Missiles (Step 53): a lock, the launcher, three warheads, homing on the moving TRACK 07.
        brain.enabled = true;
        s.identified.Add("t07"); // stand-in for an active-scan lock
        for (int k = 0; k < 3 && s.sys != "MS"; k++) { if (!Click("fc.system", null, "weapon selector")) yield break; yield return 0.2f; }
        s.salvo = 2; // stand-in for the Tuning window (its default is 2)
        Check("MISSILE selected: the launcher, HE", s.sys == "MS" && s.mount == "VLS" && s.warheads[s.warhead] == "HE", s.sys + " " + s.mount + " " + s.warheads[s.warhead]);
        if (!Click("fc.track", null, "TRACK (locked)")) yield break;
        s.LookAt("t07");
        yield return (Func<bool>)(() => s.wepCharge >= s.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (missiles)")) yield break; yield return 0.15f; }
        Func<HandoffBoardState, float> hull = sh => { float h = 0; foreach (var a in sh.SideArmour) h += a; foreach (var c in sh.compartments) h += c.integ; return h; };
        int cells0 = s.vlsCells, landed0 = s.shotResults;
        float hull0 = hull(dummyShip);
        if (!Click("rate", "10", "x10 rate key")) yield break;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "launch (HE salvo of 2)", "fc.lever")) yield break;
        Check("A salvo of two leaves the launcher", s.Rounds.FindAll(r => r.missile).Count == 2 && s.vlsCells == cells0 - 2, s.vlsCells + " cells left");
        yield return (Func<bool>)(() => s.shotResults >= landed0 + 2);
        yield return (Func<bool>)(() => s.shotResults >= landed0 + 2);
        Check("Both home in on the moving TRACK 07 and the HE does damage", s.lastShotHit && s.shotReport.Contains(" · HE") && hull(dummyShip) < hull0,
              "\"" + s.shotReport + "\", armour + integrity " + hull0 + " → " + hull(dummyShip));
        //     EMP: no hull damage, its power goes down and it slows.
        if (!Click("fc.mount", null, "warhead selector (EMP)")) yield break;
        Check("The turret knob picks the warhead", s.warheads[s.warhead] == "EMP", s.warheads[s.warhead]);
        s.salvo = 1;
        s.LookAt("t07");
        yield return (Func<bool>)(() => s.wepCharge >= s.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (EMP)")) yield break; yield return 0.15f; }
        int landed1 = s.shotResults;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "launch (EMP)", "fc.lever")) yield break;
        yield return (Func<bool>)(() => s.shotResults > landed1);
        yield return (Func<bool>)(() => s.shotResults > landed1);
        double spdHit = t07Ship.SpeedKms;
        Check("EMP: its power goes down", s.shotReport != null && s.shotReport.EndsWith(" · EMP · POWER DOWN") && dummyShip.buses[0].fill == 0, "\"" + s.shotReport + "\"");
        yield return 2f;
        Check("Without power it slows", t07Ship.SpeedKms < spdHit, spdHit.ToString("0.00") + " → " + t07Ship.SpeedKms.ToString("0.00") + " km/s");
        //     PENETRATOR: straight to a compartment behind the side.
        if (!Click("fc.mount", null, "warhead selector (PENETRATOR)")) yield break;
        s.LookAt("t07");
        yield return (Func<bool>)(() => s.wepCharge >= s.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (PEN)")) yield break; yield return 0.15f; }
        int landed2 = s.shotResults;
        if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "launch (PENETRATOR)", "fc.lever")) yield break;
        yield return (Func<bool>)(() => s.shotResults > landed2);
        yield return (Func<bool>)(() => s.shotResults > landed2);
        Check("PENETRATOR: straight through to a compartment", s.shotReport != null && s.shotReport.Contains(" · PEN · "), "\"" + s.shotReport + "\"");
        Check("Four cells used", s.vlsCells == cells0 - 4, s.vlsCells + " left");
        Shot("28b_missiles");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Back to the railgun; hold TRACK 07 still for the fight.
        if (!Click("fc.system", null, "weapon selector (RAILGUN)")) yield break;
        brain.enabled = false; t07Ship.vx = t07Ship.vy = 0; s.identified.Remove("t07");

        s.identified.Add("t07"); // stand-in for an active scan: from here on we see what our hits do (Step 61)
        // 21d. Fighters (Step 54): the interceptors launch from the hangar, fly to TRACK 07, make attack runs
        //     on the side facing them, and RECALL brings them home to land.
        //     Stand-in for the helm: 2,000 km ahead of its bow (never hit yet), stopped, facing it.
        double sbd = t07.worldHdg * System.Math.PI / 180;
        s.ownX = t07.worldX + System.Math.Sin(sbd) * 2000; s.ownY = t07.worldY + System.Math.Cos(sbd) * 2000;
        s.vx = s.vy = 0; s.thr = 0;
        s.hdg = s.ordHdg = ((int)System.Math.Round(t07.worldHdg) + 180) % 360;
        s.target = "t07";
        var vxField = typeof(HandoffPlotPanel).GetField("viewX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var vyField = typeof(HandoffPlotPanel).GetField("viewY", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        vxField.SetValue(plotPanel, (s.ownX + t07.worldX) / 2); vyField.SetValue(plotPanel, (s.ownY + t07.worldY) / 2); // stand-in for panning the plot
        var ilField = typeof(HandoffHangarPanel).GetField("interlock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hangarPanel = board.GetComponent<HandoffHangarPanel>();
        Func<string> interlockText = () => { var t = ilField.GetValue(hangarPanel) as TMPro.TMP_Text; return t ? t.text : ""; };
        if (board.Motion && board.Motion.ScopeDown) board.Motion.SetScopeDown(false); // the periscope covers the plot's middle
        //     Power for the catapult: ENG and WEP hold all ten units, so ENG gives its back (we are stopped) and AUX draws.
        if (!Click("drawer", "power", "PWR key")) yield break;
        yield return 0.8f;
        for (int k = 0; k < 3 && s.buses[0].mode != HandoffBoardState.BusMode.Vent; k++) if (!Click("power.bus", "0", "ENG selector (toward VENT)")) yield break;
        for (int k = 0; k < 3 && s.buses[3].mode != HandoffBoardState.BusMode.Draw; k++) if (!Click("power.bus", "3", "AUX selector (toward DRAW)")) yield break;
        yield return (Func<bool>)(() => s.BusUnits("aux") >= 1);
        if (!Click("drawer", "hangar", "HGR key")) yield break;
        yield return 0.8f;
        Check("The interceptors are selected, on DESTROY", s.CurrentSquad.key == "gw" && s.order == "destroy" && s.SquadReady(s.CurrentSquad) == 4, s.CurrentSquad.key + " " + s.order + ", " + s.SquadReady(s.CurrentSquad) + " ready");
        s.SetLbrg(s.hdg.ToString()); // stand-in for typing the launch bearing: dead ahead
        if (!s.doorsOpen && !Click("hangar.doors", null, "bay door master")) yield break;
        Check("The catapult can charge (AUX powered)", s.StageBlock("cat") == null, (s.StageBlock("cat") ?? "") + " AUX " + s.BusUnits("aux") + " units, ENG " + s.buses[0].mode);
        for (int i = 0; i < s.stages.Length; i++)
        {
            if (!Click("hangar.stage", i.ToString(), s.stages[i].label + " stage")) yield break;
            int k = i;
            yield return (Func<bool>)(() => s.stages[k].progress >= 100f);
        }
        Check("Every launch stage at 100%", s.AllStagesDone && s.LaunchBlock() == null, s.LaunchBlock() ?? "");
        var hLever = Hit("hangar.lever", null);
        Vector2 hLeverAt = board.BoardPoint(RectTransformUtility.WorldToScreenPoint(hLever.GetComponentInParent<Canvas>().rootCanvas.worldCamera,
                                            hLever.transform.TransformPoint(((RectTransform)hLever.transform).rect.center)));
        if (!PlotDrag(board, hLeverAt, hLeverAt + new Vector2(240, 0), "pull the launch lever", "hangar.lever")) yield break;
        yield return 0.6f;
        Check("Four interceptors take off: four wedges on the plot", s.Flights.Count == 1 && s.Flights[0].craft == 4 && plotPanel.FightersShown == 4 && s.Launched(s.CurrentSquad),
              s.Flights.Count + " flight, " + (s.Flights.Count > 0 ? s.Flights[0].craft : 0) + " craft, " + plotPanel.FightersShown + " drawn");
        var flight = s.Flights.Count > 0 ? s.Flights[0] : null;
        if (flight == null) yield break;
        if (!Click("rate", "10", "x10 rate key")) yield break;
        double fx0 = flight.x, fy0 = flight.y; float ft0 = board.MissionSeconds;
        yield return 2f;
        double flown = System.Math.Sqrt((flight.x - fx0) * (flight.x - fx0) + (flight.y - fy0) * (flight.y - fy0)), took = board.MissionSeconds - ft0;
        Check("They fly toward TRACK 07 at 8 km/s", System.Math.Abs(flown / took - s.fighterSpeedKms) < 0.5 && interlockText().StartsWith("OUTBOUND TO TRACK 07"),
              (flown / took).ToString("0.00") + " km/s; \"" + interlockText() + "\"");
        yield return (Func<bool>)(() => System.Math.Sqrt((flight.x - s.ownX) * (flight.x - s.ownX) + (flight.y - s.ownY) * (flight.y - s.ownY)) > 1000); // clear of our ship's picture
        Shot("28c_fighters_outbound");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        float prow0 = dummyShip.SideArmour[0];
        yield return (Func<bool>)(() => s.flightRuns >= 1);
        float prowAfter = Mathf.Max(0f, prow0 - 4 * s.fighterArmourDamage);
        Check("In range, one attack run: 10 armour per fighter off the side facing them, its bow", s.flightRuns == 1 && prow0 > 4 * s.fighterArmourDamage
              && s.flightReport == " PROW ARMOUR " + HandoffBoardState.JsRound(prowAfter / dummyShip.sideArmourMax[0] * 100f) + "%"
              && Mathf.Approximately(dummyShip.SideArmour[0], prowAfter),
              "\"" + s.flightReport + "\"; PROW armour " + prow0 + " → " + dummyShip.SideArmour[0]);
        Check("The hangar reports the attack", interlockText().StartsWith("ATTACKING TRACK 07"), "\"" + interlockText() + "\"");
        Shot("28d_fighters_attacking");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("hangar.recall", null, "RECALL")) yield break;
        Check("RECALL turns them home", flight.returning && interlockText().StartsWith("RETURNING"), "\"" + interlockText() + "\"");
        int runs = s.flightRuns;
        yield return (Func<bool>)(() => s.Flights.Count == 0);
        yield return (Func<bool>)(() => s.Flights.Count == 0); // 15 s each; the way home is about 11 s at x10
        Check("Home, they land: the squadron is aboard and its stages clear", s.Flights.Count == 0 && !s.Launched(s.CurrentSquad) && !s.AllStagesDone && plotPanel.FightersShown == 0 && s.flightRuns == runs,
              s.Flights.Count + " flights, " + plotPanel.FightersShown + " drawn, " + (s.flightRuns - runs) + " runs after the recall");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.6f;

        // 21e. Point defence (Step 55): hit, TRACK 07 fires missiles back; PDC-A on AUTO shoots them down;
        //     with its ladder dropped, the next salvo gets through and hits us.
        //     Stand-in for the helm: 6,000 km off its bow, stopped, turned so it bears 042° from our bow (PDC-A's arc).
        double pdb = t07.worldHdg * System.Math.PI / 180;
        s.ownX = t07.worldX + System.Math.Sin(pdb) * 6000; s.ownY = t07.worldY + System.Math.Cos(pdb) * 6000;
        s.vx = s.vy = 0; s.thr = 0;
        s.hdg = s.ordHdg = (((int)System.Math.Round(t07.worldHdg) + 180 - 42) % 360 + 360) % 360;
        vxField.SetValue(plotPanel, (s.ownX + t07.worldX) / 2); vyField.SetValue(plotPanel, (s.ownY + t07.worldY) / 2); // stand-in for panning the plot
        brain.enabled = true; // it thinks again (it holds its fire for a tick, so old damage doesn't count)
        yield return 1f;
        for (int k = 0; k < 3 && s.sys != "PD"; k++) { if (!Click("fc.system", null, "weapon selector")) yield break; yield return 0.2f; }
        Check("PDC selected: group A, autoloaders engaged", s.sys == "PD" && s.mount == "PDA" && s.loaders, s.sys + " " + s.mount + ", loaders " + s.loaders);
        bool noSol = !s.Sol.set;
        yield return (Func<bool>)(() => s.wepCharge >= s.ShotCost);
        for (int i = 0; i < 4; i++) if (!s.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (PDC-A)")) yield break; yield return 0.15f; }
        Check("The ladder arms with no solution: PDC-A is on AUTO", noSol && System.Array.TrueForAll(s.sw, b => b) && s.pdcAuto[0] && !s.pdcAuto[1], string.Join(",", System.Array.ConvertAll(s.sw, b => b ? "1" : "0")));
        //     We hit it (stand-in for a railgun hit, tested above): it fires back.
        s.pdcKillChance = 1f; // stand-in: a sure PDC, so the result is certain (the real chance is in the Tuning window)
        brain.firesBack = true;
        int enemyCells = dummyShip.vlsCells, kills0 = s.pdcKills, taken0 = s.hitsTaken;
        dummyShip.TakeSlug("PROW");
        yield return (Func<bool>)(() => s.Incoming.Count > 0);
        Check("Hit, TRACK 07 fires a salvo of two back at us", s.Incoming.Count == 2 && dummyShip.vlsCells == enemyCells - 2, s.Incoming.Count + " incoming, its cells " + enemyCells + " → " + dummyShip.vlsCells);
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => s.Incoming.Count == 0 || System.Math.Sqrt((s.Incoming[0].x - s.ownX) * (s.Incoming[0].x - s.ownX) + (s.Incoming[0].y - s.ownY) * (s.Incoming[0].y - s.ownY)) < 4600);
        Shot("28e_incoming_pdc_arc");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        yield return (Func<bool>)(() => s.Incoming.Count == 0);
        yield return (Func<bool>)(() => s.Incoming.Count == 0); // 15 s each; the flight is about 13 s at x10
        Check("PDC-A shoots both down; nothing hits us", s.pdcKills == kills0 + 2 && s.hitsTaken == taken0 && s.pdcReport == "PDC-A · MISSILE DOWN",
              (s.pdcKills - kills0) + " down, " + (s.hitsTaken - taken0) + " hits on us, \"" + s.pdcReport + "\"");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Changing weapons leaves PDC-A on AUTO; back on PDC-A its ladder shows armed.
        for (int k = 0; k < 3 && s.sys != "RG"; k++) { if (!Click("fc.system", null, "weapon selector (to RAILGUN)")) yield break; yield return 0.2f; }
        Check("On the railgun, PDC-A stays on AUTO", s.sys == "RG" && s.pdcAuto[0], s.sys);
        for (int k = 0; k < 3 && s.sys != "PD"; k++) { if (!Click("fc.system", null, "weapon selector (back to PDC)")) yield break; yield return 0.2f; }
        Check("Back on PDC-A, its ladder is still armed", s.sys == "PD" && s.mount == "PDA" && System.Array.TrueForAll(s.sw, b => b), "");
        //     Dropping ARM takes it off AUTO: the next salvo gets through.
        if (!Click("fc.flip", "3", "ARM rocker (drop)")) yield break;
        Check("Dropping ARM takes PDC-A off AUTO", !s.sw[3] && !s.pdcAuto[0], "");
        var ourArmour = (float[])s.SideArmour.Clone();
        var ourInteg = System.Array.ConvertAll(s.compartments, c => c.integ);
        if (!Click("rate", "10", "x10 rate key")) yield break;
        dummyShip.TakeSlug("PROW"); // stand-in for another railgun hit
        yield return (Func<bool>)(() => s.Incoming.Count > 0);
        yield return (Func<bool>)(() => s.Incoming.Count > 0); // its 120 s between salvos
        yield return (Func<bool>)(() => s.Incoming.Count == 0);
        yield return (Func<bool>)(() => s.Incoming.Count == 0);
        yield return 0.4f;
        Check("With no PDC on AUTO, both hit us, with the alert tone", s.hitsTaken == taken0 + 2 && s.incomingReport != null && s.incomingReport.StartsWith("MISSILE HIT · ") && board.LastSound == "alert",
              (s.hitsTaken - taken0) + " hits, last \"" + s.incomingReport + "\", last cue \"" + board.LastSound + "\"");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Our ship can die too (Step 58): its port side open, three slugs into the reactor (stand-in for enemy fire).
        s.SideArmour[3] = 0f;
        for (int k = 0; k < 3; k++) s.TakeSlug("PORT");
        yield return 1f;
        Check("Our reactor at 0: our ship is destroyed and goes dark", s.destroyed && s.Comp("rct").integ == 0 && s.TotalFill == 0,
              "RCT " + s.Comp("rct").integ + ", power drawn " + s.TotalFill);
        s.destroyed = false;
        //     Clean up for the steps below: TRACK 07 held still and quiet, our ship repaired (stand-in for damage control).
        brain.firesBack = false; brain.enabled = false; t07Ship.vx = t07Ship.vy = 0;
        s.pdcKillChance = 0.7f;
        System.Array.Copy(ourArmour, s.SideArmour, ourArmour.Length);
        for (int c = 0; c < s.compartments.Length; c++) s.compartments[c].integ = ourInteg[c];
        for (int k = 0; k < 3 && s.sys != "RG"; k++) { if (!Click("fc.system", null, "weapon selector (RAILGUN)")) yield break; yield return 0.2f; }

        // 21f. Boarding (Step 60, BOARDING_README.md): the assault boats, on DISABLE, carry marines to TRACK 07,
        //     hold off while no face is open, board, wreck its engines and magazine, and come home. DISABLE keeps it
        //     alive for the fight below; DESTROY and SECURE were checked on the bare ship model.
        //     Stand-in for the helm: 1,500 km off its starboard beam, stopped.
        double sb2 = (t07.worldHdg + 90) * System.Math.PI / 180;
        s.ownX = t07.worldX + System.Math.Sin(sb2) * 1500; s.ownY = t07.worldY + System.Math.Cos(sb2) * 1500;
        s.vx = s.vy = 0; s.thr = 0;
        s.hdg = s.ordHdg = ((int)System.Math.Round(t07.worldHdg) + 270) % 360;
        s.target = "t07";
        vxField.SetValue(plotPanel, (s.ownX + t07.worldX) / 2); vyField.SetValue(plotPanel, (s.ownY + t07.worldY) / 2);
        if (!Click("drawer", "hangar", "HGR key")) yield break;
        yield return 0.8f;
        if (s.CurrentSquad.key != "ih" && !Click("hangar.squad", null, "squadron selector (ASSAULT BOAT)")) yield break;
        for (int k = 0; k < 3 && s.order != "disable"; k++) if (!Click("hangar.order", null, "order knob")) yield break;
        Check("The assault boats are selected, on DISABLE", s.CurrentSquad.key == "ih" && s.order == "disable" && s.SquadReady(s.CurrentSquad) == 3, s.CurrentSquad.key + " " + s.order + ", " + s.SquadReady(s.CurrentSquad) + " ready");
        s.SetLbrg(s.hdg.ToString()); // stand-in for typing the launch bearing
        float defenders = s.defenderShare;
        s.defenderShare = 0f; // stand-in: an undefended ship, so the marines win for certain (the odds are in the Tuning window)
        var armourWas = (float[])dummyShip.SideArmour.Clone();
        for (int k = 0; k < 4; k++) dummyShip.SideArmour[k] = Mathf.Max(dummyShip.SideArmour[k], 10f); // stand-in: no face open, so the boats must hold off at first
        for (int i = 0; i < s.stages.Length; i++)
        {
            if (!Click("hangar.stage", i.ToString(), s.stages[i].label + " stage (boats)")) yield break;
            int k = i;
            yield return (Func<bool>)(() => s.stages[k].progress >= 100f);
        }
        int marinesBefore = s.marines;
        if (!PlotDrag(board, hLeverAt, hLeverAt + new Vector2(240, 0), "pull the launch lever (boats)", "hangar.lever")) yield break;
        yield return 0.4f;
        var boats = s.Flights.Find(f => f.squad == "ih");
        Check("Three boats launch with 18 marines", boats != null && boats.craft == 3 && boats.marines == 18 && s.marines == marinesBefore - 18,
              boats != null ? boats.craft + " boats, " + boats.marines + " marines, " + s.marines + " left aboard" : "no flight");
        if (boats == null) yield break;
        if (!Click("rate", "10", "x10 rate key")) yield break;
        yield return (Func<bool>)(() => boats.holding != null);
        yield return (Func<bool>)(() => boats.holding != null);
        Check("No face open: they hold off", interlockText() == "HOLDING OFF TRACK 07 · NO OPEN SIDE", "\"" + interlockText() + "\"");
        Shot("28f_boats_holding");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        dummyShip.SideArmour[1] = 0f; // stand-in for a slug opening its starboard side
        yield return (Func<bool>)(() => boats.aboard);
        Check("A face open, the ship stopped: they board", boats.aboard && interlockText().StartsWith("BOARDING TRACK 07 · "), "\"" + interlockText() + "\"");
        yield return (Func<bool>)(() => !boats.aboard);
        Check("The marines win and disable it: engines and magazine wrecked, the ship still alive", boats.outcome == "TRACK 07 DISABLED" && dummyShip.Comp("eng").integ == 0 && dummyShip.Comp("mag").integ == 0 && !dummyShip.destroyed && !t07.wreck,
              "\"" + boats.outcome + "\", ENG " + dummyShip.Comp("eng").integ + ", MAG " + dummyShip.Comp("mag").integ);
        Check("The hangar reports it on the way home", interlockText().StartsWith("TRACK 07 DISABLED · RETURNING"), "\"" + interlockText() + "\"");
        yield return (Func<bool>)(() => s.Flights.Count == 0);
        yield return (Func<bool>)(() => s.Flights.Count == 0);
        Check("They land: the marines are back aboard", s.Flights.Count == 0 && s.marines == marinesBefore && !s.Launched(s.CurrentSquad), s.marines + " marines aboard");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        System.Array.Copy(armourWas, dummyShip.SideArmour, armourWas.Length); // its armour as it was
        s.defenderShare = defenders;
        if (!Click("drawer", "fire control", "FIRE key")) yield break;
        yield return 0.6f;

        // 22. A fight to the kill (Step 49; Step 58: the reactor sits behind the PORT face): close in off
        //     TRACK 07's port side and shoot until it is a wreck.
        var before = (float[])dummyShip.SideArmour.Clone(); // the shots above may have marked its sides
        var integBefore = System.Array.ConvertAll(dummyShip.compartments, c => c.integ);
        //     Stand-in for the helm: 10,000 km off its port beam, stopped, side-on so turret B (port) bears.
        double tb = (t07.worldHdg - 90) * System.Math.PI / 180;
        s.ownX = t07.worldX + System.Math.Sin(tb) * 10000; s.ownY = t07.worldY + System.Math.Cos(tb) * 10000;
        s.vx = s.vy = 0; s.thr = 0;
        s.hdg = s.ordHdg = ((int)System.Math.Round(t07.worldHdg) + 180) % 360;
        yield return 1f;
        s.LookAt("t07");
        s.sol[s.SysIndex] = new HandoffBoardState.Solution(); // stand-in: clear the old solution
        if (!PlotDrag(board, knobAt, knobAt + new Vector2(8, 0), "turn the RNG knob (1 step)", "fc.knob")) yield break;
        best = 0;
        for (int d = -1; d <= 1; d++) if (MissKm(s, t07, s.hdg + s.Sol.brg + d) < MissKm(s, t07, s.hdg + s.Sol.brg + best)) best = d;
        if (best != 0 && !PlotDrag(board, brgAt, brgAt + new Vector2(6 * best, 0), "turn the BRG knob to the nearest degree", "fc.knob")) yield break;
        s.SlewTo();
        //     The targeting computer (the periscope) shows a scanned ship's hull in its SUBJECT box (Step 61).
        if (board.Motion) board.Motion.SetScopeDown(true);
        yield return 1.2f;
        var scope = board.GetComponent<HandoffPeriscopePanel>();
        var subjName = typeof(HandoffPeriscopePanel).GetField("subjectName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(scope) as TMPro.TMP_Text;
        var subjHull = typeof(HandoffPeriscopePanel).GetField("subjectClass", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(scope) as TMPro.TMP_Text;
        string hullNow = HandoffPeriscopePanel.HullLine(dummyShip);
        Check("Scanned, the targeting computer shows its hull: each face, and the room behind an open one", subjName && subjHull && subjName.text.StartsWith("TRACK 07") && subjHull.text == hullNow && hullNow.StartsWith("BOW ") && hullNow.Contains(" \u25B8 "),
              subjName && subjHull ? "\"" + subjName.text + "\" / \"" + subjHull.text + "\"" : "no readout");
        //     CLASS in the targeting computer: the plot shows TRACK 07 as its own ship, in red (owner 2026-10-24).
        if (!Click("scope.classify", null, "CLASS key")) yield break;
        yield return 0.6f;
        Check("Classified: TRACK 07 shows on the plot as its own ship model, in place of its dot", s.classified.Contains("t07") && plotPanel.ContactShipShown && subjName.text == "TRACK 07 · CRUISER",
              "\"" + subjName.text + "\", picture " + plotPanel.ContactShipShown);
        //     The sweep runs from a small circle round our ship, not from the ship itself.
        var sweepInner = (float)typeof(HandoffSweepGraphic).GetField("innerPx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                .GetValue(plotPanel.GetComponentInChildren<HandoffSweepGraphic>(true));
        Check("The sweep starts at a small circle just outside our ship", sweepInner > 15f, sweepInner.ToString("0") + " px");
        Shot("28g_targeting_computer");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (board.Motion) board.Motion.SetScopeDown(false);
        yield return 0.6f;
        if (s.mount != "RGB" && !Click("fc.mount", null, "turret selector (RG-B)")) yield break;
        if (!Click("rate", "10", "x10 rate key")) yield break;
        var reports = new List<string>();
        for (int shot = 0; shot < 10 && !t07.wreck; shot++)
        {
            yield return (Func<bool>)(() => s.wepCharge >= s.ShotCost);
            for (int i = 0; i < 4; i++)
                if (!s.sw[i]) { if (!Click("fc.flip", i.ToString(), "arming rocker " + (i + 1) + " (shot " + (shot + 1) + ")")) yield break; yield return 0.15f; }
            int landed = s.shotResults;
            if (!PlotDrag(board, leverAt, leverAt + new Vector2(240, 0), "fire (shot " + (shot + 1) + ")", "fc.lever")) yield break;
            yield return (Func<bool>)(() => s.shotResults > landed);
            reports.Add(s.shotReport);
        }
        string log = string.Join(" | ", reports);
        int opened = reports.FindIndex(r => r == "HIT · TRACK 07 · PORT OPEN");
        bool armourFirst = opened >= 0;
        for (int i = 0; i < opened; i++) if (!reports[i].StartsWith("HIT · TRACK 07 · PORT ARMOUR ")) armourFirst = false;
        Check("The first hits open its port side", armourFirst, log);
        bool reactorOnly = opened >= 0;
        for (int i = opened + 1; i < reports.Count - 1; i++) if (reports[i] != "HIT · TRACK 07 · PORT · RCT −2") reactorOnly = false;
        Check("Then each hit takes 2 off the reactor behind it, the same room every time", reports.Count - opened - 2 > 0 && reactorOnly, reports.Count + " shots");
        Check("Its reactor gone, it is destroyed: a cold wreck", t07.wreck && dummyShip.destroyed && dummyShip.Comp("rct").integ == 0 && !s.Heard(t07) && reports[reports.Count - 1] == "TRACK 07 DESTROYED",
              "RCT " + dummyShip.Comp("rct").integ + ", last \"" + reports[reports.Count - 1] + "\"");
        bool untouched = dummyShip.SideArmour[0] == before[0] && dummyShip.SideArmour[1] == before[1] && dummyShip.SideArmour[2] == before[2];
        for (int c = 0; c < dummyShip.compartments.Length; c++) if (dummyShip.compartments[c].key != "rct" && dummyShip.compartments[c].integ != integBefore[c]) untouched = false;
        Check("Nothing else on it was touched", untouched, "");
        yield return 0.4f;
        Check("The kill cue sounds", board.LastSound == "kill", "last cue \"" + board.LastSound + "\"");
        Shot("29_destroyed");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        if (!Click("rate", "1", "x1 rate key")) yield break;
        if (!Click("drawer", "fire control", "FIRE key (close)")) yield break;
        yield return 0.5f;

        // 23. Engineering's ship diagram (Step 50): our ship bow up, a lamp per compartment.
        if (!Click("drawer", "engineering", "ENGR key")) yield break;
        yield return 0.8f;
        var eng = board.GetComponent<HandoffEngineeringPanel>();
        var picField = typeof(HandoffEngineeringPanel).GetField("shipPicture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var lampsField = typeof(HandoffEngineeringPanel).GetField("compartmentLamps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pic = picField.GetValue(eng) as UnityEngine.UI.RawImage;
        var lamps = lampsField.GetValue(eng) as UnityEngine.UI.Image[];
        Check("Engineering shows our ship, bow up", pic && pic.enabled && pic.texture != null && Mathf.Approximately(Mathf.DeltaAngle(pic.rectTransform.localEulerAngles.z, s.hdg), 0f),
              pic ? "turned back " + pic.rectTransform.localEulerAngles.z.ToString("0") + "° for heading " + s.hdg.ToString("000") + "°" : "no picture");
        Color green = ((Color)new Color32(0x8F, 0xC3, 0x3A, 0xFF)).gamma, amber = ((Color)new Color32(0xE0, 0xA0, 0x20, 0xFF)).gamma, red = ((Color)new Color32(0xE0, 0x4B, 0x3C, 0xFF)).gamma;
        System.Func<UnityEngine.UI.Image, Color, bool> lit = (l, c) => Mathf.Abs(l.color.r - c.r) < 0.01f && Mathf.Abs(l.color.g - c.g) < 0.01f && Mathf.Abs(l.color.b - c.b) < 0.01f;
        bool allGreen = lamps != null && lamps.Length == 6;
        if (allGreen) foreach (var l in lamps) if (!lit(l, green)) allGreen = false;
        Check("Six compartment lamps, all green: undamaged", allGreen, lamps != null ? lamps.Length + " lamps" : "none");
        //     Stand-in for an enemy hit: habitation hurt, hangar nearly gone.
        s.Comp("hab").integ = 2; s.Comp("hgr").integ = 1;
        yield return 0.6f;
        Check("A hurt compartment's lamp turns amber, a nearly-gone one red", lit(lamps[2], amber) && lit(lamps[3], red) && lit(lamps[0], green), "HAB 2, HGR 1");
        Shot("30_engineering_diagram");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        s.Comp("hab").integ = 5; s.Comp("hgr").integ = 5;
        yield return 0.6f;
        Check("Repaired, they go green again", lit(lamps[2], green) && lit(lamps[3], green), "");

        // 24. Conditions (Step 59, DAMAGE_README.md): fire, vacuum and radiation on our ship, handled with
        //     engineering's own rockers. Hits are stand-ins (enemy fire comes with the ship AI); chances and
        //     spread are set to certain so each result is exact; the real odds are in the Tuning window.
        var armour0 = (float[])s.SideArmour.Clone();
        var rooms0 = System.Array.ConvertAll(s.compartments, c => new[] { c.integ, c.crew });
        float spreadF = s.fireSpreadPerSecond, spreadV = s.vacuumSpreadPerSecond, spreadR = s.radSpreadPerSecond;
        s.fireSpreadPerSecond = s.vacuumSpreadPerSecond = s.radSpreadPerSecond = 0f;
        var recallRocker = System.Array.Find((HandoffEngineeringPanel.Repair[])typeof(HandoffEngineeringPanel).GetField("repairs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(eng), r => r.id == "recall");
        Check("DUMMY 1 is now RECALL, with the repair rockers' art", recallRocker != null && recallRocker.fitted && recallRocker.rocker.sprite.name.StartsWith("rocker_22x30") && recallRocker.label.text == "RECALL",
              recallRocker != null ? recallRocker.rocker.sprite.name + ", \"" + recallRocker.label.text + "\"" : "missing");
        //     An HE hit through our open bow sets the magazine on fire.
        s.SideArmour[0] = 0f; s.fireChanceHE = 1f;
        string heHit = s.TakeWarhead("PROW", "HE");
        Check("HE through the open bow: the magazine catches fire", heHit.EndsWith(" · FIRE") && s.Comp("mag").fire && s.Comp("mag").cond == "FIRE", "\"" + heHit + "\"");
        //     The fire spreads to the magazine's neighbours, and only those.
        s.fireSpreadPerSecond = 1f;
        float tickF = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds > tickF); // exactly one tick of spread
        s.fireSpreadPerSecond = 0f;
        Check("It spreads to the rooms next to it (bridge, reactor, hangar), not the others", s.Comp("bri").fire && s.Comp("rct").fire && s.Comp("hgr").fire && !s.Comp("eng").fire && !s.Comp("hab").fire,
              "BRI " + s.Comp("bri").fire + ", RCT " + s.Comp("rct").fire + ", HGR " + s.Comp("hgr").fire + ", ENG " + s.Comp("eng").fire + ", HAB " + s.Comp("hab").fire);
        //     A fire burns its room down.
        if (!Click("rate", "10", "x10 rate key")) yield break;
        int magBurn = s.Comp("mag").integ; float burnFrom = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds - burnFrom >= s.fireDamageSeconds + 1);
        if (!Click("rate", "1", "x1 rate key")) yield break;
        Check("A fire takes integrity off its room over time", s.Comp("mag").integ < magBurn, "MAG " + magBurn + " → " + s.Comp("mag").integ);
        //     REPAIR sends the team to the magazine: it puts the fire out.
        if (!Click("eng.pick", "mag", "MAG cell")) yield break;
        if (!Click("eng.run", "party", "REPAIR rocker (MAG)")) yield break;
        yield return (Func<bool>)(() => !s.PartyOut);
        Check("The team puts the magazine's fire out", !s.Comp("mag").fire && s.Comp("mag").cond != "FIRE", "");
        //     VENT the bridge: the fire goes, and so does its crew.
        if (!Click("eng.pick", "bri", "BRI cell")) yield break;
        if (!Click("eng.run", "vent", "VENT rocker (BRI)")) yield break;
        Check("VENT puts the bridge's fire out, and its crew die", !s.Comp("bri").fire && s.Comp("bri").vented && s.Comp("bri").crew == 0 && s.Comp("bri").doorsShut, "crew " + s.Comp("bri").crew);
        Shot("31_conditions");
        yield return 0.2f; // the shot lands at the end of the frame, before the next action
        //     The reactor and hangar fires: out by stand-in, to keep the reactor whole for what follows.
        s.Comp("rct").fire = false; s.Comp("hgr").fire = false;
        //     A slug through our open starboard side breaches the hangar: half its crew die, and vacuum spreads.
        s.SideArmour[1] = 0f; s.breachChanceSlug = 1f;
        int hgrCrew = s.Comp("hgr").crew;
        string slugHit = s.TakeSlug("STBD");
        Check("A slug through the open starboard side breaches the hangar: half its crew die", slugHit.EndsWith(" · BREACH") && s.Comp("hgr").vented && !s.Comp("hgr").doorsShut && s.Comp("hgr").crew == hgrCrew - Mathf.CeilToInt(hgrCrew * s.breachCrewLoss),
              "\"" + slugHit + "\", crew " + hgrCrew + " → " + s.Comp("hgr").crew);
        if (!Click("eng.pick", "hgr", "HGR cell")) yield break;
        Check("REPAIR waits until the doors are shut", s.EngBlock("party") == "SEAL THE DOORS FIRST", s.EngBlock("party") ?? "open");
        if (!Click("eng.run", "seal", "SEAL rocker (HGR)")) yield break;
        s.vacuumSpreadPerSecond = 1f;
        float tickV = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds > tickV);
        s.vacuumSpreadPerSecond = 0f;
        Check("SEAL shuts its doors: the vacuum does not spread", s.Comp("hgr").doorsShut && !s.Comp("mag").vented && !s.Comp("hab").vented && !s.Comp("eng").vented, "");
        if (!Click("eng.run", "party", "REPAIR rocker (HGR)")) yield break;
        yield return (Func<bool>)(() => !s.PartyOut);
        Check("The team restores the hangar's air", !s.Comp("hgr").vented && !s.Comp("hgr").doorsShut, "");
        //     RECALL calls a team back before it finishes.
        s.Comp("eng").integ = 3;
        if (!Click("eng.pick", "eng", "ENG cell")) yield break;
        if (!Click("eng.run", "party", "REPAIR rocker (ENG)")) yield break;
        if (!Click("eng.run", "recall", "RECALL rocker")) yield break;
        yield return 2f;
        Check("RECALL brings the team back: no repair lands", !s.PartyOut && s.Comp("eng").integ == 3, "ENG " + s.Comp("eng").integ);
        //     Two slugs into our open port side: the reactor leaks radiation.
        s.SideArmour[3] = 0f; s.breachChanceSlug = 0f; s.radChanceReactor = 1f;
        s.Comp("rct").integ = 5; // the fires above burned it a little; two slugs now leave it at 1
        string radHit = s.TakeSlug("PORT") + " /" + s.TakeSlug("PORT");
        s.radSpreadPerSecond = 1f;
        float tickR = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds > tickR);
        s.radSpreadPerSecond = 0f;
        Check("The reactor leaks radiation into the rooms next to it", radHit.Contains("RADIATION") && s.Comp("rct").rad && s.Comp("mag").rad && s.Comp("bri").rad && !s.Comp("hgr").rad,
              "\"" + radHit + "\", RCT " + s.Comp("rct").integ);
        if (!Click("rate", "10", "x10 rate key")) yield break;
        int magCrew = s.Comp("mag").crew; float radFrom = board.MissionSeconds;
        yield return (Func<bool>)(() => board.MissionSeconds - radFrom >= s.radCrewLossSeconds + 1);
        Check("Radiation kills crew while they stay", s.Comp("mag").crew < magCrew, "MAG crew " + magCrew + " → " + s.Comp("mag").crew);
        s.Comp("rct").integ = s.reactorContainedAt; // stand-in for two repairs
        yield return (Func<bool>)(() => !s.Comp("rct").rad && !s.Comp("mag").rad);
        Check("Reactor repaired: the radiation fades", !s.Comp("rct").rad && !s.Comp("mag").rad && !s.Comp("bri").rad, "");
        if (!Click("rate", "1", "x1 rate key")) yield break;
        //     Put our ship back (stand-in for damage control) and the real odds back.
        System.Array.Copy(armour0, s.SideArmour, armour0.Length);
        for (int c = 0; c < s.compartments.Length; c++)
        {
            var room = s.compartments[c];
            room.integ = rooms0[c][0]; room.crew = rooms0[c][1]; room.fire = room.rad = room.vented = room.doorsShut = false; room.cond = null;
        }
        s.fireSpreadPerSecond = spreadF; s.vacuumSpreadPerSecond = spreadV; s.radSpreadPerSecond = spreadR;
        s.fireChanceHE = condChances[0]; s.fireChancePEN = condChances[1]; s.breachChanceSlug = condChances[2]; s.radChanceReactor = condChances[3];
        if (!Click("drawer", "engineering", "ENGR key (close)")) yield break;
        yield return 1f;
    }

    // How close a slug fired from (px,py) on this bearing passes a target moving at (vx,vy), km.
    static double PassKm(double px, double py, double aimDeg, double speed, double tx, double ty, double vx, double vy, double maxT)
    {
        double a = aimDeg * System.Math.PI / 180, wx = vx - speed * System.Math.Sin(a), wy = vy - speed * System.Math.Cos(a);
        double rx = tx - px, ry = ty - py, ww = wx * wx + wy * wy;
        double t = ww > 0 ? System.Math.Max(0, System.Math.Min(maxT, -(rx * wx + ry * wy) / ww)) : 0;
        return System.Math.Sqrt((rx + wx * t) * (rx + wx * t) + (ry + wy * t) * (ry + wy * t));
    }

    // How far a straight shot from our ship on this bearing passes from a contact, km (huge if it's behind us).
    static double MissKm(HandoffBoardState s, HandoffBoardState.Track t, double aimDeg)
    {
        double a = aimDeg * System.Math.PI / 180, px = t.worldX - s.ownX, py = t.worldY - s.ownY;
        double along = px * System.Math.Sin(a) + py * System.Math.Cos(a);
        return along < 0 ? double.MaxValue : System.Math.Abs(px * System.Math.Cos(a) - py * System.Math.Sin(a));
    }

    // Where two bearing lines meet (world km; bearings clockwise from north). False if near parallel.
    static bool Cross(HandoffBoardState.BearingFix a, HandoffBoardState.BearingFix b, out double x, out double y)
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
    static bool PlotDrag(HandoffBoardControls board, Vector2 a, Vector2 b, string label, string expect = "plot")
    {
        var es = EventSystem.current;
        Vector2 sa = BoardToScreen(board, a), sb = BoardToScreen(board, b);
        var ped = new PointerEventData(es) { position = sa, pressPosition = sa, button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>();
        es.RaycastAll(ped, hits);
        var top = hits.Count > 0 ? ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject) : null;
        var hh = top ? top.GetComponent<HandoffHit>() : null;
        if (hh == null || hh.action != expect)
        {
            Check(label, false, "blocked: topmost at board " + a + " is " + (hits.Count > 0 ? hits[0].gameObject.name : "nothing"));
            return false;
        }
        ped.pointerCurrentRaycast = ped.pointerPressRaycast = hits[0];
        ExecuteEvents.Execute(top, ped, ExecuteEvents.pointerDownHandler);
        ped.position = sb; // released without a drag event: HandoffHit sends the release point as the last drag
        ExecuteEvents.Execute(top, ped, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(top, ped, ExecuteEvents.pointerClickHandler);
        return true;
    }

    static Vector2 BoardToScreen(HandoffBoardControls board, Vector2 bp)
    {
        var rt = (RectTransform)board.transform;
        var local = new Vector3(bp.x - rt.rect.width / 2f, rt.rect.height / 2f - bp.y, 0f);
        var cam = board.GetComponentInParent<Canvas>().rootCanvas.worldCamera;
        return RectTransformUtility.WorldToScreenPoint(cam, rt.TransformPoint(local));
    }

    static TMPro.TMP_Text MarkLabelText(HandoffBoardControls board, int i)
    {
        var f = typeof(HandoffPlotPanel).GetField("labels", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var list = f.GetValue(board.GetComponent<HandoffPlotPanel>()) as List<TMPro.TMP_Text>;
        return list != null && i < list.Count ? list[i] : null;
    }
    static string MarkLabel(HandoffBoardControls board, int i) { var t = MarkLabelText(board, i); return t ? t.text : "?"; }
    static Vector2 MarkLabelPos(HandoffBoardControls board, int i) { var t = MarkLabelText(board, i); return t ? t.rectTransform.anchoredPosition : Vector2.zero; }

    // ---- helpers ----------------------------------------------------------------------------

    static bool BusesAre(HandoffBoardState s, HandoffBoardState.BusMode mode, int fill)
    {
        foreach (var b in s.buses) if (b.mode != mode || b.fill != fill) return false;
        return true;
    }

    static bool BusesAreMode(HandoffBoardState s, HandoffBoardState.BusMode mode)
    {
        foreach (var b in s.buses) if (b.mode != mode) return false;
        return true;
    }

    static string SigNote(HandoffBoardControls board)
    {
        var panel = board.GetComponent<HandoffSensorsPanel>();
        var f = typeof(HandoffSensorsPanel).GetField("sigNote", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var t = panel && f != null ? f.GetValue(panel) as TMPro.TMP_Text : null;
        return t ? t.text : "?";
    }

    static bool AllFull(HandoffBoardState s)
    {
        foreach (var b in s.buses) if (b.fill < s.busMax) return false;
        return true;
    }

    static string Buses(HandoffBoardState s)
    {
        var parts = new List<string>();
        foreach (var b in s.buses) parts.Add(b.key.ToUpperInvariant() + " " + b.mode.ToString().ToUpperInvariant() + " " + b.fill);
        return string.Join(", ", parts);
    }

    static HandoffHit Hit(string action, string arg)
    {
        foreach (var h in UnityEngine.Object.FindObjectsByType<HandoffHit>(FindObjectsSortMode.None))
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
        foreach (var h in UnityEngine.Object.FindObjectsByType<HandoffHit>(FindObjectsSortMode.None))
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
