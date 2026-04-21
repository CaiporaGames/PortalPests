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
    private bool _isRuntimeInventoryInstance;
    private bool _isRuntimeWorldInstance;
    private bool _eventsRegistered;


    private async void Awake()
    {
        _identity = GetComponent<PersistentItemIdentity>();
        _grab = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
    }

    private async void Start()
    {
        await InitializeSelfAsync();
    }

    private async UniTask InitializeSelfAsync()
    {
        _saveManager = ServiceLocator.Resolve<PersistentItemSaveManager>();
        _hipInventory = FindFirstObjectByType<HipInventory>();

        if (_saveManager == null)
        {
            Debug.LogError($"[{name}] PersistentItemSaveManager not found.");
            return;
        }

        if (!_saveManager.IsInitialized)
            await _saveManager.InitializeAsync();

        RegisterGrabEvents();

        if (_isRuntimeInventoryInstance)
        {
            Debug.Log($"{name} is a runtime inventory instance.");
            return;
        }

        if (_isRuntimeWorldInstance)
        {
            transform.localScale = worldScale;
            _rb.isKinematic = false;
            _rb.useGravity = true;
            gameObject.SetActive(true);
            return;
        }

        var record = _saveManager.GetOrCreateRecord(_identity, transform);

        if (record.state == PersistentItemState.Consumed)
        {
            gameObject.SetActive(false);
            return;
        }

        if (record.state == PersistentItemState.InInventory)
        {
            gameObject.SetActive(false);
            return;
        }

        if (record.state == PersistentItemState.InWorld)
        {
            if (record.sceneName == SceneManager.GetActiveScene().name)
            {
                transform.SetPositionAndRotation(record.position, record.rotation);
                transform.localScale = worldScale;
                _rb.isKinematic = false;
                _rb.useGravity = true;
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void RegisterGrabEvents()
    {
        if (_eventsRegistered || _grab == null)
            return;

        _grab.selectEntered.AddListener(OnGrab);
        _grab.selectExited.AddListener(OnRelease);
        _eventsRegistered = true;
    }

    public void MarkAsRuntimeInventoryInstance()
    {
        _isRuntimeInventoryInstance = true;
    }

    public void MarkAsRuntimeWorldInstance()
    {
        _isRuntimeWorldInstance = true;
    }

    private async void Initialize(bool dummy)
    {
        if (_isRuntimeInventoryInstance)
        {
            _saveManager = ServiceLocator.Resolve<PersistentItemSaveManager>();
            _hipInventory = FindFirstObjectByType<HipInventory>();

            _grab.selectEntered.AddListener(OnGrab);
            _grab.selectExited.AddListener(OnRelease);

            Debug.Log($"{name} is a runtime inventory instance. Skipping world-state initialization.");
            return;
        }

        if (_isRuntimeWorldInstance)
        {
            _saveManager = ServiceLocator.Resolve<PersistentItemSaveManager>();
            _hipInventory = FindFirstObjectByType<HipInventory>();

            transform.localScale = worldScale;
            _rb.isKinematic = false;
            _rb.useGravity = true;
            gameObject.SetActive(true);

            _grab.selectEntered.AddListener(OnGrab);
            _grab.selectExited.AddListener(OnRelease);
            return;
        }
        
        _saveManager = ServiceLocator.Resolve<PersistentItemSaveManager>();
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

    private void ExitSlotState()
    {
        if (_currentSlot != null)
        {
            _currentSlot.Clear();
            _currentSlot = null;
        }

        transform.SetParent(null);
        transform.localScale = worldScale;

        _rb.isKinematic = false;
        _rb.useGravity = true;
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
        if (_hipInventory != null)
            _hipInventory.RemoveItem(this);

        ExitSlotState();
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

        // Not stored in inventory -> becomes world item
        transform.SetParent(null);
        transform.localScale = worldScale;
        _rb.isKinematic = false;
        _rb.useGravity = true;

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
        _rb.useGravity = false;

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
        _rb.useGravity = false;

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

    private void OnDestroy()
    {
        if (_grab != null && _eventsRegistered)
        {
            _grab.selectEntered.RemoveListener(OnGrab);
            _grab.selectExited.RemoveListener(OnRelease);
        }
    }
}