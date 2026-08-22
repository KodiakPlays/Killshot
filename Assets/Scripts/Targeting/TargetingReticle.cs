using UnityEngine;

/// <summary>
/// Simple world-space reticle. Expects a Renderer (mesh or sprite) on the same GameObject or a child.
/// Call SetPosition(...) each frame to keep it on the target point.
/// Call SetLocked(true) to make it red when target is locked.
/// </summary>
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
