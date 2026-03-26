using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Active/Cleave", fileName = "Cleave Skill")]
public class Cleave_SkillData : ActiveSkillDataSO
{
    [Header("Projectile")]
    public SwordAura swordAura;
    public float speed = 6f;
    public float lifeTime = 2f;
    public LayerMask targetLayer;

    [Header("Damage")]
    public float baseDamageMultiplier = 1.5f;
    public float promDamageMultiplier = 2f;

    public override IActiveSkill CreateRuntimeSkill()
    {
        return new Cleave_Skill();
    }
}