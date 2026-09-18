using UnityEngine;

public sealed class UnityGachaRandom : IGachaRandom
{
    public float Range(float minInclusive, float maxExclusive) =>
        Random.Range(minInclusive, maxExclusive);

    public int Range(int minInclusive, int maxExclusive) =>
        Random.Range(minInclusive, maxExclusive);
}
