using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RecruitUnitIconView : MonoBehaviour
{
    [SerializeField] private Image bgImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject duplicateMark;
    [SerializeField] private UnitVisualConfigSO visualConfig;

    private CanvasGroup revealCanvasGroup;
    private Coroutine revealRoutine;

    public void Setup(GachaResult result)
    {
        iconImage.sprite = result.Unit.icon;
        bgImage.color = visualConfig != null ? visualConfig.GetRarityColor(result.Unit.rarity) : Color.white;
        duplicateMark.SetActive(result.IsDuplicateReward);
    }

    public void Setup(UnitDataSO unit)
    {
        iconImage.sprite = unit.icon;
        bgImage.color = visualConfig != null ? visualConfig.GetRarityColor(unit.rarity) : Color.white;
        duplicateMark.SetActive(false);
    }

    public void RevealAnimated(float duration)
    {
        gameObject.SetActive(true);
        EnsureCanvasGroup();

        if (revealRoutine != null)
            StopCoroutine(revealRoutine);

        if (duration <= 0f)
        {
            ShowImmediately();
            return;
        }

        revealRoutine = StartCoroutine(RevealRoutine(duration));
    }

    public void ShowImmediately()
    {
        gameObject.SetActive(true);

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        EnsureCanvasGroup();
        revealCanvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
    }

    private IEnumerator RevealRoutine(float duration)
    {
        revealCanvasGroup.alpha = 0f;
        transform.localScale = Vector3.one * 0.72f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            revealCanvasGroup.alpha = eased;
            transform.localScale = Vector3.one * Mathf.Lerp(0.72f, 1f, eased);
            yield return null;
        }

        revealCanvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
        revealRoutine = null;
    }

    private void EnsureCanvasGroup()
    {
        if (revealCanvasGroup != null)
            return;

        revealCanvasGroup = GetComponent<CanvasGroup>();

        if (revealCanvasGroup == null)
            revealCanvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
}
