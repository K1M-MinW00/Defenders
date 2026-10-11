using UnityEditor;
using UnityEngine;

public static class SkillTelegraphPrefabMigration
{
    private const string UnitBasePrefabPath = "Assets/Prefabs/Units/Unit_Base.prefab";
    private const string MaterialFolderPath = "Assets/Art/Materials";
    private const string MaterialPath = MaterialFolderPath + "/SkillTelegraph.mat";

    [MenuItem("Tools/Combat/Configure Skill Telegraph Prefab")]
    public static void ConfigureUnitBase()
    {
        Material material = GetOrCreateMaterial();
        GameObject root = PrefabUtility.LoadPrefabContents(UnitBasePrefabPath);
        UnitController controller = root.GetComponentInChildren<UnitController>(true);
        Transform telegraphTransform = controller.transform.Find("SkillTelegraph");

        if (telegraphTransform == null)
        {
            GameObject telegraphObject = new GameObject("SkillTelegraph");
            telegraphObject.transform.SetParent(controller.transform, false);
            telegraphTransform = telegraphObject.transform;
        }

        LineRenderer line = telegraphTransform.GetComponent<LineRenderer>();
        if (line == null)
            line = telegraphTransform.gameObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.loop = false;
        line.widthMultiplier = 0.055f;
        line.numCapVertices = 3;
        line.numCornerVertices = 2;
        line.textureMode = LineTextureMode.Stretch;
        line.sortingOrder = 600;
        line.sharedMaterial = material;
        line.enabled = false;
        line.positionCount = 0;

        SkillTelegraphView view = telegraphTransform.GetComponent<SkillTelegraphView>();
        if (view == null)
            view = telegraphTransform.gameObject.AddComponent<SkillTelegraphView>();

        SerializedObject serialized = new SerializedObject(view);
        serialized.FindProperty("line").objectReferenceValue = line;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, UnitBasePrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
        Debug.Log("[SkillTelegraphPrefabMigration] Unit base telegraph configured.");
    }

    private static Material GetOrCreateMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material != null)
            return material;

        if (!AssetDatabase.IsValidFolder(MaterialFolderPath))
            AssetDatabase.CreateFolder("Assets/Art", "Materials");

        material = new Material(Shader.Find("Sprites/Default"))
        {
            name = "SkillTelegraph"
        };
        AssetDatabase.CreateAsset(material, MaterialPath);
        return material;
    }
}
