using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public byte id;
    public string displayName;
    public int stackCount;
    public int stackLimit;
    public string spriteName;
    public Sprite sprite;
    public string prefabName;
    public GameObject prefab;

    public Item(byte id, string displayName, string spriteName, string prefabName, int stackCount, int stackLimit)
    {
        this.id = id;
        this.displayName = displayName;
        this.spriteName = spriteName;
        this.prefabName = prefabName;
        this.stackCount = stackCount;
        this.stackLimit = stackLimit;

        this.sprite = Resources.Load<Sprite>("Sprites/Items/" + this.spriteName);
        this.prefab = Resources.Load<GameObject>("Prefabs/Items/" + this.prefabName);
    }

    public Item(){ }
}
