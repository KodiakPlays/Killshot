using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 33i. The plot tools and plot clicks: POINT, RULER, COMPASS marks with their labels, ERASE
/// (click a mark to delete it), the capture surface that takes the pointer only for the three
/// marking tools, and the passive bearing label that puts the periscope on its contact (lookAt).
/// Rules and values from board-logic.js (plotPt, plotDown, plotMove, plotUp, eraseMark,
/// pressErase, lookAt, pickTarget, validSet; renderVals marksView, plotCapture, passives) and
/// UIStateSpec §8.4.
/// </summary>
public class PlotPanel : Panel
{
    [System.Serializable]
    public class Passive
    {
        public string key;
        public TMP_Text label;
    }

    [SerializeField] private Graphic capture;
    [SerializeField] private MarksGraphic marks;
    [SerializeField] private TMP_Text labelTemplate;
    [SerializeField] private RectTransform hitLayer;
    [SerializeField] private Passive[] passives;

    [Header("Our ship on the plot (Step 37): the 3D model seen from above (ShipView)")]
    [SerializeField] private RawImage ownShipImage;
    [Tooltip("The ship's length on screen at zoom 1×: the design's own-ship glyph is a 34×76 px hull.")]
    [Tune("Plot & our ship")]
    [SerializeField] private float ownShipPxAtFullZoom = 76f;
    [Tooltip("It shrinks with zoom like the design's glyph, but never below this (older handoff: legible at 30 px) "
           + "and never above the maximum (owner 2026-10-17).")]
    [Tune("Plot & our ship")]
    [SerializeField] private float ownShipMinPx = 30f, ownShipMaxPx = 120f;
    Color ownShipColor => Palette.ownShip;
    private ShipView shipView;

    [Header("Ruins on the plot (owner 2026-10-27): the level's terrain within visual range, from above, behind the marks")]
    [Tooltip("Covers the plot. Its own camera draws the WorldTerrain layer at the plot's pan and zoom (TacticalWorld shows only the pieces within visual range). "
           + "Needs the BoardLinearImage material, as our ship's view: the line art comes red on black, and the material keys the black out.")]
    [SerializeField] private RawImage ruinsImage;
    Color ruinsColor => Palette.secondary;
    private Camera ruinsCamera;
    private RenderTexture ruinsTexture;

    [Header("Sensor circle (Step 42): what DRADIS can hear, around our ship")]
    [Tooltip("Owner 2026-10-18: while DRADIS is on, a circle around our ship at the sensor (hearing) range.")]
    [SerializeField] private MarksGraphic sensorRing;
    [Tooltip("The circle eases to a new range (UIStateSpec §9.1, lines 204 and 210: .5 s).")]
    [Tune("Sensors")] [SerializeField] private Eased sensorRingMotion = new Eased(0.5f, CssEase.Ease);
    [Tooltip("senEdge / senGlow: the ring and the fill inside it; white grid ink.data .26 / .04, amber grid alarm .3 / .05.")]
    [SerializeField] private float senEdgeAlpha = 0.26f, senGlowAlpha = 0.04f, senEdgeAlphaAmber = 0.3f, senGlowAlphaAmber = 0.05f;
    private float sensorKm;
    private System.Action<float> setSensorKm;
    [Tooltip("Owner 2026-10-26: the active scan cone, while DRADIS and ACTIVE are on: bright edges and a faint fill, from the small circle round our ship out to the scan's range.")]
    [Tune("Sensors")] [SerializeField] private float coneEdgeAlpha = 0.6f, coneFillAlpha = 0.07f;
    private float coneKm;
    private Eased coneMotion; // eases like the sensor circle
    private System.Action<float> setConeKm;

    [Header("Radar sweep (Step 43): turns inside the sensor circle while DRADIS is on")]
    [SerializeField] private SweepGraphic sweep;
    [Tooltip("One turn takes base − perGain × gain seconds (sweepDur: 5.4 − senBoost × .5). It holds still while the board is paused (owner rule).")]
    [Tune("Sensors")] [SerializeField] private float sweepSecondsBase = 5.4f, sweepSecondsPerGain = 0.5f;
    private float sweepDeg;
    [Tooltip("The small circle round our ship (owner 2026-10-24) sits this far outside the ship's picture, px; the sweep starts there.")]
    [Tune("Sensors")] [SerializeField] private float innerRingMarginPx = 8f;
    /// <summary>The inner circle's radius on screen: half our ship's drawn length plus the margin.</summary>
    float InnerRingPx => Mathf.Clamp(ownShipPxAtFullZoom * (float)Controls.ZoomExact, ownShipMinPx, ownShipMaxPx) / 2f + innerRingMarginPx;
    /// <summary>The sweep's bearing now, degrees clockwise from north; and whether it shows.</summary>
    public float SweepDeg => sweepDeg;
    public bool SweepShown => sweep && sweep.isActiveAndEnabled;
    /// <summary>The sensor circle's radius now, km (it eases), or 0 while DRADIS is off or SEN has no power.</summary>
    public float SensorRingKm => State != null && State.sensors.Listening ? sensorKm : 0f;

    [Header("Passive bearings (Step 44): contacts DRADIS hears but has not identified")]
    [SerializeField] private MarksGraphic passiveLines;
    [Tooltip("A hidden plot label carrying a plot.look hit (the export's text #39), cloned per heard contact.")]
    [SerializeField] private TMP_Text passiveLabelTemplate;
    [Tooltip("The bearing line runs out to the sensor circle; its label sits this far out, or at the circle if nearer.")]
    [Tune("Sensors")] [SerializeField] private float passiveLabelPx = 200f;
    [Tooltip("How strong a fresh trail line is (Step 45); it fades to nothing.")]
    [Tune("Sensors")] [SerializeField] private float trailAlpha = 0.45f;
    private readonly List<TMP_Text> passiveLabels = new List<TMP_Text>();

