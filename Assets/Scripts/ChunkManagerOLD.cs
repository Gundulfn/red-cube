// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Mirror;

// public class ChunkOLD
// {
//     public string chunkName;
//     public byte[] blocks;
//     public byte[] renderMap;

//     public Chunk(string chunkName, byte[] blocks, byte[] renderMap)
//     {
//         this.chunkName = chunkName;
//         this.blocks = blocks;
//         this.renderMap = renderMap;
//     }

//     public Chunk() { }

//     public List<Vector3> UpdateRenderMap(int x, int y, int z, int chunkLimit)
//     {
//         List<Vector3> positions = new List<Vector3>();

//         int index = findBlockIndex(x, y, z, chunkLimit);

//         for (int i = -1; i <= 1; i++)
//         {
//             for (int j = -1; j <= 0; j++)
//             {
//                 for (int k = -1; k <= 1; k++)
//                 {
//                     // Corner block          
//                     int m = i * j * k;
//                     if (m != 0)
//                         continue;

//                     // Current block
//                     if (i == 0 && j == 0 & k == 0)
//                         continue;

//                     if (x + i < 0 || y + j < 0 || z + k < 0)
//                         continue;

//                     int nIndex = findBlockIndex(x + i, y + j, z + k, chunkLimit);

//                     // Remaining blocks
//                     if (renderMap[nIndex] == 1)
//                     {
//                         continue;
//                     }

//                     if (renderMap[nIndex] == 0 && blocks[nIndex] == 1)
//                     {
//                         positions.Add(new Vector3(x + i, y + j, z + k));
//                         renderMap[nIndex] = 1;
//                         continue;
//                     }
//                 }
//             }
//         }

//         return positions;
//     }

//     public int findBlockIndex(int x, int y, int z, int _chunkLimit)
//     {
//         //int index = x * 64 + (z + 1) * _y;
//         int index = x * (int)Mathf.Pow(_chunkLimit, 2) + z * _chunkLimit + y;

//         return index;
//     }

//     public int findNextChunkBlock(int x, int y, int z, int _chunkLimit)
//     {
//         string[] chunkElements = chunkName.Split('C');
//         int initialX = int.Parse(chunkElements[0]);
//         int initialZ = int.Parse(chunkElements[1]);

//         x = (x != -1 ? 0 : -1);
//         z = (z != -1 ? 0 : -1);

//         int nextChunkX = x * _chunkLimit + initialX;
//         int nextChunkZ = z * _chunkLimit + initialZ;
//         string nextChunkName = nextChunkX.ToString() + "C" + nextChunkZ.ToString();

//         //Chunk nextChunk = ChunkManager.Ge

//         return 0;
//     }
// }

// public class ChunkManagerOLD : NetworkBehaviour
// {
//     public static ChunkManagerOLD instance;
//     public GameObject cubePrefab;

    // public string currentChunk = "0C0";
    // public string prevChunk = "0C0";

//     private const int CHUNK_LIMIT = 32;
//     private const int RENDER_DISTANCE = 2;

//     private Vector2 startPos = Vector2.zero;

//     private List<string> chunks = new List<string>();
//     private Transform playerTransform;

//     // Memory for all calculated chunks before
//     private Dictionary<string, Chunk> generatedChunks = new Dictionary<string, Chunk>();

//     private Dictionary<string, Queue<GameObject>> blockObjectDict = new Dictionary<string, Queue<GameObject>>();

//     void Awake()
//     {
//         instance = this;
//     }

//     void Start()
//     {
        // Queue<GameObject> blockPool = new Queue<GameObject>();
        // int POOL_SIZE = (int)Mathf.Pow(CHUNK_LIMIT, 2) * (int)Mathf.Pow(2 * RENDER_DISTANCE + 1, 2);

        // for (int i = 0; i < POOL_SIZE; i++)
        // {
        //     GameObject block = Instantiate(cubePrefab);
        //     block.transform.parent = transform;
        //     //block.SetActive(false);
        //     NetworkServer.Spawn(block);
        //     blockPool.Enqueue(block);
        // }

        // blockObjectDict.Add(cubePrefab.name, blockPool);
