using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Power : MonoBehaviour
{
    public UnityEngine.UI.Slider reactorSlider, engineSlider, weaponSlider, senorSlider;
    public int reactorPower, enginePower, weaponPower, sensorPower;
    
    void Update()
    {
        reactorSlider.value = reactorPower;
        engineSlider.value = enginePower;
        weaponSlider.value = weaponPower;
        senorSlider.value = sensorPower;
    }
}
