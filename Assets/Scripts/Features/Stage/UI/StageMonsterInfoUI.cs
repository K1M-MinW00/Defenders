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

    private readonly List<StageMonsterInfoItemUI> spawnedItems = new();
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
        ClearItems();
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
        ClearItems();

        HashSet<MonsterDataSO> added = new();
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

                    StageMonsterInfoItemUI item = Instantiate(itemTemplate, contentRoot);
                    item.name = $"Monster Info - {data.name}";
                    item.gameObject.SetActive(true);
                    item.Bind(data);
                    spawnedItems.Add(item);
                }
            }
        }

        if (emptyText != null)
            emptyText.gameObject.SetActive(spawnedItems.Count == 0);
    }

    private void ClearItems()
    {
        foreach (StageMonsterInfoItemUI item in spawnedItems)
        {
            if (item != null)
            {
                item.gameObject.SetActive(false);
                Destroy(item.gameObject);
            }
        }

        spawnedItems.Clear();
    }
}
