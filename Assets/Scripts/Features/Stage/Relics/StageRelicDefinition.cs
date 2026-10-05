using System.Collections.Generic;
using UnityEngine;

public enum StageRelicId
{
    WarlordsSeal,
    HawkeyeCrystal,
    BerserkersSpring,
    FrostShackles,
    ReinforcementFlare,
    EliteSummons,
    AbundantVein,
    DiceOfFate,
}

public sealed class StageRelicDefinition
{
    public StageRelicId Id { get; }
    public string Name { get; }
    public string ShortDescription { get; }
    public string Description { get; }
    public string Symbol { get; }
    public Sprite Icon
    {
        get
        {
            if (icon == null && !string.IsNullOrEmpty(iconResourcePath))
                icon = Resources.Load<Sprite>(iconResourcePath);

            return icon;
        }
    }

    private readonly string iconResourcePath;
    private Sprite icon;

    public StageRelicDefinition(
        StageRelicId id,
        string name,
        string shortDescription,
        string description,
        string symbol,
        string iconResourcePath)
    {
        Id = id;
        Name = name;
        ShortDescription = shortDescription;
        Description = description;
        Symbol = symbol;
        this.iconResourcePath = iconResourcePath;
    }
}

public static class StageRelicCatalog
{
    public static readonly IReadOnlyList<StageRelicDefinition> All = new[]
    {
        new StageRelicDefinition(StageRelicId.WarlordsSeal, "전쟁군주의 인장", "아군 공격력 +15%", "이번 스테이지 동안 모든 아군 유닛의 공격력이 15% 증가합니다.", "공", "UI/Relics/relic_ally_attack"),
        new StageRelicDefinition(StageRelicId.HawkeyeCrystal, "매의 눈 수정", "치명타 확률 +20%p", "이번 스테이지 동안 모든 아군 유닛의 치명타 확률이 20%p 증가합니다.", "치", "UI/Relics/relic_ally_critical"),
        new StageRelicDefinition(StageRelicId.BerserkersSpring, "광전사의 태엽", "아군 공격 속도 +20%", "이번 스테이지 동안 모든 아군 유닛의 공격 속도가 20% 증가합니다.", "속", "UI/Relics/relic_ally_attack_speed"),
        new StageRelicDefinition(StageRelicId.FrostShackles, "서리의 족쇄", "적 이동 속도 -20%", "이번 스테이지 동안 모든 적의 이동 속도가 20% 감소합니다.", "빙", "UI/Relics/relic_enemy_move_slow"),
        new StageRelicDefinition(StageRelicId.ReinforcementFlare, "지원군 신호탄", "1성 유닛 1~5명 획득", "무작위 1성 유닛을 1~5명 즉시 획득합니다. 이 효과는 인구 제한을 무시합니다.", "증", "UI/Relics/relic_random_one_star_units"),
        new StageRelicDefinition(StageRelicId.EliteSummons, "정예 소환장", "2성 유닛 1명 획득", "무작위 2성 유닛 1명을 즉시 획득합니다.", "정", "UI/Relics/relic_random_two_star_unit"),
        new StageRelicDefinition(StageRelicId.AbundantVein, "풍요의 광맥", "광물 10~30개 획득", "광물을 10~30개 무작위로 즉시 획득합니다.", "광", "UI/Relics/relic_random_minerals"),
        new StageRelicDefinition(StageRelicId.DiceOfFate, "운명의 주사위", "유닛 리롤 10회 무료", "이번 스테이지에서 다음 10회의 유닛 리롤 비용이 무료가 됩니다.", "운", "UI/Relics/relic_free_rerolls"),
    };
}
