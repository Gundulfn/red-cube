using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public bool rendered = false;
    public string name;
    public byte[] blockMap;
    public byte[] renderMap;
    public GameObject[] gameObjects;

    public Chunk() {}
    
}
