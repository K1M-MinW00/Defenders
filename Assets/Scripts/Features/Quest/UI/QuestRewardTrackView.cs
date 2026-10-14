using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class QuestRewardTrackView : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Slider scoreSlider;
    [SerializeField] private QuestRewardPeriod period;
    [SerializeField] private int maximumScore = 100;
    [SerializeField] private int currentScore;
    [SerializeField] private List<QuestRewardView> rewards = new();

    public void SetState(int score, IReadOnlyCollection<int> claimed)
    {
        currentScore = Mathf.Max(0, score);
        Refresh();
        foreach (QuestRewardView reward in rewards)
            if (reward != null)
            {
                reward.Configure(period, reward.Threshold, currentScore >= reward.Threshold,
                    claimed != null && claimed.Contains(reward.Threshold));
            }
    }

    public void Refresh()
    {
        int clamped = Mathf.Clamp(currentScore, 0, maximumScore);
        if (scoreText != null)
            scoreText.text = currentScore.ToString();
        if (scoreSlider != null)
        {
            scoreSlider.minValue = 0;
            scoreSlider.maxValue = maximumScore;
            scoreSlider.wholeNumbers = true;
            scoreSlider.SetValueWithoutNotify(clamped);
        }

        foreach (QuestRewardView reward in rewards)
        {
            if (reward == null)
                continue;

            RectTransform marker = reward.transform as RectTransform;
            float normalized = Mathf.Clamp01((float)reward.Threshold / maximumScore);
            marker.anchorMin = new Vector2(normalized, .5f);
            marker.anchorMax = new Vector2(normalized, .5f);
            marker.pivot = new Vector2(.5f, .5f);
            marker.anchoredPosition = Vector2.zero;
            reward.Configure(period, reward.Threshold, currentScore >= reward.Threshold, false);
        }
    }

    private void Awake() => Refresh();
}
