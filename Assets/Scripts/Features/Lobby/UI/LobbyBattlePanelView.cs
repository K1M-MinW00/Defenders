using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyBattlePanelView : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private Image profileIconImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text powerText;
    [SerializeField] private Slider expSlider;

    [Header("Progress")]
    [SerializeField] private TMP_Text sectorStageText;
    [SerializeField] private TMP_Text bestWaveText;

    [Header("Resources")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text gemText;
    [SerializeField] private TMP_Text fuelText;

    [Header("Battle")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private Button startButton;

    private void Awake()
    {
        startButton.onClick.AddListener(HandleStartButtonClicked);
    }

    private void OnEnable()
    {
        UserDataRoot userData = UserDataManager.Instance.UserData;

        if (userData == null)
            return;

        Refresh();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        UserDataManager.Instance.OnProfileUpdated += HandleProfileUpdated;
        UserDataManager.Instance.OnProgressUpdated += HandleProgressUpdated;
        UserDataManager.Instance.OnResourceUpdated += HandleResourceUpdated;
    }

    private void UnsubscribeEvents()
    {
        if (UserDataManager.Instance == null)
            return;

        UserDataManager.Instance.OnProfileUpdated -= HandleProfileUpdated;
        UserDataManager.Instance.OnProgressUpdated -= HandleProgressUpdated;
        UserDataManager.Instance.OnResourceUpdated -= HandleResourceUpdated;
    }

    private void HandleProfileUpdated()
    {
        RefreshProfile(UserDataManager.Instance.UserData);
    }

    private void HandleProgressUpdated()
    {
        RefreshProgress(UserDataManager.Instance.UserData);
    }

    private void HandleResourceUpdated()
    {
        RefreshResources(UserDataManager.Instance.UserData);
    }

    public void Refresh()
    {
        UserDataRoot userData = UserDataManager.Instance.UserData;

        if (userData == null)
        {
            Debug.LogError("LobbyPanelUI Refresh failed. UserData is null.");
            return;
        }

        RefreshProfile(userData);
        RefreshProgress(userData);
        RefreshResources(userData);
    }

    private void RefreshProfile(UserDataRoot userData)
    {
        UserProfileData profile = userData.Profile;

        levelText.text = profile.Level.ToString();
        powerText.text = userData.Roster.Power.ToString();

        profileIconImage.sprite = UnitDatabase.GetIcon(profile.IconId);

        expSlider.value = GetNormalizedExp(profile.Exp, profile.Level);
    }

    private void RefreshProgress(UserDataRoot userData)
    {
        UserProgressData progress = userData.Progress;

        sectorStageText.text = $"{progress.CurrentSector}-{progress.CurrentStage}";

        bestWaveText.text = progress.BestWaveCleared.ToString();
    }

    private void RefreshResources(UserDataRoot userData)
    {
        UserResourceData resource = userData.Resource;

        goldText.text = resource.Gold.ToString("N0");
        gemText.text = resource.Gem.ToString("N0");
        fuelText.text = $"{resource.Fuel} / {resource.MaxFuel}";
    }

    private void HandleStartButtonClicked()
    {
        UserDataRoot userData = UserDataManager.Instance.UserData;

        if (userData == null)
        {
            Debug.LogError("Start battle failed. UserData is null.");
            return;
        }

        List<string> selectedUnitIds = userData.Roster.SelectedUnitIds;

        if (selectedUnitIds == null || selectedUnitIds.Count == 0)
        {
            Debug.LogError("Start battle failed. SelectedUnitIds is empty.");
            return;
        }

        StageEnterData enterData = new(userData.Progress.CurrentSector, userData.Progress.CurrentStage, selectedUnitIds);

        StageEnterHolder.Set(enterData);

        SceneManager.LoadScene(gameSceneName);
    }

    // TODO : 추후 UserLevelTable 또는 UserLevelCalculator 로 이동
    private float GetNormalizedExp(int exp, int level)
    {
        int requiredExp = GetRequiredExp(level);

        if (requiredExp <= 0)
            return 0f;

        return (float)exp / requiredExp;
    }

    private int GetRequiredExp(int level)
    {
        return 100 + (level - 1) * 50;
    }
}