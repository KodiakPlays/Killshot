using UnityEngine;

public class RadarTarget : MonoBehaviour
{
    public Sprite icon;
    public Color color = Color.white;
    public bool trackRotation = false;

    private void OnEnable()
    {
        Radar.RegisterTarget(this);
    }

    private void OnDisable()
    {
        Radar.UnregisterTarget(this);
    }
}
