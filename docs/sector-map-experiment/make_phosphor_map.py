"""Phosphor sector map: makes the experiment's copy of the sector map picture.

What it does:
Takes the Codex sector map picture (OperationalMapConsole.png, left untouched) and makes a copy
where both screens (the map on the right, the briefing screen on the left) look like our phosphor
screens: green lines with a soft glow, darker edges. The painted words on them are rubbed out and
written again in our letters: the big ones in the square-block letters of the intro's title, the
small ones in Share Tech Mono. Some are rubbed out with nothing written back (STARWOLF and the
DESTROYER lines: the game does not use the name STARWOLF, owner 2026-11-11).
It also makes Scanlines.png, the strip laid over the map screen in the test scene
Sector Map Phosphor.unity. Both pictures go next to the original.

How it works:
- Only the two screens' glass is changed. The metal and cables stay as painted.
- The copy is twice the size of the original, so the block letters keep their squares.
- Colours and block letters come from the intro's glyphs.json.

Run from the repo root:
  uv run --with pillow --with numpy --with opencv-python-headless python docs/sector-map-experiment/make_phosphor_map.py
"""
import json, sys
from pathlib import Path
import numpy as np
import cv2
from PIL import Image, ImageDraw, ImageFont, ImageFilter

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / "Assets/UI/Board/Art/Briefing/OperationalMapConsole.png"
OUT = Path(sys.argv[1]) if len(sys.argv) > 1 else ROOT / "Assets/UI/Board/Art/Briefing"
GLYPHS = json.loads((ROOT / "Assets/StarwolfIntro/Resources/StarwolfIntro/glyphs.json").read_text())
THIN = ROOT / "Assets/UI/Fonts/ShareTechMono/ShareTechMono-Regular.ttf"

def hexrgb(h): return np.array([int(h[i:i + 2], 16) for i in (1, 3, 5)], float)
PHOSPHOR, RIM, GLOW = hexrgb(GLYPHS["phosphor"]), hexrgb(GLYPHS["rim"]), hexrgb(GLYPHS["glow"])
SCREEN_BG = np.array([2, 12, 4], float)

# The screens' glass, in the original picture's pixels (1840 x 1035): box, corner rounding.
GLASSES = [
    ((724, 209, 1676, 965), 0),   # the map screen (right)
    ((131, 253, 524, 917), 12),   # the briefing screen (left), owner 2026-11-11: "make that green"
]
# Painted words on the screens: box to rub out, the new words ("" = none), block or thin, where they go.
# anchor: "l" = start at the box's left edge, "r" = end at its right edge.
WORDS = [
    ((762, 242, 968, 262), "SECTOR PLOT", "block", "l"),  # owner 2026-11-11: no STARWOLF, the name is not used in the game
    ((762, 267, 890, 282), "", "thin", "l"),  # DESTROYER NAVIGATION: rubbed out, no new words (owner 2026-11-11)
    ((1491, 243, 1605, 259), "RELATIVE BEARING", "thin", "r"),
    ((950, 335, 1019, 351), "NAV FIX 03", "thin", "l"),
    ((1292, 367, 1342, 382), "PRIMARY", "thin", "l"),
    ((1462, 836, 1542, 852), "OUTER DRIFT", "thin", "l"),
    ((788, 650, 912, 669), "MISSION ARCHIVE", "block", "l"),
    # the briefing screen (left)
    ((153, 273, 260, 295), "", "block", "l"),  # STARWOLF: rubbed out (owner 2026-11-11)
    ((153, 308, 318, 323), "", "thin", "l"),   # DESTROYER / OPERATIONS: rubbed out (owner 2026-11-11)
    ((152, 372, 268, 388), "MISSION RECORD", "block", "l"),
    ((153, 834, 264, 848), "CAMPAIGN ARCHIVE", "thin", "l"),
]
SCALE = 2          # the copy is twice the original size
CELL, GAP = 2, 1   # block letters: square size and gap, in the copy's pixels
LETTER_GAP = 2     # cells between block letters (the intro uses 4)

GLYPH = {g["character"]: g["rows"] for g in GLYPHS["glyphs"]}
GLYPH["/"] = ["000011", "000011", "000110", "000110", "001100",
              "001100", "011000", "011000", "110000", "110000"]  # glyphs.json has no slash


