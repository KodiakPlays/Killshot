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
// - ENGINEERING's damage report lists each part that is not whole as a REPAIR button. With no
//   damage it says ALL PARTS WHOLE.
// - The travel bar at the top fills as actions are taken. A won mission gives a fresh bar of 10
//   units.
// - Each room shows its own actions. ENGINEERING repairs one part at a time. HANGAR restocks
//   torpedoes and gives extra ammo. QUARTERS heals the marines.
// - Each action costs units and gives back a share, not everything. It is saved at once.
// - HANGAR's and QUARTERS' actions always show, so the stock can be read. They grey out when full.
// - An action that doesn't fit in the bar is greyed out. When the bar is full, SECTOR MAP flashes.
// - The SECTOR MAP tab loads the SectorMap scene. The map shuts the bar, so any units left are
//   lost. It shuts it however the rooms were left.
// - With no open bar, after a lost mission or back without a new win, the bar reads 10 / 10.
//   Every action is greyed out and SECTOR MAP flashes.
//
// Technical:
// - Data: rooms is a list of Room: name, view, tab, people and actions. Each Person has a name, a picture
//   Button and lines. Each Line has replies. A Reply's next is a line number from 0, or -1 to end.
// - The Ship scene has four rooms: BRIDGE, ENGINEERING, HANGAR and QUARTERS.
// - Start adds every click in code: tabs, people, answers and the SECTOR MAP tab.
// - Swap the paintings and people's pictures in the ArtList on the same object.
// - SaveGame: it reads SaveGame.Current.travelOpen and changes damage, stores and travelUsed.
//   SaveGame.KeepWonDamage opens the bar. SectorMap.Start shuts it.
//   Each action calls SaveGame.Save. Outside the campaign there are no actions and the bar is full.
// - Action buttons are copies of actionTemplate (the switched-off Action), made in the room's
//   actions list each time a room shows. The bar's fill is stretched; its right edge moves.
// - A save that kept no stores means all are full. Then the numbers come from ourShip.
// - ourShip (the Asura stats file) tells torpedoes (MS), slugs (RG) and PDC rounds (PD) apart.
// - Costs and what each action gives are [Tune("Travel bar")] numbers. They show in the Tuning
//   window only with this scene open.
// - Trap: actions find their room by its name: ENGINEERING, HANGAR, QUARTERS. A renamed room
//   loses its actions.
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
        [Tooltip("Where this room's action buttons go. Empty: no actions here.")]
        public RectTransform actions;
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

    [Header("Travel bar and actions (owner 2026-11-09 and 2026-11-10)")]
    [Tooltip("Our ship's stats file. It tells which saved rounds are torpedoes, railgun slugs or PDC rounds.")]
    [SerializeField] private ShipStats ourShip;
    [Tooltip("The bar's fill, stretched across the bar. Its right edge moves as the bar fills.")]
    [SerializeField] private RectTransform travelFill;
    [Tooltip("The bar's words: TRAVEL, units used, units in the bar.")]
    [SerializeField] private TMP_Text travelText;
    [Tooltip("Switched off. Copied once for each action, into the room's Actions.")]
    [SerializeField] private Button actionTemplate;
    [Tooltip("Units in the travel bar. Each action fills its cost. Full: the actions lock and SECTOR MAP flashes.")]
    [Tune("Travel bar")] [SerializeField] private int travelUnits = 10;
    [Tune("Travel bar")] [SerializeField] private int reactorCost = 3, roomCost = 2, armourCost = 1;
    [Tooltip("Points one repair gives a room, the reactor too, out of 5.")]
    [Tune("Travel bar")] [SerializeField] private int roomPoints = 2;
    [Tooltip("Share of a side's full armour one repair gives back.")]
    [Tune("Travel bar")] [SerializeField] private float armourShare = 0.25f;
    [Tune("Travel bar")] [SerializeField] private int ammoCost = 1;
    [Tooltip("Share of each railgun's and PDC group's full rounds one EXTRA AMMO gives back, rounded down.")]
    [Tune("Travel bar")] [SerializeField] private float ammoShare = 0.2f;
    [Tune("Travel bar")] [SerializeField] private int torpedoCost = 2;
    [Tooltip("Torpedoes one RESTOCK gives back, to the emptiest launcher first.")]
    [Tune("Travel bar")] [SerializeField] private int torpedoes = 4;
    [Tune("Travel bar")] [SerializeField] private int marineCost = 2;
    [Tooltip("Marines one HEAL gives back.")]
    [Tune("Travel bar")] [SerializeField] private int marines = 6;
    [Tooltip("SECTOR MAP flashes this many times a second once the bar is full.")]
    [Tune("Travel bar")] [SerializeField] private float flashPerSecond = 1f;

    [Header("Look")]
    [SerializeField] private Color tabPicked = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color tabNot = new Color32(0x77, 0x82, 0x8A, 0xFF);

    private Person talking;
    private int saying, shown;
    private readonly System.Collections.Generic.List<Button> made = new System.Collections.Generic.List<Button>(); // the room's action buttons
    private TMP_Text mapLabel;

    // The bar is full, or shut: no more actions, go on to the map. Outside the campaign it is always full.
    bool Full => !SaveGame.InCampaign || !SaveGame.Current.travelOpen || SaveGame.Current.travelUsed >= travelUnits;

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
        mapTab.onClick.AddListener(() => SceneManager.LoadScene(mapScene)); // the map shuts the travel bar
        mapLabel = mapTab.GetComponentInChildren<TMP_Text>();
        for (int i = 0; i < replyButtons.Length; i++)
        {
            int k = i;
            replyButtons[i].onClick.AddListener(() => Answer(k));
        }
        ShowDamage();
        ShowBar();
        Show(0);
    }

    private void Update()
    {
        if (Full && mapLabel) mapLabel.color = Color.Lerp(tabNot, tabPicked, Mathf.PingPong(Time.time * 2f * flashPerSecond, 1f)).gamma;
    }

    void ShowBar()
    {
        int used = Full ? travelUnits : SaveGame.Current.travelUsed;
        if (travelFill) travelFill.anchorMax = new Vector2(used / (float)Mathf.Max(1, travelUnits), 1f);
        if (travelText) travelText.text = "TRAVEL  " + used + " / " + travelUnits;
    }

    // The actions in the room shown: ENGINEERING repairs, HANGAR restocks, QUARTERS heals the marines (owner 2026-11-10).
    void ShowActions()
    {
        foreach (var b in made) Destroy(b.gameObject);
        made.Clear();
        var list = rooms[shown].actions;
        if (!SaveGame.InCampaign || !actionTemplate || !list) return;
        var save = SaveGame.Current;
        string room = rooms[shown].name;
        if (room == "ENGINEERING")
            foreach (var p in save.damage)
            {
                if (p.now >= p.most) continue;
                var part = p;
                bool armour = p.name.EndsWith(" ARMOUR");
                float gain = armour ? p.most * armourShare : roomPoints;
                Add(list, "REPAIR " + p.name, Mathf.FloorToInt(p.now) + " / " + Mathf.RoundToInt(p.most), "+" + Mathf.FloorToInt(gain),
                    armour ? armourCost : p.name == "REACTOR" ? reactorCost : roomCost, true, () => part.now = Mathf.Min(part.most, part.now + gain));
            }
        // Restocking always shows, greyed out when full, so the stock can be read.
        var stores = Stores();
        if (room == "HANGAR")
        {
            var tubes = stores.FindAll(p => Kind(p.name) == "MS");
            Add(list, "RESTOCK TORPEDOES", Total(tubes), "+" + torpedoes, torpedoCost, tubes.Exists(p => p.now < p.most), () =>
            {
                for (int k = 0; k < torpedoes; k++)
                {
                    var low = tubes.Find(p => p.now < p.most);
                    foreach (var p in tubes) if (p.now < p.most && p.now < low.now) low = p;
                    if (low != null) low.now++;
                }
            });
            var guns = stores.FindAll(p => Kind(p.name) == "RG" || Kind(p.name) == "PD");
            Add(list, "EXTRA AMMO", "RAILGUN " + Total(guns.FindAll(p => Kind(p.name) == "RG")) + "   PDC " + Total(guns.FindAll(p => Kind(p.name) == "PD")),
                "+" + Mathf.RoundToInt(ammoShare * 100f) + "%", ammoCost, guns.Exists(p => p.now < p.most),
                () => { foreach (var p in guns) p.now = Mathf.Min(p.most, p.now + Mathf.Floor(p.most * ammoShare)); });
        }
        if (room == "QUARTERS")
        {
            var m = stores.FindAll(p => p.name == SaveGame.Marines);
            Add(list, "HEAL MARINES", Total(m), "+" + marines, marineCost, m.Exists(p => p.now < p.most),
                () => m[0].now = Mathf.Min(m[0].most, m[0].now + marines));
        }
    }

    // The saved supplies. A save with none kept means all are full: read them from our stats file.
    System.Collections.Generic.List<SaveGame.Part> Stores()
    {
        if (SaveGame.Current.stores.Count > 0 || !ourShip) return SaveGame.Current.stores;
        var full = new System.Collections.Generic.List<SaveGame.Part>();
        foreach (var m in ourShip.weapons.mounts) full.Add(new SaveGame.Part { name = m.label, now = m.ammo, most = m.ammo });
        full.Add(new SaveGame.Part { name = SaveGame.Marines, now = ourShip.hangar.marines, most = ourShip.hangar.marines });
        return full;
    }

    // One action's button: what, how it stands, what it gives, its cost. Greyed out when not needed or it doesn't fit in the bar.
    void Add(RectTransform list, string what, string state, string gain, int cost, bool needed, System.Action run)
    {
        var b = Instantiate(actionTemplate, list);
        var label = b.GetComponentInChildren<TMP_Text>(true); // true: the copy is still switched off here
        label.text = what + "   " + state + "   " + gain + "   COST " + cost;
        b.interactable = needed && !Full && SaveGame.Current.travelUsed + cost <= travelUnits;
        label.alpha = b.interactable ? 1f : 0.35f;
        b.onClick.AddListener(() =>
        {
            if (Full || SaveGame.Current.travelUsed + cost > travelUnits) return;
            run();
            SaveGame.Current.travelUsed += cost;
            SaveGame.Save();
            ShowDamage();
            ShowBar();
            ShowActions();
        });
        b.gameObject.SetActive(true);
        made.Add(b);
    }

    // The mount's kind in our stats file, by its label: RG railgun, PD point defence, MS torpedo launcher.
    string Kind(string label)
    {
        if (ourShip) foreach (var m in ourShip.weapons.mounts) if (m.label == label) return m.kind;
        return "";
    }

    static string Total(System.Collections.Generic.List<SaveGame.Part> parts)
    {
        float now = 0f, most = 0f;
        foreach (var p in parts) { now += p.now; most += p.most; }
        return Mathf.FloorToInt(now) + " / " + Mathf.RoundToInt(most);
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
        shown = room;
        ShowActions();
    }

    void ShowDamage()
    {
        if (!damageReport) return;
        var damage = SaveGame.InCampaign ? SaveGame.Current.damage : null;
        bool hurt = damage != null && damage.Exists(p => p.now < p.most);
        damageReport.text = hurt ? "" : "ALL PARTS WHOLE."; // each damaged part is a REPAIR button in its place
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
