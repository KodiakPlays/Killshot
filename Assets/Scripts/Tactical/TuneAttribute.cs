// =================================================================================================
// TuneAttribute.cs
//
// What it does:
// This is the [Tune] mark. A setting on a board script marked [Tune("Section")] shows in the
// Tuning window (Starwolf > Tuning), under that section. It does nothing in the game itself.
//
// How it works:
// - Each mark names a section, for example "Sensors", "Enemy" or "Keys".
// - The Tuning window looks through every script in the open scene for marked settings. It looks
//   inside their groups and lists of settings too.
// - The Board tab shows them grouped by section. The "Keys" section shows on the Keys tab.
// - A ship's numbers do not use this mark. They are in the ship's stats file in Assets/Ships, and
//   show on the Ship and Enemy tabs.
// - Many scripts use the mark. Among them are BoardManager, EnemyBrain, TacticalWorld, QuestSystem
//   and several board panels.
//
// Technical:
// - A plain attribute for fields only. It keeps one value, Section, a piece of text.
// - It changes nothing in the Inspector. Only StarwolfTuningWindow reads it.
// - The Tuning window's Walk() finds it by reflection. It looks at public fields and
//   [SerializeField] fields, up to 4 levels deep.
// - Trap: a marked field Unity does not save never shows. Private fields need [SerializeField].
// - Trap: the section is free text. A typo makes a new section. "Keys" goes to the Keys tab.
// - A marked field that is a group of settings shows with all its parts, as one entry.
// - It is outside Assets/Editor. Game scripts can use it.
// =================================================================================================

using System;

[AttributeUsage(AttributeTargets.Field)]
public class TuneAttribute : Attribute
{
    public readonly string Section;
    public TuneAttribute(string section) { Section = section; }
}
