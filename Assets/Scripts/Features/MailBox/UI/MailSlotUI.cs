using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MailSlotUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text expireText;

    [Header("Rewards")]
    [SerializeField] private Transform rewardRoot;
    [SerializeField] private CommonSlotUI slotPrefab;

    [Header("Buttons")]
    [SerializeField] private Button button;

    private readonly List<CommonSlotUI> rewardSlots = new();
    private MailData currentMail;
    private Action<MailData> clicked;
    private bool canClaim;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(HandleClicked);
    }

    public void Setup(MailData mail, Action<MailData> onClick, IGameIconProvider icons)
    {
        if (mail == null)
        {
            Clear();
            return;
        }

        currentMail = mail;
        clicked = onClick;
        titleText.text = mail.Title;

        DateTime expireTime = mail.ExpireAt.ToDateTime();
        TimeSpan remain = expireTime - DateTime.UtcNow;
        expireText.text = GetExpireText(remain);

        canClaim = !mail.Claimed && remain.TotalSeconds > 0;
        SetInteractionEnabled(true);
        ReconcileRewardSlots(mail, icons);
    }

    public void SetInteractionEnabled(bool enabled)
    {
        if (button != null)
            button.interactable = enabled && canClaim;
    }

    public void Clear()
    {
        currentMail = null;
        clicked = null;
        canClaim = false;

        if (titleText != null)
            titleText.text = string.Empty;
        if (expireText != null)
            expireText.text = string.Empty;
        if (button != null)
            button.interactable = false;

        HideRewardsFrom(0);
    }

    private void HandleClicked()
    {
        if (currentMail == null || currentMail.Claimed || !canClaim)
            return;

        clicked?.Invoke(currentMail);
    }

    private void ReconcileRewardSlots(MailData mail, IGameIconProvider icons)
    {
        int visibleCount = 0;

        if (mail.Rewards != null)
        {
            foreach (RewardData reward in mail.Rewards)
            {
                if (!RewardPresentationResolver.TryResolve(reward, icons, out RewardPresentation presentation))
                    continue;

                CommonSlotUI slot = GetOrCreateRewardSlot(visibleCount++);
                slot.gameObject.SetActive(true);
                slot.SetupPresentation(
                    presentation,
                    reward.Amount,
                    () => ItemTooltipUI.Show(presentation.DisplayName, slot.transform as RectTransform));
            }
        }

        HideRewardsFrom(visibleCount);
    }

    private CommonSlotUI GetOrCreateRewardSlot(int index)
    {
        if (index < rewardSlots.Count)
            return rewardSlots[index];

        CommonSlotUI slot = Instantiate(slotPrefab, rewardRoot);
        rewardSlots.Add(slot);
        return slot;
    }

    private void HideRewardsFrom(int firstHiddenIndex)
    {
        for (int i = firstHiddenIndex; i < rewardSlots.Count; i++)
        {
            rewardSlots[i].Clear();
            rewardSlots[i].gameObject.SetActive(false);
        }
    }

    private static string GetExpireText(TimeSpan remain)
    {
        if (remain.TotalSeconds <= 0)
            return "만료됨";

        if (remain.TotalDays >= 1)
            return $"{Mathf.CeilToInt((float)remain.TotalDays)}일 후 만료";

        if (remain.TotalHours >= 1)
            return $"{Mathf.CeilToInt((float)remain.TotalHours)}시간 후 만료";

        if (remain.TotalMinutes >= 1)
            return $"{Mathf.CeilToInt((float)remain.TotalMinutes)}분 후 만료";

        return "곧 만료";
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClicked);
    }
}
