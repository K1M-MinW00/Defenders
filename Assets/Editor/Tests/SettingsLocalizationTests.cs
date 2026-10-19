using NUnit.Framework;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public sealed class SettingsLocalizationTests
{
    private static readonly (string Key, string Korean, string English)[] ExpectedEntries =
    {
        ("settings.title", "설정", "Settings"),
        ("settings.sound", "사운드", "Sound"),
        ("settings.language", "언어", "Language"),
        ("settings.account", "계정", "Account"),
        ("settings.copy", "복사", "Copy"),
        ("settings.link", "연결", "Link"),
        ("settings.user_id_unavailable", "사용자 ID를 확인할 수 없습니다.", "Unable to retrieve the user ID."),
        ("settings.user_id_copied", "사용자 ID를 복사했습니다.", "User ID copied."),
        ("common.tap_to_close", "탭하여 닫으세요.", "Tap to close."),
    };

    [Test]
    public void CommonTable_ContainsKoreanAndEnglishSettingsEntries()
    {
        StringTableCollection collection =
            LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Common);
        Assert.That(collection, Is.Not.Null);

        StringTable korean = collection.GetTable("ko") as StringTable;
        StringTable english = collection.GetTable("en") as StringTable;
        Assert.That(korean, Is.Not.Null);
        Assert.That(english, Is.Not.Null);

        foreach ((string key, string koreanValue, string englishValue) in ExpectedEntries)
        {
            Assert.That(korean.GetEntry(key)?.Value, Is.EqualTo(koreanValue), $"Missing Korean: {key}");
            Assert.That(english.GetEntry(key)?.Value, Is.EqualTo(englishValue), $"Missing English: {key}");
        }
    }

    [Test]
    public void LobbySettingsPanel_HasAllLocalizedTextReferences()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/LobbyScene.unity", OpenSceneMode.Single);
        SettingsPanelView view = Object.FindFirstObjectByType<SettingsPanelView>(FindObjectsInactive.Include);
        Assert.That(view, Is.Not.Null);

        SerializedObject serializedView = new(view);
        string[] properties =
        {
            "titleText",
            "soundLabelText",
            "languageLabelText",
            "accountLabelText",
            "copyUidButtonText",
            "linkAccountButtonText",
            "dismissHintText",
        };

        foreach (string propertyName in properties)
        {
            Object reference = serializedView.FindProperty(propertyName)?.objectReferenceValue;
            Assert.That(reference, Is.Not.Null, $"Missing settings text reference: {propertyName}");
        }
    }

    [TestCase("ko", "설정")]
    [TestCase("en", "Settings")]
    public void LocalizationService_ResolvesSettingsTitleForSelectedLocale(string localeCode, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        string localized = LocalizationService.Get(LocalizationTables.Common, "settings.title");

        Assert.That(localized, Is.EqualTo(expected));
    }
}
