using UnityEngine;

public class DamageUIService : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private StagePoolManager poolManager;
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private Transform damageUIRoot;

    [Header("Prefab")]
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private int prewarmCnt = 20;


    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private float randomX = 0.15f;


    private void Awake()
    {
        if (poolManager != null && damagePopupPrefab != null)
            poolManager.Prewarm(damagePopupPrefab.gameObject, prewarmCnt, PoolCategory.UI);
    }

    private void OnEnable()
    {
        if (unitRoster != null)
            unitRoster.OnUnitDamaged += HandleUnitDamaged;
    }

    private void OnDisable()
    {
        if (unitRoster != null)
            unitRoster.OnUnitDamaged -= HandleUnitDamaged;
    }

    public void Show(Vector3 worldPos, DamageResult result)
    {
        if (!result.WasApplied || poolManager == null || damagePopupPrefab == null)
            return;

        // 약간 랜덤으로 겹침 완화
        float rx = Random.Range(-randomX, randomX);
        Vector3 spawnPos = worldPos + worldOffset + new Vector3(rx, 0f, 0f);

        DamagePopup popup = poolManager.Spawn(damagePopupPrefab, spawnPos, Quaternion.identity, PoolCategory.UI, damageUIRoot);

        if (popup == null)
            return;

        popup.Setup(Mathf.RoundToInt(result.AppliedAmount), result.WasCritical);
    }

    private void HandleUnitDamaged(UnitController unit, DamageResult result)
    {
        if (unit == null)
            return;

        Show(unit.transform.position, result);
    }
}
