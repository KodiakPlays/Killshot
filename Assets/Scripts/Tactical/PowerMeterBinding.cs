using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the Power panel's bar meters straight from PowerManager.
///
/// It reuses the existing art and shader from the UI prefab — the same
/// UI_GradeDisplay meters the cockpit used. It just does not go through
/// UIController to get there.
///
/// UIController's own power sync sits after two calls in Update that reference
/// UI which the Tactical scenes do not load. Any exception there aborts the
/// rest of Update, so the sync never ran. This is a direct, short path with
/// nothing in between to break.
/// </summary>
public class PowerMeterBinding : MonoBehaviour
{
    [System.Serializable]
    public class Meter
    {
        [Tooltip("engines, arms, bay, support or sig. Leave blank for the reactor.")]
        public string system;
        public Image bar;
        [Tooltip("The lamp above the bar, if there is one.")]
        public Image node;
    }

    [SerializeField] private PowerManager power;
    [SerializeField] private Meter[] meters;
    [SerializeField] private Image reactorBar;

    [Header("Node lamp colours")]
    [SerializeField] private Color nodeDrawing = new Color(0.56f, 0.76f, 0.23f, 1f);
    [SerializeField] private Color nodeVenting = new Color(0.88f, 0.29f, 0.24f, 1f);
    [SerializeField] private Color nodeIdle    = new Color(0.30f, 0.34f, 0.36f, 1f);

    private static readonly int CurId = Shader.PropertyToID("_PowerCur");
    private static readonly int MaxId = Shader.PropertyToID("_PowerMax");

    // Every instanced material has to be destroyed, or each play session leaks
    // six of them.
    private readonly List<Material> owned = new List<Material>();

    private void Awake()
    {
        if (power == null) power = GetComponentInParent<PowerManager>();

        // Each meter needs its own material instance, or they all share one
        // and every bar shows the same value.
        foreach (var m in meters)
            if (m != null && m.bar != null && m.bar.material != null)
            {
                var inst = new Material(m.bar.material);
                owned.Add(inst);
                m.bar.material = inst;
            }

        if (reactorBar != null && reactorBar.material != null)
        {
            var inst = new Material(reactorBar.material);
            owned.Add(inst);
            reactorBar.material = inst;
        }
    }

    private void OnDestroy()
    {
        foreach (var m in owned)
            if (m != null) Destroy(m);
        owned.Clear();
    }

    private void LateUpdate()
    {
        if (power == null) return;

        foreach (var m in meters)
        {
            if (m == null || string.IsNullOrEmpty(m.system)) continue;

            int cur = power.GetSystemPower(m.system);
            int max = power.GetSystemMaxPower(m.system);
            var state = power.GetSystemState(m.system);

            if (m.bar != null && m.bar.material != null)
            {
                m.bar.material.SetFloat(CurId, cur);
                m.bar.material.SetFloat(MaxId, Mathf.Max(1, max));
            }

            if (m.node != null)
                m.node.color = state == PowerState.Draw ? nodeDrawing
                             : state == PowerState.Vent ? nodeVenting
                             : nodeIdle;
        }

        if (reactorBar != null && reactorBar.material != null)
        {
            // Reactor shows what is still free, not what exists — the prototype
            // reads the same way: free bars lit, allocated bars dim.
            int max = power.GetMaxReactorPower();
            reactorBar.material.SetFloat(CurId, power.GetReactorPower());
            reactorBar.material.SetFloat(MaxId, Mathf.Max(1, max));
        }
    }
}
