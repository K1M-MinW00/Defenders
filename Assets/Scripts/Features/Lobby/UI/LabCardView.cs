using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LabCardView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private LabCardGradient gradient;
    [SerializeField] private Graphic selectionFrame;
    [SerializeField] private Image icon;
    [SerializeField] private Graphic lockedIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text valueText;

    private LabCardDataSO card;
    private System.Action<LabCardDataSO> onClick;
    private Tween revealTween;

    public float RevealDuration => 0.34f;

    public void Bind(LabCardDataSO value, bool acquired, System.Action<LabCardDataSO> clicked)
    {
        card = value;
        onClick = clicked;
        bool faceUp = acquired && card != null;
        ApplyCardColors(faceUp, card != null ? card.Rarity : LabCardRarity.Common);
        SetSelectionVisible(false);
        icon.sprite = faceUp ? card.Icon : null;
        icon.enabled = faceUp && card.Icon != null;
        if (lockedIcon != null) lockedIcon.enabled = !faceUp;
        nameText.text = faceUp ? card.DisplayName : string.Empty;
        valueText.text = faceUp ? card.GetFormattedValue() : string.Empty;
        button.interactable = faceUp;
    }

    public void ShowOffer(LabCardDataSO value, bool faceUp, bool highlighted, System.Action<LabCardDataSO> clicked)
    {
        TweenLifecycle.Kill(ref revealTween);
        transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
        ApplyOfferState(value, faceUp, highlighted, clicked);
    }

    public void PlayReveal(LabCardDataSO value, bool highlighted)
    {
        TweenLifecycle.Kill(ref revealTween);
        button.interactable = false;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScaleX(0f, RevealDuration * 0.5f).SetEase(Ease.InQuad));
        sequence.AppendCallback(() => ApplyOfferState(value, true, highlighted, null));
        sequence.Append(transform.DOScaleX(1f, RevealDuration * 0.5f).SetEase(Ease.OutBack));
        if (highlighted)
            sequence.Append(transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0f), 0.38f, 1, 0.35f));
        revealTween = sequence.BindTo(this, useUnscaledTime: true);
    }

    private void ApplyOfferState(LabCardDataSO value, bool faceUp, bool highlighted, System.Action<LabCardDataSO> clicked)
    {
        card = value;
        onClick = clicked;
        ApplyCardColors(faceUp, card != null ? card.Rarity : LabCardRarity.Common);
        SetSelectionVisible(faceUp && highlighted);
        icon.sprite = faceUp ? card.Icon : null;
        icon.enabled = faceUp && card.Icon != null;
        if (lockedIcon != null) lockedIcon.enabled = !faceUp;
        nameText.text = faceUp ? card.DisplayName : string.Empty;
        valueText.text = faceUp ? card.GetFormattedValue() : string.Empty;
        button.interactable = !faceUp;
    }

    public void HandleClick() => onClick?.Invoke(card);

    private void SetSelectionVisible(bool visible)
    {
        if (selectionFrame != null)
            selectionFrame.enabled = visible;
    }

    private void ApplyCardColors(bool faceUp, LabCardRarity rarity)
    {
        background.color = Color.white;
        if (gradient == null)
            gradient = GetComponent<LabCardGradient>();
        if (gradient == null)
            return;

        if (!faceUp)
        {
            gradient.SetColors(new Color(0.48f, 0.5f, 0.54f), new Color(0.22f, 0.24f, 0.28f));
            return;
        }

        gradient.SetRarity(rarity);
    }

    private void OnDisable()
    {
        TweenLifecycle.Kill(ref revealTween);
        transform.localScale = Vector3.one;
        SetSelectionVisible(false);
    }
}
