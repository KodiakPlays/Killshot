using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using TMPro;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private List<Button> btnPowerBool = new List<Button>();

    [SerializeField]
    private List<Image> imgPowerMet = new List<Image>();

    private List<UIPowerClass> uiPowerMetClass = new List<UIPowerClass>();

    public Dictionary<int, IEnumerator> powerAnimCoroutine = new Dictionary<int, IEnumerator>();

    [SerializeField]
    private Shader shaPowerMet;
    [SerializeField] private Shader shaReactorMet;
    private int reactorCharges = 0;

    [SerializeField]
    private List<Sprite> uiSprite = new List<Sprite>();

    [SerializeField]
    private List<Image> btnPowerBoolImage = new List<Image>();

    [SerializeField] TextMeshProUGUI frequancyTMP;
    [SerializeField] Transform frequancyTrans;
    private float frequancyFlt = 0f;

    [SerializeField] List<Transform> tunerTrans = new List<Transform>();

    void Start()
    {
        FrequancyTune(360f);

        StartPower();
    }

    private void StartPower()
    {
        for (int i = 0; i < btnPowerBool.Count-1; i++)
        {
            uiPowerMetClass.Add(new UIPowerClass(new Material(shaPowerMet), 6, 1, 0f, false));

            imgPowerMet[i].material = uiPowerMetClass[i].mat;
        }

        uiPowerMetClass.Add(new UIPowerClass(new Material(shaReactorMet), 15, 15, 0f, false));

        imgPowerMet[btnPowerBool.Count-1].material = uiPowerMetClass[btnPowerBool.Count-1].mat;
    }

    public void ChargeBtn(int i)
    {
        if (uiPowerMetClass[i].charge == false)
        {
            ChargeOn(i,1);
        }
        else if (uiPowerMetClass[i].charge == true)
        {
            ChargeOff(i);
        }
    }

    public void ChargeOn(int i, int speed)
    {
        uiPowerMetClass[i].Charge(true);
        //uiPowerMetClass[uiPowerMetClass.Count - 1].Charge(true);

        powerAnimCoroutine[i] = ChargeOnAnim(i, 1);

        StartCoroutine(powerAnimCoroutine[i]);

        btnPowerBoolImage[i].sprite = uiSprite[1];
    }

    public void ChargeOff(int i)
    {
        Debug.Log("off: " + (i));

        uiPowerMetClass[i].Charge(false);

        StopCoroutine(powerAnimCoroutine[i]);

        btnPowerBoolImage[i].sprite = uiSprite[0];

        //int chargeTrue = 0;

        //for (int j = 0; j < uiPowerMetClass.Count - 1; j++)
        //{
        //    if (uiPowerMetClass[j].charge)
        //    {
        //        chargeTrue++;
        //    }
        //}

        //if (chargeTrue > 0)
        //{
        //    uiPowerMetClass[uiPowerMetClass.Count - 1].Charge(true);

        //}else if (chargeTrue <= 0)
        //{
        //    uiPowerMetClass[uiPowerMetClass.Count - 1].Charge(false);
        //}

    }

    private void BoolBtn(int i)
    {

    }

    public IEnumerator ChargeOnAnim(int i, int speed)
    {
        Debug.Log("i: " + i);
        float time = 0;

        float crgAmt = 5f;

        while (time < crgAmt)
        {

            Mathf.MoveTowards(0, crgAmt, time);

            time += (Time.deltaTime) * (speed);

            yield return null;

        }

        if (uiPowerMetClass[i].cur >= uiPowerMetClass[i].max || uiPowerMetClass[btnPowerBool.Count - 1].cur <= 0)
        {
            ChargeOff(i);
        }
        else if (uiPowerMetClass[i].cur < uiPowerMetClass[i].max && uiPowerMetClass[btnPowerBool.Count - 1].cur > 0)
        {
            uiPowerMetClass[i].cur++;
            uiPowerMetClass[uiPowerMetClass.Count - 1].cur--;

            uiPowerMetClass[i].mat.SetFloat("_PowerCur", uiPowerMetClass[i].cur);
            uiPowerMetClass[uiPowerMetClass.Count - 1].mat.SetFloat("_PowerCur", uiPowerMetClass[uiPowerMetClass.Count - 1].cur);

            //shaReactorMet[btnPowerBool.Count - 1].mat.SetFloat("_PowerCur", uiPowerMetClass[i].cur);

            ChargeOn(i, speed);
            //powerAnimCoroutine.Add(StartCoroutine(ChargeOnAnim(i, speed)));
        }
    }

    public void VentBtn()
    {
        for (int i = 0; i < uiPowerMetClass.Count - 1; i++)
        {
           uiPowerMetClass[i].Charge(false);
            StopCoroutine(powerAnimCoroutine[i]);
        }

        StartCoroutine(VentAnim());
    }

    private IEnumerator VentAnim()
    {
        float time = 0;

        float speed = 1;

        float crgAmt = 5f;

        while (time < crgAmt)
        {
            Mathf.MoveTowards(0, crgAmt, time);

            time += (Time.deltaTime) * (speed);

            for (int i = 0; i < uiPowerMetClass.Count - 1; i++)
            {
                if (uiPowerMetClass[i].cur > 1)
                {
                    uiPowerMetClass[i].cur--;
                    uiPowerMetClass[uiPowerMetClass.Count - 1].cur++;

                    uiPowerMetClass[i].mat.SetFloat("_PowerCur", uiPowerMetClass[i].cur);
                    uiPowerMetClass[uiPowerMetClass.Count - 1].mat.SetFloat("_PowerCur", uiPowerMetClass[uiPowerMetClass.Count - 1].cur);

                    //Mathf.MoveTowards(0, crgAmt, time);
                    //time += (Time.deltaTime) * (speed);

                    yield return new WaitForSeconds(.5f);
                }
            }
        }
    }

    public void FrequancyTune(float speed)
    {
        frequancyFlt = frequancyFlt + speed; //(360 * speed);

        if (frequancyFlt >= 360 * 5)
        {
            frequancyFlt = 360 * 5;
        }
        else if (frequancyFlt <= 0)
        {
            frequancyFlt = 0;
        }

        //float multi = Mathf.Pow(100, 3);

        //float roundedFrequancy = MathF.Round(frequancyFlt * multi) / multi;

        frequancyTrans.rotation = Quaternion.Euler(0f, 0f, frequancyFlt * -1);

        frequancyTMP.text = frequancyFlt.ToString();

        //frequancyFlt = roundedFrequancy;

        float normF = tunerTrans[1].localPosition.x / tunerTrans[2].localPosition.x;

        tunerTrans[0].localPosition = Vector2.Lerp(tunerTrans[1].localPosition, tunerTrans[2].localPosition, frequancyFlt/(360*5));


    }

    public void BtnScannerCloke()
    {
        //cloke the ship
    }

    public void BtnRepair()
    {
        //repair player ship
    }

    public void SignalGhost()
    {
        //create a signal elseware
    }
}
