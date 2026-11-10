// =================================================================================================
// StarwolfIntroController.cs
//
// What it does:
// This draws the green screen of blocky lit squares. It is used for the intro and the title menu.
// It shows the INITIALISING loading line and title cards such as MISSION ONE. It is in the
// 00_Start, 01_Briefing, SaveSlots and SectorMap scenes.
//
// How it works:
// - It has three ways to play. Full intro: SECTION 36 flickers in square by square and stays.
//   Then it turns into the title and cuts to a three-row menu. INITIALISING only: types that line,
//   then says it is finished. Title only: types the title in and keeps it on screen, with no menu.
//   SECTION 36 plays once per game. Back at the title later, only the title flickers in from
//   nothing, then the menu shows.
// - At start it reads glyphs.json and the hum sound from the intro's Resources folder. It asks
//   PhosphorData for a picture of each word. In the full intro, SECTION 36 and the title are
//   sized to one shared box.
// - It builds a full-screen layer with a black backing and a 16:9 display. It starts the hardware
//   hum, fading in over 0.6 seconds.
// - If anything fails at start, it writes the error to the Console and switches off. In
//   INITIALISING mode it says it is finished at once, and the game moves on.
// - Every frame it draws a 1920 by 1080 picture through the PhosphorDisplay shader. Its clock runs
//   only while the picture is on screen. A slow first frame cannot skip the SECTION 36 flicker.
// - Words type in from left to right over half their time, then stay. The times for each part are
//   set in the Inspector.
// - Menu: Up, Down and Tab move between rows. Enter or Space picks. The mouse can point and
//   click. The second row is skipped when it is switched off.
// - Picking a row runs the New Game, second row or Settings action set in the scene.
// - An option adds small dim text in the four corners and the CLEARANCE LEVEL squares. The
//   INITIALISING screens in 01_Briefing and SectorMap use it. The title menu shows a tiny dim
//   watermark in the bottom-right corner. Its text, size, colour and place are set in the Inspector.
// - Other scripts can restart the intro, skip to the menu, or hide and show the menu.
//
// Technical:
// - Namespace Starwolf.Intro. Modes: initialisingOnly, titleOnly, or neither for the full intro.
// - Files: glyphData and hardwareHum fall back to Resources/StarwolfIntro. displayShader must be
//   set to Starwolf/PhosphorDisplay.
// - Awake builds six masks: SECTION 36, INITIALISING..., the title, NEW GAME, the second row and
//   SETTINGS. They go to the shader's _Ident to _Settings.
// - ApplyStaticUniforms() runs every frame. A shader reload resets the material, and this puts the
//   masks and colours back.
// - The picture is a 1920 x 1080 ARGBHalf linear render texture, drawn with Graphics.Blit.
//   Trap: an sRGB texture turns the near-black field mid green.
// - The clock uses Time.unscaledDeltaTime. While a word flickers, one frame adds at most 1/30 s.
// - The static introSeen flag remembers SECTION 36 has played. It resets when play mode starts.
// - Scene events: finished, newGame, loadGame and settings, wired in each scene. ScreenFlow
//   methods are the usual targets.
// - Menu keys and clicks are read in OnGUI. PlaytestRunner calls Activate(1) and reads MenuVisible
//   and Elapsed. It also adds a hidden INITIALISING copy with no shader, to test the failure path.
// - RestartIntro, SkipToMenu, HideMenu, ShowMenu, SetLoadGameAvailable and SeekPreview are public.
//   No script calls them.
// =================================================================================================

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Starwolf.Intro
{
    [DisallowMultipleComponent]
    public sealed class StarwolfIntroController : MonoBehaviour
    {
        [Header("Shared assets")]
        public TextAsset glyphData;
        public Shader displayShader;
        public AudioClip hardwareHum;
        [Header("Timing in seconds")]
        [Min(0.1f)] public float sectionDuration = 2.5f;
        [Min(0.1f)] public float initialisingDuration = 1.2f;
        [Min(0.1f)] public float titleDuration = 2.0f;
        [Tooltip("Owner 2026-10-24: INITIALISING... and the title-only pages type in left to right over this share of their time, then stay. No pull-back.")]
        [Range(0.1f, 1f)] public float typeShare = 0.5f;
        [Tooltip("Owner 2026-11-09: in the full intro, SECTION 36 flickers into being, square by square, over this many seconds.")]
        [Min(0.05f)] public float flickerInSeconds = 0.4f;
        [Tooltip("Owner 2026-11-09: then SECTION 36 turns into STARWOLF over this many seconds. Squares lit in both words stay lit; the rest flicker over.")]
        [Min(0.05f)] public float changeSeconds = 0.6f;
        [Tooltip("Owner 2026-11-09: while a word flickers in or over, thin bands slip sideways and the light smears and grains along them. 0 = none, 1 = strong.")]
        [Range(0f, 2f)] public float diffusion = 0.7f;
        [Tooltip("Owner 2026-10-24: the intro is SECTION 36, STARWOLF, then the menu. INITIALISING... plays on its own after the beta page: tick this there, and it types that line, holds, then calls finished.")]
        public bool initialisingOnly;
        [Tooltip("Owner 2026-10-26: the welcome page after NEW GAME. Types the title in, then holds it on the phosphor screen with no menu; that page's own text draws on top.")]
        public bool titleOnly;
        public string titleText = "STARWOLF";
        [Tooltip("The menu's second row (owner 2026-11-08: the title shows LEVEL LIST there).")]
        public string secondRowText = "LOAD GAME";
        [Tooltip("Moves the title up (pixels on the 1920x1080 screen).")]
        public float titleOffset;
        [Tooltip("Overlay sort order of the phosphor screen. Lower it to let another canvas draw on top.")]
        public int canvasOrder = short.MaxValue;
        public UnityEvent finished = new UnityEvent();
        [Range(0, 0.3f)] public float humVolume = 0.12f;
        [Header("Connect these to the game's own systems")]
        public UnityEvent newGame = new UnityEvent();
        public UnityEvent loadGame = new UnityEvent();
        public UnityEvent settings = new UnityEvent();
        public bool loadGameAvailable = true;
        [Header("Loading screen corners")]
        [Tooltip("Owner 2026-11-10: small dim text in the four corners and the CLEARANCE LEVEL squares, as in the SECTION 36 reference picture (References/Company Logo.png). Ticked on INITIALISING in 01_Briefing.")]
        public bool cornerText;
        [TextArea] public string topLeft = "UC-FLEET\n// CLASSIFIED";
        [TextArea] public string topRight = "S36-PR-001\nREV A\n1993.11.04"; // owner 2026-11-10: S36 to match SECTION 36 (the reference says S38)
        [TextArea] public string bottomLeft = "RESEARCH   /   DEVELOPMENT   /   OPERATIONS";
        [TextArea] public string bottomRight = "CLEARANCE LEVEL 3";
        [Tooltip("How many of the five clearance squares under the bottom-right text are lit. The rest are hollow.")]
        [Range(0, 5)] public int clearanceLit = 3;
        [Tooltip("Share Tech Mono: ShareTechMono-Regular SDF.")]
        public TMP_FontAsset cornerFont;
        [Tooltip("Leave empty: the font's own material gives thin strokes, like the reference (2026-11-10). The Phosphor Glow material makes them too thick.")]
        public Material cornerMaterial;
        [Tooltip("The reference picture's corner text, measured: #73E18A, a little see-through.")]
        public Color cornerColour = new Color(0.45f, 0.88f, 0.54f, 0.8f);
        [Tooltip("Text size, in pixels on the 1920x1080 screen.")]
        [Min(1)] public float cornerSize = 24;
        [Header("Menu")]
        [Tooltip("Owner 2026-11-10: tiny dim text in the bottom-right corner of the title menu. Empty: none.")]
        public string watermark = "Shagun Shah";
        [Tooltip("The watermark's text size, in pixels on the 1920x1080 screen.")]
        [Min(1)] public float watermarkSize = 10;
        public Color watermarkColour = new Color(0.45f, 0.88f, 0.54f, 0.5f);
        public event Action<int> MenuAction;
        public bool MenuVisible => !initialisingOnly && !titleOnly && clock >= TotalDuration;
        public float TotalDuration => initialisingOnly ? Mathf.Max(.1f, initialisingDuration) : Mathf.Max(.1f, sectionDuration) + Mathf.Max(.1f, titleDuration);
        public float Elapsed => clock;
        public int GridPitchPixels { get; private set; }

        Material material;
        RenderTexture target;
        Canvas canvas;
        RawImage display;
        RectTransform corners;
        TextMeshProUGUI watermarkText;
        // Owner 2026-11-10: SECTION 36 plays once, when the game starts. Coming back to the title later (MAIN MENU, or
        // the end of a fight) shows only STARWOLF flickering in from nothing, then the menu.
        static bool introSeen;
        bool returning;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetIntroSeen() { introSeen = false; }
        Texture2D[] masks;
        AudioSource source;
        PhosphorData data;
        float clock;
        int selected;
        bool suspended, done;
        static readonly string[] Names = { "New Game", "Load Game", "Settings" };
        static readonly string[] MaskProperties = { "_Ident", "_Boot", "_Title", "_NewGame", "_LoadGame", "_Settings" };

        void Awake()
        {
            try
            {
                if (!glyphData) glyphData = Resources.Load<TextAsset>("StarwolfIntro/glyphs");
                if (!hardwareHum) hardwareHum = Resources.Load<AudioClip>("StarwolfIntro/hardware-hum");
                data = PhosphorData.Load(glyphData);
                if (!displayShader) throw new InvalidOperationException("Assign the supplied PhosphorDisplay shader; use the scene builder.");
                material = new Material(displayShader) { hideFlags = HideFlags.HideAndDontSave };
                // Owner 2026-11-09: in the full intro SECTION 36 and the title are sized to each other (the
                // narrower one's letters spread out), so they share one box, centred, and the S's line up.
                int fit = 0;
                if (!initialisingOnly && !titleOnly)
                {
                    data.Cells("SECTION 36", true, out var a); data.Cells(titleText, true, out var b);
                    fit = Mathf.Max(a.x, b.x);
                }
                masks = new[] { data.MakeMask("SECTION 36", true, fit), data.MakeMask("INITIALISING...", false),
                    data.MakeMask(titleText, true, fit), data.MakeMask("NEW GAME", false),
                    data.MakeMask(secondRowText, false), data.MakeMask("SETTINGS", false) };
                if (!initialisingOnly && !titleOnly)
                {
                    returning = introSeen;
                    introSeen = true;
                    if (returning)
                    {
                        // The title flickers over from an empty picture the size of SECTION 36's box, so the box itself
                        // flickers in too. The clock starts where SECTION 36 would end.
                        var blank = new Texture2D(masks[0].width, masks[0].height, TextureFormat.RGBA32, false, true)
                            { name = "Empty", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                        blank.SetPixels32(new Color32[masks[0].width * masks[0].height]); blank.Apply(false, true);
                        Destroy(masks[0]); masks[0] = blank;
                        clock = Mathf.Max(.1f, sectionDuration);
                    }
                }
                GridPitchPixels = data.Pitch;
                ApplyStaticUniforms();
                BuildCanvas();
                source = gameObject.AddComponent<AudioSource>();
                source.clip = hardwareHum; source.loop = true; source.playOnAwake = false;
                source.spatialBlend = 0; source.volume = 0;
                if (hardwareHum) source.Play();
                else Debug.LogWarning("Starwolf: hardware hum asset missing.", this);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
                enabled = false;
                // Step 83: INITIALISING... can't play, so the game goes straight on to the board
                if (initialisingOnly && !done) { done = true; finished.Invoke(); }
            }
        }
        // The picture goes to the screen through a Canvas, not OnGUI. OnGUI encoded it twice, which
        // paled the text and rounded the dark green field to black (Step 70, measured). A black
        // backing fills the bars; the display keeps 16:9, the same rectangle ScreenRect() gives.
        void BuildCanvas()
        {
            var root = new GameObject("Phosphor screen", typeof(Canvas), typeof(RawImage));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = canvasOrder;
            canvas.enabled = false;
            var backing = root.GetComponent<RawImage>();
            backing.color = Color.black; backing.raycastTarget = false;
            display = new GameObject("Display", typeof(RawImage), typeof(AspectRatioFitter)).GetComponent<RawImage>();
            display.transform.SetParent(root.transform, false);
            display.raycastTarget = false;
            var fitter = display.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1920f / 1080f;
            if (cornerText || (!initialisingOnly && !titleOnly && !string.IsNullOrEmpty(watermark))) BuildCorners();
        }
        // Owner 2026-11-10: the reference picture's small corner text, in Share Tech Mono, dim. Laid out on the
        // 1920x1080 screen and scaled with the display (Update), so it stays in the corners of the 16:9 picture.
        // Places measured from References/Company Logo.png. The clearance squares are drawn by the shader.
        void BuildCorners()
        {
            corners = new GameObject("Corners", typeof(RectTransform)).GetComponent<RectTransform>();
            corners.SetParent(display.transform, false);
            corners.sizeDelta = new Vector2(1920, 1080);
            if (cornerText)
            {
                Corner(topLeft, new Vector2(0, 1), new Vector2(110, -95), TextAlignmentOptions.TopLeft);
                Corner(topRight, new Vector2(1, 1), new Vector2(-113, -95), TextAlignmentOptions.TopRight);
                Corner(bottomLeft, new Vector2(0, 0), new Vector2(116, 164), TextAlignmentOptions.BottomLeft);
                Corner(bottomRight, new Vector2(1, 0), new Vector2(-121, 195), TextAlignmentOptions.BottomRight);
            }
            if (!initialisingOnly && !titleOnly && !string.IsNullOrEmpty(watermark))
            {
                // Owner 2026-11-10: a tiny watermark in the bottom-right corner, on the menu only (Update).
                watermarkText = Corner(watermark, new Vector2(1, 0), new Vector2(-80, 60), TextAlignmentOptions.BottomRight);
                watermarkText.fontSize = watermarkSize; watermarkText.color = watermarkColour; watermarkText.characterSpacing = 2;
                watermarkText.enabled = false;
            }
        }
        TextMeshProUGUI Corner(string text, Vector2 corner, Vector2 offset, TextAlignmentOptions alignment)
        {
            var t = new GameObject("Corner text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            var rect = t.rectTransform;
            rect.SetParent(corners, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = corner;
            rect.sizeDelta = new Vector2(900, 200);
            rect.anchoredPosition = offset;
            if (cornerFont) t.font = cornerFont;
            if (cornerMaterial) t.fontSharedMaterial = cornerMaterial;
            t.text = text; t.fontSize = cornerSize; t.color = cornerColour; t.alignment = alignment;
            t.textWrappingMode = TextWrappingModes.NoWrap; t.characterSpacing = 4; t.raycastTarget = false;
            return t;
        }
        void Update()
        {
            if (canvas) canvas.enabled = !suspended && target;
            if (corners) corners.localScale = Vector3.one * (display.rectTransform.rect.width / 1920f);
            if (watermarkText) watermarkText.enabled = MenuVisible;
            if (suspended || !material) return;
            // Owner 2026-11-02: the first frame can take 6 s; it must not use up SECTION 36 unseen. The clock starts
            // once the picture shows, and a slow frame can't skip SECTION 36's quick flicker-in (2026-11-09).
            float s = Mathf.Max(.1f, sectionDuration);
            bool flickering = !initialisingOnly && !titleOnly && (clock < flickerInSeconds || (returning && clock < s + changeSeconds));
            if (canvas && canvas.enabled) clock += Mathf.Min(Time.unscaledDeltaTime, flickering ? 1f / 30f : Time.maximumDeltaTime);
            if (source) source.volume = humVolume * Mathf.Clamp01(clock / .6f);
            DrawFrame();
            if (initialisingOnly && !done && clock >= TotalDuration) { done = true; finished.Invoke(); }
        }
        public void RestartIntro() { clock = returning ? Mathf.Max(.1f, sectionDuration) : 0; selected = 0; suspended = false; done = false; if (source && !source.isPlaying) source.Play(); }
        public void SkipToMenu() { clock = TotalDuration; }
        // Game integration calls this after New/Load Game, or while its Settings panel owns input.
        public void HideMenu() { suspended = true; if (source) source.Pause(); }
        public void ShowMenu() { suspended = false; clock = TotalDuration; if (source) source.UnPause(); }
        public void SetLoadGameAvailable(bool available) { loadGameAvailable = available; if (!available && selected == 1) selected = 0; }
        public void SeekPreview(float seconds) { clock = Mathf.Max(0, seconds); DrawFrame(); }
        public RenderTexture PreviewTexture => target;
        // Re-applied every frame rather than only in Awake. A shader reload resets the material to
        // the defaults in the Properties block, the mask sizes come back zero, mask() divides by
        // zero and the whole intro goes black until the next play. Nine assignments a frame is
        // cheaper than a title sequence that silently stops drawing.
        void ApplyStaticUniforms()
        {
            if (!material || masks == null || data == null) return;
            for (int i = 0; i < masks.Length; i++)
            {
                if (!masks[i]) continue;
                material.SetTexture(MaskProperties[i], masks[i]);
                material.SetVector(MaskProperties[i] + "Size", new Vector4(masks[i].width, masks[i].height, 0, 0));
            }
            // _Phosphor is a declared Color property, so in Linear colour space Unity converts it
            // from sRGB itself. Do not also call .linear or it converts twice. The same holds for
            // _Background, the dark green field (owner 2026-10-25).
            ColorUtility.TryParseHtmlString(data.phosphor, out var phosphor);
            ColorUtility.TryParseHtmlString(data.background, out var background);
            material.SetColor("_Phosphor", phosphor);
            material.SetColor("_Background", background);
            // Owner 2026-11-10: the reference's richer green. Each square fades to a deeper green at its edge,
            // and the glow round the squares is pure green (it was the paler square colour).
            ColorUtility.TryParseHtmlString(data.rim, out var rim);
            ColorUtility.TryParseHtmlString(data.glow, out var glow);
            material.SetColor("_Rim", rim);
            material.SetColor("_Glow", glow);
            material.SetFloat("_Corners", cornerText ? 1 : 0);
            material.SetFloat("_ClearanceLit", clearanceLit);
            material.SetFloat("_Pitch", GridPitchPixels);
            material.SetFloat("_TitleOffset", titleOffset);
            // The full intro's two words share one box, centred (Awake sizes them to each other); its left
            // edge sets the squares the flicker counts in.
            material.SetFloat("_Flicker", !initialisingOnly && !titleOnly ? 1 : 0);
            material.SetFloat("_Diffusion", diffusion);
            material.SetFloat("_Left", masks[0] ? -masks[0].width / 2f : 0f);
        }
        void DrawFrame()
        {
            if (!material) return;
            ApplyStaticUniforms();
            if (!target)
            {
                // Linear read/write on purpose. An sRGB target encodes on write and the backbuffer
                // encodes again, so the near-black CRT field came out mid green. See Known issue #6.
                // Half float, not ARGB32: the tube's unlit field sits a few thousandths above black
                // and 8 bits quantises that either to nothing or to a visible green step.
                target = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
                    { name = "Starwolf phosphor display", filterMode = FilterMode.Bilinear };
                target.Create();
                if (display) display.texture = target;
            }
            float s = Mathf.Max(.1f, sectionDuration), b = Mathf.Max(.1f, initialisingDuration);
            float t = Mathf.Max(.1f, titleDuration);
            // Owner 2026-11-09: SECTION 36 flickers in, stays, and flickers over into STARWOLF; then a
            // straight cut to the menu (stage 4), with no pull-back. INITIALISING... (stage 1) plays on its own.
            float stage = initialisingOnly ? 1 : titleOnly ? 2 : clock < s ? 0 : clock < s + t ? 2 : 4;
            float local = stage <= 1 || titleOnly ? clock : clock - s;
            float duration = stage == 1 ? b : stage == 0 ? s : t;
            material.SetFloat("_Stage", stage); material.SetFloat("_Clock", clock);
            material.SetFloat("_Reveal", stage < 3 ? Mathf.Clamp01(local / (duration * typeShare)) : 1f);
            material.SetFloat("_Progress", Mathf.Clamp01(local / duration));
            material.SetFloat("_Change", local);
            material.SetFloat("_ChangeSeconds", stage == 0 ? flickerInSeconds : changeSeconds);
            material.SetFloat("_Selected", selected); material.SetFloat("_LoadAvailable", loadGameAvailable ? 1 : 0);
            Graphics.Blit(Texture2D.blackTexture, target, material);
        }
        Rect ScreenRect()
        {
            float scale = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
            return new Rect((Screen.width - 1920 * scale) / 2, (Screen.height - 1080 * scale) / 2, 1920 * scale, 1080 * scale);
        }
        void OnGUI()
        {
            if (suspended || !target) return;
            // Input only; the picture is drawn by the canvas (BuildCanvas).
            int oldDepth = GUI.depth; GUI.depth = -1000;
            if (MenuVisible) HandleMenu(Event.current, ScreenRect());
            GUI.depth = oldDepth;
        }
        void HandleMenu(Event e, Rect screen)
        {
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.DownArrow || e.keyCode == KeyCode.Tab || e.keyCode == KeyCode.UpArrow)
                {
                    int direction = e.keyCode == KeyCode.UpArrow || (e.keyCode == KeyCode.Tab && e.shift) ? -1 : 1;
                    do { selected = (selected + direction + 3) % 3; } while (selected == 1 && !loadGameAvailable);
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Space)
                { Activate(selected); e.Use(); }
            }
            if (e.type != EventType.MouseMove && e.type != EventType.MouseDown) return;
            var p = (e.mousePosition - screen.position) * (1920f / screen.width);
            for (int i = 0; i < 3; i++)
            {
                var rect = new Rect(660, 404 + 112 * i, 600, 96);
                if (!rect.Contains(p) || (i == 1 && !loadGameAvailable)) continue;
                selected = i;
                if (e.type == EventType.MouseDown && e.button == 0) { Activate(i); e.Use(); }
            }
        }
        public void Activate(int index)
        {
            if (!MenuVisible || suspended || index < 0 || index > 2 || (index == 1 && !loadGameAvailable)) return;
            var action = index == 0 ? newGame : index == 1 ? loadGame : settings;
            action.Invoke(); MenuAction?.Invoke(index);
            #if UNITY_EDITOR
            if (action.GetPersistentEventCount() == 0 && MenuAction == null)
                Debug.Log("Starwolf menu: " + Names[index] + ". Connect this event to your game system.", this);
            #endif
        }
        void OnDisable() { if (source) source.Pause(); if (canvas) canvas.enabled = false; }
        void OnEnable() { if (source && !suspended) source.UnPause(); }
        void OnDestroy()
        {
            if (target) { target.Release(); Destroy(target); }
            if (material) Destroy(material);
            if (masks != null) foreach (var mask in masks) if (mask) Destroy(mask);
            if (source) Destroy(source);
        }
    }
}
