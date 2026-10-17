# SECTION 36 — Logo Design README

**Status:** Canonical wordmark, terminal presentation, frame, web implementation, and Unity implementation defined.

**Canonical logo text:** `SECTION 36`

The primary logo contains **no subtitle**. Do not add `STRATEGIC SYSTEMS DIVISION` beneath the wordmark.

## 0. Canonical visual reference

`canonical_terminal_reference.png` is the **authoritative visual reference** for the SECTION 36 terminal presentation. Match it closely rather than reinterpreting it.

The reference locks the following characteristics:

- coarse square phosphor cells for the primary wordmark
- 10-row main glyph grid
- 2-cell main stroke thickness
- 8 × 8 px illuminated cells on a 12 px pitch at the 1920 × 1080 logical reference scale
- 1-cell dotted orthogonal frame
- square exterior corners and square terminals
- no chamfers, diagonal corner cuts, bevels, or simulated curves
- dark green-black CRT field
- pale green phosphor core with restrained bloom
- visible scanlines, low grain, subtle screen curvature, and slight vignette
- generous negative space around the central mark
- small monospaced/raster support text positioned near the screen edges

The supplied reference image contains a legacy `STRATEGIC SYSTEMS DIVISION` line beneath `SECTION 36`. **That line is not part of the current primary logo and must be omitted in new production assets.** Everything else about the reference should be treated as the target presentation language.

When adapting this system to another primary wordmark such as `STARWOLF`, do **not** redesign the style. Preserve the same cell size, pitch, frame construction, stroke weight, phosphor treatment, margins, CRT treatment, and composition. Change only the glyph matrix required to spell the new wordmark.

## 1. Core visual idea

SECTION 36 should look as though a late-1970s or 1980s military computer draws its own identification mark on a monochrome phosphor display.

The identity comes from the actual pixel construction. CRT bloom, scanlines, grain, and flicker sit on top of that geometry; they do not define it.

## 2. Non-negotiable mono-stroke rule

The primary `SECTION 36` wordmark uses a strict **2-cell mono stroke**.

**Every main horizontal and vertical stroke is exactly 2 cells thick.**

This applies to:

- horizontal bars
- vertical bars
- internal bars
- terminals
- counters
- structural strokes in every glyph

Do not use a 1-cell section anywhere inside the primary wordmark. Do not thicken a normal stroke to 3 cells. Where two valid 2-cell strokes meet, their union may naturally occupy a larger junction area; the constituent strokes themselves remain 2 cells thick.

The frame and microtext use a finer 1-cell construction and therefore remain visually subordinate to the wordmark.

## 3. Canonical grid

Every main glyph uses a **10-column × 10-row** bitmap.

Each illuminated cell has identical dimensions. Cell spacing remains constant throughout the wordmark.

Recommended hierarchy:

| Element | Construction |
| --- | --- |
| Primary `SECTION 36` glyphs | 10 × 10 matrix, 2-cell strokes |
| Dotted frame | 1-cell stroke |
| Terminal microtext | 1-cell raster text |
| Status indicators | 1-cell construction |

The difference between primary lettering and support graphics comes from stroke thickness and scale, never from inconsistent dot sizes within a glyph.

## 4. No simulated curves

The wordmark must not contain:

- rounded corners
- chamfered corners
- diagonal cuts
- bevelled terminals
- one-cell corner notches
- stepped outer corners intended to imply curves

This rule matters especially for `S`, `C`, `O`, `3`, and `6`.

### S

- top bar: 2 cells high
- upper-left vertical: 2 cells wide
- middle bar: 2 cells high
- lower-right vertical: 2 cells wide
- bottom bar: 2 cells high
- all outer terminals remain square

Canonical mask:

```text
1111111111
1111111111
1100000000
1100000000
1111111111
1111111111
0000000011
0000000011
1111111111
1111111111
```

### C

- top bar: 2 cells high
- left vertical: 2 cells wide
- bottom bar: 2 cells high
- right side remains open
- upper-right and lower-right terminals stay square

Canonical mask:

```text
1111111111
1111111111
1100000000
1100000000
1100000000
1100000000
1100000000
1100000000
1111111111
1111111111
```

### O

The O is a true rectangle.

- top bar: 2 cells high
- bottom bar: 2 cells high
- left side: 2 cells wide
- right side: 2 cells wide
- all four exterior corner cells remain illuminated

Canonical mask:

```text
1111111111
1111111111
1100000011
1100000011
1100000011
1100000011
1100000011
1100000011
1111111111
1111111111
```

### 3

- top, middle, and bottom bars: 2 cells high
- right vertical sections: 2 cells wide
- all joins remain square

Canonical mask:

```text
1111111111
1111111111
0000000011
0000000011
1111111111
1111111111
0000000011
0000000011
1111111111
1111111111
```

### 6

- top, middle, and bottom bars: 2 cells high
- upper-left vertical: 2 cells wide
- lower-left vertical: 2 cells wide
- lower-right vertical: 2 cells wide
- rectangular counter

Canonical mask:

```text
1111111111
1111111111
1100000000
1100000000
1111111111
1111111111
1100000011
1100000011
1111111111
1111111111
```

## 5. Other glyphs

