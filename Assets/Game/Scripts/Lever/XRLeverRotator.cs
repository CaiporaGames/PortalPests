using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class XRLeverRotator : MonoBehaviour
{
    [Header("XR")]
    [SerializeField] private XRSimpleInteractable interactable;

    [Header("Lever Rotation")]
    [SerializeField] private Transform leverToRotate;
    [SerializeField] private Vector3 leverUpLocalEuler = new Vector3(-45f, 0f, 0f);
    [SerializeField] private Vector3 leverDownLocalEuler = new Vector3(45f, 0f, 0f);
    [SerializeField] private float rotateDuration = 0.25f;

    [Header("Behavior")]
    [SerializeField] private bool startsDown = false;
    [SerializeField] private bool ignoreInputWhileRotating = true;

    [Header("Platform Controller")]
    [SerializeField] private BatteryPoweredLeverPlatformController platformController;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    private bool _isDown;
    private bool _isRotating;

    private void Reset()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        leverToRotate = transform;
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (interactable == null)
            interactable = GetComponent<XRSimpleInteractable>();

        if (leverToRotate == null)
            leverToRotate = transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        _isDown = startsDown;

        leverToRotate.localRotation = Quaternion.Euler(
            _isDown ? leverDownLocalEuler : leverUpLocalEuler
        );
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnLeverSelected);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnLeverSelected);
    }

    private void OnLeverSelected(SelectEnterEventArgs args)
    {
        if (_isRotating && ignoreInputWhileRotating)
            return;

        ToggleLever();
    }

    public void ToggleLever()
    {
        SetLeverState(!_isDown);
    }

    public void SetLeverDown()
    {
        SetLeverState(true);
    }

    public void SetLeverUp()
    {
        SetLeverState(false);
    }

    private void SetLeverState(bool down)
    {
        if (_isDown == down && _isRotating == false)
            return;

        _isDown = down;

        audioSource.Play();

        if (_isDown)
            platformController?.SetLeverDown();
        else
            platformController?.SetLeverUp();

        RotateLever(
            Quaternion.Euler(_isDown ? leverDownLocalEuler : leverUpLocalEuler)
        ).Forget();
    }

    private async UniTaskVoid RotateLever(Quaternion targetRotation)
    {
        _isRotating = true;

        Quaternion startRotation = leverToRotate.localRotation;

        float timer = 0f;

        while (timer < rotateDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / rotateDuration);
            float eased = SmoothStep(t);

            leverToRotate.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                eased
            );

            await UniTask.Yield();
        }

        leverToRotate.localRotation = targetRotation;

        _isRotating = false;
    }


    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }
}