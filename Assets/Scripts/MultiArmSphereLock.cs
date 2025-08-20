using UnityEngine;
using System.Collections.Generic;

public class MultiArmSphereLock : MonoBehaviour
{
    [System.Serializable]
    public class ArmSetup
    {
        public Transform rayOrigin;
        public GameObject armObject;
        public Transform tip;              // Tip of the arm for precise grabbing
        public float detectRadius = 2f;
        public float rayDistance = 10f;
        public float moveSpeed = 5f;
        public LayerMask hitLayers;
        public LayerMask grabLayers;       // Layers that make the arm busy

        public Vector3? lockedPoint;       // Target point for movement
        public bool isBusy;                // Is the arm grabbing
        public Transform lockedTarget;     // Transform of the locked target
        public GameObject CurrentGrabbedGameObject; // Object being grabbed
    }

    public ArmSetup[] arms;
    public bool showDebug = true;

    // Keep track of all currently grabbed objects
    private HashSet<GameObject> claimedObjects = new HashSet<GameObject>();

    private void FixedUpdate()
    {
        claimedObjects.Clear();

        // Step 0: Check busy state for each arm
        foreach (var arm in arms)
        {
            CheckBusy(arm);

            if (arm.isBusy && arm.CurrentGrabbedGameObject != null)
            {
                // Claim the object so no other arm can grab it
                claimedObjects.Add(arm.CurrentGrabbedGameObject);

                // Do not move if busy
                arm.lockedPoint = null;
                arm.lockedTarget = null;
            }
        }

        // Step 1: Gather all potential targets for non-busy arms
        Dictionary<Transform, ArmSetup> targetToClosestArm = new Dictionary<Transform, ArmSetup>();

        foreach (var arm in arms)
        {
            if (arm.isBusy) continue;

            Collider[] hits = Physics.OverlapSphere(arm.rayOrigin.position, arm.detectRadius, arm.hitLayers);
            foreach (var col in hits)
            {
                // Skip already claimed objects
                if (claimedObjects.Contains(col.gameObject))
                    continue;

                float dist = Vector3.Distance(arm.rayOrigin.position, col.transform.position);

                // Assign closest arm to target
                if (targetToClosestArm.TryGetValue(col.transform, out ArmSetup existingArm))
                {
                    float existingDist = Vector3.Distance(existingArm.rayOrigin.position, col.transform.position);
                    if (dist < existingDist)
                        targetToClosestArm[col.transform] = arm; // closer arm takes it
                }
                else
                {
                    targetToClosestArm[col.transform] = arm; // first arm sees it
                }
            }
        }

        // Step 2: Assign locked points only to closest arms
        foreach (var kvp in targetToClosestArm)
        {
            Transform target = kvp.Key;
            ArmSetup arm = kvp.Value;

            Vector3 dir = (target.position - arm.rayOrigin.position).normalized;
            if (Physics.Raycast(arm.rayOrigin.position, dir, out RaycastHit hit, arm.rayDistance, arm.hitLayers))
            {
                arm.lockedPoint = hit.point;
                arm.lockedTarget = target;
            }
        }

        // Step 3: Move only the arms that have locked points
        foreach (var arm in arms)
        {
            if (arm.lockedPoint.HasValue && !arm.isBusy)
            {
                MoveArm(arm, arm.lockedPoint.Value);
            }
        }
    }

    private void CheckBusy(ArmSetup arm)
    {
        if (arm.tip == null)
        {
            Debug.LogWarning("Arm tip not assigned for " + arm.armObject.name);
            return;
        }

        // Detect if arm tip is touching/grabbing an object
        Collider[] hits = Physics.OverlapSphere(arm.tip.position, 0.1f, arm.grabLayers);
        arm.isBusy = hits.Length > 0;

        if (arm.isBusy)
        {
            // Store grabbed object
            arm.CurrentGrabbedGameObject = hits[0].gameObject;
        }
        else
        {
            // Clear if not grabbing
            arm.CurrentGrabbedGameObject = null;
        }

        if (showDebug)
            Debug.DrawRay(arm.tip.position, Vector3.forward * 0.2f, arm.isBusy ? Color.red : Color.green);
    }

    private void MoveArm(ArmSetup arm, Vector3 target)
    {
        arm.armObject.transform.position = Vector3.MoveTowards(
            arm.armObject.transform.position,
            target,
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
                Gizmos.color = arm.isBusy ? Color.red : Color.yellow;
                Gizmos.DrawWireSphere(arm.rayOrigin.position, arm.detectRadius);

                if (arm.tip != null)
                {
                    Gizmos.color = arm.isBusy ? Color.red : Color.green;
                    Gizmos.DrawWireSphere(arm.tip.position, 0.1f);
                }
            }
        }
    }
}
