// RiverBoat.cs
// Attach this to your boat GameObject

using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class RiverBoat : MonoBehaviour
{
    [Header("River Flow Settings")]
    [Tooltip("Drag your Spline Container GameObject here")]
    public SplineContainer riverSpline;
    
    [Tooltip("How fast the river current pushes the boat")]
    public float flowSpeed = 5f;
    
    [Header("Height Lock Settings")]
    [Tooltip("The Y position where the boat will stay locked")]
    public float boatHeight = 1f;
    
    [Header("Rotation Lock Settings")]
    [Tooltip("Lock the boat's rotation on X axis (prevent tipping forward/backward)")]
    public bool lockRotationX = true;
    
    [Tooltip("Lock the boat's rotation on Z axis (prevent tipping left/right)")]
    public bool lockRotationZ = true;
    
    [Tooltip("Should the boat rotate to face downstream?")]
    public bool rotateToFlowDirection = true;
    
    [Tooltip("How fast the boat rotates to face downstream")]
    public float rotationSpeed = 2f;
    
    [Header("Buoyancy Settings")]
    [Tooltip("Enable gentle bobbing motion")]
    public bool enableBuoyancy = true;
    
    [Tooltip("How high the boat bobs up and down (very small values!)")]
    [Range(0.01f, 0.2f)]
    public float buoyancyHeight = 0.05f;
    
    [Tooltip("How fast the boat bobs")]
    [Range(0.5f, 3f)]
    public float buoyancySpeed = 1.5f;
    
    [Tooltip("Maximum tilt angle from buoyancy (degrees)")]
    [Range(0f, 5f)]
    public float maxBuoyancyTilt = 2f;
    
    private float buoyancyTime = 0f;
    
    [Header("References")]
    [Tooltip("Drag your boat's Rigidbody here, or leave empty to auto-find")]
    public Rigidbody rb;
    
    [Header("Debug")]
    public bool showDebugLines = true;
    
    private void Start()
    {
        // Auto-find rigidbody if not set
        if (rb == null)
            rb = GetComponent<Rigidbody>();
            
        if (rb == null)
        {
            Debug.LogError("RiverBoat needs a Rigidbody component!");
        }
        
        if (riverSpline == null)
        {
            Debug.LogError("RiverBoat needs a Spline Container assigned!");
        }
    }
    
    private void FixedUpdate()
    {
        if (riverSpline == null || rb == null) return;
        
        // Get the spline from the container
        Spline spline = riverSpline.Spline;
        if (spline == null) return;
        
        // Find closest point on spline to boat (in local space)
        Vector3 localPos = riverSpline.transform.InverseTransformPoint(transform.position);
        SplineUtility.GetNearestPoint(spline, localPos, out float3 nearestPoint, out float t);
        
        // Convert back to world space
        Vector3 worldNearestPoint = riverSpline.transform.TransformPoint(nearestPoint);
        
        // Get the tangent (flow direction) at this point
        float3 localTangent = spline.EvaluateTangent(t);
        Vector3 worldTangent = riverSpline.transform.TransformDirection(localTangent).normalized;
        
        // Apply flow force downstream
        rb.AddForce(worldTangent * flowSpeed, ForceMode.Force);
        
        // Calculate buoyancy offset
        float buoyancyOffset = 0f;
        float buoyancyTiltX = 0f;
        float buoyancyTiltZ = 0f;
        
        if (enableBuoyancy)
        {
            buoyancyTime += Time.fixedDeltaTime * buoyancySpeed;
            
            // Gentle bobbing using sine wave
            buoyancyOffset = Mathf.Sin(buoyancyTime) * buoyancyHeight;
            
            // Very subtle rocking motion
            buoyancyTiltX = Mathf.Sin(buoyancyTime * 0.7f) * maxBuoyancyTilt;
            buoyancyTiltZ = Mathf.Cos(buoyancyTime * 0.5f) * maxBuoyancyTilt * 0.5f;
        }
        
        // Lock boat to specific height with buoyancy
        Vector3 pos = transform.position;
        pos.y = boatHeight + buoyancyOffset;
        transform.position = pos;
        
        // Zero out vertical velocity to prevent floating
        Vector3 vel = rb.linearVelocity;
        vel.y = 0;
        rb.linearVelocity = vel;
        
        // Handle rotation
        Quaternion targetRotation = Quaternion.identity;
        
        if (rotateToFlowDirection)
        {
            // Smoothly rotate to face downstream
            targetRotation = Quaternion.LookRotation(worldTangent);
        }
        else
        {
            targetRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
        
        // Apply buoyancy tilt if enabled (but very gently)
        if (enableBuoyancy && !lockRotationX && !lockRotationZ)
        {
            targetRotation *= Quaternion.Euler(buoyancyTiltX, 0, buoyancyTiltZ);
        }
        else if (enableBuoyancy)
        {
            // Apply limited tilt even with locks
            float tiltX = lockRotationX ? 0 : buoyancyTiltX;
            float tiltZ = lockRotationZ ? 0 : buoyancyTiltZ;
            targetRotation *= Quaternion.Euler(tiltX, 0, tiltZ);
        }
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        
        // Lock specific rotation axes if needed
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        if (lockRotationX)
            eulerAngles.x = 0;
        if (lockRotationZ)
            eulerAngles.z = 0;
        transform.rotation = Quaternion.Euler(eulerAngles);
        
        // Zero out angular velocity to prevent spinning
        rb.angularVelocity = Vector3.zero;
        
        // Debug visualization
        if (showDebugLines)
        {
            Debug.DrawLine(transform.position, worldNearestPoint, Color.cyan);
            Debug.DrawRay(worldNearestPoint, worldTangent * 3f, Color.blue);
        }
    }
}