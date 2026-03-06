using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Signal
{
    public float frequency;
    public int bandType; // 1, 2, or 3
    public string message;
    public bool isEncrypted;
    public float timeLimit = 30f; // Time to intercept in seconds
}

public class CommsManager : MonoBehaviour
{
    [Header("Signal Settings")]
    [SerializeField] private float maxSignalRange = 200000f; // 200k units
    [SerializeField] private float signalLockTolerance = 1.0f; // +/- 1.0 for successful lock
    
    [Header("Audio")]
    [SerializeField] private AudioSource signalToneSource;
    [SerializeField] private AudioClip signalTone;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failureSound;

    private Signal currentSignal;
    private int currentBand = 1;
    private float currentFrequency = 1.0f;
    private bool isIntercepting = false;
    private List<string> interceptedMessages = new List<string>();

    private void Start()
    {
        UIController.Instance?.ShowCommsPanel(false);
    }

    public void ReceiveSignal(Signal signal)
    {
        if (isIntercepting) return;
        
        currentSignal = signal;
        isIntercepting = true;
        UIController.Instance?.ShowCommsPanel(true);
        Time.timeScale = 0f; // Pause the game
        StartCoroutine(SignalInterceptionTimer());
    }

    private void Update()
    {
        if (!isIntercepting) return;

        // Band selection with number keys 1,2,3
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchBand(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchBand(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchBand(3);

        // Frequency tuning with arrow keys
        if (Input.GetKey(KeyCode.LeftArrow))
            AdjustFrequency(-0.1f);
        if (Input.GetKey(KeyCode.RightArrow))
            AdjustFrequency(0.1f);

        UpdateSignalStrength();
    }

    private void SwitchBand(int band)
    {
        currentBand = band;
        UIController.Instance?.SetCommsBand(band);
    }

    private void AdjustFrequency(float delta)
    {
        currentFrequency = Mathf.Clamp(currentFrequency + delta, 1.0f, 99.9f);
        UIController.Instance?.SetCommsFrequency(currentFrequency);
    }

    public void OnFrequencyChanged(float value)
    {
        currentFrequency = value;
        UIController.Instance?.SetCommsFrequency(currentFrequency);
        UpdateSignalStrength();
    }

    private void UpdateSignalStrength()
    {
        if (currentSignal == null) return;

        if (currentBand != currentSignal.bandType)
        {
            UIController.Instance?.SetCommsSignalStrength(Color.grey);
            if (signalToneSource != null && signalToneSource.isPlaying) signalToneSource.Stop();
            return;
        }

        float distance = Mathf.Abs(currentFrequency - currentSignal.frequency);
        float strength = 1f - (distance / 10f);
        strength = Mathf.Clamp01(strength);

        UIController.Instance?.SetCommsSignalStrength(Color.Lerp(Color.red, Color.green, strength));
        
        if (signalToneSource != null)
        {
            signalToneSource.pitch = 0.5f + strength;
            if (!signalToneSource.isPlaying)
                signalToneSource.Play();
        }
    }

    private void TryLockSignal()
    {
        if (currentSignal == null) return;

        float distance = Mathf.Abs(currentFrequency - currentSignal.frequency);
        bool correctBand = currentBand == currentSignal.bandType;
        bool frequencyMatch = distance <= signalLockTolerance;

        if (correctBand && frequencyMatch)
        {
            AudioSource.PlayClipAtPoint(successSound, Camera.main.transform.position);
            interceptedMessages.Add(currentSignal.message);
            UIController.Instance?.AddCommsLog(currentSignal.message);
            CloseInterceptPanel(true);
        }
        else
        {
            AudioSource.PlayClipAtPoint(failureSound, Camera.main.transform.position);
            CloseInterceptPanel(false);
        }
    }

    private void CloseInterceptPanel(bool success)
    {
        isIntercepting = false;
        currentSignal = null;
        UIController.Instance?.ShowCommsPanel(false);
        Time.timeScale = 1f;
        
        if (signalToneSource != null && signalToneSource.isPlaying)
            signalToneSource.Stop();
    }

    private IEnumerator SignalInterceptionTimer()
    {
        float timeLeft = currentSignal.timeLimit;
        
        while (timeLeft > 0 && isIntercepting)
        {
            timeLeft -= Time.unscaledDeltaTime; // Use unscaledDeltaTime because game is paused
            // Update timer UI here if you want to show it
            yield return null;
        }

        if (isIntercepting)
        {
            CloseInterceptPanel(false);
        }
    }

    // Call this method to generate test signals
    public void GenerateTestSignal()
    {
        Signal testSignal = new Signal
        {
            frequency = Random.Range(1.0f, 99.9f),
            bandType = Random.Range(1, 4),
            message = "==|RAJA|==\n8 Hrs to Delta Point\nSpeed 90\nInitiating SUPERCRUISE",
            isEncrypted = Random.value > 0.7f,
            timeLimit = 30f
        };
        
        ReceiveSignal(testSignal);
    }

    public void FrequancyTune(float speed)
    {
        UIController.Instance?.FrequancyTune(speed);
    }
}
