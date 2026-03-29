using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Aegis.GrenadeSystem.HiEx
{
    public class GrenadePickup : MonoBehaviour
    {
        [SerializeField] AudioClip grenadePickupSound;
        private AudioSource audioSource;

        private XRGrabInteractable grab;

        private void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

         private void OnEnable()
        {
            grab.selectExited.AddListener(OnPickUp);
        }

        private void OnDisable()
        {
            grab.selectExited.RemoveListener(OnPickUp);
        }

        private void OnPickUp(SelectExitEventArgs args)
        {
            EventBus.TriggerEvent(EventType.GrenadePickedUp, gameObject);
            audioSource.clip = grenadePickupSound;
            audioSource.Play();
        }     
    }
}