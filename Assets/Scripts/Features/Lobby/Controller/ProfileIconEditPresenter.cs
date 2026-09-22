using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ProfileIconEditPresenter
{
    private readonly UserDataRoot userData;
    private string currentIconId;
    private string selectedIconId;

    public string SelectedIconId => selectedIconId;

    public ProfileIconEditPresenter(UserDataRoot userData)
    {
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public ProfileIconEditState Open(bool isSaving)
    {
        currentIconId = ProfileIconResolver.ResolveIconId(userData.Profile?.IconId, userData.Roster);
        selectedIconId = currentIconId;
        return Build(isSaving, true);
    }

    public bool TrySelect(string iconId)
    {
        if (!ProfileIconResolver.IsSelectable(iconId, userData.Roster))
            return false;

        selectedIconId = iconId;
        return true;
    }

    public ProfileIconEditState Build(bool isSaving, bool includeOptions = false)
    {
        return new ProfileIconEditState
        {
            Options = includeOptions ? BuildOptions() : null,
            PreviewIcon = ProfileIconResolver.ResolveIcon(selectedIconId, userData.Roster),
            CanSave = !isSaving && !string.IsNullOrWhiteSpace(selectedIconId) && selectedIconId != currentIconId,
        };
    }

    public void MarkSaved()
    {
        currentIconId = selectedIconId;
    }

    private IReadOnlyList<ProfileIconOption> BuildOptions()
    {
        List<ProfileIconOption> options = new();
        HashSet<string> addedIds = new();

        if (userData.Roster?.OwnedUnits == null)
            return options;

        foreach (UserUnitData unit in userData.Roster.OwnedUnits)
        {
            string unitId = unit?.UnitId;
            Sprite icon = GameConfig.Units.GetIcon(unitId);
            if (string.IsNullOrWhiteSpace(unitId) || icon == null || !addedIds.Add(unitId))
                continue;

            options.Add(new ProfileIconOption { IconId = unitId, Icon = icon });
        }

        return options;
    }
}
