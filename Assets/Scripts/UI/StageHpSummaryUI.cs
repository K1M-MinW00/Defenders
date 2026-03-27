using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageHpSummaryUI : MonoBehaviour
{
    [Header("Unit HP")]
    [SerializeField] private Slider unitSlider;
    [SerializeField] private TextMeshProUGUI unitHpText;

    [Header("Enemy HP")]
    [SerializeField] private Slider enemySlider;
    [SerializeField] private TextMeshProUGUI enemyHpText;

    private MonsterWaveHpTracker monsterHpTracker;
    private UnitRosterHpTracker unitHpTracker;

    public void Initialize(MonsterWaveHpTracker monsterHpTracker, UnitRosterHpTracker unitHpTracker)
    {
        this.monsterHpTracker = monsterHpTracker;
        this.unitHpTracker = unitHpTracker;

        Bind();
        Refresh();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void Bind()
    {
        if (unitHpTracker != null)
            unitHpTracker.OnTotalHpChanged += HandleUnitSlider;

        if (monsterHpTracker != null)
            monsterHpTracker.OnWaveHpChanged += HandleEnemySlider;
    }

    private void Unbind()
    {
        if (unitHpTracker != null)
            unitHpTracker.OnTotalHpChanged -= HandleUnitSlider;

        if (monsterHpTracker != null)
            monsterHpTracker.OnWaveHpChanged -= HandleEnemySlider;
    }

    private void Refresh()
    {
        if (unitHpTracker != null)
            HandleUnitSlider(unitHpTracker.CurrentHp, unitHpTracker.MaxHp);

        if (monsterHpTracker != null)
            HandleEnemySlider(monsterHpTracker.CurrentHp, monsterHpTracker.MaxHp);
    }

    private void HandleUnitSlider(float currentHp, float maxHp)
    {
        if (unitSlider != null)
            unitSlider.value = unitHpTracker != null ? unitHpTracker.GetHpRatio() : 0f;

        if (unitHpText != null)
            unitHpText.text = $"{currentHp:0}";
    }

    private void HandleEnemySlider(float currentHp, float maxHp)
    {
        if (enemySlider != null)
            enemySlider.value = monsterHpTracker != null ? monsterHpTracker.GetHpRatio(): 0f;

        if (enemyHpText != null)
            enemyHpText.text = $"{currentHp:0}";
    }
}