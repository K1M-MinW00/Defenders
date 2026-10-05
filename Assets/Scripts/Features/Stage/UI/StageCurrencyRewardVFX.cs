using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageCurrencyRewardVFX : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private RectTransform effectsRoot;
    [SerializeField] private RectTransform currencyTarget;
    [SerializeField] private Image[] currencyIcons;
    [SerializeField] private TMP_Text gainText;
    [SerializeField] private CanvasGroup gainTextGroup;

    [Header("Gain Text Colors")]
    [SerializeField] private Color waveRewardTextColor = Color.white;
    [SerializeField] private Color bonusRewardTextColor = new(1f, 0.82f, 0.18f, 1f);

    [Header("Burst")]
    [SerializeField, Min(1)] private int minimumIconCount = 5;
    [SerializeField, Min(1)] private int goldPerAdditionalIcon = 3;
    [SerializeField] private Vector2 scatterRadius = new(110f, 230f);
    [SerializeField, Min(0.01f)] private float scatterDuration = 0.3f;
    [SerializeField, Min(0f)] private float gatherDelay = 0.45f;

    [Header("Gather And Collect")]
    [SerializeField] private Vector2 centerGatherRadius = new(34f, 82f);
    [SerializeField, Min(0.01f)] private float centerGatherDuration = 0.28f;
    [SerializeField, Min(0f)] private float centerHoldDuration = 0.08f;
    [SerializeField, Min(0.01f)] private float collectDuration = 0.42f;
    [SerializeField, Min(0f)] private float collectInterval = 0.055f;

    private const float GainTextAnimationDuration = 0.92f;

    private Tween rewardTween;
    private Tween targetTween;
    private Tween gainTween;
    private TaskCompletionSource<bool> rewardCompletion;
    private Vector3 targetBaseScale = Vector3.one;
    private Vector2 gainTextBasePosition;
    private Sprite[] baseCurrencySprites;

    private void Awake()
    {
        if (currencyTarget != null)
            targetBaseScale = currencyTarget.localScale;
        if (gainText != null)
            gainTextBasePosition = gainText.rectTransform.anchoredPosition;
        if (currencyIcons != null)
        {
            baseCurrencySprites = new Sprite[currencyIcons.Length];
            for (int i = 0; i < currencyIcons.Length; i++)
                baseCurrencySprites[i] = currencyIcons[i] != null ? currencyIcons[i].sprite : null;
        }

        ResetVisuals();
    }

    public void Play(int amount)
    {
        _ = PlayAsync(amount);
    }

    public Task PlayAsync(int amount)
    {
        return PlayWaveRewardAsync(amount);
    }

    public Task PlayWaveRewardAsync(int amount)
    {
        if (amount <= 0 || effectsRoot == null || currencyTarget == null ||
            currencyIcons == null || currencyIcons.Length == 0)
            return Task.CompletedTask;

        StopAnimation();
        ResetVisuals();

        Vector2 targetPosition = effectsRoot.InverseTransformPoint(currencyTarget.position);
        int extraIcons = Mathf.CeilToInt(amount / (float)Mathf.Max(1, goldPerAdditionalIcon));
        int iconCount = Mathf.Clamp(extraIcons, minimumIconCount, currencyIcons.Length);
        Sequence sequence = DOTween.Sequence().BindTo(this, useUnscaledTime: true);

        float centerGatherTime = scatterDuration + gatherDelay;
        float firstCollectTime = centerGatherTime + centerGatherDuration + centerHoldDuration;
        float finalCollectTime = firstCollectTime + (iconCount - 1) * collectInterval + collectDuration;
        TaskCompletionSource<bool> completion = new();
        rewardCompletion = completion;

        for (int i = 0; i < iconCount; i++)
        {
            Image icon = currencyIcons[i];
            if (icon == null)
                continue;

            RectTransform iconRect = icon.rectTransform;
            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.01f)
                direction = Vector2.up;

            float radius = Random.Range(scatterRadius.x, scatterRadius.y);
            Vector2 scatterPosition = direction * radius;
            Vector2 gatherDirection = Random.insideUnitCircle.normalized;
            if (gatherDirection.sqrMagnitude < 0.01f)
                gatherDirection = direction;
            Vector2 gatherPosition = gatherDirection *
                                     Random.Range(centerGatherRadius.x, centerGatherRadius.y);
            float burstStart = i * 0.018f;
            float collectStart = firstCollectTime + i * collectInterval;

            icon.gameObject.SetActive(true);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.localScale = Vector3.one * 0.62f;

            sequence.Insert(
                burstStart,
                iconRect.DOAnchorPos(scatterPosition, scatterDuration).SetEase(Ease.OutBack));
            sequence.Insert(
                burstStart,
                iconRect.DOScale(1f, scatterDuration * 0.7f).SetEase(Ease.OutBack));
            sequence.Insert(
                centerGatherTime,
                iconRect.DOAnchorPos(gatherPosition, centerGatherDuration).SetEase(Ease.InOutCubic));
            sequence.Insert(
                collectStart,
                iconRect.DOAnchorPos(targetPosition, collectDuration).SetEase(Ease.InCubic));
            sequence.Insert(
                collectStart + collectDuration * 0.45f,
                iconRect.DOScale(0.45f, collectDuration * 0.55f).SetEase(Ease.InQuad));

            Image capturedIcon = icon;
            sequence.InsertCallback(collectStart + collectDuration, () =>
            {
                capturedIcon.gameObject.SetActive(false);
                PulseCurrencyTarget();
            });
        }

        sequence.InsertCallback(
            finalCollectTime,
            () => ShowGainText($"+ {amount}", waveRewardTextColor));
        sequence.AppendInterval(GainTextAnimationDuration);
        rewardTween = sequence;
        sequence.OnComplete(() => CompleteRewardAnimation(sequence, completion));
        sequence.OnKill(() => CompleteRewardAnimation(sequence, completion));
        return completion.Task;
    }

    public Task PlayBonusRewardAsync(int amount)
    {
        if (amount <= 0 || gainText == null || gainTextGroup == null)
            return Task.CompletedTask;

        StopAnimation();
        ResetVisuals();

        Sequence sequence = DOTween.Sequence().BindTo(this, useUnscaledTime: true);
        TaskCompletionSource<bool> completion = new();
        rewardCompletion = completion;
        sequence.AppendCallback(() => ShowGainText($"Bonus + {amount}", bonusRewardTextColor));
        sequence.AppendInterval(GainTextAnimationDuration);
        rewardTween = sequence;
        sequence.OnComplete(() => CompleteRewardAnimation(sequence, completion));
        sequence.OnKill(() => CompleteRewardAnimation(sequence, completion));
        return completion.Task;
    }

    private void CompleteRewardAnimation(Tween completedTween, TaskCompletionSource<bool> completion)
    {
        if (rewardTween == completedTween)
        {
            rewardTween = null;
            rewardCompletion = null;
        }

        completion.TrySetResult(true);
    }

    private void PulseCurrencyTarget()
    {
        TweenLifecycle.Kill(ref targetTween);
        currencyTarget.localScale = targetBaseScale;

        Tween createdTween = currencyTarget
            .DOPunchScale(Vector3.one * 0.14f, 0.2f, 4, 0.55f)
            .BindTo(this, useUnscaledTime: true);

        targetTween = createdTween;
        createdTween.OnComplete(() =>
        {
            currencyTarget.localScale = targetBaseScale;
            if (targetTween == createdTween)
                targetTween = null;
        });
    }

    private void ShowGainText(string message, Color color)
    {
        if (gainText == null || gainTextGroup == null)
            return;

        TweenLifecycle.Kill(ref gainTween);
        gainText.text = message;
        gainText.color = color;
        Vector2 basePosition = gainTextBasePosition;
        gainText.rectTransform.anchoredPosition = basePosition;
        gainText.rectTransform.localScale = Vector3.one * 0.82f;
        gainTextGroup.alpha = 0f;
        gainText.gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence()
            .Append(gainTextGroup.DOFade(1f, 0.12f))
            .Join(gainText.rectTransform.DOScale(1f, 0.18f).SetEase(Ease.OutBack))
            .AppendInterval(0.55f)
            .Append(gainTextGroup.DOFade(0f, 0.25f))
            .Join(gainText.rectTransform.DOAnchorPosY(
                basePosition.y + 24f,
                0.25f).SetEase(Ease.InQuad))
            .BindTo(this, useUnscaledTime: true);

        gainTween = sequence;
        sequence.OnComplete(() =>
        {
            gainText.gameObject.SetActive(false);
            gainText.rectTransform.anchoredPosition = basePosition;
            if (gainTween == sequence)
                gainTween = null;
        });
    }

    private void OnDisable()
    {
        StopAnimation();
        ResetVisuals();
    }

    private void StopAnimation()
    {
        TweenLifecycle.Kill(ref rewardTween);
        TweenLifecycle.Kill(ref targetTween);
        TweenLifecycle.Kill(ref gainTween);
        rewardCompletion?.TrySetResult(true);
        rewardCompletion = null;
    }

    private void ResetVisuals()
    {
        if (currencyIcons != null)
        {
            for (int i = 0; i < currencyIcons.Length; i++)
            {
                Image icon = currencyIcons[i];
                if (icon == null)
                    continue;

                icon.gameObject.SetActive(false);
                icon.rectTransform.anchoredPosition = Vector2.zero;
                icon.rectTransform.localScale = Vector3.one;
                if (baseCurrencySprites != null && i < baseCurrencySprites.Length)
                    icon.sprite = baseCurrencySprites[i];
            }
        }

        if (gainText != null)
        {
            gainText.gameObject.SetActive(false);
            gainText.rectTransform.anchoredPosition = gainTextBasePosition;
        }
        if (gainTextGroup != null)
            gainTextGroup.alpha = 0f;
        if (currencyTarget != null)
            currencyTarget.localScale = targetBaseScale;
    }
}
