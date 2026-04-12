using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRHandDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hingePivot;
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable handleGrabInteractable;

    [Header("Door Limits")]
    [SerializeField] private float minAngle = 0f;
    [SerializeField] private float maxAngle = 100f;

    [Header("Behavior")]
    [SerializeField] private float followSpeed = 12f;
    [SerializeField] private bool stayOpenWhenReleased = true;
    [SerializeField] private float autoCloseSpeed = 4f;

    [Header("Debug")]
    [SerializeField] private bool invertDirection = false;

    private Transform _grabbingHand;
    private bool _isGrabbed;

    private float _currentAngle;
    private float _targetAngle;

    private Quaternion _initialLocalRotation;

    private void Awake()
    {
        if (hingePivot == null)
            hingePivot = transform;

        _initialLocalRotation = hingePivot.localRotation;

        if (handleGrabInteractable != null)
        {
            handleGrabInteractable.selectEntered.AddListener(OnGrabbed);
            handleGrabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDestroy()
    {
        if (handleGrabInteractable != null)
        {
            handleGrabInteractable.selectEntered.RemoveListener(OnGrabbed);
            handleGrabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void Update()
    {
        if (_isGrabbed && _grabbingHand != null)
        {
            _targetAngle = CalculateHandDrivenAngle();
        }
        else if (!stayOpenWhenReleased)
        {
            _targetAngle = Mathf.MoveTowards(_targetAngle, minAngle, autoCloseSpeed * Time.deltaTime * 100f);
        }

        _currentAngle = Mathf.Lerp(_currentAngle, _targetAngle, Time.deltaTime * followSpeed);
        _currentAngle = Mathf.Clamp(_currentAngle, minAngle, maxAngle);

        hingePivot.localRotation = _initialLocalRotation * Quaternion.Euler(0f, _currentAngle, 0f);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        _isGrabbed = true;

        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
        {
            _grabbingHand = interactor.transform;
        }
        else
        {
            _grabbingHand = args.interactorObject.transform;
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        _isGrabbed = false;
        _grabbingHand = null;
    }

    private float CalculateHandDrivenAngle()
    {
        Vector3 localHandPos = hingePivot.parent != null
            ? hingePivot.parent.InverseTransformPoint(_grabbingHand.position)
            : _grabbingHand.position;

        Vector3 localHingePos = hingePivot.parent != null
            ? hingePivot.parent.InverseTransformPoint(hingePivot.position)
            : hingePivot.position;

        Vector3 dir = localHandPos - localHingePos;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return _currentAngle;

        dir.Normalize();

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        if (invertDirection)
            angle = -angle;

        angle = NormalizeAngle(angle);

        return Mathf.Clamp(angle, minAngle, maxAngle);
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;

        while (angle < -180f)
            angle += 360f;

        return angle;
    }

    public bool IsOpenEnough(float openThreshold = 70f)
    {
        return _currentAngle >= openThreshold;
    }

    public void ForceOpen()
    {
        _targetAngle = maxAngle;
        _currentAngle = maxAngle;
        hingePivot.localRotation = _initialLocalRotation * Quaternion.Euler(0f, _currentAngle, 0f);
    }

    public void ForceClosed()
    {
        _targetAngle = minAngle;
        _currentAngle = minAngle;
        hingePivot.localRotation = _initialLocalRotation * Quaternion.Euler(0f, _currentAngle, 0f);
    }
}