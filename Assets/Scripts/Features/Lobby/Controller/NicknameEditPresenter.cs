using System;

public sealed class NicknameEditPresenter
{
    private readonly UserDataRoot userData;
    private readonly int paidChangeCost;
    private string currentNickname;
    private int currentCost;

    public NicknameEditPresenter(UserDataRoot userData, int paidChangeCost)
    {
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
        this.paidChangeCost = Math.Max(0, paidChangeCost);
    }

    public string Open()
    {
        UserProfileData profile = userData.Profile;
        currentNickname = profile?.Nickname ?? string.Empty;
        currentCost = profile?.HasUsedFreeNicknameChange == true ? paidChangeCost : 0;
        return currentNickname;
    }

    public NicknameEditState Build(string input, bool isSaving)
    {
        string normalized = NicknamePolicy.Normalize(input);
        return new NicknameEditState
        {
            NormalizedNickname = normalized,
            ConfirmLabel = currentCost > 0 ? $"변경 (보석 {currentCost:N0})" : "무료 변경",
            CanSave = !isSaving && normalized != currentNickname && NicknamePolicy.IsValid(normalized),
        };
    }

    public void MarkSaved(string nickname)
    {
        currentNickname = NicknamePolicy.Normalize(nickname);
    }
}
