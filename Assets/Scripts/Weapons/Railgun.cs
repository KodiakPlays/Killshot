using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Hold Space to charge the railgun (requires Arms power > 90%).
/// Release Space to fire in the ship's facing direction.
/// Enemies in the beam path are destroyed instantly.
/// After firing, all power is drained and the ship enters a brief standby before rebooting.
/// </summary>
public class Railgun : WeaponBase
{
    [Header("Railgun Settings")]
    [SerializeField] private float maxRange = 2000f;
    [SerializeField] private float chargeTime = 2f; // Visual reference for charge-progress UI
    [SerializeField] private int maxPenetrations = 10;
    [SerializeField] private float damageDropoffPerPenetration = 0.15f;

    [Header("Railgun Standby")]
    [SerializeField] private float standbyDuration = 4f; // Seconds the ship is offline after firing

    [Header("Visual Effects")]
    [SerializeField] private LineRenderer beamRenderer;
    [SerializeField] private float beamDuration = 0.3f;
    [SerializeField] private float beamWidth = 0.5f;
    [SerializeField] private Color beamColor = Color.cyan;
    [SerializeField] private GameObject chargeEffect;
    [SerializeField] private GameObject impactEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip fireSound;

    [Header("Fire Point")]
    [SerializeField] private Transform firePoint;

    private bool isCharging = false;
    private bool isOnStandby = false;
    private float chargeStartTime;
    private AudioSource audioSource;
    private PowerManager powerManager;
    private ShipStability shipStability;
    private PlayerShip playerShip;
    private WeaponManager weaponManager;
    private bool _ltRtBothPrevFrame = false;
    private Coroutine _chargeRumbleCoroutine;

    protected override void Start()
    {
        base.Start();

        range = maxRange;

        if (firePoint == null)
            firePoint = transform;

        powerManager  = GetComponentInParent<PowerManager>();
        shipStability  = GetComponentInParent<ShipStability>();
        playerShip    = GetComponentInParent<PlayerShip>();
        weaponManager = GetComponentInParent<WeaponManager>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (chargeSound != null || fireSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (beamRenderer == null)
        {
            GameObject beamObject = new GameObject("RailgunBeam");
            beamObject.transform.SetParent(transform);
            beamRenderer = beamObject.AddComponent<LineRenderer>();
            SetupLineRenderer();
        }
        else
        {
            SetupLineRenderer();
        }

        beamRenderer.enabled = false;
    }

    private void Update()
    {
        if (isOnStandby) return;

        // Only handle input when this is the active weapon
        if (weaponManager != null && weaponManager.GetActiveWeapon() != this) return;

        // Keyboard: Space to charge and release to fire
        if (Input.GetKeyDown(KeyCode.Space))
            TryStartCharging();

        if (Input.GetKeyUp(KeyCode.Space) && isCharging)
        {
            if (chargeEffect != null) chargeEffect.SetActive(false);
            FireRailgun();
        }

        // Controller: hold LT + RT to charge, release RT (while LT held) to fire
        float ltAxis = Input.GetAxis("LeftTrigger");
        float rtAxis = Input.GetAxis("RightTrigger");
        bool ltRtBoth = ltAxis > 0.5f && rtAxis > 0.5f;

        if (ltRtBoth && !_ltRtBothPrevFrame)
            TryStartCharging();

        if (!ltRtBoth && _ltRtBothPrevFrame && isCharging && ltAxis > 0.5f)
        {
            // RT released while LT still held → fire
            if (chargeEffect != null) chargeEffect.SetActive(false);
            FireRailgun();
        }

        _ltRtBothPrevFrame = ltRtBoth;
    }

    private void TryStartCharging()
    {
        if (isCharging) return;

        if (powerManager == null)
        {
            Debug.LogWarning("[Railgun] No PowerManager found.");
            return;
        }

        float armsEfficiency = powerManager.GetSystemEfficiency("arms");
        if (armsEfficiency <= 0.9f)
        {
            Debug.Log($"[Railgun] Cannot charge: Arms power at {armsEfficiency * 100f:F0}% (need > 90%)");
            return;
        }

        isCharging = true;
        chargeStartTime = Time.time;

        // Begin draining all ship power during charge
        powerManager.VentAllSystems();

        if (chargeEffect != null) chargeEffect.SetActive(true);

        if (audioSource != null && chargeSound != null)
            audioSource.PlayOneShot(chargeSound);

        _chargeRumbleCoroutine = StartCoroutine(ChargeRumble());

        Debug.Log("[Railgun] Charging...");
    }

    // --- Haptics ---

    private IEnumerator ChargeRumble()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) yield break;

