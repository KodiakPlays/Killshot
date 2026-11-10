// =================================================================================================
// RadarTarget.cs
//
// What it does:
// Marks a ship to show as a dot on the old DRADIS screen of the older physics ship. That screen is
// the Radar script. It is on the EnemyShip1 and EnemyShip2 prefabs.
// The battle board does not use this script.
//
// How it works:
// - In the Inspector you set the dot's picture and colour. You set whether the dot turns with the
//   ship's heading.
// - When the object switches on, it signs up with Radar. When it switches off, it signs off.
//   Radar then removes its dot.
// - The older UIController screen reads the picture and colour when it makes the dot.
//
// Technical:
// - Data: icon (a Sprite), color (white by default) and trackRotation. All three are public and
//   set in the Inspector.
// - OnEnable calls Radar.RegisterTarget(this). OnDisable calls Radar.UnregisterTarget(this).
// - UIController.CreateOrGetRadarBlip reads icon and color once, when it makes the dot. Radar
//   reads trackRotation every frame.
// - Trap: a new icon or color after the dot exists shows nothing new.
// - Trap: with no Radar.Instance at OnEnable, the target is never listed. Switching it off and
//   on again lists it.
// =================================================================================================

using UnityEngine;

public class RadarTarget : MonoBehaviour
{
    public Sprite icon;
    public Color color = Color.white;
    public bool trackRotation = false;

    private void OnEnable()
    {
        Radar.RegisterTarget(this);
    }

    private void OnDisable()
    {
        Radar.UnregisterTarget(this);
    }
}
