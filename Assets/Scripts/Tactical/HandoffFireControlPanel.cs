using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 33g. The Fire Control drawer: the SIGHT key and periscope bearing, the weapon system and
/// mount selectors with the mount's readout, the firing solution (TRACK and the RNG/BRG/SPD knobs),
/// the capacitor, the LOAD → ANGLE → BEARING → ARM ladder and the fire lever. Rules and values from
/// board-logic.js (pickSystem/cycleSystem, cycleMount, computeSol, nudgeSol, stepKnob, flip,
/// leverDown and the lever drag, drawWep, the capacitor in the tick; renderVals mountShort,
/// sightLamp, scopeBrgLabel, brgEchoLabel/Ink, sysAngle, sysPositions, mountAngle, mountPositions,
/// mountLabel, mountArc, mountRange, sysState/Ink, solLamp, trackId/Ink, trackSrc, solInk, solRange,
/// solBrg, solSpd, rng/brg/spdAngle, capPct, capCells, capLabel, capInk, capLamp, capNote,
/// interlocks, leverLeft, leverTrack, readyLabel/Ink) and UIStateSpec §8.2 and §11.
/// The PDC (quadrants, LOADERS) and missile (warhead, SALVO) controls were never measured — the
/// drawer was measured on RAILGUN — so they are not drawn (HANDOFF_GAPS); their state still shows.
/// </summary>
public class HandoffFireControlPanel : HandoffPanel
{
    [System.Serializable]
    public class Position { public Image lamp, glow; public TMP_Text label; }

    [System.Serializable]
    public class Rung { public Image rocker, lamp, glow; public TMP_Text hint; }

    [Header("Header")]
    [SerializeField] private TMP_Text mountShort;
    [SerializeField] private Image sightLamp, sightGlow;
    [SerializeField] private TMP_Text scopeBearing, bearingEcho;

    [Header("01 Weapon system")]
    [SerializeField] private HandoffBoardControls.Dial systemSelector, mountSelector;
    [SerializeField] private Position[] systemPositions, mountPositions;
    [SerializeField] private TMP_Text mountLabel, mountArc, mountRange, sysState;

    [Header("02 Firing solution")]
    [SerializeField] private Image solLamp, solGlow;
    [Tooltip("The TRACK (COMPUTE) key; it and its lamp drop while held (handoff README: push-buttons move 1 px).")]
    [SerializeField] private RectTransform trackKey;
    [Tune("Motion")] [SerializeField] private float trackPressPx = 1f;
    [SerializeField] private TMP_Text trackId, trackSrc, solRange, solBrg, solSpd;
    [Tooltip("The '°' run after the REL BRG value: the value now carries its own '°'.")]
    [SerializeField] private TMP_Text solBrgDegree;
    [SerializeField] private HandoffBoardControls.Dial rngKnob, brgKnob, spdKnob;
    [Tooltip("stepKnob pitch, px per step: RNG 8, BRG 6, SPD 8 (§8.2).")]
    [SerializeField] private float rngPitch = 8f, brgPitch = 6f, spdPitch = 8f;

    [Header("03 Capacitor")]
    [SerializeField] private TMP_Text capPct, capLabel, capNote;
    [Tooltip("Ten cells, left (1) to right (10).")]
    [SerializeField] private Image[] capCells;
    [Tooltip("The shot-cost mark (drawn #273), moved to the boundary after cell round(cost/10).")]
    [SerializeField] private RectTransform capMark;
    [SerializeField] private Image capLamp, capGlow;

    [Header("04 Arming sequence and the lever")]
    [SerializeField] private Rung[] ladder;
    [SerializeField] private Sprite rockerUp, rockerLocked, rockerThrown;
    [SerializeField] private TMP_Text readyLabel;
    [SerializeField] private RectTransform leverSlot, leverCap, leverFill;
    [SerializeField] private float capLeftPct = 2f, capTravelPct = 72f, leverTravelPx = 210f, slotMargin = 4f, capMargin = 12f;

    [Header("Colours (design sRGB)")]
    [SerializeField] private Color nominal = new Color32(0x8F, 0xC3, 0x3A, 0xFF);
    [SerializeField] private Color caution = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color alarm = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
    [SerializeField] private Color data = new Color32(0xE8, 0xED, 0xF0, 0xFF);
    [SerializeField] private Color label = new Color32(0xC9, 0xD2, 0xD8, 0xFF);
    [SerializeField] private Color stencil = new Color32(0x77, 0x82, 0x8A, 0xFF);
    [SerializeField] private Color wellStencil = new Color32(0x5B, 0x66, 0x6F, 0xFF);
    [SerializeField] private Color stencilDim = new Color32(0x5E, 0x68, 0x70, 0xFF);
    [SerializeField] private Color lampOffPanel = new Color32(0x22, 0x26, 0x2A, 0xFF);
    [SerializeField] private Color lampOffSelector = new Color32(0x1B, 0x20, 0x24, 0xFF);
    [SerializeField] private Color cautionHot = new Color32(0xFF, 0xC4, 0x4D, 0xFF);
    [Tooltip("tokens.json 'white': lit data-bar cells (no measured lit capacitor cell — HANDOFF_GAPS).")]
    [SerializeField] private Color cellLit = Color.white;
    [SerializeField] private Color cellUnlit = new Color32(0x0E, 0x13, 0x16, 0xFF);

