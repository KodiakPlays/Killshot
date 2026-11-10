// =================================================================================================
// Targetable.cs
//
// What it does:
// Targetable marks a 3D object that the older TargetingSystem can aim at and lock. It is part of
// the older physics ships and guns. The battle board does not use this script. No scene or prefab
// has it.
//
// How it works:
// - It has one setting: the point to aim at. If that is left empty, it uses the object's
//   position.
// - It fills an empty aim point with the object itself. It does this when it is added in the
//   editor, and again when play starts.
// - TargetingSystem looks for it on whatever is under the middle of the screen. Then it puts its
//   reticle on the aim point.
//
// Technical:
// - One public field: targetPoint, a Transform. Reset() and Awake() set it to this transform when
//   it is empty.
// - TargetingSystem finds it with GetComponentInParent<Targetable>() on the collider its ray hits.
// - Trap: it needs a collider on the object or a child. Without one the ray never finds it.
// - No other script uses it. Board targeting is the lock in WeaponSystem and PeriscopePanel.
// =================================================================================================

using UnityEngine;

public class Targetable : MonoBehaviour
{
    [Tooltip("Optional explicit point to aim at. If null, object's transform is used.")]
    public Transform targetPoint;

    void Reset()
    {
        if (targetPoint == null) targetPoint = this.transform;
    }

    void Awake()
    {
        if (targetPoint == null) targetPoint = this.transform;
    }
}
