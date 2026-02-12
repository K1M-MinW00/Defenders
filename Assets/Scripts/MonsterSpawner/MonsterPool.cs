//using UnityEngine;
//using System.Collections.Generic;

//public class MonsterPool : MonoBehaviour
//{
//    private Dictionary<string, Queue<GameObject>> pool = new();
//    private Dictionary<string, GameObject> prefabCache = new();

//    public void Init(StageData stageData)
//    {
//        HashSet<string> requiredMonsterIds = CollectMonsterIds(stageData);

//        foreach (var monsterId in requiredMonsterIds)
//            Prewarm(monsterId, 20);
//    }

//    private HashSet<string> CollectMonsterIds(StageData stageData)
//    {
//        HashSet<string> ids = new();

//        foreach(var wave in stageData.waves)
//        {
//            foreach(var sub in wave.subWaves)
//            {
//                foreach (var spawnGroup in sub.spawnGroups)
//                    ids.Add(spawnGroup.monsterId);
//            }
//        }

//        return ids;
//    }

//    private void PreWarm(string monsterId,int count)
//    {
//        if(!pool.ContainsKey(monsterId))
//            pool[monsterId] = new Queue<GameObject>();

//        // GameObject obj = 
//    }

//    public GameObject Spawn(string monsterId, Vector3 position)
//    {
//        if (!pool.ContainsKey(monsterId))
//        {
//            Debug.LogError($"MonsterPool: No pool for {monsterId}");
//            return null;
//        }

//        GameObject obj = pool[monsterId].Count > 0
//            ? pool[monsterId].Dequeue()
//            : Instantiate(GetPrefab(monsterId), transform);

//        obj.transform.position = position;
//        obj.SetActive(true);
//        return obj;
//    }

//    public void Despawn(string monsterId, GameObject obj)
//    {
//        obj.SetActive(false);
//        pool[monsterId].Enqueue(obj);
//    }

//    private GameObject GetPrefab(string monsterId)
//    {
//        foreach (var item in poolItems)
//        {
//            if (item.monsterId == monsterId)
//                return item.prefab;
//        }
//        return null;
//    }
//}
