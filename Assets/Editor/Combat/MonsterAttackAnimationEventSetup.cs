using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class MonsterAttackAnimationEventSetup
{
    private const string MonsterPrefabFolder = "Assets/Prefabs/Monsters";
    private const string MonsterControllerPath = "Assets/Animations/Monster/Monster_Base.controller";
    private const string AttackSpeedParameter = "AttackSpeedMultiplier";
    private const float HitNormalizedTime = 0.55f;
    private const float FinishNormalizedTime = 0.95f;

    [MenuItem("Tools/Defenders/Combat/Configure Monster Attack Events")]
    public static void Apply()
    {
        ConfigureAttackSpeedParameter();

        var attackClips = new HashSet<AnimationClip>();
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { MonsterPrefabFolder });
        int configuredPrefabs = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            try
            {
                MonsterAttackBase[] attacks = root.GetComponentsInChildren<MonsterAttackBase>(true);
                if (attacks.Length == 0)
                    continue;

                var prefabAttackClips = new HashSet<AnimationClip>();
                Animator[] animators = root.GetComponentsInChildren<Animator>(true);

                foreach (Animator animator in animators)
                {
                    if (animator.runtimeAnimatorController == null)
                        continue;

                    foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                    {
                        if (clip != null && clip.name.Contains("Attack", StringComparison.OrdinalIgnoreCase))
                            prefabAttackClips.Add(clip);
                    }
                }

                bool canUseAnimationEvents = prefabAttackClips.Count > 0;
                bool changed = ConfigureAttackComponents(attacks, canUseAnimationEvents);

                foreach (AnimationClip clip in prefabAttackClips)
                    attackClips.Add(clip);

                if (canUseAnimationEvents)
                {
                    foreach (Animator animator in animators)
                    {
                        if (animator.runtimeAnimatorController == null)
                            continue;

                        if (animator.GetComponent<MonsterAnimationEvent>() == null)
                        {
                            animator.gameObject.AddComponent<MonsterAnimationEvent>();
                            changed = true;
                        }
                    }
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    configuredPrefabs++;
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        foreach (AnimationClip clip in attackClips)
            ConfigureClip(clip);

        AssetDatabase.SaveAssets();
        Debug.Log($"[MonsterAttackAnimationEventSetup] Configured {configuredPrefabs} prefabs and {attackClips.Count} attack clips.");
    }

    private static void ConfigureAttackSpeedParameter()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(MonsterControllerPath);
        if (controller == null)
        {
            Debug.LogError($"[MonsterAttackAnimationEventSetup] Controller not found: {MonsterControllerPath}");
            return;
        }

        if (!controller.parameters.Any(parameter => parameter.name == AttackSpeedParameter))
            controller.AddParameter(AttackSpeedParameter, AnimatorControllerParameterType.Float);

        foreach (AnimatorControllerLayer layer in controller.layers)
            ConfigureAttackState(layer.stateMachine);

        EditorUtility.SetDirty(controller);
    }

    private static void ConfigureAttackState(AnimatorStateMachine stateMachine)
    {
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            AnimatorState state = childState.state;
            if (!state.name.Equals("Attack", StringComparison.OrdinalIgnoreCase))
                continue;

            state.speedParameterActive = true;
            state.speedParameter = AttackSpeedParameter;
        }

        foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
            ConfigureAttackState(childStateMachine.stateMachine);
    }

    private static bool ConfigureAttackComponents(
        IEnumerable<MonsterAttackBase> attacks,
        bool enabled)
    {
        bool changed = false;

        foreach (MonsterAttackBase attack in attacks)
        {
            var serializedAttack = new SerializedObject(attack);
            SerializedProperty useEvents = serializedAttack.FindProperty("useAnimationEvents");

            if (useEvents == null || useEvents.boolValue == enabled)
                continue;

            useEvents.boolValue = enabled;
            serializedAttack.ApplyModifiedPropertiesWithoutUndo();
            changed = true;
        }

        return changed;
    }

    private static void ConfigureClip(AnimationClip clip)
    {
        var events = new List<AnimationEvent>(AnimationUtility.GetAnimationEvents(clip));
        bool changed = false;

        if (!HasEvent(events, nameof(MonsterAnimationEvent.OnAttackHit)))
        {
            events.Add(new AnimationEvent
            {
                functionName = nameof(MonsterAnimationEvent.OnAttackHit),
                time = clip.length * HitNormalizedTime
            });
            changed = true;
        }

        if (!HasEvent(events, nameof(MonsterAnimationEvent.OnAttackFinished)))
        {
            events.Add(new AnimationEvent
            {
                functionName = nameof(MonsterAnimationEvent.OnAttackFinished),
                time = clip.length * FinishNormalizedTime
            });
            changed = true;
        }

        if (!changed)
            return;

        AnimationUtility.SetAnimationEvents(clip, events.OrderBy(animationEvent => animationEvent.time).ToArray());
        EditorUtility.SetDirty(clip);
    }

    private static bool HasEvent(IEnumerable<AnimationEvent> events, string functionName)
    {
        return events.Any(animationEvent => animationEvent.functionName == functionName);
    }
}
