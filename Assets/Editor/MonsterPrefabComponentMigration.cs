using UnityEditor;
using UnityEngine;

public static class MonsterPrefabComponentMigration
{
    private const string MonsterBasePrefabPath = "Assets/Prefabs/Monsters/Monster_Base.prefab";

    [MenuItem("Tools/Combat/Configure Monster Prefab Components")]
    public static void ConfigureMonsterBase()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(MonsterBasePrefabPath);
        MonsterController controller = root.GetComponentInChildren<MonsterController>(true);

        if (controller.GetComponent<MonsterTargetingController>() == null)
            controller.gameObject.AddComponent<MonsterTargetingController>();

        PrefabUtility.SaveAsPrefabAsset(root, MonsterBasePrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
        Debug.Log("[MonsterPrefabComponentMigration] Monster base targeting component configured.");
    }
}
