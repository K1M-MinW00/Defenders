using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DropSpawnView : MonoBehaviour
{
    [SerializeField] private Transform body;
    [SerializeField] private float height = 1.5f;
    [SerializeField] private float duration = 0.28f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Landing")]
    [SerializeField] private LineRenderer landingRing;
    [SerializeField, Min(0.01f)] private float squashDuration = 0.08f;
    [SerializeField, Min(0.01f)] private float recoverDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float ringDuration = 0.22f;

    [Header("Drag")]
    [SerializeField, Min(0f)] private float dragLiftHeight = 0.28f;
    [SerializeField, Min(0f)] private float dragSwayAngle = 6f;
    [SerializeField, Min(0.01f)] private float dragLiftDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float dragSwayDuration = 0.24f;

    [Header("Reroll")]
    [SerializeField, Min(0.01f)] private float rerollExitDuration = 0.18f;

    [Header("Skill Leap")]
    [SerializeField, Min(0f)] private float skillLeapHeight = 0.55f;
    [SerializeField, Min(0.01f)] private float skillLeapUpDuration = 0.16f;
    [SerializeField, Min(0.01f)] private float skillLeapDownDuration = 0.14f;

    private readonly List<SpriteRenderer> spriteRenderers = new();
    private readonly List<Color> spriteBaseColors = new();
    private Vector3 bodyBaseLocalPos;
    private Vector3 bodyBaseLocalScale;
    private Quaternion bodyBaseLocalRotation;
    private Tween playTween;
    private Tween ringTween;
    private Tween dragSwayTween;

    public bool IsPlaying => playTween != null && playTween.IsActive();

    private void Awake()
    {
        if (body == null)
            body = transform;

        bodyBaseLocalPos = body.localPosition;
        bodyBaseLocalScale = body.localScale;
        bodyBaseLocalRotation = body.localRotation;
        CacheSpriteColors();
        ResetLandingRing();
    }

    private void OnEnable() => Replay();

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
        RestoreSpriteColors();
        body.gameObject.SetActive(true);

        Vector3 startPos = bodyBaseLocalPos + Vector3.up * height;
        Vector3 squashScale = Vector3.Scale(bodyBaseLocalScale, new Vector3(1.16f, 0.82f, 1f));
        body.localPosition = startPos;
        body.localScale = Vector3.Scale(bodyBaseLocalScale, new Vector3(0.86f, 1.18f, 1f));

        Sequence sequence = DOTween.Sequence()
            .Join(body.DOLocalMove(bodyBaseLocalPos, duration).SetEase(ease))
            .Join(body.DOScale(bodyBaseLocalScale, duration).SetEase(Ease.InQuad))
            .AppendCallback(PlayLandingRing)
            .Append(body.DOScale(squashScale, squashDuration).SetEase(Ease.OutQuad))
            .Append(body.DOScale(bodyBaseLocalScale, recoverDuration).SetEase(Ease.OutBack))
            .BindTo(this);

        playTween = sequence;
        sequence.OnComplete(() =>
        {
            if (playTween == sequence)
                playTween = null;
        });
    }

    public void PrepareDeferredSpawn()
    {
        StopPlayback();
        ResetBody();
        SetSpriteAlpha(0f);
    }

    public void PlayRerollExit(Action onComplete)
    {
        if (!isActiveAndEnabled)
        {
            onComplete?.Invoke();
            return;
        }

        StopPlayback();
        Sequence sequence = DOTween.Sequence()
            .Join(body.DOScale(bodyBaseLocalScale * 0.72f, rerollExitDuration).SetEase(Ease.InBack))
            .Join(DOTween.To(() => 1f, SetSpriteAlpha, 0f, rerollExitDuration))
            .BindTo(this, useUnscaledTime: true);

        playTween = sequence;
        sequence.OnComplete(() =>
        {
            if (playTween == sequence)
                playTween = null;
            onComplete?.Invoke();
        });
    }

    public void BeginDragHold()
    {
        if (!isActiveAndEnabled)
            return;

        StopPlayback();
        RestoreSpriteColors();
        body.gameObject.SetActive(true);
        body.localRotation = bodyBaseLocalRotation * Quaternion.Euler(0f, 0f, -dragSwayAngle);

        Tween liftTween = body
            .DOLocalMove(bodyBaseLocalPos + Vector3.up * dragLiftHeight, dragLiftDuration)
            .SetEase(Ease.OutQuad)
            .BindTo(this, useUnscaledTime: true);
        playTween = liftTween;
        liftTween.OnComplete(() =>
        {
            if (playTween == liftTween)
                playTween = null;
        });

        dragSwayTween = body
            .DOLocalRotateQuaternion(
                bodyBaseLocalRotation * Quaternion.Euler(0f, 0f, dragSwayAngle),
                dragSwayDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .BindTo(this, useUnscaledTime: true);
    }

    public void PlayPlacementLanding()
    {
        if (!isActiveAndEnabled)
            return;

        StopPlayback();
        Vector3 squashScale = Vector3.Scale(bodyBaseLocalScale, new Vector3(1.16f, 0.82f, 1f));

        Sequence sequence = DOTween.Sequence()
            .Join(body.DOLocalMove(bodyBaseLocalPos, dragLiftDuration).SetEase(Ease.InQuad))
            .Join(body.DOLocalRotateQuaternion(bodyBaseLocalRotation, dragLiftDuration).SetEase(Ease.OutQuad))
            .AppendCallback(PlayLandingRing)
            .Append(body.DOScale(squashScale, squashDuration).SetEase(Ease.OutQuad))
            .Append(body.DOScale(bodyBaseLocalScale, recoverDuration).SetEase(Ease.OutBack))
            .BindTo(this, useUnscaledTime: true);

        playTween = sequence;
        sequence.OnComplete(() =>
        {
            if (playTween == sequence)
                playTween = null;
        });
    }

    public void PlayFusionUpgrade()
    {
        if (!isActiveAndEnabled)
            return;

        PlayLandingRing();
        body.DOPunchScale(bodyBaseLocalScale * 0.22f, 0.24f, 5, 0.55f)
            .BindTo(this, useUnscaledTime: true);
    }

    public void PlaySkillLeap(Action onLanded)
    {
        if (!isActiveAndEnabled || body == null)
        {
            onLanded?.Invoke();
            return;
        }

        StopPlayback();
        RestoreSpriteColors();
        body.gameObject.SetActive(true);
        body.localPosition = bodyBaseLocalPos;
        body.localRotation = bodyBaseLocalRotation;
        body.localScale = bodyBaseLocalScale;

        Vector3 apex = bodyBaseLocalPos + Vector3.up * skillLeapHeight;
        Vector3 stretchScale = Vector3.Scale(bodyBaseLocalScale, new Vector3(0.92f, 1.08f, 1f));
        Vector3 squashScale = Vector3.Scale(bodyBaseLocalScale, new Vector3(1.14f, 0.86f, 1f));

        Sequence sequence = DOTween.Sequence()
            .Join(body.DOLocalMove(apex, skillLeapUpDuration).SetEase(Ease.OutQuad))
            .Join(body.DOScale(stretchScale, skillLeapUpDuration).SetEase(Ease.OutQuad))
            .Append(body.DOLocalMove(bodyBaseLocalPos, skillLeapDownDuration).SetEase(Ease.InQuad))
            .Join(body.DOScale(bodyBaseLocalScale, skillLeapDownDuration).SetEase(Ease.InQuad))
            .AppendCallback(() =>
            {
                PlayLandingRing();
                onLanded?.Invoke();
            })
            .Append(body.DOScale(squashScale, squashDuration).SetEase(Ease.OutQuad))
            .Append(body.DOScale(bodyBaseLocalScale, recoverDuration).SetEase(Ease.OutBack))
            .BindTo(this);

        playTween = sequence;
        sequence.OnComplete(() =>
        {
            if (playTween == sequence)
                playTween = null;
        });
    }

    public void CancelSkillLeap()
    {
        StopPlayback();
        ResetBody();
    }

    private void StopPlayback()
    {
        TweenLifecycle.Kill(ref playTween);
        TweenLifecycle.Kill(ref ringTween);
        TweenLifecycle.Kill(ref dragSwayTween);
    }

    private void ResetBody()
    {
        if (body == null)
            return;

        body.localPosition = bodyBaseLocalPos;
        body.localScale = bodyBaseLocalScale;
        body.localRotation = bodyBaseLocalRotation;
        RestoreSpriteColors();
        ResetLandingRing();
    }

    private void PlayLandingRing()
    {
        if (landingRing == null)
            return;

        TweenLifecycle.Kill(ref ringTween);
        landingRing.gameObject.SetActive(true);
        landingRing.transform.localScale = Vector3.one * 0.45f;
        SetRingAlpha(0.72f);

        Sequence sequence = DOTween.Sequence()
            .Join(landingRing.transform.DOScale(1.45f, ringDuration).SetEase(Ease.OutCubic))
            .Join(DOTween.To(() => 0.72f, SetRingAlpha, 0f, ringDuration))
            .BindTo(this, useUnscaledTime: true);

        ringTween = sequence;
        sequence.OnComplete(() =>
        {
            ResetLandingRing();
            if (ringTween == sequence)
                ringTween = null;
        });
    }

    private void ResetLandingRing()
    {
        if (landingRing == null)
            return;

        landingRing.gameObject.SetActive(false);
        landingRing.transform.localScale = Vector3.one;
    }

    private void SetRingAlpha(float alpha)
    {
        if (landingRing == null)
            return;

        Color start = landingRing.startColor;
        Color end = landingRing.endColor;
        start.a = alpha;
        end.a = alpha;
        landingRing.startColor = start;
        landingRing.endColor = end;
    }

    private void CacheSpriteColors()
    {
        spriteRenderers.Clear();
        spriteBaseColors.Clear();
        foreach (SpriteRenderer renderer in body.GetComponentsInChildren<SpriteRenderer>(true))
        {
            spriteRenderers.Add(renderer);
            spriteBaseColors.Add(renderer.color);
        }
    }

    private void SetSpriteAlpha(float alpha)
    {
        for (int i = 0; i < spriteRenderers.Count; i++)
        {
            SpriteRenderer renderer = spriteRenderers[i];
            if (renderer == null)
                continue;

            Color color = spriteBaseColors[i];
            color.a *= alpha;
            renderer.color = color;
        }
    }

    private void RestoreSpriteColors()
    {
        for (int i = 0; i < spriteRenderers.Count; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = spriteBaseColors[i];
        }
    }
}
