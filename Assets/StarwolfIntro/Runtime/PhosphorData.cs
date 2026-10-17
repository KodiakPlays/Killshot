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
        public string phosphor, background;
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
            if (!ColorUtility.TryParseHtmlString(phosphor, out _) || !ColorUtility.TryParseHtmlString(background, out _))
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
        public List<Vector2Int> Cells(string text, bool frame, out Vector2Int size)
        {
            var cells = new List<Vector2Int>();
            int cursor = frame ? framePaddingX : 0;
            int top = frame ? framePaddingY : 0;
            foreach (char c in text)
            {
                var glyph = Get(c);
                for (int y = 0; y < rowCount; y++)
                    for (int x = 0; x < glyph.rows[y].Length; x++)
                        if (glyph.rows[y][x] == '1') cells.Add(new Vector2Int(cursor + x, top + y));
                cursor += glyph.rows[0].Length + letterGapCells;
            }
            int width = cursor - letterGapCells + (frame ? framePaddingX : 0);
            int height = rowCount + top * 2;
            if (frame)
            {
                for (int x = 0; x < width; x++) { cells.Add(new Vector2Int(x, 0)); cells.Add(new Vector2Int(x, height - 1)); }
                for (int y = 1; y < height - 1; y++) { cells.Add(new Vector2Int(0, y)); cells.Add(new Vector2Int(width - 1, y)); }
            }
            size = new Vector2Int(width, height);
            return cells;
        }
        public Texture2D MakeMask(string text, bool frame)
        {
            var cells = Cells(text, frame, out var size);
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
