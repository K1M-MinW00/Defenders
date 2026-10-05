#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class UnitSpawnPresentationPrefabBuilder
{
    private const string PrefabPath = "Assets/Prefabs/Units/Unit_Base.prefab";
    private const int RingSegments = 40;

    [MenuItem("Tools/Defenders/Build Unit Spawn Presentation")]
    public static void Build()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            DropSpawnView view = root.GetComponentInChildren<DropSpawnView>(true);
            if (view == null)
                throw new System.InvalidOperationException("Unit_Base has no DropSpawnView.");

            Transform presentationRoot = view.transform;
            Transform existing = presentationRoot.Find("LandingRing");
            GameObject ringObject = existing != null
                ? existing.gameObject
                : new GameObject("LandingRing");
            ringObject.transform.SetParent(presentationRoot, false);
            ringObject.transform.localPosition = new Vector3(0f, -0.12f, 0f);
            ringObject.layer = presentationRoot.gameObject.layer;

            LineRenderer ring = ringObject.GetComponent<LineRenderer>();
            if (ring == null)
                ring = ringObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = RingSegments;
            ring.widthMultiplier = 0.055f;
            ring.numCornerVertices = 2;
            ring.numCapVertices = 2;
            ring.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");
            ring.startColor = new Color(0.28f, 1f, 0.88f, 0.72f);
            ring.endColor = ring.startColor;
            ring.sortingOrder = 3;

            for (int i = 0; i < RingSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / RingSegments;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.58f, Mathf.Sin(angle) * 0.3f, 0f));
            }

            ringObject.SetActive(false);

            SerializedObject serializedView = new(view);
            serializedView.FindProperty("landingRing").objectReferenceValue = ring;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log("[UnitSpawnPresentationPrefabBuilder] Unit_Base spawn presentation updated.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
#endif
