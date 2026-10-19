using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyProfileLocalizationTests
{
    [TestCase(false, 0)]
    [TestCase(true, 500)]
    public void NicknamePresenter_ExposesCurrentChangeCost(bool usedFreeChange, int expectedCost)
    {
        var userData = new UserDataRoot
        {
            Profile = new UserProfileData
            {
                Nickname = "Hero",
                HasUsedFreeNicknameChange = usedFreeChange,
            },
        };
        var presenter = new NicknameEditPresenter(userData, 500);
        presenter.Open();

        NicknameEditState state = presenter.Build("New Hero", false);

        Assert.That(state.ChangeCost, Is.EqualTo(expectedCost));
        Assert.That(state.CanSave, Is.True);
    }

    [TestCase("ko", "무료 변경")]
    [TestCase("en", "Change for Free")]
    public void FreeNicknameChange_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(Localize("nickname.confirm.free"), Is.EqualTo(expected));
    }

    [TestCase("ko", "변경 (보석 500)")]
    [TestCase("en", "Change (500 Gems)")]
    public void PaidNicknameChange_FormatsCost(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(Localize("nickname.confirm.paid", 500), Is.EqualTo(expected));
    }

    [TestCase("ko", "이미 사용 중인 닉네임입니다.")]
    [TestCase("en", "This nickname is already in use.")]
    public void TakenNicknameFailure_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(Localize("nickname.failure.already_taken"), Is.EqualTo(expected));
    }

    [TestCase("ko", "프로필 아이콘 저장에 실패했습니다.")]
    [TestCase("en", "Failed to save the profile icon.")]
    public void ProfileIconSaveFailure_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(Localize("profile.icon.save_failed"), Is.EqualTo(expected));
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
