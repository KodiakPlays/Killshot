using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScanningProcess : MonoBehaviour
{
    [SerializeField]
    GameObject mScannerActive, mScanningProcess, mFinalReadout;
    [SerializeField]
    GameObject blinkObject, normalObject;
    [SerializeField]
     Slider mScanningProcessSlider;
    bool isNumberMatch;
    int randNumber;
    [SerializeField] TextMeshProUGUI detectedEnemyName;
     GameObject detectedEnemy;
    // Start is called before the first frame update
    void Start()
    {
        isNumberMatch = false;
        mScannerActive.SetActive(true);
        mScanningProcess.SetActive(false);
        mFinalReadout.SetActive(false);
        randNumber = Random.Range(8, 19);
        blinkObject.SetActive(false);
        normalObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
       // blinkObject.SetActive(true);
        if (GameManager.Instance.isEnemyDetect)
        {
            detectedEnemy = GameManager.Instance.detectedEnemy;
            mScannerActive.SetActive(false);
            mScanningProcess.SetActive(true);

            if (Input.GetKeyDown(KeyCode.RightArrow) && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                AudioManager.Instance.OnClick();
                mScanningProcessSlider.value++;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                AudioManager.Instance.OnClick();
                mScanningProcessSlider.value--;
            }
            if(mScanningProcessSlider.value == randNumber)
            {
                StartCoroutine(ToggleObject());
                isNumberMatch = true;
                normalObject.SetActive(false);
            }
            else
            {
                isNumberMatch = false;
                normalObject.SetActive(true);
                blinkObject.SetActive(false) ;
            }
        }
        else
        {
            mScannerActive.SetActive(true);
            mScanningProcess.SetActive(false);
            mFinalReadout.SetActive(false);
        }
    }
    private IEnumerator ToggleObject()
    {
        while (mScanningProcessSlider.value == randNumber)
        {
            blinkObject.SetActive(!blinkObject.activeSelf); // Toggle the active state
            yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds
        }
    }
    public void LockButton()
    {
        AudioManager.Instance.OnClick();
        if (isNumberMatch)
        {
            mScannerActive.SetActive(false);
            mScanningProcess.SetActive(false);
            mFinalReadout.SetActive(true);
            detectedEnemyName.text = "Name : " + detectedEnemy.name;
        }
        else if(!isNumberMatch) 
        {
            mScannerActive.SetActive(false);
            mScanningProcess.SetActive(true);
            mFinalReadout.SetActive(false);
        }
        
    }
    public void LeftButton()
    {
        mScanningProcessSlider.value--;
        AudioManager.Instance.OnClick();
    }
    public void RightButton()
    {
        mScanningProcessSlider.value++;
        AudioManager.Instance.OnClick();
    }
}