    [Header("Contacts on the plot (Step 34): identified hostiles at their true range and bearing")]
    [Tooltip("Stand-in art until Claude Design's contact glyphs arrive (CLAUDE_DESIGN_NEEDS 1.2).")]
    [SerializeField] private MarksGraphic contactMarks;
    [SerializeField] private TMP_Text contactLabelTemplate;
    [Tooltip("Owner 2026-10-26 (Step 75): a contact in sight (visualRangeKm) shows as its own 3D model seen from above, in "
           + "place of its dot (a ShipView camera on the contact's model): red identified, amber unknown, grey a wreck.")]
    [SerializeField] private RawImage contactShipImage;
    Color wreckInk => Palette.secondary;
    private ShipView[] shipViews;
    /// <summary>A contact's ship picture is on the plot (the playtest reads it).</summary>
    public bool ContactShipShown => contactShipImage && contactShipImage.enabled;
    [Tune("Plot & our ship")]
    [SerializeField] private Vector2 contactLabelOffset = new Vector2(10, 0);
    private readonly List<TMP_Text> contactLabels = new List<TMP_Text>();

    [Tooltip("A passive label shows only while its contact is heard and not identified (passives).")]
    [SerializeField] private bool passiveVisibility = true;

    [Header("Plot space (§8.4): 60.9 layer px = 1000 km")]
    [SerializeField] private Vector2 plotCentre = new Vector2(960, 545);
    [SerializeField] private double pxPer1000Km = 60.9;

    // Step 41 (owner 2026-10-18): the grid stays put and our ship moves across it. The plot shows the
    // world around a view centre, which starts on our ship. Drag the empty grid, or hold the arrow
    // keys, to pan.
    [Header("Panning the plot (Step 41)")]
    [SerializeField] private PlotGrid grid;
    [Tooltip("Arrow keys pan the plot this fast, screen px per second.")]
    [Tune("Plot & our ship")] [SerializeField] private float panKeysPxPerS = 600f;
    private double viewX, viewY, panX0, panY0;
    private bool viewSet, panning;
    private Vector2 panPress;
    /// <summary>The world point at the plot centre, km.</summary>
    public Vector2 View => new Vector2((float)viewX, (float)viewY);
    /// <summary>C, and a level's start point (Step 73): the view jumps to our ship. Before the board is up
    /// there is nothing to do: its first refresh centres on our ship anyway.</summary>
    public void CentreOnOurShip()
    {
        if (!Controls) return;
        viewX = State.nav.x; viewY = State.nav.y; viewSet = true;
        Refresh();
    }

    [Header("Axis numbers (Step 65; distances from our ship since Step 76)")]
    [Tooltip("The numbers are made under this, at run time (Plot/Scale).")]
    [SerializeField] private RectTransform axisLayer;
    [SerializeField] private TMP_FontAsset axisFont;
    [Tooltip("The board thickens Share Tech Mono at small sizes with its own material. Empty uses the font's own.")]
    [SerializeField] private Material axisMaterial;
    [SerializeField] private float axisFontSize = 11f;
    [SerializeField] private Color axisColour = new Color(0.7314692f, 0.759380937f, 0.7812595f, 1f);

    [Header("Labels — offsets have no value in the package (HANDOFF_GAPS)")]
    [Tune("Plot tools")]
    [SerializeField] private Vector2 pointLabelOffset = new Vector2(10, 0);
    [Tune("Plot tools")]
    [SerializeField] private Vector2 rulerLabelOffset = new Vector2(0, -12);
    [Tune("Plot tools")]
    [SerializeField] private Vector2 ringLabelOffset = new Vector2(8, 0);
    [Tooltip("ERASE click area: the point box, and the ruler's height (§8.4).")]
    [SerializeField] private float pointBox = 16f, rulerBox = 14f;
    [Tune("Plot tools")]
    [SerializeField] private float draftAlpha = 0.75f;

    [Header("Intercept helpers (Step 46, owner 2026-10-23)")]
    [Tooltip("A RULER end within this many screen px of a POINT snaps to it.")]
    [Tune("Plot tools")] [SerializeField] private float rulerSnapPx = 12f;
    [Tooltip("A track (a RULER from one POINT to another) projects its course this far ahead, mission seconds.")]
    [Tune("Plot tools")] [SerializeField] private float projectSeconds = 300f;
    [Tooltip("A slug in flight trails this many screen px behind it (Step 48).")]
    [Tune("Periscope & fire control")] [SerializeField] private float slugTrailPx = 24f;
    [Tooltip("The selected weapon's reach, a dashed ring round our ship while Fire Control is open.")]
    [SerializeField] private MarksGraphic weaponRing;
    [Tooltip("A PDC arc's curved edge is drawn in steps this many degrees long (Step 55).")]
    [SerializeField] private float pdcArcStepDeg = 2f;

    [Header("Fighters (Step 54): one small wedge per craft, white like our ship (owner 2026-10-26), no labels")]
    [Tooltip("A hidden wedge image (hull_wedge_filled, nose up; the outline vanishes this small), cloned per fighter in flight.")]
    [SerializeField] private Image fighterTemplate;
    [Tooltip("A fighter's length on screen, px.")]
    [Tune("Hangar & fighters")] [SerializeField] private float fighterPx = 16f;
    [Tooltip("Gap between fighters in the V, px.")]
    [Tune("Hangar & fighters")] [SerializeField] private float fighterGapPx = 10f;
    private readonly List<Image> fighters = new List<Image>();
    /// <summary>Fighters drawn now (the playtest reads it).</summary>
    public int FightersShown { get; private set; }

    Color ownForce => Palette.ownForce;
    Color caution => Palette.caution;
    Color alarm => Palette.alarm;
    Color data => Palette.data;
    [Tooltip("sweepEdge on the amber grid: rgb(255,150,130).")]
    [SerializeField] private Color sweepInkAmber = new Color32(0xFF, 0x96, 0x82, 0xFF);

    private readonly List<TMP_Text> labels = new List<TMP_Text>();
    private readonly List<ClickSpot> hits = new List<ClickSpot>();

    bool Capturing => Controls.Tool == "POINT" || Controls.Tool == "RULER" || Controls.Tool == "COMPASS";

    // plotPt: pointer to plot-layer px at 1x, rounded
    // the board measures a whole drag at the zoom of the press (_plot.k)
    private double pressZoom = 1;
    int LayerX(float bx) => ShipState.JsRound((bx - plotCentre.x) / pressZoom);
    int LayerY(float by) => ShipState.JsRound((by - plotCentre.y) / pressZoom);

