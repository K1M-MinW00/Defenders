using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRecruitPanelView : MonoBehaviour
{
    [Header("Currency")]
    [SerializeField] private TMP_Text gemText;
    [SerializeField] private TMP_Text ticketText;
    [SerializeField] private Image ticketImage;

    [Header("Banner")]
    [SerializeField] private Image bannerImage;
    [SerializeField] private TMP_Text pityText;

    [Header("Buttons")]
    [SerializeField] private Button normalButton;
    [SerializeField] private Button specialButton;

    [SerializeField] private Button recruitOneButton;
    [SerializeField] private Button recruitTenButton;

    [SerializeField] private Button rateButton;

    [Header("Banner Data")]
    [SerializeField] private GachaDataSO normalBanner;
    [SerializeField] private GachaDataSO specialBanner;

    [Header("Popup")]
    [SerializeField] private RatePopupPanelView ratePopup;
    [SerializeField] private RecruitResultPopupView resultPopup;
    [SerializeField] private GemConfirmPopupView gemConfirmPopup;

    private GachaDataSO currentBanner;

    private const string NormalTicketId = "summon_ticket_normal";
    private const string SpecialTicketId = "summon_ticket_speical";

    private void Awake()
    {
        normalButton.onClick.AddListener(() => { SelectBanner(normalBanner); });

        specialButton.onClick.AddListener(() => { SelectBanner(specialBanner); });

        recruitOneButton.onClick.AddListener(RecruitOne);
        recruitTenButton.onClick.AddListener(RecruitTen);
        rateButton.onClick.AddListener(OpenRatePopup);
    }

    private void OnEnable()
    {
        SelectBanner(normalBanner);
    }

    private void SelectBanner(GachaDataSO banner)
    {
        currentBanner = banner;
        Refresh();
    }

    private void Refresh()
    {
        RefreshResources();
        RefreshBanner();
        RefreshPity();
    }

    private void RefreshResources()
    {
        UserResourceData resource = UserDataManager.Instance.UserData.Resource;

        gemText.text = resource.Gem.ToString("N0");

        string ticketId = currentBanner.recruitType == RecruitType.Normal ? NormalTicketId : SpecialTicketId;
        ticketImage.sprite = ItemDatabase.Get(ticketId).Icon;

        int count = UserDataManager.Instance.InventoryService.GetItemCount(ticketId);

        ticketText.text = count.ToString("N0");
    }

    private void RefreshBanner()
    {
        bannerImage.sprite = currentBanner.bannerImage;
    }

    private void RefreshPity()
    {
        int remain = UserDataManager.Instance.GachaService.GetRemainPity(currentBanner);

        pityText.text = $"앞으로 {remain}회 모집 안에 전설 유닛 확정 획득";
    }

    private void OpenRatePopup()
    {
        ratePopup.Open(currentBanner);
    }

    private void RecruitOne()
    {
        TryRecruit(1);
    }

    private void RecruitTen()
    {
        TryRecruit(10);
    }

    private void TryRecruit(int count)
    {
        RecruitCostModel cost = CalculateCost(currentBanner, count);

        if (cost.NeedGem == false)
        {
            ExecuteRecruit(count, cost);
            return;
        }

        gemConfirmPopup.Open(cost.GemUseCount, () => { ExecuteRecruit(count, cost); });
    }

    private RecruitCostModel CalculateCost(GachaDataSO banner, int recruitCount)
    {
        string ticketId = banner.recruitType == RecruitType.Normal ? NormalTicketId : SpecialTicketId;

        int ownedTicket = UserDataManager.Instance.InventoryService.GetItemCount(ticketId);

        int ticketUse = Mathf.Min(ownedTicket, recruitCount);

        int shortage = recruitCount - ticketUse;

        return new RecruitCostModel
        {
            TicketUseCount = ticketUse,
            GemUseCount = shortage * banner.gemCost
        };
    }

    private void ExecuteRecruit(int count, RecruitCostModel cost)
    {
        string ticketId = currentBanner.recruitType == RecruitType.Normal ? NormalTicketId : SpecialTicketId;

        InventoryService inventory = UserDataManager.Instance.InventoryService;
        ResourceService resource = UserDataManager.Instance.ResourceService;

        if (cost.GemUseCount > 0)
        {
            bool success = resource.SpendGem(cost.GemUseCount);

            if (!success)
            {
                Debug.Log("Gem 부족");
                return;
            }
        }

        if (cost.TicketUseCount > 0)
        {
            bool success = inventory.RemoveStackItem(ItemCategory.Consumable, ticketId, cost.TicketUseCount);

            if (!success)
            {
                Debug.LogError("티켓 부족");
                return;
            }
        }

        List<GachaResult> results = UserDataManager.Instance.GachaService.Draw(currentBanner, count);

        resultPopup.Open(results);

        Refresh();
    }
}