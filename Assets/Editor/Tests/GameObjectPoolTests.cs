using NUnit.Framework;
using UnityEngine;

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
}
