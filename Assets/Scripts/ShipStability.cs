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

    [Header("Turn Severity Thresholds")]
    [SerializeField] private float greenZoneThreshold = 60f; // Degrees
    [SerializeField] private float yellowZoneThreshold = 90f; // Degrees

    private bool canDodge = true;
    private float lastDodgeTime;
    private const float DODGE_COOLDOWN = 0.5f; // Time between possible dodges

    private void Start()
    {
        currentStability = maxStability;
    }

    private void Update()
    {
        // Recover stability over time when above 10%
        if (currentStability > maxStability * 0.1f)
        {
            currentStability = Mathf.Min(maxStability, currentStability + stabilityRecoveryRate * Time.deltaTime);
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
            
            // Reset dodge ability after cooldown
            Invoke(nameof(ResetDodge), DODGE_COOLDOWN);
        }
    }

    private void ResetDodge()
    {
        canDodge = true;
    }

    public float CalculateTurnStabilityDrain(float turnAngle, float currentSpeed)
    {
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / 100f); // Assuming 100 is max speed
        float angleSeverity;

        if (Mathf.Abs(turnAngle) <= greenZoneThreshold)
        {
            angleSeverity = 0.5f;
        }
        else if (Mathf.Abs(turnAngle) <= yellowZoneThreshold)
        {
            angleSeverity = 1f;
        }
        else
        {
            angleSeverity = 2f;
        }

        float drain = turnStabilityDrainMultiplier * angleSeverity * normalizedSpeed;
        currentStability = Mathf.Max(0, currentStability - drain);
        
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
}
