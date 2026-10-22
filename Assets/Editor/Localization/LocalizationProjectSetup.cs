using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public static class LocalizationProjectSetup
{
    private const string RootDirectory = "Assets/Localization";
    private const string LocalesDirectory = RootDirectory + "/Locales";
    private const string TablesDirectory = RootDirectory + "/Tables";
    private const string SettingsPath = RootDirectory + "/LocalizationSettings.asset";
    private const string KoreanLocalePath = LocalesDirectory + "/Korean (ko).asset";
    private const string EnglishLocalePath = LocalesDirectory + "/English (en).asset";
    private const string LanguagePreferenceKey = "Setting_Language";

    [MenuItem("Tools/Defenders/Localization/Setup Korean and English")]
    public static void Setup()
    {
        EnsureDirectory(RootDirectory);
        EnsureDirectory(LocalesDirectory);
        EnsureDirectory(TablesDirectory);

        LocalizationSettings settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            settings.name = "Defenders Localization Settings";
            AssetDatabase.CreateAsset(settings, SettingsPath);
        }

        LocalizationEditorSettings.ActiveLocalizationSettings = settings;

        Locale korean = GetOrCreateLocale(SystemLanguage.Korean, "한국어", KoreanLocalePath);
        Locale english = GetOrCreateLocale(SystemLanguage.English, "English", EnglishLocalePath);
        AddLocaleIfMissing(korean);
        AddLocaleIfMissing(english);

        ConfigureKoreanFallback(english, korean);
        ConfigureCommonStringTable(korean, english);
        SystemMessagesTableSetup.Configure(korean, english, TablesDirectory);
        LobbyStringTableSetup.Configure(korean, english, TablesDirectory);
        InventoryStringTableSetup.Configure(korean, english, TablesDirectory);
        UnitProgressionStringTableSetup.Configure(korean, english, TablesDirectory);
        StageStringTableSetup.Configure(korean, english, TablesDirectory);
        RelicStringTableSetup.Configure(korean, english, TablesDirectory);
        LabCardStringTableSetup.Configure(korean, english, TablesDirectory);
        TutorialStringTableSetup.Configure(korean, english, TablesDirectory);

        LocalizationSettings.ProjectLocale = korean;
        settings.GetStringDatabase().UseFallback = true;

        EnsureStartupLocaleSelectors(settings);

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[LocalizationProjectSetup] Korean and English locales are configured.");
    }

    private static void EnsureStartupLocaleSelectors(LocalizationSettings settings)
    {
        var selectors = settings.GetStartupLocaleSelectors();
        bool alreadyConfigured =
            selectors.Count == 3 &&
            selectors[0] is PlayerPrefLocaleSelector playerPreference &&
            playerPreference.PlayerPreferenceKey == LanguagePreferenceKey &&
            selectors[1] is SupportedDeviceLocaleSelector &&
            selectors[2] is SpecificLocaleSelector specificLocale &&
            specificLocale.LocaleId == new LocaleIdentifier("en");

        if (alreadyConfigured)
            return;

        selectors.Clear();
        selectors.Add(new PlayerPrefLocaleSelector
        {
            PlayerPreferenceKey = LanguagePreferenceKey,
        });
        selectors.Add(new SupportedDeviceLocaleSelector());
        selectors.Add(new SpecificLocaleSelector
        {
            LocaleId = new LocaleIdentifier("en"),
        });
    }

    private static Locale GetOrCreateLocale(SystemLanguage language, string displayName, string path)
    {
        Locale locale = AssetDatabase.LoadAssetAtPath<Locale>(path);
        if (locale != null)
            return locale;

        locale = Locale.CreateLocale(language);
        locale.name = displayName;
        AssetDatabase.CreateAsset(locale, path);
        return locale;
    }

    private static void AddLocaleIfMissing(Locale locale)
    {
        if (LocalizationEditorSettings.GetLocale(locale.Identifier) == null)
            LocalizationEditorSettings.AddLocale(locale);
    }

    private static void ConfigureKoreanFallback(Locale english, Locale korean)
    {
        FallbackLocale fallback = english.Metadata.GetMetadata<FallbackLocale>();
        if (fallback == null)
        {
            english.Metadata.AddMetadata(new FallbackLocale(korean));
            EditorUtility.SetDirty(english);
            return;
        }

        if (fallback.Locale != korean)
        {
            fallback.Locale = korean;
            EditorUtility.SetDirty(english);
        }
    }

    private static void ConfigureCommonStringTable(Locale korean, Locale english)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Common);
        if (collection == null)
        {
            collection = LocalizationEditorSettings.CreateStringTableCollection(
                LocalizationTables.Common,
                TablesDirectory,
                new List<Locale> { korean, english });
        }

        StringTable koreanTable = GetOrCreateTable(collection, korean);
        StringTable englishTable = GetOrCreateTable(collection, english);

        SetEntry(koreanTable, "settings.title", "설정");
        SetEntry(englishTable, "settings.title", "Settings");
        SetEntry(koreanTable, "settings.sound", "사운드");
        SetEntry(englishTable, "settings.sound", "Sound");
        SetEntry(koreanTable, "settings.language", "언어");
        SetEntry(englishTable, "settings.language", "Language");
        SetEntry(koreanTable, "settings.account", "계정");
        SetEntry(englishTable, "settings.account", "Account");
        SetEntry(koreanTable, "settings.copy", "복사");
        SetEntry(englishTable, "settings.copy", "Copy");
        SetEntry(koreanTable, "settings.link", "연결");
        SetEntry(englishTable, "settings.link", "Link");
        SetEntry(koreanTable, "settings.user_id_unavailable", "사용자 ID를 확인할 수 없습니다.");
        SetEntry(englishTable, "settings.user_id_unavailable", "Unable to retrieve the user ID.");
        SetEntry(koreanTable, "settings.user_id_copied", "사용자 ID를 복사했습니다.");
        SetEntry(englishTable, "settings.user_id_copied", "User ID copied.");
        SetEntry(koreanTable, "settings.account_management", "계정 관리");
        SetEntry(englishTable, "settings.account_management", "Account Management");
        SetEntry(koreanTable, "settings.google_sign_in", "Google로 로그인");
        SetEntry(englishTable, "settings.google_sign_in", "Sign in with Google");
        SetEntry(koreanTable, "settings.apple_sign_in", "Apple로 로그인");
        SetEntry(englishTable, "settings.apple_sign_in", "Sign in with Apple");
        SetEntry(koreanTable, "settings.logout", "로그아웃");
        SetEntry(englishTable, "settings.logout", "Log Out");
        SetEntry(koreanTable, "settings.withdraw", "회원 탈퇴");
        SetEntry(englishTable, "settings.withdraw", "Delete Account");
        SetEntry(koreanTable, "exit.title", "게임 종료");
        SetEntry(englishTable, "exit.title", "Exit Game");
        SetEntry(koreanTable, "exit.message", "게임을 종료하시겠습니까?");
        SetEntry(englishTable, "exit.message", "Are you sure you want to exit the game?");
        SetEntry(koreanTable, "exit.confirm", "종료");
        SetEntry(englishTable, "exit.confirm", "Exit");
        SetEntry(koreanTable, "common.cancel", "취소");
        SetEntry(englishTable, "common.cancel", "Cancel");
        SetEntry(koreanTable, "common.tap_to_close", "탭하여 닫으세요.");
        SetEntry(englishTable, "common.tap_to_close", "Tap to close.");
        SetEntry(koreanTable, "resource.gold", "골드");
        SetEntry(englishTable, "resource.gold", "Gold");
        SetEntry(koreanTable, "resource.gem", "젬");
        SetEntry(englishTable, "resource.gem", "Gems");
        SetEntry(koreanTable, "resource.fuel", "연료");
        SetEntry(englishTable, "resource.fuel", "Fuel");
        SetEntry(koreanTable, "resource.experience", "경험치");
        SetEntry(englishTable, "resource.experience", "Experience");
        SetEntry(koreanTable, "resource.research_material", "연구 재료");
        SetEntry(englishTable, "resource.research_material", "Research Materials");
        SetEntry(koreanTable, "startup.action.start", "시작");
        SetEntry(englishTable, "startup.action.start", "Start");
        SetEntry(koreanTable, "startup.action.retry", "재시도");
        SetEntry(englishTable, "startup.action.retry", "Retry");
        SetEntry(koreanTable, "startup.status.initializing", "초기화 중...");
        SetEntry(englishTable, "startup.status.initializing", "Initializing...");
        SetEntry(koreanTable, "startup.status.checking_login", "로그인 확인 중...");
        SetEntry(englishTable, "startup.status.checking_login", "Checking Login...");
        SetEntry(koreanTable, "startup.status.loading_user_data", "사용자 데이터 불러오는 중...");
        SetEntry(englishTable, "startup.status.loading_user_data", "Loading User Data...");
        SetEntry(koreanTable, "startup.status.ready", "게임 준비 완료");
        SetEntry(englishTable, "startup.status.ready", "Game Ready");
        SetEntry(koreanTable, "startup.status.loading_lobby", "로비 불러오는 중...");
        SetEntry(englishTable, "startup.status.loading_lobby", "Loading Lobby...");
        SetEntry(koreanTable, "startup.error.boot_failed", "게임 시작에 실패했습니다.");
        SetEntry(englishTable, "startup.error.boot_failed", "Failed to start the game.");
        SetEntry(koreanTable, "startup.error.login_failed", "로그인에 실패했습니다.");
        SetEntry(englishTable, "startup.error.login_failed", "Login failed.");
        SetEntry(koreanTable, "startup.error.user_data_load_failed", "사용자 데이터를 불러오지 못했습니다.");
        SetEntry(englishTable, "startup.error.user_data_load_failed", "Failed to load user data.");
        SetEntry(koreanTable, "startup.error.required_service_missing", "필수 서비스를 사용할 수 없습니다.");
        SetEntry(englishTable, "startup.error.required_service_missing", "A required service is unavailable.");
        SetEntry(koreanTable, "startup.error.lobby_scene_missing", "로비 화면을 찾을 수 없습니다.");
        SetEntry(englishTable, "startup.error.lobby_scene_missing", "The Lobby scene is missing.");
        SetEntry(koreanTable, "startup.error.scene_already_loading", "화면을 불러오는 중입니다.");
        SetEntry(englishTable, "startup.error.scene_already_loading", "A scene is already loading.");
        SetEntry(koreanTable, "startup.error.lobby_load_failed", "로비를 불러오지 못했습니다.");
        SetEntry(englishTable, "startup.error.lobby_load_failed", "Failed to load the Lobby.");

        EditorUtility.SetDirty(koreanTable);
        EditorUtility.SetDirty(englishTable);
        EditorUtility.SetDirty(collection.SharedData);
    }

    private static StringTable GetOrCreateTable(StringTableCollection collection, Locale locale)
    {
        return collection.GetTable(locale.Identifier) as StringTable ??
               collection.AddNewTable(locale.Identifier) as StringTable;
    }

    private static void SetEntry(StringTable table, string key, string value)
    {
        StringTableEntry entry = table.GetEntry(key);
        if (entry == null)
            table.AddEntry(key, value);
        else
            entry.Value = value;
    }

    private static void EnsureDirectory(string path)
    {
        if (Directory.Exists(path))
            return;

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureDirectory(parent);

        AssetDatabase.CreateFolder(parent, folderName);
    }
}
