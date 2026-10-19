using System;

public readonly struct LocalizationKey : IEquatable<LocalizationKey>
{
    public LocalizationKey(string table, string entry)
    {
        Table = table;
        Entry = entry;
    }

    public string Table { get; }
    public string Entry { get; }
    public bool IsValid => !string.IsNullOrWhiteSpace(Table) && !string.IsNullOrWhiteSpace(Entry);

    public bool Equals(LocalizationKey other) => Table == other.Table && Entry == other.Entry;
    public override bool Equals(object obj) => obj is LocalizationKey other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Table, Entry);
    public override string ToString() => IsValid ? $"{Table}:{Entry}" : "<invalid localization key>";
}
