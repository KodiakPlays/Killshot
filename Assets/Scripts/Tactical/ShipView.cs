// =================================================================================================
// ShipView.cs
//
// What it does:
// A ShipView is a small camera that films one 3D ship model from straight above. It makes a
// square picture, and the plot shows it as the ship. The board scene has one for our ship and one
// for an enemy ship's model. More are copied during play.
//
// How it works:
// - When it starts it makes a square picture, 256 pixels by default. Its camera films into it.
//   It finds the Ship it is watching.
// - Every frame, after everything has moved, it moves over its ship. The frame is sized to the
//   ship's length from its stats file. The ship fills 80 percent of the frame by default.
// - North stays up. The camera never turns, and the model turns inside the picture. For a wreck,
//   the camera turns with the wreck, and the plot turns the picture instead.
// - While this camera films, it hides every other ship's model. Each picture shows only its ship.
// - A ship that joins during a level, such as one of the battle group, starts with no camera.
//   TacticalWorld asks ShipView for one. It copies an enemy ship's camera and points it at the
//   new ship. A big ship's copy gets a sharper picture: one pixel per 4 units of its length, up to
//   2,048.
// - PlotPanel uses the pictures to draw our ship and contacts on the plot. PeriscopePanel uses the
//   enemy pictures in the targeting computer.
// - The picture size, the ship's share of the frame and the units per pixel are in the Tuning
//   window. They are under Plot and our ship.
//
// Technical:
// - Needs a Camera. Inspector: target (the model's transform), then [Tune("Plot & our ship")]
//   resolution (256 px), shipFill (0.8) and unitsPerPx (4).
// - Awake() makes the render texture (ARGB32, linear, 4x smoothing). It adds itself to
//   ShipView.All.
// - LateUpdate() calls Follow(). The camera is orthographic, 2 lengths above the model. The
//   frame's height is length divided by shipFill.
// - Length comes from Ship.Stats.length, the stats file. Ship.FitModel sizes the model to it.
// - Hiding: it listens to RenderPipelineManager begin and end camera events. It sets
//   forceRenderingOff on every other Ship.All model while this camera draws.
// - Give(ship) is called by TacticalWorld when a ship joins. It copies a view whose target is a
//   WorldContact. With none in the scene, the new ship gets no picture. Sharpen() then sizes the
//   copy's picture to a power of two, from resolution up to 2,048 px.
// - Own() finds the view whose target is not a WorldContact: our ship.
// - Readers: PlotPanel (Own, All, Texture, ShipFill, WidthShare) and PeriscopePanel (Texture,
//   ShipFill, WreckHeading). Trap: PeriscopePanel lists the views once. Later copies are missing.
// - PlotPanel sets WreckHeading for a wreck. Null means north up.
// =================================================================================================

using UnityEngine;

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

    /// <summary>Every ship view now open.</summary>
    public static readonly System.Collections.Generic.List<ShipView> All = new System.Collections.Generic.List<ShipView>();

    /// <summary>Owner 2026-11-10: a ship in sight shows its line picture, never a dot. A ship a level brings (the battle group)
    /// has no camera of its own in the board scene, so it gets a copy of a contact's camera, watching it.</summary>
    public static void Give(Ship ship)
    {
        if (!ship) return;
        ShipView model = null;
        foreach (var v in All)
        {
            if (v.target == ship.transform) return;
            if (!model && v.target && v.target.GetComponentInParent<WorldContact>()) model = v;
        }
        if (!model) return;
        var copy = Instantiate(model, model.transform.parent);
        copy.name = "Contact View Camera · " + ship.name;
        copy.target = ship.transform;
        copy.Ship = ship;
        copy.WreckHeading = null;
        copy.Sharpen(ship.Stats.length);
    }

    [Tooltip("Owner 2026-11-10: a big ship or a station gets a sharper picture: one pixel for every this many units of its length "
           + "(never below Resolution, at most 2,048 px). A station 3,000 units long was blocky at 256 px.")]
    [Tune("Plot & our ship")]
    [SerializeField] private float unitsPerPx = 4f;

    void Sharpen(float length)
    {
        int px = Mathf.Clamp(Mathf.NextPowerOfTwo(Mathf.CeilToInt(length / Mathf.Max(0.1f, unitsPerPx))), resolution, 2048);
        if (!cam || (rt && rt.width >= px)) return;
        cam.targetTexture = null;
        if (rt) { rt.Release(); Destroy(rt); }
        rt = new RenderTexture(px, px, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear) { name = "ShipView (" + name + ")", hideFlags = HideFlags.DontSave, antiAliasing = 4 };
        cam.targetTexture = rt;
    }

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
        All.Add(this);
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
        All.Remove(this);
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= OnlyOurShip;
        UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= EveryShip;
        if (cam) cam.targetTexture = null;
        if (rt) { rt.Release(); Destroy(rt); }
    }
}
