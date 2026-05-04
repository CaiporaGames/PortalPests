using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyMassPressureButtonChestOpener : MonoBehaviour
{
    [Header("Weight")]
    [SerializeField] private float requiredMass = 5f;
    [SerializeField] private bool allowCombinedMass = false;

    [Header("Chest Top")]
    [SerializeField] private Transform holderToRotate;
    [SerializeField] private Vector3 closedEuler = Vector3.zero;
    [SerializeField] private Vector3 openEuler = new Vector3(-90f, 0f, 0f);

    [Header("Rising Object")]
    [SerializeField] private Transform risingObject;
    [SerializeField] private float riseYDistance = 0.5f;

    [Header("Animation")]
    [SerializeField] private float openDuration = 0.6f;
    [SerializeField] private float closeDuration = 0.6f;

    [Header("Button Visual")]
    [SerializeField] private Transform buttonVisual;
    [SerializeField] private Vector3 pressedLocalOffset = new Vector3(0f, -0.03f, 0f);

    private readonly HashSet<Rigidbody> _rigidbodiesOnButton = new HashSet<Rigidbody>();

    private Vector3 _buttonStartLocalPosition;
    private Vector3 _risingObjectStartLocalPosition;
    private Vector3 _risingObjectRaisedLocalPosition;

    private bool _isPressed;
    private bool _isAnimating;
    private bool _wantedPressedState;

    private void Awake()
    {
        if (buttonVisual != null)
            _buttonStartLocalPosition = buttonVisual.localPosition;

        if (risingObject != null)
        {
            _risingObjectStartLocalPosition = risingObject.localPosition;
            _risingObjectRaisedLocalPosition =
                _risingObjectStartLocalPosition + new Vector3(0f, riseYDistance, 0f);
        }

        if (holderToRotate != null)
            holderToRotate.localRotation = Quaternion.Euler(closedEuler);
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            rb = other.GetComponentInParent<Rigidbody>();

        if (rb == null)
            return;

        _rigidbodiesOnButton.Add(rb);
        EvaluateButtonState();
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            rb = other.GetComponentInParent<Rigidbody>();

        if (rb == null)
            return;

        _rigidbodiesOnButton.Remove(rb);
        EvaluateButtonState();
    }

    private void EvaluateButtonState()
    {
        bool shouldBePressed = HasEnoughMass();

        _wantedPressedState = shouldBePressed;

        if (shouldBePressed == _isPressed)
            return;

        _isPressed = shouldBePressed;

        if (_isPressed)
            OpenChest().Forget();
        else
            CloseChest().Forget();
    }

    private bool HasEnoughMass()
    {
        _rigidbodiesOnButton.RemoveWhere(rb => rb == null);

        if (allowCombinedMass)
        {
            float totalMass = 0f;

            foreach (Rigidbody rb in _rigidbodiesOnButton)
                totalMass += rb.mass;

            return totalMass >= requiredMass;
        }

        foreach (Rigidbody rb in _rigidbodiesOnButton)
        {
            if (rb.mass >= requiredMass)
                return true;
        }

        return false;
    }

    private async UniTaskVoid OpenChest()
    {
        await AnimateChest(
            Quaternion.Euler(openEuler),
            _risingObjectRaisedLocalPosition,
            true,
            openDuration
        );
    }

    private async UniTaskVoid CloseChest()
    {
        await AnimateChest(
            Quaternion.Euler(closedEuler),
            _risingObjectStartLocalPosition,
            false,
            closeDuration
        );
    }

    private async UniTask AnimateChest(
        Quaternion targetHolderRotation,
        Vector3 targetRisingObjectPosition,
        bool pressedVisual,
        float duration
    )
    {
        if (_isAnimating)
            return;

        _isAnimating = true;

        Quaternion holderStartRotation = holderToRotate != null
            ? holderToRotate.localRotation
            : Quaternion.identity;

        Vector3 risingStartPosition = risingObject != null
            ? risingObject.localPosition
            : Vector3.zero;

        Vector3 buttonStartPosition = buttonVisual != null
            ? buttonVisual.localPosition
            : Vector3.zero;

        Vector3 buttonTargetPosition = _buttonStartLocalPosition;

        if (pressedVisual)
            buttonTargetPosition += pressedLocalOffset;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float eased = SmoothStep(t);

            if (holderToRotate != null)
                holderToRotate.localRotation =
                    Quaternion.Slerp(holderStartRotation, targetHolderRotation, eased);

            if (risingObject != null)
                risingObject.localPosition =
                    Vector3.Lerp(risingStartPosition, targetRisingObjectPosition, eased);

            if (buttonVisual != null)
                buttonVisual.localPosition =
                    Vector3.Lerp(buttonStartPosition, buttonTargetPosition, eased);

            await UniTask.Yield();
        }

        if (holderToRotate != null)
            holderToRotate.localRotation = targetHolderRotation;

        if (risingObject != null)
            risingObject.localPosition = targetRisingObjectPosition;

        if (buttonVisual != null)
            buttonVisual.localPosition = buttonTargetPosition;

        _isAnimating = false;

        bool currentShouldBePressed = HasEnoughMass();

        if (currentShouldBePressed != _wantedPressedState)
        {
            _wantedPressedState = currentShouldBePressed;
            _isPressed = currentShouldBePressed;

            if (_isPressed)
                OpenChest().Forget();
            else
                CloseChest().Forget();
        }
    }

    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }
}