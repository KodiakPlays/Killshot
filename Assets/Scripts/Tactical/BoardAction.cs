using UnityEngine;

/// <summary>
/// One button on one of the instrument rails.
///
/// Adding a system or a tool means creating one of these assets and dropping it
/// into a rail's list. It never means editing the scene.
///
/// Create with: Assets > Create > Starwolf > Board Action
/// </summary>
[CreateAssetMenu(fileName = "Action", menuName = "Starwolf/Board Action")]
public class BoardAction : ScriptableObject
{
    public enum Kind { Tool, System }
    public enum Panel { None, Drawer }

    [Header("Identity")]
    public string id = "";
    public string label = "";
    public Kind kind = Kind.System;

    [Header("Icon")]
    public Sprite icon;
    [Tooltip("Tools are drawn as white line art and get tinted. System icons " +
             "carry their own colour — the handoff is explicit that they keep it.")]
    public bool tintIcon = false;
    public Color tint = Color.white;

    [Header("What clicking it opens")]
    public Panel panel = Panel.None;

    [Tooltip("Handoff section 5: the targeting computer is 376 x 830.")]
    public Vector2 panelSize = new Vector2(376f, 830f);

    [Tooltip("Gap from the rail's outer edge, and from the top of the screen. " +
             "Handoff: right 104, top 100.")]
    public Vector2 panelOffset = new Vector2(104f, 100f);

    [Tooltip("The screen this panel shows. Leave empty for a blank drawer.")]
    public RenderTexture panelContent;

    [Tooltip("Handoff: it pulls out from the rail rather than popping. 0.42s.")]
    public float slideSeconds = 0.42f;

    public bool OpensAPanel { get { return panel != Panel.None; } }
}
