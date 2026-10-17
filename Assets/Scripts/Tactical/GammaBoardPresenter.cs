using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes the handoff board blend its translucency in gamma space, as the browser board does,
/// inside a Linear project. The board camera draws the board canvas into a linear-format render
/// texture the size of the screen; a RawImage on an overlay canvas shows it through
/// Starwolf/UI/BoardGammaToLinear. The board's sprites are imported with sRGB off, and its colours
/// are written pre-encoded (Color.gamma) because the canvas linearises vertex colours once — so the
/// blend sees the design's sRGB values, and nothing is converted until that last step.
/// Same size as the screen, so the board canvas's raycasts still line up with the mouse.
/// </summary>
[ExecuteAlways]
public class GammaBoardPresenter : MonoBehaviour
{
    [SerializeField] private Camera boardCamera;
    [SerializeField] private Camera presentCamera;
    [SerializeField] private RawImage screen;

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
        if (target != null && target.width == w && target.height == h) return;

        if (target != null) { boardCamera.targetTexture = null; target.Release(); DestroyImmediate(target); }
        target = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        {
            name = "HandoffBoard (gamma)",
            hideFlags = HideFlags.DontSave,
            antiAliasing = 1,
        };
        target.Create();
        boardCamera.targetTexture = target;
        screen.texture = target;
    }
}
