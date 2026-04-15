using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(UnitController))]
public class Unit_Editor : Editor
{
    private const float CornerRadius = 0.08f;

    private void OnSceneGUI()
    {
        UnitController unit = (UnitController)target;

        if (unit == null)
            return;

        if (!Application.isPlaying)
            return;

        DrawAttackRange(unit);
        DrawTargetLine(unit);
        DrawNavMeshPath(unit);
        DrawStateLabel(unit);
    }

    private void DrawAttackRange(UnitController unit)
    {
        if (unit.Runtime == null)
            return;

        float range = unit.Runtime.FinalStats.DetectRange;

        Handles.color = new Color(0f, 1f, 0f, 0.15f);
        Handles.DrawWireDisc(unit.transform.position, Vector3.forward, range);
    }

    private void DrawTargetLine(UnitController unit)
    {
        if (unit.Target == null)
            return;

        Handles.color = Color.red;
        Handles.DrawLine(unit.transform.position, unit.Target.transform.position);

        Handles.color = Color.yellow;
        Handles.SphereHandleCap(
            0,
            unit.Target.transform.position,
            Quaternion.identity,
            0.2f,
            EventType.Repaint
        );
    }

    private void DrawNavMeshPath(UnitController unit)
    {
        NavMeshAgent agent = unit.GetComponent<NavMeshAgent>();

        if (agent == null || !agent.hasPath)
            return;

        var path = agent.path;
        if (path == null || path.corners == null || path.corners.Length < 2)
            return;

        switch (agent.pathStatus)
        {
            case NavMeshPathStatus.PathComplete:
                Handles.color = Color.cyan;
                break;
            case NavMeshPathStatus.PathPartial:
                Handles.color = Color.yellow;
                break;
            case NavMeshPathStatus.PathInvalid:
                Handles.color = Color.red;
                break;
        }

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Handles.DrawLine(path.corners[i], path.corners[i + 1]);
        }

        Handles.color = Color.blue;
        foreach (var corner in path.corners)
        {
            Handles.SphereHandleCap(0, corner, Quaternion.identity, CornerRadius, EventType.Repaint);
        }
    }

    private void DrawStateLabel(UnitController unit)
    {
        string stateName = "Unknown";

        try
        {
            var fsmField = typeof(UnitController).GetField("fsm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fsm = fsmField?.GetValue(unit);

            var currentStateProp = fsm?.GetType().GetProperty("CurrentState");
            var state = currentStateProp?.GetValue(fsm);

            if (state != null)
                stateName = state.GetType().Name;
        }
        catch { }

        string targetName = unit.Target != null ? unit.Target.name : "None";

        Handles.color = Color.white;
        Handles.Label(
            unit.transform.position + Vector3.up * 0.8f,
            $"State: {stateName}\nTarget: {targetName}"
        );
    }
}