using UnityEngine;

/// <summary>
/// Marker component for objects that can be targeted by the targeting system.
/// Attach to enemies or other targetable objects.
/// </summary>
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
