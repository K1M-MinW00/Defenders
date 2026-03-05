using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitHUDController : MonoBehaviour
{
    [Header("Binding")]
    [SerializeField] private UnitInstance unit;

    [Header("UI")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private TextMeshProUGUI starText;

    private void Awake()
    {
        if (unit == null)
            unit = GetComponentInParent<UnitInstance>();
    }

    private void OnEnable()
    {
        if (unit == null)
            return;

        unit.OnStarChanged += HandleStarChanged;
        unit.OnHpChanged += HandleHpChanged;
        unit.OnEnergyChanged += HandleEnergyChanged;
    }

    private void OnDisable()
    {
        if (unit == null)
            return;

        unit.OnStarChanged -= HandleStarChanged;
        unit.OnHpChanged -= HandleHpChanged;
        unit.OnEnergyChanged -= HandleEnergyChanged;
    }

    private void HandleStarChanged(UnitInstance instance) => RefreshAll();

    private void HandleHpChanged(UnitInstance instance)
    {
        RefreshHp();
    }

    private void HandleEnergyChanged(UnitInstance instance)
    {
        RefreshEnergy();
    }


    private void RefreshHp()
    {
        float maxHp = Mathf.Max(1f, unit.Stats.maxHp);
        hpSlider.minValue = 0f;
        hpSlider.maxValue = maxHp;
        hpSlider.value = Mathf.Clamp(unit.Hp,0f,maxHp);
    }
    private void RefreshEnergy()
    {
        float maxE = Mathf.Max(1f, unit.Stats.maxMp);
        energySlider.minValue = 0f;
        energySlider.maxValue = maxE;
        energySlider.value = Mathf.Clamp(unit.Mp, 0f, maxE);
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
