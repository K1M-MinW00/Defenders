using System.Collections.Generic;
using UnityEngine;

public class DamageUIService : MonoBehaviour
{
    private sealed class PendingDamage
    {
        public float Amount;
        public float ShowAt;
        public Vector3 WorldPosition;
    }

    [Header("Refs")]
    [SerializeField] private StagePoolManager poolManager;
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private Transform damageUIRoot;

    [Header("Prefab")]
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private int prewarmCnt = 20;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new(0f, 0.8f, 0f);
    [SerializeField] private float randomX = 0.15f;

    [Header("Readability")]
    [SerializeField, Min(0f)] private float normalDamageMergeWindow = 0.15f;

    private readonly Dictionary<MonsterHealth, PendingDamage> pendingDamage = new();
    private readonly List<MonsterHealth> flushBuffer = new();

    private void Awake()
    {
        if (poolManager != null && damagePopupPrefab != null)
            poolManager.Prewarm(damagePopupPrefab.gameObject, prewarmCnt, PoolCategory.UI);
    }

    private void OnEnable()
    {
        if (unitRoster != null)
            unitRoster.OnUnitHealed += HandleUnitHealed;
    }

    private void OnDisable()
    {
        if (unitRoster != null)
            unitRoster.OnUnitHealed -= HandleUnitHealed;

        pendingDamage.Clear();
        flushBuffer.Clear();
    }

    private void Update()
    {
        if (pendingDamage.Count == 0)
            return;

        float now = Time.time;
        flushBuffer.Clear();

        foreach (KeyValuePair<MonsterHealth, PendingDamage> pair in pendingDamage)
        {
            if (now >= pair.Value.ShowAt)
                flushBuffer.Add(pair.Key);
        }

        for (int i = 0; i < flushBuffer.Count; i++)
        {
            MonsterHealth target = flushBuffer[i];
            if (!pendingDamage.Remove(target, out PendingDamage value))
                continue;

            SpawnPopup(value.WorldPosition, popup => popup.SetupDamage(value.Amount, false));
        }
    }

    public void ShowMonsterDamage(MonsterHealth target, DamageResult result)
    {
        if (target == null || !result.WasApplied)
            return;

        Vector3 worldPosition = target.transform.position;
        if (result.WasCritical || normalDamageMergeWindow <= 0f)
        {
            SpawnPopup(worldPosition, popup => popup.SetupDamage(result.AppliedAmount, result.WasCritical));
            return;
        }

        if (!pendingDamage.TryGetValue(target, out PendingDamage pending))
        {
            pending = new PendingDamage();
            pendingDamage.Add(target, pending);
        }

        pending.Amount += result.AppliedAmount;
        pending.WorldPosition = worldPosition;
        pending.ShowAt = Time.time + normalDamageMergeWindow;
    }

    public void FlushMonsterDamage(MonsterHealth target)
    {
        if (target == null || !pendingDamage.Remove(target, out PendingDamage pending))
            return;

        SpawnPopup(pending.WorldPosition, popup => popup.SetupDamage(pending.Amount, false));
    }

    private void HandleUnitHealed(UnitController unit, float amount)
    {
        if (unit == null || amount <= 0f)
            return;

        SpawnPopup(unit.transform.position, popup => popup.SetupHeal(amount));
    }

    private void SpawnPopup(Vector3 worldPosition, System.Action<DamagePopup> setup)
    {
        if (poolManager == null || damagePopupPrefab == null)
            return;

        float randomOffset = Random.Range(-randomX, randomX);
        Vector3 spawnPosition = worldPosition + worldOffset + new Vector3(randomOffset, 0f, 0f);
        DamagePopup popup = poolManager.Spawn(
            damagePopupPrefab,
            spawnPosition,
            Quaternion.identity,
            PoolCategory.UI,
            damageUIRoot);

        if (popup != null)
            setup?.Invoke(popup);
    }
}
