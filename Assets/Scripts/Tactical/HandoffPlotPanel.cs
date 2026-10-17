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

    [Tooltip("A passive label shows only while its contact is heard and not identified (passives).")]
    [SerializeField] private bool passiveVisibility = true;

    [Header("Plot space (§8.4): own ship at the plot centre; 60.9 layer px = 1000 km")]
    [SerializeField] private Vector2 plotCentre = new Vector2(960, 545);
    [SerializeField] private double pxPer1000Km = 60.9;

    [Header("Labels — offsets have no value in the package (HANDOFF_GAPS)")]
    [SerializeField] private Vector2 pointLabelOffset = new Vector2(10, 0);
    [SerializeField] private Vector2 rulerLabelOffset = new Vector2(0, -12);
    [SerializeField] private Vector2 ringLabelOffset = new Vector2(8, 0);
    [Tooltip("ERASE click area: the point box, and the ruler's height (§8.4).")]
    [SerializeField] private float pointBox = 16f, rulerBox = 14f;
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
    Vector2 Board(int x, int y) => plotCentre + new Vector2(x, y) * (float)Controls.ZoomExact;

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
        State.MarkDown(Controls.Tool, LayerX(board.x), LayerY(board.y), Controls.MissionSeconds);
        return true;
    }

    public override bool Drag(string action, string arg, Vector2 board)
    {
        if (action != "plot" || State.draft == null) return false;
        State.MarkMove(LayerX(board.x), LayerY(board.y));
        return true;
    }

    public override bool Up(string action, string arg, Vector2 board)
    {
        if (action != "plot" || State.draft == null) return false;
        State.MarkUp();
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
            Vector2 a = Board(m.x, m.y), b = Board(m.x2, m.y2);
            shapes.Add(new HandoffMarksGraphic.Shape { kind = m.kind, a = a, b = b, alpha = alpha });

            var label = Label(i);
            label.alpha = alpha;
            Vector2 at;
            if (m.kind == "point") { label.text = Clock(m.t); label.alignment = TextAlignmentOptions.Left; at = a + pointLabelOffset; }
            else
            {
                int dx = m.x2 - m.x, dy = m.y2 - m.y;
                label.text = "RNG " + Km(HandoffBoardState.MarkLen(m)) + " km · BRG " + Brg(dx, dy) + "°";
                if (m.kind == "ruler")
                {
                    label.alignment = TextAlignmentOptions.Center;
                    at = Board(HandoffBoardState.JsRound(m.x + dx / 2.0), HandoffBoardState.JsRound(m.y + dy / 2.0)) + rulerLabelOffset;
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

        // passive bearing labels: amber while the scope is on the contact
        bool down = Controls.Motion && Controls.Motion.ScopeDown;
        foreach (var p in passives)
        {
            var t = s.TrackOf(p.key);
            if (t == null || !p.label) continue;
            if (passiveVisibility) p.label.gameObject.SetActive(s.Heard(t) && !s.identified.Contains(t.key));
            Paint(p.label, s.ScopeOn(t, down) ? caution : Controls.GridAmber ? alarm : data);
        }
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
    string Km(double px) => HandoffBoardState.JsRound(px * 1000.0 / pxPer1000Km).ToString("#,0", System.Globalization.CultureInfo.InvariantCulture);

    static string Brg(int dx, int dy)
    {
        double deg = (System.Math.Atan2(dx, -dy) * 180 / System.Math.PI + 360) % 360;
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
