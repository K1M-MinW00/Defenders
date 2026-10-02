using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageWaveTrackUI : MonoBehaviour
{
    private const int MaxVisibleNodes = 4;
    private const int MaxVisibleConnectors = MaxVisibleNodes - 1;

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

    private readonly List<WaveNodeUI> nodeSlots = new(MaxVisibleNodes);
    private readonly List<ConnectorSlot> connectorSlots = new(MaxVisibleConnectors);
    private bool slotsCreated;

    public void Initialize(StageDataSO stageData)
    {
        EnsureSlots();
    }

    public void Refresh(List<WaveData> waves, int currentIndex)
    {
        if (waves == null || waves.Count == 0 || !EnsureSlots())
        {
            SetAllSlotsActive(false);
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, waves.Count - 1);
        List<int> visibleIndices = BuildVisibleWaveIndices(waves.Count, currentIndex);

        for (int slotIndex = 0; slotIndex < nodeSlots.Count; slotIndex++)
        {
            bool isVisible = slotIndex < visibleIndices.Count;
            WaveNodeUI node = nodeSlots[slotIndex];
            node.gameObject.SetActive(isVisible);
            if (!isVisible)
                continue;

            int waveIndex = visibleIndices[slotIndex];
            node.Setup(GetWaveSprite(waves[waveIndex].waveType), waveIndex + 1);
            if (waveIndex == currentIndex)
                node.SetAsCurrent();
        }

        for (int slotIndex = 0; slotIndex < connectorSlots.Count; slotIndex++)
        {
            bool isVisible = slotIndex < visibleIndices.Count - 1;
            if (!isVisible)
            {
                connectorSlots[slotIndex].Hide();
                continue;
            }

            int leftWaveIndex = visibleIndices[slotIndex];
            int rightWaveIndex = visibleIndices[slotIndex + 1];
            bool isEllipsis = rightWaveIndex - leftWaveIndex > 1;
            bool isCleared = currentIndex >= rightWaveIndex;
            connectorSlots[slotIndex].Show(
                isEllipsis,
                isCleared ? clearedConnectorColor : Color.white);
        }
    }

    public static List<int> BuildVisibleWaveIndices(int totalCount, int currentIndex)
    {
        List<int> result = new(MaxVisibleNodes);
        if (totalCount <= 0)
            return result;

        currentIndex = Mathf.Clamp(currentIndex, 0, totalCount - 1);
        if (totalCount <= MaxVisibleNodes)
        {
            for (int index = 0; index < totalCount; index++)
                result.Add(index);
            return result;
        }

        int finalWaveIndex = totalCount - 1;
        int blockStart = currentIndex / 3 * 3;
        bool isFinalBlock = blockStart + 3 >= finalWaveIndex;

        if (isFinalBlock)
        {
            int finalWindowStart = totalCount - MaxVisibleNodes;
            for (int index = finalWindowStart; index < totalCount; index++)
                result.Add(index);
            return result;
        }

        result.Add(blockStart);
        result.Add(blockStart + 1);
        result.Add(blockStart + 2);
        result.Add(finalWaveIndex);
        return result;
    }

    private bool EnsureSlots()
    {
        if (slotsCreated)
            return true;

        if (waveTrackContainer == null || waveNodePrefab == null ||
            connectorPrefab == null || ellipsisPrefab == null)
        {
            Debug.LogError($"[{nameof(StageWaveTrackUI)}] Wave track references are missing.", this);
            return false;
        }

        for (int slotIndex = 0; slotIndex < MaxVisibleNodes; slotIndex++)
        {
            WaveNodeUI node = Instantiate(waveNodePrefab, waveTrackContainer);
            node.name = $"WaveNode_{slotIndex + 1}";
            nodeSlots.Add(node);

            if (slotIndex >= MaxVisibleConnectors)
                continue;

            GameObject line = Instantiate(connectorPrefab, waveTrackContainer);
            line.name = $"WaveConnector_{slotIndex + 1}";
            GameObject ellipsis = Instantiate(ellipsisPrefab, waveTrackContainer);
            ellipsis.name = $"WaveEllipsis_{slotIndex + 1}";
            connectorSlots.Add(new ConnectorSlot(line, ellipsis));
        }

        slotsCreated = true;
        SetAllSlotsActive(false);
        return true;
    }

    private void SetAllSlotsActive(bool active)
    {
        foreach (WaveNodeUI node in nodeSlots)
            node.gameObject.SetActive(active);
        foreach (ConnectorSlot connector in connectorSlots)
            connector.Hide();
    }

    private Sprite GetWaveSprite(WaveType type)
    {
        return type switch
        {
            WaveType.Normal => normalWaveSprite,
            WaveType.Elite => eliteWaveSprite,
            WaveType.Boss => bossWaveSprite,
            _ => normalWaveSprite,
        };
    }

    private sealed class ConnectorSlot
    {
        private readonly GameObject lineObject;
        private readonly Image lineImage;
        private readonly GameObject ellipsisObject;
        private readonly TMP_Text ellipsisText;

        public ConnectorSlot(GameObject line, GameObject ellipsis)
        {
            lineObject = line;
            lineImage = line != null ? line.GetComponent<Image>() : null;
            ellipsisObject = ellipsis;
            ellipsisText = ellipsis != null ? ellipsis.GetComponent<TMP_Text>() : null;
        }

        public void Show(bool useEllipsis, Color color)
        {
            lineObject.SetActive(!useEllipsis);
            ellipsisObject.SetActive(useEllipsis);

            if (lineImage != null)
                lineImage.color = color;
            if (ellipsisText != null)
                ellipsisText.color = color;
        }

        public void Hide()
        {
            lineObject.SetActive(false);
            ellipsisObject.SetActive(false);
        }
    }
}
