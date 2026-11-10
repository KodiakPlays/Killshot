// =================================================================================================
// BoardingPodLauncher.cs
//
// What it does:
// An older physics weapon that fires a BoardingPod at a chosen ship. WeaponManager can switch to
// it when it is in WeaponManager's gun list. It is on the PlayerShip prefab, with no pod prefab,
// no ammo, and not in the gun list. The battle board does not use this script.
//
// How it works:
// - It is built on WeaponBase. That gives it range, reload time, ammo and damage settings.
// - At the start its range becomes the launch range, 5,000 units by default. It loads one pod
//   from its ammo.
// - WeaponManager passes on the target. It comes from a TargetingSystem lock or the load button
//   on the older UIController screen. A target closer than 200 units or further than 5,000 is
//   refused.
// - It can fire only when loaded, with a valid target, ammo left and the reload time passed.
// - Firing makes a pod at the launch tube, moving forward at 500 units per second. The pod then
//   steers to the target. Its damage is the base damage times the damage factor.
// - After the reload time it loads the next pod, if any ammo is left.
//
// Technical:
// - It extends WeaponBase. It overrides Start, CanFire and Fire(point).
// - SetTarget(transform) is called by WeaponManager.LoadActiveWeapon. TargetingSystem and
//   UIController call that. ClearTarget has no outside caller.
// - Fire ignores the point it is given. It aims the pod at the target from SetTarget.
// - It calls BoardingPod.Initialize(target, launcher forward times podVelocity, damage).
// - Numbers: pod speed (500), launch range (200 to 5,000), ammo and reload are in the Inspector.
// - Trap: the range is checked once, in SetTarget. A target that moves out of range stays valid.
// - Trap: loading takes a pod from the ammo, and CanFire needs ammo above 0. The last pod loads
//   but never fires.
// - Trap: Fire needs boardingPodPrefab and launchTube set. Without them it gives an error.
// - Trap: the pod keeps only the speed it is given. It flies along the pod's forward, set by the
//   launch tube.
// =================================================================================================

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
