// =================================================================================================
// BoardPalette.cs
//
// What it does:
// The battle board's colours, in one file: Assets/UI/Board/Palette.asset. Every panel and
// BoardManager take their colours from it. Change a colour there and the whole board follows.
//
// How it works:
// - State colours: green for working and amber for a warning. Bright amber is for a warning about
//   to turn bad. Red is for failed or hostile, and blue for our craft. There are bright and dark
//   screen greens, and white for our ship on the plot.
// - Text colours: readouts, labels, notes and second lines. More are for lettering on the metal,
//   dim lettering and lettering in a dark well. One is for controls that cannot be used now.
// - Off colours: unlit lamps, empty and lit bar cells, unlit power lamps and unlit ring parts.
// - Each colour is typed as it should look on screen. The panels convert it for the board.
// - If a panel has no palette file set, it uses this script's starting colours. They can differ
//   from the file.
//
// Technical:
// - Data: one Color per use, in three groups: States, Text, and Off and empty. Each is typed as it
//   looks on screen (sRGB).
// - Or(palette) gives that file. With none, it gives one shared copy made with this script's
//   starting colours.
// - Panel reads it through its Palette property. BoardManager calls Or for its lamps and labels.
// - Panel.Paint and BoardManager.Tint turn each colour to gamma before they use it.
// - Each panel and BoardManager has a palette slot in the Inspector. Each should point at
//   Assets/UI/Board/Palette.asset.
// - Trap: a panel with an empty slot ignores the file. It shows this script's starting colours.
// - To add a colour: add a field here, then set its value in the file.
// =================================================================================================

using UnityEngine;

public class BoardPalette : ScriptableObject
{
    [Header("States")]
    [Tooltip("Green: working, on, ready.")]
    public Color nominal = new Color32(0x8F, 0xC3, 0x3A, 0xFF);
    [Tooltip("Amber: needs attention.")]
    public Color caution = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [Tooltip("Bright amber: a caution that is about to become a problem.")]
    public Color cautionHot = new Color32(0xFF, 0xC4, 0x4D, 0xFF);
    [Tooltip("Red: failed, refused, hostile.")]
    public Color alarm = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
    [Tooltip("Blue: our own forces, such as fighters out and the damage control party.")]
    public Color ownForce = new Color32(0x4A, 0x9F, 0xD8, 0xFF);
    [Tooltip("Bright green: screen glow, a thrown switch.")]
    public Color phosphor = new Color32(0xB6, 0xF0, 0x4A, 0xFF);
    [Tooltip("Dark green: an unlit screen glow.")]
    public Color phosphorOff = new Color32(0x1B, 0x28, 0x10, 0xFF);
    [Tooltip("White: our ship, our fighters and our slugs on the plot (owner 2026-10-18 and 2026-10-26).")]
    public Color ownShip = new Color32(0xE8, 0xED, 0xF0, 0xFF);

    [Header("Text")]
    [Tooltip("Near-white: readouts and live numbers.")]
    public Color data = new Color32(0xE8, 0xED, 0xF0, 0xFF);
    [Tooltip("Light grey: labels.")]
    public Color label = new Color32(0xC9, 0xD2, 0xD8, 0xFF);
    [Tooltip("Grey: notes under a readout.")]
    public Color note = new Color32(0x93, 0x9E, 0xA6, 0xFF);
    [Tooltip("Mid grey: second-line text, wrecks.")]
    public Color secondary = new Color32(0x8A, 0x95, 0x9D, 0xFF);
    [Tooltip("Mid grey: stencilled lettering on the metal.")]
    public Color stencil = new Color32(0x77, 0x82, 0x8A, 0xFF);
    [Tooltip("Dim grey: stencilled lettering that is off.")]
    public Color stencilDim = new Color32(0x5E, 0x68, 0x70, 0xFF);
    [Tooltip("Grey: lettering inside a dark well.")]
    public Color wellStencil = new Color32(0x5B, 0x66, 0x6F, 0xFF);
    [Tooltip("Grey-blue: a control that can't be used now.")]
    public Color inactive = new Color32(0x6E, 0x7B, 0x85, 0xFF);

    [Header("Off and empty")]
    [Tooltip("An unlit lamp on a panel.")]
    public Color lampOffPanel = new Color32(0x22, 0x26, 0x2A, 0xFF);
    [Tooltip("An unlit lamp beside a selector knob.")]
    public Color lampOffSelector = new Color32(0x1B, 0x20, 0x24, 0xFF);
    [Tooltip("An empty bar cell.")]
    public Color cellUnlit = new Color32(0x0E, 0x13, 0x16, 0xFF);
    [Tooltip("An unlit core lamp in the power drawer.")]
    public Color coreUnlit = new Color32(0x2A, 0x2F, 0x34, 0xFF);
    [Tooltip("An unlit ring segment or rail lamp.")]
    public Color ringUnlit = new Color32(0x1A, 0x1F, 0x23, 0xFF);
    [Tooltip("White: a lit bar cell.")]
    public Color cellLit = Color.white;

    private static BoardPalette defaults;

    /// <summary>This palette, or the design's own colours when no palette file is set.</summary>
    public static BoardPalette Or(BoardPalette palette)
    {
        if (palette) return palette;
        if (!defaults) defaults = CreateInstance<BoardPalette>();
        return defaults;
    }
}
