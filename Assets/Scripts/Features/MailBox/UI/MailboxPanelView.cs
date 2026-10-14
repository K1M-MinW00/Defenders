using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[DisallowMultipleComponent]
public sealed class MailboxPanelView : MonoBehaviour, IModalDismissHandler
{
    private ModalOverlayLifecycle lifecycle;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    public void Open() => Lifecycle.Show();
    public void Close() => Lifecycle.Close();
    public void Dismiss() => Close();

    [Header("Mail List")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private MailSlotUI mailSlotPrefab;

    [Header("Buttons")]
    [SerializeField] private Button receiveAllButton;
    [SerializeField] private Button deleteAllButton;

    private MailboxService mailboxService;
    private readonly Dictionary<string, MailSlotUI> slotsByMailId = new();
    private readonly List<string> staleMailIds = new();
    private readonly Queue<MailSlotUI> availableSlots = new();
    private bool isProcessing;
    private int activationVersion;

    private void Awake()
    {
        receiveAllButton.onClick.AddListener(HandleReceiveAllButtonClicked);
        deleteAllButton.onClick.AddListener(HandleDeleteAllButtonClicked);
    }

    private async void OnEnable()
    {
        int currentActivation = ++activationVersion;
        UserDataManager userDataManager = UserDataManager.Instance;
        mailboxService = userDataManager != null ? userDataManager.MailboxService : null;
        if (mailboxService == null)
        {
            Debug.LogError("[MailboxPanelView] MailboxService is not ready.");
            return;
        }

        SetProcessing(true);

        try
        {
            await RefreshAsync(currentActivation);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[MailboxPanelView] Load failed: {exception}");
            UIFeedbackToast.Show(LobbyOperationFeedbackMessages.MailboxLoadFailed);
        }
        finally
        {
            if (currentActivation == activationVersion && isActiveAndEnabled)
                SetProcessing(false);
        }
    }

    private void OnDisable()
    {
        activationVersion++;
        isProcessing = false;
    }

    public async Task RefreshAsync()
    {
        await RefreshAsync(activationVersion);
    }

    private async Task RefreshAsync(int expectedActivation)
    {
        if (mailboxService == null)
            return;

        await mailboxService.LoadMailsAsync();
        if (!isActiveAndEnabled || expectedActivation != activationVersion)
            return;

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
            slot.gameObject.SetActive(true);
            slot.transform.SetSiblingIndex(i);
            slot.Setup(mail, HandleMailClicked, GameConfig.Icons);
            slot.SetInteractionEnabled(!isProcessing);
        }

        ReleaseStaleSlots(visibleMailIds);
    }

    private MailSlotUI GetOrCreateSlot(string mailId)
    {
        if (slotsByMailId.TryGetValue(mailId, out MailSlotUI slot) && slot != null)
            return slot;

        while (availableSlots.Count > 0 && slot == null)
            slot = availableSlots.Dequeue();

        if (slot == null)
            slot = Instantiate(mailSlotPrefab, contentRoot);

        slot.transform.SetParent(contentRoot, false);
        slotsByMailId[mailId] = slot;
        return slot;
    }

    private void ReleaseStaleSlots(HashSet<string> visibleMailIds)
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
                slot.Clear();
                slot.gameObject.SetActive(false);
                availableSlots.Enqueue(slot);
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
        availableSlots.Clear();
    }
}
