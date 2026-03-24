using System.IO;
using UnityEngine;

/// <summary>
/// Example data class using manual binary serialization.
/// </summary>
public class PlayerProfile : IBinarySerializable
{
    public int level;
    public float xp;
    public string name;
    public bool isReady;
    public Vector3 position;

    // Parameterless constructor required for deserialization
    public PlayerProfile() { }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(level);
        writer.Write(xp);
        writer.Write(name);
        writer.Write(isReady);
        writer.Write(position.x);
        writer.Write(position.y);
        writer.Write(position.z);
    }

    public void Deserialize(BinaryReader reader)
    {
        level = reader.ReadInt32();
        xp = reader.ReadSingle();
        name = reader.ReadString();
        isReady = reader.ReadBoolean();
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        position = new Vector3(x, y, z);
    }
}