using System.Collections.Generic;
using System.IO;
//Since save service saves one root object, this is the object that goes into SaveAsync.
public class PersistentItemsData : IBinarySerializable
{
    public List<PersistentItemRecord> items = new();

    public PersistentItemsData() { }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(items.Count);

        for (int i = 0; i < items.Count; i++)
        {
            items[i].Serialize(writer);
        }
    }

    public void Deserialize(BinaryReader reader)
    {
        items.Clear();

        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            var item = new PersistentItemRecord();
            item.Deserialize(reader);
            items.Add(item);
        }
    }
}