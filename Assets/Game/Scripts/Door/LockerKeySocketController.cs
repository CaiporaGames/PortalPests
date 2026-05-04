using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class LockerKeySocketDoubleDoorController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer keyIcon;
    [Header("Socket")]
    [SerializeField] private XRSocketInteractor socket;

    [Header("Doors")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [SerializeField] private Vector3 leftDoorOpenEuler = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 rightDoorOpenEuler = new Vector3(0f, 0f, 0f);

    [SerializeField] private float doorOpenDuration = 1f;

    [Header("Key Turn")]
    [SerializeField] private Vector3 keyTurnLocalEuler = new Vector3(0f, 0f, 90f);
    [SerializeField] private float keyTurnDuration = 0.5f;

    [Header("Options")]
    [SerializeField] private bool lockKeyInSocket = true;
    [SerializeField] private bool openOnlyOnce = true;

    private bool _hasOpened;
    private bool _isAnimating;

    private Quaternion _leftDoorStartRotation;
    private Quaternion _rightDoorStartRotation;

    private IXRSelectInteractable _insertedKey;

    private void Awake()
    {
        if (socket == null)
            socket = GetComponent<XRSocketInteractor>();

        if (leftDoor != null)
            _leftDoorStartRotation = leftDoor.localRotation;

        if (rightDoor != null)
            _rightDoorStartRotation = rightDoor.localRotation;
    }

    private void OnEnable()
    {
        if (socket != null)
            socket.selectEntered.AddListener(OnKeyInserted);
    }

    private void OnDisable()
    {
        if (socket != null)
            socket.selectEntered.RemoveListener(OnKeyInserted);
    }

    private void OnKeyInserted(SelectEnterEventArgs args)
    {
        if (_isAnimating)
            return;

        if (openOnlyOnce && _hasOpened)
            return;

        _insertedKey = args.interactableObject;

        if (lockKeyInSocket)
            LockInsertedKey(args.interactableObject);

        TurnKeyAndOpenDoors().Forget();
    }

    private void LockInsertedKey(IXRSelectInteractable interactable)
    {
        Transform keyTransform = interactable.transform;

        var grab = keyTransform.GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.enabled = false;

        var rb = keyTransform.GetComponent<Rigidbody>();
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
    }

    private async UniTaskVoid TurnKeyAndOpenDoors()
    {
        _isAnimating = true;

        Transform keyTransform = _insertedKey != null ? _insertedKey.transform : null;

        if (keyTransform != null)
            await RotateKey(keyTransform);

        await OpenDoors();

        _hasOpened = true;
        _isAnimating = false;
        keyIcon.enabled = false;
    }

    private async UniTask RotateKey(Transform keyTransform)
    {
        Quaternion startRotation = keyTransform.localRotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(keyTurnLocalEuler);

        float timer = 0f;

        while (timer < keyTurnDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / keyTurnDuration);
            float eased = SmoothStep(t);

            keyTransform.localRotation = Quaternion.Slerp(startRotation, endRotation, eased);

            await UniTask.Yield();
        }

        keyTransform.localRotation = endRotation;
    }

    private async UniTask OpenDoors()
    {
        Quaternion leftStart = leftDoor != null ? leftDoor.localRotation : Quaternion.identity;
        Quaternion rightStart = rightDoor != null ? rightDoor.localRotation : Quaternion.identity;

        Quaternion leftEnd = Quaternion.Euler(leftDoorOpenEuler);
        Quaternion rightEnd = Quaternion.Euler(rightDoorOpenEuler);

        float timer = 0f;

        while (timer < doorOpenDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / doorOpenDuration);
            float eased = SmoothStep(t);

            if (leftDoor != null)
                leftDoor.localRotation = Quaternion.Slerp(leftStart, leftEnd, eased);

            if (rightDoor != null)
                rightDoor.localRotation = Quaternion.Slerp(rightStart, rightEnd, eased);

            await UniTask.Yield();
        }

        if (leftDoor != null)
            leftDoor.localRotation = leftEnd;

        if (rightDoor != null)
            rightDoor.localRotation = rightEnd;
    }

    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }
}