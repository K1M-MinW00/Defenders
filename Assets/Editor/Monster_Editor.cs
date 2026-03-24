using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(MonsterController))]
public class MonsterControllerEditor : Editor
{
    private void OnSceneGUI()
    {
        MonsterController monster = (MonsterController)target;

        if (monster == null)
            return;

        if (!Application.isPlaying)
            return;

        DrawNavMeshPath(monster);
        DrawTargetGizmo(monster);
    }

    private void DrawNavMeshPath(MonsterController monster)
    {
        NavMeshAgent agent = monster.Agent;
        if (agent == null || !agent.hasPath)
            return;

        NavMeshPath path = agent.path;
        if (path == null || path.corners == null || path.corners.Length < 2)
            return;

        Handles.color = Color.cyan;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Handles.DrawLine(path.corners[i], path.corners[i + 1]);
        }

        Handles.color = Color.blue;
        foreach (Vector3 corner in path.corners)
        {
            Handles.SphereHandleCap(0, corner, Quaternion.identity, 0.1f, EventType.Repaint);
        }
    }

    private void DrawTargetGizmo(MonsterController monster)
    {
        if (monster.Target == null)
            return;

        Handles.color = Color.red;
        Handles.DrawLine(monster.transform.position, monster.Target.transform.position);

        Handles.color = new Color(1f, 0f, 0f, 0.15f);
        Handles.DrawWireDisc(monster.transform.position, Vector3.forward, monster.AttackRange);
    }
}