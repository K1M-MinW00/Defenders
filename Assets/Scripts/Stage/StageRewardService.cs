using UnityEngine;

public class StageRewardService : MonoBehaviour
{
    [SerializeField] private EconomyManager economyManager;

    public void GiveWaveReward(WaveData waveData)
    {
        if (waveData == null)
            return;

        economyManager.ApplyWaveReward(waveData.waveType);
    }

    public void GiveStageClearReward(StageData stageData)
    {
        if (stageData == null)
            return;

        // TODO : 유저 데이터 - 스테이지 클리어 보상
        // UserProgressManager.Instance.MarkStageCleared(stageData.stageId);
    }
    // TODO : 유물 보상 추가
}