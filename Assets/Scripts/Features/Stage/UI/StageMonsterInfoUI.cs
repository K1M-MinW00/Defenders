using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageMonsterInfoUI : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeBlockerButton;

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private StageMonsterInfoItemUI itemTemplate;
    [SerializeField] private TMP_Text emptyText;

    private readonly List<StageMonsterInfoItemUI> itemPool = new();
    private StageSessionController session;
    public event Action<bool> VisibilityChanged;

    public void Initialize(StageSessionController stageSession)
    {
        Dispose();
        session = stageSession;

        openButton?.onClick.AddListener(Open);
        closeBlockerButton?.onClick.AddListener(Close);
        Close();
    }

    public void Dispose()
    {
        openButton?.onClick.RemoveListener(Open);
        closeBlockerButton?.onClick.RemoveListener(Close);
        HideItems();
        session = null;
    }

    public void Close()
    {
        if (panelRoot == null)
            return;

        bool wasVisible = panelRoot.activeSelf;
        panelRoot.SetActive(false);
        if (wasVisible)
            VisibilityChanged?.Invoke(false);
    }

    private void Open()
    {
        if (panelRoot == null || session?.CurrentWave == null)
            return;

        Populate(session.CurrentWave);
        panelRoot.SetActive(true);
        VisibilityChanged?.Invoke(true);

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    private void Populate(WaveData wave)
    {
        HideItems();

        HashSet<MonsterDataSO> added = new();
        int visibleCount = 0;
        if (wave?.subWaves != null)
        {
            foreach (SubWaveData subWave in wave.subWaves)
            {
                if (subWave?.spawnEntries == null)
                    continue;

                foreach (MonsterSpawnEntry entry in subWave.spawnEntries)
                {
                    MonsterDataSO data = entry?.data;
                    if (data == null || !added.Add(data))
                        continue;

                    StageMonsterInfoItemUI item = GetOrCreateItem(visibleCount++);
                    item.name = $"Monster Info - {data.name}";
                    item.gameObject.SetActive(true);
                    item.Bind(data);
                }
            }
        }

        if (emptyText != null)
            emptyText.gameObject.SetActive(visibleCount == 0);
    }

    private StageMonsterInfoItemUI GetOrCreateItem(int index)
    {
        if (index < itemPool.Count)
            return itemPool[index];

        StageMonsterInfoItemUI item = Instantiate(itemTemplate, contentRoot);
        itemPool.Add(item);
        return item;
    }

    private void HideItems()
    {
        foreach (StageMonsterInfoItemUI item in itemPool)
            if (item != null)
                item.gameObject.SetActive(false);
    }
}
