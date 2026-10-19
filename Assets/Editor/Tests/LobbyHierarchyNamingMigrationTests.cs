using NUnit.Framework;

public sealed class LobbyHierarchyNamingMigrationTests
{
    [TestCase("Promote_Button", false, "PromoteButton")]
    [TestCase("SkillName_Text", true, "SkillNameText")]
    [TestCase("HP_Tab", false, "HpTab")]
    [TestCase("Panel_LowerBtn", false, "PanelLowerButton")]
    [TestCase("Level & Power ", false, "LevelAndPower")]
    [TestCase("RewardSlot_1", false, "RewardSlot01")]
    [TestCase("Label", true, "Text")]
    public void NormalizeName_UsesProjectConvention(string source, bool isText, string expected)
    {
        Assert.That(LobbyHierarchyNamingMigration.NormalizeName(source, isText), Is.EqualTo(expected));
    }
}
