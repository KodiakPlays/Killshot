using System;

/// <summary>
/// Marks a value the owner tunes. Every field with this attribute, on any component in the open
/// scene (including fields inside its serializable classes and arrays), shows in one window:
/// Starwolf → Tuning (Assets/Editor/StarwolfTuningWindow.cs), grouped by section.
/// Add it to every new tunable value (owner, 2026-10-17: all tuning in one place).
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class TuneAttribute : Attribute
{
    public readonly string Section;
    public TuneAttribute(string section) { Section = section; }
}
