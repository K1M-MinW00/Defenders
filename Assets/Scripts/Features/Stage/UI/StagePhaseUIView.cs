using UnityEngine;

public class StagePhaseUIView : MonoBehaviour
{
    [SerializeField] private GameObject commonHUD;
    [SerializeField] private GameObject prepareHUD;
    [SerializeField] private GameObject combatHUD;

    public void SetPhase(StageState state)
    {
        bool isPreparing = state == StageState.Preparing;
        bool isCombat = state == StageState.Combat;
        bool isResult = state == StageState.StageClear || state == StageState.StageFail;

        if (commonHUD != null)
            commonHUD.SetActive(!isResult);

        if (prepareHUD != null)
            prepareHUD.SetActive(isPreparing && !isResult);

        if (combatHUD != null)
            combatHUD.SetActive(isCombat && !isResult);
    }
}
