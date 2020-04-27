using System.Threading;
using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
    private const int CHUNK_SIZE = 16;
    private const int RENDER_DISTANCE = 1;

    private string currentChunk = "";
    private string prevChunk = "";

    private Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();

    private Queue<ChunkThreadInfo<Chunk>> chunkThreadInfoQueue = new Queue<ChunkThreadInfo<Chunk>>();

    private Transform playerTransform;

    void Start()
    {
        if (!this.isLocalPlayer)
            return;

        if (this.isServer)
        {
            RegisterServerHandlers();
        }
        else
        {
            RegisterClientHandlers();
        }
    }

    void Update()
    {
        if (!this.isLocalPlayer || !GetComponent<CharacterController>().enabled)
            return;

        SetPlayerReady();

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

        if (currentChunk != prevChunk)
        {
            List<string> nearChunks = new List<string>();
            findNearChunks(nearChunks, currentChunk);

            // Destroy past chunks
            foreach (string chunkName in chunks.Keys)
            {
                if (chunkName != currentChunk && !nearChunks.Contains(chunkName))
                {
                    chunks[chunkName].DestroyChunk();
                }
            }

            if (!chunks.ContainsKey(currentChunk) || !chunks[currentChunk].rendered)
            {
                nearChunks.Add(currentChunk);
            }

            // Spawn new chunks
            nearChunks.ForEach(chunkName =>
            {
                if (!chunks.ContainsKey(chunkName))
                {
                    if (this.isServer)
                    {
                        // Calculate chunk
                        CreateChunk(OnChunkCreated, chunkName);
                    }
                    else
                    {
                        // Ask chunk info to server
                        RequestChunkDataFromServer(chunkName);
                    }
                }
                else if (!chunks[chunkName].rendered)
                {
                    chunks[chunkName].SpawnChunk();
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

        if (chunks[msg.name].rendered)
        {
            if (msg.blockId != 0)
            {
                chunks[msg.name].SpawnBlock(msg.index, msg.blockId);
            }
            else
            {
                chunks[msg.name].DestroyBlock(msg.index);
                UpdateChunkRenderMap(msg.index, msg.name);
            }
        }
    }

    public void OnChunkUpdateRequestMessage(NetworkConnection conn, ChunkUpdateRequestMessage msg)
    {
        if (chunks[msg.name].rendered && msg.blockId != 0)
        {
            chunks[msg.name].SpawnBlock(msg.index, msg.blockId);
        }
        else
        {
            chunks[msg.name].DestroyBlock(msg.index);
            UpdateChunkRenderMap(msg.index, msg.name);
        }

        ChunkUpdateMessage updateMsg = new ChunkUpdateMessage(msg.name, msg.index, msg.blockId);

        NetworkServer.SendToAll(updateMsg);
    }

    public void SendChunkUpdateRequestMessage(Vector3 pos, byte blockId = 0)
    {
        ChunkUpdateRequestMessage msg = new ChunkUpdateRequestMessage();

        msg.name = findChunkName(pos.x, pos.z);
        msg.index = chunks[msg.name].findBlockIndex(pos.x, pos.y, pos.z); ;
        msg.blockId = blockId;

        NetworkClient.Send(msg);
    }

    public void OnChunkRequestMessage(NetworkConnection conn, ChunkRequestMessage msg)
    {
        if (chunks.ContainsKey(msg.name))
        {
            ChunkMessage chunkMessage = new ChunkMessage(chunks[msg.name].name, chunks[msg.name].blockMap, chunks[msg.name].renderMap);

            NetworkServer.SendToClientOfPlayer(conn.identity, chunkMessage);
        }
        else
        {
            Chunk chunk = new Chunk(msg.name, CHUNK_SIZE);
            calculateChunk(chunk);

            chunks[msg.name] = chunk;

            ChunkMessage chunkMessage = new ChunkMessage(chunk.name, chunk.blockMap, chunk.renderMap);

            NetworkServer.SendToClientOfPlayer(conn.identity, chunkMessage);
        }
    }

    public void OnChunkMessage(NetworkConnection conn, ChunkMessage msg)
    {
        Chunk chunk = new Chunk(msg.name, msg.blockMap, msg.renderMap, CHUNK_SIZE);
        chunks[chunk.name] = chunk;
        chunk.SpawnChunk();
    }

    public void RequestChunkDataFromServer(string chunkName)
    {
        ChunkRequestMessage crm = new ChunkRequestMessage(chunkName);

        NetworkClient.Send(crm);
    }

    [Server]
    void CreateChunk(Action<Chunk> callback, string _chunkName)
    {
        ThreadStart threadStart = delegate
        {
            Chunk newChunk = new Chunk(_chunkName, CHUNK_SIZE);
            calculateChunk(newChunk);

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
        chunk.SpawnChunk();
    }

    void calculateChunk(Chunk chunk)
    {
        int i = 0;
        int prevY = -1;

        for (int x = chunk.initialX; x < chunk.initialX + CHUNK_SIZE; x++)
        {
            for (int z = chunk.initialZ; z < chunk.initialZ + CHUNK_SIZE; z++)
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
    }

    int calculateSurfaceCoordinates(int x, int z)
    {
        float xCoord = (float)x / CHUNK_SIZE;
        float zCoord = (float)z / CHUNK_SIZE;

        return Mathf.RoundToInt(Mathf.PerlinNoise(xCoord, zCoord) * CHUNK_SIZE);
    }

    void findNearChunks(List<string> chunkNames, string chunkName)
    {
        string[] coordinates = chunkName.Split('C');
        int initialX = int.Parse(coordinates[0]);
        int initialZ = int.Parse(coordinates[1]);

        int x = 0;
        int z = 0;

        for (int i = -1 * RENDER_DISTANCE; i <= RENDER_DISTANCE; i++)
        {
            for (int w = -1 * RENDER_DISTANCE; w <= RENDER_DISTANCE; w++)
            {
                // Current chunk
                if (i == 0 && w == 0)
                    continue;

                x = initialX + i * CHUNK_SIZE;
                z = initialZ + w * CHUNK_SIZE;

                chunkNames.Add(x.ToString() + "C" + z.ToString());
            }
        }
    }

    // To update chunk renderMap and get if there is a block from other chunk to render
    private void UpdateChunkRenderMap(int index, string chunkName)
    {
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

                    int nIndex = chunks[chunkName].findBlockIndex((x + initialX + i), (y + j), (z + initialZ + k));

                    // Check if index is from other chunk
                    string indexChunkName = findChunkName(x + initialX + i, z + initialZ + k);

                    if (indexChunkName != chunkName)
                    {
                        Chunk chunk = chunks[indexChunkName];

                        if (chunk.renderMap[nIndex] == 0 && chunk.blockMap[nIndex] != 0)
                        {
                            chunk.SpawnBlock(nIndex, chunk.blockMap[nIndex]);
                        }

                        continue;
                    }

                    // Check if nIndex is not out of range of renderMap
                    if (chunks[chunkName].renderMap.Length > nIndex && nIndex >= 0)
                    {
                        // Remaining blocks
                        if (chunks[chunkName].renderMap[nIndex] == 1)
                        {
                            continue;
                        }

                        if (chunks[chunkName].renderMap[nIndex] == 0 && chunks[chunkName].blockMap[nIndex] != 0)
                        {
                            Debug.Log(chunkName +" " + nIndex);
                            Vector3 pos = new Vector3(x + initialX + i, y + j, z + initialZ + k);
                            SendChunkUpdateRequestMessage(pos, chunks[chunkName].blockMap[nIndex]);
                            continue;
                        }
                    }
                }
            }
        }
    }

    private string findChunkName(float x, float z)
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

    public static int getChunkSize()
    {
        return CHUNK_SIZE;
    }

    public static int getRenderDistance()
    {
        return RENDER_DISTANCE;
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
    }
}