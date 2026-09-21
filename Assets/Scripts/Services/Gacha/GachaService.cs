using UnityEngine;

public sealed class GachaService
{
    private readonly UserDataRoot userData;
    private UserGachaData GachaData => userData.Gacha;

    public GachaService(UserDataRoot userData)
    {
        this.userData = userData;
    }

    public int GetCurrentPity(RecruitType recruitType)
    {
        if (GachaData == null)
            return 0;

        return recruitType switch
        {
            RecruitType.Normal => GachaData.NormalPity,
            RecruitType.Special => GachaData.SpecialPity,
            _ => 0
        };
    }

    public int GetRemainPity(GachaDataSO banner)
    {
        if (banner == null)
            return 0;

        int currentPity = GetCurrentPity(banner.recruitType);

        return Mathf.Max(0, banner.legendPityCount - currentPity);
    }

}
