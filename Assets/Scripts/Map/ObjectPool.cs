using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    public string prefabName;
    public int poolSize;
    public Queue<GameObject> objectPool;
    private GameObject prefab;

    public ObjectPool(string prefabName, int poolSize)
    {
        this.poolSize = poolSize;
        this.prefabName = prefabName;

        InitializeObjectPool();
    }

    private void InitializeObjectPool()
    {
        prefab = (GameObject)Resources.Load("Prefabs/Items/" + prefabName);

        if (prefab == null)
        {
            Debug.Log("ERR: Resource " + prefabName + " not found!");
        }

        objectPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject block = GameObject.Instantiate(prefab);
            objectPool.Enqueue(block);
        }
    }

    public GameObject Dequeue()
    {
        if (objectPool.Count <= poolSize / 4)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject block = GameObject.Instantiate(prefab);
                objectPool.Enqueue(block);
            }

            poolSize = poolSize * 2;
        }

        if (objectPool.Count == 0)
        {
            Debug.Log(prefabName + " pool is empty.");
            return null;
        }

        return objectPool.Dequeue();
    }
}