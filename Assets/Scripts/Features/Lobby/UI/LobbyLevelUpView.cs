using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LobbyLevelUpView : MonoBehaviour
{
    public event Action Closed;

    [Header("Scene References")]
    [SerializeField] private RectTransform root;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform portrait;
    [SerializeField] private UnityEngine.UI.Image portraitImage;
    [SerializeField] private RectTransform previousLevelBadge;
    [SerializeField] private TMP_Text previousLevelText;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private RectTransform currentLevelBadge;
    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private CanvasGroup fuelPanel;
    [SerializeField] private TMP_Text fuelText;
    [SerializeField] private CanvasGroup researchNotice;
    [SerializeField] private UnityEngine.UI.Image researchIcon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text researchText;
    [SerializeField] private TMP_Text dismissHintText;
    [SerializeField] private LobbyLevelUpPointerRelay pointerRelay;
    private Sequence sequence;
    private bool dismissReady;
    private LevelUpPresentationData currentPresentation;
    private bool hasPresentation;

    public bool IsVisible => root != null && root.gameObject.activeSelf;

    private void Awake()
    {
        if (pointerRelay != null)
            pointerRelay.Initialize(HandlePointerClick);
    }

    public void Show(LevelUpPresentationData data, Sprite profileIcon, Sprite laboratoryIcon)
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError($"[{nameof(LobbyLevelUpView)}] Level-up UI scene references are incomplete.", this);
            return;
        }
        currentPresentation = data;
        hasPresentation = true;
        sequence?.Kill();
        dismissReady = false;
        root.gameObject.SetActive(true);
        root.SetAsLastSibling();

        portraitImage.sprite = profileIcon;
        portraitImage.enabled = profileIcon != null;
        previousLevelText.SetText(data.PreviousLevel.ToString());
        currentLevelText.SetText(data.CurrentLevel.ToString());
        RefreshLocalizedText();
        researchIcon.sprite = laboratoryIcon;
        researchIcon.enabled = laboratoryIcon != null;
        researchNotice.gameObject.SetActive(data.UnlocksResearch);

        portrait.anchoredPosition = new Vector2(0f, 105f);
        previousLevelBadge.anchoredPosition = new Vector2(0f, -15f);
        arrow.gameObject.SetActive(false);
        currentLevelBadge.gameObject.SetActive(false);
        fuelPanel.alpha = 0f;
        fuelPanel.transform.localScale = Vector3.one * 0.85f;
        researchNotice.alpha = 0f;
        researchNotice.transform.localScale = Vector3.one * 0.85f;

        sequence = DOTween.Sequence().SetUpdate(true).SetLink(root.gameObject);
        sequence.AppendInterval(0.75f);
        sequence.Append(previousLevelBadge.DOAnchorPosX(-105f, 0.38f).SetEase(Ease.OutCubic));
        sequence.AppendCallback(() =>
        {
            arrow.gameObject.SetActive(true);
            currentLevelBadge.gameObject.SetActive(true);
            arrow.localScale = Vector3.zero;
            currentLevelBadge.localScale = Vector3.zero;
        });
        sequence.Append(arrow.DOScale(1f, 0.22f).SetEase(Ease.OutBack));
        sequence.Join(currentLevelBadge.DOScale(1f, 0.28f).SetEase(Ease.OutBack));
        sequence.AppendInterval(0.35f);
        sequence.Append(fuelPanel.DOFade(1f, 0.25f));
        sequence.Join(fuelPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        if (data.UnlocksResearch)
        {
            sequence.AppendInterval(0.25f);
            sequence.Append(researchNotice.DOFade(1f, 0.25f));
            sequence.Join(researchNotice.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        }
        sequence.AppendInterval(0.2f);
        sequence.OnComplete(() => dismissReady = true);
    }

    private void HandlePointerClick(PointerEventData eventData)
    {
        if (!dismissReady || content == null ||
            RectTransformUtility.RectangleContainsScreenPoint(content, eventData.position, eventData.pressEventCamera))
            return;

        Hide();
    }

    public void Hide()
    {
        if (!IsVisible)
            return;

        sequence?.Kill();
        root.gameObject.SetActive(false);
        Closed?.Invoke();
    }

    private bool HasRequiredReferences() =>
        root != null && content != null && portrait != null && portraitImage != null &&
        previousLevelBadge != null && previousLevelText != null && arrow != null &&
        currentLevelBadge != null && currentLevelText != null && fuelPanel != null &&
        fuelText != null && researchNotice != null && researchIcon != null &&
        titleText != null && researchText != null && dismissHintText != null && pointerRelay != null;

    private void OnEnable() => LocalizationService.LocaleChanged += HandleLocaleChanged;

    private void OnDisable() => LocalizationService.LocaleChanged -= HandleLocaleChanged;

    private void HandleLocaleChanged(string _)
    {
        if (hasPresentation && IsVisible)
            RefreshLocalizedText();
    }

    private void RefreshLocalizedText()
    {
        if (!hasPresentation || titleText == null)
            return;

        titleText.SetText(LocalizationService.Get(LocalizationTables.Tutorial, "level_up.title"));
        string maxFuel = LocalizationService.Get(LocalizationTables.Tutorial, "level_up.max_fuel");
        fuelText.SetText($"<color=#73E58B>{maxFuel}</color>\n{currentPresentation.PreviousMaxFuel:N0}  <color=#61E686>▶</color>  {currentPresentation.CurrentMaxFuel:N0}");
        researchText.SetText(LocalizationService.Get(LocalizationTables.Tutorial, "level_up.research_unlocked"));
        dismissHintText.SetText(LocalizationService.Get(LocalizationTables.Tutorial, "level_up.dismiss"));
    }

    private void OnDestroy()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        sequence?.Kill();
    }
}

public sealed class LobbyLevelUpPointerRelay : MonoBehaviour, IPointerClickHandler
{
    private Action<PointerEventData> callback;
    public void Initialize(Action<PointerEventData> handler) => callback = handler;
    public void OnPointerClick(PointerEventData eventData) => callback?.Invoke(eventData);
}
