using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class StatusEffectIconSetTests
{
    [Test]
    public void ResourceIconSet_ContainsAllStatusAndBerserkSprites()
    {
        StatusEffectIconSetSO iconSet = AssetDatabase.LoadAssetAtPath<StatusEffectIconSetSO>(
            "Assets/Resources/GameData/UI/StatusEffectIconSet.asset");

        Assert.That(iconSet, Is.Not.Null);
        Assert.That(iconSet.slow, Is.Not.Null);
        Assert.That(iconSet.stun, Is.Not.Null);
        Assert.That(iconSet.poison, Is.Not.Null);
        Assert.That(iconSet.burn, Is.Not.Null);
        Assert.That(iconSet.defenseDown, Is.Not.Null);
        Assert.That(iconSet.berserk, Is.Not.Null);
    }

    [TestCase("status_slow")]
    [TestCase("status_stun")]
    [TestCase("status_poison")]
    [TestCase("status_burn")]
    [TestCase("status_defense_down")]
    [TestCase("status_berserk")]
    public void GeneratedIcon_UsesSpriteUiImportSettings(string name)
    {
        string path = $"Assets/Art/UI/Generated/StatusEffects/{name}.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        Assert.That(importer, Is.Not.Null);
        Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
        Assert.That(importer.mipmapEnabled, Is.False);
        Assert.That(importer.alphaIsTransparency, Is.True);
    }
}
