using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A vertical strip of icon buttons down one edge of the board.
///
/// Geometry comes from a shared RailStyle, so the left and right rails cannot
/// drift apart. Contents come from a list of BoardAction assets.
/// </summary>
public class IconRail : MonoBehaviour
{
    public enum Side { Left, Right }

    [Header("Source")]
    [SerializeField] private RailStyle style;
    [SerializeField] private TacticalArtProfile art;
    [SerializeField] private Side side = Side.Left;
    [SerializeField] private List<BoardAction> actions = new List<BoardAction>();

    /// <summary>Raised when a rail button is pressed.</summary>
    public event System.Action<BoardAction> Clicked;

    public RailStyle Style { get { return style; } set { style = value; } }
    public TacticalArtProfile Art { get { return art; } set { art = value; } }
    public Side WhichSide { get { return side; } set { side = value; } }
    public List<BoardAction> Actions { get { return actions; } }

    /// <summary>Outer edge of this rail's plate, in pixels from its own screen edge.</summary>
    public float OuterEdge
    {
        get { return style == null ? 0f : style.edgeInset + style.PlateWidth; }
    }

    /// <summary>Wipe the children and lay them out again.</summary>
    public void Rebuild()
    {
        if (style == null)
        {
            Debug.LogWarning("[IconRail] No RailStyle on " + name, this);
            return;
        }

        var self = (RectTransform)transform;
        for (int i = self.childCount - 1; i >= 0; i--)
        {
            var child = self.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
        }

        // Sit against our own edge of the screen, vertically centred.
        bool left = side == Side.Left;
        self.anchorMin = self.anchorMax = new Vector2(left ? 0f : 1f, 0.5f);
        self.pivot = new Vector2(0.5f, 0.5f);
        self.sizeDelta = new Vector2(style.railWidth, style.PlateHeight);
        float x = style.edgeInset + style.PlateWidth * 0.5f;
        self.anchoredPosition = new Vector2(left ? x : -x, 0f);
        self.localScale = Vector3.one;

        // The plate is the same height on both rails, whatever each one holds,
        // so the pair reads as a mirror. Handoff section 4.
        var plate = NewRect("Plate", self, new Vector2(style.PlateWidth, style.PlateHeight));
        var praw = plate.gameObject.AddComponent<RawImage>();
        praw.texture = style.plate;
        praw.color = style.plateTint;
        praw.raycastTarget = false;
        if (style.plate != null)
            praw.uvRect = new Rect(0f, 0f,
                style.PlateWidth / style.plate.width,
                style.PlateHeight / style.plate.height);

        // Buttons are centred inside the shared plate height.
        int n = actions.Count;
        float stack = style.HeightFor(n) - style.padding * 2f;
        float top = stack * 0.5f;

        for (int i = 0; i < n; i++)
        {
            var a = actions[i];
            if (a == null) continue;

            var btn = NewRect(string.IsNullOrEmpty(a.id) ? a.name : a.id, self,
                              new Vector2(style.cell, style.cell));
            btn.anchoredPosition = new Vector2(
                0f, top - style.cell * 0.5f - i * (style.cell + style.gap));

            var face = btn.gameObject.AddComponent<Image>();
            face.sprite = style.buttonFace;
            face.color = style.buttonFaceTint;
            face.type = Image.Type.Sliced;

            var button = btn.gameObject.AddComponent<Button>();
            button.targetGraphic = face;
            var captured = a;
            button.onClick.AddListener(() => { if (Clicked != null) Clicked(captured); });

            var glyph = NewRect("Icon", btn, new Vector2(style.iconSize, style.iconSize));
            var img = glyph.gameObject.AddComponent<Image>();
            img.sprite = a.icon;
            img.color = a.tintIcon
                ? (art != null ? art.iconTint : a.tint)
                : Color.white;
            img.raycastTarget = false;
            img.preserveAspect = true;
        }
    }

    private static RectTransform NewRect(string n, Transform parent, Vector2 size)
    {
        var go = new GameObject(n, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;   // canvases in camera mode hand down a scale
        return rt;
    }
}
