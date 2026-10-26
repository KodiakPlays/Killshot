using UnityEngine;

/// <summary>
/// One ship's numbers, in one file (owner, 2026-10-27: a stats file per ship). Each ship part reads its
/// section: navigation, power, sensors, weapons, hangar, damage and orders. Starwolf → Tuning edits them, on
/// the Ship tab (our ship) and the Enemy tab. The files are in Assets/Ships. The enemy is our ship with other
/// stats: same sections, its own numbers.
/// </summary>
public class ShipStats : ScriptableObject
{
    [Tooltip("What the ship is, shown at the top of its Tuning tab.")]
    public string shipClass = "Asura-class destroyer";
    public NavSystem.Stats navigation = new NavSystem.Stats();
    public PowerSystem.Stats power = new PowerSystem.Stats();
    public SensorSystem.Stats sensors = new SensorSystem.Stats();
    public WeaponSystem.Stats weapons = new WeaponSystem.Stats();
    public HangarSystem.Stats hangar = new HangarSystem.Stats();
    public DamageSystem.Stats damage = new DamageSystem.Stats();
    public OrdersSystem.Stats orders = new OrdersSystem.Stats();
}
