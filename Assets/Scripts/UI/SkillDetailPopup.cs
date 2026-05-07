using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetailPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Button closeButton;

    [SerializeField] private Image skillIconImage;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillDescriptionText;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        Close();
    }

    public void Open(SkillViewData skill)
    {
        if (skill == null)
            return;

        if (root != null)
            root.SetActive(true);
        else
            gameObject.SetActive(true);

        if (skillIconImage != null)
            skillIconImage.sprite = skill.Icon;

        if (skillNameText != null)
            skillNameText.text = skill.DisplayName;

        if (skillDescriptionText != null)
            skillDescriptionText.text = skill.Description;
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);
        else
            gameObject.SetActive(false);
    }
}