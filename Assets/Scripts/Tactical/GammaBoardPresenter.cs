// =================================================================================================
// GammaBoardPresenter.cs
//
// What it does:
// This makes the battle board's see-through layers mix their colours the way a web browser does.
// It is in the board scene, and in the Ship and Fire Control Codex scenes.
//
// How it works:
// - The board camera draws the whole board into an off-screen picture the size of the screen.
//   The colours are mixed in that picture with the design's values, left unconverted.
// - A second camera shows that picture full screen on a top layer. A board shader converts the
//   colours once, at that last step.
// - It runs in the editor as well as in play mode. Every frame it checks the screen size and the
//   smoothing setting. It makes a new off-screen picture only when one of them changes.
// - The smoothing setting softens the edges of shapes the board draws itself: room rings, bars,
//   plot lines and wedges. 4 is the default. 1 means hard, grainy edges.
// - The picture is the same size as the screen. Mouse clicks on the board line up with what you
//   see.
// - When it is switched off, the board camera gets its normal output back. The picture is freed.
//
// Technical:
// - [ExecuteAlways]. Inspector fields: boardCamera, presentCamera, screen (a RawImage on an
//   overlay canvas) and msaaSamples (4).
// - Refresh() runs in OnEnable and every LateUpdate. It rebuilds the texture only on a new
//   presentCamera size or msaaSamples value.
// - The texture is ARGB32, linear, 24-bit depth, never saved. boardCamera.targetTexture and
//   screen.texture both point at it.
// - The shader is Starwolf/UI/BoardGammaToLinear, in Assets/UI/Shader. Its material is in
//   Assets/UI/Board/Materials.
// - Trap: board sprites are imported with sRGB off. Board colours in code are written with
//   Color.gamma. A new sprite or colour must follow the same rule, or it looks wrong.
// - Five files in Assets/UI/Board have sRGB on and still look right. Three are plain white plot
//   shapes. Two are sector map pictures.
// - No script calls it. PlotGrid has a gammaBoard switch for the same rule.
// =================================================================================================

using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class GammaBoardPresenter : MonoBehaviour
{
    [SerializeField] private Camera boardCamera;
    [SerializeField] private Camera presentCamera;
    [SerializeField] private RawImage screen;
    [Tooltip("Smoothing (MSAA samples) for the shapes the board draws itself — the compartment rings, bars, plot "
           + "lines, wedges. The browser design is smoothed; at 1 they come out hard-edged and grainy (owner 2026-10-24).")]
    [SerializeField] private int msaaSamples = 4;

    private RenderTexture target;

    private void OnEnable() { Refresh(); }

    private void LateUpdate() { Refresh(); }

    private void OnDisable()
    {
        if (boardCamera != null) boardCamera.targetTexture = null;
        if (screen != null) screen.texture = null;
        if (target != null) { target.Release(); DestroyImmediate(target); target = null; }
    }

    private void Refresh()
    {
        if (boardCamera == null || presentCamera == null || screen == null) return;
        int w = Mathf.Max(1, presentCamera.pixelWidth), h = Mathf.Max(1, presentCamera.pixelHeight);
        if (target != null && target.width == w && target.height == h && target.antiAliasing == Mathf.Max(1, msaaSamples)) return;

        if (target != null) { boardCamera.targetTexture = null; target.Release(); DestroyImmediate(target); }
        target = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        {
            name = "Tactical Board (gamma)",
            hideFlags = HideFlags.DontSave,
            antiAliasing = Mathf.Max(1, msaaSamples),
        };
        target.Create();
        boardCamera.allowMSAA = msaaSamples > 1;
        boardCamera.targetTexture = target;
        screen.texture = target;
    }
}
