using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChargeLaser : MonoBehaviour
{
    public Slider chargeSlider;  // Reference to the UI Slider
    public Button chargeButton;  // Reference to the Charge Button
    public float chargeTime = 9f;  // Time it takes to fully charge

    public bool isCharging = false;  // To check if currently charging
    public bool isCharged = false;  // To check if fully charged
    private Coroutine chargeCoroutine;

    void Start()
    {
        chargeSlider.value = 0;
    }

    private void Update()
    {
        if (!GameManager.Instance.lActive)
        {
            if (isCharging)
            {
                StopCoroutine(chargeCoroutine);
                isCharging = false;
            }
        }

       
        if(chargeSlider.value == 1)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                //Optionally reset charge on Return / Enter key press and also when it finished whole charge
                Invoke("ResetValue", 0.3f);
            }
        }
    }
    void ResetValue()
    {
        chargeSlider.value = 0;
        isCharged = false;
    }
    public void OnChargeButtonClick()
    {
       // AudioManager.Instance.PlayLaserLoading();
        if (GameManager.Instance.lActive )//&& !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (isCharging)
            {
                StopCoroutine(chargeCoroutine);
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
        float elapsedTime = chargeSlider.value * chargeTime;

        while (elapsedTime < chargeTime)
        {
            chargeSlider.value = elapsedTime / chargeTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        chargeSlider.value = 1;
        isCharging = false;
        isCharged = true;
       // Debug.Log("is charged: " + isCharged);
        //Debug.Log("is isCharging: " + isCharging);
    }
}
