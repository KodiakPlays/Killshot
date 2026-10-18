using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 33d. The Comms drawer: band selector, tuning knob and the 56-bar receiver scope, the LOCK
/// rocker, and the decrypt screen with its four key dials. Rules and values from board-logic.js
/// (cycleChan, startTune, tuneOff, lockSignal, stepDial, vigenere; renderVals chanAngle, chanLamps,
/// freqLabel, freqBearing, tuneAngle, spectrumBars, sigLamp, lockCap/lockLamp, tuneNote, commsState,
/// plainChars, solvedLabel, keyDials) and UIStateSpec §8.2 and §11 "Comms: tune, lock, decrypt".
/// </summary>
public class HandoffCommsPanel : HandoffPanel
{
    [System.Serializable]
    public class KeyDial
    {
        public HandoffBoardControls.Dial pointer;
        public TMP_Text letter;
        public Image box;
    }

    [Header("Header")]
    [SerializeField] private TMP_Text commsState;

    [Header("Band selector")]
    [SerializeField] private HandoffBoardControls.Dial bandPointer;
    [SerializeField] private Image[] bandLamps, bandGlows;
    [SerializeField] private TMP_Text[] bandLabels;

    [Header("Tuning")]
    [SerializeField] private TMP_Text freqLabel;
    [SerializeField] private HandoffBoardControls.Dial tuneIndicator;
    [Tooltip("Marker on the scope ring at the tuned frequency (freqBearing).")]
    [SerializeField] private HandoffBoardControls.Dial ringMarker;
    [Tooltip("Tuning knob / scope centre, board px.")]
    [SerializeField] private Vector2 scopeCentre = new Vector2(1590f, 422.5f);

    [Header("Receiver scope — 56 bars hanging inward from the rim")]
    [SerializeField] private RectTransform[] bars;
    [Tooltip("Rim radius the bars hang from, board px (measured from the bars).")]
    [SerializeField] private float rimRadius = 63f; // the scope glass's radius (glass_round_126)
    [SerializeField] private float barWidth = 2f;
    [Tooltip("Bar length = base + (height / 98) x span (spectrumBars len).")]
    [SerializeField] private float barBase = 7f, barSpan = 27f;

    [Header("Lock")]
    [SerializeField] private Image lockRocker;
    [SerializeField] private Sprite rockerUp, rockerLocked, rockerThrown;
    [SerializeField] private Image lockLamp, lockGlow, sigLamp, sigGlow;
    [SerializeField] private TMP_Text tuneNote;

    [Header("Decrypt")]
    [SerializeField] private TMP_Text solvedLabel;
    [Tooltip("One text per message letter, in order (spaces are gaps in the layout).")]
    [SerializeField] private TMP_Text[] letters;
    [SerializeField] private KeyDial[] keyDials;
    [Tooltip("The dial just stepped stays highlighted this long (stepDial timeout).")]
    [SerializeField] private float activeDialSeconds = 1.1f;

    [Header("Colours (design sRGB)")]
    [SerializeField] private Color nominal = new Color32(0x8F, 0xC3, 0x3A, 0xFF);
    [SerializeField] private Color caution = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color alarm = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
    [SerializeField] private Color ownForce = new Color32(0x4A, 0x9F, 0xD8, 0xFF);
    [SerializeField] private Color stencil = new Color32(0x77, 0x82, 0x8A, 0xFF);
    [SerializeField] private Color data = new Color32(0xE8, 0xED, 0xF0, 0xFF);
    [SerializeField] private Color phosphor = new Color32(0xB6, 0xF0, 0x4A, 0xFF);
    [SerializeField] private Color lampOffSelector = new Color32(0x1B, 0x20, 0x24, 0xFF);
    [SerializeField] private Color lampOffPanel = new Color32(0x22, 0x26, 0x2A, 0xFF);

