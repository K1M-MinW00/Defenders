using System.Collections;
using System.Net.NetworkInformation;
using System.Xml;
using UnityEngine;

public class SuicideAttack : MonoBehaviour, IMonsterAttack
{
    [Header("Cast")]
    [SerializeField] private float castTime = .8f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private LayerMask targetMask;

    [Header("VFX")]
    [SerializeField] private SpriteRenderer bodySrdr;
    [SerializeField] private float scaleMultiplier = 2f;

    private bool casting;
    private UnitInstance target;

    private void Awake()
    {
        if (bodySrdr == null)
            bodySrdr = GetComponentInChildren<SpriteRenderer>();
    }
    public void Execute(MonsterController ctx)
    {
        if (ctx == null)
            return;

        if (casting)
            return;

        target = ctx.TargetUnit;
        if (target == null || !target.IsAlive)
            return;

        casting = true;
        ctx.StartCoroutine(CastAndExplode(ctx));
    }

    private IEnumerator CastAndExplode(MonsterController ctx)
    {
        ctx.Agent.isStopped = true;

        float timer = 0f;

        while(timer < castTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / castTime);

            bodySrdr.color = Color.Lerp(Color.white, Color.red, t);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * scaleMultiplier, t);

            yield return null;
        }

        if(target == null || !target.IsAlive)
        {
            ResetVisual();
            casting = false;
            yield return null;
        }

        Vector2 center = ctx.transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, explosionRadius,targetMask);
        
        for(int i=0;i<hits.Length;i++)
        {
            var ui = hits[i].GetComponent<UnitInstance>();
            if (ui == null || !ui.IsAlive)
                continue;

            ui.TakeDamage(ctx.AtkDamage);
        }

        ResetVisual();
        ctx.Health.Kill();
        casting = false;
    }

    private void ResetVisual()
    {
        bodySrdr.color = Color.white;
        transform.localScale = Vector3.one;
    }
}
