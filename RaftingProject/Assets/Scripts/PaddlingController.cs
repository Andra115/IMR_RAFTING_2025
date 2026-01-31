using UnityEngine;

public class PaddlingController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody boatRigidbody;
    public Transform leftHand;   // Your VR Controller (Left)
    public Transform rightHand;  // Your VR Controller (Right)
    
    [Header("Paddle Settings")]
    public float paddleForwardForce = 5.0f;
    public float paddleTurnTorque = 2.0f; // How much we turn per stroke
    public float maxBoatSpeed = 8f;
    
    [Header("Detection Settings (Same as Swimming)")]
    public float minimumMovementDistance = 0.12f;
    public float strokeCooldown = 0.25f;

    // State: We only paddle if we are actually holding the paddles
    private bool isHoldingLeft = false;
    private bool isHoldingRight = false;

    // Tracking for movement detection
    private Vector3 prevLeftPos;
    private Vector3 prevRightPos;
    private float leftHandCooldown = 0f;
    private float rightHandCooldown = 0f;

    void Start()
    {
        // Initialize previous positions to avoid instant movement on start
        if(leftHand) prevLeftPos = leftHand.position;
        if(rightHand) prevRightPos = rightHand.position;
    }

    void FixedUpdate()
    {
        // Update Cooldowns
        if (leftHandCooldown > 0) leftHandCooldown -= Time.deltaTime;
        if (rightHandCooldown > 0) rightHandCooldown -= Time.deltaTime;

        // handle paddling logic
        HandlePaddling();

        // Optional: Keep boat upright or clamp speed
        ClampBoatSpeed();
        
        // Update positions for next frame
        UpdatePreviousState();
    }

    void HandlePaddling()
    {
        // Check Left Hand
        if (isHoldingLeft && CheckMovement(leftHand, ref leftHandCooldown, prevLeftPos))
        {
            ApplyPaddleForce(1); // 1 = Left Side
        }

        // Check Right Hand
        if (isHoldingRight && CheckMovement(rightHand, ref rightHandCooldown, prevRightPos))
        {
            ApplyPaddleForce(-1); // -1 = Right Side
        }
    }

    // Returns true if the hand moved enough to count as a "stroke"
    bool CheckMovement(Transform hand, ref float cooldown, Vector3 prevPos)
    {
        if (cooldown > 0) return false;

        float linearMovement = Vector3.Distance(hand.position, prevPos);

        // Simple distance check (like your PositionOrRotation mode)
        if (linearMovement > minimumMovementDistance)
        {
            cooldown = strokeCooldown; // Reset cooldown
            return true;
        }
        return false;
    }

    // side: 1 for Left (Turns Right), -1 for Right (Turns Left)
    void ApplyPaddleForce(int side)
    {
        // 1. Move Forward (Both paddles push boat forward)
        // We use the BOAT'S forward direction, not the hand's
        Vector3 forwardDir = boatRigidbody.transform.forward;
        boatRigidbody.AddForce(forwardDir * paddleForwardForce, ForceMode.VelocityChange);

        // 2. Rotate (Steering)
        // Left paddle (side 1) adds POSITIVE Torque (Rotates Right)
        // Right paddle (side -1) adds NEGATIVE Torque (Rotates Left)
        // Note: Check your specific axis, usually Y is up.
        Vector3 rotationDir = Vector3.up * (paddleTurnTorque * side);
        boatRigidbody.AddTorque(rotationDir, ForceMode.VelocityChange);
    }

    void ClampBoatSpeed()
    {
        // Preserve vertical velocity (gravity/buoyancy), clamp horizontal
        Vector3 vel = boatRigidbody.linearVelocity;
        Vector3 horizontal = new Vector3(vel.x, 0, vel.z);

        if (horizontal.magnitude > maxBoatSpeed)
        {
            horizontal = horizontal.normalized * maxBoatSpeed;
            boatRigidbody.linearVelocity = new Vector3(horizontal.x, vel.y, horizontal.z);
        }
    }

    void UpdatePreviousState()
    {
        if(leftHand) prevLeftPos = leftHand.position;
        if(rightHand) prevRightPos = rightHand.position;
    }

    // --- PUBLIC METHODS FOR YOUR GRAB SYSTEM ---

    public void SetLeftPaddleState(bool isHeld)
    {
        isHoldingLeft = isHeld;
    }

    public void SetRightPaddleState(bool isHeld)
    {
        isHoldingRight = isHeld;
    }
}