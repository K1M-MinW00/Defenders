using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class QuestItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text pointText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionText;
    private Action action;

    public void Configure(int points, string description, int progress, int target, bool scoreClaimed, Action onAction)
    {
        target = Mathf.Max(1, target);
        progress = Mathf.Clamp(progress, 0, target);
        bool completed = progress >= target;

        if (pointText != null) pointText.text = points.ToString();
        if (descriptionText != null) descriptionText.text = description;
        if (progressText != null) progressText.text = $"{progress}/{target}";
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = target;
            progressSlider.wholeNumbers = true;
            progressSlider.SetValueWithoutNotify(progress);
        }
        action = onAction;
        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(HandleAction);
            actionButton.onClick.AddListener(HandleAction);
        }
        if (actionText != null) actionText.text = scoreClaimed ? "획득함" : completed ? "획득" : "이동";
        if (actionButton != null)
        {
            actionButton.interactable = !scoreClaimed;
            if (actionButton.targetGraphic is Image image)
                image.color = scoreClaimed
                    ? new Color(.35f, .38f, .39f, 1f)
                    : completed
                        ? new Color(.88f, .16f, .13f, 1f)
                        : new Color(.03f, .58f, .93f, 1f);
        }
    }

    private void HandleAction() => action?.Invoke();

    private void OnDestroy()
    {
        if (actionButton != null)
            actionButton.onClick.RemoveListener(HandleAction);
    }

#if UNITY_EDITOR
    public void SetReferences(TMP_Text points, TMP_Text description, Slider slider, TMP_Text progressLabel, Button action, TMP_Text actionLabel)
    {
        pointText = points;
        descriptionText = description;
        progressSlider = slider;
        progressText = progressLabel;
        actionButton = action;
        actionText = actionLabel;
    }
#endif
}
