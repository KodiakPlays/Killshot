// =================================================================================================
// BogieClass.cs
//
// What it does:
// A small record for one enemy contact in the older ship UI (UIController). It is a list entry
// and is not placed on anything in a scene. The battle board does not use this script.
//
// How it works:
// - It keeps four things: the contact's object, its mesh, an image on the enemy weapon screen, and
//   a material for that screen's shader.
// - It is made with those four things filled in.
// - WeapStart sets the screen's shader to its starting look: no laser, no range ring, no
//   stability flag, and a red range colour.
// - UIController keeps a list of these. It steps through the list to pick the next target. It
//   hides or shows each contact's model when the view zooms. It fires the laser effect on a
//   contact's screen.
// - No other script adds to UIController's list.
//
// Technical:
// - Data: go (the contact's object), mesh, wepImageGo (its image on the enemy weapon screen) and
//   matWep (that image's material).
// - It is a plain class. UIController.bogieList shows the entries in the Inspector.
// - The list is empty in every saved scene and prefab. No code makes a BogieClass.
// - UIController.AddBogie adds one. It stops with an error before it reaches WeapStart.
//   RemoveBogie removes one. Nothing calls either.
// - WeapStart sets _LaserFire, _RangeVisOn, _StabilityBool and _RangeColor on matWep.
// - UIController's enemy laser sets _GunLoc, _LaserDegree, _LaserSize, _LaserStart and _LaserEnd
//   on matWep.
// - Trap: the map's zoom steps expect go to have a MeshRenderer and an icon as its first child.
//   UIController never reads the mesh field.
// =================================================================================================

using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BogieClass
{
    public GameObject go;
    public Mesh mesh;
    public GameObject wepImageGo;
    public Material matWep;

    public BogieClass(GameObject go, Mesh mesh, GameObject wepImageGo, Material matWep)//need more data like stats etc
    {
        this.go = go;
        this.mesh = mesh;
        this.wepImageGo = wepImageGo;
        this.matWep = matWep;
    }

    public void WeapStart()
    {
        matWep.SetInt("_LaserFire", 0);
        matWep.SetInt("_RangeVisOn", 0);
        matWep.SetInt("_StabilityBool", 0);
        matWep.SetVector("_RangeColor", new Vector3(1f, 0f,0f));
    }


}
