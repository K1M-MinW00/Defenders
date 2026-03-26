using System;
using UnityEngine;

[Serializable]
public class SkillUpgradeEntry
{
    [Header("Unlock / Upgrade")]
    [Min(0)] public int promotionLevel;
    public bool unlocksSkill = false;

    [Header("UI Text")]
    [TextArea] public string description;
}