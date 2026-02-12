using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/StageData")]
public class StageData : ScriptableObject
{
    public string stageName;
    public int stageId;

    public List<WaveData> waves; // 해당 스테이지의 웨이브 목록
}
