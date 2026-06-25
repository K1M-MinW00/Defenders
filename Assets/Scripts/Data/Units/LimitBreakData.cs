using System;
using UnityEngine;

[Serializable]
public class LimitBreakData
{
    public StatType statType;
    public float value;

    [TextArea]
    public string description;
}