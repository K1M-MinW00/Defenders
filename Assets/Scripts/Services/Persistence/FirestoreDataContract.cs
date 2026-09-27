internal static class FirestoreDataContract
{
    public const string UsersCollection = "users";
    public const string MailboxesCollection = "mailboxes";
    public const string MailsCollection = "mails";

    public static class UserFields
    {
        public const string SchemaVersion = "SchemaVersion";
        public const string CreatedAt = "CreatedAt";
        public const string UpdatedAt = "UpdatedAt";
        public const string Profile = "Profile";
        public const string Resource = "Resource";
        public const string Roster = "Roster";
        public const string Progress = "Progress";
        public const string Inventory = "Inventory";
        public const string Gacha = "Gacha";
        public const string Ad = "Ad";
        public const string Shop = "Shop";
    }

    public static class MailFields
    {
        public const string Claimed = "Claimed";
    }
}
