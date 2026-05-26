using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GMToolWindow : EditorWindow
{
    [Serializable]
    public class RewardEntry
    {
        public RewardType Type;
        public string Id;
        public int Amount = 1;
    }

    private Vector2 scroll;

    private string targetUid;

    private string title = "GM Reward";

    [TextArea(3, 5)]
    private string description = "운영 보상입니다.";

    private MailType mailType = MailType.GM;

    private int expireDays = 7;

    private readonly List<RewardEntry> rewards = new();

    [MenuItem("Tools/GM Tool")]
    public static void Open()
    {
        GetWindow<GMToolWindow>("GM Tool");
    }

    private void OnEnable()
    {
        if (rewards.Count == 0)
        {
            rewards.Add(new RewardEntry());
        }
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        GUILayout.Space(10);

        DrawMailSection();

        GUILayout.Space(15);

        DrawRewardSection();

        GUILayout.Space(20);

        DrawBottomButtons();

        EditorGUILayout.EndScrollView();
    }

    private void DrawMailSection()
    {
        GUILayout.Label("Mail Info", EditorStyles.boldLabel);

        targetUid =
            EditorGUILayout.TextField("Target UID", targetUid);

        title =
            EditorGUILayout.TextField("Title", title);

        GUILayout.Label("Description");

        description =
            EditorGUILayout.TextArea(description, GUILayout.Height(60));

        mailType =
            (MailType)EditorGUILayout.EnumPopup(
                "Mail Type",
                mailType);

        expireDays =
            EditorGUILayout.IntField(
                "Expire Days",
                expireDays);
    }

    private void DrawRewardSection()
    {
        GUILayout.Label("Rewards", EditorStyles.boldLabel);

        for (int i = 0; i < rewards.Count; i++)
        {
            DrawRewardEntry(i);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("+ Add Reward"))
        {
            rewards.Add(new RewardEntry());
        }
    }

    private void DrawRewardEntry(int index)
    {
        RewardEntry reward = rewards[index];

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label($"Reward #{index + 1}",
            EditorStyles.boldLabel);

        GUILayout.FlexibleSpace();

        GUI.backgroundColor = Color.red;

        if (GUILayout.Button("Remove", GUILayout.Width(80)))
        {
            rewards.RemoveAt(index);

            GUI.backgroundColor = Color.white;

            return;
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        reward.Type =
            (RewardType)EditorGUILayout.EnumPopup(
                "Type",
                reward.Type);

        reward.Id =
            EditorGUILayout.TextField(
                "Id",
                reward.Id);

        reward.Amount =
            EditorGUILayout.IntField(
                "Amount",
                reward.Amount);

        EditorGUILayout.EndVertical();

        GUILayout.Space(5);
    }

    private void DrawBottomButtons()
    {
        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("Send Mail", GUILayout.Height(40)))
        {
            SendMail();
        }

        GUI.backgroundColor = Color.white;
    }

    private async void SendMail()
    {
        if (string.IsNullOrWhiteSpace(targetUid))
        {
            Debug.LogError("Target UID is Empty");
            return;
        }

        List<RewardData> rewardDataList = new();

        foreach (RewardEntry reward in rewards)
        {
            rewardDataList.Add(new RewardData
            {
                Type = reward.Type,
                Id = reward.Id,
                Amount = reward.Amount
            });
        }

        await GMFirestoreService.SendMailAsync(
            targetUid,
            title,
            description,
            mailType,
            rewardDataList,
            expireDays);

        Debug.Log("GM Mail Send Complete");
    }
}