//     }

    // public GameObject SpawnFromDict(string blockName, Vector3 pos)
    // {
    //     if (!blockObjectDict.ContainsKey(blockName))
    //     {
    //         return null;
    //     }

    //     GameObject block = blockObjectDict[blockName].Dequeue();
    //     block.SetActive(true);
    //     block.transform.position = pos;
    //     return block;
    // }

    // public void SetPlayerTransform(Transform t)
    // {
    //     playerTransform = GameObject.FindWithTag("Player").GetComponent<Transform>();

    //     float x = playerTransform.position.x;
    //     float z = playerTransform.position.z;

    //     x = Mathf.RoundToInt(x / CHUNK_LIMIT);
    //     z = Mathf.RoundToInt(z / CHUNK_LIMIT);

    //     string chunkName = (x * CHUNK_LIMIT).ToString() + "C" + (z * CHUNK_LIMIT).ToString();
    //     SpawnChunk(chunkName);

    //     findNearChunks(chunkName).ForEach(_chunkName =>
    //     {
    //         SpawnChunk(_chunkName);
    //     });
    // }

//     void Update()
//     {
        // if (playerTransform == null)
        //     return;

        // float x = playerTransform.position.x;
        // float z = playerTransform.position.z;

        // x = Mathf.RoundToInt(x / CHUNK_LIMIT);
        // z = Mathf.RoundToInt(z / CHUNK_LIMIT);

        // string chunkName = (x * CHUNK_LIMIT).ToString() + "C" + (z * CHUNK_LIMIT).ToString();

        // currentChunk = chunkName;

        // // currentChunk = chunkName;

        // if (currentChunk != prevChunk)
        // {
        //     List<string> nearChunks = findNearChunks(currentChunk);

        //     // Destroy past chunks
        //     chunks.ForEach(chunk =>
        //     {
        //         if (chunk != currentChunk && !nearChunks.Contains(chunk))
        //         {
        //             DestroyChunk(chunk);
        //         }
        //     });

        //     // Spawn new chunks
        //     nearChunks.ForEach(_chunkName =>
        //     {
        //         if (!chunks.Contains(_chunkName))
        //         {
        //             SpawnChunk(_chunkName);
        //         }
        //     });

        //     prevChunk = currentChunk;
        // }
//     }

    // int calculateSurfaceCoordinates(int x, int z)
    // {
    //     float xCoord = (float)x / CHUNK_LIMIT;
    //     float zCoord = (float)z / CHUNK_LIMIT;

    //     return Mathf.RoundToInt(Mathf.PerlinNoise(xCoord, zCoord) * CHUNK_LIMIT);
    // }

//     public void DestroyBlock(GameObject obj)
//     {
//         Vector3 pos = obj.transform.localPosition;
//         float x = pos.x;
//         float z = pos.z;

//         x = Mathf.RoundToInt(x / CHUNK_LIMIT);
//         z = Mathf.RoundToInt(z / CHUNK_LIMIT);

//         string chunkName = (x * CHUNK_LIMIT).ToString() + "C" + (z * CHUNK_LIMIT).ToString();

//         List<Vector3> blocksToGenerate = generatedChunks[chunkName].UpdateRenderMap((int)pos.x, (int)pos.y, (int)pos.z, CHUNK_LIMIT);
//         obj.transform.parent = transform;
//         obj.SetActive(false);
//         blockObjectDict[cubePrefab.name].Enqueue(obj);

//         GameObject chunkObj = GameObject.Find(chunkName);

        // foreach (Vector3 blockPos in blocksToGenerate) {
        //     GameObject block = SpawnFromDict(cubePrefab.name, blockPos);
        //     block.transform.parent = chunkObj.transform;
        // }

//     }

//     public void SpawnBlock(string chunkName)
//     {
//         GameObject chunkObj = GameObject.Find(chunkName);

