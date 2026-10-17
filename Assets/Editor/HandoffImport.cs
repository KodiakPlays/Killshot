using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Step 29. Prepares the Claude Design handoff for uGUI: sprite import settings and 9-slice
/// borders from asset-map.csv, one TMP font asset per .ttf, and the few white helper sprites the
/// board needs that the package does not ship (rounded fills, plot glow, vignette, scanlines).
/// Safe to re-run. Rules come from the handoff README, "Canvas and import".
/// See Assets/Scenes/Tactical/UI_INTEGRATION_LOG.md.
/// </summary>
public static class HandoffImport
{
    public const string Root = "Assets/UI2/Claude Design handoff/export";
    public const string Gen = "Assets/UI2/Generated";

    // Characters every font asset carries: printable ASCII plus the board's specials (README, Fonts).
    const string Specials = "—−°±·×▸▶◀";

    // Corner radii used by drawn fills in layout.json ("1px" .. "6px"); "50%" is the circle.
    static readonly int[] Radii = { 1, 2, 3, 4, 5, 6 };

    [MenuItem("Starwolf/Handoff/1 · Apply import settings")]
    public static void ApplyAll()
    {
        ImportSprites();
        MakeFonts();
        MakeShapes();
        AssetDatabase.SaveAssets();
        Debug.Log("[HandoffImport] done");
    }

    // ---- sprites -------------------------------------------------------------------------

