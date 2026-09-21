using UnityEngine;

public sealed class LobbyBattleViewState
{
    public Sprite ProfileIcon { get; set; }
    public int Level { get; set; }
    public int Power { get; set; }
    public float NormalizedExp { get; set; }
    public int Sector { get; set; }
    public int Stage { get; set; }
    public int BestWaveCleared { get; set; }
    public int Gold { get; set; }
    public int Gem { get; set; }
    public int Fuel { get; set; }
    public int MaxFuel { get; set; }
    public bool CanStartBattle { get; set; }
}
