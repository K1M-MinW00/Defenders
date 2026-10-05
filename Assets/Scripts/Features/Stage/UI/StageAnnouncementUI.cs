using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public sealed class StageAnnouncementUI : MonoBehaviour
{
    [Header("Common")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform animatedRoot;

    [Header("Stage Intro")]
    [SerializeField] private GameObject stageIntroRoot;
    [SerializeField] private TMP_Text stageInfoText;

    [Header("Boss Wave")]
    [SerializeField] private GameObject bossWaveRoot;

    [Header("Timing")]
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.18f;
    [SerializeField, Min(0f)] private float holdDuration = 1f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.22f;

    private Tween announcementTween;
    private TaskCompletionSource<bool> announcementCompletion;

    private void Awake()
    {
        HideImmediate();
    }

    public Task ShowStageIntroAsync(StageDataSO stage)
    {
        if (stageInfoText != null)
            stageInfoText.SetText("{0}-{1}", stage?.sector ?? 0, stage?.stage ?? 0);

        return PlayAsync(showStageIntro: true);
    }

    public Task ShowBossWaveAsync()
    {
        return PlayAsync(showStageIntro: false);
    }

    private Task PlayAsync(bool showStageIntro)
    {
        StopAnimation();

        if (canvasGroup == null || animatedRoot == null)
            return Task.CompletedTask;

        stageIntroRoot?.SetActive(showStageIntro);
        bossWaveRoot?.SetActive(!showStageIntro);
        canvasGroup.alpha = 0f;
        animatedRoot.localScale = Vector3.one * 0.82f;
        gameObject.SetActive(true);

        TaskCompletionSource<bool> completion = new();
        announcementCompletion = completion;

        Sequence sequence = DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, fadeInDuration))
            .Join(animatedRoot.DOScale(1f, fadeInDuration).SetEase(Ease.OutBack))
            .AppendInterval(holdDuration)
            .Append(canvasGroup.DOFade(0f, fadeOutDuration))
            .Join(animatedRoot.DOScale(1.08f, fadeOutDuration).SetEase(Ease.InQuad))
            .BindTo(this, useUnscaledTime: true);

        announcementTween = sequence;
        sequence.OnComplete(() => CompleteAnimation(sequence, completion));
        sequence.OnKill(() => CompleteAnimation(sequence, completion));
        return completion.Task;
    }

    private void CompleteAnimation(Tween completedTween, TaskCompletionSource<bool> completion)
    {
        if (announcementTween == completedTween)
        {
            announcementTween = null;
            announcementCompletion = null;
            HideImmediate();
        }

        completion.TrySetResult(true);
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    private void StopAnimation()
    {
        TweenLifecycle.Kill(ref announcementTween);
        announcementCompletion?.TrySetResult(true);
        announcementCompletion = null;
    }

    private void HideImmediate()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        if (animatedRoot != null)
            animatedRoot.localScale = Vector3.one;
        stageIntroRoot?.SetActive(false);
        bossWaveRoot?.SetActive(false);
    }
}
