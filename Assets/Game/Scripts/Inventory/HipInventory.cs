using UnityEngine;

public class HipInventory : MonoBehaviour
{
    [SerializeField] private InventorySlot[] slots;

    public InventorySlot[] Slots => slots;

    public bool TryStoreItemInSlot(PersistentPickableItem item, InventorySlot targetSlot)
    {
        if (item == null || targetSlot == null)
            return false;

        if (targetSlot.IsOccupied && targetSlot.currentItem != item)
            return false;

        if (item.CurrentSlot != null && item.CurrentSlot != targetSlot)
        {
            item.CurrentSlot.Clear();
        }

        targetSlot.AssignItem(item);
        return true;
    }

    public void RemoveItem(PersistentPickableItem item)
    {
        if (item == null)
            return;

        foreach (var slot in slots)
        {
            if (slot.currentItem == item)
            {
                slot.Clear();
                item.transform.SetParent(null);
                return;
            }
        }
    }

    public InventorySlot GetSlotByIndex(int index)
    {
        if (index < 0 || index >= slots.Length)
            return null;

        return slots[index];
    }
}