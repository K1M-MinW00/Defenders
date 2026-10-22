using UnityEngine;

public sealed class BossAreaAttackAbility : BossAbilityBehaviour
{
    [Header("Target")]
    [SerializeField] private BossTargetSelection targetSelection = BossTargetSelection.DensestUnitCluster;
    [SerializeField, Min(0.1f)] private float radius = 2f;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float telegraphDuration = 2f;
    [SerializeField, Min(0f)] private float recoveryDuration = 0.5f;

    [Header("Damage")]
    [SerializeField, Min(0f)] private float damageMultiplier = 1.5f;

    [Header("Presentation")]
    [SerializeField] private GameObject telegraphPrefab;

    private BossAbilityTarget target;
    private GameObject telegraphInstance;
    private BossAreaTelegraphView telegraphView;
    private bool impactApplied;
    private bool complete;

    public override bool IsComplete => complete;

    public override bool CanStart(MonsterController owner)
    {
        return base.CanStart(owner) && owner.UnitRoster != null && owner.UnitRoster.CountAliveUnits() > 0;
    }

    protected override void OnBegin()
    {
        target = BossAbilityTargetSelector.Select(
            Owner.UnitRoster.Units,
            Owner.transform.position,
            targetSelection,
            radius);

        if (!target.IsValid)
        {
            complete = true;
            return;
        }

        Owner.FacePosition(target.Position);
        Owner.PlayAttack();
        PlayBossSfx(
            Owner.Data?.BossDefinition?.audio?.areaAttackTelegraph,
            GameAudioCue.BossAreaTelegraph);

        if (telegraphPrefab != null)
        {
            telegraphInstance = Instantiate(telegraphPrefab, target.Position, Quaternion.identity);
            telegraphInstance.transform.localScale = Vector3.one * (radius * 2f);
            telegraphView = telegraphInstance.GetComponent<BossAreaTelegraphView>();
        }
        else
        {
            telegraphView = BossAreaTelegraphView.Create(target.Position, radius, telegraphDuration);
            telegraphInstance = telegraphView.gameObject;
        }
    }

    protected override void OnTick(float deltaTime)
    {
        if (complete)
            return;

        if (telegraphView != null)
        {
            float progress = telegraphDuration <= 0f ? 1f : Mathf.Clamp01(Elapsed / telegraphDuration);
            telegraphView.SetProgress(progress);
        }

        if (!impactApplied && Elapsed >= telegraphDuration)
        {
            impactApplied = true;
            ApplyImpact();
            DestroyTelegraph();
        }

        if (Elapsed >= telegraphDuration + recoveryDuration)
            complete = true;
    }

    protected override void OnCancel()
    {
        DestroyTelegraph();
    }

    protected override void OnEnd()
    {
        DestroyTelegraph();
    }

    protected override void ResetAbility()
    {
        target = default;
        impactApplied = false;
        complete = false;
    }

    private void ApplyImpact()
    {
        PlayBossSfx(
            Owner?.Data?.BossDefinition?.audio?.areaAttackImpact,
            GameAudioCue.BossAreaImpact);

        if (Owner?.UnitRoster?.Units == null)
            return;

        float radiusSquared = radius * radius;
        float damage = Owner.AtkDamage * damageMultiplier;
        foreach (UnitController unit in Owner.UnitRoster.Units)
        {
            if (unit == null || unit.IsDead ||
                (unit.transform.position - target.Position).sqrMagnitude > radiusSquared)
                continue;

            unit.CombatHealth.ApplyDamage(
                new DamageRequest(damage, Owner, DamageOrigin.Skill));

            if (!unit.IsDead)
                ApplyConfiguredStatusEffects(unit);
        }
    }

    private void ApplyConfiguredStatusEffects(UnitController unit)
    {
        BossAreaAttackStatusSettings settings = Owner?.Data?.BossDefinition?.areaAttackStatusEffects;
        if (settings == null || unit?.StatusEffects == null)
            return;

        if (settings.enableSlow)
        {
            unit.StatusEffects.Apply(StatusEffectApplication.Slow(
                settings.slowDuration,
                settings.slowRatio));
        }

        if (settings.enableStun)
            unit.StatusEffects.Apply(StatusEffectApplication.Stun(settings.stunDuration));

        if (settings.enablePoison)
        {
            unit.StatusEffects.Apply(StatusEffectApplication.Poison(
                settings.poisonDuration,
                Owner.AtkDamage * settings.poisonDamageMultiplier,
                Owner,
                settings.poisonTickInterval));
        }

        if (settings.enableBurn)
        {
            unit.StatusEffects.Apply(StatusEffectApplication.Burn(
                settings.burnDuration,
                Owner.AtkDamage * settings.burnDamageMultiplier,
                Owner,
                settings.burnTickInterval));
        }

        if (settings.enableDefenseDown)
        {
            unit.StatusEffects.Apply(StatusEffectApplication.DefenseDown(
                settings.defenseDownDuration,
                settings.defenseDownPercentagePoints));
        }
    }

    private void DestroyTelegraph()
    {
        if (telegraphInstance != null)
            Destroy(telegraphInstance);

        telegraphInstance = null;
        telegraphView = null;
    }

    private static void PlayBossSfx(AudioClipSettings audio, GameAudioCue cue)
    {
        if (audio == null || !audio.IsValid)
            return;

        GameAudioManager.Instance?.PlaySfx(
            audio,
            GameAudioPriority.High,
            cue,
            0.1f);
    }
}
