using System;
using System.Collections;
using UnityEngine;

public class StagePrepareTimerController : MonoBehaviour
{
    [SerializeField] private float prepareDuration = 10f;

    private Coroutine prepareRoutine;
    private Action onPrepareFinished;
    private float timer;

    public float PrepareDuration => prepareDuration;
    public float CurrentTimer => timer;
    public bool IsPreparing => prepareRoutine != null;

    public event Action<float> OnPrepareTimerChanged;

    public void StartPreparePhase(Action finishedCallback)
    {
        StopPreparePhase();

        onPrepareFinished = finishedCallback;
        timer = prepareDuration;

        OnPrepareTimerChanged?.Invoke(timer);
        prepareRoutine = StartCoroutine(CoPrepare());
    }

    public void ForceFinishPrepare()
    {
        if (prepareRoutine == null)
            return;

        Action callback = onPrepareFinished;
        StopPreparePhase();
        callback?.Invoke();
    }

    public void StopPreparePhase()
    {
        if (prepareRoutine != null)
        {
            StopCoroutine(prepareRoutine);
            prepareRoutine = null;
        }

        onPrepareFinished = null;
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
        callback?.Invoke();
    }
}
