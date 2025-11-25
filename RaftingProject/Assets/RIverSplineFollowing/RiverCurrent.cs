using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class RaftRiverFlow : MonoBehaviour
{
    [Header("Movement")]
    public SplineContainer riverSpline;
    public float flowStrength = 10f;
    public float turbulence = 0.5f;
    
    [Header("Height Lock")]
    [Tooltip("Boat will stay EXACTLY at this Y position")]
    public float targetHeight = 0f;
    
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("❌ NO RIGIDBODY!");
        if (riverSpline == null) Debug.LogError("❌ NO SPLINE!");
        Debug.Log("✅ Height-locked raft flow active");
    }

    void FixedUpdate()
    {
        // Apply river flow
        Vector3 flowDirection = GetFlowDirection(transform.position);
        Debug.DrawRay(transform.position, flowDirection * 3f, Color.red, 0.1f);
        ApplyRiverFlow(flowDirection);
        
        // LOCK HEIGHT (prevents floating onto land)
        LockHeight();
    }

    void LockHeight()
    {
        // 1. Zero vertical velocity
        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;
        
        // 2. Snap to exact height
        Vector3 pos = transform.position;
        pos.y = targetHeight;
        transform.position = pos;
    }

    Vector3 GetFlowDirection(Vector3 boatPos)
    {
        SplineUtility.GetNearestPoint(riverSpline.Spline, boatPos, 
            out _, out float nearestT);
        
        float3 tangent = riverSpline.Spline.EvaluateTangent(nearestT);
        return math.normalize(tangent);
    }

    void ApplyRiverFlow(Vector3 direction)
    {
        Vector3 flowForce = direction * flowStrength;
        flowForce.y = 0f; // Ensure no vertical force
        
        // Add turbulence
        flowForce += new Vector3(
            Random.Range(-turbulence, turbulence),
            0,
            Random.Range(-turbulence, turbulence)
        );
        
        rb.AddForce(flowForce, ForceMode.Acceleration);
    }
}