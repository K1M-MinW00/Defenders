using System;
using System.Collections;
using UnityEngine;

public class StageFlowController : MonoBehaviour
{
    [SerializeField] private float prepareDuration = 10f;
    [SerializeField] private StageUIController stageUI;

    private Coroutine prepareRoutine;
    private Action onPrepareFinished;
    private float timer;

    public void StartPreparePhase(Action finishedCallback)
    {
        StopPreparePhase();

        onPrepareFinished = finishedCallback;
        timer = prepareDuration;
        prepareRoutine = StartCoroutine(CoPrepare());
    }

    public void ForceFinishPrepare()
    {
        StopPreparePhase();
        onPrepareFinished?.Invoke();
    }

    public void StopPreparePhase()
    {
        if (prepareRoutine != null)
        {
            StopCoroutine(prepareRoutine);
            prepareRoutine = null;
        }
    }

    private IEnumerator CoPrepare()
    {
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            stageUI?.UpdatePrepTimer(timer);
            yield return null;
        }

        prepareRoutine = null;
        onPrepareFinished?.Invoke();
    }
}