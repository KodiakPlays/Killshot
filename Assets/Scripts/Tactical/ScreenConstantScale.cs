using UnityEngine;

/// <summary>
/// Gives an object a MINIMUM size on screen.
///
/// It draws at true world scale whenever that is big enough to see, and stops
/// shrinking once it would fall below <see cref="minPixels"/>. Zoom in and the
/// ship grows like a real object; zoom out and it settles into a glyph instead
/// of vanishing. This is how the prototype handles plot symbols.
///
/// A tactical plot works at tens of thousands of units and a ship is about 116
/// units long, so true scale alone is only readable when zoomed right in.
/// </summary>
[ExecuteAlways]
public class ScreenConstantScale : MonoBehaviour
{
    [SerializeField] private Camera plotCamera;

    [Tooltip("Never draw smaller than this many pixels tall. Above this it is true scale.")]
    [SerializeField] private float minPixels = 170f;

    [Tooltip("Never draw larger than this many pixels tall. 0 means no ceiling.")]
    [SerializeField] private float maxPixels = 0f;

    [SerializeField] private float minScale = 0.05f;
    [SerializeField] private float maxScale = 200000f;

    // Serialized on purpose. Measured fresh each time, every domain reload would
    // take the ALREADY-scaled transform as the new base and compound away.
    [SerializeField, HideInInspector] private Vector3 baseScale = Vector3.one;
    [SerializeField, HideInInspector] private float baseHeight = -1f;

    public Camera PlotCamera { get { return plotCamera; } set { plotCamera = value; } }
    public float MinPixels { get { return minPixels; } set { minPixels = value; } }
    public float MaxPixels { get { return maxPixels; } set { maxPixels = value; } }

    /// <summary>How tall this would draw at true scale, in pixels, right now.</summary>
    public float TruePixels
    {
        get
        {
            if (plotCamera == null || !plotCamera.orthographic || baseHeight <= 0f) return 0f;
            return baseHeight / ((plotCamera.orthographicSize * 2f) / Mathf.Max(1, plotCamera.pixelHeight));
        }
    }

    private void Awake()
    {
        if (baseHeight <= 0f) Measure();   // only ever measure once
    }

    /// <summary>
    /// Record the untouched size, so scaling stays relative to it.
    /// Resets to the stored base first, so calling this twice is safe.
    /// </summary>
    public void Measure()
    {
        if (baseHeight > 0f) transform.localScale = baseScale;
        else baseScale = transform.localScale;

        float h = 0f;
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
            h = Mathf.Max(h, r.bounds.size.y);
        }
        baseHeight = h > 0.0001f ? h : 1f;
    }

    /// <summary>Forget the stored base and measure the current transform afresh.</summary>
    public void Remeasure(Vector3 trueScale)
    {
        baseHeight = -1f;
        transform.localScale = trueScale;
        baseScale = trueScale;
        Measure();
    }

    private void LateUpdate()
    {
        if (plotCamera == null || !plotCamera.orthographic) return;
        if (baseHeight <= 0f) Measure();

        float worldPerPixel = (plotCamera.orthographicSize * 2f) / Mathf.Max(1, plotCamera.pixelHeight);
        float truePixels = baseHeight / worldPerPixel;

        // True scale by default. Only step in when it would be too small to see,
        // or bigger than a ceiling if one is set.
        float wantedPixels = truePixels;
        if (truePixels < minPixels) wantedPixels = minPixels;
        else if (maxPixels > 0f && truePixels > maxPixels) wantedPixels = maxPixels;

        float k = Mathf.Clamp((wantedPixels * worldPerPixel) / baseHeight, minScale, maxScale);
        transform.localScale = baseScale * k;
    }
}
