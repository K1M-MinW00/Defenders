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
    [SerializeField] private Button backdropButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    private readonly List<ProfileIconSlotUI> slots = new();
    private ProfileIconEditPresenter presenter;
    private bool isSaving;

    private void Awake()
    {
        backdropButton?.onClick.AddListener(Close);
        cancelButton.onClick.AddListener(Close);
        confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
    }

    public void Open()
    {
        if (!isSaving && panelRoot != null)
            panelRoot.SetActive(true);
    }

    private void OnEnable()
    {
        UserDataRoot userData = UserDataManager.Instance?.UserData;
        if (userData == null)
        {
            Debug.LogError("[EditProfilePanelView] User data is not ready.");
            confirmButton.interactable = false;
            return;
        }

        presenter ??= new ProfileIconEditPresenter(userData);
        ProfileIconEditState state = presenter.Open(isSaving);
        ApplyState(state);
        CreateIconSlots(state.Options);
    }

    private void OnDisable()
    {
        ClearSlots();
    }

    public void Close()
    {
        if (!isSaving)
            panelRoot.SetActive(false);
    }

    private void CreateIconSlots(IReadOnlyList<ProfileIconOption> options)
    {
        ClearSlots();

        if (options == null)
            return;

        foreach (ProfileIconOption option in options)
        {
            ProfileIconSlotUI slot = Instantiate(slotPrefab, contentRoot);
            slot.Initialize(option.IconId, option.Icon, HandleIconSelected);
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
        if (presenter == null || !presenter.TrySelect(iconId))
            return;

        ApplyState(presenter.Build(isSaving));
    }

    private void ApplyState(ProfileIconEditState state)
    {
        if (state == null)
            return;

        previewIconImage.sprite = state.PreviewIcon;
        confirmButton.interactable = state.CanSave;
        cancelButton.interactable = !isSaving;
    }

    private async void HandleConfirmButtonClicked()
    {
        if (isSaving || presenter == null)
            return;

        ProfileIconEditState state = presenter.Build(false);
        if (!state.CanSave)
            return;

        isSaving = true;
        ApplyState(presenter.Build(true));

        bool succeeded = await UserDataManager.Instance.UpdateProfileIconAsync(presenter.SelectedIconId);

        isSaving = false;
        if (!succeeded)
        {
            UIFeedbackToast.Show("프로필 아이콘 저장에 실패했습니다.");
            ApplyState(presenter.Build(false));
            return;
        }

        presenter.MarkSaved();
        Close();
    }

    private void OnDestroy()
    {
        if (backdropButton != null)
            backdropButton.onClick.RemoveListener(Close);

        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(Close);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);
    }
}
