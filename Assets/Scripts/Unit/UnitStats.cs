using System;
using UnityEngine;

[Serializable]
public class UnitStats
{
    [Min(0)] public float maxHp;
    [Min(0)] public float maxMp;
    [Min(0.1f)] public float speed;
    [Min(0)] public float atk;
    [Min(0)] public float def;
    [Min(0)] public float attackRange;
    [Min(0)] public float detectRange;
    [Min(0.01f)] public float attackPerSec;
}
