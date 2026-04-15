using System;
using UnityEngine;

public class UnitAnimationController : MonoBehaviour
{
    [SerializeField] private ModelView view;

    private UnitController owner;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;

        if (view == null)
            view = GetComponentInChildren<ModelView>();
    }

    public void PlayIdle() => view?.PlayIdle();
    public void PlayMove() => view?.PlayMove();
    public void PlayAttack() => view?.PlayAttack();
    public void PlaySkill() => view?.PlaySkill();
    public void PlayDie() => view?.PlayDie();

    public void FaceTarget(MonsterController target)
    {
        if (target == null) return;
        view?.FaceTo(transform.position, target.transform.position);
    }

    public Vector2 GetFacingDirection()
    {
        return view != null ? view.GetFacingDirection() : Vector2.right;
    }

    public void FaceTo(Vector3 from, Vector3 to)
    {
        view?.FaceTo(from, to);
    }
}