using UnityEngine;

public class CurvedRaycast : MonoBehaviour
{
    [Header("Curve Settings")]
    public Transform startPoint;
    public Transform endPoint;
    public float curveHeight = 2f;
    public int segments = 20;

    [Header("Raycast Settings")]
    public float rayRadius = 0.1f; // for SphereCast
    public LayerMask hitLayers;

    void Update()
    {
        CastCurvedRay();
    }

    void CastCurvedRay()
    {
        Vector3 previousPoint = startPoint.position;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 currentPoint = GetParabolaPoint(startPoint.position, endPoint.position, curveHeight, t);

            // Draw a debug line
            Debug.DrawLine(previousPoint, currentPoint, Color.green);

            // Cast a short ray (or sphere) from previous point to current
            Vector3 direction = (currentPoint - previousPoint).normalized;
            float distance = Vector3.Distance(previousPoint, currentPoint);

            if (Physics.SphereCast(previousPoint, rayRadius, direction, out RaycastHit hit, distance, hitLayers))
            {
                Debug.Log("Hit: " + hit.collider.name);
                // Optional: stop after first hit
                break;
            }

            previousPoint = currentPoint;
        }
    }

    Vector3 GetParabolaPoint(Vector3 start, Vector3 end, float height, float t)
    {
        // Basic parabolic formula: (1-t)*start + t*end + curve height offset
        Vector3 mid = Vector3.Lerp(start, end, t);
        float parabola = 4 * height * t * (1 - t); // peaks at t=0.5
        mid.y += parabola;
        return mid;
    }
}
