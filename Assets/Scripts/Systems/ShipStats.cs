// =================================================================================================
// ShipStats.cs
//
// What it does:
// The shape of a ship's stats file: all of one ship's game numbers in one file. The files are in
// Assets/Ships, one per ship, for example Asura and Garuda. The Tuning window edits them.
//
// How it works:
// - A file has the ship's class name, its length and its width in units. The length and width
//   make the ship's box. Shots and crashes hit the box.
// - It then has one section per ship system: navigation, power, sensors, weapons, hangar, damage
//   and orders. Each section is the list of numbers for that system.
// - Each ship object points at its file through the Ship script. Each system reads its section
//   from there.
// - The Tuning window edits the files on its Ship, Enemy, Hard Points and Ranges tabs. Hard Points
//   and Ranges let you pick any stats file.
//
// Technical:
// - Data: shipClass, length and width (units), then one Stats section per system: navigation,
//   power, sensors, weapons, hangar, damage and orders.
// - Each section's fields are written in its system's script, as NavSystem.Stats,
//   PowerSystem.Stats and so on. Add a number there.
// - Ship.Stats gives the file in the editor and a copy of it in play mode.
// - NavSystem.BoxHalf reads length and width. Ship sizes the model to length.
// - StarwolfTuningWindow finds every ShipStats file in the project. It edits them and calls
//   Ship.PullFromFile in play mode.
// - Trap: there is no Create menu item. Copy an existing file to make a new ship.
// =================================================================================================

using UnityEngine;

public class ShipStats : ScriptableObject
{
    [Tooltip("What the ship is, shown at the top of its Tuning tab.")]
    public string shipClass = "Asura-class destroyer";
    [Tooltip("Length, units (owner 2026-10-29). At x1 a small plot square is 50 units; the plot draws the ship this long.")]
    public float length = 100f;
    [Tooltip("Width, units (owner 2026-11-01). The ship is a solid rectangle, this long and this wide, round its model: shots and crashes hit it. The Hard Points tab of Starwolf → Tuning draws it.")]
    public float width = 64f;
    public NavSystem.Stats navigation = new NavSystem.Stats();
    public PowerSystem.Stats power = new PowerSystem.Stats();
    public SensorSystem.Stats sensors = new SensorSystem.Stats();
    public WeaponSystem.Stats weapons = new WeaponSystem.Stats();
    public HangarSystem.Stats hangar = new HangarSystem.Stats();
    public DamageSystem.Stats damage = new DamageSystem.Stats();
    public OrdersSystem.Stats orders = new OrdersSystem.Stats();
}
