using System;
using System.Collections;
using UnityEngine;

public sealed class BossBattleTimerController : MonoBehaviour
{
    private MonsterSpawner monsterSpawner;
    private Coroutine timerRoutine;
    private BossController primaryBoss;
    private Action onExpired;
    private bool enrageTriggered;
    private bool warningTriggered;

    public float RemainingTime { get; private set; }
    public bool IsRunning => timerRoutine != null;
    public BossController PrimaryBoss => primaryBoss;

    public event Action<float> TimerChanged;
    public event Action WarningStarted;
    public event Action<BossController> PrimaryBossChanged;
    public event Action TimerStopped;

    public void Initialize(MonsterSpawner spawner)
    {
        StopTimer();
        monsterSpawner = spawner;
    }

    public bool TryStart(WaveData wave, Action expired)
    {
        StopTimer();
        if (wave == null || wave.waveType != WaveType.Boss || expired == null || monsterSpawner == null)
            return false;

        BossWaveSettings settings = wave.bossSettings ?? new BossWaveSettings();
        RemainingTime = Mathf.Max(1f, settings.timeLimit);
        onExpired = expired;
        enrageTriggered = false;
        warningTriggered = false;
        primaryBoss = null;
        monsterSpawner.OnMonsterSpawned += HandleMonsterSpawned;
        TimerChanged?.Invoke(RemainingTime);
        timerRoutine = StartCoroutine(RunTimer(settings));
        return true;
    }

    public void StopTimer()
    {
        bool wasActive = timerRoutine != null || primaryBoss != null;
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        if (monsterSpawner != null)
            monsterSpawner.OnMonsterSpawned -= HandleMonsterSpawned;

        onExpired = null;
        primaryBoss = null;
        enrageTriggered = false;
        warningTriggered = false;

        if (wasActive)
            TimerStopped?.Invoke();
    }

    private IEnumerator RunTimer(BossWaveSettings settings)
    {
        float enrageAt = Mathf.Clamp(settings.enrageLeadTime, 0f, RemainingTime);
        float warningAt = Mathf.Clamp(settings.warningLeadTime, 0f, RemainingTime);

        while (RemainingTime > 0f)
        {
            RemainingTime = Mathf.Max(0f, RemainingTime - Time.deltaTime);
            TryTriggerMilestones(enrageAt, warningAt);
            TimerChanged?.Invoke(RemainingTime);
            yield return null;
        }

        yield return new WaitForEndOfFrame();
        timerRoutine = null;
        monsterSpawner.OnMonsterSpawned -= HandleMonsterSpawned;
        Action expired = onExpired;
        onExpired = null;
        expired?.Invoke();
    }

    private void TryTriggerMilestones(float enrageAt, float warningAt)
    {
        if (!enrageTriggered && RemainingTime <= enrageAt)
        {
            enrageTriggered = true;
            primaryBoss?.ActivateEnrage();
        }

        if (!warningTriggered && RemainingTime <= warningAt)
        {
            warningTriggered = true;
            WarningStarted?.Invoke();
        }
    }

    private void HandleMonsterSpawned(MonsterController monster)
    {
        if (primaryBoss != null || monster == null || !monster.TryGetComponent(out BossController boss))
            return;

        primaryBoss = boss;
        PrimaryBossChanged?.Invoke(primaryBoss);

        if (enrageTriggered)
            primaryBoss.ActivateEnrage();
    }

    private void OnDisable()
    {
        StopTimer();
    }
}
