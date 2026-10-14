using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class EquipmentDetailOverlayView : MonoBehaviour, IModalDismissHandler
{
    public enum DisplayMode
    {
        Inventory,
        Equipped,
        Preview,
    }

    [Header("Content")]
    [SerializeField] private TMP_Text headerLabel;
    [SerializeField] private Image rarityHeader;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private EquipmentSlotView equipmentSlot;
    [SerializeField] private Image baseEffectIcon;
    [SerializeField] private TMP_Text baseEffectText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject bonusEffectRoot;
    [SerializeField] private Image bonusEffectIcon;
    [SerializeField] private TMP_Text bonusEffectText;

    [Header("Actions")]
    [SerializeField] private Button leftButton;
    [SerializeField] private TMP_Text leftButtonLabel;
    [SerializeField] private Button primaryButton;
    [SerializeField] private TMP_Text primaryButtonLabel;
    [SerializeField] private Button rightButton;
    [SerializeField] private TMP_Text rightButtonLabel;
    [SerializeField] private Button closeButton;

    [Header("Stat Icons")]
    [SerializeField] private Sprite attackStatIcon;
    [SerializeField] private Sprite criticalStatIcon;
    [SerializeField] private Sprite maxHealthStatIcon;
    [SerializeField] private Sprite damageReductionStatIcon;

    private Action dismissed;
    private ModalOverlayLifecycle lifecycle;

    public DisplayMode CurrentMode { get; private set; }
    public bool IsVisible => Lifecycle.IsVisible;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Dismiss);
    }

    public void ShowInventory(
        EquipmentItemData item,
        EquipmentDataSO definition,
        Action onDismantle,
        bool canDismantle,
        Action onDismissed = null)
    {
        if (!ShowContent(item, definition, DisplayMode.Inventory, onDismissed))
            return;
        BindButton(leftButton, leftButtonLabel, "취소", Dismiss, true);
        BindButton(primaryButton, primaryButtonLabel, "분해", onDismantle, canDismantle);
        BindButton(rightButton, rightButtonLabel, null, null, false);
    }

    public void ShowEquipped(
        EquipmentItemData item,
        EquipmentDataSO definition,
        Action onUnequip,
        Action onEnhance,
        Action onReplace,
        bool isMaxLevel,
        bool actionsEnabled,
        Action onDismissed = null)
    {
        if (!ShowContent(item, definition, DisplayMode.Equipped, onDismissed))
            return;
        BindButton(leftButton, leftButtonLabel, "해제", onUnequip, actionsEnabled);
        BindButton(
            primaryButton,
            primaryButtonLabel,
            isMaxLevel ? "최대" : "강화",
            isMaxLevel ? null : onEnhance,
            actionsEnabled && !isMaxLevel);
        BindButton(rightButton, rightButtonLabel, "장비 교체", onReplace, actionsEnabled);
    }

    public void ShowPreview(
        EquipmentItemData item,
        EquipmentDataSO definition,
        Action onDismissed = null)
    {
        if (!ShowContent(item, definition, DisplayMode.Preview, onDismissed))
            return;
        BindButton(leftButton, leftButtonLabel, "확인", Dismiss, true);
        BindButton(primaryButton, primaryButtonLabel, null, null, false);
        BindButton(rightButton, rightButtonLabel, null, null, false);
    }

    private bool ShowContent(
        EquipmentItemData item,
        EquipmentDataSO definition,
        DisplayMode mode,
        Action onDismissed)
    {
        if (item == null || definition == null)
            return false;

        CurrentMode = mode;
        dismissed = onDismissed;
        if (headerLabel != null) headerLabel.text = "장비";
        rarityHeader.color = RarityPalette.Get(definition.Rarity);
        rarityText.text = RarityName(definition.Rarity);
        itemNameText.text = definition.ItemName;
        equipmentSlot.Bind(item, definition, false, null);
        SetIcon(baseEffectIcon, GetStatIcon(definition.BaseStatType));
        baseEffectText.text = FormatBase(item, definition);
        descriptionText.text = definition.Description;

        bool hasBonus = item.Level >= EquipmentBalanceConfigSO.MaxLevel && item.BonusOption != null;
        bool showBonus = hasBonus && bonusEffectRoot != null && bonusEffectText != null;
        if (bonusEffectRoot != null) bonusEffectRoot.SetActive(showBonus);
        if (showBonus)
        {
            SetIcon(bonusEffectIcon, GetStatIcon(item.BonusOption.StatType));
            bonusEffectText.text = FormatBonus(item.BonusOption);
        }

        Lifecycle.Show();
        return true;
    }

    public void Dismiss()
    {
        Lifecycle.Close(CompleteDismiss);
    }

    private void CompleteDismiss()
    {
        Action callback = dismissed;
        dismissed = null;
        callback?.Invoke();
    }

    public void Close(Action onClosed = null)
    {
        dismissed = null;
        Lifecycle.Close(onClosed);
    }

    public void HideImmediately()
    {
        dismissed = null;
        Lifecycle.HideImmediately();
    }

    private static void BindButton(Button button, TMP_Text buttonLabel, string label, Action action, bool interactable)
    {
        if (button == null) return;
        bool visible = !string.IsNullOrWhiteSpace(label);
        button.gameObject.SetActive(visible);
        button.onClick.RemoveAllListeners();
        button.interactable = visible && interactable;
        if (buttonLabel != null) buttonLabel.text = label ?? string.Empty;
        if (visible && action != null) button.onClick.AddListener(() => action());
    }

    private static void SetIcon(Image image, Sprite sprite)
    {
        if (image == null) return;
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.enabled = sprite != null;
    }

    private Sprite GetStatIcon(EquipmentStatType type) => type switch
    {
        EquipmentStatType.Attack => attackStatIcon,
        EquipmentStatType.CritChance => criticalStatIcon,
        EquipmentStatType.MaxHp => maxHealthStatIcon,
        EquipmentStatType.DamageReduction => damageReductionStatIcon,
        _ => null
    };

    private static string FormatBase(EquipmentItemData item, EquipmentDataSO definition)
    {
        string stat = definition.BaseStatType == EquipmentStatType.MaxHp ? "최대 체력" : "공격력";
        return $"{stat}  +{GameConfig.EquipmentBalance.GetBaseStatValue(definition, item.Level):N0}";
    }

    private static string FormatBonus(EquipmentBonusOptionData bonus)
    {
        string stat = bonus.StatType switch
        {
            EquipmentStatType.MaxHp => "최대 체력",
            EquipmentStatType.Attack => "공격력",
            EquipmentStatType.CritChance => "치명타 확률",
            _ => "받는 피해 감소"
        };
        return $"{stat}  +{bonus.Value:0.#}{(bonus.ModifierType == EquipmentModifierType.Percent ? "%" : string.Empty)}";
    }

    private static string RarityName(Rarity rarity) => rarity switch
    {
        Rarity.Legend => "전설",
        Rarity.Epic => "에픽",
        Rarity.Rare => "희귀",
        _ => "일반"
    };
}
