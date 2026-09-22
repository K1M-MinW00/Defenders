using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SafeAreaFitter : MonoBehaviour
{
    [SerializeField] private RectTransform[] targets;
    [SerializeField] private bool applyHorizontal = true;
    [SerializeField] private bool applyVertical = true;

    private TargetLayout[] layouts;
    private Rect appliedSafeArea = new(-1f, -1f, -1f, -1f);
    private Vector2Int appliedScreenSize = new(-1, -1);

    private void Awake()
    {
        CaptureLayouts();
    }

    private void OnEnable()
    {
        Apply(force: true);
    }

    private void Update()
    {
        Apply(force: false);
    }

    private void CaptureLayouts()
    {
        if (targets == null)
        {
            layouts = Array.Empty<TargetLayout>();
            return;
        }

        layouts = new TargetLayout[targets.Length];

        for (int i = 0; i < targets.Length; i++)
            layouts[i] = new TargetLayout(targets[i]);
    }

    private void Apply(bool force)
    {
        if (layouts == null)
            CaptureLayouts();

        Rect safeArea = Screen.safeArea;
        Vector2Int screenSize = new(Screen.width, Screen.height);

        if (!force && safeArea == appliedSafeArea && screenSize == appliedScreenSize)
            return;

        if (screenSize.x <= 0 || screenSize.y <= 0)
            return;

        Vector2 safeMin = safeArea.position;
        Vector2 safeMax = safeArea.position + safeArea.size;
        safeMin.x /= screenSize.x;
        safeMin.y /= screenSize.y;
        safeMax.x /= screenSize.x;
        safeMax.y /= screenSize.y;

        if (!applyHorizontal)
        {
            safeMin.x = 0f;
            safeMax.x = 1f;
        }

        if (!applyVertical)
        {
            safeMin.y = 0f;
            safeMax.y = 1f;
        }

        Vector2 safeSize = safeMax - safeMin;

        foreach (TargetLayout layout in layouts)
            layout.Apply(safeMin, safeSize);

        appliedSafeArea = safeArea;
        appliedScreenSize = screenSize;
    }

    [Serializable]
    private sealed class TargetLayout
    {
        private readonly RectTransform target;
        private readonly Vector2 anchorMin;
        private readonly Vector2 anchorMax;
        private readonly Vector2 offsetMin;
        private readonly Vector2 offsetMax;

        public TargetLayout(RectTransform target)
        {
            this.target = target;
            anchorMin = default;
            anchorMax = default;
            offsetMin = default;
            offsetMax = default;

            if (target == null)
                return;

            anchorMin = target.anchorMin;
            anchorMax = target.anchorMax;
            offsetMin = target.offsetMin;
            offsetMax = target.offsetMax;
        }

        public void Apply(Vector2 safeMin, Vector2 safeSize)
        {
            if (target == null)
                return;

            target.anchorMin = safeMin + Vector2.Scale(anchorMin, safeSize);
            target.anchorMax = safeMin + Vector2.Scale(anchorMax, safeSize);
            target.offsetMin = offsetMin;
            target.offsetMax = offsetMax;
        }
    }
}
