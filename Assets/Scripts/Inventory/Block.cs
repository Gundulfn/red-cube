using System;
using UnityEngine;

public class Block : Item
{
    public int maxHp;

    public Block(byte id, string displayName, string spriteName, string prefabName, int stackCount, int stackLimit, int maxHp) : 
        base(id, displayName, spriteName, prefabName, stackCount, stackLimit)
    {
        this.maxHp = maxHp;
    }

    public Block(){ }

    public GameObject Use(Vector3 pos)
    {
        GameObject block = MonoBehaviour.Instantiate(this.prefab, pos, Quaternion.identity);
        block.name = CreateUniqueName();
        block.GetComponent<BlockObject>().hp = this.maxHp;
        return block;
    }  

    public string CreateUniqueName()
    {
        string randomName = "";
        string createTime = DateTime.Now.ToBinary().ToString();
        int randomNum = UnityEngine.Random.Range(0, 12345);
        randomName = createTime + randomNum.ToString();
        
        return randomName;
    }
}