using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The SAVE SLOTS screen (owner 2026-11-08): NEW GAME opens it. Three rows, one per save. An empty row starts a new
/// campaign; a used one carries on with it. Either opens the sector map. DELETE empties a row after a second click.
/// The saves themselves are SaveGame's.
/// </summary>
public class SaveSlots : MonoBehaviour
{
    [Tooltip("The text on each slot's row, slot 1 first.")]
    [SerializeField] private TMP_Text[] rows;
    [Tooltip("Each slot's DELETE button, slot 1 first. Hidden while its slot is empty.")]
    [SerializeField] private TMP_Text[] deletes;
    [Tooltip("Picking a slot opens this scene.")]
    [SerializeField] private string mapScene = "SectorMap";
    private int asking = -1; // the slot whose DELETE asks to be clicked again

    private void Start() => Show();

    void Show()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            var save = SaveGame.Read(i);
            rows[i].text = (i + 1) + "  " + (save == null ? "EMPTY" : "CAMPAIGN · " + save.done.Count + (save.done.Count == 1 ? " MISSION DONE" : " MISSIONS DONE"))
                         + "\n<size=26><alpha=#B0>   " + (save == null ? "START A NEW CAMPAIGN." : "SAVED " + save.savedAt + ".") + "</size>";
            if (i >= deletes.Length) continue;
            deletes[i].transform.parent.gameObject.SetActive(save != null);
            deletes[i].text = asking == i ? "SURE? CLICK AGAIN" : "DELETE";
        }
    }

    /// <summary>A slot's row: play that slot. Safe to call from a Button's onClick.</summary>
    public void Pick(int slot)
    {
        if (Time.timeSinceLevelLoad < 0.5f) return; // the click that opened this screen must not pick a slot
        SaveGame.Play(slot);
        SceneManager.LoadScene(mapScene);
    }

    /// <summary>A slot's DELETE: the first click asks, the second empties the slot.</summary>
    public void Delete(int slot)
    {
        if (asking != slot) { asking = slot; Show(); return; }
        SaveGame.Delete(slot);
        asking = -1;
        Show();
    }
}
