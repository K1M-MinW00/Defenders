using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class InventoryTestGrantWindow : EditorWindow
{
    private static readonly RewardType[] SupportedTypes =
    {
        RewardType.Gold, RewardType.Gem, RewardType.Fuel, RewardType.ResearchMaterial,
        RewardType.Experience, RewardType.Item, RewardType.Equipment, RewardType.Unit,
    };

    private readonly List<ItemDataSO> selectableItems = new();
    private readonly List<UnitDataSO> selectableUnits = new();
    private RewardType rewardType = RewardType.Gold;
    private int selectedDefinitionIndex;
    private int amount = 100;
    private bool persistToFirebase;
    private bool isGranting;
    private string resultMessage;
    private MessageType resultMessageType = MessageType.Info;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Defenders/Test Reward Grant")]
    public static void Open() => GetWindow<InventoryTestGrantWindow>("Test Reward Grant");

    private void OnEnable()
    {
        minSize = new Vector2(420f, 590f);
        RefreshCatalogs();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.LabelField("테스트 보상 지급", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Play Mode에서 현재 로그인한 계정에 실제 보상 처리 규칙으로 재화, 아이템, 장비, 유닛을 지급합니다.",
            MessageType.Info);
        EditorGUILayout.Space(8f);
        DrawAccountStatus();
        EditorGUILayout.Space(10f);
        DrawQuickGrantSection();
        EditorGUILayout.Space(12f);
        DrawCustomGrantSection();
        EditorGUILayout.Space(12f);
        DrawPersistenceSection();
        EditorGUILayout.Space(12f);
        DrawGrantButton();

        if (!string.IsNullOrWhiteSpace(resultMessage))
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(resultMessage, resultMessageType);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawAccountStatus()
    {
        UserDataManager manager = UserDataManager.Instance;
        bool ready = EditorApplication.isPlaying && manager != null && manager.IsLoaded;
        UserDataRoot data = ready ? manager.UserData : null;

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("현재 계정", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("상태", ready ? "지급 가능" : "Play Mode 로그인 필요");
            if (data?.Profile != null)
            {
                EditorGUILayout.LabelField("닉네임", data.Profile.Nickname ?? string.Empty);
                EditorGUILayout.LabelField("UID", data.Profile.UserId ?? string.Empty);
                EditorGUILayout.LabelField("계정 성장", $"레벨 {data.Profile.Level:N0}   경험치 {data.Profile.Exp:N0}");
            }
            if (data?.Resource != null)
            {
                EditorGUILayout.LabelField("보유 재화",
                    $"골드 {data.Resource.Gold:N0}   젬 {data.Resource.Gem:N0}   연료 {data.Resource.Fuel:N0}/{data.Resource.MaxFuel:N0}");
                EditorGUILayout.LabelField("연구 재료", data.Resource.ResearchMaterial.ToString("N0"));
            }
        }
    }

    private void DrawQuickGrantSection()
    {
        EditorGUILayout.LabelField("빠른 지급", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawQuickButton("골드 +1,000", RewardType.Gold, 1000);
            DrawQuickButton("젬 +500", RewardType.Gem, 500);
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawQuickButton("연료 +10", RewardType.Fuel, 10);
            DrawQuickButton("연구 재료 +100", RewardType.ResearchMaterial, 100);
            DrawQuickButton("경험치 +100", RewardType.Experience, 100);
        }
    }

    private void DrawQuickButton(string label, RewardType type, int grantAmount)
    {
        using (new EditorGUI.DisabledScope(!CanGrant()))
        {
            if (GUILayout.Button(label, GUILayout.Height(30f)))
                Grant(new RewardData { Type = type, Amount = grantAmount });
        }
    }

    private void DrawCustomGrantSection()
    {
        EditorGUILayout.LabelField("직접 선택", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            string[] typeLabels = SupportedTypes.Select(GetTypeLabel).ToArray();
            int currentTypeIndex = Mathf.Max(0, System.Array.IndexOf(SupportedTypes, rewardType));
            RewardType nextType = SupportedTypes[EditorGUILayout.Popup("보상 종류", currentTypeIndex, typeLabels)];
            if (nextType != rewardType)
            {
                rewardType = nextType;
                selectedDefinitionIndex = 0;
                amount = GetDefaultAmount(rewardType);
            }

            string selectedId = DrawDefinitionSelector();
            amount = Mathf.Max(1, EditorGUILayout.IntField("수량", amount));
            DrawPreview(selectedId);
        }
    }

    private string DrawDefinitionSelector()
    {
        if (rewardType is RewardType.Item or RewardType.Equipment)
        {
            List<ItemDataSO> options = GetFilteredItems();
            string[] labels = options.Select(item => $"[{item.Rarity}] {item.ItemName} ({item.ItemId})").ToArray();
            selectedDefinitionIndex = DrawPopup("아이템", selectedDefinitionIndex, labels);
            return options.Count > 0 ? options[selectedDefinitionIndex].ItemId : string.Empty;
        }

        if (rewardType == RewardType.Unit)
        {
            string[] labels = selectableUnits.Select(unit => $"[{unit.rarity}] {unit.displayName} ({unit.unitId})").ToArray();
            selectedDefinitionIndex = DrawPopup("유닛", selectedDefinitionIndex, labels);
            return selectableUnits.Count > 0 ? selectableUnits[selectedDefinitionIndex].unitId : string.Empty;
        }

        return string.Empty;
    }

    private static int DrawPopup(string label, int index, string[] options)
    {
        if (options.Length == 0)
        {
            EditorGUILayout.HelpBox("선택 가능한 데이터가 없습니다.", MessageType.Warning);
            return 0;
        }
        return EditorGUILayout.Popup(label, Mathf.Clamp(index, 0, options.Length - 1), options);
    }

    private void DrawPreview(string selectedId)
    {
        RewardData reward = new() { Type = rewardType, Id = selectedId, Amount = amount };
        if (!RewardPresentationResolver.TryResolve(reward, GameConfig.Icons, out RewardPresentation presentation))
            return;

        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            Texture preview = presentation.Icon != null
                ? AssetPreview.GetAssetPreview(presentation.Icon) ?? AssetPreview.GetMiniThumbnail(presentation.Icon)
                : null;
            GUILayout.Label(preview, GUILayout.Width(64f), GUILayout.Height(64f));
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField(presentation.DisplayName, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("등급", presentation.Rarity.ToString());
                EditorGUILayout.LabelField("지급량", amount.ToString("N0"));
            }
        }
    }

    private void DrawPersistenceSection()
    {
        persistToFirebase = EditorGUILayout.ToggleLeft("Firebase에도 저장", persistToFirebase);
        EditorGUILayout.HelpBox(
            persistToFirebase
                ? "현재 로그인 계정의 서버 데이터가 변경됩니다. 테스트 계정에서만 사용하세요."
                : "현재 Play Mode 세션에만 반영되며 게임을 종료하면 사라집니다.",
            persistToFirebase ? MessageType.Warning : MessageType.None);
    }

    private void DrawGrantButton()
    {
        string selectedId = GetSelectedDefinitionId();
        bool selectionValid = !RequiresDefinition(rewardType) || !string.IsNullOrWhiteSpace(selectedId);
        using (new EditorGUI.DisabledScope(!CanGrant() || !selectionValid))
        {
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.35f, 0.8f, 0.45f);
            if (GUILayout.Button("현재 계정에 지급", GUILayout.Height(42f)))
                Grant(new RewardData { Type = rewardType, Id = selectedId, Amount = amount });
            GUI.backgroundColor = previous;
        }
    }

    private async void Grant(RewardData reward)
    {
        UserDataManager manager = UserDataManager.Instance;
        if (!CanGrant() || reward == null || manager == null)
            return;

        isGranting = true;
        resultMessage = "지급 처리 중...";
        resultMessageType = MessageType.Info;
        Repaint();

        RewardGrantResult result;
        try
        {
            result = await manager.GrantEditorTestRewardsAsync(new[] { reward }, persistToFirebase);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            result = RewardGrantResult.Fail(RewardGrantFailure.InvalidData);
        }

        if (this == null)
            return;

        isGranting = false;
        if (result.Succeeded)
        {
            resultMessage = result.DiscardedEquipmentCount > 0
                ? $"지급 완료. 장비 슬롯 부족으로 {result.DiscardedEquipmentCount}개가 제외되었습니다."
                : "지급이 완료되었습니다.";
            resultMessageType = result.DiscardedEquipmentCount > 0 ? MessageType.Warning : MessageType.Info;
        }
        else
        {
            resultMessage = $"지급 실패: {result.Failure}";
            resultMessageType = MessageType.Error;
        }
        Repaint();
    }

    private void RefreshCatalogs()
    {
        try
        {
            GameConfig.Initialize();
            selectableItems.Clear();
            selectableItems.AddRange(GameConfig.Items.GetAll().Where(item => item != null));
            selectableUnits.Clear();
            selectableUnits.AddRange(GameConfig.Units.GetAll().Where(unit => unit != null)
                .OrderBy(unit => unit.rarity).ThenBy(unit => unit.displayName));
        }
        catch (System.Exception exception)
        {
            resultMessage = $"게임 데이터 로드 실패: {exception.Message}";
            resultMessageType = MessageType.Error;
        }
    }

    private List<ItemDataSO> GetFilteredItems() => selectableItems
        .Where(item => rewardType == RewardType.Equipment
            ? item.Category == ItemCategory.Equipment
            : item.Category != ItemCategory.Equipment)
        .OrderBy(item => item.Rarity).ThenBy(item => item.ItemName).ToList();

    private string GetSelectedDefinitionId()
    {
        if (rewardType is RewardType.Item or RewardType.Equipment)
        {
            List<ItemDataSO> options = GetFilteredItems();
            return options.Count > 0 ? options[Mathf.Clamp(selectedDefinitionIndex, 0, options.Count - 1)].ItemId : string.Empty;
        }
        if (rewardType == RewardType.Unit)
            return selectableUnits.Count > 0
                ? selectableUnits[Mathf.Clamp(selectedDefinitionIndex, 0, selectableUnits.Count - 1)].unitId
                : string.Empty;
        return string.Empty;
    }

    private bool CanGrant() => !isGranting && EditorApplication.isPlaying &&
        UserDataManager.Instance != null && UserDataManager.Instance.IsLoaded;

    private static bool RequiresDefinition(RewardType type) =>
        type is RewardType.Item or RewardType.Equipment or RewardType.Unit;

    private static int GetDefaultAmount(RewardType type) => type switch
    {
        RewardType.Gold => 1000, RewardType.Gem => 500, RewardType.Fuel => 10,
        RewardType.Experience => 100, RewardType.ResearchMaterial => 100, _ => 1,
    };

    private static string GetTypeLabel(RewardType type) => type switch
    {
        RewardType.Gold => "골드", RewardType.Gem => "젬", RewardType.Fuel => "연료",
        RewardType.ResearchMaterial => "연구 재료", RewardType.Experience => "계정 경험치",
        RewardType.Item => "아이템", RewardType.Equipment => "장비", RewardType.Unit => "유닛",
        _ => type.ToString(),
    };
}
