using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 33a. The board frame's controls, clickable with no game wiring: tool rail, systems keys
/// and drawer latches, helm and orders headers, periscope carriage, pause and rate keys with the
/// mission clock, the alarm strip, and the console's SCALE knob and grid-tint selector.
/// Behaviour and every value are from the handoff's reference/UIStateSpec.md and board-logic.js
/// (renderVals); colours are the design's sRGB values and are pre-encoded when applied, because
/// the board blends in gamma space (GammaBoardPresenter). Later this state is fed by the game.
/// </summary>
public class HandoffBoardControls : MonoBehaviour
{
    [System.Serializable]
    public class Dial
    {
        public RectTransform pointer;
        [Tooltip("Knob centre, in the pointer's parent anchored space.")]
        public Vector2 centre;
        public Vector2 homePosition;
        [Tooltip("CSS angle the pointer was laid out at, degrees clockwise.")]
        public float homeAngle;

        [Tooltip("The pointer's CSS transition (UIStateSpec §9.1); 0 s = none, it jumps.")]
        public HandoffEased motion = new HandoffEased();
        [System.NonSerialized] private System.Action<float> apply;

        // CSS rotate() is clockwise on screen; in Unity's y-up space that is a negative z turn.
        // The transition interpolates the angle as a number, as CSS does (no shortest-way wrap).
        public void Set(float cssDeg)
        {
            if (!pointer) return;
            if (motion == null) motion = new HandoffEased();
            if (apply == null) apply = Apply;
            motion.Set(cssDeg, apply);
        }

        void Apply(float cssDeg)
        {
            float d = (cssDeg - homeAngle) * Mathf.Deg2Rad, c = Mathf.Cos(d), s = Mathf.Sin(d);
            Vector2 v = homePosition - centre;
            pointer.anchoredPosition = centre + new Vector2(v.x * c + v.y * s, -v.x * s + v.y * c);
            pointer.localEulerAngles = new Vector3(0, 0, -cssDeg);
        }
    }

    /// <summary>Step 40. One click cue from the prototype's playTone (References/starwolf-prototype.html):
    /// a 10 ms rise to the volume, an exponential fall to 0.0001 at max(.03 s, length), cut 30 ms after
    /// the length. Made as a clip on first use, and again whenever a value changes.</summary>
    [System.Serializable]
    public class Tone
    {
        [Tooltip("Pitch, Hz.")] public float hz = 440f;
        [Tooltip("Length, s.")] public float seconds = 0.05f;
        [Tooltip("Loudness, 0–1 (the prototype's gain).")] public float volume = 0.02f;
        [Tooltip("Sawtooth instead of square.")] public bool saw;
        [System.NonSerialized] private AudioClip clip;
        [System.NonSerialized] private Vector4 made;

        public Tone() { }
        public Tone(float hz, float seconds, float volume, bool saw) { this.hz = hz; this.seconds = seconds; this.volume = volume; this.saw = saw; }