        while (isCharging)
        {
            float t = Mathf.Clamp01((Time.time - chargeStartTime) / chargeTime);
            // Left motor: low-frequency rumble escalates 0.05 → 0.55
            // Right motor: high-frequency buzz escalates 0.1 → 0.4
            gamepad.SetMotorSpeeds(Mathf.Lerp(0.05f, 0.55f, t), Mathf.Lerp(0.1f, 0.4f, t));
            yield return null;
        }

        StopChargeRumble();
    }

    private void StopChargeRumble()
    {
        if (_chargeRumbleCoroutine != null)
        {
            StopCoroutine(_chargeRumbleCoroutine);
            _chargeRumbleCoroutine = null;
        }
        Gamepad.current?.SetMotorSpeeds(0f, 0f);
    }

    private IEnumerator FireRumble()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) yield break;

        // Sharp full-power kick for 0.15 s
        gamepad.SetMotorSpeeds(1f, 1f);
        yield return new WaitForSeconds(0.15f);

        // Decay to medium rumble for another 0.25 s (recoil echo)
        gamepad.SetMotorSpeeds(0.4f, 0.2f);
        yield return new WaitForSeconds(0.25f);

        gamepad.SetMotorSpeeds(0f, 0f);
    }

    private void SetupLineRenderer()
    {
        beamRenderer.startWidth = beamWidth;
        beamRenderer.endWidth = beamWidth * 0.5f; // Taper the beam
        beamRenderer.material = new Material(Shader.Find("Sprites/Default"));
        beamRenderer.startColor = beamColor;
        beamRenderer.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, 0.3f);
        beamRenderer.positionCount = 2;
        beamRenderer.useWorldSpace = true;
        
        // Optional: Add texture for more interesting beam
        // beamRenderer.textureMode = LineTextureMode.Tile;
    }

    public override bool CanFire() => false; // Railgun manages its own input via Update()

    public override void Fire(Vector3 target) { } // Disabled — railgun self-fires through Update()

    public void FireInstant()
    {
        if (!isCharging && !isOnStandby)
            FireRailgun();
    }

    private void FireRailgun()
    {
        isCharging = false;
        lastFireTime = Time.time;

        StopChargeRumble();
        StartCoroutine(FireRumble());

        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);

        // Instantly drain all remaining power
        if (powerManager != null)
            powerManager.DrainAllPowerInstantly();

        // Fire in the ship's facing direction
        Transform shipTransform = playerShip != null ? playerShip.transform : transform.root;
        Vector3 fireDirection = shipTransform.up;
        Vector3 fireOrigin = firePoint != null ? firePoint.position : transform.position;

        // Penetrating raycast
        List<RaycastHit> allHits = new List<RaycastHit>();
        Vector3 currentOrigin = fireOrigin;
        float remainingRange = maxRange;
        float currentDamage = baseDamage * damageModifier;
        int penetrationCount = 0;

        while (remainingRange > 0 && penetrationCount <= maxPenetrations)
        {
            RaycastHit hit;
            if (Physics.Raycast(currentOrigin, fireDirection, out hit, remainingRange))
            {
                allHits.Add(hit);
                ApplyDamage(hit, currentDamage);
                SpawnImpactEffect(hit.point, hit.normal);
                currentDamage *= (1f - damageDropoffPerPenetration);

                if (ShouldStopBeam(hit)) break;

                currentOrigin = hit.point + fireDirection * 0.1f;
                remainingRange -= hit.distance + 0.1f;
                penetrationCount++;
            }
            else break;
        }

        Vector3 beamEnd = allHits.Count > 0
            ? allHits[allHits.Count - 1].point
            : fireOrigin + fireDirection * maxRange;

        StartCoroutine(DrawBeam(fireOrigin, beamEnd));
        StartCoroutine(PostFireStandby());

        // Trigger the weapon screen UI effect
        if (UIController.Instance != null)
            UIController.Instance.RailFire();

        Debug.Log($"[Railgun] Fired! Cut through {allHits.Count} objects.");
    }

    private void ApplyDamage(RaycastHit hit, float damage)
    {
        // Enemies are destroyed instantly by the railgun
        EnemyShip enemyShip = hit.collider.GetComponent<EnemyShip>();
        if (enemyShip != null)
        {
            enemyShip.TakeDamage(9999);
            return;
        }

        PlayerShip hitPlayer = hit.collider.GetComponent<PlayerShip>();
        if (hitPlayer != null)
        {
            if (!CompareTag(hit.collider.tag))
                hitPlayer.TakeDamage(damage);
            return;
        }

        Shields shields = hit.collider.GetComponent<Shields>();
        if (shields != null)
        {
            shields.TakeDamage(damage);
            return;
        }

        IDamageable damageable = hit.collider.GetComponent<IDamageable>();
        if (damageable != null && damageable.CanBeDamaged())
        {
            damageable.TakeDamage(damage);
            return;
        }

        if (hit.collider.CompareTag("Asteroid") || hit.collider.CompareTag("Debris"))
            Destroy(hit.collider.gameObject);
    }

    private bool ShouldStopBeam(RaycastHit hit)
    {
        return hit.collider.CompareTag("HeavyArmor") || hit.collider.CompareTag("Station");
    }

    private void SpawnImpactEffect(Vector3 position, Vector3 normal)
    {
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, position, Quaternion.LookRotation(normal));
            Destroy(effect, 2f);
        }
    }

    private IEnumerator DrawBeam(Vector3 start, Vector3 end)
    {
        beamRenderer.enabled = true;
        beamRenderer.SetPosition(0, start);
        beamRenderer.SetPosition(1, end);

        float elapsed = 0f;
        while (elapsed < beamDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / beamDuration;
            float alpha = 1f - t;

            Color startColor = beamColor;
            startColor.a = alpha;
            Color endColor = startColor;
            endColor.a = alpha * 0.3f;

            beamRenderer.startColor = startColor;
            beamRenderer.endColor = endColor;
            beamRenderer.startWidth = beamWidth * (1f - t * 0.5f);
            beamRenderer.endWidth = beamWidth * 0.5f * (1f - t * 0.5f);

            yield return null;
        }

        beamRenderer.enabled = false;
    }

    private IEnumerator PostFireStandby()
    {
        isOnStandby = true;

        if (playerShip != null)
            playerShip.EnterRailgunStandby(standbyDuration);

        yield return new WaitForSeconds(standbyDuration);

        isOnStandby = false;

        // Reboot reactor — regen kicks in and engines/arms start drawing
        if (powerManager != null)
            powerManager.RebootReactor();

        Debug.Log("[Railgun] Ready.");
    }

    public void CancelCharge()
    {
        if (isCharging)
        {
            isCharging = false;
            if (chargeEffect != null) chargeEffect.SetActive(false);
        }
    }

    // Public state accessors for UI
    public bool IsCharging() => isCharging;
    public bool IsOnStandby() => isOnStandby;
    public float GetChargeProgress() => isCharging ? Mathf.Clamp01((Time.time - chargeStartTime) / chargeTime) : 0f;
    public float GetChargeTime() => chargeTime;
    public int GetMaxPenetrations() => maxPenetrations;
    public float GetMaxRange() => maxRange;
    public new int GetCurrentAmmo() => currentAmmo;
    public new int GetMaxAmmo() => maxAmmo;
}
