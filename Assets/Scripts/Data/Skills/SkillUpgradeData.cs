using System;
using UnityEngine;

[Serializable]
public class SkillUpgradeData
{
    [Min(0)] public int promotionLevel;
    [TextArea] public string description;
}