`E`, `T`, `I`, and `N` obey the same 2-cell stroke rule.

The N may use a stepped internal diagonal because a diagonal is structurally required for legibility. That diagonal must itself use 2 × 2 cell blocks. The outer left and right stems remain 2 cells wide.

The authoritative glyph definitions live in:

- `section36_web_demo.html` → `GLYPHS`
- `Section36Logo.cs` → `Glyphs`

These two tables must remain identical.

## 6. Frame

The surrounding frame uses a finer matrix than the wordmark.

- 1-cell stroke
- square dots
- horizontal and vertical segments only
- strict 90-degree corners
- no chamfers
- no diagonal corner cuts

The frame may remain broken at selected points. It should read as a terminal-drawn technical frame rather than a decorative badge.

## 7. Core logo versus terminal presentation

The core studio mark is:

```text
SECTION 36
```

Optionally surrounded by the dotted orthogonal frame.

The following elements belong only to terminal presentation and are not part of the permanent logo:

- `UC-FLEET`
- `// CLASSIFIED`
- revision codes
- clearance levels
- research/development/operations labels
- scanlines
- phosphor flicker
- screen curvature

Do not place a subtitle or secondary organisation name beneath the wordmark.

## 8. Colour and display treatment

Starting values:

| Item | Value |
| --- | --- |
| Background | `#020B05` |
| Main phosphor | `#B4F4AD` |
| Secondary phosphor | `#8FC08D` |
| Logical surface | 1920 × 1080 |
| Main cell pitch | 12 px |
| Main cell size | 8 × 8 px |
| Fine frame cell size | 4 × 4 px |
| Fine frame pitch | 9 px |

Keep the clean geometry sharp. Add display effects afterwards:

1. subtle phosphor bloom
2. subtle scanlines
3. low screen grain
4. slight vignette
5. extremely low flicker, if animated

Never blur strongly enough to hide cell boundaries or alter the apparent glyph silhouette.

## 9. STARWOLF derived mark

`STARWOLF` is a derived game mark inside the same visual system, not a separate logo style.

For the terminal/title-screen version:

- use the same 10-row square-cell construction as SECTION 36
- use the same 2-cell primary stroke weight
- use the same 8 × 8 px cell size and 12 px pitch at the reference scale
- use the same 1-cell dotted frame
- use the same pale green phosphor and CRT treatment
- use the same square-terminal logic
- keep letter construction geometric and grid-native
- allow stepped 2 × 2 blocks only where a diagonal is structurally required, such as `R` or `W`
- do not introduce a new font, solid vector strokes, metallic treatment, reticles, badges, circles, wolf symbols, claw marks, or unrelated HUD decoration
- do not add a subtitle

The media logo and the in-game logo must share the same underlying glyph geometry. The in-game reveal may later sit on, or transition into, the tactical grid, but that animation must not alter the canonical STARWOLF letterforms.

**Critical adaptation rule:** use the SECTION 36 reference as a template and replace only the central wordmark. Do not ask a renderer or image generator to invent a new interpretation of the style.

## 10. Web implementation

Use Canvas or SVG generated from the bitmap definitions.

Do not render the primary wordmark with a webfont.

For each glyph:

1. read its 10 × 10 matrix
2. iterate through rows and columns
3. draw one identical square cell for every `1`
4. advance the next character by a fixed amount
5. apply CRT effects only after the clean geometry renders

The included `section36_web_demo.html` follows this system.

## 11. Unity 6 implementation

Use the same bitmap matrices in C#.

Recommended pipeline:

```text
Glyph bitmap
→ generated Texture2D or mesh
→ RawImage
→ CRT material/effects
```

Use Point filtering on the clean generated texture. Do not use TextMeshPro for the primary SECTION 36 wordmark. TextMeshPro may render optional terminal metadata outside the logo.

The included `Section36Logo.cs` uses the canonical masks directly and generates the mark at runtime.

## 12. Validation

A build is not approved until it passes all of these checks:

- [ ] Logo reads `SECTION 36`.
- [ ] No subtitle appears beneath the wordmark.
- [ ] Every normal primary stroke measures exactly 2 cells thick.
- [ ] No primary horizontal bar is only 1 cell high.
- [ ] No primary vertical bar is only 1 cell wide.
- [ ] S has square top-left and bottom-right terminals.
- [ ] C has square open terminals.
- [ ] O forms a true rectangle with all four exterior corner cells illuminated.
- [ ] 3 uses 2-cell top, middle, and bottom bars.
- [ ] 6 uses 2-cell top, middle, and bottom bars.
- [ ] No chamfers, bevels, rounded corners, or diagonal cuts appear on outer glyph boundaries.
- [ ] Frame uses only horizontal and vertical construction.
- [ ] Web and Unity glyph matrices match exactly.
- [ ] Geometry remains correct with every CRT effect disabled.

## 13. One-line design rule

**SECTION 36 uses the canonical terminal reference: square phosphor cells, a 10 × 10 glyph grid, strict 2-cell mono strokes, square outer corners, a 1-cell orthogonal frame, and no curves, chamfers, diagonal cuts, or 1-cell sections in the primary wordmark. STARWOLF inherits this exact system rather than introducing a new style.**
