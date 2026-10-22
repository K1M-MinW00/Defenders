using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public sealed class CombatStatisticsSceneWiringTests
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    [Test]
    public void GameScene_CombatStatisticsHud_IsFullyWiredAndInactiveByDefault()
    {
        SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();

        try
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            StageCombatStatisticsUI view = FindInScene<StageCombatStatisticsUI>(scene);
            StageUIController controller = FindInScene<StageUIController>(scene);

            Assert.That(view, Is.Not.Null);
            Assert.That(view.gameObject.activeSelf, Is.False);
            Assert.That(controller, Is.Not.Null);

            SerializedObject controllerObject = new(controller);
            Assert.That(
                controllerObject.FindProperty("combatStatisticsUI").objectReferenceValue,
                Is.SameAs(view));

            SerializedObject viewObject = new(view);
            SerializedProperty rows = viewObject.FindProperty("rows");
            Assert.That(rows.arraySize, Is.EqualTo(3));
            for (int i = 0; i < rows.arraySize; i++)
            {
                StageCombatStatisticsRowUI row =
                    rows.GetArrayElementAtIndex(i).objectReferenceValue as StageCombatStatisticsRowUI;
                AssertRowIsWired(row, i + 1);
            }

            AssertReference<UnityEngine.UI.Button>(viewObject, "damageButton");
            AssertReference<UnityEngine.UI.Button>(viewObject, "healingButton");
            AssertReference<UnityEngine.UI.Button>(viewObject, "takenButton");
            AssertReference<TMP_Text>(viewObject, "damageLabel");
            AssertReference<TMP_Text>(viewObject, "healingLabel");
            AssertReference<TMP_Text>(viewObject, "takenLabel");

            AssertLocalizedLabels(view);
        }
        finally
        {
            if (previousSetup.Length > 0)
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            else
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
    }

    private static void AssertRowIsWired(StageCombatStatisticsRowUI row, int expectedRank)
    {
        Assert.That(row, Is.Not.Null, $"Rank {expectedRank} row is missing.");
        SerializedObject rowObject = new(row);
        AssertReference<TMP_Text>(rowObject, "rankText");
        AssertReference<UnityEngine.UI.Image>(rowObject, "portraitImage");
        AssertReference<TMP_Text>(rowObject, "damageText");
        AssertReference<TMP_Text>(rowObject, "healingText");
        AssertReference<TMP_Text>(rowObject, "takenText");
        AssertSlider(rowObject, "damageSlider");
        AssertSlider(rowObject, "healingSlider");
        AssertSlider(rowObject, "takenSlider");
        AssertMeterLabelsAreAboveSlider(row.transform, "Damage Meter");
        AssertMeterLabelsAreAboveSlider(row.transform, "Healing Meter");
        AssertMeterLabelsAreAboveSlider(row.transform, "Taken Meter");
    }

    private static void AssertMeterLabelsAreAboveSlider(Transform row, string meterName)
    {
        Transform meter = row.Find(meterName);
        RectTransform icon = meter?.Find("Icon") as RectTransform;
        RectTransform value = meter?.Find("Value") as RectTransform;
        RectTransform track = meter?.Find("Track") as RectTransform;

        Assert.That(meter, Is.Not.Null, $"{meterName} is missing.");
        Assert.That(icon, Is.Not.Null, $"{meterName} icon is missing.");
        Assert.That(value, Is.Not.Null, $"{meterName} value is missing.");
        Assert.That(track, Is.Not.Null, $"{meterName} track is missing.");
        Assert.That(icon.anchoredPosition.y, Is.GreaterThan(track.anchoredPosition.y));
        Assert.That(value.anchoredPosition.y, Is.GreaterThan(track.anchoredPosition.y));
    }

    private static void AssertSlider(SerializedObject owner, string propertyName)
    {
        UnityEngine.UI.Slider slider =
            owner.FindProperty(propertyName).objectReferenceValue as UnityEngine.UI.Slider;
        Assert.That(slider, Is.Not.Null, $"{propertyName} is not wired.");
        Assert.That(slider.interactable, Is.False);
        Assert.That(slider.minValue, Is.EqualTo(0f));
        Assert.That(slider.maxValue, Is.EqualTo(1f));
        Assert.That(slider.fillRect, Is.Not.Null);
    }

    private static void AssertLocalizedLabels(StageCombatStatisticsUI view)
    {
        var expectedKeys = new HashSet<string>
        {
            "combat.statistics.rank",
            "combat.statistics.unit",
            "combat.statistics.damage",
            "combat.statistics.healing",
            "combat.statistics.damage_taken",
        };
        LocalizeStringEvent[] localizers =
            view.GetComponentsInChildren<LocalizeStringEvent>(includeInactive: true);

        Assert.That(localizers, Has.Length.EqualTo(8));
        foreach (LocalizeStringEvent localizer in localizers)
        {
            Assert.That(localizer.StringReference.TableReference.TableCollectionName, Is.EqualTo(LocalizationTables.Stage));
            Assert.That(expectedKeys, Does.Contain(localizer.StringReference.TableEntryReference.Key));
            Assert.That(localizer.OnUpdateString.GetPersistentEventCount(), Is.EqualTo(1));
            Assert.That(localizer.OnUpdateString.GetPersistentTarget(0), Is.TypeOf<TextMeshProUGUI>());
            Assert.That(localizer.OnUpdateString.GetPersistentMethodName(0), Is.EqualTo("SetText"));
        }
    }

    private static void AssertReference<T>(SerializedObject owner, string propertyName)
        where T : Object
    {
        Assert.That(
            owner.FindProperty(propertyName).objectReferenceValue,
            Is.InstanceOf<T>(),
            $"{propertyName} is not wired.");
    }

    private static T FindInScene<T>(Scene scene) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T component = root.GetComponentInChildren<T>(includeInactive: true);
            if (component != null)
                return component;
        }

        return null;
    }
}
