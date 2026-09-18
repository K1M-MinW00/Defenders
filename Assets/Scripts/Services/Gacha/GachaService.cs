using UnityEngine;

public class GachaService
{
    private UserGachaData GachaData => UserDataManager.Instance.UserData.Gacha;

    public int GetCurrentPity(RecruitType recruitType)
    {
        return recruitType switch
        {
            RecruitType.Normal => GachaData.NormalPity,
            RecruitType.Special => GachaData.SpecialPity,
            _ => 0
        };
    }

    public int GetRemainPity(GachaDataSO banner)
    {
        int currentPity = GetCurrentPity(banner.recruitType);

        return Mathf.Max(0, banner.legendPityCount - currentPity);
    }

}
