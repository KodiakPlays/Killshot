// =================================================================================================
// ClickSpot.cs
//
// What it does:
// A spot on the battle board that you can click, drag or scroll on. It is on every button, knob,
// switch and lever of the board. These are in the board scenes and the panel prefabs. It only
// says what the player touched and where. BoardManager decides what happens.
//
// How it works:
// - Each spot has an action name and an extra word, set in the Inspector. For example, a drawer
//   key has the action "drawer" and the drawer's name.
// - It sends BoardManager the action, the word and the mouse place. It does this on a click, a
//   press, a drag, a release or a mouse wheel turn. It finds BoardManager above it in the scene.
// - A click that ends a drag is not sent as a click.
// - A drag sends the distance moved from the press.
// - A short drag that Unity does not report as a drag still sends its last move on release.
// - The plot adds a spot on each of its marks by itself. Clicking one with ERASE removes it.
//
// Technical:
// - Data: action and arg, set in the Inspector. Each panel's Click, Down, Drag and Up looks for
//   the action names it knows.
// - Unity's event system calls its five pointer methods. The object needs an image that takes
//   clicks (Raycast Target on).
// - Each method calls BoardManager: Click, Down, Up, Drag or Scroll. Points are screen px.
//   BoardManager turns them into board px.
// - Drag gets the whole move from the press, never the move of one frame.
// - It finds BoardManager once, with GetComponentInParent. Switched-off parents count.
// - Trap: a spot with no BoardManager above it throws an error when touched.
// - PlotPanel adds a ClickSpot to each mark in code. PlaytestRunner finds spots and presses them.
// =================================================================================================

using UnityEngine;
using UnityEngine.EventSystems;

public class ClickSpot : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
{
    public string action;
    public string arg;

    private BoardManager owner;

    private BoardManager Owner
    {
        get { if (owner == null) owner = GetComponentInParent<BoardManager>(true); return owner; }
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
