using System.IO;

public class PersistentWorldObjectState : IBinarySerializable
{
    public string objectId;
    public string sceneName;
    public bool isDestroyed;
    public bool isActivated;

    public PersistentWorldObjectState() { }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(objectId ?? "");
        writer.Write(sceneName ?? "");
        writer.Write(isDestroyed);
        writer.Write(isActivated);
    }

    public void Deserialize(BinaryReader reader)
    {
        objectId = reader.ReadString();
        sceneName = reader.ReadString();
        isDestroyed = reader.ReadBoolean();
        isActivated = reader.ReadBoolean();
    }
}