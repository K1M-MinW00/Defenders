using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectIconSet", menuName = "Game/UI/Status Effect Icon Set")]
public sealed class StatusEffectIconSetSO : ScriptableObject
{
    public const string ResourcePath = "GameData/UI/StatusEffectIconSet";

    public Sprite slow;
    public Sprite stun;
    public Sprite poison;
    public Sprite burn;
    public Sprite defenseDown;
    public Sprite berserk;

    private static StatusEffectIconSetSO cached;

    public static StatusEffectIconSetSO Load()
    {
        if (cached == null)
            cached = Resources.Load<StatusEffectIconSetSO>(ResourcePath);
        return cached;
    }

    public Sprite Get(StatusEffectType type) => type switch
    {
        StatusEffectType.Slow => slow,
        StatusEffectType.Stun => stun,
        StatusEffectType.Poison => poison,
        StatusEffectType.Burn => burn,
        StatusEffectType.DefenseDown => defenseDown,
        _ => null,
    };
}
