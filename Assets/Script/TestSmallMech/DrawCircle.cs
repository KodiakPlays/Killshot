using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCircle : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int subdivision = 20;
    public int radius = 5;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float anglestep = 2f * Mathf.PI / subdivision;
        lineRenderer.positionCount = subdivision;
        for (int i = 0; i < subdivision; i++)
        {
            float xPos = radius * Mathf.Cos(anglestep * i);
            float zPos = radius * Mathf.Sin(anglestep * i);

            Vector3 pointInCircle = new Vector3(xPos, 0, zPos);
            lineRenderer.SetPosition(i, pointInCircle);
        }
    }
}
