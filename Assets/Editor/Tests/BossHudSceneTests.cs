using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class BossHudSceneTests
{
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";

    [Test]
    public void GameScene_BossHudIsVisibleSizedAndWired()
    {
        EditorSceneManager.OpenScene(GameScenePath);

        GameObject canvas = GameObject.Find("Stage HUD Canvas");
        Assert.That(canvas, Is.Not.Null);

        Transform container = canvas.transform.Find("BossBattleHUD");
        Transform infoRoot = container?.Find("BossInfoRoot");
        Transform timerRoot = container?.Find("BossTimerRoot");
        Assert.That(container, Is.Not.Null);
        Assert.That(infoRoot, Is.Not.Null);
        Assert.That(timerRoot, Is.Not.Null);
        Assert.That(((RectTransform)infoRoot).rect.width, Is.GreaterThan(0f));
        Assert.That(((RectTransform)infoRoot).rect.height, Is.GreaterThan(0f));
        Assert.That(((RectTransform)infoRoot).rect.width, Is.LessThanOrEqualTo(640f));
        Assert.That(((RectTransform)infoRoot).rect.height, Is.LessThanOrEqualTo(82f));
        Assert.That(((RectTransform)infoRoot).anchoredPosition.y, Is.GreaterThanOrEqualTo(-20f));
        Assert.That(((RectTransform)timerRoot).rect.width, Is.GreaterThan(0f));
        Assert.That(((RectTransform)timerRoot).rect.height, Is.GreaterThan(0f));
        RectTransform timerRect = (RectTransform)timerRoot;
        Assert.That(timerRect.anchorMin, Is.EqualTo(new Vector2(0f, 1f)));
        Assert.That(timerRect.anchorMax, Is.EqualTo(new Vector2(0f, 1f)));
        Assert.That(timerRect.pivot, Is.EqualTo(new Vector2(0f, 1f)));
        Assert.That(timerRect.anchoredPosition.x, Is.LessThan(250f));

        BossHudView hud = infoRoot.GetComponent<BossHudView>();
        BossBattleTimerView timer = timerRoot.GetComponent<BossBattleTimerView>();
        Assert.That(hud, Is.Not.Null);
        Assert.That(timer, Is.Not.Null);

        SerializedObject hudObject = new(hud);
        Assert.That(hudObject.FindProperty("root").objectReferenceValue, Is.Not.Null);
        Assert.That(hudObject.FindProperty("nameText").objectReferenceValue, Is.Not.Null);
        Assert.That(hudObject.FindProperty("hpSlider").objectReferenceValue, Is.Not.Null);
        Assert.That(hudObject.FindProperty("shieldSlider").objectReferenceValue, Is.Not.Null);
        Assert.That(hudObject.FindProperty("enrageIcon").objectReferenceValue, Is.Not.Null);
        GameObject enrageObject = hudObject.FindProperty("enrageIcon").objectReferenceValue as GameObject;
        Assert.That(enrageObject.GetComponent<UnityEngine.UI.Image>().sprite, Is.Not.Null);

        SerializedObject timerObject = new(timer);
        Assert.That(timerObject.FindProperty("root").objectReferenceValue, Is.Not.Null);
        Assert.That(timerObject.FindProperty("timerText").objectReferenceValue, Is.Not.Null);
        Assert.That(timerObject.FindProperty("blinkGroup").objectReferenceValue, Is.Not.Null);

        StageUIController stageUI = Object.FindFirstObjectByType<StageUIController>(FindObjectsInactive.Include);
        SerializedObject stageUIObject = new(stageUI);
        Assert.That(stageUIObject.FindProperty("bossBattleTimerView").objectReferenceValue, Is.EqualTo(timer));
        Assert.That(stageUIObject.FindProperty("bossHudView").objectReferenceValue, Is.EqualTo(hud));
    }
}
