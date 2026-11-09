using UnityEngine;

/// <summary>
/// A small camera looking straight down at one 3D ship in the tactical world, rendering it into a
/// texture the board shows on the plot (owner 2026-10-17: our ship must appear on the plot, as
/// the 3D model seen from above). The same setup will serve the future external viewports.
/// North stays up: the camera follows the ship but never turns; the ship model turns.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ShipView : MonoBehaviour
{
    [SerializeField] private Transform target;
    [Tooltip("Texture size, px (square).")]
    [Tune("Plot & our ship")]
    [SerializeField] private int resolution = 256;
    [Tooltip("The ship's length as a share of the frame; the rest is margin for the turn.")]
    [Tune("Plot & our ship")]
    [SerializeField, Range(0.3f, 1f)] private float shipFill = 0.8f;

    private Camera cam;
    private RenderTexture rt;

    public Texture Texture => rt;
    /// <summary>The ship it watches (our ship, or a contact's model).</summary>
    public Transform Target => target;
    /// <summary>The ship whose model it watches (its stats give the length the plot draws).</summary>
    public Ship Ship { get; private set; }

    /// <summary>The view of our own ship: the one whose target is not a contact (there may be contact views too).</summary>
    public static ShipView Own()
    {
        foreach (var v in FindObjectsByType<ShipView>(FindObjectsSortMode.None))
            if (v.target && !v.target.GetComponentInParent<WorldContact>()) return v;
        return null;
    }
    /// <summary>The ship's length as a share of the image, so the plot can size the image by the ship.</summary>
    public float ShipFill => shipFill;
    /// <summary>The model's width as a share of its length, wings and all (owner 2026-11-02: fighters come out beside it).</summary>
    public float WidthShare => Ship ? Ship.WidthShare : 0.5f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        rt = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        { name = "ShipView (" + (target ? target.name : "none") + ")", hideFlags = HideFlags.DontSave, antiAliasing = 4 };
        cam.targetTexture = rt;
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);

        if (target) Ship = target.GetComponentInParent<Ship>();
        Follow();
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += OnlyOurShip;
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += EveryShip;
    }

    // The models are their real size (owner 2026-11-08), so a ship close by would show in this picture: it films only its own.
    void OnlyOurShip(UnityEngine.Rendering.ScriptableRenderContext c, Camera camera) { if (camera == cam) Hide(true); }
    void EveryShip(UnityEngine.Rendering.ScriptableRenderContext c, Camera camera) { if (camera == cam) Hide(false); }
    void Hide(bool off)
    {
        foreach (var s in Ship.All)
            if (s.transform != target) foreach (var r in s.Models) if (r) r.forceRenderingOff = off;
    }

    private void LateUpdate() => Follow();

    /// <summary>Owner 2026-11-01: a wreck's heading, or null. The plot cuts a wreck's picture across the ship, so the
    /// camera turns with it: the ship's nose is up in the picture, and the plot turns the picture instead.</summary>
    public float? WreckHeading { get; set; }

    void Follow()
    {
        if (!target) return;
        // the model is its Length long (Ship.FitModel), so the frame and the camera's depth follow that number
        float length = Mathf.Max(1f, Ship ? Ship.Stats.length : 1f);
        cam.orthographicSize = length / shipFill / 2f;
        cam.nearClipPlane = length * 0.5f;
        cam.farClipPlane = length * 4f;
        transform.SetPositionAndRotation(target.position + new Vector3(0, 0, -2f * length), Quaternion.Euler(0, 0, -(WreckHeading ?? 0f))); // look along +z, north up
    }

    private void OnDestroy()
    {
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= OnlyOurShip;
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= EveryShip;
        if (cam) cam.targetTexture = null;
        if (rt) { rt.Release(); Destroy(rt); }
    }
}
