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
    private bool isSaving;
    private int operationVersion;
    private ModalOverlayLifecycle lifecycle;
    private GameObject Root => panelRoot != null ? panelRoot : gameObject;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(Root);

    private void Awake()
    {
        cancelButton.onClick.AddListener(Dismiss);
        confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
    }

    public void Open()
    {
        if (!isSaving)
            Lifecycle.Show();
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
        if (isSaving || presenter == null)
            return;

        ProfileIconEditState state = presenter.Build(false);
        if (!state.CanSave)
            return;

        isSaving = true;
        int currentOperation = ++operationVersion;
        ApplyState(presenter.Build(true));

        bool succeeded;
        try
        {
            succeeded = await UserDataManager.Instance.UpdateProfileIconAsync(presenter.SelectedIconId);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[EditProfilePanelView] Save failed: {exception}");
            succeeded = false;
        }

        isSaving = false;
        if (!isActiveAndEnabled || currentOperation != operationVersion)
            return;

        if (!succeeded)
        {
            UIFeedbackToast.Show("프로필 아이콘 저장에 실패했습니다.");
            ApplyState(presenter.Build(false));
            return;
        }

        presenter.MarkSaved();
        Dismiss();
    }

    private void OnDestroy()
    {
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(Dismiss);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);
    }
}
