using UnityEditor;
using UnityEngine;

public static class StatusEffectIconSetup
{
    private const string SourceDirectory = "Assets/Art/UI/Generated/StatusEffects";
    private const string AssetDirectory = "Assets/Resources/GameData/UI";
    private const string AssetPath = AssetDirectory + "/StatusEffectIconSet.asset";

    [MenuItem("Tools/Game/Bosses/Connect Status Effect Icons")]
    public static void Configure()
    {
        string[] names =
        {
            "status_slow", "status_stun", "status_poison", "status_burn",
            "status_defense_down", "status_berserk",
        };
        foreach (string name in names)
            ConfigureTexture($"{SourceDirectory}/{name}.png");

        EnsureFolder("Assets/Resources", "GameData");
        EnsureFolder("Assets/Resources/GameData", "UI");

        StatusEffectIconSetSO iconSet = AssetDatabase.LoadAssetAtPath<StatusEffectIconSetSO>(AssetPath);
        if (iconSet == null)
        {
            iconSet = ScriptableObject.CreateInstance<StatusEffectIconSetSO>();
            AssetDatabase.CreateAsset(iconSet, AssetPath);
        }

        iconSet.slow = Load("status_slow");
        iconSet.stun = Load("status_stun");
        iconSet.poison = Load("status_poison");
        iconSet.burn = Load("status_burn");
        iconSet.defenseDown = Load("status_defense_down");
        iconSet.berserk = Load("status_berserk");
        EditorUtility.SetDirty(iconSet);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Status effect and berserk icons are imported and connected.");
    }

    private static Sprite Load(string name) =>
        AssetDatabase.LoadAssetAtPath<Sprite>($"{SourceDirectory}/{name}.png");

    private static void ConfigureTexture(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            throw new System.InvalidOperationException($"Texture importer not found: {path}");

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 256;
        importer.SaveAndReimport();
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }
}
