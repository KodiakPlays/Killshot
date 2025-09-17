using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

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

    void Start()
    {
        StartPower();
    }

    private void StartPower()
    {
        for (int i = 0; i < btnPowerBool.Count; i++)
        {
            uiPowerMetClass.Add(new UIPowerClass(new Material(shaPowerMet), 6+i, 1, 0f, false));

            imgPowerMet[i].material = uiPowerMetClass[i].mat;
        }

        //for (int i = 0; i < 5; i++)
        //{
        //    powerAnimCoroutine.Add(i, ChargeOnAnim(i, 1));
        //}
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
        //if (powerAnimCoroutine == null)
        //{
            //uiPowerMetClass[i].mat.SetFloat("_Charge", 1f);
        uiPowerMetClass[i].Charge(true);

        powerAnimCoroutine[i] = ChargeOnAnim(i, 1);

        StartCoroutine(powerAnimCoroutine[i]);

        //powerAnimCoroutine.Add(StartCoroutine(ChargeOnAnim(i, speed)));
        //}
    }

    public void ChargeOff(int i)
    {
        Debug.Log("off: " + (i));
        //if (powerAnimCoroutine != null)
        //{
        //uiPowerMetClass[i].mat.SetFloat("_Charge", 0f);
        uiPowerMetClass[i].Charge(false);
            //uiPowerMetClass.R
        StopCoroutine(powerAnimCoroutine[i]);
        //}

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

        if (uiPowerMetClass[i].cur >= uiPowerMetClass[i].max)
        {
            ChargeOff(i);
        }
        else if (uiPowerMetClass[i].cur < uiPowerMetClass[i].max)
        {
            uiPowerMetClass[i].cur++;

            uiPowerMetClass[i].mat.SetFloat("_PowerCur", uiPowerMetClass[i].cur);

            ChargeOn(i, speed);
            //powerAnimCoroutine.Add(StartCoroutine(ChargeOnAnim(i, speed)));
        }
    }

    private IEnumerator VentAnim(int i)
    {
        float time = 0;

        float speed = 1;

        while (time < 1)
        {
            Mathf.MoveTowards(0, 1, time);

            time += (Time.deltaTime) * (speed);

            yield return null;

        }

        uiPowerMetClass[i].cur = 1;

        uiPowerMetClass[i].UpdateMat();
    }
}
