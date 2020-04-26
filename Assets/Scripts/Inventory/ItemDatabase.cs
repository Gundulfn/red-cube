using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance;
    public List<Item> items = new List<Item>();

    void Awake()
    {
        instance = this;

        BuildItemDatabase();
    }

    public Item GetItemById(int id) {
        return items.Find(item => item.id == id);
    }

    void BuildItemDatabase()
    {
        Block dirtBlock = new Block(1, "Dirt Block", "dirtBlock", "dirtBlock", 1, 50, 100);
        Block lavaBlock = new Block(2, "Lava Block", "lavaBlock", "lavaBlock", 1, 50, 100);
        Block stoneBlock = new Block(3, "Stone Block", "stoneBlock", "stoneBlock", 1, 50, 100);
        Block concreteBlock = new Block(4, "Concrete Block", "concreteBlock", "concreteBlock", 1, 50, 100);
        Block torchBlock = new Block(5, "Torch Block", "torchBlock", "torchBlock", 1, 50, 100);
        Block cobblestoneBlock = new Block(6, "Cobblestone Block", "cobblestoneBlock", "cobblestoneBlock", 1, 50, 100);
        Block brickBlock = new Block(7, "Brick Block", "brickBlock", "brickBlock", 1, 50, 100);
        Block tileBlock = new Block(8, "Tile Block", "tileBlock", "tileBlock", 1, 50, 100);
        Block sandBlock = new Block(9, "Sand Block", "sandBlock", "sandBlock", 1, 50, 100);
        Block woodBlock = new Block(10, "Wood Block", "woodBlock", "woodBlock", 1, 50, 100);
        Block asphaltBlock = new Block(11, "Asphalt Block", "asphaltBlock", "asphaltBlock", 1, 50, 100);
        Block marbleBlock = new Block(12, "Marble Block", "marbleBlock", "marbleBlock", 1, 50, 100);
        Block rockBlock = new Block(13, "Rock Block", "rockBlock", "rockBlock", 1, 50, 100);
        

        items.Add(lavaBlock);
        items.Add(stoneBlock);
        items.Add(dirtBlock);
        items.Add(concreteBlock);
        items.Add(torchBlock);
        items.Add(brickBlock);
        items.Add(cobblestoneBlock);
        items.Add(rockBlock);
        items.Add(tileBlock);
        items.Add(sandBlock);
        items.Add(woodBlock);
        items.Add(asphaltBlock);
        items.Add(marbleBlock);
    }
}
