using UnityEditor;
using UnityEngine;

public sealed class TutorialDebugWindow : EditorWindow
{
    private int selectedStepIndex;
    private bool isSaving;
    private string resultMessage = string.Empty;

    [MenuItem("Tools/Tutorial/Debug Window")]
    private static void Open() => GetWindow<TutorialDebugWindow>("Tutorial Debug");

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += HandlePlayModeChanged;
        EditorApplication.update += Repaint;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
        EditorApplication.update -= Repaint;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("First Battle Tutorial", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Play Mode에서 로그인한 뒤 사용할 수 있습니다.",
                MessageType.Info);
            return;
        }

        UserDataManager manager = UserDataManager.Instance;
        if (manager == null || !manager.IsLoaded)
        {
            EditorGUILayout.HelpBox("사용자 데이터 로드를 기다리는 중입니다.", MessageType.Info);
            return;
        }

        UserTutorialData tutorial = manager.UserData?.Tutorial;
        EditorGUILayout.LabelField("User", manager.CurrentUserId ?? "-");
        EditorGUILayout.LabelField("Current step", tutorial?.NextStepId ?? "-");
        EditorGUILayout.LabelField("Completed", (tutorial?.Completed ?? false).ToString());
        EditorGUILayout.LabelField("Skipped", (tutorial?.Skipped ?? false).ToString());
        EditorGUILayout.Space();

        string[] steps = new string[FirstBattleTutorialSequence.Steps.Count];
        for (int i = 0; i < steps.Length; i++)
            steps[i] = $"{i + 1:00}. {FirstBattleTutorialSequence.Steps[i]}";

        selectedStepIndex = EditorGUILayout.Popup(
            "Start from step",
            Mathf.Clamp(selectedStepIndex, 0, steps.Length - 1),
            steps);

        using (new EditorGUI.DisabledScope(isSaving))
        {
            if (GUILayout.Button("Reset to First Battle"))
                SaveStep(FirstBattleTutorialSequence.SummonWave1Unit);

            if (GUILayout.Button("Set Selected Step"))
                SaveStep(FirstBattleTutorialSequence.Steps[selectedStepIndex]);
        }

        if (!string.IsNullOrWhiteSpace(resultMessage))
            EditorGUILayout.HelpBox(resultMessage, MessageType.None);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "로비 단계는 LobbyScene에서, 전투 행동 단계는 튜토리얼 전투 진입 후 확인하세요. " +
            "변경 내용은 현재 로그인 계정의 Firestore에 저장됩니다.",
            MessageType.Warning);
    }

    private async void SaveStep(string stepId)
    {
        UserDataManager manager = UserDataManager.Instance;
        if (manager == null || !manager.IsLoaded || isSaving)
            return;

        isSaving = true;
        resultMessage = "Saving...";
        bool succeeded = await manager.SetTutorialStepForDebugAsync(stepId);
        resultMessage = succeeded ? $"Saved: {stepId}" : "Save failed. Check the Console.";
        isSaving = false;
        Repaint();
    }

    private void HandlePlayModeChanged(PlayModeStateChange _) => Repaint();
}
