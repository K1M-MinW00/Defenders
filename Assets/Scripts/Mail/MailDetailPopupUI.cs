using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailDetailPopupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;

    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private Button claimButton;


    public void Show(MailData mail,System.Action<MailData> onClaim)
    {
        gameObject.SetActive(true);

        titleText.text = mail.Title;

        descriptionText.text = mail.Description;

        claimButton.interactable = !mail.Claimed;

        claimButton.onClick.RemoveAllListeners();

        claimButton.onClick.AddListener(() =>
        {
            onClaim?.Invoke(mail);
        });
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}