using UnityEngine;

/// <summary>
/// Handles acquiring targets under the center of the screen, displays a world-space reticle,
/// and supports a simple toggle lock that turns the reticle red when locked.
/// Press the configured `lockKey` to lock/unlock the current target.
/// When a target is locked, the system will call `WeaponManager.LoadActiveWeapon(target)` if present.
/// </summary>
public class TargetingSystem : MonoBehaviour
{
    [Header("Detection")]
    public Camera cam;
    public float maxDistance = 1000f;
    public LayerMask targetLayers = ~0;

    [Header("Reticle")]
    public GameObject reticlePrefab; // world-space prefab with TargetingReticle
    private GameObject activeReticle;
    private TargetingReticle reticleScript;

    [Header("Locking")]
    public KeyCode lockKey = KeyCode.T;
    private Targetable currentTarget;
    private Targetable lockedTarget;

    [Header("Integration")]
    public WeaponManager weaponManager; // optional

    void Reset()
    {
        cam = Camera.main;
    }

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (weaponManager == null) weaponManager = FindObjectOfType<WeaponManager>();
        if (reticlePrefab == null && weaponManager != null)
        {
            reticlePrefab = weaponManager.targetingReticlePrefab;
        }

        if (reticlePrefab != null)
        {
            activeReticle = Instantiate(reticlePrefab, Vector3.zero, Quaternion.identity);
            reticleScript = activeReticle.GetComponent<TargetingReticle>();
            activeReticle.SetActive(false);
        }
    }

    void Update()
    {
        UpdateAim();
        HandleLockInput();
        UpdateReticleVisual();
    }

    private void UpdateAim()
    {
        currentTarget = null;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, targetLayers, QueryTriggerInteraction.Ignore))
        {
            var t = hit.collider.GetComponentInParent<Targetable>();
            if (t != null)
            {
                currentTarget = t;
                if (activeReticle != null)
                {
                    activeReticle.SetActive(true);
                    reticleScript?.SetPosition(t.targetPoint != null ? t.targetPoint.position : t.transform.position, cam);
                }
            }
            else
            {
                if (activeReticle != null) activeReticle.SetActive(false);
            }
        }
        else
        {
            if (activeReticle != null) activeReticle.SetActive(false);
        }
    }

    private void HandleLockInput()
    {
        if (Input.GetKeyDown(lockKey))
        {
            if (currentTarget != null && lockedTarget == currentTarget)
            {
                // unlock
                lockedTarget = null;
                weaponManager?.LoadActiveWeapon(null);
            }
            else if (currentTarget != null)
            {
                // lock current
                lockedTarget = currentTarget;
                weaponManager?.LoadActiveWeapon(lockedTarget.targetPoint != null ? lockedTarget.targetPoint : lockedTarget.transform);
            }
        }
    }

    private void UpdateReticleVisual()
    {
        if (reticleScript == null) return;
        bool isLocked = (currentTarget != null && lockedTarget == currentTarget);
        reticleScript.SetLocked(isLocked);
    }

    /// <summary>
    /// Public accessor for the locked target.
    /// </summary>
    public Transform GetLockedTargetTransform()
    {
        return lockedTarget != null ? (lockedTarget.targetPoint != null ? lockedTarget.targetPoint : lockedTarget.transform) : null;
    }
}
