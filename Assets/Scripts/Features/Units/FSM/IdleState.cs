using UnityEngine;

public class IdleState : IState
{
    private UnitController owner;
    public IdleState(UnitController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.Movement.Stop();
        owner.Animation.PlayIdle();
    }

    public void Update()
    {
        if (owner.IsDead)
            return;

        if (owner.FSMController.TryChangeToSkill())
            return;

        if (!owner.FSMController.TryEnsureTarget())
            return;

        owner.FSMController.ChangeToTargetState();
    }

    public void Exit() { }
}
