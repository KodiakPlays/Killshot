using UnityEngine;
using UnityEngine.UI;

public class ToggleHandler : MonoBehaviour
{
    [SerializeField] Toggle myToggle; // Reference to the Toggle component
    [SerializeField] GameObject areneMap, arenaWholeMap;

    void Start()
    {
        // Add listener to detect toggle changes
        myToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    public void OnToggleChanged(bool isOn)
    {
        if (isOn && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            arenaWholeMap.SetActive(false);
            areneMap.SetActive(true);
            Debug.Log("Toggle is ON");
            // Perform actions when the toggle is on
        }
        else if(!isOn && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            arenaWholeMap.SetActive(true);
            areneMap.SetActive(false);
            Debug.Log("Toggle is OFF");
            // Perform actions when the toggle is off
        }
    }
}
