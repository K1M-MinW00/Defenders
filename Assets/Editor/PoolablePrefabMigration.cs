using UnityEditor;
using UnityEngine;

public static class PoolablePrefabMigration
{
    [MenuItem("Tools/Pooling/Configure Poolable Prefabs")]
    public static void ConfigurePrefabs()
    {
        int modifiedPrefabCount = 0;
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });

        foreach (string prefabGuid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool modified = false;

            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is not IPoolable || behaviour.GetComponent<Poolable>() != null)
                    continue;

                behaviour.gameObject.AddComponent<Poolable>();
                modified = true;
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                modifiedPrefabCount++;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[PoolablePrefabMigration] Configured {modifiedPrefabCount} pooled prefabs.");
    }
}
