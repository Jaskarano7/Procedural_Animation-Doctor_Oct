using UnityEngine;

public class MultiArmSphereCastLock : MonoBehaviour
{
    [System.Serializable]
    public class ArmSetup
    {
        public Transform rayOrigin;        // Where the spherecast starts
        public GameObject armObject;       // The arm to move
        public float sphereRadius = 0.5f;  // Thickness of the cast
        public float rayDistance = 10f;    // How far forward to check
        public float moveSpeed = 5f;       // Movement speed toward target
        public LayerMask hitLayers;        // Layers to detect

        [HideInInspector] public Vector3? lockedPoint;
    }

    public ArmSetup[] arms;
    public bool showDebug = true;

    private void FixedUpdate()
    {
        foreach (var arm in arms)
        {
            ValidateLock(arm);

            if (!arm.lockedPoint.HasValue)
                ScanWithSphereCast(arm);

            MoveArm(arm);
        }
    }

    private void ValidateLock(ArmSetup arm)
    {
        if (!arm.lockedPoint.HasValue) return;

        // Check if target is still in line of sight
        Vector3 dir = (arm.lockedPoint.Value - arm.rayOrigin.position).normalized;

        if (!Physics.SphereCast(arm.rayOrigin.position, arm.sphereRadius, dir, out RaycastHit hit, arm.rayDistance, arm.hitLayers) ||
            (hit.point - arm.lockedPoint.Value).sqrMagnitude > 0.04f) // tolerance
        {
            arm.lockedPoint = null;
        }
    }

    private void ScanWithSphereCast(ArmSetup arm)
    {
        Vector3 dir = arm.rayOrigin.forward;

        if (Physics.SphereCast(arm.rayOrigin.position, arm.sphereRadius, dir, out RaycastHit hit, arm.rayDistance, arm.hitLayers))
        {
            arm.lockedPoint = hit.point;

            if (showDebug)
                Debug.DrawLine(arm.rayOrigin.position, hit.point, Color.green);
        }
        else
        {
            if (showDebug)
                Debug.DrawRay(arm.rayOrigin.position, dir * arm.rayDistance, Color.red);
        }
    }

    private void MoveArm(ArmSetup arm)
    {
        if (!arm.lockedPoint.HasValue) return;

        arm.armObject.transform.position = Vector3.MoveTowards(
            arm.armObject.transform.position,
            arm.lockedPoint.Value,
            arm.moveSpeed * Time.fixedDeltaTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebug || arms == null) return;

        foreach (var arm in arms)
        {
            if (arm.rayOrigin != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(arm.rayOrigin.position, arm.sphereRadius);
            }
        }
    }
}
