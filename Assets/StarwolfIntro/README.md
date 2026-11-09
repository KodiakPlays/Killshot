# The phosphor screen

**Summary.** This folder draws the old green-screen look used by the intro (SECTION 36, STARWOLF, the
menu), the two title cards (MISSION ONE, then WOLF HUNT) and INITIALISING with its loading bar. One shader draws the screen. One
script puts it on the screen and decides what it shows. A text material gives normal text the same
green glow. To use the look on a new screen, add the script to an object, pick a mode, and put any
normal text on a canvas above it.

What changed, step by step, and why, is in `Assets/Scenes/Tactical/BUILD_LOG.md` (Steps 27, 56, 70,
71, 94, 106, 174). This file only says how it works and how to use it.

---

## 1 · The files

| File | What it does |
|---|---|
| `Shaders/PhosphorDisplay.shader` | Draws the screen: the dot letters, the glow around them, the dark green background, scanlines, the darker edges, and the loading bar. |
| `Runtime/StarwolfIntroController.cs` | The script. Makes the letter shapes, runs the timing, draws the shader into an image, and shows that image on screen. Also handles the menu keys and clicks. |
| `Runtime/PhosphorData.cs` | Reads the dot font and turns a word into a letter shape. |
| `Resources/StarwolfIntro/glyphs.json` | The dot font: each letter drawn as rows of `1` and `0`. Also holds the two colours. |
| `Resources/StarwolfIntro/hardware-hum.wav` | The low hum that plays under the screen. |
| `Materials/ShareTechMono-Regular SDF Phosphor Glow.mat` | Material for normal TextMeshPro text: green glow to match the dots. |

---

## 2 · How it works

1. On start, the script reads `glyphs.json` and makes one black-and-white image per word: SECTION
   36, INITIALISING..., the title, and the three menu items. Each lit cell becomes a small square.
2. Every frame, the shader draws a 1920 × 1080 image. It lights the squares of the current word in
   green, adds a soft glow around them, adds the dark green background, then darkens the edges and
   adds faint scanlines.
3. The script shows that image on a full-screen canvas, kept at 16:9, with black bars if the window
   is a different shape.
4. In the full intro, SECTION 36 flickers into being where it stands, square by square in a quick
   scattered order. It stays, then turns into STARWOLF. Squares lit in both words stay lit: the S
   and the whole box. Squares only SECTION 36 uses flicker out, and squares only STARWOLF uses
   flicker on. While a word flickers in or over, thin bands slip sideways and the light smears and
   grains along them. It dies away as the squares settle, and never shows on the menu.
5. The two words are sized to each other. STARWOLF is narrower, so its letters are spread out to fill
   SECTION 36's box: the gaps between letters grow from 4 squares to 6 or 7. Both boxes are the same
   size and centred on the screen, so the S's line up. The box stays still during the change-over;
   only the letters inside change. The script works the spacing out from the two words' widths.
   This is only in the full intro. INITIALISING... and the title-only pages keep normal spacing.
6. INITIALISING... and the title-only pages still "type in" left to right, one column of dots at a
   time, in the middle, then stay.

**Why a canvas and not OnGUI.** OnGUI brightened the picture twice. The text looked pale and the
dark green background turned black. The canvas brightens it once, which is correct. Do not go back
to OnGUI for drawing. OnGUI is still used, but only to read the menu keys and clicks.

---

## 3 · The three modes

Set these on the script in the inspector.

| Mode | Tick | What it shows | Used in |
|---|---|---|---|
| Full intro | nothing | SECTION 36 flickers in, turns into the title, then the menu | `00_Start` |
| Title only | `titleOnly` | The title types in and stays. No menu. | `01_Briefing`, `MISSION ONE card` and `WOLF HUNT card` |
| INITIALISING only | `initialisingOnly` | INITIALISING... and the loading bar, then it calls `finished` | `01_Briefing`, `INITIALISING` |

The loading bar fills from empty to full over `initialisingDuration` seconds. When it is full, the
script calls `finished`. In `01_Briefing`, `finished` loads the board.

All three modes use one clock. Each frame adds at most `Time.maximumDeltaTime` (a third of a second)
to it. When play starts in the Editor, the picture can freeze for 4 to 6 seconds on the first frame.
That freeze no longer uses up SECTION 36 (Step 153).

In `01_Briefing`, the `ScreenFlow` object runs the order (`Assets/Scripts/Tactical/ScreenFlow.cs`).
The MISSIONS list shows first. Pick a level, and INITIALISING switches on. Its loading bar fills, then
the board loads. The list ignores a pick in its first half second (Step 153). ScreenFlow's **Cards**
list is empty, so the MISSION ONE and WOLF HUNT cards do not show.

