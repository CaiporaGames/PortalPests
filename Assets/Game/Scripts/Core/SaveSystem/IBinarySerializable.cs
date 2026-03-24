using System.IO;

public interface IBinarySerializable
{
    void Serialize(BinaryWriter writer);
    void Deserialize(BinaryReader reader);
}