//using System.Collections.Generic;
//using UnityEngine;

//public sealed class StageBuffManager : MonoBehaviour
//{
//    [SerializeField] private UnitRoster unitRoster;

//    private readonly List<RuntimeBuff> globalUnitBuffs = new();
//    private readonly List<RuntimeBuff> globalMonsterDebuffs = new();

//    public IReadOnlyList<RuntimeBuff> GlobalUnitBuffs => globalUnitBuffs;
//    public IReadOnlyList<RuntimeBuff> GlobalMonsterDebuffs => globalMonsterDebuffs;

//    public void AddGlobalUnitBuff(RuntimeBuff buff)
//    {
//        if (buff == null)
//            return;

//        globalUnitBuffs.Add(buff);
//        RecalculateAllUnits();
//    }

//    public void AddGlobalMonsterDebuff(RuntimeBuff debuff)
//    {
//        if (debuff == null)
//            return;

//        globalMonsterDebuffs.Add(debuff);
//    }

//    public void RegisterUnit(UnitController unit)
//    {
//        if (unit == null)
//            return;

//        unit.StatService.Recalculate(resetHp: false);
//    }

//    public void OnWaveEnded()
//    {
//        RemoveUntilWaveEnd(globalUnitBuffs);
//        RemoveUntilWaveEnd(globalMonsterDebuffs);

//        AdvanceWaveCount(globalUnitBuffs);
//        AdvanceWaveCount(globalMonsterDebuffs);

//        RecalculateAllUnits();
//    }

//    public void OnStageEnded()
//    {
//        globalUnitBuffs.Clear();
//        globalMonsterDebuffs.Clear();
//        RecalculateAllUnits();
//    }

//    public float GetGlobalUnitAdditive(BuffStatType statType)
//    {
//        float total = 0f;

//        foreach (var buff in globalUnitBuffs)
//        {
//            if (buff.StatType == statType && buff.ValueType == BuffModifyType.Additive)
//                total += buff.Value;
//        }

//        return total;
//    }

//    public float GetGlobalUnitMultiplier(BuffStatType statType)
//    {
//        float factor = 1f;

//        foreach (var buff in globalUnitBuffs)
//        {
//            if (buff.StatType == statType && buff.ValueType == BuffModifyType.Multiplicative)
//                factor *= (1f + buff.Value);
//        }

//        return factor;
//    }

//    public float GetGlobalMonsterAdditive(BuffStatType statType)
//    {
//        float total = 0f;

//        foreach (var debuff in globalMonsterDebuffs)
//        {
//            if (debuff.StatType == statType && debuff.ValueType == BuffModifyType.Additive)
//                total += debuff.Value;
//        }

//        return total;
//    }

//    public float GetGlobalMonsterMultiplier(BuffStatType statType)
//    {
//        float factor = 1f;

//        foreach (var debuff in globalMonsterDebuffs)
//        {
//            if (debuff.StatType == statType && debuff.ValueType == BuffModifyType.Multiplicative)
//                factor *= (1f + debuff.Value);
//        }

//        return factor;
//    }

//    private void RemoveUntilWaveEnd(List<RuntimeBuff> list)
//    {
//        list.RemoveAll(b => b.DurationType == BuffDurationType.UntilWaveEnd);
//    }

//    private void AdvanceWaveCount(List<RuntimeBuff> list)
//    {
//        foreach (var buff in list)
//            buff.AdvanceWave();

//        list.RemoveAll(b => b.IsExpired());
//    }

//    private void RecalculateAllUnits()
//    {
//        if (unitRoster == null)
//            return;

//        foreach (var unit in unitRoster.Units)
//        {
//            if (unit == null)
//                continue;

//            unit.StatService.Recalculate(resetHp: false);
//        }
//    }
//}