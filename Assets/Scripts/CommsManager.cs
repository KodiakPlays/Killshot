using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    
    [Header("UI References")]
    [SerializeField] private GameObject signalInterceptPanel;
    [SerializeField] private Slider frequencySlider;
    [SerializeField] private Image signalStrengthIndicator;
    [SerializeField] private TextMeshProUGUI bandDisplay;
    [SerializeField] private TextMeshProUGUI frequencyDisplay;
    [SerializeField] private Button lockButton;
    [SerializeField] private Image[] bandIndicators; // 3 indicator lights for bands
    [SerializeField] private AudioSource signalToneSource;
    [SerializeField] private TextMeshProUGUI commsLog;
    
    [Header("Audio")]
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
        signalInterceptPanel.SetActive(false);
        SetupUI();
    }

    private void SetupUI()
    {
        frequencySlider.minValue = 1.0f;
        frequencySlider.maxValue = 99.9f;
        frequencySlider.onValueChanged.AddListener(OnFrequencyChanged);
        lockButton.onClick.AddListener(TryLockSignal);
    }

    public void ReceiveSignal(Signal signal)
    {
        if (isIntercepting) return;
        
        currentSignal = signal;
        isIntercepting = true;
        signalInterceptPanel.SetActive(true);
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
        bandDisplay.text = $"Band: {band}";
        
        // Update band indicators
        for (int i = 0; i < bandIndicators.Length; i++)
        {
            bandIndicators[i].color = (i + 1 == band) ? Color.green : Color.gray;
        }
    }

    private void AdjustFrequency(float delta)
    {
        currentFrequency = Mathf.Clamp(currentFrequency + delta, 1.0f, 99.9f);
        frequencySlider.value = currentFrequency;
        frequencyDisplay.text = $"Frequency: {currentFrequency:F1}";
    }

    private void OnFrequencyChanged(float value)
    {
        currentFrequency = value;
        frequencyDisplay.text = $"Frequency: {currentFrequency:F1}";
        UpdateSignalStrength();
    }

    private void UpdateSignalStrength()
    {
        if (currentSignal == null) return;

        if (currentBand != currentSignal.bandType)
        {
            signalStrengthIndicator.color = Color.grey;
            if (signalToneSource.isPlaying) signalToneSource.Stop();
            return;
        }

        float distance = Mathf.Abs(currentFrequency - currentSignal.frequency);
        float strength = 1f - (distance / 10f); // Strength decreases as distance increases
        strength = Mathf.Clamp01(strength);

        signalStrengthIndicator.color = Color.Lerp(Color.red, Color.green, strength);
        
        // Update audio feedback
        signalToneSource.pitch = 0.5f + strength;
        if (!signalToneSource.isPlaying)
            signalToneSource.Play();
    }

    private void TryLockSignal()
    {
        if (currentSignal == null) return;

        float distance = Mathf.Abs(currentFrequency - currentSignal.frequency);
        bool correctBand = currentBand == currentSignal.bandType;
        bool frequencyMatch = distance <= signalLockTolerance;

        if (correctBand && frequencyMatch)
        {
            // Success!
            AudioSource.PlayClipAtPoint(successSound, Camera.main.transform.position);
            AddToLog(currentSignal.message);
            CloseInterceptPanel(true);
        }
        else
        {
            // Failure
            AudioSource.PlayClipAtPoint(failureSound, Camera.main.transform.position);
            CloseInterceptPanel(false);
        }
    }

    private void AddToLog(string message)
    {
        interceptedMessages.Add(message);
        commsLog.text = string.Join("\n", interceptedMessages);
    }

    private void CloseInterceptPanel(bool success)
    {
        isIntercepting = false;
        currentSignal = null;
        signalInterceptPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
        
        if (signalToneSource.isPlaying)
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
}
