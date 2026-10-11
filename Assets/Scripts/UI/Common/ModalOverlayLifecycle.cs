using System;
using UnityEngine;

/// <summary>
/// Centralizes the visibility and transition lifecycle for an existing modal root.
/// The owner remains responsible for clearing feature-specific transient state.
/// </summary>
public sealed class ModalOverlayLifecycle
{
    private readonly GameObject root;
    private bool isClosing;

    public ModalOverlayLifecycle(GameObject rootObject)
    {
        root = rootObject;
    }

    public bool IsVisible => root != null && root.activeSelf;

    public void Show()
    {
        if (root == null)
            return;

        isClosing = false;
        root.SetActive(true);
        root.transform.SetAsLastSibling();
    }

    public void Close(Action onClosed = null)
    {
        if (root == null)
        {
            onClosed?.Invoke();
            return;
        }

        if (isClosing)
            return;

        isClosing = true;
        ModalTransitionUtility.RequestExit(root, () =>
        {
            isClosing = false;
            root.SetActive(false);
            onClosed?.Invoke();
        });
    }

    public void HideImmediately()
    {
        isClosing = false;
        if (root != null)
            root.SetActive(false);
    }
}
