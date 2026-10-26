using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 33h-3. The periscope: drag across the aperture to slew, the MAG knob, SLEW (to the target)
/// and CLASS, the off-axis, MAG and bearing readouts, the target brackets, the convergence bar, and
/// the live sight picture. Rules and values from board-logic.js (startSlew, moveSlew, endSlew,
/// magDown, slewTo, classify, scopeOn, scopeSubject, scopeOffDeg, offBearing, drawScope; renderVals
/// mountShort, scopeSubjectName/Class, classifyLamp/Glow, slewLamp/Glow, slewLabel, slewInk,
/// scopeBrgLabel, offBearingLabel, offAxisInk, scopeZoomLabel, magAngle, tgtX, tgtVisible, scopeConv)
/// and UIStateSpec §11 "Periscope".
/// </summary>
public class HandoffPeriscopePanel : HandoffPanel
{
    [SerializeField] private TMP_Text mountShort;
    [SerializeField] private Image aperture;
    [Tooltip("periscope_aperture swap: idle, and the amber rim while slewing.")]
    [SerializeField] private Sprite apertureIdle, apertureSlewing;
    [SerializeField] private HandoffScopeSight sight;

    [Header("Readouts")]
    [SerializeField] private TMP_Text offAxis, zoomLabel, bearing, slewLabel, subjectName, subjectClass;
    [SerializeField] private HandoffBoardControls.Dial magPointer;

    [Header("Target brackets — move with tgtX = centre + clamp(off x 5.2, ±150) px")]
    [SerializeField] private RectTransform[] brackets;
    [Tooltip("The name under the brackets (renderVals targetName).")]
    [SerializeField] private TMP_Text targetName;
    [SerializeField] private float bracketHomeX = 887.2f, sightCentreX = 960f, pxPerDeg = 5.2f, maxShift = 150f, visibleDeg = 27f;

    [Header("Convergence bar (scopeConv), left-anchored")]
    [SerializeField] private RectTransform convBar;
    [SerializeField] private float convLeft = 777f, convWidth = 366f;
    [Tooltip("The periscope group's origin in board x.")]
    [SerializeField] private float parentLeft = 750f;

    [Header("Lamps")]
    [SerializeField] private Image slewLamp, classLamp;
    [SerializeField] private Graphic slewGlow, classGlow;

    [Header("Colours (design sRGB)")]
    [SerializeField] private Color nominal = new Color32(0x8F, 0xC3, 0x3A, 0xFF);
    [SerializeField] private Color caution = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color data = new Color32(0xE8, 0xED, 0xF0, 0xFF);
    [SerializeField] private Color railLegend = new Color32(0x8C, 0x97, 0x9F, 0xFF);
    [SerializeField] private Color lampOffPanel = new Color32(0x22, 0x26, 0x2A, 0xFF);

    private double slewBrg0, slewX0, magZ0, magX0;
    private float[] bracketX;

    bool ScopeDown => Controls.Motion && Controls.Motion.ScopeDown;

