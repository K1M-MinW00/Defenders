using System.Collections;
using UnityEngine;

public class SuicideAttack : MonoBehaviour, IMonsterAttack
{
    protected MonsterController owner;

    [Header("Cast")]
    [SerializeField] private float castTime = .8f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private LayerMask targetMask;

    [Header("VFX")]
    [SerializeField] private SpriteRenderer bodySrdr;
    [SerializeField] private float scaleMultiplier = 2f;

    private bool casting;
    private Coroutine _castRoutine;

    private UnitController target;

    private void Awake()
    {
        if (owner == null)
            owner = GetComponent<MonsterController>();

        if (bodySrdr == null)
            bodySrdr = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnDisable()
    {
        ResetVisual();
        casting = false;
        target = null;
    }


    private IEnumerator CastAndExplode(MonsterController ctx)
    {
        ctx.Agent.isStopped = true;

        float timer = 0f;

        while(timer < castTime)
        {
            if(ctx == null || ctx.Health.IsDead)
            {
                CleanupCast();
                yield break;
            }

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / castTime);

            bodySrdr.color = Color.Lerp(Color.white, Color.red, t);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * scaleMultiplier, t);

            yield return null;
        }

        if(target == null || !target.IsAlive)
        {
            CleanupCast();
            yield break;
        }

        Vector2 center = ctx.transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, explosionRadius,targetMask);
        
        for(int i=0;i<hits.Length;i++)
        {
            var ui = hits[i].GetComponent<UnitController>();
            if (ui == null || !ui.IsAlive)
                continue;

            ui.TakeDamage(ctx.AtkDamage);
        }

        CleanupCast();
        ctx.Health.Kill();
    }

    private void ResetVisual()
    {
        bodySrdr.color = Color.white;
        transform.localScale = Vector3.one;
    }

    private void CleanupCast()
    {
        if (!casting)
            return;

        casting = false;
        ResetVisual();
        target = null;
    }

    public bool CanAttack()
    {
        if (owner == null || owner.Health.IsDead)
            return false;

        if (!owner.IsTargetInAttackRange())
            return false;

        return true;
    }

    public bool TryAttack(UnitController target)
    {
        if (!CanAttack())
            return false;

        if (target == null || !target.IsAlive)
            return false;

        casting = true;
        _castRoutine = owner.StartCoroutine(CastAndExplode(owner));

        return true;
    }
}
