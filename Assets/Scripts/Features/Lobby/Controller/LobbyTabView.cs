using UnityEngine;
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

    private void Awake()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int idx = i;
            tabs[i].button.onClick.AddListener(() => ShowTab(idx));
        }
    }

    private void Start()
    {
        ShowTab(defaultTabIndex);
    }

    public void ShowTab(int tabIndex)
    {
        if (selectedTabIndex == tabIndex)
            return;

        for(int i=0;i<tabs.Length; i++)
        {
            tabs[i].panel.SetActive(i == tabIndex);

            if (tabs[i].highlight != null)
                tabs[i].highlight.SetActive(i == tabIndex);
        }

        selectedTabIndex = tabIndex;
    }
}
