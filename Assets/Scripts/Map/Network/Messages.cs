using Mirror;

public class ChunkMessage : MessageBase
{
    public string name;
    public byte[] blockMap;
    public byte[] renderMap;

    public ChunkMessage() { }

    public ChunkMessage(string name, byte[] blockMap, byte[] renderMap)
    {
        this.name = name;
        this.blockMap = blockMap;
        this.renderMap = renderMap;
    }
}

public class ChunkRequestMessage : MessageBase
{
    public string name;

    public ChunkRequestMessage() { }

    public ChunkRequestMessage(string name)
    {
        this.name = name;
    }
}

public class ChunkUpdateMessage : MessageBase
{
    public string name;
    public int index;
    public byte blockId;

    public ChunkUpdateMessage() { }

    public ChunkUpdateMessage(string name, int index, byte blockId)
    {
        this.name = name;
        this.index = index;
        this.blockId = blockId;
    }
}

public class ChunkUpdateRequestMessage : MessageBase
{
    public string name;
    public int index;
    public byte blockId;

    public ChunkUpdateRequestMessage() { }
}