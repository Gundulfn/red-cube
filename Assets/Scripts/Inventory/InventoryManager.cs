using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using Mirror;

public class InventoryManager : NetworkBehaviour
{
    public static InventoryManager inventoryManager;

    void Awake()
    {
        inventoryManager = this;
    }

    public List<InvItem> Load(string username = "Gundulf")
    {
        Debug.Log("Loading Inventory...");
        
        var serializer = new XmlSerializer(typeof(List<InvItem>), new XmlRootAttribute("invItems"));
        string path = Path.Combine(Application.persistentDataPath, username + ".xml");

        if (!System.IO.File.Exists(path))
            return null;

 		using(var stream = new FileStream(path, FileMode.Open))
 		{
 			List<InvItem> _invItems = serializer.Deserialize(stream) as List<InvItem>;
            
            return _invItems;
 		}
    }

    public void Save(List<InvItem> invItems)
    {
        string path = Path.Combine(Application.persistentDataPath, "Gundulf.xml");

        XmlDocument xmlDocument = new XmlDocument();
        
        if (System.IO.File.Exists(path))
            xmlDocument.Load(path);

        foreach(InvItem invItem in invItems)
        {
            XmlElement itemElement = xmlDocument.CreateElement("invItem");
                
            XmlAttribute _slot = xmlDocument.CreateAttribute("_slot");
            _slot.Value = invItem.slot.ToString();
            itemElement.Attributes.Append(_slot);

            XmlAttribute _itemId = xmlDocument.CreateAttribute("_itemId");
            _itemId.Value = invItem.itemId.ToString();
            itemElement.Attributes.Append(_itemId);

            XmlAttribute _stackCount = xmlDocument.CreateAttribute("_stackCount");
            _stackCount.Value = invItem.stackCount.ToString();
            itemElement.Attributes.Append(_stackCount);

            xmlDocument.DocumentElement.AppendChild(itemElement);
        }

        xmlDocument.Save(path);
    }

    public void SendInvMsg(NetworkConnection conn, string username = "Sabun")
    {
        Debug.Log("HHEYE");
        List<InvItem> invItems = Load(username);

        InvMessage msg = new InvMessage();

        foreach(InvItem invItem in invItems)
        {
            msg.invList += invItem.slot + ";" + invItem.itemId + ";" + invItem.stackCount + ";" + "/";
        }

        NetworkServer.SendToClientOfPlayer(conn.identity, msg);
    }
}

public class InvMessage: MessageBase
{
    public string invList;
}

[XmlType("invItem")]
public class InvItem 
{
    [XmlAttribute("_slot")]
    public int slot;

    [XmlAttribute("_itemId")]
    public int itemId;
    
    [XmlAttribute("_stackCount")]
    public int stackCount;
    

    public InvItem(int _slot, int _itemId, int _stackCount) {
        this.slot = _slot;
        this.itemId = _itemId;
        this.stackCount = _stackCount;
    }

    public InvItem() {}
}
