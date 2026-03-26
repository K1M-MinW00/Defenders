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
        Vector2 dir = GetAttackDirection();
        Vector2 spawnPos = (Vector2)owner.transform.position;

        float dmg = owner.Attack * (promotionLevel > 1 ? CleaveData.baseDamageMultiplier : CleaveData.promDamageMultiplier);
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

    private Vector2 GetAttackDirection()
    {
        if(owner.Target != null && !owner.Target.Health.IsDead)
        {
            Vector2 dirToTarget = ((Vector2)owner.Target.transform.position - (Vector2)owner.transform.position);
            return dirToTarget.normalized;
        }
        return owner.GetFacingDirection();
    }
}