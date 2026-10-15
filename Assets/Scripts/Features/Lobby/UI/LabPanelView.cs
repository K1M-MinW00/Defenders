using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
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
    private UserDataManager userDataManager;
    private bool isSubscribed;
    private ModalOverlayLifecycle offerLifecycle;
    private ModalOverlayLifecycle detailLifecycle;
    private ModalOverlayLifecycle OfferLifecycle =>
        offerLifecycle ??= new ModalOverlayLifecycle(offerOverlay);
    private ModalOverlayLifecycle DetailLifecycle =>
        detailLifecycle ??= new ModalOverlayLifecycle(detailPopup);

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        developButton.onClick.AddListener(HandleDevelop);
        offerDismissButton.onClick.AddListener(DismissOffer);
        detailDismissButton?.onClick.AddListener(DismissDetail);
        OfferLifecycle.HideImmediately();
        DetailLifecycle.HideImmediately();
    }

    private void OnEnable()
    {
        if (!TryInitialize())
            return;

        Subscribe();
        BuildCards();
        ConfigureGrid();
        Refresh();
    }

    public void Initialize(UserDataManager manager)
    {
        if (manager == null)
            return;

        if (userDataManager != manager)
        {
            Unsubscribe();
            userDataManager = manager;
        }

        if (!isActiveAndEnabled || !TryInitialize())
            return;

        Subscribe();
        BuildCards();
        ConfigureGrid();
        Refresh();
    }

    private void OnDisable()
    {
        operationVersion++;
        currentOffer = null;
        Unsubscribe();
        OfferLifecycle.HideImmediately();
        DetailLifecycle.HideImmediately();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        developButton?.onClick.RemoveListener(HandleDevelop);
        offerDismissButton?.onClick.RemoveListener(DismissOffer);
        detailDismissButton?.onClick.RemoveListener(DismissDetail);
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
        UserDataRoot data = userDataManager?.UserData;
        if (data?.Resource == null || data.Profile == null || data.Lab == null || GameConfig.Lab == null)
        {
            developButton.interactable = false;
            return;
        }

        currencyText.SetText(data.Resource.ResearchMaterial.ToString("N0"));
        priceText.SetText(userDataManager.GetNextLabDevelopmentCost().ToString("N0"));
        int capacity = LabDevelopmentUseCase.GetCardCapacity(data.Profile.Level);
        int owned = data.Lab.AcquiredCardIds.Count;
        limitText.SetText("보유 {0}/{1}  다음 슬롯 레벨 {2}", owned, capacity, ((data.Profile.Level / 5) + 1) * 5);
        developButton.interactable = !resolving;
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
        if (resolving || userDataManager == null) return;
        LabOfferResult result = userDataManager.CreateLabOffer();
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
        if (resolving || selected == null || currentOffer == null || userDataManager == null) return;

        IReadOnlyList<LabCardDataSO> offerSnapshot = currentOffer;
        int selectedIndex = offerSnapshot.ToList().IndexOf(selected);
        if (selectedIndex < 0 || selectedIndex >= offerViews.Count)
            return;

        resolving = true;
        developButton.interactable = false;
        int currentOperation = ++operationVersion;
        try
        {
            LabDevelopmentFailure failure;
            try
            {
                failure = await userDataManager.AcquireLabCardAsync(selected, quotedCost, offerSnapshot);
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
                UIFeedbackToast.Show(FailureMessage(failure));
                return;
            }

            offerViews[selectedIndex].PlayReveal(selected, true);
            float selectedRevealWait = offerViews[selectedIndex].RevealDuration + revealDelay;
            await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(selectedRevealWait * 1000f));
            if (!IsCurrentOperation(currentOperation))
                return;
            for (int i = 0; i < offerSnapshot.Count; i++)
            {
                if (i != selectedIndex)
                    offerViews[i].PlayReveal(offerSnapshot[i], false);
            }
            float revealDuration = offerViews.Count > 0 ? offerViews[0].RevealDuration : 0f;
            await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(revealDuration * 1000f));
            if (!IsCurrentOperation(currentOperation))
                return;
            offerDismissButton.interactable = true;
            ShowDetail(selected);
        }
        finally
        {
            resolving = false;
            if (isActiveAndEnabled)
                Refresh();
        }
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

    private bool TryInitialize()
    {
        userDataManager ??= UserDataManager.Instance;
        if (userDataManager?.UserData == null || GameConfig.Lab == null)
        {
            developButton.interactable = false;
            return false;
        }

        return true;
    }

    private void Subscribe()
    {
        if (isSubscribed || userDataManager == null)
            return;

        userDataManager.OnResourceUpdated += Refresh;
        userDataManager.OnLabUpdated += Refresh;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || userDataManager == null)
            return;

        userDataManager.OnResourceUpdated -= Refresh;
        userDataManager.OnLabUpdated -= Refresh;
        isSubscribed = false;
    }

    private bool ValidateReferences()
    {
        List<string> missing = new();
        if (currencyText == null) missing.Add(nameof(currencyText));
        if (priceText == null) missing.Add(nameof(priceText));
        if (limitText == null) missing.Add(nameof(limitText));
        if (developButton == null) missing.Add(nameof(developButton));
        if (cardContent == null) missing.Add(nameof(cardContent));
        if (cardGrid == null) missing.Add(nameof(cardGrid));
        if (cardPrefab == null) missing.Add(nameof(cardPrefab));
        if (offerOverlay == null) missing.Add(nameof(offerOverlay));
        if (offerContent == null) missing.Add(nameof(offerContent));
        if (offerCardPrefab == null) missing.Add(nameof(offerCardPrefab));
        if (offerDismissButton == null) missing.Add(nameof(offerDismissButton));
        if (detailPopup == null) missing.Add(nameof(detailPopup));
        if (detailBackground == null) missing.Add(nameof(detailBackground));
        if (detailGradient == null) missing.Add(nameof(detailGradient));
        if (detailIcon == null) missing.Add(nameof(detailIcon));
        if (detailName == null) missing.Add(nameof(detailName));
        if (detailDescription == null) missing.Add(nameof(detailDescription));
        if (detailValue == null) missing.Add(nameof(detailValue));

        if (missing.Count == 0)
            return true;

        Debug.LogError($"[LabPanelView] Required UI references are not assigned: {string.Join(", ", missing)}", this);
        return false;
    }
}
