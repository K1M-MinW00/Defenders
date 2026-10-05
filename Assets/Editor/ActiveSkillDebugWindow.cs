using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class ActiveSkillDebugWindow : EditorWindow
{
    private sealed class LiveRecord
    {
        public UnitController Unit;
        public string SkillName = "-";
        public int CastCount;
        public int CurrentHitCount;
        public float CurrentDamage;
        public int CurrentCriticalCount;
        public int TotalHitCount;
        public float TotalDamage;
        public int TotalCriticalCount;
        public float LastCastTime;
    }

    private static readonly string[] DebugPropertyNames =
    {
        "damageMultiplier",
        "upgrade_damageMultiplier",
        "healMultiplier",
        "upgrade_healMultiplier",
        "radius",
        "impactRadius",
        "rainRadius",
        "hitRadius",
        "explosionRadius",
        "beamLength",
        "beamWidth",
        "boxSize",
        "arrowCount",
        "upgrade_arrowCount",
        "multCnt",
        "duration",
        "upgrade_duration"
    };

    private readonly Dictionary<int, LiveRecord> records = new();
    private Vector2 liveScroll;
    private Vector2 configScroll;
    private int tab;

    [MenuItem("Tools/Defenders/Active Skill Debugger")]
    public static void Open()
    {
        GetWindow<ActiveSkillDebugWindow>("Active Skill Debugger");
    }

    private void OnEnable()
    {
        CombatDebugTelemetry.SkillApplied += HandleSkillApplied;
        CombatDebugTelemetry.DamageApplied += HandleDamageApplied;
        EditorApplication.update += RepaintDuringPlay;
    }

    private void OnDisable()
    {
        CombatDebugTelemetry.SkillApplied -= HandleSkillApplied;
        CombatDebugTelemetry.DamageApplied -= HandleDamageApplied;
        EditorApplication.update -= RepaintDuringPlay;
    }

    private void OnGUI()
    {
        tab = GUILayout.Toolbar(tab, new[] { "실시간 전투", "프리팹 설정" });
        EditorGUILayout.Space(6f);

        if (tab == 0)
            DrawLiveView();
        else
            DrawPrefabView();
    }

    private void DrawLiveView()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.HelpBox(
                EditorApplication.isPlaying
                    ? "스킬 피해만 집계합니다. 기본 공격과 패시브 효과 피해는 제외됩니다."
                    : "Play Mode에서 유닛별 실제 스킬 피해를 확인할 수 있습니다.",
                MessageType.Info);

            if (GUILayout.Button("기록 초기화", GUILayout.Width(90f), GUILayout.Height(38f)))
                records.Clear();
        }

        if (!EditorApplication.isPlaying)
            return;

        RegisterSceneUnits();
        liveScroll = EditorGUILayout.BeginScrollView(liveScroll);

        foreach (LiveRecord record in records.Values
                     .Where(x => x.Unit != null)
                     .OrderBy(x => x.Unit.name))
        {
            DrawLiveRecord(record);
        }

        EditorGUILayout.EndScrollView();
    }

    private static void DrawLiveRecord(LiveRecord record)
    {
        UnitController unit = record.Unit;
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(unit.name, EditorStyles.linkLabel, GUILayout.Width(145f)))
                    Selection.activeObject = unit.gameObject;

                EditorGUILayout.LabelField(record.SkillName, EditorStyles.boldLabel, GUILayout.Width(190f));
                EditorGUILayout.LabelField($"FSM: {unit.FSMController?.CurrentStateName ?? "-"}");
                EditorGUILayout.LabelField(
                    $"Energy: {unit.Energy.CurrentEnergy:0.#}/{unit.Energy.MaxEnergy:0.#}",
                    GUILayout.Width(145f));
            }

            string targetName = unit.TargetTransform != null && unit.Target != null
                ? unit.Target.TargetTransform.name
                : "없음";
            EditorGUILayout.LabelField(
                $"대상: {targetName}   최근 시전: {record.LastCastTime:0.00}s   시전 횟수: {record.CastCount}");
            EditorGUILayout.LabelField(
                $"현재 시전 — 적중 {record.CurrentHitCount}, 피해 {record.CurrentDamage:0.#}, 치명타 {record.CurrentCriticalCount}");
            EditorGUILayout.LabelField(
                $"누적 — 적중 {record.TotalHitCount}, 피해 {record.TotalDamage:0.#}, 치명타 {record.TotalCriticalCount}");
        }
    }

    private void DrawPrefabView()
    {
        EditorGUILayout.HelpBox(
            "유닛 프리팹에 직렬화된 실제 수치를 표시합니다. 스크립트 기본값보다 프리팹 값이 우선합니다.",
            MessageType.Info);

        configScroll = EditorGUILayout.BeginScrollView(configScroll);
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Units" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            ActiveSkillBase skill = prefab != null ? prefab.GetComponent<ActiveSkillBase>() : null;
            if (skill == null)
                continue;

            DrawPrefabSkill(prefab, skill);
        }
        EditorGUILayout.EndScrollView();
    }

    private static void DrawPrefabSkill(GameObject prefab, ActiveSkillBase skill)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(prefab.name, EditorStyles.linkLabel, GUILayout.Width(145f)))
                    Selection.activeObject = prefab;

                EditorGUILayout.LabelField(skill.GetType().Name, EditorStyles.boldLabel);
            }

            EditorGUILayout.LabelField(
                $"Target: {skill.TargetType}   Fail: {skill.TargetFailPolicy}   Resolve: {skill.ResolutionPolicy}");

            SerializedObject serialized = new(skill);
            List<string> values = new();
            foreach (string propertyName in DebugPropertyNames)
            {
                SerializedProperty property = serialized.FindProperty(propertyName);
                if (property == null)
                    continue;

                values.Add($"{property.displayName}: {GetPropertyValue(property)}");
            }

            EditorGUILayout.LabelField(string.Join("   |   ", values), EditorStyles.wordWrappedLabel);
        }
    }

    private static string GetPropertyValue(SerializedProperty property)
    {
        return property.propertyType switch
        {
            SerializedPropertyType.Integer => property.intValue.ToString(),
            SerializedPropertyType.Float => property.floatValue.ToString("0.###"),
            SerializedPropertyType.Vector2 => property.vector2Value.ToString("0.##"),
            _ => "-"
        };
    }

    private void HandleSkillApplied(CombatDebugTelemetry.SkillAppliedEvent evt)
    {
        if (evt.Caster == null)
            return;

        LiveRecord record = GetOrCreateRecord(evt.Caster);
        record.SkillName = evt.Skill.GetType().Name;
        record.CastCount++;
        record.CurrentHitCount = 0;
        record.CurrentDamage = 0f;
        record.CurrentCriticalCount = 0;
        record.LastCastTime = evt.Time;
    }

    private void HandleDamageApplied(CombatDebugTelemetry.DamageAppliedEvent evt)
    {
        if (evt.Request.Origin != DamageOrigin.Skill || evt.Request.Source is not UnitController caster)
            return;

        LiveRecord record = GetOrCreateRecord(caster);
        record.CurrentHitCount++;
        record.CurrentDamage += evt.Result.AppliedAmount;
        record.TotalHitCount++;
        record.TotalDamage += evt.Result.AppliedAmount;

        if (evt.Result.WasCritical)
        {
            record.CurrentCriticalCount++;
            record.TotalCriticalCount++;
        }
    }

    private LiveRecord GetOrCreateRecord(UnitController unit)
    {
        int id = unit.GetInstanceID();
        if (records.TryGetValue(id, out LiveRecord record))
            return record;

        record = new LiveRecord { Unit = unit };
        records.Add(id, record);
        return record;
    }

    private void RegisterSceneUnits()
    {
        UnitController[] units = Object.FindObjectsByType<UnitController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        for (int i = 0; i < units.Length; i++)
            GetOrCreateRecord(units[i]);
    }

    private void RepaintDuringPlay()
    {
        if (EditorApplication.isPlaying)
            Repaint();
    }
}
