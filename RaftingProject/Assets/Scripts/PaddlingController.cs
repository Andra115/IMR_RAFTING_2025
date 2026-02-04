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
    public Transform leftPaddleHolster;
    public Transform rightPaddleHolster;

    [Header("Paddle Settings")]
    public float paddleForwardForce = 5.0f;
    public float paddleTurnTorque = 2.0f;
    public float maxBoatSpeed = 8f;

    [Header("Detection Settings")]
    public float minimumMovementDistance = 0.12f;
    public float strokeCooldown = 0.25f;

    [Header("Water Effects")]
    public GameObject splashEffectPrefab;  // Drag your particle effect here
    public float waterLevel = 0f;          // Y position of water surface
    public AudioClip[] paddleSplashSounds; // Array of splash sound clips
    public AudioSource audioSource;        // Audio source for playing sounds
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;
    public float splashSoundCooldown = 1.5f; // Cooldown between splash sounds

    // State
    private bool isHoldingLeft = false;
    private bool isHoldingRight = false;

    // Tracking
    private Vector3 prevLeftPos;
    private Vector3 prevRightPos;
    private float leftHandCooldown = 0f;
    private float rightHandCooldown = 0f;
    private float splashSoundTimer = 0f; // Timer for sound cooldown

    void Start()
    {
        if (leftHand) prevLeftPos = leftHand.position;
        if (rightHand) prevRightPos = rightHand.position;

        // Auto-create audio source if not assigned
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.maxDistance = 20f;
        }

        // Dock paddles at start
        DockPaddle(leftPaddle, leftPaddleHolster);
        DockPaddle(rightPaddle, rightPaddleHolster);
    }

    void FixedUpdate()
    {
        if (leftHandCooldown > 0) leftHandCooldown -= Time.deltaTime;
        if (rightHandCooldown > 0) rightHandCooldown -= Time.deltaTime;
        if (splashSoundTimer > 0) splashSoundTimer -= Time.deltaTime;

        HandlePaddling();
        ClampBoatSpeed();
        UpdatePreviousState();
    }

    void HandlePaddling()
    {
        if (isHoldingLeft && CheckMovement(leftHand, ref leftHandCooldown, prevLeftPos))
        {
            ApplyPaddleForce(1);
            PlaySplashEffect(leftHand.position); // Play splash at paddle tip
        }

        if (isHoldingRight && CheckMovement(rightHand, ref rightHandCooldown, prevRightPos))
        {
            ApplyPaddleForce(-1);
            PlaySplashEffect(rightHand.position);
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

    void PlaySplashEffect(Vector3 paddlePosition)
    {
        Debug.Log($"PlaySplashEffect called at position: {paddlePosition}, Water level: {waterLevel}");

        // Only play splash if paddle is near/below water level
        if (paddlePosition.y > waterLevel + 0.3f)
        {
            Debug.Log($"Paddle too high! Paddle Y: {paddlePosition.y}, Water threshold: {waterLevel + 0.3f}");
            return;
        }

        Debug.Log("Paddle is in water - spawning splash!");

        // Spawn splash particle effect at water level
        if (splashEffectPrefab != null)
        {
            Vector3 splashPos = new Vector3(paddlePosition.x, waterLevel, paddlePosition.z);
            GameObject splash = Instantiate(splashEffectPrefab, splashPos, Quaternion.identity);
            Debug.Log($"Splash spawned at {splashPos}");
            Destroy(splash, 3f);
        }
        else
        {
            Debug.LogWarning("Splash Effect Prefab is NULL!");
        }

        // Play random splash sound with cooldown
        if (splashSoundTimer <= 0f && paddleSplashSounds != null && paddleSplashSounds.Length > 0 && audioSource != null)
        {
            AudioClip randomClip = paddleSplashSounds[Random.Range(0, paddleSplashSounds.Length)];
            if (randomClip != null)
            {
                audioSource.PlayOneShot(randomClip, soundVolume);
                splashSoundTimer = splashSoundCooldown; // Reset cooldown timer
                Debug.Log($"Playing sound: {randomClip.name} at volume {soundVolume}");
            }
            else
            {
                Debug.LogWarning("Random clip is NULL!");
            }
        }
        else if (splashSoundTimer > 0f)
        {
            Debug.Log($"Sound on cooldown. Time remaining: {splashSoundTimer:F2}s");
        }
        else
        {
            Debug.LogWarning($"Sound issue - Sounds array: {paddleSplashSounds?.Length ?? 0}, AudioSource: {(audioSource != null ? "EXISTS" : "NULL")}");
        }
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
        if (leftHand) prevLeftPos = leftHand.position;
        if (rightHand) prevRightPos = rightHand.position;
    }

    void DockPaddle(GameObject paddle, Transform holster)
    {
        if (paddle == null || holster == null) return;

        paddle.transform.SetParent(holster);
        paddle.transform.localPosition = Vector3.zero;
        paddle.transform.localRotation = Quaternion.identity;

        Rigidbody rb = paddle.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void UndockPaddle(GameObject paddle)
    {
        if (paddle == null) return;

        paddle.transform.SetParent(null);

        Rigidbody rb = paddle.GetComponent<Rigidbody>();
        if (rb)
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

        Collider col = leftPaddle.GetComponent<Collider>();
        if (col) col.enabled = false;
    }

    public void OnLeftPaddleReleased()
    {
        SetLeftPaddleState(false);

        Collider col = leftPaddle.GetComponent<Collider>();
        if (col) col.enabled = true;

        DockPaddle(leftPaddle, leftPaddleHolster);
    }

    public void OnRightPaddleGrabbed()
    {
        SetRightPaddleState(true);
        UndockPaddle(rightPaddle);

        Collider col = rightPaddle.GetComponent<Collider>();
        if (col) col.enabled = false;
    }

    public void OnRightPaddleReleased()
    {
        SetRightPaddleState(false);

        Collider col = rightPaddle.GetComponent<Collider>();
        if (col) col.enabled = true;

        DockPaddle(rightPaddle, rightPaddleHolster);
    }
}