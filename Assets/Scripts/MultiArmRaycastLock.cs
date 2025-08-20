using UnityEngine;

public class MultiArmRaycastLock : MonoBehaviour
{
    [System.Serializable]
    public class ArmSetup
    {
        public Transform rayOrigin;
        public GameObject armObject;
        public float rayDistance = 10f;
        public float moveSpeed = 5f;

        public int raysHorizontal = 5;
        public int raysVertical = 3;
        public float horizontalAngle = 60f;
        public float verticalAngle = 20f;

        public LayerMask hitLayers;

        [HideInInspector] public Vector3? lockedPoint;
    }

    public ArmSetup[] arms;

    private void FixedUpdate()
    {
        foreach (var arm in arms)
        {
            // If we already have a locked point, check if it's still valid
            if (arm.lockedPoint.HasValue)
            {
                Vector3 dir = (arm.lockedPoint.Value - arm.rayOrigin.position).normalized;
                if (!Physics.Raycast(arm.rayOrigin.position, dir, out RaycastHit hit, arm.rayDistance, arm.hitLayers)
                    || Vector3.Distance(hit.point, arm.lockedPoint.Value) > 0.2f)
                {
                    // Lost lock
                    arm.lockedPoint = null;
                }
            }

            // If no locked point, scan
            if (!arm.lockedPoint.HasValue)
            {
                HandleArmScan(arm);
            }

            // Move toward locked point if we have one
            if (arm.lockedPoint.HasValue)
            {
                arm.armObject.transform.position = Vector3.MoveTowards(
                    arm.armObject.transform.position,
                    arm.lockedPoint.Value,
                    arm.moveSpeed * Time.fixedDeltaTime
                );
            }
        }
    }

    private void HandleArmScan(ArmSetup arm)
    {
        float closestDist = Mathf.Infinity;
        Vector3 bestHit = Vector3.zero;

        for (int v = 0; v < arm.raysVertical; v++)
        {
            float vAngle = Mathf.Lerp(-arm.verticalAngle, arm.verticalAngle,
                arm.raysVertical == 1 ? 0.5f : (float)v / (arm.raysVertical - 1));

            for (int h = 0; h < arm.raysHorizontal; h++)
            {
                float hAngle = Mathf.Lerp(-arm.horizontalAngle, arm.horizontalAngle,
                    arm.raysHorizontal == 1 ? 0.5f : (float)h / (arm.raysHorizontal - 1));

                Vector3 dir = Quaternion.Euler(vAngle, hAngle, 0) * arm.rayOrigin.forward;

                if (Physics.Raycast(arm.rayOrigin.position, dir, out RaycastHit hit, arm.rayDistance, arm.hitLayers))
                {
                    Debug.DrawLine(arm.rayOrigin.position, hit.point, Color.green);
                    if (hit.distance < closestDist)
                    {
                        closestDist = hit.distance;
                        bestHit = hit.point;
                    }
                }
                else
                {
                    Debug.DrawRay(arm.rayOrigin.position, dir * arm.rayDistance, Color.red);
                }
            }
        }

        if (closestDist < Mathf.Infinity)
        {
            arm.lockedPoint = bestHit;
        }
    }
}
