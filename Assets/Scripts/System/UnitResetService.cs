using System.Collections.Generic;
using UnityEngine;

public class UnitResetService : MonoBehaviour
{
    private readonly Dictionary<UnitInstance, Vector3> preWavePositions = new();

    public void CapturePreWavePositions(UnitRoster roster)
    {
        preWavePositions.Clear();

        foreach (var u in roster.Units)
        {
            if (u == null) 
                continue;

            preWavePositions[u] = u.transform.position;
        }
    }

    public void ResetUnitsForPrepare(UnitRoster roster)
    {
        foreach (var u in roster.Units)
        {
            if (u == null) 
                continue;

            // 부활 + 풀피 + 에너지 0
            u.ResetForPrepare();
     
            // 위치 복구
            if (preWavePositions.TryGetValue(u, out var pos))
                u.transform.position = pos;


            // (선택) NavMeshAgent 경로 초기화
            var pc = u.GetComponent<PlayerCharacter>();
            if (pc != null && pc.agent != null)
            {
                pc.ClearTarget();
                pc.agent.ResetPath();
                pc.agent.isStopped = true;
            }
        }
    }

    public bool HasAnyAlive(UnitRoster roster)
    {
        foreach (var u in roster.Units)
        {
            if (u != null && u.IsAlive)
                return true;
        }
        return false;
    }
}