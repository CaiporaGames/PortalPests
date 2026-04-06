using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(PersistentItemIdentity))]
[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class PersistentPickableItem : MonoBehaviour
{
    [Header("Scale")]
    [SerializeField] private Vector3 worldScale = Vector3.one;
    [SerializeField] private Vector3 slotScale = Vector3.one * 0.5f;

    private PersistentItemIdentity _identity;
    private XRGrabInteractable _grab;
    private Rigidbody _rb;
    private PersistentItemSaveManager _saveManager;
    private HipInventory _hipInventory;

    private readonly HashSet<InventorySlot> _nearbySlots = new();

    private InventorySlot _currentSlot;
    public InventorySlot CurrentSlot => _currentSlot;

    private async void Awake()
    {
        _identity = GetComponent<PersistentItemIdentity>();
        _grab = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
    }

    private async void Start()
    {
        _saveManager = FindFirstObjectByType<PersistentItemSaveManager>();
        _hipInventory = FindFirstObjectByType<HipInventory>();

        if (_saveManager != null && !_saveManager.IsInitialized)
            await _saveManager.InitializeAsync();

        var record = _saveManager.GetOrCreateRecord(_identity, transform);

        if (record.state == PersistentItemState.Consumed)
        {
            gameObject.SetActive(false);
            return;
        }

        if (record.state == PersistentItemState.InInventory)
        {
            // scene item should not appear if it is already in inventory
            gameObject.SetActive(false);
            return;
        }

        if (record.state == PersistentItemState.InWorld)
        {
            if (record.sceneName == SceneManager.GetActiveScene().name)
            {
                transform.SetPositionAndRotation(record.position, record.rotation);
                transform.localScale = worldScale;
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
                return;
            }
        }

        _grab.selectEntered.AddListener(OnGrab);
        _grab.selectExited.AddListener(OnRelease);
    }

    private void OnDestroy()
    {
        if (_grab != null)
        {
            _grab.selectEntered.RemoveListener(OnGrab);
            _grab.selectExited.RemoveListener(OnRelease);
        }
    }

    public void NotifyEnteredSlotTrigger(InventorySlot slot)
    {
        if (slot != null)
            _nearbySlots.Add(slot);
    }

    public void NotifyExitedSlotTrigger(InventorySlot slot)
    {
        if (slot != null)
            _nearbySlots.Remove(slot);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (_currentSlot != null)
        {
            _currentSlot.Clear();
            _currentSlot = null;
        }

        transform.SetParent(null);
        transform.localScale = worldScale;

        _rb.isKinematic = false;
    }

    private async void OnRelease(SelectExitEventArgs args)
    {
        InventorySlot targetSlot = GetBestNearbySlot();

        if (_hipInventory != null && targetSlot != null)
        {
            bool stored = _hipInventory.TryStoreItemInSlot(this, targetSlot);
            if (stored)
            {
                SnapToSlot(targetSlot);
                await _saveManager.MarkInInventoryAsync(_identity, targetSlot.SlotIndex);
                return;
            }
        }

        await _saveManager.MarkInWorldAsync(
            _identity,
            transform,
            SceneManager.GetActiveScene().name
        );
    }

    private InventorySlot GetBestNearbySlot()
    {
        InventorySlot best = null;
        float bestDistance = float.MaxValue;

        foreach (var slot in _nearbySlots)
        {
            if (slot == null)
                continue;

            if (slot.IsOccupied && slot.currentItem != this)
                continue;

            float distance = Vector3.Distance(transform.position, slot.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = slot;
            }
        }

        return best;
    }

    public void SnapToSlot(InventorySlot slot)
    {
        _currentSlot = slot;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;

        transform.SetParent(slot.transform);
        transform.SetPositionAndRotation(slot.transform.position, slot.transform.rotation);
        transform.localScale = slotScale;
    }

    public void RestoreToSlot(InventorySlot slot)
    {
        _currentSlot = slot;
        slot.AssignItem(this);

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;

        transform.SetParent(slot.transform);
        transform.SetPositionAndRotation(slot.transform.position, slot.transform.rotation);
        transform.localScale = slotScale;
    }

    public async UniTask ConsumeAsync(string targetId = "")
    {
        if (_saveManager != null)
            await _saveManager.MarkConsumedAsync(_identity, targetId);

        gameObject.SetActive(false);
    }

    public string ItemId => _identity.ItemId;
}