    private double tuneBase, tuneFreq0;
    private float activeDialUntil;
    [Tooltip("sigPulse: the SIG lamp blinks while own emissions are tracked (§9.2 line 893).")]
    [SerializeField] private float blinkSeconds = 1.1f, blinkLitFraction = 0.52f, blinkLowAlpha = 0.2f;

    public override bool Click(string action, string arg)
    {
        var s = State;
        switch (action)
        {
            case "comms.band": s.CycleChan(); return true;
            case "comms.lock": s.LockSignal(); return true;
            case "comms.dial":
                // shift-click steps back (stepDial)
                bool back = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                s.StepDial(int.Parse(arg), back);
                // the board restarts the highlight timer on every dial click, even a refused one
                activeDialUntil = Time.unscaledTime + activeDialSeconds;
                return true;
        }
        return false;
    }

    public override bool Down(string action, string arg, Vector2 board)
    {
        if (action != "comms.tune") return false;
        tuneBase = System.Math.Atan2(board.y - scopeCentre.y, board.x - scopeCentre.x);
        tuneFreq0 = State.freq;
        return true;
    }

    // startTune: freq = f0 + (turn since press / pi) x 26, the turn wrapped to ±pi.
    public override bool Drag(string action, string arg, Vector2 board)
    {
        if (action != "comms.tune") return false;
        double d = System.Math.Atan2(board.y - scopeCentre.y, board.x - scopeCentre.x) - tuneBase;
        while (d > System.Math.PI) d -= System.Math.PI * 2;
        while (d < -System.Math.PI) d += System.Math.PI * 2;
        State.SetFreq(tuneFreq0 + d / System.Math.PI * 26.0);
        return true;
    }

    private float sigGlowBase = -1f; // the glow's built strength (0.75 of the lamp)

    private void Update()
    {
        if (Controls == null) return;
        if (sigLamp)
        {
            bool tracked = State.signal >= State.signalTrackedAt;
            bool lit = !tracked || (Time.unscaledTime % blinkSeconds) / blinkSeconds < blinkLitFraction;
            var c = sigLamp.color; c.a = lit ? 1f : blinkLowAlpha; sigLamp.color = c;
            // opacity on the lamp element dims its box-shadow glow too
            if (sigGlow) { if (sigGlowBase < 0f) sigGlowBase = sigGlow.color.a; var g = sigGlow.color; g.a = sigGlowBase * (lit ? 1f : blinkLowAlpha); sigGlow.color = g; }
        }
        if (State.activeDial < 0 || Time.unscaledTime < activeDialUntil) return;
        State.activeDial = -1;
        Refresh();
    }