    // Step 36: marks live in world km (x east, y north) so they stay put when our ship moves.
    // The pointer is still rounded to whole layer px first, as the board's plotPt does.
    double KmPerPx => 1000.0 / pxPer1000Km;
    double SnapKm => rulerSnapPx * KmPerPx / Controls.ZoomExact;
    double WorldX(float bx) => viewX + LayerX(bx) * KmPerPx;
    double WorldY(float by) => viewY - LayerY(by) * KmPerPx;
    /// <summary>A world point (km) in board px.</summary>
    public Vector2 Screen(double wx, double wy)
    {
        double z = Controls.ZoomExact / KmPerPx;
        return plotCentre + new Vector2((float)((wx - viewX) * z), (float)(-(wy - viewY) * z));
    }
    /// <summary>Step 75: our ship where it is drawn now (it glides between ticks), in board px.</summary>
    Vector2 OwnAt => Screen(State.nav.DrawnX, State.nav.DrawnY);

    private void Update()
    {
        if (Controls == null) return;
        if (sweep && State.sensors.Listening && !Controls.Paused)
        {
            float turn = Mathf.Max(0.5f, sweepSecondsBase - State.sensors.senBoost * sweepSecondsPerGain);
            // in game time (owner 2026-10-24): `turn` seconds a sweep at x1, ten times as fast at x10
            float before = sweepDeg, step = 360f * Time.unscaledDeltaTime * Controls.Rate / turn;
            sweepDeg = (sweepDeg + step) % 360f;
            // Step 45: the sweep leaves a bearing line on each heard contact it passes. Step 63 (owner 2026-10-24):
            // with ACTIVE on, a contact the cone covers is identified as the sweep passes it (the design needed
            // the cone dial let go, or a contact-board row clicked, and said nothing).
            // Step 69 (owner 2026-10-25): every pass over a ship the cone covers tags it with a beep, scanned or not.
            foreach (var t in State.tracks)
            {
                if (t.own || t.wreck) continue;
                float past = Mathf.Repeat((float)State.sensors.ExactBearing(t) - before, 360f);
                if (past <= 0f || past > step) continue;
                bool scanned = State.sensors.identified.Contains(t.key);
                if (State.sensors.Heard(t) || State.sensors.ConeCovers(t)) State.sensors.See(t); // Step 75: out of sight, the plot shows it here until the next pass
                if (State.sensors.ConeCovers(t)) { Controls.SweepTag(); if (!scanned) { State.sensors.identified.Add(t.key); Refresh(); } continue; }
                if (!scanned && State.sensors.Heard(t)) { State.sensors.LayBearing(t, Controls.MissionSeconds); Refresh(); }
            }
            sweep.rectTransform.localEulerAngles = new Vector3(0f, 0f, -sweepDeg); // clockwise on screen
        }
        // Step 75: ships glide between ticks, so while time runs the plot redraws every frame
        // ponytail: redraws the whole plot, as panning already does; redraw only what moves if the profiler minds
        if (!Controls.Paused) Refresh();
        // not while typing in a field (the hangar's launch bearing)
        var typing = UnityEngine.EventSystems.EventSystem.current ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject : null;
        if (typing && typing.GetComponent<TMP_InputField>()) return;
        // Step 65 (owner 2026-10-25): C centres the plot on our ship again after panning away
        if (Input.GetKeyDown(KeyCode.C)) { CentreOnOurShip(); return; }
        float x = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
        float y = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
        if (x == 0f && y == 0f) return;
        double km = panKeysPxPerS * Time.unscaledDeltaTime * KmPerPx / Controls.ZoomExact;
        viewX += x * km;
        viewY += y * km;
        Refresh();
    }

    public override bool Click(string action, string arg)
    {
        switch (action)
        {
            case "plot.erase":
                if (Controls.Tool == "ERASE") State.EraseMark(int.Parse(arg));
                return true;
            case "plot.look":
                if (State.LookAt(arg) && Controls.Motion && !Controls.Motion.ScopeDown) Controls.Motion.SetScopeDown(true);
                return true;
        }
        return false;
    }

    public override bool Down(string action, string arg, Vector2 board)
    {
        if (action == "plot.pan") { panning = true; panPress = board; panX0 = viewX; panY0 = viewY; return true; }
        if (action != "plot" || !Capturing) return false;
        pressZoom = Controls.ZoomExact;
        double wx = WorldX(board.x), wy = WorldY(board.y);
        var snap = Controls.Tool == "RULER" ? State.PointNear(wx, wy, SnapKm) : null; // Step 46
        if (snap != null) { wx = snap.x; wy = snap.y; }
        State.MarkDown(Controls.Tool, wx, wy, Controls.MissionSeconds);
        if (snap != null && State.draft != null) State.draft.ptA = snap.id;
        return true;
    }

    public override bool Drag(string action, string arg, Vector2 board)
    {
        if (action == "plot.pan" && panning)
        {
            // the world follows the pointer: drag right and the view centre moves west
            double k = KmPerPx / Controls.ZoomExact;
            viewX = panX0 - (board.x - panPress.x) * k;
            viewY = panY0 + (board.y - panPress.y) * k;
            return true;
        }
        if (action != "plot" || State.draft == null) return false;
        double wx = WorldX(board.x), wy = WorldY(board.y);
        var d = State.draft;
        var snap = d.kind == "ruler" ? State.PointNear(wx, wy, SnapKm) : null; // Step 46
        if (snap != null && snap.id != d.ptA) { wx = snap.x; wy = snap.y; d.ptB = snap.id; }
        else d.ptB = 0;
        State.MarkMove(wx, wy);
        return true;
    }

    public override bool Up(string action, string arg, Vector2 board)
    {
        if (action == "plot.pan") { panning = false; return true; }
        if (action != "plot" || State.draft == null) return false;
        State.MarkUp(State.markMinLen * KmPerPx);
        return true;
    }

