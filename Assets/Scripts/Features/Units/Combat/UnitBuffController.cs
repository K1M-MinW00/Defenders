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

    public void ApplyOrRefreshBuff(
        BuffApplication application,
        StatRefreshPolicy refreshPolicy = StatRefreshPolicy.KeepRatio)
    {
        if (!application.IsValid)
            return;

        if (ApplyOrRefreshWithoutRecalculate(application))
            owner.StatService.Recalculate(refreshPolicy);
    }

    public void ApplyOrRefreshBuffs(
        BuffApplication first,
        BuffApplication second,
        StatRefreshPolicy refreshPolicy = StatRefreshPolicy.KeepRatio)
    {
        bool statChanged = ApplyOrRefreshWithoutRecalculate(first);
        statChanged |= ApplyOrRefreshWithoutRecalculate(second);

        if (statChanged)
            owner.StatService.Recalculate(refreshPolicy);
    }

    public void RemoveBuff(string buffId, StatRefreshPolicy refreshPolicy = StatRefreshPolicy.KeepRatio)
    {
        int removed = RemoveBuffWithoutRefresh(buffId);
        if (removed > 0)
            owner.StatService.Recalculate(refreshPolicy);
    }

    public void RemoveBuffs(
        string firstBuffId,
        string secondBuffId,
        StatRefreshPolicy refreshPolicy = StatRefreshPolicy.KeepRatio)
    {
        int removed = RemoveBuffWithoutRefresh(firstBuffId);
        removed += RemoveBuffWithoutRefresh(secondBuffId);

        if (removed > 0)
            owner.StatService.Recalculate(refreshPolicy);
    }

    private int RemoveBuffWithoutRefresh(string buffId)
    {
        return activeBuffs.RemoveAll(x => x.BuffId == buffId);
    }

    private bool ApplyOrRefreshWithoutRecalculate(BuffApplication application)
    {
        if (!application.IsValid)
            return false;

        for (int i = 0; i < activeBuffs.Count; i++)
        {
            RuntimeBuff current = activeBuffs[i];
            if (current.BuffId != application.BuffId)
                continue;

            if (current.CanRefreshFrom(application))
                return current.RefreshFrom(application);

            activeBuffs[i] = application.CreateRuntimeBuff();
            return true;
        }

        activeBuffs.Add(application.CreateRuntimeBuff());
        return true;
    }

    public void CompleteWave()
    {
        bool changed = false;

        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            RuntimeBuff buff = activeBuffs[i];
            if (!buff.CompleteWave())
                continue;

            activeBuffs.RemoveAt(i);
            changed = true;
        }

        if (changed)
            owner.StatService.Recalculate(StatRefreshPolicy.KeepRatio);
    }

    public void ClearAllBuffs()
    {
        if (activeBuffs.Count == 0)
            return;

        activeBuffs.Clear();
        owner.StatService.Recalculate(StatRefreshPolicy.KeepRatio);
    }

    public float GetAdditive(StatType statType)
    {
        float total = 0f;

        foreach (RuntimeBuff buff in activeBuffs)
        {
            if (buff.StatType != statType)
                continue;

            if (buff.ModifyType != BuffModifyType.Flat)
                continue;

            total += buff.Value;
        }

        return total;
    }

    public float GetMultiplier(StatType statType)
    {
        float total = 1f;

        foreach (RuntimeBuff buff in activeBuffs)
        {
            if (buff.StatType != statType)
                continue;

            if (buff.ModifyType != BuffModifyType.Percent)
                continue;

            total *= (1f + buff.Value);
        }

        return total;
    }
}
