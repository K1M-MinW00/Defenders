using UnityEngine;
using UnityEngine.UI;

public class LobbyTabController : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Button button;
        public GameObject highlight;
        public GameObject panel;
    }

    [SerializeField] private Tab[] tabs;
    [SerializeField] private int defaultTab = 2;

    private int currentIndex = -1;

    private void Awake()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int idx = i;
            tabs[i].button.onClick.AddListener(() => SelectTab(idx));
        }
    }

    private void Start()
    {
        SelectTab(defaultTab);
    }

    public void SelectTab(int idx)
    {
        if (currentIndex == idx)
            return;

        for(int i=0;i<tabs.Length; i++)
        {
            tabs[i].panel.SetActive(i == idx);

            if (tabs[i].highlight != null)
                tabs[i].highlight.SetActive(i == idx);
        }

        currentIndex = idx;
    }
}
