using UnityEngine;

public class DamageUIService : MonoBehaviour
{
    public static DamageUIService Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private ObjectPool pool;
    [SerializeField] private Transform popupCanvasRoot;

    [Header("Pool Key")]
    [SerializeField] private string poolKey = "DamagePopup";

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private float randomX = 0.15f;

    [SerializeField] private GameObject damagePopup;
    [SerializeField] private int prewarmCnt = 20;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Init(ObjectPool pool)
    {
        this.pool = pool;
        pool.Prewarm(poolKey, damagePopup, prewarmCnt);
    }

    public void Show(Vector3 worldPos, float damage)
    {
        if (pool == null || popupCanvasRoot == null)
            return;

        // ¾à°£ ·£´ýÀ¸·Î °ãÄ§ ¿ÏÈ­
        float rx = Random.Range(-randomX, randomX);
        Vector3 spawnPos = worldPos + worldOffset + new Vector3(rx, 0f, 0f);

        GameObject obj = pool.Spawn(poolKey, spawnPos, popupCanvasRoot);
        if (obj == null) return;

        var popup = obj.GetComponent<DamagePopup>();
        if (popup == null) return;

        popup.Setup(Mathf.RoundToInt(damage));
    }

    public void Despawn(GameObject obj)
    {
        if(pool == null || obj == null) 
            return;

        pool.Despawn(poolKey, obj);
    }
}
