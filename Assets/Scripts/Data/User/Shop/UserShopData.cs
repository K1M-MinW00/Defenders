using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public sealed class UserShopData
{
    [FirestoreProperty] public List<UserShopPurchaseData> Purchases { get; set; } = new();
    [FirestoreProperty] public List<UserShopTabSeenData> SeenTabs { get; set; } = new();
}

[FirestoreData]
public sealed class UserShopTabSeenData
{
    [FirestoreProperty] public int Tab { get; set; }
    [FirestoreProperty] public string Marker { get; set; }
}

[FirestoreData]
public sealed class UserShopPurchaseData
{
    [FirestoreProperty] public string ProductId { get; set; }
    [FirestoreProperty] public string PeriodKey { get; set; }
    [FirestoreProperty] public int Count { get; set; }
    [FirestoreProperty] public Timestamp LastPurchasedAt { get; set; }
}
