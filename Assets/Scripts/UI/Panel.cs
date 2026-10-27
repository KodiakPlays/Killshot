using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Base for one drawer or station of the handoff board. BoardManager passes on the input
/// it does not handle itself and asks every panel to redraw after each change and each tick.
/// A panel only reads and changes ShipState; it never touches game systems.
/// </summary>
public abstract class Panel : MonoBehaviour
{
    /// <summary>One piece of the panel's art: a sprite and every image that shows it.</summary>
    [System.Serializable]
    public class ArtSlot
    {
        public string name;
        public Sprite sprite;
        public Image[] images;
    }

    /// <summary>One font of the panel: the font, its material, and every text that uses them.</summary>
    [System.Serializable]
    public class FontSlot
    {
        public string name;
        public TMP_FontAsset font;
        [Tooltip("One of this font's material presets. Leave it empty for the font's own material.")]
        public Material material;
        public TMP_Text[] texts;
    }

    [Tooltip("This panel's art. Swap a sprite here and every image listed under it changes.")]
    [SerializeField] private ArtSlot[] art;
    [Tooltip("This panel's fonts. Swap a font here and every text listed under it changes.")]
    [SerializeField] private FontSlot[] fonts;

    [Tooltip("The board's colours: Assets/UI/Board/Palette.asset. Every panel uses the same file.")]
    [SerializeField] private BoardPalette palette;
    /// <summary>The board's colours. Without a palette file, the design's own colours.</summary>
    protected BoardPalette Palette => BoardPalette.Or(palette);

    protected BoardManager Controls { get; private set; }
    protected ShipState State => Controls.State;

    public void Bind(BoardManager controls) { Controls = controls; }

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
    // a sprite or font swapped in the lists goes onto its images and texts
    private void OnValidate() { ApplyArt(art); ApplyFonts(fonts); }

    /// <summary>Puts each entry's sprite on its images. The ArtList script uses it too.</summary>
    public static void ApplyArt(ArtSlot[] art)
    {
        if (art == null) return;
        foreach (var a in art)
            if (a != null && a.sprite && a.images != null)
                foreach (var img in a.images)
                    if (img && img.sprite != a.sprite) { img.sprite = a.sprite; UnityEditor.EditorUtility.SetDirty(img); }
    }

    /// <summary>Puts each entry's font and material on its texts. A material made for another font
    /// is ignored, and the font's own material is used. The ArtList script uses it too.</summary>
    public static void ApplyFonts(FontSlot[] fonts)
    {
        if (fonts == null) return;
        foreach (var f in fonts)
        {
            if (f == null || !f.font || f.texts == null) continue;
            var mat = f.material && f.material.GetTexture(ShaderUtilities.ID_MainTex) == f.font.atlasTexture ? f.material : f.font.material;
            foreach (var t in f.texts)
            {
                if (!t || (t.font == f.font && t.fontSharedMaterial == mat)) continue;
                t.font = f.font;
                t.fontSharedMaterial = mat;
                UnityEditor.EditorUtility.SetDirty(t);
            }
        }
    }
#endif

    /// <summary>Text as the browser shows it: runs of spaces collapse to one (white-space: normal).
    /// The board's code builds some lines with "   " that never render (Step 33e verification).</summary>
    public static string Html(string s) => System.Text.RegularExpressions.Regex.Replace(s ?? "", " {2,}", " ");
}
