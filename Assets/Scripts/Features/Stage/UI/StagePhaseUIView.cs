using UnityEngine;

public class StagePhaseUIView : MonoBehaviour
{
    [SerializeField] private GameObject commonHUD;
    [SerializeField] private GameObject prepareHUD;
    [SerializeField] private GameObject combatHUD;
    private bool isTopHudHidden;
    private bool isRewardPresentationVisible;
    private StageState currentState;

    public void SetPhase(StageState state)
    {
        currentState = state;
        bool isPreparing = state == StageState.Preparing;
        bool isCombat = state == StageState.Combat;
        bool isResult = state == StageState.StageClear || state == StageState.StageFail;

        if (commonHUD != null)
            commonHUD.SetActive(!isResult && !isTopHudHidden);

        if (prepareHUD != null)
            prepareHUD.SetActive(
                !isResult && (isPreparing ||
                              (state == StageState.Resolving && isRewardPresentationVisible)));

        if (combatHUD != null)
            combatHUD.SetActive(isCombat && !isResult);
    }

    public void SetTopHudHidden(bool hidden, StageState currentState)
    {
        isTopHudHidden = hidden;
        SetPhase(currentState);
    }

    public void SetRewardPresentationVisible(bool visible)
    {
        isRewardPresentationVisible = visible;
        SetPhase(currentState);
    }
}
