using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Steps 30–31. Builds the handoff board in 02.1_Grid from layout/layout.json: every texture,
/// sprite, lamp, icon, text run and drawn shape, placed in px at 1920x1080 (README, "Layout").
/// The default state is visible; every other state (drawers, helm, orders, periscope) is built
/// under its own inactive group. Re-running deletes the old board and builds it again.
/// The rules for the gaps in the package are in Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md.
/// </summary>
public static class HandoffBoardBuilder
{
    const string ScenePath = "Assets/Scenes/Tactical/02.1_Grid.unity";
    const string CanvasName = "Board";
    const string RootName = "HandoffBoard";
    const string Root = HandoffImport.Root;
    const string Gen = HandoffImport.Gen;

    // Drawing order of the default-state panels, back to front.
    static readonly string[] PanelOrder =
        { "Frame", "Plot", "AlarmStrip", "TimeControl", "ToolRail", "SystemsRail", "Console", "Orders", "Helm", "Periscope" };

    // Which panel each default-state record belongs to. layout.json lists each record type in page
    // order, so a panel is a run of indices. Read off this export (2026-09-27); a re-export that
    // changes the page must update this table. Ask Claude Design for a panel tag per record.
    static readonly (string section, int from, int to, string panel)[] PanelRuns =
    {
        ("textures", 0, 0, "Frame"),
        ("sprites", 0, 10, "Frame"), ("text", 0, 5, "Frame"), ("drawn", 0, 24, "Frame"),
        ("sprites", 11, 17, "TimeControl"), ("text", 6, 13, "TimeControl"), ("lamps", 0, 0, "TimeControl"),
        ("sprites", 18, 18, "AlarmStrip"), ("text", 14, 15, "AlarmStrip"), ("lamps", 1, 1, "AlarmStrip"),
        ("sprites", 19, 35, "ToolRail"), ("lamps", 2, 6, "ToolRail"), ("icons", 0, 4, "ToolRail"),
        ("text", 16, 20, "ToolRail"), ("drawn", 25, 30, "ToolRail"),
        ("drawn", 31, 32, "Plot"), ("text", 21, 36, "Plot/Scale"),
        ("drawn", 33, 55, "Plot/Mockup"), ("text", 37, 53, "Plot/Mockup"), ("lamps", 7, 12, "Plot/Mockup"),
        ("sprites", 36, 54, "SystemsRail"), ("lamps", 13, 20, "SystemsRail"), ("icons", 5, 11, "SystemsRail"),
        ("text", 54, 59, "SystemsRail"), ("drawn", 56, 63, "SystemsRail"),
        ("sprites", 55, 73, "Periscope"), ("sprites", 376, 376, "Periscope"), ("text", 60, 77, "Periscope"),
        ("drawn", 64, 80, "Periscope"), ("lamps", 21, 23, "Periscope"),
        ("sprites", 74, 81, "Console"), ("sprites", 377, 380, "Console"), ("lamps", 24, 26, "Console"),
        ("text", 78, 82, "Console"),
        ("sprites", 82, 92, "Orders"), ("sprites", 381, 384, "Orders"), ("lamps", 27, 29, "Orders"),
        ("text", 83, 89, "Orders"), ("drawn", 81, 84, "Orders"),
        ("sprites", 93, 95, "Helm"), ("lamps", 30, 30, "Helm"), ("text", 90, 99, "Helm"),
    };

    // Periscope clip box while housed: the board animates its height 44 -> 656 px (UIStateSpec 9.1).
    static readonly Rect PeriscopeHoused = new Rect(750, 0, 420, 44);

    // Shadow margins the asset-map notes give per side (left, top, right, bottom), in 1x px.
    // Checked against image size minus element size.
    static readonly Dictionary<string, Vector4> SideMargins = new Dictionary<string, Vector4>
    {
        { "helm_collapsed", new Vector4(32, 44, 32, 8) },
        { "helm_body", new Vector4(32, 44, 32, 8) },
        { "periscope_housing", new Vector4(46, 8, 46, 64) },
        { "orders_body_closed", new Vector4(16, 8, 16, 20) },
        { "orders_body_open", new Vector4(16, 8, 16, 20) },
        { "periscope_carriage", new Vector4(12, 4, 12, 16) }, // measured solid-body inset; sums to image minus element
    };

    // Uniform transparent pad measured in the PNGs, for sprites whose notes give no margin
    // (Step 30 verification, defect D1/D3). The image-minus-element rule is wrong for these:
    // most are 9-sliced, so their image size is a slice source, not the element plus a shadow.
    static readonly Dictionary<string, float> MeasuredMargins = new Dictionary<string, float>
    {
        { "glass_r3", 2 }, { "glass_r3_c", 2 }, { "glass_r3_d", 2 }, { "glass_r3_e", 2 },
        { "well_r2", 2 }, { "well_r3", 2 }, { "well_r4", 2 }, { "bay_r4", 2 }, { "bay_r4_b", 2 },
        { "rate_key_off", 2 }, { "rate_key_on", 2 }, { "alarm_strip", 2 },
        { "key_flat_a", 4 }, { "key_flat_b", 4 }, { "plate_dark", 4 }, { "plate_rail", 4 }, { "plate_legend", 4 },
        { "bus_terminal", 4 }, { "lever_slot_launch", 4 }, { "well_reactor", 4 },
        { "screen_thick_r3", 8 }, { "screen_thick_r4", 8 },
        { "icon_sensors_layer2", 6 }, // stacks on icon_sensors, "same size and position"
    };

    // "Fixed composition, placed as one piece" (asset-map): drawn at native 1x size, top-left here.
    // layout.json records only a strip of each. Each top-left is found by lining the sprite's own
    // pipes and clamps up with the drawn records that measured them (left: sprite #0, drawn #1–2;
    // mid: drawn #11–14 and sprite #3; right: drawn #23 and sprite #4) — every anchor agrees to 1 px.
    static readonly Dictionary<string, Vector2> FixedCompositions = new Dictionary<string, Vector2>
    {
        { "conduit_left", new Vector2(30, 708) },
        { "conduit_mid", new Vector2(146, 996) },
        { "conduit_right", new Vector2(1592, 702) },
    };

    static Rect FixedRect(string name)
    {
        var s = LoadSprite(name);
        return new Rect(FixedCompositions[name], new Vector2(s.rect.width, s.rect.height) / (s.pixelsPerUnit / 100f));
    }

    // A lit lamp's glow: lamp_glow is 32 px for the 16 px lamp_dot, so it reaches 8 px past the lamp.
    const float GlowReach = 8f;
    // Glow strength. board-logic.js draws lit-lamp glows as box-shadows at .7–.8 alpha (e.g.
    // "0 0 7px rgba(143,195,58,.75)"); layout.json gives the lamp's alpha, not the glow's.
    const float GlowAlpha = 0.75f;
    // Gradient stand-ins this big (drawn #34 and #38, the two big discs over the plot centre) are built but hidden:
    // a flat disc that size dominates the plot (Step 31 QA). They return when colour stops arrive.
    const float HideStandInOver = 200f;
    // TMP's SDF strokes come out thinner than the browser's Share Tech Mono (Step 31 QA: 7–17 px
    // runs peaked at 0.72–0.91 of the reference; Barlow matched). Share Tech Mono uses a dilated
    // material copy. Measured by brightest-5% ratio against board-1920x1080.png.
    const string ThickenedFamily = "Share Tech Mono";
    const float ThickenDilate = 0.3f;
    // Stand-in for the 36 "gradient" fills the package gives no colours for.
    const string StandInToken = "ink.stencilDim";
    const float StandInAlpha = 0.35f;

    class Rec
    {
        public string section, kind, state, panel, name;
        public int index, typeOrder;
        public JObject j;
        public Rect box;           // board px, y down: the record's bounding box
        public int depth;
    }

    class Group { public RectTransform rt; public Vector2 origin; }

    static JObject layout, tokens, plotGrid;
    static Dictionary<string, Dictionary<string, string>> assetMap;
    static Dictionary<string, Vector4> marginCache;
    // Each built sprite's shadow margin as placed (left, top, right, bottom, board px): a control's
    // click area must not include its baked shadow (Step 33f verification).
    static Dictionary<GameObject, Vector4> spriteGrow;
    static Dictionary<string, TMP_FontAsset> fonts;
    static StringBuilder report;
    static Dictionary<string, int> counts;

    [MenuItem("Starwolf/Handoff/2 · Build static board in 02.1_Grid")]
    public static void Build()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            if (scene.isDirty) { Debug.LogError("[HandoffBoard] Save " + scene.path + " first."); return; }
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        var canvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(c => c.name == CanvasName);
        if (canvas == null) { Debug.LogError("[HandoffBoard] No canvas named " + CanvasName); return; }

        Load();
        EnsureGammaPipeline(canvas);
        var old = canvas.transform.Find(RootName);
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var root = NewRect(RootName, canvas.transform);
        root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(1920, 1080);
        root.anchoredPosition = Vector2.zero;
        var board = new Group { rt = root, origin = Vector2.zero };

