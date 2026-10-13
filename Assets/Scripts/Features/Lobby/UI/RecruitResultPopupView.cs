using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class RecruitResultPopupView : MonoBehaviour, IModalDismissHandler
{
    [Header("UI")]
    [SerializeField] private Button closeButton;

    [Header("Result")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private RecruitUnitIconView slotPrefab;

    private readonly List<RecruitUnitIconView> slotPool = new();
    private int activeResultCount;
    private bool isPresenting;
    private ModalOverlayLifecycle lifecycle;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    public int ResultCount => activeResultCount;
    public event Action SkipRequested;

    private void Awake()
    {
        closeButton.onClick.AddListener(HandleCloseButton);
    }

    public void Prepare(IReadOnlyList<GachaResult> results)
    {
        Lifecycle.Show();
        HideAllSlots();
        isPresenting = true;

        if (results == null)
            return;

        for (int i = 0; i < results.Count; i++)
        {
            RecruitUnitIconView slot = GetOrCreateSlot(i);
            slot.Setup(results[i]);
            slot.Hide();
        }

        activeResultCount = results.Count;
    }

    public void Reveal(int index, float duration = 0f)
    {
        if (index < 0 || index >= activeResultCount)
            return;

        slotPool[index].RevealAnimated(duration);
    }

    public void RevealAll()
    {
        for (int i = 0; i < activeResultCount; i++)
            slotPool[i].ShowImmediately();

        isPresenting = false;
    }

    private RecruitUnitIconView GetOrCreateSlot(int index)
    {
        if (index < slotPool.Count)
            return slotPool[index];

        RecruitUnitIconView slot = Instantiate(slotPrefab, contentRoot);
        slot.name = $"RecruitResultSlot_{slotPool.Count + 1}";
        slotPool.Add(slot);
        return slot;
    }

    private void HideAllSlots()
    {
        for (int i = 0; i < slotPool.Count; i++)
            slotPool[i].Hide();

        activeResultCount = 0;
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
