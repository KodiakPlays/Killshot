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
        }
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
        if (!Click("drawer", "power", "PWR key")) yield break;
        yield return 0.8f;
        var drawer = GameObject.Find("State · power");
        Check("The Power drawer opens", drawer != null && drawer.activeInHierarchy, "");
        Shot("04_power_open");
        yield return 0.2f;

        // 7. While paused, set every bus to DRAW. Power waits (owner 2026-10-17: nothing real changes
        //    while paused). Setting all four first also shares the reactor evenly: it serves buses
        //    first come, first served, so a bus sent to DRAW late can find the reactor already spent.
        string[] names = { "ENG", "WEP", "SEN", "AUX" };
        for (int i = 0; i < 4; i++)
        {
            if (!Click("power.bus", i.ToString(), names[i] + " selector")) yield break;
            Check(names[i] + " bus set to DRAW", s.buses[i].mode == HandoffBoardState.BusMode.Draw, s.buses[i].mode.ToString());
            yield return 0.3f;
        }
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
        if (!Click("power.shed", null, "EMERGENCY LOAD SHED")) yield break;
        Check("LOAD SHED vents every bus", BusesAreMode(s, HandoffBoardState.BusMode.Vent), Buses(s));
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
        if (!Click("tool", "POINT", "POINT tool")) yield break;
        if (!PlotDrag(board, new Vector2(1100, 400), new Vector2(1100, 400), "drop a POINT")) yield break;
        yield return 0.2f;
        if (!Click("tool", "RULER", "RULER tool")) yield break;
        if (!PlotDrag(board, new Vector2(700, 700), new Vector2(820, 640), "draw a RULER")) yield break;
        yield return 0.2f;
        if (!Click("tool", "COMPASS", "COMPASS tool")) yield break;
        if (!PlotDrag(board, new Vector2(1200, 545), new Vector2(1300, 445), "draw a COMPASS")) yield break;
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

        //     Move our ship 5,000 km east. Stand-in for the helm, which is not built yet.
        var own = GameObject.Find("World").transform.Find("OwnShip");
        Vector3 home = own.position;
        own.position = home + new Vector3(5000f, 0f, 0f);
        double shift = 5000.0 * 60.9 / 1000.0 * board.ZoomExact; // px the world slides left on the plot
        yield return 1.2f; // a few board ticks
        string pt1 = MarkLabel(board, 0), ru1 = MarkLabel(board, 1), co1 = MarkLabel(board, 2);
        Vector2 ptPos1 = MarkLabelPos(board, 0), ruPos1 = MarkLabelPos(board, 1);
        Check("The board knows our ship moved", Mathf.Abs((float)s.ownX - home.x - 5000f) < 0.01f, "ownX " + s.ownX.ToString("0") + " km");
        Check("The POINT stays put in space", Mathf.Abs(ptPos0.x - ptPos1.x - (float)shift) < 0.5f && Mathf.Abs(ptPos0.y - ptPos1.y) < 0.5f && pt1 == pt0,
              "moved " + (ptPos1.x - ptPos0.x).ToString("0.0") + " px on the plot (expected " + (-shift).ToString("0.0") + "), label unchanged");
        // the ruler label snaps to whole px from our ship, so it can land half a px either side
        Check("The RULER stays put in space", Mathf.Abs(ruPos0.x - ruPos1.x - (float)shift) <= 0.51f && ru1 == ru0,
              "moved " + (ruPos1.x - ruPos0.x).ToString("0.0") + " px, still \"" + ru1 + "\"");
        Check("The COMPASS re-measures from our ship", co1 != co0 && co1.StartsWith("RNG "), "\"" + co0 + "\" → \"" + co1 + "\"");
        Shot("11_ship_moved");
        yield return 0.2f;
        own.position = home;
        yield return 1.2f;
        Check("Moving back restores every label", MarkLabel(board, 0) == pt0 && MarkLabel(board, 1) == ru0 && MarkLabel(board, 2) == co0, "");

        //     ERASE pressed twice clears the plot
        if (!Click("tool", "ERASE", "ERASE tool")) yield break;
        if (!Click("tool", "ERASE", "ERASE tool (again)")) yield break;
        yield return 0.3f;
        Check("ERASE twice clears every mark", s.marks.Count == 0, s.marks.Count + " marks left");
        Shot("12_cleared");
        yield return 1f;
    }

    /// <summary>Press at board px a, release at board px b, on the plot, through the EventSystem.
    /// The plot's capture surface must be on top there.</summary>
    static bool PlotDrag(HandoffBoardControls board, Vector2 a, Vector2 b, string label)
    {
        var es = EventSystem.current;
        Vector2 sa = BoardToScreen(board, a), sb = BoardToScreen(board, b);
        var ped = new PointerEventData(es) { position = sa, pressPosition = sa, button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>();
        es.RaycastAll(ped, hits);
        var top = hits.Count > 0 ? ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject) : null;
        var hh = top ? top.GetComponent<HandoffHit>() : null;
        if (hh == null || hh.action != "plot")
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

    /// <summary>Click a board control where it is on screen, through the EventSystem. Fails if
    /// the control can't be found or something else is on top of it.</summary>
    static bool Click(string action, string arg, string label)
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
        ExecuteEvents.Execute(handler, ped, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(handler, ped, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(handler, ped, ExecuteEvents.pointerClickHandler);
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
