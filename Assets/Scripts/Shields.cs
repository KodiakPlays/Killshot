// =================================================================================================
// Shields.cs
//
// What it does:
// An energy barrier for the older physics player ship. It takes the damage from hits before the
// hull does. It shows as a coloured picture around the ship. It is on the PlayerShip prefab.
// The battle board does not use this script.
//
// How it works:
// - It starts with 100 health and hidden. It uses the picture set in the Inspector, or the first
//   one found on its object or a child.
// - The old Railgun and Laser scripts look for it on what they hit and send it the damage.
// - A hit lowers its health. If more than 30 is left, it shows for 0.5 seconds. The colour goes
//   from cyan at full to red at empty, and fades as it weakens.
// - At 30 health or less it blinks on and off every 0.5 seconds.
// - At 0 it switches off and hides. Damage past 0 is lost. Hits after that go to PlayerShip, on
//   the front of the hull.
// - Once broken, it waits 3 seconds after the last hit and then refills at 10 health per second.
//   It does not refill after a hit that leaves it above 0.
//
// Technical:
// - Data: currentShieldHealth and maxShieldHealth (100), in shield points. isShieldActive turns
//   false at 0 and back on when the refill starts.
// - TakeDamage(damage) is called by Laser (enemy shots) and Railgun. Once empty, it calls
//   PlayerShip.TakeDamage(damage).
// - Laser looks for Shields before PlayerShip. Railgun looks for PlayerShip first.
// - Trap: on the PlayerShip prefab both scripts are on one object. Railgun hits go straight to
//   the hull.
// - Trap: Shields passes no hit direction. With Shields on the ship, every laser hit on the hull
//   goes to the front.
// - Numbers: health, refill rate, refill delay, blink gap, the 30 limit and the colours are in
//   the Inspector. The 0.5-second flash is fixed in code.
// - Timers use Time.time and WaitForSeconds, both scaled time.
// - Trap: it needs a SpriteRenderer. Without one, a flash, blink or refill gives an error.
// =================================================================================================

using UnityEngine;
using System.Collections;

public class Shields : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float maxShieldHealth = 100f;
    [SerializeField] private float currentShieldHealth;
    [SerializeField] private float rechargeRate = 10f;
    [SerializeField] private float rechargeDelay = 3f;
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer shieldRenderer;
    [SerializeField] private Color fullShieldColor = Color.cyan;
    [SerializeField] private Color deplatedShieldColor = Color.red;
    [SerializeField] private float blinkRate = 0.5f;
    [SerializeField] private float criticalShieldThreshold = 30f;

    private PlayerShip playerShip;
    private bool isShieldActive = true;
    private bool isRecharging = false;
    private float lastDamageTime;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        playerShip = GetComponent<PlayerShip>();
        if (shieldRenderer == null)
        {
            shieldRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        currentShieldHealth = maxShieldHealth;
        if (shieldRenderer != null)
        {
            shieldRenderer.enabled = false;  // Shield starts invisible
        }
    }

    private void Start()
    {
        // Remove initial visual update since shield starts invisible
    }

    private void Update()
    {
        if (!isShieldActive && Time.time - lastDamageTime >= rechargeDelay && currentShieldHealth < maxShieldHealth)
        {
            StartRecharge();
        }

        if (isRecharging)
        {
            RechargeShield();
        }
    }

    private Coroutine flashCoroutine;
    private const float FLASH_DURATION = 0.5f;  // Duration shield stays visible when hit

    public void TakeDamage(float damage)
    {
        lastDamageTime = Time.time;
        isRecharging = false;

        if (currentShieldHealth > 0)
        {
            currentShieldHealth = Mathf.Max(0, currentShieldHealth - damage);
            
            // Show shield when hit
            if (currentShieldHealth > criticalShieldThreshold)
            {
                if (flashCoroutine != null)
                {
                    StopCoroutine(flashCoroutine);
                }
                flashCoroutine = StartCoroutine(FlashShield());
            }
            
            UpdateShieldVisuals();

            if (currentShieldHealth <= 0)
            {
                DeactivateShield();
            }
            else if (currentShieldHealth <= criticalShieldThreshold && blinkCoroutine == null)
            {
                if (flashCoroutine != null)
                {
                    StopCoroutine(flashCoroutine);
                    flashCoroutine = null;
                }
                blinkCoroutine = StartCoroutine(BlinkShield());
            }
        }
        else
        {
            // Shield is depleted, damage goes to the ship
            playerShip.TakeDamage(damage);
        }
    }

    private void UpdateShieldVisuals()
    {
        if (shieldRenderer != null)
        {
            // Update shield color based on health percentage
            float healthPercentage = currentShieldHealth / maxShieldHealth;
            Color currentColor = Color.Lerp(deplatedShieldColor, fullShieldColor, healthPercentage);
            
            // Update alpha based on shield health
            currentColor.a = Mathf.Lerp(0.2f, 0.8f, healthPercentage);
            
            shieldRenderer.color = currentColor;
        }
    }

    private void StartRecharge()
    {
        isRecharging = true;
        isShieldActive = true;
        
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        
        // Hide shield if health is above critical threshold
        if (currentShieldHealth > criticalShieldThreshold)
        {
            shieldRenderer.enabled = false;
        }
        else
        {
            shieldRenderer.enabled = true;
            UpdateShieldVisuals();
        }
    }

    private void RechargeShield()
    {
        currentShieldHealth = Mathf.Min(maxShieldHealth, currentShieldHealth + rechargeRate * Time.deltaTime);
        
        // If shield has recharged above critical threshold, hide it
        if (currentShieldHealth > criticalShieldThreshold && blinkCoroutine == null)
        {
            shieldRenderer.enabled = false;
        }
        else
        {
            UpdateShieldVisuals();
        }
        
        if (currentShieldHealth >= maxShieldHealth)
        {
            isRecharging = false;
        }
    }

    private void DeactivateShield()
    {
        isShieldActive = false;
        if (shieldRenderer != null)
        {
            shieldRenderer.enabled = false;
        }
    }

    private IEnumerator FlashShield()
    {
        shieldRenderer.enabled = true;
        UpdateShieldVisuals();
        yield return new WaitForSeconds(FLASH_DURATION);
        
        // Only hide shield if health is above critical threshold
        if (currentShieldHealth > criticalShieldThreshold && blinkCoroutine == null)
        {
            shieldRenderer.enabled = false;
        }
        flashCoroutine = null;
    }

    private IEnumerator BlinkShield()
    {
        while (currentShieldHealth <= criticalShieldThreshold && currentShieldHealth > 0)
        {
            shieldRenderer.enabled = !shieldRenderer.enabled;
            yield return new WaitForSeconds(blinkRate);
        }
        
        if (currentShieldHealth > criticalShieldThreshold)
        {
            shieldRenderer.enabled = true;
        }
        
        blinkCoroutine = null;
    }
}
