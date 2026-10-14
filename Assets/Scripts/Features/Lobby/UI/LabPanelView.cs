using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LabPanelView : MonoBehaviour, IModalDismissHandler
{
    [Header("Main")]
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text limitText;
    [SerializeField] private Button developButton;
    [SerializeField] private Transform cardContent;
    [SerializeField] private GridLayoutGroup cardGrid;
    [SerializeField] private LabCardView cardPrefab;
    [Header("Offer")]
    [SerializeField] private GameObject offerOverlay;
    [SerializeField] private Transform offerContent;
    [SerializeField] private LabCardView offerCardPrefab;
    [SerializeField] private Button offerDismissButton;
    [Header("Detail")]
    [SerializeField] private GameObject detailPopup;
    [SerializeField] private Image detailBackground;
    [SerializeField] private LabCardGradient detailGradient;
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private TMP_Text detailDescription;
    [SerializeField] private TMP_Text detailValue;
    [SerializeField] private Button detailDismissButton;
    [SerializeField, Min(0f)] private float revealDelay = 0.55f;

    private readonly List<LabCardView> cards = new();
    private readonly List<LabCardView> offerViews = new();
    private IReadOnlyList<LabCardDataSO> currentOffer;
    private int quotedCost;
    private bool resolving;
    private int operationVersion;
    private ModalOverlayLifecycle offerLifecycle;
    private ModalOverlayLifecycle detailLifecycle;
    private ModalOverlayLifecycle OfferLifecycle =>
        offerLifecycle ??= new ModalOverlayLifecycle(offerOverlay);
    private ModalOverlayLifecycle DetailLifecycle =>
        detailLifecycle ??= new ModalOverlayLifecycle(detailPopup);

    private void Awake()
    {
        OfferLifecycle.HideImmediately();
        DetailLifecycle.HideImmediately();
    }

    private void OnEnable()
    {
        developButton?.onClick.AddListener(HandleDevelop);
        offerDismissButton?.onClick.AddListener(DismissOffer);
        detailDismissButton?.onClick.AddListener(DismissDetail);
        if (UserDataManager.Instance != null)
        {
            UserDataManager.Instance.OnResourceUpdated += Refresh;
            UserDataManager.Instance.OnLabUpdated += Refresh;
        }
        BuildCards();
        ConfigureGrid();
        Refresh();
    }

    private void OnDisable()
    {
        operationVersion++;
        resolving = false;
        currentOffer = null;
        developButton?.onClick.RemoveListener(HandleDevelop);
        offerDismissButton?.onClick.RemoveListener(DismissOffer);
        detailDismissButton?.onClick.RemoveListener(DismissDetail);
        if (UserDataManager.Instance != null)
        {
            UserDataManager.Instance.OnResourceUpdated -= Refresh;
            UserDataManager.Instance.OnLabUpdated -= Refresh;
        }
        OfferLifecycle.HideImmediately();
        DetailLifecycle.HideImmediately();
    }

    private void BuildCards()
    {
        if (cardPrefab == null || cardContent == null || GameConfig.Lab == null) return;

        int requiredCount = GameConfig.Lab.Cards.Count(card => card != null);
        while (cards.Count < requiredCount)
        {
            LabCardView view = Instantiate(cardPrefab, cardContent);
            cards.Add(view);
        }

        for (int i = 0; i < cards.Count; i++)
        {
            bool visible = i < requiredCount;
            cards[i].gameObject.SetActive(visible);
            if (!visible)
                cards[i].Clear();
        }
    }

    private void Refresh()
    {
        UserDataRoot data = UserDataManager.Instance?.UserData;
        if (data == null || GameConfig.Lab == null) return;
        currencyText?.SetText(data.Resource.ResearchMaterial.ToString("N0"));
        priceText?.SetText(UserDataManager.Instance.GetNextLabDevelopmentCost().ToString("N0"));
        int capacity = LabDevelopmentUseCase.GetCardCapacity(data.Profile.Level);
        int owned = data.Lab.AcquiredCardIds.Count;
        limitText?.SetText("보유 {0}/{1}  다음 슬롯 레벨 {2}", owned, capacity, ((data.Profile.Level / 5) + 1) * 5);
        HashSet<string> acquired = new(data.Lab.AcquiredCardIds);
        LabCardDataSO[] sortedCards = GameConfig.Lab.Cards.Where(card => card != null).OrderBy(card => card.Rarity).ToArray();
        for (int i = 0; i < cards.Count && i < sortedCards.Length; i++)
        {
            LabCardDataSO card = sortedCards[i];
            cards[i].Bind(card, acquired.Contains(card.CardId), ShowDetail);
        }
    }

    private void ConfigureGrid()
    {
        if (cardGrid == null)
            return;

        cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        cardGrid.constraintCount = 5;
    }

    private void HandleDevelop()
    {
        UserDataManager manager = UserDataManager.Instance;
        if (resolving || manager == null) return;
        LabOfferResult result = manager.CreateLabOffer();
        if (!result.Succeeded)
        {
            UIFeedbackToast.Show(FailureMessage(result.Failure));
            return;
        }
        currentOffer = result.Cards;
        quotedCost = result.Cost;
        while (offerViews.Count < currentOffer.Count)
        {
            LabCardView view = Instantiate(offerCardPrefab, offerContent);
            offerViews.Add(view);
        }

        for (int i = 0; i < offerViews.Count; i++)
        {
            bool visible = i < currentOffer.Count;
            offerViews[i].gameObject.SetActive(visible);
            if (visible)
                offerViews[i].ShowOffer(currentOffer[i], false, false, HandleOfferSelected);
            else
                offerViews[i].Clear();
        }
        OfferLifecycle.Show();
        offerDismissButton.interactable = false;
    }

    private async void HandleOfferSelected(LabCardDataSO selected)
    {
        if (resolving || selected == null || currentOffer == null) return;
        resolving = true;
        int currentOperation = ++operationVersion;
        LabDevelopmentFailure failure;
        try
        {
            failure = await UserDataManager.Instance.AcquireLabCardAsync(selected, quotedCost, currentOffer);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[LabPanelView] Development failed: {exception}");
            failure = LabDevelopmentFailure.SaveFailed;
        }

        if (!IsCurrentOperation(currentOperation))
            return;
        if (failure != LabDevelopmentFailure.None)
        {
            resolving = false;
            UIFeedbackToast.Show(FailureMessage(failure));
            return;
        }
        int selectedIndex = currentOffer.ToList().IndexOf(selected);
        if (selectedIndex < 0 || selectedIndex >= offerViews.Count)
        {
            resolving = false;
            return;
        }
        offerViews[selectedIndex].PlayReveal(selected, true);
        float selectedRevealWait = offerViews[selectedIndex].RevealDuration + revealDelay;
        await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(selectedRevealWait * 1000f));
        if (!IsCurrentOperation(currentOperation))
            return;
        for (int i = 0; i < currentOffer.Count; i++)
        {
            if (i != selectedIndex)
                offerViews[i].PlayReveal(currentOffer[i], false);
        }
        float revealDuration = offerViews.Count > 0 ? offerViews[0].RevealDuration : 0f;
        await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(revealDuration * 1000f));
        if (!IsCurrentOperation(currentOperation))
            return;
        offerDismissButton.interactable = true;
        resolving = false;
        ShowDetail(selected);
    }

    private bool IsCurrentOperation(int version) =>
        isActiveAndEnabled && version == operationVersion;

    private void ShowDetail(LabCardDataSO card)
    {
        if (card == null) return;
        detailIcon.sprite = card.Icon;
        detailIcon.enabled = card.Icon != null;
        if (detailBackground != null) detailBackground.color = Color.white;
        detailGradient?.SetRarity(card.Rarity);
        detailName.text = card.DisplayName;
        detailDescription.text = card.Description;
        detailValue.text = card.GetFormattedValue();
        DetailLifecycle.Show();
    }

    private void DismissOffer()
    {
        if (!resolving && offerDismissButton.interactable)
            OfferLifecycle.Close();
    }

    private void DismissDetail()
    {
        DetailLifecycle.Close();
    }

    public void Dismiss()
    {
        if (DetailLifecycle.IsVisible)
        {
            DismissDetail();
            return;
        }

        DismissOffer();
    }

    private static string FailureMessage(LabDevelopmentFailure failure) => failure switch
    {
        LabDevelopmentFailure.InsufficientCurrency => "재화가 부족합니다.",
        LabDevelopmentFailure.LevelLocked => "계정 레벨을 더 올리면 새로운 기술을 개발할 수 있습니다.",
        LabDevelopmentFailure.AllCardsAcquired => "모든 기술 카드를 획득했습니다.",
        LabDevelopmentFailure.SaveFailed => "저장에 실패했습니다. 잠시 후 다시 시도해 주세요.",
        _ => "기술 개발을 진행할 수 없습니다.",
    };
}
