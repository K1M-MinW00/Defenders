using System.Collections.Generic;
using UnityEngine;

public class UnitBuffController : MonoBehaviour
{
    private UnitController owner;
    private readonly List<RuntimeBuff> activeBuffs = new();

    public IReadOnlyList<RuntimeBuff> ActiveBuffs => activeBuffs;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;
    }

    public void AddBuff(RuntimeBuff buff)
    {
        if (buff == null)
            return;

        activeBuffs.Add(buff);
        owner.StatService.Recalculate(resetHp: false);
    }

    public void RemoveBuff(string buffId)
    {
        int removed = activeBuffs.RemoveAll(b => b.BuffId == buffId);
        if (removed > 0)
            owner.StatService.Recalculate(resetHp: false);
    }

    public void ClearWaveBuffs()
    {
        int removed = activeBuffs.RemoveAll(b => b.DurationType == BuffDurationType.UntilWaveEnd);
        if (removed > 0)
            owner.StatService.Recalculate(resetHp: false);
    }

    public void AdvanceWaveBuffs()
    {
        bool changed = false;

        foreach (var buff in activeBuffs)
        {
            if (buff.DurationType == BuffDurationType.WaveCount)
            {
                buff.AdvanceWave();
                changed = true;
            }
        }

        int removed = activeBuffs.RemoveAll(b => b.IsExpired());
        if (changed || removed > 0)
            owner.StatService.Recalculate(resetHp: false);
    }

    public void ClearAllBuffs()
    {
        if (activeBuffs.Count == 0)
            return;

        activeBuffs.Clear();
        owner.StatService.Recalculate(resetHp: false);
    }

    public float GetAdditive(BuffStatType statType)
    {
        float total = 0f;

        foreach (var buff in activeBuffs)
        {
            if (buff.StatType == statType && buff.ValueType == BuffModifyType.Additive)
                total += buff.Value;
        }

        return total;
    }

    public float GetMultiplier(BuffStatType statType)
    {
        float factor = 1f;

        foreach (var buff in activeBuffs)
        {
            if (buff.StatType == statType && buff.ValueType == BuffModifyType.Multiplicative)
                factor *= (1f + buff.Value);
        }

        return factor;
    }
}