using DG.Tweening;
using UnityEngine;

public class DropSpawnView : MonoBehaviour
{
    [SerializeField] private Transform body;
    [SerializeField] private float height = 1.5f;
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 bodyBaseLocalPos;
    private Tween playTween;

    public bool IsPlaying => playTween != null && playTween.IsActive();

    private void Awake()
    {
        if (body == null)
            body = transform;
        bodyBaseLocalPos = body.localPosition;
    }

    private void OnEnable()
    {
        Replay();
    }

    private void OnDisable()
    {
        StopPlayback();
        ResetBody();
    }

    public void Replay()
    {
        if (!isActiveAndEnabled)
            return;

        StopPlayback();
        Vector3 startPos = bodyBaseLocalPos + Vector3.up * height;
        Vector3 endPos = bodyBaseLocalPos;
        Vector3 startScale = Vector3.one * 1.05f;

        body.localPosition = startPos;
        body.localScale = startScale;

        Sequence sequence = DOTween.Sequence()
            .Join(body.DOLocalMove(endPos, duration).SetEase(ease))
            .Join(body.DOScale(Vector3.one, duration).SetEase(ease))
            .BindTo(this);

        playTween = sequence;
        sequence.OnComplete(() =>
        {
            if (playTween == sequence)
                playTween = null;
        });
    }

    private void StopPlayback()
    {
        TweenLifecycle.Kill(ref playTween);
    }

    private void ResetBody()
    {
        if (body == null)
            return;

        body.localPosition = bodyBaseLocalPos;
        body.localScale = Vector3.one;
    }
}
