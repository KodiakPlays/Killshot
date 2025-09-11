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
    [SerializeField] private float reloadTime = 5f;
    
    private Transform[] currentTargets;
    private bool[] tubeLoaded;
    private bool[] tubeLocked;
    private float[] lockProgress;

    protected override void Start()
    {
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
}
