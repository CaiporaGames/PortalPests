using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    // Public properties to expose input data
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    // Event for jumping
    public event System.Action OnJumpEvent;

    private Controls controls;

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();

            // Bind input actions
            controls.Player.Move.performed += OnMove;
            controls.Player.Move.canceled += OnMove;
         /*    controls.Player.Look.performed += OnLook;
            controls.Player.Look.canceled += OnLook; */
            controls.Player.Jump.performed += OnJump;
        }

        // Enable controls
        controls.Player.Enable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        // Store movement input (WASD or joystick)
        MoveInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        // Store look input (mouse movement)
        LookInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        // Invoke jump event if action is performed
        if (context.performed)
            OnJumpEvent?.Invoke();
    }

    private void OnDisable()
    {
        // Disable controls when the object is disabled
        controls.Player.Disable();
    }
}