    public override void Refresh()
    {
        var s = State;
        if (!viewSet) { viewX = s.nav.x; viewY = s.nav.y; viewSet = true; }
        if (!grid) grid = FindAnyObjectByType<PlotGrid>();
        if (grid)
        {
            // Step 65: the view stays inside the battle space, so the plot never shows black past its edge,
            // unless our ship has left it: then the view may follow the ship out
            Rect r = grid.rectTransform.rect;
            double k = KmPerPx / Controls.ZoomExact;
            double lx = System.Math.Max(0, s.ArenaHalfW - r.width / 2 * k), ly = System.Math.Max(0, s.ArenaHalfH - r.height / 2 * k);
            if (s.OutsideArena) { lx = System.Math.Max(lx, System.Math.Abs(s.nav.x)); ly = System.Math.Max(ly, System.Math.Abs(s.nav.y)); }
            viewX = System.Math.Max(-lx, System.Math.Min(lx, viewX));
            viewY = System.Math.Max(-ly, System.Math.Min(ly, viewY));
            // the lines move with the world: the view's offset from the world origin, in board px
            double z = Controls.ZoomExact / KmPerPx;
            grid.OffsetPx = new Vector2((float)(-viewX * z), (float)(viewY * z));
            grid.ArenaHalfKm = new Vector2((float)s.ArenaHalfW, (float)s.ArenaHalfH); // Step 65: the grid is the battle space
            ShowAxes();
            ShowRuins(r.size, z);
        }
        if (capture) capture.raycastTarget = Capturing;
        bool erasing = Controls.Tool == "ERASE";

        var all = new List<ShipState.Mark>(s.marks);
        if (s.draft != null) all.Add(s.draft);
        var shapes = new List<MarksGraphic.Shape>();
        for (int i = 0; i < all.Count; i++)
        {
            var m = all[i];
            bool isDraft = m == s.draft;
            float alpha = isDraft ? draftAlpha : 1f;
            Vector2 a = Screen(s.MarkX(m), s.MarkY(m)), b = Screen(m.x2, m.y2);
            shapes.Add(new MarksGraphic.Shape { kind = m.kind, a = a, b = b, alpha = alpha });

            var label = Label(i);
            label.alpha = alpha;
            Vector2 at;
            if (m.kind == "point") { label.text = Clock(m.t); label.alignment = TextAlignmentOptions.Left; at = a + pointLabelOffset; }
            else
            {
                double dx = m.x2 - s.MarkX(m), dy = m.y2 - s.MarkY(m); // km, y north
                label.text = "RNG " + ShipState.JsRound(s.MarkLen(m)).ToString("#,0", System.Globalization.CultureInfo.InvariantCulture)
                           + " km · BRG " + Brg(dx, dy) + "°";
                if (m.kind == "ruler")
                {
                    label.alignment = TextAlignmentOptions.Center;
                    ShipState.Mark fromPt, toPt;
                    double crs, spd;
                    if (s.TrackLeg(m, out fromPt, out toPt, out crs, out spd))
                    {
                        // a target's track: its course and speed, and where it will be projectSeconds on
                        label.text = "CRS " + (ShipState.JsRound(crs) % 360).ToString("000") + "° · "
                                   + spd.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km/s";
                        double ahead = spd * projectSeconds, rc = crs * System.Math.PI / 180.0;
                        shapes.Add(new MarksGraphic.Shape { kind = "dash", a = Screen(toPt.x, toPt.y),
                            b = Screen(toPt.x + System.Math.Sin(rc) * ahead, toPt.y + System.Math.Cos(rc) * ahead), alpha = alpha });
                    }
                    else if (s.nav.SpeedKms >= 0.01) label.text += " · " + Duration(s.MarkLen(m) / s.nav.SpeedKms); // time to cover it at our speed
                    // the board rounds the label's spot to whole layer px from the plot centre. km→px
                    // leaves float dust (250.49999999999997 for 250.5), so trim it before rounding
                    double mx = ShipState.JsRound(System.Math.Round((m.x + dx / 2.0 - viewX) / KmPerPx, 6));
                    double my = ShipState.JsRound(System.Math.Round(-(m.y + dy / 2.0 - viewY) / KmPerPx, 6));
                    at = plotCentre + new Vector2((float)mx, (float)my) * (float)Controls.ZoomExact + rulerLabelOffset;
                }
                else { label.alignment = TextAlignmentOptions.Left; at = b + ringLabelOffset; }
            }
            Place(label.rectTransform, at, label.alignment == TextAlignmentOptions.Left ? 0f : 0.5f);

            var hit = Hit(i);
            hit.gameObject.SetActive(erasing && !isDraft); // marks take the pointer only in ERASE
            hit.arg = m.id.ToString();
            var hr = (RectTransform)hit.transform;
            if (m.kind == "point") { hr.sizeDelta = new Vector2(pointBox, pointBox); Put(hr, a, 0f); }
            else if (m.kind == "ruler") { hr.sizeDelta = new Vector2((b - a).magnitude, rulerBox); Put(hr, (a + b) / 2f, -Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg); }
            else { float r = (b - a).magnitude; hr.sizeDelta = new Vector2(2 * r, 2 * r); Put(hr, a, 0f); }
        }
        for (int i = all.Count; i < labels.Count; i++) labels[i].gameObject.SetActive(false);
        for (int i = all.Count; i < hits.Count; i++) hits[i].gameObject.SetActive(false);
        // our slugs in flight (Step 48): a dot with a short trail fading behind it, no label;
        // white like our ship, not the marks' blue (owner 2026-10-26)
        var slugInk = ownShipColor.gamma;
        foreach (var r in s.weapons.Rounds)
        {
            Vector2 at = Screen(r.x, r.y), back = new Vector2((float)-r.dx, (float)r.dy) * slugTrailPx; // board y is down
            shapes.Add(new MarksGraphic.Shape { kind = "point", a = at, b = at, alpha = 1f, ink = slugInk });
            shapes.Add(new MarksGraphic.Shape { kind = "fade", a = at, b = at + back, alpha = 0.8f, ink = slugInk });
        }
        // a prize our marines hold (Step 60): an own-force dot where it lies, no label (placeholder until its glyph)
        foreach (var t in s.tracks)
            if (t.captured && t.hasWorld) { var at = Screen(t.worldX, t.worldY); shapes.Add(new MarksGraphic.Shape { kind = "point", a = at, b = at, alpha = 1f }); }
        if (marks) marks.Set(shapes);

        // our ship where it is (Step 41: it moves across the plot): the 3D model's top-down view, sized by zoom and clamped
        if (ownShipImage)
        {
            if (!shipView) shipView = ShipView.Own();
            ownShipImage.texture = shipView ? shipView.Texture : null;
            ownShipImage.enabled = shipView && shipView.Texture;
            ownShipImage.color = ownShipColor.gamma; // the board canvas linearises vertex colours; pre-encode
            float shipPx = Mathf.Clamp(ownShipPxAtFullZoom * (float)Controls.ZoomExact, ownShipMinPx, ownShipMaxPx);
            float side = shipPx / (shipView ? shipView.ShipFill : 1f);
            ownShipImage.rectTransform.sizeDelta = new Vector2(side, side);
            Put(ownShipImage.rectTransform, OwnAt, 0f);
        }

        // fighters in flight (Step 54): a V behind the leader, each wedge pointing where the flight flies
        int nf = 0;
        if (fighterTemplate)
        {
            var ink = ownShipColor.gamma; // owner 2026-10-26: white, like our ship
            foreach (var f in s.hangar.Flights)
            {
                double h = f.hdg * System.Math.PI / 180.0;
                Vector2 ahead = new Vector2((float)System.Math.Sin(h), (float)-System.Math.Cos(h)), right = new Vector2(-ahead.y, ahead.x); // board y is down
                Vector2 lead = Screen(f.x, f.y);
                if (f.order == "escort" && !f.returning)
                {
                    // owner 2026-10-26: an escort holds outside the small circle, whole V clear, even after zooming out grows it
                    Vector2 off = lead - OwnAt;
                    float clear = InnerRingPx + f.craft / 2 * fighterGapPx + fighterPx / 2f;
                    if (off.magnitude < clear) lead = OwnAt + (off.sqrMagnitude > 0f ? off.normalized : ahead) * clear;
                }
                for (int c = 0; c < f.craft; c++)
                {
                    int row = (c + 1) / 2;
                    var img = Fighter(nf++);
                    img.color = ink;
                    img.rectTransform.sizeDelta = new Vector2(fighterPx * 0.5f, fighterPx);
                    Put(img.rectTransform, lead + (right * (c % 2 == 1 ? -row : row) - ahead * row) * fighterGapPx, -(float)f.hdg);
                }
            }
            for (int i = nf; i < fighters.Count; i++) fighters[i].gameObject.SetActive(false);
        }
        FightersShown = nf;
        // a squadron takes off with its whole V (rows behind the leader, half a craft) outside the small circle round
        // our ship, and lands when its leader's nose reaches the circle
        double kmPx = KmPerPx / Controls.ZoomExact;
        s.hangar.landClearKm = (InnerRingPx + fighterPx / 2f) * kmPx;
        s.hangar.launchClearKm = s.hangar.landClearKm + s.hangar.SquadReady(s.hangar.CurrentSquad) / 2 * fighterGapPx * kmPx;

        // the selected weapon's reach while Fire Control is open (Step 46)
        if (weaponRing)
        {
            var reachShapes = new List<MarksGraphic.Shape>();
            var mount = s.weapons.MountOf(s.weapons.mount);
            if (Controls.Motion && Controls.Motion.OpenDrawerName == "fire control" && mount != null && mount.rangeKm > 0f)
            {
                Vector2 c = OwnAt;
                reachShapes.Add(new MarksGraphic.Shape { kind = "dashring", a = c, b = c + new Vector2((float)(mount.rangeKm / KmPerPx * Controls.ZoomExact), 0f), alpha = 0.7f });
            }
            // Step 55: each PDC group on AUTO shows its arc, a dashed sector off our hull (the design's PDC arc, accent.caution at .45)
            var pdGroups = System.Array.Find(s.weapons.systems, y => y.id == "PD").mounts;
            for (int g = 0; g < pdGroups.Length && g < s.weapons.pdcAuto.Length; g++)
            {
                var pm = s.weapons.MountOf(pdGroups[g]);
                if (!s.weapons.pdcAuto[g] || pm == null) continue;
                Vector2 c = OwnAt;
                float r = (float)(pm.rangeKm / KmPerPx * Controls.ZoomExact);
                System.Func<float, Vector2> at = deg => c + new Vector2(Mathf.Sin(deg * Mathf.Deg2Rad), -Mathf.Cos(deg * Mathf.Deg2Rad)) * r; // board y is down
                float hdg = (float)s.nav.DrawnHeading, from = hdg + pm.arc - pm.halfArc, to = hdg + pm.arc + pm.halfArc;
                reachShapes.Add(new MarksGraphic.Shape { kind = "dash", a = c, b = at(from), alpha = 0.45f });
                reachShapes.Add(new MarksGraphic.Shape { kind = "dash", a = c, b = at(to), alpha = 0.45f });
                for (float d = from; d < to; d += pdcArcStepDeg)
                    reachShapes.Add(new MarksGraphic.Shape { kind = "dash", a = at(d), b = at(Mathf.Min(to, d + pdcArcStepDeg)), alpha = 0.45f });
            }
            Paint(weaponRing, caution);
            weaponRing.Set(reachShapes);
        }

        if (sensorRing)
        {
            if (setSensorKm == null) setSensorKm = km => { sensorKm = km; DrawSensorRing(); };
            sensorRingMotion.Set(s.sensors.PassiveRangeKm, setSensorKm);
            // the cone reaches the scan's range, which in UV is past the hearing circle (ConeCovers)
            if (coneMotion == null) { coneMotion = new Eased(sensorRingMotion.seconds, sensorRingMotion.ease); setConeKm = km => { coneKm = km; DrawSensorRing(); }; }
            coneMotion.Set(s.sensors.SenRangeKm, setConeKm);
            DrawSensorRing();
        }

        // Step 75 (owner 2026-10-26): in sight (visualRangeKm) a ship shows as its own model from above, gliding: red
        // once identified; amber and nameless if not (you see its shape, not who it is, and can't target it); grey
        // once a wreck, drifting on. Out of sight an identified hostile is a dot where the sweep last touched it
        // (res_* in the board: a mark only once identified). km to plot px at 1x: 60.9 px per 1000 km, then the zoom.
        var dots = new List<MarksGraphic.Shape>();
        int n = 0;
        string pictured = null; // the one contact picture's track
        foreach (var t in s.tracks)
        {
            if (t.own || t.captured) continue;
            bool known = t.hostile && s.sensors.identified.Contains(t.key), inSight = s.sensors.InSight(t);
            if (!known && !inSight) continue;
            Vector2 at;
            if (inSight) at = t.nav ? Screen(t.nav.DrawnX, t.nav.DrawnY) : Screen(t.worldX, t.worldY);
            else if (t.hasWorld)
            {
                if (!t.seen) s.sensors.See(t); // identified another way: its first fix is now
                at = Screen(t.seenX, t.seenY);
            }
            else
            {
                double b = t.bearing * System.Math.PI / 180.0, px = t.rangeKm * pxPer1000Km / 1000.0 * Controls.ZoomExact;
                at = OwnAt + new Vector2((float)(System.Math.Sin(b) * px), (float)(-System.Math.Cos(b) * px));
            }
            Color ink = t.wreck ? wreckInk : known ? alarm : caution;
            var view = inSight ? ViewOf(t.key) : null;
            float labelPush = 0f; // a pictured contact's label sits clear of its picture
            if (contactShipImage && pictured == null && view && view.Texture)
            {
                // its model from above, sized as ours is; north up like ours (the model turns)
                contactShipImage.texture = view.Texture;
                contactShipImage.color = ink.gamma;
                float px = Mathf.Clamp(ownShipPxAtFullZoom * (float)Controls.ZoomExact, ownShipMinPx, ownShipMaxPx) / view.ShipFill;
                contactShipImage.rectTransform.sizeDelta = new Vector2(px, px);
                Put(contactShipImage.rectTransform, at, 0f);
                contactShipImage.enabled = true;
                pictured = t.key;
                labelPush = px * view.ShipFill / 2f;
            }
            else if (t.wreck || !known) continue; // no picture: a wreck keeps its dim dot (below); an unknown shows nothing
            else dots.Add(new MarksGraphic.Shape { kind = "point", a = at, b = at, alpha = 1f });
            if (t.wreck) continue; // a wreck carries no label (Step 49)
            var cl = ContactLabel(n++);
            Paint(cl, ink);
            float rng = inSight ? t.rangeKm : t.seenRangeKm;
            cl.text = (known ? t.name : "BOGEY " + t.key.ToUpperInvariant()) + "\nRNG " + ShipState.JsRound(rng).ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " km";
            Place(cl.rectTransform, at + contactLabelOffset + new Vector2(labelPush, 0f), 0f);
        }
        for (int i = n; i < contactLabels.Count; i++) contactLabels[i].gameObject.SetActive(false);
        if (contactShipImage && pictured == null) contactShipImage.enabled = false;
        // enemy missiles coming at us (Step 55): a red dot with a short trail, like our own rounds, no label
        foreach (var r in s.weapons.Incoming)
        {
            Vector2 at = Screen(r.x, r.y), back = new Vector2((float)-r.dx, (float)r.dy) * slugTrailPx; // board y is down
            dots.Add(new MarksGraphic.Shape { kind = "point", a = at, b = at, alpha = 1f });
            dots.Add(new MarksGraphic.Shape { kind = "fade", a = at, b = at + back, alpha = 0.8f });
        }
        if (contactMarks) contactMarks.Set(dots);

        // passive bearings: a line fading out from our ship toward each contact DRADIS hears but has not
        // identified (bearing only, no range), with its bearing; amber while the scope is on it (passives)
        bool down = Controls.Motion && Controls.Motion.ScopeDown;
        var bearings = new List<MarksGraphic.Shape>();
        int np = 0;
        Vector2 ownAt = OwnAt;
        float reach = (float)(s.sensors.PassiveRangeKm / KmPerPx * Controls.ZoomExact);
        // the bearing trail (Step 45): each line from where our ship was, fading over bearingTrailSeconds
        s.sensors.PruneBearings(Controls.MissionSeconds);
        foreach (var f in s.sensors.BearingFixes)
        {
            float life = 1f - (Controls.MissionSeconds - f.t) / Mathf.Max(1f, s.sensors.stats.bearingTrailSeconds);
            double rad = f.brg * System.Math.PI / 180.0;
            Vector2 from = Screen(f.x, f.y), dir = new Vector2((float)System.Math.Sin(rad), (float)-System.Math.Cos(rad));
            bearings.Add(new MarksGraphic.Shape { kind = "fade", a = from, b = from + dir * reach, alpha = trailAlpha * life });
        }
        foreach (var t in s.tracks)
        {
            if (t.own || !s.sensors.Heard(t) || s.sensors.identified.Contains(t.key) || !t.seen) continue; // Step 75: from the sweep's first touch
            double rad = t.seenBrg * System.Math.PI / 180.0; // where the sweep last heard it; the label shows it in whole degrees
            var dir = new Vector2((float)System.Math.Sin(rad), (float)-System.Math.Cos(rad)); // board y is down
            bool on = s.ScopeOn(t, down);
            bearings.Add(new MarksGraphic.Shape { kind = "fade", a = ownAt, b = ownAt + dir * reach, alpha = on ? 1f : 0.6f });
            var pl = PassiveLabel(np++);
            pl.text = "BRG " + (ShipState.JsRound(t.seenBrg) % 360).ToString("000");
            pl.alpha = on ? 1f : 0.6f;
            Paint(pl, on ? caution : Controls.GridAmber ? alarm : data);
            Place(pl.rectTransform, ownAt + dir * Mathf.Min(reach, passiveLabelPx), dir.x < 0f ? 1f : 0f);
            var look = pl.GetComponent<ClickSpot>();
            if (look) look.arg = t.key;
        }
        for (int i = np; i < passiveLabels.Count; i++) passiveLabels[i].gameObject.SetActive(false);
        // wrecks (Step 49): a dim dot where each destroyed ship drifts, no label; in sight it shows grey as its model (Step 75)
        foreach (var t in s.tracks)
            if (t.wreck && t.hasWorld && t.key != pictured)
            {
                var w = t.nav ? Screen(t.nav.DrawnX, t.nav.DrawnY) : Screen(t.worldX, t.worldY);
                bearings.Add(new MarksGraphic.Shape { kind = "point", a = w, b = w, alpha = 0.45f });
            }
        if (passiveLines) { Paint(passiveLines, Controls.GridAmber ? alarm : data); passiveLines.Set(bearings); }

        // the export's own passive labels (none since Step 44; kept for a configured one)
        foreach (var p in passives)
        {
            var t = s.TrackOf(p.key);
            if (!p.label) continue;
            if (t == null) { p.label.gameObject.SetActive(false); continue; } // its contact is gone (Step 34)
            if (passiveVisibility) p.label.gameObject.SetActive(s.sensors.Heard(t) && !s.sensors.identified.Contains(t.key));
            Paint(p.label, s.ScopeOn(t, down) ? caution : Controls.GridAmber ? alarm : data);
        }
    }

