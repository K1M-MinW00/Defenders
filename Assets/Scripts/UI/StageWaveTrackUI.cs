using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageWaveTrackUI : MonoBehaviour
{
    [SerializeField] private Transform waveTrackContainer;
    [SerializeField] private WaveNodeUI waveNodePrefab;
    [SerializeField] private GameObject connectorPrefab;
    [SerializeField] private GameObject ellipsisPrefab;

    [Header("Wave Sprites")]
    [SerializeField] private Sprite normalWaveSprite;
    [SerializeField] private Sprite eliteWaveSprite;
    [SerializeField] private Sprite bossWaveSprite;

    [Header("Connector Colors")]
    [SerializeField] private Color clearedConnectorColor = Color.green;
    [SerializeField] private Color upcomingConnectorColor = Color.gray;

    private StageDataSO stageData;

    public void Initialize(StageDataSO stageData)
    {
        this.stageData = stageData;
    }

    public void Refresh(List<WaveData> waves, int currentIndex)
    {
        if (waveTrackContainer == null || waveNodePrefab == null || waves == null || waves.Count == 0)
            return;

        Clear();

        List<int> visibleIndices = BuildVisibleWaveIndices(waves.Count, currentIndex);
        bool showEllipsis = ShouldShowEllipsis(visibleIndices);

        for (int i = 0; i < visibleIndices.Count; i++)
        {
            int waveIndex = visibleIndices[i];

            WaveNodeUI node = Instantiate(waveNodePrefab, waveTrackContainer);
            node.Setup(GetWaveSprite(waves[waveIndex].waveType), waveIndex + 1);

            if (waveIndex == currentIndex)
                node.SetAsCurrent();
            else
                node.SetAsUpcoming();

            bool needConnector = i < visibleIndices.Count - 1;
            if (!needConnector)
                continue;

            bool insertEllipsisHere = showEllipsis && i == 2 && visibleIndices.Count == 4;

            if (insertEllipsisHere)
            {
                if (ellipsisPrefab != null)
                    Instantiate(ellipsisPrefab, waveTrackContainer);
            }
            else
            {
                CreateConnector(visibleIndices[i], visibleIndices[i + 1], currentIndex);
            }
        }
    }

    private void CreateConnector(int leftWaveIndex, int rightWaveIndex, int currentIndex)
    {
        if (connectorPrefab == null)
            return;

        GameObject connectorObj = Instantiate(connectorPrefab, waveTrackContainer);
        Image connectorImage = connectorObj.GetComponent<Image>();

        if (connectorImage == null)
            return;

        bool isClearedConnector = currentIndex >= rightWaveIndex;
        connectorImage.color = isClearedConnector ? clearedConnectorColor : upcomingConnectorColor;
    }

    private void Clear()
    {
        for (int i = waveTrackContainer.childCount - 1; i >= 0; i--)
            Destroy(waveTrackContainer.GetChild(i).gameObject);
    }

    private bool ShouldShowEllipsis(List<int> visibleIndices)
    {
        if (visibleIndices == null || visibleIndices.Count < 4)
            return false;

        int third = visibleIndices[2];
        int fourth = visibleIndices[3];
        return fourth - third > 1;
    }

    private List<int> BuildVisibleWaveIndices(int totalCount, int currentIndex)
    {
        List<int> result = new();

        if (totalCount <= 0)
            return result;

        currentIndex = Mathf.Clamp(currentIndex, 0, totalCount - 1);

        if (totalCount <= 4)
        {
            for (int i = 0; i < totalCount; i++)
                result.Add(i);
            return result;
        }

        int lastWindowStart = totalCount - 4;

        if (currentIndex >= lastWindowStart)
        {
            for (int i = lastWindowStart; i < totalCount; i++)
                result.Add(i);
            return result;
        }

        int blockStart = (currentIndex / 3) * 3;
        result.Add(blockStart);
        result.Add(blockStart + 1);
        result.Add(blockStart + 2);
        result.Add(totalCount - 1);

        return result;
    }

    private Sprite GetWaveSprite(WaveType type)
    {
        return type switch
        {
            WaveType.Normal => normalWaveSprite,
            WaveType.Elite => eliteWaveSprite,
            WaveType.Boss => bossWaveSprite,
            _ => normalWaveSprite
        };
    }
}