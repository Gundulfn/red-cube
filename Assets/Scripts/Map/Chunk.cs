using UnityEngine;

public class Chunk
{
    public bool rendered = false;
    public string name;
    public byte[] blockMap;
    public byte[] renderMap;
    public GameObject[] gameObjects;

    private int chunkSize;

    public int initialX;
    public int initialZ;

    private int _volume;

    public Chunk(string name, int chunkSize)
    {
        this.name = name;
        this.chunkSize = chunkSize;

        string[] coordinates = name.Split('C');

        initialX = int.Parse(coordinates[0]);
        initialZ = int.Parse(coordinates[1]);

        _volume = (int)Mathf.Pow(chunkSize, 3);

        gameObjects = new GameObject[_volume];
        blockMap = new byte[_volume];
        renderMap = new byte[_volume];
    }

    public Chunk(string name, byte[] blockMap, byte[] renderMap, int chunkSize)
    {
        this.name = name;
        this.blockMap = blockMap;
        this.renderMap = renderMap;
        this.chunkSize = chunkSize;

        string[] coordinates = name.Split('C');

        initialX = int.Parse(coordinates[0]);
        initialZ = int.Parse(coordinates[1]);

        _volume = (int)Mathf.Pow(chunkSize, 3);

        gameObjects = new GameObject[_volume];
    }

    #region Chunk Functions

    public void SpawnChunk()
    {
        rendered = true;

        // Instantiate Chunk
        for (int i = 0; i < _volume; i++)
        {
            if (blockMap[i] != 0 && renderMap[i] != 0)
            {
                string prefabName = ItemDatabase.instance.GetItemById(blockMap[i]).prefabName;
                Vector3 pos = findPos(i);
                
                GameObject obj = ObjectPoolManager.instance.GetObjFromPool(prefabName);

                if(obj == null)
                    continue;

                obj.transform.position = pos;
                gameObjects[i] = obj;
            }
        }
    }

    public void DestroyChunk()
    {
        rendered = false;

        for (int i = 0; i < _volume; i++)
        {
            GameObject obj = gameObjects[i];

            if (obj == null)
                continue;

            byte prevBlockId = blockMap[i];
            string prefabName = ItemDatabase.instance.GetItemById(prevBlockId).prefabName;

            obj.SetActive(false);
            ObjectPoolManager.instance.AddObjToPool(prefabName, obj);
            gameObjects[i] = null;
        }
    }

    #endregion

    #region Block Functions

    public void SpawnBlock(int index, byte blockId)
    {
        if (index < _volume)
        {
            blockMap[index] = blockId;
            renderMap[index] = 1;

            Vector3 pos = findPos(index);
            string prefabName = ItemDatabase.instance.GetItemById(blockId).prefabName;

            GameObject obj = ObjectPoolManager.instance.GetObjFromPool(prefabName);

            if(obj == null)
                return;

            obj.transform.position = pos;
            gameObjects[index] = obj;
        }
        else
        {
            Debug.Log("CAnt SPAWn");
        }
    }

    public void DestroyBlock(int index)
    {
        if (index < _volume)
        {
            byte prevBlockId = blockMap[index];
            Block block = (Block)ItemDatabase.instance.GetItemById(prevBlockId);

            if (block == null)
                return;

            blockMap[index] = 0;
            string prefabName = block.prefabName;
            GameObject obj = gameObjects[index];

            if(obj == null)
                return;

            obj.SetActive(false);
            ObjectPoolManager.instance.AddObjToPool(prefabName, obj);

            gameObjects[index] = null;
        }
        else
        {
            Debug.Log("CAnt destroy");
        }
    }
    #endregion

    public int findBlockIndex(float x, float y, float z)
    {
        int localX = (int)(x / chunkSize) * chunkSize;
        int localY = (int)(y / chunkSize) * chunkSize;
        int localZ = (int)(z / chunkSize) * chunkSize;

        if (z < 0 && z % chunkSize != 0)
        {
            localZ -= chunkSize;
        }

        if (x < 0 && x % chunkSize != 0)
        {
            localX -= chunkSize;
        }

        int index = ((int)x - localX) * (int)Mathf.Pow(chunkSize, 2) + ((int)z - localZ) * chunkSize + (int)y;

        return index;
    }

    public Vector3 findPos(int index)
    {
        // Find position from index
        int x = index / (int)Mathf.Pow(chunkSize, 2);
        int y = index % (chunkSize);
        int z = (index % (int)Mathf.Pow(chunkSize, 2)) / (chunkSize);

        return new Vector3(x + initialX, y, z + initialZ);
    }
}