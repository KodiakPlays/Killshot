// =================================================================================================
// PointDefenseCanon.cs
//
// What it does:
// An older physics PDC: a fast gun with several barrels that fires PDCBullet shots. It is one of
// the guns WeaponManager can switch between, but no scene or prefab has it. The battle board does
// not use this script. The board's PDCs are in WeaponSystem.
//
// How it works:
// - It is built on WeaponBase. That gives it reload time, ammo and damage settings.
// - It must spin up before it can fire. Spinning up takes 0.5 seconds. No script asks it to spin
//   up. It never fires.
// - It fires only with ammo left and once the reload time has passed after the last shot.
//   It works out a rate of 30 shots a second. The fire check does not use that rate.
// - Each shot leaves the next barrel in turn, straight out the front of the gun. It is turned up
//   to 2 degrees at random. It flies at 2,000 units per second and uses one ammo.
// - Each bullet's damage is the base damage times the damage factor.
// - If given a target, it turns toward it every frame at up to 45 degrees per second. No script
//   gives it a target.
//
// Technical:
// - It extends WeaponBase. It overrides Start, CanFire and Fire(point). Fire ignores the point.
// - WeaponManager.FireActiveWeapon calls CanFire and Fire. SpinUp, SpinDown and TrackTarget have
//   no caller.
// - Fire calls PDCBullet.Initialize(direction times bulletVelocity, damage).
// - Numbers: rate (30), spread (2 degrees), bullet speed (2,000) and turn speed (45) are in the
//   Inspector. The 0.5-second spin-up is fixed in code.
// - Trap: Start sets fireRate from rateOfFire. WeaponBase.CanFire checks reloadTime instead.
// - Trap: it fires along transform.forward. The older ships face along transform.up. The gun
//   object must be turned to match.
// - Trap: the gunBarrels list must not be empty, or firing gives an error.
// =================================================================================================

using UnityEngine;
using System.Collections;

public class PointDefenseCanon : WeaponBase
{
    [Header("PDC Specific")]
    [SerializeField] private float rateOfFire = 30f; // rounds per second
    [SerializeField] private float spread = 2f; // bullet spread in degrees
    [SerializeField] private float bulletVelocity = 2000f;
    [SerializeField] private Transform[] gunBarrels;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float maxTrackingSpeed = 45f; // degrees per second
    
    private Transform currentTarget;
    private bool isSpunUp = false;
    private float spinUpTime = 0.5f;
    private int currentBarrel = 0;

    protected override void Start()
    {
        base.Start();
        fireRate = 1f / rateOfFire; // Convert rate of fire to time between shots
    }

    public override bool CanFire()
    {
        return base.CanFire() && isSpunUp && currentAmmo > 0;
    }

    public void SpinUp()
    {
        if (!isSpunUp)
        {
            StartCoroutine(SpinUpRoutine());
        }
    }

    private IEnumerator SpinUpRoutine()
    {
        yield return new WaitForSeconds(spinUpTime);
        isSpunUp = true;
    }

    public void SpinDown()
    {
        isSpunUp = false;
    }

    public override void Fire(Vector3 target)
    {
        if (!CanFire()) return;

        // Calculate spread
        Vector3 spreadDirection = CalculateSpread(transform.forward);
        
        // Fire bullet
        GameObject bullet = Instantiate(bulletPrefab, gunBarrels[currentBarrel].position, Quaternion.LookRotation(spreadDirection));
        PDCBullet bulletScript = bullet.GetComponent<PDCBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(spreadDirection * bulletVelocity, baseDamage * damageModifier);
        }

        // Update firing status
        currentBarrel = (currentBarrel + 1) % gunBarrels.Length;
        currentAmmo--;
        lastFireTime = Time.time;
    }

    private Vector3 CalculateSpread(Vector3 forward)
    {
        // Add random spread
        float randomSpreadX = Random.Range(-spread, spread);
        float randomSpreadY = Random.Range(-spread, spread);
        return Quaternion.Euler(randomSpreadX, randomSpreadY, 0) * forward;
    }

    public void TrackTarget(Transform target)
    {
        currentTarget = target;
        // Implement smooth rotation towards target with maxTrackingSpeed limit
    }

    private void Update()
    {
        if (currentTarget != null)
        {
            // Calculate desired rotation to face target
            Vector3 targetDirection = (currentTarget.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            
            // Smoothly rotate towards target with speed limit
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                maxTrackingSpeed * Time.deltaTime
            );
        }
    }
}