        public AudioClip Clip(string name)
        {
            var key = new Vector4(hz, seconds, volume, saw ? 1f : 0f);
            if (clip && key == made) return clip;
            int rate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 48000;
            float end = Mathf.Max(0.03f, seconds), v = Mathf.Max(volume, 1e-4f);
            int n = Mathf.Max(1, Mathf.CeilToInt((seconds + 0.03f) * rate));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate, p = t * hz % 1f;
                float g = t < 0.01f ? v * t / 0.01f : v * Mathf.Pow(1e-4f / v, Mathf.Min(1f, (t - 0.01f) / (end - 0.01f)));
                data[i] = g * (saw ? 2f * p - 1f : (p < 0.5f ? 1f : -1f));
            }
            if (clip) Object.Destroy(clip);
            clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            made = key;
            return clip;
        }
    }

    [Tooltip("The board's own state and rules (board-logic.js). Panels read it.")]
    [SerializeField] private HandoffBoardState state = new HandoffBoardState();
    [Tooltip("Our ship's navigation (the NavSystem on OwnShip): its position, heading, throttle and movement.")]
    [UnityEngine.Serialization.FormerlySerializedAs("helm")]
    [SerializeField] private NavSystem nav;
    [Tooltip("Our ship's power (the PowerSystem on OwnShip): its reactor, buses and load shed.")]
    [SerializeField] private PowerSystem power;
    [Tooltip("Our ship's sensors (the SensorSystem on OwnShip): DRADIS, active scan, its signal, what it has identified.")]
    [SerializeField] private SensorSystem sensors;
    [Tooltip("Our ship's comms (the CommsSystem on OwnShip): its radio, the carrier lock and the cipher dials.")]
    [SerializeField] private CommsSystem comms;
    [Tooltip("Our ship's weapons (the WeaponSystem on OwnShip): fire control, rounds in flight and point defence.")]
    [SerializeField] private WeaponSystem weapons;
    [Tooltip("Our ship's hangar (the HangarSystem on OwnShip): squadrons, launches, fighters and assault boats.")]
    [SerializeField] private HangarSystem hangar;
    [Tooltip("Our ship's damage model (the DamageSystem on OwnShip): compartments, hull, conditions, crew and damage control.")]
    [SerializeField] private DamageSystem damage;
    [Tooltip("Our ship's orders station (the OrdersSystem on OwnShip): alert condition, AUX order pool and flicking rockers.")]
    [SerializeField] private OrdersSystem orders;
    [SerializeField] private HandoffBoardMotion motion;
    [SerializeField] private HandoffPlotGrid grid;
    [SerializeField] private Canvas canvas;
    [Tooltip("The board's colours: Assets/UI/Board/Palette.asset. The panels use the same file.")]
    [SerializeField] private BoardPalette palette;

    [Header("Tool rail — TOOLS, UIStateSpec §8.4")]
    [SerializeField] private string[] tools = { "CURSOR", "POINT", "RULER", "COMPASS", "ERASE" };
    [SerializeField] private RectTransform[] toolKeys;
    [SerializeField] private Image[] toolLamps;
    [SerializeField] private RectTransform selectedFrame;
    [SerializeField] private RectTransform selectedBar;
    Color toolLampOn => BoardPalette.Or(palette).phosphor;
    Color toolLampOff => BoardPalette.Or(palette).ringUnlit;

    [Header("Systems rail — one key per drawer, lamp lit while it is out")]
    [SerializeField] private string[] systemDrawers = { "fire control", "sensors", "power", "hangar", "engineering", "comms" };
    [SerializeField] private Image[] systemLamps;

    [Header("Time control")]
    [SerializeField] private TMP_Text clock;
    [SerializeField] private float missionSeconds = 7412f;
    [Tooltip("tickPower runs every 420 ms; the clock advances by the rate in seconds each tick while running.")]
    [Tune("Board & time")]
    [SerializeField] private float tickSeconds = 0.42f;
    [SerializeField] private bool paused = true;
    [SerializeField] private float rate = 1f;
    [SerializeField] private Image pauseKey;
    [SerializeField] private Sprite pauseKeyRun, pauseKeyPaused;
    [SerializeField] private TMP_Text pauseLabel;
    [SerializeField] private Image pauseLamp, pauseGlow;
    Color pauseInkOn => BoardPalette.Or(palette).caution;
    Color pauseInkOff => BoardPalette.Or(palette).secondary;
    Color pauseLampOn => BoardPalette.Or(palette).caution;
    Color pauseLampOff => BoardPalette.Or(palette).stencilDim;
    [SerializeField] private float[] rates = { 0.25f, 1f, 5f, 10f };
    [SerializeField] private Image[] rateKeys;
    [SerializeField] private TMP_Text[] rateLabels;
    [SerializeField] private Sprite rateKeyOn, rateKeyOff;
    Color rateInkOn => BoardPalette.Or(palette).data;
    Color rateInkOff => BoardPalette.Or(palette).secondary;

    [Header("Alarm strip — new contacts since the last acknowledgement (Step 44)")]
    [SerializeField] private GameObject[] alarmParts;
    [Tooltip("The strip's count (\"3\") and its words (\"NEW CONTACTS SINCE COMMIT\").")]
    [SerializeField] private TMP_Text alarmCount, alarmText;

    [Header("Sound — the prototype's click cues (audioCue tab, power, auxfail)")]
    [Tune("Sound")] [SerializeField] private Tone tabSound = new Tone(560f, 0.035f, 0.018f, false);
    [Tune("Sound")] [SerializeField] private Tone switchSound = new Tone(310f, 0.045f, 0.022f, false);
    [Tune("Sound")] [SerializeField] private Tone refusedSound = new Tone(150f, 0.08f, 0.025f, true);
    [Tooltip("A new contact raises the alarm strip (the prototype's alert cue).")]
    [Tune("Sound")] [SerializeField] private Tone alertSound = new Tone(170f, 0.42f, 0.045f, true);
    [Tooltip("The railgun fires (the prototype's railgun cue).")]
    [Tune("Sound")] [SerializeField] private Tone railgunSound = new Tone(90f, 0.25f, 0.05f, true);
    [Tooltip("A slug lands: a hit, or a miss at the end of its range (Step 48).")]
    [Tune("Sound")] [SerializeField] private Tone hitSound = new Tone(680f, 0.16f, 0.035f, false);
    [Tune("Sound")] [SerializeField] private Tone missSound = new Tone(150f, 0.08f, 0.025f, true);
    [Tooltip("A ship destroyed (the prototype's kill cue, its first tone).")]
    [Tune("Sound")] [SerializeField] private Tone killSound = new Tone(120f, 0.28f, 0.04f, true);
    [Tooltip("The DRADIS sweep tags a ship: with ACTIVE on, every pass over it (owner 2026-10-25, Step 69).")]
    [Tune("Sound")] [SerializeField] private Tone tagSound = new Tone(880f, 0.05f, 0.02f, false);
    [SerializeField] private AudioSource voice;
    // the prototype's "tab" cue is a view change: the rails, the latches and the station headers
    static readonly string[] TabActions = { "tool", "drawer", "helm", "orders", "scope" };

    [Header("Console — SCALE knob and grid tint")]
    [SerializeField, Range(0.1f, 1f)] private float zoom = 1f;
    [Tooltip("Full zoom-out (owner 2026-10-25). If the battle space is too narrow to fill the plot's width at this zoom, zooming stops sooner, where it does.")]
    [Tune("Board & time")]
    [SerializeField, Range(0.1f, 1f)] private float zoomOutLimit = 0.35f;
    [Tooltip("SCALE knob: zoom x10 per this many px of drag (UIStateSpec §8.2).")]
    [Tune("Board & time")]
    [SerializeField] private float zoomDragPxPerDecade = 160f;
    [Tooltip("Plot wheel: zoom x this per notch (§8.3).")]
    [Tune("Board & time")]
    [SerializeField] private float wheelStep = 1.12f;
    [SerializeField] private Dial scaleDial;
    [SerializeField] private TMP_Text zoomLabel;
    [SerializeField] private Dial tintDial;
    [SerializeField] private Image tintLampWhite, tintLampAmber;
    [SerializeField] private TMP_Text tintLabelWhite, tintLabelAmber;
    Color tintInkOn => BoardPalette.Or(palette).data;
    Color tintInkOff => BoardPalette.Or(palette).stencil;
    Color tintLampOn => BoardPalette.Or(palette).data;
    Color tintLampOff => BoardPalette.Or(palette).lampOffSelector;

    // Other objects can read State in their own Awake, before ours runs, so it hands our ship its systems first.
    public HandoffBoardState State { get { if (state.nav == null) UseSystems(); return state; } }
    void UseSystems() { state.UseNav(nav); state.UsePower(power); state.UseSensors(sensors); state.UseComms(comms); state.UseWeapons(weapons); state.UseHangar(hangar); state.UseDamage(damage); state.UseOrders(orders); }
    /// <summary>Raised once per board tick, after the board's own rules and before the panels
    /// redraw. The tactical world steps here, inside the board's single timer (Step 34).</summary>
    public event System.Action Ticked;
    public HandoffBoardMotion Motion => motion;
    public string Tool { get; private set; }
    public bool Paused { get { return paused; } }
    public float Rate { get { return rate; } }
    /// <summary>Step 75: game seconds since the last tick, up to one tick's worth; 0 while paused. Ships are drawn this far
    /// along between ticks.</summary>
    public float SinceTick => paused ? 0f : Mathf.Clamp01((Time.unscaledTime - (nextTick - tickSeconds)) / tickSeconds) * rate;
    public float Zoom { get { return zoom; } }
    /// <summary>The zoom as the browser holds it (a double, rounded to 0.01).</summary>
    public double ZoomExact { get { return zoomExact; } }
    private double zoomExact = 1;
    public float MissionSeconds { get { return missionSeconds; } }
    /// <summary>Click cues played so far, and the last one ("tab", "switch", "refused"). The playtest reads these.</summary>
    public int Sounds { get; private set; }
    public string LastSound { get; private set; }
    /// <summary>The board's second grid tint ("RED" in the design, amber here).</summary>
    public bool GridAmber { get { return grid && grid.GridTint == HandoffPlotGrid.Tint.Amber; } }

    private Vector2 barHome, frameHome;
    private float nextTick;
    private HandoffPanel[] panels = new HandoffPanel[0];
    private HandoffRockers rockers;
    private int shotsHeard, resultsHeard, killsHeard, hitsTakenHeard, boatWarningsHeard;
    private bool edgeWarned, trackedWarned;

    private void Awake()
    {
        if (!nav) Debug.LogError("[HandoffBoardControls] our ship's NavSystem is not set");
        if (!power) Debug.LogError("[HandoffBoardControls] our ship's PowerSystem is not set");
        if (!sensors) Debug.LogError("[HandoffBoardControls] our ship's SensorSystem is not set");
        if (!comms) Debug.LogError("[HandoffBoardControls] our ship's CommsSystem is not set");
        if (!weapons) Debug.LogError("[HandoffBoardControls] our ship's WeaponSystem is not set");
        if (!hangar) Debug.LogError("[HandoffBoardControls] our ship's HangarSystem is not set");
        if (!damage) Debug.LogError("[HandoffBoardControls] our ship's DamageSystem is not set");
        if (!orders) Debug.LogError("[HandoffBoardControls] our ship's OrdersSystem is not set");
        UseSystems();
    }

    private void Start()
    {
        frameHome = selectedFrame ? selectedFrame.anchoredPosition : Vector2.zero;
        barHome = selectedBar ? selectedBar.anchoredPosition : Vector2.zero;
        if (motion) motion.Changed += ShowSystems;
        panels = GetComponentsInChildren<HandoffPanel>(true); // a panel can sit on its own station object (a prefab)
        rockers = GetComponent<HandoffRockers>();
        foreach (var p in panels) p.Bind(this);
        RefreshPanels();
        PickTool(tools[0]);
        ShowTime();
        ShowSystems();
        SetZoom(zoom);
        ShowTint();
        ShowAlarm();
        state.missionNow = missionSeconds;
        nextTick = Time.unscaledTime + tickSeconds;
    }

    private void OnDestroy() { if (motion) motion.Changed -= ShowSystems; HandoffEased.ClearAll(); }

    private void Update()
    {
        // The board's single timer (tickPower). Only the mission clock is board-local so far.
        // thrown repair and order rockers fall back after 400 ms (flickBack)
        if (state.orders.ExpireFlicks(Time.unscaledTime)) RefreshPanels();
        // a shot fired (from the lever) and a shot landing (in the tick) each get their cue
        if (state.weapons.shotsFired != shotsHeard) { shotsHeard = state.weapons.shotsFired; Play(railgunSound, "railgun"); }
        if (state.weapons.shotResults != resultsHeard)
        {
            resultsHeard = state.weapons.shotResults;
            if (state.kills != killsHeard) { killsHeard = state.kills; Play(killSound, "kill"); }
            else Play(state.weapons.lastShotHit ? hitSound : missSound, state.weapons.lastShotHit ? "hit" : "miss");
        }
        if (state.weapons.hitsTaken != hitsTakenHeard) { hitsTakenHeard = state.weapons.hitsTaken; Play(alertSound, "alert"); } // Step 55: an enemy missile hit us
        if (state.hangar.boatWarnings != boatWarningsHeard) { boatWarningsHeard = state.hangar.boatWarnings; Play(alertSound, "alert"); } // owner 2026-10-27: boats stranded, or lost
        HandoffEased.TickAll(); // control transitions (Step 33j)
        // setInterval drops ticks it missed during a stall; do the same rather than bursting
        if (Time.unscaledTime - nextTick > tickSeconds) nextTick = Time.unscaledTime;
        while (Time.unscaledTime >= nextTick)
        {
            nextTick += tickSeconds;
            // Owner 2026-10-17: nothing real changes while paused (spec Rule 2.d), so the whole tick
            // waits — power, charges, signal, the bow, the world. The design ran tickPower while paused
            // (UIStateSpec §11). Controls still click; panels still redraw.
            if (!paused)
            {
                missionSeconds += rate; ShowClock();
                state.missionNow = missionSeconds; // the solution's clock (Step 52)
                state.power.Tick(rate);
                state.orders.TickAux(rate);
                state.nav.Tick(rate); // the ship steers and moves; it reads the engine room before a repair lands this tick, as tickPower does
                // Step 65: the battle space. Crossing out drops time to x1, so a fast rate can't kill you in a blink
                if (state.TickArena(rate)) { rate = 1f; ShowTime(); Play(alertSound, "alert"); }
                bool nearEdge = state.NearArenaEdge || state.outsideFor >= 0f;
                if (nearEdge && !edgeWarned) Play(alertSound, "alert");
                edgeWarned = nearEdge;
                ShowAlarm();
                state.weapons.TickRounds(rate); // Step 48: slugs fly, and hit or run out of range
                state.hangar.TickFlights(rate); // Step 54: fighters fly, attack, come home
                state.weapons.TickIncoming(rate); // Step 55: enemy missiles home on us; our PDCs on AUTO take their chances
                state.damage.TickParty(rate);
                state.damage.TickConditions(rate); // Step 59: fire, vacuum and radiation burn, kill and spread
                state.hangar.TickLaunch(rate);
                state.weapons.TickCharge(rate);
                state.sensors.Tick(rate);
                // Step 68 (owner 2026-10-25): the alert tone once as our signal crosses the tracked line
                bool tracked = state.sensors.signal >= state.sensors.stats.signalTrackedAt;
                if (tracked && !trackedWarned) Play(alertSound, "alert");
                trackedWarned = tracked;
                if (Ticked != null) Ticked();
                state.NoteWrecks(); // Step 59: a contact whose reactor burned out is a wreck
                if (state.sensors.NoteNewContacts()) { ShowAlarm(); Play(alertSound, "alert"); }
            }
            RefreshPanels();
        }
    }

    // ---- input from HandoffHit -----------------------------------------------------------

    public void Click(string action, string arg)
    {
        bool refused = rockers && rockers.Refuse(action, arg); // reads the rocker as clicked, before the click changes it
        int orderRefusals = state.orders.orderRefusals;
        bool owned = true;
        switch (action)
        {
            case "tool": if (arg == "ERASE") PressErase(); else PickTool(arg); RefreshPanels(); break;
            case "drawer": motion.ToggleDrawer(arg); RefreshPanels(); break; // the plot shows the weapon ring while FIRE is open (Step 46)
            case "helm": motion.ToggleHelm(); break;
            case "orders": motion.ToggleOrders(); break;
            case "scope": motion.ToggleScope(); RefreshPanels(); break;
            case "pause": paused = !paused; ShowTime(); break;
            case "rate": rate = float.Parse(arg, System.Globalization.CultureInfo.InvariantCulture); paused = false; ShowTime(); break;
            case "ack": state.sensors.AckContacts(); ShowAlarm(); break;
            case "tint": grid.GridTint = grid.GridTint == HandoffPlotGrid.Tint.White ? HandoffPlotGrid.Tint.Amber : HandoffPlotGrid.Tint.White; ShowTint(); RefreshPanels(); break;
            default:
                owned = false;
                foreach (var p in panels) if (p.Click(action, arg)) { owned = true; break; }
                RefreshPanels();
                break;
        }
        // a refused order throws and flicks back as the design has it; it still sounds refused (owner)
        if (refused || state.orders.orderRefusals != orderRefusals) Play(refusedSound, "refused");
        else if (System.Array.IndexOf(TabActions, action) >= 0) Play(tabSound, "tab");
        else if (owned) Play(switchSound, "switch");
    }

    /// <summary>Step 69: sweep tags so far. Counted apart from <see cref="Sounds"/>, so a tag never stands in for
    /// the click cue a check waits on. The playtest reads it.</summary>
    public int Tags { get; private set; }
    public void SweepTag() { Tags++; if (voice) voice.PlayOneShot(tagSound.Clip("tag")); }

    void Play(Tone t, string name)
    {
        Sounds++;
        LastSound = name;
        if (voice) voice.PlayOneShot(t.Clip(name));
    }

    void RefreshPanels() { foreach (var p in panels) p.Refresh(); }

    /// <summary>A screen point in board px (1920x1080 layout space, top-left origin, y down).</summary>
    public Vector2 BoardPoint(Vector2 screen)
    {
        var rt = (RectTransform)transform;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screen, canvas ? canvas.worldCamera : null, out local);
        return new Vector2(local.x + rt.rect.width / 2f, rt.rect.height / 2f - local.y);
    }

    private double dragZoom0;

    public void Down(string action, string arg, Vector2 screen)
    {
        if (action == "zoom") { dragZoom0 = zoomExact; return; }
        foreach (var p in panels) if (p.Down(action, arg, BoardPoint(screen))) { RefreshPanels(); return; }
    }

    public void Up(string action, string arg, Vector2 screen)
    {
        foreach (var p in panels) if (p.Up(action, arg, BoardPoint(screen))) { RefreshPanels(); return; }
    }

    // scaleDown: zoom = z0 x 10^((x - x0) / 160), measured from the press, not frame to frame —
    // per-frame steps would be rounded away by setZoom's 0.01 rounding (Step 33a verification).
    public void Drag(string action, string arg, Vector2 sincePress, Vector2 screen)
    {
        if (action != "zoom")
        {
            foreach (var p in panels) if (p.Drag(action, arg, BoardPoint(screen))) { RefreshPanels(); return; }
            return;
        }
        float px = sincePress.x / (canvas ? canvas.scaleFactor : 1f); // board px
        SetZoom(dragZoom0 * System.Math.Pow(10, px / (double)zoomDragPxPerDecade));
    }

    public void Scroll(string action, string arg, float notches)
    {
        // marks and plot labels sit inside the plot element, so the wheel still zooms over them
        if ((action != "plot" && !action.StartsWith("plot.")) || notches == 0f) return;
        // in double, as the browser does: 0.70 / 1.12 is 0.62499… and rounds to 0.62 (Step 33i verification)
        double step = System.Math.Round((double)wheelStep, 6);
        SetZoom(zoomExact * (notches > 0f ? step : 1 / step));
    }

    // ---- tool rail -----------------------------------------------------------------------

    public void PickTool(string tool)
    {
        Tool = tool;
        state.draft = null; // pickTool clears any draft
        int sel = System.Array.IndexOf(tools, tool);
        for (int i = 0; i < toolLamps.Length; i++) Tint(toolLamps[i], i == sel ? toolLampOn : toolLampOff);
        if (sel >= 0 && toolKeys.Length > sel && toolKeys[0])
        {
            Vector2 step = toolKeys[sel].anchoredPosition - toolKeys[0].anchoredPosition;
            if (selectedFrame) selectedFrame.anchoredPosition = frameHome + step;
            if (selectedBar) selectedBar.anchoredPosition = barHome + step;
        }
    }

    // ERASE again while ERASE is live clears the plot and returns to CURSOR (§8.4).
    void PressErase()
    {
        if (Tool == "ERASE") { state.marks.Clear(); state.sensors.BearingFixes.Clear(); PickTool(tools[0]); }
        else PickTool("ERASE");
    }

    // ---- alarm strip ---------------------------------------------------------------------

    void ShowAlarm()
    {
        // Step 65: the battle space's warnings take the strip over; its count shows the seconds left
        string edge = state.deserted ? "SHIP LOST — LEFT THE BATTLE SPACE"
                    : state.outsideFor >= 0f ? "EXITING THE BATTLE SPACE — TURN BACK"
                    : state.NearArenaEdge ? "APPROACHING THE EDGE OF THE BATTLE SPACE" : null;
        bool on = edge != null || state.sensors.newContacts > 0;
        foreach (var p in alarmParts) if (p && p.activeSelf != on) p.SetActive(on);
        if (!on) return;
        if (alarmCount) alarmCount.text = edge == null ? state.sensors.newContacts.ToString()
            : state.outsideFor >= 0f && !state.deserted ? Mathf.CeilToInt(state.arenaReturnSeconds - state.outsideFor).ToString() : "!";
        if (alarmText) alarmText.text = edge ?? (state.sensors.newContacts == 1 ? "NEW CONTACT SINCE COMMIT" : "NEW CONTACTS SINCE COMMIT");
    }

    // ---- systems rail --------------------------------------------------------------------

    void ShowSystems()
    {
        string open = motion ? motion.OpenDrawerName : null;
        for (int i = 0; i < systemLamps.Length && i < systemDrawers.Length; i++)
            Tint(systemLamps[i], systemDrawers[i] == open ? toolLampOn : toolLampOff);
    }

    // ---- time control --------------------------------------------------------------------

    void ShowTime()
    {
        if (pauseKey) pauseKey.sprite = paused ? pauseKeyPaused : pauseKeyRun;
        if (pauseLabel) { pauseLabel.text = paused ? "PAUSED" : "RUNNING"; Tint(pauseLabel, paused ? pauseInkOn : pauseInkOff); }
        Tint(pauseLamp, paused ? pauseLampOn : pauseLampOff);
        if (pauseGlow) pauseGlow.gameObject.SetActive(paused);
        for (int i = 0; i < rateKeys.Length && i < rates.Length; i++)
        {
            bool on = !paused && Mathf.Approximately(rate, rates[i]);
            if (rateKeys[i]) rateKeys[i].sprite = on ? rateKeyOn : rateKeyOff;
            if (i < rateLabels.Length) Tint(rateLabels[i], on ? rateInkOn : rateInkOff);
        }
        ShowClock();
    }

    void ShowClock()
    {
        if (!clock) return;
        int t = Mathf.FloorToInt(missionSeconds);
        clock.text = (t / 3600 % 100).ToString("00") + ":" + (t / 60 % 60).ToString("00") + ":" + (t % 60).ToString("00");
    }

    // ---- console -------------------------------------------------------------------------

    /// <summary>Step 65 (owner 2026-10-25): zooming out stops at `zoomOutLimit`, or sooner where the battle space's left
    /// and right edges reach the plot's sides (1712 px at 60.9 px per 1,000 km), so there's never black down the sides.
    /// A 100,000 km map fills at x0.28, past the x0.35 limit; only one under about 80,000 km wide stops sooner. The top
    /// and bottom run on past the plot and you pan to them. The design went to 0.1.</summary>
    public double MinZoom => System.Math.Max(zoomOutLimit, System.Math.Min(1.0, (grid ? grid.rectTransform.rect.width : 1712.0) / (state.arenaWidthKm * 60.9 / 1000.0)));

    public void SetZoom(double z)
    {
        // z*100 in double, as the browser does, so near-half products round the same way; the
        // exact double is kept for the next step, the float only for display and the grid
        zoomExact = System.Math.Max(MinZoom, System.Math.Min(1, System.Math.Floor(z * 100.0 + 0.5) / 100));
        zoom = (float)zoomExact;
        if (grid) grid.Zoom = zoom;
        if (zoomLabel) zoomLabel.text = "x" + (zoom >= 1f ? "1.0" : zoom.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
        scaleDial.Set(-140f + Mathf.Log10(zoom / 0.1f) * 280f); // knobAngle
        RefreshPanels(); // the plot (marks, contacts, our ship) follows the zoom at once
    }

    void ShowTint()
    {
        bool white = !grid || grid.GridTint == HandoffPlotGrid.Tint.White;
        tintDial.Set(white ? 60f : 120f); // tintAngle
        Tint(tintLampWhite, white ? tintLampOn : tintLampOff);
        Tint(tintLampAmber, white ? tintLampOff : tintLampOn);
        Tint(tintLabelWhite, white ? tintInkOn : tintInkOff);
        Tint(tintLabelAmber, white ? tintInkOff : tintInkOn);
    }

    // ---- colour --------------------------------------------------------------------------

    // Design colours are sRGB; pre-encode so the canvas's linear conversion lands on them,
    // keeping the graphic's own alpha.
    static void Tint(Graphic g, Color design)
    {
        if (!g) return;
        var c = design.gamma;
        c.a = g.color.a;
        g.color = c;
    }
}
