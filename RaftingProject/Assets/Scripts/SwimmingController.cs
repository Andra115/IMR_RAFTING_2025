using UnityEngine;

public class SwimmingController : MonoBehaviour
{
    public Transform leftHand;
    public Transform rightHand;
    public Transform head;
    public Rigidbody playerBody;

    public float swimForce = 3.0f;
    public float maxSwimSpeed = 2f;

    public enum DetectionMode { PositionOrRotation, PositionAndRotation, VelocityBased }
    public DetectionMode detectionMode = DetectionMode.PositionOrRotation;

    public float minimumMovementDistance = 0.12f;
    public float minimumRotationAngle = 25f;
    public float noiseThreshold = 0.02f;
    public float minimumVelocity = 0.7f;

    public float strokeCooldown = 0.25f;

    private Vector3 prevLeftPos;
    private Vector3 prevRightPos;
    private Quaternion prevLeftRot;
    private Quaternion prevRightRot;

    private float leftHandCooldown = 0f;
    private float rightHandCooldown = 0f;

    void Start()
    {
        playerBody.useGravity = false;
        playerBody.linearDamping = 2f;
        playerBody.angularDamping = 5f;
        playerBody.constraints = RigidbodyConstraints.FreezeRotation;

        prevLeftPos = leftHand.position;
        prevRightPos = rightHand.position;
        prevLeftRot = leftHand.rotation;
        prevRightRot = rightHand.rotation;
    }

    void FixedUpdate()
    {
        if (leftHandCooldown > 0) leftHandCooldown -= Time.deltaTime;
        if (rightHandCooldown > 0) rightHandCooldown -= Time.deltaTime;

        HandleSwimming();
        ClampHorizontalSpeed();
        UpdatePreviousState();
    }

    void HandleSwimming()
    {
        // Direction player will move
        Vector3 swimDirection = head.forward;
        swimDirection.y = 0; // stays at same height
        swimDirection.Normalize();

        HandleHand(leftHand, ref leftHandCooldown, prevLeftPos, prevLeftRot);
        HandleHand(rightHand, ref rightHandCooldown, prevRightPos, prevRightRot);
    }

    void HandleHand(Transform hand, ref float cooldown, Vector3 prevPos, Quaternion prevRot)
    {
        if (cooldown > 0)
            return;

        float linearMovement = Vector3.Distance(hand.position, prevPos);
        float rotationAngle = Quaternion.Angle(prevRot, hand.rotation);
        float velocity = linearMovement / Time.fixedDeltaTime;

        // FILTER tracking noise
        if (linearMovement < noiseThreshold)
            return;

        bool triggered = false;

        switch (detectionMode)
        {
            case DetectionMode.VelocityBased:
                triggered = velocity > minimumVelocity;
                break;

            case DetectionMode.PositionOrRotation:
                triggered =
                    linearMovement > minimumMovementDistance ||
                    rotationAngle > minimumRotationAngle;
                break;

            case DetectionMode.PositionAndRotation:
                triggered =
                    linearMovement > minimumMovementDistance &&
                    rotationAngle > minimumRotationAngle;
                break;
        }

        if (!triggered)
            return;

        // Swimming pushes the body forward
        Vector3 dir = head.forward;
        dir.y = 0;
        dir.Normalize();

        playerBody.AddForce(dir * swimForce, ForceMode.VelocityChange);

        cooldown = strokeCooldown;
    }

    void ClampHorizontalSpeed()
    {
        Vector3 vel = playerBody.linearVelocity;
        Vector3 horizontal = new Vector3(vel.x, 0, vel.z);

        if (horizontal.magnitude > maxSwimSpeed)
        {
            horizontal = horizontal.normalized * maxSwimSpeed;
            playerBody.linearVelocity = new Vector3(horizontal.x, vel.y, horizontal.z);
        }
    }

    void UpdatePreviousState()
    {
        prevLeftPos = leftHand.position;
        prevRightPos = rightHand.position;
        prevLeftRot = leftHand.rotation;
        prevRightRot = rightHand.rotation;
    }
}
