using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Lancer_Active_Aura : MonoBehaviour
{
    [SerializeField] private SpriteRenderer rangeSrdr;

    [Header("Refresh")]
    [SerializeField] private float refreshInterval = 0.2f;
    [SerializeField] private float buffDuration = 0.35f;

    private readonly HashSet<UnitController> unitsInRange = new();

    private float attackBonusPercent = 0.20f;
    private float attackSpeedBonusPercent = 0.15f;
    private LayerMask allyLayer;
    private string attackBuffId;
    private string attackSpeedBuffId;

    private float lifeTimer;
    private float refreshTimer;
    private CircleCollider2D circleTrigger;

    public void Initialize(float duration,float radius,float attackBonusPercent,float attackSpeedBonusPercent,LayerMask allyLayer,string uniqueId)
    {
        this.attackBonusPercent = attackBonusPercent;
        this.attackSpeedBonusPercent = attackSpeedBonusPercent;
        this.allyLayer = allyLayer;

        attackBuffId = $"LancerFlag_Attack_{uniqueId}";
        attackSpeedBuffId = $"LancerFlag_AttackSpeed_{uniqueId}";

        circleTrigger = GetComponent<CircleCollider2D>();

        circleTrigger.isTrigger = true;
        circleTrigger.radius = radius;

        float spriteSize = rangeSrdr.sprite.bounds.size.x;
        float targetDiameter = radius * 2f;
        float scale = targetDiameter / spriteSize;

        rangeSrdr.transform.localScale = new Vector3(scale, scale, 1f);

        lifeTimer = duration;
        refreshTimer = 0f;
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            RefreshBuffs();
            refreshTimer = refreshInterval;
        }

        if (lifeTimer <= 0f)
        {
            CleanupAllBuffs();
            Destroy(gameObject);
        }
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
                invalidUnits ??= new List<UnitController>();
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
        foreach (UnitController unit in unitsInRange)
        {
            if (unit == null)
                continue;

            RemoveBuff(unit);
        }

        unitsInRange.Clear();
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
        if (!other.TryGetComponent<UnitController>(out UnitController unit))
            return;

        if (!unitsInRange.Remove(unit))
            return;

        RemoveBuff(unit);
    }
}