using UnityEngine;

/// <summary>
/// Defines the different types of weapons available in the game
/// </summary>
public enum WeaponType
{
    Laser,
    Macrocannon,
    Missile,
    PointDefense,
    BoardingPod
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
