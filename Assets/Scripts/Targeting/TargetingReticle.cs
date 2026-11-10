// =================================================================================================
// TargetingReticle.cs
//
// What it does:
// This is the aiming mark the older TargetingSystem shows over a target in the 3D world. It is
// part of the older physics ships and guns. The battle board does not use this script. No scene
// or prefab has it.
//
// How it works:
// - It needs a model or sprite on the same object or under it. It finds it by itself if none is
//   set.
// - TargetingSystem moves it onto the target's aim point every frame. It turns to face the camera.
// - SetLocked colours it red when locked and white when not. Both colours can be set in the
//   Inspector.
// - The reticle prefab can come from TargetingSystem, or from WeaponManager's reticle setting.
//
// Technical:
// - Inspector: unlockedColor (white), lockedColor (red) and targetRenderer. Reset() and Awake()
//   fill targetRenderer with the first Renderer found under it.
// - SetPosition(worldPosition, cam) moves it there. It turns it to point the same way as the
//   camera.
// - SetLocked(bool) sets targetRenderer.material.color.
// - Only TargetingSystem calls it: SetPosition while a target is under the middle of the screen,
//   and SetLocked every frame.
// - Trap: .material makes a copy of the material for this reticle. The prefab's material stays
//   as it is.
// - Trap: the material's shader needs a main colour, or SetLocked shows no change.
// =================================================================================================

using UnityEngine;

public class TargetingReticle : MonoBehaviour
{
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Renderer targetRenderer;

    void Reset()
    {
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
    }

    void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
    }

    public void SetLocked(bool locked)
    {
        if (targetRenderer == null) return;
        var mat = targetRenderer.material;
        if (mat == null) return;
        mat.color = locked ? lockedColor : unlockedColor;
    }

    public void SetPosition(Vector3 worldPosition, Camera cam)
    {
        transform.position = worldPosition;
        if (cam != null)
        {
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward);
        }
    }
}
