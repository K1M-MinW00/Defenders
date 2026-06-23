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
        currentIconId = UserDataManager.Instance.UserData.Profile.IconId;
        selectedIconId = currentIconId;

        RefreshPreviewIcon();
        CreateIconSlots();
    }

    private void OnDisable()
    {
        ClearSlots();
    }

    private void Close()
    {
        if (isSaving)
            return;

        panelRoot.SetActive(false);
    }

    private void CreateIconSlots()
    {
        ClearSlots();

        var ownedUnits = UserDataManager.Instance.UserData.Roster.OwnedUnits;

        foreach (var unit in ownedUnits)
        {
            string unitId = unit.UnitId;
            Sprite iconSprite = UnitDatabase.GetIcon(unitId);

            ProfileIconSlotUI slot = Instantiate(slotPrefab, contentRoot);
            slot.Initialize(unitId, iconSprite, HandleIconSelected);

            slots.Add(slot);
        }
    }

    private void ClearSlots()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        slots.Clear();
    }
    private void HandleIconSelected(string iconId)
    {
        selectedIconId = iconId;
        RefreshPreviewIcon();
    }

    private void RefreshPreviewIcon()
    {
        previewIconImage.sprite = UnitDatabase.GetIcon(selectedIconId);
    }

    private async void HandleConfirmButtonClicked()
    {
        if (isSaving)
            return;

        if (selectedIconId == currentIconId)
        {
            Close();
            return;
        }

        isSaving = true;

        confirmButton.interactable = false;
        cancelButton.interactable = false;

        await UserDataManager.Instance.UpdateProfileIconAsync(selectedIconId);

        confirmButton.interactable = true;
        cancelButton.interactable = true;

        isSaving = false;
        Close();
    }
}