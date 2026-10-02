using UnityEngine;

public class StagePhaseUIView : MonoBehaviour
{
    [SerializeField] private GameObject commonHUD;
    [SerializeField] private GameObject prepareHUD;
    [SerializeField] private GameObject combatHUD;
    private bool isTopHudHidden;

    public void SetPhase(StageState state)
    {
        bool isPreparing = state == StageState.Preparing;
        bool isCombat = state == StageState.Combat;
        bool isResult = state == StageState.StageClear || state == StageState.StageFail;

        if (commonHUD != null)
            commonHUD.SetActive(!isResult && !isTopHudHidden);

        if (prepareHUD != null)
            prepareHUD.SetActive(isPreparing && !isResult);

        if (combatHUD != null)
            combatHUD.SetActive(isCombat && !isResult);
    }

    public void SetTopHudHidden(bool hidden, StageState currentState)
    {
        isTopHudHidden = hidden;
        SetPhase(currentState);
    }
}
