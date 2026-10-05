using UnityEngine;

public sealed class SkillTelegraphView : MonoBehaviour
{
    private const int CircleSegments = 40;

    private LineRenderer line;
    private Material runtimeMaterial;
    private Transform followCenter;
    private Transform lineOrigin;
    private Transform lineTarget;
    private Vector3 fixedCenter;
    private Vector3 followOffset;
    private Vector2 fixedDirection;
    private float radius;
    private float lineLength;
    private Color baseColor;
    private bool circleMode;

    public static SkillTelegraphView GetOrCreate(Transform owner)
    {
        SkillTelegraphView existing = owner.GetComponentInChildren<SkillTelegraphView>(true);
        if (existing != null)
            return existing;

        GameObject child = new("SkillTelegraph");
        child.transform.SetParent(owner, false);
        return child.AddComponent<SkillTelegraphView>();
    }

    private void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = false;
        line.widthMultiplier = 0.055f;
        line.numCapVertices = 3;
        line.numCornerVertices = 2;
        line.textureMode = LineTextureMode.Stretch;
        line.sortingOrder = 600;

        Shader shader = Shader.Find("Sprites/Default");
        runtimeMaterial = new Material(shader) { name = "SkillTelegraph_Runtime" };
        line.sharedMaterial = runtimeMaterial;
        Hide();
    }

    private void Update()
    {
        if (line == null || !line.enabled)
            return;

        float alpha = Mathf.Lerp(0.25f, 0.9f, (Mathf.Sin(Time.time * 14f) + 1f) * 0.5f);
        Color color = baseColor;
        color.a *= alpha;
        line.startColor = color;
        line.endColor = color;

        if (circleMode)
            RefreshCircle();
        else
            RefreshLine();
    }

    public void ShowCircle(Vector3 center, float value, Color color)
    {
        followCenter = null;
        fixedCenter = center;
        ShowCircleInternal(value, color);
    }

    public void ShowCircle(Transform target, Vector3 offset, float value, Color color)
    {
        followCenter = target;
        followOffset = offset;
        ShowCircleInternal(value, color);
    }

    public void ShowLine(Transform origin, Vector2 direction, float length, Color color)
    {
        circleMode = false;
        lineOrigin = origin;
        lineTarget = null;
        fixedDirection = direction.normalized;
        lineLength = Mathf.Max(0.1f, length);
        baseColor = color;
        line.positionCount = 2;
        line.loop = false;
        line.widthMultiplier = 0.065f;
        line.enabled = true;
        RefreshLine();
    }

    public void ShowLine(Transform origin, Transform target, float length, Color color)
    {
        lineTarget = target;
        Vector2 direction = target != null
            ? (Vector2)(target.position - origin.position)
            : Vector2.right;
        ShowLine(origin, direction, length, color);
        lineTarget = target;
    }

    public void Hide()
    {
        followCenter = null;
        lineOrigin = null;
        lineTarget = null;
        if (line != null)
        {
            line.enabled = false;
            line.positionCount = 0;
        }
    }

    private void ShowCircleInternal(float value, Color color)
    {
        circleMode = true;
        radius = Mathf.Max(0.05f, value);
        baseColor = color;
        line.positionCount = CircleSegments;
        line.loop = true;
        line.widthMultiplier = Mathf.Clamp(radius * 0.035f, 0.035f, 0.09f);
        line.enabled = true;
        RefreshCircle();
    }

    private void RefreshCircle()
    {
        Vector3 center = followCenter != null
            ? followCenter.position + followOffset
            : fixedCenter;

        for (int i = 0; i < CircleSegments; i++)
        {
            float angle = Mathf.PI * 2f * i / CircleSegments;
            line.SetPosition(i, center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
    }

    private void RefreshLine()
    {
        if (lineOrigin == null)
        {
            Hide();
            return;
        }

        Vector2 direction = lineTarget != null
            ? (Vector2)(lineTarget.position - lineOrigin.position).normalized
            : fixedDirection;
        if (direction.sqrMagnitude <= 0.0001f)
            direction = Vector2.right;

        Vector3 start = lineOrigin.position + Vector3.up * 0.15f;
        line.SetPosition(0, start);
        line.SetPosition(1, start + (Vector3)(direction * lineLength));
    }

    private void OnDisable() => Hide();

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}
