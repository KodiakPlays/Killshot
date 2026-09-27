using TMPro;
using UnityEngine;

/// <summary>
/// Speed and heading, straight off the ship. The handoff's FLIGHT panel wants
/// signature and reactor here too — add them when they mean something.
/// </summary>
public class ShipReadout : MonoBehaviour
{
    [SerializeField] private PlayerShip ship;
    [SerializeField] private TextMeshProUGUI text;

    private void Reset() { text = GetComponent<TextMeshProUGUI>(); }

    private void LateUpdate()
    {
        if (ship == null || text == null) return;

        // Compass degrees: 0 is north, 90 is east. transform.up is the nose.
        float hdg = Mathf.Repeat(-ship.transform.eulerAngles.z, 360f);

        text.text = "SPD " + Mathf.RoundToInt(ship.GetCurrentSpeed()).ToString("N0") + " u/s" +
                    "    HDG " + Mathf.RoundToInt(hdg).ToString("000") + "°";
    }
}
