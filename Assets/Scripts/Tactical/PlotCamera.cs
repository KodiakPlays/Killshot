using UnityEngine;

/// <summary>
/// Owns the tactical plot's camera: zoom, and optionally following own ship.
///
/// Zoom is one number — the orthographic size. Scrolling sets a target and the
/// actual eases toward it, the same commanded-versus-actual pattern the ship
/// uses. Zoom is anchored on the cursor, so the world point under the pointer
/// stays put instead of the view lurching.
/// </summary>
// Runs before PlotGridFitter and ScreenConstantScale, which both read camera
// state in LateUpdate. Without a fixed order they would use last frame's zoom
// and lag by a frame, which reads as jitter while scrolling.
[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Camera))]
public class PlotCamera : MonoBehaviour
{
    [Header("Zoom")]
    [SerializeField] private float minSize = 400f;
    [Tooltip("Pulled all the way out. The arena is about 50,000 units across.")]
    [SerializeField] private float maxSize = 30000f;
    [Tooltip("Multiplier per scroll notch. 1.0 is no zoom.")]
    [SerializeField] private float stepPerNotch = 1.22f;
    [SerializeField] private float smoothing = 14f;
    [SerializeField] private bool anchorZoomOnCursor = true;

    [Header("Follow")]
    [Tooltip("Leave empty for a fixed camera. Set to own ship to watch it fly.")]
    [SerializeField] private Transform follow;
    [SerializeField] private float followSmoothing = 8f;

    private Camera cam;
    private float targetSize;

    // Where the pointer is, when it is actually over the plot. PlotSurface
    // reports this. Zooming anchors here rather than on the raw mouse position,
    // so the view never lurches toward a point hidden behind a panel or a stale
    // position from when the pointer left the window.
    private Vector2 anchorScreen;
    private bool anchorValid;

    /// <summary>Called by PlotSurface as the pointer moves over or leaves the plot.</summary>
    public void SetPointer(Vector2 screen, bool overPlot)
    {
        anchorScreen = screen;
        anchorValid = overPlot;
    }

    public float TargetSize { get { return targetSize; } }
    public Transform Follow { get { return follow; } set { follow = value; } }

    /// <summary>The plot camera, resolved on demand so nothing depends on Awake.</summary>
    public Camera Cam
    {
        get
        {
            if (cam == null) cam = GetComponent<Camera>();
            return cam;
        }
    }

    /// <summary>World units per screen pixel at the current zoom.</summary>
    public float WorldPerPixel
    {
        get { return (Cam.orthographicSize * 2f) / Mathf.Max(1, Cam.pixelHeight); }
    }

    /// <summary>Screen point to a world point on the plot plane.</summary>
    public Vector3 ScreenToPlot(Vector2 screen)
    {
        var c = Cam;
        var w = c.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -c.transform.position.z));
        w.z = 0f;
        return w;
    }

    /// <summary>World point on the plot plane to a screen point.</summary>
    public Vector2 PlotToScreen(Vector3 world)
    {
        return Cam.WorldToScreenPoint(new Vector3(world.x, world.y, 0f));
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        targetSize = cam.orthographicSize;
    }

    private void LateUpdate()
    {
        if (Cam == null) return;
        if (targetSize <= 0f) targetSize = Cam.orthographicSize;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
            targetSize = Mathf.Clamp(
                targetSize * Mathf.Pow(stepPerNotch, -Mathf.Sign(scroll)), minSize, maxSize);

        if (!Mathf.Approximately(Cam.orthographicSize, targetSize))
        {
            // Anchor only on a pointer that is genuinely over the plot. Otherwise
            // zoom about the centre, which is what you want when the cursor is on
            // a panel or outside the window.
            bool useAnchor = anchorZoomOnCursor && anchorValid && follow == null;
            Vector3 before = useAnchor ? ScreenToPlot(anchorScreen) : Vector3.zero;

            float t = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
            Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, targetSize, t);

            if (useAnchor)
            {
                Vector3 after = ScreenToPlot(anchorScreen);
                transform.position += new Vector3(before.x - after.x, before.y - after.y, 0f);
            }
        }

        if (follow != null)
        {
            float t = 1f - Mathf.Exp(-followSmoothing * Time.unscaledDeltaTime);
            var want = new Vector3(follow.position.x, follow.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, want, t);
        }
    }
}
