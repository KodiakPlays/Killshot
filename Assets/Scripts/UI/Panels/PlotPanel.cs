using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 33i. The plot tools and plot clicks: POINT, RULER, COMPASS marks with their labels, ERASE
/// (click a mark to delete it), the capture surface that takes the pointer only for the three
/// marking tools, and the passive bearing label that puts the periscope on its contact (lookAt).
/// Rules and values from board-logic.js (plotPt, plotDown, plotMove, plotUp, eraseMark,
/// pressErase, lookAt, pickTarget, validSet; renderVals marksView, plotCapture, passives) and
/// UIStateSpec §8.4.
/// </summary>
public class PlotPanel : Panel
{
    [System.Serializable]
    public class Passive
    {
        public string key;
        public TMP_Text label;
    }

    [SerializeField] private Graphic capture;
    [SerializeField] private MarksGraphic marks;
    [SerializeField] private TMP_Text labelTemplate;
    [SerializeField] private RectTransform hitLayer;
    [SerializeField] private Passive[] passives;

    [Header("Our ship on the plot (Step 37): the 3D model seen from above (ShipView)")]
    [SerializeField] private RawImage ownShipImage;
    [Tooltip("A ship is drawn at its length (its stats file) at the plot's scale: ours, 200 units, is 152 px at x1. "
           + "It shrinks with zoom like the design's glyph, but never below this (older handoff: legible at 30 px) "
           + "and never above the maximum (owner 2026-10-17). The maximum is big enough for the Garuda at full zoom-in (x4, 1,064 px), "
           + "so a picture is never smaller than its ship in the close-up.")]
    [Tune("Plot & our ship")]
    [SerializeField] private float ownShipMinPx = 30f, ownShipMaxPx = 1200f;
    Color ownShipColor => Palette.ownShip;
    private ShipView shipView;

    [Header("Ruins on the plot (owner 2026-10-27): the level's terrain within visual range, from above, behind the marks")]
    [Tooltip("Covers the plot. Its own camera draws the WorldTerrain layer at the plot's pan and zoom (TacticalWorld shows only the pieces within visual range). "
           + "Needs the BoardLinearImage material, as our ship's view: the line art comes red on black, and the material keys the black out.")]
    [SerializeField] private RawImage ruinsImage;
    Color ruinsColor => Palette.secondary;
    private Camera ruinsCamera;
    private RenderTexture ruinsTexture;

    [Header("Close-up (owner 2026-11-08): zoomed in, ships and rocks in sight show as they really look")]
    [Tooltip("Covers the plot, just above the Ground and under the grid. Its own camera films ships and rocks at the plot's pan and zoom, "
           + "with the Close Up Renderer: grey metal ships, rock, lit by the Sun light in the board scene, with a soft glow. It is cut at the visual range circles.")]
    [SerializeField] private RawImage closeUpImage;
    [Tooltip("The close-up starts to show at the first zoom and has fully replaced the line pictures at the second. Zoomed out past the first, only the line pictures show.")]
    [Tune("Plot & our ship")] [SerializeField] private float closeUpFromZoom = 1f, closeUpFullZoom = 1.5f;
    [Tooltip("The close-up camera's renderer: its number in the list on Assets/Settings/PC_RPAsset (the Close Up Renderer).")]
    [SerializeField] private int closeUpRenderer = 1;
    [Tooltip("The close-up's glow and colour. Open the file to change them.")]
    [SerializeField] private VolumeProfile closeUpLook;
    [Tooltip("The way the sunlight comes from, degrees clockwise from north (up the plot).")]
    [Tune("Plot & our ship")] [SerializeField] private float sunFromDeg = 300f;
    [Tooltip("How high the sun is above the plot, degrees: 90 shines straight down, a low sun catches the edges.")]
    [Tune("Plot & our ship")] [Range(5f, 90f)] [SerializeField] private float sunHeightDeg = 35f;
    [System.Serializable]
    public class NebulaLayer
    {
        [Tooltip("The picture. It repeats in every direction. To change the look, put another picture in its Texture.")]
        public RawImage image;
        [Tooltip("How near it looks, 0 to 1. At 1 it moves with the grid as you pan and zoom; at 0.1 it hardly moves, so it looks far away.")]
        [Range(0f, 1f)] public float depth = 0.2f;
        [Tooltip("One copy of the picture is this many units across at x1.")]
        [Min(1f)] public float tileUnits = 4000f;
        [Tooltip("How strongly it shows, 0 to 1.")]
        [Range(0f, 1f)] public float alpha = 1f;
    }
    [Tooltip("Owner 2026-11-08: space under the ships in the close-up, the farthest layer first. The pictures are in Assets/Art/Close Up.")]
    [SerializeField] private NebulaLayer[] nebula;
    private Material nebulaMaterial; // its own copy: it shows its pictures' see-through parts the usual way