    // The hearing range: Heard() takes a contact inside it while DRADIS is listening (PassiveRangeKm).
    void DrawSensorRing()
    {
        var s = State;
        var shapes = new List<MarksGraphic.Shape>();
        if (s.sensors.Listening && sensorKm > 0f)
        {
            bool amber = Controls.GridAmber;
            Paint(sensorRing, amber ? alarm : data);
            Vector2 c = OwnAt, e = c + new Vector2((float)(sensorKm / KmPerPx * Controls.ZoomExact), 0f);
            shapes.Add(new MarksGraphic.Shape { kind = "disc", a = c, b = e, alpha = amber ? senGlowAlphaAmber : senGlowAlpha });
            shapes.Add(new MarksGraphic.Shape { kind = "circle", a = c, b = e, alpha = amber ? senEdgeAlphaAmber : senEdgeAlpha });
            // owner 2026-10-24: a small circle just outside our ship; the sweep runs from it to the rim
            shapes.Add(new MarksGraphic.Shape { kind = "circle", a = c, b = c + new Vector2(InnerRingPx, 0f), alpha = amber ? senEdgeAlphaAmber : senEdgeAlpha });
            // owner 2026-10-26: the active scan cone starts at that small circle too
            if (s.sensors.active && coneKm > 0f)
            {
                float r = (float)(coneKm / KmPerPx * Controls.ZoomExact), inner = InnerRingPx;
                System.Func<float, Vector2> at = deg => new Vector2(Mathf.Sin(deg * Mathf.Deg2Rad), -Mathf.Cos(deg * Mathf.Deg2Rad)); // board y is down
                shapes.Add(new MarksGraphic.Shape { kind = "sector", a = c, b = c + at(s.sensors.coneBrg) * r, inner = inner, half = s.sensors.coneArc, alpha = coneFillAlpha });
                foreach (int side in new[] { -1, 1 })
                {
                    var edge = at(s.sensors.coneBrg + side * s.sensors.coneArc);
                    shapes.Add(new MarksGraphic.Shape { kind = "line", a = c + edge * inner, b = c + edge * r, alpha = coneEdgeAlpha });
                }
            }
        }
        sensorRing.Set(shapes);

        if (!sweep) return;
        bool on = s.sensors.Listening && sensorKm > 0f;
        if (sweep.gameObject.activeSelf != on) sweep.gameObject.SetActive(on);
        if (!on) return;
        bool red = Controls.GridAmber;
        Paint(sweep, red ? sweepInkAmber : data);
        sweep.SetAlphas(red ? 0.75f : 0.7f, red ? 0.11f : 0.09f); // sweepEdge, sweepTail
        sweep.SetInner(InnerRingPx);
        float d = 2f * (float)(sensorKm / KmPerPx * Controls.ZoomExact);
        if (sweep.rectTransform.sizeDelta != new Vector2(d, d)) sweep.rectTransform.sizeDelta = new Vector2(d, d);
        Put(sweep.rectTransform, OwnAt, -sweepDeg);
    }

