using System;

public static class UserDataMigrator
{
    public static bool MigrateToCurrent(UserDataRoot data, string userId)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (data.SchemaVersion < 0)
            throw new InvalidOperationException($"Invalid user data schema version: {data.SchemaVersion}");

        if (data.SchemaVersion > UserDataSchema.CurrentVersion)
        {
            throw new InvalidOperationException(
                $"User data schema version {data.SchemaVersion} is newer than supported version {UserDataSchema.CurrentVersion}.");
        }

        bool changed = UserDataNormalizer.Normalize(data, userId);

        while (data.SchemaVersion < UserDataSchema.CurrentVersion)
        {
            switch (data.SchemaVersion)
            {
                case 0:
                    MigrateVersion0To1(data);
                    break;

                case 1:
                    MigrateVersion1To2(data);
                    break;

                case 2:
                    MigrateVersion2To3(data);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"No migration path exists for user data schema version {data.SchemaVersion}.");
            }

            changed = true;
        }

        return changed;
    }

    private static void MigrateVersion0To1(UserDataRoot data)
    {
        data.SchemaVersion = 1;
    }

    private static void MigrateVersion1To2(UserDataRoot data)
    {
        data.Shop ??= UserDataFactory.CreateDefaultShop();
        data.SchemaVersion = 2;
    }

    private static void MigrateVersion2To3(UserDataRoot data)
    {
        data.Shop ??= UserDataFactory.CreateDefaultShop();
        data.Shop.SeenTabs ??= new System.Collections.Generic.List<UserShopTabSeenData>();
        data.SchemaVersion = 3;
    }
}