//         for (int i = 0; i < generatedChunks[chunkName].renderMap.Length; i++)
//         {
//             if (generatedChunks[chunkName].renderMap[i] == 1)
//             {
//                 GameObject block = blockObjectDict[cubePrefab.name].Dequeue();
//                 block.transform.parent = chunkObj.transform;
//             }
//         }
//     }

    // List<string> findNearChunks(string chunkName)
    // {
    //     List<string> chunkNames = new List<string>();

    //     string[] coordinates = chunkName.Split('C');
    //     int initialX = int.Parse(coordinates[0]);
    //     int initialZ = int.Parse(coordinates[1]);

    //     for (int i = -1 * RENDER_DISTANCE; i <= RENDER_DISTANCE; i++)
    //     {
    //         for (int w = -1 * RENDER_DISTANCE; w <= RENDER_DISTANCE; w++)
    //         {
    //             // Current chunk
    //             if (i == 0 && w == 0)
    //                 continue;

    //             int x = initialX + i * CHUNK_LIMIT;
    //             int z = initialZ + w * CHUNK_LIMIT;

    //             chunkNames.Add(x.ToString() + "C" + z.ToString());
    //         }
    //     }

    //     return chunkNames;
    // }

    // void DestroyChunk(string chunkName)
    // {
    //     GameObject chunkObj = GameObject.Find(chunkName);
    //     int blockCount = chunkObj.transform.childCount;

    //     for (int i = 0; i < blockCount; i++)
    //     {
    //         GameObject block = chunkObj.transform.GetChild(0).gameObject;
    //         block.transform.parent = transform;
    //         block.SetActive(false);

    //         blockObjectDict[cubePrefab.name].Enqueue(block);
    //     }

    //     chunks.Remove(chunkName);
    //     //Destroy(chunkObj);
    // }

    // void SpawnChunk(string chunkName)
    // {
    //     string[] coordinates = chunkName.Split('C');
    //     int initialX = int.Parse(coordinates[0]);
    //     int initialZ = int.Parse(coordinates[1]);

    //     GameObject chunkObj = Instantiate(new GameObject(), new Vector3(initialX, 0, initialZ), Quaternion.identity);
    //     chunkObj.name = chunkName;

    //     Chunk chunk;

    //     // If chunk calculated before get it from memory
    //     if (generatedChunks.ContainsKey(chunkName))
    //     {
    //         chunk = generatedChunks[chunkName];
    //     }
    //     else
    //     {
    //         chunk = calculateChunk(initialX, initialZ);
    //         //blockArr = calculateChunk(initialX, initialZ);
    //     }

    //     // Instantiate Chunk
    //     int AREA = CHUNK_LIMIT * (CHUNK_LIMIT);
    //     int H = (int)Mathf.Pow(CHUNK_LIMIT, 3);

    //     for (int i = 0; i < H; i++)
    //     {
    //         if (chunk.blocks[i] == 1 && chunk.renderMap[i] != 0)
    //         {
    //             int x = i / AREA;
    //             int y = i % (CHUNK_LIMIT);
    //             int z = (i % AREA) / (CHUNK_LIMIT);

    //             Vector3 pos = new Vector3(x + initialX, y, z + initialZ);
    //             GameObject block = SpawnFromDict(cubePrefab.name, pos);
    //             block.transform.parent = chunkObj.transform;
    //         }
    //     }

    //     // StaticBatchingUtility.Combine(chunkObj);

    //     // Save chunk to memory
    //     generatedChunks[chunkName] = chunk;

    //     chunks.Add(chunkName);
    // }

    // Chunk calculateChunk(int initialX, int initialZ)
    // {
    //     Chunk chunk = new Chunk();

    //     chunk.chunkName = initialX.ToString() + "C" + initialZ.ToString();

    //     chunk.blocks = new byte[(int)Mathf.Pow(CHUNK_LIMIT, 3)];
    //     chunk.renderMap = new byte[(int)Mathf.Pow(CHUNK_LIMIT, 3)];

    //     int i = 0;

    //     for (int x = initialX; x < initialX + CHUNK_LIMIT; x++)
    //     {
    //         for (int z = initialZ; z < initialZ + CHUNK_LIMIT; z++)
    //         {
    //             int y = calculateSurfaceCoordinates(x, z);

    //             for (int _y = 0; _y < CHUNK_LIMIT; _y++)
    //             {
    //                 if (_y >= y)
    //                     chunk.renderMap[i] = 1;

    //                 if (_y <= y)
    //                     chunk.blocks[i] = 1;

    //                 i++;
    //             }
    //         }
    //     }

    //     return chunk;
    // }

//     // public Chunk GetGeneratedChunk(string chunkName)
//     // {
//     //     if(generatedChunks.ContainsKey(chunkName))
//     //     {
//     //         return generatedChunks[chunkName];
//     //     }

//     //     return null;
//     // }
// }
