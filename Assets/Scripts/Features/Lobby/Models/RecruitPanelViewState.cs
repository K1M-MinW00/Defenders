using UnityEngine;

public sealed class RecruitPanelViewState
{
    public GachaDataSO Banner { get; set; }
    public Sprite PortraitImage { get; set; }
    public bool HasPickup { get; set; }
    public Sprite TicketIcon { get; set; }
    public int GemCount { get; set; }
    public int TicketCount { get; set; }
    public int RemainingPity { get; set; }
    public bool CanRecruitOne { get; set; }
    public bool CanRecruitTen { get; set; }
}
