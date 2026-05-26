using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MailSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text expireText;

    [SerializeField] private Transform rewardRoot;
    [SerializeField] private CommonSlotUI slotPrefab;
    // [SerializeField] private InventorySlotUI itemSlotPrefab;



    [SerializeField] private Button button;

    private MailData currentMail;

    public void Setup(MailData mail, Action<MailData> onClick)
    {
        currentMail = mail;

        titleText.text = mail.Title;

        expireText.text = GetExpireText(mail.ExpireAt);
        button.interactable = !mail.Claimed;

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
                    CreateResourceSlot(GameIconDatabase.GetResourceIcon(RewardType.Gold), reward.Amount, "골드");
                    break;

                case RewardType.Gem:
                    CreateResourceSlot(GameIconDatabase.GetResourceIcon(RewardType.Gem),reward.Amount,"잼");
                    break;

                case RewardType.Fuel:
                    CreateResourceSlot(GameIconDatabase.GetResourceIcon(RewardType.Fuel),reward.Amount,"연료");
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

    private void CreateResourceSlot(Sprite icon, int amount, string rewardName)
    {
        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup( icon, amount, true, Rarity.Normal, null);
    }

    private void CreateItemSlot(RewardData reward)
    {
        ItemDataSO itemData = ItemDatabase.GetItem(reward.Id);

        if (itemData == null)
            return;

        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup(itemData.Icon,reward.Amount,itemData.Stackable,itemData.Rarity,null);
        // () => { itemDetailPopup.Show(itemData, reward.Amount); }
    }

    private void CreateUnitSlot(RewardData reward)
    {
        UnitDataSO unitData = UnitDatabase.GetUnit(reward.Id);

        if (unitData == null)
            return;

        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup(unitData.icon,1,false,unitData.rarity,null);
    }

    private void CreateEquipmentSlot(RewardData reward)
    {
        ItemDataSO equipmentData = ItemDatabase.GetItem(reward.Id);

        if (equipmentData == null)
            return;

        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);

        slot.Setup(equipmentData.Icon, 1,false,equipmentData.Rarity,null);
    }

    private void ClearRewardSlots()
    {
        for (int i = rewardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(rewardRoot.GetChild(i).gameObject);
        }
    }

    private string GetExpireText(long expireAt)
    {
        DateTime expireDate = DateTimeOffset.FromUnixTimeSeconds(expireAt).LocalDateTime;

        TimeSpan remain = expireDate - DateTime.Now;

        if (remain.TotalDays >= 1)
        {
            return $"{Mathf.CeilToInt((float)remain.TotalDays)}일 후 만료";
        }

        if (remain.TotalHours >= 1)
        {
            return $"{Mathf.CeilToInt((float)remain.TotalHours)}시간 후 만료";
        }

        return "곧 만료";
    }
}