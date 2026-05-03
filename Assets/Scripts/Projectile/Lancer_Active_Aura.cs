using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Lancer_Active_Aura : MonoBehaviour, IPoolable
{
    [SerializeField] private SpriteRenderer rangeSrdr;
    private Poolable poolable;
    private CircleCollider2D circleTrigger;

    [Header("Refresh")]
    [SerializeField] private float refreshInterval = 0.2f;
    [SerializeField] private float buffDuration = 0.35f;

    private readonly HashSet<UnitController> unitsInRange = new();
    private readonly List<UnitController> invalidUnits = new();

    private float attackBonusPercent;
    private float attackSpeedBonusPercent;
    private LayerMask allyLayer;

    private string attackBuffId;
    private string attackSpeedBuffId;

    private float lifeTimer;
    private float refreshTimer;
    private bool isActive;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
        circleTrigger = GetComponent<CircleCollider2D>();

        if (circleTrigger != null)
            circleTrigger.isTrigger = true;

        if (poolable == null)
            poolable = gameObject.AddComponent<Poolable>();
    }

    public void Initialize(float duration,float radius,float attackBonusPercent,float attackSpeedBonusPercent,LayerMask allyLayer,string uniqueId)
    {
        this.attackBonusPercent = attackBonusPercent;
        this.attackSpeedBonusPercent = attackSpeedBonusPercent;
        this.allyLayer = allyLayer;

        attackBuffId = $"LancerFlag_Attack_{uniqueId}";
        attackSpeedBuffId = $"LancerFlag_AttackSpeed_{uniqueId}";

        circleTrigger.radius = radius;

        float spriteSize = rangeSrdr.sprite.bounds.size.x;
        float targetDiameter = radius * 2f;
        float scale = targetDiameter / spriteSize;

        rangeSrdr.transform.localScale = new Vector3(scale, scale, 1f);

        lifeTimer = duration;
        refreshTimer = 0f;
        isActive = true;
    }

    private void Update()
    {
        if (!isActive)
            return;

        lifeTimer -= Time.deltaTime;
        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            RefreshBuffs();
            refreshTimer = refreshInterval;
        }

        if (lifeTimer <= 0f)
            ReturnToPool();
    }

    private void RefreshBuffs()
    {
        if (unitsInRange.Count == 0)
            return;

        List<UnitController> invalidUnits = null;

        foreach (UnitController unit in unitsInRange)
        {
            if (unit == null || unit.IsDead)
            {
                invalidUnits.Add(unit);
                continue;
            }

            ApplyOrRefreshBuff(unit);
        }

        if (invalidUnits == null)
            return;

        foreach (UnitController unit in invalidUnits)
            unitsInRange.Remove(unit);
    }

    private void ApplyOrRefreshBuff(UnitController unit)
    {
        RuntimeBuff attackBuff = new RuntimeBuff(
            buffId: attackBuffId,
            statType: BuffStatType.Attack,
            modifyType: BuffModifyType.Multiplicative,
            value: attackBonusPercent,
            durationType: BuffDurationType.Timed,
            durationSeconds: buffDuration
        );

        RuntimeBuff attackSpeedBuff = new RuntimeBuff(
            buffId: attackSpeedBuffId,
            statType: BuffStatType.AttackPerSec,
            modifyType: BuffModifyType.Multiplicative,
            value: attackSpeedBonusPercent,
            durationType: BuffDurationType.Timed,
            durationSeconds: buffDuration
        );

        unit.BuffController.RemoveBuff(attackBuffId, StatRefreshPolicy.KeepRatio);
        unit.BuffController.RemoveBuff(attackSpeedBuffId, StatRefreshPolicy.KeepRatio);

        unit.BuffController.AddBuff(attackBuff, StatRefreshPolicy.KeepRatio);
        unit.BuffController.AddBuff(attackSpeedBuff, StatRefreshPolicy.KeepRatio);
    }

    private void RemoveBuff(UnitController unit)
    {
        if (unit == null)
            return;

        unit.BuffController.RemoveBuff(attackBuffId, StatRefreshPolicy.KeepRatio);
        unit.BuffController.RemoveBuff(attackSpeedBuffId, StatRefreshPolicy.KeepRatio);
    }

    private void CleanupAllBuffs()
    {
        if (unitsInRange.Count > 0)
        {
            foreach (UnitController unit in unitsInRange)
            {
                if (unit == null)
                    continue;

                RemoveBuff(unit);
            }
        }

        unitsInRange.Clear();
        invalidUnits.Clear();
    }

    private void ReturnToPool()
    {
        if (!isActive)
            return;

        CleanupAllBuffs();

        isActive = false;
        poolable.ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & allyLayer.value) == 0)
            return;

        if (!other.TryGetComponent<UnitController>(out UnitController unit))
            return;

        if (unit == null || unit.IsDead)
            return;

        if (unitsInRange.Add(unit))
            ApplyOrRefreshBuff(unit);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & allyLayer.value) == 0)
            return;

        if (!other.TryGetComponent<UnitController>(out UnitController unit))
            return;

        if (!unitsInRange.Remove(unit))
            return;

        RemoveBuff(unit);
    }

    public void OnSpawn()
    {
        isActive = false;
        unitsInRange.Clear();
        invalidUnits.Clear();
    }

    public void OnDespawn()
    {
        CleanupAllBuffs();
        
        isActive = false;

        attackBonusPercent = 0f;
        attackSpeedBonusPercent = 0f;
        allyLayer = 0;
        attackBuffId = null;
        attackSpeedBuffId = null;
        lifeTimer = 0f;
        refreshTimer = 0f;
    }
}