using UnityEngine;
using System.Collections.Generic;

public class RiverSpline : MonoBehaviour
{
    [Header("Spline Points")]
    [Tooltip("Add empty GameObjects along your river path in order")]
    public List<Transform> splinePoints = new List<Transform>();
    
    [Header("Spline Settings")]
    [Tooltip("How many interpolated points between each control point")]
    public int resolution = 10;
    
    [Header("Debug Visualization")]
    public bool showSpline = true;
    public Color splineColor = Color.green;
    
    private List<Vector3> cachedSplinePositions = new List<Vector3>();
    
    private void Start()
    {
        GenerateSpline();
    }
    
    private void OnValidate()
    {
        // Regenerate when values change in editor
        GenerateSpline();
    }
    
    // Generate smooth spline from control points
    public void GenerateSpline()
    {
        cachedSplinePositions.Clear();
        
        if (splinePoints.Count < 2) return;
        
        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            Vector3 p0 = splinePoints[Mathf.Max(i - 1, 0)].position;
            Vector3 p1 = splinePoints[i].position;
            Vector3 p2 = splinePoints[i + 1].position;
            Vector3 p3 = splinePoints[Mathf.Min(i + 2, splinePoints.Count - 1)].position;
            
            for (int j = 0; j < resolution; j++)
            {
                float t = j / (float)resolution;
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                cachedSplinePositions.Add(point);
            }
        }
        
        // Add final point
        if (splinePoints.Count > 0)
            cachedSplinePositions.Add(splinePoints[splinePoints.Count - 1].position);
    }
    
    // Get closest point on spline to a given position
    public Vector3 GetClosestPoint(Vector3 position, out float t)
    {
        if (cachedSplinePositions.Count == 0)
        {
            t = 0;
            return position;
        }
        
        float minDist = float.MaxValue;
        int closestIndex = 0;
        
        for (int i = 0; i < cachedSplinePositions.Count; i++)
        {
            float dist = Vector3.Distance(position, cachedSplinePositions[i]);
            if (dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }
        
        t = closestIndex / (float)(cachedSplinePositions.Count - 1);
        return cachedSplinePositions[closestIndex];
    }
    
    // Get direction (tangent) at a point on the spline
    public Vector3 GetDirection(float t)
    {
        if (cachedSplinePositions.Count < 2) return Vector3.forward;
        
        int index = Mathf.FloorToInt(t * (cachedSplinePositions.Count - 1));
        index = Mathf.Clamp(index, 0, cachedSplinePositions.Count - 2);
        
        Vector3 direction = (cachedSplinePositions[index + 1] - cachedSplinePositions[index]).normalized;
        return direction;
    }
    
    // Catmull-Rom spline interpolation for smooth curves
    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
    
    private void OnDrawGizmos()
    {
        if (!showSpline || cachedSplinePositions.Count < 2) return;
        
        // Draw the spline path
        Gizmos.color = splineColor;
        for (int i = 0; i < cachedSplinePositions.Count - 1; i++)
        {
            Gizmos.DrawLine(cachedSplinePositions[i], cachedSplinePositions[i + 1]);
        }
        
        // Draw control points as red spheres
        Gizmos.color = Color.red;
        foreach (Transform point in splinePoints)
        {
            if (point != null)
                Gizmos.DrawSphere(point.position, 0.3f);
        }
    }
}