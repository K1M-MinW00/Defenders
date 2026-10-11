using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LobbyModalStack : MonoBehaviour
{
    private static readonly List<ModalBackdropDismiss> Entries = new();

    public static event Action EmptyBackRequested;
    [SerializeField] private LobbyExitConfirmationView exitConfirmation;

    public static void Register(ModalBackdropDismiss entry)
    {
        if (entry == null) return;
        Entries.Remove(entry);
        Entries.Add(entry);
        RefreshInputState();
    }

    public static void Unregister(ModalBackdropDismiss entry)
    {
        if (entry != null) Entries.Remove(entry);
        RefreshInputState();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        for (int i = Entries.Count - 1; i >= 0; i--)
        {
            ModalBackdropDismiss entry = Entries[i];
            if (entry == null || !entry.isActiveAndEnabled)
            {
                Entries.RemoveAt(i);
                continue;
            }

            entry.DismissFromSystemBack();
            return;
        }

        if (EmptyBackRequested != null)
        {
            EmptyBackRequested.Invoke();
            return;
        }

        exitConfirmation.Show();
    }

    private static void RefreshInputState()
    {
        for (int i = Entries.Count - 1; i >= 0; i--)
        {
            if (Entries[i] == null || !Entries[i].isActiveAndEnabled)
                Entries.RemoveAt(i);
        }

        Transform top = Entries.Count > 0 ? Entries[^1].transform : null;
        for (int i = 0; i < Entries.Count; i++)
        {
            // 자식 Overlay가 최상단인 경우 부모의 CanvasGroup을 끄면 자식 입력도 함께 막힌다.
            bool isTopOrAncestor = i == Entries.Count - 1 ||
                                   (top != null && top.IsChildOf(Entries[i].transform));
            Entries[i].SetStackInteraction(isTopOrAncestor);
        }
    }

    private void OnDestroy()
    {
        Entries.Clear();
        EmptyBackRequested = null;
    }
}
