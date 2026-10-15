using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class EditProfilePanelView : MonoBehaviour, IModalDismissHandler
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
    private ProfileIconEditPresenter presenter;
    private UserDataManager userDataManager;
    private bool isSaving;
    private int operationVersion;
    private ModalOverlayLifecycle lifecycle;
    private GameObject Root => panelRoot != null ? panelRoot : gameObject;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(Root);

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        cancelButton.onClick.AddListener(Dismiss);
        confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
    }

    public void Open()
    {
        if (!isSaving)
            Lifecycle.Show();
    }

    public void Initialize(UserDataManager manager)
    {
        if (manager == null || userDataManager == manager)
            return;

        userDataManager = manager;
        presenter = manager.UserData != null ? new ProfileIconEditPresenter(manager.UserData) : null;

        if (isActiveAndEnabled)
            RefreshEditor();
    }

    private void OnEnable()
    {
        if (!TryInitialize())
        {
            Debug.LogError("[EditProfilePanelView] User data is not ready.");
            confirmButton.interactable = false;
            return;
        }

        RefreshEditor();
    }

    private void RefreshEditor()
    {
        ProfileIconEditState state = presenter.Open(isSaving);
        ApplyState(state);
        ReconcileIconSlots(state.Options);
    }

    private void OnDisable()
    {
        operationVersion++;
        HideSlotsFrom(0);
    }

    public void Dismiss()
    {
        if (!isSaving)
            Lifecycle.Close();
    }

    public void HideImmediately() => Lifecycle.HideImmediately();

    // Inspector compatibility for scenes created before the modal convention.
    public void Close() => Dismiss();

    private void ReconcileIconSlots(IReadOnlyList<ProfileIconOption> options)
    {
        int visibleCount = 0;
        if (options != null)
        {
            foreach (ProfileIconOption option in options)
            {
                ProfileIconSlotUI slot = GetOrCreateSlot(visibleCount++);
                slot.gameObject.SetActive(true);
                slot.Initialize(option.IconId, option.Icon, HandleIconSelected);
            }
        }

        HideSlotsFrom(visibleCount);
    }

    private ProfileIconSlotUI GetOrCreateSlot(int index)
    {
        if (index < slots.Count)
            return slots[index];

        ProfileIconSlotUI slot = Instantiate(slotPrefab, contentRoot);
        slots.Add(slot);
        return slot;
    }

    private void HideSlotsFrom(int firstHiddenIndex)
    {
        for (int i = firstHiddenIndex; i < slots.Count; i++)
        {
            slots[i].Clear();
            slots[i].gameObject.SetActive(false);
        }
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
        if (isSaving || presenter == null || userDataManager == null)
            return;

        ProfileIconEditState state = presenter.Build(false);
        if (!state.CanSave)
            return;

        isSaving = true;
        int currentOperation = ++operationVersion;
        ApplyState(presenter.Build(true));

        try
        {
            bool succeeded;
            try
            {
                succeeded = await userDataManager.UpdateProfileIconAsync(presenter.SelectedIconId);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[EditProfilePanelView] Save failed: {exception}");
                succeeded = false;
            }

            if (!IsCurrentOperation(currentOperation))
                return;

            if (!succeeded)
            {
                UIFeedbackToast.Show("프로필 아이콘 저장에 실패했습니다.");
                return;
            }

            presenter.MarkSaved();
            Lifecycle.Close();
        }
        finally
        {
            if (this != null)
            {
                isSaving = false;
                if (isActiveAndEnabled && presenter != null)
                    ApplyState(presenter.Build(false));
            }
        }
    }

    private void OnDestroy()
    {
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(Dismiss);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);
    }

    private bool IsCurrentOperation(int version) =>
        this != null && isActiveAndEnabled && version == operationVersion;

    private bool TryInitialize()
    {
        userDataManager ??= UserDataManager.Instance;
        if (userDataManager?.UserData == null)
            return false;

        presenter ??= new ProfileIconEditPresenter(userDataManager.UserData);
        return true;
    }

    private bool ValidateReferences()
    {
        bool valid = previewIconImage != null && contentRoot != null && slotPrefab != null &&
                     cancelButton != null && confirmButton != null;
        if (!valid)
            Debug.LogError("[EditProfilePanelView] Required UI references are not assigned.", this);
        return valid;
    }
}
