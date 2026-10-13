using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class MonsterPathPreview : MonoBehaviour
{
    private const float RefreshInterval = 0.12f;
    private const float SampleDistance = 1.5f;

    [Header("Preconfigured Path Lines")]
    [SerializeField] private List<LineRenderer> lines = new();
    private readonly HashSet<int> spawnIndices = new();
    private NavMeshPath candidatePath;
    private NavMeshPath bestPath;

    private Transform[] spawnPoints;
    private UnitRoster unitRoster;
    private float nextRefreshAt;
    private bool visible;

    public void Show(WaveData wave, Transform[] points, UnitRoster roster)
    {
        Hide();
        EnsurePaths();
        spawnPoints = points;
        unitRoster = roster;
        CollectSpawnIndices(wave);
        if (spawnIndices.Count > lines.Count)
            Debug.LogError($"[{nameof(MonsterPathPreview)}] Path line capacity is {lines.Count}, but {spawnIndices.Count} paths were requested.", this);
        visible = true;
        nextRefreshAt = 0f;
        RefreshPaths();
    }

    public void Hide()
    {
        visible = false;
        spawnPoints = null;
        unitRoster = null;
        spawnIndices.Clear();
        SetAllLinesActive(false);
    }

    private void Update()
    {
        if (!visible || Time.unscaledTime < nextRefreshAt)
            return;

        nextRefreshAt = Time.unscaledTime + RefreshInterval;
        RefreshPaths();
    }

    private void RefreshPaths()
    {
        EnsurePaths();

        if (spawnPoints == null || unitRoster == null || unitRoster.RegisteredCount == 0)
        {
            SetAllLinesActive(false);
            return;
        }

        int lineIndex = 0;
        foreach (int spawnIndex in spawnIndices)
        {
            if (lineIndex >= lines.Count)
                break;
            LineRenderer line = lines[lineIndex++];
            if (line == null)
                continue;
            if (spawnIndex < 0 || spawnIndex >= spawnPoints.Length || spawnPoints[spawnIndex] == null ||
                !TryFindClosestUnitPath(spawnPoints[spawnIndex].position, unitRoster, bestPath))
            {
                line.gameObject.SetActive(false);
                continue;
            }

            line.gameObject.SetActive(true);
            line.positionCount = bestPath.corners.Length;
            line.SetPositions(bestPath.corners);
            float length = CalculateLength(bestPath.corners);
            line.textureScale = new Vector2(Mathf.Max(1f, length * 2.5f), 1f);
        }

        for (; lineIndex < lines.Count; lineIndex++)
            lines[lineIndex].gameObject.SetActive(false);
    }

    private bool TryFindClosestUnitPath(Vector3 spawnPosition, UnitRoster roster, NavMeshPath destination)
    {
        if (roster == null || destination == null)
            return false;

        EnsurePaths();
        if (!TrySample(spawnPosition, out Vector3 sampledSpawn))
            return false;

        float shortestLength = float.PositiveInfinity;
        Vector3[] shortestCorners = null;

        foreach (UnitController unit in roster.Units)
        {
            if (unit == null || unit.IsDead || !TrySample(unit.transform.position, out Vector3 sampledUnit))
                continue;

            candidatePath.ClearCorners();
            if (!NavMesh.CalculatePath(sampledSpawn, sampledUnit, NavMesh.AllAreas, candidatePath) ||
                candidatePath.status != NavMeshPathStatus.PathComplete || candidatePath.corners.Length < 2)
                continue;

            float length = CalculateLength(candidatePath.corners);
            if (length >= shortestLength)
                continue;

            shortestLength = length;
            shortestCorners = (Vector3[])candidatePath.corners.Clone();
        }

        if (shortestCorners == null)
            return false;

        destination.ClearCorners();
        return NavMesh.CalculatePath(shortestCorners[0], shortestCorners[^1], NavMesh.AllAreas, destination) &&
               destination.status == NavMeshPathStatus.PathComplete;
    }

    private void EnsurePaths()
    {
        candidatePath ??= new NavMeshPath();
        bestPath ??= new NavMeshPath();
    }

    private static bool TrySample(Vector3 position, out Vector3 sampled)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, SampleDistance, NavMesh.AllAreas))
        {
            sampled = hit.position;
            return true;
        }

        sampled = default;
        return false;
    }

    private void CollectSpawnIndices(WaveData wave)
    {
        if (wave?.subWaves == null)
            return;

        foreach (SubWaveData subWave in wave.subWaves)
        {
            if (subWave?.spawnEntries == null)
                continue;

            foreach (MonsterSpawnEntry entry in subWave.spawnEntries)
            {
                if (entry != null && entry.count > 0)
                    spawnIndices.Add(entry.spawnPointIndex);
            }
        }
    }

    private void SetAllLinesActive(bool active)
    {
        foreach (LineRenderer line in lines)
        {
            if (line != null)
                line.gameObject.SetActive(active);
        }
    }

    private static float CalculateLength(Vector3[] corners)
    {
        float length = 0f;
        for (int i = 1; i < corners.Length; i++)
            length += Vector3.Distance(corners[i - 1], corners[i]);
        return length;
    }

}
