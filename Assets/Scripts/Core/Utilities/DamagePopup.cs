using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour, IPoolable
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float lifeTime = 0.8f;

    private Poolable poolable;
    private float timer;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();

        if(poolable == null)
            poolable = gameObject.AddComponent<Poolable>();

        damageText = GetComponent<TextMeshProUGUI>();
    }

    public void Setup(int damage)
    {
        if (damageText != null)
            damageText.SetText("{0}", damage);
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if(timer <= 0f)
            poolable.ReturnToPool();
    }

    public void OnDespawn()
    {
        if (damageText != null)
            damageText.text = string.Empty;
    }

    public void OnSpawn()
    {
        timer = lifeTime;
    }
}
