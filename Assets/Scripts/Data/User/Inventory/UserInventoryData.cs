using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class UserInventoryData
{
    [FirestoreProperty] public List<InventoryStackItem> Materials { get; set; } = new();
    [FirestoreProperty] public List<InventoryStackItem> Consumables { get; set; } = new();
    [FirestoreProperty] public List<EquipmentItemData> Equipments { get; set; } = new();
}