def recolour(rgb, box, rounding):
    """The glass turns phosphor green: brightness picks a colour from dark, to rim, to phosphor."""
    x0, y0, x1, y1 = box
    glass = rgb[y0:y1, x0:x1].astype(float)
    lum = glass @ np.array([0.3, 0.55, 0.15]) - 6.0
    t = np.clip(lum / 170.0, 0, 1)[..., None]
    low = SCREEN_BG + (RIM - SCREEN_BG) * np.clip(t / 0.5, 0, 1)
    out = np.where(t < 0.5, low, RIM + (PHOSPHOR - RIM) * np.clip((t - 0.5) / 0.5, 0, 1))
    # glow: a tight halo and a wide one round every lit line
    near = cv2.GaussianBlur(t[..., 0], (0, 0), 2.0)[..., None]
    far = cv2.GaussianBlur(t[..., 0], (0, 0), 8.0)[..., None]
    out += GLOW * (0.25 * near + 0.12 * far)
    # darker towards the glass edges, like the intro's vignette
    h, w = t.shape[:2]
    yy, xx = np.mgrid[0:h, 0:w]
    r2 = ((xx / w - 0.5) * 2) ** 2 + ((yy / h - 0.5) * 2) ** 2
    out *= (1 - 0.35 * np.clip(r2 / 2, 0, 1))[..., None]
    # only inside the glass's rounded corners; the frame round them keeps its paint
    m = Image.new("L", (w, h), 0)
    ImageDraw.Draw(m).rounded_rectangle((0, 0, w - 1, h - 1), radius=rounding, fill=255)
    m = np.asarray(m.filter(ImageFilter.GaussianBlur(0.7))).astype(float)[..., None] / 255
    rgb[y0:y1, x0:x1] = np.clip(out * m + glass * (1 - m), 0, 255).astype(np.uint8)


def rub_out(rgb):
    """Fills each painted word's box from the map round it."""
    mask = np.zeros(rgb.shape[:2], np.uint8)
    for (x0, y0, x1, y1), *_ in WORDS:
        mask[y0 - 1:y1 + 1, x0 - 1:x1 + 1] = 255  # the whole box: no faint ghost of the old words is left
    return cv2.inpaint(rgb, mask, 4, cv2.INPAINT_TELEA)


def block_word(text):
    """Square-block letters, as the intro draws STARWOLF, at small size. Returns a white mask."""
    rows = len(next(iter(GLYPH.values())))
    pitch = CELL + GAP
    widths = [len(GLYPH[c][0]) for c in text]
    w = sum(widths) * pitch + LETTER_GAP * pitch * (len(text) - 1)
    img = Image.new("L", (w, rows * pitch), 0)
    d = ImageDraw.Draw(img)
    x = 0
    for c in text:
        for r, line in enumerate(GLYPH[c]):
            for col, on in enumerate(line):
                if on == "1":
                    px, py = x + col * pitch, r * pitch
                    d.rectangle((px, py, px + CELL - 1, py + CELL - 1), fill=255)
        x += (len(GLYPH[c][0]) + LETTER_GAP) * pitch
    return img


def thin_word(text, width):
    """Share Tech Mono, sized to the painted word's width."""
    size = 10
    while ImageFont.truetype(str(THIN), size + 1).getlength(text) <= width: size += 1
    font = ImageFont.truetype(str(THIN), size)
    l, t, r, b = font.getbbox(text)
    img = Image.new("L", (r - l + 2, b - t + 2), 0)
    ImageDraw.Draw(img).text((1 - l, 1 - t), text, font=font, fill=255)
    return img


def write_words(big):
    """Draws each word in green with a glow on the twice-size copy."""
    layer = Image.new("L", big.size, 0)   # bright words
    dim = Image.new("L", big.size, 0)     # quieter words
    for (x0, y0, x1, y1), text, kind, anchor in WORDS:
        if not text: continue  # rubbed out only
        if kind == "block":
            m = block_word(text)
            target = layer
        else:
            m = thin_word(text, (x1 - x0) * SCALE)
            target = dim
        h = m.size[1]
        cy = (y0 + y1) / 2 * SCALE
        x = x0 * SCALE if anchor == "l" else x1 * SCALE - m.size[0]
        target.paste(m, (int(x), int(cy - h / 2)), m)
    a = np.asarray(big).astype(float)
    for lay, strength in ((layer, 1.0), (dim, 0.75)):
        m = np.asarray(lay).astype(float)[..., None] / 255
        glow = np.asarray(lay.filter(ImageFilter.GaussianBlur(3 * SCALE))).astype(float)[..., None] / 255
        a = a * (1 - m * strength) + PHOSPHOR * m * strength
        a += GLOW * glow * 0.6 * strength
    return Image.fromarray(np.clip(a, 0, 255).astype(np.uint8))


def scanlines():
    """A 1 x 4 strip, repeated down the map screen in the test scene."""
    alpha = [70, 30, 0, 30]
    img = Image.new("RGBA", (1, 4))
    for y, a in enumerate(alpha): img.putpixel((0, y), (0, 0, 0, a))
    return img


def main():
    src = Image.open(SRC).convert("RGBA")
    rgb = np.asarray(src)[..., :3].copy()
    rgb = rub_out(rgb)
    for box, rounding in GLASSES: recolour(rgb, box, rounding)
    big = Image.fromarray(rgb).resize((src.width * SCALE, src.height * SCALE), Image.LANCZOS)
    big = write_words(big).convert("RGBA")
    big.putalpha(src.getchannel("A").resize(big.size, Image.LANCZOS))
    OUT.mkdir(parents=True, exist_ok=True)
    big.save(OUT / "OperationalMapConsole Phosphor.png")
    scanlines().save(OUT / "Scanlines.png")
    print("wrote", OUT / "OperationalMapConsole Phosphor.png", big.size)


if __name__ == "__main__":
    main()
