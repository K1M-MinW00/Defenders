using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class UserDataManager
{
    public string GetShopNotificationMarker(
        ShopTabType tab,
        ShopProductGroup group,
        DateTimeOffset? utcNow = null)
    {
        return ShopNotificationPolicy.BuildMarker(
            tab,
            group,
            product => GetShopProductState(product, utcNow),
            utcNow ?? DateTimeOffset.UtcNow);
    }

    public bool HasShopNotification(
        ShopTabType tab,
        ShopProductGroup group,
        DateTimeOffset? utcNow = null)
    {
        if (UserData?.Shop == null)
            return false;

        string marker = GetShopNotificationMarker(tab, group, utcNow);
        return ShopNotificationPolicy.HasNotification(UserData.Shop, tab, marker);
    }

    public async Task<bool> MarkShopTabSeenAsync(
        ShopTabType tab,
        ShopProductGroup group,
        DateTimeOffset? utcNow = null)
    {
        if (UserData?.Shop == null || group == null)
            return false;

        string marker = GetShopNotificationMarker(tab, group, utcNow);
        if (string.IsNullOrWhiteSpace(marker) ||
            !ShopNotificationPolicy.HasNotification(UserData.Shop, tab, marker))
            return true;

        return await RunSerializedMutationAsync(async () =>
        {
            List<UserShopTabSeenData> previous = UserData.Shop.SeenTabs?
                .Where(seen => seen != null)
                .Select(seen => new UserShopTabSeenData
                {
                    Tab = seen.Tab,
                    Marker = seen.Marker,
                })
                .ToList() ?? new List<UserShopTabSeenData>();

            ShopNotificationPolicy.MarkSeen(UserData.Shop, tab, marker);
            bool saved = await SaveSectionAsync(
                UserData.Shop,
                () => repository.SaveSectionsAsync(
                    CurrentUserId,
                    new UserDataUpdate { Shop = UserData.Shop }),
                "shop notification");

            if (!saved)
            {
                UserData.Shop.SeenTabs = previous;
                return false;
            }

            RaiseShopUpdated();
            return true;
        });
    }
}
