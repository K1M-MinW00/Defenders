using System;
using UnityEngine;

public class UnitEnergy : MonoBehaviour
{
    private UnitController owner;
    private bool isCombatPhase;

    public event Action<float, float> OnEnergyChanged;
    public event Action OnEnergyFull;

    public float Current => owner.Runtime.CurrentEnergy;
    public float Max => owner.Runtime.MaxEnergy;
    public bool IsFull => Current >= Max;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;
    }

    public void SetCombatPhase(bool active)
    {
        isCombatPhase = active;
    }

    public void Tick(float deltaTime)
    {
        if (owner.Runtime == null) return;
        if (!owner.Runtime.CanRecoverEnergy) return;
        if (!isCombatPhase) return;
        if (IsFull) return;

        Add(owner.Runtime.FinalStats.EnergyRecovery * deltaTime);
    }

    public void Add(float amount)
    {
        if (owner.Runtime == null || amount <= 0f)
            return;

        float prev = owner.Runtime.CurrentEnergy;
        owner.Runtime.CurrentEnergy = Mathf.Clamp(prev + amount, 0f, Max);

        OnEnergyChanged?.Invoke(Current, Max);

        if (prev < Max && owner.Runtime.CurrentEnergy >= Max)
            OnEnergyFull?.Invoke();
    }

    public void ConsumeAll()
    {
        if (owner.Runtime == null) return;

        owner.Runtime.CurrentEnergy = 0f;
        OnEnergyChanged?.Invoke(Current, Max);
    }

    public void ResetToZero()
    {
        if (owner.Runtime == null) return;

        owner.Runtime.CurrentEnergy = 0f;
        OnEnergyChanged?.Invoke(Current, Max);
    }
}