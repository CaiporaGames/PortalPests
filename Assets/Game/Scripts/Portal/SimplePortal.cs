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
        Transform fromPortal = transform;
        Transform toPortal = linkedPortal.transform;
        Transform toExit = linkedPortal.exitPoint;

        Vector3 localPosition = fromPortal.InverseTransformPoint(rb.position);
        Quaternion localRotation = Quaternion.Inverse(fromPortal.rotation) * rb.rotation;

        Vector3 newWorldPosition = toPortal.TransformPoint(localPosition);
        Quaternion newWorldRotation = toPortal.rotation * localRotation;

        Vector3 velocity = GetLinearVelocity(rb);
        Vector3 localVelocity = fromPortal.InverseTransformDirection(velocity);
        Vector3 newWorldVelocity = toPortal.TransformDirection(localVelocity);

        Vector3 localAngularVelocity = fromPortal.InverseTransformDirection(rb.angularVelocity);
        Vector3 newWorldAngularVelocity = toPortal.TransformDirection(localAngularVelocity);

        // Push object out in front of destination portal
        newWorldPosition = toExit.position + toExit.forward * exitOffset;

        Debug.Log($"[{name}] Teleporting {rb.name} -> {linkedPortal.name}");
        Debug.Log($"[{name}] New Pos: {newWorldPosition}, Exit Forward: {toExit.forward}");

        rb.position = newWorldPosition;
        rb.rotation = newWorldRotation;
        SetLinearVelocity(rb, newWorldVelocity);
        rb.angularVelocity = newWorldAngularVelocity;

        _recentTeleports[rb] = Time.time;
        linkedPortal.RegisterRecentTeleport(rb);
    }

    public void RegisterRecentTeleport(Rigidbody rb)
    {
        _recentTeleports[rb] = Time.time;
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