    public override bool Click(string action, string arg)
    {
        switch (action)
        {
            case "scope.slewTo": State.SlewTo(); return true;
            case "scope.classify": State.Classify(ScopeDown); return true;
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

    public override void Refresh()
    {
        var s = State;
        var m = s.weapons.MountOf(s.weapons.mount) ?? s.weapons.stats.mounts[1];
        var tgt = s.TrackOf(s.weapons.target);
        bool down = ScopeDown;

        if (mountShort) mountShort.text = m.shortName;
        if (aperture) aperture.sprite = s.slewing ? apertureSlewing : apertureIdle;
        if (sight) sight.zoom = (float)s.scopeZoom;

        var off = s.weapons.OffBearing();
        if (offAxis)
        {
            offAxis.text = HandoffBoardState.OffLabel(off);
            Paint(offAxis, s.ScopeOnAxis ? nominal : s.ScopeOffDeg <= 12 ? caution : data);
        }
        if (zoomLabel) zoomLabel.text = HandoffBoardState.JsToFixed(s.scopeZoom, 1) + "×";
        magPointer.Set((float)((s.scopeZoom - 1) / 7 * 280 - 140));
        if (bearing) bearing.text = HandoffBoardState.JsRound(s.scopeBrg).ToString("000");
        if (slewLabel)
        {
            // ▶ and ◀ come from the Noto Sans Symbols 2 fallback (HANDOFF_GAPS row 30, done)
            slewLabel.text = s.slewing ? (s.slewDx >= 0 ? "SLEW ▶" : "◀ SLEW") : "SLEW";
            Paint(slewLabel, s.slewing ? caution : railLegend);
        }

        // target brackets
        if (bracketX == null || bracketX.Length != brackets.Length)
        {
            bracketX = new float[brackets.Length];
            for (int i = 0; i < brackets.Length; i++) bracketX[i] = brackets[i] ? brackets[i].anchoredPosition.x : 0f;
        }
        if (targetName) targetName.text = tgt != null ? s.NameOf(tgt) : "\u2014"; // Step 61: BOGEY until scanned
        bool visible = off.HasValue && System.Math.Abs(off.Value) < visibleDeg;
        float x = off.HasValue ? sightCentreX + Mathf.Clamp((float)off.Value * pxPerDeg, -maxShift, maxShift) : sightCentreX - 300f;
        for (int i = 0; i < brackets.Length; i++)
        {
            var b = brackets[i];
            if (!b) continue;
            b.gameObject.SetActive(visible);
            b.anchoredPosition = new Vector2(bracketX[i] + (x - bracketHomeX), b.anchoredPosition.y);
        }

        // convergence: 100 - range / mount range, 6..96 %, 0 with no target
        if (convBar)
        {
            float pct = tgt != null ? Mathf.Max(6, Mathf.Min(96, HandoffBoardState.JsRound(100 - (tgt.rangeKm / (double)m.rangeKm) * 100))) : 0;
            float w = convWidth * pct / 100f;
            convBar.sizeDelta = new Vector2(w, convBar.sizeDelta.y);
            convBar.anchoredPosition = new Vector2(convLeft - parentLeft + w / 2f, convBar.anchoredPosition.y);
        }

        // subject and lamps
        var subj = s.ScopeSubject(down);
        bool cls = subj != null && s.classified.Contains(subj.key);
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
            if (subjectName) subjectName.text = subj == null ? "—" : s.NameOf(subj); // classified or not, BOGEY until scanned
            if (subjectClass) subjectClass.text = subj == null ? "NO SUBJECT ON BEARING" : cls ? klass + " HULL" : "HULL UNCLASSIFIED";
        }
        Color classInk = subj == null ? lampOffPanel : cls ? nominal : caution;
        Paint(classLamp, classInk); Paint(classGlow, classInk); Show(classGlow, subj != null);
        Color slewInk = s.ScopeOnAxis ? nominal : caution;
        Paint(slewLamp, slewInk); Paint(slewGlow, slewInk);
    }

    static readonly string[] FaceNames = { "BOW", "STBD", "AFT", "PORT" };

    /// <summary>"BOW 100 · STBD ▸ HGR 3 · AFT 50 · PORT 50": armour per face in %, and through an open face (▸, as the
    /// engineering lines use it) the room behind it and its integrity (DAMAGE_README.md). A wreck or a prize says so.</summary>
    public static string HullLine(HandoffBoardState ship)
    {
        if (ship.destroyed) return "WRECK";
        if (ship.captured) return "PRIZE — HELD BY OUR MARINES";
        var parts = new string[4];
        for (int i = 0; i < 4; i++)
        {
            float a = ship.damage.SideArmour[i];
            var room = ship.damage.Comp(DamageSystem.RoomBehind(i));
            parts[i] = a > 0f ? FaceNames[i] + " " + HandoffBoardState.JsRound(a / ship.damage.stats.sideArmourMax[i] * 100f)
                              : FaceNames[i] + " ▸ " + (room != null ? room.tag + " " + room.integ : "OPEN");
        }
        return string.Join(" · ", parts);
    }
}
