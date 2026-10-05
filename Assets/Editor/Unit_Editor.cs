using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(UnitController))]
public class Unit_Editor : Editor
{
    private const float CornerRadius = 0.08f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!Application.isPlaying)
            return;

        UnitController unit = (UnitController)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Combat Debug", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextField("Runtime State", unit.RuntimeState.ToString());
            EditorGUILayout.TextField("FSM State", unit.FSMController?.CurrentStateName ?? "None");
            EditorGUILayout.TextField("Attack Target", GetTargetName(unit.Target));

            UnitSkillController skills = unit.SkillController;
            EditorGUILayout.TextField("Active Skill", GetSkillName(skills?.ActiveSkill));
            EditorGUILayout.TextField("Passive Skill", GetSkillName(skills?.PassiveSkill));
            EditorGUILayout.TextField("Skill Phase", skills?.ExecutionPhase.ToString() ?? "None");
            EditorGUILayout.TextField(
                "Skill Target",
                GetTargetName(skills?.ExecutionContext?.EnemyTarget));
        }

        Repaint();
    }

    private void OnSceneGUI()
    {
        UnitController unit = (UnitController)target;

        if (unit == null)
            return;

        if (!Application.isPlaying)
            return;

        DrawAttackRange(unit);
        DrawTargetLine(unit);
        DrawSkillTargetLine(unit);
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
        Handles.DrawLine(unit.transform.position, unit.Target.TargetTransform.position);

        Handles.color = Color.yellow;
        Handles.SphereHandleCap(
            0,
            unit.Target.TargetTransform.position,
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

    private void DrawSkillTargetLine(UnitController unit)
    {
        ICombatTarget target = unit.SkillController?.ExecutionContext?.EnemyTarget;
        if (!CombatTargetSelector.IsValid(target))
            return;

        Handles.color = new Color(1f, 0.45f, 0f, 1f);
        Handles.DrawDottedLine(
            unit.transform.position,
            target.TargetTransform.position,
            4f);
    }

    private void DrawStateLabel(UnitController unit)
    {
        string stateName = unit.FSMController?.CurrentStateName ?? "None";
        string targetName = GetTargetName(unit.Target);
        UnitSkillController skills = unit.SkillController;
        string skillName = GetSkillName(skills?.ActiveSkill);
        string skillPhase = skills?.ExecutionPhase.ToString() ?? "None";

        Handles.color = Color.white;
        Handles.Label(
            unit.transform.position + Vector3.up * 0.8f,
            $"State: {stateName}\nTarget: {targetName}\nSkill: {skillName} ({skillPhase})"
        );
    }

    private static string GetTargetName(ICombatTarget combatTarget)
    {
        return CombatTargetSelector.IsValid(combatTarget)
            ? combatTarget.TargetTransform.name
            : "None";
    }

    private static string GetSkillName(Component skill)
    {
        return skill != null ? skill.GetType().Name : "None";
    }
}
