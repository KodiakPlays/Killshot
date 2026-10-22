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
/// Unity Recorder. Output: Logs/Playtests/&lt;time&gt;/ — playtest.mp4, report.md, shots/*.png.
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

    static void Begin(string stamp)
    {
        runDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/Playtests/" + stamp));
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
        yield return 1f;
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
        HandoffHit target = null;
        foreach (var h in UnityEngine.Object.FindObjectsByType<HandoffHit>(FindObjectsSortMode.None))
            if (h.action == action && (h.arg ?? "") == (arg ?? "") && h.isActiveAndEnabled) { target = h; break; } // Unity saves a null arg as ""
        if (target == null) { Check("Click " + label, false, "control not found"); return false; }
        var rt = (RectTransform)target.transform;
        var canvas = target.GetComponentInParent<Canvas>().rootCanvas;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rt.TransformPoint(rt.rect.center));
        var es = EventSystem.current;
        var ped = new PointerEventData(es) { position = screen, pressPosition = screen, button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>();
        es.RaycastAll(ped, hits);
        GameObject top = hits.Count > 0 ? hits[0].gameObject : null;
        var handler = top ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(top) : null;
        if (handler != target.gameObject)
        {
            Check("Click " + label, false, "blocked: topmost at " + screen + " is " + (top ? top.name : "nothing"));
            return false;
        }
        ped.pointerCurrentRaycast = ped.pointerPressRaycast = hits[0];
        if (down) ExecuteEvents.Execute(handler, ped, ExecuteEvents.pointerDownHandler);
        if (up)
        {
            ExecuteEvents.Execute(handler, ped, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(handler, ped, ExecuteEvents.pointerClickHandler);
        }
        return true;
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
