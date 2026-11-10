// =================================================================================================
// Autopilot.cs
//
// What it does:
// An autopilot for the older physics ship (PlayerShip). It flies the ship to a point, or keeps it
// side-on to a target at a set distance. No scene, prefab or other script turns it on. The battle
// board does not use this script.
//
// How it works:
// - It goes on the same object as PlayerShip, PowerManager and InternalSubsystems, and finds them
//   at the start.
// - Other code starts it in one of two ways: fly to a point, or fight a target at a range with
//   the left or right side facing it. The ranges on offer are 0, 5,000, 10,000, 18,000 and 25,000.
// - Every physics step, any of W, A, S, D or a left click switches it off at once.
// - Flying to a point: it turns the ship toward the point and pushes it forward. It stops the
//   autopilot within 1 unit of the point. It does not brake. It does not steer round obstacles.
// - Fighting: it turns the ship side-on to the target. When the ship is over 500 units too far,
//   it may push forward. It does so only if its nose points partly toward the target. If it is
//   too close, it does not push.
// - Turning speed is the ship's turn rate times two factors: the engine power from PowerManager
//   and the damage factor from InternalSubsystems.
// - Pushing forward sets the ship's speed straight to 100 units per second. This is multiplied by
//   the engine power and by the engine damage factor from InternalSubsystems.
// - Each start, stop and range change writes a line to the Console.
//
// Technical:
// - Data: navigationTarget (a world point), combatTarget, desiredRange in units, and orientation
//   (AutopilotOrientation: Port or Starboard).
// - Methods: EngageNavigation, EngageCombat, SetRange(index), SetOrientation and Disengage. No
//   script calls them.
// - It runs in FixedUpdate. It reads keys through the old Input Manager (Input.GetKey).
// - It calls PowerManager.GetSystemEfficiency("engines"). It calls InternalSubsystems for
//   GetTurnRateMultiplier and GetSpeedMultiplier. It reads PlayerShip.turnRate (degrees per
//   second).
// - It moves the Rigidbody: MoveRotation to turn, and linearVelocity set outright to push. The
//   ship turns on the XY plane, nose along transform.up.
// - Numbers: the range list (engagementRanges) is in the Inspector. The top speed of 100, the
//   500-unit band and the 1-unit arrival distance are fixed in code.
// - Trap: the sides are swapped. Port puts the target on the ship's right. Starboard puts it on
//   the left.
// - Trap: a push sets the ship's speed outright each step. A faster ship drops to that speed.
// - Trap: once side-on, the nose points across the line to the target. A push then does not
//   close the range.
// - Trap: the left click is read in FixedUpdate. A click on a frame with no physics step is missed.
// =================================================================================================

using UnityEngine;

public enum AutopilotOrientation
{
    Port,       // Face target with port (left) side
    Starboard   // Face target with starboard (right) side
}

public class Autopilot : MonoBehaviour
{
    [Header("Autopilot Settings")]
    [SerializeField] private float[] engagementRanges = { 0f, 5000f, 10000f, 18000f, 25000f };

    // State
    private bool isActive = false;
    private bool isCombatMode = false;

    // Navigation target
    private Vector3 navigationTarget;
    private bool hasNavigationTarget = false;

    // Combat parameters
    private Transform combatTarget;
    private float desiredRange = 10000f;
    private AutopilotOrientation orientation = AutopilotOrientation.Port;

    // References
    private PlayerShip playerShip;
    private Rigidbody rb;
    private PowerManager powerManager;
    private InternalSubsystems subsystems;

    private void Awake()
    {
        playerShip = GetComponent<PlayerShip>();
        rb = GetComponent<Rigidbody>();
        powerManager = GetComponent<PowerManager>();
        subsystems = GetComponent<InternalSubsystems>();
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        // Check for manual input - disengage immediately per GDD
        if (HasManualInput())
        {
            Disengage();
            return;
        }

        if (isCombatMode && combatTarget != null)
        {
            UpdateCombatAutopilot();
        }
        else if (hasNavigationTarget)
        {
            UpdateNavigationAutopilot();
        }
    }

    /// <summary>
    /// Activate navigation autopilot toward a world position.
    /// </summary>
    public void EngageNavigation(Vector3 target)
    {
        navigationTarget = target;
        hasNavigationTarget = true;
        isCombatMode = false;
        isActive = true;
        Debug.Log($"[Autopilot] Navigation engaged to {target}");
    }

    /// <summary>
    /// Activate combat autopilot per GDD: set target, range, and orientation.
    /// </summary>
    public void EngageCombat(Transform target, float range, AutopilotOrientation orient)
    {
        combatTarget = target;
        desiredRange = range;
        orientation = orient;
        isCombatMode = true;
        isActive = true;
        Debug.Log($"[Autopilot] Combat engaged - Target: {target.name}, Range: {range}, Orientation: {orient}");
    }

