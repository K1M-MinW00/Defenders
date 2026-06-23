using UnityEngine;

public class DamageUIService : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private StagePoolManager poolManager;
    [SerializeField] private Transform damageUIRoot;

    [Header("Prefab")]
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private int prewarmCnt = 20;


    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private float randomX = 0.15f;


    private void Start()
    {
        if (poolManager != null && damagePopupPrefab != null)
            poolManager.Prewarm(damagePopupPrefab.gameObject, prewarmCnt, PoolCategory.UI);
    }

    public void Show(Vector3 worldPos, float damage)
    {
        if (poolManager == null || damagePopupPrefab == null)
            return;

        // ¾à°£ ·£´ýÀ¸·Î °ãÄ§ ¿ÏÈ­
        float rx = Random.Range(-randomX, randomX);
        Vector3 spawnPos = worldPos + worldOffset + new Vector3(rx, 0f, 0f);

        DamagePopup popup = poolManager.Spawn(damagePopupPrefab, spawnPos, Quaternion.identity, PoolCategory.UI, damageUIRoot);

        if (popup == null)
            return;

        popup.Setup(Mathf.RoundToInt(damage));
    }
}
