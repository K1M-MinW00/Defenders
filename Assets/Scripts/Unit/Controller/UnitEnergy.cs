using System;
using UnityEngine;

public class UnitEnergy : MonoBehaviour
{
    private UnitController owner;
    [SerializeField] private float energyRecovery = 10f;
    private float currentEnergy;
    private float maxEnergy = 100f;
    private bool isCombatPhase;

    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public bool IsFull => CurrentEnergy >= MaxEnergy;

    public event Action<float, float> OnEnergyChanged;
    public event Action OnEnergyFull;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;
        currentEnergy = 0f;

        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    public void SetCombatPhase(bool active)
    {
        isCombatPhase = active;
    }

    public void Tick(float deltaTime)
    {
        if (!owner.Runtime.CanUseActive) return;
        if (!isCombatPhase) return;
        if (IsFull) return;

        Add(energyRecovery * deltaTime);
    }

    public void Add(float amount)
    {
        if (owner.Runtime == null || amount <= 0f)
            return;

        float prev = currentEnergy;
        currentEnergy = Mathf.Clamp(prev + amount, 0f, MaxEnergy);

        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

        if (prev < maxEnergy && currentEnergy >= maxEnergy)
            OnEnergyFull?.Invoke();
    }

    public void ConsumeAll()
    {
        currentEnergy = 0f;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }
}