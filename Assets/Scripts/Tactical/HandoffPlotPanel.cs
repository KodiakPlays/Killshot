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
public class HandoffPlotPanel : HandoffPanel
{
    [System.Serializable]
    public class Passive
    {
        public string key;
        public TMP_Text label;
    }

    [SerializeField] private Graphic capture;
    [SerializeField] private HandoffMarksGraphic marks;
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
    [Tune("Plot & our ship")]
    [Tooltip("Our ship's line colour on the plot (owner 2026-10-18: white; the design's ink.data #E8EDF0).")]
    [SerializeField] private Color ownShipColor = new Color(0xE8 / 255f, 0xED / 255f, 0xF0 / 255f, 1f);
    private ShipView shipView;

    [Header("Contacts on the plot (Step 34): identified hostiles at their true range and bearing")]
    [Tooltip("Stand-in art until Claude Design's contact glyphs arrive (CLAUDE_DESIGN_NEEDS 1.2).")]
    [SerializeField] private HandoffMarksGraphic contactMarks;
    [SerializeField] private TMP_Text contactLabelTemplate;
    [Tune("Plot & our ship")]
    [SerializeField] private Vector2 contactLabelOffset = new Vector2(10, 0);
    private readonly List<TMP_Text> contactLabels = new List<TMP_Text>();

    [Tooltip("A passive label shows only while its contact is heard and not identified (passives).")]
    [SerializeField] private bool passiveVisibility = true;

    [Header("Plot space (§8.4): own ship at the plot centre; 60.9 layer px = 1000 km")]
    [SerializeField] private Vector2 plotCentre = new Vector2(960, 545);
    [SerializeField] private double pxPer1000Km = 60.9;

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

    [Header("Colours (design sRGB)")]
    [SerializeField] private Color ownForce = new Color32(0x4A, 0x9F, 0xD8, 0xFF);
    [SerializeField] private Color caution = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color alarm = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
    [SerializeField] private Color data = new Color32(0xE8, 0xED, 0xF0, 0xFF);

    private readonly List<TMP_Text> labels = new List<TMP_Text>();
    private readonly List<HandoffHit> hits = new List<HandoffHit>();

    bool Capturing => Controls.Tool == "POINT" || Controls.Tool == "RULER" || Controls.Tool == "COMPASS";

    // plotPt: pointer to plot-layer px at 1x, rounded
    // the board measures a whole drag at the zoom of the press (_plot.k)
    private double pressZoom = 1;
    int LayerX(float bx) => HandoffBoardState.JsRound((bx - plotCentre.x) / pressZoom);
    int LayerY(float by) => HandoffBoardState.JsRound((by - plotCentre.y) / pressZoom);

    // Step 36: marks live in world km (x east, y north) so they stay put when our ship moves.
    // The pointer is still rounded to whole layer px first, as the board's plotPt does.
    double KmPerPx => 1000.0 / pxPer1000Km;
    double WorldX(float bx) => State.ownX + LayerX(bx) * KmPerPx;
    double WorldY(float by) => State.ownY - LayerY(by) * KmPerPx;
    Vector2 Screen(double wx, double wy)
    {
        double z = Controls.ZoomExact / KmPerPx;
        return plotCentre + new Vector2((float)((wx - State.ownX) * z), (float)(-(wy - State.ownY) * z));
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
        if (action != "plot" || !Capturing) return false;
        pressZoom = Controls.ZoomExact;
        State.MarkDown(Controls.Tool, WorldX(board.x), WorldY(board.y), Controls.MissionSeconds);
        return true;
    }

    public override bool Drag(string action, string arg, Vector2 board)
    {
        if (action != "plot" || State.draft == null) return false;
        State.MarkMove(WorldX(board.x), WorldY(board.y));
        return true;
    }

    public override bool Up(string action, string arg, Vector2 board)
    {
        if (action != "plot" || State.draft == null) return false;
        State.MarkUp(State.markMinLen * KmPerPx);
        return true;
    }

