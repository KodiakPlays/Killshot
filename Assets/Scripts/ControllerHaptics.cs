// =================================================================================================
// ControllerHaptics.cs
//
// What it does:
// Makes a game controller rumble when a gun fires, the ship is hit or the power is switched. With
// no controller plugged in, it does nothing. It is in every scene, the battle board included.
// Only the older physics ship and gun scripts ask it for a buzz.
//
// How it works:
// - Before the first scene opens, it makes one object named [ControllerHaptics]. That object
//   stays through scene changes. A second copy removes itself.
// - Each named event has a set buzz of three numbers. They are the heavy motor's strength, the
//   light motor's strength, and how long it lasts. The time is 0.04 to 0.3 seconds.
// - The gun events cover the laser, PDC, cannon, macrocannon, torpedo, broadside and boarding pod.
// - The ship events cover a dodge, hyperspeed on, off and lost, taking damage, a power switch and
//   a gun change.
// - A buzz turns the motors on, waits for its time, then turns them off.
// - Other scripts can turn the motors on and leave them on, or stop them all.
// - When the object is removed, it stops the motors.
// - PlayerShip, WeaponManager, BroadsideCannon and LaserWeapon ask it for buzzes.
//
// Technical:
// - ControllerHaptics.Instance is set in Awake. CreateInstance makes the object before the first
//   scene loads ([RuntimeInitializeOnLoadMethod]).
// - Each named event is a static method (LaserFired, TookDamage, PowerToggled...). Each calls
//   Pulse(heavy, light, seconds).
// - PlayerShip calls the ship events, SetContinuous and StopAll. It also calls Pulse with other
//   numbers. WeaponManager, BroadsideCannon and LaserWeapon call the gun events.
// - CannonFired has no caller.
// - Rumble goes through the new Input System: Gamepad.current.SetMotorSpeeds, each motor 0 to 1.
// - Numbers: fixed in code, one line per event. The Tuning window does not show them.
// - Trap: the wait uses scaled time. While the game is paused, a buzz keeps going until it
//   resumes.
// - Trap: buzzes do not add up. The newest one sets the motors. The first to end turns them off.
// - Trap: SetContinuous leaves the motors on until StopAll runs or any buzz ends.
// =================================================================================================

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
