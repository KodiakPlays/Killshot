using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Singleton MonoBehaviour that wraps Unity Input System gamepad rumble.
/// Static convenience methods make haptics a one-liner from anywhere.
/// Auto-creates itself on first use and persists across scenes.
/// All methods are no-ops when no gamepad is connected.
/// </summary>
public class ControllerHaptics : MonoBehaviour
{
    public static ControllerHaptics Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Gamepad.current?.SetMotorSpeeds(0f, 0f);
    }

    // ----------------------------------------------------------------
    // Static convenience wrappers — weapon fire
    // ----------------------------------------------------------------
    public static void LaserFired()        => Instance?.Pulse(0.00f, 0.25f, 0.06f);
    public static void PDCFired()          => Instance?.Pulse(0.05f, 0.38f, 0.04f);
    public static void CannonFired()       => Instance?.Pulse(0.55f, 0.25f, 0.18f);
    public static void MacrocannonFired()  => Instance?.Pulse(0.75f, 0.30f, 0.22f);
    public static void MissileFired()      => Instance?.Pulse(0.45f, 0.15f, 0.16f);
    public static void BroadsideFired()    => Instance?.Pulse(0.45f, 0.20f, 0.14f);
    public static void BoardingPodFired()  => Instance?.Pulse(0.30f, 0.10f, 0.20f);

    // ----------------------------------------------------------------
    // Static convenience wrappers — ship events
    // ----------------------------------------------------------------
    public static void DodgeExecuted()     => Instance?.Pulse(0.60f, 0.80f, 0.10f);
    public static void HyperspeedOn()      => Instance?.Pulse(0.35f, 0.55f, 0.30f);
    public static void HyperspeedOff()     => Instance?.Pulse(0.20f, 0.10f, 0.10f);
    public static void HyperspeedLost()    => Instance?.Pulse(0.70f, 0.40f, 0.20f);
    public static void TookDamage()        => Instance?.Pulse(0.85f, 0.60f, 0.15f);
    public static void PowerToggled()      => Instance?.Pulse(0.00f, 0.20f, 0.05f);
    public static void WeaponSwitched()    => Instance?.Pulse(0.00f, 0.15f, 0.05f);

    // ----------------------------------------------------------------
    // Continuous motor control (caller is responsible for stopping)
    // ----------------------------------------------------------------
    public static void SetContinuous(float lowFreq, float highFreq)
        => Gamepad.current?.SetMotorSpeeds(lowFreq, highFreq);

    public static void StopAll()
        => Gamepad.current?.SetMotorSpeeds(0f, 0f);

    // ----------------------------------------------------------------
    // Instance methods
    // ----------------------------------------------------------------

    /// <summary>Fires a rumble pulse. Multiple simultaneous pulses stack on top of each other.</summary>
    public void Pulse(float lowFreq, float highFreq, float duration)
    {
        StartCoroutine(PulseRoutine(lowFreq, highFreq, duration));
    }

    private IEnumerator PulseRoutine(float lowFreq, float highFreq, float duration)
    {
        var gp = Gamepad.current;
        if (gp == null) yield break;

        gp.SetMotorSpeeds(lowFreq, highFreq);
        yield return new WaitForSeconds(duration);
        gp.SetMotorSpeeds(0f, 0f);
    }

    // ----------------------------------------------------------------
    // Ensure an instance exists (called lazily from static methods)
    // ----------------------------------------------------------------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (Instance != null) return;
        var go = new GameObject("[ControllerHaptics]");
        go.AddComponent<ControllerHaptics>();
    }
}
