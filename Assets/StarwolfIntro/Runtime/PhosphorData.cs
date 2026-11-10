// =================================================================================================
// PhosphorData.cs
//
// What it does:
// This reads the blocky green letters used on the intro screen. Each letter is drawn as a grid of
// small squares. It turns a word into a black-and-white picture for the intro screen to show.
// StarwolfIntroController is the only script that uses it.
//
// How it works:
// - The letter shapes, sizes and colours are in glyphs.json, in the intro's Resources folder. Load
//   reads that file.
// - Load checks the file and stops with an error if anything is wrong: the sizes, the four colour
//   codes, or a letter that is badly written or listed twice.
// - Each letter is 10 rows of 0s and 1s. Every stroke is two squares wide and two tall. Letters
//   that break this rule are refused.
// - Cells lays out a word, letter by letter, with gaps between letters. It can add a box round
//   the word. It can spread the letters out to fill a set width. Two words then share one box
//   size.
// - MakeMask turns that layout into a picture: a white square for each lit cell, with gaps
//   between squares. The intro's display shader colours it.
// - Lower-case letters are read as capitals. A letter with no shape in the file is an error.
//
// Technical:
// - Namespace Starwolf.Intro. PhosphorData and PhosphorGlyph are plain classes, filled from JSON.
// - The file is Assets/StarwolfIntro/Resources/StarwolfIntro/glyphs.json. Its numbers are cell and
//   gap sizes in pixels, and gaps and padding in cells.
// - Colours: phosphor, background, rim and glow, as #RRGGBB text. The controller passes them to
//   the shader.
// - Load(TextAsset) and Validate() throw ArgumentException on any fault. Get(c) throws on a
//   missing letter.
// - Cells(text, frame, out size, fitWidth) gives lit cells from the top left, and the size in
//   cells.
// - MakeMask() builds an RGBA32 Texture2D: point filtered, linear, no mipmaps, unreadable after
//   Apply. Pitch is cellPixels plus cellGapPixels.
// - StarwolfIntroController is the only caller. It loads the file once and makes six masks.
// - Trap: Validate wants schemaVersion 1, rowCount 10 and strokeCells 2. Change those and every
//   load fails.
// =================================================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Starwolf.Intro
{
    [Serializable] public sealed class PhosphorGlyph { public string character; public string[] rows; }
    [Serializable] public sealed class PhosphorData
    {
        public int schemaVersion, rowCount, strokeCells, cellPixels, cellGapPixels, letterGapCells;
        public int framePaddingX, framePaddingY;
        // Owner 2026-11-10: rim = the deeper green at each square's edge; glow = the pure green round the squares.
        public string phosphor, background, rim, glow;
        public PhosphorGlyph[] glyphs;
        public int Pitch => cellPixels + cellGapPixels;
        public static PhosphorData Load(TextAsset source)
        {
            if (!source) throw new ArgumentNullException(nameof(source));
            var data = JsonUtility.FromJson<PhosphorData>(source.text);
            data.Validate();
            return data;
        }
        public void Validate()
        {
            if (schemaVersion != 1 || rowCount != 10 || strokeCells != 2 || cellPixels < 1 ||
                cellGapPixels < 1 || letterGapCells < 1 || framePaddingX < 1 || framePaddingY < 1)
                throw new ArgumentException("Invalid phosphor grid specification.");
            if (!ColorUtility.TryParseHtmlString(phosphor, out _) || !ColorUtility.TryParseHtmlString(background, out _) ||
                !ColorUtility.TryParseHtmlString(rim, out _) || !ColorUtility.TryParseHtmlString(glow, out _))
                throw new ArgumentException("Invalid palette.");
            var seen = new HashSet<string>();
            foreach (var glyph in glyphs)
            {
                if (glyph.character.Length != 1 || !seen.Add(glyph.character) || glyph.rows.Length != rowCount)
                    throw new ArgumentException("Invalid or duplicate glyph.");
                int width = glyph.rows[0].Length;
                if (width == 0 || width % 2 != 0) throw new ArgumentException("Glyph width must use two-cell units.");
                for (int y = 0; y < rowCount; y++)
                {
                    if (glyph.rows[y].Length != width) throw new ArgumentException("Unequal row widths.");
                    for (int x = 0; x < width; x++)
                        if ((glyph.rows[y][x] != '0' && glyph.rows[y][x] != '1') ||
                            glyph.rows[y][x] != glyph.rows[y / 2 * 2][x / 2 * 2])
                            throw new ArgumentException("Glyph violates two-cell construction.");
                }
            }
        }
        public PhosphorGlyph Get(char c)
        {
            c = char.ToUpperInvariant(c);
            foreach (var glyph in glyphs) if (glyph.character[0] == c) return glyph;
            throw new ArgumentException("Missing glyph: " + c);
        }
        // Top-left cell coordinates; SVG and in-game masks use this same convention.
        // fitWidth (owner 2026-11-09, "sized to each other"): a word narrower than this many squares has its
        // letters spread out to fill it, the extra shared between the gaps, so two words share one box.
        public List<Vector2Int> Cells(string text, bool frame, out Vector2Int size, int fitWidth = 0)
        {
            var cells = new List<Vector2Int>();
            int cursor = frame ? framePaddingX : 0;
            int top = frame ? framePaddingY : 0;
            int gaps = Mathf.Max(1, text.Length - 1), extra = 0;
            if (fitWidth > 0) { Cells(text, frame, out var natural); extra = Mathf.Max(0, fitWidth - natural.x); }
            for (int i = 0; i < text.Length; i++)
            {
                var glyph = Get(text[i]);
                for (int y = 0; y < rowCount; y++)
                    for (int x = 0; x < glyph.rows[y].Length; x++)
                        if (glyph.rows[y][x] == '1') cells.Add(new Vector2Int(cursor + x, top + y));
                cursor += glyph.rows[0].Length + letterGapCells + (i < text.Length - 1 ? (i + 1) * extra / gaps - i * extra / gaps : 0);
            }
            int width = Mathf.Max(fitWidth, cursor - letterGapCells + (frame ? framePaddingX : 0)); // one letter: the box still fills
            int height = rowCount + top * 2;
            if (frame)
            {
                for (int x = 0; x < width; x++) { cells.Add(new Vector2Int(x, 0)); cells.Add(new Vector2Int(x, height - 1)); }
                for (int y = 1; y < height - 1; y++) { cells.Add(new Vector2Int(0, y)); cells.Add(new Vector2Int(width - 1, y)); }
            }
            size = new Vector2Int(width, height);
            return cells;
        }
        public Texture2D MakeMask(string text, bool frame, int fitWidth = 0)
        {
            var cells = Cells(text, frame, out var size, fitWidth);
            int width = size.x * Pitch, height = size.y * Pitch;
            var pixels = new Color32[width * height];
            foreach (var cell in cells)
                for (int y = 0; y < cellPixels; y++)
                    for (int x = 0; x < cellPixels; x++)
                        pixels[(height - 1 - cell.y * Pitch - y) * width + cell.x * Pitch + x] = new Color32(255,255,255,255);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true)
                { name = text, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return texture;
        }
    }
}
