using System.Collections.Generic;
using UnityEngine;

public class UnitBuffController : MonoBehaviour
{
    private UnitController owner;
    private readonly List<RuntimeBuff> activeBuffs = new();

#if UNITY_EDITOR
    [System.Serializable]
    private sealed class BuffDebugEntry
    {
        public string buffId;
        public StatType statType;
        public BuffModifyType modifyType;
        public float value;
        public BuffDurationType durationType;
        public float remainingTime;
        public int remainingWaves;

        public void CopyFrom(RuntimeBuff buff)
        {
            buffId = buff.BuffId;
            statType = buff.StatType;
            modifyType = buff.ModifyType;
            value = buff.Value;
            durationType = buff.DurationType;
            remainingTime = buff.RemainingTime;
            remainingWaves = buff.RemainingWaves;
        }
    }

    [SerializeField, HideInInspector] private int debugActiveBuffCount;
    [SerializeField, HideInInspector] private List<BuffDebugEntry> debugActiveBuffs = new();
#endif

    public IReadOnlyList<RuntimeBuff> ActiveBuffs => activeBuffs;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;
        activeBuffs.Clear();
        SyncDebugSnapshot();
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

        SyncDebugSnapshot();
    }

    public void ApplyOrRefreshBuff(
        BuffApplication application,
        StatRefreshPolicy refreshPolicy = StatRefreshPolicy.KeepRatio)
    {
        if (!application.IsValid)
            return;

        if (ApplyOrRefreshWithoutRecalculate(application))
            owner.StatService.Recalculate(refreshPolicy);

        SyncDebugSnapshot();
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

        SyncDebugSnapshot();
    }

    public void RemoveBuff(string buffId, StatRefreshPolicy refreshPolicy = StatRefreshPolicy.KeepRatio)
    {
        int removed = RemoveBuffWithoutRefresh(buffId);
        if (removed > 0)
            owner.StatService.Recalculate(refreshPolicy);

        SyncDebugSnapshot();
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

        SyncDebugSnapshot();
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

        SyncDebugSnapshot();
    }

    public void ClearAllBuffs()
    {
        if (activeBuffs.Count == 0)
            return;

        activeBuffs.Clear();
        owner.StatService.Recalculate(StatRefreshPolicy.KeepRatio);
        SyncDebugSnapshot();
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

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void SyncDebugSnapshot()
    {
#if UNITY_EDITOR
        debugActiveBuffCount = activeBuffs.Count;

        while (debugActiveBuffs.Count < activeBuffs.Count)
            debugActiveBuffs.Add(new BuffDebugEntry());

        if (debugActiveBuffs.Count > activeBuffs.Count)
        {
            debugActiveBuffs.RemoveRange(
                activeBuffs.Count,
                debugActiveBuffs.Count - activeBuffs.Count);
        }

        for (int i = 0; i < activeBuffs.Count; i++)
            debugActiveBuffs[i].CopyFrom(activeBuffs[i]);
#endif
    }
}
