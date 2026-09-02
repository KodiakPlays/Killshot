using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// The plot's input surface. An invisible graphic covering the board that turns
/// pointer events into world positions.
///
/// This is the bridge every tool needs. Nothing here knows what a tool is — it
/// reports where on the plot something happened and lets a listener decide.
///
/// It sits behind every panel and rail in the canvas, so clicking a button or a
/// panel never reaches the plot.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PlotSurface : MonoBehaviour,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField] private PlotCamera plot;

    [Header("Panning")]
    [Tooltip("Which button drags the view. Left is left free for tools.")]
    [SerializeField] private PointerEventData.InputButton panButton = PointerEventData.InputButton.Middle;
    [SerializeField] private bool rightButtonAlsoPans = true;

    /// <summary>A click on the plot, in world coordinates.</summary>
    public event System.Action<Vector2> Clicked;
    /// <summary>A left-drag on the plot: start and current, both in world coordinates.</summary>
    public event System.Action<Vector2, Vector2> Dragged;
    /// <summary>A left-drag finished: start and end, both in world coordinates.</summary>
    public event System.Action<Vector2, Vector2> DragEnded;
    /// <summary>The pointer moved over the plot, in world coordinates.</summary>
    public event System.Action<Vector2> Hovered;

    public PlotCamera Plot { get { return plot; } set { plot = value; } }

    private Vector2 dragStartWorld;
    private Vector3 panStartWorld;
    private bool panning;

    private void Reset() { EnsureRaycastTarget(); }
    private void Awake() { EnsureRaycastTarget(); }

    /// <summary>
    /// Needs a Graphic to receive events, but must not draw anything.
    /// A zero-alpha Image with raycasts on is the standard way to do this.
    /// </summary>
    private void EnsureRaycastTarget()
    {
        var g = GetComponent<Graphic>();
        if (g == null)
        {
            var img = gameObject.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);
            g = img;
        }
        g.raycastTarget = true;
    }

    private Vector2 ToWorld(PointerEventData e)
    {
        if (plot == null) return Vector2.zero;
        var w = plot.ScreenToPlot(e.position);
        return new Vector2(w.x, w.y);
    }

    public void OnPointerMove(PointerEventData e)
    {
        if (plot != null) plot.SetPointer(e.position, true);   // zoom anchors here
        if (Hovered != null) Hovered(ToWorld(e));
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (plot != null) plot.SetPointer(e.position, false);  // fall back to centre zoom
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left) return;
        if (e.dragging) return;                       // a pan is not a click
        if (Clicked != null) Clicked(ToWorld(e));
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (IsPanButton(e.button))
        {
            panning = true;
            panStartWorld = plot != null ? plot.ScreenToPlot(e.position) : Vector3.zero;
            return;
        }
        if (e.button != PointerEventData.InputButton.Left) return;
        dragStartWorld = ToWorld(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (panning)
        {
            if (plot == null) return;
            // Drag the world, not the camera: move the camera by however far the
            // grabbed point has slipped from under the cursor.
            var now = plot.ScreenToPlot(e.position);
            var delta = panStartWorld - now;
            plot.transform.position += new Vector3(delta.x, delta.y, 0f);
            return;
        }
        if (e.button != PointerEventData.InputButton.Left) return;
        if (Dragged != null) Dragged(dragStartWorld, ToWorld(e));
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (panning) { panning = false; return; }
        if (e.button != PointerEventData.InputButton.Left) return;
        if (DragEnded != null) DragEnded(dragStartWorld, ToWorld(e));
    }

    private bool IsPanButton(PointerEventData.InputButton b)
    {
        if (b == panButton) return true;
        return rightButtonAlsoPans && b == PointerEventData.InputButton.Right;
    }
}
