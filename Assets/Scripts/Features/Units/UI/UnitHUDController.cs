using UnityEngine;
using UnityEngine.UI;

public class UnitHUDController : MonoBehaviour
{
    [Header("Binding")]
    [SerializeField] private UnitController unit;

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private UnitStarIconView starIcon;

    private void Awake()
    {
        if (unit == null)
            unit = GetComponent<UnitController>();
        if (starIcon == null)
            starIcon = GetComponentInChildren<UnitStarIconView>(true);
    }

    private void OnEnable()
    {
        if (unit == null)
            return;

        unit.OnInitialized += HandleInitialized;
        unit.OnStatsChanged += HandleStarChanged;
        unit.Health.OnDead += HandleDead;
        unit.Health.OnHpChanged += HandleHpChanged;
        unit.Energy.OnEnergyChanged += HandleEnergyChanged;

        if (unit.Runtime != null)
            RefreshAll();
    }

    private void OnDisable()
    {
        if (unit == null)
            return;

        unit.OnInitialized -= HandleInitialized;
        unit.OnStatsChanged -= HandleStarChanged;
        unit.Health.OnDead -= HandleDead;
        unit.Health.OnHpChanged -= HandleHpChanged;
        unit.Energy.OnEnergyChanged -= HandleEnergyChanged;
    }

    private void HandleInitialized(UnitController instance) => RefreshAll();

    private void HandleStarChanged(UnitController instance) => RefreshAll();

    private void HandleHpChanged(UnitController instance, float curHp, float maxHp)
    {
        if (!instance.IsDead)
            SetHudVisible(true);

        RefreshHp();
    }

    private void HandleDead(UnitController instance)
    {
        SetHudVisible(false);
    }

    private void HandleEnergyChanged(float current, float max)
    {
        RefreshEnergy();
    }

    private void SetHudVisible(bool visible)
    {
        if(root !=  null)
            root.SetActive(visible);
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
        starIcon?.SetStar(unit.Star);
    }

    private void RefreshAll()
    {
        SetHudVisible(!unit.IsDead);

        RefreshStar();
        RefreshHp();
        RefreshEnergy();
    }
}
