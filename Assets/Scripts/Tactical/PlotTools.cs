using UnityEngine;

/// <summary>
/// The seven plotting instruments on the left rail.
///
/// Five are MODES — you select one and it stays selected. Two, UNDO and CLEAR
/// ALL, are immediate ACTIONS: they do their job on press and never become the
/// active tool. The rail has to treat those differently or the board ends up
/// stuck in a tool that does nothing.
/// </summary>
public class PlotTools : MonoBehaviour
{
    public enum Tool { None, Point, Line, Ruler, Compass, Erase }

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

    /// <summary>Raised when the active tool changes, so the rail can light up.</summary>
    public event System.Action<Tool> ToolChanged;

    /// <summary>Raised with a live readout while measuring. Empty when nothing to say.</summary>
    public event System.Action<string> Readout;

    private Tool active = Tool.None;
    public Tool Active { get { return active; } }

    public void SetTool(Tool t)
    {
        if (overlay == null) return;
        if (active == t) t = Tool.None;         // pressing the live tool puts it away
        active = t;
        overlay.SetPreview(null);
        Say("");
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
            case "point":   SetTool(Tool.Point);   break;
            case "line":    SetTool(Tool.Line);    break;
            case "ruler":   SetTool(Tool.Ruler);   break;
            case "compass": SetTool(Tool.Compass); break;
            case "erase":   SetTool(Tool.Erase);   break;

            // Actions, not modes. They never become the active tool.
            case "undo":
                Say(overlay.Undo() ? "UNDONE" : "NOTHING TO UNDO");
                break;
            case "clear":
                overlay.Clear();
                Say("PLOT CLEARED");
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
                overlay.Add(PlotAnnotation.Point(world, markColour));
                Say(Position(world));
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
            case Tool.Line:
                return PlotAnnotation.Between(PlotAnnotation.Kind.Line, from, to, markColour);
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

    private static string Position(Vector2 w)
    {
        return "X " + Mathf.RoundToInt(w.x).ToString("N0") + "   Y " + Mathf.RoundToInt(w.y).ToString("N0");
    }

    private void Say(string s) { if (Readout != null) Readout(s); }
}
