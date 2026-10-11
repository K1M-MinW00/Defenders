using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AppServicesPrefabMigration
{
    private const string PrefabFolderPath = "Assets/Prefabs/System";
    private const string PrefabPath = PrefabFolderPath + "/AppServices.prefab";
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/StartScene.unity",
        "Assets/Scenes/LobbyScene.unity",
        "Assets/Scenes/GameScene.unity"
    };

    [MenuItem("Tools/Bootstrap/Configure App Services Prefab")]
    public static void ConfigureScenes()
    {
        GameObject prefab = CreateOrUpdatePrefab();

        foreach (string scenePath in ScenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (FindServicesRoot(scene) == null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.name = "[App Services]";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("[AppServicesPrefabMigration] App services prefab configured in startup, lobby, and game scenes.");
    }

    private static GameObject CreateOrUpdatePrefab()
    {
        if (!AssetDatabase.IsValidFolder(PrefabFolderPath))
            AssetDatabase.CreateFolder("Assets/Prefabs", "System");

        GameObject root = new GameObject("[App Services]");
        root.AddComponent<GameSettingsManager>();
        root.AddComponent<AuthService>();
        root.AddComponent<UserDataManager>();
        root.AddComponent<AdManager>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    private static GameObject FindServicesRoot(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponent<GameSettingsManager>() != null &&
                root.GetComponent<AuthService>() != null &&
                root.GetComponent<UserDataManager>() != null &&
                root.GetComponent<AdManager>() != null)
            {
                return root;
            }
        }

        return null;
    }
}
