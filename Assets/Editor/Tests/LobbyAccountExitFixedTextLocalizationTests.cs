using NUnit.Framework;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

public sealed class LobbyAccountExitFixedTextLocalizationTests
{
    [TestCase("ko", "settings.account_management", "계정 관리")]
    [TestCase("en", "settings.account_management", "Account Management")]
    [TestCase("ko", "settings.google_sign_in", "Google로 로그인")]
    [TestCase("en", "settings.google_sign_in", "Sign in with Google")]
    [TestCase("ko", "settings.apple_sign_in", "Apple로 로그인")]
    [TestCase("en", "settings.apple_sign_in", "Sign in with Apple")]
    [TestCase("ko", "settings.logout", "로그아웃")]
    [TestCase("en", "settings.logout", "Log Out")]
    [TestCase("ko", "settings.withdraw", "회원 탈퇴")]
    [TestCase("en", "settings.withdraw", "Delete Account")]
    [TestCase("ko", "exit.title", "게임 종료")]
    [TestCase("en", "exit.title", "Exit Game")]
    [TestCase("ko", "exit.message", "게임을 종료하시겠습니까?")]
    [TestCase("en", "exit.message", "Are you sure you want to exit the game?")]
    [TestCase("ko", "exit.confirm", "종료")]
    [TestCase("en", "exit.confirm", "Exit")]
    [TestCase("ko", "common.cancel", "취소")]
    [TestCase("en", "common.cancel", "Cancel")]
    public void FixedText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Common);
        StringTable table = collection?.GetTable(localeCode) as StringTable;
        Assert.That(table?.GetEntry(key)?.Value, Is.EqualTo(expected));
    }
}
