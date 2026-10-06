using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LabPanelView : MonoBehaviour
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

#if UNITY_EDITOR
    private void Reset() => AutoWireEditorReferences();

    private void OnValidate()
    {
        if (!Application.isPlaying)
            AutoWireEditorReferences();
    }

    private void AutoWireEditorReferences()
    {
        Transform panel = transform.Find("Panel");
        Transform upperBar = transform.Find("Upper_Bar");
        Transform goldPanel = upperBar?.Find("Gold_Panel");
        if (currencyText == null)
            currencyText = goldPanel?.GetComponentInChildren<TMP_Text>(true);
        if (developButton == null)
            developButton = panel?.Find("Upgrade_Button")?.GetComponent<Button>();
        if (priceText == null && developButton != null)
            priceText = developButton.GetComponentInChildren<TMP_Text>(true);
        if (limitText == null)
            limitText = panel?.Find("LimitText")?.GetComponent<TMP_Text>();
        if (cardContent == null)
            cardContent = panel?.Find("Scroll View/Viewport/Content");
        if (cardGrid == null && cardContent != null)
            cardGrid = cardContent.GetComponent<GridLayoutGroup>();
        if (cardPrefab == null)
        {
            GameObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Lab/LabCard.prefab");
            cardPrefab = asset?.GetComponent<LabCardView>();
        }
        if (offerCardPrefab == null)
        {
            GameObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Lab/LabOfferCard.prefab");
            offerCardPrefab = asset?.GetComponent<LabCardView>();
        }
    }
#endif

    private void OnEnable()
    {
        developButton?.onClick.AddListener(HandleDevelop);
        offerDismissButton?.onClick.AddListener(DismissOffer);
        detailDismissButton?.onClick.AddListener(() => detailPopup.SetActive(false));
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
        developButton?.onClick.RemoveListener(HandleDevelop);
        offerDismissButton?.onClick.RemoveListener(DismissOffer);
        if (UserDataManager.Instance != null)
        {
            UserDataManager.Instance.OnResourceUpdated -= Refresh;
            UserDataManager.Instance.OnLabUpdated -= Refresh;
        }
    }

    private void BuildCards()
    {
        foreach (LabCardView item in cards) if (item != null) Destroy(item.gameObject);
        cards.Clear();
        if (cardPrefab == null || cardContent == null || GameConfig.Lab == null) return;
        foreach (LabCardDataSO card in GameConfig.Lab.Cards.Where(card => card != null).OrderBy(card => card.Rarity))
        {
            LabCardView view = Instantiate(cardPrefab, cardContent);
            view.gameObject.SetActive(true);
            cards.Add(view);
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
        limitText?.SetText("보유 {0}/{1} · 다음 슬롯 레벨 {2}", owned, capacity, ((data.Profile.Level / 5) + 1) * 5);
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
        if (cardGrid == null && cardContent != null)
            cardGrid = cardContent.GetComponent<GridLayoutGroup>();
        if (cardGrid == null)
            return;

        cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        cardGrid.constraintCount = 5;
    }

    private void HandleDevelop()
    {
        if (resolving) return;
        LabOfferResult result = UserDataManager.Instance.CreateLabOffer();
        if (!result.Succeeded)
        {
            UIFeedbackToast.Show(FailureMessage(result.Failure));
            return;
        }
        currentOffer = result.Cards;
        quotedCost = result.Cost;
        foreach (LabCardView item in offerViews) if (item != null) Destroy(item.gameObject);
        offerViews.Clear();
        foreach (LabCardDataSO card in currentOffer)
        {
            LabCardView view = Instantiate(offerCardPrefab, offerContent);
            view.gameObject.SetActive(true);
            view.ShowOffer(card, false, false, HandleOfferSelected);
            offerViews.Add(view);
        }
        offerOverlay.SetActive(true);
        offerDismissButton.interactable = false;
    }

    private async void HandleOfferSelected(LabCardDataSO selected)
    {
        if (resolving) return;
        resolving = true;
        LabDevelopmentFailure failure = await UserDataManager.Instance.AcquireLabCardAsync(selected, quotedCost, currentOffer);
        if (failure != LabDevelopmentFailure.None)
        {
            resolving = false;
            UIFeedbackToast.Show(FailureMessage(failure));
            return;
        }
        int selectedIndex = currentOffer.ToList().IndexOf(selected);
        offerViews[selectedIndex].PlayReveal(selected, true);
        float selectedRevealWait = offerViews[selectedIndex].RevealDuration + revealDelay;
        await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(selectedRevealWait * 1000f));
        for (int i = 0; i < offerViews.Count; i++)
        {
            if (i != selectedIndex)
                offerViews[i].PlayReveal(currentOffer[i], false);
        }
        await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(offerViews[0].RevealDuration * 1000f));
        offerDismissButton.interactable = true;
        resolving = false;
        ShowDetail(selected);
    }

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
        detailPopup.SetActive(true);
    }

    private void DismissOffer()
    {
        if (!resolving && offerDismissButton.interactable) offerOverlay.SetActive(false);
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
