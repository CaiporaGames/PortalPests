using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BatterySocketFilter : MonoBehaviour, IXRSelectFilter
{
    [Header("Accepted Interaction Layers")]
    [SerializeField] private InteractionLayerMask acceptedBatteryLayers;
    [SerializeField] private ParticleSystem electricSparks;
    [SerializeField] private AudioSource electricSparksAudio;

    [Header("Inserted Behavior")]
    [SerializeField] private bool lockAfterInserted = true;

    private XRSocketInteractor _socket;
    private IXRSelectInteractable _insertedBattery;

    public bool canProcess => isActiveAndEnabled;

    private void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();
    }

    private IEnumerator Timer()
    {
        electricSparks.Play();

        // Add pitch variation BEFORE playing the audio
        electricSparksAudio.pitch = Random.Range(0.9f, 1.1f);
        electricSparksAudio.Play();

        float randomDelay = Random.Range(0.5f, 1.5f);
        yield return new WaitForSeconds(randomDelay);

        electricSparks.Stop();
        electricSparksAudio.Stop();
    }


    private void Start()
    {
        StartCoroutine(Timer());
    }

    private void OnEnable()
    {
        if (_socket == null)
            _socket = GetComponent<XRSocketInteractor>();

        _socket.selectFilters.Add(this);
        _socket.selectEntered.AddListener(OnBatteryInserted);
        _socket.selectExited.AddListener(OnBatteryRemoved);
    }

    private void OnDisable()
    {
        if (_socket == null)
            return;

        _socket.selectFilters.Remove(this);
        _socket.selectEntered.RemoveListener(OnBatteryInserted);
        _socket.selectExited.RemoveListener(OnBatteryRemoved);
    }

    public bool Process(
        IXRSelectInteractor interactor,
        IXRSelectInteractable interactable)
    {
        if (interactable == null)
            return false;

        if (_insertedBattery != null)
            return false;

        InteractionLayerMask interactableLayers = interactable.interactionLayers;

        return (interactableLayers & acceptedBatteryLayers) != 0;
    }

    private void OnBatteryInserted(SelectEnterEventArgs args)
    {
        _insertedBattery = args.interactableObject;

        Transform battery = _insertedBattery.transform;

        Transform attach = _socket.attachTransform != null
            ? _socket.attachTransform
            : _socket.transform;

        battery.SetPositionAndRotation(
            attach.position,
            attach.rotation
        );

        Rigidbody rb = battery.GetComponent<Rigidbody>();

        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (lockAfterInserted)
        {
            XRGrabInteractable grab = battery.GetComponent<XRGrabInteractable>();

            if (grab != null)
                grab.enabled = false;
        }

        EventBus.TriggerEvent(EventType.BatteryInstalledChanged, true);
        StopCoroutine(Timer());
    }

    private void OnBatteryRemoved(SelectExitEventArgs args)
    {
        if (args.interactableObject == _insertedBattery)
            _insertedBattery = null;
            EventBus.TriggerEvent(EventType.BatteryInstalledChanged, false);
    }
}