using UnityEngine;

public class ShipStability : MonoBehaviour
{
    [Header("Stability Settings")]
    [SerializeField] private float maxStability = 100f;
    [SerializeField] private float currentStability;
    [SerializeField] private float stabilityRecoveryRate = 5f; // Per second
    [SerializeField] private float turnStabilityDrainMultiplier = 1f;
    [SerializeField] private float speedStabilityDrainMultiplier = 0.5f;
    [SerializeField] private float dodgeStabilityCost = 45f;

    [Header("Turn Severity Thresholds (degrees per frame at 50 Hz)")]
    [SerializeField] private float greenZoneThreshold = 0.6f;  // <=60 deg arc — small heading changes
    [SerializeField] private float yellowZoneThreshold = 1.1f; // ~45 deg arc — moderate turns
    // Red zone is anything above yellowZoneThreshold (~90+ deg arc)

    private bool canDodge = true;
    private float lastDodgeTime;
    private const float DODGE_COOLDOWN = 0.5f; // Time between possible dodges



    private void Start()
    {
        currentStability = maxStability;

        if (UIController.Instance != null)
            UIController.Instance.StabilityMeterStart(currentStability, maxStability);
    }

    private void Update()
    {
        // Recover stability over time, but slower if in critical state
        if (currentStability < maxStability)
        {
            float recoveryRate = stabilityRecoveryRate;
            
            // Slower recovery when in critical state (below 10%)
            if (IsStabilityCritical())
            {
                recoveryRate *= 0.3f; // Much slower recovery when critical
            }
            
            currentStability = Mathf.Min(maxStability, currentStability + recoveryRate * Time.deltaTime);

            if (UIController.Instance != null)
                UIController.Instance.StabilityMeterUpdate(currentStability);
        }
    }

    public float GetStabilityPercentage()
    {
        return currentStability / maxStability;
    }

    public bool CanPerformDodge()
    {
        return canDodge && currentStability >= dodgeStabilityCost;
    }

    public void ApplyDodge()
    {
        if (CanPerformDodge())
        {
            currentStability -= dodgeStabilityCost;
            canDodge = false;
            lastDodgeTime = Time.time;

            if (UIController.Instance != null)
                UIController.Instance.StabilityMeterUpdate(currentStability);

            // Reset dodge ability after cooldown
            Invoke(nameof(ResetDodge), DODGE_COOLDOWN);
        }
    }

    private void ResetDodge()
    {
        canDodge = true;
    }

    // powerDecayMultiplier: pass PowerManager.GetEngineStabilityDecayMultiplier() — spec: -5% per engine PWR-1
    public float CalculateTurnStabilityDrain(float turnAngleThisFrame, float currentSpeed, float powerDecayMultiplier = 1f)
    {
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / 100f);
        
        float drainMultiplier;
        string zone;
        
        if (turnAngleThisFrame <= greenZoneThreshold)
        {
            drainMultiplier = 0.1f;
            zone = "Green";
        }
        else if (turnAngleThisFrame <= yellowZoneThreshold)
        {
            drainMultiplier = 0.5f;
            zone = "Yellow";
        }
        else
        {
            // Red Zone: large or fast turns (~90+ deg). Extreme stability drain.
            drainMultiplier = 2.0f;
            zone = "Red";
        }
        
        float speedMultiplier = 1f + (normalizedSpeed * speedStabilityDrainMultiplier);
        
        // Engine power bonus reduces decay (spec: -5% per PWR-1)
        float drain = turnStabilityDrainMultiplier * drainMultiplier * speedMultiplier * powerDecayMultiplier * Time.fixedDeltaTime;
        
        currentStability = Mathf.Max(0, currentStability - drain);
        
        if (UIController.Instance != null)
            UIController.Instance.StabilityMeterUpdate(currentStability);
        
        if (drain > 0.01f)
            Debug.Log($"Turn Zone: {zone}, Angle: {turnAngleThisFrame:F2} deg, Speed: {currentSpeed:F1}, Drain: {drain:F3}, Stability: {currentStability:F1}%");
        
        return drain;
    }

    public bool IsStabilityDepleted()
    {
        return currentStability <= 0;
    }

    public bool IsStabilityCritical()
    {
        return currentStability <= maxStability * 0.1f;
    }

    public float GetCurrentStability()
    {
        return currentStability;
    }

    public float GetMaxStability()
    {
        return maxStability;
    }

    public bool CanDodgeAgain()
    {
        return canDodge;
    }

    public void ApplyStabilityDrain(float amount)
    {
        currentStability = Mathf.Max(0, currentStability - amount);

        if (UIController.Instance != null)
            UIController.Instance.StabilityMeterUpdate(currentStability);
    }

}