    public override void Refresh()
    {
        var s = State;
        if (capture) capture.raycastTarget = Capturing;
        bool erasing = Controls.Tool == "ERASE";

        var all = new List<HandoffBoardState.Mark>(s.marks);
        if (s.draft != null) all.Add(s.draft);
        var shapes = new List<HandoffMarksGraphic.Shape>();
        for (int i = 0; i < all.Count; i++)
        {
            var m = all[i];
            bool isDraft = m == s.draft;
            float alpha = isDraft ? draftAlpha : 1f;
            Vector2 a = Screen(s.MarkX(m), s.MarkY(m)), b = Screen(m.x2, m.y2);
            shapes.Add(new HandoffMarksGraphic.Shape { kind = m.kind, a = a, b = b, alpha = alpha });

            var label = Label(i);
            label.alpha = alpha;
            Vector2 at;
            if (m.kind == "point") { label.text = Clock(m.t); label.alignment = TextAlignmentOptions.Left; at = a + pointLabelOffset; }
            else
            {
                double dx = m.x2 - s.MarkX(m), dy = m.y2 - s.MarkY(m); // km, y north
                label.text = "RNG " + HandoffBoardState.JsRound(s.MarkLen(m)).ToString("#,0", System.Globalization.CultureInfo.InvariantCulture)
                           + " km · BRG " + Brg(dx, dy) + "°";
                if (m.kind == "ruler")
                {
                    label.alignment = TextAlignmentOptions.Center;
                    // the board rounds the label's spot to whole layer px from our ship. km→px leaves
                    // float dust (250.49999999999997 for 250.5), so trim it before rounding
                    double mx = HandoffBoardState.JsRound(System.Math.Round((m.x + dx / 2.0 - s.ownX) / KmPerPx, 6));
                    double my = HandoffBoardState.JsRound(System.Math.Round(-(m.y + dy / 2.0 - s.ownY) / KmPerPx, 6));
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
        if (marks) marks.Set(shapes);

        // our ship, always at the plot centre: the 3D model's top-down view, sized by zoom and clamped
        if (ownShipImage)
        {
            if (!shipView) shipView = FindAnyObjectByType<ShipView>();
            ownShipImage.texture = shipView ? shipView.Texture : null;
            ownShipImage.enabled = shipView && shipView.Texture;
            ownShipImage.color = ownShipColor.gamma; // the board canvas linearises vertex colours; pre-encode
            float shipPx = Mathf.Clamp(ownShipPxAtFullZoom * (float)Controls.ZoomExact, ownShipMinPx, ownShipMaxPx);
            float side = shipPx / (shipView ? shipView.ShipFill : 1f);
            ownShipImage.rectTransform.sizeDelta = new Vector2(side, side);
            Put(ownShipImage.rectTransform, plotCentre, 0f);
        }

        // identified hostile contacts at their true spot (res_* in the board: a mark only once
        // identified). km to plot px at 1x: 60.9 px per 1000 km, then the zoom.
        var dots = new List<HandoffMarksGraphic.Shape>();
        int n = 0;
        foreach (var t in s.tracks)
        {
            if (!t.hostile || !s.identified.Contains(t.key)) continue;
            // drawn at its exact world spot, so it lines up with marks; the rounded bearing and range
            // are for readouts only (Step 36 verification)
            Vector2 at;
            if (t.hasWorld) at = Screen(t.worldX, t.worldY);
            else
            {
                double b = t.bearing * System.Math.PI / 180.0, px = t.rangeKm * pxPer1000Km / 1000.0 * Controls.ZoomExact;
                at = plotCentre + new Vector2((float)(System.Math.Sin(b) * px), (float)(-System.Math.Cos(b) * px));
            }
            dots.Add(new HandoffMarksGraphic.Shape { kind = "point", a = at, b = at, alpha = 1f });
            var cl = ContactLabel(n++);
            cl.text = t.name + "\nRNG " + HandoffBoardState.JsRound(t.rangeKm).ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " km";
            Place(cl.rectTransform, at + contactLabelOffset, 0f);
        }
        for (int i = n; i < contactLabels.Count; i++) contactLabels[i].gameObject.SetActive(false);
        if (contactMarks) contactMarks.Set(dots);

        // passive bearing labels: amber while the scope is on the contact
        bool down = Controls.Motion && Controls.Motion.ScopeDown;
        foreach (var p in passives)
        {
            var t = s.TrackOf(p.key);
            if (!p.label) continue;
            if (t == null) { p.label.gameObject.SetActive(false); continue; } // its contact is gone (Step 34)
            if (passiveVisibility) p.label.gameObject.SetActive(s.Heard(t) && !s.identified.Contains(t.key));
            Paint(p.label, s.ScopeOn(t, down) ? caution : Controls.GridAmber ? alarm : data);
        }
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

    HandoffHit Hit(int i)
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
            var h = go.AddComponent<HandoffHit>();
            h.action = "plot.erase";
            hits.Add(h);
        }
        return hits[i];
    }

    // Children of a plot-sized rect anchored top-left at the plot origin (104,100).
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
        return (HandoffBoardState.JsRound(deg) % 360).ToString("000");
    }

    // clk: hh:mm:ss from the mission clock; seconds print as JS prints a number (2.25 stays 2.25)
    static string Clock(float t)
    {
        double s = t;
        string P(double n) { var str = n.ToString("R", System.Globalization.CultureInfo.InvariantCulture); return str.Length < 2 ? "0" + str : str; }
        return P(System.Math.Floor(s / 3600) % 100) + ":" + P(System.Math.Floor(s / 60) % 60) + ":" + P(s % 60);
    }
}
