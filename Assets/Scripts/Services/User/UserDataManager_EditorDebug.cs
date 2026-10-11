#if UNITY_EDITOR
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class UserDataManager
{
    public async Task<bool> GrantEditorTestItemsAsync(IReadOnlyDictionary<string, int> grants, bool persistToFirebase = false)
    {
        if (!IsLoaded || grants == null || grants.Count == 0 || UserData?.Inventory == null)
            return false;

        return await RunSerializedMutationAsync(async () =>
        {
            UserInventoryData next = UserDataCloner.Copy(UserData.Inventory);
            InventoryMutationService mutations = new(GameConfig.Items);
            EquipmentInventoryIndex index = new(next, UserData.Roster);
            int remainingEquipmentSlots = EquipmentInventoryIndex.UnequippedCapacity - index.UnequippedCount;

            foreach (KeyValuePair<string, int> grant in grants)
            {
                ItemDataSO definition = GameConfig.Items.Get(grant.Key);
                if (definition == null || grant.Value <= 0)
                {
                    Debug.LogError($"[Test Item Grant] Invalid item: {grant.Key} x{grant.Value}");
                    return false;
                }

                if (definition.Category == ItemCategory.Equipment)
                {
                    if (grant.Value > remainingEquipmentSlots)
                    {
                        Debug.LogError($"[Test Item Grant] Equipment capacity exceeded: {grant.Key} x{grant.Value}");
                        return false;
                    }
                    remainingEquipmentSlots -= grant.Value;
                }

                if (!mutations.Add(next, grant.Key, grant.Value).Succeeded)
                    return false;
            }

            if (persistToFirebase)
            {
                try { await repository.SaveSectionsAsync(CurrentUserId, new UserDataUpdate { Inventory = next }); }
                catch (System.Exception exception)
                {
                    Debug.LogException(exception);
                    return false;
                }
            }

            UserData.Inventory = next;
            RaiseInventoryUpdated();
            Debug.Log(persistToFirebase
                ? "[Test Item Grant] Applied and saved to Firebase."
                : "[Test Item Grant] Applied to this Play Mode session only.");
            return true;
        });
    }
}
#endif
