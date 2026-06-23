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

    public void Setup(MailData mail, Action<MailData> onClick)
    {
        currentMail = mail;

        titleText.text = mail.Title;

        DateTime expireTime = mail.ExpireAt.ToDateTime();
        TimeSpan remain = expireTime - DateTime.UtcNow;

        expireText.text = GetExpireText(remain);

        button.interactable = !mail.Claimed && remain.TotalSeconds > 0;

        CreateRewardSlots(mail);

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            if (currentMail.Claimed)
                return;

            onClick?.Invoke(mail);
        });
    }

    private void CreateRewardSlots(MailData mail)
    {
        ClearRewardSlots();

        foreach (RewardData reward in mail.Rewards)
        {
            switch (reward.Type)
            {
                case RewardType.Gold:
                    CreateResourceSlot(GameIconDatabase.GetResourceIcon(RewardType.Gold), reward.Amount);
                    break;

                case RewardType.Gem:
                    CreateResourceSlot(GameIconDatabase.GetResourceIcon(RewardType.Gem), reward.Amount);
                    break;

                case RewardType.Fuel:
                    CreateResourceSlot(GameIconDatabase.GetResourceIcon(RewardType.Fuel), reward.Amount);
                    break;

                case RewardType.Item:
                    CreateItemSlot(reward);
                    break;

                case RewardType.Unit:
                    CreateUnitSlot(reward);
                    break;

                case RewardType.Equipment:
                    CreateEquipmentSlot(reward);
                    break;
            }
        }
    }

    private void CreateResourceSlot(Sprite icon, int amount)
    {
        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup(icon, amount, true, Rarity.Normal, null);
    }

    private void CreateItemSlot(RewardData reward)
    {
        ItemDataSO itemData = ItemDatabase.Get(reward.Id);

        if (itemData == null)
            return;

        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup(itemData.Icon, reward.Amount, itemData.Stackable, itemData.Rarity, null);
    }

    private void CreateUnitSlot(RewardData reward)
    {
        UnitDataSO unitData = UnitDatabase.Get(reward.Id);

        if (unitData == null)
            return;

        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup(unitData.icon, 1, false, unitData.rarity, null);
    }

    private void CreateEquipmentSlot(RewardData reward)
    {
        ItemDataSO equipmentData = ItemDatabase.Get(reward.Id);

        if (equipmentData == null)
            return;

        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup(equipmentData.Icon, 1, false, equipmentData.Rarity, null);
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