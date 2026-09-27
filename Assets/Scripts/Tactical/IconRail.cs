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

    [Header("Active state (handoff 6.1)")]
    [Tooltip("Handoff: active gains a lighter face and a 3px left bar.")]
    [SerializeField] private Color activeFaceTint = new Color(0.16f, 0.19f, 0.22f, 0.95f);
    [SerializeField] private float activeBarWidth = 3f;

    private readonly System.Collections.Generic.Dictionary<string, Image> faces =
        new System.Collections.Generic.Dictionary<string, Image>();
    private readonly System.Collections.Generic.Dictionary<string, GameObject> bars =
        new System.Collections.Generic.Dictionary<string, GameObject>();

    /// <summary>
    /// Light up one button and dim the rest. Pass an id this rail does not have
    /// and everything goes dark, which is what you want when the live tool
    /// belongs to the other rail.
    /// </summary>
    public void SetActiveId(string id)
    {
        foreach (var kv in faces)
            if (kv.Value != null)
                kv.Value.color = kv.Key == id ? activeFaceTint : style.buttonFaceTint;

        foreach (var kv in bars)
            if (kv.Value != null)
                kv.Value.SetActive(kv.Key == id);
    }

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

        faces.Clear();
        bars.Clear();

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

            // The 3px left bar that marks the live tool. Built once, shown and
            // hidden by SetActiveId rather than rebuilt.
            var bar = NewRect("ActiveBar", btn, new Vector2(activeBarWidth, style.cell));
            bar.anchorMin = new Vector2(0f, 0.5f);
            bar.anchorMax = new Vector2(0f, 0.5f);
            bar.anchoredPosition = new Vector2(activeBarWidth * 0.5f, 0f);
            var barImg = bar.gameObject.AddComponent<Image>();
            barImg.color = a.tintIcon && art != null ? art.iconTint : a.tint;
            barImg.raycastTarget = false;
            bar.gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(a.id))
            {
                faces[a.id] = face;
                bars[a.id] = bar.gameObject;
            }

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
