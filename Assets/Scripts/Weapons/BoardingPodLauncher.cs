using UnityEngine;
using System.Collections;

public class BoardingPodLauncher : WeaponBase
{
    [Header("Boarding Pod Specific")]
    [SerializeField] private GameObject boardingPodPrefab;
    [SerializeField] private Transform launchTube;
    [SerializeField] private float podVelocity = 500f;
    [SerializeField] private float maxLaunchRange = 5000f;
    [SerializeField] private float minLaunchRange = 200f;
    [SerializeField] private float reloadTime = 10f;
    
    private bool isLoaded = false;
    private Transform currentTarget;
    private bool hasValidTarget = false;

    protected override void Start()
    {
        base.Start();
        range = maxLaunchRange;
        if (currentAmmo > 0)
        {
            LoadPod();
        }
    }

    private void LoadPod()
    {
        isLoaded = true;
        currentAmmo--;
    }

    public void SetTarget(Transform target)
    {
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance >= minLaunchRange && distance <= maxLaunchRange)
        {
            currentTarget = target;
            hasValidTarget = true;
        }
        else
        {
            ClearTarget();
        }
    }

    public void ClearTarget()
    {
        currentTarget = null;
        hasValidTarget = false;
    }

    public override bool CanFire()
    {
        return base.CanFire() && isLoaded && hasValidTarget;
    }

    public override void Fire(Vector3 target)
    {
        if (!CanFire()) return;

        // Launch the boarding pod
        GameObject pod = Instantiate(boardingPodPrefab, launchTube.position, launchTube.rotation);
        BoardingPod podScript = pod.GetComponent<BoardingPod>();
        if (podScript != null)
        {
            podScript.Initialize(currentTarget, transform.forward * podVelocity, baseDamage * damageModifier);
        }

        isLoaded = false;
        lastFireTime = Time.time;

        StartCoroutine(ReloadSequence());
    }

    private IEnumerator ReloadSequence()
    {
        yield return new WaitForSeconds(reloadTime);
        
        if (currentAmmo > 0)
        {
            LoadPod();
        }
    }
}
