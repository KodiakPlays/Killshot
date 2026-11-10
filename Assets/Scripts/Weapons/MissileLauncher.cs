// =================================================================================================
// MissileLauncher.cs
//
// What it does:
// An older physics weapon: a set of launch tubes that lock onto a ship and fire guided torpedoes
// (the Missile script). It is one of the guns WeaponManager can switch between.
// No scene or prefab has it. The battle board does not use this script.
//
// How it works:
// - It is built on WeaponBase. Settings left at 0 in the Inspector get defaults: 5 torpedoes,
//   5 seconds to reload a tube, 150 damage. Its range becomes the lock range, 25,000 units.
// - At the start it loads one torpedo into each tube, while ammo lasts.
// - WeaponManager asks it to lock every loaded tube onto a target. The target must be between 500
//   and 25,000 units away.
// - A tube locks after 2 seconds. It drops the lock if the target is gone or moves out of range.
// - Firing launches a torpedo from every tube that is loaded and locked. Each torpedo gets the
//   target and the base damage times the damage factor.
// - WeaponManager only fires it while spare ammo is left and the reload time has passed.
//   Torpedoes still in the tubes when the spare ammo runs out are never fired.
// - Each tube that fired reloads after the reload time, if ammo is left.
// - WeaponUIDisplay asks it for the first tube's lock progress.
//
// Technical:
// - It extends WeaponBase. It overrides Start, CanFire and Fire(point). Fire ignores the point.
// - Data: one entry per launch tube in tubeLoaded, tubeLocked, lockProgress (seconds) and
//   currentTargets.
// - WeaponManager.LoadActiveWeapon calls LockAllTubes(target). WeaponManager.FireActiveWeapon
//   calls Fire. WeaponUIDisplay calls GetLockProgress(0).
// - FireMissile calls Missile.Initialize(target, damage). The torpedo starts with the tube's
//   rotation.
// - Numbers: lock time (2 s), lock range (500 to 25,000), tubes and prefab are in the Inspector.
// - Trap: the tube arrays are sized in Start. With no launch tubes, GetLockProgress(0) gives an
//   error.
// - Trap: CanFire passes with a loaded tube that is not locked. Fire then launches nothing, and
//   WeaponManager still buzzes.
// - Trap: once locked, a tube never checks the range again.
// - Trap: a second lock request during the 2 seconds starts a second count. That tube then locks
//   in 1 second.
// =================================================================================================

using UnityEngine;
using System.Collections;

public class MissileLauncher : WeaponBase
{
    [Header("Missile Launcher Specific")]
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Transform[] launchTubes;
    [SerializeField] private float lockOnTime = 2f;
    [SerializeField] private float minLockRange = 500f;
    [SerializeField] private float maxLockRange = 25000f;
    
    private Transform[] currentTargets;
    private bool[] tubeLoaded;
    private bool[] tubeLocked;
    private float[] lockProgress;

    protected override void Start()
    {
        // Apply defaults for fields left at 0 in the Inspector
        if (maxAmmo == 0)      maxAmmo     = 5;
        if (angleOfFire == 0f) angleOfFire = 360f;  // guided — fire in any direction
        if (reloadTime == 0f)  reloadTime  = 5f;    // 5s between salvos
        if (baseDamage == 0f)  baseDamage  = 150f;

        base.Start();
        range = maxLockRange;
        
        // Initialize arrays
        currentTargets = new Transform[launchTubes.Length];
        tubeLoaded = new bool[launchTubes.Length];
        tubeLocked = new bool[launchTubes.Length];
        lockProgress = new float[launchTubes.Length];

        // Start with all tubes loaded
        for (int i = 0; i < tubeLoaded.Length; i++)
        {
            if (currentAmmo > 0)
            {
                tubeLoaded[i] = true;
                currentAmmo--;
            }
        }
    }

    public void AttemptLock(Transform target, int tubeIndex)
    {
        if (!tubeLoaded[tubeIndex] || tubeLocked[tubeIndex]) return;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance >= minLockRange && distance <= maxLockRange)
        {
            currentTargets[tubeIndex] = target;
            StartCoroutine(LockSequence(tubeIndex));
        }
    }

    private IEnumerator LockSequence(int tubeIndex)
    {
        lockProgress[tubeIndex] = 0f;
        
        while (lockProgress[tubeIndex] < lockOnTime)
        {
            // Check if target is still valid
            if (currentTargets[tubeIndex] == null ||
                Vector3.Distance(transform.position, currentTargets[tubeIndex].position) > maxLockRange)
            {
                ClearLock(tubeIndex);
                yield break;
            }

            lockProgress[tubeIndex] += Time.deltaTime;
            yield return null;
        }

        tubeLocked[tubeIndex] = true;
    }

    public void ClearLock(int tubeIndex)
    {
        currentTargets[tubeIndex] = null;
        tubeLocked[tubeIndex] = false;
        lockProgress[tubeIndex] = 0f;
    }

    public override bool CanFire()
    {
        return base.CanFire() && System.Array.Exists(tubeLoaded, loaded => loaded);
    }

    public override void Fire(Vector3 target)
    {
        for (int i = 0; i < launchTubes.Length; i++)
        {
            if (tubeLoaded[i] && tubeLocked[i] && currentTargets[i] != null)
            {
                FireMissile(i);
            }
        }
    }

    private void FireMissile(int tubeIndex)
    {
        GameObject missile = Instantiate(missilePrefab, launchTubes[tubeIndex].position, launchTubes[tubeIndex].rotation);
        Missile missileScript = missile.GetComponent<Missile>();
        if (missileScript != null)
        {
            missileScript.Initialize(currentTargets[tubeIndex], baseDamage * damageModifier);
        }

        tubeLoaded[tubeIndex] = false;
        tubeLocked[tubeIndex] = false;
        currentTargets[tubeIndex] = null;
        lastFireTime = Time.time;

        StartCoroutine(ReloadTube(tubeIndex));
    }

    private IEnumerator ReloadTube(int tubeIndex)
    {
        yield return new WaitForSeconds(reloadTime);
        
        if (currentAmmo > 0)
        {
            tubeLoaded[tubeIndex] = true;
            currentAmmo--;
        }
    }

    public float GetLockProgress(int tubeIndex)
    {
        return lockProgress[tubeIndex] / lockOnTime;
    }

    /// <summary>
    /// Attempts to lock all loaded, unlocked tubes onto the given target.
    /// Called by the UI load button.
    /// </summary>
    public void LockAllTubes(Transform target)
    {
        for (int i = 0; i < launchTubes.Length; i++)
            AttemptLock(target, i);
    }
}
