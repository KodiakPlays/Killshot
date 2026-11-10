// =================================================================================================
// PowerManagerEditor.cs
//
// What it does:
// This is the Inspector for PowerManager, the older power script. That script is on the
// PlayerShip prefab and in the Demo scene. This Inspector keeps the power numbers up to date during
// play mode. The battle board does not use this script.
//
// How it works:
// - It draws PowerManager's normal settings, with nothing added.
// - In play mode, while the Inspector shows a PowerManager, it redraws the Inspector every
//   editor frame.
// - Each power channel's current power and state (standby, draw, vent) stay correct on screen
//   while the game runs.
//
// Technical:
// - [CustomEditor(typeof(PowerManager))] makes Unity draw this Inspector for every PowerManager.
// - OnEnable adds ForceRepaint to EditorApplication.update. OnDisable takes it off again.
// - ForceRepaint calls Repaint() only while Application.isPlaying is true.
// - The values it keeps fresh are PowerChannel.currentPower and PowerChannel.currentState.
// - No other script calls it. The battle board's power is in PowerSystem.
// =================================================================================================

using UnityEditor;
using UnityEngine;

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