    public override void Refresh()
    {
        var s = State;
        double off = s.TuneOff;
        bool solved = s.Solved;

        if (commsState)
        {
            commsState.text = s.locked ? (solved ? "MESSAGE READ" : "DECIPHERING") : "INCOMING SIGNAL";
            Paint(commsState, s.locked ? (solved ? nominal : caution) : ownForce);
        }

        // band selector: chanAngle = (chan - 2) x 40
        bandPointer.Set((s.chan - 2) * 40f);
        for (int i = 0; i < bandLamps.Length; i++)
        {
            bool on = s.chan == i + 1;
            Paint(bandLamps[i], on ? phosphor : lampOffSelector);
            if (i < bandGlows.Length) { Paint(bandGlows[i], phosphor); Show(bandGlows[i], on); }
            if (i < bandLabels.Length) Paint(bandLabels[i], on ? phosphor : stencil);
        }

        // tuning
        if (freqLabel) freqLabel.text = s.freq.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        tuneIndicator.Set((float)(-150.0 + (s.freq - 1.0) / 99.0 * 300.0)); // tuneAngle
        ringMarker.Set((float)((s.freq - 1.0) / 99.0 * 360.0));             // freqBearing

        // receiver scope: spectrumBars
        for (int k = 0; k < bars.Length; k++)
        {
            // in double, as the browser does (Step 33d verification)
            double f = 1.0 + k / 55.0 * 99.0;
            double hgtD = 8 + (k * 137) % 23;
            if (s.chan == s.signalChan) hgtD += System.Math.Max(0.0, 62.0 - System.Math.Abs(f - s.signalFreq) * 7.0);
            float hgt = (float)hgtD;
            float h = Mathf.Min(98f, hgt);
            float len = barBase + h / 98f * barSpan;
            float rot = k / 56f * 360f;
            float a = rot * Mathf.Deg2Rad;
            // hangs inward from the rim; CSS rotate(0) points down the screen
            Vector2 dir = new Vector2(-Mathf.Sin(a), Mathf.Cos(a)); // board px, y down
            Vector2 c = scopeCentre + dir * (rimRadius - len / 2f);
            var bar = bars[k];
            if (!bar) continue;
            bar.sizeDelta = new Vector2(barWidth, len);
            bar.anchoredPosition = new Vector2(c.x - ParentOrigin(bar).x, -(c.y - ParentOrigin(bar).y));
            bar.localEulerAngles = new Vector3(0, 0, -rot);
            bool near = System.Math.Abs(f - s.freq) < 2.0;
            var img = bar.GetComponent<Image>();
            if (img)
            {
                Color ink = near ? phosphor : nominal;
                float alpha = near ? 1f : hgtD > 46.0 ? 0.6f : 0.26f;
                var g = ink.gamma; g.a = alpha; img.color = g;
            }
        }

        // lock
        if (lockRocker) lockRocker.sprite = s.locked ? rockerThrown : off <= s.lockWindow ? rockerUp : rockerLocked;
        Color lockInk = s.locked ? alarm : off <= s.lockWindow ? caution : lampOffPanel;
        Paint(lockLamp, lockInk); Paint(lockGlow, lockInk); Show(lockGlow, s.locked || off <= s.lockWindow);
        Color sigInk = off <= s.lockWindow ? nominal : off < 12.0 ? caution : lampOffPanel;
        Paint(sigLamp, sigInk); Paint(sigGlow, sigInk); Show(sigGlow, off < 12.0);
        if (tuneNote)
        {
            tuneNote.text = s.locked ? "CARRIER LOCKED — TRAFFIC IS ENCIPHERED"
                : s.chan != s.signalChan ? "NO CARRIER ON THIS BAND" : off <= s.lockWindow ? "CARRIER FOUND" : "SEEKING";
            Paint(tuneNote, s.locked ? nominal : off <= s.lockWindow ? caution : stencil);
        }

        // decrypt
        if (solvedLabel)
        {
            solvedLabel.text = s.locked ? (solved ? "PLAIN" : "CIPHER") : "NO CARRIER";
            Paint(solvedLabel, s.locked && solved ? nominal : stencil);
        }
        string txt = s.Plaintext.Replace(" ", "");
        for (int i = 0; i < letters.Length && i < txt.Length; i++)
        {
            var t = letters[i];
            if (!t) continue;
            t.text = txt[i].ToString();
            bool live = s.activeDial >= 0 && i % s.cipherKey.Length == s.activeDial;
            var c = (live ? alarm : data).gamma;
            c.a = live || s.locked ? 1f : 0.42f;
            t.color = c;
        }
        for (int i = 0; i < keyDials.Length && i < s.dials.Length; i++)
        {
            var kd = keyDials[i];
            int d = s.dials[i];
            kd.pointer.Set(d * (360f / 26f));
            if (kd.letter)
            {
                kd.letter.text = ((char)('A' + d)).ToString();
                var c = (s.activeDial == i ? alarm : s.locked ? phosphor : nominal).gamma;
                c.a = s.activeDial == i || s.locked ? 1f : 0.42f;
                kd.letter.color = c;
            }
            if (kd.box)
            {
                var c = (s.locked ? data : Color.black).gamma;
                c.a = s.locked ? 0.05f : 0.4f;
                kd.box.color = c;
            }
        }
    }

    // Groups are full-board rects anchored top-left, so a child's parent origin is (0,0) in board px.
    static Vector2 ParentOrigin(RectTransform rt) => Vector2.zero;
}
