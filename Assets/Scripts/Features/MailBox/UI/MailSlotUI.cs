using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailSlotUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text expireText;

    [Header("Rewards")]
    [SerializeField] private Transform rewardRoot;
    [SerializeField] private CommonSlotUI slotPrefab;

    [Header("Buttons")]
    [SerializeField] private Button button;

    private MailData currentMail;
    private bool canClaim;

    public void Setup(MailData mail, Action<MailData> onClick, IGameIconProvider icons)
    {
        currentMail = mail;

        titleText.text = mail.Title;

        DateTime expireTime = mail.ExpireAt.ToDateTime();
        TimeSpan remain = expireTime - DateTime.UtcNow;

        expireText.text = GetExpireText(remain);

        canClaim = !mail.Claimed && remain.TotalSeconds > 0;
        SetInteractionEnabled(true);

        CreateRewardSlots(mail, icons);

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            if (currentMail.Claimed)
                return;

            onClick?.Invoke(currentMail);
        });
    }

    public void SetInteractionEnabled(bool enabled)
    {
        if (button != null)
            button.interactable = enabled && canClaim;
    }

    private void CreateRewardSlots(MailData mail, IGameIconProvider icons)
    {
        ClearRewardSlots();

        if (mail.Rewards == null)
            return;

        foreach (RewardData reward in mail.Rewards)
        {
            switch (reward.Type)
            {
                case RewardType.Gold:
                    CreateResourceSlot(icons, RewardType.Gold, reward.Amount);
                    break;

                case RewardType.Gem:
                    CreateResourceSlot(icons, RewardType.Gem, reward.Amount);
                    break;

                case RewardType.Fuel:
                    CreateResourceSlot(icons, RewardType.Fuel, reward.Amount);
                    break;

                case RewardType.Item:
                    CreateItemSlot(reward, icons);
                    break;

                case RewardType.Unit:
                    CreateUnitSlot(reward, icons);
                    break;

                case RewardType.Equipment:
                    CreateEquipmentSlot(reward, icons);
                    break;
            }
        }
    }

    private void CreateResourceSlot(IGameIconProvider icons, RewardType type, int amount)
    {
        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup(
            icons.GetResourceIcon(type),
            icons.GetRarityFrame(Rarity.Normal),
            amount,
            true,
            null);
    }

    private void CreateItemSlot(RewardData reward, IGameIconProvider icons)
    {
        ItemDataSO itemData = GameConfig.Items.Get(reward.Id);

        if (itemData == null)
            return;

        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup(
            itemData.Icon,
            icons.GetRarityFrame(itemData.Rarity),
            reward.Amount,
            itemData.Stackable,
            null);
    }

    private void CreateUnitSlot(RewardData reward, IGameIconProvider icons)
    {
        UnitDataSO unitData = GameConfig.Units.Get(reward.Id);

        if (unitData == null)
            return;

        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup(unitData.icon, icons.GetRarityFrame(unitData.rarity), 1, false, null);
    }

    private void CreateEquipmentSlot(RewardData reward, IGameIconProvider icons)
    {
        ItemDataSO equipmentData = GameConfig.Items.Get(reward.Id);

        if (equipmentData == null)
            return;

        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup(equipmentData.Icon, icons.GetRarityFrame(equipmentData.Rarity), 1, false, null);
    }

    private void ClearRewardSlots()
    {
        for (int i = rewardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(rewardRoot.GetChild(i).gameObject);
        }
    }

    private string GetExpireText(TimeSpan remain)
    {
        if (remain.TotalSeconds <= 0)
        {
            return "만료됨";
        }

        if (remain.TotalDays >= 1)
        {
            return $"{Mathf.CeilToInt((float)remain.TotalDays)}일 후 만료";
        }

        if (remain.TotalHours >= 1)
        {
            return $"{Mathf.CeilToInt((float)remain.TotalHours)}시간 후 만료";
        }

        if (remain.TotalMinutes >= 1)
        {
            return $"{Mathf.CeilToInt((float)remain.TotalMinutes)}분 후 만료";
        }

        return "곧 만료";
    }
}