The whole order: `00_Start` shows SECTION 36, STARWOLF and the menu. NEW GAME loads `01_Briefing`, which
shows the MISSIONS list and then INITIALISING. Then the board loads (`02.1_Grid`).

---

## 4 · The inspector fields

| Field | What it does |
|---|---|
| `glyphData`, `displayShader`, `hardwareHum` | The font, the shader and the hum. `displayShader` must be set. The other two load themselves if empty. |
| `sectionDuration`, `titleDuration`, `initialisingDuration` | How long each word stays, in seconds. |
| `typeShare` | INITIALISING... and the title-only pages only. How much of that time the typing takes. 0.5 = types in the first half, stays for the second. |
| `flickerInSeconds` | Full intro only. How long SECTION 36 takes to flicker in. Default 0.4. |
| `changeSeconds` | Full intro only. How long SECTION 36 takes to turn into the title. Default 0.6. |
| `diffusion` | Full intro only. How strong the sideways slip, smear and grain are while a word flickers. 0 = none, 1 = strong, up to 2. Default 0.7. |
| `titleText` | The title word. STARWOLF in the intro, MISSION ONE and WOLF HUNT on the title cards. Only letters in the font work (see section 6). |
| `titleOffset` | Moves the title up, in pixels on the 1920 × 1080 screen. 0 = centre. |
| `canvasOrder` | Which canvases draw on top. The screen covers every canvas with a lower number. Default 32767 covers everything. |
| `humVolume` | Volume of the hum. |
| `newGame`, `loadGame`, `settings`, `loadGameAvailable` | Menu wiring. Full intro mode only. |

---

## 5 · Putting the look on a new screen

1. Add an empty object to the scene. Add `StarwolfIntroController` to it.
2. Set `displayShader` to `PhosphorDisplay`. The easy way is to copy the component from an existing
   screen, such as `INITIALISING` in `01_Briefing`, then change the fields.
3. Pick a mode (section 3). For a page with a title and some text, tick `titleOnly`, set
   `titleText` and `titleOffset`, and set `canvasOrder` to 0.
4. For normal text on top:
   - Put the text on a canvas set to **Screen Space - Overlay**, with **Sort Order** higher than the
     script's `canvasOrder` (for example 1).
   - Font: `ShareTechMono-Regular SDF`. Material: `ShareTechMono-Regular SDF Phosphor Glow`.
   - Colour: `#A2F9A0`. Upper case. A little letter spacing looks right.
5. Press Play and look at it.

`MISSION ONE card` in `01_Briefing` is a working example of steps 1 to 3. No screen has normal text on
top now. The DEMO welcome page had it until Step 94 removed the page.

---

## 6 · Changing the dot font and colours

**Colours** are in `glyphs.json`:

- `phosphor`: the dot colour, `#A2F9A0`.
- `background`: the screen colour, `#001100` (very dark green).

These match the reference images. Change them here, not in the shader.

**Letters.** The font only has the letters it has needed so far:

`A B C D E F G H I L M N O P R S T U W 3 6 .` and space.

A word with any other letter stops the script with "Missing glyph". To add a letter, add an entry to
`glyphs` in `glyphs.json`:

- exactly 10 rows;
- each row the same length, and an even number of cells;
- strokes two cells thick, to match the others.

Copy a similar letter and edit it. The script checks these rules on start and says what is wrong.

---

## 7 · Tuning the look

These are in the shader's **Properties** block at the top of `PhosphorDisplay.shader`. The script
does not set them, so the value written there is the value used.

| Property | What it does |
|---|---|
| `_ScanDepth` | How dark the scanlines are. 0 = none. |
| `_Vignette` | How much the screen darkens towards the corners. |
| `_ScreenInset` | How wide the dark rounded edge of the screen is. |

The glow around the dots is the two `bloom` lines in `frag()`. The first number (.015) is the glow
right next to a dot. The second (.009) is the wider, fainter glow.

---

## 8 · Traps

- **Do not use the full `TextMeshPro/Distance Field` shader** for the glow text. It draws magenta
  blocks in this project. The glow material uses the mobile shader, and its "underlay" is the glow.
- **Switching a canvas from Screen Space - Camera to Overlay** keeps each child's Z position. A
  large Z hides the text. Set Z to 0.
- **Every value the script sends to the shader must also be listed in the shader's Properties
  block.** If one is missing, the screen goes black after the shader reloads.
- **Unity's `GammaToLinearSpace` is an estimate** that is far too dark near black. Use
  `GammaToLinearSpaceExact` if you ever need that conversion.

---

## 9 · Not built yet

From the owner's reference images, still to come:

- the frame with open sides;
- the small second line under the title (STRATEGIC SYSTEMS DIVISION);
- the corner text and the clearance squares;
- a lit rim around the screen and visible scanline bands.
