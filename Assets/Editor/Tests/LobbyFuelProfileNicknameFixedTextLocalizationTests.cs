using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyFuelProfileNicknameFixedTextLocalizationTests
{
    [TestCase("ko", "fuel.purchase.title", "연료 구매")]
    [TestCase("en", "fuel.purchase.title", "Purchase Fuel")]
    [TestCase("ko", "fuel.purchase.ad", "<sprite=4>광고")]
    [TestCase("en", "fuel.purchase.ad", "<sprite=4>Ad")]
    [TestCase("ko", "profile.title", "프로필")]
    [TestCase("en", "profile.title", "Profile")]
    [TestCase("ko", "profile.cancel", "취소")]
    [TestCase("en", "profile.cancel", "Cancel")]
    [TestCase("ko", "profile.confirm", "확인")]
    [TestCase("en", "profile.confirm", "Confirm")]
    [TestCase("ko", "nickname.title", "닉네임")]
    [TestCase("en", "nickname.title", "Nickname")]
    [TestCase("ko", "nickname.rule", "2~12자 (공백, 특수문자, 비속어 불가)")]
    [TestCase("en", "nickname.rule", "2–12 characters (no spaces, symbols, or profanity)")]
    [TestCase("ko", "nickname.placeholder", "이름을 입력하세요")]
    [TestCase("en", "nickname.placeholder", "Enter a nickname")]
    [TestCase("ko", "nickname.cancel", "취소")]
    [TestCase("en", "nickname.cancel", "Cancel")]
    public void FixedText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
