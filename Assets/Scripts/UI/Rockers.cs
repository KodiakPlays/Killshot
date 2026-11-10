// =================================================================================================
// Rockers.cs
//
// What it does:
// Makes the rocker switches on the battle board slide smoothly between up and down. It is on the
// board's top object in the battle scene, next to BoardManager. Each rocker's picture shows a
// fixed state. This script covers the jump with a short slide.
//
// How it works:
// - The rocker groups and the rockers are set in the Inspector. Each group has a slide time and
//   its thrown and locked pictures. It sets the pad's spot in the picture and how far the pad
//   moves.
// - The panels only swap a rocker's picture. Every frame, after the panels, this script checks
//   each rocker for a new picture.
// - On a change it shows a cover over the pad. Then it slides the old pad and the new pad from the
//   old spot to the new one. The new pad fades in. At the end the cover and both pads hide again.
// - A change while the rocker is hidden (its drawer shut) is only noted. No slide plays.
// - Before BoardManager acts on a click, it asks this script. A rocker that shows its locked
//   picture dips a little and springs back. BoardManager plays the refused sound. It still passes
//   the click on to the panels.
// - It runs on real time and keeps moving while the game is paused. The slide and dip times are in
//   the Tuning window, Board tab, Motion.
//
// Technical:
// - Data: families have a slide time, the pad's cut-out (image px), travel (px), and the thrown
//   and locked pictures.
// - rockers each have an image, a cover, two pad images, a family number, and an action and arg.
//   The action and arg match the rocker's ClickSpot.
// - BoardManager finds it on the same object. It calls Refuse(action, arg) before each click.
// - LateUpdate watches each rocker's picture. It runs after the panels have swapped pictures that
//   frame.
// - At rest the cover and both pads are off. The board then shows the plain picture.
// - Numbers: each family's slide time and the dip size and time are in the "Motion" [Tune]
//   section. Pad cut-outs and travel are in the Inspector.
// - Trap: the cut-out takes the whole texture of the rocker's picture. Pictures packed in an atlas
//   would cut wrong.
// - Moving(action, arg) says if a pad moves now. PlaytestRunner calls it.
// =================================================================================================

using UnityEngine;
using UnityEngine.UI;

public class Rockers : MonoBehaviour
{
    [System.Serializable]
    public class Family
    {
        public string name;
        [Tooltip("How long the pad takes to slide (UIStateSpec §9.1).")]
        [Tune("Motion")] public float seconds = 0.14f;
        [Tooltip("The pad's cut-out at its up position: image px from the top-left, margin included; wide enough for the thrown pad's glow.")]
        public Rect pad;
        [Tooltip("How much lower the thrown pad sits in the art, px.")]
        public float travel;
        public Sprite thrown, locked;
    }

    [System.Serializable]
    public class Rocker
    {
        public Image image, cover;
        public RawImage oldPad, newPad;
        public int family;
        public string action, arg;
        [System.NonSerialized] public Sprite shown;
        [System.NonSerialized] public float from, to, y, slideEnd, nudgeEnd;
        [System.NonSerialized] public bool on;
    }

    [SerializeField] private Family[] families = new Family[0];
    [SerializeField] private Rocker[] rockers = new Rocker[0];
    [SerializeField] private CssEase slideEase = new CssEase(0.3f, 1.5f, 0.5f, 1f);
    [SerializeField] private CssEase fadeEase = CssEase.Ease;
    [Tooltip("A locked rocker that is clicked dips this far toward thrown and springs back (owner addition).")]
    [Tune("Motion")] [SerializeField] private float nudgePx = 2f;
    [Tune("Motion")] [SerializeField] private float nudgeSeconds = 0.12f;

    /// <summary>Whether this rocker's pad is sliding or nudging right now.</summary>
    public bool Moving(string action, string arg) { var r = Find(action, arg); return r != null && r.on; }

    /// <summary>Call before a click is handled. If the rocker shows its locked sprite, the click is
    /// refused: the pad nudges and this returns true.</summary>
    public bool Refuse(string action, string arg)
    {
        var r = Find(action, arg);
        if (r == null || !r.image.isActiveAndEnabled) return false;
        var f = families[r.family];
        if (r.image.sprite != f.locked) return false;
        Cut(r, r.oldPad, f.locked); Cut(r, r.newPad, f.locked);
        r.from = r.to = f.pad.y;
        r.nudgeEnd = Time.unscaledTime + nudgeSeconds;
        return true;
    }

    // After every Update and pointer event this frame, so a sprite swapped this frame slides this frame.
    private void LateUpdate()
    {
        float now = Time.unscaledTime; // moves while paused, like the other control transitions
        foreach (var r in rockers)
        {
            if (!r.image) continue;
            var f = families[r.family];
            var s = r.image.sprite;
            if (s != r.shown)
            {
                // a change while the drawer is shut is only recorded, so reopening it doesn't animate
                if (r.shown && r.image.isActiveAndEnabled)
                {
                    r.from = now < r.slideEnd ? r.y : Top(f, r.shown);
                    Cut(r, r.oldPad, r.shown); Cut(r, r.newPad, s);
                    r.slideEnd = now + f.seconds;
                    r.nudgeEnd = 0f; // a slide replaces a nudge
                }
                r.shown = s;
                r.to = Top(f, s);
            }
            bool on = r.image.isActiveAndEnabled && (now < r.slideEnd || now < r.nudgeEnd);
            if (on != r.on) { r.on = on; r.cover.enabled = r.oldPad.enabled = r.newPad.enabled = on; }
            if (!on) continue;
            float k = 1f - Mathf.Max(0f, r.slideEnd - now) / Mathf.Max(f.seconds, 1e-4f);
            float n = 1f - Mathf.Max(0f, r.nudgeEnd - now) / Mathf.Max(nudgeSeconds, 1e-4f);
            r.y = Mathf.LerpUnclamped(r.from, r.to, slideEase.Evaluate(k)) + nudgePx * Mathf.Sin(Mathf.PI * n);
            r.oldPad.rectTransform.anchoredPosition = r.newPad.rectTransform.anchoredPosition = new Vector2(f.pad.x, -r.y);
            var c = r.newPad.color; c.a = fadeEase.Evaluate(k); r.newPad.color = c;
        }
    }

    float Top(Family f, Sprite s) => f.pad.y + (s == f.thrown ? f.travel : 0f);

    // ponytail: assumes each rocker sprite is its whole texture (single, not atlased) and the Image
    // rect is the whole image; use sprite.textureRect if they are ever packed
    void Cut(Rocker r, RawImage pad, Sprite s)
    {
        var f = families[r.family];
        var z = r.image.rectTransform.rect.size;
        float t = Top(f, s);
        pad.texture = s.texture;
        pad.uvRect = new Rect(f.pad.x / z.x, 1f - (t + f.pad.height) / z.y, f.pad.width / z.x, f.pad.height / z.y);
    }

    Rocker Find(string action, string arg)
    {
        foreach (var r in rockers) if (r.action == action && (r.arg ?? "") == (arg ?? "")) return r;
        return null;
    }
}
