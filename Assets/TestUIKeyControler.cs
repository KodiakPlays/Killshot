using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class TestUIKeyControler : MonoBehaviour
{
    public UIController uiContoller;
    public Image gridBK;
    public Material gridMat;
    [SerializeField] private Shader gridShad;
    float rotation = 0;
    float movmentSpeed = 100;

    void Start()
    {
        gridMat = new Material(gridShad);
        gridBK.GetComponent<Image>().material = gridMat;

        gridMat.SetFloat("_SpeedMovement", rotation);
        gridMat.SetFloat("_SpeedRotation", movmentSpeed);
    }

    void Update()
    {
        //increase speed
        if (Input.GetKey(KeyCode.W))
        {

            gridMat.SetFloat("_SpeedMovement", (++movmentSpeed / 100));

            uiContoller.updateSpeedometer(1000f);//this is a made up number
        }
        else if (Input.GetKey(KeyCode.S))
        {
            gridMat.SetFloat("_SpeedMovement", (--movmentSpeed / 100));

            uiContoller.updateSpeedometer(-1000f);//this is a made up number
        }

        //bring speedometer back to 0
        if (Input.GetKeyUp(KeyCode.W))
        {

            uiContoller.updateSpeedometer(0f);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {

            uiContoller.updateSpeedometer(0f);
        }

        //ship rotates
        if (Input.GetKey(KeyCode.A))
        {
            gridMat.SetFloat("_SpeedRotation", (++rotation / 100));

            uiContoller.updateCompass(++rotation / 100f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            gridMat.SetFloat("_SpeedRotation", (--rotation / 100));

            uiContoller.updateCompass(--rotation / 100f);
        }

        //ship gets hit/takes dmg effect
        if (Input.GetKeyDown(KeyCode.B))
        {
            uiContoller.updateShipHit(1f);
        }

        //load in sensor info
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            uiContoller.updateCompass(false, 3, 0, 0, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))//asteroid
        {
            uiContoller.updateCompass(true, 0, 25, 25, 25, 25);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))//orbiter
        {
            uiContoller.updateCompass(true, 1, 50, 50, 50, 50);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))//enemy ship
        {
            uiContoller.updateCompass(true, 2, 100, 100, 100, 100);
        }
    }
}
