using System;

/// <summary>
/// Marks a value the owner tunes on the board (the plot, sounds, the clock). Every field with this attribute,
/// on any component in the open scene (including fields inside its serializable classes and arrays), shows on
/// the Board tab of Starwolf → Tuning (Assets/Editor/StarwolfTuningWindow.cs), grouped by section. The "Keys"
/// section shows on its own Keys tab instead (Starwolf → Keys).
/// A ship's numbers don't use it: they go in the ship part's Stats section, which is in the ship's stats file
/// (Assets/Ships) and shows on the Ship and Enemy tabs (owner, 2026-10-27).
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class TuneAttribute : Attribute
{
    public readonly string Section;
    public TuneAttribute(string section) { Section = section; }
}
