using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Text for the plot's annotations. Pooled TMP objects, repositioned each frame
/// from world coordinates, so a label stays stuck to its mark while you pan and
/// zoom. Text cannot come out of PlotOverlay's mesh — that is one Graphic
/// drawing lines.
/// </summary>
public class PlotLabels : MonoBehaviour
{
    [SerializeField] private PlotOverlay overlay;
    [SerializeField] private PlotCamera plot;
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private float fontSize = 15f;
    [SerializeField] private Vector2 offset = new Vector2(12f, -10f);

    [Tooltip("Vertical gap used when two labels land on the same spot.")]
    [SerializeField] private float lineHeight = 20f;

    private readonly List<TextMeshProUGUI> pool = new List<TextMeshProUGUI>();
    private readonly List<Vector2> taken = new List<Vector2>();

    private void LateUpdate()
    {
        if (overlay == null || plot == null || font == null) return;

        var marks = overlay.Marks;
        int used = 0;
        taken.Clear();

        for (int i = 0; i < marks.Count; i++)
        {
            string text = marks[i].Label;
            if (string.IsNullOrEmpty(text)) continue;

            var t = Get(used++);
            t.text = text;
            t.color = marks[i].colour;

            // A point and a ruler end can share a coordinate exactly. Stack them
            // instead of printing one over the other.
            var p = ToLocal(marks[i].LabelAnchor) + offset;
            for (int g = 0; g < taken.Count; g++)
                if (Vector2.Distance(p, taken[g]) < lineHeight) { p.y -= lineHeight; g = -1; }
            taken.Add(p);

            ((RectTransform)t.transform).anchoredPosition = p;
        }

        for (int i = used; i < pool.Count; i++) pool[i].gameObject.SetActive(false);
    }

    private Vector2 ToLocal(Vector2 world)
    {
        // Screen Space - Camera canvas: passing null here silently returns zero
        // and every label stacks in the middle.
        var canvas = GetComponentInParent<Canvas>();
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform, plot.PlotToScreen(world),
            canvas != null ? canvas.worldCamera : null, out local);
        return local;
    }

    private TextMeshProUGUI Get(int i)
    {
        while (pool.Count <= i)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0f, 1f);            // grows right and down from the mark
            rt.sizeDelta = new Vector2(220f, 44f);
            rt.localScale = Vector3.one;

            var t = go.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.fontSharedMaterial = font.material;      // font alone leaves this null and the text invisible
            t.fontSize = fontSize;
            t.alignment = TextAlignmentOptions.TopLeft;
            t.raycastTarget = false;
            pool.Add(t);
        }
        pool[i].gameObject.SetActive(true);
        return pool[i];
    }
}
