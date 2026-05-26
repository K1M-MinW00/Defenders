using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBattleUIController : MonoBehaviour
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

    [Header("References")]
    [SerializeField] private ProfileIconDatabase iconDatabase;

    private void OnEnable()
    {
        UserDataRoot userData = UserDataManager.Instance.UserData;

        if (userData == null)
            return;

        RefreshProfile(userData);
        RefreshProgress(userData);
        RefreshResources(userData);

        UserDataManager.Instance.OnProfileUpdated += HandleProfileUpdated;
        UserDataManager.Instance.OnProgressUpdated += HandleProgressUpdated;
        UserDataManager.Instance.OnResourceUpdated += HandleResourceUpdated;
    }

    private void OnDisable()
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

        levelText.text = $"{profile.Level}";
        powerText.text = $"{userData.Roster.Power}";

        Sprite iconSprite = iconDatabase.GetIcon(profile.IconId);
        profileIconImage.sprite = iconSprite;

        float normalizedExp = GetNormalizedExp(profile.Exp, profile.Level);
        expSlider.value = normalizedExp;
    }

    private void RefreshProgress(UserDataRoot userData)
    {
        UserProgressData progress = userData.Progress;

        sectorStageText.text = $"{progress.CurrentSector}-{progress.CurrentStage}";

        bestWaveText.text = $"{progress.BestWaveCleared}";
    }

    private void RefreshResources(UserDataRoot userData)
    {
        UserResourceData resource = userData.Resource;

        goldText.text = resource.Gold.ToString("N0");
        gemText.text = resource.Gem.ToString("N0");
        fuelText.text = $"{resource.Fuel} / {resource.MaxFuel}";
    }

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