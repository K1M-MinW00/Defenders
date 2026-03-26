using UnityEngine;

public class Cleave_Skill : ActiveSkillBase
{
    private Cleave_SkillData CleaveData => data as Cleave_SkillData;


    public override bool CanUseSkill()
    {
        if (!base.CanUseSkill())
            return false;

        if(CleaveData == null)
            return false;

        return true;
    }
    public override void BeginSkill()
    {
        base.BeginSkill();
        owner.StopMovement();
    }

    public override void OnSkillHit()
    {
        Vector2 dir = owner.GetAttackDirection();
        Vector2 spawnPos = (Vector2)owner.transform.position;

        float dmg = owner.Attack;
        float value = ActiveTier2Unlocked == false ? CleaveData.baseDamageMultiplier : CleaveData.promDamageMultiplier;
        dmg *= value;

        SwordAura aura = Instantiate(CleaveData.swordAura,spawnPos,Quaternion.identity);
        aura.Initialize(dmg, dir, CleaveData.speed, CleaveData.lifeTime,CleaveData.targetLayer);
    }

    public override void EndSkill()
    {
        base.EndSkill();
    }

    public override void CancelSkill()
    {
        base.CancelSkill();
    }

}