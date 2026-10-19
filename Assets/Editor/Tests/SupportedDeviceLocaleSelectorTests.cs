using NUnit.Framework;
using UnityEngine;

public sealed class SupportedDeviceLocaleSelectorTests
{
    [Test]
    public void KoreanDevice_SelectsKorean()
    {
        Assert.That(SupportedDeviceLocaleSelector.GetLanguageCode(SystemLanguage.Korean), Is.EqualTo("ko"));
    }

    [TestCase(SystemLanguage.English)]
    [TestCase(SystemLanguage.Japanese)]
    [TestCase(SystemLanguage.Unknown)]
    public void NonKoreanDevice_SelectsEnglish(SystemLanguage language)
    {
        Assert.That(SupportedDeviceLocaleSelector.GetLanguageCode(language), Is.EqualTo("en"));
    }
}
