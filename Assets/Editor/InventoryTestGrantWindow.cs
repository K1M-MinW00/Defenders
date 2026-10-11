using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class InventoryTestGrantWindow : EditorWindow
{
    private string itemId = "equipment_helmet_rare";
    private int amount = 1;
    private bool persistToFirebase;

    [MenuItem("Tools/GM Tool - Local Inventory")]
    public static void Open() => GetWindow<InventoryTestGrantWindow>("Local Inventory GM");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("현재 플레이어 테스트 지급", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Play Mode에서 현재 로그인한 테스트 계정의 인벤토리에 직접 지급합니다. UID 입력이나 GM 우편 쓰기 권한이 필요하지 않습니다.",
            MessageType.Info);

        itemId = EditorGUILayout.TextField("Item ID", itemId);
        amount = EditorGUILayout.IntField("Amount", amount);
        persistToFirebase = EditorGUILayout.Toggle("Firebase 저장", persistToFirebase);
        if (!persistToFirebase)
            EditorGUILayout.HelpBox("현재 실행 세션에만 반영됩니다. Play Mode를 종료하면 사라지므로 Firebase 보안 규칙과 무관하게 UI를 테스트할 수 있습니다.", MessageType.None);

        bool ready = EditorApplication.isPlaying && UserDataManager.Instance != null && UserDataManager.Instance.IsLoaded;
        using (new EditorGUI.DisabledScope(!ready))
        {
            if (GUILayout.Button("현재 플레이어에게 지급", GUILayout.Height(38)))
                Grant(new Dictionary<string, int> { [itemId] = amount });

            if (GUILayout.Button("장비 테스트 세트 12종 지급", GUILayout.Height(38)))
            {
                Dictionary<string, int> set = new();
                string[] parts = { "helmet", "armor", "main_weapon", "sub_weapon" };
                string[] rarities = { "rare", "epic", "legend" };
                foreach (string part in parts)
                    foreach (string rarity in rarities)
                        set[$"equipment_{part}_{rarity}"] = 1;
                Grant(set);
            }
        }

        if (!ready)
            EditorGUILayout.HelpBox("게임을 실행하고 테스트 계정 로그인이 완료되면 버튼이 활성화됩니다.", MessageType.Warning);
    }

    private async void Grant(IReadOnlyDictionary<string, int> items)
    {
        bool succeeded = await UserDataManager.Instance.GrantEditorTestItemsAsync(items, persistToFirebase);
        Debug.Log(succeeded ? "[Local Inventory GM] Test items granted." : "[Local Inventory GM] Grant failed.");
    }
}
