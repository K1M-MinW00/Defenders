using System;
using System.Collections.Generic;

public sealed class LobbyBattlePresenter
{
    private readonly UserDataRoot userData;
    private readonly UserLevelProgressionSO levelProgression;

    public LobbyBattlePresenter(UserDataRoot userData, UserLevelProgressionSO levelProgression)
    {
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
        this.levelProgression = levelProgression;
    }

    public LobbyBattleViewState Build()
    {
        UserProfileData profile = userData.Profile;
        UserRosterData roster = userData.Roster;
        UserProgressData progress = userData.Progress;
        UserResourceData resource = userData.Resource;

        if (profile == null || roster == null || progress == null || resource == null)
            return null;

        return new LobbyBattleViewState
        {
            ProfileIcon = ProfileIconResolver.ResolveIcon(profile.IconId, roster),
            Level = profile.Level,
            Power = roster.Power,
            NormalizedExp = GetNormalizedExp(profile.Exp, profile.Level),
            Sector = progress.CurrentSector,
            Stage = progress.CurrentStage,
            BestWaveCleared = progress.BestWaveCleared,
            Gold = resource.Gold,
            Gem = resource.Gem,
            Fuel = resource.Fuel,
            MaxFuel = resource.MaxFuel,
            CanStartBattle = ValidateBattleStart(out _) == LobbyBattleStartFailure.None,
        };
    }

    public bool TryBuildStageEnterData(out StageEnterData enterData, out LobbyBattleStartFailure failure)
    {
        failure = ValidateBattleStart(out List<string> selectedUnitIds);
        if (failure != LobbyBattleStartFailure.None)
        {
            enterData = null;
            return false;
        }

        enterData = new StageEnterData(
            userData.Progress.CurrentSector,
            userData.Progress.CurrentStage,
            selectedUnitIds);
        return true;
    }

    private LobbyBattleStartFailure ValidateBattleStart(out List<string> selectedUnitIds)
    {
        selectedUnitIds = null;
        if (userData.Profile == null || userData.Roster == null ||
            userData.Progress == null || userData.Resource == null)
        {
            return LobbyBattleStartFailure.UserDataUnavailable;
        }

        if (userData.Progress.CurrentSector <= 0 || userData.Progress.CurrentStage <= 0)
            return LobbyBattleStartFailure.InvalidProgress;

        if (userData.Roster.SelectedUnitIds == null || userData.Roster.SelectedUnitIds.Count == 0)
            return LobbyBattleStartFailure.EmptyFormation;

        HashSet<string> ownedIds = new(StringComparer.Ordinal);
        if (userData.Roster.OwnedUnits != null)
        {
            foreach (UserUnitData ownedUnit in userData.Roster.OwnedUnits)
            {
                if (ownedUnit != null && !string.IsNullOrWhiteSpace(ownedUnit.UnitId))
                    ownedIds.Add(ownedUnit.UnitId);
            }
        }

        HashSet<string> selectedIds = new(StringComparer.Ordinal);
        selectedUnitIds = new List<string>(userData.Roster.SelectedUnitIds.Count);
        foreach (string unitId in userData.Roster.SelectedUnitIds)
        {
            if (string.IsNullOrWhiteSpace(unitId) || !ownedIds.Contains(unitId) ||
                GameConfig.Units.Get(unitId) == null || !selectedIds.Add(unitId))
            {
                selectedUnitIds = null;
                return LobbyBattleStartFailure.InvalidFormation;
            }

            selectedUnitIds.Add(unitId);
        }

        return LobbyBattleStartFailure.None;
    }

    private float GetNormalizedExp(int exp, int level)
    {
        if (levelProgression != null)
            return levelProgression.GetNormalizedExp(exp, level);

        int requiredExp = Math.Max(0, 100 + (level - 1) * 50);
        return requiredExp > 0 ? Math.Clamp((float)Math.Max(0, exp) / requiredExp, 0f, 1f) : 0f;
    }
}
