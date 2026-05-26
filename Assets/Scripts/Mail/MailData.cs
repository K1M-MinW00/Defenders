using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public class MailData
{
    [FirestoreProperty]
    public string MailId { get; set; }

    [FirestoreProperty]
    public string Title { get; set; }

    [FirestoreProperty]
    public string Description { get; set; }

    [FirestoreProperty]
    public long CreatedAt { get; set; }

    [FirestoreProperty]
    public long ExpireAt { get; set; }

    [FirestoreProperty]
    public bool Claimed { get; set; }

    [FirestoreProperty]
    public bool Deleted { get; set; }

    [FirestoreProperty]
    public MailType MailType { get; set; }

    [FirestoreProperty]
    public List<RewardData> Rewards { get; set; }
}

[FirestoreData]
public class RewardData
{
    [FirestoreProperty]
    public RewardType Type { get; set; }

    [FirestoreProperty]
    public string Id { get; set; }

    [FirestoreProperty]
    public int Amount { get; set; }
}

public enum RewardType
{
    Gold,
    Gem,
    Fuel,
    Item,
    Equipment,
    Unit,
}

public enum MailType
{
    System,
    Event,
    Coupon,
    GM,
    Purchase,
    Compensation
}