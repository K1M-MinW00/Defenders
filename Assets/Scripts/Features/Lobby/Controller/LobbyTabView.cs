using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyTabView : MonoBehaviour
{
    [System.Serializable]
    public class TabEntry
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
        selection = new LobbyTabSelection(tabCount);
        clickHandlers = new UnityAction[tabCount];
        SetAllTabsInactive();

        for (int i = 0; i < tabCount; i++)
        {
            TabEntry entry = tabs[i];
            if (entry?.button == null)
                continue;

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
            entry?.panel?.SetActive(false);
            entry?.highlight?.SetActive(false);
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
