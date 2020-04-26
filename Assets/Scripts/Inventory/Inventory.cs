using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Inventory : NetworkBehaviour
{
    public List<Item> items = new List<Item>();
    public GameObject inventoryPanel;
    private UIInventory inventoryUI;

    private int currentItem = 0;

    // public void SetupClient()
    // {
    //     NetworkClient.RegisterHandler<InvMessage>(OnInvMessage);
    //     NetworkClient.Connect("localhost");
    // }

    // public void OnInvMessage(NetworkConnection conn, InvMessage msg)
    // {
    //     List<InvItem> invItems = new List<InvItem>();
    //     string[] listElements = msg.invList.Split('/');

    //     foreach (string listElement in listElements)
    //     {
    //         string[] itemElements = listElement.Split(';');
    //         InvItem invItem = new InvItem(Int32.Parse(itemElements[0]), Int32.Parse(itemElements[1]), Int32.Parse(itemElements[2]));
    //         invItems.Add(invItem);
    //     }

    //     LoadUsersItems(invItems);
    // }

    void Start()
    {
        if (!this.isLocalPlayer)
            return;

        inventoryPanel = GameObject.Find("InventoryPanel");
        inventoryUI = inventoryPanel.GetComponent<UIInventory>();

        LoadUsersItems();
        // if (this.isServer)
        // {
        //     List<InvItem> invItems = new List<InvItem>();
        //     invItems = InventoryManager.inventoryManager.Load();
        //     LoadUsersItems(invItems);
        // }
    }

    public Item GetCurrentItem()
    {
        return items[currentItem];
    }

    public void SetActiveItem(int slot)
    {
        currentItem = slot;
        inventoryUI.SetActiveSlot(slot);
    }

    public void AddItem(Item item, int slot)
    {
        items.Add(item);
        inventoryUI.AddItem(item, slot);
    }

    void LoadUsersItems(List<InvItem> invItems)
    {
        foreach (InvItem invItem in invItems)
        {
            Item item = ItemDatabase.instance.GetItemById(invItem.itemId);
            item.stackCount = invItem.stackCount;
            AddItem(item, invItem.slot);
        }
    }

    void LoadUsersItems()
    {
        AddItem( ItemDatabase.instance.GetItemById(1), 0);
        AddItem( ItemDatabase.instance.GetItemById(2), 1);
        AddItem( ItemDatabase.instance.GetItemById(3), 2);
        AddItem( ItemDatabase.instance.GetItemById(4), 3);
        AddItem( ItemDatabase.instance.GetItemById(5), 4);
        AddItem( ItemDatabase.instance.GetItemById(6), 5);
        AddItem( ItemDatabase.instance.GetItemById(7), 6);
        AddItem( ItemDatabase.instance.GetItemById(8), 7);
        AddItem( ItemDatabase.instance.GetItemById(9), 8);
        AddItem( ItemDatabase.instance.GetItemById(10), 9);
    }

    public string CreateUniqueName()
    {
        string randomName = "";
        string createTime = DateTime.Now.ToBinary().ToString();
        int randomNum = UnityEngine.Random.Range(0, 12345);
        randomName = createTime + randomNum.ToString();

        return randomName;
    }

    [Command]
    public void CmdSpawnBlock(string prefabName, Vector3 pos)
    {
        GameObject prefab = (GameObject)Resources.Load("Prefabs/Items/" + prefabName);
        GameObject block = MonoBehaviour.Instantiate(prefab, pos, Quaternion.identity);

        block.name = CreateUniqueName();

        Event _event = new Event(prefabName, block.name, "createBlock", pos.x, pos.y, pos.z);

        BlockManager.blockManager.eventContainer.Add(_event);

        NetworkServer.Spawn(block);
    }

    [Command]
    public void CmdDestroy(GameObject block)
    {
        if (block.GetComponent<NetworkIdentity>())
        {
            Event _event = new Event(block.name, "removeBlock");
            BlockManager.blockManager.eventContainer.Add(_event);
            Destroy(block);
        }
    }

    private List<InvItem> GetInventoryList()
    {
        List<InvItem> invItems = new List<InvItem>();

        foreach (Item item in items)
        {
            InvItem invItem = new InvItem(inventoryUI.GetSlotIndex(item), item.id, item.stackCount);
            invItems.Add(invItem);
        }

        return invItems;
    }

    private void SaveInventory()
    {
        List<InvItem> invList = GetInventoryList();
        InventoryManager.inventoryManager.Save(invList);
    }

    [Command]
    public void CmdSaveInventory()
    {
        List<InvItem> invList = GetInventoryList();
        InventoryManager.inventoryManager.Save(invList);
    }

    void OnApplicationQuit()
    {
        if(this.isServer)
        {
            // BlockManager.blockManager.Save();
            //SaveInventory();
        }
        

    }
}
