// using System.Threading;
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using Mirror;

// public class ChunkManager : NetworkBehaviour
// {
//     // private bool isLocalPlayer = true;
//     // private bool isServer = true;

//     private int CHUNK_SIZE = 16;
//     private int RENDER_DISTANCE = 1;

//     private Transform playerTransform;
//     private string currentChunk = "";
//     private string prevChunk = "";
//     private Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();

//     Dictionary<string, Queue<GameObject>> objectPools = new Dictionary<string, Queue<GameObject>>();
//     public GameObject cubePrefab;
//     private Queue<ChunkThreadInfo<string>> chunkThreadInfoQueue = new Queue<ChunkThreadInfo<string>>();

//     void Start()
//     {
//         if (!this.isLocalPlayer)
//             return;
        
//         InitializeObjectPool();
//     }

//     void Update()
//     {
//         if (!this.isLocalPlayer)
//             return;
        
//         if (playerTransform == null)
//             return;

//         float x = playerTransform.position.x;
//         float z = playerTransform.position.z;

//         x = Mathf.RoundToInt(x / CHUNK_SIZE);
//         z = Mathf.RoundToInt(z / CHUNK_SIZE);

//         string chunkName = (x * CHUNK_SIZE).ToString() + "C" + (z * CHUNK_SIZE).ToString();

//         currentChunk = chunkName;

//         if (currentChunk != prevChunk)
//         {
//             List<string> nearChunks = findNearChunks(currentChunk);

//             if(chunkThreadInfoQueue.Count > 0)
//             {
//                 Debug.Log(chunkThreadInfoQueue.Count);
//                 ChunkThreadInfo<string> chunkThreadInfo = chunkThreadInfoQueue.Dequeue();
//                 chunkThreadInfo.callback(chunkThreadInfo.parameter);
//             }

//             // Destroy past chunks
//             foreach (string _chunkName in chunks.Keys)
//             {
//                 if (_chunkName != currentChunk && !nearChunks.Contains(_chunkName))
//                 {
//                     DestroyChunk(_chunkName);
//                 }
//             }

//             if (!chunks.ContainsKey(currentChunk) || !chunks[currentChunk].rendered)
//             {    
//                 nearChunks.Add(currentChunk);
//             }

//             // Spawn new chunks
//             nearChunks.ForEach(_chunkName =>
//             {
//                 if (!chunks.ContainsKey(_chunkName))
//                 {
//                     if (this.isServer)
//                     {
//                         Debug.Log("Server tries to create chunk");
//                         // Calculate chunk and add it to memory
//                         CreateChunk(OnChunkCreated, _chunkName);
//                         // Chunk newChunk = calculateChunk(_chunkName);
//                         // chunks[_chunkName] = newChunk;
//                     }
//                     else
//                     {
//                         // Ask chunk info to server
//                         CmdReceiveChunk(_chunkName);
//                     }
//                 }

//                 // TODO: Do this with a callback to thread
//                 // if (!chunks[_chunkName].rendered)
//                 // {
//                 //     SpawnChunk(_chunkName);
//                 // }
//             });

//             prevChunk = currentChunk;
//         }
//     }
    
//     void CreateChunk(Action<string> callback, string _chunkName, bool sendToClient = false)
//     {
//         ThreadStart threadStart = delegate{
//             Chunk newChunk = calculateChunk(_chunkName);
//             newChunk.name = _chunkName;
//             chunks[_chunkName] = newChunk;
//             ChunkThreadInfo<string> chunkInfo = new ChunkThreadInfo<string>(callback, _chunkName);
            
//             if(sendToClient)
//             {
//                 RpcFetchChunk(_chunkName, newChunk.blockMap, newChunk.renderMap);
//             }
//             else
//             {
//                 chunkThreadInfoQueue.Enqueue( chunkInfo );   
//             }

//             Debug.Log("Server added created chunk to queue");
//         };

//         new Thread(threadStart).Start();
//     }

//     void OnChunkCreated(string chunkName)
//     {   
//         if (!chunks[chunkName].rendered)
//         {
//             SpawnChunk(chunkName);
//         }
//     }

