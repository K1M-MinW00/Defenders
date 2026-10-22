using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class BossSummonAbility : BossAbilityBehaviour
{
    private readonly List<MonsterController> summons = new();
    private bool complete;

    public override bool IsComplete => complete;

    public override void Initialize(MonsterController owner)
    {
        BossSummonSettings settings = owner?.Data?.BossDefinition?.summon;
        if (settings != null)
            ConfigureTiming(settings.initialDelay, settings.cooldown);

        base.Initialize(owner);
    }

    public override bool CanStart(MonsterController owner)
    {
        BossSummonSettings settings = owner?.Data?.BossDefinition?.summon;
        if (settings == null || !settings.enabled || owner.Spawner == null)
            return false;

        RemoveInactiveSummons();
        return summons.Count < settings.maxAliveSummons && base.CanStart(owner);
    }

    protected override void OnBegin()
    {
        BossSummonSettings settings = Owner?.Data?.BossDefinition?.summon;
        if (settings == null || !settings.enabled || settings.candidates == null || settings.candidates.Count == 0)
        {
            complete = true;
            return;
        }

        RemoveInactiveSummons();
        int available = Mathf.Max(0, settings.maxAliveSummons - summons.Count);
        int spawnCount = Mathf.Min(settings.count, available);
        PlaySummonSound();
        for (int index = 0; index < spawnCount; index++)
        {
            MonsterDataSO candidate = settings.candidates[index % settings.candidates.Count];
            Vector3 position = FindSpawnPosition(Owner.transform.position, settings.spawnRadius, index, spawnCount);
            MonsterController summoned = Owner.Spawner.TrySpawnReinforcement(
                candidate,
                position,
                settings.hpMultiplier,
                settings.attackMultiplier);
            if (summoned != null)
                summons.Add(summoned);
        }

        complete = true;
    }

    protected override void OnTick(float deltaTime) { }

    protected override void ResetAbility() => complete = false;

    public override void ResetRuntime()
    {
        summons.Clear();
        base.ResetRuntime();
    }

    private void RemoveInactiveSummons()
    {
        summons.RemoveAll(monster => monster == null || !monster.IsRuntimeInitialized || monster.IsDead);
    }

    private void PlaySummonSound()
    {
        AudioClipSettings audio = Owner?.Data?.BossDefinition?.audio?.summon;
        if (audio == null || !audio.IsValid)
            return;

        GameAudioManager.Instance?.PlaySfx(
            audio,
            GameAudioPriority.High,
            GameAudioCue.BossSummon,
            0.1f);
    }

    internal static Vector3 FindSpawnPosition(Vector3 center, float radius, int index, int count)
    {
        float angle = count <= 0 ? 0f : (360f * index / count);
        Vector3 requested = center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * Mathf.Max(0.5f, radius);
        return NavMesh.SamplePosition(requested, out NavMeshHit hit, 2f, NavMesh.AllAreas)
            ? hit.position
            : center;
    }
}
