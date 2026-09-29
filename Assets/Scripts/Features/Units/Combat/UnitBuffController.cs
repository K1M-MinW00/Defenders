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

    public void UpsertBuff(RuntimeBuff buff, StatRefreshPolicy refreshPolicy = StatRefreshPolicy.KeepRatio)
    {
        if (buff == null)
            return;

        RemoveBuffWithoutRefresh(buff.BuffId);
        activeBuffs.Add(buff);
        owner.StatService.Recalculate(refreshPolicy);
    }

    public void UpsertBuffs(
        RuntimeBuff first,
        RuntimeBuff second,
        StatRefreshPolicy refreshPolicy = StatRefreshPolicy.KeepRatio)
    {
        if (first == null && second == null)
            return;

        if (first != null)
        {
            RemoveBuffWithoutRefresh(first.BuffId);
            activeBuffs.Add(first);
        }

        if (second != null)
        {
            RemoveBuffWithoutRefresh(second.BuffId);
            activeBuffs.Add(second);
        }

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