    [Header("Close-up shots and blasts (owner 2026-11-08: a shot fired looks like a shot)")]
    [Tooltip("What shot streaks are drawn with: Assets/Art/Close Up/Glow.mat. Its Colour sets how bright every glow is.")]
    [SerializeField] private Material glow;
    [Tooltip("Plays where a shot hits, or a torpedo is shot down: Assets/Art/Close Up/Hit Flash.prefab.")]
    [SerializeField] private ParticleSystem hitFlash;
    [Tooltip("Plays where a ship is destroyed: Assets/Art/Close Up/Kill Blast.prefab.")]
    [SerializeField] private ParticleSystem killBlast;
    [Tooltip("A destroyed ship's two halves in the close-up: Assets/Art/Close Up/Wreck Metal.mat.")]
    [SerializeField] private Material wreckLook;
    [Tooltip("Owner 2026-11-10: a torpedo's model in the close-up, a small cylinder: Assets/Art/Close Up/Ship Metal.mat. With none, a pale stroke stands in.")]
    [SerializeField] private Material torpedoLook;
    private readonly List<Transform> torpedoModels = new List<Transform>();
    [Tooltip("A shot streak's width at the front, units: a railgun slug, then a torpedo.")]
    [Tune("Plot & our ship")] [SerializeField] private float slugWidthUnits = 6f, torpedoWidthUnits = 11f;
    [Tooltip("Owner 2026-11-10 (drop, drift, flare): a torpedo's body in the close-up, along its nose, this long in units, and its colour. It drifts cold before its motor lights.")]
    [Tune("Plot & our ship")] [SerializeField] private float torpedoBodyUnits = 14f;
    [Tune("Plot & our ship")] [SerializeField] private Color torpedoBodyInk = new Color(0.87f, 0.9f, 0.93f, 0.7f);
    [Tooltip("Its motor bursts on with a flare this big across in the close-up, units. It grows and fades over Torpedo Flare Seconds (game seconds). On the table a ring grows by Torpedo Ring Px where it lit.")]
    [Tune("Plot & our ship")] [SerializeField] private float torpedoFlareUnits = 60f, torpedoFlareSeconds = 0.6f, torpedoRingPx = 16f;
    [Tooltip("The colour of our shots' streaks, then the enemy's. The Glow material makes them brighter still.")]
    [Tune("Plot & our ship")] [SerializeField] private Color ourShotGlow = new Color(0.5f, 0.75f, 1f), theirShotGlow = new Color(1f, 0.35f, 0.15f);
    const float EffectZ = -100f; // in front of the hulls, toward the close-up camera
    struct Shot { public WeaponSystem.Round r; public Vector3 head, tail; public bool ours; }
    private readonly List<Shot> shotsNow = new List<Shot>();
    private readonly Dictionary<WeaponSystem.Round, Vector3> shotsBefore = new Dictionary<WeaponSystem.Round, Vector3>();
    private readonly List<LineRenderer> streaks = new List<LineRenderer>();
    private readonly HashSet<WeaponSystem.Round> flying = new HashSet<WeaponSystem.Round>();
    private readonly List<ParticleSystem[]> blasts = new List<ParticleSystem[]>(); // flashes and blasts playing: they run in game time
    const float StreakTail = 0.35f; // a streak's width at its tail, as a share of its width at the front
    const float BlastWithinSeconds = 2f; // a kill first seen this many game seconds after it happened gets no blast
    class Halves { public Transform[] piece = new Transform[2]; public Vector3[] centre = new Vector3[2]; }
    private readonly Dictionary<ShipState.Track, Halves> wrecks = new Dictionary<ShipState.Track, Halves>();
    private Camera closeUpCamera;
    private RenderTexture closeUpTexture;
    // the ships the close-up leaves out: those out of sight (a rock in the way), and wrecks, which keep their line
    // picture broken in two (owner 2026-11-01)
    private readonly HashSet<NavSystem> notInCloseUp = new HashSet<NavSystem>();
    /// <summary>How far the close-up has replaced the line pictures at this zoom, 0 to 1.</summary>
    float CloseUp => closeUpImage ? Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(closeUpFromZoom, closeUpFullZoom, (float)Controls.ZoomExact)) : 0f;

    [Header("Gas clouds on the plot (owner 2026-11-01): each cloud in visual range, behind the ruins and the marks")]
    [Tooltip("The picture for one cloud. It stays switched off and is copied for each cloud. Its sprite is on this panel's Art list.")]
    [SerializeField] private Image cloudImage;
    [Tooltip("How strongly a cloud shows on the grid, 0 to 1. It is drawn in the ruins' colour.")]
    [Tune("Plot & our ship")] [Range(0f, 1f)] [SerializeField] private float cloudAlpha = 0.45f;
    [Tooltip("Owner 2026-11-02: past visual range, while DRADIS listens, a cloud shows as a hollow circle. How strong the circle is, 0 to 1.")]
    [Tune("Sensors")] [Range(0f, 1f)] [SerializeField] private float cloudRingAlpha = 0.5f;
    [Tooltip("The circle's size: this many times the cloud's average half size.")]
    [Tune("Sensors")] [SerializeField] private float cloudRingSize = 1f;
    private readonly List<Image> cloudImages = new List<Image>();
    private TacticalWorld world;

    [Header("Sensor circle (Step 42): what DRADIS can hear, around our ship")]
    [Tooltip("Owner 2026-10-18: while DRADIS is on, a circle around our ship at the sensor (hearing) range.")]
    [SerializeField] private MarksGraphic sensorRing;
    [Tooltip("The circle eases to a new range (UIStateSpec §9.1, lines 204 and 210: .5 s).")]
    [Tune("Sensors")] [SerializeField] private Eased sensorRingMotion = new Eased(0.5f, CssEase.Ease);
    [Tooltip("senEdge / senGlow: the ring and the fill inside it; white grid ink.data .26 / .04, amber grid alarm .3 / .05.")]
    [SerializeField] private float senEdgeAlpha = 0.26f, senGlowAlpha = 0.04f, senEdgeAlphaAmber = 0.3f, senGlowAlphaAmber = 0.05f;
    private float sensorUnits;
    private System.Action<float> setSensorUnits;
    [Tooltip("Owner 2026-10-26: the active scan cone, while DRADIS and ACTIVE are on: bright edges and a faint fill, from the small circle round our ship out to the scan's range.")]
    [Tune("Sensors")] [SerializeField] private float coneEdgeAlpha = 0.6f, coneFillAlpha = 0.07f;
    private float coneUnits;
    private Eased coneMotion; // eases like the sensor circle
    private System.Action<float> setConeUnits;

    [Header("Radar sweep (Step 43): turns inside the sensor circle while DRADIS is on")]
    [SerializeField] private SweepGraphic sweep;
    [Tooltip("One turn takes base − perGain × gain seconds (sweepDur: 5.4 − senBoost × .5). It holds still while the board is paused (owner rule).")]
    [Tune("Sensors")] [SerializeField] private float sweepSecondsBase = 5.4f, sweepSecondsPerGain = 0.5f;
    [Tooltip("How much of the sweep line shows: 1 as designed, 0 unseen (owner 2026-10-29: it spins wildly at x10). "
           + "Unseen, it still turns, leaves bearing lines and identifies.")]
    [Tune("Sensors")] [Range(0f, 1f)] [SerializeField] private float sweepOpacity = 0f;
    private float sweepDeg;
    [Tooltip("The small circle round our ship (owner 2026-10-24) sits this far outside the ship's picture, px; the sweep starts there.")]
    [Tune("Sensors")] [SerializeField] private float innerRingMarginPx = 8f;
    /// <summary>The inner circle's radius on screen: half our ship's drawn length plus the margin.</summary>
    float InnerRingPx => ShipPx(shipView) / 2f + innerRingMarginPx;
    /// <summary>Owner 2026-10-29: a ship's length on screen, px: its length (units) at the plot's scale and zoom, within the min and max.</summary>
    float ShipPx(ShipView v) => Mathf.Clamp((v && v.Ship ? v.Ship.Stats.length : 0f) / (float)UnitsPerPx * (float)Controls.ZoomExact, ownShipMinPx, ownShipMaxPx);
    /// <summary>The sweep's bearing now, degrees clockwise from north; and whether it shows.</summary>
    public float SweepDeg => sweepDeg;
    public bool SweepShown => sweep && sweep.isActiveAndEnabled;
    /// <summary>The sensor circle's radius now, units (it eases), or 0 while DRADIS is off or SEN has no power.</summary>
    public float SensorRingUnits => State != null && State.sensors.Listening ? sensorUnits : 0f;

    [Header("Passive bearings (Step 44): contacts DRADIS hears but has not identified")]
    [SerializeField] private MarksGraphic passiveLines;
    [Tooltip("A hidden plot label carrying a plot.look hit (the export's text #39), cloned per heard contact.")]
    [SerializeField] private TMP_Text passiveLabelTemplate;
    [Tooltip("Owner 2026-11-02: a heard contact is a red bar curved along the DRADIS circle, toward it, turning as it moves: how much of the circle it covers, degrees.")]
    [Tune("Sensors")] [SerializeField] private float passiveBarDeg = 6f;
    [Tooltip("How thick the bar is, px. Its BRG label is just outside it.")]
    [Tune("Sensors")] [SerializeField] private float passiveBarWidthPx = 4f;
    [Tooltip("The bar's picture, bent along the circle end to end (Art/Plot/DradisContactBar.png). Draw it white: it is coloured red here.")]
    [SerializeField] private Texture passiveBarArt;
    private MarksGraphic barMarks; // the bars' own layer, a copy of passiveLines with the picture
    private readonly List<TMP_Text> passiveLabels = new List<TMP_Text>();

    [Header("Contacts on the plot (Step 34): identified hostiles at their true range and bearing")]
    [Tooltip("Stand-in art until Claude Design's contact glyphs arrive (CLAUDE_DESIGN_NEEDS 1.2).")]
    [SerializeField] private MarksGraphic contactMarks;
    [SerializeField] private TMP_Text contactLabelTemplate;
    [Tooltip("Owner 2026-10-26 (Step 75): a contact in sight (visualRangeUnits) shows as its own 3D model seen from above, in "
           + "place of its dot (a ShipView camera on the contact's model): red identified, amber unknown, grey a wreck.")]
    [SerializeField] private RawImage contactShipImage;
    [Tooltip("Owner 2026-11-01: a killed ship breaks in two. How fast the halves drift apart, each, units a game second.")]
    [Tune("Plot & our ship")] [SerializeField] private float wreckDriftUnits = 5f;
    [Tooltip("How fast the halves turn, degrees a game second.")]
    [Tune("Plot & our ship")] [SerializeField] private float wreckSpinDeg = 3f;
    [Tooltip("Out of sight, a wreck's dot fades away over this many game seconds.")]
    [Tune("Plot & our ship")] [SerializeField] private float wreckDotFadeSeconds = 120f;
    private RawImage wreckBackHalf; // a copy of contactShipImage, made at the first break
    private bool wreckPictured;
    Color wreckInk => Palette.secondary;
    private ShipView[] shipViews;
    /// <summary>A contact's ship picture is on the plot (the playtest reads it).</summary>
    public bool ContactShipShown => contactShipImage && contactShipImage.enabled;
    [Tune("Plot & our ship")]
    [SerializeField] private Vector2 contactLabelOffset = new Vector2(10, 0);
    private readonly List<TMP_Text> contactLabels = new List<TMP_Text>();

    [Tooltip("A passive label shows only while its contact is heard and not identified (passives).")]
    [SerializeField] private bool passiveVisibility = true;

    // Plot space (§8.4): the scale is the grid's (owner 2026-10-29: at x1 a small square is 50 units).
    // The plot's middle in board px. It was a fixed (960, 545). The plot now grows with screens that aren't 16:9
    // (owner 2026-11-10), so the middle comes from the grid's size.
    Vector2 plotCentre => (marks ? marks.origin : new Vector2(104, 100)) + (grid ? grid.rectTransform.rect.size / 2f : new Vector2(856, 445));

    // Step 41 (owner 2026-10-18): the grid stays put and our ship moves across it. The plot shows the
    // world around a view centre, which starts on our ship. Drag the empty grid, or hold the arrow
    // keys, to pan.
    [Header("Panning the plot (Step 41)")]
    [SerializeField] private PlotGrid grid;
    [Tooltip("Arrow keys pan the plot this fast, screen px per second.")]
    [Tune("Plot & our ship")] [SerializeField] private float panKeysPxPerS = 600f;
    [Tooltip("Owner 2026-10-29: the plot's keys. Change them on the Keys tab of Starwolf → Tuning.")]
    [Tune("Keys")] [SerializeField] private KeyCode centreOnShipKey = KeyCode.C, panLeftKey = KeyCode.LeftArrow, panRightKey = KeyCode.RightArrow, panUpKey = KeyCode.UpArrow, panDownKey = KeyCode.DownArrow;
    private double viewX, viewY, panX0, panY0;
    private bool viewSet, panning;
    private Vector2 panPress;

    [Header("The grid forms as the board appears (Step 120)")]
    [Tooltip("Owner 2026-10-26, slowed 2026-11-01: how long the grid's lines take to glide in, real seconds: big squares first, then small.")]
    [Tune("Plot & our ship")] [SerializeField] private float gridFormSeconds = 4f;
    [Tooltip("How long our ship, the axis numbers and the rest of the plot then take to fade in, real seconds.")]
    [Tune("Plot & our ship")] [SerializeField] private float plotFadeSeconds = 1.5f;
    private const float FormLongestStep = 0.1f; // a stall while the level loads doesn't use up the forming time
    private float formClock;
    private List<CanvasGroup> fading;
    /// <summary>The grid has formed and the plot has faded in.</summary>
    public bool GridFormed => formClock > 0f && fading == null;

    /// <summary>Owner 2026-10-29: the plot follows our ship as it moves, until you pan it; C follows again.</summary>
    private bool following = true;
    /// <summary>The world point at the plot centre, units.</summary>
    public Vector2 View => new Vector2((float)viewX, (float)viewY);
    /// <summary>C, and a level's start point (Step 73): the view jumps to our ship, and follows it again. Before the
    /// board is up there is nothing to do: its first refresh centres on our ship anyway.</summary>
    public void CentreOnOurShip()
    {
        following = true;
        if (!Controls) return;
        viewX = State.nav.DrawnX; viewY = State.nav.DrawnY; viewSet = true;
        Refresh();
    }

    [Header("Axis numbers (Step 65; distances from our ship since Step 76)")]
    [Tooltip("The numbers are made under this, at run time (Plot/Scale).")]
    [SerializeField] private RectTransform axisLayer;
    [SerializeField] private TMP_FontAsset axisFont;
    [Tooltip("The board thickens Share Tech Mono at small sizes with its own material. Empty uses the font's own.")]
    [SerializeField] private Material axisMaterial;
    [SerializeField] private float axisFontSize = 11f;
    [SerializeField] private Color axisColour = new Color(0.7314692f, 0.759380937f, 0.7812595f, 1f);
    [Tooltip("Owner 2026-11-01: the edge numbers follow the square sizes. A size whose squares are at least this wide, board px, has a number on every line.")]
    [Tune("Plot & our ship")] [SerializeField] private float axisAllNumbersPx = 60f;
    [Tooltip("A size whose squares are this wide or narrower, board px, has no numbers: the next size up takes over.")]
    [Tune("Plot & our ship")] [SerializeField] private float axisHalfNumbersPx = 40f;

    [Header("Labels — offsets have no value in the package (HANDOFF_GAPS)")]
    [Tune("Plot tools")]
    [SerializeField] private Vector2 pointLabelOffset = new Vector2(10, 0);
    [Tune("Plot tools")]
    [SerializeField] private Vector2 rulerLabelOffset = new Vector2(0, -12);
    [Tune("Plot tools")]
    [SerializeField] private Vector2 ringLabelOffset = new Vector2(8, 0);
    [Tooltip("Step 100 (owner 2026-10-27): the size of the RULER and COMPASS labels, pt. 0: the template's own.")]
    [Tune("Plot tools")] [SerializeField] private float markLabelSize = 14f;
    [Tooltip("ERASE click area: the point box, and the ruler's height (§8.4).")]
    [SerializeField] private float pointBox = 16f, rulerBox = 14f;
    [Tune("Plot tools")]
    [SerializeField] private float draftAlpha = 0.75f;

    [Header("Intercept helpers (Step 46, owner 2026-10-23)")]
    [Tooltip("A RULER end within this many screen px of a POINT snaps to it.")]
    [Tune("Plot tools")] [SerializeField] private float rulerSnapPx = 12f;
    [Tooltip("A track (a RULER from one POINT to another) projects its course this far ahead, mission seconds.")]
    [Tune("Plot tools")] [SerializeField] private float projectSeconds = 300f;
    [Tooltip("A slug in flight trails this many screen px behind it (Step 48). Any other shot trails this much for its share of a slug's speed.")]
    [Tune("Periscope & fire control")] [SerializeField] private float slugTrailPx = 120f;
    [Tooltip("The selected weapon's reach, a dashed ring round our ship while Fire Control is open.")]
    [SerializeField] private MarksGraphic weaponRing;
    [Tooltip("A PDC arc's curved edge is drawn in steps this many degrees long (Step 55).")]
    [SerializeField] private float pdcArcStepDeg = 2f;
    [Tooltip("Owner 2026-11-01: each railgun's firing arc is a soft dotted cone while Fire Control is open; the selected turret this strong, the other less.")]
    [Tune("Periscope & fire control")] [Range(0f, 1f)] [SerializeField] private float railArcAlpha = 0.6f, railArcAlphaOther = 0.3f;
    [Tooltip("Owner 2026-11-01: a locked ship has the targeting computer's white corners on the plot, round its length: their smallest half-size, px.")]
    [Tune("Periscope & fire control")] [SerializeField] private float lockCornersPx = 16f;
    [Tooltip("How long each corner's arms are, as a share of that half-size.")]
    [Tune("Periscope & fire control")] [Range(0.1f, 1f)] [SerializeField] private float lockCornerArm = 0.35f;
    [Tooltip("Owner 2026-11-01: the plot is a command table, and a contact out of sight is a marker where it was last fixed. It fades over this many game seconds.")]
    [Tune("Sensors")] [SerializeField] private float staleSeconds = 60f;
    [Tooltip("How faint an old marker gets, 0 to 1.")]
    [Tune("Sensors")] [Range(0f, 1f)] [SerializeField] private float staleAlpha = 0.35f;
    [Tooltip("From this many game seconds after its last fix, a marker's label says how old it is.")]
    [Tune("Sensors")] [SerializeField] private float staleLabelSeconds = 10f;
    [Tooltip("Owner 2026-11-02: past visual range a rock shows only as a clean hollow circle, where the ACTIVE SCAN cone passed it. It fades away over this many game seconds.")]
    [Tune("Sensors")] [SerializeField] private float rockEchoSeconds = 30f;
    [Tooltip("Owner 2026-11-02: the circle blinks. It is bright as the ping passes, and drops to the dim share over this many game seconds.")]
    [Tune("Sensors")] [SerializeField] private float rockEchoBlinkSeconds = 1f;
    [Tooltip("How bright the circle stays between blinks, 0 to 1.")]
    [Tune("Sensors")] [Range(0f, 1f)] [SerializeField] private float rockEchoDim = 0.45f;
    [Tooltip("Owner 2026-11-02: a rock's echo is a clean hollow circle. Its radius is this many times the rock's half size, so a big rock gives a bigger circle.")]
    [Tune("Sensors")] [SerializeField] private float rockEchoSize = 1f;
    [Tooltip("The smallest a rock's circle gets, px radius, however far you zoom out.")]
    [Tune("Sensors")] [SerializeField] private float rockEchoMinPx = 3f;
    [Tooltip("The largest a rock's circle gets, px radius, however far you zoom in (owner 2026-11-02: some were very large).")]
    [Tune("Sensors")] [SerializeField] private float rockEchoMaxPx = 10f;
    [Tooltip("The colour of a rock's circle.")]
    [Tune("Sensors")] [SerializeField] private Color rockEchoInk = new Color32(0x8A, 0x95, 0x9D, 0xB0);
    [Tooltip("Owner 2026-11-01: draws each ship's collision box on the plot, to see that it fits round the model.")]
    [Tune("Plot & our ship")] [SerializeField] private bool showShipBoxes = false;
    [Tooltip("Owner 2026-11-01: inside the cone, the gun itself: a solid bar that turns slowly onto the target. Shots fly straight down it.")]
    [Tune("Periscope & fire control")] [Range(0f, 1f)] [SerializeField] private float gunBarAlpha = 1f, gunBarAlphaOther = 0.4f;
    [Tooltip("Owner 2026-10-29: a thin ring round our ship at the visual range (the ship's stats file, 10,000 units), always on. "
           + "Inside it you see ships with your eyes. Its strength, 0 to 1; 0 hides it.")]
    [Tune("Sensors")] [SerializeField] private float visualRingAlpha = 0.35f;

    [Header("Fighters (Step 54): one small triangle per craft, white like our ship (owner 2026-10-26), no labels")]
    [Tooltip("A hidden triangle image (triangle-128, nose up; owner 2026-11-02: small triangles), cloned per fighter in flight.")]
    [SerializeField] private Image fighterTemplate;
    [Tooltip("A fighter's size on screen, px.")]
    [Tune("Hangar & fighters")] [SerializeField] private float fighterPx = 16f;
    [Tooltip("Gap between fighters in the V, px.")]
    [Tune("Hangar & fighters")] [SerializeField] private float fighterGapPx = 10f;
    private readonly List<Image> fighters = new List<Image>();
    /// <summary>Fighters drawn now (the playtest reads it).</summary>
    public int FightersShown { get; private set; }

    Color ownForce => Palette.ownForce;
    Color caution => Palette.caution;
    Color alarm => Palette.alarm;
    Color data => Palette.data;
    [Tooltip("sweepEdge on the amber grid: rgb(255,150,130).")]
    [SerializeField] private Color sweepInkAmber = new Color32(0xFF, 0x96, 0x82, 0xFF);

    private readonly List<TMP_Text> labels = new List<TMP_Text>();
    private readonly List<ClickSpot> hits = new List<ClickSpot>();

    bool Capturing => Controls.Tool == "POINT" || Controls.Tool == "RULER" || Controls.Tool == "COMPASS";

    // plotPt: pointer to plot-layer px at 1x, rounded
    // the board measures a whole drag at the zoom of the press (_plot.k)
    private double pressZoom = 1;
    int LayerX(float bx) => ShipState.JsRound((bx - plotCentre.x) / pressZoom);
    int LayerY(float by) => ShipState.JsRound((by - plotCentre.y) / pressZoom);

    // Step 36: marks live in world units (x east, y north) so they stay put when our ship moves.
    // The pointer is still rounded to whole layer px first, as the board's plotPt does.
    double UnitsPerPx => 1000.0 / grid.PxPer1000Units;
    double SnapUnits => rulerSnapPx * UnitsPerPx / Controls.ZoomExact;
    double WorldX(float bx) => viewX + LayerX(bx) * UnitsPerPx;
    double WorldY(float by) => viewY - LayerY(by) * UnitsPerPx;
    /// <summary>A world point (units) in board px.</summary>
    public Vector2 Screen(double wx, double wy)
    {
        double z = Controls.ZoomExact / UnitsPerPx;
        return plotCentre + new Vector2((float)((wx - viewX) * z), (float)(-(wy - viewY) * z));
    }
    /// <summary>Step 75: our ship where it is drawn now (it glides between ticks), in board px.</summary>
    Vector2 OwnAt => Screen(State.nav.DrawnX, State.nav.DrawnY);
    /// <summary>Owner 2026-11-01: a shot where it is drawn now, and the far end of its trail. It slides toward its spot at the
    /// next tick, at up to twice its speed. It doesn't slow for a hit: the dot stops there and the trail runs on into it.</summary>
    Vector2 ShotAt(WeaponSystem.Round r, out Vector2 tail, bool ours)
    {
        double dt = Controls.TickGameSeconds, step = r.speed * dt, pxPerUnit = Controls.ZoomExact / UnitsPerPx;
        Controls.Slide(ref r.drawn, r.x, r.y, r.x + r.dx * step, r.y + r.dy * step, r.speed * 2);
        double along = (r.drawn.x - r.x) * r.dx + (r.drawn.y - r.y) * r.dy, past = System.Math.Max(0, along - State.weapons.StepAhead(r, dt));
        Vector2 at = Screen(r.drawn.x - r.dx * past, r.drawn.y - r.dy * past);
        double trail = System.Math.Min(slugTrailPx * r.speed / State.weapons.stats.slugSpeedUnits, (r.travelled + along) * pxPerUnit) - past * pxPerUnit;
        tail = at + new Vector2((float)-r.dx, (float)r.dy) * (float)System.Math.Max(0, trail); // board y is down
        if (closeUpCamera && closeUpCamera.enabled) // owner 2026-11-08: the same shot as a glowing streak in the close-up
        {
            double hx = r.drawn.x - r.dx * past, hy = r.drawn.y - r.dy * past;
            double back = System.Math.Max(0, System.Math.Min(slugTrailPx * r.speed / State.weapons.stats.slugSpeedUnits * UnitsPerPx, r.travelled + along) - past); // its x1 length in units
            shotsNow.Add(new Shot { r = r, ours = ours, head = new Vector3((float)hx, (float)hy, EffectZ), tail = new Vector3((float)(hx - r.dx * back), (float)(hy - r.dy * back), EffectZ) });
        }
        return at;
    }
    // The targeting computer's white corners round a point on the plot.
    void AddCorners(List<MarksGraphic.Shape> shapes, Vector2 c, float half)
    {
        float arm = half * lockCornerArm;
        foreach (int sx in new[] { -1, 1 })
            foreach (int sy in new[] { -1, 1 })
            {
                Vector2 corner = c + new Vector2(sx, sy) * half;
                shapes.Add(new MarksGraphic.Shape { kind = "line", a = corner, b = corner - new Vector2(sx * arm, 0f), alpha = 1f, ink = data.gamma });
                shapes.Add(new MarksGraphic.Shape { kind = "line", a = corner, b = corner - new Vector2(0f, sy * arm), alpha = 1f, ink = data.gamma });
            }
    }

    // A ship's collision box as drawn now.
    void AddBox(List<MarksGraphic.Shape> shapes, NavSystem nav)
    {
        float rad = (float)nav.DrawnHeading * Mathf.Deg2Rad;
        Vector2 half = nav.BoxHalf, fore = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * half.y, right = new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad)) * half.x;
        var at = new Vector2((float)nav.DrawnX, (float)nav.DrawnY);
        var corners = new[] { at + fore + right, at + fore - right, at - fore - right, at - fore + right };
        for (int i = 0; i < 4; i++)
            shapes.Add(new MarksGraphic.Shape { kind = "line", a = Screen(corners[i].x, corners[i].y), b = Screen(corners[(i + 1) % 4].x, corners[(i + 1) % 4].y), alpha = 1f, ink = data.gamma });
    }

    /// <summary>A mount where it is drawn: on its hard point, or at our ship's centre without one.</summary>
    Vector2 MountAt(WeaponSystem.Mount m)
    {
        Vector2 off = State.weapons.HardPointOffset(m.id, State.nav.DrawnHeading);
        return Screen(State.nav.DrawnX + off.x, State.nav.DrawnY + off.y);
    }

    // Step 120: the plot starts empty. Every layer from the axis numbers to the marks waits for the grid.
    private void Start()
    {
        if (!grid) grid = FindAnyObjectByType<PlotGrid>();
        grid.Formed = 0f;
        fading = new List<CanvasGroup>();
        for (int i = axisLayer.GetSiblingIndex(); i <= hitLayer.GetSiblingIndex(); i++)
        {
            var layer = axisLayer.parent.GetChild(i).gameObject.AddComponent<CanvasGroup>();
            layer.alpha = 0f;
            fading.Add(layer);
        }
        var below = new List<Graphic> { closeUpImage };
        if (nebula != null) foreach (var n in nebula) below.Add(n.image);
        foreach (var g in below) if (g) { var layer = g.gameObject.AddComponent<CanvasGroup>(); layer.alpha = 0f; fading.Add(layer); }
    }

    void FormGrid()
    {
        formClock += Mathf.Min(Time.unscaledDeltaTime, FormLongestStep);
        grid.Formed = Mathf.Clamp01(formClock / Mathf.Max(gridFormSeconds, 0.01f));
        float alpha = Mathf.Clamp01((formClock - gridFormSeconds) / Mathf.Max(plotFadeSeconds, 0.01f));
        foreach (var layer in fading) layer.alpha = alpha;
        if (alpha < 1f) return;
        foreach (var layer in fading) Destroy(layer);
        fading = null;
    }

    private void Update()
    {
        if (Controls == null) return;
        if (fading != null) FormGrid();
        if (sweep && State.sensors.Listening && !Controls.Paused)
        {
            float turn = Mathf.Max(0.5f, sweepSecondsBase - State.sensors.senBoost * sweepSecondsPerGain);
            // in game time (owner 2026-10-24): `turn` seconds a sweep at x1, ten times as fast at x10
            float before = sweepDeg, step = 360f * Time.unscaledDeltaTime * Controls.Rate / turn;
            sweepDeg = (sweepDeg + step) % 360f;
            // The sweep fixes the bearing of each heard contact it passes. Step 63 (owner 2026-10-24):
            // with ACTIVE on, a contact the cone covers is identified as the sweep passes it (the design needed
            // the cone dial let go, or a contact-board row clicked, and said nothing).
            // Step 69 (owner 2026-10-25): every pass over a ship the cone covers tags it with a beep, scanned or not.
            foreach (var t in State.tracks)
            {
                if (t.own || t.wreck) continue;
                float past = Mathf.Repeat((float)State.sensors.ExactBearing(t) - before, 360f);
                if (past <= 0f || past > step) continue;
                bool scanned = State.sensors.identified.Contains(t.key);
                if (State.sensors.Heard(t) || State.sensors.ConeCovers(t)) State.sensors.See(t); // Step 75: out of sight, the plot shows it here until the next pass
                if (State.sensors.ConeCovers(t)) { Controls.SweepTag(); State.sensors.NoteScan(t); if (!scanned) Refresh(); continue; }
                if (!scanned && State.sensors.Heard(t)) Refresh();
            }
            if (!world) world = FindAnyObjectByType<TacticalWorld>();
            if (world) world.SweepRocks(before, step); // owner 2026-11-02: rocks in the ACTIVE SCAN cone echo
            sweep.rectTransform.localEulerAngles = new Vector3(0f, 0f, -sweepDeg); // clockwise on screen
        }
        // Step 75: ships glide between ticks, so while time runs the plot redraws every frame
        // ponytail: redraws the whole plot, as panning already does; redraw only what moves if the profiler minds
        if (following) { viewX = State.nav.DrawnX; viewY = State.nav.DrawnY; } // where our ship is drawn, so it holds still at the centre
        if (!Controls.Paused) Refresh();
        // not while typing in a field (the hangar's launch bearing)
        var typing = UnityEngine.EventSystems.EventSystem.current ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject : null;
        if (typing && typing.GetComponent<TMP_InputField>()) return;
        // Step 65 (owner 2026-10-25): C centres the plot on our ship again after panning away
        if (Input.GetKeyDown(centreOnShipKey)) { CentreOnOurShip(); return; }
        float x = (Input.GetKey(panRightKey) ? 1f : 0f) - (Input.GetKey(panLeftKey) ? 1f : 0f);
        float y = (Input.GetKey(panUpKey) ? 1f : 0f) - (Input.GetKey(panDownKey) ? 1f : 0f);
        if (x == 0f && y == 0f) return;
        following = false;
        double km = panKeysPxPerS * Time.unscaledDeltaTime * UnitsPerPx / Controls.ZoomExact;
        viewX += x * km;
        viewY += y * km;
        Refresh();
    }

    public override bool Click(string action, string arg)
    {
        switch (action)
        {
            case "plot.erase":
                if (Controls.Tool == "ERASE") State.EraseMark(int.Parse(arg));
                return true;
        }
        return false;
    }

    public override bool Down(string action, string arg, Vector2 board)
    {
        if (action == "plot.pan") { panning = true; panPress = board; panX0 = viewX; panY0 = viewY; return true; }
        if (action != "plot" || !Capturing) return false;
        pressZoom = Controls.ZoomExact;
        double wx = WorldX(board.x), wy = WorldY(board.y);
        var snap = Controls.Tool == "RULER" ? State.PointNear(wx, wy, SnapUnits) : null; // Step 46
        if (snap != null) { wx = snap.x; wy = snap.y; }
        State.MarkDown(Controls.Tool, wx, wy, Controls.MissionSeconds);
        if (snap != null && State.draft != null) State.draft.ptA = snap.id;
        return true;
    }

    public override bool Drag(string action, string arg, Vector2 board)
    {
        if (action == "plot.pan" && panning)
        {
            // the world follows the pointer: drag right and the view centre moves west
            if (board != panPress) following = false; // a click on the grid that doesn't move it keeps following
            double k = UnitsPerPx / Controls.ZoomExact;
            viewX = panX0 - (board.x - panPress.x) * k;
            viewY = panY0 + (board.y - panPress.y) * k;
            return true;
        }
        if (action != "plot" || State.draft == null) return false;
        double wx = WorldX(board.x), wy = WorldY(board.y);
        var d = State.draft;
        var snap = d.kind == "ruler" ? State.PointNear(wx, wy, SnapUnits) : null; // Step 46
        if (snap != null && snap.id != d.ptA) { wx = snap.x; wy = snap.y; d.ptB = snap.id; }
        else d.ptB = 0;
        State.MarkMove(wx, wy);
        return true;
    }

    public override bool Up(string action, string arg, Vector2 board)
    {
        if (action == "plot.pan") { panning = false; return true; }
        if (action != "plot" || State.draft == null) return false;
        State.MarkUp(State.markMinLen * UnitsPerPx);
        return true;
    }

    public override void Refresh()
    {
        var s = State;
        if (!viewSet) { viewX = s.nav.x; viewY = s.nav.y; viewSet = true; }
        if (!grid) grid = FindAnyObjectByType<PlotGrid>();
        if (grid)
        {
            // Step 65: the view stays inside the battle space, so the plot never shows black past its edge,
            // unless our ship has left it: then the view may follow the ship out
            Rect r = grid.rectTransform.rect;
            double k = UnitsPerPx / Controls.ZoomExact;
            double lx = System.Math.Max(0, s.ArenaHalfW - r.width / 2 * k), ly = System.Math.Max(0, s.ArenaHalfH - r.height / 2 * k);
            if (s.OutsideArena) { lx = System.Math.Max(lx, System.Math.Abs(s.nav.x)); ly = System.Math.Max(ly, System.Math.Abs(s.nav.y)); }
            viewX = System.Math.Max(-lx, System.Math.Min(lx, viewX));
            viewY = System.Math.Max(-ly, System.Math.Min(ly, viewY));
            // the lines move with the world: the view's offset from the world origin, in board px
            double z = Controls.ZoomExact / UnitsPerPx;
            grid.OffsetPx = new Vector2((float)(-viewX * z), (float)(viewY * z));
            grid.ArenaHalfUnits = new Vector2((float)s.ArenaHalfW, (float)s.ArenaHalfH); // Step 65: the grid is the battle space
            ShowAxes();
            ShowRuins(r.size, z);
            ShowCloseUp(r.size, z);
            ShowClouds(z);
        }
        if (capture) capture.raycastTarget = Capturing;
        bool erasing = Controls.Tool == "ERASE";

        var all = new List<ShipState.Mark>(s.marks);
        if (s.draft != null) all.Add(s.draft);
        var shapes = new List<MarksGraphic.Shape>();
        int graded = 0; // compass numbers used this refresh
        for (int i = 0; i < all.Count; i++)
        {
            var m = all[i];
            bool isDraft = m == s.draft;
            float alpha = isDraft ? draftAlpha : 1f;
            Vector2 a = Screen(s.MarkX(m), s.MarkY(m)), b = Screen(m.x2, m.y2);
            shapes.Add(new MarksGraphic.Shape { kind = m.kind, a = a, b = b, alpha = alpha });

            var label = Label(i);
            Paint(label, m.kind == "ruler" ? data : ownForce); // owner 2026-11-01: the distance is the point of a ruler: white, to stand off the blue line
            label.alpha = alpha;
            Vector2 at;
            if (m.kind == "point") { label.text = Clock(m.t); label.alignment = TextAlignmentOptions.Left; at = a + pointLabelOffset; }
            else
            {
                double dx = m.x2 - s.MarkX(m), dy = m.y2 - s.MarkY(m); // units, y north
                string km = ShipState.JsRound(s.MarkLen(m)).ToString("#,0", System.Globalization.CultureInfo.InvariantCulture) + " km";
                label.text = m.kind == "ruler" ? km : km + " · BRG " + Brg(dx, dy) + "°"; // owner 2026-10-27: the ruler shows the distance only
                // owner 2026-11-08: with a fixed gun picked (the spinal railgun), the ruler also shows how long its slug takes to fly that far
                if (m.kind == "ruler" && s.weapons.FixedGun) label.text += " · SLUG " + Duration(s.MarkLen(m) / s.weapons.stats.slugSpeedUnits);
                if (m.kind == "ring") Grade(shapes, a, (b - a).magnitude, alpha, ref graded);
                if (m.kind == "ruler")
                {
                    label.alignment = TextAlignmentOptions.Center;
                    ShipState.Mark fromPt, toPt;
                    double crs, spd;
                    if (s.TrackLeg(m, out fromPt, out toPt, out crs, out spd))
                    {
                        // a target's track: its course and speed, and where it will be projectSeconds on
                        label.text = "CRS " + (ShipState.JsRound(crs) % 360).ToString("000") + "° · "
                                   + spd.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km/s";
                        double ahead = spd * projectSeconds, rc = crs * System.Math.PI / 180.0;
                        shapes.Add(new MarksGraphic.Shape { kind = "dash", a = Screen(toPt.x, toPt.y),
                            b = Screen(toPt.x + System.Math.Sin(rc) * ahead, toPt.y + System.Math.Cos(rc) * ahead), alpha = alpha });
                    }
                    // the board rounds the label's spot to whole layer px from the plot centre. units→px
                    // leaves float dust (250.49999999999997 for 250.5), so trim it before rounding
                    double mx = ShipState.JsRound(System.Math.Round((m.x + dx / 2.0 - viewX) / UnitsPerPx, 6));
                    double my = ShipState.JsRound(System.Math.Round(-(m.y + dy / 2.0 - viewY) / UnitsPerPx, 6));
                    at = plotCentre + new Vector2((float)mx, (float)my) * (float)Controls.ZoomExact + rulerLabelOffset;
                }
                else { label.alignment = TextAlignmentOptions.Left; at = b + ringLabelOffset; }
            }
            Place(label.rectTransform, at, label.alignment == TextAlignmentOptions.Left ? 0f : 0.5f);

            var hit = Hit(i);
            hit.gameObject.SetActive(erasing && !isDraft); // marks take the pointer only in ERASE
            hit.arg = m.id.ToString();
            var hr = (RectTransform)hit.transform;
            if (m.kind == "point") { hr.sizeDelta = new Vector2(pointBox, pointBox); Put(hr, a, 0f); }
            else if (m.kind == "ruler") { hr.sizeDelta = new Vector2((b - a).magnitude, rulerBox); Put(hr, (a + b) / 2f, -Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg); }
            else { float r = (b - a).magnitude; hr.sizeDelta = new Vector2(2 * r, 2 * r); Put(hr, a, 0f); }
        }
        for (int i = all.Count; i < labels.Count; i++) labels[i].gameObject.SetActive(false);
        for (int i = graded; i < gradeLabels.Count; i++) gradeLabels[i].gameObject.SetActive(false);
        ShowJumpPoint(shapes);
        for (int i = all.Count; i < hits.Count; i++) hits[i].gameObject.SetActive(false);
        // our slugs and torpedoes in flight (Step 48): a dot with a trail fading behind it, no label;
        // white like our ship, not the marks' blue (owner 2026-10-26)
        var slugInk = ownShipColor.gamma;
        foreach (var r in s.weapons.Rounds)
        {
            Vector2 tail, at = ShotAt(r, out tail, true);
            float seen = TableDot(r);
            shapes.Add(new MarksGraphic.Shape { kind = "point", a = at, b = at, alpha = seen, ink = slugInk });
            shapes.Add(new MarksGraphic.Shape { kind = "fade", a = at, b = tail, alpha = 0.8f * seen, ink = slugInk });
            AddLightRing(shapes, r);
        }
        // a prize our marines hold (Step 60): an own-force dot where it lies, no label (placeholder until its glyph)
        foreach (var t in s.tracks)
            if (t.captured && t.hasWorld) { var at = Screen(t.worldX, t.worldY); shapes.Add(new MarksGraphic.Shape { kind = "point", a = at, b = at, alpha = 1f }); }
        if (marks) marks.Set(shapes);

        // our ship where it is (Step 41: it moves across the plot): the 3D model's top-down view, sized by zoom and clamped
        if (ownShipImage)
        {
            if (!shipView) shipView = ShipView.Own();
            ownShipImage.texture = shipView ? shipView.Texture : null;
            ownShipImage.enabled = shipView && shipView.Texture;
            var shipInk = ownShipColor.gamma; // the board canvas linearises vertex colours; pre-encode
            shipInk.a *= 1f - CloseUp;
            ownShipImage.color = shipInk;
            float side = ShipPx(shipView) / (shipView ? shipView.ShipFill : 1f);
            ownShipImage.rectTransform.sizeDelta = new Vector2(side, side);
            Put(ownShipImage.rectTransform, OwnAt, 0f);
        }

        // fighters in flight (Step 54): a V behind the leader, each wedge pointing where the flight flies
        int nf = 0;
        // owner 2026-11-02: how far a fighter's centre is from our ship's, a whole fighter clear of the side as drawn
        float sidePx = ShipPx(shipView) / 2f * (shipView ? shipView.WidthShare : 0.5f) + fighterPx / 2f;
        if (fighterTemplate)
        {
            var ink = ownShipColor.gamma; // owner 2026-10-26: white, like our ship
            foreach (var f in s.hangar.Flights)
            {
                Controls.Slide(ref f.drawn, f.x, f.y, f.x, f.y); // owner 2026-11-01: it slides after the flight, up to one tick behind
                double drawnX, drawnY;
                float drawnHdg = FlightAt(f, out drawnX, out drawnY);
                double h = drawnHdg * System.Math.PI / 180.0;
                Vector2 ahead = new Vector2((float)System.Math.Sin(h), (float)-System.Math.Cos(h)), right = new Vector2(-ahead.y, ahead.x); // board y is down
                Vector2 lead = Screen(drawnX, drawnY), off = lead - OwnAt;
                float clear = InnerRingPx + f.craft / 2 * fighterGapPx + fighterPx / 2f;
                // owner 2026-10-26: an escort holds outside the small circle, whole V clear, even after zooming out grows it
                if (f.order == "escort" && !f.returning && f.runLeft <= 0 && off.magnitude < clear) lead = OwnAt + (off.sqrMagnitude > 0f ? off.normalized : ahead) * clear;
                // owner 2026-11-02: the V opens as it leaves the ship and closes as it lands; shut where they come out, none on the hull
                float open = Mathf.Clamp01(((lead - OwnAt).magnitude - sidePx) / (clear - sidePx));
                for (int c = 0; c < f.craft; c++)
                {
                    int row = (c + 1) / 2;
                    var img = Fighter(nf++);
                    img.color = ink;
                    img.rectTransform.sizeDelta = new Vector2(fighterPx, fighterPx);
                    Put(img.rectTransform, lead + (right * (c % 2 == 1 ? -row : row) - ahead * row) * fighterGapPx * open, -drawnHdg);
                }
            }
            for (int i = nf; i < fighters.Count; i++) fighters[i].gameObject.SetActive(false);
        }
        FightersShown = nf;
        // an escort flies out until its whole V (rows behind the leader, half a craft) is outside the small circle round
        // our ship, and a squadron lands when its leader's nose reaches the circle
        double kmPx = UnitsPerPx / Controls.ZoomExact;
        s.hangar.landClearUnits = (InnerRingPx + fighterPx / 2f) * kmPx;
        s.hangar.launchClearUnits = s.hangar.landClearUnits + s.hangar.SquadReady(s.hangar.CurrentSquad) / 2 * fighterGapPx * kmPx;
        s.hangar.launchSideUnits = sidePx * kmPx;

        // the selected weapon's reach while Fire Control is open (Step 46)
        if (weaponRing)
        {
            var reachShapes = new List<MarksGraphic.Shape>();
            var mount = s.weapons.MountOf(s.weapons.mount);
            bool fcOpen = Controls.Motion && Controls.Motion.OpenDrawerName == "fire control";
            if (fcOpen && mount != null && mount.rangeUnits > 0f)
            {
                Vector2 c = OwnAt;
                reachShapes.Add(new MarksGraphic.Shape { kind = "dashring", a = c, b = c + new Vector2((float)(mount.rangeUnits / UnitsPerPx * Controls.ZoomExact), 0f), alpha = 0.7f });
            }
            // a mount's arc: a dashed sector off our hull, out to its reach (Step 55's PDC arc)
            System.Action<WeaponSystem.Mount, float, string> sector = (pm, alpha, line) =>
            {
                Vector2 c = MountAt(pm);
                float r = (float)(pm.rangeUnits / UnitsPerPx * Controls.ZoomExact);
                System.Func<float, Vector2> at = deg => c + new Vector2(Mathf.Sin(deg * Mathf.Deg2Rad), -Mathf.Cos(deg * Mathf.Deg2Rad)) * r; // board y is down
                float hdg = (float)s.nav.DrawnHeading, from = hdg + pm.arc - pm.halfArc, to = hdg + pm.arc + pm.halfArc;
                reachShapes.Add(new MarksGraphic.Shape { kind = line, a = c, b = at(from), alpha = alpha });
                reachShapes.Add(new MarksGraphic.Shape { kind = line, a = c, b = at(to), alpha = alpha });
                for (float d = from; d < to; d += pdcArcStepDeg)
                    reachShapes.Add(new MarksGraphic.Shape { kind = line, a = at(d), b = at(Mathf.Min(to, d + pdcArcStepDeg)), alpha = alpha });
            };
            // owner 2026-11-01: while Fire Control is open each railgun shows its soft dotted firing arc from its
            // hard point, and the gun itself, a solid bar; the selected turret is the stronger
            if (fcOpen)
                foreach (var id in s.weapons.MountIds("RG"))
                {
                    var rm = s.weapons.MountOf(id);
                    if (rm == null) continue;
                    bool selected = id == s.weapons.mount;
                    if (rm.halfArc > 0) sector(rm, selected ? railArcAlpha : railArcAlphaOther, "dots"); // owner 2026-11-08: a fixed gun (the spinal railgun) has no cone, only its line
                    float aim = Mathf.Lerp(rm.aimWas, rm.aimDeg, Controls.Paused ? 1f : Controls.SinceTick / Controls.TickGameSeconds); // it glides between ticks
                    float deg = ((float)s.nav.DrawnHeading + rm.arc + aim) * Mathf.Deg2Rad, barPx = (float)(rm.rangeUnits / UnitsPerPx * Controls.ZoomExact);
                    Vector2 gun = MountAt(rm);
                    reachShapes.Add(new MarksGraphic.Shape { kind = "line", a = gun, b = gun + new Vector2(Mathf.Sin(deg), -Mathf.Cos(deg)) * barPx, alpha = selected ? gunBarAlpha : gunBarAlphaOther });
                }
            // Step 55: each PDC group on AUTO shows its arc (the design's PDC arc, accent.caution at .45)
            var pdGroups = s.weapons.MountIds("PD");
            for (int g = 0; g < pdGroups.Length && g < s.weapons.pdcAuto.Length; g++)
            {
                var pm = s.weapons.MountOf(pdGroups[g]);
                if (s.weapons.pdcAuto[g] && pm != null) sector(pm, 0.45f, "dash");
            }
            if (visualRingAlpha > 0f)
            {
                Vector2 own = OwnAt;
                reachShapes.Add(new MarksGraphic.Shape { kind = "circle", a = own, b = own + new Vector2((float)(s.sensors.stats.visualRangeUnits / UnitsPerPx * Controls.ZoomExact), 0f),
                                                         alpha = visualRingAlpha, ink = data.gamma });
            }
            if (showShipBoxes)
            {
                AddBox(reachShapes, s.nav);
                foreach (var t in s.tracks) if (t.nav) AddBox(reachShapes, t.nav);
            }
            Paint(weaponRing, caution);
            weaponRing.Set(reachShapes);
        }

        if (sensorRing)
        {
            if (setSensorUnits == null) setSensorUnits = km => { sensorUnits = km; DrawSensorRing(); };
            sensorRingMotion.Set(s.sensors.PassiveRangeUnits, setSensorUnits);
            // the cone reaches the scan's range, which in UV is past the hearing circle (ConeCovers)
            if (coneMotion == null) { coneMotion = new Eased(sensorRingMotion.seconds, sensorRingMotion.ease); setConeUnits = km => { coneUnits = km; DrawSensorRing(); }; }
            coneMotion.Set(s.sensors.SenRangeUnits, setConeUnits);
            DrawSensorRing();
        }

        // Step 75 (owner 2026-10-26): in sight (visualRangeUnits) a ship shows as its own model from above, gliding: red
        // once identified; amber and nameless if not (you see its shape, not who it is, and can't target it); grey
        // once a wreck, drifting on. Out of sight an identified hostile is a dot where the sweep last touched it
        // (res_* in the board: a mark only once identified). units to plot px at 1x: 60.9 px per 1000 units, then the zoom.
        var dots = new List<MarksGraphic.Shape>();
        // owner 2026-11-02: past visual range a rock is only its echo, a clean hollow circle where the ACTIVE SCAN cone passed it, that blinks
        // as the ping passes and fades; in sight the rock itself shows. A rock across the edge shows both, cut at the circle (ClipToSight).
        ClipToSight();
        if (world && rockEchoMarks)
        {
            var echoes = new List<MarksGraphic.Shape>();
            foreach (var e in world.RockEchoes)
            {
                float age = Controls.MissionSeconds - e.Value.bornAt, left = 1f - age / Mathf.Max(1f, rockEchoSeconds);
                if (left <= 0f || !e.Key) continue;
                float blink = Mathf.Lerp(rockEchoDim, 1f, 1f - age / Mathf.Max(0.01f, rockEchoBlinkSeconds));
                var at = Screen(e.Value.at.x, e.Value.at.y);
                float r = Mathf.Clamp(e.Value.half * rockEchoSize / (float)UnitsPerPx * (float)Controls.ZoomExact, rockEchoMinPx, rockEchoMaxPx);
                echoes.Add(new MarksGraphic.Shape { kind = "circle", a = at, b = at + new Vector2(r, 0f), alpha = left * blink, ink = rockEchoInk.gamma });
            }
            rockEchoMarks.Set(echoes);
        }
        int n = 0;
        string pictured = null; // the one contact picture's track
        float closeUp = CloseUp;
        notInCloseUp.Clear();
        foreach (var t in s.tracks) if (!t.own && t.nav && (t.wreck || !s.sensors.InSight(t))) notInCloseUp.Add(t.nav);
        wreckPictured = false;
        foreach (var t in s.tracks)
        {
            if (t.own || t.captured) continue;
            bool known = (t.hostile || t.friendly) && s.sensors.identified.Contains(t.key), inSight = s.sensors.InSight(t);
            bool locked = s.weapons.LockedOn(t.key); // owner 2026-11-01: a locked ship is tracked on the grid, where it is now
            if (!known && !inSight && !locked) continue;
            // owner 2026-11-01: a scanned ship under the ACTIVE SCAN cone moves smoothly too. ACTIVE off, it hops again from where it stopped
            bool scanning = known && t.hasWorld && !t.wreck && s.sensors.Listening && s.sensors.ConeCovers(t);
            if (scanning)
            {
                s.sensors.See(t);
                if (t.nav) { t.seenX = t.nav.DrawnX; t.seenY = t.nav.DrawnY; }
            }
            bool current = inSight || locked || scanning;
            Vector2 at;
            if (current) at = t.nav ? Screen(t.nav.DrawnX, t.nav.DrawnY) : Screen(t.worldX, t.worldY);
            else if (t.hasWorld)
            {
                if (!t.seen) s.sensors.See(t); // identified another way: its first fix is now
                at = Screen(t.seenX, t.seenY);
            }
            else
            {
                double b = t.bearing * System.Math.PI / 180.0, px = t.rangeUnits / UnitsPerPx * Controls.ZoomExact;
                at = OwnAt + new Vector2((float)(System.Math.Sin(b) * px), (float)(-System.Math.Cos(b) * px));
            }
            Color ink = t.wreck ? wreckInk : known && t.friendly ? Palette.ownForce : known || locked ? alarm : caution; // owner 2026-11-09: our battle group is blue once scanned
            float age = current || !t.hasWorld ? 0f : Controls.MissionSeconds - t.seenAt;
            float fresh = Mathf.Lerp(1f, staleAlpha, age / Mathf.Max(1f, staleSeconds));
            var view = inSight ? ViewOf(t.key) : null;
            float labelPush = 0f; // a pictured contact's label sits clear of its picture
            if (contactShipImage && pictured == null && view && view.Texture)
            {
                // its model from above, sized as ours is; north up like ours (the model turns)
                contactShipImage.texture = view.Texture;
                var picInk = ink.gamma;
                if (!t.wreck || (wrecks.TryGetValue(t, out var cutUp) && cutUp != null)) picInk.a *= 1f - closeUp; // the close-up shows the ship itself (a wreck, in two); a wreck it can't cut keeps its line picture
                contactShipImage.color = picInk;
                float px = ShipPx(view) / view.ShipFill;
                if (t.wreck) ShowBroken(t, view, at, px, ink);
                else
                {
                    contactShipImage.uvRect = new Rect(0f, 0f, 1f, 1f);
                    contactShipImage.rectTransform.sizeDelta = new Vector2(px, px);
                    Put(contactShipImage.rectTransform, at, 0f);
                }
                contactShipImage.enabled = true;
                pictured = t.key;
                labelPush = px * view.ShipFill / 2f;
            }
            else if (t.wreck) continue; // no picture: a wreck keeps its dim dot (below); an unknown in sight is an amber dot (Step 99)
            else dots.Add(new MarksGraphic.Shape { kind = "point", a = at, b = at, alpha = fresh * (inSight ? 1f - closeUp : 1f), ink = known && t.friendly ? ink.gamma : default });
            if (t.wreck) continue; // a wreck carries no label (Step 49)
            if (locked)
            {
                float half = Mathf.Max(lockCornersPx, labelPush, (t.nav ? t.nav.BoxHalf.y : 0f) / (float)UnitsPerPx * (float)Controls.ZoomExact);
                AddCorners(dots, at, half);
                labelPush = half;
            }
            var cl = ContactLabel(n++);
            Paint(cl, ink);
            cl.alpha = fresh;
            cl.raycastTarget = false; // owner 2026-10-29: clicking a contact does nothing; the periscope picks the target
            float rng = current ? t.rangeUnits : t.seenRangeUnits;
            cl.text = (known ? t.name : "BOGEY " + t.key.ToUpperInvariant()) + (locked ? " · LOCKED" : "") + "\nRNG " + ShipState.JsRound(rng).ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " km" + (age >= staleLabelSeconds ? " · " + Duration(age) + " AGO" : "");
            Place(cl.rectTransform, at + contactLabelOffset + new Vector2(labelPush, 0f), 0f);
        }
        for (int i = n; i < contactLabels.Count; i++) contactLabels[i].gameObject.SetActive(false);
        if (contactShipImage && pictured == null) contactShipImage.enabled = false;
        if (wreckBackHalf) wreckBackHalf.enabled = wreckPictured;
        if (world) // owner 2026-11-02: past visual range DRADIS shows a gas cloud as a hollow circle, about the cloud's size
            foreach (var c in world.Clouds)
                if (c && c.seen && !c.inSight)
                {
                    var cloudAt = Screen(c.At.x, c.At.y);
                    float r = Mathf.Max(6f, (c.widthUnits + c.heightUnits) / 4 * cloudRingSize / (float)UnitsPerPx * (float)Controls.ZoomExact);
                    dots.Add(new MarksGraphic.Shape { kind = "circle", a = cloudAt, b = cloudAt + new Vector2(r, 0f), alpha = cloudRingAlpha, ink = ruinsColor.gamma });
                }
        // enemy torpedoes coming at us (Step 55): a red dot with a trail, like our own rounds, no label
        foreach (var r in s.weapons.Incoming)
        {
            Vector2 tail, at = ShotAt(r, out tail, false);
            float seen = TableDot(r);
            dots.Add(new MarksGraphic.Shape { kind = "point", a = at, b = at, alpha = seen });
            dots.Add(new MarksGraphic.Shape { kind = "fade", a = at, b = tail, alpha = 0.8f * seen });
            AddLightRing(dots, r);
        }
        if (contactMarks) contactMarks.Set(dots);
        ShowShotsAndBlasts();

        // passive bearings (owner 2026-11-02): a red bar curved along the DRADIS circle toward each contact DRADIS hears but has
        // not identified (bearing only, no range), with its bearing just outside; brighter, its label amber, while the scope is
        // on it (passives). It turns smoothly as either ship moves. The circle is often off the plot at normal zoom; the bar goes with it.
        bool down = Controls.Motion && Controls.Motion.ScopeDown;
        var bearings = new List<MarksGraphic.Shape>();
        var bars = new List<MarksGraphic.Shape>();
        if (!barMarks && passiveLines) { barMarks = CopyLayer(passiveLines, "Contact bars (drawn)"); barMarks.art = passiveBarArt; barMarks.SetMaterialDirty(); }
        int np = 0;
        Vector2 ownAt = OwnAt;
        float ring = (float)(sensorUnits / UnitsPerPx * Controls.ZoomExact); // where the DRADIS circle is drawn now
        foreach (var t in s.tracks)
        {
            if (ring <= 0f || t.own || !s.sensors.Heard(t) || s.sensors.identified.Contains(t.key) || !t.seen) continue; // Step 75: from the sweep's first touch
            double brg = t.nav ? (System.Math.Atan2(t.nav.DrawnX - s.nav.DrawnX, t.nav.DrawnY - s.nav.DrawnY) * 180 / System.Math.PI + 360) % 360 : s.sensors.ExactBearing(t);
            double rad = brg * System.Math.PI / 180.0; // where it is drawn now; the label shows it in whole degrees
            var dir = new Vector2((float)System.Math.Sin(rad), (float)-System.Math.Cos(rad)); // board y is down
            bool on = s.ScopeOn(t, down);
            bars.Add(new MarksGraphic.Shape { kind = "sector", a = ownAt, b = ownAt + dir * (ring + passiveBarWidthPx / 2f), inner = ring - passiveBarWidthPx / 2f,
                                              half = passiveBarDeg / 2f, alpha = on ? 1f : 0.6f, ink = alarm.gamma });
            var pl = PassiveLabel(np++);
            pl.text = "BRG " + (ShipState.JsRound(brg) % 360).ToString("000");
            pl.alpha = on ? 1f : 0.6f;
            Paint(pl, on ? caution : Controls.GridAmber ? alarm : data);
            Place(pl.rectTransform, ownAt + dir * (ring + passiveBarWidthPx + 6f), dir.x < 0f ? 1f : 0f);
            pl.raycastTarget = false; // owner 2026-10-29: clicking a bearing does nothing
        }
        for (int i = np; i < passiveLabels.Count; i++) passiveLabels[i].gameObject.SetActive(false);
        // wrecks (Step 49): a dim dot where each destroyed ship drifts, no label; in sight it shows grey as its model (Step 75)
        foreach (var t in s.tracks)
            if (t.wreck && t.hasWorld && t.key != pictured)
            {
                float left = 1f - (s.missionNow - t.wreckSeenAt) / Mathf.Max(wreckDotFadeSeconds, 0.01f); // owner 2026-11-01: out of sight, the dot fades away
                if (left <= 0f) continue;
                var w = t.nav ? Screen(t.nav.DrawnX, t.nav.DrawnY) : Screen(t.worldX, t.worldY);
                bearings.Add(new MarksGraphic.Shape { kind = "point", a = w, b = w, alpha = 0.45f * left });
            }
        if (passiveLines) { Paint(passiveLines, Controls.GridAmber ? alarm : data); passiveLines.Set(bearings); }
        if (barMarks) barMarks.Set(bars);

        // the export's own passive labels (none since Step 44; kept for a configured one)
        foreach (var p in passives)
        {
            var t = s.TrackOf(p.key);
            if (!p.label) continue;
            if (t == null) { p.label.gameObject.SetActive(false); continue; } // its contact is gone (Step 34)
            if (passiveVisibility) p.label.gameObject.SetActive(s.sensors.Heard(t) && !s.sensors.identified.Contains(t.key));
            Paint(p.label, s.ScopeOn(t, down) ? caution : Controls.GridAmber ? alarm : data);
        }
    }

    // The hearing range: Heard() takes a contact inside it while DRADIS is listening (PassiveRangeUnits).
    void DrawSensorRing()
    {
        var s = State;
        var shapes = new List<MarksGraphic.Shape>();
        if (s.sensors.Listening && sensorUnits > 0f)
        {
            bool amber = Controls.GridAmber;
            Paint(sensorRing, amber ? alarm : data);
            Vector2 c = OwnAt, e = c + new Vector2((float)(sensorUnits / UnitsPerPx * Controls.ZoomExact), 0f);
            shapes.Add(new MarksGraphic.Shape { kind = "disc", a = c, b = e, alpha = amber ? senGlowAlphaAmber : senGlowAlpha });
            shapes.Add(new MarksGraphic.Shape { kind = "circle", a = c, b = e, alpha = amber ? senEdgeAlphaAmber : senEdgeAlpha });
            // owner 2026-10-24: a small circle just outside our ship; the sweep runs from it to the rim
            shapes.Add(new MarksGraphic.Shape { kind = "circle", a = c, b = c + new Vector2(InnerRingPx, 0f), alpha = amber ? senEdgeAlphaAmber : senEdgeAlpha });
            // owner 2026-10-26: the active scan cone starts at that small circle too
            if (s.sensors.active && coneUnits > 0f)
            {
                float r = (float)(coneUnits / UnitsPerPx * Controls.ZoomExact), inner = InnerRingPx;
                System.Func<float, Vector2> at = deg => new Vector2(Mathf.Sin(deg * Mathf.Deg2Rad), -Mathf.Cos(deg * Mathf.Deg2Rad)); // board y is down
                shapes.Add(new MarksGraphic.Shape { kind = "sector", a = c, b = c + at(s.sensors.coneBrg) * r, inner = inner, half = s.sensors.coneArc, alpha = coneFillAlpha });
                foreach (int side in new[] { -1, 1 })
                {
                    var edge = at(s.sensors.coneBrg + side * s.sensors.coneArc);
                    shapes.Add(new MarksGraphic.Shape { kind = "line", a = c + edge * inner, b = c + edge * r, alpha = coneEdgeAlpha });
                }
            }
        }
        sensorRing.Set(shapes);

        if (!sweep) return;
        bool on = s.sensors.Listening && sensorUnits > 0f;
        if (sweep.gameObject.activeSelf != on) sweep.gameObject.SetActive(on);
        if (!on) return;
        bool red = Controls.GridAmber;
        Paint(sweep, red ? sweepInkAmber : data);
        sweep.SetAlphas((red ? 0.75f : 0.7f) * sweepOpacity, (red ? 0.11f : 0.09f) * sweepOpacity); // sweepEdge, sweepTail
        sweep.SetInner(InnerRingPx);
        float d = 2f * (float)(sensorUnits / UnitsPerPx * Controls.ZoomExact);
        if (sweep.rectTransform.sizeDelta != new Vector2(d, d)) sweep.rectTransform.sizeDelta = new Vector2(d, d);
        Put(sweep.rectTransform, OwnAt, -sweepDeg);
    }

    /// <summary>The selected weapon's reach drawn now, units, or 0 (the playtest reads it).</summary>
    public float WeaponRingUnits
    {
        get
        {
            var m = State != null ? State.weapons.MountOf(State.weapons.mount) : null;
            return Controls && Controls.Motion && Controls.Motion.OpenDrawerName == "fire control" && m != null ? m.rangeUnits : 0f;
        }
    }

    // time at our speed: "45 S", "15 MIN", "1 H 12 MIN" (mission time)
    static string Duration(double seconds)
    {
        if (seconds < 60) return ShipState.JsRound(seconds) + " S";
        int min = ShipState.JsRound(seconds / 60);
        return min < 60 ? min + " MIN" : (min / 60) + " H " + (min % 60) + " MIN";
    }

    TMP_Text PassiveLabel(int i)
    {
        while (passiveLabels.Count <= i)
        {
            var t = Instantiate(passiveLabelTemplate, passiveLabelTemplate.transform.parent);
            t.name = "Passive label " + passiveLabels.Count;
            passiveLabels.Add(t);
        }
        passiveLabels[i].gameObject.SetActive(true);
        return passiveLabels[i];
    }

    // Owner 2026-11-01: a killed ship's picture, cut in two across the ship where the shot struck. The halves drift
    // apart along the ship's line and turn slowly, in game time.
    void ShowBroken(ShipState.Track t, ShipView view, Vector2 at, float px, Color ink)
    {
        if (!wreckBackHalf) wreckBackHalf = Instantiate(contactShipImage, contactShipImage.transform.parent);
        wreckBackHalf.texture = view.Texture;
        wreckBackHalf.color = contactShipImage.color;
        wreckPictured = true;
        t.wreckSeenAt = State.missionNow;
        float hdg = (float)(t.nav ? t.nav.DrawnHeading : t.worldHdg), age = State.missionNow - t.wreckAt;
        view.WreckHeading = hdg;
        float cut = 0.5f + (t.breakAt - 0.5f) * view.ShipFill; // up the picture
        float apart = wreckDriftUnits * age / (float)UnitsPerPx * (float)Controls.ZoomExact;
        Half(contactShipImage, new Rect(0f, cut, 1f, 1f - cut), at, hdg, cut * px / 2f + apart, px, wreckSpinDeg * age);
        Half(wreckBackHalf, new Rect(0f, 0f, 1f, cut), at, hdg, cut * px / 2f - px / 2f - apart, px, -wreckSpinDeg * 0.7f * age);
    }

    // One half of a wreck's picture, `along` px toward the ship's nose from its centre.
    void Half(RawImage image, Rect part, Vector2 at, float hdg, float along, float px, float spin)
    {
        image.uvRect = part;
        image.rectTransform.sizeDelta = new Vector2(px, px * part.height);
        float rad = hdg * Mathf.Deg2Rad;
        Put(image.rectTransform, at + new Vector2(Mathf.Sin(rad), -Mathf.Cos(rad)) * along, spin - hdg);
    }

    // The ShipView watching a contact's model, if there is one.
    ShipView ViewOf(string key)
    {
        if (shipViews == null) shipViews = FindObjectsByType<ShipView>(FindObjectsSortMode.None);
        foreach (var v in shipViews)
        {
            var wc = v && v.Target ? v.Target.GetComponentInParent<WorldContact>() : null;
            if (wc && wc.trackKey == key) return v;
        }
        return null;
    }

    // Owner 2026-11-02: where a flight is drawn now, on a curve from its last spot to its new one that follows its heading at each
    // end, as far along as its slide has got. Returns the way it points there.
    float FlightAt(HangarSystem.Flight f, out double x, out double y)
    {
        double cx = f.x - f.fromX, cy = f.y - f.fromY, chord = System.Math.Sqrt(cx * cx + cy * cy);
        double lx = f.x - f.drawn.x, ly = f.y - f.drawn.y;
        double k = chord > 0 ? System.Math.Max(0, 1 - System.Math.Sqrt(lx * lx + ly * ly) / chord) : 1;
        double from = f.fromHdg * System.Math.PI / 180, to = f.hdg * System.Math.PI / 180;
        double along = k * k * (3 - 2 * k), leave = k * (1 - k) * (1 - k) * chord, arrive = k * k * (1 - k) * chord;
        x = f.fromX + cx * along + System.Math.Sin(from) * leave - System.Math.Sin(to) * arrive;
        y = f.fromY + cy * along + System.Math.Cos(from) * leave - System.Math.Cos(to) * arrive;
        return Mathf.LerpAngle((float)f.fromHdg, (float)f.hdg, (float)k);
    }

    Image Fighter(int i)
    {
        while (fighters.Count <= i)
        {
            var f = Instantiate(fighterTemplate, fighterTemplate.transform.parent);
            f.name = "Fighter " + fighters.Count;
            fighters.Add(f);
        }
        fighters[i].gameObject.SetActive(true);
        return fighters[i];
    }

    TMP_Text ContactLabel(int i)
    {
        while (contactLabels.Count <= i)
        {
            var t = Instantiate(contactLabelTemplate, contactLabelTemplate.transform.parent);
            t.name = "Contact label " + contactLabels.Count;
            Paint(t, alarm);
            contactLabels.Add(t);
        }
        contactLabels[i].gameObject.SetActive(true);
        return contactLabels[i];
    }

    // Step 100 (owner 2026-10-27): the compass ring is graded like a compass: a tick every 10 degrees, a longer one
    // every 30 with its number, 000 at the top (north), 359 just left of it and 001 just right.
    readonly List<TMP_Text> gradeLabels = new List<TMP_Text>();
    void Grade(List<MarksGraphic.Shape> shapes, Vector2 c, float r, float alpha, ref int used)
    {
        if (r < 12f) return;
        for (int d = 0; d < 360; d += 10)
        {
            bool major = d % 30 == 0;
            var dir = new Vector2(Mathf.Sin(d * Mathf.Deg2Rad), -Mathf.Cos(d * Mathf.Deg2Rad)); // board y is down
            shapes.Add(new MarksGraphic.Shape { kind = "line", a = c + dir * r, b = c + dir * (r + (major ? 10f : 5f)), alpha = alpha });
            if (!major || r < 40f) continue;
            while (gradeLabels.Count <= used)
            {
                var t = Instantiate(labelTemplate, labelTemplate.transform.parent);
                t.name = "Compass number " + gradeLabels.Count;
                Paint(t, ownForce);
                t.alignment = TextAlignmentOptions.Center;
                gradeLabels.Add(t);
            }
            var l = gradeLabels[used++];
            l.gameObject.SetActive(true);
            l.alpha = alpha;
            l.text = d.ToString("000");
            Place(l.rectTransform, c + dir * (r + 20f), 0.5f);
        }
    }

    // Step 98: once the target is down, the jump point: a ring the size of the reach that wins, and its name
    QuestSystem quest;
    TMP_Text jumpLabel;
    void ShowJumpPoint(List<MarksGraphic.Shape> shapes)
    {
        if (!quest) quest = FindAnyObjectByType<QuestSystem>();
        bool open = quest && quest.ExitOpen;
        if (open)
        {
            var at = Screen(quest.ExitAt.x, quest.ExitAt.y);
            float r = Mathf.Max(6f, (float)(quest.ExitReachUnits / UnitsPerPx * Controls.ZoomExact));
            var ink = Palette.nominal.gamma;
            shapes.Add(new MarksGraphic.Shape { kind = "circle", a = at, b = at + new Vector2(r, 0f), alpha = 1f, ink = ink });
            shapes.Add(new MarksGraphic.Shape { kind = "point", a = at, b = at, alpha = 1f, ink = ink });
            if (!jumpLabel) { jumpLabel = Instantiate(labelTemplate, labelTemplate.transform.parent); jumpLabel.name = "Jump point label"; }
            Paint(jumpLabel, Palette.nominal);
            jumpLabel.text = quest.ExitName;
            jumpLabel.alignment = TextAlignmentOptions.Left;
            Place(jumpLabel.rectTransform, at + new Vector2(r + 6f, 0f), 0f);
        }
        if (jumpLabel && jumpLabel.gameObject.activeSelf != open) jumpLabel.gameObject.SetActive(open);
    }

    TMP_Text Label(int i)
    {
        while (labels.Count <= i)
        {
            var t = Instantiate(labelTemplate, labelTemplate.transform.parent);
            t.name = "Mark label " + labels.Count;
            Paint(t, ownForce);
            if (markLabelSize > 0f) { t.fontSize = markLabelSize; t.fontSharedMaterial = t.font.material; } // owner 2026-10-27: larger and clearer
            labels.Add(t);
        }
        labels[i].gameObject.SetActive(true);
        return labels[i];
    }

    ClickSpot Hit(int i)
    {
        while (hits.Count <= i)
        {
            var go = new GameObject("Mark hit " + hits.Count, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = hitLayer.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(hitLayer, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            go.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            var h = go.AddComponent<ClickSpot>();
            h.action = "plot.erase";
            hits.Add(h);
        }
        return hits[i];
    }

    // Step 76 (owner 2026-10-26): the axis numbers are distances from our ship, in units, along the top and left edges.
    // 0 is level with our ship: they slide as the ship moves and spread as you zoom.
    // Owner 2026-11-01: they follow the grid's sizes. As one size's numbers close up they fade, and the next takes over.
    // No titles. The design's bottom row stays gone: the ORDERS panel covers that edge's left end. The look is on the inspector.
    private readonly List<TextMeshProUGUI> axisLabels = new List<TextMeshProUGUI>();
    const float AxisTopPx = 38f, AxisLeftPx = 23f, AxisEdgePx = 20f; // the design's top row and left column, plot px

    void ShowAxes()
    {
        if (!axisLayer || !axisFont) return;
        Rect r = grid.rectTransform.rect;
        var o = marks ? marks.origin : Vector2.zero;
        double unit = grid.SmallestUnits;
        float step = (float)unit * grid.PxPerUnit;
        if (step < 1f) return;
        Vector2 own = OwnAt;
        int used = 0;
        for (int k = Mathf.CeilToInt((o.x + AxisEdgePx - own.x) / step); own.x + k * step <= o.x + r.width - AxisEdgePx; k++)
        {
            double units = System.Math.Abs(k) * unit;
            float share = grid.LineShare(units, axisAllNumbersPx, axisHalfNumbersPx);
            if (share > 0f) AxisLabel(used++, units, new Vector2(own.x + k * step, o.y + AxisTopPx), share);
        }
        for (int k = Mathf.CeilToInt((o.y + AxisEdgePx - own.y) / step); own.y + k * step <= o.y + r.height - AxisEdgePx; k++)
        {
            double units = System.Math.Abs(k) * unit;
            float share = grid.LineShare(units, axisAllNumbersPx, axisHalfNumbersPx);
            if (share > 0f) AxisLabel(used++, units, new Vector2(o.x + AxisLeftPx, own.y + k * step), share);
        }
        for (int i = used; i < axisLabels.Count; i++) if (axisLabels[i].gameObject.activeSelf) axisLabels[i].gameObject.SetActive(false);
    }

    void AxisLabel(int i, double km, Vector2 board, float alpha)
    {
        while (axisLabels.Count <= i)
        {
            var go = new GameObject("Axis number", typeof(RectTransform));
            go.transform.SetParent(axisLayer, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0f, 1f); // as Put expects
            t.rectTransform.sizeDelta = new Vector2(50f, 12f); // room for "100,000"
            t.font = axisFont; // through the properties, not serialization (BUILD_LOG trap 2)
            t.fontSharedMaterial = axisMaterial ? axisMaterial : axisFont.material;
            t.fontSize = axisFontSize;
            t.color = axisColour;
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.richText = false;
            t.raycastTarget = false;
            axisLabels.Add(t);
        }
        var l = axisLabels[i];
        l.text = km.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture);
        l.alpha = axisColour.a * alpha;
        if (!l.gameObject.activeSelf) l.gameObject.SetActive(true);
        Put(l.rectTransform, board, 0f);
    }

    // Children of a plot-sized rect anchored top-left at the plot origin (104,100).
    // Ruins (see ruinsImage): a camera looking straight down at the found terrain, framed as the plot is
    // (the view's centre at plotCentre, z board px per units), into a picture the size of the plot
    void ShowRuins(Vector2 size, double z)
    {
        if (!ruinsImage) return;
        int w = Mathf.Max(1, Mathf.RoundToInt(size.x)), h = Mathf.Max(1, Mathf.RoundToInt(size.y));
        if (!ruinsCamera)
        {
            ruinsCamera = new GameObject("Ruins View Camera").AddComponent<Camera>();
            ruinsCamera.orthographic = true;
            ruinsCamera.allowHDR = false; // as the ship views: the line-art pass keeps its red on black
            ruinsCamera.clearFlags = CameraClearFlags.SolidColor;
            ruinsCamera.backgroundColor = new Color(0, 0, 0, 0);
            ruinsCamera.cullingMask = LayerMask.GetMask("WorldTerrain");
            ruinsCamera.nearClipPlane = 1f;
            ruinsCamera.farClipPlane = 200000f;
        }
        if (!ruinsTexture || ruinsTexture.width != w || ruinsTexture.height != h)
        {
            if (ruinsTexture) { ruinsCamera.targetTexture = null; ruinsTexture.Release(); Destroy(ruinsTexture); }
            ruinsTexture = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            { name = "Ruins view", hideFlags = HideFlags.DontSave, antiAliasing = 4 };
            ruinsCamera.targetTexture = ruinsTexture;
            ruinsImage.texture = ruinsTexture;
        }
        ruinsCamera.orthographicSize = (float)(h / 2.0 / z);
        ruinsCamera.transform.SetPositionAndRotation(new Vector3((float)viewX, (float)viewY, -100000f), Quaternion.identity); // look along +z, north up
        ruinsImage.rectTransform.sizeDelta = size;
        Put(ruinsImage.rectTransform, plotCentre, 0f);
        var ink = ruinsColor.gamma; // the board canvas linearises vertex colours; pre-encode
        ink.a *= 1f - CloseUp; // the close-up shows the rocks themselves
        ruinsImage.color = ink;
    }

    // The close-up (see closeUpImage): framed as the ruins are, filmed only while it shows
    void ShowCloseUp(Vector2 size, double z)
    {
        if (!closeUpImage) return;
        float a = CloseUp;
        closeUpImage.enabled = a > 0f;
        if (closeUpCamera) closeUpCamera.enabled = a > 0f;
        if (nebula != null) foreach (var n in nebula) if (n.image) n.image.enabled = a > 0f;
        if (a <= 0f) return;
        if (!closeUpCamera)
        {
            closeUpCamera = new GameObject("Close-up Camera").AddComponent<Camera>();
            closeUpCamera.gameObject.layer = LayerMask.NameToLayer("WorldShips"); // its look (the Volume below) reaches only cameras that take this layer's looks
            closeUpCamera.orthographic = true;
            closeUpCamera.clearFlags = CameraClearFlags.SolidColor;
            closeUpCamera.backgroundColor = new Color(0, 0, 0, 0);
            closeUpCamera.cullingMask = LayerMask.GetMask("WorldShips", "WorldTerrain", "TransparentFX"); // TransparentFX: shots, blasts, wreck halves
            closeUpCamera.nearClipPlane = 1f;
            closeUpCamera.farClipPlane = 200000f;
            var data = closeUpCamera.GetUniversalAdditionalCameraData();
            data.SetRenderer(closeUpRenderer);
            data.renderPostProcessing = closeUpLook;
            data.volumeLayerMask = 1 << closeUpCamera.gameObject.layer;
            if (closeUpLook) { var look = closeUpCamera.gameObject.AddComponent<Volume>(); look.isGlobal = true; look.sharedProfile = closeUpLook; }
            closeUpImage.material = new Material(closeUpImage.material); // its own copy: the circles are set on it
            closeUpImage.material.SetFloat("_KeyBlack", 0f); // shown as filmed
            nebulaMaterial = new Material(closeUpImage.material);
            closeUpImage.material.SetFloat("_SrcBlend", (float)BlendMode.One); // its light adds over the nebula: a glow over empty space shows
            RenderPipelineManager.beginCameraRendering += CloseUpBegins;
            RenderPipelineManager.endCameraRendering += CloseUpEnds;
        }
        float sharp = Mathf.Max(1f, closeUpImage.canvas.rootCanvas.scaleFactor); // the screen's pixels, not the board's 1920 x 1080 layout
        int w = Mathf.Max(1, Mathf.RoundToInt(size.x * sharp)), h = Mathf.Max(1, Mathf.RoundToInt(size.y * sharp));
        if (!closeUpTexture || closeUpTexture.width != w || closeUpTexture.height != h)
        {
            if (closeUpTexture) { closeUpCamera.targetTexture = null; closeUpTexture.Release(); Destroy(closeUpTexture); }
            closeUpTexture = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            { name = "Close-up view", hideFlags = HideFlags.DontSave, antiAliasing = 4 };
            closeUpCamera.targetTexture = closeUpTexture;
            closeUpImage.texture = closeUpTexture;
        }
        closeUpCamera.orthographicSize = (float)(size.y / 2.0 / z);
        closeUpCamera.transform.SetPositionAndRotation(new Vector3((float)viewX, (float)viewY, -100000f), Quaternion.identity); // look along +z, north up
        closeUpImage.rectTransform.sizeDelta = size;
        Put(closeUpImage.rectTransform, plotCentre, 0f);
        closeUpImage.color = new Color(1f, 1f, 1f, a);
        // the nebula: each layer repeats, and moves and grows less than the grid the farther away it is
        if (nebula != null)
            foreach (var n in nebula)
            {
                if (!n.image) continue;
                n.image.material = nebulaMaterial; // cut at the visual range circles too
                n.image.rectTransform.sizeDelta = size;
                Put(n.image.rectTransform, plotCentre, 0f);
                double tilePx = n.tileUnits / UnitsPerPx * System.Math.Pow(Controls.ZoomExact, n.depth);
                double shift = n.depth / n.tileUnits; // tiles the layer moves per unit the view moves: what is at the plot's centre doesn't change as you zoom
                n.image.uvRect = new Rect((float)(viewX * shift - size.x / 2 / tilePx), (float)(viewY * shift - size.y / 2 / tilePx), (float)(size.x / tilePx), (float)(size.y / tilePx));
                n.image.color = new Color(1f, 1f, 1f, a * n.alpha);
            }
        var sun = RenderSettings.sun; // the board scene's sun: Sun Source in its Lighting settings
        if (sun) // the light travels away from where it comes from; the camera side of the plot is -z
        {
            float from = sunFromDeg * Mathf.Deg2Rad, up = sunHeightDeg * Mathf.Deg2Rad;
            sun.transform.rotation = Quaternion.LookRotation(-new Vector3(Mathf.Cos(up) * Mathf.Sin(from), Mathf.Cos(up) * Mathf.Cos(from), -Mathf.Sin(up)));
        }
    }

    // Owner 2026-11-08: in the close-up each shot in flight is a glowing streak, ours blue-white and theirs red. A shot gone
    // before its reach ran out has hit something, or been shot down: a flash where it was. A destroyed ship: a blast, and
    // its model in two halves that drift apart and turn, as its line picture does (owner 2026-11-01).
    void ShowShotsAndBlasts()
    {
        bool on = closeUpCamera && closeUpCamera.enabled;
        int used = 0, models = 0;
        foreach (var shot in shotsNow)
        {
            var r = shot.r;
            var ink = shot.ours ? ourShotGlow : theirShotGlow;
            if (!r.torpedo || r.motor) Streak(used++, shot.tail, shot.head, r.torpedo ? torpedoWidthUnits : slugWidthUnits, new Color(ink.r, ink.g, ink.b, 0f), ink);
            if (!r.torpedo) continue;
            // owner 2026-11-10 (drop, drift, flare): the torpedo's body along its nose, which swings onto the target while it drifts cold.
            // The nose turns on between ticks, at its turn now.
            double deg = (r.motor ? System.Math.Atan2(r.dx, r.dy) * 180 / System.Math.PI : r.nose) + r.turn * Controls.SinceTick, rad = deg * System.Math.PI / 180;
            var along = new Vector3((float)System.Math.Sin(rad), (float)System.Math.Cos(rad), 0f);
            if (torpedoLook) // owner 2026-11-10: a small cylinder, just behind the glows
            {
                var model = TorpedoModel(models++);
                model.SetPositionAndRotation(new Vector3(shot.head.x, shot.head.y, EffectZ + 10f), Quaternion.Euler(0f, 0f, -(float)deg));
                model.localScale = new Vector3(torpedoWidthUnits / 2f, torpedoBodyUnits / 2f, torpedoWidthUnits / 2f); // the cylinder is 2 long, 1 across
            }
            else Streak(used++, shot.head - along * (torpedoBodyUnits / 2f), shot.head + along * (torpedoBodyUnits / 2f), torpedoWidthUnits / 2f, torpedoBodyInk, torpedoBodyInk, 1f);
            // its motor bursts on: a flare that grows and fades
            float age = r.motor ? (float)r.lit + Controls.SinceTick : float.MaxValue;
            if (age >= torpedoFlareSeconds) continue;
            float f = 1f - age / torpedoFlareSeconds, size = torpedoFlareUnits * (1.5f - 0.5f * f);
            var flare = Color.Lerp(ink, Color.white, 0.5f);
            flare.a = f;
            Streak(used++, shot.head - along * (size / 2f), shot.head + along * (size / 2f), size, flare, flare, 1f);
        }
        for (int i = used; i < streaks.Count; i++) streaks[i].enabled = false;
        for (int i = models; i < torpedoModels.Count; i++) torpedoModels[i].gameObject.SetActive(false);
        flying.Clear();
        foreach (var shot in shotsNow) flying.Add(shot.r);
        if (on && hitFlash)
            foreach (var before in shotsBefore)
                if (!flying.Contains(before.Key) && !Flying(before.Key) && before.Key.travelled < before.Key.maxUnits - 1) Play(hitFlash, before.Value);
        KeepGameTime();
        shotsBefore.Clear();
        foreach (var shot in shotsNow) shotsBefore[shot.r] = shot.head;
        shotsNow.Clear();

        foreach (var t in State.tracks)
        {
            if (!t.wreck || !t.nav) continue;
            if (!wrecks.TryGetValue(t, out var halves))
            {
                if (!on) continue; // cut only when the close-up first shows it: a big model takes a moment
                wrecks[t] = halves = Break(t);
                if (on && killBlast && State.missionNow - t.wreckAt < BlastWithinSeconds) Play(killBlast, new Vector3((float)t.nav.DrawnX, (float)t.nav.DrawnY, EffectZ));
            }
            if (halves == null) continue;
            float hdg = (float)t.nav.DrawnHeading, age = State.missionNow - t.wreckAt;
            var turn = Quaternion.Euler(0f, 0f, -hdg);
            var at = new Vector3((float)t.nav.DrawnX, (float)t.nav.DrawnY, 0f);
            var nose = turn * Vector3.up;
            bool show = on && State.sensors.InSight(t);
            for (int h = 0; h < 2; h++)
            {
                var piece = halves.piece[h];
                piece.gameObject.SetActive(show);
                if (!show) continue;
                float apart = (h == 0 ? 1f : -1f) * wreckDriftUnits * age, spin = h == 0 ? wreckSpinDeg * age : -wreckSpinDeg * 0.7f * age;
                piece.SetPositionAndRotation(at + turn * halves.centre[h] + nose * apart, Quaternion.Euler(0f, 0f, spin - hdg));
            }
        }
    }

    // A flash or a blast, in game time: still while the board is paused, five times as fast at x5 (owner: everything in game time).
    void Play(ParticleSystem effect, Vector3 at)
    {
        if (effect) blasts.Add(Instantiate(effect, at, Quaternion.identity).GetComponentsInChildren<ParticleSystem>());
    }
    void KeepGameTime()
    {
        blasts.RemoveAll(b => b[0] == null); // a finished one destroys itself
        float speed = Controls.Paused ? 0f : Controls.Rate;
        foreach (var b in blasts) foreach (var ps in b) { var main = ps.main; main.simulationSpeed = speed; }
    }

    // a round still in either list (one not drawn this time, while the close-up was off, is not a hit)
    bool Flying(WeaponSystem.Round r) => State.weapons.Rounds.Contains(r) || State.weapons.Incoming.Contains(r);

    // One glowing stroke in the close-up, streaks[i]: tail to head, its width at the head and a share of that at the tail.
    void Streak(int i, Vector3 tail, Vector3 head, float width, Color tailInk, Color headInk, float tailShare = StreakTail)
    {
        while (streaks.Count <= i) streaks.Add(NewStreak());
        var line = streaks[i];
        line.enabled = true;
        line.SetPosition(0, tail);
        line.SetPosition(1, head);
        line.startWidth = width * tailShare;
        line.endWidth = width;
        line.startColor = tailInk;
        line.endColor = headInk;
    }

    // Owner 2026-11-10: torpedo models for the close-up, made as needed and kept for the next frame.
    Transform TorpedoModel(int i)
    {
        while (torpedoModels.Count <= i)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Torpedo model";
            Destroy(go.GetComponent<Collider>());
            go.layer = LayerMask.NameToLayer("TransparentFX");
            var look = go.GetComponent<MeshRenderer>();
            look.sharedMaterial = torpedoLook;
            look.shadowCastingMode = ShadowCastingMode.Off;
            torpedoModels.Add(go.transform);
        }
        torpedoModels[i].gameObject.SetActive(true);
        return torpedoModels[i];
    }

    // Owner 2026-11-10: zoomed in, a torpedo's model shows, so its table dot and trail fade out as the close-up comes in.
    float TableDot(WeaponSystem.Round r) => r.torpedo && torpedoLook ? 1f - CloseUp : 1f;

    // Owner 2026-11-10 (drop, drift, flare): on the table, a ring grows and fades where a torpedo's motor lit.
    void AddLightRing(List<MarksGraphic.Shape> list, WeaponSystem.Round r)
    {
        float age = r.torpedo && r.motor ? ((float)r.lit + Controls.SinceTick) / torpedoFlareSeconds : 1f;
        if (age >= 1f) return;
        var at = Screen(r.litX, r.litY);
        list.Add(new MarksGraphic.Shape { kind = "circle", a = at, b = at + new Vector2(4f + torpedoRingPx * age, 0f), alpha = 1f - age, ink = caution.gamma });
    }

    LineRenderer NewStreak()
    {
        var line = new GameObject("Shot streak").AddComponent<LineRenderer>();
        line.gameObject.layer = LayerMask.NameToLayer("TransparentFX");
        line.sharedMaterial = glow;
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.textureMode = LineTextureMode.Stretch;
        line.alignment = LineAlignment.View;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        return line;
    }

    // A destroyed ship's model cut in two across the ship where the shot struck (breakAt, 0 the stern to 1 the nose): the
    // triangles ahead of the cut, and those behind. Each half is centred on its own middle so it turns about it. Null if the
    // model can't be read (its file needs Read/Write on, as the cruiser's has).
    Halves Break(ShipState.Track t)
    {
        var ship = t.nav.GetComponent<Ship>();
        var mf = ship ? ship.GetComponentInChildren<MeshFilter>(true) : null;
        if (!mf || !mf.sharedMesh || !mf.sharedMesh.isReadable || !wreckLook) return null;
        var m = mf.sharedMesh;
        var toShip = ship.transform.worldToLocalMatrix * mf.transform.localToWorldMatrix; // the ship's own space: nose up (+y)
        var v = m.vertices;
        var normals = m.normals;
        for (int i = 0; i < v.Length; i++) v[i] = toShip.MultiplyPoint3x4(v[i]);
        for (int i = 0; i < normals.Length; i++) normals[i] = toShip.MultiplyVector(normals[i]).normalized;
        float cut = (t.breakAt - 0.5f) * ship.Stats.length;
        var parts = new[] { new List<int>(), new List<int>() }; // ahead of the cut, behind it
        for (int sub = 0; sub < m.subMeshCount; sub++)
        {
            var tri = m.GetTriangles(sub);
            for (int k = 0; k < tri.Length; k += 3)
            {
                var part = parts[(v[tri[k]].y + v[tri[k + 1]].y + v[tri[k + 2]].y) / 3f >= cut ? 0 : 1];
                part.Add(tri[k]); part.Add(tri[k + 1]); part.Add(tri[k + 2]);
            }
        }
        var halves = new Halves();
        for (int h = 0; h < 2; h++)
        {
            var c = Vector3.zero;
            foreach (int i in parts[h]) c += v[i];
            c /= Mathf.Max(1, parts[h].Count);
            var newIndex = new Dictionary<int, int>(); // only the points this half uses
            var points = new List<Vector3>();
            var pointNormals = new List<Vector3>();
            var tris = parts[h];
            for (int k = 0; k < tris.Count; k++)
            {
                if (!newIndex.TryGetValue(tris[k], out int n))
                {
                    newIndex[tris[k]] = n = points.Count;
                    points.Add(v[tris[k]] - c);
                    pointNormals.Add(tris[k] < normals.Length ? normals[tris[k]] : Vector3.forward);
                }
                tris[k] = n;
            }
            var mesh = new Mesh { name = ship.name + (h == 0 ? " front half" : " back half"), indexFormat = points.Count > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16 };
            mesh.SetVertices(points);
            mesh.SetNormals(pointNormals);
            mesh.SetTriangles(tris, 0);
            mesh.UploadMeshData(true);
            var go = new GameObject(mesh.name);
            go.layer = LayerMask.NameToLayer("TransparentFX");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = wreckLook;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            go.SetActive(false);
            halves.piece[h] = go.transform;
            halves.centre[h] = c;
        }
        return halves;
    }

    void CloseUpBegins(ScriptableRenderContext context, Camera cam) { if (cam == closeUpCamera) LeaveOut(true); }
    void CloseUpEnds(ScriptableRenderContext context, Camera cam) { if (cam == closeUpCamera) LeaveOut(false); }
    void LeaveOut(bool off)
    {
        foreach (var ship in Ship.All)
            if (notInCloseUp.Contains(ship.Nav)) foreach (var r in ship.Models) if (r) r.forceRenderingOff = off;
    }

    // Owner 2026-11-02: what is in sight shows inside the visual range circle round our ship and round each fighter flight;
    // past them a rock shows only as its DRADIS echo. A rock across the edge is cut there: the ruins picture keeps what is
    // inside the circles, the echoes (their own copy of the contact marks, under them) what is outside.
    private MarksGraphic rockEchoMarks;
    private readonly Vector4[] sightCircles = new Vector4[4];

    // A copy of a marks layer, just under it, for marks with a look of their own (made while the game runs)
    MarksGraphic CopyLayer(MarksGraphic of, string name)
    {
        var m = Instantiate(of, of.transform.parent);
        foreach (Transform c in m.transform) Destroy(c.gameObject);
        var fade = m.GetComponent<CanvasGroup>(); // the grid's fade-in may be on the layer as it is copied; the copy would stay at 0
        if (fade) Destroy(fade);
        m.name = name;
        m.transform.SetSiblingIndex(of.transform.GetSiblingIndex());
        return m;
    }
    void ClipToSight()
    {
        if (!ruinsImage || !contactMarks || !marks) return;
        if (!rockEchoMarks)
        {
            ruinsImage.material = new Material(ruinsImage.material); // its own copy: our ship's view uses the same material
            rockEchoMarks = CopyLayer(contactMarks, "Rock Echoes");
            rockEchoMarks.material = new Material(ruinsImage.material);
            rockEchoMarks.material.SetFloat("_KeyBlack", 0f);
            rockEchoMarks.material.SetFloat("_KeepOutside", 1f);
        }
        var canvas = ruinsImage.canvas.rootCanvas.transform; // UI vertices reach the shader in the root canvas's space
        float eyes = (float)(State.sensors.stats.visualRangeUnits / UnitsPerPx * Controls.ZoomExact);
        int n = 0;
        void Add(double x, double y)
        {
            if (n >= sightCircles.Length) return;
            Vector2 at = Screen(x, y);
            Vector3 c = canvas.InverseTransformPoint(marks.World(at)), edge = canvas.InverseTransformPoint(marks.World(at + new Vector2(eyes, 0f)));
            sightCircles[n++] = new Vector4(c.x, c.y, Vector2.Distance(c, edge), 0f);
        }
        Add(State.nav.DrawnX, State.nav.DrawnY);
        if (State.hangar) foreach (var f in State.hangar.Flights) Add(f.x, f.y);
        foreach (var m in new[] { ruinsImage.material, rockEchoMarks.material, closeUpCamera ? closeUpImage.material : null, nebulaMaterial })
            if (m) { m.SetVectorArray("_Circles", sightCircles); m.SetFloat("_CircleCount", n); }
    }

    // Gas clouds (see cloudImage): a copy of the picture for each cloud in visual range, at the plot's pan and zoom.
    // Each copy is flipped its own way, so the clouds don't all look the same.
    void ShowClouds(double z)
    {
        if (!cloudImage) return;
        if (!world) world = FindAnyObjectByType<TacticalWorld>();
        if (!world) return;
        while (cloudImages.Count < world.Clouds.Count)
        {
            var copy = Instantiate(cloudImage, cloudImage.transform.parent);
            copy.transform.SetSiblingIndex(cloudImage.transform.GetSiblingIndex());
            if (fading != null) fading.Add(copy.GetComponent<CanvasGroup>()); // Step 120: the copy fades in with the layer it was copied from
            cloudImages.Add(copy);
        }
        var ink = ruinsColor.gamma;
        ink.a = cloudAlpha;
        for (int i = 0; i < cloudImages.Count; i++)
        {
            var c = i < world.Clouds.Count ? world.Clouds[i] : null;
            bool show = c && c.inSight;
            Show(cloudImages[i], show);
            if (!show) continue;
            cloudImages[i].rectTransform.sizeDelta = new Vector2((float)(c.widthUnits * z), (float)(c.heightUnits * z));
            cloudImages[i].rectTransform.localScale = new Vector3(i % 2 == 0 ? 1f : -1f, i % 4 < 2 ? 1f : -1f, 1f);
            Put(cloudImages[i].rectTransform, Screen(c.At.x, c.At.y), 0f);
            cloudImages[i].color = ink;
        }
    }

    private void OnDestroy()
    {
        if (ruinsCamera) { ruinsCamera.targetTexture = null; Destroy(ruinsCamera.gameObject); }
        if (ruinsTexture) { ruinsTexture.Release(); Destroy(ruinsTexture); }
        RenderPipelineManager.beginCameraRendering -= CloseUpBegins;
        RenderPipelineManager.endCameraRendering -= CloseUpEnds;
        if (closeUpCamera) { closeUpCamera.targetTexture = null; Destroy(closeUpCamera.gameObject); if (closeUpImage) Destroy(closeUpImage.material); Destroy(nebulaMaterial); }
        foreach (var w in wrecks.Values) if (w != null) foreach (var p in w.piece) if (p) { Destroy(p.GetComponent<MeshFilter>().sharedMesh); Destroy(p.gameObject); }
        if (closeUpTexture) { closeUpTexture.Release(); Destroy(closeUpTexture); }
    }

    void Put(RectTransform rt, Vector2 board, float rotZ)
    {
        var o = marks ? marks.origin : Vector2.zero;
        rt.anchoredPosition = new Vector2(board.x - o.x, -(board.y - o.y));
        rt.localEulerAngles = new Vector3(0, 0, rotZ);
    }

    void Place(RectTransform rt, Vector2 board, float pivotX)
    {
        rt.pivot = new Vector2(pivotX, 0.5f);
        Put(rt, board, 0f);
    }

    // marksView helpers

    static string Brg(double dx, double dyNorth)
    {
        double deg = (System.Math.Atan2(dx, dyNorth) * 180 / System.Math.PI + 360) % 360;
        return (ShipState.JsRound(deg) % 360).ToString("000");
    }

    // clk: hh:mm:ss from the mission clock; seconds print as JS prints a number (2.25 stays 2.25)
    static string Clock(float t)
    {
        double s = System.Math.Floor(t); // the clock runs in fractions of a second since the spacing change: whole seconds here
        string P(double n) { var str = n.ToString("R", System.Globalization.CultureInfo.InvariantCulture); return str.Length < 2 ? "0" + str : str; }
        return P(System.Math.Floor(s / 3600) % 100) + ":" + P(System.Math.Floor(s / 60) % 60) + ":" + P(s % 60);
    }
}
