using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCircle : MonoBehaviour
{
    public int segments = 25;
    public Material circleMat;
    public int radius;
    public LineRenderer lineRenderer;
    public LayerMask shipLayer;

    public float targetTime = 10.0f;
    public GameObject spaceShip;
    SpaceshipMovement shipMoveScript;
    [SerializeField] bool isShipInside;

    private void Awake()
    {
        shipMoveScript = spaceShip.gameObject.GetComponent<SpaceshipMovement>();
        LineRendIn();
    }
    private void Update()
    {
        DrawCircle();
        StartTimerIfShipOutside();
    }
    void LineRendIn()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // Set LineRenderer properties
        lineRenderer.positionCount = segments + 1;
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
        lineRenderer.useWorldSpace = true;
        //lineRenderer.startColor = circleColor;
        //lineRenderer.endColor = circleColor;
        lineRenderer.material = circleMat;
        DrawCircle();
    }
    void DrawCircle()
    {
        // Calculate segment size
        float angleStep = 360f / segments;

        // Update LineRenderer positions
        for (int i = 0; i <= segments; i++)
        {
            float angle = angleStep * i;
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            lineRenderer.SetPosition(i, transform.position + new Vector3(x, 0, z));
        }
    }
    void StartTimerIfShipOutside()
    {
        if (!isShipInside)
        {
            targetTime -= Time.deltaTime;
            shipMoveScript.alertText.text = "Ship is out of range you only have " + ((int)targetTime) + " sec to come back";
            shipMoveScript.alertText.color = Color.red;
            if (targetTime <= 0.0f)
            {
                targetTime = 0.0f;
                TimerEnded();

            }
        }
    }
    void TimerEnded()
    {
        Debug.Log("Destroy ship by time over");
        Destroy(spaceShip);
        GameManager.Instance.EndGame();
    }
  
   
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Spaceship"))
        {
            isShipInside = true;
            targetTime = 10;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Spaceship"))
        {
            isShipInside = false;
        }
    }
}
