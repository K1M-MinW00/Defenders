using UnityEngine;

[DisallowMultipleComponent]
public sealed class BossAreaTelegraphView : MonoBehaviour
{
    private const int SegmentCount = 64;

    [SerializeField] private Color areaColor = new(1f, 0.08f, 0.04f, 0.16f);
    [SerializeField] private Color progressColor = new(1f, 0.18f, 0.04f, 0.42f);
    [SerializeField] private Color outlineColor = new(1f, 0.3f, 0.08f, 0.9f);

    private Mesh progressMesh;
    private Mesh areaMesh;
    private Material areaMaterial;
    private Material progressMaterial;
    private Material outlineMaterial;
    private LineRenderer outline;
    private float radius;

    public float Progress { get; private set; }

    public static BossAreaTelegraphView Create(Vector3 position, float radius, float duration)
    {
        GameObject instance = new("BossAreaTelegraph");
        instance.transform.position = position;
        BossAreaTelegraphView view = instance.AddComponent<BossAreaTelegraphView>();
        view.Initialize(radius, duration);
        return view;
    }

    public void Initialize(float areaRadius, float duration)
    {
        radius = Mathf.Max(0.1f, areaRadius);
        BuildArea();
        BuildProgress();
        BuildOutline();
        SetProgress(duration <= 0f ? 1f : 0f);
    }

    public void SetProgress(float normalizedProgress)
    {
        Progress = Mathf.Clamp01(normalizedProgress);
        UpdateProgressMesh(Progress);

        if (outline != null)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Progress * Mathf.PI * 10f);
            outline.widthMultiplier = Mathf.Lerp(0.045f, 0.09f, pulse);
            outline.startColor = outline.endColor = Color.Lerp(outlineColor, Color.white, pulse * 0.35f);
        }
    }

    private void BuildArea()
    {
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        areaMesh = CreateSectorMesh(1f);
        filter.sharedMesh = areaMesh;
        areaMaterial = CreateMaterial(areaColor);
        renderer.sharedMaterial = areaMaterial;
        renderer.sortingOrder = 10;
    }

    private void BuildProgress()
    {
        GameObject progressObject = new("Progress");
        progressObject.transform.SetParent(transform, false);
        MeshFilter filter = progressObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = progressObject.AddComponent<MeshRenderer>();
        progressMesh = new Mesh { name = "Boss Telegraph Progress" };
        filter.sharedMesh = progressMesh;
        progressMaterial = CreateMaterial(progressColor);
        renderer.sharedMaterial = progressMaterial;
        renderer.sortingOrder = 11;
    }

    private void BuildOutline()
    {
        GameObject outlineObject = new("Outline");
        outlineObject.transform.SetParent(transform, false);
        outline = outlineObject.AddComponent<LineRenderer>();
        outline.useWorldSpace = false;
        outline.loop = true;
        outline.positionCount = SegmentCount;
        outline.widthMultiplier = 0.06f;
        outline.numCornerVertices = 2;
        outline.numCapVertices = 2;
        outlineMaterial = CreateMaterial(outlineColor);
        outline.sharedMaterial = outlineMaterial;
        outline.sortingOrder = 12;

        for (int index = 0; index < SegmentCount; index++)
        {
            float angle = index / (float)SegmentCount * Mathf.PI * 2f;
            outline.SetPosition(index, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
        }
    }

    private Mesh CreateSectorMesh(float progress)
    {
        Mesh mesh = new() { name = "Boss Telegraph Area" };
        FillSectorMesh(mesh, progress);
        return mesh;
    }

    private void UpdateProgressMesh(float progress)
    {
        if (progressMesh != null)
            FillSectorMesh(progressMesh, progress);
    }

    private void FillSectorMesh(Mesh mesh, float progress)
    {
        int visibleSegments = Mathf.Clamp(Mathf.CeilToInt(SegmentCount * progress), 1, SegmentCount);
        float coveredAngle = Mathf.PI * 2f * Mathf.Clamp01(progress);
        Vector3[] vertices = new Vector3[visibleSegments + 2];
        int[] triangles = new int[visibleSegments * 3];
        vertices[0] = Vector3.zero;

        for (int index = 0; index <= visibleSegments; index++)
        {
            float ratio = index / (float)visibleSegments;
            float angle = Mathf.PI * 0.5f - coveredAngle * ratio;
            vertices[index + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

            if (index >= visibleSegments)
                continue;

            int triangleIndex = index * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = index + 1;
            triangles[triangleIndex + 2] = index + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Sprites/Default");
        Material material = new(shader) { color = color };
        return material;
    }

    private void OnDestroy()
    {
        DestroyRuntimeObject(progressMesh);
        DestroyRuntimeObject(areaMesh);
        DestroyRuntimeObject(areaMaterial);
        DestroyRuntimeObject(progressMaterial);
        DestroyRuntimeObject(outlineMaterial);
    }

    private static void DestroyRuntimeObject(Object target)
    {
        if (target == null)
            return;

        if (Application.isPlaying)
            Destroy(target);
        else
            DestroyImmediate(target);
    }
}
