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

    private int selectedTabIndex = -1;
    private UnityAction[] clickHandlers;

    private void Awake()
    {
        if (tabs == null)
            return;

        clickHandlers = new UnityAction[tabs.Length];

        for (int i = 0; i < tabs.Length; i++)
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
        if (tabs == null || tabIndex < 0 || tabIndex >= tabs.Length || selectedTabIndex == tabIndex)
            return;

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

        selectedTabIndex = tabIndex;
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
