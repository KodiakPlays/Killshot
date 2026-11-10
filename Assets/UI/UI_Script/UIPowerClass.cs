// =================================================================================================
// UIPowerClass.cs
//
// What it does:
// A small record for one power bar in the older ship screens (UIController). It keeps the bar's
// material and its power numbers. It is not placed on anything in a scene. The battle board does
// not use this script.
//
// How it works:
// - It keeps the bar's material, its full and current power, and whether it is charging. Two
//   more values are kept but never used.
// - When made, it puts the current and full power on the material. It sets the charge speed to 5
//   and turns charging off.
// - Charge turns the material's charging look on or off.
// - UIController makes one for each power channel and one for the reactor. Every frame it copies
//   PowerManager's numbers into them.
// - It is written as a script for objects in a scene. UIController makes it in code instead.
//   Unity shows a warning in the Console when that happens.
//
// Technical:
// - Data: mat (the bar's material), max and cur (power units) and charge (the charging look is
//   on). pwr and pwrShift are never read.
// - UIController.StartPower makes them with "new" and keeps them in uiPowerMetClass. The
//   reactor's bar is last in that list.
// - UIController.SyncPowerBarsFromManager writes cur, max and charge directly. It sets the
//   material's power values itself.
// - UpdateMat writes _PowerCur, _PowerMax and _ChargeVelocity on mat. Charge writes _Charge as 0
//   or 1.
// - UIController calls Charge from ChargeOn, ChargeOff, VentBtn and EmergencyVentBtn.
// - Trap: it is a MonoBehaviour made with "new". It has no object in the scene, and Unity warns
//   in the Console.
// - The charge speed of 5 is in the code.
// =================================================================================================

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
    public bool pwrShift;

    public UIPowerClass(Material mat, int max, int cur, float pwr, bool charge)
    {
        this.mat = mat;
        this.max = max;
        this.cur = cur;
        this.pwr = pwr;
        this.charge = charge;
        this.pwrShift = pwrShift;

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
            //Debug.Log("off: " + (max - 6));

            mat.SetFloat("_Charge", 0f);
            charge = false;
        }
        else if (chg)
        {
            //Debug.Log("on: " + (max - 6));
            mat.SetFloat("_Charge", 1f);
            charge = true;
        }
    }
}
