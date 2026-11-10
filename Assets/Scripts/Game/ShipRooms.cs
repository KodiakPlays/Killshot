// =================================================================================================
// ShipRooms.cs
//
// What it does:
// Our ship between campaign missions. It is in the Ship scene. There is a tab for each room, and
// a SECTOR MAP tab to go on. Each room is one picture with people in it. Click a person to talk.
//
// How it works:
// - The rooms, their tabs, their pictures and their people are set in the Inspector. Each person
//   has a picture button and a list of lines. Each line has up to three answers.
// - When the scene opens, it shows the first room. A picture with art on it loses the grey tint
//   of the stand-in box.
// - Clicking a tab shows that room, hides the others and closes the talk box. The picked tab's
//   name turns amber.
// - Clicking a person opens the talk box with their picture, name and first line.
// - Each answer button leads to another line, or ends the talk. A line with no answers shows a
//   single LEAVE button.
// - The damage report shows each part of our ship that is not whole, from the save in SaveGame.
//   With no damage it says ALL PARTS WHOLE.
// - REPAIR ALL shows only while there is damage. A click clears the damage in the save.
// - The SECTOR MAP tab loads the SectorMap scene.
//
// Technical:
// - Data: rooms is a list of Room: name, view, tab and people. Each Person has a name, a picture
//   Button and lines. Each Line has replies. A Reply's next is a line number from 0, or -1 to end.
// - The Ship scene has four rooms: BRIDGE, ENGINEERING, HANGAR and QUARTERS.
// - Start adds every click in code: tabs, people, answers, REPAIR ALL and the SECTOR MAP tab.
// - Swap the paintings and people's pictures in the ArtList on the same object.
// - SaveGame: it reads SaveGame.Current.damage and calls SaveGame.Repair. Outside the campaign the
//   report says ALL PARTS WHOLE.
// - EndScreenPanel opens the Ship scene after a campaign mission. No other script calls this one.
// - Tab names turn to gamma before they are coloured. The canvas blends in gamma space.
// - Trap: there are as many answer buttons as replyButtons. Extra replies in a Line never show.
// - Numbers: the rooms, lines, colours and mapScene are in the Inspector.
// =================================================================================================

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShipRooms : MonoBehaviour
{
    [System.Serializable]
    public class Reply
    {
        public string text;
        [Tooltip("The line this answer leads to, counting from 0. -1 ends the talk.")]
        public int next = -1;
    }

    [System.Serializable]
    public class Line
    {
        [TextArea] public string text;
        [Tooltip("Up to three answers. None: a single LEAVE.")]
        public Reply[] replies;
    }

    [System.Serializable]
    public class Person
    {
        public string name;
        [Tooltip("Their picture in the room. Click it to talk. The talk box shows the same picture.")]
        public Button picture;
        [Tooltip("What they say, from the first line.")]
        public Line[] lines;
    }

    [System.Serializable]
    public class Room
    {
        public string name;
        [Tooltip("The room: its painting and the people in it. Shown while its tab is picked.")]
        public GameObject view;
        public Button tab;
        public Person[] people;
    }

    [SerializeField] private Room[] rooms;
    [Tooltip("The tab that leaves the ship for the sector map.")]
    [SerializeField] private Button mapTab;
    [SerializeField] private string mapScene = "SectorMap";

    [Header("Talk box")]
    [SerializeField] private GameObject talkBox;
    [SerializeField] private Image talkPicture;
    [SerializeField] private TMP_Text talkName, talkLine;
    [Tooltip("The answer buttons, top first. Each one's first text is its words.")]
    [SerializeField] private Button[] replyButtons;

    [Header("Repairs (owner 2026-11-08): in the campaign, damage stays on the ship until it is repaired here")]
    [Tooltip("The damage report in ENGINEERING: each part of our ship that is not whole.")]
    [SerializeField] private TMP_Text damageReport;
    [Tooltip("REPAIR ALL: the ship is whole again. Shown while there is damage.")]
    [SerializeField] private Button repairAll;

    [Header("Look")]
    [SerializeField] private Color tabPicked = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color tabNot = new Color32(0x77, 0x82, 0x8A, 0xFF);

    private Person talking;
    private int saying;

    private void Start()
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            int room = i;
            rooms[i].tab.onClick.AddListener(() => Show(room));
            ShowArt(rooms[i].view.GetComponent<Image>());
            foreach (var p in rooms[i].people)
            {
                var person = p;
                p.picture.onClick.AddListener(() => Talk(person));
                ShowArt(p.picture.image);
            }
        }
        mapTab.onClick.AddListener(() => SceneManager.LoadScene(mapScene));
        for (int i = 0; i < replyButtons.Length; i++)
        {
            int k = i;
            replyButtons[i].onClick.AddListener(() => Answer(k));
        }
        if (repairAll) repairAll.onClick.AddListener(() => { SaveGame.Repair(); ShowDamage(); });
        ShowDamage();
        Show(0);
    }

    // A picture with art on it (the Art List) shows the art as it is; the grey of the stand-in box would tint it.
    static void ShowArt(Image image)
    {
        if (image && image.sprite) image.color = Color.white;
    }

    /// <summary>Show one room; the others hide and the talk stops.</summary>
    public void Show(int room)
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i].view.SetActive(i == room);
            var label = rooms[i].tab.GetComponentInChildren<TMP_Text>();
            if (label) label.color = (i == room ? tabPicked : tabNot).gamma; // the board canvas linearises vertex colours; pre-encode
        }
        talking = null;
        talkBox.SetActive(false);
    }

    void ShowDamage()
    {
        if (!damageReport) return;
        var damage = SaveGame.InCampaign ? SaveGame.Current.damage : null;
        bool hurt = damage != null && damage.Count > 0;
        var lines = new System.Text.StringBuilder();
        if (hurt)
            foreach (var p in damage)
                if (p.now < p.most) lines.AppendLine(p.name.PadRight(16) + Mathf.FloorToInt(p.now) + " / " + Mathf.RoundToInt(p.most));
        damageReport.text = hurt ? lines.ToString() : "ALL PARTS WHOLE.";
        if (repairAll) repairAll.gameObject.SetActive(hurt);
    }

    void Talk(Person person)
    {
        talking = person;
        talkName.text = person.name;
        talkPicture.sprite = person.picture.image.sprite;
        talkPicture.color = person.picture.image.color;
        Say(0);
    }

    void Say(int line)
    {
        if (talking == null || line < 0 || line >= talking.lines.Length) { talking = null; talkBox.SetActive(false); return; }
        saying = line;
        var l = talking.lines[line];
        talkLine.text = l.text;
        int n = l.replies != null ? l.replies.Length : 0;
        for (int i = 0; i < replyButtons.Length; i++)
        {
            bool shown = i < Mathf.Max(1, n);
            replyButtons[i].gameObject.SetActive(shown);
            if (shown) replyButtons[i].GetComponentInChildren<TMP_Text>().text = n == 0 ? "LEAVE" : l.replies[i].text;
        }
        talkBox.SetActive(true);
    }

    void Answer(int i)
    {
        if (talking == null) return;
        var replies = talking.lines[saying].replies;
        Say(replies != null && i < replies.Length ? replies[i].next : -1);
    }
}
