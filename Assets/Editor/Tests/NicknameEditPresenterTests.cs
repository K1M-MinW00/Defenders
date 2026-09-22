using System;

public sealed class NicknameEditPresenterTests
{
    public void Build_UsesFreeChangeAndValidatesInput()
    {
        UserDataRoot userData = CreateUserData("Hero", false);
        NicknameEditPresenter presenter = new(userData, 500);

        string current = presenter.Open();
        NicknameEditState unchanged = presenter.Build(current, false);
        NicknameEditState changed = presenter.Build("  New Hero  ", false);

        Assert(unchanged.ConfirmLabel == "무료 변경", "First nickname change should be free.");
        Assert(!unchanged.CanSave, "Unchanged nickname should not be saved.");
        Assert(changed.CanSave && changed.NormalizedNickname == "New Hero", "Valid changed nickname should be normalized and savable.");
    }

    public void Build_UsesPaidCostAndHonorsSavingState()
    {
        UserDataRoot userData = CreateUserData("Hero", true);
        NicknameEditPresenter presenter = new(userData, 500);
        presenter.Open();

        NicknameEditState state = presenter.Build("Another", true);

        Assert(state.ConfirmLabel == "변경 (보석 500)", "Paid nickname cost should be displayed.");
        Assert(!state.CanSave, "Save should be disabled while another save is running.");
    }

    private static UserDataRoot CreateUserData(string nickname, bool usedFreeChange)
    {
        return new UserDataRoot
        {
            Profile = new UserProfileData
            {
                Nickname = nickname,
                HasUsedFreeNicknameChange = usedFreeChange,
            },
        };
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
