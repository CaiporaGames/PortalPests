using System.IO;
using UnityEngine;

public class PersistentItemRecord : IBinarySerializable
{
    public string itemId;
    public PersistentItemType itemType;
    public PersistentItemState state;

    public string sceneName;
    public int slotIndex;

    public Vector3 position;
    public Quaternion rotation;

    public string consumedByTargetId;

    public PersistentItemRecord() { }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(itemId ?? "");
        writer.Write((int)itemType);
        writer.Write((int)state);

        writer.Write(sceneName ?? "");
        writer.Write(slotIndex);

        writer.Write(position.x);
        writer.Write(position.y);
        writer.Write(position.z);

        writer.Write(rotation.x);
        writer.Write(rotation.y);
        writer.Write(rotation.z);
        writer.Write(rotation.w);

        writer.Write(consumedByTargetId ?? "");
    }

    public void Deserialize(BinaryReader reader)
    {
        itemId = reader.ReadString();
        itemType = (PersistentItemType)reader.ReadInt32();
        state = (PersistentItemState)reader.ReadInt32();

        sceneName = reader.ReadString();
        slotIndex = reader.ReadInt32();

        float px = reader.ReadSingle();
        float py = reader.ReadSingle();
        float pz = reader.ReadSingle();
        position = new Vector3(px, py, pz);

        float rx = reader.ReadSingle();
        float ry = reader.ReadSingle();
        float rz = reader.ReadSingle();
        float rw = reader.ReadSingle();
        rotation = new Quaternion(rx, ry, rz, rw);

        consumedByTargetId = reader.ReadString();
    }
}