        var recs = ReadRecords();
        BuildDefault(board, recs.Where(r => r.state == "default").ToList());
        foreach (var state in recs.Select(r => r.state).Where(s => s != "default").Distinct())
        {
            var g = Child(board, "State · " + state, new Rect(0, 0, 1920, 1080), false);
            // placed while active: an inactive TMP measures ~1/10 of its real width (Step 31 QA, R2)
            Place(g, recs.Where(r => r.state == state).ToList());
            g.rt.gameObject.SetActive(false);
        }
        AddBodyBlockers(root);
        AddReferenceOverlay(board);
        WireMotion(root);
        WireControls(root, canvas);
        WirePower(root);
        WireSensors(root);
        WireComms(root);
        WireEngineering(root);
        WireHangar(root);
        WireFireControl(root);
        WireHelm(root);
        WireOrders(root);
        WirePeriscope(root);
        WirePlot(root);
        WireRockers(root);
        RelinkWorld(root);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[HandoffBoard] built\n" + string.Join("\n", counts.OrderBy(k => k.Key).Select(k => k.Key + ": " + k.Value)) + "\n" + report);
    }

    // ---- gamma-space board (owner, 2026-10-04: "fix in the board only") ------------------
    // Board Camera draws the Board canvas into a linear-format texture; Main Camera shows it through
    // an overlay canvas and Starwolf/UI/BoardGammaToLinear. See GammaBoardPresenter.cs.
    static void EnsureGammaPipeline(Canvas canvas)
    {
        int ui = LayerMask.NameToLayer("UI");
        var main = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).First(c => c.name == "Main Camera");
        Color ground;
        ColorUtility.TryParseHtmlString((string)tokens["board.ground"]["hex"], out ground);

        var boardGo = GameObject.Find("Board Camera") ?? new GameObject("Board Camera");
        boardGo.transform.position = main.transform.position;
        var cam = Get<Camera>(boardGo);
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = ground.gamma; // Unity linearises the clear colour; this lands as the sRGB value
        cam.cullingMask = 1 << ui;
        cam.depth = main.depth - 1;
        cam.allowHDR = false;
        cam.allowMSAA = false;
        var urp = Get<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>(boardGo);
        urp.renderPostProcessing = false;
        urp.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None;
        urp.requiresDepthOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;
        urp.requiresColorOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;

        canvas.worldCamera = cam;
        // Unity linearises UI vertex colours once whatever this flag says (measured: icon grey 168
        // came out 100 = linear(168)). Leave it off; Hex() pre-encodes so the result is the token.
        canvas.vertexColorAlwaysGammaSpace = false;
        canvas.gameObject.layer = ui; // the board camera draws only the UI layer

        main.cullingMask = 0;
        main.clearFlags = CameraClearFlags.SolidColor;
        main.backgroundColor = Color.black;

        var presentGo = GameObject.Find("Board Present") ?? new GameObject("Board Present", typeof(RectTransform));
        presentGo.layer = ui;
        var pc = Get<Canvas>(presentGo);
        pc.renderMode = RenderMode.ScreenSpaceOverlay;
        var raw = presentGo.GetComponentInChildren<RawImage>();
        if (raw == null)
        {
            var rt = NewRect("Screen", presentGo.transform);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            raw = rt.gameObject.AddComponent<RawImage>();
        }
        raw.raycastTarget = false;
        raw.material = BoardMaterial();

        var presenter = Get<GammaBoardPresenter>(boardGo);
        var so = new SerializedObject(presenter);
        so.FindProperty("boardCamera").objectReferenceValue = cam;
        so.FindProperty("presentCamera").objectReferenceValue = main;
        so.FindProperty("screen").objectReferenceValue = raw;
        so.ApplyModifiedPropertiesWithoutUndo();
        presenter.enabled = false; presenter.enabled = true; // rebuild the texture with the new refs
    }

    // GetComponent can hand back Unity's fake null in the editor, which ?? does not catch.
    static T Get<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    static Material BoardMaterial()
    {
        const string path = Gen + "/BoardGammaToLinear.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Starwolf/UI/BoardGammaToLinear"));
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    // Our ship's live 3D view on the plot (Step 37): encodes the camera image the way the board stores colour.
    static Material ShipImageMaterial()
    {
        const string path = Gen + "/BoardLinearImage.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Starwolf/UI/BoardLinearImage"));
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    // ---- loading -------------------------------------------------------------------------

    static void Load()
    {
        layout = JObject.Parse(File.ReadAllText(Root + "/layout/layout.json"));
        tokens = (JObject)JObject.Parse(File.ReadAllText(Root + "/tokens.json"))["tokens"];
        plotGrid = JObject.Parse(File.ReadAllText(Root + "/textures/plot-grid.json"));
        assetMap = HandoffImport.ReadAssetMap();
        marginCache = new Dictionary<string, Vector4>();
        spriteGrow = new Dictionary<GameObject, Vector4>();
        report = new StringBuilder();
        counts = new Dictionary<string, int>();
        fonts = new Dictionary<string, TMP_FontAsset>();
        foreach (var n in new[] { "ShareTechMono-Regular", "BarlowCondensed-Regular", "BarlowCondensed-Medium", "BarlowCondensed-SemiBold" })
            fonts[n] = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Gen + "/Fonts/" + n + " SDF.asset");
    }

    static List<Rec> ReadRecords()
    {
        var all = new List<Rec>();
        var kinds = new (string section, string kind, int order)[]
            { ("textures", "texture", 0), ("sprites", "sprite", 1), ("drawn", "drawn", 2), ("lamps", "lamp", 4), ("icons", "icon", 5), ("text", "text", 6) };
        var icons = ((JArray)layout["icons"]).Select(Box).ToList();
        var compositions = FixedCompositions.Keys.Select(FixedRect).ToList();
        float px = (float)plotGrid["rect"]["x"], py = (float)plotGrid["rect"]["y"],
              pw = (float)plotGrid["rect"]["w"], ph = (float)plotGrid["rect"]["h"];

        foreach (var k in kinds)
        {
            var arr = (JArray)layout[k.section];
            for (int i = 0; i < arr.Count; i++)
            {
                var j = (JObject)arr[i];
                var r = new Rec { section = k.section, kind = k.kind, index = i, typeOrder = k.order, j = j, state = (string)j["state"], box = Box(j) };
                string label = (string)(j["sprite"] ?? j["texture"] ?? j["fill"]) ?? Short((string)j["text"]);
                r.name = k.kind + "#" + i + " " + label;

                if (k.section == "drawn")
                {
                    // Alpha-0 records are built as invisible pieces: some light up with state
                    // (e.g. the unselected spectrum-band rows). They draw nothing until then.
                    // Rail icon parts measured as rectangles; the icon sprites already carry them.
                    if (icons.Any(ic => Inside(r.box, ic, 4f))) { Count("skipped: drawn part of a rail icon"); continue; }
                    // Pipes, clamps and vents measured inside a conduit; the composition sprite carries them.
                    if (r.state == "default" && compositions.Any(cr => Inside(r.box, cr, 1f))) { Count("skipped: drawn part of a fixed composition"); continue; }
                    // The plot ground and glow are built from plot-grid.json instead.
                    if (Mathf.Abs(r.box.x - px) < 1 && Mathf.Abs(r.box.y - py) < 1 && Mathf.Abs(r.box.width - pw) < 1 && Mathf.Abs(r.box.height - ph) < 1)
                    { Count("replaced: plot ground/glow from plot-grid.json"); continue; }
                }
                if (r.state == "default") r.panel = PanelOf(k.section, i);
                all.Add(r);
            }
        }
        return all;
    }

    static string PanelOf(string section, int i)
    {
        foreach (var run in PanelRuns)
            if (run.section == section && i >= run.from && i <= run.to) return run.panel;
        Count("UNASSIGNED default record");
        report.AppendLine("unassigned: " + section + "#" + i);
        return "Unassigned";
    }

    // ---- default state -------------------------------------------------------------------

    static void BuildDefault(Group board, List<Rec> recs)
    {
        var plotRect = new Rect((float)plotGrid["rect"]["x"], (float)plotGrid["rect"]["y"], (float)plotGrid["rect"]["w"], (float)plotGrid["rect"]["h"]);
        var groups = new Dictionary<string, Group>();
        foreach (var panel in PanelOrder.Concat(recs.Any(r => r.panel == "Unassigned") ? new[] { "Unassigned" } : new string[0]))
        {
            if (panel == "Plot") { BuildPlot(board, plotRect, recs); continue; }
            Rect area = panel == "Periscope" ? PeriscopeHoused : new Rect(0, 0, 1920, 1080);
            var g = Child(board, panel, area, panel == "Periscope");
            Place(g, recs.Where(r => r.panel == panel).ToList());
        }
    }

    static void BuildPlot(Group board, Rect plot, List<Rec> recs)
    {
        // plot-grid.json rect shadow: "0 0 0 6px rgba(0,0,0,.35)" — a 6 px ring outside the plot.
        var ring = Child(board, "PlotRing", new Rect(plot.x - 6, plot.y - 6, plot.width + 12, plot.height + 12), false);
        Fill(ring.rt.gameObject, "black", 0.35f, "6px");

        var g = Child(board, "Plot", plot, true);
        var ground = Child(g, "Ground", plot, false);
        Fill(ground.rt.gameObject, (string)plotGrid["ground"]["token"], (float)plotGrid["ground"]["alpha"], (string)plotGrid["rect"]["radius"]);

        var grid = Child(g, "Grid", plot, false);
        var pg = grid.rt.gameObject.AddComponent<HandoffPlotGrid>();
        pg.raycastTarget = false;

        var white = plotGrid["tints"]["WHITE"];
        var glow = Child(g, "Glow", plot, false);
        Img(glow.rt.gameObject, ShapeSprite("plot_glow"), Hex((string)white["glow"]["token"], (float)white["glow"]["alpha"]), false);

        Place(Child(g, "Scale", plot, false), recs.Where(r => r.panel == "Plot/Scale").ToList());
        Place(Child(g, "Mockup", plot, false), recs.Where(r => r.panel == "Plot/Mockup").ToList());

        var tube = plotGrid["tube"];
        var scan = Child(g, "Scanlines", plot, false);
        var si = Img(scan.rt.gameObject, ShapeSprite("scanline"), Hex("black", (float)tube["scanlines"]["alphaAtDefault"]), false);
        si.type = Image.Type.Tiled;
        var vig = Child(g, "Vignette", plot, false);
        Img(vig.rt.gameObject, ShapeSprite("plot_vignette"), Hex("black", (float)tube["vignette"]["edgeAlpha"]), false);

        // plot-grid.json rect shadow: "inset 0 0 0 1px rgba(255,255,255,.13)" — a 1 px edge inside.
        var edge = Child(g, "Edge", plot, false);
        var c = Hex("white", 0.13f);
        EdgeLine(edge, "Top", new Rect(plot.x, plot.y, plot.width, 1), c);
        EdgeLine(edge, "Bottom", new Rect(plot.x, plot.yMax - 1, plot.width, 1), c);
        EdgeLine(edge, "Left", new Rect(plot.x, plot.y + 1, 1, plot.height - 2), c);
        EdgeLine(edge, "Right", new Rect(plot.xMax - 1, plot.y + 1, 1, plot.height - 2), c);
        Count("not built: plot inset shadow 0 4px 22px (no blur tool)");
    }

    static void EdgeLine(Group g, string name, Rect r, Color c)
    {
        var e = Child(g, name, r, false);
        Img(e.rt.gameObject, null, c, false);
    }

    static void AddReferenceOverlay(Group board)
    {
        var g = Child(board, "ReferenceOverlay (board-1920x1080.png)", new Rect(0, 0, 1920, 1080), false);
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/layout/board-1920x1080.png");
        Img(g.rt.gameObject, s, new Color(1, 1, 1, 0.5f), false);
        g.rt.gameObject.SetActive(false);
    }

    // A drawer, the open helm and orders station, and the lowered periscope are solid hardware:
    // clicks and the wheel on their empty faces must not fall through to the plot (Step 33b
    // verification). An invisible blocker over each body, first child so its controls sit above.
    static void AddBodyBlockers(RectTransform root)
    {
        foreach (var kv in (JObject)layout["regions"])
        {
            var r = kv.Value;
            Blocker((RectTransform)root.Find("State · " + kv.Key), new Rect((float)r["x"], (float)r["y"], (float)r["w"], (float)r["h"]), Vector2.zero);
        }
        // the housing as drawn when lowered (sprite periscope_housing, 750,44 420x580); the clip
        // hides it — and its raycasts — while housed
        Blocker((RectTransform)root.Find("Periscope"), new Rect(750, 44, 420, 580), PeriscopeHoused.position);
        // the stowed orders station also overlaps the plot's bottom edge (sprite#82, orders_body_closed)
        Blocker((RectTransform)root.Find("Orders"), new Rect(104, 936.5f, 392, 121.5f), Vector2.zero);
    }

    static void Blocker(RectTransform parent, Rect board, Vector2 origin)
    {
        var rt = NewRect("Block · body", parent);
        TopLeft(rt, board, origin);
        rt.SetAsFirstSibling();
        rt.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0); // raycast target, draws nothing
        Count("built: body blocker");
    }

    // ---- motion (Step 32) ----------------------------------------------------------------

    static readonly string[] DrawerStates = { "power", "sensors", "comms", "engineering", "hangar", "fire control" };

    static void WireMotion(RectTransform root)
    {
        var m = root.gameObject.AddComponent<HandoffBoardMotion>();
        var so = new SerializedObject(m);

        var list = so.FindProperty("drawers");
        list.arraySize = DrawerStates.Length;
        for (int i = 0; i < DrawerStates.Length; i++)
        {
            var g = (RectTransform)root.Find("State · " + DrawerStates[i]);
            var cg = g.gameObject.AddComponent<CanvasGroup>();
            var e = list.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("name").stringValue = DrawerStates[i];
            e.FindPropertyRelative("group").objectReferenceValue = g;
            e.FindPropertyRelative("fade").objectReferenceValue = cg;
        }

        so.FindProperty("periscopeClip").objectReferenceValue = root.Find("Periscope");
        so.FindProperty("scopeHousedOnly").objectReferenceValue = Named(root, "Periscope", "text#61 ").gameObject;
        so.FindProperty("scopeLoweredOnly").objectReferenceValue = root.Find("State · periscope").gameObject;

        so.FindProperty("helmClosed").objectReferenceValue = root.Find("Helm").gameObject;
        so.FindProperty("helmOpen").objectReferenceValue = root.Find("State · helm").gameObject;
        so.FindProperty("helmLatchClosed").objectReferenceValue = Named(root, "Helm", "sprite#95 ");
        so.FindProperty("helmLatchOpen").objectReferenceValue = Named(root, "State · helm", "sprite#327 ");

        so.FindProperty("ordersClosed").objectReferenceValue = root.Find("Orders").gameObject;
        so.FindProperty("ordersOpen").objectReferenceValue = root.Find("State · orders").gameObject;
        so.FindProperty("ordersLatchClosed").objectReferenceValue = Named(root, "Orders", "sprite#89 ");
        so.FindProperty("ordersLatchOpen").objectReferenceValue = Named(root, "State · orders", "sprite#352 ");

        // bd-blink on the alarm strip lamp ("while unacked") — the lamp and its glow.
        SetRefs(so.FindProperty("blinkers"),
            Named(root, "AlarmStrip", "lamp#1 ").GetComponent<Graphic>(),
            Named(root, "AlarmStrip", "glow · lamp#1 ").GetComponent<Graphic>());

        // bd-drift-b on the FTR-2 wing, always (UIStateSpec 9.2): translate(-12px, 7px), 21 s.
        var drifts = so.FindProperty("drifts");
        drifts.arraySize = 1;
        var drift = drifts.GetArrayElementAtIndex(0);
        SetRefs(drift.FindPropertyRelative("targets"),
            Named(root, "Plot/Mockup", "drawn#46 "), Named(root, "Plot/Mockup", "drawn#47 "),
            Named(root, "Plot/Mockup", "drawn#48 "), Named(root, "Plot/Mockup", "drawn#49 "));
        drift.FindPropertyRelative("offset").vector2Value = new Vector2(-12, 7);
        drift.FindPropertyRelative("seconds").floatValue = 21f;

        // bd-ghost on the predicted-position ghost and its label, always: opacity .38 <-> .62, 4 s.
        var pulses = so.FindProperty("pulses");
        pulses.arraySize = 1;
        var pulse = pulses.GetArrayElementAtIndex(0);
        SetRefs(pulse.FindPropertyRelative("targets"),
            Named(root, "Plot/Mockup", "drawn#54 ").GetComponent<Graphic>(),
            Named(root, "Plot/Mockup", "text#51 ").GetComponent<Graphic>(),
            Named(root, "Plot/Mockup", "text#52 ").GetComponent<Graphic>());
        pulse.FindPropertyRelative("from").floatValue = 0.38f;
        pulse.FindPropertyRelative("to").floatValue = 0.62f;
        pulse.FindPropertyRelative("seconds").floatValue = 4f;

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---- controls (Step 33a) -------------------------------------------------------------

    static void WireControls(RectTransform root, Canvas canvas)
    {
        var c = root.gameObject.AddComponent<HandoffBoardControls>();
        var so = new SerializedObject(c);
        so.FindProperty("motion").objectReferenceValue = root.GetComponent<HandoffBoardMotion>();
        so.FindProperty("grid").objectReferenceValue = root.Find("Plot/Grid").GetComponent<HandoffPlotGrid>();
        so.FindProperty("canvas").objectReferenceValue = canvas;
        // Step 40: one voice for the click cues
        var voice = root.gameObject.AddComponent<AudioSource>();
        voice.playOnAwake = false;
        so.FindProperty("voice").objectReferenceValue = voice;

        // Tool rail: keys SEL, PNT, RUL, CMP, ERS in rail order (UIStateSpec §8.1 lines 156–160).
        string[] tools = { "CURSOR", "POINT", "RULER", "COMPASS", "ERASE" };
        int[] toolKeys = { 20, 24, 27, 30, 33 }, toolLamps = { 2, 3, 4, 5, 6 };
        SetRefs(so.FindProperty("toolKeys"), toolKeys.Select(i => (Object)Named(root, "ToolRail", "sprite#" + i + " ")).ToArray());
        SetRefs(so.FindProperty("toolLamps"), toolLamps.Select(i => (Object)Gfx(root, "ToolRail", "lamp#" + i + " ")).ToArray());
        so.FindProperty("selectedFrame").objectReferenceValue = Named(root, "ToolRail", "sprite#22 ");
        so.FindProperty("selectedBar").objectReferenceValue = Named(root, "ToolRail", "drawn#26 ");
        for (int i = 0; i < tools.Length; i++) Hit(Named(root, "ToolRail", "sprite#" + toolKeys[i] + " "), "tool", tools[i]);

        // Systems rail: FIRE, SENS, PWR, HGR, ENGR, COMM (lines 397–402) and each drawer's latch.
        string[] drawers = { "fire control", "sensors", "power", "hangar", "engineering", "comms" };
        int[] sysKeys = { 37, 40, 43, 46, 49, 52 }, sysLamps = { 13, 15, 17, 18, 19, 20 };
        SetRefs(so.FindProperty("systemLamps"), sysLamps.Select(i => (Object)Gfx(root, "SystemsRail", "lamp#" + i + " ")).ToArray());
        for (int i = 0; i < drawers.Length; i++)
        {
            Hit(Named(root, "SystemsRail", "sprite#" + sysKeys[i] + " "), "drawer", drawers[i]);
            foreach (Transform t in root.Find("State · " + drawers[i]))
                if (t.name.EndsWith(" latch")) Hit((RectTransform)t, "drawer", drawers[i]);
        }

        // Time control (lines 130, 138) and the alarm strip (line 146).
        so.FindProperty("clock").objectReferenceValue = Named(root, "TimeControl", "text#7 ").GetComponent<TMPro.TMP_Text>();
        so.FindProperty("pauseKey").objectReferenceValue = Gfx(root, "TimeControl", "sprite#13 ");
        so.FindProperty("pauseKeyRun").objectReferenceValue = LoadSprite("pause_key_run");
        so.FindProperty("pauseKeyPaused").objectReferenceValue = LoadSprite("pause_key_paused");
        so.FindProperty("pauseLabel").objectReferenceValue = Named(root, "TimeControl", "text#8 ").GetComponent<TMPro.TMP_Text>();
        so.FindProperty("pauseLamp").objectReferenceValue = Gfx(root, "TimeControl", "lamp#0 ");
        so.FindProperty("pauseGlow").objectReferenceValue = Gfx(root, "TimeControl", "glow · lamp#0 ");
        Hit(Named(root, "TimeControl", "sprite#13 "), "pause", null);
        string[] rateArgs = { "0.25", "1", "5", "10" };
        int[] rateKeys = { 14, 15, 16, 17 }, rateLabels = { 10, 11, 12, 13 };
        SetRefs(so.FindProperty("rateKeys"), rateKeys.Select(i => (Object)Gfx(root, "TimeControl", "sprite#" + i + " ")).ToArray());
        SetRefs(so.FindProperty("rateLabels"), rateLabels.Select(i => (Object)Named(root, "TimeControl", "text#" + i + " ").GetComponent<TMPro.TMP_Text>()).ToArray());
        so.FindProperty("rateKeyOn").objectReferenceValue = LoadSprite("rate_key_on");
        so.FindProperty("rateKeyOff").objectReferenceValue = LoadSprite("rate_key_off");
        for (int i = 0; i < rateKeys.Length; i++) Hit(Named(root, "TimeControl", "sprite#" + rateKeys[i] + " "), "rate", rateArgs[i]);
        SetRefs(so.FindProperty("alarmParts"),
            Named(root, "AlarmStrip", "sprite#18 ").gameObject, Named(root, "AlarmStrip", "lamp#1 ").gameObject,
            Named(root, "AlarmStrip", "glow · lamp#1 ").gameObject, Named(root, "AlarmStrip", "text#14 ").gameObject,
            Named(root, "AlarmStrip", "text#15 ").gameObject);
        Hit(Named(root, "AlarmStrip", "sprite#18 "), "ack", null);
        so.FindProperty("alarmCount").objectReferenceValue = Tmp(root, "AlarmStrip", "text#15 ");
        so.FindProperty("alarmText").objectReferenceValue = Tmp(root, "AlarmStrip", "text#14 ");
        // Owner, 2026-10-18: the strip shows only for a real alert; the board loads quiet (already acked).
        // ponytail: hidden at build; a real alert needs a show path (sensors step, new contact identified)
        var alarm = so.FindProperty("alarmParts");
        for (int i = 0; i < alarm.arraySize; i++) ((GameObject)alarm.GetArrayElementAtIndex(i).objectReferenceValue).SetActive(false);

        // Periscope carriage (line 1318), helm header (1862), orders header (1757).
        Hit(Named(root, "Periscope", "sprite#55 "), "scope", null);
        Hit(Named(root, "Helm", "sprite#93 "), "helm", null);
        Hit(Named(root, "State · helm", "sprite#325 "), "helm", null);
        // The orders header is the strip above the alert-condition well: body top to well top,
        // 31 px in the closed layout (936.5 to 967.5); the same strip in the open layout.
        HeaderHit(root, "Orders", new Rect(104, 936.5f, 392, 31));
        HeaderHit(root, "State · orders", new Rect(104, 790.5f, 392, 31));

        // Console: SCALE knob (line 1721, drag) and grid tint selector (line 1735, click).
        var scaleKnob = Named(root, "Console", "sprite#79 ");
        so.FindProperty("scaleDial").FindPropertyRelative("pointer").objectReferenceValue = Named(root, "Console", "sprite#377 ");
        so.FindProperty("scaleDial").FindPropertyRelative("centre").vector2Value = scaleKnob.anchoredPosition;
        so.FindProperty("scaleDial").FindPropertyRelative("homePosition").vector2Value = Named(root, "Console", "sprite#377 ").anchoredPosition;
        so.FindProperty("scaleDial").FindPropertyRelative("homeAngle").floatValue = 140f;
        so.FindProperty("zoomLabel").objectReferenceValue = Named(root, "Console", "text#80 ").GetComponent<TMPro.TMP_Text>();
        Hit(scaleKnob, "zoom", null);
        var tintKnob = Named(root, "Console", "sprite#81 ");
        so.FindProperty("tintDial").FindPropertyRelative("pointer").objectReferenceValue = Named(root, "Console", "sprite#380 ");
        so.FindProperty("tintDial").FindPropertyRelative("centre").vector2Value = tintKnob.anchoredPosition;
        so.FindProperty("tintDial").FindPropertyRelative("homePosition").vector2Value = Named(root, "Console", "sprite#380 ").anchoredPosition;
        so.FindProperty("tintDial").FindPropertyRelative("homeAngle").floatValue = 60f;
        EaseDial(so.FindProperty("tintDial"), 0.2f, Spring); // line 1739
        so.FindProperty("tintLampWhite").objectReferenceValue = Gfx(root, "Console", "lamp#25 ");
        so.FindProperty("tintLampAmber").objectReferenceValue = Gfx(root, "Console", "lamp#26 ");
        so.FindProperty("tintLabelWhite").objectReferenceValue = Named(root, "Console", "text#81 ").GetComponent<TMPro.TMP_Text>();
        so.FindProperty("tintLabelAmber").objectReferenceValue = Named(root, "Console", "text#82 ").GetComponent<TMPro.TMP_Text>();
        Hit(tintKnob, "tint", null);

        // The plot takes the wheel (§8.3), and a drag on the empty grid pans it (Step 41, owner
        // 2026-10-18). It lies under every mark, label and the marking tools' capture.
        Hit((RectTransform)root.Find("Plot/Ground"), "plot.pan", null);

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---- Power drawer (Step 33b) ----------------------------------------------------------

    static void WirePower(RectTransform root)
    {
        const string P = "State · power";
        var p = root.gameObject.AddComponent<HandoffPowerPanel>();
        var so = new SerializedObject(p);
        so.FindProperty("loadLabel").objectReferenceValue = Tmp(root, P, "text#101 ");
        so.FindProperty("reactorLabel").objectReferenceValue = Tmp(root, P, "text#103 ");
        // r1..r10: the ink.data face of each unit cell, left to right (drawn #87, #89 … #105)
        SetRefs(so.FindProperty("reactorCells"), Enumerable.Range(0, 10).Select(i => (Object)Gfx(root, P, "drawn#" + (87 + 2 * i) + " ")).ToArray());
        SetRefs(so.FindProperty("coreCells"), Enumerable.Range(31, 5).Select(i => (Object)Gfx(root, P, "lamp#" + i + " ")).ToArray());
        SetRefs(so.FindProperty("coreGlows"), Enumerable.Range(31, 5).Select(i => (Object)Gfx(root, P, "glow · lamp#" + i + " ")).ToArray());
        so.FindProperty("coreHealthLabel").objectReferenceValue = Tmp(root, P, "text#105 ");

        so.FindProperty("shedRocker").objectReferenceValue = Gfx(root, P, "sprite#110 ");
        so.FindProperty("rockerUp").objectReferenceValue = LoadSprite("rocker_30x38_up");
        so.FindProperty("rockerThrown").objectReferenceValue = LoadSprite("rocker_30x38_thrown");
        so.FindProperty("shedLamp").objectReferenceValue = Gfx(root, P, "lamp#36 ");
        so.FindProperty("shedGlow").objectReferenceValue = Gfx(root, P, "glow · lamp#36 ");
        so.FindProperty("shedNote").objectReferenceValue = Tmp(root, P, "text#107 ");
        Hit(Named(root, P, "sprite#110 "), "power.shed", null);

        // Bus columns ENG, WEP, SEN, AUX: five cells each (drawn, listed top to bottom), the units
        // and mode readouts, the mode lamp, and the rotary selector with its pointer.
        int[] firstCell = { 106, 112, 118, 124 }, units = { 108, 112, 116, 120 }, modes = { 110, 114, 118, 122 };
        int[] lamps = { 37, 38, 39, 40 }, knobs = { 117, 123, 129, 135 }, pointers = { 388, 392, 396, 400 };
        var cols = so.FindProperty("columns");
        cols.arraySize = 4;
        for (int c = 0; c < 4; c++)
        {
            var e = cols.GetArrayElementAtIndex(c);
            // bottom cell first: s1 is the lowest (see HANDOFF_GAPS — the package does not say)
            SetRefs(e.FindPropertyRelative("cells"), Enumerable.Range(0, 5).Select(k => (Object)Gfx(root, P, "drawn#" + (firstCell[c] + 4 - k) + " ")).ToArray());
            e.FindPropertyRelative("units").objectReferenceValue = Tmp(root, P, "text#" + units[c] + " ");
            e.FindPropertyRelative("mode").objectReferenceValue = Tmp(root, P, "text#" + modes[c] + " ");
            e.FindPropertyRelative("lamp").objectReferenceValue = Gfx(root, P, "lamp#" + lamps[c] + " ");
            e.FindPropertyRelative("lampGlow").objectReferenceValue = Gfx(root, P, "glow · lamp#" + lamps[c] + " ");
            var knob = Named(root, P, "sprite#" + knobs[c] + " ");
            var ptr = Named(root, P, "sprite#" + pointers[c] + " ");
            var dial = e.FindPropertyRelative("selector");
            dial.FindPropertyRelative("pointer").objectReferenceValue = ptr;
            dial.FindPropertyRelative("centre").vector2Value = knob.anchoredPosition;
            dial.FindPropertyRelative("homePosition").vector2Value = ptr.anchoredPosition;
            dial.FindPropertyRelative("homeAngle").floatValue = 0f; // laid out at STANDBY
            EaseDial(dial, 0.18f, Spring); // bus columns, line 592
            Hit(knob, "power.bus", c.ToString());
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---- Sensors drawer (Step 33c) --------------------------------------------------------

    // Contact board rows: only one row was measured. Further rows use the spectrum rows' pitch in
    // the same drawer (drawn #144–147, 24.5 px) — the package does not give the board's (HANDOFF_GAPS).
    const float ContactRowPitch = 24.5f;
    const int ContactRows = 3; // hostile, non-own TRACKS: the most the board can list

    static void WireSensors(RectTransform root)
    {
        const string S = "State · sensors";
        var group = (RectTransform)root.Find(S);
        var p = root.gameObject.AddComponent<HandoffSensorsPanel>();
        var so = new SerializedObject(p);

        System.Action<string, string, int> master = (prop, sprite, lamp) =>
        {
            var m = so.FindProperty(prop);
            m.FindPropertyRelative("rocker").objectReferenceValue = Gfx(root, S, sprite);
            m.FindPropertyRelative("lamp").objectReferenceValue = Gfx(root, S, "lamp#" + lamp + " ");
            m.FindPropertyRelative("glow").objectReferenceValue = Gfx(root, S, "glow · lamp#" + lamp + " ");
        };
        master("dradis", "sprite#145 ", 41);
        master("active", "sprite#147 ", 42);
        so.FindProperty("rockerUp").objectReferenceValue = LoadSprite("rocker_30x38_up");
        so.FindProperty("rockerThrown").objectReferenceValue = LoadSprite("rocker_30x38_thrown");
        Hit(Named(root, S, "sprite#145 "), "sensors.dradis", null);
        Hit(Named(root, S, "sprite#147 "), "sensors.active", null);

        so.FindProperty("sigPct").objectReferenceValue = Tmp(root, S, "text#132 ");
        so.FindProperty("sigNote").objectReferenceValue = Tmp(root, S, "text#133 ");
        SetRefs(so.FindProperty("sigCells"), Enumerable.Range(131, 10).Select(i => (Object)Gfx(root, S, "drawn#" + i + " ")).ToArray());
        so.FindProperty("sigLamp").objectReferenceValue = Gfx(root, S, "lamp#43 ");
        so.FindProperty("sigGlow").objectReferenceValue = Gfx(root, S, "glow · lamp#43 ");

        so.FindProperty("rangeHeader").objectReferenceValue = Tmp(root, S, "text#126 ");
        so.FindProperty("rangeEnvelope").objectReferenceValue = Tmp(root, S, "text#144 ");
        // the plot's "SENSOR RANGE / 19,000 KM" label rides the envelope ring (senCapTop, senRangeLabel)
        SetRefs(so.FindProperty("envelopeLabel"), Named(root, "Plot/Mockup", "text#37 "), Named(root, "Plot/Mockup", "text#38 "));
        so.FindProperty("envelopeRange").objectReferenceValue = Tmp(root, "Plot/Mockup", "text#38 ");
        so.FindProperty("gainValue").objectReferenceValue = Tmp(root, S, "text#141 ");
        so.FindProperty("boostCost").objectReferenceValue = Tmp(root, S, "text#145 ");
        // a two-line box whose text the panel rewrites: wrap inside the measured width
        Tmp(root, S, "text#145 ").textWrappingMode = TMPro.TextWrappingModes.Normal;
        so.FindProperty("gainCap").objectReferenceValue = Named(root, S, "sprite#153 ");
        so.FindProperty("gainTrack").objectReferenceValue = Named(root, S, "drawn#141 ");
        Hit(Named(root, S, "sprite#152 "), "sensors.gain", null);

        string[] bands = { "THERM", "RAD", "UV", "EM" };
        var bandProp = so.FindProperty("bands");
        bandProp.arraySize = 4;
        for (int b = 0; b < 4; b++)
        {
            var e = bandProp.GetArrayElementAtIndex(b);
            var bg = Named(root, S, "drawn#" + (144 + b) + " ");
            e.FindPropertyRelative("bg").objectReferenceValue = bg.GetComponent<Image>();
            e.FindPropertyRelative("name").objectReferenceValue = Tmp(root, S, "text#" + (148 + 2 * b) + " ");
            e.FindPropertyRelative("count").objectReferenceValue = Tmp(root, S, "text#" + (149 + 2 * b) + " ");
            e.FindPropertyRelative("lamp").objectReferenceValue = Gfx(root, S, "lamp#" + (44 + b) + " ");
            e.FindPropertyRelative("glow").objectReferenceValue = Gfx(root, S, "glow · lamp#" + (44 + b) + " ");
            Hit(bg, "sensors.band", bands[b]);
        }

        // the cone dial: needle (drawn #149) turns about the gauge centre; the cone wedge replaces
        // the gradient stand-in (drawn #148) on the round well
        var gauge = Named(root, S, "sprite#157 ");
        var needle = Named(root, S, "drawn#149 ");
        var nd = so.FindProperty("needle");
        nd.FindPropertyRelative("pointer").objectReferenceValue = needle;
        nd.FindPropertyRelative("centre").vector2Value = gauge.anchoredPosition;
        nd.FindPropertyRelative("homePosition").vector2Value = needle.anchoredPosition;
        nd.FindPropertyRelative("homeAngle").floatValue = -118f; // coneNeedle at the initial bearing 062
        var standIn = Named(root, S, "drawn#148 ");
        standIn.gameObject.SetActive(false);
        var wedgeRt = NewRect("Cone (HandoffWedge)", group);
        TopLeft(wedgeRt, new Rect(1473, 577, 64, 64), Vector2.zero);
        wedgeRt.SetSiblingIndex(standIn.GetSiblingIndex() + 1);
        var wedge = wedgeRt.gameObject.AddComponent<HandoffWedge>();
        wedge.color = new Color(143 / 255f, 195 / 255f, 58 / 255f, 0.2f); // coneInk
        wedge.raycastTarget = false;
        so.FindProperty("cone").objectReferenceValue = wedge;
        Hit(gauge, "sensors.cone", null);
        so.FindProperty("coneBearing").objectReferenceValue = Tmp(root, S, "text#159 ");
        so.FindProperty("coneArcLabel").objectReferenceValue = Tmp(root, S, "text#163 ");
        // "ARC °" wraps the value run (text#163); the "°" belongs after the value, which the
        // panel now writes, so the wrapper keeps only "ARC" (Step 33c verification, D2)
        Tmp(root, S, "text#162 ").text = "ARC";
        so.FindProperty("emitLabel").objectReferenceValue = Tmp(root, S, "text#164 ");
        Hit(Named(root, S, "sprite#160 "), "sensors.narrow", null);
        Hit(Named(root, S, "sprite#161 "), "sensors.widen", null);

        // contact board: the measured row, copied down for the others; a row background per row
        so.FindProperty("bogeyCount").objectReferenceValue = Tmp(root, S, "text#170 ");
        var template = new[] { Named(root, S, "text#171 "), Named(root, S, "text#172 "), Named(root, S, "text#173 "),
                               Named(root, S, "text#174 "), Named(root, S, "lamp#49 "), Named(root, S, "glow · lamp#49 ") };
        int firstIndex = template.Min(t => t.GetSiblingIndex());
        var rowsProp = so.FindProperty("rows");
        rowsProp.arraySize = ContactRows;
        for (int i = 0; i < ContactRows; i++)
        {
            var parts = i == 0 ? template : template.Select(t =>
            {
                var c = (RectTransform)Object.Instantiate(t.gameObject, group).transform;
                c.name = t.name + " (row " + (i + 1) + ")";
                c.anchoredPosition += new Vector2(0, -ContactRowPitch * i);
                return c;
            }).ToArray();
            var bg = NewRect("Contact row " + (i + 1) + " (bg)", group);
            TopLeft(bg, new Rect(1458, 743 + ContactRowPitch * i, 342, 19.5f), Vector2.zero);
            bg.SetSiblingIndex(firstIndex);
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0);
            Hit(bg, "sensors.identify", i.ToString());
            var e = rowsProp.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("bg").objectReferenceValue = bgImg;
            e.FindPropertyRelative("name").objectReferenceValue = parts[0].GetComponent<TMPro.TMP_Text>();
            e.FindPropertyRelative("brg").objectReferenceValue = parts[1].GetComponent<TMPro.TMP_Text>();
            e.FindPropertyRelative("rng").objectReferenceValue = parts[2].GetComponent<TMPro.TMP_Text>();
            e.FindPropertyRelative("klass").objectReferenceValue = parts[3].GetComponent<TMPro.TMP_Text>();
            e.FindPropertyRelative("dot").objectReferenceValue = parts[4].GetComponent<Image>();
            e.FindPropertyRelative("dotGlow").objectReferenceValue = parts[5].GetComponent<Image>();
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---- Comms drawer (Step 33d) ----------------------------------------------------------

    static void SetDial(SerializedProperty dial, RectTransform pointer, RectTransform about, float homeAngle)
    {
        dial.FindPropertyRelative("pointer").objectReferenceValue = pointer;
        dial.FindPropertyRelative("centre").vector2Value = about.anchoredPosition;
        dial.FindPropertyRelative("homePosition").vector2Value = pointer.anchoredPosition;
        dial.FindPropertyRelative("homeAngle").floatValue = homeAngle;
    }

    // ---- control transitions (Step 33j, UIStateSpec §9.1) ---------------------------------

    // the selector spring every knob pointer uses: .2s cubic-bezier(.3,1.5,.5,1)
    static readonly CssEase Spring = new CssEase(0.3f, 1.5f, 0.5f, 1f);

    static void Ease(SerializedProperty eased, float seconds, CssEase ease)
    {
        eased.FindPropertyRelative("seconds").floatValue = seconds;
        var e = eased.FindPropertyRelative("ease");
        e.FindPropertyRelative("x1").floatValue = ease.x1; e.FindPropertyRelative("y1").floatValue = ease.y1;
        e.FindPropertyRelative("x2").floatValue = ease.x2; e.FindPropertyRelative("y2").floatValue = ease.y2;
        Count("built: control transition");
    }

    static void EaseDial(SerializedProperty dial, float seconds, CssEase ease) => Ease(dial.FindPropertyRelative("motion"), seconds, ease);

    static void WireComms(RectTransform root)
    {
        const string C = "State · comms";
        var p = root.gameObject.AddComponent<HandoffCommsPanel>();
        var so = new SerializedObject(p);
        so.FindProperty("commsState").objectReferenceValue = Tmp(root, C, "text#178 ");

        // band selector: the selector face is drawn #152, its knob drawn #156, pointer drawn #157
        // laid out at band 1 = chanAngle -40
        SetDial(so.FindProperty("bandPointer"), Named(root, C, "drawn#157 "), Named(root, C, "drawn#156 "), -40f);
        EaseDial(so.FindProperty("bandPointer"), 0.2f, Spring); // line 843
        SetRefs(so.FindProperty("bandLamps"), Enumerable.Range(50, 3).Select(i => (Object)Gfx(root, C, "lamp#" + i + " ")).ToArray());
        // only lamps the layout gives a glow have one (lamp #50 here)
        SetRefs(so.FindProperty("bandGlows"), Enumerable.Range(50, 3).Select(i => (Object)GlowOf(root, C, i)).ToArray());
        SetRefs(so.FindProperty("bandLabels"), Enumerable.Range(180, 3).Select(i => (Object)Tmp(root, C, "text#" + i + " ")).ToArray());
        Hit(Named(root, C, "drawn#152 "), "comms.band", null);

        // tuning: knob sprite #176; its indicator drawn #217 (tuneAngle -50 at 34.0); the ring
        // marker drawn #216 on the scope glass #175 (freqBearing 120 at 34.0)
        var knob = Named(root, C, "sprite#176 ");
        so.FindProperty("freqLabel").objectReferenceValue = Tmp(root, C, "text#185 ");
        SetDial(so.FindProperty("tuneIndicator"), Named(root, C, "drawn#217 "), knob, -50f);
        SetDial(so.FindProperty("ringMarker"), Named(root, C, "drawn#216 "), Named(root, C, "sprite#175 "), 120f);
        Hit(knob, "comms.tune", null);

        // the 56 receiver bars, drawn #160–215 in order k = 0..55
        SetRefs(so.FindProperty("bars"), Enumerable.Range(160, 56).Select(i => (Object)Named(root, C, "drawn#" + i + " ")).ToArray());

        so.FindProperty("lockRocker").objectReferenceValue = Gfx(root, C, "sprite#178 ");
        so.FindProperty("rockerUp").objectReferenceValue = LoadSprite("rocker_30x38_up");
        so.FindProperty("rockerLocked").objectReferenceValue = LoadSprite("rocker_30x38_locked");
        so.FindProperty("rockerThrown").objectReferenceValue = LoadSprite("rocker_30x38_thrown");
        so.FindProperty("lockLamp").objectReferenceValue = Gfx(root, C, "lamp#53 ");
        so.FindProperty("lockGlow").objectReferenceValue = GlowOf(root, C, 53);
        so.FindProperty("sigLamp").objectReferenceValue = Gfx(root, C, "lamp#54 ");
        so.FindProperty("sigGlow").objectReferenceValue = GlowOf(root, C, 54);
        so.FindProperty("tuneNote").objectReferenceValue = Tmp(root, C, "text#188 ");
        Hit(Named(root, C, "sprite#178 "), "comms.lock", null);

        // decrypt: 59 letter runs in message order (text #191–249), four key dials
        so.FindProperty("solvedLabel").objectReferenceValue = Tmp(root, C, "text#190 ");
        SetRefs(so.FindProperty("letters"), Enumerable.Range(191, 59).Select(i => (Object)Tmp(root, C, "text#" + i + " ")).ToArray());
        int[] knobs = { 185, 187, 189, 191 }, pointers = { 401, 402, 403, 404 }, keyLetters = { 250, 251, 252, 253 }, boxes = { 219, 220, 221, 222 };
        var kd = so.FindProperty("keyDials");
        kd.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            var e = kd.GetArrayElementAtIndex(i);
            var k = Named(root, C, "sprite#" + knobs[i] + " ");
            SetDial(e.FindPropertyRelative("pointer"), Named(root, C, "sprite#" + pointers[i] + " "), k, 0f); // laid out at A
            EaseDial(e.FindPropertyRelative("pointer"), 0.14f, CssEase.Ease); // key dials, line 929
            e.FindPropertyRelative("letter").objectReferenceValue = Tmp(root, C, "text#" + keyLetters[i] + " ");
            e.FindPropertyRelative("box").objectReferenceValue = Gfx(root, C, "drawn#" + boxes[i] + " ");
            Hit(k, "comms.dial", i.ToString());
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---- Engineering drawer (Step 33e) ----------------------------------------------------

    // Compartment rows sit 40 px apart (the six measured ring records, drawn #237–249). Only the
    // selected row's background was measured (drawn #245, the reactor row): every row gets one.
    const float CompRowPitch = 40f;

    static void WireEngineering(RectTransform root)
    {
        const string E = "State · engineering";
        var group = (RectTransform)root.Find(E);
        var p = root.gameObject.AddComponent<HandoffEngineeringPanel>();
        var so = new SerializedObject(p);
        string[] keys = { "mag", "bri", "hab", "hgr", "rct", "eng" };
        int[] rings = { 237, 239, 241, 243, 246, 248 };
        Named(root, E, "drawn#245 ").gameObject.SetActive(false); // replaced by a background per row

        // Step 50 (owner 2026-10-23): our ship's top-down picture replaces the design's line-and-blocks
        // diagram (#230 line, #231–#236 blocks) on its dark box (#228/#229, kept), bow up, a lamp per
        // compartment. The panel sizes and turns the picture and colours the lamps.
        for (int d = 230; d <= 236; d++) Named(root, E, "drawn#" + d + " ").gameObject.SetActive(false);
        var box = Named(root, E, "drawn#229 ");
        var diagram = NewRect("Ship diagram", group);
        TopLeft(diagram, new Rect(1477, 337, 112, 199), Vector2.zero);
        diagram.SetSiblingIndex(box.GetSiblingIndex() + 1);
        var pic = NewRect("Ship (bow up)", diagram);
        pic.anchorMin = pic.anchorMax = pic.pivot = new Vector2(0.5f, 0.5f);
        pic.anchoredPosition = Vector2.zero;
        pic.sizeDelta = new Vector2(199, 199);
        var picImg = pic.gameObject.AddComponent<RawImage>();
        picImg.material = ShipImageMaterial();
        picImg.raycastTarget = false;
        picImg.enabled = false; // no texture until play (a RawImage with none draws white)
        so.FindProperty("shipPicture").objectReferenceValue = picImg;
        var lampSprite = LoadSprite("lamp_dot");
        var lampList = new List<Object>();
        foreach (var k in new[] { "bri", "mag", "hab", "hgr", "rct", "eng" })
        {
            var lr = NewRect("Lamp · " + k, diagram);
            lr.anchorMin = lr.anchorMax = lr.pivot = new Vector2(0.5f, 0.5f);
            lr.sizeDelta = new Vector2(8, 8);
            lampList.Add(Img(lr.gameObject, lampSprite, Hex("accent.nominal", 1f), false));
        }
        SetRefs(so.FindProperty("compartmentLamps"), lampList.ToArray());
        Count("built: engineering ship diagram");
        var rowsProp = so.FindProperty("rows");
        rowsProp.arraySize = keys.Length;
        for (int r = 0; r < keys.Length; r++)
        {
            var e = rowsProp.GetArrayElementAtIndex(r);
            e.FindPropertyRelative("key").stringValue = keys[r];
            var cell = Named(root, E, "drawn#" + (231 + r) + " ");
            e.FindPropertyRelative("cell").objectReferenceValue = cell.GetComponent<Image>();
            Hit(cell, "eng.pick", keys[r]);

            var standIn = Named(root, E, "drawn#" + rings[r] + " ");
            int at = standIn.GetSiblingIndex();
            standIn.gameObject.SetActive(false);

            var bg = NewRect("Compartment row " + keys[r] + " (bg)", group);
            TopLeft(bg, new Rect(1589, 317 + CompRowPitch * r, 162, 38), Vector2.zero);
            bg.SetSiblingIndex(at);
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.sprite = ShapeSprite("round_2");
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0, 0, 0, 0);
            Hit(bg, "eng.pick", keys[r]);
            e.FindPropertyRelative("rowBg").objectReferenceValue = bgImg;

            var ring = NewRect("Ring " + keys[r] + " (HandoffWedge x5)", group);
            TopLeft(ring, new Rect(1599, 319 + CompRowPitch * r, 34, 34), Vector2.zero);
            ring.SetSiblingIndex(bg.GetSiblingIndex() + 1);
            var segs = new Object[5];
            for (int k = 0; k < 5; k++)
            {
                var seg = NewRect("Segment " + (k + 1), ring);
                seg.anchorMin = Vector2.zero; seg.anchorMax = Vector2.one; seg.offsetMin = seg.offsetMax = Vector2.zero;
                var w = seg.gameObject.AddComponent<HandoffWedge>();
                w.raycastTarget = false;
                segs[k] = w;
            }
            SetRefs(e.FindPropertyRelative("ring"), segs);
            e.FindPropertyRelative("tag").objectReferenceValue = Tmp(root, E, "text#" + (256 + 7 * r) + " ");
            e.FindPropertyRelative("atmo").objectReferenceValue = Tmp(root, E, "text#" + (258 + 7 * r) + " ");
            e.FindPropertyRelative("cond").objectReferenceValue = Tmp(root, E, "text#" + (260 + 7 * r) + " ");
            e.FindPropertyRelative("crew").objectReferenceValue = Tmp(root, E, "text#" + (262 + 7 * r) + " ");
        }
        so.FindProperty("hazardLine").objectReferenceValue = Tmp(root, E, "text#298 ");
        so.FindProperty("degradedLine").objectReferenceValue = Tmp(root, E, "text#299 ");
        so.FindProperty("partyLine").objectReferenceValue = Tmp(root, E, "text#300 ");
        // the right-hand part of the status row: a longer line ("DC PARTY ▸ HANGAR BAY 1 OF 6")
        // grows leftward from the screen edge rather than past it (HANDOFF_GAPS — no template)
        Tmp(root, E, "text#300 ").alignment = TMPro.TextAlignmentOptions.Right;
        so.FindProperty("compName").objectReferenceValue = Tmp(root, E, "text#302 ");
        so.FindProperty("engNote").objectReferenceValue = Tmp(root, E, "text#303 ");
        so.FindProperty("compLamp").objectReferenceValue = Gfx(root, E, "lamp#55 ");
        so.FindProperty("compGlow").objectReferenceValue = GlowOf(root, E, 55);
        var knob = Named(root, E, "sprite#202 ");
        SetDial(so.FindProperty("actingOn"), Named(root, E, "sprite#411 "), knob, 120f); // reactor, index 4
        EaseDial(so.FindProperty("actingOn"), 0.2f, Spring); // line 1030
        Hit(knob, "eng.cycle", null);

        // Step 59 (owner 2026-10-24): DUMMY 1 becomes RECALL (calls the damage-control team back), with the
        // REPAIR / SEAL / VENT rocker art and its own legend. DUMMY 2 stays a blanking plate until a step needs it.
        string[] ids = { "reboot", "party", "seal", "vent", "recall", "dum2" };
        int[] rockers = { 206, 209, 212, 215, 218, 221 };
        var rp = so.FindProperty("repairs");
        rp.arraySize = ids.Length;
        for (int i = 0; i < ids.Length; i++)
        {
            var e = rp.GetArrayElementAtIndex(i);
            bool fitted = i < 5; // DUMMY 2 is a blanking plate (tbd)
            string family = i == 0 ? "rocker_reboot" : "rocker_22x30";
            e.FindPropertyRelative("id").stringValue = ids[i];
            e.FindPropertyRelative("fitted").boolValue = fitted;
            var rocker = Named(root, E, "sprite#" + rockers[i] + " ");
            if (ids[i] == "recall")
            {
                rocker.GetComponent<Image>().sprite = LoadSprite(family + "_up");
                Tmp(root, E, "text#" + (304 + i) + " ").text = "RECALL";
            }
            e.FindPropertyRelative("rocker").objectReferenceValue = rocker.GetComponent<Image>();
            e.FindPropertyRelative("up").objectReferenceValue = LoadSprite(family + "_up");
            e.FindPropertyRelative("locked").objectReferenceValue = LoadSprite(family + "_locked");
            e.FindPropertyRelative("thrown").objectReferenceValue = LoadSprite(family + "_thrown");
            e.FindPropertyRelative("lamp").objectReferenceValue = Gfx(root, E, "lamp#" + (56 + i) + " ");
            e.FindPropertyRelative("glow").objectReferenceValue = GlowOf(root, E, 56 + i);
            e.FindPropertyRelative("label").objectReferenceValue = Tmp(root, E, "text#" + (304 + i) + " ");
            if (fitted) Hit(rocker, "eng.run", ids[i]);
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---- Hangar drawer (Step 33f) ---------------------------------------------------------

    // An invisible marker at a layout record's element rect (no shadow margin), centre pivot.
    static RectTransform Marker(RectTransform group, string name, Rect board)
    {
        var rt = NewRect(name, group);
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = board.size;
        rt.anchoredPosition = new Vector2(board.center.x, -board.center.y);
        return rt;
    }

    // A plain fill image at a layout rect, centre pivot, inserted just after `after`.
    static RectTransform FillAt(RectTransform group, string name, Rect board, Transform after, Color design, float alpha)
    {
        var rt = Marker(group, name, board);
        rt.SetSiblingIndex(after.GetSiblingIndex() + 1);
        var img = rt.gameObject.AddComponent<Image>();
        var c = design.gamma; c.a = alpha; img.color = c;
        img.raycastTarget = false;
        return rt;
    }

    static Rect ElementRect(string section, int index)
    {
        var j = layout[section][index];
        return new Rect((float)j["x"], (float)j["y"], (float)j["w"], (float)j["h"]);
    }

    static void WireHangar(RectTransform root)
    {
        const string H = "State · hangar";
        var group = (RectTransform)root.Find(H);
        var p = root.gameObject.AddComponent<HandoffHangarPanel>();
        var so = new SerializedObject(p);
        Color nominal = Hex("accent.nominal", 1f).linear; // design sRGB (Hex pre-encodes; undo for FillAt)

        so.FindProperty("ready").objectReferenceValue = Tmp(root, H, "text#311 ");
        so.FindProperty("headerScreen").objectReferenceValue = Named(root, H, "sprite#227 ");
        var doorKnob = Named(root, H, "sprite#230 ");
        SetDial(so.FindProperty("doorSelector"), Named(root, H, "sprite#414 "), doorKnob, 60f); // sealed
        EaseDial(so.FindProperty("doorSelector"), 0.2f, Spring); // line 1108
        Hit(doorKnob, "hangar.doors", null);
        so.FindProperty("doorLamp").objectReferenceValue = Gfx(root, H, "lamp#62 ");
        so.FindProperty("doorGlow").objectReferenceValue = GlowOf(root, H, 62);
        so.FindProperty("doorInkLamp").objectReferenceValue = Gfx(root, H, "lamp#63 ");
        so.FindProperty("pressInkLamp").objectReferenceValue = Gfx(root, H, "lamp#64 ");
        so.FindProperty("catInkLamp").objectReferenceValue = Gfx(root, H, "lamp#65 ");
        so.FindProperty("doorLabel").objectReferenceValue = Tmp(root, H, "text#314 ");
        so.FindProperty("pressLabel").objectReferenceValue = Tmp(root, H, "text#316 ");
        so.FindProperty("catLabel").objectReferenceValue = Tmp(root, H, "text#318 ");
        so.FindProperty("catPct").objectReferenceValue = Tmp(root, H, "text#320 ");
        var catWellSprite = Named(root, H, "sprite#232 ");
        var catWell = Marker(group, "Cat charge well (marker)", ElementRect("sprites", 232));
        so.FindProperty("catWell").objectReferenceValue = catWell;
        so.FindProperty("catFill").objectReferenceValue = FillAt(group, "Cat charge fill", ElementRect("sprites", 232), catWellSprite, nominal, 1f);

        // squadron screens: craft cells are the latch_track_helm records; the fuel bars are the
        // drawn records inside them; craft 06 has fuel 0, so it has no record and is created
        var rowsProp = so.FindProperty("squadRows");
        rowsProp.arraySize = 2;
        int[][] tags = { new[] { 324, 325, 326, 327, 328, 329 }, new[] { 334, 335, 336, 337 } };
        int[][] cells = { new[] { 235, 236, 237, 238, 239, 240 }, new[] { 242, 243, 244, 245 } };
        int[][] fuel = { new[] { 253, 254, 255, 256, 257, -1 }, new[] { 258, 259, 260, 261 } };
        int[] names = { 322, 332 }, states = { 323, 333 }, readies = { 330, 338 }, roles = { 331, 339 }, sel = { 66, 67 };
        for (int r = 0; r < 2; r++)
        {
            var e = rowsProp.GetArrayElementAtIndex(r);
            e.FindPropertyRelative("name").objectReferenceValue = Tmp(root, H, "text#" + names[r] + " ");
            e.FindPropertyRelative("state").objectReferenceValue = Tmp(root, H, "text#" + states[r] + " ");
            e.FindPropertyRelative("ready").objectReferenceValue = Tmp(root, H, "text#" + readies[r] + " ");
            e.FindPropertyRelative("role").objectReferenceValue = Tmp(root, H, "text#" + roles[r] + " ");
            e.FindPropertyRelative("selLamp").objectReferenceValue = Gfx(root, H, "lamp#" + sel[r] + " ");
            e.FindPropertyRelative("selGlow").objectReferenceValue = GlowOf(root, H, sel[r]);
            SetRefs(e.FindPropertyRelative("tags"), tags[r].Select(i => (Object)Tmp(root, H, "text#" + i + " ")).ToArray());
            var cellRefs = new Object[cells[r].Length];
            var fuelRefs = new Object[cells[r].Length];
            for (int c = 0; c < cells[r].Length; c++)
            {
                var cellRect = ElementRect("sprites", cells[r][c]);
                cellRefs[c] = Marker(group, "Craft cell " + (r + 1) + "." + (c + 1) + " (marker)", cellRect);
                fuelRefs[c] = fuel[r][c] >= 0
                    ? Named(root, H, "drawn#" + fuel[r][c] + " ")
                    : FillAt(group, "Craft fuel " + (r + 1) + "." + (c + 1) + " (no record: fuel 0)", cellRect, Named(root, H, "sprite#" + cells[r][c] + " "), nominal, 1f);
            }
            SetRefs(e.FindPropertyRelative("cells"), cellRefs);
            SetRefs(e.FindPropertyRelative("fuel"), fuelRefs);
        }
        so.FindProperty("bezel").objectReferenceValue = Named(root, H, "drawn#252 ");
        // right-hand readouts whose text grows ("IN BAY" → "AIRBORNE — DESTROY", "SAFE" → "AIRBORNE"):
        // right-aligned so they grow leftward inside the screen (HANDOFF_GAPS — no template)
        foreach (var id in new[] { "text#323 ", "text#333 ", "text#358 " }) Tmp(root, H, id).alignment = TMPro.TextAlignmentOptions.Right;

        // launch order
        var ship = Named(root, H, "sprite#247 ");
        SetDial(so.FindProperty("shipSelector"), Named(root, H, "sprite#417 "), ship, 60f);
        EaseDial(so.FindProperty("shipSelector"), 0.2f, Spring); // line 1189
        Hit(ship, "hangar.squad", null);
        var cmd = Named(root, H, "sprite#250 ");
        SetDial(so.FindProperty("commandSelector"), Named(root, H, "sprite#421 "), cmd, 60f);
        EaseDial(so.FindProperty("commandSelector"), 0.2f, Spring); // line 1216
        Hit(cmd, "hangar.order", null);
        so.FindProperty("squadName").objectReferenceValue = Tmp(root, H, "text#342 ");
        so.FindProperty("orderName").objectReferenceValue = Tmp(root, H, "text#347 ");
        so.FindProperty("devLabel").objectReferenceValue = Tmp(root, H, "text#345 ");
        so.FindProperty("bearing").objectReferenceValue = BearingField(group, p, Named(root, H, "text#344 "));

        // launch sequence: six rockers, their lamps, labels and loading wells
        int[] rockers = { 254, 257, 260, 263, 266, 269 }, wells = { 255, 258, 261, 264, 267, 270 };
        var st = so.FindProperty("stages");
        st.arraySize = 6;
        for (int i = 0; i < 6; i++)
        {
            var e = st.GetArrayElementAtIndex(i);
            var rocker = Named(root, H, "sprite#" + rockers[i] + " ");
            e.FindPropertyRelative("rocker").objectReferenceValue = rocker.GetComponent<Image>();
            e.FindPropertyRelative("lamp").objectReferenceValue = Gfx(root, H, "lamp#" + (68 + i) + " ");
            e.FindPropertyRelative("label").objectReferenceValue = Tmp(root, H, "text#" + (349 + i) + " ");
            var wellRect = ElementRect("sprites", wells[i]);
            e.FindPropertyRelative("well").objectReferenceValue = Marker(group, "Stage well " + (i + 1) + " (marker)", wellRect);
            e.FindPropertyRelative("fill").objectReferenceValue = FillAt(group, "Stage fill " + (i + 1), wellRect, Named(root, H, "sprite#" + wells[i] + " "), nominal, 1f);
            Hit(rocker, "hangar.stage", i.ToString());
        }
        so.FindProperty("rockerUp").objectReferenceValue = LoadSprite("rocker_24x32_up");
        so.FindProperty("rockerLocked").objectReferenceValue = LoadSprite("rocker_24x32_locked");
        so.FindProperty("rockerThrown").objectReferenceValue = LoadSprite("rocker_24x32_thrown");
        so.FindProperty("interlock").objectReferenceValue = Tmp(root, H, "text#356 ");
        // a two-line box whose text the panel rewrites: wrap inside the measured width
        Tmp(root, H, "text#356 ").textWrappingMode = TMPro.TextWrappingModes.Normal;
        so.FindProperty("leverHint").objectReferenceValue = Tmp(root, H, "text#358 ");
        so.FindProperty("launchLamp").objectReferenceValue = Gfx(root, H, "lamp#74 ");
        var slot = Named(root, H, "sprite#273 ");
        so.FindProperty("leverSlot").objectReferenceValue = slot;
        so.FindProperty("leverCap").objectReferenceValue = Named(root, H, "sprite#274 ");
        // the green hazard fill left of the cap: drawn live, no record; placed in the slot's groove
        // (slot element 798–856, its 9-slice rims 16 px) — see HANDOFF_GAPS
        so.FindProperty("leverFill").objectReferenceValue = FillAt(group, "Launch lever track (no record)", new Rect(1457, 814, 0, 26), slot, nominal, 0.5f);
        Hit(slot, "hangar.lever", null);
        var recall = Named(root, H, "sprite#275 ");
        so.FindProperty("recall").objectReferenceValue = recall.GetComponent<Image>();
        so.FindProperty("recallUp").objectReferenceValue = LoadSprite("button_recall_up");
        so.FindProperty("recallPressed").objectReferenceValue = LoadSprite("button_recall_pressed");
        so.FindProperty("recallLamp").objectReferenceValue = Gfx(root, H, "lamp#75 ");
        Hit(recall, "hangar.recall", null);
        // Step 40: the pressed art fades in over the up art while RECALL eases down (.1 s)
        var overlay = NewRect("Recall pressed (easing)", recall);
        overlay.anchorMin = Vector2.zero; overlay.anchorMax = Vector2.one; overlay.offsetMin = overlay.offsetMax = Vector2.zero;
        var overlayImg = Img(overlay.gameObject, LoadSprite("button_recall_pressed"), Color.white, false);
        overlayImg.enabled = false;
        so.FindProperty("recallOverlay").objectReferenceValue = overlayImg;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // The launch bearing field (setLbrg, a text input — no record: the measurement skipped the
    // input's value). Right edge flush with its "°" label (text #344 at x 1636), the site's 32 px
    // width, the "°" label's line box and size; Share Tech Mono, ink.data. See HANDOFF_GAPS.
    static TMPro.TMP_InputField BearingField(RectTransform group, HandoffHangarPanel panel, RectTransform degree)
    {
        var deg = degree.GetComponent<TMPro.TextMeshProUGUI>();
        var field = NewRect("Launch bearing (input field)", group);
        TopLeft(field, new Rect(1636 - 32, 512, 32, 13.5f), Vector2.zero);
        field.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        var area = NewRect("Text Area", field);
        area.anchorMin = Vector2.zero; area.anchorMax = Vector2.one; area.offsetMin = area.offsetMax = Vector2.zero;
        area.gameObject.AddComponent<RectMask2D>();
        var textRt = NewRect("Text", area);
        textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one; textRt.offsetMin = textRt.offsetMax = Vector2.zero;
        var t = textRt.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
        t.font = deg.font;
        t.fontSharedMaterial = deg.fontSharedMaterial;
        t.fontSize = deg.fontSize;
        t.color = Hex("ink.data", 1f);
        t.alignment = TMPro.TextAlignmentOptions.Right;
        t.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        var input = field.gameObject.AddComponent<TMPro.TMP_InputField>();
        input.textViewport = area;
        input.textComponent = t;
        input.contentType = TMPro.TMP_InputField.ContentType.IntegerNumber;
        input.characterLimit = 3;
        input.text = "062";
        input.caretColor = Hex("ink.data", 1f);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(input.onValueChanged, new UnityEngine.Events.UnityAction<string>(panel.OnBearingTyped));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(input.onEndEdit, new UnityEngine.Events.UnityAction<string>(panel.OnBearingDone));
        Count("built: input field (launch bearing)");
        return input;
    }

    // ---- Fire Control drawer (Step 33g) ---------------------------------------------------

    static void WireFireControl(RectTransform root)
    {
        const string F = "State · fire control";
        var group = (RectTransform)root.Find(F);
        var p = root.gameObject.AddComponent<HandoffFireControlPanel>();
        var so = new SerializedObject(p);

        so.FindProperty("mountShort").objectReferenceValue = Tmp(root, F, "text#362 ");
        so.FindProperty("sightLamp").objectReferenceValue = Gfx(root, F, "lamp#76 ");
        so.FindProperty("sightGlow").objectReferenceValue = GlowOf(root, F, 76);
        Hit(Named(root, F, "sprite#283 "), "fc.sight", null);
        so.FindProperty("scopeBearing").objectReferenceValue = Tmp(root, F, "text#366 ");
        so.FindProperty("bearingEcho").objectReferenceValue = Tmp(root, F, "text#367 ");

        var sysKnob = Named(root, F, "sprite#287 ");
        SetDial(so.FindProperty("systemSelector"), Named(root, F, "sprite#425 "), sysKnob, 60f); // RAILGUN
        EaseDial(so.FindProperty("systemSelector"), 0.2f, Spring); // line 1484
        Hit(sysKnob, "fc.system", null);
        var mountKnob = Named(root, F, "sprite#288 ");
        SetDial(so.FindProperty("mountSelector"), Named(root, F, "sprite#428 "), mountKnob, 60f); // RG-A
        EaseDial(so.FindProperty("mountSelector"), 0.2f, Spring); // line 1502
        Hit(mountKnob, "fc.mount", null);
        System.Action<string, int[], int[]> positions = (prop, lamps, labels) =>
        {
            var arr = so.FindProperty(prop);
            arr.arraySize = lamps.Length;
            for (int i = 0; i < lamps.Length; i++)
            {
                var e = arr.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("lamp").objectReferenceValue = Gfx(root, F, "lamp#" + lamps[i] + " ");
                e.FindPropertyRelative("glow").objectReferenceValue = GlowOf(root, F, lamps[i]);
                e.FindPropertyRelative("label").objectReferenceValue = Tmp(root, F, "text#" + labels[i] + " ");
            }
        };
        positions("systemPositions", new[] { 77, 78, 79 }, new[] { 369, 370, 371 });
        positions("mountPositions", new[] { 80, 81 }, new[] { 372, 373 });
        so.FindProperty("mountLabel").objectReferenceValue = Tmp(root, F, "text#374 ");
        so.FindProperty("mountArc").objectReferenceValue = Tmp(root, F, "text#375 ");
        so.FindProperty("mountRange").objectReferenceValue = Tmp(root, F, "text#376 ");
        so.FindProperty("sysState").objectReferenceValue = Tmp(root, F, "text#377 ");

        so.FindProperty("solLamp").objectReferenceValue = Gfx(root, F, "lamp#82 ");
        so.FindProperty("solGlow").objectReferenceValue = GlowOf(root, F, 82);
        Hit(Named(root, F, "sprite#292 "), "fc.track", null);
        so.FindProperty("trackKey").objectReferenceValue = Named(root, F, "sprite#292 ");
        so.FindProperty("trackId").objectReferenceValue = Tmp(root, F, "text#380 ");
        so.FindProperty("trackSrc").objectReferenceValue = Tmp(root, F, "text#381 ");
        so.FindProperty("solRange").objectReferenceValue = Tmp(root, F, "text#383 ");
        so.FindProperty("solBrg").objectReferenceValue = Tmp(root, F, "text#386 ");
        so.FindProperty("solBrgDegree").objectReferenceValue = Tmp(root, F, "text#385 ");
        so.FindProperty("solSpd").objectReferenceValue = Tmp(root, F, "text#388 ");
        string[] fields = { "rng", "brg", "spd" };
        int[] knobs = { 294, 295, 296 }, ptrs = { 429, 430, 431 };
        for (int i = 0; i < 3; i++)
        {
            var k = Named(root, F, "sprite#" + knobs[i] + " ");
            SetDial(so.FindProperty(fields[i] + "Knob"), Named(root, F, "sprite#" + ptrs[i] + " "), k, 0f); // no solution: 0°
            Hit(k, "fc.knob", fields[i]);
        }

        so.FindProperty("capPct").objectReferenceValue = Tmp(root, F, "text#392 ");
        so.FindProperty("capLabel").objectReferenceValue = Tmp(root, F, "text#394 ");
        so.FindProperty("capNote").objectReferenceValue = Tmp(root, F, "text#393 ");
        SetRefs(so.FindProperty("capCells"), new[] { 266, 267, 268, 269, 270, 271, 272, 274, 275, 276 }.Select(i => (Object)Gfx(root, F, "drawn#" + i + " ")).ToArray());
        so.FindProperty("capMark").objectReferenceValue = Named(root, F, "drawn#273 ");
        so.FindProperty("capLamp").objectReferenceValue = Gfx(root, F, "lamp#83 ");
        so.FindProperty("capGlow").objectReferenceValue = GlowOf(root, F, 83);

        int[] rockers = { 302, 307, 312, 317 }, hints = { 397, 399, 401, 403 };
        var lad = so.FindProperty("ladder");
        lad.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            var e = lad.GetArrayElementAtIndex(i);
            var rk = Named(root, F, "sprite#" + rockers[i] + " ");
            e.FindPropertyRelative("rocker").objectReferenceValue = rk.GetComponent<Image>();
            e.FindPropertyRelative("lamp").objectReferenceValue = Gfx(root, F, "lamp#" + (84 + i) + " ");
            e.FindPropertyRelative("glow").objectReferenceValue = GlowOf(root, F, 84 + i);
            e.FindPropertyRelative("hint").objectReferenceValue = Tmp(root, F, "text#" + hints[i] + " ");
            Hit(rk, "fc.flip", i.ToString());
        }
        so.FindProperty("rockerUp").objectReferenceValue = LoadSprite("rocker_30x38_up");
        so.FindProperty("rockerLocked").objectReferenceValue = LoadSprite("rocker_30x38_locked");
        so.FindProperty("rockerThrown").objectReferenceValue = LoadSprite("rocker_30x38_thrown");
        so.FindProperty("readyLabel").objectReferenceValue = Tmp(root, F, "text#404 ");
        var slot = Named(root, F, "sprite#322 ");
        so.FindProperty("leverSlot").objectReferenceValue = slot;
        so.FindProperty("leverCap").objectReferenceValue = Named(root, F, "sprite#323 ");
        // the red hazard fill left of the cap: drawn live, no record; in the slot's groove
        // (slot element 796.5–854.5, its 9-slice rims 16 px) — see HANDOFF_GAPS
        Color alarm = Hex("accent.alarm", 1f).linear;
        so.FindProperty("leverFill").objectReferenceValue = FillAt(group, "Fire lever track (no record)", new Rect(1457, 812.5f, 0, 26), slot, alarm, 0.5f);
        Hit(slot, "fc.lever", null);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---- Helm (Step 33h-1) ------------------------------------------------------------------

    static void WireHelm(RectTransform root)
    {
        const string O = "State · helm", C = "Helm";
        var p = root.gameObject.AddComponent<HandoffHelmPanel>();
        var so = new SerializedObject(p);

        // header readouts: closed (default texts #90–99, lamp #30) and open (texts #407–416, lamp #88)
        System.Action<string, string, int, int> header = (prop, panel, t0, lamp) =>
        {
            var h = so.FindProperty(prop);
            string[] f = { "hdg", "turn", "thrCaption", "thr", "pct", "gLoad", "apState" };
            int[] at = { 3, 4, 5, 7, 6, 8, 9 }; // offsets from "HELM": 083, STEADY, THR, 0, %, 1.0 G, AUTO — DIRECT
            for (int i = 0; i < f.Length; i++) h.FindPropertyRelative(f[i]).objectReferenceValue = Tmp(root, panel, "text#" + (t0 + at[i]) + " ");
            h.FindPropertyRelative("lamp").objectReferenceValue = Gfx(root, panel, "lamp#" + lamp + " ");
            h.FindPropertyRelative("glow").objectReferenceValue = GlowOf(root, panel, lamp);
        };
        header("closed", C, 90, 30);
        header("open", O, 407, 88);

        // 01 FLIGHT: the fader well takes the press (thrDown); the cap and track follow thr
        Hit(Named(root, O, "sprite#335 "), "helm.thr", null);
        so.FindProperty("thrCap").objectReferenceValue = Named(root, O, "sprite#336 ");
        so.FindProperty("thrTrack").objectReferenceValue = Named(root, O, "drawn#281 ");
        so.FindProperty("hdgRing").objectReferenceValue = Named(root, O, "drawn#282 ");
        so.FindProperty("dialHdg").objectReferenceValue = Tmp(root, O, "text#421 ");
        so.FindProperty("dialTurn").objectReferenceValue = Tmp(root, O, "text#422 ");

        // 02 ATTITUDE HOLD: AP_ATT order NOSE, STBD, PORT, TAIL = drawn #285, #290, #295, #300;
        // each key is followed by its stem outline, stem core, arrow outline and arrow core
        string[] att = { "NOSE", "STBD", "PORT", "TAIL" };
        string[] notes = { "NOSE ON TARGET", "STBD BEAM TO TARGET", "PORT BEAM TO TARGET", "TAIL TO TARGET" };
        var keys = so.FindProperty("attKeys");
        keys.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            int d = 285 + i * 5;
            var e = keys.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("key").stringValue = att[i];
            e.FindPropertyRelative("note").stringValue = notes[i];
            // The glyph is turned -45° inside a key turned +45°, so it stands upright; build it
            // upright at its recorded size, not as a square at 45° (Step 33h-1 verification).
            // The arrow and its core take the AP_ATT clip-path triangle.
            foreach (int k in new[] { d + 1, d + 3 })
            {
                var rt = Named(root, O, "drawn#" + k + " ");
                rt.sizeDelta = ElementRect("drawn", k).size;
                rt.localEulerAngles = Vector3.zero;
            }
            foreach (int k in new[] { d + 3, d + 4 })
            {
                var go = Named(root, O, "drawn#" + k + " ").gameObject;
                var img = go.GetComponent<Image>();
                var c = img.color;
                Object.DestroyImmediate(img);
                var poly = go.AddComponent<HandoffPolygon>();
                poly.color = c;
                poly.raycastTarget = false;
                poly.SetPoints(AttClip[i]);
            }
            string[] parts = { "stem", "stemCore", "arrow", "arrowCore" };
            for (int k = 0; k < 4; k++) e.FindPropertyRelative(parts[k]).objectReferenceValue = Gfx(root, O, "drawn#" + (d + 1 + k) + " ");
            var key = Named(root, O, "drawn#" + d + " ");
            SetRefs(e.FindPropertyRelative("edge"), InsetEdge(key, i == 0 ? Hex("accent.nominal", 1f) : Hex("ink.data", 0.26f))); // NOSE held at load
            Hit(key, "helm.att", att[i]);
        }
        so.FindProperty("attNote").objectReferenceValue = Tmp(root, O, "text#425 ");

        // 03 APPROACH
        Hit(Named(root, O, "sprite#343 "), "helm.range", "-1");
        Hit(Named(root, O, "sprite#344 "), "helm.range", "1");
        so.FindProperty("rangeDown").objectReferenceValue = Tmp(root, O, "text#427 ");
        so.FindProperty("rangeUp").objectReferenceValue = Tmp(root, O, "text#430 ");
        so.FindProperty("rangeLabel").objectReferenceValue = Tmp(root, O, "text#429 ");
        string[] modeKeys = { "DIRECT", "FLANK", "EVASIVE" };
        int[] modeTexts = { 431, 433, 435 };
        var modes = so.FindProperty("modes");
        modes.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            var e = modes.GetArrayElementAtIndex(i);
            var row = Named(root, O, "drawn#" + (306 + i) + " ");
            e.FindPropertyRelative("key").stringValue = modeKeys[i];
            e.FindPropertyRelative("bg").objectReferenceValue = row.GetComponent<Graphic>();
            SetRefs(e.FindPropertyRelative("edge"), InsetEdge(row, i == 0 ? Hex("ink.data", 0.42f) : Hex("white", 0.08f))); // DIRECT at load
            e.FindPropertyRelative("lamp").objectReferenceValue = Gfx(root, O, "lamp#" + (89 + i) + " ");
            e.FindPropertyRelative("glow").objectReferenceValue = GlowOf(root, O, 89 + i);
            e.FindPropertyRelative("label").objectReferenceValue = Tmp(root, O, "text#" + modeTexts[i] + " ");
            Hit(row, "helm.mode", modeKeys[i]);
        }

        // 04 STABILITY: the nine cell faces, top first (drawn #310, #312 … #326)
        SetRefs(so.FindProperty("stabCells"), Enumerable.Range(0, 9).Select(i => (Object)Gfx(root, O, "drawn#" + (310 + i * 2) + " ")).ToArray());
        so.FindProperty("stabPct").objectReferenceValue = Tmp(root, O, "text#439 ");
        so.FindProperty("gLoad").objectReferenceValue = Tmp(root, O, "text#441 ");
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---- Orders station (Step 33h-2) -----------------------------------------------------

    static void WireOrders(RectTransform root)
    {
        const string O = "State · orders", C = "Orders";
        var p = root.gameObject.AddComponent<HandoffOrdersPanel>();
        var so = new SerializedObject(p);

        // the alert selector, stowed (knob #91, pointer #384, lamps #27–29, texts #84–89) and open
        // (knob #354, pointer #435, lamps #92–94, texts #443–448). Clickable in both (spec line 1767).
        System.Action<string, string, int, int, int, int> alertSet = (prop, panel, knob, pointer, lamp0, text0) =>
        {
            var a = so.FindProperty(prop);
            var k = Named(root, panel, "sprite#" + knob + " ");
            SetDial(a.FindPropertyRelative("pointer"), Named(root, panel, "sprite#" + pointer + " "), k, 60f); // CONDITION 3
            EaseDial(a.FindPropertyRelative("pointer"), 0.2f, Spring); // alert, line 1772
            Hit(k, "orders.alert", null);
            // only the first lamp has a glow in the export; the others get a copy of it (HANDOFF_GAPS)
            var glows = new Object[3];
            for (int i = 0; i < 3; i++) glows[i] = GlowOf(root, panel, lamp0 + i) ?? CopyGlow(root, panel, lamp0, lamp0 + i);
            SetRefs(a.FindPropertyRelative("lamps"), Enumerable.Range(0, 3).Select(i => (Object)Gfx(root, panel, "lamp#" + (lamp0 + i) + " ")).ToArray());
            SetRefs(a.FindPropertyRelative("glows"), glows);
            SetRefs(a.FindPropertyRelative("labels"), Enumerable.Range(0, 3).Select(i => (Object)Tmp(root, panel, "text#" + (text0 + i) + " ")).ToArray());
            a.FindPropertyRelative("condition").objectReferenceValue = Tmp(root, panel, "text#" + (text0 + 4) + " ");
            a.FindPropertyRelative("release").objectReferenceValue = Tmp(root, panel, "text#" + (text0 + 5) + " ");
        };
        alertSet("stowed", C, 91, 384, 27, 84);
        alertSet("open", O, 354, 435, 92, 443);

        // AUX pool. The right column ends at x 460 (REACTOR, 0%, NO FEED): right-aligned so longer
        // values grow left (HANDOFF_GAPS — no template for the row)
        foreach (var id in new[] { "text#450 ", "text#452 ", "text#454 " }) Tmp(root, O, id).alignment = TMPro.TextAlignmentOptions.Right;
        so.FindProperty("actingOn").objectReferenceValue = Tmp(root, O, "text#450 ");
        so.FindProperty("auxPct").objectReferenceValue = Tmp(root, O, "text#452 ");
        so.FindProperty("orderNote").objectReferenceValue = Tmp(root, O, "text#453 ");
        so.FindProperty("auxRate").objectReferenceValue = Tmp(root, O, "text#454 ");
        // cells drawn #329–338 are latch frames only; a lit cell gets a 1 px inset ink.data face,
        // as the stability cells have (stand-in, HANDOFF_GAPS)
        var group = (RectTransform)root.Find(O);
        var cells = new Object[10];
        for (int i = 0; i < 10; i++)
        {
            var cell = Named(root, O, "drawn#" + (329 + i) + " ");
            var r = ElementRect("drawn", 329 + i);
            var face = FillAt(group, "Aux cell face " + (i + 1) + " (no record)", new Rect(r.x + 1, r.y + 1, r.width - 2, r.height - 2), cell, Hex("ink.data", 1f).linear, 1f);
            face.gameObject.SetActive(false);
            cells[i] = face.GetComponent<Graphic>();
        }
        SetRefs(so.FindProperty("auxCells"), cells);

        // order rockers: PARTY, SEAL, VENT live; FIRE, BLKHD, SCRUB blanking plates
        string[] ids = { "party", "seal", "vent", "fire", "blkhd", "scrub" };
        int[] rockers = { 359, 362, 365, 368, 371, 374 };
        var ord = so.FindProperty("orders");
        ord.arraySize = 6;
        for (int i = 0; i < 6; i++)
        {
            var e = ord.GetArrayElementAtIndex(i);
            var rk = Named(root, O, "sprite#" + rockers[i] + " ");
            e.FindPropertyRelative("id").stringValue = ids[i];
            e.FindPropertyRelative("blank").boolValue = i >= 3;
            e.FindPropertyRelative("rocker").objectReferenceValue = rk.GetComponent<Image>();
            e.FindPropertyRelative("lamp").objectReferenceValue = Gfx(root, O, "lamp#" + (95 + i) + " ");
            e.FindPropertyRelative("glow").objectReferenceValue = GlowOf(root, O, 95 + i);
            e.FindPropertyRelative("label").objectReferenceValue = Tmp(root, O, "text#" + (455 + i) + " ");
            if (i < 3) Hit(rk, "orders.run", ids[i]);
        }
        so.FindProperty("rockerUp").objectReferenceValue = LoadSprite("rocker_22x30_up");
        so.FindProperty("rockerLocked").objectReferenceValue = LoadSprite("rocker_22x30_locked");
        so.FindProperty("rockerThrown").objectReferenceValue = LoadSprite("rocker_22x30_thrown");
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // The tactical world (Step 34) sits outside the board, so a rebuild keeps it, but its link to
    // the board it feeds points at the board just destroyed. Point it at the new one.
    static void RelinkWorld(RectTransform root)
    {
        var world = Object.FindAnyObjectByType<TacticalWorld>(FindObjectsInactive.Include);
        if (world == null) return;
        var so = new SerializedObject(world);
        so.FindProperty("board").objectReferenceValue = root.GetComponent<HandoffBoardControls>();
        so.ApplyModifiedPropertiesWithoutUndo();
        Count("relinked: tactical world");
    }

    // ---- Plot tools (Step 33i) -------------------------------------------------------------

    static void WirePlot(RectTransform root)
    {
        var plot = (RectTransform)root.Find("Plot");
        var mockup = plot.Find("Mockup");
        var p = root.gameObject.AddComponent<HandoffPlotPanel>();
        var so = new SerializedObject(p);
        int at = mockup.GetSiblingIndex() + 1; // above the plot's content, under the tube artefacts

        System.Func<string, RectTransform> layer = name =>
        {
            var rt = NewRect(name, plot);
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(1712, 890);
            rt.anchoredPosition = Vector2.zero;
            rt.SetSiblingIndex(at++);
            return rt;
        };

        // a hidden copy of a plot label (text #47, TMP 11 px), cloned at run time for labels
        System.Func<RectTransform, string, TMPro.TMP_Text> labelTemplate = (parent, name) =>
        {
            var go = Object.Instantiate(Named(root, "Plot/Mockup", "text#47 ").gameObject, parent);
            go.name = name;
            var t = go.GetComponent<TMPro.TMP_Text>();
            t.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            t.overflowMode = TMPro.TextOverflowModes.Overflow;
            t.raycastTarget = false;
            var r = (RectTransform)go.transform;
            r.anchorMin = r.anchorMax = new Vector2(0, 1);
            go.SetActive(false);
            return t;
        };

        // Step 37: our ship is the 3D model seen from above (ShipView), not the design's placeholder
        // rectangles (drawn #41–45, which also carried a heading line at a fixed mock angle). The
        // design's glyph is a 34×76 hull turned to heading that shrinks with zoom; the panel sizes it.
        foreach (var id in new[] { "drawn#41 ", "drawn#42 ", "drawn#43 ", "drawn#44 ", "drawn#45 " })
        {
            Named(root, "Plot/Mockup", id).gameObject.SetActive(false);
            Count("hidden: own-ship placeholder");
        }
        // Owner, 2026-10-18: nothing standing on the plot. Our ship's name, call sign and mock
        // heading/speed line (#42–44) and the SENSOR RANGE label (#37–38) go: the player learns these
        // through the tools (the sensors panel still positions #37–38). The PDC GROUP A arc and label
        // (#39–41) show only once the PDCs are activated, wired in the fire-control step. BOARD POINT
        // ALPHA and its × (#49–50) were a mock waypoint. #33 is an empty circle; #35 a frozen piece of the
        // radar sweep, which is built for real in the sensors step (HANDOFF_GAPS row 105).
        foreach (var id in new[] { "text#37 ", "text#38 ", "text#42 ", "text#43 ", "text#44 ",
                                   "drawn#39 ", "drawn#40 ", "text#40 ", "text#41 ", "text#49 ", "text#50 ",
                                   "drawn#33 ", "drawn#35 " })
        {
            Named(root, "Plot/Mockup", id).gameObject.SetActive(false);
            Count("hidden: standing plot text and art");
        }
        // Step 42 (owner 2026-10-18): the sensor circle, DRADIS's hearing range around our ship,
        // under our ship and everything else on the plot
        var ringRt = layer("Sensor ring (drawn)");
        ringRt.gameObject.AddComponent<CanvasRenderer>();
        var ringG = ringRt.gameObject.AddComponent<HandoffMarksGraphic>();
        ringG.color = Hex("ink.data", 1f);
        ringG.raycastTarget = false;
        so.FindProperty("sensorRing").objectReferenceValue = ringG;

        // Step 46: the selected weapon's reach, a dashed amber ring while Fire Control is open
        var wrRt = layer("Weapon ring (drawn)");
        wrRt.gameObject.AddComponent<CanvasRenderer>();
        var wrG = wrRt.gameObject.AddComponent<HandoffMarksGraphic>();
        wrG.color = Hex("accent.caution", 1f);
        wrG.raycastTarget = false;
        so.FindProperty("weaponRing").objectReferenceValue = wrG;

        // Step 44: passive bearing lines, fading out from our ship toward heard contacts
        var pasRt = layer("Passive bearings (drawn)");
        pasRt.gameObject.AddComponent<CanvasRenderer>();
        var pasG = pasRt.gameObject.AddComponent<HandoffMarksGraphic>();
        pasG.color = Hex("ink.data", 1f);
        pasG.raycastTarget = false;
        so.FindProperty("passiveLines").objectReferenceValue = pasG;

        // Step 43: the radar sweep, turning inside the sensor circle (the panel sizes, places and turns it)
        var sweepRt = layer("Sensor sweep (drawn)");
        sweepRt.pivot = new Vector2(0.5f, 0.5f);
        sweepRt.gameObject.AddComponent<CanvasRenderer>();
        var sweepG = sweepRt.gameObject.AddComponent<HandoffSweepGraphic>();
        sweepG.color = Hex("ink.data", 1f);
        sweepG.raycastTarget = false;
        sweepRt.gameObject.SetActive(false);
        so.FindProperty("sweep").objectReferenceValue = sweepG;

        var shipRt = layer("Own ship (3D view)");
        shipRt.gameObject.AddComponent<CanvasRenderer>();
        var shipImg = shipRt.gameObject.AddComponent<RawImage>();
        shipImg.material = ShipImageMaterial();
        shipImg.raycastTarget = false;
        shipImg.enabled = false; // no texture until play (a RawImage with none draws white); the panel turns it on
        shipRt.pivot = new Vector2(0.5f, 0.5f);
        shipRt.sizeDelta = new Vector2(95, 95);
        shipRt.anchoredPosition = new Vector2(960 - 104, -(545 - 100));
        so.FindProperty("ownShipImage").objectReferenceValue = shipImg;

        // Step 54 (owner 2026-10-23): our fighters, small wedges in own-force blue; the panel clones,
        // places and turns one per craft
        var ftRt = NewRect("Fighter (template)", layer("Fighters"));
        ftRt.anchorMin = ftRt.anchorMax = new Vector2(0, 1);
        ftRt.pivot = new Vector2(0.5f, 0.5f);
        ftRt.gameObject.AddComponent<CanvasRenderer>();
        var ftImg = ftRt.gameObject.AddComponent<Image>();
        ftImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Hulls/hull_wedge_filled.png");
        ftImg.preserveAspect = true;
        ftImg.raycastTarget = false;
        ftRt.gameObject.SetActive(false);
        so.FindProperty("fighterTemplate").objectReferenceValue = ftImg;

        // Step 34: identified hostile contacts at their true range and bearing. Stand-in red dots
        // and name labels until the contact glyphs arrive (CLAUDE_DESIGN_NEEDS 1.2). Under the marks.
        var cm = layer("Contacts (drawn)");
        cm.gameObject.AddComponent<CanvasRenderer>();
        var cg = cm.gameObject.AddComponent<HandoffMarksGraphic>();
        cg.color = Hex("accent.alarm", 1f);
        cg.raycastTarget = false;
        so.FindProperty("contactMarks").objectReferenceValue = cg;
        so.FindProperty("contactLabelTemplate").objectReferenceValue = labelTemplate(layer("Contact labels"), "Contact label (template)");

        // Step 34 (owner 2026-10-17): the four mock-up contacts are gone, so their plot art is hidden,
        // not deleted: CONTACT 22's BRG 149 line, FTR-2's wing, BC-01, the predicted-position ghost,
        // the contact vectors and the asteroid field.
        string[] mock = { "text#39 ", "text#45 ", "text#46 ", "text#47 ", "text#48 ", "text#51 ", "text#52 ", "text#53 ",
                          "drawn#36 ", "drawn#37 ", "drawn#46 ", "drawn#47 ", "drawn#48 ", "drawn#49 ", "drawn#50 ",
                          "drawn#51 ", "drawn#52 ", "drawn#53 ", "drawn#54 ", "drawn#55 ",
                          "lamp#7 ", "lamp#8 ", "lamp#9 ", "lamp#10 ", "lamp#11 ", "lamp#12 " };
        foreach (var id in mock) { Named(root, "Plot/Mockup", id).gameObject.SetActive(false); Count("hidden: mock-up contact art"); }

        // plotCapture: takes the pointer only for POINT, RULER, COMPASS (the panel switches it)
        var cap = layer("Marks capture");
        var capImg = cap.gameObject.AddComponent<Image>();
        capImg.color = new Color(0, 0, 0, 0);
        capImg.raycastTarget = false;
        cap.gameObject.AddComponent<HandoffHit>().action = "plot";
        so.FindProperty("capture").objectReferenceValue = capImg;
        so.FindProperty("grid").objectReferenceValue = root.Find("Plot/Grid").GetComponent<HandoffPlotGrid>();

        var m = layer("Marks (drawn)");
        m.gameObject.AddComponent<CanvasRenderer>();
        var g = m.gameObject.AddComponent<HandoffMarksGraphic>();
        g.color = Hex("accent.ownForce", 1f); // every mark colour is own-force blue (§8.4)
        g.raycastTarget = false;
        so.FindProperty("marks").objectReferenceValue = g;

        // mark labels: TMP at 11 px, cloned at run time
        so.FindProperty("labelTemplate").objectReferenceValue = labelTemplate(layer("Mark labels"), "Mark label (template)");
        so.FindProperty("hitLayer").objectReferenceValue = layer("Mark hits (ERASE)");

        // the passive bearing label in the export (BRG 149, CONTACT 22) puts the scope on its contact.
        // Step 44: it is the template for every heard contact's label (the panel sets the key)
        so.FindProperty("passives").arraySize = 0;
        var brg = Named(root, "Plot/Mockup", "text#39 ");
        Hit(brg, "plot.look", null);
        so.FindProperty("passiveLabelTemplate").objectReferenceValue = brg.GetComponent<TMPro.TMP_Text>();
        Count("built: plot marks layers");
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // A lamp the export gives no glow: a copy of another lamp's glow, moved by the lamps' offset,
    // hidden until the panel lights it.
    static Graphic CopyGlow(RectTransform root, string panel, int from, int to)
    {
        var glow = (RectTransform)GlowOf(root, panel, from).transform;
        var a = Named(root, panel, "lamp#" + from + " ");
        var b = Named(root, panel, "lamp#" + to + " ");
        var copy = (RectTransform)Object.Instantiate(glow.gameObject, glow.parent).transform;
        copy.name = "glow · lamp#" + to + " (copy of #" + from + ")";
        copy.anchoredPosition = glow.anchoredPosition + (b.anchoredPosition - a.anchoredPosition);
        copy.SetSiblingIndex(b.GetSiblingIndex());
        copy.gameObject.SetActive(false);
        Count("built: copied lamp glow");
        return copy.GetComponent<Graphic>();
    }

    // ---- Periscope (Step 33h-3) -----------------------------------------------------------

    static void WirePeriscope(RectTransform root)
    {
        const string P = "Periscope";
        var group = (RectTransform)root.Find(P);
        var p = root.gameObject.AddComponent<HandoffPeriscopePanel>();
        var so = new SerializedObject(p);

        so.FindProperty("mountShort").objectReferenceValue = Tmp(root, P, "text#63 ");
        var ap = Named(root, P, "sprite#63 ");
        so.FindProperty("aperture").objectReferenceValue = ap.GetComponent<Image>();
        so.FindProperty("apertureIdle").objectReferenceValue = LoadSprite("periscope_aperture_idle");
        so.FindProperty("apertureSlewing").objectReferenceValue = LoadSprite("periscope_aperture_slewing");
        Hit(ap, "scope.slew", null);

        // the sight canvas (canvases[1], 790,86.5 340x340) under a circle mask: the aperture is a
        // 340 px circle (asset-map). Above the aperture glass, under the vignette.
        var maskRt = NewRect("Sight mask (circle)", group);
        TopLeft(maskRt, new Rect(790, 86.5f, 340, 340), PeriscopeHoused.position);
        maskRt.SetSiblingIndex(ap.GetSiblingIndex() + 1);
        var maskImg = maskRt.gameObject.AddComponent<Image>();
        maskImg.sprite = ShapeSprite("aperture_mask");
        maskImg.raycastTarget = false;
        maskRt.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        var sightRt = NewRect("Sight (drawScope)", maskRt);
        sightRt.anchorMin = Vector2.zero; sightRt.anchorMax = Vector2.one; sightRt.offsetMin = sightRt.offsetMax = Vector2.zero;
        sightRt.gameObject.AddComponent<CanvasRenderer>(); // AddComponent of a Graphic subclass does not bring it here
        var sight = sightRt.gameObject.AddComponent<HandoffScopeSight>();
        sight.raycastTarget = false;
        so.FindProperty("sight").objectReferenceValue = sight;
        Count("built: periscope sight (drawScope)");

        so.FindProperty("offAxis").objectReferenceValue = Tmp(root, P, "text#66 ");
        so.FindProperty("zoomLabel").objectReferenceValue = Tmp(root, P, "text#68 ");
        so.FindProperty("bearing").objectReferenceValue = Tmp(root, P, "text#71 ");
        so.FindProperty("slewLabel").objectReferenceValue = Tmp(root, P, "text#72 ");
        so.FindProperty("subjectName").objectReferenceValue = Tmp(root, P, "text#75 ");
        so.FindProperty("subjectClass").objectReferenceValue = Tmp(root, P, "text#76 ");
        var knob = Named(root, P, "sprite#66 ");
        SetDial(so.FindProperty("magPointer"), Named(root, P, "sprite#376 "), knob, -124f); // 1.4x
        Hit(knob, "scope.mag", null);

        // target brackets (drawn #64–71) and the track name (text #64) move together
        var br = Enumerable.Range(64, 8).Select(i => (Object)Named(root, P, "drawn#" + i + " ")).ToList();
        br.Add(Named(root, P, "text#64 "));
        SetRefs(so.FindProperty("brackets"), br.ToArray());
        so.FindProperty("targetName").objectReferenceValue = Tmp(root, P, "text#64 "); // binding assumed: renderVals targetName (HANDOFF_GAPS)
        so.FindProperty("convBar").objectReferenceValue = Named(root, P, "drawn#80 ");

        so.FindProperty("slewLamp").objectReferenceValue = Gfx(root, P, "lamp#22 ");
        so.FindProperty("slewGlow").objectReferenceValue = GlowOf(root, P, 22);
        so.FindProperty("classLamp").objectReferenceValue = Gfx(root, P, "lamp#23 ");
        so.FindProperty("classGlow").objectReferenceValue = GlowOf(root, P, 23) ?? CopyGlow(root, P, 22, 23);
        Hit(Named(root, P, "sprite#69 "), "scope.slewTo", null);
        Hit(Named(root, P, "sprite#72 "), "scope.classify", null);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // AP_ATT clip: polygon(...) as box fractions, x from left, y from top
    static readonly Vector2[][] AttClip =
    {
        new[] { new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) }, // NOSE
        new[] { new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(0f, 1f) }, // STBD
        new[] { new Vector2(0f, 0.5f), new Vector2(1f, 0f), new Vector2(1f, 1f) }, // PORT
        new[] { new Vector2(0.5f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f) }  // TAIL
    };

    // box-shadow "inset 0 0 0 1px": four 1 px lines inside the element, children so they turn with it.
    // Built in the load state; the panel recolours them.
    static Object[] InsetEdge(RectTransform el, Color c)
    {
        var lines = new Object[4];
        Vector2[] min = { new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 0) };
        Vector2[] max = { new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        string[] names = { "Top", "Bottom", "Left", "Right" };
        for (int i = 0; i < 4; i++)
        {
            var rt = NewRect("Edge · " + names[i], el);
            rt.anchorMin = min[i]; rt.anchorMax = max[i];
            rt.pivot = new Vector2(min[i].x == max[i].x ? min[i].x : 0.5f, min[i].y == max[i].y ? min[i].y : 0.5f);
            bool horizontal = i < 2;
            rt.sizeDelta = horizontal ? new Vector2(0, 1) : new Vector2(1, -2); // sides between top and bottom
            rt.anchoredPosition = Vector2.zero;
            var img = rt.gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = c;
            lines[i] = img;
        }
        Count("built: inset edge (helm keys and mode rows)");
        return lines;
    }

    // A lamp's glow, or null when the layout gives that lamp none.
    static Graphic GlowOf(RectTransform root, string panel, int lamp)
    {
        foreach (Transform c in root.Find(panel)) if (c.name.StartsWith("glow · lamp#" + lamp + " ")) return c.GetComponent<Graphic>();
        return null;
    }

    static TMPro.TMP_Text Tmp(RectTransform root, string panel, string prefix) => Named(root, panel, prefix).GetComponent<TMPro.TMP_Text>();

    static Graphic Gfx(RectTransform root, string panel, string prefix) => Named(root, panel, prefix).GetComponent<Graphic>();

    static void Hit(RectTransform target, string action, string arg)
    {
        var gfx = target.GetComponent<Graphic>();
        gfx.raycastTarget = true;
        Vector4 g;
        // raycastPadding is (left, bottom, right, top); positive shrinks the click area
        if (spriteGrow.TryGetValue(target.gameObject, out g)) gfx.raycastPadding = new Vector4(g.x, g.w, g.z, g.y);
        var h = target.gameObject.AddComponent<HandoffHit>();
        h.action = action;
        h.arg = arg;
    }

    static void HeaderHit(RectTransform root, string panel, Rect board)
    {
        var p = (RectTransform)root.Find(panel);
        var rt = NewRect("Hit · orders header", p);
        TopLeft(rt, board, Vector2.zero);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        Hit(rt, "orders", null);
    }

    // A built record by name prefix ("sprite#95 ") under a panel path; stand-ins carry a
    // "STAND-IN " prefix in front of it.
    static RectTransform Named(RectTransform root, string panel, string prefix)
    {
        var p = root.Find(panel);
        foreach (Transform c in p) if (c.name.StartsWith(prefix) || c.name.StartsWith("STAND-IN " + prefix)) return (RectTransform)c;
        throw new System.Exception("[HandoffBoard] not found: " + panel + "/" + prefix);
    }

    static void SetRefs(SerializedProperty arr, params Object[] refs)
    {
        arr.arraySize = refs.Length;
        for (int i = 0; i < refs.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = refs[i];
    }

    // ---- placing records -----------------------------------------------------------------

    // Draw order inside a group: a record inside another record's box draws after it; among
    // equals, by record type (texture, sprite, drawn, glow, lamp, icon, text), then page order.
    static void Place(Group g, List<Rec> recs)
    {
        foreach (var r in recs)
            r.depth = recs.Count(o => o != r && Contains(o.box, r.box));
        foreach (var r in recs.OrderBy(r => r.depth).ThenBy(r => r.typeOrder).ThenBy(r => r.index))
        {
            switch (r.kind)
            {
                case "texture": MakeTexture(g, r); break;
                case "sprite": case "icon": MakeSprite(g, r, (string)r.j["sprite"]); break;
                case "lamp": MakeLamp(g, r); break;
                case "text": MakeText(g, r); break;
                case "drawn": MakeDrawn(g, r); break;
            }
        }
    }

    static void MakeTexture(Group g, Rec r)
    {
        var p = r.j["place"];
        var area = p != null ? new Rect((float)p["x"], (float)p["y"], (float)p["w"], (float)p["h"]) : r.box;
        var go = NewChild(g, r.name, area, 0, null);
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/textures/" + (string)r.j["texture"] + ".png");
        if (s == null) Missing(r);
        Img(go, s, Color.white, false);
        Count("built: texture");
    }

    static void MakeSprite(Group g, Rec r, string sprite)
    {
        var s = LoadSprite(sprite);
        if (s == null) { Missing(r); return; }
        Vector2 at;
        if (FixedCompositions.TryGetValue(sprite, out at))
        {
            var native = new Vector2(s.rect.width, s.rect.height) / (s.pixelsPerUnit / 100f);
            Img(NewChild(g, r.name, new Rect(at, native), 0, null), s, Tint(r, sprite), false);
            Count("built: " + r.kind + " (fixed composition)");
            return;
        }
        var size = Unrotated(r);
        var m = Margin(sprite, s);
        bool sliced = s.border != Vector4.zero;
        Vector4 grow = m;
        if (!sliced)
        {
            float coreW = s.rect.width / (s.pixelsPerUnit / 100f) - m.x - m.z;
            float coreH = s.rect.height / (s.pixelsPerUnit / 100f) - m.y - m.w;
            float sx = coreW > 0 ? size.x / coreW : 1f, sy = coreH > 0 ? size.y / coreH : 1f;
            grow = new Vector4(m.x * sx, m.y * sy, m.z * sx, m.w * sy);
        }
        var c = r.box.center;
        var full = new Vector2(size.x + grow.x + grow.z, size.y + grow.y + grow.w);
        // asymmetric margins move the image centre off the element centre
        var centre = new Vector2(c.x + (grow.z - grow.x) / 2f, c.y + (grow.w - grow.y) / 2f);
        var go = NewChild(g, r.name, new Rect(centre.x - full.x / 2f, centre.y - full.y / 2f, full.x, full.y), Rot(r), full);
        var img = Img(go, s, Tint(r, sprite), false);
        spriteGrow[go] = grow;
        if (sliced) img.type = Image.Type.Sliced;
        Count("built: " + r.kind);
    }

    static void MakeLamp(Group g, Rec r)
    {
        string tok = (string)r.j["tint"];
        // unlit tokens: lamp.off, lamp.offPanel, lamp.offSelector, lamp.phosphorOff, cell.*
        bool lit = !(tok.StartsWith("lamp.off") || (tok.StartsWith("lamp.") && tok.EndsWith("Off")) || tok.StartsWith("cell."));
        // Every lamp that can glow gets its glow; an unlit one starts hidden (panels switch it).
        if (r.j["glow"] != null && r.j["glow"].Type != JTokenType.Null)
        {
            var gs = LoadSprite((string)r.j["glow"]);
            var b = r.box;
            var go = NewChild(g, "glow · " + r.name, new Rect(b.x - GlowReach, b.y - GlowReach, b.width + 2 * GlowReach, b.height + 2 * GlowReach), 0, null);
            Img(go, gs, Hex(tok, (float)r.j["alpha"] * GlowAlpha), false);
            go.SetActive(lit);
            Count(lit ? "built: lamp glow" : "built: lamp glow (unlit, hidden)");
        }
        var s = LoadSprite((string)r.j["sprite"]);
        if (s == null) { Missing(r); return; }
        var lamp = NewChild(g, r.name, r.box, Rot(r), null);
        Img(lamp, s, Hex(tok, (float)r.j["alpha"]), false);
        Count("built: lamp");
    }

    static void MakeText(Group g, Rec r)
    {
        string family = (string)r.j["font"];
        int weight = (int)r.j["weight"];
        string fontName = family == "Share Tech Mono" ? "ShareTechMono-Regular"
            : weight >= 600 ? "BarlowCondensed-SemiBold" : weight >= 500 ? "BarlowCondensed-Medium" : "BarlowCondensed-Regular";
        var fa = fonts[fontName];
        if (fa == null) { Count("MISSING font " + fontName); return; }
        string text = (string)r.j["text"];
        float size = (float)r.j["size"];
        var b = r.box;

        // Vertical runs (the plot's Y axis legend): box much taller than wide; reads bottom to top.
        bool vertical = b.height > 2f * b.width && text.Length > 1;
        bool tall = !vertical && b.height > size * 1.8f;
        Vector2 u = vertical ? new Vector2(b.height, b.width) : new Vector2(b.width, b.height);
        float rot = vertical ? 90f : -(float)r.j["rotation"];

        var go = NewChild(g, r.name, b, rot, u); // centred on the box, rotated about its centre
        var t = go.AddComponent<TextMeshProUGUI>();
        t.font = fa;
        t.fontSharedMaterial = family == ThickenedFamily ? ThickMaterial(fa) : fa.material;
        t.richText = false;
        t.text = text;
        t.fontSize = size;
        t.color = Hex((string)r.j["color"], (float)r.j["alpha"]);
        t.characterSpacing = (float)r.j["trackingPx"] / size * 100f;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        t.margin = Vector4.zero;
        t.raycastTarget = false;
        string align = (string)r.j["align"];
        t.alignment = align == "center" ? TextAlignmentOptions.Center : align == "right" ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
        if (tall) BreakLines(t, text, b.width, b.height, size);
        else if (!vertical && align == "start") RestoreLeadingSpace(t, r, text);

        // "X ( )": a label wrapped round a separate run ("1,000 KM") that sits inside its box.
        // The closing part belongs at the far end of the box, not after the gap.
        var wrap = Regex.Match(text, @"^(.*\()\s+(\).*)$");
        if (wrap.Success)
        {
            t.text = wrap.Groups[1].Value;
            var close = Object.Instantiate(go, go.transform.parent);
            close.name = r.name + " (close)";
            var ct = close.GetComponent<TextMeshProUGUI>();
            ct.text = wrap.Groups[2].Value;
            ct.alignment = TextAlignmentOptions.Right;
            ct.ForceMeshUpdate(true);
            Count("text: wrapper split round inner run");
        }
        t.ForceMeshUpdate(true);
        Count("built: text");
    }

    // layout.json trims text, so a run that began with a space (the gap after the "3" in
    // "3 NEW CONTACTS…") starts flush at its box's left edge. Only when another run in the same
    // state ends exactly where this box begins is the slack in front; otherwise it is trailing.
    static void RestoreLeadingSpace(TextMeshProUGUI t, Rec r, string text)
    {
        float cell = t.GetPreferredValues("0").x;
        float slack = r.box.width - t.GetPreferredValues(text).x;
        if (slack < 0.8f * cell) return;
        bool abutted = ((JArray)layout["text"]).Any(o => (string)o["state"] == r.state && o != r.j
            && Mathf.Abs((float)o["x"] + (float)o["w"] - r.box.xMin) <= 1.5f
            && Mathf.Abs((float)o["y"] + (float)o["h"] / 2f - r.box.center.y) <= r.box.height / 2f);
        if (!abutted) return;
        // all the slack is in front, so the run ends at the box's right edge
        t.alignment = TextAlignmentOptions.Right;
        Count("text: leading space restored (right-aligned)");
    }

    static Material ThickMaterial(TMP_FontAsset fa)
    {
        string path = Gen + "/Fonts/" + fa.name + " Small.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) { mat = new Material(fa.material) { name = fa.name + " Small" }; AssetDatabase.CreateAsset(mat, path); }
        mat.CopyPropertiesFromMaterial(fa.material);
        mat.SetFloat("_FaceDilate", ThickenDilate);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // The board's multi-line labels were separate lines joined into one record. Rebuild the breaks:
    // the fewest lines whose widest line fits the measured width, choosing the break whose widest
    // line is closest to it; then space the lines so they fill the measured height (CSS line boxes).
    static void BreakLines(TextMeshProUGUI t, string text, float w, float h, float size)
    {
        if (t.GetPreferredValues(text).x <= w + 1.5f) return; // one line in a tall line box
        var words = text.Split(' ');
        string best = null;
        float bestErr = float.MaxValue;
        int lines = 0;
        for (int n = 2; n <= Mathf.Min(4, words.Length) && best == null; n++)
        {
            foreach (var cut in Cuts(words.Length, n))
            {
                var parts = new List<string>();
                int from = 0;
                foreach (int c in cut.Concat(new[] { words.Length })) { parts.Add(string.Join(" ", words, from, c - from)); from = c; }
                float widest = parts.Max(p => t.GetPreferredValues(p).x);
                if (widest > w + 1.5f) continue;
                float err = Mathf.Abs(widest - w);
                if (err < bestErr) { bestErr = err; best = string.Join("\n", parts); lines = n; }
            }
        }
        if (best == null) { t.textWrappingMode = TextWrappingModes.Normal; Count("text: no clean break, wrapped"); return; }
        t.text = best;
        var fi = t.font.faceInfo;
        float advance = fi.lineHeight / fi.pointSize * size;
        t.lineSpacing = (h / lines - advance) / size * 100f;
        Count("text: multi-line rebuilt");
    }

    // Every way to cut `count` words into `n` non-empty runs (positions of the n-1 cuts).
    static IEnumerable<int[]> Cuts(int count, int n)
    {
        if (n == 1) { yield return new int[0]; yield break; }
        for (int first = 1; first <= count - n + 1; first++)
            foreach (var rest in Cuts(count - first, n - 1))
                yield return new[] { first }.Concat(rest.Select(x => x + first)).ToArray();
    }

    static void MakeDrawn(Group g, Rec r)
    {
        string fill = (string)r.j["fill"];
        var size = Unrotated(r);
        var c = r.box.center;
        string name = r.name;
        string tok = fill;
        float alpha = (float)r.j["alpha"];
        if (fill == "gradient") { name = "STAND-IN " + name; tok = StandInToken; alpha *= StandInAlpha; Count("stand-in: gradient fill"); }
        if ((bool)r.j["shadow"]) Count("not built: drawn shadow (no values in package)");
        var go = NewChild(g, name, new Rect(c.x - size.x / 2f, c.y - size.y / 2f, size.x, size.y), Rot(r), size);
        Fill(go, tok, alpha, (string)r.j["radius"]);
        if (fill == "gradient" && size.x > HideStandInOver && size.y > HideStandInOver)
        { go.SetActive(false); Count("stand-in hidden: large gradient"); }
        Count("built: drawn");
    }

    static void Fill(GameObject go, string token, float alpha, string radius)
    {
        Sprite s = null;
        if (radius == "50%") s = ShapeSprite("circle");
        else
        {
            // "4px 0px 0px 4px" (one record) is built with 4 px on every corner.
            var m = Regex.Match(radius ?? "", @"^(\d+)px");
            int px = m.Success ? int.Parse(m.Groups[1].Value) : 0;
            if (px > 0) s = ShapeSprite("round_" + px);
            if (px > 0 && s == null) { Count("radius with no shape: " + px + "px"); }
        }
        var img = Img(go, s, Hex(token, alpha), false);
        if (s != null && s.border != Vector4.zero) img.type = Image.Type.Sliced;
    }

    // ---- geometry ------------------------------------------------------------------------

    static Rect Box(JToken j) => new Rect((float)j["x"], (float)j["y"], (float)j["w"], (float)j["h"]);

    static bool Contains(Rect a, Rect b)
    {
        const float e = 0.5f;
        return a.xMin <= b.xMin + e && a.yMin <= b.yMin + e && a.xMax >= b.xMax - e && a.yMax >= b.yMax - e
            && a.width * a.height > b.width * b.height + 0.5f;
    }

    static bool Inside(Rect inner, Rect outer, float tol) =>
        inner.xMin >= outer.xMin - tol && inner.yMin >= outer.yMin - tol && inner.xMax <= outer.xMax + tol && inner.yMax <= outer.yMax + tol;

    static float Rot(Rec r) => r.j["rotation"] == null ? 0f : -(float)r.j["rotation"]; // CSS clockwise -> Unity z

    // Unrotated size of a rotated record. layout.json gives "size" for knob pointers and ticks;
    // otherwise solve the bounding box (README: rotated records carry the rotated bounding box).
    static Vector2 Unrotated(Rec r)
    {
        var sz = r.j["size"] as JArray;
        if (sz != null) return new Vector2((float)sz[0], (float)sz[1]);
        float deg = r.j["rotation"] == null ? 0 : (float)r.j["rotation"];
        float a = Mathf.Abs(Mathf.Cos(deg * Mathf.Deg2Rad)), s = Mathf.Abs(Mathf.Sin(deg * Mathf.Deg2Rad));
        float W = r.box.width, H = r.box.height;
        if (s < 1e-3f) return new Vector2(W, H);
        if (a < 1e-3f) return new Vector2(H, W);
        float d = a * a - s * s;
        if (Mathf.Abs(d) < 0.05f) { float side = W / (a + s); return new Vector2(side, side); }
        return new Vector2((W * a - H * s) / d, (H * a - W * s) / d);
    }

    // ---- rockers (Step 40) ----------------------------------------------------------------

    // UIStateSpec §9.1 (lines 1162–1208): the pad's top eases on cubic-bezier(.3,1.5,.5,1) and its
    // background on ease. Pad boxes were measured in the art: image px from the top-left, the 14 px
    // margin included, wide and tall enough for the thrown pad's glow. Travel is the art's: the
    // 30x38 thrown pad sits 11 px lower, where the spec says top 3 → 13 (10). The art wins, so the
    // slide lands exactly on the baked pad.
    static readonly (string name, float seconds, Rect pad, float travel)[] RockerFamilies =
    {
        ("rocker_30x38", 0.14f, new Rect(15, 17, 28, 23), 11),
        ("rocker_24x32", 0.14f, new Rect(15, 17, 22, 19), 9),
        ("rocker_22x30", 0.12f, new Rect(15, 16, 20, 18), 9),
        ("rocker_reboot", 0.12f, new Rect(15, 16, 88, 19), 9),
    };

    // Every clickable rocker gets a cover and two pad cut-outs, all off until a pad moves.
    static void WireRockers(RectTransform root)
    {
        var so = new SerializedObject(root.gameObject.AddComponent<HandoffRockers>());
        var fams = so.FindProperty("families");
        fams.arraySize = RockerFamilies.Length;
        for (int i = 0; i < RockerFamilies.Length; i++)
        {
            var f = RockerFamilies[i];
            var e = fams.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("name").stringValue = f.name;
            e.FindPropertyRelative("seconds").floatValue = f.seconds;
            e.FindPropertyRelative("pad").rectValue = f.pad;
            e.FindPropertyRelative("travel").floatValue = f.travel;
            e.FindPropertyRelative("thrown").objectReferenceValue = LoadSprite(f.name + "_thrown");
            e.FindPropertyRelative("locked").objectReferenceValue = LoadSprite(f.name + "_locked");
        }
        // the cover is the well floor (tokens: sprite.well.01, sampled as the floor behind the pad)
        var well = Hex("sprite.well.01", 1f);
        var found = new List<(HandoffHit hit, Image img, int fam)>();
        foreach (var h in root.GetComponentsInChildren<HandoffHit>(true))
        {
            var img = h.GetComponent<Image>();
            if (!img || !img.sprite) continue;
            int fam = System.Array.FindIndex(RockerFamilies, f => img.sprite.name.StartsWith(f.name + "_"));
            if (fam >= 0) found.Add((h, img, fam));
        }
        var list = so.FindProperty("rockers");
        list.arraySize = found.Count;
        for (int i = 0; i < found.Count; i++)
        {
            var (hit, img, fam) = found[i];
            var f = RockerFamilies[fam];
            var rt = (RectTransform)img.transform;
            var e = list.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("image").objectReferenceValue = img;
            e.FindPropertyRelative("family").intValue = fam;
            e.FindPropertyRelative("action").stringValue = hit.action;
            e.FindPropertyRelative("arg").stringValue = hit.arg ?? "";
            e.FindPropertyRelative("cover").objectReferenceValue = RockerLayer<Image>(rt, "Pad cover", new Rect(f.pad.x, f.pad.y, f.pad.width, f.pad.height + f.travel), well);
            e.FindPropertyRelative("oldPad").objectReferenceValue = RockerLayer<RawImage>(rt, "Pad (old)", f.pad, Color.white);
            e.FindPropertyRelative("newPad").objectReferenceValue = RockerLayer<RawImage>(rt, "Pad (new)", f.pad, Color.white);
            Count("built: rocker (cover + two pads)");
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // A layer on a rocker image, placed in that image's px from its top-left; off until used.
    static T RockerLayer<T>(RectTransform rocker, string name, Rect box, Color c) where T : Graphic
    {
        var rt = NewRect(name, rocker);
        TopLeft(rt, box, Vector2.zero);
        var g = rt.gameObject.AddComponent<T>();
        g.color = c;
        g.raycastTarget = false;
        g.enabled = false;
        return g;
    }

    // ---- sprites, margins, colours -------------------------------------------------------

    static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/sprites/@2x/" + name + ".png")
            ?? AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/sprites/" + name + ".png");
    }

    static Sprite ShapeSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Gen + "/Shapes/" + name + ".png");

    // Shadow margin around the element, in 1x px: (left, top, right, bottom).
    // 1. the per-side table above; 2. "N px [shadow|glow] margin" in the asset-map notes;
    // 3. an exact-match record's image size minus element size; 4. none.
    static Vector4 Margin(string sprite, Sprite s)
    {
        Vector4 m;
        if (marginCache.TryGetValue(sprite, out m)) return m;
        string how;
        float pad;
        if (SideMargins.TryGetValue(sprite, out m)) how = "table";
        else if (MeasuredMargins.TryGetValue(sprite, out pad)) { m = new Vector4(pad, pad, pad, pad); how = "measured"; }
        else
        {
            Dictionary<string, string> row;
            var hit = assetMap.TryGetValue("sprites/" + sprite + ".png", out row)
                ? Regex.Match(row["notes"], @"(\d+(?:\.\d+)?) px (?:shadow |glow )?margin") : Match.Empty;
            if (hit.Success) { float v = float.Parse(hit.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture); m = new Vector4(v, v, v, v); how = "notes"; }
            else
            {
                float imgW = s.rect.width / (s.pixelsPerUnit / 100f), imgH = s.rect.height / (s.pixelsPerUnit / 100f);
                var exact = ((JArray)layout["sprites"]).FirstOrDefault(j => (string)j["sprite"] == sprite && (string)j["match"] == "exact" && j["size"] == null && (float)j["rotation"] == 0f);
                float mx = exact == null ? 0 : (imgW - (float)exact["w"]) / 2f, my = exact == null ? 0 : (imgH - (float)exact["h"]) / 2f;
                // only for non-sliced sprites, and only when both axes agree (D1)
                if (exact != null && s.border == Vector4.zero && mx > 0.25f && Mathf.Abs(mx - my) <= 0.5f)
                {
                    m = new Vector4(mx, my, mx, my); how = "image minus element";
                }
                else { m = Vector4.zero; how = "none"; }
            }
        }
        marginCache[sprite] = m;
        report.AppendLine("margin " + sprite + " = " + m + " (" + how + ")");
        return m;
    }

    static Color Tint(Rec r, string sprite)
    {
        if (r.j["tint"] != null && r.j["tint"].Type == JTokenType.String)
            return Hex((string)r.j["tint"], r.j["alpha"] != null ? (float)r.j["alpha"] : 1f);
        Dictionary<string, string> row;
        if (assetMap.TryGetValue("sprites/" + sprite + ".png", out row))
        {
            string tok = row["tint_token"];
            if (tok != "none" && tok != "state" && tokens[tok] != null) return Hex(tok, 1f);
        }
        return Color.white;
    }

    static Color Hex(string token, float alpha)
    {
        var t = tokens[token];
        Color c;
        if (t == null || !ColorUtility.TryParseHtmlString((string)t["hex"], out c))
        {
            Count("UNKNOWN colour token: " + token);
            c = Color.magenta;
        }
        // Gamma board: the canvas linearises vertex colours once, so pre-encode them; what reaches
        // the gamma-space blend is then the token's sRGB value. Alpha is never converted.
        c = c.gamma;
        c.a = alpha;
        return c;
    }

    // ---- objects -------------------------------------------------------------------------

    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.localScale = Vector3.one; // BUILD_LOG trap 3: children of a Screen Space-Camera canvas
        return rt;
    }

    // A child group whose own rect is `area` (board px); its children keep board coordinates.
    static Group Child(Group parent, string name, Rect area, bool clip)
    {
        var rt = NewRect(name, parent.rt);
        TopLeft(rt, area, parent.origin);
        if (clip) rt.gameObject.AddComponent<RectMask2D>();
        return new Group { rt = rt, origin = area.position };
    }

    // `size` set: placed by centre with that size, rotated about the centre. Otherwise top-left.
    static GameObject NewChild(Group g, string name, Rect box, float rotZ, Vector2? size)
    {
        var rt = NewRect(name, g.rt);
        if (size == null && rotZ == 0f) TopLeft(rt, box, g.origin);
        else
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size ?? box.size;
            rt.anchoredPosition = new Vector2(box.center.x - g.origin.x, -(box.center.y - g.origin.y));
            rt.localEulerAngles = new Vector3(0, 0, rotZ);
        }
        return rt.gameObject;
    }

    static void TopLeft(RectTransform rt, Rect box, Vector2 origin)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(box.x - origin.x, -(box.y - origin.y));
        rt.sizeDelta = box.size;
    }

    static Image Img(GameObject go, Sprite s, Color c, bool raycast)
    {
        var img = go.AddComponent<Image>();
        img.sprite = s;
        img.color = c;
        img.raycastTarget = raycast;
        return img;
    }

    static void Missing(Rec r) { Count("MISSING sprite"); report.AppendLine("missing sprite: " + r.name); }
    static void Count(string k) { int n; counts.TryGetValue(k, out n); counts[k] = n + 1; }
    static string Short(string s) => s == null ? "" : (s.Length > 24 ? s.Substring(0, 24) : s);
}
