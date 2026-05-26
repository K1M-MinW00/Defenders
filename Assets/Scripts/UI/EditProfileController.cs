using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditProfileController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Preview")]
    [SerializeField] private Image previewIconImage;

    [Header("Scroll")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private ProfileIconSlotUI slotPrefab;

    [Header("Buttons")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    [Header("References")]
    [SerializeField] private ProfileIconDatabase iconDatabase;
    [SerializeField] private SettingsPanelController settingsPanel;
    [SerializeField] private Image icon;

    private readonly List<ProfileIconSlotUI> slots = new();

    private string currentIconId;
    private string selectedIconId;
    private bool isSaving;

    private void Awake()
    {
        root.SetActive(false);

        cancelButton.onClick.AddListener(Close);
        confirmButton.onClick.AddListener(OnClickConfirm);
    }

    private void OnEnable()
    {
        root.SetActive(true);

        currentIconId = UserDataManager.Instance.UserData.Profile.IconId;
        selectedIconId = currentIconId;

        RefreshPreview();
        BuildIconSlots();
    }

    private void Close()
    {
        if (isSaving)
            return;

        root.SetActive(false);
    }

    private void BuildIconSlots()
    {
        ClearSlots();

        var ownedUnits = UserDataManager.Instance.UserData.Roster.OwnedUnits;
        Debug.Log(ownedUnits.Count);

        foreach (var unit in ownedUnits)
        {
            string unitId = unit.UnitId;
            Sprite iconSprite = iconDatabase.GetIcon(unitId);

            ProfileIconSlotUI slot = Instantiate(slotPrefab, contentRoot);
            slot.Initialize(unitId, iconSprite, OnSelectIcon);

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

    private void OnSelectIcon(string iconId)
    {
        selectedIconId = iconId;

        RefreshPreview();
    }

    private void RefreshPreview()
    {
        previewIconImage.sprite = iconDatabase.GetIcon(selectedIconId);
    }

    private async void OnClickConfirm()
    {
        if (isSaving)
            return;

        if (selectedIconId == currentIconId)
        {
            root.SetActive(false);
            return;
        }

        isSaving = true;
        confirmButton.interactable = false;
        cancelButton.interactable = false;

        await UserDataManager.Instance.UpdateProfileIconAsync(selectedIconId);

        isSaving = false;
        confirmButton.interactable = true;
        cancelButton.interactable = true;

        settingsPanel.Refresh();
        root.SetActive(false);
    }
}