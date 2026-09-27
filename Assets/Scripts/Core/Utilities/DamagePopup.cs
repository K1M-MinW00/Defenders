using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour, IPoolable
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float lifeTime = 0.8f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = new(1f, 0.75f, 0f, 1f);
    [SerializeField] private float criticalFontSizeMultiplier = 1.35f;

    private Poolable poolable;
    private Animation popupAnimation;
    private CanvasGroup canvasGroup;
    private float normalFontSize;
    private float timer;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();

        if(poolable == null)
            poolable = gameObject.AddComponent<Poolable>();

        damageText = GetComponent<TextMeshProUGUI>();
        popupAnimation = GetComponent<Animation>();
        canvasGroup = GetComponent<CanvasGroup>();
        normalFontSize = damageText != null ? damageText.fontSize : 1f;
    }

    public void Setup(int damage, bool isCritical)
    {
        if (damageText == null)
            return;

        damageText.SetText("{0}", damage);
        damageText.color = isCritical ? criticalColor : normalColor;
        damageText.fontSize = normalFontSize * (isCritical ? criticalFontSizeMultiplier : 1f);
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

        if (damageText != null)
            damageText.fontSize = normalFontSize;
    }

    public void OnSpawn()
    {
        timer = lifeTime;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (popupAnimation != null)
        {
            popupAnimation.Stop();
            popupAnimation.Play();
        }
    }
}
