using System;
using TMPro;
using UnityEngine;

public class GemConfirmPopupView : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;

    private readonly OneShotConfirmation confirmation = new();
    private bool isConfirming;

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

        messageText.text = $"{gemCost:N0} 개를 사용하여 모집하시겠습니까?";
        gameObject.SetActive(true);
        PopupBackStack.Push(this, OnCancel);
        return true;
    }

    public void OnConfirm()
    {
        if (!confirmation.HasPendingAction)
            return;

        isConfirming = true;
        gameObject.SetActive(false);
        isConfirming = false;
        confirmation.TryConfirm();
    }

    public void OnCancel()
    {
        confirmation.Cancel();
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        PopupBackStack.Remove(this);

        if (!isConfirming)
            confirmation.Cancel();
    }
}
