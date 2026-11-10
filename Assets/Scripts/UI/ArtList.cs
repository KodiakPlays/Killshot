// =================================================================================================
// ArtList.cs
//
// What it does:
// A list of pictures and fonts for a part of the board with no panel script. The frame, the tool
// rail, the systems rail and the time control each have one. So do the console, the alarm strip
// and the signal box. The menu screens use it too: the Ship, SaveSlots, SectorMap and briefing
// scenes, and the Escape menu. It lets you swap a picture or a font in one place.
//
// How it works:
// - In the Inspector, each picture entry has a name, a picture and every image that shows it.
// - Each font entry has a name, a font, an optional font material and every text that uses it.
// - When you change the list in the Inspector, it puts each picture on all its images. It puts
//   each font on all its texts. Panel does that work, with the same lists the panels use.
// - It runs only in the editor. A built game does not use it.
//
// Technical:
// - Data: art is a list of Panel.ArtSlot. fonts is a list of Panel.FontSlot.
// - OnValidate calls Panel.ApplyArt and Panel.ApplyFonts. That code is inside UNITY_EDITOR.
// - ApplyFonts skips a material made for another font. The text then gets the font's default
//   material.
// - No script calls ArtList. The Art window lists the pictures in its entries.
// - Only images and texts listed in an entry change. Others keep the picture or font they have.
// =================================================================================================

using UnityEngine;

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
