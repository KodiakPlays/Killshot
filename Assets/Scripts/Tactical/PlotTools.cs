using UnityEngine;

/// <summary>
/// The plotting instruments on the left rail.
///
/// Five buttons, all modes. MOUSE is the default and means "no tool": the plot
/// is just something you look at.
///
/// Clear-all is nested in the eraser: press ERASE a second time while it is
/// already live and the plot is wiped. It does not get a button of its own.
/// </summary>
public class PlotTools : MonoBehaviour
{
    public enum Tool { Mouse, Point, Ruler, Compass, Erase }

    [SerializeField] private PlotSurface surface;
    [SerializeField] private PlotOverlay overlay;
    [SerializeField] private PlotCamera plot;

    [Header("Colours")]
    [Tooltip("Handoff: green for measurement.")]
    [SerializeField] private Color measureColour = new Color(0.561f, 0.765f, 0.227f, 1f);
    [Tooltip("Handoff: amber for orders and plotted marks.")]
    [SerializeField] private Color markColour = new Color(0.910f, 0.639f, 0.239f, 1f);

    [Header("Feel")]
    [Tooltip("How near, in pixels, ERASE has to be to remove a mark.")]
    [SerializeField] private float erasePixelRadius = 14f;

    /// <summary>Raised when the live tool changes, so the rail can light up.</summary>
    public event System.Action<Tool> ToolChanged;

    /// <summary>Live readout. Empty string means there is nothing to say.</summary>
    public event System.Action<string> Readout;

    private Tool active = Tool.Mouse;
    private int pointCount;

    public Tool Active { get { return active; } }

    /// <summary>The BoardAction id of the live tool, for the rail highlight.</summary>
    public string ActiveId
    {
        get
        {
            switch (active)
            {
                case Tool.Point:   return "point";
                case Tool.Ruler:   return "ruler";
                case Tool.Compass: return "compass";
                case Tool.Erase:   return "erase";
                default:           return "mouse";
            }
        }
    }

    public void SetTool(Tool t)
    {
        if (overlay == null) return;
        active = t;
        overlay.SetPreview(null);
        Say(t == Tool.Mouse ? "" : t.ToString().ToUpper() + " READY");
        if (ToolChanged != null) ToolChanged(active);
    }

    /// <summary>
    /// Route a rail button by its BoardAction id.
    /// Deliberately not called Invoke — MonoBehaviour already has one and the
    /// overload resolution gets ambiguous.
    /// </summary>
    public void Press(string id)
    {
        if (overlay == null)
        {
            Debug.LogError("[PlotTools] No overlay assigned; '" + id + "' does nothing.", this);
            return;
        }

        switch (id)
        {
            case "mouse":   SetTool(Tool.Mouse);   break;
            case "point":   SetTool(Tool.Point);   break;
            case "ruler":   SetTool(Tool.Ruler);   break;
            case "compass": SetTool(Tool.Compass); break;

            case "erase":
                // Pressing the eraser when it is already live wipes the plot.
                // Clear-all lives here rather than taking a button of its own.
                if (active == Tool.Erase)
                {
                    overlay.Clear();
                    pointCount = 0;
                    Say("PLOT CLEARED");
                }
                else SetTool(Tool.Erase);
                break;
        }
    }

    private void OnEnable()
    {
        // Fail loudly. A missing reference here means no tool responds to
        // anything, with nothing on screen or in the console to explain it.
        if (surface == null || overlay == null || plot == null)
        {
            Debug.LogError("[PlotTools] Unassigned: " +
                (surface == null ? "surface " : "") +
                (overlay == null ? "overlay " : "") +
                (plot == null ? "plot" : "") +
                "— no tool will do anything.", this);
            return;
        }

        surface.Clicked   += OnClick;
        surface.Dragged   += OnDrag;
        surface.DragEnded += OnDragEnd;
    }

    private void OnDisable()
    {
        if (surface == null) return;
        surface.Clicked   -= OnClick;
        surface.Dragged   -= OnDrag;
        surface.DragEnded -= OnDragEnd;
    }

    private void OnClick(Vector2 world)
    {
        switch (active)
        {
            case Tool.Point:
                pointCount++;
                var mark = PlotAnnotation.Point(world, markColour, "P" + pointCount, MissionClock.Now);
                overlay.Add(mark);
                Say(mark.designator + " " + MissionClock.Stamp(mark.stampedAt) +
                    "   X " + Mathf.RoundToInt(world.x).ToString("N0") +
                    "   Y " + Mathf.RoundToInt(world.y).ToString("N0"));
                break;

            case Tool.Erase:
                Say(overlay.EraseNear(world, erasePixelRadius) ? "ERASED" : "NOTHING THERE");
                break;
        }
    }

    private void OnDrag(Vector2 from, Vector2 to)
    {
        var m = Build(from, to);
        if (m == null) return;
        overlay.SetPreview(m);
        Say(Describe(m));
    }

    private void OnDragEnd(Vector2 from, Vector2 to)
    {
        var m = Build(from, to);
        if (m == null) return;
        // A drag that never went anywhere is a click, not a mark.
        if (plot != null && m.Range < plot.WorldPerPixel * 4f) { overlay.SetPreview(null); return; }
        overlay.SetPreview(m);
        overlay.CommitPreview();
        Say(Describe(m));
    }

    private PlotAnnotation Build(Vector2 from, Vector2 to)
    {
        switch (active)
        {
            case Tool.Ruler:
                return PlotAnnotation.Between(PlotAnnotation.Kind.Ruler, from, to, measureColour);
            case Tool.Compass:
                return PlotAnnotation.Between(PlotAnnotation.Kind.Ring, from, to, measureColour);
            default:
                return null;
        }
    }

    /// <summary>
    /// Range and bearing — the measurement needed to build a firing solution.
    /// Bearing is compass degrees: 0 is north, 90 is east.
    /// </summary>
    private static string Describe(PlotAnnotation m)
    {
        if (m.kind == PlotAnnotation.Kind.Ring)
            return "RADIUS " + Mathf.RoundToInt(m.Range).ToString("N0") + " u";
        return "RNG " + Mathf.RoundToInt(m.Range).ToString("N0") + " u" +
               "   BRG " + Mathf.RoundToInt(m.Bearing).ToString("000") + "°";
    }

    private void Say(string s) { if (Readout != null) Readout(s); }
}
