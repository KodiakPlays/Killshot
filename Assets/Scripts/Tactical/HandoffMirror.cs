using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 68 (owner 2026-10-25). Keeps copies of board widgets in step with their originals: each copy
/// takes its original's colour (blink included), text and visibility every frame. The signal box in
/// the frame's top-right mirrors the Sensors drawer's E-SIG row this way, so the signal shows with
/// the drawer shut and the rules stay in HandoffSensorsPanel.
/// </summary>
public class HandoffMirror : MonoBehaviour
{
    [SerializeField] private Graphic[] originals;
    [SerializeField] private Graphic[] copies;

    private void LateUpdate()
    {
        for (int i = 0; i < originals.Length && i < copies.Length; i++)
        {
            Graphic a = originals[i], b = copies[i];
            if (!a || !b) continue;
            bool on = a.gameObject.activeSelf;
            if (b.gameObject.activeSelf != on) b.gameObject.SetActive(on);
            b.color = a.color;
            if (a is TMP_Text ta && b is TMP_Text tb && tb.text != ta.text) tb.text = ta.text;
        }
    }
}
