using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    [Header("Slot Info")]
    [SerializeField] private int slotIndex;

    [Header("Visual")]
    [SerializeField] private Renderer slotRenderer;
    [SerializeField] private Color emptyColor = Color.green;
    [SerializeField] private Color filledColor = Color.blue;

    public int SlotIndex => slotIndex;
    public bool IsOccupied => currentItem != null;

    public PersistentPickableItem currentItem;

    private void Start()
    {
        RefreshVisual();
    }

    public void AssignItem(PersistentPickableItem item)
    {
        currentItem = item;
        RefreshVisual();
    }

    public void Clear()
    {
        currentItem = null;
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (slotRenderer == null || slotRenderer.material == null)
            return;

        slotRenderer.material.color = IsOccupied ? filledColor : emptyColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        var item = other.GetComponentInParent<PersistentPickableItem>();
        if (item == null)
            return;

        item.NotifyEnteredSlotTrigger(this);
    }

    private void OnTriggerExit(Collider other)
    {
        var item = other.GetComponentInParent<PersistentPickableItem>();
        if (item == null)
            return;

        item.NotifyExitedSlotTrigger(this);
    }
}