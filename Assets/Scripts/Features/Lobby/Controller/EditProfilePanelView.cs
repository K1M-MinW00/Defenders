using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditProfilePanelView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Preview")]
    [SerializeField] private Image previewIconImage;

    [Header("Scroll")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private ProfileIconSlotUI slotPrefab;

    [Header("Button")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    private readonly List<ProfileIconSlotUI> slots = new();
    private string currentIconId;
    private string selectedIconId;
    private bool isSaving;

    private void Awake()
    {
        panelRoot.SetActive(false);
        cancelButton.onClick.AddListener(Close);
        confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
    }

    private void OnEnable()
    {
        UserDataRoot userData = UserDataManager.Instance.UserData;
        currentIconId = ProfileIconResolver.ResolveIconId(userData.Profile.IconId, userData.Roster);
        selectedIconId = currentIconId;

        RefreshPreviewIcon();
        CreateIconSlots(userData.Roster);
        RefreshButtons();
    }

    private void OnDisable()
    {
        ClearSlots();
    }

    private void Close()
    {
        if (!isSaving)
            panelRoot.SetActive(false);
    }

    private void CreateIconSlots(UserRosterData roster)
    {
        ClearSlots();
        HashSet<string> addedIds = new();

        if (roster?.OwnedUnits == null)
            return;

        foreach (UserUnitData unit in roster.OwnedUnits)
        {
            string unitId = unit?.UnitId;
            Sprite iconSprite = UnitDatabase.GetIcon(unitId);
            if (string.IsNullOrWhiteSpace(unitId) || iconSprite == null || !addedIds.Add(unitId))
                continue;

            ProfileIconSlotUI slot = Instantiate(slotPrefab, contentRoot);
            slot.Initialize(unitId, iconSprite, HandleIconSelected);
            slots.Add(slot);
        }
    }

    private void ClearSlots()
    {
        foreach (ProfileIconSlotUI slot in slots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        slots.Clear();
    }

    private void HandleIconSelected(string iconId)
    {
        if (!ProfileIconResolver.IsSelectable(iconId, UserDataManager.Instance.UserData.Roster))
            return;

        selectedIconId = iconId;
        RefreshPreviewIcon();
        RefreshButtons();
    }

    private void RefreshPreviewIcon()
    {
        UserDataRoot userData = UserDataManager.Instance.UserData;
        previewIconImage.sprite = ProfileIconResolver.ResolveIcon(selectedIconId, userData.Roster);
    }

    private void RefreshButtons()
    {
        bool hasChanged = selectedIconId != currentIconId;
        confirmButton.interactable = !isSaving && hasChanged;
        cancelButton.interactable = !isSaving;
    }

    private async void HandleConfirmButtonClicked()
    {
        if (isSaving || selectedIconId == currentIconId)
            return;

        isSaving = true;
        RefreshButtons();

        bool succeeded = await UserDataManager.Instance.UpdateProfileIconAsync(selectedIconId);

        isSaving = false;
        if (!succeeded)
        {
            UIFeedbackToast.Show("프로필 아이콘 저장에 실패했습니다.");
            RefreshButtons();
            return;
        }

        currentIconId = selectedIconId;
        Close();
    }

    private void OnDestroy()
    {
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(Close);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);
    }
}