    private float knobX0, leverX0, leverV0;
    private bool leverHeld, shotThisPull, trackHeld;
    private Vector2 trackHome, trackLampHome;
    private float capMarkX0 = float.NaN;

    public override bool Click(string action, string arg)
    {
        var s = State;
        switch (action)
        {
            case "fc.sight": if (Controls.Motion) Controls.Motion.ToggleScope(); return true;
            case "fc.system": s.CycleSystem(); return true;
            case "fc.mount": if (s.sys == "MS") s.CycleWarhead(); else s.CycleMount(); return true; // Step 53: with one launcher, the knob picks the warhead
            case "fc.track": s.ComputeSol(); return true;
            case "fc.flip": s.Flip(int.Parse(arg)); return true;
        }
        return false;
    }

    public override bool Down(string action, string arg, Vector2 board)
    {
        if (action == "fc.track") { PressTrack(true); return true; }
        if (action == "fc.knob") { knobX0 = board.x; return true; }
        if (action == "fc.lever") { leverHeld = true; shotThisPull = false; leverX0 = board.x; leverV0 = State.fireLever; return true; }
        return false;
    }

    // stepKnob: whole steps of `pitch` px as the pointer moves; the origin advances by the steps taken.
    public override bool Drag(string action, string arg, Vector2 board)
    {
        var s = State;
        if (action == "fc.knob")
        {
            float pitch = arg == "brg" ? brgPitch : arg == "spd" ? spdPitch : rngPitch;
            int n = (int)((board.x - knobX0) / pitch); // Math.trunc
            if (n != 0) { knobX0 += n * pitch; s.NudgeSol(arg, n); }
            return true;
        }
        if (action == "fc.lever" && leverHeld)
        {
            s.fireLever = Mathf.Clamp01(leverV0 + (board.x - leverX0) / leverTravelPx);
            if (s.fireLever > s.leverFireAt && s.AllSet && !shotThisPull) { shotThisPull = true; s.Fire(); }
            return true;
        }
        return false;
    }

    public override bool Up(string action, string arg, Vector2 board)
    {
        if (action == "fc.track") { PressTrack(false); return true; }
        if (action != "fc.lever") return false;
        leverHeld = false;
        State.fireLever = 0f; // springs back
        return true;
    }

    // No transition: §9.1 lists none for the TRACK key, so it drops and returns at once.
    void PressTrack(bool down)
    {
        if (down == trackHeld) return;
        trackHeld = down;
        if (down)
        {
            if (trackKey) trackHome = trackKey.anchoredPosition;
            if (solLamp) trackLampHome = solLamp.rectTransform.anchoredPosition;
        }
        var drop = down ? new Vector2(0f, -trackPressPx) : Vector2.zero;
        if (trackKey) trackKey.anchoredPosition = trackHome + drop;
        if (solLamp) solLamp.rectTransform.anchoredPosition = trackLampHome + drop;
    }

