using System;
using System.Collections.Generic;
using UnityEngine;

public static class PopupBackStack
{
    private static readonly List<Entry> Entries = new();

    public static void Push(MonoBehaviour owner, Action closeAction)
    {
        if (owner == null || closeAction == null)
            return;

        Remove(owner);
        Entries.Add(new Entry(owner, closeAction));
    }

    public static void Remove(MonoBehaviour owner)
    {
        if (owner == null)
            return;

        for (int i = Entries.Count - 1; i >= 0; i--)
        {
            if (Entries[i].Owner == null || Entries[i].Owner == owner)
                Entries.RemoveAt(i);
        }
    }

    public static bool TryCloseTop()
    {
        while (Entries.Count > 0)
        {
            int index = Entries.Count - 1;
            Entry entry = Entries[index];

            if (entry.Owner == null || !entry.Owner.isActiveAndEnabled)
            {
                Entries.RemoveAt(index);
                continue;
            }

            entry.CloseAction.Invoke();
            return true;
        }

        return false;
    }

    private readonly struct Entry
    {
        public Entry(MonoBehaviour owner, Action closeAction)
        {
            Owner = owner;
            CloseAction = closeAction;
        }

        public MonoBehaviour Owner { get; }
        public Action CloseAction { get; }
    }
}
