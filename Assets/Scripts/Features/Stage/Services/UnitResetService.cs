using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitResetService : MonoBehaviour
{
    private readonly Dictionary<UnitController, Vector3> preWavePositions = new();

    public void CapturePreWavePositions(UnitRoster roster)
    {
        preWavePositions.Clear();

        if (roster == null)
            return;

        foreach (UnitController unit in roster.Units)
        {
            if (unit == null) 
                continue;

            preWavePositions[unit] = unit.transform.position;
        }
    }

    public void RestoreAll(UnitRoster roster)
    {
        if (roster == null)
            return;

        foreach (UnitController unit in roster.Units)
        {
            if (unit == null) 
                continue;

            unit.RestoreForPrepare();
     
            NavMeshAgent agent = unit.Movement.Agent != null ? unit.Movement.Agent : unit.GetComponent<NavMeshAgent>();
            if (agent == null)
                continue;

            if(!agent.enabled)
                agent.enabled = true;

            agent.isStopped = true;
            agent.ResetPath();

            if (preWavePositions.TryGetValue(unit, out Vector3 pos))
                agent.Warp(pos);
        }
    }
}