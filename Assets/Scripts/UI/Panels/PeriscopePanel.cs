// =================================================================================================
// PeriscopePanel.cs
//
// What it does:
// The Periscope panel on the battle board. It is the targeting computer. The player turns the
// round sight onto a ship. They zoom until the ship fits the box in the middle. Then they wait for
// the lock bar to fill. It shows what a scan has found about that ship. It is on the Periscope
// prefab.
//
// How it works:
// - Dragging across the sight turns its bearing. Dragging the MAG knob sideways zooms from 1x to
//   8x. SLEW turns the sight straight onto the target, or onto a bearing dialled by hand.
//   ShipState makes each change.
// - BoardManager redraws it after every click and every 0.42 seconds. It reads ShipState,
//   WeaponSystem and SensorSystem.
// - The readouts show the gun's short name, the zoom and the bearing. They show how far off axis
//   the sight is: green on axis, amber within 12 degrees. SLEW shows an arrow while dragging.
//   ScopeSight draws the moving sight picture with this zoom.
// - While the periscope is down, it picks the heard ship nearest the sight's middle, within 27
//   degrees. The box appears, with the ship's name (BOGEY until scanned) and a hint: SLEW LEFT or
//   RIGHT, ZOOM IN, ZOOM OUT, TOO FAR, LOCKING with a percent, or LOCKED with seconds left.
// - The box is white, amber when the ship fits it, and green once locked on that ship.
// - Through BoardManager it plays a beep as the ship comes to fit. Another beep plays as the lock
//   comes on. The refused sound plays as the lock drops.
// - The ship's picture is the plot's top-down picture of it, from its ShipView camera, drawn red.
//   Its size follows zoom and range. Every frame, after the models move, it turns the picture
//   flat. The bow points left or right by the way the ship is heading.
// - The lock bar fills as the lock builds, then runs down as the lock runs out. It is amber, then
//   green while locked.
// - The subject box names the ship on the sight's bearing. Once scanned, it shows each side's
//   armour in percent. Through an open side, it shows the room the next hit would damage and that
//   room's health. A destroyed ship reads WRECK, a captured one PRIZE.
// - The SLEW lamp is green on axis and amber off it. The playtest runner reads the subject box,
//   the armour line and the angle of the ship picture.
//
// Technical:
// - Data: the sight's bearing, zoom and slew are in ShipState (scopeBrg, scopeZoom, slewing). This
//   script keeps drag start points and the fit and lock state of the last redraw.
// - BoardManager calls Click, Down, Drag, Up and Refresh. The click is "scope.slewTo". Press and
//   drag are "scope.slew" and "scope.mag".
// - It calls ShipState: SlewTo, Slew, SetZoom and EndSlew. It reads ScopeOn, ScopeFits, ScopeFill,
//   ScopeSubject and ScopeOnAxis.
// - The lock is in WeaponSystem: Locked, LockedOn, lockFill, lockLeft and LockBar. The sounds go
//   through BoardManager.LockCue and LockLost.
// - It finds the ShipView cameras with FindObjectsByType and matches them by WorldContact.trackKey.
//   LateUpdate turns the picture by NavSystem.DrawnHeading less ShipView.WreckHeading.
// - HullLine is public and static. PlaytestRunner calls it. It reads PictureBowDeg and the private
//   subjectName, subjectClass and shipPicture fields by name.
// - Numbers: slew speed, zoom drag, fit distance and fit slack are [Tune] fields on ShipState,
//   under "Periscope & fire control". Lock times are in the weapons part of the stats file.
// - The view width, px per degree, box size, lock bar edges and red ink are in the Inspector.
// - Trap: the ShipView list is found once, the first time a ship is in the sight. A ShipView made
//   later is missed.
// - Trap: convLeft and parentLeft are board px. The picture needs the BoardLinearImage material.
//   The 12 degree amber line is in the code.
// =================================================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PeriscopePanel : Panel
{
    [SerializeField] private TMP_Text mountShort;
    [SerializeField] private Image aperture;
    [Tooltip("periscope_aperture swap: idle, and the amber rim while slewing.")]
    [SerializeField] private Sprite apertureIdle, apertureSlewing;
    [SerializeField] private ScopeSight sight;

    [Header("Readouts")]
    [SerializeField] private TMP_Text offAxis, zoomLabel, bearing, slewLabel, subjectName, subjectClass;
    [SerializeField] private BoardManager.Dial magPointer;

    [Header("Target brackets (owner 2026-11-01): a box at the sight's centre; zoom until the ship fits it")]
    [SerializeField] private RectTransform[] brackets;
    [Tooltip("The name under the brackets: the ship in the sight.")]
    [SerializeField] private TMP_Text targetName;
    [Tooltip("Under the name, what to do: SLEW LEFT, ZOOM IN, ZOOM OUT, LOCKING, LOCKED (owner 2026-11-01).")]
    [SerializeField] private TMP_Text lockHint;
    [SerializeField] private float pxPerDeg = 5.2f, maxShift = 150f, visibleDeg = 27f;

    [Header("The ship in the sight (Step 109): the plot's top-down picture of a contact, turned so up is away from us")]
    [Tooltip("Under the sight's mask, anchored at its centre. Needs the BoardLinearImage material, as the plot's: the line art comes on black, and the material keys the black out.")]
    [SerializeField] private RawImage shipPicture;
    [Tooltip("The brackets' size, px: a ship that fits them is this long.")]
    [SerializeField] private float bracketPx = 42f;
    [SerializeField] private Color shipInk = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
    private ShipView[] views;
    private NavSystem pictured; // the ship in the picture, if any
    private ShipView pictureView; // the camera that films it
    private int bowTo;          // 90 or 270: its bow points right or left in the sight

    [Header("The lock bar (owner 2026-11-01; was the design's convergence bar, which only showed range), left-anchored")]
    [SerializeField] private RectTransform convBar;
    [SerializeField] private float convLeft = 777f, convWidth = 366f;
    [Tooltip("The periscope group's origin in board x.")]
    [SerializeField] private float parentLeft = 750f;

    [Header("Lamps")]
    [SerializeField] private Image slewLamp;
    [SerializeField] private Graphic slewGlow;

    Color nominal => Palette.nominal;
    Color caution => Palette.caution;
    Color data => Palette.data;
    [SerializeField] private Color railLegend = new Color32(0x8C, 0x97, 0x9F, 0xFF);
    Color lampOffPanel => Palette.lampOffPanel;

    private double slewBrg0, slewX0, magZ0, magX0;
    private Graphic[] bracketInk;
    private Image lockBarInk;
    private bool fitWas, lockedWas;

    bool ScopeDown => Controls.Motion && Controls.Motion.ScopeDown;

    public override bool Click(string action, string arg)
    {
        switch (action)
        {
            case "scope.slewTo": State.SlewTo(); return true;
        }
        return false;
    }

    public override bool Down(string action, string arg, Vector2 board)
    {
        var s = State;
        switch (action)
        {
            case "scope.slew": slewX0 = board.x; slewBrg0 = s.scopeBrg; s.slewing = true; s.slewDx = 0; return true;
            case "scope.mag": magX0 = board.x; magZ0 = s.scopeZoom; return true;
        }
        return false;
    }

    public override bool Drag(string action, string arg, Vector2 board)
    {
        switch (action)
        {
            case "scope.slew": State.Slew(slewBrg0, board.x - slewX0); return true;
            case "scope.mag": State.SetZoom(magZ0, board.x - magX0); return true;
        }
        return false;
    }

    public override bool Up(string action, string arg, Vector2 board)
    {
        if (action != "scope.slew") return false;
        State.EndSlew();
        return true;
    }

    // What the sight tells you to do (owner 2026-11-01).
    static string Hint(ShipState s, ShipState.Track seen, double off, bool fits)
    {
        if (s.weapons.Locked) { var t = s.TrackOf(s.weapons.lockKey); return (t != null ? "LOCKED · " + s.NameOf(t) : "LOCKED") + " · " + Mathf.CeilToInt(s.weapons.lockLeft) + " S"; }
        if (seen == null) return "";
        if (!s.ScopeOn(seen, true)) return off > 0 ? "SLEW RIGHT" : "SLEW LEFT";
        if (fits) return "LOCKING " + ShipState.JsRound(s.weapons.lockFill * 100f) + "%";
        if (s.ScopeFill(seen) >= 1) return "ZOOM OUT";
        return s.scopeZoom >= 8 ? "TOO FAR" : "ZOOM IN";
    }

    public override void Refresh()
    {
        var s = State;
        var m = s.weapons.MountOf(s.weapons.mount) ?? s.weapons.stats.mounts[0];
        var tgt = s.TrackOf(s.weapons.target);
        bool down = ScopeDown;

        if (mountShort) mountShort.text = m.shortName;
        if (aperture) aperture.sprite = s.slewing ? apertureSlewing : apertureIdle;
        if (sight) sight.zoom = (float)s.scopeZoom;

        var off = s.weapons.OffBearing();
        if (offAxis)
        {
            offAxis.text = ShipState.OffLabel(off);
            Paint(offAxis, s.ScopeOnAxis ? nominal : s.ScopeOffDeg <= 12 ? caution : data);
        }
        if (zoomLabel) zoomLabel.text = ShipState.JsToFixed(s.scopeZoom, 1) + "×";
        magPointer.Set((float)((s.scopeZoom - 1) / 7 * 280 - 140));
        if (bearing) bearing.text = ShipState.JsRound(s.scopeBrg).ToString("000");
        if (slewLabel)
        {
            // ▶ and ◀ come from the Noto Sans Symbols 2 fallback (HANDOFF_GAPS row 30, done)
            slewLabel.text = s.slewing ? (s.slewDx >= 0 ? "SLEW ▶" : "◀ SLEW") : "SLEW";
            Paint(slewLabel, s.slewing ? caution : railLegend);
        }

        // the ship in the sight (Step 109, owner 2026-10-29): the heard contact nearest the sight's centre
        ShipState.Track seen = null; double seenOff = 0;
        if (down)
            foreach (var t in s.tracks)
            {
                double o = ((t.bearing - s.scopeBrg + 540) % 360) - 180;
                if (System.Math.Abs(o) >= visibleDeg || (seen != null && System.Math.Abs(o) >= System.Math.Abs(seenOff)) || !s.sensors.Heard(t)) continue;
                seen = t; seenOff = o;
            }
        bool fits = seen != null && s.ScopeOn(seen, down) && s.ScopeFits(seen), locked = s.weapons.Locked;

        // owner 2026-11-01: a box at the sight's centre; the ship fits it at the right zoom, and the lock fills
        if (bracketInk == null) bracketInk = System.Array.ConvertAll(brackets, b => b ? b.GetComponent<Graphic>() : null);
        for (int i = 0; i < brackets.Length; i++)
        {
            if (!brackets[i]) continue;
            brackets[i].gameObject.SetActive(seen != null);
            if (bracketInk[i]) Paint(bracketInk[i], seen != null && s.weapons.LockedOn(seen.key) ? nominal : fits ? caution : data);
        }
        if (targetName) targetName.text = seen != null ? s.NameOf(seen) : "\u2014"; // Step 61: BOGEY until scanned
        if (lockHint)
        {
            lockHint.text = Hint(s, seen, seenOff, fits);
            Paint(lockHint, locked ? nominal : fits ? caution : data);
        }
        if (fits && !fitWas) Controls.LockCue(false);
        if (locked && !lockedWas) Controls.LockCue(true);
        if (!locked && lockedWas) Controls.LockLost();
        fitWas = fits; lockedWas = locked;

        // as big as zoom and range make it. It fills the brackets when the lock can start. Seen from above, always level
        // (owner 2026-10-29): bow to the right if it heads to our right, else to the left.
        if (shipPicture)
        {
            var view = seen != null ? ViewOf(seen.key) : null;
            if (view && !view.Texture) view = null;
            shipPicture.enabled = view;
            pictured = view ? seen.nav : null;
            pictureView = view;
            if (view)
            {
                shipPicture.texture = view.Texture;
                shipPicture.color = shipInk.gamma; // the board canvas linearises vertex colours; pre-encode
                float side = Mathf.Min(2000f, (float)s.ScopeFill(seen) * bracketPx / view.ShipFill);
                shipPicture.rectTransform.sizeDelta = new Vector2(side, side);
                shipPicture.rectTransform.anchoredPosition = new Vector2(Mathf.Clamp((float)seenOff * pxPerDeg, -maxShift, maxShift), 0f);
                double rel = ((seen.worldHdg - seen.bearing) % 360 + 360) % 360; // its heading, clockwise from our line of sight
                bowTo = rel < 180 ? 90 : 270;
            }
        }

        // the lock bar fills as the targeting computer locks on, then runs down as the lock runs out
        if (convBar)
        {
            float w = convWidth * s.weapons.LockBar;
            convBar.sizeDelta = new Vector2(w, convBar.sizeDelta.y);
            convBar.anchoredPosition = new Vector2(convLeft - parentLeft + w / 2f, convBar.anchoredPosition.y);
            if (!lockBarInk) lockBarInk = convBar.GetComponent<Image>();
            if (lockBarInk) Paint(lockBarInk, locked ? nominal : caution);
        }

        // subject and lamp
        var subj = s.ScopeSubject(down);
        bool cls = subj != null && s.Scanned(subj); // owner 2026-11-01: a scan gives the class
        string klass = subj == null || string.IsNullOrEmpty(subj.klass) ? "UNKN" : subj.klass;
        // Step 61 (owner 2026-10-24: the periscope is the targeting computer): a scanned subject shows its hull in the
        // SUBJECT box's own two lines — name and class, then each face and, through an open one, the room behind it
        var hull = subj != null && s.Scanned(subj) && s.ShipOf != null ? s.ShipOf(subj.key) : null;
        if (hull != null)
        {
            if (subjectName) subjectName.text = subj.name + (cls ? " · " + klass : "");
            if (subjectClass) subjectClass.text = HullLine(hull);
        }
        else
        {
            if (subjectName) subjectName.text = subj == null ? "—" : s.NameOf(subj); // BOGEY until scanned
            if (subjectClass) subjectClass.text = subj == null ? "NO SUBJECT ON BEARING" : cls ? klass + " HULL" : "NOT SCANNED";
        }
        Color slewInk = s.ScopeOnAxis ? nominal : caution;
        Paint(slewLamp, slewInk); Paint(slewGlow, slewInk);
    }

    // Owner 2026-10-29: the ship in the sight is always flat. The model turns every frame and Refresh runs once a tick,
    // so the picture turns here, after the model has moved. North-up picture: bow to 090 or 270.
    // Owner 2026-11-09 (it tilted again): the camera that films a wreck turns with it (ShipView.WreckHeading), so the picture
    // is not always north-up. Take the camera's turn off too.
    private void LateUpdate()
    {
        if (pictured) shipPicture.rectTransform.localEulerAngles = new Vector3(0f, 0f, (float)pictured.DrawnHeading - (pictureView ? pictureView.WreckHeading ?? 0f : 0f) - bowTo);
    }

    /// <summary>The way the pictured ship's bow points in the sight, degrees counter-clockwise from up: 90 or 270 when it lies
    /// flat (the playtest reads it). NaN with no ship in the picture.</summary>
    public float PictureBowDeg => pictured ? Mathf.Repeat(shipPicture.rectTransform.localEulerAngles.z - ((float)pictured.DrawnHeading - (pictureView ? pictureView.WreckHeading ?? 0f : 0f)), 360f) : float.NaN;

    // The ShipView watching a contact's model (the plot's), if there is one.
    ShipView ViewOf(string key)
    {
        if (views == null) views = FindObjectsByType<ShipView>(FindObjectsSortMode.None);
        foreach (var v in views)
        {
            var wc = v && v.Target ? v.Target.GetComponentInParent<WorldContact>() : null;
            if (wc && wc.trackKey == key) return v;
        }
        return null;
    }

    static readonly string[] FaceNames = { "BOW", "STBD", "AFT", "PORT" };

    /// <summary>"BOW 100 · STBD ▸ RCT 3 · AFT 50 · PORT 50": armour per face in %, and through an open face (▸, as the
    /// engineering lines use it) the room the next hit would damage and its integrity; on a side, the room at the middle
    /// of the ship, where a lock aims (DAMAGE_README.md). A wreck or a prize says so.</summary>
    public static string HullLine(ShipState ship)
    {
        if (ship.destroyed) return "WRECK";
        if (ship.captured) return "PRIZE — HELD BY OUR MARINES";
        var parts = new string[4];
        for (int i = 0; i < 4; i++)
        {
            float a = ship.damage.SideArmour[i];
            var room = ship.damage.NextRoom(i);
            parts[i] = a > 0f ? FaceNames[i] + " " + ShipState.JsRound(a / ship.damage.stats.sideArmourMax[i] * 100f)
                              : FaceNames[i] + " ▸ " + (room != null ? room.tag + " " + room.integ : "OPEN");
        }
        return string.Join(" · ", parts);
    }
}