//     [Command]
//     void CmdReceiveChunk(string chunkName)
//     {
//         Debug.Log("I heard ur call bro! " + chunkName);

//         if (chunks.ContainsKey(chunkName))
//         {
//             RpcFetchChunk(chunkName, chunks[chunkName].blockMap, chunks[chunkName].renderMap);
//         }
//         else
//         {
//             CreateChunk(OnChunkCreated, chunkName, true);
//         }
//     }


//     [ClientRpc]
//     void RpcFetchChunk(string chunkName, byte[] blockMap, byte[] renderMap)
//     {
//         Debug.Log("I got ur message bro!" + chunkName + renderMap.Length.ToString());
//         Chunk chunk = new Chunk();
//         chunk.name = chunkName;

//         chunk.blockMap = blockMap;
//         chunk.renderMap = renderMap;

//         chunks[chunk.name] = chunk;
//         OnChunkCreated(chunk.name);
//     }

//     Chunk calculateChunk(string chunkName)
//     {
//         string[] coordinates = chunkName.Split('C');
//         int initialX = int.Parse(coordinates[0]);
//         int initialZ = int.Parse(coordinates[1]);

//         Chunk chunk = new Chunk();

//         chunk.name = initialX.ToString() + "C" + initialZ.ToString();

//         chunk.blockMap = new byte[(int)Mathf.Pow(CHUNK_SIZE, 3)];
//         chunk.renderMap = new byte[(int)Mathf.Pow(CHUNK_SIZE, 3)];

//         int i = 0;

//         for (int x = initialX; x < initialX + CHUNK_SIZE; x++)
//         {
//             for (int z = initialZ; z < initialZ + CHUNK_SIZE; z++)
//             {
//                 int y = calculateSurfaceCoordinates(x, z);

//                 for (int _y = 0; _y < CHUNK_SIZE; _y++)
//                 {
//                     if (_y >= y)
//                         chunk.renderMap[i] = 1;

//                     if (_y <= y)
//                         chunk.blockMap[i] = 1;

//                     i++;
//                 }
//             }
//         }

//         return chunk;
//     }

//     int calculateSurfaceCoordinates(int x, int z)
//     {
//         float xCoord = (float)x / CHUNK_SIZE;
//         float zCoord = (float)z / CHUNK_SIZE;

//         return Mathf.RoundToInt(Mathf.PerlinNoise(xCoord, zCoord) * CHUNK_SIZE);
//     }

//     List<string> findNearChunks(string chunkName)
//     {
//         List<string> chunkNames = new List<string>();

//         string[] coordinates = chunkName.Split('C');
//         int initialX = int.Parse(coordinates[0]);
//         int initialZ = int.Parse(coordinates[1]);

//         for (int i = -1 * RENDER_DISTANCE; i <= RENDER_DISTANCE; i++)
//         {
//             for (int w = -1 * RENDER_DISTANCE; w <= RENDER_DISTANCE; w++)
//             {
//                 // Current chunk
//                 if (i == 0 && w == 0)
//                     continue;

//                 int x = initialX + i * CHUNK_SIZE;
//                 int z = initialZ + w * CHUNK_SIZE;

//                 chunkNames.Add(x.ToString() + "C" + z.ToString());
//             }
//         }

//         return chunkNames;
//     }

//     void DestroyChunk(string chunkName)
//     {
//         // GameObject chunkObj = GameObject.Find(chunkName);
//         Chunk chunk = chunks[chunkName];

//         chunks[chunkName].rendered = false;

//         int j = chunk.gameObjects.Count;
//         for (int i = 0; i < j; i++)
//         {
//             GameObject block = chunk.gameObjects.Dequeue();
//             block.SetActive(false);
//             objectPools[cubePrefab.name].Enqueue(block);
//         }
//     }

//     void DestroyBlock(GameObject block)
//     {
//         block.SetActive(false);
//         objectPools[cubePrefab.name].Enqueue(block);
//     }

//     void SpawnChunk(string chunkName)
//     {
//         string[] coordinates = chunkName.Split('C');
//         int initialX = int.Parse(coordinates[0]);
//         int initialZ = int.Parse(coordinates[1]);

