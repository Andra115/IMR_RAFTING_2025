using UnityEngine;

public class SwimmingController : MonoBehaviour
{
    public Transform leftHand;
    public Transform rightHand;
    public Transform head;
    public Transform waterSurface;
    public Rigidbody playerBody;

    public float swimForce = 3.5f;
    public float maxSwimSpeed = 2f;
    public float headAboveWater = 0.1f;

    public enum DetectionMode { PositionOrRotation, PositionAndRotation, VelocityBased }
    public DetectionMode detectionMode = DetectionMode.PositionOrRotation;

    public float minimumMovementDistance = 0.1f;      // raised from 0.01f to ignore jitter
    public float minimumRotationAngle = 20f;
    public float noiseThreshold = 0.01f;               // raised slightly to kill tiny jitter

    public float minimumVelocity = 0.5f;

    public bool enableDebugLogs = true;

    private Vector3 prevLeftPos;
    private Vector3 prevRightPos;
    private Quaternion prevLeftRot;
    private Quaternion prevRightRot;

    private float waterLevelY;

    private float leftHandCooldown = 0f;
    private float rightHandCooldown = 0f;
    public float strokeCooldown = 0.25f;

    void Start()
    {
        waterLevelY = waterSurface.position.y + (waterSurface.localScale.y / 2f);

        playerBody.useGravity = false;
        playerBody.linearDamping = 2f;
        playerBody.angularDamping = 5f;
        playerBody.constraints = RigidbodyConstraints.FreezeRotation;

        prevLeftPos = leftHand.position;
        prevRightPos = rightHand.position;
        prevLeftRot = leftHand.rotation;
        prevRightRot = rightHand.rotation;

        Debug.Log("Swimming Controller Initialized. Water Level Y: " + waterLevelY);
    }

    void FixedUpdate()
    {
        if (leftHandCooldown > 0) leftHandCooldown -= Time.fixedDeltaTime;
        if (rightHandCooldown > 0) rightHandCooldown -= Time.fixedDeltaTime;

        HandleSwimming();
        KeepAtSurface();
        ClampHorizontalSpeed();
        UpdatePreviousState();
    }

    void HandleSwimming()
    {
        Vector3 swimDirection = head.forward;
        swimDirection.y = 0;
        swimDirection.Normalize();

        if (swimDirection.magnitude < 0.1f) return;

        HandleHand(
            leftHand,
            ref leftHandCooldown,
            prevLeftPos,
            prevLeftRot,
            "LEFT HAND"
        );

        HandleHand(
            rightHand,
            ref rightHandCooldown,
            prevRightPos,
            prevRightRot,
            "RIGHT HAND"
        );
    }

    void HandleHand(
        Transform hand,
        ref float cooldown,
        Vector3 prevPos,
        Quaternion prevRot,
        string label
    )
    {
        if (cooldown > 0) return;

        float linearMovement = Vector3.Distance(hand.position, prevPos);
        float rotationAngle = Quaternion.Angle(prevRot, hand.rotation);
        float velocity = linearMovement / Time.fixedDeltaTime;

        // Jitter filtering
        bool movementPastNoise = linearMovement > noiseThreshold;
        bool rotationPastNoise = rotationAngle > 1f;

        if (enableDebugLogs && (movementPastNoise || rotationPastNoise))
        {
            Debug.Log(
                $"[{label}] Move: {linearMovement:F4}m, Rot: {rotationAngle:F2}°, Vel: {velocity:F2} m/s"
            );
        }

        bool triggered = false;

        switch (detectionMode)
        {
            case DetectionMode.VelocityBased:
                triggered = velocity > minimumVelocity;
                break;

            case DetectionMode.PositionOrRotation:
            
                triggered =
                    (linearMovement > minimumMovementDistance) ||
                    (rotationAngle > minimumRotationAngle);
                
                Debug.Log(
                $"[{triggered}] Move: {linearMovement:F4}m, Rot: {rotationAngle:F2}°, Vel: {velocity:F2} m/s"
            );
                break;

            case DetectionMode.PositionAndRotation:
                triggered =
                    (linearMovement > minimumMovementDistance) &&
                    (rotationAngle > minimumRotationAngle);
                break;
        }

        if (triggered)
        {
            Vector3 swimDir = head.forward;
            swimDir.y = 0;
            swimDir.Normalize();

            ApplySwimForce(swimDir, $"{label} (Move: {linearMovement:F3}m, Rot: {rotationAngle:F1}°)");
            cooldown = strokeCooldown;
        }
    }

    void UpdatePreviousState()
    {
        prevLeftPos = leftHand.position;
        prevRightPos = rightHand.position;
        prevLeftRot = leftHand.rotation;
        prevRightRot = rightHand.rotation;
    }

    void ApplySwimForce(Vector3 direction, string handSource)
    {
        //Debug.Log($"<color=green>[SWIM STROKE APPLIED]</color> {handSource} -> Direction: {direction}");
        playerBody.AddForce(direction * swimForce, ForceMode.VelocityChange);
    }

    void ClampHorizontalSpeed()
    {
        Vector3 current = playerBody.linearVelocity;
        Vector3 horizontal = new Vector3(current.x, 0, current.z);

        if (horizontal.magnitude > maxSwimSpeed)
        {
            horizontal = horizontal.normalized * maxSwimSpeed;
            playerBody.linearVelocity = new Vector3(horizontal.x, current.y, horizontal.z);
        }
    }

    void KeepAtSurface()
    {
        float targetY = waterLevelY + headAboveWater - head.localPosition.y;

        Vector3 newPos = new Vector3(
            playerBody.position.x,
            targetY,
            playerBody.position.z
        );

        playerBody.MovePosition(newPos);
    }
}