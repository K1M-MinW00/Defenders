using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingMaterialSlot : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text ownedCountText;
    [SerializeField] private TMP_Text selectedCountText;

    private MaterialDataSO materialData;
    private int ownedCount;
    private Action onAdd;
    private Action onRemove;

    public string ItemId => materialData?.ItemId;

    public void Setup(MaterialDataSO data, int owned, Action addHandler, Action removeHandler)
    {
        materialData = data;
        ownedCount = Mathf.Max(0, owned);
        onAdd = addHandler;
        onRemove = removeHandler;

        if (iconImage != null)
            iconImage.sprite = data != null ? data.Icon : null;

        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(HandleAddClicked);
            selectButton.onClick.AddListener(HandleAddClicked);
        }

        if (minusButton != null)
        {
            minusButton.onClick.RemoveListener(HandleRemoveClicked);
            minusButton.onClick.AddListener(HandleRemoveClicked);
        }

        Refresh(0);
    }

    private void HandleAddClicked()
    {
        onAdd?.Invoke();
    }

    private void HandleRemoveClicked()
    {
        onRemove?.Invoke();
    }

    public void Refresh(int selectedCount)
    {
        selectedCount = Mathf.Clamp(selectedCount, 0, ownedCount);

        if (ownedCountText != null)
            ownedCountText.text = ownedCount.ToString("N0");

        if (selectedCountText != null)
            selectedCountText.text = selectedCount > 0 ? selectedCount.ToString("N0") : string.Empty;

        if (minusButton != null)
            minusButton.gameObject.SetActive(selectedCount > 0);

        if (selectButton != null)
            selectButton.interactable = materialData != null && selectedCount < ownedCount;
    }

    private void OnDestroy()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(HandleAddClicked);

        if (minusButton != null)
            minusButton.onClick.RemoveListener(HandleRemoveClicked);

        onAdd = null;
        onRemove = null;
    }
}
