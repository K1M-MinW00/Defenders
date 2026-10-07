using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class InventoryMutationServiceTests
{
    private readonly List<ItemDataSO> createdItems = new();

    [TearDown]
    public void TearDown()
    {
        foreach (ItemDataSO item in createdItems)
            Object.DestroyImmediate(item);

        createdItems.Clear();
    }

    [Test]
    public void Add_PlacesStackableItemsInTheirCategory()
    {
        MaterialDataSO material = CreateMaterial("material", MaterialType.Training);
        ConsumableDataSO consumable = CreateConsumable("ticket");
        InventoryMutationService service = CreateService(material, consumable);
        UserInventoryData inventory = new();

        InventoryMutationResult materialResult = service.Add(inventory, material.ItemId, 3);
        InventoryMutationResult consumableResult = service.Add(inventory, consumable.ItemId, 2);

        Assert.That(materialResult.Succeeded, Is.True);
        Assert.That(consumableResult.Succeeded, Is.True);
        Assert.That(inventory.Materials[0].Count, Is.EqualTo(3));
        Assert.That(inventory.Consumables[0].Count, Is.EqualTo(2));
    }

    [Test]
    public void Add_CreatesUniqueEquipmentInstances()
    {
        EquipmentDataSO equipment = CreateEquipment("sword");
        InventoryMutationService service = CreateService(equipment);
        UserInventoryData inventory = new();

        InventoryMutationResult result = service.Add(inventory, equipment.ItemId, 2);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(inventory.Equipments, Has.Count.EqualTo(2));
        Assert.That(inventory.Equipments[0].UniqueId, Is.Not.Empty);
        Assert.That(inventory.Equipments[1].UniqueId, Is.Not.EqualTo(inventory.Equipments[0].UniqueId));
    }

    [Test]
    public void ConsumeBatch_DoesNotMutate_WhenAnyItemIsInsufficient()
    {
        MaterialDataSO first = CreateMaterial("first", MaterialType.Training);
        MaterialDataSO second = CreateMaterial("second", MaterialType.Training);
        InventoryMutationService service = CreateService(first, second);
        UserInventoryData inventory = new();
        service.Add(inventory, first.ItemId, 5);
        service.Add(inventory, second.ItemId, 1);

        InventoryMutationResult result = service.ConsumeBatch(
            inventory,
            new Dictionary<string, int> { [first.ItemId] = 3, [second.ItemId] = 2 });

        Assert.That(result.Failure, Is.EqualTo(InventoryMutationFailure.InsufficientItems));
        Assert.That(service.GetCount(inventory, first.ItemId), Is.EqualTo(5));
        Assert.That(service.GetCount(inventory, second.ItemId), Is.EqualTo(1));
    }

    [Test]
    public void Consume_RemovesEmptyStack()
    {
        ConsumableDataSO consumable = CreateConsumable("ticket");
        InventoryMutationService service = CreateService(consumable);
        UserInventoryData inventory = new();
        service.Add(inventory, consumable.ItemId, 1);

        InventoryMutationResult result = service.Consume(inventory, consumable.ItemId, 1);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(inventory.Consumables, Is.Empty);
    }

    private InventoryMutationService CreateService(params ItemDataSO[] items) =>
        new(new ItemCatalog(items));

    private MaterialDataSO CreateMaterial(string id, MaterialType type)
    {
        MaterialDataSO item = ScriptableObject.CreateInstance<MaterialDataSO>();
        item.ItemId = id;
        item.MaterialType = type;
        createdItems.Add(item);
        return item;
    }

    private ConsumableDataSO CreateConsumable(string id)
    {
        ConsumableDataSO item = ScriptableObject.CreateInstance<ConsumableDataSO>();
        item.ItemId = id;
        createdItems.Add(item);
        return item;
    }

    private EquipmentDataSO CreateEquipment(string id)
    {
        EquipmentDataSO item = ScriptableObject.CreateInstance<EquipmentDataSO>();
        item.ItemId = id;
        createdItems.Add(item);
        return item;
    }
}
