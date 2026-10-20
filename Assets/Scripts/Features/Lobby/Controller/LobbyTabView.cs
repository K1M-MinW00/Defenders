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
        [Min(0)] public int requiredAccountLevel;
        public GameObject lockVisual;
    }

    [SerializeField] private TabEntry[] tabs;
    [SerializeField] private int defaultTabIndex = 2;

    private UnityAction[] clickHandlers;
    private LobbyTabSelection selection;
    private int? interactionRestrictedToIndex;
    public event Action<int> TabSelected;
    public int SelectedIndex => selection?.SelectedIndex ?? -1;

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
        EnsureLockVisuals();
        RefreshFeatureLocks();

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

    private void OnEnable()
    {
        if (UserDataManager.Instance != null)
            UserDataManager.Instance.OnProfileUpdated += RefreshFeatureLocks;
    }

    private void OnDisable()
    {
        if (UserDataManager.Instance != null)
            UserDataManager.Instance.OnProfileUpdated -= RefreshFeatureLocks;
    }

    public void ShowTab(int tabIndex)
    {
        if (selection == null || !selection.TrySelect(tabIndex))
            return;

        ApplySelection(tabIndex);
        TabSelected?.Invoke(tabIndex);
    }

    public Button GetTabButton(int tabIndex) =>
        tabs != null && tabIndex >= 0 && tabIndex < tabs.Length ? tabs[tabIndex]?.button : null;

    public RectTransform GetTabTarget(int tabIndex) =>
        GetTabButton(tabIndex)?.transform as RectTransform;

    public void RefreshFeatureLocks()
    {
        int accountLevel = UserDataManager.Instance?.UserData?.Profile?.Level ?? 0;
        if (tabs == null)
            return;

        foreach (TabEntry entry in tabs)
        {
            if (entry?.button == null)
                continue;

            int index = System.Array.IndexOf(tabs, entry);
            bool unlocked = LobbyFeatureUnlockPolicy.IsUnlocked(accountLevel, entry.requiredAccountLevel);
            bool allowedByGuide = !interactionRestrictedToIndex.HasValue || interactionRestrictedToIndex.Value == index;
            entry.button.interactable = unlocked && allowedByGuide;
            if (entry.lockVisual != null)
                entry.lockVisual.SetActive(!unlocked);
        }
    }

    public void RestrictInteractionTo(int? tabIndex)
    {
        interactionRestrictedToIndex = tabIndex;
        RefreshFeatureLocks();
    }

    private void EnsureLockVisuals()
    {
        if (tabs == null)
            return;

        foreach (TabEntry entry in tabs)
        {
            if (entry?.button == null || entry.requiredAccountLevel <= 0 || entry.lockVisual != null)
                continue;

            entry.lockVisual = LobbyLockVisualFactory.Create(entry.button.transform);
        }
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
        if (UserDataManager.Instance != null)
            UserDataManager.Instance.OnProfileUpdated -= RefreshFeatureLocks;

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
