using System;
using UnityEngine;
using UnityEngine.Events;

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
        [Min(0.1f)] public float pullbackDuration = 3.0f;
        [Range(0, 0.3f)] public float humVolume = 0.12f;
        [Header("Connect these to the game's own systems")]
        public UnityEvent newGame = new UnityEvent();
        public UnityEvent loadGame = new UnityEvent();
        public UnityEvent settings = new UnityEvent();
        public bool loadGameAvailable = true;
        public event Action<int> MenuAction;
        public bool MenuVisible => clock >= TotalDuration;
        public float TotalDuration => Mathf.Max(.1f, sectionDuration) + Mathf.Max(.1f, initialisingDuration) + Mathf.Max(.1f, titleDuration) + Mathf.Max(.1f, pullbackDuration);
        public float Elapsed => clock;
        public Vector2 GridOriginPixels { get; private set; }
        public int GridPitchPixels { get; private set; }

        Material material;
        RenderTexture target;
        Texture2D[] masks;
        AudioSource source;
        PhosphorData data;
        float clock;
        int selected;
        bool suspended;
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
                    data.MakeMask("STARWOLF", true), data.MakeMask("NEW GAME", false),
                    data.MakeMask("LOAD GAME", false), data.MakeMask("SETTINGS", false) };
                GridPitchPixels = data.Pitch;
                GridOriginPixels = new Vector2(-masks[2].width * .5f, -masks[2].height * .5f);
                ApplyStaticUniforms();
                source = gameObject.AddComponent<AudioSource>();
                source.clip = hardwareHum; source.loop = true; source.playOnAwake = false;
                source.spatialBlend = 0; source.volume = 0;
                if (hardwareHum) source.Play();
                else Debug.LogWarning("Starwolf: hardware hum asset missing.", this);
            }
            catch (Exception e) { Debug.LogException(e, this); enabled = false; }
        }
        void Update()
        {
            if (suspended || !material) return;
            clock += Time.unscaledDeltaTime;
            if (source) source.volume = humVolume * Mathf.Clamp01(clock / .6f);
            DrawFrame();
        }
        public void RestartIntro() { clock = 0; selected = 0; suspended = false; if (source && !source.isPlaying) source.Play(); }
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
            // _Phosphor and _Background are declared Color properties, so in Linear colour space
            // Unity converts them from sRGB itself. Do not also call .linear or they convert twice.
            ColorUtility.TryParseHtmlString(data.phosphor, out var phosphor);
            ColorUtility.TryParseHtmlString(data.background, out var background);
            material.SetColor("_Phosphor", phosphor);
            material.SetColor("_Background", background);
            material.SetFloat("_Pitch", GridPitchPixels);
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
            }
            float s = Mathf.Max(.1f, sectionDuration), b = Mathf.Max(.1f, initialisingDuration);
            float t = Mathf.Max(.1f, titleDuration), z = Mathf.Max(.1f, pullbackDuration);
            float stage = clock < s ? 0 : clock < s + b ? 1 : clock < s + b + t ? 2 : clock < s + b + t + z ? 3 : 4;
            float local = stage == 0 ? clock : stage == 1 ? clock - s : stage == 2 ? clock - s - b : clock - s - b - t;
            float duration = stage == 0 ? s : stage == 1 ? b : stage == 2 ? t : z;
            float fade = stage <= 1 ? Mathf.Clamp01(local / .2f) * Mathf.Clamp01((duration - local) / .2f) : 1;
            // The grid is a flourish inside the pull-back only. It swells and goes again, so every
            // beat the player actually rests on — the logos and the menu — sits on a black field.
            material.SetFloat("_GridVisibility",
                stage > 2.5f && stage < 3.5f ? Mathf.Sin(Mathf.Clamp01(local / z) * Mathf.PI) : 0f);
            material.SetFloat("_Stage", stage); material.SetFloat("_Phase", local);
            material.SetFloat("_Fade", fade); material.SetFloat("_Clock", clock);
            material.SetFloat("_ZoomProgress", Mathf.Clamp01((clock - s - b - t) / z));
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
            int oldDepth = GUI.depth; GUI.depth = -1000;
            var screen = ScreenRect();
            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.blackTexture);
                GUI.DrawTexture(screen, target, ScaleMode.StretchToFill, false);
            }
            if (MenuVisible) HandleMenu(Event.current, screen);
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
        void OnDisable() { if (source) source.Pause(); }
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
