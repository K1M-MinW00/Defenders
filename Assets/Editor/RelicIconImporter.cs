using UnityEditor;

public sealed class RelicIconImporter : AssetPostprocessor
{
    private const string RelicIconFolder = "Assets/Resources/UI/Relics/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(RelicIconFolder, System.StringComparison.Ordinal))
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
        importer.filterMode = UnityEngine.FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
    }
}