//         Chunk chunk = chunks[chunkName];
//         chunk.rendered = true;
//         chunk.gameObjects = new Queue<GameObject>();

//         // Instantiate Chunk
//         int AREA = CHUNK_SIZE * (CHUNK_SIZE);
//         int VOLUME = (int)Mathf.Pow(CHUNK_SIZE, 3);

//         for (int i = 0; i < VOLUME; i++)
//         {
//             if (chunk.blockMap[i] == 1 && chunk.renderMap[i] != 0)
//             {
//                 int x = i / AREA;
//                 int y = i % (CHUNK_SIZE);
//                 int z = (i % AREA) / (CHUNK_SIZE);

//                 Vector3 pos = new Vector3(x + initialX, y, z + initialZ);
//                 GameObject block = SpawnFromDict(cubePrefab.name, pos);
//                 chunk.gameObjects.Enqueue(block);
//             }
//         }
//     }

//     public GameObject SpawnFromDict(string blockName, Vector3 pos)
//     {
//         if (!objectPools.ContainsKey(blockName))
//         {
//             return null;
//         }

//         GameObject block = objectPools[blockName].Dequeue();
//         block.SetActive(true);
//         block.transform.position = pos;
//         return block;
//     }

//     public void SetPlayerTransform(Transform t)
//     {
//         playerTransform = t;

//         float x = playerTransform.position.x;
//         float z = playerTransform.position.z;

//         x = Mathf.RoundToInt(x / CHUNK_SIZE);
//         z = Mathf.RoundToInt(z / CHUNK_SIZE);

//         string chunkName = (x * CHUNK_SIZE).ToString() + "C" + (z * CHUNK_SIZE).ToString();

//         currentChunk = chunkName;
//     }

//     void InitializeObjectPool()
//     {
//         Queue<GameObject> objectPool = new Queue<GameObject>();
//         int POOL_SIZE = (int)Mathf.Pow(CHUNK_SIZE, 2) * (int)Mathf.Pow(2 * RENDER_DISTANCE + 2, 2);

//         for (int i = 0; i < POOL_SIZE; i++)
//         {
//             GameObject block = Instantiate(cubePrefab);
//             // block.transform.parent = transform;
//             objectPool.Enqueue(block);
//         }

//         objectPools.Add(cubePrefab.name, objectPool);
//     }
// }

// public struct ChunkThreadInfo<T> {
//     public readonly Action<T> callback;
//     public readonly T parameter;

//     public ChunkThreadInfo (Action<T> callback, T parameter)
//     {
//         this.callback = callback;
//         this.parameter = parameter;
//     }
// }

// /*
// void Update() {
// 		if (mapDataThreadInfoQueue.Count > 0) {
// 			for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
// 				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue ();
// 				threadInfo.callback (threadInfo.parameter);
// 			}
// 		}
// 	}

// public void RequestMapData(Action<MapData> callback) {
// 		ThreadStart threadStart = delegate {
// 			MapDataThread (callback);
// 		};

// 		new Thread (threadStart).Start ();
// 	}

// 	void MapDataThread(Action<MapData> callback) {
// 		MapData mapData = GenerateMapData ();
// 		lock (mapDataThreadInfoQueue) {
// 			mapDataThreadInfoQueue.Enqueue (new MapThreadInfo<MapData> (callback, mapData));
// 		}
// 	}

// 	MapData GenerateMapData() {

	
// 		return new MapData (noiseMap, colourMap);
// 	}


// 	struct MapThreadInfo<T> {
// 		public readonly Action<T> callback;
// 		public readonly T parameter;

// 		public MapThreadInfo (Action<T> callback, T parameter)
// 		{
// 			this.callback = callback;
// 			this.parameter = parameter;
// 		}
		
// 	}

// }

// public struct MapData {
// 	public readonly float[,] heightMap;
// 	public readonly Color[] colourMap;

// 	public MapData (float[,] heightMap, Color[] colourMap)
// 	{
// 		this.heightMap = heightMap;
// 		this.colourMap = colourMap;
// 	}
// }
// */
