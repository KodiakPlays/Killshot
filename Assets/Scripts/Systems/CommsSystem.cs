using UnityEngine;

/// <summary>
/// Comms: one ship's radio. It holds the band and frequency, the lock on a carrier, the four cipher dials,
/// and the message it can hear. Our ship and every enemy carry one (the enemy is our ship with other stats).
/// Owner 2026-10-26: comms live in one place, here. Rules from board-logic.js (SIGNAL, CIPHER_KEY, MESSAGE,
/// vigenere, tuneOff, cycleChan, startTune, lockSignal, stepDial).
/// </summary>
public class CommsSystem : MonoBehaviour
{
    [Header("Radio — SIGNAL, CIPHER_KEY, MESSAGE and the board's initial state")]
    [Range(1, 3)] public int chan = 1;
    [Tooltip("Doubles, as in the browser: float error moved the ±0.3 lock window at 72.5 (Step 33d verification).")]
    public double freq = 34.0; // 1–100 (Unity's Range slider does not take doubles)
    public bool locked;
    public int[] dials = { 0, 0, 0, 0 };
    [Tooltip("The dial just stepped, highlighted for a moment (activeDial); -1 for none.")]
    public int activeDial = -1;
    public int signalChan = 2;
    public double signalFreq = 72.2;
    [Tooltip("LOCK holds when tuned within this of the carrier (lockSignal).")]
    public double lockWindow = 0.3;
    public int[] cipherKey = { 22, 14, 11, 5 };
    public string message = "HOSTILE FLEET INBOUND VECTOR TWO SEVEN NINE HOLD STATION AWAIT RELIEF";

    const string AZ = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>vigenere: shift each letter by the key in turn; dir +1 enciphers, -1 deciphers.</summary>
    public static string Vigenere(string text, int[] key, int dir)
    {
        var sb = new System.Text.StringBuilder(text.Length);
        int li = 0;
        foreach (char ch in text)
        {
            int i = AZ.IndexOf(ch);
            if (i < 0) { sb.Append(ch); continue; }
            int k = key[li % key.Length];
            li++;
            sb.Append(AZ[((i + dir * k) % 26 + 26) % 26]);
        }
        return sb.ToString();
    }

    public string Ciphertext => Vigenere(message, cipherKey, 1);

    /// <summary>What the decrypt screen shows: the ciphertext, or once locked, deciphered by the dials.</summary>
    public string Plaintext => locked ? Vigenere(Ciphertext, dials, -1) : Ciphertext;

    public bool Solved
    {
        get { for (int i = 0; i < dials.Length; i++) if (dials[i] != cipherKey[i]) return false; return true; }
    }

    /// <summary>tuneOff: distance from the carrier; 99 when on the wrong band.</summary>
    public double TuneOff => chan != signalChan ? 99.0 : System.Math.Abs(freq - signalFreq);

    /// <summary>cycleChan: band 1 → 2 → 3 → 1; drops any lock.</summary>
    public void CycleChan() { chan = chan >= 3 ? 1 : chan + 1; locked = false; }

    /// <summary>startTune: frequency 1–100 in 0.1 steps; any movement drops the lock.</summary>
    public void SetFreq(double f)
    {
        freq = System.Math.Floor(System.Math.Max(1.0, System.Math.Min(100.0, f)) * 10.0 + 0.5) / 10.0;
        locked = false;
    }

    /// <summary>lockSignal: locks only within the window of the carrier.</summary>
    public void LockSignal() { if (TuneOff <= lockWindow) locked = true; }

    /// <summary>stepDial: only while locked; a step forward, or back with shift, through A–Z.</summary>
    public bool StepDial(int i, bool back)
    {
        if (!locked || i < 0 || i >= dials.Length) return false;
        dials[i] = ((dials[i] + (back ? -1 : 1)) % 26 + 26) % 26;
        activeDial = i;
        return true;
    }
}
