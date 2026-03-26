using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatService
{
    public event Action OnAnyUnitDamaged;

    private readonly HashSet<UnitController> units = new();
    private readonly HashSet<MonsterController> monsters = new();

    public void RegisterUnit(UnitController unit)
    {
        if (unit == null)
            return;

        units.Add(unit);
    }

    public void UnregisterUnit(UnitController unit)
    {
        if(unit == null) 
            return;

        units.Remove(unit);
    }

    public void RegisterMonster(MonsterController monster)
    {
        if (monster == null)
            return;

        monsters.Add(monster);
    }

    public void UnregisterMonster(MonsterController monster)
    {
        if (monster == null)
            return;

        monsters.Remove(monster);
    }

    public void NotifyAnyUnitDamaged()
    {
        OnAnyUnitDamaged?.Invoke();
    }

    public MonsterController FindClosestMonster(Vector3 from)
    {
        MonsterController best = null;
        float bestD = float.MaxValue;

        foreach(var monster in monsters)
        {
            if (monster == null || monster.Health.IsDead)
                continue;

            float sqr = (monster.transform.position - from).sqrMagnitude;
            if(sqr < bestD)
            {
                bestD = sqr;
                best = monster;
            }
        }
        return best;
    }

    public UnitController FindClosestUnit(Vector3 from)
    {
        UnitController best = null;
        float bestD = float.MaxValue;

        foreach(var unit in units)
        {
            if (unit == null || unit.IsDead)
                continue;

            float sqr = (unit.transform.position - from).sqrMagnitude;

            if(sqr < bestD)
            {
                bestD = sqr;
                best = unit;
            }
        }
        return best;
    }

    public void ClearMonsters()
    {
        monsters.Clear();
    }
}
