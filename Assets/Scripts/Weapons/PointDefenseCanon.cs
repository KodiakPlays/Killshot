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
