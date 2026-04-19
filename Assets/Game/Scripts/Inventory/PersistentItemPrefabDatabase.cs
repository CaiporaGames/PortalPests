using System;
using UnityEngine;

[Serializable]
public class PersistentItemPrefabEntry
{
    public PersistentItemType itemType;
    public PersistentPickableItem prefab;
}

public class PersistentItemPrefabDatabase : MonoBehaviour
{
    [SerializeField] private PersistentItemPrefabEntry[] entries;

    public PersistentPickableItem GetPrefab(PersistentItemType itemType)
    {
        if (entries == null)
            return null;

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].itemType == itemType)
                return entries[i].prefab;
        }

        return null;
    }
}