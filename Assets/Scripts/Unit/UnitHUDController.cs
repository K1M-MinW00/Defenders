using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitHUDController : MonoBehaviour
{
    [Header("Binding")]
    [SerializeField] private UnitRuntime unit;

    [Header("UI")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private TextMeshProUGUI starText;

    private void Awake()
    {
        if (unit == null)
            unit = GetComponentInParent<UnitRuntime>();
    }

    private void OnEnable()
    {
        if (unit == null)
            return;

        unit.OnStatsChanged += HandleStarChanged;
        unit.OnHpChanged += HandleHpChanged;
        unit.OnMpChanged += HandleEnergyChanged;
    }

    private void OnDisable()
    {
        if (unit == null)
            return;

        unit.OnStatsChanged -= HandleStarChanged;
        unit.OnHpChanged -= HandleHpChanged;
        unit.OnMpChanged -= HandleEnergyChanged;
    }

    private void HandleStarChanged(UnitRuntime instance) => RefreshAll();

    private void HandleHpChanged(UnitRuntime instance, float curHp, float maxHp)
    {
        RefreshHp();
    }

    private void HandleEnergyChanged(UnitRuntime instance)
    {
        RefreshEnergy();
    }


    private void RefreshHp()
    {
        float maxHp = Mathf.Max(1f, unit.FinalStats.maxHp);
        hpSlider.minValue = 0f;
        hpSlider.maxValue = maxHp;
        hpSlider.value = Mathf.Clamp(unit.CurrentHp,0f,maxHp);
    }
    private void RefreshEnergy()
    {
        float maxE = Mathf.Max(1f, unit.FinalStats.maxMp);
        energySlider.minValue = 0f;
        energySlider.maxValue = maxE;
        energySlider.value = Mathf.Clamp(unit.CurrentMp, 0f, maxE);
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
