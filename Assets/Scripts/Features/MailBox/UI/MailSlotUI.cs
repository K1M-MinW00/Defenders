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
    private IGameIconProvider currentIcons;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(HandleClicked);
    }

    private void OnEnable() => LocalizationService.LocaleChanged += HandleLocaleChanged;
    private void OnDisable() => LocalizationService.LocaleChanged -= HandleLocaleChanged;

    public void Setup(MailData mail, Action<MailData> onClick, IGameIconProvider icons)
    {
        if (mail == null)
        {
            Clear();
            return;
        }

        currentMail = mail;
        clicked = onClick;
        currentIcons = icons;
        titleText.text = MailTitleResolver.Resolve(mail);

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
        currentIcons = null;
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
            return Localize("mail.expired");

        if (remain.TotalDays >= 1)
            return Localize("mail.expires_days", Mathf.CeilToInt((float)remain.TotalDays));

        if (remain.TotalHours >= 1)
            return Localize("mail.expires_hours", Mathf.CeilToInt((float)remain.TotalHours));

        if (remain.TotalMinutes >= 1)
            return Localize("mail.expires_minutes", Mathf.CeilToInt((float)remain.TotalMinutes));

        return Localize("mail.expires_soon");
    }

    private void HandleLocaleChanged(string _)
    {
        if (currentMail != null)
            Setup(currentMail, clicked, currentIcons);
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

    private void OnDestroy()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        if (button != null)
            button.onClick.RemoveListener(HandleClicked);
    }
}
