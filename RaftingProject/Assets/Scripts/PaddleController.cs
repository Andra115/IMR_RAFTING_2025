using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PaddleController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody boatRb;
    public Transform bladePoint;

    [Header("Settings")]
    public float paddleStrength = 50f;

    private Vector3 lastBladePos;
    private bool isInWater = false;
    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    private Rigidbody paddleRb;

    void Start()
    {
        // Get components
        paddleRb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable missing on paddle!");
        }
        else
        {
            // Subscribe to grab events
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }

        if (bladePoint == null)
        {
            Debug.LogWarning("BladePoint is null! Using paddle transform.");
            bladePoint = transform;
        }

        if (boatRb == null)
            Debug.LogError("BoatRb not assigned in Inspector!");

        // Asigură-te că paddle-ul are setările corecte
        if (paddleRb != null)
        {
            paddleRb.useGravity = false;
            paddleRb.isKinematic = false;
            Debug.Log($"Paddle Rigidbody initialized: useGravity={paddleRb.useGravity}, isKinematic={paddleRb.isKinematic}");
        }

        // ⭐ FIX PRINCIPAL: Ignore collision pentru TOATE collider-ele paddle-ului (inclusiv BladePoint)
        if (boatRb != null)
        {
            // Găsește TOATE collider-ele paddle-ului (inclusiv child objects)
            Collider[] paddleColliders = GetComponentsInChildren<Collider>();
            Collider[] boatColliders = boatRb.GetComponentsInChildren<Collider>();

            foreach (Collider paddleCol in paddleColliders)
            {
                foreach (Collider boatCol in boatColliders)
                {
                    Physics.IgnoreCollision(paddleCol, boatCol);
                    Debug.Log($"✓ Ignoring collision: {paddleCol.gameObject.name} <-> {boatCol.gameObject.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning("Could not set up collision ignore - boatRb missing!");
        }

        lastBladePos = bladePoint.position;
        Debug.Log("Paddle Controller initialized successfully!");
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        Debug.Log("✓ PADDLE GRABBED!");
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        Debug.Log("✗ PADDLE RELEASED!");
    }

    void FixedUpdate()
    {
        // Debug periodic (la fiecare 50 frame-uri pentru a nu spama console-ul)
        if (Time.frameCount % 50 == 0)
        {
            Debug.Log($"[Paddle Status] Grabbed: {isGrabbed} | InWater: {isInWater} | Position: {transform.position:F2}");
        }

        // DOAR aplică forța când paddle-ul este GRABBED și în apă
        if (!isGrabbed || !isInWater || boatRb == null)
        {
            lastBladePos = bladePoint.position;
            return;
        }

        ApplyPaddlingForce();
    }

    void ApplyPaddlingForce()
    {
        Vector3 bladeVelocity = (bladePoint.position - lastBladePos) / Time.fixedDeltaTime;
        Vector3 relativeVelocity = bladeVelocity - boatRb.GetPointVelocity(bladePoint.position);
        float backwardAmount = Vector3.Dot(-boatRb.transform.forward, relativeVelocity);

        if (backwardAmount > 0)
        {
            Vector3 force = boatRb.transform.forward * backwardAmount * paddleStrength;
            force.y = 0;
            boatRb.AddForce(force, ForceMode.Force);
            Debug.Log($"⚡ Applying force to boat: magnitude={force.magnitude:F2}, direction={force.normalized}");
        }

        lastBladePos = bladePoint.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Paddle collided with: {other.gameObject.name} (Tag: {other.tag})");

        if (other.CompareTag("Water"))
        {
            isInWater = true;
            Debug.Log("💧 Blade ENTERED water");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = false;
            Debug.Log("💧 Blade EXITED water");
        }
    }

    void OnDestroy()
    {
        // Cleanup events
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    // Debug vizual în Scene view
    void OnDrawGizmos()
    {
        if (bladePoint != null)
        {
            // Arată poziția bladePoint
            Gizmos.color = isInWater ? Color.blue : Color.red;
            Gizmos.DrawWireSphere(bladePoint.position, 0.1f);

            // Arată direcția bărcii dacă există
            if (boatRb != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, boatRb.transform.forward * 0.5f);
            }
        }
    }
}
