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
    private SkillDataSO currentSkill;
    private int currentPromotion;

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

        currentSkill = skill;
        currentPromotion = promotion;
        gameObject.SetActive(true);

        if (skillIconImage != null)
            skillIconImage.sprite = skill.icon;

        if (skillNameText != null)
            skillNameText.text = skill.LocalizedName;

        HideSlots();

        int index = 0;
        for (int upgradeIndex = 0; upgradeIndex < skill.upgrades.Count; upgradeIndex++)
        {
            SkillUpgradeData data = skill.upgrades[upgradeIndex];
            SkillUpgradeSlot slot = GetOrCreateSlot(index++);
            slot.Setup(promotion, data, skill.GetLocalizedUpgradeDescription(upgradeIndex));
            slot.gameObject.SetActive(true);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable() => LocalizationService.LocaleChanged += HandleLocaleChanged;

    private void OnDisable() => LocalizationService.LocaleChanged -= HandleLocaleChanged;

    private void HandleLocaleChanged(string _)
    {
        if (currentSkill != null)
            Open(currentSkill, currentPromotion);
    }

    private void OnDestroy()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
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
