using UnityEngine;

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public sealed class GameSceneCompositionRoot : MonoBehaviour
{
    [SerializeField] private StageSessionController stageSessionController;

    private void Awake()
    {
        if (stageSessionController == null)
        {
            Debug.LogError($"[{nameof(GameSceneCompositionRoot)}] Stage session controller is not assigned.", this);
            return;
        }

        stageSessionController.Initialize(
            UserDataManager.Instance,
            SceneFlowService.Shared,
            GameSettingsManager.Instance);
    }
}
