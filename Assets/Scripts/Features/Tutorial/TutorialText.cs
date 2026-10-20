using System.Collections.Generic;

public static class TutorialText
{
    private static readonly IReadOnlyDictionary<string, string> Keys = new Dictionary<string, string>
    {
        [FirstBattleTutorialSequence.SummonWave1Unit] = "battle.summon_wave_1",
        [FirstBattleTutorialSequence.MoveWave1Unit] = "battle.move_wave_1",
        [FirstBattleTutorialSequence.StartWave1] = "battle.start_wave_1",
        [FirstBattleTutorialSequence.WaitWave1Clear] = "battle.wait_wave_1_clear",
        [FirstBattleTutorialSequence.SummonWave2Unit] = "battle.summon_wave_2",
        [FirstBattleTutorialSequence.WaitWave2Fusion] = "battle.wait_wave_2_fusion",
        [FirstBattleTutorialSequence.StartWave2] = "battle.start_wave_2",
        [FirstBattleTutorialSequence.WaitWave2Clear] = "battle.wait_wave_2_clear",
        [FirstBattleTutorialSequence.SummonWave3Unit] = "battle.summon_wave_3",
        [FirstBattleTutorialSequence.StartWave3] = "battle.start_wave_3",
        [FirstBattleTutorialSequence.WaitEliteClear] = "battle.wait_elite_clear",
        [FirstBattleTutorialSequence.ChooseRelic] = "battle.choose_relic",
        [FirstBattleTutorialSequence.SummonPostRelicB] = "battle.summon_post_relic",
        [FirstBattleTutorialSequence.RerollBToA] = "battle.reroll",
        [FirstBattleTutorialSequence.WaitThreeStarFusion] = "battle.wait_three_star_fusion",
        [FirstBattleTutorialSequence.IncreasePopulation] = "battle.increase_population",
        [FirstBattleTutorialSequence.FreePlay] = "battle.free_play",
        [FirstBattleTutorialSequence.ClearBattle] = "battle.clear",
        [FirstBattleTutorialSequence.ReturnToLobby] = "battle.return_to_lobby",
    };

    public static string Get(string stepId, string unitAName = null, string unitBName = null)
    {
        if (!Keys.TryGetValue(stepId ?? string.Empty, out string key))
            return stepId ?? string.Empty;

        return LocalizationService.Get(LocalizationTables.Tutorial, key)
            .Replace("{unitA}", unitAName ?? string.Empty)
            .Replace("{unitB}", unitBName ?? string.Empty);
    }
}
