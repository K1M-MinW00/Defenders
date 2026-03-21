using UnityEngine;

public class StageRewardService : MonoBehaviour
{
    public void GiveWaveReward(WaveData waveData)
    {
        if (waveData == null)
            return;

        EconomyManager.Instance.ApplyWaveReward(waveData.waveType);
    }

    public void GiveStageClearReward(StageData stageData)
    {
        if (stageData == null)
            return;

        // TODO : 유저 데이터 - 스테이지 클리어 보상
        // UserProgressManager.Instance.MarkStageCleared(stageData.stageId);
    }
}