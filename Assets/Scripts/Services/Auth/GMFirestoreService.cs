using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class GMFirestoreService
{
    public static async Task SendMailAsync(string uid,string title,string description,MailType mailType,List<RewardData> rewards,int expireDays)
    {
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogError("UID is null or empty.");
            return;
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        string mailId = Guid.NewGuid().ToString();

        MailData mail = new()
        {
            MailId = mailId,
            Title = title,
            Description = description,
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            ExpireAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(expireDays)),
            Claimed = false,
            MailType = mailType,
            Rewards = rewards
        };

        try
        {
            await db
                .Collection("mailboxes")
                .Document(uid)
                .Collection("mails")
                .Document(mailId)
                .SetAsync(mail);

            Debug.Log(
                $"Mail Send Success : {uid}");
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"SendMailAsync Exception : {e}");
        }
    }
}