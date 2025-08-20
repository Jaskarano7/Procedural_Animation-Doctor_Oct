using UnityEngine;

public class MultiArmRaycast : MonoBehaviour
{
    [System.Serializable]
    public class ArmSetup
    {
        public Transform rayOriginA; // Approach ray origin
        public Transform rayOriginB; // Stop ray origin
        public GameObject armObject; // Arm to move
        public float approachDistance = 10f;
        public float stopDistance = 2f;
        public float moveSpeed = 2f;

        public int raysPerArm = 5;      // Number of rays
        public float spreadAngle = 30f; // Degrees spread

        [HideInInspector] public bool isMoving;
    }

    public ArmSetup[] arms; // 4 arms with 2 ray origins each

    private void FixedUpdate()
    {
        foreach (var arm in arms)
        {
            HandleArmRaycast(arm);
        }
    }

    private void HandleArmRaycast(ArmSetup arm)
    {
        Vector3 bestHitPoint = Vector3.zero;
        float bestDistance = Mathf.Infinity;
        bool foundHit = false;

        for (int r = 0; r < arm.raysPerArm; r++)
        {
            // Horizontal spread
            float angleH = Mathf.Lerp(-arm.spreadAngle, arm.spreadAngle, (float)r / (arm.raysPerArm - 1));
            Vector3 dir = Quaternion.Euler(0, angleH, 0) * arm.rayOriginA.forward;

            if (Physics.Raycast(arm.rayOriginA.position, dir, out RaycastHit hit, arm.approachDistance))
            {
                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestHitPoint = hit.point;
                    foundHit = true;
                }
                Debug.DrawLine(arm.rayOriginA.position, hit.point, Color.green);
            }
            else
            {
                Debug.DrawRay(arm.rayOriginA.position, dir * arm.approachDistance, Color.red);
            }
        }

        if (foundHit)
        {
            arm.isMoving = true;
            arm.armObject.transform.position = Vector3.MoveTowards(
                arm.armObject.transform.position,
                bestHitPoint,
                arm.moveSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            arm.isMoving = false;
        }
    }
}
