public sealed class MonsterAbilityState : IState
{
    private readonly MonsterController owner;
    private IMonsterAbility ability;
    private bool completed;

    public MonsterAbilityState(MonsterController owner)
    {
        this.owner = owner;
    }

    public bool TryConfigure(IMonsterAbility nextAbility)
    {
        if (ability != null || nextAbility == null || !nextAbility.CanStart(owner))
            return false;

        ability = nextAbility;
        completed = false;
        return true;
    }

    public void Enter()
    {
        owner.StopMovement();
        owner.CancelAttack();
        ability?.Begin(owner);
    }

    public void Update()
    {
        if (ability == null)
        {
            owner.CompleteCurrentAbility();
            return;
        }

        ability.Tick(UnityEngine.Time.deltaTime);
        if (!ability.IsComplete)
            return;

        completed = true;
        owner.CompleteCurrentAbility();
    }

    public void Exit()
    {
        if (completed)
            ability?.End();
        else
            ability?.Cancel();

        ability = null;
        completed = false;
    }
}
