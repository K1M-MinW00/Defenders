using DG.Tweening;
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

    [Header("Skill Ready Feedback")]
    [SerializeField] private RectTransform energyPulseTarget;
    [SerializeField] private Image energyFillImage;
    [SerializeField] private Color energyReadyColor = new(1f, 0.9f, 0.15f, 1f);
    [SerializeField, Min(0.05f)] private float energyPulseDuration = 0.28f;
    [SerializeField, Range(0.05f, 0.35f)] private float energyPulseStrength = 0.16f;

    [Header("World Sorting")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private int hudSortingOrder = 5000;

    private bool interactionHidden;
    private bool transitionHidden;
    private Vector3 energyPulseBaseScale = Vector3.one;
    private Color energyNormalColor = Color.white;
    private Tween energyPulseTween;

    private void Awake()
    {
        if (unit == null)
            unit = GetComponent<UnitController>();
        if (starIcon == null)
            starIcon = GetComponentInChildren<UnitStarIconView>(true);
        if (hudCanvas == null)
            hudCanvas = GetComponentInChildren<Canvas>(true);
        if (energyPulseTarget == null && energySlider != null)
            energyPulseTarget = energySlider.transform as RectTransform;
        if (energyFillImage == null && energySlider != null && energySlider.fillRect != null)
            energyFillImage = energySlider.fillRect.GetComponent<Image>();
        if (energyPulseTarget != null)
            energyPulseBaseScale = energyPulseTarget.localScale;
        if (energyFillImage != null)
            energyNormalColor = energyFillImage.color;

        ApplyHudSorting();
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
        unit.Energy.OnEnergyFull += HandleEnergyFull;

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
        unit.Energy.OnEnergyFull -= HandleEnergyFull;

        TweenLifecycle.Kill(ref energyPulseTween);
        if (energyPulseTarget != null)
            energyPulseTarget.localScale = energyPulseBaseScale;
        if (energyFillImage != null)
            energyFillImage.color = energyNormalColor;

        interactionHidden = false;
        transitionHidden = false;
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
            root.SetActive(visible && !interactionHidden && !transitionHidden);
    }

    public void SetInteractionHidden(bool hidden)
    {
        interactionHidden = hidden;
        RefreshVisibility();
    }

    public void SetTransitionHidden(bool hidden)
    {
        transitionHidden = hidden;
        RefreshVisibility();
    }

    private void RefreshVisibility()
    {
        SetHudVisible(unit != null && unit.Runtime != null && !unit.IsDead);
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
        float maxE = Mathf.Max(1f, unit.Energy.MaxEnergy);
        energySlider.minValue = 0f;
        energySlider.maxValue = maxE;
        energySlider.value = Mathf.Clamp(unit.Energy.CurrentEnergy, 0f, maxE);
        if (energyFillImage != null)
            energyFillImage.color = unit.Energy.CurrentEnergy >= maxE
                ? energyReadyColor
                : energyNormalColor;
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

    private void HandleEnergyFull()
    {
        if (unit == null || !unit.IsCombatPhase || energyPulseTarget == null)
            return;

        TweenLifecycle.Kill(ref energyPulseTween);
        energyPulseTarget.localScale = energyPulseBaseScale;
        energyPulseTween = energyPulseTarget
            .DOPunchScale(Vector3.one * energyPulseStrength, energyPulseDuration, 4, 0.5f)
            .BindTo(this);
    }

    private void ApplyHudSorting()
    {
        if (hudCanvas == null)
            return;

        // Unit bodies use world-Y sorting around 1000. A separate overridden
        // canvas keeps every unit HUD above every unit body regardless of Y.
        hudCanvas.overrideSorting = true;
        hudCanvas.sortingOrder = hudSortingOrder;
    }
}
