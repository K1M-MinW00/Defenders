public sealed class FuelPanelViewState
{
    public int Fuel { get; set; }
    public int MaxFuel { get; set; }
    public int NextRecoverSeconds { get; set; }
    public int FullRecoverSeconds { get; set; }
    public int DailyAdWatchCount { get; set; }
    public int DailyAdLimit { get; set; }
    public bool CanRequestRewardAd { get; set; }
}
