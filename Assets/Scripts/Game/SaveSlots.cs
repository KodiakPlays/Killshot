// =================================================================================================
// SaveSlots.cs
//
// What it does:
// The SAVE SLOTS screen. It is in the SaveSlots scene. It shows three rows, one per save. Picking
// an empty row starts a new campaign. Picking a used row goes on with that campaign. Either one
// opens the sector map.
//
// How it works:
// - When the screen opens, it reads each slot through SaveGame and writes its row. An empty slot
//   says EMPTY. A used one shows the save time and how much of the campaign is won. It reads
//   "CAMPAIGN", then for example "1 OF 10" and "10%". With all won it reads "CAMPAIGN COMPLETE".
//   Only the missions switched on in the Campaign file count.
// - Each row's DELETE button shows only while its slot has a save.
// - A click on a row picks that slot in SaveGame and loads the SectorMap scene. Clicks in the
//   first half second after the screen opens are ignored.
// - The first click on DELETE changes it to SURE? CLICK AGAIN. The second click deletes the save.
// - The row buttons in the scene call it when clicked.
//
// Technical:
// - Data: rows and deletes are the row texts and DELETE texts, slot 1 first. asking is the slot
//   waiting for a second DELETE click, or -1.
// - Pick(slot) and Delete(slot) are set on the Buttons in the SaveSlots scene. Slots count from 0.
// - Pick calls SaveGame.Play, then loads mapScene. Delete calls SaveGame.Delete on the second
//   click.
// - Show reads each slot with SaveGame.Read and counts wins with Campaign.Won. Each DELETE click
//   redraws the rows.
// - The DELETE button is the parent object of each DELETE text. Show hides it for an empty slot.
// - With no Campaign file set, a used row shows how many levels are won, with no percent.
// - SectorMap opens this scene when no slot is picked.
// - Numbers: mapScene and the Campaign file are in the Inspector.
// =================================================================================================

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlots : MonoBehaviour
{
    [Tooltip("The text on each slot's row, slot 1 first.")]
    [SerializeField] private TMP_Text[] rows;
    [Tooltip("Each slot's DELETE button, slot 1 first. Hidden while its slot is empty.")]
    [SerializeField] private TMP_Text[] deletes;
    [Tooltip("Picking a slot opens this scene.")]
    [SerializeField] private string mapScene = "SectorMap";
    [Tooltip("The Campaign file: a used slot shows how many of its missions are won, as a percent (owner 2026-11-09).")]
    [SerializeField] private Campaign campaign;
    private int asking = -1; // the slot whose DELETE asks to be clicked again

    // "CAMPAIGN · 4 OF 10 · 40%", or "CAMPAIGN COMPLETE · 100%" once every mission is won. Old test levels don't count.
    string Progress(SaveGame.Data save)
    {
        int total = campaign ? campaign.On.Length : 0;
        if (total == 0) return "CAMPAIGN · " + save.done.Count + (save.done.Count == 1 ? " MISSION DONE" : " MISSIONS DONE");
        int won = campaign.Won(save.done);
        return won >= total ? "CAMPAIGN COMPLETE · 100%" : "CAMPAIGN · " + won + " OF " + total + " · " + Mathf.FloorToInt(100f * won / total) + "%";
    }

    private void Start() => Show();

    void Show()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            var save = SaveGame.Read(i);
            rows[i].text = (i + 1) + "  " + (save == null ? "EMPTY" : Progress(save))
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