    public static Dictionary<string, Dictionary<string, string>> ReadAssetMap()
    {
        var lines = File.ReadAllLines(Root + "/asset-map.csv");
        var head = SplitCsv(lines[0]);
        var map = new Dictionary<string, Dictionary<string, string>>();
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cells = SplitCsv(line);
            var row = new Dictionary<string, string>();
            for (int i = 0; i < head.Count && i < cells.Count; i++) row[head[i]] = cells[i];
            map[row["file"]] = row;
        }
        return map;
    }

    static List<string> SplitCsv(string line)
    {
        var outp = new List<string>();
        var cur = new System.Text.StringBuilder();
        bool quoted = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"') { cur.Append('"'); i++; }
                else if (c == '"') quoted = false;
                else cur.Append(c);
            }
            else if (c == '"') quoted = true;
            else if (c == ',') { outp.Add(cur.ToString()); cur.Clear(); }
            else cur.Append(c);
        }
        outp.Add(cur.ToString());
        return outp;
    }

    static void ImportSprites()
    {
        var map = ReadAssetMap();
        var pngs = Directory.GetFiles(Root, "*.png", SearchOption.AllDirectories)
            .Select(p => p.Replace('\\', '/')).ToList();

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var path in pngs)
            {
                var ti = (TextureImporter)AssetImporter.GetAtPath(path);
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                // README says sRGB on. Off here on purpose: the board blends in gamma space, like the
                // browser (owner, 2026-10-04), so sprites must reach the blend undecoded.
                // See GammaBoardPresenter.cs.
                ti.sRGBTexture = false;
                ti.alphaIsTransparency = true;
                ti.alphaSource = TextureImporterAlphaSource.FromInput;
                ti.filterMode = FilterMode.Bilinear;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.mipmapEnabled = false;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.maxTextureSize = 4096;
                ti.spritePixelsPerUnit = path.Contains("/@2x/") ? 200 : 100;

                var settings = new TextureImporterSettings();
                ti.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                ti.SetTextureSettings(settings);

                string rel = path.Substring(Root.Length + 1);
                Dictionary<string, string> row;
                ti.spriteBorder = map.TryGetValue(rel, out row)
                    ? new Vector4(F(row["nine_slice_left"]), F(row["nine_slice_bottom"]),
                                  F(row["nine_slice_right"]), F(row["nine_slice_top"]))
                    : Vector4.zero;
                ti.SaveAndReimport();
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }
        Debug.Log("[HandoffImport] sprites: " + pngs.Count);
    }

    static float F(string s) { float v; return float.TryParse(s, out v) ? v : 0f; }

    // ---- fonts ---------------------------------------------------------------------------

    static void MakeFonts()
    {
        Directory.CreateDirectory(Gen + "/Fonts");
        var ascii = new string(Enumerable.Range(32, 95).Select(i => (char)i).ToArray());
        var fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset");
        var symbols = SymbolsFallback();

        foreach (var ttf in Directory.GetFiles(Root + "/fonts", "*.ttf"))
        {
            string name = Path.GetFileNameWithoutExtension(ttf);
            string path = Gen + "/Fonts/" + name + " SDF.asset";
            var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fa == null)
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(ttf.Replace('\\', '/'));
                fa = TMP_FontAsset.CreateFontAsset(font, 72, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                                                   AtlasPopulationMode.Dynamic, true);
                fa.name = name + " SDF";
                AssetDatabase.CreateAsset(fa, path);
                fa.atlasTextures[0].name = name + " SDF Atlas";
                AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
                fa.material.name = name + " SDF Material";
                AssetDatabase.AddObjectToAsset(fa.material, fa);
            }
            string ignored;
            fa.TryAddCharacters(ascii + Specials, out ignored);
            // README: ▸ is not in Share Tech Mono, nor in TMP's Liberation fallback. Noto Sans Symbols 2
            // (OFL) supplies ▸ ▶ ◀ and goes first; Liberation still covers any other stray character.
            if (fa.fallbackFontAssetTable == null) fa.fallbackFontAssetTable = new List<TMP_FontAsset>();
            if (symbols != null && !fa.fallbackFontAssetTable.Contains(symbols))
                fa.fallbackFontAssetTable.Insert(0, symbols);
            if (fallback != null && !fa.fallbackFontAssetTable.Contains(fallback))
                fa.fallbackFontAssetTable.Add(fallback);
            EditorUtility.SetDirty(fa);
            string lacking = new string((ascii + Specials).Where(c => !fa.HasCharacter(c)).ToArray());
            Debug.Log("[HandoffImport] font " + name + ": " + fa.characterTable.Count + " characters" +
                      (lacking.Length > 0 ? ", not in this face: " + lacking : ""));
        }
    }

    // ▸ ▶ ◀ (HANDOFF_GAPS row 30): from Noto Sans Symbols 2, SIL OFL 1.1 (licence beside the .ttf).
    const string SymbolsTtf = "Assets/UI/Fonts/NotoSansSymbols2/NotoSansSymbols2-Regular.ttf";
    const string SymbolGlyphs = "\u25B8\u25B6\u25C0";

    static TMP_FontAsset SymbolsFallback()
    {
        string path = Gen + "/Fonts/NotoSansSymbols2 SDF.asset";
        var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (fa == null)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(SymbolsTtf);
            if (font == null) { Debug.LogWarning("[HandoffImport] missing " + SymbolsTtf); return null; }
            fa = TMP_FontAsset.CreateFontAsset(font, 72, 9, GlyphRenderMode.SDFAA, 256, 256,
                                               AtlasPopulationMode.Dynamic, true);
            fa.name = "NotoSansSymbols2 SDF";
            AssetDatabase.CreateAsset(fa, path);
            fa.atlasTextures[0].name = "NotoSansSymbols2 SDF Atlas";
            AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
            fa.material.name = "NotoSansSymbols2 SDF Material";
            AssetDatabase.AddObjectToAsset(fa.material, fa);
        }
        string missing;
        fa.TryAddCharacters(SymbolGlyphs, out missing);
        EditorUtility.SetDirty(fa);
        Debug.Log("[HandoffImport] symbols fallback: " + fa.characterTable.Count + " characters" +
                  (string.IsNullOrEmpty(missing) ? "" : ", missing " + missing));
        return fa;
    }

    // ---- generated white sprites ---------------------------------------------------------

    static void MakeShapes()
    {
        Directory.CreateDirectory(Gen + "/Shapes");
        const int ss = 4; // drawn at 4x, imported at 400 PPU, so they land at 1x and stay sharp at 2x
        foreach (int r in Radii)
        {
            int n = (2 * r + 2) * ss;
            float rr = r * ss;
            WriteSprite("round_" + r, n, n, (x, y) =>
            {
                // distance from the pixel centre to the nearest corner circle's centre
                float px = x + 0.5f, py = y + 0.5f;
                float dx = Mathf.Max(0, Mathf.Max(rr - px, px - (n - rr))), dy = Mathf.Max(0, Mathf.Max(rr - py, py - (n - rr)));
                return Mathf.Clamp01(rr + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
            }, 100 * ss, new Vector4(rr, rr, rr, rr));
        }
        WriteSprite("circle", 128, 128, (x, y) =>
        {
            float d = Mathf.Sqrt((x - 64) * (x - 64) + (y - 64) * (y - 64));
            return Mathf.Clamp01(64.5f - d);
        }, 100, Vector4.zero);
        // the periscope sight's circle mask (340 px aperture); 128 px stepped visibly when stretched
        WriteSprite("aperture_mask", 512, 512, (x, y) =>
        {
            float d = Mathf.Sqrt((x + 0.5f - 256) * (x + 0.5f - 256) + (y + 0.5f - 256) * (y + 0.5f - 256));
            return Mathf.Clamp01(256f - d);
        }, 100, Vector4.zero);

        // plot-grid.json: radial ellipse at 50% 48%, CSS farthest-corner sizing.
        // Glow: full at the centre, transparent at 68% (white tint) — alpha is set on the Image.
        WriteSprite("plot_glow", 256, 256, (x, y) => Mathf.Clamp01(1f - PlotEllipse(x, y, 256) / 0.68f), 100, Vector4.zero);
        // Vignette: clear to 56%, full at the edge (100%) — edge alpha is set on the Image.
        WriteSprite("plot_vignette", 256, 256, (x, y) => Mathf.Clamp01((PlotEllipse(x, y, 256) - 0.56f) / 0.44f), 100, Vector4.zero);
        // Scanlines: one 1 px dark line every 3 px. Tiled by the Image, black, alpha set on the Image.
        WriteSprite("scanline", 4, 3, (x, y) => y == 2 ? 1f : 0f, 100, Vector4.zero);
    }

    // CSS radial-gradient(ellipse at 50% 48%): farthest-side radii (0.5, 0.52) scaled by sqrt 2.
    // y is texture space (bottom-up), so 48% from the top is 52% from the bottom.
    static float PlotEllipse(int x, int y, int n)
    {
        float u = (x + 0.5f) / n, v = 1f - (y + 0.5f) / n;
        float a = (u - 0.5f) / (0.5f * 1.41421f), b = (v - 0.48f) / (0.52f * 1.41421f);
        return Mathf.Sqrt(a * a + b * b);
    }

    static void WriteSprite(string name, int w, int h, System.Func<int, int, float> alpha, float ppu, Vector4 border)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha(x, y)));
        string path = Gen + "/Shapes/" + name + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.sRGBTexture = false; // gamma-space board, as ImportSprites
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.filterMode = FilterMode.Bilinear;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.wrapMode = name == "scanline" ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        ti.spritePixelsPerUnit = ppu;
        ti.spriteBorder = border;
        var s = new TextureImporterSettings();
        ti.ReadTextureSettings(s);
        s.spriteMeshType = SpriteMeshType.FullRect;
        ti.SetTextureSettings(s);
        ti.SaveAndReimport();
    }
}
