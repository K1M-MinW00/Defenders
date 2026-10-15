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
    private readonly System.Collections.Generic.List<SkillUpgradeSlot> slotPool = new();

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

        HideSlots();

        int index = 0;
        foreach (SkillUpgradeData data in skill.upgrades)
        {
            SkillUpgradeSlot slot = GetOrCreateSlot(index++);
            slot.Setup(promotion, data);
            slot.gameObject.SetActive(true);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private SkillUpgradeSlot GetOrCreateSlot(int index)
    {
        if (index < slotPool.Count)
            return slotPool[index];

        SkillUpgradeSlot slot = Instantiate(slotPrefab, contentRoot);
        slot.name = $"SkillUpgradeSlot_{slotPool.Count + 1}";
        slotPool.Add(slot);
        return slot;
    }

    private void HideSlots()
    {
        foreach (SkillUpgradeSlot slot in slotPool)
            if (slot != null)
                slot.gameObject.SetActive(false);
    }
}
