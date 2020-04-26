using Mirror;

public class ChunkMessage : MessageBase
{
    public string name;
    public byte[] blockMap;
    public byte[] renderMap;
}

public class ChunkRequestMessage : MessageBase
{
    public string name;
}

public class ChunkUpdateMessage : MessageBase
{
    public string name;
    public int index;
    public byte blockId;
}

public class ChunkUpdateRequestMessage : MessageBase
{
    public string name;
    public int index;
    public byte blockId;
}