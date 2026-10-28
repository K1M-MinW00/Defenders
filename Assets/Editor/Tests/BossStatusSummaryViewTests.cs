using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;

public sealed class BossStatusSummaryViewTests
{
    private GameObject root;
    private TextMeshProUGUI styleSource;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Boss Status Summary Test", typeof(RectTransform));
        GameObject textObject = new("Style", typeof(RectTransform));
        textObject.transform.SetParent(root.transform, false);
        styleSource = textObject.AddComponent<TextMeshProUGUI>();
        styleSource.font = TMP_Settings.defaultFontAsset;
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(root);

    [Test]
    public void ArmoredOrc_GroupsSkillWeaknessAndStrongResistance()
    {
        BossStatusSummaryView view = root.AddComponent<BossStatusSummaryView>();
        view.Bind(Load("Boss_Armored_Orc"), styleSource);

        Assert.That(view.SkillCount, Is.EqualTo(2));
        Assert.That(view.WeaknessCount, Is.EqualTo(1));
        Assert.That(view.ResistanceCount, Is.EqualTo(2));
        Assert.That(view.ImmunityCount, Is.Zero);
    }

    [Test]
    public void ArmoredSkeleton_SeparatesImmunityFromResistance()
    {
        BossStatusSummaryView view = root.AddComponent<BossStatusSummaryView>();
        view.Bind(Load("Boss_Armored_Skeleton"), styleSource);

        Assert.That(view.ImmunityCount, Is.EqualTo(1));
        Assert.That(view.ResistanceCount, Is.GreaterThanOrEqualTo(0));
    }

    [TestCase("ko")]
    [TestCase("en")]
    public void EveryBossSummary_FitsCompactInformationColumn(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        string[] guids = AssetDatabase.FindAssets("t:BossDefinitionSO", new[] { "Assets/GameData/Monsters/Bosses/Definitions" });

        foreach (string guid in guids)
        {
            BossDefinitionSO definition = AssetDatabase.LoadAssetAtPath<BossDefinitionSO>(
                AssetDatabase.GUIDToAssetPath(guid));
            BossStatusSummaryView view = root.GetComponent<BossStatusSummaryView>();
            if (view == null)
                view = root.AddComponent<BossStatusSummaryView>();
            view.Bind(definition, styleSource);

            Assert.That(view.PreferredContentWidth, Is.LessThanOrEqualTo(500f),
                $"{definition.name} summary is too wide in {localeCode}.");
        }
    }

    private static BossDefinitionSO Load(string bossId)
    {
        BossDefinitionSO definition = AssetDatabase.LoadAssetAtPath<BossDefinitionSO>(
            $"Assets/GameData/Monsters/Bosses/Definitions/{bossId}_Definition.asset");
        Assert.That(definition, Is.Not.Null);
        return definition;
    }
}
