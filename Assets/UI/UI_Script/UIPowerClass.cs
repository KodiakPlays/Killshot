using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class UIPowerClass : MonoBehaviour
{
    public Material mat;
    public int max, cur;
    public float pwr;
    public bool charge;

    public UIPowerClass(Material mat, int max, int cur, float pwr, bool charge)
    {
        this.mat = mat;
        this.max = max;
        this.cur = cur;
        this.pwr = pwr;
        this.charge = charge;

        UpdateMat();
    }

    public void UpdateMat()
    {
        Charge(false);
        mat.SetFloat("_PowerCur", cur);
        mat.SetFloat("_PowerMax", max);
        mat.SetFloat("_ChargeVelocity", 5);
    }

    public void Charge(bool chg)
    {
      
        if (!chg)
        {
            Debug.Log("off: " + (max - 6));

            mat.SetFloat("_Charge", 0f);
            charge = false;
        }
        else if (chg)
        {
            Debug.Log("on: " + (max - 6));
            mat.SetFloat("_Charge", 1f);
            charge = true;
        }
    }
}
