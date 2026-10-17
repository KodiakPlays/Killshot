using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A clickable, draggable or scrollable spot on the handoff board. It only names what was touched
/// (action and arg) and where; HandoffBoardControls decides what that does. Added by the board
/// builder to the image that is the control's hardware, which is then the only raycast target there.
/// </summary>
public class HandoffHit : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
{
    public string action;
    public string arg;

    private HandoffBoardControls owner;

    private HandoffBoardControls Owner
    {
        get { if (owner == null) owner = GetComponentInParent<HandoffBoardControls>(true); return owner; }
    }

    public void OnPointerClick(PointerEventData e) { if (!e.dragging) Owner.Click(action, arg); }
    public void OnPointerDown(PointerEventData e) { Owner.Down(action, arg, e.position); }
    public void OnPointerUp(PointerEventData e)
    {
        // The EventSystem holds OnDrag back until 10 px of movement; the browser sends every move.
        // Deliver the release point as a last drag so short drags still count (Step 33i verification).
        if (!e.dragging && e.position != e.pressPosition) Owner.Drag(action, arg, e.position - e.pressPosition, e.position);
        Owner.Up(action, arg, e.position);
    }
    public void OnDrag(PointerEventData e) { Owner.Drag(action, arg, e.position - e.pressPosition, e.position); } // total since the press
    public void OnScroll(PointerEventData e) { Owner.Scroll(action, arg, e.scrollDelta.y); }
}
