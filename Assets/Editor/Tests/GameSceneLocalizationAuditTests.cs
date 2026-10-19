using NUnit.Framework;

public sealed class GameSceneLocalizationAuditTests
{
    [TestCase("Stage UI Root/Stage HUD Canvas/HP Summary View/Monster_Slider/Monster Count View")]
    [TestCase("Stage UI Root/Stage HUD Canvas/Top HUD/Stage Info View")]
    [TestCase("Stage UI Root/Stage Popup Canvas/Monster Info Popup/Panel/Scroll View/Viewport/Content/Monster Info Item Template/Description")]
    [TestCase("Stage UI Root/Stage Popup Canvas/Monster Info Popup/Panel/Scroll View/Viewport/Content/Monster Info Item Template/Name Background/Name")]
    [TestCase("Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Clear Panel/Return_Button/Text (TMP)")]
    [TestCase("Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Clear Panel/StageData_Panel/StageName_Text")]
    [TestCase("Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Clear Panel/StageData_Panel/SurvivedWave_Panel/SurvivedWave_Text")]
    [TestCase("Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Fail Panel/Return_Button/Text (TMP)")]
    [TestCase("Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Fail Panel/StageData_Panel/StageName_Text")]
    [TestCase("Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Fail Panel/StageData_Panel/SurvivedWave_Panel/SurvivedWave_Text")]
    public void RuntimeLocalizedPath_IsRecognized(string hierarchy)
    {
        Assert.That(GameSceneLocalizationAudit.IsKnownRuntimeLocalized(hierarchy), Is.True);
    }

    [TestCase("Stage UI Root/Unknown/Label")]
    [TestCase("")]
    [TestCase(null)]
    public void UnknownPath_StillRequiresReview(string hierarchy)
    {
        Assert.That(GameSceneLocalizationAudit.IsKnownRuntimeLocalized(hierarchy), Is.False);
    }
}