    /// <summary>
    /// Set engagement range from predefined bands (0, 5000, 10000, 18000, 25000).
    /// </summary>
    public void SetRange(int rangeIndex)
    {
        if (rangeIndex >= 0 && rangeIndex < engagementRanges.Length)
        {
            desiredRange = engagementRanges[rangeIndex];
            Debug.Log($"[Autopilot] Range set to {desiredRange}");
        }
    }

    /// <summary>
    /// Set which side faces the target.
    /// </summary>
    public void SetOrientation(AutopilotOrientation orient)
    {
        orientation = orient;
    }

    /// <summary>
    /// Disengage autopilot instantly.
    /// </summary>
    public void Disengage()
    {
        if (isActive)
        {
            isActive = false;
            isCombatMode = false;
            hasNavigationTarget = false;
            combatTarget = null;
            Debug.Log("[Autopilot] Disengaged");
        }
    }

    private void UpdateNavigationAutopilot()
    {
        // Calculate direction to target
        Vector3 dirToTarget = (navigationTarget - transform.position);
        float distToTarget = dirToTarget.magnitude;

        if (distToTarget < 1f)
        {
            // Arrived (but don't decelerate per GDD - just stop autopilot logic)
            Disengage();
            return;
        }

        // Rotate toward target - ship must turn before burning per GDD
        RotateToward(dirToTarget.normalized);

        // Accelerate forward (no deceleration per GDD)
        ApplyThrust();
    }

    private void UpdateCombatAutopilot()
    {
        if (combatTarget == null)
        {
            Disengage();
            return;
        }

        Vector3 dirToTarget = combatTarget.position - transform.position;
        float currentDist = dirToTarget.magnitude;

        // Calculate desired heading based on orientation
        // Port = target should be to our left, Starboard = target should be to our right
        Vector3 desiredForward;

        if (orientation == AutopilotOrientation.Port)
        {
            // Rotate so our right side faces away from target (target on left/port)
            desiredForward = Quaternion.Euler(0, 0, 90) * dirToTarget.normalized;
        }
        else
        {
            // Rotate so our left side faces away from target (target on right/starboard)
            desiredForward = Quaternion.Euler(0, 0, -90) * dirToTarget.normalized;
        }

        RotateToward(desiredForward);

        // Maintain distance - approach if too far, retreat if too close
        float rangeDiff = currentDist - desiredRange;
        float rangeThreshold = 500f; // Dead zone to prevent oscillation

        if (rangeDiff > rangeThreshold)
        {
            // Too far - move toward target
            if (Vector3.Dot(transform.up, dirToTarget.normalized) > 0)
            {
                ApplyThrust();
            }
        }
        else if (rangeDiff < -rangeThreshold)
        {
            // Too close - move away (but don't auto-reverse, just stop thrusting)
            // Per GDD, autopilot doesn't decelerate automatically
        }
    }

    private void RotateToward(Vector3 desiredDirection)
    {
        if (playerShip == null || rb == null) return;

        // Calculate angle to desired heading (2D rotation around Z axis)
        float targetAngle = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = rb.rotation.eulerAngles.z;
        float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

        // Apply turn based on current ship turn rate and engine power
        float enginePower = powerManager != null ? powerManager.GetSystemEfficiency("engines") : 1f;
        float subsysMult = subsystems != null ? subsystems.GetTurnRateMultiplier() : 1f;
        float effectiveTurnRate = playerShip.turnRate * enginePower * subsysMult;
        float maxTurnThisFrame = effectiveTurnRate * Time.fixedDeltaTime;

        float turnAmount = Mathf.Clamp(angleDiff, -maxTurnThisFrame, maxTurnThisFrame);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, 0, turnAmount));
    }

    private void ApplyThrust()
    {
        if (rb == null) return;

        float enginePower = powerManager != null ? powerManager.GetSystemEfficiency("engines") : 1f;
        float subsysMult = subsystems != null ? subsystems.GetSpeedMultiplier() : 1f;
        float maxSpeed = 100f * enginePower * subsysMult;

        // Accelerate in ship's forward direction (no auto-deceleration per GDD)
        rb.linearVelocity = transform.up * maxSpeed;
    }

    private bool HasManualInput()
    {
        // GDD: Disengages instantly if player inputs any movement (WASD or mouse click)
        return Input.GetKey(KeyCode.W) ||
               Input.GetKey(KeyCode.A) ||
               Input.GetKey(KeyCode.S) ||
               Input.GetKey(KeyCode.D) ||
               Input.GetMouseButtonDown(0);
    }

    // Public getters
    public bool IsActive() => isActive;
    public bool IsCombatMode() => isCombatMode;
    public float GetDesiredRange() => desiredRange;
    public AutopilotOrientation GetOrientation() => orientation;
    public float[] GetEngagementRanges() => engagementRanges;
}
