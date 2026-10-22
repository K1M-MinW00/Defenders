using TMPro;
using UnityEngine;

public sealed class BossHudView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private UnityEngine.UI.Slider hpSlider;
    [SerializeField] private UnityEngine.UI.Slider shieldSlider;
    [SerializeField] private GameObject enrageIcon;

    private BossBattleTimerController battleTimer;
    private BossController boss;

    public void Initialize(BossBattleTimerController timer)
    {
        Dispose();
        battleTimer = timer;
        if (battleTimer != null)
        {
            battleTimer.PrimaryBossChanged += BindBoss;
            battleTimer.TimerStopped += UnbindBoss;
        }

        SetVisible(false);
    }

    public void Dispose()
    {
        if (battleTimer != null)
        {
            battleTimer.PrimaryBossChanged -= BindBoss;
            battleTimer.TimerStopped -= UnbindBoss;
        }

        battleTimer = null;
        UnbindBoss();
    }

    private void BindBoss(BossController nextBoss)
    {
        UnbindBoss();
        boss = nextBoss;
        if (boss == null)
            return;

        MonsterHealth health = boss.GetComponent<MonsterHealth>();
        health.OnHpChanged += HandleHpChanged;
        health.OnDead += HandleBossDead;
        boss.Enraged += HandleEnraged;
        if (boss.Shield != null)
        {
            boss.Shield.ShieldChanged += HandleShieldChanged;
            boss.Shield.ShieldBroken += HandleShieldBroken;
        }

        if (nameText != null)
        {
            MonsterController monster = boss.GetComponent<MonsterController>();
            nameText.text = monster?.Data != null
                ? monster.Data.displayName
                : boss.Definition != null ? boss.Definition.bossId : boss.name;
        }

        HandleHpChanged(health, 0f);
        HandleShieldChanged(
            boss.Shield != null ? boss.Shield.CurrentShield : 0f,
            boss.Shield != null ? boss.Shield.MaxShield : 0f);
        if (enrageIcon != null)
            enrageIcon.SetActive(boss.IsEnraged);
        SetVisible(true);
    }

    private void UnbindBoss()
    {
        if (boss != null)
        {
            MonsterHealth health = boss.GetComponent<MonsterHealth>();
            health.OnHpChanged -= HandleHpChanged;
            health.OnDead -= HandleBossDead;
            boss.Enraged -= HandleEnraged;
            if (boss.Shield != null)
            {
                boss.Shield.ShieldChanged -= HandleShieldChanged;
                boss.Shield.ShieldBroken -= HandleShieldBroken;
            }
        }

        boss = null;
        SetVisible(false);
    }

    private void HandleHpChanged(MonsterHealth health, float _)
    {
        if (hpSlider == null || health == null)
            return;

        hpSlider.minValue = 0f;
        hpSlider.maxValue = Mathf.Max(1f, health.MaxHp);
        hpSlider.value = health.CurrentHp;
    }

    private void HandleShieldChanged(float current, float maximum)
    {
        if (shieldSlider == null)
            return;

        shieldSlider.gameObject.SetActive(maximum > 0f && current > 0f);
        shieldSlider.minValue = 0f;
        shieldSlider.maxValue = Mathf.Max(1f, maximum);
        shieldSlider.value = current;
    }

    private void HandleShieldBroken()
    {
        if (shieldSlider != null)
            shieldSlider.gameObject.SetActive(false);
    }

    private void HandleEnraged()
    {
        if (enrageIcon != null)
            enrageIcon.SetActive(true);
    }

    private void HandleBossDead(MonsterHealth _)
    {
        UnbindBoss();
    }

    private void SetVisible(bool visible)
    {
        if (root != null)
            root.SetActive(visible);
    }

    private void OnDestroy()
    {
        Dispose();
    }
}
