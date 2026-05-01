using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerLocationData : IBinarySerializable
{
    public bool hasSavedLocation;
    public string sceneName;
    public Vector3 position;
    public Quaternion rotation;

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(hasSavedLocation);
        writer.Write(sceneName ?? "");

        writer.Write(position.x);
        writer.Write(position.y);
        writer.Write(position.z);

        writer.Write(rotation.x);
        writer.Write(rotation.y);
        writer.Write(rotation.z);
        writer.Write(rotation.w);
    }

    public void Deserialize(BinaryReader reader)
    {
        hasSavedLocation = reader.ReadBoolean();
        sceneName = reader.ReadString();

        position = new Vector3(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );

        rotation = new Quaternion(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );
    }
}