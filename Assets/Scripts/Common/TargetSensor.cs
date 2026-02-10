using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TargetSensor : MonoBehaviour
{
    private readonly List<Monster> enemiesInRange = new();

    public IReadOnlyList<Monster> EnemiesInRange => enemiesInRange;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        if (other.TryGetComponent(out Monster enemy))
        {
            if (!enemiesInRange.Contains(enemy))
                enemiesInRange.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        if (other.TryGetComponent(out Monster enemy))
        {
            enemiesInRange.Remove(enemy);
        }
    }

    public Monster GetClosestEnemy(Vector3 from)
    {
        Monster closest = null;
        float minDist = float.MaxValue;

        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            if (enemiesInRange[i] == null)
            {
                enemiesInRange.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(from, enemiesInRange[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemiesInRange[i];
            }
        }

        return closest;
    }
}
