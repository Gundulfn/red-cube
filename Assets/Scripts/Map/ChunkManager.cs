using System.Threading;
using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

public struct ChunkThreadInfo<T>
{
    public readonly Action<T> callback;
    public readonly T chunk;

    public ChunkThreadInfo(Action<T> callback, T chunk)
    {
        this.callback = callback;
        this.chunk = chunk;
    }
}

public class ChunkManager : NetworkBehaviour
{
    private int CHUNK_SIZE = 16;
    private int RENDER_DISTANCE = 1;

    private Transform playerTransform;
    private string currentChunk = "";
    private string prevChunk = "";
    private Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();

    private Queue<ChunkThreadInfo<Chunk>> chunkThreadInfoQueue = new Queue<ChunkThreadInfo<Chunk>>();

    private Dictionary<string, ObjectPool> objectPools = new Dictionary<string, ObjectPool>();

    private bool objectPoolsLoaded = false;

    void Start()
    {
        if (!this.isLocalPlayer)
            return;

        StartCoroutine(InitObjectPoolds());

        if (this.isServer)
        {
            RegisterServerHandlers();
        }
        else
        {
            RegisterClientHandlers();
        }
    }

    IEnumerator InitObjectPoolds()
    {
        InitializeObjectPool();
        yield return new WaitForSeconds(5f);
        objectPoolsLoaded = true;
        SetPlayerReady();
    }

    void Update()
    {
        if (!this.isLocalPlayer || !objectPoolsLoaded)
            return;

        if (playerTransform == null)
            return;

        if (chunkThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < chunkThreadInfoQueue.Count; i++)
            {
                ChunkThreadInfo<Chunk> chunkThreadInfo = chunkThreadInfoQueue.Dequeue();

                if (chunkThreadInfo.callback == null)
                {
                    Debug.Log("Callback not found");
                    continue;
                }

                chunkThreadInfo.callback(chunkThreadInfo.chunk);
            }
        }

        float x = playerTransform.position.x;
        float z = playerTransform.position.z;

        x = Mathf.RoundToInt(x / CHUNK_SIZE);
        z = Mathf.RoundToInt(z / CHUNK_SIZE);

        string chunkName = (x * CHUNK_SIZE).ToString() + "C" + (z * CHUNK_SIZE).ToString();

        currentChunk = chunkName;

