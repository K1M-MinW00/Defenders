using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LobbyTabView : MonoBehaviour
{
    [Serializable]
    private sealed class TabEntry
    {
        public Button button;
        public GameObject highlight;
        public GameObject panel;
    }

    [SerializeField] private TabEntry[] tabs;
    [SerializeField] private int defaultTabIndex = 2;

    private UnityAction[] clickHandlers;
    private LobbyTabSelection selection;

    private void Awake()
    {
        int tabCount = tabs?.Length ?? 0;
        if (!ValidateReferences(tabCount))
        {
            enabled = false;
            return;
        }

        selection = new LobbyTabSelection(tabCount);
        clickHandlers = new UnityAction[tabCount];
        SetAllTabsInactive();

        for (int i = 0; i < tabCount; i++)
        {
            TabEntry entry = tabs[i];
            int tabIndex = i;
            clickHandlers[i] = () => ShowTab(tabIndex);
            entry.button.onClick.AddListener(clickHandlers[i]);
        }
    }

    private void Start()
    {
        if (tabs == null || tabs.Length == 0)
            return;

        ShowTab(Mathf.Clamp(defaultTabIndex, 0, tabs.Length - 1));
    }

    public void ShowTab(int tabIndex)
    {
        if (selection == null || !selection.TrySelect(tabIndex))
            return;

        ApplySelection(tabIndex);
    }

    public bool ShowPanel(string panelName)
    {
        if (tabs == null || string.IsNullOrWhiteSpace(panelName))
            return false;

        for (int i = 0; i < tabs.Length; i++)
        {
            if (string.Equals(tabs[i].panel.name, panelName, StringComparison.Ordinal))
            {
                ShowTab(i);
                return true;
            }
        }
        return false;
    }

    private bool ValidateReferences(int tabCount)
    {
        if (tabCount == 0)
        {
            Debug.LogError($"[{nameof(LobbyTabView)}] At least one tab entry is required.", this);
            return false;
        }

        bool isValid = true;
        for (int i = 0; i < tabCount; i++)
        {
            TabEntry entry = tabs[i];
            if (entry != null && entry.button != null && entry.highlight != null && entry.panel != null)
                continue;

            Debug.LogError($"[{nameof(LobbyTabView)}] Tab entry {i} is missing a button, highlight, or panel reference.", this);
            isValid = false;
        }

        return isValid;
    }

    private void ApplySelection(int tabIndex)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            TabEntry entry = tabs[i];
            if (entry == null)
                continue;

            bool isSelected = i == tabIndex;

            if (entry.panel != null)
                entry.panel.SetActive(isSelected);

            if (entry.highlight != null)
                entry.highlight.SetActive(isSelected);
        }

    }

    private void SetAllTabsInactive()
    {
        if (tabs == null)
            return;

        foreach (TabEntry entry in tabs)
        {
            if (entry?.panel != null)
                entry.panel.SetActive(false);

            if (entry?.highlight != null)
                entry.highlight.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (tabs == null || clickHandlers == null)
            return;

        int count = Mathf.Min(tabs.Length, clickHandlers.Length);
        for (int i = 0; i < count; i++)
        {
            if (tabs[i]?.button != null && clickHandlers[i] != null)
                tabs[i].button.onClick.RemoveListener(clickHandlers[i]);
        }
    }
}
