// =================================================================================================
// WorldContact.cs
//
// What it does:
// A WorldContact marks a ship in the world that is not our ship: the Garuda, an arriving enemy,
// or a ship of our battle group. It links that ship to its track on the board. In the board scene
// it is on the Garuda, and on the switched-off copy arriving enemies are made from. The battle
// group prefabs (Indra, Marut, Varuna) have it too.
//
// How it works:
// - It names the board track the ship drives, for example t07 for the Garuda.
// - The friendly setting marks a ship of our battle group. Its name and class on the board are
//   read from the object's name, for example INDRA and CARRIER.
// - The cannot-be-destroyed setting marks a mission ship that must stay in the fight. It is
//   copied into the ship record.
// - The object needs the same ship parts as our ship: NavSystem, PowerSystem,
//   SensorSystem, CommsSystem, WeaponSystem, HangarSystem, DamageSystem and OrdersSystem.
// - The first time another script asks for its Ship, it builds one ship record from those parts.
//   The enemy runs on the same rules as our ship, with different numbers.
// - TacticalWorld finds every WorldContact under it and moves them. It writes their bearing and
//   range into their tracks. EnemyBrain needs one on the same object.
// - ShipView, PlotPanel and PeriscopePanel use it to tell an enemy's model from ours.
// =================================================================================================

using UnityEngine;

/// <summary>
/// Step 34. A contact in the tactical world: where it is (this transform, in units) and which board
/// track it drives (ShipState.tracks, by key). TacticalWorld reads it once per board tick.
/// Its NavSystem, on the same object, moves it; its PowerSystem powers it; its SensorSystem hears and scans; its CommsSystem listens; its WeaponSystem fights; its HangarSystem launches; its DamageSystem takes the hits; its OrdersSystem sets the alert.
/// </summary>
[RequireComponent(typeof(NavSystem), typeof(PowerSystem), typeof(SensorSystem))]
[RequireComponent(typeof(CommsSystem), typeof(WeaponSystem), typeof(HangarSystem))]
[RequireComponent(typeof(DamageSystem), typeof(OrdersSystem))]
public class WorldContact : MonoBehaviour
{
    [Tooltip("The board track this contact drives, e.g. t07 (GARUDA).")]
    public string trackKey = "t07";
    [Tooltip("Owner 2026-11-09: one of our battle group. Placed in a level, its name and class come from this object's name, "
           + "\"Indra · Carrier\". It shows blue once scanned and can't be targeted. Its brain should have Hunts off and Escorts on (it flies in formation with us).")]
    public bool friendly;
    [Tooltip("Owner 2026-11-10: a mission piece that can't be destroyed (MILK RUN's pirate corvette). Hits still strip its armour, "
           + "but its reactor never goes below 1, its crew can't fight off our marines, and marines sent to DESTROY it capture it instead.")]
    public bool cannotBeDestroyed;

    /// <summary>Step 49: this ship's own model — the same one ours runs on (the enemy is our ship
    /// with different stats, FTUE_PLAN §4). It takes our hits. Undamaged at load.</summary>
    public ShipState Ship
    {
        get
        {
            if (ship == null) { ship = new ShipState(); ship.UseNav(GetComponent<NavSystem>()); ship.UsePower(GetComponent<PowerSystem>()); ship.UseSensors(GetComponent<SensorSystem>()); ship.UseComms(GetComponent<CommsSystem>()); ship.UseWeapons(GetComponent<WeaponSystem>()); ship.UseHangar(GetComponent<HangarSystem>()); ship.UseDamage(GetComponent<DamageSystem>()); ship.UseOrders(GetComponent<OrdersSystem>()); ship.cannotBeDestroyed = cannotBeDestroyed; }
            return ship;
        }
    }
    [System.NonSerialized] private ShipState ship;
}
