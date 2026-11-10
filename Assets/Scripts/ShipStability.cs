// =================================================================================================
// ShipStability.cs
//
// What it does:
// A stability meter for the older physics player ship. Hard turns and dodges use it up, and it
// refills over time. It is on the PlayerShip prefab. The battle board does not use this script.
//
// How it works:
// - It starts full at 100 and shows the meter on the older UIController screen.
// - Every frame it refills at 5 per second. At 10 or less it refills at 1.5 per second.
//   When it is full again, it turns the alert light off.
// - When PlayerShip turns, it asks for a drain each physics step. Gentle turns drain a little,
//   medium turns more, and sharp turns the most. Faster speed adds up to half as much again.
//   The engine power bonus from PowerManager lowers the drain.
// - A drain above 0.01 in one step turns the alert light on. It writes the turn details to the
//   Console.
// - A dodge costs 45. It needs at least 45 left, and there is a 0.5-second wait between dodges.
// - Other scripts can ask if it is empty, if it is low (10 or less), and its value.
//   PlayerShip stops turning and damages the hull when it is empty.
// - Every change updates the meter on the UIController screen.
//
// Technical:
// - Data: currentStability and maxStability (100), in stability points.
// - PlayerShip calls CalculateTurnStabilityDrain(degrees this step, speed, power factor) each
//   physics step it turns. The power factor is PowerManager.GetEngineStabilityDecayMultiplier.
// - PlayerShip also calls CanPerformDodge, ApplyDodge, IsStabilityDepleted and CanDodgeAgain.
//   PlayerShip has [RequireComponent(typeof(ShipStability))].
// - Turn zones: by default up to 0.6 degrees per step is gentle, up to 1.1 is medium, above is
//   sharp. Their factors are 0.1, 0.5 and 2.
// - It calls UIController.Instance: StabilityMeterStart, StabilityMeterUpdate and SetAlertActive.
// - Numbers: in the Inspector. The low mark (10%), the slow refill factor (0.3) and the dodge
//   wait (0.5 s) are fixed in code.
// - Trap: with the code's numbers the biggest turn drain is 3 per second. The refill is 5 per
//   second, or 1.5 at 10 or less. Turning alone only empties a meter already at 10 or less.
// - Trap: the PlayerShip prefab sets the zone limits to 60 and 90. Its ship turns 0.4 degrees a
//   step at most. Every turn is gentle, at 0.15 per second at most.
// - Trap: the zone limits are per physics step (0.02 s). A new physics step length moves them.
// - ApplyStabilityDrain(amount) has no caller. Railgun finds this script but never calls it.
// =================================================================================================

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



    private bool alertActive = false;

    private void Start()
    {
        currentStability = maxStability;

        if (UIController.Instance != null)
        {
            UIController.Instance.StabilityMeterStart(currentStability, maxStability);
            UIController.Instance.SetAlertActive(false);
        }
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

            // Hide alert when stability fully restores
            if (alertActive && currentStability >= maxStability)
            {
                alertActive = false;
                UIController.Instance?.SetAlertActive(false);
            }
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

            ShowAlert();

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
        {
            ShowAlert();
            Debug.Log($"Turn Zone: {zone}, Angle: {turnAngleThisFrame:F2} deg, Speed: {currentSpeed:F1}, Drain: {drain:F3}, Stability: {currentStability:F1}%");
        }
        
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

        ShowAlert();
    }

    private void ShowAlert()
    {
        if (!alertActive)
        {
            alertActive = true;
            UIController.Instance?.SetAlertActive(true);
        }
    }
}
