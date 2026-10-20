using System;
using System.Collections.Generic;

public static class FirstBattleTutorialSequence
{
    public const int Version = 4;

    public const string SummonWave1Unit = "summon_wave_1_unit";
    public const string MoveWave1Unit = "move_wave_1_unit";
    public const string StartWave1 = "start_wave_1";
    public const string WaitWave1Clear = "wait_wave_1_clear";
    public const string SummonWave2Unit = "summon_wave_2_unit";
    public const string WaitWave2Fusion = "wait_wave_2_fusion";
    public const string StartWave2 = "start_wave_2";
    public const string WaitWave2Clear = "wait_wave_2_clear";
    public const string SummonWave3Unit = "summon_wave_3_unit";
    public const string StartWave3 = "start_wave_3";
    public const string WaitEliteClear = "wait_elite_clear";
    public const string ChooseRelic = "choose_relic";
    public const string SummonPostRelicB = "summon_post_relic_b";
    public const string RerollBToA = "reroll_b_to_a";
    public const string WaitThreeStarFusion = "wait_three_star_fusion";
    public const string IncreasePopulation = "increase_population";
    public const string FreePlay = "free_play";
    public const string ClearBattle = "clear_battle";
    public const string ReturnToLobby = "return_to_lobby";
    public const string Finish = "finish";
    public const string InitialStep = SummonWave1Unit;

    private static readonly string[] OrderedSteps =
    {
        SummonWave1Unit,
        MoveWave1Unit,
        StartWave1,
        WaitWave1Clear,
        SummonWave2Unit,
        WaitWave2Fusion,
        StartWave2,
        WaitWave2Clear,
        SummonWave3Unit,
        SummonPostRelicB,
        RerollBToA,
        WaitThreeStarFusion,
        IncreasePopulation,
        StartWave3,
        WaitEliteClear,
        ChooseRelic,
        FreePlay,
        ClearBattle,
        ReturnToLobby,
        Finish,
    };

    private static readonly IReadOnlyDictionary<string, int> Indices = BuildIndices();

    public static IReadOnlyList<string> Steps => OrderedSteps;

    public static bool Contains(string stepId) =>
        !string.IsNullOrWhiteSpace(stepId) && Indices.ContainsKey(stepId);

    public static bool IsBattleStep(string stepId) =>
        Indices.TryGetValue(stepId ?? string.Empty, out int index) &&
        index <= Indices[FreePlay];

    public static bool TryGetNext(string currentStepId, out string nextStepId)
    {
        nextStepId = null;
        if (!Indices.TryGetValue(currentStepId ?? string.Empty, out int index))
            return false;

        if (index + 1 < OrderedSteps.Length)
            nextStepId = OrderedSteps[index + 1];

        return true;
    }

    private static IReadOnlyDictionary<string, int> BuildIndices()
    {
        Dictionary<string, int> indices = new(StringComparer.Ordinal);
        for (int i = 0; i < OrderedSteps.Length; i++)
            indices.Add(OrderedSteps[i], i);

        return indices;
    }
}