    /// <summary>The selected weapon's reach drawn now, km, or 0 (the playtest reads it).</summary>
    public float WeaponRingKm
    {
        get
        {
            var m = State != null ? State.weapons.MountOf(State.weapons.mount) : null;
            return Controls && Controls.Motion && Controls.Motion.OpenDrawerName == "fire control" && m != null ? m.rangeKm : 0f;
        }
    }

    // time at our speed: "45 S", "15 MIN", "1 H 12 MIN" (mission time)
    static string Duration(double seconds)
    {
        if (seconds < 60) return ShipState.JsRound(seconds) + " S";
        int min = ShipState.JsRound(seconds / 60);
        return min < 60 ? min + " MIN" : (min / 60) + " H " + (min % 60) + " MIN";
    }

    TMP_Text PassiveLabel(int i)
    {
        while (passiveLabels.Count <= i)
        {
            var t = Instantiate(passiveLabelTemplate, passiveLabelTemplate.transform.parent);
            t.name = "Passive label " + passiveLabels.Count;
            passiveLabels.Add(t);
        }
        passiveLabels[i].gameObject.SetActive(true);
        return passiveLabels[i];
    }

    // The ShipView watching a contact's model, if there is one.
    ShipView ViewOf(string key)
    {
        if (shipViews == null) shipViews = FindObjectsByType<ShipView>(FindObjectsSortMode.None);
        foreach (var v in shipViews)
        {
            var wc = v && v.Target ? v.Target.GetComponentInParent<WorldContact>() : null;
            if (wc && wc.trackKey == key) return v;
        }
        return null;
    }

