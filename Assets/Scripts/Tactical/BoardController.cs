using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Routes rail clicks to drawers. Keeps one drawer open at a time.
/// This is the only thing that knows about both the rails and the panels.
/// </summary>
public class BoardController : MonoBehaviour
{
    [SerializeField] private IconRail leftRail;
    [SerializeField] private IconRail rightRail;
    [SerializeField] private List<DrawerPanel> drawers = new List<DrawerPanel>();
    [SerializeField] private List<BoardAction> drawerActions = new List<BoardAction>();
    [Tooltip("Receives the left rail's tool buttons.")]
    [SerializeField] private PlotTools tools;

    public IconRail LeftRail { get { return leftRail; } set { leftRail = value; } }
    public IconRail RightRail { get { return rightRail; } set { rightRail = value; } }
    public List<DrawerPanel> Drawers { get { return drawers; } }
    public List<BoardAction> DrawerActions { get { return drawerActions; } }
    public PlotTools Tools { get { return tools; } set { tools = value; } }

    private void Start()
    {
        // Rebuild here rather than in the rails' own Awake, so the click
        // listeners are attached before anything can press one.
        Hook(leftRail);
        Hook(rightRail);
        didStart = true;
    }

    /// <summary>
    /// OnDisable drops the listeners, so they have to come back. Without this,
    /// disabling and re-enabling this object leaves every rail button dead with
    /// no error to show why.
    /// </summary>
    private void OnEnable()
    {
        if (!didStart) return;          // Start has not run yet; it will hook them
        Hook(leftRail);
        Hook(rightRail);
    }

    private bool didStart;

    private void Hook(IconRail rail)
    {
        if (rail == null) return;
        rail.Rebuild();
        rail.Clicked -= OnAction;
        rail.Clicked += OnAction;
    }

    private void OnDisable()
    {
        if (leftRail != null) leftRail.Clicked -= OnAction;
        if (rightRail != null) rightRail.Clicked -= OnAction;
    }

    private void OnAction(BoardAction a)
    {
        if (a == null) return;

        // Tools go to the plot, not to a drawer.
        if (a.kind == BoardAction.Kind.Tool)
        {
            if (tools != null) tools.Press(a.id);
            else Debug.LogWarning("[Board] No PlotTools assigned; " + a.label + " does nothing.", this);
            return;
        }

        if (!a.OpensAPanel) { Debug.Log("[Board] " + a.label + " has no panel yet."); return; }

        int i = drawerActions.IndexOf(a);
        if (i < 0 || i >= drawers.Count || drawers[i] == null)
        {
            Debug.LogWarning("[Board] No drawer built for " + a.label, this);
            return;
        }

        bool wantOpen = !drawers[i].IsOpen;
        for (int d = 0; d < drawers.Count; d++)          // one drawer at a time
            if (drawers[d] != null) drawers[d].Set(d == i && wantOpen);
    }
}
