using UnityEngine;

public class TentacleMovement : MonoBehaviour
{
    [Header("Tentacle Settings")]
    public Transform[] rayOrigins;         // 2 total: left and right
    public Transform[] tentacleTargets;    // 4 total: 0-1 left, 2-3 right

    [Header("Raycast Settings")]
    public float rayDistance = 10f;
    public LayerMask hitLayers;
    public int raysPerArm = 5;
    public float startAngle = 90f;
    public float endAngle = 270f;
    public float verticalStart = -10f;
    public float verticalEnd = 30f;
    public float moveSmoothness = 5f;

    [Header("Lock Settings")]
    public float maxTargetMoveDistance = 1.5f;
    public float targetLostTimeout = 0.5f;

    private Vector3?[] lockedPoints;          // Now only per ray origin
    private float[] timeSinceLastHit;
    private Vector3[][] rayDirections;        // precomputed directions per ray origin

    void Start()
    {
        lockedPoints = new Vector3?[rayOrigins.Length];
        timeSinceLastHit = new float[rayOrigins.Length];
        rayDirections = new Vector3[rayOrigins.Length][];

        // Precompute deterministic ray directions for each origin
        for (int i = 0; i < rayOrigins.Length; i++)
        {
            rayDirections[i] = new Vector3[raysPerArm];
            for (int r = 0; r < raysPerArm; r++)
            {
                float angleH = Mathf.Lerp(startAngle, endAngle, (float)r / (raysPerArm - 1));
                float angleV = Mathf.Lerp(verticalStart, verticalEnd, (float)r / (raysPerArm - 1));
                rayDirections[i][r] = Quaternion.Euler(angleV, angleH, 0) * Vector3.up; // using up instead of forward
            }
        }
    }

    void Update()
    {
        for (int side = 0; side < rayOrigins.Length; side++)
        {
            Transform origin = rayOrigins[side];

            // 1️ Check if locked target is still valid
            if (lockedPoints[side].HasValue)
            {
                Vector3 currentLock = lockedPoints[side].Value;

                if (Physics.Linecast(origin.position, currentLock, out RaycastHit lockHit, hitLayers))
                {
                    float moveDelta = Vector3.Distance(lockHit.point, currentLock);

                    if (moveDelta > maxTargetMoveDistance)
                    {
                        lockedPoints[side] = null; // moved too far
                    }
                    else
                    {
                        lockedPoints[side] = lockHit.point; // keep tracking same point
                        timeSinceLastHit[side] = 0f;
                    }
                }
                else
                {
                    timeSinceLastHit[side] += Time.deltaTime;
                    if (timeSinceLastHit[side] > targetLostTimeout)
                        lockedPoints[side] = null; // lost contact
                }
            }

            // 2️ If no lock, search deterministically
            if (!lockedPoints[side].HasValue)
            {
                Vector3 bestHitPoint = Vector3.zero;
                float bestDistance = Mathf.Infinity;
                bool foundHit = false;

                for (int r = 0; r < raysPerArm; r++)
                {
                    Vector3 dir = origin.rotation * rayDirections[side][r];

                    if (Physics.Raycast(origin.position, dir, out RaycastHit hit, rayDistance, hitLayers))
                    {
                        if (hit.distance < bestDistance)
                        {
                            bestDistance = hit.distance;
                            bestHitPoint = hit.point;
                            foundHit = true;
                        }
                        Debug.DrawLine(origin.position, hit.point, Color.green);
                    }
                    else
                    {
                        Debug.DrawRay(origin.position, dir * rayDistance, Color.red);
                    }
                }

                if (foundHit)
                {
                    lockedPoints[side] = bestHitPoint;
                    timeSinceLastHit[side] = 0f;
                }
            }

            // 3️ Move BOTH tentacles for this side to lock
            if (lockedPoints[side].HasValue)
            {
                int firstIndex = side * 2; // Left side: 0, Right side: 2
                int secondIndex = firstIndex + 1;

                if (firstIndex < tentacleTargets.Length)
                {
                    tentacleTargets[firstIndex].position = Vector3.Lerp(
                        tentacleTargets[firstIndex].position,
                        lockedPoints[side].Value,
                        Time.deltaTime * moveSmoothness
                    );
                }
                if (secondIndex < tentacleTargets.Length)
                {
                    tentacleTargets[secondIndex].position = Vector3.Lerp(
                        tentacleTargets[secondIndex].position,
                        lockedPoints[side].Value,
                        Time.deltaTime * moveSmoothness
                    );
                }
            }
        }
    }
}
