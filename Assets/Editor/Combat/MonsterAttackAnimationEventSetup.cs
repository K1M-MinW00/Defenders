using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MonsterAttackAnimationEventSetup
{
    private const string MonsterPrefabFolder = "Assets/Prefabs/Monsters";
    private const float HitNormalizedTime = 0.55f;
    private const float FinishNormalizedTime = 0.95f;

    [MenuItem("Tools/Defenders/Combat/Configure Monster Attack Events")]
    public static void Apply()
    {
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
        AnimationEvent[] retainedEvents = AnimationUtility.GetAnimationEvents(clip)
            .Where(animationEvent =>
                animationEvent.functionName != nameof(MonsterAnimationEvent.OnAttackHit) &&
                animationEvent.functionName != nameof(MonsterAnimationEvent.OnAttackFinished))
            .ToArray();

        var events = new List<AnimationEvent>(retainedEvents)
        {
            new AnimationEvent
            {
                functionName = nameof(MonsterAnimationEvent.OnAttackHit),
                time = clip.length * HitNormalizedTime
            },
            new AnimationEvent
            {
                functionName = nameof(MonsterAnimationEvent.OnAttackFinished),
                time = clip.length * FinishNormalizedTime
            }
        };

        AnimationUtility.SetAnimationEvents(clip, events.OrderBy(animationEvent => animationEvent.time).ToArray());
        EditorUtility.SetDirty(clip);
    }
}