    Image Fighter(int i)
    {
        while (fighters.Count <= i)
        {
            var f = Instantiate(fighterTemplate, fighterTemplate.transform.parent);
            f.name = "Fighter " + fighters.Count;
            fighters.Add(f);
        }
        fighters[i].gameObject.SetActive(true);
        return fighters[i];
    }

    TMP_Text ContactLabel(int i)
    {
        while (contactLabels.Count <= i)
        {
            var t = Instantiate(contactLabelTemplate, contactLabelTemplate.transform.parent);
            t.name = "Contact label " + contactLabels.Count;
            Paint(t, alarm);
            contactLabels.Add(t);
        }
        contactLabels[i].gameObject.SetActive(true);
        return contactLabels[i];
    }

    TMP_Text Label(int i)
    {
        while (labels.Count <= i)
        {
            var t = Instantiate(labelTemplate, labelTemplate.transform.parent);
            t.name = "Mark label " + labels.Count;
            Paint(t, ownForce);
            labels.Add(t);
        }
        labels[i].gameObject.SetActive(true);
        return labels[i];
    }

    ClickSpot Hit(int i)
    {
        while (hits.Count <= i)
        {
            var go = new GameObject("Mark hit " + hits.Count, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = hitLayer.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(hitLayer, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            go.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            var h = go.AddComponent<ClickSpot>();
            h.action = "plot.erase";
            hits.Add(h);
        }
        return hits[i];
    }

    // Step 76 (owner 2026-10-26): the axis numbers are distances from our ship, in km, along the top and left edges.
    // 0 is level with our ship, then one number per big square: they slide as the ship moves and spread as you zoom.
    // No titles. The design's bottom row stays gone: the ORDERS panel covers that edge's left end. The look is on the inspector.
    private readonly List<TextMeshProUGUI> axisLabels = new List<TextMeshProUGUI>();
    const float AxisTopPx = 38f, AxisLeftPx = 23f, AxisEdgePx = 20f; // the design's top row and left column, plot px

    void ShowAxes()
    {
        if (!axisLayer || !axisFont) return;
        Rect r = grid.rectTransform.rect;
        var o = marks ? marks.origin : Vector2.zero;
        float cellPx;
        double major = grid.CellKm(out cellPx) * grid.MajorEvery;
        float step = cellPx * grid.MajorEvery; // px per big square
        if (step < 1f) return;
        Vector2 own = OwnAt;
        int used = 0;
        for (int k = Mathf.CeilToInt((o.x + AxisEdgePx - own.x) / step); own.x + k * step <= o.x + r.width - AxisEdgePx; k++)
            AxisLabel(used++, System.Math.Abs(k) * major, new Vector2(own.x + k * step, o.y + AxisTopPx));
        for (int k = Mathf.CeilToInt((o.y + AxisEdgePx - own.y) / step); own.y + k * step <= o.y + r.height - AxisEdgePx; k++)
            AxisLabel(used++, System.Math.Abs(k) * major, new Vector2(o.x + AxisLeftPx, own.y + k * step));
        for (int i = used; i < axisLabels.Count; i++) if (axisLabels[i].gameObject.activeSelf) axisLabels[i].gameObject.SetActive(false);
    }

    void AxisLabel(int i, double km, Vector2 board)
    {
        while (axisLabels.Count <= i)
        {
            var go = new GameObject("Axis number", typeof(RectTransform));
            go.transform.SetParent(axisLayer, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0f, 1f); // as Put expects
            t.rectTransform.sizeDelta = new Vector2(50f, 12f); // room for "100,000"
            t.font = axisFont; // through the properties, not serialization (BUILD_LOG trap 2)
            t.fontSharedMaterial = axisMaterial ? axisMaterial : axisFont.material;
            t.fontSize = axisFontSize;
            t.color = axisColour;
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.richText = false;
            t.raycastTarget = false;
            axisLabels.Add(t);
        }
        var l = axisLabels[i];
        l.text = km.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture);
        if (!l.gameObject.activeSelf) l.gameObject.SetActive(true);
        Put(l.rectTransform, board, 0f);
    }

    // Children of a plot-sized rect anchored top-left at the plot origin (104,100).
    // Ruins (see ruinsImage): a camera looking straight down at the found terrain, framed as the plot is
    // (the view's centre at plotCentre, z board px per km), into a picture the size of the plot
    void ShowRuins(Vector2 size, double z)
    {
        if (!ruinsImage) return;
        int w = Mathf.Max(1, Mathf.RoundToInt(size.x)), h = Mathf.Max(1, Mathf.RoundToInt(size.y));
        if (!ruinsCamera)
        {
            ruinsCamera = new GameObject("Ruins View Camera").AddComponent<Camera>();
            ruinsCamera.orthographic = true;
            ruinsCamera.allowHDR = false; // as the ship views: the line-art pass keeps its red on black
            ruinsCamera.clearFlags = CameraClearFlags.SolidColor;
            ruinsCamera.backgroundColor = new Color(0, 0, 0, 0);
            ruinsCamera.cullingMask = LayerMask.GetMask("WorldTerrain");
            ruinsCamera.nearClipPlane = 1f;
            ruinsCamera.farClipPlane = 200000f;
        }
        if (!ruinsTexture || ruinsTexture.width != w || ruinsTexture.height != h)
        {
            if (ruinsTexture) { ruinsCamera.targetTexture = null; ruinsTexture.Release(); Destroy(ruinsTexture); }
            ruinsTexture = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            { name = "Ruins view", hideFlags = HideFlags.DontSave, antiAliasing = 4 };
            ruinsCamera.targetTexture = ruinsTexture;
            ruinsImage.texture = ruinsTexture;
        }
        ruinsCamera.orthographicSize = (float)(h / 2.0 / z);
        ruinsCamera.transform.SetPositionAndRotation(new Vector3((float)viewX, (float)viewY, -100000f), Quaternion.identity); // look along +z, north up
        ruinsImage.rectTransform.sizeDelta = size;
        Put(ruinsImage.rectTransform, plotCentre, 0f);
        ruinsImage.color = ruinsColor.gamma; // the board canvas linearises vertex colours; pre-encode
    }

    private void OnDestroy()
    {
        if (ruinsCamera) { ruinsCamera.targetTexture = null; Destroy(ruinsCamera.gameObject); }
        if (ruinsTexture) { ruinsTexture.Release(); Destroy(ruinsTexture); }
    }

    void Put(RectTransform rt, Vector2 board, float rotZ)
    {
        var o = marks ? marks.origin : Vector2.zero;
        rt.anchoredPosition = new Vector2(board.x - o.x, -(board.y - o.y));
        rt.localEulerAngles = new Vector3(0, 0, rotZ);
    }

    void Place(RectTransform rt, Vector2 board, float pivotX)
    {
        rt.pivot = new Vector2(pivotX, 0.5f);
        Put(rt, board, 0f);
    }

    // marksView helpers

    static string Brg(double dx, double dyNorth)
    {
        double deg = (System.Math.Atan2(dx, dyNorth) * 180 / System.Math.PI + 360) % 360;
        return (ShipState.JsRound(deg) % 360).ToString("000");
    }

    // clk: hh:mm:ss from the mission clock; seconds print as JS prints a number (2.25 stays 2.25)
    static string Clock(float t)
    {
        double s = t;
        string P(double n) { var str = n.ToString("R", System.Globalization.CultureInfo.InvariantCulture); return str.Length < 2 ? "0" + str : str; }
        return P(System.Math.Floor(s / 3600) % 100) + ":" + P(System.Math.Floor(s / 60) % 60) + ":" + P(s % 60);
    }
}
