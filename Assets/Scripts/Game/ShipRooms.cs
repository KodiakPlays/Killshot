using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The ship between missions (owner 2026-11-08): the board's metal frame with a tab for each room (BRIDGE, ENGINEERING,
/// HANGAR, QUARTERS) and SECTOR MAP to go on. Each room is one 2D painting with people in it. Click a person and the talk
/// box opens: their picture, their name, what they say and the answers you can pick. The paintings and pictures are
/// stand-in boxes until the art comes: put it on the Art List on this object, one slot per painting and per person.
/// </summary>
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
