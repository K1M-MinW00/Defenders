using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MailboxPopupUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private MailSlotUI mailSlotPrefab;

    [SerializeField] private Button receiveAllButton;
    [SerializeField] private Button deleteAllButton;

    private MailboxService mailboxService;

    private async void OnEnable()
    {
        mailboxService = UserDataManager.Instance.MailboxService;

        receiveAllButton.onClick.RemoveAllListeners();
        deleteAllButton.onClick.RemoveAllListeners();

        receiveAllButton.onClick.AddListener(
            OnClickReceiveAll);

        deleteAllButton.onClick.AddListener(
            OnClickDeleteAll);

        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        ClearSlots();

        string uid = AuthManager.Instance.CurrentUser.UserId;

        await mailboxService.LoadMailsAsync(uid);

        foreach (MailData mail in mailboxService.CachedMails)
        {
            CreateSlot(mail);
        }
    }

    private void CreateSlot(MailData mail)
    {
        MailSlotUI slot = Instantiate(mailSlotPrefab, contentRoot);

        slot.Setup(mail,OnClickMail);
    }

    private async void OnClickMail(MailData mail)
    {
        string uid = AuthManager.Instance.CurrentUser.UserId;

        await mailboxService.ClaimMailAsync(uid, mail);

        await RefreshAsync();
    }

    private void ClearSlots()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }
    private async void OnClickReceiveAll()
    {
        string uid = AuthManager.Instance.CurrentUser.UserId;

        await mailboxService .ClaimAllAsync(uid);

        await RefreshAsync();
    }

    private async void OnClickDeleteAll()
    {
        string uid = AuthManager.Instance.CurrentUser.UserId;

        await mailboxService.DeleteAllAsync(uid);

        await RefreshAsync();
    }
}