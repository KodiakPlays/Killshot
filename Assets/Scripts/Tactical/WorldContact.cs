using UnityEngine;

/// <summary>
/// Step 34. A contact in the tactical world: where it is (this transform, in km) and which board
/// track it drives (HandoffBoardState.tracks, by key). TacticalWorld reads it once per board tick.
/// Its NavSystem, on the same object, moves it; its PowerSystem powers it; its SensorSystem hears and scans; its CommsSystem listens; its WeaponSystem fights; its HangarSystem launches; its DamageSystem takes the hits; its OrdersSystem sets the alert.
/// </summary>
[RequireComponent(typeof(NavSystem), typeof(PowerSystem), typeof(SensorSystem))]
[RequireComponent(typeof(CommsSystem), typeof(WeaponSystem), typeof(HangarSystem))]
[RequireComponent(typeof(DamageSystem), typeof(OrdersSystem))]
public class WorldContact : MonoBehaviour
{
    [Tooltip("The board track this contact drives, e.g. t07 (GARUDA).")]
    public string trackKey = "t07";

    /// <summary>Step 49: this ship's own model — the same one ours runs on (the enemy is our ship
    /// with different stats, FTUE_PLAN §4). It takes our hits. Undamaged at load.</summary>
    public HandoffBoardState Ship
    {
        get
        {
            if (ship == null) { ship = new HandoffBoardState(); ship.UseNav(GetComponent<NavSystem>()); ship.UsePower(GetComponent<PowerSystem>()); ship.UseSensors(GetComponent<SensorSystem>()); ship.UseComms(GetComponent<CommsSystem>()); ship.UseWeapons(GetComponent<WeaponSystem>()); ship.UseHangar(GetComponent<HangarSystem>()); ship.UseDamage(GetComponent<DamageSystem>()); ship.UseOrders(GetComponent<OrdersSystem>()); }
            return ship;
        }
    }
    [System.NonSerialized] private HandoffBoardState ship;
}
