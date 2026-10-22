using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MonsterController))]
[RequireComponent(typeof(BossShieldController))]
public sealed class BossController : MonoBehaviour
{
    [SerializeField] private BossDefinitionSO definition;

    private readonly List<BossAbilityBehaviour> abilities = new();
    private MonsterController owner;
    private BossShieldController shieldController;
    private bool initialized;

    public BossDefinitionSO Definition => definition;
    public BossShieldController Shield => shieldController;
    public bool IsEnraged { get; private set; }

    public event Action Enraged;

    public void Configure(BossDefinitionSO bossDefinition)
    {
        definition = bossDefinition;
    }

    private void Awake()
    {
        shieldController = GetComponent<BossShieldController>();
        CacheAbilities();
    }

    private void Update()
    {
        if (!initialized || owner == null || !owner.CanRunBehavior || owner.IsUsingAbility)
            return;

        foreach (BossAbilityBehaviour ability in abilities)
        {
            if (ability != null && owner.TryUseAbility(ability))
                break;
        }
    }

    public void Initialize(MonsterController monster)
    {
        ResetRuntime();
        owner = monster;
        CacheAbilities();

        foreach (BossAbilityBehaviour ability in abilities)
            ability.Initialize(owner);

        shieldController?.Initialize(owner, definition);
        initialized = definition != null;
    }

    public bool ActivateEnrage()
    {
        if (!initialized || IsEnraged || owner == null || owner.IsDead)
            return false;

        IsEnraged = true;
        owner.SetCombatMultipliers(
            definition.enragedDamageMultiplier,
            definition.enragedAttackSpeedMultiplier);
        PlayEnrageSound();
        Enraged?.Invoke();
        return true;
    }

    public void ResetRuntime()
    {
        foreach (BossAbilityBehaviour ability in abilities)
            ability?.ResetRuntime();

        shieldController?.ResetRuntime();
        owner?.SetCombatMultipliers(1f, 1f);
        owner = null;
        initialized = false;
        IsEnraged = false;
        Enraged = null;
    }

    private void CacheAbilities()
    {
        abilities.Clear();
        GetComponents(abilities);
        abilities.Sort((left, right) => right.Priority.CompareTo(left.Priority));
    }

    private void PlayEnrageSound()
    {
        AudioClipSettings audio = definition?.audio?.enrage;
        if (audio == null || !audio.IsValid)
            return;

        GameAudioManager.Instance?.PlaySfx(
            audio,
            GameAudioPriority.High,
            GameAudioCue.BossEnrage,
            0.1f);
    }
}
