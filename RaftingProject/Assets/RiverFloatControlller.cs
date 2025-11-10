using UnityEngine;
using UnityEngine.InputSystem;

public class RiverFloatController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed in units per second")]
    public float moveSpeed = 10f;

    [Tooltip("Optional: Input Action for movement")]
    public InputAction moveAction; // Should be Vector2 type for WASD / joystick

    private void OnEnable()
    {
        if (moveAction != null) moveAction.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.Disable();
    }

    private void Update()
    {
        Vector2 input = Vector2.zero;

        if (moveAction != null)
        {
            input = moveAction.ReadValue<Vector2>();
        }
        else
        {
            // Fallback to standard keyboard
            input.y = Input.GetKey(KeyCode.W) ? 1f : 0f;
            input.y -= Input.GetKey(KeyCode.S) ? 1f : 0f;
            input.x = Input.GetKey(KeyCode.D) ? 1f : 0f;
            input.x -= Input.GetKey(KeyCode.A) ? 1f : 0f;
        }

        Vector3 move = transform.forward * input.y + transform.right * input.x;
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}