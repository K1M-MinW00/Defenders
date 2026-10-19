using NUnit.Framework;

public sealed class LobbyLocalizationAuditTests
{
    [TestCase("모집", true)]
    [TestCase("Recruit", true)]
    [TestCase("공격력 +10%", true)]
    [TestCase("+10%", false)]
    [TestCase("  ", false)]
    [TestCase(null, false)]
    public void HasTranslatableText_DetectsOnlyTextThatNeedsLocalization(string value, bool expected)
    {
        Assert.That(LobbyLocalizationAudit.HasTranslatableText(value), Is.EqualTo(expected));
    }

    [TestCase("상품명", "Assets/Prefabs/UI/Shop/Card.prefab", "Card/Name", LocalizationAuditClassification.DynamicDataCandidate)]
    [TestCase("모집", "Assets/Scenes/LobbyScene.unity", "Canvas/Recruit/Label", LocalizationAuditClassification.FixedUiCandidate)]
    [TestCase("로그인 1회", "Assets/Scenes/LobbyScene.unity", "Content/QuestItem_Login/Description", LocalizationAuditClassification.NeedsReview)]
    [TestCase("점검 보상", "Assets/Prefabs/UI/UI_MailSlot.prefab", "UI_MailSlot/Title_Text", LocalizationAuditClassification.DynamicDataCandidate)]
    [TestCase("7일 후 만료", "Assets/Prefabs/UI/UI_MailSlot.prefab", "UI_MailSlot/Exp_Text", LocalizationAuditClassification.DynamicDataCandidate)]
    [TestCase("이동", "Assets/Prefabs/UI/Quest/QuestItem.prefab", "QuestItem/ActionButton/Label", LocalizationAuditClassification.DynamicDataCandidate)]
    [TestCase("로그인 1회", "Assets/Scenes/LobbyScene.unity", "Content/QuestItem_Login/Content/Description", LocalizationAuditClassification.DynamicDataCandidate)]
    [TestCase("보상 정보", "Assets/Prefabs/UI/Quest/QuestRewardDisplay.prefab", "QuestRewardDisplay/Panel/Title", LocalizationAuditClassification.DynamicDataCandidate)]
    [TestCase("확인", "Assets/Scenes/LobbyScene.unity", "LobbyScreenCanvas/NicknameEditOverlay/ModalCard/ActionButtons/Check_Button/Label", LocalizationAuditClassification.DynamicDataCandidate)]
    public void Classify_ProvidesSafeInitialTriage(
        string text,
        string assetPath,
        string hierarchyPath,
        LocalizationAuditClassification expected)
    {
        Assert.That(LobbyLocalizationAudit.Classify(text, assetPath, hierarchyPath), Is.EqualTo(expected));
    }
}
