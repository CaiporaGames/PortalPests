using Aegis.GrenadeSystem.HiEx;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class GrenadeThrowHandler : MonoBehaviour
{
    private XRGrabInteractable grab;
    private Rigidbody rb;

    private HiExGrenade grenade;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        grenade = GetComponent<HiExGrenade>();
    }

    private void OnEnable()
    {
        grab.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        grab.selectExited.RemoveListener(OnReleased);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        //Debug.Log("Grenade released!");

        // Optional: check if it was actually thrown (has velocity)
      /*   if (rb.linearVelocity.magnitude > 0.1f)
        {
            Debug.Log("Grenade thrown!");
 */
            grenade.StartExplosionTimer();
        /* }
        else
        {
            Debug.Log("Dropped, not thrown.");
        } */
    }
}