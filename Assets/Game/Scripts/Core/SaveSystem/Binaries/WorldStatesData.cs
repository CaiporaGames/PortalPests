using System.Collections.Generic;
using System.IO;

public class WorldStatesData : IBinarySerializable
{
    public List<PersistentWorldObjectState> objects = new();

    public WorldStatesData() { }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(objects.Count);

        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].Serialize(writer);
        }
    }

    public void Deserialize(BinaryReader reader)
    {
        objects.Clear();

        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            var state = new PersistentWorldObjectState();
            state.Deserialize(reader);
            objects.Add(state);
        }
    }
}