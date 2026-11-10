// =================================================================================================
// WeaponType.cs
//
// What it does:
// The list of gun kinds for the older physics weapons, and the shape of one gun slot in
// WeaponManager. It is not put on any object. The battle board does not use this script.
//
// How it works:
// - The gun kinds are: Laser, Macrocannon, Missile (a torpedo), PointDefense (a PDC), BoardingPod,
//   Railgun and Broadside.
// - A gun slot has a gun kind, the gun on the ship and an on/off switch. It has a slot number
//   and a name. These show in the Inspector on WeaponManager.
// - WeaponManager, WeaponSystemExample, WeaponUIDisplay and the older UIController screen use
//   these to pick guns and icons.
//
// Technical:
// - WeaponType values, in order from 0: Laser, Macrocannon, Missile, PointDefense, BoardingPod,
//   Railgun, Broadside.
// - Trap: scenes and prefabs save the kind as that number. A name added or moved in the middle
//   changes every saved slot. Add new kinds at the end.
// - WeaponSlot fields: weaponType, weaponInstance (a WeaponBase), isActive, slotIndex and
//   slotName. It is marked [System.Serializable].
// - WeaponManager sets slotIndex and fills an empty slotName at the start.
// - UIController.GetWeaponIcon picks one icon per WeaponType. Broadside gets no icon.
// - Trap: nothing checks that a slot's kind matches its gun.
// =================================================================================================

using UnityEngine;

public enum WeaponType
{
    Laser,
    Macrocannon,
    Missile,
    PointDefense,
    BoardingPod,
    Railgun,
    Broadside
}

/// <summary>
/// Serializable data structure for weapon configurations
/// </summary>
[System.Serializable]
public class WeaponSlot
{
    public WeaponType weaponType;
    public WeaponBase weaponInstance;
    public bool isActive = true;
    
    [Header("Slot Configuration")]
    public int slotIndex;
    public string slotName;
}
