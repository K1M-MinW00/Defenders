using System;
using UnityEngine;

[Serializable]
public struct SkillFloatUpgradeValue
{
    [SerializeField] private float baseValue;
    [SerializeField] private float upgradedValue;

    public float BaseValue => baseValue;
    public float UpgradedValue => upgradedValue;

    public SkillFloatUpgradeValue(float baseValue, float upgradedValue)
    {
        this.baseValue = baseValue;
        this.upgradedValue = upgradedValue;
    }

    public float Resolve(bool upgraded) => upgraded ? upgradedValue : baseValue;
}

[Serializable]
public struct SkillIntUpgradeValue
{
    [SerializeField] private int baseValue;
    [SerializeField] private int upgradedValue;

    public int BaseValue => baseValue;
    public int UpgradedValue => upgradedValue;

    public SkillIntUpgradeValue(int baseValue, int upgradedValue)
    {
        this.baseValue = baseValue;
        this.upgradedValue = upgradedValue;
    }

    public int Resolve(bool upgraded) => upgraded ? upgradedValue : baseValue;
}
