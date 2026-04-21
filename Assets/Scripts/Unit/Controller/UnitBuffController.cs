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
        activeBuffs.Clear();
    }

    private void Update()
    {
        if (activeBuffs.Count == 0)
            return;

        bool changed = false;

        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            RuntimeBuff buff = activeBuffs[i];

            if (buff.DurationType != BuffDurationType.Timed)
                continue;

            buff.Tick(Time.deltaTime);

            if (buff.IsExpired())
            {
                activeBuffs.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
            owner.StatService.Recalculate(StatRefreshPolicy.KeepRatio);
    }

    public void AddBuff(RuntimeBuff buff, StatRefreshPolicy refreshPolicy = StatRefreshPolicy.KeepRatio)
    {
        if (buff == null)
            return;

        activeBuffs.Add(buff);
        owner.StatService.Recalculate(refreshPolicy);
    }

    public void RemoveBuff(string buffId, StatRefreshPolicy refreshPolicy = StatRefreshPolicy.KeepRatio)
    {
        int removed = activeBuffs.RemoveAll(x => x.BuffId == buffId);
        if (removed > 0)
            owner.StatService.Recalculate(refreshPolicy);
    }

    public void ClearWaveBuffs()
    {
        int removed = activeBuffs.RemoveAll(x => x.DurationType == BuffDurationType.UntilWaveEnd);
        if (removed > 0)
            owner.StatService.Recalculate(StatRefreshPolicy.KeepRatio);
    }

    public void AdvanceWaveBuffs()
    {
        bool changed = false;

        foreach (RuntimeBuff buff in activeBuffs)
        {
            if (buff.DurationType != BuffDurationType.WaveCount)
                continue;

            buff.AdvanceWave();
            changed = true;
        }

        int removed = activeBuffs.RemoveAll(x => x.IsExpired());
        if (changed || removed > 0)
            owner.StatService.Recalculate(StatRefreshPolicy.KeepRatio);
    }

    public void ClearAllBuffs()
    {
        if (activeBuffs.Count == 0)
            return;

        activeBuffs.Clear();
        owner.StatService.Recalculate(StatRefreshPolicy.KeepRatio);
    }

    public float GetAdditive(BuffStatType statType)
    {
        float total = 0f;

        foreach (RuntimeBuff buff in activeBuffs)
        {
            if (buff.StatType != statType)
                continue;

            if (buff.ModifyType != BuffModifyType.Additive)
                continue;

            total += buff.Value;
        }

        return total;
    }

    public float GetMultiplier(BuffStatType statType)
    {
        float total = 1f;

        foreach (RuntimeBuff buff in activeBuffs)
        {
            if (buff.StatType != statType)
                continue;

            if (buff.ModifyType != BuffModifyType.Multiplicative)
                continue;

            total *= (1f + buff.Value);
        }

        return total;
    }
} 