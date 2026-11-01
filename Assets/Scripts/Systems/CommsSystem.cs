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

    /// <summary>Quiet: nothing on the air. OnAir: a message to tune, lock and decode (the fixed signal is always this).
    /// Read: decoded, and it stays on the screen. Lost: not locked in time.</summary>
    public enum Air { Quiet, OnAir, Read, Lost }

    /// <summary>Owner 2026-10-29: a ship in this level sends real messages. Off: the one fixed signal above.</summary>
    [System.NonSerialized] public bool realSignals;
    [System.NonSerialized] public Air air = Air.OnAir;
    /// <summary>The mission time a message not yet locked goes off the air.</summary>
    [System.NonSerialized] public double offAirAt;
    /// <summary>Messages that have come in (the board plays a tone for each), and messages decoded.</summary>
    [System.NonSerialized] public int signalsIn, messagesRead;

    const string AZ = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    static readonly string[] Compass = { "NORTH", "NORTHEAST", "EAST", "SOUTHEAST", "SOUTH", "SOUTHWEST", "WEST", "NORTHWEST" };
    static readonly string[] Low = { "ZERO", "FIVE", "TEN", "FIFTEEN" };
    static readonly string[] Tens = { "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY" };

    /// <summary>A bearing (0 = north, clockwise) as one of eight direction words.</summary>
    public static string Direction(double deg) => Compass[(int)System.Math.Floor(((deg % 360 + 360) % 360 + 22.5) / 45.0) % 8];

    /// <summary>A speed in words, to the nearest 5. ponytail: stops at NINETY FIVE; add hundreds if a ship ever goes faster.</summary>
    public static string SpeedWords(double speed)
    {
        int n = Mathf.Clamp(Mathf.RoundToInt((float)speed / 5f) * 5, 0, 95);
        return n < 20 ? Low[n / 5] : Tens[n / 10 - 2] + (n % 10 == 5 ? " FIVE" : "");
    }

    /// <summary>Owner 2026-11-01: an intercepted message, so it doesn't speak to us. All in words, so all of it is in
    /// code: GARUDA HEADING SOUTHWEST SPEED FIFTY BEARING NORTHEAST. The bearing is the sender's, from our ship.</summary>
    public static string Report(string sender, double bearingToSender, double heading, double speed) =>
        sender + " HEADING " + Direction(heading) + " SPEED " + SpeedWords(speed) + " BEARING " + Direction(bearingToSender);

    /// <summary>A level's ship sends real messages: the fixed signal goes, and the radio starts quiet.</summary>
    public void UseRealSignals() { realSignals = true; air = Air.Quiet; locked = false; }

    /// <summary>A new message comes in, on a new band and frequency and in a new code. The dials go back to A.
    /// Every part of the key is at least 1: a key of all A would read before a dial is turned.</summary>
    public void Receive(string plain, double until)
    {
        message = plain;
        signalChan = Random.Range(1, 4);
        signalFreq = Random.Range(50, 951) / 10.0;
        for (int i = 0; i < cipherKey.Length; i++) { cipherKey[i] = Random.Range(1, 26); dials[i] = 0; }
        locked = false;
        air = Air.OnAir;
        offAirAt = until;
        signalsIn++;
    }

    /// <summary>A message is on the air and not read yet. If it isn't locked when its time is up, it is lost.</summary>
    public bool Waiting(double now)
    {
        if (realSignals && air == Air.OnAir && !locked && now >= offAirAt) air = Air.Lost;
        return realSignals && air == Air.OnAir;
    }

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
    public string Plaintext => air == Air.Read ? message : air != Air.OnAir ? "" : locked ? Vigenere(Ciphertext, dials, -1) : Ciphertext;

    public bool Solved
    {
        get { for (int i = 0; i < dials.Length; i++) if (dials[i] != cipherKey[i]) return false; return true; }
    }

    /// <summary>tuneOff: distance from the carrier; 99 when on the wrong band.</summary>
    public double TuneOff => air != Air.OnAir || chan != signalChan ? 99.0 : System.Math.Abs(freq - signalFreq);

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
        if (!locked || air == Air.Read || i < 0 || i >= dials.Length) return false;
        dials[i] = ((dials[i] + (back ? -1 : 1)) % 26 + 26) % 26;
        activeDial = i;
        if (realSignals && Solved) { air = Air.Read; messagesRead++; }
        return true;
    }
}
