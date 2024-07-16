using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArcRenderer : MonoBehaviour
{
    public int segments = 50; // Number of segments to divide the arc into
    public float radius = 5.0f; // Radius of the arc
    public float angle = 90.0f; // Angle of the arc in degrees
    public float lineWidth = 0.1f; // Width of the line

    public LineRenderer lineRenderer;
    private Vector3 initialForward;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = lineWidth;
        //initialForward = transform.forward;
        DrawArc();
    }

    void Update()
    {
        DrawArc();
    }

    void DrawArc()
    {
        lineRenderer.positionCount = segments + 3; // +3 to include lines to start and end points of the arc

        float angleStep = angle / segments;
        float currentAngle = -angle / 2; // Start at the leftmost point of the arc

        // Starting line from the GameObject to the first point of the arc
        lineRenderer.SetPosition(0, transform.position);

        for (int i = 0; i <= segments; i++)
        {
            float rad = Mathf.Deg2Rad * currentAngle;
            Vector3 point = new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);
            point = transform.position + transform.rotation * point;
            //point = transform.position + Quaternion.LookRotation(initialForward) * point;
            lineRenderer.SetPosition(i + 1, point);
            currentAngle += angleStep;
        }

        // Ending line from the GameObject to the last point of the arc
        lineRenderer.SetPosition(segments + 2, transform.position);
    }
}
