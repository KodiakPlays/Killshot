using UnityEngine;

/// <summary>
/// The tactical board's theme: colours, frames and shared glyphs.
///
/// Per-button icons are NOT here — they live on individual BoardAction assets,
/// because per-button is the granularity you actually want when swapping art.
///
/// Create with: Assets > Create > Starwolf > Tactical Art Profile
/// </summary>
[CreateAssetMenu(fileName = "TacticalArtProfile", menuName = "Starwolf/Tactical Art Profile")]
public class TacticalArtProfile : ScriptableObject
{
    [Header("Frames and plate")]
    public Sprite metalPlate;
    public Sprite panelFrame;

    [Header("Hull glyphs — drawn on the plot")]
    public Sprite hullFilled;
    public Sprite hullOutline;

    [Header("Plot colours")]
    public Color plotBackground = new Color(0.055f, 0.043f, 0.031f, 1f);
    public Color gridFine       = new Color(0.816f, 0.608f, 0.204f, 0.05f);
    public Color gridMinor      = new Color(0.816f, 0.608f, 0.204f, 0.10f);
    public Color gridMajor      = new Color(0.816f, 0.608f, 0.204f, 0.26f);

    [Header("Chrome colours")]
    public Color iconTint       = new Color(0.816f, 0.608f, 0.204f, 1f);
    public Color iconTintActive = new Color(0.984f, 0.792f, 0.353f, 1f);
    public Color frameTint      = new Color(0.816f, 0.608f, 0.204f, 0.40f);
    public Color panelWell      = new Color(0.039f, 0.059f, 0.067f, 0.86f);
}