        if (currentChunk != prevChunk)
        {
            List<string> nearChunks = findNearChunks(currentChunk);

            // Destroy past chunks
            foreach (string _chunkName in chunks.Keys)
            {
                if (_chunkName != currentChunk && !nearChunks.Contains(_chunkName))
                {
                    DestroyChunk(_chunkName);
                }
            }

            if (!chunks.ContainsKey(currentChunk) || !chunks[currentChunk].rendered)
            {
                nearChunks.Add(currentChunk);
            }

            // Spawn new chunks
            nearChunks.ForEach(_chunkName =>
            {
                if (!chunks.ContainsKey(_chunkName))
                {
                    if (this.isServer)
                    {
                        // Calculate chunk
                        CreateChunk(OnChunkCreated, _chunkName);
                    }
                    else
                    {
                        // Ask chunk info to server
                        RequestChunkDataFromServer(_chunkName);
                    }
                }
                else if (!chunks[_chunkName].rendered)
                {
                    SpawnChunk(_chunkName);
                }
            });

            prevChunk = currentChunk;
        }
    }

    public void RegisterServerHandlers()
    {
        NetworkServer.RegisterHandler<ChunkRequestMessage>(OnChunkRequestMessage);
        NetworkServer.RegisterHandler<ChunkUpdateRequestMessage>(OnChunkUpdateRequestMessage);
        NetworkServer.RegisterHandler<ChunkUpdateMessage>(OnChunkUpdateMessage);
    }

    public void RegisterClientHandlers()
    {
        NetworkClient.RegisterHandler<ChunkMessage>(OnChunkMessage);
        NetworkClient.RegisterHandler<ChunkUpdateMessage>(OnChunkUpdateMessage);
    }

    public void OnChunkUpdateMessage(NetworkConnection conn, ChunkUpdateMessage msg)
    {
        if (!chunks.ContainsKey(msg.name)) return;

        chunks[msg.name].renderMap[msg.index] = 1;

        if (chunks[msg.name].rendered)
        {
            if (msg.blockId != 0)
            {
                chunks[msg.name].blockMap[msg.index] = msg.blockId;
                SpawnBlock(msg.name, msg.index, msg.blockId);
            }
            else
            {
                DestroyBlock(msg.name, msg.index);
            }
        }
    }

    public void OnChunkUpdateRequestMessage(NetworkConnection conn, ChunkUpdateRequestMessage msg)
    {
        Debug.Log("Message income");
        if (chunks[msg.name].rendered && msg.blockId != 0)
        {
            chunks[msg.name].renderMap[msg.index] = 1;
            chunks[msg.name].blockMap[msg.index] = msg.blockId;
            SpawnBlock(msg.name, msg.index, msg.blockId);
        }
        else
        {
            DestroyBlock(msg.name, msg.index);
        }

        ChunkUpdateMessage updateMsg = new ChunkUpdateMessage();

        updateMsg.name = msg.name;
        updateMsg.index = msg.index;
        updateMsg.blockId = msg.blockId;

        NetworkServer.SendToAll(updateMsg);
    }

    public void SendChunkUpdateRequestMessage(Vector3 pos, byte blockId = 0)
    {
        Debug.Log("ChunkUpdateRequestMessage");
        ChunkUpdateRequestMessage msg = new ChunkUpdateRequestMessage();

        msg.name = findChunkName(pos.x, pos.z, CHUNK_SIZE);
        msg.index = findBlockIndex(pos.x, pos.y, pos.z, CHUNK_SIZE); ;
        msg.blockId = blockId;

        NetworkClient.Send(msg);
    }

    public void SendChunkUpdateMessage(Vector3 pos, byte blockId = 0)
    {
        //Debug.Log("ChunkUpdateMessage");
        ChunkUpdateMessage msg = new ChunkUpdateMessage();

        msg.name = findChunkName(pos.x, pos.z, CHUNK_SIZE);
        msg.index = findBlockIndex(pos.x, pos.y, pos.z, CHUNK_SIZE);
        msg.blockId = blockId;

        if (blockId != 0)
        {
            SpawnBlock(msg.name, msg.index, msg.blockId);
        }
        else
        {
            DestroyBlock(msg.name, msg.index);
        }

        NetworkServer.SendToAll(msg);
    }

    public void SpawnBlock(string chunkName, int index, byte blockId)
    {

        chunks[chunkName].blockMap[index] = blockId;
        chunks[chunkName].renderMap[index] = 1;

        if (chunks[chunkName].gameObjects != null && index < chunks[chunkName].gameObjects.Length)
        {
            Vector3 pos = findPos(index, chunkName);
            string prefabName = ItemDatabase.instance.GetItemById(blockId).prefabName;

            GameObject block = SpawnFromDict(prefabName, pos);
            chunks[chunkName].gameObjects[index] = block;
        }
        else
        {
            Debug.Log("CAnt SPAWn");
        }
    }

    void DestroyBlock(string chunkName, int index)
    {
        Debug.Log("Destroying");
        if (!chunks.ContainsKey(chunkName))
            return;

        chunks[chunkName].blockMap[index] = 0;

        // To update chunk renderMap and get if there is a block from other chunk to render 
        List<string> blocksFromNextChunk = UpdateChunkRenderMap(index, chunkName, CHUNK_SIZE);

        if (blocksFromNextChunk.Count != 0)
        {
            foreach (string blockInfo in blocksFromNextChunk)
            {
                string[] infoElements = blockInfo.Split(';');
                string nextChunkName = infoElements[0];

                Chunk chunk = chunks[nextChunkName];
                int _index = int.Parse(infoElements[1]);

                if (chunk.renderMap[_index] == 0 && chunk.blockMap[_index] != 0)
                {
                    SpawnBlock(nextChunkName, _index, chunk.blockMap[_index]);
                }
            }
        }

        Debug.Log("NextChunk");
        if (chunks[chunkName].gameObjects != null && index < chunks[chunkName].gameObjects.Length)
        {
            byte prevBlockId = chunks[chunkName].blockMap[index];
            Block obj = (Block)ItemDatabase.instance.GetItemById(prevBlockId);

            Debug.Log("null object");

            if (obj == null)
                return;

            string prefabName = obj.prefabName;
            GameObject block = chunks[chunkName].gameObjects[index];
            block.SetActive(false);
            objectPools[prefabName].objectPool.Enqueue(block);
            chunks[chunkName].gameObjects[index] = null;

            Debug.Log("ended");
        }
        else
        {
            Debug.Log("CAnt destroy");
        }
    }

    public void OnChunkRequestMessage(NetworkConnection conn, ChunkRequestMessage msg)
    {
        if (chunks.ContainsKey(msg.name))
        {
            Chunk chunk = chunks[msg.name];
            ChunkMessage chunkMessage = new ChunkMessage();

            chunkMessage.name = chunk.name;
            chunkMessage.blockMap = chunk.blockMap;
            chunkMessage.renderMap = chunk.renderMap;

            NetworkServer.SendToClientOfPlayer(conn.identity, chunkMessage);
        }
        else
        {
            Chunk chunk = calculateChunk(msg.name);
            ChunkMessage chunkMessage = new ChunkMessage();

            chunks[msg.name] = chunk;

            chunkMessage.name = chunk.name;
            chunkMessage.blockMap = chunk.blockMap;
            chunkMessage.renderMap = chunk.renderMap;

            NetworkServer.SendToClientOfPlayer(conn.identity, chunkMessage);
        }
    }

    public void OnChunkMessage(NetworkConnection conn, ChunkMessage msg)
    {
        Chunk chunk = new Chunk();

        chunk.name = msg.name;
        chunk.blockMap = msg.blockMap;
        chunk.renderMap = msg.renderMap;

        chunks[chunk.name] = chunk;

        SpawnChunk(chunk.name);
    }

    public void RequestChunkDataFromServer(string chunkName)
    {
        ChunkRequestMessage crm = new ChunkRequestMessage();

        crm.name = chunkName;

        NetworkClient.Send(crm);
    }

    [Server]
    void CreateChunk(Action<Chunk> callback, string _chunkName)
    {
        ThreadStart threadStart = delegate
        {
            Chunk newChunk = calculateChunk(_chunkName);
            newChunk.name = _chunkName;
            newChunk.rendered = false;

            ChunkThreadInfo<Chunk> chunkInfo = new ChunkThreadInfo<Chunk>(callback, newChunk);

            lock (chunkThreadInfoQueue)
            {
                chunkThreadInfoQueue.Enqueue(chunkInfo);
            }
        };

        new Thread(threadStart).Start();
    }

    [Server]
    void OnChunkCreated(Chunk chunk)
    {
        chunks[chunk.name] = chunk;
        SpawnChunk(chunk.name);
    }

    Chunk calculateChunk(string chunkName)
    {
        string[] coordinates = chunkName.Split('C');
        int initialX = int.Parse(coordinates[0]);
        int initialZ = int.Parse(coordinates[1]);

        Chunk chunk = new Chunk();

        chunk.name = initialX.ToString() + "C" + initialZ.ToString();

        chunk.blockMap = new byte[(int)Mathf.Pow(CHUNK_SIZE, 3)];
        chunk.renderMap = new byte[(int)Mathf.Pow(CHUNK_SIZE, 3)];

        int i = 0;
        int prevY = -1;

        for (int x = initialX; x < initialX + CHUNK_SIZE; x++)
        {
            for (int z = initialZ; z < initialZ + CHUNK_SIZE; z++)
            {
                int y = calculateSurfaceCoordinates(x, z);

                // To detect if there is a space between y-blocks
                int extraBlock = 0;

                if (prevY != -1 && y - prevY != 1)
                {
                    extraBlock = 1;
                }
                else
                {
                    prevY = y;
                }

                for (int _y = 0; _y < CHUNK_SIZE; _y++)
                {
                    if (_y + extraBlock >= y)
                    {
                        chunk.renderMap[i] = 1;
                    }

                    if (_y <= y)
                        chunk.blockMap[i] = 1;

                    i++;
                }

            }
        }

        return chunk;
    }

    int calculateSurfaceCoordinates(int x, int z)
    {
        float xCoord = (float)x / CHUNK_SIZE;
        float zCoord = (float)z / CHUNK_SIZE;

        return Mathf.RoundToInt(Mathf.PerlinNoise(xCoord, zCoord) * CHUNK_SIZE);
    }

    List<string> findNearChunks(string chunkName)
    {
        List<string> chunkNames = new List<string>();

        string[] coordinates = chunkName.Split('C');
        int initialX = int.Parse(coordinates[0]);
        int initialZ = int.Parse(coordinates[1]);

        for (int i = -1 * RENDER_DISTANCE; i <= RENDER_DISTANCE; i++)
        {
            for (int w = -1 * RENDER_DISTANCE; w <= RENDER_DISTANCE; w++)
            {
                // Current chunk
                if (i == 0 && w == 0)
                    continue;

                int x = initialX + i * CHUNK_SIZE;
                int z = initialZ + w * CHUNK_SIZE;

                chunkNames.Add(x.ToString() + "C" + z.ToString());
            }
        }

        return chunkNames;
    }

    void DestroyChunk(string chunkName)
    {
        Chunk chunk = chunks[chunkName];
        chunks[chunkName].rendered = false;

        if (chunk.gameObjects == null)
            return;

        int j = chunk.gameObjects.Length;

        for (int i = 0; i < j; i++)
        {
            GameObject block = chunks[chunkName].gameObjects[i];

            if (block == null)
                continue;

            byte prevBlockId = chunks[chunkName].blockMap[i];
            string prefabName = ItemDatabase.instance.GetItemById(prevBlockId).prefabName;

            block.SetActive(false);
            objectPools[prefabName].objectPool.Enqueue(block);
            chunks[chunkName].gameObjects[i] = null;
        }
    }

    void SpawnChunk(string chunkName)
    {
        Chunk chunk = chunks[chunkName];
        chunk.rendered = true;

        int VOLUME = (int)Mathf.Pow(CHUNK_SIZE, 3);

        if (chunk.gameObjects == null)
            chunk.gameObjects = new GameObject[VOLUME];

        // Instantiate Chunk
        for (int i = 0; i < VOLUME; i++)
        {
            if (chunk.blockMap[i] != 0 && chunk.renderMap[i] != 0)
            {
                string prefabName = ItemDatabase.instance.GetItemById(chunk.blockMap[i]).prefabName;

                Vector3 pos = findPos(i, chunkName);
                GameObject block = SpawnFromDict(prefabName, pos);
                chunk.gameObjects[i] = block;
            }
        }
    }

    public GameObject SpawnFromDict(string blockName, Vector3 pos)
    {
        if (!objectPools.ContainsKey(blockName))
        {
            return null;
        }

        GameObject block = objectPools[blockName].Dequeue();

        if (block != null)
        {
            block.SetActive(true);
            block.transform.position = pos;
        }

        return block;
    }

    public void SetPlayerTransform(Transform t)
    {
        playerTransform = t;

        float x = playerTransform.position.x;
        float z = playerTransform.position.z;

        x = Mathf.RoundToInt(x / CHUNK_SIZE);
        z = Mathf.RoundToInt(z / CHUNK_SIZE);

        string chunkName = (x * CHUNK_SIZE).ToString() + "C" + (z * CHUNK_SIZE).ToString();

        currentChunk = chunkName;
    }

    private void InitializeObjectPool()
    {
        int POOL_SIZE = (int)Mathf.Pow(CHUNK_SIZE, 2) * (int)Mathf.Pow(2 * RENDER_DISTANCE + 2, 2);
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

    private void SetPlayerReady()
    {
        playerTransform = GetComponent<Transform>();

        float x = playerTransform.position.x;
        float z = playerTransform.position.z;

        x = Mathf.RoundToInt(x / CHUNK_SIZE);
        z = Mathf.RoundToInt(z / CHUNK_SIZE);

        string chunkName = (x * CHUNK_SIZE).ToString() + "C" + (z * CHUNK_SIZE).ToString();

        currentChunk = chunkName;

        GetComponent<CharacterController>().enabled = true;
    }

    private List<string> UpdateChunkRenderMap(int index, string chunkName, int CHUNK_SIZE)
    {
        List<string> blocksFromNextChunk = new List<string>();

        Chunk chunk = chunks[chunkName];
        int AREA = CHUNK_SIZE * CHUNK_SIZE;

        string[] coordinates = chunkName.Split('C');
        int initialX = int.Parse(coordinates[0]);
        int initialZ = int.Parse(coordinates[1]);

        // Find position from index
        int x = index / (int)Mathf.Pow(CHUNK_SIZE, 2);
        int y = index % (CHUNK_SIZE);
        int z = (index % (int)Mathf.Pow(CHUNK_SIZE, 2)) / (CHUNK_SIZE);

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    // Corner block          
                    int m = i * j * k;
                    if (m != 0)
                        continue;

                    // Current block
                    if (i == 0 && j == 0 & k == 0)
                        continue;

                    // Blocks which have two different coordinates from 0
                    int sum = i + j + k;
                    if (sum == 0 || sum == -2 || sum == 2)
                    {
                        continue;
                    }

                    int nIndex = findBlockIndex((x + initialX + i), (y + j), (z + initialZ + k), CHUNK_SIZE);

                    // Check if index is from other chunk
                    string checkName = findChunkName(x + initialX + i, z + initialZ + k, CHUNK_SIZE);
                    if (checkName != chunkName)
                    {
                        blocksFromNextChunk.Add(checkName + ";" + nIndex.ToString());
                        continue;
                    }

                    // Check if nIndex is not out of range of renderMap
                    if (chunk.renderMap.Length > nIndex && nIndex >= 0)
                    {
                        // Remaining blocks
                        if (chunk.renderMap[nIndex] == 1)
                        {
                            continue;
                        }

                        if (chunk.renderMap[nIndex] == 0 && chunk.blockMap[nIndex] != 0)
                        {
                            //SpawnBlock(chunkName, nIndex, chunk.blockMap[nIndex]);
                            Vector3 pos = new Vector3(x + initialX + i, y + j, z + initialZ + k);
                            SendChunkUpdateRequestMessage(pos, chunk.blockMap[nIndex]);
                            continue;
                        }
                    }
                }
            }
        }

        return blocksFromNextChunk;
    }

    private int findBlockIndex(float x, float y, float z, int CHUNK_SIZE)
    {
        int localX = (int)(x / CHUNK_SIZE) * CHUNK_SIZE;
        int localZ = (int)(z / CHUNK_SIZE) * CHUNK_SIZE;
        //int localY = (int)y;

        if (z < 0 && z % CHUNK_SIZE != 0)
        {
            localZ = localZ - CHUNK_SIZE;
        }

        if (x < 0 && x % CHUNK_SIZE != 0)
        {
            localX = localX - CHUNK_SIZE;
        }

        int index = ((int)x - localX) * (int)Mathf.Pow(CHUNK_SIZE, 2) + ((int)z - localZ) * CHUNK_SIZE + (int)y;

        return index;
    }

    private string findChunkName(float x, float z, int CHUNK_SIZE)
    {
        int localX = (int)(x / CHUNK_SIZE) * CHUNK_SIZE;
        int localZ = (int)(z / CHUNK_SIZE) * CHUNK_SIZE;

        if (z < 0 && z % CHUNK_SIZE != 0)
        {
            localZ = localZ - CHUNK_SIZE;
        }

        if (x < 0 && x % CHUNK_SIZE != 0)
        {
            localX = localX - CHUNK_SIZE;
        }

        return localX.ToString() + "C" + localZ.ToString();
    }

    private Vector3 findPos(int index, string chunkName)
    {
        int AREA = CHUNK_SIZE * CHUNK_SIZE;

        string[] coordinates = chunkName.Split('C');
        int initialX = int.Parse(coordinates[0]);
        int initialZ = int.Parse(coordinates[1]);

        // Find position from index
        int x = index / (int)Mathf.Pow(CHUNK_SIZE, 2);
        int y = index % (CHUNK_SIZE);
        int z = (index % (int)Mathf.Pow(CHUNK_SIZE, 2)) / (CHUNK_SIZE);

        return new Vector3(x + initialX, y, z + initialZ);
    }
}