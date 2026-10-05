using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour, IPoolable
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float lifeTime = 0.8f;
    [SerializeField] private float riseDistance = 0.45f;
    [SerializeField] private float appearDuration = 0.14f;
    [SerializeField] private float fadeDuration = 0.22f;
    [SerializeField] private Color normalDamageColor = new(1f, 0.85f, 0.05f, 1f);
    [SerializeField] private Color criticalDamageColor = new(1f, 0.12f, 0.08f, 1f);
    [SerializeField] private Color healColor = new(0.2f, 1f, 0.35f, 1f);
    [SerializeField] private float criticalFontSizeMultiplier = 1.3f;

    private Poolable poolable;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private float normalFontSize;
    private Vector3 baseScale;
    private Vector2 spawnAnchoredPosition;
    private Tween popupTween;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();

        if(poolable == null)
            poolable = gameObject.AddComponent<Poolable>();

        damageText = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = transform as RectTransform;
        normalFontSize = damageText != null ? damageText.fontSize : 1f;
        baseScale = transform.localScale;
    }

    public void SetupDamage(float damage, bool isCritical)
    {
        if (damageText == null)
            return;

        damageText.SetText(FormatAmount(damage));
        damageText.color = isCritical ? criticalDamageColor : normalDamageColor;
        damageText.fontSize = normalFontSize * (isCritical ? criticalFontSizeMultiplier : 1f);
    }

    public void SetupHeal(float amount)
    {
        if (damageText == null)
            return;

        damageText.SetText($"+{FormatAmount(amount)}");
        damageText.color = healColor;
        damageText.fontSize = normalFontSize;
    }

    public static string FormatAmount(float amount)
    {
        float value = Mathf.Max(0f, amount);
        if (value >= 1_000_000f)
            return $"{value / 1_000_000f:0.#}M";
        if (value >= 1_000f)
            return $"{value / 1_000f:0.#}K";

        return Mathf.RoundToInt(value).ToString();
    }

    public void OnDespawn()
    {
        TweenLifecycle.Kill(ref popupTween);

        if (damageText != null)
            damageText.text = string.Empty;

        if (damageText != null)
            damageText.fontSize = normalFontSize;

        RestoreTransform();
    }

    public void OnSpawn()
    {
        TweenLifecycle.Kill(ref popupTween);
        spawnAnchoredPosition = rectTransform != null
            ? rectTransform.anchoredPosition
            : Vector2.zero;

        transform.localScale = baseScale * 0.65f;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        float duration = Mathf.Max(0.1f, lifeTime);
        float appear = Mathf.Min(Mathf.Max(0.05f, appearDuration), duration * 0.45f);
        float fade = Mathf.Min(Mathf.Max(0.05f, fadeDuration), duration * 0.45f);

        Sequence sequence = DOTween.Sequence().BindTo(this);
        sequence.Insert(0f,
            transform.DOScale(baseScale * 1.08f, appear).SetEase(Ease.OutBack));
        sequence.Insert(appear,
            transform.DOScale(baseScale, Mathf.Min(0.08f, duration - appear)).SetEase(Ease.OutQuad));

        if (canvasGroup != null)
        {
            sequence.Insert(0f, canvasGroup.DOFade(1f, Mathf.Min(0.08f, appear)));
            sequence.Insert(duration - fade, canvasGroup.DOFade(0f, fade));
        }

        if (rectTransform != null)
        {
            sequence.Insert(0f,
                rectTransform.DOAnchorPosY(spawnAnchoredPosition.y + riseDistance, duration)
                    .SetEase(Ease.OutQuad));
        }

        sequence.AppendInterval(Mathf.Max(0f, duration - sequence.Duration()));
        sequence.OnComplete(() => poolable?.ReturnToPool());
        popupTween = sequence;
    }

    private void RestoreTransform()
    {
        transform.localScale = baseScale;
        if (rectTransform != null)
            rectTransform.anchoredPosition = spawnAnchoredPosition;
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
}
