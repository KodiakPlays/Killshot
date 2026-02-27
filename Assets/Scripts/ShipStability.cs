using UnityEngine;
using UnityEngine.UI;

public class ShipStability : MonoBehaviour
{
    [Header("Stability Settings")]
    [SerializeField] private float maxStability = 100f;
    [SerializeField] private float currentStability;
    [SerializeField] private float stabilityRecoveryRate = 5f; // Per second
    [SerializeField] private float turnStabilityDrainMultiplier = 1f;
    [SerializeField] private float speedStabilityDrainMultiplier = 0.5f;
    [SerializeField] private float dodgeStabilityCost = 45f;

    [Header("Turn Severity Thresholds (degrees per frame)")]
    [SerializeField] private float greenZoneThreshold = 1.0f; // Small heading changes within 60° total
    [SerializeField] private float yellowZoneThreshold = 1.5f; // Moderate turns (~45° arcs)
    [SerializeField] private float redZoneThreshold = 3.0f; // Large/fast turns (~90°+ or more)

    private bool canDodge = true;
    private float lastDodgeTime;
    private const float DODGE_COOLDOWN = 0.5f; // Time between possible dodges

    [Header("Stability UI")]
    [SerializeField] private Shader stabilityShader;
    [SerializeField] private Image stabilityImg;
    [SerializeField] private Image stabilityDisplayImg; // Assign same Image as PlayerShip's velocityMeterImg[0]
    private float stabMax = 1;

    private void Start()
    {
        currentStability = maxStability;

        StabilityMeterStart(currentStability, maxStability);
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

            StabilityMeterUpdate(currentStability);
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

    public float CalculateTurnStabilityDrain(float turnAngleThisFrame, float currentSpeed)
    {
        // Normalize speed (assuming max operational speed around 100 units)
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / 100f);
        
        // Determine turn severity zone based on degrees per frame
        float drainMultiplier;
        string zone;
        
        if (turnAngleThisFrame <= greenZoneThreshold)
        {
            // Green Zone: Small heading changes within 60°. Low risk.
            drainMultiplier = 0.1f;
            zone = "Green";
        }
        else if (turnAngleThisFrame <= yellowZoneThreshold)
        {
            // Yellow Zone: Moderate turns (~45° arcs). Higher stability drain.
            drainMultiplier = 0.5f;
            zone = "Yellow";
        }
        else
        {
            // Red Zone: Large or fast turns (~90° or more). Extreme stability drain.
            drainMultiplier = 2.0f;
            zone = "Red";
        }
        
        // Sharp turns at high speed drain much more stability
        float speedMultiplier = 1f + (normalizedSpeed * speedStabilityDrainMultiplier);
        
        // Calculate final drain
        float drain = turnStabilityDrainMultiplier * drainMultiplier * speedMultiplier * Time.fixedDeltaTime;
        
        // Apply the drain
        currentStability = Mathf.Max(0, currentStability - drain);
        
        // Debug info for testing
        if (drain > 0.01f)
        {
            Debug.Log($"Turn Zone: {zone}, Angle: {turnAngleThisFrame:F2}°, Speed: {currentSpeed:F1}, Drain: {drain:F3}, Stability: {currentStability:F1}%");
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

    public void StabilityMeterStart(float cur, float max)
    {
        if (stabilityImg == null) return;
        stabilityImg.material = new Material(stabilityShader);
        stabilityImg.material.SetColor("_OnColor", new Color(1f, 1f, 1f));

        stabilityImg.material.SetFloat("_PowerCur", cur);
        stabilityImg.material.SetFloat("_PowerMax", max);

        stabMax = max;
    }

    public void StabilityMeterUpdate(float cur)
    {
        if (stabilityDisplayImg == null) return;
        stabilityDisplayImg.material.SetFloat("_PowerCur", cur);

        StabilityMeterColor(cur);
    }

    public void StabilityMeterColor(float cur)
    {
        if (stabilityDisplayImg == null) return;
        Color[] col = { new Color(1f, 1f, 1f), new Color(0.8117647f, 0.6f, 0f), new Color(1f, 0f, 0f) };

        if (cur >= stabMax * .25)
        {
            stabilityDisplayImg.material.SetColor("_OnColor", col[0]);
        }
        else if (cur < stabMax * .25 && cur >= stabMax * .1)
        {
            stabilityDisplayImg.material.SetColor("_OnColor", col[1]);
        }
        else if (cur < stabMax * .1)
        {
            stabilityDisplayImg.material.SetColor("_OnColor", col[2]);
        }
    }

    /// <summary>
    /// Called by UIController to assign UI references that live on the Canvas prefab.
    /// </summary>
    public void AssignUIReferences(
        Shader stabilityShaderRef,
        Image stabilityImgRef)
    {
        stabilityShader = stabilityShaderRef;
        stabilityImg = stabilityImgRef;
    }
}
