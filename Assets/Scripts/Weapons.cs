using Unity.Mathematics;
using UnityEngine;
// Add this if Laser is in the same namespace, otherwise adjust as needed


public class Weapons : MonoBehaviour
{
    public GameObject laserPrefab;
    public Transform firePoint; // Assign in inspector or set to ship's position
    public int maxAmmo = 5;
    public float rechargeTime = 3f; // Time to fully recharge
    public float fireRate = 0.25f; // Minimum time between shots

    private int currentAmmo;
    private float lastFireTime;
    private bool isRecharging;
    private float rechargeTimer;

    void Start()
    {
        currentAmmo = maxAmmo;
        isRecharging = false;
        rechargeTimer = 0f;
    }

    void Update()
    {
        // Fire if Space (keyboard) or X (controller) is pressed
        bool fireInput = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton2); // X is usually JoystickButton2
        if (fireInput && currentAmmo > 0 && Time.time - lastFireTime > fireRate && !isRecharging)
        {
            FireLaser();
        }

        // Start recharge if ammo depleted
        if (currentAmmo <= 0 && !isRecharging)
        {
            isRecharging = true;
            rechargeTimer = 0f;
        }

        // Handle recharge
        if (isRecharging)
        {
            rechargeTimer += Time.deltaTime;
            if (rechargeTimer >= rechargeTime)
            {
                currentAmmo = maxAmmo;
                isRecharging = false;
            }
        }
    }

    void FireLaser()
    {
        lastFireTime = Time.time;
        currentAmmo--;
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
        Quaternion spawnRot = firePoint ? firePoint.rotation : transform.rotation;
        GameObject laserObj = Instantiate(laserPrefab, spawnPos, quaternion.identity);
        Laser laser = laserObj.GetComponent<Laser>();
        if (laser != null)
        {
            laser.Fire(firePoint ? firePoint.forward : transform.forward);
        }
        else
        {
            // fallback: apply velocity directly if no Laser script
            Rigidbody rb = laserObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = (firePoint ? firePoint.forward : transform.forward) * 30f;
            }
            Destroy(laserObj, 3f);
        }
    }
}
