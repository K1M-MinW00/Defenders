using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetailPopup : MonoBehaviour
{
    [SerializeField] private Button closeButton;

    [Header("Header")]
    [SerializeField] private Image skillIconImage;
    [SerializeField] private TMP_Text skillNameText;

    [Header("Content")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private SkillUpgradeSlot slotPrefab;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        Close();
    }

    public void Open(SkillDataSO skill, int promotion)
    {
        if (skill == null)
            return;

        gameObject.SetActive(true);

        if (skillIconImage != null)
            skillIconImage.sprite = skill.icon;

        if (skillNameText != null)
            skillNameText.text = skill.skillName;

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (SkillUpgradeData data in skill.upgrades)
        {
            SkillUpgradeSlot slot = Instantiate(slotPrefab, contentRoot);

            slot.Setup(promotion, data);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}