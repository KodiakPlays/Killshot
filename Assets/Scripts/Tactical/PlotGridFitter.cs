using UnityEngine;

/// <summary>
/// Keeps the grid quad covering whatever the plot camera can see.
///
/// The grid pattern is computed from world position in the shader, so moving
/// and resizing the quad does not move the grid — it only decides how much of
/// it exists. Without this the quad runs out at its fixed size and the plot
/// goes black past the edge when you zoom far out.
/// </summary>
[ExecuteAlways]
public class PlotGridFitter : MonoBehaviour
{
    [SerializeField] private Camera plotCamera;
    [Tooltip("Extra coverage past the visible edge, as a multiple of the view.")]
    [SerializeField] private float margin = 1.35f;

    public Camera PlotCamera { get { return plotCamera; } set { plotCamera = value; } }

    private void LateUpdate()
    {
        if (plotCamera == null || !plotCamera.orthographic) return;

        float halfH = plotCamera.orthographicSize;
        float halfW = halfH * plotCamera.aspect;
        float span = Mathf.Max(halfW, halfH) * 2f * margin;

        transform.position = new Vector3(plotCamera.transform.position.x,
                                         plotCamera.transform.position.y, 0f);
        transform.localScale = new Vector3(span, span, 1f);
    }
}
