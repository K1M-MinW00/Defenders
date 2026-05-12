using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyBattleStartController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    public void OnClickStartBattle()
    {
        UserDataRoot userData = UserDataManager.Instance.Data;

        if (userData == null)
        {
            Debug.LogError("Start battle failed. UserData is null.");
            return;
        }

        int sector = userData.Progress.CurrentSector;
        int stage = userData.Progress.CurrentStage;

        List<string> selectedUnitIds = userData.Roster.SelectedUnitIds;

        if (selectedUnitIds == null || selectedUnitIds.Count == 0)
        {
            Debug.LogError("Start battle failed. SelectedUnitIds is empty.");
            return;
        }

        StageEnterData enterData = new StageEnterData(sector,stage,selectedUnitIds);

        StageEnterHolder.Set(enterData);

        SceneManager.LoadScene(gameSceneName);
    }
}