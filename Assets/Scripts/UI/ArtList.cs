using UnityEngine;

/// <summary>
/// The Art list for a board piece with no panel script: the frame, the tool rails, the time
/// control, the console, the alarm strip and the signal box (owner, 2026-10-27). Each entry is a
/// sprite and every image that shows it. The panels have the same list on HandoffPanel.
/// </summary>
public class ArtList : MonoBehaviour
{
    [Tooltip("This piece's art. Swap a sprite here and every image listed under it changes.")]
    [SerializeField] private HandoffPanel.ArtSlot[] art;

#if UNITY_EDITOR
    private void OnValidate() => HandoffPanel.ApplyArt(art);
#endif
}
