using TMPro;
using UnityEngine;

public sealed class StageCombatStatisticsUI : MonoBehaviour
{
    [SerializeField] private StageCombatStatisticsRowUI[] rows;
    [SerializeField] private UnityEngine.UI.Button damageButton;
    [SerializeField] private UnityEngine.UI.Button healingButton;
    [SerializeField] private UnityEngine.UI.Button takenButton;
    [SerializeField] private TMP_Text damageLabel;
    [SerializeField] private TMP_Text healingLabel;
    [SerializeField] private TMP_Text takenLabel;
    [SerializeField] private Color selectedColor = new(1f, 0.82f, 0.18f, 1f);
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedButtonColor = new(0.42f, 0.3f, 0.08f, 1f);
    [SerializeField] private Color normalButtonColor = new(0.22f, 0.18f, 0.12f, 1f);

    private readonly CombatStatisticsTracker tracker = new();
    private StageSessionController session;
    private CombatStatisticType selectedType = CombatStatisticType.DamageDealt;

    public void Initialize(StageSessionController stageSession, UnitRoster roster)
    {
        Dispose();
        session = stageSession;
        tracker.Initialize(roster);
        tracker.Changed += Refresh;
        if (session != null) session.PhaseChanged += HandlePhaseChanged;
        damageButton?.onClick.AddListener(SelectDamage);
        healingButton?.onClick.AddListener(SelectHealing);
        takenButton?.onClick.AddListener(SelectTaken);
        bool isCombat = session?.CurrentState == StageState.Combat;
        gameObject.SetActive(isCombat);
        if (isCombat)
            tracker.BeginCombat();
    }

    public void Dispose()
    {
        if (session != null) session.PhaseChanged -= HandlePhaseChanged;
        damageButton?.onClick.RemoveListener(SelectDamage);
        healingButton?.onClick.RemoveListener(SelectHealing);
        takenButton?.onClick.RemoveListener(SelectTaken);
        tracker.Changed -= Refresh;
        tracker.Dispose();
        session = null;
    }

    private void OnDestroy() => Dispose();
    private void SelectDamage() => Select(CombatStatisticType.DamageDealt);
    private void SelectHealing() => Select(CombatStatisticType.HealingDone);
    private void SelectTaken() => Select(CombatStatisticType.DamageTaken);

    private void Select(CombatStatisticType type)
    {
        selectedType = type;
        Refresh();
    }

    private void HandlePhaseChanged(StageState previous, StageState current)
    {
        if (current == StageState.Combat)
        {
            tracker.BeginCombat();
            gameObject.SetActive(true);
            Refresh();
        }
        else if (previous == StageState.Combat)
        {
            tracker.EndCombat();
            gameObject.SetActive(false);
        }
    }

    private void Refresh()
    {
        var top = tracker.GetTop(selectedType, rows?.Length ?? 0);
        float maximumDamage = tracker.GetMaximum(CombatStatisticType.DamageDealt);
        float maximumHealing = tracker.GetMaximum(CombatStatisticType.HealingDone);
        float maximumTaken = tracker.GetMaximum(CombatStatisticType.DamageTaken);
        for (int i = 0; rows != null && i < rows.Length; i++)
        {
            if (i < top.Count)
                rows[i]?.SetData(i + 1, top[i], maximumDamage, maximumHealing, maximumTaken);
            else rows[i]?.SetEmpty(i + 1);
        }
        if (damageLabel != null) damageLabel.color = selectedType == CombatStatisticType.DamageDealt ? selectedColor : normalColor;
        if (healingLabel != null) healingLabel.color = selectedType == CombatStatisticType.HealingDone ? selectedColor : normalColor;
        if (takenLabel != null) takenLabel.color = selectedType == CombatStatisticType.DamageTaken ? selectedColor : normalColor;
        SetButtonColor(damageButton, selectedType == CombatStatisticType.DamageDealt);
        SetButtonColor(healingButton, selectedType == CombatStatisticType.HealingDone);
        SetButtonColor(takenButton, selectedType == CombatStatisticType.DamageTaken);
    }

    private void SetButtonColor(UnityEngine.UI.Button button, bool selected)
    {
        if (button?.targetGraphic is UnityEngine.UI.Image image)
            image.color = selected ? selectedButtonColor : normalButtonColor;
    }
}
