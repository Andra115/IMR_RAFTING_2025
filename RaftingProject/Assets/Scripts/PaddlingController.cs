using UnityEngine;

public class PaddlingController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody boatRigidbody;
    public Transform leftHand;   // Paddle tip transform
    public Transform rightHand;  // Paddle tip transform
    
    [Header("Paddle Objects & Holsters")]
    public GameObject leftPaddle;
    public GameObject rightPaddle;
    public Transform leftPaddleHolster;   // Where left paddle docks
    public Transform rightPaddleHolster;  // Where right paddle docks
    
    [Header("Paddle Settings")]
    public float paddleForwardForce = 5.0f;
    public float paddleTurnTorque = 2.0f;
    public float maxBoatSpeed = 8f;
    
    [Header("Detection Settings")]
    public float minimumMovementDistance = 0.12f;
    public float strokeCooldown = 0.25f;

    // State
    private bool isHoldingLeft = false;
    private bool isHoldingRight = false;

    // Tracking
    private Vector3 prevLeftPos;
    private Vector3 prevRightPos;
    private float leftHandCooldown = 0f;
    private float rightHandCooldown = 0f;

    void Start()
    {
        if(leftHand) prevLeftPos = leftHand.position;
        if(rightHand) prevRightPos = rightHand.position;

        // Dock paddles at start
        DockPaddle(leftPaddle, leftPaddleHolster);
        DockPaddle(rightPaddle, rightPaddleHolster);
    }

    void FixedUpdate()
    {
        if (leftHandCooldown > 0) leftHandCooldown -= Time.deltaTime;
        if (rightHandCooldown > 0) rightHandCooldown -= Time.deltaTime;

        HandlePaddling();
        ClampBoatSpeed();
        UpdatePreviousState();
    }

    void HandlePaddling()
    {
        if (isHoldingLeft && CheckMovement(leftHand, ref leftHandCooldown, prevLeftPos))
        {
            ApplyPaddleForce(1);
        }

        if (isHoldingRight && CheckMovement(rightHand, ref rightHandCooldown, prevRightPos))
        {
            ApplyPaddleForce(-1);
        }
    }

    bool CheckMovement(Transform hand, ref float cooldown, Vector3 prevPos)
    {
        if (cooldown > 0) return false;
        if (hand == null) return false;

        float linearMovement = Vector3.Distance(hand.position, prevPos);

        if (linearMovement > minimumMovementDistance)
        {
            cooldown = strokeCooldown;
            return true;
        }
        return false;
    }

    void ApplyPaddleForce(int side)
    {
        Vector3 forwardDir = boatRigidbody.transform.forward;
        boatRigidbody.AddForce(forwardDir * paddleForwardForce, ForceMode.VelocityChange);

        Vector3 rotationDir = Vector3.up * (paddleTurnTorque * side);
        boatRigidbody.AddTorque(rotationDir, ForceMode.VelocityChange);
    }

    void ClampBoatSpeed()
    {
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

    void DockPaddle(GameObject paddle, Transform holster)
    {
        if(paddle == null || holster == null) return;
        
        // Parent to holster
        paddle.transform.SetParent(holster);
        paddle.transform.localPosition = Vector3.zero;
        paddle.transform.localRotation = Quaternion.identity;
        
        // Freeze ALL position and rotation
        Rigidbody rb = paddle.GetComponent<Rigidbody>();
        if(rb)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void UndockPaddle(GameObject paddle)
    {
        if(paddle == null) return;
        
        // Unparent
        paddle.transform.SetParent(null);
        
        // Unfreeze ALL constraints
        Rigidbody rb = paddle.GetComponent<Rigidbody>();
        if(rb)
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    // --- PUBLIC METHODS FOR XR GRAB EVENTS ---

    public void SetLeftPaddleState(bool isHeld)
    {
        isHoldingLeft = isHeld;
    }

    public void SetRightPaddleState(bool isHeld)
    {
        isHoldingRight = isHeld;
    }
    
    public void OnLeftPaddleGrabbed()
    {
        SetLeftPaddleState(true);
        UndockPaddle(leftPaddle);
        
        // Disable collider when grabbed
        Collider col = leftPaddle.GetComponent<Collider>();
        if(col) col.enabled = false;
    }

    public void OnLeftPaddleReleased()
    {
        SetLeftPaddleState(false);
        
        // Re-enable collider before docking
        Collider col = leftPaddle.GetComponent<Collider>();
        if(col) col.enabled = true;
        
        DockPaddle(leftPaddle, leftPaddleHolster);
    }

    public void OnRightPaddleGrabbed()
    {
        SetRightPaddleState(true);
        UndockPaddle(rightPaddle);
        
        Collider col = rightPaddle.GetComponent<Collider>();
        if(col) col.enabled = false;
    }

    public void OnRightPaddleReleased()
    {
        SetRightPaddleState(false);
        
        Collider col = rightPaddle.GetComponent<Collider>();
        if(col) col.enabled = true;
        
        DockPaddle(rightPaddle, rightPaddleHolster);
    }
}