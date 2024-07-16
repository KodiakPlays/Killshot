using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class ChargeLaser : MonoBehaviour
{
    public Slider chargeSlider;  // Reference to the UI Slider
    public Button chargeButton;  // Reference to the Charge Button
    public float chargeTime = 9f;  // Time it takes to fully charge

    public bool isCharging = false;  // To check if currently charging
    public bool isCharged = false;  // To check if currently charging
    private Coroutine chargeCoroutine;

    void Start()
    {
       // chargeButton.onClick.AddListener(OnChargeButtonClick);
        chargeSlider.value = 0;
    }
    private void Update()
    {
        //isCharged = false;
        if (!GameManager.Instance.lActive)
        {
            chargeSlider.value = 0;
            isCharged = false;
            StopCoroutine(chargeCoroutine);
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
           // chargeSlider.value = 0;
            
        }
    }
    public void OnChargeButtonClick()
    {
        AudioManager.Instance.PlayLaserLoading();
        if (GameManager.Instance.lActive &&!(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (isCharging)
            {
                StopCoroutine(chargeCoroutine);
                chargeSlider.value = 0;
                isCharged = false;
            }

            chargeCoroutine = StartCoroutine(ChargeRoutine());
        }
        
        else
        {
            Debug.Log("lActive GameObject is not active. Charging cannot start.");
        }
    }

    IEnumerator ChargeRoutine()
    {
        isCharging = true;
        float elapsedTime = 0f;

        while (elapsedTime < chargeTime)
        {
            chargeSlider.value = elapsedTime / chargeTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        chargeSlider.value = 1;
        isCharging = false;
        isCharged = true;
    }
}
