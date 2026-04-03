using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for PowerManager that forces a repaint every editor frame
/// during Play Mode so the live currentPower / currentState values on every
/// PowerSystem are always up-to-date in the Inspector.
/// </summary>
[CustomEditor(typeof(PowerManager))]
public class PowerManagerEditor : Editor
{
    private void OnEnable()
    {
        EditorApplication.update += ForceRepaint;
    }

    private void OnDisable()
    {
        EditorApplication.update -= ForceRepaint;
    }

    private void ForceRepaint()
    {
        if (Application.isPlaying)
            Repaint();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
