using NUnit.Framework;
using UnityEngine;

public sealed class GameObjectPoolLifecycleProbe : MonoBehaviour, IPoolable
{
    public int SpawnCount { get; private set; }
    public int DespawnCount { get; private set; }

    public void OnSpawn()
    {
        SpawnCount++;
    }

    public void OnDespawn()
    {
        DespawnCount++;
    }
}

public class GameObjectPoolTests
{
    [Test]
    public void DespawnAll_ReturnsEveryActiveObject()
    {
        var rootObject = new GameObject("PoolRoot");
        var prefab = new GameObject("PoolPrefab");
        prefab.AddComponent<Poolable>();

        try
        {
            var pool = new GameObjectPool(prefab, rootObject.transform);
            pool.Spawn(Vector3.zero, Quaternion.identity);
            pool.Spawn(Vector3.one, Quaternion.identity);

            Assert.That(pool.ActiveCount, Is.EqualTo(2));

            pool.DespawnAll();

            Assert.That(pool.ActiveCount, Is.Zero);
            Assert.That(pool.InactiveCount, Is.EqualTo(2));
        }
        finally
        {
            Object.DestroyImmediate(rootObject);
            Object.DestroyImmediate(prefab);
        }
    }

    [Test]
    public void SpawnAndDespawn_InvokeLifecycleExactlyOnce()
    {
        var rootObject = new GameObject("PoolRoot");
        var prefab = new GameObject("PoolPrefab");
        prefab.AddComponent<Poolable>();
        prefab.AddComponent<GameObjectPoolLifecycleProbe>();

        try
        {
            var pool = new GameObjectPool(prefab, rootObject.transform);
            Poolable instance = pool.Spawn(Vector3.zero, Quaternion.identity);
            var probe = instance.GetComponent<GameObjectPoolLifecycleProbe>();

            Assert.That(probe.SpawnCount, Is.EqualTo(1));
            Assert.That(probe.DespawnCount, Is.Zero);
            Assert.That(pool.Despawn(instance), Is.True);
            Assert.That(probe.DespawnCount, Is.EqualTo(1));

            Assert.That(pool.Despawn(instance), Is.False);
            Assert.That(probe.DespawnCount, Is.EqualTo(1));
            Assert.That(pool.ActiveCount, Is.Zero);
            Assert.That(pool.InactiveCount, Is.EqualTo(1));
        }
        finally
        {
            Object.DestroyImmediate(rootObject);
            Object.DestroyImmediate(prefab);
        }
    }

    [Test]
    public void ReusedObject_ReceivesLifecycleForEveryRental()
    {
        var rootObject = new GameObject("PoolRoot");
        var prefab = new GameObject("PoolPrefab");
        prefab.AddComponent<Poolable>();
        prefab.AddComponent<GameObjectPoolLifecycleProbe>();

        try
        {
            var pool = new GameObjectPool(prefab, rootObject.transform);
            Poolable firstRental = pool.Spawn(Vector3.zero, Quaternion.identity);
            var probe = firstRental.GetComponent<GameObjectPoolLifecycleProbe>();

            Assert.That(pool.Despawn(firstRental), Is.True);

            Poolable secondRental = pool.Spawn(Vector3.one, Quaternion.identity);

            Assert.That(secondRental, Is.SameAs(firstRental));
            Assert.That(probe.SpawnCount, Is.EqualTo(2));
            Assert.That(probe.DespawnCount, Is.EqualTo(1));
            Assert.That(pool.ActiveCount, Is.EqualTo(1));
            Assert.That(pool.InactiveCount, Is.Zero);
        }
        finally
        {
            Object.DestroyImmediate(rootObject);
            Object.DestroyImmediate(prefab);
        }
    }
}
