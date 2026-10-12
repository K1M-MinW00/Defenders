using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class QuestRewardView : MonoBehaviour
{
    [SerializeField] private Image chestImage;
    [SerializeField] private TMP_Text thresholdText;
    [SerializeField] private int threshold;
    [SerializeField] private Button button;
    private Action clickHandler;

    public int Threshold => threshold;
    public QuestRewardPeriod Period { get; private set; }

    public void Configure(int threshold, bool reached)
        => Configure(QuestRewardPeriod.Daily, threshold, reached, false);

    public void Configure(QuestRewardPeriod period, int threshold, bool reached, bool claimed)
    {
        Period = period;
        this.threshold = Mathf.Max(0, threshold);

        if (thresholdText != null)
            thresholdText.text = this.threshold.ToString();

        if (chestImage != null)
        {
            chestImage.sprite = GameConfig.QuestRewards?.GetIcon(period, claimed);
            chestImage.enabled = chestImage.sprite != null;
        }

        if (button != null) button.interactable = true;
    }

    public void SetClickHandler(Action handler)
    {
        if (button != null && clickHandler != null) button.onClick.RemoveListener(HandleClick);
        clickHandler = handler;
        if (button != null && clickHandler != null) button.onClick.AddListener(HandleClick);
    }

    private void HandleClick() => clickHandler?.Invoke();
    private void OnDestroy() { if (button != null) button.onClick.RemoveListener(HandleClick); }

}
