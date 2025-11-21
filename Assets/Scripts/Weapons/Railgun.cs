using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// High-damage, low fire-rate penetrating weapon that shoots through multiple targets
/// Uses LineRenderer for instant beam visual effect
/// </summary>
public class Railgun : WeaponBase
{
    [Header("Railgun Settings")]
    [SerializeField] private float maxRange = 2000f;
    [SerializeField] private float chargeTime = 2f; // Time to charge before firing
    [SerializeField] private int maxPenetrations = 5; // How many objects it can pierce
    [SerializeField] private float damageDropoffPerPenetration = 0.15f; // 15% damage loss per penetration
    
    [Header("Visual Effects")]
    [SerializeField] private LineRenderer beamRenderer;
    [SerializeField] private float beamDuration = 0.3f;
    [SerializeField] private float beamWidth = 0.5f;
    [SerializeField] private Color beamColor = Color.cyan;
    [SerializeField] private GameObject chargeEffect; // Optional charging visual
    [SerializeField] private GameObject impactEffect; // Impact particle effect
    
    [Header("Audio")]
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip fireSound;
    
    [Header("Fire Point")]
    [SerializeField] private Transform firePoint;

    private bool isCharging = false;
    private float chargeStartTime;
    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        
        // Set range from settings
        range = maxRange;
        
