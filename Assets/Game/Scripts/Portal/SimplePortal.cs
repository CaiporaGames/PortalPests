using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SimplePortal : MonoBehaviour
{
    [Header("Portal Links")]
    [SerializeField] private SimplePortal linkedPortal;
    [SerializeField] private Transform exitPoint;

    [Header("Rules")]
    [SerializeField] private float exitOffset = 0.25f;
    [SerializeField] private float cooldown = 0.2f;

    private readonly Dictionary<Rigidbody, float> _recentTeleports = new();
    private readonly HashSet<Rigidbody> _blockedUntilExit = new();

    private void Update()
    {
        if (_recentTeleports.Count == 0) return;

        List<Rigidbody> toRemove = new();
        foreach (var kvp in _recentTeleports)
        {
            if (Time.time - kvp.Value > cooldown)
                toRemove.Add(kvp.Key);
        }

        foreach (var rb in toRemove)
            _recentTeleports.Remove(rb);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[{name}] Trigger entered by collider: {other.name}");

        if (linkedPortal == null)
        {
            Debug.LogWarning($"[{name}] linkedPortal is null");
            return;
        }

        if (exitPoint == null)
        {
            Debug.LogWarning($"[{name}] exitPoint is null");
            return;
        }

        if (linkedPortal.exitPoint == null)
        {
            Debug.LogWarning($"[{name}] linkedPortal.exitPoint is null");
            return;
        }

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
            rb = other.GetComponentInParent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning($"[{name}] No Rigidbody found for {other.name}");
            return;
        }

        Debug.Log($"[{name}] Rigidbody found: {rb.name}");

        if (_recentTeleports.ContainsKey(rb))
        {
            Debug.Log($"[{name}] Ignored due to cooldown: {rb.name}");
            return;
        }

        // block the return trip
        if (_blockedUntilExit.Contains(rb)) return;

        XRGrabInteractable grab = other.GetComponentInParent<XRGrabInteractable>();
        if (grab != null && grab.isSelected)
        {
            Debug.Log($"[{name}] Ignored because still grabbed: {rb.name}");
            return;
        }

        TeleportRigidbody(rb);
    }

    private void TeleportRigidbody(Rigidbody rb)
    {
        Transform fromExit = exitPoint;
        Transform toExit = linkedPortal.exitPoint;

        Vector3 oldVelocity = GetLinearVelocity(rb);

        // 1. Convert velocity to portal A's local space
        Vector3 localVelocity = fromExit.InverseTransformDirection(oldVelocity);

        // 2. Flip the forward axis (Z) so the object exits forward, not backward
        localVelocity.z = -localVelocity.z;

        // 3. Convert from that local space into portal B's world space
        Vector3 newWorldVelocity = toExit.TransformDirection(localVelocity);

        // Same treatment for rotation
        Quaternion flip = Quaternion.Euler(0f, 180f, 0f);
        Quaternion portalDelta = toExit.rotation * flip * Quaternion.Inverse(fromExit.rotation);
        Quaternion newWorldRotation = portalDelta * rb.rotation;
        Vector3 newWorldAngularVelocity = portalDelta * rb.angularVelocity;

        Vector3 newWorldPosition = toExit.position + toExit.forward * exitOffset;

        Debug.Log(
            $"[{name}] Teleporting {rb.name} -> {linkedPortal.name}\n" +
            $"Old Velocity: {oldVelocity} | New Velocity: {newWorldVelocity}"
        );

        rb.position = newWorldPosition;
        rb.rotation = newWorldRotation;
        SetLinearVelocity(rb, newWorldVelocity);
        rb.angularVelocity = newWorldAngularVelocity;

        _recentTeleports[rb] = Time.time;
        linkedPortal.BlockUntilExit(rb);
    }

    public void BlockUntilExit(Rigidbody rb)
    {
        _blockedUntilExit.Add(rb);
        _recentTeleports[rb] = Time.time;
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) rb = other.GetComponentInParent<Rigidbody>();
        if (rb == null) return;

        _blockedUntilExit.Remove(rb);
    }

    private Vector3 GetLinearVelocity(Rigidbody rb)
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }

    private void SetLinearVelocity(Rigidbody rb, Vector3 value)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = value;
#else
        rb.velocity = value;
#endif
    }
}