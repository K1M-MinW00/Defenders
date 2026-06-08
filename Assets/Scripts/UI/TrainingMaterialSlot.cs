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

    private UnitTrainingPanel trainingPanel;

    public void Setup(MaterialDataSO data, int owned, UnitTrainingPanel panel)
    {
        materialData = data;
        ownedCount = owned;
        trainingPanel = panel;

        iconImage.sprite = data.Icon;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(AddOne);

        minusButton.onClick.RemoveAllListeners();
        minusButton.onClick.AddListener(RemoveOne);

        Refresh();
    }

    private void AddOne()
    {
        trainingPanel.OnAddMaterial(materialData);
    }

    private void RemoveOne()
    {
        trainingPanel.OnRemoveMaterial(materialData);
    }

    public void Refresh()
    {
        int selectedCount = trainingPanel.GetSelectedCount(materialData.ItemId);

        ownedCountText.text = ownedCount.ToString("N0");
        selectedCountText.text = selectedCount > 0 ? selectedCount.ToString("N0") : "";
        minusButton.gameObject.SetActive(selectedCount > 0);
    }
}