        // Set up fire point
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (chargeSound != null || fireSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Set up line renderer
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

    public override bool CanFire()
    {
        return base.CanFire() && !isCharging;
    }

    public override void Fire(Vector3 target)
    {
        if (!CanFire()) return;
        
        // Start charging
        StartCoroutine(ChargeAndFire(target));
    }

    /// <summary>
    /// Fire the railgun immediately without charging (for AI or testing)
    /// </summary>
    public void FireInstant(Vector3 target)
    {
        if (!base.CanFire()) return;
        
        FireRailgun(target);
    }

    private IEnumerator ChargeAndFire(Vector3 target)
    {
        isCharging = true;
        chargeStartTime = Time.time;
        
        // Play charge sound
        if (audioSource != null && chargeSound != null)
        {
            audioSource.PlayOneShot(chargeSound);
        }
        
        // Show charge effect
        if (chargeEffect != null)
        {
            chargeEffect.SetActive(true);
        }
        
        // Wait for charge time
        yield return new WaitForSeconds(chargeTime);
        
        // Hide charge effect
        if (chargeEffect != null)
        {
            chargeEffect.SetActive(false);
        }
        
        // Fire the railgun
        FireRailgun(target);
        
        isCharging = false;
    }

    private void FireRailgun(Vector3 targetPosition)
    {
        lastFireTime = Time.time;
        currentAmmo--;
        
        // Play fire sound
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        // Calculate direction
        Vector3 fireDirection = (targetPosition - firePoint.position).normalized;
        
        // Perform raycast with penetration
        List<RaycastHit> allHits = new List<RaycastHit>();
        Vector3 currentOrigin = firePoint.position;
        float remainingRange = maxRange;
        float currentDamage = baseDamage * damageModifier;
        int penetrationCount = 0;
        
        // Keep casting rays until we run out of range, penetrations, or hit nothing
        while (remainingRange > 0 && penetrationCount <= maxPenetrations)
        {
            RaycastHit hit;
            
            if (Physics.Raycast(currentOrigin, fireDirection, out hit, remainingRange))
            {
                allHits.Add(hit);
                
                // Apply damage
                ApplyDamage(hit, currentDamage);
                
                // Spawn impact effect
                SpawnImpactEffect(hit.point, hit.normal);
                
                // Reduce damage for next penetration
                currentDamage *= (1f - damageDropoffPerPenetration);
                
                // Check if this object stops the beam
                if (ShouldStopBeam(hit))
                {
                    break;
                }
                
                // Move origin past this object for next raycast
                currentOrigin = hit.point + fireDirection * 0.1f;
                remainingRange -= hit.distance + 0.1f;
                penetrationCount++;
            }
            else
            {
                // No more hits, beam continues to max range
                break;
            }
        }
        
        // Determine beam end point
        Vector3 beamEnd;
        if (allHits.Count > 0)
        {
            beamEnd = allHits[allHits.Count - 1].point;
        }
        else
        {
            beamEnd = firePoint.position + fireDirection * maxRange;
        }
        
        // Draw the beam
        StartCoroutine(DrawBeam(firePoint.position, beamEnd));
        
        Debug.Log($"Railgun fired! Penetrated {penetrationCount} objects, dealt damage to {allHits.Count} targets");
    }

    private void ApplyDamage(RaycastHit hit, float damage)
    {
        // Check for various damageable components
        
        // Enemy ships
        EnemyShip enemyShip = hit.collider.GetComponent<EnemyShip>();
        if (enemyShip != null)
        {
            enemyShip.TakeDamage();
            Debug.Log($"Railgun hit enemy for {damage} damage");
            return;
        }
        
        // Player ship (friendly fire or enemy railgun)
        PlayerShip playerShip = hit.collider.GetComponent<PlayerShip>();
        if (playerShip != null)
        {
            // Check if it's friendly fire
            if (!CompareTag(hit.collider.tag))
            {
                playerShip.TakeDamage(damage);
                Debug.Log($"Railgun hit player for {damage} damage");
            }
            return;
        }
        
        // Shields
        Shields shields = hit.collider.GetComponent<Shields>();
        if (shields != null)
        {
            shields.TakeDamage(damage);
            Debug.Log($"Railgun hit shields for {damage} damage");
            return;
        }
        
        // Generic damageable interface (if you implement one)
        IDamageable damageable = hit.collider.GetComponent<IDamageable>();
        if (damageable != null && damageable.CanBeDamaged())
        {
            damageable.TakeDamage(damage);
            return;
        }
        
        // Destructible obstacles (asteroids, debris, etc.)
        // You can add specific component checks here
        if (hit.collider.CompareTag("Asteroid") || hit.collider.CompareTag("Debris"))
        {
            // Destroy or damage asteroid/debris
            Destroy(hit.collider.gameObject);
            Debug.Log($"Railgun destroyed {hit.collider.gameObject.name}");
        }
    }

    private bool ShouldStopBeam(RaycastHit hit)
    {
        // Define what stops the railgun beam
        // Most things don't stop it, but you might want certain objects to
        
        // Heavy armor or specific materials could stop it
        if (hit.collider.CompareTag("HeavyArmor") || hit.collider.CompareTag("Station"))
        {
            return true;
        }
        
        // By default, beam continues through most objects
        return false;
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
        
        // Optional: Animate beam width
        float elapsed = 0f;
        float originalWidth = beamWidth;
        
        while (elapsed < beamDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / beamDuration;
            
            // Fade out the beam
            float alpha = 1f - t;
            Color startColor = beamColor;
            startColor.a = alpha;
            Color endColor = startColor;
            endColor.a = alpha * 0.3f;
            
            beamRenderer.startColor = startColor;
            beamRenderer.endColor = endColor;
            
            // Optional: Shrink beam width over time
            beamRenderer.startWidth = originalWidth * (1f - t * 0.5f);
            beamRenderer.endWidth = originalWidth * 0.5f * (1f - t * 0.5f);
            
            yield return null;
        }
        
        beamRenderer.enabled = false;
    }

    /// <summary>
    /// Check if railgun is currently charging
    /// </summary>
    public bool IsCharging()
    {
        return isCharging;
    }

    /// <summary>
    /// Get charge progress (0 to 1)
    /// </summary>
    public float GetChargeProgress()
    {
        if (!isCharging) return 0f;
        return Mathf.Clamp01((Time.time - chargeStartTime) / chargeTime);
    }

    /// <summary>
    /// Cancel charging (useful for interrupted shots)
    /// </summary>
    public void CancelCharge()
    {
        if (isCharging)
        {
            StopAllCoroutines();
            isCharging = false;
            
            if (chargeEffect != null)
            {
                chargeEffect.SetActive(false);
            }
        }
    }

    // Public getters for UI
    public float GetChargeTime() => chargeTime;
    public int GetMaxPenetrations() => maxPenetrations;
    public float GetMaxRange() => maxRange;
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
}
