using System;

public sealed class LobbyRecruitPresenter
{
    private readonly UserDataRoot userData;
    private readonly InventoryService inventoryService;
    private readonly GachaService gachaService;

    public LobbyRecruitPresenter(
        UserDataRoot userData,
        InventoryService inventoryService,
        GachaService gachaService)
    {
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
        this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        this.gachaService = gachaService ?? throw new ArgumentNullException(nameof(gachaService));
    }

    public RecruitPanelViewState Build(GachaDataSO banner)
    {
        if (banner == null)
            return null;

        ItemDataSO ticket = GameConfig.Items.Get(banner.TicketItemId);
        int gemCount = userData.Resource?.Gem ?? 0;
        int ticketCount = inventoryService.GetItemCount(banner.TicketItemId);

        return new RecruitPanelViewState
        {
            Banner = banner,
            PortraitImage = banner.PickupUnit != null ? banner.PickupUnit.icon : banner.BannerImage,
            HasPickup = banner.RecruitType == RecruitType.Special && banner.PickupUnit != null,
            TicketIcon = ticket?.Icon,
            GemCount = gemCount,
            TicketCount = ticketCount,
            RemainingPity = gachaService.GetRemainPity(banner),
            CanRecruitOne = CanAfford(banner, 1, ticketCount, gemCount),
            CanRecruitTen = CanAfford(banner, 10, ticketCount, gemCount),
        };
    }

    public RecruitCostModel CalculateCost(GachaDataSO banner, int recruitCount)
    {
        if (banner == null || recruitCount <= 0)
            return new RecruitCostModel();

        int ownedTickets = inventoryService.GetItemCount(banner.TicketItemId);
        int ticketUse = Math.Min(ownedTickets, recruitCount);
        int shortage = recruitCount - ticketUse;

        long requiredGem = (long)shortage * Math.Max(0, banner.GemCost);

        return new RecruitCostModel
        {
            TicketUseCount = ticketUse,
            GemUseCount = requiredGem > int.MaxValue ? int.MaxValue : (int)requiredGem,
        };
    }

    private static bool CanAfford(
        GachaDataSO banner,
        int recruitCount,
        int ticketCount,
        int gemCount)
    {
        if (banner == null || recruitCount <= 0 || banner.GemCost < 0)
            return false;

        int shortage = Math.Max(0, recruitCount - ticketCount);
        long requiredGem = (long)shortage * banner.GemCost;

        return requiredGem <= gemCount;
    }
}
