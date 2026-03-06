using UnityEngine;
using System.Collections;

public class Macrocannon : WeaponBase
{
    [Header("Macrocannon Specific")]
    [SerializeField] private int numBarrels = 3;
    [SerializeField] private float shellVelocity = 1000f;
    [SerializeField] private float maxRange = 12500f;
    [SerializeField] private float damagePerUnit = 0.01f;
    [SerializeField] private Transform[] barrels;
    [SerializeField] private GameObject shellPrefab;
    
    private int loadedShells = 0;
    private bool isArmed = false;
    private Transform currentTarget;
    private bool hasLock = false;

    protected override void Start()
    {
        base.Start();
        range = maxRange;
    }

    public void EnterViewfinder()
    {
        // Switch to gun's targeting interface
        // Implementation depends on your UI system
    }

    public void ArmWeapon()
    {
        if (loadedShells < numBarrels && currentAmmo > 0)
        {
            StartCoroutine(LoadShell());
        }
    }

    private IEnumerator LoadShell()
    {
        isArmed = true;
        float loadTime = 1f; // Time to load each shell
        yield return new WaitForSeconds(loadTime);
        loadedShells++;
        currentAmmo--;
    }

    public override bool CanFire()
    {
        return base.CanFire() && loadedShells > 0 && isArmed;
    }

    public override void Fire(Vector3 target)
    {
        if (!CanFire()) return;

        // Calculate damage based on distance
        float distance = Vector3.Distance(transform.position, target);
        float damageAtDistance = CalculateDamage(distance);

        // Fire the shell
        GameObject shell = Instantiate(shellPrefab, barrels[numBarrels - loadedShells].position, transform.rotation);
        Shell shellScript = shell.GetComponent<Shell>();
        if (shellScript != null)
        {
            shellScript.Initialize(transform.forward * shellVelocity, damageAtDistance);
        }

        loadedShells--;
        lastFireTime = Time.time;

        if (loadedShells <= 0)
        {
            isArmed = false;
        }
    }

    private float CalculateDamage(float distance)
    {
        float damageFromDistance = distance * damagePerUnit;
        float totalDamage = Mathf.Max(baseDamage, damageFromDistance);
        return totalDamage * damageModifier;
    }

    public void AttemptLock(Transform target)
    {
        currentTarget = target;
        StartCoroutine(LockSequence());
    }

    private IEnumerator LockSequence()
    {
        float lockTime = 1f; // Time to achieve lock
        yield return new WaitForSeconds(lockTime);
        
        if (currentTarget != null && 
            Vector3.Distance(transform.position, currentTarget.position) <= maxRange)
        {
            hasLock = true;
        }
    }

    public void ClearLock()
    {
        hasLock = false;
        currentTarget = null;
    }

    public bool HasValidLock()
    {
        return hasLock && currentTarget != null;
    }
}
