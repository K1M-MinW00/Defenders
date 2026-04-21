using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyUnitTabUI : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private Transform battleSquadRoot;
    [SerializeField] private Transform waitingListRoot;

    [Header("Prefab")]
    [SerializeField] private UnitCardUI unitCardPrefab;

    private List<LobbyUnitViewModel> battleSquadViewModels = new();
    private List<LobbyUnitViewModel> waitingListViewModels = new();

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        Debug.Log("Refresh");
        UserDataRoot userData = UserDataManager.Instance.CurrentData;

        if (userData == null)
        {
            Debug.LogError("[LobbyUnitTabUI] UserData is Null");
            return;
        }

        UserRosterData roster = userData.Roster;

        if (roster == null)
        {
            Debug.LogError("[LobbyUnitTabUI] Roster is Null");
            return;
        }

        battleSquadViewModels.Clear();
        waitingListViewModels.Clear();

        HashSet<int> battleSet = new HashSet<int>(roster.BattleSquadUnitCodes);

        foreach (int unitCodeValue in roster.BattleSquadUnitCodes)
        {
            UnitCode unitCode = (UnitCode)unitCodeValue;
            UserUnitData userUnit = roster.OwnedUnits.FirstOrDefault(x => x.UnitCodeValue == unitCodeValue);

            if (userUnit == null)
            {
                Debug.LogError($"[LobbyUnitTabUI] UserUnitData not found for BattleSquad unit : {userUnit.UnitCode}");
                continue;
            }

            LobbyUnitViewModel vm = CreateViewModel(userUnit);

            if (vm != null)
                battleSquadViewModels.Add(vm);
        }

        foreach (UserUnitData userUnit in roster.OwnedUnits)
        {
            if (battleSet.Contains(userUnit.UnitCodeValue))
                continue;

            LobbyUnitViewModel vm = CreateViewModel(userUnit);
            if (vm != null)
                waitingListViewModels.Add(vm);

        }
        RebuildCardList(battleSquadRoot, battleSquadViewModels);
        RebuildCardList(waitingListRoot, waitingListViewModels);
    }
    private LobbyUnitViewModel CreateViewModel(UserUnitData userUnit)
    {
        UnitDataSO unitData = UnitMasterDataManager.Instance.GetUnitData(userUnit.UnitCode);

        if (unitData == null)
        {
            Debug.LogWarning($"[LobbyUnitTabUI] UnitDataSO not found for UnitCode : {userUnit.UnitCode}");
            return null;
        }

        return new LobbyUnitViewModel
        {
            UnitCode = userUnit.UnitCode,
            Icon = unitData.icon,
            //Level = userUnit.Level,
            //Promotion = userUnit.Promotion
        };
    }

    private void RebuildCardList(Transform root, List<LobbyUnitViewModel> viewModels)
    {
        if (root == null || unitCardPrefab == null)
            return;

        ClearChildren(root);

        foreach (LobbyUnitViewModel vm in viewModels)
        {
            UnitCardUI card = Instantiate(unitCardPrefab, root);
            card.Bind(vm);
        }
    }

    private void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }
}