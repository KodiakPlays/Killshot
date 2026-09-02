using UnityEngine;

/// <summary>
/// Every number you are likely to want to twiddle, in one asset.
///
/// The values themselves still live on the components — this is not a runtime
/// indirection, so nothing gets slower and nothing hides behind a lookup.
/// Edit here, press Apply, and the values are written into the scene as real
/// serialized values. Capture pulls the scene's current numbers back in.
///
/// Create with: Assets > Create > Starwolf > Tactical Tuning
/// </summary>
[CreateAssetMenu(fileName = "TacticalTuning", menuName = "Starwolf/Tactical Tuning")]
public class TacticalTuning : ScriptableObject
{
    [Header("Movement — PlayerShip")]
    [Tooltip("Speed change per second. Top speed is 100 x engine power, so 50 takes 2s to get there.")]
    public float accelerationRate = 50f;
    [Tooltip("Degrees per second, MULTIPLIED BY ENGINE POWER. At 20 a full circle takes 18s.")]
    public float turnRate = 90f;
    public float dodgeDistance = 50f;
    public float dodgeDuration = 0.2f;
    [Tooltip("Engine bars drained per second at full speed.")]
    public float enginePowerDrainRate = 0.2f;

    [Header("Power — PowerManager")]
    [Tooltip("Bars per second, SPLIT across every system currently drawing.")]
    public float powerFillRate = 0.5f;
    [Tooltip("Bars per second while venting.")]
    public float ventRate = 1f;
    [Tooltip("Reactor capacity. Each bar of system power costs 0.5 of this.")]
    public int reactorMaxPower = 8;

    [Header("Stability — ShipStability")]
    public float maxStability = 100f;
    public float stabilityRecoveryRate = 5f;
    public float turnStabilityDrainMultiplier = 1f;
    public float speedStabilityDrainMultiplier = 0.5f;
    public float dodgeStabilityCost = 45f;

    [Header("Plot camera — PlotCamera")]
    [Tooltip("Zoomed all the way in.")]
    public float zoomMin = 400f;
    [Tooltip("Zoomed all the way out. The arena is about 50,000 units across.")]
    public float zoomMax = 30000f;
    [Tooltip("Where the camera starts.")]
    public float zoomDefault = 30000f;
    [Tooltip("Multiplier per scroll notch.")]
    public float zoomStepPerNotch = 1.22f;
    public float zoomSmoothing = 14f;
    public float followSmoothing = 8f;

    [Header("Own ship glyph — ScreenConstantScale")]
    [Tooltip("MINIMUM size on screen, in pixels. Zoomed out the ship never drops " +
             "below this. Zoomed in past the crossover it grows to true scale.")]
    public float shipMinPixels = 170f;
    [Tooltip("Maximum size on screen, in pixels. 0 means no ceiling.")]
    public float shipMaxPixels = 0f;

    [Header("Grid — TacticalGrid.mat")]
    [Tooltip("World units between minor lines.")]
    public float gridCellSize = 2000f;
    [Tooltip("Fine lines per cell. 5 gives a 400-unit fine grid.")]
    public float gridFineDivisions = 5f;
    [Tooltip("A major line every N cells.")]
    public float gridMajorEvery = 5f;
    [Tooltip("Stroke width in PIXELS, not world units. Stays constant at any zoom.")]
    public float gridLineWidthPixels = 1.1f;
    public float gridMajorWidthScale = 1.8f;
    public float gridFineWidthScale = 0.8f;
    [Tooltip("A grid tier vanishes once its lines crowd closer than this many pixels.")]
    public float gridFadeMinPixels = 6f;
    [Tooltip("A grid tier is at full strength once its lines are this far apart.")]
    public float gridFadeMaxPixels = 26f;
}
