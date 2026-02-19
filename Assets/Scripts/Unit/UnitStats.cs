using System;
using UnityEngine;

[Serializable]
public class UnitStats
{
    [Min(0)] public float atk;
    [Min(0)] public float def;
    [Min(0)] public float maxHp;
    [Min(0)] public float range;
    public static UnitStats operator *(UnitStats s, float m)
    {
        return new UnitStats
        {
            atk = s.atk * m,
            def = s.def * m,
            maxHp = s.maxHp * m,
            range = s.range * m
        };
    }

}
