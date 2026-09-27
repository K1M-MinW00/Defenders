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
            if (!RewardPresentationResolver.TryResolve(reward, icons, out RewardPresentation presentation))
                continue;

            CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);
            slot.Setup(
                presentation.Icon,
                presentation.Frame,
                reward.Amount,
                presentation.ShowAmount,
                null);
        }
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
