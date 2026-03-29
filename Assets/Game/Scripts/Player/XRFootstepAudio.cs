using UnityEngine;
using UnityEngine.InputSystem;

public class XRFootstepAudio : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;   // drag your XRI Default Input Actions asset here

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Tuning")]
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private float minInputMagnitude = 0.2f;
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.15f;

    private InputAction _moveAction;
    private float _stepCooldown;
    private int _lastClipIndex = -1;

    private void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogWarning("[XRFootstepAudio] No Input Actions asset assigned.");
            return;
        }

        // Match exactly what you saw in the screenshot
        _moveAction = inputActions.FindAction("XRI Left Locomotion/Move");

        if (_moveAction == null)
            Debug.LogWarning("[XRFootstepAudio] Could not find 'XRI Left Locomotion/Move'.");
    }

    private void OnEnable()  => _moveAction?.Enable();
    private void OnDisable() => _moveAction?.Disable();

    private void Update()
    {
        if (_moveAction == null) return;

        float magnitude = _moveAction.ReadValue<Vector2>().magnitude;

        if (magnitude < minInputMagnitude)
        {
            _stepCooldown = 0f;
            if (audioSource.isPlaying)
                audioSource.Stop();
            return;
        }

        _stepCooldown -= Time.deltaTime;

        if (_stepCooldown <= 0f)
        {
            PlayStep(magnitude);
            _stepCooldown = Mathf.Lerp(stepInterval, stepInterval * 0.4f, magnitude);
        }
    }

    private void PlayStep(float magnitude)
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        int index;
        do { index = Random.Range(0, footstepClips.Length); }
        while (footstepClips.Length > 1 && index == _lastClipIndex);
        _lastClipIndex = index;

        audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, magnitude);
        audioSource.clip = footstepClips[index];
        audioSource.Play();
    }
}