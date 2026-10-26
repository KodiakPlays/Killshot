using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base for one drawer or station of the handoff board. HandoffBoardControls passes on the input
/// it does not handle itself and asks every panel to redraw after each change and each tick.
/// A panel only reads and changes HandoffBoardState; it never touches game systems.
/// </summary>
public abstract class HandoffPanel : MonoBehaviour
{
    /// <summary>One piece of the panel's art: a sprite and every image that shows it.</summary>
    [System.Serializable]
    public class ArtSlot
    {
        public string name;
        public Sprite sprite;
        public Image[] images;
    }

    [Tooltip("This panel's art. Swap a sprite here and every image listed under it changes.")]
    [SerializeField] private ArtSlot[] art;

    protected HandoffBoardControls Controls { get; private set; }
    protected HandoffBoardState State => Controls.State;

    public void Bind(HandoffBoardControls controls) { Controls = controls; }

    /// <summary>Handle a click; return true if this panel owns the action.</summary>
    public virtual bool Click(string action, string arg) => false;

    /// <summary>Press, drag and release, with the pointer in board px (y down).</summary>
    public virtual bool Down(string action, string arg, Vector2 board) => false;
    public virtual bool Drag(string action, string arg, Vector2 board) => false;
    public virtual bool Up(string action, string arg, Vector2 board) => false;

    /// <summary>Redraw from the state.</summary>
    public abstract void Refresh();

    // Design colours are sRGB; the board blends in gamma space and the canvas linearises vertex
    // colours once, so pre-encode. Keeps the graphic's own alpha.
    public static void Paint(Graphic g, Color design)
    {
        if (!g) return;
        var c = design.gamma;
        c.a = g.color.a;
        g.color = c;
    }

    public static void Show(Component c, bool on) { if (c) c.gameObject.SetActive(on); }

#if UNITY_EDITOR
    // a sprite swapped in the art list goes onto its images
    private void OnValidate() => ApplyArt(art);

    /// <summary>Puts each entry's sprite on its images. The ArtList script uses it too.</summary>
    public static void ApplyArt(ArtSlot[] art)
    {
        if (art == null) return;
        foreach (var a in art)
            if (a != null && a.sprite && a.images != null)
                foreach (var img in a.images)
                    if (img && img.sprite != a.sprite) { img.sprite = a.sprite; UnityEditor.EditorUtility.SetDirty(img); }
    }
#endif

    /// <summary>Text as the browser shows it: runs of spaces collapse to one (white-space: normal).
    /// The board's code builds some lines with "   " that never render (Step 33e verification).</summary>
    public static string Html(string s) => System.Text.RegularExpressions.Regex.Replace(s ?? "", " {2,}", " ");
}