    public override void Refresh()
    {
        var s = State;
        var y = s.Sys;
        var m = s.MountOf(s.mount) ?? s.mounts[1];
        var tgt = s.TrackOf(s.target);
        bool scopeDown = Controls.Motion && Controls.Motion.ScopeDown;

        if (mountShort) mountShort.text = m.shortName;
        Paint(sightLamp, scopeDown ? nominal : lampOffPanel); Paint(sightGlow, nominal); Show(sightGlow, scopeDown);
        if (scopeBearing) scopeBearing.text = HandoffBoardState.JsRound(s.scopeBrg).ToString("000");
        var off = s.OffBearing();
        if (bearingEcho)
        {
            bearingEcho.text = s.OnBearing ? "ON BEARING" : "OFF " + HandoffBoardState.OffLabel(off);
            Paint(bearingEcho, s.OnBearing ? nominal : caution);
        }

        // 01 weapon system: sysAngle [60, 90, 120]; mountAngle 90 for one mount, else [60, 120]
        systemSelector.Set(new[] { 60f, 90f, 120f }[s.SysIndex]);
        for (int i = 0; i < systemPositions.Length && i < s.systems.Length; i++)
        {
            var sy = s.systems[i];
            bool on = sy.id == s.sys;
            bool armed = System.Array.IndexOf(sy.mounts, s.mount) >= 0 && s.sw[3];
            Color lamp = armed ? alarm : on ? data : lampOffSelector;
            Paint(systemPositions[i].lamp, lamp); Paint(systemPositions[i].glow, lamp); Show(systemPositions[i].glow, armed || on);
            Paint(systemPositions[i].label, on ? data : stencil);
        }
        int mi = System.Array.IndexOf(y.mounts, s.mount);
        mountSelector.Set(y.mounts.Length == 1 ? 90f : new[] { 60f, 120f }[Mathf.Max(0, mi)]);
        for (int i = 0; i < mountPositions.Length; i++)
        {
            var pos = mountPositions[i];
            bool used = i < y.mounts.Length;
            Show(pos.lamp, used); Show(pos.label, used);
            if (!used) { Show(pos.glow, false); continue; }
            var mm = s.MountOf(y.mounts[i]);
            bool on = y.mounts[i] == s.mount;
            if (pos.label) { pos.label.text = mm != null ? mm.shortName : y.mounts[i]; Paint(pos.label, on ? data : stencil); }
            Paint(pos.lamp, on ? data : lampOffSelector); Paint(pos.glow, data); Show(pos.glow, on);
        }
        if (mountLabel) mountLabel.text = m.label;
        if (mountArc) mountArc.text = m.halfArc >= 180 ? "ALL ROUND" : "ARC " + m.arc.ToString("000") + "° ±" + m.halfArc + "°";
        if (mountRange) mountRange.text = m.rangeKm.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " km";
        if (sysState)
        {
            sysState.text = s.sys == "RG" ? "RAIL " + m.loaded + " · SLUGS " + m.ammo
                : s.sys == "PD" ? m.ammo + " RDS · LOADERS " + (s.loaders ? "ENGAGED" : "OFF")
                : s.warheads[s.warhead] + " · SALVO " + s.salvo + " OF " + s.vlsCells; // cells left
            Paint(sysState, s.sys == "PD" && !s.loaders ? caution : data);
        }

        // 02 firing solution
        var sol = s.Sol;
        string tid = s.TrackIdFor();
        Color solInk = sol.set ? nominal : tid != null ? caution : lampOffSelector;
        Paint(solLamp, solInk); Paint(solGlow, solInk); Show(solGlow, sol.set || tid != null);
        bool manual = sol.set && sol.manual; // Step 47: dialled by hand from the plot
        if (trackId) { trackId.text = manual ? "MANUAL" : tid ?? "NO TRACK"; Paint(trackId, manual || tid != null ? data : stencil); }
        if (trackSrc) trackSrc.text = manual ? "DIALLED BY HAND" : tid != null ? "FROM SENSOR LOCK" : "NOT IDENTIFIED ON SENSORS";
        Color valInk = sol.set ? data : wellStencil;
        if (solRange) { solRange.text = sol.set ? sol.rangeKm.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) : "—"; Paint(solRange, valInk); }
        if (solBrg) { solBrg.text = (sol.set ? (sol.brg > 0 ? "+" : "") + sol.brg : "—") + "°"; Paint(solBrg, valInk); }
        Show(solBrgDegree, false);
        if (solSpd) { solSpd.text = sol.set ? sol.spd.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) : "—"; Paint(solSpd, valInk); }
        rngKnob.Set(sol.set ? (sol.rangeKm / 100f * 12f) % 360f : 0f);
        brgKnob.Set(sol.set ? sol.brg * 6f : 0f);
        spdKnob.Set(sol.set ? (HandoffBoardState.JsRound(sol.spd / 0.05f) * 12) % 360 : 0f);

        // 03 capacitor
        float cost = s.ShotCost;
        int lit = HandoffBoardState.JsRound(s.wepCharge / 100f * 10f), mk = HandoffBoardState.JsRound(cost / 100f * 10f);
        for (int k = 0; k < capCells.Length; k++) Paint(capCells[k], k + 1 <= lit ? cellLit : cellUnlit);
        if (capMark && capCells.Length >= 10)
        {
            // the mark sits on the boundary after cell `mk` (measured after cell 7 for RAILGUN)
            if (float.IsNaN(capMarkX0)) capMarkX0 = capMark.anchoredPosition.x - Right(capCells[6]);
            int at = Mathf.Clamp(mk, 1, 10) - 1;
            capMark.anchoredPosition = new Vector2(Right(capCells[at]) + capMarkX0, capMark.anchoredPosition.y);
        }
        Color capInk = s.wepCharge >= cost ? nominal : s.BusUnits("wep") > 0 ? caution : alarm;
        if (capPct) capPct.text = HandoffBoardState.JsRound(s.wepCharge) + "%"; // measured ink.data, not capInk
        if (capLabel) capLabel.text = cost + "% PER SHOT";
        Color capLampInk = s.wepCharge >= cost ? nominal : s.BusUnits("wep") > 0 ? caution : lampOffPanel;
        Paint(capLamp, capLampInk); Paint(capGlow, capLampInk); Show(capGlow, s.wepCharge >= cost || s.BusUnits("wep") > 0);
        if (capNote)
        {
            capNote.text = s.BusUnits("wep") < 1 ? "WEP BUS DEAD — CAPACITOR BLEEDING OFF"
                : s.wepCharge >= cost ? "CHARGED — GATE WILL ARM" : "CHARGING AT +" + (s.BusUnits("wep") * 5) + "/S — GATE LOCKED";
            Paint(capNote, capInk);
        }

        // 04 arming: interlocks
        for (int i = 0; i < ladder.Length && i < s.sw.Length; i++)
        {
            var r = ladder[i];
            bool on = s.sw[i];
            bool prevOk = i == 0 || s.sw[i - 1];
            bool gated = (i == 1 && !s.InArc) || (i == 2 && !s.OnBearing) || (i == 3 && s.wepCharge < cost)
                         || (i == 0 && (!s.WepReady || !s.Sol.set || (s.sys == "PD" && !s.loaders)));
            bool open = prevOk && !gated;
            if (r.rocker) r.rocker.sprite = on ? rockerThrown : open ? rockerUp : rockerLocked;
            Color lamp = on ? cautionHot : open ? stencilDim : lampOffPanel;
            Paint(r.lamp, lamp); Paint(r.glow, lamp); Show(r.glow, on || open);
            if (r.hint)
            {
                r.hint.text = on ? "SET" : open ? new[] { "ROUND IN RAIL", "ELEV SET", "ON AXIS", "GATE LIVE" }[i]
                    : i == 0 && !s.WeaponsReleased ? "NOT RELEASED"
                    : i == 0 && !s.MagOk ? "MAGAZINE HIT"
                    : i == 0 && !s.WepReady ? "NO WEP BUS"
                    : i == 3 && prevOk && s.wepCharge < cost ? "CHARGING"
                    : i == 1 && prevOk && !s.InArc ? "OUT OF ARC" : "LOCKED"; // Step 48: turn to bring the turret to bear
                Paint(r.hint, on ? caution : open ? label : wellStencil);
            }
        }

        // the lever: leverLeft = (2 + lever x 72)% of the slot; the red track behind it
        if (leverSlot && leverCap)
        {
            float slotLeft = leverSlot.anchoredPosition.x - leverSlot.sizeDelta.x / 2f + slotMargin;
            float slotW = leverSlot.sizeDelta.x - 2f * slotMargin;
            float capLeft = slotLeft + slotW * (capLeftPct + s.fireLever * capTravelPct) / 100f;
            leverCap.anchoredPosition = new Vector2(capLeft - capMargin + leverCap.sizeDelta.x / 2f, leverCap.anchoredPosition.y);
            if (leverFill)
            {
                leverFill.sizeDelta = new Vector2(slotW * s.fireLever, leverFill.sizeDelta.y);
                leverFill.anchoredPosition = new Vector2(slotLeft + leverFill.sizeDelta.x / 2f, leverFill.anchoredPosition.y);
            }
        }

        // readyLabel: the first condition that matches
        if (readyLabel)
        {
            string t;
            if (s.fired) t = s.Rounds.Count > 0 || s.shotReport == null ? "ROUNDS AWAY" : s.shotReport; // Step 48: then HIT · … or MISS
            else if (s.AllSet) t = "SOLUTION LOCKED — MOUNT READY";
            else if (tgt == null && !manual) t = "NO TARGET DESIGNATED";
            else if (tid == null && !manual) t = "NO TRACKING ID — NOT IDENTIFIED";
            else if (!s.BriOk) t = "BRIDGE DAMAGED — PLOT COMPUTER OFFLINE";
            else if (!sol.set) t = "NO FIRING SOLUTION";
            else if (!s.WeaponsReleased) t = "WEAPONS NOT RELEASED — CONDITION 2 REQUIRED";
            else if (!s.MagOk) t = "MAGAZINE DAMAGED — NO ROUNDS STRUCK UP";
            else if (s.sys == "PD" && !s.loaders) t = "PDC AUTOLOADERS OFF";
            else t = "ARMING SEQUENCE INCOMPLETE";
            readyLabel.text = t;
            Paint(readyLabel, s.fired ? alarm : s.AllSet ? data : label);
        }
    }

    static float Right(Graphic g) { var rt = g.rectTransform; return rt.anchoredPosition.x + rt.sizeDelta.x / 2f; }
}
