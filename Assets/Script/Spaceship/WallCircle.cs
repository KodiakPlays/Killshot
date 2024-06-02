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

    [SerializeField] bool isShipInside;

    private void Awake()
    {
        LineRendIn();
    }
    private void Update()
    {
        DrawCicle();
        if (!isShipInside)
        {
            targetTime -= Time.deltaTime;
            if (targetTime <= 0.0f)
            {
                targetTime = 0.0f;
                timerEnded();
            }
        }
    }
    void timerEnded()
    {
        Debug.Log("Destroy ship by time over");
        Destroy(spaceShip);
        GameManager.Instance.EndGame();
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
        DrawCicle();
    }
    void DrawCicle()
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
