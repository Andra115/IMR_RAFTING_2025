using UnityEngine;

public class RiverBoat : MonoBehaviour
{
    [Header("Height Lock Settings")]
    public float boatHeight = 1f;
    
    [Header("References")]
    public Rigidbody rb;
    
    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        if (rb == null) return;
        
        // ONLY lock Y position (keep boat at water level)
        Vector3 pos = transform.position;
        pos.y = boatHeight;
        transform.position = pos;
        
        // ONLY zero vertical velocity
        Vector3 vel = rb.linearVelocity;
        vel.y = 0;
        rb.linearVelocity = vel;
    }
}