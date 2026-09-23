using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class MailboxPanelView : MonoBehaviour
{
    public void Open() => gameObject.SetActive(true);
    public void Close() => gameObject.SetActive(false);

    [Header("Mail List")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private MailSlotUI mailSlotPrefab;

    [Header("Buttons")]
    [SerializeField] private Button receiveAllButton;
    [SerializeField] private Button deleteAllButton;

    private MailboxService mailboxService;
    private readonly Dictionary<string, MailSlotUI> slotsByMailId = new();
    private readonly List<string> staleMailIds = new();
    private bool isProcessing;

    private void Awake()
    {
        receiveAllButton.onClick.AddListener(HandleReceiveAllButtonClicked);
        deleteAllButton.onClick.AddListener(HandleDeleteAllButtonClicked);
    }

    private async void OnEnable()
    {
        mailboxService = UserDataManager.Instance.MailboxService;

        SetProcessing(true);

        try
        {
            await RefreshAsync();
        }
        catch (Exception exception)
        {
            Debug.LogError($"[MailboxPanelView] Load failed: {exception}");
            UIFeedbackToast.Show(LobbyOperationFeedbackMessages.MailboxLoadFailed);
        }
        finally
        {
            SetProcessing(false);
        }
    }

    public async Task RefreshAsync()
    {
        await mailboxService.LoadMailsAsync();
        ReconcileSlots(mailboxService.CachedMails);
        RefreshButtonStates();
    }

    private void ReconcileSlots(IReadOnlyList<MailData> mails)
    {
        HashSet<string> visibleMailIds = new();

        for (int i = 0; i < mails.Count; i++)
        {
            MailData mail = mails[i];
            if (mail == null || string.IsNullOrWhiteSpace(mail.MailId) || !visibleMailIds.Add(mail.MailId))
                continue;

            MailSlotUI slot = GetOrCreateSlot(mail.MailId);
            slot.transform.SetSiblingIndex(i);
            slot.Setup(mail, HandleMailClicked, GameConfig.Icons);
            slot.SetInteractionEnabled(!isProcessing);
        }

        RemoveStaleSlots(visibleMailIds);
    }

    private MailSlotUI GetOrCreateSlot(string mailId)
    {
        if (slotsByMailId.TryGetValue(mailId, out MailSlotUI slot) && slot != null)
            return slot;

        slot = Instantiate(mailSlotPrefab, contentRoot);
        slotsByMailId[mailId] = slot;
        return slot;
    }

    private void RemoveStaleSlots(HashSet<string> visibleMailIds)
    {
        staleMailIds.Clear();

        foreach (string mailId in slotsByMailId.Keys)
        {
            if (!visibleMailIds.Contains(mailId))
                staleMailIds.Add(mailId);
        }

        foreach (string mailId in staleMailIds)
        {
            MailSlotUI slot = slotsByMailId[mailId];
            if (slot != null)
            {
                slot.gameObject.SetActive(false);
                Destroy(slot.gameObject);
            }

            slotsByMailId.Remove(mailId);
        }

        staleMailIds.Clear();
    }

    private async void HandleMailClicked(MailData mail)
    {
        if (isProcessing)
            return;

        SetProcessing(true);

        try
        {
            MailboxClaimResult result = await UserDataManager.Instance.ClaimMailAsync(mail);

            if (!result.Succeeded)
            {
                Debug.LogWarning($"[MailboxPanelView] Claim failed: {result.Failure}");
                UIFeedbackToast.Show(LobbyOperationFeedbackMessages.Get(result.Failure));
            }
            await RefreshAsync();
        }
        catch (Exception exception)
        {
            Debug.LogError($"[MailboxPanelView] Claim refresh failed: {exception}");
            UIFeedbackToast.Show(LobbyOperationFeedbackMessages.MailboxLoadFailed);
        }
        finally
        {
            SetProcessing(false);
        }
    }

    private async void HandleReceiveAllButtonClicked()
    {
        if (isProcessing)
            return;

        SetProcessing(true);

        try
        {
            MailboxClaimResult result = await UserDataManager.Instance.ClaimAllMailAsync();

            if (!result.Succeeded)
            {
                if (result.Failure != MailboxClaimFailure.NoClaimableMail)
                    Debug.LogWarning($"[MailboxPanelView] Claim all failed: {result.Failure}");

                UIFeedbackToast.Show(LobbyOperationFeedbackMessages.Get(result.Failure));
            }
            await RefreshAsync();
        }
        catch (Exception exception)
        {
            Debug.LogError($"[MailboxPanelView] Claim all refresh failed: {exception}");
            UIFeedbackToast.Show(LobbyOperationFeedbackMessages.MailboxLoadFailed);
        }
        finally
        {
            SetProcessing(false);
        }
    }

    private async void HandleDeleteAllButtonClicked()
    {
        if (isProcessing)
            return;

        SetProcessing(true);

        try
        {
            await mailboxService.DeleteAllAsync();
            await RefreshAsync();
        }
        catch (Exception exception)
        {
            Debug.LogError($"[MailboxPanelView] Delete failed: {exception}");
            UIFeedbackToast.Show(LobbyOperationFeedbackMessages.MailboxDeleteFailed);
        }
        finally
        {
            SetProcessing(false);
        }
    }

    private void SetProcessing(bool processing)
    {
        isProcessing = processing;

        foreach (MailSlotUI slot in slotsByMailId.Values)
        {
            if (slot != null)
                slot.SetInteractionEnabled(!processing);
        }

        RefreshButtonStates();
    }

    private void RefreshButtonStates()
    {
        bool hasClaimableMail = false;
        bool hasClaimedMail = false;
        DateTime now = DateTime.UtcNow;

        if (mailboxService?.CachedMails != null)
        {
            foreach (MailData mail in mailboxService.CachedMails)
            {
                if (mail == null)
                    continue;

                hasClaimedMail |= mail.Claimed;
                hasClaimableMail |= !mail.Claimed && mail.ExpireAt != null &&
                    mail.ExpireAt.ToDateTime() > now;
            }
        }

        if (receiveAllButton != null)
            receiveAllButton.interactable = !isProcessing && hasClaimableMail;

        if (deleteAllButton != null)
            deleteAllButton.interactable = !isProcessing && hasClaimedMail;
    }

    private void OnDestroy()
    {
        if (receiveAllButton != null)
            receiveAllButton.onClick.RemoveListener(HandleReceiveAllButtonClicked);

        if (deleteAllButton != null)
            deleteAllButton.onClick.RemoveListener(HandleDeleteAllButtonClicked);

        slotsByMailId.Clear();
    }
}
