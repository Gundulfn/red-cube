using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager instance;
    private Dictionary<string, ObjectPool> objectPools = new Dictionary<string, ObjectPool>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(InitObjectPools());
    }

    IEnumerator InitObjectPools()
    {
        InitializeObjectPool();
        yield return new WaitForSeconds(5f);

        // To set player ready and let it fall to earth :)
        GetComponent<CharacterController>().enabled = true;
    }

    public GameObject GetObjFromPool(string objName)
    {
        if (!objectPools.ContainsKey(objName))
        {
            return null;
        }

        GameObject obj = objectPools[objName].Dequeue();

        if (obj != null)
        {
            obj.SetActive(true);
        }

        return obj;
    }

    public void AddObjToPool(string prefabName, GameObject obj)
    {
        if (!objectPools.ContainsKey(prefabName))
        {
            return;
        }

        objectPools[prefabName].objectPool.Enqueue(obj);
    }

    private void InitializeObjectPool()
    {
        int chunkSize = ChunkManager.getChunkSize();
        int renderDistance = ChunkManager.getRenderDistance();

        int POOL_SIZE = (int)Mathf.Pow(chunkSize, 2) * (int)Mathf.Pow(2 * renderDistance + 2, 2);

        objectPools["dirtBlock"] = new ObjectPool("dirtBlock", POOL_SIZE);
        objectPools["lavaBlock"] = new ObjectPool("lavaBlock", 100);
        objectPools["stoneBlock"] = new ObjectPool("stoneBlock", 200);
        objectPools["concreteBlock"] = new ObjectPool("concreteBlock", 50);
        objectPools["torchBlock"] = new ObjectPool("torchBlock", 10);
        objectPools["cobblestoneBlock"] = new ObjectPool("cobblestoneBlock", 20);
        objectPools["brickBlock"] = new ObjectPool("brickBlock", 50);
        objectPools["tileBlock"] = new ObjectPool("tileBlock", 20);
        objectPools["woodBlock"] = new ObjectPool("woodBlock", 100);
        objectPools["sandBlock"] = new ObjectPool("sandBlock", 50);
    }
}
