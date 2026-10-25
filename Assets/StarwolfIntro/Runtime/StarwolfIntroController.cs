using System;
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
        [Tooltip("Owner 2026-10-24: each line types in left to right over this share of its time, holds, then cuts straight to the next. No pull-back.")]
        [Range(0.1f, 1f)] public float typeShare = 0.5f;
        [Tooltip("Owner 2026-10-24: the intro is SECTION 36, STARWOLF, then the menu. INITIALISING... plays on its own after the beta page: tick this there, and it types that line, holds, then calls finished.")]
        public bool initialisingOnly;
        [Tooltip("Owner 2026-10-26: the welcome page after NEW GAME. Types the title in, then holds it on the phosphor screen with no menu; that page's own text draws on top.")]
        public bool titleOnly;
        public string titleText = "STARWOLF";
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
        public event Action<int> MenuAction;
        public bool MenuVisible => !initialisingOnly && !titleOnly && clock >= TotalDuration;
        public float TotalDuration => initialisingOnly ? Mathf.Max(.1f, initialisingDuration) : Mathf.Max(.1f, sectionDuration) + Mathf.Max(.1f, titleDuration);
        public float Elapsed => clock;
        public int GridPitchPixels { get; private set; }

        Material material;
        RenderTexture target;
        Canvas canvas;
        RawImage display;
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
                masks = new[] { data.MakeMask("SECTION 36", true), data.MakeMask("INITIALISING...", false),
                    data.MakeMask(titleText, true), data.MakeMask("NEW GAME", false),
                    data.MakeMask("LOAD GAME", false), data.MakeMask("SETTINGS", false) };
                GridPitchPixels = data.Pitch;
                ApplyStaticUniforms();
                BuildCanvas();
                source = gameObject.AddComponent<AudioSource>();
                source.clip = hardwareHum; source.loop = true; source.playOnAwake = false;
                source.spatialBlend = 0; source.volume = 0;
                if (hardwareHum) source.Play();
                else Debug.LogWarning("Starwolf: hardware hum asset missing.", this);
            }
            catch (Exception e) { Debug.LogException(e, this); enabled = false; }
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
        }
        void Update()
        {
            if (canvas) canvas.enabled = !suspended && target;
            if (suspended || !material) return;
            clock += Time.unscaledDeltaTime;
            if (source) source.volume = humVolume * Mathf.Clamp01(clock / .6f);
            DrawFrame();
            if (initialisingOnly && !done && clock >= TotalDuration) { done = true; finished.Invoke(); }
        }
        public void RestartIntro() { clock = 0; selected = 0; suspended = false; done = false; if (source && !source.isPlaying) source.Play(); }
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
            material.SetFloat("_Pitch", GridPitchPixels);
            material.SetFloat("_TitleOffset", titleOffset);
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
            // Owner 2026-10-24: SECTION 36, then STARWOLF, each typed in place, then a straight cut to
            // the menu (stage 4), with no pull-back. INITIALISING... (stage 1) plays on its own.
            float stage = initialisingOnly ? 1 : titleOnly ? 2 : clock < s ? 0 : clock < s + t ? 2 : 4;
            float local = stage <= 1 || titleOnly ? clock : clock - s;
            float duration = stage == 1 ? b : stage == 0 ? s : t;
            material.SetFloat("_Stage", stage); material.SetFloat("_Clock", clock);
            material.SetFloat("_Reveal", stage < 3 ? Mathf.Clamp01(local / (duration * typeShare)) : 1f);
            material.SetFloat("_Progress", Mathf.Clamp01(local / duration));
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
