using System;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GemConfirmPopupView : MonoBehaviour, IModalDismissHandler
{
    [SerializeField] private TMP_Text messageText;

    private readonly OneShotConfirmation confirmation = new();
    private bool isConfirming;
    private int currentGemCost;
    private ModalOverlayLifecycle lifecycle;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    public bool Open(int gemCost, Action confirmAction)
    {
        if (gemCost < 0 || !confirmation.TryOpen(confirmAction))
        {
            Debug.LogError("[GemConfirmPopupView] A non-negative cost and confirmation action are required.");
            return false;
        }

        if (messageText == null)
        {
            confirmation.Cancel();
            Debug.LogError("[GemConfirmPopupView] Message text is not assigned.", this);
            return false;
        }

        currentGemCost = gemCost;
        RefreshMessage();
        Lifecycle.Show();
        return true;
    }

    public void OnConfirm()
    {
        if (!confirmation.HasPendingAction)
            return;

        isConfirming = true;
        Lifecycle.Close(() =>
        {
            isConfirming = false;
            confirmation.TryConfirm();
        });
    }

    public void OnCancel()
    {
        confirmation.Cancel();
        Lifecycle.Close();
    }

    public void Dismiss() => OnCancel();

    private void OnEnable()
    {
        LocalizationService.LocaleChanged += HandleLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        if (!isConfirming)
            confirmation.Cancel();
    }

    private void HandleLocaleChanged(string _) => RefreshMessage();

    private void RefreshMessage()
    {
        if (messageText == null || !confirmation.HasPendingAction)
            return;

        messageText.text = LocalizationService.Get(
            LocalizationTables.Lobby,
            "recruit.confirm_gems",
            currentGemCost);
    }
}
