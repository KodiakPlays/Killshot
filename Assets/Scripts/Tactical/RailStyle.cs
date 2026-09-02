using UnityEngine;

/// <summary>
/// The geometry both instrument rails share.
///
/// The design handoff is explicit about this: "Geometrically identical to the
/// left rail ... This symmetry matters; the two rails must never drift apart
/// in size or rhythm." One asset, read by both rails, so they cannot.
///
/// Create with: Assets > Create > Starwolf > Rail Style
/// </summary>
[CreateAssetMenu(fileName = "RailStyle", menuName = "Starwolf/Rail Style")]
public class RailStyle : ScriptableObject
{
    [Header("Geometry (handoff section 4)")]
    public float railWidth = 64f;
    public float cell = 52f;
    public float gap = 5f;
    public float padding = 6f;
    [Tooltip("Distance from the screen edge to the rail. Handoff says 26.")]
    public float edgeInset = 26f;
    public float iconSize = 34f;

    [Header("Symmetry")]
    [Tooltip("Both rails get a plate this many cells tall, whatever they hold, " +
             "so the pair reads as a mirror. Set to the larger rail's count.")]
    public int plateCells = 7;

    [Header("Button face")]
    public Sprite buttonFace;
    [Tooltip("Handoff: semi-transparent so the metal grain reads through.")]
    public Color buttonFaceTint = new Color(0.078f, 0.094f, 0.110f, 0.60f);

    [Header("Backing plate")]
    public Texture2D plate;
    public Color plateTint = new Color(0.72f, 0.72f, 0.74f, 1f);
    [Tooltip("How far the plate extends past the buttons on each side.")]
    public float plateBleed = 6f;

    /// <summary>Outer width of the plate. Used to keep panels clear of the rail.</summary>
    public float PlateWidth { get { return railWidth + plateBleed * 2f; } }

    /// <summary>Height of a rail holding this many buttons.</summary>
    public float HeightFor(int count)
    {
        if (count <= 0) return padding * 2f;
        return count * cell + (count - 1) * gap + padding * 2f;
    }

    /// <summary>The shared plate height, so both rails match.</summary>
    public float PlateHeight { get { return HeightFor(plateCells) + plateBleed * 2f; } }
}
