using System.Runtime.Serialization;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour, IPoolable
{
    [SerializeField] private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void Setup(int damage)
    {
        text.text = damage.ToString();
    }

    public void OnDespawn()
    {
    }

    public void OnSpawn()
    {   
    }

    public void Despawn()
    {
        DamageUIService.Instance?.Despawn(gameObject);
    }
}
