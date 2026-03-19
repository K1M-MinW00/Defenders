using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitResetService : MonoBehaviour
{
    private readonly Dictionary<UnitRuntime, Vector3> preWavePositions = new();

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
            u.RestoreForPrepare();
     
            var agent = u.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = true;

                if (preWavePositions.TryGetValue(u, out var pos))
                    agent.Warp(pos);
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