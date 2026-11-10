// =================================================================================================
// Macrocannon.cs
//
// What it does:
// An older physics weapon: a big gun with three barrels that fires Shell shots. Shots hit harder
// the further away the target point is. It is one of the guns WeaponManager can switch between,
// but no scene or prefab has it. The battle board does not use this script.
//
// How it works:
// - It is built on WeaponBase. That gives it reload time, ammo and damage settings. At the start
//   its range becomes 12,500 units.
// - WeaponManager asks it to load. Each request loads one shell into a barrel after 1 second, up
//   to three. Each shell loaded uses one ammo.
// - It can fire when a shell is loaded, ammo is left and the reload time has passed.
// - Firing makes a shell at the next barrel. The shell flies straight out the front of the gun at
//   1,000 units per second.
// - Damage is 0.01 per unit of distance to the target point, but never less than the base damage.
//   Then it is multiplied by the damage factor.
// - It has a lock: 1 second after a lock request, it locks if the target is within range.
//   No script asks for a lock. It never locks. WeaponUIDisplay shows SCANNING for it.
// - Its viewfinder part is empty and does nothing.
//
// Technical:
// - It extends WeaponBase. It overrides Start, CanFire and Fire(point).
// - WeaponManager.LoadActiveWeapon calls ArmWeapon. WeaponManager.FireActiveWeapon calls Fire.
//   WeaponUIDisplay calls HasValidLock.
// - AttemptLock, ClearLock and EnterViewfinder have no caller.
// - Fire calls Shell.Initialize(gun forward times shellVelocity, damage). The shell flies along
//   the gun's transform.forward. It does not turn toward the target.
// - Numbers: barrels (3), shell speed (1,000), range (12,500) and damage per unit (0.01) are in
//   the Inspector. The 1-second load and lock times are fixed in code.
// - Trap: ArmWeapon checks the barrel count when asked, before the load ends. Four quick requests
//   load four shells. The next shot then gives an error.
// - Trap: loading takes ammo, and CanFire needs ammo above 0. Shells loaded with the last ammo
//   never fire.
// - Trap: the barrels list must have numBarrels entries. Fewer gives an error on firing.
// =================================================================================================

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
