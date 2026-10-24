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
    [Tooltip("How far above the ship the camera sits (km). The world is the x/y plane, seen from -z.")]
    [Tune("Plot & our ship")]
    [SerializeField] private float height = 50f;

    private Camera cam;
    private RenderTexture rt;
    private float shipLength = 1f;

    public Texture Texture => rt;
    /// <summary>The ship it watches (our ship, or a contact's model).</summary>
    public Transform Target => target;

    /// <summary>The view of our own ship: the one whose target is not a contact (there may be contact views too).</summary>
    public static ShipView Own()
    {
        foreach (var v in FindObjectsByType<ShipView>(FindObjectsSortMode.None))
            if (v.target && !v.target.GetComponentInParent<WorldContact>()) return v;
        return null;
    }
    /// <summary>The ship's length as a share of the image, so the plot can size the image by the ship.</summary>
    public float ShipFill => shipFill;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        rt = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        { name = "ShipView (" + (target ? target.name : "none") + ")", hideFlags = HideFlags.DontSave, antiAliasing = 4 };
        cam.targetTexture = rt;
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);

        // the ship's longest side, measured once with the model unturned
        if (target)
        {
            var rot = target.rotation;
            target.rotation = Quaternion.identity;
            var b = new Bounds(target.position, Vector3.zero);
            bool any = false;
            foreach (var r in target.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
            target.rotation = rot;
            if (any) shipLength = Mathf.Max(b.size.x, b.size.y);
        }
        cam.orthographicSize = shipLength / shipFill / 2f;
        Follow();
    }

    private void LateUpdate() => Follow();

    void Follow()
    {
        if (!target) return;
        transform.SetPositionAndRotation(target.position + new Vector3(0, 0, -height), Quaternion.identity); // look along +z, north up
    }

    private void OnDestroy()
    {
        if (cam) cam.targetTexture = null;
        if (rt) { rt.Release(); Destroy(rt); }
    }
}
