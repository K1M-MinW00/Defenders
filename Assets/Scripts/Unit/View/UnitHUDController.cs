using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitHUDController : MonoBehaviour
{
    [Header("Binding")]
    [SerializeField] private UnitController unit;

    [Header("UI")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private TextMeshProUGUI starText;

    private void Awake()
    {
        if (unit == null)
            unit = GetComponentInParent<UnitController>();
    }

    private void OnEnable()
    {
        if (unit == null)
            return;

        unit.OnStatsChanged += HandleStarChanged;
        unit.Health.OnHpChanged += HandleHpChanged;
        unit.Energy.OnEnergyChanged += HandleEnergyChanged;
    }

    private void OnDisable()
    {
        if (unit == null)
            return;

        unit.OnStatsChanged -= HandleStarChanged;
        unit.Health.OnHpChanged -= HandleHpChanged;
        unit.Energy.OnEnergyChanged -= HandleEnergyChanged;
    }

    private void HandleStarChanged(UnitController instance) => RefreshAll();

    private void HandleHpChanged(UnitController instance, float curHp, float maxHp)
    {
        RefreshHp();
    }

    private void HandleEnergyChanged(float current, float max)
    {
        RefreshEnergy();
    }


    private void RefreshHp()
    {
        float maxHp = Mathf.Max(1f, unit.Health.MaxHp);
        hpSlider.minValue = 0f;
        hpSlider.maxValue = maxHp;
        hpSlider.value = Mathf.Clamp(unit.Health.CurrentHp,0f,maxHp);
    }
    private void RefreshEnergy()
    {
        float maxE = 100f;
        energySlider.minValue = 0f;
        energySlider.maxValue = maxE;
        energySlider.value = Mathf.Clamp(unit.Energy.CurrentEnergy, 0f, maxE);
    }

    private void RefreshStar()
    {
        starText.text = unit.Star.ToString();
    }

    private void RefreshAll()
    {
        RefreshStar();
        RefreshHp();
        RefreshEnergy();
    }
}
