using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecruitResultPopupView : MonoBehaviour, IModalDismissHandler
{
    [Header("UI")]
    [SerializeField] private Button closeButton;

    [Header("Result")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private RecruitUnitIconView slotPrefab;

    private readonly List<RecruitUnitIconView> slots = new();
    private bool isPresenting;
    private ModalOverlayLifecycle lifecycle;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    public int ResultCount => slots.Count;
    public event Action SkipRequested;

    private void Awake()
    {
        closeButton.onClick.AddListener(HandleCloseButton);
    }

    public void Open(List<GachaResult> results)
    {
        Prepare(results);
        RevealAll();
    }

    public void Prepare(IReadOnlyList<GachaResult> results)
    {
        Lifecycle.Show();
        Clear();
        isPresenting = true;

        if (results == null)
            return;

        foreach (GachaResult result in results)
        {
            RecruitUnitIconView slot = Instantiate(slotPrefab, contentRoot);
            slot.Setup(result);
            slot.gameObject.SetActive(false);
            slots.Add(slot);
        }
    }

    public void Reveal(int index, float duration = 0f)
    {
        if (index < 0 || index >= slots.Count)
            return;

        slots[index].RevealAnimated(duration);
    }

    public void RevealAll()
    {
        foreach (RecruitUnitIconView slot in slots)
            slot.ShowImmediately();

        isPresenting = false;
    }

    private void Clear()
    {
        slots.Clear();

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);
    }

    public void Close()
    {
        Lifecycle.Close();
    }

    public void Dismiss()
    {
        if (isPresenting)
        {
            SkipRequested?.Invoke();
            return;
        }

        Close();
    }

    private void HandleCloseButton()
    {
        Dismiss();
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(HandleCloseButton);
    }
}
