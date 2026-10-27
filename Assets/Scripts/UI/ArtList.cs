using UnityEngine;

/// <summary>
/// The Art list for a board piece with no panel script: the frame, the tool rails, the time
/// control, the console, the alarm strip and the signal box (owner, 2026-10-27). Each entry is a
/// sprite and every image that shows it. The panels have the same list on Panel.
/// </summary>
public class ArtList : MonoBehaviour
{
    [Tooltip("This piece's art. Swap a sprite here and every image listed under it changes.")]
    [SerializeField] private Panel.ArtSlot[] art;
    [Tooltip("This piece's fonts. Swap a font here and every text listed under it changes.")]
    [SerializeField] private Panel.FontSlot[] fonts;

#if UNITY_EDITOR
    private void OnValidate() { Panel.ApplyArt(art); Panel.ApplyFonts(fonts); }
#endif
}
