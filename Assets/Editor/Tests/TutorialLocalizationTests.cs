using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

public sealed class TutorialLocalizationTests
{
    [Test]
    public void AllStringTables_HaveNoMissingOrEmptyLocaleEntries()
    {
        List<string> gaps = new();
        int checkedCount = 0;
        foreach (StringTableCollection collection in LocalizationEditorSettings.GetStringTableCollections())
        {
            foreach (SharedTableData.SharedTableEntry shared in collection.SharedData.Entries)
            {
                foreach (StringTable table in collection.StringTables)
                {
                    checkedCount++;
                    StringTableEntry entry = table.GetEntry(shared.Id);
                    if (entry == null || string.IsNullOrWhiteSpace(entry.Value))
                        gaps.Add($"{collection.TableCollectionName}/{table.LocaleIdentifier.Code}/{shared.Key}");
                }
            }
        }

        Assert.That(checkedCount, Is.GreaterThan(0), "No localization entries were examined.");
        Assert.That(gaps, Is.Empty, string.Join("\n", gaps));
    }

    [Test]
    public void TutorialTable_HasCompleteKoreanAndEnglishEntries()
    {
        StringTableCollection collection =
            LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Tutorial);
        Assert.That(collection, Is.Not.Null);
        Assert.That(collection.SharedData.Entries.Count, Is.EqualTo(39));

        foreach (string locale in new[] { "ko", "en" })
        {
            StringTable table = collection.GetTable(locale) as StringTable;
            Assert.That(table, Is.Not.Null, locale);
            foreach (SharedTableData.SharedTableEntry shared in collection.SharedData.Entries)
            {
                StringTableEntry entry = table.GetEntry(shared.Id);
                Assert.That(entry, Is.Not.Null, $"{locale}/{shared.Key}");
                Assert.That(entry.Value, Is.Not.Empty, $"{locale}/{shared.Key}");
            }
        }
    }

    [TestCase("ko", "level_up.title", "레벨업")]
    [TestCase("en", "level_up.title", "LEVEL UP")]
    [TestCase("ko", "loading.status", "불러오는 중")]
    [TestCase("en", "loading.status", "LOADING")]
    public void TutorialTable_ContainsExpectedCoreText(string locale, string key, string expected)
    {
        StringTableCollection collection =
            LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Tutorial);
        StringTable table = collection.GetTable(locale) as StringTable;
        Assert.That(table.GetEntry(key)?.Value, Is.EqualTo(expected));
    }
}
