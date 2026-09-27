using System;
using Firebase.Firestore;
using UnityEngine;

[Serializable]
[FirestoreData]
public class RewardData
{
    [SerializeField] private RewardType type;
    [SerializeField] private string id;
    [SerializeField] private int amount;

    [FirestoreProperty]
    public RewardType Type
    {
        get => type;
        set => type = value;
    }

    [FirestoreProperty]
    public string Id
    {
        get => id;
        set => id = value;
    }

    [FirestoreProperty]
    public int Amount
    {
        get => amount;
        set => amount = value;
    }
}
