using System;
using TMPro;
using UnityEngine;

public class GemConfirmPopupView : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;

    private Action onConfirm;

    public void Open(int gemCost,Action confirmAction)
    {
        gameObject.SetActive(true);

        onConfirm = confirmAction;

        messageText.text = $"{gemCost:N0} 개를 사용하여 모집하시겠습니까?";
    }

    public void OnConfirm()
    {
        gameObject.SetActive(false);
        onConfirm?.Invoke();
    }

    public void OnCancel()
    {
        gameObject.SetActive(false);
    }
}