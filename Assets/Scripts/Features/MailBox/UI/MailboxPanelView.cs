using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MailboxPanelView : MonoBehaviour
{
    [Header("Mail List")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private MailSlotUI mailSlotPrefab;

    [Header("Buttons")]
    [SerializeField] private Button receiveAllButton;
    [SerializeField] private Button deleteAllButton;

    private MailboxService mailboxService;
    private bool isProcessing;

    private void Awake()
    {
        receiveAllButton.onClick.AddListener(HandleReceivceAllButtonClicked);
        deleteAllButton.onClick.AddListener(HandleDeleteAllButtonClicked);
    }

    private async void OnEnable()
    {
        mailboxService = UserDataManager.Instance.MailboxService;

        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        ClearSlots();

        string uid = AuthService.Instance.CurrentUser.UserId;

        await mailboxService.LoadMailsAsync();

        foreach (MailData mail in mailboxService.CachedMails)
        {
            CreateMailSlot(mail);
        }
    }

    private void CreateMailSlot(MailData mail)
    {
        MailSlotUI slot = Instantiate(mailSlotPrefab, contentRoot);

        slot.Setup(mail,HandleMailClicked);
    }

    private async void HandleMailClicked(MailData mail)
    {
        if (isProcessing)
            return;

        isProcessing = true;

        string uid = AuthService.Instance.CurrentUser.UserId;

        await mailboxService.ClaimMailAsync(mail);

        await RefreshAsync();

        isProcessing = false;
    }

    private void ClearSlots()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }
    private async void HandleReceivceAllButtonClicked()
    {
        if (isProcessing)
            return;

        isProcessing = true;

        string uid = AuthService.Instance.CurrentUser.UserId;

        await mailboxService .ClaimAllAsync();

        await RefreshAsync();

        isProcessing = false;
    }

    private async void HandleDeleteAllButtonClicked()
    {
        if (isProcessing)
            return;

        string uid = AuthService.Instance.CurrentUser.UserId;

        await mailboxService.DeleteAllAsync();

        await RefreshAsync();

        isProcessing = false;
    }
}