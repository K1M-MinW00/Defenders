using System;
using System.Collections;
using UnityEngine;

public sealed class StagePrepareTimerController : MonoBehaviour
{
    private float prepareDuration = 90f;

    private Coroutine prepareRoutine;
    private Action onPrepareFinished;
    private float timer;
    private bool isPreparing;

    public float PrepareDuration => prepareDuration;
    public float CurrentTimer => timer;
    public bool IsPreparing => isPreparing;

    public event Action<float> OnPrepareTimerChanged;

    public void Configure(float duration)
    {
        if (isPreparing)
            StopPreparePhase();

        prepareDuration = Mathf.Max(0f, duration);
        timer = prepareDuration;
        OnPrepareTimerChanged?.Invoke(timer);
    }

    public bool TryStartPreparePhase(Action finishedCallback)
    {
        StopPreparePhase();

        if (!isActiveAndEnabled || finishedCallback == null)
            return false;

        onPrepareFinished = finishedCallback;
        timer = Mathf.Max(0f, prepareDuration);
        isPreparing = true;

        OnPrepareTimerChanged?.Invoke(timer);
        prepareRoutine = StartCoroutine(CoPrepare());
        return true;
    }

    public bool ForceFinishPrepare()
    {
        if (!isPreparing)
            return false;

        Action callback = onPrepareFinished;
        StopPreparePhase();
        timer = 0f;
        OnPrepareTimerChanged?.Invoke(timer);
        callback?.Invoke();
        return true;
    }

    public void StopPreparePhase()
    {
        if (prepareRoutine != null)
        {
            StopCoroutine(prepareRoutine);
            prepareRoutine = null;
        }

        onPrepareFinished = null;
        isPreparing = false;
    }

    private IEnumerator CoPrepare()
    {
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            
            if (timer < 0f)
                timer = 0f;

            OnPrepareTimerChanged?.Invoke(timer);

            yield return null;
        }

        prepareRoutine = null;
        Action callback = onPrepareFinished;
        onPrepareFinished = null;
        isPreparing = false;
        callback?.Invoke();
    }

    private void OnDisable()
    {
        StopPreparePhase();
    }

}
