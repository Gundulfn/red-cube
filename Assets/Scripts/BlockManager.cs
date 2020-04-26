using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using Mirror;
using System.Xml;

public class BlockManager: NetworkBehaviour
{
    public EventContainer eventContainer = new EventContainer();

    public static BlockManager blockManager;

    void Awake()
    {
        blockManager = this;
    }

    void Start()
    {
        Load();
    }

    public void Load()
    {
        //Debug.Log("Loading Blocks...");
        
        var serializer = new XmlSerializer(typeof(List<Event>), new XmlRootAttribute("events"));
        string path = Path.Combine(Application.persistentDataPath, "events.xml");

        if (!System.IO.File.Exists(Path.Combine(Application.persistentDataPath, "events.xml")))
            return;

 		using(var stream = new FileStream(path, FileMode.Open))
 		{
 			List<Event> _events = serializer.Deserialize(stream) as List<Event>;
            
            foreach (Event e in _events) {
                switch(e._type)
                {
                    case "createBlock":
                        Vector3 pos = new Vector3(e.x, e.y, e.z);
                        GameObject blockPrefab = (GameObject)Resources.Load("Prefabs/Items/" + e._prefabName);
                        GameObject block = GameObject.Instantiate(blockPrefab, pos, Quaternion.identity);
                        
                        block.name = e._name;
                        NetworkServer.Spawn(block);
                        break;

                    case "removeBlock":
                        GameObject obj = GameObject.Find(e._name);
                        NetworkServer.Destroy(obj);
                        break;    
                }
            }
 		}
    }

    public void Save()
    {
        Debug.Log("Saving Blocks...");

        string path = Path.Combine(Application.persistentDataPath, "events.xml");

        XmlDocument xmlDocument = new XmlDocument();

        if (System.IO.File.Exists(path))
            xmlDocument.Load(path);
        
        foreach (Event e in eventContainer.GetEvents(-1)) {
            XmlElement eventElement = xmlDocument.CreateElement("event");

            XmlAttribute _name = xmlDocument.CreateAttribute("_name");
            _name.Value = e._name;
            eventElement.Attributes.Append(_name);

            XmlAttribute _type = xmlDocument.CreateAttribute("_type");
            _type.Value = e._type;
            eventElement.Attributes.Append(_type);
            
            if(e._type == "createBlock")
            {
                XmlAttribute prefabName = xmlDocument.CreateAttribute("prefabName");
                prefabName.Value = e._prefabName;
                eventElement.Attributes.Append(prefabName);

                XmlAttribute x = xmlDocument.CreateAttribute("x");
                x.Value = e.x.ToString();
                eventElement.Attributes.Append(x);

                XmlAttribute y = xmlDocument.CreateAttribute("y");
                y.Value = e.y.ToString();
                eventElement.Attributes.Append(y);

                XmlAttribute z = xmlDocument.CreateAttribute("z");
                z.Value = e.z.ToString();
                eventElement.Attributes.Append(z);
            }

            xmlDocument.DocumentElement.AppendChild(eventElement);
        }
        
        xmlDocument.Save(path);
    }

}

public class EventContainer {
    public List<Event> events = new List<Event>();

    public void Add(Event e) {
        events.Add(e);
    }

    public List<Event> GetEvents(int size) {
        if (size == -1)
            return events;
               
        return events.GetRange(0, size);
    }

    public void RemoveEvents(int size) {
        events.RemoveRange(0, size);
    }

    public void SetEvents(List<Event> _events) {
        events = _events;
    }
}

[XmlType("event")]
public class Event 
{
    [XmlAttribute("prefabName")]
    public string _prefabName;

    [XmlAttribute("_name")]
    public string _name;
    
    [XmlAttribute("_type")]
    public string _type;
    
    [XmlAttribute("x")]
    public float x;
    
    [XmlAttribute("y")]
    public float y;

    [XmlAttribute("z")]
    public float z;

    public Event(string _prefabName, string _name, string _type, float x, float y, float z) {
        this._prefabName = _prefabName;
        this._name = _name;
        this._type = _type;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Event(string _name, string _type) {
        this._name = _name;
        this._type = _type;
    }

    public Event() {}
}

